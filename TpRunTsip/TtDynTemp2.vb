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
    ''' Provides methods to set up an episode of fetching local and remote callsign (call1 & call2) data from any DB table that has column fields named 'call1' and 'call2.
    ''' </summary>
    ''' <remarks>
    ''' <listtype="bullet">
    ''' <item> The methods in this class are used to perform operations on any 
    ''' DB table that has column fields named 'call1' and 'call2'.</item>
    ''' <item>The DB table name is prescribed by the caller.</item>
    ''' <item>The supported operations are: TtSelectTemp2, TtFetchTemp2 and TtCloseTemp2.</item>
    ''' <item>The caller first calls the method TtSelectTemp2() that returns a cursor 
    ''' for use with a subsequent call to TtFetchTemp1().</item>
    ''' <item>The method TtCloseTemp2 must be called to release internal cursor resources
    ''' (a cursor encapsulates an ODBC connection and statement handle.).</item>
    ''' </list>
    ''' </remarks>
    Public Class TtDynTemp2
        Public Shared cursors As Cursor() = Arrays.CreateArrayUsingDefaultElementConstructor(Of Cursor)(Constant.NUM_CURSORS_FEW)
        Public Shared nNextFreeCursor As Integer = 0

        '------------------------------------------------------------------------------


        ''' <summary>
        ''' This method returns the integer index of the next free (available) Cursor object;
        ''' this method is used by TtSelectTemp2() which returns the index of a 
        ''' fully-instantiated cursor to be used in subsequent calls to TtFetchTemp2().
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
            If TpRunTsip.TtDynTemp2.nNextFreeCursor >= Constant.NUM_CURSORS_FEW Then
                '	Search for a free cursors in the list
                For nInd = 0 To Constant.NUM_CURSORS_FEW - 1
                    If Not TpRunTsip.TtDynTemp2.cursors(nInd).cursorOpen Then
                        curHandle = nInd
                        Exit For
                    End If
                Next
                If curHandle = -1 Then
                    ' There are no more open cursors
                    Application.Exit(Microsoft.VisualBasic.Constants.vbCrLf & "TtDynTemp2.GetNextFreeCursor(): ERROR: No available cursors.")
                End If
            Else
                '	Just use the next available cursor
                curHandle = TpRunTsip.TtDynTemp2.nNextFreeCursor
                '  And increment the free cursor count.
                TpRunTsip.TtDynTemp2.nNextFreeCursor += 1
            End If
            Return curHandle
        End Function

        ''' <summary>
        ''' This method selects rows of callsign data from any DB table 
        ''' that has columns named 'call1' and 'call2'. The caller 
        ''' precribes the temp2 table name, the selection criteria and the order
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
        Public Shared Function TtSelectTemp2(table As String, searchCriteria As String, orderBy As String) As Integer         ' full MS SQL Server Temp2 table name 
            ' selection criteria 
            ' how is selection to be ordered 
            Dim [select] = "select call1, call2 from {0} "

            Dim curHandle As Integer          ' cursor handle 

            Dim sqlRet As SQLRETURN = 0
            Dim hStmt As SQLHANDLE

            ' Construct the select clause 
            Dim stmt_buf = SQLCHARPTR.Format([select], table)

            ' Construct the 'where' part of the select clause 
            If Not Equals(searchCriteria, Nothing) AndAlso Not SQLCHARPTR.IsNullOrWhiteSpace(searchCriteria) Then
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
            curHandle = TpRunTsip.TtDynTemp2.GetNextFreeCursor()

            '	Allocate and open the handle
            Dim hConn As SQLHDBC = Ssutil.NewConn()

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynTemp2.TtSelectTemp2(): ERROR: SQLExecDirect() failed for: " & Microsoft.VisualBasic.Constants.vbLf & stmt_buf)
            End If

            '	Inititialize the cursor handle structure
            TpRunTsip.TtDynTemp2.cursors(curHandle).pastLastRow = False
            TpRunTsip.TtDynTemp2.cursors(curHandle).cursorOpen = ODBC.IsOK(sqlRet)
            TpRunTsip.TtDynTemp2.cursors(curHandle).hStmt = hStmt
            TpRunTsip.TtDynTemp2.cursors(curHandle).hConn = hConn
            TpRunTsip.TtDynTemp2.cursors(curHandle).tableName = table

            Return curHandle
        End Function

        ''' <summary>
        ''' This method fetches a row of call1 & call2 data from a PDF TS temp1 table, 
        ''' using the same cursor index as previously returned by a call to TtSelectTemp2(). 
        ''' </summary>
        ''' <paramname="curHandle"> - cursor handle (index) returned by TtSelectTemp1().</param>
        ''' <paramname="temp2Rec"> - a TtTemp2 object the encapsulates the dyad call1 & call2.</param>
        ''' <paramname="nullInd"> - array[2] of ODBC null indicators associated with temp2Rec.</param>
        ''' <returns></returns>

        Public Shared Function TtFetchTemp2(curHandle As Integer, <Out> ByRef temp2Rec As TtTemp2, <Out> ByRef nullInd As SQLLEN()) As Integer              ' handle returned by 'suSelectTemp2' 
            ' caller's struct for row of data 
            ' caller's array of null indicators 
            ' 'out' requirement
            temp2Rec = Nothing
            nullInd = Nothing

            ' Check that the cursor is valid.
            If TpRunTsip.TtDynTemp2.cursors(curHandle).cursorOpen <> True Then
                ' Cursor isn't opened yet 
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynTemp2.TtFetchTemp2(): ERROR: invalid cursor - not open.")
                Return [Error].DYN_CUR_NOT_OPEN
            End If

            Dim sqlRet As SQLRETURN = 0
            Dim hStmt As SQLHANDLE = TpRunTsip.TtDynTemp2.cursors(curHandle).hStmt

            sqlRet = ODBC.SQLFetch(hStmt)
            If Not ODBC.IsOK(sqlRet) Then
                If sqlRet = ODBC.SQL_NO_DATA Then
                    ' No more data to fetch.
                    Return sqlRet
                Else
                    ' Something is very wrong.
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynTemp2.TtFetchTemp2(): ERROR: SQLFetch() failed, returned " & sqlRet.ToString())
                    Return [Error].ODBC_FETCH_FAILED
                End If
            End If

            '	Now read in the fields
            temp2Rec = New TtTemp2()
            nullInd = NullHelper.CreateArrayOfNullInd(TtTemp2.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

            Try
                Ssutil.DbGetString(hStmt, 1, "call1", temp2Rec.call1, TtTemp2.CALL1_SZ, nullInd(TtTemp2.CALL1))
                Ssutil.DbGetString(hStmt, 2, "call2", temp2Rec.call2, TtTemp2.CALL2_SZ, nullInd(TtTemp2.CALL2))
            Catch e As Exception
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtDynTemp2.TtFetchTemp2(): ERROR: DbGetString() failed: " & Microsoft.VisualBasic.Constants.vbLf & e.Message)
                GenUtil.SetErr("ttFetchTemp202: Input error on field: " & e.Message)
                Return [Error].ODBC_GET_FAILED
            End Try

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method closes an active cursor object by freeing the ODBC 
        ''' connection and statement handles and setting the cursor field
        ''' cursorOpen to false. 
        ''' </summary>
        ''' <paramname="curHandle"> - index of the Cursor object.</param>
        ''' <returns></returns>
        ''' <para>-   Constant.SUCCESS                - successful outcome.</para>
        ''' <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        Public Shared Function TtCloseTemp2(curHandle As Integer) As Integer
            If curHandle >= Constant.NUM_CURSORS_FEW OrElse curHandle < 0 Then
                ' Bad handle 
                Return [Error].DYN_CUR_NOT_OPEN
            End If

            If Not TpRunTsip.TtDynTemp2.cursors(curHandle).cursorOpen Then
                ' Cursor isn't opened yet 
                Return [Error].DYN_CUR_NOT_OPEN
            End If

            ' Close the cursor.

            ' First close and release the statement handle.
            Dim hStmt As SQLHANDLE = TpRunTsip.TtDynTemp2.cursors(curHandle).hStmt
            ODBC.SQLCloseCursor(hStmt)
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)

            ' Now disconnect from ODBC and free the handle.
            ' Use Ssutil.DisConn to close the connection because it maintains a count
            ' of the number of open connections.
            Ssutil.DisConn(TpRunTsip.TtDynTemp2.cursors(CInt(curHandle)).hConn)

            ' Modify the cursor accordingly.
            TpRunTsip.TtDynTemp2.cursors(curHandle).hConn = SQLHDBC.Zero
            TpRunTsip.TtDynTemp2.cursors(curHandle).cursorOpen = False

            Return 0
        End Function





    End Class
End Namespace
