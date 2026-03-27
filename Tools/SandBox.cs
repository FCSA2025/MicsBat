using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;

namespace Tools
{
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;            //Invented to mimic (char *) for [In]  only.
    using SQLCHARPTRINOUT = IntPtr;       //Invented to mimic (char *) for [In, Out].
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLINTEGERPTR = IntPtr;
    using SQLLEN = Int64;
    using SQLLENPTR = IntPtr;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;
    using SQLSETPOSIROW = UInt64;
    using SQLSMALLINT = Int16;
    using SQLSMALLINTPTR = IntPtr;
    using SQLULEN = UInt64;
    using SQLUSMALLINT = UInt16;
    using SQLUINTEGER = UInt32;

    public class SandBox
    {

        public static void Go(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\Sandbox.log";
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




            }
            catch (Exception e)
            {
                Console.Error.Write("\n\nERROR: exception: {0}", e.Message);
                Console.Error.Write("\n\n{0}", e.StackTrace);
            }
        }

        /// <summary>
        /// Calculates distance and bearings on the reference model of Earth as an oblate
        /// spheroid; (lat, lng) values use units of a second of arc (3600 times the value in degrees).
        /// </summary>
        /// <remarks>
        /// For a relatively small area such as a city, the earth's surface can be thought of
        /// as a flat surface. 
        /// On the other hand, when high accuracy of larger areas is needed, it is necessary 
        /// to use a more accurate and reliable model of the Earth such as an ellipsoid or geoid.
        /// <para>
        /// Because the earth is not a perfect sphere (it is wider at the equator than at the poles) an 
        /// ellipsoid is often used to model its shape. The reference ellipsoid is defined by its 
        /// dimensions for the major and minor axes and the amount of flattening at the poles.
        /// </para><para>
        /// Ellipsoids that model the earth are very near to being spherical, so close that they can be 
        /// called a spheroid. Since the flattening occurs at the poles due to the centrifugal force of 
        /// the rotation of the earth, the Earth's geometry may be further defined as an oblate spheroid.
        /// </para>
        /// \image html earth_ellipsoid.jpg "Earth as an ellipsoid (exaggerated)."
        /// \image html EllipsoidFigA.PNG "Ellipsoid geometry."
        /// \n\n\n 
        /// \image html EarthEllipsoidFigB.png "Local geometry".
        /// </remarks>
        /// <param name="latStn1Sec"> - latitude of station 1.</param>
        /// <param name="latStn2Sec"> - latitude of station 2.</param>
        /// <param name="longStn1Sec"> - longitude of station 1.</param>
        /// <param name="longStn2Sec"> - longitude of station 2.</param>
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

            // Define constants.
            const double SEC_TO_RAD = 4.84813681E-6;
            const double ECCSQ = 0.00669454;
            const double SQUARE = 2.0;
            const double S = 30.9221917;
            const double PI = 3.1415926535897932384626;
            const double RAD_TO_DEG = 57.295779513;

            /* convert to radians */
            latRad1 = latStn1Sec * SEC_TO_RAD;
            latRad2 = latStn2Sec * SEC_TO_RAD;
            longRad1 = longStn1Sec * SEC_TO_RAD;
            longRad2 = longStn2Sec * SEC_TO_RAD;

            /* compute latitudinal and longitudinal diffs between the stations */
            latDiff = (latRad1 - latRad2 == 0.0) ? 1.0E-8 : latRad1 - latRad2;
            longDiff = (longRad1 - longRad2 == 0.0) ? 1.0E-8 : longRad1 - longRad2;

            latAvr = latRad2 + (latDiff / 2.0);

            c1 = 1.0 - (ECCSQ * Math.Pow(Math.Sin(latAvr), SQUARE));
            am = Math.Sqrt(c1) / S;
            bearingMid = Math.Abs(Math.Atan(longDiff * Math.Cos(latAvr) * c1 / ((1.0 - ECCSQ) * latDiff)));
            bearingDiff = longDiff * Math.Sin(latAvr);
            bearing12 = bearingMid - bearingDiff / 2.0;

            if (latDiff >= 0.0)
            {
                if (longDiff < 0.0)
                {
                    bearing12 = PI + bearing12;
                }
                else
                {
                    bearing12 = PI - bearing12 - bearingDiff;
                }
            }
            else if (longDiff <= 0.0)
            {
                bearing12 = 2.0 * PI - bearing12 - bearingDiff;
            }

            bearing21 = bearing12 + bearingDiff + PI;
            if (bearing21 >= 2.0 * PI)
            {
                bearing21 = bearing21 - 2.0 * PI;
            }

            distanceKm = Math.Abs((longDiff * Math.Cos(latAvr)) / (am * Math.Sin(bearingMid) *
                        0.0048481368));
            if (distanceKm < 0.001)
            {
                distanceKm = 0.001;
            }

            /* convert bearings back to degrees */
            bearing12 = bearing12 * RAD_TO_DEG;
            bearing21 = bearing21 * RAD_TO_DEG;
        }




    }
}
