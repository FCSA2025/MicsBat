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
/// This program inserts new records and updates or deletes existing records
/// in the TS-specific Main Data Base (MDB) tables i.a.w. the directives 
/// contained in successfully validated TS PDF import file table sets.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - MtUpdate.PNG" ""
/// </remarks>
/// 
namespace MtUpdate
{
    using SQLLEN = Int64;
    using SQLHDBC = IntPtr;
    using SQLRETURN = Int16;
    using SQLUINTEGER = UInt32;
    using SQLPOINTER = IntPtr;
    using System.Runtime.InteropServices;
    using System.IO;
    /// <summary>
    /// This class provides the Main() method for the MtUpdate application that
    /// inserts new records and updates or deletes existing records
    /// in the TS-specific Main Data Base (MDB) tables i.a.w. the directives 
    /// contained in successfully validated TS PDF import file table sets.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class MtUpdate
    {
        private static bool mIsCreateSpoofTables = false;
        private static bool mIsAbortIfAlreadyPosted = true;
        private static bool mIsWriteSpoofTestData = false;

        /// <summary>
        /// This is the Main() method for the MICS program MtUpdate and
        /// provides high-level functionality and control.
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if true
                string mLog2FilePath = @"d:\MicsBatchLogs\MtUpdate.log";
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
                int rc = 0;
                string validatedFor;
                int exitCode = 0;
                string str;

                int[] addCount;     //  total recs added , site, ante, chan 
                int[] delCount;     //  total recs deleted, site, ante, chan 
                int[] updCount;     //  total recs updated, site, ante, chan 
                int[] totCount;     //  total records procd, site, ante, chan 
                int[] chngCount;    //  total records changed, site ante, chan, route town 

                // Enable attempts to insert/update/delete MDB table records.
                // Disable 'spoof' mode.
                // These can be negated for test purposes using command line flags.
                Info.MdbWriteEnabled = true;

                // Disable the 'spoofing' mode.
                // The spoofing mode replaces the MDB schema 'main' with the schema of the
                // user that executed this program (e.g. 'hulme'). Consequently, hulme.mt_chan
                // acts as a surrogate for main.mt_chan etc.
                // Spoofing mode can be turned on for test purposes using a command line flag.
                Info.SpoofModeIsOff = true;

                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                GetEnvVariablesForUtConnect();

                //	Grab write access to the database, if possible.  If not, exit.
                if ((rc = Qutils.EnterQueue(Info.DbName, "WRITE", 120)) != Constant.SUCCESS)
                {
                    Log2.e("\nMtUpdate.Main(): ERROR: call to Qutils.EnterQueue() failed, rc = " + rc);
                    Qutils.ExplainQueue(Info.DbName, "WRITE", rc, null);
                    Application.ExitQuietly(100);
                }

                // Establish an FCSA user session with the database.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    //  Can't connect to database 
                    str = String.Format("\r\nMtUpdate.Main(): ERROR: Cannot connect to database {0}, reason {1:D}\r\n", Info.DbName, rc);
                    Log2.e(str);
                    Console.Write(str);
                    Qutils.ExitQueue(Info.DbName, "READ");
                    Application.Exit(Error.ODBC_SQLCONNECT_FAILED);
                }

                // Create new spoof tables and populate with test data, as required.
                if (mIsCreateSpoofTables)
                {
                    Spoofing.CreateSpoofMdbTables(Info.GlobalSchema, mIsWriteSpoofTestData);
                }

                // Initialize the billing.
                BiUtil.BiBillingRec("MTUPDATE", Info.PdfName);

                //...Log2.v("\nMtUpdate.Main(): Info:\n" + Info.ToString());

                // Verify that the TS pdf exists; if not, clean up then exit.
                if (!Ssutil.UtTableExist(Constant.FT, Info.PdfName))
                {
                    Log2.e("\r\nMtUpdate.Main(): ERROR: One or more user ft_ tables was not found in the database for root: {0}.", Info.PdfName);
                    Console.Write("\r\nERROR! \r\nPDF '{0}' does not exist\r\n\r\n", Info.PdfName);
                    exitCode = Error.USERTABLENOTFOUND;
                    goto cleanUpAndExit;
                }

                // If file has already updated the mdb, dont update.
                if ((rc = FilewUtil.UtFilewValidated(Constant.FT, Info.PdfName, out validatedFor)) != Constant.SUCCESS)
                {
                    str = String.Format("\nMtUpdate.Main(): ERROR: unable to fetch validation code from table {0}.ft_{1}_titl.", Info.GlobalSchema, Info.PdfName);
                    Log2.e(str);
                    ErrMsg.UtPrintMessage(Error.FILEWVAL);
                    exitCode = Error.FTTITLEFETCHFAILED;
                    goto cleanUpAndExit;
                }

                // Check if the PDF's TS _title record has its 'validated' column set to 
                // 'P' for posted. Abort processing or ignore depending on command line option.
                if (validatedFor[0].Equals(Constant.UPDATE_POSTED))
                {
                    if (mIsAbortIfAlreadyPosted)
                    {
                        Log2.e("\nMtUpdate.Main(): ERROR: These user tables have already updated the MDB.");
                        ErrMsg.UtPrintMessage(Error.ALREADYPOSTED, "TS", Info.PdfName, "Update");
                        Console.Write("\r\n");
                        exitCode = Error.ALREADYPOSTED;
                        goto cleanUpAndExit;
                    }
                    else
                    {
                        // Cheat.
                        validatedFor = "U";
                    }
                }

                // We only update the MDB tables if the PDF's TS _title record has its 
                // 'validated' column set to either 'M' or 'U'.
                if (!validatedFor[0].Equals(Constant.UPDATE_VALIDATED) &&
                    !validatedFor[0].Equals(Constant.M_UPDATE_VALIDATED))
                {
                    //	Not validated for update.
                    Log2.e("\nMtValidate.Main(): ERROR: These user tables have not been validated for UPDATE.");
                    Console.Write("\r\n*ERROR* - Filew has not been validated for update.\r\n");
                    exitCode = Error.NOTMDBUPDVAL;
                    goto cleanUpAndExit;
                }

                // Initialize the counters.
                const int NDIM = 3;
                addCount = Arrays.CreateAndFillArray(NDIM, 0);    // total recs added , site, ante, chan 
                delCount = Arrays.CreateAndFillArray(NDIM, 0);    // total recs deleted, site, ante, chan 
                updCount = Arrays.CreateAndFillArray(NDIM, 0);    // total recs updated, site, ante, chan 
                totCount = Arrays.CreateAndFillArray(NDIM, 0);    // total records procd, site, ante, chan 
                chngCount = Arrays.CreateAndFillArray(NDIM + 2, 0);   // total record changed, site, ante, chan, route, town 

                // Set the operating state of the classes that provide high-level read/write
                // methods that access TS MDB tables.
                DynMdbAntenna.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMdbChannel.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMdbSite.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynSdbTown.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynSdbRout.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);

                // Set the operating state of the classes that provide high-level read/write
                // methods that access the DB adm.audit_trail table.
                DynAuditTrail.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);

                // ============================================================
                // Call the update controller method.
                // This method provides the core functionality for MtUpdate.
                // ============================================================
                rc = MtDoTSUpdate(Info.PdfName, ref addCount, ref delCount, ref updCount,
                                    ref totCount, ref chngCount);

                // Release the hold on the database a.s.a.p.
                Qutils.ExitQueue(Info.DbName, "WRITE");

                // Count the total number of TS MDB table records that were changed.
                int totalProcessed = 0;

                totalProcessed += SumArrayElements(totCount);
                totalProcessed += SumArrayElements(chngCount);

                // Handle errors and/or no records changed scenario.
                if (rc != Constant.SUCCESS || totalProcessed == 0)
                {
                    Console.Write("\r\n");
                    Console.Write("\r\nWARNING!");
                    Console.Write("\r\nNo records were Updated, Deleted, or Added");
                    Console.Write("\r\n\r\n");
                    goto cleanUpAndExit;
                }

                // Some records were changed: write change summary to the Console.
                if (Info.MdbWriteEnabled)
                {
                    // Print summary of changes 
                    PrintSummaryOfChanges(addCount, delCount, updCount, totCount, chngCount);

                    // Update the validate flag to 'P'osted.
                    // This method calls AtRecordChng.AtRecordChange()
                    SetValidationToPosted();
                }
                // If writing to the MDB has been prohibited by the user then write to Console all
                // of the SQL queries that were inhibited.
                else
                {
                    Console.Write("\r\n");
                    Console.Write("\r\n[-d] command-line option: INHIBITED SQL INSERT/UPDATE QUERIES:");
                    Console.Write("\r\n=============================================================");
                    Console.Write("\r\n");
                    Console.Write(Info.Text);
                    Console.Write("\r\n\r\nUPDATE {0}.ft_{1}_titl SET validated = 'P'", Info.GlobalSchema, Info.PdfName);
                    Console.Write("\r\n");
                }

                // Although using a 'goto to label' construct is somewhat taboo
                // it has been adopted in this Main() program to avoid having multiple nested
                // if-else constructs that would make it significantly more difficult 
                // to comprehend the flow of control.
                cleanUpAndExit:

                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(exitCode);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nMtUpdate.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nMtUpdate.Main(): stack trace: \r\n\r\n" + e.StackTrace);
                BiUtil.BiBillingRec(Constant.BI_END, "");
                Ssutil.UtDisconnect(1);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

        }

        /// <summary>
        /// This method gets the values of Windows environment variables that
        /// are required to execute GetCoords; specifically these are 'MICSUSER'
        /// and 'PASSWORD' that are required for a successful call to UtConnect().
        /// </summary>
        public static void GetEnvVariablesForUtConnect()
        {
            // Get the user's MICS ID from the environment.
            Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                Log2.e("\r\nFtImport.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
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
            Console.Write("\r\n MtUpdate updates the TS 'main' tables with records from a ");
            Console.Write("\r\n previously imported and validated TS PDF.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: MtUpdate  <dbName> <projectCode> <pdfName> [-c] [-d] [-p] [-s] [-t] [-!<schema>]");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        projectCode      : user's project billing code.");
            Console.Write("\r\n        pdfName          : TS PDF table set root name (the XXX in ft_XXX_site, etc).");
            Console.Write("\r\n");
            Console.Write("\r\n        --- For Testing -------------------------------------------------------------");
            Console.Write("\r\n        -c               : create empty TS spoof tables in the user's schema.");
            Console.Write("\r\n        -d               : inhibit attempts to insert/update MDB or user tables and instead");
            Console.Write("\r\n                           write the inhibated SQL queries to the console for inspection.");
            Console.Write("\r\n        -p               : process update even when TS _titl validated is 'P'osted.");
            Console.Write("\r\n        -s               : enable spoof mode - schema 'main' is replaced by user's schema.");
            Console.Write("\r\n        -t               : create empty TS spoof tables and populate with simple test data.");
            Console.Write("\r\n        -!<schema>       : creates and populates user tables with the precribed schema name.");
            Console.Write("\r\n                           Usage restricted to users who are members of the Administrator Group.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     MtUpdate  fcsa  HULME1_0  tsupdates  ");
            Console.Write("\r\n");
            Console.Write("\r\n     MtUpdate  fcsa  HULME1_0  tsupdates -!fmda2");
            Console.Write("\r\n     ");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with qty. 3 manadatory command-line arguments.");
            Console.Write("\r\n       2. Options can be placed anywhere, in any order, on the command-line.");
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
                if (arg.StartsWith("-"))
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

                // Parse the character after the '-'.
                char secondChar = arg.ToUpper()[1];
                switch (secondChar)
                {
                    case 'D':
                        Info.MdbWriteEnabled = false;
                        break;
                    case 'S':
                        Info.SpoofModeIsOff = false;
                        break;
                    case 'C':
                        mIsCreateSpoofTables = true;
                        break;
                    case 'P':
                        mIsAbortIfAlreadyPosted = false;
                        break;
                    case 'T':
                        mIsCreateSpoofTables = true;
                        mIsWriteSpoofTestData = true;
                        break;
                    case '!':
                        if (arg.Length > 2)
                        {
                            string schemaName = arg.Substring(2);

                            // If we don't check the user's permissions now the program may fail at
                            // run time with an SQL error.
                            if (User.IsInAdministratorGroup())
                            {
                                // Save the schema to the Info static structure member.
                                // This has a global effect throughout the code so that all synthesized SQL
                                // queries will explicitely include the schema name 'forced' on the command-line.
                                Info.GlobalSchema = schemaName;
                                Ssutil.Glb_Schema = schemaName;
                            }
                            else
                            {
                                Console.Write("\n The -! option is restricted to users who are members of the Administrators Group.");
                                WriteUsageToConsole();
                                Environment.Exit(98);
                            }
                        }
                        else
                        {
                            Console.Write("\n Invalid syntax for option -!");
                            WriteUsageToConsole();
                            Environment.Exit(98);
                        }
                        break;
                    default:
                        Console.Write("\r\n Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                        break;
                }

            }

            // Parse the regular arguments; there must be qty. 4 of them.
            if (regularArgs.Count != 3)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Constant.FAILURE);
            }

            // Parse the args.
            Info.DbName = regularArgs[0];
            Info.ProjectCode = regularArgs[1];
            Info.PdfName = regularArgs[2];

            // Truncate the PdfName to the first 16 characters.
            if (Info.PdfName.Length > Constant.DISP_NM_SZ - 1)
            {
                Info.PdfName = Info.PdfName.Substring(0, Constant.DISP_NM_SZ - 1);
            }
        }

        /// <summary>
        /// This method provides the core functionality of the MtUpdate application
        /// </summary>
        /// <param name="pdfName"></param>
        /// <param name="addCount"> - number of records added, site, antenna, channel.</param>
        /// <param name="delCount"> - number of records deleted, site, antenna, channel.</param>
        /// <param name="updCount"> - number of records updated, site, antenna, channel.</param>
        /// <param name="totCount"> - number of records processed, site, antenna, channel.</param>
        /// <param name="chngCount"> - number of records affected by Change Call (GK).</param>
        /// <returns></returns>
        private static int MtDoTSUpdate(string pdfName,
                                        ref int[] addCount,     // number of records added, site, antenna, channel 
                                        ref int[] delCount,     // number of records deleted, site, antenna, channel 
                                        ref int[] updCount,     // number of records updated, site, antenna, channel 
                                        ref int[] totCount,     // number of records processed, site, antenna, channel
                                        ref int[] chngCount    // number of records affected by Change Call 
                                        )
        {
            FtTitl ftTitl;
            SQLLEN[] nullInds;

            // Local variables 
            UserInfoData userInfo;
            int titlHandle;
            int status = Constant.SUCCESS;
            string curDate;
            string curTime;
            string fullFtTitlTableName;
            int rc;

            SQLHDBC hConn;

            // Get the user's username for report output 
            if ((rc = UserInfo.UtGetUserInfo(out userInfo)) != Constant.SUCCESS)
            {
                ErrMsg.UtPrintMessage(rc);
                Log2.e("\n\nMtUpdate.MtDoTSUpdate(): ERROR: call to UserInfo.UtGetUserInfo() failed.");
                return (Constant.FAILURE);
            }

            // Get current time/date for report header 
            GenUtil.UtGetDateTime(out curDate, out curTime);

            // Print header for TS Update report 
            Console.Write("Date: {0}            Build: {1}            Time: {2}\r\n",
                                curDate, Info.BuildMetaData, curTime);
            Console.Write("          ****************** Master DataBase Update ******************\r\n");
            Console.Write("          ***  PDF Name: {0,-16}      User Name: {1,-7} ****\r\n",
                                    pdfName, userInfo.micsUser.micsid);

            // Convert TS PDF name from short to long format 
            GenUtil.UtCvtName(Constant.FT_TITL, pdfName, out fullFtTitlTableName);

            // Get a handle on the TS PDF data - to access Title record 
            if ((titlHandle = DynFtTitl.FtSelectTitl(fullFtTitlTableName, "", "")) < 0)
            {
                Console.Write("\r\nERROR! \r\nCould not access pdf title record\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                Console.Write("\r\n");
                return (Constant.FAILURE);
            }

            // Check PDF for validation mark 
            if ((DynFtTitl.FtFetchTitl(titlHandle, out ftTitl, out nullInds)) != Constant.SUCCESS)
            {
                Console.Write("\r\nERROR! \r\nCould not read pdf title record.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                Console.Write("\r\n");
                return (Constant.FAILURE);
            }
            else
            {
                DynFtTitl.FtCloseTitl(titlHandle);
            }

            // Begin transaction for updates */
            hConn = Ssutil.NewConn();

            // Change to Manual Commit mode.  This will commit everything up to here
            // and not commit anything until we explicitly commit it or roll it back.
            rc = Ssutil.SetCommitMode(hConn, Enums.COMMIT.MANUAL);

            //sqlRet = ODBC.SQLSetConnectAttr(hConn, ODBC.SQL_ATTR_AUTOCOMMIT, (SQLPOINTER)ODBC.SQL_AUTOCOMMIT_OFF, ODBC.SQL_IS_UINTEGER);

            if (rc != Constant.SUCCESS)
            {
                //	Could not set SQL commit mode to manual.
                Log2.e("\nMtUpdate.MtDoEsUpdate(): ERROR: could not set the database connection to manual commit.");
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_DBC, hConn);
                Console.Write("\r\nMtDoTsUpdate: Could not set the database connection to manual commit.\r\n{0}\r\n",
                                GenUtil.GetUserMess());
                return Constant.FAILURE;
            }

            // Handle channel records from PDF 
            rc = Mdb.UpdateMtChanTable(hConn, pdfName, userInfo.micsUser.micsid, ref addCount[Constant.CHANINDEX],
                                              ref delCount[Constant.CHANINDEX], ref updCount[Constant.CHANINDEX], ref totCount[Constant.CHANINDEX]);
            if (rc != Constant.SUCCESS)
            {
                Log2.e("\nMtUpdate.MtDoEsUpdate(): ERROR: call to UpdateMtChanTable() failed, rc = " + rc);
                status = Constant.FAILURE;
            }

            // Handle antenna records from PDF 
            rc = Mdb.UpdateMtAnteTable(hConn, pdfName, userInfo.micsUser.micsid, ref addCount[Constant.ANTEINDEX],
                                              ref delCount[Constant.ANTEINDEX], ref updCount[Constant.ANTEINDEX], ref totCount[Constant.ANTEINDEX]);
            if (rc != Constant.SUCCESS)
            {
                Log2.e("\nMtUpdate.MtDoEsUpdate(): ERROR: call to UpdateMtAnteTable() failed, rc = " + rc);
                status = Constant.FAILURE;
            }

            // Handle site records from PDF 
            rc = Mdb.UpdateMtSiteTable(hConn, pdfName, userInfo.micsUser.micsid, ref addCount[Constant.SITEINDEX], ref delCount[Constant.SITEINDEX],
                                              ref updCount[Constant.SITEINDEX], ref totCount[Constant.SITEINDEX]);
            if (rc != Constant.SUCCESS)
            {
                Log2.e("\nMtUpdate.MtDoEsUpdate(): ERROR: call to UpdateMtSiteTable() failed, rc = " + rc);
                status = Constant.FAILURE;
            }

            // Handle change of callsign records 
            rc = Mdb.UpdateMtChng(hConn, pdfName, ref chngCount, userInfo.micsUser.micsid);
            if (rc != Constant.SUCCESS)
            {
                Log2.e("\nMtUpdate.MtDoEsUpdate(): ERROR: call to UpdateMtChng() failed, rc = " + rc);
                status = Constant.FAILURE;
            }

            // Decide to commit or rollback, then return 
            if (status == Constant.SUCCESS)
            {
                Ssutil.DbCommit(hConn);

                Ssutil.DisConn(hConn);

                status = Constant.SUCCESS;
            }
            else
            {
                Ssutil.DbRollBack(hConn);

                Ssutil.DisConn(hConn);

                status = Constant.FAILURE;
            }

            // Restore Commit mode to AUTO.
            Ssutil.SetCommitMode(hConn, Enums.COMMIT.AUTO);

            return status;
        }

        /// <summary>
        /// This method writes to the Console a summary of the number of TS MDB table
        /// records that were added, deleted, modified and/or updated.
        /// </summary>
        /// <param name="addCount"> - number of records added, site, antenna, channel.</param>
        /// <param name="delCount"> - number of records deleted, site, antenna, channel.</param>
        /// <param name="updCount"> - number of records updated, site, antenna, channel.</param>
        /// <param name="totCount"> - number of records processed, site, antenna, channel.</param>
        /// <param name="chngCount"> - number of records affected by Change Call (GK).</param>
        private static void PrintSummaryOfChanges(int[] addCount,
                                                    int[] delCount,
                                                    int[] updCount,
                                                    int[] totCount,
                                                    int[] chngCount
                                                    )
        {
            // Site record processing count 
            Console.Write("\r\n");
            Console.Write("\tSite records added       : {0}\r\n", addCount[Constant.SITEINDEX]);
            Console.Write("\tSite records updated     : {0}\r\n", updCount[Constant.SITEINDEX]);
            Console.Write("\tSite records deleted     : {0}\r\n", delCount[Constant.SITEINDEX]);
            Console.Write("\tSite records modified by\r\n");
            Console.Write("\t  change of Call Sign    : {0}\r\n", chngCount[Constant.SITEINDEX]);
            Console.Write("\tSite records processed   : {0}\r\n", totCount[Constant.SITEINDEX]);

            // Antenna record processing count 
            Console.Write("\r\n");
            Console.Write("\tAntenna records added    : {0}\r\n", addCount[Constant.ANTEINDEX]);
            Console.Write("\tAntenna records updated  : {0}\r\n", updCount[Constant.ANTEINDEX]);
            Console.Write("\tAntenna records deleted  : {0}\r\n", delCount[Constant.ANTEINDEX]);
            Console.Write("\tAntenna records modified by\r\n");
            Console.Write("\t  change of Call Sign    : {0}\r\n", chngCount[Constant.ANTEINDEX]);
            Console.Write("\tAntenna records processed: {0}\r\n", totCount[Constant.ANTEINDEX]);

            // Channel record processing count 
            Console.Write("\r\n");
            Console.Write("\tChannel records added    : {0}\r\n", addCount[Constant.CHANINDEX]);
            Console.Write("\tChannel records updated  : {0}\r\n", updCount[Constant.CHANINDEX]);
            Console.Write("\tChannel records deleted  : {0}\r\n", delCount[Constant.CHANINDEX]);
            Console.Write("\tChannel records modified by\r\n");
            Console.Write("\t  change of Call Sign    : {0}\r\n", chngCount[Constant.CHANINDEX]);
            Console.Write("\tChannel records processed: {0}\r\n", totCount[Constant.CHANINDEX]);

            /*
             * TASK 464: Added messages for the number of Tower Notes and
             * Routes records modified by change of Call Sign
             */
            Console.Write("\r\n\tTower Notes records modified by\r\n");
            Console.Write("\t  change of Call Sign    : {0}\r\n", chngCount[Constant.TOWNINDEX]);
            Console.Write("\tRoutes records modified by\r\n");
            Console.Write("\t  change of Call Sign    : {0}\r\n", chngCount[Constant.ROUTINDEX]);

            Console.Write("\r\nMaster DataBase Update succesfully completed.\r\n");
            Console.Write("\r\n");
        }

        /// <summary>
        /// This methods sets the validation field in the TS PDF's _titl table record and
        /// the web.user_tables record to 'P' for 'Posted'.
        /// </summary>
        private static void SetValidationToPosted()
        {
            // Update the validate flag - P)osted
            if (Info.MdbWriteEnabled)
            {
                int cursorID;
                FtTitl ftTitl;
                SQLLEN[] nullInds;
                string fullTitlTableName;
                int rc;

                AtRecordChng.AtRecordChange(Info.DbName, Info.PdfName, Constant.FT);

                GenUtil.UtCvtName(Constant.FT_TITL, Info.PdfName, out fullTitlTableName);

                if ((cursorID = DynFtTitl.FtSelectTitl(fullTitlTableName, "", "")) < 0)
                {
                    Log2.e("\nMtUpdate.Main(): ERROR: call to FtSelectTitl() failed, fullTitlTableName = " + fullTitlTableName);
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }
                else
                {

                    if ((rc = DynFtTitl.FtFetchTitl(cursorID, out ftTitl, out nullInds)) == Constant.SUCCESS)
                    {
                        ftTitl.validated = Constant.UPDATE_POSTED.ToString();
                        nullInds[FtTitl.VALIDATED] = Constant.DB_NOT_NULL;

                        DynFtTitl.FtUpdateTitl(cursorID, ftTitl, nullInds);
                    }
                    else
                    {
                        Log2.e("\nMtUpdate.Main(): ERROR: call to FtFetchTitl() failed, fullTitlTableName = " + fullTitlTableName);
                        ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    }

                    DynFtTitl.FtCloseTitl(cursorID);

                    UserInfo.UtUpdateCentralTable("U", Info.PdfName, Constant.FT, "P", "N");
                }
            }
        }

        /// <summary>
        /// This method returns the summation of all of the elements of
        /// a prescribed integer array.
        /// </summary>
        /// <param name="elements"> - array of integers.</param>
        /// <returns></returns>
        private static int SumArrayElements(int[] elements)
        {
            int count = 0;

            foreach (int element in elements)
            {
                count += element;
            }

            return count;
        }


    }
}


