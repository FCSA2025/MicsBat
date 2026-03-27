using _Configuration;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    using SQLHANDLE = IntPtr;
    using SQLRETURN = Int16;
    using SQLHDBC = IntPtr;
    using SQLHSTMT = IntPtr;

    public class CreateTableTaflLicenseeNameToFcsaOper
    {
        private const string SQL_TABLE_NAME = "hulme.TaflLicenseeNameToFcsaOper";

        private static Dictionary<string, string> dictionary = new Dictionary<string, string>();

        public static void Go(string[] args)
        {
            try
            {
#if true
                string mLog2FilePath = @"d:\MicsBatchLogs\IsedTaflToTable.log";
                if (Log2.SetLogFilePath(mLog2FilePath))
                {
                    Log2.Erase();
                    Log2.Set(Log2.FileOpenClose.PER_SESSION);
                    Log2.Set(Log2.WriteMode.ENABLED);
                    Log2.Set(Log2.Level.VERBOSE);
                    Info.BuildMetaData = Info.CollateExeMetaData();
                    Log2.v("\nBuild: " + Info.BuildMetaData);
                }
                else
                {
                    Console.Error.Write("\r\nERROR: could not open Log2 file: " + mLog2FilePath);
                }
#endif
                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                // File the dictionary with <MICSoper, ISED LicenseeName> string pairs.
                PopulateDictionary();

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                GetEnvVariablesForUtConnect();

                // Establish an FCSA user session with the database.
                int rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    Console.Write("TBD -- Can't connect to database {0}.\r\n", Info.DbName);

                    Application.ExitQuietly(11);
                }

                // Set up to launch SQL queries.
                SQLHDBC hConn = Ssutil.NewConn();
                SQLHSTMT hStmt;
                ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

                // If the target SQL table already exists then drop it.
                string query = String.Format("IF OBJECT_ID('{0}') IS NOT NULL DROP TABLE {0}", SQL_TABLE_NAME);
                DoSQL(hStmt, query);

                // Create a new SQL table.
                query = "" +
                        "CREATE TABLE " + SQL_TABLE_NAME + " (" +
                            "[ISEDLicenseeName][varchar](1000)," +
                            "[MICSoper][char](6)" +
                            "CONSTRAINT[PK_" + SQL_TABLE_NAME + "] PRIMARY KEY CLUSTERED " +
                            "(" +
                                "[ISEDLicenseeName] ASC" +
                            ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON[PRIMARY]" +
                        ") ON[PRIMARY]";

                DoSQL(hStmt, query);

                // Insert the records from the <MICSoper, ISED LicenseeName> dictionary into the SQL table.
                int numRecords = 0;
                foreach(KeyValuePair<string,string> kvp in dictionary)
                {
                    query = String.Format("INSERT INTO {0} VALUES ('{1}', '{2}')", SQL_TABLE_NAME, kvp.Key, kvp.Value);
                    DoSQL(hStmt, query);

                    numRecords++;
                    Console.Write("\nInserted:   {0,-64}, {1}", kvp.Key, kvp.Value);
                }

                Console.Write("\n\n{0} records inserted into SQL table: {1}", numRecords, SQL_TABLE_NAME);

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(0);
            }
            catch (Exception e)
            {
                string errMsg = String.Format("\n\nGo(): ERROR: exception: {0}\n{1}", e.Message, e.StackTrace);
                Console.Write(errMsg);
                Log2.e(errMsg);
            }
        }

        private static void PopulateDictionary()
        {
            dictionary.Add("ABC ALLEN BUSINESS COMMUNICATIONS LTD.", "ABCCOM");
            dictionary.Add("ABC COMMUNICATIONS LTD", "ABCCOM");
            dictionary.Add("BC HYDRO (FIXED)", "BCHY");
            dictionary.Add("BC HYDRO (LAND MOBILE)", "BCHY");
            dictionary.Add("BC HYDRO (WIMAX)", "BCHY");
            dictionary.Add("BELL MEDIA", "BELL");
            dictionary.Add("BELL MEDIA INC", "BELL");
            dictionary.Add("BELL MEDIA INC.", "BELL");
            dictionary.Add("BELL MEDIA INC.  (CHVR)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CFIX-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CFVM-FM / CIKI-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CFZZ-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CHEY-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CHIK-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CHRD-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CIGB-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CIKI-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CIMF-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CIMO-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CITE-FM1)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CITF-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CJAB-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CJAD-AM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CJDM-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CJMV-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CJOI-FM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CKGM-AM)", "BELL");
            dictionary.Add("BELL MÉDIA INC. (CKTF-FM)", "BELL");
            dictionary.Add("BELL MEDIA INC., CTV BARRIE, SCOTT WILLIAMS PEIN# 6064762", "BELL");
            dictionary.Add("BELL MEDIA RADIO ATLANTIC INC.", "BELL");
            dictionary.Add("BELL MEDIA RADIO ATLANTIC INC. ATT  TRISH DICKINSON PEIN#6075813", "BELL");
            dictionary.Add("BELL MEDIA RADIO G.P.", "BELL");
            dictionary.Add("BELL MEIDA INC. - CIDR-FM, A DIVISION OF CHUM LIMITED", "BELL");
            dictionary.Add("BELL MOBILITY INC.", "BMCE");
            dictionary.Add("BELL MOBILITY INC. (RADIO DIVISION)", "BMCE");
            dictionary.Add("BRAGG COMMUNICATIONS INC.", "BRAGG");
            dictionary.Add("FREEDOM MOBILE INC.", "GLW");
            dictionary.Add("HYDRO ONE - SAULT STE. MARIE LP", "HYONE");
            dictionary.Add("HYDRO ONE NETWORKS INC. CARRIER SERVICES", "HYONE");
            dictionary.Add("HYDRO-QUÉBEC", "HYQU");
            dictionary.Add("BELL MTS, A DIVISION OF BELL CANADA", "MTS");
            dictionary.Add("NAVIGATA COMMUNICATIONS LIMITED", "NAVI");
            dictionary.Add("BELL CANADA", "NTTEL");
            dictionary.Add("BELL CANADA - WIRELESS MGMT OFFICE", "NTTEL");
            dictionary.Add("NORTHWESTEL INC.", "NWT");
            dictionary.Add("NORTHERN TEL – WIRELESS MGMT OFFICE", "ONT");
            dictionary.Add("ROGERS COMMUNICATIONS CANADA INC.", "RCTL");
            dictionary.Add("SHAW CABLESYSTEMS (MANITOBA) LTD.", "SHAW  ");
            dictionary.Add("SHAW CABLESYSTEMS G.P.", "SHAW  ");
            dictionary.Add("SHAW CABLESYSTEMS GP", "SHAW  ");
            dictionary.Add("SHAW CABLESYSTEMS INC.", "SHAW  ");
            dictionary.Add("SHAW TELECOM G.P.", "SHAW  ");
            dictionary.Add("SASKTEL", "STEL");
            dictionary.Add("TBAYTEL - FIXED", "TBAY");
            dictionary.Add("TELESAT CANADA                         ", "TELS");
            dictionary.Add("TERAGO NETWORKS INC.", "TERAGO");
            dictionary.Add("TELUS COMMUNICATIONS INC.", "TLUSMC");
            dictionary.Add("VIDÉOTRON LTÉE", "VDTR");
            dictionary.Add("WIREIE INC.", "WIREIE");
            dictionary.Add("XPLORE INC.", "XCI");
            dictionary.Add("ZAYO CANADA INC.", "ZAYO");

        }

        /// <summary>
        /// This method encapsulates the submission of a query to the ODBC / SQL Server 
        /// without having to specify the length of the query string.
        /// </summary>
        /// <param name="hStmt"> - an open ODBC statement handle</param>
        /// <param name="query"> - SQL query to be submitted.</param>
        /// <returns></returns>
        public static int DoSQL(SQLHANDLE hStmt, string query)
        {
            SQLRETURN sqlRet = 0;

            sqlRet = ODBC.SQLExecDirect(hStmt, query, query.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string errmsg = ODBC.GetDiagnostics(hStmt, query);
                Console.Write("\n" + errmsg);
                Log2.e("\n" + errmsg);
            }

            return sqlRet;
        }

        /// <summary>
        /// This methods gets the values of Windows environment variables that
        /// are required to execute GetCoords; specifically these are 'MICSUSER'
        /// and 'PASSWORD' that are required for a successful call to UtConnect().
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
            Console.Write("\r\n This program overwrites the SQL table [TaflLicenseeNameToFcsaOper] with fresh values.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: TaflLicenseeNameToFcsaOper <dbName>");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     TaflLicenseeNameToFcsaOper fcsa");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with qty. 1 command-line arguments.");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the user-prescribed command-line arguments.
        /// </summary>
        /// <param name="args"></param>
        public static void ParseCommandLineArgs(string[] args)
        {
            if (args.Length == 0)
            {
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Create separate lists of 'flag' args prefixed with '-' and those that
            // are not.
            List<string> flagArgs = new List<string>();
            List<string> regularArgs = new List<string>();

            foreach (string arg in args)
            {
                if (Strings.IsNumeric(arg))
                {
                    regularArgs.Add(arg);
                }
                else if (arg.StartsWith("-"))
                {
                    flagArgs.Add(arg);
                }
                else
                {
                    regularArgs.Add(arg);
                }
            }

            // Parse the flags.
            foreach (string arg in flagArgs)
            {
                // Check that we don't just have a minus character.
                if (arg.Length == 1)
                {
                    Console.Write("\r\n Invalid flag: '{0}'", arg);
                    WriteUsageToConsole();
                    Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                }

                // Process the flags.
                string flag = Strings.DropFirstChar(arg).ToUpper();
                switch (flag)
                {
                    case "D":
                        Info.MdbWriteEnabled = false;
                        break;
                    default:
                        Console.Write("\r\n Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                        break;
                }

            }

            // Parse the regular arguments; there must be qty. 1 of them.
            if (regularArgs.Count != 1)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Parse the args.
            Info.DbName = regularArgs[0];
        }



    }
}
