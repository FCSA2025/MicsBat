# Documented File: MicsSolnSourceCodeHashes.cs
**Repository Path:** `Tools\MicsSolnSourceCodeHashes.cs`
**Primary Layer:** `Tools`
**Namespace:** `Tools`

## Source Code Representation
```csharp
﻿using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tools
{
    public class MicsSolnSourceCodeHashes
    {
        private const string pathToSourceCodeFilesList = @"D:\MicsBatchLogs\MICS#SourceCodeFiles.txt";
        private const string pathToHashResultsFile = @"D:\MicsBatchLogs\MICS#SourceCodeFileHashes.txt";
        private const string tempFilePath = @"D:\MicsBatchLogs\temp.txt";
        public static void Go(string[] args)
        {
            try
            {
                string[] filePaths = File.ReadAllLines(pathToSourceCodeFilesList);

                TextWriter tw = new StreamWriter(pathToHashResultsFile, false);

                int maxPathLength = 0;

                foreach(string filePath in filePaths)
                {
                    if (filePath.Length > maxPathLength) maxPathLength = filePath.Length;

                    string contentsStr = CodeParser.ReadFileAsTrimmedString(filePath);
                    string hashBytesStr = "File not found.";

                    if (!String.IsNullOrWhiteSpace(contentsStr))
                    {
                        //Strip all C and C++ type comments.
                        contentsStr = CodeParser.StripComments(contentsStr);

                        //contentsStr = CodeParser.ReplaceTabsWithSpaces(contentsStr);

                        //Remove any "#if false .... #endif" blocks that are ignored by the preprocessor.
                        string pattern = @"#if\s+false(.|\n)*?#endif";
                        contentsStr = Regex.Replace(contentsStr, pattern, "");

                        // Change any instances of \r\n\r\n to \r\n\
                        contentsStr = contentsStr.ToString().Replace("\r\n\r\n", "\r\n");
                        //string[] lines = File.ReadAllLines(filePath);


                        //byte[] allBytesInFile = File.ReadAllBytes(filePath);
                        byte[] allBytesInFile = Encoding.ASCII.GetBytes(contentsStr);

                        // Compute the MD5 hash of the file contents.
                        byte[] hashBytes = new MD5CryptoServiceProvider().ComputeHash(allBytesInFile);

                        hashBytesStr = HashBytesToString(hashBytes);
                    }

                    tw.Write("\n{0, -70} {1}", filePath, hashBytesStr);
                }

                tw.Close();

                Console.Write("\nmaxPathLength = {0}", maxPathLength);

                string path = @"d:\users\ahulme\MICS#\Tools\Tools.cs";
                Console.Write("\nMD5 hash of {0}  :   {1}", path, GetMD5OfFile(path));
            }
            catch (Exception e)
            {
                Console.Error.Write("\nERROR: MicsSolnSourceCodeHashes.Go(): exception: {0}\n{1}", e.Message, e.StackTrace);
            }
        }

        public static string HashBytesToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i < arrInput.Length; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }

            return sOutput.ToString();
        }

        /// <summary>
        /// This method returns the MD5 hash string for the contents of a prescribed file path.
        /// </summary>
        /// <returns></returns>
        public static string GetMD5OfFile(string filePath)
        {
            try
            {
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = File.ReadAllBytes(filePath);

                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    // Convert the byte array to hexadecimal string
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("X2"));
                    }
                    return sb.ToString();
                }

            }
            catch (Exception)
            {
            }
            return "File not found.";
        }





    }
}

```
