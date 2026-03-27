using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace _Auxlib
{
    /// <summary>
    /// Provides a method that calculates the horizon elevation loss in dB.
    /// </summary>
    public class HorLoss
    {
        /// <summary>
        /// Calculates the horizon elevation loss in dB.  
        /// </summary>
        /// <param name="frequency"> - operating frequency in GHz</param>
        /// <param name="elevationAng"> - elevation ang of the horizon</param>
        /// <param name="lossHor"> - horizon elevation loss in dB</param>
        public static void AxHorLoss(double frequency,            /* input  - frequency in GHz */
                                     double elevationAng,   /* input  - elevation angle */
                                     out double lossHor)               /* output - horizon loss */
        {
            // 'out' requirement.
            lossHor = 0.0;

            if (elevationAng > 0.0)
            {
                /* calculation for positive elevation angles */
                lossHor = 20.0 * Log10(1.0 + (4.5 * Sqrt(frequency) *
                                     elevationAng)) + (Pow(frequency, 1.0 / 3.0) *
                             elevationAng);
            }
            else
            {
                /* calculations for negative and zero elevation angles */
                lossHor = (elevationAng > -0.5) ? 8.0 * elevationAng : -4.0;

            }
            lossHor = (lossHor > 30.0) ? 30.0 : lossHor;
        }




    }
}
