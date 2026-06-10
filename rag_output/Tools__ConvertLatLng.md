# Documented File: ConvertLatLng.cs
**Repository Path:** `Tools\ConvertLatLng.cs`
**Primary Layer:** `Tools`
**Namespace:** `Tools`

## Source Code Representation
```csharp
﻿using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class ConvertLatLng
    {
        private static string mLatitudeArg = null;
        private static string mLongitudeArg = null;

        public static void Go(string[] args)
        {
#if true
                string mLog2FilePath = @"d:\MicsBatchLogs\ConvertLatLng.log";
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
            string latitudeStr = "";
            string lngitudeStr = "";
            int latitideCentiseconds = int.MinValue;
            int lngitideCentiseconds = int.MinValue;
            double latitideDegrees = double.MinValue;
            double lngitideDegrees = double.MinValue;

            ParseCommandLineArgs(args);

            bool isStringFormat = Strings.LastCharIs(mLatitudeArg, 'N') || Strings.LastCharIs(mLatitudeArg, 'S');
            bool isCentisecondFormat = Strings.IsInteger(mLatitudeArg) && Strings.IsInteger(mLongitudeArg);
            bool isDegreesFormat = Strings.IsNumeric(mLatitudeArg) && Strings.IsNumeric(mLongitudeArg);

            if (isStringFormat)
            {
                latitudeStr = mLatitudeArg;
                lngitudeStr = mLongitudeArg;

                LatLng.ConvertToCentiseconds(latitudeStr, lngitudeStr, out latitideCentiseconds, out lngitideCentiseconds);

                LatLng.ConvertToDegrees(latitideCentiseconds, lngitideCentiseconds, out latitideDegrees, out lngitideDegrees);
            }
            else if (isCentisecondFormat)
            {
                latitideCentiseconds = Convert.ToInt32(mLatitudeArg);
                lngitideCentiseconds = Convert.ToInt32(mLongitudeArg);

                LatLng.ConvertToDegrees(latitideCentiseconds, lngitideCentiseconds, out latitideDegrees, out lngitideDegrees);

                LatLng.ConvertToString(latitideCentiseconds, lngitideCentiseconds, out latitudeStr, out lngitudeStr);
            }
            else if (isDegreesFormat)
            {
                latitideDegrees = Convert.ToDouble(mLatitudeArg);
                lngitideDegrees = Convert.ToDouble(mLongitudeArg);

                LatLng.ConvertToCentiseconds(latitideDegrees, lngitideDegrees, out latitideCentiseconds, out lngitideCentiseconds);

                LatLng.ConvertToString(latitideDegrees, lngitideDegrees, out latitudeStr, out lngitudeStr);
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(String.Format("\n\n  Lat, Long  :  {0}, {1}", latitudeStr, lngitudeStr));
            sb.Append(String.Format("\n\n  Lat, Long  :  {0}, {1}", latitideCentiseconds, lngitideCentiseconds));
            sb.Append(String.Format("\n\n  Lat, Long  :  {0:F6}, {1}", latitideDegrees, lngitideDegrees.ToString("000.000000")));

            Console.Write(sb.ToString());
        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program reads/writes a value from/to a registry key under HKEY_LOCAL_MACHINE.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: ConvertLatLng  <latitude>  <longitude>");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        latitude    : formatted as  49-26-24.72N, centiseconds (int) or decimal degrees.");
            Console.Write("\r\n        longitude   : formatted as 117-21-26.93W, centiseconds (int) or decimal degrees.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n       ConvertLatLng  49-26-24.72N  117-21-26.93W");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the user-prescribed command-line arguments.
        /// </summary>
        /// <param name="args"></param>
        public static void ParseCommandLineArgs(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Write("\r\nERROR: Invalid command line args");
                WriteUsageToConsole();
                Application.ExitQuietly(1);
            }

            mLatitudeArg = args[0].ToUpper();
            mLongitudeArg = args[1].ToUpper();
        }





    }
}

```
