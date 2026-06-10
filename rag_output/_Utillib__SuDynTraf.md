# Documented File: SuDynTraf.cs
**Repository Path:** `_Utillib\SuDynTraf.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _NewLib;
using System;

namespace _Utillib
{
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;
    using SQLLENPTR = IntPtr;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides methods that serve SuTraf objects to/from a database.
    /// </summary>
    public class SuDynTraf
    {
        //===================================================================================================================================
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;
        //===================================================================================================================================

        /// <summary>
        /// This method returns the integer index of the next free (available) Cursor object.
        /// </summary>
        /// <remarks>
        ///  The user is responsible for populating the values of the cursor object 
        ///  (e.g. hConn, hStmt etc). The user is also responsible for setting the field
        ///  cursorOpen to true before using it and false to release it.
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
                    Application.Exit("\r\nSuDynTraf.GetNextFreeCursor(): ERROR: No available cursors.");
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
        /// Selects SuTraf records from a user's su_XXX_traf DB table for
        /// the prescribed SDF name (XXX), SQL search criteria and ordering clauses; the 
        /// method returns an index to a Cursor object that can be used by subsequent
        /// calls to SuFetchTraf().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for SuFetchTraf() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int SuSelectTraf(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v.v("\n\nSuDynTraf.SuSelectTraf(): Entry");
            SQLHANDLE hStmt;
            SQLHANDLE hUpdate;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the SELECT clause.
            string stmt_buf = "SELECT " + SuTraf.AllColumnsForSqlSelect + " FROM " + table;

            //Construct the 'where' part of the SELECT clause.
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                stmt_buf += " where " + searchCriteria;
            }

            //If "order by" is not specified then assume it's for update.
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                // Order by was specified.
                stmt_buf += " order by " + orderBy;
            }

            //Get a Cursor object.
            curHandle = GetNextFreeCursor();

            //Make an new connection with the DB.
            SQLHDBC hConn = Ssutil.NewConn();

            //Get a statement handle.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            //Separate statement handle for update.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            //...Log2.v.v("\r\nSuDynTraf.SuSelectTraf(): [1] sqlRet = " + sqlRet);

            //...Log2.v.v("\nSuDynTraf.SuSelectTraf(): query = " + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, ODBC.SQL_NTS);

            //...Log2.v.v("\r\nSuDynTraf.SuSelectTraf(): [4] sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "Could not Select Traf for criteria: " + searchCriteria + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                //...Log2.v.e("\nSuDynTraf.SuSelectTraf(): ERROR: SQLExecute() failed");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //	Populate the cursor handle structure
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].cCurrAcode = "";
            cursors[curHandle].tableName = table;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hUpdate = hUpdate;
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].sqlQuery = stmt_buf;

            //...Log2.v.v("\n\nSuDynTraf.SuSelectTraf(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Retrieves a single row of SuTraf data from a table in the database using 
        /// a Cursor object previously created by a call to SuSelectTraf().
        /// </summary>
        /// <param name="curHandle"> - index of a Cursor object.</param>
        /// <param name="suTraf"> - a SuTraf object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for suTraf.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - SuCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static short SuFetchTraf(int curHandle, out SuTraf suTraf, out SQLLEN[] nullInd)
        {
            //...Log2.v.v("\n\nSuDynTraf.SuFetchTraf(): Entry:");

            //Create an SuTraf object to output.
            suTraf = new SuTraf();
            //Create an array of nulls to output.
            nullInd = new SQLLEN[SuTraf.NUM_COLUMNS];

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                //...Log2.v.e("\n\nSuDynTraf.SuFetchTraf(): Exit: FAIL: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v.v("\n\nSuDynTraf.SuFetchTraf(): Exit: ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return sqlRet;
                }
                else
                {
                    //	Error
                    //...Log2.v.e("\n\nSuDynTraf.SuFetchTraf(): Exit: ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return Error.DYN_MS_SQL_SERVER_ERR;
                }
            }

            // Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "cmd", out suTraf.cmd, SuTraf.CMD_SZ, out nullInd[SuTraf.CMD]);
                Ssutil.DbGetString(hStmt, 2, "recstat", out suTraf.recstat, SuTraf.RECSTAT_SZ, out nullInd[SuTraf.RECSTAT]);
                Ssutil.DbGetString(hStmt, 3, "trafcode", out suTraf.trafcode, SuTraf.TRAFCODE_SZ, out nullInd[SuTraf.TRAFCODE]);
                Ssutil.DbGetString(hStmt, 4, "ecode", out suTraf.ecode, SuTraf.ECODE_SZ, out nullInd[SuTraf.ECODE]);
                Ssutil.DbGetString(hStmt, 5, "xreftrcde", out suTraf.xreftrcde, SuTraf.XREFTRCDE_SZ, out nullInd[SuTraf.XREFTRCDE]);
                Ssutil.DbGetString(hStmt, 6, "xrefeqcde", out suTraf.xrefeqcde, SuTraf.XREFEQCDE_SZ, out nullInd[SuTraf.XREFEQCDE]);
                Ssutil.DbGetString(hStmt, 7, "trdesc", out suTraf.trdesc, SuTraf.TRDESC_SZ, out nullInd[SuTraf.TRDESC]);
                Ssutil.DbGetString(hStmt, 8, "mdate", out suTraf.mdate, SuTraf.MDATE_SZ, out nullInd[SuTraf.MDATE]);
                Ssutil.DbGetString(hStmt, 9, "mtime", out suTraf.mtime, SuTraf.MTIME_SZ, out nullInd[SuTraf.MTIME]);
            }
            catch (Exception e)
            {
                //...Log2.v.e("\n\nSuDynTraf.SuFetchTraf(): ERROR: Exception caught: " + e.Message);
                GenUtil.SetErr("dynfeTraf02 -- Could not get traf, because of field " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v.v("\n\nSuDynTraf.SuFetchTraf(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Closes (releases) a cursor instantiated by a previous call to SuSelectTraf(); 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int SuCloseTraf(int curHandle)
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






    }
}





```
