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

    /// <summary>
    /// This class provides methods that serve MeAnte data to/from a database.
    /// </summary>
    public class DynMeAnte
    {
        private const string mBaseTableName = "me_ante";

        private const string mMdbTableName = "main." + mBaseTableName;

        private static string mTableName = mMdbTableName;

        private static bool mMdbWriteEnabled = true;

        private static bool mSpoofModeIsOff = true;

        private const string mSELECT = "SELECT * FROM {0} ";

        private const string mUPDATE = "UPDATE {0} SET {1} WHERE location='{2}' AND call1='{3}' ";

        private const string mINSERT = "INSERT INTO {0} VALUES ({1}) ";

        private const string mDELETE = "DELETE FROM {0} WHERE location='{1}' AND call1='{2}' ";

        private const string mCLOC_UPDATE = "UPDATE {0} SET location='{1}', userid='{2}', mdate='{3}', mtime='{4}' WHERE location='{5}' ";

        private const string mCCAL_UPDATE = "UPDATE {0} SET call1='{1}', userid='{2}', mdate='{3}', mtime='{4}' WHERE call1='{5}' ";

        //------------------------------------------------------------------------------------
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;

        //------------------------------------------------------------------------------------

        public static string TableName
        {
            get { return mTableName; }
        }

        /// <summary>
        /// This method can be used to set whether the MDB <b>main.me_ante</b> table should be written to,
        /// (SQL DELETE, INSERT and/or UPDATE queries) or not; it can also set whether the SQL queries should
        /// target the actual main.me_ante table or a user's 'spoof' table (e.g. hulme.audit_trail).
        /// </summary>
        /// <param name="mdbWriteEnabled"> - boolean that prescribes whether me_ante record
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
                    Log2.e("\nDynMeAnte.SetState(): ERROR: spoofMode is ON but userSchema is null or whitespace.");
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
                //...Log2.v("\r\nDynMdbAntenna.GetNextFreeCursor(): found available cursor: curHandle = " + curHandle);
            }

            return curHandle;
        }


        /// <summary>
        /// This method returns true if a record exists in the MDB table <b>main.me_ante</b>
        /// that has a prescribed 'location' and 'call sign'.
        /// </summary>
        /// <param name="location"> - the prescribed 'location' string.</param>
        /// <param name="call1"> the prescribed 'call sign' string.</param>
        /// <returns></returns>
        public static bool MdbRecordExists(string location, string call1)
        {
            string whereClause = String.Format("location = '{0}'  AND  call1 = '{1}'",
                                                location, call1);
            return (Ssutil.DbCountRows(mTableName, whereClause) == 1);
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
        public static int MeCloseAnte(int curHandle)
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
        public static int MeSelectAnte(string searchCriteria, /* selection criteria */
                              string orderBy)               /* how is selection to be ordered */
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
                stmt_buf += "WHERE ";
                stmt_buf += searchCriteria;
            }

            /* If "order by" is specified then add order by clause */
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                /* Order by was specified */
                stmt_buf += " ORDER BY ";
                stmt_buf += orderBy;
            }

            //	Get next free cursor area
            curHandle = GetNextFreeCursor();

            //	allocate and open the handle
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            //...Log2.v("\nDynMeAnte.MeSelectAnte(): query = " + stmt_buf);
            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nDynMeAnte.MeSelectAnte(): ERROR: SQLExecDirect failed, query = \n" + stmt_buf);
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
        /// Fetch an antenna record from the callers cursor.  
        /// </summary>
        /// <param name="curHandle"></param>
        /// <param name="meAnte"></param>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public static int MeFetchAnte(int curHandle,
                                        out MeAnte meAnte,
                                        out SQLLEN[] nullInds)        /* caller's array of null indicators */
        {
            // 'out' requirement.
            meAnte = null;
            nullInds = null;

            FetchCount.IncrementMeAnte();

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\nDynMeAnte.MeFetchAnte(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\nDynMeAnte.MeFetchAnte(): ERROR: SQLFetch() failed.");
                    return Error.ODBC_FETCH_FAILED;
                }
            }

            meAnte = new MeAnte();
            nullInds = NullHelper.CreateArrayOfNullInd(MeAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            //	Now read in the fields
            try
            {
                // Initialize auto-indexing for columns.
                Ssutil.DbStartGets();

                Ssutil.DbGetString(hStmt, 0, "location", out meAnte.location, MeAnte.LOCATION_SZ, out nullInds[MeAnte.LOCATION]);
                Ssutil.DbGetString(hStmt, 0, "call1", out meAnte.call1, MeAnte.CALL1_SZ, out nullInds[MeAnte.CALL1]);
                Ssutil.DbGetString(hStmt, 0, "txband", out meAnte.txband, MeAnte.TXBAND_SZ, out nullInds[MeAnte.TXBAND]);
                Ssutil.DbGetString(hStmt, 0, "rxband", out meAnte.rxband, MeAnte.RXBAND_SZ, out nullInds[MeAnte.RXBAND]);
                Ssutil.DbGetString(hStmt, 0, "acodetx", out meAnte.acodetx, MeAnte.ACODETX_SZ, out nullInds[MeAnte.ACODETX]);
                Ssutil.DbGetString(hStmt, 0, "acoderx", out meAnte.acoderx, MeAnte.ACODERX_SZ, out nullInds[MeAnte.ACODERX]);
                Ssutil.DbGetFloat(hStmt, 0, "g_t", out meAnte.g_t, out nullInds[MeAnte.G_T]);
                Ssutil.DbGetFloat(hStmt, 0, "lnat", out meAnte.lnat, out nullInds[MeAnte.LNAT]);
                Ssutil.DbGetFloat(hStmt, 0, "aht", out meAnte.aht, out nullInds[MeAnte.AHT]);
                Ssutil.DbGetFloat(hStmt, 0, "afslt", out meAnte.afslt, out nullInds[MeAnte.AFSLT]);
                Ssutil.DbGetFloat(hStmt, 0, "afslr", out meAnte.afslr, out nullInds[MeAnte.AFSLR]);
                Ssutil.DbGetFloat(hStmt, 0, "txhgmax", out meAnte.txhgmax, out nullInds[MeAnte.TXHGMAX]);
                Ssutil.DbGetFloat(hStmt, 0, "rxhgmax", out meAnte.rxhgmax, out nullInds[MeAnte.RXHGMAX]);
                Ssutil.DbGetInt(hStmt, 0, "satlongit", out meAnte.satlongit, out nullInds[MeAnte.SATLONGIT]);
                Ssutil.DbGetFloat(hStmt, 0, "satlong", out meAnte.satlong, out nullInds[MeAnte.SATLONG]);
                Ssutil.DbGetString(hStmt, 0, "satlongs", out meAnte.satlongs, MeAnte.SATLONGS_SZ, out nullInds[MeAnte.SATLONGS]);
                Ssutil.DbGetFloat(hStmt, 0, "az", out meAnte.az, out nullInds[MeAnte.AZ]);
                Ssutil.DbGetFloat(hStmt, 0, "el", out meAnte.el, out nullInds[MeAnte.EL]);
                Ssutil.DbGetFloat(hStmt, 0, "sarc1", out meAnte.sarc1, out nullInds[MeAnte.SARC1]);
                Ssutil.DbGetFloat(hStmt, 0, "sarc2", out meAnte.sarc2, out nullInds[MeAnte.SARC2]);
                Ssutil.DbGetFloat(hStmt, 0, "rxpre", out meAnte.rxpre, out nullInds[MeAnte.RXPRE]);
                Ssutil.DbGetFloat(hStmt, 0, "txpre", out meAnte.txpre, out nullInds[MeAnte.TXPRE]);
                Ssutil.DbGetFloat(hStmt, 0, "rxtro", out meAnte.rxtro, out nullInds[MeAnte.RXTRO]);
                Ssutil.DbGetFloat(hStmt, 0, "txtro", out meAnte.txtro, out nullInds[MeAnte.TXTRO]);
                Ssutil.DbGetString(hStmt, 0, "licence", out meAnte.licence, MeAnte.LICENCE_SZ, out nullInds[MeAnte.LICENCE]);
                Ssutil.DbGetString(hStmt, 0, "satname", out meAnte.satname, MeAnte.SATNAME_SZ, out nullInds[MeAnte.SATNAME]);
                Ssutil.DbGetString(hStmt, 0, "stata", out meAnte.stata, MeAnte.STATA_SZ, out nullInds[MeAnte.STATA]);
                Ssutil.DbGetString(hStmt, 0, "nota", out meAnte.nota, MeAnte.NOTA_SZ, out nullInds[MeAnte.NOTA]);
                Ssutil.DbGetString(hStmt, 0, "op2", out meAnte.op2, MeAnte.OP2_SZ, out nullInds[MeAnte.OP2]);
                Ssutil.DbGetInt(hStmt, 0, "antref", out meAnte.antref, out nullInds[MeAnte.ANTREF]);
                Ssutil.DbGetString(hStmt, 0, "orbit", out meAnte.orbit, MeAnte.ORBIT_SZ, out nullInds[MeAnte.ORBIT]);
                Ssutil.DbGetString(hStmt, 0, "mdate", out meAnte.mdate, MeAnte.MDATE_SZ, out nullInds[MeAnte.MDATE]);
                Ssutil.DbGetString(hStmt, 0, "mtime", out meAnte.mtime, MeAnte.MTIME_SZ, out nullInds[MeAnte.MTIME]);
                Ssutil.DbGetString(hStmt, 0, "userid", out meAnte.userid, MeAnte.USERID_SZ, out nullInds[MeAnte.USERID]);
            }
            catch (Exception e)
            {
                Log2.e("\nDynMeAnte.MeFetchAnte(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                GenUtil.SetErr("meFetchAnte02: Input error on field: " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\nDynMeAnte.MeFetchAnte(): meAnte:\n" + meAnte.ToStringWN(nullInds));

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method updates all of the column values of a record in the MDB ES channel table.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="meAnte"> - a prescribed MeAnte object that provides the updated field values.</param>
        /// <param name="nullInds"> - an array of ODBC nullInds associated with meAnte.</param>
        /// <returns></returns>
        public static int MeUpdateAnte(SQLHANDLE hConn, MeAnte meAnte, SQLLEN[] nullInds)
        {
            //...Log2.v("\nDynMeAnte.MeUpdateAnte(): Entry");

            string update_buf;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            // Prepare the update statement
            update_buf = String.Format(mUPDATE, mTableName, meAnte.ToStringAsCSVequates(nullInds), meAnte.location, meAnte.call1);

            //...Log2.v("\nDynMeAnte.MeUpdateAnte(): query = " + update_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, update_buf, update_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("\nDynMeAnte.MeUpdateAnte(): ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\n{1}", sqlRet, update_buf);
                    Log2.e(str);
                    str = String.Format("meUpdateAnte04 -- Error writing Antenna: {0} {1} ",
                                  meAnte.location, meAnte.call1);
                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return -4;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + update_buf;
                //...Log2.v("\nDynMeAnte.MeUpdateAnte(): Updating of main.me_ante records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\nDynMeAnte.MeUpdateAnte(): Exit");
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Inserts a MeAnte record into the database using column values prescribed 
        /// by the fields of the MeAnte object.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="meAnte"> - a MeAnte object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for meAnte</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int MeInsertAnte(SQLHANDLE hConn, MeAnte meAnte, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynMeAnte.MeInsertAnte(): Entry");

            string cSQL;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynMeAnte.MeInsertAnte(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            //Create the SQL insert statement.
            cSQL = String.Format(mINSERT, mTableName, meAnte.ToStringAsCSV(nullInd));

            //...Log2.v("nDynMeAnte.MeInsertAnte(): SQLExecDirect():\r\n" + cSQL);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\n\nDynMeAnte.MeInsertAnte(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                    string str = "meInsertAnte02 -- Error inserting chan";
                    Ssutil.DbGetDiagStmt(hStmt, str); ;
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }

                //...Log2.v("\n\nDynMeAnte.MeInsertAnte(): successfuly inserted record into table main.me_ante");
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynMeAnte.MeInsertAnte(): Insertion of main.me_ante records is DISABLED.");
            }

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynMeAnte.MeInsertAnte(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method deletes a record with a prescribed key from the MDB
        /// ES antenna table: main.me_chan.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="location"> - a prescribed location string.</param>
        /// <param name="call1"> - a prescribed call sign string.</param>
        /// <returns></returns>
        public static int MeDeleteAnte(SQLHANDLE hConn, string location, string call1)
        {
            //...Log2.v("\n\nDynMeAnte.MeDeleteAnte(): Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string delete_buf = String.Format(mDELETE, mTableName, location, call1);

            //...Log2.v("\nDynMeAnte.MeDeleteAnte(): query = " + delete_buf);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, delete_buf, delete_buf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    string str = String.Format("meDeleteAnte01 -- Error deleting from main.me_chan");
                    Ssutil.DbGetDiagStmt(hStmt, str);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Log2.e("\nDynMeAnte.MeDeleteAnte(): ERROR: SQLExecDirect() failed for query = " + delete_buf);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + delete_buf;
                //...Log2.v("\nDynMeAnte.MeDeleteAnte(): Deletion of main.me_chan records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\n\nDynMeAnte.MeDeleteAnte(): Exit");
            return 0;
        }

        /// <summary>
        /// This method updates records in the MDB <b>main.me_ante</b> table i.a.w. the 
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
        public static int MeCLocUpdateAnte(SQLHANDLE hConn, FeCLoc feCLoc, string userName, string sysDate, string sysTime, ref int cLocCount)
        {
            string cSQL;
            int status = Constant.FAILURE;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLLEN nRows = 0;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            /* Update the MDB ante records */
            cSQL = String.Format(mCLOC_UPDATE,
                                 mTableName, feCLoc.newlocation, userName, sysDate, sysTime, feCLoc.oldlocation);

            //...Log2.v("\nDynMeAnte.MeCLocUpdateAnte(): query = " + cSQL);

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
                    Log2.e("\nDynMeAnte.MeCLocUpdateAnte(): ERROR: call to SQLExecDirec() failed for query =\n" + cSQL);
                    status = Constant.FAILURE;
                    Console.Write("\r\nERROR!  Could not Change Ante record Location fields.\r\n");
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynMeAnte.MeCLocUpdateAnte(): Update of main.me_ante records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            return status;
        }

        /// <summary>
        /// This method updates records in the MDB <b>main.me_ante</b> table i.a.w. the 
        /// 'GK' directives in a user's ES PDF import file; specifically, all records that
        /// have 'call1 = X' are changed to 'call1 = Y' where X and Y are user-prescribed
        /// call sign strings.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>SQLHANDLE hConn, 
        /// <param name="feCCal"> - prescribed feCCal object that prescribes X and Y.</param>
        /// <param name="userName"> - user's MICS ID.</param>
        /// <param name="sysDate"> - prescribed current system date.</param>
        /// <param name="sysTime"> - prescribed current system time.</param>
        /// <param name="cLocCount"> - a count of the number of records that were updated.</param>
        /// <returns></returns>
        public static int MeCCalUpdateAnte(SQLHANDLE hConn, FeCCal feCCal, string userName, string sysDate, string sysTime, ref int cLocCount)
        {
            string cSQL;
            int status = Constant.FAILURE;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLLEN nRows = 0;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            /* Update the MDB ante records */
            cSQL = String.Format(mCCAL_UPDATE,
                                 mTableName, feCCal.newcallsign, userName, sysDate, sysTime, feCCal.oldcallsign);

            //...Log2.v("\nDynMeAnte.MeCCalUpdateAnte(): query = " + cSQL);

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
                    Log2.e("\nDynMeAnte.MeCCalUpdateAnte(): ERROR: call to SQLExecDirec() failed for query =\n" + cSQL);
                    status = Constant.FAILURE;
                    Console.Write("\r\nERROR!  Could not Change Ante record CallSign fields.\r\n");
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynMeAnte.MeCCalUpdateAnte(): Update of main.me_ante records is DISABLED.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            return status;
        }




    }
}
