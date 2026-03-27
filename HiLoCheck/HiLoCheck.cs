using _Auxlib;
using _Configuration;
using _DataStructures;
using _NewLib;
using _OHloss;
using _Utillib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This program checks for HiLo violations for the sites referenced in the prescribed
/// TS data file and also adjacent sites within a prescribed distance; it is called by
/// the WebMICS Auxiliary Engineering tool 'Check Band HiLo Frequencies'.
/// </summary>
/// <remarks>
/// Each RF channel requires two frequencies, a transmit frequency (Tx) and a receive
/// frequency (Rx). 
/// <list type="bullet>">
/// <item>All Tx frequencies are in one half of the band, and all Rx frequencies are in the other half.</item>
/// <item>Frequencies are normally assigned so that all frequencies transmitting from a site are either
/// in the upper half (Hi) or the lower half (Lo) of the band.</item>
/// </list>
/// This program checks for HiLo violations for the sites referenced in the prescribed
/// TS data file and also adjacent sites within a prescribed distance.
/// 
/// The command-line usage is:
/// \image html "Usage - HiLoCheck.PNG" "" 
/// </remarks>
namespace HiLoCheck
{
    /// <summary>
    /// This class provides the Main() method for the MICS HiLoCheck program.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class HiLoCheck
    {
        private static int mMinDistSecLat = 5; /* seconds of latitude -- about 154m. */
        private static double mMinDistKm;
        public static bool mIsVerbose = false;
        private static TextWriter mTW = Console.Out;

        /// <summary>
        /// This is the Main() method for the MICS program 
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:MicsBatchLogs\HiLoCheck.log";
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

                // UtConnect() expects to receive the user's MICS ID and password
                // from the static class Info. Parse the environmental variables
                // to get and set the Info fields for MICS ID and password.
                GetEnvVariablesForUtConnect();

                // Unlike most other MICS programs, the user's project charge code is not 
                // passed in through the command-line; instead we need to get it from the
                // WindowsShell environment variable MICS_PROJECT.
                string pc;
                GenUtil.GetProjectCode(out pc);
                Info.ProjectCode = pc;

                // Write build metadata and date-time to the output stream.
                string dateTime = Info.ToFormatA(DateTime.Now);
                mTW.Write("\r\nhilocheck program build {0} started at {1}\r\n", Info.BuildMetaData, dateTime);

                // Establish an FCSA user session with the database.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    Console.Write("\r\n*ERROR* Cannot connect to database: {0}, for reason {1}\r\n", Info.DbName, rc);
                    Application.ExitQuietly(98);
                }

                // Construct the string to be placed in the 'info' column in
                // the billing table.
                string infoBuffer = Info.PdfName;

                // Initialize the billing.
                BiUtil.BiBillingRec("HILOCHECK", infoBuffer);

                // Get the distance in Km.
                mMinDistKm = mMinDistSecLat * 0.03087;

                mTW.Write("\r\nProposed File: {0} {1} with min dist of {2} secs of latitude ({3:F2}Km).\r\n",
                                 Info.PdfName, (mIsVerbose ? "in verbose mode," : ""), mMinDistSecLat, mMinDistKm);

                FtTitl ftTitl;
                int nRet = FtUtils.FtGetTitle(out ftTitl, Info.PdfName);

                // Make a call to HiloCheckFunc(); this does all of the 'heavy lifting'
                // w.r.t. the HiLo analysis and creation of text output of the results.
                if (nRet == 0)
                {
                    if (!String.IsNullOrWhiteSpace(ftTitl.validated) && !ftTitl.validated.Equals("N"))
                    {
                        nRet = HiLoAnalysis2021.HiloCheckFunc(Info.PdfName, mIsVerbose, mMinDistKm, mTW);

                        mTW.Write("\r\n{0} Hilo Violation(s) encountered.\r\n", nRet);
                    }
                    else
                    {
                        //	This file hasn't been validated, hilocheck relies on the bandwords being set by validate.
                        mTW.Write("\r\nThis file has not been validated.  hilocheck relies on the bandwords in the sites\r\nbeing set by validate.  Run validate on this file first.\r\n");
                    }
                }

                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                //	Get the distance in Km.
                mMinDistKm = mMinDistSecLat * 0.03087;

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                // Flush/close the TextWriter stream.
                mTW.Close();

                Application.ExitQuietly(0);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nHiLoCheck.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nHiLoCheck.Main(): stack trace: \r\n\r\n" + e.StackTrace);
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
            Console.Write("\r\n This program checks for HiLo violations for the sites referenced in the prescribed");
            Console.Write("\r\n TS data file and also adjacent sites within a prescribed distance.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: HiLoCheck <dbName> <TSdataFileName> [/o:<outFilePath>] [/m:<minSecsLat>] [/v]");
            Console.Write("\r\n =====");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        TSdataFileName   : name of a validated TS data file.");
            Console.Write("\r\n        outFilePath      : full path to prescribed output file.");
            Console.Write("\r\n        minSecsLat       : minimum distance in integer seconds of latitude,");
            Console.Write("\r\n                           if absent, this defaults to 5 seconds (~154m).");
            Console.Write("\r\n        v                : verbose output.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     HiLoCheck  fcsa xci4qc0044v2");
            Console.Write("\r\n     ");
            Console.Write("\r\n     HiLoCheck  fcsa xci4qc0044v2 /v /m:2 /o:d:\\users\\ahulme\\HiLoCheckOut.txt");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with qty. 2, 3, 4 or 5 command-line arguments.");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the user-prescribed command-line arguments; a valid 
        /// command-line must have qty. 2, 3, 4 or 5 command-line arguments..
        /// </summary>
        /// <param name="args"></param>
        public static void ParseCommandLineArgs(string[] args)
        {
            if (!(args.Length == 2 || args.Length == 3 || args.Length == 4 || args.Length == 5))
            {
                WriteUsageToConsole();
                Application.ExitQuietly(97);
            }

            Info.DbName = args[0];

            Info.PdfName = args[1];

            for (int i = 2; i < args.Length; i++)
            {
                string arg = args[i].ToUpper();

                if (arg.Length <= 3)
                {
                    if (arg.Equals("/V"))
                    {
                        mIsVerbose = true;
                        continue;
                    }
                    else
                    {
                        Console.Write("\r\nInvalid optional argument: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(97);
                    }
                }

                string prefix = arg.Substring(0, 3);
                string outFilePath = arg.Substring(3);

                switch (prefix)
                {
                    case "/O:":
                        Info.OutFilePath = outFilePath;

                        try
                        {
                            mTW = new StreamWriter(outFilePath);
                        }
                        catch (Exception e)
                        {
                            Log2.e("\nHiLoCheck.ParseCommandLineArgs(): ERROR: Could not open output file: " + outFilePath + "\n" + e.Message);
                            Console.Write("\r\nCould not open output file: {0}\r\n", outFilePath);
                            WriteUsageToConsole();
                            Application.ExitQuietly(97);
                        }

                        break;
                    case "/M:":
                        try
                        {
                            string seconds = arg.Substring(3);

                            mMinDistSecLat = Convert.ToInt32(seconds);

                            if (mMinDistSecLat < 1)
                            {
                                Console.Write("\r\nInvalid optional argument: {0}\r\n", arg);
                                WriteUsageToConsole();
                                Application.ExitQuietly(97);
                            }
                        }
                        catch (Exception e)
                        {
                            Log2.e("\nHiLoCheck.ParseCommandLineArgs(): ERROR: Convert.ToInt32() threw an exception:\n" + e.Message);
                            Console.Write("\r\nString to integer conversion error: {0}\r\n", arg);
                            WriteUsageToConsole();
                            Application.ExitQuietly(97);
                        }
                        break;
                    default:
                        Console.Write("\r\nInvalid optional argument: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(97);
                        break;
                }

            } // i

            //...Log2.v("\nHiLoCheck.ParseCommandLineArgs(): Info.DbName:       " + Strings.AddBars(Info.DbName));
            //...Log2.v("\nHiLoCheck.ParseCommandLineArgs(): Info.PdfName:      " + Strings.AddBars(Info.PdfName));
            //...Log2.v("\nHiLoCheck.ParseCommandLineArgs(): Info.OutFilePath:  " + Strings.AddBars(Info.OutFilePath));
            //...Log2.v("\nHiLoCheck.ParseCommandLineArgs(): mMinDistSecLat:    " + mMinDistSecLat);
            //...Log2.v("\nHiLoCheck.ParseCommandLineArgs(): mIsVerbose:        " + mIsVerbose);
        }




    }
}


