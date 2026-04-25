using _Auxlib;
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
/// This program is called by the WebMICS Auxiliary Engineering 'Satellite Bearings' tool;  
/// it reads in several command line arguments and passes them into the SatAze.AxSataze() 
/// method that calculates the azimuth and elevation angle from an earth station to       
/// a geostationary satellite taking atmospheric refraction into account; the results     
/// of the calculations are inserted as rows in the user's returnvalues table.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - WsatAze.PNG" ""
/// </remarks>
namespace WsatAze
{
    /// <summary>
    /// This class provides the Main() method for the WsatAze.exe program.
    /// </summary>
    /// <remarks>
    /// This program is called by the WebMICS Auxiliary Engineering 'Satellite Bearings' tool.
    /// 
    /// It reads in several command line arguments and passes them into the SatAze.AxSataze() 
    /// method that calculates the azimuth and elevation angle from an earth station to       
    /// a geostationary satellite taking atmospheric refraction into account.
    /// 
    /// The results of the calculations are inserted as rows in the user's returnvalues table.
    /// </remarks>
    public class WsatAze
    {
        private static AxStation mEarthStation = new AxStation();  // earth station
        private static AxStation mSatelliteData = new AxStation(); // satellite data
        private static double mRefractIndex;                       // refractive index		
        private static string mKey;                                // key for returnvalues table


        /// <summary>
        /// This is the Main() method for the MICS program WsatAze.
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if true
                string mLog2FilePath = @"d:\MicsBatchLogs\WsatAze.log";
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
                double azim;                         // ES to satellite azimuth	
                double elevAng;                      // elevation angle		
                double rtElevAng;                    // ES to satellite refractd elev angle
                string[] cVals = new string[4];

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

                // Establish an FCSA user session with the database.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    // Can't connect to database 
                    string str = String.Format("\r\nWsatAze.Main(): ERROR: Can't connect to database {0}.\r\n", Info.DbName);
                    Log2.e(str);
                    Console.Write(str);
                    Application.ExitQuietly(97);
                }

                // Construct the string to be placed in the 'info' column in
                // the billing table.
                string infoBuffer = mKey;

                // Initialize the billing.
                BiUtil.BiBillingRec("WSATAZE", infoBuffer);

                // The following method call performs the required calculations.
                rc = SatAze.AxSataze(
                                        "M",
                                         ref mEarthStation,
                                         mRefractIndex,
                                         ref mSatelliteData,
                                         out azim,
                                         out elevAng,
                                         out rtElevAng);

                // Check the returned values for errors.
                if (rc != Constant.SUCCESS)
                {
                    Log2.e("\nWsatAze.Main(): ERROR: call to SatAze.AxSataze() returned " + rc);
                    Console.Write("\r\n*** ERROR: call to SatAze.AxSataze() returned  {0}\r\n", rc);
                    Application.ExitQuietly(96);
                }

                // Amalgamate the results into a string[].
                cVals[0] = String.Format("{0}", rc);
                cVals[1] = String.Format("{0,12:F4}", azim);
                cVals[2] = String.Format("{0,12:F4}", elevAng);
                cVals[3] = String.Format("{0,12:F4}", rtElevAng);

                // Just in case, delete any rows in the user's returnvalues table
                // that have their 'retkey' column equal to the 'key' string prescribed
                // on the comman-line.
                Retval.DeleteRowsWithKey(mKey);

                // Write the results to the user's returnvalues table.
                rc = Retval.RetVal(mKey, ref cVals);

                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(0);

            }
            catch (Exception e)
            {
                Log2.e("\r\r\n\nWsatAze.Main(): exception caught: " + e.Message);
                Log2.e("\r\r\n\nWsatAze.Main(): stack trace: \r\r\n\n" + e.StackTrace);
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
            Console.Write("\r\n");
            Console.Write("\r\n This program is used by the WebMICS Auxiliary Engineering 'Satellite Bearings' tool.  +");
            Console.Write("\r\n It reads in several command line arguments and passes them into the SatAze.AxSataze() +");
            Console.Write("\r\n method that calculates the azimuth and elevation angle from an earth station to       +");
            Console.Write("\r\n a geostationary satellite taking atmospheric refraction into account. The results     +");
            Console.Write("\r\n of the calculations are inserted as rows in the user's returnvalues table.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: WsatAze  <dbName> <lat> <long> <grndAlt> <anteHeight> <satLong> <refIndex> <key>");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        lat              : latitude  of site (dd-mm-ss.hh).");
            Console.Write("\r\n        long             : longitude of site (dd-mm-ss.hh).");
            Console.Write("\r\n        grndAlt          : altitude of site ground-level (m).");
            Console.Write("\r\n        anteHeight       : height of antenna above ground-level (m).");
            Console.Write("\r\n        satLong          : satellite longitude.");
            Console.Write("\r\n        refIndex         : refractive index of atmosphere (250 or 400).");
            Console.Write("\r\n        key              : identifier written to the user's DB returnvalues table.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n      WsatAze  fcsa 51-03-10.20N 114-00-59.56W 1067 4 111.1 330 2022-4");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with qty. 8 command-line arguments.");
            Console.Write("\r\n       2. The <lat>  is taken to be north whether this argument ends with 'N' or not.");
            Console.Write("\r\n       2. The <long> is taken to be west  whether this argument ends with 'W' or not.");
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
                    WriteUsageToConsole();
                    Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                }

                // Process the flags.
                string flag = Strings.DropFirstChar(arg).ToUpper();
                switch (flag)
                {

                    default:
                        Console.Write("\r\n Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                        break;
                }

            }

            // Parse the regular arguments; there must be qty. 8 of them.
            if (regularArgs.Count != 8)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Parse the args.
            Info.DbName = regularArgs[0];

            Station.LoadStn(ref mEarthStation, regularArgs[1], Constant.LATITUDE);

            Station.LoadStn(ref mEarthStation, regularArgs[2], Constant.LONGITUDE);

            // Get Station Ground Height.
            mEarthStation.elevM = Convert.ToDouble(regularArgs[3]);

            // Get Station antenna height.
            mEarthStation.antHtM = Convert.ToDouble(regularArgs[4]);

            // Satellite longitude.
            double dSatLong;
            dSatLong = Convert.ToDouble(regularArgs[5]);
            if (dSatLong >= -180.0)
            {
                mSatelliteData.LL.longSeconds = (int)(Math.Abs(dSatLong) * 3600.0);
                mSatelliteData.LL.longSens = (dSatLong >= 0.0) ? "W" : "E";
            }
            else
            {
                Console.Write("\r\nInvalid Satellite Longitude: {0}\r\n", regularArgs[5]);
                Application.ExitQuietly(98);
            }

            // Get the refraction coefficient. This is really only 400 or 250.
            mRefractIndex = Convert.ToDouble(regularArgs[6]);

            // Get the key for the return values table.
            mKey = regularArgs[7];

            //...Log2.v("\nWsatAze:" + Info.ToString());
            //...Log2.v("\nes: " + mEarthStation.ToString());
            //...Log2.v("\nste: " + mSatelliteData.ToString());
            //...Log2.v("\nrefInd: " + mRefractIndex);
            //...Log2.v("\ncKey: " + mKey);

        }




    }
}


