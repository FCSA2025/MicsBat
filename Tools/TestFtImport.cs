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
    public class TestFtImport
    {

        // Files, Directories, Accounts, DBs.
        const string AHULME_BIN_DIR = @"D:\Users\ahulme\bin";
        const string MICS_RELEASE_BIN_DIR = @"D:\Users\ahulme\MICS#\_bin\Release";
        const string MICS_DEBUG_BIN_DIR = @"D:\Users\ahulme\MICS#\_bin\Debug";
        //const string PROD_BIN_DIR = @"D:\prod\bin";
        const string PROD_BIN_DIR = @"D:\Users\ahulme\FCSA_Native_Progs\bin20180406";
        const string FTIMPORT_MICS_EXE = MICS_RELEASE_BIN_DIR + @"\FtImport.exe";
        const string FTIMPORT_PROD_EXE = PROD_BIN_DIR + @"\FtImport.exe";
        const string FTPRINT_EXE = PROD_BIN_DIR + @"\FtPrint.exe";
        const string COMPFXVALIDATEOUTPUTS_EXE = AHULME_BIN_DIR + @"\CompFxValidateOutputs.exe";
        const string TS_PROD_BIN_OUT_DIR = @"D:\Users\ahulme\MICS#\TestData\TS\ProdBin_Results";
        const string TS_MICS_CS_OUT_DIR = @"D:\Users\ahulme\MICS#\TestData\TS\MICS#_Results";
        const string INETPUB_DIR = @"D:\inetpub\micstest\mics\userdirs\hulme\hulme1";
        const string IMPORT_FAILED = "  ------------ IMPORT FAILED";

        const string DB_NAME = "fcsa";
        const string PROJECT_CODE = "HULME1_0";

        private static string TS_PDF_LIST_FILE;

        private static string[] mPdfNames;
        private static string[] mPdfPaths;
        private static bool mRunProdBin = false;
        private static bool mRunMicsCs = false;
        private static bool mSingleShotMode = false;
        private static bool mVerbose = false;
        private static string mSingleShotPDF = "";
        private static int mProdExitCode;
        private static int mMicsExitCode;


        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="args"></param>
        public static void Test_FtImport(string[] args)
        {
            string stdOut;
            string stdErr;
            int exitCode;
            string result = "";

            try
            {
#if false
                // Turn on developmental logging.
                string mLog2FilePath = @"d:\MicsBatchLogs\Test_FtImport.log";
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
                //mPdfPaths = File.ReadAllLines(TS_PDF_LIST_FILE);
                mPdfPaths = ParseInputFile(TS_PDF_LIST_FILE);
                // Get the names of all available PDFs.
                mPdfNames = GetListofPdfNames(mPdfPaths);

                // Check that paths are valid.
                CheckFilesExist(mPdfPaths);

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
                    if (mSingleShotMode && !(pdfName.ToLower().Equals(mSingleShotPDF.ToLower())))
                    {
                        continue;
                    }

                    StringBuilder sb = new StringBuilder("\n");
                    sb.Append((i + 1).ToString().PadLeft(3));
                    sb.Append(" of ");
                    sb.Append(mPdfNames.Length);
                    sb.Append(" :   ");
                    sb.Append(pdfName.PadRight(31));
                    Console.Error.Write(sb);

                    Console.WriteLine("\n\n=============== " + pdfName + " =========================================================\r\n");

                    // Construct the command-line args for C# FtImport.exe as a string.
                    string commandLineMicsFtImportArgs = CreateCommandLineArgsForMicsFtImport(pdfName, pdfPath);

                    // Construct the command-line args for C/C++ feValidate.exe as a string.
                    string commandLineProdFtImportArgs = CreateCommandLineArgsForProdFtImport(pdfName, pdfPath);

                    // Construct the command-line args for C/C++ FtPrint as a string.
                    string commandLineFtPrintArgs = CreateCommandLineArgsForFtPrint(pdfName);

                    // Construct the path of the file that FtPrint's stdOut is written to.
                    string prodFtStdOutFilePath = TS_PROD_BIN_OUT_DIR + @"\ProdBin_FtImport_" + pdfName + ".out";
                    string micsFtStdOutFilePath = TS_MICS_CS_OUT_DIR + @"\MICS#_FtImport_" + pdfName + ".out";

                    // Construct the path of the file that FtPrint's stdOut is written to.
                    string prodFtPrintFilePath = TS_PROD_BIN_OUT_DIR + @"\ProdBin_FtImport_" + pdfName + ".pri";
                    string micsFtPrintFilePath = TS_MICS_CS_OUT_DIR + @"\MICS#_FtImport_" + pdfName + ".pri";

                    // D:\prod\bin\feValidate.exe -------------------------------------------------------------------------------
                    if (mRunProdBin)
                    {
                        // Now execute FtImport as a command-line task.
                        Run(FTIMPORT_PROD_EXE, commandLineProdFtImportArgs, out stdOut, out stdErr, out mProdExitCode);
                        // Write the stdOut of FtImport.exe to file.
                        File.WriteAllText(prodFtStdOutFilePath, stdOut);
                        // Now run FtPrint to see how FtImport changed the DB.
                        Run(FTPRINT_EXE, commandLineFtPrintArgs, out stdOut, out stdErr, out exitCode);
                        // Write the stdOut of FtPrint.exe to file.
                        File.WriteAllText(prodFtPrintFilePath, stdOut);
                    }

                    // MICS#: FtImport.exe -----------------------------------------------------------------------------------
                    if (mRunMicsCs)
                    {
                        // Now execute FtImport as a command-line task.
                        Run(FTIMPORT_MICS_EXE, commandLineMicsFtImportArgs, out stdOut, out stdErr, out mMicsExitCode);
                        // Write the stdOut of FtImport.exe to file.
                        File.WriteAllText(micsFtStdOutFilePath, stdOut);
                        // Now run FtPrint to see how FtImport changed the DB.
                        Run(FTPRINT_EXE, commandLineFtPrintArgs, out stdOut, out stdErr, out exitCode);
                        // Write the stdOut of FtPrint.exe to file.
                        File.WriteAllText(micsFtPrintFilePath, stdOut);
                    }

                    // We can use CompFtValOut.exe to compare the .out and .pri output files.
                    if (mRunProdBin && mRunMicsCs)
                    {
                        string errorIndicator = "";

                        Run(COMPFXVALIDATEOUTPUTS_EXE, "  " + TS_PROD_BIN_OUT_DIR + @"\ProdBin_FtImport_" + pdfName + ".out  " + TS_MICS_CS_OUT_DIR + @"\MICS#_FtImport_" + pdfName + ".out", out stdOut, out stdErr, out exitCode);
                        Console.Write("\n" + stdOut);

                        string exitCodeResult = "PASS";
                        if (mProdExitCode != mMicsExitCode)
                        {
                            exitCodeResult = "----";
                            errorIndicator = "<<<<<<<<<<";
                        }

                        string stdOutResult = "PASS";
                        if (!stdOut.Contains("Files are identical: True"))
                        {
                            stdOutResult = "----";
                            errorIndicator = "<<<<<<<<<<";
                        }

                        Run(COMPFXVALIDATEOUTPUTS_EXE, "  " + prodFtPrintFilePath + "  " + micsFtPrintFilePath, out stdOut, out stdErr, out exitCode);
                        Console.Write("\n" + stdOut);

                        string ftPrintResult = "PASS";
                        if (!stdOut.Contains("Files are identical: True"))
                        {
                            ftPrintResult = "----";
                            errorIndicator = "<<<<<<<<<<";
                        }

                        result = String.Format("{0,3}  {1} / {2} / {3}     {4}", mProdExitCode, exitCodeResult, stdOutResult, ftPrintResult, errorIndicator);
                    }

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
        /// TBD
        /// </summary>
        /// <param name="pdfFilePaths"></param>
        public static void CheckFilesExist(string[] pdfFilePaths)
        {
            if (!mVerbose) return;

            foreach (string path in pdfFilePaths)
            {
                string result;
                if (File.Exists(path))
                {
                    result = ": exists";
                }
                else
                {
                    result = ": FILE NOT FOUND";
                }

                string str = String.Format("\n{0} {1}", path.PadRight(60), result);
                Console.Error.Write(str);
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
        /// <param name="pdfName"></param>
        /// <param name="pdfPath"></param>
        /// <returns></returns>
        public static string CreateCommandLineArgsForMicsFtImport(string pdfName, string pdfPath)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" " + DB_NAME);
            sb.Append(" " + PROJECT_CODE);
            sb.Append(" -f ");
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
                        mSingleShotMode = true;
                        mSingleShotPDF = args[i].ToLower().Replace("-o:", "");
                        Console.WriteLine("singleShotPDF = " + mSingleShotPDF);
                    }
                    else if (args[i].ToLower().Equals("-p"))
                    {
                        mRunProdBin = true;
                    }
                    else if (args[i].ToLower().Equals("-m"))
                    {
                        mRunMicsCs = true;
                    }
                    else if (args[i].ToLower().Equals("-v"))
                    {
                        mVerbose = true;
                    }
                    else if (args[i].ToLower().StartsWith("-f:"))
                    {
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
            sb.AppendLine(TAB1 + "Usage:    testing FtImport [-v] [-m] [-p] [-o:PDFname] <-f:PDFlistFilePath");
            sb.AppendLine();
            sb.AppendLine(TAB1 + "Options:");
            sb.AppendLine(TAB2 + "-v" + TAB1 + "VERBOSE.");
            sb.AppendLine(TAB2 + "-m" + TAB1 + "Run MICS# FtImport");
            sb.AppendLine(TAB2 + "-p" + TAB1 + "Run native ftImporte.exe");
            sb.AppendLine(TAB2 + "-o:   " + "ONLY use single PDF as prescribed by <PDFfilePath>");
            sb.AppendLine(TAB2 + "-f:   " + "Path to file containing a list of PDFs to test with.");
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
        /// <param name="pdfPath"></param>
        /// <returns></returns>
        private static string CreateCommandLineArgsForProdFtImport(string pdfName, string pdfPath)
        {
            return CreateCommandLineArgsForMicsFtImport(pdfName, pdfPath);
        }


        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="pdfName"></param>
        /// <returns></returns>
        private static string CreateCommandLineArgsForFtPrint(string pdfName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" " + DB_NAME);
            sb.Append(" " + PROJECT_CODE);
            sb.Append(" dummy " + pdfName);

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
            sb.Append(" " + PROJECT_CODE);
            sb.Append(" " + pdfName);

            return sb.ToString();
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="pdfName"></param>
        /// <returns></returns>
        private static string CreateCommandLineArgsForMicsCsFtPrint(string pdfName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" " + DB_NAME);
            sb.Append(" " + PROJECT_CODE);
            sb.Append(" " + pdfName);

            return sb.ToString();
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="pdfName"></param>
        /// <returns></returns>
        private static string CreateCommandLineArgsForMicsCs(string pdfName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" " + DB_NAME);
            sb.Append(" " + PROJECT_CODE);
            sb.Append(" -o" + TS_MICS_CS_OUT_DIR + @"\MICS#_FtImport_" + pdfName + ".out");
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

