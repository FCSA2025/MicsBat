using _Configuration;
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Auxlib
{
    /// <summary>
    /// Provides a method that calculates latitude and longitude degrees in seconds for an
    /// AxLocation object. 
    /// </summary>
    public class AxSub1
    {
        /// <summary>
        /// Calculates latitude and longitude degrees in seconds for an
        /// AxLocation object.  
        /// </summary>
        /// <param name="statn"></param>
        public static void AxDsconv(ref AxLocation statn)
        {
            /* latSeconds = (degrees*60 + minutes) * 60 + seconds */
            statn.latSeconds = (statn.latDeg * Constant.DEG_MIN_SEC + statn.latMin) *
                         Constant.DEG_MIN_SEC + statn.latSec;

            /* Negitive output if latitude sens is South, positive if North */
            if (statn.latSens.Equals(Constant.SOUTH))
            {
                statn.latSeconds *= -1.0;
            }

            /* longSeconds = (degrees*60 + minutes) * 60 + seconds */
            statn.longSeconds = (statn.longDeg * Constant.DEG_MIN_SEC + statn.longMin) *

                          Constant.DEG_MIN_SEC + statn.longSec;

            /* Negitive output if longitude sens is East, positive if West */
            if (statn.longSens.Equals(Constant.EAST))
            {
                statn.longSeconds *= -1.0;
            }
        }





    }
}
