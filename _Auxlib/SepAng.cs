using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace _Auxlib
{
    /// <summary>
    /// Provides a method that calculates the separation angle between 
    /// a vector from an Earth Station (ES) to a satellite and the 
    /// vector from an Earth Station to a Terrestrial Station (TS).
    /// </summary>
    public class SepAng
    {
        /// <summary>
        /// Calculates the separation angle between a vector from an Earth 
        /// station to a satellite and the vector from an Earth station to terrestrial 
        /// station.  
        /// </summary>
        /// <param name="EtoSazim"> - earth stn to satellite azimuth</param>
        /// <param name="EtoSelev"> - earth stn to satellite elevation</param>
        /// <param name="EtoEazim"> - earth stn1 to terr stn2 azimuth</param>
        /// <param name="EtoEelev"> - earth stn1 to terr stn2 elevation</param>
        /// <param name="intersectAng"> - separation angle</param>
        public static void AxSepAng(double EtoSazim,      /* input  - earth to satellite azim */
                  double EtoSelev,      /* input  - earth to satellite elev */
                  double EtoEazim,      /* input  - earth to terr stn azim */
                  double EtoEelev,      /* input  - earth to terr stn elev */
                  out double intersectAng) /* output - separation angle */
        {
            double varW,        /* internal work variable */
                varX,       /* internal work variable */
                varY,       /* internal work variable */
                varZ,       /* internal work variable */
                azimAng1Rad,    /* EtoSazim in radians - earth to satellite */
                azimAng2Rad,    /* EtoEazim in radians - earth 1 to earth 2 */
                elevAng1Rad,    /* EtoSelev in radians - earth to satellite */
                elevAng2Rad;    /* EtoSelev in radians - earth 1 to earth 2 */

            /* convert angles from degrees to radians */
            azimAng1Rad = EtoSazim * Constant.DEG_TO_RAD;
            elevAng1Rad = EtoSelev * Constant.DEG_TO_RAD;
            azimAng2Rad = EtoEazim * Constant.DEG_TO_RAD;
            elevAng2Rad = EtoEelev * Constant.DEG_TO_RAD;

            varW = Cos(azimAng2Rad - azimAng1Rad) * Cos(elevAng2Rad) *
            Cos(elevAng1Rad) + Sin(elevAng2Rad) * Sin(elevAng1Rad);
            varX = 1 - Pow(varW, Constant.SQUARE);
            varY = (varX <= 0.0) ? -Sqrt(-varX) : Sqrt(varX);
            varZ = (varW == 0.0) ? 0.0 : varY / varW;

            intersectAng = Atan(varZ) * Constant.RAD_TO_DEG;

            /* be sure resulting angle is positive */
            if (intersectAng < 0.0)
            {
                intersectAng += 180;
            }
        }






    }
}
