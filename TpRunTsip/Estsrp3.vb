Imports _Configuration
Imports _NewLib
Imports _Utillib
Imports _DataStructures
Imports System
Imports System.IO
Imports System.Linq
Imports System.Math
Imports SQLCHARPTR = System.String            'Invented to mimic (char *) for [In]  only.
Imports SQLHANDLE = System.IntPtr
Imports SQLHDBC = System.IntPtr
Imports SQLLEN = System.Int64
Imports SQLRETURN = System.Int16

Namespace TpRunTsip


    ''' <summary>
    ''' Provides methods used to generate the Case Detail (.CASEDET) for es-ts.
    ''' </summary>
    Public Class Estsrp3
        Private Shared mFile As String
        Private Shared tRec As EsTsSelRec
        Private Shared mLastCase As Integer
        Private Shared mLastSubCase As Integer
        Private Class PrintLineEsTs3
            Inherits TpRunTsip.PrintLine
            ''' <summary>
            ''' This method overrides the base class's PageHeader method
            ''' and writes a page header appropriate for a CASEDET report.
            ''' </summary>
            ''' <paramname=""></param>
            Public Overrides Sub PageHeader()
                MyBase.PageBefore()

                TpRunTsip.Estsrp3.EsTs3Hdr(Me, TpRunTsip.Estsrp3.tRec)
            End Sub
        End Class

        ''' <summary>
        ''' This method manages the production of the Case Detail (.CASEDET) for es-ts.
        ''' </summary>
        ''' <paramname="tw"> - the TextWriter object assigned to the .CASEDET report.</param>
        ''' <paramname="ttName"> - the unique ID substring of the Te DB table names.</param>
        ''' <paramname="tpParm"> - the TpParm object providing source data.</param>
        ''' <returns></returns>
        Public Shared Function EsTsRp3(tw As TextWriter, ttName As String, tpParm As TpParm) As Integer
            '...Log2.v("\nEstsrp3.EsTsRp3(): Entry");

            Dim hConn As SQLHDBC = Ssutil.NewConn()
            Dim hStmt As SQLHANDLE
            Dim sqlRet As SQLRETURN
            Dim nullInd As SQLLEN

            Dim cSQL As String

            Dim pRec As EsTsSelRec = New EsTsSelRec()

            'tRec is 'global' and so can be accessed by the PageHeader override.
            TpRunTsip.Estsrp3.tRec = pRec

            Dim pl As TpRunTsip.Estsrp3.PrintLineEsTs3 = New TpRunTsip.Estsrp3.PrintLineEsTs3()
            pl.OutFile(tw)
            pl.FormFeed() ' Just to mimic the behaviour of the MICS legacy C++ code.

            Call TpRunTsip.Estsrp3.EsInitCase()

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)

            'mFile is 'global' and so can be accessed by the PageHeader override.
            TpRunTsip.Estsrp3.mFile = ttName

            cSQL = SQLCHARPTR.Format("select protype,envtype,coordist,fsep,analopt,spherecalc,margin,a.terrcall1,a.terrcall2,a.earthlocation,a.terrname1,a.terrname2,a.earthname,terroper,terroper2,earthoper,terrlatit,terrlongit,terrgrnd,earthlatit,earthlongit,earthgrnd,radiozone,rainzone,a.etreport,a.tereport ,etcaseno,tecaseno,etsubcases,tesubcases,intreq,etdist,etazim,teazim,tudist,tuazim,utazim,eudist,euazim,ueazim,a.processed,b.interferer,b.terrbndcde,b.terranum,b.earthcall1,b.earthband,terracode,earthacode,satname,satoper,satlongit,txpre,txtro,rxpre,rxtro,sarc1,sarc2,mode1,mode2,intause,b.etreport,b.tereport,b.processed,etsubcaseno,tesubcaseno,esazim,eselev,teelev,etelev,tuelev,utelev,euelev,ediscang,tdiscang,adisc_set,adisc_ute,terrht,earthht,tvazim,evazim,tvelev,evelev,tvdistes,tvdisttu,evdistes,evdisttu,angleutv,anglesev,terrchid,earthchid,inttraftx,victrafrx,inteqpttx,viceqptrx,intfreqtx,vicfreqrx,freqsep,inttxpwr,inttxafls,inttxpwr2,inttxafls2,vicpwrrx,vicrxafls,stattx,statrx,c.etreport,c.tereport,ctxinttraftx,ctxvictrafrx,ctxeqpt,calctype,earthmdsc,terrmdsc,eartheirp,terreirp,scang,loss20mode1,calci20mode1,reqd20mode1,marg20mode1,loss01mode1,calci01mode1,reqd01mode1,marg01mode1,loss01mode2,calci01mode2,reqd01mode2,marg01mode2,energy,c.processed,terrant,terraxref,terramodel,terragain,earthaxref,earthamodel,earthagain,remterragain,tsoffaxis,tstrueaz,tstrueel,angleuta,angleeta,angleatv,adisc_atv FROM " & Microsoft.VisualBasic.Constants.vbTab & "{0}.te_{1}_site a, {0}.te_{2}_ante b, {0}.te_{3}_chan c, {0}.te_{4}_parm WHERE a.terrcall1 = b.terrcall1 and a.terrcall2 = b.terrcall2 and a.earthlocation = b.earthlocation and b.terrcall1 = c.terrcall1 and b.terrcall2 = c.terrcall2 and b.terranum = c.terranum and b.terrbndcde = c.terrbndcde and b.earthlocation = c.earthlocation and b.earthcall1 = c.earthcall1 and b.interferer = c.interferer and c.etreport != 0 and c.interferer = 'E' ORDER BY etcaseno, etsubcaseno ", Info.GlobalSchema, ttName, ttName, ttName, ttName)

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length)
            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "Estsrp3.EsTsRp3(): ERROR: SQLExecDirect() failed on query:" & Microsoft.VisualBasic.Constants.vbLf & cSQL)
                Ssutil.DbGetDiagStmt(hStmt, "estsrp301: Failed to Execute:-")
                Return [Error].ODBC_EXECDIRECT_FAILED
            End If

            While True
                sqlRet = ODBC.SQLFetch(hStmt)
                If sqlRet = Constant.NOMORERECS Then
                    Exit While
                ElseIf Not ODBC.IsOK(sqlRet) Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "Estsrp3.EsTsRp3(): ERROR: SQLFetch() failed.")
                    Ssutil.DbGetDiagStmt(hStmt, "estsrp302: Failed Fetch:")
                    Return [Error].ODBC_FETCH_FAILED
                End If

                Try
                    Ssutil.DbStartGets()
                    Ssutil.DbGetString(hStmt, 0, "protype", pRec.protype, EsTsSelRec.PROTYPE, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "envtype", pRec.envtype, EsTsSelRec.ENVTYPE, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "coordist", pRec.coordist, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "fsep", pRec.fsep, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "analopt", pRec.analopt, EsTsSelRec.ANALOPT, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "spherecalc", pRec.spherecalc, EsTsSelRec.SPHERECALC, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "margin", pRec.margin, nullInd)

                    ' site
                    Ssutil.DbGetString(hStmt, 0, "terrcall1", pRec.terrcall1, EsTsSelRec.TERRCALL1, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terrcall2", pRec.terrcall2, EsTsSelRec.TERRCALL2, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthlocation", pRec.earthlocation, EsTsSelRec.EARTHLOCATION, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terrname1", pRec.terrname1, EsTsSelRec.TERRNAME1, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terrname2", pRec.terrname2, EsTsSelRec.TERRNAME2, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthname", pRec.earthname, EsTsSelRec.EARTHNAME, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terroper", pRec.terroper, EsTsSelRec.TERROPER, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terroper2", pRec.terroper2, EsTsSelRec.TERROPER2, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthoper", pRec.earthoper, EsTsSelRec.EARTHOPER, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "terrlatit", pRec.terrlatit, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "terrlongit", pRec.terrlongit, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "terrgrnd", pRec.terrgrnd, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "earthlatit", pRec.earthlatit, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "earthlongit", pRec.earthlongit, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "earthgrnd", pRec.earthgrnd, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "radiozone", pRec.radiozone, EsTsSelRec.RADIOZONE, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "rainzone", pRec.rainzone, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "siteetrep", pRec.siteetrep, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "siteterep", pRec.siteterep, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "etcaseno", pRec.etcaseno, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "tecaseno", pRec.tecaseno, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "etsubcases", pRec.etsubcases, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "tesubcases", pRec.tesubcases, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "intreq", pRec.intreq, EsTsSelRec.INTREQ, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "etdist", pRec.etdist, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "etazim", pRec.etazim, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "teazim", pRec.teazim, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "tudist", pRec.tudist, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "tuazim", pRec.tuazim, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "utazim", pRec.utazim, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "eudist", pRec.eudist, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "euazim", pRec.euazim, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "ueazim", pRec.ueazim, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "siteproc", pRec.siteproc, nullInd)

                    '	Antenna
                    Ssutil.DbGetString(hStmt, 0, "interferer", pRec.interferer, EsTsSelRec.INTERFERER, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terrbndcde", pRec.terrbndcde, EsTsSelRec.TERRBNDCDE, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "terranum", pRec.terranum, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthcall1", pRec.earthcall1, EsTsSelRec.EARTHCALL1, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthband", pRec.earthband, EsTsSelRec.EARTHBAND, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terracode", pRec.terracode, EsTsSelRec.TERRACODE, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthacode", pRec.earthacode, EsTsSelRec.EARTHACODE, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "sname", pRec.sname, EsTsSelRec.SNAME, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "satoper", pRec.satoper, EsTsSelRec.SATOPER, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "satlongit", pRec.satlongit, nullInd)
                    Ssutil.DbGetFloat(hStmt, 0, "txpre", pRec.txpre, nullInd)
                    Ssutil.DbGetFloat(hStmt, 0, "txtro", pRec.txtro, nullInd)
                    Ssutil.DbGetFloat(hStmt, 0, "rxpre", pRec.rxpre, nullInd)
                    Ssutil.DbGetFloat(hStmt, 0, "rxtro", pRec.rxtro, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "sarc1", pRec.sarc1, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "sarc2", pRec.sarc2, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "mode1", pRec.mode1, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "mode2", pRec.mode2, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "intause", pRec.intause, EsTsSelRec.INTAUSE, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "anteetrep", pRec.anteetrep, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "anteterep", pRec.anteterep, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "anteproc", pRec.anteproc, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "etsubcaseno", pRec.etsubcaseno, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "tesubcaseno", pRec.tesubcaseno, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "esazim", pRec.esazim, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "eselev", pRec.eselev, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "teelev", pRec.teelev, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "etelev", pRec.etelev, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "tuelev", pRec.tuelev, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "utelev", pRec.utelev, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "euelev", pRec.euelev, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "ediscang", pRec.ediscang, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "tdiscang", pRec.tdiscang, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "adisc_set", pRec.adisc_set, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "adisc_ute", pRec.adisc_ute, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "terrht", pRec.terrht, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "earthht", pRec.earthht, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "tvazim", pRec.tvazim, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "evazim", pRec.evazim, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "tvelev", pRec.tvelev, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "evelev", pRec.evelev, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "tvdistes", pRec.tvdistes, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "tvdisttu", pRec.tvdisttu, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "evdistes", pRec.evdistes, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "evdisttu", pRec.evdisttu, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "angleutv", pRec.angleutv, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "anglesev", pRec.anglesev, nullInd)

                    '	Channels
                    Ssutil.DbGetString(hStmt, 0, "terrchid", pRec.terrchid, EsTsSelRec.TERRCHID, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthchid", pRec.earthchid, EsTsSelRec.EARTHCHID, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "inttraftx", pRec.inttraftx, EsTsSelRec.INTTRAFTX, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "victrafrx", pRec.victrafrx, EsTsSelRec.VICTRAFRX, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "inteqpttx", pRec.inteqpttx, EsTsSelRec.INTEQPTTX, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "viceqptrx", pRec.viceqptrx, EsTsSelRec.VICEQPTRX, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "intfreqtx", pRec.intfreqtx, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "vicfreqrx", pRec.vicfreqrx, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "freqsep", pRec.freqsep, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "inttxpwr", pRec.inttxpwr, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "inttxafls", pRec.inttxafls, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "inttxpwr2", pRec.inttxpwr2, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "inttxafls2", pRec.inttxafls2, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "vicpwrrx", pRec.vicpwrrx, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "vicrxafls", pRec.vicrxafls, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "stattx", pRec.stattx, EsTsSelRec.STATTX, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "statrx", pRec.statrx, EsTsSelRec.STATRX, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "chanetrep", pRec.chanetrep, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "chanterep", pRec.chanterep, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "ctxinttraftx", pRec.ctxinttraftx, EsTsSelRec.CTXINTTRAFTX, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "ctxvictrafrx", pRec.ctxvictrafrx, EsTsSelRec.CTXVICTRAFRX, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "ctxeqpt", pRec.ctxeqpt, EsTsSelRec.CTXEQPT, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "calctype", pRec.calctype, EsTsSelRec.CALCTYPE, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "earthmdsc", pRec.earthmdsc, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "terrmdsc", pRec.terrmdsc, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "eartheirp", pRec.eartheirp, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "terreirp", pRec.terreirp, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "scang", pRec.scang, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "loss20mode1", pRec.loss20mode1, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "calci20mode1", pRec.calci20mode1, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "reqd20mode1", pRec.reqd20mode1, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "marg20mode1", pRec.marg20mode1, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "loss01mode1", pRec.loss01mode1, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "calci01mode1", pRec.calci01mode1, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "reqd01mode1", pRec.reqd01mode1, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "marg01mode1", pRec.marg01mode1, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "loss01mode2", pRec.loss01mode2, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "calci01mode2", pRec.calci01mode2, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "reqd01mode2", pRec.reqd01mode2, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "marg01mode2", pRec.marg01mode2, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "energy", pRec.energy, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "chanproc", pRec.chanproc, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "terrant", pRec.terrant, nullInd)

                    '	antenna again.
                    Ssutil.DbGetString(hStmt, 0, "terraxref", pRec.terraxref, EsTsSelRec.TERRAXREF, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terramod", pRec.terramod, EsTsSelRec.TERRAMOD, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "terragain", pRec.terragain, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthaxref", pRec.earthaxref, EsTsSelRec.EARTHAXREF, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthamod", pRec.earthamod, EsTsSelRec.EARTHAMOD, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "earthagain", pRec.earthagain, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "terragain2", pRec.terragain2, nullInd)

                    Ssutil.DbGetString(hStmt, 0, "tsoffaxis", pRec.tsoffaxis, EsTsSelRec.TSOFFAXIS, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "tstrueaz", pRec.tstrueaz, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "tstrueel", pRec.tstrueel, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "angleuta", pRec.angleuta, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "angleeta", pRec.angleeta, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "angleatv", pRec.angleatv, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "adisc_atv", pRec.adisc_atv, nullInd)
                Catch e As Exception
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "Estsrp3.EsTsRp3(): ERROR: an ODBC 'Get' failed: " & e.Message)
                    Ssutil.DbGetDiagStmt(hStmt, "estsrp303: Failed retrieving " & e.Message)
                    Return [Error].ODBC_GET_FAILED
                End Try

                TpRunTsip.Estsrp3.ReportEtDetail(pl, ttName, pRec)
            End While

            pl.Output()
            pl.IntAt(0, "Number of reported ES-TS cases: {0}", tpParm.numcases)
            pl.Output()

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
            sqlRet = CShort(Ssutil.DisConn(hConn))

            '...Log2.v("\nEstsrp3.EsTsRp3(): Exit");
            '&&Console.Error.Write("estsrp3.estsrp3(): pRec.esazim = {0}", pRec.esazim);
            Return 0
        End Function

        ''' <summary>
        ''' This method produces the ES-TS Header for the CASEDET report.  
        ''' </summary>
        ''' <paramname="pl"> - PrintLine object used for text formatting and accumulation.</param>
        ''' <paramname="pRec"> - EsTsSelRec object providing source data.</param>
        Private Shared Sub EsTs3Hdr(pl As TpRunTsip.Estsrp3.PrintLineEsTs3, pRec As EsTsSelRec)
            Dim [cDate] As String
            Dim cTime As String
            GenUtil.UtGetDateTime([cDate], cTime)

            pl.LeftAt(0, "Frequency Coordination System Association")
            pl.IntAt(114, "Page:{0,3:D}", pl.PageNumber)
            pl.Output()

            pl.LeftAt(0, "MASTER DATA BASE -- ES to TS Interference Study Report - Case Detail")
            pl.LeftAt(114, "Date:{0,10}", [cDate])
            pl.Output()
            pl.LeftAt(114, "Time:{0,10}", cTime)
            pl.Output()

            pl.LeftAt(69, "Environment  : ")
            pl.LeftAt(85, pRec.envtype)
            pl.LeftAt(101, "PDF File    : ")
            pl.LeftAt(115, TpRunTsip.Estsrp3.mFile)
            pl.Output()

            Return
        End Sub

        ''' <summary>
        ''' This method initializes the internal 'house-keeping' inside the class.  
        ''' </summary>
        ''' <paramname=""></param>
        Private Shared Sub EsInitCase()
            TpRunTsip.Estsrp3.mLastCase = -1
            TpRunTsip.Estsrp3.mLastSubCase = -1
        End Sub

        ''' <summary>
        ''' This method writes out the case detail report fragment for one set of site, 
        ''' ante and channel results data.  
        ''' </summary>
        ''' <paramname="pl"> - PrintLine object used for text formatting and accumulation.</param>
        ''' <paramname="ttName"> - the unique ID substring of the Te DB table names.</param>
        ''' <paramname="pRec"> - EsTsSelRec object providing source data.</param>
        Private Shared Sub ReportEtDetail(pl As TpRunTsip.Estsrp3.PrintLineEsTs3, ttName As String, pRec As EsTsSelRec)
            If Not TpRunTsip.Estsrp3.EsIsDupCase(pRec) Then
                TpRunTsip.Estsrp3.EsTs3CaseHdr(pl, pRec)
            End If

            If Not TpRunTsip.Estsrp3.EsIsDupSubCase(pRec) Then
                TpRunTsip.Estsrp3.EtSubCaseHdr(pl, pRec)
            End If

            TpRunTsip.Estsrp3.EsTs3Det(pl, pRec)

            Return
        End Sub

        ''' <summary>
        ''' This method tests for a duplicate case.  
        ''' </summary>
        ''' <paramname="pRec"> - EsTsSelRec object providing source data.</param>
        ''' <returns></returns>
        Private Shared Function EsIsDupCase(pRec As EsTsSelRec) As Boolean
            Dim [Is] As Boolean

            If pRec.etcaseno = TpRunTsip.Estsrp3.mLastCase Then
                [Is] = True
            Else
                [Is] = False
                TpRunTsip.Estsrp3.mLastCase = pRec.etcaseno
                TpRunTsip.Estsrp3.mLastSubCase = -1   '	Reset the subcase as well.
            End If

            Return [Is]
        End Function

        ''' <summary>
        ''' This method tests for duplicate sub-case.  
        ''' </summary>
        ''' <paramname="pRec"> - EsTsSelRec object providing source data.</param>
        ''' <returns></returns>
        Private Shared Function EsIsDupSubCase(pRec As EsTsSelRec) As Boolean
            Dim [Is] As Boolean

            If pRec.etsubcaseno = TpRunTsip.Estsrp3.mLastSubCase Then
                [Is] = True
            Else
                [Is] = False
                TpRunTsip.Estsrp3.mLastSubCase = pRec.etsubcaseno
            End If

            Return [Is]
        End Function

        ''' <summary>
        ''' This method produces the header for each case.  
        ''' </summary>
        ''' <paramname="pl"> - PrintLine object used for text formatting and accumulation.</param>
        ''' <paramname="pRec"> - EsTsSelRec object providing source data.</param>
        Private Shared Sub EsTs3CaseHdr(pl As TpRunTsip.Estsrp3.PrintLineEsTs3, pRec As EsTsSelRec)
            '.HEADER etcaseno 
            Dim maxdist = 0.0

            maxdist = Max(Max(pRec.txtro, pRec.rxtro), Max(pRec.txpre, pRec.rxpre))

            pl.PageHeader()
            pl.IntAt(0, "Case Number : {0}", pRec.etcaseno)
            If pRec.protype.Equals("T") Then
                pl.LeftAt(29, "Interferer from Environment File")
            Else
                pl.LeftAt(29, "Interferer from Proposed File")
            End If

            pl.LeftAt(69, "Analysis Type: ")
            pl.LeftAt(85, pRec.analopt)
            pl.LeftAt(101, "Coord. Dist.: ")
            pl.DoubleAt(115, "{0,8:F2}", maxdist)
            pl.LeftAt(124, "km")
            pl.Output()

            pl.DoubleAt(0, "Maximum Frequency Separation : {0,7:F2}", pRec.fsep)
            pl.LeftAt(69, "Margin       : ")
            pl.DoubleAt(85, "{0,6:F2}", pRec.margin)
            pl.Output()

            pl.LeftAt(0, "+-       Site Information")
            pl.LeftAt(100, "-+")
            pl.LeftAt(106, "Notes: Interference Case")
            pl.Output()

            pl.LeftAt(0, "CALL E  ")
            pl.LeftAt(10, "Loc.Code")
            pl.LeftAt(19, "|")
            pl.LeftAt(21, "ES Site Name E")
            pl.LeftAt(38, "Satellite Name S")
            pl.LeftAt(55, "|")
            pl.LeftAt(57, "OP E")
            pl.LeftAt(64, "OP S")
            pl.LeftAt(71, "|")
            pl.LeftAt(73, "Satellite Service")
            pl.LeftAt(91, "|")
            pl.LeftAt(93, "Radio/")
            pl.LeftAt(101, "|")
            pl.LeftAt(113, "Yes ____  No ____")
            pl.Output()

            pl.LeftAt(0, "Call T")
            pl.LeftAt(10, "Call U")
            pl.LeftAt(19, "|")
            pl.LeftAt(21, "Station  T")
            pl.LeftAt(38, "Station  U")
            pl.LeftAt(55, "|")
            pl.LeftAt(57, "OP T ")
            pl.LeftAt(64, "OP U")
            pl.LeftAt(71, "|")
            pl.LeftAt(74, "ARC 1")
            pl.LeftAt(84, "ARC 2")
            pl.LeftAt(91, "|")
            pl.LeftAt(93, "Rain Zn")
            pl.LeftAt(101, "|")
            pl.LeftAt(113, "Path Profile")
            pl.Output()

            pl.LeftAt(0, "+--------")
            pl.LeftAt(10, "---------")
            pl.LeftAt(20, "----------------")
            pl.LeftAt(37, "-----------------")
            pl.LeftAt(57, "------")
            pl.LeftAt(64, "------")
            pl.LeftAt(74, "---.---")
            pl.LeftAt(84, "--.---")
            pl.LeftAt(93, "------- +")
            pl.LeftAt(113, "Yes ____  No ____")
            pl.Output()

            pl.LeftAt(0, pRec.earthcall1)
            pl.LeftAt(10, pRec.earthlocation)
            pl.LeftAt(20, pRec.earthname)
            Dim nPos As Integer = 54 - pRec.sname.Trim().Length
            pl.LeftAt(nPos, pRec.sname)
            pl.LeftAt(57, pRec.earthoper)
            pl.LeftAt(64, pRec.satoper)
            pl.DoubleAt(74, "{0,6:F2}", pRec.sarc1)
            pl.DoubleAt(84, "{0,5:F2}", pRec.sarc2)
            pl.LeftAt(94, pRec.radiozone)
            pl.IntAt(97, "{0}", pRec.rainzone)
            pl.LeftAt(113, "Blockage: ____ dB")
            pl.Output()

            pl.LeftAt(0, pRec.terrcall1)
            pl.LeftAt(10, pRec.terrcall2)
            pl.LeftAt(20, pRec.terrname1.Trim())
            If pRec.terrname1.Length + pRec.terrname2.Trim().Length > 33 Then
                pl.LeftAt(-1, "->")
            Else
                pl.LeftAt(CInt(54 - pRec.terrname2.Length), pRec.terrname2)
            End If

            pl.LeftAt(57, pRec.terroper)
            pl.LeftAt(64, pRec.terroper2)
            pl.LeftAt(74, "Longitude: ")
            pl.DoubleAt(-1, "{0,6:F2}", pRec.satlongit / 360000)
            pl.LeftAt(92, "Deg. W")
            If pRec.tsoffaxis.Equals("Y") Then
                pl.LeftAt(102, "Offaxis(A)")
            End If
            pl.LeftAt(113, "At K=___ ")
            pl.Output()

            If pRec.terrname1.Length + pRec.terrname2.Trim().Length > 33 Then
                pl.LeftAt(CInt(54 - pRec.terrname2.Length), pRec.terrname2)
                pl.Output()
            End If
        End Sub

        ''' <summary>
        ''' This method produces the text for a subcase.  
        ''' </summary>
        ''' <paramname="pl"> - PrintLine object used for text formatting and accumulation.</param>
        ''' <paramname="pRec"> - EsTsSelRec object providing source data.</param>
        Private Shared Sub EtSubCaseHdr(pl As TpRunTsip.Estsrp3.PrintLineEsTs3, pRec As EsTsSelRec)
            '	.header etsubcaseno 
            '
            pl.Output()

            pl.LeftAt(0, "+----")
            pl.LeftAt(27, "Case Geometry")
            pl.LeftAt(60, "---*+*---")
            pl.LeftAt(79, "Geometry to Rain Volume")
            pl.LeftAt(112, "----+")
            pl.Output()

            pl.LeftAt(2, "Site")
            pl.LeftAt(11, "Dist")
            pl.LeftAt(20, "Azmth")
            pl.LeftAt(30, "Elev")
            pl.LeftAt(36, "|")
            pl.LeftAt(45, "Angle")
            pl.LeftAt(53, "Total Ant.")
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "Dist to Vol:")
            pl.LeftAt(83, "on ES")
            pl.LeftAt(93, "on TU")
            pl.LeftAt(102, "Azmth")
            pl.LeftAt(113, "Elev")
            pl.Output()

            pl.LeftAt(0, "T -> E")
            pl.DoubleAt(10, "{0,6:F2}", pRec.etdist)
            pl.DoubleAt(20, "{0,6:F2}", pRec.teazim)
            pl.DoubleAt(29, "{0,6:F2}", pRec.teelev)
            pl.LeftAt(36, "|")
            pl.LeftAt(55, "Discr.")
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "T -> Vol:")
            pl.DoubleAt(81, "{0,6:F2}", pRec.tvdistes)
            pl.DoubleAt(91, "{0,6:F2}", pRec.tvdisttu)
            pl.DoubleAt(101, "{0,6:F2}", pRec.tvazim)
            pl.DoubleAt(111, "{0,6:F2}", pRec.tvelev)
            pl.Output()

            pl.LeftAt(0, "T -> U")
            pl.DoubleAt(10, "{0,6:F2}", pRec.tudist)
            pl.DoubleAt(20, "{0,6:F2}", pRec.tuazim)
            pl.DoubleAt(29, "{0,6:F2}", pRec.tuelev)
            pl.LeftAt(36, "|")
            pl.LeftAt(39, "SET:")
            pl.DoubleAt(45, "{0,6:F2}", pRec.ediscang)
            pl.DoubleAt(55, "{0,6:F2}", pRec.adisc_set)
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "E -> Vol:")
            pl.DoubleAt(81, "{0,6:F2}", pRec.evdistes)
            pl.DoubleAt(91, "{0,6:F2}", pRec.evdisttu)
            pl.DoubleAt(101, "{0,6:F2}", pRec.evazim)
            pl.DoubleAt(111, "{0,6:F2}", pRec.evelev)
            pl.Output()

            pl.LeftAt(0, "E -> U")
            pl.DoubleAt(10, "{0,6:F2}", pRec.eudist)
            pl.DoubleAt(20, "{0,6:F2}", pRec.euazim)
            pl.DoubleAt(29, "{0,6:F2}", pRec.euelev)
            pl.LeftAt(36, "|")
            pl.LeftAt(39, "UTE:")
            pl.DoubleAt(45, "{0,6:F2}", pRec.tdiscang)
            pl.DoubleAt(55, "{0,6:F2}", pRec.adisc_ute)
            pl.LeftAt(64, "|")
            pl.Output()

            pl.LeftAt(0, "E -> T")
            pl.DoubleAt(10, "{0,6:F2}", pRec.etdist)
            pl.DoubleAt(20, "{0,6:F2}", pRec.etazim)
            pl.DoubleAt(29, "{0,6:F2}", pRec.etelev)
            pl.LeftAt(36, "|")
            If pRec.tsoffaxis.Equals("Y") Then
                pl.LeftAt(39, "ATE:")
                pl.DoubleAt(45, "{0,6:F2}", pRec.angleeta)
            End If
            pl.LeftAt(64, "|")
            pl.LeftAt(70, "Angle SEV:")
            pl.DoubleAt(81, "{0,6:F2}", pRec.anglesev)
            pl.LeftAt(94, "Scang:")
            pl.DoubleAt(101, "{0,6:F2}", pRec.scang)
            pl.Output()

            pl.LeftAt(0, "E -> S")
            pl.DoubleAt(20, "{0,6:F2}", pRec.esazim)
            pl.DoubleAt(29, "{0,6:F2}", pRec.eselev)
            pl.LeftAt(36, "|")
            pl.LeftAt(64, "+")
            pl.LeftAt(70, "Angle UTV:")
            pl.DoubleAt(81, "{0,6:F2}", pRec.angleutv)
            pl.LeftAt(112, "----+")
            pl.Output()

            If pRec.tsoffaxis.Equals("Y") Then
                pl.LeftAt(0, "E -> A")
                pl.DoubleAt(20, "{0,6:F2}", pRec.tstrueaz)
                pl.DoubleAt(29, "{0,6:F2}", pRec.tstrueel)
                pl.LeftAt(36, "|")
                pl.LeftAt(70, "Angle ATV:")
                pl.DoubleAt(81, "{0,6:F2}", pRec.angleatv)
                pl.LeftAt(91, "Ant. Disc:")
                pl.DoubleAt(102, "{0,6:F2}", pRec.adisc_atv)
            End If
            pl.Output()

            '* Antenna Information 
            pl.LeftAt(0, "-- TX Interferer Stn.Ant.Sys.Info")
            pl.LeftAt(-1, "  ----- Station E")
            pl.LeftAt(-1, " --- ES ---")
            pl.LeftAt(63, "*+*")
            pl.LeftAt(68, "-- RX   Victim   Stn.Ant.Sys.Info")
            pl.LeftAt(-1, "  ----- Station T")
            pl.LeftAt(-1, "   --- TS  -")
            pl.Output()
            pl.Output()

            pl.LeftAt(0, "Antenna #: ")
            pl.DoubleAt(30, "Gain: {0,6:F2}", pRec.earthagain)
            pl.LeftAt(41, "dB")
            pl.LeftAt(49, "Coord Dist.")
            pl.LeftAt(64, "|")
            pl.IntAt(68, "Antenna #: {0}", pRec.terranum)
            pl.LeftAt(86, "AUse: {0}", pRec.intause)
            pl.DoubleAt(100, "Gain: {0,6:F2}", pRec.terragain)
            pl.LeftAt(111, "dB")
            If pRec.tsoffaxis.Equals("Y") Then
                pl.LeftAt(118, "Adisc.:")
                pl.DoubleAt(126, "{0,6:F2}", pRec.adisc_atv)
            End If
            pl.Output()

            pl.LeftAt(0, "TX.Ant.Code: {0}", pRec.earthacode)
            pl.LeftAt(30, "Model:{0}", pRec.earthamod)
            pl.FloatAt(49, "Trop: {0,6:F2} km", pRec.rxtro)
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "RX.Ant.Code: {0}", pRec.terracode)
            pl.LeftAt(100, "Model:{0}", pRec.terramod)
            pl.Output()

            pl.LeftAt(0, "Xref... Ant: ")
            If pRec.earthaxref.Length = 0 Then
                pl.LeftAt(-1, pRec.earthacode)
            Else
                pl.LeftAt(-1, pRec.earthaxref)
            End If
            pl.DoubleAt(49, "Prec: {0,6:F2} km", pRec.rxpre)
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "Xref... Ant: ")

            If pRec.terraxref.Length = 0 Then
                pl.LeftAt(-1, pRec.terracode)
            Else
                pl.LeftAt(-1, pRec.terraxref)
            End If
            pl.Output()

            pl.LeftAt(2, "Ant.Elev.: ")
            pl.DoubleAt(13, "{0,7:F2}", pRec.earthht)
            pl.LeftAt(21, "m (AMSL)")
            pl.LeftAt(32, "TX.Ant.Ht:")
            pl.DoubleAt(42, "{0,6:F1} m (AGL)", pRec.earthht - pRec.earthgrnd)
            pl.LeftAt(64, "|")
            pl.LeftAt(70, "Ant.Elev.: ")
            pl.DoubleAt(81, "{0,7:F2}", pRec.terrht)
            pl.LeftAt(89, "m (AMSL)")
            pl.LeftAt(100, "RX.Ant.Ht:")
            pl.DoubleAt(110, "{0,6:F1} m (AGL)", pRec.terrht - pRec.terrgrnd)
            pl.Output()

            pl.DoubleAt(2, "Stn.Elev.: {0,7:F2} m (AMSL)", pRec.earthgrnd)
            pl.LeftAt(64, "+")
            pl.DoubleAt(70, "Stn.Elev.: {0,7:F2} m (AMSL)", pRec.terrgrnd)
            pl.Output()
            pl.Output()

            pl.LeftAt(0, "-- TX Interferer Sys.Eqp.CTX.Band Info")
            pl.LeftAt(-1, " - Station E --- ES ---")
            pl.LeftAt(63, "*+*")
            pl.LeftAt(68, "-  RX Victim Sys.Eqp.CTX.Band Info")
            pl.LeftAt(-1, " ----- Station T   --- TS  -")
            pl.Output()

            pl.LeftAt(0, "Eqpt. Planned / Traffic Code: {0}", pRec.inteqpttx)
            pl.LeftAt(40, pRec.inttraftx)
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "Eqpt. Planned / Traffic Code: {0}", pRec.viceqptrx)
            pl.LeftAt(108, pRec.victrafrx)
            pl.Output()

            pl.LeftAt(6, "Default   Traffic Code: ")
            pl.LeftAt(40, pRec.ctxinttraftx)
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "Eqpt. Default / Traffic Code: {0}", pRec.ctxeqpt)
            pl.LeftAt(108, pRec.ctxvictrafrx)
            pl.Output()

            pl.LeftAt(19, "Freq.Bnd : {0}", pRec.earthband)
            pl.LeftAt(64, "+")
            pl.LeftAt(87, "Freq.Bnd : {0}", pRec.terrbndcde)
            pl.Output()

            pl.LeftAt(0, "-")
            pl.LeftAt(64, "-")
            pl.RightAt(pl.LastPos(), "-")
            pl.Output()

            pl.LeftAt(0, "SC")
            pl.Output()

            pl.LeftAt(0, "UA")
            pl.LeftAt(6, "TX.Freq.")
            pl.LeftAt(19, "RX.Freq.")
            pl.LeftAt(30, "Chn")
            pl.LeftAt(35, "Freq.")
            pl.LeftAt(43, "INT.")
            pl.LeftAt(50, "INT.")
            pl.LeftAt(57, "INT.")
            pl.LeftAt(64, "INT.")
            pl.LeftAt(71, "VIC.")
            pl.LeftAt(78, "VIC.")
            pl.LeftAt(84, "VIC.")
            pl.LeftAt(100, "Interference Req.: {0} (", pRec.intreq)
            If pRec.calctype.Trim().Length < 2 Then
                pl.LeftAt(-1, "CALC.)")
            Else
                pl.LeftAt(-1, "TABLE)")
            End If
            pl.Output()

            pl.LeftAt(0, "BS")
            pl.LeftAt(7, "(MHz)")
            pl.LeftAt(20, "(MHz)")
            pl.LeftAt(30, "Sts")
            pl.LeftAt(35, "Sep.")
            pl.LeftAt(43, "TXPwr")
            pl.LeftAt(50, "AFLS")
            pl.LeftAt(57, "Net")
            pl.LeftAt(64, "EIRP")
            pl.LeftAt(71, "Net")
            pl.LeftAt(78, "AFSL")
            pl.LeftAt(84, "RXPwr")
            pl.LeftAt(100, "Loss")
            pl.LeftAt(108, "CALC.")
            pl.LeftAt(117, "REQD.")
            'pl.LeftAt(127, "----");
            pl.Output()

            pl.LeftAt(0, " E")
            pl.LeftAt(7, "STN.E")
            pl.LeftAt(20, "STN.T")
            pl.LeftAt(30, "T.R")
            pl.LeftAt(35, "(MHz)")
            pl.LeftAt(43, "(dBw)")
            pl.LeftAt(57, "ATGn")
            pl.LeftAt(64, "(dBw)")
            pl.LeftAt(71, "AtGn")
            pl.LeftAt(84, "(dBm)")
            pl.LeftAt(100, "(dB)")
            If pRec.calctype.Equals("I") Then
                pl.RightAt(110, "-I")
                pl.RightAt(119, "-I")
            ElseIf pRec.calctype.Equals("C") Then
                pl.RightAt(110, "C/I")
                pl.RightAt(119, "C/I")
            Else
                pl.RightAt(110, pRec.calctype.Trim())
                pl.RightAt(119, pRec.calctype.Trim())
            End If
            pl.LeftAt(124, "Margin")
            pl.Output()

            pl.LeftAt(0, "---")
            pl.LeftAt(4, "------.---- ")
            pl.LeftAt(17, "------.---- ")
            pl.LeftAt(30, "-.-")
            pl.LeftAt(34, "----.--")
            pl.LeftAt(43, "--.--")
            pl.LeftAt(50, "--.-")
            pl.LeftAt(55, "---.--")
            pl.LeftAt(63, "---.--")
            pl.LeftAt(70, "---.--")
            pl.LeftAt(78, "--.-")
            pl.LeftAt(83, "----.--")
            pl.LeftAt(100, "---.--")
            pl.LeftAt(107, "----.--")
            pl.LeftAt(116, "----.--")
            pl.LeftAt(124, "----.--")
            pl.Output()
        End Sub

        ''' <summary>
        ''' This method produces the text for the detail record.  
        ''' </summary>
        ''' <paramname="pl"> - PrintLine object used for text formatting and accumulation.</param>
        ''' <paramname="pRec"> - EsTsSelRec object providing source data.</param>
        Private Shared Sub EsTs3Det(pl As TpRunTsip.Estsrp3.PrintLineEsTs3, pRec As EsTsSelRec)
            Dim intfreqtxr As Double
            Dim vicfreqrxr As Double
            Dim freqsepr As Double

            If pl.LineNumber() + 4 > pl.LastLine() Then
                pl.PageHeader()
            End If
            pl.IntAt(0, "{0,3:D}", pRec.etsubcaseno)
            intfreqtxr = pRec.intfreqtx / 1000.0
            pl.RightAt(14, GenUtil.Ccommas(intfreqtxr, 4))     ' ("ZZ,ZZn.nnnn"),
            vicfreqrxr = pRec.vicfreqrx / 1000.0
            pl.RightAt(27, GenUtil.Ccommas(vicfreqrxr, 4)) ' ("ZZ,ZZn.nnnn"),
            pl.LeftAt(30, pRec.stattx)
            pl.LeftAt(32, pRec.statrx)
            freqsepr = pRec.freqsep / 1000.0
            pl.DoubleAt(34, "{0,7:F2}", freqsepr)  ' ("-ZZn.nn"),
            pl.DoubleAt(43, "{0,5:F2}", pRec.inttxpwr)
            pl.DoubleAt(50, "{0,4:F1}", pRec.inttxafls)
            pl.DoubleAt(55, "{0,6:F2}", pRec.earthmdsc)
            pl.DoubleAt(63, "{0,6:F2}", pRec.eartheirp)
            pl.DoubleAt(70, "{0,6:F2}", pRec.terrmdsc)
            pl.DoubleAt(78, "{0,4:F1}", pRec.vicrxafls)
            pl.DoubleAt(83, "{0,7:F2}", pRec.vicpwrrx)
            If pRec.spherecalc.Equals("2") Then
                pl.LeftAt(93, "20% S:")
            Else
                pl.LeftAt(93, "20% T:")
            End If
            pl.DoubleAt(100, "{0,6:F2}", pRec.loss20mode1)
            pl.DoubleAt(107, "{0,7:F2}", pRec.calci20mode1)
            pl.DoubleAt(116, "{0,7:F2}", pRec.reqd20mode1)
            pl.DoubleAt(124, "{0,7:F2}", pRec.marg20mode1)
            pl.Output()

            pl.LeftAt(20, "Station U Ant.Gain:")
            pl.DoubleAt(43, "{0,4:F1}", pRec.terragain2)
            pl.LeftAt(65, "Energy Dispersal:")
            pl.DoubleAt(85, "{0,5:F2}", pRec.energy)
            pl.LeftAt(92, ".01% T:")
            pl.DoubleAt(100, "{0,6:F2}", pRec.loss01mode1)
            pl.DoubleAt(107, "{0,7:F2}", pRec.calci01mode1)
            pl.DoubleAt(116, "{0,7:F2}", pRec.reqd01mode1)
            pl.DoubleAt(124, "{0,7:F2}", pRec.marg01mode1)
            pl.Output()

            pl.LeftAt(63, "20% RAC/TAC Factor:")
            If pRec.intreq.Equals("FCSA") Then
                pl.LeftAt(84, "-13.49")
            Else
                pl.LeftAt(84, "-23.49")
            End If
            pl.LeftAt(92, ".01% P:")
            pl.DoubleAt(100, "{0,6:F2}", pRec.loss01mode2)
            pl.DoubleAt(107, "{0,7:F2}", pRec.calci01mode2)
            pl.DoubleAt(116, "{0,7:F2}", pRec.reqd01mode2)
            pl.DoubleAt(124, "{0,7:F2}", pRec.marg01mode2)
            pl.Output()

            Return
        End Sub


    End Class
End Namespace
