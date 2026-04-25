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
/// This program establishes an ODBC connection with a prescribed database and then calls
/// ODBC.SQLExecDirect() using the SQL query prescribed in a string provided as a 
/// command-line argument; alternatively, the query can be read from a text file whose
/// path is prescribed by a command line argument.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - SqlExecDirect.png" ""
/// </remarks>
namespace SQLExecDirect
{
    using SQLLEN = Int64;
    using SQLHDBC = IntPtr;
    using SQLRETURN = Int16;
    using SQLUINTEGER = UInt32;
    using SQLPOINTER = IntPtr;
    using SQLHANDLE = IntPtr;
    using System.Runtime.InteropServices;
    using System.IO;

    /// <summary>
    /// This class provides the Main() method for the SQLExecDirect application that
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class SQLExecDirect
    {
        private static string mQuery = "";
        private static string mFilePath = "";

        const int COULD_NOT_SET_SQL_COMMIT_MODE_TO_MANUAL = 1;
        const int ODBC_SQLEXECDIRECT_FAILED = 2;


        /// <summary>
        /// This is the Main() method for the MICS program SQLExecDirect and
        /// provides high-level functionality and control.
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\SQLExecDirect.log";
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

                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                GetEnvVariablesForUtConnect();

                //	Grab write access to the database, if possible.  If not, exit.
                if ((rc = Qutils.EnterQueue(Info.DbName, "WRITE", 30)) != Constant.SUCCESS)
                {
                    Log2.e("\nSQLExecDirect.Main(): ERROR: call to Qutils.EnterQueue() failed, rc = " + rc);
                    Qutils.ExplainQueue(Info.DbName, "WRITE", rc, null);
                    Application.ExitQuietly(100);
                }

                // Establish an FCSA user session with the database.
                // Note: UtConnect() sets Info.GlobalSchema.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    //  Can't connect to database 
                    Console.Write("\r\nSQLExecDirect: Could not connect to database {0}\r\n{1}\r\n",
                                        Info.DbName, GenUtil.GetUserMess());

                    Application.ExitQuietly(101);
                }

                // Initialize the billing.
                BiUtil.BiBillingRec("SQLEXEC", "a Mics# program");

                //...Log2.v("\nSQLExecDirect.Main(): Info:\n" + Info.ToString());

                // Set the operating state of the classes that provide high-level read/write
                // methods that access TS MDB tables. 
                DynMdbAntenna.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMdbChannel.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMdbSite.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);

                // Set the operating state of the classes that provide high-level read/write
                // methods that access the DB adm.audit_table table.
                DynAuditTrail.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);

                // =================================================================
                // This method provides the core functionality for SQLExecDirect.
                // =================================================================
                int exitCode = DoSQLExecDirect(mQuery);

                // Release the hold on the database a.s.a.p.
                Qutils.ExitQueue(Info.DbName, "WRITE");

                // Write the SQL DELETE query strings to the Console.
                string message = "";
                if (exitCode == Constant.SUCCESS)
                {
                    message = String.Format("\r\n    SUCCESS  :  {0}", mQuery);
                }
                else
                {
                    message = String.Format("\r\n    FAILED==>:  {0}", mQuery);
                }

                Console.Write(message);

                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(exitCode);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nSQLExecDirect.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nSQLExecDirect.Main(): stack trace: \r\n\r\n" + e.StackTrace);
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
            Console.Write("\r\n This program establishes an ODBC connection with a prescribed database and then calls +");
            Console.Write("\r\n ODBC.SQLExecDirect() using the SQL query prescribed in a string provided as a         +");
            Console.Write("\r\n command-line argument; alternatively, the query can be read from a text file whose    +");
            Console.Write("\r\n path is prescribed by a command line argument.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: SQLExecDirect  <dbName> <projectCode> <mode> <arg>");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        projectCode      : user's project billing code.");
            Console.Write("\r\n        mode             : either 'query' or 'file'.");
            Console.Write("\r\n        arg              : if mode = 'query' then arg should be a string providing a complete SQL query.");
            Console.Write("\r\n        arg              : if mode = 'file'  then arg should be the path to a file containing SQL queries.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     SQLExecDirect  fcsa  hulme1_0  query \"SELECT * FROM foo.bar\"  ");
            Console.Write("\r\n     ");
            Console.Write("\r\n Notes:");
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

                // Process the flags.
                string flag = Strings.DropFirstChar(arg).ToUpper();
                switch (flag)
                {
                    case "D":
                        break;
                    default:
                        Console.Write("\r\n ERROR: Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                        break;
                }
            }

            // Parse the regular arguments; there must be qty. 4 of them.
            if (regularArgs.Count != 4)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Constant.FAILURE);
            }

            // Parse the mandated args.
            Info.DbName = regularArgs[0];
            Info.ProjectCode = regularArgs[1];

            string mode = regularArgs[2].ToLower().Trim();
            switch (mode)
            {
                case "query":
                    mQuery = regularArgs[3];
                    break;
                case "file":
                    mFilePath = regularArgs[3];

                    try
                    {
                        mQuery = File.ReadAllText(mFilePath);
                    }
                    catch (Exception e)
                    {
                        Console.Write("\r\n ERROR: could not read the file: {0}", e.Message);
                        Log2.e("\n\nSQLExecDirect.ParseCommandLineArgs(): ERROR: File.ReadAllText(): exception: " + e.Message);
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                    }

                    break; 
                default:
                    Console.Write("\r\n ERROR: Invalid mode: {0}\r\n", regularArgs[2]);
                    WriteUsageToConsole();
                    Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                    break;
            }

            //...Log2.v("\nSQLExecDirect.ParseCommandLineArgs(): Info.DbName      = " + Info.DbName);
            //...Log2.v("\nSQLExecDirect.ParseCommandLineArgs(): Info.ProjectCode = " + Info.ProjectCode);
            //...Log2.v("\nSQLExecDirect.ParseCommandLineArgs(): mode             = " + mode);
            //...Log2.v("\nSQLExecDirect.ParseCommandLineArgs(): mQuery           = \n" + mQuery);
        }

        /// <summary>
        /// This method provides the core functionality of the SQLExecDirect application
        /// </summary>
        /// <param name="query"> the SQL query that it to be executed.</param>
        /// <returns></returns>
        private static int DoSQLExecDirect(string query)
        {
            // Local variables 
            int status = Constant.SUCCESS;
            int rc;
            SQLRETURN sqlRet = 0;

            // Begin transaction for updates; get an ODBC connection and statement handles.
            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            // Change to Manual Commit mode.  This will commit everything up to here
            // and not commit anything until we explicitly commit it or roll it back.
            rc = Ssutil.SetCommitMode(hConn, Enums.COMMIT.MANUAL);

            if (rc != Constant.SUCCESS)
            {
                //	Could not set SQL commit mode to manual.
                Log2.e("\nSQLExecDirect.MtDoEsUpdate(): ERROR: could not set the database connection to manual commit.");
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_DBC, hConn);
                Console.Write("\r\nMtDoTsUpdate: Could not set the database connection to manual commit.\r\n{0}\r\n",
                                GenUtil.GetUserMess());
                return COULD_NOT_SET_SQL_COMMIT_MODE_TO_MANUAL;
            }

            // Attempt to execute the SQL query (or queries).
            sqlRet = ODBC.SQLExecDirect(hStmt, query, query.Length);

            if (!ODBC.IsOKorNoData(sqlRet))
            {
                status = ODBC_SQLEXECDIRECT_FAILED;
                //...Log2.v("\nSQLExecDirect.DoSQLExecDirect(): call failed: SQL diagnostics:\n" + ODBC.GetDiagnostics(hStmt, query));
            }

            // Decide to commit or rollback, then return 
            if (status == Constant.SUCCESS)
            {
                Ssutil.DbCommit(hConn);

                Ssutil.DisConn(hConn);
            }
            else
            {
                Ssutil.DbRollBack(hConn);

                Ssutil.DisConnStmt(hConn, hStmt);
            }

            // Restore Commit mode to AUTO.
            Ssutil.SetCommitMode(hConn, Enums.COMMIT.AUTO);

            return status;
        }




    }
}


