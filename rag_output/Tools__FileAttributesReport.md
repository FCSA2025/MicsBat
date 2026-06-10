# Documented File: FileAttributesReport.cs
**Repository Path:** `Tools\FileAttributesReport.cs`
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
    public class FileAttributesReport
    {
        public static void Go(string[] args)
        {
            // Parse, process and sanitize the command-line arguments.
            // The prescribed input file path will be parked in Info.InFilePath
            ParseCommandLineArgs(args);

            // Get the file attributes..
            FileAttributes attributes = new FileAttributes();
            try
            {
                attributes = File.GetAttributes(Info.InFilePath);
            }
            catch (Exception e)
            {
                Console.Error.Write("\n\nERROR: call to File.GetAttributes() threw exception: {0}", e.Message);
                Application.ExitQuietly(666);
            }

            StringBuilder sb = new StringBuilder();

            //           12345678901234567890
            sb.Append("\nArchive            : ");
            string result = (attributes & FileAttributes.Archive) == FileAttributes.Archive ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nCompressed         : ");
            result = (attributes & FileAttributes.Compressed) == FileAttributes.Compressed ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nDevice             : ");
            result = (attributes & FileAttributes.Device) == FileAttributes.Device ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nDirectory          : ");
            result = (attributes & FileAttributes.Directory) == FileAttributes.Directory ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nEncrypted          : ");
            result = (attributes & FileAttributes.Encrypted) == FileAttributes.Encrypted ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nHidden             : ");
            result = (attributes & FileAttributes.Hidden) == FileAttributes.Hidden ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nIntegrityStream    : ");
            result = (attributes & FileAttributes.IntegrityStream) == FileAttributes.IntegrityStream ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nNormal             : ");
            result = (attributes & FileAttributes.Normal) == FileAttributes.Normal ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nNoScrubData        : ");
            result = (attributes & FileAttributes.NoScrubData) == FileAttributes.NoScrubData ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nNotContentIndexed  : ");
            result = (attributes & FileAttributes.NotContentIndexed) == FileAttributes.NotContentIndexed ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nOffline            : ");
            result = (attributes & FileAttributes.Offline) == FileAttributes.Offline ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nReadOnly           : ");
            result = (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nReparsePoint       : ");
            result = (attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nSparseFile         : ");
            result = (attributes & FileAttributes.SparseFile) == FileAttributes.SparseFile ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nSystem             : ");
            result = (attributes & FileAttributes.System) == FileAttributes.System ? "yes" : " no";
            sb.Append(result);

            sb.Append("\nTemporary          : ");
            result = (attributes & FileAttributes.Temporary) == FileAttributes.Temporary ? "yes" : " no";
            sb.Append(result);


            Console.Write("\n{0}\n", sb.ToString());

            Application.ExitQuietly(0);
        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program retrieves the Windows file attributes of a file at the, ");
            Console.Write("\r\n prescribed path and writes them to the console.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: FileAttributes  <filePath>");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        filePath      : a fully-qualified Windows file path.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     FileAttributes  d:\\prod\\bin\\TsipInitiator.exe");
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
