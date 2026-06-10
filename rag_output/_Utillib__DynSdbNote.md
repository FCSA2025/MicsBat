# Documented File: DynSdbNote.cs
**Repository Path:** `_Utillib\DynSdbNote.cs`
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
    using System.Runtime.InteropServices;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides methods to select, fetch, insert, update and delete equipment records from the subsiduary database table
    /// <b>main.sd_note</b>.
    /// </summary>
    public class DynSdbNote
    {
        private const string mBaseTableName = "sd_note";

        private const string mMdbTableName = "main." + mBaseTableName;

        private static string mTableName = mMdbTableName;

        private static bool mMdbWriteEnabled = true;

        private static bool mSpoofModeIsOff = true;

        private const string mSELECT = "SELECT * FROM {0} ";

        private const string mUPDATE = "UPDATE {0} SET {1} WHERE oper='{2}' AND nonum='{3}' ";

        private const string mINSERT = "INSERT INTO {0} VALUES ({1}) ";

        private const string mDELETE = "DELETE FROM {0} WHERE oper='{1}' AND nonum='{2}' ";

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
        /// This method can be used to set whether the SDB <b>main.sd_note</b> table should be written to,
        /// (SQL DELETE, INSERT and/or UPDATE queries) or not; it can also set whether the SQL queries should
        /// target the actual main.sd_note table or a user's 'spoof' table (e.g. hulme.sd_note).
        /// </summary>
        /// <param name="mdbWriteEnabled"> - boolean that prescribes whether sd_note record
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
                    Log2.e("\nDynSdbNote.SetState(): ERROR: spoofMode is ON but userSchema is null or whitespace.");
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

            //...Log2.v("\r\nDynSdbNote.GetNextFreeCursor(): nNextFreeCursor = " + nNextFreeCursor);
            if (nNextFreeCursor >= Constant.NUM_CURSORS_FEW)
            {
                //	Search for a free cursors in the list
                for (int nInd = 0; nInd < Constant.NUM_CURSORS_FEW; nInd++)
                {
                    //...Log2.v("\r\nDynSdbNote.GetNextFreeCursor(): nInd = " + nInd);
                    if (!cursors[nInd].cursorOpen)
                    {
                        curHandle = nInd;
                        break;
                    }
                }

                if (curHandle == -1)
                {
                    // There are no more open cursors
                    GenUtil.SetErr("DynSdbNote01 -- No more open cursors.");
                    Application.Exit("\r\nDynSdbNote.GetNextFreeCursor(): ERROR: No available cursors.");
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

            //...Log2.v("\r\nDynSdbNote.GetNextFreeCursor(): found available cursor: curHandle = " + curHandle);
            cursors[curHandle].cursorOpen = true;

            return curHandle;
        }

        /// <summary>
        /// Selects records from the database table <b>main.sd_note</b> i.a.w.
        /// the prescribed SQL search criteria and ordering clauses; the 
        /// method returns an index to a cursor object that can be used by subsequent
        /// calls to SdFetchNote().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// </remarks>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for SdFetchNote() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECUTE_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int SdSelectNote(string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynSdbNote.SdSelectNote(): Entry");

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
                    Log2.e("\nDynSdbNote.SdSelectNote(): ERROR : Exit: call to SQLAllocHandle() failed, sqlRet = " + sqlRet);
                    return Error.ODBC_SQLALLOCHANDLE_FAILED;
                }

                SdNote.BindPtrsToCols(mhStmt, out mTgtValPtrs, out mNullIndPtrs);
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
                Log2.e("\nDynSdbNote.SdSelectNote(): ERROR : Exit: Attempt to close the cursor on the reusable statement handle failed.");
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

            //...Log2.v("\r\nDynSdbNote.SdSelectNote(): stmt_buf = \r\n" + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(mhStmt, stmt_buf, stmt_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nDynSdbNote.SdSelectNote(): ERROR : Exit: call to SQLExecDirect() failed, sqlRet = " + sqlRet);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            mMagicNumber++;

            // Populate the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = SQLHANDLE.Zero;
            cursors[curHandle].hConn = SQLHANDLE.Zero;
            cursors[curHandle].magicNumber = mMagicNumber;

            //...Log2.v("\n\nDynSdbNote.SdSelectNote(): Exit");
            return (curHandle);
        }

        /// <summary>
        /// Retrieves a single equipment record from the database table <b>main.sd_note</b> using 
        /// the cursor object created by a previous call to SdSelectNote().
        /// </summary>
        /// <param name="nCursor"> - FtCursor object.</param>
        /// <param name="sdNote"> - a SdNote object populated with data from the row.</param>
        /// <param name="nullInds"> - array of ODBC nullInds for sdNote.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int SdFetchNote(int nCursor, out SdNote sdNote, out SQLLEN[] nullInds)
        {
            // 'out' requirments.
            sdNote = null;
            nullInds = null;

            //...Log2.v("\n\nDynSdbNote.SdFetchNote(): Entry");

            FetchCount.IncrementSdNote();

            int nRet = -666;

            // Test for contiguity, i.e. that the previous select and 
            // this fetch are a matched pair.
            if (cursors[nCursor].magicNumber != mMagicNumber)
            {
                Log2.e("\n\nDynSdbNote.SdFetchNote(): Exit: ERROR: failed contiguity test.");

                return -666;
            }

            // Fetch the next row of selected Note data.
            SQLRETURN sqlRet = ODBC.SQLFetch(mhStmt);

            // Check the SQLRETURN and act accordingly.
            if (!ODBC.IsOK(sqlRet))
            {
                // If no more Note records available then break out the while-loop.
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\n\nDynSdbNote.SdFetchNote(): Exit: sqlRet == ODBC.SQL_NO_DATA");
                    return ODBC.SQL_NO_DATA;
                }
                else
                {
                    // We have an error.
                    Log2.e("\n\nDynSdbNote.SdFetchNote(): Exit: ERROR: SQLFetch(): sqlRet = " + sqlRet);
                    Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, mhStmt);
                    return -11;
                }
            }
            else
            {
                // We had a successful SQLFetch() operation; now get the record's column data.
                try
                {
                    SdNote.ReadColBindings(mTgtValPtrs, mNullIndPtrs, out sdNote, out nullInds);

                    nRet = Constant.SUCCESS;
                }
                catch (Exception e)
                {
                    string str = String.Format("Fetch() failed for reason: " + e.Message);
                    GenUtil.SetError(1011, str);
                    Log2.e("\n\nDynSdbNote.SdFetchNote(): Exit: ERROR: call to ReadColBindings() threw an exception:\n" + e.Message);
                    return -22;
                } //try-catch

            } //if-else

            //...Log2.v(sdNote.ToStringWN(nullInds));

            //...Log2.v("\n\nDynSdbNote.SdFetchNote(): Exit");
            return nRet;
        } //end of SdFetchNote

        /// <summary>
        /// Closes (releases) a cursor instantiated by a previous call to SdSelectNote(); 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        public static int SdCloseNote(int curHandle)
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
        /// This method updates all of the column values of a prescribed record in the SDB Note table
        /// main.sd_note.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="sdNote"> - a prescribed SdNote object that provides the updated field values.</param>
        /// <param name="nullInds"> - an array of ODBC nullInds associated with sdNote.</param>
        /// <returns></returns>
        public static int SdUpdateNote(SQLHANDLE hConn, SdNote sdNote, SQLLEN[] nullInds)
        {
            //...Log2.v("\nDynSdbNote.SdUpdateNote(): Entry");

            string update_buf;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            // Prepare the update statement
            update_buf = String.Format(mUPDATE, mTableName, sdNote.ToStringAsCSVequates(nullInds), sdNote.oper, sdNote.nonum);

            //...Log2.v("\nDynSdbNote.SdUpdateNote(): query = " + update_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, update_buf, update_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("\nDynSdbNote.SdUpdateNote(): ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\n{1}", sqlRet, update_buf);
                    Log2.e(str);
                    str = String.Format("mtUpdateNote04 -- Error writing Note: {0} {1}", sdNote.oper, sdNote.nonum);

                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return -4;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + update_buf;
                //...Log2.v("\nDynSdbNote.SdUpdateNote(): Updating of main.sd_note records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\nDynSdbNote.SdUpdateNote(): Exit");
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method inserts a record into the SDB Note table main.sd_note using column values prescribed 
        /// by the fields of a prescribed SdNote object.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="sdNote"> - a SdNote object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for sdNote</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int SdInsertNote(SQLHANDLE hConn, SdNote sdNote, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynSdbNote.SdInsertNote(): Entry");

            string cSQL;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynSdbNote.SdInsertNote(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            //Create the SQL insert statement.
            cSQL = String.Format(mINSERT, mTableName, sdNote.ToStringAsCSV(nullInd));

            //...Log2.v("nDynSdNote.SdInsertNote(): SQLExecDirect():\r\n" + cSQL);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\n\nDynSdbNote.SdInsertNote(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                    string str = "mtInsertNote02 -- Error inserting antenna";
                    Ssutil.DbGetDiagStmt(hStmt, str); ;
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }

                //...Log2.v("\n\nDynSdbNote.SdInsertNote(): successfuly inserted record into table main.sd_note");
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynSdbNote.SdInsertNote(): Insertion of main.sd_note records is DISABLED.");
            }

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynSdbNote.SdInsertNote(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method deletes a record with prescribed keys (oper, nonum) from the SDB
        /// Note table main.sd_note.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="oper"></param>
        /// <param name="nonum"></param>
        /// <returns></returns>
        public static int SdDeleteNote(SQLHANDLE hConn, string oper, string nonum)
        {
            //...Log2.v("\n\nDynSdbNote.SdDeleteNote(): Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string delete_buf = String.Format(mDELETE, mTableName, oper, nonum);

            //...Log2.v("\nDynSdbNote.SdDeleteNote(): query = " + delete_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, delete_buf, delete_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("mtDeleteNote01 -- Error deleting from main.sd_note");
                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Log2.e("\nDynSdbNote.SdDeleteNote(): ERROR: SQLExecDirect() failed for query = " + delete_buf);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + delete_buf;
                //...Log2.v("\nDynSdbNote.SdDeleteNote(): Deletion of main.sd_note records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynSdbNote.SdDeleteNote(): Exit");
            return 0;
        }

        /// <summary>
        /// This method returns true if a record exists in the SDB Note table <b>main.sd_note</b>
        /// that has a prescribed key (oper, nonum).
        /// </summary>
        /// <param name="oper"></param>
        /// <param name="nonum"></param>
        /// <returns></returns>
        public static bool SdbRecordExistsWithKey(string oper, string nonum)
        {
            string whereClause = String.Format(" oper = '{0}' AND nonum = '{1}'", oper, nonum);

            int nRecords = Ssutil.DbCountRows(mTableName, whereClause);

            return (nRecords > 0);
        }

        /// <summary>
        /// This method returns a populated SdNote object and associated ODBC nullInds
        /// corresponding to the record in the main SDB Note table 'main.sd_note' that has the 
        /// prescribed key (oper, nonum).
        /// </summary>
        /// <param name="oper"></param>
        /// <param name="nonum"></param>
        /// <param name="sdNote"></param>
        /// <param name="nullInds"></param>
        /// <returns>True if a record with the prescribed key was found in the table; otherwise false.</returns>
        public static bool FetchRecordWithKey(string oper, string nonum, out SdNote sdNote, out SQLLEN[] nullInds)
        {
            // 'out' requirements.
            sdNote = null;
            nullInds = null;

            bool isSuccessful = false;

            string whereClause = String.Format(" oper = '{0}' AND nonum = '{1}'", oper, nonum);

            // Do the 'SELECT'.
            int nCursor = SdSelectNote(whereClause, "");

            if (nCursor < 0)
            {
                Log2.e("\nDynSdbNote.FetchRecordWithKeys(): ERROR: Select() failed, nCursor = " + nCursor);
                return false;
            }

            // Do the 'FETCH'.
            int rc = SdFetchNote(nCursor, out sdNote, out nullInds);

            if (rc == Constant.SUCCESS)
            {
                isSuccessful = true;
            }
            else
            {
                Log2.e("\nDynSdbNote.FetchRecordWithKeys(): ERROR: Fetch() failed, rc = " + rc);
                isSuccessful = false;
            }

            return isSuccessful;
        }

    }
}














```
