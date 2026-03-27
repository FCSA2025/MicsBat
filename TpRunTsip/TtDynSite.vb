Imports System
Imports _Configuration
Imports _DataStructures
Imports _NewLib
Imports _Utillib
Imports SQLCHARPTR = System.String            'Invented to mimic (char *) for [In]  only.
Imports SQLHANDLE = System.IntPtr
Imports SQLLEN = System.Int64
Imports SQLPOINTER = System.IntPtr
Imports SQLRETURN = System.Int16

Namespace TpRunTsip

    ''' <summary>
    ''' Provides methods to prepare and insert 
    ''' records into the database table <b>&lt;userID&gt;.tt_&lt;tableName&gt;_&lt;runID&gt;_site</b>
    ''' </summary>
    ''' <remarks>
    ''' <listtype="bullet">
    ''' <item> The methods in this class are used to perform operations on the
    ''' tt SITE table. The actual SITE table name is a variable, prescribed by the caller.</item>
    ''' <item>The supported operations are: TtPrepareSite, TtInsertSite and TtSiteClose.</item>
    ''' <item>The caller must first call the method TtPrepareSite() that creates a cursor 
    ''' for use with a subsequent call to TtInsertSite().</item>
    ''' <item>The method closeSite must be called to release internal cursor resources (a cursor 
    ''' encapsulates an ODBC connection and statement handle et. al.).</item>
    ''' </list>
    ''' </remarks>
    Public Class TtDynSite
#If PINVOKE
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static void set_ttDynSite_ConnStmtHandles([In] SQLHANDLE hConnection, [In] SQLHANDLE hStatement);
#End If

        Private Shared hConn As SQLHANDLE
        Private Shared hStmtInsert As SQLHANDLE

        Private Shared hTTConn As SQLHANDLE   '	Use different local variables for the read.
        Private Shared hTTStmt As SQLHANDLE

        ''' <summary>
        ''' This method closes the internal (private) cursor object by freeing the ODBC 
        ''' connection and statement handles. 
        ''' </summary>
        ''' <paramname=""></param> 
        Public Shared Sub TtSiteClose()
            If TpRunTsip.TtDynSite.hTTStmt <> SQLHANDLE.Zero Then
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TtDynSite.hTTStmt)
                TpRunTsip.TtDynSite.hTTStmt = SQLHANDLE.Zero
                Ssutil.DisConn(TpRunTsip.TtDynSite.hTTConn)
                TpRunTsip.TtDynSite.hTTConn = SQLHANDLE.Zero
            End If
            If TpRunTsip.TtDynSite.hStmtInsert <> SQLHANDLE.Zero Then
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TtDynSite.hStmtInsert)
                TpRunTsip.TtDynSite.hStmtInsert = SQLHANDLE.Zero
                Ssutil.DisConn(TpRunTsip.TtDynSite.hConn)
                TpRunTsip.TtDynSite.hConn = SQLHANDLE.Zero
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
        Public Shared Function TtPrepareSite(table As String) As Integer
            Dim insert_buf As String
            Dim sqlRet As SQLRETURN

            insert_buf = SQLCHARPTR.Format("insert into {0} (interferer, intcall1, intcall2, viccall1, viccall2, intname1, intname2, vicname1, vicname2, intoper, intoper2, vicoper, vicoper2, intlatit, intlongit, intgrnd, viclatit, viclongit, vicgrnd, report, caseno, subcases, int1int2dist, vic1vic2dist, int1vic1dist, distadv, intoffax, vicoffax, intvicaz, vicintaz, processed) values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", table)

            TpRunTsip.TtDynSite.hConn = Ssutil.NewConn()

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TtDynSite.hConn, TpRunTsip.TtDynSite.hStmtInsert)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynSite.TtPrepareSite(): ERROR: call to SQLAllocHandle() failed.")
                Return [Error].ODBC_SQLALLOCHANDLE_FAILED
            End If

            sqlRet = ODBC.SQLPrepare(TpRunTsip.TtDynSite.hStmtInsert, insert_buf, insert_buf.Length)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynSite.TtPrepareSite(): ERROR: call to SQLPrepare() failed:" & Microsoft.VisualBasic.Constants.vbLf & insert_buf)
                Ssutil.DbGetDiagStmt(TpRunTsip.TtDynSite.hStmtInsert, "ttPrepareSite03: Failure to Prepare:-" & Microsoft.VisualBasic.Constants.vbLf & insert_buf)
                Return [Error].ODBC_PREPARE_FAILED
            End If

            ' AH: REMOVE when possible.
            ' This passes hConn and hStmy handles instantiated in C# over to the 
            ' associated C/C++ native code.
#If PINVOKE
            set_ttDynSite_ConnStmtHandles(hConn, hStmtInsert);
#End If

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method calls ODBC.SQLExecute() to perform insertion of a record
        ''' into a DB table by using the internal (private) ODBC statement handle
        ''' that was prepared on by a previous call to ODBC.SQLPrepare().
        ''' </summary>
        ''' <paramname="ttSite"> - TtSite object (record) to be inserted into the DB table.</param>
        ''' <paramname="nullInd"> - array of ODBC nullInds associated with ttSite.</param>
        ''' <returns></returns>
        Public Shared Function TtInsertSite(ttSite As TtSite, nullInd As SQLLEN()) As Integer
            Dim sqlRet As SQLRETURN = Constant.FAILURE

            ' Check that the static hStmt has been instanciated (by a previous call to TtPrepareSite()).
            If TpRunTsip.TtDynSite.hStmtInsert Is Nothing Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynSite.TtInsertSite(): ERROR: hStmt is null.")
                Return [Error].ODBC_NULL_HANDLE
            End If

            'The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            'to contain the values to bind to. This neccessitates copying the values of the nullInd
            'array elements into global memory with an SQLLENPTR pointer assigned to each one.
            Dim nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd)

            'The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            'to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            'into global memory with an SQLPOINTER pointer assigned to each one. FtAnte provides
            'a convenience method that does exactly this.
            Dim parameterValuePtr As SQLPOINTER() = ttSite.CopyToArrayOfSQLPOINTERs()

            Try
                ' Initialize auto-column numbering.
                Ssutil.DbStartBinds()

                Ssutil.DbBindStringInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "interferer", parameterValuePtr(_DataStructures.TtSite.INTERFERER), TtSite.INTERFERER_SZ, nullIndPtr(_DataStructures.TtSite.INTERFERER))
                Ssutil.DbBindStringInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "intcall1", parameterValuePtr(_DataStructures.TtSite.INTCALL1), TtSite.INTCALL1_SZ, nullIndPtr(_DataStructures.TtSite.INTCALL1))
                Ssutil.DbBindStringInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "intcall2", parameterValuePtr(_DataStructures.TtSite.INTCALL2), TtSite.INTCALL2_SZ, nullIndPtr(_DataStructures.TtSite.INTCALL2))
                Ssutil.DbBindStringInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "viccall1", parameterValuePtr(_DataStructures.TtSite.VICCALL1), TtSite.VICCALL1_SZ, nullIndPtr(_DataStructures.TtSite.VICCALL1))
                Ssutil.DbBindStringInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "viccall2", parameterValuePtr(_DataStructures.TtSite.VICCALL2), TtSite.VICCALL2_SZ, nullIndPtr(_DataStructures.TtSite.VICCALL2))
                Ssutil.DbBindStringInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "intname1", parameterValuePtr(_DataStructures.TtSite.INTNAME1), TtSite.INTNAME1_SZ, nullIndPtr(_DataStructures.TtSite.INTNAME1))
                Ssutil.DbBindStringInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "intname2", parameterValuePtr(_DataStructures.TtSite.INTNAME2), TtSite.INTNAME2_SZ, nullIndPtr(_DataStructures.TtSite.INTNAME2))
                Ssutil.DbBindStringInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "vicname1", parameterValuePtr(_DataStructures.TtSite.VICNAME1), TtSite.VICNAME1_SZ, nullIndPtr(_DataStructures.TtSite.VICNAME1))
                Ssutil.DbBindStringInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "vicname2", parameterValuePtr(_DataStructures.TtSite.VICNAME2), TtSite.VICNAME2_SZ, nullIndPtr(_DataStructures.TtSite.VICNAME2))
                Ssutil.DbBindStringInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "intoper", parameterValuePtr(_DataStructures.TtSite.INTOPER), TtSite.INTOPER_SZ, nullIndPtr(_DataStructures.TtSite.INTOPER))
                Ssutil.DbBindStringInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "intoper2", parameterValuePtr(_DataStructures.TtSite.INTOPER2), TtSite.INTOPER2_SZ, nullIndPtr(_DataStructures.TtSite.INTOPER2))
                Ssutil.DbBindStringInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "vicoper", parameterValuePtr(_DataStructures.TtSite.VICOPER), TtSite.VICOPER_SZ, nullIndPtr(_DataStructures.TtSite.VICOPER))
                Ssutil.DbBindStringInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "vicoper2", parameterValuePtr(_DataStructures.TtSite.VICOPER2), TtSite.VICOPER2_SZ, nullIndPtr(_DataStructures.TtSite.VICOPER2))
                Ssutil.DbBindIntInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "intlatit", parameterValuePtr(_DataStructures.TtSite.INTLATIT), nullIndPtr(_DataStructures.TtSite.INTLATIT))
                Ssutil.DbBindIntInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "intlongit", parameterValuePtr(_DataStructures.TtSite.INTLONGIT), nullIndPtr(_DataStructures.TtSite.INTLONGIT))
                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "intgrnd", parameterValuePtr(_DataStructures.TtSite.INTGRND), nullIndPtr(_DataStructures.TtSite.INTGRND))
                Ssutil.DbBindIntInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "viclatit", parameterValuePtr(_DataStructures.TtSite.VICLATIT), nullIndPtr(_DataStructures.TtSite.VICLATIT))
                Ssutil.DbBindIntInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "viclongit", parameterValuePtr(_DataStructures.TtSite.VICLONGIT), nullIndPtr(_DataStructures.TtSite.VICLONGIT))
                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "vicgrnd", parameterValuePtr(_DataStructures.TtSite.VICGRND), nullIndPtr(_DataStructures.TtSite.VICGRND))
                Ssutil.DbBindShortInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "report", parameterValuePtr(_DataStructures.TtSite.REPORT), nullIndPtr(_DataStructures.TtSite.REPORT))
                Ssutil.DbBindIntInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "caseno", parameterValuePtr(_DataStructures.TtSite.CASENO), nullIndPtr(_DataStructures.TtSite.CASENO))
                Ssutil.DbBindIntInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "subcases", parameterValuePtr(_DataStructures.TtSite.SUBCASES), nullIndPtr(_DataStructures.TtSite.SUBCASES))
                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "int1int2dist", parameterValuePtr(_DataStructures.TtSite.INT1INT2DIST), nullIndPtr(_DataStructures.TtSite.INT1INT2DIST))
                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "vic1vic2dist", parameterValuePtr(_DataStructures.TtSite.VIC1VIC2DIST), nullIndPtr(_DataStructures.TtSite.VIC1VIC2DIST))
                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "int1vic1dist", parameterValuePtr(_DataStructures.TtSite.INT1VIC1DIST), nullIndPtr(_DataStructures.TtSite.INT1VIC1DIST))
                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "distadv", parameterValuePtr(_DataStructures.TtSite.DISTADV), nullIndPtr(_DataStructures.TtSite.DISTADV))
                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "intoffax", parameterValuePtr(_DataStructures.TtSite.INTOFFAX), nullIndPtr(_DataStructures.TtSite.INTOFFAX))
                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "vicoffax", parameterValuePtr(_DataStructures.TtSite.VICOFFAX), nullIndPtr(_DataStructures.TtSite.VICOFFAX))
                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "intvicaz", parameterValuePtr(_DataStructures.TtSite.INTVICAZ), nullIndPtr(_DataStructures.TtSite.INTVICAZ))
                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "vicintaz", parameterValuePtr(_DataStructures.TtSite.VICINTAZ), nullIndPtr(_DataStructures.TtSite.VICINTAZ))
                Ssutil.DbBindIntInput(TpRunTsip.TtDynSite.hStmtInsert, 0, "processed", parameterValuePtr(_DataStructures.TtSite.PROCESSED), nullIndPtr(_DataStructures.TtSite.PROCESSED))
            Catch e As Exception
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynSite.TtInsertSite(): ERROR: ODBC parameter binding attempt failed: " & e.Message)
                Ssutil.DbGetDiagStmt(TpRunTsip.TtDynSite.hStmtInsert, "ttInsertSite02: Problem binding parameter: " & e.Message)
                Return [Error].ODBC_BINDING_FAILED
            End Try

            sqlRet = ODBC.SQLExecute(TpRunTsip.TtDynSite.hStmtInsert)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynSite.TtInsertSite(): ERROR: ODBC SQLExecute() failed.")
                Ssutil.DbGetDiagStmt(TpRunTsip.TtDynSite.hStmtInsert, "ttInsertSite03: Failure to Execute.")
                Return [Error].ODBC_EXECUTE_FAILED
            End If

            Return Constant.SUCCESS
        End Function






    End Class
End Namespace
