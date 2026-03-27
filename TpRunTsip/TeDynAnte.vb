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
    ''' records into the database table <b>&lt;userID&gt;.te_&lt;tableName&gt;_&lt;runID&gt;_ante</b>
    ''' </summary>
    ''' <remarks>
    ''' <listtype="bullet">
    ''' <item> The methods in this class are used to perform operations on the
    ''' te ANTE table. The actual ANTE table name is a variable, prescribed by the caller.</item>
    ''' <item>The supported operations are: TePrepareAnte, TeInsertAnte and TeAnteClose.</item>
    ''' <item>The caller must first call the method TtPrepareAnte() that creates a cursor 
    ''' for use with a subsequent call to TeInsertAnte().</item>
    ''' <item>The method closeSite must be called to release internal cursor resources (a cursor 
    ''' encapsulates an ODBC connection and statement handle et. al.).</item>
    ''' </list>
    ''' </remarks>
    Public Class TeDynAnte
        <DllImport("tpRunTsip.dll", CharSet:=CharSet.Ansi)>
        Private Shared Function tePrepareAnteMethod(
        <[In]> table As String) As Integer
        End Function

        '--------------------------------------------------------------------

        Public Shared hTEAStmt As SQLHANDLE
        Public Shared hTEAConn As SQLHDBC = SQLHDBC.Zero
        Public Shared cAnteTable As String

        ''' <summary>
        ''' This method calls ODBC.SQLPrepare() to prepare a table insert on an internal (private) statement 
        ''' handle that can then be used in a subsequent call to ODBC.SQLExecute to
        ''' actually perform the SQL record insertion.
        ''' </summary>
        ''' <paramname="table"> - full name of the DB table into which the record is to be inserted.</param>
        ''' <returns></returns>
        Public Shared Function TePrepareAnte(table As String) As Integer
            Dim insert_buf As String

            Dim sqlRet As SQLRETURN

            TpRunTsip.TeDynAnte.cAnteTable = table

            ' For a hybrid build, we need to set the static variable cAnteTable
            ' in native file teDynAnte.cpp.
#If PINVOKE
            tePrepareAnte(table);
#End If

            insert_buf = SQLCHARPTR.Format("insert into {0} (interferer, terrcall1, terrcall2, terrbndcde, terranum, earthlocation, earthcall1, earthband, terracode, earthacode, satname, satoper, satlongit, txpre, txtro, rxpre, rxtro, sarc1, sarc2, intause, mode1, mode2, etreport, tereport, etsubcaseno, tesubcaseno, esazim, eselev, teelev, etelev, tuelev, utelev, euelev, ediscang, tdiscang, adisc_set, adisc_ute, terrht, earthht, tvazim, evazim, tvelev, evelev, tvdistes, tvdisttu, evdistes, evdisttu, angleutv, anglesev, tsoffaxis, tstrueaz, tstrueel, angleute, angleuta, angleeta, angleatv, adisc_atv, terragain, terramodel, terraxref, earthagain, earthamodel, earthaxref, processed) values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", table)

            TpRunTsip.TeDynAnte.hTEAConn = Ssutil.NewConn()

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TeDynAnte.hTEAConn, TpRunTsip.TeDynAnte.hTEAStmt)

            sqlRet = ODBC.SQLPrepare(TpRunTsip.TeDynAnte.hTEAStmt, insert_buf, insert_buf.Length)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeDynAnte.TePrepareAnte(): ERROR: SQLPrepare() failed.")

                Ssutil.DbGetDiagStmt(TpRunTsip.TeDynAnte.hTEAStmt, "tePrepareAnte01: Could not prepare TE antenna:-")

                Return [Error].ODBC_PREPARE_FAILED
            End If

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method closes the internal (private) cursor object by freeing the ODBC 
        ''' connection and statement handles. 
        ''' </summary>
        ''' <paramname=""></param> 
        Public Shared Function TeCloseAnte() As Integer
            If TpRunTsip.TeDynAnte.hTEAConn IsNot Nothing Then
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TeDynAnte.hTEAStmt)

                TpRunTsip.TeDynAnte.hTEAStmt = SQLHANDLE.Zero

                Ssutil.DisConn(TpRunTsip.TeDynAnte.hTEAConn)

                TpRunTsip.TeDynAnte.hTEAConn = SQLHDBC.Zero
            End If
            Return 0
        End Function

        ''' <summary>
        ''' This method calls ODBC.SQLExecute() to perform insertion of a record
        ''' into a DB table by using the internal (private) ODBC statement handle
        ''' that was prepared on by a previous call to ODBC.SQLPrepare().
        ''' </summary>
        ''' <paramname="teAnte"> - TeAnte object (record) to be inserted into the DB table.</param>
        ''' <paramname="nullInd"> - array of ODBC nullInds associated with teAnte.</param>
        ''' <returns></returns>
        Public Shared Function TeInsertAnte(teAnte As TeAnte, nullInd As SQLLEN()) As Integer
            '&&Console.Error.Write("\nteDynAnte.teInsertAnte(): insertStruct.esazim = {0}", insertStruct.esazim);
            Dim sqlRet As SQLRETURN

            If TpRunTsip.TeDynAnte.hTEAConn Is Nothing Then
                ' Reprepare because of commits 
                TpRunTsip.TeDynAnte.TePrepareAnte(TpRunTsip.TeDynAnte.cAnteTable)
            End If


            'The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            'to contain the values to bind to. This neccessitates copying the values of the nullInd
            'array elements into global memory with an SQLLENPTR pointer assigned to each one.
            Dim nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd)

            'The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            'to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            'into global memory with an SQLPOINTER pointer assigned to each one. FtAnte provides
            'a convenience method that does exactly this.
            Dim parameterValuePtr As SQLPOINTER() = teAnte.CopyToArrayOfSQLPOINTERs()

            Try
                ' Initialize auto-indexing.
                Ssutil.DbStartBinds()

                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "interferer", parameterValuePtr(_DataStructures.TeAnte.INTERFERER), TeAnte.INTERFERER_SZ, nullIndPtr(_DataStructures.TeAnte.INTERFERER))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "terrcall1", parameterValuePtr(_DataStructures.TeAnte.TERRCALL1), TeAnte.TERRCALL1_SZ, nullIndPtr(_DataStructures.TeAnte.TERRCALL1))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "terrcall2", parameterValuePtr(_DataStructures.TeAnte.TERRCALL2), TeAnte.TERRCALL2_SZ, nullIndPtr(_DataStructures.TeAnte.TERRCALL2))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "terrbndcde", parameterValuePtr(_DataStructures.TeAnte.TERRBNDCDE), TeAnte.TERRBNDCDE_SZ, nullIndPtr(_DataStructures.TeAnte.TERRBNDCDE))
                Ssutil.DbBindShortInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "terranum", parameterValuePtr(_DataStructures.TeAnte.TERRANUM), nullIndPtr(_DataStructures.TeAnte.TERRANUM))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "earthlocation", parameterValuePtr(_DataStructures.TeAnte.EARTHLOCATION), TeAnte.EARTHLOCATION_SZ, nullIndPtr(_DataStructures.TeAnte.EARTHLOCATION))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "earthcall1", parameterValuePtr(_DataStructures.TeAnte.EARTHCALL1), TeAnte.EARTHCALL1_SZ, nullIndPtr(_DataStructures.TeAnte.EARTHCALL1))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "earthband", parameterValuePtr(_DataStructures.TeAnte.EARTHBAND), TeAnte.EARTHBAND_SZ, nullIndPtr(_DataStructures.TeAnte.EARTHBAND))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "terracode", parameterValuePtr(_DataStructures.TeAnte.TERRACODE), TeAnte.TERRACODE_SZ, nullIndPtr(_DataStructures.TeAnte.TERRACODE))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "earthacode", parameterValuePtr(_DataStructures.TeAnte.EARTHACODE), TeAnte.EARTHACODE_SZ, nullIndPtr(_DataStructures.TeAnte.EARTHACODE))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "satname", parameterValuePtr(_DataStructures.TeAnte.SATNAME), TeAnte.SATNAME_SZ, nullIndPtr(_DataStructures.TeAnte.SATNAME))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "satoper", parameterValuePtr(_DataStructures.TeAnte.SATOPER), TeAnte.SATOPER_SZ, nullIndPtr(_DataStructures.TeAnte.SATOPER))
                Ssutil.DbBindIntInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "satlongit", parameterValuePtr(_DataStructures.TeAnte.SATLONGIT), nullIndPtr(_DataStructures.TeAnte.SATLONGIT))
                Ssutil.DbBindFloatInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "txpre", parameterValuePtr(_DataStructures.TeAnte.TXPRE), nullIndPtr(_DataStructures.TeAnte.TXPRE))
                Ssutil.DbBindFloatInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "txtro", parameterValuePtr(_DataStructures.TeAnte.TXTRO), nullIndPtr(_DataStructures.TeAnte.TXTRO))
                Ssutil.DbBindFloatInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "rxpre", parameterValuePtr(_DataStructures.TeAnte.RXPRE), nullIndPtr(_DataStructures.TeAnte.RXPRE))
                Ssutil.DbBindFloatInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "rxtro", parameterValuePtr(_DataStructures.TeAnte.RXTRO), nullIndPtr(_DataStructures.TeAnte.RXTRO))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "sarc1", parameterValuePtr(_DataStructures.TeAnte.SARC1), nullIndPtr(_DataStructures.TeAnte.SARC1))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "sarc2", parameterValuePtr(_DataStructures.TeAnte.SARC2), nullIndPtr(_DataStructures.TeAnte.SARC2))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "intause", parameterValuePtr(_DataStructures.TeAnte.INTAUSE), TeAnte.INTAUSE_SZ, nullIndPtr(_DataStructures.TeAnte.INTAUSE))
                Ssutil.DbBindShortInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "mode1", parameterValuePtr(_DataStructures.TeAnte.MODE1), nullIndPtr(_DataStructures.TeAnte.MODE1))
                Ssutil.DbBindShortInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "mode2", parameterValuePtr(_DataStructures.TeAnte.MODE2), nullIndPtr(_DataStructures.TeAnte.MODE2))
                Ssutil.DbBindShortInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "etreport", parameterValuePtr(_DataStructures.TeAnte.ETREPORT), nullIndPtr(_DataStructures.TeAnte.ETREPORT))
                Ssutil.DbBindShortInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "tereport", parameterValuePtr(_DataStructures.TeAnte.TEREPORT), nullIndPtr(_DataStructures.TeAnte.TEREPORT))
                Ssutil.DbBindIntInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "etsubcaseno", parameterValuePtr(_DataStructures.TeAnte.ETSUBCASENO), nullIndPtr(_DataStructures.TeAnte.ETSUBCASENO))
                Ssutil.DbBindIntInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "tesubcaseno", parameterValuePtr(_DataStructures.TeAnte.TESUBCASENO), nullIndPtr(_DataStructures.TeAnte.TESUBCASENO))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "esazim", parameterValuePtr(_DataStructures.TeAnte.ESAZIM), nullIndPtr(_DataStructures.TeAnte.ESAZIM))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "eselev", parameterValuePtr(_DataStructures.TeAnte.ESELEV), nullIndPtr(_DataStructures.TeAnte.ESELEV))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "teelev", parameterValuePtr(_DataStructures.TeAnte.TEELEV), nullIndPtr(_DataStructures.TeAnte.TEELEV))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "etelev", parameterValuePtr(_DataStructures.TeAnte.ETELEV), nullIndPtr(_DataStructures.TeAnte.ETELEV))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "tuelev", parameterValuePtr(_DataStructures.TeAnte.TUELEV), nullIndPtr(_DataStructures.TeAnte.TUELEV))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "utelev", parameterValuePtr(_DataStructures.TeAnte.UTELEV), nullIndPtr(_DataStructures.TeAnte.UTELEV))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "euelev", parameterValuePtr(_DataStructures.TeAnte.EUELEV), nullIndPtr(_DataStructures.TeAnte.EUELEV))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "ediscang", parameterValuePtr(_DataStructures.TeAnte.EDISCANG), nullIndPtr(_DataStructures.TeAnte.EDISCANG))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "tdiscang", parameterValuePtr(_DataStructures.TeAnte.TDISCANG), nullIndPtr(_DataStructures.TeAnte.TDISCANG))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "adisc_set", parameterValuePtr(_DataStructures.TeAnte.ADISC_SET), nullIndPtr(_DataStructures.TeAnte.ADISC_SET))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "adisc_ute", parameterValuePtr(_DataStructures.TeAnte.ADISC_UTE), nullIndPtr(_DataStructures.TeAnte.ADISC_UTE))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "terrht", parameterValuePtr(_DataStructures.TeAnte.TERRHT), nullIndPtr(_DataStructures.TeAnte.TERRHT))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "earthht", parameterValuePtr(_DataStructures.TeAnte.EARTHHT), nullIndPtr(_DataStructures.TeAnte.EARTHHT))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "tvazim", parameterValuePtr(_DataStructures.TeAnte.TVAZIM), nullIndPtr(_DataStructures.TeAnte.TVAZIM))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "evazim", parameterValuePtr(_DataStructures.TeAnte.EVAZIM), nullIndPtr(_DataStructures.TeAnte.EVAZIM))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "tvelev", parameterValuePtr(_DataStructures.TeAnte.TVELEV), nullIndPtr(_DataStructures.TeAnte.TVELEV))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "evelev", parameterValuePtr(_DataStructures.TeAnte.EVELEV), nullIndPtr(_DataStructures.TeAnte.EVELEV))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "tvdistes", parameterValuePtr(_DataStructures.TeAnte.TVDISTES), nullIndPtr(_DataStructures.TeAnte.TVDISTES))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "tvdisttu", parameterValuePtr(_DataStructures.TeAnte.TVDISTTU), nullIndPtr(_DataStructures.TeAnte.TVDISTTU))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "evdistes", parameterValuePtr(_DataStructures.TeAnte.EVDISTES), nullIndPtr(_DataStructures.TeAnte.EVDISTES))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "evdisttu", parameterValuePtr(_DataStructures.TeAnte.EVDISTTU), nullIndPtr(_DataStructures.TeAnte.EVDISTTU))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "angleutv", parameterValuePtr(_DataStructures.TeAnte.ANGLEUTV), nullIndPtr(_DataStructures.TeAnte.ANGLEUTV))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "anglesev", parameterValuePtr(_DataStructures.TeAnte.ANGLESEV), nullIndPtr(_DataStructures.TeAnte.ANGLESEV))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "tsoffaxis", parameterValuePtr(_DataStructures.TeAnte.TSOFFAXIS), TeAnte.TSOFFAXIS_SZ, nullIndPtr(_DataStructures.TeAnte.TSOFFAXIS))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "tstrueaz", parameterValuePtr(_DataStructures.TeAnte.TSTRUEAZ), nullIndPtr(_DataStructures.TeAnte.TSTRUEAZ))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "tstrueel", parameterValuePtr(_DataStructures.TeAnte.TSTRUEEL), nullIndPtr(_DataStructures.TeAnte.TSTRUEEL))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "angleute", parameterValuePtr(_DataStructures.TeAnte.ANGLEUTE), nullIndPtr(_DataStructures.TeAnte.ANGLEUTE))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "angleuta", parameterValuePtr(_DataStructures.TeAnte.ANGLEUTA), nullIndPtr(_DataStructures.TeAnte.ANGLEUTA))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "angleeta", parameterValuePtr(_DataStructures.TeAnte.ANGLEETA), nullIndPtr(_DataStructures.TeAnte.ANGLEETA))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "angleatv", parameterValuePtr(_DataStructures.TeAnte.ANGLEATV), nullIndPtr(_DataStructures.TeAnte.ANGLEATV))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "adisc_atv", parameterValuePtr(_DataStructures.TeAnte.ADISC_ATV), nullIndPtr(_DataStructures.TeAnte.ADISC_ATV))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "terragain", parameterValuePtr(_DataStructures.TeAnte.TERRAGAIN), nullIndPtr(_DataStructures.TeAnte.TERRAGAIN))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "terramodel", parameterValuePtr(_DataStructures.TeAnte.TERRAMODEL), TeAnte.TERRAMODEL_SZ, nullIndPtr(_DataStructures.TeAnte.TERRAMODEL))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "terraxref", parameterValuePtr(_DataStructures.TeAnte.TERRAXREF), TeAnte.TERRAXREF_SZ, nullIndPtr(_DataStructures.TeAnte.TERRAXREF))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "earthagain", parameterValuePtr(_DataStructures.TeAnte.EARTHAGAIN), nullIndPtr(_DataStructures.TeAnte.EARTHAGAIN))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "earthamodel", parameterValuePtr(_DataStructures.TeAnte.EARTHAMODEL), TeAnte.EARTHAMODEL_SZ, nullIndPtr(_DataStructures.TeAnte.EARTHAMODEL))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "earthaxref", parameterValuePtr(_DataStructures.TeAnte.EARTHAXREF), TeAnte.EARTHAXREF_SZ, nullIndPtr(_DataStructures.TeAnte.EARTHAXREF))
                Ssutil.DbBindIntInput(TpRunTsip.TeDynAnte.hTEAStmt, 0, "processed", parameterValuePtr(_DataStructures.TeAnte.PROCESSED), nullIndPtr(_DataStructures.TeAnte.PROCESSED))
            Catch e As Exception
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeDynAnte.TeInsertAnte(): ERROR: ODBC Binding failed: " & e.Message)
                Ssutil.DbGetDiagStmt(TpRunTsip.TeDynAnte.hTEAStmt, "teInsertAnte01: Error binding field " & e.Message)
                Return [Error].ODBC_BINDING_FAILED
            End Try

            sqlRet = ODBC.SQLExecute(TpRunTsip.TeDynAnte.hTEAStmt)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeDynAnte.TeInsertAnte(): ERROR: SQLExecute() failed: ")
                Ssutil.DbGetDiagStmt(TpRunTsip.TeDynAnte.hTEAStmt, "teInsertAnte02: Execution error for TE Antenna:")
                Return [Error].ODBC_EXECUTE_FAILED
            End If

            Return Constant.SUCCESS
        End Function



    End Class
End Namespace
