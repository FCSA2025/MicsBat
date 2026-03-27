using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using static System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _NewLib.Maths;

namespace TpRunTsip
{
    using _Auxlib;
    using System.Runtime.InteropServices;
    using SQLLEN = Int64;
    public class TeSubCalc
    {
#if PINVOKE
        [DllImport("tpruntsip.dll", CharSet = CharSet.Ansi)]
        private extern static int teCalcScang([In] TpParm parmStruct,
                                                [In, Out] TeSite siteStruct,
                                                [In, Out] TeAnte anteStruct,
                                                [In, Out] TeChan chanStruct,
                                                [In, Out] SQLLEN[] chanNulls,
                                                [In] double earthAGain,
                                                [In] double terrAGain,
                                                [In] double earthAfslt,
                                                [In] SQLLEN nullEarthAfslt,
                                                [In] double earthAfslr,
                                                [In] SQLLEN nullEarthAfslr,
                                                [In] double terrAfsl,
                                                [In, Out] ref double loss01mode2,
                                                [In, Out] ref SQLLEN nullL01M2,
                                                [In] string intPrintMsg,
                                                [In] string vicPrintMsg);
        public static int TeCalcScang_NATIVE(TpParm parmStruct,
                                ref TeSite siteStruct,
                                ref TeAnte anteStruct,
                                ref TeChan chanStruct,
                                ref SQLLEN[] chanNulls,
                                double earthAGain,
                                double terrAGain,
                                double earthAfslt,
                                SQLLEN nullEarthAfslt,
                                double earthAfslr,
                                SQLLEN nullEarthAfslr,
                                double terrAfsl,
                                out double loss01mode2,
                                out SQLLEN nullL01M2,
                                string intPrintMsg,
                                string vicPrintMsg)
        {
            // 'out' requirements.
            loss01mode2 = 0.0;
            nullL01M2 = Constant.DB_NULL;

            int retVal;

            retVal = teCalcScang(parmStruct, siteStruct, anteStruct, chanStruct, chanNulls,
                                    earthAGain, terrAGain, earthAfslt, nullEarthAfslt,
                                    earthAfslr, nullEarthAfslr, terrAfsl,
                                    ref loss01mode2, ref nullL01M2,
                                    intPrintMsg, vicPrintMsg);



            return retVal;
        }
#endif
        // Switched when the message about the beam being below 0 is printed.
        public static bool IsLowElevMessToPrint = true;
        //Same thing, western end.
        public static bool IsDroppedBelowToPrint = true;

        private static ScanFormatted sf = new ScanFormatted();

        //----------------------------------------------------------------------
        /// <summary>
        /// This method generates the Terrestrial and Earth station table names based 
        /// on the users input and tableType (ie. user entered ENV file is MDB_TS then 
        /// the TS site table name is 'mt_site').  
        /// </summary>
        /// <param name="parmStruct"></param>
        /// <param name="tabType"></param>
        /// <param name="terrTableName"></param>
        /// <param name="earthTableName"></param>
        /// <param name="azimTableName"></param>
        /// <param name="terrMDB"></param>
        /// <param name="earthMDB"></param>
        public static void TeTableNames(TpParm parmStruct,
                                            int tabType,
                                            out string terrTableName,
                                            out string earthTableName,
                                            out string azimTableName,
                                            out bool terrMDB,
                                            out bool earthMDB)
        {
            //...Log2.v("\nTeSubCalc.TeTableNames(): Entry");

            // 'out' requirement.
            terrTableName = "";
            earthTableName = "";
            azimTableName = "";
            terrMDB = false;
            earthMDB = false;

            int esType;

            parmStruct.envtype.Trim();
            if (tabType == Constant.FT_SITE)
            {
                esType = Constant.FE_SITE;
            }
            else if (tabType == Constant.FT_ANTE)
            {
                esType = Constant.FE_ANTE;
            }
            else
            {
                esType = Constant.FE_CHAN;
            }

            if (Strings.FirstCharIs(parmStruct.protype, 'T'))
            {
                GenUtil.UtCvtName(tabType, parmStruct.proname, out terrTableName);
                if (parmStruct.envtype.Equals("PDF_ES"))
                {
                    GenUtil.UtCvtName(esType, parmStruct.envname, out earthTableName);
                    GenUtil.UtCvtName(Constant.FE_AZIM, parmStruct.envname, out azimTableName);
                }
                else
                {   /* envtype = "MDB_ES" */
                    earthMDB = true;
                    azimTableName = "me_azim";
                    if (esType == Constant.FE_SITE)
                    {
                        earthTableName = "me_site";
                    }
                    else if (esType == Constant.FE_ANTE)
                    {
                        earthTableName = "me_ante";
                    }
                    else
                    {
                        earthTableName = "me_chan";
                    }
                }
            }
            else
            {   /* protype == 'E' */
                GenUtil.UtCvtName(esType, parmStruct.proname, out earthTableName);
                GenUtil.UtCvtName(Constant.FE_AZIM, parmStruct.proname, out azimTableName);
                if (parmStruct.envtype.Equals("PDF_TS"))
                {
                    GenUtil.UtCvtName(tabType, parmStruct.envname, out terrTableName);
                }
                else
                {   /* envtype = "MDB_TS" */
                    terrMDB = true;
                    if (tabType == Constant.FT_SITE)
                    {
                        terrTableName = "mt_site";
                    }
                    else if (tabType == Constant.FT_ANTE)
                    {
                        terrTableName = "mt_ante";
                    }
                    else
                    {
                        terrTableName = "mt_chan";
                    }
                }
            }

            string str = String.Format("{0}  {1}  {2}  {3}  {4}", terrTableName, earthTableName, azimTableName, terrMDB, earthMDB);

            //...Log2.v("\nTeSubCalc.TeTableNames(): Exit: " + str);
        }

        /// <summary>
        /// This method checks the horizon about an antenna and stores the azimuth 
        /// elevation and distance to the point on that horizon where the azimuth is at 
        /// its minimum point.  
        /// </summary>
        /// <param name="select"></param>
        /// <param name="isMDB"></param>
        /// <param name="azimTabName"></param>
        /// <param name="dist"></param>
        /// <param name="azim"></param>
        /// <param name="elev"></param>
        /// <param name="intPrintMsg"></param>
        /// <param name="vicPrintMsg"></param>
        /// <returns></returns>
        public static int TeCheckRadioHoriz(string select,
                          bool isMDB,
                          string azimTabName,
                          double dist,
                          double azim,
                          ref double elev,
                          string intPrintMsg,
                          string vicPrintMsg)
        {
            int tmpAzimHandle, rc;
            SQLLEN[] tmpmeAzimNulls;  //[ME_AZIM_SIZE_];
            SQLLEN[] tmpfeAzimNulls;  //[FE_AZIM_SIZE_];

            string tempSel;
            float savAzim, savElev, savDist, tableElev, tableDist;
            FeAzim feTmpAzim;
            MeAzim meTmpAzim;


            if (isMDB == true)
            {
                if ((tmpAzimHandle = DynMeAzim.MeSelectAzim(select, "azim")) < 0)
                {
                    return (tmpAzimHandle);
                }
                if ((rc = DynMeAzim.MeFetchAzim(tmpAzimHandle, out meTmpAzim, out tmpmeAzimNulls)) == Constant.SUCCESS)
                {
                    if (meTmpAzim.azim > azim)
                    {
                        /*	Don't print the error message. Just set the default value. 
                        ErrMsg.UtPrintMessage(GENERROR, intPrintMsg);
                        ErrMsg.UtPrintMessage(GENERROR, vicPrintMsg);
                        ErrMsg.UtPrintMessage(AZIMDEFAULT, azimTabName, select);
                        */
                        tableElev = 0.0f;
                        tableDist = 1.0f;
                    }
                    else if (meTmpAzim.azim == azim)
                    {
                        tableElev = (float)meTmpAzim.elev;
                        tableDist = (float)meTmpAzim.dist;
                    }
                    else
                    {
                        savAzim = (float)meTmpAzim.azim;
                        savElev = (float)meTmpAzim.elev;
                        savDist = (float)meTmpAzim.dist;

                        while ((rc = DynMeAzim.MeFetchAzim(tmpAzimHandle, out meTmpAzim, out tmpmeAzimNulls)) == Constant.SUCCESS)
                        {
                            if (meTmpAzim.azim < azim)
                            {
                                savAzim = (float)meTmpAzim.azim;
                                savElev = (float)meTmpAzim.elev;
                                savDist = (float)meTmpAzim.dist;
                            }
                            else
                            {
                                break;
                            }
                        }

                        GenUtil.Interp((float)azim, savAzim, (float)meTmpAzim.azim, savDist, (float)meTmpAzim.dist, out tableDist);

                        GenUtil.Interp((float)azim, savAzim, (float)meTmpAzim.azim, savElev, (float)meTmpAzim.elev, out tableElev);
                    }
                }
                else if (rc == Constant.NOMORERECS)
                {
                    /*	Don't print the error message, just use default value 
                    ErrMsg.UtPrintMessage(GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(AZIMDEFAULT, azimTabName, select);
                    */
                    tableElev = 0.0f;
                    tableDist = 1.0f;
                }
                else
                {
                    return (rc);
                }

                DynMeAzim.MeCloseAzim(tmpAzimHandle);
            }
            else
            {
                tempSel = String.Format("{0} and cmd != 'D'", select);

                if ((tmpAzimHandle = DynFeAzim.FeSelectAzim(azimTabName, tempSel, "azim")) < 0)
                {
                    return (tmpAzimHandle);
                }

                if ((rc = DynFeAzim.FeFetchAzim(tmpAzimHandle, out feTmpAzim, out tmpfeAzimNulls)) == Constant.SUCCESS)
                {
                    if (feTmpAzim.azim > azim)
                    {
                        /*	Don't print the error message.
                        ErrMsg.UtPrintMessage(GENERROR, intPrintMsg);
                        ErrMsg.UtPrintMessage(GENERROR, vicPrintMsg);
                        ErrMsg.UtPrintMessage(AZIMDEFAULT,azimTabName,tempSel);
                        */
                        tableElev = 0.0f;
                        tableDist = 1.0f;
                    }
                    else if (feTmpAzim.azim == azim)
                    {
                        tableElev = (float)feTmpAzim.elev;
                        tableDist = (float)feTmpAzim.dist;
                    }
                    else
                    {
                        savAzim = (float)feTmpAzim.azim;
                        savElev = (float)feTmpAzim.elev;
                        savDist = (float)feTmpAzim.dist;

                        while ((rc = DynFeAzim.FeFetchAzim(tmpAzimHandle, out feTmpAzim, out tmpfeAzimNulls)) == Constant.SUCCESS)
                        {
                            if (feTmpAzim.azim < azim)
                            {
                                savAzim = (float)feTmpAzim.azim;
                                savElev = (float)feTmpAzim.elev;
                                savDist = (float)feTmpAzim.dist;
                            }
                            else
                            {
                                break;
                            }
                        }

                        GenUtil.Interp((float)azim, savAzim, (float)feTmpAzim.azim, savDist, (float)feTmpAzim.dist, out tableDist);

                        GenUtil.Interp((float)azim, savAzim, (float)feTmpAzim.azim, savElev, (float)feTmpAzim.elev, out tableElev);
                    }
                }
                else if (rc == Constant.NOMORERECS)
                {
                    tableElev = 0.0f;
                    tableDist = 1.0f;
                }
                else
                {
                    return (rc);
                }

                DynFeAzim.FeCloseAzim(tmpAzimHandle);
            }

            if ((tableDist < dist) && (tableElev > elev))
            {
                elev = (double)tableElev;
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Module Global.  
        /// </summary>
        /// <param name="siteStruct"> - struct of Es site info</param>
        /// <param name="isMDB"> - is ES from the MDB flag</param>
        /// <param name="anteStruct"> - struct of ES ante info</param>
        /// <param name="tableName"> - name of ES table</param>
        /// <param name="stnStruct"> - AUX type ES station struct</param>
        /// <returns></returns>
        public static int TeFillEsStnStr(TeSite siteStruct,  /* struct of Es site info */
                       bool isMDB,         /* is ES from the MDB flag */
                       TeAnte anteStruct,  /* struct of ES ante info */
                       string tableName,	/* name of ES table */
                       out AxStation stnStruct) /* AUX type ES station struct */
        {
            // 'out' requirement.
            stnStruct = new AxStation();

            string select;
            int rc;
            SQLLEN[] tmpAnteNulls;  //[FE_ANTE_SIZE_];
            FeAnte feTmpAnte;

            stnStruct.elevM = siteStruct.earthgrnd;
            stnStruct.name = siteStruct.earthname;

            TpSub.TpLoadLat(siteStruct.earthlatit, out stnStruct.LL.latSens, out stnStruct.LL.latDeg, out stnStruct.LL.latMin, out stnStruct.LL.latSec);

            TpSub.TpLoadLong(siteStruct.earthlongit, out stnStruct.LL.longSens, out stnStruct.LL.longDeg, out stnStruct.LL.longMin, out stnStruct.LL.longSec);

            select = String.Format("location = '{0}'", anteStruct.earthlocation);

            if ((rc = TpMdbPdfGet.TeAnteGet(select, tableName, isMDB, out feTmpAnte, out tmpAnteNulls)) != Constant.SUCCESS)
            {
                return (rc);
            }

            stnStruct.antHtM = feTmpAnte.aht;

            return (Constant.SUCCESS);
        }

        private static double incr = -1.0;

        /// <summary>
        /// This method calulates the angle SET (satellite, Earth Station, Terrestrial 
        /// Station.  
        /// </summary>
        /// <param name="es"></param>
        /// <param name="anteStruct"> - struct of Es site info</param>
        /// <param name="siteStruct"> - struct of Es site info</param>
        /// <param name="parmStruct"> - struct of Es site info</param>
        /// <param name="anteNulls"></param>
        /// <param name="refIndex"></param>
        /// <param name="intPrintMsg"></param>
        /// <param name="vicPrintMsg"></param>
        /// <returns></returns>
        public static int TeCalcSET(AxStation es,
          ref TeAnte anteStruct,   /* struct of Es site info */
          TeSite siteStruct,    /* struct of Es site info */
          TpParm parmStruct,   /* struct of Es site info */
          ref SQLLEN[] anteNulls,
          double refIndex,

          string intPrintMsg,

          string vicPrintMsg)
        {
            //...Log2.v("\nTeSubCalc.TeSubCalc.TeCalcSET()(): Entry");
            //&&Console.Error.Write("\nTeSubCalc.TeCalcSet():");

            double nlng;
            double tmp;
            double earthAGain;
            double tmpEsAzim = 0.0;
            double tmpEsElev;
            double tmpSET;
            double savEsAzim = 0.0;
            double savEsElev = 0.0;
            double savSET = 0.0;

            float minADisc;
            float tmpMin;
            float adiscctxv;
            float adiscxtxv;
            float adiscctxh;
            float adiscxtxh;

            int rc;
            int tmpG;
            int tmpGain;

            string anteTableName;
            string junk;
            string select;

            AxStation ste;

            PatternStruct patDat = null;

            /*double  dArcStep;*/
            ParmStrct pParms;     /*	Pointer to the anonymous parm structure. */
            string cArcStep;
            int nAboveHorizon = 0;  /*	0 starting, -1 starts below, +1 moved above */

            if (incr < 0)
            {
                /*  Break out the arc step from the parm field (parmparm) */
                pParms = GenUtil.ParmBreakOut(parmStruct.parmparm);

                cArcStep = GenUtil.ParmByName(pParms, "ARCSTEP");

                if (cArcStep == null || (incr = Convert.ToDouble(cArcStep)) <= 0.0)
                {
                    /*  Default to the normal arcstep */
                    if (anteStruct.sarc2 <= 9.5)
                    {
                        incr = 1.0;
                    }
                    else
                    {
                        incr = anteStruct.sarc2 / 9.5;
                    }
                }

                TpRunTsip.mdArcStep = incr;
                //...Log2.v("\nA: TpRunTsip.mdArcStep = " + TpRunTsip.mdArcStep);
            }

            //...Log2.v("\nB: TpRunTsip.mdArcStep = " + TpRunTsip.mdArcStep);

            anteTableName = "sd_ante";

            /* get antenna pattern and calculate antenna discriminations for
             * the earth antenna
             */
            //&&Console.Error.Write("\nanteStruct.earthacode = " + anteStruct.earthacode);
            if (!anteStruct.earthacode.StartsWith("CCIR"))
            {
                if ((rc = TpGetDat.TpGetPattern(anteStruct.earthacode, out patDat)) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    if (rc == Error.NOANTDFOUND || rc == Error.NOANTEFOUND)
                    {
                        select = String.Format("acode = '{0}'", anteStruct.earthacode);
                        ErrMsg.UtPrintMessage(rc, anteTableName, select);
                        return (Constant.CONT_PROCESSING);
                    }
                    else
                    {
                        Log2.e("\nTeSubCalc.TeCalcSET(): ERROR: call to TpGetPattern() failed");
                        TpRunTsip.mTW_ERR.Write("Error Getting antenna pattern: {0}\n", rc);
                    }
                    return (rc);
                }
            }
            tmpMin = (float)Constant.MAXADISC;

            ste = new AxStation();

            //&&Console.Error.Write("\nteSubCalc.teCalcSet(): baboon: anteStruct->sarc1 = {0}", anteStruct.sarc1);
            //&&Console.Error.Write("\nteSubCalc.teCalcSet(): baboon: anteStruct->sarc2 = {0}", anteStruct.sarc2);
            //&&Console.Error.Write("\nteSubCalc.teCalcSet(): baboon: incr = {0}", incr);
            for (nlng = anteStruct.sarc1 - anteStruct.sarc2;
                 nlng <= anteStruct.sarc1 + anteStruct.sarc2;
                 nlng += incr)
            {

                ste.LL.longSeconds = nlng * 3600.0;

                rc = SatAze.Sataze(es, ste, out tmpEsAzim, out tmp, out tmpEsElev);
                //&&Console.Error.Write("\nteSubCalc.teCalcSet(): turtle: tmpEsAzim = {0}", tmpEsAzim);
                if (rc < 0)
                {
                    /*	Error.  Ignore this longitude	*/
                    TpRunTsip.mTW_ERR.Write("Error in calculating Satellite elevation, Long: {0:F2} ({1})\n",
                                   nlng, rc);
                    continue;
                }
                else if (rc > 0)
                {
                    /*	Beam is beneath the Horizon */
                    if (nAboveHorizon == 0)
                    {
                        /*	The beam starts out beneath the level.  Set this to -1 to indicate
                        *		this */
                        nAboveHorizon = -1;
                        continue;
                    }
                    else if (nAboveHorizon == +1)
                    {
                        /*	If the beam is now beneath the horizon, and it was above it
                        *		we stop. */
                        if (IsDroppedBelowToPrint)
                        {
                            TpRunTsip.mTW_ERR.Write("\n** WARNING ** The service arc has dropped below the level (0.0 elev).\nThis occurred at Longitude {0:F2} for site {1}.\nBeams below 0.0 degrees elevation are not processed.\n-- This message will only appear once. --\n",
                                             nlng, anteStruct.earthlocation);
                            IsDroppedBelowToPrint = false;
                        }
                        break;
                    }
                    else
                    {
                        /*	Beam is below the horizon, but has never been above.  */
                        continue;
                    }
                }
                else
                {
                    /*	Beam is above horizon.  Check if it just moved above */
                    if (nAboveHorizon == -1)
                    {
                        if (IsLowElevMessToPrint)
                        {
                            TpRunTsip.mTW_ERR.Write("\n** WARNING ** The service arc starts below the level (0.0 elev).\nIt first comes above the level at Longitude {0:F2} for site {1}.\nBeams below 0.0 degrees elevation are not processed.\n-- This message will only appear once. --\n",
                                             nlng, anteStruct.earthlocation);
                            IsLowElevMessToPrint = false;
                        }
                    }
                    nAboveHorizon = +1;
                }


                SepAng.AxSepAng(tmpEsAzim, tmpEsElev, siteStruct.etazim, anteStruct.etelev,
                         out tmpSET);

                minADisc = (float)Constant.MAXADISC;
                if (!anteStruct.earthacode.StartsWith("CCIR"))
                {
                    TpSub.TpFindDisc(patDat.pattern, patDat.numPts, (float)Abs(tmpSET), out adiscctxv,
                               out adiscxtxv, out adiscctxh, out adiscxtxh);

                    if (patDat.nulls[Constant.DCOV] != Constant.DB_NULL)
                    {
                        minADisc = (minADisc < adiscctxv) ? minADisc : adiscctxv;
                    }
                    if (patDat.nulls[Constant.DXPV] != Constant.DB_NULL)
                    {
                        minADisc = (minADisc < adiscxtxv) ? minADisc : adiscxtxv;
                    }
                    if (patDat.nulls[Constant.DCOH] != Constant.DB_NULL)
                    {
                        minADisc = (minADisc < adiscctxh) ? minADisc : adiscctxh;
                    }
                    if (patDat.nulls[Constant.DXPH] != Constant.DB_NULL)
                    {
                        minADisc = (minADisc < adiscxtxh) ? minADisc : adiscxtxh;
                    }
                }
                else
                {
                    if (Strings.FirstCharIs((anteStruct.interferer), 'E'))
                    {
                        junk = anteStruct.earthacode.Substring(0, 4);
                        tmpG = Convert.ToInt32(anteStruct.earthacode.Substring(4, 2));
                        tmpGain = Convert.ToInt32(anteStruct.earthacode.Substring(6, 2));
                    }
                    else
                    {  /* TS-ES */
                       /* positions 5&6 are the againRx */
                        junk = anteStruct.earthacode.Substring(0, 4);
                        tmpGain = Convert.ToInt32(anteStruct.earthacode.Substring(4, 2));
                        tmpG = Convert.ToInt32(anteStruct.earthacode.Substring(6, 2));
                    }
                    earthAGain = (double)tmpGain;

                    if (Abs(tmpSET) < 1.0)
                    {
                        minADisc = 0.0f;
                    }
                    else if (Abs(tmpSET) >= 48.0)
                    {
                        minADisc = (float)(earthAGain + 10.0);
                    }
                    else
                    {
                        minADisc = (float)(earthAGain - (32.0 - 25.0 * Log10(tmpSET)));
                    }
                }

                if (minADisc < tmpMin)
                {
                    savSET = tmpSET;
                    savEsAzim = tmpEsAzim;
                    //&&Console.Error.Write("\nteSubCalc.teCalcSET(): goat: savEsAzim = {0}", savEsAzim);
                    savEsElev = tmpEsElev;
                    tmpMin = minADisc;
                }
                else if (minADisc == tmpMin)
                {
                    tmpMin = minADisc;

                    if (savSET > tmpSET)
                    {
                        savSET = tmpSET;
                        savEsAzim = tmpEsAzim;
                        //&&Console.Error.Write("\nteSubCalc.teCalcSET(): pig: savEsAzim = {0}", savEsAzim);
                        savEsElev = tmpEsElev;
                    }
                }
            }

            if (tmpMin == Constant.MAXADISC)
            {
                anteNulls[TeAnte.EDISCANG] = Constant.DB_NULL;
                anteNulls[TeAnte.ESAZIM] = Constant.DB_NULL;
                anteNulls[TeAnte.ESELEV] = Constant.DB_NULL;
                anteNulls[TeAnte.ADISC_SET] = Constant.DB_NULL;
            }
            else
            {
                anteStruct.ediscang = savSET;
                anteStruct.esazim = savEsAzim;
                //&&Console.Error.Write("\nteSubCalc.teCalcSET(): anteStruct.esazim = {0}", anteStruct.esazim);
                anteStruct.eselev = savEsElev;
                anteStruct.adisc_set = (double)tmpMin;

                anteNulls[TeAnte.EDISCANG] = Constant.DB_NOT_NULL;
                anteNulls[TeAnte.ESAZIM] = Constant.DB_NOT_NULL;
                anteNulls[TeAnte.ESELEV] = Constant.DB_NOT_NULL;
                anteNulls[TeAnte.ADISC_SET] = Constant.DB_NOT_NULL;
            }

            //...Log2.v("\nTeSubCalc.TeCalcSET(): Exit");
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method performs rain calculations for the given site and antenna. The 
        /// calculated values are as follows: evdistes - earth to rain volume dist on 
        /// the ES vector tvdistes - terr. to rain volume dist on the ES vector tvazim 
        /// - TS to rain volume azimuth tvelev - TS to rain volume elevation angleutv - 
        /// angle UTV (remote TS, TS, rain volume) tvdisttu - TS to rain volume on the 
        /// TU vector evdisttu - ES to rain volume on the TU vector evazim - earth to 
        /// rain volume azimuth evelev - earth to rain volume elevation anglesev - 
        /// angle SEV (satellite, earth, rain volume).  
        /// </summary>
        /// <param name="anteStruct"> - struct of ES info</param>
        /// <param name="anteNulls"></param>
        /// <param name="siteStruct"> - struct of ES info</param>
        /// <param name="tuVec"> - Geometric T to U vect</param>
        public static void TeAnteRainCalcs(ref TeAnte anteStruct,    /* struct of ES info */
                         ref SQLLEN[] anteNulls,
                         TeSite siteStruct,	/* struct of ES info */
                     double[] tuVec)       /* Geometric T to U vect */
        {
            double[] esVec = new double[3];
            double[] teVec = new double[3];
            double[] etVec = new double[3];
            double[] euVec = new double[3];
            double[] tu1Vec = new double[3];
            double[] tvVec = new double[3];
            double[] evVec = new double[3];

            /*	Offaxis antenna vector - GJS - 1108 - 2002.12 */
            double[] taVec = new double[3];

            double[] evAngVec = new double[3];
            double lambda;
            double tmpVal;
            double HR;
            double tuDist;
            double tuElev;
            double nu;
            double RAD;
            double ettu1;
            double eset;
            double et2;
            double estu1;
            double esDist;
            double h;
            double az;
            double ez;
            double te2;
            double tu1es;
            double tees;
            double tu1te;
            double tu2;
            double tute;
            double[] tvAngVec = new double[3];
            double cosalp;
            double cosbet;

            RAD = 3.1415926536 / 180.0;

            HR = CalcHR(siteStruct.earthlatit / 100.0);

            /*	Build the geometry using SEZ vectors around the ES site */
            /*	esVec is the unit vector from the ES site to the satellite */
            TeVects.TeBuildVector(1.0, anteStruct.eselev, anteStruct.esazim, out esVec);
            /*	etVec is the vector from the es site to the ts site */
            TeVects.TeBuildVector(siteStruct.etdist, anteStruct.etelev, siteStruct.etazim, out etVec);
            /*	euVec is the vector from the es site to the u site (u is other end of the
            *		ts link */
            TeVects.TeBuildVector(siteStruct.eudist, anteStruct.euelev, siteStruct.euazim, out euVec);
            /*	tuVec is the vector (SEZ in the es frame of reference) from the ts site
            *		to u. fed in because we need the geometric vector here, not the refracted
            *		vector or one using the radio horizon.  GJS - 1108 - 2003.01
            teVectorSub(euVec, etVec, tuVec); */
            TeVects.TeVectorLen(tuVec, out tuDist);
            /*	and tu1Vec is the unit vector from ts to u */
            TeVects.TeVectorUnit(tuVec, out tu1Vec);

            tuElev = AtanD(tu1Vec[2] / Sqrt(1.0 - Pow(tu1Vec[2], 2.0)));

            if (Strings.FirstCharIs((anteStruct.tsoffaxis), 'Y'))
            {
                /*	Off axis angle.  Create the unit vector along antenna boresight
                *		Note that we are creating this vector in the SEZ coordination system
                *		of the ES station, although the arguments are for the SEZ coordinates
                *		at the Terrestrial site.  It is felt that within 200km or so, this will
                *		not give material error.  GJS - 1108 - 2003.01 */
                TeVects.TeBuildVector(1.0, anteStruct.tstrueel, anteStruct.tstrueaz, out taVec);
            }

            /* Rain Scatter Cell on ES vector */

            /*	ettu1 is the dot product of es to ts on the ts to u unit vector. */
            TeVects.TeVectorLinMux(etVec, tu1Vec, out ettu1);
            /*	eset is the dot product of earth to sat on the earth to terrestrial vec.*/
            TeVects.TeVectorLinMux(esVec, etVec, out eset);
            /*	et2 is the square of the length of the earth to terrestrial vector */
            TeVects.TeVectorLinMux(etVec, etVec, out et2);
            /*	estu1 is the dot prod of the earth-sat vec on the terr. link vect. */
            TeVects.TeVectorLinMux(esVec, tu1Vec, out estu1);

            /*	lambda is the distance along the es vector that we have a rain cell.
            *		reference needed on this, as it is different from the Engineering specs */
            lambda = (-ettu1 * eset + et2 * estu1) / (-ettu1 + estu1 * eset);
            lambda = (lambda < 0.1) ? 0.1 : lambda;

            TeVects.TeVectorLen(esVec, out esDist);
            anteStruct.evdistes = lambda * esDist;
            anteNulls[TeAnte.EVDISTES] = Constant.DB_NOT_NULL;

            /*	evdistes is the distance along the es to Sat vector that the rain
            *		cell is found.  If this is above the maximum height (HR), then it is set
            *		to the distance to the maximum height */
            h = anteStruct.evdistes * SinD(anteStruct.eselev);
            if (h > HR)
            {
                anteStruct.evdistes = HR / SinD(anteStruct.eselev);
                lambda = anteStruct.evdistes;
            }

            /*	tvVec becomes the vector from the ts site to the rain volume */
            TeVects.TeVectorSclMux(lambda, esVec, out tvVec);

            /*	calculated form the es-V - es-TS vectors */
            TeVects.TeVectorSub(tvVec, etVec, out tvVec);

            /*	Get the distance (tvdistes) ts to volume */

            //AH: REMOVE
            bool filter = anteStruct.terrcall1.Equals("CHX576");
            filter &= anteStruct.terrcall2.Equals("VEL885");
            if (filter)
            {
                string str = String.Format("\nFILTER-alpha: {0}  {1}  {2}  {3}", anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.tvdistes, anteNulls[TeAnte.TVDISTES]);
                //...Log2.v(str);
            }

            TeVects.TeVectorLen(tvVec, out anteStruct.tvdistes);
            anteNulls[TeAnte.TVDISTES] = Constant.DB_NOT_NULL;

            //AH: REMOVE
            filter = anteStruct.terrcall1.Equals("CHX576");
            filter &= anteStruct.terrcall2.Equals("VEL885");
            if (filter)
            {
                string str = String.Format("\nFILTER-beta: {0}  {1}  {2}  {3}", anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.tvdistes, anteNulls[TeAnte.TVDISTES]);
                //...Log2.v(str);
            }

            /*	Get the angles of the TV vector. */
            tvAngVec[0] = Abs(Atan(tvVec[1] / tvVec[0])); /* 0 is angle from South */
            tvAngVec[1] = Abs(Atan(tvVec[0] / tvVec[1])); /*	1 is angle from East */
                                                          /*	2 is elevation angle.  All wrt the SEZ at the ES site */
            tvAngVec[2] = Abs(Atan((tvVec[2] / anteStruct.tvdistes) /
                                          (Pow(1.0 - Pow(tvVec[2] / anteStruct.tvdistes, 2.0), 0.5))));

            az = tvAngVec[0] / RAD;

            if (tvVec[0] >= 0.0)
            {
                if (tvVec[1] >= 0.0)
                {
                    anteStruct.tvazim = 180.0 - az;
                }
            }
            else if (tvVec[1] >= 0.0)
            {
                anteStruct.tvazim = az;
            }
            else
            {
                anteStruct.tvazim = 360.0 - az;
            }
            anteNulls[TeAnte.TVAZIM] = Constant.DB_NOT_NULL;

            anteStruct.tvelev = tvAngVec[2] / RAD;
            anteNulls[TeAnte.TVELEV] = Constant.DB_NOT_NULL;

            /*	Why this is done, rather than remove the fabs call above,
            *		is not clear and might be historical */
            if (tvVec[2] < 0.0)
            {
                anteStruct.tvelev = -anteStruct.tvelev;
            }

            /*	Get the projection of the Terrestrial to link vector on the terr. to
            *		volume vector */
            TeVects.TeVectorLinMux(tuVec, tvVec, out tmpVal);
            /*	cosalp is the distance along the TU vector as if both were unit vectors */
            cosalp = tmpVal / (tuDist * anteStruct.tvdistes);

            /*	calculate utv from one side of a right unit triangle */
            anteStruct.angleutv = 90.0 - AtanD(cosalp / (Pow(1.0 - Pow(cosalp, 2.0), 0.5)));
            anteNulls[TeAnte.ANGLEUTV] = Constant.DB_NOT_NULL;


            TeVects.TeVectorSclMux(-1.0, etVec, out teVec); /*	Get ready for the next section, but
																			*		this is needed for offaxis angles */

            if (Strings.FirstCharIs((anteStruct.tsoffaxis), 'Y'))
            {
                /*	The antenna at the TS site is an offaxis angle antenna.
                *		Calculate the angles for its true azimuth (A) */
                anteStruct.angleuta = TeVects.Vangle(tuVec, taVec);
                anteNulls[TeAnte.ANGLEUTA] = Constant.DB_NOT_NULL;

                anteStruct.angleeta = TeVects.Vangle(teVec, taVec);
                anteNulls[TeAnte.ANGLEETA] = Constant.DB_NOT_NULL;

                anteStruct.angleatv = TeVects.Vangle(tvVec, taVec);
                anteNulls[TeAnte.ANGLEATV] = Constant.DB_NOT_NULL;
            }


            /*	Rain Scatter Cell on TU vector */
            /*	Exactly the same logic as above, on the TU vector, mutatis mutandi */

            TeVects.TeVectorLinMux(teVec, teVec, out te2);
            TeVects.TeVectorLinMux(tu1Vec, esVec, out tu1es);
            TeVects.TeVectorLinMux(teVec, esVec, out tees);
            TeVects.TeVectorLinMux(tu1Vec, teVec, out tu1te);
            TeVects.TeVectorLinMux(tuVec, tuVec, out tu2);
            TeVects.TeVectorLinMux(tuVec, teVec, out tute);

            nu = (te2 * tu1es - tees * tu1te) / (tu1es * tu1te - tees);
            nu = (nu < 0.1) ? 0.1 : nu;

            anteStruct.tvdisttu = nu;
            anteNulls[TeAnte.TVDISTTU] = Constant.DB_NOT_NULL;

            h = anteStruct.tvdisttu * SinD(tuElev);

            if (h > HR)
            {
                anteStruct.tvdisttu = HR / SinD(tuElev);
                nu = anteStruct.tvdisttu;
            }
            TeVects.TeVectorSclMux(nu, tu1Vec, out evVec);
            TeVects.TeVectorSub(evVec, teVec, out evVec);

            TeVects.TeVectorLen(evVec, out anteStruct.evdisttu);
            anteNulls[TeAnte.EVDISTTU] = Constant.DB_NOT_NULL;

            evAngVec[0] = Abs(Atan(evVec[1] / evVec[0]));
            evAngVec[1] = Abs(Atan(evVec[0] / evVec[1]));
            evAngVec[2] = Abs(Atan((evVec[2] / anteStruct.evdisttu) /
                            (Pow(1.0 - Pow(evVec[2] / anteStruct.evdisttu, 2.0), 0.5))));

            ez = evAngVec[0] / RAD;

            if (evVec[0] >= 0.0)
            {
                if (evVec[1] >= 0.0)
                {
                    anteStruct.evazim = 180.0 - ez;
                }
                else
                {
                    anteStruct.evazim = 180.0 + ez;
                }
            }
            else if (evVec[1] >= 0.0)
            {
                anteStruct.evazim = ez;
            }
            else
            {
                anteStruct.evazim = 360.0 - ez;
            }
            anteNulls[TeAnte.EVAZIM] = Constant.DB_NOT_NULL;

            anteStruct.evelev = evAngVec[2] / RAD;
            anteNulls[TeAnte.EVELEV] = Constant.DB_NOT_NULL;

            if (evVec[2] < 0.0)
            {
                anteStruct.evelev = -anteStruct.evelev;
            }

            TeVects.TeVectorLinMux(evVec, esVec, out tmpVal);

            cosbet = tmpVal / anteStruct.evdisttu;

            anteStruct.anglesev = -AtanD(cosbet / Sqrt(1.0 - Pow(cosbet, 2.0))) + 90.0;
            anteNulls[TeAnte.ANGLESEV] = Constant.DB_NOT_NULL;
        }

        /// <summary>
        /// Calculates the HR value given the latitude of the earth 
        /// station.  
        /// </summary>
        /// <param name="earthLatSec"></param>
        /// <returns></returns>
        public static double CalcHR(double earthLatSec)
        {
            double HR, earthLatDeg;

            earthLatDeg = earthLatSec / 3600.0;
            if (earthLatDeg <= 20.0)
            {
                HR = 5.2;
            }
            else if (earthLatDeg <= 70.0)
            {
                HR = 7.15 - (0.081 * earthLatDeg) - (2450.0 / Pow(earthLatDeg, 3.0));
            }
            else
            {
                HR = 1.47;
            }
            return (HR);
        }

        /// <summary>
        /// Calculates the 20% loss for mode 1 (tropospheric scattering).  
        /// </summary>
        /// <param name="dist"></param>
        /// <param name="radio"></param>
        /// <param name="freqMhz"></param>
        /// <param name="earthht"></param>
        /// <param name="terrht"></param>
        /// <param name="spherecalc"></param>
        /// <param name="loss"></param>
        public static void TeCalcL20M1(double dist,
                                 string radio,
                                 double freqMhz,
                                 double earthht,
                                 double terrht,
                                 string spherecalc,
                                 out double loss)
        {
            double const1, const2, C1 = 17.6, C2 = 6374.82;
            double x, fx, y, yt, ye, gyt, gye, fsl;


            if (Strings.FirstCharIs(radio, 'A') &&
                (Strings.FirstCharIs(spherecalc, 'Y') || Strings.FirstCharIs(spherecalc, '2'))
                )
            {
                x = 2.2 * dist * Pow(freqMhz, 1.0 / 3.0) * Pow(Constant.K_FACTOR * C2, -2.0 / 3.0);
                fx = 11.0 + 10.0 * Log10(x) - C1 * x;
                y = 9.600001 * Pow(freqMhz, 2.0 / 3.0) * Pow(Constant.K_FACTOR * C2, -1.0 / 3.0);
                yt = y * terrht;
                ye = y * earthht;
                if (yt > 2.0)
                {
                    gyt = C1 * Pow(yt - 1.1, 0.5) - 5.0 * Log10(yt - 1.1) - 8.0;
                }
                else
                {
                    gyt = 20.0 * Log10(yt + 0.1 * Pow(yt, 3.0));
                }
                if (ye > 2.0)
                {
                    gye = C1 * Pow(ye - 1.1, 0.5) - 5.0 * Log10(ye - 1.1) - 8.0;
                }
                else
                {
                    gye = 20.0 * Log10(ye + 0.1 * Pow(ye, 3.0));
                }
                fsl = 32.45 + 20.0 * Log10(dist) + 20.0 * Log10(freqMhz);
                loss = ((fsl - fx - gyt - gye) < fsl) ? fsl : (fsl - fx - gyt - gye);
            }
            else
            {
                if (dist <= 90.0)
                {
                    const1 = 104.49;
                    const2 = 20.0;
                }
                else if (dist <= 160.0)
                {
                    if (Strings.FirstCharIs(radio, 'A'))
                    {
                        const1 = -228.0;
                        const2 = 190.0;
                    }
                    else if (Strings.FirstCharIs(radio, 'B'))
                    {
                        const1 = -207.4;
                        const2 = 179.57;
                    }
                    else
                    {
                        const1 = 191.75;
                        const2 = 171.56;
                    }
                }
                else
                {
                    if (Strings.FirstCharIs(radio, 'A'))
                    {
                        const1 = 14.0;
                        const2 = 80.0;
                    }
                    else if (Strings.FirstCharIs(radio, 'B'))
                    {
                        const1 = 17.8;
                        const2 = 77.4;
                    }
                    else
                    {
                        const1 = 15.36;
                        const2 = 77.6;
                    }
                }
                if (freqMhz > 0)
                {
                    loss = const1 + const2 * Log10(dist) + 20.0 * Log10(freqMhz / 4000.0);
                }
                else
                {
                    loss = 0;
                }
            }
        }

        /// <summary>
        /// Calculates the . 01% loss for mode 1.  
        /// </summary>
        /// <param name="radio"></param>
        /// <param name="loss20m1"></param>
        /// <param name="freqMhz"></param>
        /// <param name="elevAng"></param>
        /// <param name="dist"></param>
        /// <param name="type"></param>
        /// <param name="loss"></param>
        public static void TeCalcL01M1(string radio,
                                 double loss20m1,
                                 double freqMhz,
                                 double elevAng,
                                 double dist,
                                 short type,
                                 out double loss)
        {
            // 'out' requirement.
            loss = 0.0;

            double lossOxygen;
            double lossVapour;
            double lossOther;
            double baseRate = 0.0;
            double rateOfAtten = 0.0;
            double lossHor;
            double freqGhz;

            if (dist < 90.0)
            {
                loss = loss20m1;
                return;
            }

            freqGhz = freqMhz / 1000.0;
            if (freqMhz > 0)
            {
                baseRate = 120.0 + 20.0 * Log10(freqGhz);

                lossOxygen = 0.0068 * Pow(freqGhz, Constant.SQUARE) *
                               ((1.0 / Pow(60.0 - freqGhz, Constant.SQUARE)) +
                               (1.0 / Pow(60.0 + freqGhz, Constant.SQUARE)) +
                               (1.0 / (Pow(freqGhz, Constant.SQUARE) + 0.36)));

                lossVapour = (0.000003 * Pow(freqGhz, Constant.SQUARE)) + 0.00035 *
                               ((1.0 / (Pow(1.0 - 22.3 / freqGhz, Constant.SQUARE) +
                               (9.0 / Pow(freqGhz, Constant.SQUARE)))) +
                               (1.0 / Pow(1.0 + 22.3 / freqGhz, Constant.SQUARE)));


                if (Strings.FirstCharIs(radio, 'A'))
                {
                    lossOther = 0.154 * Pow(1.0 + 3.05 * Log10(freqGhz), 0.4) *
                                Pow(0.9028 + 0.0486 * Log10(0.01), Constant.SQUARE);
                    rateOfAtten = lossOxygen + lossVapour + lossOther;
                }
                else
                {
                    lossOther = Pow(0.272 + 0.047 * Log10(0.01), Constant.SQUARE);
                    if (Strings.FirstCharIs(radio, 'B'))
                    {
                        rateOfAtten = lossOxygen + 2.0 * lossVapour + lossOther;
                    }
                    else
                    {
                        rateOfAtten = lossOxygen + 5.0 * lossVapour + lossOther;
                    }
                }
            }

            if (type == Constant.ES_TS)
            {
                HorLoss.AxHorLoss(freqGhz, elevAng, out lossHor);
            }
            else
            {
                lossHor = 0.0;
            }

            if (freqMhz > 0)
            {
                loss = baseRate + (dist * rateOfAtten) + lossHor;
            }
            else
            {
                loss = 0;
            }
        }

        /// <summary>
        /// Calculates the rain scattering angle. The rain volume is first 
        /// calculated to be on the ES vector and then the TU vector. The transmission 
        /// loss for scattering propagation is calculated for both cases and the 
        /// minimum loss determines the calculated placement of the volume. The angle 
        /// is either SEV (satellite, earth, rain volume) or UTV (TS remote site, TS, 
        /// rain volume).  
        /// </summary>
        /// <param name="parmStruct"></param>
        /// <param name="siteStruct"></param>
        /// <param name="anteStruct"></param>
        /// <param name="chanStruct"></param>
        /// <param name="chanNulls"></param>
        /// <param name="earthAGain"></param>
        /// <param name="terrAGain"></param>
        /// <param name="earthAfslt"></param>
        /// <param name="nullEarthAfslt"></param>
        /// <param name="earthAfslr"></param>
        /// <param name="nullEarthAfslr"></param>
        /// <param name="terrAfsl"></param>
        /// <param name="loss01mode2"></param>
        /// <param name="nullL01M2"></param>
        /// <param name="intPrintMsg"></param>
        /// <param name="vicPrintMsg"></param>
        /// <returns></returns>
        public static int TeCalcScang(TpParm parmStruct,
                                ref TeSite siteStruct,
                                ref TeAnte anteStruct,
                                ref TeChan chanStruct,
                                ref SQLLEN[] chanNulls,
                                double earthAGain,
                                double terrAGain,
                                double earthAfslt,
                                SQLLEN nullEarthAfslt,
                                double earthAfslr,
                                SQLLEN nullEarthAfslr,
                                double terrAfsl,
                                out double loss01mode2,
                                out SQLLEN nullL01M2,
                                string intPrintMsg,
                                string vicPrintMsg)
        {
            // 'out' requirements.
            loss01mode2 = 0.0;
            nullL01M2 = Constant.DB_NULL;

            double adisccv = 0.0;
            double adiscxv = 0.0;
            double adiscch = 0.0;
            double adiscxh = 0.0;
            double terrMbnd = 0.0;
            double adiscSev = Constant.DFLT_ADISCW;
            double adiscUtv = Constant.DFLT_ADISCW;
            double adiscAlpha = Constant.DFLT_ADISCW;
            double adiscBeta = Constant.DFLT_ADISCW;
            double HR;
            double R;
            double alpha;
            double beta;
            double abtx;
            double abrx;
            double lrtx;
            double lrrx;
            double kltx;
            double klrx;
            double ettu1;
            double eset;
            double et2;
            double estu;
            double estu1;
            double evDist;
            double tvDist;
            double junk;
            double tmpVal;
            double cond;
            double tuDist;
            double te2;
            double tu1es;
            double tees;
            double tu1te;
            double tu2;
            double tute;
            double lossVolAlpha;
            double lossVolBeta;
            double disc;
            double[] esVec = new double[3];
            double[] etVec = new double[3];
            double[] euVec = new double[3];
            double[] teVec = new double[3];
            double[] tuVec = new double[3];
            double[] tu1Vec = new double[3];
            double[] tmpVec = new double[3];
            double[] tvVec = new double[3];
            double[] evVec = new double[3];
            double tvdiscang;

            string junkName = "";
            string terrSiteName;
            string terrAnteName = "";

            SQLLEN nullCv = 0;
            SQLLEN nullXv = 0;
            SQLLEN nullCh = 0;
            SQLLEN nullXh = 0;
            bool terrMDB = false;
            bool earthMDB = false;

            int tmpG = 0; ;
            int tmpGain = 0;
            int rc;

            SdBand curBand;

            if (anteStruct.earthacode.StartsWith("CCIR"))
            {
                int nRes = sf.Parse(anteStruct.earthacode, "%4s%2d%2d");
                List<object> results = sf.Results;
                junkName = (string)results[0];

                if (Strings.FirstCharIs((anteStruct.interferer), 'E'))
                {
                    //&&Console.Error.Write("\nBolshoi");
                    tmpG = (int)results[1];
                    tmpGain = (int)results[2];
                }
                else
                {  /* TS-ES */
                    //&&Console.Error.Write("\nVelvet");
                    /* positions 5&6 are the againRx */
                    tmpG = (int)results[2];
                    tmpGain = (int)results[1];
                }

                //&&Console.Error.Write("\njunkName = " + junkName);
                //&&Console.Error.Write("\ntmpG = " + tmpG);
                //&&Console.Error.Write("\ntmpGain = " + tmpGain);
            }

            //&&Console.Error.Write("\nteSubCalc.teCalc.Scang(): aardvark: earthacode = " +  anteStruct.earthacode);
            /* calc discrimination for angle SEV -> adiscSev */
            if (!anteStruct.earthacode.StartsWith("CCIR"))
            {
                if ((rc = TpGetDat.TpCalcDisc(anteStruct.earthacode,
                                     Abs(anteStruct.anglesev), out adisccv, out adiscxv,
                                                         out adiscch, out adiscxh, out nullCv, out nullXv, out nullCh, out nullXh,
                                                         intPrintMsg, vicPrintMsg)) != Constant.SUCCESS)
                {
                    return (rc);
                }

                FindMin(adisccv, adiscxv, adiscch, adiscxh, (short)nullCv, (short)nullXv, (short)nullCh, (short)nullXh, ref adiscSev);
                if (adiscSev == Constant.DFLT_ADISCW)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.CALCDISC, "SEV");
                    return (Constant.FAILURE);
                }
            }
            else
            {
                if (Abs(anteStruct.anglesev) < 1.0)
                {
                    adiscSev = 0.0;
                }
                else if (Abs(anteStruct.anglesev) >= 48.0)
                {
                    adiscSev = (double)tmpGain + 10.0;
                }
                else
                {
                    adiscSev = (double)tmpGain - (32.0 - 25.0 * Log10(anteStruct.anglesev));
                }
            }

            /* check if local TS site is a passive reflector:
             * if call1 begins with '%'
             */
            if (Strings.FirstCharIs(anteStruct.terrcall1, '%'))
            {

                TeSubCalc.TeTableNames(parmStruct, Constant.FT_SITE, out terrSiteName, out junkName, out junkName,
                             out terrMDB, out earthMDB);

                TeSubCalc.TeTableNames(parmStruct, Constant.FT_ANTE, out terrAnteName, out junkName, out junkName,
                             out terrMDB, out earthMDB);

                /* get midband freq from SDB for terrestrial site */
                if (TsipUtils.UtGetBand(anteStruct.terrbndcde, out curBand) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.INVALIDBANDCODE, anteStruct.terrbndcde);
                    return (Constant.FAILURE);
                }
                terrMbnd = curBand.bmidf;

                /* calculate terrestrials's passive reflector & discrim */
                if (Strings.FirstCharIs(anteStruct.interferer, 'T'))
                {
                    rc = TtCalkPassive.TtCalcPassive(true, terrMDB, terrAnteName,
                                       anteStruct.terrcall1, anteStruct.terrcall2,
                                                         anteStruct.terrbndcde, anteStruct.terranum, terrMbnd,
                                         anteStruct.angleutv, out disc, out junk, intPrintMsg,
                                         vicPrintMsg);
                }
                else
                {
                    rc = TtCalkPassive.TtCalcPassive(false, terrMDB, terrAnteName,
                                       anteStruct.terrcall1, anteStruct.terrcall2,
                                                         anteStruct.terrbndcde, anteStruct.terranum, terrMbnd,
                                         anteStruct.angleutv, out disc, out junk, intPrintMsg,
                                                         vicPrintMsg);
                }
                if (rc != Constant.SUCCESS)
                {
                    return (rc);
                }
                adiscUtv = disc;
            }
            else
            {
                /* terrestrial site is not a passive repeater
                 * get antenna pattern and calculate antenna discriminations for
                 * the TS antenna
                 */
                /* get full table names for subsidiary ante and antd info */

                /*	Decide on the discrimination angle if this is off-axis - 1108
                *		GJS - 2002.12 */
                if (Strings.FirstCharIs((anteStruct.tsoffaxis), 'Y'))
                {
                    tvdiscang = anteStruct.angleatv;
                }
                else
                {
                    tvdiscang = anteStruct.angleutv;
                }

                /* calc discrimination for angle UTV -> adiscUtv */
                if ((rc = TpGetDat.TpCalcDisc(anteStruct.terracode,
                                     Abs(tvdiscang), out adisccv, out adiscxv,
                                                         out adiscch, out adiscxh, out nullCv, out nullXv, out nullCh,
                                                         out nullXh, intPrintMsg, vicPrintMsg)) != Constant.SUCCESS)
                {
                    return (rc);
                }

                FindMin(adisccv, adiscxv, adiscch, adiscxh, (short)nullCv, (short)nullXv, (short)nullCh, (short)nullXh, ref adiscUtv);

                if (adiscUtv == Constant.DFLT_ADISCW)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.CALCDISC, "UTV");
                    return (Constant.FAILURE);
                }
            }


            /*-----------------------------*/
            /* set up the needed variables */

            HR = CalcHR(siteStruct.earthlatit / 100.0);

            TeVects.TeBuildVector(1.0, anteStruct.eselev, anteStruct.esazim, out esVec);
            TeVects.TeBuildVector(siteStruct.etdist, anteStruct.etelev, siteStruct.etazim,
                            out etVec);
            TeVects.TeBuildVector(siteStruct.eudist, anteStruct.euelev, siteStruct.euazim,
                            out euVec);

            TeVects.TeVectorSub(euVec, etVec, out tuVec);
            TeVects.TeVectorUnit(tuVec, out tu1Vec);


            /*------------------------*/
            /* Rain Cell on ES Vector */

            TeVects.TeVectorLinMux(etVec, tu1Vec, out ettu1);
            TeVects.TeVectorLinMux(esVec, etVec, out eset);
            TeVects.TeVectorLinMux(etVec, etVec, out et2);
            TeVects.TeVectorLinMux(esVec, tuVec, out estu);
            TeVects.TeVectorLinMux(esVec, tu1Vec, out estu1);
            cond = (et2 * estu1 - ettu1 * eset) / (estu1 * eset - ettu1);

            cond = (cond < 0.1) ? 0.1 : cond;

            TeVects.TeVectorSclMux(cond, esVec, out tmpVec);
            TeVects.TeVectorSub(tmpVec, etVec, out tvVec);
            TeVects.TeVectorLen(tvVec, out tvDist);
            TeVects.TeVectorLen(tuVec, out tuDist);

            TeVects.TeVectorLinMux(tvVec, tuVec, out tmpVal);
            alpha = AcosD(tmpVal / (tvDist * tuDist));

            /* check if local TS site is a passive reflector:
             * if call1 begins with '%'
             */
            if (Strings.FirstCharIs(anteStruct.terrcall1, '%'))
            {
                /* calculate terrestrials's passive reflector out  discrim */
                if (Strings.FirstCharIs(anteStruct.interferer, 'T'))
                {
                    rc = TtCalkPassive.TtCalcPassive(true, terrMDB, terrAnteName,
                                       anteStruct.terrcall1, anteStruct.terrcall2,
                                                         anteStruct.terrbndcde, anteStruct.terranum, terrMbnd,
                                                         alpha, out disc, out junk, intPrintMsg, vicPrintMsg);
                }
                else
                {
                    rc = TtCalkPassive.TtCalcPassive(false, terrMDB, terrAnteName,
                                       anteStruct.terrcall1, anteStruct.terrcall2,
                                                         anteStruct.terrbndcde, anteStruct.terranum, terrMbnd,
                                                         alpha, out disc, out junk, intPrintMsg, vicPrintMsg);
                }
                if (rc != Constant.SUCCESS)
                {
                    return (rc);
                }
                adiscAlpha = disc;
            }
            else
            {/* terrestrial site is not a passive repeater
		 * get antenna pattern and calculate antenna discriminations for
		 * the TS antenna	 */
             /* calc discrimination for angle Alpha -> adiscAlpha */
                if ((rc = TpGetDat.TpCalcDisc(anteStruct.terracode,
                                     Abs(alpha), out adisccv, out adiscxv, out adiscch, out adiscxh,
                                                         out nullCv, out nullXv, out nullCh, out nullXh, intPrintMsg,
                                                         vicPrintMsg)) != Constant.SUCCESS)
                {
                    return (rc);
                }

                FindMin(adisccv, adiscxv, adiscch, adiscxh, (short)nullCv, (short)nullXv, (short)nullCh, (short)nullXh, ref adiscAlpha);
                if (adiscAlpha == Constant.DFLT_ADISCW)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.CALCDISC, "Alpha");
                    return (Constant.FAILURE);
                }
            }

            /* check Max Rain Cell Ht or check for minimum distance condition loss*/
            if ((anteStruct.evdistes * SinD(anteStruct.eselev) > HR) ||
                ((20.0 * Log10(tvDist) + adiscAlpha) <
                     (20.0 * Log10(anteStruct.tvdistes) + adiscUtv)))
            {
                cond = HR / SinD(anteStruct.eselev);
                TeVects.TeVectorSclMux(cond, esVec, out tmpVec);
                TeVects.TeVectorSub(tmpVec, etVec, out tvVec);
                TeVects.TeVectorLen(tvVec, out tvDist);
                anteStruct.tvdistes = tvDist;
                TeVects.TeVectorLinMux(tvVec, tuVec, out tmpVal);
                alpha = AcosD(tmpVal / (tvDist * tuDist));

                /* check if local TS site is a passive reflector:
                 * if call1 begins with '%'
                 */
                if (Strings.FirstCharIs(anteStruct.terrcall1, '%'))
                {
                    /* calc terrestrials's passive reflector out  discrim */
                    if (Strings.FirstCharIs(anteStruct.interferer, 'T'))
                    {
                        rc = TtCalkPassive.TtCalcPassive(true, terrMDB, terrAnteName,
                                             anteStruct.terrcall1, anteStruct.terrcall2,
                                             anteStruct.terrbndcde, anteStruct.terranum,
                                                             terrMbnd, alpha, out disc, out junk, intPrintMsg,
                                                             vicPrintMsg);
                    }
                    else
                    {
                        rc = TtCalkPassive.TtCalcPassive(false, terrMDB, terrAnteName,
                                             anteStruct.terrcall1, anteStruct.terrcall2,
                                             anteStruct.terrbndcde, anteStruct.terranum,
                                                             terrMbnd, alpha, out disc, out junk, intPrintMsg,
                                                             vicPrintMsg);
                    }
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    adiscAlpha = disc;
                }
                else
                {/* terrestrial site is not a passive repeater
			 * get antenna pattern and calc antenna discrims for
			 * the TS antenna
			 */
                 /* calc discrimination for angle Alpha -> adiscAlpha */
                    if ((rc = TpGetDat.TpCalcDisc(anteStruct.terracode,
                                         Abs(alpha), out adisccv, out adiscxv, out adiscch, out adiscxh,
                                                             out nullCv, out nullXv, out nullCh, out nullXh, intPrintMsg,
                                                             vicPrintMsg)) != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    adiscAlpha = Constant.DFLT_ADISCW;

                    FindMin(adisccv, adiscxv, adiscch, adiscxh, (short)nullCv, (short)nullXv, (short)nullCh,
                        (short)nullXh, ref adiscAlpha);

                    if (adiscAlpha == Constant.DFLT_ADISCW)
                    {
                        ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                        ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                        ErrMsg.UtPrintMessage(Error.CALCDISC, "Alpha");
                        return (Constant.FAILURE);
                    }
                }

            }
            else
            {
                alpha = anteStruct.angleutv;
                adiscAlpha = adiscUtv;
            }


            /*------------------------*/
            /* Rain Cell on TU Vector */

            TeVects.TeVectorSclMux(-1.0, etVec, out teVec);

            TeVects.TeVectorLinMux(teVec, teVec, out te2);
            TeVects.TeVectorLinMux(tu1Vec, esVec, out tu1es);
            TeVects.TeVectorLinMux(teVec, esVec, out tees);
            TeVects.TeVectorLinMux(tu1Vec, teVec, out tu1te);
            TeVects.TeVectorLinMux(tuVec, tuVec, out tu2);
            TeVects.TeVectorLinMux(tuVec, teVec, out tute);
            cond = (te2 * tu1es - tees * tu1te) / (tu1es * tu1te - tees);

            cond = (cond < 0.1) ? 0.1 : cond;

            TeVects.TeVectorSclMux(cond, tu1Vec, out tmpVec);
            TeVects.TeVectorSub(tmpVec, teVec, out evVec);
            TeVects.TeVectorLen(evVec, out evDist);

            beta = AcosD((cond * tu1es - tees) /
                     (Pow(cond, Constant.SQUARE) * tu2 + te2 - 2.0 * cond * tute));

            /* calc discrimination for angle Beta -> adiscBeta */
            if (!anteStruct.earthacode.StartsWith("CCIR"))
            {
                if ((rc = TpGetDat.TpCalcDisc(anteStruct.earthacode,
                                     Abs(beta), out adisccv, out adiscxv, out adiscch, out adiscxh,
                                                         out nullCv, out nullXv, out nullCh, out nullXh, intPrintMsg,
                                                         vicPrintMsg)) != Constant.SUCCESS)
                {
                    return (rc);
                }
                FindMin(adisccv, adiscxv, adiscch, adiscxh, (short)nullCv, (short)nullXv, (short)nullCh, (short)nullXh, ref adiscBeta);
                if (adiscBeta == Constant.DFLT_ADISCW)
                {
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                    ErrMsg.UtPrintMessage(Error.CALCDISC, "Beta");
                    return (Constant.FAILURE);
                }
            }
            else
            {
                if (Abs(beta) < 1.0)
                {
                    adiscBeta = 0.0;
                }
                else if (Abs(beta) >= 48.0)
                {
                    adiscBeta = (double)tmpGain + 10.0;
                }
                else
                {
                    adiscBeta = (double)tmpGain - (32.0 - 25.0 * Log10(beta));
                }
            }

            /* check Max Rain Cell Ht */
            /* check for minimum distance condition loss */
            if ((anteStruct.tvdistes * SinD(anteStruct.tuelev) > HR) ||
                ((20.0 * Log10(evDist) + adiscBeta) <
                     (20.0 * Log10(anteStruct.evdisttu) + adiscSev)))
            {
                cond = HR / SinD(anteStruct.tuelev);
                TeVects.TeVectorSclMux(cond, tu1Vec, out tmpVec);
                TeVects.TeVectorSub(tmpVec, teVec, out evVec);
                TeVects.TeVectorLen(evVec, out evDist);
                anteStruct.evdisttu = evDist;
                beta = AcosD((cond * tu1es - tees) /
                         (Pow(cond, Constant.SQUARE) * tu2 + te2 - 2.0 * cond * tute));

                /* calc discrimination for angle Beta -> adiscBeta */
                if (!anteStruct.earthacode.StartsWith("CCIR"))
                {
                    if ((rc = TpGetDat.TpCalcDisc(anteStruct.earthacode,
                                         Abs(beta), out adisccv, out adiscxv, out adiscch, out adiscxh,
                                                             out nullCv, out nullXv, out nullCh, out nullXh, intPrintMsg,
                                                             vicPrintMsg)) != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    adiscBeta = Constant.DFLT_ADISCW;
                    FindMin(adisccv, adiscxv, adiscch, adiscxh, (short)nullCv, (short)nullXv, (short)nullCh, (short)nullXh, ref adiscBeta);
                    if (adiscBeta == Constant.DFLT_ADISCW)
                    {
                        ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                        ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                        ErrMsg.UtPrintMessage(Error.CALCDISC, "Beta");
                        return (Constant.FAILURE);
                    }
                }
                else
                {
                    if (Abs(beta) < 1.0)
                    {
                        adiscBeta = 0.0;
                    }
                    else if (Abs(beta) >= 48.0)
                    {
                        adiscBeta = (double)tmpGain + 10.0;
                    }
                    else
                    {
                        adiscBeta = (double)tmpGain - (32.0 - 25.0 * Log10(beta));
                    }
                }

            }
            else
            {
                beta = anteStruct.anglesev;
                adiscBeta = adiscSev;
            }

            /*-----------------------------------------------*/
            /* Calculation of Rain Scatter Transmission Loss */

            /* values stored in KHz, ProvValues uses MHz */
            if (TpSub.TePropValues(siteStruct.rainzone, chanStruct.intfreqtx / 1000.0,
                                 chanStruct.intfreqtx / 1000.0, out R, out abtx, out lrtx) != 0)
            {
                /*	The rain zone was invalid and has been set to 9 */
                TpRunTsip.mTW_ERR.Write("\n*WARNING* Rain Zone {0} is invalid: {1},{2}. Using 9.",
                                siteStruct.rainzone,
                                siteStruct.terrcall1,
                                siteStruct.earthlocation);
            }
            /*  No need to print an error here, as it will already have been printed */
            TpSub.TePropValues(siteStruct.rainzone, chanStruct.intfreqtx / 1000.0,
                         chanStruct.vicfreqrx / 1000.0, out R, out abrx, out lrrx);

            /* calculations use freq in GHz */
            if (chanStruct.intfreqtx > 0)
            {
                kltx = 168.0 - 20.0 * Log10(chanStruct.intfreqtx / 1000000.0) - 13.2 * Log10(R) +
                       lrtx + abtx;
            }
            else
            {
                /* intfreqtx is null or an invalid value */
                kltx = 9999999;     /* max out kltx */
            }

            if (chanStruct.vicfreqrx > 0)
            {
                klrx = 168.0 - 20.0 * Log10(chanStruct.vicfreqrx / 1000000.0) - 13.2 * Log10(R) +
                       lrrx + abrx;
            }
            else
            {
                /* vicfreqrx is null or an invalid value */
                klrx = 9999999;     /* max out klrx */
            }
            kltx = (klrx < kltx) ? klrx : kltx;

            /*--------------------------------------------------------*/
            /* calculate transmission loss for scattering propagation */

            if (nullEarthAfslr == Constant.DB_NULL)
            {
                earthAfslr = 0.0;
            }

            if (nullEarthAfslt == Constant.DB_NULL)
            {
                earthAfslt = 0.0;
            }

            if (Strings.FirstCharIs(chanStruct.interferer, 'T') || Strings.FirstCharIs(chanStruct.interferer, 'E'))
            {
                if (Strings.FirstCharIs(chanStruct.interferer, 'T'))
                {
                    lossVolAlpha = kltx + 20.0 * Log10(anteStruct.tvdistes) -
                                     (terrAGain - adiscAlpha) + earthAfslr + terrAfsl;
                    lossVolBeta = kltx + 20.0 * Log10(anteStruct.evdisttu) -
                                    (earthAGain - adiscBeta) + earthAfslr + terrAfsl;
                }
                else
                {
                    lossVolAlpha = kltx + 20.0 * Log10(anteStruct.tvdistes) -
                                     (terrAGain - adiscAlpha) + earthAfslt + terrAfsl;
                    lossVolBeta = kltx + 20.0 * Log10(anteStruct.evdisttu) -
                                    (earthAGain - adiscBeta) + earthAfslt + terrAfsl;
                }

                if (lossVolAlpha < lossVolBeta)
                {
                    loss01mode2 = lossVolAlpha;
                    chanStruct.scang = alpha;
                }
                else
                {
                    loss01mode2 = lossVolBeta;
                    chanStruct.scang = beta;
                }
                chanNulls[Constant.TE_CHAN_SCANG] = Constant.DB_NOT_NULL;
                nullL01M2 = Constant.DB_NOT_NULL;
            }
            else
            {
                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                ErrMsg.UtPrintMessage(Error.GENERROR,
                               "\tSCANG Calculations inconclusive for Rain Vol placement");
                chanNulls[Constant.TE_CHAN_SCANG] = Constant.DB_NULL;
                nullL01M2 = Constant.DB_NULL;
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Calculates the minimum of the four antenna discriminations 
        /// passed to it (note nulls for the discriminations are also checked) the 
        /// adiscSev parameter must be initialized to something acceptable.  
        /// </summary>
        /// <param name="adisccv"></param>
        /// <param name="adiscxv"></param>
        /// <param name="adiscch"></param>
        /// <param name="adiscxh"></param>
        /// <param name="nullCv"></param>
        /// <param name="nullXv"></param>
        /// <param name="nullCh"></param>
        /// <param name="nullXh"></param>
        /// <param name="adiscSev"></param>
        public static void FindMin(double adisccv,
                                        double adiscxv,
                                        double adiscch,
                                        double adiscxh,
                                        short nullCv,
                                        short nullXv,
                                        short nullCh,
                                        short nullXh,
                                        ref double adiscSev)
        {
            if (nullCv != Constant.DB_NULL)
            {
                adiscSev = (adiscSev < adisccv) ? adiscSev : adisccv;
            }
            if (nullXv != Constant.DB_NULL)
            {
                adiscSev = (adiscSev < adiscxv) ? adiscSev : adiscxv;
            }
            if (nullCh != Constant.DB_NULL)
            {
                adiscSev = (adiscSev < adiscch) ? adiscSev : adiscch;
            }
            if (nullXh != Constant.DB_NULL)
            {
                adiscSev = (adiscSev < adiscxh) ? adiscSev : adiscxh;
            }
        }





    }
}
