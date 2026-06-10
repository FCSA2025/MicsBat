# Documented File: SuDynPlnd.cs
**Repository Path:** `_Utillib\SuDynPlnd.cs`
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
    /// This class provides methods that serve SuPlnd data to/from database [schema].su_XXX_plnd tables.
    /// </summary>
    public class SuDynPlnd
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
                    Application.Exit("\r\nSuDynPlnd.GetNextFreeCursor(): ERROR: No available cursors.");
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
        /// Selects SuPlnd records from an [schema].su_XXX_plnd table in the database for
        /// the prescribed SDF name (XXX), SQL search criteria and ordering clauses; the 
        /// method returns an index to a Cursor object that can be used by subsequent
        /// calls to SuFetchPlnd().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for SuFetchPlnd() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int SuSelectPlnd(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nSuDynPlnd.SuSelectPlnd(): Entry");
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the select clause.
            string stmt_buf = "select " + SuPlnd.AllColumnsForSqlSelect + " from " + table;

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

            //...Log2.v("\nSuDynPlnd.SuSelectPlnd(): query = " + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, ODBC.SQL_NTS);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "Could not Select Plnd for criteria: " + searchCriteria + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                Log2.e("\nSuDynPlnd.SuSelectPlnd(): ERROR: SQLExecute() failed");
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

            //...Log2.v("\n\nSuDynPlnd.SuSelectPlnd(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Retrieves a single row of SuPlnd data from a [schema].su_XXX_plnd table in the database using 
        /// a previously created Cursor object.
        /// </summary>
        /// <param name="curHandle"> - index of a Cursor object.</param>
        /// <param name="suPlnd"> - a SuPlnd object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for suPlnd.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - SuCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int SuFetchPlnd(int curHandle, out SuPlnd suPlnd, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nSuDynPlnd.SuFetchPlnd(): Entry:");

            //Create an SuPlnd object and associated nullInds.
            suPlnd = new SuPlnd();
            nullInd = new SQLLEN[SuPlnd.NUM_COLUMNS];

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Log2.e("\n\nSuDynPlnd.SuFetchPlnd(): Exit: FAIL: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\n\nSuDynPlnd.SuFetchPlnd(): Exit: ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\n\nSuDynPlnd.SuFetchPlnd(): Exit: ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return Constant.FAILURE;
                }
            }

            // Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "cmd", out suPlnd.cmd, SuPlnd.CMD_SZ, out nullInd[SuPlnd.CMD]);
                Ssutil.DbGetString(hStmt, 2, "recstat", out suPlnd.recstat, SuPlnd.RECSTAT_SZ, out nullInd[SuPlnd.RECSTAT]);
                Ssutil.DbGetString(hStmt, 3, "sband", out suPlnd.sband, SuPlnd.SBAND_SZ, out nullInd[SuPlnd.SBAND]);
                Ssutil.DbGetString(hStmt, 4, "splan", out suPlnd.splan, SuPlnd.SPLAN_SZ, out nullInd[SuPlnd.SPLAN]);
                Ssutil.DbGetShort(hStmt, 5, "spno", out suPlnd.spno, out nullInd[SuPlnd.SPNO]);
                Ssutil.DbGetDouble(hStmt, 6, "set1", out suPlnd.set1, out nullInd[SuPlnd.SET1]);
                Ssutil.DbGetString(hStmt, 7, "s1chid", out suPlnd.s1chid, SuPlnd.S1CHID_SZ, out nullInd[SuPlnd.S1CHID]);
                Ssutil.DbGetDouble(hStmt, 8, "set2", out suPlnd.set2, out nullInd[SuPlnd.SET2]);
                Ssutil.DbGetString(hStmt, 9, "s2chid", out suPlnd.s2chid, SuPlnd.S2CHID_SZ, out nullInd[SuPlnd.S2CHID]);
                Ssutil.DbGetDouble(hStmt, 10, "set3", out suPlnd.set3, out nullInd[SuPlnd.SET3]);
                Ssutil.DbGetString(hStmt, 11, "s3chid", out suPlnd.s3chid, SuPlnd.S3CHID_SZ, out nullInd[SuPlnd.S3CHID]);
                Ssutil.DbGetDouble(hStmt, 12, "set4", out suPlnd.set4, out nullInd[SuPlnd.SET4]);
                Ssutil.DbGetString(hStmt, 13, "s4chid", out suPlnd.s4chid, SuPlnd.S4CHID_SZ, out nullInd[SuPlnd.S4CHID]);
                Ssutil.DbGetString(hStmt, 14, "mdate", out suPlnd.mdate, SuPlnd.MDATE_SZ, out nullInd[SuPlnd.MDATE]);
                Ssutil.DbGetString(hStmt, 15, "mtime", out suPlnd.mtime, SuPlnd.MTIME_SZ, out nullInd[SuPlnd.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\n\nSuDynPlnd.SuFetchPlnd(): ERROR: Exception caught: " + e.Message);
                GenUtil.SetErr("dynSuPlnd02 -- Could not get Plnd record, because of field " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\nSuDynPlnd.SuFetchPlnd(): suPlnd = \n" + suPlnd.ToStringWN(nullInd));

            //...Log2.v("\n\nSuDynPlnd.SuFetchPlnd(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Closes (releases) a cursor associated with a SuPlnd record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int SuClosePlnd(int curHandle)
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
