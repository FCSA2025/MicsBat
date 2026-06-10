# Documented File: Vch.cs
**Repository Path:** `Vch\Vch.cs`
**Primary Layer:** `Vch`
**Namespace:** `Vch`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This program performs checks on imported TS PDF data (a prescribed set of ft_ DB tables) 
/// that are additional to, and independent of, those checks performed by the program FtValidate.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - Vch.PNG" ""
/// </remarks>
namespace Vch
{
    /// <summary>
    /// This class provides the Main() method for the MICS program Vch.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class Vch
    {
        private static bool mDetailRequested = false;
        private static bool mExitIfNotValidated = true;

        /// <summary>
        /// This is the Main() method for the Vch program 
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\Vch.log";
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

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                GetEnvVariablesForUtConnect();

                // Establish an FCSA user session with the database.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    // Can't connect to database 
                    Console.Write("Vch -- Can't connect to database {0}.\r\n", Info.DbName);
                    Application.ExitQuietly(11);
                }

                // Write out the build information.
                Console.Write("Vch.exe: Build Configuration: {0}\r\n", Info.ManagedBuildInfo());

                // Check that the prescribed PDF tables set is validated for UPDATE.
                // If it isn't then exit unless the user included the command line
                // option to suppress this test.

                FtTitl ftTitle;
                rc = FtUtils.FtGetTitle(out ftTitle, Info.PdfName);

                if (rc == Constant.SUCCESS)
                {
                    if (ftTitle.validated == "U")
                    {
                        Console.Write("Vch -- The PDF table set is validated for UPDATE.\r\n");
                    }
                    else
                    {
                        // The PDF table set is not validated for UPDATE so exit. 
                        Console.Write("Vch -- The PDF table set is NOT validated for UPDATE.\r\n");

                        //...Log2.v("\nVch.Main(): The PDF table set is not validated for UPDATE.");
                        if (mExitIfNotValidated)
                        {
                            Console.Write("    -- Vch processing will not be performed.\r\n");
                            Console.Write("    -- To force Vch processing use the command line option [-force]'.\r\n");
                            Application.ExitQuietly(102);
                        }
                        else
                        {
                            Console.Write("       (Command-line option '-force' used; Vch processing will continue.)\r\n");
                        }
                    }
                }
                else
                {
                    // Failed to fetch FtTitle record from ft_PDF_titl table. 
                    Console.Write("Vch -- ERROR: could not fetch titl record from table.\r\n");
                    Log2.e("\nVch.Main(): ERROR: call to FtUtils.FtGetTitle() failed.");
                    Application.ExitQuietly(103);
                }


                // Get system date and time.
                string curDate;
                string curTime;
                GenUtil.UtGetDateTime(out curDate, out curTime);

                // Write report header.
                Console.Write("\r\nPDF POST VALIDATION REPORT");
                Console.Write("\r\nRUN DATE: {0}  {1}\r\nProject Code [{2}]\r\n", curDate, curTime, Info.ProjectCode);

                // Call the top-level method that does all the work.
                int nRet = Vcheck.Go(Info.PdfName, mDetailRequested);

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(0);

            }
            catch (Exception e)
            {
                Log2.e("\r\r\n\nVch.Main(): exception caught: " + e.Message);
                Log2.e("\r\r\n\nVch.Main(): stack trace: \r\r\n\n" + e.StackTrace);
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
            Console.Write("\r\n This program performs checks on imported TS PDF data (a prescribed set of ft_ DB tables)");
            Console.Write("\r\n that are additional to, and independent of, those checks performed by the program FtValidate.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: Vch <dbName> <projCode> <pdfName> [-force] [-d]");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        project          : user's project 'charge' code.");
            Console.Write("\r\n        pdfName          : the XXX in a table name like ft_XXX_ante, etc.");
            Console.Write("\r\n        -force           : continue even if PDF tables are not validated for UPDATE.");
            Console.Write("\r\n        -d               : outputs additional detailed remote-end link information.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     Vch fcsa HULME1_0 regadd -d -force");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. ");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.ManagedBuildInfo());
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
                    case "D":
                        mDetailRequested = true;
                        break;
                    case "FORCE":
                        mExitIfNotValidated = false;
                        break;
                    default:
                        Console.Write("\r\n Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                        break;
                }

            }

            // Parse the regular arguments; there must be qty. 3 of them.
            if (regularArgs.Count != 3)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Parse the args.
            Info.DbName = regularArgs[0];
            Info.ProjectCode = regularArgs[1];
            Info.PdfName = regularArgs[2];

            //...Log2.v("\nVch: Info = " + Info.ToString());
            //...Log2.v("\nVch: mCLflagSet_D        = " + mCLflagSet_D);
            //...Log2.v("\nVch: mCLflagSet_F        = " + mCLflagSet_F);
            //...Log2.v("\nVch: mExitIfNotValidated = " + mExitIfNotValidated);
        }




    }
}


```
