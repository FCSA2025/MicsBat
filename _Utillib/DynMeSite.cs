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

    public class DynMeSite
    {
        private const string mBaseTableName = "me_site";

        private const string mMdbTableName = "main." + mBaseTableName;

        private static string mTableName = mMdbTableName;

        private static bool mMdbWriteEnabled = true;

        private static bool mSpoofModeIsOff = true;

        private const string mSELECT = "SELECT * FROM {0} ";

        private const string mUPDATE = "UPDATE {0} SET {1} WHERE location='{2}' ";

        private const string mINSERT = "INSERT INTO {0} VALUES ({1}) ";

        private const string mDELETE = "DELETE FROM {0} WHERE location='{1}' ";

        private const string mCLOC_UPDATE = "UPDATE {0} SET location='{1}', userid='{2}', mdate='{3}', mtime='{4}' WHERE location='{5}' ";

        //---------------------------------------------------------------------------

        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;

        //---------------------------------------------------------------------------

        public static string TableName
        {
            get { return mTableName; }
        }

        /// <summary>
        /// This method can be used to set whether the MDB <b>main.me_site</b> table should be written to,
        /// (SQL DELETE, INSERT and/or UPDATE queries) or not; it can also set whether the SQL queries should
        /// target the actual main.me_site table or a user's 'spoof' table (e.g. hulme.audit_trail).
        /// </summary>
        /// <param name="mdbWriteEnabled"> - boolean that prescribes whether me_site record
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
                    Log2.e("\nDynMeSite.SetState(): ERROR: spoofMode is ON but userSchema is null or whitespace.");
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
            return curHandle;
        }

        /// <summary>
        /// Closes (releases) a cursor associated with an MeSite record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the FtCursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int MeCloseSite(int curHandle)
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
        /// Select a site record from a site database.  
        /// </summary>
        /// <remarks>
        /// This method performs the setup for an open cursor operation. The caller 
        /// identifies the site table and the search criteria for the selection and how 
        /// the rows are to be ordered. If searchCriteria is NULL then all the rows are 
        /// retrieved. If orderBy is NULL then the cursor is set for UPDATE. The MS SQL Server 
        /// documentation states that 'order by' and 'update' are mutually exclusive. 
        /// Testing indicates that the caller can in fact select with the 'order by' 
        /// and then perform an update contrary to the documentation.  
        /// </remarks>
        /// <param name="searchCriteria"> - the search criteria excluding the 'where'</param>
        /// <param name="orderBy"> - the order by info of the select statement excluding the actual 'order by'</param>
        /// <returns></returns>
        public static int MeSelectSite(string searchCriteria, /* selection criteria */
                                 string orderBy)                    /* how is selection to be ordered */
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
                stmt_buf += "where ";
                stmt_buf += searchCriteria;
            }

            /* If "order by" is specified then add order by clause */
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                /* Order by was specified */
                stmt_buf += " order by ";
                stmt_buf += orderBy;
            }

            // Get next free cursor area
            curHandle = GetNextFreeCursor();

            // Allocate and open the handle
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            //...Log2.v("\nDynMeSite.MeSelectSite: query = " + stmt_buf);
            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nDynMeSite.MeSelectSite(): ERROR: SQLExecDirect() failed.");
                return Error.ODBC_EXECUTE_FAILED;
            }

            // Inititialize the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].hConn = hConn;

            return (curHandle);
        }

        /// <summary>
        /// Gets a row from a TS Site table.  
        /// </summary>
        /// <remarks>
        /// This method fetches a row from a PDF TS Site table. The routine 
        /// 'mtSelectSite' must have been called prior to calling this routine.  
        /// <para>
        /// Caller must supply an array of null indicators large enough (one null 
        /// indicator per nullable field in a row).  
        /// </para>
        /// </remarks>
        /// <param name="curHandle"> - cursor handle returned by 'mtSelectSite'</param>
        /// <param name="meSite"></param>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public static int MeFetchSite(int curHandle,
                                out MeSite meSite,
                                out SQLLEN[] nullInds)
        {
            // 'out' requirement.
            meSite = null;
            nullInds = null;

            FetchCount.IncrementMeSite();

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\nDynMeSite.MeFetchSite: sqlRet == ODBC.SQL_NO_DATA");
                    return sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\nDynMeSite.MeFetchSite(): ERROR: SQLFetch() failed.");
                    return Error.ODBC_FETCH_FAILED;
                }
            }

            //	Now read in the fields
            meSite = new MeSite();
            nullInds = NullHelper.CreateArrayOfNullInd(MeSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            try
            {
                // Initialize colum auto-indexing.
                Ssutil.DbStartGets();

                Ssutil.DbGetString(hStmt, 0, "location", out meSite.location, MeSite.LOCATION_SZ, out nullInds[MeSite.LOCATION]);
                Ssutil.DbGetString(hStmt, 0, "name", out meSite.name, MeSite.NAME_SZ, out nullInds[MeSite.NAME]);
                Ssutil.DbGetString(hStmt, 0, "prov", out meSite.prov, MeSite.PROV_SZ, out nullInds[MeSite.PROV]);
                Ssutil.DbGetString(hStmt, 0, "oper", out meSite.oper, MeSite.OPER_SZ, out nullInds[MeSite.OPER]);
                Ssutil.DbGetInt(hStmt, 0, "latit", out meSite.latit, out nullInds[MeSite.LATIT]);
                Ssutil.DbGetString(hStmt, 0, "strlatit", out meSite.strlatit, MeSite.STRLATIT_SZ, out nullInds[MeSite.STRLATIT]);
                Ssutil.DbGetString(hStmt, 0, "strlatits", out meSite.strlatits, MeSite.STRLATITS_SZ, out nullInds[MeSite.STRLATITS]);
                Ssutil.DbGetInt(hStmt, 0, "longit", out meSite.longit, out nullInds[MeSite.LONGIT]);
                Ssutil.DbGetString(hStmt, 0, "strlongit", out meSite.strlongit, MeSite.STRLONGIT_SZ, out nullInds[MeSite.STRLONGIT]);
                Ssutil.DbGetString(hStmt, 0, "strlongits", out meSite.strlongits, MeSite.STRLONGITS_SZ, out nullInds[MeSite.STRLONGITS]);
                Ssutil.DbGetFloat(hStmt, 0, "grnd", out meSite.grnd, out nullInds[MeSite.GRND]);
                Ssutil.DbGetString(hStmt, 0, "radio", out meSite.radio, MeSite.RADIO_SZ, out nullInds[MeSite.RADIO]);
                Ssutil.DbGetShort(hStmt, 0, "rain", out meSite.rain, out nullInds[MeSite.RAIN]);
                Ssutil.DbGetString(hStmt, 0, "sdate", out meSite.sdate, MeSite.SDATE_SZ, out nullInds[MeSite.SDATE]);
                Ssutil.DbGetString(hStmt, 0, "stats", out meSite.stats, MeSite.STATS_SZ, out nullInds[MeSite.STATS]);
                Ssutil.DbGetString(hStmt, 0, "nots", out meSite.nots, MeSite.NOTS_SZ, out nullInds[MeSite.NOTS]);
                Ssutil.DbGetString(hStmt, 0, "oprtyp", out meSite.oprtyp, MeSite.OPRTYP_SZ, out nullInds[MeSite.OPRTYP]);
                Ssutil.DbGetString(hStmt, 0, "reg", out meSite.reg, MeSite.REG_SZ, out nullInds[MeSite.REG]);
                Ssutil.DbGetString(hStmt, 0, "mdate", out meSite.mdate, MeSite.MDATE_SZ, out nullInds[MeSite.MDATE]);
                Ssutil.DbGetString(hStmt, 0, "mtime", out meSite.mtime, MeSite.MTIME_SZ, out nullInds[MeSite.MTIME]);
                Ssutil.DbGetString(hStmt, 0, "userid", out meSite.userid, MeSite.USERID_SZ, out nullInds[MeSite.USERID]);
            }
            catch (Exception e)
            {
                Log2.e("\nDynMeSite.MeFetchSite(): ERROR: ODBC 'Get' failed: " + e.Message);
                GenUtil.SetErr("meFetchSite02: Input error on field: " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\nDynMeSite.MeFetchSite: meSite =\n" + meSite.ToStringWN(nullInds));

            return Constant.SUCCESS;
        }


        /// <summary>
        /// This method updates all of the column values of a record in the MDB ES site table.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="meSite"> - a prescribed MeSite object that provides the updated field values.</param>
        /// <param name="nullInds"> - an array of ODBC nullInds associated with meSite.</param>
        /// <returns></returns>
        public static int MeUpdateSite(SQLHANDLE hConn, MeSite meSite, SQLLEN[] nullInds)
        {
            //...Log2.v("\nDynMeSite.MeUpdateSite(): Entry");

            string update_buf;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            // Prepare the update statement
            update_buf = String.Format(mUPDATE, mTableName, meSite.ToStringAsCSVequates(nullInds), meSite.location);

            //...Log2.v("\nDynMeSite.MeUpdateSite(): query = " + update_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, update_buf, update_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("\nDynMeSite.MeUpdateSite(): ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\n{1}", sqlRet, update_buf);
                    Log2.e(str);
                    str = String.Format("meUpdateSite04 -- Error writing Site: {0} ", meSite.location);
                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return -4;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + update_buf;
                //...Log2.v("\nDynMeSite.MeUpdateSite(): Updating of main.me_site records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\nDynMeSite.MeUpdateSite(): Exit");
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Inserts a MeSite record into the database using column values prescribed 
        /// by the fields of the MeSite object.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="meSite"> - a MeSite object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for meSite</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int MeInsertSite(SQLHANDLE hConn, MeSite meSite, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynMeSite.MeInsertSite(): Entry");

            string cSQL;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynMeSite.MeInsertSite(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            //Create the SQL insert statement.
            cSQL = String.Format(mINSERT, mTableName, meSite.ToStringAsCSV(nullInd));

            //...Log2.v("nDynMeSite.MeInsertSite(): SQLExecDirect():\r\n" + cSQL);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\n\nDynMeSite.MeInsertSite(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                    string str = "meInsertSite02 -- Error inserting site";
                    Ssutil.DbGetDiagStmt(hStmt, str); ;
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }

                //...Log2.v("\n\nDynMeSite.MeInsertSite(): successfuly inserted record into table main.me_site");
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynMeSite.MeInsertSite(): Insertion of main.me_site records is DISABLED.");
            }

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynMeSite.MeInsertSite(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method deletes a record with a prescribed location from the MDB
        /// ES site table: main.me_site.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="location"> - a prescribed location string.</param>
        /// <returns></returns>
        public static int MeDeleteSite(SQLHANDLE hConn, string location)
        {
            //...Log2.v("\n\nDynMeSite.MeDeleteSite(): Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string delete_buf = String.Format(mDELETE, mTableName, location);

            //...Log2.v("\nDynMeSite.MeDeleteSite(): query = " + delete_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, delete_buf, delete_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("meDeleteSite01 -- Error deleting from main.me_site");
                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Log2.e("\nDynMeSite.MeDeleteSite(): ERROR: SQLExecDirect() failed for query = " + delete_buf);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + delete_buf;
                //...Log2.v("\nDynMeSite.MeDeleteSite(): Deletion of main.me_site records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynMeSite.MeDeleteSite(): Exit");
            return 0;
        }

        /// <summary>
        /// This method updates records in the MDB <b>main.me_site</b> table i.a.w. the 
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
        public static int MeCLocUpdateSite(SQLHANDLE hConn, FeCLoc feCLoc, string userName, string sysDate, string sysTime, ref int cLocCount)
        {
            string cSQL;
            int status = Constant.FAILURE;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLLEN nRows = 0;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            /* Update the MDB site records */
            cSQL = String.Format(mCLOC_UPDATE,
                                 mTableName, feCLoc.newlocation, userName, sysDate, sysTime, feCLoc.oldlocation);

            //...Log2.v("\nDynMeSite.MeCLocUpdateSite(): query = " + cSQL);

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
                    Log2.e("\nDynMeSite.MeCLocUpdateSite(): ERROR: call to SQLExecDirec() failed for query =\n" + cSQL);
                    status = Constant.FAILURE;
                    Console.Write("\r\nERROR!  Could not Change Site record Location fields.\r\n");
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynMeSite.MeCLocUpdateSite(): Update of main.me_site records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            return status;
        }

        /// <summary>
        /// This method returns true if a record exists in the MDB table <b>main.me_site</b>
        /// that has a prescribed 'location'.
        /// </summary>
        /// <param name="location"> - the prescribed 'location' string.</param>
        /// <returns></returns>
        public static bool MdbRecordExists(string location)
        {
            string whereClause = String.Format("location = '{0}'", location);
            return (Ssutil.DbCountRows(mTableName, whereClause) == 1);
        }











    }
}
