using System;
using System.Text;
using _DataStructures;
using _NewLib;
using _Configuration;

namespace _Utillib
{

    using System.Runtime.InteropServices;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides methods to select and fetch CTXD records from the subsiduary database table
    /// <b>main.sd_plnd</b>.
    /// </summary>
    public class DynSdbPlnd
    {
        private const string mBaseTableName = "sd_plnd";

        private const string mMdbTableName = "main." + mBaseTableName;

        private static string mTableName = mMdbTableName;

        private static bool mMdbWriteEnabled = true;

        private static bool mSpoofModeIsOff = true;

        private const string mSELECT = "SELECT * FROM {0} ";

        private const string mUPDATE = "UPDATE {0} SET {1} WHERE sband='{2}' AND splan='{3}' AND spno='{4}' ";

        private const string mINSERT = "INSERT INTO {0} VALUES ({1}) ";

        private const string mDELETE_ONE = "DELETE FROM {0} WHERE sband='{1}' AND splan='{2}' AND spno='{3}' ";

        private const string mDELETE_SET = "DELETE FROM {0} WHERE sband='{1}' AND splan='{2}' ";

        //=================================================================================================================
        // Using FtCursor here because it already exists and is a superset of what we need.
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        private static int nNextFreeCursor = 0;
        //=================================================================================================================

        // The reusable statement handle and the pointer arrays that will be bound to it.
        private static SQLHANDLE mhStmt = SQLHANDLE.Zero;
        private static SQLPOINTER[] mTgtValPtrs;
        private static SQLPOINTER[] mNullIndPtrs;

        // An integer that will be assigned to each separate call to 'select'
        // and will allow the subsequent 'fetch' to determine contiguity.
        private static int mMagicNumber = 0;
        //=================================================================================================================

        public static string TableName
        {
            get { return mTableName; }
        }

        /// <summary>
        /// This method can be used to set whether the SDB <b>main.sd_plnd</b> table should be written to,
        /// (SQL DELETE, INSERT and/or UPDATE queries) or not; it can also set whether the SQL queries should
        /// target the actual main.sd_plnd table or a user's 'spoof' table (e.g. hulme.sd_plnd).
        /// </summary>
        /// <param name="mdbWriteEnabled"> - boolean that prescribes whether sd_plnd record
        /// DELETE, INSERT and/or UPDATE operations will be performed (the default) or inhibited.</param>
        /// <param name="spoofModeIsOff"> - boolean that prescribes whether 'spoof' mode is enabled or disabled (the default).</param>
        /// <param name="userSchema"> - the user's prescribed SQL Server schema name.</param>
        public static void SetState(bool mdbWriteEnabled, bool spoofModeIsOff, string userSchema)
        {
            mMdbWriteEnabled = mdbWriteEnabled;
            mSpoofModeIsOff = spoofModeIsOff;


            if (spoofModeIsOff)
            {
                mTableName = mMdbTableName;
            }
            else
            {
                if (String.IsNullOrWhiteSpace(userSchema))
                {
                    Log2.e("\nDynSdbPlnd.SetState(): ERROR: spoofMode is ON but userSchema is null or whitespace.");
                    // Set the mTableName to something nonsensical so that all subsequent
                    // SQL queries will fail because the table does not exist.
                    mTableName = "idonotexist" + "." + mBaseTableName;
                }
                else
                {
                    mTableName = userSchema + "." + mBaseTableName;
                }

            }
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

            //...Log2.v("\r\nDynSdbPlnd.GetNextFreeCursor(): nNextFreeCursor = " + nNextFreeCursor);
            if (nNextFreeCursor >= Constant.NUM_CURSORS_FEW)
            {
                //	Search for a free cursors in the list
                for (int nInd = 0; nInd < Constant.NUM_CURSORS_FEW; nInd++)
                {
                    //...Log2.v("\r\nDynSdbPlnd.GetNextFreeCursor(): nInd = " + nInd);
                    if (!cursors[nInd].cursorOpen)
                    {
                        curHandle = nInd;
                        break;
                    }
                }

                if (curHandle == -1)
                {
                    // There are no more open cursors
                    GenUtil.SetErr("DynSdbPlnd01 -- No more open cursors.");
                    Application.Exit("\r\nDynSdbPlnd.GetNextFreeCursor(): ERROR: No available cursors.");
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

            //...Log2.v("\r\nDynSdbPlnd.GetNextFreeCursor(): found available cursor: curHandle = " + curHandle);
            cursors[curHandle].cursorOpen = true;

            return curHandle;
        }

        /// <summary>
        /// Selects CTX records from the database table <b>main.sd_plnd</b> for
        /// the prescribed SQL search criteria and ordering clauses; the 
        /// method returns an index to a cursor object that can be used by subsequent
        /// calls to SdFetchPlnd().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for SdFetchPlnd() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECUTE_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int SdSelectPlnd(string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynSdbPlnd.SdSelectPlnd(): Entry");

            SQLRETURN sqlRet;
            SQLHDBC hConn = Ssutil.NewConn();

            int curHandle = GetNextFreeCursor();

            // If this is the very first time that this 'select' method is 
            // called then we must instantiate the reusable statement handle
            // and establish the bindings to it.
            if (mhStmt == SQLHANDLE.Zero)
            {
                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out mhStmt);

                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\nDynSdbPlnd.SdSelectPlnd(): ERROR : Exit: call to SQLAllocHandle() failed, sqlRet = " + sqlRet);
                    return Error.ODBC_SQLALLOCHANDLE_FAILED;
                }

                SdPlnd.BindPtrsToCols(mhStmt, out mTgtValPtrs, out mNullIndPtrs);
            }

            // If an active cursor still exists from a previous result set and we try to 
            // re-use the statement handle in a subsequent SQLExec() or SQLExecDirect() to
            // select, fetch or insert tables then we will get the following error:
            // DIAG [24000] [Microsoft][SQL Server Native Client 11.0]Invalid cursor state (0).
            //
            // We need to close the statement handle's cursor before reusing the statement.
            // Calling SQLFreeStmt with the SQL_CLOSE option is equivalent to calling
            // SQLCloseCursor, except that SQLFreeStmt with SQL_CLOSE does not affect
            // the application if no cursor is open on the statement. If no cursor is
            // open, a call to SQLCloseCursor returns SQLSTATE 24000(Invalid cursor state).

            sqlRet = ODBC.SQLFreeStmt(mhStmt, ODBC.SQL_CLOSE);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nDynSdbPlnd.SdSelectPlnd(): ERROR : Exit: Attempt to close the cursor on the reusable statement handle failed.");
                Application.Exit(666);
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

            //...Log2.v("\r\nDynSdbPlnd.SdSelectPlnd(): stmt_buf = \r\n" + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(mhStmt, stmt_buf, stmt_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nDynSdbPlnd.SdSelectPlnd(): ERROR : Exit: call to SQLExecDirect() failed, sqlRet = " + sqlRet);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            mMagicNumber++;

            // Populate the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = SQLHANDLE.Zero;
            cursors[curHandle].hConn = SQLHANDLE.Zero;
            cursors[curHandle].magicNumber = mMagicNumber;

            //...Log2.v("\n\nDynSdbPlnd.SdSelectPlnd(): Exit");
            return (curHandle);
        }

        /// <summary>
        /// Retrieves a single row of CTX data from the database table <b>main.sd_plnd</b> using 
        /// the cursor object created by a previous call to SdSelectPlnd().
        /// </summary>
        /// <param name="nCursor"> - FtCursor object.</param>
        /// <param name="sdPlnd"> - a SdPlnd object populated with data from the row.</param>
        /// <param name="nullInds"> - array of ODBC nullInds for sdPlnd.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int SdFetchPlnd(int nCursor, out SdPlnd sdPlnd, out SQLLEN[] nullInds)
        {
            // 'out' requirments.
            sdPlnd = null;
            nullInds = null;

            //...Log2.v("\n\nDynSdbPlnd.SdFetchPlnd(): Entry");

            FetchCount.IncrementSdCtxd();

            int nRet = -666;

            // Test for contiguity, i.e. that the previous select and 
            // this fetch are a matched pair.
            if (cursors[nCursor].magicNumber != mMagicNumber)
            {
                Log2.e("\n\nDynSdbPlnd.SdFetchPlnd(): Exit: ERROR: failed contiguity test.");

                return -666;
            }

            // Fetch the next row of selected Plnd data.
            SQLRETURN sqlRet = ODBC.SQLFetch(mhStmt);

            // Check the SQLRETURN and act accordingly.
            if (!ODBC.IsOK(sqlRet))
            {
                // If no more Plnd records available then break out the while-loop.
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\n\nDynSdbPlnd.SdFetchPlnd(): Exit: sqlRet == ODBC.SQL_NO_DATA");

                    // If an active cursor still exists on the handle from a previous result set
                    // and we try to use it in a subsequent SQLExec() or SQLExecDirect() to 
                    // select, fetch or insert tables then we will get the following error:
                    // DIAG [24000] [Microsoft][SQL Server Native Client 11.0]Invalid cursor state (0).
                    //
                    // We need to close the statement handle's cursor before reusing the statement.
                    // Calling SQLFreeStmt with the SQL_CLOSE option is equivalent to calling 
                    // SQLCloseCursor, except that SQLFreeStmt with SQL_CLOSE does not affect 
                    // the application if no cursor is open on the statement. If no cursor is 
                    // open, a call to SQLCloseCursor returns SQLSTATE 24000(Invalid cursor state).

                    return ODBC.SQL_NO_DATA;
                }
                else
                {
                    // We have an error.
                    Log2.e("\n\nDynSdbPlnd.SdFetchPlnd(): Exit: ERROR: SQLFetch(): sqlRet = " + sqlRet);
                    Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, mhStmt);
                    return -11;
                }
            }
            else
            {
                // We had a successful SQLFetch() operation; now get the record's column data.
                try
                {
                    SdPlnd.ReadColBindings(mTgtValPtrs, mNullIndPtrs, out sdPlnd, out nullInds);

                    nRet = Constant.SUCCESS;
                }
                catch (Exception e)
                {
                    string str = String.Format("Fetch() failed for reason: " + e.Message);
                    GenUtil.SetError(1011, str);
                    Log2.e("\n\nDynSdbPlnd.SdFetchPlnd(): Exit: ERROR: call to ReadColBindings() threw an exception:\n" + e.Message);
                    return -22;
                } //try-catch

            } //if-else

            //...Log2.v(sdPlnd.ToStringWN(nullInds));

            //...Log2.v("\n\nDynSdbPlnd.SdFetchPlnd(): Exit");
            return nRet;
        } //end of SdFetchPlnd

        /// <summary>
        /// Closes (releases) a cursor associated with a CTX record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        public static int SdClosePlnd(int curHandle)
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
            Ssutil.DisConn(cursors[curHandle].hConn);
            cursors[curHandle].hConn = IntPtr.Zero;
            cursors[curHandle].cursorOpen = false;

            return 0;
        }

        /// <summary>
        /// This method updates all of the column values of a prescribed record in the SDB CTX table
        /// main.sd_plnd.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="sdPlnd"> - a prescribed SdPlnd object that provides the updated field values.</param>
        /// <param name="nullInds"> - an array of ODBC nullInds associated with sdPlnd.</param>
        /// <returns></returns>
        public static int SdUpdatePlnd(SQLHANDLE hConn, SdPlnd sdPlnd, SQLLEN[] nullInds)
        {
            //...Log2.v("\nDynSdbPlnd.SdUpdatePlnd(): Entry");

            string update_buf;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            // Prepare the update statement
            update_buf = String.Format(mUPDATE, mTableName, sdPlnd.ToStringAsCSVequates(nullInds), sdPlnd.sband, sdPlnd.splan, sdPlnd.spno);

            //...Log2.v("\nDynSdbPlnd.SdUpdatePlnd(): query = " + update_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, update_buf, update_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("\nDynSdbPlnd.SdUpdatePlnd(): ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\n{1}", sqlRet, update_buf);
                    Log2.e(str);
                    str = String.Format("mtUpdatePlnd04 -- Error writing Plnd: {0}", sdPlnd.ToString());

                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return -4;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + update_buf;
                //...Log2.v("\nDynSdbPlnd.SdUpdatePlnd(): Updating of main.sd_plnd records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\nDynSdbPlnd.SdUpdatePlnd(): Exit");
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Inserts a record into the SDB table main.sd_plnd using column values prescribed 
        /// by the fields of an SdPlnd object.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="sdPlnd"> - a SdPlnd object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for sdPlnd</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int SdInsertPlnd(SQLHANDLE hConn, SdPlnd sdPlnd, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynSdbPlnd.SdInsertPlnd(): Entry");

            string cSQL;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynSdbPlnd.SdInsertPlnd(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            //Create the SQL insert statement.
            cSQL = String.Format(mINSERT, mTableName, sdPlnd.ToStringAsCSV(nullInd));

            //...Log2.v("nDynSdPlnd.SdInsertPlnd(): SQLExecDirect():\r\n" + cSQL);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\n\nDynSdbPlnd.SdInsertPlnd(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                    string str = "mtInsertPlnd02 -- Error inserting CTX";
                    Ssutil.DbGetDiagStmt(hStmt, str); ;
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }

                //...Log2.v("\n\nDynSdbPlnd.SdInsertPlnd(): successfuly inserted record into table main.sd_plnd");
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynSdbPlnd.SdInsertPlnd(): Insertion of main.sd_plnd records is DISABLED.");
            }

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynSdbPlnd.SdInsertPlnd(): Exit");
            return Constant.SUCCESS;
        }


        /// <summary>
        /// This method deletes a single record with a prescribed key from the SDB
        /// CTX table main.sd_plnd. 
        /// </summary>
        /// <param name="hConn"></param>
        /// <param name="sband"></param>
        /// <param name="splan"></param>
        /// <param name="spno"></param>
        /// <returns></returns>
        public static int SdDeletePlnd(SQLHANDLE hConn, string sband, string splan, short spno)
        {
            //...Log2.v("\n\nDynSdbPlnd.SdDeletePlnd(): Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string delete_buf = String.Format(mDELETE_ONE, mTableName, sband, splan, spno);

            //...Log2.v("\nDynSdbPlnd.SdDeletePlnd(): query = " + delete_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, delete_buf, delete_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("mtDeletePlnd01 -- Error deleting from main.sd_plnd");
                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Log2.e("\nDynSdbPlnd.SdDeletePlnd(): ERROR: SQLExecDirect() failed for query = " + delete_buf);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + delete_buf;
                //...Log2.v("\nDynSdbPlnd.SdDeletePlnd(): Deletion of main.sd_plnd records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynSdbPlnd.SdDeletePlnd(): Exit");
            return 0;
        }


        /// <summary>
        /// This method deletes a set of detail records from the SDB CTX table main.sd_plnd
        /// that all have the prescribed 'master' sub-key.
        /// </summary>
        /// <param name="hConn"></param>
        /// <param name="sband"></param>
        /// <param name="splan"></param>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        public static int SdDeletePlnd(SQLHANDLE hConn, string sband, string splan, out int rowCount)
        {
            // 'out' requirement.
            rowCount = Int32.MinValue;

            //...Log2.v("\n\nDynSdbPlnd.SdDeletePlnd(): SET: Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string delete_buf = String.Format(mDELETE_SET, mTableName, sband, splan);

            //...Log2.v("\nDynSdbPlnd.SdDeletePlnd(): SET: query = " + delete_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, delete_buf, delete_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("mtDeletePlnd01 -- Error deleting from main.sd_plnd");
                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Log2.e("\nDynSdbPlnd.SdDeletePlnd(): ERROR: SET: SQLExecDirect() failed for query = " + delete_buf);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }

                // Get the number of records deleted.
                IntPtr intPtr = Marshal.AllocHGlobal(sizeof(SQLLEN));
                ODBC.SQLRowCount(hStmt, intPtr);
                rowCount = (int)Marshal.ReadInt64(intPtr);
                //...Log2.v("\n\nDynSdbPlnd.SdDeletePlnd(): rowCount = " + rowCount);
            }
            else
            {
                Info.Text += "\r\n\r\n" + delete_buf;
                //...Log2.v("\nDynSdbPlnd.SdDeletePlnd(): SET: Deletion of main.sd_plnd records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynSdbPlnd.SdDeletePlnd(): SET: Exit");
            return 0;
        }


        /// <summary>
        /// This method returns true if a record exists in the SDB table <b>main.sd_plnd</b>
        /// that has a prescribed CTX key {sband, splan, spno}.
        /// </summary>
        /// <param name="sband"></param>
        /// <param name="splan"></param>
        /// <param name="spno"></param>
        /// <returns></returns>
        public static bool SdbRecordExistsWithKey(string sband, string splan, string spno)
        {
            string whereClause = String.Format(" sband = '{0}' AND splan = '{1}' AND spno = '{2}' ", sband, splan, spno);

            int nRecords = Ssutil.DbCountRows(mTableName, whereClause);

            return (nRecords > 0);
        }

    }
}












