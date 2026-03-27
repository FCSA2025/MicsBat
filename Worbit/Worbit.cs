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
/// This program reads in qty.15 command line arguments and passes most of them
/// into the AxOrbitSupp.AxOrbit() method and returns the results in the user's 
/// returnvalues table; this program is invoked by the WebMICS 
/// 'Auxiliary Engineering' -> 'Orbit Intersection' tool.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - Worbit.PNG" ""
/// </remarks>
namespace Worbit
{
    /// <summary>
    /// This class contains the main method that reads in qty.15 command line arguments and passes most of them
    /// into the AxOrbitSupp.AxOrbit() method and returns the results in the user's 
    /// returnvalues table.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class Worbit
    {
        private static AxStation mStn1 = new AxStation();
        private static AxStation mStn2 = new AxStation(); // stn1 and stn2 data  

        private static int mTrueVals = Constant.FALSE;  // flag - true values entered 
        private static double mTrueAzim;
        private static double mTrueElev;      // true azimuth and elevation 
        private static string mKey;

        /// <summary>
        /// This is the Main() method for the MICS program Worbit. 
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLog\Worbit.log";
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
                int retVal;
                char cSense;

                double azim = 0.0;           // satellite azimuth	
                double crAzEnd = 0.0;        // critical azimuth end	
                double crAzSt = 0.0;         // critical azimuth start	
                double crLEnd = 0.0;         // critical longitude end	
                double crLSt = 0.0;          // critical long start		
                double dist = 0.0;           // distance			
                double elevAng = 0.0;        // elevation angle		
                double e1_10 = 0.0;          // maximum EIRP for 1 to 10 GHz	
                double e10_15 = 0.0;         // maximum EIRP for 10 to 15 GHz	
                double eGt15 = 0.0;          // maximum EIRP for more than 15 GHz
                double insL = 0.0;           // intersection longitude	
                double minAngSep = 0.0;      // minimum angular separation
                int trans = Constant.FALSE;  // flag - true if stations transposed
                string crLEndSen = "";       // critical longitude end sense	
                string crLStSen = "";        // critical longitude start sense
                string insLSen = "";         // intersection longitude sense	

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
                if (pc.Equals("N/A"))
                {
                    Console.Write("\nERROR: environment variable MICS_PROJECT is not set.");
                    Application.ExitQuietly(122);
                }
                else
                {
                    // Environment variable MICS_PROJECT is set.
                    Info.ProjectCode = pc;
                }

                //...Log2.v("\nWorbit:" + Info.ToString());

                // Establish an FCSA user session with the database.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    // Can't connect to database 
                    Console.Write("TBD -- Can't connect to database {0}.\r\n", Info.DbName);

                    Application.ExitQuietly(11);
                }

                // Construct the string to be placed in the 'info' column in
                // the billing table.
                string infoBuffer = mKey;

                // Initialize the billing.
                BiUtil.BiBillingRec("WORBIT", infoBuffer);

                // Perform the orbit calculations.
                retVal = AxOrbitSupp.AxOrbit("M",
                                                ref mStn1,
                                                ref mStn2,
                                                mTrueAzim,
                                                mTrueElev,
                                                mTrueVals,
                                                ref dist,
                                                ref azim,
                                                ref elevAng,
                                                ref insL,
                                                ref insLSen,
                                                ref minAngSep,
                                                ref e1_10,
                                                ref e10_15,
                                                ref eGt15,
                                                ref crAzSt,
                                                ref crAzEnd,
                                                ref crLSt,
                                                ref crLStSen,
                                                ref crLEnd,
                                                ref crLEndSen,
                                                ref trans);

                //...Log2.v("\nWorbit.Main(): retVal = " + retVal);

                //	Accumulate the return values into an string array.
                string[] cVals = new string[19];

                GenUtil.UtLatConvStr((int)(mStn1.LL.latSeconds * 100.0), out cVals[0], out cSense);
                cVals[0] += cSense;
                GenUtil.UtLongConvStr((int)(mStn1.LL.longSeconds * 100.0), out cVals[1], out cSense);
                cVals[1] += cSense;
                cVals[2] = String.Format("{0}", retVal);
                cVals[3] = String.Format("{0,12:F4}", dist);
                cVals[4] = String.Format("{0,12:F4}", azim);
                cVals[5] = String.Format("{0,12:F6}", elevAng);
                cVals[6] = String.Format("{0,12:F6}", insL);
                cVals[7] = insLSen;
                cVals[8] = String.Format("{0,12:F6}", minAngSep);
                cVals[9] = String.Format("{0,12:F6}", e1_10);
                cVals[10] = String.Format("{0,12:F6}", e10_15);
                cVals[11] = String.Format("{0,12:F6}", eGt15);
                cVals[12] = String.Format("{0,12:F6}", crAzSt);
                cVals[13] = String.Format("{0,12:F6}", crAzEnd);
                cVals[14] = String.Format("{0,12:F6}", crLSt);
                cVals[15] = crLStSen;
                cVals[16] = String.Format("{0,12:F6}", crLEnd);
                cVals[17] = crLEndSen;
                cVals[18] = String.Format("{0}", trans);

                // Just in case, delete any rows in the user's returnvalues table whose
                // 'retkey' column equals the key prescribed on Worbit's command-line invocation.
                rc = Retval.DeleteRowsWithKey(mKey);

                //...Log2.v("\nWorbit.Main(): call to Retval.DeleteRowsWithKey() returned rc = " + rc);

                // Insert the results as rows in the user's returnvalues table, e.g.
                // fcsa.hulme.returnvalues. Each row is inserted with the 'retkey'
                // column set to the 15th command-line argument <key> so that the result rows
                // inserted by running Worbit.exe can be uniquely identified by the next
                // MICS program that reads from the returnvalues table.
                rc = Retval.RetVal(mKey, ref cVals);

                //...Log2.v("\nWorbit.Main(): call to Retval.RetVal() returned rc = " + rc);

                for (int i = 0; i < cVals.Length; i++)
                {
                    Console.Write("\ncVals[{0,2}] = {1}", i, cVals[i]);
                }

                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(0);

            }
            catch (Exception e)
            {
                Log2.e("\r\r\n\nWorbit.Main(): exception caught: " + e.Message);
                Log2.e("\r\r\n\nWorbit.Main(): stack trace: \r\r\n\n" + e.StackTrace);
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
            Console.Write("\r\n This program reads in a plethora of command line arguments and passes most of them   +");
            Console.Write("\r\n into the AxOrbitSupp.AxOrbit() method and inserts the results of the call as rows    +");
            Console.Write("\r\n in the user's returnvalues table. Worbit.exe is used by the WebMICS Auxiliary        +");
            Console.Write("\r\n Engineering 'Orbit Intersection' tool.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: Worbit <qty. 15 command-line arguments as identified below>");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\n        1.  <database>        - The name of the database used for lookups");
            Console.Write("\n        2.  <Site1 call> or 0 - Optional call sign of first site");
            Console.Write("\n        3.  <Site1 Lat>       - The latitude (dd-mm-ss.hh[S]) of site1");
            Console.Write("\n        4.  <Site1 Long>      - Site1 longitude");
            Console.Write("\n        5.  <Site1 Grnd>      - Ground height of Site1(m)");
            Console.Write("\n        6.  <Site1 Aht>       - Antenna height of Site1(m)");
            Console.Write("\n        7.  <Site2 call> or 0 - Optional call sign of second site");
            Console.Write("\n        8.  <Site2 Lat>       - The latitude of Site2");
            Console.Write("\n        9.  <Site2 Long>      - The longitude of Site2");
            Console.Write("\n       10.  <Site2 Grnd>      - Ground height of Site2(m)");
            Console.Write("\n       11.  <Site2 Aht>       - Antenna height of Site2(m)");
            Console.Write("\n       12.  <Use True>        - T or F for use True azimuth or not");
            Console.Write("\n       13.  <True Azimuth>    - The true Azimuth to use or 0");
            Console.Write("\n       14.  <True Elevation>  - The true elevation or 0");
            Console.Write("\n       15.  <Key>             - Disambiguator for returning values"); Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     Worbit fcsa 0 47-31-26N 52-50-46W 159 14 0 47-30-53N 52-44-22W 163 7  0  0  0  alpha");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with qty. 15 command-line arguments.");
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
                regularArgs.Add(arg);
            }

            // Parse the regular arguments; there must be qty. 15 of them.
            if (regularArgs.Count != 15)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Parse the args.

            // Get the name of the database.
            Info.DbName = regularArgs[0];

            // Call sign of first station 
            mStn1.name = args[1];

            // Get Station 1 latitude 
            Station.LoadStn(ref mStn1, args[2], Constant.LATITUDE);

            // Get Station 1 longitude 
            Station.LoadStn(ref mStn1, args[3], Constant.LONGITUDE);

            // Get Station 1 Ground Height 
            mStn1.elevM = Convert.ToDouble(args[4]);

            // Get Station 1 antenna Height 
            mStn1.antHtM = Convert.ToDouble(args[5]);

            // Call sign of second station 
            mStn2.name = args[6];

            // Get Station 2 latitude 
            Station.LoadStn(ref mStn2, args[7], Constant.LATITUDE);

            // Get Station 2 longitude 
            Station.LoadStn(ref mStn2, args[8], Constant.LONGITUDE);

            // Get Station 2 Ground Height 
            mStn2.elevM = Convert.ToDouble(args[9]);

            // Get Station 2 antenna Height 
            mStn2.antHtM = Convert.ToDouble(args[10]);

            // The use true switch T or F 
            if (args[11].ToUpper() == "T")
            {
                mTrueVals = Constant.TRUE;
            }

            //	Get the true azimuth 
            mTrueAzim = Convert.ToDouble(args[12]);

            //	Get the true elevation 
            mTrueElev = Convert.ToDouble(args[13]);

            //	Get the key 
            mKey = args[14];

            //...Log2.v("\n");
            //...Log2.v("\n" + mStn1.ToString());
            //...Log2.v("\n" + mStn2.ToString());
            //...Log2.v("\ntrueVals = " + mTrueVals);
            //...Log2.v("\ntrueAzim = " + mTrueAzim);
            //...Log2.v("\ntrueElev = " + mTrueElev);
            //...Log2.v("\ncKey = " + mKey);
        }




    }
}


