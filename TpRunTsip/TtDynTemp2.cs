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
    /// Provides methods to set up an episode of fetching local and remote callsign (call1 & call2) data from any DB table that has column fields named 'call1' and 'call2.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item> The methods in this class are used to perform operations on any 
    /// DB table that has column fields named 'call1' and 'call2'.</item>
    /// <item>The DB table name is prescribed by the caller.</item>
    /// <item>The supported operations are: TtSelectTemp2, TtFetchTemp2 and TtCloseTemp2.</item>
    /// <item>The caller first calls the method TtSelectTemp2() that returns a cursor 
    /// for use with a subsequent call to TtFetchTemp1().</item>
    /// <item>The method TtCloseTemp2 must be called to release internal cursor resources
    /// (a cursor encapsulates an ODBC connection and statement handle.).</item>
    /// </list>
    /// </remarks>
    public class TtDynTemp2
    {
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;

        //------------------------------------------------------------------------------


        /// <summary>
        /// This method returns the integer index of the next free (available) Cursor object;
        /// this method is used by TtSelectTemp2() which returns the index of a 
        /// fully-instantiated cursor to be used in subsequent calls to TtFetchTemp2().
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
                    Application.Exit("\r\nTtDynTemp2.GetNextFreeCursor(): ERROR: No available cursors.");
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
        /// that has columns named 'call1' and 'call2'. The caller 
        /// precribes the temp2 table name, the selection criteria and the order
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
        public static int TtSelectTemp2(string table,         /* full MS SQL Server Temp2 table name */
                          string searchCriteria,    /* selection criteria */
                                    string orderBy)     /* how is selection to be ordered */

        {
            string select = "select call1, call2 from {0} ";

            int curHandle;          /* cursor handle */

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;

            /* Construct the select clause */
            string stmt_buf = String.Format(select, table);

            /* Construct the 'where' part of the select clause */
            if (searchCriteria != null && !String.IsNullOrWhiteSpace(searchCriteria))
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

            //	Allocate and open the handle
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTtDynTemp2.TtSelectTemp2(): ERROR: SQLExecDirect() failed for: \n" + stmt_buf);
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
        /// This method fetches a row of call1 & call2 data from a PDF TS temp1 table, 
        /// using the same cursor index as previously returned by a call to TtSelectTemp2(). 
        /// </summary>
        /// <param name="curHandle"> - cursor handle (index) returned by TtSelectTemp1().</param>
        /// <param name="temp2Rec"> - a TtTemp2 object the encapsulates the dyad call1 & call2.</param>
        /// <param name="nullInd"> - array[2] of ODBC null indicators associated with temp2Rec.</param>
        /// <returns></returns>
        public static int TtFetchTemp2(int curHandle,				/* handle returned by 'suSelectTemp2' */

                           out TtTemp2 temp2Rec,    /* caller's struct for row of data */
                                 out SQLLEN[] nullInd)  /* caller's array of null indicators */
        {
            // 'out' requirement
            temp2Rec = null;
            nullInd = null;

            // Check that the cursor is valid.
            if (cursors[curHandle].cursorOpen != true)
            {
                /* Cursor isn't opened yet */
                Log2.e("\nTtDynTemp2.TtFetchTemp2(): ERROR: invalid cursor - not open.");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    // No more data to fetch.
                    return (int)sqlRet;
                }
                else
                {
                    // Something is very wrong.
                    Log2.e("\nTtDynTemp2.TtFetchTemp2(): ERROR: SQLFetch() failed, returned " + sqlRet);
                    return Error.ODBC_FETCH_FAILED;
                }
            }

            //	Now read in the fields
            temp2Rec = new TtTemp2();
            nullInd = NullHelper.CreateArrayOfNullInd(TtTemp2.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            try
            {
                Ssutil.DbGetString(hStmt, 1, "call1", out temp2Rec.call1, TtTemp2.CALL1_SZ, out nullInd[TtTemp2.CALL1]);
                Ssutil.DbGetString(hStmt, 2, "call2", out temp2Rec.call2, TtTemp2.CALL2_SZ, out nullInd[TtTemp2.CALL2]);
            }
            catch (Exception e)
            {
                Log2.e("\nTtDynTemp2.TtFetchTemp2(): ERROR: DbGetString() failed: \n" + e.Message);
                GenUtil.SetErr("ttFetchTemp202: Input error on field: " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method closes an active cursor object by freeing the ODBC 
        /// connection and statement handles and setting the cursor field
        /// cursorOpen to false. 
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int TtCloseTemp2(int curHandle)
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
            ODBC.SQLCloseCursor(hStmt);
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
