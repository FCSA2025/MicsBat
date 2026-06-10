# Documented File: AxSub2.cs
**Repository Path:** `_Auxlib\AxSub2.cs`
**Primary Layer:** `_Auxlib`
**Namespace:** `_Auxlib`

## Source Code Representation
```csharp
using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _Auxlib
{
    /// <summary>
    /// Provides methods for distance and bearing calculations using the IAU 1968 ellipsoid model for the Earth's surface.
    /// </summary>
    public class AxSub2
    {

#if PINVOKE
        [DllImport("auxlib.dll", CharSet = CharSet.Ansi)]
        private extern static void axDistan(double latStn1Sec, double latStn2Sec, double longStn1Sec, double longStn2Sec, [In, Out] ref double distanceKm, [In, Out] ref double bearing12, [In, Out] ref double bearing21);
        //--------------------------------------------------------------------
        public static void AxDistan_NATIVE(double latStn1Sec, double latStn2Sec, double longStn1Sec, double longStn2Sec, ref double distanceKm, ref double bearing12, ref double bearing21)
        {
            axDistan(latStn1Sec, latStn2Sec, longStn1Sec, longStn2Sec, ref distanceKm, ref bearing12, ref bearing21);
        }
#endif

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
        /// spheroid; (lat, lng) values use unit of a degrees.
        /// </summary>
        /// <param name="latStn1Deg"></param>
        /// <param name="latStn2Deg"></param>
        /// <param name="longStn1Deg"></param>
        /// <param name="longStn2Deg"></param>
        /// <param name="distanceKm"></param>
        /// <param name="bearing12"></param>
        /// <param name="bearing21"></param>
        public static void AxDistanDeg(
                                        double latStn1Deg,       /* latitude for Stn 1 */
                                        double latStn2Deg,       /* latitude for Stn 2 */
                                        double longStn1Deg,      /* longitude for Stn 1 */
                                        double longStn2Deg,      /* longitude for Stn 2 */
                                        out double distanceKm,   /* output - distance between stations */
                                        out double bearing12,    /* output - bearing from Stn 1 to 2 */
                                        out double bearing21)    /* output - bearing from Stn 2 to 1 */
        {
            // Convert to seconds.
            const double DEG_TO_SEC = 3600.0;
            double latStn1Sec = latStn1Deg * DEG_TO_SEC;
            double latStn2Sec = latStn2Deg * DEG_TO_SEC;
            double longStn1Sec = longStn1Deg * DEG_TO_SEC;
            double longStn2Sec = longStn2Deg * DEG_TO_SEC;

            // Call the legacy AxDistan().
            AxDistan(latStn1Sec, latStn2Sec, longStn1Sec, longStn2Sec, out distanceKm, out bearing12, out bearing21);

            return;
        }







    }
}

```
