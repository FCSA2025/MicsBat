# Documented File: SuDynCtx.cs
**Repository Path:** `_Utillib\SuDynCtx.cs`
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
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides methods that serve SuCtx data to/from database [schema].su_XXX_ctx_ tables.
    /// </summary>
    public class SuDynCtx
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
                    Application.Exit("\r\nSuDynCtx.GetNextFreeCursor(): ERROR: No available cursors.");
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
        /// Selects SuCtx records from an [schema].su_XXX_ctx_ table in the database for
        /// the prescribed SDF name (XXX), SQL search criteria and ordering clauses; the 
        /// method returns an index to a Cursor object that can be used by subsequent
        /// calls to SuFetchCtx().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for SuFetchCtx() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int SuSelectCtx(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nSuDynCtx.SuSelectCtx(): Entry");
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the select clause.
            string stmt_buf = "select " + SuCtx.AllColumnsForSqlSelect + " from " + table;

            //Construct the 'where' part of the select clause.
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                //Search criteria was specified.
                //strcat_s(stmt_buf, sizeof(stmt_buf), "where ");
                //strcat_s(stmt_buf, sizeof(stmt_buf), searchCriteria);
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

            //Set the statement's attributes.
            sqlRet = ODBC.SQLSetStmtAttr(hStmt, ODBC.SQL_ATTR_CURSOR_TYPE, (SQLPOINTER)ODBC.SQL_CURSOR_DYNAMIC, 0);
            sqlRet = ODBC.SQLSetStmtAttr(hStmt, ODBC.SQL_ATTR_CONCURRENCY, (SQLPOINTER)ODBC.SQL_CONCUR_VALUES, 0);

            //...Log2.v("\nSuDynCtx.SuSelectCtx(): query = " + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, ODBC.SQL_NTS);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "Could not Select Ctx for criteria: " + searchCriteria + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                Log2.e("\nSuDynCtx.SuSelectCtx(): ERROR: SQLExecute() failed");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //	Populate the cursor handle structure
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].tableName = table;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cCurrentLoc = "";
            cursors[curHandle].cCurrentCall1 = "";
            cursors[curHandle].sqlQuery = stmt_buf;

            //...Log2.v("\n\nSuDynCtx.SuSelectCtx(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Retrieves a single row of SuCtx data from a [schema].su_XXX_ctx_ table in the database using 
        /// a previously created Cursor object.
        /// </summary>
        /// <param name="curHandle"> - index of a Cursor object.</param>
        /// <param name="suCtx"> - a SuCtx object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for suCtx.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - SuCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int SuFetchCtx(int curHandle, out SuCtx suCtx, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nSuDynCtx.SuFetchCtx(): Entry:");

            //Create an SuCtx object and associated nullInds.
            suCtx = new SuCtx();
            nullInd = new SQLLEN[SuCtx.NUM_COLUMNS];

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Log2.e("\n\nSuDynCtx.SuFetchCtx(): Exit: FAIL: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\n\nSuDynCtx.SuFetchCtx(): Exit: ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\n\nSuDynCtx.SuFetchCtx(): Exit: ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return Constant.FAILURE;
                }
            }

            // Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "cmd", out suCtx.cmd, SuCtx.CMD_SZ, out nullInd[SuCtx.CMD]);
                Ssutil.DbGetString(hStmt, 2, "recstat", out suCtx.recstat, SuCtx.RECSTAT_SZ, out nullInd[SuCtx.RECSTAT]);
                Ssutil.DbGetString(hStmt, 3, "tfcr", out suCtx.tfcr, SuCtx.TFCR_SZ, out nullInd[SuCtx.TFCR]);
                Ssutil.DbGetString(hStmt, 4, "tfci", out suCtx.tfci, SuCtx.TFCI_SZ, out nullInd[SuCtx.TFCI]);
                Ssutil.DbGetString(hStmt, 5, "rxeqp", out suCtx.rxeqp, SuCtx.RXEQP_SZ, out nullInd[SuCtx.RXEQP]);
                Ssutil.DbGetFloat(hStmt, 6, "rqco", out suCtx.rqco, out nullInd[SuCtx.RQCO]);
                Ssutil.DbGetFloat(hStmt, 7, "rqcull", out suCtx.rqcull, out nullInd[SuCtx.RQCULL]);
                Ssutil.DbGetFloat(hStmt, 8, "rqwrst", out suCtx.rqwrst, out nullInd[SuCtx.RQWRST]);
                Ssutil.DbGetShort(hStmt, 9, "ctxndp", out suCtx.ctxndp, out nullInd[SuCtx.CTXNDP]);
                Ssutil.DbGetString(hStmt, 10, "ctxdesc", out suCtx.ctxdesc, SuCtx.CTXDESC_SZ, out nullInd[SuCtx.CTXDESC]);
                Ssutil.DbGetString(hStmt, 11, "mdate", out suCtx.mdate, SuCtx.MDATE_SZ, out nullInd[SuCtx.MDATE]);
                Ssutil.DbGetString(hStmt, 12, "mtime", out suCtx.mtime, SuCtx.MTIME_SZ, out nullInd[SuCtx.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\n\nSuDynCtx.SuFetchCtx(): ERROR: Exception caught: " + e.Message);
                GenUtil.SetErr("dynSuCtx02 -- Could not get CTX record, because of field " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\nSuDynCtx.SuFetchCtx(): suCtx = \n" + suCtx.ToStringWN(nullInd));

            //...Log2.v("\n\nSuDynCtx.SuFetchCtx(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Closes (releases) a cursor associated with a SuCtx record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int SuCloseCtx(int curHandle)
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
