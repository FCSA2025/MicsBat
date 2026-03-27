using _Auxlib;
using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFDcont
{
    /// <summary>
    /// This class provides methods that perform spherical trigonometric calculations for
    /// a spherical earth.
    /// </summary>
    public class Sphere
    {

        /// <summary>
        /// This method returns the latitude and longitude of a point on a spherical
        /// earth, defined as being at a prescribed distance and azimuth from a 
        /// prescribed (lat, long) location, assuming a spherical earth and using 
        /// spherical trigronometry to compute 'exact' results (as opposed to a local 
        /// 'flat earth' approximation).
        /// </summary>
        /// <remarks>
        /// We first calculate the endpoint using spherical trigonometry. Then we 
        /// calculate the actual distance and bearing to this point using the method
        /// AxDistan(). We then use the difference in bearing to correct the (lat, long)
        /// of the end point and iterate this process until we achieve the desired azimuthal accuracy.
        /// 
        /// All input and output angles are in decimal degrees.
        /// </remarks>
        /// <param name="dLatin"></param>
        /// <param name="dLongin"></param>
        /// <param name="dDistKm"></param>
        /// <param name="dAzimuth"></param>
        /// <param name="dLatout"></param>
        /// <param name="dLongout"></param>
        /// <returns></returns>
        public static int InvDist(double dLatin,
                            double dLongin,
                            double dDistKm,
                            double dAzimuth,
                            out double dLatout,
                            out double dLongout)
        {
            // 'out' requirements.
            dLatout = Double.MinValue;
            dLongout = Double.MinValue;

            double dLat2;
            double dLatsec2;
            double dLong2;
            double dLongsec2;
            double dDist2Km;
            double dBearing12;
            double dBearing21;
            double dAzDiff;
            double dDistDiff;
            double dLatsec = dLatin * 3600.0;
            double dLongsec = dLongin * 3600.0;
            double dNewDist = dDistKm;
            double dNewAz = Math.IEEERemainder(dAzimuth, 360.0); // fmod(dAzimuth, 360.0);
            int nRet = 1;
            int nCount = 10;

            while ((nCount--) != 0)
            {
                /*	Get spherical earth location */
                LLFromDistSE(dLatin, dLongin, dNewDist, dNewAz, out dLat2, out dLong2);

                /*	In case we bomb, return the latest findings */
                dLatout = dLat2;
                dLongout = dLong2;

                /*	The last routine returns values in degrees.  Convert to seconds */
                dLatsec2 = dLat2 * 3600.0;
                dLongsec2 = dLong2 * 3600.0;

                /*	Now calculate the actual distance and bearing using axDistan */
                AxSub2.AxDistan(dLatsec, dLatsec2, dLongsec, dLongsec2,
                                out dDist2Km, out dBearing12, out dBearing21);

                dAzDiff = dAzimuth - dBearing12;
                dDistDiff = dDistKm - dDist2Km;

                /*	We now have a distance and bearing correction.  Enter these into the 
                *		lat/long calcs and try again. */
                if (Math.Abs(dAzDiff) < 0.001)
                {
                    dLatout = dLatsec2 / 3600.0;
                    dLongout = dLongsec2 / 3600.0;
                    nRet = 0;
                    break;
                }
                else
                {
                    /*	Correct the azimuth and distance and try again */
                    dNewDist += dDistDiff;
                    dNewAz += dAzDiff;
                }
            }

            return (nRet);
        }

        /// <summary>
        /// This method calculates the latitude and longitude at a given distance and azimuth from 
        /// a prescribed reference point assuming a spherical earth and uses spherical trigronometry
        /// to compute 'exact' results (as opposed to a local 'flat earth' approximation).
        /// </summary>
        /// <remarks>
        /// All input and output angles are in decimal degrees. 
        /// </remarks>
        /// <param name="dLat"></param>
        /// <param name="dLong"></param>
        /// <param name="dDist"></param>
        /// <param name="dAz"></param>
        /// <param name="dLatOut"></param>
        /// <param name="dLongOut"></param>
        /// <returns></returns>
        public static int LLFromDistSE(double dLat,             /*  Latitude of the input coord   */
                                        double dLong,           /*  Longitude of the input coord  */
                                        double dDist,           /*  distance from the pt in km    */
                                        double dAz,             /*  Azimuth in decimal degrees    */
                                        out double dLatOut,     /*  Resultant Latitude of point   */
                                        out double dLongOut)    /*  Resultant Longitude of point  */
        {
            double dDeltaLong;                  /*  Difference in longitude               */
            double dDistDeg = (dDist / 111.12); /*  Distance in degrees       */
            double dCoLat = 90.0 - dLat;        /*  Only good for northern Hemi?  */
            double dCoLatOut;

            dCoLatOut = SphSide(dCoLat, dDistDeg, dAz);
            dDeltaLong = SphAngle(dDistDeg, dAz, dCoLatOut);

            /*  Now get the actual coordinates from the coLatitude and the difference
                in Longitude */
            dLatOut = 90.0 - dCoLatOut;
            dLongOut = dLong - dDeltaLong;    /* Longitude is reversed in this system
                                        in that West is positive */

            return 0;
        }

        /// <summary>
        /// This method returns the spherical trigonometric calculation of the geodesic 
        /// length of a side of a triangle (whose 3 vertices lay on a spherical earth) 
        /// given the geodesic length of the other two sides and the opposite angle 
        /// subtended between them; angle given in decimal degrees.
        /// </summary>
        /// <param name="dOpside1"></param>
        /// <param name="dOpside2"></param>
        /// <param name="dOpangle"></param>
        /// <returns></returns>
        public static double SphSide(double dOpside1, double dOpside2, double dOpangle)
        {
            double dRet;
            double dSide1R = dOpside1 * Constant.DEG_TO_RAD;
            double dSide2R = dOpside2 * Constant.DEG_TO_RAD;
            double dOppAng = dOpangle * Constant.DEG_TO_RAD;

            /*  Calculate the cosine of the arc.  This will be negative if the
                angle is > 90, but symetrical around 0 */

            dRet = Math.Acos(Math.Cos(dSide1R) * Math.Cos(dSide2R) +
                        Math.Sin(dSide1R) * Math.Sin(dSide2R) * Math.Cos(dOppAng));

            dRet *= Constant.RAD_TO_DEG;

            return dRet;
        }

        /// <summary>
        /// This method returns the spherical trigonometric calculation of the angle 
        /// of a triangle (whose 3 vertices lay on a spherical earth) given the geodesic 
        /// length of the opposite side, the length of an adjacent side, and the angle 
        /// opposite the adjacent side; all angles given in decimal degrees.
        /// </summary>
        /// <remarks>
        /// The calculation is the familiar 'Sine Rule'.
        /// </remarks>
        /// <param name="dOpside"></param>
        /// <param name="dAdjAngle"></param>
        /// <param name="dAdjSide"></param>
        /// <returns></returns>
        public static double SphAngle(double dOpside, double dAdjAngle, double dAdjSide)
        {
            double dRet;
            double dSideOpp = dOpside * Constant.DEG_TO_RAD;
            double dAnglAdj = dAdjAngle * Constant.DEG_TO_RAD;
            double dSideAdj = dAdjSide * Constant.DEG_TO_RAD;

            if (Math.Abs(dSideAdj) < 0.000001)
            {
                dRet = 0.00;
            }
            else
            {
                dRet = Math.Asin(Math.Sin(dSideOpp) * Math.Sin(dAnglAdj) / Math.Sin(dSideAdj));
            }
            dRet *= Constant.RAD_TO_DEG;

            return dRet;
        }


    }
}
