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
    public class TestFtValidate
    {

        // Files, Directories, Accounts, DBs.
        private const string MICS_DEV_BIN_DIR = @"D:\Users\ahulme\MICS#\_bin\Release";
        private const string MICS_REGRESSION_BIN_DIR = @"D:\Users\ahulme\MICS#20200115\_bin\Release";
        private const string PROD_BIN_DIR = @"D:\prod\bin";
        private const string FCSA_WIN_NEW_DIR = @"D:\Users\ahulme\FCSAwinNew\_build\x64\Release";
        private const string LEGACY_NATIVE_BIN_DIR = @"D:\Users\ahulme\FCSA_Native_Progs\bin20180406";

        private static string[] exePaths = new string[6]
        {
            "NOT_USED",
            MICS_DEV_BIN_DIR,
            MICS_REGRESSION_BIN_DIR,
            PROD_BIN_DIR,
            FCSA_WIN_NEW_DIR,
            LEGACY_NATIVE_BIN_DIR
        };

        private static string FtValidate_exe_A = "";
        private static string FtValidate_exe_B = "";

        const string MY_BIN = @"d:\users\ahulme\bin";

        const string DB = " fcsa";
        const string PROJECT_CODE = " HULME1_0";

        private static string TS_PDF_LIST_FILE;

        const string FTIMPORT_EXE = PROD_BIN_DIR + @"\FtImport.exe";
        const string FTPRINT_EXE = PROD_BIN_DIR + @"\FtPrint.exe";

        const string FTPRINT_OUT_DIR_A = @"D:\Users\ahulme\MICS#\TestData\TS\FtValidateTests_A";
        const string FTPRINT_OUT_DIR_B = @"D:\Users\ahulme\MICS#\TestData\TS\FtValidateTests_B";

        const string INETPUB_DIR = @"D:\inetpub\micstest\mics\userdirs\hulme\hulme1";
        private static string[] mPdfNames;
        private static string[] mPdfPaths;
        private static bool run_A = false;
        private static bool run_B = false;
        private static bool singleShotMode = false;
        private static bool verbose = false;
        private static bool logging = false;
        private static bool reImportPDF = false;  // Applies only to case of neither -p nor -m being requested.
        private static string singleShotPDF = "";
        private static string lastStdOut = "";

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="args"></param>
        public static void Test_FtValidate(string[] args)
        {
            try
            {
                // Process command-line arguments, if any.
                ProcessArgs(args);

                // Get the paths of all available PDFs.
                //mPdfPaths = File.ReadAllLines(TS_PDF_LIST_FILE);
                mPdfPaths = ParseInputFile(TS_PDF_LIST_FILE);
                // Get the names of all available PDFs.
                mPdfNames = GetListofPdfNames(mPdfPaths);

                if (verbose)
                {
                    for (int i = 0; i < mPdfNames.Length; i++)
                    {
                        Console.WriteLine(mPdfNames[i].PadRight(16) + mPdfPaths[i]);
                    }
                }

                // Write configuration paths to Console.Out
                Console.Write("\n\nFTIMPORT_EXE = " + FTIMPORT_EXE);
                Console.Write("\nFTPRINT_EXE  = " + FTPRINT_EXE + "\n");
                Console.Write("\nFTVALIDATE_EXE_A = " + FtValidate_exe_A);
                Console.Write("\nFTVALIDATE_EXE_B = " + FtValidate_exe_B + "\n");
                Console.Write("\nFTPRINT_OUT_DIR_A = " + FTPRINT_OUT_DIR_A);
                Console.Write("\nFTPRINT_OUT_DIR_B = " + FTPRINT_OUT_DIR_B);

                // Create the results output directories if they do not already exist.
                Directory.CreateDirectory(FTPRINT_OUT_DIR_A);
                Directory.CreateDirectory(FTPRINT_OUT_DIR_B);

                // This loop iterates over all PDFs.
                for (int i = 0; i < mPdfNames.Length; i++)
                {
                    string pdfName = mPdfNames[i];

                    if (singleShotMode && !(pdfName.ToLower().Equals(singleShotPDF.ToLower())))
                    {
                        continue;
                    }

                    Console.WriteLine("\n\n=============== " + pdfName + " =========================================================\r\n");

                    // Construct the command-line invocation for Production C/C++ .exe as a string.
                    string commandLine_A = CreateCommandLineFor_A(pdfName);
                    // Construct the command-line invocation for MICS# .exe as a string.
                    string commandLine_B = CreateCommandLineFor_B(pdfName);

                    // Construct the command-line invocation for FtPrint of Production C/C++ as a string.
                    string commandLineProdFtPrint = CreateCommandLineForFtPrint_A(pdfName);
                    // Construct the command-line invocation for FtPrint of MICS# as a string.
                    string commandLineMicsCsFtPrint = CreateCommandLineForFtPrint_B(pdfName);

                    // D:\prod\bin\ftValidate.exe -------------------------------------------------------------------------------
                    if (run_A)
                    {
                        // Reset the database tables back to their 'virgin' values as prescribed by the PDF text files.
                        ImportPDFtoDB(i);
                        // Now execute FtValidate as a command-line task.
                        CommandLineTask(commandLine_A);
                        // Now run FtPrint to see how FtValidate changed the DB.
                        CommandLineTask(commandLineProdFtPrint);
                    }

                    // MICS#: FtValidate.exe -----------------------------------------------------------------------------------
                    if (run_B)
                    {
                        // Reset the database tables back to their 'virgin' values as prescribed by the PDF text files.
                        ImportPDFtoDB(i);
                        // Now execute FtValidate as a command-line task.
                        CommandLineTask(commandLine_B);
                        // Now run FtPrint to see how FtValidate changed the DB.
                        CommandLineTask(commandLineMicsCsFtPrint);
                    }

                    if (run_A && run_B)
                    {
                        CommandLineTask(MY_BIN + @"\CompFxValidateOutputs.exe  " + FTPRINT_OUT_DIR_A + @"\FtValidate_A_" + pdfName + ".out  " + FTPRINT_OUT_DIR_B + @"\FtValidate_B_" + pdfName + ".out");
                        bool ftValOutIsIdentical = false;
                        string result = "";
                        if (lastStdOut.Contains("Files are identical: True"))
                        {
                            ftValOutIsIdentical = true;
                        }

                        CommandLineTask(MY_BIN + @"\CompFxValidateOutputs.exe  " + FTPRINT_OUT_DIR_A + @"\FtValidate_A_" + pdfName + ".pri  " + FTPRINT_OUT_DIR_B + @"\FtValidate_B_" + pdfName + ".pri");
                        bool ftPrintOutIsIdentical = false;

                        if (lastStdOut.Contains("Files are identical: True"))
                        {
                            ftPrintOutIsIdentical = true;
                        }

                        if (ftValOutIsIdentical && ftPrintOutIsIdentical)
                        {
                            result = "  PASS / PASS";
                        }
                        else if (ftValOutIsIdentical && !ftPrintOutIsIdentical)
                        {
                            result = "  PASS / FAIL  <<<<<";
                        }
                        else if (!ftValOutIsIdentical && ftPrintOutIsIdentical)
                        {
                            result = "  FAIL / PASS  <<<<<";
                        }
                        else
                        {
                            result = "  FAIL / FAIL  <<<<<";
                        }

                        StringBuilder sb = new StringBuilder();
                        sb.Append((i + 1).ToString().PadLeft(3));
                        sb.Append(" of ");
                        sb.Append(mPdfNames.Length);
                        sb.Append(" :   ");
                        sb.Append(pdfName.PadRight(20));
                        sb.Append(result);

                        Console.Write("\n\n" + sb.ToString());
                        Console.Error.Write("\n\n" + sb.ToString());
                    }

                    if (!run_A && !run_B && reImportPDF)
                    {
                        ImportPDFtoDB(i);
                    }

                }  // end of loop-over-allPDFs

                Console.WriteLine("\r\nPDF files found: " + mPdfNames.Length + "\r\n\r\n");

                CommandLineTask(@"D:\Users\ahulme\bin\CleanProdFilesRead.exe -v");

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
        public static string[] ParseInputFile(string filePath)
        {
            List<string> validLines = new List<string>();

            string[] allLines = File.ReadAllLines(filePath);

            foreach (string line in allLines)
            {
                string candidate = line.Trim();
                if (!String.IsNullOrWhiteSpace(candidate))
                {
                    if (!candidate.StartsWith("//"))
                    {
                        validLines.Add(candidate);
                    }
                }
            }

            return validLines.ToArray();
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="i"></param>
        public static void ImportPDFtoDB(int i)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(FTIMPORT_EXE);
            sb.Append(DB);
            sb.Append(PROJECT_CODE);
            sb.Append(" -f ");
            sb.Append(mPdfNames[i]);
            sb.Append(" ");
            sb.Append(EnQuote(mPdfPaths[i]));
            string commandLine = sb.ToString();

            Console.WriteLine("IMPORT: " + commandLine);

            CommandLineTask(commandLine);
        }
        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="args"></param>
        public static void ProcessArgs(string[] args)
        {
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
                        // Truncate to first 16 characters.
                        singleShotPDF = Strings.Truncate(singleShotPDF, 16);
                        Console.WriteLine("singleShotPDF = " + singleShotPDF);
                    }
                    else if (args[i].ToLower().StartsWith("-a:"))
                    {
                        run_A = true;

                        string numberStr = args[i].Substring(3);
                        int index = 0;
                        try
                        {
                            index = Convert.ToInt32(numberStr);
                            FtValidate_exe_A = exePaths[index] + @"\FtValidate.exe";
                        }
                        catch (Exception) { }

                        if (Maths.InRange(1, 5, index))
                        {
                            FtValidate_exe_A = exePaths[index] + @"\FtValidate.exe";
                        }
                        else
                        {
                            Console.Write("\n\n The string after '-a:' must be a number between 1 and 5.");
                            WriteCliUsageMessage();
                            Environment.Exit(666);
                        }
                    }
                    else if (args[i].ToLower().StartsWith("-b:"))
                    {
                        run_B = true;

                        string numberStr = args[i].Substring(3);
                        int index = 0;
                        try
                        {
                            index = Convert.ToInt32(numberStr);
                            FtValidate_exe_B = exePaths[index] + @"\FtValidate.exe";
                        }
                        catch (Exception) { }

                        if (Maths.InRange(1, 5, index))
                        {
                            FtValidate_exe_B = exePaths[index] + @"\FtValidate.exe";
                        }
                        else
                        {
                            Console.Write("\n\n The string after '-b:' must be a number between 1 and 5.");
                            WriteCliUsageMessage();
                            Environment.Exit(666);
                        }
                    }
                    else if (args[i].ToLower().Equals("-l"))
                    {
                        logging = true;
                    }
                    else if (args[i].ToLower().Equals("-v"))
                    {
                        verbose = true;
                    }
                    else if (args[i].ToLower().Equals("-i"))
                    {
                        reImportPDF = true;
                    }
                    else if (args[i].ToLower().StartsWith("-f:"))
                    {
                        //singleShotMode = false;
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

            Console.Write("\nProcessArgs(): singleShotMode   = " + singleShotMode);
            Console.Write("\nProcessArgs(): singleShotPDF    = " + singleShotPDF);
            Console.Write("\nProcessArgs(): run_A            = " + run_A);
            Console.Write("\nProcessArgs(): run_B            = " + run_B);
            Console.Write("\nProcessArgs(): logging          = " + logging);
            Console.Write("\nProcessArgs(): verbose          = " + verbose);
            Console.Write("\nProcessArgs(): reImportPDF      = " + reImportPDF);
            Console.Write("\nProcessArgs(): TS_PDF_LIST_FILE = " + TS_PDF_LIST_FILE);
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
            sb.AppendLine(TAB1 + "Usage:    testing FtValidate <-f:<path>> [-a:N] [-b:M] [-d] [-i] [-m]  [-o:<PDFname>]");
            sb.AppendLine();
            sb.AppendLine(TAB1 + "Options:");
            sb.AppendLine();
            sb.AppendLine(TAB2 + "-a:N" + TAB1 + "Run test stream A using FtValidate.exe in directory #N (see below).");
            sb.AppendLine(TAB2 + "-b:M" + TAB1 + "Run test stream B using FtValidate.exe in directory #M (see below).");
            sb.AppendLine(TAB2 + "-i  " + TAB1 + "IMPORT PDF text file(s) into the DB;");
            sb.AppendLine(TAB2 + "-f: " + TAB1 + "File <path> containing a list of PDFs to test with.");
            sb.AppendLine(TAB2 + "-o: " + TAB1 + "ONLY use single PDF with prescribed <PDFname>");
            sb.AppendLine(TAB2 + "-L  " + TAB1 + "LOG.");
            sb.AppendLine(TAB2 + "-v  " + TAB1 + "VERBOSE.");
            sb.AppendLine();
            sb.AppendLine(TAB1 + "FtValidate.exe directory choices:");
            sb.AppendLine();
            sb.AppendLine(TAB2 + "1 =" + TAB1 + @"D:\Users\ahulme\MICS#\_bin\Release");
            sb.AppendLine(TAB2 + "2 =" + TAB1 + @"D:\Users\ahulme\MICS#20200115\_bin\Release");
            sb.AppendLine(TAB2 + "3 =" + TAB1 + @"D:\prod\bin");
            sb.AppendLine(TAB2 + "4 =" + TAB1 + @"D:\Users\ahulme\FCSAwinNew\_build\x64\Release");
            sb.AppendLine(TAB2 + "5 =" + TAB1 + @"D:\Users\ahulme\FCSA_Native_Progs\bin20180406");
            sb.AppendLine();
            sb.AppendLine(TAB2 + "<...>  indicates a mandatory argument.\r\n");
            sb.AppendLine(TAB2 + "[...]  indicates an optional argument.");
            sb.AppendLine();
            sb.AppendLine(TAB1 + "Eg.:");
            sb.AppendLine(TAB2 + "");
            sb.AppendLine();
            sb.AppendLine(TAB1 + "Notes:");
            sb.AppendLine(TAB2 + "1. Use of the -a and/or -b options automatically invokes -i.");
            Console.WriteLine(sb.ToString());

        }
        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="commandLine"></param>
        public static void CommandLineTask(string commandLine)
        {
            //Console.Write("\nCommandLineTask(): commandLine = " + commandLine + "\n");

            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine(commandLine);
            cmd.StandardInput.WriteLine("echo ExitCode = %errorlevel%");
            cmd.StandardInput.WriteLine();
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();

            //Console.WriteLine("S--------------------------");
            lastStdOut = StripFirstAndLastLines(cmd.StandardOutput.ReadToEnd(), 3, 3);
            Console.WriteLine(lastStdOut);
            //Console.WriteLine("E---------------------------");
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="pdfPaths"></param>
        /// <returns></returns>
        private static string[] GetListofPdfNames(string[] pdfPaths)
        {
            string[] pdfNames = new string[pdfPaths.Length];

            string pdfName;
            const string pattern = @"(\w+)\.txt";
            for (int i = 0; i < pdfPaths.Length; i++)
            {
                Match match = Regex.Match(pdfPaths[i].ToLower(), pattern);
                pdfName = Regex.Replace(match.Value, @"\.txt", "");
                pdfName = pdfName.ToLower();

                // Truncate the PDF name to the first 16 characters.
                pdfNames[i] = Strings.Truncate(pdfName, 16);
            }

            return pdfNames;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="pdfName"></param>
        /// <returns></returns>
        private static string CreateCommandLineFor_A(string pdfName)
        {
            // %MICSEXEPATH%\ftValidate.exe fcsa HULME1_0 -oD:\inetpub\micstest\mics\userdirs\hulme\hulme1\%1.out -LD:\inetpub\micstest\mics\userdirs\hulme\hulme1\ftValidate.log  -v %1)
            StringBuilder sb = new StringBuilder();
            sb.Append(FtValidate_exe_A);
            sb.Append(DB);
            sb.Append(PROJECT_CODE);
            sb.Append(" -o" + FTPRINT_OUT_DIR_A + @"\FtValidate_A_" + pdfName + ".out");
            //sb.Append(" -v " + pdfName);
            sb.Append("  " + pdfName);
            string commandLine = sb.ToString();

            //Console.WriteLine(commandLine);
            return commandLine;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="pdfName"></param>
        /// <returns></returns>
        private static string CreateCommandLineForFtPrint_A(string pdfName)
        {
            //
            StringBuilder sb = new StringBuilder();
            sb.Append(FTPRINT_EXE);
            sb.Append(DB);
            sb.Append(PROJECT_CODE);
            sb.Append(" -o" + FTPRINT_OUT_DIR_A + @"\FtValidate_A_" + pdfName + ".pri");
            sb.Append(" F");
            sb.Append(" " + pdfName);
            string commandLine = sb.ToString();

            //Console.WriteLine(commandLine);
            return commandLine;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="pdfName"></param>
        /// <returns></returns>
        private static string CreateCommandLineForFtPrint_B(string pdfName)
        {
            //
            StringBuilder sb = new StringBuilder();
            sb.Append(FTPRINT_EXE);
            sb.Append(DB);
            sb.Append(PROJECT_CODE);
            sb.Append(" -o" + FTPRINT_OUT_DIR_B + @"\FtValidate_B_" + pdfName + ".pri ");
            sb.Append(" F");
            sb.Append(" " + pdfName);
            string commandLine = sb.ToString();

            //Console.WriteLine(commandLine);
            return commandLine;
        }

        /// <summary>
        /// Construct the command-line invocation for MICS# .exe as a string.
        /// </summary>
        /// <param name="pdfName"></param>
        /// <returns></returns>
        private static string CreateCommandLineFor_B(string pdfName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(FtValidate_exe_B);

            sb.Append(DB);
            sb.Append(PROJECT_CODE);
            sb.Append(" -o" + FTPRINT_OUT_DIR_B + @"\FtValidate_B_" + pdfName + ".out");
            if (logging)
            {
                sb.Append(" -L" + FTPRINT_OUT_DIR_B + @"\FtValidate_B_" + pdfName + ".log");
            }
            //sb.Append(" -v " + pdfName);
            sb.Append("  " + pdfName);
            string commandLine = sb.ToString();

            //Console.WriteLine(commandLine);
            return commandLine;
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




    }
}
