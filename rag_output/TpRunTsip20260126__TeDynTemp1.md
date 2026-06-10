# Documented File: TeDynTemp1.cs
**Repository Path:** `TpRunTsip20260126\TeDynTemp1.cs`
**Primary Layer:** `TpRunTsip20260126`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TpRunTsip
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
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
    /// Provides methods to set up an episode of fetching location data from 
    /// any DB table that has a column field named 'location'.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item> The methods in this class are used to perform operations on any 
    /// DB table that has a column field named 'location'.</item>
    /// <item>The DB table name is prescribed by the caller.</item>
    /// <item>The supported operations are: TeSelectTemp1, TeFetchTemp1 and TeCloseTemp1.</item>
    /// <item>The caller first calls the method TeSelectTemp1() that returns a cursor 
    /// for use with a subsequent call to TeFetchTemp1().</item>
    /// <item>The method TeCloseTemp1 must be called to release internal cursor resources
    /// (a cursor encapsulates an ODBC connection and statement handle.).</item>
    /// </list>
    /// </remarks>
    public class TeDynTemp1
    {
        //===================================================================================

        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;

        //====================================================================================

        /// <summary>
        /// This method returns the integer index of the next free (available) Cursor object;
        /// this method is used by TeSelectTemp1() which returns the index of a 
        /// fully-instantiated cursor to be used in subsequent calls to TeFetchTemp1().
        /// </summary>
        /// <remarks>
        ///  The User is responsible for instantiating fields of the cursor object 
        ///  (i.e. hConn, hStmt etc). The User is also responsible for setting the cursor field
        ///  cursorOpen to true before using it and then false to release it.
        /// </remarks>
        /// <returns></returns>
        /// <para> - integer index of the next free Cursor object.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        public static int GetNextFreeCursor()
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
                    Qutils.ExitQueue(Info.DbName, "READ");
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
        /// This method closes an active cursor object by freeing the ODBC 
        /// connection and statement handles and setting the cursor field
        /// cursorOpen to false. 
        /// </summary>
        /// <param name="curHandle"> - cursor handle (index).</param>
        /// <returns></returns>
        public static int TeCloseTemp1(int curHandle)
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

            // Close the cursor.

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

        /// <summary>
        /// This method fetches a row of call1 data from a DB table, 
        /// using the same cursor index as previously returned by a call to TeSelectTemp1(). 
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved. 
        /// <para>If orderBy is NULL then the actual fetched row order is indeterminable.</para> 
        /// </remarks>
        /// <param name="table"> - full MS SQL Server temp1 table name</param>
        /// <param name="searchCriteria"> - SQL 'SELECT' query 'WHERE' clause.</param>
        /// <param name="orderBy"> - SQL 'SELECT' query 'ORDER BY' clause.</param>
        /// <returns></returns>
        public static int TeSelectTemp1(string table,         /* full MS SQL Server temp1 table name */
                          string searchCriteria,    /* selection criteria */
                                    string orderBy)    /* how is selection to be ordered */
        {
            string select = "select location from {0} ";

            int curHandle;          /* cursor handle */

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;

            /* Construct the select clause */
            string stmt_buf = String.Format(select, table);

            /* Construct the 'where' part of the select clause */
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                /* Search criteria was specified */
                stmt_buf += "where ";
                stmt_buf += searchCriteria;
            }

            /* If "order by" is not specified then assume its for update */
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                /* Order by was specified */
                stmt_buf += " order by ";
                stmt_buf += orderBy;
            }

            //	Get next free cursor area
            curHandle = GetNextFreeCursor();

            //	allocate and open the handle
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTeDynTemp1.TeSelectTemp1(): ERROR: SQLExecDirect failed.");
                return Error.ODBC_EXECUTE_FAILED;
            }

            //	Inititialize the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].tableName = table;

            return (curHandle);
        }

        /// <summary>
        /// This method fetches a row of location data from a DB table, 
        /// using the same cursor index as previously returned by a call to TeSelectTemp1(). 
        /// </summary>
        /// <param name="curHandle"> - cursor handle (index) returned by TtSelectTemp1().</param>
        /// <param name="location"> - fetched location.</param>
        /// <param name="nullInd"> - ODBC null indicator associated  with the fetch of location.</param>
        /// <returns></returns>
        public static int TeFetchTemp1(int curHandle, out string location, out SQLLEN nullInd)
        {
            // 'out' requirement.
            location = null;
            nullInd = Constant.DB_NULL;

            if (cursors[curHandle].cursorOpen != true)
            {
                /* Cursor isn't opened yet */
                Log2.e("\nTeDynTemp1.TeFetchTemp1(): ERROR: Cursor object is not marked as 'open'.");
                return (Error.DYN_CUR_NOT_OPEN);
            }

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
                    Log2.e("\nTeDynTemp1.TeFetchTemp1(): ERROR: SQLFetch failed.");
                    return Error.ODBC_FETCH_FAILED;
                }
            }

            //	Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "location", out location, Constant.LOCATION_SZ, out nullInd);
            }
            catch (Exception e)
            {
                Log2.e("\nTeDynTemp1.TeFetchTemp1(): ERROR: ODBC 'Get' failed: " + e.Message);
                GenUtil.SetErr("teFetchTemp102: Input error on field: " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            return (Constant.SUCCESS);
        }






    }
}

```
