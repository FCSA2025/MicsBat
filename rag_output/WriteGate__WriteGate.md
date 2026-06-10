# Documented File: WriteGate.cs
**Repository Path:** `WriteGate\WriteGate.cs`
**Primary Layer:** `WriteGate`
**Namespace:** `WriteGate`

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
/// This program checks if there is already a HOLD on the WRITE queue for a prescribed database;
/// if there is, then it exits; if not, it establishes a HOLD on the database
/// and waits for keyboard input from the user; when the user presses any key
/// the HOLD is released and the program exits.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - WriteGate.PNG" ""
/// </remarks>
namespace WriteGate
{
    /// <summary>
    /// This class provides the Main() method for the MICS program WriteGate;
    /// WriteGate checks if there is already a HOLD on the WRITE queue for a prescribed database;
    /// if there is, then it exits; if not, it establishes a HOLD on the database
    /// and waits for keyboard input from the user; when the user presses any key
    /// the HOLD is released and the program exits.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class WriteGate
    {

        /// <summary>
        /// This is the Main() method for the MICS program WriteGate.
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\WriteGate.log";
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

                // Get the user's MICS ID from the environment.
                // This is needed for a successful call to Qutils.EnterQueue().
                Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
                if (String.IsNullOrWhiteSpace(Info.MicsUserName))
                {
                    Log2.e("\r\nWriteGate.Main(): ERROR: Windows environment variable MicsUser is not set.");
                    Console.Error.Write("\r\n\r\nERROR: this program needs the environment variable MICSUSER to be set.\r\n");
                    Application.ExitQuietly(Error.ENVVARMICSUSERNOTSET);
                }

                // Grab write access to the database, if possible.  If not, exit.
                int rc;

                if ((rc = Qutils.EnterQueue(Info.DbName, "WRITE", 5)) != 0)
                {
                    Qutils.ExplainQueue(Info.DbName, "WRITE", rc, null);
                    Application.ExitQuietly(1);
                }

                //	We have the hold.  Let the user know.
                Console.Write("\r\nDatabase {0} is now WRITE locked.  Press any key to release...", Info.DbName);

                // Wait for the user to press any key.
                Console.ReadKey();

                Qutils.ExitQueue(Info.DbName, "WRITE");

                Console.Write("\r\n\r\nWRITE Gate on {0} has been released.\r\n", Info.DbName);

                Application.ExitQuietly(0);

            }
            catch (Exception e)
            {
                Log2.e("\r\r\n\nWriteGate.Main(): exception caught: " + e.Message);
                Log2.e("\r\r\n\nWriteGate.Main(): stack trace: \r\r\n\n" + e.StackTrace);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
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
            Console.Write("\r\n This program checks if there is already a hold on a prescribed database.    +");
            Console.Write("\r\n If there is, then it exits. If not, it establishes a hold on the database   +");
            Console.Write("\r\n and waits for keyboard input from the user. When the user presses any key   +");
            Console.Write("\r\n the hold is released and the program exits.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: WriteGate  <dbName>");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n        WriteGate  fcsa");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with qty. 1 command-line arguments.");
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

            // Parse the regular arguments; there must be qty. 3 of them.
            if (regularArgs.Count != 1)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Parse the args.
            Info.DbName = regularArgs[0];

            //...Log2.v("\nWriteGate:" + Info.ToString());
        }




    }
}



```
