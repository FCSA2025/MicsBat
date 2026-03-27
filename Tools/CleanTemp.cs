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
    /// <summary>
    /// Clean the directory %TEMP% of any garbage files and folders.
    /// </summary>
    public class CleanTemp
    {
        public static List<string> validExtensions = new List<string>() {
             ".txt", ".log", ".csv", ".zip", ".xls", ".xlsx", ".xlsm", ".xlsb", ".xltx", ".bat", ".cs", ".sln", ".csproj", ".sql", ".kml" };
        public static void Go(string[] args)
        {
            // Get the %TEMP% directory path.
            string tempDirPath = Environment.GetEnvironmentVariable("TEMP");

            if (String.IsNullOrWhiteSpace(tempDirPath))
            {
                Console.Write("\n\nTools.CleanTemp.Go(): ERROR: %TEMP% is not set.");
                return;
            }

            // Check that it is a valid directory path.
            if (!Directory.Exists(tempDirPath))
            {
                Console.Write("\n\nTools.CleanTemp.Go(): ERROR: %TEMP% is not a valid directory path: {0}", tempDirPath);
                return;
            }

            // We have a valid %TEMP% directory path.
            // Create a list of all tier-1 subdirectories.
            string[] subDirPaths = Directory.GetDirectories(tempDirPath);

            // The junk subdirectries have names that contain a single dot character ('.').
            foreach (string subDirPath in subDirPaths)
            {
                string dirName = Path.GetFileName(subDirPath);

                if (dirName.Contains("."))
                {
                    Console.Write("\nDeleting sub-directory: {0}", dirName);

                    // The following call deletes the specified directory and subdirectories.
                    Directory.Delete(subDirPath, true);
                }
            }

            // Get a list of all files in %TEMP%.
            string[] filePaths = Directory.GetFiles(tempDirPath);

            // Delete all files beginning with dd_ and having extension .log
            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileName(filePath);
                if (Regex.IsMatch(fileName, @"dd_[^.]+\.log"))
                {
                    Console.Write("\nDeleting file: {0}", fileName);
                    File.Delete(filePath);
                }
            }

            // Delete all files with extension .tmp
            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileName(filePath);
                if (Regex.IsMatch(fileName, @"[^.]+\.tmp"))
                {
                    Console.Write("\nDeleting file: {0}", fileName);
                    File.Delete(filePath);
                }
            }

            // Delete all files beginning with '~'.
            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileName(filePath);
                if (fileName[0] == '~')
                {
                    Console.Write("\nDeleting file: {0}", fileName);
                    File.Delete(filePath);
                }
            }

            // Delete all files with wacky extensions.
            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileName(filePath);
                string fileExtension = Path.GetExtension(filePath);
                if (!Strings.IsInListCaseInsensitive(validExtensions, fileExtension))
                {
                    Console.Write("\nDeleting file: {0}", fileName);
                    File.Delete(filePath);
                }
            }
        }





        }
}
