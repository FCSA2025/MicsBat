using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tools
{
    /// <summary>
    /// TBD
    /// </summary>
    public class TestFeImport
    {
        // Files, Directories, Accounts, DBs.
        const string MICS_RELEASE_BIN_DIR = @"D:\Users\ahulme\MICS#\_bin\Release";
        //const string MICS_RELEASE_BIN_DIR = @"D:\Users\fmda";
        const string MICS_DEBUG_BIN_DIR = @"D:\Users\ahulme\MICS#\_bin\Debug";
        //const string PROD_BIN_DIR = @"D:\prod\bin";
        const string PROD_BIN_DIR = @"D:\Users\ahulme\FCSA_Native_Progs";
        const string FEIMPORT_MICS_RELEASE_EXE = MICS_RELEASE_BIN_DIR + @"\FeImport.exe";
        const string FEIMPORT_MICS_DEBUG_EXE = MICS_DEBUG_BIN_DIR + @"\FeImport.exe";
        const string FEIMPORT_EXE = PROD_BIN_DIR + @"\ftImport.exe";
        const string FEIMPORT_PROD_EXE = PROD_BIN_DIR + @"\FeImport.exe";
        const string DB = " fcsa";
        const string PROJECT_CODE = " HULME1_0";
        //const string TS_PDF_DIR = @"D:\Users\ahulme\MICS#\TestData\TS\PDFs";

        //const string TS_PDF_LIST_FILE = @"D:\Users\ahulme\MICS#\TestData\TS\PDFs\FeImport_datafilesList.txt";
        //const string TS_PDF_LIST_FILE = @"D:\Users\ahulme\MICS#\TestData\TS\PDFs\Everything.txt";
        private static string TS_PDF_LIST_FILE;

        const string TS_PROD_BIN_OUT_DIR = @"D:\Users\ahulme\MICS#\TestData\TS\ProdBin_Results";
        const string TS_MICS_CS_OUT_DIR = @"D:\Users\ahulme\MICS#\TestData\TS\MICS#_Results";
        const string INETPUB_DIR = @"D:\inetpub\micstest\mics\userdirs\hulme\hulme1";
        private static string TEMP = Environment.GetEnvironmentVariable("TEMP");

        //private static bool runProdBin = false;
        //private static bool runMicsCs = false;
        private static bool singleShotMode = false;
        private static bool verbose = false;
        //private static bool logging = false;
        //private static bool reImportPDF = false;  // Applies only to case of neither -p nor -m being requested.
        private static string singleShotPDF = "";
        private static string lastStdOut = "";
        private static Enums.BuildConfig config = Enums.BuildConfig.RELEASE;

        private enum SF { SUCCESS, FAIL, UNKNOWN };

        private class TestData
        {
            public string pdfPath = "";
            public string pdfName = "";
            public SF correctResult = SF.UNKNOWN;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="args"></param>
        public static void Test_FeImport(string[] args)
        {
            try
            {
                // Process command-line arguments, if any.
                ProcessArgs(args);

                // Get the paths of all available PDFs.
                //mPdfPaths = File.ReadAllLines(TS_PDF_LIST_FILE);
                TestData[] testData = ParseListOfTestFiles(TS_PDF_LIST_FILE);

                if (verbose)
                {
                    for (int i = 0; i < testData.Length; i++)
                    {
                        Console.WriteLine(testData[i].correctResult.ToString().PadRight(12) + testData[i].pdfName.PadRight(48) + testData[i].pdfPath);
                    }
                }

                int testCount = 0;
                int passCount = 0;
                int failCount = 0;
                int noResultCount = 0;
                int inValidFileCount = 0;
                string outcome;

                // This loop iterates over all PDFs.
                for (int i = 0; i < testData.Length; i++)
                {
                    testCount++;

                    string pdfPath = testData[i].pdfPath;
                    string pdfName = testData[i].pdfName;
                    SF correctResult = testData[i].correctResult;

                    string destName = MakeDestName(pdfPath);

                    if (singleShotMode && !(pdfName.ToLower().Equals(singleShotPDF.ToLower())))
                    {
                        continue;
                    }

                    Console.Write("\n=============== " + pdfName + " =========================================================\n");

                    Console.Error.Write("\n\n" + pdfName.PadRight(40));

                    // Construct the command-line arguments for FeImport as a single string.
                    string command = FEIMPORT_MICS_RELEASE_EXE;
                    string commandLineArgs = CreateCommandLineArgs(destName, pdfPath, config);

                    // Execute FtImport in a Windows Shell and collect stdout,
                    // stderr and exitCode outputs.
                    string stdOutData;
                    string stdErrData;
                    int exitCode;

                    WindowsShell.RunCommand(command,
                                            commandLineArgs,
                                            out stdOutData,
                                            out stdErrData,
                                            out exitCode
                                            );

                    // Write the full command line and stdOut, stdErr and exitCode
                    // data to Console.Out.
                    Console.Write("\n" + command + commandLineArgs);
                    Console.Write(stdOutData);
                    Console.Write(stdErrData);

                    string str = exitCode == 0 ? " (SUCCESSFUL import)" : " (import FAILED)";
                    Console.Write("FeImport exitCode = " + exitCode + str + "\n");

                    // Collate a summary message and write it to Console.Error

                    if (exitCode == Constant.SUCCESS)
                    {
                        outcome = "Imported OK:                 ";
                    }
                    else
                    {
                        //"Import Succeeded:            "
                        outcome = String.Format("Import failed (err = {0,5}): ", exitCode);
                    }

                    string testResult = "";

                    bool passed = (exitCode == 0) && (correctResult == SF.SUCCESS)
                                  ||
                                  (exitCode != 0) && (correctResult == SF.FAIL);

                    if (exitCode == Error.IMPNOTEXIST)
                    {
                        inValidFileCount++;
                        testResult = "INVALID IMPORT FILE PATH";
                    }
                    else if (exitCode == Error.FILEISEMPTY)
                    {
                        inValidFileCount++;
                        testResult = "IMPORT FILE IS EMPTY";
                    }
                    else if (exitCode == Error.IMPORTFILEHASNOPARSABLECONTENT)
                    {
                        inValidFileCount++;
                        testResult = "IMPORT FILE HAS NO PARSABLE CONTENT";
                    }
                    else if (correctResult == SF.UNKNOWN)
                    {
                        noResultCount++;
                        testResult = "NO_RESULT";
                    }
                    else if (passed)
                    {
                        passCount++;
                        testResult = outcome + "      Test = PASS     ";
                    }
                    else
                    {
                        failCount++;
                        testResult = outcome + "      Test = FAIL <-----------";
                    }

                    Console.Error.Write(testResult);

                }  // end of loop-over-allPDFs

                Console.WriteLine("\r\nPDF testData files found: " + testData.Length + "\r\n\r\n");

                Console.Error.Write("\n");
                Console.Error.Write(String.Format("\nTOTAL TESTS         = {0}", testCount));
                Console.Error.Write(String.Format("\nTOTAL FILE PROBLEMS = {0}", inValidFileCount));
                Console.Error.Write(String.Format("\nTOTAL NO RESULT     = {0}", noResultCount));
                Console.Error.Write(String.Format("\n\nTOTAL PASSED        = {0}", passCount));
                Console.Error.Write(String.Format("\nTOTAL FAILED        = {0}", failCount));

                //Console.Error.Write(String.Format("\nTOTAL NO_RESULT = {0}", noResultCount));

                //CommandLineTask(@"D:\Users\ahulme\bin\CleanProdFilesRead.exe -v");

                Environment.Exit(666);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static TestData[] ParseListOfTestFiles(string filePath)
        {
            List<TestData> testDataList = new List<TestData>();

            string[] allLines = File.ReadAllLines(filePath);

            int lineNum = 0;
            foreach (string line in allLines)
            {
                lineNum++;

                string candidate = line.Trim();

                candidate = candidate.Replace(@"""", "");  // Purge any quotation marks.

                if (!String.IsNullOrWhiteSpace(candidate))
                {
                    if (candidate.StartsWith("//"))  // Strip comments.
                    {
                        // Skip to next line.
                        continue;
                    }

                    // Check that we have a CSV list with just one comma.
                    string[] fields = line.Split(',');
                    if (fields.Length != 2)
                    {
                        Console.Error.Write("\nERROR: invalid syntax: line {0} of file: {1}", lineNum, filePath);
                        continue;
                    }

                    TestData td = new TestData();
                    switch (fields[0].Trim().ToUpper())
                    {
                        case "F":
                            td.correctResult = SF.FAIL;
                            break;
                        case "S":
                            td.correctResult = SF.SUCCESS;
                            break;
                        default:
                            td.correctResult = SF.UNKNOWN;
                            Console.Error.Write("\nERROR: invalid syntax: line {0} of file: {1}", lineNum, filePath);
                            break;
                    }

                    if (td.correctResult != SF.UNKNOWN)
                    {
                        td.pdfPath = fields[1].Trim();
                        td.pdfName = GetPdfName(td.pdfPath);

                        testDataList.Add(td);
                    }
                }
            }

            return testDataList.ToArray();
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="args"></param>
        public static void ProcessArgs(string[] args)
        {
            //Console.WriteLine("args.Length = " + args.Length);
            if (args.Length == 0)
            {
                WriteCliUsageMessage();
                Environment.Exit(666);
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].ToLower().StartsWith("-o:"))
                    {
                        singleShotMode = true;
                        singleShotPDF = args[i].ToLower().Replace("-o:", "");
                        Console.WriteLine("singleShotPDF = " + singleShotPDF);
                    }
                    else if (args[i].ToLower().Equals("-v"))
                    {
                        verbose = true;
                    }
                    else if (args[i].ToLower().StartsWith("-f:"))
                    {
                        singleShotMode = false;
                        TS_PDF_LIST_FILE = args[i].ToLower().Replace("-f:", "");
                    }
                    else
                    {
                        Console.WriteLine("ERROR: unknown command line argument: " + args[i]);
                        WriteCliUsageMessage();
                        Environment.Exit(666);
                    }
                }
            }
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name=""></param>
        public static void WriteCliUsageMessage()
        {
            const string TAB1 = "    ";
            const string TAB2 = "        ";
            StringBuilder sb = new StringBuilder("\r\n\r\n");
            sb.AppendLine(TAB1 + "Usage:    testing FeImport [-L] [-f:<FilePath>] [-o:<PDFfilePath>] [-v]");
            sb.AppendLine();
            sb.AppendLine(TAB1 + "Options:");
            sb.AppendLine(TAB2 + "-f:   " + "File pathname containing a list of PDFs to test with.");
            sb.AppendLine(TAB2 + "-o:   " + "ONLY use single PDF as prescribed by <PDFfilePath>");
            sb.AppendLine(TAB2 + "-v" + TAB1 + "VERBOSE.");
            sb.AppendLine();

            Console.WriteLine(sb.ToString());

        }
        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="commandLine"></param>
        public static int CommandLineTask(string commandLine)
        {
            Process cmd = new Process();

            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;

            cmd.Start();

            cmd.StandardInput.WriteLine(commandLine);

            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();

            cmd.WaitForExit();

            string output = cmd.StandardOutput.ReadToEnd();

            lastStdOut = StripFirstAndLastLines(output, 0, 0);
            Console.WriteLine(lastStdOut);

            int appExitCode = Int32.MinValue;
            string pattern = @"Application\.Exit\(\): exit code = ([-|0-9]+)";
            Match match = Regex.Match(lastStdOut, pattern);
            if (match.Success)
            {
                string str = match.Groups[1].Value;
                if (!int.TryParse(str, out appExitCode))
                {
                    appExitCode = int.MinValue;
                }
            }

            //Console.Error.Write("\nappExitCode = " + appExitCode);
            return appExitCode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pdfPath"></param>
        /// <returns></returns>
        private static string GetPdfName(string pdfPath)
        {
            string pdfName = "";
            const string pattern = @"((\w|[ ()])+)\.txt";

            Match match = Regex.Match(pdfPath.ToLower(), pattern);
            if (match.Success)
            {
                pdfName = Regex.Replace(match.Value, @"\.txt", "");
            }
            else
            {
                pdfName = "No_Match";
            }

            return pdfName;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="pdfName"></param>
        /// <returns></returns>
        private static string CreateCommandLineForProdBin(string pdfName)
        {
            // %MICSEXEPATH%\ftValidate.exe fcsa HULME1_0 -oD:\inetpub\micstest\mics\userdirs\hulme\hulme1\%1.out -LD:\inetpub\micstest\mics\userdirs\hulme\hulme1\ftValidate.log  -v %1)
            StringBuilder sb = new StringBuilder();
            sb.Append(FEIMPORT_PROD_EXE);
            sb.Append(DB);
            sb.Append(PROJECT_CODE);
            sb.Append(" -o" + TS_PROD_BIN_OUT_DIR + @"\ProdBin_FeImport_" + pdfName + ".out");
            sb.Append(" -v " + pdfName);
            string commandLine = sb.ToString();

            //Console.WriteLine(commandLine);
            return commandLine;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="destName"></param>
        /// <param name="pdfName"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static string CreateCommandLineArgs(string destName, string pdfName, Enums.BuildConfig config)
        {
            // %MICSEXEPATH%\ftValidate.exe fcsa HULME1_0 -oD:\inetpub\micstest\mics\userdirs\hulme\hulme1\%1.out -LD:\inetpub\micstest\mics\userdirs\hulme\hulme1\ftValidate.log  -v %1)
            StringBuilder sb = new StringBuilder(" ");

            sb.Append("-D ");
            sb.Append(DB);
            sb.Append(PROJECT_CODE);
            sb.Append(" " + destName);
            sb.Append(" \"" + pdfName + "\"");

            return sb.ToString();
        }

        /// <summary>
        /// Place single quotes around each element in a comma separated list.  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EnQuote(string str)
        {
            return "\"" + str + "\"";
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="InStr"></param>
        /// <param name="first"></param>
        /// <param name="last"></param>
        /// <returns></returns>
        public static string StripFirstAndLastLines(string InStr, int first, int last)
        {
            // InStr is assumed to be multi-line with <r><n> delimeters
            string[] lines = Regex.Split(InStr, "\r\n");

            string outStr = "";

            if (lines.Length < (first + last))
            {
                outStr = "StripFirstAndLast(): error";
            }
            else if (lines.Length == (first + last))
            {
                outStr = "StripFirstAndLast(): no lines";
            }
            else
            {
                List<string> survivors = new List<string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    if ((i >= first) && (i <= (lines.Length - last - 1)))
                    {
                        survivors.Add(lines[i]);
                    }
                }

                foreach (string survivor in survivors)
                {
                    outStr += survivor + "\r\n";
                }

            }
            return outStr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string MakeDestName(string filePath)
        {
            string result = "DEFAULT_DEST_NAME";

            string fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);

            // Truncate after the first space in the file name stub.
            char[] delimeters = new char[1] { ' ' };
            result = fileNameNoExt.Split(delimeters)[0];

            //Console.Write("\nresult = " + result);
            return result;
        }



    }
}
