# Documented File: ScrapeCTFA.cs
**Repository Path:** `Tools\ScrapeCTFA.cs`
**Primary Layer:** `Tools`
**Namespace:** `Tools`

## Source Code Representation
```csharp
﻿using _DataStructures;
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

    public class ScrapeCTFA
    {
        private const string INPUT_FILE_PATH = @"D:\users\ahulme\CTFA 2022.txt";
        private const string PATTERN_PRIMARY_SERVICE = @"primary service([a-zA-Z- ]+)end primary service";
        private const string TABLE_NAME = "hulme.CTFA2022";

        private class Record
        {
            private double mLowerFreqKHz;
            private double mUpperFreqKHz;
            private List<string> mServiceTypes;

            public double LowerFreqKHz { get { return mLowerFreqKHz; } set { mLowerFreqKHz = value; } }
            public double UpperFreqKHz { get { return mUpperFreqKHz; } set { mUpperFreqKHz = value; } }
            public List<string> ServiceTypes { get { return mServiceTypes; } set { mServiceTypes = value; } }

            public Record()
            {
                mLowerFreqKHz = 0.0;
                mUpperFreqKHz = 0.0;
                mServiceTypes = new List<string>();
            }
        }

        public static void Go(string[] args)
        {
            string lowerFreqStr;
            string upperFreqStr;

            try
            {
                int multiplier = 1;   // Use KHz as the unit of frequency.

                string[] lines = File.ReadAllLines(INPUT_FILE_PATH);

                Record record = null;
                List<Record> records = new List<Record>();

                foreach (string line in lines)
                {
                    string lineUC = line.ToUpper();

                    if (lineUC.Contains("CANADIAN TABLE OF FREQUENCY ALLOCATIONS"))
                    {
                        // Skip this header.
                    }
                    else if (lineUC.StartsWith("KHZ"))
                    {
                        //Console.Write("\n{0}", line);
                    }
                    else if (lineUC.StartsWith("MHZ"))
                    {
                        multiplier = 1000;
                        //Console.Write("\n{0}", line);
                    }
                    else if (lineUC.StartsWith("GHZ"))
                    {
                        multiplier = 1000000;
                        //Console.Write("\n{0}", line);
                    }
                    else if (lineUC.Contains("\t"))
                    {
                        if (record != null) records.Add(record);

                        record = new Record();

                        int indexOfTab = line.IndexOf("\t");

                        string[] numberFields = line.Substring(0, indexOfTab).Split('-');

                        lowerFreqStr = numberFields[0].Trim();
                        upperFreqStr = numberFields[1].Trim();

                        lowerFreqStr = lowerFreqStr.Replace(" ", "");
                        upperFreqStr = upperFreqStr.Replace(" ", "");

                        //Console.Write("\n|{0}| |{1}|", lowerFreqStr, upperFreqStr);

                        double lowerFreqKHz = multiplier * Convert.ToDouble(lowerFreqStr);
                        double upperFreqKHz = multiplier * Convert.ToDouble(upperFreqStr);

                        string secondColumnStr = line.Substring(indexOfTab + 1, line.Length - indexOfTab- 1);

                        MatchCollection matchColl = Regex.Matches(secondColumnStr, PATTERN_PRIMARY_SERVICE);

                        string payload = "";
                        foreach (Match match in matchColl)
                        {
                            payload = match.Groups[1].Value;
                        }

                        if (lineUC.Contains("EARTH-TO-SPACE")) payload += " (Earth-to-space)";
                        if (lineUC.Contains("SPACE-TO-EARTH")) payload += " (space-to-Earth)";

                        Console.Write(";    {0} - {1}         {2}", lowerFreqKHz, upperFreqKHz, payload);

                        record.LowerFreqKHz = lowerFreqKHz;
                        record.UpperFreqKHz = upperFreqKHz;
                        record.ServiceTypes.Add(payload);
                    }
                    else
                    {
                        MatchCollection matchColl = Regex.Matches(line, PATTERN_PRIMARY_SERVICE);

                        string payload = "";
                        foreach (Match match in matchColl)
                        {
                            payload = match.Groups[1].Value;
                        }

                        if (lineUC.Contains("EARTH-TO-SPACE")) payload += " (Earth-to-space)";
                        if (lineUC.Contains("SPACE-TO-EARTH")) payload += " (space-to-Earth)";

                        Console.Write("\n      {0}", payload);

                        if (!payload.Equals("")) record.ServiceTypes.Add(payload);
                    }
                }

                Console.Write("\n\n=============================================================================");

                int index = 0;
                foreach (Record rec in records)
                {
                    Console.Write("\n{0},  {1} - {2} ", index++, rec.LowerFreqKHz, rec.UpperFreqKHz);

                    foreach(string serviceType in rec.ServiceTypes)
                    {
                        Console.Write("  |{0}|", serviceType);
                    }
                }

                Info.DbName = "micsdev";
                Info.MicsUserName = "hulme1";
                Info.Password = "bananarama";

                Ssutil.UtConnect(Info.DbName, 1);

                SQLLEN[] nullInds = NullHelper.CreateArrayOfNullInd(CTFA.NUM_COLUMNS, NullHelper.ColumnStatus.NOT_NULL);
                foreach (Record rekord in records)
                {
                    CTFA ctfa = new CTFA();
                    ctfa.LowerFreqKHz = rekord.LowerFreqKHz;
                    ctfa.UpperFreqKHz = rekord.UpperFreqKHz;

                    ctfa.Fixed = Strings.IsInListCaseInsensitive(rekord.ServiceTypes, "FIXED") ? "Y" : "N";
                    ctfa.EarthToSatellite = Strings.IsInListCaseInsensitive(rekord.ServiceTypes, "FIXED-SATELLITE (Earth-to-space)") ? "Y" : "N";
                    ctfa.SatelliteToEarth = Strings.IsInListCaseInsensitive(rekord.ServiceTypes, "FIXED-SATELLITE (space-to-Earth)") ? "Y" : "N";

                    ctfa.SrspID = "Hello";

                    int handle = DynCTFA.InsertCTFA(TABLE_NAME, ctfa, nullInds);
                }

                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(0);

            }
            catch (Exception e)
            {
                Console.Write("\n\nERROR: Exception: {0}\n{1}", e.Message, e.StackTrace);
            }
        }







    }
}

```
