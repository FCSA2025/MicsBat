using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// PCNscan.exe is called by WebMICS when the user right-clicks on a 
/// (File -> Open ->) TS or ES PDF file  and selects the menu item 'PCN'. 
/// PCNscan.exe is invoked by dblogger and will search the database 
/// looking for sites within a given distance of a file, and return 
/// (via the returnvalues table) all the operators of all sites within 
/// the prescribed radius and in adjacent bands.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - PCNscan.PNG" ""
/// </remarks>
namespace PCNscan
{
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;            //Invented to mimic (char *) for [In]  only.
    using SQLCHARPTRINOUT = IntPtr;       //Invented to mimic (char *) for [In, Out].
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLINTEGERPTR = IntPtr;
    using SQLLEN = Int64;
    using SQLLENPTR = IntPtr;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;
    using SQLSETPOSIROW = UInt64;
    using SQLSMALLINT = Int16;
    using SQLSMALLINTPTR = IntPtr;
    using SQLULEN = UInt64;
    using SQLUSMALLINT = UInt16;
    using SQLUINTEGER = UInt32;
    using _Auxlib;
    using System.IO;

    /// <summary>
    /// This class provides the Main method for the MICS program PCNscan.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class PCNscan
    {
        private static int mTabType = -1;
        private static string mKey = "";
        private static float mCoordDist = 0.0f;
        private static TextWriter mTWlogFile = null;
        private static bool mToConsole = false;

        /// <summary>
        /// This is the method for the MICS program PCNscan.
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            int nRet;

            Console.Write("\r\nPCNscan build {0}\r\n", Info.BuildMetaData);

            try
            {
                // Enable or disable developmental run-time logging.
#if false          
                string mLog2FilePath = @"d:\MicsBatchLogs\PCNscan.log";
                if (Log2.SetLogFilePath(mLog2FilePath))
                {
                    Log2.Erase();
                    Log2.Set(Log2.FileOpenClose.PER_SESSION);
                    Log2.Set(Log2.WriteMode.ENABLED);
                    Log2.Set(Log2.Level.VERBOSE);
                    Log2.v("\nBuild: " + Info.BuildMetaData);
                }
                else
                {
                    Console.Error.Write("\r\nERROR: could not open Log2 file: " + mLog2FilePath);
                }
#endif
                int rc = 0;

                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                //...Log2.v("\nPCNscan.Main(): A");

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                //
                // Also, this method *must* be called before Qutils.EnterQueue()
                // because it needs Info.MicsUserName to be set.
                GetEnvVariablesForUtConnect();

                //...Log2.v("\nPCNscan.Main(): B");

                // Queue for READ access to the database.
                if ((nRet = Qutils.EnterQueue(Info.DbName, "READ", 30)) != 0)
                {
                    //Explain any error message.
                    Qutils.ExplainQueue(Info.DbName, "READ", nRet, null);
                    Application.ExitQuietly(100);
                }

                //...Log2.v("\nPCNscan.Main(): C");

                // Establish an FCSA user session with the database.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    Console.Write("\r\nCannot connect to database: {0}, reason {1}\r\n{2}\r\n",
                                    Info.DbName, rc, GenUtil.GetUserMess());
                    Qutils.ExitQueue(Info.DbName, "READ");
                    Application.ExitQuietly(125);
                }

                //...Log2.v("\nPCNscan.Main(): D");

                // Construct the string to be placed in the 'mics function' column in
                // the billing table.
                string infoBuffer = "";
                switch (mTabType)
                {
                    case Constant.FT:
                        infoBuffer = "PCNSCAN FT";
                        break;
                    case Constant.FE:
                        infoBuffer = "PCNSCAN FE";
                        break;
                    default:
                        infoBuffer = "mTabType?!";
                        break;
                }

                // Initialize the billing.
                BiUtil.BiBillingRec(infoBuffer, Info.PdfName);

                // Check that the prescribed PDF table set actually exists in the DB.
                if (!Ssutil.UtTableExist(mTabType, Info.PdfName))
                {
                    Log2.e("\n\nPCNscan.Main(): call to Ssutil.UtTableExist() failed for mTabType = {0}, Info.PdfName = {1}", mTabType, Info.PdfName);
                    Console.Write("\r\nERROR: Table '{0}' does not exist.\r\n", Info.PdfName);
                    Qutils.ExitQueue(Info.DbName, "READ");
                    Application.ExitQuietly(123);
                }

                //...Log2.v("\nPCNscan.Main(): E");

                // Check that the prescribed PDF table set has been successfully validated.

                string validatedFor;
                if (FilewUtil.UtFilewValidated(mTabType, Info.PdfName, out validatedFor) != Constant.SUCCESS)
                {
                    Console.Write("\r\nERROR: table is not validated.\r\n");
                    Qutils.ExitQueue(Info.DbName, "READ");
                    Application.ExitQuietly(122);
                }

                //...Log2.v("\nPCNscan.Main(): F");

                if (validatedFor[0] == Constant.UPDATE_POSTED)
                {
                    Console.Write("\r\nERROR: {0} has already been posted.\r\n", Info.PdfName);
                    Qutils.ExitQueue(Info.DbName, "READ");
                    Application.ExitQuietly(121);
                }

                //...Log2.v("\nPCNscan.Main(): G");

                if ((validatedFor[0] != Constant.TSIP_VALIDATED) &&
                    (validatedFor[0] != Constant.SLA_VALIDATED) &&
                    (validatedFor[0] != Constant.UPDATE_VALIDATED) &&
                    (validatedFor[0] != Constant.M_UPDATE_VALIDATED) &&
                    (validatedFor[0] != Constant.M_TSIP_VALIDATED) &&
                    (validatedFor[0] != Constant.M_SLA_VALIDATED))
                {
                    Console.Write("\r\nERROR: file has not been validated correctly.\r\n");
                    Qutils.ExitQueue(Info.DbName, "READ");
                    Application.ExitQuietly(120);
                }

                // We will need to know 'this' user's Ultrix ID so that we can exclude it
                // from the list of PCN-affected operators. The following method call
                // assumes that Info.MicsUserName has already been set and uses it to
                // lookup the corresponding Ultrix ID from the DB table adm.account_details.
                // The result is also written to Info.UltrixID
                string dummy;
                Suutils.GetUltrixID(out dummy);

                //...Log2.v("\nInfo object :" + Info.ToString());

                // The following method call provides almost all of PCNscan's functionality.
                rc = PcnOperatorSelect();

                //...Log2.v("\nPCNscan.Main(): H");

                // If log file was created then close it.
                if (mTWlogFile != null) mTWlogFile.Close();

                // Stop holding READ access on the database.
                Qutils.ExitQueue(Info.DbName, "READ");

                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                //...Log2.v("\nPCNscan.Main(): I");

                Application.ExitQuietly(0);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nPCNscan.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nPCNscan.Main(): stack trace: \r\n\r\n" + e.StackTrace);
                BiUtil.BiBillingRec(Constant.BI_END, "");
                Ssutil.UtDisconnect(1);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

        }

        /// <summary>
        /// This methods gets the values of Windows environment variables that
        /// are required to successfully execute Qutils.EnterQueue() and/or
        /// Ssutil.UtConnect(); specifically these are 'MICSUSER'and 'PASSWORD'.
        /// </summary>
        public static void GetEnvVariablesForUtConnect()
        {
            // Get the user's MICS ID from the environment.
            Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                Log2.e("\r\nFeImport.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
                Application.ExitQuietly(Error.ENVVARMICSUSERNOTSET);
            }

            // Get the user's password from the environment.
            Info.Password = Environment.GetEnvironmentVariable("PASSWORD");         // REQUIRED (but can be anything).
            if (String.IsNullOrWhiteSpace(Info.Password))
            {
                // Password just has to be set to something; its value is never used.
                Info.Password = "Bananarama";
            }
        }


        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n PCNscan: This program searches the MDB to find all TS and ES sites located within 'keyhole'     +");
            Console.Write("\r\n =======  coordination contours centered on each of the sites identified in the prescribed PDF   +");
            Console.Write("\r\n          and operating in adjacent frequency bands; it then  collates a list of all the         +");
            Console.Write("\r\n          operators of these PCN-affected sites and writes this result to a 'returnvalues'       +");
            Console.Write("\r\n          table in the user's DB schema.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: PCN <dbName> <projectCode> <pdfName> <stationType> <coordDist> <retkey> [logFile] [-w]");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        projectCode      : user's charge code.");
            Console.Write("\r\n        pdfName          : PDF root name, i.e. the XXX in table ft_XXX_site");
            Console.Write("\r\n        stationType      : type of sites in the PDF: either 'TS' or 'ES'.");
            Console.Write("\r\n        coordDist        : coordination distance in kms; must be in range 1.0 to 1000.0");
            Console.Write("\r\n                         : otherwise it will be defaulted to 200.0 kms.");
            Console.Write("\r\n        retkey           : unique identifier string used to tag the results in ");
            Console.Write("\r\n                           the user's 'returnvalues' table (max. 20 characters).");
            Console.Write("\r\n        logFile          : full path to an optional logging file;");
            Console.Write("\r\n                         : the parent directory must already exist.");
            Console.Write("\r\n                         : and the user must have the required privileges.");
            Console.Write("\r\n        -w               : also write the resulting list of operators to the console.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     PCNscan  fcsa hulme1_0 PCNscanTestTS TS 1000 ABC D:\\extractlogs\\PCNscanLog.txt");
            Console.Write("\r\n     ");
            Console.Write("\r\n     PCNscan  fcsa hulme1_0 PCNscanTestES ES 650 sd34w ");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. PCN is the abbreviation for 'Prior Coordination Notice'. ");
            Console.Write("\r\n       2. Running PCNscan is the first step in enabling a FCSA member to send      +");
            Console.Write("\r\n          advisory messages to those FCSA members that operate PCN-affected sites.");
            Console.Write("\r\n");
        }


        /// <summary>
        /// This method parses the user-prescribed command-line arguments.
        /// </summary>
        /// <param name="args"></param>
        public static void ParseCommandLineArgs(string[] args)
        {
            // Create separate lists of 'flag' args prefixed with '-' and those that
            // are not.
            List<string> flagArgs = new List<string>();
            List<string> regularArgs = new List<string>();

            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    flagArgs.Add(arg);
                }
                else
                {
                    regularArgs.Add(arg);
                }
            }

            // Parse the optional flags (prefixed with '-').
            foreach (string arg in flagArgs)
            {
                // Check that we don't just have a minus character.
                if (arg.Length == 1)
                {
                    Console.Write("\n\r Invalid flag: '{0}'", arg);
                    WriteUsageToConsole();
                    Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                }

                // Process the flags.
                string flag = Strings.DropFirstChar(arg).ToUpper();
                switch (flag)
                {
                    case "W":
                        mToConsole = true;
                        break;
                    default:
                        Console.Write("\n\r Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                        break;
                }

            }

            // Parse the regular arguments; there must be qty. 6 or 7 of them.
            if ((regularArgs.Count != 6) && (regularArgs.Count != 7))
            {
                Console.Write("\n\r Invalid number of arguments: must be 6 or 7.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(127);
            }

            // Parse the args.

            // 1st command-line argument: database name.
            Info.DbName = regularArgs[0];

            // 2nd command-line argument: project code.
            Info.ProjectCode = regularArgs[1];

            // 3rd command-line argument: file name.
            Info.PdfName = regularArgs[2];

            // 4th command-line argument: station type 'TS" or 'ES'.
            switch (regularArgs[3].ToUpper())
            {
                case "TS":
                    mTabType = Constant.FT;
                    break;
                case "ES":
                    mTabType = Constant.FE;
                    break;
                default:
                    Console.Write("\r\n\r\n ERROR: 4th command-line argument must be 'TS' of 'ES'.");
                    WriteUsageToConsole();
                    Application.ExitQuietly(127);
                    break;
            }

            // 5th command-line argument: distance (kms).
            try
            {
                mCoordDist = Convert.ToSingle(regularArgs[4]);

                if ((mCoordDist < 1.0) || (mCoordDist > 1000.0))
                {
                    Console.Write("\r\n ERROR: Invalid distance input ({0:F1}km).  Coordination Dist set to 200km\r\n",
                                    mCoordDist);
                    mCoordDist = 200.0f;
                }

            }
            catch
            {
                Console.Write("\r\n\r\n ERROR: 5th command-line argument must be a valid floating-point number +");
                Console.Write("\r\n        between 1.0 and 1000.0 (kilometers).");
                WriteUsageToConsole();
                Application.ExitQuietly(127);
            }

            // 6th command-line argument: key.
            mKey = regularArgs[5];
            if (mKey.Length > 20)
            {
                Console.Write("\r\n\r\n ERROR: key [{0}] exceeds the maximum 20 characters.\r\n", mKey);
                WriteUsageToConsole();
                Application.ExitQuietly(126);
            }

            // 7th command-line argument: log file path (optional).
            if (regularArgs.Count == 7)
            {
                string filePath = regularArgs[6];

                try
                {
                    mTWlogFile = new StreamWriter(filePath, false);
                }
                catch (Exception e)
                {
                    Log2.e("\n\nPCNscan.ParseCommandLineArgs(): ERROR: could not open log file: filePath");
                    Log2.e("\nException: " + e.Message);
                    Console.Write("\n\nERROR: " + e.Message + "\n");
                }
            }

            //...Log2.v("\nmTabType    : " + mTabType);
            //...Log2.v("\nmCoordDist  : " + mCoordDist);
            //...Log2.v("\nmKey        : " + mKey);
            //...Log2.v("\nmTextWriter : " + (mTWlogFile == null ? "null" : mTWlogFile.ToString()));
            //...Log2.v("\nmToConsole  : " + mToConsole);
        }

        /// <summary>
        /// This method inserts a list of operators affected by the sites identified 
        /// in the user-prescribed PDF input table into the user's SQL Server 
        /// 'returnvalues' table.
        /// </summary>
        /// <returns></returns>
        private static int PcnOperatorSelect()
        {
            int rc = 0;

            //string[] operatorList = Arrays.CreateFillStringArray(Constant.MAX_USERID_LIST, "");
            //string[] selected = Arrays.CreateFillStringArray(Constant.MAX_USERID_LIST, "");
            List<string> operatorListX = new List<string>();

            /* Cull operator codes from mdb */
            if ((rc = CreateOperatorList(mTabType, Info.PdfName, mCoordDist, out operatorListX)) != Constant.SUCCESS)
            {
                Log2.e("\nPCNscan.PcnOperatorSelect(): ERROR: call to CreateOperatorList() failed, rc = {0}", rc);
                Console.Write("\r\npcnscan: Could not create Operator list for reason {0}.\r\n", rc);
                return (rc);
            }

            //...Log2.v("\nPCNscan.PcnOperatorSelect(): operatorListX.Count = {0}", operatorListX.Count);

            /* Load operator codes into table */
            OperatorLoadTable(null, operatorListX, "", mKey);

            return (rc);
        }

        /// <summary>
        /// This method creates a list of operators affected by the sites identified 
        /// in the user-prescribed PDF who need to be sent a PCN.
        /// </summary>
        /// <param name="tabType"></param>
        /// <param name="pdfTableName"></param>
        /// <param name="coordDist"></param>
        /// <param name="operatorList"></param>
        /// <returns></returns>
        private static int CreateOperatorList(int tabType,
                                                string pdfTableName,
                                                float coordDist,
                                                out List<string> operatorList)
        {
            // 'out' requirement.
            operatorList = new List<string>();

            int rc;

            CreateTempTables();

            if (tabType == Constant.FT)
            { /* TS-TS and TS-ES */

                /* TS-TS */
                /* get all the affected operators from the TS MDB table */
                if ((rc = GetTsTsOperators(pdfTableName, coordDist, ref operatorList)) != Constant.SUCCESS)
                {
                    Log2.e("\nPCNscan.CreateOperatorList(): ERROR: call to GetTsTsOperators() failed, rc = {0}", rc);
                    return (rc);
                }

                /* TS-ES */
                /* get all the affected operators from the ES MDB table */
                if ((rc = GetTsEsOperators(pdfTableName, coordDist, ref operatorList)) != Constant.SUCCESS)
                {
                    Log2.e("\nPCNscan.CreateOperatorList(): ERROR: call to GetTsEsOperators() failed, rc = {0}", rc);
                    return (rc);
                }

            }
            else
            {
                /* ES-TS only */
                /* get all the affected operators from the TS MDB table */
                if ((rc = GetEsTsOperators(pdfTableName, coordDist, ref operatorList)) != Constant.SUCCESS)
                {
                    Log2.e("\nPCNscan.CreateOperatorList(): ERROR: call to GetEsTsOperators() failed, rc = {0}", rc);
                    return (rc);
                }
            }

            //removeTempTables();

            return (Constant.SUCCESS);
        }

        /// <summary>
        ///  This method returns a list of TS operators that fall within the
        ///  'keyhole' coordination contour centered on each site identified 
        ///  in a prescribed TS PDF (scaled on the coordinating distance) and 
        ///  that also have adjacent band codes.
        /// </summary>
        /// <param name="pdfTableName"></param>
        /// <param name="coordDist"></param>
        /// <param name="operatorList"></param>
        /// <returns></returns>
        private static int GetTsTsOperators(string pdfTableName,
                                            float coordDist,    /* coordinating distance */
                                            ref List<string> operatorList)

        {
            string ftSiteTableName;

            string operCode;
            string ultrixId;
            char status;
            int rc;
            int opercheck;

            SQLLEN isNull;

            // ROUGH CULL - select all sites falling within a 'keyhole' centered
            // on each site. 
            if ((rc = TsTsRoughCull(pdfTableName, coordDist)) != Constant.SUCCESS)
            {
                Log2.e("\nPCNscan.GetTsTsOperators(): ERROR: call to TsTsRoughCull() failed, rc = {0}", rc);
                return (rc);
            }
            GenUtil.UtCvtName(Constant.FT_SITE, pdfTableName, out ftSiteTableName);

            /* for each site in the culling range */
            SQLRETURN sqlRet;
            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string query = String.Format("SELECT DISTINCT oper FROM {0}.tt_terre_tmp1 ", Info.GlobalSchema);
            sqlRet = ODBC.SQLExecDirect(hStmt, query, query.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nPCNscan.GetTsTsOperators(): ERROR: call to SQLExecDirect() failed, query = {0}", query);
            }

            while (true)
            {
                sqlRet = ODBC.SQLFetch(hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    break;
                }

                rc = Ssutil.DbGetString(hStmt, 1, "oper", out operCode, MtSite.OPER_SZ, out isNull);

                /* get userid given operCode */
                opercheck = BillingUtil.UtGetUserId(operCode, out ultrixId, out status);

                if (opercheck == Constant.SUCCESS)
                {
                    /* 	check to see if it is not already in the list and that
                    *		this is not the present user.                        */
                    if (!BillingUtil.UtUserIdInList(operatorList, ultrixId) &&
                            !(Info.UltrixID.Equals(ultrixId)))
                    {

                        /* if the site is in range and it uses bands adjacent to those
                            in the pdf, add the ultrix ID for the operator to the operator
                            list.   */
                        operatorList.Add(ultrixId);

                    }
                }
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            //...Log2.v("\nPCNscan.GetTsTsOperators(): operatorList.Count = {0}", operatorList.Count);

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method parses all the site-antennas-channels specified in a prescribed 
        /// TS PDF table set to identify the channel bands (bndcde) used by each site;
        /// for each band its 'adjacent' bands are retrieved from the 'badj' column of 
        /// the MDB table main.sd_band and this adjacency is  collated
        /// information in the form of 'bit array'. 
        /// </summary>
        /// <param name="pdfName"></param>
        /// <param name="adjacentBands"></param>
        /// <returns></returns>
        private static int GetTsAdjBands(string pdfName, int[] adjacentBands)
        {
            /* Local variables */
            SQLLEN[] nArrayFW;  // [FtAnte.SIZE_];
            string tableName;
            int aID1;
            int rc;
            FtAnte ftAntenna;

            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);

            /* read antenna information */
            if ((aID1 = DynAntenna.FtSelectAntenna(tableName, "", "")) < 0)
            {
                Console.Write("\t{0}\tWARNING - Could not read antenna information\n", "W");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                return (Error.DYN_MS_SQL_SERVER_ERR);
            }

            rc = Error.NOBANDCHANGE;

            while (DynAntenna.FtFetchAntenna(aID1, out ftAntenna, out nArrayFW) == Constant.SUCCESS)
            {
                rc = InsertAdjBands(ftAntenna.bndcde, adjacentBands);
            }

            DynAntenna.FtCloseAntenna(aID1);
            return rc;
        }

        /// <summary>
        /// This method sets the bits in a 'bit array' that correspond to bands that
        /// are adjacent to the band prescribed by bndcde.
        /// </summary>
        /// <param name="bandCode"></param>
        /// <param name="adjacentBands"></param>
        /// <returns></returns>
        private static int InsertAdjBands(string bandCode, int[] adjacentBands)
        {
            string adjBandString;
            SdBand tBand;
            string[] tokens;
            int bcode = 0;
            int nRet;

            nRet = Suutils.SdGetBand(bandCode, out tBand);
            if (nRet == 0)
            {
                adjBandString = tBand.badj;

                char[] delimiters = new char[3] { ' ', ',', ';' };

                tokens = adjBandString.Split(delimiters);

                foreach (string token in tokens)
                {
                    bcode = GetBitPosForBand(token);
                    if (bcode > 0)
                    {
                        SetBit(adjacentBands, bcode);
                    }
                }
            }

            if (bcode == 0)
            {
                /* bandCode is not an integer, so no band related information is changed,
                     and no PCN coordination is required. */
                return (Error.NOBANDCHANGE);
            }
            else
            {
                return Constant.SUCCESS;
            }
        }

        /// <summary>
        /// This method sets the bit at a prescribed position in a 'bit array'.
        /// </summary>
        /// <param name="bitline"></param>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        private static int SetBit(int[] bitline, int bitPosition)
        {
            int index;

            /* return FAILURE if the number is too large */
            if (bitPosition > Constant.MAXBITS || bitPosition < 0)
            {
                return Constant.FAILURE;
            }
            index = 0;

            /* step through the array of integers till we get to the right one */
            while (bitPosition > Constant.INTLENGTH)
            {
                index++;
                bitPosition = bitPosition - Constant.INTLENGTH;
            }
            bitline[index] = bitline[index] | (1 << bitPosition);

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method searches the MDB and fetches all the TS sites inside the 
        /// 'keyhole' centred on each of the TS sites identified in the prescribed PDF
        ///  'ft_XXX_site' table and out to a prescribed coordination distance; the results
        ///  of this method are stored in a temporary table named 'tt_terre_tmp1' in
        ///  the user's schema.
        /// </summary>
        /// <param name="ftTableName"></param>
        /// <param name="coordDist"></param>
        /// <returns></returns>
        private static int TsTsRoughCull(string ftTableName, float coordDist)
        {
            //...Log2.v("\nPCNscan.TsTsRoughCull(): A");

            string sqlCommand;
            int rc;
            FtSiteStr ftSite = null;
            string cCall;
            string cLastCall;
            BandBits tBands;
            string cSearch = "";

            cLastCall = "";
            while ((rc = FtUtils.FtNextCall(out cCall, cLastCall, ftTableName)) == 0)
            {
                //...Log2.v("\nPCNscan.TsTsRoughCull(): B");

                if ((rc = FtUtils.FtGetSite(cCall, out ftSite, 2, ftTableName)) != 0)
                {
                    //	error
                    Log2.e("\nPCNscan.TsTsRoughCull(): ERROR: call to FtGetSite() failed, rc = {0}", rc);
                    Console.Write("\r\nERROR: - tsRoughCull - Could not get site '{0}', reason {1}\n",
                                        cCall, rc);
                }
                else
                {
                    //	Pull in the operator list from the mdb.  
                    //	First get the adjacent bands for this site.

                    //...Log2.v("\n\nPCNscan.TsTsRoughCull(): ===== Fetched TS site: " + ftSite.stSite.KeysToString() + "\n");

                    uint[] bandwordArray = ftSite.stSite.GetBandWdArray();

                    FtUtils.FtSiteAdjBands(bandwordArray, out tBands);      //	Get the adjacent bands in tBands

                    FtUtils.FtSQLSearchString(tBands, out cSearch);   //	Convert them to a SQL search string

                    for (int nInd = 0; nInd < ftSite.nNumAnts; nInd++)
                    {
                        sqlCommand = String.Format("INSERT INTO {0}.tt_terre_tmp1 SELECT s.call1, s.latit, s.longit, s.oper   FROM main.mt_site s  WHERE s.oprtyp='FT' AND ({1}) AND  tsip.keyhole_hs({2}, {3}, s.latit, s.longit, {4}, {5}) <= 2 ",
                                                        Info.GlobalSchema, cSearch,
                                                        ftSite.stSite.latit, ftSite.stSite.longit,
                                                        ftSite.stAntsPtr[nInd].azmth, coordDist);

                        //...Log2.v("\n\nPCNscan.TsTsRoughCull(): sqlCommand = {0}", sqlCommand);

                        if (!FtUtils.DbExecute(sqlCommand))
                        {
                            Console.Write("\nTSTS Call {0} Execute problem...\n{1}\n", cCall, GenUtil.GetUserMess());
                        }

                        string str = String.Format("\r\n\r\nTSTS Call {0}: Insert code is:-\r\n{1}\r\nusermess:'{2}'\r\n",
                                                        cCall, sqlCommand, GenUtil.GetUserMess());

                        // If log file is required...
                        if (mTWlogFile != null) WriteToLogFile(str);

                    }
                }

                cLastCall = cCall;
            }

            if ((rc != Error.DYN_MS_SQL_SERVER_ERR) && (rc != Constant.NOMORERECS))
            {
                return (rc);
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        ///  This method returns a list of ES operators that fall within the
        ///  'keyhole' coordination contour centered on each site identified 
        ///  in a prescribed TS PDF (scaled on the coordinating distance) and 
        ///  that also have adjacent band codes.
        /// </summary>
        /// <param name="pdfTableName"></param>
        /// <param name="coordDist"></param>
        /// <param name="operatorList"></param>
        /// <returns></returns>
        private static int GetTsEsOperators(string pdfTableName, float coordDist, ref List<string> operatorList)
        {
            string operCode;
            string ultrixId;

            char status;
            int rc;
            int opercheck;
            SQLLEN isNull;

            SQLRETURN sqlRet;
            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;

            /* Fill the temp table */
            if ((rc = TsEsRoughCull(pdfTableName)) != Constant.SUCCESS)
            {
                return (rc);
            }

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string cSQL = "SELECT DISTINCT oper FROM te_earth_tmp1 ";
            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            /* for each site in the culling range */
            while (true)
            {
                sqlRet = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlRet))
                {
                    break;
                }

                // (call1 char (9) not null,  latit int null,  longit int null,  oper char (6) null )

                sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, 1, "operCode", out operCode, MtSite.OPER_SZ, out isNull);

                /* get userid given operCode */
                opercheck = BillingUtil.UtGetUserId(operCode, out ultrixId, out status);

                if (opercheck == 0)
                {
                    /* 	check to see if it is not already in the list and that
                    *  	this is not the present user.                        */
                    if (!BillingUtil.UtUserIdInList(operatorList, ultrixId) &&
                        !Info.UltrixID.Equals(ultrixId))
                    {
                        operatorList.Add(ultrixId);
                    }
                }
            }

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            //...Log2.v("\nPCNscan.GetTsEsOperators(): operatorList.Count = {0}", operatorList.Count);
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method creates two temporary DB tables in the user's schema to store the
        /// results from a call to the CreateOperatorList() method: the table 'tt_terre_tmp1'
        /// is used to store the list of PCN-affected operators of TS sites and the table 
        /// 'te_earth_tmp1' is used to store the list of PCN-affected operators of ES sites.
        /// </summary>
        private static void CreateTempTables()
        {
            /* create temporary tables */

            /* if the temporary table exists drop it */
            if (Ssutil.UtTableExist(Constant.TT_TEMP1, Constant.TERRE_TBL))
            {
                Ssutil.UtDropTable(Constant.TT_TEMP1, Constant.TERRE_TBL);
            }
            /* create a one dimensional temporary table */
            Ssutil.UtCreateTable(Constant.TT_TEMP1, Constant.TERRE_TBL);

            /* if the earth temporary table exists drop it */
            if (Ssutil.UtTableExist(Constant.TE_TEMP1, Constant.EARTH_TBL))
            {
                Ssutil.UtDropTable(Constant.TE_TEMP1, Constant.EARTH_TBL);
            }
            /* create a one dimensional earth temporary table */
            Ssutil.UtCreateTable(Constant.TE_TEMP1, Constant.EARTH_TBL);

        }

        /// <summary>
        /// This method searches the MDB and fetches all the ES sites inside the 
        /// 'keyhole' centred on each of the TS sites identified in the prescribed PDF
        ///  'ft_XXX_site' table and out to a prescribed coordination distance; the results
        ///  of this method are stored in a temporary table named 'te_earth_tmp1' in
        ///  the user's schema.
        /// </summary>
        /// <param name="ftTableName"></param>
        /// <returns></returns>
        private static int TsEsRoughCull(string ftTableName)
        {
            //...Log2.v("\nPCNscan.TsEsRoughCull(): ping");

            string sqlCommand;
            int rc;
            FtSiteStr ftSite = null;
            string cCall;
            string cLastCall;
            BandBits tBands;
            string cSearch = "";

            cLastCall = "";
            while ((rc = FtUtils.FtNextCall(out cCall, cLastCall, ftTableName)) == 0)
            {
                if ((rc = FtUtils.FtGetSite(cCall, out ftSite, 2, ftTableName)) != 0)
                {
                    //	error
                    Console.Write("\r\nERROR: - tsEsRoughCull - Could not get site '{0}', reason {1}\n",
                                    cCall, rc);
                }
                else
                {
                    //	Pull in the operator list from the mdb.  
                    //	First get the adjacent bands for this site.

                    //...Log2.v("\n\nPCNscan.TsEsRoughCull(): ===== Fetched TS site: " + ftSite.stSite.KeysToString() + "\n");

                    uint[] bandwordArray = ftSite.stSite.GetBandWdArray();

                    FtUtils.FtSiteAdjBands(bandwordArray, out tBands);      //	Get the adjacent bands in tBands

                    //...Log2.v("\nPCNscan.TsEsRoughCull(): tBands:\n" + tBands.ToString());

                    //	Convert them to a SQL search string
                    Suutils.FtBandBitsToList(tBands, out cSearch);

                    for (int i = 0; i < 8; i++)
                    {
                        //...Log2.v("\nPCNscan.TsEsRoughCull(): tBands:\n" + Strings.ToBinaryWR(tBands.bitArray[i]));
                    }

                    for (int nInd = 0; nInd < ftSite.nNumAnts; nInd++)
                    {
                        //	Pull in all sites in the es mdb that are FT opers, match the band adjacency requirements,
                        //	and who are within the keyhole distance given by the coordinating distance on their 
                        //	es antenna records.
                        sqlCommand = String.Format("INSERT INTO {0}.te_earth_tmp1 SELECT s.location, s.latit, s.longit, s.oper FROM main.me_site s JOIN main.me_ante a ON s.location = a.location  WHERE s.oprtyp='FE' AND (a.txband IN ({1}) OR a.rxband in ({2})) AND  tsip.keyhole_hs({3}, {4}, s.latit, s.longit, {5}, tsip.maxnum(tsip.maxnum(a.txtro, a.txpre), tsip.maxnum(a.rxtro, a.rxpre))) <= 2 ",
                                                    Info.GlobalSchema, cSearch, cSearch,
                                                    ftSite.stSite.latit, ftSite.stSite.longit,
                                                    ftSite.stAntsPtr[nInd].azmth);

                        if (!FtUtils.DbExecute(sqlCommand))
                        {
                            Console.Write("\nTSES Execute problem for {0}\n'{1}'\n", cCall, GenUtil.GetUserMess());
                        }

                        string str = String.Format("\r\n\r\nTSES Call {0}: Insert code is:-\r\n{1}\r\n'{2}'\r\n",
                                                        cCall, sqlCommand, GenUtil.GetUserMess());
                        //...Log2.v(str);

                        // If log file is required...
                        if (mTWlogFile != null) WriteToLogFile(str);
                    }
                }

                cLastCall = cCall;
            }

            if (rc != Constant.NOMORERECS && rc != 0)
            {
                return (rc);
            }

            return (0);
        }

        /// <summary>
        ///  This method returns a list of TS operators that fall within the
        ///  'keyhole' coordination contour centered on each site identified 
        ///  in a prescribed ES PDF (scaled on the coordinating distance) and 
        ///  that also have adjacent band codes.
        /// </summary>
        /// <param name="pdfTableName"></param>
        /// <param name="coordDist"></param>
        /// <param name="operatorList"></param>
        /// <returns></returns>
        private static int GetEsTsOperators(string pdfTableName, float coordDist, ref List<string> operatorList)
        {
            string callOne;

            string feSiteTableName;
            int feSiteHandle;
            FeSite feSiteStruct;
            SQLLEN[] feSiteNulls;

            string feAnteTableName;
            int feAnteHandle;
            FeAnte feAnteStruct;
            SQLLEN[] feAnteNulls; ;

            string selectionCriteria;
            string operCode;
            string ultrixId;
            int latitude;
            int longitude;
            double intLatit;
            double intLongit;
            double vicLatit;
            double vicLongit;
            double distKm;
            double bearingMid;
            double bearingDiff;

            bool inRange;
            char status;
            int rc;
            int nRet;
            int opercheck;
            int[] adjBands;

            /* Create and clear bit array for adjacent bands   */
            adjBands = Arrays.CreateAndFillArray(2, 0);

            /* Store adjacent bands in bit array    */
            if ((rc = GetEsAdjBands(pdfTableName, adjBands)) != Constant.SUCCESS)
            {
                return (rc);
            }

            // Create a temporary file containing sites that fall within the keyhole.

            if ((rc = EsTsRoughCull(pdfTableName)) != Constant.SUCCESS)
            {
                return (rc);
            }

            GenUtil.UtCvtName(Constant.FE_SITE, pdfTableName, out feSiteTableName);
            GenUtil.UtCvtName(Constant.FE_ANTE, pdfTableName, out feAnteTableName);

            /* for each site in the culling range */
            SQLLEN isNull;
            SQLRETURN sqlRet;
            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string cSQL = String.Format("SELECT DISTINCT call1 FROM {0}.tt_terre_tmp1 ORDER BY call1", Info.GlobalSchema);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            while (true)
            {
                sqlRet = ODBC.SQLFetch(hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    break;
                }

                nRet = Ssutil.DbGetString(hStmt, 1, "call1", out callOne, MtSite.CALL1_SZ, out isNull);

                /* get the related TS information for the affected site */
                if ((rc = GetEsTsInfo(callOne, out operCode, out latitude, out longitude)) != Constant.SUCCESS)
                {
                    return (rc);
                }
                /* get userid given operCode */
                opercheck = BillingUtil.UtGetUserId(operCode, out ultrixId, out status);
                if (opercheck == Constant.SUCCESS)
                {
                    /* check to see if it is not already in the list and that
                         this is not the present user.                        */
                    if (!BillingUtil.UtUserIdInList(operatorList, ultrixId) &&
                        (!Info.UltrixID.Equals(ultrixId)))
                    {

                        /* not in list */
                        if ((feSiteHandle = DynFeSite.FeSelectSite(feSiteTableName, "", "")) < 0)
                        {
                            return (feSiteHandle);
                        }

                        inRange = false;

                        while (((rc = DynFeSite.FeFetchSite(feSiteHandle, out feSiteStruct, out feSiteNulls)) == Constant.SUCCESS) &&
                            (!inRange))
                        {

                            /* FINE CULL - determine if the site falls within the coordinating distance */
                            vicLatit = latitude / 100.00;
                            vicLongit = longitude / 100.00;
                            intLatit = feSiteStruct.latit / 100.00;
                            intLongit = feSiteStruct.longit / 100.00;

                            AxSub2.AxDistan(intLatit, vicLatit, intLongit, vicLongit, out distKm, out bearingMid, out bearingDiff);

                            selectionCriteria = String.Format("location = '{0}'", feSiteStruct.location);

                            if ((feAnteHandle = DynFeAnte.FeSelectAnte(feAnteTableName, selectionCriteria, "")) < 0)
                            {
                                return (feAnteHandle);
                            }

                            while (((rc = DynFeAnte.FeFetchAnte(feAnteHandle, out feAnteStruct, out feAnteNulls)) == Constant.SUCCESS) &&
                                (!inRange))
                            {
                                /* determine if this location falls in the range of any of
                                     the interfering sites coordinating distances */
                                if ((distKm <= feAnteStruct.txtro) ||
                                    (distKm <= feAnteStruct.txpre) ||
                                    (distKm <= feAnteStruct.rxtro) ||
                                    (distKm <= feAnteStruct.rxpre))
                                {
                                    inRange = true;
                                }
                            }
                            /* check return code from fetch */
                            if ((rc != Constant.SUCCESS) && ((rc != Error.DYN_MS_SQL_SERVER_ERR) && (rc != Constant.NOMORERECS)))
                            {
                                return (rc);
                            }

                            DynFeAnte.FeCloseAnte(feAnteHandle);
                        }
                        /* check return code from fetch */
                        if ((rc != Constant.SUCCESS) && ((rc != Error.DYN_MS_SQL_SERVER_ERR) && (rc != Constant.NOMORERECS)))
                        {
                            return (rc);
                        }

                        DynFeSite.FeCloseSite(feSiteHandle);

                        if (inRange && OperTsBandsAdjacent(callOne, adjBands))
                        {
                            operatorList.Add(ultrixId);
                        }
                    }
                }
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            //...Log2.v("\nPCNscan.GetEsTsOperators(): operatorList.Count = {0}", operatorList.Count);
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method parses all the site-antennas-channels specified in a prescribed 
        /// ES PDF table set to identify the channel bands (bndcde) used by each site;
        /// for each band its 'adjacent' bands are retrieved from the 'badj' column of 
        /// the MDB table main.sd_band and this adjacency is  collated
        /// information in the form of 'bit array'. 
        /// </summary>
        /// <param name="pdfName"></param>
        /// <param name="adjacentBands"></param>
        /// <returns></returns>
        private static int GetEsAdjBands(string pdfName, int[] adjacentBands)
        {
            /* Local variables */
            SQLLEN[] nArrayFW;  // [FtAnte.SIZE_];
            string tableName;
            int aID1;
            int rc;
            FeAnte feAntenna;

            GenUtil.UtCvtName(Constant.FE_ANTE, pdfName, out tableName);

            /* read antenna information */
            if ((aID1 = DynFeAnte.FeSelectAnte(tableName, "", "")) < 0)
            {
                Console.Write("\t{0}\tWARNING - Could not read antenna information\r\n", "W");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                return (Error.DYN_MS_SQL_SERVER_ERR);
            }

            rc = Error.NOBANDCHANGE;

            while (DynFeAnte.FeFetchAnte(aID1, out feAntenna, out nArrayFW) == Constant.SUCCESS)
            {
                /* if there is a transmit band, include it... */
                if (nArrayFW[FeAnte.TXBAND] != Constant.DB_NULL)
                {
                    rc = InsertAdjBands(feAntenna.txband, adjacentBands);
                }
                /* if there is a receive band, include it... */
                if (nArrayFW[FeAnte.RXBAND] != Constant.DB_NULL)
                {
                    rc = InsertAdjBands(feAntenna.rxband, adjacentBands);
                }
            }

            DynFeAnte.FeCloseAnte(aID1);
            return rc;
        }

        /// <summary>
        /// This method searches the MDB and fetches all the TS sites inside the 
        /// 'keyhole' centred on each of the ES sites identified in the prescribed PDF
        ///  'fe_XXX_site' table and out to a prescribed coordination distance; the results
        ///  of this method are stored in a temporary table named 'tt_terre_tmp1' in
        ///  the user's schema.
        /// </summary>
        /// <param name="earthName"></param>
        /// <returns></returns>
        private static int EsTsRoughCull(string earthName)
        {
            //...Log2.v("\nPCNscan.EsTsRoughCull(): ping");

            string sqlCommand;
            int rc;
            FeSiteStr feSite;
            string cLoc;
            string cLastLoc;
            BandBits tBands = new BandBits();
            BandBits tRxbands = new BandBits();
            string cSearch;

            cLastLoc = "";
            while ((rc = FeUtils.FeNextLoc(out cLoc, cLastLoc, earthName)) == 0)
            {
                if ((rc = FeUtils.FeGetSite(cLoc, out feSite, 2, earthName)) != 0)
                {
                    //	error
                    Console.Write("ERROR: - esTsRoughCull - Could not retrieve site {0}\n{1}\n",
                                    cLoc, GenUtil.GetUserMess());
                    return -1;
                }
                else
                {
                    //...Log2.v("\n\nPCNscan.EsTsRoughCull(): ===== Fetched ES site: " + feSite.stSite.KeysToString() + "\n");

                    //	Go through all the antennas for this site.
                    for (int nInd = 0; nInd < feSite.nNumAnts; nInd++)
                    {
                        tBands.Initialize();
                        tRxbands.Initialize();

                        //	First get the adjacent bands for this antenna
                        Suutils.SuAdjBands(feSite.stAnts[nInd].rxband, out tRxbands);  //	Get the adjacent rxbands 
                        Suutils.SuAdjBands(feSite.stAnts[nInd].txband, out tBands);        //	Get the adjacent txbands


                        tBands.BitWiseOR(tRxbands);

                        //	Combine them.
                        //	Convert them to a SQL search string
                        FtUtils.FtSQLSearchString(tBands, out cSearch);

                        sqlCommand = String.Format("INSERT INTO {0}.tt_terre_tmp1 SELECT s.call1, s.latit, s.longit, s.oper FROM main.mt_site s  WHERE s.oprtyp='FT' and ({1}) and tsip.keyhole_hs({2}, {3}, s.latit, s.longit, {4}, {5}) <= 2 ",
                                                    Info.GlobalSchema, cSearch,
                                                    feSite.stSite.latit, feSite.stSite.longit,
                                                    feSite.stAnts[nInd].az,
                                                    Math.Max(Math.Max(feSite.stAnts[nInd].txpre, feSite.stAnts[nInd].txtro),
                                                             Math.Max(feSite.stAnts[nInd].rxpre, feSite.stAnts[nInd].rxtro)));


                        if (!FtUtils.DbExecute(sqlCommand))
                        {
                            Console.Write("ERROR: - esTsRoughCull - Error executing:\n{0}\n{1}.",
                                           sqlCommand, GenUtil.GetUserMess());
                        }

                        string str = String.Format("\r\n\r\nESTS Location {0}: Insert code is:-\r\n{1}\r\n'{2}'\r\n", cLoc, sqlCommand, GenUtil.GetUserMess());
                        //...Log2.v(str);

                        // If log file is required ...
                        if (mTWlogFile != null) WriteToLogFile(str);
                    }
                }

                cLastLoc = cLoc;
            }

            if (rc != Constant.NOMORERECS && rc != 0)
            {
                return (rc);
            }

            return (0);
        }

        /// <summary>
        /// This method returns true if the TS site prescribed by call1 operates
        /// (Tx or Rx) in any of the adjacent frequency bands specified in the 
        /// prescribed 'bit array'; otherwise false.
        /// </summary>
        /// <param name="call1"></param>
        /// <param name="adjacentBands"></param>
        /// <returns></returns>
        private static bool OperTsBandsAdjacent(string call1, int[] adjacentBands)
        {
            string bandCode;
            bool rc = false;
            string cSQL;
            int nRet;
            SQLRETURN sqlRet;
            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLLEN isNull;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("SELECT DISTINCT bndcde FROM main.mt_ante WHERE call1 = '{0}'", call1);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nPCNscan.OperTsBandsAdjacent(): ERROR: call to ODBC.SQLExecDirect() failed, cSQL " + cSQL);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return false;
            }

            while (true)
            {
                sqlRet = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlRet))
                {
                    break;
                }

                try
                {
                    nRet = Ssutil.DbGetString(hStmt, 1, "bndcde", out bandCode, MtChan.BNDCDE_SZ, out isNull);

                }
                catch (Exception e)
                {
                    Log2.e("\n\nPCNscan.OperTsBandsAdjacent(): ERROR: call to Ssutil.DbGetString() failed, threw exception: " + e.Message);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return false;
                }

                if (CheckBit(adjacentBands, GetBitPosForBand(bandCode)))
                {
                    rc = true;
                    break;
                }
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return rc;
        }

        /// <summary>
        /// This method returns true if the prescribed 'bit array' has a
        /// bit set at the prescribed bit-position.
        /// </summary>
        /// <param name="bitline"></param>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        private static bool CheckBit(int[] bitline, int bitPosition)
        {
            int index;

            if ((bitPosition > Constant.MAXBITS) || (bitPosition < 0))
            {
                return false;
            }

            /* step through the array of integers till we get to the right one */
            index = 0;
            while (bitPosition > Constant.INTLENGTH)
            {
                index++;
                bitPosition = bitPosition - Constant.INTLENGTH;
            }

            if ((bitline[index] & (1 << bitPosition)) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This method returns the bit position that corresponds to a prescribed band code (bndcde).
        /// </summary>
        /// <param name="bndcde"></param>
        /// <returns></returns>
        private static int GetBitPosForBand(string bndcde)
        {
            SdBand ptBand;
            int nRet;
            int nBandPos;

            nRet = Suutils.SdGetBand(bndcde, out ptBand);   /*	0 is good end */

            if (nRet != 0)
            {
                nBandPos = 0;
            }
            else
            {
                nBandPos = ptBand.bandbitpos;
            }
            return nBandPos;
        }

        /// <summary>
        /// This method gets the TS operator code, longitude and latitude of the TS site 
        /// that has a prescribed call sign (call1); this information is fetched from the 
        /// TS MDB table main.mt_site.
        /// </summary>
        /// <param name="call1"></param>
        /// <param name="operCode"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        private static int GetEsTsInfo(string call1, out string operCode, out int latitude, out int longitude)
        {
            // 'out requirement.
            operCode = "";
            latitude = Int32.MinValue;
            longitude = Int32.MinValue;

            int mtSiteHandle;
            MtSite mtSiteStruct;
            SQLLEN[] mtSiteNulls;

            string selectionCriteria;
            int rc;

            selectionCriteria = String.Format("call1 = '{0}'", call1);

            if ((mtSiteHandle = DynMdbSite.MtSelectSite(selectionCriteria, "")) < 0)
            {
                return (mtSiteHandle);
            }

            if ((rc = DynMdbSite.MtFetchSite(mtSiteHandle, out mtSiteStruct, out mtSiteNulls)) == Constant.SUCCESS)
            {
                latitude = mtSiteStruct.latit;
                longitude = mtSiteStruct.longit;
                operCode = mtSiteStruct.oper;
            }
            else
            {
                return (rc);
            }

            DynMdbSite.MtCloseSite(mtSiteHandle);
            return (Constant.SUCCESS);
        }


        /// <summary>
        /// This method inserts records into the user's 'returnvalues' table that correspond to 
        /// a list of site operators that need to be sent a PCN.
        /// </summary>
        /// <param name="screenName"></param>
        /// <param name="operatorList"></param>
        /// <param name="sChar"></param>
        /// <param name="cKey"></param>
        private static void OperatorLoadTable(string screenName, List<string> operatorList, string sChar, string cKey)
        {
            int i;
            const int MAX_OPERS = 100; // 100;

            string[] opers = new string[MAX_OPERS];

            // Apply the legacy C++ code's 'Too many operators' constraint.
            if (operatorList.Count > MAX_OPERS)
            {
                Console.Write("\npcnscan: Too many operators, only {0} will be returned.\n", MAX_OPERS);
                opers = operatorList.GetRange(0, MAX_OPERS).ToArray();
            }
            else
            {
                opers = operatorList.ToArray();
            }

            /*	Insert them into the return table */
            Retval.RetVal(cKey, ref opers);

            if (mToConsole)
            {
                // Convert the opers string elements to upper case.
                for (i = 0; i < opers.Length; i++) opers[i] = opers[i].ToUpper();

                // Sort the operator names into alphabetical order.
                Array.Sort(opers, StringComparer.Ordinal);

                // Write results to the console.
                Console.Write("\r\nPCN: operators to contact");
                Console.Write("\r\n=========================\r\n");

                string[] nameops = new string[opers.Length];

                int retVal = FetchOperLongNames(opers, out nameops);

                for (i = 0; i < opers.Length; i++)
                {
                    if (retVal == Constant.SUCCESS)
                    {
                        Console.Write("\r\n{0,5}.   {1} - {2} ", i + 1, opers[i], nameops[i]);
                    }
                    else
                    {
                        Console.Write("\r\n{0,5}.   {1}", i + 1, opers[i]);
                    }
                }

                Console.Write("\r\n");
            }

        }

        /// <summary>
        /// This method writes a replica of elaborated list of PCN-affected operators presented
        /// by WebMICS to a text logFile whose path was prescribed in the user's command line 
        /// invocation of the program PCNscan.
        /// </summary>
        /// <param name="str"></param>
        private static void WriteToLogFile(string str)
        {
            string text = "\r\n" + Info.ToFormatA(DateTime.Now) + "\r\n: " + str;

            mTWlogFile.Write(text);
        }

        /// <summary>
        /// This method returns an array of elaborated operator names, corresponding to the 
        /// 'nameop' column of SDB table main.sd_oper, corresponding to a prescribed list
        /// of abbreviated operator names (the 'oper' column of the same table).
        /// </summary>
        /// <param name="opers"></param>
        /// <param name="nameops"></param>
        /// <returns></returns>
        private static int FetchOperLongNames(string[] opers, out string[] nameops)
        {
            // 'out' requirement.
            nameops = new string[opers.Length];

            int retVal = Constant.SUCCESS;
            SQLHANDLE hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            SQLLEN nullInd;

            for (int i = 0; i < opers.Length; i++)
            {
                // Fetch the long name for the operator: the nameop column in main.sd_oper

                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

                string cSQL = String.Format("SELECT nameop FROM main.sd_oper WHERE oper='{0}'", opers[i]);

                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                sqlRet = ODBC.SQLFetch(hStmt);

                try
                {
                    Ssutil.DbGetString(hStmt, 1, "nameop", out nameops[i], Constant.NAMEOP_SZ, out nullInd);

                    if (nullInd == Constant.DB_NULL) nameops[i] = "null";
                }
                catch (Exception e)
                {
                    Log2.e("\nPCNscan.FetchOperLongNames(): ERROR: call to ODBC.DbGetString() threw exception: " + e.Message);
                    retVal = Constant.FAILURE;
                }

                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            }

            Ssutil.DisConn(hConn);

            return retVal;
        }




    }
}


