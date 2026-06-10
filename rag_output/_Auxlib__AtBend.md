# Documented File: AtBend.cs
**Repository Path:** `_Auxlib\AtBend.cs`
**Primary Layer:** `_Auxlib`
**Namespace:** `_Auxlib`

## Source Code Representation
```csharp
using System;
using static System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _Configuration;

namespace _Auxlib
{
    /// <summary>
    /// Provides methods that calculate atmospheric refraction.
    /// </summary>
    public class AtBend
    {
        /// <summary>
        /// This method is a portal for calculating a ray's refraction 
        /// angle in degrees. Depending on 
        /// the geometric elevation angle, it calls either AtBend() or CoTanBend()
        /// to actually perform the calculations.  
        /// </summary>
        /// <param name="dElev"> - Geometric Elevation Angle in degrees</param>
        /// <param name="nLoHi"> - 1 - N0 = 400, 2 - N0 = 250</param>
        /// <param name="dH"> - Height (AMSL) of the antenna in Km</param>
        /// <param name="dTau"> - Output refraction angle in degrees</param>
        /// <returns></returns>
        public static int Refrac(double dElev,   /*	Geometric Elevation Angle in degrees */
                                int nLoHi,      /*	1 - N0 = 400, 2 - N0 = 250 */
                                double dH,      /*	Height (AMSL) of the antenna in Km */
                                out double dTau)           /*	Output refraction angle in degrees */
        {
            if (dElev >= 9.0)
            {
                return CoTanBend(dElev, nLoHi, dH, out dTau);
            }
            else
            {
                return Atbend(dElev, dH, nLoHi, out dTau);
            }
        }

        /// <summary>
        /// Calculates the co-tangent approximation to the refraction angle for higher 
        /// beam elevations. This is taken from Charlie Felter's paper "Calculation of 
        /// the Refracted Orbit Trace Using Approximation Function, (May 1977)".  
        /// </summary>
        /// <param name="dElev"> - Geometric Elevation Angle in degrees</param>
        /// <param name="nLoHi"> - 1 - N0 = 400, 2 - N0 = 250</param>
        /// <param name="dH"> - Height (AMSL) of the antenna in Km</param>
        /// <param name="dTau"> - Output refraction angle in degrees</param>
        /// <returns></returns>
        public static int CoTanBend(double dElev,     /*	Geometric Elevation Angle in degrees */
                            int nLoHi,      /*	1 - N0 = 400, 2 - N0 = 250 */
                            double dH,              /*	Height (AMSL) of the antenna in Km */
                            out double dTau)           /*	Output refraction angle in degrees */
        {
            double dNs = Exp(-dH / 7.0);
            double dcotan;

            /*	Error checking.  The angle must be greater than 8 degrees and the refraction
            *		coefficient selector must be 1 or 2.0 */
            if (dElev < 8.0)
            {
                /*	The angle is too low */
                dTau = 0.0;
                return Error.ELEVANGLETOOLOW;
            }

            switch (nLoHi)
            {
                case 1:         /*	N0 = 400 */
                    dNs *= 400.0;
                    break;

                case 2:         /*	N0 = 250 */
                    dNs *= 250.0;
                    break;

                default:
                    dTau = 0;
                    return -1;
            }

            if (Abs(90.0 - dElev) < 0.1)
            {
                dcotan = 0.0;
            }
            else
            {
                dcotan = 1.0 / Tan(dElev / Constant.RADTODEG);
            }

            dTau = Constant.RADTODEG * 1e-6 * dNs * dcotan;
            return 0;
        }

        /// <summary>
        /// Calculates the refraction for low elevation angles (less than or equal to 
        /// 8 degrees) using a polynomial approximation from Al Moreno (ITU Report 393-3).  
        /// </summary>
        /// <param name="dEps"> - The antenna elevation angle at the ground</param>
        /// <param name="dH"> - The height of the antenna above sea level _IN_KM_</param>
        /// <param name="nLoHi"> - 1 - for N0 = 400; 2 - for N0 = 250</param>
        /// <param name="dTau"> - The output refraction</param>
        /// <returns></returns>
        public static int Atbend(double dEps,         /*  The antenna elevation angle at the ground */
                     double dH,             /*	The height of the antenna above sea level _IN_KM_ */
                     int nLoHi,     /*	1 - for N0 = 400; 2 - for N0 = 250        */
                     out double dTau)          /*	The output refraction											*/
                                               /*
                                               *   Return value of 0 means calculation went okay,
                                               *										-2 H out of range
                                               *		Note that eps may be checked separately to be between the elevation of the local
                                               *		horizon and 8 degrees. See the IsEpsOK routine.
                                               */
        {
            int nRet = 0;

            /*	Check the values first */
            if ((dH < 0) || (dH > 4.0))
            {
                nRet = -2;
            }
            /*	Select the equation.  The maximum refraction is also the default */
            switch (nLoHi)
            {
                case 2:                 /*	N0 = 250 */
                    dTau = (1.755698 + 0.313461 * dH) +
                                    ((0.815022 + 0.109154 * dH) +
                                     dEps * (0.0295668 + 0.0185682 * dH)) * dEps;
                    dTau = 1.0 / dTau;
                    break;
                case 1:                 /*	N0 = 400 */
                default:                /*	Anything else */
                    dTau = (0.7885809 + (0.175963 + 0.0251620 * dH) * dH) +
                                    ((0.549056 + (0.0744484 + 0.0101650 * dH) * dH) +
                                    dEps * (0.0187029 + 0.0143814 * dH)) * dEps;
                    dTau = 1.0 / dTau;
                    break;
            }
            return nRet;
        }




    }
}

```
