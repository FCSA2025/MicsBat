# Documented File: DynMtPhantomTxLink.cs
**Repository Path:** `_Utillib\DynMtPhantomTxLink.cs`
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
    /// This class provides methods to select and fetch records from a SQL Server MtPhantomTxLink table.
    /// </summary>
    public class DynMtPhantomTxLink
    {
        private const string mSELECT = "SELECT * FROM {0} ";

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

            //...Log2.v("\r\nDynMtPhantomTxLink.GetNextFreeCursor(): nNextFreeCursor = " + nNextFreeCursor);
            if (nNextFreeCursor >= Constant.NUM_CURSORS_FEW)
            {
                //	Search for a free cursors in the list
                for (int nInd = 0; nInd < Constant.NUM_CURSORS_FEW; nInd++)
                {
                    //...Log2.v("\r\nDynMtPhantomTxLink.GetNextFreeCursor(): nInd = " + nInd);
                    if (!cursors[nInd].cursorOpen)
                    {
                        curHandle = nInd;
                        break;
                    }
                }

                if (curHandle == -1)
                {
                    // There are no more open cursors
                    GenUtil.SetErr("DynMtPhantomTxLink01 -- No more open cursors.");
                    Application.Exit("\r\nDynMtPhantomTxLink.GetNextFreeCursor(): ERROR: No available cursors.");
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

            //...Log2.v("\r\nDynMtPhantomTxLink.GetNextFreeCursor(): found available cursor: curHandle = " + curHandle);
            cursors[curHandle].cursorOpen = true;

            return curHandle;
        }

        /// <summary>
        /// Selects records from the prescribed MtPhantomTxLink database table i.a.w.
        /// the prescribed SQL search criteria and ordering clauses; the 
        /// method returns an index to a cursor object that can be used by subsequent
        /// calls to FetchMtPhantomTxLink().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// </remarks>
        /// <param name="tableName"> - fully-qualified SQL table name.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for FetchMtPhantomTxLink() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECUTE_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int SelectMtPhantomTxLink(string tableName, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynMtPhantomTxLink.SelectMtPhantomTxLink(): Entry");

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
                    Log2.e("\nDynMtPhantomTxLink.SelectMtPhantomTxLink(): ERROR : Exit: call to SQLAllocHandle() failed, sqlRet = " + sqlRet);
                    return Error.ODBC_SQLALLOCHANDLE_FAILED;
                }

                MtPhantomTxLink.BindPtrsToCols(mhStmt, out mTgtValPtrs, out mNullIndPtrs);
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
                Log2.e("\nDynMtPhantomTxLink.SelectMtPhantomTxLink(): ERROR : Exit: Attempt to close the cursor on the reusable statement handle failed.");
                Application.Exit(666);
            }

            // Create the SQL query string.
            string stmt_buf = String.Format(mSELECT, tableName);

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

            //...Log2.v("\r\nDynMtPhantomTxLink.SelectMtPhantomTxLink(): stmt_buf = \r\n" + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(mhStmt, stmt_buf, stmt_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nDynMtPhantomTxLink.SelectMtPhantomTxLink(): ERROR : Exit: call to SQLExecDirect() failed, sqlRet = " + sqlRet);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            mMagicNumber++;

            // Populate the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = SQLHANDLE.Zero;
            cursors[curHandle].hConn = SQLHANDLE.Zero;
            cursors[curHandle].magicNumber = mMagicNumber;

            //...Log2.v("\n\nDynMtPhantomTxLink.SelectMtPhantomTxLink(): Exit");
            return (curHandle);
        }

        /// <summary>
        /// Retrieves a single equipment record from the previously prescribed MtPhantomTxLink table using 
        /// the cursor object created by a previous call to SelectMtPhantomTxLink().
        /// </summary>
        /// <param name="nCursor"> - FtCursor object.</param>
        /// <param name="mtPhantomTxLink"> - a MtPhantomTxLink object populated with data from the row.</param>
        /// <param name="nullInds"> - array of ODBC nullInds for MtPhantomTxLink object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int FetchMtPhantomTxLink(int nCursor, out MtPhantomTxLink mtPhantomTxLink, out SQLLEN[] nullInds)
        {
            // 'out' requirments.
            mtPhantomTxLink = null;
            nullInds = null;

            //...Log2.v("\n\nDynMtPhantomTxLink.FetchMtPhantomTxLink(): Entry");

            int nRet = -666;

            // Test for contiguity, i.e. that the previous select and 
            // this fetch are a matched pair.
            if (cursors[nCursor].magicNumber != mMagicNumber)
            {
                Log2.e("\n\nDynMtPhantomTxLink.FetchMtPhantomTxLink(): Exit: ERROR: failed contiguity test.");

                return -666;
            }

            // Fetch the next row of selected Town data.
            SQLRETURN sqlRet = ODBC.SQLFetch(mhStmt);

            // Check the SQLRETURN and act accordingly.
            if (!ODBC.IsOK(sqlRet))
            {
                // If no more Town records available then break out the while-loop.
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\n\nDynMtPhantomTxLink.FetchMtPhantomTxLink(): Exit: sqlRet == ODBC.SQL_NO_DATA");
                    return ODBC.SQL_NO_DATA;
                }
                else
                {
                    // We have an error.
                    Log2.e("\n\nDynMtPhantomTxLink.FetchMtPhantomTxLink(): Exit: ERROR: SQLFetch(): sqlRet = " + sqlRet);
                    Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, mhStmt);
                    return -11;
                }
            }
            else
            {
                // We had a successful SQLFetch() call1ation; now get the record's column data.
                try
                {
                    MtPhantomTxLink.ReadColBindings(mTgtValPtrs, mNullIndPtrs, out mtPhantomTxLink, out nullInds);

                    nRet = Constant.SUCCESS;
                }
                catch (Exception e)
                {
                    string str = String.Format("Fetch() failed for reason: " + e.Message);
                    GenUtil.SetError(1011, str);
                    Log2.e("\n\nDynMtPhantomTxLink.FetchMtPhantomTxLink(): Exit: ERROR: call to ReadColBindings() threw an exception:\n" + e.Message);
                    return -22;
                } //try-catch

            } //if-else

            //...Log2.v(MtPhantomTxLink.ToStringWN(nullInds));

            //...Log2.v("\n\nDynMtPhantomTxLink.FetchMtPhantomTxLink(): Exit");
            return nRet;
        } //end of FetchMtPhantomTxLink

        /// <summary>
        /// Closes (releases) a cursor instantiated by a previous call to SelectMtPhantomTxLink(); 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        public static int CloseMtPhantomTxLink(int curHandle)
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
        /// This method returns true if a record exists in the prescribed MtPhantomTxLink table
        /// that has a prescribed key ('keyfield').
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="keyField"></param>
        /// <returns></returns>
        public static bool MtPhantomTxLinkRecordExistsWithKey(string tableName, string keyField)
        {
            string whereClause = String.Format(" keyfield = '{0}' ", keyField);

            int nRecords = Ssutil.DbCountRows(tableName, whereClause);

            return (nRecords > 0);
        }

        /// <summary>
        /// This method returns a populated MtPhantomTxLink object and associated ODBC nullInds
        /// corresponding to the record in the prescribed MtPhantomTxLink table that has the 
        /// prescribed key ('keyfield').
        /// </summary>
        /// <param name="tableName"> - fully-qualified SQL table name.</param>
        /// <param name="keyField"></param>
        /// <param name="mtPhantomTxLink"></param>
        /// <param name="nullInds"></param>
        /// <returns>True if a record with the prescribed key was found in the table; otherwise false.</returns>
        public static bool FetchMtPhantomTxLinkRecordWithKey(string tableName, string keyField, out MtPhantomTxLink mtPhantomTxLink, out SQLLEN[] nullInds)
        {
            // 'out' requirements.
            mtPhantomTxLink = null;
            nullInds = null;

            bool isSuccessful = false;

            string whereClause = String.Format(" keyfield = '{0}' ", keyField);

            // Do the 'SELECT'.
            int nCursor = SelectMtPhantomTxLink(tableName, whereClause, "");

            if (nCursor < 0)
            {
                Log2.e("\nDynMtPhantomTxLink.FetchMtPhantomTxLinkRecordWithKey(): ERROR: Select() failed, nCursor = " + nCursor);
                return false;
            }

            // Do the 'FETCH'.
            int rc = FetchMtPhantomTxLink(nCursor, out mtPhantomTxLink, out nullInds);

            if (rc == Constant.SUCCESS)
            {
                isSuccessful = true;
            }
            else
            {
                Log2.e("\nDynMtPhantomTxLink.FetchRecordWithKey(): ERROR: Fetch() failed, rc = " + rc);
                isSuccessful = false;
            }

            // Clean up.
            CloseMtPhantomTxLink(nCursor);

            return isSuccessful;
        }




    }
}















```
