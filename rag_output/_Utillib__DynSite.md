# Documented File: DynSite.cs
**Repository Path:** `_Utillib\DynSite.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using _Configuration;
    using _NewLib;
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
    /// Provides methods to select, update, fetch, insert and delete PDF TS SITE
    /// records in the database table <b>&lt;userID&gt;.ft_&lt;pdfName&gt;_site</b>
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item> The methods in this class are used to perform operations on the
    /// SITE table. The actual SITE table name is a variable.</item>
    /// <item>The supported operations are: selectSite, fetchSite,
    /// updateSite, insertSite, deleteSite and closeSite.</item>
    /// <item>The caller must first call selectSite to select the required rows:
    /// this method simply sets up the cursor for subsequent use.</item>
    /// <item>Once selectSite has been called, the user must call fetchSite
    /// to  retreive a row.</item>
    /// <item>The methods updateSite, deleteSite and insertSite can only
    /// be called after a row has been fetched. Note: the code doesn't
    /// enforce this rule.</item>
    /// <item>The method closeSite must be called to release ODBC resources 
    /// (connection and statement handles etc) and to release the cursor back the 'pool'.</item>
    /// </list>
    /// </remarks>
    public class DynSite
    {
        //===================================================================================================================================
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        private static int nNextFreeCursor = 0;
        //===================================================================================================================================

        /// <summary>
        /// This method returns the integer index of the next free (available) FtCursor object.
        /// </summary>
        /// <remarks>
        ///  The user is responsible for populating the values of the cursor object 
        ///  (e.g. hConn, hStmt etc). The user is also responsible for setting the field
        ///  cursorOpen to true before using it and false to release it.
        /// </remarks>
        /// <returns></returns>
        /// <para> - integer index of the next free FtCursor object.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        public static int GetNextFreeCursor()
        {
            //...Log2.v("\n\nDynSite.GetNextFreeCursor(): Entry: nNextFreeCursor = " + nNextFreeCursor);

            //Get next free cursor area.
            int curHandle = -1;

            //...Log2.v("\n\nDynSite.GetNextFreeCursor(): nNextFreeCursor = " + nNextFreeCursor);
            if (nNextFreeCursor >= Constant.NUM_CURSORS_FEW)
            {
                //	Search for a free cursors in the list
                for (int nInd = 0; nInd < Constant.NUM_CURSORS_FEW; nInd++)
                {
                    //...Log2.v("\n\nDynSite.GetNextFreeCursor(): nInd = " + nInd);
                    if (!cursors[nInd].cursorOpen)
                    {
                        curHandle = nInd;
                        break;
                    }
                }

                if (curHandle == -1)
                {
                    // There are no more open cursors
                    GenUtil.SetErr("dynSite01 -- No more open cursors.");
                    Application.Exit("\r\nDynSite.GetNextFreeCursor(): ERROR: No available cursors.");
                    return -1;
                }

            }
            else
            {
                //	Just use the next available cursor
                curHandle = nNextFreeCursor;

                //  And increment the free cursor count.
                nNextFreeCursor++;
            }

            //...Log2.v("\n\nDynSite.GetNextFreeCursor(): found available cursor: curHandle = " + curHandle);

            cursors[curHandle].cursorOpen = true;

            //...Log2.v("\n\nDynSite.GetNextFreeCursor(): Exit");
            return curHandle;
        }

        /// <summary>
        /// This method returns the integer index of the next free (available) FtCursor object
        /// for a prescribed table name.
        /// </summary>
        /// <remarks>
        ///  The user is responsible for populating the values of the cursor object 
        ///  (e.g. hConn, hStmt etc). The user is also responsible for setting the field
        ///  cursorOpen to true before using it and false to release it.
        /// </remarks>
        /// <returns></returns>
        /// <para> - integer index of the next free FtCursor object.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        public static int GetNextFreeCursor(string tableName)
        {
            int nextFreeCursor = GetNextFreeCursor();

            cursors[nextFreeCursor].tableName = tableName;

            return nextFreeCursor;
        }

        /// <summary>
        /// Selects site records from a site table in the database for
        /// the prescribed site table name, SQL search criteria and ordering clauses; the 
        /// method returns an index to a FtCursor object that can be used by subsequent
        /// calls to FtFetchSite().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for FtFetchSite() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECUTE_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int FtSelectSite(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynSite.FtSelectSite(): Entry");
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the select clause.
            string stmt_buf = "select " + FtSite.AllColumnsForSqlSelect + " from " + table;

            //Construct the 'where' part of the select clause.
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                stmt_buf += " where " + searchCriteria;
            }

            //If "order by" is not specified then assume it's for update.
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                // Order by was specified.
                //strcat_s(stmt_buf, sizeof(stmt_buf), " order by ");
                //strcat_s(stmt_buf, sizeof(stmt_buf), orderBy);
                stmt_buf += " order by " + orderBy;
            }

            //...Log2.v("\r\nDynSite.FtSelectSite(): stmt_buf = " + stmt_buf);

            //Get next free cursor area.
            curHandle = GetNextFreeCursor();

            //Make an new connection with the DB.
            SQLHDBC hConn = Ssutil.NewConn();

            //Get a statement handle.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            //...Log2.v("\r\nDynSite.FtSelectSite(): [1] sqlRet = " + sqlRet);

            //Set the statement's attributes.
            sqlRet = ODBC.SQLSetStmtAttr(hStmt, ODBC.SQL_ATTR_CURSOR_TYPE, (SQLPOINTER)ODBC.SQL_CURSOR_DYNAMIC, 0);
            //...Log2.v("\r\nDynSite.FtSelectSite(): [2] sqlRet = " + sqlRet);

            sqlRet = ODBC.SQLSetStmtAttr(hStmt, ODBC.SQL_ATTR_CONCURRENCY, (SQLPOINTER)ODBC.SQL_CONCUR_VALUES, 0);
            //...Log2.v("\r\nDynSite.FtSelectSite(): [3] sqlRet = " + sqlRet);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length);
            //...Log2.v("\r\nDynSite.FtSelectSite(): [4] sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.w("\nDynSite.FtSelectSite(): Could not Select Site for criteria: " + searchCriteria);

                Ssutil.DbGetDiagStmt(hStmt, "Could not Select Site for criteria: " + searchCriteria + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);

                cursors[curHandle].hStmt = IntPtr.Zero;
                cursors[curHandle].cursorOpen = false;

                return -2;
            }

            //	Populate the cursor handle structure
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].tableName = table;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cCurrentCall1 = "";
            cursors[curHandle].sqlQuery = stmt_buf;

            //...Log2.v("\n\nDynSite.FtSelectSite(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Updates the fields in a PDF TS site record to mirror those prescribed in
        /// a FtSite object.
        /// </summary>
        /// <param name="curHandle">- index of the FtCursor object.</param>
        /// <param name="pSite"> - FtSite object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pSite.</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - update attempt was successful.</para>
        /// <para>- ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>- ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>- ErrorMessages.ODBC_BINDING_ERROR - attempt to create an ODBC parameter binding failed.  </para>
        /// <para>- ErrorMessages.ODBC_QUERY_FAILED - update attempt failed - ODBC diagnostic information will be written to output.</para>
        public static int FtUpdateSite(int curHandle, FtSite pSite, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynSite.FtUpdateSite(): Entry");

            string update_buf;
            SQLHANDLE hStmt;
            SQLHANDLE hConn = Ssutil.NewConn();

            if (hConn == SQLHANDLE.Zero)
            {
                Application.Exit("\r\nDynSite.FtUpdateSite(): hConn = Ssutil.NewConn() FAILED");
            }

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Ssutil.DisConnStmt(hConn, hStmt);
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (cursors[curHandle].pastLastRow)
            {
                /* Cursor is past the last row */
                Ssutil.DisConnStmt(hConn, hStmt);
                return (Error.DYN_PAST_LAST_ROW);
            }

            //Prepare the update statement.

            //First construct the SQL 'where' clause.
            StringBuilder whereSB = new StringBuilder();
            whereSB.Append(" where call1='");
            whereSB.Append(cursors[curHandle].cCurrentCall);
            whereSB.Append("'");

            StringBuilder sb = new StringBuilder();
            sb.Append("update ");
            sb.Append(cursors[curHandle].tableName);
            sb.Append(" set ");
            sb.Append(FtSite.AllColumnsForSqlUpdateAsBindings);
            sb.Append(whereSB);

            update_buf = sb.ToString();

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pSite
            //into global memory with an SQLPOINTER pointer assigned to each one. FtSite provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = pSite.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                //This will be used to enumerate the binding sequence; ODBC definition is that first binding is N = 1;
                int colNum;

                // FtSite member #00  string  cmd
                colNum = 1;
                Ssutil.DbBindStringInput(hStmt, colNum, "cmd", parameterValuePtr[colNum - 1], (SQLULEN)pSite.cmd.Length, nullIndPtr[colNum - 1]);

                // FtSite member #01  string  recstat
                colNum = 2;
                Ssutil.DbBindStringInput(hStmt, colNum, "recstat", parameterValuePtr[colNum - 1], (SQLULEN)pSite.recstat.Length, nullIndPtr[colNum - 1]);

                // FtSite member #02  string  call1
                colNum = 3;
                Ssutil.DbBindStringInput(hStmt, colNum, "call1", parameterValuePtr[colNum - 1], (SQLULEN)pSite.call1.Length, nullIndPtr[colNum - 1]);

                // FtSite member #03  string  name
                colNum = 4;
                Ssutil.DbBindStringInput(hStmt, colNum, "name", parameterValuePtr[colNum - 1], (SQLULEN)pSite.name.Length, nullIndPtr[colNum - 1]);

                // FtSite member #04  string  prov
                colNum = 5;
                Ssutil.DbBindStringInput(hStmt, colNum, "prov", parameterValuePtr[colNum - 1], (SQLULEN)pSite.prov.Length, nullIndPtr[colNum - 1]);

                // FtSite member #05  string  oper
                colNum = 6;
                Ssutil.DbBindStringInput(hStmt, colNum, "oper", parameterValuePtr[colNum - 1], (SQLULEN)pSite.oper.Length, nullIndPtr[colNum - 1]);

                // FtSite member #06  int  latit
                colNum = 7;
                Ssutil.DbBindIntInput(hStmt, colNum, "latit", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #07  int  longit
                colNum = 8;
                Ssutil.DbBindIntInput(hStmt, colNum, "longit", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #08  float  grnd
                colNum = 9;
                Ssutil.DbBindFloatInput(hStmt, colNum, "grnd", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #09  string  stats
                colNum = 10;
                Ssutil.DbBindStringInput(hStmt, colNum, "stats", parameterValuePtr[colNum - 1], (SQLULEN)pSite.stats.Length, nullIndPtr[colNum - 1]);

                // FtSite member #10  string  sdate
                colNum = 11;
                Ssutil.DbBindStringInput(hStmt, colNum, "sdate", parameterValuePtr[colNum - 1], (SQLULEN)pSite.sdate.Length, nullIndPtr[colNum - 1]);

                // FtSite member #11  string  loc
                colNum = 12;
                Ssutil.DbBindStringInput(hStmt, colNum, "loc", parameterValuePtr[colNum - 1], (SQLULEN)pSite.loc.Length, nullIndPtr[colNum - 1]);

                // FtSite member #12  string  icaccount
                colNum = 13;
                Ssutil.DbBindStringInput(hStmt, colNum, "icaccount", parameterValuePtr[colNum - 1], (SQLULEN)pSite.icaccount.Length, nullIndPtr[colNum - 1]);

                // FtSite member #13  string  reg
                colNum = 14;
                Ssutil.DbBindStringInput(hStmt, colNum, "reg", parameterValuePtr[colNum - 1], (SQLULEN)pSite.reg.Length, nullIndPtr[colNum - 1]);

                // FtSite member #14  string  spoint
                colNum = 15;
                Ssutil.DbBindStringInput(hStmt, colNum, "spoint", parameterValuePtr[colNum - 1], (SQLULEN)pSite.spoint.Length, nullIndPtr[colNum - 1]);

                // FtSite member #15  string  nots
                colNum = 16;
                Ssutil.DbBindStringInput(hStmt, colNum, "nots", parameterValuePtr[colNum - 1], (SQLULEN)pSite.nots.Length, nullIndPtr[colNum - 1]);

                // FtSite member #16  string  oprtyp
                colNum = 17;
                Ssutil.DbBindStringInput(hStmt, colNum, "oprtyp", parameterValuePtr[colNum - 1], (SQLULEN)pSite.oprtyp.Length, nullIndPtr[colNum - 1]);

                // FtSite member #17  string  snumb
                colNum = 18;
                Ssutil.DbBindStringInput(hStmt, colNum, "snumb", parameterValuePtr[colNum - 1], (SQLULEN)pSite.snumb.Length, nullIndPtr[colNum - 1]);

                // FtSite member #18  short  notwr
                colNum = 19;
                Ssutil.DbBindShortInput(hStmt, colNum, "notwr", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #19  int  bandwd1
                colNum = 20;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #20  int  bandwd2
                colNum = 21;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #21  int  bandwd3
                colNum = 22;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd3", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #22  int  bandwd4
                colNum = 23;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd4", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #23  int  bandwd5
                colNum = 24;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd5", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #24  int  bandwd6
                colNum = 25;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd6", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #25  int  bandwd7
                colNum = 26;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd7", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #26  int  bandwd8
                colNum = 27;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd8", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #27  string  mdate
                colNum = 28;
                Ssutil.DbBindStringInput(hStmt, colNum, "mdate", parameterValuePtr[colNum - 1], (SQLULEN)pSite.mdate.Length, nullIndPtr[colNum - 1]);

                // FtSite member #28  string  mtime
                colNum = 29;
                Ssutil.DbBindStringInput(hStmt, colNum, "mtime", parameterValuePtr[colNum - 1], (SQLULEN)pSite.mtime.Length, nullIndPtr[colNum - 1]);

            }
            catch (Exception e)
            {
                Log2.e("\r\nDynSite.FtUpdateSite(): ERROR: Exception caught");
                Ssutil.DbGetDiagStmt(hStmt, "dynSite03 -- Error binding parameters for site on field: " + e.Message);
                Ssutil.DisConnStmt(hConn, hStmt);
                return -3;
            }

            //...Log2.v("\r\nDynSite.FtUpdateSite(): SQLExecDirect():\r\n" + update_buf);
            SQLRETURN sqlRet;
            sqlRet = ODBC.SQLExecDirect(hStmt, update_buf, update_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\r\nDynSite.FtUpdateSite(): SQLExecDirect(): ERROR: sqlRet = " + sqlRet);
                string str = "dynSite04 -- Error writing Site: " + pSite.call1;
                Ssutil.DbGetDiagStmt(hStmt, str);
                //...Log2.v(str);
                Ssutil.DisConnStmt(hConn, hStmt);
                return -4;
            }

            //...Log2.v("\r\nDynSite.FtUpdateSite(): SQLExecDirect(): SUCCEEDED: sqlRet = " + sqlRet);

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //Application.Exit("DynSite.FtUpdateSite(): forced termination.");

            //...Log2.v("\n\nDynSite.FtUpdateSite(): Exit");
            return 0;
        }

        /// <summary>
        /// Retrieves a single row of site data from a table in the database using 
        /// the FtCursor object created by a previous call to FtSelectSite().
        /// </summary>
        /// <param name="curHandle"> - FtCursor object.</param>
        /// <param name="siteRec"> - a FtSite object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for siteRec.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int FtFetchSite(int curHandle, out FtSite siteRec, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynSite.FtFetchSite(): Entry");

            //Create an FtSite object to output.
            siteRec = new FtSite();
            //Create an array of nulls to output.
            nullInd = new SQLLEN[FtSite.NUM_COLUMNS];

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                //...Log2.v("\r\nDynSite.FtFetchSite(): FAIL: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // memset(siteRec, 0, sizeof(struct ftSite_));		/* clear to be safe */
            siteRec.Initialize();

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\r\nDynSite.FtFetchSite(): ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    //...Log2.v("\r\nDynSite.FtFetchSite(): ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return -1;
                }
            }

            //...Log2.v("\r\nDynSite.FtFetchSite(): ODBC.SQLFetch(): SUCCEEDED");

            // Now read in the fields
            try
            {
                //The following integer will be used to enumerate the SQL table column numbers, starting at 1.
                int colNum;

                // Ftsite member #00  string  cmd
                colNum = 1;
                Ssutil.DbGetString(hStmt, colNum, "cmd", out siteRec.cmd, Constant.CMD_SZ, out nullInd[colNum - 1]);

                // Ftsite member #01  string  recstat
                colNum = 2;
                Ssutil.DbGetString(hStmt, colNum, "recstat", out siteRec.recstat, Constant.RECSTAT_SZ, out nullInd[colNum - 1]);

                // Ftsite member #02  string  call1
                colNum = 3;
                Ssutil.DbGetString(hStmt, colNum, "call1", out siteRec.call1, Constant.CALL_SZ, out nullInd[colNum - 1]);

                // Ftsite member #03  string  name
                colNum = 4;
                Ssutil.DbGetString(hStmt, colNum, "name", out siteRec.name, Constant.FT_SITE_NAME_SZ, out nullInd[colNum - 1]);

                // Ftsite member #04  string  prov
                colNum = 5;
                Ssutil.DbGetString(hStmt, colNum, "prov", out siteRec.prov, Constant.PROV_SZ, out nullInd[colNum - 1]);

                // Ftsite member #05  string  oper
                colNum = 6;
                Ssutil.DbGetString(hStmt, colNum, "oper", out siteRec.oper, Constant.OPER_SZ, out nullInd[colNum - 1]);

                // Ftsite member #06  int  latit
                colNum = 7;
                Ssutil.DbGetInt(hStmt, colNum, "latit", out siteRec.latit, out nullInd[colNum - 1]);

                // Ftsite member #07  int  longit
                colNum = 8;
                Ssutil.DbGetInt(hStmt, colNum, "longit", out siteRec.longit, out nullInd[colNum - 1]);

                // Ftsite member #08  float  grnd
                colNum = 9;
                Ssutil.DbGetFloat(hStmt, colNum, "grnd", out siteRec.grnd, out nullInd[colNum - 1]);

                // Ftsite member #09  string  stats
                colNum = 10;
                Ssutil.DbGetString(hStmt, colNum, "stats", out siteRec.stats, Constant.STATS_SZ, out nullInd[colNum - 1]);

                // Ftsite member #10  string  sdate
                colNum = 11;
                Ssutil.DbGetString(hStmt, colNum, "sdate", out siteRec.sdate, Constant.DATE_SZ, out nullInd[colNum - 1]);

                // Ftsite member #11  string  loc
                colNum = 12;
                Ssutil.DbGetString(hStmt, colNum, "loc", out siteRec.loc, Constant.FT_SITE_LOC_SZ, out nullInd[colNum - 1]);

                // Ftsite member #12  string  icaccount
                colNum = 13;
                Ssutil.DbGetString(hStmt, colNum, "icaccount", out siteRec.icaccount, Constant.ICACCOUNT_SZ, out nullInd[colNum - 1]);

                // Ftsite member #13  string  reg
                colNum = 14;
                Ssutil.DbGetString(hStmt, colNum, "reg", out siteRec.reg, Constant.REG_SZ, out nullInd[colNum - 1]);

                // Ftsite member #14  string  spoint
                colNum = 15;
                Ssutil.DbGetString(hStmt, colNum, "spoint", out siteRec.spoint, Constant.SPOINT_SZ, out nullInd[colNum - 1]);

                // Ftsite member #15  string  nots
                colNum = 16;
                Ssutil.DbGetString(hStmt, colNum, "nots", out siteRec.nots, Constant.NOTS_SZ, out nullInd[colNum - 1]);

                // Ftsite member #16  string  oprtyp
                colNum = 17;
                Ssutil.DbGetString(hStmt, colNum, "oprtyp", out siteRec.oprtyp, Constant.OPRTYP_SZ, out nullInd[colNum - 1]);

                // Ftsite member #17  string  snumb
                colNum = 18;
                Ssutil.DbGetString(hStmt, colNum, "snumb", out siteRec.snumb, Constant.SNUMB_SZ, out nullInd[colNum - 1]);

                // Ftsite member #18  short  notwr
                colNum = 19;
                Ssutil.DbGetShort(hStmt, colNum, "notwr", out siteRec.notwr, out nullInd[colNum - 1]);

                // Ftsite member #19  int  bandwd1
                colNum = 20;
                Ssutil.DbGetInt(hStmt, colNum, "bandwd1", out siteRec.bandwd1, out nullInd[colNum - 1]);

                // Ftsite member #20  int  bandwd2
                colNum = 21;
                Ssutil.DbGetInt(hStmt, colNum, "bandwd2", out siteRec.bandwd2, out nullInd[colNum - 1]);

                // Ftsite member #21  int  bandwd3
                colNum = 22;
                Ssutil.DbGetInt(hStmt, colNum, "bandwd3", out siteRec.bandwd3, out nullInd[colNum - 1]);

                // Ftsite member #22  int  bandwd4
                colNum = 23;
                Ssutil.DbGetInt(hStmt, colNum, "bandwd4", out siteRec.bandwd4, out nullInd[colNum - 1]);

                // Ftsite member #23  int  bandwd5
                colNum = 24;
                Ssutil.DbGetInt(hStmt, colNum, "bandwd5", out siteRec.bandwd5, out nullInd[colNum - 1]);

                // Ftsite member #24  int  bandwd6
                colNum = 25;
                Ssutil.DbGetInt(hStmt, colNum, "bandwd6", out siteRec.bandwd6, out nullInd[colNum - 1]);

                // Ftsite member #25  int  bandwd7
                colNum = 26;
                Ssutil.DbGetInt(hStmt, colNum, "bandwd7", out siteRec.bandwd7, out nullInd[colNum - 1]);

                // Ftsite member #26  int  bandwd8
                colNum = 27;
                Ssutil.DbGetInt(hStmt, colNum, "bandwd8", out siteRec.bandwd8, out nullInd[colNum - 1]);

                // Ftsite member #27  string  mdate
                colNum = 28;
                Ssutil.DbGetString(hStmt, colNum, "mdate", out siteRec.mdate, Constant.DATE_SZ, out nullInd[colNum - 1]);

                // Ftsite member #28  string  mtime
                colNum = 29;
                Ssutil.DbGetString(hStmt, colNum, "mtime", out siteRec.mtime, Constant.TIME_SZ, out nullInd[colNum - 1]);

            }
            catch (Exception e)
            {
                //...Log2.v("\r\nDynSite.FtFetchSite(): Exception caught: " + e.Message);
                //      seterr("dynSite02 -- Could not get site, because of field %s", cName);
                GenUtil.SetErr("dynSite02 -- Could not get sitenna, because of field " + e.Message);
                return -2;
            }

            //...Log2.v(siteRec.ToString());

            //	Save the key for deletes.
            cursors[curHandle].cCurrentCall = siteRec.call1;

            //...Log2.v("\n\nDynSite.FtFetchSite(): Exit");
            return 0;
        }

        /// <summary>
        /// Retrieves the call1 for a single row of site data from a table in the database using 
        /// the FtCursor object created by a previous call to FtSelectSite().
        /// </summary>
        /// <param name="curHandle"> - cursor handle as used in previous call to FtSelectSite().</param>
        /// <param name="call1"> - output call1 string.</param>
        /// <param name="nullInd"> - output ODBC nullInds for fetched call1.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int FtFetchSiteCall1(int curHandle, out string call1, out SQLLEN nullInd)
        {
            //...Log2.v("\n\nDynSite.FtFetchSiteCall1(): Entry");

            // 'out' requirement;
            call1 = "";
            nullInd = Constant.DB_NULL;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                //...Log2.v("\r\nDynSite.FtFetchSiteCall1(): FAIL: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\r\nDynSite.FtFetchSiteCall1(): ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    //...Log2.v("\r\nDynSite.FtFetchSiteCall1(): ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return -1;
                }
            }

            //...Log2.v("\r\nDynSite.FtFetchSiteCall1(): ODBC.SQLFetch(): SUCCEEDED");

            // Now read in the call1 field
            try
            {
                //The following integer will be used to enumerate the SQL table column numbers, starting at 1.
                int colNum;

                // Ftsite member #02  string  call1
                colNum = 3;
                Ssutil.DbGetString(hStmt, colNum, "call1", out call1, Constant.CALL_SZ, out nullInd);
            }
            catch (Exception e)
            {
                //...Log2.v("\r\nDynSite.FtFetchSiteCall1(): Exception caught: " + e.Message);
                //      seterr("dynSite02 -- Could not get site, because of field %s", cName);
                GenUtil.SetErr("dynSite02 -- Could not get sitenna, because of field " + e.Message);
                return -2;
            }

            //...Log2.v(siteRec.ToString());

            //	Save the key for deletes.
            cursors[curHandle].cCurrentCall = call1;

            //...Log2.v("\n\nDynSite.FtFetchSiteCall1(): Exit");
            return 0;
        }

        /// <summary>
        /// Deletes a prescribed PDF TS site record from the database.
        /// </summary>
        /// <param name="curHandle"> - index of the FtCursor object that encapsulates the details of the record deletion.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - deletion attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        public static int FtDeleteSite(int curHandle)
        {
            //...Log2.v("\n\nDynSite.FtDeleteSite(): Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hConn = Ssutil.NewConn();
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            //Create the SQL statement for deletion of a site record.
            string delete_buf = String.Format("delete from {0} where call1='{1}'",
                cursors[curHandle].tableName,
                cursors[curHandle].cCurrentCall
                );

            //...Log2.v("\r\nDynSite.FtDeleteSite(): SQLExecDirect():\r\n" + delete_buf);
            sqlRet = ODBC.SQLExecDirect(hUpdate, delete_buf, delete_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hUpdate, "ftDeleteSite01 -- Error deleting from " + cursors[curHandle].tableName);
                Ssutil.DisConnStmt(hConn, hUpdate);
                return -1;
            }

            Ssutil.DisConnStmt(hConn, hUpdate);

            //...Log2.v("\n\nDynSite.FtDeleteSite(): Exit");
            return 0;
        }

        /// <summary>
        /// Closes (releases) a cursor associated with a PDF TS site record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// FtCursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the FtCursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        public static int FtCloseSite(int curHandle)
        {
            //...Log2.v("\nDynSite.FtCloseSite(): Entry: curHandle = " + curHandle);

            if (curHandle >= Constant.NUM_CURSORS_FEW || curHandle < 0)
            {
                /* Bad handle */
                Log2.e("\r\nDynSite.FtCloseSite(): ERROR: bad handle: curHandle = " + curHandle);
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Log2.e("\r\nDynSite.FtCloseSite(): ERROR: cursor not open: curHandle = " + curHandle);
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // Close the Site cursor.

            // First close and release the statement handle.
            SQLHANDLE hStmt = cursors[curHandle].hStmt;
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            // Now disconnect from ODBC.
            // Use Ssutil.DisConn to close the connection because it maintains a count
            // of the number of open connections.
            // Note that DisConn() also frees the connection handle.
            Ssutil.DisConn(cursors[curHandle].hConn);

            // Modify the cursor accordingly.
            cursors[curHandle].hConn = SQLHDBC.Zero;
            cursors[curHandle].cursorOpen = false;

            //...Log2.v("\nDynSite.FtCloseSite(): Exit");
            return 0;
        }

        /// <summary>
        /// Inserts a site record into the database using column values prescribed 
        /// by the fields of the FtSite object.
        /// </summary>
        /// <param name="curHandle"> - index of the FtCursor object that encapsulates the details of the record insertion.</param>
        /// <param name="pSite"> - a FtSite object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pSite</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int FtInsertSite(int curHandle, FtSite pSite, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynSite.FtInsertSite(): Entry");

            //char update_buf[STMT_BUF_SZ];
            string update_buf;

            //SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;

            SQLHANDLE hConn = Ssutil.NewConn();
            if (hConn == SQLHANDLE.Zero)
            {
                Application.Exit("\r\nDynSite.FtInsertSite(): hConn = Ssutil.NewConn() FAILED");
            }

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Ssutil.DisConnStmt(hConn, hStmt);
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (cursors[curHandle].pastLastRow)
            {
                /* Cursor is past the last row */
                Ssutil.DisConnStmt(hConn, hStmt);
                return (Error.DYN_PAST_LAST_ROW);
            }

            //Create the SQL insert statement.
            update_buf = FtSite.BuildSqlInsertString(cursors[curHandle].tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pSite
            //into global memory with an SQLPOINTER pointer assigned to each one. FtSite provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = pSite.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                //This integer is used to enumerate the binding sequence; ODBC definition is that first binding is N = 1;
                int colNum;

                // FtSite member #00  string  cmd
                colNum = 1;
                Ssutil.DbBindStringInput(hStmt, colNum, "cmd", parameterValuePtr[colNum - 1], (SQLULEN)pSite.cmd.Length, nullIndPtr[colNum - 1]);

                // FtSite member #01  string  recstat
                colNum = 2;
                Ssutil.DbBindStringInput(hStmt, colNum, "recstat", parameterValuePtr[colNum - 1], (SQLULEN)pSite.recstat.Length, nullIndPtr[colNum - 1]);

                // FtSite member #02  string  call1
                colNum = 3;
                Ssutil.DbBindStringInput(hStmt, colNum, "call1", parameterValuePtr[colNum - 1], (SQLULEN)pSite.call1.Length, nullIndPtr[colNum - 1]);

                // FtSite member #03  string  name
                colNum = 4;
                Ssutil.DbBindStringInput(hStmt, colNum, "name", parameterValuePtr[colNum - 1], (SQLULEN)pSite.name.Length, nullIndPtr[colNum - 1]);

                // FtSite member #04  string  prov
                colNum = 5;
                Ssutil.DbBindStringInput(hStmt, colNum, "prov", parameterValuePtr[colNum - 1], (SQLULEN)pSite.prov.Length, nullIndPtr[colNum - 1]);

                // FtSite member #05  string  oper
                colNum = 6;
                Ssutil.DbBindStringInput(hStmt, colNum, "oper", parameterValuePtr[colNum - 1], (SQLULEN)pSite.oper.Length, nullIndPtr[colNum - 1]);

                // FtSite member #06  int  latit
                colNum = 7;
                Ssutil.DbBindIntInput(hStmt, colNum, "latit", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #07  int  longit
                colNum = 8;
                Ssutil.DbBindIntInput(hStmt, colNum, "longit", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #08  float  grnd
                colNum = 9;
                Ssutil.DbBindFloatInput(hStmt, colNum, "grnd", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #09  string  stats
                colNum = 10;
                Ssutil.DbBindStringInput(hStmt, colNum, "stats", parameterValuePtr[colNum - 1], (SQLULEN)pSite.stats.Length, nullIndPtr[colNum - 1]);

                // FtSite member #10  string  sdate
                colNum = 11;
                Ssutil.DbBindStringInput(hStmt, colNum, "sdate", parameterValuePtr[colNum - 1], (SQLULEN)pSite.sdate.Length, nullIndPtr[colNum - 1]);

                // FtSite member #11  string  loc
                colNum = 12;
                Ssutil.DbBindStringInput(hStmt, colNum, "loc", parameterValuePtr[colNum - 1], (SQLULEN)pSite.loc.Length, nullIndPtr[colNum - 1]);

                // FtSite member #12  string  icaccount
                colNum = 13;
                Ssutil.DbBindStringInput(hStmt, colNum, "icaccount", parameterValuePtr[colNum - 1], (SQLULEN)pSite.icaccount.Length, nullIndPtr[colNum - 1]);

                // FtSite member #13  string  reg
                colNum = 14;
                Ssutil.DbBindStringInput(hStmt, colNum, "reg", parameterValuePtr[colNum - 1], (SQLULEN)pSite.reg.Length, nullIndPtr[colNum - 1]);

                // FtSite member #14  string  spoint
                colNum = 15;
                Ssutil.DbBindStringInput(hStmt, colNum, "spoint", parameterValuePtr[colNum - 1], (SQLULEN)pSite.spoint.Length, nullIndPtr[colNum - 1]);

                // FtSite member #15  string  nots
                colNum = 16;
                Ssutil.DbBindStringInput(hStmt, colNum, "nots", parameterValuePtr[colNum - 1], (SQLULEN)pSite.nots.Length, nullIndPtr[colNum - 1]);

                // FtSite member #16  string  oprtyp
                colNum = 17;
                Ssutil.DbBindStringInput(hStmt, colNum, "oprtyp", parameterValuePtr[colNum - 1], (SQLULEN)pSite.oprtyp.Length, nullIndPtr[colNum - 1]);

                // FtSite member #17  string  snumb
                colNum = 18;
                Ssutil.DbBindStringInput(hStmt, colNum, "snumb", parameterValuePtr[colNum - 1], (SQLULEN)pSite.snumb.Length, nullIndPtr[colNum - 1]);

                // FtSite member #18  short  notwr
                colNum = 19;
                Ssutil.DbBindShortInput(hStmt, colNum, "notwr", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #19  int  bandwd1
                colNum = 20;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #20  int  bandwd2
                colNum = 21;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #21  int  bandwd3
                colNum = 22;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd3", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #22  int  bandwd4
                colNum = 23;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd4", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #23  int  bandwd5
                colNum = 24;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd5", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #24  int  bandwd6
                colNum = 25;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd6", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #25  int  bandwd7
                colNum = 26;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd7", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #26  int  bandwd8
                colNum = 27;
                Ssutil.DbBindIntInput(hStmt, colNum, "bandwd8", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtSite member #27  string  mdate
                colNum = 28;
                Ssutil.DbBindStringInput(hStmt, colNum, "mdate", parameterValuePtr[colNum - 1], (SQLULEN)pSite.mdate.Length, nullIndPtr[colNum - 1]);

                // FtSite member #28  string  mtime
                colNum = 29;
                Ssutil.DbBindStringInput(hStmt, colNum, "mtime", parameterValuePtr[colNum - 1], (SQLULEN)pSite.mtime.Length, nullIndPtr[colNum - 1]);
            }
            catch (Exception e)
            {
                Ssutil.DbGetDiagStmt(hStmt, "ftInsertSite01 -- Error binding parameters for site on field: " + e.Message);
                Ssutil.DisConnStmt(hConn, hStmt);
                return -1;
            }

            //...Log2.v("nDynSite.FtInsertSite(): SQLExecDirect():\r\n" + update_buf);
            SQLRETURN sqlRet;
            sqlRet = ODBC.SQLExecDirect(hStmt, update_buf, update_buf.Length);

            //...Log2.v("\r\nDynSite.FtInsertSite(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = "ftInsertSite02 -- Error writing Site: " + pSite.call1;
                Ssutil.DbGetDiagStmt(hStmt, str);
                Log2.e("\n\nDynSite.FtInsertSite(): ERROR: sqlRet = {0} for keys = {1}", sqlRet, pSite.KeysToString());
                Log2.e("\n" + ODBC.GetDiagnostics(hStmt, update_buf));
                Ssutil.DisConnStmt(hConn, hStmt);
                return -2;
            }

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynSite.FtInsertSite(): Exit");
            return 0;
        }

        /// <summary>
        /// This method determines whether a TS site record exists in a 
        /// prescribed database tables that matches the prescribed call sign.
        /// </summary>
        /// <param name="tableName"> - fully qualified DB table name.</param>
        /// <param name="call1"> - prescribed call sign to match to.</param>
        /// <returns></returns>
        public static bool FtSiteExistsInTable(string tableName, string call1)
        {
            int numRowsFound;
            string whereClause;

            whereClause = String.Format(" call1 = '{0}'", call1);

            numRowsFound = Ssutil.DbCountRows(tableName, whereClause);

            return numRowsFound >= 1;
        }

        /// <summary>
        /// This method returns a fully-populated FtSite object and its associated array of nullInds
        /// corresponding to a single record fetched from the User table ft_XXX_site where XXX is 
        /// the PDF name; the (FtSite) record is uniquely identified by its key value (call1).
        /// </summary>
        /// <param name="pdfName"></param>
        /// <param name="call1"></param>
        /// <param name="ftSite"></param>
        /// <param name="nullInds"></param>
        /// <param name="found"></param>
        /// <returns></returns>
        public static int FetchFtSiteByKey(string pdfName, string call1, out FtSite ftSite, out SQLLEN[] nullInds, out bool found)
        {
            // 'out' requirements.
            ftSite = null;
            nullInds = null;
            found = false;

            int retVal = Constant.SUCCESS;
            string tableName;
            int cursorID;
            int rc;
            string whereClause;

            // Get the complete SQL Table name from the PDF 'short' name.
            GenUtil.UtCvtName(Constant.FT_SITE, pdfName, out tableName);

            // Form the SQL SELECT where clause.
            whereClause = String.Format("call1 = '{0}'", call1);

            // SELECT the ftSite by its key.
            cursorID = DynSite.FtSelectSite(tableName, whereClause, "");

            // If the SELECT worked then attempt to fetch the ftSite.
            if (cursorID < 0)
            {
                retVal = Constant.FAILURE;
            }
            else
            {
                // Constant.SUCCESS                - fetch attempt was successful.</para>
                // Constant.FAILURE                - fetch attempt failed.</para>
                // ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
                // ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
                // ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
                rc = DynSite.FtFetchSite(cursorID, out ftSite, out nullInds);

                switch (rc)
                {
                    case Constant.SUCCESS:
                        retVal = Constant.SUCCESS;
                        found = true;
                        break;
                    case ODBC.SQL_NO_DATA:
                        found = false;
                        retVal = Constant.SUCCESS;
                        break;
                    default:
                        retVal = Constant.FAILURE;
                        break;
                }
            }

            DynSite.FtCloseSite(cursorID);

            return retVal;
        }

        /// <summary>
        /// This method outputs a BandBits object from a user ft_<pdfName>_site table record
        /// having a prescribed call1.
        /// </summary>
        /// <param name="pdfName"></param>
        /// <param name="call1"></param>
        /// <param name="bandBits"></param>
        /// <param name="found"></param>
        /// <returns></returns>
        public static int FetchBandBits(string pdfName, string call1, out BandBits bandBits, out bool found)
        {
            // 'out' requirements.
            bandBits = null;
            found = false;

            FtSite ftSite;
            SQLLEN[] nullInds;
            int retVal = Constant.FAILURE;

            // Fetch the whole FtSite object from the user's prescribed ft_<pdfName>_site PDF table.
            retVal = FetchFtSiteByKey(pdfName, call1, out ftSite, out nullInds, out found);

            if (retVal == Constant.SUCCESS)
            {
                if (found)
                {
                    if ((nullInds[FtSite.BANDWD1] != Constant.DB_NULL) && (nullInds[FtSite.BANDWD2] != Constant.DB_NULL))
                    {
                        bandBits = ftSite.GetBandBits();
                    }
                    else
                    {
                        retVal = Constant.FAILURE;
Log2.e("\n\nDynSite.FetchBandBits(): ERROR: FetchFtSiteByKey() returned Constant.SUCCESS, 'found' was true, but bandwd1 and/or bandwd2 have DB nulls for pdf = {0}, call1 = {1}", pdfName, call1);
                    }
                }
                else
                {
                    retVal = Constant.FAILURE;
Log2.e("\n\nDynSite.FetchBandBits(): ERROR: FetchFtSiteByKey() returned Constant.SUCCESS but 'found' was false for pdf = {0}, call1 = {1}", pdfName, call1);
                }
            }

            return retVal;

        }

        /// <summary>
        /// This method outputs a list of FtSite objects as SELECTed and FETCHed from the prescribed user
        /// table using prescribed search criteria (SQL WHERE clause) and ORDER BY criteria.
        /// </summary>
        /// <param name="siteTableName"> - name of the site table from a user's PDF import table set.</param>
        /// <param name="searchCriteria"> - SQL WHERE clause.</param>
        /// <param name="orderBy"> - SQL ORDER BT clause.</param>
        /// <param name="siteList"> - List of FtSite objects provided as output.</param>
        public static void GetListOfFtSites(string siteTableName, string searchCriteria, string orderBy, out List<FtSite> siteList)
        {
            // 'out' requirement.
            siteList = new List<FtSite>();

            FtSite ftSite;
            SQLLEN[] nullInds;

            //...Log2.v("\nDynSite.GetListOfSites(): siteTableName = " + siteTableName);

            int handle = DynSite.FtSelectSite(siteTableName, searchCriteria, orderBy);
            if (handle < 0) return;

            //...Log2.v("\nDynSite.GetListOfSites(): select site handle = " + handle);

            while (DynSite.FtFetchSite(handle, out ftSite, out nullInds) == Constant.SUCCESS)
            {
                siteList.Add(ftSite);

                //...Log2.v("\n" + String.Format("DynSite.GetListOfSites(): ftSite = {0}", ftSite.KeysToString()));
            }

            DynSite.FtCloseSite(handle);

            return;
        }


    }
}

```
