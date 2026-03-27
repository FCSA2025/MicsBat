Imports System
Imports _Configuration
Imports _DataStructures
Imports _NewLib
Imports _Utillib
Imports SQLCHARPTR = System.String            'Invented to mimic (char *) for [In]  only.
Imports SQLHANDLE = System.IntPtr
Imports SQLHDBC = System.IntPtr
Imports SQLLEN = System.Int64
Imports SQLRETURN = System.Int16
Imports System.Runtime.InteropServices

Namespace TpRunTsip

    ''' <summary>
    ''' Provides methods to set up an episode of fetching location data from 
    ''' any DB table that has a column field named 'location'.
    ''' </summary>
    ''' <remarks>
    ''' <listtype="bullet">
    ''' <item> The methods in this class are used to perform operations on any 
    ''' DB table that has a column field named 'location'.</item>
    ''' <item>The DB table name is prescribed by the caller.</item>
    ''' <item>The supported operations are: TeSelectTemp1, TeFetchTemp1 and TeCloseTemp1.</item>
    ''' <item>The caller first calls the method TeSelectTemp1() that returns a cursor 
    ''' for use with a subsequent call to TeFetchTemp1().</item>
    ''' <item>The method TeCloseTemp1 must be called to release internal cursor resources
    ''' (a cursor encapsulates an ODBC connection and statement handle.).</item>
    ''' </list>
    ''' </remarks>
    Public Class TeDynTemp1
        '===================================================================================

        Public Shared cursors As Cursor() = Arrays.CreateArrayUsingDefaultElementConstructor(Of Cursor)(Constant.NUM_CURSORS_FEW)
        Public Shared nNextFreeCursor As Integer = 0

        '====================================================================================

        ''' <summary>
        ''' This method returns the integer index of the next free (available) Cursor object;
        ''' this method is used by TeSelectTemp1() which returns the index of a 
        ''' fully-instantiated cursor to be used in subsequent calls to TeFetchTemp1().
        ''' </summary>
        ''' <remarks>
        '''  The User is responsible for instantiating fields of the cursor object 
        '''  (i.e. hConn, hStmt etc). The User is also responsible for setting the cursor field
        '''  cursorOpen to true before using it and then false to release it.
        ''' </remarks>
        ''' <returns></returns>
        ''' <para> - integer index of the next free Cursor object.</para>
        ''' <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        Public Shared Function GetNextFreeCursor() As Integer
            'Get next free cursor area.
            Dim curHandle = [Error].NO_CURSOR_AVAILABLE
            If TpRunTsip.TeDynTemp1.nNextFreeCursor >= Constant.NUM_CURSORS_FEW Then
                '	Search for a free cursors in the list
                For nInd = 0 To Constant.NUM_CURSORS_FEW - 1
                    If Not TpRunTsip.TeDynTemp1.cursors(nInd).cursorOpen Then
                        curHandle = nInd
                        Exit For
                    End If
                Next
                If curHandle = -1 Then
                    ' There are no more open cursors
                    'GenUtil.SetErr("dynSite01 -- No more open cursors.");
                    'return -1;
                    Qutils.ExitQueue(Info.DbName, "READ")
                    Application.Exit(Microsoft.VisualBasic.Constants.vbCrLf & "DynAntenna.GetNextFreeCursor(): ERROR: No available cursors.")
                End If
            Else
                '	Just use the next available cursor
                curHandle = TpRunTsip.TeDynTemp1.nNextFreeCursor
                '  And increment the free cursor count.
                TpRunTsip.TeDynTemp1.nNextFreeCursor += 1
            End If
            Return curHandle
        End Function

        ''' <summary>
        ''' This method closes an active cursor object by freeing the ODBC 
        ''' connection and statement handles and setting the cursor field
        ''' cursorOpen to false. 
        ''' </summary>
        ''' <paramname="curHandle"> - cursor handle (index).</param>
        ''' <returns></returns>
        Public Shared Function TeCloseTemp1(curHandle As Integer) As Integer
            If curHandle >= Constant.NUM_CURSORS_FEW OrElse curHandle < 0 Then
                ' Bad handle 
                Return [Error].DYN_CUR_NOT_OPEN
            End If

            If Not TpRunTsip.TeDynTemp1.cursors(curHandle).cursorOpen Then
                ' Cursor isn't opened yet 
                Return [Error].DYN_CUR_NOT_OPEN
            End If

            ' Close the cursor.

            ' First close and release the statement handle.
            Dim hStmt As SQLHANDLE = TpRunTsip.TeDynTemp1.cursors(curHandle).hStmt
            '!!ODBC.SQLCloseCursor(hStmt);
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)

            ' Now disconnect from ODBC and free the handle.
            ' Use Ssutil.DisConn to close the connection because it maintains a count
            ' of the number of open connections.
            Ssutil.DisConn(TpRunTsip.TeDynTemp1.cursors(CInt(curHandle)).hConn)

            ' Modify the cursor accordingly.
            TpRunTsip.TeDynTemp1.cursors(curHandle).hConn = SQLHDBC.Zero
            TpRunTsip.TeDynTemp1.cursors(curHandle).cursorOpen = False

            Return 0
        End Function

        ''' <summary>
        ''' This method fetches a row of call1 data from a DB table, 
        ''' using the same cursor index as previously returned by a call to TeSelectTemp1(). 
        ''' </summary>
        ''' <remarks>
        ''' If searchCriteria is NULL then all the rows are retrieved. 
        ''' <para>If orderBy is NULL then the actual fetched row order is indeterminable.</para> 
        ''' </remarks>
        ''' <paramname="table"> - full MS SQL Server temp1 table name</param>
        ''' <paramname="searchCriteria"> - SQL 'SELECT' query 'WHERE' clause.</param>
        ''' <paramname="orderBy"> - SQL 'SELECT' query 'ORDER BY' clause.</param>
        ''' <returns></returns>
        Public Shared Function TeSelectTemp1(table As String, searchCriteria As String, orderBy As String) As Integer         ' full MS SQL Server temp1 table name 
            ' selection criteria 
            ' how is selection to be ordered 
            Dim [select] = "select location from {0} "

            Dim curHandle As Integer          ' cursor handle 

            Dim sqlRet As SQLRETURN = 0
            Dim hStmt As SQLHANDLE

            ' Construct the select clause 
            Dim stmt_buf = SQLCHARPTR.Format([select], table)

            ' Construct the 'where' part of the select clause 
            If Not SQLCHARPTR.IsNullOrWhiteSpace(searchCriteria) Then
                ' Search criteria was specified 
                stmt_buf += "where "
                stmt_buf += searchCriteria
            End If

            ' If "order by" is not specified then assume its for update 
            If Not SQLCHARPTR.IsNullOrWhiteSpace(orderBy) Then
                ' Order by was specified 
                stmt_buf += " order by "
                stmt_buf += orderBy
            End If

            '	Get next free cursor area
            curHandle = TpRunTsip.TeDynTemp1.GetNextFreeCursor()

            '	allocate and open the handle
            Dim hConn As SQLHDBC = Ssutil.NewConn()

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeDynTemp1.TeSelectTemp1(): ERROR: SQLExecDirect failed.")
                Return [Error].ODBC_EXECUTE_FAILED
            End If

            '	Inititialize the cursor handle structure
            TpRunTsip.TeDynTemp1.cursors(curHandle).pastLastRow = False
            TpRunTsip.TeDynTemp1.cursors(curHandle).cursorOpen = ODBC.IsOK(sqlRet)
            TpRunTsip.TeDynTemp1.cursors(curHandle).hStmt = hStmt
            TpRunTsip.TeDynTemp1.cursors(curHandle).hConn = hConn
            TpRunTsip.TeDynTemp1.cursors(curHandle).tableName = table

            Return curHandle
        End Function

        ''' <summary>
        ''' This method fetches a row of location data from a DB table, 
        ''' using the same cursor index as previously returned by a call to TeSelectTemp1(). 
        ''' </summary>
        ''' <paramname="curHandle"> - cursor handle (index) returned by TtSelectTemp1().</param>
        ''' <paramname="location"> - fetched location.</param>
        ''' <paramname="nullInd"> - ODBC null indicator associated  with the fetch of location.</param>
        ''' <returns></returns>
        Public Shared Function TeFetchTemp1(curHandle As Integer, <Out> ByRef location As String, <Out> ByRef nullInd As SQLLEN) As Integer
            ' 'out' requirement.
            location = Nothing
            nullInd = Constant.DB_NULL

            If TpRunTsip.TeDynTemp1.cursors(curHandle).cursorOpen <> True Then
                ' Cursor isn't opened yet 
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeDynTemp1.TeFetchTemp1(): ERROR: Cursor object is not marked as 'open'.")
                Return [Error].DYN_CUR_NOT_OPEN
            End If

            Dim sqlRet As SQLRETURN = 0
            Dim hStmt As SQLHANDLE = TpRunTsip.TeDynTemp1.cursors(curHandle).hStmt

            sqlRet = ODBC.SQLFetch(hStmt)

            If Not ODBC.IsOK(sqlRet) Then
                If sqlRet = ODBC.SQL_NO_DATA Then
                    Return sqlRet
                Else
                    '	Error
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeDynTemp1.TeFetchTemp1(): ERROR: SQLFetch failed.")
                    Return [Error].ODBC_FETCH_FAILED
                End If
            End If

            '	Now read in the fields
            Try
                Ssutil.DbGetString(hStmt, 1, "location", location, Constant.LOCATION_SZ, nullInd)
            Catch e As Exception
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeDynTemp1.TeFetchTemp1(): ERROR: ODBC 'Get' failed: " & e.Message)
                GenUtil.SetErr("teFetchTemp102: Input error on field: " & e.Message)
                Return [Error].ODBC_GET_FAILED
            End Try

            Return Constant.SUCCESS
        End Function






    End Class
End Namespace
