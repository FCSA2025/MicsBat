Imports _Configuration
Imports _NewLib
Imports _Utillib
Imports _DataStructures
Imports System
Imports System.IO
Imports SQLCHARPTR = System.String            'Invented to mimic (char *) for [In]  only.
Imports SQLHANDLE = System.IntPtr
Imports SQLHDBC = System.IntPtr
Imports SQLLEN = System.Int64
Imports SQLRETURN = System.Int16
Imports System.Runtime.InteropServices

Namespace TpRunTsip

    ''' <summary>
    ''' Provides methods used to produce the Case Detail Report (.CASEDET) 
    ''' for Es-Ts interference.
    ''' </summary>
    Public Class Tsesrp3
        Private Shared mFile As String
        Private Shared tRec As TsEsSelRec
        Private Shared mLastCase As Integer
        Private Shared mLastSubCase As Integer

        ''' <summary>
        ''' This class extends the PrintLine base class to provide page header text
        ''' specific to the report type.
        ''' </summary>
        Private Class PrintLineTsEs3
            Inherits TpRunTsip.PrintLine
            ''' <summary>
            ''' This method overrides that of the PrintLine base class to provide
            ''' page header text specific to the report type.
            ''' </summary>
            ''' <paramname=""></param>
            Public Overrides Sub PageHeader()
                MyBase.PageBefore()

                TpRunTsip.Tsesrp3.TsEs3Hdr(Me, TpRunTsip.Tsesrp3.tRec)
            End Sub
        End Class

        ''' <summary>
        ''' This method retrieves TSIP results data from the DB and produces
        ''' the bulk of the text for the CASEDET report.
        ''' </summary>
        ''' <paramname="tw"> - the TextWriter object assigned to the .CASEDET report.</param>
        ''' <paramname="ttName"> - the unique ID substring of the Te DB table names.</param>
        ''' <paramname="tpParm"> - the TpParm object providing parameter data.</param>
        ''' <returns></returns>
        Public Shared Function TsEsRp3(tw As TextWriter, ttName As String, tpParm As TpParm) As Integer
            '...Log2.v("\nTsesrp3.TsEsRp3(): Entry");

            Dim hConn As SQLHDBC = Ssutil.NewConn()
            Dim hStmt As SQLHANDLE
            Dim sqlRet As SQLRETURN
            Dim nullInd As SQLLEN

            Dim cSQL As String

            Dim pRec As TsEsSelRec = New TsEsSelRec()

            'tRec is 'global' and so can be accessed by the PageHeader override.
            TpRunTsip.Tsesrp3.tRec = pRec

            Dim pl As TpRunTsip.Tsesrp3.PrintLineTsEs3 = New TpRunTsip.Tsesrp3.PrintLineTsEs3()
            pl.OutFile(tw)
            pl.FormFeed() ' Just to mimic the behaviour of the MICS legacy C++ code.

            Call TpRunTsip.Tsesrp3.TsInitCase()       '	Initialize the case counters.

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)

            'mFile is 'global' and so can be accessed by the PageHeader override.
            TpRunTsip.Tsesrp3.mFile = ttName

            cSQL = SQLCHARPTR.Format("SELECT protype,envtype,coordist,fsep,analopt,margin,a.terrcall1,a.terrcall2,a.earthlocation,terrname1,terrname2,earthname,terroper,terroper2,earthoper,terrlatit,terrlongit, terrgrnd,earthlatit,earthlongit,earthgrnd,radiozone,rainzone,a.etreport,a.tereport,etcaseno,tecaseno,etsubcases,tesubcases,intreq,etdist,etazim,teazim,tudist,tuazim,utazim,eudist,euazim,ueazim,a.processed,b.interferer,b.terrbndcde,b.terranum,b.earthcall1,earthband,terracode,earthacode,satname, satoper,satlongit,txpre,txtro,rxpre,rxtro,sarc1,sarc2,mode1,mode2,intause,b.etreport,b.tereport,b.processed,etsubcaseno,tesubcaseno,esazim,eselev,teelev,etelev,tuelev,utelev,euelev,ediscang,tdiscang,adisc_set,adisc_ute,terrht,earthht,tvazim,evazim,tvelev,evelev,tvdistes,tvdisttu,evdistes,evdisttu,angleutv,anglesev,terrchid,earthchid,inttraftx,victrafrx,inteqpttx,viceqptrx,intfreqtx,vicfreqrx,freqsep,inttxpwr,inttxafls,inttxpwr2,inttxafls2,vicpwrrx,vicrxafls,c.etreport,c.tereport,ctxinttraftx,ctxvictrafrx,ctxeqpt,calctype,earthmdsc,terrmdsc,eartheirp,terreirp,scang,loss20mode1,calci20mode1,reqd20mode1,marg20mode1,loss01mode1,calci01mode1,reqd01mode1,marg01mode1,loss01mode2,calci01mode2,reqd01mode2,marg01mode2,stattx,statrx,energy,c.processed,terrant,terraxref,terramodel,terragain,earthaxref,earthamodel,earthagain,remterragain,tsoffaxis,tstrueaz,tstrueel,angleute,angleuta,angleeta,angleatv,adisc_atv" & Microsoft.VisualBasic.Constants.vbTab & "FROM " & Microsoft.VisualBasic.Constants.vbTab & "te_{0}_site a,te_{1}_ante b,te_{2}_chan c,te_{3}_parm WHERE a.terrcall1 = b.terrcall1 and a.terrcall2 = b.terrcall2 and a.earthlocation = b.earthlocation and b.terrcall1 = c.terrcall1 and b.terrcall2 = c.terrcall2 and b.terranum = c.terranum and b.terrbndcde = c.terrbndcde and b.earthlocation = c.earthlocation and b.earthcall1 = c.earthcall1 and b.interferer = c.interferer and c.tereport != 0 and c.interferer = 'T' ORDER BY tecaseno, tesubcaseno, freqsep ", ttName, ttName, ttName, ttName)

            '...Log2.v("\n\nTsesrp3.TsEsRp3(): ttName = " + ttName);
            '...Log2.v("\n\nTsesrp3.TsEsRp3(): cSQL = " + cSQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length)
            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "Tsesrp3.TsEsRp3(): ERROR: SQLExecDirect() failed on query:" & Microsoft.VisualBasic.Constants.vbLf & cSQL)
                Ssutil.DbGetDiagStmt(hStmt, "tsesrp301: Failed to Execute:-")
                Return [Error].ODBC_EXECDIRECT_FAILED
            End If

            While True
                sqlRet = ODBC.SQLFetch(hStmt)
                If sqlRet = Constant.NOMORERECS Then
                    ' If no more records available then break out of the while-loop.
                    Exit While
                ElseIf Not ODBC.IsOK(sqlRet) Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "Tsesrp3.TsEsRp3(): ERROR: SQLFetch() failed.")
                    Ssutil.DbGetDiagStmt(hStmt, "tsesrp302: Failed Fetch:")
                    Return -2
                End If

                Try
                    Ssutil.DbStartGets()
                    Ssutil.DbGetString(hStmt, 0, "protype", pRec.protype, TsEsSelRec.PROTYPE, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "envtype", pRec.envtype, TsEsSelRec.ENVTYPE, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "coordist", pRec.coordist, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "fsep", pRec.fsep, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "analopt", pRec.analopt, TsEsSelRec.ANALOPT, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "margin", pRec.margin, nullInd)

                    ' site
                    Ssutil.DbGetString(hStmt, 0, "terrcall1", pRec.terrcall1, TsEsSelRec.TERRCALL1, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terrcall2", pRec.terrcall2, TsEsSelRec.TERRCALL2, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthlocation", pRec.earthlocation, TsEsSelRec.EARTHLOCATION, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terrname1", pRec.terrname1, TsEsSelRec.TERRNAME1, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terrname2", pRec.terrname2, TsEsSelRec.TERRNAME2, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthname", pRec.earthname, TsEsSelRec.EARTHNAME, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terroper", pRec.terroper, TsEsSelRec.TERROPER, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terroper2", pRec.terroper2, TsEsSelRec.TERROPER2, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthoper", pRec.earthoper, TsEsSelRec.EARTHOPER, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "terrlatit", pRec.terrlatit, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "terrlongit", pRec.terrlongit, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "terrgrnd", pRec.terrgrnd, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "earthlatit", pRec.earthlatit, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "earthlongit", pRec.earthlongit, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "earthgrnd", pRec.earthgrnd, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "radiozone", pRec.radiozone, TsEsSelRec.RADIOZONE, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "rainzone", pRec.rainzone, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "siteetrep", pRec.siteetrep, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "siteterep", pRec.siteterep, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "etcaseno", pRec.etcaseno, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "tecaseno", pRec.tecaseno, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "etsubcases", pRec.etsubcases, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "tesubcases", pRec.tesubcases, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "intreq", pRec.intreq, TsEsSelRec.INTREQ, nullInd)
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
                    Ssutil.DbGetString(hStmt, 0, "interferer", pRec.interferer, TsEsSelRec.INTERFERER, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terrbndcde", pRec.terrbndcde, TsEsSelRec.TERRBNDCDE, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "terranum", pRec.terranum, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthcall1", pRec.earthcall1, TsEsSelRec.EARTHCALL1, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthband", pRec.earthband, TsEsSelRec.EARTHBAND, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terracode", pRec.terracode, TsEsSelRec.TERRACODE, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthacode", pRec.earthacode, TsEsSelRec.EARTHACODE, nullInd)
                    'ifnull(satname; '') as sname;
                    Ssutil.DbGetString(hStmt, 0, "satname", pRec.satname, TsEsSelRec.SATNAME, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "satoper", pRec.satoper, TsEsSelRec.SATOPER, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "satlongit", pRec.satlongit, nullInd)
                    Ssutil.DbGetFloat(hStmt, 0, "txpre", pRec.txpre, nullInd)
                    Ssutil.DbGetFloat(hStmt, 0, "txtro", pRec.txtro, nullInd)
                    Ssutil.DbGetFloat(hStmt, 0, "rxpre", pRec.rxpre, nullInd)
                    Ssutil.DbGetFloat(hStmt, 0, "rxtro", pRec.rxtro, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "sarc1", pRec.sarc1, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "sarc2", pRec.sarc2, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "mode1", pRec.mode1, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "mode2", pRec.mode2, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "intause", pRec.intause, TsEsSelRec.INTAUSE, nullInd)
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
                    Ssutil.DbGetString(hStmt, 0, "terrchid", pRec.terrchid, TsEsSelRec.TERRCHID, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthchid", pRec.earthchid, TsEsSelRec.EARTHCHID, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "inttraftx", pRec.inttraftx, TsEsSelRec.INTTRAFTX, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "victrafrx", pRec.victrafrx, TsEsSelRec.VICTRAFRX, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "inteqpttx", pRec.inteqpttx, TsEsSelRec.INTEQPTTX, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "viceqptrx", pRec.viceqptrx, TsEsSelRec.VICEQPTRX, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "intfreqtx", pRec.intfreqtx, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "vicfreqrx", pRec.vicfreqrx, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "freqsep", pRec.freqsep, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "inttxpwr", pRec.inttxpwr, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "inttxafls", pRec.inttxafls, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "inttxpwr2", pRec.inttxpwr2, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "inttxafls2", pRec.inttxafls2, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "vicpwrrx", pRec.vicpwrrx, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "vicrxafls", pRec.vicrxafls, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "chanetrep", pRec.chanetrep, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "chanterep", pRec.chanterep, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "ctxinttraftx", pRec.ctxinttraftx, TsEsSelRec.CTXINTTRAFTX, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "ctxvictrafrx", pRec.ctxvictrafrx, TsEsSelRec.CTXVICTRAFRX, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "ctxeqpt", pRec.ctxeqpt, TsEsSelRec.CTXEQPT, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "calctype", pRec.calctype, TsEsSelRec.CALCTYPE, nullInd)
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
                    Ssutil.DbGetString(hStmt, 0, "stattx", pRec.stattx, TsEsSelRec.STATTX, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "statrx", pRec.statrx, TsEsSelRec.STATRX, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "energy", pRec.energy, nullInd)
                    Ssutil.DbGetInt(hStmt, 0, "chanproc", pRec.chanproc, nullInd)
                    Ssutil.DbGetShort(hStmt, 0, "terrant", pRec.terrant, nullInd)

                    '	antenna again.
                    Ssutil.DbGetString(hStmt, 0, "terraxref", pRec.terraxref, TsEsSelRec.TERRAXREF, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "terramod", pRec.terramod, TsEsSelRec.TERRAMOD, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "terragain", pRec.terragain, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthaxref", pRec.earthaxref, TsEsSelRec.EARTHAXREF, nullInd)
                    Ssutil.DbGetString(hStmt, 0, "earthamod", pRec.earthamod, TsEsSelRec.EARTHAMOD, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "earthagain", pRec.earthagain, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "terragain2", pRec.terragain2, nullInd)  ' in channel

                    Ssutil.DbGetString(hStmt, 0, "tsoffaxis", pRec.tsoffaxis, TsEsSelRec.TSOFFAXIS, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "tstrueaz", pRec.tstrueaz, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "tstrueel", pRec.tstrueel, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "angleute", pRec.angleute, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "angleuta", pRec.angleuta, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "angleeta", pRec.angleeta, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "angleatv", pRec.angleatv, nullInd)
                    Ssutil.DbGetDouble(hStmt, 0, "adisc_atv", pRec.adisc_atv, nullInd)
                Catch e As Exception
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "Tsesrp3.TsEsRp3(): ERROR: an ODBC 'Get' failed: " & e.Message)
                    Ssutil.DbGetDiagStmt(hStmt, "estsrp303: Failed retrieving " & e.Message)
                    Return [Error].ODBC_GET_FAILED
                End Try

                TpRunTsip.Tsesrp3.ReportTeDetail(pl, ttName, pRec)

            End While ' End of while loop (SQL fetches).

            pl.Output()
            pl.IntAt(0, "Number of reported TS-ES cases: {0}", tpParm.numtecases)
            pl.Output()

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
            sqlRet = CShort(Ssutil.DisConn(hConn))

            '...Log2.v("\nTsesrp3.TsEsRp3(): Exit");
            '&&Console.Error.Write("\ntsesrp3.tsesrp3(): pRec.esazim = {0}", pRec.esazim);
            Return 0
        End Function

        ''' <summary>
        ''' This method produces the text used for the TsEs case header.  
        ''' </summary>
        ''' <paramname="pl"> - PrintLine object with header override.</param>
        ''' <paramname="pRec"> - TsEsSelRec object providing results data.</param>
        Private Shared Sub TsEs3Hdr(pl As TpRunTsip.Tsesrp3.PrintLineTsEs3, pRec As TsEsSelRec)
            Dim [cDate] As String
            Dim cTime As String
            GenUtil.UtGetDateTime([cDate], cTime)

            pl.LeftAt(0, "Frequency Coordination System Association")
            pl.IntAt(114, "Page:{0,3:D}", pl.PageNumber)
            pl.Output()

            pl.LeftAt(0, "MASTER DATA BASE -- TS to ES Interference Study Report - Case Detail")
            pl.LeftAt(114, "Date:{0,10}", [cDate])
            pl.Output()
            pl.LeftAt(114, "Time:{0,10}", cTime)
            pl.Output()

            pl.LeftAt(69, "Environment  : ")
            pl.LeftAt(85, pRec.envtype)
            pl.LeftAt(101, "Coord. Dist.: {0} km", GenUtil.Ccommas(pRec.coordist, 2))
            pl.Output()

            pl.DoubleAt(0, "Maximum Frequency Separation: {0,7:F2}", pRec.fsep)
            pl.Output()

            Return
        End Sub

        ''' <summary>
        ''' This method provides initialization of internal counters.  
        ''' </summary>
        ''' <paramname=""></param>
        Private Shared Sub TsInitCase()
            TpRunTsip.Tsesrp3.mLastCase = -1
            TpRunTsip.Tsesrp3.mLastSubCase = -1
        End Sub

        ''' <summary>
        ''' This method produces the CASEDET text fragment one set of site, ante and channel data.  
        ''' </summary>
        ''' <paramname="pl"> - PrintLine object with header override.</param>
        ''' <paramname="ttName"> - the unique ID substring of the Te DB table names.</param>
        ''' <paramname="pRec"> - TsEsSelRec object providing results data.</param>
        Private Shared Sub ReportTeDetail(pl As TpRunTsip.Tsesrp3.PrintLineTsEs3, ttName As String, pRec As TsEsSelRec)
            '...Log2.v("\n\nTsesrp3.ReportTeDetail(): pRec.tecaseno, pRec.tesubcaseno: " + pRec.tecaseno + " :: " + pRec.tesubcaseno);

            Dim IsFirstCase As Boolean

            If Not TpRunTsip.Tsesrp3.TsIsDupCase(pRec, IsFirstCase) Then
                If IsFirstCase Then
                    pl.PageHeader()
                End If
                TpRunTsip.Tsesrp3.TeCaseHdr(pl, pRec)
            End If

            If Not TpRunTsip.Tsesrp3.TsIsDupSubCase(pRec) Then
                TpRunTsip.Tsesrp3.TeSubCaseHdr(pl, pRec)
            End If

            TpRunTsip.Tsesrp3.TsEs3Det(pl, pRec)

            Return
        End Sub

        ''' <summary>
        ''' This method detects whether the current case is a duplicate of a previous interference case.  
        ''' </summary>
        ''' <paramname="pRec"> - TsEsSelRec object providing results data.</param>
        ''' <paramname="IsFirstCase"> - boolean, true if *not* duplicate.</param>
        ''' <returns></returns>
        Private Shared Function TsIsDupCase(pRec As TsEsSelRec, <Out> ByRef IsFirstCase As Boolean) As Boolean
            Dim [Is] As Boolean

            IsFirstCase = TpRunTsip.Tsesrp3.mLastCase = -1
            If pRec.tecaseno = TpRunTsip.Tsesrp3.mLastCase Then
                [Is] = True
            Else
                [Is] = False
                TpRunTsip.Tsesrp3.mLastCase = pRec.tecaseno
                TpRunTsip.Tsesrp3.mLastSubCase = -1   '	Reset the subcase.
            End If

            '...Log2.v("\n\nTsesrp3.TsIsDupCase(): isDup, pRec.tecaseno, pRec.tesubcaseno: " + Is + " :: " + pRec.tecaseno + " :: " + pRec.tesubcaseno);

            Return [Is]
        End Function

        ''' <summary>
        ''' This method tests whether a subcase is a duplicate of a previous subcase.  
        ''' </summary>
        ''' <paramname="pRec">- TsEsSelRec object providing results data.</param>
        ''' <returns></returns>
        Private Shared Function TsIsDupSubCase(pRec As TsEsSelRec) As Boolean
            Dim [Is] As Boolean

            If pRec.tesubcaseno = TpRunTsip.Tsesrp3.mLastSubCase Then
                [Is] = True
            Else
                [Is] = False
                TpRunTsip.Tsesrp3.mLastSubCase = pRec.tesubcaseno
            End If

            Return [Is]
        End Function

        ''' <summary>
        ''' This method produces text for the case header that preceeds the results
        ''' data for a Te interference case.  
        ''' </summary>
        ''' <paramname="pl"> - PrintLine object with header override.</param>
        ''' <paramname="pRec">- TsEsSelRec object providing results data.</param>
        Private Shared Sub TeCaseHdr(pl As TpRunTsip.Tsesrp3.PrintLineTsEs3, pRec As TsEsSelRec)
            pl.PageBefore()
            pl.IntAt(0, "Case Number : {0}", pRec.tecaseno)
            If pRec.protype.Equals("T") Then
                pl.LeftAt(29, "Interferer from Proposed File")
            Else
                pl.LeftAt(29, "Interferer from Environment File")
            End If

            pl.LeftAt(69, "Analysis Type: ")
            pl.LeftAt(85, pRec.analopt)
            pl.LeftAt(101, "PDF File    : ")
            pl.LeftAt(115, TpRunTsip.Tsesrp3.mFile)
            pl.Output()

            pl.LeftAt(69, "Margin       : ")
            pl.DoubleAt(85, "{0,6:F2}", pRec.margin)
            pl.Output()

            pl.LeftAt(0, "+-")
            pl.LeftAt(10, "Site Information")
            pl.LeftAt(100, "-+")
            pl.LeftAt(106, "Notes: Interference Case")
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
            pl.LeftAt(73, "Satellite Service")
            pl.LeftAt(91, "|")
            pl.LeftAt(93, "Radio/")
            pl.LeftAt(101, "|")
            pl.LeftAt(113, "Yes ____  No ____")
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

            pl.LeftAt(0, pRec.terrcall1)
            pl.LeftAt(10, pRec.terrcall2)
            pl.LeftAt(20, pRec.terrname1)
            If pRec.terrname1.Trim().Length + pRec.terrname2.Trim().Length > 33 Then
                pl.LeftAt(-1, "->")
            Else
                pl.LeftAt(54 - CInt(pRec.terrname2.Length), pRec.terrname2)
            End If

            pl.LeftAt(57, pRec.terroper)
            pl.LeftAt(64, pRec.terroper2)
            pl.DoubleAt(74, "{0,6:F2}", pRec.sarc1)
            pl.DoubleAt(84, "{0,5:F2}", pRec.sarc2)
            pl.LeftAt(94, pRec.radiozone)
            pl.ShortAt(97, "{0}", pRec.rainzone)
            If pRec.tsoffaxis.Equals("Y") Then
                pl.LeftAt(102, "Offaxis")
            End If
            pl.LeftAt(113, "Blockage: ____ dB")
            pl.Output()

            If pRec.terrname1.Trim().Length + pRec.terrname2.Trim().Length > 33 Then
                pl.LeftAt(54 - CInt(pRec.terrname2.Length), pRec.terrname2)
                pl.Output()
            End If

            pl.LeftAt(0, pRec.earthcall1)
            pl.LeftAt(10, pRec.earthlocation)
            pl.LeftAt(20, pRec.earthname)
            pl.LeftAt(54 - pRec.satname.Trim().Length, pRec.satname)
            pl.LeftAt(57, pRec.earthoper)
            pl.LeftAt(64, pRec.satoper)
            pl.DoubleAt(74, "Longitude: {0,6:F2}", pRec.satlongit / 360000.0)
            pl.LeftAt(92, "Deg. W")

            If pRec.tsoffaxis.Equals("Y") Then
                pl.LeftAt(104, "(A)")
            End If
            pl.LeftAt(113, "At K=___ ")
            pl.Output()

            Return
        End Sub

        ''' <summary>
        ''' This method produces text for the subcase header that preceeds the results
        ''' data for a Te interference subcase. 
        ''' </summary>
        ''' <paramname="pl"> - PrintLine object with header override.</param>
        ''' <paramname="pRec"> - TsEsSelRec object providing results data.</param>
        Private Shared Sub TeSubCaseHdr(pl As TpRunTsip.Tsesrp3.PrintLineTsEs3, pRec As TsEsSelRec)
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

            pl.LeftAt(1, "T -> E")
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

            pl.LeftAt(1, "T -> U")
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

            pl.LeftAt(1, "E -> U")
            pl.DoubleAt(10, "{0,6:F2}", pRec.eudist)
            pl.DoubleAt(20, "{0,6:F2}", pRec.euazim)
            pl.DoubleAt(29, "{0,6:F2}", pRec.euelev)
            pl.LeftAt(36, "|")
            pl.LeftAt(39, "UTE:")
            pl.DoubleAt(45, "{0,6:F2}", pRec.angleute)
            If Not pRec.tsoffaxis.Equals("Y") Then
                pl.DoubleAt(55, "{0,6:F2}", pRec.adisc_ute)
            End If
            pl.LeftAt(64, "|")
            pl.Output()

            pl.LeftAt(1, "E -> T")
            pl.DoubleAt(10, "{0,6:F2}", pRec.etdist)
            pl.DoubleAt(20, "{0,6:F2}", pRec.etazim)
            pl.DoubleAt(29, "{0,6:F2}", pRec.etelev)
            pl.LeftAt(36, "|")

            If pRec.tsoffaxis.Equals("Y") Then
                pl.LeftAt(39, "UTA:")
                pl.DoubleAt(45, "{0,6:F2}", pRec.angleuta)
            End If
            pl.LeftAt(64, "|")
            pl.LeftAt(70, "Angle SEV:")
            pl.DoubleAt(81, "{0,6:F2}", pRec.anglesev)
            pl.LeftAt(94, "Scang:")
            pl.DoubleAt(101, "{0,6:F2}", pRec.scang)
            pl.Output()

            pl.LeftAt(1, "E -> S")
            pl.DoubleAt(20, "{0,6:F2}", pRec.esazim)
            pl.DoubleAt(29, "{0,6:F2}", pRec.eselev)
            pl.LeftAt(36, "|")

            If pRec.tsoffaxis.Equals("Y") Then
                pl.LeftAt(39, "ETA:")
                pl.DoubleAt(45, "{0,6:F2}", pRec.angleeta)
                pl.DoubleAt(55, "{0,6:F2}", pRec.adisc_ute)
            End If
            pl.LeftAt(64, "+")
            pl.LeftAt(70, "Angle UTV:")
            pl.DoubleAt(81, "{0,6:F2}", pRec.angleutv)
            pl.LeftAt(112, "----+")
            pl.Output()

            If pRec.tsoffaxis.Equals("Y") Then
                pl.LeftAt(1, "T -> A")
                pl.DoubleAt(20, "{0,6:F2}", pRec.tstrueaz)
                pl.DoubleAt(29, "{0,6:F2}", pRec.tstrueel)
                pl.LeftAt(36, "|")
                pl.LeftAt(70, "Angle ATV:")
                pl.DoubleAt(81, "{0,6:F2}", pRec.angleatv)
                pl.LeftAt(94, "Adisc:")
                pl.DoubleAt(101, "{0,6:F2}", pRec.adisc_atv)
                pl.Output()
            End If
            pl.Output()

            ' Antenna Information 
            pl.LeftAt(0, "-- TX Interferer Stn.Ant.Sys.Info")
            pl.LeftAt(-1, "  ----- Station T")
            pl.LeftAt(-1, " --- TS ---")
            pl.LeftAt(63, "*+*")
            pl.LeftAt(68, "-- RX   Victim   Stn.Ant.Sys.Info")
            pl.LeftAt(-1, "  ----- Station E   --- ES  -")
            pl.Output()

            pl.ShortAt(0, "Antenna #: {0}", pRec.terranum)
            pl.LeftAt(18, "AUse: {0}", pRec.intause)
            pl.DoubleAt(31, "Gain:{0,5:F1}", pRec.terragain)
            pl.LeftAt(42, "dB")
            If pRec.tsoffaxis.Equals("Y") Then
                pl.LeftAt(47, "Adisc:")
                pl.DoubleAt(54, "{0,6:F2}", pRec.adisc_atv)
            End If
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "Antenna #: ")
            pl.DoubleAt(98, "Gain:{0,5:F1}", pRec.earthagain)
            pl.LeftAt(109, "dB")
            pl.LeftAt(119, "Coord Dist.")
            pl.Output()

            pl.LeftAt(0, "TX.Ant.Code: {0}", pRec.terracode)
            pl.LeftAt(31, "Model:")
            pl.LeftAt(-1, pRec.terramod)
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "RX.Ant.Code: ")
            pl.LeftAt(-1, pRec.earthacode)
            pl.LeftAt(98, "Model:")
            pl.LeftAt(-1, pRec.earthamod)
            pl.FloatAt(119, "Trop:{0,4:F0}", pRec.rxtro)
            pl.LeftAt(-1, " km")
            pl.Output()

            pl.LeftAt(0, "Xref... Ant: ")
            If pRec.terraxref.Length = 0 Then
                pl.LeftAt(-1, pRec.terracode)
            Else
                pl.LeftAt(-1, pRec.terraxref)
            End If
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "Xref... Ant: ")

            If pRec.earthaxref.Length = 0 Then
                pl.LeftAt(-1, pRec.earthacode)
            Else
                pl.LeftAt(-1, pRec.earthaxref)
            End If
            pl.DoubleAt(119, "Prec:{0,4:F0} km", pRec.rxpre)
            pl.Output()

            pl.LeftAt(2, "Ant.Elev.: ")
            pl.DoubleAt(13, "{0,7:F2}", pRec.terrht)
            pl.LeftAt(21, "m (AMSL)")
            pl.LeftAt(32, "TX.Ant.Ht: ")
            pl.DoubleAt(-1, "{0,6:F1}", pRec.terrht - pRec.terrgrnd)
            pl.LeftAt(50, "m (AGL)")
            pl.LeftAt(64, "|")
            pl.LeftAt(70, "Ant.Elev.: ")
            pl.DoubleAt(81, "{0,7:F2}", pRec.earthht)
            pl.LeftAt(89, "m (AMSL)")
            pl.LeftAt(100, "RX.Ant.Ht: ")
            pl.DoubleAt(-1, "{0,6:F1}", pRec.earthht - pRec.earthgrnd)
            pl.LeftAt(118, "m (AGL)")
            pl.Output()

            pl.LeftAt(2, "Stn.Elev.: ")
            pl.DoubleAt(-1, "{0,7:F2}", pRec.terrgrnd)
            pl.LeftAt(-1, " m (AMSL)")
            pl.LeftAt(64, "+")
            pl.LeftAt(70, "Stn.Elev.: ")
            pl.DoubleAt(-1, "{0,7:F2} m (AMSL)", pRec.earthgrnd)
            pl.Output(2)

            pl.LeftAt(0, "-- TX Interferer Sys.Eqp.CTX.Band Info")
            pl.LeftAt(-1, " - Station T")
            pl.LeftAt(-1, " --- TS ---")
            pl.LeftAt(63, "*+*")
            pl.LeftAt(68, "-  RX Victim Sys.Eqp.CTX.Band Info")
            pl.LeftAt(-1, " ----- Station E")
            pl.LeftAt(-1, "   --- ES  -")
            pl.Output()

            pl.LeftAt(0, "Eqpt. Planned / Traffic Code: ")
            pl.LeftAt(-1, pRec.inteqpttx)
            pl.LeftAt(40, pRec.inttraftx)
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "Eqpt. Planned / Traffic Code: ")
            pl.LeftAt(-1, pRec.viceqptrx)
            pl.LeftAt(108, pRec.victrafrx)
            pl.Output()

            pl.LeftAt(6, "Default   Traffic Code: ")
            pl.LeftAt(40, pRec.ctxinttraftx)
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "Eqpt. Default / Traffic Code: ")
            pl.LeftAt(-1, pRec.ctxeqpt)
            pl.LeftAt(108, pRec.ctxvictrafrx)
            pl.Output()

            pl.LeftAt(19, "Freq.Bnd : ")
            pl.LeftAt(-1, pRec.terrbndcde)
            pl.LeftAt(64, "+")
            pl.LeftAt(87, "Freq.Bnd : ")
            pl.LeftAt(-1, pRec.earthband)
            pl.Output()

            pl.LeftAt(0, "-")
            pl.LeftAt(64, "-")
            pl.RightAt(pl.LastPos(), "-")
            pl.Output()

            ' Case information 
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
            pl.LeftAt(100, "Interference Req.: ")
            pl.LeftAt(-1, pRec.intreq)
            pl.LeftAt(-1, " (")
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
            ' .t 127  .pr  "----" 
            pl.Output()

            pl.LeftAt(0, " E")
            pl.LeftAt(7, "STN.T")
            pl.LeftAt(20, "STN.E")
            pl.LeftAt(30, "T.R")
            pl.LeftAt(35, "(MHz)")
            pl.LeftAt(43, "(dBm)")
            pl.LeftAt(57, "ATGn")
            pl.LeftAt(64, "(dBw)")
            pl.LeftAt(71, "AtGn")
            pl.LeftAt(84, "(dBw)")
            pl.LeftAt(100, "(dB)")

            If pRec.calctype.Equals("I") Then
                pl.RightAt(110, "-I")
            ElseIf pRec.calctype.Equals("C") Then
                pl.RightAt(110, "C/I")
            Else
                pl.RightAt(110, pRec.calctype.Trim())
            End If

            If pRec.calctype.Equals("I") Then
                pl.RightAt(119, "-I")
            ElseIf pRec.calctype.Equals("C") Then
                pl.RightAt(119, "C/I")
            Else
                pl.RightAt(119, pRec.calctype)
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

            Return
        End Sub

        ''' <summary>
        ''' This method produces the CASEDET text fragment providing detailed TSIP results for a Ts-Es
        ''' interference case.
        ''' </summary>
        ''' <paramname="pl"> - PrintLine object with header override.</param>
        ''' <paramname="pRec"> - TsEsSelRec object providing results data.</param>
        Private Shared Sub TsEs3Det(pl As TpRunTsip.Tsesrp3.PrintLineTsEs3, pRec As TsEsSelRec)
            Dim intfreqtxr As Double
            Dim vicfreqrxr As Double
            Dim freqsepr As Double

            '.DETAIL  .NEED 7
            If pl.LineNumber() + 4 > pl.LastLine() Then
                pl.PageBefore()
            End If

            pl.IntAt(0, "{0,3:D}", pRec.tesubcaseno)
            intfreqtxr = pRec.intfreqtx / 1000.0
            pl.RightAt(14, GenUtil.Ccommas(intfreqtxr, 4))
            vicfreqrxr = pRec.vicfreqrx / 1000.0
            pl.RightAt(27, GenUtil.Ccommas(vicfreqrxr, 4))
            pl.LeftAt(30, pRec.stattx)
            pl.LeftAt(32, pRec.statrx)
            freqsepr = pRec.freqsep / 1000.0
            pl.DoubleAt(34, "{0,7:F2}", freqsepr)
            pl.DoubleAt(43, "{0,5:F2}", pRec.inttxpwr)
            pl.DoubleAt(50, "{0,4:F2}", pRec.inttxafls)
            pl.DoubleAt(55, "{0,6:F2}", pRec.terrmdsc)
            pl.DoubleAt(63, "{0,6:F2}", pRec.terreirp)
            pl.DoubleAt(70, "{0,6:F2}", pRec.earthmdsc)
            pl.DoubleAt(78, "{0,4:F2}", pRec.vicrxafls)
            pl.DoubleAt(83, "{0,7:F2}", pRec.vicpwrrx)
            pl.LeftAt(93, "20% S:")
            pl.DoubleAt(100, "{0,6:F2}", pRec.loss20mode1)
            pl.DoubleAt(107, "{0,7:F2}", pRec.calci20mode1)
            pl.DoubleAt(116, "{0,7:F2}", pRec.reqd20mode1)
            pl.DoubleAt(124, "{0,7:F2}", pRec.marg20mode1)
            pl.Output()

            pl.LeftAt(14, "Station U Ant. gain:")
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
                pl.LeftAt(84, "-17.47")
            Else
                pl.LeftAt(84, "-21.73")
            End If
            pl.LeftAt(92, ".01% P:")
            pl.DoubleAt(100, "{0,6:F2}", pRec.loss01mode2)
            pl.DoubleAt(107, "{0,7:F2}", pRec.calci01mode2)
            pl.DoubleAt(116, "{0,7:F2}", pRec.reqd01mode2)
            pl.DoubleAt(124, "{0,7:F2}", pRec.marg01mode2)
            pl.Output()
        End Sub


    End Class
End Namespace
