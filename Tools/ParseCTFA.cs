using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tools
{
    using SQLLEN = Int64;

    public class ParseCTFA
    {
        private const string CFTA_TEXT_FILE_PATH = @"D:\Users\ahulme\CTFA2018.txt";

        private const string templateFreqRange = @"^(\d[0123456789 .]+)-\s*(\d[0123456789 .]+)\s*";

        private const string fixedSatelliteSpaceToEarth = @"FIXED-SATELLITE (space-to-Earth)";
        private const string fixedSatelliteEarthToSpace = @"FIXED-SATELLITE (Earth-to-space)";
        private const string fixedTerrestrial = @"FIXED";

        public enum Units { Unknown, KHz, MHz, GHz };
        public enum Usage { UNKNOWN, TS, ESUP, ESDN, ESUPDN, TS_ESUPDN, TS_ESUP, TS_ESDN };

        public class Record
        {
            public double freqLoKHz = Double.MinValue;
            public double freqHiKHz = Double.MinValue;
            public bool esUplink = false;
            public bool esDownlink = false;
            public bool tsUsage = false;
            public int ctfaPage = 0;
            public Usage usage = Usage.UNKNOWN;
            public string srspIDs = "";
            public string bndcdes = "";

            public void DetermineUsage()
            {
                usage = Usage.UNKNOWN;

                if (tsUsage && !esUplink && !esDownlink)
                {
                    usage = Usage.TS;
                }
                else if (!tsUsage && esUplink && !esDownlink)
                {
                    usage = Usage.ESUP;
                }
                else if (!tsUsage && !esUplink && esDownlink)
                {
                    usage = Usage.ESDN;
                }
                else if (!tsUsage && esUplink && esDownlink)
                {
                    usage = Usage.ESUPDN;
                }
                else if (tsUsage && esUplink && esDownlink)
                {
                    usage = Usage.TS_ESUPDN;
                }
                else if (tsUsage && esUplink && !esDownlink)
                {
                    usage = Usage.TS_ESUP;
                }
                else if (tsUsage && !esUplink && esDownlink)
                {
                    usage = Usage.TS_ESDN;
                }

            }

            public Record DeepCopy()
            {
                Record dc = new Record();

                dc.freqLoKHz = freqLoKHz;
                dc.freqHiKHz = freqHiKHz;
                dc.esUplink = esUplink;
                dc.esDownlink = esDownlink;
                dc.tsUsage = tsUsage;
                dc.ctfaPage = ctfaPage;
                dc.usage = usage;
                dc.srspIDs = srspIDs;
                dc.bndcdes = bndcdes;

                return dc;
            }

            public override string ToString()
            {
                return String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}", freqLoKHz, freqHiKHz,
                                        esUplink ? 1 : 0,
                                        esDownlink ? 1 : 0,
                                        tsUsage ? 1 : 0,
                                        ctfaPage, usage, srspIDs, bndcdes.Trim());
            }

            public static string Banner()
            {
                return String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}", "freqLoKHz", "freqHiKHz", "esUplink", "esDownlink", "tsUsage", "ctfaPage", "usage", "srspIDs", "bndcdes");
            }

            public static RecordComparer Comparer = new RecordComparer();

            public class RecordComparer : IComparer<Record>
            {
                public int Compare(Record x, Record y)
                {
                    return x.freqLoKHz.CompareTo(y.freqLoKHz);
                }
            }
        }

        public static void Go(string[] args)
        {
            try
            {
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\ParseCTFA.log";
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
                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                GetEnvVariablesForUtConnect();

                // Select the database.
                Info.DbName = "fcsa";

                // Establish an FCSA user session with the database.
                int rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    Console.Error.Write("TBD -- Can't connect to database {0}.\r\n", Info.DbName);

                    Application.ExitQuietly(11);
                }

                string table = "hulme.SRSPbands";
                int cursor = DynSRSP.SelectSRSP(table, "", "");

                SRSP srspRecord;
                SQLLEN[] nullInds;

                List<SRSP> srspList = new List<SRSP>();

                while (ODBC.IsOK((short)DynSRSP.FetchSRSP(cursor, out srspRecord, out nullInds)))
                {
                    Console.Error.Write("\n{0}", srspRecord.ToString());
                    srspList.Add(srspRecord);
                }

                DynSRSP.CloseSRSP(cursor);

                //==================================================

                string[] allLines = File.ReadAllLines(CFTA_TEXT_FILE_PATH);

                if ((allLines == null) || (allLines.Length == 0))
                {
                    Console.Error.Write("\n\nERROR: allLines[] is either null or an empty.");
                }

                Units units = Units.Unknown;
                int pageNum = 0;

                Record currentRecord = null;

                List<Record> records = new List<Record>();

                LineParser lineParser = new LineParser(allLines);

                while (!lineParser.EndOfText)
                {
                    //Console.Write("\n{0},  {1}", lineParser.Index, lineParser.Line);

                    if (LineParser.LineContains(lineParser.Line, "CANADIAN TABLE OF FREQUENCY ALLOCATIONS", true))
                    {
                        lineParser.MoveAhead();
                        switch (lineParser.Line.Trim())
                        {
                            case "KHz":
                                units = Units.KHz;
                                break;
                            case "MHz":
                                units = Units.MHz;
                                break;
                            case "GHz":
                                units = Units.GHz;
                                break;
                            default:
                                Console.Error.Write("\n\nERROR: missing units at line index {0}, page {1}.", lineParser.Index, pageNum);
                                Environment.Exit(1);
                                break;
                        }

                        lineParser.MoveAhead();
                        try
                        {
                            pageNum = Convert.ToInt32(lineParser.Line);
                        }
                        catch
                        {
                            Console.Error.Write("\n\nERROR: could not read page number at line index {0}.", lineParser.Index);
                            Environment.Exit(1);
                        }

                        //Console.Write("\n\n=============================== units = {0}, page = {1}.", units, pageNum);

                    }

                    if (Regex.IsMatch(lineParser.Line.Trim(), templateFreqRange))
                    {
                        //Console.Write("\n\n******** Range line: {0}", lineParser.Line);

                        double freqLoKHz, freqHiKHz;
                        int retVal = ParseFrequencyRange(lineParser.Line, units, out freqLoKHz, out freqHiKHz);

                        // Add the current record to the list.
                        if (currentRecord != null)
                        {
                            records.Add(currentRecord);
                            //Console.Write("\n^^^^^^^^^^^^^^^^^^^^^^^^^^ RECORD ADDED - {0}", records.Count);
                        }

                        // Start a new record.
                        currentRecord = new Record();
                        currentRecord.freqLoKHz = freqLoKHz;
                        currentRecord.freqHiKHz = freqHiKHz;
                        currentRecord.ctfaPage = pageNum;
                    }

                    if (!lineParser.LineContains("MOBILE", false))
                    {
                        if (lineParser.LineContainsIgnoreWhitespace(fixedSatelliteEarthToSpace, false))
                        {
                            //Console.Write("\n$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ {0}", lineParser.Line);
                            currentRecord.esUplink = true;
                        }
                        else if (lineParser.LineContainsIgnoreWhitespace(fixedSatelliteSpaceToEarth, false))
                        {
                            //Console.Write("\n$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ {0}", lineParser.Line);
                            currentRecord.esDownlink = true;
                        }
                        else if (!lineParser.EndOfText && lineParser.Line.ToUpper().Contains(fixedTerrestrial))
                        {
                            //Console.Write("\n$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ {0}", lineParser.Line);
                            currentRecord.tsUsage = true;
                        }
                    }

                    //Console.Write("\nXXXXXXXXXXXXXXXX Number of records = {0}", records.Count);

                    lineParser.MoveAhead();
                }

                // Add the final current record.
                if (currentRecord != null) records.Add(currentRecord);

                //Console.Write("\nNumber of records = {0}", records.Count);

                // Determine the TS and ES usage for all records.
                foreach (Record record in records) record.DetermineUsage();

                // Truncate the list of records to keep only those that have ES or TS usage.
                List<Record> tempList = new List<Record>();
                foreach (Record record in records)
                {
                    if (record.usage != Usage.UNKNOWN) tempList.Add(record);
                }
                records = tempList;

                // Populate the srspIDs fields.
                foreach (Record record in records)
                {
                    foreach (SRSP srsp in srspList)
                    {
                        if (IsOverlap((double)srsp.freqLoKHz, (double)srsp.freqHiKHz, record.freqLoKHz, record.freqHiKHz))
                        {
                            if (!(srsp.srspID.Contains("101") && record.usage == Usage.TS))
                            {
                                record.srspIDs += " " + srsp.srspID + ";";
                            }
                        }
                    }
                }

                // Populate the bndcdes fields.
                List<SdBand> sdBandList;
                DynSdbBand.SetTableName("hulme.sd_band");
                DynSdbBand.GetSdbBandList(out sdBandList);

                foreach (Record record in records)
                {
                    foreach (SdBand sdBand in sdBandList)
                    {
                        if (IsOverlap((double)sdBand.blo, (double)sdBand.bhi, record.freqLoKHz, record.freqHiKHz))
                        {
                            record.bndcdes += " " + sdBand.bndcde.Trim() + ";";
                        }
                    }
                }

                // Write out the list of records.
                Console.Write("\n\n============= All TS and ES records ================\n");
                Console.Write("\n{0}", Record.Banner());
                foreach (Record record in records)
                {
                    Console.Write("\n{0}", record.ToString());
                }

                Console.Write("\n\nTotal number of records = {0}\n", records.Count());

                // Now create 4 lists, one for each of ES-up, ES_down, ES and TS.
                List<Record> esUpRecords = new List<Record>();
                List<Record> esDownRecords = new List<Record>();
                List<Record> esRecords = new List<Record>();
                List<Record> tsRecords = new List<Record>();

                foreach (Record record in records)
                {
                    if (record.esUplink) esUpRecords.Add(record);
                    if (record.esDownlink) esDownRecords.Add(record);
                    if (record.esUplink || record.esDownlink) esRecords.Add(record);
                    if (record.tsUsage) tsRecords.Add(record);
                }

                // Write out the TS records.
                Console.Write("\n\n============= All TS records ================\n");
                Console.Write("\n{0}", Record.Banner());
                foreach (Record record in tsRecords)
                {
                    Console.Write("\n{0}", record.ToString());
                }

                List<Record> tsAggregatedRecords;

                AggregateBands(tsRecords, out tsAggregatedRecords);

                // Write out the TS records.
                Console.Write("\n\n============= Aggregated TS records ================\n");
                Console.Write("\n{0}", Record.Banner());
                foreach (Record record in tsAggregatedRecords)
                {
                    Console.Write("\n{0}", record.ToString());
                }

                List<Record> esUpAggregatedRecords;

                AggregateBands(esUpRecords, out esUpAggregatedRecords);

                // Write out the ES-Up records.
                Console.Write("\n\n============= Aggregated ES Uplink records ================\n");
                Console.Write("\n{0}", Record.Banner());
                foreach (Record record in esUpAggregatedRecords)
                {
                    Console.Write("\n{0}", record.ToString());
                }

                List<Record> esDownAggregatedRecords;

                AggregateBands(esDownRecords, out esDownAggregatedRecords);

                // Write out the ES-Down records.
                Console.Write("\n\n============= Aggregated ES Downlink records ================\n");
                Console.Write("\n{0}", Record.Banner());
                foreach (Record record in esDownAggregatedRecords)
                {
                    Console.Write("\n{0}", record.ToString());
                }

                List<Record> esAggregatedRecords;

                AggregateBands(esRecords, out esAggregatedRecords);

                // Write out the ES records.
                Console.Write("\n\n============= Aggregated ES records ================\n");
                Console.Write("\n{0}", Record.Banner());
                foreach (Record record in esAggregatedRecords)
                {
                    Console.Write("\n{0}", record.ToString());
                }

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(0);
            }
            catch (Exception e)
            {
                Console.Error.Write("\n\nERROR: ParseCTFA.Go(): exception: {0}\n{1}", e.Message, e.StackTrace);
            }

            return;
        }

        public static void AggregateBands(List<Record> records, out List<Record> tsAggregatedRecords)
        {
            // 'out'.
            tsAggregatedRecords = new List<Record>();

            // Ensure that the records list is sorted in ascending order of freqLoKHz.
            records.Sort(Record.Comparer);

            Record tsAggRecord = records.First().DeepCopy();

            foreach (Record record in records)
            {
                if (record == records.First())
                {
                    continue;
                }

                if (tsAggRecord.freqHiKHz == record.freqLoKHz)
                {
                    // Continue to aggregate the band.
                    tsAggRecord.freqHiKHz = record.freqHiKHz;

                    tsAggRecord.esUplink = tsAggRecord.esUplink || record.esUplink;
                    tsAggRecord.esDownlink = tsAggRecord.esDownlink || record.esDownlink;

                    tsAggRecord.tsUsage = tsAggRecord.tsUsage || record.tsUsage;
                }
                else
                {
                    // There is a frequency discontinuity.

                    // Determine the usage for the current aggregate record.
                    tsAggRecord.DetermineUsage();

                    // Add the current aggregated record to the list.
                    tsAggregatedRecords.Add(tsAggRecord);

                    // Start a new aggregated record.
                    tsAggRecord = record.DeepCopy();
                }

                if (record == records.Last())
                {
                    // Add the last aggregated record to the list.
                    tsAggregatedRecords.Add(tsAggRecord);
                    continue;
                }
            }
        }

        /// <summary>
        /// This method parses the line string containing the ISED prescribed frequency range; it 
        /// contains both the upper and lower frequency limits but these are present in several different
        /// formats.
        /// </summary>
        /// <remarks>
        /// Formats encountered:
        /// 698 - 806
        /// 941 - 941.5
        /// 960 - 1 164
        /// 1 164 - 1 215
        /// 1 610.6 - 1 613.8
        /// 9 900 – 10 000
        /// 10.45 - 10.5
        /// 15.135 - 15.295
        /// 81 - 84
        /// </remarks>
        /// <param name="line"></param>
        /// <param name="units"></param>
        /// <param name="freqLoKHz"></param>
        /// <param name="freqHiKHz"></param>
        /// <returns></returns>
        public static int ParseFrequencyRange(string line, Units units, out double freqLoKHz, out double freqHiKHz)
        {
            // 'out'.
            freqLoKHz = Double.MinValue;
            freqHiKHz = Double.MaxValue;

            string freqLoStr = "";
            string freqHiStr = "";

            int retVal = Constant.FAILURE;

            if (String.IsNullOrWhiteSpace(line)) return retVal;

            // We assume that all frequency band strings contain a single '-' between the two numbers.
            char[] hyphen = new char[1] { '-' };

            string[] numberStrs = line.Split(hyphen);
            if (numberStrs.Length != 2)
            {
                Console.Error.Write("\n\nERROR: invalid frequency range string: |{0}|", line);
                return retVal;
            }

            freqLoStr = numberStrs[0].Trim();
            freqHiStr = numberStrs[1].Trim();

            // Some of the frequency range strings use a space to denote a ',' thousands seperator.
            // We need to remedy this.
            freqLoStr = freqLoStr.Replace(" ", "");
            freqHiStr = freqHiStr.Replace(" ", "");

            //Console.Write("\n@@@@@@@@@@@@@@@@@@@@@@@@ freqLoStr = |{0}|, freqHiStr = |{1}|", freqLoStr, freqHiStr);

            // Now attempt the conversions to double.
            try
            {
                freqLoKHz = Convert.ToDouble(freqLoStr);
                freqHiKHz = Convert.ToDouble(freqHiStr);

                if (units == Units.KHz)
                {
                    // Nothing needs to be done.
                }
                else if (units == Units.MHz)
                {
                    freqLoKHz *= 1000.0;
                    freqHiKHz *= 1000.0;
                }
                else if (units == Units.GHz)
                {
                    freqLoKHz *= 1000000.0;
                    freqHiKHz *= 1000000.0;
                }

                retVal = Constant.SUCCESS;
            }
            catch
            {
                Console.Error.Write("\n\nERROR: attempt to convert frequency range string to doubles failed for: |{0}|", line);
                return retVal;
            }

            //Console.Write("\n++++++++++++++ freqLoKHz = {0}, freqHiKHz = {1}", freqLoKHz, freqHiKHz);

            return retVal;
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

        public static bool IsOverlap(double aLower, double aUpper, double bLower, double bUpper)
        {
            bool result = false;

            // First, handle the pathological case of equality at both ends.
            if (aLower == bLower && aUpper == bUpper)
            {
                result = true;
            }
            else if (bLower >= aLower && bLower < aUpper)
            {
                result = true;
            }
            else if (bUpper > aLower && bUpper <= aUpper)
            {
                result = true;
            }

            return result;
        }


    }
}
