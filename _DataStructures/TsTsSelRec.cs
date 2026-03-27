using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using SQLLEN = Int64;

    /// <summary>
    /// This class has fields that can be populated with data fetched from
    /// the results tables <b>tt_{0}_site a</b>, <b>tt_{0}_ante b</b>, and <b>tt_{0}_chan</b>
    /// and then used to produce the Case Detail Report (.CASEDET) 
    /// and Over-Horizon Loss (.CASEOHL) Report for Ts-Ts interference.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TsTsSelRec
    {
        // IMPORTANT!
        // =========
        // The following qty. TBD member values correspond to the columns
        // of an FtAnte table stored in the database; for simplicity, the
        // member and column names are prescribed to be identical.
        // The order of appearance of these qty. TBD 'column' members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        //	site ===========================================================================
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.INTERFERER_SZ)]
        public string interferer; /* P - proposed, E - environment */
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CALLSIGN_SZ)]
        public string intcall1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CALLSIGN_SZ)]
        public string intcall2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CALLSIGN_SZ)]
        public string viccall1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CALLSIGN_SZ)]
        public string viccall2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FT_SITE_NAME_SZ)]
        public string intname1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FT_SITE_NAME_SZ)]
        public string intname2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FT_SITE_NAME_SZ)]
        public string vicname1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FT_SITE_NAME_SZ)]
        public string vicname2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.OPERCODE_SZ)]
        public string intoper;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.OPERCODE_SZ)]
        public string intoper2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.OPERCODE_SZ)]
        public string vicoper;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.OPERCODE_SZ)]
        public string vicoper2;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int intlatit;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int intlongit;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intgrnd;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int viclatit;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int viclongit;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicgrnd;
        // calculated fields =========================================================
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short report;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int caseno;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int subcases;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double int1int2dist;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vic1vic2dist;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double int1vic1dist;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double distadv;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intoffax;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicoffax;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intvicaz;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicintaz;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int processed;

        //	ante ========================================================================
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.BNDCDE_SZ)]
        public string intbndcde;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short intanum;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.BNDCDE_SZ)]
        public string vicbndcde;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short vicanum;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ACODE_SZ)]
        public string intacode;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ACODE_SZ)]
        public string vicacode;
        // calculated fields ===========================================================
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int subcaseno;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adiscctxh;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adiscctxv;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adisccrxh;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adisccrxv;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adiscxtxh;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adiscxtxv;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adiscxrxh;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adiscxrxv;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.INTAUSE_SZ)]
        public string intause;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.VICAUSE_SZ)]
        public string vicause;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float intgain;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float vicgain;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ACODE_SZ)]
        public string intaxref;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ANTE_MODEL_SZ)]
        public string intamodel;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ACODE_SZ)]
        public string vicaxref;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ANTE_MODEL_SZ)]
        public string vicamodel;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.INTAOFFAX_SZ)]
        public string intaoffax;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double inthopaz;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intantaz;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intoffantax;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.VICAOFFAX_SZ)]
        public string vicaoffax;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vichopaz;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicantaz;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicoffantax;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float intaht;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float vicaht;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intvicel;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicintel;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intelev;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicelev;

        // chan ==========================================================================
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CHID_SZ)]
        public string intchid;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CHID_SZ)]
        public string vicchid;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.INTPOLAR_SZ)]
        public string intpolar;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.VICPOLAR_SZ)]
        public string vicpolar;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.INTSTATTX_SZ)]
        public string intstattx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.VICSTATTX_SZ)]
        public string vicstatrx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.TRAFCODE_SZ)]
        public string inttraftx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.TRAFCODE_SZ)]
        public string victrafrx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ECODE_SZ)]
        public string inteqpttx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ECODE_SZ)]
        public string viceqptrx;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intfreqtxr;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicfreqrxr;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicpwrrx;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intpwrtx;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intafsltx;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicafslrx;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short rxant;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short txant;

        // 	calculated fields ============================================================
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.TRAFCODE_SZ)]
        public string ctxinttraftx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.TRAFCODE_SZ)]
        public string ctxvictrafrx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ECODE_SZ)]
        public string ctxeqpt;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CALCTYPE_SZ)]
        public string calctype;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double totantdisc;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double freqsepr;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double reqdcalc;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double patloss;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calcico;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calcixp;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double resti;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double eirpadv;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double tiltdisc;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double pathloss80;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calcico80;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calcixp80;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double reqd80;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double resti80;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double pathloss99;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calcico99;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calcixp99;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double reqd99;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double resti99;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int ohresult;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double rqco;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ECODE_SZ)]
        public string ctxinteqpt;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.INTEQTYPE_SZ)]
        public string inteqtype;          // A for analog equipment and D for Digital.
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.VICEQTYPE_SZ)]
        public string viceqtype;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intbwchans;      //	If public int is A this is number of channels, 
                                       //  If D this is the bandwidth.
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicbwchans;

        //---------------------------------------------------------------------------------

        public const int NUM_COLUMNS = 116;

        //---------------------------------------------------------------------------------

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the member fields together with
        /// their associated ODBC nullInds.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringWN(SQLLEN[] nullInds)
        {
            int n = 0;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "interferer =      " + interferer);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intcall1 =      " + intcall1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intcall2 =      " + intcall2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "viccall1 =      " + viccall1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "viccall2 =      " + viccall2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intname1 =      " + intname1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intname2 =      " + intname2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicname1 =      " + vicname1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicname2 =      " + vicname2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intoper =      " + intoper);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intoper2 =      " + intoper2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicoper =      " + vicoper);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicoper2 =      " + vicoper2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intlatit =      " + intlatit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intlongit =      " + intlongit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intgrnd =      " + intgrnd);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "viclatit =      " + viclatit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "viclongit =      " + viclongit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicgrnd =      " + vicgrnd);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "report =      " + report);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "caseno =      " + caseno);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "subcases =      " + subcases);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "int1int2dist =      " + int1int2dist);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vic1vic2dist =      " + vic1vic2dist);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "int1vic1dist =      " + int1vic1dist);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "distadv =      " + distadv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intoffax =      " + intoffax);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicoffax =      " + vicoffax);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intvicaz =      " + intvicaz);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicintaz =      " + vicintaz);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "processed =      " + processed);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intbndcde =      " + intbndcde);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intanum =      " + intanum);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicbndcde =      " + vicbndcde);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicanum =      " + vicanum);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intacode =      " + intacode);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicacode =      " + vicacode);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "subcaseno =      " + subcaseno);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "adiscctxh =      " + adiscctxh);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "adiscctxv =      " + adiscctxv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "adisccrxh =      " + adisccrxh);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "adisccrxv =      " + adisccrxv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "adiscxtxh =      " + adiscxtxh);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "adiscxtxv =      " + adiscxtxv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "adiscxrxh =      " + adiscxrxh);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "adiscxrxv =      " + adiscxrxv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intause =      " + intause);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicause =      " + vicause);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intgain =      " + intgain);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicgain =      " + vicgain);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intaxref =      " + intaxref);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intamodel =      " + intamodel);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicaxref =      " + vicaxref);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicamodel =      " + vicamodel);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intaoffax =      " + intaoffax);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "inthopaz =      " + inthopaz);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intantaz =      " + intantaz);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intoffantax =      " + intoffantax);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicaoffax =      " + vicaoffax);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vichopaz =      " + vichopaz);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicantaz =      " + vicantaz);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicoffantax =      " + vicoffantax);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intaht =      " + intaht);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicaht =      " + vicaht);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intvicel =      " + intvicel);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicintel =      " + vicintel);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intelev =      " + intelev);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicelev =      " + vicelev);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intchid =      " + intchid);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicchid =      " + vicchid);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intpolar =      " + intpolar);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicpolar =      " + vicpolar);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intstattx =      " + intstattx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicstatrx =      " + vicstatrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "inttraftx =      " + inttraftx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "victrafrx =      " + victrafrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "inteqpttx =      " + inteqpttx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "viceqptrx =      " + viceqptrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intfreqtxr =      " + intfreqtxr);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicfreqrxr =      " + vicfreqrxr);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicpwrrx =      " + vicpwrrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intpwrtx =      " + intpwrtx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intafsltx =      " + intafsltx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicafslrx =      " + vicafslrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxant =      " + rxant);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txant =      " + txant);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ctxinttraftx =      " + ctxinttraftx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ctxvictrafrx =      " + ctxvictrafrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ctxeqpt =      " + ctxeqpt);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "calctype =      " + calctype);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "totantdisc =      " + totantdisc);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "freqsepr =      " + freqsepr);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "reqdcalc =      " + reqdcalc);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "patloss =      " + patloss);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "calcico =      " + calcico);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "calcixp =      " + calcixp);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "resti =      " + resti);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "eirpadv =      " + eirpadv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tiltdisc =      " + tiltdisc);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "pathloss80 =      " + pathloss80);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "calcico80 =      " + calcico80);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "calcixp80 =      " + calcixp80);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "reqd80 =      " + reqd80);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "resti80 =      " + resti80);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "pathloss99 =      " + pathloss99);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "calcico99 =      " + calcico99);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "calcixp99 =      " + calcixp99);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "reqd99 =      " + reqd99);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "resti99 =      " + resti99);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ohresult =      " + ohresult);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rqco =      " + rqco);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ctxinteqpt =      " + ctxinteqpt);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "inteqtype =      " + inteqtype);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "viceqtype =      " + viceqtype);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intbwchans =      " + intbwchans);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicbwchans =      " + vicbwchans);

            return sb.ToString();
        }

        //--------------------------------------------------------------------

        public const int INTERFERER_SZ = Constant.INTERFERER_SZ;
        public const int INTCALL1_SZ = Constant.CALLSIGN_SZ;
        public const int INTCALL2_SZ = Constant.CALLSIGN_SZ;
        public const int VICCALL1_SZ = Constant.CALLSIGN_SZ;
        public const int VICCALL2_SZ = Constant.CALLSIGN_SZ;
        public const int INTNAME1_SZ = Constant.FT_SITE_NAME_SZ;
        public const int INTNAME2_SZ = Constant.FT_SITE_NAME_SZ;
        public const int VICNAME1_SZ = Constant.FT_SITE_NAME_SZ;
        public const int VICNAME2_SZ = Constant.FT_SITE_NAME_SZ;
        public const int INTOPER_SZ = Constant.OPERCODE_SZ;
        public const int INTOPER2_SZ = Constant.OPERCODE_SZ;
        public const int VICOPER_SZ = Constant.OPERCODE_SZ;
        public const int VICOPER2_SZ = Constant.OPERCODE_SZ;
        public const int INTBNDCDE_SZ = Constant.BNDCDE_SZ;
        public const int VICBNDCDE_SZ = Constant.BNDCDE_SZ;
        public const int INTACODE_SZ = Constant.ACODE_SZ;
        public const int VICACODE_SZ = Constant.ACODE_SZ;
        public const int INTAUSE_SZ = Constant.INTAUSE_SZ;
        public const int VICAUSE_SZ = Constant.VICAUSE_SZ;
        public const int INTAXREF_SZ = Constant.ACODE_SZ;
        public const int INTAMODEL_SZ = Constant.ANTE_MODEL_SZ;
        public const int VICAXREF_SZ = Constant.ACODE_SZ;
        public const int VICAMODEL_SZ = Constant.ANTE_MODEL_SZ;
        public const int INTAOFFAX_SZ = Constant.INTAOFFAX_SZ;
        public const int VICAOFFAX_SZ = Constant.VICAOFFAX_SZ;
        public const int INTCHID_SZ = Constant.CHID_SZ;
        public const int VICCHID_SZ = Constant.CHID_SZ;
        public const int INTPOLAR_SZ = Constant.INTPOLAR_SZ;
        public const int VICPOLAR_SZ = Constant.VICPOLAR_SZ;
        public const int INTSTATTX_SZ = Constant.INTSTATTX_SZ;
        public const int VICSTATRX_SZ = Constant.VICSTATTX_SZ;
        public const int INTTRAFTX_SZ = Constant.TRAFCODE_SZ;
        public const int VICTRAFRX_SZ = Constant.TRAFCODE_SZ;
        public const int INTEQPTTX_SZ = Constant.ECODE_SZ;
        public const int VICEQPTRX_SZ = Constant.ECODE_SZ;
        public const int CTXINTTRAFTX_SZ = Constant.TRAFCODE_SZ;
        public const int CTXVICTRAFRX_SZ = Constant.TRAFCODE_SZ;
        public const int CTXEQPT_SZ = Constant.ECODE_SZ;
        public const int CALCTYPE_SZ = Constant.CALCTYPE_SZ;
        public const int CTXINTEQPT_SZ = Constant.ECODE_SZ;
        public const int INTEQTYPE_SZ = Constant.INTEQTYPE_SZ;
        public const int VICEQTYPE_SZ = Constant.VICEQTYPE_SZ;

        //---------------------------------------------------------------------
        public const int INTERFERER = 000;
        public const int INTCALL1 = 001;
        public const int INTCALL2 = 002;
        public const int VICCALL1 = 003;
        public const int VICCALL2 = 004;
        public const int INTNAME1 = 005;
        public const int INTNAME2 = 006;
        public const int VICNAME1 = 007;
        public const int VICNAME2 = 008;
        public const int INTOPER = 009;
        public const int INTOPER2 = 010;
        public const int VICOPER = 011;
        public const int VICOPER2 = 012;
        public const int INTLATIT = 013;
        public const int INTLONGIT = 014;
        public const int INTGRND = 015;
        public const int VICLATIT = 016;
        public const int VICLONGIT = 017;
        public const int VICGRND = 018;
        public const int REPORT = 019;
        public const int CASENO = 020;
        public const int SUBCASES = 021;
        public const int INT1INT2DIST = 022;
        public const int VIC1VIC2DIST = 023;
        public const int INT1VIC1DIST = 024;
        public const int DISTADV = 025;
        public const int INTOFFAX = 026;
        public const int VICOFFAX = 027;
        public const int INTVICAZ = 028;
        public const int VICINTAZ = 029;
        public const int PROCESSED = 030;
        public const int INTBNDCDE = 031;
        public const int INTANUM = 032;
        public const int VICBNDCDE = 033;
        public const int VICANUM = 034;
        public const int INTACODE = 035;
        public const int VICACODE = 036;
        public const int SUBCASENO = 037;
        public const int ADISCCTXH = 038;
        public const int ADISCCTXV = 039;
        public const int ADISCCRXH = 040;
        public const int ADISCCRXV = 041;
        public const int ADISCXTXH = 042;
        public const int ADISCXTXV = 043;
        public const int ADISCXRXH = 044;
        public const int ADISCXRXV = 045;
        public const int INTAUSE = 046;
        public const int VICAUSE = 047;
        public const int INTGAIN = 048;
        public const int VICGAIN = 049;
        public const int INTAXREF = 050;
        public const int INTAMODEL = 051;
        public const int VICAXREF = 052;
        public const int VICAMODEL = 053;
        public const int INTAOFFAX = 054;
        public const int INTHOPAZ = 055;
        public const int INTANTAZ = 056;
        public const int INTOFFANTAX = 057;
        public const int VICAOFFAX = 058;
        public const int VICHOPAZ = 059;
        public const int VICANTAZ = 060;
        public const int VICOFFANTAX = 061;
        public const int INTAHT = 062;
        public const int VICAHT = 063;
        public const int INTVICEL = 064;
        public const int VICINTEL = 065;
        public const int INTELEV = 066;
        public const int VICELEV = 067;
        public const int INTCHID = 068;
        public const int VICCHID = 069;
        public const int INTPOLAR = 070;
        public const int VICPOLAR = 071;
        public const int INTSTATTX = 072;
        public const int VICSTATRX = 073;
        public const int INTTRAFTX = 074;
        public const int VICTRAFRX = 075;
        public const int INTEQPTTX = 076;
        public const int VICEQPTRX = 077;
        public const int INTFREQTXR = 078;
        public const int VICFREQRXR = 079;
        public const int VICPWRRX = 080;
        public const int INTPWRTX = 081;
        public const int INTAFSLTX = 082;
        public const int VICAFSLRX = 083;
        public const int RXANT = 084;
        public const int TXANT = 085;
        public const int CTXINTTRAFTX = 086;
        public const int CTXVICTRAFRX = 087;
        public const int CTXEQPT = 088;
        public const int CALCTYPE = 089;
        public const int TOTANTDISC = 090;
        public const int FREQSEPR = 091;
        public const int REQDCALC = 092;
        public const int PATLOSS = 093;
        public const int CALCICO = 094;
        public const int CALCIXP = 095;
        public const int RESTI = 096;
        public const int EIRPADV = 097;
        public const int TILTDISC = 098;
        public const int PATHLOSS80 = 099;
        public const int CALCICO80 = 100;
        public const int CALCIXP80 = 101;
        public const int REQD80 = 102;
        public const int RESTI80 = 103;
        public const int PATHLOSS99 = 104;
        public const int CALCICO99 = 105;
        public const int CALCIXP99 = 106;
        public const int REQD99 = 107;
        public const int RESTI99 = 108;
        public const int OHRESULT = 109;
        public const int RQCO = 110;
        public const int CTXINTEQPT = 111;
        public const int INTEQTYPE = 112;
        public const int VICEQTYPE = 113;
        public const int INTBWCHANS = 114;
        public const int VICBWCHANS = 115;
        //---------------------------------------------------------------------
        /*
        interferer
        intcall1
        intcall2
        viccall1
        viccall2
        intname1
        intname2
        vicname1
        vicname2
        intoper
        intoper2
        vicoper
        vicoper2
        intlatit
        intlongit
        intgrnd
        viclatit
        viclongit
        vicgrnd
        report
        caseno
        subcases
        int1int2dist
        vic1vic2dist
        int1vic1dist
        distadv
        intoffax
        vicoffax
        intvicaz
        vicintaz
        processed
        intbndcde
        intanum
        vicbndcde
        vicanum
        intacode
        vicacode
        subcaseno
        adiscctxh
        adiscctxv
        adisccrxh
        adisccrxv
        adiscxtxh
        adiscxtxv
        adiscxrxh
        adiscxrxv
        intause
        vicause
        intgain
        vicgain
        intaxref
        intamodel
        vicaxref
        vicamodel
        intaoffax
        inthopaz
        intantaz
        intoffantax
        vicaoffax
        vichopaz
        vicantaz
        vicoffantax
        intaht
        vicaht
        intvicel
        vicintel
        intelev
        vicelev
        intchid
        vicchid
        intpolar
        vicpolar
        intstattx
        vicstatrx
        inttraftx
        victrafrx
        inteqpttx
        viceqptrx
        intfreqtxr
        vicfreqrxr
        vicpwrrx
        intpwrtx
        intafsltx
        vicafslrx
        rxant
        txant
        ctxinttraftx
        ctxvictrafrx
        ctxeqpt
        calctype
        totantdisc
        freqsepr
        reqdcalc
        patloss
        calcico
        calcixp
        resti
        eirpadv
        tiltdisc
        pathloss80
        calcico80
        calcixp80
        reqd80
        resti80
        pathloss99
        calcico99
        calcixp99
        reqd99
        resti99
        ohresult
        rqco
        ctxinteqpt
        inteqtype
        viceqtype
        intbwchans
        vicbwchans
        */


    }
}
