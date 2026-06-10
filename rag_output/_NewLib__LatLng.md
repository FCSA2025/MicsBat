# Documented File: LatLng.cs
**Repository Path:** `_NewLib\LatLng.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
﻿using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// This class encapsulates the concept of a (lat, lng) pair.
    /// </summary>
    public class LatLng
    {
        private double mLat = double.MaxValue;
        private double mLng = double.MaxValue;

        public double Lat { get { return mLat; } set { mLat = value; } }
        public double Lng { get { return mLng; } set { mLng = value; } }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LatLng() { }

        /// <summary>
        /// Creates a LatLng object for a prescribed (lat, lng) pair.
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        public LatLng(double lat, double lng)
        {
            mLat = lat;
            mLng = lng;
        }

        /// <summary>
        /// Creates a LatLng object for a prescribed (lat, lng) pair given in the
        /// form of a comma-separated string.
        /// </summary>
        /// <param name="latCommaLng"></param>
        public LatLng(string latCommaLng)
        {
            string[] values = latCommaLng.Split(',');
            mLat = Convert.ToDouble(values[0]);
            mLng = Convert.ToDouble(values[1]);
        }

        /// <summary>
        /// Returns a string value containing the object's (lat, lng) pair values.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0},{1}", mLat, mLng);
        }

        /// <summary>
        /// This method calculates the Euclidean (flat-earth) distance between two points; distance is in kms.
        /// </summary>
        /// <param name="lat1">  latitude of 1st position in degrees.</param>
        /// <param name="lat2"> longitude of 1st position in degrees.</param>
        /// <param name="lng1">  latitude of 2nd position in degrees.</param>
        /// <param name="lng2"> longitude of 2nd position in degrees.</param>
        /// <returns></returns>
        public static double EuclidDist(double lat1, double lng1, double lat2, double lng2)
        {
            // Define the radius of the earth as its Volumetric Radius.
            // The Volumetric Radius is the radius of a spherical Earth that 
            // has the same volume as the IAU 1968 Ellipsoid.
            const double r = 6371.0008; // kms.

            // Convert to radians.
            const double DEG_TO_RAD = 0.01745329251994329576923690768489;
            lat1 *= DEG_TO_RAD;
            lng1 *= DEG_TO_RAD;
            lat2 *= DEG_TO_RAD;
            lng2 *= DEG_TO_RAD;

            double q = r * Math.Cos(lat1);
            double x1 = q * Math.Cos(lng1);
            double y1 = q * Math.Sin(lng1);
            double z1 = q * Math.Sin(lat1);

            q = r * Math.Cos(lat2);
            double x2 = q * Math.Cos(lng2);
            double y2 = q * Math.Sin(lng2);
            double z2 = q * Math.Sin(lat2);

            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2) + Math.Pow(z1 - z2, 2)); ;
        }

        /// <summary>
        /// This method uses Haversine's formula for the geodesic distance over the 
        /// Earth's surface between two points, assuming a spherical earth; the returned
        /// distance is in kms.
        /// </summary>
        /// <param name="lat1">  latitude of 1st position in degrees.</param>
        /// <param name="lat2"> longitude of 1st position in degrees.</param>
        /// <param name="lng1">  latitude of 2nd position in degrees.</param>
        /// <param name="lng2"> longitude of 2nd position in degrees.</param>
        /// <returns></returns>
        public static double Haversine(double lat1, double lng1, double lat2, double lng2)
        {
            // Define the radius of the earth as its Volumetric Radius.
            // The Volumetric Radius is the radius of a spherical Earth that 
            // has the same volume as the IAU 1968 Ellipsoid.
            const double r = 6371.0008; // kms.

            // Convert to radians.
            const double DEG_TO_RAD = 0.01745329251994329576923690768489;
            lat1 *= DEG_TO_RAD;
            lng1 *= DEG_TO_RAD;
            lat2 *= DEG_TO_RAD;
            lng2 *= DEG_TO_RAD;

            var sdlat = Math.Sin((lat2 - lat1) / 2);
            var sdlon = Math.Sin((lng2 - lng1) / 2);
            var q = sdlat * sdlat + Math.Cos(lat1) * Math.Cos(lat2) * sdlon * sdlon;
            var d = 2 * r * Math.Asin(Math.Sqrt(q));

            return d;
        }

        /// <summary>
        /// This method uses Haversine's formula for the geodesic distance over the 
        /// Earth's surface between two points, assuming a spherical earth; the returned
        /// distance is in kms.
        /// </summary>
        /// <param name="latLng1"> - LatLng object prescribing the first position.</param>
        /// <param name="latLng2"> - LatLng object prescribing the second position.</param>
        /// <returns></returns>
        public static double Haversine(LatLng latLng1, LatLng latLng2)
        {
            return Haversine(latLng1.Lat, latLng1.Lng, latLng2.Lat, latLng2.Lng);
        }

        /// <summary>
        /// This method uses Vincenty's formula for the geodesic distance over the 
        /// Earth's surface between two points on an oblate spheroid; the returned
        /// distance is in kms.
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns></returns>
        public static double Vincenty(double lat1, double lng1, double lat2, double lng2)
        {
            double a = 6378137, b = 6356752.3142, f = 1 / 298.257223563; // WGS-84
                                                                         // ellipsiod
            const double DEG_TO_RAD = 0.01745329251994329576923690768489;
            const double EPSILON = 1.0E-12;

            double L = (lng2 - lng1) * DEG_TO_RAD;
            double U1 = Math.Atan((1 - f) * Math.Tan(lat1 * DEG_TO_RAD));
            double U2 = Math.Atan((1 - f) * Math.Tan(lat2 * DEG_TO_RAD));
            double sinU1 = Math.Sin(U1), cosU1 = Math.Cos(U1);
            double sinU2 = Math.Sin(U2), cosU2 = Math.Cos(U2);

            double cosSqAlpha, sinSigma, cos2SigmaM, cosSigma, sigma;

            double lambda = L, lambdaP, iterLimit = 20;
            do
            {
                double sinLambda = Math.Sin(lambda), cosLambda = Math.Cos(lambda);
                sinSigma = Math.Sqrt((cosU2 * sinLambda) * (cosU2 * sinLambda) + (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda) * (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda));
                if (sinSigma == 0)
                {
                    return 0; // co-incident points
                }
                cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
                sigma = Math.Atan2(sinSigma, cosSigma);
                double sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
                cosSqAlpha = 1 - sinAlpha * sinAlpha;
                cos2SigmaM = cosSigma - 2 * sinU1 * sinU2 / cosSqAlpha;
                if (double.IsNaN(cos2SigmaM))
                {
                    cos2SigmaM = 0; // equatorial line: cosSqAlpha=0
                }
                double C = f / 16 * cosSqAlpha * (4 + f * (4 - 3 * cosSqAlpha));
                lambdaP = lambda;
                lambda = L + (1 - C) * f * sinAlpha * (sigma + C * sinSigma * (cos2SigmaM + C * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));
            } while (Math.Abs(lambda - lambdaP) > EPSILON && --iterLimit > 0);

            if (iterLimit == 0)
            {
                return double.NaN;
            }
            double uSquared = cosSqAlpha * (a * a - b * b) / (b * b);
            double A = 1 + uSquared / 16384 * (4096 + uSquared * (-768 + uSquared * (320 - 175 * uSquared)));
            double B = uSquared / 1024 * (256 + uSquared * (-128 + uSquared * (74 - 47 * uSquared)));
            double deltaSigma = B * sinSigma * (cos2SigmaM + B / 4 * (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM) - B / 6 * cos2SigmaM * (-3 + 4 * sinSigma * sinSigma) * (-3 + 4 * cos2SigmaM * cos2SigmaM)));
            double s = b * A * (sigma - deltaSigma);

            // Convert to kms.
            return s * 0.001;
        }

        /// <summary>
        /// This method uses Vincenty's formula for the geodesic distance over the 
        /// Earth's surface between two points on an oblate spheroid; the returned
        /// distance is in kms.
        /// </summary>
        /// <param name="latLng1"></param>
        /// <param name="latLng2"></param>
        /// <returns></returns>
        public static double Vincenty(LatLng latLng1, LatLng latLng2)
        {
            return Vincenty(latLng1.Lat, latLng1.Lng, latLng2.Lat, latLng2.Lng);
        }

        /// <summary>
        /// Calculates distance and bearings on the reference model of Earth as an oblate
        /// spheroid, as per the legacy AxSub2.AxDistan().
        /// </summary>
        /// <param name="latStn1Sec"> - latitude of station 1 in seconds of arc.</param>
        /// <param name="latStn2Sec"> - latitude of station 2 in seconds of arc.</param>
        /// <param name="longStn1Sec"> - longitude of station 1 in seconds of arc.</param>
        /// <param name="longStn2Sec"> - longitude of station 2 in seconds of arc.</param>
        /// <param name="distanceKm"> - calculated distance between stations 1 & 2.</param>
        /// <param name="bearing12"> - calculated bearing from station 1 to station 2.</param>
        /// <param name="bearing21"> - calculated bearing from station 2 to station 1.</param>
        public static void AxDistan(
                      double latStn1Sec,    /* latitude for Stn 1 */
                      double latStn2Sec,    /* latitude for Stn 2 */
                      double longStn1Sec,   /* longitude for Stn 1 */
                      double longStn2Sec,   /* longitude for Stn 2 */
                      out double distanceKm,   /* output - distance between stations */
                      out double bearing12,    /* output - bearing from Stn 1 to 2 */
                      out double bearing21)    /* output - bearing from Stn 2 to 1 */
        {
            double latRad1,                /* latitude in radians of stn 1      */
                            latRad2,                /* latitude in radians of stn 2      */
                            longRad1,               /* longitude in radians of stn 1     */
                            longRad2,               /* longitude in radians of stn 2     */
                            latDiff,                /* lat diff between stn 1 & 2        */
                            longDiff,               /* long diff between stn 1 & 2       */
                            latAvr,                 /* average latitude in radians       */
                            c1,                     /* temporary variable                */
                            am,                     /* temporary variable                */
                            bearingMid,             /* bearing from 1 to 2 at path midpnt*/
                            bearingDiff;            /* diff in bearing at end of path    */


            /* convert to radians */
            latRad1 = latStn1Sec * Constant.SEC_TO_RAD;
            latRad2 = latStn2Sec * Constant.SEC_TO_RAD;
            longRad1 = longStn1Sec * Constant.SEC_TO_RAD;
            longRad2 = longStn2Sec * Constant.SEC_TO_RAD;

            /* compute latitudinal and longitudinal diffs between the stations */
            latDiff = (latRad1 - latRad2 == 0.0) ? 1.0E-8 : latRad1 - latRad2;
            longDiff = (longRad1 - longRad2 == 0.0) ? 1.0E-8 : longRad1 - longRad2;

            latAvr = latRad2 + (latDiff / 2.0);

            c1 = 1.0 - (Constant.ECCSQ * Math.Pow(Math.Sin(latAvr), Constant.SQUARE));
            am = Math.Sqrt(c1) / Constant.S;
            bearingMid = Math.Abs(Math.Atan(longDiff * Math.Cos(latAvr) * c1 / ((1.0 - Constant.ECCSQ) * latDiff)));
            bearingDiff = longDiff * Math.Sin(latAvr);
            bearing12 = bearingMid - bearingDiff / 2.0;

            if (latDiff >= 0.0)
            {
                if (longDiff < 0.0)
                {
                    bearing12 = Constant.PI + bearing12;
                }
                else
                {
                    bearing12 = Constant.PI - bearing12 - bearingDiff;
                }
            }
            else if (longDiff <= 0.0)
            {
                bearing12 = 2.0 * Constant.PI - bearing12 - bearingDiff;
            }

            bearing21 = bearing12 + bearingDiff + Constant.PI;
            if (bearing21 >= 2.0 * Constant.PI)
            {
                bearing21 = bearing21 - 2.0 * Constant.PI;
            }

            distanceKm = Math.Abs((longDiff * Math.Cos(latAvr)) / (am * Math.Sin(bearingMid) *
                        0.0048481368));
            if (distanceKm < 0.001)
            {
                distanceKm = 0.001;
            }

            /* convert bearings back to degrees */
            bearing12 = bearing12 * Constant.RAD_TO_DEG;
            bearing21 = bearing21 * Constant.RAD_TO_DEG;
        }

        /// <summary>
        /// Calculates distance and bearings on the reference model of Earth as an oblate
        /// spheroid, as per the legacy AxSub2.AxDistan().
        /// </summary>
        /// <param name="latStn1Deg"></param>
        /// <param name="longStn1Deg"></param>
        /// <param name="latStn2Deg"></param>
        /// <param name="longStn2Deg"></param>
        /// <returns></returns>
        public static double AxDistDeg(
                                        double latStn1Deg,       /* latitude  for Stn 1 */
                                        double longStn1Deg,      /* longitude for Stn 1 */
                                        double latStn2Deg,       /* latitude  for Stn 2 */
                                        double longStn2Deg       /* longitude for Stn 2 */
                                        )
        {
            // Convert to seconds.
            const double DEG_TO_SEC = 3600.0;
            double latStn1Sec = latStn1Deg * DEG_TO_SEC;
            double latStn2Sec = latStn2Deg * DEG_TO_SEC;
            double longStn1Sec = longStn1Deg * DEG_TO_SEC;
            double longStn2Sec = longStn2Deg * DEG_TO_SEC;

            double distanceKm;
            double bearing12, bearing21;

            // Call the legacy AxDistan().
            AxDistan(latStn1Sec, latStn2Sec, longStn1Sec, longStn2Sec, out distanceKm, out bearing12, out bearing21);

            return distanceKm;
        }

        /// <summary>
        /// This method writes to the console giving a comparison of the results returned by the Euclidean, Haversine, Vincenty,
        /// and legacy AxDistDeg() methods.
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        public static void CompareDistanceMethods(double lat1, double lng1, double lat2, double lng2)
        {
            Console.Write("\nEuclidDist = {0:F3}", LatLng.EuclidDist(lat1, lng1, lat2, lng2));

            Console.Write("\nHaversine = {0:F3}", LatLng.Haversine(lat1, lng1, lat2, lng2));

            Console.Write("\nVincenty  = {0:F3}", LatLng.Vincenty(lat1, lng1, lat2, lng2));

            Console.Write("\nAxDistDeg = {0:F3}", AxDistDeg(lat1, lng1, lat2, lng2));
        }

        /// <summary>
        /// This method writes to the console giving a comparison of the results returned by the Eucliden, Haversine, Vincenty,
        /// and legacy AxDistDeg() methods.
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        public static string CompareDistanceMethodsAsCSV(double lat1, double lng1, double lat2, double lng2)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(String.Format("{0:F6}, {1:F6}, {2:F6}, {3:F6}, ", lat1, lng1, lat2, lng2));

            sb.Append(String.Format("{0:F3}, ", LatLng.EuclidDist(lat1, lng1, lat2, lng2)));

            sb.Append(String.Format("{0:F3}, ", LatLng.Haversine(lat1, lng1, lat2, lng2)));

            sb.Append(String.Format("{0:F3}, ", LatLng.Vincenty(lat1, lng1, lat2, lng2)));

            sb.Append(String.Format("{0:F3}", AxDistDeg(lat1, lng1, lat2, lng2)));

            return sb.ToString();
        }

        /// <summary>
        /// This method converts double (lat, lng) values to strings (including N-S, E-W sense letter).
        /// </summary>
        /// <param name="latDeg"></param>
        /// <param name="lngDeg"></param>
        /// <param name="latStr"></param>
        /// <param name="lngStr"></param>
        /// <returns></returns>
        public static int ConvertToString(double latDeg, double lngDeg, out string latStr, out string lngStr)
        {
            // 'out'.
            latStr = "";
            lngStr = "";

            string latSense = (latDeg >= 0.0) ? "N" : "S";
            string lngSense = (lngDeg >= 0.0) ? "E" : "W";

            LatLng.ConvertToString(Constant.LATITUDE, Math.Abs(latDeg), out latStr);
            LatLng.ConvertToString(Constant.LONGITUDE, Math.Abs(lngDeg), out lngStr);

            latStr += latSense;
            lngStr += lngSense;

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method converts integer centisecond (lat, lng) values to strings (including N-S, E-W sense letter).
        /// </summary>
        /// <param name="latCS"></param>
        /// <param name="lngCS"></param>
        /// <param name="latStr"></param>
        /// <param name="lngStr"></param>
        /// <returns></returns>
        public static int ConvertToString(int latCS, int lngCS, out string latStr, out string lngStr)
        {
            // 'out'.
            latStr = "";
            lngStr = "";

            string latSense = (latCS >= 0.0) ? "N" : "S";
            string lngSense = (lngCS >= 0.0) ? "W" : "E";  // (sic.)

            LatLng.ConvertToString(Constant.LATITUDE, Math.Abs(latCS), out latStr);
            LatLng.ConvertToString(Constant.LONGITUDE, Math.Abs(lngCS), out lngStr);

            latStr += latSense;
            lngStr += lngSense;

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method converts an individual latitude or longitude centisecond value to 
        /// a string with format ddd-mm-ss.ss  .
        /// </summary>
        /// <param name="latLngType"></param>
        /// <param name="latLngCS"></param>
        /// <param name="latLngStr"></param>
        /// <returns></returns>
        public static int ConvertToString(int latLngType, int latLngCS, out string latLngStr)
        {
            // 'out'.
            latLngStr = "";

            int centiseconds = latLngCS;

            // Get the integer degrees string.
            int degrees = centiseconds / 360000;
            int remainder = centiseconds - degrees * 360000;
            string degreesStr = "XXX";
            if (latLngType == Constant.LATITUDE)
            {
                degreesStr = String.Format("{0:D2}", degrees);
            }
            else if (latLngType == Constant.LONGITUDE)
            {
                degreesStr = String.Format("{0:D3}", degrees);
            }

            // Get the integer minutes string.
            int minutes = remainder / 6000;
            remainder = remainder - minutes * 6000;
            string minutesStr = String.Format("{0:D2}", minutes);

            // Get the integer seconds string.
            int seconds = remainder / 100;
            remainder = remainder - seconds * 100;
            string secondsStr = String.Format("{0:D2}", seconds);

            // Get the remaining hundrethsOfSecond.
            int hundrethsOfSecond = remainder;
            string hundrethsOfSecondStr = String.Format("{0:D2}", hundrethsOfSecond);

            latLngStr = String.Format("{0}-{1}-{2}.{3}", degreesStr, minutesStr, secondsStr, hundrethsOfSecondStr);

            //...Log2.v("\nLatLng.ConvertToString(): {0}, {1}, {2}, {3}, {4}", latLngCS, degrees, minutes, seconds, hundrethsOfSecond);

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method converts an individual latitude or longitude double value to 
        /// a string with format ddd-mm-ss.ss  .
        /// </summary>
        /// <param name="latLngType"></param>
        /// <param name="latLngDeg"></param>
        /// <param name="latLngStr"></param>
        /// <returns></returns>
        public static int ConvertToString(int latLngType, double latLngDeg, out string latLngStr)
        {
            // 'out'.
            latLngStr = "";

            // Convert degrees to centiseconds so that we can use integer modulo arithmetic.
            int centiseconds = (int)(latLngDeg * 360000 + 0.5);

            ConvertToString(latLngType, centiseconds, out latLngStr);

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method converts (lat, lng) strings to double values.
        /// </summary>
        /// <param name="latStr"></param>
        /// <param name="lngStr"></param>
        /// <param name="latDeg"></param>
        /// <param name="lngDeg"></param>
        /// <returns></returns>
        public static int ConvertToDegrees(string latStr, string lngStr, out double latDeg, out double lngDeg)
        {
            // 'out'.
            latDeg = double.MinValue;
            lngDeg = double.MinValue;

            int latCS;
            int lngCS;

            // Latitude.
            string latitNS = Strings.LastChar(latStr).ToString().ToUpper();
            string strLatit = latStr.Substring(0, latStr.Length - 1);
            int retValLat = UtStrConvLongLat(Constant.LATITUDE, strLatit, latitNS, out latCS);

            // Longitude.
            string lngitEW = Strings.LastChar(lngStr).ToString().ToUpper();
            string strLngit = lngStr.Substring(0, lngStr.Length - 1);
            int retValLng = UtStrConvLongLat(Constant.LONGITUDE, strLngit, lngitEW, out lngCS);

            // Convert centiseconds to decimal degrees.
            latDeg = latCS / 360000.0;
            lngDeg = lngCS / 360000.0;

            // Handle the sign.
            if (strLatit == "S") latDeg = -latDeg;
            if (strLngit == "W") lngDeg = -lngDeg;

            return (retValLat == Constant.SUCCESS) && (retValLng == Constant.SUCCESS) ? Constant.SUCCESS : Constant.FAILURE;
        }

        /// <summary>
        /// This method converts integer centisecond (lat, lng) values to double values.
        /// </summary>
        /// <param name="latCS"></param>
        /// <param name="lngCS"></param>
        /// <param name="latDeg"></param>
        /// <param name="lngDeg"></param>
        /// <returns></returns>
        public static int ConvertToDegrees(int latCS, int lngCS, out double latDeg, out double lngDeg)
        {
            // Convert centiseconds to decimal degrees.
            latDeg = latCS / 360000.0;
            lngDeg = -lngCS / 360000.0;

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method converts (lat, lng) strings into integer centiseconds.
        /// </summary>
        /// <param name="latStr"></param>
        /// <param name="lngStr"></param>
        /// <param name="latCS"></param>
        /// <param name="lngCS"></param>
        /// <returns></returns>
        public static int ConvertToCentiseconds(string latStr, string lngStr, out int latCS, out int lngCS)
        {
            // 'out'.
            latCS = int.MinValue;
            lngCS = int.MinValue;

            // Latitude.
            string latitNS = Strings.LastChar(latStr).ToString().ToUpper();
            string strLatit = latStr.Substring(0, latStr.Length - 1);
            int retValLat = UtStrConvLongLat(Constant.LATITUDE, strLatit, latitNS, out latCS);

            // Longitude.
            string lngitEW = Strings.LastChar(lngStr).ToString().ToUpper();
            string strLngit = lngStr.Substring(0, lngStr.Length - 1);
            int retValLng = UtStrConvLongLat(Constant.LONGITUDE, strLngit, lngitEW, out lngCS);

            return (retValLat == Constant.SUCCESS) && (retValLng == Constant.SUCCESS) ? Constant.SUCCESS : Constant.FAILURE;
        }

        /// <summary>
        /// This method converts (lat, lng) double values into integer centiseconds.
        /// </summary>
        /// <param name="latDeg"></param>
        /// <param name="lngDeg"></param>
        /// <param name="latCS"></param>
        /// <param name="lngCS"></param>
        /// <returns></returns>
        public static int ConvertToCentiseconds(double latDeg, double lngDeg, out int latCS, out int lngCS)
        {
            // Convert decimal degrees to centiseconds.
            latCS = (int)(latDeg * 360000 + 0.5);
            lngCS = -(int)(lngDeg * 360000 + 0.5);  // (sic.)

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method converts a latitude or longitude string to centiseconds of arc.
        /// </summary>
        /// <param name="latLongCode"></param>
        /// <param name="latLongStr"></param>
        /// <param name="orientStr"></param>
        /// <param name="latLong"></param>
        /// <returns></returns>
        public static int UtStrConvLongLat(int latLongCode,
                                            string latLongStr,
                                            string orientStr,
                                            out int latLong)
        {
            //...Log2.v(String.Format("\nLatLng.UtStrConvLongLat(): Entry: {0}  {1}  {2}", latLongCode, latLongStr, orientStr));

            // 'out' requirement.
            latLong = Int32.MinValue;

            string[] tokens;      // [Constant.MAX_TOKENS];      /* tokens found */                            /* work string */
            int tc;

            int degrees;
            int minutes;
            int seconds;
            int hundSeconds;
            int orientation;

            try
            {
                /* check orientation characters */
                if (((latLongCode == Constant.LONGITUDE)
                        && (!orientStr.Equals(Constant.EAST))
                        && (!orientStr.Equals(Constant.WEST)))
                || ((latLongCode == Constant.LATITUDE)
                        && (!orientStr.Equals(Constant.NORTH))
                        && (!orientStr.Equals(Constant.SOUTH))))
                {
                    /* invalid orientation for given latLongCode */
                    Log2.e("\nLatLng.UtStrConvLongLat(): ERROR: invalid orientation for given latLongCode.");
                    return (Constant.FAILURE);
                }

                /* break down workString into tokens */
                tc = TokenizeLatLong(latLongStr, out tokens);

                foreach (string token in tokens)
                {
                    //...Log2.v("\nLatLng.UtStrConvLongLat(): " + Strings.AddBars(token));
                }

                /* init individual values */
                degrees = minutes = seconds = hundSeconds = 0;

                // Input Lat/Long format: 00-00-00.00

                /* convert each token to it's respective value.  The last token is
                   hundreths of seconds (if it exists), the second last is seconds
                   (if it exists), the third last is minutes (if it exists) and
                   the fourth last is degrees (if it exists).
                */

                // Degrees.
                degrees = Convert.ToInt32(tokens[0]);
                if (((latLongCode == Constant.LATITUDE)
                        && (degrees > Constant.MAX_LAT_DEGREES))
                    || ((latLongCode == Constant.LONGITUDE)
                        && (degrees > Constant.MAX_LONG_DEGREES)))
                {
                    Log2.e("\nLatLng.UtStrConvLongLat(): ERROR: degrees: invalid value: " + degrees);
                    return (Constant.FAILURE);
                }

                if (degrees < 0)
                {
                    /* token string invalid - too large possibly */
                    Log2.e("\nLatLng.UtStrConvLongLat(): ERROR: degrees: invalid value: " + degrees);
                    return (Constant.FAILURE);
                }

                // Minutes.
                minutes = Convert.ToInt32(tokens[1]);
                if (minutes > Constant.MAX_MINUTES)
                {
                    return (Constant.FAILURE);
                }
                if (minutes < 0)
                {
                    /* token string invalid - too large possibly */
                    Log2.e("\nLatLng.UtStrConvLongLat(): ERROR: minutes: invalid value: " + minutes);
                    return (Constant.FAILURE);
                }

                // Seconds.
                seconds = Convert.ToInt32(tokens[2]);
                if (seconds > Constant.MAX_SECONDS_LL)
                {
                    Log2.e("\nLatLng.UtStrConvLongLat(): ERROR: seconds: invalid value: " + seconds);
                    return (Constant.FAILURE);
                }
                if (seconds < 0)
                {
                    /* token string invalid - too large possibly */
                    Log2.e("\nLatLng.UtStrConvLongLat(): ERROR: seconds: invalid value: " + seconds);
                    return (Constant.FAILURE);
                }

                // Hundreths of seconds.
                if (String.IsNullOrWhiteSpace(tokens[3]))
                {
                    hundSeconds = 0;
                }
                else
                {
                    hundSeconds = Convert.ToInt32(tokens[3]);
                    if (tokens[3].Length == 1)
                    {
                        hundSeconds *= 10;
                    }
                    else if (tokens[3].Length > 2)
                    {
                        Log2.e("\nLatLng.UtStrConvLongLat(): ERROR: hundSeconds: too many digits: " + tokens[3]);
                        return (Constant.FAILURE);
                    }
                    if (hundSeconds < 0)
                    {
                        /* token string invalid - too large possibly */
                        Log2.e("\nLatLng.UtStrConvLongLat(): ERROR: hundSeconds: invalid value: " + hundSeconds);
                        return (Constant.FAILURE);
                    }
                }

                // Allow a value MAX_LAT_DEGREES/MAX_LONG_DEGREES
                // only if the other values are 0.
                if ((((latLongCode == Constant.LATITUDE) && (degrees == Constant.MAX_LAT_DEGREES))
                            || ((latLongCode == Constant.LONGITUDE) && (degrees == Constant.MAX_LONG_DEGREES)))
                        &&
                            ((minutes != 0) || (seconds != 0) || (hundSeconds != 0))
                   )
                {
                    Log2.e("\nLatLng.UtStrConvLongLat(): ERROR: invalid MAX_LAT_DEGREES/MAX_LONG_DEGREES: " + latLongStr);
                    return (Constant.FAILURE);
                }

                orientation = 1;    /* positive orientation */

                if ((orientStr.Equals(Constant.SOUTH)) || (orientStr.Equals(Constant.EAST)))
                {
                    /* set orientation to negative */
                    orientation = -1;
                }

                /* calculate converted value */
                latLong = ((((((degrees * 60) + minutes) * 60) + seconds) * 100) + hundSeconds) * orientation;

                //...Log2.v(String.Format("\nLatLng.UtStrConvLongLat(): {0} - {1} - {2} - {3}", degrees, minutes, seconds, hundSeconds));

                //...Log2.v(String.Format("\nLatLng.UtStrConvLongLat(): Exit: latLong = {0}", latLong));
                return (Constant.SUCCESS);

            }
            catch (Exception e)
            {
                Log2.e("\nLatLng.UtStrConvLongLat(): ERROR: string to integer conversion exception: " + e.Message);
                Log2.e("\n{0}", e.StackTrace);
                return (Constant.FAILURE);
            }
        }

        /// <summary>
        /// This method parses the numerical elements of a latitude and/or longitude string
        /// (format: 00-00-00.00) and returns a four-element string array corresponding to the
        /// degrees, minutes, seconds and centiseconds.
        /// </summary>
        /// <param name="str"> - the prescribed string to be parsed.</param>
        /// <param name="tokens"> - populated string[4] object.</param>
        /// <returns> - Constant.MAX_TOKENS.</returns>
        public static int TokenizeLatLong(string str, out string[] tokens)
        {
            // 'out' requirement
            tokens = new string[Constant.MAX_TOKENS];

            int tc;     /* token counter */
            bool done;   /* signal when done flag */
            string wt;  /* work token pointer */

            /* If last char in string is '.' then it must be removed or will
             * cause problems later.  Check for this and replace it with
             * NULL if there.
            */
            if (Strings.LastCharIs(str, '.'))
            {
                str = str.Substring(0, str.Length - 1);
            }

            done = false;
            tc = 0;     /* initialize token counter */
            wt = "";     /* begin of string - first token */

            /* While not pointing to NULL terminator and done flag not set */
            int i = 0;
            while ((i < str.Length) && !done)
            {

                char c = str[i++];

                /* If character is a separator char (the '-' or '.') */
                if ((c == Constant.SEP_CHAR1) || (c == Constant.SEP_CHAR2))
                {
                    tokens[tc++] = wt;  /* register previous token */

                    if (tc == Constant.MAX_TOKENS)
                    {
                        return (Constant.MAX_TOKENS); /* too many tokens */
                    }

                    wt = ""; /* start next token */

                    if (c == Constant.SEP_CHAR2)
                    {
                        /* decimal seperator encountered */
                        tokens[tc++] = str.Substring(i);  /* the remainder of
							 * the string is the
							 * hundreds seconds
							 * token */
                        done = true;   /* set done flag */
                    }

                }
                else
                {
                    // Accumulate the character into the token buffer.
                    wt += c;

                    // AH: Bug fix on 20190728.
                    // We have to ensure that the final token is saved.
                    if (i == str.Length)
                    {
                        tokens[tc++] = wt;  /* register final token */
                        done = true;   /* set done flag */
                    }
                    // AH: end of bug fix.
                }

            }  // while()

            return (Constant.MAX_TOKENS);
        }


    }
}

```
