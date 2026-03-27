using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CompFxValidateOutputs
{
    class CompareResults
    {
        private static string mFilePath1 = null;
        private static string mFilePath2 = null;
        private static string mErrorMessage = null;

        private enum FILETYPE { UNKNOWN, FTVAL, FEVAL, FTPRI, FEPRI }

        private static FILETYPE mFileType = FILETYPE.UNKNOWN;
        private static int mCount = 1;

        static void Main(string[] args)
        {
            try
            {
#if true
                string mLog2FilePath = @"d:\users\ahulme\temp\CompareResults.log";
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

                Console.WriteLine();
                ParseCommandLineInput(args);

                string[] text1 = File.ReadAllLines(mFilePath1);
                string[] text2 = File.ReadAllLines(mFilePath2);

                bool textFilesMatch = IntelligentCompare(text1, text2);

                Console.Write("\nFiles are identical: " + textFilesMatch + "\n");

                int exitCode = 1;
                if (textFilesMatch) exitCode = 0;
                Environment.Exit(exitCode);
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException caught in Main():");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.Data);
                Console.WriteLine(e.GetType());
            }
        }

        private static void ParseCommandLineInput(string[] args)
        {
            if (args.Length == 2)
            {
                mFilePath1 = args[0];
                HardFailIfFileNotExist(mFilePath1);
                //HardFailIfFileEmpty(mFilePath1);
                //HardFailIfFileAllWhiteSpace(mFilePath1);
                //HardFailIfNotFtValidateOutput(mFilePath1);

                mFilePath2 = args[1];
                HardFailIfFileNotExist(mFilePath2);
                //HardFailIfFileEmpty(mFilePath2);
                //HardFailIfFileAllWhiteSpace(mFilePath2);
                //HardFailIfNotFtValidateOutput(mFilePath2);
            }
            else
            {
                string str = @"

Purpose:   Intelligent comparison of the contents of two 'validate' output files
           of the following types:

               - FtValidate output files;
               - FtPrint output files;

               - FeValidate output files;
               - FePrint output files;

Usage:     CompFxValidateOutputs  file1  file2

where:
           file1 :   path to 1st output file.
           file2 :   path to 2nd output file.";
                Console.WriteLine(str);
                Environment.Exit(666);
            }
        }

        private static void HardFailIfFileNotExist(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("\nFile not found:  " + path + "\n");
                Environment.Exit(666);
            }
        }

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

        private static bool IntelligentCompare(string[] text_A, string[] text_B)
        {
            Log2.v("\nCompareResults.IntelligentCompare(): Entry");

            bool textIsIdentical = true;

            List<ContentLine> contentLines_A = ScrapeContent(text_A);
            List<ContentLine> contentLines_B = ScrapeContent(text_B);

            if (contentLines_A.Count != contentLines_B.Count)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                sb.AppendLine("The two files have different non-blank line counts.");
                sb.AppendLine("    file1 has " + contentLines_A.Count + " non-blank lines.");
                sb.AppendLine("    file2 has " + contentLines_B.Count + " non-blank lines.");

                Console.Write(sb.ToString());

                textIsIdentical = false;
            }

            for (int i = 0; i < contentLines_A.Count; i++)
            {
                Log2.v("\n");
                Log2.v(String.Format("\n{0:D8} : {1}", contentLines_A[i].LineNumber, contentLines_A[i].Content));
                Log2.v(String.Format("\n{0:D8} : {1}", contentLines_A[i].LineNumber, contentLines_A[i].Content));

                string line1Sanit = SanitizeDecimalNumbers(contentLines_A[i].Content);
                string line2Sanit = SanitizeDecimalNumbers(contentLines_B[i].Content);

                if (!line1Sanit.Equals(line2Sanit))
                {
                    if (IgnoreTheseLines(contentLines_A[i].Content, contentLines_B[i].Content))
                    {
                        // Skip to next iteration of for-loop.
                        continue;
                    }
                    else  // We found a difference between two lines.
                    {
                        int charPosition = GetPositionOfFirstCharDifference(contentLines_A[i].Content, contentLines_B[i].Content);
                        Console.Write("\nDifference at lines: " + contentLines_A[i].LineNumber + ", " + contentLines_B[i].LineNumber + ",  character position " + charPosition + ".");
                        string line1Preamble = "\n    file_A, line " + contentLines_A[i].LineNumber + ":  ";
                        Console.Write(line1Preamble + contentLines_A[i].Content);
                        string line2Preamble = "\n    file_B, line " + contentLines_B[i].LineNumber + ":  ";
                        Console.Write(line2Preamble + contentLines_B[i].Content);
                        Console.Write(BuildPositionIndicatorString(charPosition, line1Preamble.Length));
                        textIsIdentical = false;
                        break;
                    }

                }
            }


            Log2.v("\nCompareResults.IntelligentCompare(): Exit: textIsIdentical = " + textIsIdentical);

            return textIsIdentical;
        }

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

        private static bool IgnoreTheseLines(string line1, string line2)
        {
            bool ignore = false;

            string[] patternArray = new string[8];
            patternArray[0] = "PDF VALIDATION REPORT";
            patternArray[1] = "RUN DATE:";
            patternArray[2] = "Project Code [";
            patternArray[3] = "PDF Validation Completed";
            patternArray[4] = "* TS-PDF:";
            patternArray[5] = "* ES-PDF NAME:";
            patternArray[6] = "getcoords -- build";
            patternArray[7] = "<title>HTML from";

            for (int i = 0; i < patternArray.Length; i++)
            {
                if (BothContain(line1, line2, patternArray[i]))
                {
                    ignore = true;
                    break;
                }
            }

            return ignore;
        }

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

        //First line is index zero.
        //Negative return value indicates that there is no non-trivial content.
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

        //First line is index zero.
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

        private static List<ContentLine> ScrapeContent(string[] allLines)
        {
            List<ContentLine> contentLines = new List<ContentLine>();

            for (int index = 0; index < allLines.Length; index++)
            {
                string candidate = allLines[index].Trim();
                bool keep = true;

                if (String.IsNullOrWhiteSpace(candidate)) keep = false;
                if (candidate.Contains("Build Configuration")) keep = false;

                if (keep)
                {
                    ContentLine contentLine = new ContentLine(candidate, index + 1);

                    contentLines.Add(contentLine);
                }
            }

            return contentLines;
        }

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

    }
}
