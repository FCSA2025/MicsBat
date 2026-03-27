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
    ''' records into the database table <b>&lt;userID&gt;.tt_&lt;tableName&gt;_&lt;runID&gt;_ante</b>
    ''' </summary>
    ''' <remarks>
    ''' <listtype="bullet">
    ''' <item> The methods in this class are used to perform operations on the
    ''' tt ANTE table. The actual ANTE table name is a variable, prescribed by the caller.</item>
    ''' <item>The supported operations are: TtPrepareAnte, TtInsertAnte and TtAnteClose.</item>
    ''' <item>The caller must first call the method TtPrepareAnte() that creates a cursor 
    ''' for use with a subsequent call to TtInsertAnte().</item>
    ''' <item>The method closeSite must be called to release internal cursor resources (a cursor 
    ''' encapsulates an ODBC connection and statement handle et. al.).</item>
    ''' </list>
    ''' </remarks>public class TtDynAnte
    Public Class TtDynAnte
#If PINVOKE
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static void set_ttDynAnte_ConnStmtHandles([In] SQLHANDLE hConnection, [In] SQLHANDLE hStatement);
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
        Public Shared Sub TtCloseAnte()
            If TpRunTsip.TtDynAnte.hTTStmt <> SQLHANDLE.Zero Then
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TtDynAnte.hTTStmt)
                TpRunTsip.TtDynAnte.hTTStmt = SQLHANDLE.Zero
                Ssutil.DisConn(TpRunTsip.TtDynAnte.hTTConn)
                TpRunTsip.TtDynAnte.hTTConn = SQLHANDLE.Zero
            End If
            If TpRunTsip.TtDynAnte.hStmtInsert <> SQLHANDLE.Zero Then
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TtDynAnte.hStmtInsert)
                TpRunTsip.TtDynAnte.hStmtInsert = SQLHANDLE.Zero
                Ssutil.DisConn(TpRunTsip.TtDynAnte.hConn)
                TpRunTsip.TtDynAnte.hConn = SQLHANDLE.Zero
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
        Public Shared Function TtPrepareAnte(table As String) As Integer
            Dim insert_buf As String
            Dim sqlRet As SQLRETURN

            insert_buf = SQLCHARPTR.Format("insert into {0} (interferer, intcall1, intcall2, intbndcde, intanum, viccall1, viccall2, vicbndcde, vicanum, intacode, vicacode, report, subcaseno, adiscctxh, adiscctxv, adisccrxh, adisccrxv, adiscxtxh, adiscxtxv, adiscxrxh, adiscxrxv, processed,intause, vicause, intgain, vicgain, intaxref, intamodel, vicaxref, vicamodel, intaoffax, inthopaz, intantaz, intoffantax, vicaoffax," & Microsoft.VisualBasic.Constants.vbTab & "vichopaz," & Microsoft.VisualBasic.Constants.vbTab & "vicantaz," & Microsoft.VisualBasic.Constants.vbTab & "vicoffantax, intaht, vicaht, intvicel, vicintel, intelev, vicelev, caseno) values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", table)

            TpRunTsip.TtDynAnte.hConn = Ssutil.NewConn()

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TtDynAnte.hConn, TpRunTsip.TtDynAnte.hStmtInsert)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynAnte.TtPrepareAnte(): ERROR: call to SQLAllocHandle() failed.")
                Return [Error].ODBC_SQLALLOCHANDLE_FAILED
            End If

            sqlRet = ODBC.SQLPrepare(TpRunTsip.TtDynAnte.hStmtInsert, insert_buf, insert_buf.Length)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynAnte.TtPrepareAnte(): ERROR: call to SQLPrepare() failed.")
                Ssutil.DbGetDiagStmt(TpRunTsip.TtDynAnte.hStmtInsert, "ttPrepareAnte01: Error preparing: " & insert_buf)
                Return [Error].ODBC_PREPARE_FAILED
            End If

            ' AH: REMOVE when possible.
            ' This passes hConn and hStmy handles instantiated in C# over to the 
            ' associated C/C++ native code.
#If PINVOKE
            set_ttDynAnte_ConnStmtHandles(hConn, hStmtInsert);
#End If

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method calls ODBC.SQLExecute() to perform insertion of a record
        ''' into a DB table by using the internal (private) ODBC statement handle
        ''' that was prepared on by a previous call to ODBC.SQLPrepare().
        ''' </summary>
        ''' <paramname="insertStruct"> - TtAnte object (record) to be inserted into the DB table.</param>
        ''' <paramname="nullInd"> - array of ODBC nullInds associated with ttAnte.</param>
        ''' <returns></returns>
        Public Shared Function TtInsertAnte(insertStruct As TtAnte, nullInd As SQLLEN()) As Integer
            Dim sqlRet As SQLRETURN = Constant.FAILURE

            ' Check that the static hStmt has been instanciated (by a previous 
            ' call to TtPrepareSite()).
            If TpRunTsip.TtDynAnte.hStmtInsert Is Nothing Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynAnte.TtInsertAnte(): ERROR: hStmt is null.")
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
            Dim parameterValuePtr As SQLPOINTER() = insertStruct.CopyToArrayOfSQLPOINTERs()

            Try
                Ssutil.DbStartBinds()

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "interferer", parameterValuePtr(TtAnte.INTERFERER), TtAnte.INTERFERER_SZ, nullIndPtr(TtAnte.INTERFERER))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intcall1", parameterValuePtr(TtAnte.INTCALL1), TtAnte.INTCALL1_SZ, nullIndPtr(TtAnte.INTCALL1))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intcall2", parameterValuePtr(TtAnte.INTCALL2), TtAnte.INTCALL2_SZ, nullIndPtr(TtAnte.INTCALL2))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intbndcde", parameterValuePtr(TtAnte.INTBNDCDE), TtAnte.INTBNDCDE_SZ, nullIndPtr(TtAnte.INTBNDCDE))

                Ssutil.DbBindShortInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intanum", parameterValuePtr(TtAnte.INTANUM), nullIndPtr(TtAnte.INTANUM))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "viccall1", parameterValuePtr(TtAnte.VICCALL1), TtAnte.VICCALL1_SZ, nullIndPtr(TtAnte.VICCALL1))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "viccall2", parameterValuePtr(TtAnte.VICCALL2), TtAnte.VICCALL2_SZ, nullIndPtr(TtAnte.VICCALL2))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "vicbndcde", parameterValuePtr(TtAnte.VICBNDCDE), TtAnte.VICBNDCDE_SZ, nullIndPtr(TtAnte.VICBNDCDE))

                Ssutil.DbBindShortInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "vicanum", parameterValuePtr(TtAnte.VICANUM), nullIndPtr(TtAnte.VICANUM))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intacode", parameterValuePtr(TtAnte.INTACODE), TtAnte.INTACODE_SZ, nullIndPtr(TtAnte.INTACODE))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "vicacode", parameterValuePtr(TtAnte.VICACODE), TtAnte.VICACODE_SZ, nullIndPtr(TtAnte.VICACODE))

                Ssutil.DbBindShortInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "report", parameterValuePtr(TtAnte.REPORT), nullIndPtr(TtAnte.REPORT))

                Ssutil.DbBindIntInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "subcaseno", parameterValuePtr(TtAnte.SUBCASENO), nullIndPtr(TtAnte.SUBCASENO))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "adiscctxh", parameterValuePtr(TtAnte.ADISCCTXH), nullIndPtr(TtAnte.ADISCCTXH))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "adiscctxv", parameterValuePtr(TtAnte.ADISCCTXV), nullIndPtr(TtAnte.ADISCCTXV))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "adisccrxh", parameterValuePtr(TtAnte.ADISCCRXH), nullIndPtr(TtAnte.ADISCCRXH))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "adisccrxv", parameterValuePtr(TtAnte.ADISCCRXV), nullIndPtr(TtAnte.ADISCCRXV))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "adiscxtxh", parameterValuePtr(TtAnte.ADISCXTXH), nullIndPtr(TtAnte.ADISCXTXH))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "adiscxtxv", parameterValuePtr(TtAnte.ADISCXTXV), nullIndPtr(TtAnte.ADISCXTXV))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "adiscxrxh", parameterValuePtr(TtAnte.ADISCXRXH), nullIndPtr(TtAnte.ADISCXRXH))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "adiscxrxv", parameterValuePtr(TtAnte.ADISCXRXV), nullIndPtr(TtAnte.ADISCXRXV))

                Ssutil.DbBindIntInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "processed", parameterValuePtr(TtAnte.PROCESSED), nullIndPtr(TtAnte.PROCESSED))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intause", parameterValuePtr(TtAnte.INTAUSE), TtAnte.INTAUSE_SZ, nullIndPtr(TtAnte.INTAUSE))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "vicause", parameterValuePtr(TtAnte.VICAUSE), TtAnte.VICAUSE_SZ, nullIndPtr(TtAnte.VICAUSE))

                Ssutil.DbBindFloatInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intgain", parameterValuePtr(TtAnte.INTGAIN), nullIndPtr(TtAnte.INTGAIN))

                Ssutil.DbBindFloatInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "vicgain", parameterValuePtr(TtAnte.VICGAIN), nullIndPtr(TtAnte.VICGAIN))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intaxref", parameterValuePtr(TtAnte.INTAXREF), TtAnte.INTAXREF_SZ, nullIndPtr(TtAnte.INTAXREF))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intamodel", parameterValuePtr(TtAnte.INTAMODEL), TtAnte.INTAMODEL_SZ, nullIndPtr(TtAnte.INTAMODEL))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "vicaxref", parameterValuePtr(TtAnte.VICAXREF), TtAnte.VICAXREF_SZ, nullIndPtr(TtAnte.VICAXREF))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "vicamodel", parameterValuePtr(TtAnte.VICAMODEL), TtAnte.VICAMODEL_SZ, nullIndPtr(TtAnte.VICAMODEL))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intaoffax", parameterValuePtr(TtAnte.INTAOFFAX), TtAnte.INTAOFFAX_SZ, nullIndPtr(TtAnte.INTAOFFAX))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "inthopaz", parameterValuePtr(TtAnte.INTHOPAZ), nullIndPtr(TtAnte.INTHOPAZ))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intantaz", parameterValuePtr(TtAnte.INTANTAZ), nullIndPtr(TtAnte.INTANTAZ))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intoffantax", parameterValuePtr(TtAnte.INTOFFANTAX), nullIndPtr(TtAnte.INTOFFANTAX))

                Ssutil.DbBindStringInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "vicaoffax", parameterValuePtr(TtAnte.VICAOFFAX), TtAnte.VICAOFFAX_SZ, nullIndPtr(TtAnte.VICAOFFAX))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "vichopaz", parameterValuePtr(TtAnte.VICHOPAZ), nullIndPtr(TtAnte.VICHOPAZ))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "vicantaz", parameterValuePtr(TtAnte.VICANTAZ), nullIndPtr(TtAnte.VICANTAZ))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "vicoffantax", parameterValuePtr(TtAnte.VICOFFANTAX), nullIndPtr(TtAnte.VICOFFANTAX))

                Ssutil.DbBindFloatInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intaht", parameterValuePtr(TtAnte.INTAHT), nullIndPtr(TtAnte.INTAHT))

                Ssutil.DbBindFloatInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "vicaht", parameterValuePtr(TtAnte.VICAHT), nullIndPtr(TtAnte.VICAHT))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intvicel", parameterValuePtr(TtAnte.INTVICEL), nullIndPtr(TtAnte.INTVICEL))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "vicintel", parameterValuePtr(TtAnte.VICINTEL), nullIndPtr(TtAnte.VICINTEL))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "intelev", parameterValuePtr(TtAnte.INTELEV), nullIndPtr(TtAnte.INTELEV))

                Ssutil.DbBindDoubleInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "vicelev", parameterValuePtr(TtAnte.VICELEV), nullIndPtr(TtAnte.VICELEV))

                Ssutil.DbBindIntInput(TpRunTsip.TtDynAnte.hStmtInsert, 0, "caseno", parameterValuePtr(TtAnte.CASENO), nullIndPtr(TtAnte.CASENO))
            Catch e As Exception
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynAnte.TtInsertAnte(): ERROR: ODBC 'binding' attempt failed: " & e.Message)
                Ssutil.DbGetDiagStmt(TpRunTsip.TtDynAnte.hStmtInsert, "ttInsertAnte02: Problem binding parameter: " & e.Message)
                Return [Error].ODBC_BINDING_FAILED
            End Try

            sqlRet = ODBC.SQLExecute(TpRunTsip.TtDynAnte.hStmtInsert)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynAnte.TtInsertAnte(): ERROR: ODBC SQLExecute() failed: ")
                Ssutil.DbGetDiagStmt(TpRunTsip.TtDynAnte.hStmtInsert, "ttInsertAnte03: Error executing.")
                Return [Error].ODBC_EXECUTE_FAILED
            End If

            Return Constant.SUCCESS
        End Function


    End Class
End Namespace
