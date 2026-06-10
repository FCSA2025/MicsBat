# Documented File: ComsearchToTAFL.cs
**Repository Path:** `ComsearchToTAFL\ComsearchToTAFL.cs`
**Primary Layer:** `ComsearchToTAFL`
**Namespace:** `ComsearchToTAFL`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


/// <summary>
/// This program reads from a Comsearch-provided CSV text file, converts the Comsearch link data 
/// to TAFL link data and writes this to an output CSV text file; this TAFL-formatted CSV file   
/// can then be imported into the MDB tables using FCSA's existing import toolset; the Comsearch 
/// input data usually covers the 70-90 GHz band for the entire USA; by default, the Comsearch   
/// data is geographically filtered to retain only links that are within 50 km of the            
/// Canada-USA border (southern and Alaskan); optionally, the user can prescribe another         
/// distance-to-border to be used for geographic filtering.   
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - ComsearchToTAFL.png" ""
/// </remarks>
namespace ComsearchToTAFL
{
    /// <summary>
    /// This class provides the Main() method for the ComsearchToTAFL console program.
    /// </summary>
    public class ComsearchToTAFL
    {
        private static double mMaxDistToBorderKm = 50.0;
        private static string mImportCsvFilePath = @"";
        private static string mExportTaflFilePath = @"";

        /// <summary>
        /// The Main() method for the ComsearchToTAFL console program.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if true
                string mLog2FilePath = @"d:\MicsBatchLogs\ComsearchToTAFL.log";
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
                //============================================================================================

                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                // Write out the command-line arguments.
                Console.Write("\n\nComsearch import file:    {0}", mImportCsvFilePath);
                Console.Write("\nTAFL export file:         {0}", mExportTaflFilePath);
                Console.Write("\nDistance from border:     {0} km.", mMaxDistToBorderKm);

                // Check that the import file can be read from.
                string errMsg;
                if (!ImportExport.CanReadFromFile(mImportCsvFilePath, out errMsg))
                {
                    Console.Write("\n{0}\n{1}", mImportCsvFilePath, errMsg);
                    Log2.v("Cannnot read from " + mImportCsvFilePath);
                    Application.ExitQuietly(Error.CANNOTREADIMPORTFILE);
                }

                // Check that the export file can be written to.
                if (!ImportExport.CanWriteToFile(mExportTaflFilePath, out errMsg))
                {
                    Console.Write("\n{0}\n{1}", mExportTaflFilePath, errMsg);
                    Log2.v("Cannnot write to " + mImportCsvFilePath);
                    Application.ExitQuietly(Error.CANNOTWRITEEXPORTFILE);
                }

                // Read the CSV text file containing the Comsearch raw records.
                // Parse the CSV fields and ensure that all records have the same number of fields.
                // The method call returns an array of column titles (the first line of text).
                // The method also returns an array of RawRecord objects.
                List<RawRecord> rawRecords;
                string[] columnTitles;
                ReadVerifyComsearchRawRecords(out rawRecords, out columnTitles);
                Log2.v("After ReadVerifyComsearchRawRecords");

                // Geographically cull the list of RawRecords to retain only those within
                // a prescribed distance of the Canadian border.
                RawRecord.SelectRecordsNearToBorder(rawRecords, mMaxDistToBorderKm, out rawRecords);
                Log2.v("After SelectRecordsNearToBorder");

                // As an intermediate step, convert the RawRecords to NiceRecord objects that
                // are easier to work with.
                string msg = String.Format("\n\nConverting raw Comsearch records to intermediate records:       STARTED ...   ");
                //...Log2.v(msg);
                Console.Write(msg);

                List<NiceRecord> niceRecords = new List<NiceRecord>();
                foreach (RawRecord rawRec in rawRecords)
                {
                    niceRecords.Add(new NiceRecord(rawRec));
                }

                msg = String.Format("FINISHED.");
                //...Log2.v(msg);
                Console.Write(msg);

                // Convert NiceRecords to TAFL records and export to a CSV text file.
                ExportAsTAFLrecords(mExportTaflFilePath, niceRecords);
            }
            catch (Exception e)
            {
                Log2.e("\n\nComsearchToTAFL.Main(): exception caught: " + e.Message);
                Log2.e("\n\nComsearchToTAFL.Main(): stack trace: \n\n" + e.StackTrace);
                Application.Exit(Error.FATAL_EXCEPTION);
            }

        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program reads from a Comsearch-provided CSV text file, converts the Comsearch link data +");
            Console.Write("\r\n to TAFL link data and writes this to an output CSV text file. This TAFL-formatted CSV file   +");
            Console.Write("\r\n can then be imported into the MDB tables using FCSA's existing import toolset. The Comsearch +");
            Console.Write("\r\n input data usually covers the 70-90 GHz band for the entire USA. By default, the Comsearch   +");
            Console.Write("\r\n data is geographically filtered to retain only links that are within 50 km of the            +");
            Console.Write("\r\n Canada-USA border (southern and Alaskan). Optionally, the user can prescribe another         + ");
            Console.Write("\r\n distance-to-border to be used for geographic filtering.   +");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: ComsearchToTAFL  <inputFilePath>  <outputFilePath>  [-d<distKM>]");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        inputFilePath   : the full file path to the input Comsearch CSV text file.");
            Console.Write("\r\n        outputFilePath  : the full file path to the output TAFL CSV text file.");
            Console.Write("\r\n");
            Console.Write("\r\n        --- Optional Parameters -------------------------------------------------------------");
            Console.Write("\r\n");
            Console.Write("\r\n        -d<distKM>      : retain only data within 'distKM' of the Canadian border.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     ComsearchToTAFL  D:\\Users\\ahulme\\Comsearch.csv  D:\\temp\\TAFL.csv  -d18.5");
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
                // Check that we don't just have a minus character.
                if (arg.Length == 1)
                {
                    Console.Write("\r\n Invalid flag: '{0}'", arg);
                    WriteUsageToConsole();
                    Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                }

                // Process the flags.
                string flag = Strings.DropFirstChar(arg).ToUpper();
                switch (arg.ToUpper()[1])
                {
                    case 'D':
                        try
                        {
                            mMaxDistToBorderKm = Convert.ToDouble(arg.Substring(2, arg.Length - 2));
                        }
                        catch
                        {
                            Console.Write("\r\n Invalid option: {0} ; error converting to floating point number.\r\n", arg);
                            WriteUsageToConsole();
                            Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                        }

                        break;
                    default:
                        Console.Write("\r\n Invalid option: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                        break;
                }

            }

            // Parse the regular arguments; there must be qty. 3 of them.
            if (regularArgs.Count != 2)
            {
                Console.Write("\r\n Invalid number of mandatory arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Parse the args.
            mImportCsvFilePath = regularArgs[0];
            mExportTaflFilePath = regularArgs[1];

            //...Log2.v("\nComsearchToTAFL.ParseCommandLineArgs(): INPUTCSVFILEPATH = {0}", INPUTCSVFILEPATH);
            //...Log2.v("\nComsearchToTAFL.ParseCommandLineArgs(): EXPORTTOTAFLCSVFILEPATH = {0}", EXPORTTOTAFLCSVFILEPATH);
            //...Log2.v("\nComsearchToTAFL.ParseCommandLineArgs(): MAX_DIST_TO_BORDER_KM = {0}", MAX_DIST_TO_BORDER_KM);
        }

        /// <summary>
        /// This method reads in the CSV text line records from the prescribed Comsearch import file,
        /// parses the the individual fields, and returns the result as a list of raw record objects.
        /// </summary>
        /// <param name="rawRecords"></param>
        /// <param name="columnTitles"></param>
        public static void ReadVerifyComsearchRawRecords(out List<RawRecord> rawRecords, out string[] columnTitles)
        {
            // 'out' requirement.
            rawRecords = new List<RawRecord>();
            columnTitles = null;

            string msg = String.Format("\n\nReading Comsearch raw records from import file:                 STARTED ...   ");
            //...Log2.v(msg);
            Console.Write(msg);

            // Read all the Comsearch CSV records.
            string[] lines = File.ReadAllLines(mImportCsvFilePath);

            //...Log2.v("\nComsearchToTAFL.ReadVerifyComsearchRawRecords(): INPUTCSVFILEPATH line count = {0}", lines.Count());
            //...Log2.v("\nComsearchToTAFL.ReadVerifyComsearchRawRecords(): First line = \n{0}\n", lines[0]);

            // The first record contains the column titles.
            int numFields = CSV.ParseFields(lines[0], out columnTitles);
            if (numFields != RawRecord.NUM_COLUMNS - 1)
            {
                msg = String.Format("\nComsearchToTAFL.ReadVerifyComsearchRawRecords(): ERROR: record = {0}: numFields = {1} should be {2}", 0, numFields, RawRecord.NUM_COLUMNS - 1);
                Log2.e(msg);
                Console.Write("\n\nERROR: first CSV record (column headings) has incorrect number of fields: {1} should be {2}", 0, numFields, RawRecord.NUM_COLUMNS - 1);
                Application.Exit(1);
            }

            // Parse the CSV records and convert them to RawRecord objects.
            // Note:
            //       - CSV.ParseFields() returns string fields that are de-quoted and trimmed.
            //       - However, the orginal alphabetical case is preserved.
            for (int i = 1; i < lines.Length; i++)
            {
                string lineUpperCase = lines[i].ToUpper();
                string[] fields;
                numFields = CSV.ParseFields(lineUpperCase, out fields);
                if (numFields != RawRecord.NUM_COLUMNS - 1)
                {
                    Log2.e("\n\nComsearchToTAFL.ReadVerifyComsearchRawRecords(): ERROR: {0}", lines[i]);
                    msg = String.Format("\nComsearchToTAFL.ReadVerifyComsearchRawRecords(): ERROR: record = {0}: numFields = {1} should be {2}", i, numFields, RawRecord.NUM_COLUMNS - 1);
                    Log2.e(msg);
                    Console.Write("\nERROR: CSV line# {0} has incorrect number of fields: {1} should be {2}", i, numFields, RawRecord.NUM_COLUMNS);
                    Application.Exit(1);
                }

                RawRecord rawRec = new RawRecord(fields, i);

                rawRecords.Add(rawRec);
            }

            msg = String.Format("\nSUCCESS: {0} Comsearch raw records imported (excluding headings line).", rawRecords.Count);
            //...Log2.v(msg);
            Console.Write("FINISHED.\n{0} lines were read.", lines.Count());
            Console.Write("\nAll lines have qty. {0} fields.", RawRecord.NUM_COLUMNS);

            return;
        }

        /// <summary>
        /// This method receives a List of Comsearch NiceRecord objects, creates the
        /// equivalent TAFL records and writes them to a prescribed CSV formatted
        /// text file.
        /// </summary>
        /// <param name="pathToTAFLexportFile"></param>
        /// <param name="niceRecords"></param>
        public static void ExportAsTAFLrecords(string pathToTAFLexportFile, List<NiceRecord> niceRecords)
        {
            string msg = String.Format("\n\nConverting intermediate records to TAFL records and exporting:  STARTED ...   ");
            //...Log2.v(msg);
            Console.Write(msg);

            // Instantiate a TextWriter object to write the sythnesized TAFL records
            // to a CSV-formatted file. It will overwrite an existing file.
            TextWriter tw = new StreamWriter(mExportTaflFilePath, false, Encoding.UTF8);

            // Convert each niceRecord to TAFL records and write these out to a CSV file.
            int taflCount = 0;
            bool firstLine = true;

            foreach (NiceRecord niceRec in niceRecords)
            {
                List<TAFL> taflList;

                Conversions.ComsearchToIsedTafl(niceRec, out taflList);

                taflCount += taflList.Count();

                foreach (TAFL tafl in taflList) 
                {
                    string fmt = "";
                    if (firstLine)
                    {
                        fmt = "{0}";
                        firstLine = false;
                    }
                    else
                    {
                        fmt = "\n{0}";
                    }
                    tw.Write(fmt, tafl.ToStringAsCSVexportRFC4180(tafl.mNullInds));
                } 
            }

            tw.Close();

            msg = String.Format("FINISHED.");
            //...Log2.v(msg);
            Console.Write(msg);

            Console.Write("\n\nQty. {0} Comsearch records were successfully exported as qty. {1} TAFL records.\n", niceRecords.Count, taflCount);
        }
    }
}

```
