Imports System
Imports _Configuration
Imports _DataStructures
Imports _NewLib
Imports _Utillib
Imports SQLHANDLE = System.IntPtr
Imports SQLRETURN = System.Int16
Imports SQLLEN = System.Int64
Imports SQLPOINTER = System.IntPtr

Imports System.Runtime.InteropServices

Namespace TpRunTsip

    ''' <summary>
    ''' Provides methods to prepare and insert 
    ''' records into the database table <b>&lt;userID&gt;.tt_&lt;tableName&gt;_&lt;runID&gt;_chan</b>
    ''' </summary>
    ''' <remarks>
    ''' <listtype="bullet">
    ''' <item> The methods in this class are used to perform operations on the
    ''' tt CHAN table. The actual CHAN table name is a variable, prescribed by the caller.</item>
    ''' <item>The supported operations are: TtPrepareChan, TtInsertChan and TtChanClose.</item>
    ''' <item>The caller must first call the method TtPrepareChan() that creates a cursor 
    ''' for use with a subsequent call to TtInsertChan().</item>
    ''' <item>The method closeChan must be called to release internal cursor resources (a cursor 
    ''' encapsulates an ODBC connection and statement handle et. al.).</item>
    ''' </list>
    ''' </remarks>
    Public Class TtDynChan
#If PINVOKE
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static void set_ttDynChan_ConnStmtHandles([In] SQLHANDLE hConnection, [In] SQLHANDLE hStatement);
#End If
        Private Shared hConnInsert As SQLHANDLE
        Private Shared hStmtInsert As SQLHANDLE

        Private Shared hConnSelect As SQLHANDLE   '	Use different local variables for the read.
        Private Shared hStmtSelect As SQLHANDLE


        ''' <summary>
        ''' This method closes the internal (private) cursor object by freeing the ODBC 
        ''' connection and statement handles. 
        ''' </summary>
        ''' <paramname=""></param> 
        Public Shared Sub TtChanClose()
            If TpRunTsip.TtDynChan.hStmtSelect <> SQLHANDLE.Zero Then
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TtDynChan.hStmtSelect)
                TpRunTsip.TtDynChan.hStmtSelect = SQLHANDLE.Zero
                Ssutil.DisConn(TpRunTsip.TtDynChan.hConnSelect)
                TpRunTsip.TtDynChan.hConnSelect = SQLHANDLE.Zero
            End If

            If TpRunTsip.TtDynChan.hStmtInsert <> SQLHANDLE.Zero Then
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TtDynChan.hStmtInsert)
                TpRunTsip.TtDynChan.hStmtInsert = SQLHANDLE.Zero
                Ssutil.DisConn(TpRunTsip.TtDynChan.hConnInsert)
                TpRunTsip.TtDynChan.hConnInsert = SQLHANDLE.Zero
            End If

            Return
        End Sub

        ''' <summary>
        ''' This method closes the internal (private) cursor object by freeing the ODBC 
        ''' connection and statement handles. 
        ''' </summary>
        ''' <paramname=""></param> 
        Public Shared Sub TtChanClose(hConn As SQLHANDLE, hStmt As SQLHANDLE)
            If hStmt <> SQLHANDLE.Zero Then
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
                hStmt = SQLHANDLE.Zero
                Ssutil.DisConn(hConn)
                hConn = SQLHANDLE.Zero
            End If

            Return
        End Sub

        ''' <summary>
        ''' This method calls ODBC.SQLPrepare() to prepare a table insert on an internal (private) statement 
        ''' handle that can then be used in a subsequent call to ODBC.SQLExecute to
        ''' actually perform the SQL record insertion.
        ''' </summary>
        ''' <paramname="table"> - full name of the DB table into which the record is to be inserted.</param>
        ''' <returns></returns>
        Public Shared Function TtPrepareChan(table As String) As Integer
            Dim insert_buf As String
            Dim sqlRet As SQLRETURN

            insert_buf = String.Format("insert into {0} (interferer, intcall1, intcall2, intbndcde, intanum, intchid, viccall1,  viccall2, vicbndcde, vicanum, vicchid, intpolar, vicpolar, intstattx, vicstatrx, inttraftx, victrafrx,  inteqpttx, viceqptrx, intfreqtx, vicfreqrx, vicpwrrx, intpwrtx, intafsltx,  vicafslrx, rxant, txant, ctxinttraftx, ctxvictrafrx, ctxeqpt,  calctype, report, totantdisc, freqsep, reqdcalc, patloss, calcico, calcixp,  resti, eirpadv, tiltdisc,  pathloss80, calcico80, calcixp80, reqd80, resti80,  pathloss99, calcico99, calcixp99, reqd99, resti99,  ohresult, rqco,  processed, caseno, ctxinteqpt, inteqtype, viceqtype,  intbwchans, vicbwchans) values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", table)

            TpRunTsip.TtDynChan.hConnInsert = Ssutil.NewConn()

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TtDynChan.hConnInsert, TpRunTsip.TtDynChan.hStmtInsert)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynAnte.TtPrepareChan(): ERROR: call to SQLAllocHandle() failed.")
                Return [Error].ODBC_SQLALLOCHANDLE_FAILED
            End If

            sqlRet = ODBC.SQLPrepare(TpRunTsip.TtDynChan.hStmtInsert, insert_buf, insert_buf.Length)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynAnte.TtPrepareChan(): ERROR: call to SQLPrepare() failed.")
                Ssutil.DbGetDiagStmt(TpRunTsip.TtDynChan.hStmtInsert, "ttPrepareChan01: Error preparing: " & insert_buf)
                Return [Error].ODBC_PREPARE_FAILED
            End If

            ' AH: REMOVE when possible.
            ' This passes hConn and hStmy handles instantiated in C# over to the 
            ' associated C/C++ native code.
#If PINVOKE
            set_ttDynChan_ConnStmtHandles(hConnInsert, hStmtInsert);
#End If
            Return Constant.SUCCESS
        End Function


        ''' <summary>
        ''' This method calls ODBC.SQLExecute() to perform an SQL 'SELECT' query 
        ''' on an internal (private) ODBC statement handle that can then be used in a 
        ''' subsequent call to ODBC.SQLFetch() to retrieve the selected records.        
        ''' </summary>
        ''' <paramname="table"> - full name of the DB table to be read.</param>
        ''' <paramname="orderby"> - 'ORDER BY' clause in the SQL SELECT query.</param>
        ''' <paramname="cInWhere"> - 'WHERE' clause in the SQL SELECT query.</param>
        ''' <paramname="hConn"> - ODBC connection handle.</param>
        ''' <paramname="hStmt"> - ODBC statement handle.</param>
        ''' <returns></returns>
        Public Shared Function TtChanPrepareRead(table As String, orderby As String, cInWhere As String, <Out> ByRef hConn As SQLHANDLE, <Out> ByRef hStmt As SQLHANDLE) As Integer
            ' 'out' requirement.
            hConn = SQLHANDLE.Zero
            hStmt = SQLHANDLE.Zero

            Dim read_buf As String

            Dim cOrder As String
            Dim cWhere As String

            Dim sqlRet As SQLRETURN

            If Equals(orderby, Nothing) OrElse orderby.Equals("") Then
                cOrder = ""
            Else
                cOrder = " ORDER BY "
                cOrder += orderby
            End If

            If Equals(cInWhere, Nothing) OrElse cInWhere.Equals("") Then
                cWhere = ""
            Else
                cWhere = " WHERE "
                cWhere += cInWhere
            End If

            read_buf = String.Format("select interferer, intcall1, intcall2, intbndcde, intanum,intchid, viccall1, viccall2, vicbndcde, vicanum, vicchid,intpolar, vicpolar, intstattx,vicstatrx, inttraftx, victrafrx, inteqpttx, viceqptrx, intfreqtx, vicfreqrx,vicpwrrx, intpwrtx, intafsltx, vicafslrx, rxant, txant, ctxinttraftx,ctxvictrafrx, ctxeqpt, calctype, report, totantdisc, freqsep,reqdcalc, patloss, calcico, calcixp, resti, eirpadv,tiltdisc, pathloss80, calcico80, calcixp80, reqd80, resti80, pathloss99, calcico99, calcixp99, reqd99, resti99, ohresult, rqco, processed, caseno, ctxinteqpt, inteqtype, viceqtype, intbwchans, vicbwchans from {0}.{1} {2} {3} ", Info.GlobalSchema, table, cWhere, cOrder)

            hConn = Ssutil.NewConn()

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)
            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynChan.TtChanPrepareRead(): ERROR: SQLAllocHandle() failed.")
                Return [Error].ODBC_SQLALLOCHANDLE_FAILED
            End If

            sqlRet = ODBC.SQLExecDirect(hStmt, read_buf, read_buf.Length)

            If Not ODBC.IsOK(sqlRet) Then
                If sqlRet = ODBC.SQL_NO_DATA Then
                    Return Constant.NOMORERECS
                Else
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynChan.TtChanPrepareRead(): ERROR: SQLExecDirect() failed.")
                    Ssutil.DbGetDiagStmt(hStmt, "ttChanPrepareRead02: Read error:")
                    Return [Error].ODBC_EXECUTE_FAILED - 2
                End If
            End If

            Return 0
        End Function

        ''' <summary>
        ''' This method performs an ODBC.SQLFetch() call on a statement handle 
        ''' prepared in a previous call to TtChanPrepareRead().
        ''' </summary>
        ''' <paramname="hStmt"> - ODBC statement handle.</param>
        ''' <paramname="pChan"> - TtChan object (record) to be read from the DB table.</param>
        ''' <paramname="nullInd"> - array of ODBC nullInds associated with pChan.</param>
        ''' <returns></returns>
        Public Shared Function TtChanRead(hStmt As SQLHANDLE, <Out> ByRef pChan As TtChan, <Out> ByRef nullInd As SQLLEN()) As Integer
            ' 'out' requirements.
            pChan = Nothing
            nullInd = Nothing

            Dim sqlRet As SQLRETURN

            sqlRet = ODBC.SQLFetch(hStmt)
            If Not ODBC.IsOK(sqlRet) Then
                If ODBC.IsNoData(sqlRet) Then
                    Return ODBC.SQL_NO_DATA
                Else
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynChan.TtChanRead(): ERROR: SQLFetch() failed.")
                    Ssutil.DbGetDiagStmt(hStmt, "ttChanRead01: Could not fetch.")
                    Return [Error].ODBC_FETCH_FAILED
                End If
            End If

            pChan = New TtChan()
            nullInd = NullHelper.CreateArrayOfNullInd(TtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

            Try
                Ssutil.DbStartGets()
                Ssutil.DbGetString(hStmt, 0, "interferer", pChan.interferer, TtChan.INTERFERER_SZ, nullInd(TtChan.INTERFERER))
                Ssutil.DbGetString(hStmt, 0, "intcall1", pChan.intcall1, TtChan.INTCALL1_SZ, nullInd(TtChan.INTCALL1))
                Ssutil.DbGetString(hStmt, 0, "intcall2", pChan.intcall2, TtChan.INTCALL2_SZ, nullInd(TtChan.INTCALL2))
                Ssutil.DbGetString(hStmt, 0, "intbndcde", pChan.intbndcde, TtChan.INTBNDCDE_SZ, nullInd(TtChan.INTBNDCDE))
                Ssutil.DbGetShort(hStmt, 0, "intanum", pChan.intanum, nullInd(TtChan.INTANUM))
                Ssutil.DbGetString(hStmt, 0, "intchid", pChan.intchid, TtChan.INTCHID_SZ, nullInd(TtChan.INTCHID))
                Ssutil.DbGetString(hStmt, 0, "viccall1", pChan.viccall1, TtChan.VICCALL1_SZ, nullInd(TtChan.VICCALL1))
                Ssutil.DbGetString(hStmt, 0, "viccall2", pChan.viccall2, TtChan.VICCALL2_SZ, nullInd(TtChan.VICCALL2))
                Ssutil.DbGetString(hStmt, 0, "vicbndcde", pChan.vicbndcde, TtChan.VICBNDCDE_SZ, nullInd(TtChan.VICBNDCDE))
                Ssutil.DbGetShort(hStmt, 0, "vicanum", pChan.vicanum, nullInd(TtChan.VICANUM))
                Ssutil.DbGetString(hStmt, 0, "vicchid", pChan.vicchid, TtChan.VICCHID_SZ, nullInd(TtChan.VICCHID))
                Ssutil.DbGetString(hStmt, 0, "intpolar", pChan.intpolar, TtChan.INTPOLAR_SZ, nullInd(TtChan.INTPOLAR))
                Ssutil.DbGetString(hStmt, 0, "vicpolar", pChan.vicpolar, TtChan.VICPOLAR_SZ, nullInd(TtChan.VICPOLAR))
                Ssutil.DbGetString(hStmt, 0, "intstattx", pChan.intstattx, TtChan.INTSTATTX_SZ, nullInd(TtChan.INTSTATTX))
                Ssutil.DbGetString(hStmt, 0, "vicstatrx", pChan.vicstatrx, TtChan.VICSTATRX_SZ, nullInd(TtChan.VICSTATRX))
                Ssutil.DbGetString(hStmt, 0, "inttraftx", pChan.inttraftx, TtChan.INTTRAFTX_SZ, nullInd(TtChan.INTTRAFTX))
                Ssutil.DbGetString(hStmt, 0, "victrafrx", pChan.victrafrx, TtChan.VICTRAFRX_SZ, nullInd(TtChan.VICTRAFRX))
                Ssutil.DbGetString(hStmt, 0, "inteqpttx", pChan.inteqpttx, TtChan.INTEQPTTX_SZ, nullInd(TtChan.INTEQPTTX))
                Ssutil.DbGetString(hStmt, 0, "viceqptrx", pChan.viceqptrx, TtChan.VICEQPTRX_SZ, nullInd(TtChan.VICEQPTRX))
                Ssutil.DbGetDouble(hStmt, 0, "intfreqtx", pChan.intfreqtx, nullInd(TtChan.INTFREQTX))
                Ssutil.DbGetDouble(hStmt, 0, "vicfreqrx", pChan.vicfreqrx, nullInd(TtChan.VICFREQRX))
                Ssutil.DbGetDouble(hStmt, 0, "vicpwrrx", pChan.vicpwrrx, nullInd(TtChan.VICPWRRX))
                Ssutil.DbGetDouble(hStmt, 0, "intpwrtx", pChan.intpwrtx, nullInd(TtChan.INTPWRTX))
                Ssutil.DbGetDouble(hStmt, 0, "intafsltx", pChan.intafsltx, nullInd(TtChan.INTAFSLTX))
                Ssutil.DbGetDouble(hStmt, 0, "vicafslrx", pChan.vicafslrx, nullInd(TtChan.VICAFSLRX))
                Ssutil.DbGetShort(hStmt, 0, "rxant", pChan.rxant, nullInd(TtChan.RXANT))
                Ssutil.DbGetShort(hStmt, 0, "txant", pChan.txant, nullInd(TtChan.TXANT))
                Ssutil.DbGetString(hStmt, 0, "ctxinttraftx", pChan.ctxinttraftx, TtChan.CTXINTTRAFTX_SZ, nullInd(TtChan.CTXINTTRAFTX))
                Ssutil.DbGetString(hStmt, 0, "ctxvictrafrx", pChan.ctxvictrafrx, TtChan.CTXVICTRAFRX_SZ, nullInd(TtChan.CTXVICTRAFRX))
                Ssutil.DbGetString(hStmt, 0, "ctxeqpt", pChan.ctxeqpt, TtChan.CTXEQPT_SZ, nullInd(TtChan.CTXEQPT))
                Ssutil.DbGetString(hStmt, 0, "calctype", pChan.calctype, TtChan.CALCTYPE_SZ, nullInd(TtChan.CALCTYPE))
                Ssutil.DbGetShort(hStmt, 0, "report", pChan.report, nullInd(TtChan.REPORT))
                Ssutil.DbGetDouble(hStmt, 0, "totantdisc", pChan.totantdisc, nullInd(TtChan.TOTANTDISC))
                Ssutil.DbGetDouble(hStmt, 0, "freqsep", pChan.freqsep, nullInd(TtChan.FREQSEP))
                Ssutil.DbGetDouble(hStmt, 0, "reqdcalc", pChan.reqdcalc, nullInd(TtChan.REQDCALC))
                Ssutil.DbGetDouble(hStmt, 0, "patloss", pChan.patloss, nullInd(TtChan.PATLOSS))
                Ssutil.DbGetDouble(hStmt, 0, "calcico", pChan.calcico, nullInd(TtChan.CALCICO))
                Ssutil.DbGetDouble(hStmt, 0, "calcixp", pChan.calcixp, nullInd(TtChan.CALCIXP))
                Ssutil.DbGetDouble(hStmt, 0, "resti", pChan.resti, nullInd(TtChan.RESTI))
                Ssutil.DbGetDouble(hStmt, 0, "eirpadv", pChan.eirpadv, nullInd(TtChan.EIRPADV))
                Ssutil.DbGetDouble(hStmt, 0, "tiltdisc", pChan.tiltdisc, nullInd(TtChan.TILTDISC))
                Ssutil.DbGetDouble(hStmt, 0, "pathloss80", pChan.pathloss80, nullInd(TtChan.PATHLOSS80))
                Ssutil.DbGetDouble(hStmt, 0, "calcico80", pChan.calcico80, nullInd(TtChan.CALCICO80))
                Ssutil.DbGetDouble(hStmt, 0, "calcixp80", pChan.calcixp80, nullInd(TtChan.CALCIXP80))
                Ssutil.DbGetDouble(hStmt, 0, "reqd80", pChan.reqd80, nullInd(TtChan.REQD80))
                Ssutil.DbGetDouble(hStmt, 0, "resti80", pChan.resti80, nullInd(TtChan.RESTI80))
                Ssutil.DbGetDouble(hStmt, 0, "pathloss99", pChan.pathloss99, nullInd(TtChan.PATHLOSS99))
                Ssutil.DbGetDouble(hStmt, 0, "calcico99", pChan.calcico99, nullInd(TtChan.CALCICO99))
                Ssutil.DbGetDouble(hStmt, 0, "calcixp99", pChan.calcixp99, nullInd(TtChan.CALCIXP99))
                Ssutil.DbGetDouble(hStmt, 0, "reqd99", pChan.reqd99, nullInd(TtChan.REQD99))
                Ssutil.DbGetDouble(hStmt, 0, "resti99", pChan.resti99, nullInd(TtChan.RESTI99))
                Ssutil.DbGetInt(hStmt, 0, "ohresult", pChan.ohresult, nullInd(TtChan.OHRESULT))
                Ssutil.DbGetDouble(hStmt, 0, "rqco", pChan.rqco, nullInd(TtChan.RQCO))
                Ssutil.DbGetInt(hStmt, 0, "processed", pChan.processed, nullInd(TtChan.PROCESSED))
                Ssutil.DbGetInt(hStmt, 0, "caseno", pChan.caseno, nullInd(TtChan.CASENO))
                Ssutil.DbGetString(hStmt, 0, "ctxinteqpt", pChan.ctxinteqpt, TtChan.CTXINTEQPT_SZ, nullInd(TtChan.CTXINTEQPT))
                Ssutil.DbGetString(hStmt, 0, "inteqtype", pChan.inteqtype, TtChan.INTEQTYPE_SZ, nullInd(TtChan.INTEQTYPE))
                Ssutil.DbGetString(hStmt, 0, "viceqtype", pChan.viceqtype, TtChan.VICEQTYPE_SZ, nullInd(TtChan.VICEQTYPE))
                Ssutil.DbGetDouble(hStmt, 0, "intbwchans", pChan.intbwchans, nullInd(TtChan.INTBWCHANS))
                Ssutil.DbGetDouble(hStmt, 0, "vicbwchans", pChan.vicbwchans, nullInd(TtChan.VICBWCHANS))
            Catch e As Exception
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynChan.TtChanRead(): ERROR: ODBC 'Get' request failed: " & e.Message)
                Ssutil.DbGetDiagStmt(hStmt, "ttChanRead02: Get column error for " & e.Message)
                Return [Error].ODBC_GET_FAILED
            End Try

            pChan.calctype.Trim()  '	length is used as a marker.

            Return 0
        End Function

        ''' <summary>
        ''' This method calls ODBC.SQLExecute() to perform insertion of a record
        ''' into a DB table by using the internal (private) ODBC statement handle
        ''' that was prepared on by a previous call to ODBC.SQLPrepare().
        ''' </summary>
        ''' <paramname="insertStruct"> - TtChan object (record) to be inserted into the DB table.</param>
        ''' <paramname="nullInd"> - array of ODBC nullInds associated with ttChan.</param>
        ''' <returns></returns>
        Public Shared Function TtInsertChan(insertStruct As TtChan, nullInd As SQLLEN()) As Integer
            '...Log2.v("\nttInsertChan(): Entry");

            Dim sqlRet As SQLRETURN

            '...Log2.v("\nTtDynChan.TtInsertChan():\n" + insertStruct.ToStringWN(nullInd));

            ' Check that the static hStmt has been instanciated (by a previous 
            ' call to TtPrepareChan()).
            If TpRunTsip.TtDynChan.hStmtInsert Is Nothing Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynChan.TtInsertChan(): ERROR: hStmt is null.")
                Return [Error].ODBC_NULL_HANDLE
            End If

            'The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            'to contain the values to bind to. This neccessitates copying the values of the nullInd
            'array elements into global memory with an SQLLENPTR pointer assigned to each one.
            Dim nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd)

            'The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            'to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            'into global memory with an SQLPOINTER pointer assigned to each one. TtChan provides
            'a convenience method that does exactly this.
            Dim parameterValuePtr As SQLPOINTER() = insertStruct.CopyToArrayOfSQLPOINTERs()

            Try
                Ssutil.DbStartBinds()

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "interferer", parameterValuePtr(TtChan.INTERFERER), TtChan.INTERFERER_SZ, nullIndPtr(TtChan.INTERFERER))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "intcall1", parameterValuePtr(TtChan.INTCALL1), TtChan.INTCALL1_SZ, nullIndPtr(TtChan.INTCALL1))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "intcall2", parameterValuePtr(TtChan.INTCALL2), TtChan.INTCALL2_SZ, nullIndPtr(TtChan.INTCALL2))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "intbndcde", parameterValuePtr(TtChan.INTBNDCDE), TtChan.INTBNDCDE_SZ, nullIndPtr(TtChan.INTBNDCDE))

                Ssutil.DbBindShortInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "intanum", parameterValuePtr(TtChan.INTANUM), nullIndPtr(TtChan.INTANUM))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "intchid", parameterValuePtr(TtChan.INTCHID), TtChan.INTCHID_SZ, nullIndPtr(TtChan.INTCHID))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "viccall1", parameterValuePtr(TtChan.VICCALL1), TtChan.VICCALL1_SZ, nullIndPtr(TtChan.VICCALL1))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "viccall2", parameterValuePtr(TtChan.VICCALL2), TtChan.VICCALL2_SZ, nullIndPtr(TtChan.VICCALL2))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "vicbndcde", parameterValuePtr(TtChan.VICBNDCDE), TtChan.VICBNDCDE_SZ, nullIndPtr(TtChan.VICBNDCDE))

                Ssutil.DbBindShortInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "vicanum", parameterValuePtr(TtChan.VICANUM), nullIndPtr(TtChan.VICANUM))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "vicchid", parameterValuePtr(TtChan.VICCHID), TtChan.VICCHID_SZ, nullIndPtr(TtChan.VICCHID))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "intpolar", parameterValuePtr(TtChan.INTPOLAR), TtChan.INTPOLAR_SZ, nullIndPtr(TtChan.INTPOLAR))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "vicpolar", parameterValuePtr(TtChan.VICPOLAR), TtChan.VICPOLAR_SZ, nullIndPtr(TtChan.VICPOLAR))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "intstattx", parameterValuePtr(TtChan.INTSTATTX), TtChan.INTSTATTX_SZ, nullIndPtr(TtChan.INTSTATTX))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "vicstatrx", parameterValuePtr(TtChan.VICSTATRX), TtChan.VICSTATRX_SZ, nullIndPtr(TtChan.VICSTATRX))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "inttraftx", parameterValuePtr(TtChan.INTTRAFTX), TtChan.INTTRAFTX_SZ, nullIndPtr(TtChan.INTTRAFTX))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "victrafrx", parameterValuePtr(TtChan.VICTRAFRX), TtChan.VICTRAFRX_SZ, nullIndPtr(TtChan.VICTRAFRX))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "inteqpttx", parameterValuePtr(TtChan.INTEQPTTX), TtChan.INTEQPTTX_SZ, nullIndPtr(TtChan.INTEQPTTX))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "viceqptrx", parameterValuePtr(TtChan.VICEQPTRX), TtChan.VICEQPTRX_SZ, nullIndPtr(TtChan.VICEQPTRX))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "intfreqtx", parameterValuePtr(TtChan.INTFREQTX), nullIndPtr(TtChan.INTFREQTX))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "vicfreqrx", parameterValuePtr(TtChan.VICFREQRX), nullIndPtr(TtChan.VICFREQRX))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "vicpwrrx", parameterValuePtr(TtChan.VICPWRRX), nullIndPtr(TtChan.VICPWRRX))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "intpwrtx", parameterValuePtr(TtChan.INTPWRTX), nullIndPtr(TtChan.INTPWRTX))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "intafsltx", parameterValuePtr(TtChan.INTAFSLTX), nullIndPtr(TtChan.INTAFSLTX))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "vicafslrx", parameterValuePtr(TtChan.VICAFSLRX), nullIndPtr(TtChan.VICAFSLRX))

                Ssutil.DbBindShortInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "rxant", parameterValuePtr(TtChan.RXANT), nullIndPtr(TtChan.RXANT))

                Ssutil.DbBindShortInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "txant", parameterValuePtr(TtChan.TXANT), nullIndPtr(TtChan.TXANT))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "ctxinttraftx", parameterValuePtr(TtChan.CTXINTTRAFTX), TtChan.CTXINTTRAFTX_SZ, nullIndPtr(TtChan.CTXINTTRAFTX))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "ctxvictrafrx", parameterValuePtr(TtChan.CTXVICTRAFRX), TtChan.CTXVICTRAFRX_SZ, nullIndPtr(TtChan.CTXVICTRAFRX))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "ctxeqpt", parameterValuePtr(TtChan.CTXEQPT), TtChan.CTXEQPT_SZ, nullIndPtr(TtChan.CTXEQPT))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "calctype", parameterValuePtr(TtChan.CALCTYPE), TtChan.CALCTYPE_SZ, nullIndPtr(TtChan.CALCTYPE))

                Ssutil.DbBindShortInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "report", parameterValuePtr(TtChan.REPORT), nullIndPtr(TtChan.REPORT))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "totantdisc", parameterValuePtr(TtChan.TOTANTDISC), nullIndPtr(TtChan.TOTANTDISC))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "freqsep", parameterValuePtr(TtChan.FREQSEP), nullIndPtr(TtChan.FREQSEP))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "reqdcalc", parameterValuePtr(TtChan.REQDCALC), nullIndPtr(TtChan.REQDCALC))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "patloss", parameterValuePtr(TtChan.PATLOSS), nullIndPtr(TtChan.PATLOSS))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "calcico", parameterValuePtr(TtChan.CALCICO), nullIndPtr(TtChan.CALCICO))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "calcixp", parameterValuePtr(TtChan.CALCIXP), nullIndPtr(TtChan.CALCIXP))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "resti", parameterValuePtr(TtChan.RESTI), nullIndPtr(TtChan.RESTI))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "eirpadv", parameterValuePtr(TtChan.EIRPADV), nullIndPtr(TtChan.EIRPADV))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "tiltdisc", parameterValuePtr(TtChan.TILTDISC), nullIndPtr(TtChan.TILTDISC))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "pathloss80", parameterValuePtr(TtChan.PATHLOSS80), nullIndPtr(TtChan.PATHLOSS80))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "calcico80", parameterValuePtr(TtChan.CALCICO80), nullIndPtr(TtChan.CALCICO80))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "calcixp80", parameterValuePtr(TtChan.CALCIXP80), nullIndPtr(TtChan.CALCIXP80))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "reqd80", parameterValuePtr(TtChan.REQD80), nullIndPtr(TtChan.REQD80))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "resti80", parameterValuePtr(TtChan.RESTI80), nullIndPtr(TtChan.RESTI80))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "pathloss99", parameterValuePtr(TtChan.PATHLOSS99), nullIndPtr(TtChan.PATHLOSS99))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "calcico99", parameterValuePtr(TtChan.CALCICO99), nullIndPtr(TtChan.CALCICO99))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "calcixp99", parameterValuePtr(TtChan.CALCIXP99), nullIndPtr(TtChan.CALCIXP99))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "reqd99", parameterValuePtr(TtChan.REQD99), nullIndPtr(TtChan.REQD99))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "resti99", parameterValuePtr(TtChan.RESTI99), nullIndPtr(TtChan.RESTI99))

                Ssutil.DbBindIntInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "ohresult", parameterValuePtr(TtChan.OHRESULT), nullIndPtr(TtChan.OHRESULT))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "rqco", parameterValuePtr(TtChan.RQCO), nullIndPtr(TtChan.RQCO))

                Ssutil.DbBindIntInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "processed", parameterValuePtr(TtChan.PROCESSED), nullIndPtr(TtChan.PROCESSED))

                Ssutil.DbBindIntInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "caseno", parameterValuePtr(TtChan.CASENO), nullIndPtr(TtChan.CASENO))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "ctxinteqpt", parameterValuePtr(TtChan.CTXINTEQPT), TtChan.CTXINTEQPT_SZ, nullIndPtr(TtChan.CTXINTEQPT))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "inteqtype", parameterValuePtr(TtChan.INTEQTYPE), TtChan.INTEQTYPE_SZ, nullIndPtr(TtChan.INTEQTYPE))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "viceqtype", parameterValuePtr(TtChan.VICEQTYPE), TtChan.VICEQTYPE_SZ, nullIndPtr(TtChan.VICEQTYPE))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "intbwchans", parameterValuePtr(TtChan.INTBWCHANS), nullIndPtr(TtChan.INTBWCHANS))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynChan.hStmtInsert, 0, "vicbwchans", parameterValuePtr(TtChan.VICBWCHANS), nullIndPtr(TtChan.VICBWCHANS))
            Catch e As Exception
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynChan.TtInsertChan(): ERROR: ODBC binding attempt failed:  " & e.Message)
                Ssutil.DbGetDiagStmt(TpRunTsip.TtDynChan.hStmtInsert, "ttInsertChan02: Problem binding column " & e.Message)
                Return [Error].ODBC_BINDING_FAILED
            End Try

            sqlRet = ODBC.SQLExecute(TpRunTsip.TtDynChan.hStmtInsert)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynChan.TtInsertChan(): ERROR: call to SQLExecute() failed.")
                Return [Error].ODBC_EXECUTE_FAILED
            End If

            '...Log2.v("\nttInsertChan(): Exit");
            Return Constant.SUCCESS
        End Function





    End Class
End Namespace
