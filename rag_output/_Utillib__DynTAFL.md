# Documented File: DynTAFL.cs
**Repository Path:** `_Utillib\DynTAFL.cs`
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
    /// This class provides methods to select, fetch, insert, update and delete equipment records from
    /// a TAFL table, as produced by Bill Venn when importing ISED data.
    /// </summary>
    public class DynTAFL
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

            //...Log2.v("\r\nDynTAFL.GetNextFreeCursor(): nNextFreeCursor = " + nNextFreeCursor);
            if (nNextFreeCursor >= Constant.NUM_CURSORS_FEW)
            {
                //	Search for a free cursors in the list
                for (int nInd = 0; nInd < Constant.NUM_CURSORS_FEW; nInd++)
                {
                    //...Log2.v("\r\nDynTAFL.GetNextFreeCursor(): nInd = " + nInd);
                    if (!cursors[nInd].cursorOpen)
                    {
                        curHandle = nInd;
                        break;
                    }
                }

                if (curHandle == -1)
                {
                    // There are no more open cursors
                    GenUtil.SetErr("DynTAFL01 -- No more open cursors.");
                    Application.Exit("\r\nDynTAFL.GetNextFreeCursor(): ERROR: No available cursors.");
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

            //...Log2.v("\r\nDynTAFL.GetNextFreeCursor(): found available cursor: curHandle = " + curHandle);
            cursors[curHandle].cursorOpen = true;

            return curHandle;
        }

        /// <summary>
        /// Selects records from the prescribed TAFL database table i.a.w.
        /// the prescribed SQL search criteria and ordering clauses; the 
        /// method returns an index to a cursor object that can be used by subsequent
        /// calls to FetchTAFL().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// </remarks>
        /// <param name="tableName"> - fully-qualified SQL table name.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for FetchTAFL() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECUTE_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int SelectTAFL(string tableName, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynTAFL.SelectTAFL(): Entry");

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
                    Log2.e("\nDynTAFL.SelectTAFL(): ERROR : Exit: call to SQLAllocHandle() failed, sqlRet = " + sqlRet);
                    return Error.ODBC_SQLALLOCHANDLE_FAILED;
                }

                TAFL.BindPtrsToCols(mhStmt, out mTgtValPtrs, out mNullIndPtrs);
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
                Log2.e("\nDynTAFL.SelectTAFL(): ERROR : Exit: Attempt to close the cursor on the reusable statement handle failed.");
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

            //...Log2.v("\r\nDynTAFL.SelectTAFL(): stmt_buf = \r\n" + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(mhStmt, stmt_buf, stmt_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nDynTAFL.SelectTAFL(): ERROR : Exit: call to SQLExecDirect() failed, sqlRet = " + sqlRet);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            mMagicNumber++;

            // Populate the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = SQLHANDLE.Zero;
            cursors[curHandle].hConn = SQLHANDLE.Zero;
            cursors[curHandle].magicNumber = mMagicNumber;

            //...Log2.v("\n\nDynTAFL.SelectTAFL(): Exit");
            return (curHandle);
        }

        /// <summary>
        /// Retrieves a single equipment record from the previously prescribed TAFL table using 
        /// the cursor object created by a previous call to SelectTAFL().
        /// </summary>
        /// <param name="nCursor"> - FtCursor object.</param>
        /// <param name="tafl"> - a TAFL object populated with data from the row.</param>
        /// <param name="nullInds"> - array of ODBC nullInds for TAFL object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int FetchTAFL(int nCursor, out TAFL tafl, out SQLLEN[] nullInds)
        {
            // 'out' requirments.
            tafl = null;
            nullInds = null;

            //...Log2.v("\n\nDynTAFL.FetchTAFL(): Entry");

            int nRet = -666;

            // Test for contiguity, i.e. that the previous select and 
            // this fetch are a matched pair.
            if (cursors[nCursor].magicNumber != mMagicNumber)
            {
                Log2.e("\n\nDynTAFL.FetchTAFL(): Exit: ERROR: failed contiguity test.");

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
                    //...Log2.v("\n\nDynTAFL.FetchTAFL(): Exit: sqlRet == ODBC.SQL_NO_DATA");
                    return ODBC.SQL_NO_DATA;
                }
                else
                {
                    // We have an error.
                    Log2.e("\n\nDynTAFL.FetchTAFL(): Exit: ERROR: SQLFetch(): sqlRet = " + sqlRet);
                    Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, mhStmt);
                    return -11;
                }
            }
            else
            {
                // We had a successful SQLFetch() call1ation; now get the record's column data.
                try
                {
                    TAFL.ReadColBindings(mTgtValPtrs, mNullIndPtrs, out tafl, out nullInds);

                    nRet = Constant.SUCCESS;
                }
                catch (Exception e)
                {
                    string str = String.Format("Fetch() failed for reason: " + e.Message);
                    GenUtil.SetError(1011, str);
                    Log2.e("\n\nDynTAFL.FetchTAFL(): Exit: ERROR: call to ReadColBindings() threw an exception:\n" + e.Message);
                    return -22;
                } //try-catch

            } //if-else

            //...Log2.v(TAFL.ToStringWN(nullInds));

            //...Log2.v("\n\nDynTAFL.FetchTAFL(): Exit");
            return nRet;
        } //end of FetchTAFL

        /// <summary>
        /// Closes (releases) a cursor instantiated by a previous call to SelectTAFL(); 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        public static int CloseTAFL(int curHandle)
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
        /// This method returns true if a record exists in the prescribed TAFL table
        /// that has a prescribed key ('keyfield').
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="keyField"></param>
        /// <returns></returns>
        public static bool TAFLRecordExistsWithKey(string tableName, string keyField)
        {
            string whereClause = String.Format(" keyfield = '{0}' ", keyField);

            int nRecords = Ssutil.DbCountRows(tableName, whereClause);

            return (nRecords > 0);
        }

        /// <summary>
        /// This method returns a populated TAFL object and associated ODBC nullInds
        /// corresponding to the record in the prescribed TAFL table that has the 
        /// prescribed key ('keyfield').
        /// </summary>
        /// <param name="tableName"> - fully-qualified SQL table name.</param>
        /// <param name="keyField"></param>
        /// <param name="tafl"></param>
        /// <param name="nullInds"></param>
        /// <returns>True if a record with the prescribed key was found in the table; otherwise false.</returns>
        public static bool FetchTAFLRecordWithKey(string tableName, string keyField, out TAFL tafl, out SQLLEN[] nullInds)
        {
            // 'out' requirements.
            tafl = null;
            nullInds = null;

            bool isSuccessful = false;

            string whereClause = String.Format(" keyfield = '{0}' ", keyField);

            // Do the 'SELECT'.
            int nCursor = SelectTAFL(tableName, whereClause, "");

            if (nCursor < 0)
            {
                Log2.e("\nDynTAFL.FetchTAFLRecordWithKey(): ERROR: Select() failed, nCursor = " + nCursor);
                return false;
            }

            // Do the 'FETCH'.
            int rc = FetchTAFL(nCursor, out tafl, out nullInds);

            if (rc == Constant.SUCCESS)
            {
                isSuccessful = true;
            }
            else
            {
                Log2.e("\nDynTAFL.FetchRecordWithKey(): ERROR: Fetch() failed, rc = " + rc);
                isSuccessful = false;
            }

            // Clean up.
            CloseTAFL(nCursor);

            return isSuccessful;
        }

        /// <summary>
        /// Inserts a TAFL record into the database using column values prescribed 
        /// by the fields of the TAFL object.
        /// </summary>
        /// <param name="tableName"> - name of the SQL table to INSERT into.</param>
        /// <param name="tAFL"> - a FtSite object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pSite</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int InsertTAFL(string tableName, TAFL tAFL, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynTAFL.InsertTAFL(): Entry");

            string update_buf;

            SQLHANDLE hStmt;

            SQLHANDLE hConn = Ssutil.NewConn();

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            //Create the SQL insert statement.
            update_buf =TAFL.BuildSqlInsertString(tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pSite
            //into global memory with an SQLPOINTER pointer assigned to each one. FtSite provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = tAFL.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            // This integer is used to enumerate the binding sequence; ODBC definition is that first binding is N = 1;
            int colNum = -1;
            try
            {
                colNum = 1;
                Ssutil.DbBindStringInput(hStmt, colNum, "TxRx", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.TxRx.Length, nullIndPtr[colNum - 1]);

                colNum = 2;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "FrequencyMhz", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 3;
                Ssutil.DbBindStringInput(hStmt, colNum, "Frequencyrecordidentifier", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Frequencyrecordidentifier.Length, nullIndPtr[colNum - 1]);

                colNum = 4;
                Ssutil.DbBindStringInput(hStmt, colNum, "Regulatoryservice", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Regulatoryservice.Length, nullIndPtr[colNum - 1]);

                colNum = 5;
                Ssutil.DbBindStringInput(hStmt, colNum, "CommunicationType", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.CommunicationType.Length, nullIndPtr[colNum - 1]);

                colNum = 6;
                Ssutil.DbBindStringInput(hStmt, colNum, "Conformitytofrequencyplan", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Conformitytofrequencyplan.Length, nullIndPtr[colNum - 1]);

                colNum = 7;
                Ssutil.DbBindStringInput(hStmt, colNum, "Frequencyallocationname", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Frequencyallocationname.Length, nullIndPtr[colNum - 1]);

                colNum = 8;
                Ssutil.DbBindStringInput(hStmt, colNum, "Channel", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Channel.Length, nullIndPtr[colNum - 1]);

                colNum = 9;
                Ssutil.DbBindStringInput(hStmt, colNum, "Internationalcoordinationnumber", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Internationalcoordinationnumber.Length, nullIndPtr[colNum - 1]);

                colNum = 10;
                Ssutil.DbBindStringInput(hStmt, colNum, "Analogdigital", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Analogdigital.Length, nullIndPtr[colNum - 1]);

                colNum = 11;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "OccupiedbandwidthkHz", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 12;
                Ssutil.DbBindStringInput(hStmt, colNum, "Designationofemission", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Designationofemission.Length, nullIndPtr[colNum - 1]);

                colNum = 13;
                Ssutil.DbBindStringInput(hStmt, colNum, "Modulationtype", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Modulationtype.Length, nullIndPtr[colNum - 1]);

                colNum = 14;
                Ssutil.DbBindStringInput(hStmt, colNum, "Filtrationinstalled", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Filtrationinstalled.Length, nullIndPtr[colNum - 1]);

                colNum = 15;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "TxeffectiveradiatedpowerERPdBW", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 16;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "TxtransmitterpowerW", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 17;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "TotallossesdB", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 18;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "AnalogcapacityChannels", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 19;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "DigitalcapacityMbits", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 20;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "RxunfadedreceivedsignalleveldBW", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 21;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "RxthresholdsignallevelforBER10e3dBW", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 22;
                Ssutil.DbBindStringInput(hStmt, colNum, "Manufacturer", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Manufacturer.Length, nullIndPtr[colNum - 1]);

                colNum = 23;
                Ssutil.DbBindStringInput(hStmt, colNum, "Modelnumber", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Modelnumber.Length, nullIndPtr[colNum - 1]);

                colNum = 24;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "AntennagaindBi", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 25;
                Ssutil.DbBindStringInput(hStmt, colNum, "Antennapattern", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Antennapattern.Length, nullIndPtr[colNum - 1]);

                colNum = 26;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "Halfpower3dBbeamwidthdeg", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 27;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "FronttobackratiodB", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 28;
                Ssutil.DbBindStringInput(hStmt, colNum, "Polarization", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Polarization.Length, nullIndPtr[colNum - 1]);

                colNum = 29;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "Heightabovegroundlevelm", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 30;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "Azimuthofmainlobedeg", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 31;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "Verticalelevationangledeg", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 32;
                Ssutil.DbBindStringInput(hStmt, colNum, "Stationlocation", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Stationlocation.Length, nullIndPtr[colNum - 1]);

                colNum = 33;
                Ssutil.DbBindStringInput(hStmt, colNum, "Licenseestationreference", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Licenseestationreference.Length, nullIndPtr[colNum - 1]);

                colNum = 34;
                Ssutil.DbBindStringInput(hStmt, colNum, "Callsign", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Callsign.Length, nullIndPtr[colNum - 1]);

                colNum = 35;
                Ssutil.DbBindStringInput(hStmt, colNum, "Typeofstation", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Typeofstation.Length, nullIndPtr[colNum - 1]);

                colNum = 36;
                Ssutil.DbBindStringInput(hStmt, colNum, "ITUclassofstation", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.ITUclassofstation.Length, nullIndPtr[colNum - 1]);

                colNum = 37;
                Ssutil.DbBindStringInput(hStmt, colNum, "Stationcostcategory", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Stationcostcategory.Length, nullIndPtr[colNum - 1]);

                colNum = 38;
                Ssutil.DbBindIntInput(hStmt, colNum, "Numberofidenticalstations", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 39;
                Ssutil.DbBindStringInput(hStmt, colNum, "Referenceidentifier", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Referenceidentifier.Length, nullIndPtr[colNum - 1]);

                colNum = 40;
                Ssutil.DbBindStringInput(hStmt, colNum, "Provinces", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Provinces.Length, nullIndPtr[colNum - 1]);

                colNum = 41;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "LatitudeWGS84", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 42;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "LongitudeWGS84", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 43;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "Groundelevationabovemeansealevelm", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 44;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "Antennastructureheightabovegroundlevelm", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 45;
                Ssutil.DbBindStringInput(hStmt, colNum, "Congestionzone", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Congestionzone.Length, nullIndPtr[colNum - 1]);

                colNum = 46;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "Radiusofoperationkm", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 47;
                Ssutil.DbBindStringInput(hStmt, colNum, "Satellitename", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Satellitename.Length, nullIndPtr[colNum - 1]);

                colNum = 48;
                Ssutil.DbBindStringInput(hStmt, colNum, "Authorizationnumber", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Authorizationnumber.Length, nullIndPtr[colNum - 1]);

                colNum = 49;
                Ssutil.DbBindStringInput(hStmt, colNum, "MWService", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.MWService.Length, nullIndPtr[colNum - 1]);

                colNum = 50;
                Ssutil.DbBindStringInput(hStmt, colNum, "Subservice", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Subservice.Length, nullIndPtr[colNum - 1]);

                colNum = 51;
                Ssutil.DbBindStringInput(hStmt, colNum, "Licencetype", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Licencetype.Length, nullIndPtr[colNum - 1]);

                colNum = 52;
                Ssutil.DbBindStringInput(hStmt, colNum, "Authorizationstatus", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Authorizationstatus.Length, nullIndPtr[colNum - 1]);

                colNum = 53;
                Ssutil.DbBindStringInput(hStmt, colNum, "Inservicedate", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Inservicedate.Length, nullIndPtr[colNum - 1]);

                colNum = 54;
                Ssutil.DbBindStringInput(hStmt, colNum, "Accountnumber", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Accountnumber.Length, nullIndPtr[colNum - 1]);

                colNum = 55;
                Ssutil.DbBindStringInput(hStmt, colNum, "Licenseename", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Licenseename.Length, nullIndPtr[colNum - 1]);

                colNum = 56;
                Ssutil.DbBindStringInput(hStmt, colNum, "Licenseeaddress", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Licenseeaddress.Length, nullIndPtr[colNum - 1]);

                colNum = 57;
                Ssutil.DbBindStringInput(hStmt, colNum, "Operationalstatus", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Operationalstatus.Length, nullIndPtr[colNum - 1]);

                colNum = 58;
                Ssutil.DbBindStringInput(hStmt, colNum, "Stationclass", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Stationclass.Length, nullIndPtr[colNum - 1]);

                colNum = 59;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "HorizontalpowerW", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 60;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "VerticalpowerW", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 61;
                Ssutil.DbBindStringInput(hStmt, colNum, "Standbytransmitterinformation", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.Standbytransmitterinformation.Length, nullIndPtr[colNum - 1]);

                colNum = 62;
                Ssutil.DbBindStringInput(hStmt, colNum, "MICSoper", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.MICSoper.Length, nullIndPtr[colNum - 1]);

                colNum = 63;
                Ssutil.DbBindStringInput(hStmt, colNum, "MICSopnote", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.MICSopnote.Length, nullIndPtr[colNum - 1]);

                colNum = 64;
                Ssutil.DbBindStringInput(hStmt, colNum, "MICSoprtyp", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.MICSoprtyp.Length, nullIndPtr[colNum - 1]);

                colNum = 65;
                Ssutil.DbBindStringInput(hStmt, colNum, "MICScompany", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.MICScompany.Length, nullIndPtr[colNum - 1]);

                colNum = 66;
                Ssutil.DbBindStringInput(hStmt, colNum, "RecordAction", parameterValuePtr[colNum - 1], (SQLULEN)tAFL.RecordAction.Length, nullIndPtr[colNum - 1]);

                colNum = 67;
                Ssutil.DbBindIntInput(hStmt, colNum, "Keyfield", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);
            }
            catch
            {
                Log2.e("\n\nDynTAFL.InsertTAFL(): ERROR: call to Ssutil.DbBindXXXInput() failed for colNum = {0}", colNum);
                Ssutil.DisConnStmt(hConn, hStmt);
                return -1;
            }

            //...Log2.v("nDynTAFL.InsertTAFL(): SQLExecDirect():\r\n" + update_buf);
            SQLRETURN sqlRet;
            sqlRet = ODBC.SQLExecDirect(hStmt, update_buf, update_buf.Length);

            //...Log2.v("\r\nDynTAFL.InsertTAFL(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynTAFL.InsertTAFL(): ERROR: call to SQLExecDirect failed for query:\n{0}", update_buf);
                Log2.e("\n" + ODBC.GetDiagnostics(hStmt, update_buf));
                Log2.e("\n{0}", tAFL.ToString());
                Ssutil.DisConnStmt(hConn, hStmt);
                return -2;
            }

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynTAFL.InsertTAFL(): Exit");
            return 0;
        }


    }
}















```
