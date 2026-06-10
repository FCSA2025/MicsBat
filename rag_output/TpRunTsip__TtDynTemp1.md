# Documented File: TtDynTemp1.cs
**Repository Path:** `TpRunTsip\TtDynTemp1.cs`
**Primary Layer:** `TpRunTsip`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TpRunTsip
{
    using _Utillib;
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;            //Invented to mimic (char *) for [In]  only.
    using SQLCHARPTRINOUT = IntPtr;       //Invented to mimic (char *) for [In, Out].
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLINTEGERPTR = IntPtr;
    using SQLLEN = Int64;
    using SQLLENPTR = IntPtr;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;
    using SQLSETPOSIROW = UInt64;
    using SQLSMALLINT = Int16;
    using SQLSMALLINTPTR = IntPtr;
    using SQLULEN = UInt64;
    using SQLUSMALLINT = UInt16;

    /// <summary>
    /// Provides methods to set up an episode of fetching callsign (call1) data from 
    /// any DB table that has a column field named 'call1'.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item> The methods in this class are used to perform operations on any 
    /// DB table that has a column field named 'call1'.</item>
    /// <item>The DB table name is prescribed by the caller.</item>
    /// <item>The supported operations are: TtSelectTemp1, TtFetchTemp1 and TtCloseTemp1.</item>
    /// <item>The caller first calls the method TtSelectTemp1() that returns a cursor 
    /// for use with a subsequent call to TtFetchTemp1().</item>
    /// <item>The method TtCloseTemp1 must be called to release internal cursor resources
    /// (a cursor encapsulates an ODBC connection and statement handle.).</item>
    /// </list>
    /// </remarks>
    public class TtDynTemp1
    {
        //=====================================================================================

        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;

        //=====================================================================================


        /// <summary>
        /// This method returns the integer index of the next free (available) Cursor object;
        /// this method is used by TtSelectTemp1() which returns the index of a 
        /// fully-instantiated cursor to be used in subsequent calls to TtFetchTemp1().
        /// </summary>
        /// <remarks>
        ///  The User is responsible for instantiating fields of the cursor object 
        ///  (i.e. hConn, hStmt etc). The User is also responsible for setting the cursor field
        ///  cursorOpen to true before using it and then false to release it.
        /// </remarks>
        /// <returns></returns>
        /// <para> - integer index of the next free Cursor object.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        private static int GetNextFreeCursor()
        {
            //Get next free cursor area.
            int curHandle = Error.NO_CURSOR_AVAILABLE;
            if (nNextFreeCursor >= Constant.NUM_CURSORS_FEW)
            {
                //	Search for a free cursors in the list
                for (int nInd = 0; nInd < Constant.NUM_CURSORS_FEW; nInd++)
                {
                    if (!cursors[nInd].cursorOpen)
                    {
                        curHandle = nInd;
                        break;
                    }
                }
                if (curHandle == -1)
                {
                    // There are no more open cursors
                    //GenUtil.SetErr("dynSite01 -- No more open cursors.");
                    //return -1;
                    Application.Exit("\r\nDynAntenna.GetNextFreeCursor(): ERROR: No available cursors.");
                }
            }
            else
            {
                //	Just use the next available cursor
                curHandle = nNextFreeCursor;
                //  And increment the free cursor count.
                nNextFreeCursor++;
            }
            return curHandle;
        }

        /// <summary>
        /// This method selects rows of callsign data from any DB table 
        /// that has a column named 'call1'. The caller 
        /// precribes the temp1 table name, the selection criteria and the order
        /// in which callsigns are to be fetched. 
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved. 
        /// <para>If orderBy is NULL then the actual fetched row order is indeterminable.</para> 
        /// </remarks>
        /// <param name="table"> - full MS SQL Server temp1 table name</param>
        /// <param name="searchCriteria"> - SQL 'SELECT' query 'WHERE' clause.</param>
        /// <param name="orderBy"> - SQL 'SELECT' query 'ORDER BY' clause.</param>
        /// <returns></returns>
        public static int TtSelectTemp1(string table,         /* full MS SQL Server temp1 table name */
                          string searchCriteria,    /* selection criteria */
                                    string orderBy     /* how is selection to be ordered) */
                                    )

        {
            //...Log2.v("\nTtDynTemp1.TtSelectTemp1(): Entry");

            string select = "select call1 from {0} ";
            string stmt_buf;
            int curHandle;          /* cursor handle */

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;

            /* Construct the select clause */
            stmt_buf = String.Format(select, table);

            /* Construct the 'where' part of the select clause */
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                /* Search criteria was specified */
                stmt_buf += "where ";
                stmt_buf += searchCriteria;
            }

            /* If "order by" is not specified then assume its for update */
            else if (!String.IsNullOrWhiteSpace(orderBy))
            {
                /* Order by was specified */
                stmt_buf += " order by ";
                stmt_buf += orderBy;
            }

            curHandle = GetNextFreeCursor();

            //	allocate and open the handle
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTtDynTemp1.TtSelectTemp1(): ERROR: SQLExecDirect() failed for: \n" + stmt_buf);

                return Error.ODBC_EXECUTE_FAILED;
            }


            //	Inititialize the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].tableName = table;

            //...Log2.v("\nTtDynTemp1.TtSelectTemp1(): Exit");
            return (curHandle);
        }

        /// <summary>
        /// This method fetches a row of call1 data from a DB table, 
        /// using the same cursor index as previously returned by a call to TtSelectTemp1(). 
        /// </summary>
        /// <param name="curHandle"> - cursor handle (index) returned by TtSelectTemp1().</param>
        /// <param name="call1"> - fetched callsign.</param>
        /// <param name="nullInd"> - ODBC null indicator associated  with the fetch of call1.</param>
        /// <returns></returns>
        public static int TtFetchTemp1(int curHandle,
                           out string call1,
                                 out SQLLEN nullInd)
        {
            //...Log2.v("\nTtDynTemp1.TtFetchTemp1(): Entry");

            // 'out' requirement.
            call1 = null;
            nullInd = Constant.DB_NULL;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            sqlRet = ODBC.SQLFetch(hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\nTtDynTemp1.TtFetchTemp1(): ERROR: SQLFetch() failed.");
                    return Error.ODBC_FETCH_FAILED;
                }
            }

            //	Now read in the fields

            try
            {
                Ssutil.DbGetString(hStmt, 1, "call1", out call1, Constant.CALL_SZ, out nullInd);
            }
            catch (Exception e)
            {
                Log2.e("\nTtDynTemp1.TtFetchTemp1():ERROR: ODBC 'Get' failed: " + e.Message);
                GenUtil.SetErr("ttFetchTemp102: Input error on field: " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\nTtDynTemp1.TtFetchTemp1(): Exit");
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method closes an active cursor object by freeing the ODBC 
        /// connection and statement handles and setting the cursor field
        /// cursorOpen to false. 
        /// </summary>
        /// <param name="curHandle"> - cursor handle (index).</param>
        /// <returns></returns>
        public static int TtCloseTemp1(int curHandle)
        {
            if (curHandle >= Constant.NUM_CURSORS_FEW || curHandle < 0)
            {
                /* Bad handle */
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // Close the antenna cursor.

            // First close and release the statement handle.
            SQLHANDLE hStmt = cursors[curHandle].hStmt;
            //!!ODBC.SQLCloseCursor(hStmt);
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            // Now disconnect from ODBC and free the handle.
            // Use Ssutil.DisConn to close the connection because it maintains a count
            // of the number of open connections.
            Ssutil.DisConn(cursors[curHandle].hConn);

            // Modify the cursor accordingly.
            cursors[curHandle].hConn = SQLHDBC.Zero;
            cursors[curHandle].cursorOpen = false;

            return 0;
        }








    }
}

```
