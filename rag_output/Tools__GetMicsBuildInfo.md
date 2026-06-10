# Documented File: GetMicsBuildInfo.cs
**Repository Path:** `Tools\GetMicsBuildInfo.cs`
**Primary Layer:** `Tools`
**Namespace:** `Tools`

## Source Code Representation
```csharp
﻿using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tools
{
    public class GetMicsBuildInfo
    {
#pragma warning disable CS0414
        private static bool mIsDirectory;
#pragma warning restore CS0414

        private static string mPath = "";

        private static string[] mMICSexeNames = new string[36]
{
"FeImport.exe",
"FePrint.exe",
"FeValidate.exe",
"FtImport.exe",
"FtPrint.exe",
"FtValidate.exe",
"GenCtx.exe",
"GetCoords.exe",
"GetOHLrep.exe",
"getprofrep.exe",
"HiLoCheck.exe",
"hold.exe",
"Interpol.exe",
"meUpdate.exe",
"MtUpdate.exe",
"pcnscan.exe",
"pfdcont.exe",
"sdUpdateAnte.exe",
"sdUpdateBand.exe",
"sdUpdateCtx.exe",
"sdUpdateEqpt.exe",
"sdUpdateNote.exe",
"sdUpdateOper.exe",
"sdUpdatePlan.exe",
"sdUpdateRout.exe",
"sdUpdateTown.exe",
"sdUpdateTowr.exe",
"sdUpdateTraf.exe",
"TpRunTsip.exe",
"TsipInitiator.exe",
"TsipQdelete.exe",
"Vch.exe",
"wOrbit.exe",
"Wpassive.exe",
"writegate.exe",
"WsatAze.exe"
};


        public static void Go(string[] args)
        {
            try
            {
                // There should only be one command-line argument namely the value of mPath.
                ParseCommandLineArgs(args);

                // Validate the value of mPath.
                if (Directory.Exists(mPath))
                {
                    mIsDirectory = true;
                }
                else
                {
                    // Invalid path.
                    Console.Error.Write("\n\nERROR: Invalid directory path: {0}\n", mPath);
                    WriteUsageToConsole();
                    Application.ExitQuietly(1);
                }

                // Create a list of MICS program executables to be processed.
                List<string> MICSexePaths = new List<string>();

                foreach (string exeName in mMICSexeNames) { MICSexePaths.Add(Path.Combine(mPath, exeName)); }

                // Execute the MICS program with no arguments and strip the build info-tag from
                // the resulting "command-line usage" message.
                foreach (string exePath in MICSexePaths)
                {
                    // Check that the exe file actually exists at the prescribed location.
                    if (!File.Exists(exePath))
                    {
                        Console.Error.Write("\nERROR: File not found: {0}", exePath);
                        continue;
                    }

                    string stdOut = "";
                    string stdErr = "";
                    int exitCode = 0;

                    int retVal = WindowsShell.RunCommand(exePath, "", out stdOut, out stdErr, out exitCode);

                    if (retVal != Constant.SUCCESS)
                    {
                        Console.Error.Write("\n\nERROR: call to WindowsShell.RunCommand() failed for exePath = {0}.\n", exePath);
                        continue;
                    }

                    // Strip out the build info-tag from the stdOut text.
                    // Build: 210329-1218/64-R
                    string pattern = @"(\d\d\d\d\d\d-\d\d\d\d/\d\d-\w)";

                    Match match = Regex.Match(stdOut.ToUpper(), pattern);

                    if (match.Success)
                    {
                        string buildInfoTag = match.Groups[1].Value.ToString();
                        Console.Write("\n{0},{1}", Path.GetFileName(exePath), buildInfoTag);
                    }
                    else
                    {
                        Console.Write("\n{0},{1}", Path.GetFileName(exePath), "N/A");
                    }
                }

            }
            catch (Exception e)
            {
                Console.Error.Write("\\nERROR: exception: {0}", e.Message);
                Console.Error.Write("\\n{0}", e.StackTrace);
                Application.ExitQuietly(666);
            }
        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n This program provides the program build info-tags for MICS programs in a.");
            Console.Write("\r\n prescribed directory. The results are written to Console.Out.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: GetMicsBuildInfo <path>");
            Console.Write("\r\n =====");
            Console.Write("\r\n");
            Console.Write("\r\n        path      : a fully-qualified Windows path to a directory.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     GetMicsBuildInfo d:\\prod\\bin");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program must be called with a single command-line argument.");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the user-prescribed command-line arguments.
        /// </summary>
        /// <param name="args"></param>
        public static void ParseCommandLineArgs(string[] args)
        {
            if (args.Length != 1)
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

            // Parse the regular arguments; there must be qty. 1 of them.
            if (regularArgs.Count != 1)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Parse the args.
            mPath = args[0];
        }


    }
}


```
