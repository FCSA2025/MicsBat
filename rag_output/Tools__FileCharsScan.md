# Documented File: FileCharsScan.cs
**Repository Path:** `Tools\FileCharsScan.cs`
**Primary Layer:** `Tools`
**Namespace:** `Tools`

## Source Code Representation
```csharp
﻿using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class FileCharsScan
    {
        private const string OUT_FILE_PATH = @"d:\MicsBatchLogs\FileCharsScanOut.txt";
        public class Tally
        {
            public char mChar = (char)0;
            public int mCodePoint = 0;
            public int mCount = 0;

            public Tally(char chr)
            {
                mChar = chr;
                mCount = 0;
                mCodePoint = (int)chr;
            }

            public void IncCount() { mCount++; }

            public override string ToString()
            {
                return String.Format("char = {0}, Unicode Code Point = U+{1:X4}, count = {2,6}", mChar, mCodePoint, mCount);
            }

            public static void Process(ref List<Tally> tallyList, char chr)
            {
                // If the list is empty add the first tally object.
                if (tallyList.Count == 0)
                {
                    tallyList.Add(new Tally(chr));
                    return;
                }

                // Does the prescribed char already exist in a tallyList element?
                bool foundChar = false;

                foreach (Tally tally in tallyList)
                {
                    // If the chr already exists in a tally object then increment its count.
                    if (tally.mChar == chr)
                    {
                        foundChar = true;
                        tally.IncCount();
                        break;
                    }
                }

                // chr does not already exist in a tally - create one and add to the list.
                if (!foundChar)
                {
                    tallyList.Add(new Tally(chr));
                }

            }
        }


        public static void Go(string[] args)
        {
            try
            {

                string[] lines = null;
                List<Tally> tallyList = new List<Tally>();
                StringBuilder sb = new StringBuilder();

                // This places the prescribed file path into Info.InFilePath   .
                ParseCommandLineArgs(args);

                // Validate the file path by reading all the characters in the file.
                try
                {
                    lines = File.ReadAllLines(Info.InFilePath);
                }
                catch (Exception e)
                {
                    Console.Error.Write("\n\nERROR: exception: {0}", e.Message);
                    Application.ExitQuietly(666);
                }

                // Scan characters in line to identify any Unicode code points above U+7F.
                for (int row = 0; row < lines.Length; row++)
                {
                    char[] chars = lines[row].ToCharArray();

                    for (int col = 0; col < chars.Length; col++)
                    {
                        char chr = chars[col];

                        int cp = chr;

                        if (cp > 0x7F)
                        {
                            sb.Append(String.Format("\nrow = {0}, col = {1}, codePoint = {2:X4}, char = {3}", row, col, cp, chr));

                            Tally.Process(ref tallyList, chr);
                        }
                    }
                }

                // Write the results to a file so that the UTF-8 'tally' characters are preserved.
                // Writing to the Console does not preserve all the UTF-8 characters.
                sb.Append("\n==================================================================");

                string funnyString = "\n";

                foreach (Tally tally in tallyList)
                {
                    funnyString += tally.mChar;

                    byte[] b;
                    int numBytes;

                    string mangledStr = MangleUTF8CharToWindows1252(tally.mChar, out b, out numBytes);

                    sb.Append(String.Format("\n{0},U+{1:X4},{2},{3},{4},x{5:X2},x{6:X2},x{7:X2},x{8:X2}",
                                               tally.mChar, tally.mCodePoint, tally.mCount, numBytes, mangledStr, b[0], b[1], b[2], b[3]));
                }

                sb.Append("\n==================================================================");

                sb.Append(funnyString);
                sb.Append(funnyString.ToUpper());

                File.WriteAllText(OUT_FILE_PATH, sb.ToString());

            }
            catch (Exception e)
            {
                Console.Write("\n\nERROR: exception: {0}\n{1}", e.Message, e.StackTrace);
            }
        }

        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n This program checks a prescribed file to identify any Unicode");
            Console.Write("\r\n characters that are encoded using 2, 3, or 4 bytes.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: TFileCharsScan  <filePath>");
            Console.Write("\r\n =====");
            Console.Write("\r\n");
            Console.Write("\r\n        filePath      : full path to prescribed input file.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     FileCharsScan  d:\foor\bar.csv");
            Console.Write("\r\n     ");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the user-prescribed command-line arguments; a valid 
        /// command-line must have qty. 2, 3, 4 or 5 command-line arguments..
        /// </summary>
        /// <param name="args"></param>
        public static void ParseCommandLineArgs(string[] args)
        {
            if (args.Length != 1)
            {
                WriteUsageToConsole();
                Application.ExitQuietly(97);
            }

            Info.InFilePath = args[0];
        }

        public static string MangleUTF8CharToWindows1252(char charUTF8, out byte[] fileBytes, out int numBytes)
        {
            // 'out'.
            fileBytes = new byte[4] { 32, 32, 32, 32 };
            numBytes = 0;

            char[] charArray = new char[1];
            charArray[0] = charUTF8;

            Encoding encoding = Encoding.UTF8;

            byte[] temp = encoding.GetBytes(charArray);

            numBytes = temp.Length;

            for (int i = 0; i < numBytes; i++)
            {
                fileBytes[i] = temp[i];
            }

            encoding = Encoding.GetEncoding("windows-1252");

            return encoding.GetString(fileBytes);
        }



    }
}

```
