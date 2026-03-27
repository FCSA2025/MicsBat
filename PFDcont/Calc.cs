using _Configuration;
using _DataStructures;
using _NewLib;
using _OHloss;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _NewLib.Enums;

namespace PFDcont
{
    /// <summary>
    /// This class provides methods that perform the various calculations required to
    /// compute the Power Flux Density contours or coverage.
    /// </summary>
    public class Calc
    {
        private static int mDistError = 0;
        private static int mCurrRow = -1;

        private const double ACCURACY = 0.1;

        public delegate double LossFunct(double w, Tpass tArgs);

        /// <summary>
        /// This method returns the Power Flux Density (PFD) for prescribed minimum required Rx power, bandwidth and frequency.
        /// </summary>
        /// <param name="pfd"></param>
        /// <returns></returns>
        public static double Pfd(PFD pfd)
        {
            return (pfd.MinRxPwr - 10.0 * Math.Log10(pfd.Bandwidth) + 20.0 * Math.Log10(pfd.Frequency) - 68.55);
        }

        /// <summary>
        /// This method calculates the root of a non-linear equation using the <i>regula falsi</i>
        /// (false-position) iterative technique; the equation is defined by a function f(x) and
        /// and a value V such that the root x0 satisfies f(x0) = V within a tolerance of +/- 0.1
        /// </summary>
        /// <param name="dFrom"></param>
        /// <param name="dTo"></param>
        /// <param name="dSolveFor"></param>
        /// <param name="dRoot"></param>
        /// <param name="dFunc"></param>
        /// <param name="stArgs"></param>
        /// <param name="calcula"></param>
        /// <returns></returns>
        public static int FalsePos(double dFrom,           /*  examine from                        */
                                    double dTo,             /*  To                                  */
                                    double dSolveFor,       /*  The function value we seek          */
                                    ref double dRoot,       /*  The output root                     */
                                    LossFunct dFunc,        /*  The function  */
                                    Tpass stArgs,
                                    ref ExZoneCalc calcula) /*  the work area                 */
        {
            calcula.nIter = 0;
            calcula.dLow = dFrom;
            calcula.dLowVal = dFunc(dFrom, stArgs) - dSolveFor;
            calcula.dHigh = dTo;
            calcula.dHighVal = dFunc(dTo, stArgs) - dSolveFor;

            calcula.nIter = 0;

            while (true)
            {
                /* Ensure that there is a solution */
                if (calcula.dLowVal * calcula.dHighVal > 0)
                {
                    /*  If the product of the end-points is positive then they are both
                        on the same side of the x-axis. */
                    calcula.sResult = "End Points same sign.";
                    return (Constant.FAILURE);

                }
                else // A
                {
                    dRoot = calcula.dLow +
                             (Math.Abs(calcula.dLowVal) / (Math.Abs(calcula.dLowVal) +
                                                         Math.Abs(calcula.dHighVal))) *
                             (calcula.dHigh - calcula.dLow);

                    calcula.dNextVal = dFunc(dRoot, stArgs) - dSolveFor;

                    /*  See if we have an end condition */
                    if (Math.Abs(calcula.dNextVal) < ACCURACY)
                    {
                        /*  Exit condition.  We have solved the equation */
                        calcula.sResult = "";
                        return (0);
                    }
                    else
                    { // B
                        /*  Check for too many iterations */
                        if (++(calcula.nIter) > 100)
                        {
                            calcula.sResult = "Too many Iterations.";
                            return (-2);
                        }
                        else
                        { // C
                            /*  Calculate the next end point set */
                            if (calcula.dNextVal * calcula.dLowVal > 0.0)
                            {
                                /* Next val is the same sign as the low val */
                                calcula.dLow = dRoot;
                                calcula.dLowVal = calcula.dNextVal;
                            }
                            else
                            {
                                calcula.dHigh = dRoot;
                                calcula.dHighVal = calcula.dNextVal;
                            }

                        } // else C
                    } // else B
                } // else A.
            } // end of while.
        } // end of method.

        /// <summary>
        /// This method calculates the part of the correction factor due to
        /// continental or marine influences. The actual values of the parameterized
        /// variables are passed in, as the equation is the same.
        /// </summary>
        /// <param name="dDE"></param>
        /// <param name="dC1"></param>
        /// <param name="dC2"></param>
        /// <param name="dC3"></param>
        /// <param name="dN1"></param>
        /// <param name="dN2"></param>
        /// <param name="dN3"></param>
        /// <param name="dFm"></param>
        /// <param name="dFinf"></param>
        /// <returns></returns>
        public static double CF90a(double dDE,
                                    double dC1,
                                    double dC2,
                                    double dC3,
                                    double dN1,
                                    double dN2,
                                    double dN3,
                                    double dFm,
                                    double dFinf)
        {
            double dF2 = dFinf + (dFm - dFinf) * Math.Exp(-(dC2 * Math.Pow(dDE, dN2)));

            double dY09 = (dC1 * Math.Pow(dDE, dN1) - dF2) * Math.Exp(-(dC3 * Math.Pow(dDE, dN3))) + dF2;

            return (2.41 * dY09);
        }

        /// <summary>
        /// This method calculates the Correction factor to convert terrain losses corresponding to 50% 
        /// probability to 90% probability.
        /// </summary>
        /// <param name="eClim"></param>
        /// <param name="dDist"></param>
        /// <param name="dHte"></param>
        /// <param name="dHre"></param>
        /// <param name="dFreqMHz"></param>
        /// <returns></returns>
        public static double CF90(PFDclimate eClim,
                                    double dDist,
                                    double dHte,
                                    double dHre,
                                    double dFreqMHz)
        {

            double dDL = 3.0 * (Math.Sqrt(2.0 * dHte) + Math.Sqrt(2.0 * dHre));
            double dDs1 = 65.0 * Math.Pow(100.0 / dFreqMHz, 1.0 / 3.0);
            double dDE; /*	Effective distance in km */
            double dRet;

            if (dDist > (dDL + dDs1))
            {
                dDE = 130.0 + dDist - (dDL + dDs1);
            }
            else
            {
                dDE = (130.0 * dDist) / (dDL + dDs1);
            }

            if (eClim == PFDclimate.CONTINENTAL)
            {
                dRet = CF90a(dDE, 9.48e-3, 5.70e-11, 5.56e-6, 1.33, 3.96, 2.44, 8.20, 3.00);
            }
            else
            {
                dRet = CF90a(dDE, 1.29e-4, 1.93e-15, 2.81e-4, 2.14, 5.80, 1.65, 10.0, 4.50);
            }

            return (dRet);
        }

        /// <summary>
        /// This method returns the latitude and longitude (in both numeric and text formats)
        /// for a location having a prescribed distance and azimuth relative to a prescribed location.
        /// </summary>
        /// <param name="cLatout"></param>
        /// <param name="cLongout"></param>
        /// <param name="dLatout"></param>
        /// <param name="dLongout"></param>
        /// <param name="dLat"></param>
        /// <param name="dLong"></param>
        /// <param name="dAz"></param>
        /// <param name="dDist"></param>
        /// <returns></returns>
        public static int LLstrFromDist(out string cLatout,
                                         out string cLongout,
                                         out double dLatout,
                                         out double dLongout,
                                         double dLat,
                                         double dLong,
                                         double dAz,
                                         double dDist)
        {
            int nRet;
            char cOrient;

            if ((nRet = LLFromDist(dLat, dLong, dDist, dAz, out dLatout, out dLongout)) == 0)
            {
                GenUtil.UtLatConvStr(LatLong.FDegToSecs(dLatout), out cLatout, out cOrient);
                GenUtil.UtLongConvStr(LatLong.FDegToSecs(dLongout), out cLongout, out cOrient);
            }
            else
            {
                cLatout = "-";
                cLongout = "-";
            }

            return (nRet);
        }

        /// <summary>
        /// This method returns the distance-dependent propagation loss using CTE's
        /// Over-Horizon (terrain) Loss functions and the affect of atmospheric absorption
        /// (i.e. dBs per km).
        /// </summary>
        /// <param name="dDist"></param>
        /// <param name="dAz"></param>
        /// <param name="dFreq"></param>
        /// <param name="dLa"></param>
        /// <param name="nLat1"></param>
        /// <param name="nLong1"></param>
        /// <param name="dAht1"></param>
        /// <param name="dAht2"></param>
        /// <param name="nPolarization"></param>
        /// <param name="eClim"></param>
        /// <param name="eCalc"></param>
        /// <param name="dDistHorizon"></param>
        /// <param name="nStatusCode"></param>
        /// <returns></returns>
        public static double DistanceDependentLoss(double dDist,
                                             double dAz,
                                             double dFreq,
                                             double dLa,
                                             int nLat1,
                                             int nLong1,
                                             double dAht1,
                                             double dAht2,
                                             int nPolarization,
                                             int eClim,
                                             PFDcalcMode eCalc,
                                             out double dDistHorizon,
                                             out int nStatusCode)
        {


            OhLossXfer sctOHLoss;  /*  Structure for the OH Loss transfer */
            double dLat2;
            double dLong2;
            double dLoss;

            int nRet;
            string cLat;
            string cLong;

            /*  OH-Loss model, using CTE's path profile program. */

            /*	The oh-loss program only returns the terrain losses, not the free-space
            *		losses as well.  So we add in the free-space losses */

            dLoss = LossByFSL(dDist, dFreq, dLa);

            /*  Now set up the OHLoss structure for the routine */
            sctOHLoss = new OhLossXfer();

            sctOHLoss.lat_1 = LatLong.FsecsToDeg(nLat1);
            sctOHLoss.lng_1 = LatLong.FsecsToDeg(nLong1);

            /*	Now calculate the lat2,long2 from the distance and azimuth and then
            *		convert them to NAD 83.
            *		We are using the spherical earth calculations, on the assumption that
            *		this will be close enough for the database we use to transform the
            *		coordinates. */
            nRet = LLstrFromDist(out cLat, out cLong, out dLat2, out dLong2,
                                      LatLong.FsecsToDeg(nLat1), LatLong.FsecsToDeg(nLong1), dAz, dDist);
            if (nRet != 0)
            {
                Console.Write("\r\n* Could not get Lat/Long from Az/Dist for Az={0,7:F2}. Value may be wrong.\r\n", dAz);
            }

            sctOHLoss.lat_2 = dLat2;
            sctOHLoss.lng_2 = dLong2;

            /*  a zero antenna height will break the ohloss routine */
            if (dAht1 > 0.0)
            {
                sctOHLoss.s1_anthght = dAht1;
            }
            else
            {
                sctOHLoss.s1_anthght = 10.0;
            }
            if (dAht2 > 0.0)
            {
                sctOHLoss.s2_anthght = dAht2;
            }
            else
            {
                sctOHLoss.s2_anthght = 10.0;
            }
            sctOHLoss.freq = dFreq;
            sctOHLoss.polarization = (short)nPolarization;
            sctOHLoss.clim_region = (short)eClim;
            sctOHLoss.K_median = 1.333333;    /*  K  */

            CTEfunctions.Calc_OhLoss(ref sctOHLoss);

            if (sctOHLoss.error_status == PRF.OK)
            {
                dDistHorizon = sctOHLoss.dist_horiz1;

                /*	If the calcmode is for coverage, we add the correction factor for
                *		10% of the time to the result for 50. */

                if ((eCalc == PFDcalcMode.COVERAGE50) || (eCalc == PFDcalcMode.COVERAGE90))
                {
                    /*	If the 50% loss is zero this means that it is line-of-sight, and the
                    *		only loss is free space loss.  In this case the correction factor for
                    *		the 90% of the time will be ignored as well.  GJS - 2002.05.31 */
                    if (Math.Abs(sctOHLoss.ohloss_95[(int)PercentTime.FIFTY]) > 0.0001)
                    {
                        dLoss += sctOHLoss.ohloss_95[(int)PercentTime.FIFTY];

                        if (eCalc == PFDcalcMode.COVERAGE90)
                        {
                            dLoss += CF90((PFDclimate)eClim, dDist, sctOHLoss.s1_effhght,
                                            sctOHLoss.s2_effhght, dFreq);
                        }
                    }
                }
                else
                {
                    /*	for pfdcontours use the eighty percent losses at the 95% confidence
                    *		level. */
                    dLoss += sctOHLoss.ohloss_95[(int)PercentTime.EIGHTY];
                }

                nStatusCode = 0;
            }
            else
            {
                /*  Store the error in the result */
                dLoss = -1.0;

                dDistHorizon = -1.0;

                nStatusCode = (int)sctOHLoss.error_status;
            }

            //...Log2.v("\nCalc.TerrainLoss(): dLoss = " + dLoss);
            return (dLoss);
        }

        /// <summary>
        /// This method returns the distance, measured along the azimith from a 
        /// prescribed transmitter location , at which the received power level 
        /// has dropped to a prescribed value.
        /// </summary>
        /// <param name="pfddist"></param>
        /// <param name="dLat"></param>
        /// <param name="dLong"></param>
        /// <param name="dAz"></param>
        /// <param name="dPtx"></param>
        /// <param name="dAfsl"></param>
        /// <param name="dBW"></param>
        /// <param name="dGain"></param>
        /// <param name="dFreqMHz"></param>
        /// <param name="dPFD"></param>
        /// <param name="ftxheight"></param>
        /// <param name="frxheight"></param>
        /// <param name="dLa"></param>
        /// <param name="nPolarization"></param>
        /// <param name="eClim"></param>
        /// <param name="eCalc"></param>
        /// <returns></returns>
        public static int DistOHL(ref double pfddist,
                                     double dLat,
                                     double dLong,
                                     double dAz,
                                     double dPtx,
                                     double dAfsl,
                                     double dBW,
                                     double dGain,
                                     double dFreqMHz,
                                     double dPFD,
                                     double ftxheight,
                                     double frxheight,
                                     double dLa,
                                     int nPolarization,
                                     PFDclimate eClim,
                                     PFDcalcMode eCalc)
        {
            int nRet = Constant.SUCCESS;
            double dRHS;
            double dDist;
            //double				dClose = 0.1;
            double dLoss;
            double FARPOINT = 500.0;
            double dDistHorizon;
            int nStatusCode = 0;

            /*	Calculate the RHS of the equation.  This is what we solve the loss for */
            dRHS = dPtx - dAfsl - (10.0 * Math.Log10(dBW)) + dGain +
                        (20.0 * Math.Log10(dFreqMHz)) - 38.55 - dPFD;

            /******************************************************************
            *
            *		Alternate logic.  Because we have a random bug in the search algorithm
            *		that manifests itself as the CTE routines returning random values.  We
            *		try here to just run from .1 km to the far distance. GJS - 2003.02.12
            *
            *******************************************************************/
            dLoss = 0.00;       /*	Initialize for the first iteration */

            for (dDist = 0.1; (dLoss < dRHS) && (dDist < FARPOINT) && (dLoss >= 0.0); dDist += 0.1)
            {

                /*	Now go from here.  The step will depend on whether the loss at this
                *		distance is greater or less than the required loss. */
                dLoss = DistanceDependentLoss(dDist, dAz, dFreqMHz, dLa,
                                                LatLong.FDegToSecs(dLat), LatLong.FDegToSecs(dLong),
                                                ftxheight, frxheight, nPolarization,
                                                (int)eClim, eCalc, out dDistHorizon, out nStatusCode);
            }

            /*	Check why we exited, and if okay (dLoss >= RHS) converge in */
            if (dLoss < 0.0)
            {
                Console.Write("\r\n*** Error getting terrain loss for Azimuth {0,6:F2}, code {0}\r\n",
                           dAz, nStatusCode);
                nRet = -10;
            }
            else if (dDist >= FARPOINT)
            {
                Console.Write("\r\n*** Error in terrain loss calc. Way too far. Az: {0,6:F2}.\r\n", dAz);
                nRet = -13;
            }
            else
            {
                /*	The current losses are close enough.  This is the first point where the
                *		losses exceed dRHS */
                pfddist = dDist - 0.1;        /*	Go back one */
                nRet = 0;
            }

            //...Log2.v("\nCalc.DistOHL(): pfddist = " + pfddist);
            return (nRet);
        }

        /// <summary>
        /// This method returns the latitude and longitude (floating-point degrees)
        /// of a location that is at a prescribed distance and azimuth relative to
        /// a prescribed location.
        /// </summary>
        /// <param name="dLat"></param>
        /// <param name="dLong"></param>
        /// <param name="dDist"></param>
        /// <param name="dAz"></param>
        /// <param name="dLatOut"></param>
        /// <param name="dLongOut"></param>
        /// <returns></returns>
        public static int LLFromDist(double dLat,             /*  Latitude of the input coord   */
                                        double dLong,          /*  Longitude of the input coord  */
                                        double dDist,          /*  distance from the pt in km    */
                                        double dAz,            /*  Azimuth in decimal degrees    */
                                        out double dLatOut,    /*  Resultant Latitude of point   */
                                        out double dLongOut)   /*  Resultant Longitude of point  */

        {
            return (Sphere.InvDist(dLat, dLong, dDist, dAz, out dLatOut, out dLongOut));
        }


        /// <summary>
        /// This method calculates the trans-horizon distance between two antennas who's 
        /// Line-of-Sight (LOS) just grazes the horizon; this is one of the required
        /// input parameters to the Extended-HATA urban propagation model.
        /// </summary>
        /// <param name="dHeightMW"></param>
        /// <param name="dHeightPCS"></param>
        /// <returns></returns>
        public static double THDist(double dHeightMW, double dHeightPCS)
        {
            return (4.123 * (Math.Sqrt(dHeightMW) + Math.Sqrt(dHeightPCS)));
        }

        /// <summary>
        /// This method returns the Free-Space path loss (FSL) attenuation along an obstacle free, line-of-sight
        /// path through free space with a prescribed distance, frequency and atmospheric absorption
        /// coefficient.
        /// </summary>
        /// <param name="dDist"></param>
        /// <param name="tArgs"></param>
        /// <returns></returns>
        public static double LossByFSL(double dDist, Tpass tArgs)
        {
            double result = FSL(tArgs.mFreqMHz, dDist) + tArgs.mLa * dDist;

            //...Log2.v("\nCalc.LossByFSL(): result = " + result);
            return (result);
        }

        /// <summary>
        /// This method returns the Free-Space path loss (FSL) attenuation along an obstacle free, line-of-sight
        /// path through free space with a prescribed distance, frequency and atmospheric absorption
        /// coefficient.
        /// </summary>
        /// <param name="dDist"></param>
        /// <param name="dFreq"></param>
        /// <param name="dLa"></param>
        /// <returns></returns>
        public static double LossByFSL(double dDist, double dFreq, double dLa)
        {
            Tpass tArgs = new Tpass();
            tArgs.mFreqMHz = dFreq;
            tArgs.mLa = dLa;

            return LossByFSL(dDist, tArgs);
        }

        /// <summary>
        /// This method returns the Free-Space path loss (FSL) attenuation along an obstacle free, line-of-sight
        /// path through free space with a prescribed distance and frequency (but excluding atmospheric absorption).
        /// </summary>
        /// <param name="dFMHz"></param>
        /// <param name="dDist"></param>
        /// <returns></returns>
        public static double FSL(double dFMHz, double dDist)
        {
            double result;

            if (dDist <= 0.00 || dFMHz <= 0.00)
            {
                //...Log2.v("\nCalc.FSL(): result = zero");
                return (0.0);
            }

            /* use free space loss */
            result = 32.45 +
                     20.0 * Math.Log10(dDist) +
                     20.0 * Math.Log10(dFMHz);

            //...Log2.v("\nCalc.FSL(): result = " + result);
            return (result);
        }

        /// <summary>
        /// This method calculates the distance from a transmitter to a location with 
        /// a prescribed PFD value assuming spherical earth losses on a smooth globe.
        /// </summary>
        /// <remarks>
        /// We calculate the free space loss at the Radio Horizon:
        /// <list type="bullet"><item>
        /// If the loss is greater than the RHS of the equation, then we know to use 
        /// the FSL in the attenuation calculations; 
        /// </item><item>
        /// If it is less then we calculate the spherical earth losses at the radio 
        /// horizon plus 1 km (to avoid the singularity in the first km of the RadioHorizon).  
        /// </item><item>
        /// If this loss is greater than the right hand side, we know that the desired loss is somewhere in
        /// between the Radio Horizon and the RH plus one, so we use linear
        /// interpolation between the two.</item>
        /// </list>
        /// </remarks>
        /// <param name="dDist"></param>
        /// <param name="dPtx"></param>
        /// <param name="dAfsl"></param>
        /// <param name="dBW"></param>
        /// <param name="dGain"></param>
        /// <param name="dFreqMHz"></param>
        /// <param name="dPFD"></param>
        /// <param name="dHtx"></param>
        /// <param name="dHrx"></param>
        /// <param name="dLa"></param>
        /// <returns></returns>
        public static int DistPFD(ref double dDist,
                                    double dPtx,
                                    double dAfsl,
                                    double dBW,
                                    double dGain,
                                    double dFreqMHz,
                                    double dPFD,
                                    double dHtx,
                                    double dHrx,
                                    double dLa)
        {
            int nRet = Constant.SUCCESS;
            double dRHS;
            double dHorDist;
            Tpass tArgs = new Tpass();
            ExZoneCalc calcula = new ExZoneCalc();
            double dLossatRH;
            double dLossatRHplus1;

            /*  Spherical earth loss on a smooth globe */
            dHorDist = THDist(dHtx, dHrx);

            tArgs.mHorDist = dHorDist;
            tArgs.mFreqMHz = dFreqMHz;
            tArgs.mLa = dLa;

            /*	The RHS is the required loss for the given PFD */

            dRHS = dPtx - dAfsl - (10.0 * Math.Log10(dBW)) + dGain +
                           (20.0 * Math.Log10(dFreqMHz)) - 38.55 - dPFD;

            if ((dLossatRH = LossByFSL(dHorDist, tArgs)) >= dRHS)
            {
                /*  We want to use Free space losses in the calculation */
                nRet = FalsePos(0.00, dHorDist + 1, dRHS, ref dDist, LossByFSL, tArgs, ref calcula);
            }
            else if ((dLossatRHplus1 = LossBySEL(dHorDist + 1, tArgs)) <= dRHS)
            {
                /*  Spherical earth losses */
                nRet = FalsePos(dHorDist + 1, 999.9, dRHS, ref dDist, LossBySEL, tArgs, ref calcula);
            }
            else
            {
                /*  The losses are between the two.  Use linear interpolation. */
                dDist = dHorDist + (dRHS - dLossatRH) / (dLossatRHplus1 - dLossatRH);
                nRet = 0;
            }

            return (nRet);
        }

        /// <summary>
        /// This method calculates the propagation loss at a prescribed distance from a transmitter 
        /// using spherical earth equations and a prescribed atmospheric aborption coefficient (dB per km).
        /// </summary>
        /// <param name="dDist"></param>
        /// <param name="tArgs"></param>
        /// <returns></returns>
        public static double LossBySEL(double dDist, Tpass tArgs)
        {
            return (PLSEht(tArgs.mFreqMHz, dDist, tArgs.mHorDist) + tArgs.mLa * dDist);
        }

        /// <summary>
        /// This method calculates the spherical earth losses for a prescribed 
        /// frequency and radio horizon.
        /// </summary>
        /// <param name="dFMHz"></param>
        /// <param name="dDist"></param>
        /// <param name="dDistHorizon"></param>
        /// <returns></returns>
        public static double PLSEht(double dFMHz, double dDist, double dDistHorizon)
        {
            double dtheta = (dDist - dDistHorizon) / 8.5; // theta1(dDist, dDistHorizon);

            if (dDist <= 0.0 || dFMHz <= 0.0)
            {
                return (0.0);
            }

            return (29.73 +
                     30.0 * Math.Log10(dFMHz) +
                     10.0 * Math.Log10(dDist) +
                     30.0 * Math.Log10(dtheta) +
                     NAB(dDist, dtheta)
                   );
        }

        /// <summary>
        /// This method returns the value of the subfunction NAB() used in the spherical
        /// earth calculation method PLSEht(): 
        /// <br>NAB  =  20*Log10(5 + (0.27*Theta * Dist)/4000) + (0.00125 * Theta<sup>2</sup>)
        /// </summary>
        /// <param name="dDist"></param>
        /// <param name="dtheta"></param>
        /// <returns></returns>
        public static double NAB(double dDist, double dtheta)
        {
            double A = dtheta * dDist / 4000.0;
            double B = 0.001063 * (dtheta * dtheta);

            //assert(dDist > 0.00);
            if (dDist <= 0.0)
            {
                string str = "\n\nPFDcont.NAB(): ERROR: dDist is not positive: dDist = " + dDist;
                Log2.e(str);
                Console.Write(str);
                Application.ExitQuietly(1666);
            }

            return (20.0 * Math.Log10(5.0 + (0.27 * A)) + (1.17261 * B));
        }

        /// <summary>
        /// This method returns the effective transmitter gain as the antenna gain minus 
        /// the minimum disc; the transmitting polarization is also returned via the nPol argument.
        /// </summary>
        /// <param name="dGain"></param>
        /// <param name="tDisc"></param>
        /// <param name="nPol"></param>
        /// <returns></returns>
        public static double Gtx(double dGain, SuAntd tDisc, out int nPol)
        {
            double dMin = tDisc.dcov;

            nPol = 1;       /* 0 - horizontal tx polarization, 1 - vertical */
                            /*  Get the minimum discrimination */
            if (dMin >= tDisc.dxpv)
            {
                dMin = tDisc.dxpv;
            }
            if (dMin >= tDisc.dcoh)
            {
                dMin = tDisc.dcoh;

                nPol = 0;
            }
            if (dMin >= tDisc.dxph)
            {
                dMin = tDisc.dxph;

                nPol = 0;
            }

            return (dGain - dMin);
        }

        /// <summary>
        /// This is the principal top-level 'worker' method of the PFDcont program
        /// that performs all the logic and calculation required to append a new 
        /// (lat, long) point to the results list.
        /// </summary>
        /// <param name="mApTable"></param>
        /// <param name="ftabAzIn"></param>
        /// <param name="pFD"></param>
        /// <param name="ptRowDisc"></param>
        /// <returns></returns>
        public static int AddPoint(ref List<TtabRow> mApTable,
                                    double ftabAzIn,
                                    PFD pFD,
                                    SuAntd ptRowDisc)
        {
            double pfdmindist = 0.0;
            double pfdmaxdist = 0.0;
            double dMinLat;
            double dMinLong;
            double dMaxLat;
            double dMaxLong;
            string minlat;
            string minlong;
            string maxlat;
            string maxlong;
            string orientlat;
            string orientlong;
            double dGtx;
            double ftabAz = ftabAzIn;

            int nRet;
            int nPolarization;

            double dLat = pFD.Latitude;
            double dLong = pFD.Longitude;
            double fptx = pFD.TxPwr;
            double fsl = pFD.Fsl;
            double bandwidth = pFD.Bandwidth;
            double frequency = pFD.Frequency;
            double minpfdlevel = pFD.MinPFDlevel;
            double maxpfdlevel = pFD.MaxPFDlevel;
            double ftxheight = pFD.TxHeight;
            double frxheight = pFD.RxHeight;
            double dLa = pFD.AtmosAtten;
            double dAntGain = pFD.AnteGain;
            PFDloss eLossIn = pFD.Eloss;
            PFDclimate eClim = pFD.Climate;
            PFDcalcMode eCalcModeIn = pFD.CalcMode;

            /*  Get the gain of the antenna along this azimuth */
            dGtx = Gtx(dAntGain, ptRowDisc, out nPolarization);

            // This is only here for developmental logging.
            string str = String.Format("\n\nAddpoint-{0} {1}>Az: {2,5:F1}deg Gain: {3,5:F1}",
                                        (eCalcModeIn == PFDcalcMode.PFDCONTOUR ? "PfdContour" : "Coverage"),
                                        (eLossIn == PFDloss.SPHERICAL ? "Spherical Earth" : "Terrain"),
                                        ftabAz, dGtx);
            //...Log2.v(str);

            if (eLossIn == PFDloss.SPHERICAL)
            {

                /*	-------- Spherical Earth Losses ----------- */
                /*  Now calculate the distance to the min and max pfd contour */
                nRet = DistPFD(ref pfdmindist, fptx, fsl, bandwidth, dGtx, frequency,
                               minpfdlevel, ftxheight, frxheight, dLa);
                if (nRet != Constant.SUCCESS)
                {
                    if (mDistError == 0)
                    {
                        Console.Write("\r\n* Can't get min distance for azimuth {0,6:F0}\r\n", ftabAz);
                    }
                    mDistError++;
                    pfdmindist = 0.0;
                }

                if (eCalcModeIn == PFDcalcMode.PFDCONTOUR)
                {
                    /*  Now get the max distance */
                    nRet = DistPFD(ref pfdmaxdist, fptx, fsl, bandwidth, dGtx, frequency,
                                   maxpfdlevel, ftxheight, frxheight, dLa);
                    if (nRet != 0)
                    {
                        if (mDistError == 0)
                        {
                            Console.Write("\r\nCan't get max distance for azimuth {0,6:F0}\r\n", ftabAz);
                        }
                        mDistError++;
                        pfdmaxdist = 0.0;
                    }
                }

            }
            else if (eLossIn == PFDloss.TERRAIN)
            {
                /*	---------------- Terrain Losses ------------------------------- */

                if (eCalcModeIn == PFDcalcMode.PFDCONTOUR)
                {
                    /*	-----------  Terrain Losses with pfdcontours -------------- */
                    /*  First calculate the distance to the min and max pfd contour */
                    nRet = DistOHL(ref pfdmindist, dLat, dLong, ftabAz, fptx, fsl,
                                                   bandwidth, dGtx, frequency, minpfdlevel, ftxheight,
                                                   frxheight, dLa, nPolarization, eClim, eCalcModeIn);
                    if (nRet != 0)
                    {
                        if (mDistError == 0)
                        {
                            Console.Write("\r\n*** Can't get min distance using ohl for azimuth {0,6:F2}.\r\n", ftabAz);
                        }
                        mDistError++;
                        pfdmindist = 0.0;
                    }

                    /*  Now get the max distance */
                    nRet = DistOHL(ref pfdmaxdist, dLat, dLong, ftabAz, fptx, fsl,
                                                   bandwidth, dGtx, frequency, maxpfdlevel, ftxheight,
                                                   frxheight, dLa, nPolarization, eClim, eCalcModeIn);
                    if (nRet != 0)
                    {
                        if (mDistError == 0)
                        {
                            Console.Write("\r\n*** Can't get max distance using ohl for azimuth {0,6:F0}\r\n", ftabAz);
                        }
                        mDistError++;
                        pfdmaxdist = 0.0;
                    }

                    //...Log2.v("\nCalc.AddPoint(): PFDCONTOUR, TERRAIN: pfdmindist = " + pfdmindist);
                    //...Log2.v("\nCalc.AddPoint(): PFDCONTOUR, TERRAIN: pfdmaxdist = " + pfdmaxdist);

                }
                else
                {
                    /*	----------- Terrain losses with Coverage Contours --------------  */
                    /*	Calculate the minimum coverage distance using the ohloss routines */

                    /*  calculate the distance to the 50%  contour */
                    nRet = DistOHL(ref pfdmindist, dLat, dLong, ftabAz, fptx, fsl,
                                                   bandwidth, dGtx, frequency, minpfdlevel, ftxheight,
                                                   frxheight, dLa, nPolarization, eClim, PFDcalcMode.COVERAGE50);
                    if (nRet != 0)
                    {
                        if (mDistError == 0)
                        {
                            Console.Write("\r\n*** Can't get 50% distance using ohl for azimuth {0,6:F0}\r\n", ftabAz);
                        }
                        mDistError++;
                        pfdmindist = 0.0;
                    }

                    /*  calculate the distance to the 10%  contour.  */
                    nRet = DistOHL(ref pfdmaxdist, dLat, dLong, ftabAz, fptx, fsl,
                                                   bandwidth, dGtx, frequency, maxpfdlevel, ftxheight,
                                                   frxheight, dLa, nPolarization, eClim, PFDcalcMode.COVERAGE90);
                    if (nRet != 0)
                    {
                        if (mDistError == 0)
                        {
                            Console.Write("\r\n*** Can't get 90% distance using ohl for azimuth {0,6:F0}\r\n", ftabAz);
                        }
                        mDistError++;
                        pfdmaxdist = 0.0;
                    }
                }

            }

            /*  Calculate the lats and longs for the contour point. */
            LLFromDist(dLat, dLong, pfdmindist, ftabAz, out dMinLat, out dMinLong);

            /*  Convert to strings */
            GenUtil.UtLatConvStr((int)(dMinLat * 360000.0), out minlat, out orientlat);
            GenUtil.UtLongConvStr((int)(dMinLong * 360000.0), out minlong, out orientlong);

            /******************************************************************
            *
            *		Add a new row to the table.
            *
            *******************************************************************/
            mApTable.Add(new TtabRow());

            mCurrRow++;         /* mCurrRow always points to the last row added */

            /*	Fill in the common fields and the minimum distance */
            mApTable[mCurrRow].dAz = ftabAz;
            mApTable[mCurrRow].dGain = dGtx;
            mApTable[mCurrRow].dmindist = pfdmindist;
            mApTable[mCurrRow].dminlat = dMinLat;
            mApTable[mCurrRow].dminlong = dMinLong;
            mApTable[mCurrRow].cminlat = minlat;
            mApTable[mCurrRow].cminlong = minlong;

            if (eCalcModeIn == PFDcalcMode.PFDCONTOUR)
            {
                /*	If we are dealing with the Power Flux Density Contours then we need to
                *		specify both minimum and maximum */
                LLFromDist(dLat, dLong, pfdmaxdist, ftabAz, out dMaxLat, out dMaxLong);

                GenUtil.UtLatConvStr((int)(dMaxLat * 360000.0), out maxlat, out orientlat);
                GenUtil.UtLongConvStr((int)(dMaxLong * 360000.0), out maxlong, out orientlong);

                /*	Put them into the table */
                mApTable[mCurrRow].dmaxdist = pfdmaxdist;
                mApTable[mCurrRow].dmaxlat = dMaxLat;
                mApTable[mCurrRow].dmaxlong = dMaxLong;
                mApTable[mCurrRow].cmaxlat = maxlat;
                mApTable[mCurrRow].cmaxlong = maxlong;
            }
            else
            {
                /*	With coverage contours, if the losses are spherical we only need one
                *		column.  Otherwise the second column is the 90% value          */
                if (eLossIn == PFDloss.TERRAIN)
                {
                    /*	Not spherical.  Present the 90% contour */
                    LLFromDist(dLat, dLong, pfdmaxdist, ftabAz, out dMaxLat, out dMaxLong);

                    GenUtil.UtLatConvStr((int)(dMaxLat * 360000.0), out maxlat, out orientlat);
                    GenUtil.UtLongConvStr((int)(dMaxLong * 360000.0), out maxlong, out orientlong);

                    /*	Add to table */
                    mApTable[mCurrRow].dmaxdist = pfdmaxdist;
                    mApTable[mCurrRow].dmaxlat = dMaxLat;
                    mApTable[mCurrRow].dmaxlong = dMaxLong;
                    mApTable[mCurrRow].cmaxlat = maxlat;
                    mApTable[mCurrRow].cmaxlong = maxlong;
                }
                else
                {
                    /*	Spherical losses for the coverage.  No second column */
                    pfdmaxdist = 0.0;
                    maxlat = "";
                    maxlong = "";
                    dMaxLat = 0.0;
                    dMaxLong = 0.0;

                    /*	Lats and longs of zero indicate a null entry */
                    mApTable[mCurrRow].dmaxdist = pfdmaxdist;
                    mApTable[mCurrRow].dmaxlat = dMaxLat;
                    mApTable[mCurrRow].dmaxlong = dMaxLong;
                    mApTable[mCurrRow].cmaxlat = maxlat;
                    mApTable[mCurrRow].cmaxlong = maxlong;
                }
            }

            return (Constant.SUCCESS);
        }


        /// <summary>
        /// This method converts a prescribed absolute (North-zero) azimuth to a relative 
        /// azimuth w.r.t. a pattern pointing along the antenna's main beam.
        /// </summary>
        /// <param name="fAz"></param>
        /// <param name="fAntAz"></param>
        /// <returns></returns>
        public static double TruetoPat(double fAz, double fAntAz)
        {
            double fOutVal;

            fOutVal = fAz - fAntAz;
            if (fOutVal < 0.00)
            {
                fOutVal += 360.0;
            }
            return (fOutVal);
        }

        /// <summary>
        /// This method converts a prescribed relative azimuth (w.r.t. a pattern pointing 
        /// along an antenna's main beam) to an absolute (North-zero) azimuth. 
        /// </summary>
        /// <param name="dPatAz"></param>
        /// <param name="dAntAz"></param>
        /// <returns></returns>
        public static double PatToTrue(double dPatAz, double dAntAz)
        {
            double dOutVal = dPatAz + dAntAz;

            if (dOutVal >= 360.0)
            {
                dOutVal -= 360.0;
            }
            return (dOutVal);
        }

        /// <summary>
        /// This method gets the next pattern beyond a given azimuth, or from a given pattern element.
        /// </summary>
        /// <param name="ptAntDsc"></param>
        /// <param name="nPatCt"></param>
        /// <param name="dPatAz"></param>
        /// <param name="nLastPat"></param>
        /// <returns></returns>
        public static int NextPat(SuAntd[] ptAntDsc, int nPatCt, double dPatAz, int nLastPat)
        {
            int nFoundPat;

            if (nLastPat == Constant.NOT_INITIALIZED)
            {
                /*	Initialize -- scan for the right azimuth */
                nFoundPat = StepThrough(ptAntDsc, nPatCt, dPatAz);
            }
            else
            {
                /*	This is not initialization, we just want the next one */
                nFoundPat = StepNext(ptAntDsc, nPatCt, nLastPat);
            }
            return (nFoundPat);
        }

        /// <summary>
        /// This method steps through a prescribed antenna pattern looking for the 
        /// first azimuth just greater than the required azimuth taking pattern symmetry into account.
        /// </summary>
        /// <param name="ptAntDsc"></param>
        /// <param name="nPatCt"></param>
        /// <param name="dPatAz"></param>
        /// <returns></returns>
        public static int StepThrough(SuAntd[] ptAntDsc, int nPatCt, double dPatAz)
        {
            bool isFound = false;
            int nInd;
            int nStep = -1;

            for (nInd = 0; nInd < nPatCt; nInd++)
            {
                if (ptAntDsc[nInd].antang > dPatAz)
                {
                    /*	It was the last one */
                    isFound = true;
                    break;
                }
            }

            nStep = nInd;  // Assign whether found or not.

            // Handle symmetry.  If the azimuth is not found, then check
            // to see if this is a symmetric pattern.
            if (!isFound)
            {
                if (ptAntDsc[nPatCt - 1].antang <= 180.0)
                {
                    /* We have a symmetric pattern, and we haven't found the azimuth yet */
                    for (nInd = nPatCt - 1; nInd >= 0; nInd--)
                    {
                        if ((360.0 - ptAntDsc[nInd].antang) > dPatAz)
                        {
                            /* 	The next one is too far */
                            isFound = true;
                            break;
                        }
                    }
                    nStep = -nInd;
                }
            }

            // When we get here nStep will have the index of the pattern element
            // containing the element just prior to the desired azimuth.  If the
            // pattern is symmetric and the selection is on the reverse, then nStep
            // will be negative.
            if (isFound)
            {
                return (nStep);
            }
            else
            {
                // If not found, then return one more than the number in the pattern.
                return (nPatCt + 1);
            }
        }

        /// <summary>
        /// This method gets the next pattern element taking into account that the pattern 
        /// may be symmetrical and the continuous 'loop around' from 360 to 0.
        /// </summary>
        /// <param name="ptAntDsc"></param>
        /// <param name="nPatCt"></param>
        /// <param name="nLastPat"></param>
        /// <returns></returns>
        public static int StepNext(SuAntd[] ptAntDsc, int nPatCt, int nLastPat)
        {
            // Add one to nLastPat.  If it is positive it moves it up the array, and
            // if negative, if moves it down to zero.
            nLastPat++;

            if (nLastPat >= nPatCt)
            {
                /*	Gone over the end.  Check for a symmetric pattern, and if it is
                *		set the nLastPat to neg, otherwise, set it to zero */
                if (ptAntDsc[nPatCt - 1].antang <= 180.0)
                {
                    /* 	Symmetric.  Start back down */
                    nLastPat = -(nPatCt - 1);
                }
                else
                {
                    /*	Is is not symmetric, so start it at 0 */
                    nLastPat = 0;
                }
            }
            return (nLastPat);
        }

        /// <summary>
        /// This method returns true if the calculation mode (prescribed by command
        /// line argument #2) is of the 'Coverage' type.
        /// </summary>
        /// <param name="calcMode"></param>
        /// <returns></returns>
        public static bool IsCoverage(PFDcalcMode calcMode)
        {
            return (calcMode == PFDcalcMode.COVERAGE50) || (calcMode == PFDcalcMode.COVERAGE90);
        }

    }
}
