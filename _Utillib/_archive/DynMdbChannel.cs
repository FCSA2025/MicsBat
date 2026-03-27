using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
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
    /// Provides methods to select and fetch TS CHANNEL records from the database table
    /// <b>main.mt_chan</b>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item> The methods in this class are used to perform operations on the
    /// CHANNEL table. The actual CHANNEL table name is a variable.</item>
    /// <item>The supported operations are: selectChannel, fetchChannel,
    /// and closeChannel.</item>
    /// <item>The caller must first call selectChannel to select the required rows:
    /// this method simply sets up the cursor for subsequent use.</item>
    /// <item>Once selectChannel has been called, the user must call fetchChannel
    /// to  retreive a row.</item>
    /// <item>The method closeChannel must be called to release ODBC resources 
    /// (connection and statement handles etc) and to release the cursor back the 'pool'.</item>
    /// </list>
    /// </remarks>	
    public class DynMdbChannel
    {
#if PINVOKE
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int mtSelectChannel(string searchCriteria, string orderBy);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int mtFetchChannel(int curHandle, [In, Out] MtChan pChan, [In, Out] SQLLEN[] pChanNull);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int mtCloseChannel(int curHandle);
        //----------------------------------------------------------------------
        public static int MtSelectChannel_NATIVE(string searchCriteria, string orderBy)
        {
            //!@Log2.v("\r\nDynMdbChannel.MtSelectChannel_NATIVE(): searchCriteria = " + searchCriteria);
            //!@Log2.v("\r\nDynMdbChannel.MtSelectChannel_NATIVE(): orderBy = " + orderBy);
            return mtSelectChannel(searchCriteria, orderBy);
        }
        public static int MtCloseChannel_NATIVE(int curHandle)
        {
            return mtCloseChannel(curHandle);
        }
        public static int MtFetchChannel_NATIVE(int curHandle, out MtChan mtChan, out SQLLEN[] mtChanNull)
        {
            //!@Log2.v("\n\nDynMdbChannel.MtFetchChannel_NATIVE(): Entry: curHandle = " + curHandle);

            mtChan = new MtChan();
            mtChanNull = NullHelper.CreateArrayOfNullInd(MtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            int rc = mtFetchChannel(curHandle, mtChan, mtChanNull);

            switch (rc)
            {
                case ODBC.SQL_ERROR:
                    Log2.e("\r\nDynMdbChannel.MtFetchChannel_NATIVE(): ERROR: mtFetchChannel() returned SQL_ERROR");
                    break;
                case ODBC.SQL_INVALID_HANDLE:
                    Log2.e("\r\nDynMdbChannel.MtFetchChannel_NATIVE(): ERROR: mtFetchChannel() returned SQL_INVALID_HANDLE");
                    break;
            }

            //!@Log2.v("\n\nDynMdbChannel.MtFetchChannel_NATIVE(): Exit: rc = " + rc);
            return rc;
        }

#endif
        //===================================================================================================================================
        // Using FtCursor here because it already exists and is a superset of what we need.
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        private static int nNextFreeCursor = 0;
        //===================================================================================================================================

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
        public static int GetNextFreeCursor()
        {
            ////!@Log2.v("\r\nDynMdbAntenna.GetNextFreeCursor(): Status:");
            //for (int i = 0; i < cursors.Length; i++)
            //{
            //    //!@Log2.v("\r\n     " + i + "     " + cursors[i].cursorOpen);
            //}

            //Get next free cursor area.
            int curHandle = -1;

            ////!@Log2.v("\r\nDynMdbChannel.GetNextFreeCursor(): nNextFreeCursor = " + nNextFreeCursor);
            if (nNextFreeCursor >= Constant.NUM_CURSORS_FEW)
            {
                //	Search for a free cursors in the list
                for (int nInd = 0; nInd < Constant.NUM_CURSORS_FEW; nInd++)
                {
                    ////!@Log2.v("\r\nDynMdbChannel.GetNextFreeCursor(): nInd = " + nInd);
                    if (!cursors[nInd].cursorOpen)
                    {
                        curHandle = nInd;
                        break;
                    }
                }

                if (curHandle == -1)
                {
                    // There are no more open cursors
                    GenUtil.SetErr("DynMdbChannel01 -- No more open cursors.");
                    Application.Exit("\r\nDynMdbChannel.GetNextFreeCursor(): ERROR: No available cursors.");
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

            //!@Log2.v("\r\nDynMdbChannel.GetNextFreeCursor(): found available cursor: curHandle = " + curHandle);
            cursors[curHandle].cursorOpen = true;

            return curHandle;
        }

        /// <summary>
        /// Selects channel records from the database table <b>main.mt_chan</b> for
        /// the prescribed SQL search criteria and ordering clauses; the 
        /// method returns an index to a cursor object that can be used by subsequent
        /// calls to MtFetchChannel().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for MtFetchChannel() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECUTE_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int MtSelectChannel(string searchCriteria, string orderBy)
        {
            Log2.n("\n\nDynMdbChannel.MtSelectChannel(): Entry");

            StringBuilder sb = new StringBuilder();

            // Construct the select clause.
            sb.Append("select ");
            sb.Append(MtChan.AllColumnsForSqlSelect);
            sb.Append(" from main.mt_chan");

            // Construct the 'where' part of the select clause.
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                // Search criteria was specified.
                sb.Append(" where ");
                sb.Append(searchCriteria);
            }

            // If "order by" is specified then add order by clause.
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                // Order by was specified.
                sb.Append(" order by ");
                sb.Append(orderBy);
            }

            string stmt_buf = sb.ToString();

            Log2.n("\r\nDynMdbChannel.MtSelectChannel(): stmt_buf = \r\n" + stmt_buf);

            int curHandle = GetNextFreeCursor();

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            // Allocate and open the handle.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length);

            // Populate the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].hConn = hConn;

            Log2.n("\n\nDynMdbChannel.MtSelectChannel(): Exit");
            return (curHandle);
        }

        /// <summary>
        /// Retrieves a single row of antenna data from the database table <b>main.mt_chan</b> using 
        /// the cursor object created by a previous call to MtSelectChannel().
        /// </summary>
        /// <param name="nCursor"> - FtCursor object.</param>
        /// <param name="mtChan"> - a MtChan object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for mtChan.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int MtFetchChannel(int nCursor, out MtChan mtChan, out SQLLEN[] nullInd)
        {
            Log2.n("\n\nDynMdbChannel.MtFetchChannel(): Entry");

            FetchCount.IncrementMtChan();

            int nRet = -666;
            SQLHSTMT hStmt = cursors[nCursor].hStmt;
            mtChan = new MtChan();
            nullInd = NullHelper.CreateArrayOfNullInd(MtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            // Fetch the next row of selected Channele data.
            SQLRETURN sqlRet = ODBC.SQLFetch(hStmt);
            // Check the SQLRETURN an act accordingly.
            if (!ODBC.IsOK(sqlRet))
            {
                // If no more Channel records available then break out the while-loop.
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    Log2.n("\n\nDynMdbChannel.MtFetchChannel(): Exit: sqlRet == ODBC.SQL_NO_DATA");
                    return ODBC.SQL_NO_DATA;
                }
                else
                {
                    // We have an error.
                    Log2.e("\n\nDynMdbChannel.MtFetchChannel(): Exit: ERROR: SQLFetch(): sqlRet = " + sqlRet);
                    Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, hStmt);
                    return -11;
                }
            }
            else
            {
                // We had a successful SQLFetch() operation; now get the record's column data.
                int colNum = -666;
                try
                {
                    
                    // MtChan member #0  string  call1
                    colNum = 1;
                    Ssutil.DbGetString(hStmt, colNum, "call1", out mtChan.call1, Constant.CALL_SZ, out nullInd[colNum - 1]);

                    // MtChan member #1  string  call2
                    colNum = 2;
                    Ssutil.DbGetString(hStmt, colNum, "call2", out mtChan.call2, Constant.CALL_SZ, out nullInd[colNum - 1]);

                    // MtChan member #2  string  bndcde
                    colNum = 3;
                    Ssutil.DbGetString(hStmt, colNum, "bndcde", out mtChan.bndcde, Constant.BNDCDE_SZ, out nullInd[colNum - 1]);

                    // MtChan member #3  string  splan
                    colNum = 4;
                    Ssutil.DbGetString(hStmt, colNum, "splan", out mtChan.splan, Constant.PLAN_SZ, out nullInd[colNum - 1]);

                    // MtChan member #4  short  hl
                    colNum = 5;
                    Ssutil.DbGetShort(hStmt, colNum, "hl", out mtChan.hl, out nullInd[colNum - 1]);

                    // MtChan member #5  short  vh
                    colNum = 6;
                    Ssutil.DbGetShort(hStmt, colNum, "vh", out mtChan.vh, out nullInd[colNum - 1]);

                    // MtChan member #6  string  chid
                    colNum = 7;
                    Ssutil.DbGetString(hStmt, colNum, "chid", out mtChan.chid, Constant.CHID_SZ, out nullInd[colNum - 1]);

                    // MtChan member #7  double  freqtx
                    colNum = 8;
                    Ssutil.DbGetDouble(hStmt, colNum, "freqtx", out mtChan.freqtx, out nullInd[colNum - 1]);

                    // MtChan member #8  string  poltx
                    colNum = 9;
                    Ssutil.DbGetString(hStmt, colNum, "poltx", out mtChan.poltx, Constant.POLTX_SZ, out nullInd[colNum - 1]);

                    // MtChan member #9  short  antnumbtx1
                    colNum = 10;
                    Ssutil.DbGetShort(hStmt, colNum, "antnumbtx1", out mtChan.antnumbtx1, out nullInd[colNum - 1]);

                    // MtChan member #10  short  antnumbtx2
                    colNum = 11;
                    Ssutil.DbGetShort(hStmt, colNum, "antnumbtx2", out mtChan.antnumbtx2, out nullInd[colNum - 1]);

                    // MtChan member #11  string  eqpttx
                    colNum = 12;
                    Ssutil.DbGetString(hStmt, colNum, "eqpttx", out mtChan.eqpttx, Constant.EQPTTX_SZ, out nullInd[colNum - 1]);

                    // MtChan member #12  string  eqptutx
                    colNum = 13;
                    Ssutil.DbGetString(hStmt, colNum, "eqptutx", out mtChan.eqptutx, Constant.EQPTUTX_SZ, out nullInd[colNum - 1]);

                    // MtChan member #13  float  pwrtx
                    colNum = 14;
                    Ssutil.DbGetFloat(hStmt, colNum, "pwrtx", out mtChan.pwrtx, out nullInd[colNum - 1]);

                    // MtChan member #14  float  atpccde
                    colNum = 15;
                    Ssutil.DbGetFloat(hStmt, colNum, "atpccde", out mtChan.atpccde, out nullInd[colNum - 1]);

                    // MtChan member #15  float  afsltx1
                    colNum = 16;
                    Ssutil.DbGetFloat(hStmt, colNum, "afsltx1", out mtChan.afsltx1, out nullInd[colNum - 1]);

                    // MtChan member #16  float  afsltx2
                    colNum = 17;
                    Ssutil.DbGetFloat(hStmt, colNum, "afsltx2", out mtChan.afsltx2, out nullInd[colNum - 1]);

                    // MtChan member #17  string  traftx
                    colNum = 18;
                    Ssutil.DbGetString(hStmt, colNum, "traftx", out mtChan.traftx, Constant.TRAFTX_SZ, out nullInd[colNum - 1]);

                    // MtChan member #18  string  srvctx
                    colNum = 19;
                    Ssutil.DbGetString(hStmt, colNum, "srvctx", out mtChan.srvctx, Constant.SRVCTX_SZ, out nullInd[colNum - 1]);

                    // MtChan member #19  string  stattx
                    colNum = 20;
                    Ssutil.DbGetString(hStmt, colNum, "stattx", out mtChan.stattx, Constant.STATTX_SZ, out nullInd[colNum - 1]);

                    // MtChan member #20  double  freqrx
                    colNum = 21;
                    Ssutil.DbGetDouble(hStmt, colNum, "freqrx", out mtChan.freqrx, out nullInd[colNum - 1]);

                    // MtChan member #21  string  polrx
                    colNum = 22;
                    Ssutil.DbGetString(hStmt, colNum, "polrx", out mtChan.polrx, Constant.POLRX_SZ, out nullInd[colNum - 1]);

                    // MtChan member #22  short  antnumbrx1
                    colNum = 23;
                    Ssutil.DbGetShort(hStmt, colNum, "antnumbrx1", out mtChan.antnumbrx1, out nullInd[colNum - 1]);

                    // MtChan member #23  short  antnumbrx2
                    colNum = 24;
                    Ssutil.DbGetShort(hStmt, colNum, "antnumbrx2", out mtChan.antnumbrx2, out nullInd[colNum - 1]);

                    // MtChan member #24  short  antnumbrx3
                    colNum = 25;
                    Ssutil.DbGetShort(hStmt, colNum, "antnumbrx3", out mtChan.antnumbrx3, out nullInd[colNum - 1]);

                    // MtChan member #25  string  eqptrx
                    colNum = 26;
                    Ssutil.DbGetString(hStmt, colNum, "eqptrx", out mtChan.eqptrx, Constant.ECODE_SZ, out nullInd[colNum - 1]);

                    // MtChan member #26  string  eqpturx
                    colNum = 27;
                    Ssutil.DbGetString(hStmt, colNum, "eqpturx", out mtChan.eqpturx, Constant.EQPTURX_SZ, out nullInd[colNum - 1]);

                    // MtChan member #27  float  afslrx1
                    colNum = 28;
                    Ssutil.DbGetFloat(hStmt, colNum, "afslrx1", out mtChan.afslrx1, out nullInd[colNum - 1]);

                    // MtChan member #28  float  afslrx2
                    colNum = 29;
                    Ssutil.DbGetFloat(hStmt, colNum, "afslrx2", out mtChan.afslrx2, out nullInd[colNum - 1]);

                    // MtChan member #29  float  afslrx3
                    colNum = 30;
                    Ssutil.DbGetFloat(hStmt, colNum, "afslrx3", out mtChan.afslrx3, out nullInd[colNum - 1]);

                    // MtChan member #30  float  pwrrx1
                    colNum = 31;
                    Ssutil.DbGetFloat(hStmt, colNum, "pwrrx1", out mtChan.pwrrx1, out nullInd[colNum - 1]);

                    // MtChan member #31  float  pwrrx2
                    colNum = 32;
                    Ssutil.DbGetFloat(hStmt, colNum, "pwrrx2", out mtChan.pwrrx2, out nullInd[colNum - 1]);

                    // MtChan member #32  float  pwrrx3
                    colNum = 33;
                    Ssutil.DbGetFloat(hStmt, colNum, "pwrrx3", out mtChan.pwrrx3, out nullInd[colNum - 1]);

                    // MtChan member #33  string  trafrx
                    colNum = 34;
                    Ssutil.DbGetString(hStmt, colNum, "trafrx", out mtChan.trafrx, Constant.TRAFCODE_SZ, out nullInd[colNum - 1]);

                    // MtChan member #34  float  esint
                    colNum = 35;
                    Ssutil.DbGetFloat(hStmt, colNum, "esint", out mtChan.esint, out nullInd[colNum - 1]);

                    // MtChan member #35  float  tsint
                    colNum = 36;
                    Ssutil.DbGetFloat(hStmt, colNum, "tsint", out mtChan.tsint, out nullInd[colNum - 1]);

                    // MtChan member #36  string  srvcrx
                    colNum = 37;
                    Ssutil.DbGetString(hStmt, colNum, "srvcrx", out mtChan.srvcrx, Constant.TRAFCODE_SZ, out nullInd[colNum - 1]);

                    // MtChan member #37  string  statrx
                    colNum = 38;
                    Ssutil.DbGetString(hStmt, colNum, "statrx", out mtChan.statrx, Constant.STATRX_SZ, out nullInd[colNum - 1]);

                    // MtChan member #38  string  routnumb
                    colNum = 39;
                    Ssutil.DbGetString(hStmt, colNum, "routnumb", out mtChan.routnumb, Constant.ROUTE_SZ, out nullInd[colNum - 1]);

                    // MtChan member #39  short  stnnumb
                    colNum = 40;
                    Ssutil.DbGetShort(hStmt, colNum, "stnnumb", out mtChan.stnnumb, out nullInd[colNum - 1]);

                    // MtChan member #40  short  hopnumb
                    colNum = 41;
                    Ssutil.DbGetShort(hStmt, colNum, "hopnumb", out mtChan.hopnumb, out nullInd[colNum - 1]);

                    // MtChan member #41  string  sdate
                    colNum = 42;
                    Ssutil.DbGetString(hStmt, colNum, "sdate", out mtChan.sdate, Constant.DATE_SZ, out nullInd[colNum - 1]);

                    // MtChan member #42  string  notetx
                    colNum = 43;
                    Ssutil.DbGetString(hStmt, colNum, "notetx", out mtChan.notetx, Constant.NOTA_SZ, out nullInd[colNum - 1]);

                    // MtChan member #43  string  noterx
                    colNum = 44;
                    Ssutil.DbGetString(hStmt, colNum, "noterx", out mtChan.noterx, Constant.NOTA_SZ, out nullInd[colNum - 1]);

                    // MtChan member #44  string  notegnl
                    colNum = 45;
                    Ssutil.DbGetString(hStmt, colNum, "notegnl", out mtChan.notegnl, Constant.NOTA_SZ, out nullInd[colNum - 1]);

                    // MtChan member #45  string  cpoint
                    colNum = 46;
                    Ssutil.DbGetString(hStmt, colNum, "cpoint", out mtChan.cpoint, Constant.CPOINT_SZ, out nullInd[colNum - 1]);

                    // MtChan member #46  string  feetx
                    colNum = 47;
                    Ssutil.DbGetString(hStmt, colNum, "feetx", out mtChan.feetx, Constant.FEETX_SZ, out nullInd[colNum - 1]);

                    // MtChan member #47  string  feerx
                    colNum = 48;
                    Ssutil.DbGetString(hStmt, colNum, "feerx", out mtChan.feerx, Constant.FEERX_SZ, out nullInd[colNum - 1]);

                    // MtChan member #48  string  mdate
                    colNum = 49;
                    Ssutil.DbGetString(hStmt, colNum, "mdate", out mtChan.mdate, Constant.DATE_SZ, out nullInd[colNum - 1]);

                    // MtChan member #49  string  mtime
                    colNum = 50;
                    Ssutil.DbGetString(hStmt, colNum, "mtime", out mtChan.mtime, Constant.TIME_SZ, out nullInd[colNum - 1]);

                    // MtChan member #50  string  userid
                    colNum = 51;
                    Ssutil.DbGetString(hStmt, colNum, "userid", out mtChan.userid, Constant.USERID_SZ, out nullInd[colNum - 1]);

                    nRet = Constant.SUCCESS;

                }
                catch (Exception e)
                {
                    string str = String.Format("Could not get Channel {0}, because of field {1}", colNum, e.Message);
                    GenUtil.SetError(1011, str);
                    //!@Log2.v("\n\nDynMdbChannel.MtFetchChannel(): Exit: ERROR: Marker L: " + str);
                    return -22;
                } //try-catch

            } //if-else

            Log2.n("\n\nDynMdbChannel.MtFetchChannel(): Exit");
            return nRet;
        } //end of MtFetchChannel

        /// <summary>
        /// Closes (releases) a cursor associated with a channel record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        public static int MtCloseChannel(int curHandle)
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

            /* Close the site cursor */
            SQLHANDLE hStmt = cursors[curHandle].hStmt;
            //!@ODBC.SQLCloseCursor(hStmt);
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(cursors[curHandle].hConn);
            cursors[curHandle].hConn = IntPtr.Zero;
            cursors[curHandle].cursorOpen = false;

            return 0;
        }



    }
}
