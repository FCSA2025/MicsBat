# Documented File: TeCalcs.cs
**Repository Path:** `TpRunTsip20260126\TeCalcs.cs`
**Primary Layer:** `TpRunTsip20260126`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TpRunTsip
{
    using _Configuration;
    using _Utillib;
    using _Auxlib;
    using _NewLib;
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;            //Invented to mimic (char *) for [In]  only.
    using SQLCHARPTRINOUT = IntPtr;       //Invented to mimic (char *) for [In, Out].
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLINTEGERPTR = IntPtr;
    using SQLLEN = Int64;
    using SQLLENPTR = IntPtr;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;
    using SQLSETPOSIROW = UInt64;
    using SQLSMALLINT = Int16;
    using SQLSMALLINTPTR = IntPtr;
    using SQLULEN = UInt64;
    using SQLUSMALLINT = UInt16;
    using System.Runtime.InteropServices;
    using static System.Math;
    using static _NewLib.Maths;
    using global::TpRunTsip;

    /// <summary>
    /// Provides methods that perform TSIP calculations for the ES case.
    /// </summary>
    public class TeCalcs
    {
#if PINVOKE
        [DllImport("tpruntsip.dll", CharSet = CharSet.Ansi)]
        private extern static int teAnteCalcs([In] TpParm parmStruct,
                                                [In, Out] TeAnte anteStruct,
                                                [In, Out] TeSite siteStruct,
                                                [In, Out] SQLLEN[] anteNulls,
                                                [In] string intPrintMsg,
                                                [In] string vicPrintMsg);

        [DllImport("tpruntsip.dll", CharSet = CharSet.Ansi)]
        private extern static int teChanCalcs([In, Out] TeChan chanStruct,
                                                [In, Out] TeAnte anteStruct,
                                                [In, Out] TeSite siteStruct,
                                                [In] TpParm parmStruct,
                                                [In, Out] SQLLEN[] chanNulls,
                                                [In] string intPrintMsg,
                                                [In] string vicPrintMsg);


        public static int TeChanCalcs_NATIVE(ref TeChan chanStruct,
                                 ref TeAnte anteStruct,
                                 ref TeSite siteStruct,
                                 TpParm parmStruct,
                                 ref SQLLEN[] chanNulls,
                                 string intPrintMsg,
                                 string vicPrintMsg)
        {
            return teChanCalcs(chanStruct, anteStruct, siteStruct, parmStruct, chanNulls, intPrintMsg, vicPrintMsg);
        }

        public static int TeAnteCalcs_NATIVE(TpParm parmStruct,
                    ref TeAnte anteStruct,
                    ref TeSite siteStruct,
                    ref SQLLEN[] anteNulls,
                    string intPrintMsg,
                    string vicPrintMsg)
        {
            return teAnteCalcs(parmStruct, anteStruct, siteStruct, anteNulls, intPrintMsg, vicPrintMsg);
        }
#endif
        //-------------------------------------------------------------------------------

        static string oldTrafTx = "";
        static string oldTrafRx = "";
        static string oldEqptTx = "";
        static string oldEqptRx = "";
        static int oldRc;
        static CtxStruct curCtx = new CtxStruct();
        static ScanFormatted sf = new ScanFormatted();
        //-------------------------------------------------------------------------------


        /// <summary>
        /// This method performs calculations at the site level to populate the following fields in 
        /// the ES SH Site Table: terrname2, earthoper, intreq, tudist, tuazim, utazim, 
        /// etdist, etazim, teazim, eudist, euazim, ueazim.  
        /// </summary>
        /// <param name="parmStruct"> - TpParm object providing parameter data.</param>
        /// <param name="siteStruct"> - TeSite object.</param>
        /// <param name="siteNulls"> - ODBC nullInds associated with siteStruct.</param>
        /// <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        /// <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        /// <returns></returns>
        public static int TeSiteCalcs(TpParm parmStruct,
                                ref TeSite siteStruct,
                                ref SQLLEN[] siteNulls,
                                string intPrintMsg,
                                string vicPrintMsg)
        {
            //...Log2.v("\nTeCalcs.TeSiteCalcs(): Entry");

            int rc;
            bool terrMDB, earthMDB;
            SQLLEN[] tmpSiteNulls;  // [FtSite.SIZE_ > FE_SITE_SIZE_ ? FtSite.SIZE_ : FE_SITE_SIZE_];
            int uLatit, uLongit;
            string select = "";
            string terrTabName;  // [TABLE_NM_SZ]
            string earthTabName;  // [TABLE_NM_SZ]
            string junkName;  // [TABLE_NM_SZ]
            string terrOprTyp;  // [3]
            string earthOprTyp;  // [3]
            FtSite ftTmpSite;
            FeSite feTmpSite;

            TeSubCalc.TeTableNames(parmStruct, Constant.FT_SITE, out terrTabName, out earthTabName, out junkName, out terrMDB, out earthMDB);

            if ((rc = TpMdbPdfGet.TtSiteGetCall(siteStruct.terrcall2, terrTabName, terrMDB, out ftTmpSite, out tmpSiteNulls)) !=
                   Constant.SUCCESS)
            {
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "SITE", select);
                /* can't find remote site - tell calling process to continue processing */
                return (Constant.CONT_PROCESSING);
            }

            siteStruct.terrname2 = ftTmpSite.name;
            siteNulls[TeSite.TERRNAME2] = tmpSiteNulls[FtSite.NAME];
            siteStruct.terroper2 = ftTmpSite.oper;
            siteNulls[TeSite.TERROPER2] = tmpSiteNulls[FtSite.NAME];

            uLatit = ftTmpSite.latit;
            uLongit = ftTmpSite.longit;

            if ((rc = TpMdbPdfGet.TtSiteGetCall(siteStruct.terrcall1, terrTabName, terrMDB, out ftTmpSite, out tmpSiteNulls)) != Constant.SUCCESS)
            {
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "SITE", select);
                return (rc);
            }
            terrOprTyp = ftTmpSite.oprtyp;

            select = String.Format("location = '{0}'", siteStruct.earthlocation);
            if ((rc = TpMdbPdfGet.TeSiteGetLoc(siteStruct.earthlocation, earthTabName, earthMDB, out feTmpSite, out tmpSiteNulls)) != Constant.SUCCESS)
            {
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "SITE", select);
                return (rc);
            }
            earthOprTyp = feTmpSite.oprtyp;

            if (Strings.FirstCharIs(earthOprTyp, 'F') && Strings.FirstCharIs(terrOprTyp, 'F') && !siteStruct.earthoper.Equals("TGLB"))
            {
                siteStruct.intreq = "FCSA";
            }
            else
            {
                siteStruct.intreq = "CCIR";
            }

            siteNulls[TeSite.INTREQ] = Constant.DB_NOT_NULL;

            AxSub2.AxDistan(siteStruct.terrlatit / 100.0, uLatit / 100.0,
                       siteStruct.terrlongit / 100.0, uLongit / 100.0,
                       out siteStruct.tudist, out siteStruct.tuazim,
                       out siteStruct.utazim);
            siteNulls[TeSite.TUDIST] = Constant.DB_NOT_NULL;
            siteNulls[TeSite.TUAZIM] = Constant.DB_NOT_NULL;
            siteNulls[TeSite.UTAZIM] = Constant.DB_NOT_NULL;

            AxSub2.AxDistan(siteStruct.earthlatit / 100.0, siteStruct.terrlatit / 100.0,
                       siteStruct.earthlongit / 100.0, siteStruct.terrlongit / 100.0,
                       out siteStruct.etdist, out siteStruct.etazim,
                       out siteStruct.teazim);
            siteNulls[TeSite.ETDIST] = Constant.DB_NOT_NULL;
            siteNulls[TeSite.ETAZIM] = Constant.DB_NOT_NULL;
            siteNulls[TeSite.TEAZIM] = Constant.DB_NOT_NULL;

            AxSub2.AxDistan(siteStruct.earthlatit / 100.0, uLatit / 100.0,
                       siteStruct.earthlongit / 100.0, uLongit / 100.0,
                       out siteStruct.eudist, out siteStruct.euazim,
                       out siteStruct.ueazim);
            siteNulls[TeSite.EUDIST] = Constant.DB_NOT_NULL;
            siteNulls[TeSite.EUAZIM] = Constant.DB_NOT_NULL;
            siteNulls[TeSite.UEAZIM] = Constant.DB_NOT_NULL;

            //...Log2.v("\nTeCalcs.TeSiteCalcs(): Exit");

            return (Constant.SUCCESS);
        }


        /// <summary>
        /// This method performs calculations at the antenna level to fill in the following fields in 
        /// the ES SH Ante Table: terrht, earthht, teelev, etelev, tuelev, utelev, 
        /// euelev, tdiscang, adisc_ute. It also calls the method TeAnteRainCalcs that populates 
        /// additional antenna fields.  
        /// </summary>
        /// <param name="parmStruct"> - TpParm object providing parameter data.</param>
        /// <param name="anteStruct"> - TeAnte object.</param>
        /// <param name="siteStruct"> - TeSite object.</param>
        /// <param name="anteNulls"> - ODBC nullInds associated with anteStruct.</param>
        /// <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        /// <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        /// <returns></returns>
        public static int TeAnteCalcs(TpParm parmStruct,
                    ref TeAnte anteStruct,
                    ref TeSite siteStruct,
                    ref SQLLEN[] anteNulls,
                    string intPrintMsg,
                    string vicPrintMsg)
        {
            //...Log2.v("\nTeCalcs.TeAnteCalcs(): Entry");

            int rc;
            bool earthMDB;
            bool terrMDB;
            SQLLEN[] tmpAnteNulls;  //[FtAnte.SIZE_];
            SQLLEN[] tmpSiteNulls;  //[FtSite.SIZE_];
            SQLLEN[] teAnteNulls;  //[FE_ANTE_SIZE_];
            SQLLEN nullADiscCTxV;
            SQLLEN nullADiscXTxV;
            SQLLEN nullADiscCTxH;
            SQLLEN nullADiscXTxH;

            double antHtTKm;
            double antHtEKm;
            double antHtUKm;
            double minADisc;
            double tLnkAht;
            double tLnkGrnd;
            double tmpTeElev;
            double tmpEtElev;
            double tmpTuElev;
            double tmpUtElev;
            double tmpEuElev;
            double tmpUeElev;
            double tAht;
            double eAht;
            double junk;
            double disc;
            double adiscctxv;
            double adiscxtxv;
            double adiscctxh;
            double adiscxtxh;
            double terrMbnd;
            double dtAntaz;
            double dtAntel;

            double detElev;
            double deuElev;
            double[] etVec;  //[3];    /*	Geometric E to T vector */
            double[] euVec;  //[3];    /*	Geometric E to U vector */
            double[] tuVec;  //[3];    /*	This is the geometric T to U vector, must be calculated
                             /*		here - GJS - 1108 - 2003.01 */

            string terrTabName;
            string earthTabName;  //TABLE_NM_SZ]
            string terrSiteName;  //TABLE_NM_SZ]
            string earthSiteName;  //TABLE_NM_SZ]
            string azimTabName;  //TABLE_NM_SZ]
            string anteTabName;  //TABLE_NM_SZ]
            string antdTabName;  //TABLE_NM_SZ]
            string junkName;  //TABLE_NM_SZ]
            string select = "";  //SEL_STMT_SIZE];

            FtSite ftTmpSite;
            FtAnte ftTmpAnte;
            FeAnte feTmpAnte;
            SdBand curBand;

            int terrxtype;

            AxStation es;

            SuAntStr pAnt;
            int nRet;


            TeSubCalc.TeTableNames(parmStruct, Constant.FT_SITE, out terrSiteName, out earthSiteName, out junkName, out terrMDB, out earthMDB);

            if ((rc = TpMdbPdfGet.TtSiteGetCall(siteStruct.terrcall2, terrSiteName, terrMDB, out ftTmpSite, out tmpSiteNulls)) != Constant.SUCCESS)
            {
                Log2.w("\nTeCalcs.TeAnteCalcs(): EXIT: WARNING: can't find remote site: TtSiteGetCall() returned " + rc);

                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "SITE", select);
                /* can't find remote site - tell calling process to continue processing */
                return (Constant.CONT_PROCESSING);
            }
            tLnkGrnd = ftTmpSite.grnd;

            TeSubCalc.TeTableNames(parmStruct, Constant.FT_ANTE, out terrTabName, out earthTabName, out azimTabName, out terrMDB, out earthMDB);

            if (Strings.FirstCharIs(parmStruct.protype, 'T'))
            {
                junkName = parmStruct.proname;
            }
            else
            {
                junkName = parmStruct.envname;
            }
            /* get the remote antenna's height */
            tLnkAht = GetHighestAntenna(junkName, anteStruct.terrcall1,
                                          anteStruct.terrcall2, anteStruct.terrbndcde,
                                          anteStruct.terranum);
            //...Log2.v("\nGetHighestAntenna: " + tLnkAht);
            //&&Console.Error.Write("\nTeAnteCalcs(): tLnkAht = " + tLnkAht);

            if (tLnkAht < 0.0)
            {
                /* Can't find remote antennae;
                 * not a fatal error, print message and continue w/ next ante */
                Log2.w("\nTeCalcs.TeAnteCalcs(): EXIT: WARNING: Call to GetHighestAntenna() returned " + tLnkAht);

                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR,
                               "\tWARNING: Could not find remote antenna to calculate antenna height");
                return (Constant.CONT_PROCESSING);
            }

            if ((rc = TpMdbPdfGet.TtAnteGet(anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.terrbndcde,
                                  anteStruct.terranum, terrTabName, terrMDB, out ftTmpAnte, out tmpAnteNulls)) != Constant.SUCCESS)
            {
                Log2.e("\nTeCalcs.TeAnteCalcs(): ERROR: EXIT: call to TtAnteGet() returned " + rc);
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "ANTE", select);
                return (rc);
            }
            tAht = ftTmpAnte.aht;
            //&&Console.Error.Write("\nTeAnteCalcs(): tAht = " + tAht);

            select = String.Format("location = '{0}' and call1 = '{1}'", anteStruct.earthlocation, anteStruct.earthcall1);
            if ((rc = TpMdbPdfGet.TeAnteGet(select, earthTabName, earthMDB, out feTmpAnte, out teAnteNulls)) != Constant.SUCCESS)
            {
                Log2.e("\nTeCalcs.TeAnteCalcs(): EXIT: ERROR: call to TeAnteGet() returned " + rc);
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "ANTE", select);
                return (rc);
            }
            eAht = feTmpAnte.aht;
            //&&Console.Error.Write("\nTeAnteCalcs(): eAht = " + eAht);

            /*------ ------*/
            anteStruct.terrht = siteStruct.terrgrnd + tAht;
            anteNulls[TeAnte.TERRHT] = Constant.DB_NOT_NULL;

            anteStruct.earthht = siteStruct.earthgrnd + eAht;
            anteNulls[TeAnte.EARTHHT] = Constant.DB_NOT_NULL;

            antHtTKm = anteStruct.terrht / 1000.0;
            antHtEKm = anteStruct.earthht / 1000.0;
            antHtUKm = (tLnkGrnd + tLnkAht) / 1000.0;


            AxSub3.AxElev(antHtTKm, antHtEKm, siteStruct.etdist, out tmpTeElev, out tmpEtElev);

            detElev = tmpEtElev;    /*	Save this for the geometric vectors */
            //&&Console.Error.Write("\nTeAnteCalcs(): detElev = " + detElev);

            CalcRefElev(siteStruct.etdist, tmpTeElev, antHtTKm, antHtEKm, out tmpTeElev);

            select = String.Format("location = '{0}' and call1 = '{1}'",
                          anteStruct.earthlocation, anteStruct.earthcall1);

            if ((rc = TeSubCalc.TeCheckRadioHoriz(select, earthMDB, azimTabName,
                                        siteStruct.etdist, siteStruct.teazim,
                                                                    ref tmpTeElev, intPrintMsg,
                                        vicPrintMsg)) != Constant.SUCCESS)
            {
                Log2.e("\nTeCalcs.TeAnteCalcs(): EXIT: ERROR: A: call to TeCheckRadioHoriz() returned " + rc);
                return (rc);
            }

            //&&Console.Error.Write("\nTeAnteCalcs(): anteStruct.terracode = " + anteStruct.terracode);

            anteStruct.teelev = tmpTeElev;
            anteNulls[TeAnte.TEELEV] = Constant.DB_NOT_NULL;


            CalcRefElev(siteStruct.etdist, tmpEtElev, antHtEKm, antHtTKm, out tmpEtElev);

            if ((rc = TeSubCalc.TeCheckRadioHoriz(select, earthMDB, azimTabName, siteStruct.etdist, siteStruct.etazim, ref tmpEtElev, intPrintMsg, vicPrintMsg)) != Constant.SUCCESS)
            {
                Log2.e("\nTeCalcs.TeAnteCalcs(): EXIT: ERROR: B: call to TeCheckRadioHoriz() returned " + rc);
                return (rc);
            }

            anteStruct.etelev = tmpEtElev;
            anteNulls[TeAnte.ETELEV] = Constant.DB_NOT_NULL;

            AxSub3.AxElev(antHtTKm, antHtUKm, siteStruct.tudist, out tmpTuElev, out tmpUtElev);

            CalcRefElev(siteStruct.tudist, tmpTuElev, antHtTKm, antHtUKm, out tmpTuElev);

            anteStruct.tuelev = tmpTuElev;
            anteNulls[TeAnte.TUELEV] = Constant.DB_NOT_NULL;

            CalcRefElev(siteStruct.tudist, tmpUtElev, antHtUKm, antHtTKm, out tmpUtElev);

            anteStruct.utelev = tmpUtElev;
            anteNulls[TeAnte.UTELEV] = Constant.DB_NOT_NULL;


            AxSub3.AxElev(antHtEKm, antHtUKm, siteStruct.eudist, out tmpEuElev, out tmpUeElev);

            deuElev = tmpEuElev;    /*	Save for geometric vectors */

            CalcRefElev(siteStruct.eudist, tmpEuElev, antHtEKm, antHtUKm, out tmpEuElev);

            anteStruct.euelev = tmpEuElev;
            anteNulls[TeAnte.EUELEV] = Constant.DB_NOT_NULL;

            TeVects.TeBuildVector(siteStruct.etdist, detElev, siteStruct.etazim, out etVec);
            // euVec is the vector from the es site to the u site (u is other end of the ts link.
            TeVects.TeBuildVector(siteStruct.eudist, deuElev, siteStruct.euazim, out euVec);

            // tuVec is the vector (SEZ in the es frame of reference) from the ts site to u.
            TeVects.TeVectorSub(euVec, etVec, out tuVec);

            //&&Console.Error.Write("\nteAnteCalcs(): dog");
            if ((rc = TeSubCalc.TeFillEsStnStr(siteStruct, earthMDB, anteStruct, earthTabName,
                                     out es)) != Constant.SUCCESS)
            {
                Log2.e("\nTeCalcs.TeAnteCalcs(): ERROR: call to TeFillEsStnStr() returned " + rc);
                return (rc);
            }

            //&&Console.Error.Write("\nteAnteCalcs(): cat"); Console.Error.Flush();
            if ((rc = TeSubCalc.TeCalcSET(es, ref anteStruct, siteStruct, parmStruct, ref anteNulls,
                                  Constant.REF_INDEX, intPrintMsg, vicPrintMsg)) != Constant.SUCCESS)
            {
                Log2.e("\nTeCalcs.TeAnteCalcs(): ERROR: call to TeCalcSET() returned " + rc);

                if (rc == Error.NOTEMPDATA)
                {
                    Log2.e("\nteAnteCalcs(): fox");
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    TpRunTsip.mTW_ERR.Write("Temporary Antenna code: {0}\n", anteStruct.earthacode);
                    ErrMsg.UtPrintMessage(Error.NOTEMPDATA);
                    TpRunTsip.mTW_ERR.Write("\n");
                    return (Constant.CONT_PROCESSING);
                }
                return (rc);
            }

            //&&Console.Error.Write("\nTeAnteCalcs(): anteStruct.terracode = " + anteStruct.terracode);

            /*	Calculate the ute angle.  This is used for the reports if we have an
            *		off-axis angle situation. */
            SepAng.AxSepAng(siteStruct.tuazim, anteStruct.tuelev, siteStruct.teazim,
                         anteStruct.teelev, out anteStruct.angleute);
            anteNulls[TeAnte.ANGLEUTE] = Constant.DB_NOT_NULL;

            /*	If the antenna is an offaxis antenna on the terrestrial side, use the
            *		offaxis az and el for the input parameters - GJS - 1108 - 2002.12 */
            if (Strings.FirstCharIs((anteStruct.tsoffaxis), 'Y'))
            {
                /*	Offaxis antenna */
                dtAntaz = anteStruct.tstrueaz;
                dtAntel = anteStruct.tstrueel;
            }
            else
            {
                dtAntaz = siteStruct.tuazim;
                dtAntel = anteStruct.tuelev;
            }


            SepAng.AxSepAng(dtAntaz, dtAntel, siteStruct.teazim, anteStruct.teelev, out anteStruct.tdiscang);

            anteNulls[TeAnte.TDISCANG] = Constant.DB_NOT_NULL;

            /* check if local TS site is a passive reflector:
             * if call1 begins with '%'				 */
            if (Strings.FirstCharIs(anteStruct.terrcall1, '%'))
            {
                /* get midband freq from SDB for terr site */
                if (Suutils.SdGetBand(anteStruct.terrbndcde, out curBand) != 0)
                {
                    Log2.e("\nTeCalcs.TeAnteCalcs(): ERROR: call to SdGetBand() returned " + rc);
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.INVALIDBANDCODE, anteStruct.terrbndcde);
                    return (Constant.FAILURE);
                }
                terrMbnd = curBand.bmidf;

                /* calculate terrestrial's passive reflector  discrimination */
                if (Strings.FirstCharIs(anteStruct.interferer, 'T'))
                {
                    rc = TtCalkPassive.TtCalcPassive(true, terrMDB, terrTabName,
                                       anteStruct.terrcall1, anteStruct.terrcall2,
                                                         anteStruct.terrbndcde, anteStruct.terranum,
                                                         terrMbnd, anteStruct.tdiscang, out disc, out junk,
                                         intPrintMsg, vicPrintMsg);
                }
                else
                {
                    rc = TtCalkPassive.TtCalcPassive(false, terrMDB, terrTabName,
                                         anteStruct.terrcall1, anteStruct.terrcall2,
                                                         anteStruct.terrbndcde, anteStruct.terranum,
                                                         terrMbnd, anteStruct.tdiscang, out disc, out junk,
                                         intPrintMsg, vicPrintMsg);
                }
                if (rc != Constant.SUCCESS)
                {
                    Log2.e("\nTeCalcs.TeAnteCalcs(): ERROR: call to TtCalcPassive() returned " + rc);
                    return (rc);
                }
                adiscctxv = disc;
                adiscxtxv = disc;
                adiscctxh = disc;
                adiscxtxh = disc;
                nullADiscCTxV = Constant.DB_NOT_NULL;
                nullADiscXTxV = Constant.DB_NOT_NULL;
                nullADiscCTxH = Constant.DB_NOT_NULL;
                nullADiscXTxH = Constant.DB_NOT_NULL;

            }
            else
            {
                /* TS is not a passive repeater; get antenna pattern and
                 * calculate antenna discriminations for the TS antenna
                 */
                /* get full table names for subsidiary ante and antd info */
                if ((!parmStruct.tempant.Trim().Equals("")) &&
                    (anteStruct.terracode.StartsWith("$")))
                {
                    GenUtil.UtCvtName(Constant.TP_SU_ANTD, parmStruct.tempant, out antdTabName);
                    GenUtil.UtCvtName(Constant.TP_SU_ANTE, parmStruct.tempant, out anteTabName);
                }
                else
                {
                    antdTabName = "sd_antd";
                    anteTabName = "sd_ante";
                }

                /* get the antenna model, gain and xref for this ante */
                //&&Console.Error.Write("\nTgt05");
                nRet = Suutils.SuGetAnt(anteStruct.terracode, out pAnt);
                if (nRet != 0)
                {
                    Log2.e("\nTeCalcs.TeAnteCalcs(): ERROR: A: SuGetAnt returned " + nRet);
                }

                if (pAnt.acDscPtr == null)
                {
                    Log2.e("\nTeCalcs.TeAnteCalcs(): ERROR: A: SuGetAnt returned with pAnt.acDscPtr == null");
                    Log2.e("\n" + pAnt.ToString());
                }

                if (nRet == 0)
                {
                    if (pAnt.acAnt.again != 0.0)
                    {
                        anteStruct.terragain = pAnt.acAnt.again;
                        anteNulls[TeAnte.TERRAGAIN] = Constant.DB_NOT_NULL;
                    }

                    if (pAnt.acAnt.amodel.Length > 0)
                    {
                        anteStruct.terramodel = pAnt.acAnt.amodel;
                        anteNulls[TeAnte.TERRAMODEL] = Constant.DB_NOT_NULL;
                    }

                    //if (!pAnt.acAnt.acode.Equals(anteStruct.terracode)) {
                    anteStruct.terraxref = pAnt.acAnt.axref;
                    anteNulls[TeAnte.TERRAXREF] = Constant.DB_NOT_NULL;
                    //}

                    terrxtype = pAnt.acAnt.axtype;
                    if ((terrxtype == 1 || terrxtype == 2) && (anteStruct.tdiscang < 0))
                    {
                        anteStruct.tdiscang = 360.0 + anteStruct.tdiscang;
                    }
                }

                /* get antenna pattern and calculate antenna discriminations
                 * for the terrestrial antenna
                 */
                if ((rc = TpGetDat.CalcDisc(pAnt, anteStruct.tdiscang, out adiscctxv,
                                     out adiscxtxv, out adiscctxh, out adiscxtxh, out nullADiscCTxV,
                                   out nullADiscXTxV, out nullADiscCTxH, out nullADiscXTxH,
                                                   intPrintMsg, vicPrintMsg)) != Constant.SUCCESS)
                {
                    Log2.e("\nTeCalcs.TeAnteCalcs(): ERROR: A: call to CalcDisc() returned " + rc);
                    return (rc);
                }
            }

            minADisc = Constant.MAXADISC;
            if (nullADiscCTxV != Constant.DB_NULL)
            {
                minADisc = (minADisc < adiscctxv) ? minADisc : adiscctxv;
            }
            if (nullADiscXTxV != Constant.DB_NULL)
            {
                minADisc = (minADisc < adiscxtxv) ? minADisc : adiscxtxv;
            }
            if (nullADiscCTxH != Constant.DB_NULL)
            {
                minADisc = (minADisc < adiscctxh) ? minADisc : adiscctxh;
            }
            if (nullADiscXTxH != Constant.DB_NULL)
            {
                minADisc = (minADisc < adiscxtxh) ? minADisc : adiscxtxh;
            }

            if (minADisc == Constant.MAXADISC)
            {
                anteNulls[TeAnte.ADISC_UTE] = Constant.DB_NULL;
            }
            else
            {
                anteStruct.adisc_ute = minADisc;
                anteNulls[TeAnte.ADISC_UTE] = Constant.DB_NOT_NULL;
            }

            anteTabName = "sd_ante";

            /* get the antenna model, gain and xref for this ante */
            //&&Console.Error.Write("\nTgt06");
            nRet = Suutils.SuGetAnt(anteStruct.earthacode, out pAnt);
            if (nRet == Constant.SUCCESS)
            {
                if (pAnt.acAnt.again != 0.0)
                {
                    anteStruct.earthagain = pAnt.acAnt.again;
                    anteNulls[TeAnte.EARTHAGAIN] = Constant.DB_NOT_NULL;
                }

                if (pAnt.acAnt.amodel.Length > 0)
                {
                    anteStruct.earthamodel = pAnt.acAnt.amodel;
                    anteNulls[TeAnte.EARTHAMODEL] = Constant.DB_NOT_NULL;
                }

                //	We have returned a cross reference code
                anteStruct.earthaxref = pAnt.acAnt.axref;
                anteNulls[TeAnte.EARTHAXREF] = Constant.DB_NOT_NULL;
            }

            /* Rain scatter volume calculations */
            TeSubCalc.TeAnteRainCalcs(ref anteStruct, ref anteNulls, siteStruct, tuVec);

            /*	We now should have the discrimination angle to the volume from the
            *		all the antenna directions.  If it is an off-axis antenna, we must
            *		calculate the off-axis discrimination (adisc_atv) */
            if (Strings.FirstCharIs((anteStruct.tsoffaxis), 'Y') &&
                anteNulls[TeAnte.ANGLEATV] != Constant.DB_NULL)
            {
                if ((rc = TpGetDat.CalcDisc(pAnt,
                                 anteStruct.angleatv, out adiscctxv, out adiscxtxv,
                                                     out adiscctxh, out adiscxtxh, out nullADiscCTxV,
                                 out nullADiscXTxV, out nullADiscCTxH, out nullADiscXTxH,
                                                     intPrintMsg, vicPrintMsg)) != Constant.SUCCESS)
                {
                    Log2.e("\nTeCalcs.TeAnteCalcs(): ERROR: B: call to CalcDisc() returned " + rc);
                    return (rc);
                }

                minADisc = Constant.MAXADISC;
                if (nullADiscCTxV != Constant.DB_NULL)
                {
                    minADisc = (minADisc < adiscctxv) ? minADisc : adiscctxv;
                }
                if (nullADiscXTxV != Constant.DB_NULL)
                {
                    minADisc = (minADisc < adiscxtxv) ? minADisc : adiscxtxv;
                }
                if (nullADiscCTxH != Constant.DB_NULL)
                {
                    minADisc = (minADisc < adiscctxh) ? minADisc : adiscctxh;
                }
                if (nullADiscXTxH != Constant.DB_NULL)
                {
                    minADisc = (minADisc < adiscxtxh) ? minADisc : adiscxtxh;
                }

                if (minADisc == Constant.MAXADISC)
                {
                    anteNulls[TeAnte.ADISC_ATV] = Constant.DB_NULL;
                }
                else
                {
                    anteStruct.adisc_atv = minADisc;
                    anteNulls[TeAnte.ADISC_ATV] = Constant.DB_NOT_NULL;
                }
            }

            //...Log2.v("\nTeCalcs.TeAnteCalcs(): Exit");
            return (Constant.SUCCESS);
        }


        /// <summary>
        /// This method returns the height of the highest antenna, on the remote side of
        /// a given local antenna, that the local antenna communicates with. 
        /// </summary>
        /// <remarks>
        /// All the remote antennae from the MDB 
        /// are determined along with all the remote antennae from within the PDF. The 
        /// two lists are then merged with the antennae in the PDF taking precedence 
        /// over those in the MDB. The reason for this is that the user may be adding a 
        /// new antenna that is higher than any in the MDB or may be deleting the 
        /// highest antennae in the MDB.  
        /// </remarks>
        /// <param name="pdf"> - name of the PDF.</param>
        /// <param name="call1"> - local callsign.</param>
        /// <param name="call2"> - remote callsign.</param>
        /// <param name="bndcde"> - band code.</param>
        /// <param name="anum"> - antenna number.</param>
        /// <returns></returns>
        public static double GetHighestAntenna(string pdf,
                                        string call1,
                                        string call2,
                                        string bndcde,
                                        short anum)
        {
            float dHt = -1.0f;
            float dHt1 = -1.0f;
            string cSelect;
            string pdfName;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            /*	Just get the highest antenna on this link at the other end */
            cSelect = String.Format("select max(aht) from main.mt_ante where call1='{0}' and call2='{1}' and bndcde='{2}' group by call1, call2, bndcde ",
                            call2, call1, bndcde);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSelect, cSelect.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Qutils.ExitQueue(Info.DbName, "READ");
                Log2.e("\nTeCalcs.GetHighestAntenna(): ERROR: call to SQLExecDirect() failed for:\n" + cSelect);
                Application.Exit(666);
            }

            sqlRet = ODBC.SQLFetch(hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                dHt = -1.0f;
            }
            else
            {
                long temp;
                Ssutil.DbGetFloat(hStmt, 1, "max(aht)", out dHt, out temp);
            }

            // We must close the cursor on the statement handle before reusing it.
            ODBC.SQLFreeStmt(hStmt, ODBC.SQL_CLOSE);

            /* It may also be in the pdf */
            if (!pdf.Equals("mt_ante") && pdf.Length > 0)
            {
                /* Get the name of the antenna table */
                GenUtil.UtCvtName(Constant.FT_ANTE, pdf, out pdfName);
                cSelect = String.Format("select max(aht) from {0} where call1='{1}' and call2='{2}' and bndcde='{3}' group by call1, call2, bndcde ",
                                pdfName, call2, call1, bndcde);

                sqlRet = ODBC.SQLExecDirect(hStmt, cSelect, cSelect.Length);
                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\nTeCalcs.GetHighestAntenna(): ERROR: SQLExecDirect() failed for:\n" + cSelect);
                    string str = String.Format("getHighestAntenna02: Could not select from {0} for {1} {2} {3}.",
                                    pdfName, call2, call1, bndcde);
                    Ssutil.DbGetDiagStmt(hStmt, str);
                    return -2.0;
                }

                sqlRet = ODBC.SQLFetch(hStmt);
                if (ODBC.IsOK(sqlRet))
                {
                    long temp;
                    Ssutil.DbGetFloat(hStmt, 1, "max(aht)", out dHt1, out temp);
                }
                else
                {
                    Log2.e("\nTeCalcs.GetHighestAntenna(): ERROR: SQLFetch failed.");
                    dHt1 = -1.0f;
                }
            }

            /*	Return the highest antenna */
            if (dHt1 > dHt)
            {
                dHt = dHt1;
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return (double)dHt;
        }

        /// <summary>
        /// Calculates the refracted elevation between two 
        /// antennae given the distance between the sites, the elevation angle between 
        /// the sites, and the heights of both antennae.  
        /// </summary>
        /// <param name="dist"> - distance between sites.</param>
        /// <param name="elevAng"> - elevation angle.</param>
        /// <param name="antHt1Km"> - height of 1st. antenna in Km.</param>
        /// <param name="antHt2Km"> - height of 2nd. antenna in Km.</param>
        /// <param name="tmpElev"> - calculated refracted elevation.</param>
        public static void CalcRefElev(double dist,
                                           double elevAng,
                                           double antHt1Km,
                                           double antHt2Km,
                                           out double tmpElev)
        {
            double phi;
            double rho;
            double chi;
            double sig;
            double del;
            double arf;         /* apparent earth radius factor */

            phi = AsinD((dist * CosD(elevAng)) / (Constant.ERTHRD + antHt2Km));

            rho = Log(Constant.REF_INDEX / (Constant.REF_INDEX + -7.31 * Exp(0.005577 * Constant.REF_INDEX)));

            arf = Pow((1.0 - (rho * Constant.REF_INDEX * Constant.ERTHRD * 0.000001)) /
                    (1.0 + Constant.REF_INDEX * 0.000001), -1.0);

            chi = ((rho * (arf - 1) * (Constant.ERTHRD + antHt1Km)) / (12.0 * arf)) +
                    (1.0 / 3.0) * Pow((2.0 * arf + 1.0) / (2.0 * arf), Constant.SQUARE) - 0.5;

            phi = phi * DEG_TO_RAD;

            sig = (2.0 * arf * (antHt2Km - antHt1Km)) - ((Constant.ERTHRD + antHt1Km) *
                    Pow(phi, Constant.SQUARE) * (1.0 + chi * Pow(phi, Constant.SQUARE)));

            del = 2.0 * (Constant.ERTHRD + antHt1Km) * phi * arf *
                  (1.0 + 2.0 * chi * Pow(phi, Constant.SQUARE));

            tmpElev = AtanD(sig / del);
        }

        // TOP

        /// <summary>
        /// This method performs calculations at the channel level to populate the following fields in 
        /// the ES SH Chan Table: etreport, tereport, intfreqtx, vicfreqrx, 
        /// reqd20mode1, reqd01mode1, reqd01mode2, terrmdsc, earthmdsc, terreirp, 
        /// eartheirp, scang, loss20mode1, calci20mode1, loss01mode1, calci01mode1, 
        /// loss01mode2, calci01mode2, marg20mode1, marg01mode1, marg01mode2, calctype. 
        /// In addition, for P/C calculations, the following are also populated: ctxinttraftx, 
        /// ctxvictrafrx, ctxeqpt, freqsep,.  
        /// </summary>
        /// <param name="chanStruct"> - TeChan object.</param>
        /// <param name="anteStruct"> - TeAnte object.</param>
        /// <param name="siteStruct"> - TeSite object.</param>
        /// <param name="parmStruct"> - TpParm object providing parameter data.</param>
        /// <param name="chanNulls"> - ODBC nullInds associated with chanStruct.</param>
        /// <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        /// <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        /// <returns></returns>
        public static int TeChanCalcs(ref TeChan chanStruct,
                    ref TeAnte anteStruct,
                    ref TeSite siteStruct,
                    TpParm parmStruct,
                    ref SQLLEN[] chanNulls,
                    string intPrintMsg,
                    string vicPrintMsg)
        {
            //...Log2.v("\nTeCalcs.TeChanCalcs(): Entry");
            //&&Console.Error.Write("\nTeCalcs.TeChanCalcs(): eagle: anteStruct.earthacode = " + anteStruct.earthacode);

            string select = "";
            string remTerrACode;
            string terrAnteTabName;
            string earthAnteTabName;
            string terrChanTabName;
            string earthChanTabName;
            string terrSiteTabName;
            string earthSiteTabName;
            string anteTabName;
            string junkName;

            int tmpG = 0; ;
            int tmpGain = 0;
            int rc;

            SQLLEN[] ttmpChanNulls;  //[FtChan.SIZE_];
            SQLLEN[] etmpChanNulls;  //[FE_CHAN_SIZE_];
            SQLLEN[] tmpAnteNulls;   //[FE_ANTE_SIZE_];
            SQLLEN[] ttAnteNulls;    //[FtAnte.SIZE_];
            short direction;
            SQLLEN nullRemTPowTx = 0;
            short isSDB;
            bool terrMDB;
            bool earthMDB;
            short ctxFlag;
            short tmpAnum;

            SQLLEN nullEarthAfslr;
            SQLLEN nullEarthAfslt;
            SQLLEN nullEarthPowTx;
            SQLLEN nullL01M2;
            SQLLEN nullTerrPowTx;
            SQLLEN nullEarthPowRx;

            double terrAfsl;
            double terrPowTx;
            double earthPowTx = 0.0;
            double earthPowRx;
            double remTerrPowTx;
            double earthAfslr;
            double earthAfslt;
            double patLossLink;
            double value;
            double junk;
            double remTerrAfslTx;
            double loss01mode2;
            double fsepHi;
            double fsepMid;
            double fsepLo;
            double fsepMax;
            double cdist;
            double earthAGain;
            double terrAGain;
            double intMbnd;
            double vicMbnd;
            double intEqptStab;
            double vicEqptStab;
            double remTerrAGain;

            FtChan ftTmpChan;
            FeChan feTmpChan;
            FtAnte ftTmpAnte;
            FeAnte feTmpAnte;

            char cVicType;              /* 	Either 'A' or 'D' Victim */
            char cIntType;              /*	Either 'A' or 'D' Interferor */
                                        /*	Equipments.  Only two will be used.*/

            TcTxAnalog tVicAnalog;
            TcTxDigital tVicDigital;
            TcTxAnalog tIntAnalog;
            TcTxDigital tIntDigital;

            double reqLo;
            double reqMid;
            double reqHi;

            string tempeqpt;
            char cTypeOfInt;

            SuAntStr pAnt;
            int nRet;

            Enums.Ecalctype ecalctype;

            if (Strings.FirstCharIs((chanStruct.interferer), 'T'))
            {
                direction = Constant.TS_ES;
            }
            else
            {
                direction = Constant.ES_TS;
            }

            chanStruct.etreport = Constant.FALSE;
            chanStruct.tereport = Constant.FALSE;
            chanNulls[TeChan.ETREPORT] = Constant.DB_NOT_NULL;
            chanNulls[TeChan.TEREPORT] = Constant.DB_NOT_NULL;


            TeSubCalc.TeTableNames(parmStruct, Constant.FT_SITE, out terrSiteTabName, out earthSiteTabName, out junkName, out terrMDB, out earthMDB);


            TeSubCalc.TeTableNames(parmStruct, Constant.FT_ANTE, out terrAnteTabName, out earthAnteTabName, out junkName, out terrMDB, out earthMDB);


            TeSubCalc.TeTableNames(parmStruct, Constant.FT_CHAN, out terrChanTabName, out earthChanTabName, out junkName, out terrMDB, out earthMDB);


            if (direction == Constant.TS_ES)
            {
                if ((rc = TpGetDat.TpCalcMbnds(chanStruct.terrbndcde, anteStruct.earthband,
                                      out intMbnd, out vicMbnd, intPrintMsg, vicPrintMsg)) != Constant.SUCCESS)
                {
                    return (rc);
                }
            }
            else
            {
                if ((rc = TpGetDat.TpCalcMbnds(anteStruct.earthband, chanStruct.terrbndcde,
                                      out intMbnd, out vicMbnd, intPrintMsg, vicPrintMsg)) != Constant.SUCCESS)
                {
                    return (rc);
                }
            }

            /* check if local TS site is a passive reflector:
             * if call1 begins with '%'
             */
            if (Strings.FirstCharIs(anteStruct.terrcall1, '%'))
            {

                /* calculate terrestrial's passive reflector gain */
                if (direction == Constant.TS_ES)
                {
                    rc = TtCalkPassive.TtCalcPassive(true, terrMDB, terrAnteTabName,
                                         chanStruct.terrcall1,
                                         chanStruct.terrcall2, chanStruct.terrbndcde,
                                         chanStruct.terranum, intMbnd,
                                         anteStruct.tdiscang, out junk, out terrAGain,
                                         intPrintMsg, vicPrintMsg);
                }
                else
                {
                    rc = TtCalkPassive.TtCalcPassive(false, terrMDB, terrAnteTabName,
                                         chanStruct.terrcall1,
                                         chanStruct.terrcall2, chanStruct.terrbndcde,
                                         chanStruct.terranum, vicMbnd,
                                         anteStruct.tdiscang, out junk, out terrAGain,
                                         intPrintMsg, vicPrintMsg);
                }
                if (rc != Constant.SUCCESS)
                {
                    return (rc);
                }

            }
            else
            {/* terrestrial is not a passive repeater */

                anteTabName = "sd_ante";
                isSDB = Constant.TRUE;

                rc = SuAnteGet(anteStruct.terracode, anteTabName, isSDB, out terrAGain);
                //&&Console.Error.Write("\nteCalcs.teChanCalcs(): apple");
                if (rc != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "ANTE", select);
                    return (rc);
                }
            }

            if (anteStruct.earthacode.StartsWith("CCIR"))
            {
                int nRes = sf.Parse(anteStruct.earthacode, "%4s%2d%2d");
                List<object> results = sf.Results;
                junkName = (string)results[0];

                if (direction == Constant.ES_TS)
                {
                    //&&Console.Error.Write("\nteCalcs.teChanCalcs(): bee");
                    /* positions 7&8 are the againTx */
                    tmpG = (int)results[1];
                    tmpGain = (int)results[2];
                }
                else
                {  /* TS-ES */
                    //&&Console.Error.Write("\nteCalcs.teChanCalcs(): cow");
                    /* positions 5&6 are the againRx */
                    tmpG = (int)results[2];
                    tmpGain = (int)results[1];
                }
                earthAGain = (double)tmpGain;

                //&&Console.Error.Write("\njunkName = " + junkName);
                //&&Console.Error.Write("\ntmpG = " + tmpG);
                //&&Console.Error.Write("\ntmpGain = " + tmpGain);
            }
            else
            {
                //&&Console.Error.Write("\nteCalcs.teChanCalcs(): dingo");
                anteTabName = "sd_ante";
                isSDB = Constant.TRUE;

                if ((rc = SuAnteGet(anteStruct.earthacode, anteTabName, isSDB, out earthAGain)) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "ANTE", select);
                    return (rc);
                }
            }

            //&&Console.Error.Write("\nTeCalcs.TeChanCalcs(): ferret: anteStruct.earthacode = " + anteStruct.earthacode);

            /* get local antenna's AFSL information, tx and rx */
            select = String.Format("location = '{0}' and call1 = '{1}'", anteStruct.earthlocation, anteStruct.earthcall1);

            if ((rc = TpMdbPdfGet.TeAnteGet(select, earthAnteTabName, earthMDB, out feTmpAnte, out tmpAnteNulls)) != Constant.SUCCESS)
            {
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "ANTE", select);
                return (rc);
            }


            earthAfslr = feTmpAnte.afslr;
            earthAfslt = feTmpAnte.afslt;
            nullEarthAfslr = tmpAnteNulls[FeAnte.AFSLR];
            nullEarthAfslt = tmpAnteNulls[FeAnte.AFSLT];

            if (direction == Constant.TS_ES)
            {
                chanStruct.vicrxafls = feTmpAnte.afslr;
                chanNulls[TeChan.VICRXAFLS] = Constant.DB_NOT_NULL;
            }
            else
            {
                chanStruct.inttxafls = feTmpAnte.afslt;
                chanNulls[TeChan.INTTXAFLS] = Constant.DB_NOT_NULL;
            }


            /* Get local Channel information
             * NOTE: using the first channel fetched (arbitrary) since there are no
             *       default values & for BAND calcs there is no Chan level info
             */
            select = String.Format("location = '{0}' and call1 = '{1}'", chanStruct.earthlocation, chanStruct.earthcall1);
            if ((rc = TpMdbPdfGet.TeChanGet(select, earthChanTabName, earthMDB, out feTmpChan, out etmpChanNulls)) != Constant.SUCCESS)
            {
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "CHAN", select);
                return (rc);
            }


            /* BAND (ie no Chan info) use midband freq's for tx & rx frequencies */
            if (parmStruct.analopt.Trim().Equals("BAND"))
            {
                chanStruct.intfreqtx = intMbnd;
                chanStruct.vicfreqrx = vicMbnd;
                chanStruct.remterracode = anteStruct.terracode;
                chanStruct.energy = feTmpChan.p4khz;
                chanNulls[TeChan.INTFREQTX] = Constant.DB_NOT_NULL;
                chanNulls[TeChan.VICFREQRX] = Constant.DB_NOT_NULL;
                chanNulls[TeChan.REMTERRACODE] = Constant.DB_NOT_NULL;
                chanNulls[TeChan.ENERGY] = Constant.DB_NOT_NULL;

                /* get the remote antenna model, gain and xref for this ante */
                nRet = Suutils.SuGetAnt(chanStruct.remterracode, out pAnt);

                if (nRet == 0)
                {
                    if (pAnt.acAnt.again != 0.0)
                    {
                        chanStruct.remterragain = pAnt.acAnt.again;
                        chanNulls[TeChan.REMTERRAGAIN] = Constant.DB_NOT_NULL;
                    }
                }
            }


            if ((etmpChanNulls[FeChan.MAXTXPOWER] != Constant.DB_NULL) &&
                  (etmpChanNulls[FeChan.P4KHZ] != Constant.DB_NULL))
            {
                earthPowTx = feTmpChan.maxtxpower - feTmpChan.p4khz;
                nullEarthPowTx = Constant.DB_NOT_NULL;
            }
            else
            {
                nullEarthPowTx = Constant.DB_NULL;
            }
            terrPowTx = 13.0;

            /* Get Rem.Terr.Stn Channel info */
            if (!parmStruct.analopt.Equals("BAND"))
            {

                if ((rc = TpMdbPdfGet.TtChanGet(chanStruct.terrcall2, chanStruct.terrcall1,
                                      chanStruct.terrbndcde, chanStruct.terrchid,
                                                        terrChanTabName, terrMDB, out ftTmpChan, out ttmpChanNulls)) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "CHAN", select);
                    return (rc);
                }


                if (chanStruct.terrant == 1)
                {
                    if (ttmpChanNulls[FtChan.AFSLTX1] == Constant.DB_NULL)
                    {
                        remTerrAfslTx = 0.0;
                    }
                    else
                    {
                        remTerrAfslTx = ftTmpChan.afsltx1;
                    }
                }
                else
                {
                    if (ttmpChanNulls[FtChan.AFSLTX2] == Constant.DB_NULL)
                    {
                        remTerrAfslTx = 0.0;
                    }
                    else
                    {
                        remTerrAfslTx = ftTmpChan.afsltx2;
                    }
                }

                remTerrPowTx = ftTmpChan.pwrtx;
                nullRemTPowTx = ttmpChanNulls[FtChan.PWRTX];
                if (direction == Constant.TS_ES)
                {
                    if (ttmpChanNulls[FtChan.PWRTX] != Constant.DB_NULL)
                    {
                        chanStruct.inttxpwr2 = ftTmpChan.pwrtx;
                        chanNulls[TeChan.INTTXPWR2] = Constant.DB_NOT_NULL;
                        chanStruct.inttxafls2 = remTerrAfslTx;
                        chanNulls[TeChan.INTTXAFLS2] = Constant.DB_NOT_NULL;
                    }
                }
            }

            //	Count for debugging.
            //glbTimesThrough++;

            if (direction == Constant.ES_TS)
            {
                if (siteStruct.intreq.Equals("FCSA"))
                {
                    chanStruct.reqd20mode1 = -158.0;
                    chanStruct.reqd01mode1 = -138.0;
                    chanStruct.reqd01mode2 = -138.0;
                }
                else
                {
                    chanStruct.reqd20mode1 = -151.0;
                    chanStruct.reqd01mode1 = -137.0;
                    chanStruct.reqd01mode2 = -137.0;
                }
                chanNulls[TeChan.REQD20MODE1] = Constant.DB_NOT_NULL;
                chanNulls[TeChan.REQD01MODE1] = Constant.DB_NOT_NULL;
                chanNulls[TeChan.REQD01MODE2] = Constant.DB_NOT_NULL;
            }
            else
            {

                //	If ES is receiving the interference, then the band analysis uses the
                //	i20, it01, and ip01 fields as the initial filter.  If these are absent
                //	as they can be, then a zero here means it will fail the filter and 
                //	never get to the full channel analysis.  So if these values are absent
                //	we set them to their defaults before using them. -- OEL - 2014-07-03
                //
                if (etmpChanNulls[FeChan.I20] == Constant.DB_NULL)
                {
                    feTmpChan.i20 = -160.0f;
                    etmpChanNulls[FeChan.I20] = Constant.DB_NOT_NULL;
                }
                if (etmpChanNulls[FeChan.IT01] == Constant.DB_NULL)
                {
                    feTmpChan.it01 = -153.0f;
                    etmpChanNulls[FeChan.IT01] = Constant.DB_NOT_NULL;
                }
                if (etmpChanNulls[FeChan.IP01] == Constant.DB_NULL)
                {
                    feTmpChan.ip01 = -153.0f;
                    etmpChanNulls[FeChan.IP01] = Constant.DB_NOT_NULL;
                }
                chanStruct.reqd20mode1 = feTmpChan.i20;
                //...Log2.v(String.Format("\nA: reqd20mode1 = {0}", chanStruct.reqd20mode1));
                chanStruct.reqd01mode1 = feTmpChan.it01;
                chanStruct.reqd01mode2 = feTmpChan.ip01;
                chanNulls[TeChan.REQD20MODE1] = etmpChanNulls[FeChan.I20];
                chanNulls[TeChan.REQD01MODE1] = etmpChanNulls[FeChan.IT01];
                chanNulls[TeChan.REQD01MODE2] = etmpChanNulls[FeChan.IP01];
            }

            /*--------------------*/
            /* MDSC && EIRP Calcs */

            chanStruct.terrmdsc = terrAGain - anteStruct.adisc_ute;
            chanNulls[TeChan.TERRMDSC] = Constant.DB_NOT_NULL;

            if (direction == Constant.TS_ES)
            {
                if (nullEarthAfslr != Constant.DB_NULL)
                {
                    chanStruct.earthmdsc = earthAGain - anteStruct.adisc_set - earthAfslr;
                    chanNulls[TeChan.EARTHMDSC] = Constant.DB_NOT_NULL;
                }
                else
                {
                    chanNulls[TeChan.EARTHMDSC] = Constant.DB_NULL;
                }
                chanStruct.terreirp = terrPowTx + chanStruct.terrmdsc;
                chanNulls[TeChan.TERREIRP] = Constant.DB_NOT_NULL;
            }
            else
            {
                if (nullEarthAfslt != Constant.DB_NULL)
                {
                    chanStruct.earthmdsc = earthAGain - anteStruct.adisc_set - earthAfslt;
                    chanNulls[TeChan.EARTHMDSC] = Constant.DB_NOT_NULL;
                    if (nullEarthPowTx != Constant.DB_NULL)
                    {
                        chanStruct.eartheirp = earthPowTx + chanStruct.earthmdsc;
                        chanNulls[TeChan.EARTHEIRP] = Constant.DB_NOT_NULL;
                    }
                    else
                    {
                        chanNulls[TeChan.EARTHEIRP] = Constant.DB_NULL;
                    }
                }
                else
                {
                    chanNulls[TeChan.EARTHMDSC] = Constant.DB_NULL;
                    chanNulls[TeChan.EARTHEIRP] = Constant.DB_NULL;
                }
            }

            //&&Console.Error.Write("\nTeCalcs.TeChanCalcs(): bear: anteStruct.earthacode = " + anteStruct.earthacode);
            if ((rc = TeSubCalc.TeCalcScang(parmStruct, ref siteStruct, ref anteStruct, ref chanStruct,
                                  ref chanNulls, earthAGain, terrAGain, earthAfslt,
                                                        nullEarthAfslt, earthAfslr, nullEarthAfslr, 0.0,
                                                        out loss01mode2, out nullL01M2, intPrintMsg,
                                  vicPrintMsg)) != Constant.SUCCESS)
            {
                return (rc);
            }


            if (anteStruct.mode1 == Constant.TRUE)
            {

                /* freq stored in KHz, CalcL20M1 uses MHz */
                TeSubCalc.TeCalcL20M1(siteStruct.etdist, siteStruct.radiozone,
                              chanStruct.intfreqtx / 1000.0, anteStruct.earthht,
                              anteStruct.terrht, parmStruct.spherecalc,
                              out chanStruct.loss20mode1);

                chanNulls[TeChan.LOSS20MODE1] = Constant.DB_NOT_NULL;

                if (chanNulls[TeChan.EARTHMDSC] != Constant.DB_NULL)
                {
                    if (direction == Constant.TS_ES)
                    {
                        chanStruct.calci20mode1 = chanStruct.terreirp +
                                                   chanStruct.earthmdsc -
                                                     chanStruct.loss20mode1;
                        chanNulls[TeChan.CALCI20MODE1] = Constant.DB_NOT_NULL;
                    }
                    else
                    {
                        if (chanNulls[TeChan.EARTHEIRP] != Constant.DB_NULL)
                        {
                            chanStruct.calci20mode1 = chanStruct.terrmdsc +
                                                         chanStruct.eartheirp -
                                                         chanStruct.loss20mode1;
                            chanNulls[TeChan.CALCI20MODE1] = Constant.DB_NOT_NULL;
                        }
                        else
                        {
                            chanNulls[TeChan.CALCI20MODE1] = Constant.DB_NULL;
                        }
                    }
                }
                else
                {
                    chanNulls[TeChan.CALCI20MODE1] = Constant.DB_NULL;
                }

                /* freq stored in KHz, CalcL01M1 uses MHz */
                TeSubCalc.TeCalcL01M1(siteStruct.radiozone, chanStruct.loss20mode1,
                              chanStruct.intfreqtx / 1000.0, anteStruct.etelev,
                              siteStruct.etdist, direction, out chanStruct.loss01mode1);
                chanNulls[TeChan.LOSS01MODE1] = Constant.DB_NOT_NULL;


                if (chanNulls[TeChan.EARTHMDSC] != Constant.DB_NULL)
                {
                    if (direction == Constant.TS_ES)
                    {
                        chanStruct.calci01mode1 = chanStruct.terreirp +
                                                     chanStruct.earthmdsc -
                                                     chanStruct.loss01mode1;
                        chanNulls[TeChan.CALCI01MODE1] = Constant.DB_NOT_NULL;
                    }
                    else
                    {
                        if (chanNulls[TeChan.EARTHEIRP] != Constant.DB_NULL)
                        {
                            chanStruct.calci01mode1 = chanStruct.terrmdsc +
                                                         chanStruct.eartheirp -
                                                         chanStruct.loss01mode1;
                            chanNulls[TeChan.CALCI01MODE1] = Constant.DB_NOT_NULL;
                        }
                        else
                        {
                            chanNulls[TeChan.CALCI01MODE1] = Constant.DB_NULL;
                        }
                    }
                }
                else
                {
                    chanNulls[TeChan.CALCI01MODE1] = Constant.DB_NULL;
                }
            }

            if (anteStruct.mode2 == Constant.TRUE)
            {

                chanStruct.loss01mode2 = loss01mode2;
                chanNulls[TeChan.LOSS01MODE2] = nullL01M2;

                if (chanNulls[TeChan.LOSS01MODE2] != Constant.DB_NULL)
                {
                    if (direction == Constant.TS_ES)
                    {
                        chanStruct.calci01mode2 = terrPowTx - chanStruct.loss01mode2;
                        chanNulls[TeChan.CALCI01MODE2] = Constant.DB_NOT_NULL;
                    }
                    else
                    {
                        if (nullEarthPowTx != Constant.DB_NULL)
                        {
                            chanStruct.calci01mode2 = earthPowTx - chanStruct.loss01mode2;
                            chanNulls[TeChan.CALCI01MODE2] = Constant.DB_NOT_NULL;
                        }
                        else
                        {
                            chanNulls[TeChan.CALCI01MODE2] = Constant.DB_NULL;
                        }
                    }
                }
                else
                {
                    chanNulls[TeChan.CALCI01MODE2] = Constant.DB_NULL;
                }
            }

            if (anteStruct.mode1 == Constant.TRUE)
            {
                if ((chanNulls[TeChan.CALCI20MODE1] != Constant.DB_NULL) &&
                    (chanNulls[TeChan.REQD20MODE1] != Constant.DB_NULL))
                {
                    chanStruct.marg20mode1 = chanStruct.reqd20mode1 - chanStruct.calci20mode1;
                    chanNulls[TeChan.MARG20MODE1] = Constant.DB_NOT_NULL;
                }
                else
                {
                    chanNulls[TeChan.MARG20MODE1] = Constant.DB_NULL;
                }

                if ((chanNulls[TeChan.CALCI01MODE1] != Constant.DB_NULL) &&
                    (chanNulls[TeChan.REQD01MODE1] != Constant.DB_NULL))
                {
                    chanStruct.marg01mode1 = chanStruct.reqd01mode1 - chanStruct.calci01mode1;
                    chanNulls[TeChan.MARG01MODE1] = Constant.DB_NOT_NULL;
                }
                else
                {
                    chanNulls[TeChan.MARG01MODE1] = Constant.DB_NULL;
                }
            }

            if (anteStruct.mode2 == Constant.TRUE)
            {
                if ((chanNulls[TeChan.CALCI01MODE2] != Constant.DB_NULL) &&
                    (chanNulls[TeChan.REQD01MODE2] != Constant.DB_NULL))
                {
                    chanStruct.marg01mode2 = chanStruct.reqd01mode2 - chanStruct.calci01mode2;
                    chanNulls[TeChan.MARG01MODE2] = Constant.DB_NOT_NULL;
                }
                else
                {
                    chanNulls[TeChan.MARG01MODE2] = Constant.DB_NULL;
                }
            }


            /* if anyone one of  the margins is less than the paramter.margin
             * then continue with PC calcs
             * 	ie. if mode1 and (either marg20mode1<parm.margin or
             *	    marg01mode1<parm.margin)
             *	    or mode2 and (marg01mode2<parm.margin)
             *	then continue with P/C calcs
             *	else return and go on to the next channel
             */
            if (((chanNulls[TeChan.MARG20MODE1] != Constant.DB_NULL) &&
                 (chanStruct.marg20mode1 < parmStruct.margin) &&
                     (anteStruct.mode1 == Constant.TRUE)) ||

                    ((chanNulls[TeChan.MARG01MODE1] != Constant.DB_NULL) &&
                     (chanStruct.marg01mode1 < parmStruct.margin) &&
                     (anteStruct.mode1 == Constant.TRUE)) ||

                    ((chanNulls[TeChan.MARG01MODE2] != Constant.DB_NULL) &&
                     (chanStruct.marg01mode2 < parmStruct.margin) &&
                     (anteStruct.mode2 == Constant.TRUE)))
            {
                /* BAND interference was found; continue with P/C calculations*/
            }
            else
            {
                /* no BAND interference; no P/C calculations will be done */
                return (Constant.PC_SKIP);
            }

            if (parmStruct.analopt.Equals("BAND"))
            {
                if (direction == Constant.TS_ES)
                {
                    chanStruct.tereport = Constant.TRUE;
                }
                else
                {
                    chanStruct.etreport = Constant.TRUE;
                }
                chanStruct.calctype = "-I";
                chanNulls[TeChan.CALCTYPE] = Constant.DB_NOT_NULL;
                return (Constant.PC_SKIP);
            }

            /*@@@@@@@@@@@@@  Begin Plan/Channel Calculations @@@@@@@@@@@@@@@@@@*/

            /* Get Terr.Stn Channel info needed for calcs from pdf or mdb tables */
            if ((rc = TpMdbPdfGet.TtChanGet(chanStruct.terrcall1, chanStruct.terrcall2,
                                                    chanStruct.terrbndcde, chanStruct.terrchid, terrChanTabName,
                                                    terrMDB, out ftTmpChan, out ttmpChanNulls)) != Constant.SUCCESS)
            {
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                if (rc == Constant.FAILURE)
                {
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "CHAN", select);
                }
                return (rc);
            }


            if (direction == Constant.TS_ES)
            {
                if (chanStruct.terrant == 1)
                {
                    terrAfsl = ftTmpChan.afsltx1;
                    if (ttmpChanNulls[FtChan.AFSLTX1] == Constant.DB_NULL)
                    {
                        terrAfsl = 0.0;
                    }
                    tmpAnum = ftTmpChan.antnumbtx1;
                }
                else
                {
                    terrAfsl = ftTmpChan.afsltx2;
                    if (ttmpChanNulls[FtChan.AFSLTX2] == Constant.DB_NULL)
                    {
                        terrAfsl = 0.0;
                    }
                    tmpAnum = ftTmpChan.antnumbtx2;
                }
            }
            else
            {   /* ES-TS */
                if (chanStruct.terrant == 1)
                {
                    terrAfsl = ftTmpChan.afslrx1;
                    if (ttmpChanNulls[FtChan.AFSLRX1] == Constant.DB_NULL)
                    {
                        terrAfsl = 0.0;
                    }
                    tmpAnum = ftTmpChan.antnumbrx1;
                }
                else if (chanStruct.terrant == 2)
                {
                    terrAfsl = ftTmpChan.afslrx2;
                    if (ttmpChanNulls[FtChan.AFSLRX2] == Constant.DB_NULL)
                    {
                        terrAfsl = 0.0;
                    }
                    tmpAnum = ftTmpChan.antnumbrx2;
                }
                else
                {
                    terrAfsl = ftTmpChan.afslrx3;
                    if (ttmpChanNulls[FtChan.AFSLRX3] == Constant.DB_NULL)
                    {
                        terrAfsl = 0.0;
                    }
                    tmpAnum = ftTmpChan.antnumbrx3;
                }
            }
            terrPowTx = ftTmpChan.pwrtx;
            nullTerrPowTx = ttmpChanNulls[FtChan.PWRTX];

            /*	Now get the remote end antenna to get the values for the
                    recalculation of the RX power at this end (gain and fsl)
                    GJS (1042) */
            select = String.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and ( ause = 'TX ' or ause = 'TR ')",
                              ftTmpChan.call2, ftTmpChan.call1, ftTmpChan.bndcde);
            if ((rc = TpMdbPdfGet.TtAnteGetCond(select, terrAnteTabName,
                                                            terrMDB, out ftTmpAnte, out ttAnteNulls)) != Constant.SUCCESS)
            {
                /* If we fail it is probably because there are only RX antennas
                   at the other end.  Get any antenna. */
                select = String.Format("call1 = '{0,-9}' and call2 = '{1,-9}' and bndcde = '{2,-4}' ",
                      ftTmpChan.call2, ftTmpChan.call1, ftTmpChan.bndcde);
                if ((rc = TpMdbPdfGet.TtAnteGetCond(select, terrAnteTabName,
                                        terrMDB, out ftTmpAnte, out ttAnteNulls)) != Constant.SUCCESS)
                {
                    /*  True Failure.  Message out. */
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "ANTE", select);
                    return (rc);
                }
            }


            remTerrACode = ftTmpAnte.acode;
            chanStruct.remterracode = ftTmpAnte.acode;
            chanNulls[TeChan.REMTERRACODE] = Constant.DB_NOT_NULL;

            if (chanNulls[TeChan.REMTERRACODE] != Constant.DB_NULL)
            {
                //&&Console.Error.Write("\nTgt08");
                /* get the remote antenna model, gain and xref for this ante */
                nRet = Suutils.SuGetAnt(chanStruct.remterracode, out pAnt);
                if (nRet == 0)
                {
                    if (pAnt.acAnt.again != 0.0)
                    {
                        chanStruct.remterragain = pAnt.acAnt.again;
                        chanNulls[TeChan.REMTERRAGAIN] = Constant.DB_NOT_NULL;
                    }
                }
            }

            /* check if remote TS site is a passive reflector:
             * if call2 begins with '%'	 */
            if (Strings.FirstCharIs(anteStruct.terrcall2, '%'))
            {

                /* calculate terrestrial's remote passive reflector gain */
                if (direction == Constant.TS_ES)
                {
                    rc = TtCalkPassive.TtCalcPassive(true, terrMDB, terrAnteTabName,
                                       chanStruct.terrcall2, chanStruct.terrcall1,
                                                         chanStruct.terrbndcde, 0 /* chanStruct.terranum */,
                                                         intMbnd, anteStruct.tdiscang, out junk, out remTerrAGain,
                                         intPrintMsg, vicPrintMsg);
                }
                else
                {
                    rc = TtCalkPassive.TtCalcPassive(false, terrMDB, terrAnteTabName,
                                       chanStruct.terrcall2, chanStruct.terrcall1,
                                                         chanStruct.terrbndcde, 0 /* chanStruct.terranum */,
                                                         vicMbnd, anteStruct.tdiscang, out junk, out remTerrAGain,
                                         intPrintMsg, vicPrintMsg);
                }
                if (rc != Constant.SUCCESS)
                {
                    return (rc);
                }
            }
            else
            {/* terrestrial is not a passive repeater */


                anteTabName = "sd_ante";
                isSDB = Constant.TRUE;

                if ((rc = SuAnteGet(remTerrACode, anteTabName, isSDB, out remTerrAGain)) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "ANTE", select);
                    return (rc);
                }
            }

            /* Get Earth Stn Channel info needed for calcs from pdf or mdb tables */
            select = String.Format("location = '{0}' and call1 = '{1}' and chid = '{2}'",
                            chanStruct.earthlocation, chanStruct.earthcall1,
                              chanStruct.earthchid);
            if ((rc = TpMdbPdfGet.TeChanGet(select, earthChanTabName, earthMDB, out feTmpChan,
                                out etmpChanNulls)) != Constant.SUCCESS)
            {
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "CHAN", select);
                return (rc);
            }

            /*	Check to see if the powers are null before assigning them	|OEL TSIP ES Bug 1 */
            if ((nullEarthPowTx = etmpChanNulls[FeChan.PWRTX]) == Constant.DB_NULL)
            {
                earthPowTx = 0.0;
            }
            else
            {
                earthPowTx = feTmpChan.pwrtx;
            }
            if ((nullEarthPowRx = etmpChanNulls[FeChan.PWRRX]) == Constant.DB_NULL)
            {
                earthPowRx = 0.0;
            }
            else
            {
                earthPowRx = feTmpChan.pwrrx;
            }


            if ((rc = TpGetDat.TpGetEqptStab(chanStruct.inteqpttx, chanStruct.viceqptrx,
                                    parmStruct.tempequip, out intEqptStab, out vicEqptStab,
                                                            intPrintMsg, vicPrintMsg)) != Constant.SUCCESS)
            {
                if (rc != Constant.FAILURE)
                {
                    return (rc);
                }
                ctxFlag = Constant.FALSE;
            }
            else
            {
                ctxFlag = Constant.TRUE;
            }

            fsepMid = Abs(chanStruct.intfreqtx - chanStruct.vicfreqrx);

            fsepHi = fsepMid + (intEqptStab * intMbnd + vicEqptStab * vicMbnd);

            fsepLo = fsepMid - (intEqptStab * intMbnd + vicEqptStab * vicMbnd);
            fsepLo = Max(0.0, fsepLo);  /*	If negative, assume zero */
                                        /*
                                         * TASK 466: Maximum frequency separation is now set in the
                                         * parameters by the user, and a choice between this and the
                                         * default is made by tpMaxFSep.
                                         */

            cdist = anteStruct.txpre;
            if (cdist < anteStruct.txtro) cdist = anteStruct.txtro;
            if (cdist < anteStruct.rxpre) cdist = anteStruct.rxpre;
            if (cdist < anteStruct.rxtro) cdist = anteStruct.rxtro;


            fsepMax = TpGetDat.TpMaxFSep(parmStruct.fsep, cdist, siteStruct.etdist,
                                  chanStruct.inttraftx, chanStruct.victrafrx);

            if (fsepHi >= fsepMax && fsepLo >= fsepMax)
            {
                /* not a fatal error but this case not in contention */
                return (Constant.SUCCESS);
            }

            if (TpRunTsip.GlbIsCtxCalc == 0)
            {
                if ((!oldTrafTx.Equals(chanStruct.inttraftx)) ||
                    (!oldTrafRx.Equals(chanStruct.victrafrx)) ||
                        (!oldEqptTx.Equals(chanStruct.inteqpttx)) ||
                        (!oldEqptRx.Equals(chanStruct.viceqptrx)))
                {
                    oldTrafTx = chanStruct.inttraftx;
                    oldTrafRx = chanStruct.victrafrx;
                    oldEqptTx = chanStruct.inteqpttx;
                    oldEqptRx = chanStruct.viceqptrx;


                    rc = TpGetDat.TpGetCtxInfo(chanStruct.inttraftx,
                                      chanStruct.victrafrx, chanStruct.viceqptrx,
                                                        parmStruct.tempequip, ref curCtx,
                                                        intPrintMsg, vicPrintMsg);

                    ////...Log2.v(curCtx.ToString());

                }
                else
                {
                    rc = oldRc;
                }

                oldRc = rc;
                if (rc != Constant.SUCCESS)
                {
                    if (rc == Error.DYN_MS_SQL_SERVER_ERR)
                    {
                        return (Error.DYN_MS_SQL_SERVER_ERR);
                    }
                    rc = Constant.FAILURE;
                }
            }
            else
            {
                /*	This will cause the calculation routines to be used */
                rc = Constant.FAILURE;
            }

            if (rc == Constant.SUCCESS)
            {
                /*	CTX curve found.  use it for the parameters. */
                chanStruct.ctxinttraftx = curCtx.ctxtraftx;
                chanStruct.ctxvictrafrx = curCtx.ctxtrafrx;
                chanStruct.ctxeqpt = curCtx.ctxeqpt;
                chanStruct.calctype = curCtx.calcType;
                chanNulls[TeChan.CTXINTTRAFTX] = Constant.DB_NOT_NULL;
                chanNulls[TeChan.CTXVICTRAFRX] = Constant.DB_NOT_NULL;
                chanNulls[TeChan.CTXEQPT] = Constant.DB_NOT_NULL;
                chanNulls[TeChan.CALCTYPE] = Constant.DB_NOT_NULL;

                if (chanStruct.calctype.Equals("-I"))
                {
                    ecalctype = Enums.Ecalctype.eMINUSI;
                }
                else
                {
                    ecalctype = Enums.Ecalctype.eCOVERI;
                }

                if (ctxFlag == Constant.TRUE)
                {
                    /*	If the equipments were found ... */
                    TpGetDat.TpCalcSepReqd(chanStruct.calctype, curCtx.ctxPts, fsepHi, fsepMid,
                                  fsepLo, curCtx.numPts, out chanStruct.freqsep, out value);
                    if (ecalctype == Enums.Ecalctype.eMINUSI)
                    {
                        /* convert to dbW */
                        value = value - 30.0;
                        //...Log2.v(String.Format("\nA: value = {0}", value));
                    }
                }
                else
                {
                    chanStruct.freqsep = fsepHi / 100.0;

                    if (chanStruct.freqsep / 1000.0 <= 500.0)
                    {
                        value = 95.0;
                    }
                    else
                    {
                        value = 0.0;
                    }
                }
                chanNulls[TeChan.FREQSEP] = Constant.DB_NOT_NULL;
                //...Log2.v(String.Format("\nB: value = {0}", value));

            }
            else
            {
                /*	Could not find the CTX curve, use the calculations.  First get the
                *		appropriate equipment/traffic crossreferences. */

                rc = GetCtx.GetCtxEqpt(chanStruct.victrafrx, chanStruct.viceqptrx,
                                    chanStruct.inttraftx, chanStruct.inteqpttx,
                                                out cVicType, out cIntType,
                                          out tVicAnalog, out tVicDigital, out tIntAnalog, out tIntDigital);

                ////...Log2.v("\n\n{0}  {1}", cVicType, cIntType);
                ////...Log2.v(tVicAnalog.ToString());
                ////...Log2.v(tVicDigital.ToString());
                ////...Log2.v(tIntAnalog.ToString());
                ////...Log2.v(tIntDigital.ToString());

                if (rc == 0)
                {
                    /*	We got the crossreferences, first store the crossreference codes,
                    *		then calculate the requirements */
                    GetCtx.XrefCodes(chanStruct.inttraftx, chanStruct.inteqpttx,
                              cIntType, tIntAnalog, tIntDigital,
                                        out chanStruct.ctxinttraftx, out tempeqpt);

                    GetCtx.XrefCodes(chanStruct.victrafrx, chanStruct.viceqptrx,
                              cVicType, tVicAnalog, tVicDigital,
                                        out chanStruct.ctxvictrafrx, out chanStruct.ctxeqpt);

                    chanNulls[TeChan.CTXINTTRAFTX] = Constant.DB_NOT_NULL;
                    chanNulls[TeChan.CTXVICTRAFRX] = Constant.DB_NOT_NULL;
                    chanNulls[TeChan.CTXEQPT] = Constant.DB_NOT_NULL;

                    /*	Get the three requirements for the three frequencies centered around
                    *		the nominal frequency separation (+ / - the stabilities). */
                    GetCtx.GetCtxReq(cIntType, cVicType,
                                    tVicAnalog, tVicDigital, tIntAnalog, tIntDigital,
                                        fsepHi / 1000.0, (direction == Constant.TS_ES), out cTypeOfInt, out reqHi);

                    GetCtx.GetCtxReq(cIntType, cVicType,
                                    tVicAnalog, tVicDigital, tIntAnalog, tIntDigital,
                                        fsepMid / 1000.0, (direction == Constant.TS_ES), out cTypeOfInt, out reqMid);

                    GetCtx.GetCtxReq(cIntType, cVicType,
                                    tVicAnalog, tVicDigital, tIntAnalog, tIntDigital,
                                        fsepLo / 1000.0, (direction == Constant.TS_ES), out cTypeOfInt, out reqLo);

                    //...Log2.v(String.Format("\nreqHi = {0}", reqHi));
                    //...Log2.v(String.Format("\nreqMid = {0}", reqMid));
                    //...Log2.v(String.Format("\nreqLo = {0}", reqLo));

                    /*	Choose which of these is used depending on the type of interference */
                    if (cTypeOfInt == 'I')
                    {
                        /*	Interference only.  Choose the most negative */
                        if ((reqHi <= reqMid) && (reqHi <= reqLo))
                        {
                            chanStruct.freqsep = fsepHi;
                            value = reqHi;
                        }
                        else if ((reqLo <= reqMid) && (reqLo <= reqHi))
                        {
                            chanStruct.freqsep = fsepLo;
                            value = reqLo;
                        }
                        else
                        {
                            chanStruct.freqsep = fsepMid;
                            value = reqMid;
                        }
                        /* convert to dbW */
                        value = value - 30.0;

                        chanStruct.calctype = "I";
                        ecalctype = Enums.Ecalctype.eMINUSI;
                        //...Log2.v(String.Format("\nC: value = {0}", value));
                    }
                    else
                    {
                        if ((reqHi >= reqMid) && (reqHi >= reqLo))
                        {
                            chanStruct.freqsep = fsepHi;
                            value = reqHi;
                            //...Log2.v(String.Format("\nD-1: value = {0}", value));
                        }
                        else if ((reqLo >= reqMid) && (reqLo >= reqHi))
                        {
                            chanStruct.freqsep = fsepLo;
                            value = reqLo;
                            //...Log2.v(String.Format("\nD-2: value = {0}", value));
                        }
                        else
                        {
                            chanStruct.freqsep = fsepMid;
                            value = reqMid;
                            //...Log2.v(String.Format("\nD-3: value = {0}", value));
                        }
                        chanStruct.calctype = "C";
                        ecalctype = Enums.Ecalctype.eCOVERI;
                    }

                    chanNulls[TeChan.CALCTYPE] = Constant.DB_NOT_NULL;
                    chanNulls[TeChan.FREQSEP] = Constant.DB_NOT_NULL;
                }
                else
                {
                    /*	Could not get the equipment */
                    TpRunTsip.mTW_ERR.Write("\nCould not retrieve traffic/equipment cross reference ({0}):-\n  %s into %s\n%s/%s into %s/%s\n",
                                     rc, intPrintMsg, vicPrintMsg,
                                 chanStruct.inttraftx, chanStruct.inteqpttx,
                                     chanStruct.victrafrx, chanStruct.viceqptrx);
                    return (Constant.SUCCESS);
                }
            }


            /*----------------------------------------*/
            /* have all the data - start Calculations */

            /*--------------------*/
            /* MDSC && EIRP Calcs */
            chanStruct.terrmdsc = terrAGain - anteStruct.adisc_ute - terrAfsl;
            chanNulls[TeChan.TERRMDSC] = Constant.DB_NOT_NULL;

            if (direction == Constant.TS_ES)
            {
                if (nullTerrPowTx != Constant.DB_NULL)
                {
                    chanStruct.terreirp = (terrPowTx - 30.0) + chanStruct.terrmdsc;
                    chanNulls[TeChan.TERREIRP] = Constant.DB_NOT_NULL;
                }
                else
                {
                    chanNulls[TeChan.TERREIRP] = Constant.DB_NULL;
                }
                chanNulls[TeChan.EARTHEIRP] = Constant.DB_NULL;
            }
            else
            {
                if ((chanNulls[TeChan.EARTHMDSC] != Constant.DB_NULL) &&
                    (nullEarthPowTx != Constant.DB_NULL))
                {
                    chanStruct.eartheirp = earthPowTx + chanStruct.earthmdsc;
                    chanNulls[TeChan.EARTHEIRP] = Constant.DB_NOT_NULL;
                }
                else
                {
                    chanNulls[TeChan.EARTHEIRP] = Constant.DB_NULL;
                }
                chanNulls[TeChan.TERREIRP] = Constant.DB_NULL;
            }

            //&&Console.Error.Write("\nTeCalcs.TeChanCalcs(): crab: anteStruct.earthacode = " + anteStruct.earthacode);
            if ((rc = TeSubCalc.TeCalcScang(parmStruct, ref siteStruct, ref anteStruct, ref chanStruct,
                                                    ref chanNulls, earthAGain, terrAGain, earthAfslt,
                                                    nullEarthAfslt, earthAfslr, nullEarthAfslr,
                                                    terrAfsl, out loss01mode2, out nullL01M2,
                                                    intPrintMsg, vicPrintMsg)) != Constant.SUCCESS)
            {
                return (rc);
            }


            GenUtil.FreeSpacePathLoss(siteStruct.tudist, chanStruct.intfreqtx / 1000.0, out patLossLink);

            if (anteStruct.mode1 == Constant.TRUE)
            {
                /* freq's stored in KHz, CalcL20M1 uses MHz */
                TeSubCalc.TeCalcL20M1(siteStruct.etdist, siteStruct.radiozone,
                            chanStruct.intfreqtx / 1000.0, anteStruct.earthht,
                              anteStruct.terrht, parmStruct.spherecalc,
                              out chanStruct.loss20mode1);

                chanNulls[TeChan.LOSS20MODE1] = Constant.DB_NOT_NULL;

                /* freq's stored in KHz, CalcL01M1 uses MHz */
                TeSubCalc.TeCalcL01M1(siteStruct.radiozone, chanStruct.loss20mode1,
                              chanStruct.intfreqtx / 1000.0, anteStruct.etelev,
                              siteStruct.etdist, direction, out chanStruct.loss01mode1);

                chanNulls[TeChan.LOSS01MODE1] = Constant.DB_NOT_NULL;

                if (chanNulls[TeChan.EARTHMDSC] != Constant.DB_NULL)
                {
                    if (direction == Constant.TS_ES)
                    {
                        if ((nullTerrPowTx != Constant.DB_NULL) &&
                            (chanNulls[TeChan.TERREIRP] != Constant.DB_NULL))
                        {
                            chanStruct.calci20mode1 = chanStruct.terreirp +
                                                       chanStruct.earthmdsc -
                                                         chanStruct.loss20mode1;
                            chanStruct.calci01mode1 = chanStruct.terreirp +
                                                         chanStruct.earthmdsc -
                                                         chanStruct.loss01mode1;
                            if (ecalctype == Enums.Ecalctype.eCOVERI)
                            {
                                if (nullEarthPowRx != Constant.DB_NULL)
                                {
                                    chanStruct.calci20mode1 = earthPowRx -
                                                               chanStruct.calci20mode1;

                                    chanStruct.calci01mode1 = earthPowRx -
                                                               chanStruct.calci01mode1;
                                    chanNulls[TeChan.CALCI20MODE1] = Constant.DB_NOT_NULL;
                                    chanNulls[TeChan.CALCI01MODE1] = Constant.DB_NOT_NULL;
                                }
                                else
                                {
                                    chanNulls[TeChan.CALCI20MODE1] = Constant.DB_NULL;
                                    chanNulls[TeChan.CALCI01MODE1] = Constant.DB_NULL;
                                }
                            }
                            else
                            {
                                chanNulls[TeChan.CALCI20MODE1] = Constant.DB_NOT_NULL;
                                chanNulls[TeChan.CALCI01MODE1] = Constant.DB_NOT_NULL;
                            }
                        }
                        else
                        {
                            chanNulls[TeChan.CALCI20MODE1] = Constant.DB_NULL;
                            chanNulls[TeChan.CALCI01MODE1] = Constant.DB_NULL;
                        }
                    }
                    else
                    {
                        if ((chanNulls[TeChan.EARTHEIRP] != Constant.DB_NULL) &&
                              (chanNulls[TeChan.TERRMDSC] != Constant.DB_NULL))
                        {
                            chanStruct.calci20mode1 = chanStruct.terrmdsc +
                                                         chanStruct.eartheirp -
                                                         chanStruct.loss20mode1;
                            chanStruct.calci01mode1 = chanStruct.terrmdsc +
                                                         chanStruct.eartheirp -
                                                         chanStruct.loss01mode1;

                            if (ecalctype == Enums.Ecalctype.eCOVERI)
                            {
                                if (nullRemTPowTx != Constant.DB_NULL)
                                {
                                    chanStruct.calci20mode1 = chanStruct.vicpwrrx - 30.0 - chanStruct.calci20mode1;
                                    chanStruct.calci01mode1 = chanStruct.vicpwrrx - 30.0 - chanStruct.calci01mode1;
                                    chanNulls[TeChan.CALCI20MODE1] = Constant.DB_NOT_NULL;
                                    chanNulls[TeChan.CALCI01MODE1] = Constant.DB_NOT_NULL;
                                }
                                else
                                {
                                    chanNulls[TeChan.CALCI20MODE1] = Constant.DB_NULL;
                                    chanNulls[TeChan.CALCI01MODE1] = Constant.DB_NULL;
                                }
                            }
                            else
                            {
                                chanNulls[TeChan.CALCI20MODE1] = Constant.DB_NOT_NULL;
                                chanNulls[TeChan.CALCI01MODE1] = Constant.DB_NOT_NULL;
                            }
                        }
                        else
                        {
                            chanNulls[TeChan.CALCI20MODE1] = Constant.DB_NULL;
                            chanNulls[TeChan.CALCI01MODE1] = Constant.DB_NULL;
                        }
                    }
                }
                else
                {
                    chanNulls[TeChan.CALCI20MODE1] = Constant.DB_NULL;
                    chanNulls[TeChan.CALCI01MODE1] = Constant.DB_NULL;
                }


                if (ecalctype == Enums.Ecalctype.eCOVERI)
                {
                    if (siteStruct.intreq.Equals("FCSA"))
                    {
                        if (direction == Constant.TS_ES)
                        {
                            chanStruct.reqd20mode1 = value - 17.47;
                            chanStruct.reqd01mode1 = value - 35.54;
                        }
                        else
                        {
                            chanStruct.reqd20mode1 = value - 13.49;
                            chanStruct.reqd01mode1 = value - 33.49;
                        }
                    }
                    else
                    {
                        if (direction == Constant.TS_ES)
                        {
                            chanStruct.reqd20mode1 = value - 21.73;
                            chanStruct.reqd01mode1 = value - 40.50;
                        }
                        else
                        {
                            chanStruct.reqd20mode1 = value - 23.49;
                            chanStruct.reqd01mode1 = value - 37.49;
                        }
                    }
                }
                else
                {
                    chanStruct.reqd20mode1 = value;
                    chanStruct.reqd01mode1 = value;
                }
                chanNulls[TeChan.REQD20MODE1] = Constant.DB_NOT_NULL;
                chanNulls[TeChan.REQD01MODE1] = Constant.DB_NOT_NULL;

                if (chanNulls[TeChan.CALCI20MODE1] != Constant.DB_NULL)
                {
                    if (ecalctype == Enums.Ecalctype.eCOVERI)
                    {
                        chanStruct.marg20mode1 = chanStruct.calci20mode1 -
                                                    chanStruct.reqd20mode1;
                        chanNulls[TeChan.MARG20MODE1] = Constant.DB_NOT_NULL;
                        if (chanStruct.marg20mode1 < parmStruct.margin)
                        {
                            if (direction == Constant.TS_ES)
                            {
                                chanStruct.tereport = Constant.TRUE;
                            }
                            else
                            {
                                chanStruct.etreport = Constant.TRUE;
                            }
                        }
                    }
                    else
                    {
                        chanStruct.marg20mode1 = chanStruct.reqd20mode1 -
                                                    chanStruct.calci20mode1;
                        chanNulls[TeChan.MARG20MODE1] = Constant.DB_NOT_NULL;
                        if (chanStruct.marg20mode1 < parmStruct.margin)
                        {
                            if (direction == Constant.TS_ES)
                            {
                                chanStruct.tereport = Constant.TRUE;
                            }
                            else
                            {
                                chanStruct.etreport = Constant.TRUE;
                            }
                        }
                    }
                }
                else
                {
                    chanNulls[TeChan.MARG20MODE1] = Constant.DB_NULL;
                }

                if (chanNulls[TeChan.CALCI01MODE1] != Constant.DB_NULL)
                {
                    if (ecalctype == Enums.Ecalctype.eCOVERI)
                    {
                        chanStruct.marg01mode1 = chanStruct.calci01mode1 -
                                                    chanStruct.reqd01mode1;
                        chanNulls[TeChan.MARG01MODE1] = Constant.DB_NOT_NULL;
                        if (chanStruct.marg01mode1 < parmStruct.margin)
                        {
                            if (direction == Constant.TS_ES)
                            {
                                chanStruct.tereport = Constant.TRUE;
                            }
                            else
                            {
                                chanStruct.etreport = Constant.TRUE;
                            }
                        }
                    }
                    else
                    {
                        chanStruct.marg01mode1 = chanStruct.reqd01mode1 -
                                                    chanStruct.calci01mode1;
                        chanNulls[TeChan.MARG01MODE1] = Constant.DB_NOT_NULL;
                        if (chanStruct.marg01mode1 < parmStruct.margin)
                        {
                            if (direction == Constant.TS_ES)
                            {
                                chanStruct.tereport = Constant.TRUE;
                            }
                            else
                            {
                                chanStruct.etreport = Constant.TRUE;
                            }
                        }
                    }
                }
                else
                {
                    chanNulls[TeChan.MARG01MODE1] = Constant.DB_NULL;
                }
            }


            if (anteStruct.mode2 == Constant.TRUE)
            {
                chanStruct.loss01mode2 = loss01mode2;
                chanNulls[TeChan.LOSS01MODE2] = nullL01M2;

                if (chanNulls[TeChan.LOSS01MODE2] != Constant.DB_NULL)
                {
                    if (direction == Constant.TS_ES)
                    {
                        if (nullTerrPowTx != Constant.DB_NULL)
                        {
                            chanStruct.calci01mode2 = (terrPowTx - 30.0) - chanStruct.loss01mode2;
                            if (ecalctype == Enums.Ecalctype.eCOVERI)
                            {
                                if (nullEarthPowRx != Constant.DB_NULL)
                                {
                                    chanStruct.calci01mode2 = earthPowRx - chanStruct.calci01mode2;
                                    chanNulls[TeChan.CALCI01MODE2] = Constant.DB_NOT_NULL;
                                }
                                else
                                {
                                    chanNulls[TeChan.CALCI01MODE2] = Constant.DB_NULL;
                                }
                            }
                            else
                            {
                                chanNulls[TeChan.CALCI01MODE2] = Constant.DB_NOT_NULL;
                            }
                        }
                        else
                        {
                            chanNulls[TeChan.CALCI01MODE2] = Constant.DB_NULL;
                        }
                    }
                    else
                    {
                        if (nullEarthPowTx != Constant.DB_NULL)
                        {
                            chanStruct.calci01mode2 = earthPowTx - chanStruct.loss01mode2;
                            if (ecalctype == Enums.Ecalctype.eCOVERI)
                            {
                                if (nullRemTPowTx != Constant.DB_NULL)
                                {
                                    chanStruct.calci01mode2 = chanStruct.vicpwrrx - chanStruct.calci01mode2; // GJS - 150723A - 2015-12-23
                                    chanStruct.calci01mode2 += Constant.DBM_TO_DBW_CONVERSION;                          // AH  - 160627A - 2016-06-27
                                    chanNulls[TeChan.CALCI01MODE2] = Constant.DB_NOT_NULL;
                                }
                                else
                                {
                                    chanNulls[TeChan.CALCI01MODE2] = Constant.DB_NULL;
                                }
                            }
                            else
                            {
                                chanNulls[TeChan.CALCI01MODE2] = Constant.DB_NOT_NULL;
                            }
                        }
                        else
                        {
                            chanNulls[TeChan.CALCI01MODE2] = Constant.DB_NULL;
                        }
                    }
                }
                else
                {
                    chanNulls[TeChan.CALCI01MODE2] = Constant.DB_NULL;
                }

                if (ecalctype == Enums.Ecalctype.eCOVERI)
                {
                    if (siteStruct.intreq.Equals("FCSA"))
                    {
                        if (direction == Constant.TS_ES)
                        {
                            chanStruct.reqd01mode2 = value - 35.54;
                        }
                        else
                        {
                            chanStruct.reqd01mode2 = value - 33.49;
                        }
                    }
                    else
                    {
                        if (direction == Constant.TS_ES)
                        {
                            chanStruct.reqd01mode2 = value - 40.50;
                        }
                        else
                        {
                            chanStruct.reqd01mode2 = value - 37.49;
                        }
                    }
                }
                else
                {
                    chanStruct.reqd01mode2 = value;
                }
                chanNulls[TeChan.REQD01MODE2] = Constant.DB_NOT_NULL;

                if (chanNulls[TeChan.CALCI01MODE2] != Constant.DB_NULL)
                {
                    if (ecalctype == Enums.Ecalctype.eCOVERI)
                    {
                        chanStruct.marg01mode2 = chanStruct.calci01mode2 - chanStruct.reqd01mode2;
                        chanNulls[TeChan.MARG01MODE2] = Constant.DB_NOT_NULL;
                        if (chanStruct.marg01mode2 < parmStruct.margin)
                        {
                            if (direction == Constant.TS_ES)
                            {
                                chanStruct.tereport = Constant.TRUE;
                            }
                            else
                            {
                                chanStruct.etreport = Constant.TRUE;
                            }
                        }
                    }
                    else
                    {
                        chanStruct.marg01mode2 = chanStruct.reqd01mode2 - chanStruct.calci01mode2;
                        chanNulls[TeChan.MARG01MODE2] = Constant.DB_NOT_NULL;
                        if (chanStruct.marg01mode2 < parmStruct.margin)
                        {
                            if (direction == Constant.TS_ES)
                            {
                                chanStruct.tereport = Constant.TRUE;
                            }
                            else
                            {
                                chanStruct.etreport = Constant.TRUE;
                            }
                        }
                    }
                }
                else
                {
                    chanNulls[TeChan.MARG01MODE2] = Constant.DB_NULL;
                }
            }



            //...Log2.v("\nTeCalcs.TeChanCalcs(): Exit");
            return (Constant.SUCCESS);
        }

        // BOTTOM

        /// <summary>
        /// This method retrieves the antenna gain from the prescribed 
        /// antenna table in the DB, i.a.w. with the given selection criteria.  
        /// </summary>
        /// <param name="acode"> - antenna code.</param>
        /// <param name="tabName"> - name of antenna table.</param>
        /// <param name="isSDB"> - indicates whether the tables are in the secondary DB table set, or not.</param>
        /// <param name="suAgain"> - the retrieved antenna gain.</param>
        /// <returns></returns>
        public static int SuAnteGet(string acode,
                         string tabName,
                         short isSDB,
                         out double suAgain)
        {
            // 'out' requirement.
            suAgain = 0.0;

            SuAntStr pAnt;
            int nRet;

            //&&Console.Error.Write("\nTgt09");
            nRet = Suutils.SuGetAnt(acode, out pAnt);

            if (nRet != 0)
            {
                return (Constant.FAILURE);
            }

            suAgain = pAnt.acAnt.again;

            return (Constant.SUCCESS);
        }










    }
}

```
