# Documented File: DirectoryConfig.cs
**Repository Path:** `Tools\DirectoryConfig.cs`
**Primary Layer:** `Tools`
**Namespace:** `Tools`

## Source Code Representation
```csharp
﻿using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tools
{
    public class DirectoryConfig
    {
        public static void Go(string[] args)
        {
            try
            {
                // Parse, process and sanitize the command-line arguments.
                // The prescribed input file path will be parked in Info.InFilePath
                ParseCommandLineArgs(args);

                StringBuilder sb = new StringBuilder();

                DirectoryInfo directoryInfo = new DirectoryInfo(Info.InFilePath);
                if (directoryInfo != null)
                {
                    // Get the FileInfo object for every file in the prescribed directory.
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    if (fileInfos.Length > 0)
                    {
                        sb.Append("\nFiles:");
                        foreach (FileInfo subFile in fileInfos)
                        {
                            sb.Append("\n   " + subFile.Name + " (" + subFile.Length + " bytes)");
                        }
                    }

                }

                Console.Write("\n{0}\n", sb.ToString());

                Application.ExitQuietly(0);
            }
            catch (Exception e)
            {
                Console.Error.Write("\n\nERROR: Go() threw exception: {0}\n{1}", e.Message, e.StackTrace);
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
            Console.Write("\r\n");
            Console.Write("\r\n This program outputs a list of the contents of a prescribed directory");
            Console.Write("\r\n that can be used for configuration management purposes.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: DirectoryConfig  <dirPath>");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dirPath      : a fully-qualified Windows directory path.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     DirectoryConfig   D:\\prod\\bin ");
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

            // Parse the regular arguments; there must be qty. 1 of them.
            if (regularArgs.Count != 1)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Constant.FAILURE);
            }

            // Parse the args.
            Info.InFilePath = regularArgs[0];
        }







    }
}

```
