# Documented File: TtCalcs.cs
**Repository Path:** `TpRunTsip 20231124\TtCalcs.cs`
**Primary Layer:** `TpRunTsip 20231124`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using _Auxlib;
using _Configuration;
using _DataStructures;
using _OHloss;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static _NewLib.Maths;

namespace TpRunTsip
{
    using global::TpRunTsip;
    using SQLLEN = Int64;
    /// <summary>
    /// Provides methods that perform the mathematical calculations
    /// required for TSIP analysis of Tt cases.
    /// </summary>
    public class TtCalcs
    {

#if PINVOKE
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]

        private extern static int ttTsorbCalcs([In] TpParm parmStruct,
                      [In] string offazm,
                      [In] float aht,
                      [In] string call2,
                      [In] string bndcde,
                      [In] float tazmth,
                      [In] float telvtn,
                      [In] FtSite siteStruct);

        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]

        private extern static int calcTotADisc([In, Out] SQLLEN[] anteNulls,
                                                    [In, Out] TtChan chanStruct,
                                                    [In, Out] TtAnte anteStruct,
                                                    [In, Out] ref double totAdiscX,
                                                    [In, Out] ref double totAdiscC,
                                                    [In, Out] ref short copolar,
                                                    [In, Out] ref SQLLEN nullTotAdX,
                                                    [In, Out] ref double totAdisc);

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute()]
        public static int CalcTotADisc_NATIVE(SQLLEN[] anteNulls,
                                                    TtChan chanStruct,
                                                    TtAnte anteStruct,
                                                    ref double totAdiscX,
                                                    ref double totAdiscC,
                                                    ref short copolar,
                                                    ref SQLLEN nullTotAdX,
                                                    ref double totAdisc)
        {
            //...Log2.v("\nTtCalcs.CalcTotADisc_NATIVE(): Entry");
            int rc = Constant.FAILURE;

            try
            {
                rc = calcTotADisc(anteNulls,
                                  chanStruct,
                                  anteStruct,
                                  ref totAdiscX,
                                  ref totAdiscC,
                                  ref copolar,
                                  ref nullTotAdX,
                                  ref totAdisc);
            }
            catch (Exception e)
            {
                Log2.e("\nTtCalcs.CalcTotADisc_NATIVE(): ERROR: " + e.Message);
                Log2.e("\nTtCalcs.CalcTotADisc_NATIVE(): ERROR: " + e.StackTrace);
            }

            //...Log2.v("\nTtCalcs.CalcTotADisc_NATIVE(): Exit");
            return rc;
        }

        public static int TtTsorbCalcs_NATIVE(TpParm parmStruct,
                      string offazm,
                      float aht,
                      string call2,
                      string bndcde,
                      float tazmth,
                      float telvtn,
                      FtSite siteStruct)
        {
            //...Log2.v("\nTtCalcs.TtTsorbCalcs_NATIVE(): Entry");

            int result;

            result = ttTsorbCalcs(parmStruct, offazm, aht, call2, bndcde, tazmth, telvtn, siteStruct);

            //...Log2.v("\nTtCalcs.TtTsorbCalcs_NATIVE(): Exit");
            return result;
        }
#endif

        private static Enums.Ecalctype ecalctype;

        /// <summary>
        /// This method performs ORBIT or TsOrb calculations if the User requested 
        /// TsOrb calcs in the parameter file.  
        /// </summary>
        /// <param name="parmStruct"> - TpParm object providing paramater data.</param>
        /// <param name="offazm"> - TBD.</param>
        /// <param name="aht"> - TBD.</param>
        /// <param name="call2"> - TBD.</param>
        /// <param name="bndcde"> - TBD.</param>
        /// <param name="tazmth"> - TBD.</param>
        /// <param name="telvtn"> - TBD.</param>
        /// <param name="siteStruct"> - FtSite object encapsulating the site information.</param>
        /// <returns></returns>
        public static int TtTsorbCalcs(TpParm parmStruct,
                                        string offazm,
                                        float aht,
                                        string call2,
                                        string bndcde,
                                        float tazmth,
                                        float telvtn,
                                        FtSite siteStruct)
        {
            double dist = 0.0;
            double azim = 0.0;
            double elevAng = 0.0;
            double insL = 0.0;
            double minAngSep = 0.0;
            double e1_10 = 0.0;
            double e10_15 = 0.0;
            double eGt15 = 0.0;
            double crAzSt = 0.0;
            double crAzEnd = 0.0;
            double crLSt = 0.0;
            double crLEnd = 0.0;
            double trueAzim = 0.0;
            double trueElev = 0.0;

            string insLSen = "";
            string crLStSen = "";
            string crLEndSen = "";
            string siteSelect = "";

            int rc;
            int nInd;
            int trueVals = Constant.FALSE;
            int trans = 0;

            //FILE fp;

            FtSiteStr pSite;
            FtSiteStrNulls pSiteNulls;

            AxStation s1 = new AxStation();
            AxStation s2 = new AxStation();

            // Copy proposed's call1 information into Orbit's station 1
            // structure and the porposed's call2 information into station 2 
            // structure - both site and antenna information are needed

            s1.elevM = siteStruct.grnd;
            s1.name = siteStruct.name;

            TpSub.TpLoadLat(siteStruct.latit, out s1.LL.latSens, out s1.LL.latDeg, out s1.LL.latMin, out s1.LL.latSec);

            TpSub.TpLoadLong(siteStruct.longit, out s1.LL.longSens, out s1.LL.longDeg, out s1.LL.longMin, out s1.LL.longSec);

            s1.antHtM = aht;

            // Now fill in s2 struct with call2's info.
            if ((rc = TpMdbPdfGet.TtFullSiteGet(call2, parmStruct.proname, false, out pSite, out pSiteNulls)) != 0)
            {
                if (rc < 0)
                {
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "SITE", siteSelect);
                }
                return (rc);
            }

            // A record was found for selected data /
            s2.name = pSite.stSite.name;
            s2.elevM = pSite.stSite.grnd;

            TpSub.TpLoadLat(pSite.stSite.latit, out s2.LL.latSens, out s2.LL.latDeg, out s2.LL.latMin, out s2.LL.latSec);

            TpSub.TpLoadLong(pSite.stSite.longit, out s2.LL.longSens, out s2.LL.longDeg, out s2.LL.longMin, out s2.LL.longSec);

            // Need to get the call2's antenna height /
            nInd = FtUtils.FtGetMainAnte(pSite, siteStruct.call1, bndcde);
            if (nInd < 0)
            {
                Log2.e("\nTtCalcs.TtTsorbCalcs(): ERROR: FtGetMainAnte() failed to find a main antenna at opposite end.");
                GenUtil.SetErr("ttTsorbCalcs: No main antenna at opposite end: %s, %s, %s",
                         siteStruct.call1, call2, bndcde);
                return (rc);
            }

            // A record was found for selected data /
            s2.antHtM = pSite.stAntsPtr[nInd].aht;


            // -------Perform ORBIT or tsOrb calculations-------/

            if (Strings.FirstCharIs(offazm, 'Y'))
            {
                trueVals = Constant.TRUE;
                trueAzim = (double)tazmth;
                trueElev = (double)telvtn;
            }
            rc = AxOrbitSupp.AxOrbit(Constant.METERS, ref s1, ref s2, trueAzim, trueElev, trueVals, ref dist, ref azim,
                         ref elevAng, ref insL, ref insLSen, ref minAngSep, ref e1_10, ref e10_15, ref eGt15,
                         ref crAzSt, ref crAzEnd, ref crLSt, ref crLStSen, ref crLEnd, ref crLEndSen,
                         ref trans);


            // Write to the Orbit report.

            TpRunTsip.mReports.OrbitWritten = true;

            AxOrbitSupp.AxRptOrb(TpRunTsip.mTW_ORBIT, dist, azim, elevAng, insL, insLSen, minAngSep,
                       e1_10, e10_15, eGt15, crAzSt, crAzEnd, crLSt, crLStSen,
                       crLEnd, crLEndSen, trans, rc, trueVals, s1, s2);

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method performs the calculations for a TSIP TS-TS case
        /// at the site level.  
        /// </summary>
        /// <param name="ttSite"> - TtSite object describing the current site.</param>
        /// <param name="siteNulls"> - ODBC nullInds associated with ttSite.</param>
        /// <param name="intLnkGrnd"> - TBD.</param>
        /// <param name="vicLnkGrnd"> - TBD.</param>
        /// <param name="azimIV"> - TBD.</param>
        /// <param name="azimVI"> - TBD.</param>
        /// <param name="azimXY"> - azimuth bearing from interferer -> remote.</param>
        /// <param name="azimAB"> - azimuth bearing from victim -> remote.</param>
        public static void TtSiteCalcs(ref TtSite ttSite,
                                        ref SQLLEN[] siteNulls,
                                        double intLnkGrnd,
                                        double vicLnkGrnd,
                                        double azimIV,
                                        double azimVI,
                                        double azimXY,            /*	Interferor . remote */
                                        double azimAB)            /*	Victim . remote */
        {
            double offax;

            /* compute distance advantage	 */
            ttSite.distadv = 20.0 * Log10(ttSite.int1vic1dist /
                                                ttSite.vic1vic2dist);
            siteNulls[TtSite.DISTADV] = Constant.DB_NOT_NULL;

            /* compute interferer's off-axis angle */
            OffAngle(ttSite.int1vic1dist, ttSite.int1int2dist,
                         ttSite.intgrnd, ttSite.vicgrnd, intLnkGrnd,
                             azimIV, azimXY, out offax);
            ttSite.intoffax = offax;
            siteNulls[TtSite.INTOFFAX] = Constant.DB_NOT_NULL;

            /* compute victims's off-axis angle */
            OffAngle(ttSite.int1vic1dist, ttSite.vic1vic2dist,
                         ttSite.vicgrnd, ttSite.intgrnd, vicLnkGrnd,
                             azimVI, azimAB, out offax);
            ttSite.vicoffax = offax;
            siteNulls[TtSite.VICOFFAX] = Constant.DB_NOT_NULL;

        }

        /// <summary>
        /// Calculates the offaxis angles between the victim and interferer and corrects 
        /// for sign using the azimuths. The call parameters describe a victim 'V' pointing to 
        /// its other end 'A' and interfered with from 'I'. The method returns the offaxis angle 
        /// from azimVA.   
        /// </summary>
        /// <param name="IVdist"> - distance from Interferer to Victim.</param>
        /// <param name="VAdist"> - distance from Victim to its other end.</param>
        /// <param name="Igrnd"> - ground height at Interferer site.</param>
        /// <param name="Vgrnd"> - ground height at Victim site.</param>
        /// <param name="AGrnd"> - ground height of Victim's other end.</param>
        /// <param name="azimVI"> - azimuth bearing from Victim to Interferer.</param>
        /// <param name="azimVA"> - azimuth bearing frim Victim to its other end.</param>
        /// <param name="offax"> - offaxis angle from azimVA.</param>
        public static void OffAngle(double IVdist, double VAdist,
                                double Igrnd, double Vgrnd, double AGrnd,
                                double azimVI, double azimVA, out double offax)
        {
            if (IVdist <= 0.001)
            {
                offax = 90.0;
            }
            else
            {
                TtCalcIAng(Igrnd, Vgrnd, AGrnd, IVdist, VAdist, azimVI, azimVA, out offax);
            }
        }

        /// <summary>
        /// Calculates the subtended angle XAY between 3 points given the
        /// ground altitude and distance, and the azimuths between the two sites and 
        /// the common apex.  
        /// </summary>
        /// <param name="Xgrnd"> - ground height of site X.</param>
        /// <param name="Agrnd"> - ground height of site A.</param>
        /// <param name="Ygrnd"> - ground height of site Y.</param>
        /// <param name="XAdist"> - distance between X and A.</param>
        /// <param name="XYdist"> - distance between X and Y.</param>
        /// <param name="azimXA"> - azimuth bearing from X to A.</param>
        /// <param name="azimXY"> - azimuth bearing from X to Y.</param>
        /// <param name="iAng"> - the calculated subtended angle in degrees.</param>
        public static void TtCalcIAng(double Xgrnd,
                    double Agrnd,
                      double Ygrnd,
                      double XAdist,
                      double XYdist,
                      double azimXA,
                      double azimXY,
                      out double iAng)
        {
            // 'out' requirement.
            iAng = 0.0;

            double elevXA, elevAX, elevXY, elevYX;
            double xXA, yXA, zXA, xXY, yXY, zXY, Y;
            double dCheck;

            /* calc elev X-A */
            AxSub3.AxElev(Xgrnd / 1000.0, Agrnd / 1000.0, XAdist, out elevXA, out elevAX);
            /* calc elev X-Y */
            AxSub3.AxElev(Xgrnd / 1000.0, Ygrnd / 1000.0, XYdist, out elevXY, out elevYX);

            xXA = XAdist * CosD(180.0 - azimXA) * CosD(elevXA);
            yXA = XAdist * CosD(azimXA - 90.0) * CosD(elevXA);
            zXA = XAdist * SinD(elevXA);

            xXY = XYdist * CosD(180.0 - azimXY) * CosD(elevXY);
            yXY = XYdist * CosD(azimXY - 90.0) * CosD(elevXY);
            zXY = XYdist * SinD(elevXY);

            Y = (xXA * xXY + yXA * yXY + zXA * zXY) / (XAdist * XYdist);

            if (Y > 0.99999)
            {
                iAng = 0.0;
            }
            else
            {
                iAng = AcosD(Y);
            }

            /*	Get the sign of the offaxis angle.  The sign of the angle will be the
            *		same as the sign of the sine (?!?) of the offaxis azimuth minus the
            *		boresight azimuth. */
            dCheck = SinD(azimXA - azimXY);
            if (dCheck < 0.0)
            {
                iAng = -(iAng);
            }
        }

        /// <summary>
        /// Thismethod performs TSIP calculations for a TS-TS case at the 
        /// antenna level.  
        /// </summary>
        /// <param name="parmStruct"> - TpParm object providing paramater data.</param>
        /// <param name="pProSite"> - FtSiteStr object describing the proposed site.</param>
        /// <param name="pProSiteNulls"> - ODBC nullInds associated with pProSite.</param>
        /// <param name="nProAntNum"> number of proposed antennae.</param>
        /// <param name="pEnvSite"> - FtSiteStr object describing an environment site.</param>
        /// <param name="pEnvSiteNulls"> - ODBC nullInds associated with pEnvSite.</param>
        /// <param name="nEnvAntNum"> - number of antennae at environment site.</param>
        /// <param name="anteStruct"> - TtAnte object providing current antenna data.</param>
        /// <param name="siteStruct"> - TtSite object providing current site pair data</param>
        /// <param name="anteNulls"> - ODBC nullInds associated with anteStruct.</param>
        /// <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        /// <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        /// <param name="dIntOffax"> - Inteferer antenna off-axis angle.</param>
        /// <param name="dVicOffax"> - Victim antenna off-axis angle.</param>
        /// <returns></returns>
        public static int TtAnteCalcs(TpParm parmStruct, /* current parameter record data  */
                        FtSiteStr pProSite,
                        FtSiteStrNulls pProSiteNulls,
                        int nProAntNum,
                        FtSiteStr pEnvSite,
                        FtSiteStrNulls pEnvSiteNulls,
                        int nEnvAntNum,
                        TtAnte anteStruct, /* current antenna data           */
                        TtSite siteStruct, /* current site pair data         */
                        SQLLEN[] anteNulls,  /* (NOT_)NULL for each ante field */
                        string intPrintMsg,
                        string vicPrintMsg,
                        double dIntOffax,   /*	Antenna off-axis angles				*/
                        double dVicOffax)
        {
            int rc;
            bool isMDB;
            double disc;
            double junk;
            double intMbnd;
            double vicMbnd;
            string intTableName;
            string vicTableName;
            SuBand pBand;
            bool nMdbCheck;

            /* get full names of victim site and antenna tables */
            TpSub.TtTableName(siteStruct.interferer, parmStruct, out intTableName, out vicTableName, out isMDB);

            /* check if local site is a passive reflector (interferer):
             * if call1 begins with '%'
             */
            if (Strings.FirstCharIs(anteStruct.intcall1, '%'))
            {
                /* get midband freq from SDB for interferer */
                if (Suutils.SuGetBand(anteStruct.intbndcde, out pBand) != 0)
                {
                    Log2.e("\nTtCalcs.TtAnteCalcs(): ERROR: call to SuGetBand() failed.");
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.INVALIDBANDCODE, anteStruct.intbndcde);
                    return (Constant.FAILURE);
                }

                intMbnd = pBand.bmidf;
                pBand = null;

                /* calculate interfer's passive reflector and discrimination */
                if (Strings.FirstCharIs(siteStruct.interferer, 'P'))
                {
                    nMdbCheck = false;
                }
                else
                {
                    nMdbCheck = isMDB;
                }
                rc = TtCalkPassive.TtCalcPassive(true, nMdbCheck, intTableName,
                                   anteStruct.intcall1, anteStruct.intcall2,
                                   anteStruct.intbndcde, anteStruct.intanum,
                                   intMbnd, dIntOffax, out disc, out junk, intPrintMsg,
                                                     vicPrintMsg);
                if (rc != Constant.SUCCESS)
                {
                    string str = String.Format("\n***Error in Passive Calculation:-\nInt: {0}\nVic: {1}\n",
                                   intPrintMsg, vicPrintMsg);
                    Log2.e("\nTtCalcs.TtAnteCalcs(): ERROR: call to TtCalcPassive() failed: " + str);
                    TpRunTsip.mTW_ERR.Write(str);
                    return (rc);
                }

                anteStruct.adiscctxv = disc;
                anteStruct.adiscxtxv = disc;
                anteStruct.adiscctxh = disc;
                anteStruct.adiscxtxh = disc;
                anteNulls[TtAnte.ADISCCTXV] = Constant.DB_NOT_NULL;
                anteNulls[TtAnte.ADISCXTXV] = Constant.DB_NOT_NULL;
                anteNulls[TtAnte.ADISCCTXH] = Constant.DB_NOT_NULL;
                anteNulls[TtAnte.ADISCXTXH] = Constant.DB_NOT_NULL;

            }
            else
            {
                /* interferer is not a passive repeater
                   * get antenna pattern and calculate antenna discriminations for
                   * the interferer or transmit (tx) antenna
                   *
                   * get full table names for subsidiary ante and antd info */
                /* not temp. Ante table so Ante data from SDB */

                rc = TpGetDat.TpCalcDisc(anteStruct.intacode,
                                            dIntOffax,
                                            out anteStruct.adiscctxv,
                                            out anteStruct.adiscxtxv,
                                            out anteStruct.adiscctxh,
                                            out anteStruct.adiscxtxh,
                                            out anteNulls[TtAnte.ADISCCTXV],
                                            out anteNulls[TtAnte.ADISCXTXV],
                                            out anteNulls[TtAnte.ADISCCTXH],
                                            out anteNulls[TtAnte.ADISCXTXH],
                                            intPrintMsg, vicPrintMsg);

                if (rc != Constant.SUCCESS)
                {
                    if (rc == Constant.CONT_PROCESSING)
                    {
                        /* not a fatal error, continue with next ante */
                        return (Constant.SUCCESS);
                    }
                    TpRunTsip.mTW_ERR.Write("\n***Error in Interferor Discrim. Calc:\nInt: {0}\nVic: {1}\n",
                                   intPrintMsg, vicPrintMsg);
                    return (rc);
                }
            }
            /* check if local site is a passive reflector (victim):
             * if call1 begins with '%'
             */
            if (Strings.FirstCharIs(anteStruct.viccall1, '%'))
            {

                /* get midband freq from SDB for victim */
                if (Suutils.SuGetBand(anteStruct.vicbndcde, out pBand) != 0)
                {
                    Log2.e("\nTtCalcs.TtAnteCalcs(): ERROR: call to SuGetBand() failed.");
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.INVALIDBANDCODE, anteStruct.vicbndcde);
                    return (Constant.FAILURE);
                }

                vicMbnd = pBand.bmidf;
                pBand = null;

                /* calculate victim's passive reflector and discrimination */
                if (Strings.FirstCharIs(siteStruct.interferer, 'P'))
                {
                    nMdbCheck = isMDB;
                }
                else
                {
                    nMdbCheck = false;
                }
                rc = TtCalkPassive.TtCalcPassive(false, nMdbCheck, vicTableName,
                                   anteStruct.viccall1, anteStruct.viccall2,
                                   anteStruct.vicbndcde, anteStruct.vicanum,
                                   vicMbnd, dVicOffax, out disc, out junk,
                                     intPrintMsg, vicPrintMsg);

                if (rc != Constant.SUCCESS)
                {
                    string str = String.Format("\n***Error in Proposed Passive Calculation:\nInt: {0}\nVic: {1}\n",
                                   intPrintMsg, vicPrintMsg);
                    Log2.e("\nTtCalcs.TtAnteCalcs(): ERROR: call to TtCalcPassive() failed: " + str);
                    TpRunTsip.mTW_ERR.Write(str);
                    return (rc);
                }
                anteStruct.adisccrxv = disc;
                anteStruct.adiscxrxv = disc;
                anteStruct.adisccrxh = disc;
                anteStruct.adiscxrxh = disc;

                anteNulls[TtAnte.ADISCCRXV] = Constant.DB_NOT_NULL;
                anteNulls[TtAnte.ADISCXRXV] = Constant.DB_NOT_NULL;
                anteNulls[TtAnte.ADISCCRXH] = Constant.DB_NOT_NULL;
                anteNulls[TtAnte.ADISCXRXH] = Constant.DB_NOT_NULL;
            }
            else
            {/* victim is not a passive repeater
		 * get antenna pattern and calculate antenna discriminations for
		 * the victim or receive (rx) antenna
		 */
             /* not temp. Ante table so Ante data from SDB */

                rc = TpGetDat.TpCalcDisc(anteStruct.vicacode,
                                                dVicOffax,
                                                out anteStruct.adisccrxv,
                                                out anteStruct.adiscxrxv,
                                                out anteStruct.adisccrxh,
                                                out anteStruct.adiscxrxh,
                                                out anteNulls[TtAnte.ADISCCRXV],
                                                out anteNulls[TtAnte.ADISCXRXV],
                                                out anteNulls[TtAnte.ADISCCRXH],
                                                out anteNulls[TtAnte.ADISCXRXH],
                                                intPrintMsg,
                                                vicPrintMsg);

                if (rc != Constant.SUCCESS)
                {
                    if (rc == Constant.CONT_PROCESSING)
                    {
                        /* not a fatal error, continue with next ante */
                        return (Constant.SUCCESS);
                    }


                    TpRunTsip.mTW_ERR.Write("\n***Error in Victim Discrimination Calc:\nInt: {0}\nVic: {1}\n",
                                   intPrintMsg, vicPrintMsg);
                    return (rc);
                }
            }

            return (Constant.SUCCESS);
        }

        //TOP

        private static string oldTrafTx = "";
        private static string oldTrafRx = "";
        private static string oldEqptTx = "";
        private static string oldEqptRx = "";
        private static int oldRc = 0;
        private static CtxStruct curCtx = new CtxStruct();

        /// <summary>
        /// This method performs the TSIP calculations for a TS-TS case at the 
        /// channel level. Calculations are done for the worst case first (called Band 
        /// Calculations). If no interference is found then the rest of the channels on 
        /// this antenna are not processed. If Band Calculations produce interferece 
        /// and the User requested P/C (Plan/Channel) calculations then each channel is 
        /// processed individually with actual, as opposed to worst case, data.  
        /// </summary>
        /// <param name="chanStruct"> - TtChan object providing current channel data.</param>
        /// <param name="anteStruct"> - TtAnte object providing current antenna data.</param>
        /// <param name="anteNulls"> - ODBC nullInds associated with anteStruct.</param>
        /// <param name="siteStruct"> - TtSite object providing current site data.</param>
        /// <param name="parmStruct"> - TpParm object providing paramater data.</param>
        /// <param name="patNulls"> - ODBC nullInds associated with antenna pattern values.</param>
        /// <param name="chanNulls"> - ODBC nullInds associated with chanStruct.</param>
        /// <param name="intPowrTx"> - Interferer's transmit power level.</param>
        /// <param name="intAFSLtx"> - antenna feed system loss.</param>
        /// <param name="nullIntAfslTx"> - ODBC nullInd associated with intAFSLtx.</param>
        /// <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        /// <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        /// <param name="totADiscW"> - total antenna discrimination.</param>
        /// <param name="intMbnd"> - interferer's mid-band frequency.</param>
        /// <param name="vicMbnd"> - victim's mid-band frequency.</param>
        /// <param name="intPatLoss"> - transmission path loss for the interfering signal.</param>
        /// <returns></returns>
        public static int TtChanCalcs(TtChan chanStruct, /* current channel data */
                        TtAnte anteStruct, /* current antenna data */
                        SQLLEN[] anteNulls,  /* Nulls for the antenna struct */
                        TtSite siteStruct, /* current site data */
                        TpParm parmStruct, /* current parameter record data */
                        SQLLEN[] patNulls,   /* nulls for pattern values */
                        SQLLEN[] chanNulls,  /* (NOT_)NULL for each chan field */
                        double intPowrTx,
                        double intAFSLtx,
                        SQLLEN nullIntAfslTx,
                        string intPrintMsg,
                        string vicPrintMsg,
                        double totADiscW,
                        double intMbnd,
                        double vicMbnd,
                        double intPatLoss)
        {
            //...Log2.v("\nTtCalcs.TtChanCalcs(): Entry");

            double bandTmp;
            double totAdiscC = 0.0; ;
            double totAdiscX;
            double vicAFSLrx;
            double viclnkAFSLtx;
            double viclnkPowTx;
            double intEqptStab;
            double vicEqptStab;
            double intAGain;
            double vicAGain;
            double totADisc = 0.0;
            double fsepHi;
            double fsepLo;
            double fsepMid;
            double fsepMax;
            double viclnkAGain;
            char cCalcType;        /*  The type of loss calculation selected */

            string intTableName;
            string vicTableName;
            string select;
            string vicChanTable;
            int rc;
            int nRet;
            SQLLEN[] vicChanNulls;  //[FT_CHAN_SIZE_];
            short copolar = 0; ;
            short viclnkANumTx = 0; ;
            SQLLEN nullViclnkAfslTx = Constant.DB_NOT_NULL;
            SQLLEN nullAntNumTx = Constant.DB_NOT_NULL;
            SQLLEN nullPowTx = Constant.DB_NOT_NULL;
            SQLLEN nullVicAfslRx = Constant.DB_NOT_NULL;
            SQLLEN nullTotAdX = Constant.DB_NOT_NULL;
            SQLLEN nullTotAdC = Constant.DB_NOT_NULL;
            bool isMDB;

            FtChan ftTmpChan;

            double intTiltDisc;
            double vicTiltDisc;

            char cVicType;          /* 	Either 'A' or 'D' Victim */
            char cIntType;          /*	Either 'A' or 'D' Interferor */
            TcTxAnalog tVicAnalog;      /*	Equipments.  Only two will be */
            TcTxDigital tVicDigital;    /*	used. */
            TcTxAnalog tIntAnalog;
            TcTxDigital tIntDigital;

            double reqLo;
            double reqMid;
            double reqHi;

            char cTypeOfInt;

            double dActualDist;

            nullViclnkAfslTx = -1;
            viclnkAFSLtx = 0.0;
            totAdiscX = 0.0;
            viclnkAFSLtx = 0.0;
            viclnkPowTx = 0.0;
            viclnkAGain = 0.0;

            TpSub.TtTableName(siteStruct.interferer, parmStruct, out intTableName, out vicTableName, out isMDB);

            GenUtil.UtCvtName(Constant.FT_CHAN, vicTableName, out vicChanTable);

            /* calculate interferer and victim gains for antenna or passive */
            rc = CalcGains(chanStruct, anteStruct, siteStruct, parmStruct, out intAGain,
                                out vicAGain, intPrintMsg, vicPrintMsg);

            //...Log2.v("\nttChanCalcs(): 1");

            if (rc != Constant.SUCCESS)
            {
                string str = String.Format("\n***Error in Calculating Victim Gain:\nInt: {0}\nVic: {1}\n",
                           intPrintMsg, vicPrintMsg);
                Log2.e("\nTtCalcs.TtChanCalcs(): ERROR: call to CalcGains() failed, the Exit: " + str);
                Console.Out.Write(str);
                return (rc);
            }

            /*  Because the gain is not stored (the value stored in the gain column
            *   is the tgain from the antenna record -- which is never used) in the
            *   ttAnte structure, we need to store it here so that it prints out on
            *   the reports.
            */
            anteStruct.intgain = (float)intAGain;
            anteNulls[TtAnte.INTGAIN] = Constant.DB_NOT_NULL;
            anteStruct.vicgain = (float)vicAGain;
            anteNulls[TtAnte.VICGAIN] = Constant.DB_NOT_NULL;

            rc = GetVicLnkInfo(chanStruct, anteStruct, siteStruct, parmStruct, vicTableName, isMDB,
                                    vicMbnd, ref viclnkAFSLtx, ref viclnkANumTx, ref viclnkPowTx, ref viclnkAGain, ref nullViclnkAfslTx,
                                    ref nullAntNumTx, ref nullPowTx, intPrintMsg, vicPrintMsg);

            if (rc != Constant.SUCCESS)
            {
                string str = String.Format("\n***Error in getting victim Link Info:\nInt: {0}\nVic: {1}\n",
                           intPrintMsg, vicPrintMsg);
                Log2.e("\nTtCalcs.TtChanCalcs(): ERROR: call to GetVicLnkInfo() failed then Exit: " + str);
                TpRunTsip.mTW_ERR.Write(str);
                return (rc);
            }

            //...Log2.v("\nttChanCalcs(): 2");

            /****************************************************************************\
            *
            *		Do the multipoint calculations, then start on the basic BAND preliminary
            *		calculations.  If we fail these we don't need to go further.
            *
            \****************************************************************************/
            rc = MultiPointCalcs(chanStruct);

            if (rc != Error.CONTINUE)
            {
                string str = String.Format("\n***Error in Multipoint Calculations:\nInt: {0}\nVic: {1}\n",
                           intPrintMsg, vicPrintMsg);
                Log2.e("\nTtCalcs.TtChanCalcs(): ERROR: call to MultiPointCalcs() failed the Exit: " + str);
                TpRunTsip.mTW_ERR.Write(str);
                return (rc);
            }

            //...Log2.v("\nttChanCalcs(): 3");

            chanStruct.patloss = intPatLoss;
            chanNulls[TtChan.PATLOSS] = Constant.DB_NOT_NULL;

            chanStruct.totantdisc = totADiscW;
            chanNulls[TtChan.TOTANTDISC] = Constant.DB_NOT_NULL;

            chanStruct.reqdcalc = 110.0;
            chanNulls[TtChan.REQDCALC] = Constant.DB_NOT_NULL;

            if ((nullAntNumTx != Constant.DB_NULL) &&
                    (nullPowTx != Constant.DB_NULL) &&
                (nullIntAfslTx != Constant.DB_NULL))
            {
                chanStruct.eirpadv = (viclnkPowTx + viclnkAGain - viclnkAFSLtx) -
                                        (40.0 + intAGain - intAFSLtx);
                chanNulls[TtChan.EIRPADV] = Constant.DB_NOT_NULL;

                chanStruct.calcico = siteStruct.distadv + chanStruct.totantdisc +
                                      chanStruct.eirpadv;
                chanNulls[TtChan.CALCICO] = Constant.DB_NOT_NULL;

                chanStruct.resti = chanStruct.calcico - chanStruct.reqdcalc;
                chanNulls[TtChan.RESTI] = Constant.DB_NOT_NULL;

                if (chanStruct.resti < parmStruct.margin)
                {
                    if (parmStruct.analopt.Equals("BAND"))
                    {
                        /* BAND (ie no Chan info) use midband freq's
                         * for tx & rx frequencies */
                        chanStruct.intfreqtx = intMbnd;
                        chanStruct.vicfreqrx = vicMbnd;
                        chanNulls[TtChan.INTFREQTX] = Constant.DB_NOT_NULL;
                        chanNulls[TtChan.VICFREQRX] = Constant.DB_NOT_NULL;

                        chanStruct.report = Constant.TRUE;
                        chanStruct.calctype = "C/I";
                        chanNulls[TtChan.CALCTYPE] = Constant.DB_NOT_NULL;
                        //	Band report ...
                        return (Constant.PC_SKIP);
                    }
                }
                else
                {
                    return (Constant.PC_SKIP);
                }
            }
            else
            {
                return (Constant.PC_SKIP);
            }

            //...Log2.v("\nTtCalcs.TtChanCalcs(): 3-1");

            /*@@@@@@@@@@@@@  Begin Plan/Channel Calculations @@@@@@@@@@@@@@@@@@*/
            rc = TpGetDat.TpGetEqptStab(chanStruct.inteqpttx, chanStruct.viceqptrx,
                                    parmStruct.tempequip, out intEqptStab, out vicEqptStab,
                                    intPrintMsg, vicPrintMsg);

            if (rc != Constant.SUCCESS)
            {
                string str = String.Format("\n***Error getting eqpt stability for:-\nTx Equip {0} into Rx Equip: {1}\nInt: {2}\nVic: {3}\n",
                             chanStruct.inteqpttx, chanStruct.viceqptrx,
                           intPrintMsg, vicPrintMsg);
                Log2.e("\nTtCalcs.TtChanCalcs(): ERROR: call to TpGetEqptStab() failed the Exit: " + str);
                TpRunTsip.mTW_ERR.Write(str);
                return (rc);
            }

            //...Log2.v("\nttChanCalcs(): 4");

            fsepMid = Abs(chanStruct.intfreqtx - chanStruct.vicfreqrx);
            fsepHi = fsepMid + (intEqptStab * intMbnd + vicEqptStab * vicMbnd);
            fsepLo = fsepMid - (intEqptStab * intMbnd + vicEqptStab * vicMbnd);
            fsepLo = Max(0.0, fsepLo);

            /*
            * TASK 466: Maximum frequency separation is now set in the
            * parameters by the user, and a choice between this and the
            * default is set by tpMaxFSep.
            */
            fsepMax = TpGetDat.TpMaxFSep(parmStruct.fsep, parmStruct.coordist, siteStruct.int1vic1dist,
                                  chanStruct.inttraftx, chanStruct.victrafrx);
            if (fsepHi >= fsepMax && fsepLo >= fsepMax)
            {
                /* not a fatal error but this case not in contention */
                return (Constant.SUCCESS);
            }

            //...Log2.v("\nttChanCalcs(): 5");

            select = String.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and chid = '{3}'",
                              chanStruct.viccall1, chanStruct.viccall2,
                              chanStruct.vicbndcde, chanStruct.vicchid);

            if (Strings.FirstCharIs(siteStruct.interferer, 'E'))
            {
                rc = TpMdbPdfGet.TtChanGet(chanStruct.viccall1, chanStruct.viccall2,
                              chanStruct.vicbndcde, chanStruct.vicchid,
                                            vicChanTable, false, out ftTmpChan, out vicChanNulls);
            }
            else
            {
                rc = TpMdbPdfGet.TtChanGet(chanStruct.viccall1, chanStruct.viccall2,
                              chanStruct.vicbndcde, chanStruct.vicchid,
                                            vicChanTable, isMDB, out ftTmpChan, out vicChanNulls);
            }
            if (rc != Constant.SUCCESS)
            {
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "CHAN", select);
                if (rc == Constant.FAILURE)
                {
                    if (Strings.FirstCharIs(siteStruct.interferer, 'E'))
                    {
                        /* if we looked for the channel in the PDF and didn't find it this is
                         * simply a warning - ie don't return a hard and fast stop */
                        return (Constant.SUCCESS);
                    }
                    return (rc);
                }
            }
            if (Strings.FirstCharIs(chanStruct.intcall2, '%'))
            {
                vicAFSLrx = 0.0;
                nullVicAfslRx = Constant.DB_NOT_NULL;
            }
            else
            {
                if (chanStruct.rxant == 1)
                {
                    if (vicChanNulls[FtChan.AFSLRX1] == Constant.DB_NULL)
                    {
                        vicAFSLrx = 0.0;
                        nullVicAfslRx = Constant.DB_NOT_NULL;
                    }
                    else
                    {
                        vicAFSLrx = ftTmpChan.afslrx1;
                        nullVicAfslRx = vicChanNulls[FtChan.AFSLRX1];
                    }
                }
                else if (chanStruct.rxant == 2)
                {
                    if (vicChanNulls[FtChan.AFSLRX2] == Constant.DB_NULL)
                    {
                        vicAFSLrx = 0.0;
                        nullVicAfslRx = Constant.DB_NOT_NULL;
                    }
                    else
                    {
                        vicAFSLrx = ftTmpChan.afslrx2;
                        nullVicAfslRx = vicChanNulls[FtChan.AFSLRX2];
                    }
                }
                else
                {
                    if (vicChanNulls[FtChan.AFSLRX3] == Constant.DB_NULL)
                    {
                        vicAFSLrx = 0.0;
                        nullVicAfslRx = Constant.DB_NOT_NULL;
                    }
                    else
                    {
                        vicAFSLrx = ftTmpChan.afslrx3;
                        nullVicAfslRx = vicChanNulls[FtChan.AFSLRX3];
                    }
                }
            }

            //...Log2.v("\nTtCalcs.TtChanCalcs(): 5-1-2");

            if (TpRunTsip.GlbIsCtxCalc == 0)
            {
                //...Log2.v("\nttChanCalcs(): 5-2");

                /*	We will be using the ctx curves when we can */
                if ((!oldTrafTx.Equals(chanStruct.inttraftx)) ||
                        (!oldTrafRx.Equals(chanStruct.victrafrx)) ||
                        (!oldEqptTx.Equals(chanStruct.inteqpttx)) ||
                        (!oldEqptRx.Equals(chanStruct.viceqptrx)))
                {
                    //...Log2.v("\nttChanCalcs(): 5-3");

                    /*	This is a new case.  Store the information and get the CTX info */
                    oldTrafTx = chanStruct.inttraftx;
                    oldTrafRx = chanStruct.victrafrx;
                    oldEqptTx = chanStruct.inteqpttx;
                    oldEqptRx = chanStruct.viceqptrx;

                    rc = TpGetDat.TpGetCtxInfo(chanStruct.inttraftx,
                                      chanStruct.victrafrx, chanStruct.viceqptrx,
                                      parmStruct.tempequip, ref curCtx, intPrintMsg,
                                      vicPrintMsg);
                }
                else
                {
                    //...Log2.v("\nttChanCalcs(): 5-4");
                    rc = oldRc;
                }

                //...Log2.v("\nttChanCalcs(): 5-5");

                oldRc = rc;
                if (rc == Error.DYN_MS_SQL_SERVER_ERR)
                {
                    return (Error.DYN_MS_SQL_SERVER_ERR); /*	Pass it back up in this case */
                }
            }
            else
            {
                //...Log2.v("\nttChanCalcs(): 5-6");
                /*	This will cause the calculations to be used. */
                rc = Constant.FAILURE;
            }

            //...Log2.v("\nttChanCalcs(): 5-7");

            /**************************************************************************\
            *
            *		If we did not find the ctx curve, then we will attempt to calculate the
            *		ctx requirements using the CTX... programs (converted to subroutines)
            *		1083 - GJS - 2003.12
            *
            \**************************************************************************/
            if (rc == Constant.SUCCESS)
            {
                /*	CTX curve found.  Get the requirements from the ctx curve returned */
                chanStruct.ctxinttraftx = curCtx.ctxtraftx;
                chanStruct.ctxvictrafrx = curCtx.ctxtrafrx;
                chanStruct.ctxeqpt = curCtx.ctxeqpt;
                chanStruct.calctype = curCtx.calcType;
                chanNulls[TtChan.CTXINTTRAFTX] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.CTXVICTRAFRX] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.CTXEQPT] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.CALCTYPE] = Constant.DB_NOT_NULL;

                /*	Store the co-channel requirement for aggregate int. repoorts
                *		- 1181 - GJS - 2006.02.04 */
                chanStruct.rqco = curCtx.rqco;
                chanNulls[TtChan.RQCO] = Constant.DB_NOT_NULL;

                /*	Set the local enumeration variable for later */
                if (chanStruct.calctype.Equals("-I"))
                {
                    ecalctype = Enums.Ecalctype.eMINUSI;
                }
                else
                {
                    ecalctype = Enums.Ecalctype.eCOVERI;
                }

                TpGetDat.TpCalcSepReqd(chanStruct.calctype, curCtx.ctxPts, fsepHi, fsepMid,
                                fsepLo, curCtx.numPts, out chanStruct.freqsep,
                                out chanStruct.reqdcalc);

                //...Log2.v("\nttChanCalcs(): 6");

                chanNulls[TtChan.FREQSEP] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.REQDCALC] = Constant.DB_NOT_NULL;
            }
            else
            {
                /*	Could not find the CTX curve, use the calculations.  First get the
                *		appropriate equipment/traffic crossreferences. */
                rc = GetCtx.GetCtxEqpt(chanStruct.victrafrx, chanStruct.viceqptrx,
                                    chanStruct.inttraftx, chanStruct.inteqpttx,
                                                out cVicType, out cIntType,
                                          out tVicAnalog, out tVicDigital, out tIntAnalog, out tIntDigital);

                //...Log2.v("\nttChanCalcs(): 7");

                if (rc == 0)
                {
                    /*	We got the crossreferences, first store the crossreference codes,
                    *		then calculate the requirements */
                    GetCtx.XrefCodes(chanStruct.inttraftx, chanStruct.inteqpttx,
                              cIntType, tIntAnalog, tIntDigital,
                                        out chanStruct.ctxinttraftx, out chanStruct.ctxinteqpt);
                    GetCtx.XrefCodes(chanStruct.victrafrx, chanStruct.viceqptrx,
                              cVicType, tVicAnalog, tVicDigital,
                                        out chanStruct.ctxvictrafrx, out chanStruct.ctxeqpt);

                    //...Log2.v("\nttChanCalcs(): 8");

                    chanNulls[TtChan.CTXINTTRAFTX] = Constant.DB_NOT_NULL;
                    chanNulls[TtChan.CTXINTEQPT] = Constant.DB_NOT_NULL;
                    chanNulls[TtChan.CTXVICTRAFRX] = Constant.DB_NOT_NULL;
                    chanNulls[TtChan.CTXEQPT] = Constant.DB_NOT_NULL;

                    /*	As soon as we have retrieved the ctx?eqpt tables, we check for
                    *		the presence of the curves we will need and flag the record if they
                    *		are not there.  The curves themselves are passed automatically to
                    *		the calculation routines.  GJS - 1215 - 2006.04.13 */
                    chanStruct.inteqtype = cIntType == 'D' ? "D" : "A";
                    chanStruct.viceqtype = cVicType == 'D' ? "D" : "A";

                    chanNulls[TtChan.INTEQTYPE] = Constant.DB_NOT_NULL;
                    chanNulls[TtChan.VICEQTYPE] = Constant.DB_NOT_NULL;

                    if (cIntType == 'D')
                    {
                        /*	Digital interferer, if there is no power spectrum store the
                        *		bandwidth */
                        if (tIntDigital.nSpect <= 0)
                        {
                            /*	There is no power spectrum, use the bandwidth */
                            chanStruct.intbwchans = tIntDigital.bandwidth;
                            chanNulls[TtChan.INTBWCHANS] = Constant.DB_NOT_NULL;
                        }
                    }

                    /*	Check the filter response */
                    if (cVicType == 'D')
                    {
                        /*	The victim is digital */
                        if (tVicDigital.nFilter <= 0)
                        {
                            chanStruct.vicbwchans = tVicDigital.bandwidth;
                            chanNulls[TtChan.VICBWCHANS] = Constant.DB_NOT_NULL;
                        }
                    }
                    else
                    {
                        /*	The victim is analog */
                        if (tVicAnalog.nFilter <= 0)
                        {
                            chanStruct.vicbwchans = (double)tVicAnalog.numchan;
                            chanNulls[TtChan.VICBWCHANS] = Constant.DB_NOT_NULL;
                        }
                    }

                    /*	Get the three requirements for the three frequencies centered around
                    *		the nominal frequency separation (+ / - the stabilities). */
                    nRet = GetCtx.GetCtxReq(cIntType, cVicType,
                                                 tVicAnalog, tVicDigital, tIntAnalog, tIntDigital,
                                                     fsepHi / 1000.0, false, out cTypeOfInt, out reqHi);

                    //...Log2.v("\nttChanCalcs(): 9");

                    if (nRet != 0)
                    {
                        /*	If this is going to be a problem, it will occur the first time
                        *		and we bail out here.  We do not need to check the rest of the
                        *		times.
                        */
                        TpRunTsip.mTW_ERR.Write("\n*** Error {0}. CTX curve too long or separation (%.0f) too great for\n    %s into %s\n    Subcase aborted.\n",
                                       nRet, fsepHi / 1000.0, intPrintMsg, vicPrintMsg);
                        TpRunTsip.mTW_ERR.Write("    Int. Type: %c, Vic. Type: %c", cIntType, cVicType);
                        TpRunTsip.mTW_ERR.Write("\n    Int Traff/Equip: {0}/{1}",
                                   chanStruct.inttraftx, chanStruct.inteqpttx);
                        TpRunTsip.mTW_ERR.Write("\n    Vic Traff/Equip: {0}/{1}",
                                   chanStruct.victrafrx, chanStruct.viceqptrx);
                        TpRunTsip.mTW_ERR.Write("\n    CTX req (tfci/tfcr/eqpr): %s / %s / %s\n",
                                   chanStruct.ctxinttraftx,
                                         chanStruct.ctxvictrafrx,
                                         chanStruct.ctxeqpt);
                        return (Constant.SUCCESS);  /*	Return success to ignore this error and carry on */
                    }
                    nRet = GetCtx.GetCtxReq(cIntType, cVicType,
                                               tVicAnalog, tVicDigital, tIntAnalog, tIntDigital,
                                                   fsepMid / 1000.0, false, out cTypeOfInt, out reqMid);
                    nRet = GetCtx.GetCtxReq(cIntType, cVicType,
                                               tVicAnalog, tVicDigital, tIntAnalog, tIntDigital,
                                                   fsepLo / 1000.0, false, out cTypeOfInt, out reqLo);

                    //...Log2.v(String.Format("\nttChanCalcs(): 10, cTypeOfInt = |{0}|", cTypeOfInt));

                    /*	Choose which of these is used depending on the type of interference */
                    if (cTypeOfInt == 'I')
                    {
                        /*	Interference only.  Choose the most negative */
                        //...Log2.v("\nttChanCalcs(): 10-1");
                        if ((reqHi <= reqMid) && (reqHi <= reqLo))
                        {
                            chanStruct.freqsep = fsepHi;
                            chanStruct.reqdcalc = reqHi;
                        }
                        else if ((reqLo <= reqMid) && (reqLo <= reqHi))
                        {
                            chanStruct.freqsep = fsepLo;
                            chanStruct.reqdcalc = reqLo;
                        }
                        else
                        {
                            chanStruct.freqsep = fsepMid;
                            chanStruct.reqdcalc = reqMid;
                        }
                        chanStruct.calctype = "I";
                        ecalctype = Enums.Ecalctype.eMINUSI;
                    }
                    else
                    {
                        //...Log2.v("\nttChanCalcs(): 10-2");
                        //	C/I
                        if ((reqHi >= reqMid) && (reqHi >= reqLo))
                        {
                            chanStruct.freqsep = fsepHi;
                            chanStruct.reqdcalc = reqHi;
                        }
                        else if ((reqLo >= reqMid) && (reqLo >= reqHi))
                        {
                            chanStruct.freqsep = fsepLo;
                            chanStruct.reqdcalc = reqLo;
                        }
                        else
                        {
                            chanStruct.freqsep = fsepMid;
                            chanStruct.reqdcalc = reqMid;
                        }
                        chanStruct.calctype = "C";
                        ecalctype = Enums.Ecalctype.eCOVERI;
                    }

                    chanNulls[TtChan.CALCTYPE] = Constant.DB_NOT_NULL;
                    chanNulls[TtChan.FREQSEP] = Constant.DB_NOT_NULL;
                    chanNulls[TtChan.REQDCALC] = Constant.DB_NOT_NULL;

                    /*	We also need to store the co-channel requirement for the Aggregate
                    *		Interference reports. - 1181 - GJS - 2006.02.04 */
                    nRet = GetCtx.GetCtxReq(cIntType, cVicType,
                                               tVicAnalog, tVicDigital, tIntAnalog, tIntDigital,
                                                   0.0, false, out cTypeOfInt, out chanStruct.rqco);

                    //...Log2.v("\nttChanCalcs(): 11");

                    chanNulls[TtChan.RQCO] = Constant.DB_NOT_NULL;

                }
                else
                {

                    /*	Could not get the equipment */
                    string str = String.Format("\nCould not retrieve equipment/traffic cross reference:-\n{0} into {1}\n{2}/{3} into {4}/{5}\n",
                                     intPrintMsg, vicPrintMsg, chanStruct.inttraftx,
                                     chanStruct.inteqpttx, chanStruct.victrafrx,
                                     chanStruct.viceqptrx);
                    Log2.e("\nTtCalcs.TtChanCalcs(): ERROR: Exit: " + str);
                    TpRunTsip.mTW_ERR.Write(str);
                    return (Constant.SUCCESS);
                }
            }

            //...Log2.v("\nTtCalcs.TtChanCalcs(): 3");

            /*----------------------------------------*/
            /* have all the data - start Calculations */
            /*----------------------------------------*/

            rc = CalcTotADisc(patNulls, chanStruct, anteStruct, ref totAdiscX,
                                   ref totAdiscC, ref copolar, ref nullTotAdX, ref totADisc);

            if (rc != Constant.SUCCESS)
            {
                /* not a fatal error print message and cont with next chan */
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(rc);
                return (Constant.SUCCESS);
            }

            //...Log2.v("\nttChanCalcs(): 12");

            /*
             * TASK 499: Check the first two letters of the traffic code for
             * both the interferer and the victim. If either one begins with
             * PS, the antenna is a PCS antenna and the PCS antenna tilt
             * discrimination has to be calculated. The order of the ground
             * heights provided to this function is determined by which
             * antenna is the PCS antenna.
             *
          *  Modified 99.03 by GJS (1021) to calculate the tilt discrimination for
          *  any antenna.
          */
            rc = TtPcsTilt(anteStruct.intacode, siteStruct.intgrnd, anteStruct.intaht,
                               siteStruct.vicgrnd, anteStruct.vicaht,
                                         siteStruct.int1vic1dist, out intTiltDisc);

            rc = TtPcsTilt(anteStruct.vicacode, siteStruct.vicgrnd, anteStruct.vicaht,
                               siteStruct.intgrnd, anteStruct.intaht,
                                         siteStruct.int1vic1dist, out vicTiltDisc);

            //...Log2.v("\nttChanCalcs(): 13");

            chanStruct.tiltdisc = vicTiltDisc + intTiltDisc;
            chanStruct.totantdisc = totADisc + chanStruct.tiltdisc;
            /*	Also add them to the co and cross polar */
            totAdiscX += chanStruct.tiltdisc;
            totAdiscC += chanStruct.tiltdisc;
            chanNulls[TtChan.TILTDISC] = Constant.DB_NOT_NULL;
            chanNulls[TtChan.TOTANTDISC] = Constant.DB_NOT_NULL;

            /*
             * TASK 421: This method selects the proper propagation loss model
             * according to the user's choice, indicated by the first character
             * in parmStruct.spherecalc.
             */
            cCalcType = parmStruct.spherecalc[0];

            if (cCalcType == '5')
            {
                /**************************************************************************\
                *     If this is an OH-loss calculation, then we first run the pathloss
                *     as if it were Free Space Loss type '3'.  Only if it fails this
                *     do we run it as an OH-Loss case. (Task 1050)
                \**************************************************************************/
                cCalcType = '3';
            }

            if (TtPathLoss(siteStruct, anteStruct, chanStruct, cCalcType, out dActualDist))
            {
                chanNulls[TtChan.PATLOSS] = Constant.DB_NOT_NULL;
            }
            else
            {
                Log2.e("\nTtCalcs.TtChanCalcs(): ERROR: Pathloss calculation failed, the Exit.");
                TpRunTsip.mTW_ERR.Write("Pathloss calculation failed\n\n");
                return Constant.FAILURE;
            }

            //...Log2.v("\nttChanCalcs(): 14");

            if ((nullViclnkAfslTx != Constant.DB_NULL) &&
                (nullAntNumTx != Constant.DB_NULL) &&
                (nullIntAfslTx != Constant.DB_NULL))
            {
                chanStruct.eirpadv = (viclnkPowTx - viclnkAFSLtx + viclnkAGain) -
                                        (intPowrTx - intAFSLtx + intAGain);
                chanNulls[TtChan.EIRPADV] = Constant.DB_NOT_NULL;
            }

            if (ecalctype == Enums.Ecalctype.eMINUSI)
            {
                //...Log2.v("\nttChanCalcs(): 14-1");
                if ((nullIntAfslTx != Constant.DB_NULL) && (nullTotAdC != Constant.DB_NULL))
                {
                    //...Log2.v("\nttChanCalcs(): 14-1-1");
                    bandTmp = intPowrTx + intAGain - chanStruct.patloss +
                                vicAGain - intAFSLtx - totAdiscC;

                    if (nullVicAfslRx != Constant.DB_NULL)
                    {
                        //...Log2.v("\nttChanCalcs(): 14-1-2");
                        chanStruct.calcico = bandTmp - vicAFSLrx;
                        chanNulls[TtChan.CALCICO] = Constant.DB_NOT_NULL;
                    }
                }

                //...Log2.v("\nttChanCalcs(): 14-1-3");

                if ((nullIntAfslTx != Constant.DB_NULL) && (nullTotAdX != Constant.DB_NULL))
                {
                    //...Log2.v("\nttChanCalcs(): 14-1-4");

                    bandTmp = intPowrTx + intAGain - chanStruct.patloss +
                                vicAGain - intAFSLtx - totAdiscX;

                    if (nullVicAfslRx != Constant.DB_NULL)
                    {
                        //...Log2.v("\nttChanCalcs(): 14-1-5");

                        chanStruct.calcixp = bandTmp - vicAFSLrx;
                        chanNulls[TtChan.CALCIXP] = Constant.DB_NOT_NULL;
                    }
                }

                //...Log2.v("\nttChanCalcs(): 14-1-6");

                if (copolar == Constant.TRUE)
                {
                    //...Log2.v("\nttChanCalcs(): 14-1-7");

                    if (chanNulls[TtChan.CALCICO] != Constant.DB_NULL)
                    {
                        //...Log2.v("\nttChanCalcs(): 14-1-8");

                        chanStruct.resti = chanStruct.reqdcalc - chanStruct.calcico;
                        chanNulls[TtChan.RESTI] = Constant.DB_NOT_NULL;
                    }
                }
                else
                {
                    //...Log2.v("\nttChanCalcs(): 14-1-9");

                    if (chanNulls[TtChan.CALCIXP] != Constant.DB_NULL)
                    {
                        //...Log2.v("\nttChanCalcs(): 14-1-10");

                        chanStruct.resti = chanStruct.reqdcalc - chanStruct.calcixp;
                        chanNulls[TtChan.RESTI] = Constant.DB_NOT_NULL;
                    }
                }

                //...Log2.v("\nttChanCalcs(): 14-1-11");

                if (chanNulls[TtChan.RESTI] != Constant.DB_NULL)
                {
                    //...Log2.v("\nttChanCalcs(): 14-1-12");

                    chanStruct.report = (short)((chanStruct.resti < parmStruct.margin) ? Constant.TRUE : Constant.FALSE);
                }

                //...Log2.v("\nttChanCalcs(): 14-1-13");
            }
            else
            {    /* calc type is C/I */

                //...Log2.v("\nttChanCalcs(): 14-2");

                if (chanNulls[TtChan.EIRPADV] != Constant.DB_NULL)
                {
                    if (nullTotAdC != Constant.DB_NULL)
                    {

                        bandTmp = intPowrTx - intAFSLtx + intAGain - chanStruct.patloss +
                                  vicAGain - totAdiscC - vicAFSLrx;

                        chanStruct.calcico = chanStruct.vicpwrrx - bandTmp;
                        chanNulls[TtChan.CALCICO] = Constant.DB_NOT_NULL;
                    }
                    if (nullTotAdX != Constant.DB_NULL)
                    {
                        bandTmp = intPowrTx - intAFSLtx + intAGain - chanStruct.patloss +
                                  vicAGain - totAdiscX - vicAFSLrx;

                        chanStruct.calcixp = chanStruct.vicpwrrx - bandTmp;
                        chanNulls[TtChan.CALCIXP] = Constant.DB_NOT_NULL;
                    }
                }

                //...Log2.v("\nttChanCalcs(): 15");

                if (copolar == Constant.TRUE)
                {
                    if (chanNulls[TtChan.CALCICO] != Constant.DB_NULL)
                    {
                        chanStruct.resti = chanStruct.calcico - chanStruct.reqdcalc;
                        chanNulls[TtChan.RESTI] = Constant.DB_NOT_NULL;
                    }
                }
                else
                {
                    if (chanNulls[TtChan.CALCIXP] != Constant.DB_NULL)
                    {
                        chanStruct.resti = chanStruct.calcixp - chanStruct.reqdcalc;
                        chanNulls[TtChan.RESTI] = Constant.DB_NOT_NULL;
                    }
                }


                if (chanNulls[TtChan.RESTI] != Constant.DB_NULL)
                {
                    chanStruct.report = (short)((chanStruct.resti < parmStruct.margin) ?
                                         Constant.TRUE :
                                         Constant.FALSE);
                }
            }

            //...Log2.v("\nttChanCalcs(): 15-A");
            /************************************************************************\
            *
            *   If we have a hit (report is true) and the selection for loss
            *   calculation was '5' (for OH-loss), then we run the actual OH-Loss
            *   calculation.  We have previously run this as a type '3' (FSL) and
            *   are only concerned if it fails this. (Task 1050)
            *
            \************************************************************************/
            if (chanStruct.report == Constant.TRUE && Strings.FirstCharIs((parmStruct.spherecalc), '5'))
            {
                //...Log2.v("\nttChanCalcs(): 15-B");
                /*  This was OH-Loss, First we recalculate the path loss using the
                *   oh-loss routines. */
                if (TtPathLoss(siteStruct, anteStruct, chanStruct, '5', out dActualDist))
                {
                }
                else
                {
                    TpRunTsip.mTW_ERR.Write("Pathloss calculation failed in OH-Loss on {0} into {1}\nResult: {2}, using Freespace loss.\n\n",
                               siteStruct.intcall1, siteStruct.viccall1, chanStruct.ohresult);
                }

                //...Log2.v("\nttChanCalcs(): 16");

                chanNulls[TtChan.PATLOSS] = Constant.DB_NOT_NULL;

                /*  The structure here is a bit different, there are three lines to
                *   display */
                if (ecalctype == Enums.Ecalctype.eMINUSI)
                {
                    /*  Get the I value in calcico and calcixp.
                    *   First get the common terms */
                    chanStruct.calcico80 =
                    chanStruct.calcico99 =
                    chanStruct.calcico = intPowrTx + intAGain - intAFSLtx +
                                            vicAGain - vicAFSLrx;
                    chanStruct.calcico -= chanStruct.patloss;
                    chanStruct.calcixp = chanStruct.calcico - totAdiscX;
                    chanStruct.calcico -= totAdiscC;
                    /******  80% of the time at the 95% CF ******/
                    chanStruct.calcico80 -= chanStruct.pathloss80;
                    chanStruct.calcixp80 = chanStruct.calcico80 - totAdiscX;
                    chanStruct.calcico80 -= totAdiscC;
                    /******  99.99% of the time at the 95% CF ******/
                    chanStruct.calcico99 -= chanStruct.pathloss99;
                    chanStruct.calcixp99 = chanStruct.calcico99 - totAdiscX;
                    chanStruct.calcico99 -= totAdiscC;

                    /*  Derive the Required values from the FSL value */
                    chanStruct.reqd80 = chanStruct.reqdcalc;
                    chanStruct.reqd99 = chanStruct.reqdcalc + 10.0;

                    /*  Calculate the margin. */
                    if (copolar == Constant.TRUE)
                    {
                        chanStruct.resti = chanStruct.reqdcalc - chanStruct.calcico;
                        chanStruct.resti80 = chanStruct.reqd80 - chanStruct.calcico80;
                        chanStruct.resti99 = chanStruct.reqd99 - chanStruct.calcico99;
                    }
                    else
                    {
                        chanStruct.resti = chanStruct.reqdcalc - chanStruct.calcixp;
                        chanStruct.resti80 = chanStruct.reqd80 - chanStruct.calcixp80;
                        chanStruct.resti99 = chanStruct.reqd99 - chanStruct.calcixp99;
                    }
                }
                else
                {
                    /*  This is a C/I ctx curve calculate the margins
                    *   First get the common terms */
                    chanStruct.calcico80 =
                    chanStruct.calcico99 =
                    chanStruct.calcico = chanStruct.vicpwrrx -
                                           (intPowrTx + intAGain - intAFSLtx +
                                            vicAGain - vicAFSLrx);
                    /*  For C/I we subtract the path loss and the antenna discrimination
                    *   from the denominator (i.e. Add it to the result) */
                    chanStruct.calcico += chanStruct.patloss;
                    chanStruct.calcixp = chanStruct.calcico + totAdiscX;
                    chanStruct.calcico += totAdiscC;
                    /******  80% of the time at the 95% CF ******/
                    chanStruct.calcico80 += chanStruct.pathloss80;
                    chanStruct.calcixp80 = chanStruct.calcico80 + totAdiscX;
                    chanStruct.calcico80 += totAdiscC;
                    /******  99.99% of the time at the 95% CF ******/
                    chanStruct.calcico99 += chanStruct.pathloss99;
                    chanStruct.calcixp99 = chanStruct.calcico99 + totAdiscX;
                    chanStruct.calcico99 += totAdiscC;

                    /*  Derive the Required values from the FSL value */
                    chanStruct.reqd80 = chanStruct.reqdcalc;
                    chanStruct.reqd99 = chanStruct.reqdcalc - 10.0;

                    /*  Calculate the margin. */
                    if (copolar == Constant.TRUE)
                    {
                        chanStruct.resti = chanStruct.calcico - chanStruct.reqdcalc;
                        chanStruct.resti80 = chanStruct.calcico80 - chanStruct.reqd80;
                        chanStruct.resti99 = chanStruct.calcico99 - chanStruct.reqd99;
                    }
                    else
                    {
                        chanStruct.resti = chanStruct.calcixp - chanStruct.reqdcalc;
                        chanStruct.resti80 = chanStruct.calcixp80 - chanStruct.reqd80;
                        chanStruct.resti99 = chanStruct.calcixp99 - chanStruct.reqd99;
                    }
                } /* -I or C/I check */

                /*  Decide if we have a case */
                if (chanStruct.resti < parmStruct.margin ||
                    chanStruct.resti80 < parmStruct.margin ||
                    chanStruct.resti99 < parmStruct.margin)
                {
                    chanStruct.report = Constant.TRUE;
                }
                else
                {
                    chanStruct.report = Constant.TRUE; /*  Turn the report off */
                }
                /*  Now set the not null flags for ingres */
                chanNulls[TtChan.CALCICO] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.PATHLOSS80] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.PATHLOSS99] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.CALCICO80] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.CALCICO99] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.CALCIXP] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.CALCIXP80] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.CALCIXP99] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.REQD80] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.REQD99] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.RESTI] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.RESTI80] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.RESTI99] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.REPORT] = Constant.DB_NOT_NULL;
                chanNulls[TtChan.OHRESULT] = Constant.DB_NOT_NULL;

            }

            //...Log2.v("\nTtCalcs.TtChanCalcs(): Exit, final");
            return (Constant.SUCCESS);
        }

        //END

        /// <summary>
        /// Calculates the interferer's and victim's antenna 
        /// gains including the special case of an antenna being  a 
        /// passive reflector.  
        /// </summary>
        /// <param name="chanStruct"> - TtChan object describing a channel.</param>
        /// <param name="anteStruct"> - TtAnte object describing an antenna.</param>
        /// <param name="siteStruct"> - TtSite object describing a site.</param>
        /// <param name="parmStruct"> - TpParm object providing parameter data.</param>
        /// <param name="interfererGain"> - the calculated gain of the interfer's antenna.</param>
        /// <param name="victimGain"> - the calculated gain of the victim's antenna.</param>
        /// <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        /// <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        /// <returns></returns>
        public static int CalcGains(TtChan chanStruct,
                                    TtAnte anteStruct,
                                    TtSite siteStruct,
                                    TpParm parmStruct,
                                    out double interfererGain,
                                    out double victimGain,
                                    string intPrintMsg,
                                    string vicPrintMsg)
        {
            // 'out' requirements.
            interfererGain = 0.0;
            victimGain = 0.0;

            string acode;

            string intTableName;
            string vicTableName;
            int rc;
            bool isMDB;
            double junk;

            /* Subsidiary antenna structure */
            SuAntStr pSubAnt;


            TpSub.TtTableName(siteStruct.interferer, parmStruct, out intTableName, out vicTableName, out isMDB);

            if (Strings.FirstCharIs(anteStruct.intcall1, '%'))
            {
                /* use interfer passive reflector gain */
                if (Strings.FirstCharIs(siteStruct.interferer, 'P'))
                {
                    rc = TtCalkPassive.TtCalcPassive(true, false, intTableName, anteStruct.intcall1,
                                         anteStruct.intcall2, anteStruct.intbndcde,
                                         anteStruct.intanum, chanStruct.intfreqtx,
                                         siteStruct.intoffax, out junk, out interfererGain,
                                         intPrintMsg, vicPrintMsg);
                }
                else
                {
                    rc = TtCalkPassive.TtCalcPassive(true, isMDB, intTableName, anteStruct.intcall1,
                                         anteStruct.intcall2, anteStruct.intbndcde,
                                         anteStruct.intanum, chanStruct.intfreqtx,
                                         siteStruct.intoffax, out junk, out interfererGain,
                                         intPrintMsg, vicPrintMsg);
                }
                if (rc != Constant.SUCCESS)
                {
                    string str = String.Format("\n***Error in Int Passive Calc. for {0}:\nInt: {1}\nVic: {2}\n",
                                   anteStruct.intcall1, intPrintMsg, vicPrintMsg);
                    Log2.e("\nTtCalcs.CalcGains(): ERROR: call to TtCalcPassive() failed: " + str);
                    TpRunTsip.mTW_ERR.Write(str);
                    return (rc);
                }
            }
            else
            {
                /* calc interfer antenna gain */
                acode = anteStruct.intacode;
                /* get antenna gain from SDB for interferer */
                /*  use the subupt routine, hopefully the antenna is in the cache */
                if ((rc = Suutils.SuGetAnt(acode, out pSubAnt)) != 0)
                {
                    TpRunTsip.mTW_ERR.Write("\n*ERROR* Can't get interferer Antenna Code [{0}], reason {1}.\n", acode, rc);
                    return (Constant.FAILURE);
                }

                interfererGain = pSubAnt.acAnt.again;
            }

            if (Strings.FirstCharIs(anteStruct.viccall1, '%'))
            {
                /* use victim passive reflector gain */
                if (Strings.FirstCharIs(siteStruct.interferer, 'P'))
                {
                    rc = TtCalkPassive.TtCalcPassive(false, isMDB, vicTableName, anteStruct.viccall1,
                                         anteStruct.viccall2, anteStruct.vicbndcde,
                                         anteStruct.vicanum, chanStruct.vicfreqrx,
                                         siteStruct.vicoffax, out junk, out victimGain,
                                         intPrintMsg, vicPrintMsg);
                }
                else
                {
                    rc = TtCalkPassive.TtCalcPassive(false, false, vicTableName, anteStruct.viccall1,
                                         anteStruct.viccall2, anteStruct.vicbndcde,
                                         anteStruct.vicanum, chanStruct.vicfreqrx,
                                         siteStruct.vicoffax, out junk, out victimGain,
                                         intPrintMsg, vicPrintMsg);
                }
                if (rc != Constant.SUCCESS)
                {
                    string str = String.Format("\n***Error in Vic Passive Calc for {0}:-\nInt: {1}\nVic: {2}\n",
                                     anteStruct.viccall1, intPrintMsg, vicPrintMsg);
                    Log2.e("\nTtCalcs.CalcGains(): ERROR: call to TtCalcPassive() failed: " + str);
                    TpRunTsip.mTW_ERR.Write(str);
                    return (rc);
                }
            }
            else
            {
                acode = anteStruct.vicacode;
                if ((rc = Suutils.SuGetAnt(acode, out pSubAnt)) != 0)
                {
                    string str = String.Format("\n*ERROR* Can't get victim Antenna Code [{0}], reason {1}.\n", acode, rc);
                    Log2.e("\nTtCalcs.CalcGains(): ERROR: call to SuGetAnt() failed: " + str);
                    TpRunTsip.mTW_ERR.Write(str);
                    return (Constant.FAILURE);
                }

                victimGain = pSubAnt.acAnt.again;
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method performs the Antenna Tilt modification and returns the calculated
        /// output discrimination.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>For the given 
        /// inteferer and victim ground height, and their distance, the elevation is 
        /// calculated between the two sites.</item> 
        /// <item>Using the elevation, we calculate the 
        /// tilt angle of the antenna, and look this up in the sd_antd table along with 
        /// the antenna code provided as the first argument.</item> 
        /// <item>The dtilt value derived 
        /// from that table is then added to the current total antenna discrimination.</item> 
        /// <item>The program looks first for the temporary antenna table and tries here 
        /// first, and then it tries the sd_antd table if there is no temporary table 
        /// or the query fails.</item>
        /// </list>
        /// </remarks>
        /// <param name="acode"> - antenna code</param>
        /// <param name="Xgrnd"> - X ground height in meters.</param>
        /// <param name="Xht"> - X antenna height in meters.</param>
        /// <param name="Ygrnd"> - Y ground height in meters.</param>
        /// <param name="Yht"> - Y antenna height in meters.</param>
        /// <param name="XYdist"> - distance between X and Y in meters.</param>
        /// <param name="disc"> - calculated output discrimination.</param>
        /// <returns></returns>
        public static int TtPcsTilt(string acode,     /*	Antenna code */
                                    double Xgrnd,     /*	X Ground in m. */
                                    double Xht,       /*	X Antenna Height */
                                    double Ygrnd,     /*	Y Ground */
                                    double Yht,       /*	Y Antenna Height */
                                    double XYdist,    /*	Distance */
                                    out double disc)  /*	Output discrimination */
        {
            // 'out' requirement.
            disc = 0.0;

            float tiltang;

            double elevXY, elevYX;
            int rc = -1;
            SuAntStr pstrAntenna = null;
            SuAntd strDisc;

            if ((rc = Suutils.SuGetAnt(acode, out pstrAntenna)) >= 0)
            {  /*  Read in the antenna */
               /*  Check that this antenna has discrimination entries */
                if (pstrAntenna.acAnt.anip > 0)
                {
                    /*  Get the elevation angle */
                    AxSub3.AxElev((Xgrnd + Xht) / 1000.0, (Ygrnd + Yht) / 1000.0, XYdist, out elevXY,
                                       out elevYX);
                    if (elevXY >= 0)
                    {
                        tiltang = 0;
                    }
                    else
                    {
                        tiltang = (float)-elevXY;
                    }
                    /*  This will return the interpolated value */
                    rc = Suutils.InterpADisc(pstrAntenna.acDscPtr, tiltang, pstrAntenna.acAnt.anip,
                                     out strDisc);

                    if (rc >= 0)
                    {
                        disc = strDisc.dtilt;
                    }
                    else
                    {
                        disc = 0.0;
                    }
                }
                else
                {
                    /*  This antenna has no discrimination curves (like a passive) */
                    disc = 0.0;
                    rc = 0;
                }
            }
            else
            {
                /*  Error in reading in the antenna */
                disc = 0.0;
            }

            return rc;
        }

        /// <summary>
        /// Calculates propagation path loss and saves the results in the
        /// TtChan object. Any one of four distinct path loss models can be applied,
        /// as prescribed by the User in the input parameter file.  
        /// </summary>
        /// <remarks>
        /// Five path loss models are currently available:
        /// <list type="bullet">
        /// <item>the standard CCIR-SJM method;  (ID '1');</item>
        /// <item>the Spherical Earth model (ID '2');</item>
        /// <item>the Free Space model (ID '3'); and</item>
        /// <item>the PCS Extension to Hata ModelCOST-231 model (ID '4').</item> 
        /// <item>the OH-Loss model provided by CTE (ID '5').</item> 
        /// </list>
        /// The height of the interfering and victim antennas are obtained, and the model is selected. 
        /// For the PCS model, the heights are matched up with the PCS or MW antenna by checking 
        /// which one has a traffic code beginning with "PC". 
        /// <para>With the exception of calcPatLoss, 
        /// which does its own internal conversion, the frequencies are converted from KHz into 
        /// MHz by dividing them by 1000 as they are passed in to the functions.</para>
        /// </remarks>
        /// <param name="siteStruct"> - TtSite object.</param>
        /// <param name="anteStruct"> - TtAnte object</param>
        /// <param name="chanStruct"> - TtChan object.</param>
        /// <param name="spherecalc"> - character ID code of the path loss model to be used (see remarks).</param>
        /// <param name="dDistUsed"> - the actual distance used.</param>
        /// <returns></returns>
        public static bool TtPathLoss(TtSite siteStruct,   /* Site structure      	*/
                              TtAnte anteStruct,     /* Antenna structure   	*/
                              TtChan chanStruct,     /* current channel data	*/
                              char spherecalc,        /* prop. loss model    	*/
                              out double dDistUsed)          /* The actual dist used */
        {
            //...Log2.v("\nTtCalcs.TtPathLoss(): Entry");

            double pcsHeight;
            double mwHeight;
            double txHeight;
            double rxHeight;
            double dActualDistKm;

            _DataStructures.OhLossXfer sctOHLoss;  /*  Structure for the OH Loss transfer */
                                                   /*  The following are used in the OH-Loss calculations to convert NAD 27
                                                   *   (used by MICS) to NAD 83 used by the OHLoss database */

            txHeight = anteStruct.intaht;
            rxHeight = anteStruct.vicaht;
            /**************************************************************************\
            *
            *		Instead of the surface distance between two sites, we use the path
            *		distance.  1179 - GJS - 2004.08.24
            *
            \**************************************************************************/
            dActualDistKm = AxSub3.PathDist((txHeight + siteStruct.intgrnd) / 1000.0,
                                   (rxHeight + siteStruct.vicgrnd) / 1000.0,
                                                         siteStruct.int1vic1dist);
            //...Log2.v("\nTtCalcs.TtPathLoss(): A");

            dDistUsed = dActualDistKm;

            switch (spherecalc)
            {
                case '1':
                    //...Log2.v("\nTtCalcs.TtPathLoss(): B");
                    GenUtil.CalcPatLoss(dActualDistKm, chanStruct.intfreqtx, out chanStruct.patloss);
                    break;

                case '2':
                    //...Log2.v("\nTtCalcs.TtPathLoss(): C");
                    txHeight += siteStruct.intgrnd;
                    rxHeight += siteStruct.vicgrnd;

                    SphericalPathLoss(rxHeight, txHeight, dActualDistKm,
                                        chanStruct.intfreqtx / 1000, out chanStruct.patloss);
                    break;

                case '3':
                    //...Log2.v("\nTtCalcs.TtPathLoss(): D");
                    GenUtil.FreeSpacePathLoss(dActualDistKm, chanStruct.intfreqtx / 1000, out chanStruct.patloss);
                    break;

                case '4':
                    //...Log2.v("\nTtCalcs.TtPathLoss(): E");
                    if (!chanStruct.inttraftx.StartsWith("PS"))
                    {
                        //...Log2.v("\nTtCalcs.TtPathLoss(): F");
                        /*
                        *   If the first two letters in inttraftx are "PS", then
                        *   the PCS antenna is the interferer and the MW antenna is
                        *   the victim.
                        */
                        pcsHeight = txHeight + siteStruct.intgrnd;
                        mwHeight = rxHeight + siteStruct.vicgrnd;

                        PCSPropLoss(pcsHeight, mwHeight, dActualDistKm,
                                      chanStruct.intfreqtx / 1000, out chanStruct.patloss);
                    }
                    else if (!chanStruct.victrafrx.StartsWith("PS"))
                    {
                        //...Log2.v("\nTtCalcs.TtPathLoss(): G");
                        /*
                         * Otherwise, the first two letters in victraftx are "PS"
                         * and the PCS antenna is the victim and the MW antenna is
                         * the interferer.
                         */
                        pcsHeight = rxHeight + siteStruct.vicgrnd;
                        mwHeight = txHeight + siteStruct.intgrnd;

                        PCSPropLoss(pcsHeight, mwHeight, dActualDistKm,
                                      chanStruct.intfreqtx / 1000, out chanStruct.patloss);
                    }
                    else
                    {
                        //...Log2.v("\nTtCalcs.TtPathLoss(): H");
                        GenUtil.FreeSpacePathLoss(dActualDistKm,
                                          chanStruct.intfreqtx / 1000,
                                            out chanStruct.patloss);
                    }
                    break;

                case '5':
                    /*  OH-Loss model, using CTE's path profile program. */
                    //...Log2.v("\nTtCalcs.TtPathLoss(): I");
                    /*  Store the FSL for the display */
                    GenUtil.FreeSpacePathLoss(dActualDistKm, chanStruct.intfreqtx / 1000, out chanStruct.patloss);

                    chanStruct.pathloss80 =
                    chanStruct.pathloss99 = chanStruct.patloss;

                    /*  If the interferer and victim are colocated, the skip this
                    *   processing */
                    if (siteStruct.int1vic1dist <= 0.001)
                    {
                        //...Log2.v("\nTtCalcs.TtPathLoss(): J");
                        /*  if the sites are co-located, we can't just set the distance to
                        *   .001 or something, because we are dealing with coordinates, so
                        *   we set the type of result to colocation and the loss to zero by
                        *   not changing the initial value at all. */
                        chanStruct.ohresult = 100;
                    }
                    else
                    {
                        //...Log2.v("\nTtCalcs.TtPathLoss(): K");
                        /*  Now set up the OHLoss structure for the routine */
                        sctOHLoss = new _DataStructures.OhLossXfer();

                        sctOHLoss.lat_1 = LatLong.FsecsToDeg(siteStruct.intlatit);
                        sctOHLoss.lng_1 = LatLong.FsecsToDeg(siteStruct.intlongit);
                        sctOHLoss.lat_2 = LatLong.FsecsToDeg(siteStruct.viclatit);
                        sctOHLoss.lng_2 = LatLong.FsecsToDeg(siteStruct.viclongit);

                        /*  a zero antenna height will break the ohloss routine */
                        if (anteStruct.intaht > 0.0)
                        {
                            sctOHLoss.s1_anthght = anteStruct.intaht;
                        }
                        else
                        {
                            TpRunTsip.mTW_ERR.Write("\n** WARNING ** {0}-{1}: Interfering antenna height of zero. 10m used.\n",
                                    siteStruct.intcall1, siteStruct.intname1);
                            sctOHLoss.s1_anthght = 10.0;
                        }

                        if (anteStruct.vicaht > 0.0)
                        {
                            sctOHLoss.s2_anthght = anteStruct.vicaht;
                        }
                        else
                        {
                            TpRunTsip.mTW_ERR.Write("\n** WARNING ** {0}-{1}: Victim antenna height of zero. 10m used.\n",
                                    siteStruct.viccall1, siteStruct.vicname1);
                            sctOHLoss.s2_anthght = 10.0;
                        }

                        sctOHLoss.freq = chanStruct.intfreqtx / 1000.0;

                        if (chanStruct.intpolar.Equals("H"))
                        {
                            sctOHLoss.polarization = 0;             /*  Horizontal polarization */
                        }
                        else
                        {
                            sctOHLoss.polarization = 1;             /*  Anything else we assume Vertical */
                        }

                        sctOHLoss.clim_region = 0;         /*  Continental temperate */
                        sctOHLoss.K_median = 1.333333;  /*  K  */

                        //...Log2.v("\nTtCalcs.TtPathLoss(): L");
                        CTEfunctions.Calc_OhLoss(ref sctOHLoss);
                        //...Log2.v("\nTtCalcs.TtPathLoss(): L-1");

                        if (sctOHLoss.error_status == Enums.PRF.OK)
                        {

                            chanStruct.pathloss80 += sctOHLoss.ohloss_95[1];
                            chanStruct.pathloss99 += sctOHLoss.ohloss_95[5];
                            chanStruct.ohresult = sctOHLoss.calc_type;

                            dDistUsed = sctOHLoss.eff_dist;        /*	The distance used is the
					                                    *		effective distance      */
                                                                   //	Tell the report that 50,000:1 maps used, without changing the calling
                                                                   //	sequence or table structure.
                            if (sctOHLoss.cded50_files_used)
                            {
                                chanStruct.ohresult += 1000;
                            }

                        }
                        else if (sctOHLoss.error_status == Enums.PRF.NOMAP)
                        {
                            //...Log2.v("\nTtCalcs.TtPathLoss(): M");
                            /*	No map was present.  This should not be fatal.  Print an error
                            *		message and do free space loss */
                            TpRunTsip.mTW_ERR.Write("\n** WARNING ** Map ({0}) for path from %s to %s \n              is missing. Using FSL.\n",
                                             sctOHLoss.map_name,
                                             siteStruct.intcall1,
                                             siteStruct.viccall1);

                            GenUtil.FreeSpacePathLoss(dActualDistKm, chanStruct.intfreqtx / 1000, out chanStruct.patloss);

                            chanStruct.pathloss80 += chanStruct.patloss;
                            chanStruct.pathloss99 += chanStruct.patloss;
                            chanStruct.ohresult = -1;   /*	Non-terminal error */

                            return false;
                        }
                        else
                        {
                            //...Log2.v("\nTtCalcs.TtPathLoss(): N");
                            /*  Store the actual error in the ohresult field.  Use free space
                                      *		loss. */
                            GenUtil.FreeSpacePathLoss(dActualDistKm, chanStruct.intfreqtx / 1000, out chanStruct.patloss);

                            chanStruct.pathloss80 += chanStruct.patloss;
                            chanStruct.pathloss99 += chanStruct.patloss;
                            chanStruct.ohresult = -(int)sctOHLoss.error_status;

                            return false;
                        }
                    }
                    break;

                default:
                    return false;
            }

            //...Log2.v("\nTtCalcs.TtPathLoss(): Exit");
            return true;
        }

        /// <summary>
        /// This method performs a Ts-Ts propagation path loss calculation 
        /// i.a.w. the Spherical Earth model.  
        /// </summary>
        /// <param name="rxHeight"> - recieve antenna's height.</param>
        /// <param name="txHeight"> - transmit antenna's height.</param>
        /// <param name="plength"> - distance between recieve and transmit antennae.</param>
        /// <param name="freq"> - transmission frequency.</param>
        /// <param name="ploss"> - calculated path loss.</param>
        public static void SphericalPathLoss(double rxHeight, double txHeight, double plength,
                                             double freq, out double ploss)
        {
            double transDist;                   /* Transition distance in km   */
            double theta, H, h, N, FSL; /* Temporary variables for calculation */

            transDist = 4.123 * (Sqrt(rxHeight) + Sqrt(txHeight));
            FSL = 32.45 + (20.0 * Log10(plength)) + (20.0 * Log10(freq));

            if (plength > transDist)
            {
                theta = (plength - transDist) / 8.5;
                H = theta * plength / 4000;
                h = 1.063 * (theta * theta) * .001;
                N = (20.0 * Log10(5.0 + (.27 * H))) + (1.17261 * h);
                ploss = 29.73 + (30.0 * Log10(freq)) +
                                 (10.0 * Log10(plength)) + (30.0 * Log10(theta)) + N;
                if (ploss < FSL) ploss = FSL;
            }
            else
            {
                ploss = FSL;
            }
        }

        /// <summary>
        /// Calculates and returns the propagation path loss i.a.w. the PCS Extension
        /// to the Hata ModelCOST-231 Model. 
        /// </summary>
        /// <remarks>
        /// Note the use of explicit multiplications instead of using the math library pow() 
        /// function; early, and even recent, versions of the UNIX, Linux and VC++ standard
        /// math library had problems with the pow(x, n) function when n was an integer and
        /// especially for n= 2.
        /// </remarks>
        /// <param name="pcsHeight"> - TBD.</param>
        /// <param name="mwHeight"> - TBD.</param>
        /// <param name="plength"> - TBD.</param>
        /// <param name="freq"> - operating frequency.</param>
        /// <param name="pLoss"> - calculated path loss.</param>
        public static void PCSPropLoss(double pcsHeight,
                                  double mwHeight,
                                    double plength,
                                    double freq,
                                    out double pLoss)
        {
            // 'out' requirement.
            pLoss = 0.0;

            double transDist,                   /* Transition distance in km	*/
                theta, H, h, N, FSL, alpha;   /* Temporary variables for calculation */

            transDist = 4.123 * (Sqrt(pcsHeight) + Sqrt(mwHeight));
            FSL = 32.45 + (20.0 * Log10(plength)) + (20.0 * Log10(freq));

            if (plength > transDist)
            {
                /*
                 * propagation Loss: pLoss
                 * (Hourly median transmission loss 50% of the time)
                 */
                theta = (plength - transDist) / 8.5;    /* in milliradians	*/
                H = theta * plength / 4000;
                h = 1.063 * theta * theta * .001;
                N = (20.0 * Log10(5 + (.27 * H))) + (1.17261 * h);
                pLoss = 29.73 + (30.0 * Log10(freq)) + (10.0 * Log10(plength)) + (30.0 * Log10(theta)) + N;
                if (pLoss < FSL) pLoss = FSL;

            }
            else
            {
                if (pcsHeight > 60.0)
                {
                    pLoss = FSL;
                }
                else
                {
                    if (pcsHeight <= 9.0)
                    {
                        alpha = (((1.1 * Log10(freq)) - 0.7) * pcsHeight) -
                                      ((1.56 * Log10(freq)) - 0.8);
                    }
                    else if (pcsHeight <= 28.0)
                    {
                        alpha = (2.68 * pcsHeight) - 3.53 - (0.1017 * pcsHeight *
                                  pcsHeight) + (.00152 * pcsHeight * pcsHeight * pcsHeight);
                    }
                    else
                    {
                        alpha = 25.49 + (19.92 * Log10(pcsHeight / 28.0));
                    }
                    pLoss = 69.55 + (26.16 * Log10(freq)) - (13.82 * Log10(mwHeight)) +
                               ((44.9 - (6.55 * Log10(mwHeight))) * Log10(plength)) -
                               (2.0 * Log10(freq / 28) * Log10(freq / 28)) - 11.4 - alpha;
                }
            }
        }

        /// <summary>
        /// This method retrieves remote site data (for a victim site) from the DB. 
        /// </summary>
        /// <param name="chanStruct"> - TtChan object.</param>
        /// <param name="anteStruct"> - TtAnte object.</param>
        /// <param name="siteStruct"> - TtSite object.</param>
        /// <param name="parmStruct"> - TpParm object providing parameter data.</param>
        /// <param name="vicTableName"> - unique part of victim table name.</param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        /// <param name="vicMbnd"> - victim's mid-band frequency.</param>
        /// <param name="viclnkAFSLtx"> - victim's antenna feed system loss.</param>
        /// <param name="viclnkANumTx"> - victim's lik antenna number.</param>
        /// <param name="viclnkPowTx"> - victim's link transmit power.</param>
        /// <param name="viclnkAntGain"> - victim's link antenna gain.</param>
        /// <param name="nullViclnkAfslTx"> - ODBC nullInds associated with viclnkAFSLtx.</param>
        /// <param name="nullAntNumTx"> - ODBC nullInds associated with viclnkANumTx.</param>
        /// <param name="nullPowTx"> - ODBC nullInds associated with viclnkPowTx.</param>
        /// <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        /// <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        /// <returns></returns>
        public static int GetVicLnkInfo(TtChan chanStruct,
                                                 TtAnte anteStruct,
                                                 TtSite siteStruct,
                                                 TpParm parmStruct,
                                                 string vicTableName,
                                                 bool isMDB,
                                                 double vicMbnd,
                                                 ref double viclnkAFSLtx,
                                                 ref short viclnkANumTx,
                                                 ref double viclnkPowTx,
                                                 ref double viclnkAntGain,
                                                 ref SQLLEN nullViclnkAfslTx,
                                                 ref SQLLEN nullAntNumTx,
                                                 ref SQLLEN nullPowTx,
                                                 string intPrintMsg,
                                                 string vicPrintMsg)
        {
            string tmpACode;
            SuAntStr pAnt;

            int rc;
            SQLLEN[] viclnkChanNulls;  //[FtChan.SIZE_];
            SQLLEN[] tmpAnteNulls;  //[FtAnte.SIZE_];
            FtChan ftTmpChan;
            FtAnte ftTmpAnte;
            double junk;
            string select = "";
            string vicChanTable;
            string vicAnteTable;

            GenUtil.UtCvtName(Constant.FT_CHAN, vicTableName, out vicChanTable);
            GenUtil.UtCvtName(Constant.FT_ANTE, vicTableName, out vicAnteTable);

            select = String.Format("call2='{0}' and call1='{1}' and bndcde='{2}' and chid='{3}'",
                                chanStruct.viccall2, chanStruct.viccall1, chanStruct.vicbndcde, chanStruct.vicchid);
            if (Strings.FirstCharIs(siteStruct.interferer, 'E'))
            {
                rc = TpMdbPdfGet.TtChanGet(chanStruct.viccall2, chanStruct.viccall1, chanStruct.vicbndcde,
                                 chanStruct.vicchid, vicChanTable, false, out ftTmpChan, out viclnkChanNulls);
            }
            else
            {
                rc = TpMdbPdfGet.TtChanGet(chanStruct.viccall2, chanStruct.viccall1, chanStruct.vicbndcde,
                                 chanStruct.vicchid, vicChanTable, isMDB, out ftTmpChan, out viclnkChanNulls);
            }
            if (rc != Constant.SUCCESS)
            {
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "CHAN", select);
                if (rc == Constant.FAILURE)
                {
                    if (Strings.FirstCharIs(siteStruct.interferer, 'E'))
                    {
                        /* if we looked for the remote channel in the PDF and didn't find it this is
                         * simply a warning - ie don;t return a hard and fast stop */
                        return (Constant.SUCCESS);
                    }
                }
                return (rc);
            }

            if (Strings.FirstCharIs(chanStruct.viccall2, '%'))
            {
                viclnkAFSLtx = 0.0;
                nullViclnkAfslTx = Constant.DB_NOT_NULL;
                nullAntNumTx = Constant.DB_NULL;
            }
            else
            {
                if (chanStruct.txant == 1)
                {
                    viclnkAFSLtx = ftTmpChan.afsltx1;
                    nullViclnkAfslTx = viclnkChanNulls[FtChan.AFSLTX1];
                    viclnkANumTx = ftTmpChan.antnumbtx1;
                    nullAntNumTx = viclnkChanNulls[FtChan.ANTNUMBTX1];
                }
                else
                {
                    viclnkAFSLtx = ftTmpChan.afsltx2;
                    nullViclnkAfslTx = viclnkChanNulls[FtChan.AFSLTX2];
                    viclnkANumTx = ftTmpChan.antnumbtx2;
                    nullAntNumTx = viclnkChanNulls[FtChan.ANTNUMBTX2];
                }
            }
            viclnkPowTx = ftTmpChan.pwrtx;
            nullPowTx = viclnkChanNulls[FtChan.PWRTX];


            if (nullAntNumTx != Constant.DB_NULL)
            {
                /* need to get the victim link's antenna information */
                select = String.Format("call1 = '{0,-9:S}' and call2 = '{1,-9:S}' and bndcde = '{2,-4:S}' and anum = {3}",
                    anteStruct.viccall2, anteStruct.viccall1,
                    anteStruct.vicbndcde, viclnkANumTx);

                if (Strings.FirstCharIs(siteStruct.interferer, 'E'))
                {
                    rc = TpMdbPdfGet.TtAnteGet(anteStruct.viccall2, anteStruct.viccall1, anteStruct.vicbndcde,
                                                 viclnkANumTx, vicAnteTable, false, out ftTmpAnte, out tmpAnteNulls);
                }
                else
                {
                    rc = TpMdbPdfGet.TtAnteGet(anteStruct.viccall2, anteStruct.viccall1, anteStruct.vicbndcde,
                                                 viclnkANumTx, vicAnteTable, isMDB, out ftTmpAnte, out tmpAnteNulls);
                }
                if (rc != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "ANTE", select);

                    if (rc == Constant.FAILURE)
                    {
                        if (Strings.FirstCharIs(siteStruct.interferer, 'E'))
                        {
                            /* can't find remote antenna in the PDF return a success message
                             * rather than an error	 */
                            return (Constant.SUCCESS);
                        }
                    }
                    return (rc);
                }

                /* record found for this antenna - store info */
                tmpACode = ftTmpAnte.acode;

                select = String.Format("acode = '%-12s'", tmpACode);

                if ((rc = Suutils.SuGetAnt(tmpACode, out pAnt)) != 0)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "ANTE", tmpACode);
                    if (rc != Constant.NOMORERECS)
                    {
                        return (Error.DYN_MS_SQL_SERVER_ERR);
                    }
                    else
                    {
                        return (Constant.FAILURE);
                    }
                }

                viclnkAntGain = pAnt.acAnt.again;

                rc = Constant.SUCCESS;
            }
            else if (Strings.FirstCharIs(chanStruct.viccall2, '%'))
            {
                /*	Other end is a passive.  */
                /* calculate victim's passive reflector and discrimination at call2 */
                if (Strings.FirstCharIs(siteStruct.interferer, 'P'))
                {
                    rc = TtCalkPassive.TtCalcPassive(false, isMDB, vicTableName, anteStruct.viccall2,
                                         anteStruct.viccall1, anteStruct.vicbndcde,
                                         /* anteStruct.vicanum */ 0, vicMbnd,
                                         siteStruct.vicoffax, out junk, out viclnkAntGain,
                                         intPrintMsg, vicPrintMsg);
                }
                else
                {
                    rc = TtCalkPassive.TtCalcPassive(false, false, vicTableName, anteStruct.viccall2,
                                         anteStruct.viccall1, anteStruct.vicbndcde,
                                         /* anteStruct.vicanum */ 0, vicMbnd,
                                         siteStruct.vicoffax, out junk, out viclnkAntGain,
                                         intPrintMsg, vicPrintMsg);
                }
                if (rc == Constant.SUCCESS)
                {
                    nullAntNumTx = Constant.DB_NOT_NULL;
                }
            }
            return (rc);
        }

        /// <summary>
        /// This method identifies extraneous point-to-multipoint cases 
        /// that do not need to be reported.  
        /// </summary>
        /// <param name="chanStruct"> - TtChan object to be vetted.</param>
        /// <returns>
        /// Either:
        /// <list type="bullet">
        /// <item>Error.CONTINUE; or</item>
        /// <item>Constant.PC_SKIP.</item>
        /// </list>
        /// </returns>
        public static int MultiPointCalcs(TtChan chanStruct)
        {
            if (!chanStruct.inttraftx.StartsWith("PM") &&
                !chanStruct.inttraftx.StartsWith("PS"))
            {
                return (Error.CONTINUE);
            }

            else if (!chanStruct.inttraftx.Equals(chanStruct.victrafrx))
            {
                return (Error.CONTINUE);
            }

            else if (chanStruct.intfreqtx != chanStruct.vicfreqrx)
            {
                return (Error.CONTINUE);
            }

            else if (chanStruct.intcall2.Equals(chanStruct.viccall1) ||
                chanStruct.intcall1.Equals(chanStruct.viccall2))
            {
                return (Constant.PC_SKIP);
            }

            else
            {
                return (Error.CONTINUE);
            }
        }

        /// <summary>
        /// Calculates the total discrimination for the given 
        /// antenna.  
        /// </summary>
        /// <param name="anteNulls"> - ODBC nullInds associated with anteStruct.</param>
        /// <param name="chanStruct"> - TtChan object.</param>
        /// <param name="anteStruct"> - TtAnte object.</param>
        /// <param name="totAdiscX"> - TBD.</param>
        /// <param name="totAdiscC"> - TBD.</param>
        /// <param name="copolar"> - TBD.</param>
        /// <param name="nullTotAdX"> - ODBC nullInd associated with totAdiscX.</param>
        /// <param name="totAdisc"> - calculated total antenna discrimination.</param>
        /// <returns></returns>
        public static int CalcTotADisc(SQLLEN[] anteNulls,
                                        TtChan chanStruct,
                                        TtAnte anteStruct,
                                        ref double totAdiscX,
                                        ref double totAdiscC,
                                        ref short copolar,
                                        ref SQLLEN nullTotAdX,
                                        ref double totAdisc)
        {
            double horizCopolar = 0.0;
            double vertCopolar = 0.0;
            double hvXpolar1 = 0.0;
            double hvXpolar2 = 0.0;
            double vhXpolar1 = 0.0;
            double vhXpolar2 = 0.0;
            double tmpTotAdiscX = 0.0;
            SQLLEN nullHorizCo;
            SQLLEN nullVertCo;
            SQLLEN nullTotAdC = Constant.DB_NOT_NULL;
            SQLLEN tempNull = 0;
            int rc;
            string cIntPol;
            string cVicPol;

            /*
            *		We handle circular polarizations like B for now.  So we check for the
            *		circular polarization and convert it to B if found.
            */
            ConvertPol(out cIntPol, chanStruct.intpolar);
            ConvertPol(out cVicPol, chanStruct.vicpolar);

            if ((anteNulls[TtAnte.ADISCCTXH] != Constant.DB_NULL) &&
                (anteNulls[TtAnte.ADISCCRXH] != Constant.DB_NULL))
            {
                horizCopolar = anteStruct.adiscctxh + anteStruct.adisccrxh;
                nullHorizCo = Constant.DB_NOT_NULL;
            }
            else
            {
                nullHorizCo = Constant.DB_NULL;
            }
            if ((anteNulls[TtAnte.ADISCCTXV] != Constant.DB_NULL) &&
                (anteNulls[TtAnte.ADISCCRXV] != Constant.DB_NULL))
            {
                vertCopolar = anteStruct.adiscctxv + anteStruct.adisccrxv;
                nullVertCo = Constant.DB_NOT_NULL;
            }
            else
            {
                nullVertCo = Constant.DB_NULL;
            }

            /*------------------------*
             * for intpolar is H or B *
             *------------------------*/
            if ((Strings.FirstCharIs(cIntPol, 'H')) || (Strings.FirstCharIs(cIntPol, 'B')))
            {
                nullTotAdX = Constant.DB_NOT_NULL;
                if ((anteNulls[TtAnte.ADISCXTXH] != Constant.DB_NULL) &&
                    (anteNulls[TtAnte.ADISCCRXV] != Constant.DB_NULL))
                {
                    hvXpolar1 = anteStruct.adiscxtxh + anteStruct.adisccrxv;
                    if ((anteNulls[TtAnte.ADISCCTXH] != Constant.DB_NULL) &&
                        (anteNulls[TtAnte.ADISCXRXV] != Constant.DB_NULL))
                    {
                        hvXpolar2 = anteStruct.adiscctxh + anteStruct.adiscxrxv;
                        totAdiscX = (hvXpolar1 < hvXpolar2) ? hvXpolar1 : hvXpolar2;
                    }
                    else
                    {
                        totAdiscX = hvXpolar1;
                    }
                }
                else
                {
                    if ((anteNulls[TtAnte.ADISCCTXH] != Constant.DB_NULL) &&
                        (anteNulls[TtAnte.ADISCXRXV] != Constant.DB_NULL))
                    {
                        totAdiscX = anteStruct.adiscctxh + anteStruct.adiscxrxv;
                    }
                    else
                    {
                        nullTotAdX = Constant.DB_NULL;
                    }
                }
                if (Strings.FirstCharIs(cIntPol, 'B'))
                {
                    tmpTotAdiscX = totAdiscX;
                    tempNull = nullTotAdX;
                }
            }

            /*------------------------*
             * for intpolar is V or B *
             *------------------------*/
            if ((Strings.FirstCharIs(cIntPol, 'V')) || (Strings.FirstCharIs(cIntPol, 'B')))
            {
                nullTotAdX = Constant.DB_NOT_NULL;
                if ((anteNulls[TtAnte.ADISCXTXV] != Constant.DB_NULL) &&
                    (anteNulls[TtAnte.ADISCCRXH] != Constant.DB_NULL))
                {
                    vhXpolar1 = anteStruct.adiscxtxv + anteStruct.adisccrxh;
                    if ((anteNulls[TtAnte.ADISCCTXV] != Constant.DB_NULL) &&
                        (anteNulls[TtAnte.ADISCXRXH] != Constant.DB_NULL))
                    {
                        vhXpolar2 = anteStruct.adiscctxv + anteStruct.adiscxrxh;
                        totAdiscX = (vhXpolar1 < vhXpolar2) ? vhXpolar1 : vhXpolar2;
                    }
                    else
                    {
                        totAdiscX = vhXpolar1;
                    }
                }
                else
                {
                    if ((anteNulls[TtAnte.ADISCCTXV] != Constant.DB_NULL)
                        && (anteNulls[TtAnte.ADISCXRXH] != Constant.DB_NULL))
                    {
                        totAdiscX = anteStruct.adiscctxv + anteStruct.adiscxrxh;
                    }
                    else
                    {
                        nullTotAdX = Constant.DB_NULL;
                    }
                }
                if (Strings.FirstCharIs(cIntPol, 'B'))
                {
                    if (nullTotAdX == Constant.DB_NULL)
                    {
                        nullTotAdX = tempNull;
                        totAdiscX = tmpTotAdiscX;
                    }
                    else if (tempNull != Constant.DB_NULL)
                    {
                        totAdiscX = (tmpTotAdiscX < totAdiscX) ? tmpTotAdiscX : totAdiscX;
                    }
                }
            }

            /*----------------------------------------------------*/
            if (Strings.FirstCharIs(cIntPol, 'H'))
            {
                if (Strings.FirstCharIs(cVicPol, 'H'))
                {
                    if ((rc = DiscOfSingleDirAnt(nullHorizCo, horizCopolar, Constant.TRUE, out totAdisc, out copolar)) != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                }
                else if (Strings.FirstCharIs(cVicPol, 'V'))
                {
                    if ((rc = DiscOfSingleDirAnt(nullTotAdX, totAdiscX, Constant.FALSE, out totAdisc, out copolar)) != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                }
                else
                {   /* chanStruct.vicpolar ==  "B" */
                    if ((rc = DiscOfMultiDirAnt(nullTotAdX, nullHorizCo, totAdiscX, horizCopolar, Constant.FALSE, out totAdisc, out copolar)) != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                }
                if (nullHorizCo == Constant.DB_NULL)
                {
                    nullTotAdC = Constant.DB_NULL;
                }
                else
                {
                    totAdiscC = horizCopolar;
                }
            }
            else if (Strings.FirstCharIs(cIntPol, 'V'))
            {
                if (Strings.FirstCharIs(cVicPol, 'H'))
                {
                    if ((rc = DiscOfSingleDirAnt(nullTotAdX, totAdiscX, Constant.FALSE, out totAdisc, out copolar)) != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                }
                else if (Strings.FirstCharIs(cVicPol, 'V'))
                {
                    if ((rc = DiscOfSingleDirAnt(nullVertCo, vertCopolar, Constant.TRUE, out totAdisc, out copolar)) != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                }
                else
                {   /* chanStruct.vicpolar ==  "B" */
                    if ((rc = DiscOfMultiDirAnt(nullTotAdX, nullVertCo, totAdiscX, vertCopolar, Constant.FALSE, out totAdisc, out copolar)) != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                }
                if (nullVertCo == Constant.DB_NULL)
                {
                    nullTotAdC = Constant.DB_NULL;
                }
                else
                {
                    totAdiscC = vertCopolar;
                }
            }
            else
            {   /* chanStruct.intpolar == "B"  or circular */
                /*
                 * TASK 504: Added the second part of the following conditions, dealing
                 * with the victim polarization. Previously, this section of the code seemed
                 * to assume that the victim polarization was 'B'.
                 */
                if ((nullHorizCo == Constant.DB_NULL) || (Strings.FirstCharIs(cVicPol, 'V')))
                {
                    if (nullVertCo == Constant.DB_NULL)
                    {
                        nullTotAdC = Constant.DB_NULL;
                    }
                    else
                    {
                        totAdiscC = vertCopolar;
                    }
                }
                else
                {
                    if ((nullVertCo == Constant.DB_NULL) || (Strings.FirstCharIs(cVicPol, 'H')))
                    {
                        totAdiscC = horizCopolar;
                    }
                    else
                    {
                        totAdiscC = (horizCopolar < vertCopolar) ? horizCopolar : vertCopolar;
                    }
                }
                if (Strings.FirstCharIs(cVicPol, 'H'))
                {
                    if ((rc = DiscOfMultiDirAnt(nullTotAdX, nullHorizCo, totAdiscX, horizCopolar, Constant.FALSE, out totAdisc, out copolar)) != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                }
                else if (Strings.FirstCharIs(cVicPol, 'V'))
                {
                    if ((rc = DiscOfMultiDirAnt(nullTotAdX, nullVertCo, totAdiscX, vertCopolar, Constant.FALSE, out totAdisc, out copolar)) != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                }
                else
                {   /* chanStruct.vicpolar ==  "B" or circular */
                    if (nullTotAdX == Constant.DB_NULL)
                    {
                        if ((rc = DiscOfSingleDirAnt(nullTotAdC, totAdiscC, Constant.TRUE, out totAdisc, out copolar)) != Constant.SUCCESS)
                        {
                            return (rc);
                        }
                    }
                    else
                    {
                        if (nullTotAdC == Constant.DB_NULL)
                        {
                            totAdisc = totAdiscX;
                            copolar = Constant.FALSE;
                        }
                        else
                        {
                            if (totAdiscC < totAdiscX)
                            {
                                totAdisc = totAdiscC;
                                copolar = Constant.TRUE;
                            }
                            else
                            {
                                totAdisc = totAdiscX;
                                copolar = Constant.FALSE;
                            }
                        }
                    }
                }
            }
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method checks an input polarization code for circular polarization and, if found,
        /// converts/outputs it as "B"; otherwise the output polarization code is the same as the input.
        /// </summary>
        /// <param name="outpol"> - output polarization code.</param>
        /// <param name="inpol"> - input polarization code.</param>
        public static void ConvertPol(out string outpol, string inpol)
        {
            // 'out' requirement.
            outpol = "";

            if (!"CLR".Contains(inpol.ToUpper()[0]))
            {
                //	The input is not circular polarized
                outpol = inpol.ToUpper();
            }
            else
            {
                outpol = "B";
            }

            return;
        }

        /// <summary>
        /// This method determines the total antenna 
        /// discrimination between two single directional antennae. The orientation of 
        /// the two antennae may be copolar or crosspolar.  
        /// </summary>
        /// <param name="nullDisc"> - ODBC nullInd associated with disc.</param>
        /// <param name="disc"> - calculated discrimination.</param>
        /// <param name="samePolar"> - flag denoting the orientation.</param>
        /// <param name="totantdisc"> - resulting discrimination</param>
        /// <param name="copolar"> - resulting orientation.</param>
        /// <returns></returns>
        public static int DiscOfSingleDirAnt(SQLLEN nullDisc,  /* null value of disc */
                                                double disc,    /* calc'd discrimination */
                                                short samePolar,    /* flag for orientation */
                                                out double totantdisc,/* resulting discrim */
                                                out short copolar) /* resulting orientation */
        {
            // 'out' requirements.
            totantdisc = 0.0; ;
            copolar = -666;

            if (nullDisc == Constant.DB_NULL)
            {
                return (Error.SINGLEDIR_ADISC);
            }

            totantdisc = disc;
            copolar = samePolar;

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method determines the total antenna 
        /// discrimination between one single and one multi-directional antenna. The 
        /// orientation of the two antenna is also determined to be copolar or 
        /// crosspolar.  
        /// </summary>
        /// <param name="nullXpDisc"> - ODBC nullInd associated with discXp.</param>
        /// <param name="nullCpDisc"> - ODBC nullInd associated with discCo.</param>
        /// <param name="discXp"> - calculated discrimination of crosspolar orientation.</param>
        /// <param name="discCo"> - calculated discrimination of copolar orientation</param>
        /// <param name="samePolar"> - flag indicating the xpolar orientation.</param>
        /// <param name="totantdisc"> - resulting total antenna discrimination.</param>
        /// <param name="copolar"> - flag indicating the resulting orientation.</param>
        /// <returns></returns>
        public static int DiscOfMultiDirAnt(SQLLEN nullXpDisc, /* null value of crosspolar antenna disc */
                                            SQLLEN nullCpDisc,  /* null value of copolar antenna disc */
                                            double discXp,  /* calc'd discrimination of cross polar orientation */
                                            double discCo,  /* calc'd discrimination of co polar orientation */
                                            short samePolar,    /* flag for xpolar orientation*/
                                            out double totantdisc,/* resulting discrim */
                                            out short copolar) /* resulting orientation */
        {
            // 'out' requirements.
            totantdisc = 0.0;
            copolar = -666;

            int rc;

            if (nullCpDisc == Constant.DB_NULL)
            {
                if ((rc = DiscOfSingleDirAnt(nullXpDisc, discXp, samePolar,
                    out totantdisc, out copolar)) != Constant.SUCCESS)
                {
                    return (rc);
                }
            }
            else
            {
                if (nullXpDisc == Constant.DB_NULL)
                {
                    totantdisc = discCo;
                    copolar = Constant.TRUE;
                }
                else
                {
                    if (discCo < discXp)
                    {
                        totantdisc = discCo;
                        copolar = Constant.TRUE;
                    }
                    else
                    {
                        totantdisc = discXp;
                        copolar = Constant.FALSE;
                    }
                }
            }

            return (Constant.SUCCESS);
        }



    }
}

```
