# Documented File: BlockDbReadWrite.cs
**Repository Path:** `Tools\BlockDbReadWrite.cs`
**Primary Layer:** `Tools`
**Namespace:** `Tools`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;

namespace Tools
{
    using SQLLEN = Int64;
    using SQLHDBC = IntPtr;

    public class BlockDbReadWrite
    {
        private static string mDbAccessMode = "";

        public static void Go(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if true
                string mLog2FilePath = @"d:\MicsBatchLogs\BlockDbReadWrite.log";
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
                Info.PdfName = "blocking";

                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                GetEnvVariablesForUtConnect();

                Console.Write("\n\nAttempting to obtain {0} mode access to database {1} ...", mDbAccessMode, Info.DbName);

                //	Grab write access to the database, if possible.  If not, exit.
                if ((rc = Qutils.EnterQueue(Info.DbName, mDbAccessMode, 30)) != Constant.SUCCESS)
                {
                    Log2.e("\nBlockDatabase.Go(): ERROR: call to Qutils.EnterQueue() failed, rc = " + rc);
                    Qutils.ExplainQueue(Info.DbName, mDbAccessMode, rc, null);
                    Application.ExitQuietly(100);
                }

                Console.Write("\n\n{0} mode access to DB {1} successfully obtained.", mDbAccessMode, Info.DbName);

                // Establish an FCSA user session with the database.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    //  Can't connect to database 
                    Console.Write("\r\nERROR: call to UtConnect() failed: could not connect to database {0}\r\n{1}\r\n",
                                        Info.DbName, GenUtil.GetUserMess());

                    Qutils.ExitQueue(Info.DbName, mDbAccessMode);
                    Application.ExitQuietly(101);
                }

                // Initialize the billing.
                BiUtil.BiBillingRec("BLOCKING", Info.PdfName);

                //...Log2.v("\nBlockDatabase.Go(): Info:\n" + Info.ToString());

                //	We have the HOLD; let the user know.
                Console.Write("\r\nPress any key to exit the DB {0} mode access ...", mDbAccessMode);

                // Wait for the user to press any key.
                Console.ReadKey();

                // Exit the Read or Write queue.
                Qutils.ExitQueue(Info.DbName, mDbAccessMode);

                Console.Write("\nThe {0} mode access was released for DB {1}.\n", mDbAccessMode, Info.DbName);

                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(rc);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nBlockDatabase.Go(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nBlockDatabase.Go(): stack trace: \r\n\r\n" + e.StackTrace);
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
                Log2.e("\r\nFtImport.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
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
            Console.Write("\r\n This program establishes a READ or WRITE hold on access to a prescribed database, ");
            Console.Write("\r\n blocking access to other users until a key is pressed.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: BlockDatabase  <dbName>  <projectCode>  <blockType>");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        projectCode      : user's project billing code.");
            Console.Write("\r\n        blockType        : Should be either READ or WRITE.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     BlockDatabase  fcsa  HULME1_0   WRITE  ");
            Console.Write("\r\n     ");
            Console.Write("\r\n Notes:");
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

            // Parse the regular arguments; there must be qty. 2 of them.
            if (regularArgs.Count != 3)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Constant.FAILURE);
            }

            // Parse the args.
            Info.DbName = regularArgs[0];
            Info.ProjectCode = regularArgs[1];
            string blockParam = regularArgs[2];

            // Check the READ / WRITE block parameter.
            switch (blockParam.ToLower())
            {
                case "read":
                    mDbAccessMode = "READ";
                    break;
                case "write":
                    mDbAccessMode = "WRITE";
                    break;
                default:
                    Console.Write("\r\n Invalid DB access blocking type: \"{0}\".   Should be READ or WRITE.\r\n", blockParam);
                    WriteUsageToConsole();
                    Application.ExitQuietly(Constant.FAILURE);
                    break;
            }
        }




    }
}

```
