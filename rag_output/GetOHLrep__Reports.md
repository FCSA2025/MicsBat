# Documented File: Reports.cs
**Repository Path:** `GetOHLrep\Reports.cs`
**Primary Layer:** `GetOHLrep`
**Namespace:** `GetOHLrep`

## Source Code Representation
```csharp
﻿using _Auxlib;
using _Configuration;
using _DataStructures;
using _NewLib;
using _OHloss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOHLrep
{
    /// <summary>
    /// This class provides methods that write GetOHLrep program output 'reports' to
    /// Console.Out; the supported report format types are PLAIN (simple text), HTML and CSV.
    /// </summary>
    public class Reports
    {
        /*	Arrays for the ohloss values */
        private static string[] aPol = new string[2] { "Horizontal", "Vertical" };

        private static string[] aClimate = new string[3] { "Continental Temperate", "Maritime Temperate Over Land", "Maritime Temperate Over Sea" };

        private static double[] dTimePct = new double[8] { 50.0, 80.0, 90.0, 99.0, 99.9, 99.99, 99.995, 99.9975 };


        /// <summary>
        /// This method manages the production of the PLAIN (simple text) and HTML
        /// output report format types.
        /// </summary>
        /// <param name="eRep"></param>
        /// <param name="p"></param>
        /// <param name="sctOHLoss"></param>
        public static void WritePlainOrHTML(Enums.eRepType eRep, Parameters p, OhLossXfer sctOHLoss)
        {
            /*	Produce the reports if the the table has been filled in.  They are
                        *		written to stdout, which dblogger has redirected to something it
                        *		can find. */

            Write.htm(eRep, Constant.ON);
            /*	Write the header  */
            Write.h(1, eRep, Constant.ON);
            Console.Write("Frequency Coordination System Association");
            Console.Write("\r\nOver Horizon Loss Report");

            Write.h(1, eRep, Constant.OFF);

            Write.h(3, eRep, Constant.ON);
            Console.Write("\r\nMicrowave Interference Calculation System\r\n");

            Write.h(3, eRep, Constant.OFF);

            Write.table(eRep, Constant.ON);

            Write.td(eRep);
            Console.Write("\r\nFrom:                                  ");

            Write.td(eRep);
            Console.Write("To:\r\n");

            Write.tr(eRep);

            Write.td(eRep);
            if (p.mSiteCallSignA.Length > 0)
            {
                Console.Write("\r\n{0,-38} ", p.mSiteCallSignA);
            }
            else
            {
                Console.Write("\r\n{0,38} ", " ");
            }

            Write.td(eRep);
            if (p.mSiteCallSignB.Length > 0)
            {
                Console.Write("{0,-38}", p.mSiteCallSignB);
            }
            else
            {
            }

            Write.tr(eRep);

            Write.td(eRep);
            if (p.mSiteCallSignA.Length > 0)
            {
                Console.Write("\r\n{0,-38} ", p.mSiteNameA);
            }
            else
            {
                Console.Write("\r\n{0,38} ", " ");
            }

            Write.td(eRep);
            if (p.mSiteCallSignB.Length > 0)
            {
                Console.Write("{0,-38}", p.mSiteNameB);
            }


            Write.tr(eRep);
            Console.Write("\r\nNAD 83:");

            Write.td(eRep);
            Console.Write(" {0,-12} {1,-13}     ", p.mLat83A, p.mLong83A);

            Write.td(eRep);
            Console.Write("{0,-12} {1,-13}", p.mLat83B, p.mLong87B);

            /*	UTM coordinates have to be calculated, as they are not fed in. */
            UTM_Geo.GetUTMfrom83_Alternate(p.nLata83, p.nLonga83, out p.utman, out p.utmae, out p.nutmzonea);

            UTM_Geo.GetUTMfrom83_Alternate(p.nLatb83, p.nLongb83, out p.utmbn, out p.utmbe, out p.nutmzoneb);

            Write.tr(eRep);
            Console.Write("\r\nUTM:..: ");

            Write.td(eRep);
            Console.Write("{0,-3}Z {1,6:F0}E {2,7:F0}N          ", p.nutmzonea, p.utmae, p.utman);

            Write.td(eRep);
            Console.Write("{0,-3}Z {1,6:F0}E {2,7:F0}N\r\n", p.nutmzoneb, p.utmbe, p.utmbn);

            Write.table(eRep, Constant.OFF);

            Write.br(eRep);
            Console.Write("Distance.....: {0,7:F2} km", p.fdist);

            Write.br(eRep);
            Console.Write("Bearing A->B.: {0,7:F2} degrees", p.fbearinga);

            Write.br(eRep);
            Console.Write("Bearing B->A.: {0,7:F2} degrees", p.fbearingb);

            Write.br(eRep);
            Console.Write("Antenna Ht(A): {0,5:F1}m       Median K: {1,7:F5}    Antenna Ht(B): {2,5:F1}m\r\n",
                            sctOHLoss.s1_anthght, sctOHLoss.K_median,
                            sctOHLoss.s2_anthght);
            Write.br(eRep);
            Console.Write("Frequency:... {0,8:F1}MHz  ", sctOHLoss.freq);

            Write.br(eRep);
            Console.Write("Polarization: {0}\r\n", aPol[sctOHLoss.polarization]);

            Write.br(eRep);
            Console.Write("Climate Zone: {0}\r\n", aClimate[sctOHLoss.clim_region]);

            Write.br(eRep);
            Console.Write("Horizon Distance from A...: {0,7:F2} km", sctOHLoss.dist_horiz1);

            Write.br(eRep);
            Console.Write("Horizon Distance from B...: {0,7:F2} km", sctOHLoss.dist_horiz2);

            Write.br(eRep);
            Console.Write("Horizon Elevation from A..: {0,7:F2} m", sctOHLoss.elev_horiz1);

            Write.br(eRep);
            Console.Write("Horizon Elevation from B..: {0,7:F2} m", sctOHLoss.elev_horiz2);

            Write.br(eRep);
            Console.Write("Horizon Angle from A......: {0,7:F2} deg.",
                            sctOHLoss.angl_horiz1 * 57.2957795131);

            Write.br(eRep);
            Console.Write("Horizon Angle from B......: {0,7:F2} deg.",
                            sctOHLoss.angl_horiz2 * 57.2957795131);

            Write.br(eRep);
            Console.Write("Effective Antenna Height A: {0,7:F2} m", sctOHLoss.s1_effhght);

            Write.br(eRep);
            Console.Write("Effective Antenna Height B: {0,7:F2} m", sctOHLoss.s2_effhght);

            Write.br(eRep);
            Console.Write("Effective Distance........: {0,7:F2} km", sctOHLoss.eff_dist);

            Write.br(eRep);
            Console.Write("Horizon Cross-over from A.: {0,7:F2} km", sctOHLoss.horiz_xover);

            Write.br(eRep);
            Console.Write("Reference Diffraction Loss: {0,7:F2} db", sctOHLoss.refdiff_loss);

            Write.br(eRep);
            Console.Write("Reference Scatter Loss....: {0,7:F2} db",
                            sctOHLoss.refscatt_loss);

            Write.br(eRep);
            Console.Write("Reference Combined Loss...: {0,7:F2} db", sctOHLoss.refcomb_loss);

            Write.br(eRep);
            Console.Write("Median Loss L(0.5)........: {0,7:F2} db", sctOHLoss.median_loss);

            Write.br(eRep);

            Console.Write("\r\nLosses:");


            Write.table(eRep, Constant.ON);
            Console.Write("\r\nPct of Time");

            Write.td(eRep);
            Console.Write("  50% Conf.");

            Write.td(eRep);
            Console.Write("  95% Conf.");
            for (int nInd = 0; nInd < 8; nInd++)
            {

                Write.tr(eRep);
                Console.Write("\r\n{0,10:F4}", dTimePct[nInd]);

                Write.td(eRep);
                Console.Write("{0,11:F1}", sctOHLoss.ohloss_50[nInd]);

                Write.td(eRep);
                Console.Write("{0,11:F1}", sctOHLoss.ohloss_95[nInd]);
            }

            Write.table(eRep, Constant.OFF);

            Console.Write("\r\n\r\nCalculation Type..........: ");

            switch ((Enums.OHLcalcType)sctOHLoss.calc_type)
            {
                case Enums.OHLcalcType.OHL_LOS:
                    Console.Write("Line of Sight");
                    break;

                case Enums.OHLcalcType.OHL_SKE:
                    Console.Write("Single Knife Edge");
                    break;

                case Enums.OHLcalcType.OHL_ISOL:
                    Console.Write("Rounded Isolated Obstacle");
                    break;

                case Enums.OHLcalcType.OHL_DKE:
                    Console.Write("Double Knife Edge");
                    break;

                case Enums.OHLcalcType.OHL_IRT:
                    Console.Write("Irregular Terrain");
                    break;

                default:
                    Console.Write("Unidentified Terrain");
                    break;
            }
            Console.Write("\r\n\r\nPath Description:-\r\n{0}\r\n", sctOHLoss.path_record);
            Console.Write("\r\n");


            Write.htm(eRep, Constant.OFF);
        }

        /// <summary>
        /// This method manages the production of Comma-Separated Values (CSV)
        /// output report format type.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sctOHLoss"></param>
        public static void WriteCSV(Parameters param, OhLossXfer sctOHLoss)
        {
            /*	Comma Separated Variable output. Write the header  */
            if (param.mSiteCallSignA.Length != 0 || param.mSiteCallSignB.Length != 0)
            {
                /*	Display the station */
                Console.Write("\"{0}\",\"{1}\"", param.mSiteCallSignA, param.mSiteNameA);
                Console.Write(",\"{0}\",\"{1}\"", param.mSiteCallSignB, param.mSiteNameB);
            }
            Console.Write("\r\n\"{0,-12}\",\"{1,-13}\",\"{2,-12}\",\"{3,-13}\"",
                    param.mLat83A, param.mLong83A, param.mLat83B, param.mLong87B);

            Console.Write("\r\n{0,5:F1},{1,7:F5},{2,5:F1}", sctOHLoss.s1_anthght, sctOHLoss.K_median,
                    sctOHLoss.s2_anthght);
            Console.Write("\r\n{0,8:F1},", sctOHLoss.freq);
            Console.Write("\"{0}\",", aPol[sctOHLoss.polarization]);
            Console.Write("\"{0}\"", aClimate[sctOHLoss.clim_region]);
            Console.Write("\r\n{0,6:F2}", sctOHLoss.dist_horiz1);
            Console.Write("\r\n{0,6:F2}", sctOHLoss.dist_horiz2);
            Console.Write("\r\n{0,6:F2}", sctOHLoss.elev_horiz1);
            Console.Write("\r\n{0,6:F2}", sctOHLoss.elev_horiz2);
            Console.Write("\r\n{0,6:F2}", sctOHLoss.angl_horiz1 * 57.2957795131);
            Console.Write("\r\n{0,6:F2}", sctOHLoss.angl_horiz2 * 57.2957795131);
            Console.Write("\r\n{0,6:F2}", sctOHLoss.s1_effhght);
            Console.Write("\r\n{0,6:F2}", sctOHLoss.s2_effhght);
            Console.Write("\r\n{0,6:F2}", sctOHLoss.eff_dist);
            Console.Write("\r\n{0,6:F2}", sctOHLoss.horiz_xover);
            Console.Write("\r\n{0,6:F2}", sctOHLoss.refdiff_loss);
            Console.Write("\r\n{0,6:F2}", sctOHLoss.refscatt_loss);
            Console.Write("\r\n{0,6:F2}", sctOHLoss.refcomb_loss);
            Console.Write("\r\n{0,6:F2}", sctOHLoss.median_loss);
            for (int nInd = 0; nInd < 8; nInd++)
            {
                Console.Write("\r\n{0,10:F4},{1,11:F1},{2,11:F1}", dTimePct[nInd],
                        sctOHLoss.ohloss_50[nInd], sctOHLoss.ohloss_95[nInd]);
            }
            switch ((Enums.OHLcalcType)sctOHLoss.calc_type)
            {
                case Enums.OHLcalcType.OHL_LOS:
                    Console.Write("\r\n\"Line of Sight\"");
                    break;

                case Enums.OHLcalcType.OHL_SKE:
                    Console.Write("\r\n\"Single Knife Edge\"");
                    break;

                case Enums.OHLcalcType.OHL_ISOL:
                    Console.Write("\r\n\"Rounded Isolated Obstacle\"");
                    break;

                case Enums.OHLcalcType.OHL_DKE:
                    Console.Write("\r\n\"Double Knife Edge\"");
                    break;

                case Enums.OHLcalcType.OHL_IRT:
                    Console.Write("\r\n\"Irregular Terrain\"");
                    break;

                default:
                    Console.Write("\r\n\"Unidentified Terrain\"");
                    break;
            }
            Console.Write("\r\n\"{0}\"", sctOHLoss.path_record);
            Console.Write("\r\n");
        }




    }
}

```
