# Documented File: AxInvPos.cs
**Repository Path:** `_Auxlib\AxInvPos.cs`
**Primary Layer:** `_Auxlib`
**Namespace:** `_Auxlib`

## Source Code Representation
```csharp
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
    /// Provides a method that computes, between any two antennae, the 
    /// the great circle distance, the bearings of each point to the 
    /// other, and the elevation angle from each antenna to the other. 
    /// </summary>
    public class AxInvPos
    {

        /// <summary>
        /// This method is used to compute, between any two antennae, the 
        /// the great circle distance, the bearings of each point to the 
        /// other, and the elevation angle from each antenna to the other. 
        /// </summary>
        /// <param name="distUnit"> - distance unit, M or F</param>
        /// <param name="s1"> - structure of information for station 1</param>
        /// <param name="s2"> - structure of information for station 2 Each structure contains the location of the station in degrees, minutes, seconds, and sense (N,S,E,W), as well as the station name, elevation above sea level, and ante height</param>
        /// <param name="distKm"> - stn1 to stn2 distance in KM</param>
        /// <param name="bearing12"> - bearing from stn 1 to stn 2</param>
        /// <param name="bearing21"> - bearing from stn 2 to stn 1</param>
        /// <param name="elevAng12"> - elevation angle from stn 1 to stn 2</param>
        /// <param name="elevAng21"> - elevation angle from stn 1 to stn 2</param>
        public static void AxInvpos(string distUnit,		/* input  - distance unit */
                                    AxStation s1,           /* input  - station 1 data */
                                    AxStation s2, 	        /* input  - station 2 data */

                                    out double distKm,		/* output - dist between stns */
                                    out double bearing12,	/* output - bearing from 1 to 2 */
                                    out double bearing21,	/* output - bearing from 2 to 1 */
                                    out double elevAng12,	/* output - elev from 1 to 2 */
                                    out double elevAng21)   /* output - elev from 2 to 1 */
        {

            double antHt1Aksl;      /* ant ht above sea level for stn1  */
            double antHt2Aksl;      /* ant ht above sea level for stn2  */

            /* convert all distances to meters */
            if (distUnit.Equals(Constant.FEET))
            {
                s1.elevM *= Constant.FT_TO_METERS;
                s1.antHtM *= Constant.FT_TO_METERS;
                s2.elevM *= Constant.FT_TO_METERS;
                s2.antHtM *= Constant.FT_TO_METERS;
            }

            /* convert degrees to seconds */
            AxSub1.AxDsconv(ref s1.LL);

            AxSub1.AxDsconv(ref s2.LL);

            /* find the distance and bearings for stn1 to stn2 and stn2 to stn1 */
            AxSub2.AxDistan(s1.LL.latSeconds, s2.LL.latSeconds, s1.LL.longSeconds,
                s2.LL.longSeconds, out distKm, out bearing12, out bearing21);

            /* heights in Km */
            antHt1Aksl = (s1.elevM + s1.antHtM) * 0.001;  // / 1000.0;
            antHt2Aksl = (s2.elevM + s2.antHtM) * 0.001;  // / 1000.0;

            /* find the elevations from stn1 to stn2 and stn2 to stn1 */
            AxSub3.AxElev(antHt1Aksl, antHt2Aksl, distKm, out elevAng12, out elevAng21);
        }




    }
}

```
