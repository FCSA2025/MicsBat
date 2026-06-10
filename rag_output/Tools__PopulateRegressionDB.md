# Documented File: PopulateRegressionDB.cs
**Repository Path:** `Tools\PopulateRegressionDB.cs`
**Primary Layer:** `Tools`
**Namespace:** `Tools`

## Source Code Representation
```csharp
﻿using _Configuration;
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
/// ODBC.PopulateRegressionDB() using the SQL query prescribed in a string provided as a 
/// command-line argument; alternatively, the query can be read from a text file whose
/// path is prescribed by a command line argument.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - SqlExecDirect.png" ""
/// </remarks>
namespace Tools
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
    /// This class provides the Main() method for the PopulateRegressionDB application.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class PopulateRegressionDB
    {
        //private static string mQuery = "";

        const int COULD_NOT_SET_SQL_COMMIT_MODE_TO_MANUAL = 1;
        const int ODBC_SQLEXECDIRECT_FAILED = 2;


        /// <summary>
        /// This is the Main() method for the MICS program PopulateRegressionDB and
        /// provides high-level functionality and control.
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        public static void Go(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if true
                string mLog2FilePath = @"d:\MicsBatchLogs\PopulateRegressionDB.log";
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

                FileStream fileStream = File.OpenRead(Info.InFilePath);
                StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8, true);

                if (fileStream == null) Log2.v("\nfileStream is NULL.");
                if (streamReader == null) Log2.v("\nstreamReader is NULL.");

                String line;
                int maxGoBlockChars = 0;
                StringBuilder sb = new StringBuilder();
                List<string> goBlocks = new List<string>();

                while (!streamReader.EndOfStream)
                {
                    line = streamReader.ReadLine().Trim();

                    if (line == null) Log2.e("\nERROR: line is NULL.");

                    // If string is blank proceed to the next line.
                    if (String.IsNullOrWhiteSpace(line)) continue;

                    // If the line starts with a "--" comment designator then discard the line.
                    else if ((line.Length >= 2) && (line.Substring(0, 2) == "--")) continue;

                    // If line contains only "GO" then complete the current GoBlock and
                    // begin filling a new GoBlock string.
                    else if (line.ToUpper() == "GO")
                    {
                        sb.Append(line);
                        if (sb.Length > maxGoBlockChars) maxGoBlockChars = sb.Length;
                        goBlocks.Add(sb.ToString());
                        sb = new StringBuilder();
                    }

                    // If we have now reached EOF and line is not "GO" then add one.
                    else if (streamReader.EndOfStream)
                    {
                        sb.Append(line);
                        sb.Append("\r\nGO");
                        if (sb.Length > maxGoBlockChars) maxGoBlockChars = sb.Length;
                        goBlocks.Add(sb.ToString());
                    }

                    else // Accumulate the line in the current GoBlock.
                    {
                        sb.Append(line);
                        sb.Append("\r\n");
                    }
                }

                Log2.v("Max GoBlock length is {0}, characters.", maxGoBlockChars);
                Log2.v("\nNumber of GoBlocks is {0}\n", goBlocks.Count);

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                GetEnvVariablesForUtConnect();

                //	Grab write access to the database, if possible.  If not, exit.
                if ((rc = Qutils.EnterQueue(Info.DbName, "WRITE", 30)) != Constant.SUCCESS)
                {
                    Log2.e("\nPopulateRegressionDB.Main(): ERROR: call to Qutils.EnterQueue() failed, rc = " + rc);
                    Qutils.ExplainQueue(Info.DbName, "WRITE", rc, null);
                    Application.ExitQuietly(100);
                }

                // Establish an FCSA user session with the database.
                // Note: UtConnect() sets Info.GlobalSchema.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    //  Can't connect to database 
                    Log2.v("\r\nPopulateRegressionDB: Could not connect to database {0}\r\n{1}\r\n",
                                        Info.DbName, GenUtil.GetUserMess());

                    Application.ExitQuietly(101);
                }

                // Set the operating state of the classes that provide high-level read/write
                // methods that access TS MDB tables. 
                DynMdbAntenna.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMdbChannel.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMdbSite.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);

                // Set the operating state of the classes that provide high-level read/write
                // methods that access the DB adm.audit_table table.
                DynAuditTrail.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);

                SQLHDBC hConn = Ssutil.NewConn();
                SQLHANDLE hStmt;

                //// Change to Manual Commit mode.  This will commit everything up to here
                //// and not commit anything until we explicitly commit it or roll it back.
                //rc = Ssutil.SetCommitMode(hConn, Enums.COMMIT.MANUAL);

                //if (rc != Constant.SUCCESS)
                //{
                //    //	Could not set SQL commit mode to manual.
                //    Log2.e("\nMeUpdate.MeDoEsUpdate(): ERROR: could not set the database connection to manual commit.");
                //    Ssutil.DbGetDiag(ODBC.SQL_HANDLE_DBC, hConn);
                //    Console.Write("\r\nMeDoTsUpdate: Could not set the database connection to manual commit.\r\n{0}\r\n",
                //                    GenUtil.GetUserMess());
                //    Application.ExitQuietly(18965);
                //}

                bool allGoBlocksOK = false;

                int index = 0;
                string query = "";
                foreach (string goBlock in goBlocks)
                {
                    Console.Write("\rGoBlock# {0,8} of {1}", index, goBlocks.Count);

                    Log2.v("\nGoBlock# {0}; length = {1} ======================================================================", index, goBlock.Length);
                    Log2.v("\n{0}", goBlock);

                    query = goBlock;

                    // Remove all the single "GO" line.
                    query = query.Replace("\r\nGO", "");
                    // Replace line feeds by a single space.
                    query = query.Replace("\r\n", " ");

                    SQLRETURN sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
                    if (!ODBC.IsOK(sqlRet))
                    {
                        Log2.e("\r\nSsutil.DbCountRows(): ERROR: call to SQLAllocHandle() failed, sqlRet =  " + sqlRet);
                        Application.ExitQuietly(Error.ODBC_SQLALLOCHANDLE_FAILED);
                    }

                    bool thisGoBlockOK = DoQuery(hStmt, query);

                    allGoBlocksOK &= thisGoBlockOK;

                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

                    if (thisGoBlockOK)
                    {
                        Log2.v("\nDoQuery() SUCCEEDED.");
                    }
                    else
                    {
                        Log2.v("\nDoQuery() FAILED.");
                        Console.Write("\n\nGoBlock# {0} query FAILED.\n");
                    }

                    index++;
                }



                ///* Decide to commit or rollback, then return */
                //if (allGoBlocksOK)
                //{
                //    Ssutil.DbCommit(hConn);
                //    Log2.v("\nCOMMIT to all transactions.");
                //}
                //else
                //{
                //    Ssutil.DbRollBack(hConn);
                //    Log2.v("\nROLLBACK all transactions.");
                //}

                // Restore Commit mode to AUTO.
                Ssutil.SetCommitMode(hConn, Enums.COMMIT.AUTO);


                // Release the hold on the database a.s.a.p.
                Qutils.ExitQueue(Info.DbName, "WRITE");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(0);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nPopulateRegressionDB.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nPopulateRegressionDB.Main(): stack trace: \r\n\r\n" + e.StackTrace);
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
            Log2.v("\r\n");
            Log2.v("\r\n This program establishes an ODBC connection with a prescribed database and then executes  +");
            Log2.v("\r\n the SQL query contained within a prescribed text file. The maximum length of an SQL query +");
            Log2.v("\r\n is 65,000 characters. If the text file is larger than this limit then the SQL query is    +");
            Log2.v("\r\n parsed into discrete sub-queries delimited by the 'GO' command and passed, in sequence to +");
            Log2.v("\r\n ODBC.SQLExecDirect() for execution on the SQL Server.                                 ");
            Log2.v("\r\n");
            Log2.v("\r\n USAGE: PopulateRegressionDB  <dbName> <filePath>");
            Log2.v("\r\n ===== ");
            Log2.v("\r\n");
            Log2.v("\r\n        dbName           : database name, e.g. 'micsdev'.");
            Log2.v("\r\n        filePath         : fully-qualified path to text file.");
            Log2.v("\r\n");
            Log2.v("\r\n <...>  indicates a mandatory argument.\r\n");
            Log2.v("\r\n [...]  indicates an optional argument.");
            Log2.v("\r\n");
            Log2.v("\r\n e.g.");
            Log2.v("\r\n     PopulateRegressionDB  regression  d:\\users\\ahulme\\temp\\regression.sql");
            Log2.v("\r\n     ");
            Log2.v("\r\n Notes:");
            Log2.v("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
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
                    Log2.v("\r\n Invalid flag: '{0}'", arg);
                    WriteUsageToConsole();
                    Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                }

                // Process the flags.
                string flag = Strings.DropFirstChar(arg).ToUpper();
                switch (flag)
                {
                    default:
                        Log2.v("\r\n ERROR: Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                        break;
                }
            }

            // Parse the regular arguments; there must be qty. 2 of them.
            if (regularArgs.Count != 2)
            {
                Log2.v("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Constant.FAILURE);
            }

            // Parse the mandated args.
            Info.DbName = regularArgs[0];
            Info.InFilePath = regularArgs[1];

            // Check that text file exists.
            if (!File.Exists(Info.InFilePath))
            {
                Log2.v("\n\nERROR: file does not exist: {0}", Info.InFilePath);
                WriteUsageToConsole();
                Application.ExitQuietly(Constant.FAILURE);
            }

            Log2.v("\nPopulateRegressionDB.ParseCommandLineArgs(): Info.DbName      = " + Info.DbName);
            Log2.v("\nPopulateRegressionDB.ParseCommandLineArgs(): Info.InFilePath  = " + Info.InFilePath);
        }

        /// <summary>
        /// This method provides the core functionality of the PopulateRegressionDB application
        /// </summary>
        /// <param name="query"> the SQL query that it to be executed.</param>
        /// <returns></returns>
        private static int DoPopulateRegressionDB(string query)
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
                Log2.e("\nPopulateRegressionDB.MtDoEsUpdate(): ERROR: could not set the database connection to manual commit.");
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_DBC, hConn);
                Log2.v("\r\nMtDoTsUpdate: Could not set the database connection to manual commit.\r\n{0}\r\n",
                                GenUtil.GetUserMess());
                return COULD_NOT_SET_SQL_COMMIT_MODE_TO_MANUAL;
            }

            // Attempt to execute the SQL query (or queries).
            sqlRet = ODBC.SQLExecDirect(hStmt, query, query.Length);

            if (!ODBC.IsOKorNoData(sqlRet))
            {
                status = ODBC_SQLEXECDIRECT_FAILED;
                //...Log2.v("\nPopulateRegressionDB.DoPopulateRegressionDB(): call failed: SQL diagnostics:\n" + ODBC.GetDiagnostics(hStmt, query));
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

        /// <summary>
        /// This method encapsulates the submission of a query to the ODBC / SQL Server 
        /// without having to specify the length of the query string.
        /// </summary>
        /// <param name="hStmt"> - an open ODBC statement handle</param>
        /// <param name="query"> - SQL query to be submitted.</param>
        /// <returns></returns>
        public static bool DoQuery(SQLHANDLE hStmt, string query)
        {
            bool result = false;

            SQLRETURN sqlRet = 0;

            sqlRet = ODBC.SQLExecDirect(hStmt, query, query.Length);

            if (ODBC.IsOK(sqlRet))
            {
                result = true;
            }
            else
            {
                Log2.e("\n" + ODBC.GetDiagnostics(hStmt, query));
                result = false;
            }

            return result;
        }




    }
}



```
