Imports System
Imports _Configuration
Imports _DataStructures
Imports _NewLib
Imports _Utillib
Imports System.Runtime.InteropServices
Imports SQLCHARPTR = System.String            'Invented to mimic (char *) for [In]  only.
Imports SQLHANDLE = System.IntPtr
Imports SQLHDBC = System.IntPtr
Imports SQLLEN = System.Int64
Imports SQLPOINTER = System.IntPtr
Imports SQLRETURN = System.Int16

Namespace TpRunTsip

    ''' <summary>
    ''' Provides methods to prepare and insert 
    ''' records into the database table <b>&lt;userID&gt;.te_&lt;tableName&gt;_&lt;runID&gt;_chan</b>
    ''' </summary>
    ''' <remarks>
    ''' <listtype="bullet">
    ''' <item> The methods in this class are used to perform operations on the
    ''' te CHAN table. The actual CHAN table name is a variable, prescribed by the caller.</item>
    ''' <item>The supported operations are: TePrepareChan, TeInsertChan and TeChanClose.</item>
    ''' <item>The caller must first call the method TePrepareChan() that creates a cursor 
    ''' for use with a subsequent call to TeInsertChan().</item>
    ''' <item>The method TeCloseChan() must be called to release internal cursor resources (a cursor 
    ''' encapsulates an ODBC connection and statement handle et. al.).</item>
    ''' </list>
    ''' </remarks>
    Public Class TeDynChan
        <DllImport("tpRunTsip.dll", CharSet:=CharSet.Ansi)>
        Private Shared Function tePrepareChanMethod(
        <[In]> table As String) As Integer
        End Function

        Public Shared hTECStmt As SQLHANDLE
        Public Shared hTECConn As SQLHDBC = SQLHDBC.Zero
        Public Shared cChanTable As String

        ''' <summary>
        ''' This method calls ODBC.SQLPrepare() to prepare a table insert on an internal (private) statement 
        ''' handle that can then be used in a subsequent call to ODBC.SQLExecute to
        ''' actually perform the SQL record insertion.
        ''' </summary>
        ''' <paramname="table"> - full name of the DB table into which the record is to be inserted.</param>
        ''' <returns></returns>
        Public Shared Function TePrepareChan(table As String) As Integer
            Dim sqlRet As SQLRETURN

            Dim insert_buf As String

            TpRunTsip.TeDynChan.cChanTable = table

            ' For a hybrid build, we need to set the static variable cChanTable
            ' in native file teDynChan.cpp.
#If PINVOKE
            tePrepareChan(table);
#End If

            insert_buf = SQLCHARPTR.Format("insert into {0} (interferer, terrcall1, terrcall2, terrbndcde, terranum, terrchid, earthlocation, earthcall1, earthchid, inttraftx, victrafrx, inteqpttx, viceqptrx, intfreqtx, inttxpwr, inttxpwr2, inttxafls, inttxafls2, vicrxafls, vicfreqrx, vicpwrrx, stattx, statrx, energy, etreport, tereport, ctxinttraftx, ctxvictrafrx, ctxeqpt, calctype, earthmdsc, terrmdsc, eartheirp, terreirp, freqsep, scang, loss20mode1, loss01mode1, loss01mode2, calci20mode1, calci01mode1, calci01mode2, reqd20mode1, reqd01mode1, reqd01mode2, marg20mode1, marg01mode1, marg01mode2, remterracode, remterragain, processed, terrant) values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", table)

            TpRunTsip.TeDynChan.hTECConn = Ssutil.NewConn()

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TeDynChan.hTECConn, TpRunTsip.TeDynChan.hTECStmt)

            sqlRet = ODBC.SQLPrepare(TpRunTsip.TeDynChan.hTECStmt, insert_buf, insert_buf.Length)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeDynChan.TePrepareChan(): ERROR: SQLPrepare() failed.")

                Ssutil.DbGetDiagStmt(TpRunTsip.TeDynChan.hTECStmt, "tePrepareChan01: Could not prepare es channel.")

                Return [Error].ODBC_PREPARE_FAILED
            End If

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method closes the internal (private) cursor object by freeing the ODBC 
        ''' connection and statement handles. 
        ''' </summary>
        ''' <paramname=""></param>
        Public Shared Function TeCloseChan() As Integer
            If TpRunTsip.TeDynChan.hTECConn IsNot Nothing Then
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TeDynChan.hTECStmt)

                TpRunTsip.TeDynChan.hTECStmt = SQLHANDLE.Zero

                Ssutil.DisConn(TpRunTsip.TeDynChan.hTECConn)

                TpRunTsip.TeDynChan.hTECConn = SQLHDBC.Zero
            End If
            Return 0
        End Function

        ''' <summary>
        ''' This method calls ODBC.SQLExecute() to perform insertion of a record
        ''' into a DB table by using the internal (private) ODBC statement handle
        ''' that was prepared on by a previous call to ODBC.SQLPrepare().
        ''' </summary>
        ''' <paramname="teChan"> - TeChan object (record) to be inserted into the DB table.</param>
        ''' <paramname="nullInds"> - array of ODBC nullInds associated with ttChan.</param>
        ''' <returns></returns>
        Public Shared Function TeInsertChan(teChan As TeChan, nullInds As SQLLEN()) As Integer
            Dim sqlRet As SQLRETURN

            ' The following 're-prepares the channel insert since we may have done
            '    a commit in the meantime 
            If TpRunTsip.TeDynChan.hTECConn Is Nothing Then
                TpRunTsip.TeDynChan.TePrepareChan(TpRunTsip.TeDynChan.cChanTable)
            End If

            'The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            'to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            'into global memory with an SQLPOINTER pointer assigned to each one. FtAnte provides
            'a convenience method that does exactly this.
            Dim parameterValuePtr As SQLPOINTER() = teChan.CopyToArrayOfSQLPOINTERs()

            'The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            'to contain the values to bind to. This neccessitates copying the values of the nullInd
            'array elements into global memory with an SQLLENPTR pointer assigned to each one.
            Dim nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInds)

            '	exec sql execute insert_chan using...
            Try
                ' Initialize auto-increment binding 'number'.
                Ssutil.DbStartBinds()

                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "interferer", parameterValuePtr(_DataStructures.TeChan.INTERFERER), TeChan.INTERFERER_SZ, nullIndPtr(_DataStructures.TeChan.INTERFERER))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "terrcall1", parameterValuePtr(_DataStructures.TeChan.TERRCALL1), TeChan.TERRCALL1_SZ, nullIndPtr(_DataStructures.TeChan.TERRCALL1))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "terrcall2", parameterValuePtr(_DataStructures.TeChan.TERRCALL2), TeChan.TERRCALL2_SZ, nullIndPtr(_DataStructures.TeChan.TERRCALL2))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "terrbndcde", parameterValuePtr(_DataStructures.TeChan.TERRBNDCDE), TeChan.TERRBNDCDE_SZ, nullIndPtr(_DataStructures.TeChan.TERRBNDCDE))
                Ssutil.DbBindShortInput(TpRunTsip.TeDynChan.hTECStmt, 0, "terranum", parameterValuePtr(_DataStructures.TeChan.TERRANUM), nullIndPtr(_DataStructures.TeChan.TERRANUM))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "terrchid", parameterValuePtr(_DataStructures.TeChan.TERRCHID), TeChan.TERRCHID_SZ, nullIndPtr(_DataStructures.TeChan.TERRCHID))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "earthlocation", parameterValuePtr(_DataStructures.TeChan.EARTHLOCATION), TeChan.EARTHLOCATION_SZ, nullIndPtr(_DataStructures.TeChan.EARTHLOCATION))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "earthcall1", parameterValuePtr(_DataStructures.TeChan.EARTHCALL1), TeChan.EARTHCALL1_SZ, nullIndPtr(_DataStructures.TeChan.EARTHCALL1))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "earthchid", parameterValuePtr(_DataStructures.TeChan.EARTHCHID), TeChan.EARTHCHID_SZ, nullIndPtr(_DataStructures.TeChan.EARTHCHID))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "inttraftx", parameterValuePtr(_DataStructures.TeChan.INTTRAFTX), TeChan.INTTRAFTX_SZ, nullIndPtr(_DataStructures.TeChan.INTTRAFTX))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "victrafrx", parameterValuePtr(_DataStructures.TeChan.VICTRAFRX), TeChan.VICTRAFRX_SZ, nullIndPtr(_DataStructures.TeChan.VICTRAFRX))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "inteqpttx", parameterValuePtr(_DataStructures.TeChan.INTEQPTTX), TeChan.INTEQPTTX_SZ, nullIndPtr(_DataStructures.TeChan.INTEQPTTX))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "viceqptrx", parameterValuePtr(_DataStructures.TeChan.VICEQPTRX), TeChan.VICEQPTRX_SZ, nullIndPtr(_DataStructures.TeChan.VICEQPTRX))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "intfreqtx", parameterValuePtr(_DataStructures.TeChan.INTFREQTX), nullIndPtr(_DataStructures.TeChan.INTFREQTX))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "inttxpwr", parameterValuePtr(_DataStructures.TeChan.INTTXPWR), nullIndPtr(_DataStructures.TeChan.INTTXPWR))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "inttxpwr2", parameterValuePtr(_DataStructures.TeChan.INTTXPWR2), nullIndPtr(_DataStructures.TeChan.INTTXPWR2))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "inttxafls", parameterValuePtr(_DataStructures.TeChan.INTTXAFLS), nullIndPtr(_DataStructures.TeChan.INTTXAFLS))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "inttxafls2", parameterValuePtr(_DataStructures.TeChan.INTTXAFLS2), nullIndPtr(_DataStructures.TeChan.INTTXAFLS2))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "vicrxafls", parameterValuePtr(_DataStructures.TeChan.VICRXAFLS), nullIndPtr(_DataStructures.TeChan.VICRXAFLS))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "vicfreqrx", parameterValuePtr(_DataStructures.TeChan.VICFREQRX), nullIndPtr(_DataStructures.TeChan.VICFREQRX))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "vicpwrrx", parameterValuePtr(_DataStructures.TeChan.VICPWRRX), nullIndPtr(_DataStructures.TeChan.VICPWRRX))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "stattx", parameterValuePtr(_DataStructures.TeChan.STATTX), TeChan.STATTX_SZ, nullIndPtr(_DataStructures.TeChan.STATTX))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "statrx", parameterValuePtr(_DataStructures.TeChan.STATRX), TeChan.STATRX_SZ, nullIndPtr(_DataStructures.TeChan.STATRX))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "energy", parameterValuePtr(_DataStructures.TeChan.ENERGY), nullIndPtr(_DataStructures.TeChan.ENERGY))

                ' calculated fields 
                Ssutil.DbBindShortInput(TpRunTsip.TeDynChan.hTECStmt, 0, "etreport", parameterValuePtr(_DataStructures.TeChan.ETREPORT), nullIndPtr(_DataStructures.TeChan.ETREPORT))
                Ssutil.DbBindShortInput(TpRunTsip.TeDynChan.hTECStmt, 0, "tereport", parameterValuePtr(_DataStructures.TeChan.TEREPORT), nullIndPtr(_DataStructures.TeChan.TEREPORT))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "ctxinttraftx", parameterValuePtr(_DataStructures.TeChan.CTXINTTRAFTX), TeChan.CTXINTTRAFTX_SZ, nullIndPtr(_DataStructures.TeChan.CTXINTTRAFTX))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "ctxvictrafrx", parameterValuePtr(_DataStructures.TeChan.CTXVICTRAFRX), TeChan.CTXVICTRAFRX_SZ, nullIndPtr(_DataStructures.TeChan.CTXVICTRAFRX))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "ctxeqpt", parameterValuePtr(_DataStructures.TeChan.CTXEQPT), TeChan.CTXEQPT_SZ, nullIndPtr(_DataStructures.TeChan.CTXEQPT))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "calctype", parameterValuePtr(_DataStructures.TeChan.CALCTYPE), TeChan.CALCTYPE_SZ, nullIndPtr(_DataStructures.TeChan.CALCTYPE))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "earthmdsc", parameterValuePtr(_DataStructures.TeChan.EARTHMDSC), nullIndPtr(_DataStructures.TeChan.EARTHMDSC))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "terrmdsc", parameterValuePtr(_DataStructures.TeChan.TERRMDSC), nullIndPtr(_DataStructures.TeChan.TERRMDSC))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "eartheirp", parameterValuePtr(_DataStructures.TeChan.EARTHEIRP), nullIndPtr(_DataStructures.TeChan.EARTHEIRP))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "terreirp", parameterValuePtr(_DataStructures.TeChan.TERREIRP), nullIndPtr(_DataStructures.TeChan.TERREIRP))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "freqsep", parameterValuePtr(_DataStructures.TeChan.FREQSEP), nullIndPtr(_DataStructures.TeChan.FREQSEP))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "scang", parameterValuePtr(_DataStructures.TeChan.SCANG), nullIndPtr(_DataStructures.TeChan.SCANG))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "loss20mode1", parameterValuePtr(_DataStructures.TeChan.LOSS20MODE1), nullIndPtr(_DataStructures.TeChan.LOSS20MODE1))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "loss01mode1", parameterValuePtr(_DataStructures.TeChan.LOSS01MODE1), nullIndPtr(_DataStructures.TeChan.LOSS01MODE1))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "loss01mode2", parameterValuePtr(_DataStructures.TeChan.LOSS01MODE2), nullIndPtr(_DataStructures.TeChan.LOSS01MODE2))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "calci20mode1", parameterValuePtr(_DataStructures.TeChan.CALCI20MODE1), nullIndPtr(_DataStructures.TeChan.CALCI20MODE1))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "calci01mode1", parameterValuePtr(_DataStructures.TeChan.CALCI01MODE1), nullIndPtr(_DataStructures.TeChan.CALCI01MODE1))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "calci01mode2", parameterValuePtr(_DataStructures.TeChan.CALCI01MODE2), nullIndPtr(_DataStructures.TeChan.CALCI01MODE2))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "reqd20mode1", parameterValuePtr(_DataStructures.TeChan.REQD20MODE1), nullIndPtr(_DataStructures.TeChan.REQD20MODE1))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "reqd01mode1", parameterValuePtr(_DataStructures.TeChan.REQD01MODE1), nullIndPtr(_DataStructures.TeChan.REQD01MODE1))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "reqd01mode2", parameterValuePtr(_DataStructures.TeChan.REQD01MODE2), nullIndPtr(_DataStructures.TeChan.REQD01MODE2))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "marg20mode1", parameterValuePtr(_DataStructures.TeChan.MARG20MODE1), nullIndPtr(_DataStructures.TeChan.MARG20MODE1))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "marg01mode1", parameterValuePtr(_DataStructures.TeChan.MARG01MODE1), nullIndPtr(_DataStructures.TeChan.MARG01MODE1))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "marg01mode2", parameterValuePtr(_DataStructures.TeChan.MARG01MODE2), nullIndPtr(_DataStructures.TeChan.MARG01MODE2))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynChan.hTECStmt, 0, "remterracode", parameterValuePtr(_DataStructures.TeChan.REMTERRACODE), TeChan.REMTERRACODE_SZ, nullIndPtr(_DataStructures.TeChan.REMTERRACODE))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynChan.hTECStmt, 0, "remterragain", parameterValuePtr(_DataStructures.TeChan.REMTERRAGAIN), nullIndPtr(_DataStructures.TeChan.REMTERRAGAIN))
                Ssutil.DbBindIntInput(TpRunTsip.TeDynChan.hTECStmt, 0, "processed", parameterValuePtr(_DataStructures.TeChan.PROCESSED), nullIndPtr(_DataStructures.TeChan.PROCESSED))
                Ssutil.DbBindShortInput(TpRunTsip.TeDynChan.hTECStmt, 0, "terrant", parameterValuePtr(_DataStructures.TeChan.TERRANT), nullIndPtr(_DataStructures.TeChan.TERRANT))
            Catch e As Exception
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeDynChan.TeInsertChan(): ERROR: DbBind failed: " & e.Message)
                Ssutil.DbGetDiagStmt(TpRunTsip.TeDynChan.hTECStmt, "teInsertChan02: Could not bind field: " & e.Message)
                Return [Error].ODBC_BINDING_FAILED
            End Try

            sqlRet = ODBC.SQLExecute(TpRunTsip.TeDynChan.hTECStmt)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeDynChan.TeInsertChan(): ERROR: SQLExecute() failed, returned: " & sqlRet.ToString())
                Ssutil.DbGetDiagStmt(TpRunTsip.TeDynChan.hTECStmt, "teInsertChan03: Error executing.")
                Return [Error].ODBC_EXECUTE_FAILED
            End If

            Return Constant.SUCCESS
        End Function








    End Class
End Namespace
