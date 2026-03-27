using _Configuration;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This program fully populates the dcov, dxpv, dcoh, dxph columns of a subsidiary 
/// antenna table by interpolating between existing 'sparse' values for these fields; 
/// it can also 'uninterpolate' fully populated values back to their original 
/// 'sparse' state.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - Interpol.PNG" ""
/// </remarks>
namespace Interpol
{
    /// <summary>
    /// This class provides the Main() method for the MICS 'Interpol' program that
    /// fully populates the dcov, dxpv, dcoh, dxph columns of a subsidiary antenna
    /// table by interpolating between existing 'sparse' values for these fields; 
    /// it can also 'uninterpolate' fully populated values back to their original 
    /// 'sparse' state.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class Interpol
    {
        private static string mAcode = "";
        private static bool mIsInterp = true;

        /// <summary>
        /// This is the Main() method for the MICS Interpol program.
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            string fullTableName;

            int nRet = 0;
            int rc = 0;

            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\Interpol.log";
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

                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                // UtConnect() expects to receive the user's MICS ID and password
                // from the static class Info. Parse the environmental variables
                // to get and set the Info fields for MICS ID and password.
                GetEnvVariablesForUtConnect();

                // Establish an FCSA user session with the database.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    Console.Write("Interpol -- Can't connect to database {0}.\r\n", Info.DbName);

                    Application.ExitQuietly(12);
                }

                // Initialize the billing.
                BiUtil.BiBillingRec("INTERPOL", Info.SdfName);

                // Get the full table name.
                GenUtil.UtCvtName(Constant.SU_ANTD, Info.SdfName, out fullTableName);

                //...Log2.v("\nInterpol.Main(): fullTableName = " + fullTableName);

                // The following if-else block controls whether the polarization
                // discriminations are interpolated or uninterpolated.
                if (!mIsInterp)
                {
                    /*  It is an uninterpolate request */
                    //...Log2.v("\nInterpol.Main(): <--- uninterpolate");

                    nRet = Transforms.RevertToSparseDiscrims(Info.DbName, fullTableName, mAcode);
                }
                else
                {
                    /*  It is a normal interpolate request. */
                    //...Log2.v("\nInterpol.Main(): interpolate --->");

                    nRet = Transforms.PopulatePolarDiscrims(fullTableName, mAcode);
                } // normal interpolate request.

                // Report any errors.
                if (nRet != Constant.SUCCESS)
                {
                    Log2.e("\nInterpol.Main(): ERROR: Z: nRet = " + nRet);
                    Console.Write("Error:-\n{0}", GenUtil.GetUserMess());
                }

                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(nRet);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nInterpol.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nInterpol.Main(): stack trace: \r\n\r\n" + e.StackTrace);
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
            Console.Write("\r\n This program fully populates the dcov, dxpv, dcoh, dxph columns of a subsidiary ");
            Console.Write("\r\n antenna table (su_XXX_antd) by interpolating between existing 'sparse' values ");
            Console.Write("\r\n for these fields; it can also 'uninterpolate' fully populated values back to ");
            Console.Write("\r\n their original 'sparse' state.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: Interpol  [-u]  <dbName>  <projectCode>  <anteSdfName>  <aCode>");
            Console.Write("\r\n =====");
            Console.Write("\r\n");
            Console.Write("\r\n        -u               : If absent then 'interpolate'; if present then 'uninterpolate'.");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        projectCode      : user's project billing code.");
            Console.Write("\r\n        anteSdfName      : subsidiary antenna root table name (the XXX in su_XXX_antd).");
            Console.Write("\r\n        aCode            : prescribed antenna code.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     Interpol fcsa RCA1_0 1008Arev HPX4-71W");
            Console.Write("\r\n     ");
            Console.Write("\r\n     Interpol -u fcsa RCA1_0 1008Arev HPX4-71W");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n ");
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
                Application.ExitQuietly(Constant.FAILURE);
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
                    Application.ExitQuietly(13);
                }

                // Process the flags.
                string flag = Strings.DropFirstChar(arg).ToUpper();
                switch (flag)
                {
                    case "U":
                        mIsInterp = false;
                        break;
                    default:
                        Console.Write("\r\n Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(13);
                        break;
                }

            }

            // Parse the regular arguments; there must be qty. 4 of them.
            if (regularArgs.Count != 4)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Parse the args.
            Info.DbName = regularArgs[0];
            Info.ProjectCode = regularArgs[1];
            Info.SdfName = regularArgs[2];
            mAcode = regularArgs[3];

            //...Log2.v("\nInterpol:" + Info.ToString());
            //...Log2.v("\nInterpol: mAcode       = " + mAcode);
            //...Log2.v("\nInterpol: mInterpolate = " + mIsInterp);
        }






    }
}

