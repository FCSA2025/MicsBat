# Documented File: KML.cs
**Repository Path:** `_NewLib\KML.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// This class provides static methods that are useful for reading/parsing from existing
    /// KML formatted geospatial data files or creating/writing new KML data files; 
    /// Keyhole Markup Language (KML) is a specific XML notation for expressing geographic annotation 
    /// and visualization within two-dimensional maps and three-dimensional Earth browsers; 
    /// KML is supported, and actively promoted, by Google and KML files can easily be imported
    /// into Google Earth and Google Maps where the geospatial data can be visualized.
    /// </summary>
    public class KML
    {
        public const string PATH_TO_CSV_FILE = @"D:\MicsBatchLogs\Comsearch50kmSpecial.csv";

        public static string beginning = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<kml xmlns = \"http://www.google.com/earth/kml/2\">\n<Document>\n<name>kml_sample1.kml</name>";

        public static string ending = "</Document>\n</kml>";

        private static Dictionary<string, SiteRecord> latLngDict = new Dictionary<string, SiteRecord>();

        public const int NUM_DEC_PLACES = 6;

        public const double MAX_DIST_TO_BORDER = 15.0;

        public class SiteRecord
        {
            public int myID = 0;
            public string pathID = "";
            public int end = 0;
            public double lat = double.MaxValue;
            public double lng = double.MaxValue;
            public double distToBorder = double.MaxValue;
        }


        /// <summary>
        /// This method reads callsign, latitude and longitude records from a prescribed CSV formatted
        /// text file and return 
        /// </summary>
        /// <param name="pathToCSVfile"></param>
        public static string CallsignLatLngList(string pathToCSVfile)
        {
            string placemarksXML = "";
            try
            {
                double lat;
                double lng;

                string call1 = "";

                string middle = "";

                string[] lines = File.ReadAllLines(pathToCSVfile);

                foreach (string line in lines)
                {
                    string[] fields = line.Split(',');

                    call1 = fields[0].Trim();
                    lat = Convert.ToDouble(fields[1]);
                    lng = Convert.ToDouble(fields[2]);

                    middle += PlacemarkString(call1, "TS", lat, lng);
                }

                //Console.Write("{0}\n{1}\n{2}", beginning, middle, ending);
                placemarksXML = String.Format("{0}\n{1}\n{2}", beginning, middle, ending);

            }
            catch (Exception e)
            {
                Console.Error.Write("\nERROR: exception: {0}\n{1}", e.Message, e.StackTrace);
            }

            return placemarksXML;
        }

        /// <summary>
        /// The following method provides a worked example of using the two primary methods
        /// PlaceMarkString() and LineString(). The input is assumed to be a CSV file with
        /// multiple lines comprising several fields.
        /// </summary>
        /// <param name="args"></param>
        public static void Example(string[] args)
        {
            try
            {
                string desc = "";
                double lat1;
                double lng1;
                double distToBorder1;
                double lat2;
                double lng2;
                double distToBorder2;
                int count = 1;
                string pathID = "";

                string middle = "";

                string[] lines = File.ReadAllLines(PATH_TO_CSV_FILE);

                foreach (string line in lines)
                {
                    string[] fields = line.Split(',');

                    // Process site 1.
                    pathID = fields[0].Trim();
                    lat1 = Convert.ToDouble(fields[1]);
                    lng1 = Convert.ToDouble(fields[2]);
                    distToBorder1 = Convert.ToDouble(fields[3]);
                    lat2 = Convert.ToDouble(fields[4]);
                    lng2 = Convert.ToDouble(fields[5]);
                    distToBorder2 = Convert.ToDouble(fields[6]);

                    // Draw a line between the two points.
                    if ((distToBorder1 <= MAX_DIST_TO_BORDER) || (distToBorder2 <= MAX_DIST_TO_BORDER))
                    {
                        middle += LineString(lat1, lng1, lat2, lng2);
                    }

                    string key = LatLngToProxyString(lat1, lng1, NUM_DEC_PLACES);

                    if (latLngDict.ContainsKey(key))
                    {
                        // Do nothing.
                    }
                    else
                    {
                        SiteRecord sr = new SiteRecord();
                        sr.myID = count++;
                        sr.pathID = pathID;
                        sr.end = 1;
                        sr.lat = lat1;
                        sr.lng = lng1;
                        sr.distToBorder = distToBorder1;

                        latLngDict.Add(key, sr);
                    }

                    // Process site 2.
                    key = LatLngToProxyString(lat2, lng2, NUM_DEC_PLACES);

                    if (latLngDict.ContainsKey(key))
                    {
                        // Do nothing.
                    }
                    else
                    {
                        SiteRecord sr = new SiteRecord();
                        sr.myID = count++;
                        sr.pathID = pathID;
                        sr.end = 2;
                        sr.lat = lat2;
                        sr.lng = lng2;
                        sr.distToBorder = distToBorder2;

                        latLngDict.Add(key, sr);
                    }
                }

                foreach (KeyValuePair<string, SiteRecord> kvp in latLngDict)
                {
                    if (kvp.Value.distToBorder <= 15.0)
                    {
                        string name = String.Format("#{0}", kvp.Value.myID);

                        desc = String.Format("PathID {0}-{1}.\nLat/Lng = {2}, {3}\nDistance from border = {4:F1} km.", kvp.Value.pathID, kvp.Value.end, kvp.Value.lat, kvp.Value.lng, kvp.Value.distToBorder);

                        middle += PlacemarkString("", desc, kvp.Value.lat, kvp.Value.lng);
                    }
                }

                Console.Write("{0}\n{1}\n{2}", beginning, middle, ending);

            }
            catch (Exception e)
            {
                Console.Error.Write("\nERROR: exception: {0}\n{1}", e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// This method returns a single Placemark XML fragment that contains a LineString to be
        /// drawn between two (lat, lng) points.
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns></returns>
        public static string LineString(double lat1, double lng1, double lat2, double lng2)
        {
            string beginning = "<Placemark><LineString><coordinates>";
            string ending = "</coordinates></LineString ></Placemark>";

            return String.Format("\n{0},{1},{2}  {3},{4}{5}", beginning, lng1, lat1, lng2, lat2, ending);
        }

        /// <summary>
        /// This method returns a single Placemark XML fragment for a prescribed placemark
        /// name, description and (lat, lng) location.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public static string PlacemarkString(string name, string description, double latitude, double longitude)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<Placemark>");

            sb.Append("\n<name>" + name + "</name>");

            // The description string could contain CR and/or LF characters.
            // We need to convert these to the equivalent HTML command "<br />"
            // and encapsulate the modified string inside <![CDATA[   ...    ]]>
            if (description == null) description = "";
            StringBuilder descHtmlSb = new StringBuilder();
            descHtmlSb.Append("<![CDATA[");
            foreach (char c in description.ToCharArray())
            {
                switch (c)
                {
                    // LF.
                    case (char)10:
                        descHtmlSb.Append("<br />");
                        break;
                    // CR.
                    case (char)13:
                        break;
                    default:
                        descHtmlSb.Append(c);
                        break;
                }
            }
            descHtmlSb.Append("]]>");

            sb.Append("\n<description>" + descHtmlSb.ToString() + "</description>");

            sb.Append("\n<Point>");

            sb.Append("\n<coordinates>" + longitude.ToString() + ", " + latitude.ToString() + ", 0" + "</coordinates>");

            sb.Append("\n</Point>");

            sb.Append("\n</Placemark>");

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string containing a pair of longitude and latitude values in that order.
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="numDecPlaces"></param>
        /// <returns></returns>
        public static string LatLngToProxyString(double lat, double lng, int numDecPlaces)
        {
            string formatSpecifier = "F" + numDecPlaces.ToString();
            string latStr = lat.ToString(formatSpecifier);
            string lngStr = lng.ToString(formatSpecifier);

            return latStr + "_" + lngStr;
        }














    }
}

```
