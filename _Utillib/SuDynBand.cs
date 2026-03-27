using _Configuration;
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
    /// This class provides methods that serve SuBand objects to/from a database.
    /// </summary>
    public class SuDynBand
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
                    Application.Exit("\r\nSuDynBand.GetNextFreeCursor(): ERROR: No available cursors.");
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
        /// Selects SuBand records from a user's su_XXX_band DB table for
        /// the prescribed SDF name (XXX), SQL search criteria and ordering clauses; the 
        /// method returns an index to a Cursor object that can be used by subsequent
        /// calls to SuFetchBand().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for SuFetchBand() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int SuSelectBand(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nSuDynBand.SuSelectBand(): Entry");
            SQLHANDLE hStmt;
            SQLHANDLE hUpdate;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the SELECT clause.
            string stmt_buf = "SELECT " + SuBand.AllColumnsForSqlSelect + " FROM " + table;

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

            //...Log2.v("\r\nSuDynBand.SuSelectBand(): [1] sqlRet = " + sqlRet);

            //...Log2.v("\nSuDynBand.SuSelectBand(): query = " + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, ODBC.SQL_NTS);

            //...Log2.v("\r\nSuDynBand.SuSelectBand(): [4] sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "Could not Select Band for criteria: " + searchCriteria + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                Log2.e("\nSuDynBand.SuSelectBand(): ERROR: SQLExecute() failed");
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

            //...Log2.v("\n\nSuDynBand.SuSelectBand(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Retrieves a single row of SuBand data from a table in the database using 
        /// a Cursor object previously created by a call to SuSelectBand().
        /// </summary>
        /// <param name="curHandle"> - index of a Cursor object.</param>
        /// <param name="suBand"> - a SuBand object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for suBand.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - SuCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static short SuFetchBand(int curHandle, out SuBand suBand, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nSuDynBand.SuFetchBand(): Entry:");

            //Create an SuBand object to output.
            suBand = new SuBand();
            //Create an array of nulls to output.
            nullInd = new SQLLEN[SuBand.NUM_COLUMNS];

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Log2.e("\n\nSuDynBand.SuFetchBand(): Exit: FAIL: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\n\nSuDynBand.SuFetchBand(): Exit: ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\n\nSuDynBand.SuFetchBand(): Exit: ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return Error.DYN_MS_SQL_SERVER_ERR;
                }
            }

            // Now read in the fields
            try
            {

                Ssutil.DbGetString(hStmt, 1, "cmd", out suBand.cmd, SuBand.CMD_SZ, out nullInd[SuBand.CMD]);
                Ssutil.DbGetString(hStmt, 2, "recstat", out suBand.recstat, SuBand.RECSTAT_SZ, out nullInd[SuBand.RECSTAT]);
                Ssutil.DbGetString(hStmt, 3, "bndcde", out suBand.bndcde, SuBand.BNDCDE_SZ, out nullInd[SuBand.BNDCDE]);
                Ssutil.DbGetShort(hStmt, 4, "bandbitpos", out suBand.bandbitpos, out nullInd[SuBand.BANDBITPOS]);
                Ssutil.DbGetDouble(hStmt, 5, "blo", out suBand.blo, out nullInd[SuBand.BLO]);
                Ssutil.DbGetDouble(hStmt, 6, "bmidf", out suBand.bmidf, out nullInd[SuBand.BMIDF]);
                Ssutil.DbGetDouble(hStmt, 7, "bhi", out suBand.bhi, out nullInd[SuBand.BHI]);
                Ssutil.DbGetString(hStmt, 8, "badj", out suBand.badj, SuBand.BADJ_SZ, out nullInd[SuBand.BADJ]);
                Ssutil.DbGetString(hStmt, 9, "mdate", out suBand.mdate, SuBand.MDATE_SZ, out nullInd[SuBand.MDATE]);
                Ssutil.DbGetString(hStmt, 10, "mtime", out suBand.mtime, SuBand.MTIME_SZ, out nullInd[SuBand.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\n\nSuDynBand.SuFetchBand(): ERROR: Exception caught: " + e.Message);
                GenUtil.SetErr("dynfeBand02 -- Could not get band, because of field " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\nSuDynBand.SuFetchBand(): suBand = \n" + suBand.ToStringWN(nullInd));

            //	Save the key for deletes.
            cursors[curHandle].cCurrentBndcde = suBand.bndcde;

            //...Log2.v("\n\nSuDynBand.SuFetchBand(): Exit");
            return Constant.SUCCESS;
        }




        /// <summary>
        /// Closes (releases) a cursor instantiated by a previous call to SuSelectBand(); 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int SuCloseBand(int curHandle)
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
        /// as a prescribed SuBand object, exists in a prescribed DB table; if a matching record
        /// is found the method returns Constant.SUCCESS.
        /// </summary>
        /// <param name="tableName"> - prescribed DB table name.</param>
        /// <param name="suBand"> - prescribed SuBand object.</param>
        /// <returns></returns>
        public static int SuBandExist(string tableName, SuBand suBand)
        {
            //...Log2.v("\nSuDynBand(): Entry: acode = " + bandStruct.acode);

            SuBand tempStruct;
            SQLLEN[] tempNulls;
            SQLRETURN retVal;
            int cursorHandle;
            string searchCriteria;

            searchCriteria = String.Format("bndcde = '{0}'", suBand.bndcde);

            if ((cursorHandle = SuSelectBand(tableName, searchCriteria, "")) < 0)
            {
                return (cursorHandle);   /* in-conclusive result due to ERRORS */
            }

            retVal = SuFetchBand(cursorHandle, out tempStruct, out tempNulls);

            if (ODBC.IsOK(retVal))
            {
                // We found a matching record.
                SuCloseBand(cursorHandle);
                return (Constant.SUCCESS);
            }
            else // Check for NOMORERECS.
            {
                if (retVal == Constant.NOMORERECS)
                {
                    SuCloseBand(cursorHandle);
                    return (Constant.NOT_FOUND); /* this band record does not exist */
                }
                else
                {
                    // Something unexpected happened.
                    SuCloseBand(cursorHandle);
                    return (Error.DYN_MS_SQL_SERVER_ERR); /* this band record does not exist */
                }

            }
        }

        /// <summary>
        /// Inserts a SuBand record into the database using column values prescribed 
        /// by the fields of the SuBand object.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object that encapsulates the details of the record insertion.</param>
        /// <param name="suBand"> - a SuBand object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pBand</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int SuInsertBand(int curHandle, SuBand suBand, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynBand.SuInsertBand(): Entry");

            string cSQL;
            SQLRETURN sqlRet;

            SQLHANDLE hStmt;

            SQLHANDLE hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynBand.SuInsertBand(): ERROR: call to SQLAllocHandle() failed.");
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
            cSQL = SuBand.BuildSqlInsertString(cursors[curHandle].tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pBand
            //into global memory with an SQLPOINTER pointer assigned to each one. SuBand provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = suBand.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                Ssutil.DbBindStringInput(hStmt, 1, "cmd", parameterValuePtr[SuBand.CMD], SuBand.CMD_SZ, nullIndPtr[SuBand.CMD]);
                Ssutil.DbBindStringInput(hStmt, 2, "recstat", parameterValuePtr[SuBand.RECSTAT], SuBand.RECSTAT_SZ, nullIndPtr[SuBand.RECSTAT]);
                Ssutil.DbBindStringInput(hStmt, 3, "acode", parameterValuePtr[SuBand.ACODE], SuBand.ACODE_SZ, nullIndPtr[SuBand.ACODE]);
                Ssutil.DbBindIntInput(hStmt, 4, "axtype", parameterValuePtr[SuBand.AXTYPE], nullIndPtr[SuBand.AXTYPE]);
                Ssutil.DbBindStringInput(hStmt, 5, "axref", parameterValuePtr[SuBand.AXREF], SuBand.AXREF_SZ, nullIndPtr[SuBand.AXREF]);
                Ssutil.DbBindFloatInput(hStmt, 6, "again", parameterValuePtr[SuBand.AGAIN], nullIndPtr[SuBand.AGAIN]);
                Ssutil.DbBindFloatInput(hStmt, 7, "abw", parameterValuePtr[SuBand.ABW], nullIndPtr[SuBand.ABW]);
                Ssutil.DbBindShortInput(hStmt, 8, "arms", parameterValuePtr[SuBand.ARMS], nullIndPtr[SuBand.ARMS]);
                Ssutil.DbBindStringInput(hStmt, 9, "aband", parameterValuePtr[SuBand.ABAND], SuBand.ABAND_SZ, nullIndPtr[SuBand.ABAND]);
                Ssutil.DbBindStringInput(hStmt, 10, "amanu", parameterValuePtr[SuBand.AMANU], SuBand.AMANU_SZ, nullIndPtr[SuBand.AMANU]);
                Ssutil.DbBindStringInput(hStmt, 11, "apattern", parameterValuePtr[SuBand.APATTERN], SuBand.APATTERN_SZ, nullIndPtr[SuBand.APATTERN]);
                Ssutil.DbBindStringInput(hStmt, 12, "amodel", parameterValuePtr[SuBand.AMODEL], SuBand.AMODEL_SZ, nullIndPtr[SuBand.AMODEL]);
                Ssutil.DbBindShortInput(hStmt, 13, "anip", parameterValuePtr[SuBand.ANIP], nullIndPtr[SuBand.ANIP]);
                Ssutil.DbBindFloatInput(hStmt, 14, "ax0", parameterValuePtr[SuBand.AX0], nullIndPtr[SuBand.AX0]);
                Ssutil.DbBindStringInput(hStmt, 15, "adesc", parameterValuePtr[SuBand.ADESC], SuBand.ADESC_SZ, nullIndPtr[SuBand.ADESC]);
                Ssutil.DbBindStringInput(hStmt, 16, "antype", parameterValuePtr[SuBand.ANTYPE], SuBand.ANTYPE_SZ, nullIndPtr[SuBand.ANTYPE]);
                Ssutil.DbBindFloatInput(hStmt, 17, "aftbr", parameterValuePtr[SuBand.AFTBR], nullIndPtr[SuBand.AFTBR]);
                Ssutil.DbBindDoubleInput(hStmt, 18, "lofreq", parameterValuePtr[SuBand.LOFREQ], nullIndPtr[SuBand.LOFREQ]);
                Ssutil.DbBindDoubleInput(hStmt, 19, "hifreq", parameterValuePtr[SuBand.HIFREQ], nullIndPtr[SuBand.HIFREQ]);
                Ssutil.DbBindStringInput(hStmt, 20, "bandcodes", parameterValuePtr[SuBand.BANDCODES], SuBand.BANDCODES_SZ, nullIndPtr[SuBand.BANDCODES]);
                Ssutil.DbBindStringInput(hStmt, 21, "mdate", parameterValuePtr[SuBand.MDATE], SuBand.MDATE_SZ, nullIndPtr[SuBand.MDATE]);
                Ssutil.DbBindStringInput(hStmt, 22, "mtime", parameterValuePtr[SuBand.MTIME], SuBand.MTIME_SZ, nullIndPtr[SuBand.MTIME]);

            }
            catch (Exception e)
            {
                Log2.e("\n\nDynBand.SuInsertBand(): ERROR: a call to DbBindStringInput() failed.");
                string str = String.Format("suInsertBand03 -- Error binding parameters for bandnna {0} on field: {1}", suBand.acode, e.Message);
                Ssutil.DbGetDiagStmt(hStmt, str);
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_BINDING_FAILED;
            }

            //...Log2.v("nDynBand.SuInsertBand(): SQLExecDirect():\r\n" + cSQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            //...Log2.v("\r\nDynBand.SuInsertBand(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynBand.SuInsertBand(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                string str = String.Format("suInsertBand02 -- Error inserting bandnna: {0}", suBand.acode);
                Ssutil.DbGetDiagStmt(hStmt, str); ;
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //...Log2.v("\n\nDynBand.SuInsertBand(): successfuly inserted record into table: " + cursors[curHandle].tableName);

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynBand.SuInsertBand(): Exit");
            return Constant.SUCCESS;
        }
#endif




    }
}

