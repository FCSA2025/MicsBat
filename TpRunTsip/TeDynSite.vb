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
    ''' records into the database table <b>&lt;userID&gt;.te_&lt;tableName&gt;_&lt;runID&gt;_site</b>
    ''' </summary>
    ''' <remarks>
    ''' <listtype="bullet">
    ''' <item> The methods in this class are used to perform operations on the
    ''' te SITE table. The actual SITE table name is a variable, prescribed by the caller.</item>
    ''' <item>The supported operations are: TePrepareSite, TeInsertSite and TeSiteClose.</item>
    ''' <item>The caller must first call the method TePrepareSite() that creates a cursor 
    ''' for use with a subsequent call to TeInsertSite().</item>
    ''' <item>The method TeCloseSite() must be called to release internal cursor resources (a cursor 
    ''' encapsulates an ODBC connection and statement handle et. al.).</item>
    ''' </list>
    ''' </remarks>
    Public Class TeDynSite
        <DllImport("tpRunTsip.dll", CharSet:=CharSet.Ansi)>
        Private Shared Function tePrepareSiteMethod(
        <[In]> table As String) As Integer
        End Function

        '------------------------------------------------------------------

        Private Shared mhTESStmt As SQLHANDLE
        Private Shared mhTESConn As SQLHDBC = SQLHDBC.Zero
        Private Shared mcSiteTable As String

        ''' <summary>
        ''' This method calls ODBC.SQLPrepare() to prepare a table insert on an internal (private) statement 
        ''' handle that can then be used in a subsequent call to ODBC.SQLExecute to
        ''' actually perform the SQL record insertion.
        ''' </summary>
        ''' <paramname="table"> - full name of the DB table into which the record is to be inserted.</param>
        ''' <returns></returns>
        Public Shared Function TePrepareSite(table As String) As Integer
            Dim insert_buf As String
            Dim sqlRet As SQLRETURN

            TpRunTsip.TeDynSite.mcSiteTable = table

            ' For a hybrid build, we need to set the static variable cSiteTable
            ' in native file teDynSite.cpp.
#If PINVOKE
            tePrepareSite(table);
#End If

            insert_buf = SQLCHARPTR.Format("insert into {0} (terrcall1, terrcall2, earthlocation, terrname1, terrname2, earthname, terroper, terroper2, earthoper, terrlatit,terrlongit, terrgrnd, earthlatit, earthlongit, earthgrnd, radiozone, rainzone, etreport, tereport, etcaseno, tecaseno, etsubcases, tesubcases, intreq, etdist, etazim, teazim, tudist, tuazim, utazim, eudist, euazim, ueazim, processed) values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", table)

            TpRunTsip.TeDynSite.mhTESConn = Ssutil.NewConn()

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TeDynSite.mhTESConn, TpRunTsip.TeDynSite.mhTESStmt)

            sqlRet = ODBC.SQLPrepare(TpRunTsip.TeDynSite.mhTESStmt, insert_buf, insert_buf.Length)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeDynSite.TePrepareSite(): ERROR: SQLPrepare() failed.")
                Ssutil.DbGetDiagStmt(TpRunTsip.TeDynSite.mhTESStmt, "tePrepareSite01: Could not prepare the te site.")

                Return [Error].ODBC_PREPARE_FAILED
            End If

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method closes the internal (private) cursor object by freeing the ODBC 
        ''' connection and statement handles. 
        ''' </summary>
        ''' <paramname=""></param> 
        Public Shared Function CloseTESite() As Integer
            If TpRunTsip.TeDynSite.mhTESConn IsNot Nothing Then
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, TpRunTsip.TeDynSite.mhTESStmt)

                TpRunTsip.TeDynSite.mhTESStmt = SQLHANDLE.Zero

                Ssutil.DisConn(TpRunTsip.TeDynSite.mhTESConn)

                TpRunTsip.TeDynSite.mhTESConn = SQLHDBC.Zero
            End If
            Return 0
        End Function

        ''' <summary>
        ''' <summary>
        ''' This method calls ODBC.SQLExecute() to perform insertion of a record
        ''' into a DB table by using the internal (private) ODBC statement handle
        ''' that was prepared on by a previous call to ODBC.SQLPrepare().
        ''' </summary>
        ''' <paramname="teSite"> - TeSite object (record) to be inserted into the DB table.</param>
        ''' <paramname="nullInds"> - array of ODBC nullInds associated with teSite.</param>
        ''' <returns></returns></summary>        Public Shared Function TeInsertSite(teSite As TeSite, nullInds As SQLLEN()) As Integer
            Dim sqlRet As SQLRETURN

            If TpRunTsip.TeDynSite.mhTESConn = SQLHDBC.Zero Then
                TpRunTsip.TeDynSite.TePrepareSite(TpRunTsip.TeDynSite.mcSiteTable)
            End If

            'The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            'to contain the values to bind to. This neccessitates copying the values of the nullInd
            'array elements into global memory with an SQLLENPTR pointer assigned to each one.
            Dim nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInds)

            'The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            'to contain the values to bind to. This necessitates copying the 'column' values of insertStruct
            'into global memory with an SQLPOINTER pointer assigned to each one. TeSite provides
            'a convenience method that does exactly this.

            Dim parameterValuePtr As SQLPOINTER() = teSite.CopyToArrayOfSQLPOINTERs()

            '	exec sql execute insert_site using...
            Try
                ' Resets the auto column index to zero.
                Ssutil.DbStartBinds()

                Ssutil.DbBindStringInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "terrcall1", parameterValuePtr(_DataStructures.TeSite.TERRCALL1), TeSite.TERRCALL1_SZ, nullIndPtr(_DataStructures.TeSite.TERRCALL1))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "terrcall2", parameterValuePtr(_DataStructures.TeSite.TERRCALL2), TeSite.TERRCALL2_SZ, nullIndPtr(_DataStructures.TeSite.TERRCALL2))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "earthlocation", parameterValuePtr(_DataStructures.TeSite.EARTHLOCATION), TeSite.EARTHLOCATION_SZ, nullIndPtr(_DataStructures.TeSite.EARTHLOCATION))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "terrname1", parameterValuePtr(_DataStructures.TeSite.TERRNAME1), TeSite.TERRNAME1_SZ, nullIndPtr(_DataStructures.TeSite.TERRNAME1))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "terrname2", parameterValuePtr(_DataStructures.TeSite.TERRNAME2), TeSite.TERRNAME2_SZ, nullIndPtr(_DataStructures.TeSite.TERRNAME2))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "earthname", parameterValuePtr(_DataStructures.TeSite.EARTHNAME), TeSite.EARTHNAME_SZ, nullIndPtr(_DataStructures.TeSite.EARTHNAME))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "terroper", parameterValuePtr(_DataStructures.TeSite.TERROPER), TeSite.TERROPER_SZ, nullIndPtr(_DataStructures.TeSite.TERROPER))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "terroper2", parameterValuePtr(_DataStructures.TeSite.TERROPER2), TeSite.TERROPER2_SZ, nullIndPtr(_DataStructures.TeSite.TERROPER2))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "earthoper", parameterValuePtr(_DataStructures.TeSite.EARTHOPER), TeSite.EARTHOPER_SZ, nullIndPtr(_DataStructures.TeSite.EARTHOPER))
                Ssutil.DbBindIntInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "terrlatit", parameterValuePtr(_DataStructures.TeSite.TERRLATIT), nullIndPtr(_DataStructures.TeSite.TERRLATIT))
                Ssutil.DbBindIntInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "terrlongit", parameterValuePtr(_DataStructures.TeSite.TERRLONGIT), nullIndPtr(_DataStructures.TeSite.TERRLONGIT))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "terrgrnd", parameterValuePtr(_DataStructures.TeSite.TERRGRND), nullIndPtr(_DataStructures.TeSite.TERRGRND))
                Ssutil.DbBindIntInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "earthlatit", parameterValuePtr(_DataStructures.TeSite.EARTHLATIT), nullIndPtr(_DataStructures.TeSite.EARTHLATIT))
                Ssutil.DbBindIntInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "earthlongit", parameterValuePtr(_DataStructures.TeSite.EARTHLONGIT), nullIndPtr(_DataStructures.TeSite.EARTHLONGIT))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "earthgrnd", parameterValuePtr(_DataStructures.TeSite.EARTHGRND), nullIndPtr(_DataStructures.TeSite.EARTHGRND))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "radiozone", parameterValuePtr(_DataStructures.TeSite.RADIOZONE), TeSite.RADIOZONE_SZ, nullIndPtr(_DataStructures.TeSite.RADIOZONE))
                Ssutil.DbBindShortInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "rainzone", parameterValuePtr(_DataStructures.TeSite.RAINZONE), nullIndPtr(_DataStructures.TeSite.RAINZONE))
                Ssutil.DbBindShortInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "etreport", parameterValuePtr(_DataStructures.TeSite.ETREPORT), nullIndPtr(_DataStructures.TeSite.ETREPORT))
                Ssutil.DbBindShortInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "tereport", parameterValuePtr(_DataStructures.TeSite.TEREPORT), nullIndPtr(_DataStructures.TeSite.TEREPORT))
                Ssutil.DbBindIntInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "etcaseno", parameterValuePtr(_DataStructures.TeSite.ETCASENO), nullIndPtr(_DataStructures.TeSite.ETCASENO))
                Ssutil.DbBindIntInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "tecaseno", parameterValuePtr(_DataStructures.TeSite.TECASENO), nullIndPtr(_DataStructures.TeSite.TECASENO))
                Ssutil.DbBindIntInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "etsubcases", parameterValuePtr(_DataStructures.TeSite.ETSUBCASES), nullIndPtr(_DataStructures.TeSite.ETSUBCASES))
                Ssutil.DbBindIntInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "tesubcases", parameterValuePtr(_DataStructures.TeSite.TESUBCASES), nullIndPtr(_DataStructures.TeSite.TESUBCASES))
                Ssutil.DbBindStringInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "intreq", parameterValuePtr(_DataStructures.TeSite.INTREQ), TeSite.INTREQ_SZ, nullIndPtr(_DataStructures.TeSite.INTREQ))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "etdist", parameterValuePtr(_DataStructures.TeSite.ETDIST), nullIndPtr(_DataStructures.TeSite.ETDIST))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "etazim", parameterValuePtr(_DataStructures.TeSite.ETAZIM), nullIndPtr(_DataStructures.TeSite.ETAZIM))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "teazim", parameterValuePtr(_DataStructures.TeSite.TEAZIM), nullIndPtr(_DataStructures.TeSite.TEAZIM))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "tudist", parameterValuePtr(_DataStructures.TeSite.TUDIST), nullIndPtr(_DataStructures.TeSite.TUDIST))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "tuazim", parameterValuePtr(_DataStructures.TeSite.TUAZIM), nullIndPtr(_DataStructures.TeSite.TUAZIM))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "utazim", parameterValuePtr(_DataStructures.TeSite.UTAZIM), nullIndPtr(_DataStructures.TeSite.UTAZIM))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "eudist", parameterValuePtr(_DataStructures.TeSite.EUDIST), nullIndPtr(_DataStructures.TeSite.EUDIST))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "euazim", parameterValuePtr(_DataStructures.TeSite.EUAZIM), nullIndPtr(_DataStructures.TeSite.EUAZIM))
                Ssutil.DbBindDoubleInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "ueazim", parameterValuePtr(_DataStructures.TeSite.UEAZIM), nullIndPtr(_DataStructures.TeSite.UEAZIM))
                Ssutil.DbBindIntInput(TpRunTsip.TeDynSite.mhTESStmt, 0, "processed", parameterValuePtr(_DataStructures.TeSite.PROCESSED), nullIndPtr(_DataStructures.TeSite.PROCESSED))
            Catch e As Exception
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeDynSite.TeInsertSite(): ERROR: ODBC 'Bind' attempt failed: " & e.Message)
                Ssutil.DbGetDiagStmt(TpRunTsip.TeDynSite.mhTESStmt, "teInsertSite02: Could not bind " & e.Message & " to insert site.")
                Return [Error].ODBC_BINDING_FAILED
            End Try

            sqlRet = ODBC.SQLExecute(TpRunTsip.TeDynSite.mhTESStmt)
            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeDynSite.TeInsertSite(): ERROR: SQLExecute() failed trying an insert.")
                Ssutil.DbGetDiagStmt(TpRunTsip.TeDynSite.mhTESStmt, "teInsertSite03: Could not execute the insert te site.")
                Return [Error].ODBC_EXECUTE_FAILED - 3
            End If

            Return Constant.SUCCESS
        End Function



    End Class
End Namespace
