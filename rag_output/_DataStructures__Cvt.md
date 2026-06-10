# Documented File: Cvt.cs
**Repository Path:** `_DataStructures\Cvt.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates the tetrad of table type, table ID, table prefix and table
    /// suffix.
    /// </summary>
    public class Cvt
    {
        private static List<Cvt> cvtList;

        public int tabType;        // 	coded table type (see tabdefs.h) 
        public string tabID;       //	The	character rendition of the above. 
        public string prefix;      // Prefix for internal table name 
        public string suffix;      // Suffix for internal table name 

        // pgmNames usage:
        //                  0th element = edit routine pgm name
        //                  1   element = validate pgm name
        //                  2   element = print pgn name
        //                  3   element = import pgn name
        //                  4   element = update pgm name
        //                  5   element = GO/BATCH pgm name
        public string[] pgmNames;

        // 
        /// <summary>
        /// The static constructor to creates and defines the entire Cvt list.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static Cvt()
        {
            ConstructCvtList();
        }

        /// <summary>
        /// Private default constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private Cvt()
        {
            // Note:  all members and elements of type 'string' initially default to null.
            tabType = 0;
            pgmNames = new string[Constant.MAX_PGM_NAMES];
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private Cvt(int tabType, string tabID, string prefix, string suffix)
        {
            this.tabType = tabType;
            this.tabID = tabID;
            this.prefix = prefix;
            this.suffix = suffix;
            // Note:  all elements of type 'string' initially default to null.
            pgmNames = new string[Constant.MAX_PGM_NAMES];
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private Cvt(int tabType, string tabID, string prefix, string suffix, string[] pgmNames)
        {
            this.tabType = tabType;
            this.tabID = tabID;
            this.prefix = prefix;
            this.suffix = suffix;
            this.pgmNames = new string[Constant.MAX_PGM_NAMES] { pgmNames[0], pgmNames[1], pgmNames[2], pgmNames[3], pgmNames[4], pgmNames[5] };
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private Cvt(int tabType, string tabID, string prefix, string suffix, string pgm0, string pgm1, string pgm2, string pgm3, string pgm4, string pgm5)
        {
            this.tabType = tabType;
            this.tabID = tabID;
            this.prefix = prefix;
            this.suffix = suffix;
            this.pgmNames = new string[Constant.MAX_PGM_NAMES] { pgm0, pgm1, pgm2, pgm3, pgm4, pgm5 };
        }

        /// <summary>
        /// This method creates and populates the entire Ctv list.
        /// </summary>
        /// <param name=""></param>
        private static void ConstructCvtList()
        {
            // The CVT object constructor processes the following parameters with semantics
            // as described below.
            //
            // 0th parameter = coded table type (see the class _Configuration.Constant).
            // 1st parameter = text rendition of the above. 
            // 2nd parameter = prefix for internal table name.
            // 3rd parameter = suffix for internal table name 
            // 4th parameter = edit routine pgm name.
            // 5th parameter = validate pgm name.
            // 6th parameter = print pgn name.
            // 7th parameter = import pgn name.
            // 8th parameter = update pgm name.
            // 9th parameter = GO/BATCH pgm name.

            cvtList = new List<Cvt>();

            cvtList.Add(new Cvt(Constant.FT_TITL, "FT_TITL", "ft_", "_titl", "ftEdit", "ftValidate", "ftPrint", "ftImport", "mtUpdate", null));
            cvtList.Add(new Cvt(Constant.FT_SHRL, "FT_SHRL", "ft_", "_shrl"));
            cvtList.Add(new Cvt(Constant.FT_SITE, "FT_SITE", "ft_", "_site"));
            cvtList.Add(new Cvt(Constant.FT_ANTE, "FT_ANTE", "ft_", "_ante"));
            cvtList.Add(new Cvt(Constant.FT_CHAN, "FT_CHAN", "ft_", "_chan"));
            cvtList.Add(new Cvt(Constant.FT_CHNG_CALL, "FT_CHNG_CALL", "ft_", "_chng"));

            // Because the two character names are set to the 'title' names, the
            // only time these will be encountered is when looking up by tabID ("FT").
            // These are necessary for the tabID lookup in dblogger's routines
            cvtList.Add(new Cvt(Constant.FT, "FT", null, null));

            // Tables used in the Area Coordination studies
            cvtList.Add(new Cvt(Constant.AC_PARM, "AC_PARM", "ft_", "_acpa"));
            cvtList.Add(new Cvt(Constant.AC_PERI, "AC_PERI", "ft_", "_acpe"));
            cvtList.Add(new Cvt(Constant.AC_RADI, "AC_RADI", "ft_", "_acra"));
            cvtList.Add(new Cvt(Constant.AC, "AC", null, null));
            cvtList.Add(new Cvt(Constant.FE_TITL, "FE_TITL", "fe_", "_titl", "feEdit", "feValidate", "esPrint", "feImport", "meUpdate", null));
            cvtList.Add(new Cvt(Constant.FE_SHRL, "FE_SHRL", "fe_", "_shrl"));
            cvtList.Add(new Cvt(Constant.FE_SITE, "FE_SITE", "fe_", "_site"));
            cvtList.Add(new Cvt(Constant.FE_AZIM, "FE_AZIM", "fe_", "_azim"));
            cvtList.Add(new Cvt(Constant.FE_ANTE, "FE_ANTE", "fe_", "_ante"));
            cvtList.Add(new Cvt(Constant.FE_CHAN, "FE_CHAN", "fe_", "_chan"));
            cvtList.Add(new Cvt(Constant.FE_CLOC, "FE_CLOC", "fe_", "_cloc"));
            cvtList.Add(new Cvt(Constant.FE_CCAL, "FE_CCAL", "fe_", "_ccal"));
            cvtList.Add(new Cvt(Constant.FE, "FE", null, null));

            // Create PDF from the MDB - temp tables - TS
            cvtList.Add(new Cvt(Constant.CT_SITE, "CT_SITE", "ct_", "_site"));
            cvtList.Add(new Cvt(Constant.CT_ANTE, "CT_ANTE", "ct_", "_ante"));
            cvtList.Add(new Cvt(Constant.CT_CHAN, "CT_CHAN", "ct_", "_chan"));
            cvtList.Add(new Cvt(Constant.CT_TEMP, "CT_TEMP", "ct_", "_temp"));
            cvtList.Add(new Cvt(Constant.CT_RSLT, "CT_RSLT", "ct_", "_rslt"));
            cvtList.Add(new Cvt(Constant.CT, "CT", null, null));

            // Create PDF from the MDB - temp tables - ES
            cvtList.Add(new Cvt(Constant.CE_SITE, "CE_SITE", "ce_", "_site"));
            cvtList.Add(new Cvt(Constant.CE_ANTE, "CE_ANTE", "ce_", "_ante"));
            cvtList.Add(new Cvt(Constant.CE_CHAN, "CE_CHAN", "ce_", "_chan"));
            cvtList.Add(new Cvt(Constant.CE_RSLT, "CE_RSLT", "ce_", "_rslt"));
            cvtList.Add(new Cvt(Constant.CE, "CE", null, null));

            // PDF (ES and TS) Tables

            // For the program that performs the cull - using update pgm name
            cvtList.Add(new Cvt(Constant.FW_CULL, "FW_CULL", "fw_", "_cull", "fwCullEdit", null, null, null, null, "fwCullCreate"));
            cvtList.Add(new Cvt(Constant.RM_PARAM, "RM_PARAM", "rm_", "_parm", "rmEdit", null, null, null, null, "rmReport"));
            cvtList.Add(new Cvt(Constant.RS_PARAM, "RS_PARAM", "rs_", "_parm", "rsEdit", null, null, null, null, "rsReport"));
            cvtList.Add(new Cvt(Constant.SU_ANTE, "SU_ANTE", "su_", "_ante", "editAnte", "validAnte", "printAnte", "importAnte", "updateAnte", null));
            cvtList.Add(new Cvt(Constant.SU_ANTD, "SU_ANTD", "su_", "_antd"));
            cvtList.Add(new Cvt(Constant.SU_BAND, "SU_BAND", "su_", "_band", "editBand", "validBand", "printBand", "importBand", "updateBand", null));
            cvtList.Add(new Cvt(Constant.SU_CTX, "SU_CTX", "su_", "_ctx_", "editCtx", "validCtx", "printCtx", "importCtx", "updateCtx", null));
            cvtList.Add(new Cvt(Constant.SU_CTXD, "SU_CTXD", "su_", "_ctxd"));
            cvtList.Add(new Cvt(Constant.SU_EQPT, "SU_EQPT", "su_", "_eqpt", "editEqpt", "validEqpt", "printEqpt", "importEqpt", "updateEqpt", null));
            cvtList.Add(new Cvt(Constant.SU_NOTE, "SU_NOTE", "su_", "_note", "editNote", "validNote", "printNote", "importNote", "updateNote", null));
            cvtList.Add(new Cvt(Constant.SU_OPER, "SU_OPER", "su_", "_oper", "editOper", "validOper", "printOper", "importOper", "updateOper", null));
            cvtList.Add(new Cvt(Constant.SU_PLAN, "SU_PLAN", "su_", "_plan", "editPlan", "validPlan", "printPlan", "importPlan", "updatePlan", null));
            cvtList.Add(new Cvt(Constant.SU_PLND, "SU_PLND", "su_", "_plnd"));
            cvtList.Add(new Cvt(Constant.SU_ROUT, "SU_ROUT", "su_", "_rout", "editRout", "validRout", "printRout", "importRout", "updateRout", null));
            cvtList.Add(new Cvt(Constant.SU_TOWR, "SU_TOWR", "su_", "_towr", "editTowr", "validTowr", "printTowr", "importTowr", "updateTowr", null));
            cvtList.Add(new Cvt(Constant.SU_TOWN, "SU_TOWN", "su_", "_town", "editTown", "validTown", "printTown", "importTown", "updateTown", null));
            cvtList.Add(new Cvt(Constant.SU_TRAF, "SU_TRAF", "su_", "_traf", "editTraf", "validTraf", "printTraf", "importTraf", "updateTraf", null));
            cvtList.Add(new Cvt(Constant.SU_OCOO, "SU_OCOO", "su_", "_ocoo", "editOcoo", "validOcoo", "printOcoo", "importOcoo", "updateOcoo", null));
            cvtList.Add(new Cvt(Constant.SC_ANTE, "SC_ANTE", "sc_", "_ante", "scEditAnte", null, null, null, null, "scCullAnte"));
            cvtList.Add(new Cvt(Constant.SC_BAND, "SC_BAND", "sc_", "_band", "scEditBand", null, null, null, null, "scCullBand"));
            cvtList.Add(new Cvt(Constant.SC_CTX, "SC_CTX", "sc_", "_ctx_", "scEditCtx", null, null, null, null, "scCullCtx"));
            cvtList.Add(new Cvt(Constant.SC_EQPT, "SC_EQPT", "sc_", "_eqpt", "scEditEqpt", null, null, null, null, "scCullEqpt"));
            cvtList.Add(new Cvt(Constant.SC_NOTE, "SC_NOTE", "sc_", "_note", "scEditNote", null, null, null, null, "scCullNote"));
            cvtList.Add(new Cvt(Constant.SC_OPER, "SC_OPER", "sc_", "_oper", "scEditOper", null, null, null, null, "scCullOper"));
            cvtList.Add(new Cvt(Constant.SC_PLAN, "SC_PLAN", "sc_", "_plan", "scEditPlan", null, null, null, null, "scCullPlan"));
            cvtList.Add(new Cvt(Constant.SC_ROUT, "SC_ROUT", "sc_", "_rout", "scEditRout", null, null, null, null, "scCullRout"));
            cvtList.Add(new Cvt(Constant.SC_TOWR, "SC_TOWR", "sc_", "_towr", "scEditTowr", null, null, null, null, "scCullTowr"));
            cvtList.Add(new Cvt(Constant.SC_TOWN, "SC_TOWN", "sc_", "_town", "scEditTown", null, null, null, null, "scCullTown"));
            cvtList.Add(new Cvt(Constant.SC_TRAF, "SC_TRAF", "sc_", "_traf", "scEditTraf", null, null, null, null, "scCullTraf"));
            cvtList.Add(new Cvt(Constant.SC_OCOO, "SC_OCOO", "sc_", "_ocoo", "scEditOcoo", null, null, null, null, "scCullOcoo"));

            // TSIP TS SH Tables
            cvtList.Add(new Cvt(Constant.TT_PARM, "TT_PARM", "tt_", "_parm", null, null, null, null, null, "rtTTReport"));
            cvtList.Add(new Cvt(Constant.TT_SITE, "TT_SITE", "tt_", "_site"));
            cvtList.Add(new Cvt(Constant.TT_ANTE, "TT_ANTE", "tt_", "_ante"));
            cvtList.Add(new Cvt(Constant.TT_CHAN, "TT_CHAN", "tt_", "_chan"));
            cvtList.Add(new Cvt(Constant.TT, "TT", null, null));

            // TSIP ES SH Tables
            cvtList.Add(new Cvt(Constant.TE_PARM, "TE_PARM", "te_", "_parm", null, null, null, null, null, "rtTEReport"));
            cvtList.Add(new Cvt(Constant.TE_SITE, "TE_SITE", "te_", "_site"));
            cvtList.Add(new Cvt(Constant.TE_ANTE, "TE_ANTE", "te_", "_ante"));
            cvtList.Add(new Cvt(Constant.TE_CHAN, "TE_CHAN", "te_", "_chan"));
            cvtList.Add(new Cvt(Constant.TE, "TE", null, null));

            // TSIP  Temporary tables
            cvtList.Add(new Cvt(Constant.TP_SU_ANTE, "TP_SU_ANTE", "tp_", "_tant", "editTmpAnte", "validTmpAnte", "printTmpAnte", "importTmpAnte", null, null));
            cvtList.Add(new Cvt(Constant.TP_SU_ANTD, "TP_SU_ANTD", "tp_", "_tand"));
            cvtList.Add(new Cvt(Constant.TP_SU_CTX, "TP_SU_CTX", "tp_", "_tctx", "editTmpCtx", "validTmpCtx", "printTmpCtx", "importTmpCtx", null, null));
            cvtList.Add(new Cvt(Constant.TP_SU_CTXD, "TP_SU_CTXD", "tp_", "_tctd"));
            cvtList.Add(new Cvt(Constant.TP_SU_PLAN, "TP_SU_PLAN", "tp_", "_tpln", "editTmpPlan", "validTmpPlan", "printTmpPlan", "importTmpPlan", null, null));
            cvtList.Add(new Cvt(Constant.TP_SU_PLND, "TP_SU_PLND", "tp_", "_tpld"));
            cvtList.Add(new Cvt(Constant.TP_SU_EQPT, "TP_SU_EQPT", "tp_", "_teqp", "editTmpEqpt", "validTmpEqpt", "printTmpEqpt", "importTmpEqpt", null, null));
            cvtList.Add(new Cvt(Constant.TP_PARM, "TP_PARM", "tp_", "_parm", "tpEditParam", null, "tpPrintParam", null, null, "tpRunTsip"));
            cvtList.Add(new Cvt(Constant.PP_PARM, "PP_PARM", "pp_", "_parm", "tpEditParam", null, "tpPrintParam", null, null, "tpRunTsip"));
            cvtList.Add(new Cvt(Constant.TT_TEMP1, "TT_TEMP1", "tt_", "_tmp1"));
            cvtList.Add(new Cvt(Constant.TT_TEMP2, "TT_TEMP2", "tt_", "_tmp2"));
            cvtList.Add(new Cvt(Constant.TE_TEMP1, "TE_TEMP1", "te_", "_tmp1"));

            // Usage reporting temporary table
            cvtList.Add(new Cvt(Constant.BI_USAGE, "BI_USAGE", "bi_", "_summ", null, null, null, null, null, "rpUsage"));

            // Security reporting tables
            cvtList.Add(new Cvt(Constant.SE_PERM_REQ, "SE_PERM_REQ", "se_", "_req", null, null, null, null, null, "sePermRep"));
            cvtList.Add(new Cvt(Constant.SE_PERM_USER, "SE_PERM_USER", "se_", "_user", null, null, null, null, null, "sePermRep"));
            cvtList.Add(new Cvt(Constant.RP_STORAGE, "RP_STORAGE", "rp_", "_storage", null, null, null, null, null, "rpStorage"));
            cvtList.Add(new Cvt(Constant.RP_TS_FEE_DETAIL, "RP_TS_FEE_DETAIL", "rp_", "_det", null, null, null, null, null, "rpTsFee"));
            cvtList.Add(new Cvt(Constant.RP_ES_FEE_DETAIL, "RP_ES_FEE_DETAIL", "rp_", "_det", null, null, null, null, null, "rpEsFee"));
            cvtList.Add(new Cvt(Constant.RP_TS_FEE_SUMMARY, "RP_TS_FEE_SUMMARY", null, null, null, null, null, null, null, "report"));
            cvtList.Add(new Cvt(Constant.RP_ES_FEE_SUMMARY, "RP_ES_FEE_SUMMARY", null, null, null, null, null, null, null, "report"));
            cvtList.Add(new Cvt(Constant.RP_GRAND_SUM_MTD, "RP_GRAND_SUM_MTD", null, null, null, null, null, null, null, "report"));
            cvtList.Add(new Cvt(Constant.RP_GRAND_SUM_YTD, "RP_GRAND_SUM_YTD", null, null, null, null, null, null, null, "report"));
            cvtList.Add(new Cvt(Constant.RP_F_FACTOR_MTD, "RP_F_FACTOR_MTD", null, null, null, null, null, null, null, "report"));
            cvtList.Add(new Cvt(Constant.RP_F_FACTOR_YTD, "RP_F_FACTOR_YTD", null, null, null, null, null, null, null, "report"));

            // billing tables
            cvtList.Add(new Cvt(Constant.BI_MICS_MONTH, "BI_MICS_MONTH", "bi_mics_", "_month"));
            cvtList.Add(new Cvt(Constant.BI_ULTRIX_MONTH, "BI_ULTRIX_MONTH", "bi_ultrix_", "_month"));
            cvtList.Add(new Cvt(Constant.BI_MICS_YEAR, "BI_MICS_YEAR", "bi_mics_", "_year"));
            cvtList.Add(new Cvt(Constant.BI_ULTRIX_YEAR, "BI_ULTRIX_YEAR", "bi_ultrix_", "_year"));

            // Billing reports
            cvtList.Add(new Cvt(Constant.BI_MTD_USAGE, "BI_MTD_USAGE", null, null, null, null, null, null, null, "rbMonthUsage"));
            cvtList.Add(new Cvt(Constant.BI_MTD_COST, "BI_MTD_COST", null, null, null, null, null, null, null, "rbMonthCost"));
            cvtList.Add(new Cvt(Constant.BI_MTD_USAGE_SUM, "BI_MTD_USAGE_SUM", null, null, null, null, null, null, null, "rbMonthSumUsage"));
            cvtList.Add(new Cvt(Constant.BI_MTD_COST_SUM, "BI_MTD_COST_SUM", null, null, null, null, null, null, null, "rbMonthSumCost"));
            cvtList.Add(new Cvt(Constant.BI_YTD_USAGE, "BI_YTD_USAGE", null, null, null, null, null, null, null, "rbYearUsage"));
            cvtList.Add(new Cvt(Constant.BI_YTD_COST, "BI_YTD_COST", null, null, null, null, null, null, null, "rbYearCost"));
            cvtList.Add(new Cvt(Constant.BI_YTD_USAGE_SUM, "BI_YTD_USAGE_SUM", null, null, null, null, null, null, null, "rbYearSumUsage"));
            cvtList.Add(new Cvt(Constant.BI_YTD_COST_SUM, "BI_YTD_COST_SUM", null, null, null, null, null, null, null, "rbYearSumCost"));
            cvtList.Add(new Cvt(Constant.BI_USAGE_RATES, "BI_USAGE_RATES", null, null, null, null, null, null, null, "rpBilRep"));
            cvtList.Add(new Cvt(Constant.BI_USAGE_CU_MBM, "BI_USAGE_CU_MBM", null, null, null, null, null, null, null, "rpUsage"));
            cvtList.Add(new Cvt(Constant.BI_USAGE_CU_MAV, "BI_USAGE_CU_MAV", null, null, null, null, null, null, null, "rpUsage"));
            cvtList.Add(new Cvt(Constant.BI_USAGE_CD_AVG, "BI_USAGE_CD_AVG", null, null, null, null, null, null, null, "rpUsage"));

            // Main Report Temporary tables
            cvtList.Add(new Cvt(Constant.TS_ALPHA_SITE_TABLE, "TS_ALPHA_SITE_TABLE", "ts_", "_alst"));
            cvtList.Add(new Cvt(Constant.TS_ALPHA_ANTE_TABLE, "TS_ALPHA_ANTE_TABLE", "ts_", "_alat"));
            cvtList.Add(new Cvt(Constant.TS_ALPHA_CHAN_TABLE, "TS_ALPHA_CHAN_TABLE", "ts_", "_alct"));
            cvtList.Add(new Cvt(Constant.TS_ALPHA_TOWN_TABLE, "TS_ALPHA_TOWN_TABLE", "ts_", "_altn"));
            cvtList.Add(new Cvt(Constant.ES_ALPHA_SITE_TABLE, "ES_ALPHA_SITE_TABLE", "es_", "_alst"));
            cvtList.Add(new Cvt(Constant.ES_ALPHA_ANTE_TABLE, "ES_ALPHA_ANTE_TABLE", "es_", "_alat"));
            cvtList.Add(new Cvt(Constant.ES_ALPHA_CHAN_TABLE, "ES_ALPHA_CHAN_TABLE", "es_", "_alct"));

            // PLOT TYPES - and plot program names
            cvtList.Add(new Cvt(Constant.ITU_PLOT, "ITU_PLOT", null, null, null, null, null, null, null, "ituPlot"));
            cvtList.Add(new Cvt(Constant.CTX_PLOT, "CTX_PLOT", null, null, null, null, null, null, null, "ctxPlot"));
            cvtList.Add(new Cvt(Constant.ANTE_PLOT, "ANTE_PLOT", null, null, null, null, null, null, null, "antePlot"));
            cvtList.Add(new Cvt(Constant.HORIZON_PLOT, "HORIZON_PLOT", null, null, null, null, null, null, null, "horizonPlot"));
            cvtList.Add(new Cvt(Constant.RP_USERDEF, "RP_USERDEF", null, null, null, null, null, null, null, "report"));
            cvtList.Add(new Cvt(0, "0", null, null));

        }

        /// <summary>
        /// This method searches the conversion table for a prescribed table type
        /// and, if found, returns a deep-clone of the associated Cvt object. 
        /// </summary>
        /// <param name="tabType"></param>
        /// <returns></returns>
        public static Cvt SearchCvtTab(int tabType)
        {
            Cvt foundCvt = null;
            foreach (Cvt cvt in cvtList)
            {
                if (cvt.tabType == tabType)
                {
                    foundCvt = cvt.DeepClone();
                    break;
                }
            }
            return (foundCvt);
        }

        /// <summary>
        /// This method makes a deep-clone of this Cvt object.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private Cvt DeepClone()
        {
            return new Cvt(tabType, tabID, prefix, suffix, pgmNames);
        }



    }
}



```
