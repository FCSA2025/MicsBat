using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// This program 'intelligently' compares the contents of two MICS output reports (text files) and
/// returns an exit code of 0 if they are effectively identical, e.g. when dates, times
/// build types, blank lines etc are excluded from the comparison; this program was
/// first developed to compare TSIP reports and was later extended to compare FtValidate output
/// reports to enable automated regression testing.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - CompareReports.PNG" ""
/// </remarks>
namespace CompareReports
{
    /// <summary>
    /// This class provides the Main() method, and supporting methods, for the CompareReports application.
    /// </summary>
    class CompareReports
    {
        private static string mFilePath1 = null;
        private static string mFilePath2 = null;
        private static bool mExport = false;

        private static string mExportFilepath = Environment.GetEnvironmentVariable("USERPROFILE") + @"\CompareReportsExport.txt";

        private enum FILETYPE { UNKNOWN, FTVAL, FEVAL, FTPRI, FEPRI }

        private static FILETYPE mFileType = FILETYPE.UNKNOWN;
        private static int mCount = 1;

        /// <summary>
        /// This is the Main() method of the CompareReports application.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            try
            {
#if true
                string mLog2FilePath = @"d:\MicsBatchLogs\CompareReports.log";
                if (Log2.SetLogFilePath(mLog2FilePath))
                {
                    Log2.Erase();
                    Log2.Set(Log2.FileOpenClose.PER_SESSION);
                    Log2.Set(Log2.WriteMode.ENABLED);
                    Log2.Set(Log2.Level.VERBOSE);
                    Info.BuildMetaData = Info.CollateExeMetaData();
                    Log2.v("\nBuild: " + Info.ManagedBuildInfo());
                }
#endif
                ParseCommandLineArgs(args);

                string[] text1 = File.ReadAllLines(mFilePath1);
                string[] text2 = File.ReadAllLines(mFilePath2);

                if (!mExport)
                {
                    Console.Write("\nIntelligent comparison:");
                    Console.Write("\n\n    File A:   {0}", mFilePath1);
                    Console.Write("\n    File B:   {0}", mFilePath2);
                }

                string message;

                bool textFilesMatch = IntelligentCompare(text1, text2, out message);

                if (!mExport)
                {
                    Console.Write("\n\nFiles are identical: " + textFilesMatch);
                    Console.Write("\n\n\n" + message + "\n");
                }

                int exitCode = 1;
                if (textFilesMatch) exitCode = 0;
                Environment.Exit(exitCode);
            }
            catch (Exception e)
            {
                Log2.e("\n\nCompareReports.Main(): ERROR: exception: " + e.Message);
                Log2.e("\n\nCompareReports.Main(): ERROR: stack trace: \n" + e.StackTrace);
                Console.Write("\n\nERROR: runtime exception.");
            }
        }



        /// <summary>
        /// This method checks whether a file with a prescribed path already exists; if it does
        /// not then an error message is written to the console and the application exits with
        /// a non-zero exit code; if the '-e' command line option was prescribed then an error
        /// message is written to the 'export file' so as to support automated regression testing;
        /// the export file path is %USERPROFILE%&#92;CompareReportsExport&#46;txt".
        /// </summary>
        /// <param name="path"></param>
        private static void HardFailIfFileNotExist(string path)
        {
            if (!File.Exists(path))
            {
                if (mExport)
                {
                    string str = "";
                    string shim = mExport ? Strings.RepeatedChar(' ', 20) : "";
                    string aOrB = (path == mFilePath1) ? "A" : "B";

                    str += shim + "File_A:   " + mFilePath1;
                    str += "\n" + shim + "File_B:   " + mFilePath2 + "\n.";
                    str += "\n" + shim + "ERROR: File_" + aOrB + " cannot be found\n.";

                    // Creates a new file, write the contents to the file, and then closes the file. 
                    // If the target file already exists, it is overwritten.
                    File.WriteAllText(mExportFilepath, str);
                }
                else
                {
                    Console.WriteLine("\nFile not found:  " + path + "\n");
                }

                Environment.Exit(666);
            }
        }

        /// <summary>
        /// This method checks whether a file with a prescribed path is empty; if it is
        /// then an error message is written to the console and the application exits with
        /// a non-zero exit code.
        /// </summary>
        /// <param name="path"></param>
        private static void HardFailIfFileEmpty(string path)
        {
            if (File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                if (fi.Length == 0)
                {
                    Console.WriteLine("\nFile is empty:  " + path + "\n");
                    Environment.Exit(666);
                }
            }
        }

        /// <summary>
        /// This method checks whether a file with a prescribed path contains only whitespace characters; 
        /// if it does then an error message is written to the console and the application exits with
        /// a non-zero exit code.
        /// </summary>
        /// <param name="path"></param>
        private static void HardFailIfFileAllWhiteSpace(string path)
        {
            if (File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                if (fi.Length != 0)
                {
                    string content = File.ReadAllText(path);
                    char[] cArr = content.ToCharArray();
                    bool allWhiteSpace = true;
                    for (int i = 0; i < cArr.Length; i++)
                    {
                        if (!Char.IsWhiteSpace(cArr[i]))
                        {
                            allWhiteSpace = false;
                            break;
                        }
                    }
                    if (allWhiteSpace)
                    {
                        Console.WriteLine("\nFile contains no visible text:  " + path + "\n");
                        Environment.Exit(666);
                    }
                }
            }
        }

        /// <summary>
        /// This method parses the contents of a prescribed text file and checks if it
        /// contains a known string that infers its type; if no match is found the 
        /// application exits with a non-zero exit code.
        /// </summary>
        /// <param name="path"></param>
        private static void HardFailIfNotFtValidateOutput(string path)
        {
            const string FtValPattern = @"PDF VALIDATION REPORT - Build";
            const string FeValPattern = @"ES PDF VALIDATION REPORT    Build";
            const string FtPriPattern = @"\* TS-PDF.*By ftPrint:";
            const string FePriPattern = @"\* ES-PDF NAME:.*Produced by fePrint";

            // Read the file contents as one big string.
            string content = File.ReadAllText(path);

            string str = "";
            if (mCount == 1) str = "1st ";
            if (mCount == 2) str = "2nd ";

            if (Regex.IsMatch(content, FtValPattern))
            {
                mFileType = FILETYPE.FTVAL;
            }
            else if (Regex.IsMatch(content, FeValPattern))
            {
                mFileType = FILETYPE.FEVAL;
            }
            else if (Regex.IsMatch(content, FtPriPattern))
            {
                mFileType = FILETYPE.FTPRI;
            }
            else if (Regex.IsMatch(content, FePriPattern))
            {
                mFileType = FILETYPE.FEPRI;
            }
            else
            {
                mFileType = FILETYPE.UNKNOWN;
            }

            if (mFileType == FILETYPE.UNKNOWN)
            {
                Console.WriteLine("\nERROR:  " + str + "filePath:   " + path + "\n");
                Console.WriteLine("\nFile type is not one of ftPrint, fePrint, ftValidate or feValidate.\n");
                Environment.Exit(666);
            }
            else
            {
                Console.Write("\n" + str + "file type is : " + mFileType);
            }

            mCount++;
        }

        /// <summary>
        /// This is the top-level method, called in Main(), that begins the intelligent
        /// text comparison processing.
        /// </summary>
        /// <param name="text_A"></param>
        /// <param name="text_B"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool IntelligentCompare(string[] text_A, string[] text_B, out string message)
        {
            //...Log2.v("\nCompareReports.IntelligentCompare(): Entry");

            message = "";

            bool textIsIdentical = true;

            string shim = mExport ? Strings.RepeatedChar(' ', 20) : "";

            List<ContentLine> contentLines_A = ScrapeContent(text_A);
            List<ContentLine> contentLines_B = ScrapeContent(text_B);

            if (contentLines_A.Count != contentLines_B.Count)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("\nThe two files have different non-blank line counts.");
                sb.AppendLine("    file1 has " + contentLines_A.Count + " non-blank lines.");
                sb.AppendLine("    file2 has " + contentLines_B.Count + " non-blank lines.");

                //Console.Write(sb.ToString());
                message = sb.ToString();
                textIsIdentical = false;
            }

            int minCount = Math.Min(contentLines_A.Count, contentLines_B.Count);

            List<string> differences = new List<string>();

            int offset_A = 0;
            int offset_B = 0;

            for (int i = 0; i < minCount; i++)
            {
                int index_A = i + offset_A;
                int index_B = i + offset_B;

                // Guard against "Index was out of range" runtime exceptions.
                if (index_A > contentLines_A.Count - 1) index_A = contentLines_A.Count - 1;
                if (index_B > contentLines_B.Count - 1) index_B = contentLines_B.Count - 1;

                //...Log2.v("\n");
                //...Log2.v(String.Format("\n{0:D8} : {1}", contentLines_A[index_A].LineNumber, contentLines_A[index_A].Content));
                //...Log2.v(String.Format("\n{0:D8} : {1}", contentLines_B[index_B].LineNumber, contentLines_B[index_B].Content));

                string lineSanit_A = SanitizeDecimalNumbers(contentLines_A[index_A].Content);
                string lineSanit_B = SanitizeDecimalNumbers(contentLines_B[index_B].Content);

                if (!lineSanit_A.Equals(lineSanit_B))
                {
                    if (IgnoreTheseLines(contentLines_A[index_A].Content, contentLines_B[index_B].Content))
                    {
                        // Skip to next iteration of for-loop.
                        continue;
                    }
                    else  // We found a difference between two lines.
                    {
                        int charPosition = GetPositionOfFirstCharDifference(contentLines_A[index_A].Content, contentLines_B[index_B].Content);

                        string str = "";

                        string firstWord = differences.Count == 0 ? "First" : "Next";

                        if (differences.Count == 0)
                        {
                            firstWord = "First";

                            str += shim + "File_A:   " + mFilePath1;
                            str += "\n" + shim + "File_B:   " + mFilePath2 + "\n.";
                        }
                        else
                        {
                            firstWord = "Next";
                        }

                        str += String.Format("\n" + shim + firstWord + " difference at File_A line# " + contentLines_A[index_A].LineNumber + " and File_B line# " + contentLines_B[index_B].LineNumber + ",  character position " + (charPosition + 1) + ":");

                        str += "\n.";

                        string line1Preamble = "\n" + shim + "    File_A, line " + contentLines_A[index_A].LineNumber + ":  ";
                        str += String.Format(line1Preamble + contentLines_A[index_A].Content);

                        string line2Preamble = "\n" + shim + "    File_B, line " + contentLines_B[index_B].LineNumber + ":  ";
                        str += String.Format(line2Preamble + contentLines_B[index_B].Content);

                        str += String.Format(BuildPositionIndicatorString(charPosition, line1Preamble.Length));

                        textIsIdentical = false;

                        differences.Add(str);

                        // Look for a forward match recovery.
                        const int LOOK_AHEAD = 20;
                        int stepForMatch_A = ScanForward_A_ForMatchOn_B(index_A, index_B, LOOK_AHEAD, contentLines_A, contentLines_B);
                        int stepForMatch_B = ScanForward_B_ForMatchOn_A(index_A, index_B, LOOK_AHEAD, contentLines_A, contentLines_B);
                        if (stepForMatch_A > 0)
                        {
                            offset_A += stepForMatch_A;
                            //...Log2.v(String.Format("\n***** New line match: {0} <--> {1}", contentLines_A[i + offset_A].LineNumber, contentLines_B[i + offset_B].LineNumber));
                        }
                        else if (stepForMatch_B > 0)
                        {
                            offset_B += stepForMatch_B;
                            //...Log2.v(String.Format("\n***** New line match: {0} <--> {1}", contentLines_A[i + offset_A].LineNumber, contentLines_B[i + offset_B].LineNumber));
                        }

                        if (differences.Count > 4) break;
                    }
                }
            }


            foreach (string difference in differences) message += difference;

            //...Log2.v("\nCompareReports.IntelligentCompare(): Exit: textIsIdentical = " + textIsIdentical);

            if (mExport && (differences.Count > 0))
            {
                // Creates a new file, write the contents to the file, and then closes the file. 
                // If the target file already exists, it is overwritten.
                File.WriteAllText(mExportFilepath, differences[0]);
            }
            else
            {
                // Creates a new file, write the contents to the file, and then closes the file. 
                // If the target file already exists, it is overwritten.
                File.WriteAllText(mExportFilepath, "");
            }

            return textIsIdentical;
        }

        /// <summary>
        /// This method inputs a line of text and converts decimal substrings written
        /// by a 'C' program into the standard format used by C#.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string SanitizeDecimalNumbers(string line)
        {
            string result = line;

            const string PATTERN = @"(-{0,1})(\d{0,69})\.(\d{0,69})";

            MatchCollection matchColl = Regex.Matches(line, PATTERN);

            foreach (Match match in matchColl)
            {
                string str0 = match.Groups[0].Value;

                // Pathological case of pattern matching "."
                if (str0.Equals(".")) continue;

                try
                {
                    double d = Convert.ToDouble(str0);
                    float f = (float)d;

                    string substitute = String.Format("{0}", f);

                    result = result.Replace(str0, substitute);

                    //Console.Write("\n|{0}|{1}|", line, result);
                }
                catch (Exception e)
                {
                    Console.Write("\nEXCEPTION:  {0}\n{1}", e.Message, e.StackTrace);
                }
            }

            return result;
        }

        /// <summary>
        /// This method parses two prescribed lines of text and returns true if they can
        /// both be ignored because they contain date, time, build type, header text etc.
        /// </summary>
        /// <param name="lineA"></param>
        /// <param name="lineB"></param>
        /// <returns></returns>
        private static bool IgnoreTheseLines(string lineA, string lineB)
        {
            bool ignore = false;

            // These are the simple ignores w.r.t. "does the line contain this string?".
            string[] patternArray = new string[12];
            patternArray[0] = "PDF VALIDATION REPORT";
            patternArray[1] = "RUN DATE:";
            patternArray[2] = "Project Code [";
            patternArray[3] = "PDF Validation Completed";
            patternArray[4] = "* TS-PDF:";
            patternArray[5] = "* ES-PDF NAME:";
            patternArray[6] = "getcoords -- build";
            patternArray[7] = "<title>HTML from";
            patternArray[8] = "Audit Trail";
            patternArray[9] = "***  PDF Name:";
            patternArray[10] = "PDF POST VALIDATION REPORT"; // Vch.exe
            patternArray[11] = "Build";

            for (int i = 0; i < patternArray.Length; i++)
            {
                if (BothContain(lineA, lineB, patternArray[i]))
                {
                    ignore = true;
                    break;
                }
            }
#if true
            // These are the more sophisticated ignores.
            // SK records, e.g.: SK,U,U,%TLUSBCX8,ISHKHEENICKH PAS,55-00-23.26N,129-34-58.49W,347.0,2020.10.29,12:15
            // Ignore the date and time but compare the remaining fields.
            if (lineA.Trim().StartsWith("SK") && lineB.Trim().StartsWith("SK"))
            {
                //...Log2.v("\nSK record: start");
                //...Log2.v("\nSK record: lineA = " + lineA);
                //...Log2.v("\nSK record: lineB = " + lineB);

                // Parse the lines as a CSV record.
                string[] fieldsA = lineA.Split(',');
                string[] fieldsB = lineB.Split(',');

                // SK records has 10 fields (9 commas).
                if ((fieldsA.Length == fieldsB.Length) && (fieldsA.Length == 10))
                {
                    //...Log2.v("\nSK record: correct number of fields");
                    // Check the first 8 fields for equality.
                    ignore = true;
                    for (int i = 0; i <= 7; i++)
                    {
                        // If a non-date and/or non-time field is different then we can't ignore the two lines.
                        if (!fieldsA[i].Equals(fieldsB[i]))
                        {
                            //...Log2.v("\nSK record: fields not equal: index = " + i);
                            ignore = false;
                            break;
                        }
                    }
                    //...Log2.v("\nSK record: ignore = " + ignore);
                }
            }

            // AK records, e.g.: AK,U,U,%TLUSBCX9,CGF983A,11A,11,2019.09.19,17:42
            // Ignore the date and time but compare the remaining fields.
            if (lineA.Trim().StartsWith("AK") && lineB.Trim().StartsWith("AK"))
            {
                //...Log2.v("\nAK record: start");
                //...Log2.v("\nAK record: lineA = " + lineA);
                //...Log2.v("\nAK record: lineB = " + lineB);

                // Parse the lines as a CSV record.
                string[] fieldsA = lineA.Split(',');
                string[] fieldsB = lineB.Split(',');

                // AK records have 9 fields (8 commas).
                if ((fieldsA.Length == fieldsB.Length) && (fieldsA.Length == 9))
                {
                    //...Log2.v("\nAK record: correct number of fields");
                    // Check the first 7 fields for equality.
                    ignore = true;
                    for (int i = 0; i <= 6; i++)
                    {
                        // If a non-date and/or non-time field is different then we can't ignore the two lines.
                        if (!fieldsA[i].Equals(fieldsB[i]))
                        {
                            //...Log2.v("\nAK record: fields not equal: index = " + i);
                            ignore = false;
                            break;
                        }
                    }
                    //...Log2.v("\nAK record: ignore = " + ignore);
                }
            }

            // CK records, e.g.: CK,U,U,CGF983A,%TLUSBCX9,11A,1001,2019.09.19,17:42
            // Ignore the date and time but compare the remaining fields.
            if (lineA.Trim().StartsWith("CK") && lineB.Trim().StartsWith("CK"))
            {
                //...Log2.v("\nCK record: start");
                //...Log2.v("\nCK record: lineA = " + lineA);
                //...Log2.v("\nCK record: lineB = " + lineB);

                // Parse the lines as a CSV record.
                string[] fieldsA = lineA.Split(',');
                string[] fieldsB = lineB.Split(',');

                // CK records have 9 fields (8 commas).
                if ((fieldsA.Length == fieldsB.Length) && (fieldsA.Length == 9))
                {
                    //...Log2.v("\nCK record: correct number of fields");
                    // Check the first 7 fields for equality.
                    ignore = true;
                    for (int i = 0; i <= 6; i++)
                    {
                        // If a non-date and/or non-time field is different then we can't ignore the two lines.
                        if (!fieldsA[i].Equals(fieldsB[i]))
                        {
                            //...Log2.v("\nCK record: fields not equal: index = " + i);
                            ignore = false;
                            break;
                        }
                    }
                    //...Log2.v("\nCK record: ignore = " + ignore);
                }
            }
#endif
            return ignore;
        }

        /// <summary>
        /// This method returns true if a pair of prescribed strings contain a prescribed 
        /// pattern as a substring.
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private static bool BothContain(string line1, string line2, string pattern)
        {
            bool bothContain = false;

            bool line1Contains = line1.Contains(pattern);
            bool line2Contains = line2.Contains(pattern);
            if (line1Contains && line2Contains)
            {
                bothContain = true;
            }

            return bothContain;
        }

        /// <summary>
        /// This method parses lines of text in the form of an array of strings
        /// and returns the index of the first array element (line number) that
        /// contains non-trivial text; line numbers commence at zero; a negative 
        /// return value indicates that all lines just contained white space.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static int GetIndexFirstLineWithContent(string[] text)
        {
            int index = -666;
            for (int i = 0; i < text.Length; i++)
            {
                string line = text[i];
                bool lineHasNonWhiteSpaceChar = false;
                foreach (char c in line)
                {
                    if (!Char.IsWhiteSpace(c))
                    {
                        lineHasNonWhiteSpaceChar = true;
                        break;
                    }
                }
                if (lineHasNonWhiteSpaceChar)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        /// <summary>
        /// This method returns the line number (array element index) of the last
        /// line that has  any non-white space content; line numbers start at zero.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static int GetIndexLastLineWithContent(string[] text)
        {
            int index = -666;
            for (int i = 0; i < text.Length; i++)
            {
                int revIndex = text.Length - 1 - i;
                string line = text[revIndex];

                bool lineHasNonWhiteSpaceChar = false;
                foreach (char c in line)
                {
                    if (!Char.IsWhiteSpace(c))
                    {
                        lineHasNonWhiteSpaceChar = true;
                        break;
                    }
                }
                if (lineHasNonWhiteSpaceChar)
                {
                    index = revIndex;
                    break;
                }
            }
            return index;
        }

#if true
        private static bool SameNumberOfLines(int numLines1, int numLines2)
        {
            bool sameNumberOfLines = false;
            if (numLines1 == numLines2)
            {
                sameNumberOfLines = true;
                mErrorMessage = "";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                sb.AppendLine("The two files have different non-blank line counts.");
                sb.AppendLine("    file1 has " + numLines1 + " non-blank lines.");
                sb.AppendLine("    file2 has " + numLines2 + " non-blank lines.");
                mErrorMessage = sb.ToString();
                Application.ExitQuietly(666);
            }
            return sameNumberOfLines;
        }
#endif
        /// <summary>
        /// This method compares two prescribed strings, character-by-character, and
        /// returns the index of the first character position that does not match; the
        /// first character position has index zero.
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        private static int GetPositionOfFirstCharDifference(string line1, string line2)
        {
            int charPos = -1;
            char[] charArray1 = line1.ToCharArray();
            char[] charArray2 = line2.ToCharArray();
            int minLength = Math.Min(charArray1.Length, charArray2.Length);

            // Find any char difference over index 0 to minLength-1.
            for (int i = 0; i < minLength; i++)
            {
                char c1 = charArray1[i];
                char c2 = charArray2[i];
                if (!(c1 == c2))
                {
                    charPos = i;
                    break;
                }
            }

            // Handle the case of unequal length strings.
            if (charPos < 0) charPos = minLength;

            return charPos;
        }

        /// <summary>
        /// This method builds a string that is written to the console to provide
        /// a visual indication of a specific character position as an ergonomic aid.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private static string BuildPositionIndicatorString(int pos, int offset)
        {
            StringBuilder posIndStr = new StringBuilder();
            StringBuilder spaces = new StringBuilder();
            for (int i = 0; i < pos + offset - 1; i++)
            {
                spaces.Append(" ");
            }

            posIndStr.Append("\n" + spaces);
            posIndStr.Append("|");
            posIndStr.Append("\n" + spaces);
            posIndStr.Append("^\n");
            return posIndStr.ToString();
        }

        /// <summary>
        /// This method returns the index of the first string array element that contains
        /// the substring "PDF Validation Completed".
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private static int IndexOfPDFValidationCompleted(string[] lines)
        {
            int index = -666;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("PDF Validation Completed"))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0) index = lines.Length - 1;

            return index;
        }

        /// <summary>
        /// This method returns the index of the first string array element that contains
        /// the substring "PDF Name:".
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private static int IndexOfPDFname(string[] lines)
        {
            int index = -666;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("PDF Name:"))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0) index = lines.Length - 1;

            return index;
        }

        /// <summary>
        /// This methods inputs a prescribed array of strings and returns a reduced array
        /// of strings that excludes any line containing specific substrings.
        /// </summary>
        /// <param name="allLines"></param>
        /// <returns></returns>
        private static List<ContentLine> ScrapeContent(string[] allLines)
        {
            List<ContentLine> contentLines = new List<ContentLine>();

            for (int index = 0; index < allLines.Length; index++)
            {
                string candidate = allLines[index].Trim();
                bool keep = true;

                if (String.IsNullOrWhiteSpace(candidate)) keep = false;

                if (candidate.StartsWith("//")) keep = false;

                if (candidate.Contains("Validate.exe: Build Configuration")) keep = false;

                if (candidate.Contains("Vch.exe: Build Configuration:")) keep = false;

                if (candidate.Contains("Vch -- The PDF table set is validated for")) keep = false;

                if (keep)
                {
                    ContentLine contentLine = new ContentLine(candidate, index + 1);

                    contentLines.Add(contentLine);
                }
            }

            return contentLines;
        }

        /// <summary>
        /// This method performs a 'look ahead' matching of elements in a prescribed string array "A" 
        /// relative to a prescribed fixed element of a second string array "B", subject to a 'maximum look ahead' constraint.
        /// </summary>
        /// <param name="startIndex_A"></param>
        /// <param name="startIndex_B"></param>
        /// <param name="maxLookAhead"></param>
        /// <param name="cLines_A"></param>
        /// <param name="cLines_B"></param>
        /// <returns></returns>
        private static int ScanForward_A_ForMatchOn_B(int startIndex_A, int startIndex_B, int maxLookAhead, List<ContentLine> cLines_A, List<ContentLine> cLines_B)
        {
            //...Log2.v(String.Format("\nScanForward_A_ForMatchOn_B: Entry: {0},  {1},  {2}", startIndex_A, startIndex_B, maxLookAhead));

            int step = -1;
            string line_B = cLines_B[startIndex_B].Content;

            for (int offset = 0; offset < maxLookAhead; offset++)
            {
                int index = startIndex_A + offset;

                if (index >= cLines_A.Count)
                {
                    //...Log2.v(String.Format("\nScanForward_A_ForMatchOn_B: List cLines_A count exceeded: step = {0}", step));
                    return step;
                }

                string line_A = cLines_A[index].Content;

                if (line_A.Equals(line_B))
                {
                    step = offset;
                    break;
                }
            }

            //...Log2.v(String.Format("\nScanForward_A_ForMatchOn_B: Exit: step = {0}", step));

            return step;
        }

        /// <summary>
        /// This method performs a 'look ahead' matching of elements in a prescribed string array "B" 
        /// relative to a prescribed fixed element of a second string array "A", subject to a 'maximum look ahead' constraint.
        /// </summary>
        /// <param name="startIndex_A"></param>
        /// <param name="startIndex_B"></param>
        /// <param name="maxLookAhead"></param>
        /// <param name="cLines_A"></param>
        /// <param name="cLines_B"></param>
        /// <returns></returns>
        private static int ScanForward_B_ForMatchOn_A(int startIndex_A, int startIndex_B, int maxLookAhead, List<ContentLine> cLines_A, List<ContentLine> cLines_B)
        {
            //...Log2.v(String.Format("\nScanForward_B_ForMatchOn_A: Entry: {0},  {1},  {2}", startIndex_A, startIndex_B, maxLookAhead));

            int step = -1;
            string line_A = cLines_A[startIndex_A].Content;

            for (int offset = 0; offset < maxLookAhead; offset++)
            {
                int index = startIndex_B + offset;

                if (index >= cLines_B.Count)
                {
                    //...Log2.v(String.Format("\nScanForward_B_ForMatchOn_A: List cLines_B count exceeded: step = {0}", step));
                    return step;
                }

                string line_B = cLines_B[index].Content;

                if (line_A.Equals(line_B))
                {
                    step = offset;
                    break;
                }
            }

            //...Log2.v(String.Format("\nScanForward_B_ForMatchOn_A: Exit: step = {0}", step));

            return step;
        }

        /// <summary>
        /// This simple class definition encapsulates the concept of a line of text having
        /// a line number and an associated string.
        /// </summary>
        public class ContentLine
        {
            private string mContent;
            private int mLineNumber;

            public string Content { get { return mContent; } set { mContent = value; } }
            public int LineNumber { get { return mLineNumber; } set { mLineNumber = value; } }

            private ContentLine() { }

            public ContentLine(string content, int lineNum)
            {
                mContent = content;
                mLineNumber = lineNum;
            }
        } // class ContentLine

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program compares the contents of two MICS output reports (text files) and         +");
            Console.Write("\r\n returns an exit code of 0 if they are 'effectively' identical, e.g. when dates, times, +");
            Console.Write("\r\n build types, blank lines etc are excluded from the comparison. This program was        +");
            Console.Write("\r\n first developed to compare TSIP reports and was later extended to compare FtValidate   +");
            Console.Write("\r\n output reports to enable automated regression testing. ");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: CompareReports <filepath1> <filePath2> [-e]");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        filepath1     : full path to the 1st report to be compared.");
            Console.Write("\r\n        filepath2     : full path to the 2nd report to be compared.");
            Console.Write("\r\n");
            Console.Write("\r\n        --- Options -----------------------------------------------");
            Console.Write("\r\n");
            Console.Write("\r\n        -e            : instead of writing error messages to the console write ");
            Console.Write("\r\n                        them to an 'export' file whose path is %USERPROFILE%\\CompareReportsExport.txt");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     CompareReports d:\\users\\ahulme\\b123rft_OLD.txt d:\\users\\ahulme\\b123rft_NEW.txt");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with qty. 2 command-line arguments.");
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
                Application.ExitQuietly(666);
            }

            for (int i = 0; i < args.Length; i++)
            {
                //...Log2.v(String.Format("\nCompareReports.ParseCommandLineArgs(): arg[{0}] = {1}", i, args[i]));
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
                    Application.ExitQuietly(667);
                }

                // Process the flags.
                string flag = Strings.DropFirstChar(arg).ToUpper();
                switch (flag)
                {
                    case "E":
                        mExport = true;
                        break;
                    default:
                        Console.Write("\r\n Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(668);
                        break;
                }

            }

            // Parse the regular arguments; there must be qty. 2 of them.
            if (regularArgs.Count != 2)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(669);
            }

            // Parse the args.
            mFilePath1 = regularArgs[0];
            mFilePath2 = regularArgs[1];


            //...Log2.v("\nCompareReports.ParseCommandLineArgs(): mFilePath1 = " + mFilePath1);
            //...Log2.v("\nCompareReports.ParseCommandLineArgs(): mFilePath2 = " + mFilePath2);
            //...Log2.v("\nCompareReports.ParseCommandLineArgs(): mExport    = " + mExport);

            HardFailIfFileNotExist(mFilePath1);
            HardFailIfFileNotExist(mFilePath2);
        }

    }
}


