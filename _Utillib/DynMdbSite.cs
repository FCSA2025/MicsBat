using System;
using System.Text;
using _DataStructures;
using _NewLib;

namespace _Utillib
{
    using _Configuration;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// Provides methods to select and fetch TS SITE records from the database table
    /// <b>main.mt_site</b>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item> The methods in this class are used to perform operations on the
    /// SITE table. The actual SITE table name is a variable.</item>
    /// <item>The supported operations are: selectSite, fetchSite,
    /// and closeSite.</item>
    /// <item>The caller must first call selectSite to select the required rows:
    /// this method simply sets up the cursor for subsequent use.</item>
    /// <item>Once selectSite has been called, the user must call fetchSite
    /// to  retreive a row.</item>
    /// <item>The method closeSite must be called to release ODBC resources 
    /// (connection and statement handles etc) and to release the cursor back the 'pool'.</item>
    /// </list>
    /// </remarks>	
    public class DynMdbSite
    {
        private const string mBaseTableName = "mt_site";

        private const string mMdbTableName = "main." + mBaseTableName;

        private static string mTableName = mMdbTableName;

        private static bool mMdbWriteEnabled = true;

        private static bool mSpoofModeIsOff = true;

        private const string mSELECT = "SELECT * FROM {0} ";

        private const string mUPDATE = "UPDATE {0} SET {1} WHERE call1='{2}' ";

        //private const string mINSERT = "INSERT INTO {0} VALUES ({1}) ";
        private const string mINSERT = "INSERT INTO {0} ({1}) VALUES ({2}) ";

        private const string mDELETE = "DELETE FROM {0} WHERE call1='{1}' ";

        private const string mCHNG_UPDATE = "UPDATE {0} SET call1='{1}', userid='{2}', mdate='{3}', mtime='{4}' WHERE call1='{5}' ";

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
        /// This method can be used to set whether the MDB <b>main.mt_site</b> table should be written to,
        /// (SQL DELETE, INSERT and/or UPDATE queries) or not; it can also set whether the SQL queries should
        /// target the actual main.mt_site table or a user's 'spoof' table (e.g. hulme.audit_trail).
        /// </summary>
        /// <param name="mdbWriteEnabled"> - boolean that prescribes whether mt_site record
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
                    Log2.e("\nDynMdbSite.SetState(): ERROR: spoofMode is ON but userSchema is null or whitespace.");
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

            //...Log2.v("\r\nDynMdbSite.GetNextFreeCursor(): nNextFreeCursor = " + nNextFreeCursor);
            if (nNextFreeCursor >= Constant.NUM_CURSORS_FEW)
            {
                //	Search for a free cursors in the list
                for (int nInd = 0; nInd < Constant.NUM_CURSORS_FEW; nInd++)
                {
                    //...Log2.v("\r\nDynMdbSite.GetNextFreeCursor(): nInd = " + nInd);
                    if (!cursors[nInd].cursorOpen)
                    {
                        curHandle = nInd;
                        break;
                    }
                }

                if (curHandle == -1)
                {
                    // There are no more open cursors
                    GenUtil.SetErr("DynMdbSite01 -- No more open cursors.");
                    Application.Exit("\r\nDynMdbSite.GetNextFreeCursor(): ERROR: No available cursors.");
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

            //...Log2.v("\r\nDynMdbSite.GetNextFreeCursor(): found available cursor: curHandle = " + curHandle);
            cursors[curHandle].cursorOpen = true;

            return curHandle;
        }

        /// <summary>
        /// Selects site records from the database table <b>main.mt_site</b> for
        /// the prescribed SQL search criteria and ordering clauses; the 
        /// method returns an index to a cursor object that can be used by subsequent
        /// calls to MtFetchSite().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for MtFetchSite() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECUTE_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int MtSelectSite(string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynMdbSite.MtSelectSite(): Entry");

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
                    Log2.e("\nDynMdbSite.MtSelectSite(): ERROR : Exit: call to SQLAllocHandle() failed, sqlRet = " + sqlRet);
                    return Error.ODBC_SQLALLOCHANDLE_FAILED;
                }

                MtSite.BindPtrsToCols(mhStmt, out mTgtValPtrs, out mNullIndPtrs);
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
                Log2.e("\nDynMdbSite.MtSelectSite(): ERROR : Exit: Attempt to close the cursor on the reusable statement handle failed.");
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

            //...Log2.v("\r\nDynMdbSite.MtSelectSite(): stmt_buf = \r\n" + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(mhStmt, stmt_buf, stmt_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nDynMdbSite.MtSelectSite(): ERROR : Exit: call to SQLExecDirect() failed, sqlRet = " + sqlRet);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            mMagicNumber++;

            // Populate the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = SQLHANDLE.Zero;
            cursors[curHandle].hConn = SQLHANDLE.Zero;
            cursors[curHandle].magicNumber = mMagicNumber;

            //...Log2.v("\n\nDynMdbSite.MtSelectSite(): Exit");
            return (curHandle);
        }

        /// <summary>
        /// Retrieves a single row of site data from the database table <b>main.mt_site</b> using 
        /// the cursor object created by a previous call to MtSelectSite().
        /// </summary>
        /// <param name="nCursor"> - FtCursor object.</param>
        /// <param name="mtSite"> - a MtSite object populated with data from the row.</param>
        /// <param name="nullInds"> - array of ODBC nullInds for mtSite.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int MtFetchSite(int nCursor, out MtSite mtSite, out SQLLEN[] nullInds)
        {
            // 'out' requirments.
            mtSite = null;
            nullInds = null;

            //...Log2.v("\n\nDynMdbSite.MtFetchSite(): Entry");

            FetchCount.IncrementMtSite();

            int nRet = -666;

            // Test for contiguity, i.e. that the previous select and 
            // this fetch are a matched pair.
            if (cursors[nCursor].magicNumber != mMagicNumber)
            {
                Log2.e("\n\nDynMdbSite.MtFetchSite(): Exit: ERROR: failed contiguity test.");

                return -666;
            }

            // Fetch the next row of selected Site data.
            SQLRETURN sqlRet = ODBC.SQLFetch(mhStmt);

            // Check the SQLRETURN and act accordingly.
            if (!ODBC.IsOK(sqlRet))
            {
                // If no more Site records available then break out the while-loop.
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\n\nDynMdbSite.MtFetchSite(): Exit: sqlRet == ODBC.SQL_NO_DATA");

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
                    Log2.e("\n\nDynMdbSite.MtFetchSite(): Exit: ERROR: SQLFetch(): sqlRet = " + sqlRet);
                    Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, mhStmt);
                    return -11;
                }
            }
            else
            {
                // We had a successful SQLFetch() operation; now get the record's column data.
                try
                {
                    MtSite.ReadColBindings(mTgtValPtrs, mNullIndPtrs, out mtSite, out nullInds);

                    nRet = Constant.SUCCESS;
                }
                catch (Exception e)
                {
                    string str = String.Format("Fetch() failed for reason: " + e.Message);
                    GenUtil.SetError(1011, str);
                    Log2.e("\n\nDynMdbSite.MtFetchSite(): Exit: ERROR: call to ReadColBindings() threw an exception:\n" + e.Message);
                    return -22;
                } //try-catch

            } //if-else

            //...Log2.v(mtSite.ToStringWN(nullInds));

            //...Log2.v("\n\nDynMdbSite.MtFetchSite(): Exit");
            return nRet;
        } //end of MtFetchSite

        /// <summary>
        /// Closes (releases) a cursor associated with a site record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        public static int MtCloseSite(int curHandle)
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
        /// This method returns true if a record exists in the MDB table <b>main.mt_site</b>
        /// that has a prescribed call sign (call1).
        /// </summary>
        /// <param name="call1"> the prescribed 'call sign' string.</param>
        /// <returns></returns>
        public static bool MdbRecordExists(string call1)
        {
            string whereClause = String.Format(" call1 = '{0}' ", call1);

            // call1 is the 'key' for the main.mt_site table and so there
            // can only be 0 or 1 records.
            return (Ssutil.DbCountRows(mTableName, whereClause) == 1);
        }

        /// <summary>
        /// This method updates all of the column values of a prescribed record in the MDB TS site table
        /// main.mt_site.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="mtSite"> - a prescribed MtSite object that provides the updated field values.</param>
        /// <param name="nullInds"> - an array of ODBC nullInds associated with mtSite.</param>
        /// <returns></returns>
        public static int MtUpdateSite(SQLHANDLE hConn, MtSite mtSite, SQLLEN[] nullInds)
        {
            //...Log2.v("\nDynMdbSite.MtUpdateSite(): Entry");

            string update_buf;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            // Prepare the update statement
            update_buf = String.Format(mUPDATE, mTableName, mtSite.ToStringAsCSVequates(nullInds), mtSite.call1);

            //...Log2.v("\nDynMdbSite.MtUpdateSite(): query = " + update_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, update_buf, update_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("\nDynMdbSite.MtUpdateSite(): ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\n{1}", sqlRet, update_buf);
                    Log2.e(str);
                    str = String.Format("mtUpdateSite04 -- Error writing Site: {0}", mtSite.call1);

                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return -4;
                }
            }
            else
            {
                // Append the SQL UPDATE query to Info.Text so that we can later write them all out to a log file.
                Info.Text += "\r\n\r\n" + update_buf;

                //...Log2.v("\nDynMdbSite.MtUpdateSite(): Updating of main.mt_site records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\nDynMdbSite.MtUpdateSite(): Exit");
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Inserts a record into the MDB TS table main.mt_site using column values prescribed 
        /// by the fields of an MtSite object.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="mtSite"> - a MtSite object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for mtSite</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int MtInsertSite(SQLHANDLE hConn, MtSite mtSite, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynMdbSite.MtInsertSite(): Entry");

            string cSQL;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynMdbSite.MtInsertSite(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            //Handle the new mt_site field added by Bill Venn.
            if ((nullInd[MtSite.STRLATIT] != Constant.DB_NULL) && (nullInd[MtSite.STRLONGIT] != Constant.DB_NULL))
            {
                mtSite.SiteCoords = mtSite.strlatit + "-" + mtSite.strlongit;
                nullInd[MtSite.SITECOORDS] = Constant.DB_NOT_NULL;
            }


            //Create the SQL insert statement.
            //cSQL = String.Format(mINSERT, mTableName, mtSite.ToStringAsCSV(nullInd));
            cSQL = String.Format(mINSERT, mTableName, MtSite.AllColumnsForSqlSelectexceptSiteCoords, mtSite.ToStringAsCSV(nullInd));

            //...Log2.v("nDynMdbSite.MeInsertSite(): SQLExecDirect():\r\n" + cSQL);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\n\nDynMdbSite.MtInsertSite(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                    string str = "mtInsertSite02 -- Error inserting site";
                    Ssutil.DbGetDiagStmt(hStmt, str); ;
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }

                //...Log2.v("\n\nDynMdbSite.MtInsertSite(): successfuly inserted record into table main.mt_site");
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynMdbSite.MtInsertSite(): Insertion of main.mt_site records is DISABLED.");
            }

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynMdbSite.MtInsertSite(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method deletes a record with a prescribed call1 from the MDB
        /// TS site table main.mt_site.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="call1"> - a prescribed call sign string.</param>
        /// <returns></returns>
        public static int MtDeleteSite(SQLHANDLE hConn, string call1)
        {
            //...Log2.v("\n\nDynMdbSite.MtDeleteSite(): Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string delete_buf = String.Format(mDELETE, mTableName, call1);

            //...Log2.v("\nDynMdbSite.MtDeleteSite(): query = " + delete_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, delete_buf, delete_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("mtDeleteSite01 -- Error deleting from main.mt_site");
                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Log2.e("\nDynMdbSite.MtDeleteSite(): ERROR: SQLExecDirect() failed for query = " + delete_buf);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + delete_buf;
                //...Log2.v("\nDynMdbSite.MtDeleteSite(): Deletion of main.mt_site records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynMdbSite.MtDeleteSite(): Exit");
            return 0;
        }


        /// <summary>
        /// This method updates records in the MDB <b>main.mt_site</b> table i.a.w. the 
        /// 'GK' directives in a user's TS PDF import file; specifically, all records that
        /// have 'call1 = X' are changed to 'call1 = Y' where X and Y are user-prescribed
        /// call sign strings.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>SQLHANDLE hConn, 
        /// <param name="ftChng"> - prescribed feCCal object that prescribes X and Y.</param>
        /// <param name="userName"> - user's MICS ID.</param>
        /// <param name="sysDate"> - prescribed current system date.</param>
        /// <param name="sysTime"> - prescribed current system time.</param>
        /// <param name="chngCount"> - a count of the number of records that were updated.</param>
        /// <returns></returns>
        public static int MtChngUpdateSite(SQLHANDLE hConn, FtChng ftChng, string userName, string sysDate, string sysTime, ref int chngCount)
        {
            string cSQL;
            int status = Constant.SUCCESS;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLLEN nRows = 0;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            /* Update the MDB site records */
            cSQL = String.Format(mCHNG_UPDATE,
                                 mTableName, ftChng.newcall1, userName, sysDate, sysTime, ftChng.oldcall1);

            //...Log2.v("\nDynMdbSite.MtChngUpdateSite(): query = " + cSQL);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (ODBC.IsOK(sqlRet))
                {
                    IntPtr intPtr = Marshal.AllocHGlobal(sizeof(SQLLEN));
                    sqlRet = ODBC.SQLRowCount(hStmt, intPtr);
                    nRows = Marshal.ReadInt64(intPtr);

                    chngCount += (int)nRows;
                }
                else
                {
                    Log2.e("\nDynMdbSite.MtChngUpdateSite(): ERROR: call to SQLExecDirec() failed for query =\n" + cSQL);
                    status = Constant.FAILURE;
                    Console.Write("\r\nERROR!  Could not Change MDB site record CallSign fields.\r\n");
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynMdbSite.MtChngUpdateSite(): Update of main.mt_site records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            return status;
        }

        /// <summary>
        /// Returns the number of rows found in the MDB table main.mt_site using
        /// a prescribed SQL 'WHERE' condition.
        /// </summary>
        /// <param name="cCondition"> - SQL 'WHERE' clause.</param>
        /// <returns> - number of rows found.</returns>
        public static int DbCountRows(string cCondition)
        {
            //...Log2.v("\n\nDynMdbSite.DbCountRows(): Entry");

            int nCount = 0;

            SQLHANDLE hStmt = SQLHANDLE.Zero;
            SQLRETURN sqlRet = 0;
            SQLHDBC hConn = SQLHDBC.Zero;

            StringBuilder sb = new StringBuilder();
            sb.Append("select count(*) from ");
            sb.Append(mTableName);

            if (!String.IsNullOrWhiteSpace(cCondition))
            {
                sb.Append(" where ");
                sb.Append(cCondition);
            }

            string cBuf = sb.ToString();

            //...Log2.v("\r\nDynMdbSite.DbCountRows(): cBuf = " + cBuf);

            try
            {
                hConn = Ssutil.NewConn();

                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\r\nDynMdbSite.DbCountRows(): ERROR: call to SQLAllocHandle() failed, sqlRet =  " + sqlRet);
                    return Error.ODBC_SQLALLOCHANDLE_FAILED;
                }

                sqlRet = ODBC.SQLExecDirect(hStmt, cBuf, cBuf.Length);
                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\r\nDynMdbSite.DbCountRows(): ERROR: call to SQLExecDirect() failed, sqlRet =  " + sqlRet);
                    Log2.e("\r\nDynMdbSite.DbCountRows(): query =  " + cBuf);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }

                sqlRet = ODBC.SQLFetch(hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\r\nDynMdbSite.DbCountRows(): ERROR: call to SQLFetch() failed, sqlRet =  " + sqlRet);
                    return Error.ODBC_FETCH_FAILED;
                }

                SQLLEN nullInd;
                Ssutil.DbGetInt(hStmt, 1, "Count", out nCount, out nullInd);
            }
            catch (Exception e)
            {
                Log2.e("\r\nDynMdbSite.DbCountRows(): try-catch: ERROR: exception: " + e.Message);
                Ssutil.DbGetDiagStmt(hStmt, "DbCountRows: Error.");
                nCount = -3;
            }
            finally
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
            }

            //...Log2.v("\nDynMdbSite.DbCountRows(): Exit: nCount = " + nCount);
            return nCount;
        }

        /// <summary>
        /// This method outputs a list of MtSite objects as SELECTed using
        /// prescribed search criteria (SQL WHERE clause) and precribed ORDER BY clause.
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <param name="orderBy"></param>
        /// <param name="siteList"></param>
        public static void GetListOfMtSites(string searchCriteria, string orderBy, out List<MtSite> siteList)
        {
            // 'out' requirement.
            siteList = new List<MtSite>();

            MtSite mtSite;
            SQLLEN[] nullInds;

            //...Log2.v("\nDynMdbSite.GetListOfMtSites(): siteTableName = " + siteTableName);

            int handle = MtSelectSite(searchCriteria, orderBy);
            if (handle < 0) return;

            //...Log2.v("\nDynMdbSite.GetListOfMtSites(): select site handle = " + handle);

            while (MtFetchSite(handle, out mtSite, out nullInds) == Constant.SUCCESS)
            {
                siteList.Add(mtSite);

                //...Log2.v("\n" + String.Format("DynMdbSite.GetListOfMtSites(): mtSite = {0}", mtSite.KeysToString()));
            }

            MtCloseSite(handle);

            return;
        }


    }
}









