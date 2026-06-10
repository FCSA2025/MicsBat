# Documented File: TtCalcPassive.cs
**Repository Path:** `TpRunTsip 20231124\TtCalcPassive.cs`
**Primary Layer:** `TpRunTsip 20231124`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using _Auxlib;
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static _NewLib.Maths;
using _Configuration;
using _Utillib;
using _NewLib;

namespace TpRunTsip
{
    /// <summary>
    /// Provides methods that perform the 'passive' calculations for TSIP.
    /// </summary>
    /// <remarks>
    /// The 'k' in TtCalkPassive is not a typo. This class contains a legacy method
    /// called TtCalcPassive(); a class can't have a method name identical to its class name.
    /// </remarks>
    public class TtCalkPassive
    {
        /// <summary>
        /// Calculates the discrimination and antenna gains 
        /// for a passive antenna.  
        /// </summary>
        /// <param name="isInt"> - not used.</param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        /// <param name="TableName"> - antenna input file name.</param>
        /// <param name="call1"> - passive Call Sign</param>
        /// <param name="call2"> - other end from passive.</param>
        /// <param name="bndcde"> - band code.</param>
        /// <param name="anum"> - antenna number.</param>
        /// <param name="freq"> - operating frequency.</param>
        /// <param name="offAxisAng"> off-axis angle.</param>
        /// <param name="disc"> - discrimination.</param>
        /// <param name="pGain"> - antenna gain.</param>
        /// <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        /// <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        /// <returns></returns>
        public static int TtCalcPassive(bool isInt,
                        bool isMDB,
                        string TableName,           /*	antenna input file name  */
                        string call1,                       /*	Passive Call Sign				 */
                        string call2,                       /*	Other end from Passive   */
                        string bndcde,
                        short anum,
                        double freq,
                        double offAxisAng,
                        out double disc,
                        out double pGain,
                        string intPrintMsg,
                        string vicPrintMsg)
        {
            //...Log2.v("\nTtCalcPassive.TtCalcPassive(): Entry");

            // 'out' requirements.
            disc = 0.0;
            pGain = 0.0;

            string acodeP;
            int rc;
            double height;
            double width;
            double htB;
            double htA;
            double htP;
            double distAP;
            double azimAP;
            double azimPA;
            double clHtA;
            double clHtB;
            double distBP;
            double azimBP;
            double azimPB;
            double xPA;
            double yPA;
            double zPA;
            double xPB;
            double yPB;
            double zPB;
            double Y;
            double iAng;
            double clHtP;
            double elevAP;
            double elevPA;
            double elevBP;
            double elevPB;

            FtSite siteA;
            FtSite siteB;
            FtSite siteP;

            double dTGain;
            double dAreaOrGain;

            /*  first establish the geometry.  */
            if ((rc = SitesAPB(isInt, isMDB, TableName,
                               call1, call2, bndcde, anum, out acodeP, out htA, out htB, out htP,
                               out siteA, out siteB, out siteP, out dTGain, out dAreaOrGain,
                                                 intPrintMsg, vicPrintMsg)) != Constant.SUCCESS)
            {
                return (rc);
            }

            /* parse Passive's acode for height and width */
            GenUtil.TtPassiveAcode(acodeP, out height, out width);

            /* calc included angle */

            /* calc dist A-P, calc azim P-A */
            AxSub2.AxDistan(siteA.latit / 100.0, siteP.latit / 100.0, siteA.longit / 100.0,
                             siteP.longit / 100.0, out distAP, out azimAP, out azimPA);

            /* calc dist B-P, calc azim P-B */
            AxSub2.AxDistan(siteB.latit / 100.0, siteP.latit / 100.0, siteB.longit / 100.0,
                             siteP.longit / 100.0, out distBP, out azimBP, out azimPB);

            /* calculate the center line heights in km */
            clHtA = (siteA.grnd + htA) / 1000.0;
            clHtP = (siteP.grnd + htP) / 1000.0;
            clHtB = (siteB.grnd + htB) / 1000.0;
            /* calc elev P-A */
            AxSub3.AxElev(clHtA, clHtP, distAP, out elevAP, out elevPA);
            /* calc elev P-B */
            AxSub3.AxElev(clHtB, clHtP, distBP, out elevBP, out elevPB);

            xPA = distAP * CosD(180.0 - azimPA) * CosD(elevPA);
            yPA = distAP * CosD(azimPA - 90.0) * CosD(elevPA);
            zPA = distAP * SinD(elevPA);

            xPB = distBP * CosD(180.0 - azimPB) * CosD(elevPB);
            yPB = distBP * CosD(azimPB - 90.0) * CosD(elevPB);
            zPB = distBP * SinD(elevPB);

            Y = (xPA * xPB + yPA * yPB + zPA * zPB) / (distAP * distBP);
            iAng = AcosD(Y);
            /* end of calc's for included angle */

            /* calc discrim for passive */ /*	Constant changed to 0.1 from 0.0001
                                           *	1204 - GJS - 2005.12.16 */
            if (offAxisAng >= -0.1 && offAxisAng <= 0.1)
            {
                disc = 0.0;
            }
            else
            {
                if (offAxisAng > 180) offAxisAng -= 360;
                offAxisAng = Abs(offAxisAng);
                width = Abs(width);
                disc = 20.0 * Log10(width * CosD(0.5 * iAng)) +
                                20.0 * Log10(freq / 1000.0) +
                                20.0 * Log10(SinD(offAxisAng)) - 39.5995;
            }

            /* Changed to handle near field passive gains - 1222 - GJS - 2007.04.27 */
            if (dTGain != 0.0)
            {
                /*	We have the gain already calculated. */
                pGain = dTGain;
                rc = 0;
            }
            else
            {
                /*	We have to calculate the gain ourselves.  */
                if (FtUtils.IsCallPassive(call2))
                {
                    /*  If the next site is passive, then use the passive to passive gain */
                    rc = Ax14.PassPassGain(width,        /*	The width of the passive (m) 		*/
                                                        height,     /*	The height of the passive (m)  	*/
                                                        iAng,           /*	The included angle in degrees		*/
                                                        freq / 1000.0,  /*	Frequency in MHz					*/
                                                        distAP,     /*	Distance to the next site (km) 	*/
                                                        dAreaOrGain,    /*	The antenna area at the remote site	*/
                                                        out pGain);
                }
                else
                {
                    /*  use the normal passive gain calc. */
                    rc = Ax14.PassiveGain(width,         /*	The width of the passive (m) */
                                                     height,        /*	The height of the passive (m)  */
                                                     iAng,          /*	The included angle in degrees	*/
                                                     freq / 1000.0,     /*	Frequency in MHz		*/
                                                     distAP,        /*	Distance to the next site (km) */
                                                     dAreaOrGain,   /*	The antenna gain at the next or active
																		*		site	*/
                                                     out pGain);        /*	Output gain. */
                }
            }

            if (rc == 0)
            {
                /* disc limited to gain/2 and must be non-negative. 1204 - GJS - 2005.12 */
                disc = (disc > pGain) ? pGain : disc;
                disc = (disc < 0.0) ? 0.0 : disc;
            }
            else
            {
                /*	Problem with the passive gains.  */
                disc = 0.0;
            }

            //...Log2.v("\nTtCalcPassive.TtCalcPassive(): Exit: " + disc + "  " + pGain);
            return (rc);
        }

        /// <summary>
        /// This method searches for both ends of the passive path given the passive 
        /// antenna. ie. the transmitting site A, the passive site P, and the receiving 
        /// site B.  
        /// </summary>
        /// <param name="isInt"></param>
        /// <param name="isMDB"></param>
        /// <param name="TableName"></param>
        /// <param name="call1"></param>
        /// <param name="call2"></param>
        /// <param name="bndcde"></param>
        /// <param name="anum"></param>
        /// <param name="acodeP"></param>
        /// <param name="htA"></param>
        /// <param name="htB"></param>
        /// <param name="htP"> - Antenna height of the passive</param>
        /// <param name="siteA"></param>
        /// <param name="siteB"></param>
        /// <param name="siteP"></param>
        /// <param name="dTGain"> - The tgain from siteP to siteA</param>
        /// <param name="dAreaOrGain"> - Effective area (passive) or gain</param>
        /// <param name="intPrintMsg"></param>
        /// <param name="vicPrintMsg"></param>
        /// <returns></returns>
        public static int SitesAPB(bool isInt,
                    bool isMDB,
                    string TableName,
                    string call1,
                    string call2,
                    string bndcde,
                    short anum,
                    out string acodeP,
                    out double htA,
                    out double htB,
                     out double htP,    /*	Antenna height of the passive			*/
                    out FtSite siteA,
                    out FtSite siteB,
                    out FtSite siteP,
                    out double dTGain,     /*	The tgain from siteP to siteA			*/
                    out double dAreaOrGain,	/*	Effective area (passive) or gain	*/
                    string intPrintMsg,
                    string vicPrintMsg)
        {
            // 'out' requirements.
            acodeP = "";
            htA = Double.MaxValue;
            htB = Double.MaxValue;
            htP = Double.MaxValue;
            dTGain = Double.MaxValue;
            dAreaOrGain = Double.MaxValue;
            siteA = null;
            siteB = null;
            siteP = null;

            string select = "";
            int rc;

            FtSiteStr pSite;
            FtSiteStrNulls pSiteNulls;

            int nInd;
            int nAnt;
            string cCallB;
            string acodeA = "";

            double dAz1 = 0.0;
            double dEl1 = 0.0;
            double dAz2 = 0.0;
            double dEl2 = 0.0;

            /*	Make sure this is a passive case. */
            if (Strings.FirstCharIs(call1, '%'))
            {
                //	First get the passive site, and all its antennas.
                if ((rc = TpMdbPdfGet.TtSiteGetAnte(call1, TableName, isMDB, out pSite, out pSiteNulls)) != 0)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "Site P", call1);
                    return (rc);
                }

                // Got the site and antennas.
                siteP = pSite.stSite;

                //We need the antenna that points to A (the link fed in) to get the true
                // gain for the passive calculations.
                dTGain = 0.0;

                for (nInd = 0; nInd < pSite.nNumAnts; nInd++)
                {
                    if (call2.Equals(pSite.stAntsPtr[nInd].call2) &&
                            bndcde.Equals(pSite.stAntsPtr[nInd].bndcde.Trim()))
                    {
                        //	Found it
                        if (pSite.stAntsPtr[nInd].offazm.Equals("P") && pSite.stAntsPtr[nInd].tgain != 0.0)
                        {
                            dTGain = pSite.stAntsPtr[nInd].tgain;
                        }
                        break;
                    }
                }

                //	Now get the index of the antenna (B) in this site that points _AWAY_ from 
                //	the call2/bandcode fed in.  The index is nAnt.
                nInd = 0;
                nAnt = -1;

                rc = FtUtils.FtNextAnte(pSite, -1, null, bndcde);

                while (rc >= 0)
                {
                    if (!call2.Equals(pSite.stAntsPtr[rc].call2) &&
                          bndcde.Equals(pSite.stAntsPtr[rc].bndcde.Trim()))
                    {
                        //	Found a link pointing elsewhere, assume this is the other end.
                        nAnt = rc;
                        break;
                    }
                    rc = FtUtils.FtNextAnte(pSite, rc, null, bndcde);
                }
                if (nAnt >= 0)
                {
                    acodeP = pSite.stAntsPtr[nAnt].acode;
                    htP = pSite.stAntsPtr[nAnt].aht;
                    cCallB = pSite.stAntsPtr[nAnt].call2;
                }
                else
                {
                    string str = String.Format("Could not get away antenna for link {0}->{1}/{2}", call1, call2, bndcde);
                    Log2.e("\nTtCalcPassive.SitesAPB(): ERROR: " + str);
                    GenUtil.SetErr("SitesAPB: " + str);
                    return (rc);
                }

                /*	Get the other end of this passive link and all its antennas. (Site A) */
                if ((rc = TpMdbPdfGet.TtSiteGetAnte(call2, TableName, isMDB, out pSite, out pSiteNulls)) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "SITE A", select);
                    return (rc);
                }

                siteA = pSite.stSite;

                //	Get the link pointing back to get the antenna height.
                for (nInd = 0; nInd < pSite.nNumAnts; nInd++)
                {
                    if (pSite.stAntsPtr[nInd].call2.Equals(call1) &&
                            pSite.stAntsPtr[nInd].bndcde.Equals(bndcde))
                    {
                        //	Found it.
                        htA = pSite.stAntsPtr[nInd].aht;
                        acodeA = pSite.stAntsPtr[nInd].acode;
                        dAz1 = pSite.stAntsPtr[nInd].azmth;
                        dEl1 = pSite.stAntsPtr[nInd].elvtn;        /*	Az and El back to passive. */
                    }
                    else if (!pSite.stAntsPtr[nInd].call2.Equals(call1) &&
                                       pSite.stAntsPtr[nInd].bndcde.Equals(bndcde))
                    {
                        //	This link may not exist, but if it does...
                        dAz2 = pSite.stAntsPtr[nInd].azmth;
                        dEl2 = pSite.stAntsPtr[nInd].elvtn;    //	Pointing away from the passive.
                    }
                }

                /*	Get the other end of the bounced link. (Site B) */
                if ((rc = TpMdbPdfGet.TtSiteGetAnte(cCallB, TableName, isMDB, out pSite, out pSiteNulls)) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "SITE B", select);
                    return (rc);
                }

                siteB = pSite.stSite;

                //	Get the other end of the passive to B link to get the antenna height.
                for (nInd = 0; nInd < pSite.nNumAnts; nInd++)
                {
                    if (pSite.stAntsPtr[nInd].call2.Equals(call1) &&
                            pSite.stAntsPtr[nInd].bndcde.Equals(bndcde))
                    {
                        //	Found it.
                        htB = pSite.stAntsPtr[nInd].aht;
                        break;
                    }
                }

                if (dTGain == 0.0)
                {
                    if (FtUtils.IsBillBoard(acodeA))
                    {
                        /*	This is a passive antenna,  We need to calculate the effective
                        *		area going to the other end.  We assume that if we don't have
                        *		tgain, we don't have the normal stored.  */
                        double dAzNormal;
                        double dElNormal;
                        double dIncAngle;
                        double dHeight;
                        double dWidth;
                        int nRet;

                        /*	Calculate the normal given the two rays.  */
                        nRet = Ax14.CalcPassiveNormal(dAz1, dEl1, dAz2, dEl2, out dAzNormal, out dElNormal);

                        /*	Calculate the included angle from the first azimuth */
                        dIncAngle = Ax14.IncludedAngle(dAzNormal, dElNormal, dAz1, dEl1);

                        /*	Get the dimensions of the antenna.  */
                        GenUtil.TtPassiveAcode(acodeA, out dHeight, out dWidth);

                        /*	Now calculate the effective area  */
                        dAreaOrGain = Ax14.EffectiveArea(dWidth, dHeight, dIncAngle);
                    }
                    else
                    {
                        /*	This is an active antenna.  Just retrieve it and return its gain. */
                        SuAntStr pAnte = null;
                        if (Suutils.SuGetAnt(acodeA, out pAnte) == 0)
                        {
                            dAreaOrGain = pAnte.acAnt.again;
                        }
                        else
                        {
                            dAreaOrGain = 0.0;
                            return -2;  /*	Could not find the antenna at A */
                        }
                    }
                }

            }
            else
            {
                /*	Not a passive case.  Why are we here? */
                return -1;
            }

            return (Constant.SUCCESS);
        }






    }
}

```
