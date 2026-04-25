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
/// This program is invoked by the WebMICS Auxiliary Engineering tool 'Passive Calculations' 
/// and performs all required calculations by calling the method Ax14.AxPassive() and writes
/// an output report; data exchange between Wpassive and WebMICS is via intermediate text 
/// files, see <a href="AH-0019 Wpassive Input Data File Format.pdf">Wpassive Input Data File Format</a>.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - Wpassive.PNG" ""
/// </remarks>
namespace Wpassive
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class Wpassive
    {
        private static TextWriter outputTW = null;

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
                string mLog2FilePath = @"d:\MicsBatchLogs\Wpassive.log";
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
                int exitCode = Constant.SUCCESS;
                string[] inFileLines = new string[0];
                string outFilePath;
                string str;
                string note = "";
                int numPassives;
                double freqMhz;
                double[,] pasDatTab;
                AxStation[] axStns;
                bool haveDist;
                bool isPrintPat = false;

                // Instantiate all the required arrays.
                double[] beirp = Arrays.CreateAndFillArray<double>(6, 0.0); // [6],     /* EIRP from second active to first*/
                double[] dbl = Arrays.CreateAndFillArray<double>(5, 0.0);   // [5];     /* set to 1 for double passive; else 0*/
                double[] eirp = Arrays.CreateAndFillArray<double>(6, 0.0);  // [6];     /* EIRP from first active to second*/
                double[] fgain = Arrays.CreateAndFillArray<double>(6, 0.0); // [6];     /* far field gain*/
                double[] patls = Arrays.CreateAndFillArray<double>(6, 0.0); // [6];     /* pattern loss	*/
                double[] pgain = Arrays.CreateAndFillArray<double>(6, 0.0); // [6];     /* gain	*/
                double[] rsl = Arrays.CreateAndFillArray<double>(2, 0.0);   // [2];     /* received signal level*/
                double[] vfar = Arrays.CreateAndFillArray<double>(5, 0.0);  // [5];		/* set to 1 for far field, else 0*/

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

                // Verify that the input file exists and can be read; if so read in its contents.
                if (!File.Exists(Info.InFilePath))
                {
                    str = String.Format("\r\nWpassive.Main(): ERROR: the input file does not exist: {0}\r\n", Info.InFilePath);
                    Log2.e(str);
                    Console.Write("\r\n*** Cannot open input file:-\r\n   {0}\r\n", Info.InFilePath);
                    exitCode = 120;
                    goto exit_A;
                }
                else
                {
                    // Try to read the file's contents.
                    try
                    {
                        inFileLines = File.ReadAllLines(Info.InFilePath);

                        if (inFileLines.Length == 0)
                        {
                            Log2.e("\nWpassive.Main(): ERROR: input file is empty.");
                            Console.Write("\r\nERROR: input file is empty.");
                            Application.ExitQuietly(666);
                        }

                        foreach (string line in inFileLines)
                        {
                            //...Log2.v("\nInFile:   " + line);
                        }
                    }
                    catch (Exception e)
                    {
                        Log2.e("\nWpassive.Main(): ERROR: Attempt to read input file failed; threw exception: " + e.Message);
                        Console.Write("\r\nERROR: Attempt to read input file failed; permissions?");
                        exitCode = 667;
                        goto exit_A;
                    }
                }

                // Construct the path to the output file.
                str = Path.GetDirectoryName(Info.InFilePath);
                outFilePath = str + "\\" + Path.GetFileNameWithoutExtension(Info.InFilePath) + ".txt";
                //...Log2.v("\n" + outFilePath);

                // Create a TextWriter object that writes to the output file.
                try
                {
                    outputTW = File.CreateText(outFilePath);
                }
                catch (Exception e)
                {
                    Log2.e("\nWpassive.Main(): ERROR: Attempt to create/open the output file failed; threw exception: " + e.Message);
                    Console.Write("\r\nERROR: Attempt to create/open the output file failed; permissions?");
                    exitCode = 119;
                    goto exit_A;
                }

                // Establish an FCSA user session with the database.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    Console.Write("TBD -- Can't connect to database {0}.\r\n", Info.DbName);
                    exitCode = 11;
                    goto exit_B;
                }

                // Construct the string to be placed in the 'info' column in
                // the billing table.
                string infoBuffer = Info.Text;

                // Initialize the billing.
                BiUtil.BiBillingRec("WPASSIVE", infoBuffer);

                // Parse the lines read from the input file.
                rc = Ax14.AxInpPas(inFileLines, 1, out numPassives, out freqMhz, out pasDatTab, out axStns, out haveDist, out note);

                if (rc == Constant.INP_ID_FAIL)
                {
                    str = String.Format("ERROR: unable to parse input:\n    {0}\n", note);
                    Log2.e("\nWpassive.Main(): " + str);
                    outputTW.Write("\r\n" + str);
                    Console.Write("\r\n" + str);
                    exitCode = 99;
                    goto exit_C;
                }

                // Perform the passive calculations.
                rc = Ax14.AxPassive(outputTW, numPassives, freqMhz, pasDatTab, axStns, haveDist, pgain, fgain,
                                    eirp, beirp, rsl, patls, vfar, dbl);

                if (rc != Constant.SUCCESS)
                {
                    str = String.Format("ERROR: call to Ax14.AxPassive() returned rc = {0}", rc);
                    Log2.e("\nWpassive.Main(): " + str);
                    outputTW.Write("\r\n" + str);
                    Console.Write("\r\n" + str);
                    exitCode = 668;
                    goto exit_C;
                }

                // Write first line of report.
                outputTW.Write("Passive run at {0}\r\n\r\n", Info.ToFormatA(DateTime.Now));

                /* write report to output file*/
                Ax14.AxRptPas(outputTW, numPassives, isPrintPat, freqMhz, pasDatTab, axStns, pgain, fgain, eirp,
                    beirp, rsl, patls, vfar, dbl, haveDist, rc);

                // Although using 'goto' labels is somewhat taboo it has been adopted in this
                // Main() program to provide a more 'cascaded' approach to releasing resources,
                // closing connections and exiting with a prescribed exitCode.

                // Close the output file.
                exit_C: outputTW.Close();

                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the FCSA user's DB session.
                exit_B: Ssutil.UtDisconnect(1);

                exit_A: Application.ExitQuietly(exitCode);

            }
            catch (Exception e)
            {
                Log2.e("\r\r\n\nWpassive.Main(): exception caught: " + e.Message);
                Log2.e("\r\r\n\nWpassive.Main(): stack trace: \r\r\n\n" + e.StackTrace);
                if (outputTW != null) outputTW.Close();
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
            Console.Write("\r\n This program performs the passive calculations for the WebMICS Auxiliary   +");
            Console.Write("\r\n Engineering 'Passive Calculations' tool.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: Wpassive <dbName> <inFilePath> <key>");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        inFilePath       : full path to input file.");
            Console.Write("\r\n        key              : a unique identifier.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     Wpassive  fcsa D:\\users\\ahulme\\p1981-1.in 1981-1");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with qty. 3 command-line arguments.");
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
                Log2.e("\n\nWpassive.ParseCommandLineArgs(): ERROR : no command-line arguments provided - should have 3.");
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
                    Log2.e("\n\nWpassive.ParseCommandLineArgs(): ERROR : invalid option.");
                    Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                }

                // Process the flags.
                string flag = Strings.DropFirstChar(arg).ToUpper();
                switch (flag)
                {
                    default:
                        Console.Write("\r\n Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Log2.e("\n\nWpassive.ParseCommandLineArgs(): ERROR : invalid option.");
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                        break;
                }

            }

            // Parse the regular arguments; there must be qty. 3 of them.
            // Legacy C++ issue: the 3rd argument is never used!
            if (regularArgs.Count != 3)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Log2.e("\n\nWpassive.ParseCommandLineArgs(): ERROR : invalid number of arguments - should have 3.");
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Parse the args.
            Info.DbName = regularArgs[0];
            Info.InFilePath = regularArgs[1];
            Info.Text = regularArgs[2];

            //...Log2.v("\n\nInfo:" + Info.ToString());
        }




    }
}


