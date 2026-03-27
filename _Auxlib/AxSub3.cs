using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _Auxlib
{
    /// <summary>
    /// Provides methods to calculate the distance and angle of elevation between two antennae.
    /// </summary>
    public class AxSub3
    {
#if PINVOKE
        [DllImport("auxlib.dll", CharSet = CharSet.Ansi)]
        private extern static void axElev(double antHt1Amsl, double antHt2Amsl, double distanceKm, [In, Out] ref double elevAng12, [In, Out] ref double elevAng21);
        [DllImport("auxlib.dll", CharSet = CharSet.Ansi)]
        private extern static double pathdist(double antht1Km, double antht2Km, double distKm);

        public static double Pathdist_NATIVE(double antht1Km, double antht2Km, double distKm)
        {
            return pathdist(antht1Km, antht2Km, distKm);
        }

        public static void AxElev_NATIVE(double antHt1Amsl, double antHt2Amsl, double distanceKm, ref double elevAng12, ref double elevAng21)
        {
            axElev(antHt1Amsl, antHt2Amsl, distanceKm, ref elevAng12, ref elevAng21);
        }
#endif

        /// <summary>
        /// Calculates the angle of elevation between two stations.
        /// </summary>
        /// <param name="antHt1Amsl"> - height of station 1 antenna above sea level.</param>
        /// <param name="antHt2Amsl"> - height of station 2 antenna above sea level.</param>
        /// <param name="distanceKm"> - distance between the two stations.</param>
        /// <param name="elevAng12"> - elevation angle from station 1 to station 2.</param>
        /// <param name="elevAng21"> - elevation angle from station 2 to station 1.</param>
        public static void AxElev(double antHt1Amsl,  /* input  - ht of stn1 antenna ASL km				*/
                        double antHt2Amsl,  /* input  - ht of stn2 antenna ASL km				*/
                        double distanceKm,  /* input  - distance between stations 			*/
                        out double elevAng12,  /* output - elevation angle from stn1 to 2 	*/
                        out double elevAng21)  /* output - elevation angle from stn2 to 1 	*/
        {
            double temp1, temp2;   /* temporary working variables */

            /* calcs done in radians  then converted to degrees */
            temp1 = (distanceKm / Constant.ERTHRD) / (2.0 * Constant.APP_EARTH_RAD_FACT);
            temp2 = Math.Atan((antHt2Amsl - antHt1Amsl) / ((Constant.ERTHRD * 2.0 *
                     Constant.APP_EARTH_RAD_FACT + antHt1Amsl + antHt2Amsl) * Math.Tan(temp1)));

            //...Log2.v(String.Format("\n{0}   {1}   {2}   {3}   {4}", antHt1Amsl, antHt2Amsl, distanceKm, temp1, temp2));

            /* convert to degrees */
            elevAng12 = (temp2 - temp1) * Constant.RAD_TO_DEG;
            elevAng21 = (-temp2 - temp1) * Constant.RAD_TO_DEG;
        }

        /// <summary>
        /// Calculates the actual path distance between two antenae (through the air, as opposed to along the ground)
        /// using the triangle formed by the antennae and the centre of the earth.
        /// </summary>
        /// <param name="antht1Km"> - height of antenna 1.</param>
        /// <param name="antht2Km"> - height of antenna 2.</param>
        /// <param name="distKm"> - distance along the ellipsoid between the bases of antennae 1 and 2.</param>
        /// <returns> - calculated through-air path distance.</returns>
        public static double PathDist(double antht1Km,    /* Antenna height 1 - km amsl */
                                        double antht2Km,    /* Antenna height 2 - km amsl */
                                        double distKm)      /* Distance on surface - km   */
        {
            double R1 = Constant.ERTHRD + antht1Km;
            double R2 = Constant.ERTHRD + antht2Km;
            double dPath;

            if (distKm <= 0.001)
            {
                /*	For co-located sites we use a default 1m distance in all cases */
                dPath = 0.001;
            }
            else
            {
                dPath = Math.Sqrt(R1 * R1 + R2 * R2 - 2.0 * R1 * R2 * Math.Cos(distKm / Constant.ERTHRD));
            }
            return (dPath);
        }





    }
}
