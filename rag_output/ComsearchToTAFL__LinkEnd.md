# Documented File: LinkEnd.cs
**Repository Path:** `ComsearchToTAFL\LinkEnd.cs`
**Primary Layer:** `ComsearchToTAFL`
**Namespace:** `ComsearchToTAFL`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComsearchToTAFL
{
    using _Auxlib;
    using _Configuration;
    using SQLLEN = Int64;

    /// <summary>
    /// This class encapsulates the data fields that define one 'end' of a 
    /// Comsearch record; a Comsearch record can always be decomposed into
    /// two LinkEnd objects ('local' and 'remote') and a set of fields that 
    /// are common to both.
    /// </summary>
    public class LinkEnd
    {
        public string site = "";
        public string state = "";
        public double lat = double.MinValue;
        public double lng = double.MinValue;
        public double grndElev = double.MinValue;
        public string antMfr = "";
        public string antMod = "";
        public double gain = double.MinValue;
        public double bw = double.MinValue;
        public double rcagl = double.MinValue;
        public string eqptMfr = "";
        public string eqptMod = "";
        public string eqptModDesc = "";
        public string emDes = "";
        public string mod = "";
        public string acmMinMod = "";
        public string acmMaxMod = "";
        public double dataRate = double.MinValue;       // Units: Mbit/s
        public double atpcNomPwr = double.MinValue;     // Units: dBm
        public double pwr = double.MinValue;            // Units: dBm
        public double atpcMaxPwr = double.MinValue;     // Units: dBm
        public double acmMinModPwr = double.MinValue;   // Units: dBm
        public double acmMaxModPwr = double.MinValue;   // Units: dBm
        public double txLoss = double.MinValue;
        public double rxLoss = double.MinValue;
        public double cmnLoss = double.MinValue;
        public double atpcNomRSL = double.MinValue;
        public double selectRSL = double.MinValue;
        public double atpcMaxRSL = double.MinValue;
        public double acmMinModRSL = double.MinValue;
        public double acmMaxModRSL = double.MinValue;
        public double centerFreq = double.MinValue;     // Units: MHz
        public string polar = "";

        public SQLLEN[] nullInds;

        public const int NUM_COLUMNS = 33;

        public const int SITE = 0;
        public const int STATE = 1;
        public const int LAT = 2;
        public const int LNG = 3;
        public const int GRNDELEV = 4;
        public const int ANTMFR = 5;
        public const int ANTMOD = 6;
        public const int GAIN = 7;
        public const int BW = 8;
        public const int RCAGL = 9;
        public const int EQPTMFR = 10;
        public const int EQPTMOD = 11;
        public const int EQPTMODDESC = 12;
        public const int EMDES = 13;
        public const int MOD = 14;
        public const int ACMMINMOD = 15;
        public const int ACMMAXMOD = 16;
        public const int DATARATE = 17;
        public const int ATPCNOMPWR = 18;
        public const int PWR = 19;
        public const int ATPCMAXPWR = 20;
        public const int ACMMINMODPWR = 21;
        public const int ACMMAXMODPWR = 22;
        public const int TXLOSS = 23;
        public const int RXLOSS = 24;
        public const int CMNLOSS = 25;
        public const int ATPCNOMRSL = 26;
        public const int SELECTRSL = 27;
        public const int ATPCMAXRSL = 28;
        public const int ACMMINMODRSL = 29;
        public const int ACMMAXMODRSL = 30;
        public const int CENTERFREQ = 31;
        public const int POLAR = 32;

        /// <summary>
        /// A public constructor.
        /// </summary>
        public LinkEnd()
        {
            nullInds = new SQLLEN[NUM_COLUMNS];

            for (int i = 0; i < NUM_COLUMNS - 1; i++)
            {
                nullInds[i] = Constant.DB_NULL;
            }
        }

        /// <summary>
        /// This method returns a string that lists the field values
        /// of this instance of a LinkEnd object together with its field null
        /// indicators. 
        /// </summary>
        /// <returns></returns>
        public string ToStringWN()
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();

            sb.Append("" + nullInds[i++] + "      " + "site = " + site);
            sb.Append("\n" + nullInds[i++] + "      " + "state = " + state);
            sb.Append("\n" + nullInds[i++] + "      " + "lat = " + lat);
            sb.Append("\n" + nullInds[i++] + "      " + "lng = " + lng);
            sb.Append("\n" + nullInds[i++] + "      " + "grndElev = " + grndElev);
            sb.Append("\n" + nullInds[i++] + "      " + "antMfr = " + antMfr);
            sb.Append("\n" + nullInds[i++] + "      " + "antMod = " + antMod);
            sb.Append("\n" + nullInds[i++] + "      " + "gain = " + gain);
            sb.Append("\n" + nullInds[i++] + "      " + "bw = " + bw);
            sb.Append("\n" + nullInds[i++] + "      " + "rcagl = " + rcagl);
            sb.Append("\n" + nullInds[i++] + "      " + "eqptMfr = " + eqptMfr);
            sb.Append("\n" + nullInds[i++] + "      " + "eqptMod = " + eqptMod);
            sb.Append("\n" + nullInds[i++] + "      " + "eqptModDesc = " + eqptModDesc);
            sb.Append("\n" + nullInds[i++] + "      " + "emDes = " + emDes);
            sb.Append("\n" + nullInds[i++] + "      " + "mod = " + mod);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMinMod = " + acmMinMod);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMaxMod = " + acmMaxMod);
            sb.Append("\n" + nullInds[i++] + "      " + "dataRate = " + dataRate);
            sb.Append("\n" + nullInds[i++] + "      " + "atpcNomPwr = " + atpcNomPwr);
            sb.Append("\n" + nullInds[i++] + "      " + "pwr = " + pwr);
            sb.Append("\n" + nullInds[i++] + "      " + "atpcMaxPwr = " + atpcMaxPwr);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMinModPwr = " + acmMinModPwr);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMaxModPwr = " + acmMaxModPwr);
            sb.Append("\n" + nullInds[i++] + "      " + "txLoss = " + txLoss);
            sb.Append("\n" + nullInds[i++] + "      " + "rxLoss = " + rxLoss);
            sb.Append("\n" + nullInds[i++] + "      " + "cmnLoss = " + cmnLoss);
            sb.Append("\n" + nullInds[i++] + "      " + "atpcNomRSL = " + atpcNomRSL);
            sb.Append("\n" + nullInds[i++] + "      " + "selectRSL = " + selectRSL);
            sb.Append("\n" + nullInds[i++] + "      " + "atpcMaxRSL = " + atpcMaxRSL);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMinModRSL = " + acmMinModRSL);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMaxModRSL = " + acmMaxModRSL);
            sb.Append("\n" + nullInds[i++] + "      " + "centerFreq = " + centerFreq);
            sb.Append("\n" + nullInds[i++] + "      " + "polar = " + polar);

            return sb.ToString();
        }

        /// <summary>
        /// This method inputs local and remote LinkEnd objects and calculates distance, and the bearing and elevation angles, from
        /// the local location to the remote location based on a reference model of the Earth as an oblate spheroid.
        /// </summary>
        /// <param name="localEnd"></param>
        /// <param name="remoteEnd"></param>
        /// <param name="pathDistKm"></param>
        /// <param name="azimuthDeg"></param>
        /// <param name="vertElevationDeg"></param>
        public static void RelativeGeometry(LinkEnd localEnd, LinkEnd remoteEnd, out double pathDistKm, out double azimuthDeg, out double vertElevationDeg)
        {
            // 'out' requirement.
            pathDistKm = float.MinValue;
            azimuthDeg = float.MinValue;
            vertElevationDeg = float.MinValue;

            // Prepare to call AxDistan; latitudes and longitudes must be in seconds of arc.
            double latStn1Sec = localEnd.lat * 3600;
            double latStn2Sec = remoteEnd.lat * 3600;

            double longStn1Sec = -localEnd.lng * 3600;   // Comsearch longitudes are -ve.
            double longStn2Sec = -remoteEnd.lng * 3600;  // Comsearch longitudes are -ve.

            double surfaceDistKm;
            double otherEndAzimuth;

            // Calculate absolute height in Kms of local and remote antennas above MSL.
            double localHeightAboveMSL = (localEnd.grndElev + localEnd.rcagl) / 1000.0;
            double remoteHeightAboveMSL = (remoteEnd.grndElev + remoteEnd.rcagl) / 1000.0;

            // Calculate surface distances and azimuths.
            AxSub2.AxDistan(latStn1Sec, latStn2Sec, longStn1Sec, longStn2Sec, out surfaceDistKm, out azimuthDeg, out otherEndAzimuth);

            // Calculate the actual path distance between two antenae (through the air, as opposed to along the ground)
            // using the triangle formed by the antennae and the centre of the earth.
            pathDistKm = AxSub3.PathDist(localHeightAboveMSL, remoteHeightAboveMSL, surfaceDistKm);

            // Calculate the antenna elevations for the local and remote antennas.
            double elevAng21;
            AxSub3.AxElev(localHeightAboveMSL, remoteHeightAboveMSL, pathDistKm, out vertElevationDeg, out elevAng21);

            //...Log2.v("\nTools.RelativeGeometry(): {0}, {1}, \n{2}, {3}, \n{4}, {5}, {6}, \n{7}, {8}, {9}, {10}", latStn1Sec, longStn1Sec, latStn2Sec, longStn2Sec, pathDistKm, azimuthDeg, otherEndAzimuth, vertElevationDeg, elevAng21, localHeightAboveMSL, remoteHeightAboveMSL);
        }

    }
}

```
