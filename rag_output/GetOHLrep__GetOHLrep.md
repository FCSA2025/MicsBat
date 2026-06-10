# Documented File: GetOHLrep.cs
**Repository Path:** `GetOHLrep\GetOHLrep.cs`
**Primary Layer:** `GetOHLrep`
**Namespace:** `GetOHLrep`

## Source Code Representation
```csharp
﻿using _Auxlib;
using _Configuration;
using _DataStructures;
using _NewLib;
using _OHloss;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This MICS program is invoked
/// by the WebMICS Auxiliary Engineering tool for 'Over-Horizon Loss' (OHL) 
/// calculation; it inputs data for two TS stations (A and B) and outputs 
/// a report giving OHL losses (dBs) along the geodesic between A and B.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - GetOHLrep.PNG" ""
/// </remarks>
namespace GetOHLrep
{
    /// <summary>
    /// This class provides the Main() method for the MICS program that is invoked
    /// via the WebMICS Auxiliary Engineering tool for 'Over-Horizon Loss' (OHL) 
    /// calculation; GetOHLrep inputs data for two TS stations (A and B) and outputs 
    /// a report giving OHL losses (dBs) along the geodesic between A and B.
    /// </summary>
    /// <remarks>
    /// The actual OHL calculation is perfomed by calling the method <b>Calc_OhLoss()</b>
    /// provided by <b>_OHLoss.dll</b> which is a C# 'thin-wrapper' around the native code
    /// library <b>CTE.dll</b>
    ///
    /// The library <b>CTE.dll</b> encapsulates the 'over the horizon' telemetry calculation
    /// subroutine library provided by Contract Telecommunication Engineering Ltd. (CTE) 
    ///
    /// CTE provides FCSA with their 3rd-party
    /// library of functions that perform a repertoir of
    /// microwave transmission calculations. The latest version of this CTE library
    /// was provided to FCSA as a 64-bit static library compiled from
    /// 'C' code using Visual Studio (VS) 2010.
    /// 
    /// CTE does not provide the 'C' source code for their static library; also
    /// FCSA has upgraded to Visual Studio 2015 and a 'C' executable built with 
    /// VS-2015 cannot be linked with a library built using VS-2010.
    /// 
    /// Significant technical finesse was needed to successfully use CTE's
    /// static library. See the VS project <see cref="_OHloss"/> for the technical details.
    /// </remarks>
    class GetOHLrep
    {
        private static string mRepType;

        private static Enums.eRepType eRep;

        private static string mDir250K;
        private static string mDir50K;

        private static Parameters p = new Parameters();

        /// <summary>
        /// This is the Main() method for the MICS program GetOHLrep that is invoked
        /// via the WebMICS Auxiliary Engineering tool for 'Over-Horizon Loss' (OHL) 
        /// calculation. 
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\GetOHLrep.log";
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
                // Start a stop watch.
                Stopwatch clock = new Stopwatch();
                clock.Start();

                int rc = 0;

                OhLossXfer sctOHLoss;           /*  Structure for the OH Loss transfer        */

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

                // The paths to the map-data directories are prescribed in the Windows
                // environment variables FCSAMAPS50K and FCSAMAPS250K or set to
                // default values if they are undefined.
                GetEnvVariablesForMaps();

                // Prescribe the paths of the 50k and 250k OhLoss data libraries
                // and initializes the CTE library's internal state to be ready for subsequent
                // calls.
                CTEfunctions.Init_Directories(mDir250K, mDir50K);

                // Establish an FCSA user session with the database.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    Console.Write("GetOHLrep -- Can't connect to database {0}.\r\n", Info.DbName);

                    Application.ExitQuietly(11);
                }

                // Construct the string to be placed in the 'info' column in
                // the billing table.
                string infoBuffer = String.Format("{0}/{1} to {2}/{3} {4}",
                                                   p.mLat83A.Substring(0, 5), p.mLong83A.Substring(0, 6),
                                                   p.mLat83B.Substring(0, 5), p.mLong87B.Substring(0, 6),
                                                   mRepType);

                // Initialize the billing.
                BiUtil.BiBillingRec("GETOHLREP", infoBuffer);

                // Open the grid tables for the conversion of NAD 27 to NAD 83.
                AxNTv2Lib.AxOpenGrid();


                // We have (lat, long) values as character strings; we need to 
                // convert them to integers (centiseconds).
                p.nLata83 = LatLong.Get_Lat(ref p.mLat83A);
                p.nLonga83 = LatLong.Get_Long(ref p.mLong83A);
                p.nLatb83 = LatLong.Get_Lat(ref p.mLat83B);
                p.nLongb83 = LatLong.Get_Long(ref p.mLong87B);

                //	Calculate the distances and bearings between the two points A and B.
                AxSub2.AxDistan(0.01 * p.nLata83, 0.01 * p.nLatb83, 0.01 * p.nLonga83, 0.01 * p.nLongb83,
                                out p.fdist, out p.fbearinga, out p.fbearingb);

                // Prepare input variables for a subsequent call to the CTE ohloss routine.
                sctOHLoss = new OhLossXfer();

                sctOHLoss.lat_1 = LatLong.FsecsToDeg(LatLong.Get_Lat(ref p.mLat83A));
                sctOHLoss.lng_1 = LatLong.FsecsToDeg(LatLong.Get_Long(ref p.mLong83A));
                sctOHLoss.lat_2 = LatLong.FsecsToDeg(LatLong.Get_Lat(ref p.mLat83B));
                sctOHLoss.lng_2 = LatLong.FsecsToDeg(LatLong.Get_Long(ref p.mLong87B));

                if (p.mAnteHeightA <= 0.0)
                {
                    sctOHLoss.s1_anthght = 9.99;
                }
                else
                {
                    sctOHLoss.s1_anthght = p.mAnteHeightA;
                }

                if (p.mAnteHeightB <= 0.0)
                {
                    sctOHLoss.s2_anthght = 9.99;
                }
                else
                {
                    sctOHLoss.s2_anthght = p.mAnteHeightB;
                }

                sctOHLoss.freq = p.mFreqMhz;
                sctOHLoss.polarization = p.mPolarization;
                sctOHLoss.clim_region = p.mClimateZone;
                sctOHLoss.K_median = p.mK;

                // Now calculate the OH losses by calling the CTE subroutine.
                CTEfunctions.Calc_OhLoss(ref sctOHLoss);

                // Check the error status and act accordingly.
                if (sctOHLoss.error_status == Enums.PRF.OK)
                {

                    if (eRep == Enums.eRepType.eNOHTML || eRep == Enums.eRepType.eHTML)
                    {
                        /*	Produce the reports if the the table has been filled in.  They are
                        *		written to stdout, which dblogger has redirected to something it
                        *		can find. */
                        Reports.WritePlainOrHTML(eRep, p, sctOHLoss);
                    }
                    else if (eRep == Enums.eRepType.eCSV)
                    {
                        /*	Comma Separated Variable output. Write the header  */
                        Reports.WriteCSV(p, sctOHLoss);
                    }
                }
                else
                {
                    WriteErrorMessage(sctOHLoss);
                }

                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                /*	Close the grid file */
                AxNTv2Lib.AxCloseGrid();

                // Stop the timer.
                clock.Stop();
                Log2.e("\n\nElapsed time = {0}", clock.Elapsed);

                Application.ExitQuietly(0);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nGetOHLrep.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nGetOHLrep.Main(): stack trace: \r\n\r\n" + e.StackTrace);
                BiUtil.BiBillingRec(Constant.BI_END, "");
                Ssutil.UtDisconnect(1);
                AxNTv2Lib.AxCloseGrid();
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
        /// This methods gets the values of the Windows environment variables that
        /// are required to initialize the use of the _OHloss library; specifically 
        /// these are the variables 'FCSAMAPS50K' and 'FCSAMAPS250K' that
        /// can be used to override the defaults values for the paths of the 50k and 
        /// 250k OhLoss data libraries.
        /// </summary>
        public static void GetEnvVariablesForMaps()
        {
            string value;

            value = Environment.GetEnvironmentVariable("FCSAMAPS50K");
            if (String.IsNullOrWhiteSpace(value))
            {
                mDir50K = Constant.CMAPDIR50K_DEFAULT;
                //...Log2.v("\nGetOHLrep.GetEnvVariablesForMaps(): env. variable 'FCSAMAPS50K'  not set; using default: " + mDir50K);
            }
            else
            {
                mDir50K = value;
            }

            value = Environment.GetEnvironmentVariable("FCSAMAPS250K");
            if (String.IsNullOrWhiteSpace(value))
            {
                mDir250K = Constant.CMAPDIR250K_DEFAULT;
                //...Log2.v("\nGetOHLrep.GetEnvVariablesForMaps(): env. variable 'FCSAMAPS250K' not set; using default: " + mDir250K);

            }
            else
            {
                mDir250K = value;
            }
        }

        /// <summary>
        /// This method parses the (optional) call sign arguments; the user can prescribe
        /// the actual call sign or insert a hyphen '-' at that position that is 
        /// interpreted as an empty string.
        /// </summary>
        /// <param name="b"> - prescribed call sign argument to be parsed.</param>
        /// <returns> - either the call sign string or an empty string.</returns>
        private static string ParseCallSign(string b)
        {
            string a = "";

            if (b.Equals("-"))
            {
                a = "";
            }
            else
            {
                a = b;
            }

            return a;
        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when FtImport
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n GetOHLrep inputs data for two TS stations (A and B) and outputs ");
            Console.Write("\r\n a report giving OHL losses (dBs) along the geodesic between A and B.");
            Console.Write("\r\n GetOHLrep is called by the WebMICS Auxiliary Engineering tool for ");
            Console.Write("\r\n 'Over-Horizon Loss' (OHL) calculations.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: GetOHLrep <dbName> <repType> <lat83A> <lng83A> <lat83B> <lng83B>     +");
            Console.Write("\r\n =====            <heightA> <heightB> <k> <freqMHz> <pol> <clmReg>           +");
            Console.Write("\r\n                  [callA] [nameA] [callB] [nameB]                            +");
            Console.Write("\r\n                  [lat27A] [lng27A] [lat27B] [lng27B] ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        repType          : report type - 'HTML', 'NOHTML' or 'CSV'");
            Console.Write("\r\n        lat83A, lng83A   : NAD 83 (lat, long) of station A.");
            Console.Write("\r\n        lat83B, lng83B   : NAD 83 (lat, long) of station B.");
            Console.Write("\r\n        heightA, heightB : height of antenna at station A, B (m).");
            Console.Write("\r\n        k                : OHL model's k parameter value.");
            Console.Write("\r\n        freqMHz          : operating frequency (MHz).");
            Console.Write("\r\n        pol              : polarization (0 = horizontal, 1 = vertical).");
            Console.Write("\r\n        clmReg           : climactic region (0 = continental temperate,");
            Console.Write("\r\n                                             1 = maritime temperate overland,");
            Console.Write("\r\n                                             2 = maritime temperate oversea).");
            Console.Write("\r\n        callA, callB     : call sign of station A, B.");
            Console.Write("\r\n        nameA, nameB     : name      of station A, B.");
            Console.Write("\r\n        lat27A, lng27A   : NAD 27 (lat, long) of station A.");
            Console.Write("\r\n        lat27B, lng27B   : NAD 27 (lat, long) of station B.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     GetOHLrep  fcsa HTML 45-25-23.00N 075-41-44.00W 43-38-33.00N 079-23-14.00W 25 32 1.33 15.05 0 0");
            Console.Write("\r\n     ");
            Console.Write("\r\n     GetOHLrep  fcsa HTML 45-25-23.00N 075-41-44.00W 43-38-33.00N 079-23-14.00W 100 100 1.33 15.05 0 0");
            Console.Write("\r\n                CKJ977 \"OTTAWA(40 ELGIN STREET) ON\" CFY200 \"TORONTO(CN TOWER)  ON\"");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with qty. 12, 16 or 20 command-line arguments.");
            Console.Write("\r\n       2. Either/both of 'callA' & 'callB' can be a hyphen '-' as a slot-filler.");
            Console.Write("\r\n       3. The optional NAD 27 (lat, long) arguments are not used by GetOHLrep and so are redundant.");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the user-prescribed command-line arguments; a valid 
        /// command-line must contain qty. 12, 16 or 20 arguments.
        /// </summary>
        /// <param name="args"></param>
        public static void ParseCommandLineArgs(string[] args)
        {
            if (!(args.Length == 12 || args.Length == 16 || args.Length == 20))
            {
                WriteUsageToConsole();
                Application.ExitQuietly(10);
            }


            Info.DbName = args[0];

            /*	Get the HTML Switch */
            mRepType = args[1].ToUpper();

            if (mRepType.Equals("HTML"))
            {
                eRep = Enums.eRepType.eHTML;               /*	HTML Report */
            }
            else if (mRepType.Equals("NOHTML"))
            {
                eRep = Enums.eRepType.eNOHTML;         /*	Non-HTML Report */
            }
            else if (mRepType.Equals("CSV"))
            {
                eRep = Enums.eRepType.eCSV;
            }
            else
            {
                eRep = Enums.eRepType.eNOHTML;         /* Default to a non-html report */
            }

            // Get the lat/longs NAD 83 as the coordinates. These are the first five
            // arguments and are mandatory.
            p.mLat83A = args[2];
            p.mLong83A = args[3];
            p.mLat83B = args[4];
            p.mLong87B = args[5];

            p.mAnteHeightA = Convert.ToDouble(args[6]);
            if (p.mAnteHeightA == 0.0)
            {
                p.mAnteHeightA = 1.0;      /*	Zero antenna heights are not allowed */
            }

            p.mAnteHeightB = Convert.ToDouble(args[7]);
            if (p.mAnteHeightB == 0.0)
            {
                p.mAnteHeightB = 1.0;
            }

            /*  We have the coordinates, get the next set of things we need */
            p.mK = Convert.ToDouble(args[8]);
            if (p.mK == 0.0)
            {
                p.mK = (double)4.0 / (double)3.0;
            }

            p.mFreqMhz = Convert.ToDouble(args[9]);
            if (p.mFreqMhz == 0.0)
            {
                Console.Write("Invalid Frequency: {0}\r\n", args[7]);

                Application.ExitQuietly(13);
            }

            p.mPolarization = Convert.ToInt16(args[10]);

            p.mClimateZone = Convert.ToInt16(args[11]);


            // Parse the next four optional arguments. The user can prescribe
            // the actual call signs or insert a hyphen '-' at that position.
            // Hyphens are interpreted as empty strings.
            if (args.Length > 12)
            {

                p.mSiteCallSignA = ParseCallSign(args[12]);

                p.mSiteNameA = ParseCallSign(args[13]);

                p.mSiteCallSignB = ParseCallSign(args[14]);

                p.mSiteNameB = ParseCallSign(args[15]);
            }

            if (args.Length > 16)
            {
                p.mLat27A = args[16];
                p.mLong27A = args[17];
                p.mLat27B = args[18];
                p.mLong27B = args[19];
            }

        }

        /// <summary>
        /// This method writes an error message out to Console.Out in the event
        /// that a previous call to Calc_OhLoss() failed.
        /// </summary>
        /// <param name="sctOHLoss"></param>
        public static void WriteErrorMessage(OhLossXfer sctOHLoss)
        {
            string message;

            switch ((Enums.OHLerrStat)sctOHLoss.error_status)
            {
                case Enums.OHLerrStat.PRF_NOMAP:
                    message = "Map: ";
                    message += sctOHLoss.map_name;
                    message += " Not Found.";
                    break;

                case Enums.OHLerrStat.PRF_READERR:
                    message = "Read error in call to create path profile.";
                    break;

                case Enums.OHLerrStat.PRF_NOMEM:
                    message = "Out of memory in call to create path profile.";
                    break;

                case Enums.OHLerrStat.PRF_BADHEADER:
                    message = "Bad Header in call to create path profile.";
                    break;

                case Enums.OHLerrStat.PRF_NODATA:
                    message = "No data.";
                    break;
                default:
                    message = "Undefined error in call to create path profile.";
                    break;
            }

            Console.Write("getohlrep -- Error getting ohloss: {0}\r\n", message);
            Log2.e("\n\nGetOHLrep.WriteErrorMessage(): ERROR : sctOHLoss.error_status = {0}\nmessage = {1}", sctOHLoss.error_status, message);
        }


    }
}


```
