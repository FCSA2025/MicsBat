# Documented File: DynMeChan.cs
**Repository Path:** `_Utillib\DynMeChan.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
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

    public class DynMeChan
    {
        private const string mBaseTableName = "me_chan";

        private const string mMdbTableName = "main." + mBaseTableName;

        private static string mTableName = mMdbTableName;

        private static bool mMdbWriteEnabled = true;

        private static bool mSpoofModeIsOff = true;

        private const string mSELECT = "SELECT * FROM {0} ";

        private const string mUPDATE = "UPDATE {0} SET {1} WHERE location='{2}' AND call1='{3}' AND chid='{4}' ";

        private const string mINSERT = "INSERT INTO {0} VALUES ({1}) ";

        private const string mDELETE = "DELETE FROM {0} WHERE location='{1}' AND call1='{2}' AND chid='{3}' ";

        private const string mCLOC_UPDATE = "UPDATE {0} SET location='{1}', userid='{2}', mdate='{3}', mtime='{4}' WHERE location='{5}' ";

        private const string mCCAL_UPDATE = "UPDATE {0} SET call1='{1}', userid='{2}', mdate='{3}', mtime='{4}' WHERE call1='{5}' ";


        //====================================================================================
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;
        //====================================================================================

        public static string TableName
        {
            get { return mTableName; }
        }

        public static bool WriteEnabled
        {
            get { return mMdbWriteEnabled; }
        }

        /// <summary>
        /// This method can be used to set whether the MDB <b>main.me_chan</b> table should be written to,
        /// (SQL DELETE, INSERT and/or UPDATE queries) or not; it can also set whether the SQL queries should
        /// target the actual main.me_chan table or a user's 'spoof' table (e.g. hulme.audit_trail).
        /// </summary>
        /// <param name="mdbWriteEnabled"> - boolean that prescribes whether me_chan record
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
                    Log2.e("\nDynMeChan.SetState(): ERROR: spoofMode is ON but userSchema is null or whitespace.");
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

            if (curHandle != 0)
            {
                //...Log2.v("\r\nDynMeChan.GetNextFreeCursor(): found available cursor: curHandle = " + curHandle);
            }

            return curHandle;
        }

        /// <summary>
        /// Closes a cursor associated with a PDF TS Channel table. Inputs: curHandle - 
        /// the cursor handle returned by mtSelectChannel. Outputs: None.  
        /// </summary>
        /// <param name="curHandle"></param>
        /// <returns></returns>
        public static int MeCloseChan(int curHandle)
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

            // Close the antenna cursor.

            // First close and release the statement handle.
            SQLHANDLE hStmt = cursors[curHandle].hStmt;
            //!!ODBC.SQLCloseCursor(hStmt);
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            //!!Console.Error.Write("\nDynMeChan.MeCloseChan(): Free hStmt = {0:X}", (long)hStmt);

            // Now disconnect from ODBC.
            // Use Ssutil.DisConn to close the connection because it maintains a count
            // of the number of open connections.
            // Note that DisConn() also frees the connection handle.
            Ssutil.DisConn(cursors[curHandle].hConn);

            // Modify the cursor accordingly.
            cursors[curHandle].hConn = SQLHDBC.Zero;
            cursors[curHandle].cursorOpen = false;

            return Constant.SUCCESS;
        }


        /// <summary>
        /// Select a site record from a site table.  
        /// </summary>
        /// <remarks>
        /// This method performs the setup for an open cursor operation. The caller 
        /// identifies the site table and the search criteria for the selection and how 
        /// the rows are to be ordered. If searchCriteria is NULL then all the rows are 
        /// retrieved.  
        /// </remarks>
        /// <param name="searchCriteria"> - the search criteria excluding the 'where'</param>
        /// <param name="orderBy"> - the order by info of the select statement excluding the actual 'order by'</param>
        /// <returns></returns>
        public static int MeSelectChan(string searchCriteria, /* selection criteria */
                   string orderBy)              /* how is selection to be ordered */
        {
            int curHandle;          /* cursor handle */

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            /* Construct the select clause */
            string stmt_buf = String.Format(mSELECT, mTableName);

            /* Construct the 'where' part of the select clause */
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                /* Search criteria was specified */
                stmt_buf += " where ";
                stmt_buf += searchCriteria;
            }

            /* If "order by" is specified then add order by clause */
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                /* Order by was specified */
                stmt_buf += " order by ";
                stmt_buf += orderBy;
            }

            //	allocate and open the handle
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            //...Log2.v("\nDynMeChan.MeSelectChan: query = " + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nDynMeChan.MeSelectChan: ERROR: SQLExecDirect() failed, returned " + sqlRet);
                Ssutil.DbGetDiagStmt(hStmt, "dynMDBChannel - Could not execute.:\r\n");
                curHandle = -2;
            }
            else
            {
                curHandle = GetNextFreeCursor();

                //	Inititialize the cursor handle structure
                cursors[curHandle].pastLastRow = false;
                cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
                cursors[curHandle].hStmt = hStmt;
                cursors[curHandle].hConn = hConn;
                cursors[curHandle].tableName = mTableName;
            }
            //!!Console.Error.Write("\ntpMdbPdfGet.selectEarthChan(): curHandle = " + curHandle);
            //!!Console.Error.Write("\ntpMdbPdfGet.selectEarthChan(): hStmt = {0:X}", (long)hStmt);

            return curHandle;
        }

        /// <summary>
        /// To fetch a specific chanel record from the master data base.  
        /// </summary>
        /// <param name="curHandle"></param>
        /// <param name="meChan"></param>
        /// <param name="nullInd"> - caller's array of null indicators</param>
        /// <returns></returns>
        public static int MeFetchChan(int curHandle, out MeChan meChan, out SQLLEN[] nullInd)      /* caller's array of null indicators */
        {
            // 'out' requirements.
            meChan = null;
            nullInd = null;

            FetchCount.IncrementMeChan();

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            sqlRet = ODBC.SQLFetch(hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\nDynMeChan.MeFetchChan(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\nDynMeChan.MeFetchChan(): ERROR: SQLFetch() failed, returned " + sqlRet);
                    Ssutil.DbGetDiagStmt(hStmt, "meFetchChan01 - Fetch Error: ");
                    return Error.ODBC_FETCH_FAILED;
                }
            }

            meChan = new MeChan();
            nullInd = NullHelper.CreateArrayOfNullInd(MeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            //	Now read in the fields
            try
            {
                // Initialize auto-increment 'Get' column counter.
                Ssutil.DbStartGets();

                Ssutil.DbGetString(hStmt, 0, "location", out meChan.location, MeChan.LOCATION_SZ, out nullInd[MeChan.LOCATION]);

                Ssutil.DbGetString(hStmt, 0, "call1", out meChan.call1, MeChan.CALL1_SZ, out nullInd[MeChan.CALL1]);

                Ssutil.DbGetString(hStmt, 0, "chid", out meChan.chid, MeChan.CHID_SZ, out nullInd[MeChan.CHID]);

                Ssutil.DbGetDouble(hStmt, 0, "freqtx", out meChan.freqtx, out nullInd[MeChan.FREQTX]);

                Ssutil.DbGetString(hStmt, 0, "poltx", out meChan.poltx, MeChan.POLTX_SZ, out nullInd[MeChan.POLTX]);

                Ssutil.DbGetFloat(hStmt, 0, "maxtxpower", out meChan.maxtxpower, out nullInd[MeChan.MAXTXPOWER]);

                Ssutil.DbGetFloat(hStmt, 0, "pwrtx", out meChan.pwrtx, out nullInd[MeChan.PWRTX]);

                Ssutil.DbGetFloat(hStmt, 0, "p4khz", out meChan.p4khz, out nullInd[MeChan.P4KHZ]);

                Ssutil.DbGetString(hStmt, 0, "eqpttx", out meChan.eqpttx, MeChan.EQPTTX_SZ, out nullInd[MeChan.EQPTTX]);

                Ssutil.DbGetString(hStmt, 0, "traftx", out meChan.traftx, MeChan.TRAFTX_SZ, out nullInd[MeChan.TRAFTX]);

                Ssutil.DbGetString(hStmt, 0, "stattx", out meChan.stattx, MeChan.STATTX_SZ, out nullInd[MeChan.STATTX]);

                Ssutil.DbGetString(hStmt, 0, "feetx", out meChan.feetx, MeChan.FEETX_SZ, out nullInd[MeChan.FEETX]);

                Ssutil.DbGetDouble(hStmt, 0, "freqrx", out meChan.freqrx, out nullInd[MeChan.FREQRX]);

                Ssutil.DbGetString(hStmt, 0, "polrx", out meChan.polrx, MeChan.POLRX_SZ, out nullInd[MeChan.POLRX]);

                Ssutil.DbGetFloat(hStmt, 0, "pwrrx", out meChan.pwrrx, out nullInd[MeChan.PWRRX]);

                Ssutil.DbGetString(hStmt, 0, "eqptrx", out meChan.eqptrx, MeChan.EQPTRX_SZ, out nullInd[MeChan.EQPTRX]);

                Ssutil.DbGetString(hStmt, 0, "trafrx", out meChan.trafrx, MeChan.TRAFRX_SZ, out nullInd[MeChan.TRAFRX]);

                Ssutil.DbGetString(hStmt, 0, "statrx", out meChan.statrx, MeChan.STATRX_SZ, out nullInd[MeChan.STATRX]);

                Ssutil.DbGetFloat(hStmt, 0, "i20", out meChan.i20, out nullInd[MeChan.I20]);

                Ssutil.DbGetFloat(hStmt, 0, "it01", out meChan.it01, out nullInd[MeChan.IT01]);

                Ssutil.DbGetFloat(hStmt, 0, "ip01", out meChan.ip01, out nullInd[MeChan.IP01]);

                Ssutil.DbGetString(hStmt, 0, "feerx", out meChan.feerx, MeChan.FEERX_SZ, out nullInd[MeChan.FEERX]);

                Ssutil.DbGetString(hStmt, 0, "notc", out meChan.notc, MeChan.NOTC_SZ, out nullInd[MeChan.NOTC]);

                Ssutil.DbGetString(hStmt, 0, "srvctx", out meChan.srvctx, MeChan.SRVCTX_SZ, out nullInd[MeChan.SRVCTX]);

                Ssutil.DbGetString(hStmt, 0, "srvcrx", out meChan.srvcrx, MeChan.SRVCRX_SZ, out nullInd[MeChan.SRVCRX]);

                Ssutil.DbGetString(hStmt, 0, "mdate", out meChan.mdate, MeChan.MDATE_SZ, out nullInd[MeChan.MDATE]);

                Ssutil.DbGetString(hStmt, 0, "mtime", out meChan.mtime, MeChan.MTIME_SZ, out nullInd[MeChan.MTIME]);

                Ssutil.DbGetString(hStmt, 0, "userid", out meChan.userid, MeChan.USERID_SZ, out nullInd[MeChan.USERID]);
            }
            catch (Exception e)
            {
                Log2.e("\nDynMeChan.MeFetchChan(): ERROR: ODBC 'Get' failed: " + e.Message);
                GenUtil.SetErr("meFetchChan02: Input error on field: " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\nDynMeChan.MeFetchChan(): meChan:\n" + meChan.ToStringWN(nullInd));

            return Constant.SUCCESS;
        }


        /// <summary>
        /// This method updates all of the column values of a record in the MDB ES channel table.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>SQLHANDLE hConn, 
        /// <param name="meChan"> - a prescribed MeChan object that provides the updated field values.</param>
        /// <param name="nullInds"> - an array of ODBC nullInds associated with meChan.</param>
        /// <returns></returns>
        public static int MeUpdateChan(SQLHANDLE hConn, MeChan meChan, SQLLEN[] nullInds)
        {
            //...Log2.v("\nDynMeChan.MeUpdateChan(): Entry");

            string update_buf;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            // Prepare the update statement
            update_buf = String.Format(mUPDATE, mTableName, meChan.ToStringAsCSVequates(nullInds), meChan.location, meChan.call1, meChan.chid);

            //...Log2.v("\nDynMeChan.MeUpdateChan(): query = " + update_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, update_buf, update_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("\nDynMeChan.MeUpdateChan(): ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\n{1}", sqlRet, update_buf);
                    Log2.e(str);
                    str = String.Format("meUpdateChan04 -- Error writing Channel: {0} {1} {2} ",
                                  meChan.location, meChan.call1, meChan.chid);
                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return -4;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + update_buf;
                //...Log2.v("\nDynMeChan.MeUpdateChan(): Updating of main.me_chan records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\nDynMeChan.MeUpdateChan(): Exit");
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Inserts a MeChan record into the database using column values prescribed 
        /// by the fields of the MeChan object.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="meChan"> - a MeChan object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for meChan</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int MeInsertChan(SQLHANDLE hConn, MeChan meChan, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynMeChan.MeInsertChan(): Entry");

            string cSQL;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynMeChan.MeInsertChan(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            //Create the SQL insert statement.
            cSQL = String.Format(mINSERT, mTableName, meChan.ToStringAsCSV(nullInd));

            //...Log2.v("nDynMeChan.MeInsertChan(): SQLExecDirect():\r\n" + cSQL);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\n\nDynMeChan.MeInsertChan(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                    string str = "meInsertChan02 -- Error inserting chan";
                    Ssutil.DbGetDiagStmt(hStmt, str); ;
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }

                //...Log2.v("\n\nDynMeChan.MeInsertChan(): successfuly inserted record into table main.me_chan");
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynMeChan.MeInsertChan(): Insertion of main.me_chan records is DISABLED.");
            }

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynMeChan.MeInsertChan(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method deletes a record with a prescribed key from the MDB
        /// ES channel table: main.me_chan.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="location"> - a prescribed location string.</param>
        /// <param name="call1"> - a prescribed call sign string.</param>
        /// <param name="chid"> - a prescribed channel ID value.</param>
        /// <returns></returns>
        public static int MeDeleteChan(SQLHANDLE hConn, string location, string call1, string chid)
        {
            //...Log2.v("\n\nDynMeChan.MeDeleteChan(): Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string delete_buf = String.Format(mDELETE, mTableName, location, call1, chid);

            //...Log2.v("\nDynMeChan.MeDeleteChan(): query = " + delete_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, delete_buf, delete_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("meDeleteChan01 -- Error deleting from main.me_chan");
                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Log2.e("\nDynMeChan.MeDeleteChan(): ERROR: SQLExecDirect() failed for query = " + delete_buf);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + delete_buf;
                //...Log2.v("\nDynMeChan.MeDeleteChan(): Deletion of main.me_chan records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynMeChan.MeDeleteChan(): Exit");
            return 0;
        }

        /// <summary>
        /// This method updates records in the MDB <b>main.me_chan</b> table i.a.w. the 
        /// 'LK' directives in a user's ES PDF import file; specifically, all records that
        /// have 'location = X' are changed to 'location = Y' where X and Y are user-prescribed
        /// location strings.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="feCLoc"> - prescribed feCLoc object that prescribes X and Y.</param>
        /// <param name="userName"> - user's MICS ID.</param>
        /// <param name="sysDate"> - prescribed current system date.</param>
        /// <param name="sysTime"> - prescribed current system time.</param>
        /// <param name="cLocCount"> - a count of the number of records that were updated.</param>
        /// <returns></returns>
        public static int MeCLocUpdateChan(SQLHANDLE hConn, FeCLoc feCLoc, string userName, string sysDate, string sysTime, ref int cLocCount)
        {
            string cSQL;
            int status = Constant.FAILURE;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLLEN nRows = 0;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            /* Update the MDB chan records */
            cSQL = String.Format(mCLOC_UPDATE,
                                 mTableName, feCLoc.newlocation, userName, sysDate, sysTime, feCLoc.oldlocation);

            //...Log2.v("\nDynMeChan.MeCLocUpdateChan(): query = " + cSQL);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (ODBC.IsOK(sqlRet))
                {
                    IntPtr intPtr = Marshal.AllocHGlobal(sizeof(SQLLEN));
                    sqlRet = ODBC.SQLRowCount(hStmt, intPtr);
                    nRows = Marshal.ReadInt64(intPtr);

                    cLocCount += (int)nRows;
                    status = (int)nRows;
                }
                else
                {
                    Log2.e("\nDynMeChan.MeCLocUpdateChan(): ERROR: call to SQLExecDirec() failed for query =\n" + cSQL);
                    status = Constant.FAILURE;
                    Console.Write("\r\nERROR!  Could not Change Chan record Location fields.\r\n");
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynMeChan.MeCLocUpdateChan(): Update of main.me_chan records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            return status;
        }

        /// <summary>
        /// This method updates records in the MDB <b>main.me_chan</b> table i.a.w. the 
        /// 'GK' directives in a user's ES PDF import file; specifically, all records that
        /// have 'call1 = X' are changed to 'call1 = Y' where X and Y are user-prescribed
        /// call sign strings.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="feCCal"> - prescribed feCCal object that prescribes X and Y.</param>
        /// <param name="userName"> - user's MICS ID.</param>
        /// <param name="sysDate"> - prescribed current system date.</param>
        /// <param name="sysTime"> - prescribed current system time.</param>
        /// <param name="cLocCount"> - a count of the number of records that were updated.</param>
        /// <returns></returns>
        public static int MeCCalUpdateChan(SQLHANDLE hConn, FeCCal feCCal, string userName, string sysDate, string sysTime, ref int cLocCount)
        {
            string cSQL;
            int status = Constant.FAILURE;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLLEN nRows = 0;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            /* Update the MDB chan records */
            cSQL = String.Format(mCCAL_UPDATE,
                                 mTableName, feCCal.newcallsign, userName, sysDate, sysTime, feCCal.oldcallsign);

            //...Log2.v("\nDynMeChan.MeCCalUpdateChan(): query = " + cSQL);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (ODBC.IsOK(sqlRet))
                {
                    IntPtr intPtr = Marshal.AllocHGlobal(sizeof(SQLLEN));
                    sqlRet = ODBC.SQLRowCount(hStmt, intPtr);
                    nRows = Marshal.ReadInt64(intPtr);

                    cLocCount += (int)nRows;
                    status = (int)nRows;
                }
                else
                {
                    Log2.e("\nDynMeChan.MeCCalUpdateChan(): ERROR: call to SQLExecDirec() failed for query =\n" + cSQL);
                    status = Constant.FAILURE;
                    Console.Write("\r\nERROR!  Could not Change Chan record CallSign fields.\r\n");
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynMeChan.MeCCalUpdateChan(): Update of main.me_chan records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            return status;
        }


        /// <summary>
        /// This method returns true if a record exists in the MDB table <b>main.me_azim</b>
        /// that has a prescribed 'location', 'call sign' and 'channel ID".
        /// </summary>
        /// <param name="location"> - the prescribed 'location' string.</param>
        /// <param name="call1"> - the prescribed 'call sign' string.</param>
        /// <param name="chid"> - the prescribed 'channel ID' string.</param>
        /// <returns></returns>
        public static bool MdbRecordExists(string location, string call1, string chid)
        {
            string whereClause = String.Format("location = '{0}'  AND  call1 = '{1}' AND  chid = '{2}'",
                                                location, call1, chid);
            return (Ssutil.DbCountRows(mTableName, whereClause) == 1);
        }



    }
}

```
