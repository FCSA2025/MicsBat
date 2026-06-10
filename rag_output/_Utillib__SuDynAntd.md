# Documented File: SuDynAntd.cs
**Repository Path:** `_Utillib\SuDynAntd.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using System.Runtime.InteropServices;
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
    /// This class provides methods that serve SuAntd data to/from a database.
    /// </summary>
    public class SuDynAntd
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
                    Application.Exit("\r\nSuDynAntd.GetNextFreeCursor(): ERROR: No available cursors.");
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
        /// Selects antennae records from an antenna table in the database for
        /// the prescribed site table name, SQL search criteria and ordering clauses; the 
        /// method returns an index to a SuCursor object that can be used by subsequent
        /// calls to SuFetchAntd().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for SuFetchAntd() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int SuSelectAntd(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nSuDynAntd.SuSelectAntd(): Entry");
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the select clause.
            string stmt_buf = "select " + SuAntd.AllColumnsForSqlSelect + " from " + table;

            //Construct the 'where' part of the select clause.
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
            //...Log2.v("\r\nSuDynAntd.SuSelectAntd(): [1] sqlRet = " + sqlRet);

            //...Log2.v("\nSuDynAntd.SuSelectAntd(): query = " + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, ODBC.SQL_NTS);

            //...Log2.v("\r\nSuDynAntd.SuSelectAntd(): [4] sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "Could not Select Antenna for criteria: " + searchCriteria + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                Log2.e("\nSuDynAntd.SuSelectAntd(): ERROR: SQLExecute() failed");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //	Populate the cursor handle structure
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].tableName = table;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].sqlQuery = stmt_buf;

            //...Log2.v("\n\nSuDynAntd.SuSelectAntd(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Retrieves a single row of subsidiary antenna data from a table in the database using 
        /// a previously created Cursor object.
        /// </summary>
        /// <param name="curHandle"> - index of a Cursor object.</param>
        /// <param name="anteRec"> - a SuAntd object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for anteRec.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - SuCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int SuFetchAntd(int curHandle, out SuAntd anteRec, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nSuDynAntd.SuFetchAntd(): Entry:");

            //Create an SuAntd object to output.
            anteRec = new SuAntd();
            //Create an array of nulls to output.
            nullInd = new SQLLEN[SuAntd.NUM_COLUMNS];

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Log2.e("\n\nSuDynAntd.SuFetchAntd(): Exit: FAIL: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\n\nSuDynAntd.SuFetchAntd(): Exit: ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\n\nSuDynAntd.SuFetchAntd(): Exit: ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return Constant.FAILURE;
                }
            }

            // Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "cmd", out anteRec.cmd, SuAntd.CMD_SZ, out nullInd[SuAntd.CMD]);
                Ssutil.DbGetString(hStmt, 2, "acode", out anteRec.acode, SuAntd.ACODE_SZ, out nullInd[SuAntd.ACODE]);
                Ssutil.DbGetFloat(hStmt, 3, "antang", out anteRec.antang, out nullInd[SuAntd.ANTANG]);
                Ssutil.DbGetFloat(hStmt, 4, "dcov", out anteRec.dcov, out nullInd[SuAntd.DCOV]);
                Ssutil.DbGetFloat(hStmt, 5, "dxpv", out anteRec.dxpv, out nullInd[SuAntd.DXPV]);
                Ssutil.DbGetFloat(hStmt, 6, "dcoh", out anteRec.dcoh, out nullInd[SuAntd.DCOH]);
                Ssutil.DbGetFloat(hStmt, 7, "dxph", out anteRec.dxph, out nullInd[SuAntd.DXPH]);
                Ssutil.DbGetFloat(hStmt, 8, "dtilt", out anteRec.dtilt, out nullInd[SuAntd.DTILT]);
                Ssutil.DbGetInt(hStmt, 9, "interpstat", out anteRec.interpstat, out nullInd[SuAntd.INTERPSTAT]);
                Ssutil.DbGetString(hStmt, 10, "mdate", out anteRec.mdate, Constant.DATE_SZ, out nullInd[SuAntd.MDATE]);
                Ssutil.DbGetString(hStmt, 11, "mtime", out anteRec.mtime, Constant.TIME_SZ, out nullInd[SuAntd.MTIME]);

            }
            catch (Exception e)
            {
                Log2.e("\n\nSuDynAntd.SuFetchAntd(): ERROR: Exception caught: " + e.Message);
                GenUtil.SetErr("dynfeAntd02 -- Could not get antenna, because of field " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\nSuDynAntd.SuFetchAntd(): anteRec = \n" + anteRec.ToStringWN(nullInd));

            //	Save the key for deletes.
            cursors[curHandle].cCurrAcode = anteRec.acode;
            cursors[curHandle].fCurrAntang = anteRec.antang;

            //...Log2.v("\n\nSuDynAntd.SuFetchAntd(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Closes (releases) a cursor associated with a PDF ES antenna record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int SuCloseAntd(int curHandle)
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
        /// Inserts a SuAntd record into the database using column values prescribed 
        /// by the fields of the SuAntd object.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object that encapsulates the details of the record insertion.</param>
        /// <param name="suAntd"> - a SuAntd object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pAntd</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int SuInsertAntd(int curHandle, SuAntd suAntd, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynAntd.SuInsertAntd(): Entry");

            string cSQL;
            SQLRETURN sqlRet;

            SQLHANDLE hStmt;

            SQLHANDLE hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynAntd.SuInsertAntd(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            // Check the cursor for any nonsense.
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
            cSQL = SuAntd.BuildSqlInsertString(cursors[curHandle].tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAntd
            //into global memory with an SQLPOINTER pointer assigned to each one. SuAntd provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = suAntd.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {

                Ssutil.DbBindStringInput(hStmt, 1, "cmd", parameterValuePtr[SuAntd.CMD], SuAntd.CMD_SZ, nullIndPtr[SuAntd.CMD]);
                Ssutil.DbBindStringInput(hStmt, 2, "acode", parameterValuePtr[SuAntd.ACODE], SuAntd.ACODE_SZ, nullIndPtr[SuAntd.ACODE]);
                Ssutil.DbBindFloatInput(hStmt, 3, "antang", parameterValuePtr[SuAntd.ANTANG], nullIndPtr[SuAntd.ANTANG]);
                Ssutil.DbBindFloatInput(hStmt, 4, "dcov", parameterValuePtr[SuAntd.DCOV], nullIndPtr[SuAntd.DCOV]);
                Ssutil.DbBindFloatInput(hStmt, 5, "dxpv", parameterValuePtr[SuAntd.DXPV], nullIndPtr[SuAntd.DXPV]);
                Ssutil.DbBindFloatInput(hStmt, 6, "dcoh", parameterValuePtr[SuAntd.DCOH], nullIndPtr[SuAntd.DCOH]);
                Ssutil.DbBindFloatInput(hStmt, 7, "dxph", parameterValuePtr[SuAntd.DXPH], nullIndPtr[SuAntd.DXPH]);
                Ssutil.DbBindFloatInput(hStmt, 8, "dtilt", parameterValuePtr[SuAntd.DTILT], nullIndPtr[SuAntd.DTILT]);
                Ssutil.DbBindIntInput(hStmt, 9, "interpstat", parameterValuePtr[SuAntd.INTERPSTAT], nullIndPtr[SuAntd.INTERPSTAT]);
                Ssutil.DbBindStringInput(hStmt, 10, "mdate", parameterValuePtr[SuAntd.MDATE], SuAntd.MDATE_SZ, nullIndPtr[SuAntd.MDATE]);
                Ssutil.DbBindStringInput(hStmt, 11, "mtime", parameterValuePtr[SuAntd.MTIME], SuAntd.MTIME_SZ, nullIndPtr[SuAntd.MTIME]);

            }
            catch (Exception e)
            {
                Log2.e("\n\nDynAntd.SuInsertAntd(): ERROR: a call to DbBindStringInput() failed.");
                string str = String.Format("suInsertAntd03 -- Error binding parameters for antenna {0} on field: {1}", suAntd.acode, e.Message);
                Ssutil.DbGetDiagStmt(hStmt, str);
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_BINDING_FAILED;
            }

            //...Log2.v("nDynAntd.SuInsertAntd(): SQLExecDirect():\r\n" + cSQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            //...Log2.v("\r\nDynAntd.SuInsertAntd(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynAntd.SuInsertAntd(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                string str = String.Format("suInsertAntd02 -- Error inserting antenna: {0}", suAntd.acode);
                Ssutil.DbGetDiagStmt(hStmt, str); ;
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //...Log2.v("\n\nDynAntd.SuInsertAntd(): successfuly inserted record into table: " + cursors[curHandle].tableName);

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynAntd.SuInsertAntd(): Exit");
            return Constant.SUCCESS;
        }

#if false


        /// <summary>
        /// This method deletes an ES antenna record from the database.
        /// </summary>
        /// <param name="curHandle"> - a prescribed Cursor object, as returned by a previous call to SuSelectAntd().</param>
        /// <returns></returns>
        public static int SuDeleteAntd(int curHandle)
        {
            //...Log2.v("\n\nSuDynAntd.SuDeleteAntd(): Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hConn = Ssutil.NewConn();
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (cursors[curHandle].pastLastRow)
            {
                /* Cursor is past the last row */
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_PAST_LAST_ROW);
            }

            string delete_buf = String.Format("delete from {0} where location='{1}' and call21='{2}'",
                cursors[curHandle].tableName,
                cursors[curHandle].cCurrentLoc,
                cursors[curHandle].cCurrentCall1
                );

            //...Log2.v("\r\nSuDynAntd.FtDeleteAntdnna(): query = " + delete_buf);
            sqlRet = ODBC.SQLExecDirect(hUpdate, delete_buf, delete_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hUpdate, "ftDeleteAntd01 -- Error deleting from " + cursors[curHandle].tableName);
                Ssutil.DisConnStmt(hConn, hUpdate);
                Log2.e("\nSuDynAntd.SuDeleteAntd(): ERROR: SQLExecDirect() failed.");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            Ssutil.DisConnStmt(hConn, hUpdate);

            //...Log2.v("\n\nSuDynAntd.SuDeleteAntd(): Exit");
            return 0;
        }
#endif

        /// <summary>
        /// This method updates all of the column values of a record in a database ES antenna table.
        /// </summary>
        /// <param name="nCursor"> - a prescribed Cursor object, as returned by a previous call to SuSelectAntd().</param>
        /// <param name="suAntd"> - a prescribed SuAntd object that provides the updated field values.</param>
        /// <param name="nullInd"> - an array of ODBC nullInds associated with feAntd.</param>
        /// <returns></returns>
        public static int SuUpdateAntd(int nCursor, SuAntd suAntd, SQLLEN[] nullInd)
        {
            string update_buf;

            SQLRETURN sqlRet;
            SQLHANDLE hConn = Ssutil.NewConn();
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            // Check the cursor for any nonsense.
            if (!cursors[nCursor].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Log2.e("\nSuDynAntd.SuUpdateAntd(): ERROR: Cursor object has cursorOpen = false.");
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_CUR_NOT_OPEN);
            }
            if (cursors[nCursor].pastLastRow)
            {
                /* Cursor is past the last row */
                Log2.e("\nSuDynAntd.SuUpdateAntd(): ERROR: Cursor object has pastLastRow = true.");
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_PAST_LAST_ROW);
            }

            // Construct the update query.
            update_buf = String.Format("UPDATE {0} SET {1} WHERE acode='{2}' AND ABS(antang - {3}) < {4}",
                                        cursors[nCursor].tableName,
                                        SuAntd.AllColumnsForSqlUpdateAsBindings,
                                        suAntd.acode,
                                        suAntd.antang,
                                        360 * Constant.EPSILON_32);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAntd
            //into global memory with an SQLPOINTER pointer assigned to each one. FtAntd provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = suAntd.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                // Use auto-indexing of columns.
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(hUpdate, 0, "cmd", parameterValuePtr[SuAntd.CMD], SuAntd.CMD_SZ, nullIndPtr[SuAntd.CMD]);
                Ssutil.DbBindStringInput(hUpdate, 0, "acode", parameterValuePtr[SuAntd.ACODE], SuAntd.ACODE_SZ, nullIndPtr[SuAntd.ACODE]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "antang", parameterValuePtr[SuAntd.ANTANG], nullIndPtr[SuAntd.ANTANG]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "dcov", parameterValuePtr[SuAntd.DCOV], nullIndPtr[SuAntd.DCOV]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "dxpv", parameterValuePtr[SuAntd.DXPV], nullIndPtr[SuAntd.DXPV]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "dcoh", parameterValuePtr[SuAntd.DCOH], nullIndPtr[SuAntd.DCOH]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "dxph", parameterValuePtr[SuAntd.DXPH], nullIndPtr[SuAntd.DXPH]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "dtilt", parameterValuePtr[SuAntd.DTILT], nullIndPtr[SuAntd.DTILT]);
                Ssutil.DbBindIntInput(hUpdate, 0, "interpstat", parameterValuePtr[SuAntd.INTERPSTAT], nullIndPtr[SuAntd.INTERPSTAT]);
                Ssutil.DbBindStringInput(hUpdate, 0, "mdate", parameterValuePtr[SuAntd.MDATE], SuAntd.MDATE_SZ, nullIndPtr[SuAntd.MDATE]);
                Ssutil.DbBindStringInput(hUpdate, 0, "mtime", parameterValuePtr[SuAntd.MTIME], SuAntd.MTIME_SZ, nullIndPtr[SuAntd.MTIME]);

            }
            catch (Exception e)
            {
                Log2.e("\nSuDynAntd.SuUpdateAntd(): ERROR: ODBC Bind attempt failed: " + e.Message);
                Ssutil.DbGetDiagStmt(hUpdate, String.Format("suUpdateAntd03 -- Error binding parameters for Antenna {0} on field: {1}",
                                        suAntd.acode, e.Message));
                return -3;
            }

            //...Log2.v("\nSuDynAntd.SuUpdateAntd(): query = " + update_buf);

            sqlRet = ODBC.SQLExecDirect(hUpdate, update_buf, update_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = String.Format("\nSuDynAntd.SuUpdateAntd(): ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\n{1}", sqlRet, update_buf);
                Log2.e(str);
                Ssutil.DbGetDiagStmt(hUpdate, String.Format("suUpdateAntd02 -- Error inserting Antenna Point: {0}  {1}",
                                            suAntd.acode, suAntd.antang));
                return -4;
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);
            Ssutil.DisConn(hConn);

            return (0);
        }





    }
}


```
