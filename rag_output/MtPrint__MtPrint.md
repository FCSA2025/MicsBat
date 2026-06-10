# Documented File: MtPrint.cs
**Repository Path:** `MtPrint\MtPrint.cs`
**Primary Layer:** `MtPrint`
**Namespace:** `MtPrint`

## Source Code Representation
```csharp
﻿using _Configuration;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This application fetches hierachical site-ante-chan data from the MDB TS tables 
/// for a prescribed TS site and writes it to Console.Out using the same format 
/// as PDF import files. As an option, site-ante-chan for a remote site can also
/// be included.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - MtPrint.PNG" ""
/// </remarks>
namespace MtPrint
{
    /// <summary>
    /// This class provides the Main() method for the MtPrint application
    /// together with supporting methods that parse the command line arguments.
    /// </summary>
    public class MtPrint
    {
        private const string COMMENT_LINE = "*===========================================================================\r\n";
        private const int USER_SESSION = 1;
        private static string mSiteCall1 = "";
        private static string mSiteCall2 = "";

        /// <summary>
        /// This is the Main() method for the MtPrint application that provides
        /// all of the top-level control.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\MtPrint.log";
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
                string printLine = "";
                int rc;
                int nRet;

                // Parse the command line arguments and set values in Info static class.
                ParseCommandLineArgs(args);

                // UtConnect() expects to receive the user's MICS ID and password
                // from the static class Info. Parse the environmental variables
                // to get and set the Info fields for MICS ID and password.
                GetEnvVariables();

                if ((nRet = Qutils.EnterQueue(Info.DbName, "READ", 30)) != 0)
                {
                    Qutils.ExplainQueue(Info.DbName, "READ", nRet, null);
                    Application.Exit(100);
                }

                // Start a MICS user session (connects to the database).
                rc = Ssutil.UtConnect(Info.DbName, USER_SESSION);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    ErrMsg.UtPrintMessage(Error.NODATABASE, Info.DbName);
                    Application.Exit(rc);
                }

                BiUtil.BiBillingRec("MTPRINT", args[2]);

                Console.Write("{0}{1} TS SITE CALL SIGN: {2}, Produced by MtPrint build {3}\r\n{0}",
                    COMMENT_LINE, Constant.COMMENT_CHAR, mSiteCall1, Info.GetDateTimeNow());

                string where = String.Format(" call1 = '{0}' ", mSiteCall1);
                if ((DynMdbSite.DbCountRows(where) != 1))
                {
                    Console.Write("\n\nERROR: MDB table main.mt_site has no site whose call sign is '{0}'\n", mSiteCall1);
                }
                else

                {
                    // Process the local site call1.
                    printLine = "";

                    /* main loop to print flat PDF lines */
                    while ((rc = MtPrintUtils.MtPrintFw(mSiteCall1, ref printLine)) == Constant.SUCCESS)
                    {
                        Console.Write("{0}\r\n", printLine);

                        printLine = "";
                    }
                    /* an error occured so print it */
                    if (rc != Constant.NOMORERECS)
                    {
                        ErrMsg.UtPrintMessage(rc);
                    }
                    else
                    {
                        rc = Constant.SUCCESS;
                    }

                    // This closes all the DynMtSite/Ante/Azim/Chan handles and forces re-initialization.
                    printLine = null;
                    MtPrintUtils.MtPrintFw(null, ref printLine);

                    // Process the remote site call2 if it is provided.
                    if (!String.IsNullOrWhiteSpace(mSiteCall2))
                    {
                        printLine = "";

                        /* main loop to print flat PDF lines */
                        while ((rc = MtPrintUtils.MtPrintFw(mSiteCall2, ref printLine)) == Constant.SUCCESS)
                        {
                            if (String.IsNullOrWhiteSpace(printLine)) printLine = "*";

                            Console.Write("{0}\r\n", printLine);

                            printLine = "";
                        }
                        /* an error occured so print it */
                        if (rc != Constant.NOMORERECS)
                        {
                            ErrMsg.UtPrintMessage(rc);
                        }
                        else
                        {
                            rc = Constant.SUCCESS;
                        }

                        // This closes all the DynMtSite/Ante/Azim/Chan handles.
                        printLine = null;
                        MtPrintUtils.MtPrintFw(null, ref printLine);
                    }
                }

                //Console.Write("{0}", COMMENT_LINE);

                BiUtil.BiBillingRec(Constant.BI_END, "");

                Ssutil.UtDisconnect(USER_SESSION);

                Qutils.ExitQueue(Info.DbName, "READ");

                Application.ExitQuietly(rc);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nMtPrint.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nMtPrint.Main(): stack trace: \r\n\r\n" + e.StackTrace);
                BiUtil.BiBillingRec(Constant.BI_END, "");
                Ssutil.UtDisconnect(1);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }


        }

        /// <summary>
        /// This methods gets the values of Windows environment variables that
        /// are required to execute FeImport; specifically these are 'MICSUSER'
        /// and 'PASSWORD' that are required for a successful call to UtConnect().
        /// </summary>
        public static void GetEnvVariables()
        {
            // Get the user's MICS ID from the environment.
            Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                Log2.e("\nFeImport.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
                Application.Exit(Error.ENVVARMICSUSERNOTSET);
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
        /// succinct summary of mandatory and optional arguments when FeImport
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n For a prescribed site call sign, this program extracts the full   +");
            Console.Write("\r\n hierachical site-ante-chan data from the MDB TS tables and writes +");
            Console.Write("\r\n it to the console as an TS PDF.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: MtPrint <dbname> <projectCode> <call1> [call2]");
            Console.Write("\r\n =====");
            Console.Write("\r\n");
            Console.Write("\r\n        dbname          : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        projectCode     : project code, e.g. 'hulme1_34'");
            Console.Write("\r\n        call1           : local site's call sign.");
            Console.Write("\r\n        call2           : remote site's call sign.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     MtPrint fcsa hulme1_0 CTX304");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called at least qty. 3 command-line arguments.");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the command line arguments and uses their values to set
        /// various associated internal variables and flags.
        /// </summary>
        /// <param name="args"> - command line arguments.</param>
        public static void ParseCommandLineArgs(string[] args)
        {
            List<string> argsWithNoFlags = new List<string>();
            List<string> importFileList = new List<string>();

            // Too few command line args?
            if (args.Length < 3)
            {
                WriteUsageToConsole();
                Environment.Exit(Error.COMMAND_LINE_ERROR);
            }

            // Parse for, and strip out, options prefixed by '-' that can occur 
            // anywhere in the command line argument list.
            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    switch (arg.Trim().ToUpper())
                    {
                        default:
                            Log2.e("\nFeImport.Main(): ERROR: invalid option flag: " + arg);
                            WriteUsageToConsole();
                            Environment.Exit(Error.COMMAND_LINE_ERROR);
                            break;
                    }
                }
                else
                {
                    argsWithNoFlags.Add(arg);
                }
            }

            if (argsWithNoFlags.Count == 3)
            {
                Info.DbName = argsWithNoFlags[0];
                Info.ProjectCode = argsWithNoFlags[1];
                mSiteCall1 = argsWithNoFlags[2];
            }
            else if (argsWithNoFlags.Count == 4)
            {
                Info.DbName = argsWithNoFlags[0];
                Info.ProjectCode = argsWithNoFlags[1];
                mSiteCall1 = argsWithNoFlags[2];
                mSiteCall2 = argsWithNoFlags[3];
            }
            else
            {
                // Too many or too few command line arguments.
                WriteUsageToConsole();
                Environment.Exit(Error.COMMAND_LINE_ERROR);
            }

        }




    }
}


```
