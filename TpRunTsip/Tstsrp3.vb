Imports _DataStructures
Imports System
Imports System.IO
Imports _Configuration
Imports _NewLib
Imports _Utillib
Imports System.Runtime.InteropServices
Imports SQLCHARPTR = System.String            'Invented to mimic (char *) for [In]  only.
Imports SQLHANDLE = System.IntPtr
Imports SQLHDBC = System.IntPtr
Imports SQLLEN = System.Int64
Imports SQLRETURN = System.Int16

Namespace TpRunTsip

    ''' <summary>
    ''' Provides methods used to produce the Case Detail Report (.CASEDET) 
    ''' and Over-Horizon Loss (.CASEOHL) Report for Ts-Ts interference.
    ''' </summary>
    Public Class Tstsrp3
        Private Shared lastCase As Integer = -1
        Private Shared lastSubCase As Integer = -1
        Private Const SPACE As String = " "
        Private Const LEFT_PARENTHESIS As String = "("
        Private Const RIGHT_PARENTHESIS As String = ")"
        Private Shared SELECT_TEMPLATE As String = "SELECT a.interferer, a.caseno, subcaseno, a.subcases, a.intcall1, a.intcall2, a.intoper, a.intoper2, a.intname1, a.intname2, a.viccall1, a.viccall2, a.vicoper, a.vicoper2, a.vicname1, a.vicname2, a.int1int2dist, a.vic1vic2dist, a.int1vic1dist, a.intoffax, a.vicoffax, a.intgrnd, a.vicgrnd, b.intaoffax, b.inthopaz, b.intantaz, b.intoffantax, b.vicaoffax, b.vichopaz, b.vicantaz, b.vicoffantax, intvicaz, vicintaz, b.intbndcde, b.intanum, intacode, intause, intgain, vicgain, b.vicbndcde, b.vicanum, vicacode, vicause, adiscctxh, adiscctxv, adisccrxh, adisccrxv, adiscxtxh, adiscxtxv, adiscxrxh, adiscxrxv, totantdisc, intchid,(intfreqtx / 1000) as intfreqtxr,intpolar,intstattx,vicchid,(vicfreqrx / 1000) as vicfreqrxr,vicpolar,vicstatrx,inttraftx,inteqpttx,victrafrx,viceqptrx,vicpwrrx,intpwrtx,intafsltx,vicafslrx,ctxinttraftx,ctxvictrafrx,ctxeqpt,(freqsep / 1000) as freqsepr,patloss,a.distadv,eirpadv,calcico,calcixp,reqdcalc,resti,calctype,rxant,txant,intaxref,intamodel,vicaxref,vicamodel,intelev,vicelev,intvicel,vicintel,intaht,vicaht,tiltdisc,pathloss80,calcico80,calcixp80,reqd80,resti80,pathloss99,calcico99,calcixp99,reqd99,resti99,ohresult,ctxinteqpt,inteqtype,viceqtype,intbwchans,vicbwchans from {0}.tt_{1}_site a, {0}.tt_{1}_ante b, {0}.tt_{1}_chan c where a.intcall1 = b.intcall1 and a.intcall2 = b.intcall2 and a.viccall1 = b.viccall1 and a.viccall2 = b.viccall2 and a.caseno   = b.caseno and a.interferer = b.interferer and c.intcall1 = b.intcall1 and c.intcall2 = b.intcall2 and c.viccall1 = b.viccall1 and c.viccall2 = b.viccall2 and c.caseno   = b.caseno and c.interferer = b.interferer and c.intanum = b.intanum and c.vicanum = b.vicanum and c.intbndcde = b.intbndcde"
        ''' <summary>
        ''' This method retrieves TSIP results data from the DB and produces
        ''' the bulk of the text for the CASEDET and CASEOHL reports.
        ''' </summary>
        ''' <paramname="tw"> - the TextWriter object assigned to the .CASEDET (or CASEOHL) report.</param>
        ''' <paramname="ttName"> - the unique ID substring of the Ts DB table names.</param>
        ''' <paramname="tpParm"> - the TpParm object providing parameter data</param>
        ''' <paramname="IsOhOnly"> - boolean, true if only an OHL report is required.</param>
        ''' <returns></returns>
        Public Shared Function TsTsRp3(tw As TextWriter, ttName As String, tpParm As TpParm, IsOhOnly As Boolean) As Integer
            '...Log2.v("\nTstsrp3.TsTsRp3(): Entry: ttName = " + ttName);

            Dim tRec As TsTsSelRec = New TsTsSelRec()
            'REPPARM rp3Parm;
            Dim ohtrigger As Integer

            Dim hConn As SQLHDBC = Ssutil.NewConn()
            Dim hStmt As SQLHANDLE
            Dim sqlRet As SQLRETURN
            Dim nullInds = New SQLLEN(115) {}

            Dim cSQL As String

            Dim pl As TpRunTsip.PrintLine = New TpRunTsip.PrintLine()
            pl.OutFile(tw)
            pl.FormFeed()

            ohtrigger = If(IsOhOnly, 1, 0)

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)

            cSQL = SQLCHARPTR.Format(TpRunTsip.Tstsrp3.SELECT_TEMPLATE, Info.GlobalSchema, ttName)

            '	Now check for the ohonly type of report;
            If IsOhOnly Then
                Dim cOHSQL As String

                cOHSQL = SQLCHARPTR.Format(" and (c.ohresult = 0 or ((c.ohresult > 0 and (c.ohresult % 1000) <= 100) and (c.resti80 < {0} or c.resti99 < {1})))", tpParm.margin, tpParm.margin)
                cSQL += cOHSQL
            End If

            '	Order it:-
            cSQL += " ORDER BY caseno, subcaseno, intfreqtxr, vicfreqrxr "

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length)
            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "Tstsrp3.TsTsRp3(): ERROR: SQLExecDirect() failed" & Microsoft.VisualBasic.Constants.vbLf & cSQL)
                Ssutil.DbGetDiagStmt(hStmt, "tstsrp301: Failed to Execute:-")
                sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
                sqlRet = CShort(Ssutil.DisConn(hConn))

                Return [Error].ODBC_EXECDIRECT_FAILED
            End If

            '...Log2.v("\nTstsrp3.TsTsRp3(): SQLExecDirect() succeeded:\n" + cSQL);

            While True
                sqlRet = ODBC.SQLFetch(hStmt)
                If sqlRet = Constant.NOMORERECS Then
                    '...Log2.v("\nTstsrp3.TsTsRp3(): SQLFetch() returned sqlRet == Constant.NOMORERECS");
                    Exit While
                ElseIf Not ODBC.IsOK(sqlRet) Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "Tstsrp3.TsTsRp3(): call to SQLFetch() failed.")
                    Ssutil.DbGetDiagStmt(hStmt, "tstsrp302: Failed Fetch:")
                    sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
                    sqlRet = CShort(Ssutil.DisConn(hConn))
                    Return -2
                End If

                NullHelper.FillArray(nullInds, Constant.DB_NULL)

                ' Setup automatic column indexing.
                Ssutil.DbStartGets()

                Try
                    ' NOTE: the 'get' order of the columns is NOT the same as the 'member' order of SelRec.

                    Ssutil.DbGetString(hStmt, 0, "interferer", tRec.interferer, TsTsSelRec.INTERFERER_SZ, nullInds(TsTsSelRec.INTERFERER))
                    Ssutil.DbGetInt(hStmt, 0, "caseno", tRec.caseno, nullInds(TsTsSelRec.CASENO))
                    Ssutil.DbGetInt(hStmt, 0, "subcaseno", tRec.subcaseno, nullInds(TsTsSelRec.SUBCASENO))
                    Ssutil.DbGetInt(hStmt, 0, "subcases", tRec.subcases, nullInds(TsTsSelRec.SUBCASES))
                    Ssutil.DbGetString(hStmt, 0, "intcall1", tRec.intcall1, TsTsSelRec.INTCALL1_SZ, nullInds(TsTsSelRec.INTCALL1))
                    Ssutil.DbGetString(hStmt, 0, "intcall2", tRec.intcall2, TsTsSelRec.INTCALL2_SZ, nullInds(TsTsSelRec.INTCALL2))
                    Ssutil.DbGetString(hStmt, 0, "intoper", tRec.intoper, TsTsSelRec.INTOPER_SZ, nullInds(TsTsSelRec.INTOPER))
                    Ssutil.DbGetString(hStmt, 0, "intoper2", tRec.intoper2, TsTsSelRec.INTOPER2_SZ, nullInds(TsTsSelRec.INTOPER2))
                    Ssutil.DbGetString(hStmt, 0, "intname1", tRec.intname1, TsTsSelRec.INTNAME1_SZ, nullInds(TsTsSelRec.INTNAME1))
                    Ssutil.DbGetString(hStmt, 0, "intname2", tRec.intname2, TsTsSelRec.INTNAME2_SZ, nullInds(TsTsSelRec.INTNAME2))
                    Ssutil.DbGetString(hStmt, 0, "viccall1", tRec.viccall1, TsTsSelRec.VICCALL1_SZ, nullInds(TsTsSelRec.VICCALL1))
                    Ssutil.DbGetString(hStmt, 0, "viccall2", tRec.viccall2, TsTsSelRec.VICCALL2_SZ, nullInds(TsTsSelRec.VICCALL2))
                    Ssutil.DbGetString(hStmt, 0, "vicoper", tRec.vicoper, TsTsSelRec.VICOPER_SZ, nullInds(TsTsSelRec.VICOPER))
                    Ssutil.DbGetString(hStmt, 0, "vicoper2", tRec.vicoper2, TsTsSelRec.VICOPER2_SZ, nullInds(TsTsSelRec.VICOPER2))
                    Ssutil.DbGetString(hStmt, 0, "vicname1", tRec.vicname1, TsTsSelRec.VICNAME1_SZ, nullInds(TsTsSelRec.VICNAME1))
                    Ssutil.DbGetString(hStmt, 0, "vicname2", tRec.vicname2, TsTsSelRec.VICNAME2_SZ, nullInds(TsTsSelRec.VICNAME2))
                    Ssutil.DbGetDouble(hStmt, 0, "int1int2dist", tRec.int1int2dist, nullInds(TsTsSelRec.INT1INT2DIST))
                    Ssutil.DbGetDouble(hStmt, 0, "vic1vic2dist", tRec.vic1vic2dist, nullInds(TsTsSelRec.VIC1VIC2DIST))
                    Ssutil.DbGetDouble(hStmt, 0, "int1vic1dist", tRec.int1vic1dist, nullInds(TsTsSelRec.INT1VIC1DIST))
                    Ssutil.DbGetDouble(hStmt, 0, "intoffax", tRec.intoffax, nullInds(TsTsSelRec.INTOFFAX))
                    Ssutil.DbGetDouble(hStmt, 0, "vicoffax", tRec.vicoffax, nullInds(TsTsSelRec.VICOFFAX))
                    Ssutil.DbGetDouble(hStmt, 0, "intgrnd", tRec.intgrnd, nullInds(TsTsSelRec.INTGRND))
                    Ssutil.DbGetDouble(hStmt, 0, "vicgrnd", tRec.vicgrnd, nullInds(TsTsSelRec.VICGRND))
                    Ssutil.DbGetString(hStmt, 0, "intaoffax", tRec.intaoffax, TsTsSelRec.INTAOFFAX_SZ, nullInds(TsTsSelRec.INTAOFFAX))
                    Ssutil.DbGetDouble(hStmt, 0, "inthopaz", tRec.inthopaz, nullInds(TsTsSelRec.INTHOPAZ))
                    Ssutil.DbGetDouble(hStmt, 0, "intantaz", tRec.intantaz, nullInds(TsTsSelRec.INTANTAZ))
                    Ssutil.DbGetDouble(hStmt, 0, "intoffantax", tRec.intoffantax, nullInds(TsTsSelRec.INTOFFANTAX))
                    Ssutil.DbGetString(hStmt, 0, "vicaoffax", tRec.vicaoffax, TsTsSelRec.VICAOFFAX_SZ, nullInds(TsTsSelRec.VICAOFFAX))
                    Ssutil.DbGetDouble(hStmt, 0, "vichopaz", tRec.vichopaz, nullInds(TsTsSelRec.VICHOPAZ))
                    Ssutil.DbGetDouble(hStmt, 0, "vicantaz", tRec.vicantaz, nullInds(TsTsSelRec.VICANTAZ))
                    Ssutil.DbGetDouble(hStmt, 0, "vicoffantax", tRec.vicoffantax, nullInds(TsTsSelRec.VICOFFANTAX))
                    Ssutil.DbGetDouble(hStmt, 0, "intvicaz", tRec.intvicaz, nullInds(TsTsSelRec.INTVICAZ))
                    Ssutil.DbGetDouble(hStmt, 0, "vicintaz", tRec.vicintaz, nullInds(TsTsSelRec.VICINTAZ))
                    Ssutil.DbGetString(hStmt, 0, "intbndcde", tRec.intbndcde, TsTsSelRec.INTBNDCDE_SZ, nullInds(TsTsSelRec.INTBNDCDE))
                    Ssutil.DbGetShort(hStmt, 0, "intanum", tRec.intanum, nullInds(TsTsSelRec.INTANUM))
                    Ssutil.DbGetString(hStmt, 0, "intacode", tRec.intacode, TsTsSelRec.INTACODE_SZ, nullInds(TsTsSelRec.INTACODE))
                    Ssutil.DbGetString(hStmt, 0, "intause", tRec.intause, TsTsSelRec.INTAUSE_SZ, nullInds(TsTsSelRec.INTAUSE))
                    Ssutil.DbGetFloat(hStmt, 0, "intgain", tRec.intgain, nullInds(TsTsSelRec.INTGAIN))
                    Ssutil.DbGetFloat(hStmt, 0, "vicgain", tRec.vicgain, nullInds(TsTsSelRec.VICGAIN))
                    Ssutil.DbGetString(hStmt, 0, "vicbndcde", tRec.vicbndcde, TsTsSelRec.VICBNDCDE_SZ, nullInds(TsTsSelRec.VICBNDCDE))
                    Ssutil.DbGetShort(hStmt, 0, "vicanum", tRec.vicanum, nullInds(TsTsSelRec.VICANUM))
                    Ssutil.DbGetString(hStmt, 0, "vicacode", tRec.vicacode, TsTsSelRec.VICACODE_SZ, nullInds(TsTsSelRec.VICACODE))
                    Ssutil.DbGetString(hStmt, 0, "vicause", tRec.vicause, TsTsSelRec.VICAUSE_SZ, nullInds(TsTsSelRec.VICAUSE))
                    Ssutil.DbGetDouble(hStmt, 0, "adiscctxh", tRec.adiscctxh, nullInds(TsTsSelRec.ADISCCTXH))
                    Ssutil.DbGetDouble(hStmt, 0, "adiscctxv", tRec.adiscctxv, nullInds(TsTsSelRec.ADISCCTXV))
                    Ssutil.DbGetDouble(hStmt, 0, "adisccrxh", tRec.adisccrxh, nullInds(TsTsSelRec.ADISCCRXH))
                    Ssutil.DbGetDouble(hStmt, 0, "adisccrxv", tRec.adisccrxv, nullInds(TsTsSelRec.ADISCCRXV))
                    Ssutil.DbGetDouble(hStmt, 0, "adiscxtxh", tRec.adiscxtxh, nullInds(TsTsSelRec.ADISCXTXH))
                    Ssutil.DbGetDouble(hStmt, 0, "adiscxtxv", tRec.adiscxtxv, nullInds(TsTsSelRec.ADISCXTXV))
                    Ssutil.DbGetDouble(hStmt, 0, "adiscxrxh", tRec.adiscxrxh, nullInds(TsTsSelRec.ADISCXRXH))
                    Ssutil.DbGetDouble(hStmt, 0, "adiscxrxv", tRec.adiscxrxv, nullInds(TsTsSelRec.ADISCXRXV))
                    Ssutil.DbGetDouble(hStmt, 0, "totantdisc", tRec.totantdisc, nullInds(TsTsSelRec.TOTANTDISC))
                    Ssutil.DbGetString(hStmt, 0, "intchid", tRec.intchid, TsTsSelRec.INTCHID_SZ, nullInds(TsTsSelRec.INTCHID))
                    Ssutil.DbGetDouble(hStmt, 0, "intfreqtxr", tRec.intfreqtxr, nullInds(TsTsSelRec.INTFREQTXR))
                    Ssutil.DbGetString(hStmt, 0, "intpolar", tRec.intpolar, TsTsSelRec.INTPOLAR_SZ, nullInds(TsTsSelRec.INTPOLAR))
                    Ssutil.DbGetString(hStmt, 0, "intstattx", tRec.intstattx, TsTsSelRec.INTSTATTX_SZ, nullInds(TsTsSelRec.INTSTATTX))
                    Ssutil.DbGetString(hStmt, 0, "vicchid", tRec.vicchid, TsTsSelRec.VICCHID_SZ, nullInds(TsTsSelRec.VICCHID))
                    Ssutil.DbGetDouble(hStmt, 0, "vicfreqrxr", tRec.vicfreqrxr, nullInds(TsTsSelRec.VICFREQRXR))
                    Ssutil.DbGetString(hStmt, 0, "vicpolar", tRec.vicpolar, TsTsSelRec.VICPOLAR_SZ, nullInds(TsTsSelRec.VICPOLAR))
                    Ssutil.DbGetString(hStmt, 0, "vicstatrx", tRec.vicstatrx, TsTsSelRec.VICSTATRX_SZ, nullInds(TsTsSelRec.VICSTATRX))
                    Ssutil.DbGetString(hStmt, 0, "inttraftx", tRec.inttraftx, TsTsSelRec.INTTRAFTX_SZ, nullInds(TsTsSelRec.INTTRAFTX))
                    Ssutil.DbGetString(hStmt, 0, "inteqpttx", tRec.inteqpttx, TsTsSelRec.INTEQPTTX_SZ, nullInds(TsTsSelRec.INTEQPTTX))
                    Ssutil.DbGetString(hStmt, 0, "victrafrx", tRec.victrafrx, TsTsSelRec.VICTRAFRX_SZ, nullInds(TsTsSelRec.VICTRAFRX))
                    Ssutil.DbGetString(hStmt, 0, "viceqptrx", tRec.viceqptrx, TsTsSelRec.VICEQPTRX_SZ, nullInds(TsTsSelRec.VICEQPTRX))
                    Ssutil.DbGetDouble(hStmt, 0, "vicpwrrx", tRec.vicpwrrx, nullInds(TsTsSelRec.VICPWRRX))
                    Ssutil.DbGetDouble(hStmt, 0, "intpwrtx", tRec.intpwrtx, nullInds(TsTsSelRec.INTPWRTX))
                    Ssutil.DbGetDouble(hStmt, 0, "intafsltx", tRec.intafsltx, nullInds(TsTsSelRec.INTAFSLTX))
                    Ssutil.DbGetDouble(hStmt, 0, "vicafslrx", tRec.vicafslrx, nullInds(TsTsSelRec.VICAFSLRX))
                    Ssutil.DbGetString(hStmt, 0, "ctxinttraftx", tRec.ctxinttraftx, TsTsSelRec.CTXINTTRAFTX_SZ, nullInds(TsTsSelRec.CTXINTTRAFTX))
                    Ssutil.DbGetString(hStmt, 0, "ctxvictrafrx", tRec.ctxvictrafrx, TsTsSelRec.CTXVICTRAFRX_SZ, nullInds(TsTsSelRec.CTXVICTRAFRX))
                    Ssutil.DbGetString(hStmt, 0, "ctxeqpt", tRec.ctxeqpt, TsTsSelRec.CTXEQPT_SZ, nullInds(TsTsSelRec.CTXEQPT))
                    Ssutil.DbGetDouble(hStmt, 0, "freqsepr", tRec.freqsepr, nullInds(TsTsSelRec.FREQSEPR))
                    Ssutil.DbGetDouble(hStmt, 0, "patloss", tRec.patloss, nullInds(TsTsSelRec.PATLOSS))
                    Ssutil.DbGetDouble(hStmt, 0, "distadv", tRec.distadv, nullInds(TsTsSelRec.DISTADV))
                    Ssutil.DbGetDouble(hStmt, 0, "eirpadv", tRec.eirpadv, nullInds(TsTsSelRec.EIRPADV))
                    Ssutil.DbGetDouble(hStmt, 0, "calcico", tRec.calcico, nullInds(TsTsSelRec.CALCICO))
                    Ssutil.DbGetDouble(hStmt, 0, "calcixp", tRec.calcixp, nullInds(TsTsSelRec.CALCIXP))
                    Ssutil.DbGetDouble(hStmt, 0, "reqdcalc", tRec.reqdcalc, nullInds(TsTsSelRec.REQDCALC))
                    Ssutil.DbGetDouble(hStmt, 0, "resti", tRec.resti, nullInds(TsTsSelRec.RESTI))
                    Ssutil.DbGetString(hStmt, 0, "calctype", tRec.calctype, TsTsSelRec.CALCTYPE_SZ, nullInds(TsTsSelRec.CALCTYPE))
                    Ssutil.DbGetShort(hStmt, 0, "rxant", tRec.rxant, nullInds(TsTsSelRec.RXANT))
                    Ssutil.DbGetShort(hStmt, 0, "txant", tRec.txant, nullInds(TsTsSelRec.TXANT))
                    Ssutil.DbGetString(hStmt, 0, "intaxref", tRec.intaxref, TsTsSelRec.INTAXREF_SZ, nullInds(TsTsSelRec.INTAXREF))
                    Ssutil.DbGetString(hStmt, 0, "intamodel", tRec.intamodel, TsTsSelRec.INTAMODEL_SZ, nullInds(TsTsSelRec.INTAMODEL))
                    Ssutil.DbGetString(hStmt, 0, "vicaxref", tRec.vicaxref, TsTsSelRec.VICAXREF_SZ, nullInds(TsTsSelRec.VICAXREF))
                    Ssutil.DbGetString(hStmt, 0, "vicamodel", tRec.vicamodel, TsTsSelRec.VICAMODEL_SZ, nullInds(TsTsSelRec.VICAMODEL))
                    Ssutil.DbGetDouble(hStmt, 0, "intelev", tRec.intelev, nullInds(TsTsSelRec.INTELEV))
                    Ssutil.DbGetDouble(hStmt, 0, "vicelev", tRec.vicelev, nullInds(TsTsSelRec.VICELEV))
                    Ssutil.DbGetDouble(hStmt, 0, "intvicel", tRec.intvicel, nullInds(TsTsSelRec.INTVICEL))
                    Ssutil.DbGetDouble(hStmt, 0, "vicintel", tRec.vicintel, nullInds(TsTsSelRec.VICINTEL))
                    Ssutil.DbGetFloat(hStmt, 0, "intaht", tRec.intaht, nullInds(TsTsSelRec.INTAHT))
                    Ssutil.DbGetFloat(hStmt, 0, "vicaht", tRec.vicaht, nullInds(TsTsSelRec.VICAHT))
                    Ssutil.DbGetDouble(hStmt, 0, "tiltdisc", tRec.tiltdisc, nullInds(TsTsSelRec.TILTDISC))
                    Ssutil.DbGetDouble(hStmt, 0, "pathloss80", tRec.pathloss80, nullInds(TsTsSelRec.PATHLOSS80))
                    Ssutil.DbGetDouble(hStmt, 0, "calcico80", tRec.calcico80, nullInds(TsTsSelRec.CALCICO80))
                    Ssutil.DbGetDouble(hStmt, 0, "calcixp80", tRec.calcixp80, nullInds(TsTsSelRec.CALCIXP80))
                    Ssutil.DbGetDouble(hStmt, 0, "reqd80", tRec.reqd80, nullInds(TsTsSelRec.REQD80))
                    Ssutil.DbGetDouble(hStmt, 0, "resti80", tRec.resti80, nullInds(TsTsSelRec.RESTI80))
                    Ssutil.DbGetDouble(hStmt, 0, "pathloss99", tRec.pathloss99, nullInds(TsTsSelRec.PATHLOSS99))
                    Ssutil.DbGetDouble(hStmt, 0, "calcico99", tRec.calcico99, nullInds(TsTsSelRec.CALCICO99))
                    Ssutil.DbGetDouble(hStmt, 0, "calcixp99", tRec.calcixp99, nullInds(TsTsSelRec.CALCIXP99))
                    Ssutil.DbGetDouble(hStmt, 0, "reqd99", tRec.reqd99, nullInds(TsTsSelRec.REQD99))
                    Ssutil.DbGetDouble(hStmt, 0, "resti99", tRec.resti99, nullInds(TsTsSelRec.RESTI99))
                    Ssutil.DbGetInt(hStmt, 0, "ohresult", tRec.ohresult, nullInds(TsTsSelRec.OHRESULT))
                    Ssutil.DbGetString(hStmt, 0, "ctxinteqpt", tRec.ctxinteqpt, TsTsSelRec.CTXINTEQPT_SZ, nullInds(TsTsSelRec.CTXINTEQPT))
                    Ssutil.DbGetString(hStmt, 0, "inteqtype", tRec.inteqtype, TsTsSelRec.INTEQTYPE_SZ, nullInds(TsTsSelRec.INTEQTYPE))
                    Ssutil.DbGetString(hStmt, 0, "viceqtype", tRec.viceqtype, TsTsSelRec.VICEQTYPE_SZ, nullInds(TsTsSelRec.VICEQTYPE))
                    Ssutil.DbGetDouble(hStmt, 0, "intbwchans", tRec.intbwchans, nullInds(TsTsSelRec.INTBWCHANS))
                    Ssutil.DbGetDouble(hStmt, 0, "vicbwchans", tRec.vicbwchans, nullInds(TsTsSelRec.VICBWCHANS))
                Catch e As Exception
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "Tstsrp3.TsTsRp3(): ERROR: ODBC 'Get' attempt failed: " & e.Message)
                    Ssutil.DbGetDiagStmt(hStmt, "tstsrp303: Failed retrieving " & e.Message)
                    pl.Output()
                    pl.LeftAt(0, GenUtil.GetUserMess())
                    pl.Output()
                    Exit While
                End Try

                '...Log2.v("\nTstsrp3.TsTsRp3(): tRec = \n" + tRec.ToStringWN(nullInds));

                TpRunTsip.Tstsrp3.ReportTtDetail(ttName, tRec, tpParm, ohtrigger, pl)
            End While

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
            sqlRet = CShort(Ssutil.DisConn(hConn))

            '...Log2.v("\nTstsrp3.TsTsRp3(): Exit");
            Return 0
        End Function

        ''' <summary>
        ''' This method produces the CASEDET text fragment one set of site, ante and channel data.   
        ''' </summary>
        ''' <paramname="ttName"> - the unique ID substring of the Ts DB table names.</param>
        ''' <paramname="selRec"> - TsTsSelRec object providing results data.</param>
        ''' <paramname="tpParm"> - the TpParm object providing parameter data.</param>
        ''' <paramname="ohtrigger"> - flag = 1 for Line of sight, and Over-Horizon Loss cases only; flag = 0 for all cases.</param>
        ''' <paramname="pl"> - PrintLine object with header override.</param>
        Private Shared Sub ReportTtDetail(ttName As String, selRec As TsTsSelRec, tpParm As TpParm, ohtrigger As Integer, pl As TpRunTsip.PrintLine)
            '...Log2.v("\nTstsrp3.ReportTtDetail(): Entry");

            If Not TpRunTsip.Tstsrp3.IsDupSite(selRec) Then
                TpRunTsip.Tstsrp3.ReportTtSiteHeader(ttName, selRec, tpParm, ohtrigger, pl)
            End If

            If Not TpRunTsip.Tstsrp3.IsDupAnte(selRec) Then
                TpRunTsip.Tstsrp3.ReportTtAnteHeader(selRec, tpParm, ohtrigger, pl)
            End If

            TpRunTsip.Tstsrp3.ReportttChanDetail(selRec, tpParm, ohtrigger, pl)

            '...Log2.v("\nTstsrp3.ReportTtDetail(): Exit");
        End Sub

        ''' <summary>
        ''' This method detects whether the current case is a duplicate of a previous interference case.   
        ''' </summary>
        ''' <paramname="selRec"> - TsTsSelRec object providing results data.</param>
        ''' <returns></returns>
        Private Shared Function IsDupSite(selRec As TsTsSelRec) As Boolean
            Dim bRet As Boolean

            If selRec.caseno = TpRunTsip.Tstsrp3.lastCase Then
                bRet = True
            Else
                bRet = False
                TpRunTsip.Tstsrp3.lastCase = selRec.caseno
                TpRunTsip.Tstsrp3.lastSubCase = -1
            End If

            Return bRet
        End Function

        ''' <summary>
        ''' This method determines whether this antenna subcase is the same as the previous 
        ''' subcase; if so, use that subcase instead. 
        ''' </summary>
        ''' <paramname="selRec">- TsTsSelRec object providing results data.</param>
        ''' <returns></returns>
        Private Shared Function IsDupAnte(selRec As TsTsSelRec) As Boolean
            Dim bRet As Boolean

            If selRec.subcaseno = TpRunTsip.Tstsrp3.lastSubCase Then
                bRet = True
            Else
                TpRunTsip.Tstsrp3.lastSubCase = selRec.subcaseno
                bRet = False
            End If

            Return bRet
        End Function

        ''' <summary>
        ''' This method produces the text used for the Ts-Ts case Site header.    
        ''' </summary>
        ''' <paramname="ttName"> - the unique ID substring of the Ts DB table names.</param>
        ''' <paramname="selRec"> - TsTsSelRec object providing results data.</param>
        ''' <paramname="tpParm"> - the TpParm object providing parameter data.</param>
        ''' <paramname="ohtrigger"> - flag = 1 for Line of sight, and Over-Horizon Loss cases only; flag = 0 for all cases.</param>
        ''' <paramname="pl"> - PrintLine object with header override.</param>
        Private Shared Sub ReportTtSiteHeader(ttName As String, selRec As TsTsSelRec, tpParm As TpParm, ohtrigger As Integer, pl As TpRunTsip.PrintLine)
            Dim [cDate] As String
            Dim cTime As String
            Dim cBuff As String

            pl.PageBefore()

            pl.LeftAt(0, "Frequency Coordination System Association")
            cBuff = SQLCHARPTR.Format("Page: {0:D}", pl.PageNumber)
            pl.RightAt(pl.LastPos(), cBuff)
            pl.Output()

            pl.LeftAt(0, "MASTER DATA BASE -- TSTS Interference Study Report - CASE DETAIL")
            GenUtil.UtGetDateTime([cDate], cTime)
            cBuff = SQLCHARPTR.Format("Date: {0,10}", [cDate])
            pl.RightAt(pl.LastPos(), cBuff)
            pl.Output()
            pl.LeftAt(0, "Project Code [{0}]", Info.ProjectCode)
            pl.Output()

            If ohtrigger = 0 Then
                pl.LeftAt(0, "All Cases")
            Else
                pl.LeftAt(0, "Line of sight, and over-horizon loss cases only")
            End If
            cBuff = SQLCHARPTR.Format("Time: {0,10}", cTime)
            pl.RightAt(pl.LastPos(), cBuff)
            pl.Output()
            pl.Output()

            pl.LeftAt(0, "Case Number : ")
            pl.IntAt(-1, "{0,6:D}", selRec.caseno)
            If selRec.interferer.Equals("P") Then
                pl.LeftAt(24, "Interferor from Proposed File")
            Else
                pl.LeftAt(24, "Interferor from Environment File")
            End If

            pl.LeftAt(68, "Environment  :")
            pl.LeftAt(84, tpParm.envtype)

            pl.LeftAt(100, "Coord. Dist.: ")
            pl.DoubleAt(114, "{0,6:F2}", tpParm.coordist)
            pl.LeftAt(-1, " km")

            pl.Output()

            pl.LeftAt(0, "Propagation Loss Model:")
            Select Case tpParm.spherecalc(0)
                Case "1"c
                    pl.LeftAt(24, "TSIP CCIR-SJM")

                Case "2"c
                    pl.LeftAt(24, "Spherical Earth")

                Case "3"c
                    pl.LeftAt(24, "Free Space")

                Case "4"c
                    pl.LeftAt(24, "PCS-HATA")

                Case "5"c
                    pl.LeftAt(24, "OH-LOSS")
                Case Else
                    pl.LeftAt(24, "Don't Know")
            End Select

            pl.LeftAt(68, "Analysis Type: ")
            pl.LeftAt(84, tpParm.analopt)
            pl.LeftAt(100, "PDF_Run     : ")
            pl.LeftAt(114, ttName)
            pl.Output()

            pl.LeftAt(0, "Maximum Frequency Separation : ")
            pl.DoubleAt(-1, "{0,6:F1}", tpParm.fsep)
            pl.LeftAt(68, "Margin       : ")
            pl.DoubleAt(84, "{0,5:F1}", tpParm.margin)
            pl.Output()

            pl.LeftAt(0, "+----")
            pl.LeftAt(41, "Site Geometry")
            pl.LeftAt(68, "|  Grnd")
            pl.LeftAt(78, "+-------- Site to Site --------+")
            pl.Output()

            pl.LeftAt(0, "Call I")
            pl.LeftAt(9, "Call Irx")
            pl.LeftAt(18, "|")
            pl.LeftAt(20, "Station  I --> Station  Irx")
            pl.LeftAt(52, "|")
            pl.LeftAt(54, "OP I ")
            pl.LeftAt(61, "OP Irx")
            pl.LeftAt(68, "| Int.")
            pl.LeftAt(78, "|")
            pl.LeftAt(80, "I-Irx Km")
            pl.LeftAt(89, "I-Vrx Km")
            pl.LeftAt(99, "I  Ang.DEG *")
            pl.LeftAt(112, "Angle")
            pl.Output()

            pl.LeftAt(0, "CALL Vrx")
            pl.LeftAt(9, "Call V")
            pl.LeftAt(18, "|")
            pl.LeftAt(20, "Station  Vrx --> Station  V")
            pl.LeftAt(52, "|")
            pl.LeftAt(54, "OP Vrx")
            pl.LeftAt(61, "OP V")
            pl.LeftAt(68, "| Victim")
            pl.LeftAt(78, "|")
            pl.LeftAt(80, "Vrx-V Km")
            pl.LeftAt(98, "Vrx Ang.DEG *")
            pl.LeftAt(112, "Ref.")
            pl.Output()

            pl.LeftAt(0, "+--------")
            pl.LeftAt(9, "---------")
            pl.LeftAt(19, "----------------")
            pl.LeftAt(36, "----------------")
            pl.LeftAt(54, "------")
            pl.LeftAt(61, "------")
            pl.LeftAt(68, "----------")
            pl.LeftAt(80, "----.---")
            pl.LeftAt(89, "----.---")
            pl.LeftAt(98, "-------.--- +")
            pl.LeftAt(111, "---------+")
            pl.Output()

            pl.LeftAt(0, selRec.intcall1)
            pl.LeftAt(19, selRec.intname1)
            pl.LeftAt(54, selRec.intoper)
            pl.DoubleAt(68, "{0,7:F0}", selRec.intgrnd)
            pl.LeftAt(-1, "m")
            pl.DoubleAt(81, "{0,6:F2}", selRec.int1int2dist)
            pl.DoubleAt(90, "{0,6:F2}", selRec.int1vic1dist)
            pl.DoubleAt(101, "{0,6:F1}", selRec.intoffax)
            If selRec.intaoffax.Equals("Y") Then
                pl.LeftAt(111, "Offaxis")
            Else
                pl.LeftAt(111, "Main Beam")
            End If
            pl.Output()

            pl.LeftAt(9, selRec.intcall2)
            pl.LeftAt(19, "->")
            pl.LeftAt(22, selRec.intname2)
            pl.LeftAt(61, selRec.intoper2)
            pl.Output()

            pl.LeftAt(0, selRec.viccall1)
            pl.LeftAt(19, selRec.vicname1)
            pl.LeftAt(54, selRec.vicoper)
            pl.DoubleAt(68, "{0,7:F0}", selRec.vicgrnd)
            pl.LeftAt(-1, "m")
            pl.DoubleAt(81, "{0,6:F2}", selRec.vic1vic2dist)
            pl.DoubleAt(101, "{0,6:F1}", selRec.vicoffax)
            If selRec.vicaoffax.Equals("Y") Then
                pl.LeftAt(111, "Offaxis")
            Else
                pl.LeftAt(111, "Main Beam")
            End If
            pl.Output()

            pl.LeftAt(9, selRec.viccall2)
            pl.LeftAt(19, "->")
            pl.LeftAt(22, selRec.vicname2)
            pl.LeftAt(61, selRec.vicoper2)
            pl.Output()

            Return
        End Sub

        ''' <summary>
        ''' This method produces the text used for the Ts-Ts case Site header.   
        ''' </summary>
        ''' <paramname="selRec"> - TsTsSelRec object providing results data.</param>
        ''' <paramname="tpParm"> - the TpParm object providing parameter data.</param>
        ''' <paramname="ohtrigger"> - flag = 1 for Line of sight, and Over-Horizon Loss cases only; flag = 0 for all cases.</param>
        ''' <paramname="pl"> - PrintLine object with header override.</param>
        Private Shared Sub ReportTtAnteHeader(selRec As TsTsSelRec, tpParm As TpParm, ohtrigger As Integer, pl As TpRunTsip.PrintLine)
            Dim cCI As String
            Dim IsCalc As Boolean

            pl.Output()
            pl.LeftAt(0, "Antenna Geometry: Interferer: Boresight   Elev:")
            pl.DoubleAt(-1, "{0,7:F2}", selRec.intelev)
            pl.LeftAt(-1, " Az:")
            pl.DoubleAt(-1, "{0,6:F1}", selRec.intantaz)
            pl.LeftAt(-1, "|   Victim: Boresight    Elev:")
            pl.DoubleAt(-1, "{0,6:F2}", selRec.vicelev)
            pl.LeftAt(-1, " Az:")
            pl.DoubleAt(-1, "{0,6:F1}", selRec.vicantaz)
            pl.Output()

            pl.LeftAt(0, "                  To Victim: Ant.Ht:")
            pl.DoubleAt(-1, "{0,4:F0}", selRec.intaht)
            pl.LeftAt(-1, "m Elev:")
            pl.DoubleAt(-1, "{0,7:F2}", selRec.intvicel)
            pl.LeftAt(-1, " Az:")
            pl.DoubleAt(-1, "{0,6:F1}", selRec.intvicaz)
            pl.LeftAt(-1, "|   To Int: Ant.Ht:")
            pl.DoubleAt(-1, "{0,4:F0}", selRec.vicaht)
            pl.LeftAt(-1, "m Elev:")
            pl.DoubleAt(-1, "{0,6:F2}", selRec.vicintel)
            pl.LeftAt(-1, " Az:")
            pl.DoubleAt(-1, "{0,6:F1}", selRec.vicintaz)
            pl.Output()

            pl.LeftAt(0, "                  Off-axis Angle:")
            pl.DoubleAt(-1, "{0,6:F1}", selRec.intoffantax)
            pl.LeftAt(-1, " deg.                    |   Off-Axis Angle:")
            pl.DoubleAt(-1, "{0,6:F1}", selRec.vicoffantax)
            pl.Output()

            pl.LeftAt(0, "Antenna #: ")
            pl.IntAt(-1, "{0,4:D}", selRec.intanum)
            pl.LeftAt(24, "AUse: ")
            pl.LeftAt(-1, selRec.intause)
            pl.DoubleAt(36, "Gain:{0,6:F2}", selRec.intgain)
            pl.LeftAt(48, "dB")
            pl.LeftAt(64, "|")
            pl.IntAt(68, "Antenna #: {0,4:D}", selRec.vicanum)
            pl.LeftAt(92, "AUse: ")
            pl.LeftAt(-1, selRec.vicause)
            pl.LeftAt(105, "Gain:")
            pl.DoubleAt(-1, "{0,6:F2}", selRec.vicgain)
            pl.LeftAt(117, "dB")
            pl.Output()

            pl.LeftAt(0, "TX.Ant.Code: ")
            pl.LeftAt(-1, selRec.intacode)
            pl.LeftAt(27, "Discr. H CPol:")
            pl.DoubleAt(-1, "{0,6:F2} ", selRec.adiscctxh)
            pl.LeftAt(-1, "dB ___")
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "RX.Ant.Code: ")
            pl.LeftAt(-1, selRec.vicacode)
            pl.LeftAt(96, "Discr. H CPol:")
            pl.DoubleAt(-1, "{0,6:F2} ", selRec.adisccrxh)
            pl.LeftAt(-1, "dB ___")
            pl.Output()

            pl.LeftAt(0, "X Ref. Ant : ")
            If selRec.intaxref.Equals("") Then
                pl.LeftAt(-1, selRec.intacode)
            Else
                pl.LeftAt(-1, selRec.intaxref)
            End If
            pl.LeftAt(27, "Discr. V CPol:")
            pl.DoubleAt(-1, "{0,6:F2} dB ___", selRec.adiscctxv)
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "X Ref. Ant : ")
            If selRec.vicaxref.Equals("") Then
                pl.LeftAt(-1, selRec.vicacode)
            Else
                pl.LeftAt(-1, selRec.vicaxref)
            End If
            pl.LeftAt(96, "Discr. V CPol:")
            pl.DoubleAt(-1, "{0,6:F2} dB ___", selRec.adisccrxv)
            pl.Output()

            pl.LeftAt(0, "Model: ")
            pl.LeftAt(-1, selRec.intamodel)
            pl.LeftAt(27, "Discr. H XPol:")
            pl.DoubleAt(-1, "{0,6:F2} dB ___", selRec.adiscxtxh)
            pl.LeftAt(64, "|")
            pl.LeftAt(68, "Model: ")
            pl.LeftAt(-1, selRec.vicamodel)
            pl.LeftAt(96, "Discr. H XPol:")
            pl.DoubleAt(-1, "{0,6:F2} dB ___", selRec.adiscxrxh)
            pl.Output()

            pl.LeftAt(0, "Offaxis Angle:")
            pl.DoubleAt(-1, "{0,6:F1} deg.", selRec.intoffantax)
            pl.LeftAt(27, "Discr. V XPol:")
            pl.DoubleAt(-1, "{0,6:F2} dB ___", selRec.adiscxtxv)
            pl.LeftAt(64, "+")
            pl.DoubleAt(68, "Offaxis Angle:{0,6:F1} deg.", selRec.vicoffantax)
            pl.DoubleAt(96, "Discr. V XPol:{0,6:F2} dB ___", selRec.adiscxrxv)
            pl.Output()

            If selRec.intaoffax.Equals("Y") Then
                pl.DoubleAt(0, "True Azimuth of Antenna: {0,5:F1}", selRec.intantaz)
                'AH: EXTANT BUG - should be %5.1f ?
                pl.DoubleAt(-1, " deg. Azimuth of Hop: {0,5:F1} deg.", selRec.inthopaz)
            End If

            If selRec.vicaoffax.Equals("Y") Then
                pl.DoubleAt(68, "True Azimuth of Antenna: {0,5:F1}", selRec.vicantaz)
                pl.DoubleAt(-1, " deg. Azimuth of Hop: {0,5:F1} deg.", selRec.vichopaz)
            End If

            If selRec.intaoffax.Equals("Y") OrElse selRec.vicaoffax.Equals("Y") Then
                pl.Output()
            End If

            pl.Output()

            pl.LeftAt(0, "-- TX Interferer Sys.Eqp.CTX.Band Info  - Station I --- TS --- ")
            pl.LeftAt(63, "*+*")
            pl.LeftAt(68, "-- RX Victim Sys.Eqp.CTX.Band Info ----- Station Vrx --- TS --")
            pl.Output()

            pl.LeftAt(0, "Eqpt. Planned / Traffic: ")
            pl.LeftAt(-1, selRec.inteqpttx)
            pl.LeftAt(-1, "  ")
            pl.LeftAt(-1, selRec.inttraftx)

            ' 	Put in the calculation information if needed.  The following idiom on
            ' 		calctype is used to indicate that calculations (C or I) were used rather
            ' 		than CTX curves (C/I or -I). 
            cCI = selRec.calctype.Trim()
            IsCalc = cCI.Length < 2

            If IsCalc Then
                ' 	This is a calculated case 
                If selRec.inteqtype.Equals("D") Then
                    pl.LeftAt(50, "Digital")
                Else
                    pl.LeftAt(50, "Analog")
                End If
            End If

            pl.LeftAt(64, "|")
            pl.LeftAt(68, "Eqpt. Planned / Traffic: ")
            pl.LeftAt(-1, selRec.viceqptrx)
            pl.LeftAt(-1, "  ")
            pl.LeftAt(-1, selRec.victrafrx)

            If IsCalc Then
                ' 	This is a calculated case 
                If selRec.viceqtype.Equals("D") Then
                    pl.LeftAt(120, "Digital")
                Else
                    pl.LeftAt(120, "Analog")
                End If
            End If

            pl.Output()

            pl.LeftAt(0, "Eqpt. X Ref.  / Traffic: ")
            If IsCalc Then
                pl.LeftAt(-1, selRec.ctxinteqpt)
            Else
                pl.LeftAt(-1, "-")
            End If

            pl.LeftAt(35, selRec.ctxinttraftx)

            If IsCalc Then
                If selRec.intbwchans > 0 Then
                    pl.DoubleAt(43, "*B/W Used: {0,6:F2}", selRec.intbwchans)
                End If
            End If

            pl.LeftAt(64, "|")
            pl.LeftAt(68, "Eqpt. X Ref.  / Traffic: ")
            pl.LeftAt(-1, selRec.ctxeqpt)
            pl.LeftAt(-1, "  ")
            pl.LeftAt(-1, selRec.ctxvictrafrx)

            If IsCalc Then
                If selRec.vicbwchans > 0 Then
                    If selRec.viceqtype.Equals("D") Then
                        pl.DoubleAt(111, "*B/W Used: {0,6:F2}", selRec.vicbwchans)
                    Else
                        pl.DoubleAt(111, "*Channels: {0,6:F2}", selRec.vicbwchans)
                    End If
                End If
            End If

            pl.Output()

            pl.LeftAt(11, "TX.Freq.Bnd : ")
            pl.LeftAt(-1, selRec.intbndcde)
            pl.LeftAt(64, "+")
            pl.LeftAt(79, "RX.Freq.Bnd : ")
            pl.LeftAt(-1, selRec.vicbndcde)
            pl.Output()

            ' Case information 
            pl.LeftAt(0, "SC")
            pl.Output()

            pl.LeftAt(0, "UA")
            pl.LeftAt(5, "TX.Freq.")
            pl.LeftAt(17, "P")
            pl.LeftAt(20, "RX.Freq.")
            pl.LeftAt(32, "P")
            pl.LeftAt(34, "Chn")
            pl.LeftAt(39, "Freq.")
            pl.LeftAt(46, "Int.")
            pl.LeftAt(52, "Int.")
            pl.LeftAt(57, "Vic.")
            pl.LeftAt(62, "Vic.")
            If IsCalc Then
                pl.LeftAt(116, " CALC")
            Else
                pl.LeftAt(116, "TABLE")
            End If
            pl.Output()

            pl.LeftAt(0, "BS")
            pl.LeftAt(6, "(MHz)")
            pl.LeftAt(17, "O")
            pl.LeftAt(21, "(MHz)")
            pl.LeftAt(32, "O")
            pl.LeftAt(34, "Sta")
            pl.LeftAt(39, "Sep.")
            pl.LeftAt(46, "TXPwr")
            pl.LeftAt(52, "AFSL")
            pl.LeftAt(57, "AFSL")
            pl.LeftAt(62, "RXPwr")
            pl.LeftAt(70, "Tilt")
            pl.LeftAt(76, "TANT")
            pl.LeftAt(82, "T")
            pl.LeftAt(85, "CALC PATH")
            pl.LeftAt(97, "CALC  C/I  OR  -I")
            pl.LeftAt(117, "REQD.")
            pl.LeftAt(124, "MARGIN")
            pl.Output()

            pl.LeftAt(0, " E")
            pl.LeftAt(6, "Stn.I")
            pl.LeftAt(17, "L")
            pl.LeftAt(20, "Stn.Vrx")
            pl.LeftAt(32, "L")
            pl.LeftAt(34, "T.R")
            pl.LeftAt(39, "(MHz)")
            pl.LeftAt(46, "(dBm)")
            pl.LeftAt(52, "(dB)")
            pl.LeftAt(57, "(dB)")
            pl.LeftAt(62, "(dBm)")
            pl.LeftAt(70, "DISC")
            pl.LeftAt(76, "DISC")
            pl.LeftAt(82, "X")
            pl.LeftAt(88, "LOSS")
            pl.LeftAt(100, "CPol     XPol")
            pl.LeftAt(118, "(dB)")
            pl.LeftAt(126, "(dB)")
            pl.Output()

            pl.LeftAt(0, "--- ------.----- - ------.----- - -.- ----.--- --.-- --.- --.- ---.-- --.-- --.--")
            pl.LeftAt(82, "- ------ ---.- -----.-- -----.--  ----.-  ----.-")
            pl.Output()

        End Sub

        ''' <summary>
        ''' This method produces the text used for the Ts-Ts case Chan header.   
        ''' </summary>
        ''' <paramname="selRec"> - TsTsSelRec object providing results data.</param>
        ''' <paramname="tpParm"> - the TpParm object providing parameter data.</param>
        ''' <paramname="ohtrigger"> - flag = 1 for Line of sight, and Over-Horizon Loss cases only; flag = 0 for all cases.</param>
        ''' <paramname="pl"> - PrintLine object with header override.</param>
        Private Shared Sub ReportttChanDetail(selRec As TsTsSelRec, tpParm As TpParm, ohtrigger As Integer, pl As TpRunTsip.PrintLine)
            '...Log2.v("\nTstsrp3.ReportttChanDetail(): Entry");

            Dim coleft As String
            Dim coright As String
            Dim xpleft As String
            Dim xpright As String

            Dim eMapUsed = Enums.MapUsed.e250K

            pl.IntAt(0, "{0,3:D}", selRec.subcaseno)
            pl.RightAt(14, GenUtil.Ccommas(selRec.intfreqtxr, 4))
            pl.LeftAt(17, selRec.intpolar)
            pl.RightAt(29, GenUtil.Ccommas(selRec.vicfreqrxr, 4))
            pl.LeftAt(32, selRec.vicpolar)
            pl.LeftAt(34, selRec.intstattx)
            pl.LeftAt(36, selRec.vicstatrx)
            pl.DoubleAt(37, "{0,9:F3}", selRec.freqsepr)
            pl.DoubleAt(46, "{0,6:F2}", selRec.intpwrtx)
            pl.DoubleAt(53, "{0,4:F1}", selRec.intafsltx)
            pl.DoubleAt(58, "{0,4:F1}", selRec.vicafslrx)
            pl.DoubleAt(63, "{0,6:F2}", selRec.vicpwrrx)
            pl.DoubleAt(69, "{0,6:F2}", selRec.tiltdisc)
            pl.DoubleAt(75, "{0,6:F2}", selRec.totantdisc)
            If selRec.intpolar.Equals("B") AndAlso selRec.vicpolar.Equals("B") Then
                ' 
                ' 	In the special case of B into B we output the interference in a special format.  We want
                ' 	to show both the H and V from the interferer separately.  This means there will be two lines
                ' 	for each normal line, an H line and a V line.
                ' 
                pl.LeftAt(82, "H") '	First the horizontal polarization for FSL.

                '	Calculate, the base signal, the signal with FSL, and then the H and V copolar and crosspolar discriminations.
                Dim dBase As Double = selRec.intpwrtx - selRec.intafsltx + selRec.intgain + selRec.vicgain - selRec.vicafslrx

                '	The first line, first column is tx H copolar + rx H copolar
                Dim dHCoPol As Double = selRec.adiscctxh + selRec.adisccrxh
                '	second column is tx H crosspolar + rx V copolar
                Dim dHXPol As Double = selRec.adiscxtxh + selRec.adisccrxv
                '	The second line, first column is tx V copolar + rx V copolar
                Dim dVCoPol As Double = selRec.adiscctxv + selRec.adisccrxv
                '	second column is tx V crosspolar + rx H copolar
                Dim dVXPol As Double = selRec.adiscxtxv + selRec.adisccrxh
                '	The basis minus the path loss.
                Dim dBaseWPath As Double = dBase - selRec.patloss
                Dim IsCoverI = False

                '	Now we have the basic interference level without the antenna discrimination.
                '	This is all we need for the an interference calculation (I or -I), but if the
                '	CTX requirement is for a C/I or C calculation, we need to calculate the C/I ratio.
                '	The antenna discriminations and path loss are part of the interference.  They 
                '	will be subtracted in the interference cases, but added for the C/I case (since
                '	they are subtracted from the interference in the denominator).
                If Strings.FirstCharIs(selRec.calctype, "C"c) Then
                    '	Calculate the C/I ratios
                    dBaseWPath = selRec.vicpwrrx - dBase + selRec.patloss
                    IsCoverI = True
                End If

                Dim dMargin As Double
                Dim dSelect As Double

                If dHCoPol < dHXPol Then
                    TpRunTsip.Tstsrp3.ParenthesizeLowerNumber(coleft, coright, xpleft, xpright)
                    dSelect = dHCoPol  '	select this for the margin.
                Else
                    TpRunTsip.Tstsrp3.ParenthesizeUpperNumber(coleft, coright, xpleft, xpright)
                    dSelect = dHXPol
                End If

                ' This is new format for both OHloss and other calculation -- task 1069 
                Select Case tpParm.spherecalc(0)
                    Case "1"c
                        pl.LeftAt(84, "CCIR")

                    Case "2"c
                        pl.LeftAt(84, "SEL")

                    Case "3"c
                        pl.LeftAt(84, "FSL")

                    Case "4"c
                        pl.LeftAt(84, "HATA")

                    Case "5"c
                        pl.LeftAt(84, "FSL")
                    Case Else
                End Select

                pl.DoubleAt(91, "{0,5:F1}", selRec.patloss)

                pl.LeftAt(97, coleft)
                pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + If(IsCoverI, dHCoPol, -dHCoPol)) ' if -I subtract antenna disc, if C/I, add
                pl.LeftAt(-1, coright)
                pl.LeftAt(106, xpleft)
                pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + If(IsCoverI, dHXPol, -dHXPol))  ' selRec.calcixp);
                pl.LeftAt(-1, xpright)

                pl.DoubleAt(116, "{0,6:F1}", selRec.reqdcalc)
                If IsCoverI Then
                    dMargin = dBaseWPath + dSelect - selRec.reqdcalc '	Get the full margin.
                Else
                    dMargin = selRec.reqdcalc - (dBaseWPath - dSelect) '	Get the full margin.
                End If
                pl.DoubleAt(124, "{0,6:F1}", dMargin) 'selRec.resti);

                pl.Output()

                '	Next line has the V values.

                If dVCoPol < dVXPol Then
                    TpRunTsip.Tstsrp3.ParenthesizeLowerNumber(coleft, coright, xpleft, xpright)
                    dSelect = dVCoPol
                Else
                    TpRunTsip.Tstsrp3.ParenthesizeUpperNumber(coleft, coright, xpleft, xpright)
                    dSelect = dVXPol
                End If

                pl.DoubleAt(75, "{0,6:F2}", dSelect)  ' V total antenna discrimination.

                pl.LeftAt(82, "V") '	Now the Vertical polarization for FSL.

                pl.LeftAt(97, coleft)
                pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + If(IsCoverI, dVCoPol, -dVCoPol)) ' selRec.calcico);
                pl.LeftAt(-1, coright)
                pl.LeftAt(106, xpleft)
                pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + If(IsCoverI, dVXPol, -dVXPol))  ' selRec.calcixp);
                pl.LeftAt(-1, xpright)

                pl.DoubleAt(116, "{0,6:F1}", selRec.reqdcalc)
                If IsCoverI Then
                    dMargin = dBaseWPath + dSelect - selRec.reqdcalc '	Get the full margin.
                Else
                    dMargin = selRec.reqdcalc - (dBaseWPath - dSelect) '	Get the full margin.
                End If
                pl.DoubleAt(124, "{0,6:F1}", dMargin)  ' selRec.resti);

                pl.Output()


                If tpParm.spherecalc.Equals("5") Then
                    '	If this is for ohloss, then we have four more lines to print.
                    ' We ignore the following if there was an error, or it was free space
                    '    loss (i.e. no obstructions found. 
                    If selRec.ohresult >= 1000 Then
                        eMapUsed = Enums.MapUsed.e50K
                        selRec.ohresult -= 1000
                    End If
                    If selRec.ohresult > 0 AndAlso selRec.ohresult < 100 Then
                        Select Case selRec.ohresult
                            Case 1
                                pl.RightAt(79, "Single Knife Edge")

                            Case 2
                                pl.RightAt(79, "Rounded Isolated Obstacle")

                            Case 3
                                pl.RightAt(79, "Double Knife Edge")

                            Case 4
                                pl.RightAt(79, "Irregular Terrain")
                            Case Else
                                pl.RightAt(79, "Unknown Terrain")
                        End Select

                        '	Now do the 80%
                        dBaseWPath = dBase - selRec.pathloss80
                        '	Handle the C/I case
                        If IsCoverI Then
                            '	Calculate the C/I ratios
                            dBaseWPath = selRec.vicpwrrx - dBase + selRec.pathloss80
                        End If

                        If dHCoPol < dHXPol Then
                            TpRunTsip.Tstsrp3.ParenthesizeLowerNumber(coleft, coright, xpleft, xpright)
                            dSelect = dHCoPol
                        Else
                            TpRunTsip.Tstsrp3.ParenthesizeUpperNumber(coleft, coright, xpleft, xpright)
                            dSelect = dHXPol
                        End If

                        pl.LeftAt(82, "H")

                        pl.LeftAt(84, "80.00%")
                        pl.DoubleAt(91, "{0,5:F1}", selRec.pathloss80)
                        pl.LeftAt(97, coleft)
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + If(IsCoverI, dHCoPol, -dHCoPol)) '	selRec.calcico80);
                        pl.LeftAt(-1, coright)
                        pl.LeftAt(106, xpleft)
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + If(IsCoverI, dHXPol, -dHXPol))  ' selRec.calcixp80);
                        pl.LeftAt(-1, xpright)

                        pl.DoubleAt(116, "{0,6:F1}", selRec.reqd80)
                        If IsCoverI Then
                            dMargin = dBaseWPath + dSelect - selRec.reqd80   '	Get the full margin.
                        Else
                            dMargin = selRec.reqd80 - (dBaseWPath - dSelect)   '	Get the full margin.
                        End If
                        pl.DoubleAt(124, "{0,6:F1}", dMargin)   ' selRec.resti80);
                        pl.Output()

                        If eMapUsed = Enums.MapUsed.e250K Then
                            pl.RightAt(79, "1:250K Maps Used.")
                        Else
                            pl.RightAt(79, "1:50K Maps Used.")
                        End If

                        If dVCoPol < dVXPol Then
                            TpRunTsip.Tstsrp3.ParenthesizeLowerNumber(coleft, coright, xpleft, xpright)
                            dSelect = dVCoPol
                        Else
                            TpRunTsip.Tstsrp3.ParenthesizeUpperNumber(coleft, coright, xpleft, xpright)
                            dSelect = dVXPol
                        End If

                        '	Now the V line for 80%
                        pl.LeftAt(82, "V")

                        pl.LeftAt(97, coleft)
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + If(IsCoverI, dVCoPol, -dVCoPol)) '	selRec.calcico80);
                        pl.LeftAt(-1, coright)
                        pl.LeftAt(106, xpleft)
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + If(IsCoverI, dVXPol, -dVCoPol))  ' selRec.calcixp80);
                        pl.LeftAt(-1, xpright)

                        pl.DoubleAt(116, "{0,6:F1}", selRec.reqd80)
                        If IsCoverI Then
                            dMargin = dBaseWPath + dSelect - selRec.reqd80   '	Get the full margin.
                        Else
                            dMargin = selRec.reqd80 - (dBaseWPath - dSelect)   '	Get the full margin.
                        End If
                        pl.DoubleAt(124, "{0,6:F1}", dMargin)   ' selRec.resti80);
                        pl.Output()

                        '	Now the H line for 99.99%
                        dBaseWPath = dBase - selRec.pathloss99
                        If IsCoverI Then
                            '	Calculate the C/I ratios
                            dBaseWPath = selRec.vicpwrrx - dBase + selRec.pathloss99
                        End If

                        If dHCoPol < dHXPol Then
                            TpRunTsip.Tstsrp3.ParenthesizeLowerNumber(coleft, coright, xpleft, xpright)
                            dSelect = dHCoPol
                        Else
                            TpRunTsip.Tstsrp3.ParenthesizeUpperNumber(coleft, coright, xpleft, xpright)
                            dSelect = dHXPol
                        End If

                        pl.LeftAt(82, "H")

                        pl.LeftAt(84, "99.99%")
                        pl.DoubleAt(91, "{0,5:F1}", selRec.pathloss99)
                        pl.LeftAt(97, coleft)
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + If(IsCoverI, dHCoPol, -dHCoPol)) ' selRec.calcico99);
                        pl.LeftAt(-1, coright)
                        pl.LeftAt(106, xpleft)
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + If(IsCoverI, dHXPol, -dHXPol))  ' selRec.calcixp99);
                        pl.LeftAt(-1, xpright)

                        pl.DoubleAt(116, "{0,6:F1}", selRec.reqd99)
                        If IsCoverI Then
                            dMargin = dBaseWPath + dSelect - selRec.reqd99   '	Get the full margin.
                        Else
                            dMargin = selRec.reqd99 - (dBaseWPath - dSelect)   '	Get the full margin.
                        End If
                        pl.DoubleAt(124, "{0,6:F1}", dMargin) ' selRec.resti99);

                        pl.Output()

                        '	Now the V line for 99.99%
                        If dVCoPol < dVXPol Then
                            TpRunTsip.Tstsrp3.ParenthesizeLowerNumber(coleft, coright, xpleft, xpright)
                            dSelect = dVCoPol
                        Else
                            TpRunTsip.Tstsrp3.ParenthesizeUpperNumber(coleft, coright, xpleft, xpright)
                            dSelect = dVXPol
                        End If

                        pl.LeftAt(82, "V")

                        pl.LeftAt(97, coleft)
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + If(IsCoverI, dVCoPol, -dVCoPol)) ' selRec.calcico99);
                        pl.LeftAt(-1, coright)
                        pl.LeftAt(106, xpleft)
                        pl.DoubleAt(-1, "{0,6:F1}", dBaseWPath + If(IsCoverI, dVXPol, -dVXPol))  ' selRec.calcixp99);
                        pl.LeftAt(-1, xpright)

                        pl.DoubleAt(116, "{0,6:F1}", selRec.reqd99)
                        If IsCoverI Then
                            dMargin = dBaseWPath + dSelect - selRec.reqd99   '	Get the full margin.
                        Else
                            dMargin = selRec.reqd99 - (dBaseWPath - dSelect)   '	Get the full margin.
                        End If
                        pl.DoubleAt(124, "{0,6:F1}", dMargin) ' selRec.resti99);
                    ElseIf selRec.ohresult = 0 Then
                        ' This is line of sight. 
                        Dim map = If(eMapUsed = Enums.MapUsed.e250K, "250K", "50K")
                        Dim str = SQLCHARPTR.Format("Line of Sight on {0} maps -- Only reporting Free Space Loss", map)
                        pl.RightAt(79, str)
                    ElseIf selRec.ohresult = 100 Then
                        pl.RightAt(79, "*** Colocated Sites! ***")
                    Else
                        pl.RightAt(79, "Terrain Profiling returned error: ")
                        pl.IntAt(-1, "{0:D}", -selRec.ohresult)
                    End If
                End If
            Else
                '	This is the normal case, Not B into B polarizations, which are a special case.

                pl.LeftAt(82, selRec.intpolar)

                '.if intpolar = vicpolar or intpolar = "B" or  vicpolar = "B" .then
                If selRec.intpolar.Equals(selRec.vicpolar) OrElse selRec.intpolar.Equals("B") OrElse selRec.vicpolar.Equals("B") Then
                    TpRunTsip.Tstsrp3.ParenthesizeLowerNumber(coleft, coright, xpleft, xpright)
                Else
                    TpRunTsip.Tstsrp3.ParenthesizeUpperNumber(coleft, coright, xpleft, xpright)
                End If

                ' This is new format for both OHloss and other calculation -- task 1069 
                Select Case tpParm.spherecalc(0)
                    Case "1"c
                        pl.LeftAt(84, "CCIR")

                    Case "2"c
                        pl.LeftAt(84, "SEL")

                    Case "3"c
                        pl.LeftAt(84, "FSL")

                    Case "4"c
                        pl.LeftAt(84, "HATA")

                    Case "5"c
                        pl.LeftAt(84, "FSL")
                    Case Else
                End Select

                pl.DoubleAt(91, "{0,5:F1}", selRec.patloss)
                pl.LeftAt(97, coleft)
                pl.DoubleAt(-1, "{0,6:F1}", selRec.calcico)
                pl.LeftAt(-1, coright)
                pl.LeftAt(106, xpleft)
                pl.DoubleAt(-1, "{0,6:F1}", selRec.calcixp)
                pl.LeftAt(-1, xpright)

                pl.DoubleAt(116, "{0,6:F1}", selRec.reqdcalc)
                pl.DoubleAt(124, "{0,6:F1}", selRec.resti)

                If tpParm.spherecalc.Equals("5") Then
                    pl.Output()

                    ' We ignore the following if there was an error, or it was free space
                    '    loss (i.e. no obstructions found. 
                    If selRec.ohresult >= 1000 Then
                        eMapUsed = Enums.MapUsed.e50K
                        selRec.ohresult -= 1000
                    End If
                    If selRec.ohresult > 0 AndAlso selRec.ohresult < 100 Then
                        Select Case selRec.ohresult
                            Case 1
                                pl.RightAt(79, "Single Knife Edge")

                            Case 2
                                pl.RightAt(79, "Rounded Isolated Obstacle")

                            Case 3
                                pl.RightAt(79, "Double Knife Edge")

                            Case 4
                                pl.RightAt(79, "Irregular Terrain")
                            Case Else
                                pl.RightAt(79, "Unknown Terrain")
                        End Select
                        pl.LeftAt(84, "80.00%")
                        pl.DoubleAt(91, "{0,5:F1}", selRec.pathloss80)
                        pl.LeftAt(97, coleft)
                        pl.DoubleAt(-1, "{0,6:F1}", selRec.calcico80)
                        pl.LeftAt(-1, coright)
                        pl.LeftAt(106, xpleft)
                        pl.DoubleAt(-1, "{0,6:F1}", selRec.calcixp80)
                        pl.LeftAt(-1, xpright)

                        pl.DoubleAt(116, "{0,6:F1}", selRec.reqd80)
                        pl.DoubleAt(124, "{0,6:F1}", selRec.resti80)

                        pl.Output()

                        If eMapUsed = Enums.MapUsed.e250K Then
                            pl.RightAt(79, "1:250K Maps Used.")
                        Else
                            pl.RightAt(79, "1:50K Maps Used.")
                        End If

                        pl.LeftAt(84, "99.99%")
                        pl.DoubleAt(91, "{0,5:F1}", selRec.pathloss99)
                        pl.LeftAt(97, coleft)
                        pl.DoubleAt(-1, "{0,6:F1}", selRec.calcico99)
                        pl.LeftAt(-1, coright)
                        pl.LeftAt(106, xpleft)
                        pl.DoubleAt(-1, "{0,6:F1}", selRec.calcixp99)
                        pl.LeftAt(-1, xpright)

                        pl.DoubleAt(116, "{0,6:F1}", selRec.reqd99)
                        pl.DoubleAt(124, "{0,6:F1}", selRec.resti99)
                    ElseIf selRec.ohresult = 0 Then
                        ' This is line of sight. 
                        Dim map = If(eMapUsed = Enums.MapUsed.e250K, "250K", "50K")
                        Dim str = SQLCHARPTR.Format("Line of Sight on {0} maps -- Only reporting Free Space Loss", map)
                        pl.RightAt(79, str)
                    ElseIf selRec.ohresult = 100 Then
                        pl.RightAt(79, "*** Colocated Sites! ***")
                    Else
                        pl.RightAt(79, "Terrain Profiling returned error: ")
                        pl.IntAt(-1, "{0:D}", -selRec.ohresult)
                    End If
                End If
            End If

            pl.Output()

            '...Log2.v("\nTstsrp3.ReportttChanDetail(): Exit");
            Return
        End Sub

        ''' <summary>
        ''' This is a 'helper' method that returns the appropriate ' ', '(' and ')' characters
        ''' to decorate a pair of numbers; the parentheses are around the lower number in a dyad.
        ''' </summary>
        ''' <paramname="coleft"> - returns a space character.</param>
        ''' <paramname="coright"> - returns a space character.</param>
        ''' <paramname="xpleft"> - returns a '(' character.</param>
        ''' <paramname="xpright"> - returns a ')' character.</param>
        Public Shared Sub ParenthesizeLowerNumber(<Out> ByRef coleft As String, <Out> ByRef coright As String, <Out> ByRef xpleft As String, <Out> ByRef xpright As String)
            '	Put the parentheses around the lower number.
            coleft = TpRunTsip.Tstsrp3.SPACE
            coright = TpRunTsip.Tstsrp3.SPACE
            xpleft = TpRunTsip.Tstsrp3.LEFT_PARENTHESIS
            xpright = TpRunTsip.Tstsrp3.RIGHT_PARENTHESIS
        End Sub

        ''' <summary>
        ''' This is a 'helper' method that returns the appropriate ' ', '(' and ')' characters
        ''' to decorate a pair of numbers; the parentheses are around the upper number in a dyad.
        ''' </summary>
        ''' <paramname="coleft"> - returns a '(' character.</param>
        ''' <paramname="coright"> - returns a ')' character.</param>
        ''' <paramname="xpleft"> - returns a space character.</param>
        ''' <paramname="xpright"> - returns a space character.</param>
        Public Shared Sub ParenthesizeUpperNumber(<Out> ByRef coleft As String, <Out> ByRef coright As String, <Out> ByRef xpleft As String, <Out> ByRef xpright As String)
            '	Put the parentheses around the lower number.
            coleft = TpRunTsip.Tstsrp3.LEFT_PARENTHESIS
            coright = TpRunTsip.Tstsrp3.RIGHT_PARENTHESIS
            xpleft = TpRunTsip.Tstsrp3.SPACE
            xpright = TpRunTsip.Tstsrp3.SPACE
        End Sub


    End Class
End Namespace
