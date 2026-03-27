using _Configuration;
using _DataStructures;
using _NewLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
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
    public class DynFeAzim
    {
        //===================================================================================================================================
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;
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
        /// Retrieves a single row of channel data from a table in the database using 
        /// a previously-created Cursor object.
        /// </summary>
        /// <param name="curHandle"> - index to a Cursor object.</param>
        /// <param name="azimRec"> - a FeAzim object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC out nullInds for azimRec.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int FeFetchAzim(int curHandle, out FeAzim azimRec, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynFeAzim.FeFetchAzim(): Entry:");

            //Create an FeAzim object to output.
            azimRec = new FeAzim();
            //Create an array of nulls to output.
            nullInd = new SQLLEN[FeAzim.NUM_COLUMNS];

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                //...Log2.v("\r\nDynFeAzim.FeFetchAzim(): FAIL: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // memset(azimRec, 0, sizeof(struct ftChan_));		/* clear to be safe */
            azimRec.Initialize();

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\r\nDynFeAzim.FeFetchAzim(): ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\r\nDynFeAzim.FeFetchAzim(): ERROR: ODBC.SQLFetch(): sqlRet = " + sqlRet);
                    return Constant.FAILURE;
                }
            }

            // Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "cmd", out azimRec.cmd, Constant.CMD_SZ, out nullInd[FeAzim.CMD]);
                Ssutil.DbGetString(hStmt, 2, "recstat", out azimRec.recstat, Constant.RECSTAT_SZ, out nullInd[FeAzim.RECSTAT]);
                Ssutil.DbGetString(hStmt, 3, "deleteall", out azimRec.deleteall, Constant.FE_AZIM_DELETEALL_SZ, out nullInd[FeAzim.DELETEALL]);
                Ssutil.DbGetString(hStmt, 4, "location", out azimRec.location, Constant.LOCATION_SZ, out nullInd[FeAzim.LOCATION]);
                Ssutil.DbGetString(hStmt, 5, "call1", out azimRec.call1, Constant.CALLSIGN_SZ, out nullInd[FeAzim.CALL1]);
                Ssutil.DbGetFloat(hStmt, 6, "azim", out azimRec.azim, out nullInd[FeAzim.AZIM]);
                Ssutil.DbGetFloat(hStmt, 7, "elev", out azimRec.elev, out nullInd[FeAzim.ELEV]);
                Ssutil.DbGetFloat(hStmt, 8, "dist", out azimRec.dist, out nullInd[FeAzim.DIST]);
                Ssutil.DbGetFloat(hStmt, 9, "loss", out azimRec.loss, out nullInd[FeAzim.LOSS]);
                Ssutil.DbGetString(hStmt, 10, "mdate", out azimRec.mdate, Constant.DATE_SZ, out nullInd[FeAzim.MDATE]);
                Ssutil.DbGetString(hStmt, 11, "mtime", out azimRec.mtime, Constant.TIME_SZ, out nullInd[FeAzim.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\r\nDynFeAzim.FeFetchAzim(): ERROR: Exception caught: " + e.Message);
                GenUtil.SetErr("feFetchAzim02: Input error on field: " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\nDynFeAzim.FeFetchAzim(): azimRec = \n" + azimRec.ToStringWN(nullInd));

            //	Save the key for deletes.
            cursors[curHandle].cLocation = azimRec.location;
            cursors[curHandle].cCall1 = azimRec.call1;
            cursors[curHandle].fAzim = azimRec.azim;

            //...Log2.v("\n\nDynFeAzim.FeFetchAzim(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Selects azimuth records from a FeAzim table in the database for
        /// the prescribed site table name, SQL search criteria and ordering clauses; the 
        /// method returns an index to a Cursor object that can be used by subsequent
        /// calls to FeFetchAzim().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for FeFetchAzim() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECUTE_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int FeSelectAzim(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynFeAzim.FeSelectAzim(): Entry");
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the select clause.
            string stmt_buf = "select " + FeAzim.AllColumnsForSqlSelect + " from " + table;

            //Construct the 'where' part of the select clause.
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                //Search criteria was specified.
                stmt_buf += " where " + searchCriteria;
            }

            //If "order by" is not specified then assume it's for update.
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                // Order By was specified.
                stmt_buf += " order by " + orderBy;
            }

            //Get next free cursor.
            curHandle = GetNextFreeCursor();

            //Make an new connection with the DB.
            SQLHDBC hConn = Ssutil.NewConn();

            //Get a statement handle.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            //Set the statement's attributes.
            sqlRet = ODBC.SQLSetStmtAttr(hStmt, ODBC.SQL_ATTR_CURSOR_TYPE, (SQLPOINTER)ODBC.SQL_CURSOR_DYNAMIC, 0);

            sqlRet = ODBC.SQLSetStmtAttr(hStmt, ODBC.SQL_ATTR_CONCURRENCY, (SQLPOINTER)ODBC.SQL_CONCUR_VALUES, 0);

            //...Log2.v("\n\nDynFeAzim.FeSelectAzim(): query = " + stmt_buf);
            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, ODBC.SQL_NTS);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "feSelectAzim Error:\n" + searchCriteria + "\n");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                Log2.e("\nDynFeAzim.FeSelectAzim(): ERROR: SQLExecute() failed.");
                return Error.ODBC_EXECUTE_FAILED;
            }

            //	Populate the cursor handle structure
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].tableName = table;
            cursors[curHandle].cLocation = "";
            cursors[curHandle].cCall1 = "";
            cursors[curHandle].fAzim = -1.0f;
            cursors[curHandle].sqlQuery = stmt_buf;

            //...Log2.v("\n\nDynFeAzim.FeSelectAzim(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Closes (releases) a cursor associated with a PDF ES azimuth record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int FeCloseAzim(int curHandle)
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
        /// Inserts a FeAzim record into the database using column values prescribed 
        /// by the fields of the FeAzim object.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object that encapsulates the details of the record insertion.</param>
        /// <param name="pAzim"> - a FeAzim object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pAzim</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int FeInsertAzim(int curHandle, FeAzim pAzim, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynFeAzim.FeInsertAzim(): Entry");

            string cSQL;
            SQLRETURN sqlRet;

            SQLHANDLE hStmt;

            SQLHANDLE hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynFeAzim.FeInsertAzim(): ERROR: call to SQLAllocHandle() failed.");
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
            cSQL = FeAzim.BuildSqlInsertString(cursors[curHandle].tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAzim
            //into global memory with an SQLPOINTER pointer assigned to each one. FeAzim provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = pAzim.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                // Use automatic bind parameter indexing.
                Ssutil.DbStartBinds();



                Ssutil.DbBindStringInput(hStmt, 0, "cmd", parameterValuePtr[FeAzim.CMD], (SQLULEN)pAzim.cmd.Length, nullIndPtr[FeAzim.CMD]);

                Ssutil.DbBindStringInput(hStmt, 0, "recstat", parameterValuePtr[FeAzim.RECSTAT], (SQLULEN)pAzim.recstat.Length, nullIndPtr[FeAzim.RECSTAT]);

                Ssutil.DbBindStringInput(hStmt, 0, "deleteall", parameterValuePtr[FeAzim.DELETEALL], (SQLULEN)pAzim.deleteall.Length, nullIndPtr[FeAzim.DELETEALL]);

                Ssutil.DbBindStringInput(hStmt, 0, "location", parameterValuePtr[FeAzim.LOCATION], (SQLULEN)pAzim.location.Length, nullIndPtr[FeAzim.LOCATION]);

                Ssutil.DbBindStringInput(hStmt, 0, "call1", parameterValuePtr[FeAzim.CALL1], (SQLULEN)pAzim.call1.Length, nullIndPtr[FeAzim.CALL1]);

                Ssutil.DbBindFloatInput(hStmt, 0, "azim", parameterValuePtr[FeAzim.AZIM], nullIndPtr[FeAzim.AZIM]);

                Ssutil.DbBindFloatInput(hStmt, 0, "elev", parameterValuePtr[FeAzim.ELEV], nullIndPtr[FeAzim.ELEV]);

                Ssutil.DbBindFloatInput(hStmt, 0, "dist", parameterValuePtr[FeAzim.DIST], nullIndPtr[FeAzim.DIST]);

                Ssutil.DbBindFloatInput(hStmt, 0, "loss", parameterValuePtr[FeAzim.LOSS], nullIndPtr[FeAzim.LOSS]);

                Ssutil.DbBindStringInput(hStmt, 0, "mdate", parameterValuePtr[FeAzim.MDATE], (SQLULEN)pAzim.mdate.Length, nullIndPtr[FeAzim.MDATE]);

                Ssutil.DbBindStringInput(hStmt, 0, "mtime", parameterValuePtr[FeAzim.MTIME], (SQLULEN)pAzim.mtime.Length, nullIndPtr[FeAzim.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\n\nDynFeAzim.FeInsertAzim(): ERROR: a call to DbBindStringInput() failed.");
                Ssutil.DbGetDiagStmt(hStmt, "feInsertAzim01 -- Error binding parameters for azim on field: " + e.Message);
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_BINDING_FAILED;
            }

            //...Log2.v("nDynFeAzim.FeInsertAzim(): SQLExecDirect():\r\n" + cSQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            //...Log2.v("\r\nDynFeAzim.FeInsertAzim(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynFeAzim.FeInsertAzim(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                string str = "feInsertAzim02 -- Error inserting azim";
                Ssutil.DbGetDiagStmt(hStmt, str); ;
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_EXECUTE_FAILED;
            }

            //...Log2.v("\n\nDynFeAzim.FeInsertAzim(): successfuly inserted record into table: " + cursors[curHandle].tableName);

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynFeAzim.FeInsertAzim(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method deletes an ES azimuth record from the database.
        /// </summary>
        /// <param name="curHandle"> - a prescribed Cursor object, as returned by a previous call to FeSelectAzim().</param>
        /// <returns></returns>
        public static int FeDeleteAzim(int curHandle)
        {
            //...Log2.v("\n\nDynFeAzim.FeDeleteAzim(): Entry");

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

            string delete_buf = String.Format("delete from {0} where location='{1}' and call1='{2}' and azim={3:F3}",
                cursors[curHandle].tableName,
                cursors[curHandle].cLocation,
                cursors[curHandle].cCall1,
                cursors[curHandle].fAzim
                );

            //...Log2.v("\nDynFeAzim.FeDeleteAzim(): query = " + delete_buf);
            sqlRet = ODBC.SQLExecDirect(hUpdate, delete_buf, delete_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = String.Format("feDeleteAzim01 -- Error deleting [{0}] from {1}",
                                            cursors[curHandle].cLocation, cursors[curHandle].tableName);
                Ssutil.DbGetDiagStmt(hUpdate, str);
                Ssutil.DisConnStmt(hConn, hUpdate);
                Log2.e("\nDynFeAnte.FeDeleteAnte(): ERROR: SQLExecDirect() failed.");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            Ssutil.DisConnStmt(hConn, hUpdate);

            //...Log2.v("\n\nDynFeAzim.FeDeleteAzim(): Exit");
            return 0;
        }

        private const string UPDATE = "update {0} set cmd= ?, recstat= ?, deleteall= ?, location= ?, call1= ?, azim= ?, elev= ?, dist= ?, loss= ?, mdate= ?, mtime= ?  where location='{1}' and call1='{2}' and azim={3:F3}";

        /// <summary>
        /// This method updates all of the column values of a record in a database ES azimuth table.
        /// </summary>
        /// <param name="nCursor"> - a prescribed Cursor object, as returned by a previous call to FeSelectAzim().</param>
        /// <param name="feAzim"> - a prescribed FeAzim object that provides the updated field values.</param>
        /// <param name="nullInd"> - an array of ODBC nullInds associated with feAzim.</param>
        /// <returns></returns>
        public static int FeUpdateAzim(int nCursor, FeAzim feAzim, SQLLEN[] nullInd)
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
                Log2.e("\nDynFeAzim.FeUpdateAzim(): ERROR: Cursor object has cursorOpen = false.");
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_CUR_NOT_OPEN);
            }
            if (cursors[nCursor].pastLastRow)
            {
                /* Cursor is past the last row */
                Log2.e("\nDynFeAzim.FeUpdateAzim(): ERROR: Cursor object has pastLastRow = true.");
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_PAST_LAST_ROW);
            }

            // Prepare the update statement
            update_buf = String.Format(UPDATE, cursors[nCursor].tableName, cursors[nCursor].cLocation, cursors[nCursor].cCall1, cursors[nCursor].fAzim);


            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAzim
            //into global memory with an SQLPOINTER pointer assigned to each one. FeAzim provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = feAzim.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                // Use auto-indexing of columns.
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(hUpdate, 0, "cmd", parameterValuePtr[FeAzim.CMD], FeAzim.CMD_SZ, nullIndPtr[FeAzim.CMD]);
                Ssutil.DbBindStringInput(hUpdate, 0, "recstat", parameterValuePtr[FeAzim.RECSTAT], FeAzim.RECSTAT_SZ, nullIndPtr[FeAzim.RECSTAT]);
                Ssutil.DbBindStringInput(hUpdate, 0, "deleteall", parameterValuePtr[FeAzim.DELETEALL], FeAzim.DELETEALL_SZ, nullIndPtr[FeAzim.DELETEALL]);
                Ssutil.DbBindStringInput(hUpdate, 0, "location", parameterValuePtr[FeAzim.LOCATION], FeAzim.LOCATION_SZ, nullIndPtr[FeAzim.LOCATION]);
                Ssutil.DbBindStringInput(hUpdate, 0, "call1", parameterValuePtr[FeAzim.CALL1], FeAzim.CALL1_SZ, nullIndPtr[FeAzim.CALL1]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "azim", parameterValuePtr[FeAzim.AZIM], nullIndPtr[FeAzim.AZIM]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "elev", parameterValuePtr[FeAzim.ELEV], nullIndPtr[FeAzim.ELEV]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "dist", parameterValuePtr[FeAzim.DIST], nullIndPtr[FeAzim.DIST]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "loss", parameterValuePtr[FeAzim.LOSS], nullIndPtr[FeAzim.LOSS]);
                Ssutil.DbBindStringInput(hUpdate, 0, "mdate", parameterValuePtr[FeAzim.MDATE], FeAzim.MDATE_SZ, nullIndPtr[FeAzim.MDATE]);
                Ssutil.DbBindStringInput(hUpdate, 0, "mtime	", parameterValuePtr[FeAzim.MTIME], FeAzim.MTIME_SZ, nullIndPtr[FeAzim.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\nDynFeAzim.FeUpdateAzim(): ERROR: ODBC Bind attempt failed: " + e.Message);
                Ssutil.DbGetDiagStmt(hUpdate, "dynAzim03 -- Error binding parameters for Azim on field: " + e.Message);
                return -3;
            }

            //...Log2.v("\nDynFeAzim.FeUpdateAzim(): query = " + update_buf);

            sqlRet = ODBC.SQLExecDirect(hUpdate, update_buf, update_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = String.Format("\nDynFeAzim.FeUpdateAzim(): ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\n{1}", sqlRet, update_buf);
                Log2.e(str);
                Ssutil.DbGetDiagStmt(hUpdate, "feUpdateAzim04 -- Error writing Azim: " + feAzim.location);
                return -4;
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);
            Ssutil.DisConn(hConn);

            return (0);
        }







    }
}
