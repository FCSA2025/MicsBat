# Documented File: MePrint.cs
**Repository Path:** `MePrint\MePrint.cs`
**Primary Layer:** `MePrint`
**Namespace:** `MePrint`

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
/// This application fetches a data set from the database associated
/// with a previously imported ES PDF text file and writes it to Console.Out .
/// </summary>
/// <description>
/// Suppose that we have previously imported an ES PDF text file using the
/// command-line invocation:
/// <para>
/// <code>
/// feimport -D fcsa hulme1_0 esMyPDF "D:\Users\ahulme\esMyPDF.txt"
/// </code>
/// </para>
/// There will be qty. 8 tables in the database that were created by this import:
/// <list type="bullet">
/// <item>fcsa.hulme.fe_esMyPDF_ante</item>
/// <item>fcsa.hulme.fe_esMyPDF_azim</item>
/// <item>fcsa.hulme.fe_esMyPDF_ccal</item>
/// <item>fcsa.hulme.fe_esMyPDF_chan</item>
/// <item>fcsa.hulme.fe_esMyPDF_cloc</item>
/// <item>fcsa.hulme.fe_esMyPDF_shrl</item>
/// <item>fcsa.hulme.fe_esMyPDF_site</item>
/// <item>fcsa.hulme.fe_esMyPDF_titl</item>
/// </list>
/// There will also be a record associated with esMyPDF in the database
/// table <b>web.user_tables</b> that is used by WebMICS to determine whether the
/// tables associated with esMyPDF already exist, or not.
/// <para>
/// The MePrint application fetches all the data from these qty. 8 database tables. 
/// It arranges that data in the correct sequence (title -> changes -> site -> antenna -> azimuth -> channel).
/// Finally, it formats the data as strings written to Console.Out .
/// </para>
/// 
/// </description>
namespace MePrint
{
    /// <summary>
    /// This class provides the Main() method for the MePrint application
    /// together with supporting methods that parse the command line arguments.
    /// </summary>
    public class MePrint
    {
        private const string COMMENT_LINE = "*===========================================================================\r\n";
        private const int USER_SESSION = 1;
        private static string mSiteLocation = "";

        /// <summary>
        /// This is the Main() method for the MePrint application that provides
        /// all of the top-level control.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\MePrint.log";
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

                BiUtil.BiBillingRec("MEPRINT", args[2]);

                Console.Write("{0}{1} ES SITE LOCATION: {2}, Produced by MePrint build {3}\r\n{0}",
                    COMMENT_LINE, Constant.COMMENT_CHAR, mSiteLocation, Info.GetDateTimeNow());

                string where = String.Format(" location = '{0}' ", mSiteLocation);
                if ((Ssutil.DbCountRows("main.me_site", where) != 1))
                {
                    Console.Write("\n\nERROR: MDB table main.me_site has no site whose location is '{0}'\n", mSiteLocation);
                }
                else

                {
                    /* main loop to print flat PDF lines */
                    while ((rc = MePrintUtils.MePrintFw(mSiteLocation, ref printLine)) == Constant.SUCCESS)
                    {
                        Console.Write("{0}\r\n", printLine);
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

                    // This closes all the DynMeSite/Ante/Azim/Chan handles.
                    printLine = null;
                    MePrintUtils.MePrintFw(null, ref printLine);
                }

                //Console.Write("{0}", COMMENT_LINE);

                BiUtil.BiBillingRec(Constant.BI_END, "");

                Ssutil.UtDisconnect(USER_SESSION);

                Qutils.ExitQueue(Info.DbName, "READ");

                Application.ExitQuietly(rc);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nMePrint.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nMePrint.Main(): stack trace: \r\n\r\n" + e.StackTrace);
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
            Console.Write("\r\n For a prescribed site location, this program extracts the full   +");
            Console.Write("\r\n hierachical site-ante-azim-chan data from the MDB ES tables and  +");
            Console.Write("\r\n writes it to the console as an ES PDF.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: MePrint <dbname> <projectCode> <siteLocation>");
            Console.Write("\r\n =====");
            Console.Write("\r\n");
            Console.Write("\r\n        dbname          : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        projectCode     : project code, e.g. 'hulme1_34'");
            Console.Write("\r\n        siteLocation    : site's location code.");
            Console.Write("\r\n");
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

            if (argsWithNoFlags.Count != 3)
            {
                // Missing parameters.
                WriteUsageToConsole();
                Environment.Exit(Error.COMMAND_LINE_ERROR);
            }

            Info.DbName = argsWithNoFlags[0];
            Info.ProjectCode = argsWithNoFlags[1];
            mSiteLocation = argsWithNoFlags[2];
        }




    }
}

```
