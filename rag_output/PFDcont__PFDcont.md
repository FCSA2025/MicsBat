# Documented File: PFDcont.cs
**Repository Path:** `PFDcont\PFDcont.cs`
**Primary Layer:** `PFDcont`
**Namespace:** `PFDcont`

## Source Code Representation
```csharp
﻿using _Configuration;
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
using static _NewLib.Enums;

/// <summary>
/// This program computes the Power Flux Density (PFD) Countours around a transmitter 
/// using prescribed geographic location, antenna characteristics, terrain and free-space 
/// loss models, and atmospheric environments.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - PFDcont.PNG" ""
/// </remarks>
namespace PFDcont
{

    /// <summary>
    /// This class provides the Main() method for the PFDcont program that computes the 
    /// Power Flux Density Countours around a transmitter 
    /// using prescribed geographic location, antenna characteristics, terrain and free-space 
    /// loss models, and atmospheric environments.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class PFDcont
    {
        private const int TABLE_SZ = 400;
        private static List<TtabRow> mApTable = new List<TtabRow>();
        private static string mDir250K;
        private static string mDir50K;
        private static PFD mPFD = new PFD();

        private const double ACCURACY = 0.1;

        private delegate double LossFunct(double w, Tpass tArgs);

        /// <summary>
        /// This is the Main() method for the PFDcont program that computes the 
        /// Power Flux Density Countours around a transmitter 
        /// using prescribed geographic location, antenna characteristics, terrain and free-space 
        /// loss models, and atmospheric environments.
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\PFDcont.log";
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
                int err;
                int retCode;
                //string faxref;
                //double maxpfdlevel;
                //double minpfdlevel;
                int latitudeCS;
                int longitudeCS;
                //double latitude;
                //double longitude;
                double dNextAzStep;
                const double dAzStep = 1.0; /* The size of the azimuth step. */
                double dNextAzPat;
                double dLastAzPat;
                int nLastPat;
                double dCurrentAz;
                double dAzAng;		/*	the actual azimuth angle */
                char orientation;
                int nPointCt;
                SuAntd tRowDisc;
                MtSiteStr mtSiteStr = null;
                SuAntStr suAntStr;
                UserInfoData userInfoData;

                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
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
                    Console.Write("Can't connect to database {0}.\r\n", Info.DbName);

                    Application.ExitQuietly(Constant.FAILURE);
                }

                // Construct the string to be placed in the 'info' column in
                // the billing table.
                string infoBuffer = String.Format("");

                // Initialize the billing.
                BiUtil.BiBillingRec("PFDCONT", infoBuffer);

                // >>>>>>> APPLICATION SPECIFIC CODE HERE <<<<<<<<<<<

                // Get user info data.
                if ((err = UserInfo.UtGetUserInfo(out userInfoData)) != Constant.SUCCESS)
                {
                    Log2.e("\n\nPFDcont.Main(): ERROR: call to UserInfo.UtGetUserInfo() failed, returned: " + err);
                    ErrMsg.UtErrMessage(err);
                    Application.ExitQuietly(119);
                }

                // Calculate the values derived from the command line arguments.

                // Derive the base file name for Reports, CSVs and MapInfo.
                mPFD.BaseFileName = Path.GetFileNameWithoutExtension(mPFD.BaseReportPath);

                // If the user has entered a call sign, fetch the MtSite from 
                // table main.mt_site so that we can get the site's name.
                mPFD.Location = GetSiteLocation(mPFD.Call1);

                // Get the antenna cross reference.
                retCode = Suutils.SuGetAnt(mPFD.AnteCode, out suAntStr);
                if (retCode != Constant.SUCCESS)
                {
                    Log2.e("\n\nPFDcont.Main(): ERROR: call to Suutils.SuGetAnt() failed, returned: " + retCode);
                    Console.Write("\r\n*** Antenna code: {0} not found.\r\n", mPFD.AnteCode);
                    Application.ExitQuietly(116);
                }
                mPFD.AnteCodeOfXref = suAntStr.acAnt.acode;  /*	Get code of xref */

                // Calculate the minimum pfd.
                if (Calc.IsCoverage(mPFD.CalcMode))
                {
                    mPFD.MinPFDlevel = Calc.Pfd(mPFD);
                    mPFD.MaxPFDlevel = mPFD.MinPFDlevel;
                }

                //	Get the orientation from the latitude.
                orientation = Strings.LastChar(mPFD.LatStr.ToUpper());
                switch (orientation)
                {
                    case 'N':
                    case 'S':
                        // Strip off the N or S from the latitude string.
                        mPFD.LatStr = Strings.DropLastChar(mPFD.LatStr);
                        break;
                    default:
                        // Does not end in 'N" or 'S'.
                        // Check that it ends with a digit.
                        if (Strings.IsDigit(orientation))
                        {
                            // Assume that the user wants north.
                            orientation = 'N';
                        }
                        else
                        {
                            // Invalid latitude string.
                            Log2.e("\n\nPFDcont.Main(): ERROR: A: invalid latitude string: " + mPFD.LatStr);
                            Application.ExitQuietly(115);
                        }
                        break;
                }

                if (GenUtil.UtStrConvLongLat(Constant.LATITUDE, mPFD.LatStr, orientation, out latitudeCS) == Constant.FAILURE)
                {
                    Log2.e("\n\nPFDcont.Main(): ERROR: B: invalid latitude string: " + mPFD.LatStr);
                    Console.Write("\r\n*** Invalid Latitude: {0}\r\n", mPFD.LatStr);
                    Application.ExitQuietly(115);
                }

                mPFD.Latitude = latitudeCS / 360000.0;
                //...Log2.v("\nPFDcont.Main(): latitude  = " + mPFD.Latitude);

                //	Get the orientation from the longitude.
                orientation = Strings.LastChar(mPFD.LongStr.ToUpper());
                switch (orientation)
                {
                    case 'W':
                    case 'E':
                        // Strip off the W or E from the longitude string.
                        mPFD.LongStr = Strings.DropLastChar(mPFD.LongStr);
                        break;
                    default:
                        // Does not end in 'W' or 'E'.
                        // Check that it ends with a digit.
                        if (Strings.IsDigit(orientation))
                        {
                            // Assume that the user wants west.
                            orientation = 'W';
                        }
                        else
                        {
                            // Invalid longitude string.
                            Log2.e("\n\nPFDcont.Main(): ERROR: A: invalid longitude string: " + mPFD.LongStr);
                            Application.ExitQuietly(115);
                        }
                        break;
                }

                if (GenUtil.UtStrConvLongLat(Constant.LONGITUDE, mPFD.LongStr, orientation, out longitudeCS) == Constant.FAILURE)
                {
                    Log2.e("\n\nPFDcont.Main(): ERROR: B: invalid longitude string: " + mPFD.LongStr);
                    Console.Write("\r\n*** Invalid Longitude: {0}\r\n", mPFD.LongStr);
                    Application.ExitQuietly(115);
                }

                mPFD.Longitude = longitudeCS / 360000.0;
                //...Log2.v("\nPFDcont.Main(): longitude = " + mPFD.Longitude);

                // Retrieve the antenna gain.      
                mPFD.AnteGain = suAntStr.acAnt.again;

                // If logging, view the mPFD member values prior to starting the calculations.
                //...Log2.v("\n\nPFDcont.Main(): PFD:\n" + mPFD.ToString());

                /*	Got the Antenna, now get the discriminations and contours */

                // We merge the step size and the antenna pattern, so that we have
                // both in the table of azimuths.
                dNextAzStep = 0.0;

                int lastPat = Constant.NOT_INITIALIZED; // This triggers initialization of pattern element.

                nLastPat = Calc.NextPat(suAntStr.acDscPtr,
                                    suAntStr.acAnt.anip,
                                    Calc.TruetoPat(0.0, mPFD.TxAzim),
                                    lastPat);  /* Initilize the pattern element */
                if (nLastPat < 0)
                {
                    /*	We are running back down a symmetric pattern */
                    dAzAng = 360.0 - suAntStr.acDscPtr[-nLastPat].antang;
                }
                else
                {
                    /*	Running normally up through a pattern */
                    dAzAng = suAntStr.acDscPtr[nLastPat].antang;
                }

                dNextAzPat = Calc.PatToTrue(dAzAng, mPFD.TxAzim);
                dLastAzPat = -1;        // Start.  This azimuth is used to detect when
                                        // the pattern moves around past the zero point */

                // This is the master loop that performs a calculation over
                // azimuth [0, 360] degrees in 1 degree increments.
                nPointCt = 0;
                while (dNextAzStep <= 360.0)
                {
                    // We do a balanced merge of the the next step azimuth (dNextAzStep)
                    // and the next pattern azimuth (dNextAzPat).  If the next azimuth
                    // pattern is past the 360 mark, it would be less than the last
                    // Azimuth pattern.

                    if (dNextAzStep < dNextAzPat ||
                        dNextAzPat < dLastAzPat)
                    {
                        /*	The next azimuth is from the stepping */
                        retCode = Suutils.InterpADisc(suAntStr.acDscPtr,
                                                        (float)Calc.TruetoPat(dNextAzStep, mPFD.TxAzim),
                                                        suAntStr.acAnt.anip,
                                                        out tRowDisc);
                        if (retCode < 0)
                        {
                            Console.Write("\r\n* Could not interpolate antenna {0}, reason:{1}\r\n",
                                            mPFD.AnteCode, retCode);
                        }

                        dCurrentAz = dNextAzStep;
                        dNextAzStep += dAzStep;

                    }
                    else if (dNextAzStep > dNextAzPat &&
                             dNextAzPat > dNextAzStep - dAzStep)
                    {
                        int nLP = nLastPat < 0 ? -nLastPat : nLastPat;

                        // There is a pattern azimuth before this step azimuth and after
                        // the last step azimuth.

                        tRowDisc = suAntStr.acDscPtr[nLP];
                        //...Log2.v("\n\nPFDcont.Main(): A: tRowDisc = " + tRowDisc.ToString());

                        nLastPat = Calc.NextPat(suAntStr.acDscPtr,
                                            suAntStr.acAnt.anip,
                                            Calc.TruetoPat(dNextAzPat, mPFD.TxAzim),
                                            nLastPat);

                        dCurrentAz = dNextAzPat;

                        if (nLastPat < 0)
                        {
                            /*	We are running back down a symmetric pattern */
                            dAzAng = 360.0 - suAntStr.acDscPtr[-nLastPat].antang;
                        }
                        else
                        {
                            /*	Running normally up through a pattern */
                            dAzAng = suAntStr.acDscPtr[nLastPat].antang;
                        }
                        dLastAzPat = dNextAzPat;
                        dNextAzPat = Calc.PatToTrue(dAzAng, mPFD.TxAzim);
                    }
                    else
                    {

                        int nLP = nLastPat < 0 ? -nLastPat : nLastPat;

                        // A pattern and the step fall on the same azimuth.  Take the
                        // pattern, but increment both.
                        tRowDisc = suAntStr.acDscPtr[nLP];
                        //...Log2.v("\n\nPFDcont.Main(): B: tRowDisc = " + tRowDisc.ToString());

                        nLastPat = Calc.NextPat(suAntStr.acDscPtr,
                                            suAntStr.acAnt.anip,
                                            Calc.TruetoPat(dNextAzPat, mPFD.TxAzim),
                                            nLastPat);  /* Initilize the pattern element */
                        if (nLastPat < 0)
                        {
                            /*	We are running back down a symmetric pattern */
                            dAzAng = 360.0 - suAntStr.acDscPtr[-nLastPat].antang;
                        }
                        else
                        {
                            /*	Running normally up through a pattern */
                            dAzAng = suAntStr.acDscPtr[nLastPat].antang;
                        }
                        dCurrentAz = dNextAzPat;

                        dLastAzPat = dNextAzPat;
                        dNextAzPat = Calc.PatToTrue(dAzAng, mPFD.TxAzim);

                        dNextAzStep += dAzStep;

                    }

                    // Now output the point to the table.
                    int nRet;

                    nRet = Calc.AddPoint(ref mApTable,
                                            dCurrentAz,
                                            mPFD,
                                            tRowDisc
                                            );

                    if (nRet != Constant.SUCCESS)
                    {
                        /* Something went wrong */
                        Console.Write("\r\n* Could not add point {0:G} for reason {1}\r\n",
                                            tRowDisc.antang, nRet);
                    }
                    else
                    {
                        nPointCt++;         /*	Count the number of points */
                    }

                } // while-loop

                mPFD.IsCalculated = true;

                // ******************************************************************
                // Now output the reports.  The normal method of getting these reports
                // is by internet email.  This is only possible if the user's email
                // address is stored in the mics_users_extended table. The email
                // address is retrieved, and used for all output.  If there is no
                // email address, then then no reports are sent anywhere, but the
                // files produced are kept.
                // *******************************************************************

                //...Log2.v("\n\nPFDcont.Main(): Info:\n" + Info.ToString());
                //...Log2.v("\n\nPFDcont.Main(): mCurrRow = " + mCurrRow);

                // Get the user's email address.

                string cEmailAddr;
                string tsip_email;
                string auto_delete;

                if (Ssutil.EmailAddr(Info.UltrixID, Info.MicsUserName, out cEmailAddr, Constant.EMAIL_SIZE_,
                                        out tsip_email, out auto_delete) != 0)
                {
                    /*	Could not get the email address */
                    cEmailAddr = "";
                    Console.Write("\r\n** Problem: No email address.");
                }
                else
                {
                    Console.Write("\r\n* Emailing output to {0}\r\n  Filename: {1}\r\n",
                              cEmailAddr, mPFD.BaseFileName);
                }

                // Write results to files and send emails.
                Products.WriteFilesSendEmail(mPFD,
                                                mtSiteStr,
                                                suAntStr,
                                                mApTable,
                                                cEmailAddr
                                                );

                //------------------------------------------------------------------
                // Prepare for a clean exit.
                //------------------------------------------------------------------

                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(0);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nPFDcont.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nPFDcont.Main(): stack trace: \r\n\r\n" + e.StackTrace);
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
                string errMsg = "\r\nPFDcont.GetEnvVariablesForUtConnect(): ERROR: Windows environment variable MicsUser is not set.";
                Console.Error.Write(errMsg);
                Log2.e(errMsg);
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
                //...Log2.v("\nPFDcont.GetEnvVariablesForMaps(): env. variable 'FCSAMAPS50K'  not set; using default: " + mDir50K);
            }
            else
            {
                mDir50K = value;
            }

            value = Environment.GetEnvironmentVariable("FCSAMAPS250K");
            if (String.IsNullOrWhiteSpace(value))
            {
                mDir250K = Constant.CMAPDIR250K_DEFAULT;
                //...Log2.v("\nPFDcont.GetEnvVariablesForMaps(): env. variable 'FCSAMAPS250K' not set; using default: " + mDir250K);

            }
            else
            {
                mDir250K = value;
            }

            //...Log2.v("\nPFDcont.GetEnvVariablesForMaps(): FCSAMAPS50K  = " + mDir50K);
            //...Log2.v("\nPFDcont.GetEnvVariablesForMaps(): FCSAMAPS250K = " + mDir250K);
        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program computes the Power Flux Density contours around a transmitter       +");
            Console.Write("\r\n using its prescribed geographic location, antenna characteristics, terrain and    +");
            Console.Write("\r\n free-space loss models, and atmospheric environments.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: PFDcont <dbName> <calcMode> <lossType> <callSign> <txLat> <txLong>         +");
            Console.Write("\r\n =====          <txAnteCode> <txAnteHeight> <txAzimuth> <txPower> <txAnteFSL>      +");
            Console.Write("\r\n                <txGroundHeight> <rxAnteHeight> <txBandwidth> <txFrequency>        +");
            Console.Write("\r\n                <atmosAtten> <arg#17> <arg#18> <outputFlags> <outBaseFilePath>");
            Console.Write("\r\n");
            Console.Write("\r\n  1.    dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n  2.    calcMode         : 'P' or 'C',");
            Console.Write("\r\n                               - 'P' = PFD contour,");
            Console.Write("\r\n                               - 'C' = Coverage.");
            Console.Write("\r\n  3.    lossType         : 'S' or 'T',");
            Console.Write("\r\n                               - 'S' = Spherical,");
            Console.Write("\r\n                               - 'T' = Terrain.");
            Console.Write("\r\n  4.    callSign         : call sign of a TS site (to get its name);");
            Console.Write("\r\n                           or '0' to indicate no call sign.");
            Console.Write("\r\n  5.    txLat            : latitude  of transmit antenna, e.g.  45-19-07.05N .");
            Console.Write("\r\n  6.    txLong           : longitude of transmit antenna, e.g. 075-54-46.40W .");
            Console.Write("\r\n  7.    txAnteCode       : transmit antenna's code.");
            Console.Write("\r\n  8.    txAnteHeight     : transmit antenna's height above ground (m).             ");
            Console.Write("\r\n  9.    txAzimuth        : transmit antenna's azimuth.");
            Console.Write("\r\n 10.    txPower          : transmit antenna's output signal power.");
            Console.Write("\r\n 11.    txAnteFSL        : transmit antenna's Feed System Loss (FSL).");
            Console.Write("\r\n 12.    txGroundHeight   : ground height (m) at base of tower.");
            Console.Write("\r\n 13.    rxAnteHeight     : receive antenna's height above ground (m).");
            Console.Write("\r\n 14.    txBandwidth      : bandwidth of transmitted signal.");
            Console.Write("\r\n 15.    txFrequency      : frequency of transmitted signal.");
            Console.Write("\r\n 16.    atmosAtten       : atmospheric attenuation (dB per km).");
            Console.Write("\r\n                       ");
            Console.Write("\r\n ------ if calcMode is PFD contour ------                     ");
            Console.Write("\r\n 17.    arg#17           : minimum PFD level. (Climate defaults to Continental)");
            Console.Write("\r\n 18.    arg#18           : maximum PFD level. (Climate defaults to Continental)");
            Console.Write("\r\n                       ");
            Console.Write("\r\n ------ if calcMode is Coverage ------                     ");
            Console.Write("\r\n 17.    arg#17           : minimum receive power.");
            Console.Write("\r\n 18.    arg#18           : 'M' or 'C', ");
            Console.Write("\r\n                               - 'M' = Maritime    radio climate,");
            Console.Write("\r\n                               - 'C' = Continental radio climate");
            Console.Write("\r\n                       ");
            Console.Write("\r\n 19.    outputFlags      : if string contains 'R' a plain-text report (.REP) is produced");
            Console.Write("\r\n                           if string contains 'C' a CSV-formatted data file (.CSV) is produced");
            Console.Write("\r\n                           if string contains 'M' MapInfo data files (.MID and .MIF)) are produced");
            Console.Write("\r\n 20.    outBaseFilePath  : path to output files with no extension.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     PFDcont fcsa P S VEL415 45-19-07.05N 075-54-46.40W 04F0406A 25 180 0 0 0 60.0 50 7125000 0.10 -114.0 -94.0 RCM");
            Console.Write("\r\n             D:\\Inetpub\\micstest\\mics\\userdirs\\hulme\\hulme1\\p1519-2");
            Console.Write("\r\n     ");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with all 20 command-line arguments.");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the user-prescribed command-line arguments.
        /// </summary>
        /// <param name="args"></param>
        public static void ParseCommandLineArgs(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    WriteUsageToConsole();
                    Log2.e("\n\nPFDcont.ParseCommandLineArgs(): ERROR : there were no command-line arguments provided.");
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
                        Log2.e("\n\nPFDcont.ParseCommandLineArgs(): ERROR : Invalid command-line argument: '{0}'", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                    }

                    // Process the flags.
                    string flag = Strings.DropFirstChar(arg).ToUpper();
                    switch (flag)
                    {
                        default:
                            Console.Write("\r\n Invalid flag: {0}\r\n", arg);
                            Log2.e("\n\nPFDcont.ParseCommandLineArgs(): ERROR : Invalid command-line argument: '{0}'", arg);
                            WriteUsageToConsole();
                            Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                            break;
                    }

                }

                // Parse the regular arguments; there must be qty. 20 of them.
                const int NUMARGS = 20;
                if (regularArgs.Count != 20)
                {
                    Console.Write("\r\n Invalid number of arguments: found {0}; should be {1}\r\n", regularArgs.Count, NUMARGS);
                    Log2.e("\n\nPFDcont.ParseCommandLineArgs(): ERROR : Invalid number of command-line arguments: found {0}; should be {1}\r\n", regularArgs.Count, NUMARGS);
                    WriteUsageToConsole();
                    Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                }

                // #1: name of database.
                Info.DbName = regularArgs[0];

                // #2: [P]fdcontour or [C]overage.
                switch (regularArgs[1].ToUpper())
                {
                    case "P":
                        mPFD.CalcMode = PFDcalcMode.PFDCONTOUR;
                        break;
                    case "C":
                        mPFD.CalcMode = PFDcalcMode.COVERAGE50;
                        break;
                    default:
                        Console.Write("\r\nCoverage ({0}) must be either [P]fdcontour or [C]overage.\r\n", regularArgs[1]) ;
                        Log2.e("\n\nPFDcont.ParseCommandLineArgs(): ERROR : Coverage ({0}) must be either [P]fdcontour or [C]overage.\r\n", regularArgs[1]);
                        Application.ExitQuietly(126);
                        break;
                }

                // #3: Loss calculation.
                switch (regularArgs[2].ToUpper())
                {
                    case "S":
                        mPFD.Eloss = PFDloss.SPHERICAL;
                        break;
                    case "T":
                        mPFD.Eloss = PFDloss.TERRAIN;
                        break;
                    default:
                        Console.Write("\r\nLoss Calculation must be either[S]pherical or[T]errain.\r\n");
                        Log2.e("\n\nPFDcont.ParseCommandLineArgs(): ERROR : Loss Calculation must be either[S]pherical or[T]errain.\r\n");
                        Application.ExitQuietly(125);
                        break;
                }

                // #4: call sign or zero; a single 0 indicates no call sign.
                string str = regularArgs[3].ToUpper();
                if ((str.Length == 1) && (str == "0"))
                {
                    mPFD.Call1 = "";
                }
                else
                {
                    mPFD.Call1 = str;
                }

                // #5: latitude.
                mPFD.LatStr = regularArgs[4];

                // #6: longitude.
                mPFD.LongStr = regularArgs[5];

                // #7: antenna code.
                str = regularArgs[6];
                if (str.Length > Constant.ABAND_SZ - 1) str = str.Substring(0, Constant.ABAND_SZ - 1);
                mPFD.AnteCode = str.ToUpper();

                // #8: antenna height
                mPFD.TxHeight = Convert.ToDouble(regularArgs[7]);

                // #9: azimuth
                mPFD.TxAzim = Convert.ToDouble(regularArgs[8]);

                // #10: transmit power
                mPFD.TxPwr = Convert.ToDouble(regularArgs[9]);

                // #11: antenna feed system loss
                mPFD.Fsl = Convert.ToDouble(regularArgs[10]);

                // #12: ground height
                mPFD.Grnd = Convert.ToDouble(regularArgs[11]);

                // #13: Rx antenna height
                mPFD.RxHeight = Convert.ToDouble(regularArgs[12]);

                // #14: bandwidth
                mPFD.Bandwidth = Convert.ToDouble(regularArgs[13]);

                // #15: frequency
                mPFD.Frequency = Convert.ToDouble(regularArgs[14]);

                // #16: atmospheric Attenuation
                mPFD.AtmosAtten = Convert.ToDouble(regularArgs[15]);

                // #17: depends on whether we are doing pfdcontours or coverage.
                // #18: depends on whether we are doing pfdcontours or coverage.
                if (mPFD.CalcMode == PFDcalcMode.PFDCONTOUR)
                {
                    // #17: minimum pfd level
                    mPFD.MinPFDlevel = Convert.ToDouble(regularArgs[16]);

                    // #18: maximum pfd level
                    mPFD.MaxPFDlevel = Convert.ToDouble(regularArgs[17]);

                    // Default the climate to Continental for PFDCONTOUR.
                    mPFD.Climate = PFDclimate.CONTINENTAL;
                }
                else  // PDFcontCalcMode.COVERAGE
                {
                    // #17: the minimum Rx power for coverage
                    mPFD.MinRxPwr = Convert.ToDouble(regularArgs[16]);

                    // #18: is either [M]aritime or [C]ontinental
                    str = regularArgs[17].ToUpper();
                    switch (str)
                    {
                        case "M":
                            mPFD.Climate = PFDclimate.MARITIME;
                            break;
                        case "C":
                            mPFD.Climate = PFDclimate.CONTINENTAL;
                            break;
                        default:
                            Console.Write("\r\nRadio Climate must be either [M]aritime or [C]ontinental\r\n");
                            Log2.e("\n\nPFDcont.ParseCommandLineArgs(): ERROR : Radio Climate must be either [M]aritime or [C]ontinental\r\n");
                            Application.ExitQuietly(124);
                            break;
                    }
                }

                // #19: [R]eport output, [C]SV file, or [M]apinfo.
                str = regularArgs[18].ToUpper();
                if (str.Contains("R")) mPFD.IsReport = true;
                if (str.Contains("C")) mPFD.IsCSV = true;
                if (str.Contains("M")) mPFD.IsMapInfo = true;

                // #20: the printid = full pathname of report file to be written.
                mPFD.BaseReportPath = regularArgs[19];
            }
            catch (Exception e)
            {
                Log2.e("\n\nPFDcont.ParseCommandLineArgs(): ERROR: exception: " + e.Message);
                Log2.e("\nStackTrace:\n" + e.StackTrace);
                Console.Write("\r\n\r\nERROR: a string argument could not be converted to double.");
            }
        }

        /// <summary>
        /// This method returns the 'location' field of the MDB main.mt_site record 
        /// for a prescribed call sign.
        /// </summary>
        /// <param name="call1"></param>
        /// <returns></returns>
        private static string GetSiteLocation(string call1)
        {
            string location = "";
            int retCode;
            MtSiteStr mtSiteStr;

            if (!String.IsNullOrWhiteSpace(call1))
            {
                /*	The user has entered a site, pull up the site */
                retCode = MtUtils.MtGetSite(call1, out mtSiteStr, 3);

                if (retCode != Constant.SUCCESS)
                {
                    Log2.e("\n\nPFDcont.GetSiteLocation(): ERROR: call to MtUtils.MtGetSite() failed, returned: " + retCode);

                    /*	Couldn't find the site for some reason */
                    if (retCode == Constant.FAILURE)
                    {
                        Console.Write("\r\n*** Site not found: {0}\r\n", call1);
                        Application.ExitQuietly(118);
                    }
                    else
                    {
                        Console.Write("\r\n*** Could not get the site.  Reason is {0}.", retCode);
                        Application.ExitQuietly(117);
                    }
                }

                location = mtSiteStr.stSite.name;
            }
            else
            {
                location = "";
            }

            return location;
        }



    }
}


```
