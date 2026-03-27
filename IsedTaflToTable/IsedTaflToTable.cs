using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// This program downloads the current TAFL CSV file from the ISED web page, 
/// prepends a UTF-8 BOM (to ensure survival of characters with French diacritics), 
/// appends qty. 6 additional 'Venn' fields, and then inserts the augmented TAFL 
/// data set into a table on the SQL Server.
/// </summary>
/// <remarks>
/// <para></para>
/// The TS TAFL data is downloaded from the Government of Canada web file <a href="https://www.ic.gc.ca/engineering/SMS_TAFL_Files/TAFL_LTAF_Fixe.zip">https://www.ic.gc.ca/engineering/SMS_TAFL_Files/TAFL_LTAF_Fixe.zip</a>.
/// </para><para>
/// The ES TAFL data is downloaded from the Government of Canada web file <a href="https://www.ic.gc.ca/engineering/SMS_TAFL_Files/TAFL_LTAF_Satellite.zip">https://www.ic.gc.ca/engineering/SMS_TAFL_Files/TAFL_LTAF_Satellite.zip</a>.
/// </para><para>
/// The command-line usage is:
/// </para>
/// \image html "Usage - IsedTaflToTable.png" ""
/// </remarks>
namespace IsedTaflToTable
{
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides the Main() method for the MICS program IsedTaflToTable.exe
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class IsedTaflToTable
    {
        // Here is the ISED TAFL Zipped download file URL.
        const string webFileURL_TS = "https://www.ic.gc.ca/engineering/SMS_TAFL_Files/TAFL_LTAF_Fixe.zip";
        const string webFileURL_ES = "https://www.ic.gc.ca/engineering/SMS_TAFL_Files/TAFL_LTAF_Satellite.zip";

        const string TAFL_CSV_FILE_NAME = @"TAFL_LTAF_FIXE.CSV";

        const string TEMP_DIR = @"d:\perflogs";

        public static string mType = "";

        /// <summary>
        /// This is the Main() method for the MICS IsedTaflToTable program.
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\IsedTaflToTable.log";
                if (Log2.SetLogFilePath(mLog2FilePath))
                {
                    Log2.Erase();
                    Log2.Set(Log2.FileOpenClose.PER_SESSION);
                    Log2.Set(Log2.WriteMode.ENABLED);
                    Log2.Set(Log2.Level.VERBOSE);
                    Info.BuildMetaData = Info.CollateExeMetaData();
                    Log2.v("\nBuild: " + Info.BuildMetaData);
                }
                else
                {
                    Console.Error.Write("\r\nERROR: could not open Log2 file: " + mLog2FilePath);
                }
#endif

                // We want to measure the elapsed time for execution of this program so make
                // a record of the time now.
                DateTime startDateTime = DateTime.Now;

                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                GetEnvVariablesForUtConnect();

                // Establish an FCSA user session with the database.
                int rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    Console.Write("TBD -- Can't connect to database {0}.\r\n", Info.DbName);
                    Application.ExitQuietly(11);
                }

                // Connection to the ISED TAFL download file URL uses Hypertext Transfer Protocol
                // Secure (HTTPS) and so we need to tell the system what security protocols it
                // needs to contend with.
                System.Net.ServicePointManager.SecurityProtocol =
                                                                    SecurityProtocolType.Tls12 |
                                                                    SecurityProtocolType.Tls11 |
                                                                    SecurityProtocolType.Tls;



                string webFileURL = (mType == "TS") ? webFileURL_TS: webFileURL_ES;

                Console.Write("\nAttempting to download ISED TAFL zip file::\n    URL   : {0}", webFileURL);

                // Download the zip file to somewhere acessible by the current user.
                string downloadedZipFilePath = Path.Combine(TEMP_DIR, Path.GetFileName(webFileURL));

                // Perform the Zip file download asynchronously and poll until it is complete.
                IAsyncResult result = DownloadWebFileAsync(webFileURL, downloadedZipFilePath);

                Console.Write("\nTAFL zip file downloading to:  {0}", downloadedZipFilePath);

                // Start polling for completion of the asynchronous download task. 
                int elapsedTime = 0;
                int MAX_ELAPSED_TIME = 60;
                Console.Write("\n    Status: in progress ... (elapsedTime = {0} seconds)", elapsedTime);
                while (result.IsCompleted != true)
                {
                    Thread.Sleep(1000);
                    elapsedTime++;
                    Console.Write("\r    Status: in progress ... (elapsedTime = {0} seconds)", elapsedTime);
                    if (elapsedTime > MAX_ELAPSED_TIME)
                    {
                        Console.Write("\r{0,100}", "");
                        Console.Write("\r    Status: ERROR: time-out (elapsedTime = {0} seconds)", elapsedTime);
                        Log2.e("\n\nIsedTaflToTable.Main(): ERROR: method DownloadWebFileAsync() timed-out (elapsedTime = {0} seconds)", elapsedTime);
                        Application.ExitQuietly(666);
                    }
                }
                Console.Write("\r{0,100}", "");

                // Verify that a non-empty file was actually downloaded - or not.
                if (File.Exists(downloadedZipFilePath))
                {
                    Console.Write("\r    Status: download completed (elapsedTime = {0} seconds)", elapsedTime);
                    Console.Write("\n    Downloaded Zip file : {0}", downloadedZipFilePath);
                }
                else
                {
                    Console.Write("\r    Status: ERROR: download failed (incorrect URL?).");
                    Application.ExitQuietly(666);
                }

                // Commence the extraction from the Zip file.
                Console.Write("\n\nAttempting to  unpack the Zip file:");
                Console.Write("\n    Status: in progress ...");

                List<string> allExtractedFilePaths;
                DateTime dateTimeLastWrite;

                // Launch the extraction from the Zip archive and get a list of files extracted
                // and the most recent 'File Last Written to' date.
                UnZipToDirectory(downloadedZipFilePath, TEMP_DIR, out allExtractedFilePaths, out dateTimeLastWrite);

                Console.Write("\r    Status: un-zipping completed.          ");

                // Display the extracted file paths.
                foreach (string efp in allExtractedFilePaths)
                {
                    Console.Write("\n    Extracted File(s) : {0}", efp);
                }

                string pathToCSVFile = "";

                // The Zip file should have only contained a single entry.
                if (allExtractedFilePaths.Count == 1)
                {
                    // The file path of the extracted CSV file is the first (and only) entry in the list.
                    pathToCSVFile = allExtractedFilePaths[0];
                }
                else if (allExtractedFilePaths.Count > 1)
                {
                    Console.Write("\n    WARNING: zip archive contains multiple files:");
                    foreach (string filePath in allExtractedFilePaths)
                    {
                        if (Path.GetFileName(filePath).ToUpper().Equals(TAFL_CSV_FILE_NAME))
                        {
                            pathToCSVFile = filePath;
                            break;
                        }
                    }

                    if (String.IsNullOrWhiteSpace(pathToCSVFile))
                    {
                        Console.Write("\n    ERROR: ZIP file does not contain a TAFL TS file named: {0}", TAFL_CSV_FILE_NAME);
                        Application.ExitQuietly(666);
                    }
                }
                else if (allExtractedFilePaths.Count == 0)
                {
                    Console.Write("\n    ERROR: ZIP file contains no files.");
                    Application.ExitQuietly(666);
                }

                // If the CSV already has a UTF-8 BOM then fine - otherwise prepend one.
                if (FileHasUTF8BOM(pathToCSVFile))
                {
                    Console.Write("\n\nExtracted file already has a UTF-8 BOM.");
                }
                else
                {
                    PrependUTF8BOM(pathToCSVFile);
                    Console.Write("\n\nExtracted file did not have UTF-8 BOM; a BOM has been prepended.");
                }

                // Use the 'last written to' data as a label for the SQL table.
                string dateLastWrite = dateTimeLastWrite.ToString("u").Substring(0, 10).Replace("-", "");

                // If the command line option to prescribe the SQL table name has not been used
                // then default the target table name for type "TS" to "IsedTafl_" + dateLastWrite.
                // If the type is "ES" then indicate this in the default name.
                if (String.IsNullOrWhiteSpace(Info.DestName))
                {
                    if (mType == "TS") Info.DestName = "IsedTafl_" + dateLastWrite;
                    if (mType == "ES") Info.DestName = "IsedTafl_ES_" + dateLastWrite;
                }

                Console.Write("\n\nContents of the downloaded ZIP file were last changed on {0} (YYYYMMDD).", dateLastWrite);
#if true
                // Check whether a table with the same name already exists and, if so, what to do about it.
                if (Ssutil.IntTableExist(Info.DestName))
                {
                    Console.Write("\n\nWARNING: a table named {0}.{1} already exists: do you still want to proceed? (Y/N): ", Info.GlobalSchema, Info.DestName);
                    Console.Out.Flush();
                    char key = Console.ReadKey().KeyChar;
                    if ((key == 'Y') || (key == 'y'))
                    {
                        // Proceed ... the existing table will be overwritten.
                    }
                    else
                    {
                        // Any keyboard input other than Y or y causes the application to terminate.
                        // Terminate the FCSA user's DB session.
                        Ssutil.UtDisconnect(1);
                        Application.ExitQuietly(1);
                    }
                }
#endif
                Console.Write("\n\nInserting CSV TAFL records into SQL table    >>> {0}.{1} <<<", Info.GlobalSchema, Info.DestName);

                // perform the insertion of the CSV file TAFL records in the SQL Server.
                TaflCsvToSqlTable(pathToCSVFile, Info.DestName);

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                // Calculate and write out the elapsed time for execution.
                TimeSpan ts = DateTime.Now.Subtract(startDateTime);
                string elapsedTimeString = string.Format("{0}:{1}:{2}",
                                                            ts.Hours.ToString("00"),
                                                            ts.Minutes.ToString("00"),
                                                            ts.Seconds.ToString("00"));
                Console.Write("\n\nElapsed time = {0}\n", elapsedTimeString);

                // We're done.
                Application.ExitQuietly(0);

            }
            catch (Exception e)
            {
                string message = String.Format("\r\n\r\nIsedTaflToTable.Main(): exception caught: {0}\n{1}", e.Message, e.StackTrace);
                Log2.e(message);
                Console.Error.Write(message);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

        }

        /// <summary>
        /// This method returns true if the prescribed file contains a UTF-8 BOM.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool FileHasUTF8BOM(string filePath)
        {
            bool alreadyHasBOM = false;

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists) return false;
                if (fileInfo.Length < 3) return false;

                // Check if the prescribed file already has a UTF-8 BOM.     
                byte[] BOM = new byte[3] { 0xEF, 0xBB, 0xBF };
                byte[] bytes = new byte[3];

                using (FileStream fs = File.OpenRead(filePath))
                {
                    fs.Read(bytes, 0, bytes.Length);
                }

                alreadyHasBOM = (bytes[0] == BOM[0]) && (bytes[1] == BOM[1]) && (bytes[2] == BOM[2]);
            }
            catch (Exception e)
            {
                string message = String.Format("\r\n\r\nIsedTaflToTable.FileHasUTF8BOM(): exception caught: {0}\n{1}", e.Message, e.StackTrace);
                Log2.e(message);
                Console.Error.Write(message);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

            return alreadyHasBOM;
        }

        /// <summary>
        /// This method prepends a UTF-8 BOM at the beginning of a file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static int PrependUTF8BOM(string filePath)
        {
            int retVal = Constant.FAILURE;

            try
            {
                // Check if the prescribed file already has a UTF-8 BOM.     
                byte[] BOM = new byte[3] { 0xEF, 0xBB, 0xBF };

                // If the prescribed file already has a UTF-8 BOM we can return.
                if (FileHasUTF8BOM(filePath))
                {
                    return Constant.SUCCESS;
                }
                // We need to prepend the UTF-8 BOM at the start of the prescribed file.
                else
                {
                    // Open a temporary file.
                    string tempFilePath = System.IO.Path.GetTempFileName();

                    using (FileStream tempFs = File.Create(tempFilePath))
                    {
                        // Write the 3-byte UTF-8 BOM to the temporary file.
                        tempFs.Write(BOM, 0, BOM.Length);

                        // Write the contents of the prescribed file to the temporary file.
                        using (FileStream fs = File.OpenRead(filePath))
                        {
                            fs.CopyTo(tempFs);
                        }

                        // Delete the prescribed file.
                        File.Delete(filePath);

                        // Overwrite the contents of the temporary file to the prescribed file.
                        using (FileStream fs = File.OpenWrite(filePath))
                        {
                            tempFs.Position = 0;
                            tempFs.CopyTo(fs);
                        }
                    }

                    // Delete the temporary file.
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception e)
            {
                // Something went wrong.
                Log2.e("\n\nIsedTaflToTable.PrependUTF8BOM(): ERROR: exception: {0}\n{1}", e.Message, e.StackTrace);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

            return retVal;
        }

        /// <summary>
        /// This method un-zips (extracts) entries from a prescribed Zip archive to a prescribed directory; it
        /// returns a list of the file paths of the extracted entries and the most recent 'last written to'
        /// date of an extracted entry.
        /// </summary>
        /// <param name="zipFilePath"></param>
        /// <param name="destinationDirectoryName"></param>
        /// <param name="allExtractedFilePaths"></param>
        /// <param name="dateTimeLastWrite"></param>
        public static void UnZipToDirectory(string zipFilePath, string destinationDirectoryName, out List<string> allExtractedFilePaths, out DateTime dateTimeLastWrite)
        {
            // 'out' requirement.
            allExtractedFilePaths = new List<string>();
            dateTimeLastWrite = DateTime.MinValue;

            try
            {
                ZipArchive zipArchive = System.IO.Compression.ZipFile.OpenRead(zipFilePath);

                // Note that if any filePath to be extracted already exists the following code
                // will overwrite it.
                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {
                    string completeFilePath = Path.Combine(destinationDirectoryName, zipArchiveEntry.FullName);

                    if (zipArchiveEntry.Name == "")
                    {// Assuming Empty for Directory
                        Directory.CreateDirectory(Path.GetDirectoryName(completeFilePath));
                        continue;
                    }
                    // create dirs
                    var dirToCreate = destinationDirectoryName;
                    for (var i = 0; i < zipArchiveEntry.FullName.Split('/').Length - 1; i++)
                    {
                        var s = zipArchiveEntry.FullName.Split('/')[i];
                        dirToCreate = Path.Combine(dirToCreate, s);
                        if (!Directory.Exists(dirToCreate))
                            Directory.CreateDirectory(dirToCreate);
                    }

                    zipArchiveEntry.ExtractToFile(completeFilePath, true);

                    allExtractedFilePaths.Add(completeFilePath);

                    DateTime entryDateTimeLastWrite = zipArchiveEntry.LastWriteTime.DateTime;
                    if (dateTimeLastWrite < entryDateTimeLastWrite) dateTimeLastWrite = entryDateTimeLastWrite;
                }
            }
            catch (Exception e)
            {
                string message = String.Format("\r\n\r\nIsedTaflToTable.UnZipToDirectory(): exception caught: {0}\n{1}", e.Message, e.StackTrace);
                Log2.e(message);
                Console.Error.Write(message);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

        }

        /// <summary>
        /// This method performs the asynchronous download of a prescribed file URL
        /// and its writing to a prescribed file path on the Windows system.
        /// </summary>
        /// <param name="webFileURL"></param>
        /// <param name="downloadedZipFilePath"></param>
        /// <returns></returns>
        public static async Task DownloadWebFileAsync(string webFileURL, string downloadedZipFilePath)
        {
            try
            {
                if (File.Exists(downloadedZipFilePath)) File.Delete(downloadedZipFilePath);

                var httpClient = new HttpClient();

                using (var stream = await httpClient.GetStreamAsync(webFileURL))
                {
                    using (var fileStream = new FileStream(downloadedZipFilePath, FileMode.CreateNew))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
            }
            catch (Exception e)
            {
                string message = String.Format("\r\n\r\nIsedTaflToTable.DownloadWebFileAsync(): exception caught: {0}\n{1}", e.Message, e.StackTrace);
                Log2.e(message);
                Console.Error.Write(message);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

            return;
        }

        /// <summary>
        /// This methods gets the values of Windows environment variables that
        /// are required to execute GetCoords; specifically these are 'MICSUSER'
        /// and 'PASSWORD' that are required for a successful call to UtConnect().
        /// </summary>
        public static void GetEnvVariablesForUtConnect()
        {
            // Get the user's MICS ID from the environment.
            Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                Log2.e("\r\nFeImport.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
                Application.ExitQuietly(Error.ENVVARMICSUSERNOTSET);
            }

            // Get the user's password from the environment.
            Info.Password = Environment.GetEnvironmentVariable("PASSWORD");         // REQUIRED (but can be anything).
            if (String.IsNullOrWhiteSpace(Info.Password))
            {
                // Password just has to be set to something; its value is never used.
                Info.Password = "Bananarama";
            }
        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            /// This program downloads the current TAFL CSV file from the ISED web page, 
            /// prepends a UTF-8 BOM (to ensure survival of characters with French diacritics), 
            /// appends qty. 6 additional 'Venn' fields, and then inserts the augmented TAFL 
            /// data set into a table on the SQL Server.
            Console.Write("\r\n");
            Console.Write("\r\n This program downloads the current TAFL CSV file from the ISED web page,");
            Console.Write("\r\n prepends a UTF-8 BOM (to ensure survival of characters with French diacritics),");
            Console.Write("\r\n appends qty. 6 additional 'Venn' fields, and then inserts the augmented TAFL ");
            Console.Write("\r\n data set into a table on the SQL Server.");

            Console.Write("\r\n");
            Console.Write("\r\n USAGE: IsedTaflToTable <dbName> <type> [-t<tableName>]");
            Console.Write("\r\n =====");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        type             : 'TS' or 'ES'.");
            Console.Write("\r\n        tableName        : name for SQL table, e.g. 'myIsedTafl'.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n      IsedTaflToTable fcsa TS");
            Console.Write("\r\n      IsedTaflToTable fcsa ES -nmyIsedTafl");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. The environment variable MICSUSER *MUST* be set.");
            Console.Write("\r\n       2. The default SQL table name will be like 'IsedTafl_20221207");
            Console.Write("\r\n          where the number indicates the YYYYMMDD of the date on which");
            Console.Write("\r\n          the contents of the Zip file were last written to.");
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
                // Check that we have an option letter and payload.
                if (arg.Length == 1 || arg.Length == 2)
                {
                    Console.Write("\r\n Invalid flag: '{0}'", arg);
                    WriteUsageToConsole();
                    Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                }

                // Process the flags.
                string flag = arg.Substring(1, 1).ToUpper();
                Info.DestName = "";
                switch (flag)
                {
                    case "T":
                        Info.DestName = arg.Substring(2, arg.Length - 2);
                        break;
                    default:
                        Console.Write("\r\n Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                        break;
                }
            }

            // Parse the regular arguments; there must be qty. 2 of them.
            if (regularArgs.Count != 2)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Parse the args.
            Info.DbName = regularArgs[0];
            mType = regularArgs[1].ToUpper();

            // Validate the type.
            if ((mType != "TS") && (mType != "ES"))
            {
                Console.Write("\n\n ERROR: <type> must be TS or ES.\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }
        }


        /// <summary>
        /// This method performs the insertion of TAFL records in a precribed CSV text
        /// file to a prescribed table on the SQL Server; the current user's default
        /// schema is used to fully-qualify the SQL table name.
        /// </summary>
        /// <param name="pathToCsvFile"></param>
        /// <param name="sqlTableName"></param>
        public static void TaflCsvToSqlTable(string pathToCsvFile, string sqlTableName)
        {
            int numTaflRecords;
            int donePC;
            DateTime dateTimeStart;
            DateTime dateTimeNow;

            string CONSTRAINT_ID = "PK_" + Info.GlobalSchema + "_" + sqlTableName;

            SQLRETURN sqlRet = 0;
            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            DoQuery(hStmt, "SET ANSI_NULLS ON");
            DoQuery(hStmt, "SET QUOTED_IDENTIFIER ON");
            DoQuery(hStmt, "SET ANSI_PADDING ON");

            DoQuery(hStmt, String.Format("IF OBJECT_ID('{0}.{1}') IS NOT NULL DROP TABLE {0}.{1};", Info.GlobalSchema, sqlTableName));

            string query = String.Format(TAFL.CREATE_TABLE, Info.GlobalSchema, sqlTableName, CONSTRAINT_ID);
            DoQuery(hStmt, query);

            // Read in the contents of the CSV file.
            string[] lines = null;
            try
            {
                lines = File.ReadAllLines(pathToCsvFile);
            }
            catch (Exception e)
            {
                string message = String.Format("\r\n\r\nIsedTaflToTable.TaflCsvToSqlTable(): exception caught: {0}\n{1}", e.Message, e.StackTrace);
                Log2.e(message);
                Console.Error.Write(message);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

            numTaflRecords = lines.Length;

            Console.Write("\n");
            dateTimeStart = DateTime.Now;

            for (int lineNum = 1; lineNum <= lines.Length; lineNum++)
            {
                // Convert each line to upper case.
                string line = lines[lineNum - 1].ToUpper();

                string[] fields;

                CSV.ParseFields(line, out fields);

                if (fields.Length != 61)
                {
                    Console.Write("\n\nERROR: line {0} of CSV file has {1} fields - should be 61.", lineNum, fields.Length);
                    Application.ExitQuietly(666);
                }

                TAFL tafl;
                SQLLEN[] nullInds;
                string[] csvFields;

                CSV.ParseFields(line, out csvFields);

                DeLintFields(ref csvFields);

                TAFL.CreateTAFLfromCsvFields(csvFields, out tafl, out nullInds);

                tafl.Keyfield = lineNum;
                nullInds[TAFL.KEYFIELD] = Constant.DB_NOT_NULL;

                if (lineNum % 100 == 0)
                {
                    dateTimeNow = DateTime.Now;
                    TimeSpan ts = dateTimeNow - dateTimeStart;
                    double elapsedTimeSec = ts.TotalSeconds;
                    double rateLinePerSec = lineNum / elapsedTimeSec;
                    double remainingSec = (numTaflRecords - lineNum) / rateLinePerSec;

                    string msg;
                    if (remainingSec > 60)
                    {
                        msg = String.Format("{0:F0} minutes", 0.5 + remainingSec / 60);
                    }
                    else
                    {
                        msg = String.Format("{0:F0} seconds", remainingSec);
                    }

                    donePC = (int)(0.5 + (100.0 * lineNum / (double)numTaflRecords));
                    Console.Write("\r{0,100}", "");
                    Console.Write("\rInserted TAFL record: {0:n0} of {1:n0}  ({2,3}% complete: time remaining = {3}.)", lineNum, numTaflRecords, donePC, msg);
                }

                string sqlFullTableName = Info.GlobalSchema + "." + sqlTableName;

                int retVal = DynTAFL.InsertTAFL(sqlFullTableName, tafl, nullInds);

                if (retVal != Constant.SUCCESS)
                {
                    Console.Write("\n\nERROR: SQL INSERT failed for CSV file line number {0}", lineNum);
                    Application.ExitQuietly(666);
                }
            }

            Console.Write("\r{0,100}", "");
            Console.Write("\rInserted ISED record: {0:n0} of {1:n0}  (100% complete)", numTaflRecords, numTaflRecords);
        }
         
        /// Some TAFL records fields just contain a '.' or '-' to indicate 'empty';
        /// this method identifies these instances and replaces them with empty strings.
        /// </summary>
        /// <param name="fields"></param>
        public static void DeLintFields(ref string[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i] == "-") fields[i] = "";
                if (fields[i] == ".") fields[i] = "";
            }

            //if (fields[TAFL.RXUNFADEDRECEIVEDSIGNALLEVELDBW] == "-") fields[TAFL.RXUNFADEDRECEIVEDSIGNALLEVELDBW] = "";
            //if (fields[TAFL.RXTHRESHOLDSIGNALLEVELFORBER10E3DBW] == "-") fields[TAFL.RXTHRESHOLDSIGNALLEVELFORBER10E3DBW] = "";
            //if (fields[TAFL.ANALOGCAPACITYCHANNELS] == ".") fields[TAFL.ANALOGCAPACITYCHANNELS] = "";
            //if (fields[TAFL.TXEFFECTIVERADIATEDPOWERERPDBW] == "-") fields[TAFL.TXEFFECTIVERADIATEDPOWERERPDBW] = "";
            //if (fields[TAFL.TXTRANSMITTERPOWERW] == "-") fields[TAFL.TXTRANSMITTERPOWERW] = "";
            //if (fields[TAFL.HORIZONTALPOWERW] == "-") fields[TAFL.HORIZONTALPOWERW] = "";
            //if (fields[TAFL.VERTICALPOWERW] == "-") fields[TAFL.VERTICALPOWERW] = "";

        }

        /// <summary>
        /// This method encapsulates the submission of a query to the ODBC / SQL Server 
        /// without having to specify the length of the query string.
        /// </summary>
        /// <param name="hStmt"> - an open ODBC statement handle</param>
        /// <param name="query"> - SQL query to be submitted.</param>
        /// <returns></returns>
        public static int DoQuery(SQLHANDLE hStmt, string query)
        {
            SQLRETURN sqlRet = 0;

            sqlRet = ODBC.SQLExecDirect(hStmt, query, query.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n" + ODBC.GetDiagnostics(hStmt, query));
            }

            return sqlRet;
        }


    }
}

