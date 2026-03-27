using _Configuration;
using _DataStructures;
using _NewLib;
using static _NewLib.Degrees;
using static _NewLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _Utillib;
using System.IO;

/// <summary>
/// A library that provides methods for geometric, distance, elevation and gain calculations.
/// </summary>
namespace _Auxlib
{
    /// <summary>
    /// Provides methods that perform geometric and gain calculations.
    /// </summary>
    public class Ax14
    {
        private static double[] LL = new double[] { 0.2, 0.4, 0.6, 0.8, 1.0, 1.2, 1.4, 1.6 };

        private static double[] dLoss = new double[] { 11.0, 10.0, 9.0, 8.0, 7.0, 6.0, 5.0, 4.0, 3.0, 2.0, 1.0, 0.1, 0.0 };

        private static double[,] kinv = new double[13, 8]{{0.170, 0.179, 0.189, 0.210, 0.250, 0.295, 0.355, 0.410},
                                             {0.180, 0.190, 0.210, 0.238, 0.280, 0.330, 0.405, 0.465},
                                             {0.185, 0.204, 0.230, 0.270, 0.320, 0.380, 0.460, 0.540},
                                             {0.195, 0.220, 0.253, 0.308, 0.360, 0.430, 0.520, 0.620},
                                             {0.208, 0.239, 0.280, 0.340, 0.410, 0.490, 0.590, 0.700},
                                             {0.220, 0.260, 0.310, 0.380, 0.470, 0.560, 0.660, 0.780},
                                             {0.240, 0.289, 0.348, 0.430, 0.520, 0.635, 0.750, 0.880},
                                             {0.268, 0.320, 0.395, 0.500, 0.610, 0.730, 0.860, 1.020},
                                             {0.305, 0.375, 0.460, 0.580, 0.710, 0.860, 1.010, 1.200},
                                             {0.361, 0.455, 0.570, 0.720, 0.880, 1.040, 1.250, 1.495},
                                             {0.510, 0.650, 0.800, 1.030, 1.270, 1.450, 1.730, 2.120},
                                             {1.500, 1.900, 2.000, 2.020, 2.700, 2.800, 3.050, 3.750},
                                             {2.500, 2.500, 2.500, 2.500, 3.000, 3.250, 3.800, 4.600}
                                            };

        private const int POWERCOL = 0; // Constant.POWERCOL;
        private const int GAINCOL = 1;  // Constant.GAINCOL;
        private const int LOSSCOL = 2;  // Constant.LOSSCOL;
        private const int DISTCOL = 3;  // Constant.DISTCOL;

        private const int HEIGHTCOL = 0; // Constant.HEIGHTCOL;
        private const int WIDTHCOL = 1;  // Constant.WIDTHCOL;     
        private const int INCANGCOL = 2; // Constant.INCANGCOL;

        /// <summary>
        /// Calculates the powers in a connection between two active stations involving 
        /// from 1 to 4 intermediate passive reflectors.
        /// </summary>
        /// <remarks>
        /// The passive data table paDat contains the transmitter power for
        /// the two active stations, antenna gain for active stations,
        /// feeder loss for the active stations, heights of passive reflectors, 
        /// and widths of passive reflectors.  The data is arranged as follows:  
        /// 
        /// paDat[6, 4] =
        /// 
        /// 	row    stn type       col0    col1    col2    col3
        /// 	---    --- ----       ----    ----    ----    ----
        /// 	 0     Active 1       power   gain    loss
        /// 	 1     Passive 1      height  width
        /// 	 2     Passive 2      height  width
        /// 	 3     Passive 3      height  width
        /// 	 4     Passive 4      height  width
        /// 	 5     Active 2       power   gain    loss
        /// </remarks>
        /// <param name="fp"> - .NET TextWriter object.</param>
        /// <param name="np"> - the number of passives.</param>
        /// <param name="freqMHz"> - operating frequency in MHz.</param>
        /// <param name="paDat"> - passive data table (see remarks, above).</param>
        /// <param name="pa"> - array of active and passive station data, including location, name and antenna information.</param>
        /// <param name="havDist"> - flag indicating if distance and angle info has 
        /// been input and thus calcs for these can be skipped.</param>
        /// <param name="gain"> - antenna gain.</param>
        /// <param name="fgain"> - far-field gain.</param>
        /// <param name="eirp"> - Equivalent Isotropically Radiated Power (EIRP) from first active to second active.</param>
        /// <param name="beirp"> - EIRP from second active to first active.</param>
        /// <param name="rsl"> - received signal level.</param>
        /// <param name="patlos"> - pattern loss.</param>
        /// <param name="vfar"> - flags for near or far field: set to 1 for far field, 0 for near field.</param>
        /// <param name="dbl"> - flags for double passive or not: set to 1 for double passive, 0 otherwise.</param>
        /// <returns>  - Constant.SUCCESS if calculation succeeds.</returns>
        public static int AxPassive(
               TextWriter fp,       /* input  - pointer to output file or NULL for no output */
               int np,              /* input  - number of passives */
               double freqMHz,      /* input  - operating frequency */
               double[,] paDat,	    /* input  - passive data table */
               AxStation[] pa,      /* input - active & passive data*/
               bool havDist,	    /* input  - calc distances flag */
               double[] gain,		/* output - antenna gain */
               double[] fgain,	    /* output - far-field gain */
               double[] eirp,		/* output - eirp from first active to second */
               double[] beirp,	    /* output - eirp from secont active to first */
               double[] rsl,		/* output - received signal level */
               double[] patlos,	    /* output - path loss */
               double[] vfar,		/* output - flags for near or far field */
               double[] dbl)        /* output - flags for double passive or not */
        {

            int err,        /* return code variable */
                    i;      /* loop control variable */

            if (np < 1) return Constant.FAILURE;

            for (i = 0; i <= np + 1; i++)
            {

                pa[i].name = pa[i].name.Trim();
            }

            /* do calculations for distance and included angle if not supplied */
            if (!havDist)
            {

                Ax14.AxPasCnv(fp, np, paDat, pa);
            }

            /* perform passive calculations from distances and inc. angles */
            err = AxCalPas(fp, np, freqMHz, paDat, gain, fgain, eirp, beirp,
                             rsl, patlos, vfar, dbl);

            return (err);
        }

        /// <summary>
        /// Converts geographical co-ordinates, antennae heights and ground elevations into distances and included angles.
        /// </summary>
        /// <remarks>
        /// Passive data table containing transmitter power for
        /// the two active stations, antenna gain for the active
        /// stations, feeder loss for the active stations, heights
        /// of passive reflectors, and widths of passive reflectors.
        /// The data is arranged as follows: 
        /// 
        /// paDat[6, 4] =
        /// 
        /// 	row   stn type       col0    col1    col2    col3
        /// 	---   --- ----       ----    ----    ----    ----
        /// 	0     Active 1       power   gain    loss
        /// 	1     Passive 1      height  width
        /// 	2     Passive 2      height  width
        /// 	3     Passive 3      height  width
        /// 	4     Passive 4      height  width
        /// 	5     Active 2       power   gain    loss
        /// 
        /// </remarks>
        /// <param name="fp"> - .NET TextWriter object.</param>
        /// <param name="np"> - number of passive stations</param>
        /// <param name="paDat"> - passive data table that gets updated with included
        /// angle and distance to next station information (see remarks, above).</param>
        /// <param name="pa">- array of active and passive station data, including location, name and antenna information.</param>
        public static void AxPasCnv(
                                    TextWriter fp,           /* input - pointer to file for error messages */
                                    int np,             /* input - number of passives */
                                    double[,] paDat,	/* i&o   - passive data table */
                                    AxStation[] pa)        /* input - active & passive data*/
        {
            int i;          /* loop control variable          */
            double[] azBack = Arrays.CreateArrayUsingDefaultElementConstructor<double>(6);  /* azB[i] = azimuth from i to i+1 */
            double[] azForw = Arrays.CreateArrayUsingDefaultElementConstructor<double>(6);  /* azF[i] = azimuth from i to i-1 */
            double[] elBack = Arrays.CreateArrayUsingDefaultElementConstructor<double>(6);  /* elB[i] = elevAng from i to i+1 */
            double[] elForw = Arrays.CreateArrayUsingDefaultElementConstructor<double>(6);  /* elF[i] = elevang from i to i-1 */
            double antHt1, antHt2; /* antenna heights above sea level*/

            double dDistABKm, dDistBCKm, dIncAngDeg;
            int nRet;

            for (i = 0; i < np + 1; i++)
            {
                /* compute distances and azimuths */
                AxSub2.AxDistan(pa[i].LL.latSeconds, pa[i + 1].LL.latSeconds,
                                  pa[i].LL.longSeconds, pa[i + 1].LL.longSeconds,
                                 out paDat[i, Constant.DISTCOL], out azForw[i], out azBack[i + 1]);

                if (paDat[i, Constant.DISTCOL] <= 0.0)
                {
                    /* come here if distance is zero or less */
                    GenUtil.Qfprintf(fp, String.Format("Warning: Zero distance from {0} to {1}\r\n", i, i + 1));
                    paDat[i, Constant.DISTCOL] = 0.001;
                }

                antHt1 = (pa[i].elevM + pa[i].antHtM) / 1000.0;
                antHt2 = (pa[i + 1].elevM + pa[i + 1].antHtM) / 1000.0;

                /* compute elevations */
                AxSub3.AxElev(antHt1, antHt2, paDat[i, Constant.DISTCOL], out elForw[i], out elBack[i + 1]);

                //	Calculate the actual distance through the air. GJS - 2013-09
                paDat[i, Constant.DISTCOL] = AxSub3.PathDist(antHt1, antHt2, paDat[i, Constant.DISTCOL]);
            }

            /* determine angles at each passive from true bearing and elev angles */
            for (i = 1; i < np + 1; i++)
            {
                paDat[i, Constant.INCANGCOL] = Math.Abs(azForw[i] - azBack[i]);
                if (paDat[i, Constant.INCANGCOL] > 180.0)
                {
                    paDat[i, Constant.INCANGCOL] = 360.0 - paDat[i, Constant.INCANGCOL];
                }
                paDat[i, Constant.INCANGCOL] = ACosD((CosD(elForw[i]) *
                                                                   CosD(elBack[i]) *
                                                                   CosD(paDat[i, Constant.INCANGCOL])) +
                                                                  (Degrees.SinD(elForw[i]) * Degrees.SinD(elBack[i])));
                if (paDat[i, Constant.INCANGCOL] > 140.0)
                {

                    GenUtil.Qfprintf(fp, String.Format("Warning: Angle > 140 at {0}\r\n", i));
                }
                //	For debugging purposes, we put this in here so we can look at the result and compare them.
                nRet = PassiveGeometry(pa[i - 1].LL.latSeconds, pa[i - 1].LL.longSeconds,
                                             pa[i].LL.latSeconds, pa[i].LL.longSeconds,
                                             pa[i + 1].LL.latSeconds, pa[i + 1].LL.longSeconds,
                                                                     pa[i - 1].elevM, pa[i].elevM, pa[i + 1].elevM,
                                                                     pa[i - 1].antHtM, pa[i].antHtM, pa[i + 1].antHtM,
                                                                     out dDistABKm, out dDistBCKm, out dIncAngDeg);
            }
        }

        /// <summary>
        /// Calculates the passive geometry by using the locations of the two endpoints
        /// of the passive (i.e. all three sites), plus the heights of the link we are looking at.
        /// </summary>
        /// <param name="dLatASecs"> - latitude of one end of link to passive.</param>
        /// <param name="dLongASecs"> - longitude of one end of link to passive.</param>
        /// <param name="dLatBSecs"> - latitude of other end of link to passive.</param>
        /// <param name="dLongBSecs"> - longitude of other end of link to passive.</param>
        /// <param name="dLatCSecs"> - latitude of other link from passive.</param>
        /// <param name="dLongCSecs"> - longitude of other link from passive.</param>
        /// <param name="dAntAGrndM"> - ground elevation of end node 'A' relative to datum.</param>
        /// <param name="dAntBGrndM"> - ground elevation of end node 'B' relative to datum.</param>
        /// <param name="dAntCGrndM"> - ground elevation of end node 'C' relative to datum.</param>
        /// <param name="dAntAHtM"> - height above ground of end node 'A'.</param>
        /// <param name="dAntBHtM"> - height above ground of end node 'B'.</param>
        /// <param name="dAntCHtM"> - height above ground of end node 'C'.</param>
        /// <param name="dDistABKm"> - distance between end nodes 'A' and 'B'.</param>
        /// <param name="dDistBCKm"> - distance between end nodes 'B' and 'C'.</param>
        /// <param name="dIncAngDeg"> - the angle included by the lines AB and BC.</param>
        /// <returns>  - Constant.SUCCESS if calculation succeeds.</returns>
        public static int PassiveGeometry(double dLatASecs,       //	Lat/long of one end of link to passive
                                            double dLongASecs,
                                            double dLatBSecs,       //	Lat/Long of passive
                                            double dLongBSecs,
                                            double dLatCSecs,       //	Lat/Long of other link from passive
                                            double dLongCSecs,
                                            double dAntAGrndM,      //	A is to link
                                            double dAntBGrndM,
                                            double dAntCGrndM,
                                            double dAntAHtM,
                                            double dAntBHtM,
                                            double dAntCHtM,
                                            out double dDistABKm,
                                            out double dDistBCKm,
                                            out double dIncAngDeg
                                     )
        {
            double dBtoADeg;
            double dIgnoreDeg;
            double dBtoCDeg;
            double dAntHtAKm;
            double dAntHtBKm;
            double dAntHtCKm;
            double dElABDeg;
            double dElBADeg;
            double dElBCDeg;
            double dElCBDeg;

            int nRet = 0;

            //	Distance from B to A to get the bearing B to A
            AxSub2.AxDistan(dLatBSecs, dLatASecs, dLongBSecs, dLongASecs, out dDistABKm, out dBtoADeg, out dIgnoreDeg);
            if (dDistABKm == 0.0)
            {
                dDistABKm = 0.001;
                nRet = 1;       //	Warning distance was zero and set to 1m.
            }
            //	Distance from B to C to get bearing B to C
            AxSub2.AxDistan(dLatBSecs, dLatCSecs, dLongBSecs, dLongCSecs, out dDistBCKm, out dBtoCDeg, out dIgnoreDeg);
            if (dDistBCKm == 0.0)
            {
                dDistBCKm = 0.001;
                nRet = 1;       //	Warning distance was zero and set to 1m.
            }

            dAntHtAKm = (dAntAGrndM + dAntAHtM) / 1000.0;
            dAntHtBKm = (dAntBGrndM + dAntBHtM) / 1000.0;
            dAntHtCKm = (dAntCGrndM + dAntCHtM) / 1000.0;

            AxSub3.AxElev(dAntHtAKm, dAntHtBKm, dDistABKm, out dElABDeg, out dElBADeg);
            AxSub3.AxElev(dAntHtBKm, dAntHtCKm, dDistBCKm, out dElBCDeg, out dElCBDeg);

            dIncAngDeg = Math.Abs(dBtoADeg - dBtoCDeg);
            if (dIncAngDeg > 180.0)
            {
                dIncAngDeg = 360.0 - dIncAngDeg;
            }
            dIncAngDeg = Degrees.ACosD((Degrees.CosD(dElABDeg) *
                                                     Degrees.CosD(dElBCDeg) *
                                                     Degrees.CosD(dIncAngDeg)) +
                                                    (Degrees.SinD(dElABDeg) * Degrees.SinD(dElBCDeg)));
            if (dIncAngDeg > 140.0)
            {
                nRet += 2;      //	Warning the included angle is greater than 140 degrees.
            }

            return nRet;
        }

        /// <summary>
        /// Calculates the powers of connection between two
        /// active stations involving up to 4 intermediate reflectors 
        /// using the distances between each pair of
        /// antanna along the route and included angle at each passive reflector.
        /// </summary>
        /// <remarks>
        /// The passive data table paDat contains the transmitter power for
        /// the two active stations, antenna gain for active stations,
        /// feeder loss for the active stations, heights of passive reflectors, 
        /// and widths of passive reflectors.  The data is arranged as follows:
        /// 
        /// paDat[6, 4] =
        /// 
        /// 		row   stn type       col0    col1    col2  col3
        /// 		---   --- ----       ----    ----    ----  ----
        /// 		0     Active 1       power   gain    loss  dist
        /// 		1     Passive 1      height  width   iang  dist
        /// 		2     Passive 2      height  width   iang  dist
        /// 		3     Passive 3      height  width   iang  dist
        /// 		4     Passive 4      height  width   iang  dist
        /// 		5     Active 2       power   gain    loss
        /// </remarks>
        /// <param name="fp"> - .NET TextWriter object.</param>
        /// <param name="np"> - the number of passives.</param>
        /// <param name="freqMHz"> - operating frequency in MHz.</param>
        /// <param name="paDat"> - passive data table (see remarks, above).</param>
        /// <param name="gain"> - antenna gain.</param>
        /// <param name="fgain"> - far-field gain.</param>
        /// <param name="eirp"> - Equivalent Isotropically Radiated Power (EIRP) from first active to second active.</param>
        /// <param name="beirp"> - EIRP from second active to first active.</param>
        /// <param name="rsl"> - received signal level.</param>
        /// <param name="patls"> - pattern loss.</param>
        /// <param name="vfar"> - flags for near or far field: set to 1 for far field, 0 for near field.</param>
        /// <param name="dbl"> - flags for double passive or not: set to 1 for double passive, 0 otherwise.</param>
        /// <returns>  - Constant.SUCCESS if check succeeds.</returns>
        public static int AxCalPas(TextWriter fp,         /* input  - pointer to output file */
                                    int np,                /* input  - number of passives */
                                    double freqMHz,        /* input  - operating frequency */
                                    double[,] paDat,      /* input  - passive data table [][4]*/
                                    double[] gain,         /* output - antenna gain */
                                    double[] fgain,        /* output - far-field gain */
                                    double[] eirp,         /* output - eirp from first active to second */
                                    double[] beirp,        /* output - eirp from secont active to first */
                                    double[] rsl,          /* output - received signal level */
                                    double[] patls,        /* output - path loss */
                                    double[] vfar,         /* output - flags for near or far field */
                                    double[] dbl)          /* output - flags for double passive or not */
        {
            //...Log2.v("\n\nAx14.AxCalPas(): Entry");
            //...Log2.v("\r\nAx14.AxCalPas(): freqMHz = " + freqMHz);

            //AH: for testing only.
            //...Log2.v(Arrays.doubleArray2DtoString("paDat:", paDat));

            int i;                  /* loop control variable */
            int b;                  /* table index variable  */
            int j;                  /* table index variable  */

            double const1, const2,          /* intermediate term in equation        */
                            alambd,         /* intermediate term in equation        */
                            asq, alsq,      /* intermediate term in equation        */
                            akinv,          /* near field determinant - ak inverse  */
                            bovera, b2,     /* intermediate term in equation        */
                            a2, aksqin,     /* intermediate term in equation        */
                            dtmp, d,        /* intermediate term in equation        */
                            al,             /* intermediate term in equation        */
                            fsw;            /* itermediate value during iteration   */

            int nRet = 0;

            GenUtil.ReSetError();

            /*  Zero all that will be used. */
            for (i = 0; i <= np + 1; i++)
            {
                dbl[i] = 0.0;
                vfar[i] = 0.0;
                paDat[i, Constant.DISTCOL] *= 1000.0; //	Distance to metres
            }
            alsq = 0.0;

            /*	Sanity check */
            if (freqMHz <= 0.0)
            {
                GenUtil.SetError(2305, "axCalPas Error - Negative Frequency: {0:F3}", freqMHz.ToString());
                GenUtil.Qfprintf(fp, String.Format("\r\n{0}\r\n", GenUtil.GetUserMess()));
                return (1);
            }

            /* compute values of intermediate terms in equations */
            const1 = 20.0 * Math.Log10(freqMHz);
            const2 = const1 - 27.55;
            alambd = 300.0 / freqMHz;        /*	Wavelength in metres  */

            /* compute gain and path loss of first station */
            //...Log2.v("\r\nAx14.AxCalPas(): #1 paDat[0, Constant.LOSSCOL] = " + paDat[0, Constant.LOSSCOL]);
            //...Log2.v("\r\nAx14.AxCalPas(): #2 paDat[0, Constant.DISTCOL] = " + paDat[0, Constant.DISTCOL]);
            gain[0] = paDat[0, Constant.GAINCOL] - paDat[0, Constant.LOSSCOL];
            patls[0] = -(const2 + 20.0 * Math.Log10(paDat[0, Constant.DISTCOL]));

            /* passive loop */
            for (i = 1; i <= np; i++)
            {
                j = i - 1;
                /*  asq is effective area of a passive  */
                //...Log2.v("\r\nAx14.AxCalPas(): #3 paDat[i, Constant.HEIGHTCOL] = " + paDat[i, Constant.HEIGHTCOL]);
                //...Log2.v("\r\nAx14.AxCalPas(): #4 paDat[i, Constant.WIDTHCOL] = " + paDat[i, Constant.WIDTHCOL]);
                //...Log2.v("\r\nAx14.AxCalPas(): #5 paDat[i, Constant.INCANGCOL] = " + paDat[i, Constant.INCANGCOL]);
                asq = paDat[i, Constant.HEIGHTCOL] * paDat[i, Constant.WIDTHCOL] * Math.Cos(paDat[i, Constant.INCANGCOL] * 0.017453292 / 2.0);
                if (i == 1)
                {
                    /*  First passive */
                    alsq = asq;
                    dtmp = asq;
                }
                else
                {
                    /*  Use maximum effective area of passives */
                    dtmp = (asq > alsq) ? asq : alsq;
                }
                //...Log2.v("\r\nAx14.AxCalPas(): #6 paDat[j, Constant.DISTCOL] = " + paDat[j, Constant.DISTCOL]);
                //...Log2.v("\r\nAx14.AxCalPas(): #6a alambd = " + alambd);
                //...Log2.v("\r\nAx14.AxCalPas(): #6b dtmp = " + dtmp);
                akinv = Constant.PI * alambd * paDat[j, Constant.DISTCOL] / (4.0 * dtmp);
                fgain[j] = (20.0 * Math.Log10(asq)) + (2.0 * const1) - 77.32344;

                if (akinv >= 2.5)
                {
                    /* FAR-FIELD Calculations */
                    vfar[j] = 1.0;
                    alsq = asq;
                }
                else if (i == 1)
                {
                    /* Near-Field Passive/Active calc */
                    //...Log2.v("\r\nAx14.AxCalPas(): #7 paDat[0, Constant.GAINCOL] = " + paDat[0, Constant.GAINCOL]);
                    d = Math.Exp(2.3025876 * ((paDat[0, Constant.GAINCOL] - const1 + 42.2) / 20.0));
                    al = d * Math.Sqrt(Constant.PI / (4.0 * asq));

                    //...Log2.v("\r\nAx14.AxCalPas(): A:  " + al + "   " + akinv + "   " + patls[0]);

                    if (AxIntan(al, akinv, ref patls[0]) != 0)
                    {
                        GenUtil.SetError(2310, "axCalPas - Interpolation error 1:-\r\n (%s)",
                                         GenUtil.GetUserMess());
                        nRet = 2;
                        GenUtil.Qfprintf(fp, String.Format("\r\n{0}\r\n", GenUtil.GetUserMess()));
                    }
                    alsq = asq;
                }
                else
                {
                    /* Double passive Calculation.  This is assumed because it is near-field
                    *  passive and not the first passive.  */
                    GenUtil.Qfprintf(fp, String.Format("\r\nDouble passive #{0} with passive #{1}\r\n", i, j));
                    dbl[j] = 1.0;
                    fgain[j] = (fgain[j] < fgain[j - 1]) ? fgain[j] : fgain[j - 1];
                    fgain[j - 1] = fgain[j];
                    b2 = (asq > alsq) ? asq : alsq;
                    a2 = (asq < alsq) ? asq : alsq;
                    bovera = Math.Sqrt(b2 / a2);
                    if (bovera > 2.0)
                    {
                        GenUtil.SetError(2300,
                                         "axCalPas - B/A ({0:F2}) - passive area ratio - greater than 2; set to 2.0", bovera.ToString());
                        nRet = 3;
                        GenUtil.Qfprintf(fp, String.Format("\r\n{0}\r\n", GenUtil.GetUserMess()));
                        bovera = 2.0;
                    }
                    //...Log2.v("\r\nAx14.AxCalPas(): #8 paDat[j, Constant.DISTCOL] = " + paDat[j, Constant.DISTCOL]);
                    aksqin = 2 * alambd * paDat[j, Constant.DISTCOL] / a2;
                    AxIntAp(aksqin, bovera, out patls[j]);
                    alsq = b2;

                    /* if this is hop 3 and hop 1 was near, recompute */
                    /*      the hop 1 path loss                       */
                    if ((i == 2) && (vfar[0] == 0.0))
                    {
                        //...Log2.v("\r\nAx14.AxCalPas(): #9 paDat[0, Constant.DISTCOL] = " + paDat[0, Constant.DISTCOL]);
                        //...Log2.v("\r\nAx14.AxCalPas(): #10 paDat[0, Constant.GAINCOL] = " + paDat[0, Constant.GAINCOL]);

                        akinv = Constant.PI * alambd * paDat[0, Constant.DISTCOL] / (4.0 * b2);
                        d = Math.Exp(2.3025876 * ((paDat[0, Constant.GAINCOL] - const1 + 42.2) / 20.0));
                        al = d * Math.Sqrt(Constant.PI / (4.0 * b2));

                        //...Log2.v("\r\nAx14.AxCalPas(): B:  " + al + "   " + akinv + "   " + patls[0]);
                        if (AxIntan(al, akinv, ref patls[0]) != 0)
                        {
                            GenUtil.SetError(2311, "axCalPas - Interpolation error 2:\r\n (%s)",
                                             GenUtil.GetUserMess());
                            nRet = 4;
                            GenUtil.Qfprintf(fp, String.Format("\r\n{0}\r\n", GenUtil.GetUserMess()));
                        }
                    }
                }
                /* next hop far field path loss */
                //...Log2.v("\r\nAx14.AxCalPas(): #11 paDat[i, Constant.DISTCOL] = " + paDat[i, Constant.DISTCOL]);
                patls[i] = -(const2 + 20.0 * Math.Log10(paDat[i, Constant.DISTCOL]));
            } /* end passive loop */

            /* set up last station and calculate */

            //...Log2.v("\r\nAx14.AxCalPas(): #12 paDat[np + 1, Constant.GAINCOL] = " + paDat[np + 1, Constant.GAINCOL]);
            gain[np + 1] = paDat[np + 1, Constant.GAINCOL] - paDat[np + 1, Constant.LOSSCOL];
            if (alsq == 0.0)
            {
                akinv = 0.0;
            }
            else
            {
                akinv = (Constant.PI * alambd * paDat[np, Constant.DISTCOL]) / (4.0 * alsq);
            }
            vfar[np] = 1.0;

            //...Log2.v("\r\nAx14.AxCalPas(): akinv = " + akinv);
            if (akinv < 2.5)
            {      /* Near-Field */
                vfar[np] = 0.0;
                //...Log2.v("\r\nAx14.AxCalPas(): vfar[np - 1] = " + vfar[np - 1]);
                if (vfar[np - 1] != 1.0)
                {
                    //...Log2.v("\r\nAx14.AxCalPas(): np = " + np);
                    //...Log2.v("\r\nAx14.AxCalPas(): dbl[np - 1] = " + dbl[np - 1]);
                    //...Log2.v("\r\nAx14.AxCalPas(): vfar[np - 2] = " + vfar[np - 2]);
                    if ((np < 2) || (dbl[np - 1] != 1.0) || (vfar[np - 2] != 1.0))
                    {
                        GenUtil.SetError(2301, "axCalPas Error- Single passive with two near-fields.");
                        nRet = 5;
                        GenUtil.Qfprintf(fp, String.Format("\r\n{0}\r\n", GenUtil.GetUserMess()));

                        return nRet;
                    }
                }
                d = Math.Exp(2.3025876 * ((paDat[np + 1, Constant.GAINCOL] - const1 + 42.2) / 20.0));
                al = d * Math.Sqrt(Constant.PI / (4.0 * alsq));

                //...Log2.v("\r\nAx14.AxCalPas(): C:  " + al + "   " + akinv + "   " + patls[np]);
                if (AxIntan(al, akinv, ref patls[np]) != 0)
                {
                    GenUtil.SetError(2312, "axCalPas -  Interpolation error 3:\r\n (%s)", GenUtil.GetUserMess());
                    nRet = 6;
                    GenUtil.Qfprintf(fp, String.Format("\r\n{0}\r\n", GenUtil.GetUserMess()));
                }
            }

            /* set the far/near field gains and calculate throught the hops */
            for (i = 0; i < np; i++)
            {
                if (dbl[i] != 1.0)
                {
                    gain[i + 1] = fgain[i] * vfar[i] * vfar[i + 1];
                }
                else
                {
                    gain[i + 1] = fgain[i] * vfar[i - 1] * vfar[i + 1];
                }
            }

            //	Adjust the path losses for atmospheric attenuation due to Oxygen and water.
            //	It is a negative number so we subtract from it.
            for (i = 0; i <= np; i++)
            {
                patls[i] -= GenUtil.AtmosphericAtten(freqMHz * 1000.0, paDat[i, Constant.DISTCOL] / 1000.0);
            }

            eirp[0] = paDat[0, Constant.POWERCOL] + gain[0];
            beirp[np + 1] = paDat[np + 1, Constant.POWERCOL] + gain[np + 1];
            rsl[1] = eirp[0];
            rsl[0] = beirp[np + 1];

            for (i = 0; i < np + 1; i++)
            {
                b = np - i;
                fsw = 0.0;
                if (i < np)
                {
                    if (i == 0)
                    {
                        eirp[i + 1] = rsl[1] + patls[i] + (fgain[i] * vfar[i]);
                    }
                    else
                    {
                        fsw = 1.0;
                        eirp[i + 1] = rsl[1] + patls[i] +
                                      (fgain[i] * (vfar[i] + (vfar[i - 1] * dbl[i] * fsw)));
                    }
                    beirp[b] = rsl[0] + patls[b] +
                               (fgain[b - 1] * (vfar[b] + (vfar[b + 1] * dbl[b] * fsw)));
                    if (dbl[b] == 1.0)
                    {
                        beirp[b] = beirp[b] - gain[b + 1];
                    }
                }
                rsl[1] = rsl[1] + patls[i] + gain[i + 1];
                rsl[0] = rsl[0] + patls[b] + gain[b];
            }       /* end loop */

            //...Log2.v("\n\nAx14.AxCalPas(): Exit: nRet = " + nRet);
            return (nRet);
        }

        /// <summary>
        /// This method interpolates using the microflect table of ak-inverse vs. alpha-n, in accordance with 
        /// table 28B (Relative Gain) in the 1970 Edition of the Lenkurt Manual.
        /// </summary>
        /// <remarks>
        /// See reference document: 'Engineering Considerations for Microwave Communications Systems', GTE Lenkurt Incorporated, 1970.
        /// 
        /// Available as a .pdf document on the WWW:  http://bryanfields.net/mw-papers/GTE%20Lenkurt%20Book.pdf
        ///
        /// One way of analyzing the near-field situation is to treat the antenna and 
        /// the nearby passive in the same fashion as a "periscope" antenna system.In
        /// this case, a "correction factor" is calculated, and applied to the gain of
        /// the antenna, to obtain the net gain of the periscope combination.Since 
        /// this gain is referred to the location of the reflector, the "path " in 
        /// this method is simply that from the reflector to the more distant end. The shorter path 
        /// simply disappears from the calculation. Figure 28B provides curves for deriving a "periscope" correction factor.
        /// 
        /// \image html LenkurtFigure28b.PNG "Table 28b:  Relative Gain: periscope correction factor."
        /// </remarks>
        /// <param name="al"> - L parameter.</param>
        /// <param name="akinv"> - ak inverse.</param>
        /// <param name="alphan"> - the calculated interpolated value alpha-n.</param>
        /// <returns>  - Constant.SUCCESS if calculation succeeds.</returns>
        public static int AxIntan(double al,              /* input  - L parameter */
                                    double akinv,           /* input  - ak inverse */
                                    ref double alphan)      /* output - alpha-n */
        {
            //...Log2.v("\n\nAx14.AxIntan(): Entry:  " + al + "   " + akinv + "   " + alphan);

            double[] alc = new double[8] { 0.002, 0.20, 0.40, 0.60, 0.80, 1.00, 1.20, 1.40 };
            double[] ak = new double[12] { 0.10, 0.125, 0.15, 0.20, 0.24, 0.28, 0.400, 0.50, 0.61, 0.84, 1.40, 2.500 };
            double[,] an = new double[12, 8]  {
                                                {  3.0,  -0.2,  -0.5,   0.6,   0.4,  -0.7,  -1.9,  -3.8},
                                                {-13.3,  -5.8,   0.4,   0.8,   0.4,  -0.8,  -2.1,  -3.8},
                                                {  0.4,  -0.6,   0.8,   1.1,   0.4,  -0.9,  -2.2,  -3.8},
                                                {  5.3,   4.2,   2.3,   1.4,   0.3,  -1.0,  -2.3,  -3.8},
                                                {  6.0,   5.2,   3.3,   1.4,   0.2,  -1.1,  -2.4,  -3.9},
                                                {  5.9,   5.3,   3.8,   1.7,   0.0,  -1.2,  -2.6,  -4.0},
                                                {  4.4,   4.1,   3.2,   1.9,   0.2,  -1.6,  -3.0,  -4.2},
                                                {  3.0,   2.8,   2.2,   1.3,   0.0,  -1.5,  -3.2,  -4.5},
                                                {  1.7,   1.6,   1.2,   0.4,  -0.5,  -1.7,  -3.2,  -4.6},
                                                { -0.7,  -0.8,  -1.1,  -1.5,  -2.1,  -2.7,  -3.7,  -4.8},
                                                { -5.4,  -5.4,  -5.4,  -5.4,  -5.5,  -5.7,   6.2,  -6.8},
                                                {-10.5, -10.5, -10.5, -10.5, -10.5, -10.5, -10.5, -10.5}
                                            };
            double rk = 0.0, rl = 0.0;
            double rna, rnb;
            int tabSz = 11, i = 1, j;

            int nRet = 0;

            if (akinv >= ak[tabSz])
            {
                alphan = 20.0 * Math.Log10(0.78539816 / akinv);

                GenUtil.SetError(2330,
                                 "axIntan - 1/K ({0:F2}) is too large for interpolation, using {0:F2}. Values will be wrong.",
                                 akinv.ToString(), ak[tabSz].ToString());
                akinv = ak[tabSz];
                nRet = 1;
            }

            if (akinv < ak[0])
            {
                j = 0;
                rk = 0.0;

                GenUtil.SetError(2331,
                                 "axIntan - 1/K ({0:F2}) is too small for interpolation, using {0:F2}. Values will be wrong.",
                                 akinv.ToString(), ak[0].ToString());
                akinv = ak[0];
                nRet = 2;
            }
            else
            {
                for (j = tabSz - 1; j >= 0; j--)
                {
                    if (akinv >= ak[j])
                    {
                        rk = (akinv - ak[j]) / (ak[j + 1] - ak[j]);
                        break;
                    }
                }
            }

            if (al >= alc[7])
            {
                i = 7;
                rna = an[j, i];
                if (j < tabSz)
                {
                    rnb = an[j + 1, i];

                    alphan = rna + rk * (rnb - rna);
                }
                else
                {

                    alphan = rna;
                }
            }
            else
            {
                if (al < alc[0])
                {
                    i = 0;
                    rl = 0;
                }
                else
                {
                    for (i = 6; i >= 0; i--)
                    {
                        if (al >= alc[i])
                        {
                            rl = (al - alc[i]) / (alc[i + 1] - alc[i]);
                            break;
                        }
                    }
                }
                rna = an[j, i] - rl * (an[j, i] - an[j, i + 1]);
                if (j >= tabSz)
                {

                    alphan = rna;
                }
                else
                {
                    rnb = an[j + 1, i] - rl * (an[j + 1, i] - an[j + 1, i + 1]);

                    alphan = rna + rk * (rnb - rna);
                }
            }

            //...Log2.v("\n\nAx14.AxIntan(): Exit: nRet = " + nRet);
            return nRet;
        }

        /// <summary>
        /// This method interpolates using the microflect table of k-squared-inverse vs. alpha-p. 
        /// This is Table 28D in the 1970 Edition of the Lenkurt manual (figure title: "Double Passive Repeater Efficiency Curves").
        /// </summary>
        /// <param name="aksqin"> - k-squared-inverse.</param>
        /// <param name="bovera"> - b/a : ratio of large to small passive areas between 1 and 2.</param>
        /// <param name="alphap"> - alpha parameter.</param>
        public static void AxIntAp(double aksqin,      /* input  - k-squared-inverse */
                                    double bovera,      /* input  - b over a */
                                    out double alphap)  /* output - alpha-p */
        {
            double[] ak = new double[Constant.INTAP_TAB_SIZE]
                                                                {   0.10, 0.15, 0.17, 0.20, 0.25,
                                                                    0.35, 0.40, 0.65, 0.75, 0.85,
                                                                    1.00, 1.50, 2.05, 2.30, 2.60,
                                                                    3.00, 3.50, 4.40, 20.0              };
            double[] ap1 = new double[Constant.INTAP_TAB_SIZE]
                                                                {   0.90, -1.20,  1.40,  1.45,  1.40,
                                                                    1.15, -1.15, -2.00, -2.30, -2.60,
                                                                    -2.80, -3.20, -3.55, -3.80, -4.20,
                                                                    -4.80, -6.00, -8.00, -21.0          };
            double[] ap2 = new double[Constant.INTAP_TAB_SIZE]
                                                                {   -0.90,  0.00, -0.20,  0.00,  0.20,
                                                                    -0.30, -0.40,  0.30,  0.40,  0.60,
                                                                    0.90,  1.30,  0.95,  1.05,  1.30,
                                                                    1.90,  2.00,  1.60, -7.90           };
            int i = 0;
            int tabSz = Constant.INTAP_TAB_SIZE - 1;
            double apt1, apt2;

            if (aksqin <= 0.1)
            {
                alphap = 1.0;
                return;
            }

            while ((i <= tabSz) && (ak[i] < aksqin))
            {
                i++;
            }

            if (i > tabSz)
            {
                alphap = -8.0;
                return;
            }

            if (ak[i] == aksqin)
            {
                apt1 = ap1[i];
                apt2 = ap2[i];
            }
            else
            {
                apt1 = ap1[i] + ((ap1[i - 1] - ap1[i]) * (ak[i] - aksqin) /
                    (ak[i] - ak[i - 1]));
                apt2 = ap2[i] + ((ap2[i - 1] - ap2[i]) * (ak[i] - aksqin) /
                    (ak[i] - ak[i - 1]));
            }

            alphap = apt1 + ((apt2 - apt1) * (bovera - 1.0)); /* Corrected GJS 2006.07.25 */
        }

        /// <summary>
        ///   Calculate the normal to a passive given the azimuth and elevations of the two beams coming from it.
        /// </summary>
        /// <param name="azmth1"> - azimuth of node 1.</param>
        /// <param name="elvtn1"> - elevation of node 1.</param>
        /// <param name="azmth2"> - azimuth of node 2.</param>
        /// <param name="elvtn2"> - elevation of node 2.</param>
        /// <param name="azmthNorm"> - calculated azimuth of normal.</param>
        /// <param name="elvtnNorm"> - calculated elevation of normal.</param>
        /// <returns>
        ///   Returns  0 if the calculations completed successfully.
        ///   Returns  1 if the included angle is greater than 150 degrees, but calcs are complete.
        ///   Returns -1 if the included angle is too close to 180 degrees and no calculations were performed.
        ///   done.
        /// </returns>
        public static int CalcPassiveNormal(double azmth1, double elvtn1, double azmth2, double elvtn2,
                                                out double azmthNorm, out double elvtnNorm)
        {
            int nRet = 0;

            /*  Define the two unit vectors of the incoming beams.  We use the SEZ
                coordinate system.  */
            double U1e = Degrees.CosD(elvtn1) * Degrees.SinD(azmth1);
            double U1s = -Degrees.CosD(elvtn1) * Degrees.CosD(azmth1);
            double U1z = Degrees.SinD(elvtn1);

            double U2e = Degrees.CosD(elvtn2) * Degrees.SinD(azmth2);
            double U2s = -Degrees.CosD(elvtn2) * Degrees.CosD(azmth2);
            double U2z = Degrees.SinD(elvtn2);

            /*  The sum of these is a vector along the normal to the passive */
            double Pe = U1e + U2e;
            double Ps = U1s + U2s;
            double Pz = U1z + U2z;

            /*  Which has the following length */
            double dLen = Math.Sqrt(Pe * Pe + Ps * Ps + Pz * Pz);
            if (dLen < 0.5176)
            {
                /*  The length of the vector along the normal is small enough that we know
                *   the angle of reflection is greater than 150 degrees.  This value is
                *   sin(30)/sin(75) from the sin rule of the triangles involved.  */
                nRet = 1;
            }

            if (dLen > 0.001)
            {
                /*  Giving us the following unit vector as the Normal to the Passive  */
                double Ne = Pe / dLen;
                double Ns = Ps / dLen;
                double Nz = Pz / dLen;

                /*  And the azimuth and elevations of this are: */
                elvtnNorm = Degrees.ASinD(Nz);
                azmthNorm = 90.0 - Degrees.ATan2D(-Ns, Ne);
                if (azmthNorm < 0)
                {
                    //  Convert to [0, 360)
                    azmthNorm += 360.0;
                }
            }
            else
            {
                /*  The vectors are pretty much in line.  This is an error condition  */
                elvtnNorm = 0.0;
                azmthNorm = 0.0;
                nRet = -1;
            }

            return nRet;
        }

        /// <summary>
        /// Calculates the included angle of a beam given the azimuth and elevation of the normal to the billboard and the azimuth and elevation of the next site.
        /// This value should be have to be halved to get the off-axis beam angle.
        /// </summary>
        /// <param name="dNormalAz"> - azimuth of normal to billboard.</param>
        /// <param name="dNormalEl"> - elevation of normal to billboard.</param>
        /// <param name="dAz"> - azimuth of the next site.</param>
        /// <param name="dEl"> - elevation of the next site.</param>
        /// <returns>the calculated included angle.</returns>
        public static double IncludedAngle(double dNormalAz, double dNormalEl, double dAz, double dEl)
        {
            double dDiffAz = Math.Abs(dAz - dNormalAz);

            if (dDiffAz > 180.0)
            {
                dDiffAz = 360.0 - dDiffAz;
            }

            /*  return ACOSD(COSD(dAz - dNormalAz) * COSD(dEl - dNormalEl)) * 2.0; */
            return Degrees.ACosD((Degrees.CosD(dEl) * Degrees.CosD(dNormalEl) * Degrees.CosD(dDiffAz)) +
                                     (Degrees.SinD(dEl) * Degrees.SinD(dNormalEl))) * 2.0;
        }

        /// <summary>
        /// Calculates the effective area of a billboard from its dimensions and the included angle of the bounced signal.
        /// </summary>
        /// <param name="dWidth"> - billboard width.</param>
        /// <param name="dHeight"> - billboard height.</param>
        /// <param name="dIncAngle"> - included angle.</param>
        /// <returns> - calculated effective area.</returns>
        public static double EffectiveArea(double dWidth, double dHeight, double dIncAngle)
        {
            return dWidth * dHeight * Degrees.CosD(dIncAngle / 2.0);
        }

        /// <summary>
        /// Calculates the passive gain for a passive billboard in a double passive (i.e.the other end of the link is also a passive).
        /// </summary>
        /// <remarks>
        /// The calculations are slightly different here, as we don't have a gain, we have an effective area.
        /// 
        /// This will check to see if we are in the near-field area of the next active,
        /// and if so, it will correct by the use of table 28C in the Lenkurt manual.
        /// </remarks>
        /// <param name="dWidth"> - billboard width.</param>
        /// <param name="dHeight"> - billboard height.</param>
        /// <param name="dIncAngle"> - included angle.</param>
        /// <param name="dFreqMHz"> - operating frequency in MHz.</param>
        /// <param name="dDist"> - distance to the next site (kms).</param>
        /// <param name="dAreaRem"> - the antenna area at the remote site</param>
        /// <param name="dFarGain"> - the calculated gain.</param>
        /// <returns>  - Constant.SUCCESS if calculation succeeds.</returns>
        public static int PassPassGain(double dWidth,         /*	The width of the passive (m) */
                                        double dHeight,     /*	The height of the passive (m)  */
                                        double dIncAngle,   /*	The included angle in degrees	*/
                                        double dFreqMHz,    /*	Frequency in MHz		*/
                                        double dDist,           /*	Distance to the next site (km) */
                                        double dAreaRem,    /*	The antenna area at the remote site	*/
                                        out double dFarGain) /*	Output - the calculated gain   */
        {
            double dArea = EffectiveArea(dWidth, dHeight, dIncAngle);
            double dKInv = InvK(dFreqMHz, dDist * 1000.0, dArea);
            int nRet = 0;

            GenUtil.ReSetError();

            dFarGain = FarFieldGain(dArea, dFreqMHz);

            if (dKInv < 2.5)
            {
                double dL = 2.0;
                double dNearFieldCorr = 0;
                /*	Near field condition.  Modify far-field gain by results from table
                *		28C.		*/
                /*	In the case of passive to passive, the L value is the following:- */
                if (dArea > 0.0)
                {
                    dL = Math.Sqrt(dAreaRem / dArea);
                }

                if (IntFig28C(dKInv, dL, out dNearFieldCorr) != 0)
                {
                    GenUtil.SetError(2320, "passpassgain - Interpolation error: %s", GenUtil.GetUserMess());
                    nRet = 2320;
                }
                dFarGain -= dNearFieldCorr;
            }

            dFarGain /= 2.0; /*	This is the one-way gain  */

            return nRet;
        }

        /// <summary>
        /// Calculates the inverse of K, used to tell if an antenna is in the near or far field.
        /// </summary>
        /// <param name="dFreqMHz"> - operating frequency in MHz.</param>
        /// <param name="dDist"> - distance.</param>
        /// <param name="dEffArea"> - effective area.</param>
        /// <returns> - the calculated inverse of K.</returns>
        public static double InvK(double dFreqMHz, double dDist, double dEffArea)
        {
            return 3.14159265359 * (300.0 / dFreqMHz) * dDist / (4.0 * dEffArea);
        }

        /// <summary>
        /// Calculates the far-field gain for a passive given the Effective
        /// Area of the billboard and the frequency in MHz.
        /// </summary>
        /// <param name="dEffArea"> - effective area of billboard.</param>
        /// <param name="dFreqMHz"> - operating frequency.</param>
        /// <returns> - calculates far-field gain.</returns>
        public static double FarFieldGain(double dEffArea, double dFreqMHz)
        {
            return 20.0 * Math.Log10(dEffArea) + 40.0 * Math.Log10(dFreqMHz) - 77.32344;/*77.1007;*/
        }

        /// <summary>
        /// This method interpolates figure 28C of the Lenkurt manual
        /// and returns the efficiency loss in db for near field configurations.
        /// </summary>
        /// <param name="akinv"> - 1/K</param>
        /// <param name="al"> - the L parameter.</param>
        /// <param name="dCorrection"> - the calculated near-field correction in db.</param>
        /// <returns>Non-negative return values indicate success.</returns>
        public static int IntFig28C(double akinv, double al, out double dCorrection)
        {
            // Satisfy the 'out' requirement.
            dCorrection = 0.0;

            int cols = LL.Length;  // 8, used below.

            int rows = kinv.GetLength(0);

            /*	Loss is returned positive as it will be subtracted  */


            int nCol;
            int nRet = -1;
            int nIndL;
            int nIndK;
            double dPortion = -1.0;

            /*	Find the column(s) representing the value of L (al) */
            if (al < LL[0])
            {
                nCol = 0;

                GenUtil.SetError(2400, "intFig28C - L value({0:F2}) is too small, using: {0:F2}", al.ToString(), LL[0].ToString());
                al = LL[0];
                nRet = -2;
            }

            if (al > LL[cols - 1])
            {
                nCol = cols - 1;

                GenUtil.SetError(2401, "intFig28C - L value({0:F2}) is too large, using: {0:F2}", al.ToString(), LL[cols - 1].ToString());
                al = LL[cols - 1];
                nRet = -3;
            }

            /*	Scan for the L column  */
            for (nIndL = 1; nIndL < cols; nIndL++)
            {
                if (al <= LL[nIndL])
                {
                    nIndL--;
                    dPortion = (al - LL[nIndL]) / (LL[nIndL + 1] - LL[nIndL]);
                    break;
                }
            }

            if (dPortion < 0.0)
            {

                GenUtil.SetError(2402, "intFig28C - Error L value off table: {0:F2}", al.ToString());

                dCorrection = 0.0;
                nRet = -4;
                return nRet;
            }

            /*	Now go up the graph to find the loss  */
            if (akinv < kinv[0, nIndL])
            {

                GenUtil.SetError(2405, "intFig28C - 1/K is too small for interpolation: {0:F2} on L={0:F2}. Set to -11db",
                                 akinv.ToString(), al.ToString());
                nRet = -5;

                dCorrection = -11.0;
            }
            else
            {
                for (nIndK = 1; nIndK < rows; nIndK++)
                {
                    double dNext = kinv[nIndK, nIndL] +
                                                    ((kinv[nIndK, nIndL + 1] - kinv[nIndK, nIndL]) *
                                                     dPortion);
                    if (akinv <= dNext)
                    {
                        double dLast = kinv[nIndK - 1, nIndL] +
                                                     ((kinv[nIndK - 1, nIndL + 1] - kinv[nIndK - 1, nIndL]) *
                                                        dPortion);

                        dCorrection = dLoss[nIndK - 1] +
                                                        ((akinv - dLast) / (dNext - dLast)) *
                                                        (dLoss[nIndK] - dLoss[nIndK - 1]);
                        nRet++;
                        break;
                    }
                }
            }

            return nRet;
        }

        /// <summary>
        /// Calculates the passive gain for the passive billboard. This method checks whether
        /// we are in the near-field area of the next active and, if so, it will
        /// correct by using table 28C in the Lenkurt manual.
        /// </summary>
        /// <param name="dWidth"> - the width of the passive.</param>
        /// <param name="dHeight"> - the height of the passive.</param>
        /// <param name="dIncAngle"> - the included angle in degrees.</param>
        /// <param name="dFreqMHz"> - operating frequency in MHz.</param>
        /// <param name="dDist"> - distance to the next site (km).</param>
        /// <param name="dGain"> - the antenna gain at the next or active site.</param>
        /// <param name="dFarGain"></param>
        /// <returns> - the calculated passive gain.</returns>
        public static int PassiveGain(double dWidth,      /*	The width of the passive (m) */
                                     double dHeight,        /*	The height of the passive (m)  */
                                     double dIncAngle,  /*	The included angle in degrees	*/
                                     double dFreqMHz,       /*	Frequency in MHz		*/
                                     double dDist,          /*	Distance to the next site (km) */
                                     double dGain,          /*	The antenna gain at the next or active
									 										*		site	*/
                                     out double dFarGain)  /*	Output gain. */
        {
            double dArea = EffectiveArea(dWidth, dHeight, dIncAngle);
            double dKInv = InvK(dFreqMHz, dDist * 1000.0, dArea);
            int nRet = 0;

            GenUtil.ReSetError();

            dFarGain = FarFieldGain(dArea, dFreqMHz);

            if (dKInv < 2.5)
            {
                /*	Near field condition.  Modify far-field gain by results from table
                *		28C.		*/
                double dDiam = Da(dGain, dFreqMHz); /*	Get approx diam at next site  	*/
                double dL = L(dDiam, dArea);                /*	Get the second param for table 	*/
                double dNearFieldCorr = 0;
                if (IntFig28C(dKInv, dL, out dNearFieldCorr) != 0)
                {
                    GenUtil.SetError(2340, "passivegain - Interpolation error:\r\n (%s)", GenUtil.GetUserMess());
                    nRet = 1;
                }
                dFarGain -= dNearFieldCorr;
            }

            dFarGain /= 2.0;       /*	This is the one-way gain  */

            return nRet;
        }

        /// <summary>
        /// Calculates the approximate diameter of a parabolic antenna from its gain and the operating frequency.'
        /// </summary>
        /// <param name="dGaindB"> - gain of parabolic antenna.</param>
        /// <param name="dFreqMHz"> - operating frequency of parabolic antenna.</param>
        /// <returns> - the approximate diameter of the parabolic antenna.</returns>
        public static double Da(double dGaindB, double dFreqMHz)
        {
            return Math.Pow(10.0, (dGaindB - 20.0 * Math.Log10(dFreqMHz) + 42.2) / 20.0);
        }

        /// <summary>
        /// Calculates the L parameter that is used to access the Lenkurt figures to get the
        /// corrections for near field.
        /// </summary>
        /// <param name="dDiam"> - diameter.</param>
        /// <param name="dEffArea"> - effective area.</param>
        /// <returns>calculated L paramter.</returns>
        public static double L(double dDiam, double dEffArea)
        {
            return dDiam * Math.Sqrt(0.785398163 / dEffArea);
        }


        /// <summary>
        /// The WebMICS Auxiliary Engineering 'Passive Calculations' tool writes a user's input
        /// parameters to text file using CSV format; this method parses these CSV-formatted 
        /// lines of text and outputs data that can be used as input to the method AxPassive().
        /// </summary>
        /// <param name="lines"> - array of lines of text (CSV format).</param>
        /// <param name="currSet"> - number of the current set.</param>
        /// <param name="numPassives"> - number of passives.</param>
        /// <param name="freqMHz"> - operating frequency (MHz).</param>
        /// <param name="pasDatTab"> - passive data table.</param>
        /// <param name="axStns"> - active & passive data.</param>
        /// <param name="haveDist"> - calculate distances flag.</param>
        /// <param name="message"> - text of error message.</param>
        /// <remarks>
        /// The 2-dimensional array <b>pasDatTab</b> is the passive data table that, on output, 
        /// is populated with the transmitter power for the two active stations, antenna 
        /// gain for the active stations, feeder loss for the active stns, heights of 
        /// passive reflectors, and the widths of passive reflectors.  
        /// 
        /// If output argument <b>haveDist</b> is true then it also will contain the included 
        /// angles, and distances to the next station.
        /// 
        /// The data is arranged as follows:
        /// <code> </code>
        /// <code>          pasDatTab[6][4] =</code>
        /// <code> </code>
        /// <code>          row   stn type       col0    col1    col2  col3</code>
        /// <code>          ---   --- ----       ----    ----    ----  ----</code>
        /// <code>          0     Active 1       power   gain    loss  dist</code>
        /// <code>          1     Passive 1      height  width   iang  dist</code>
        /// <code>          2     Passive 2      height  width   iang  dist</code>
        /// <code>          3     Passive 3      height  width   iang  dist</code>
        /// <code>          4     Passive 4      height  width   iang  dist</code>
        /// <code>          5     Active 2       power   gain    loss</code>
        /// <code> </code>
        /// </remarks>
        /// <returns></returns>
        public static int AxInpPas(
                                    string[] lines,          // input - array of lines of text (CSV format) 
                                    int currSet,	         // input  - number of the current set 
                                    out int numPassives,     // output - number of passives 
                                    out double freqMHz,      // output - operating frequency 
                                    out double[,] pasDatTab, // output - passive data table 
                                    out AxStation[] axStns,  // output - active & passive data
                                    out bool haveDist,	     // output - calculate distances flag 
                                    out string message       // output - text of error message
            )
        {
            // 'out' requirement.
            numPassives = Int32.MinValue;
            freqMHz = Double.MinValue;
            pasDatTab = new double[6, 4];
            axStns = Arrays.CreateArrayUsingDefaultElementConstructor<AxStation>(6);
            haveDist = false;
            message = "";

            RecType recType = RecType.UNKNOWN;

            int lat;        /* latitude as read from file   */
            int lng;        /* longitude as read from file  */
            int rc;         /* return code variable         */

            string line;
            string[] fields;
            int fieldNum = 0;
            int lineNum;
            int expectedNumOfLines;
            string latSense;
            string lngSense;
            int rowIndex;
            int colIndex;

            // Check that we have something to parse.
            if (lines.Length == 0)
            {
                return (Constant.END_PROC);
            }

            // Parse the first line
            line = lines[0];

            if (String.IsNullOrWhiteSpace(line))
            {
                message = String.Format("Record {0}: Blank record found, no data processed", currSet);
                return (Constant.INP_ID_FAIL);
            }


            // If we reach here the first line must contain some text.
            // Split it into CSV fields.
            fields = line.Split(',');

            // Check that the first line has 3 CSV fields.
            if (fields.Length != 3)
            {
                message = String.Format("Record {0}: First line does not have 3 CSV fields", currSet);
                return (Constant.INP_ID_FAIL);
            }

            // So we now know that it has 3 fields; normalize them.
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = fields[i].Trim().ToUpper();
            }

            // Parse the first line, first field.
            if (fields[0].Equals("L"))
            {
                recType = RecType.INFO;
                haveDist = false;
            }
            else if (fields[0].Equals("D"))
            {
                recType = RecType.INFO;
                haveDist = true;
            }
            else
            {
                message = String.Format("Data Set {0}: Invalid calc type - must be L or D", currSet);
                return (Constant.INP_ID_FAIL);
            }

            // Parse the first line, second field.
            numPassives = Convert.ToInt32(fields[1]);
            if ((numPassives < 1) || (numPassives > 4))
            {
                message = String.Format("Data Set {0}: Invalid number of pasives -  must be 1, 2, 3, or 4", currSet);
                return (Constant.INP_ID_FAIL);
            }

            // Parse the first line, third field.
            freqMHz = Convert.ToDouble(fields[2]);

            // Check that we have the correct number of lines.
            expectedNumOfLines = 1 + 1 + numPassives + 1;
            if (lines.Length != expectedNumOfLines)
            {
                message = String.Format("Data Set {0}: too few or too many lines for the number of passives prescribed on first line", currSet);
                return (Constant.INP_ID_FAIL);
            }

            // Parse the remaining ACTIVE and PASSIVE type lines.
            // Line numbers in the input file are indexed as 0, 1, 2, ...
            // Thus, the first ACTIVE is line index 1.
            for (lineNum = 1; lineNum < expectedNumOfLines; lineNum++)
            {
                // Set the row index, re: paDat[row, col].
                rowIndex = lineNum - 1;

                // Normalize the input line of text.
                // Parse the line into CSV fields.
                line = lines[lineNum].Replace(" ", "");
                fields = line.Split(',');

                // We know the expected order of lines by type.
                if ((lineNum == 1) || (lineNum == expectedNumOfLines - 1))
                {
                    // We should have an ACTIVE type line.

                    // Check that the first field starts with 'ACTIVE#'.
                    if ((fields.Length > 0) && (!fields[0].StartsWith("Active#")))
                    {
                        message = String.Format("Data Set {0}: second line must start with Active#", currSet);
                        return (Constant.INP_ID_FAIL);
                    }
                    else
                    {
                        recType = RecType.ACTIVE;
                    }

                    // Parse the fields in this ACTIVE line.
                    axStns[rowIndex].name = fields[0];
                    pasDatTab[rowIndex, POWERCOL] = Convert.ToDouble(fields[1]);
                    pasDatTab[rowIndex, GAINCOL] = Convert.ToDouble(fields[2]);
                    pasDatTab[rowIndex, LOSSCOL] = Convert.ToDouble(fields[3]);
                }
                else
                {
                    // We should have a PASSIVE type line.

                    // Check that the first field starts with 'PASSIVE#'.
                    if ((fields.Length > 0) && (!fields[0].StartsWith("Passive#")))
                    {
                        message = String.Format("Data Set {0}: line {1} should start with Passive#", currSet, lineNum + 1);
                        return (Constant.INP_ID_FAIL);
                    }
                    else
                    {
                        recType = RecType.PASSIVE;
                    }

                    double width = Convert.ToDouble(fields[1]);
                    double height = Convert.ToDouble(fields[2]);

                    // Check for any nonsense.
                    if (width < 0.0)
                    {
                        message = String.Format("Data Set {0}: Width for passive {1} must be greater than zero", currSet, lineNum - 1);
                        return (Constant.INP_ID_FAIL);
                    }
                    if (height < 0.0)
                    {
                        message = String.Format("Data Set {0}: Height for passive {1} must be greater than zero", currSet, lineNum - 1);
                        return (Constant.INP_ID_FAIL);
                    }

                    axStns[rowIndex].name = fields[0];
                    pasDatTab[rowIndex, WIDTHCOL] = width;
                    pasDatTab[rowIndex, HEIGHTCOL] = height;
                }

                if (haveDist)
                {
                    // Continue to parse the first ACTIVE line
                    if (lineNum == 1)
                    {
                        double dist = Convert.ToDouble(fields[4]);

                        // Sanitize the distance.
                        if (dist <= 0.0)
                        {
                            message = String.Format("Data Set {0}: Invalid distance from stn {1} to {2} - must be positive", currSet, lineNum, lineNum + 1);
                            return (Constant.INP_ID_FAIL);
                        }

                        pasDatTab[rowIndex, DISTCOL] = dist;
                    }
                    else
                    {
                        // Continue to the parse the PASSIVE lines
                        if (recType == RecType.PASSIVE)
                        {
                            double angle = Convert.ToDouble(fields[3]);
                            double dist = Convert.ToDouble(fields[4]);

                            // Deal with any nonsense.
                            if (angle > 140.0)
                            {
                                message = String.Format("Data Set {0}: Invalid inc. ang at stn {1}- cannot be greater than 140", currSet, lineNum - 1);
                                return (Constant.INP_ID_FAIL);
                            }
                            if (dist <= 0.0)
                            {
                                message = String.Format("Data Set {0}: Invalid distance from stn {1} to {2} - must be positive", currSet, lineNum - 1, lineNum);
                                return (Constant.INP_ID_FAIL);
                            }

                            pasDatTab[rowIndex, INCANGCOL] = angle;
                            pasDatTab[rowIndex, DISTCOL] = dist;
                        }
                    }
                }
                else  // We do not have the distance.
                {
                    switch (recType)
                    {
                        case RecType.PASSIVE:
                            fieldNum = 3;
                            break;
                        case RecType.ACTIVE:
                            fieldNum = 4;
                            break;
                    }

                    // The latitude field is an integer whose digits map to ddmmss.
                    lat = Convert.ToInt32(fields[fieldNum++]);
                    latSense = fields[fieldNum++];

                    // The longitude field is an integer whose digits map to dddmmss.
                    lng = Convert.ToInt32(fields[fieldNum++]);
                    lngSense = fields[fieldNum++];

                    // Sanitize the lat/lng senses.
                    bool latSenseValid = (latSense.Equals("N")) || latSense.Equals("S");
                    if (!latSenseValid)
                    {
                        message = String.Format("Data Set {0}: Invalid lat senes for stn {1} - must be N or S", currSet, lineNum);
                        return (Constant.INP_ID_FAIL);
                    }
                    bool lngSenseValid = (lngSense.Equals("E")) || lngSense.Equals("W");
                    if (!lngSenseValid)
                    {
                        message = String.Format("Data Set {0}: Invalid lng sense for stn {1} - must be E or W", currSet, lineNum);
                        return (Constant.INP_ID_FAIL);
                    }

                    rc = AxSub4.AxLoadStr(lat, lng, ref axStns[lineNum - 1], out message);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }


                    axStns[rowIndex].LL.latSens = latSense;
                    axStns[rowIndex].LL.longSens = lngSense;

                    axStns[rowIndex].elevM = Convert.ToDouble(fields[fieldNum++]);
                    axStns[rowIndex].antHtM = Convert.ToDouble(fields[fieldNum]);
                }

            } // end of loop over remaining lines.

            StringBuilder sb = new StringBuilder();
            for (rowIndex = 0; rowIndex < 6; rowIndex++)
            {
                sb.Append("\n");
                for (colIndex = 0; colIndex < 4; colIndex++)
                {
                    sb.Append(Convert.ToString(pasDatTab[rowIndex, colIndex]) + "    ");
                }

            }

            //...Log2.v("\nAx14.AxInpPas(): paDat[6,4]:\n" + sb.ToString());

            sb = new StringBuilder();
            for (rowIndex = 0; rowIndex < 6; rowIndex++)
            {
                sb.Append("\n");
                sb.Append("\nAxStation[" + rowIndex.ToString() + "] :");
                sb.Append(axStns[rowIndex].ToString());
            }

            //...Log2.v("\nAx14.AxInpPas(): paDat[6,4]:\n" + sb.ToString());

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method formats and writes a text report file that presents the results
        /// of a previous call to Ax14.AxPassive().
        /// </summary>
        /// <param name="tW"> - TextWriter object for output file</param>
        /// <param name="numPassives"> - number of passives</param>
        /// <param name="calcPatterns"> - calc patterns flag</param>
        /// <param name="freqMHz"> - operating frequency</param>
        /// <param name="pasDatTab"> - [6, 4] passive data table</param>
        /// <param name="axStns"> - active & passive data</param>
        /// <param name="gain"> - antenna gain</param>
        /// <param name="fgain"> - far-field gain</param>
        /// <param name="eirp"> - eirp from first active to second</param>
        /// <param name="beirp"> - eirp from secont active to first</param>
        /// <param name="rsl"> - received signal level</param>
        /// <param name="patloss"> - pattern loss</param>
        /// <param name="vfar"> - flags for near or far field</param>
        /// <param name="dbl"> - flags for double passive or not</param>
        /// <param name="haveDist"> - calculate distances flag</param>
        /// <param name="errCode"> - Ax14.AxPassive() error code</param>
        public static void AxRptPas(TextWriter tW,       /* input - TextWriter object for output file */
                                    int numPassives,     /* input - number of passives */
                                    bool calcPatterns,   /* input - calc patterns flag */
                                    double freqMHz,      /* input - operating frequency */
                                    double[,] pasDatTab, /* input - [6, 4] passive data table */
                                    AxStation[] axStns,  /* input - active & passive data*/
                                    double[] gain,	     /* input - antenna gain */
                                    double[] fgain,	     /* input - far-field gain */
                                    double[] eirp,	     /* input - eirp from first active to second */
                                    double[] beirp,	     /* input - eirp from secont active to first */
                                    double[] rsl,	     /* input - received signal level */
                                    double[] patloss,	 /* input - pattern loss */
                                    double[] vfar,	     /* input - flags for near or far field */
                                    double[] dbl,	     /* input - flags for double passive or not */
                                    bool haveDist,	     /* input - calculate distances flag */
                                    int errCode)         /* input - error code of previous call to Ax14.AxPassive() */
        {
            double[,] patt = new double[Constant.PAT_ROWS, 2];	/* pattern for passive data     */

            int err;            /* return code variable         */
            int i;              /* loop control variable        */

            double beamWth;     /* beam width                   */
            string fD;          /* F, N, or D for far, near or dbl*/


            tW.Write("\r\n\r\nPASSIVE Program\r\n\r\n");
            tW.Write("NOTE: Distances in Meters and Power in dBm\r\n\r\n");
            tW.Write("INPUTS:\r\n");
            tW.Write("   Frequency:   {0:F1}\r\n", freqMHz);
            tW.Write("   Num Passives: {0}\r\n\r\n", numPassives);
            if (haveDist == false)
            {
                tW.Write("                Power     Ant.Gain    Feed.Loss\r\n");
                tW.Write("   Active 1:    {0:F1}        {1:F1}        {2:F1}\r\n\r\n",
                             pasDatTab[0, POWERCOL], pasDatTab[0, GAINCOL], pasDatTab[0, LOSSCOL]);
                tW.Write("               Name      Width  Height   Lat.    Long.   Elev.  Ant.Ht.\r\n");
                tW.Write("   Active 1: {0,8}                   {1:D2}{2:D2}{3:D2}{4} {5,3:D}{6:D2}{7:D2}{8}  {9,5:F1}   {10,5:F1}\r\n",
                                   axStns[0].name, axStns[0].LL.latDeg, axStns[0].LL.latMin,
                                     axStns[0].LL.latSec, axStns[0].LL.latSens, axStns[0].LL.longDeg,
                                     axStns[0].LL.longMin, axStns[0].LL.longSec, axStns[0].LL.longSens,
                                     axStns[0].elevM, axStns[0].antHtM);
                for (i = 1; i <= numPassives; i++)
                {
                    tW.Write("  Passive {0}: {1,8}   {2,5:F1}   {3,5:F1}  {4:D2}{5:D2}{6:D2}{7} {8,3:D}{9:D2}{10:D2}{11}  {12,5:F1}   {13,5:F1}\r\n",
                                          i, axStns[i].name, pasDatTab[i, WIDTHCOL], pasDatTab[i, HEIGHTCOL],
                                         axStns[i].LL.latDeg, axStns[i].LL.latMin, axStns[i].LL.latSec,
                                         axStns[i].LL.latSens, axStns[i].LL.longDeg, axStns[i].LL.longMin,
                                         axStns[i].LL.longSec, axStns[i].LL.longSens, axStns[i].elevM,
                                         axStns[i].antHtM);
                }
                i = numPassives + 1;
                tW.Write("   Active 2: {0,8}                   {1:D2}{2:D2}{3:D2}{4} {5,3:D}{6:D2}{7:D2}{8}  {9,5:F1}   {10,5:F1}\r\n\r\n",
                                     axStns[i].name, axStns[i].LL.latDeg, axStns[i].LL.latMin,
                                     axStns[i].LL.latSec, axStns[i].LL.latSens, axStns[i].LL.longDeg,
                                     axStns[i].LL.longMin, axStns[i].LL.longSec, axStns[i].LL.longSens,
                                     axStns[i].elevM, axStns[i].antHtM);
                tW.Write("                Power     Ant.Gain    Feed.Loss\r\n");
                tW.Write("   Active 2:    {0:F1}        {1:F1}        {2:F1}\r\n\r\n",
                    pasDatTab[i, POWERCOL], pasDatTab[i, GAINCOL], pasDatTab[i, LOSSCOL]);
            }
            else
            {
                tW.Write("\r\n               Name    Power     Ant.Gain    Feed.Loss   DistToNext(KM)\r\n");
                tW.Write("   Active 1: {0,8}  {1:F1}        {2:F1}        {3:F1}          {4:F2}\r\n\r\n",
                        axStns[0].name, pasDatTab[0, POWERCOL], pasDatTab[0, GAINCOL], pasDatTab[0, LOSSCOL],
                        pasDatTab[0, DISTCOL] / 1000.0);
                tW.Write("               Name     Width   Height   DistToNext(KM)    Inc.Angle\r\n");
                for (i = 1; i <= numPassives; i++)
                {
                    tW.Write("   Pasive {0}: {1,8}  {2,5:F1}    {3,5:F1}       {4,5:F2}          {5,5:F1}\r\n",
                            i, axStns[i].name, pasDatTab[i, WIDTHCOL], pasDatTab[i, HEIGHTCOL], pasDatTab[i, DISTCOL] / 1000.0,
                              pasDatTab[i, INCANGCOL]);
                }
                i = numPassives + 1;
                tW.Write("\r\n               Name    Power     Ant.Gain    Feed.Loss\r\n");
                tW.Write("   Active 2: {0,8}  {1:F1}        {2:F1}        {3:F1}\r\n\r\n",
                    axStns[i].name, pasDatTab[i, HEIGHTCOL], pasDatTab[i, WIDTHCOL], pasDatTab[i, INCANGCOL]);
            }

            tW.Write("RESULTS:\r\n");
            if (errCode == Constant.PAS_DPE_FAIL)
            {
                tW.Write("Can't do double passives with b/a > 2\r\n\r\n");
            }
            else if (errCode == Constant.PAS_SP_FAIL)
            {
                tW.Write("Can't do 2 near fields on same passive\r\n\r\n");
            }
            else
            {
                tW.Write("   Name    Angle   FarGn  ActGn  Eirp12  Eirp21  DistToNextKM  Field  PathLoss\r\n");
                if (vfar[0] == 1.0)
                {
                    fD = "F";
                }
                else
                {
                    fD = "N";
                }
                tW.Write("{0,8}                 {1,6:F1}  {2,5:F1}   {3,6:F1}     {4,6:F2}       {5,1}     {6,6:F1}\r\n",
                        axStns[0].name, gain[0], eirp[0], rsl[0], pasDatTab[0, DISTCOL] / 1000.0,
                    fD, patloss[0]);

                for (i = 1; i <= numPassives; i++)
                {
                    if (vfar[i] == 1.0)
                    {
                        fD = "F";
                    }
                    else
                    {
                        if (dbl[i] == 1.0)
                        {
                            fD = "D";
                        }
                        else
                        {
                            fD = "N";
                        }
                    }
                    tW.Write("{0,8}  {1,5:F1}   {2,5:F1}  {3,5:F1}  {4,5:F1}   {5,6:F1}     {6,6:F2}       {7}     {8,6:F1}\r\n",
                            axStns[i].name, pasDatTab[i, 2], fgain[i - 1], gain[i],
                              eirp[i], beirp[i], pasDatTab[i, DISTCOL] / 1000.0, fD, patloss[i]);
                }       /* end loop */

                i = numPassives + 1;
                if (vfar[i] == 1.0)
                {
                    fD = "F";
                }
                else
                {
                    fD = "N";
                }
                tW.Write("{0,8}                 {1,6:F1}  {2,5:F1}   {3,6:F1}\r\n",
                    axStns[i].name, gain[i], rsl[1], beirp[i]);
            }

            if (calcPatterns == true)
            {
                for (i = 1; i <= numPassives; i++)
                {
                    err = Ax12.AxPasPat(pasDatTab[i, WIDTHCOL], pasDatTab[i, INCANGCOL], freqMHz, fgain[i - 1], out patt,
                                                 out beamWth);

                    Ax12.AxRptPat(tW, axStns[i].name, pasDatTab[i, WIDTHCOL], freqMHz, beamWth, fgain[i - 1],
                             pasDatTab[i, INCANGCOL], patt, err);
                }
            }
        }




    }
}
