# Documented File: DynSdbBand.cs
**Repository Path:** `_Utillib\DynSdbBand.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
﻿using System;
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
    /// This class provides methods to select, fetch, instert, update and delete band records from the subsiduary database table
    /// <b>main.sd_band</b>.
    /// </summary>
    public class DynSdbBand
    {
        private const string mBaseTableName = "sd_band";

        private const string mMdbTableName = "main." + mBaseTableName;

        private static string mTableName = mMdbTableName;

        private static bool mMdbWriteEnabled = true;

        private static bool mSpoofModeIsOff = true;

        private const string mSELECT = "SELECT * FROM {0} ";

        private const string mUPDATE = "UPDATE {0} SET {1} WHERE bndcde='{2}' ";

        private const string mINSERT = "INSERT INTO {0} VALUES ({1}) ";

        private const string mDELETE = "DELETE FROM {0} WHERE bndcde='{1}' ";

        private const string mCHNG_UPDATE = "UPDATE {0} SET bndcde='{1}', mdate='{2}', mtime='{3}' WHERE bndcde='{4}' ";

        private static List<SdBand> mSdBandList = new List<SdBand>();

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
        /// The default SQL table name used by this class is <b>main.sd_band</b>; this method allows
        /// the caller to set the SQL table to a prescribed value so as to enable 'spoofing'.
        /// </summary>
        /// <param name="tablename"></param>
        public static void SetTableName(string tablename)
        {
            mTableName = tablename;
        }

        /// <summary>
        /// This method can be used to set whether the SDB <b>main.sd_band</b> table should be written to,
        /// (SQL DELETE, INSERT and/or UPDATE queries) or not; it can also set whether the SQL queries should
        /// target the actual main.sd_band table or a user's 'spoof' table (e.g. hulme.sd_band).
        /// </summary>
        /// <param name="mdbWriteEnabled"> - boolean that prescribes whether sd_band record
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
                    Log2.e("\nDynSdbBand.SetState(): ERROR: spoofMode is ON but userSchema is null or whitespace.");
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

            //...Log2.v("\r\nDynSdbBand.GetNextFreeCursor(): nNextFreeCursor = " + nNextFreeCursor);
            if (nNextFreeCursor >= Constant.NUM_CURSORS_FEW)
            {
                //	Search for a free cursors in the list
                for (int nInd = 0; nInd < Constant.NUM_CURSORS_FEW; nInd++)
                {
                    //...Log2.v("\r\nDynSdbBand.GetNextFreeCursor(): nInd = " + nInd);
                    if (!cursors[nInd].cursorOpen)
                    {
                        curHandle = nInd;
                        break;
                    }
                }

                if (curHandle == -1)
                {
                    // There are no more open cursors
                    GenUtil.SetErr("DynSdbBand01 -- No more open cursors.");
                    Application.Exit("\r\nDynSdbBand.GetNextFreeCursor(): ERROR: No available cursors.");
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

            //...Log2.v("\r\nDynSdbBand.GetNextFreeCursor(): found available cursor: curHandle = " + curHandle);
            cursors[curHandle].cursorOpen = true;

            return curHandle;
        }

        /// <summary>
        /// Selects records from the database table <b>main.sd_band</b> i.a.w.
        /// the prescribed SQL search criteria and ordering clauses; the 
        /// method returns an index to a cursor object that can be used by subsequent
        /// calls to SdFetchBand().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// </remarks>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for SdFetchBand() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECUTE_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int SdSelectBand(string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynSdbBand.SdSelectBand(): Entry");

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
                    Log2.e("\nDynSdbBand.SdSelectBand(): ERROR : Exit: call to SQLAllocHandle() failed, sqlRet = " + sqlRet);
                    return Error.ODBC_SQLALLOCHANDLE_FAILED;
                }

                SdBand.BindPtrsToCols(mhStmt, out mTgtValPtrs, out mNullIndPtrs);
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
                Log2.e("\nDynSdbBand.SdSelectBand(): ERROR : Exit: Attempt to close the cursor on the reusable statement handle failed.");
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

            //...Log2.v("\r\nDynSdbBand.SdSelectBand(): stmt_buf = \r\n" + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(mhStmt, stmt_buf, stmt_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nDynSdbBand.SdSelectBand(): ERROR : Exit: call to SQLExecDirect() failed, sqlRet = " + sqlRet);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            mMagicNumber++;

            // Populate the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = SQLHANDLE.Zero;
            cursors[curHandle].hConn = SQLHANDLE.Zero;
            cursors[curHandle].magicNumber = mMagicNumber;

            //...Log2.v("\n\nDynSdbBand.SdSelectBand(): Exit");
            return (curHandle);
        }

        /// <summary>
        /// Retrieves a single band record from the database table <b>main.sd_band</b> using 
        /// the cursor object created by a previous call to SdSelectBand().
        /// </summary>
        /// <param name="nCursor"> - FtCursor object.</param>
        /// <param name="sdBand"> - a SdBand object populated with data from the row.</param>
        /// <param name="nullInds"> - array of ODBC nullInds for sdBand.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int SdFetchBand(int nCursor, out SdBand sdBand, out SQLLEN[] nullInds)
        {
            // 'out' requirments.
            sdBand = null;
            nullInds = null;

            //...Log2.v("\n\nDynSdbBand.SdFetchBand(): Entry");

            FetchCount.IncrementSdBand();

            int nRet = -666;

            // Test for contiguity, i.e. that the previous select and 
            // this fetch are a matched pair.
            if (cursors[nCursor].magicNumber != mMagicNumber)
            {
                Log2.e("\n\nDynSdbBand.SdFetchBand(): Exit: ERROR: failed contiguity test.");

                return -666;
            }

            // Fetch the next row of selected Band data.
            SQLRETURN sqlRet = ODBC.SQLFetch(mhStmt);

            // Check the SQLRETURN and act accordingly.
            if (!ODBC.IsOK(sqlRet))
            {
                // If no more Band records available then break out the while-loop.
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\n\nDynSdbBand.SdFetchBand(): Exit: sqlRet == ODBC.SQL_NO_DATA");

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
                    Log2.e("\n\nDynSdbBand.SdFetchBand(): Exit: ERROR: SQLFetch(): sqlRet = " + sqlRet);
                    Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, mhStmt);
                    return -11;
                }
            }
            else
            {
                // We had a successful SQLFetch() operation; now get the record's column data.
                try
                {
                    SdBand.ReadColBindings(mTgtValPtrs, mNullIndPtrs, out sdBand, out nullInds);

                    nRet = Constant.SUCCESS;
                }
                catch (Exception e)
                {
                    string str = String.Format("Fetch() failed for reason: " + e.Message);
                    GenUtil.SetError(1011, str);
                    Log2.e("\n\nDynSdbBand.SdFetchBand(): Exit: ERROR: call to ReadColBindings() threw an exception:\n" + e.Message);
                    return -22;
                } //try-catch

            } //if-else

            //...Log2.v(sdBand.ToStringWN(nullInds));

            //...Log2.v("\n\nDynSdbBand.SdFetchBand(): Exit");
            return nRet;
        } //end of SdFetchBand

        /// <summary>
        /// Closes (releases) a cursor instantiated by a previous call to SdSelectBand(); 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        public static int SdCloseBand(int curHandle)
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
        /// This method updates all of the column values of a prescribed record in the SDB Band table
        /// main.sd_band.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="sdBand"> - a prescribed SdBand object that provides the updated field values.</param>
        /// <param name="nullInds"> - an array of ODBC nullInds associated with sdBand.</param>
        /// <returns></returns>
        public static int SdUpdateBand(SQLHANDLE hConn, SdBand sdBand, SQLLEN[] nullInds)
        {
            //...Log2.v("\nDynSdbBand.SdUpdateBand(): Entry");

            string update_buf;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            // Prepare the update statement
            update_buf = String.Format(mUPDATE, mTableName, sdBand.ToStringAsCSVequates(nullInds), sdBand.bndcde);

            //...Log2.v("\nDynSdbBand.SdUpdateBand(): query = " + update_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, update_buf, update_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("\nDynSdbBand.SdUpdateBand(): ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\n{1}", sqlRet, update_buf);
                    Log2.e(str);
                    str = String.Format("mtUpdateBand04 -- Error writing Band: {0}", sdBand.bndcde);

                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return -4;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + update_buf;
                //...Log2.v("\nDynSdbBand.SdUpdateBand(): Updating of main.sd_band records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\nDynSdbBand.SdUpdateBand(): Exit");
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method inserts a record into the SDB Band table main.sd_band using column values prescribed 
        /// by the fields of a prescribed SdBand object.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="sdBand"> - a SdBand object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for sdBand</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int SdInsertBand(SQLHANDLE hConn, SdBand sdBand, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynSdbBand.SdInsertBand(): Entry");

            string cSQL;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynSdbBand.SdInsertBand(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            //Create the SQL insert statement.
            cSQL = String.Format(mINSERT, mTableName, sdBand.ToStringAsCSV(nullInd));

            //...Log2.v("nDynSdBand.SdInsertBand(): SQLExecDirect():\r\n" + cSQL);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\n\nDynSdbBand.SdInsertBand(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                    string str = "mtInsertBand02 -- Error inserting antenna";
                    Ssutil.DbGetDiagStmt(hStmt, str); ;
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }

                //...Log2.v("\n\nDynSdbBand.SdInsertBand(): successfuly inserted record into table main.sd_band");
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynSdbBand.SdInsertBand(): Insertion of main.sd_band records is DISABLED.");
            }

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynSdbBand.SdInsertBand(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method deletes a record with a prescribed bndcde from the SDB
        /// Band table main.sd_band.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="bndcde"> - a prescribed antenna code.</param>
        /// <returns></returns>
        public static int SdDeleteBand(SQLHANDLE hConn, string bndcde)
        {
            //...Log2.v("\n\nDynSdbBand.SdDeleteBand(): Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string delete_buf = String.Format(mDELETE, mTableName, bndcde);

            //...Log2.v("\nDynSdbBand.SdDeleteBand(): query = " + delete_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, delete_buf, delete_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("mtDeleteBand01 -- Error deleting from main.sd_band");
                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Log2.e("\nDynSdbBand.SdDeleteBand(): ERROR: SQLExecDirect() failed for query = " + delete_buf);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + delete_buf;
                //...Log2.v("\nDynSdbBand.SdDeleteBand(): Deletion of main.sd_band records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynSdbBand.SdDeleteBand(): Exit");
            return 0;
        }

        /// <summary>
        /// This method returns true if a record exists in the SDB Band table <b>main.sd_band</b>
        /// that has a prescribed band code (bndcde).
        /// </summary>
        /// <param name="bndcde"> the prescribed 'call sign' string.</param>
        /// <returns></returns>
        public static bool SdbRecordExistsWithBndcde(string bndcde)
        {
            string whereClause = String.Format(" bndcde = '{0}' ", bndcde);

            int nRecords = Ssutil.DbCountRows(mTableName, whereClause);

            return (nRecords > 0);
        }

        /// <summary>
        /// This method returns a populated SdBand object and associated ODBC nullInds
        /// corresponding to the record in the main SDB Band table 'main.sd_band' that has the 
        /// prescribed key (bndcde).
        /// </summary>
        /// <param name="bndcde"></param>
        /// <param name="sdBand"></param>
        /// <param name="nullInds"></param>
        /// <returns>True if a record with the prescribed key was found in the table; otherwise false.</returns>
        public static bool FetchRecordWithBndCde(string bndcde, out SdBand sdBand, out SQLLEN[] nullInds)
        {
            // 'out' requirements.
            sdBand = null;
            nullInds = null;

            bool isSuccessful = false;

            string whereClause = String.Format(" bndcde='{0}' ", bndcde);

            // Do the 'SELECT'.
            int nCursor = SdSelectBand(whereClause, "");

            if (nCursor < 0)
            {
                Log2.e("\nDynUserTable.FetchRecordWithKeys(): ERROR: Select() failed, nCursor = " + nCursor);
                return false;
            }

            // Do the 'FETCH'.
            int rc = SdFetchBand(nCursor, out sdBand, out nullInds);

            if (rc == Constant.SUCCESS)
            {
                isSuccessful = true;
            }
            else
            {
                Log2.e("\nDynUserTable.FetchRecordWithKeys(): ERROR: Fetch() failed, rc = " + rc);
                isSuccessful = false;
            }

            SdCloseBand(nCursor);

            return isSuccessful;
        }

        /// <summary>
        /// This method outputs a list of all SdBand objects corresponding to all of the records
        /// in the SDB table main.sd_band.
        /// </summary>
        /// <param name="sdBandList"> - output list of all SdBand objects.</param>
        /// <returns></returns>
        public static int GetSdbBandList(out List<SdBand> sdBandList)
        {
            // "out" requiremment.
            sdBandList = new List<SdBand>();

            int nRet = Constant.SUCCESS;

            // Check if we already created it.
            if ((mSdBandList != null) && (mSdBandList.Count > 0))
            {
                sdBandList = mSdBandList;
            }
            else
            {
                // We need to create it.

                SdBand sdBand;
                SQLLEN[] nullInds;

                int handle = SdSelectBand("", "bndcde");
                if (handle < 0) return Constant.FAILURE;

                while (SdFetchBand(handle, out sdBand, out nullInds) == Constant.SUCCESS)
                {
                    sdBandList.Add(sdBand);

                    //...Log2.v("\nDynSdbBand.GetSdbBandList(): " + sdBand.ToStringAsCSV(nullInds));
                }

                SdCloseBand(handle);

                // Save a private copy to speed up subsequent calls.
                mSdBandList = sdBandList;
            }

            return nRet;
        }

        /// <summary>
        /// This method returns the SdBand object with the prescribed bndcde.
        /// </summary>
        /// <param name="bndcde"> - the prescribed bndcde.</param>
        /// <returns></returns>
        public static SdBand GetSdBand(string bndcde)
        {
            SdBand sdBand = null;

            List<SdBand> sdBandList;

            GetSdbBandList(out sdBandList);

            foreach (SdBand sdb in sdBandList)
            {
                if (sdb.bndcde.Equals(bndcde.ToUpper().Trim()))
                {
                    sdBand = sdb;
                    break;
                }
            }

            return sdBand;
        }

        /// <summary>
        /// This method returns a list of SdBand objects corresponding to the prescribed bndcde's 
        /// adjacent bands, as given in the badj column of the main.sd_band record.
        /// </summary>
        /// <param name="bndcde"> - the prescribed bndcde.</param>
        /// <returns></returns>
        public static List<SdBand> GetAdjSdBands(string bndcde)
        {
            List<SdBand> adjSdBandList = new List<SdBand>();

            List<string> adjBandCodeList = GetSdBand(bndcde).GetAdjBandCodeList();

            foreach (string adjBandCode in adjBandCodeList) adjSdBandList.Add(GetSdBand(adjBandCode));

            return adjSdBandList;
        }

    }
}












```
