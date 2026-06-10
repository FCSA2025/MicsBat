# Documented File: GetCoords.cs
**Repository Path:** `GetCoords\GetCoords.cs`
**Primary Layer:** `GetCoords`
**Namespace:** `GetCoords`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using _Auxlib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _OHloss;

/// <summary>
/// This program converts from the NAD 83 coordinates of a prescribed TS
/// or ES site to NAD 27 and UTM, and from UTM to NAD 27 and NAD 83;
/// The conversion results are written to Console.Out and to the user's
/// DB 'returnvalues' table accessible to other Aux. Eng. programs.
/// </summary>
/// <remarks>
/// \image html "Usage - GetCoords.PNG" "Usage - GetCoords"
/// <para>
/// GetCoords is not called directly from WebMICS but it is called indirectly
/// by some of the Auxilliary Engineering programs, e.g. Terrain Profile.
/// </para>
/// </remarks>
namespace GetCoords
{
    /// <summary>
    /// This class provides the Main() method for the GetCoords program.
    /// </summary>
    class GetCoords
    {
        private static string mKey;

        /// <summary>
        /// This is the Main() method of the GetCoords program and provides
        /// top-level control of the coordinate conversion processing.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\GetCoords.log";
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
                int rc;

                /*	Store the lats and longs as seconds * 100 */
                int nLata27 = 0;
                int nLonga27 = 0;

                int nLata83 = 0;
                int nLonga83 = 0;

                double dUTMNorth = 0.0; ;
                double dUTMEast = 0.0; ;
                short nZone = 0;

                double dGrnd = -1.0;

                /*	The output stored in the return values table is a character array
                *   with the values:
                *   {callsign, name, lat27, long27, lat83, long83, UTMNorthing, UTMEasting, zone}
                */
                string[] cRet = new string[10];
                char csense;      /*	Storage for the sense (N or S) */

                /*	Site structure and pointers */
                MtSiteStr ptSite;
                MeSiteStr peSite;

                string cKey;
                string cInput = "";
                string cCall;
                string cLoc;
                string cSiteName;

                // Verify the command line arguments for syntax and semantics.
                bool commandLineOK = true;
                if (args.Length < 4 || args.Length > 6 ||
                    (args.Length == 4 && !(args[2].Equals("CALL") || args[2].Equals("LOC"))) ||
                    (args.Length == 5 && !args[2].Equals("LL")) ||
                    (args.Length == 6 && !args[2].Equals("UTM")))
                {
                    commandLineOK = false;
                }

                // If the command line args are invalid then print the 'usage'
                // message and exit.
                if (!commandLineOK)
                {
                    WriteUsageToConsole();
                    Application.ExitQuietly(10);
                }

                Info.DbName = args[0];
                mKey = args[1];

                // UtConnect() expects to receive the user's MICS ID and password
                // from the static class Info. Parse the environmental variables
                // to get and set the Info fields for MICS ID and password.
                GetEnvVariables();

                // Unlike most other MICS programs, the user's project charge code is not 
                // passed in through the command-line; instead we need to get it from the
                // WindowsShell environment variable MICS_PROJECT.
                string pc;
                GenUtil.GetProjectCode(out pc);
                Info.ProjectCode = pc;

                /* connect to data base */
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    Console.Write("getcoords -- Could not connect to database: {0}\r\n", rc);
                    Application.ExitQuietly(11);
                }

                // Initialize the billing record.
                BiUtil.BiBillingRec("GETCOORDS", mKey);

                /*	First open the grid tables for conversion of NAD 27 to NAD 83 */
                if (AxNTv2Lib.AxOpenGrid() == 0)
                {
                    Console.Write("\r\nERROR: call to AxOpenGrid() failed.");
                    Log2.e("\r\nGetCoords.Main(): ERROR: call to AxOpenGrid() failed.");
                    Application.ExitQuietly(666);
                }

                cKey = args[1];

                if (args[2].Length < 5)
                {
                    cInput = args[2].ToUpper();
                }
                else
                {
                    Console.Write("getcoords -- Invalid command by length: {0}\r\n", args[2]);
                    Application.ExitQuietly(13);       /*	Input invalid */
                }

                // Handle the conversion type request individually:

                // By call sign for TS sites.
                if (cInput.Equals("CALL"))
                {
                    /*	retrieve site and return all the values for this site */
                    cCall = args[3].ToUpper();

                    rc = MtUtils.MtGetSite(cCall, out ptSite, 1);  /*	just the site record */
                    if (rc == 0)
                    {
                        /*	Got the site, store the name and lat/longs */
                        nLata83 = ptSite.stSite.latit;
                        nLonga83 = ptSite.stSite.longit;
                        cSiteName = ptSite.stSite.name;
                        dGrnd = ptSite.stSite.grnd;
                        cRet[0] = cCall;
                        cRet[1] = cSiteName;
                    }
                    else if (rc == -1)
                    {
                        /*	No such site - return error */
                        Console.Write("getcoords - No such site: {0}\r\n", cCall);
                        Application.ExitQuietly(15);
                    }
                    else
                    {
                        /*	Site error */
                        Console.Write("getcoords -- Error on site retrieval of {0} ({1}). \r\n", cCall, rc);
                        Application.ExitQuietly(15);  //AH: previously exit(16);
                    }
                    //AH: original: 
                    //if (get27from83(nLata83, nLonga83, &nLata27, &nLonga27) != 0) {
                    if (Get27From83(nLata83, nLonga83, out nLata27, out nLonga27) != 0)
                    {
                        Console.Write("getcoords -- Could not get NAD 27 coords for {0}, {1}",
                                       nLata83, nLonga83);
                        nLata27 = nLonga27 = 0;
                        //return(20);
                    }

                    if (!GetUTMfrom83(nLata83, nLonga83, out dUTMNorth, out dUTMEast, out nZone))
                    {
                        Console.Write("getcoords -- Could not get UTM coords from WGS 84: {0}, {1}\r\n",
                                       nLata83, nLonga83);
                        Application.ExitQuietly(21);
                    }
                }
                // By location for ES sites.
                else if (cInput.Equals("LOC"))
                {
                    /*	ES site.  Retrieve site and return all the values for this site */
                    cLoc = args[3].ToUpper();

                    rc = MeUtils.MeGetSite(cLoc, out peSite, 1);   /*	just the site record */
                    if (rc == 0)
                    {
                        /*	Got the site, store the name and lat/longs */
                        nLata83 = peSite.stSite.latit;
                        nLonga83 = peSite.stSite.longit;
                        cSiteName = peSite.stSite.name;
                        dGrnd = peSite.stSite.grnd;
                        cRet[0] = cLoc;
                        cRet[1] = cSiteName;
                    }
                    else if (rc > 0)
                    {
                        /*	No such site - return error */
                        Console.Write("getcoords - No such site: {0}\r\n", cLoc);
                        Application.ExitQuietly(15);
                    }
                    else
                    {
                        /*	Site error */
                        Console.Write("getcoords -- Error on site retrieval. \r\n");
                        Application.ExitQuietly(15); //AH: previously exit(16);
                    }
                    if (Get27From83(nLata83, nLonga83, out nLata27, out nLonga27) != 0)
                    {
                        Console.Write("getcoords -- Could not get NAD 27 coords for {0}, {1}",
                                       nLata83, nLonga83);
                        nLata27 =
                        nLonga27 = 0;
                        //	return(20);
                    }

                    if (!GetUTMfrom83(nLata83, nLonga83, out dUTMNorth, out dUTMEast, out nZone))
                    {
                        Console.Write("getcoords -- Could not get UTM coords from WGS 84: {0}, {1}\r\n",
                                       nLata83, nLonga83);
                        Application.ExitQuietly(21);
                    }
                }
                // By (latitude, longitute) dyad.
                else if (cInput.Equals("LL"))
                {
                    /*	blank the return call and site name and convert lats and longs */
                    cCall = "";
                    cSiteName = "";
                    cRet[0] = cCall;
                    cRet[1] = cSiteName;

                    /*	Convert the input lat to hundredths of a second */
                    csense = Strings.LastChar(args[3].ToUpper());

                    if (csense.Equals('N') || csense.Equals('S'))
                    {
                        /*	Sense is valid.  remove from input */
                        args[3] = Strings.DropLastChar(args[3]);
                    }
                    else
                    {
                        csense = 'N';
                    }

                    GenUtil.UtStrConvLongLat(Constant.LATITUDE, args[3], csense, out nLata83);

                    /*	Convert the input long to hundredths of a second */
                    csense = Strings.LastChar(args[4].ToUpper());

                    if (csense.Equals('W') || csense.Equals('E'))
                    {
                        /*	Sense is valid.  remove from input */
                        args[4] = Strings.DropLastChar(args[4]);
                    }
                    else
                    {
                        csense = 'W';
                    }

                    GenUtil.UtStrConvLongLat(Constant.LONGITUDE, args[4], csense, out nLonga83);
                    if (Get27From83(nLata83, nLonga83, out nLata27, out nLonga27) != 0)
                    {
                        Console.Write("getcoords -- Could not get WGS 84 coords for {0}, {1}.",
                                       nLata83, nLonga83);
                        nLata27 =
                        nLonga27 = 0;
                        //	return(20);
                    }

                    if (!GetUTMfrom83(nLata83, nLonga83, out dUTMNorth, out dUTMEast, out nZone))
                    {
                        Console.Write("getcoords -- Could not get UTM coords from WGS 84: {0}, {1}\r\n",
                                       nLata83, nLonga83);
                        Application.ExitQuietly(21);
                    }

                }

                // By UTM (northings, eastings and zone) triad.
                else if (cInput.Equals("UTM"))
                {
                    cCall = "";
                    cSiteName = "";

                    cRet[0] = cCall;
                    cRet[1] = cSiteName;

                    /*	First convert the UTM coords to doubles */
                    dUTMNorth = Convert.ToDouble(args[3]);
                    dUTMEast = Convert.ToDouble(args[4]);
                    nZone = Convert.ToInt16(args[5]);

                    /*	Get the NAD 83 from the UTM */
                    if (!Get83fromUTM(dUTMNorth, dUTMEast, nZone, out nLata83, out nLonga83))
                    {
                        Console.Write("getcoords -- Could not get WGS 84 coords from UTM for {0}, {1}.",
                                       dUTMNorth, dUTMEast);
                        Application.ExitQuietly(22);
                    }
                    /*	Get NAD27 from NAD83 */
                    if (Get27From83(nLata83, nLonga83, out nLata27, out nLonga27) != 0)
                    {
                        Console.Write("getcoords -- Could not get NAD 27 coords from WGS 84: {0}, {1}\r\n",
                                       nLata83, nLonga83);
                        nLata27 =
                        nLonga27 = 0;
                        //	return(23);
                    }
                }
                else
                {
                    /*	Invalid argument */
                    Console.Write("getcoords -- Invalid command: {0}\r\n", cInput);
                    Application.ExitQuietly(14);
                }
                /*	If we make it here, then we have all the coordinates ready to return
                *		by placing them into the return values table. */
                Console.Write("\r\ngetcoords -- build {0} \r\nCall/Loc: '{1}', site: '{2}'\r\n",
                               Info.BuildMetaData, cRet[0], cRet[1]);

                GenUtil.UtLatConvStr(nLata27, out cRet[2], out csense);
                cRet[2] += csense;

                GenUtil.UtLongConvStr(nLonga27, out cRet[3], out csense);
                cRet[3] += csense;
                Console.Write("Lat27: '{0}', Long27: '{1}'\r\n", cRet[2], cRet[3]);

                GenUtil.UtLatConvStr(nLata83, out cRet[4], out csense);
                cRet[4] += csense;

                GenUtil.UtLongConvStr(nLonga83, out cRet[5], out csense);
                cRet[5] += csense;
                Console.Write("Lat83: '{0}', Long83: '{1}'\r\n", cRet[4], cRet[5]);

                cRet[6] = String.Format("{0,11:F2}", dUTMNorth);
                cRet[7] = String.Format("{0,11:F2}", dUTMEast);
                cRet[8] = String.Format("{0}", nZone);
                Console.Write("Northing: '{0}', Easting: '{1}', Zone: '{2}'\r\n",
                                 cRet[6], cRet[7], cRet[8]);

                /*	Put in the ground height if it is present */
                if (dGrnd >= 0.0)
                {
                    cRet[9] = String.Format("{0,11:F2}", dGrnd);
                }
                else
                {
                    cRet[9] = "";
                }

                if ((rc = Retval.RetVal(cKey, ref cRet)) != 0)
                {
                    Console.Write("getcoords -- Could not return values in database: ({0}).\r\n", rc);
                    Application.ExitQuietly(30);
                }

                BiUtil.BiBillingRec(Constant.BI_END, "");

                Ssutil.UtDisconnect(1);
                Application.ExitQuietly(0);
            }
            catch (Exception e)
            {
                Log2.e("\n\nGetCoords.Main(): exception caught: " + e.Message);
                Log2.e("\n\nGetCoords.Main(): stack trace: \n\n" + e.StackTrace);
                Ssutil.UtDisconnect(1);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }
        }

        /// <summary>
        /// This method converts NAD 83 (lat, long) to NAD 27 (lat, long).
        /// </summary>
        /// <param name="nLata83"> - NAD 83 latitude.</param>
        /// <param name="nLonga83"> - NAD 83 longitude.</param>
        /// <param name="nLata27"> - NAD 27 latitude.</param>
        /// <param name="nLonga27"> - NAD 27 longitude.</param>
        /// <returns></returns>
        private static int Get27From83(int nLata83, int nLonga83, out int nLata27, out int nLonga27)
        {
            int rc = AxNTv2Lib.AxTransCoord(nLata83, nLonga83, out nLata27, out nLonga27, Enums.NTv2Dir.NAD83to27);
            return (rc);
        }

        /// <summary>
        /// This method converts NAD 27 (lat, long) to NAD 83 (lat, long).
        /// </summary>
        /// <param name="nLata27"> - NAD 27 latitude.</param>
        /// <param name="nLonga27"> - NAD 27 longitude.</param>
        /// <param name="nLata83"> - NAD 83 latitude.</param>
        /// <param name="nLonga83"> - NAD 83 longitude.</param>
        /// <returns></returns>
        private static int Get83from27(int nLata27, int nLonga27, out int nLata83, out int nLonga83)
        {
            int rc = AxNTv2Lib.AxTransCoord(nLata27, nLonga27, out nLata83, out nLonga83, Enums.NTv2Dir.NAD27to83);
            return rc;
        }

        /// <summary>
        /// This method converts NAD 83 (lat, long) to UTM (easting, northing, zone).
        /// </summary>
        /// <param name="nLata83"> - NAD 83 latitude.</param>
        /// <param name="nLonga83"> - NAD 83 longitude.</param>
        /// <param name="dUTMNorth"> - UTM northing (m)</param>
        /// <param name="dUTMEast"> - UTM easting (m)</param>
        /// <param name="nZone"> - UTM zone.</param>
        /// <returns></returns>
        private static bool GetUTMfrom83(int nLata83, int nLonga83, out double dUTMNorth, out double dUTMEast, out short nZone)
        {
            Geo_UTM_Xfer tutm = new Geo_UTM_Xfer();

            tutm.datum = (short)Enums.Datum.NAD_83_DATUM;
            tutm.latitude = nLata83 / 360000.0;
            tutm.longitude = nLonga83 / 360000.0;

            CTEfunctions.LatLngToUTM(ref tutm);

            dUTMNorth = tutm.northing;
            dUTMEast = tutm.easting;
            nZone = tutm.zone;

            return true;
        }

        /// <summary>
        /// This method converts UTM (easting, northing, zone) to NAD 83 (lat, long).
        /// </summary>
        /// <param name="dUTMNorth"> - UTM northing (m)</param>
        /// <param name="dUTMEast"> - UTM easting (m)</param>
        /// <param name="nZone"> - UTM zone.</param>
        /// <param name="nLata83"> - NAD 83 latitude.</param>
        /// <param name="nLonga83"> - NAD 83 longitude.</param>
        /// <returns></returns>
        private static bool Get83fromUTM(double dUTMNorth, double dUTMEast, short nZone, out int nLata83, out int nLonga83)
        {
            Geo_UTM_Xfer tutm = new Geo_UTM_Xfer();

            tutm.datum = (short)Enums.Datum.NAD_83_DATUM;
            tutm.northing = dUTMNorth;
            tutm.easting = dUTMEast;
            tutm.zone = nZone;

            tutm.latitude = 0.0;
            tutm.longitude = 0.0;

            CTEfunctions.UTMtoLatLng(ref tutm);

            nLata83 = (int)(tutm.latitude * 360000);
            nLonga83 = (int)(tutm.longitude * 360000);

            return true;
        }

        /// <summary>
        /// This methods gets the values of Windows environment variables that
        /// are required to execute GetCoords; specifically these are 'MICSUSER'
        /// and 'PASSWORD' that are required for a successful call to UtConnect().
        /// </summary>
        public static void GetEnvVariables()
        {
            // Get the user's MICS ID from the environment.
            Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                Log2.e("\nFeImport.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
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
        /// succinct summary of mandatory and optional arguments when FtImport
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n This program converts from the NAD 83 coordinates of a prescribed TS");
            Console.Write("\r\n or ES site to NAD 27 and UTM, and from UTM to NAD 27 and NAD 83.");
            Console.Write("\r\n The conversion results are written to Console.Out and to the user's");
            Console.Write("\r\n DB 'returnvalues' table accessible to other Aux. Eng. programs.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: GetCoords <dbname> <key> <inputType> <parm1> [parm2] [parm3]");
            Console.Write("\r\n =====");
            Console.Write("\r\n        dbname          : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        key             : sets the 'info' column in fcsa.web.daily_usage table.");
            Console.Write("\r\n        inputType       : input type,one of CALL, LOC, LL or UTM:");
            Console.Write("\r\n                              CALL - the call sign of a TS site;");
            Console.Write("\r\n                              LOC  - the location of an ES site;");
            Console.Write("\r\n                              LL   - a latitude & longitude pair;");
            Console.Write("\r\n                              UTM  - a UTM northing, easting and zone triad.");
            Console.Write("\r\n        parm1           : TS callsign, ES location, LL latitude or UTM northing.");
            Console.Write("\r\n        parm2           : LL longtitude or UTM easting.");
            Console.Write("\r\n        parm3           : UTM zone.");
            Console.Write("\r\n");
            Console.Write("\r\n <...> indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...] indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n examples:");
            Console.Write("\r\n           GetCoords  fcsa  keyInfo  CALL  KZG2702");
            Console.Write("\r\n           GetCoords  fcsa  keyInfo  LOC   CRCK");
            Console.Write("\r\n           GetCoords  fcsa  keyInfo  LL    45-20-55.19N  075-53-26.85W");
            Console.Write("\r\n           GetCoords  fcsa  keyInfo  UTM   4878434.66  395750.35 15");
            Console.Write("\r\n");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }


    }
}

```
