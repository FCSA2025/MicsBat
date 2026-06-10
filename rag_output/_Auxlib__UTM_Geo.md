# Documented File: UTM_Geo.cs
**Repository Path:** `_Auxlib\UTM_Geo.cs`
**Primary Layer:** `_Auxlib`
**Namespace:** `_Auxlib`

## Source Code Representation
```csharp
﻿using _Configuration;
using _NewLib;
using _OHloss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace _Auxlib
{
    public class UTM_Geo
    {
        /// <summary>
        /// This method provides a non-proprietary alternative to the method LatLng_to_UTM() that
        /// is provided by _OHloss.dll (using the 3rd party 'C' static library provided
        /// by CTE); 
        /// </summary>
        /// <param name="gxfer"></param>
        public static void LatLng_to_UTM_Alternate(ref Geo_UTM_Xfer gxfer)
        {
            double A, F, lat_deg, lng_deg;
            double FE = 500000.0,
                   OK = .9996;
            double recf,
                   B,
                   ES,
                   EBS,
                   TN, AP, BP,
                   CP, DP, EP,
                   OLAM,
                   DLAM,
                   S, C, T, ETA,
                   SN,
                   TMD,
                   nfn,
                   lat_rad,
                   T1, T2, T3, T4, T5, T6, T7, T8, T9;

            lat_deg = gxfer.latitude;
            lng_deg = gxfer.longitude;

            if (gxfer.datum == (short)Enums.Datum.NAD_83_DATUM)
            {
                A = 6378137.0;
                F = 298.257222101;
            }
            else
            {
                A = 6378206.4;
                F = 294.9786982;
            }

            lng_deg *= -1.0;

            recf = F;
            B = A * (recf - 1.0) / recf;
            ES = (A * A - B * B) / A / A;
            EBS = (A * A - B * B) / B / B;

            TN = (A - B) / (A + B);
            AP = A * (1.0 - TN + 5.0 * (Pow(TN, 2.0) - Pow(TN, 3.0)) / 4.0 + 81.0 * (Pow(TN, 4.0) - Pow(TN, 5.0)) / 64.0);
            BP = 3.0 * A * (TN - Pow(TN, 2.0) + 7.0 * (Pow(TN, 3.0) - Pow(TN, 4.0)) / 8.0 + 55.0 * Pow(TN, 5.0) / 64.0) / 2.0;
            CP = 15.0 * A * (Pow(TN, 2.0) - Pow(TN, 3.0) + 3.0 * (Pow(TN, 4.0) - Pow(TN, 5.0)) / 4.0) / 16.0;
            DP = 35.0 * A * (Pow(TN, 3.0) - Pow(TN, 4.0) + 11.0 * Pow(TN, 5.0) / 16.0) / 48.0;
            EP = 315.0 * A * (Pow(TN, 4.0) - Pow(TN, 5.0)) / 512.0;

            gxfer.zone = (short)(31 + (short)Floor(lng_deg / 6.0));
            if (gxfer.zone > 60)
                gxfer.zone = 60;
            if (gxfer.zone < 1)
                gxfer.zone = 1;

            OLAM = ((double)(gxfer.zone * 6) - 183.0);

            DLAM = (lng_deg - OLAM) / Constant.RAD_TO_DEG;

            lat_rad = lat_deg / Constant.RAD_TO_DEG;
            S = Sin(lat_rad);
            C = Cos(lat_rad);
            T = S / C;
            ETA = EBS * Pow(C, 2.0);

            SN = A / Sqrt(1.0 - ES * Pow(Sin(lat_rad), 2.0));

            TMD = AP * lat_rad - BP * Sin(2.0 * lat_rad) +
                  CP * Sin(4.0 * lat_rad) - DP * Sin(6.0 * lat_rad) + EP * Sin(8.0 * lat_rad);

            T1 = TMD * OK;
            T2 = SN * S * C * OK / 2.0;
            T3 = SN * S * Pow(C, 3.0) * OK * (5.0 - Pow(T, 2.0) + 9.0 * ETA + 4.0 * Pow(ETA, 2.0)) / 24.0;
            T4 = SN * S * Pow(C, 5.0) * OK * (61.0 -
                  58.0 * Pow(T, 2.0) + Pow(T, 4.0) +
                 270.0 * ETA -
                 330.0 * Pow(T, 2.0) * ETA +
                 445.0 * Pow(ETA, 2.0) +
                 324.0 * Pow(ETA, 3.0) -
                 680.0 * Pow(T, 2.0) * Pow(ETA, 2.0) +
                 88.0 * Pow(ETA, 4.0) -
                 600.0 * Pow(T, 2.0) * Pow(ETA, 3.0) -
                 192.0 * Pow(T, 2.0) * Pow(ETA, 4.0)) / 720.0;
            T5 = SN * S * Pow(C, 7.0) * OK * (
                 1385.0 -
                 3111.0 * Pow(T, 2.0) +
                 543.0 * Pow(T, 4.0) -
                 Pow(T, 6.0)) / 40320.0;

            nfn = 0.0;
            if (lat_rad < 0.0)
                nfn = 10000000.0;

            gxfer.northing = nfn + T1 + Pow(DLAM, 2.0) * T2 +
                              Pow(DLAM, 4.0) * T3 + Pow(DLAM, 6.0) * T4 + Pow(DLAM, 8.0) * T5;

            T6 = SN * C * OK;
            T7 = SN * Pow(C, 3.0) * OK * (1.0 - Pow(T, 2.0) + ETA) / 6.0;
            T8 = SN * Pow(C, 5.0) * OK * (
                 5.0 -
                 18.0 * Pow(T, 2.0) +
                 Pow(T, 4.0) +
                 14.0 * ETA -
                 58.0 * Pow(T, 2.0) * ETA +
                 13.0 * Pow(ETA, 2.0) +
                 4.0 * Pow(ETA, 3.0) -
                 64.0 * Pow(T, 2.0) * Pow(ETA, 2.0) -
                 24.0 * Pow(T, 2.0) * Pow(ETA, 3.0)) / 120.0;
            T9 = SN * Pow(C, 7.0) * OK * (
                 61.0 -
                 479.0 * Pow(T, 2.0) +
                 179.0 * Pow(T, 4.0) -
                 Pow(T, 6.0)) / 5040.0;

            gxfer.easting = FE + DLAM * T6 + Pow(DLAM, 3.0) * T7 + Pow(DLAM, 5.0) * T8 + Pow(DLAM, 7.0) * T9;
        }

        /// <summary>
        /// This method converts NAD 83 (lat, long) to UTM (easting, northing, zone); 
        /// the 3rd party CTE library (and hence _OHloss.dll) contains a function
        /// of the same name and this method is an alternative to that provided by CTE.
        /// </summary>
        /// <param name="nLata83"> - NAD 83 latitude.</param>
        /// <param name="nLonga83"> - NAD 83 longitude.</param>
        /// <param name="dUTMNorth"> - UTM northing (m)</param>
        /// <param name="dUTMEast"> - UTM easting (m)</param>
        /// <param name="nZone"> - UTM zone.</param>
        /// <returns></returns>
        public static int GetUTMfrom83_Alternate(int nLata83, int nLonga83, out double dUTMNorth, out double dUTMEast, out short nZone)
        {
            Geo_UTM_Xfer tutm = new Geo_UTM_Xfer();

            tutm.datum = (short)Enums.Datum.NAD_83_DATUM;
            tutm.latitude = nLata83 / 360000.0;
            tutm.longitude = nLonga83 / 360000.0;

            LatLng_to_UTM_Alternate(ref tutm);

            dUTMNorth = tutm.northing;
            dUTMEast = tutm.easting;
            nZone = tutm.zone;

            return (0);
        }




    }
}

```
