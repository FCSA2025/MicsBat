# Documented File: TestTpRunTsip.cs
**Repository Path:** `Tools\TestTpRunTsip.cs`
**Primary Layer:** `Tools`
**Namespace:** `Tools`

## Source Code Representation
```csharp
using _NewLib;
using System;
using System.Collections.Generic;
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
    public class TestTpRunTsip
    {
        private static bool isFirstCall = true;

        public static string[] reportTypeExtensions = new string[11]
        {
            "ERR", "AGGINT.csv", "AGGINTREP", "CASEDET", "CASEOHL", "CASESUM", "EXEC", "HILO", "ORBIT", "STATSUM", "STUDY"
        };

        public const string DATE_PATTERN = @"\d\d\d\d\.\d\d\.\d\d";  // 2018.01.10
        public const string DATE_SUBSTITUTE = "9999.99.99";

        public const string TIME_PATTERN = @"\d\d:\d\d";  // 11:23
        public const string TIME_SUBSTITUTE = "99:99";

        public const string DATE_TIME_PATTERN = DATE_PATTERN + ", " + TIME_PATTERN;  // At 2018.01.10, 12:05
        public const string DATE_TIME_SUBSTITUTE = "9999.99.99, 99:99";

        public const string DATE_TIME_1_PATTERN = DATE_PATTERN + " " + TIME_PATTERN;  // At 2018.01.10, 12:05
        public const string DATE_TIME_1_SUBSTITUTE = "9999.99.99 99:99";

        public const string CPU_TIME_PATTERN = @"\d\d:\d\d:\d\d\.\d{1,3}";  // 00:00:02.934
        public const string CPU_TIME_SUBSTITUTE = "99:99:99.999";

        public const string ELAPSED_TIME_PATTERN = @"\d\d:\d\d:\d\d";  // 00:00:10
        public const string ELAPSED_TIME_SUBSTITUTE = "99:99:99";

        public const string BUILD_DATE_TIME_PATTERN = @"\d{6}-\d{4}/\d\d-[DR]"; // 160729-0925/64-R
        public const string BUILD_DATE_TIME_SUBSTITUTE = "999999-9999/99-Z";

        public const string CACHE_TYPE_PATTERN = @"(Antennas)|(Equipment)|(CTX Curve)|(CTX Xref)|(Analog)|(Digital)";

        // Project Code[hulme1_0] Process ID 6712
        public const string PROJ_CODE_PROC_ID_PATTERN = @"Project Code \[\w+\] Process ID \d+";
        public const string PROJ_CODE_PROC_ID_SUBSTITUTE = "Project Code[#####] Process ID 9999";
        public const string MINUS_SIGN_ZERO_PATTERN = @" -0\.(0{1,}) ";
        public const string MINUS_SIGN_ZERO_SUBSTITUTE = @"  0.$1 ";

        public const string REPORT_PATH_PATTERN_A = @"(\w|\\)+\.(ERR|CSV|AGGINTREP|CASEDET|CASEOHL|CASESUM|EXEC|HILO|ORBIT|STATSUM|STUDY)";
        public const string REPORT_PATH_PATTERN_B = @"(\w|\\)+\.(ERR|AGGINT\.csv|AGGINTREP|CASEDET|CASEOHL|CASESUM|EXEC|HILO|ORBIT|STATSUM|STUDY)";
        public const string REPORT_PATH_SUBSTITUTE = @"REPORT.TYPE";

        public const string SIX_DECIMAL_PLACES_PATTERN = @"(-{0,1})(\d{1,})\.(\d{6,})";
        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="args"></param>
        public static void Test_TpRunTsip(string[] args)
        {
            if (args.Length != 3)
            {
                WriteUsageMessage();
                Environment.Exit(1);
            }

            string dirA = args[0];
            string dirB = args[1];
            string baseFileName = null;

            if (!Directory.Exists(dirA))
            {
                Console.WriteLine("\n\nDirectory 'A' does not exist:  " + dirA + "\n");
                Environment.Exit(2);
            }

            if (!Directory.Exists(dirB))
            {
                Console.WriteLine("\n\nDirectory 'B' does not exist:  " + dirB + "\n");
                Environment.Exit(3);
            }

            string reportPathA = null;
            string reportPathB = null;

            foreach (string typeExtension in reportTypeExtensions)
            {
                if (typeExtension.Equals("ERR"))
                {
                    // There is only one ERR report covering multiple TSIP runs.
                    // Consequently, we need to remove the run 'tag' from the basefileName.
                    string[] elements = args[2].Split('_');
                    baseFileName = elements[0];
                    for (int i = 1; i < elements.Length - 1; i++)
                    {
                        baseFileName += "_" + elements[i];
                    }
                }
                else
                {
                    baseFileName = args[2];

                }

                reportPathA = Path.Combine(dirA, baseFileName + "." + typeExtension);
                reportPathB = Path.Combine(dirB, baseFileName + "." + typeExtension);

                LineDifference lineDiff = FindFirstDifferenceInFiles(reportPathA, reportPathB);

                Console.Write("\n" + typeExtension.PadRight(12));

                switch (lineDiff.outcome)
                {
                    case LineDifference.Outcome.A_NOTFOUND:
                        Console.Write(" :         No matching file in directory A");
                        break;
                    case LineDifference.Outcome.B_NOTFOUND:
                        Console.Write(" :         No matching file in directory B");
                        break;
                    case LineDifference.Outcome.AB_NOTFOUND:
                        Console.Write(" :         N/A");
                        break;
                    case LineDifference.Outcome.IDENTICAL:
                        //result = ": Files are IDENTICAL except for run dates, times etc.";
                        //Console.Write(result);
                        //Console.Write("\n==========");
                        //Console.Write("\n    Report A: " + reportPathA);
                        //Console.Write("\n    Report B: " + reportPathB);
                        Console.Write(" : PASS");
                        break;
                    case LineDifference.Outcome.DIFFERENT:
                        //Console.Write(": Files are DIFFERENT at line " + lineDiff.lineNumber + ", column " + lineDiff.colNumber + ".");
                        Console.Write(" : FAIL");
                        //Console.Write("\n==========");
                        Console.Write("\n    A: line " + lineDiff.lineNumber.ToString("D4") + " : " + lineDiff.lineA);
                        Console.Write("\n    B: line " + lineDiff.lineNumber.ToString("D4") + " : " + lineDiff.lineB);
                        Console.Write("\n                   " + lineDiff.indicator);
                        Console.Write("\n                   " + lineDiff.indicator);
                        break;
                    default:
                        break;
                } // switch

            } // foreach

            Console.Write("\n\n");

        } // Main()

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="pathA"></param>
        /// <param name="pathB"></param>
        /// <returns></returns>
        public static LineDifference FindFirstDifferenceInFiles(string pathA, string pathB)
        {
            LineDifference result = new LineDifference();

            string[] allLinesA = null;
            string[] allLinesB = null;

            bool A_Found = false;
            bool B_Found = false;

            // Try file in directory A.
            try
            {
                allLinesA = File.ReadAllLines(pathA);

                A_Found = true;

                SanitizeForTsipReportType(ref allLinesA, pathA);
            }
            catch (Exception e)
            {
                result.message = "ERROR: " + e.Message;
            }

            // Try file in directory B.
            try
            {
                allLinesB = File.ReadAllLines(pathB);

                B_Found = true;

                SanitizeForTsipReportType(ref allLinesB, pathB);
            }
            catch (Exception e)
            {
                result.message = "ERROR: " + e.Message;
            }

            if (A_Found && B_Found)
            {
                result = FindFirstDifferenceInLines(allLinesA, allLinesB);
            }
            else if (!A_Found && B_Found)
            {
                result.outcome = LineDifference.Outcome.A_NOTFOUND;
            }
            else if (A_Found && !B_Found)
            {
                result.outcome = LineDifference.Outcome.B_NOTFOUND;
            }
            else
            {
                result.outcome = LineDifference.Outcome.AB_NOTFOUND;
            }


            return result;
        }
        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="allLinesA"></param>
        /// <param name="allLinesB"></param>
        /// <returns></returns>
        public static LineDifference FindFirstDifferenceInLines(string[] allLinesA, string[] allLinesB)
        {
            LineDifference result = new LineDifference();
            result.outcome = LineDifference.Outcome.IDENTICAL;

            int nLinesMin = Math.Min(allLinesA.Length, allLinesB.Length);
            int nLinesMax = Math.Max(allLinesA.Length, allLinesB.Length);

            for (int i = 0; i < nLinesMax; i++)
            {
                if (i >= nLinesMin)
                {
                    result.lineNumber = i + 1;
                    result.colNumber = 1;
                    if (i < allLinesA.Length)
                    {
                        result.outcome = LineDifference.Outcome.DIFFERENT;
                        result.lineA = allLinesA[i];
                    }
                    else
                    {
                        result.outcome = LineDifference.Outcome.DIFFERENT;
                        result.lineA = "<EOF>";
                    }

                    if (i < allLinesB.Length)
                    {
                        result.outcome = LineDifference.Outcome.DIFFERENT;
                        result.lineB = allLinesB[i];
                    }
                    else
                    {
                        result.outcome = LineDifference.Outcome.DIFFERENT;
                        result.lineB = "<EOF>";
                    }


                    break;
                }

                if (!allLinesA[i].Equals(allLinesB[i]))
                {
                    result.outcome = LineDifference.Outcome.DIFFERENT;

                    result.lineNumber = i + 1;
                    result.lineA = allLinesA[i];
                    result.lineB = allLinesB[i];

                    result.colNumber = LineDifference.FindColumnNumber(allLinesA[i], allLinesB[i]);
                    result.indicator = LineDifference.MakeIndicator(result.colNumber);
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="allLines"></param>
        /// <param name="path"></param>
        public static void SanitizeForTsipReportType(ref string[] allLines, string path)
        {
            string extension = Path.GetExtension(path);

            PurgeFormFeeds(ref allLines);

            switch (extension)
            {
                case ".ERR":
                    SanitizeForERR(ref allLines);
                    break;

                case ".csv":
                    SanitizeForAGGINT_CSV(ref allLines);
                    break;

                case ".AGGINTREP":
                    SanitizeForAGGINTREP(ref allLines);
                    break;

                case ".CASEDET":
                    SanitizeForCASEDET(ref allLines);
                    break;

                case ".CASEOHL":
                    SanitizeForCASEOHL(ref allLines);
                    break;

                case ".CASESUM":
                    SanitizeForCASESUM(ref allLines);
                    break;

                case ".EXEC":
                    SanitizeForEXEC(ref allLines);
                    break;

                case ".HILO":
                    SanitizeForHILO(ref allLines);
                    break;

                case ".STATSUM":
                    SanitizeForSTATSUM(ref allLines);
                    break;

                case ".STUDY":
                    SanitizeForSTUDY(ref allLines);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="allLines"></param>
        public static void SanitizeForERR(ref string[] allLines)
        {
            for (int i = 0; i < allLines.Length; i++)
            {

                if (Regex.IsMatch(allLines[i], @"^TSIP build"))
                {
                    allLines[i] = Regex.Replace(allLines[i], BUILD_DATE_TIME_PATTERN, BUILD_DATE_TIME_SUBSTITUTE);
                    allLines[i] = Regex.Replace(allLines[i], DATE_TIME_1_PATTERN, DATE_TIME_1_SUBSTITUTE);
                }

                if (Regex.IsMatch(allLines[i], PROJ_CODE_PROC_ID_PATTERN))
                {
                    allLines[i] = Regex.Replace(allLines[i], PROJ_CODE_PROC_ID_PATTERN, PROJ_CODE_PROC_ID_SUBSTITUTE);
                }

            }
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="allLines"></param>
        public static void PurgeFormFeeds(ref string[] allLines)
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < allLines.Length; i++)
            {
                if (allLines[i].Equals("\f"))
                {
                    // skip;
                }
                else
                {
                    lines.Add(allLines[i]);
                }
            }

            allLines = lines.ToArray();
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="allLines"></param>
        public static void SanitizeForCASEDET(ref string[] allLines)
        {
            // It is difficult to match the very first line of CASEDET reports
            // created with the C/C++ code and the new C# version. The first
            // line of the C/C++ created CASEDET may, or may not, be a form 
            // feed character depending on the nature of the input to TpRunTsip.
            // Consequently, skip all lines until we find the first line that
            // is not all whitespace.
            List<string> tempList = new List<string>();
            bool contentFound = false;

            for (int i = 0; i < allLines.Length; i++)
            {
                if (!contentFound)
                {
                    if (!String.IsNullOrWhiteSpace(allLines[i]))
                    {
                        contentFound = true;
                    }
                }

                if (contentFound)
                {
                    tempList.Add(allLines[i]);
                }
            }
            allLines = tempList.ToArray();


            for (int i = 0; i < allLines.Length; i++)
            {

                if (allLines[i].StartsWith("MASTER DATA BASE -- TSTS Interference Study Report - CASE DETAIL"))
                {
                    allLines[i] = Regex.Replace(allLines[i], DATE_PATTERN, DATE_SUBSTITUTE);
                }

                if (allLines[i].StartsWith("MASTER DATA BASE -- TS to ES Interference Study Report - Case Detail"))
                {
                    allLines[i] = Regex.Replace(allLines[i], DATE_PATTERN, DATE_SUBSTITUTE);
                }

                if (allLines[i].StartsWith("MASTER DATA BASE -- ES to TS Interference Study Report - Case Detail"))
                {
                    allLines[i] = Regex.Replace(allLines[i], DATE_PATTERN, DATE_SUBSTITUTE);
                }

                if (Regex.IsMatch(allLines[i], "^ {114}Time"))
                {
                    allLines[i] = Regex.Replace(allLines[i], TIME_PATTERN, TIME_SUBSTITUTE);
                }

                if (Regex.IsMatch(allLines[i], "^All Cases {107}Time"))
                {
                    allLines[i] = Regex.Replace(allLines[i], TIME_PATTERN, TIME_SUBSTITUTE);
                }

                // Sanitize the instances of -0.0
                if (Regex.IsMatch(allLines[i], MINUS_SIGN_ZERO_PATTERN))
                {
                    //Console.Write("\n\nallLine[" + i + "] = " + allLines[i]);
                    allLines[i] = Regex.Replace(allLines[i], MINUS_SIGN_ZERO_PATTERN, MINUS_SIGN_ZERO_SUBSTITUTE);
                    //Console.Write("\nallLine[" + i + "] = " + allLines[i]);
                }


            }
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="allLines"></param>
        public static void SanitizeForEXEC(ref string[] allLines)
        {
            for (int i = 0; i < allLines.Length; i++)
            {

                if (Regex.IsMatch(allLines[i], @"^ {20}FREQUENCY COORDINATION SYSTEM ASSOCIATION"))
                {
                    allLines[i] = Regex.Replace(allLines[i], BUILD_DATE_TIME_PATTERN, BUILD_DATE_TIME_SUBSTITUTE);
                }

                if (Regex.IsMatch(allLines[i], @"^    Date: "))
                {
                    allLines[i] = Regex.Replace(allLines[i], DATE_PATTERN, DATE_SUBSTITUTE);
                }

                if (Regex.IsMatch(allLines[i], @"^    Time: "))
                {
                    allLines[i] = Regex.Replace(allLines[i], TIME_PATTERN, TIME_SUBSTITUTE);
                }

                if (Regex.IsMatch(allLines[i], CACHE_TYPE_PATTERN))
                {
                    allLines[i] = "CACHE STATISTICS";
                }

                if (Regex.IsMatch(allLines[i], @"^.* (Report|CSV)\)$"))
                {
                    allLines[i] = Regex.Replace(allLines[i], REPORT_PATH_PATTERN_A, REPORT_PATH_SUBSTITUTE);
                }

            }

        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="allLines"></param>
        public static void SanitizeForAGGINT_CSV(ref string[] allLines)
        {
            ScanFormatted sf = new ScanFormatted();
            string[] atoms = new string[9];

            for (int i = 0; i < allLines.Length; i++)
            {
                // e.g.  CIN228,VAD472,11,22265000.000000,-28.303823,I,-39.777141,-106.708233,-66.931092
                if (Regex.IsMatch(allLines[i], SIX_DECIMAL_PLACES_PATTERN))
                {
                    allLines[i] = allLines[i].Replace(",", " ");
                    int n = sf.Parse(allLines[i], "%s%s%s%s%f%s%f%f%f");
                    List<object> results = sf.Results;

                    // Truncate the floating point values down to 4 decimal places.
                    atoms[0] = (string)results[0];
                    atoms[1] = (string)results[1];
                    atoms[2] = (string)results[2];
                    atoms[3] = (string)results[3];
                    atoms[4] = String.Format("{0:F4}", Convert.ToDouble(results[4]));
                    atoms[5] = (string)results[5];
                    atoms[6] = String.Format("{0:F4}", Convert.ToDouble(results[6]));
                    atoms[7] = String.Format("{0:F4}", Convert.ToDouble(results[7]));
                    atoms[8] = String.Format("{0:F4}", Convert.ToDouble(results[8]));

                    StringBuilder sb = new StringBuilder();

                    sb.Append(atoms[0]); sb.Append(",");
                    sb.Append(atoms[1]); sb.Append(",");
                    sb.Append(atoms[2]); sb.Append(",");
                    sb.Append(atoms[3]); sb.Append(",");
                    sb.Append(atoms[4]); sb.Append(",");
                    sb.Append(atoms[5]); sb.Append(",");
                    sb.Append(atoms[6]); sb.Append(",");
                    sb.Append(atoms[7]); sb.Append(",");
                    sb.Append(atoms[8]); sb.Append(",");

                    allLines[i] = sb.ToString();
                    //Console.Write("\n" + allLines[i]);
                }

            }
        }
        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="allLines"></param>
        public static void SanitizeForAGGINTREP(ref string[] allLines)
        {
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="allLines"></param>
        public static void SanitizeForCASESUM(ref string[] allLines)
        {
            for (int i = 0; i < allLines.Length; i++)
            {

                if (Regex.IsMatch(allLines[i], @"MASTER DATA BASE -- TSTS Interference Study Report - Case Summary"))
                {
                    allLines[i] = Regex.Replace(allLines[i], DATE_PATTERN, DATE_SUBSTITUTE);
                }

                // Sanitize the instances of -0.0
                if (Regex.IsMatch(allLines[i], MINUS_SIGN_ZERO_PATTERN))
                {
                    //Console.Write("\n\nallLine[" + i + "] = " + allLines[i]);
                    allLines[i] = Regex.Replace(allLines[i], MINUS_SIGN_ZERO_PATTERN, MINUS_SIGN_ZERO_SUBSTITUTE);
                    //Console.Write("\nallLine[" + i + "] = " + allLines[i]);
                }

            }
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="allLines"></param>
        public static void SanitizeForHILO(ref string[] allLines)
        {
            for (int i = 0; i < allLines.Length; i++)
            {

                if (Regex.IsMatch(allLines[i], "At " + DATE_TIME_PATTERN))
                {
                    allLines[i] = Regex.Replace(allLines[i], DATE_TIME_PATTERN, DATE_TIME_SUBSTITUTE);
                }

            }
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="allLines"></param>
        public static void SanitizeForCASEOHL(ref string[] allLines)
        {
            for (int i = 0; i < allLines.Length; i++)
            {

                if (Regex.IsMatch(allLines[i], "MASTER DATA BASE -- TSTS Interference Study Report - CASE DETAIL"))
                {
                    allLines[i] = Regex.Replace(allLines[i], DATE_PATTERN, DATE_SUBSTITUTE);
                }

                if (Regex.IsMatch(allLines[i], "Line of sight, and over-horizon loss cases only"))
                {
                    allLines[i] = Regex.Replace(allLines[i], TIME_PATTERN, TIME_SUBSTITUTE);
                }

            }
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="allLines"></param>
        public static void SanitizeForSTATSUM(ref string[] allLines)
        {
            for (int i = 0; i < allLines.Length; i++)
            {

                if (Regex.IsMatch(allLines[i], @"FREQUENCY COORDINATION SYSTEM ASSOCIATION {23,24}DATE:"))
                {
                    allLines[i] = Regex.Replace(allLines[i], DATE_PATTERN, DATE_SUBSTITUTE);
                }

            }
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="allLines"></param>
        public static void SanitizeForSTUDY(ref string[] allLines)
        {
            for (int i = 0; i < allLines.Length; i++)
            {

                if (Regex.IsMatch(allLines[i], @"MASTER DATA BASE.*Interference Study Summary"))
                {
                    allLines[i] = Regex.Replace(allLines[i], TIME_PATTERN, TIME_SUBSTITUTE);
                }

                if (Regex.IsMatch(allLines[i], @"Study Date"))
                {
                    allLines[i] = Regex.Replace(allLines[i], DATE_PATTERN, DATE_SUBSTITUTE);
                }

                if (Regex.IsMatch(allLines[i], @"Study Time"))
                {
                    allLines[i] = Regex.Replace(allLines[i], TIME_PATTERN, TIME_SUBSTITUTE);
                }

                if (Regex.IsMatch(allLines[i], @"CPU Time"))
                {
                    allLines[i] = Regex.Replace(allLines[i], CPU_TIME_PATTERN, CPU_TIME_SUBSTITUTE);
                }

                if (Regex.IsMatch(allLines[i], @"Elapsed Time"))
                {
                    allLines[i] = Regex.Replace(allLines[i], ELAPSED_TIME_PATTERN, ELAPSED_TIME_SUBSTITUTE);
                }

                if (Regex.IsMatch(allLines[i], @"Aggregate Interference CSV report is in file:"))
                {
                    allLines[i] = Regex.Replace(allLines[i], REPORT_PATH_PATTERN_B, REPORT_PATH_SUBSTITUTE);
                }

            }

            WriteAllLinesABToFile(@"d:\MicsBatchLogs\armadilo.txt", allLines);
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name=""></param>
        public static void WriteUsageMessage()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n");
            sb.Append("\n    Usage:  testing  TpRunTsip  dirA  dirB  rootName");
            sb.Append("\n");
            sb.Append("\n    dirA:       path to directory containing 1st set of Tsip output reports.");
            sb.Append("\n    dirB:       path to directory containing 2nd set of Tsip output reports.");
            sb.Append("\n    rootName:   filename of report without extension, e.g. tsip_tstest0183");
            sb.Append("\n");
            sb.Append("\n    e.g.  > testing TpRunTsip  D:\\Users\\ahulme\\baseline  D:\\Users\\ahulme\\new  tsip_tstest0183");
            sb.Append("\n");

            Console.WriteLine(sb.ToString());
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="path"></param>
        /// <param name="allLines"></param>
        public static void WriteAllLinesABToFile(string path, string[] allLines)
        {
            if (isFirstCall)
            {
                File.WriteAllLines(path, allLines);
                isFirstCall = false;
            }
            else
            {
                File.AppendAllLines(path, allLines);
            }
        }


    }
}

```
