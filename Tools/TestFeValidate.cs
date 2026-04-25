using _Configuration;
using _NewLib;
using _Utillib;
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
    public class TestFeValidate
    {

        // Files, Directories, Accounts, DBs.
        const string AHULME_BIN_DIR = @"D:\Users\ahulme\bin";
        const string MICS_RELEASE_BIN_DIR = @"D:\Users\ahulme\MICS#\_bin\Release";
        //const string MICS_RELEASE_BIN_DIR = @"D:\Users\fmda";
        const string MICS_DEBUG_BIN_DIR = @"D:\Users\ahulme\MICS#\_bin\Debug";
        //const string PROD_BIN_DIR = @"D:\prod\bin";
        const string PROD_BIN_DIR = @"D:\Users\ahulme\FCSA_Native_Progs\bin20180406";
        const string FEVALIDATE_MICS_RELEASE_EXE = MICS_RELEASE_BIN_DIR + @"\FeValidate.exe";
        const string FEVALIDATE_MICS_DEBUG_EXE = MICS_DEBUG_BIN_DIR + @"\FeValidate.exe";
        const string FEIMPORT_EXE = MICS_RELEASE_BIN_DIR + @"\FeImport.exe";
        const string FEVALIDATE_PROD_EXE = PROD_BIN_DIR + @"\fevalidate.exe";
        const string FEPRINT_EXE = MICS_RELEASE_BIN_DIR + @"\FePrint.exe";
        const string COMPFXVALIDATEOUTPUTS_EXE = AHULME_BIN_DIR + @"\CompFxValidateOutputs.exe";

        const string ES_PROD_BIN_OUT_DIR = @"D:\Users\ahulme\MICS#\TestData\ES\ProdBin_Results";
        const string ES_MICS_CS_OUT_DIR = @"D:\Users\ahulme\MICS#\TestData\ES\MICS#_Results";
        const string INETPUB_DIR = @"D:\inetpub\micstest\mics\userdirs\hulme\hulme1";
        const string IMPORT_FAILED = "  ------------ IMPORT FAILED";

        const string DB_NAME = "fcsa";
        //const string DB_ARG = " " + DB_NAME;
        const string PROJECT_CODE = " HULME1_0";
        //const string ES_PDF_DIR = @"D:\Users\ahulme\MICS#\TestData\ES\PDFs";

        //const string ES_PDF_LIST_FILE = @"D:\Users\ahulme\MICS#\TestData\ES\PDFs\FeValidate_datafilesList.txt";
        //const string ES_PDF_LIST_FILE = @"D:\Users\ahulme\MICS#\TestData\ES\PDFs\Everything.txt";
        private static string ES_PDF_LIST_FILE;

        private static string[] mPdfNames;
        private static string[] mPdfPaths;
        private static bool runProdBin = false;
        private static bool runMicsCs = false;
        private static bool singleShotMode = false;
        private static bool verbose = false;
        private static bool reImportPDF = false;  // Applies only to case of neither -p nor -m being requested.
        private static string singleShotPDF = "";
        private static Enums.BuildConfig config = Enums.BuildConfig.RELEASE;

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="args"></param>
        public static void Test_FeValidate(string[] args)
        {
            string stdOut;
            string stdErr;
            int exitCode;
            string result = "";

            try
            {
#if false
                // Turn on developmental logging.
                string mLog2FilePath = @"d:\MicsBatchLogs\Test_FeValidate.log";
                if (Log2.SetLogFilePath(mLog2FilePath))
                {
                    Log2.Erase();
                    Log2.Set(Log2.FileOpenClose.PER_SESSION);
                    Log2.Set(Log2.WriteMode.ENABLED);
                    Log2.Set(Log2.Level.VERBOSE);
                    Info.BuildMetaData = Info.CollateExeMetaData();
                    Log2.i("\nBuild: " + Info.BuildMetaData);
                }
                else
                {
                    Console.Error.Write("\nERROR: could not open Log2 file: " + mLog2FilePath);
                }
#endif
                // Process command-line arguments, if any.
                ProcessArgs(args);

                // Get the paths of all available PDFs.
                //mPdfPaths = File.ReadAllLines(ES_PDF_LIST_FILE);
                mPdfPaths = ParseInputFile(ES_PDF_LIST_FILE);
                // Get the names of all available PDFs.
                mPdfNames = GetListofPdfNames(mPdfPaths);

                if (verbose)
                {
                    for (int i = 0; i < mPdfNames.Length; i++)
                    {
                        Console.WriteLine(mPdfNames[i].PadRight(16) + mPdfPaths[i]);
                    }
                }

                // We need to access the database; establish a connection.
                // First, populate the Info fields required to make a connect.
                Info.DbName = DB_NAME;
                Info.MicsUserName = "hulme1";
                Info.Password = "XXX";

                // Connect to the DB.
                Ssutil.UtConnect(DB_NAME, 1);

                // This loop iterates over all PDFs.
                for (int i = 0; i < mPdfNames.Length; i++)
                {
                    string pdfName = mPdfNames[i];
                    string pdfPath = mPdfPaths[i];

                    // Handle the 'single-shot' PDF processing.
                    if (singleShotMode && !(pdfName.ToLower().Equals(singleShotPDF.ToLower())))
                    {
                        continue;
                    }

                    StringBuilder sb = new StringBuilder("\n");
                    sb.Append((i + 1).ToString().PadLeft(3));
                    sb.Append(" of ");
                    sb.Append(mPdfNames.Length);
                    sb.Append(" :   ");
                    sb.Append(pdfName.PadRight(16));
                    Console.Error.Write(sb);

                    Console.WriteLine("=============== " + pdfName + " =========================================================\r\n");

                    // Construct the command-line args for C# FeImport.exe as a string.
                    string commandLineFeImportArgs = CreateCommandLineArgsForFeImport(pdfName, pdfPath);

                    // Construct the command-line args for C/C++ feValidate.exe as a string.
                    string commandLineProdFeValidateArgs = CreateCommandLineArgsForProdBin(pdfName);

                    // Construct the command-line args for C# FeValidate.exe as a string.
                    string commandLineMicsCsFeValidateArgs = CreateCommandLineArgsForMicsCs(pdfName, config);

                    // Construct the command-line args for C# FePrint as a string.
                    string commandLineFePrintArgs = CreateCommandLineArgsForFePrint(pdfName);

                    // Construct the path of the file that FePrint's stdOut is written to.
                    string prodFePrintFilePath = ES_PROD_BIN_OUT_DIR + @"\ProdBin_FeValidate_" + pdfName + ".pri";
                    string micsFePrintFilePath = ES_MICS_CS_OUT_DIR + @"\MICS#_FeValidate_" + pdfName + ".pri";

                    // D:\prod\bin\feValidate.exe -------------------------------------------------------------------------------
                    if (runProdBin)
                    {
                        // Reset the database tables back to their 'virgin' values as prescribed by the PDF text files.
                        Run(FEIMPORT_EXE, commandLineFeImportArgs, out stdOut, out stdErr, out exitCode);
                        if (exitCode != 0)
                        {
                            result = IMPORT_FAILED;
                            goto endOfPdfLoop;
                        }
                        // Now execute FeValidate as a command-line task.
                        Run(FEVALIDATE_PROD_EXE, commandLineProdFeValidateArgs, out stdOut, out stdErr, out exitCode);
                        // Now run FePrint to see how FeValidate changed the DB.
                        Run(FEPRINT_EXE, commandLineFePrintArgs, out stdOut, out stdErr, out exitCode);
                        // Write the stdOut of FePrint to file.
                        File.WriteAllText(prodFePrintFilePath, stdOut);

                        result = "C++ version of FeValidate used.";
                    }

                    // MICS#: FeValidate.exe -----------------------------------------------------------------------------------
                    if (runMicsCs)
                    {
                        // Reset the database tables back to their 'virgin' values as prescribed by the PDF text files.
                        Run(FEIMPORT_EXE, commandLineFeImportArgs, out stdOut, out stdErr, out exitCode);
                        if (exitCode != 0)
                        {
                            result = IMPORT_FAILED;
                            goto endOfPdfLoop;
                        }
                        // Now execute FeValidate as a command-line task.
                        Run(FEVALIDATE_MICS_RELEASE_EXE, commandLineMicsCsFeValidateArgs, out stdOut, out stdErr, out exitCode);
                        // Now run FePrint to see how FeValidate changed the DB.
                        Run(FEPRINT_EXE, commandLineFePrintArgs, out stdOut, out stdErr, out exitCode);
                        // Write the stdOut of FePrint to file.
                        File.WriteAllText(micsFePrintFilePath, stdOut);

                        result = "C#  version of FeValidate used.";
                    }

                    // We can use CompFtValOut.exe to compare the .out and .pri output files.
                    if (runProdBin && runMicsCs)
                    {
                        Run(COMPFXVALIDATEOUTPUTS_EXE, "  " + ES_PROD_BIN_OUT_DIR + @"\ProdBin_FeValidate_" + pdfName + ".out  " + ES_MICS_CS_OUT_DIR + @"\MICS#_FeValidate_" + pdfName + ".out", out stdOut, out stdErr, out exitCode);
                        Console.Write("\n" + stdOut);
                        bool feValOutIsIdentical = false;

                        if (stdOut.Contains("Files are identical: True"))
                        {
                            feValOutIsIdentical = true;
                        }

                        Run(COMPFXVALIDATEOUTPUTS_EXE, "  " + ES_PROD_BIN_OUT_DIR + @"\ProdBin_FeValidate_" + pdfName + ".pri  " + ES_MICS_CS_OUT_DIR + @"\MICS#_FeValidate_" + pdfName + ".pri", out stdOut, out stdErr, out exitCode);
                        Console.Write("\n" + stdOut);
                        bool fePrintOutIsIdentical = false;
                        if (stdOut.Contains("Files are identical: True"))
                        {
                            fePrintOutIsIdentical = true;
                        }

                        if (feValOutIsIdentical && fePrintOutIsIdentical)
                        {
                            result = "  PASS / PASS";
                        }
                        else if (feValOutIsIdentical && !fePrintOutIsIdentical)
                        {
                            result = "  PASS / FAIL  <<<<<";
                        }
                        else if (!feValOutIsIdentical && fePrintOutIsIdentical)
                        {
                            result = "  FAIL / PASS  <<<<<";
                        }
                        else
                        {
                            result = "  FAIL / FAIL  <<<<<";
                        }

                    }

                    if (!runProdBin && !runMicsCs && reImportPDF)
                    {
                        Run(FEIMPORT_EXE, commandLineFeImportArgs, out stdOut, out stdErr, out exitCode);
                        if (exitCode != 0)
                        {
                            result = "IMPORT FAILED";
                            goto endOfPdfLoop;
                        }
                        result = "IMPORT succeeded";
                    }

                    endOfPdfLoop:
                    Console.Error.Write(result);

                }  // end of loop-over-allPDFs

                Console.WriteLine("\r\nPDF files found: " + mPdfNames.Length + "\r\n\r\n");

                Run(@"D:\Users\ahulme\bin\CleanProdFilesRead.exe", " -v", out stdOut, out stdErr, out exitCode);

                // Disconnect from the DB.
                Ssutil.UtDisconnect(1);

                Environment.Exit(666);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

        }

        /// <summary>
        /// 
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
        /// <param name="pdfName"></param>
        /// <param name="pdfPath"></param>
        /// <returns></returns>
        public static string CreateCommandLineArgsForFeImport(string pdfName, string pdfPath)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" " + DB_NAME);
            sb.Append(PROJECT_CODE);
            sb.Append(" -d ");
            sb.Append(pdfName);
            sb.Append(" ");
            sb.Append(EnQuote(pdfPath));

            return sb.ToString();
        }
        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="args"></param>
        public static void ProcessArgs(string[] args)
        {
            Console.WriteLine("args.Length = " + args.Length);
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
                    else if (args[i].ToLower().Equals("-p"))
                    {
                        runProdBin = true;
                    }
                    else if (args[i].ToLower().Equals("-m"))
                    {
                        runMicsCs = true;
                    }
                    else if (args[i].ToLower().Equals("-d"))
                    {
                        config = Enums.BuildConfig.DEBUG;
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
                        ES_PDF_LIST_FILE = args[i].ToLower().Replace("-f:", "");
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
            sb.AppendLine(TAB1 + "Usage:    testing FeValidate [-d] [-i] [-m] [-o:<PDFfilePath>] [-p]");
            sb.AppendLine();
            sb.AppendLine(TAB1 + "Options:");
            sb.AppendLine(TAB2 + "-d" + TAB1 + "Use the DEBUG build of FeValidate.exe.");
            sb.AppendLine(TAB2 + "-i" + TAB1 + "IMPORT PDF text file(s) into the DB.");
            sb.AppendLine(TAB2 + "-f:   " + "File pathname containing a list of PDFs to test with.");
            sb.AppendLine(TAB2 + "-m" + TAB1 + "Run MICS#.exe using DB tables for previously imported PDF(s)");
            sb.AppendLine(TAB2 + "-o:   " + "ONLY use single PDF as prescribed by <PDFfilePath>");
            sb.AppendLine(TAB2 + "-p" + TAB1 + "Run d:\\PROD\\bin\\feValidate.exe using DB tables for previously imported PDF(s)");
            sb.AppendLine(TAB2 + "-L" + TAB1 + "LOG.");
            sb.AppendLine(TAB2 + "-v" + TAB1 + "VERBOSE.");
            sb.AppendLine();
            sb.AppendLine(TAB1 + "Notes:");
            sb.AppendLine(TAB2 + "1. Use of the -m and/or -p options automatically invokes -i.");
            Console.WriteLine(sb.ToString());

        }


        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="exePath"></param>
        /// <param name="args"></param>
        /// <param name="stdOut"></param>
        /// <param name="stdErr"></param>
        /// <param name="exitCode"></param>
        public static void Run(string exePath, string args, out string stdOut, out string stdErr, out int exitCode)
        {
            WindowsShell.RunCommand(exePath, args, out stdOut, out stdErr, out exitCode);

            Console.Write("\n\n" + exePath + args);

            Console.Write("\nExitCode = " + exitCode);
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
                pdfNames[i] = pdfName;
            }

            return pdfNames;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="pdfName"></param>
        /// <returns></returns>
        private static string CreateCommandLineArgsForProdBin(string pdfName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" " + DB_NAME);
            sb.Append(PROJECT_CODE);
            sb.Append(" -o" + ES_PROD_BIN_OUT_DIR + @"\ProdBin_FeValidate_" + pdfName + ".out");
            sb.Append("  " + pdfName);

            return sb.ToString();
        }


        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="pdfName"></param>
        /// <returns></returns>
        private static string CreateCommandLineArgsForFePrint(string pdfName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" " + DB_NAME);
            sb.Append(PROJECT_CODE);
            sb.Append(" " + pdfName);

            return sb.ToString();
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="pdfName"></param>
        /// <returns></returns>
        private static string CreatePathForProd(string pdfName)
        {

            StringBuilder sb = new StringBuilder();

            sb.Append(" " + DB_NAME);
            sb.Append(PROJECT_CODE);
            sb.Append(" " + pdfName);

            return sb.ToString();
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="pdfName"></param>
        /// <returns></returns>
        private static string CreateCommandLineArgsForMicsCsFePrint(string pdfName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" " + DB_NAME);
            sb.Append(PROJECT_CODE);
            sb.Append(" " + pdfName);

            return sb.ToString();
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="pdfName"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static string CreateCommandLineArgsForMicsCs(string pdfName, Enums.BuildConfig config)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" " + DB_NAME);
            sb.Append(PROJECT_CODE);
            sb.Append(" -o" + ES_MICS_CS_OUT_DIR + @"\MICS#_FeValidate_" + pdfName + ".out");
            sb.Append("  " + pdfName);

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


    }
}

