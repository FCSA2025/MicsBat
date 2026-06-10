# Documented File: MtDeleteSiteTree.cs
**Repository Path:** `MtDeleteSiteAnteChan\MtDeleteSiteTree.cs`
**Primary Layer:** `MtDeleteSiteAnteChan`
**Namespace:** `MtDeleteSiteTree`

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
/// This program deletes an entire site-ante-chan record tree from the MDB for a prescribed TS callsign.");
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - MtDeleteSiteTree.PNG" ""
/// </remarks>
namespace MtDeleteSiteTree
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
    /// This class provides the Main() method for the MtDeleteSiteTree application.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class MtDeleteSiteTree
    {
        private static string mCallsign = "";

        const int COULD_NOT_SET_SQL_COMMIT_MODE_TO_MANUAL = 1;
        const int NO_MDB_SITE_WITH_CALL1 = 2;
        const int MDB_TS_SITE_DELETION_FAILED = 3;
        const int MDB_TS_ANTE_DELETION_FAILED = 4;
        const int MDB_TS_CHAN_DELETION_FAILED = 5;

        /// <summary>
        /// This is the Main() method for the MICS program MtDeleteSiteTree and
        /// provides high-level functionality and control.
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\MtDeleteSiteTree.log";
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
                if ((rc = Qutils.EnterQueue(Info.DbName, "WRITE", 30)) != Constant.SUCCESS)
                {
                    Log2.e("\nMtDeleteSiteTree.Main(): ERROR: call to Qutils.EnterQueue() failed, rc = " + rc);
                    Qutils.ExplainQueue(Info.DbName, "WRITE", rc, null);
                    Application.ExitQuietly(100);
                }

                // Establish an FCSA user session with the database.
                // Note: UtConnect() sets Info.GlobalSchema.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    //  Can't connect to database 
                    Console.Write("\r\nMtDeleteSiteTree: Could not connect to database {0}\r\n{1}\r\n",
                                        Info.DbName, GenUtil.GetUserMess());

                    Application.ExitQuietly(101);
                }

                // Initialize the billing.
                BiUtil.BiBillingRec("MTDELTREE", mCallsign);

                //...Log2.v("\nMtDeleteSiteTree.Main(): Info:\n" + Info.ToString());

                // Set the operating state of the classes that provide high-level read/write
                // methods that access TS MDB tables. 
                DynMdbAntenna.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMdbChannel.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMdbSite.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);

                // Set the operating state of the classes that provide high-level read/write
                // methods that access the DB adm.audit_table table.
                DynAuditTrail.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);

                // =================================================================
                // This method provides the core functionality for MtDeleteSiteTree.
                // =================================================================
                int exitCode = DoTsSiteTreeDeletes(mCallsign);

                // Release the hold on the database a.s.a.p.
                Qutils.ExitQueue(Info.DbName, "WRITE");

                // Write the SQL DELETE query strings to the Console.
                string title = "";
                if (Info.MdbWriteEnabled)
                {
                    title = String.Format("\r\nCommitted: SQL QUERIES:");

                }
                else
                {
                    title = String.Format("\r\n[-d] command-line option: INHIBITED SQL QUERIES:");
                }

                Console.Write("\r\n");
                Console.Write(title);
                Console.Write("\r\n================================================");
                Console.Write("\r\n");
                Console.Write(Info.Text);
                Console.Write("\r\n");

                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(exitCode);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nMtDeleteSiteTree.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nMtDeleteSiteTree.Main(): stack trace: \r\n\r\n" + e.StackTrace);
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
            Console.Write("\r\n This program deletes an entire site-ante-chan record tree from the MDB +");
            Console.Write("\r\n for a prescribed TS callsign.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: MtDeleteSiteTree  <dbName> <projectCode> <call1> [-d] [-s]");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        projectCode      : user's project billing code.");
            Console.Write("\r\n        call1            : callsign of the TS site tree to be deleted.");
            Console.Write("\r\n");
            Console.Write("\r\n        --- For Testing -------------------------------------------------------------");
            Console.Write("\r\n        -d               : disable attempts to insert/update/delete MDB table records.");
            Console.Write("\r\n        -s               : enable spoof mode - schema 'main' is replaced by user's schema.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     MtDeleteSiteTree  fcsa  hulme1_0  CGJ846X  ");
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
                        Info.MdbWriteEnabled = false;
                        break;
                    case "S":
                        Info.SpoofModeIsOff = false;
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
                Application.ExitQuietly(Constant.FAILURE);
            }

            // Parse the args.
            Info.DbName = regularArgs[0];
            Info.ProjectCode = regularArgs[1];
            mCallsign = regularArgs[2];
        }

        /// <summary>
        /// This method provides the core functionality of the MtDeleteSiteTree application
        /// </summary>
        /// <param name="pdfName"></param>
        /// <param name="addCount"> - number of records added, site, antenna, channel.</param>
        /// <param name="delCount"> - number of records deleted, site, antenna, channel.</param>
        /// <param name="updCount"> - number of records updated, site, antenna, channel.</param>
        /// <param name="totCount"> - number of records processed, site, antenna, channel.</param>
        /// <param name="chngCount"> - number of records affected by Change Call (GK).</param>
        /// <returns></returns>
        private static int DoTsSiteTreeDeletes(string call1)
        {
            // Local variables 
            int status = Constant.SUCCESS;
            int rc;
            string query = "";
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
                Log2.e("\nMtDeleteSiteTree.MtDoEsUpdate(): ERROR: could not set the database connection to manual commit.");
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_DBC, hConn);
                Console.Write("\r\nMtDoTsUpdate: Could not set the database connection to manual commit.\r\n{0}\r\n",
                                GenUtil.GetUserMess());
                return COULD_NOT_SET_SQL_COMMIT_MODE_TO_MANUAL;
            }

            // Before we begin the site-ante-chan tree record deletes, check that the
            // TS site with the prescribed call sign actually exists in the MDB.
            if (!DynMdbSite.MdbRecordExists(call1))
            {
                //...Log2.v("\nMtDeleteSiteTree.DoTsSiteTreeDeletes(): no TS site exists in MDB for call1 = " + call1);
                status = NO_MDB_SITE_WITH_CALL1;
            }

            // Delete the MDB TS site.
            if (status == Constant.SUCCESS)
            {
                query = String.Format("DELETE FROM {0} WHERE call1='{1}'", DynMdbSite.TableName, call1);

                Info.Text += "\n\n" + query;

                if (Info.MdbWriteEnabled)
                {
                    sqlRet = ODBC.SQLExecDirect(hStmt, query, query.Length);

                    if (!ODBC.IsOK(sqlRet)) status = MDB_TS_SITE_DELETION_FAILED;
                }
            }

            // Delete the MDB TS antennas.
            if (status == Constant.SUCCESS)
            {
                query = String.Format("DELETE FROM {0} WHERE call1='{1}'", DynMdbAntenna.TableName, call1);

                Info.Text += "\n\n" + query;

                if (Info.MdbWriteEnabled)
                {
                    sqlRet = ODBC.SQLExecDirect(hStmt, query, query.Length);

                    if (!ODBC.IsOK(sqlRet)) status = MDB_TS_ANTE_DELETION_FAILED;
                }
            }

            // Delete the MDB TS channels.
            if (status == Constant.SUCCESS)
            {
                query = String.Format("DELETE FROM {0} WHERE call1='{1}'", DynMdbChannel.TableName, call1);

                Info.Text += "\n\n" + query;

                if (Info.MdbWriteEnabled)
                {
                    sqlRet = ODBC.SQLExecDirect(hStmt, query, query.Length);

                    if (!ODBC.IsOK(sqlRet)) status = MDB_TS_CHAN_DELETION_FAILED;
                }
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



```
