using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using _Configuration;
using _NewLib;

namespace _Utillib
{
    public class GetCtx
    {
#if PINVOKE

        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int getctxreq([In] char cIntType,
                                        [In] char cVicType,
                                        [In] TcTxAnalog ptVicAnalog,
                                        [In] TcTxDigital ptVicDigital,
                                        [In] TcTxAnalog ptIntAnalog,
                                        [In] TcTxDigital ptIntDigital,
                                        [In] double dFS,
                                        [In] int IsEs,
                                        [In, Out] ref char cTypeOfInt,
                                        [In, Out] ref double dRequired);

        public static int GetCtxReq_NATIVE(char cIntType,
                                            char cVicType,
                                            TcTxAnalog ptVicAnalog,     /*	Equipments. Only two will */
                                            TcTxDigital ptVicDigital,   /*	be used. */
                                            TcTxAnalog ptIntAnalog,
                                            TcTxDigital ptIntDigital,
                                            double dFS,
                                            bool IsEs,
                                            out char cTypeOfInt,
                                            out double dRequired)
        {
            // 'out' requirement.
            cTypeOfInt = '!';
            dRequired = 0.0;

            int IsEsInt = Constant.FALSE;
            if (IsEs) IsEsInt = Constant.TRUE;

            return getctxreq(cIntType, cVicType, ptVicAnalog, ptVicDigital, ptIntAnalog, ptIntDigital,
                                dFS, IsEsInt, ref cTypeOfInt, ref dRequired);
        }

        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int getctxeqpt([In] string cRxTraf,
                                                [In] string cRxEqpt,
                                                [In] string cTxTraf,
                                                [In] string cTxEqpt,
                                                [In, Out] ref char cVicType,
                                                [In, Out] ref char cIntType,
                                                [In, Out] TcTxAnalog tVicAnalog,
                                                [In, Out] TcTxDigital tVicDigital,
                                                [In, Out] TcTxAnalog tIntAnalog,
                                                [In, Out] TcTxDigital tIntDigital);

        public static int GetCtxEqpt_NATIVE(string cRxTraf,
                                            string cRxEqpt,
                                            string cTxTraf,
                                            string cTxEqpt,
                                            out char cVicType, /* Either 'A' or 'D' Victim */
                                            out char cIntType, /* Either 'A' or 'D' Interferor */
                                            out TcTxAnalog tVicAnalog, /* Equipments.  Only two will be */
                                            out TcTxDigital tVicDigital,  /* used. */
                                            out TcTxAnalog tIntAnalog,
                                            out TcTxDigital tIntDigital)
        {
            // 'out' requirements.
            cVicType = '!';
            cIntType = '!';
            tVicAnalog = new TcTxAnalog();
            tVicDigital = new TcTxDigital();
            tIntAnalog = new TcTxAnalog();
            tIntDigital = new TcTxDigital();

            return getctxeqpt(cRxTraf, cRxEqpt, cTxTraf, cTxEqpt, ref cVicType, ref cIntType,
                                tVicAnalog, tVicDigital, tIntAnalog, tIntDigital);
        }
#endif

        /// <summary>
        /// Detect whether or not an equipment structure is the result of a cross 
        /// reference. We simply compare the calling traffic/equipment with the 
        /// resulting structure, taking into account the type of structure.  
        /// </summary>
        /// <param name="cTraf"></param>
        /// <param name="cEqpt"></param>
        /// <param name="cIntType"></param>
        /// <param name="ptAnalog"></param>
        /// <param name="ptDigital"></param>
        /// <param name="cxTraf"></param>
        /// <param name="cxEqpt"></param>
        /// <returns></returns>
        public static int XrefCodes(string cTraf,
                                    string cEqpt,
                                    char cIntType,
                                    TcTxAnalog ptAnalog,
                                    TcTxDigital ptDigital,
                                    out string cxTraf,
                                    out string cxEqpt)
        {
            // 'out' requirements.
            cxTraf = "";
            cxEqpt = "";

            int nRet;

            switch (cIntType)
            {
                case 'A':
                    if (!cTraf.Equals(ptAnalog.trafcode))
                    {
                        nRet = 1;
                        cxTraf = ptAnalog.trafcode;
                        cxEqpt = ptAnalog.ecode;
                    }
                    else
                    {
                        nRet = 0;
                        cxTraf = cTraf;
                        cxEqpt = cEqpt;
                    }
                    break;

                case 'D':
                    if (!cTraf.Equals(ptDigital.trafcode))
                    {
                        nRet = 1;
                        cxTraf = ptDigital.trafcode;
                        cxEqpt = ptDigital.ecode;
                    }
                    else
                    {
                        nRet = 0;
                        cxTraf = cTraf;
                        cxEqpt = cEqpt;
                    }
                    break;

                default:
                    GenUtil.SetError(1004, "Type neither A nor D.");
                    nRet = -1;
                    break;
            }

            return (nRet);
        }

        /// <summary>
        /// Get the equipment structures that will be used in the ctx calculations. 
        /// This means resolving which are analog, and which are digital. The actual 
        /// cross-referencing is done in the equipment retrieval routines.  
        /// </summary>
        /// <param name="cRxTraf"></param>
        /// <param name="cRxEqpt"></param>
        /// <param name="cTxTraf"></param>
        /// <param name="cTxEqpt"></param>
        /// <param name="cVicType"> - Either 'A' or 'D' Victim</param>
        /// <param name="cIntType"> - Either 'A' or 'D' Interferor</param>
        /// <param name="tVicAnalog"> - Equipments.  Only two will be</param>
        /// <param name="tVicDigital"> - used.</param>
        /// <param name="tIntAnalog"></param>
        /// <param name="tIntDigital"></param>
        /// <returns></returns>
        public static int GetCtxEqpt(string cRxTraf,
                                            string cRxEqpt,
                                            string cTxTraf,
                                            string cTxEqpt,
                                            out char cVicType, /* Either 'A' or 'D' Victim */
                                            out char cIntType, /* Either 'A' or 'D' Interferor */
                                            out TcTxAnalog tVicAnalog, /* Equipments.  Only two will be */
                                            out TcTxDigital tVicDigital,  /* used. */
                                            out TcTxAnalog tIntAnalog,
                                            out TcTxDigital tIntDigital)
        {
            // 'out' requirements.
            cVicType = '!';
            cIntType = '!';
            tVicAnalog = new TcTxAnalog();
            tVicDigital = new TcTxDigital();
            tIntAnalog = new TcTxAnalog();
            tIntDigital = new TcTxDigital();

            int nRet;
            SuEqpt tEqpt;

            // Retrieve the SdEqpt object associatede with the receiver equipment from the
            // SDB table main.sd_eqpt.
            if (Suutils.SuGetEqpt(cRxEqpt, out tEqpt) != 0)
            {
                string str = String.Format("The Rx equipment '{0}' could not be found in the Equipment table.", cRxEqpt);
                Log2.e("\nGetCtx.GetCtxEqpt(): ERROR: " + str);
                GenUtil.SetError(1000, str);
                return Error.RX_EQUIPMENT_NOT_FOUND_IN_EQUIPMENT_TABLE;
            }

            cVicType = tEqpt.etype[0];     /*	Get A or D */

            /* 	Now switch on the type of the Receiving traffic */
            switch (cVicType)
            {
                case 'A':
                    /*	Get the analog values from the CTXAEQPT table */
                    nRet = CtxUtil.GetAeqpt(cRxEqpt, cRxTraf, out tVicAnalog);
                    break;

                case 'D':
                    /*	Get the digital values from the CTXDEQPT table */
                    nRet = CtxUtil.GetDeqpt(cRxEqpt, cRxTraf, out tVicDigital);
                    break;

                default:
                    /*	Error, must be either A or D */
                    Log2.e("\nGetCtx.GetCtxEqpt(): ERROR: cVicType is neither 'A' nor 'D'.");
                    GenUtil.SetError(1010, "Victim equipment type neither A nor D.");
                    nRet = Error.VICTIM_TYPE_NOT_A_OR_D;
                    break;
            }

            if (nRet != 0)
            {
                Log2.e("\nGetCtx.GetCtxEqpt(): ERROR: unable to get equipment.");
                return (nRet);
            }

            /*	Look up the Tx Codes in the CTX_BASE to find which of analog or digital */
            if (Suutils.SuGetEqpt(cTxEqpt, out tEqpt) != 0)
            {
                string str = String.Format("The Tx equipment '{0}' could not be found in the Equipment table.", cRxEqpt);
                Log2.e("\nGetCtx.GetCtxEqpt(): ERROR: " + str);
                GenUtil.SetError(1001, str);
                return Error.TX_EQUIPMENT_NOT_FOUND_IN_EQUIPMENT_TABLE;
            }

            cIntType = tEqpt.etype[0];

            /* 	Switch on traffic */
            switch (cIntType)
            {
                case 'A':
                    /*	Get the analog values from the CTXAEQPT table */
                    nRet = CtxUtil.GetAeqpt(cTxEqpt, cTxTraf, out tIntAnalog);
                    break;

                case 'D':
                    /*	Get the digital values from the CTXDEQPT table */
                    nRet = CtxUtil.GetDeqpt(cTxEqpt, cTxTraf, out tIntDigital);
                    break;
                default:
                    /*	Error, must be either A or D */
                    Log2.e("\nGetCtx.GetCtxEqpt(): ERROR: cIntType is neither 'A' nor 'D'.");
                    GenUtil.SetError(1002, "Interferor equipment type neither A nor D.");
                    nRet = Error.VICTIM_TYPE_NOT_A_OR_D;
                    break;
            }

            return (nRet);
        }

        /// <summary>
        /// Given the equipment structures already retrieved, this routine performs the 
        /// required ctx calculations and returns the requirement.  
        /// </summary>
        /// <param name="cIntType"></param>
        /// <param name="cVicType"></param>
        /// <param name="ptVicAnalog"> - Equipments. Only two will</param>
        /// <param name="ptVicDigital"> - be used.</param>
        /// <param name="ptIntAnalog"></param>
        /// <param name="ptIntDigital"></param>
        /// <param name="dFS"></param>
        /// <param name="IsEs"></param>
        /// <param name="cTypeOfInt"></param>
        /// <param name="dRequired"></param>
        /// <returns></returns>
        public static int GetCtxReq(char cIntType,
                                            char cVicType,
                                            TcTxAnalog ptVicAnalog,     /*	Equipments. Only two will */
                                            TcTxDigital ptVicDigital,   /*	be used. */
                                            TcTxAnalog ptIntAnalog,
                                            TcTxDigital ptIntDigital,
                                            double dFS,
                                            bool IsEs,
                                            out char cTypeOfInt,
                                            out double dRequired)
        {
            // 'out' requirement.
            cTypeOfInt = '!';
            dRequired = 0.0;

            double ctoi;
            double ilvl;
            int nRet;

            /* Now go through the possible cases and call the correct routine */
            if (cIntType == 'A' && cVicType == 'A')
            {
                /* Analog into Analog */
                nRet = Suppata.SuppAta(ptVicAnalog.numchan,
                           ptVicAnalog.fmin,
                                       ptVicAnalog.fm,
                                       ptVicAnalog.sigma,
                                       (int)ptVicAnalog.nf,
                                       ptIntAnalog.numchan,
                                       ptIntAnalog.fmin,
                                       ptIntAnalog.fm,
                                       ptIntAnalog.sigma,
                                       dFS,
                                       out ctoi,
                                       out ilvl);
                cTypeOfInt = 'C';       /*	Analog victims use C/I */
                dRequired = ctoi;
                //...Log2.v("\nA_Required = " + dRequired);

            }
            else if (cIntType == 'D' && cVicType == 'A')
            {
                /* Digital into Analog */
                nRet = Suppdta.SuppDta(ptVicAnalog.numchan,
                                       ptVicAnalog.fmin,
                                       ptVicAnalog.fm,
                                       ptVicAnalog.sigma,
                                             ptVicAnalog.nf,
                                       ptVicAnalog.iffreq,
                                       ptVicAnalog.sif,
                                       ptVicAnalog.irf,
                                       ptVicAnalog.nFilter,
                                       ptVicAnalog.aFiltFS,
                                       ptVicAnalog.aFiltVal,
                                       ptVicAnalog.ai70,
                                       ptVicAnalog.ai140,
                                       ptIntDigital.nSpect,
                                       ptIntDigital.aSpectFS,
                                       ptIntDigital.aSpectVal,
                                       ptIntDigital.bandwidth,
                                       dFS,
                                             IsEs,
                                       out ctoi,
                                       out ilvl);
                cTypeOfInt = 'C';
                dRequired = ctoi;
                //...Log2.v("\nB_Required = " + dRequired);

            }
            else if (cVicType == 'D')
            {
                /* Digital or Analog into digital */
                nRet = Suppdoatd.SuppDoatd(ptVicDigital.bandwidth,
                             ptVicDigital.nf,
                                           ptVicDigital.iffreq,
                                           ptVicDigital.ai70,
                                           ptVicDigital.ai140,
                                           ptVicDigital.thdcrit,
                                           ptVicDigital.sif,
                                           ptVicDigital.irf,
                                           ptVicDigital.nFilter,
                                           ptVicDigital.aFiltFS,
                                           ptVicDigital.aFiltVal,
                                           (cIntType == 'D') ? 1 : 0,       /* 1 - D, 0 - A into D */
                                           ptIntAnalog.numchan,
                                           ptIntAnalog.fmin,
                                           ptIntAnalog.fm,
                                           ptIntAnalog.sigma,
                                           ptIntDigital.nSpect,
                                           ptIntDigital.aSpectFS,
                                           ptIntDigital.aSpectVal,
                                           ptIntDigital.bandwidth,
                                           ptVicDigital.nth,
                                           dFS,
                                                 IsEs,
                                           out ilvl);
                cTypeOfInt = 'I';
                dRequired = ilvl;
                //...Log2.v("\nC_Required = " + dRequired);

            }
            else
            {
                /* We are in error. */
                Log2.e("\nGetCtx.GetCtxReq(): ERROR: none of the A or D into A or D combinations used.");
                GenUtil.SetError(1003, "None of the A or D into A or D combinations used.");
                nRet = Error.INVALID_ARGUMENTS;
                //...Log2.v("\nD_Required = " + dRequired);
            }

            return (nRet);
        }


    }
}
