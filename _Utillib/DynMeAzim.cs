using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
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

    public class DynMeAzim
    {
        private const string mBaseTableName = "me_azim";

        private const string mMdbTableName = "main." + mBaseTableName;

        private static string mTableName = mMdbTableName;

        private static bool mMdbWriteEnabled = true;

        private static bool mSpoofModeIsOff = true;

        private const string mSELECT = "SELECT * FROM {0} ";

        private const string mUPDATE = "UPDATE {0} SET {1} WHERE location='{2}' AND call1='{3}' AND azim='{4}' ";

        private const string mINSERT = "INSERT INTO {0} VALUES ({1}) ";

        private const string mDELETE = "DELETE FROM {0} WHERE location='{1}' AND call1='{2}' AND azim='{3}' ";

        private const string mCLOC_UPDATE = "UPDATE {0} SET location='{1}', userid='{2}', mdate='{3}', mtime='{4}' WHERE location='{5}' ";

        private const string mCCAL_UPDATE = "UPDATE {0} SET call1='{1}', userid='{2}', mdate='{3}', mtime='{4}' WHERE call1='{5}' ";

        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;

        //-----------------------------------------------------------------------------------

        public static string TableName
        {
            get { return mTableName; }
        }

        /// <summary>
        /// This method can be used to set whether the MDB <b>main.me_azim</b> table should be written to,
        /// (SQL DELETE, INSERT and/or UPDATE queries) or not; it can also set whether the SQL queries should
        /// target the actual main.me_azim table or a user's 'spoof' table (e.g. hulme.audit_trail).
        /// </summary>
        /// <param name="mdbWriteEnabled"> - boolean that prescribes whether me_azim record
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
                    Log2.e("\nDynMeAzim.SetState(): ERROR: spoofMode is ON but userSchema is null or whitespace.");
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
                    Application.Exit("\r\nDynMeAzim.GetNextFreeCursor(): ERROR: No available cursors.");
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
                //...Log2.v("\r\nDynMeAzim.GetNextFreeCursor(): found available cursor: curHandle = " + curHandle);
            }

            return curHandle;
        }

        /// <summary>
        /// Closes (releases) a cursor associated with a PDF TS antenna record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int MeCloseAzim(int curHandle)
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
        /// Select a Azim record from a Azim table.  
        /// </summary>
        /// <remarks>
        /// This method performs the setup for an open cursor operation. The caller 
        /// identifies the Azim table and the search criteria for the selection and how 
        /// the rows are to be ordered. If searchCriteria is NULL then all the rows are 
        /// retrieved. If orderBy is NULL then the cursor is set for UPDATE. The MS SQL Server 
        /// documentation states that 'order by' and 'update' are mutually exclusive. 
        /// Testing indicates that the caller can in fact select with the 'order by' 
        /// and then perform an update contrary to the documentation.  
        /// </remarks>
        /// <param name="searchCriteria"> - the search criteria excluding the 'where'</param>
        /// <param name="orderBy"> - the order by info of the select statement excluding the actual 'order by'</param>
        /// <returns></returns>
        public static int MeSelectAzim(string searchCriteria, /* selection criteria */
                                         string orderBy)        /* how is selection to be ordered */
        {
            int curHandle;          /* cursor handle */

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;

            /* Construct the select clause */
            string stmt_buf = String.Format(mSELECT, mTableName);

            /* Construct the 'where' part of the select clause */
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                /* Search criteria was specified */
                stmt_buf += "where ";
                stmt_buf += searchCriteria;
            }

            /* If "order by" is not specified then assume its for update */
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                /* Order by was specified */
                stmt_buf += " order by ";
                stmt_buf += orderBy;
            }

            //	Get next free cursor area
            curHandle = GetNextFreeCursor();

            //	allocate and open the handle
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            //...Log2.v("\nDynMeAzim.MeSelectAzim(): query = " + stmt_buf);
            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nDynMeAzim.MeSelectAzim(): ERROR: SQLExecDirect failed.");
                Ssutil.DbGetDiagStmt(hStmt, "feSelectAzim Error:\n" + stmt_buf + "\n");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.ODBC_EXECUTE_FAILED;
            }

            //	Inititialize the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].hConn = hConn;

            return (curHandle);
        }

        /// <summary>
        /// Gets a row from a ES Azim table.  
        /// </summary>
        /// <remarks>
        /// This method fetches a row from a PDF ES Azim table. The routine 
        /// 'feSelectAzim' must have been called prior to calling this routine.  
        /// <para>
        /// Caller must supply an array of null indicators large enough (one null 
        /// indicator per nullable field in a row).  
        /// </para>
        /// </remarks>
        /// <param name="nCursor"> - handle returned by 'feSelectAzim'</param>
        /// <param name="meAzim"></param>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public static int MeFetchAzim(int nCursor,        /* handle returned by 'feSelectAzim' */
                                out MeAzim meAzim,        /* caller's struct for row of data */
                                out SQLLEN[] nullInds)        /* caller's array of null indicators */
        {
            // 'out' requitements.
            meAzim = new MeAzim();
            nullInds = NullHelper.CreateArrayOfNullInd(MeAzim.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            FetchCount.IncrementMeAzim();

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[nCursor].hStmt;

            sqlRet = ODBC.SQLFetch(hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\nDynMeAzim.MeFetchAzim(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\nDynMeAzim.MeFetchAzim(): ERROR: SQLFetch failed.");
                    return Error.ODBC_FETCH_FAILED;
                }
            }

            //	Now read in the fields
            try
            {
                // Initialize auto-indexing.
                Ssutil.DbStartGets();

                Ssutil.DbGetString(hStmt, 0, "location", out meAzim.location, MeAzim.LOCATION_SZ, out nullInds[MeAzim.LOCATION]);
                Ssutil.DbGetString(hStmt, 0, "call1", out meAzim.call1, MeAzim.CALL1_SZ, out nullInds[MeAzim.CALL1]);
                Ssutil.DbGetFloat(hStmt, 0, "azim", out meAzim.azim, out nullInds[MeAzim.AZIM]);
                Ssutil.DbGetFloat(hStmt, 0, "elev", out meAzim.elev, out nullInds[MeAzim.ELEV]);
                Ssutil.DbGetFloat(hStmt, 0, "dist", out meAzim.dist, out nullInds[MeAzim.DIST]);
                Ssutil.DbGetFloat(hStmt, 0, "loss", out meAzim.loss, out nullInds[MeAzim.LOSS]);
                Ssutil.DbGetString(hStmt, 0, "mdate", out meAzim.mdate, MeAzim.MDATE_SZ, out nullInds[MeAzim.MDATE]);
                Ssutil.DbGetString(hStmt, 0, "mtime", out meAzim.mtime, MeAzim.MTIME_SZ, out nullInds[MeAzim.MTIME]);
                Ssutil.DbGetString(hStmt, 0, "userid", out meAzim.userid, MeAzim.USERID_SZ, out nullInds[MeAzim.USERID]);
            }
            catch (Exception e)
            {
                Log2.e("\nDynMeAzim.MeFetchAzim(): ERROR: ODBC 'Get' attempt failed.");
                GenUtil.SetErr("meFetchAzim02: Input error on field: " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\nDynMeAzim.MeFetchAzim(): meAzim:\n" + meAzim.ToStringWN(nullInds));

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method updates all of the column values of a record in the MDB ES channel table.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="meAzim"> - a prescribed MeAzim object that provides the updated field values.</param>
        /// <param name="nullInds"> - an array of ODBC nullInds associated with meAzim.</param>
        /// <returns></returns>
        public static int MeUpdateAzim(SQLHANDLE hConn, MeAzim meAzim, SQLLEN[] nullInds)
        {
            //...Log2.v("\nDynMeAzim.MeUpdateAzim(): Entry");

            string update_buf;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            // Prepare the update statement
            update_buf = String.Format(mUPDATE, mTableName, meAzim.ToStringAsCSVequates(nullInds),
                                        meAzim.location, meAzim.call1, meAzim.azim);

            //...Log2.v("\nDynMeAzim.MeUpdateAzim(): query = " + update_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, update_buf, update_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("\r\nDynMeAzim.MeUpdateAzim(): ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\r\n{1}", sqlRet, update_buf);
                    Log2.e(str);
                    str = String.Format("meUpdateAzim04 -- Error writing Azim: {0} {1} {2} ",
                                  meAzim.location, meAzim.call1, meAzim.azim);
                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return -4;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + update_buf;
                //...Log2.v("\nDynMeAzim.MeUpdateAzim(): Updating of main.me_azim records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\nDynMeAzim.MeUpdateAzim(): Exit");
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Inserts a MeAzim record into the database using column values prescribed 
        /// by the fields of the MeAzim object.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="meAzim"> - a MeAzim object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for meAzim</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int MeInsertAzim(SQLHANDLE hConn, MeAzim meAzim, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynMeAzim.MeInsertAzim(): Entry");

            string cSQL;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynMeAzim.MeInsertAzim(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            //Create the SQL insert statement.
            cSQL = String.Format(mINSERT, mTableName, meAzim.ToStringAsCSV(nullInd));

            //...Log2.v("nDynMeAzim.MeInsertAzim(): SQLExecDirect():\r\n" + cSQL);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\n\nDynMeAzim.MeInsertAzim(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                    string str = "meInsertAzim02 -- Error inserting azim";
                    Ssutil.DbGetDiagStmt(hStmt, str); ;
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }

                //...Log2.v("\n\nDynMeAzim.MeInsertAzim(): successfuly inserted record into table main.me_azim");
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynMeAzim.MeInsertAzim(): Insertion of main.me_azim records is DISABLED.");
            }

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynMeAzim.MeInsertAzim(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method deletes a record with a prescribed key from the MDB
        /// ES channel table: main.me_azim.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="location"> - a prescribed location string.</param>
        /// <param name="call1"> - a prescribed call sign string.</param>
        /// <param name="chid"> - a prescribed channel ID.</param>
        /// <returns></returns>
        public static int MeDeleteAzim(SQLHANDLE hConn, string location, string call1, float chid)
        {
            //...Log2.v("\n\nDynMeAzim.MeDeleteAzim(): Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string delete_buf = String.Format(mDELETE, mTableName, location, call1, chid);

            //...Log2.v("\nDynMeAzim.MeDeleteAzim(): query = " + delete_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, delete_buf, delete_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("meDeleteAzim01 -- Error deleting from main.me_azim");
                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Log2.e("\nDynMeAzim.MeDeleteAzim(): ERROR: SQLExecDirect() failed for query = " + delete_buf);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + delete_buf;
                //...Log2.v("\nDynMeAzim.MeDeleteAzim(): Deletion of main.me_azim records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynMeAzim.MeDeleteAzim(): Exit");
            return 0;
        }

        /// <summary>
        /// This method updates records in the MDB <b>main.me_azim</b> table i.a.w. the 
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
        public static int MeCLocUpdateAzim(SQLHANDLE hConn, FeCLoc feCLoc, string userName, string sysDate, string sysTime, ref int cLocCount)
        {
            string cSQL;
            int status = Constant.FAILURE;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLLEN nRows = 0;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            /* Update the MDB azim records */
            cSQL = String.Format(mCLOC_UPDATE,
                                 mTableName, feCLoc.newlocation, userName, sysDate, sysTime, feCLoc.oldlocation);

            //...Log2.v("\nDynMeAzim.MeCLocUpdateAzim(): query = " + cSQL);

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
                    Log2.e("\nDynMeAzim.MeCLocUpdateAzim(): ERROR: call to SQLExecDirec() failed for query =\n" + cSQL);
                    status = Constant.FAILURE;
                    Console.Write("\r\nERROR!  Could not Azimge Azim record Location fields.\r\n");
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynMeAzim.MeCLocUpdateAzim(): Update of main.me_azim records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            return status;
        }

        /// <summary>
        /// This method updates records in the MDB <b>main.me_azim</b> table i.a.w. the 
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
        public static int MeCCalUpdateAzim(SQLHANDLE hConn, FeCCal feCCal, string userName, string sysDate, string sysTime, ref int cLocCount)
        {
            string cSQL;
            int status = Constant.FAILURE;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLLEN nRows = 0;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            /* Update the MDB azim records */
            cSQL = String.Format(mCCAL_UPDATE,
                                 mTableName, feCCal.newcallsign, userName, sysDate, sysTime, feCCal.oldcallsign);

            //...Log2.v("\nDynMeAzim.MeCCalUpdateAzim(): query = " + cSQL);

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
                    Log2.e("\nDynMeAzim.MeCCalUpdateAzim(): ERROR: call to SQLExecDirec() failed for query =\n" + cSQL);
                    status = Constant.FAILURE;
                    Console.Write("\r\nERROR!  Could not Change Azim record CallSign fields.\r\n");
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynMeAzim.MeCCalUpdateAzim(): Update of main.me_azim records is DISABLED.");
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
        /// <param name="azim"> - the prescribed 'azimuth' value.</param>
        /// <returns></returns>
        public static bool MdbRecordExists(string location, string call1, float azim)
        {
            string whereClause = String.Format("location='{0}' AND call1='{1}' AND azim ='{2}'",
                                                location, call1, azim);
            return (Ssutil.DbCountRows(mTableName, whereClause) == 1);
        }



    }
}
