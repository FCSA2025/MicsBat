Imports _Configuration
Imports _DataStructures
Imports _NewLib
Imports System
Imports _Utillib
Imports SQLCHARPTR = System.String            'Invented to mimic (char *) for [In]  only.
Imports SQLHANDLE = System.IntPtr
Imports SQLHDBC = System.IntPtr
Imports SQLLEN = System.Int64
Imports SQLRETURN = System.Int16
Imports System.Runtime.InteropServices

Namespace TpRunTsip

    ''' <summary>
    ''' Provides methods to set up an episode of fetching callsign (call1) data from 
    ''' any DB table that has a column field named 'call1'.
    ''' </summary>
    ''' <remarks>
    ''' <listtype="bullet">
    ''' <item> The methods in this class are used to perform operations on any 
    ''' DB table that has a column field named 'call1'.</item>
    ''' <item>The DB table name is prescribed by the caller.</item>
    ''' <item>The supported operations are: TtSelectTemp1, TtFetchTemp1 and TtCloseTemp1.</item>
    ''' <item>The caller first calls the method TtSelectTemp1() that returns a cursor 
    ''' for use with a subsequent call to TtFetchTemp1().</item>
    ''' <item>The method TtCloseTemp1 must be called to release internal cursor resources
    ''' (a cursor encapsulates an ODBC connection and statement handle.).</item>
    ''' </list>
    ''' </remarks>
    Public Class TtDynTemp1
        '=====================================================================================

        Public Shared cursors As Cursor() = Arrays.CreateArrayUsingDefaultElementConstructor(Of Cursor)(Constant.NUM_CURSORS_FEW)
        Public Shared nNextFreeCursor As Integer = 0

        '=====================================================================================


        ''' <summary>
        ''' This method returns the integer index of the next free (available) Cursor object;
        ''' this method is used by TtSelectTemp1() which returns the index of a 
        ''' fully-instantiated cursor to be used in subsequent calls to TtFetchTemp1().
        ''' </summary>
        ''' <remarks>
        '''  The User is responsible for instantiating fields of the cursor object 
        '''  (i.e. hConn, hStmt etc). The User is also responsible for setting the cursor field
        '''  cursorOpen to true before using it and then false to release it.
        ''' </remarks>
        ''' <returns></returns>
        ''' <para> - integer index of the next free Cursor object.</para>
        ''' <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        Private Shared Function GetNextFreeCursor() As Integer
            'Get next free cursor area.
            Dim curHandle = [Error].NO_CURSOR_AVAILABLE
            If TpRunTsip.TtDynTemp1.nNextFreeCursor >= Constant.NUM_CURSORS_FEW Then
                '	Search for a free cursors in the list
                For nInd = 0 To Constant.NUM_CURSORS_FEW - 1
                    If Not TpRunTsip.TtDynTemp1.cursors(nInd).cursorOpen Then
                        curHandle = nInd
                        Exit For
                    End If
                Next
                If curHandle = -1 Then
                    ' There are no more open cursors
                    'GenUtil.SetErr("dynSite01 -- No more open cursors.");
                    'return -1;
                    Application.Exit(Microsoft.VisualBasic.Constants.vbCrLf & "DynAntenna.GetNextFreeCursor(): ERROR: No available cursors.")
                End If
            Else
                '	Just use the next available cursor
                curHandle = TpRunTsip.TtDynTemp1.nNextFreeCursor
                '  And increment the free cursor count.
                TpRunTsip.TtDynTemp1.nNextFreeCursor += 1
            End If
            Return curHandle
        End Function

        ''' <summary>
        ''' This method selects rows of callsign data from any DB table 
        ''' that has a column named 'call1'. The caller 
        ''' precribes the temp1 table name, the selection criteria and the order
        ''' in which callsigns are to be fetched. 
        ''' </summary>
        ''' <remarks>
        ''' If searchCriteria is NULL then all the rows are retrieved. 
        ''' <para>If orderBy is NULL then the actual fetched row order is indeterminable.</para> 
        ''' </remarks>
        ''' <paramname="table"> - full MS SQL Server temp1 table name</param>
        ''' <paramname="searchCriteria"> - SQL 'SELECT' query 'WHERE' clause.</param>
        ''' <paramname="orderBy"> - SQL 'SELECT' query 'ORDER BY' clause.</param>
        ''' <returns></returns>
        Public Shared Function TtSelectTemp1(table As String, searchCriteria As String, orderBy As String) As Integer         ' full MS SQL Server temp1 table name 
            ' selection criteria 
            ' how is selection to be ordered) 
            '...Log2.v("\nTtDynTemp1.TtSelectTemp1(): Entry");

            Dim [select] = "select call1 from {0} "
            Dim stmt_buf As String
            Dim curHandle As Integer          ' cursor handle 

            Dim sqlRet As SQLRETURN = 0
            Dim hStmt As SQLHANDLE

            ' Construct the select clause 
            stmt_buf = SQLCHARPTR.Format([select], table)

            ' Construct the 'where' part of the select clause 
            If Not SQLCHARPTR.IsNullOrWhiteSpace(searchCriteria) Then
                ' Search criteria was specified 
                stmt_buf += "where "
                stmt_buf += searchCriteria

                ' If "order by" is not specified then assume its for update 
            ElseIf Not SQLCHARPTR.IsNullOrWhiteSpace(orderBy) Then
                ' Order by was specified 
                stmt_buf += " order by "
                stmt_buf += orderBy
            End If

            curHandle = TpRunTsip.TtDynTemp1.GetNextFreeCursor()

            '	allocate and open the handle
            Dim hConn As SQLHDBC = Ssutil.NewConn()

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynTemp1.TtSelectTemp1(): ERROR: SQLExecDirect() failed for: " & Microsoft.VisualBasic.Constants.vbLf & stmt_buf)

                Return [Error].ODBC_EXECUTE_FAILED
            End If


            '	Inititialize the cursor handle structure
            TpRunTsip.TtDynTemp1.cursors(curHandle).pastLastRow = False
            TpRunTsip.TtDynTemp1.cursors(curHandle).cursorOpen = ODBC.IsOK(sqlRet)
            TpRunTsip.TtDynTemp1.cursors(curHandle).hStmt = hStmt
            TpRunTsip.TtDynTemp1.cursors(curHandle).hConn = hConn
            TpRunTsip.TtDynTemp1.cursors(curHandle).tableName = table

            '...Log2.v("\nTtDynTemp1.TtSelectTemp1(): Exit");
            Return curHandle
        End Function

        ''' <summary>
        ''' This method fetches a row of call1 data from a DB table, 
        ''' using the same cursor index as previously returned by a call to TtSelectTemp1(). 
        ''' </summary>
        ''' <paramname="curHandle"> - cursor handle (index) returned by TtSelectTemp1().</param>
        ''' <paramname="call1"> - fetched callsign.</param>
        ''' <paramname="nullInd"> - ODBC null indicator associated  with the fetch of call1.</param>
        ''' <returns></returns>
        Public Shared Function TtFetchTemp1(curHandle As Integer, <Out> ByRef call1 As String, <Out> ByRef nullInd As SQLLEN) As Integer
            '...Log2.v("\nTtDynTemp1.TtFetchTemp1(): Entry");

            ' 'out' requirement.
            call1 = Nothing
            nullInd = Constant.DB_NULL

            Dim sqlRet As SQLRETURN = 0
            Dim hStmt As SQLHANDLE = TpRunTsip.TtDynTemp1.cursors(curHandle).hStmt

            sqlRet = ODBC.SQLFetch(hStmt)

            If Not ODBC.IsOK(sqlRet) Then
                If sqlRet = ODBC.SQL_NO_DATA Then
                    Return sqlRet
                Else
                    '	Error
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynTemp1.TtFetchTemp1(): ERROR: SQLFetch() failed.")
                    Return [Error].ODBC_FETCH_FAILED
                End If
            End If

            '	Now read in the fields

            Try
                Ssutil.DbGetString(hStmt, 1, "call1", call1, Constant.CALL_SZ, nullInd)
            Catch e As Exception
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynTemp1.TtFetchTemp1():ERROR: ODBC 'Get' failed: " & e.Message)
                GenUtil.SetErr("ttFetchTemp102: Input error on field: " & e.Message)
                Return [Error].ODBC_GET_FAILED
            End Try

            '...Log2.v("\nTtDynTemp1.TtFetchTemp1(): Exit");
            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method closes an active cursor object by freeing the ODBC 
        ''' connection and statement handles and setting the cursor field
        ''' cursorOpen to false. 
        ''' </summary>
        ''' <paramname="curHandle"> - cursor handle (index).</param>
        ''' <returns></returns>
        Public Shared Function TtCloseTemp1(curHandle As Integer) As Integer
            If curHandle >= Constant.NUM_CURSORS_FEW OrElse curHandle < 0 Then
                ' Bad handle 
                Return [Error].DYN_CUR_NOT_OPEN
            End If

            If Not TpRunTsip.TtDynTemp1.cursors(curHandle).cursorOpen Then
                ' Cursor isn't opened yet 
                Return [Error].DYN_CUR_NOT_OPEN
            End If

            ' Close the antenna cursor.

            ' First close and release the statement handle.
            Dim hStmt As SQLHANDLE = TpRunTsip.TtDynTemp1.cursors(curHandle).hStmt
            '!!ODBC.SQLCloseCursor(hStmt);
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)

            ' Now disconnect from ODBC and free the handle.
            ' Use Ssutil.DisConn to close the connection because it maintains a count
            ' of the number of open connections.
            Ssutil.DisConn(TpRunTsip.TtDynTemp1.cursors(CInt(curHandle)).hConn)

            ' Modify the cursor accordingly.
            TpRunTsip.TtDynTemp1.cursors(curHandle).hConn = SQLHDBC.Zero
            TpRunTsip.TtDynTemp1.cursors(curHandle).cursorOpen = False

            Return 0
        End Function








    End Class
End Namespace
