# Documented File: Tstsrp3.cs
**Repository Path:** `TpRunTsip20260126\Tstsrp3.cs`
**Primary Layer:** `TpRunTsip20260126`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using _DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TpRunTsip
{
    using _Configuration;
    using _NewLib;
    using _Utillib;
    using System.Runtime.InteropServices;
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

    /// <summary>
    /// Provides methods used to produce the Case Detail Report (.CASEDET) 
    /// and Over-Horizon Loss (.CASEOHL) Report for Ts-Ts interference.
    /// </summary>
    public class Tstsrp3
    {
        private static int lastCase = -1;
        private static int lastSubCase = -1;
        private const string SPACE = " ";
        private const string LEFT_PARENTHESIS = "(";
        private const string RIGHT_PARENTHESIS = ")";
        private static string SELECT_TEMPLATE = "SELECT a.interferer, a.caseno, subcaseno, a.subcases, a.intcall1, a.intcall2, a.intoper, a.intoper2, a.intname1, a.intname2, a.viccall1, a.viccall2, a.vicoper, a.vicoper2, a.vicname1, a.vicname2, a.int1int2dist, a.vic1vic2dist, a.int1vic1dist, a.intoffax, a.vicoffax, a.intgrnd, a.vicgrnd, b.intaoffax, b.inthopaz, b.intantaz, b.intoffantax, b.vicaoffax, b.vichopaz, b.vicantaz, b.vicoffantax, intvicaz, vicintaz, b.intbndcde, b.intanum, intacode, intause, intgain, vicgain, b.vicbndcde, b.vicanum, vicacode, vicause, adiscctxh, adiscctxv, adisccrxh, adisccrxv, adiscxtxh, adiscxtxv, adiscxrxh, adiscxrxv, totantdisc, intchid,(intfreqtx / 1000) as intfreqtxr,intpolar,intstattx,vicchid,(vicfreqrx / 1000) as vicfreqrxr,vicpolar,vicstatrx,inttraftx,inteqpttx,victrafrx,viceqptrx,vicpwrrx,intpwrtx,intafsltx,vicafslrx,ctxinttraftx,ctxvictrafrx,ctxeqpt,(freqsep / 1000) as freqsepr,patloss,a.distadv,eirpadv,calcico,calcixp,reqdcalc,resti,calctype,rxant,txant,intaxref,intamodel,vicaxref,vicamodel,intelev,vicelev,intvicel,vicintel,intaht,vicaht,tiltdisc,pathloss80,calcico80,calcixp80,reqd80,resti80,pathloss99,calcico99,calcixp99,reqd99,resti99,ohresult,ctxinteqpt,inteqtype,viceqtype,intbwchans,vicbwchans from {0}.tt_{1}_site a, {0}.tt_{1}_ante b, {0}.tt_{1}_chan c where a.intcall1 = b.intcall1 and a.intcall2 = b.intcall2 and a.viccall1 = b.viccall1 and a.viccall2 = b.viccall2 and a.caseno   = b.caseno and a.interferer = b.interferer and c.intcall1 = b.intcall1 and c.intcall2 = b.intcall2 and c.viccall1 = b.viccall1 and c.viccall2 = b.viccall2 and c.caseno   = b.caseno and c.interferer = b.interferer and c.intanum = b.intanum and c.vicanum = b.vicanum and c.intbndcde = b.intbndcde";
        /// <summary>
        /// This method retrieves TSIP results data from the DB and produces
        /// the bulk of the text for the CASEDET and CASEOHL reports.
        /// </summary>
        /// <param name="tw"> - the TextWriter object assigned to the .CASEDET (or CASEOHL) report.</param>
        /// <param name="ttName"> - the unique ID substring of the Ts DB table names.</param>
        /// <param name="tpParm"> - the TpParm object providing parameter data</param>
        /// <param name="IsOhOnly"> - boolean, true if only an OHL report is required.</param>
        /// <returns></returns>
        public static int TsTsRp3(TextWriter tw, string ttName, TpParm tpParm, bool IsOhOnly)
        {
            //...Log2.v("\nTstsrp3.TsTsRp3(): Entry: ttName = " + ttName);

            TsTsSelRec tRec = new TsTsSelRec();
            //REPPARM rp3Parm;
            int ohtrigger;

            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            SQLLEN[] nullInds = new SQLLEN[TsTsSelRec.NUM_COLUMNS];

            string cSQL;

            PrintLine pl = new PrintLine();
            pl.OutFile(tw);
            pl.FormFeed();

            ohtrigger = IsOhOnly ? 1 : 0;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format(SELECT_TEMPLATE, Info.GlobalSchema, ttName);

            //	Now check for the ohonly type of report;
            if (IsOhOnly)
            {
                string cOHSQL;

                cOHSQL = String.Format(" and (c.ohresult = 0 or ((c.ohresult > 0 and (c.ohresult % 1000) <= 100) and (c.resti80 < {0} or c.resti99 < {1})))",
                                    tpParm.margin, tpParm.margin);
                cSQL += cOHSQL;
            }

            //	Order it:-
            cSQL += " ORDER BY caseno, subcaseno, intfreqtxr, vicfreqrxr ";

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTstsrp3.TsTsRp3(): ERROR: SQLExecDirect() failed\n" + cSQL);
                Ssutil.DbGetDiagStmt(hStmt, "tstsrp301: Failed to Execute:-");
                sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                sqlRet = (SQLRETURN)Ssutil.DisConn(hConn);

                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //...Log2.v("\nTstsrp3.TsTsRp3(): SQLExecDirect() succeeded:\n" + cSQL);

            while (true)
            {
                sqlRet = ODBC.SQLFetch(hStmt);
                if (sqlRet == Constant.NOMORERECS)
                {
                    //...Log2.v("\nTstsrp3.TsTsRp3(): SQLFetch() returned sqlRet == Constant.NOMORERECS");
                    break;
                }
                else if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\nTstsrp3.TsTsRp3(): call to SQLFetch() failed.");
                    Ssutil.DbGetDiagStmt(hStmt, "tstsrp302: Failed Fetch:");
                    sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    sqlRet = (SQLRETURN)Ssutil.DisConn(hConn);
                    return -2;
                }

                NullHelper.FillArray(ref nullInds, Constant.DB_NULL);

                // Setup automatic column indexing.
                Ssutil.DbStartGets();

                try
                {
                    // NOTE: the 'get' order of the columns is NOT the same as the 'member' order of SelRec.

                    Ssutil.DbGetString(hStmt, 0, "interferer", out tRec.interferer, TsTsSelRec.INTERFERER_SZ, out nullInds[TsTsSelRec.INTERFERER]);
                    Ssutil.DbGetInt(hStmt, 0, "caseno", out tRec.caseno, out nullInds[TsTsSelRec.CASENO]);
                    Ssutil.DbGetInt(hStmt, 0, "subcaseno", out tRec.subcaseno, out nullInds[TsTsSelRec.SUBCASENO]);
                    Ssutil.DbGetInt(hStmt, 0, "subcases", out tRec.subcases, out nullInds[TsTsSelRec.SUBCASES]);
                    Ssutil.DbGetString(hStmt, 0, "intcall1", out tRec.intcall1, TsTsSelRec.INTCALL1_SZ, out nullInds[TsTsSelRec.INTCALL1]);
                    Ssutil.DbGetString(hStmt, 0, "intcall2", out tRec.intcall2, TsTsSelRec.INTCALL2_SZ, out nullInds[TsTsSelRec.INTCALL2]);
                    Ssutil.DbGetString(hStmt, 0, "intoper", out tRec.intoper, TsTsSelRec.INTOPER_SZ, out nullInds[TsTsSelRec.INTOPER]);
                    Ssutil.DbGetString(hStmt, 0, "intoper2", out tRec.intoper2, TsTsSelRec.INTOPER2_SZ, out nullInds[TsTsSelRec.INTOPER2]);
                    Ssutil.DbGetString(hStmt, 0, "intname1", out tRec.intname1, TsTsSelRec.INTNAME1_SZ, out nullInds[TsTsSelRec.INTNAME1]);
                    Ssutil.DbGetString(hStmt, 0, "intname2", out tRec.intname2, TsTsSelRec.INTNAME2_SZ, out nullInds[TsTsSelRec.INTNAME2]);
                    Ssutil.DbGetString(hStmt, 0, "viccall1", out tRec.viccall1, TsTsSelRec.VICCALL1_SZ, out nullInds[TsTsSelRec.VICCALL1]);
                    Ssutil.DbGetString(hStmt, 0, "viccall2", out tRec.viccall2, TsTsSelRec.VICCALL2_SZ, out nullInds[TsTsSelRec.VICCALL2]);
                    Ssutil.DbGetString(hStmt, 0, "vicoper", out tRec.vicoper, TsTsSelRec.VICOPER_SZ, out nullInds[TsTsSelRec.VICOPER]);
                    Ssutil.DbGetString(hStmt, 0, "vicoper2", out tRec.vicoper2, TsTsSelRec.VICOPER2_SZ, out nullInds[TsTsSelRec.VICOPER2]);
                    Ssutil.DbGetString(hStmt, 0, "vicname1", out tRec.vicname1, TsTsSelRec.VICNAME1_SZ, out nullInds[TsTsSelRec.VICNAME1]);
                    Ssutil.DbGetString(hStmt, 0, "vicname2", out tRec.vicname2, TsTsSelRec.VICNAME2_SZ, out nullInds[TsTsSelRec.VICNAME2]);
                    Ssutil.DbGetDouble(hStmt, 0, "int1int2dist", out tRec.int1int2dist, out nullInds[TsTsSelRec.INT1INT2DIST]);
                    Ssutil.DbGetDouble(hStmt, 0, "vic1vic2dist", out tRec.vic1vic2dist, out nullInds[TsTsSelRec.VIC1VIC2DIST]);
                    Ssutil.DbGetDouble(hStmt, 0, "int1vic1dist", out tRec.int1vic1dist, out nullInds[TsTsSelRec.INT1VIC1DIST]);
                    Ssutil.DbGetDouble(hStmt, 0, "intoffax", out tRec.intoffax, out nullInds[TsTsSelRec.INTOFFAX]);
                    Ssutil.DbGetDouble(hStmt, 0, "vicoffax", out tRec.vicoffax, out nullInds[TsTsSelRec.VICOFFAX]);
                    Ssutil.DbGetDouble(hStmt, 0, "intgrnd", out tRec.intgrnd, out nullInds[TsTsSelRec.INTGRND]);
                    Ssutil.DbGetDouble(hStmt, 0, "vicgrnd", out tRec.vicgrnd, out nullInds[TsTsSelRec.VICGRND]);
                    Ssutil.DbGetString(hStmt, 0, "intaoffax", out tRec.intaoffax, TsTsSelRec.INTAOFFAX_SZ, out nullInds[TsTsSelRec.INTAOFFAX]);
                    Ssutil.DbGetDouble(hStmt, 0, "inthopaz", out tRec.inthopaz, out nullInds[TsTsSelRec.INTHOPAZ]);
                    Ssutil.DbGetDouble(hStmt, 0, "intantaz", out tRec.intantaz, out nullInds[TsTsSelRec.INTANTAZ]);
                    Ssutil.DbGetDouble(hStmt, 0, "intoffantax", out tRec.intoffantax, out nullInds[TsTsSelRec.INTOFFANTAX]);
                    Ssutil.DbGetString(hStmt, 0, "vicaoffax", out tRec.vicaoffax, TsTsSelRec.VICAOFFAX_SZ, out nullInds[TsTsSelRec.VICAOFFAX]);
                    Ssutil.DbGetDouble(hStmt, 0, "vichopaz", out tRec.vichopaz, out nullInds[TsTsSelRec.VICHOPAZ]);
                    Ssutil.DbGetDouble(hStmt, 0, "vicantaz", out tRec.vicantaz, out nullInds[TsTsSelRec.VICANTAZ]);
                    Ssutil.DbGetDouble(hStmt, 0, "vicoffantax", out tRec.vicoffantax, out nullInds[TsTsSelRec.VICOFFANTAX]);
                    Ssutil.DbGetDouble(hStmt, 0, "intvicaz", out tRec.intvicaz, out nullInds[TsTsSelRec.INTVICAZ]);
                    Ssutil.DbGetDouble(hStmt, 0, "vicintaz", out tRec.vicintaz, out nullInds[TsTsSelRec.VICINTAZ]);
                    Ssutil.DbGetString(hStmt, 0, "intbndcde", out tRec.intbndcde, TsTsSelRec.INTBNDCDE_SZ, out nullInds[TsTsSelRec.INTBNDCDE]);
                    Ssutil.DbGetShort(hStmt, 0, "intanum", out tRec.intanum, out nullInds[TsTsSelRec.INTANUM]);
                    Ssutil.DbGetString(hStmt, 0, "intacode", out tRec.intacode, TsTsSelRec.INTACODE_SZ, out nullInds[TsTsSelRec.INTACODE]);
                    Ssutil.DbGetString(hStmt, 0, "intause", out tRec.intause, TsTsSelRec.INTAUSE_SZ, out nullInds[TsTsSelRec.INTAUSE]);
                    Ssutil.DbGetFloat(hStmt, 0, "intgain", out tRec.intgain, out nullInds[TsTsSelRec.INTGAIN]);
                    Ssutil.DbGetFloat(hStmt, 0, "vicgain", out tRec.vicgain, out nullInds[TsTsSelRec.VICGAIN]);
                    Ssutil.DbGetString(hStmt, 0, "vicbndcde", out tRec.vicbndcde, TsTsSelRec.VICBNDCDE_SZ, out nullInds[TsTsSelRec.VICBNDCDE]);
                    Ssutil.DbGetShort(hStmt, 0, "vicanum", out tRec.vicanum, out nullInds[TsTsSelRec.VICANUM]);
                    Ssutil.DbGetString(hStmt, 0, "vicacode", out tRec.vicacode, TsTsSelRec.VICACODE_SZ, out nullInds[TsTsSelRec.VICACODE]);
                    Ssutil.DbGetString(hStmt, 0, "vicause", out tRec.vicause, TsTsSelRec.VICAUSE_SZ, out nullInds[TsTsSelRec.VICAUSE]);
                    Ssutil.DbGetDouble(hStmt, 0, "adiscctxh", out tRec.adiscctxh, out nullInds[TsTsSelRec.ADISCCTXH]);
                    Ssutil.DbGetDouble(hStmt, 0, "adiscctxv", out tRec.adiscctxv, out nullInds[TsTsSelRec.ADISCCTXV]);
                    Ssutil.DbGetDouble(hStmt, 0, "adisccrxh", out tRec.adisccrxh, out nullInds[TsTsSelRec.ADISCCRXH]);
                    Ssutil.DbGetDouble(hStmt, 0, "adisccrxv", out tRec.adisccrxv, out nullInds[TsTsSelRec.ADISCCRXV]);
                    Ssutil.DbGetDouble(hStmt, 0, "adiscxtxh", out tRec.adiscxtxh, out nullInds[TsTsSelRec.ADISCXTXH]);
                    Ssutil.DbGetDouble(hStmt, 0, "adiscxtxv", out tRec.adiscxtxv, out nullInds[TsTsSelRec.ADISCXTXV]);
                    Ssutil.DbGetDouble(hStmt, 0, "adiscxrxh", out tRec.adiscxrxh, out nullInds[TsTsSelRec.ADISCXRXH]);
                    Ssutil.DbGetDouble(hStmt, 0, "adiscxrxv", out tRec.adiscxrxv, out nullInds[TsTsSelRec.ADISCXRXV]);
                    Ssutil.DbGetDouble(hStmt, 0, "totantdisc", out tRec.totantdisc, out nullInds[TsTsSelRec.TOTANTDISC]);
                    Ssutil.DbGetString(hStmt, 0, "intchid", out tRec.intchid, TsTsSelRec.INTCHID_SZ, out nullInds[TsTsSelRec.INTCHID]);
                    Ssutil.DbGetDouble(hStmt, 0, "intfreqtxr", out tRec.intfreqtxr, out nullInds[TsTsSelRec.INTFREQTXR]);
                    Ssutil.DbGetString(hStmt, 0, "intpolar", out tRec.intpolar, TsTsSelRec.INTPOLAR_SZ, out nullInds[TsTsSelRec.INTPOLAR]);
                    Ssutil.DbGetString(hStmt, 0, "intstattx", out tRec.intstattx, TsTsSelRec.INTSTATTX_SZ, out nullInds[TsTsSelRec.INTSTATTX]);
                    Ssutil.DbGetString(hStmt, 0, "vicchid", out tRec.vicchid, TsTsSelRec.VICCHID_SZ, out nullInds[TsTsSelRec.VICCHID]);
                    Ssutil.DbGetDouble(hStmt, 0, "vicfreqrxr", out tRec.vicfreqrxr, out nullInds[TsTsSelRec.VICFREQRXR]);
                    Ssutil.DbGetString(hStmt, 0, "vicpolar", out tRec.vicpolar, TsTsSelRec.VICPOLAR_SZ, out nullInds[TsTsSelRec.VICPOLAR]);
                    Ssutil.DbGetString(hStmt, 0, "vicstatrx", out tRec.vicstatrx, TsTsSelRec.VICSTATRX_SZ, out nullInds[TsTsSelRec.VICSTATRX]);
                    Ssutil.DbGetString(hStmt, 0, "inttraftx", out tRec.inttraftx, TsTsSelRec.INTTRAFTX_SZ, out nullInds[TsTsSelRec.INTTRAFTX]);
                    Ssutil.DbGetString(hStmt, 0, "inteqpttx", out tRec.inteqpttx, TsTsSelRec.INTEQPTTX_SZ, out nullInds[TsTsSelRec.INTEQPTTX]);
                    Ssutil.DbGetString(hStmt, 0, "victrafrx", out tRec.victrafrx, TsTsSelRec.VICTRAFRX_SZ, out nullInds[TsTsSelRec.VICTRAFRX]);
                    Ssutil.DbGetString(hStmt, 0, "viceqptrx", out tRec.viceqptrx, TsTsSelRec.VICEQPTRX_SZ, out nullInds[TsTsSelRec.VICEQPTRX]);
                    Ssutil.DbGetDouble(hStmt, 0, "vicpwrrx", out tRec.vicpwrrx, out nullInds[TsTsSelRec.VICPWRRX]);
                    Ssutil.DbGetDouble(hStmt, 0, "intpwrtx", out tRec.intpwrtx, out nullInds[TsTsSelRec.INTPWRTX]);
                    Ssutil.DbGetDouble(hStmt, 0, "intafsltx", out tRec.intafsltx, out nullInds[TsTsSelRec.INTAFSLTX]);
                    Ssutil.DbGetDouble(hStmt, 0, "vicafslrx", out tRec.vicafslrx, out nullInds[TsTsSelRec.VICAFSLRX]);
                    Ssutil.DbGetString(hStmt, 0, "ctxinttraftx", out tRec.ctxinttraftx, TsTsSelRec.CTXINTTRAFTX_SZ, out nullInds[TsTsSelRec.CTXINTTRAFTX]);
                    Ssutil.DbGetString(hStmt, 0, "ctxvictrafrx", out tRec.ctxvictrafrx, TsTsSelRec.CTXVICTRAFRX_SZ, out nullInds[TsTsSelRec.CTXVICTRAFRX]);
                    Ssutil.DbGetString(hStmt, 0, "ctxeqpt", out tRec.ctxeqpt, TsTsSelRec.CTXEQPT_SZ, out nullInds[TsTsSelRec.CTXEQPT]);
                    Ssutil.DbGetDouble(hStmt, 0, "freqsepr", out tRec.freqsepr, out nullInds[TsTsSelRec.FREQSEPR]);
                    Ssutil.DbGetDouble(hStmt, 0, "patloss", out tRec.patloss, out nullInds[TsTsSelRec.PATLOSS]);
                    Ssutil.DbGetDouble(hStmt, 0, "distadv", out tRec.distadv, out nullInds[TsTsSelRec.DISTADV]);
                    Ssutil.DbGetDouble(hStmt, 0, "eirpadv", out tRec.eirpadv, out nullInds[TsTsSelRec.EIRPADV]);
                    Ssutil.DbGetDouble(hStmt, 0, "calcico", out tRec.calcico, out nullInds[TsTsSelRec.CALCICO]);
                    Ssutil.DbGetDouble(hStmt, 0, "calcixp", out tRec.calcixp, out nullInds[TsTsSelRec.CALCIXP]);
                    Ssutil.DbGetDouble(hStmt, 0, "reqdcalc", out tRec.reqdcalc, out nullInds[TsTsSelRec.REQDCALC]);
                    Ssutil.DbGetDouble(hStmt, 0, "resti", out tRec.resti, out nullInds[TsTsSelRec.RESTI]);
                    Ssutil.DbGetString(hStmt, 0, "calctype", out tRec.calctype, TsTsSelRec.CALCTYPE_SZ, out nullInds[TsTsSelRec.CALCTYPE]);
                    Ssutil.DbGetShort(hStmt, 0, "rxant", out tRec.rxant, out nullInds[TsTsSelRec.RXANT]);
                    Ssutil.DbGetShort(hStmt, 0, "txant", out tRec.txant, out nullInds[TsTsSelRec.TXANT]);
                    Ssutil.DbGetString(hStmt, 0, "intaxref", out tRec.intaxref, TsTsSelRec.INTAXREF_SZ, out nullInds[TsTsSelRec.INTAXREF]);
                    Ssutil.DbGetString(hStmt, 0, "intamodel", out tRec.intamodel, TsTsSelRec.INTAMODEL_SZ, out nullInds[TsTsSelRec.INTAMODEL]);
                    Ssutil.DbGetString(hStmt, 0, "vicaxref", out tRec.vicaxref, TsTsSelRec.VICAXREF_SZ, out nullInds[TsTsSelRec.VICAXREF]);
                    Ssutil.DbGetString(hStmt, 0, "vicamodel", out tRec.vicamodel, TsTsSelRec.VICAMODEL_SZ, out nullInds[TsTsSelRec.VICAMODEL]);
                    Ssutil.DbGetDouble(hStmt, 0, "intelev", out tRec.intelev, out nullInds[TsTsSelRec.INTELEV]);
                    Ssutil.DbGetDouble(hStmt, 0, "vicelev", out tRec.vicelev, out nullInds[TsTsSelRec.VICELEV]);
                    Ssutil.DbGetDouble(hStmt, 0, "intvicel", out tRec.intvicel, out nullInds[TsTsSelRec.INTVICEL]);
                    Ssutil.DbGetDouble(hStmt, 0, "vicintel", out tRec.vicintel, out nullInds[TsTsSelRec.VICINTEL]);
                    Ssutil.DbGetFloat(hStmt, 0, "intaht", out tRec.intaht, out nullInds[TsTsSelRec.INTAHT]);
                    Ssutil.DbGetFloat(hStmt, 0, "vicaht", out tRec.vicaht, out nullInds[TsTsSelRec.VICAHT]);
                    Ssutil.DbGetDouble(hStmt, 0, "tiltdisc", out tRec.tiltdisc, out nullInds[TsTsSelRec.TILTDISC]);
                    Ssutil.DbGetDouble(hStmt, 0, "pathloss80", out tRec.pathloss80, out nullInds[TsTsSelRec.PATHLOSS80]);
                    Ssutil.DbGetDouble(hStmt, 0, "calcico80", out tRec.calcico80, out nullInds[TsTsSelRec.CALCICO80]);
                    Ssutil.DbGetDouble(hStmt, 0, "calcixp80", out tRec.calcixp80, out nullInds[TsTsSelRec.CALCIXP80]);
                    Ssutil.DbGetDouble(hStmt, 0, "reqd80", out tRec.reqd80, out nullInds[TsTsSelRec.REQD80]);
                    Ssutil.DbGetDouble(hStmt, 0, "resti80", out tRec.resti80, out nullInds[TsTsSelRec.RESTI80]);
                    Ssutil.DbGetDouble(hStmt, 0, "pathloss99", out tRec.pathloss99, out nullInds[TsTsSelRec.PATHLOSS99]);
                    Ssutil.DbGetDouble(hStmt, 0, "calcico99", out tRec.calcico99, out nullInds[TsTsSelRec.CALCICO99]);
                    Ssutil.DbGetDouble(hStmt, 0, "calcixp99", out tRec.calcixp99, out nullInds[TsTsSelRec.CALCIXP99]);
                    Ssutil.DbGetDouble(hStmt, 0, "reqd99", out tRec.reqd99, out nullInds[TsTsSelRec.REQD99]);
                    Ssutil.DbGetDouble(hStmt, 0, "resti99", out tRec.resti99, out nullInds[TsTsSelRec.RESTI99]);
                    Ssutil.DbGetInt(hStmt, 0, "ohresult", out tRec.ohresult, out nullInds[TsTsSelRec.OHRESULT]);
                    Ssutil.DbGetString(hStmt, 0, "ctxinteqpt", out tRec.ctxinteqpt, TsTsSelRec.CTXINTEQPT_SZ, out nullInds[TsTsSelRec.CTXINTEQPT]);
                    Ssutil.DbGetString(hStmt, 0, "inteqtype", out tRec.inteqtype, TsTsSelRec.INTEQTYPE_SZ, out nullInds[TsTsSelRec.INTEQTYPE]);
                    Ssutil.DbGetString(hStmt, 0, "viceqtype", out tRec.viceqtype, TsTsSelRec.VICEQTYPE_SZ, out nullInds[TsTsSelRec.VICEQTYPE]);
                    Ssutil.DbGetDouble(hStmt, 0, "intbwchans", out tRec.intbwchans, out nullInds[TsTsSelRec.INTBWCHANS]);
                    Ssutil.DbGetDouble(hStmt, 0, "vicbwchans", out tRec.vicbwchans, out nullInds[TsTsSelRec.VICBWCHANS]);
                }
                catch (Exception e)
                {
                    Log2.e("\nTstsrp3.TsTsRp3(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                    Ssutil.DbGetDiagStmt(hStmt, "tstsrp303: Failed retrieving " + e.Message);
                    pl.Output();
                    pl.LeftAt(0, GenUtil.GetUserMess());
                    pl.Output();
                    break;
                }

                //...Log2.v("\nTstsrp3.TsTsRp3(): tRec = \n" + tRec.ToStringWN(nullInds));

                ReportTtDetail(ttName, tRec, tpParm, ohtrigger, pl);
            }

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            sqlRet = (SQLRETURN)Ssutil.DisConn(hConn);

            //...Log2.v("\nTstsrp3.TsTsRp3(): Exit");
            return 0;
        }

        /// <summary>
        /// This method produces the CASEDET text fragment one set of site, ante and channel data.   
        /// </summary>
        /// <param name="ttName"> - the unique ID substring of the Ts DB table names.</param>
        /// <param name="selRec"> - TsTsSelRec object providing results data.</param>
        /// <param name="tpParm"> - the TpParm object providing parameter data.</param>
        /// <param name="ohtrigger"> - flag = 1 for Line of sight, and Over-Horizon Loss cases only; flag = 0 for all cases.</param>
        /// <param name="pl"> - PrintLine object with header override.</param>
        private static void ReportTtDetail(string ttName, TsTsSelRec selRec, TpParm tpParm, int ohtrigger, PrintLine pl)
        {
            //...Log2.v("\nTstsrp3.ReportTtDetail(): Entry");

            if (!IsDupSite(selRec))
            {
                ReportTtSiteHeader(ttName, selRec, tpParm, ohtrigger, pl);
            }

            if (!IsDupAnte(selRec))
            {
                ReportTtAnteHeader(selRec, tpParm, ohtrigger, pl);
            }

            ReportttChanDetail(selRec, tpParm, ohtrigger, pl);

            //...Log2.v("\nTstsrp3.ReportTtDetail(): Exit");
        }

        /// <summary>
        /// This method detects whether the current case is a duplicate of a previous interference case.   
        /// </summary>
        /// <param name="selRec"> - TsTsSelRec object providing results data.</param>
        /// <returns></returns>
        private static bool IsDupSite(TsTsSelRec selRec)
        {
            bool bRet;

            if (selRec.caseno == lastCase)
            {
                bRet = true;
            }
            else
            {
                bRet = false;
                lastCase = selRec.caseno;
                lastSubCase = -1;
            }

            return bRet;
        }

        /// <summary>
        /// This method determines whether this antenna subcase is the same as the previous 
        /// subcase; if so, use that subcase instead. 
        /// </summary>
        /// <param name="selRec">- TsTsSelRec object providing results data.</param>
        /// <returns></returns>
        private static bool IsDupAnte(TsTsSelRec selRec)
        {
            bool bRet;

            if (selRec.subcaseno == lastSubCase)
            {
                bRet = true;
            }
            else
            {
                lastSubCase = selRec.subcaseno;
                bRet = false;
            }

            return bRet;
        }

        /// <summary>
        /// This method produces the text used for the Ts-Ts case Site header.    
        /// </summary>
        /// <param name="ttName"> - the unique ID substring of the Ts DB table names.</param>
        /// <param name="selRec"> - TsTsSelRec object providing results data.</param>
        /// <param name="tpParm"> - the TpParm object providing parameter data.</param>
        /// <param name="ohtrigger"> - flag = 1 for Line of sight, and Over-Horizon Loss cases only; flag = 0 for all cases.</param>
        /// <param name="pl"> - PrintLine object with header override.</param>
        private static void ReportTtSiteHeader(string ttName, TsTsSelRec selRec, TpParm tpParm, int ohtrigger, PrintLine pl)
        {
            string cDate;
            string cTime;
            string cBuff;

            pl.PageBefore();

            pl.LeftAt(0, "Frequency Coordination System Association");
            cBuff = String.Format("Page: {0:D}", pl.PageNumber);
            pl.RightAt(pl.LastPos(), cBuff);
            pl.Output();

            pl.LeftAt(0, "MASTER DATA BASE -- TSTS Interference Study Report - CASE DETAIL");
            GenUtil.UtGetDateTime(out cDate, out cTime);
            cBuff = String.Format("Date: {0,10}", cDate);
            pl.RightAt(pl.LastPos(), cBuff);
            pl.Output();
            pl.LeftAt(0, "Project Code [{0}]", Info.ProjectCode);
            pl.Output();

            if (ohtrigger == 0)
            {
                pl.LeftAt(0, "All Cases");
            }
            else
            {
                pl.LeftAt(0, "Line of sight, and over-horizon loss cases only");
            }
            cBuff = String.Format("Time: {0,10}", cTime);
            pl.RightAt(pl.LastPos(), cBuff);
            pl.Output();
            pl.Output();

            pl.LeftAt(0, "Case Number : ");
            pl.IntAt(-1, "{0,6:D}", selRec.caseno);
            if (selRec.interferer.Equals("P"))
            {
                pl.LeftAt(24, "Interferor from Proposed File");
            }
            else
            {
                pl.LeftAt(24, "Interferor from Environment File");
            }

            pl.LeftAt(68, "Environment  :");
            pl.LeftAt(84, tpParm.envtype);

            pl.LeftAt(100, "Coord. Dist.: ");
            pl.DoubleAt(114, "{0,6:F2}", tpParm.coordist);
            pl.LeftAt(-1, " km");

            pl.Output();

            pl.LeftAt(0, "Propagation Loss Model:");
            switch (tpParm.spherecalc[0])
            {
                case '1':
                    pl.LeftAt(24, "TSIP CCIR-SJM");
                    break;

                case '2':
                    pl.LeftAt(24, "Spherical Earth");
                    break;

                case '3':
                    pl.LeftAt(24, "Free Space");
                    break;

                case '4':
                    pl.LeftAt(24, "PCS-HATA");
                    break;

                case '5':
                    pl.LeftAt(24, "OH-LOSS");
                    break;

                default:
                    pl.LeftAt(24, "Don't Know");
                    break;
            }

            pl.LeftAt(68, "Analysis Type: ");
            pl.LeftAt(84, tpParm.analopt);
            pl.LeftAt(100, "PDF_Run     : ");
            pl.LeftAt(114, ttName);
            pl.Output();

            pl.LeftAt(0, "Maximum Frequency Separation : ");
            pl.DoubleAt(-1, "{0,6:F1}", tpParm.fsep);
            pl.LeftAt(68, "Margin       : ");
            pl.DoubleAt(84, "{0,5:F1}", tpParm.margin);
            pl.Output();

            pl.LeftAt(0, "+----");
            pl.LeftAt(41, "Site Geometry");
            pl.LeftAt(68, "|  Grnd");
            pl.LeftAt(78, "+-------- Site to Site --------+");
            pl.Output();

            pl.LeftAt(0, "Call I");
            pl.LeftAt(9, "Call Irx");
            pl.LeftAt(18, "|");
            pl.LeftAt(20, "Station  I --> Station  Irx");
            pl.LeftAt(52, "|");
            pl.LeftAt(54, "OP I ");
            pl.LeftAt(61, "OP Irx");
            pl.LeftAt(68, "| Int.");
            pl.LeftAt(78, "|");
            pl.LeftAt(80, "I-Irx Km");
            pl.LeftAt(89, "I-Vrx Km");
            pl.LeftAt(99, "I  Ang.DEG *");
            pl.LeftAt(112, "Angle");
            pl.Output();

            pl.LeftAt(0, "CALL Vrx");
            pl.LeftAt(9, "Call V");
            pl.LeftAt(18, "|");
            pl.LeftAt(20, "Station  Vrx --> Station  V");
            pl.LeftAt(52, "|");
            pl.LeftAt(54, "OP Vrx");
            pl.LeftAt(61, "OP V");
            pl.LeftAt(68, "| Victim");
            pl.LeftAt(78, "|");
            pl.LeftAt(80, "Vrx-V Km");
            pl.LeftAt(98, "Vrx Ang.DEG *");
            pl.LeftAt(112, "Ref.");
            pl.Output();

            pl.LeftAt(0, "+--------");
            pl.LeftAt(9, "---------");
            pl.LeftAt(19, "----------------");
            pl.LeftAt(36, "----------------");
            pl.LeftAt(54, "------");
            pl.LeftAt(61, "------");
            pl.LeftAt(68, "----------");
            pl.LeftAt(80, "----.---");
            pl.LeftAt(89, "----.---");
            pl.LeftAt(98, "-------.--- +");
            pl.LeftAt(111, "---------+");
            pl.Output();

            pl.LeftAt(0, selRec.intcall1);
            pl.LeftAt(19, selRec.intname1);
            pl.LeftAt(54, selRec.intoper);
            pl.DoubleAt(68, "{0,7:F0}", selRec.intgrnd);
            pl.LeftAt(-1, "m");
            pl.DoubleAt(81, "{0,6:F2}", selRec.int1int2dist);
            pl.DoubleAt(90, "{0,6:F2}", selRec.int1vic1dist);
            pl.DoubleAt(101, "{0,6:F1}", selRec.intoffax);
            if (selRec.intaoffax.Equals("Y"))
            {
                pl.LeftAt(111, "Offaxis");
            }
            else
            {
                pl.LeftAt(111, "Main Beam");
            }
            pl.Output();

            pl.LeftAt(9, selRec.intcall2);
            pl.LeftAt(19, "->");
            pl.LeftAt(22, selRec.intname2);
            pl.LeftAt(61, selRec.intoper2);
            pl.Output();

            pl.LeftAt(0, selRec.viccall1);
            pl.LeftAt(19, selRec.vicname1);
            pl.LeftAt(54, selRec.vicoper);
            pl.DoubleAt(68, "{0,7:F0}", selRec.vicgrnd);
            pl.LeftAt(-1, "m");
            pl.DoubleAt(81, "{0,6:F2}", selRec.vic1vic2dist);
            pl.DoubleAt(101, "{0,6:F1}", selRec.vicoffax);
            if (selRec.vicaoffax.Equals("Y"))
            {
                pl.LeftAt(111, "Offaxis");
            }
            else
            {
                pl.LeftAt(111, "Main Beam");
            }
            pl.Output();

            pl.LeftAt(9, selRec.viccall2);
            pl.LeftAt(19, "->");
            pl.LeftAt(22, selRec.vicname2);
            pl.LeftAt(61, selRec.vicoper2);
            pl.Output();

            return;
        }

        /// <summary>
        /// This method produces the text used for the Ts-Ts case Site header.   
        /// </summary>
        /// <param name="selRec"> - TsTsSelRec object providing results data.</param>
        /// <param name="tpParm"> - the TpParm object providing parameter data.</param>
        /// <param name="ohtrigger"> - flag = 1 for Line of sight, and Over-Horizon Loss cases only; flag = 0 for all cases.</param>
        /// <param name="pl"> - PrintLine object with header override.</param>
        private static void ReportTtAnteHeader(TsTsSelRec selRec, TpParm tpParm, int ohtrigger, PrintLine pl)
        {
            string cCI;
            bool IsCalc;

            pl.Output();
            pl.LeftAt(0, "Antenna Geometry: Interferer: Boresight   Elev:");
            pl.DoubleAt(-1, "{0,7:F2}", selRec.intelev);
            pl.LeftAt(-1, " Az:");
            pl.DoubleAt(-1, "{0,6:F1}", selRec.intantaz);
            pl.LeftAt(-1, "|   Victim: Boresight    Elev:");
            pl.DoubleAt(-1, "{0,6:F2}", selRec.vicelev);
            pl.LeftAt(-1, " Az:");
            pl.DoubleAt(-1, "{0,6:F1}", selRec.vicantaz);
            pl.Output();

            pl.LeftAt(0, "                  To Victim: Ant.Ht:");
            pl.DoubleAt(-1, "{0,4:F0}", selRec.intaht);
            pl.LeftAt(-1, "m Elev:");
            pl.DoubleAt(-1, "{0,7:F2}", selRec.intvicel);
            pl.LeftAt(-1, " Az:");
            pl.DoubleAt(-1, "{0,6:F1}", selRec.intvicaz);
            pl.LeftAt(-1, "|   To Int: Ant.Ht:");
            pl.DoubleAt(-1, "{0,4:F0}", selRec.vicaht);
            pl.LeftAt(-1, "m Elev:");
            pl.DoubleAt(-1, "{0,6:F2}", selRec.vicintel);
            pl.LeftAt(-1, " Az:");
            pl.DoubleAt(-1, "{0,6:F1}", selRec.vicintaz);
            pl.Output();

            pl.LeftAt(0, "                  Off-axis Angle:");
            pl.DoubleAt(-1, "{0,6:F1}", selRec.intoffantax);
            pl.LeftAt(-1, " deg.                    |   Off-Axis Angle:");
            pl.DoubleAt(-1, "{0,6:F1}", selRec.vicoffantax);
            pl.Output();

            pl.LeftAt(0, "Antenna #: ");
            pl.IntAt(-1, "{0,4:D}", selRec.intanum);
            pl.LeftAt(24, "AUse: ");
            pl.LeftAt(-1, selRec.intause);
            pl.DoubleAt(36, "Gain:{0,6:F2}", selRec.intgain);
            pl.LeftAt(48, "dB");
            pl.LeftAt(64, "|");
            pl.IntAt(68, "Antenna #: {0,4:D}", selRec.vicanum);
            pl.LeftAt(92, "AUse: ");
            pl.LeftAt(-1, selRec.vicause);
            pl.LeftAt(105, "Gain:");
            pl.DoubleAt(-1, "{0,6:F2}", selRec.vicgain);
            pl.LeftAt(117, "dB");
            pl.Output();

            pl.LeftAt(0, "TX.Ant.Code: ");
            pl.LeftAt(-1, selRec.intacode);
            pl.LeftAt(27, "Discr. H CPol:");
            pl.DoubleAt(-1, "{0,6:F2} ", selRec.adiscctxh);
            pl.LeftAt(-1, "dB ___");
            pl.LeftAt(64, "|");
            pl.LeftAt(68, "RX.Ant.Code: ");
            pl.LeftAt(-1, selRec.vicacode);
            pl.LeftAt(96, "Discr. H CPol:");
            pl.DoubleAt(-1, "{0,6:F2} ", selRec.adisccrxh);
            pl.LeftAt(-1, "dB ___");
            pl.Output();

            pl.LeftAt(0, "X Ref. Ant : ");
            if (selRec.intaxref.Equals(""))
            {
                pl.LeftAt(-1, selRec.intacode);
            }
            else
            {
                pl.LeftAt(-1, selRec.intaxref);
            }
            pl.LeftAt(27, "Discr. V CPol:");
            pl.DoubleAt(-1, "{0,6:F2} dB ___", selRec.adiscctxv);
            pl.LeftAt(64, "|");
            pl.LeftAt(68, "X Ref. Ant : ");
            if (selRec.vicaxref.Equals(""))
            {
                pl.LeftAt(-1, selRec.vicacode);
            }
            else
            {
                pl.LeftAt(-1, selRec.vicaxref);
            }
            pl.LeftAt(96, "Discr. V CPol:");
            pl.DoubleAt(-1, "{0,6:F2} dB ___", selRec.adisccrxv);
            pl.Output();

            pl.LeftAt(0, "Model: ");
            pl.LeftAt(-1, selRec.intamodel);
            pl.LeftAt(27, "Discr. H XPol:");
            pl.DoubleAt(-1, "{0,6:F2} dB ___", selRec.adiscxtxh);
            pl.LeftAt(64, "|");
            pl.LeftAt(68, "Model: ");
            pl.LeftAt(-1, selRec.vicamodel);
            pl.LeftAt(96, "Discr. H XPol:");
            pl.DoubleAt(-1, "{0,6:F2} dB ___", selRec.adiscxrxh);
            pl.Output();

            pl.LeftAt(0, "Offaxis Angle:");
            pl.DoubleAt(-1, "{0,6:F1} deg.", selRec.intoffantax);
            pl.LeftAt(27, "Discr. V XPol:");
            pl.DoubleAt(-1, "{0,6:F2} dB ___", selRec.adiscxtxv);
            pl.LeftAt(64, "+");
            pl.DoubleAt(68, "Offaxis Angle:{0,6:F1} deg.", selRec.vicoffantax);
            pl.DoubleAt(96, "Discr. V XPol:{0,6:F2} dB ___", selRec.adiscxrxv);
            pl.Output();

            if (selRec.intaoffax.Equals("Y"))
            {
                pl.DoubleAt(0, "True Azimuth of Antenna: {0,5:F1}", selRec.intantaz);
                //AH: EXTANT BUG - should be %5.1f ?
                pl.DoubleAt(-1, " deg. Azimuth of Hop: {0,5:F1} deg.", selRec.inthopaz);
            }

            if (selRec.vicaoffax.Equals("Y"))
            {
                pl.DoubleAt(68, "True Azimuth of Antenna: {0,5:F1}", selRec.vicantaz);
                pl.DoubleAt(-1, " deg. Azimuth of Hop: {0,5:F1} deg.", selRec.vichopaz);
            }

            if (selRec.intaoffax.Equals("Y") || selRec.vicaoffax.Equals("Y"))
            {
                pl.Output();
            }

            pl.Output();

            pl.LeftAt(0, "-- TX Interferer Sys.Eqp.CTX.Band Info  - Station I --- TS --- ");
            pl.LeftAt(63, "*+*");
            pl.LeftAt(68, "-- RX Victim Sys.Eqp.CTX.Band Info ----- Station Vrx --- TS --");
            pl.Output();

            pl.LeftAt(0, "Eqpt. Planned / Traffic: ");
            pl.LeftAt(-1, selRec.inteqpttx);
            pl.LeftAt(-1, "  ");
            pl.LeftAt(-1, selRec.inttraftx);

            /*	Put in the calculation information if needed.  The following idiom on
            *		calctype is used to indicate that calculations (C or I) were used rather
            *		than CTX curves (C/I or -I). */
            cCI = selRec.calctype.Trim();
            IsCalc = cCI.Length < 2;

            if (IsCalc)
            {
                /*	This is a calculated case */
                if (selRec.inteqtype.Equals("D"))
                {
                    pl.LeftAt(50, "Digital");
                }
                else
                {
                    pl.LeftAt(50, "Analog");
                }
            }

            pl.LeftAt(64, "|");
            pl.LeftAt(68, "Eqpt. Planned / Traffic: ");
            pl.LeftAt(-1, selRec.viceqptrx);
            pl.LeftAt(-1, "  ");
            pl.LeftAt(-1, selRec.victrafrx);

            if (IsCalc)
            {
                /*	This is a calculated case */
                if (selRec.viceqtype.Equals("D"))
                {
                    pl.LeftAt(120, "Digital");
                }
                else
                {
                    pl.LeftAt(120, "Analog");
                }
            }

            pl.Output();

            pl.LeftAt(0, "Eqpt. X Ref.  / Traffic: ");
            if (IsCalc)
            {
                pl.LeftAt(-1, selRec.ctxinteqpt);
            }
            else
            {
                pl.LeftAt(-1, "-");
            }

            pl.LeftAt(35, selRec.ctxinttraftx);

            if (IsCalc)
            {
                if (selRec.intbwchans > 0)
                {
                    pl.DoubleAt(43, "*B/W Used: {0,6:F2}", selRec.intbwchans);
                }
            }

            pl.LeftAt(64, "|");
            pl.LeftAt(68, "Eqpt. X Ref.  / Traffic: ");
            pl.LeftAt(-1, selRec.ctxeqpt);
            pl.LeftAt(-1, "  ");
            pl.LeftAt(-1, selRec.ctxvictrafrx);

            if (IsCalc)
            {
                if (selRec.vicbwchans > 0)
                {
                    if (selRec.viceqtype.Equals("D"))
                    {
                        pl.DoubleAt(111, "*B/W Used: {0,6:F2}", selRec.vicbwchans);
                    }
                    else
                    {
                        pl.DoubleAt(111, "*Channels: {0,6:F2}", selRec.vicbwchans);
                    }
                }
            }

            pl.Output();

            pl.LeftAt(11, "TX.Freq.Bnd : ");
            pl.LeftAt(-1, selRec.intbndcde);
            pl.LeftAt(64, "+");
            pl.LeftAt(79, "RX.Freq.Bnd : ");
            pl.LeftAt(-1, selRec.vicbndcde);
            pl.Output();

            /* Case information */
            pl.LeftAt(0, "SC");
            pl.Output();

            pl.LeftAt(0, "UA");
            pl.LeftAt(5, "TX.Freq.");
            pl.LeftAt(17, "P");
            pl.LeftAt(20, "RX.Freq.");
            pl.LeftAt(32, "P");
            pl.LeftAt(34, "Chn");
            pl.LeftAt(39, "Freq.");
            pl.LeftAt(46, "Int.");
            pl.LeftAt(52, "Int.");
            pl.LeftAt(57, "Vic.");
            pl.LeftAt(62, "Vic.");
            if (IsCalc)
            {
                pl.LeftAt(116, " CALC");
            }
            else
            {
                pl.LeftAt(116, "TABLE");
            }
            pl.Output();

            pl.LeftAt(0, "BS");
            pl.LeftAt(6, "(MHz)");
            pl.LeftAt(17, "O");
            pl.LeftAt(21, "(MHz)");
            pl.LeftAt(32, "O");
            pl.LeftAt(34, "Sta");
            pl.LeftAt(39, "Sep.");
            pl.LeftAt(46, "TXPwr");
            pl.LeftAt(52, "AFSL");
            pl.LeftAt(57, "AFSL");
            pl.LeftAt(62, "RXPwr");
            pl.LeftAt(70, "Tilt");
            pl.LeftAt(76, "TANT");
            pl.LeftAt(82, "T");
            pl.LeftAt(85, "CALC PATH");
            pl.LeftAt(97, "CALC  C/I  OR  -I");
            pl.LeftAt(117, "REQD.");
            pl.LeftAt(124, "MARGIN");
            pl.Output();

            pl.LeftAt(0, " E");
            pl.LeftAt(6, "Stn.I");
            pl.LeftAt(17, "L");
            pl.LeftAt(20, "Stn.Vrx");
            pl.LeftAt(32, "L");
            pl.LeftAt(34, "T.R");
            pl.LeftAt(39, "(MHz)");
            pl.LeftAt(46, "(dBm)");
            pl.LeftAt(52, "(dB)");
            pl.LeftAt(57, "(dB)");
            pl.LeftAt(62, "(dBm)");
            pl.LeftAt(70, "DISC");
            pl.LeftAt(76, "DISC");
            pl.LeftAt(82, "X");
            pl.LeftAt(88, "LOSS");
            pl.LeftAt(100, "CPol     XPol");
            pl.LeftAt(118, "(dB)");
            pl.LeftAt(126, "(dB)");
            pl.Output();

            pl.LeftAt(0, "--- ------.----- - ------.----- - -.- ----.--- --.-- --.- --.- ---.-- --.-- --.--");
            pl.LeftAt(82, "- ------ ---.- -----.-- -----.--  ----.-  ----.-");
            pl.Output();

        }

        /// <summary>
        /// This method produces the text used for the Ts-Ts case Chan header.   
        /// </summary>
        /// <param name="selRec"> - TsTsSelRec object providing results data.</param>
        /// <param name="tpParm"> - the TpParm object providing parameter data.</param>
        /// <param name="ohtrigger"> - flag = 1 for Line of sight, and Over-Horizon Loss cases only; flag = 0 for all cases.</param>
        /// <param name="pl"> - PrintLine object with header override.</param>
        private static void ReportttChanDetail(TsTsSelRec selRec, TpParm tpParm, int ohtrigger, PrintLine pl)
        {
            //...Log2.v("\nTstsrp3.ReportttChanDetail(): Entry");

            string coleft;
            string coright;
            string xpleft;
            string xpright;

            Enums.MapUsed eMapUsed = Enums.MapUsed.e250K;

            pl.IntAt(0, "{0,3:D}", selRec.subcaseno);
            pl.RightAt(14, GenUtil.Ccommas(selRec.intfreqtxr, 4));
            pl.LeftAt(17, selRec.intpolar);
            pl.RightAt(29, GenUtil.Ccommas(selRec.vicfreqrxr, 4));
            pl.LeftAt(32, selRec.vicpolar);
            pl.LeftAt(34, selRec.intstattx);
            pl.LeftAt(36, selRec.vicstatrx);
            pl.DoubleAt(37, "{0,9:F3}", selRec.freqsepr);
            pl.DoubleAt(46, "{0,6:F2}", selRec.intpwrtx);
            pl.DoubleAt(53, "{0,4:F1}", selRec.intafsltx);
            pl.DoubleAt(58, "{0,4:F1}", selRec.vicafslrx);
            pl.DoubleAt(63, "{0,6:F2}", selRec.vicpwrrx);
            pl.DoubleAt(69, "{0,6:F2}", selRec.tiltdisc);
            pl.DoubleAt(75, "{0,6:F2}", selRec.totantdisc);
            if (selRec.intpolar.Equals("B") && selRec.vicpolar.Equals("B"))
            {
                /*
                *	In the special case of B into B we output the interference in a special format.  We want
                *	to show both the H and V from the interferer separately.  This means there will be two lines
                *	for each normal line, an H line and a V line.
                */
                pl.LeftAt(82, "H"); //	First the horizontal polarization for FSL.

                //	Calculate, the base signal, the signal with FSL, and then the H and V copolar and crosspolar discriminations.
                double dBase = selRec.intpwrtx - selRec.intafsltx + selRec.intgain + selRec.vicgain - selRec.vicafslrx;

                //	The first line, first column is tx H copolar + rx H copolar
                double dHCoPol = selRec.adiscctxh + selRec.adisccrxh;
                //	second column is tx H crosspolar + rx V copolar
                double dHXPol = selRec.adiscxtxh + selRec.adisccrxv;
                //	The second line, first column is tx V copolar + rx V copolar
                double dVCoPol = selRec.adiscctxv + selRec.adisccrxv;
                //	second column is tx V crosspolar + rx H copolar
                double dVXPol = selRec.adiscxtxv + selRec.adisccrxh;
                //	The basis minus the path loss.
                double dBaseWPath = dBase - selRec.patloss;
                bool IsCoverI = false;

                //	Now we have the basic interference level without the antenna discrimination.
                //	This is all we need for the an interference calculation (I or -I), but if the
                //	CTX requirement is for a C/I or C calculation, we need to calculate the C/I ratio.
                //	The antenna discriminations and path loss are part of the interference.  They 
                //	will be subtracted in the interference cases, but added for the C/I case (since
                //	they are subtracted from the interference in the denominator).
                if (Strings.FirstCharIs(selRec.calctype, 'C'))
                {
                    //	Calculate the C/I ratios
                    dBaseWPath = selRec.vicpwrrx - dBase + selRec.patloss;
                    IsCoverI = true;
                }

                double dMargin;
                double dSelect;

                if (dHCoPol < dHXPol)
                {
                    ParenthesizeLowerNumber(out coleft, out coright, out xpleft, out xpright);
                    dSelect = dHCoPol;  //	select this for the margin.
                }
                else
                {
                    ParenthesizeUpperNumber(out coleft, out coright, out xpleft, out xpright);
                    dSelect = dHXPol;
                }

                /*  This is new format for both OHloss and other calculation -- task 1069 */
                switch (tpParm.spherecalc[0])
                {
                    case '1':
                        pl.LeftAt(84, "CCIR");
                        break;

                    case '2':
                        pl.LeftAt(84, "SEL");
                        break;

                    case '3':
                        pl.LeftAt(84, "FSL");
                        break;

                    case '4':
                        pl.LeftAt(84, "HATA");
                        break;

                    case '5':
                        pl.LeftAt(84, "FSL");
                        break;

                    default:
                        break;
                }

                pl.DoubleAt(91, "{0,5:F1}", selRec.patloss);

                pl.LeftAt(97, coleft);
                pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + (IsCoverI ? dHCoPol : -dHCoPol)); // if -I subtract antenna disc, if C/I, add
                pl.LeftAt(-1, coright);
                pl.LeftAt(106, xpleft);
                pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + (IsCoverI ? dHXPol : -dHXPol));  // selRec.calcixp);
                pl.LeftAt(-1, xpright);

                pl.DoubleAt(116, "{0,6:F1}", selRec.reqdcalc);
                if (IsCoverI)
                {
                    dMargin = (dBaseWPath + dSelect) - selRec.reqdcalc; //	Get the full margin.
                }
                else
                {
                    dMargin = selRec.reqdcalc - (dBaseWPath - dSelect); //	Get the full margin.
                }
                pl.DoubleAt(124, "{0,6:F1}", dMargin);//selRec.resti);

                pl.Output();

                //	Next line has the V values.

                if (dVCoPol < dVXPol)
                {
                    ParenthesizeLowerNumber(out coleft, out coright, out xpleft, out xpright);
                    dSelect = dVCoPol;
                }
                else
                {
                    ParenthesizeUpperNumber(out coleft, out coright, out xpleft, out xpright);
                    dSelect = dVXPol;
                }

                pl.DoubleAt(75, "{0,6:F2}", dSelect);  // V total antenna discrimination.

                pl.LeftAt(82, "V"); //	Now the Vertical polarization for FSL.

                pl.LeftAt(97, coleft);
                pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + (IsCoverI ? dVCoPol : -dVCoPol)); // selRec.calcico);
                pl.LeftAt(-1, coright);
                pl.LeftAt(106, xpleft);
                pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + (IsCoverI ? dVXPol : -dVXPol));  // selRec.calcixp);
                pl.LeftAt(-1, xpright);

                pl.DoubleAt(116, "{0,6:F1}", selRec.reqdcalc);
                if (IsCoverI)
                {
                    dMargin = (dBaseWPath + dSelect) - selRec.reqdcalc; //	Get the full margin.
                }
                else
                {
                    dMargin = selRec.reqdcalc - (dBaseWPath - dSelect); //	Get the full margin.
                }
                pl.DoubleAt(124, "{0,6:F1}", dMargin);  // selRec.resti);

                pl.Output();


                if (tpParm.spherecalc.Equals("5"))
                {
                    //	If this is for ohloss, then we have four more lines to print.
                    /*  We ignore the following if there was an error, or it was free space
                    *   loss (i.e. no obstructions found. */
                    if (selRec.ohresult >= 1000)
                    {
                        eMapUsed = Enums.MapUsed.e50K;
                        selRec.ohresult -= 1000;
                    }
                    if (selRec.ohresult > 0 && selRec.ohresult < 100)
                    {
                        switch (selRec.ohresult)
                        {
                            case 1:
                                pl.RightAt(79, "Single Knife Edge");
                                break;

                            case 2:
                                pl.RightAt(79, "Rounded Isolated Obstacle");
                                break;

                            case 3:
                                pl.RightAt(79, "Double Knife Edge");
                                break;

                            case 4:
                                pl.RightAt(79, "Irregular Terrain");
                                break;

                            default:
                                pl.RightAt(79, "Unknown Terrain");
                                break;
                        }

                        //	Now do the 80%
                        dBaseWPath = dBase - selRec.pathloss80;
                        //	Handle the C/I case
                        if (IsCoverI)
                        {
                            //	Calculate the C/I ratios
                            dBaseWPath = selRec.vicpwrrx - dBase + selRec.pathloss80;
                        }

                        if (dHCoPol < dHXPol)
                        {
                            ParenthesizeLowerNumber(out coleft, out coright, out xpleft, out xpright);
                            dSelect = dHCoPol;
                        }
                        else
                        {
                            ParenthesizeUpperNumber(out coleft, out coright, out xpleft, out xpright);
                            dSelect = dHXPol;
                        }

                        pl.LeftAt(82, "H");

                        pl.LeftAt(84, "80.00%");
                        pl.DoubleAt(91, "{0,5:F1}", selRec.pathloss80);
                        pl.LeftAt(97, coleft);
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + (IsCoverI ? dHCoPol : -dHCoPol)); //	selRec.calcico80);
                        pl.LeftAt(-1, coright);
                        pl.LeftAt(106, xpleft);
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + (IsCoverI ? dHXPol : -dHXPol));  // selRec.calcixp80);
                        pl.LeftAt(-1, xpright);

                        pl.DoubleAt(116, "{0,6:F1}", selRec.reqd80);
                        if (IsCoverI)
                        {
                            dMargin = (dBaseWPath + dSelect) - selRec.reqd80;   //	Get the full margin.
                        }
                        else
                        {
                            dMargin = selRec.reqd80 - (dBaseWPath - dSelect);   //	Get the full margin.
                        }
                        pl.DoubleAt(124, "{0,6:F1}", dMargin);   // selRec.resti80);
                        pl.Output();

                        if (eMapUsed == Enums.MapUsed.e250K)
                        {
                            pl.RightAt(79, "1:250K Maps Used.");
                        }
                        else
                        {
                            pl.RightAt(79, "1:50K Maps Used.");
                        }

                        if (dVCoPol < dVXPol)
                        {
                            ParenthesizeLowerNumber(out coleft, out coright, out xpleft, out xpright);
                            dSelect = dVCoPol;
                        }
                        else
                        {
                            ParenthesizeUpperNumber(out coleft, out coright, out xpleft, out xpright);
                            dSelect = dVXPol;
                        }

                        //	Now the V line for 80%
                        pl.LeftAt(82, "V");

                        pl.LeftAt(97, coleft);
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + (IsCoverI ? dVCoPol : -dVCoPol)); //	selRec.calcico80);
                        pl.LeftAt(-1, coright);
                        pl.LeftAt(106, xpleft);
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + (IsCoverI ? dVXPol : -dVCoPol));  // selRec.calcixp80);
                        pl.LeftAt(-1, xpright);

                        pl.DoubleAt(116, "{0,6:F1}", selRec.reqd80);
                        if (IsCoverI)
                        {
                            dMargin = (dBaseWPath + dSelect) - selRec.reqd80;   //	Get the full margin.
                        }
                        else
                        {
                            dMargin = selRec.reqd80 - (dBaseWPath - dSelect);   //	Get the full margin.
                        }
                        pl.DoubleAt(124, "{0,6:F1}", dMargin);   // selRec.resti80);
                        pl.Output();

                        //	Now the H line for 99.99%
                        dBaseWPath = dBase - selRec.pathloss99;
                        if (IsCoverI)
                        {
                            //	Calculate the C/I ratios
                            dBaseWPath = selRec.vicpwrrx - dBase + selRec.pathloss99;
                        }

                        if (dHCoPol < dHXPol)
                        {
                            ParenthesizeLowerNumber(out coleft, out coright, out xpleft, out xpright);
                            dSelect = dHCoPol;
                        }
                        else
                        {
                            ParenthesizeUpperNumber(out coleft, out coright, out xpleft, out xpright);
                            dSelect = dHXPol;
                        }

                        pl.LeftAt(82, "H");

                        pl.LeftAt(84, "99.99%");
                        pl.DoubleAt(91, "{0,5:F1}", selRec.pathloss99);
                        pl.LeftAt(97, coleft);
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + (IsCoverI ? dHCoPol : -dHCoPol)); // selRec.calcico99);
                        pl.LeftAt(-1, coright);
                        pl.LeftAt(106, xpleft);
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + (IsCoverI ? dHXPol : -dHXPol));  // selRec.calcixp99);
                        pl.LeftAt(-1, xpright);

                        pl.DoubleAt(116, "{0,6:F1}", selRec.reqd99);
                        if (IsCoverI)
                        {
                            dMargin = (dBaseWPath + dSelect) - selRec.reqd99;   //	Get the full margin.
                        }
                        else
                        {
                            dMargin = selRec.reqd99 - (dBaseWPath - dSelect);   //	Get the full margin.
                        }
                        pl.DoubleAt(124, "{0,6:F1}", dMargin); // selRec.resti99);

                        pl.Output();

                        //	Now the V line for 99.99%
                        if (dVCoPol < dVXPol)
                        {
                            ParenthesizeLowerNumber(out coleft, out coright, out xpleft, out xpright);
                            dSelect = dVCoPol;
                        }
                        else
                        {
                            ParenthesizeUpperNumber(out coleft, out coright, out xpleft, out xpright);
                            dSelect = dVXPol;
                        }

                        pl.LeftAt(82, "V");

                        pl.LeftAt(97, coleft);
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + (IsCoverI ? dVCoPol : -dVCoPol)); // selRec.calcico99);
                        pl.LeftAt(-1, coright);
                        pl.LeftAt(106, xpleft);
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + (IsCoverI ? dVXPol : -dVXPol));  // selRec.calcixp99);
                        pl.LeftAt(-1, xpright);

                        pl.DoubleAt(116, "{0,6:F1}", selRec.reqd99);
                        if (IsCoverI)
                        {
                            dMargin = (dBaseWPath + dSelect) - selRec.reqd99;   //	Get the full margin.
                        }
                        else
                        {
                            dMargin = selRec.reqd99 - (dBaseWPath - dSelect);   //	Get the full margin.
                        }
                        pl.DoubleAt(124, "{0,6:F1}", dMargin); // selRec.resti99);

                    }
                    else if (selRec.ohresult == 0)
                    {
                        /*  This is line of sight. */
                        string map = eMapUsed == Enums.MapUsed.e250K ? "250K" : "50K";
                        string str = String.Format("Line of Sight on {0} maps -- Only reporting Free Space Loss", map);
                        pl.RightAt(79, str);
                    }
                    else if (selRec.ohresult == 100)
                    {
                        pl.RightAt(79, "*** Colocated Sites! ***");
                    }
                    else
                    {
                        pl.RightAt(79, "Terrain Profiling returned error: ");
                        pl.IntAt(-1, "{0:D}", -selRec.ohresult);
                    }
                }
            }
            else
            {
                //	This is the normal case, Not B into B polarizations, which are a special case.

                pl.LeftAt(82, selRec.intpolar);

                //.if intpolar = vicpolar or intpolar = "B" or  vicpolar = "B" .then
                if (selRec.intpolar.Equals(selRec.vicpolar) ||
                    selRec.intpolar.Equals("B") ||
                    selRec.vicpolar.Equals("B"))
                {
                    ParenthesizeLowerNumber(out coleft, out coright, out xpleft, out xpright);
                }
                else
                {
                    ParenthesizeUpperNumber(out coleft, out coright, out xpleft, out xpright);
                }

                /*  This is new format for both OHloss and other calculation -- task 1069 */
                switch (tpParm.spherecalc[0])
                {
                    case '1':
                        pl.LeftAt(84, "CCIR");
                        break;

                    case '2':
                        pl.LeftAt(84, "SEL");
                        break;

                    case '3':
                        pl.LeftAt(84, "FSL");
                        break;

                    case '4':
                        pl.LeftAt(84, "HATA");
                        break;

                    case '5':
                        pl.LeftAt(84, "FSL");
                        break;

                    default:
                        break;
                }

                pl.DoubleAt(91, "{0,5:F1}", selRec.patloss);
                pl.LeftAt(97, coleft);
                pl.DoubleAt(-1, "{0,6:F1}", selRec.calcico);
                pl.LeftAt(-1, coright);
                pl.LeftAt(106, xpleft);
                pl.DoubleAt(-1, "{0,6:F1}", selRec.calcixp);
                pl.LeftAt(-1, xpright);

                pl.DoubleAt(116, "{0,6:F1}", selRec.reqdcalc);
                pl.DoubleAt(124, "{0,6:F1}", selRec.resti);

                if (tpParm.spherecalc.Equals("5"))
                {
                    pl.Output();

                    /*  We ignore the following if there was an error, or it was free space
                    *   loss (i.e. no obstructions found. */
                    if (selRec.ohresult >= 1000)
                    {
                        eMapUsed = Enums.MapUsed.e50K;
                        selRec.ohresult -= 1000;
                    }
                    if (selRec.ohresult > 0 && selRec.ohresult < 100)
                    {
                        switch (selRec.ohresult)
                        {
                            case 1:
                                pl.RightAt(79, "Single Knife Edge");
                                break;

                            case 2:
                                pl.RightAt(79, "Rounded Isolated Obstacle");
                                break;

                            case 3:
                                pl.RightAt(79, "Double Knife Edge");
                                break;

                            case 4:
                                pl.RightAt(79, "Irregular Terrain");
                                break;

                            default:
                                pl.RightAt(79, "Unknown Terrain");
                                break;
                        }
                        pl.LeftAt(84, "80.00%");
                        pl.DoubleAt(91, "{0,5:F1}", selRec.pathloss80);
                        pl.LeftAt(97, coleft);
                        pl.DoubleAt(-1, "{0,6:F1}", selRec.calcico80);
                        pl.LeftAt(-1, coright);
                        pl.LeftAt(106, xpleft);
                        pl.DoubleAt(-1, "{0,6:F1}", selRec.calcixp80);
                        pl.LeftAt(-1, xpright);

                        pl.DoubleAt(116, "{0,6:F1}", selRec.reqd80);
                        pl.DoubleAt(124, "{0,6:F1}", selRec.resti80);

                        pl.Output();

                        if (eMapUsed == Enums.MapUsed.e250K)
                        {
                            pl.RightAt(79, "1:250K Maps Used.");
                        }
                        else
                        {
                            pl.RightAt(79, "1:50K Maps Used.");
                        }

                        pl.LeftAt(84, "99.99%");
                        pl.DoubleAt(91, "{0,5:F1}", selRec.pathloss99);
                        pl.LeftAt(97, coleft);
                        pl.DoubleAt(-1, "{0,6:F1}", selRec.calcico99);
                        pl.LeftAt(-1, coright);
                        pl.LeftAt(106, xpleft);
                        pl.DoubleAt(-1, "{0,6:F1}", selRec.calcixp99);
                        pl.LeftAt(-1, xpright);

                        pl.DoubleAt(116, "{0,6:F1}", selRec.reqd99);
                        pl.DoubleAt(124, "{0,6:F1}", selRec.resti99);
                    }
                    else if (selRec.ohresult == 0)
                    {
                        /*  This is line of sight. */
                        string map = eMapUsed == Enums.MapUsed.e250K ? "250K" : "50K";
                        string str = String.Format("Line of Sight on {0} maps -- Only reporting Free Space Loss", map);
                        pl.RightAt(79, str);
                    }
                    else if (selRec.ohresult == 100)
                    {
                        pl.RightAt(79, "*** Colocated Sites! ***");
                    }
                    else
                    {
                        pl.RightAt(79, "Terrain Profiling returned error: ");
                        pl.IntAt(-1, "{0:D}", -selRec.ohresult);
                    }
                }
            }

            pl.Output();

            //...Log2.v("\nTstsrp3.ReportttChanDetail(): Exit");
            return;
        }

        /// <summary>
        /// This is a 'helper' method that returns the appropriate ' ', '(' and ')' characters
        /// to decorate a pair of numbers; the parentheses are around the lower number in a dyad.
        /// </summary>
        /// <param name="coleft"> - returns a space character.</param>
        /// <param name="coright"> - returns a space character.</param>
        /// <param name="xpleft"> - returns a '(' character.</param>
        /// <param name="xpright"> - returns a ')' character.</param>
        public static void ParenthesizeLowerNumber(out string coleft, out string coright, out string xpleft, out string xpright)
        {
            //	Put the parentheses around the lower number.
            coleft = SPACE;
            coright = SPACE;
            xpleft = LEFT_PARENTHESIS;
            xpright = RIGHT_PARENTHESIS;
        }

        /// <summary>
        /// This is a 'helper' method that returns the appropriate ' ', '(' and ')' characters
        /// to decorate a pair of numbers; the parentheses are around the upper number in a dyad.
        /// </summary>
        /// <param name="coleft"> - returns a '(' character.</param>
        /// <param name="coright"> - returns a ')' character.</param>
        /// <param name="xpleft"> - returns a space character.</param>
        /// <param name="xpright"> - returns a space character.</param>
        public static void ParenthesizeUpperNumber(out string coleft, out string coright, out string xpleft, out string xpright)
        {
            //	Put the parentheses around the lower number.
            coleft = LEFT_PARENTHESIS;
            coright = RIGHT_PARENTHESIS;
            xpleft = SPACE;
            xpright = SPACE;
        }


    }
}

```
