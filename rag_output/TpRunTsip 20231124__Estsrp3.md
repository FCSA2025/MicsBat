# Documented File: Estsrp3.cs
**Repository Path:** `TpRunTsip 20231124\Estsrp3.cs`
**Primary Layer:** `TpRunTsip 20231124`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using _Configuration;
using _NewLib;
using _Utillib;
using _DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace TpRunTsip
{
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
    /// Provides methods used to generate the Case Detail (.CASEDET) for es-ts.
    /// </summary>
    public class Estsrp3
    {
        private static string mFile;
        private static EsTsSelRec tRec;
        private static int mLastCase;
        private static int mLastSubCase;
        private class PrintLineEsTs3 : PrintLine
        {
            /// <summary>
            /// This method overrides the base class's PageHeader method
            /// and writes a page header appropriate for a CASEDET report.
            /// </summary>
            /// <param name=""></param>
            public override void PageHeader()
            {
                PageBefore();

                EsTs3Hdr(this, tRec);
            }
        }

        /// <summary>
        /// This method manages the production of the Case Detail (.CASEDET) for es-ts.
        /// </summary>
        /// <param name="tw"> - the TextWriter object assigned to the .CASEDET report.</param>
        /// <param name="ttName"> - the unique ID substring of the Te DB table names.</param>
        /// <param name="tpParm"> - the TpParm object providing source data.</param>
        /// <returns></returns>
        public static int EsTsRp3(TextWriter tw, string ttName, TpParm tpParm)
        {
            //...Log2.v("\nEstsrp3.EsTsRp3(): Entry");

            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            SQLLEN nullInd;

            string cSQL;

            EsTsSelRec pRec = new EsTsSelRec();

            //tRec is 'global' and so can be accessed by the PageHeader override.
            tRec = pRec;

            PrintLineEsTs3 pl = new PrintLineEsTs3();
            pl.OutFile(tw);
            pl.FormFeed(); // Just to mimic the behaviour of the MICS legacy C++ code.

            EsInitCase();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            //mFile is 'global' and so can be accessed by the PageHeader override.
            mFile = ttName;

            cSQL = String.Format("select protype,envtype,coordist,fsep,analopt,spherecalc,margin,a.terrcall1,a.terrcall2,a.earthlocation,a.terrname1,a.terrname2,a.earthname,terroper,terroper2,earthoper,terrlatit,terrlongit,terrgrnd,earthlatit,earthlongit,earthgrnd,radiozone,rainzone,a.etreport,a.tereport ,etcaseno,tecaseno,etsubcases,tesubcases,intreq,etdist,etazim,teazim,tudist,tuazim,utazim,eudist,euazim,ueazim,a.processed,b.interferer,b.terrbndcde,b.terranum,b.earthcall1,b.earthband,terracode,earthacode,satname,satoper,satlongit,txpre,txtro,rxpre,rxtro,sarc1,sarc2,mode1,mode2,intause,b.etreport,b.tereport,b.processed,etsubcaseno,tesubcaseno,esazim,eselev,teelev,etelev,tuelev,utelev,euelev,ediscang,tdiscang,adisc_set,adisc_ute,terrht,earthht,tvazim,evazim,tvelev,evelev,tvdistes,tvdisttu,evdistes,evdisttu,angleutv,anglesev,terrchid,earthchid,inttraftx,victrafrx,inteqpttx,viceqptrx,intfreqtx,vicfreqrx,freqsep,inttxpwr,inttxafls,inttxpwr2,inttxafls2,vicpwrrx,vicrxafls,stattx,statrx,c.etreport,c.tereport,ctxinttraftx,ctxvictrafrx,ctxeqpt,calctype,earthmdsc,terrmdsc,eartheirp,terreirp,scang,loss20mode1,calci20mode1,reqd20mode1,marg20mode1,loss01mode1,calci01mode1,reqd01mode1,marg01mode1,loss01mode2,calci01mode2,reqd01mode2,marg01mode2,energy,c.processed,terrant,terraxref,terramodel,terragain,earthaxref,earthamodel,earthagain,remterragain,tsoffaxis,tstrueaz,tstrueel,angleuta,angleeta,angleatv,adisc_atv FROM 	{0}.te_{1}_site a, {0}.te_{2}_ante b, {0}.te_{3}_chan c, {0}.te_{4}_parm WHERE a.terrcall1 = b.terrcall1 and a.terrcall2 = b.terrcall2 and a.earthlocation = b.earthlocation and b.terrcall1 = c.terrcall1 and b.terrcall2 = c.terrcall2 and b.terranum = c.terranum and b.terrbndcde = c.terrbndcde and b.earthlocation = c.earthlocation and b.earthcall1 = c.earthcall1 and b.interferer = c.interferer and c.etreport != 0 and c.interferer = 'E' ORDER BY etcaseno, etsubcaseno ",
                Info.GlobalSchema, ttName, ttName, ttName, ttName);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nEstsrp3.EsTsRp3(): ERROR: SQLExecDirect() failed on query:\n" + cSQL);
                Ssutil.DbGetDiagStmt(hStmt, "estsrp301: Failed to Execute:-");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            while (true)
            {
                sqlRet = ODBC.SQLFetch(hStmt);
                if (sqlRet == Constant.NOMORERECS)
                {
                    break;
                }
                else if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\nEstsrp3.EsTsRp3(): ERROR: SQLFetch() failed.");
                    Ssutil.DbGetDiagStmt(hStmt, "estsrp302: Failed Fetch:");
                    return Error.ODBC_FETCH_FAILED;
                }

                try
                {
                    Ssutil.DbStartGets();
                    Ssutil.DbGetString(hStmt, 0, "protype", out pRec.protype, EsTsSelRec.PROTYPE, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "envtype", out pRec.envtype, EsTsSelRec.ENVTYPE, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "coordist", out pRec.coordist, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "fsep", out pRec.fsep, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "analopt", out pRec.analopt, EsTsSelRec.ANALOPT, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "spherecalc", out pRec.spherecalc, EsTsSelRec.SPHERECALC, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "margin", out pRec.margin, out nullInd);

                    // site
                    Ssutil.DbGetString(hStmt, 0, "terrcall1", out pRec.terrcall1, EsTsSelRec.TERRCALL1, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "terrcall2", out pRec.terrcall2, EsTsSelRec.TERRCALL2, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "earthlocation", out pRec.earthlocation, EsTsSelRec.EARTHLOCATION, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "terrname1", out pRec.terrname1, EsTsSelRec.TERRNAME1, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "terrname2", out pRec.terrname2, EsTsSelRec.TERRNAME2, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "earthname", out pRec.earthname, EsTsSelRec.EARTHNAME, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "terroper", out pRec.terroper, EsTsSelRec.TERROPER, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "terroper2", out pRec.terroper2, EsTsSelRec.TERROPER2, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "earthoper", out pRec.earthoper, EsTsSelRec.EARTHOPER, out nullInd);
                    Ssutil.DbGetInt(hStmt, 0, "terrlatit", out pRec.terrlatit, out nullInd);
                    Ssutil.DbGetInt(hStmt, 0, "terrlongit", out pRec.terrlongit, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "terrgrnd", out pRec.terrgrnd, out nullInd);
                    Ssutil.DbGetInt(hStmt, 0, "earthlatit", out pRec.earthlatit, out nullInd);
                    Ssutil.DbGetInt(hStmt, 0, "earthlongit", out pRec.earthlongit, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "earthgrnd", out pRec.earthgrnd, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "radiozone", out pRec.radiozone, EsTsSelRec.RADIOZONE, out nullInd);
                    Ssutil.DbGetShort(hStmt, 0, "rainzone", out pRec.rainzone, out nullInd);
                    Ssutil.DbGetShort(hStmt, 0, "siteetrep", out pRec.siteetrep, out nullInd);
                    Ssutil.DbGetShort(hStmt, 0, "siteterep", out pRec.siteterep, out nullInd);
                    Ssutil.DbGetInt(hStmt, 0, "etcaseno", out pRec.etcaseno, out nullInd);
                    Ssutil.DbGetInt(hStmt, 0, "tecaseno", out pRec.tecaseno, out nullInd);
                    Ssutil.DbGetInt(hStmt, 0, "etsubcases", out pRec.etsubcases, out nullInd);
                    Ssutil.DbGetInt(hStmt, 0, "tesubcases", out pRec.tesubcases, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "intreq", out pRec.intreq, EsTsSelRec.INTREQ, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "etdist", out pRec.etdist, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "etazim", out pRec.etazim, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "teazim", out pRec.teazim, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "tudist", out pRec.tudist, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "tuazim", out pRec.tuazim, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "utazim", out pRec.utazim, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "eudist", out pRec.eudist, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "euazim", out pRec.euazim, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "ueazim", out pRec.ueazim, out nullInd);
                    Ssutil.DbGetInt(hStmt, 0, "siteproc", out pRec.siteproc, out nullInd);

                    //	Antenna
                    Ssutil.DbGetString(hStmt, 0, "interferer", out pRec.interferer, EsTsSelRec.INTERFERER, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "terrbndcde", out pRec.terrbndcde, EsTsSelRec.TERRBNDCDE, out nullInd);
                    Ssutil.DbGetShort(hStmt, 0, "terranum", out pRec.terranum, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "earthcall1", out pRec.earthcall1, EsTsSelRec.EARTHCALL1, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "earthband", out pRec.earthband, EsTsSelRec.EARTHBAND, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "terracode", out pRec.terracode, EsTsSelRec.TERRACODE, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "earthacode", out pRec.earthacode, EsTsSelRec.EARTHACODE, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "sname", out pRec.sname, EsTsSelRec.SNAME, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "satoper", out pRec.satoper, EsTsSelRec.SATOPER, out nullInd);
                    Ssutil.DbGetInt(hStmt, 0, "satlongit", out pRec.satlongit, out nullInd);
                    Ssutil.DbGetFloat(hStmt, 0, "txpre", out pRec.txpre, out nullInd);
                    Ssutil.DbGetFloat(hStmt, 0, "txtro", out pRec.txtro, out nullInd);
                    Ssutil.DbGetFloat(hStmt, 0, "rxpre", out pRec.rxpre, out nullInd);
                    Ssutil.DbGetFloat(hStmt, 0, "rxtro", out pRec.rxtro, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "sarc1", out pRec.sarc1, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "sarc2", out pRec.sarc2, out nullInd);
                    Ssutil.DbGetShort(hStmt, 0, "mode1", out pRec.mode1, out nullInd);
                    Ssutil.DbGetShort(hStmt, 0, "mode2", out pRec.mode2, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "intause", out pRec.intause, EsTsSelRec.INTAUSE, out nullInd);
                    Ssutil.DbGetShort(hStmt, 0, "anteetrep", out pRec.anteetrep, out nullInd);
                    Ssutil.DbGetShort(hStmt, 0, "anteterep", out pRec.anteterep, out nullInd);
                    Ssutil.DbGetInt(hStmt, 0, "anteproc", out pRec.anteproc, out nullInd);
                    Ssutil.DbGetInt(hStmt, 0, "etsubcaseno", out pRec.etsubcaseno, out nullInd);
                    Ssutil.DbGetInt(hStmt, 0, "tesubcaseno", out pRec.tesubcaseno, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "esazim", out pRec.esazim, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "eselev", out pRec.eselev, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "teelev", out pRec.teelev, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "etelev", out pRec.etelev, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "tuelev", out pRec.tuelev, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "utelev", out pRec.utelev, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "euelev", out pRec.euelev, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "ediscang", out pRec.ediscang, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "tdiscang", out pRec.tdiscang, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "adisc_set", out pRec.adisc_set, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "adisc_ute", out pRec.adisc_ute, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "terrht", out pRec.terrht, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "earthht", out pRec.earthht, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "tvazim", out pRec.tvazim, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "evazim", out pRec.evazim, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "tvelev", out pRec.tvelev, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "evelev", out pRec.evelev, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "tvdistes", out pRec.tvdistes, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "tvdisttu", out pRec.tvdisttu, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "evdistes", out pRec.evdistes, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "evdisttu", out pRec.evdisttu, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "angleutv", out pRec.angleutv, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "anglesev", out pRec.anglesev, out nullInd);

                    //	Channels
                    Ssutil.DbGetString(hStmt, 0, "terrchid", out pRec.terrchid, EsTsSelRec.TERRCHID, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "earthchid", out pRec.earthchid, EsTsSelRec.EARTHCHID, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "inttraftx", out pRec.inttraftx, EsTsSelRec.INTTRAFTX, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "victrafrx", out pRec.victrafrx, EsTsSelRec.VICTRAFRX, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "inteqpttx", out pRec.inteqpttx, EsTsSelRec.INTEQPTTX, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "viceqptrx", out pRec.viceqptrx, EsTsSelRec.VICEQPTRX, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "intfreqtx", out pRec.intfreqtx, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "vicfreqrx", out pRec.vicfreqrx, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "freqsep", out pRec.freqsep, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "inttxpwr", out pRec.inttxpwr, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "inttxafls", out pRec.inttxafls, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "inttxpwr2", out pRec.inttxpwr2, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "inttxafls2", out pRec.inttxafls2, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "vicpwrrx", out pRec.vicpwrrx, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "vicrxafls", out pRec.vicrxafls, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "stattx", out pRec.stattx, EsTsSelRec.STATTX, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "statrx", out pRec.statrx, EsTsSelRec.STATRX, out nullInd);
                    Ssutil.DbGetShort(hStmt, 0, "chanetrep", out pRec.chanetrep, out nullInd);
                    Ssutil.DbGetShort(hStmt, 0, "chanterep", out pRec.chanterep, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "ctxinttraftx", out pRec.ctxinttraftx, EsTsSelRec.CTXINTTRAFTX, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "ctxvictrafrx", out pRec.ctxvictrafrx, EsTsSelRec.CTXVICTRAFRX, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "ctxeqpt", out pRec.ctxeqpt, EsTsSelRec.CTXEQPT, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "calctype", out pRec.calctype, EsTsSelRec.CALCTYPE, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "earthmdsc", out pRec.earthmdsc, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "terrmdsc", out pRec.terrmdsc, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "eartheirp", out pRec.eartheirp, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "terreirp", out pRec.terreirp, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "scang", out pRec.scang, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "loss20mode1", out pRec.loss20mode1, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "calci20mode1", out pRec.calci20mode1, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "reqd20mode1", out pRec.reqd20mode1, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "marg20mode1", out pRec.marg20mode1, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "loss01mode1", out pRec.loss01mode1, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "calci01mode1", out pRec.calci01mode1, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "reqd01mode1", out pRec.reqd01mode1, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "marg01mode1", out pRec.marg01mode1, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "loss01mode2", out pRec.loss01mode2, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "calci01mode2", out pRec.calci01mode2, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "reqd01mode2", out pRec.reqd01mode2, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "marg01mode2", out pRec.marg01mode2, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "energy", out pRec.energy, out nullInd);
                    Ssutil.DbGetInt(hStmt, 0, "chanproc", out pRec.chanproc, out nullInd);
                    Ssutil.DbGetShort(hStmt, 0, "terrant", out pRec.terrant, out nullInd);

                    //	antenna again.
                    Ssutil.DbGetString(hStmt, 0, "terraxref", out pRec.terraxref, EsTsSelRec.TERRAXREF, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "terramod", out pRec.terramod, EsTsSelRec.TERRAMOD, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "terragain", out pRec.terragain, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "earthaxref", out pRec.earthaxref, EsTsSelRec.EARTHAXREF, out nullInd);
                    Ssutil.DbGetString(hStmt, 0, "earthamod", out pRec.earthamod, EsTsSelRec.EARTHAMOD, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "earthagain", out pRec.earthagain, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "terragain2", out pRec.terragain2, out nullInd);

                    Ssutil.DbGetString(hStmt, 0, "tsoffaxis", out pRec.tsoffaxis, EsTsSelRec.TSOFFAXIS, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "tstrueaz", out pRec.tstrueaz, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "tstrueel", out pRec.tstrueel, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "angleuta", out pRec.angleuta, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "angleeta", out pRec.angleeta, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "angleatv", out pRec.angleatv, out nullInd);
                    Ssutil.DbGetDouble(hStmt, 0, "adisc_atv", out pRec.adisc_atv, out nullInd);
                }
                catch (Exception e)
                {
                    Log2.e("\nEstsrp3.EsTsRp3(): ERROR: an ODBC 'Get' failed: " + e.Message);
                    Ssutil.DbGetDiagStmt(hStmt, "estsrp303: Failed retrieving " + e.Message);
                    return Error.ODBC_GET_FAILED;
                }

                ReportEtDetail(pl, ttName, pRec);
            }

            pl.Output();
            pl.IntAt(0, "Number of reported ES-TS cases: {0}", tpParm.numcases);
            pl.Output();

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            sqlRet = (SQLRETURN)Ssutil.DisConn(hConn);

            //...Log2.v("\nEstsrp3.EsTsRp3(): Exit");
            //&&Console.Error.Write("estsrp3.estsrp3(): pRec.esazim = {0}", pRec.esazim);
            return 0;
        }

        /// <summary>
        /// This method produces the ES-TS Header for the CASEDET report.  
        /// </summary>
        /// <param name="pl"> - PrintLine object used for text formatting and accumulation.</param>
        /// <param name="pRec"> - EsTsSelRec object providing source data.</param>
        private static void EsTs3Hdr(PrintLineEsTs3 pl, EsTsSelRec pRec)
        {
            string cDate;
            string cTime;
            GenUtil.UtGetDateTime(out cDate, out cTime);

            pl.LeftAt(0, "Frequency Coordination System Association");
            pl.IntAt(114, "Page:{0,3:D}", pl.PageNumber);
            pl.Output();

            pl.LeftAt(0, "MASTER DATA BASE -- ES to TS Interference Study Report - Case Detail");
            pl.LeftAt(114, "Date:{0,10}", cDate);
            pl.Output();
            pl.LeftAt(114, "Time:{0,10}", cTime);
            pl.Output();

            pl.LeftAt(69, "Environment  : ");
            pl.LeftAt(85, pRec.envtype);
            pl.LeftAt(101, "PDF File    : ");
            pl.LeftAt(115, mFile);
            pl.Output();

            return;
        }

        /// <summary>
        /// This method initializes the internal 'house-keeping' inside the class.  
        /// </summary>
        /// <param name=""></param>
        private static void EsInitCase()
        {
            mLastCase = -1;
            mLastSubCase = -1;
        }

        /// <summary>
        /// This method writes out the case detail report fragment for one set of site, 
        /// ante and channel results data.  
        /// </summary>
        /// <param name="pl"> - PrintLine object used for text formatting and accumulation.</param>
        /// <param name="ttName"> - the unique ID substring of the Te DB table names.</param>
        /// <param name="pRec"> - EsTsSelRec object providing source data.</param>
        private static void ReportEtDetail(PrintLineEsTs3 pl, string ttName, EsTsSelRec pRec)
        {
            if (!EsIsDupCase(pRec))
            {
                EsTs3CaseHdr(pl, pRec);
            }

            if (!EsIsDupSubCase(pRec))
            {
                EtSubCaseHdr(pl, pRec);
            }

            EsTs3Det(pl, pRec);

            return;
        }

        /// <summary>
        /// This method tests for a duplicate case.  
        /// </summary>
        /// <param name="pRec"> - EsTsSelRec object providing source data.</param>
        /// <returns></returns>
        private static bool EsIsDupCase(EsTsSelRec pRec)
        {
            bool Is;

            if (pRec.etcaseno == mLastCase)
            {
                Is = true;
            }
            else
            {
                Is = false;
                mLastCase = pRec.etcaseno;
                mLastSubCase = -1;   //	Reset the subcase as well.
            }

            return Is;
        }

        /// <summary>
        /// This method tests for duplicate sub-case.  
        /// </summary>
        /// <param name="pRec"> - EsTsSelRec object providing source data.</param>
        /// <returns></returns>
        private static bool EsIsDupSubCase(EsTsSelRec pRec)
        {
            bool Is;

            if (pRec.etsubcaseno == mLastSubCase)
            {
                Is = true;
            }
            else
            {
                Is = false;
                mLastSubCase = pRec.etsubcaseno;
            }

            return Is;
        }

        /// <summary>
        /// This method produces the header for each case.  
        /// </summary>
        /// <param name="pl"> - PrintLine object used for text formatting and accumulation.</param>
        /// <param name="pRec"> - EsTsSelRec object providing source data.</param>
        private static void EsTs3CaseHdr(PrintLineEsTs3 pl, EsTsSelRec pRec)
        {
            //.HEADER etcaseno 
            double maxdist = 0.0;

            maxdist = Max(Max(pRec.txtro, pRec.rxtro), Max(pRec.txpre, pRec.rxpre));

            pl.PageHeader();
            pl.IntAt(0, "Case Number : {0}", pRec.etcaseno);
            if (pRec.protype.Equals("T"))
            {
                pl.LeftAt(29, "Interferer from Environment File");
            }
            else
            {
                pl.LeftAt(29, "Interferer from Proposed File");
            }

            pl.LeftAt(69, "Analysis Type: ");
            pl.LeftAt(85, pRec.analopt);
            pl.LeftAt(101, "Coord. Dist.: ");
            pl.DoubleAt(115, "{0,8:F2}", maxdist);
            pl.LeftAt(124, "km");
            pl.Output();

            pl.DoubleAt(0, "Maximum Frequency Separation : {0,7:F2}", pRec.fsep);
            pl.LeftAt(69, "Margin       : ");
            pl.DoubleAt(85, "{0,6:F2}", pRec.margin);
            pl.Output();

            pl.LeftAt(0, "+-       Site Information");
            pl.LeftAt(100, "-+");
            pl.LeftAt(106, "Notes: Interference Case");
            pl.Output();

            pl.LeftAt(0, "CALL E  ");
            pl.LeftAt(10, "Loc.Code");
            pl.LeftAt(19, "|");
            pl.LeftAt(21, "ES Site Name E");
            pl.LeftAt(38, "Satellite Name S");
            pl.LeftAt(55, "|");
            pl.LeftAt(57, "OP E");
            pl.LeftAt(64, "OP S");
            pl.LeftAt(71, "|");
            pl.LeftAt(73, "Satellite Service");
            pl.LeftAt(91, "|");
            pl.LeftAt(93, "Radio/");
            pl.LeftAt(101, "|");
            pl.LeftAt(113, "Yes ____  No ____");
            pl.Output();

            pl.LeftAt(0, "Call T");
            pl.LeftAt(10, "Call U");
            pl.LeftAt(19, "|");
            pl.LeftAt(21, "Station  T");
            pl.LeftAt(38, "Station  U");
            pl.LeftAt(55, "|");
            pl.LeftAt(57, "OP T ");
            pl.LeftAt(64, "OP U");
            pl.LeftAt(71, "|");
            pl.LeftAt(74, "ARC 1");
            pl.LeftAt(84, "ARC 2");
            pl.LeftAt(91, "|");
            pl.LeftAt(93, "Rain Zn");
            pl.LeftAt(101, "|");
            pl.LeftAt(113, "Path Profile");
            pl.Output();

            pl.LeftAt(0, "+--------");
            pl.LeftAt(10, "---------");
            pl.LeftAt(20, "----------------");
            pl.LeftAt(37, "-----------------");
            pl.LeftAt(57, "------");
            pl.LeftAt(64, "------");
            pl.LeftAt(74, "---.---");
            pl.LeftAt(84, "--.---");
            pl.LeftAt(93, "------- +");
            pl.LeftAt(113, "Yes ____  No ____");
            pl.Output();

            pl.LeftAt(0, pRec.earthcall1);
            pl.LeftAt(10, pRec.earthlocation);
            pl.LeftAt(20, pRec.earthname);
            int nPos = 54 - pRec.sname.Trim().Length;
            pl.LeftAt(nPos, pRec.sname);
            pl.LeftAt(57, pRec.earthoper);
            pl.LeftAt(64, pRec.satoper);
            pl.DoubleAt(74, "{0,6:F2}", pRec.sarc1);
            pl.DoubleAt(84, "{0,5:F2}", pRec.sarc2);
            pl.LeftAt(94, pRec.radiozone);
            pl.IntAt(97, "{0}", (int)pRec.rainzone);
            pl.LeftAt(113, "Blockage: ____ dB");
            pl.Output();

            pl.LeftAt(0, pRec.terrcall1);
            pl.LeftAt(10, pRec.terrcall2);
            pl.LeftAt(20, pRec.terrname1.Trim());
            if (pRec.terrname1.Length + pRec.terrname2.Trim().Length > 33)
            {
                pl.LeftAt(-1, "->");
            }
            else
            {
                pl.LeftAt((int)(54 - pRec.terrname2.Length), pRec.terrname2);
            }

            pl.LeftAt(57, pRec.terroper);
            pl.LeftAt(64, pRec.terroper2);
            pl.LeftAt(74, "Longitude: ");
            pl.DoubleAt(-1, "{0,6:F2}", (double)pRec.satlongit / 360000);
            pl.LeftAt(92, "Deg. W");
            if (pRec.tsoffaxis.Equals("Y"))
            {
                pl.LeftAt(102, "Offaxis(A)");
            }
            pl.LeftAt(113, "At K=___ ");
            pl.Output();

            if (pRec.terrname1.Length + pRec.terrname2.Trim().Length > 33)
            {
                pl.LeftAt((int)(54 - pRec.terrname2.Length), pRec.terrname2);
                pl.Output();
            }
        }

        /// <summary>
        /// This method produces the text for a subcase.  
        /// </summary>
        /// <param name="pl"> - PrintLine object used for text formatting and accumulation.</param>
        /// <param name="pRec"> - EsTsSelRec object providing source data.</param>
        private static void EtSubCaseHdr(PrintLineEsTs3 pl, EsTsSelRec pRec)
        {
            //	.header etsubcaseno 
            //
            pl.Output();

            pl.LeftAt(0, "+----");
            pl.LeftAt(27, "Case Geometry");
            pl.LeftAt(60, "---*+*---");
            pl.LeftAt(79, "Geometry to Rain Volume");
            pl.LeftAt(112, "----+");
            pl.Output();

            pl.LeftAt(2, "Site");
            pl.LeftAt(11, "Dist");
            pl.LeftAt(20, "Azmth");
            pl.LeftAt(30, "Elev");
            pl.LeftAt(36, "|");
            pl.LeftAt(45, "Angle");
            pl.LeftAt(53, "Total Ant.");
            pl.LeftAt(64, "|");
            pl.LeftAt(68, "Dist to Vol:");
            pl.LeftAt(83, "on ES");
            pl.LeftAt(93, "on TU");
            pl.LeftAt(102, "Azmth");
            pl.LeftAt(113, "Elev");
            pl.Output();

            pl.LeftAt(0, "T -> E");
            pl.DoubleAt(10, "{0,6:F2}", pRec.etdist);
            pl.DoubleAt(20, "{0,6:F2}", pRec.teazim);
            pl.DoubleAt(29, "{0,6:F2}", pRec.teelev);
            pl.LeftAt(36, "|");
            pl.LeftAt(55, "Discr.");
            pl.LeftAt(64, "|");
            pl.LeftAt(68, "T -> Vol:");
            pl.DoubleAt(81, "{0,6:F2}", pRec.tvdistes);
            pl.DoubleAt(91, "{0,6:F2}", pRec.tvdisttu);
            pl.DoubleAt(101, "{0,6:F2}", pRec.tvazim);
            pl.DoubleAt(111, "{0,6:F2}", pRec.tvelev);
            pl.Output();

            pl.LeftAt(0, "T -> U");
            pl.DoubleAt(10, "{0,6:F2}", pRec.tudist);
            pl.DoubleAt(20, "{0,6:F2}", pRec.tuazim);
            pl.DoubleAt(29, "{0,6:F2}", pRec.tuelev);
            pl.LeftAt(36, "|");
            pl.LeftAt(39, "SET:");
            pl.DoubleAt(45, "{0,6:F2}", pRec.ediscang);
            pl.DoubleAt(55, "{0,6:F2}", pRec.adisc_set);
            pl.LeftAt(64, "|");
            pl.LeftAt(68, "E -> Vol:");
            pl.DoubleAt(81, "{0,6:F2}", pRec.evdistes);
            pl.DoubleAt(91, "{0,6:F2}", pRec.evdisttu);
            pl.DoubleAt(101, "{0,6:F2}", pRec.evazim);
            pl.DoubleAt(111, "{0,6:F2}", pRec.evelev);
            pl.Output();

            pl.LeftAt(0, "E -> U");
            pl.DoubleAt(10, "{0,6:F2}", pRec.eudist);
            pl.DoubleAt(20, "{0,6:F2}", pRec.euazim);
            pl.DoubleAt(29, "{0,6:F2}", pRec.euelev);
            pl.LeftAt(36, "|");
            pl.LeftAt(39, "UTE:");
            pl.DoubleAt(45, "{0,6:F2}", pRec.tdiscang);
            pl.DoubleAt(55, "{0,6:F2}", pRec.adisc_ute);
            pl.LeftAt(64, "|");
            pl.Output();

            pl.LeftAt(0, "E -> T");
            pl.DoubleAt(10, "{0,6:F2}", pRec.etdist);
            pl.DoubleAt(20, "{0,6:F2}", pRec.etazim);
            pl.DoubleAt(29, "{0,6:F2}", pRec.etelev);
            pl.LeftAt(36, "|");
            if (pRec.tsoffaxis.Equals("Y"))
            {
                pl.LeftAt(39, "ATE:");
                pl.DoubleAt(45, "{0,6:F2}", pRec.angleeta);
            }
            pl.LeftAt(64, "|");
            pl.LeftAt(70, "Angle SEV:");
            pl.DoubleAt(81, "{0,6:F2}", pRec.anglesev);
            pl.LeftAt(94, "Scang:");
            pl.DoubleAt(101, "{0,6:F2}", pRec.scang);
            pl.Output();

            pl.LeftAt(0, "E -> S");
            pl.DoubleAt(20, "{0,6:F2}", pRec.esazim);
            pl.DoubleAt(29, "{0,6:F2}", pRec.eselev);
            pl.LeftAt(36, "|");
            pl.LeftAt(64, "+");
            pl.LeftAt(70, "Angle UTV:");
            pl.DoubleAt(81, "{0,6:F2}", pRec.angleutv);
            pl.LeftAt(112, "----+");
            pl.Output();

            if (pRec.tsoffaxis.Equals("Y"))
            {
                pl.LeftAt(0, "E -> A");
                pl.DoubleAt(20, "{0,6:F2}", pRec.tstrueaz);
                pl.DoubleAt(29, "{0,6:F2}", pRec.tstrueel);
                pl.LeftAt(36, "|");
                pl.LeftAt(70, "Angle ATV:");
                pl.DoubleAt(81, "{0,6:F2}", pRec.angleatv);
                pl.LeftAt(91, "Ant. Disc:");
                pl.DoubleAt(102, "{0,6:F2}", pRec.adisc_atv);
            }
            pl.Output();

            //* Antenna Information */
            pl.LeftAt(0, "-- TX Interferer Stn.Ant.Sys.Info");
            pl.LeftAt(-1, "  ----- Station E");
            pl.LeftAt(-1, " --- ES ---");
            pl.LeftAt(63, "*+*");
            pl.LeftAt(68, "-- RX   Victim   Stn.Ant.Sys.Info");
            pl.LeftAt(-1, "  ----- Station T");
            pl.LeftAt(-1, "   --- TS  -");
            pl.Output();
            pl.Output();

            pl.LeftAt(0, "Antenna #: ");
            pl.DoubleAt(30, "Gain: {0,6:F2}", pRec.earthagain);
            pl.LeftAt(41, "dB");
            pl.LeftAt(49, "Coord Dist.");
            pl.LeftAt(64, "|");
            pl.IntAt(68, "Antenna #: {0}", pRec.terranum);
            pl.LeftAt(86, "AUse: {0}", pRec.intause);
            pl.DoubleAt(100, "Gain: {0,6:F2}", pRec.terragain);
            pl.LeftAt(111, "dB");
            if (pRec.tsoffaxis.Equals("Y"))
            {
                pl.LeftAt(118, "Adisc.:");
                pl.DoubleAt(126, "{0,6:F2}", pRec.adisc_atv);
            }
            pl.Output();

            pl.LeftAt(0, "TX.Ant.Code: {0}", pRec.earthacode);
            pl.LeftAt(30, "Model:{0}", pRec.earthamod);
            pl.FloatAt(49, "Trop: {0,6:F2} km", pRec.rxtro);
            pl.LeftAt(64, "|");
            pl.LeftAt(68, "RX.Ant.Code: {0}", pRec.terracode);
            pl.LeftAt(100, "Model:{0}", pRec.terramod);
            pl.Output();

            pl.LeftAt(0, "Xref... Ant: ");
            if (pRec.earthaxref.Length == 0)
            {
                pl.LeftAt(-1, pRec.earthacode);
            }
            else
            {
                pl.LeftAt(-1, pRec.earthaxref);
            }
            pl.DoubleAt(49, "Prec: {0,6:F2} km", pRec.rxpre);
            pl.LeftAt(64, "|");
            pl.LeftAt(68, "Xref... Ant: ");

            if (pRec.terraxref.Length == 0)
            {
                pl.LeftAt(-1, pRec.terracode);
            }
            else
            {
                pl.LeftAt(-1, pRec.terraxref);
            }
            pl.Output();

            pl.LeftAt(2, "Ant.Elev.: ");
            pl.DoubleAt(13, "{0,7:F2}", pRec.earthht);
            pl.LeftAt(21, "m (AMSL)");
            pl.LeftAt(32, "TX.Ant.Ht:");
            pl.DoubleAt(42, "{0,6:F1} m (AGL)", pRec.earthht - pRec.earthgrnd);
            pl.LeftAt(64, "|");
            pl.LeftAt(70, "Ant.Elev.: ");
            pl.DoubleAt(81, "{0,7:F2}", pRec.terrht);
            pl.LeftAt(89, "m (AMSL)");
            pl.LeftAt(100, "RX.Ant.Ht:");
            pl.DoubleAt(110, "{0,6:F1} m (AGL)", pRec.terrht - pRec.terrgrnd);
            pl.Output();

            pl.DoubleAt(2, "Stn.Elev.: {0,7:F2} m (AMSL)", pRec.earthgrnd);
            pl.LeftAt(64, "+");
            pl.DoubleAt(70, "Stn.Elev.: {0,7:F2} m (AMSL)", pRec.terrgrnd);
            pl.Output();
            pl.Output();

            pl.LeftAt(0, "-- TX Interferer Sys.Eqp.CTX.Band Info");
            pl.LeftAt(-1, " - Station E --- ES ---");
            pl.LeftAt(63, "*+*");
            pl.LeftAt(68, "-  RX Victim Sys.Eqp.CTX.Band Info");
            pl.LeftAt(-1, " ----- Station T   --- TS  -");
            pl.Output();

            pl.LeftAt(0, "Eqpt. Planned / Traffic Code: {0}", pRec.inteqpttx);
            pl.LeftAt(40, pRec.inttraftx);
            pl.LeftAt(64, "|");
            pl.LeftAt(68, "Eqpt. Planned / Traffic Code: {0}", pRec.viceqptrx);
            pl.LeftAt(108, pRec.victrafrx);
            pl.Output();

            pl.LeftAt(6, "Default   Traffic Code: ");
            pl.LeftAt(40, pRec.ctxinttraftx);
            pl.LeftAt(64, "|");
            pl.LeftAt(68, "Eqpt. Default / Traffic Code: {0}", pRec.ctxeqpt);
            pl.LeftAt(108, pRec.ctxvictrafrx);
            pl.Output();

            pl.LeftAt(19, "Freq.Bnd : {0}", pRec.earthband);
            pl.LeftAt(64, "+");
            pl.LeftAt(87, "Freq.Bnd : {0}", pRec.terrbndcde);
            pl.Output();

            pl.LeftAt(0, "-");
            pl.LeftAt(64, "-");
            pl.RightAt(pl.LastPos(), "-");
            pl.Output();

            pl.LeftAt(0, "SC");
            pl.Output();

            pl.LeftAt(0, "UA");
            pl.LeftAt(6, "TX.Freq.");
            pl.LeftAt(19, "RX.Freq.");
            pl.LeftAt(30, "Chn");
            pl.LeftAt(35, "Freq.");
            pl.LeftAt(43, "INT.");
            pl.LeftAt(50, "INT.");
            pl.LeftAt(57, "INT.");
            pl.LeftAt(64, "INT.");
            pl.LeftAt(71, "VIC.");
            pl.LeftAt(78, "VIC.");
            pl.LeftAt(84, "VIC.");
            pl.LeftAt(100, "Interference Req.: {0} (", pRec.intreq);
            if (pRec.calctype.Trim().Length < 2)
            {
                pl.LeftAt(-1, "CALC.)");
            }
            else
            {
                pl.LeftAt(-1, "TABLE)");
            }
            pl.Output();

            pl.LeftAt(0, "BS");
            pl.LeftAt(7, "(MHz)");
            pl.LeftAt(20, "(MHz)");
            pl.LeftAt(30, "Sts");
            pl.LeftAt(35, "Sep.");
            pl.LeftAt(43, "TXPwr");
            pl.LeftAt(50, "AFLS");
            pl.LeftAt(57, "Net");
            pl.LeftAt(64, "EIRP");
            pl.LeftAt(71, "Net");
            pl.LeftAt(78, "AFSL");
            pl.LeftAt(84, "RXPwr");
            pl.LeftAt(100, "Loss");
            pl.LeftAt(108, "CALC.");
            pl.LeftAt(117, "REQD.");
            //pl.LeftAt(127, "----");
            pl.Output();

            pl.LeftAt(0, " E");
            pl.LeftAt(7, "STN.E");
            pl.LeftAt(20, "STN.T");
            pl.LeftAt(30, "T.R");
            pl.LeftAt(35, "(MHz)");
            pl.LeftAt(43, "(dBw)");
            pl.LeftAt(57, "ATGn");
            pl.LeftAt(64, "(dBw)");
            pl.LeftAt(71, "AtGn");
            pl.LeftAt(84, "(dBm)");
            pl.LeftAt(100, "(dB)");
            if (pRec.calctype.Equals("I"))
            {
                pl.RightAt(110, "-I");
                pl.RightAt(119, "-I");
            }
            else if (pRec.calctype.Equals("C"))
            {
                pl.RightAt(110, "C/I");
                pl.RightAt(119, "C/I");
            }
            else
            {
                pl.RightAt(110, pRec.calctype.Trim());
                pl.RightAt(119, pRec.calctype.Trim());
            }
            pl.LeftAt(124, "Margin");
            pl.Output();

            pl.LeftAt(0, "---");
            pl.LeftAt(4, "------.---- ");
            pl.LeftAt(17, "------.---- ");
            pl.LeftAt(30, "-.-");
            pl.LeftAt(34, "----.--");
            pl.LeftAt(43, "--.--");
            pl.LeftAt(50, "--.-");
            pl.LeftAt(55, "---.--");
            pl.LeftAt(63, "---.--");
            pl.LeftAt(70, "---.--");
            pl.LeftAt(78, "--.-");
            pl.LeftAt(83, "----.--");
            pl.LeftAt(100, "---.--");
            pl.LeftAt(107, "----.--");
            pl.LeftAt(116, "----.--");
            pl.LeftAt(124, "----.--");
            pl.Output();
        }

        /// <summary>
        /// This method produces the text for the detail record.  
        /// </summary>
        /// <param name="pl"> - PrintLine object used for text formatting and accumulation.</param>
        /// <param name="pRec"> - EsTsSelRec object providing source data.</param>
        private static void EsTs3Det(PrintLineEsTs3 pl, EsTsSelRec pRec)
        {
            double intfreqtxr;
            double vicfreqrxr;
            double freqsepr;

            if (pl.LineNumber() + 4 > pl.LastLine())
            {
                pl.PageHeader();
            }
            pl.IntAt(0, "{0,3:D}", pRec.etsubcaseno);
            intfreqtxr = pRec.intfreqtx / 1000.0;
            pl.RightAt(14, GenUtil.Ccommas(intfreqtxr, 4));     // ("ZZ,ZZn.nnnn"),
            vicfreqrxr = pRec.vicfreqrx / 1000.0;
            pl.RightAt(27, GenUtil.Ccommas(vicfreqrxr, 4)); // ("ZZ,ZZn.nnnn"),
            pl.LeftAt(30, pRec.stattx);
            pl.LeftAt(32, pRec.statrx);
            freqsepr = pRec.freqsep / 1000.0;
            pl.DoubleAt(34, "{0,7:F2}", freqsepr);  // ("-ZZn.nn"),
            pl.DoubleAt(43, "{0,5:F2}", pRec.inttxpwr);
            pl.DoubleAt(50, "{0,4:F1}", pRec.inttxafls);
            pl.DoubleAt(55, "{0,6:F2}", pRec.earthmdsc);
            pl.DoubleAt(63, "{0,6:F2}", pRec.eartheirp);
            pl.DoubleAt(70, "{0,6:F2}", pRec.terrmdsc);
            pl.DoubleAt(78, "{0,4:F1}", pRec.vicrxafls);
            pl.DoubleAt(83, "{0,7:F2}", pRec.vicpwrrx);
            if (pRec.spherecalc.Equals("2"))
            {
                pl.LeftAt(93, "20% S:");
            }
            else
            {
                pl.LeftAt(93, "20% T:");
            }
            pl.DoubleAt(100, "{0,6:F2}", pRec.loss20mode1);
            pl.DoubleAt(107, "{0,7:F2}", pRec.calci20mode1);
            pl.DoubleAt(116, "{0,7:F2}", pRec.reqd20mode1);
            pl.DoubleAt(124, "{0,7:F2}", pRec.marg20mode1);
            pl.Output();

            pl.LeftAt(20, "Station U Ant.Gain:");
            pl.DoubleAt(43, "{0,4:F1}", pRec.terragain2);
            pl.LeftAt(65, "Energy Dispersal:");
            pl.DoubleAt(85, "{0,5:F2}", pRec.energy);
            pl.LeftAt(92, ".01% T:");
            pl.DoubleAt(100, "{0,6:F2}", pRec.loss01mode1);
            pl.DoubleAt(107, "{0,7:F2}", pRec.calci01mode1);
            pl.DoubleAt(116, "{0,7:F2}", pRec.reqd01mode1);
            pl.DoubleAt(124, "{0,7:F2}", pRec.marg01mode1);
            pl.Output();

            pl.LeftAt(63, "20% RAC/TAC Factor:");
            if (pRec.intreq.Equals("FCSA"))
            {
                pl.LeftAt(84, "-13.49");
            }
            else
            {
                pl.LeftAt(84, "-23.49");
            }
            pl.LeftAt(92, ".01% P:");
            pl.DoubleAt(100, "{0,6:F2}", pRec.loss01mode2);
            pl.DoubleAt(107, "{0,7:F2}", pRec.calci01mode2);
            pl.DoubleAt(116, "{0,7:F2}", pRec.reqd01mode2);
            pl.DoubleAt(124, "{0,7:F2}", pRec.marg01mode2);
            pl.Output();

            return;
        }


    }
}

```
