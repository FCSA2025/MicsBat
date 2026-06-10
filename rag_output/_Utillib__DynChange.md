# Documented File: DynChange.cs
**Repository Path:** `_Utillib\DynChange.cs`
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
    /// The methods in this class perform operations on a PDF TS Change of Call Sign table
    /// with canonical name <b>&lt;userID&gt;.ft_&lt;pdfName&gt;_chng</b>;	
    /// the supported operations are: selectChngCall, fetchChngCall,		
    /// updatechngCall, and closechngCall.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>The caller must first call selectChngCall to select the required 	
    /// rows, this routine simply sets up the cursor for subsequent use.</item> 	
    /// <item>The method ftSelectChngCall returns a 'cursorHandle' to the caller, 	
    /// this handle must be used in all subsequent calls to fetch, update etc</item>
    /// <item>The methods updateChngCall, deleteChngCall and insertChngCall can   
    /// only be called after a row has been fetched. 	</item>		
    /// <item>The routine closeChngCall must be called to release ODBC and cursor resources.	
    /// </list>
    /// </remarks>
    public class DynChange
    {
#if PINVOKE
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int ftSelectChngCall([In] string table, [In] string searchCriteria, [In] string orderBy);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int ftFetchChngCall([In] int nCursor, [In, Out] FtChng chngCall, [In, Out] SQLLEN[] nullInd);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int ftCloseChngCall(int curHandle);
        //----------------------------------------------------------------------

        public static int FtCloseChngCall_NATIVE(int curHandle)
        {
            return ftCloseChngCall(curHandle);
        }

        public static int FtFetchChngCall_NATIVE(int nCursor, out FtChng chngCall, out SQLLEN[] nullInd)
        {
            chngCall = new FtChng();
            nullInd = NullHelper.CreateArrayOfNullInd(FtChng.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            return ftFetchChngCall(nCursor, chngCall, nullInd);
        }

        public static int FtSelectChngCall_NATIVE(string table, string searchCriteria, string orderBy)
        {
            return ftSelectChngCall(table, searchCriteria, orderBy);
        }
#endif
        static string stmt_buf;
        static string select = " select newcall1, oldcall1, name from ";
        //static string forUpdate = " for browse ";
        //static int init = 0;        /* initialization flag */

        //-----------------------------------------------------------------------

        /// <summary>
        /// Struct for keeping track of items associated with a cursor.
        /// </summary>
        private struct FtChngCursorStatus
        {
            public SQLHDBC hConn;
            public SQLHANDLE hStmt;
            public string tableName;    /* table assoc. with cursor */
            public bool cursorOpen;        /* cursor open or closed */
            public bool pastLastRow;       /* cursor past last row */
            public string cCurrentNewCall1;
        };

        /* One struct for each cursor */
        private static FtChngCursorStatus[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<FtChngCursorStatus>(Constant.NUM_CURSORS_FEW);
        private static int nNextFreeCursor = 0;

        //------------------------------------------------------------------------

        /// <summary>
        /// This method returns the integer index of the next free (available) cursor object.
        /// </summary>
        /// <remarks>
        ///  The user is responsible for populating the values of the cursor object 
        ///  (e.g. hConn, hStmt etc). The user is also responsible for setting the field
        ///  cursorOpen to true before using it and false to release it.
        /// </remarks>
        /// <returns></returns>
        /// <para> - integer index of the next free cursor object.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        public static int GetNextFreeCursor()
        {
            //Get next free cursor area.
            int curHandle = -1;
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
                    GenUtil.SetErr("dynSite01 -- No more open cursors.");
                    return Error.NO_CURSOR_AVAILABLE;
                    //Application.Exit("\r\nDynMdbSite.GetNextFreeCursor(): ERROR: No available cursors.");
                }
            }
            else
            {
                //	Just use the next available cursor
                curHandle = nNextFreeCursor;
                //  And increment the free cursor count.
                nNextFreeCursor++;
            }

            //...Log2.v("\r\nDynMdbSite.GetNextFreeCursor(): found available cursor: curHandle = " + curHandle);
            cursors[curHandle].cursorOpen = true;

            return curHandle;
        }

        /// <summary>
        /// Selects records from a PDF TS Change of Call Sign table in the database 
        /// with canonical name <b>&lt;userID&gt;.ft_&lt;pdfName&gt;_chng</b> for
        /// the prescribed table name, SQL search criteria and ordering clauses; the 
        /// method returns an index to a cursor object that can be used by subsequent
        /// calls to FtFetchChngCall().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for FtFetchChngCall() calls.</para>
        public static int FtSelectChngCall(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynChange.FtSelectChngCall(): Entry");

            int curHandle;                  /* cursor handle */

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;
            //SQLSMALLINT nNameLen = 0;

            /* Construct the select clause */
            stmt_buf = select + table;

            /* Construct the 'where' part of the select clause */
            if (!String.IsNullOrWhiteSpace(searchCriteria) && !String.IsNullOrWhiteSpace(searchCriteria))
            {
                /* Search criteria was specified */
                stmt_buf += " where " + searchCriteria;
            }

            /* If "order by" is not specified then assume its for update */
            if (String.IsNullOrWhiteSpace(orderBy) || String.IsNullOrWhiteSpace(orderBy))
            {
                /* Construct the 'for update' part of the select clause. */
                //		strcat_s(stmt_buf, sizeof(stmt_buf), forUpdate);
            }
            else
            {
                /* Order by was specified */
                stmt_buf += " order by " + orderBy;
            }

            //	Get next free cursor area
            curHandle = GetNextFreeCursor();

            //	allocate and open the handle
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length);

            //	Inititialize the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].tableName = table;
            cursors[curHandle].cCurrentNewCall1 = "";

            //...Log2.v("\n\nDynChange.FtSelectChngCall(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Retrieves a single row of data from a PDF TS Change of Call Sign Table.
        /// </summary>
        /// <param name="nCursor"> - cursor object.</param>
        /// <param name="chngCall"> - a FtChng object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for chngCall.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int FtFetchChngCall(int nCursor, out FtChng chngCall, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynChange.FtFetchChngCall(): Entry");

            chngCall = new FtChng();
            nullInd = NullHelper.CreateArrayOfNullInd(FtChng.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[nCursor].hStmt;

            sqlRet = ODBC.SQLFetch(hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\r\nDynChange.FtSelectChngCall(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    //...Log2.v("\r\nDynChange.FtFetchChngCall(): SQLFetch() returned -1");
                    return Constant.FAILURE;
                }
            }

            //	Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "newcall1", out chngCall.newcall1, Constant.DB_CALL_SZ, out nullInd[0]);

                Ssutil.DbGetString(hStmt, 2, "oldcall1", out chngCall.oldcall1, Constant.DB_CALL_SZ, out nullInd[1]);

                Ssutil.DbGetString(hStmt, 3, "name", out chngCall.name, Constant.FT_SITE_NAME_SZ, out nullInd[2]);
            }
            catch (Exception e)
            {
                //...Log2.v("\r\nDynChange.FtFetchChngCall(): exception caught: returned -2");
                GenUtil.SetErr("dynChng02: Input error on field: " + e.Message);
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, hStmt);
                return Error.ODBC_GET_FAILED;
            }

            cursors[nCursor].cCurrentNewCall1 = chngCall.newcall1;

            //...Log2.v("\n\nDynChange.FtFetchChngCall(): Exit");
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Closes (releases) a cursor associated with a PDF TS Change of Call Sign record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the FtCursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        public static int FtCloseChngCall(int curHandle)
        {
            //...Log2.v("\n\nDynChange.FtCloseChngCall(): Entry");

            if (curHandle >= Constant.NUM_CURSORS_FEW || curHandle < 0)
            {
                /* Bad handle */
                //...Log2.v("\r\nDynChange.FtCloseChngCall(): bad handle: returned DYN_CUR_NOT_OPEN");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                //...Log2.v("\r\nDynChange.FtCloseChngCall(): cursor not open: returned DYN_CUR_NOT_OPEN");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            /* Close the site cursor */
            SQLHANDLE hStmt = cursors[curHandle].hStmt;
            //!!ODBC.SQLCloseCursor(hStmt);
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(cursors[curHandle].hConn);
            cursors[curHandle].hConn = IntPtr.Zero;
            cursors[curHandle].cursorOpen = false;

            //...Log2.v("\n\nDynChange.FtCloseChngCall(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Inserts a FtChng record into the database using column values prescribed 
        /// by the fields of the FtChng object.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object that encapsulates the details of the record insertion.</param>
        /// <param name="ftChngCall"> - a FtChng object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pCCal</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int FtInsertChngCall(int curHandle, FtChng ftChngCall, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynFtChng.FtInsertChngCall(): Entry");

            string cSQL;
            SQLRETURN sqlRet;

            SQLHANDLE hStmt;

            SQLHANDLE hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynFtChng.FtInsertChngCall(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

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
            cSQL = FtChng.BuildSqlInsertString(cursors[curHandle].tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pCCal
            //into global memory with an SQLPOINTER pointer assigned to each one. FtChng provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = ftChngCall.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                // Use automatic bind parameter indexing.
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(hStmt, 0, "newcall1", parameterValuePtr[FtChng.NEWCALL1], (SQLULEN)ftChngCall.newcall1.Length, nullIndPtr[FtChng.NEWCALL1]);

                Ssutil.DbBindStringInput(hStmt, 0, "oldcall1", parameterValuePtr[FtChng.OLDCALL1], (SQLULEN)ftChngCall.oldcall1.Length, nullIndPtr[FtChng.OLDCALL1]);

                Ssutil.DbBindStringInput(hStmt, 0, "name", parameterValuePtr[FtChng.NAME], (SQLULEN)ftChngCall.name.Length, nullIndPtr[FtChng.NAME]);
            }
            catch (Exception e)
            {
                Log2.e("\n\nDynFtChng.FtInsertChngCall(): ERROR: a call to DbBindStringInput() failed.");
                Ssutil.DbGetDiagStmt(hStmt, "ftInsertChngCall01 -- Error binding parameters for site on field " + e.Message);
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_BINDING_FAILED;
            }

            //...Log2.v("nDynFtChng.FtInsertChngCall(): SQLExecDirect():\r\n" + cSQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            //...Log2.v("\r\nDynFtChng.FtInsertChngCall(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynFtChng.FtInsertChngCall(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                string str = "ftInsertChngCall02 -- Error inserting Change Call for: " + ftChngCall.name;
                Ssutil.DbGetDiagStmt(hStmt, str); ;
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_EXECUTE_FAILED;
            }

            //...Log2.v("\n\nDynFtChng.FtInsertChngCall(): successfuly inserted record into table: " + cursors[curHandle].tableName);

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynFtChng.FtInsertChngCall(): Exit");
            return Constant.SUCCESS;
        }


    }
}

```
