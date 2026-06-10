# Documented File: SuDynEqpt.cs
**Repository Path:** `_Utillib\SuDynEqpt.cs`
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
    /// This class provides methods that serve SuEqpt objects to/from a database.
    /// </summary>
    public class SuDynEqpt
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
                    Application.Exit("\r\nSuDynEqpt.GetNextFreeCursor(): ERROR: No available cursors.");
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
        /// Selects SuEqpt records from a user's su_XXX_eqpt DB table for
        /// the prescribed SDF name (XXX), SQL search criteria and ordering clauses; the 
        /// method returns an index to a Cursor object that can be used by subsequent
        /// calls to SuFetchEqpt().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for SuFetchEqpt() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int SuSelectEqpt(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nSuDynEqpt.SuSelectEqpt(): Entry");
            SQLHANDLE hStmt;
            SQLHANDLE hUpdate;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the SELECT clause.
            string stmt_buf = "SELECT " + SuEqpt.AllColumnsForSqlSelect + " FROM " + table;

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

            //...Log2.v("\r\nSuDynEqpt.SuSelectEqpt(): [1] sqlRet = " + sqlRet);

            //...Log2.v("\nSuDynEqpt.SuSelectEqpt(): query = " + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, ODBC.SQL_NTS);

            //...Log2.v("\r\nSuDynEqpt.SuSelectEqpt(): [4] sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "Could not Select Eqpt for criteria: " + searchCriteria + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                Log2.e("\nSuDynEqpt.SuSelectEqpt(): ERROR: SQLExecute() failed");
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

            //...Log2.v("\n\nSuDynEqpt.SuSelectEqpt(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Retrieves a single row of SuEqpt data from a table in the database using 
        /// a Cursor object previously created by a call to SuSelectEqpt().
        /// </summary>
        /// <param name="curHandle"> - index of a Cursor object.</param>
        /// <param name="suEqpt"> - a SuEqpt object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for suEqpt.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - SuCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static short SuFetchEqpt(int curHandle, out SuEqpt suEqpt, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nSuDynEqpt.SuFetchEqpt(): Entry:");

            //Create an SuEqpt object to output.
            suEqpt = new SuEqpt();
            //Create an array of nulls to output.
            nullInd = new SQLLEN[SuEqpt.NUM_COLUMNS];

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Log2.e("\n\nSuDynEqpt.SuFetchEqpt(): Exit: FAIL: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\n\nSuDynEqpt.SuFetchEqpt(): Exit: ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\n\nSuDynEqpt.SuFetchEqpt(): Exit: ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return Error.DYN_MS_SQL_SERVER_ERR;
                }
            }

            // Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "cmd", out suEqpt.cmd, SuEqpt.CMD_SZ, out nullInd[SuEqpt.CMD]);
                Ssutil.DbGetString(hStmt, 2, "recstat", out suEqpt.recstat, SuEqpt.RECSTAT_SZ, out nullInd[SuEqpt.RECSTAT]);
                Ssutil.DbGetString(hStmt, 3, "ecode", out suEqpt.ecode, SuEqpt.ECODE_SZ, out nullInd[SuEqpt.ECODE]);
                Ssutil.DbGetFloat(hStmt, 4, "estab", out suEqpt.estab, out nullInd[SuEqpt.ESTAB]);
                Ssutil.DbGetString(hStmt, 5, "exref", out suEqpt.exref, SuEqpt.EXREF_SZ, out nullInd[SuEqpt.EXREF]);
                Ssutil.DbGetString(hStmt, 6, "emanu", out suEqpt.emanu, SuEqpt.EMANU_SZ, out nullInd[SuEqpt.EMANU]);
                Ssutil.DbGetString(hStmt, 7, "emodel", out suEqpt.emodel, SuEqpt.EMODEL_SZ, out nullInd[SuEqpt.EMODEL]);
                Ssutil.DbGetString(hStmt, 8, "edesc", out suEqpt.edesc, SuEqpt.EDESC_SZ, out nullInd[SuEqpt.EDESC]);
                Ssutil.DbGetString(hStmt, 9, "etype", out suEqpt.etype, SuEqpt.ETYPE_SZ, out nullInd[SuEqpt.ETYPE]);
                Ssutil.DbGetString(hStmt, 10, "etraf", out suEqpt.etraf, SuEqpt.ETRAF_SZ, out nullInd[SuEqpt.ETRAF]);
                Ssutil.DbGetString(hStmt, 11, "emission", out suEqpt.emission, SuEqpt.EMISSION_SZ, out nullInd[SuEqpt.EMISSION]);
                Ssutil.DbGetFloat(hStmt, 12, "e1stif", out suEqpt.e1stif, out nullInd[SuEqpt.E1STIF]);
                Ssutil.DbGetFloat(hStmt, 13, "e2ndif", out suEqpt.e2ndif, out nullInd[SuEqpt.E2NDIF]);
                Ssutil.DbGetFloat(hStmt, 14, "thhold", out suEqpt.thhold, out nullInd[SuEqpt.THHOLD]);
                Ssutil.DbGetString(hStmt, 15, "ebndcde", out suEqpt.ebndcde, SuEqpt.EBNDCDE_SZ, out nullInd[SuEqpt.EBNDCDE]);
                Ssutil.DbGetString(hStmt, 16, "mdate", out suEqpt.mdate, SuEqpt.MDATE_SZ, out nullInd[SuEqpt.MDATE]);
                Ssutil.DbGetString(hStmt, 17, "mtime", out suEqpt.mtime, SuEqpt.MTIME_SZ, out nullInd[SuEqpt.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\n\nSuDynEqpt.SuFetchEqpt(): ERROR: Exception caught: " + e.Message);
                GenUtil.SetErr("dynfeEqpt02 -- Could not get band, because of field " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\n\nSuDynEqpt.SuFetchEqpt(): Exit");
            return Constant.SUCCESS;
        }




        /// <summary>
        /// Closes (releases) a cursor instantiated by a previous call to SuSelectEqpt(); 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int SuCloseEqpt(int curHandle)
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

#if false
         /// <summary>
        /// This method checks whether a record, whose 'acode' column is the same
        /// as a prescribed SuEqpt object, exists in a prescribed DB table; if a matching record
        /// is found the method returns Constant.SUCCESS.
        /// </summary>
        /// <param name="tableName"> - prescribed DB table name.</param>
        /// <param name="suEqpt"> - prescribed SuEqpt object.</param>
        /// <returns></returns>
        public static int SuEqptExist(string tableName, SuEqpt suEqpt)
        {
            //...Log2.v("\nSuDynEqpt(): Entry: acode = " + bandStruct.acode);

            SuEqpt tempStruct;
            SQLLEN[] tempNulls;
            SQLRETURN retVal;
            int cursorHandle;
            string searchCriteria;

            searchCriteria = String.Format("bndcde = '{0}'", suEqpt.bndcde);

            if ((cursorHandle = SuSelectEqpt(tableName, searchCriteria, "")) < 0)
            {
                return (cursorHandle);   /* in-conclusive result due to ERRORS */
            }

            retVal = SuFetchEqpt(cursorHandle, out tempStruct, out tempNulls);

            if (ODBC.IsOK(retVal))
            {
                // We found a matching record.
                SuCloseEqpt(cursorHandle);
                return (Constant.SUCCESS);
            }
            else // Check for NOMORERECS.
            {
                if (retVal == Constant.NOMORERECS)
                {
                    SuCloseEqpt(cursorHandle);
                    return (Constant.NOT_FOUND); /* this band record does not exist */
                }
                else
                {
                    // Something unexpected happened.
                    SuCloseEqpt(cursorHandle);
                    return (Error.DYN_MS_SQL_SERVER_ERR); /* this band record does not exist */
                }

            }
        }

        /// <summary>
        /// Inserts a SuEqpt record into the database using column values prescribed 
        /// by the fields of the SuEqpt object.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object that encapsulates the details of the record insertion.</param>
        /// <param name="suEqpt"> - a SuEqpt object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pEqpt</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int SuInsertEqpt(int curHandle, SuEqpt suEqpt, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynEqpt.SuInsertEqpt(): Entry");

            string cSQL;
            SQLRETURN sqlRet;

            SQLHANDLE hStmt;

            SQLHANDLE hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynEqpt.SuInsertEqpt(): ERROR: call to SQLAllocHandle() failed.");
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
            cSQL = SuEqpt.BuildSqlInsertString(cursors[curHandle].tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pEqpt
            //into global memory with an SQLPOINTER pointer assigned to each one. SuEqpt provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = suEqpt.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                Ssutil.DbBindStringInput(hStmt, 1, "cmd", parameterValuePtr[SuEqpt.CMD], SuEqpt.CMD_SZ, nullIndPtr[SuEqpt.CMD]);
                Ssutil.DbBindStringInput(hStmt, 2, "recstat", parameterValuePtr[SuEqpt.RECSTAT], SuEqpt.RECSTAT_SZ, nullIndPtr[SuEqpt.RECSTAT]);
                Ssutil.DbBindStringInput(hStmt, 3, "acode", parameterValuePtr[SuEqpt.ACODE], SuEqpt.ACODE_SZ, nullIndPtr[SuEqpt.ACODE]);
                Ssutil.DbBindIntInput(hStmt, 4, "axtype", parameterValuePtr[SuEqpt.AXTYPE], nullIndPtr[SuEqpt.AXTYPE]);
                Ssutil.DbBindStringInput(hStmt, 5, "axref", parameterValuePtr[SuEqpt.AXREF], SuEqpt.AXREF_SZ, nullIndPtr[SuEqpt.AXREF]);
                Ssutil.DbBindFloatInput(hStmt, 6, "again", parameterValuePtr[SuEqpt.AGAIN], nullIndPtr[SuEqpt.AGAIN]);
                Ssutil.DbBindFloatInput(hStmt, 7, "abw", parameterValuePtr[SuEqpt.ABW], nullIndPtr[SuEqpt.ABW]);
                Ssutil.DbBindShortInput(hStmt, 8, "arms", parameterValuePtr[SuEqpt.ARMS], nullIndPtr[SuEqpt.ARMS]);
                Ssutil.DbBindStringInput(hStmt, 9, "aband", parameterValuePtr[SuEqpt.ABAND], SuEqpt.ABAND_SZ, nullIndPtr[SuEqpt.ABAND]);
                Ssutil.DbBindStringInput(hStmt, 10, "amanu", parameterValuePtr[SuEqpt.AMANU], SuEqpt.AMANU_SZ, nullIndPtr[SuEqpt.AMANU]);
                Ssutil.DbBindStringInput(hStmt, 11, "apattern", parameterValuePtr[SuEqpt.APATTERN], SuEqpt.APATTERN_SZ, nullIndPtr[SuEqpt.APATTERN]);
                Ssutil.DbBindStringInput(hStmt, 12, "amodel", parameterValuePtr[SuEqpt.AMODEL], SuEqpt.AMODEL_SZ, nullIndPtr[SuEqpt.AMODEL]);
                Ssutil.DbBindShortInput(hStmt, 13, "anip", parameterValuePtr[SuEqpt.ANIP], nullIndPtr[SuEqpt.ANIP]);
                Ssutil.DbBindFloatInput(hStmt, 14, "ax0", parameterValuePtr[SuEqpt.AX0], nullIndPtr[SuEqpt.AX0]);
                Ssutil.DbBindStringInput(hStmt, 15, "adesc", parameterValuePtr[SuEqpt.ADESC], SuEqpt.ADESC_SZ, nullIndPtr[SuEqpt.ADESC]);
                Ssutil.DbBindStringInput(hStmt, 16, "antype", parameterValuePtr[SuEqpt.ANTYPE], SuEqpt.ANTYPE_SZ, nullIndPtr[SuEqpt.ANTYPE]);
                Ssutil.DbBindFloatInput(hStmt, 17, "aftbr", parameterValuePtr[SuEqpt.AFTBR], nullIndPtr[SuEqpt.AFTBR]);
                Ssutil.DbBindDoubleInput(hStmt, 18, "lofreq", parameterValuePtr[SuEqpt.LOFREQ], nullIndPtr[SuEqpt.LOFREQ]);
                Ssutil.DbBindDoubleInput(hStmt, 19, "hifreq", parameterValuePtr[SuEqpt.HIFREQ], nullIndPtr[SuEqpt.HIFREQ]);
                Ssutil.DbBindStringInput(hStmt, 20, "bandcodes", parameterValuePtr[SuEqpt.BANDCODES], SuEqpt.BANDCODES_SZ, nullIndPtr[SuEqpt.BANDCODES]);
                Ssutil.DbBindStringInput(hStmt, 21, "mdate", parameterValuePtr[SuEqpt.MDATE], SuEqpt.MDATE_SZ, nullIndPtr[SuEqpt.MDATE]);
                Ssutil.DbBindStringInput(hStmt, 22, "mtime", parameterValuePtr[SuEqpt.MTIME], SuEqpt.MTIME_SZ, nullIndPtr[SuEqpt.MTIME]);

            }
            catch (Exception e)
            {
                Log2.e("\n\nDynEqpt.SuInsertEqpt(): ERROR: a call to DbBindStringInput() failed.");
                string str = String.Format("suInsertEqpt03 -- Error binding parameters for bandnna {0} on field: {1}", suEqpt.acode, e.Message);
                Ssutil.DbGetDiagStmt(hStmt, str);
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_BINDING_FAILED;
            }

            //...Log2.v("nDynEqpt.SuInsertEqpt(): SQLExecDirect():\r\n" + cSQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            //...Log2.v("\r\nDynEqpt.SuInsertEqpt(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynEqpt.SuInsertEqpt(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                string str = String.Format("suInsertEqpt02 -- Error inserting bandnna: {0}", suEqpt.acode);
                Ssutil.DbGetDiagStmt(hStmt, str); ;
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //...Log2.v("\n\nDynEqpt.SuInsertEqpt(): successfuly inserted record into table: " + cursors[curHandle].tableName);

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynEqpt.SuInsertEqpt(): Exit");
            return Constant.SUCCESS;
        }
#endif




    }
}


```
