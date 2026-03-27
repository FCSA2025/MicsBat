using _Configuration;
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using System.Runtime.InteropServices;

namespace _Utillib
{
    using _NewLib;
    using SQLLEN = Int64;
    public class TpGetDat
    {
#if PINVOKE
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int tpGetCtxInfo([In] string intTraf,
                                                [In] string vicTraf,
                                                [In] string vicEqpt,
                                                [In] string tempEqpt,
                                                [In, Out] CtxStruct curCtx,
                                                [In] string intPrintMsg,
                                                [In] string vicPrintMsg);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static void tpCalcSepReqd([In] string calctype,
                                                    [In, Out] float[,] curveCI,
                                                    [In] double fsepHi,
                                                    [In] double fsepMid,
                                                    [In] double fsepLo,
                                                    [In] int numPts,
                                                    [In, Out] ref double freqsep,
                                                    [In, Out] ref double reqdcalc);

        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int tpCalcDisc([In] string acode,
                                               [In] double offax,
                                               [In, Out] ref double adisccv,
                                               [In, Out] ref double adiscxv,
                                               [In, Out] ref double adiscch,
                                               [In, Out] ref double adiscxh,
                                               [In, Out] ref SQLLEN nullCv,
                                               [In, Out] ref SQLLEN nullXv,
                                               [In, Out] ref SQLLEN nullCh,
                                               [In, Out] ref SQLLEN nullXh,
                                               [In] string intPrintMsg,
                                               [In] string vicPrintMsg);

        public static void TpCalcSepReqd_NATIVE(string calctype,
                                float[,] curveCI,
                                double fsepHi,
                                double fsepMid,
                                double fsepLo,
                                int numPts,
                                out double freqsep,
                                out double reqdcalc)
        {
            // 'out' requirements.
            freqsep = 0.0;
            reqdcalc = 0.0;

            // Native call.
            tpCalcSepReqd(calctype, curveCI, fsepHi, fsepMid, fsepLo, numPts,
                                    ref freqsep, ref reqdcalc);
        }

        public static int TpGetCtxInfo_NATIVE(string intTraf,
                      string vicTraf,
                      string vicEqpt,
                      string tempEqpt,
                      ref CtxStruct curCtx,
                      string intPrintMsg,
                      string vicPrintMsg)
        {
            int nRet;

            nRet = tpGetCtxInfo(intTraf, vicTraf, vicEqpt, tempEqpt, curCtx, intPrintMsg, vicPrintMsg);

            //...Log2.v("\nTpGetDat.TpGetCtxInfo_NATIVE(): nRet = " + nRet);

            return nRet;
        }

        public static int TpCalcDisc_NATIVE(string acode,
                 double offax,
                 ref double adisccv,
                 ref double adiscxv,
                 ref double adiscch,
                 ref double adiscxh,
                 ref SQLLEN nullCv,
                 ref SQLLEN nullXv,
                 ref SQLLEN nullCh,
                 ref SQLLEN nullXh,
                 string intPrintMsg,
                 string vicPrintMsg)
        {
            return tpCalcDisc(acode,
                                        offax,
                                        ref adisccv,
                                        ref adiscxv,
                                        ref adiscch,
                                        ref adiscxh,
                                        ref nullCv,
                                        ref nullXv,
                                        ref nullCh,
                                        ref nullXh,
                                        intPrintMsg,
                                        vicPrintMsg);
        }


#endif
        private static CtxStruct[] ctxSaved = Arrays.CreateArrayUsingDefaultElementConstructor<CtxStruct>(Constant.MAX_SAVED_CTX);

        private static bool ctxSavedIsInitialized = false;

        private static PatternStruct[] patternsSaved = new PatternStruct[Constant.MAX_SAVED_PATTS];
        private static bool init = false;

        /// <summary>
        /// This method controls the searching and loading of an antenna pattern based 
        /// on the acode and table supplied.  
        /// </summary>
        /// <param name="acode"></param>
        /// <param name="patDat"></param>
        /// <returns></returns>
        public static int TpGetPattern(string acode, out PatternStruct patDat)
        {
            // 'out' requirement.
            patDat = null;

            //&&Console.Error.Write("\ntpGetDat.tpGetPattern(): top, acode = " + acode);

            int i;
            int rc;
            int minUsed = 9999;
            int patIndex = 0;
            bool newData = false;

            if (String.IsNullOrWhiteSpace(acode))
            {
                return (Error.NOANTDFOUND);
            }

            if (!init)
            {
                for (i = 0; i < Constant.MAX_SAVED_PATTS; i++)
                {
                    patternsSaved[i] = new PatternStruct();
                    patternsSaved[i].useCounter = -1;
                }
                init = true;
            }
            for (i = 0; i < Constant.MAX_SAVED_PATTS; i++)
            {
                if (patternsSaved[i].acode.Equals(acode))
                {
                    patIndex = i;
                    newData = false;
                    (patternsSaved[i].useCounter)++;
                    break;
                }
                else if (patternsSaved[i].useCounter == -1)
                {
                    patIndex = i;
                    newData = true;
                    (patternsSaved[i].useCounter)++;
                    break;
                }
                if (minUsed > patternsSaved[i].useCounter)
                {
                    minUsed = patternsSaved[i].useCounter;
                    patIndex = i;
                }
            }

            if (i == Constant.MAX_SAVED_PATTS)
            {
                //&&Console.Error.Write("\ntpGetDat.tpGetPattern(): frog");
                newData = true;
                patternsSaved[patIndex].useCounter = 0;
            }

            //&&Console.Error.Write("\ntpGetDat.tpGetPattern(): lion");
            if (newData == true)
            {
                /* load pattern into patternsSaved[patIndex] */
                if ((rc = LoadPattern(acode, out patternsSaved[patIndex])) != Constant.SUCCESS)
                {
                    Log2.e("\nTpGetDat.TpGetPattern(): ERROR: call to LoadPattern() failed");
                    return (rc);
                }
            }

            patDat = patternsSaved[patIndex].DeepCopy();

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// LoadPattern is the routine loads data points (antd records) from the 
        /// specified table with the given acode into the patternStruct.  
        /// </summary>
        /// <param name="acode"></param>
        /// <param name="patDat"></param>
        /// <returns></returns>
        public static int LoadPattern(string acode, out PatternStruct patDat)
        {
            // 'out' requirement.
            patDat = null;

            int numPts;
            string tmpMax;

            SuAntStr pAnt;
            int nRet;

            //&&Console.Error.Write("\nTgt10");
            nRet = Suutils.SuGetAnt(acode, out pAnt);
            if (nRet != Constant.SUCCESS)
            {
                if (nRet == Constant.NOMORERECS)
                {
                    return (Error.NOANTEFOUND);
                }
                else
                {
                    return (Error.DYN_MS_SQL_SERVER_ERR);
                }
            }

            patDat = new PatternStruct();

            for (numPts = 0; numPts < pAnt.acAnt.anip; numPts++)
            {
                // AH: REMOVE!
                if (pAnt.acDscPtr[numPts] == null)
                {
                    //...Log2.v("\npAnt.acDscPtr[" + numPts + "] is null");
                }
                patDat.pattern[numPts, 0] = pAnt.acDscPtr[numPts].antang;
                patDat.pattern[numPts, 1] = pAnt.acDscPtr[numPts].dcov;
                patDat.pattern[numPts, 2] = pAnt.acDscPtr[numPts].dxpv;
                patDat.pattern[numPts, 3] = pAnt.acDscPtr[numPts].dcoh;
                patDat.pattern[numPts, 4] = pAnt.acDscPtr[numPts].dxph;

                if (numPts >= Constant.MAX_ANT_PTS)
                {
                    tmpMax = String.Format("{0}", Constant.MAX_ANT_PTS);
                    ErrMsg.UtPrintMessage(Error.MAXANTES, tmpMax);
                    break;
                }
            }
            if (numPts == 0)
            {
                return (Error.NOANTDFOUND);
            }

            patDat.nulls[Constant.DCOV] = Constant.DB_NOT_NULL; //pnulls[DCOV];
            patDat.nulls[Constant.DXPV] = Constant.DB_NOT_NULL; //pnulls[DXPV];
            patDat.nulls[Constant.DCOH] = Constant.DB_NOT_NULL; //pnulls[DCOH];
            patDat.nulls[Constant.DXPH] = Constant.DB_NOT_NULL; //pnulls[DXPH];

            patDat.numPts = numPts;
            patDat.acode = acode;

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Calculates the required antenna discrimination values from the 
        /// given antenna pattern at the specified off-axis angle. It uses the antenna 
        /// pattern in the input structure, so does not go to the database.  
        /// </summary>
        /// <param name="pAnt"></param>
        /// <param name="offax"></param>
        /// <param name="adisccv"></param>
        /// <param name="adiscxv"></param>
        /// <param name="adiscch"></param>
        /// <param name="adiscxh"></param>
        /// <param name="nullCv"></param>
        /// <param name="nullXv"></param>
        /// <param name="nullCh"></param>
        /// <param name="nullXh"></param>
        /// <param name="intPrintMsg"></param>
        /// <param name="vicPrintMsg"></param>
        /// <returns></returns>
        public static int CalcDisc(SuAntStr pAnt,
             double offax,
             out double adisccv,
             out double adiscxv,
             out double adiscch,
             out double adiscxh,
             out SQLLEN nullCv,
             out SQLLEN nullXv,
             out SQLLEN nullCh,
             out SQLLEN nullXh,
             string intPrintMsg,
             string vicPrintMsg)
        {
            // 'out' requirements.
            adisccv = 0.0;
            adiscxv = 0.0;
            adiscch = 0.0;
            adiscxh = 0.0;
            nullCv = Constant.DB_NULL;
            nullXv = Constant.DB_NULL;
            nullCh = Constant.DB_NULL;
            nullXh = Constant.DB_NULL;

            int rc;
            double dAngle = Abs(offax);
            SuAntd tOutDisc;

            /* using antenna pattern and interferer's off-axis angle find
             * antenna discriminations for the transmit (tx) antenna -
             * co and cross polar, horizontal and vertical
             */

            rc = Suutils.InterpADisc(pAnt.acDscPtr, (float)dAngle, pAnt.acAnt.anip, out tOutDisc);
            if (rc == Constant.SUCCESS)
            {
                adisccv = tOutDisc.dcov;
                adiscxv = tOutDisc.dxpv;
                adiscch = tOutDisc.dcoh;
                adiscxh = tOutDisc.dxph;

                nullCv = Constant.DB_NOT_NULL;
                nullXv = Constant.DB_NOT_NULL;
                nullCh = Constant.DB_NOT_NULL;
                nullXh = Constant.DB_NOT_NULL;
            }

            return (rc);
        }

        /// <summary>
        /// Performs a binary search for a given value in given array.  
        /// </summary>
        /// <remarks>
        /// NOTE: the array must be sorted in ascending order. Notes: If the given 
        /// value itself is found in the array its position in the array is returned in 
        /// both hi and lo. Otherwise hi and lo are set to the positions between which 
        /// the value would be inserted in order to maintain an ascending sorted list. 
        /// If the search value is less than the first in the sorted list, hi and lo 
        /// are set to 0, the first element. If the value is greater than the last in 
        /// the sorted list, hi and lo are set to the last element.  
        /// </remarks>
        /// <param name="angs"> - list of ascending sorted values</param>
        /// <param name="numAngs"> - number of elements in the list</param>
        /// <param name="angle"> - value to be searched for</param>
        /// <param name="hi"> - position of value < 'angle'</param>
        /// <param name="lo"> - position of value > 'angle'</param>
        public static void BinSearch(float[] angs,    /* input  - list of ascending sorted values */
                             int numAngs,   /* input  - number of elements in the list */
                             float angle,   /* input  - value to be searched for */
                             out int hi,       /* output - position of value < 'angle' */
                             out int lo)       /* output - position of value > 'angle' */
        {
            int mid;        /* value at midpoint of current search area */

            hi = numAngs - 1;  /* set hi to last element -zero based indexing*/
            lo = 0;        /* set lo to the first element */

            if (angle < angs[lo])
            {
                /* angle is less than the first elemenet */
                hi = lo;
            }
            else if (angle > angs[hi])
            {
                /* angle is greater than the last element */
                lo = hi;
            }
            else
            {
                while (lo + 1 != hi)
                {
                    /* continue search while not at adjacent elements */
                    numAngs = hi - lo;

                    /* compute midpoint of search area */
                    mid = lo + (int)Floor(numAngs / 2.0);

                    if (angs[mid] == angle)
                    {
                        /* search element is the midpoint value */
                        hi = mid;
                        lo = hi - 1;
                    }
                    else if (angs[mid] > angle)
                    {
                        /* search element is > midpoint value */
                        hi = mid;
                    }
                    else
                    {
                        /* search element is < midpoint value */
                        lo = mid;
                    }
                }
            }
        }

        /// <summary>
        /// This method finds the midband freq's for the given 2 bandcodes.  
        /// </summary>
        /// <param name="bndcd1"></param>
        /// <param name="bndcd2"></param>
        /// <param name="midBnd1"></param>
        /// <param name="midBnd2"></param>
        /// <param name="intPrintMsg"></param>
        /// <param name="vicPrintMsg"></param>
        /// <returns></returns>
        public static int TpCalcMbnds(string bndcd1,
                                string bndcd2,
                                out double midBnd1,
                                out double midBnd2,
                                string intPrintMsg,
                                string vicPrintMsg)
        {
            //...Log2.v("\nTpGetDat.TpCalcMbnds(): Entry");

            // 'out' requirements.
            midBnd1 = 0.0;
            midBnd2 = 0.0;

            SuBand curBand;

            /* get midband freq from SDB for interferer */
            if (Suutils.SuGetBand(bndcd1, out curBand) != 0)
            {
                Log2.e("\nTpGetDat.TpCalcMbnds(): Exit: ERROR: A");
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.INVALIDBANDCODE, bndcd1);
                return (Constant.FAILURE);
            }

            midBnd1 = curBand.bmidf;

            /* get midband freq from SDB for victim */
            if (Suutils.SuGetBand(bndcd2, out curBand) != 0)
            {
                Log2.e("\nTpGetDat.TpCalcMbnds(): Exit: ERROR: B");
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.INVALIDBANDCODE, bndcd2);
                return (Constant.FAILURE);
            }

            midBnd2 = curBand.bmidf;

            //...Log2.v("\nTpGetDat.TpCalcMbnds(): Exit: " + midBnd1 + "  " + midBnd2);
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method reads the interferer and victim equiptment stabilities from 
        /// the equiptment table for the given interferer and victim ecodes.  
        /// </summary>
        /// <param name="intEqpt"></param>
        /// <param name="vicEqpt"></param>
        /// <param name="tempEqpt"></param>
        /// <param name="intEqpStabil"></param>
        /// <param name="vicEqpStabil"></param>
        /// <param name="intPrintMsg"></param>
        /// <param name="vicPrintMsg"></param>
        /// <returns></returns>
        public static int TpGetEqptStab(string intEqpt,
                                    string vicEqpt,
                                    string tempEqpt,
                                    out double intEqpStabil,
                                    out double vicEqpStabil,
                                    string intPrintMsg,
                                    string vicPrintMsg)
        {
            /* reset the stability values */
            intEqpStabil = 0.0;
            vicEqpStabil = 0.0;

            string select;
            SuEqpt tEqpt;
            int nRet;

            if ((nRet = Suutils.SuGetEqpt(intEqpt, out tEqpt)) != 0)
            {
                if (nRet == Constant.NOMORERECS)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    select = String.Format("ecode = '{0}'", intEqpt);
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "EQPT", select);
                    return (Constant.FAILURE);
                }
                else
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    Console.WriteLine("Error Get Equipment returned {0}.\n", nRet);
                    return (Error.DYN_MS_SQL_SERVER_ERR);
                }
            }

            /* Eqpt Stabilities stored as a percent */
            intEqpStabil = tEqpt.estab / 100.0;

            /* get eqpt stability for victim */
            if ((nRet = Suutils.SuGetEqpt(vicEqpt, out tEqpt)) != 0)
            {
                if (nRet == Constant.NOMORERECS)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    select = String.Format("ecode = '{0}'", vicEqpt);
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "EQPT", select);
                    return (Constant.FAILURE);
                }
                else
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    Console.Write("Error: Get Equipment(3) returned {0}.\n", nRet);
                    return (Error.DYN_MS_SQL_SERVER_ERR);
                }
            }
            /* Eqpt Stabilities stored as a percent */
            vicEqpStabil = tEqpt.estab / 100.0;

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Calculates the maximum frequency separation, based upon the 
        /// distance between sites, coordination distance, and maximum frequency 
        /// separation entered in the parameters.  
        /// </summary>
        /// <param name="fsep"></param>
        /// <param name="coordist"></param>
        /// <param name="intvicdist"></param>
        /// <param name="inttraftx"></param>
        /// <param name="victrafrx"></param>
        /// <returns></returns>
        public static double TpMaxFSep(double fsep,
                                 double coordist,
                                 double intvicdist,
                                 string inttraftx,
                                 string victrafrx)
        {
            double newfsep;

            newfsep = fsep * 1000;
            /*
             * If the traffic type is PCS, the minimum maximum frequency separation
             * should be set to PCS_MAX_FSEP (currently 150), otherwise it should be
             * set to MAX_FSEP. This value is used only if the
             * distance between the two sites is less than half the coordination
             * distance and the Max. Freq. Separation provided in the parameter file
             * is less than this value.
             */
            /*	The following was removed after the MAX_FSEP was changed to 2000MHz.
            *		It was causing too many problems at high frequencies and short distances.
            *		The PCS logic was retained as it is not used any more.
            *		GJS - 2012.12 */
            double maxfsep;
            if (Strings.FirstCharIs(inttraftx, 'P') || Strings.FirstCharIs(victrafrx, 'P'))
            {
                maxfsep = Constant.PCS_MAX_FSEP;
                if ((intvicdist > (0.5 * coordist)) || (newfsep > maxfsep))
                {
                    maxfsep = newfsep;
                }
            }
            else
            {
                // Normal case
                maxfsep = newfsep;
            }

            return maxfsep;
        }

        /// <summary>
        /// This method controls finding and storing the all ctx data required ie. ctx 
        /// pattern and rqwrst and calc type: C/I or -I.  
        /// </summary>
        /// <remarks>
        /// This method manages the selection of a CTX pattern based on the supplied 
        /// trafic TX and RX and eqpt TX and RX. This selection is based on a series of 
        /// 12 steps. Processing is complete when a pattern is found at one of the 12 
        /// steps or all 12 steps result in no pattern being found and then a default 
        /// pattern is used. 
        /// </remarks>
        /// <param name="intTraf"></param>
        /// <param name="vicTraf"></param>
        /// <param name="vicEqpt"></param>
        /// <param name="tempEqpt"></param>
        /// <param name="curCtx"></param>
        /// <param name="intPrintMsg"></param>
        /// <param name="vicPrintMsg"></param>
        /// <returns></returns>
        public static int TpGetCtxInfo(string intTraf,
                     string vicTraf,
                     string vicEqpt,
                     string tempEqpt,
                     ref CtxStruct curCtx,
                     string intPrintMsg,
                     string vicPrintMsg)
        {

            int rc = 0; ;

            curCtx.ctxtraftx = "";
            curCtx.ctxtrafrx = "";
            curCtx.ctxeqpt = "";
            curCtx.calcType = "";

            intTraf = intTraf.Trim();
            vicTraf = vicTraf.Trim();
            vicEqpt = vicEqpt.Trim();

            //...Log2.v("\ntpGetDat.tpGetCtxInfo(): Entry");
            //...Log2.v(String.Format("\ntpGetDat.tpGetCtxInfo(): intTraf = {0}", intTraf));

            if (!intTraf.Equals(Constant.MAX_FSEP_STR))
            {
                //...Log2.v("\ntpGetDat.tpGetCtxInfo(): A");
                if ((rc = TpGetCtx(tempEqpt, intTraf, vicTraf, vicEqpt,
                                   out curCtx.ctxtraftx, out curCtx.ctxtrafrx, out curCtx.ctxeqpt,
                                                     intPrintMsg, vicPrintMsg)) != Constant.SUCCESS)
                {
                    //...Log2.v("\ntpGetDat.tpGetCtxInfo(): B");
                    if (rc == Error.DEFAULT_CTX)
                    {
                        //...Log2.v("\ntpGetDat.tpGetCtxInfo(): C");
                        //...Log2.v("\nTpGetDat.TpGetCtxInfo(): call to TpGetCtx() returned rc == Error.DEFAULT_CTX, then Exit");
                        return (rc);
                    }
                }
            }
            else
            {
                //...Log2.v("\ntpGetDat.tpGetCtxInfo(): D");
                /*	Return failure to run calculations. 1083 - GJS - 2003.12 */
                Log2.e("\nTpGetDat.TpGetCtxInfo(): ERROR: failure to run calculations");
                return (Constant.FAILURE);
            }

            //...Log2.v("\ntpGetDat.tpGetCtxInfo(): E");

            /* read rqwrst and decide which type of calcs to do: C/I or -I */
            rc = TpGetCtxData(curCtx.ctxtraftx, curCtx.ctxtrafrx,
                                   curCtx.ctxeqpt, out curCtx.calcType, out curCtx.rqco,
                                                         intPrintMsg, vicPrintMsg);

            if (rc != Constant.SUCCESS)
            {
                //...Log2.v("\ntpGetDat.tpGetCtxInfo(): F");
                if (rc == Error.NOCTXFOUND)
                {
                    //...Log2.v("\ntpGetDat.tpGetCtxInfo(): G");
                    //...Log2.v("\nTpGetDat.TpGetCtxInfo(): call to TpGetCtxData() returned NOCTXFOUND");
                    return (Constant.CONT_PROCESSING);
                }
                Log2.e("\nTpGetDat.TpGetCtxInfo(): ERROR: call to TpGetCtxData() returned: " + rc);
                return (rc);
            }

            //...Log2.v("\ntpGetDat.tpGetCtxInfo(): H");

            rc = TpGetCiCurve(ref curCtx, intPrintMsg, vicPrintMsg);

            //...Log2.v(String.Format("\ncurCtx.ctxtraftx = {0}", curCtx.ctxtraftx));
            //...Log2.v(String.Format("\ncurCtx.ctxtrafrx = {0}", curCtx.ctxtrafrx));
            //...Log2.v(String.Format("\ncurCtx.ctxeqpt = {0}", curCtx.ctxeqpt));

            if (rc != Constant.SUCCESS)
            {
                //...Log2.v("\ntpGetDat.tpGetCtxInfo(): I");
                if (rc == Error.NOCTXDPTS)
                {
                    //...Log2.v("\ntpGetDat.tpGetCtxInfo(): J");
                    Log2.e("\nTpGetDat.TpGetCtxInfo(): ERROR: call to TpGetCiCurve() returned NOCTXDPTS");
                    return (Constant.CONT_PROCESSING);
                }
                else
                {
                    //...Log2.v("\ntpGetDat.tpGetCtxInfo(): K");
                    Log2.e("\nTpGetDat.TpGetCtxInfo(): ERROR: call to TpGetCiCurve() returned: " + rc);
                    return (rc);
                }
            }

            //...Log2.v("\ntpGetDat.tpGetCtxInfo(): Exit");
            //...Log2.v("\nTpGetDat.TpGetCtxInfo(): Successful return");
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method controls the selection of a CTX pattern based on the supplied 
        /// trafic TX and RX and eqpt TX and RX. This selection is based on a series of 
        /// 12 steps. Processing is complete when a pattern is found at one of the 12 
        /// steps or all 12 steps result in no pattern being found and then a default 
        /// pattern is used. 
        /// </summary>
        /// <param name="intTraf"></param>
        /// <param name="vicTraf"></param>
        /// <param name="eqptType"></param>
        /// <param name="calcType"></param>
        /// <param name="rqco"></param>
        /// <param name="intPrintMsg"></param>
        /// <param name="vicPrintMsg"></param>
        /// <returns></returns>
        public static int TpGetCtxData(string intTraf,
                                 string vicTraf,
                                 string eqptType,
                                 out string calcType,
                                 out double rqco,
                                 string intPrintMsg,
                                 string vicPrintMsg)
        {
            // 'out' requirement.
            calcType = "";
            rqco = 0.0;

            double rqdWrst;

            SuCtxStruct pCtx;
            int nRet;

            nRet = Suutils.SuGetCtx(vicTraf, intTraf, eqptType, 1, out pCtx);
            if (nRet != 0)
            {
                return (Error.NOCTXFOUND);
                //} else if (nRet != 0) {
                //	ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                //	ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                //Console.Write("Error Get Ctx(2) returned: %d.\n", nRet);
                //	return(Error.DYN_MS_SQL_SERVER_ERR);
            }

            rqco = pCtx.CtxV.rqco;     /*	Store the co-channel requirement for 1181 - GJS */
            rqdWrst = pCtx.CtxV.rqwrst;

            if (rqdWrst < 0.0)
            {
                calcType = "-I";
            }
            else
            {
                calcType = "C/I";
            }
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method controls the selection of a CTX pattern based on the supplied 
        /// trafic TX and RX and eqpt TX and RX. This selection is based on a series of 
        /// 12 steps. Processing is complete when a pattern is found at one of the 12 
        /// steps or all 12 steps result in no pattern being found and then a default 
        /// pattern is used.  
        /// </summary>
        /// <param name="tempEqpt"></param>
        /// <param name="trafTypeTx"></param>
        /// <param name="trafTypeRx"></param>
        /// <param name="eqptType"></param>
        /// <param name="ctxTrafTx"></param>
        /// <param name="ctxTrafRx"></param>
        /// <param name="ctxEqpt"></param>
        /// <param name="intPrintMsg"></param>
        /// <param name="vicPrintMsg"></param>
        /// <returns></returns>
        public static int TpGetCtx(string tempEqpt,
               string trafTypeTx,
               string trafTypeRx,
               string eqptType,
               out string ctxTrafTx,
               out string ctxTrafRx,
               out string ctxEqpt,
               string intPrintMsg,
               string vicPrintMsg)
        {
            // 'out' requirements.
            ctxTrafTx = "";
            ctxTrafRx = "";
            ctxEqpt = "";

            //short	defaultEqpt = false;
            string tfciCTX;
            string tfcrCTX;
            string eqptCTX;
            int rc = Constant.FAILURE;

            /* a change here to make use of the new ctx_xref table - with this
             * table we can avoid steps 1-12 for any case that can be found
             * in the table - let's look for each case in the table and if found
             * simply return the xref values indicated - if not found then
             * simply use the default values
             */
            tfciCTX = trafTypeTx;
            tfcrCTX = trafTypeRx;
            eqptCTX = eqptType;

            tfciCTX = tfciCTX.Trim();
            tfcrCTX = tfcrCTX.Trim();
            eqptCTX = eqptCTX.Trim();

            //...Log2.v("\ntpGetDat.tpGetCtx(): Entry");

            if ((rc = GetCTXViaXref(tfciCTX, tfcrCTX, eqptCTX, out ctxTrafTx, out ctxTrafRx,
                                    out ctxEqpt, intPrintMsg, vicPrintMsg)) == Constant.SUCCESS)
            {
                //...Log2.v("\ntpGetDat.tpGetCtx(): A");
                /* found a ctx_xref record let's see if its valid */
                if ((rc = ValidCtx(ctxTrafTx, ctxTrafRx, ctxEqpt,
                                   intPrintMsg, vicPrintMsg)) != Constant.SUCCESS)
                {
                    //...Log2.v("\ntpGetDat.tpGetCtx(): B");

                    string str = String.Format("tpGetCtx: CTX Cross reference: {0} {1} {2} to {3} {4} {5}\n  Gives an invalid CTX curve.  Using Default",
                            tfciCTX, tfcrCTX, eqptCTX, ctxTrafTx, ctxTrafRx, ctxEqpt);
                    GenUtil.SetErr(str);
                    //...Log2.v("\nTpGetDat.TpGetCtx(): invalid CTX curve: " + str);
                    rc = Error.DEFAULT_CTX;
                }
            }

            //...Log2.v("\ntpGetDat.tpGetCtx(): Exit, final");
            return rc;
        }

        /// <summary>
        /// This method checks to see if the ctx pattern for the given traf TX and RX 
        /// and eqpt TX and RX exists.  
        /// </summary>
        /// <param name="ctxTrafTx"></param>
        /// <param name="ctxTrafRx"></param>
        /// <param name="ctxEqpt"></param>
        /// <param name="intPrintMsg"></param>
        /// <param name="vicPrintMsg"></param>
        /// <returns></returns>
        public static int ValidCtx(string ctxTrafTx,
                                        string ctxTrafRx,
                                        string ctxEqpt,
                                        string intPrintMsg,
                                        string vicPrintMsg)
        {
            int nRet;
            SuCtxStruct ptCtx;

            //...Log2.v("\ntpGetDat.validCtx(): Entry");

            nRet = Suutils.SuGetCtx(ctxTrafRx, ctxTrafTx, ctxEqpt, 1, out ptCtx);

            if (nRet == 0 && ptCtx.CtxV.ctxndp > 0)
            {
                //...Log2.v("\ntpGetDat.validCtx(): A");
                return (Constant.SUCCESS);
            }
            else if (nRet != Constant.NOMORERECS)
            {
                //...Log2.v("\ntpGetDat.validCtx(): B");
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                Console.Write("Error: Get Ctx returned {0} on {1} / {2} / {3}.\n", nRet, ctxTrafRx, ctxTrafTx, ctxEqpt);
                Console.Write("       Will default to calculations for this pair.\n");
                return (Error.DYN_MS_SQL_SERVER_ERR);
            }

            //...Log2.v("\ntpGetDat.validCtx(): Exit, final");
            return (Constant.NEXT_CTX);
        }

        /// <summary>
        /// This method uses the ctx_xref table to determine which tfci, tfcr and eqpt 
        /// to use to access the ctx table.  
        /// </summary>
        /// <param name="ctxTrafTx"></param>
        /// <param name="ctxTrafRx"></param>
        /// <param name="ctxEqpt"></param>
        /// <param name="ctxTrafTxXref"></param>
        /// <param name="ctxTrafRxXref"></param>
        /// <param name="ctxEqptXref"></param>
        /// <param name="intPrintMsg"></param>
        /// <param name="vicPrintMsg"></param>
        /// <returns></returns>
        public static int GetCTXViaXref(string ctxTrafTx,
            string ctxTrafRx,
            string ctxEqpt,
            out string ctxTrafTxXref,
            out string ctxTrafRxXref,
            out string ctxEqptXref,
            string intPrintMsg,
            string vicPrintMsg)
        {
            int nRet;

            nRet = Suutils.SuGetCtxx(ctxTrafRx, ctxTrafTx, ctxEqpt,
                             out ctxTrafRxXref, out ctxTrafTxXref, out ctxEqptXref);

            return (nRet);
        }

        /// <summary>
        /// This method loads the ctx data points into current ctxStruct.  
        /// </summary>
        /// <param name="curCtx"></param>
        /// <param name="intPrintMsg"></param>
        /// <param name="vicPrintMsg"></param>
        /// <returns></returns>
        public static int TpGetCiCurve(ref CtxStruct curCtx, string intPrintMsg, string vicPrintMsg)
        {
            int i, numPts, minUsed = 9999;
            int ctxIndex = 0;
            bool isNewData = false;
            string tmpMax;

            SuCtxStruct pCtx;

            //...Log2.v("\ntpGetDat.tpGetCiCurve(): A");

            int nRet = 0;

            if ((curCtx.ctxtrafrx.Trim().Equals("")) ||
                (curCtx.ctxtraftx.Trim().Equals("")))
            {
                //...Log2.v("\ntpGetDat.tpGetCiCurve(): B");
                return (Error.NOCTXDPTS);
            }

            if (!ctxSavedIsInitialized)
            {
                //...Log2.v("\ntpGetDat.tpGetCiCurve(): C");
                for (i = 0; i < Constant.MAX_SAVED_CTX; i++)
                {
                    ctxSaved[i].useCounter = -1;
                }
                ctxSavedIsInitialized = true;
            }

            for (i = 0; i < Constant.MAX_SAVED_CTX; i++)
            {
                //...Log2.v(String.Format("\ntpGetDat.tpGetCiCurve(): D"));
                //...Log2.v(String.Format("\ni = {0}", i));
                //...Log2.v(String.Format("\nctxSaved[i].ctxtraftx = {0}, curCtx.ctxtraftx = {1}", ctxSaved[i].ctxtraftx, curCtx.ctxtraftx));
                //...Log2.v(String.Format("\nctxSaved[i].ctxtrafrx = {0}, curCtx.ctxtrafrx = {1}", ctxSaved[i].ctxtrafrx, curCtx.ctxtrafrx));
                //...Log2.v(String.Format("\nctxSaved[i].ctxeqpt = {0}, curCtx.ctxeqpt = {1}", ctxSaved[i].ctxeqpt, curCtx.ctxeqpt));

                if ((ctxSaved[i].ctxtraftx.Equals(curCtx.ctxtraftx))
                    && (ctxSaved[i].ctxtrafrx.Equals(curCtx.ctxtrafrx))
                    && (ctxSaved[i].ctxeqpt.Equals(curCtx.ctxeqpt)))
                {
                    //...Log2.v("\ntpGetDat.tpGetCiCurve(): E");
                    ctxIndex = i;
                    isNewData = false;
                    (ctxSaved[i].useCounter)++;
                    break;
                }
                else if (ctxSaved[i].useCounter == -1)
                {
                    //...Log2.v("\ntpGetDat.tpGetCiCurve(): F");
                    ctxIndex = i;
                    isNewData = true;
                    (ctxSaved[i].useCounter)++;
                    break;
                }

                if (minUsed > ctxSaved[i].useCounter)
                {
                    //...Log2.v("\ntpGetDat.tpGetCiCurve(): G");
                    minUsed = ctxSaved[i].useCounter;
                    ctxIndex = i;
                }
            }  // for loop, index i.

            if (i == Constant.MAX_SAVED_CTX)
            {
                //...Log2.v("\ntpGetDat.tpGetCiCurve(): H");
                isNewData = true;
                ctxSaved[ctxIndex].useCounter = 0;
            }

            if (isNewData)
            {
                //...Log2.v("\ntpGetDat.tpGetCiCurve(): I");
                /* load ctx data into ctxSaved[ctxIndex] */

                numPts = 0;

                nRet = Suutils.SuGetCtx(curCtx.ctxtrafrx, curCtx.ctxtraftx, curCtx.ctxeqpt, 2, out pCtx);

                if (nRet == 0)
                {
                    //...Log2.v("\ntpGetDat.tpGetCiCurve(): J");
                    for (numPts = 0; numPts < pCtx.CtxV.ctxndp; numPts++)
                    {
                        //...Log2.v("\ntpGetDat.tpGetCiCurve(): K");
                        SuCtxD pCtxel = pCtx.pCtxD[numPts];

                        ctxSaved[ctxIndex].ctxPts[numPts, 0] = (float)pCtxel.fsep;
                        ctxSaved[ctxIndex].ctxPts[numPts, 1] = pCtxel.rq;

                        if (numPts > Constant.MAX_CTX_PTS)
                        {
                            //...Log2.v("\ntpGetDat.tpGetCiCurve(): L");
                            ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                            ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                            tmpMax = String.Format("{0}", Constant.MAX_CTX_PTS);
                            ErrMsg.UtPrintMessage(Error.MAXCTX, tmpMax);
                            break;
                        }
                    }
                }
                else
                {
                    //...Log2.v("\ntpGetDat.tpGetCiCurve(): M");
                    if (nRet != Constant.SUCCESS)
                    {
                        //...Log2.v("\ntpGetDat.tpGetCiCurve(): N");
                        ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                        ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                        Console.Write("Error Get Ctx(3) returned {0}.\n", nRet);
                        return (Error.DYN_MS_SQL_SERVER_ERR);
                    }
                }

                if (numPts == 0)
                {
                    //...Log2.v("\ntpGetDat.tpGetCiCurve(): O");
                    return (Error.NOCTXDPTS);
                }

                ctxSaved[ctxIndex].numPts = numPts;
                ctxSaved[ctxIndex].ctxtraftx = curCtx.ctxtraftx;
                ctxSaved[ctxIndex].ctxtrafrx = curCtx.ctxtrafrx;
                ctxSaved[ctxIndex].ctxeqpt = curCtx.ctxeqpt;
                ctxSaved[ctxIndex].calcType = curCtx.calcType;

                //...Log2.v(String.Format("\nctxIndex = {0}", ctxIndex));
                //...Log2.v(String.Format("\nctxSaved[ctxIndex].numPts = {0}", ctxSaved[ctxIndex].numPts));
                //...Log2.v(String.Format("\nctxSaved[ctxIndex].ctxtraftx = {0}", ctxSaved[ctxIndex].ctxtraftx));
                //...Log2.v(String.Format("\nctxSaved[ctxIndex].ctxtrafrx = {0}", ctxSaved[ctxIndex].ctxtrafrx));
                //...Log2.v(String.Format("\nctxSaved[ctxIndex].ctxeqpt = {0}", ctxSaved[ctxIndex].ctxeqpt));
                //...Log2.v(String.Format("\nctxSaved[ctxIndex].calcType = {0}", ctxSaved[ctxIndex].calcType));

            }

            curCtx = ctxSaved[ctxIndex].MakeDeepClone();

            //...Log2.v("\ntpGetDat.tpGetCiCurve(): P");
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method determines which of the three freq. separation specified to 
        /// use for calculations.  
        /// </summary>
        /// <param name="calctype"></param>
        /// <param name="curveCI"></param>
        /// <param name="fsepHi"></param>
        /// <param name="fsepMid"></param>
        /// <param name="fsepLo"></param>
        /// <param name="numPts"></param>
        /// <param name="freqsep"></param>
        /// <param name="reqdcalc"></param>
        public static void TpCalcSepReqd(string calctype,
                        float[,] curveCI,
                        double fsepHi,
                        double fsepMid,
                        double fsepLo,
                        int numPts,
                        out double freqsep,
                        out double reqdcalc)
        {
            // 'out' requirements.
            freqsep = 0.0;
            reqdcalc = 0.0;

            double reqdHi, reqdLo, reqdMid;

            TpGetCiReqd(curveCI, numPts, fsepMid, out reqdMid);
            TpGetCiReqd(curveCI, numPts, fsepHi, out reqdHi);
            TpGetCiReqd(curveCI, numPts, fsepLo, out reqdLo);

            if (calctype.Trim().Equals("-I"))
            {
                if ((reqdHi <= reqdMid) && (reqdHi <= reqdLo))
                {
                    freqsep = fsepHi;
                    reqdcalc = reqdHi;
                }
                else if ((reqdLo <= reqdMid) && (reqdLo <= reqdHi))
                {
                    freqsep = fsepLo;
                    reqdcalc = reqdLo;
                }
                else
                {
                    freqsep = fsepMid;
                    reqdcalc = reqdMid;
                }
            }
            else
            {
                if ((reqdHi >= reqdMid) && (reqdHi >= reqdLo))
                {
                    freqsep = fsepHi;
                    reqdcalc = reqdHi;
                }
                else if ((reqdLo >= reqdMid) && (reqdLo >= reqdHi))
                {
                    freqsep = fsepLo;
                    reqdcalc = reqdLo;
                }
                else
                {
                    freqsep = fsepMid;
                    reqdcalc = reqdMid;
                }
            }
        }

        /// <summary>
        /// Calculates the required c/i value from the given ctx pattern 
        /// at the specified frequency separation.  
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="numAngs"></param>
        /// <param name="freqSep"></param>
        /// <param name="reqdCI"></param>
        public static void TpGetCiReqd(float[,] curve,
                                 int numAngs,
                                 double freqSep,
                                 out double reqdCI)
        {
            // 'out' requirement.
            reqdCI = 0.0;

            int i, hi, lo;
            float[] angs = new float[Constant.MAX_CTX_PTS];
            float reqd;

            for (i = 0; i < numAngs; i++)
            {
                angs[i] = curve[i, 0];
            }

            BinSearch(angs, numAngs, (float)freqSep, out hi, out lo);

            GenUtil.Interp((float)freqSep, curve[lo, 0], curve[hi, 0], curve[lo, 1], curve[hi, 1], out reqd);

            reqdCI = (double)reqd;
        }

        /// <summary>
        /// Calculates the required antenna discrimination values from the 
        /// given antenna pattern at the specified off-axis angle.  
        /// </summary>
        /// <param name="acode"></param>
        /// <param name="offax"></param>
        /// <param name="adisccv"></param>
        /// <param name="adiscxv"></param>
        /// <param name="adiscch"></param>
        /// <param name="adiscxh"></param>
        /// <param name="nullCv"></param>
        /// <param name="nullXv"></param>
        /// <param name="nullCh"></param>
        /// <param name="nullXh"></param>
        /// <param name="intPrintMsg"></param>
        /// <param name="vicPrintMsg"></param>
        /// <returns></returns>
        public static int TpCalcDisc(string acode,
                 double offax,
                 out double adisccv,
                 out double adiscxv,
                 out double adiscch,
                 out double adiscxh,
                 out SQLLEN nullCv,
                 out SQLLEN nullXv,
                 out SQLLEN nullCh,
                 out SQLLEN nullXh,
                 string intPrintMsg,
                 string vicPrintMsg)
        {
            // 'out' requirement.
            adisccv = 0.0;
            adiscxv = 0.0;
            adiscch = 0.0;
            adiscxh = 0.0;
            nullCv = Constant.DB_NULL;
            nullXv = Constant.DB_NULL;
            nullCh = Constant.DB_NULL;
            nullXh = Constant.DB_NULL;

            int rc;
            string select;

            PatternStruct patDat;
            float aCCV, aCXV, aCCH, aCXH;

            /* using the antenna code get its antenna pattern */
            //&&Console.Error.Write("\ntpGetDat.tpCalcDisc(): Tommy: acode = " + acode);
            if (acode.StartsWith("me_azim"))
            {
                //...Log2.v("\nTpGetDat.TpCalcDisc(): Stacktrace:\n" + Environment.StackTrace);
            }

            if ((rc = TpGetPattern(acode, out patDat)) != Constant.SUCCESS)
            {
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                if (rc == Error.NOANTDFOUND || rc == Error.NOANTEFOUND)
                {
                    select = String.Format("acode =  '{0}'", acode);
                    ErrMsg.UtPrintMessage(rc, select);
                    return (Constant.CONT_PROCESSING);
                }
                else
                {
                    Console.Write("Error Get antenna pattern returned {0} for acode = {1}.\n", rc, acode);
                    Log2.e("\n\nTpGetDat.TpcalcDisc(): ERROR: Get antenna pattern returned {0} for acode = {1}.\n", rc, acode);
                }
                return (rc);
            }

            /* using antenna pattern and interferer's off-axis angle find
             * antenna discriminations for the transmit (tx) antenna -
             * co and cross polar, horizontal and vertical
             */
            TpSub.TpFindDisc(patDat.pattern, patDat.numPts, (float)Abs(offax),
                         out aCCV, out aCXV, out aCCH, out aCXH);

            adisccv = (double)aCCV;

            adiscxv = (double)aCXV;

            adiscch = (double)aCCH;

            adiscxh = (double)aCXH;


            nullCv = patDat.nulls[Constant.DCOV];

            nullXv = patDat.nulls[Constant.DXPV];

            nullCh = patDat.nulls[Constant.DCOH];

            nullXh = patDat.nulls[Constant.DXPH];

            return (Constant.SUCCESS);
        }


    }
}
