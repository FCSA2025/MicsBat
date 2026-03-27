using System;
using System.Text;
using _DataStructures;
using _NewLib;

namespace _Utillib
{
    using _Configuration;
    using System.Runtime.InteropServices;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides methods to select and fetch antenna records from the database table
    /// <b>web.user_tables</b>.
    /// </summary>
    public class DynUserTables
    {
        private const string mBaseTableName = "user_tables";

        private const string mDbTableName = "web." + mBaseTableName;

        private static string mTableName = mDbTableName;

        private const string mSELECT = "SELECT * FROM {0} ";

        private const string mUPDATE = "UPDATE {0} SET {1} WHERE operator='{2}' AND tabletype='{3}' AND file_name='{4}' ";

        private const string mINSERT = "INSERT INTO {0} VALUES ({1}) ";

        private const string mDELETE = "DELETE FROM {0} WHERE acode='{1}' ";

        //private const string mCHNG_UPDATE = "UPDATE {0} SET acode='{1}', mdate='{2}', mtime='{3}' WHERE acode='{4}' ";

        //=================================================================================================================
        // Using FtCursor here because it already exists and is a superset of what we need.
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        private static int nNextFreeCursor = 0;
        //=================================================================================================================
#if false
        // The reusable statement handle and the pointer arrays that will be bound to it.
        private static SQLHANDLE mhStmt = SQLHANDLE.Zero;
        private static SQLPOINTER[] mTgtValPtrs;
        private static SQLPOINTER[] mNullIndPtrs;

        // An integer that will be assigned to each separate call to 'select'
        // and will allow the subsequent 'fetch' to determine contiguity.
        private static int mMagicNumber = 0;
        //=================================================================================================================
#endif
        public static string TableName
        {
            get { return mTableName; }
        }

        /// <summary>
        /// This method returns the integer index of the next free (available) cursor object.
        /// </summary>
        /// <remarks>
        ///  The user is responsible for populating the values of the cursor object 
        ///  (e.g. hConn, hStmt etc). The user is also responsible for setting the field
        ///  cursorOpen to true before using it and false to release it.
        /// </remarks>
        /// <returns></returns>
        /// <para>- integer index of the next free cursor object.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        private static int GetNextFreeCursor()
        {
            //Get next free cursor area.
            int curHandle = -1;

            //...Log2.v("\r\nDynUserTables.GetNextFreeCursor(): nNextFreeCursor = " + nNextFreeCursor);
            if (nNextFreeCursor >= Constant.NUM_CURSORS_FEW)
            {
                //	Search for a free cursors in the list
                for (int nInd = 0; nInd < Constant.NUM_CURSORS_FEW; nInd++)
                {
                    //...Log2.v("\r\nDynUserTables.GetNextFreeCursor(): nInd = " + nInd);
                    if (!cursors[nInd].cursorOpen)
                    {
                        curHandle = nInd;
                        break;
                    }
                }

                if (curHandle == -1)
                {
                    // There are no more open cursors
                    GenUtil.SetErr("DynUserTables01 -- No more open cursors.");
                    Application.Exit("\r\nDynUserTables.GetNextFreeCursor(): ERROR: No available cursors.");
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

            //...Log2.v("\r\nDynUserTables.GetNextFreeCursor(): found available cursor: curHandle = " + curHandle);
            cursors[curHandle].cursorOpen = true;

            return curHandle;
        }

        /// <summary>
        /// Selects antenna records from the database table <b>main.sd_ante</b> for
        /// the prescribed SQL search criteria and ordering clauses; the 
        /// method returns an index to a cursor object that can be used by subsequent
        /// calls to Fetch().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for Fetch() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECUTE_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int Select(string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynUserTables.Select(): Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            int curHandle = GetNextFreeCursor();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nDynUserTables.Select(): ERROR : Exit: call to SQLAllocHandle() failed, sqlRet = " + sqlRet);
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            // Create the SQL query string.
            string stmt_buf = String.Format(mSELECT, mTableName);

            // Construct the 'where' part of the select clause.
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                // Search criteria was specified.
                stmt_buf += " WHERE " + searchCriteria;
            }

            // If "order by" is specified then add order by clause.
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                // Order by was specified.
                stmt_buf += " ORDER BY " + orderBy;
            }

            //...Log2.v("\r\nDynUserTables.Select(): stmt_buf = \r\n" + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nDynUserTables.Select(): ERROR : Exit: call to SQLExecDirect() failed, sqlRet = " + sqlRet);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            // Populate the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].hConn = hConn;

            //...Log2.v("\n\nDynUserTables.Select(): Exit");
            return (curHandle);
        }

        /// <summary>
        /// Retrieves a single row of antenna data from the database table <b>main.sd_ante</b> using 
        /// the cursor object created by a previous call to Select().
        /// </summary>
        /// <param name="nCursor"> - FtCursor object.</param>
        /// <param name="userTables"> - a UserTables object populated with data from the row.</param>
        /// <param name="nullInds"> - array of ODBC nullInds for userTables.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int Fetch(int nCursor, out UserTables userTables, out SQLLEN[] nullInds)
        {
            // 'out' requirments.
            userTables = null;
            nullInds = null;

            SQLHANDLE hStmt = cursors[nCursor].hStmt;

            //...Log2.v("\n\nDynUserTables.Fetch(): Entry");

            int nRet = -666;

            // Fetch the next row of selected Ante data.
            SQLRETURN sqlRet = ODBC.SQLFetch(hStmt);

            // Check the SQLRETURN and act accordingly.
            if (!ODBC.IsOK(sqlRet))
            {
                // If no more Ante records available then break out the while-loop.
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\n\nDynUserTables.Fetch(): Exit: sqlRet == ODBC.SQL_NO_DATA");
                    return ODBC.SQL_NO_DATA;
                }
                else
                {
                    // We have an error.
                    Log2.e("\n\nDynUserTables.Fetch(): Exit: ERROR: SQLFetch(): sqlRet = " + sqlRet);
                    Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, hStmt);
                    return -11;
                }
            }
            else
            {
                // We had a successful SQLFetch() operation; now get the record's column data.
                userTables = new UserTables();
                nullInds = NullHelper.CreateArrayOfNullInd(UserTables.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                try
                {
                    Ssutil.DbStartGets();
                    Ssutil.DbGetString(hStmt, 0, "operator", out userTables.oper, UserTables.OPER_SZ, out nullInds[UserTables.OPER]);
                    Ssutil.DbGetInt(hStmt, 0, "tabletype", out userTables.tabletype, out nullInds[UserTables.TABLETYPE]);
                    Ssutil.DbGetString(hStmt, 0, "file_name", out userTables.file_name, UserTables.FILE_NAME_SZ, out nullInds[UserTables.FILE_NAME]);
                    Ssutil.DbGetString(hStmt, 0, "micsid", out userTables.micsid, UserTables.MICSID_SZ, out nullInds[UserTables.MICSID]);
                    Ssutil.DbGetString(hStmt, 0, "project_code", out userTables.project_code, UserTables.PROJECT_CODE_SZ, out nullInds[UserTables.PROJECT_CODE]);
                    Ssutil.DbGetString(hStmt, 0, "validstat", out userTables.validstat, UserTables.VALIDSTAT_SZ, out nullInds[UserTables.VALIDSTAT]);
                    Ssutil.DbGetTimestamp(hStmt, 0, "create_date", out userTables.create_date, out nullInds[UserTables.CREATE_DATE]);

                    nRet = Constant.SUCCESS;
                }
                catch (Exception e)
                {
                    string str = String.Format("Fetch() failed for reason: " + e.Message);
                    GenUtil.SetError(1011, str);
                    Log2.e("\n\nDynUserTables.Fetch(): Exit: ERROR: call to ReadColBindings() threw an exception:\n" + e.Message);
                    return -22;
                } //try-catch

            } //if-else

            //...Log2.v(userTables.ToStringWN(nullInds));

            //...Log2.v("\n\nDynUserTables.Fetch(): Exit");
            return nRet;
        } //end of Fetch

        /// <summary>
        /// Closes (releases) a cursor associated with a antenna record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        public static int SdCloseAnte(int curHandle)
        {
            if (curHandle >= cursors.Length || curHandle < 0)
            {
                /* Bad handle */
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                return (Error.DYN_CUR_NOT_OPEN);
            }

            /* Close the cursor */
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, cursors[curHandle].hStmt);
            Ssutil.DisConn(cursors[curHandle].hConn);

            cursors[curHandle].hConn = IntPtr.Zero;
            cursors[curHandle].hStmt = IntPtr.Zero;
            cursors[curHandle].cursorOpen = false;

            return 0;
        }

        /// <summary>
        /// This method updates all of the column values in a prescribed record in the table web.user_tables
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="userTables"> - a prescribed UserTables object that provides the updated field values.</param>
        /// <param name="nullInds"> - an array of ODBC nullInds associated with userTables.</param>
        /// <returns></returns>
        public static int Update(SQLHANDLE hConn, UserTables userTables, SQLLEN[] nullInds)
        {
            //...Log2.v("\nDynUserTables.Update: Entry");

            string update_buf;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            // Prepare the update statement
            update_buf = String.Format(mUPDATE, mTableName, userTables.ToStringAsCSVequates(nullInds), userTables.oper, userTables.tabletype, userTables.file_name);

            //...Log2.v("\nDynUserTables.Update: query = " + update_buf);

            sqlRet = ODBC.SQLExecDirect(hStmt, update_buf, update_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = String.Format("\nDynUserTables.Update: ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\n{1}", sqlRet, update_buf);
                Log2.e(str);
                str = String.Format("user_tables -- Error updating record: {0}", userTables.ToString());

                Ssutil.DbGetDiagStmt(hStmt, str);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                return -4;
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\nDynUserTables.Update: Exit");
            return (Constant.SUCCESS);
        }
#if false
        /// <summary>
        /// Inserts a record into the SDB table main.sd_ante using column values prescribed 
        /// by the fields of an UserTables object.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="userTables"> - a UserTables object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for userTables</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int SdInsertAnte(SQLHANDLE hConn, UserTables userTables, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynUserTables.SdInsertAnte(): Entry");

            string cSQL;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynUserTables.SdInsertAnte(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            //Create the SQL insert statement.
            cSQL = String.Format(mINSERT, mTableName, userTables.ToStringAsCSV(nullInd));

            //...Log2.v("nDynUserTables.SdInsertAnte(): SQLExecDirect():\r\n" + cSQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynUserTables.SdInsertAnte(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                string str = "mtInsertAnte02 -- Error inserting antenna";
                Ssutil.DbGetDiagStmt(hStmt, str); ;
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //...Log2.v("\n\nDynUserTables.SdInsertAnte(): successfuly inserted record into table main.sd_ante");

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynUserTables.SdInsertAnte(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method deletes a record with a prescribed acode from the SDB
        /// antenna table main.sd_ante.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="location"> - a prescribed location string.</param>
        /// <param name="acode"> - a prescribed antenna code.</param>
        /// <returns></returns>
        public static int SdDeleteAnte(SQLHANDLE hConn, string acode)
        {
            //...Log2.v("\n\nDynUserTables.SdDeleteAnte(): Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string delete_buf = String.Format(mDELETE, mTableName, acode);

            //...Log2.v("\nDynUserTables.SdDeleteAnte(): query = " + delete_buf);

            sqlRet = ODBC.SQLExecDirect(hStmt, delete_buf, delete_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = String.Format("mtDeleteAnte01 -- Error deleting from main.sd_ante");
                Ssutil.DbGetDiagStmt(hStmt, str);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Log2.e("\nDynUserTables.SdDeleteAnte(): ERROR: SQLExecDirect() failed for query = " + delete_buf);
                return Error.ODBC_EXECDIRECT_FAILED;
            }


            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynUserTables.SdDeleteAnte(): Exit");
            return 0;
        }
#endif

        /// <summary>
        /// This method returns a populated UserTables object and associated ODBC nullINds
        /// i.a.w. the record fetched from the DB table 'web.user_tables' that has the prescribed key
        /// values for 'operatot', 'tabletype' and 'file_name'.
        /// </summary>
        /// <param name="oper"></param>
        /// <param name="tableType"></param>
        /// <param name="sdfName"></param>
        /// <param name="userTables"></param>
        /// <param name="nullInds"></param>
        /// <returns>True if a record with the prescribed key was found in the table; otherwise false.</returns>
        public static bool FetchRecordWithKeys(string oper, int tableType, string sdfName,
                                                out UserTables userTables,
                                                out SQLLEN[] nullInds)
        {
            // 'out' requirements.
            userTables = null;
            nullInds = null;

            string whereClause = String.Format(" operator='{0}' AND tabletype='{1}' AND file_name='{2}' ", oper, tableType, sdfName);

            int nCursor = Select(whereClause, "");
            if (nCursor < 0)
            {
                Log2.e("\nDynUserTable.FetchRecordWithKeys(): ERROR: Select() failed, nCursor = " + nCursor);
                return false;
            }

            int rc = Fetch(nCursor, out userTables, out nullInds);
            if (rc != Constant.SUCCESS)
            {
                Log2.e("\nDynUserTable.FetchRecordWithKeys(): ERROR: Fetch() failed, rc = " + rc);
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method returns true if a record exists in the SDB table <b>main.sd_ante</b>
        /// that has a prescribed antenna code (acode).
        /// </summary>
        /// <param name="oper"></param>
        /// <param name="tableType"></param>
        /// <param name="sdfName"></param>
        /// <returns></returns>
        public static bool SdbRecordExistsWithKeys(string oper, int tableType, string sdfName)
        {
            string whereClause = String.Format(" operator='{0}' AND tabletype='{1}' AND file_name='{2}' ", oper, tableType, sdfName);

            int nRecords = Ssutil.DbCountRows(mTableName, whereClause);

            return (nRecords > 0);
        }

    }
}











