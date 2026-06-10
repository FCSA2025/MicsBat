# Documented File: GetProfRep.cs
**Repository Path:** `GetProfRep\GetProfRep.cs`
**Primary Layer:** `GetProfRep`
**Namespace:** `GetProfRep`

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This MICS program is invoked
/// by the WebMICS Auxiliary Engineering tool for 'Terrain Profile'
/// calculation; it inputs data for two TS stations (A and B) and outputs 
/// a report giving ground altitude along the geodesic between A and B.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - GetProfrep.PNG" ""
/// </remarks>
namespace GetProfRep
{
    /// <summary>
    /// This class provides the Main() method for the MICS program that is invoked
    /// via the WebMICS Auxiliary Engineering tool for 'Terrain Profile' 
    /// calculation; GetProfrep inputs data for two TS stations (A and B) and outputs 
    /// a report giving ground altitudes along the geodesic between A and B.
    /// </summary>
    /// <remarks>
    /// The actual terrain altitude calculation is perfomed by calling the method <b>Create_Path_Profile()</b>
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
    public class GetProfRep
    {
        private static string mRepType;

        private static string mDir250K;
        private static string mDir50K;

        private static Parameters p = new Parameters();

        /// <summary>
        /// This is the Main() method for the MICS program GetProfRep that is invoked
        /// via the WebMICS Auxiliary Engineering tool for 'Terrain Profile' 
        /// calculation. 
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\GetProfRep.log";
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

                ProfileXfer sctProf;           /*  Structure for the OH Loss transfer        */

                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                // UtConnect() expects to receive the user's MICS ID and password
                // from the static class Info. Parse the environmental variables
                // to get and set the Info fields for MICS ID and password.
                GetEnvVariablesForUtConnect();

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
                    Console.Write("getprofrep -- Can't connect to database {0}. Reason {1}\r\n{2}\r\n",
                                    Info.DbName, rc, GenUtil.GetUserMess());

                    Application.ExitQuietly(11);
                }

                // Construct the string to be placed in the 'info' column in
                // the billing table.
                string infoBuffer = String.Format("{0}/{1} to {2}/{3} {4}",
                                                   p.mLat83A.Substring(0, 5), p.mLong83A.Substring(0, 6),
                                                   p.mLat83B.Substring(0, 5), p.mLong83B.Substring(0, 6),
                                                   mRepType);

                // Initialize the billing.
                BiUtil.BiBillingRec("GETPROFREP", infoBuffer);

                // Open the grid tables for the conversion of NAD 27 to NAD 83.
                AxNTv2Lib.AxOpenGrid();

                // We have (lat, long) values as character strings; we need to 
                // convert them to integers (centiseconds).
                p.nLata83 = LatLong.Get_Lat(ref p.mLat83A);
                p.nLonga83 = LatLong.Get_Long(ref p.mLong83A);
                p.nLatb83 = LatLong.Get_Lat(ref p.mLat83B);
                p.nLongb83 = LatLong.Get_Long(ref p.mLong83B);

                //	Calculate the distances and bearings between the two points A and B.
                AxSub2.AxDistan(0.01 * p.nLata83, 0.01 * p.nLatb83, 0.01 * p.nLonga83, 0.01 * p.nLongb83,
                                out p.fdist, out p.fbearinga, out p.fbearingb);

                // Prepare input variables for a subsequent call to the CTE ohloss routine.
                sctProf = new ProfileXfer();

                sctProf.lat_1 = LatLong.FsecsToDeg(LatLong.Get_Lat(ref p.mLat83A));
                sctProf.lng_1 = LatLong.FsecsToDeg(LatLong.Get_Long(ref p.mLong83A));
                sctProf.lat_2 = LatLong.FsecsToDeg(LatLong.Get_Lat(ref p.mLat83B));
                sctProf.lng_2 = LatLong.FsecsToDeg(LatLong.Get_Long(ref p.mLong83B));

                sctProf.dist_inc = p.fdistinc * 1000.0; /* Increment is in metres */

                //...Log2.v("\nGetProfRep.Main(): sctProf:\n" + sctProf.ToString());

                // Now calculate the terrain profile.
                CTEfunctions.Create_Path_Profile(ref sctProf);

                // Check the returned error code.
                CheckErrorCode(sctProf.error_code, out rc);

                // If there was no error reported, write out the reports.
                if (rc == 0)
                {
                    if (sctProf.cded50_files_used)
                    {
                        //	The 50K files were used.
                        p.mMapsUsed = "50K";
                    }
                    else
                    {
                        p.mMapsUsed = "250K";
                    }

                    switch (p.eRep)
                    {
                        // Standard report, HTML or non-HTML.
                        case Enums.eRepType.eHTML:
                        case Enums.eRepType.eNOHTML:
                            Reports.WritePlainOrHTML(p, sctProf);
                            break;

                        case Enums.eRepType.eCSV:
                            Reports.WriteCSV(p, sctProf);
                            break;

                        case Enums.eRepType.ePL3:
                            Reports.WritePL3(p, sctProf);
                            break;

                        default:
                            Console.Write("Report type not understood.\r\n");
                            break;
                    } // switch

                } // if


                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                /*	Close the grid file */
                AxNTv2Lib.AxCloseGrid();

                Application.ExitQuietly(0);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nGetProfRep.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nGetProfRep.Main(): stack trace: \r\n\r\n" + e.StackTrace);
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
            Console.Write("\r\n GetProfRep inputs data for two TS stations (A and B) and outputs a report");
            Console.Write("\r\n providing ground elevation above the GRS 80 datum ellipsoid at uniformly");
            Console.Write("\r\n spaced points along the geodesic between A and B. GetProfRep is called by");
            Console.Write("\r\n the WebMICS Auxiliary Engineering tool for 'Terrain Profile' calculations.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: GetProfRep <dbName> <projCode> <repType> <lat83A> <lng83A> <lat83B> <lng83B> <incr>     +");
            Console.Write("\r\n =====             [callA] [nameA] [callB] [nameB]             ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        projCode         : user's project code.");
            Console.Write("\r\n        repType          : report type - 'HTML', 'NOHTML','CSV' or 'PL3'.");
            Console.Write("\r\n        lat83A, lng83A   : NAD 83 (lat, long) of station A.");
            Console.Write("\r\n        lat83B, lng83B   : NAD 83 (lat, long) of station B.");
            Console.Write("\r\n        incr             : distance increment between profile points (km).");
            Console.Write("\r\n        callA, callB     : call sign of station A, B.");
            Console.Write("\r\n        nameA, nameB     : name      of station A, B.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     GetProfRep  fcsa hulme1_0 HTML 45-25-23.00N 075-41-44.00W 43-38-33.00N 079-23-14.00W 1.0");
            Console.Write("\r\n     ");
            Console.Write("\r\n     GetProfRep  fcsa hulme1_0 PL3 45-25-23.00N 075-41-44.00W 43-38-33.00N 079-23-14.00W 1.0");
            Console.Write("\r\n                 CKJ977 \"OTTAWA(40 ELGIN STREET) ON\" CFY200 \"TORONTO(CN TOWER)  ON\"");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *MUST* be called with qty. 8, or 12 command-line arguments.");
            Console.Write("\r\n       2. Either/both of 'callA' & 'callB' can be a hyphen '-' as a slot-filler.");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the user-prescribed command-line arguments; a valid 
        /// command-line must contain qty. 8 or 12 arguments.
        /// </summary>
        /// <param name="args"></param>
        public static void ParseCommandLineArgs(string[] args)
        {
            if (!(args.Length == 8 || args.Length == 12))
            {
                WriteUsageToConsole();
                Application.ExitQuietly(10);
            }


            Info.DbName = args[0];

            Info.ProjectCode = args[1];

            /*	Get the HTML Switch */
            mRepType = args[2].ToUpper();

            if (mRepType.Equals("HTML"))
            {
                p.eRep = Enums.eRepType.eHTML;               /*	HTML Report */
            }
            else if (mRepType.Equals("NOHTML"))
            {
                p.eRep = Enums.eRepType.eNOHTML;         /*	Non-HTML Report */
            }
            else if (mRepType.Equals("CSV"))
            {
                p.eRep = Enums.eRepType.eCSV;
            }
            else if (mRepType.Equals("PL3"))
            {
                p.eRep = Enums.eRepType.ePL3;
            }
            else
            {
                p.eRep = Enums.eRepType.eNOHTML;         /* Default to a non-html report */
            }

            // Get the lat/longs NAD 83 as the coordinates. These are the first five
            // arguments and are mandatory.
            p.mLat83A = args[3];
            p.mLong83A = args[4];
            p.mLat83B = args[5];
            p.mLong83B = args[6];

            p.fdistinc = Convert.ToDouble(args[7]);

            // Parse the next four optional arguments. The user can prescribe
            // the actual call signs or insert a hyphen '-' at that position.
            // Hyphens are interpreted as empty strings.
            if (args.Length > 8)
            {

                p.mSiteCallSignA = ParseCallSign(args[8]);

                p.mSiteNameA = ParseCallSign(args[9]);

                p.mSiteCallSignB = ParseCallSign(args[10]);

                p.mSiteNameB = ParseCallSign(args[11]);
            }

        }

        /// <summary>
        /// This method writes an error message out to Console.Out in the event
        /// that a previous call to Create_Path_Profile() failed.
        /// </summary>
        /// <param name="error_code"></param>
        /// <param name="retCode"></param>
        public static void CheckErrorCode(Enums.OHLerrStat error_code, out int retCode)
        {
            // 'out' requirement.
            retCode = 0;

            string message = "";

            switch (error_code)
            {
                case Enums.OHLerrStat.PRF_OK:
                    /*	We got the profile. It is stored in the table */
                    retCode = 0;
                    break;

                case Enums.OHLerrStat.PRF_NOMAP:
                    message = "getprofrep -- No map files found in call to create path profile.\r\n";
                    retCode = 20;
                    break;

                case Enums.OHLerrStat.PRF_READERR:
                    message = "getprofrep -- Read error in call to create path profile.\r\n";
                    retCode = 21;
                    break;

                case Enums.OHLerrStat.PRF_NOMEM:
                    message = "getprofrep -- Out of memory in call to create path profile.\r\n";
                    retCode = 22;
                    break;

                case Enums.OHLerrStat.PRF_BADHEADER:
                    message = "getprofrep -- Bad Header in call to create path profile.\r\n";
                    retCode = 23;
                    break;

                default:
                    message = "getprofrep -- Undefined error in call to create path profile.\r\n";
                    retCode = 29;
                    break;
            }

            Console.Write(message);
        }











    }
}

```
