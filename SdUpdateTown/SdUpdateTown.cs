using System;
using System.Collections.Generic;

/// <summary>
/// This program updates the Subsidiary Database (SDB) table <b>main.sd_town</b> using field
/// values provided in the prescribed user SDF table su_XXX_town; 'town' is an abbreviation 
/// for 'tower notes'. 
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - SdUpdateTown.PNG" ""
/// </remarks>
namespace SdUpdateTown
{
    using _DataStructures;
    using _Configuration;
    using _NewLib;
    using _Utillib;
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;
    public enum Action { Processed, Added, Updated, Deleted, NoAction }

    /// <summary>
    /// This class provides the Main() method for the MICS program SdUpdateTown.
    /// </summary>
    public class SdUpdateTown
    {
        private static bool mIsCreateSpoofTables = false;
        private static bool mIsWriteSpoofTestData = false;
        private static bool mIsValidIfAlreadyPosted = false;

        /// <summary>
        /// This is the Main() method for the MICS program SdUpdateTown.
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\SdUpdateTown.log";
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
                int exitCode = 0;
                int[] recCounts;
                int errCount;
                int warnCount;

                // Enable attempts to insert/update/delete MDB table records.
                // Disable 'spoof' mode.
                // These can be negated for test purposes using command line flags.
                Info.MdbWriteEnabled = true;

                // Disable the 'spoofing' mode.
                // The spoofing mode replaces the MDB schema 'main' with the schema of the
                // user that executed this program (e.g. 'hulme'). Consequently, hulme.sd_town
                // acts as a surrogate for main.sd_town etc.
                // Spoofing mode can be turned on for test purposes using a command line flag.
                Info.SpoofModeIsOff = true;

                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                GetEnvVariablesForUtConnect();

                //	Grab WRITE access to the database, if possible.  If not, exit.
                if ((rc = Qutils.EnterQueue(Info.DbName, "WRITE", 5)) != Constant.SUCCESS)
                {
                    Log2.e("\nSdUpdateTown.Main(): ERROR: call to Qutils.EnterQueue() failed, rc = " + rc);
                    Qutils.ExplainQueue(Info.DbName, "WRITE", rc, null);
                    exitCode = 100;
                    goto cleanUpAndExit;
                }

                // Establish a FCSA user session with the database.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    // Can't connect to database.
                    Console.Write("TBD -- Can't connect to database {0}.\r\n", Info.DbName);
                    exitCode = 11;
                    goto cleanUpAndExit;
                }

                // Set the operating state of the classes that provide high-level read/write
                // methods that access SDB tables. These calls must happen after Ssutil.UtConnect()
                // is called so that Info.GlobalSchema becomes known.
                DynSdbTown.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);

                // Set the operating state of the class that provides high-level read/write
                // methods that access the DB adm.audit_table table.
                DynAuditTrail.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);

                // If directed by command-line option, create new spoof tables and populate 
                // with test data.
                if (mIsCreateSpoofTables)
                {
                    Spoofing.CreateSpoofSdbTables(Info.GlobalSchema, mIsWriteSpoofTestData);
                }

                // Construct the string to be placed in the 'info' column in the billing table.
                string infoBuffer = Info.SdfName;

                // Initialize the billing helper static class.
                BiUtil.BiBillingRec("UPD_TOWN", infoBuffer);

                // Initialize the processing status for this SDF to OK.
                int status = Constant.OK;

                // Get the current date and time from the system and print it
                // at the top of each new report or validation.
                Info.SetDateTimeNow();
                Console.Write("\t\tSDB TOWER NOTES UPDATE REPORT Build {0}\t\tDate: {1} {2}\r\n",
                             Info.BuildMetaData, Info.Date, Info.Time);
                Console.Write("\r\n");
                Console.Write("SDF Name: {0}\r\n", Info.SdfName);

                // Check to see that the associated su_XXX_town table for this
                // SDF base name (XXX) actually exists in the DB.
                if (!Ssutil.UtTableExist(Constant.SU_TOWN, Info.SdfName))
                {
                    Console.Write("\r\n************************************************\r\n");
                    Console.Write("\r\nTown sdf {0} does not exist\r\n", Info.SdfName);
                    Console.Write("\r\n************************************************\r\n");
                    status = Constant.FAILURE;
                    exitCode = 98;
                    goto cleanUpAndExit;
                }

                // Change to SQL Manual Commit mode.  This will commit everything up to here
                // and not commit anything until we explicitly commit it or roll it back.
                SQLHDBC hConn = Ssutil.NewConn();
                rc = Ssutil.SetCommitMode(hConn, Enums.COMMIT.MANUAL);

                if (rc != Constant.SUCCESS)
                {
                    // Could not set SQL autocommit off.
                    Ssutil.DbGetDiag(ODBC.SQL_HANDLE_DBC, hConn);
                    Console.Write("\r\nSdUpdateTown: Could not set the database connection to manual commit.\r\n{0}\r\n",
                                    GenUtil.GetUserMess());
                    exitCode = 95;
                    goto cleanUpAndExit;
                }

                Console.Write("\r\n\t\tTOWER NOTES SDF VALIDATION RESULTS\r\n");

                // Perform validation of the prescribed su_XXX_town SDF file even if 
                // it has been validated previously.
                UserTables userTables;
                SQLLEN[] userTablesNullInds;
                status = Validation.Perform(mIsValidIfAlreadyPosted, out userTables, out userTablesNullInds);

                // Check for failure of the validation of the prescribed su_XXX_town SDF file.
                if (status != Constant.SUCCESS)
                {
                    exitCode = 1;
                    goto cleanUpAndExit;
                }
                // ============================================================
                // Call the top-level update controller method.
                // This method provides the core functionality for SdUpdateTown.
                // ============================================================
                rc = Updater.ProcessSuTown(hConn, Info.SdfName,
                                                out errCount,
                                                out warnCount,
                                                out recCounts);

                // Report counts of errors and warnings.
                Console.Write("\r\nSDB UPDATE SUMMARY: ");
                Console.Write("{0} errors and {1} warnings\r\n", errCount, warnCount);
                Console.Write("\r\n\r\n");

                // In the event of failure rollback the changes made to any DB tables
                // and exit the application in an orderly manner.
                if (rc != Constant.SUCCESS)
                {
                    Ssutil.DbRollBack(hConn);
                    exitCode = 1;
                    goto cleanUpAndExit;
                }

                // If we reach here the call to top-level update controller succeeded.
                // Commit the changes to the DB and restore AUTO mode.
                Ssutil.DbCommit(hConn);
                Ssutil.SetCommitMode(hConn, Enums.COMMIT.AUTO);

                // Update the record in web.user_tables to indicate that the SDF was
                // successfully validated.
                userTables.validstat = "P";
                userTables.create_date = new ODBC.TIMESTAMP_STRUCT(Info.Datetime);
                DynUserTables.Update(hConn, userTables, userTablesNullInds);

                // Print out the summary information for the run.
                // If writing to the SDB has been prohibited by the user (the '-d' option)
                // then this method also prints all of the SQL queries that were inhibited.
                WriteResultsToConsole(recCounts);

                // Make a record of the SDF in the audit trail.
                AtRecordChng.AtRecordChange(Info.DbName, Info.SdfName, Constant.SU_TOWN);

                Console.Write("\r\n\r\n\t************     End of Summary Information     ************\r\n");

                //=================================================
                // Perform graceful termination of the application.
                //=================================================

                // Although using a 'goto to label' construct is somewhat taboo it has
                // been adopted in this Main() program to provide a single point of 
                // exit from the application to ensure release the hold on the WRITE queue,
                // write the billing record and release ODBC resources.
                cleanUpAndExit:

                // Release the hold on the database a.s.a.p.
                Qutils.ExitQueue(Info.DbName, "WRITE");

                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(exitCode);

            }
            catch (Exception e)
            {
                Log2.e("\r\r\n\nSdUpdateTown.Main(): exception caught: " + e.Message);
                Log2.e("\r\r\n\nSdUpdateTown.Main(): stack trace: \r\r\n\n" + e.StackTrace);
                BiUtil.BiBillingRec(Constant.BI_END, "");
                Ssutil.UtDisconnect(1);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

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
            Console.Write("\r\n This program updates the Subsidiary Database (SDB) table main.sd_town using field   +");
            Console.Write("\r\n values provided in the prescribed user SDF table [user].su_[sdfName]_town.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: SdUpdateTown <dbName> <projCode> <sdfName> [-c] [-d] [-p] [-s] [-t]");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        project          : user's project 'charge' code.");
            Console.Write("\r\n        sdfName          : the XXX in user table su_XXX_town.");
            Console.Write("\r\n");
            Console.Write("\r\n        --- For Testing -------------------------------------------------------------");
            Console.Write("\r\n        -c               : create empty TS spoof tables in the user's schema.");
            Console.Write("\r\n        -d               : disable attempts to insert/update/delete MDB table records.");
            Console.Write("\r\n        -p               : force validation if already posted (web.user_tables: validstat = 'P').");
            Console.Write("\r\n        -s               : enable spoof mode - schema 'main' is replaced by user's schema.");
            Console.Write("\r\n        -t               : create empty TS spoof tables and populate with simple test data.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     SdUpdateTown fcsa HULME1_0 test_combo0");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with qty. 3 command-line arguments.");
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
                    case "S":
                        Info.SpoofModeIsOff = false;
                        break;
                    case "C":
                        mIsCreateSpoofTables = true;
                        break;
                    case "P":
                        mIsValidIfAlreadyPosted = true;
                        break;
                    case "T":
                        mIsCreateSpoofTables = true;
                        mIsWriteSpoofTestData = true;
                        break;
                    default:
                        Console.Write("\r\n Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                        break;
                }

            }

            // Parse the regular arguments; there must be qty. 3 of them.
            if (regularArgs.Count != 3)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Parse the args.
            Info.DbName = regularArgs[0];
            Info.ProjectCode = regularArgs[1];
            Info.SdfName = regularArgs[2];

            //...Log2.v("\nSdUpdateTown:" + Info.ToString());
        }

        /// <summary>
        /// This method writes the values of a set of 'Action' counters to Console.Out, comprising
        /// the individual {Processed, Added, Updated, Deleted, NoAction} counts. 
        /// </summary>
        /// <param name="recCounts"></param>
        private static void WriteResultsToConsole(int[] recCounts)
        {
            Console.Write("\r\n\r\n\t************ Summary Information for SDB Update ************\r\n");
            Console.Write("\t***** SDF Name: {0,-16}", Info.SdfName);
            Console.Write(" User Name: {0,-10} *****\r\n", Info.MicsUserName);

            Console.Write("\r\n\r\n\t Records Processed: {0}", recCounts[(int)Action.Processed]);
            Console.Write("\r\n\t     Records Added: {0}", recCounts[(int)Action.Added]);
            Console.Write("\r\n\t   Records Updated: {0}", recCounts[(int)Action.Updated]);
            Console.Write("\r\n\t   Records Deleted: {0}", recCounts[(int)Action.Deleted]);
            Console.Write("\r\n\t Records No Action: {0}", recCounts[(int)Action.NoAction]);
            Console.Write("\r\n");

            // If writing to the SDB has been prohibited by the user then write to Console all
            // of the SQL queries that were inhibited.
            if (!Info.MdbWriteEnabled)
            {
                Console.Write("\r\n");
                Console.Write("\r\n[-d] command-line option: INHIBITED SQL QUERIES:");
                Console.Write("\r\n================================================");
                Console.Write("\r\n");
                Console.Write(Info.Text);
                Console.Write("\r\n");
            }
        }


    }
}




