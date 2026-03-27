using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using _Configuration;
    using _NewLib;
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
    /// Provides methods to select, update, fetch, insert and delete PDF TS CHANNEL
    /// records in the database table <b>&lt;userID&gt;.ft_&lt;pdfName&gt;_chan</b>
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item> The methods in this class are used to perform operations on the
    /// CHANNEL table. The actual CHANNEL table name is a variable.</item>
    /// <item>The supported operations are: selectChannel, fetchChannel,
    /// updateChannel, insertChannel, deleteChannel and closeChannel.</item>
    /// <item>The caller must first call selectChannel to select the required rows:
    /// this method simply sets up the cursor for subsequent use.</item>
    /// <item>Once selectChannel has been called, the user must call fetchChannel
    /// to  retreive a row.</item>
    /// <item>The methods updateChannel, deleteChannel and insertChannel can only
    /// be called after a row has been fetched. Note: the code doesn't
    /// enforce this rule.</item>
    /// <item>The method closeChannel must be called to release ODBC resources 
    /// (connection and statement handles etc) and to release the cursor back the 'pool'.</item>
    /// </list>
    /// </remarks>
    public class DynChannel
    {
#if PINVOKE
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int ftSelectChannel([In] string table, [In] string searchCriteria, [In] string orderBy, [In] short openFlag);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        //private extern static int ftFetchChannel([In] int nCursor, [In, Out] IntPtr chanRecIntPtr, [In] int[] nullInd);
        private extern static int ftFetchChannel([In] int nCursor, [In, Out] FtChan chanRec, [In] SQLLEN[] nullInd);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int ftUpdateChannel([In] int nCursor, [In] FtChan ftChan, [In] SQLLEN[] nullInd);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int ftDeleteChannel([In] int nCursor);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int ftCloseChannel([In] int nCursor);

        public static int FtCloseChannel_NATIVE(int nCursor)
        {
            return ftCloseChannel(nCursor);
        }
        public static int FtDeleteChannel_NATIVE(int nCursor)
        {
            return ftDeleteChannel(nCursor);
        }
        public static int FtSelectChannel_NATIVE(string table, string searchCriteria, string orderBy)
        {
            return ftSelectChannel(table, searchCriteria, orderBy, 0);
        }

        public static int FtFetchChannel_NATIVE(int nCursor, out FtChan chanRec, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynChannel.FtFetchChannel_NATIVE(): Entry");

            //Create the FtChan and SQLLEN[] objects to output.
            chanRec = new FtChan();
            nullInd = new SQLLEN[FtChan.NUM_COLUMNS];

            //The default P/Invoke marshalling takes care of everything.
            int nRet = ftFetchChannel(nCursor, chanRec, nullInd);

            //...Log2.v("\n\nDynChannel.FtFetchChannel_NATIVE(): Exit");
            return nRet;
        }

        public static int FtUpdateChannel_NATIVE(int nCursor, FtChan chanRec, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynChannel.FtUpdateChannel_NATIVE(): Entry");

            int nRet = -666;

            //The default P/Invoke marshalling takes care of everything.
            nRet = ftUpdateChannel(nCursor, chanRec, nullInd);

            //...Log2.v("\n\nDynChannel.FtUpdateChannel_NATIVE(): Exit");
            return nRet;
        }

#endif
        //===================================================================================================================================
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;
        //===================================================================================================================================

        /// <summary>
        /// This method returns the integer index of the next free (available) FtCursor object.
        /// </summary>
        /// <remarks>
        ///  The user is responsible for populating the values of the cursor object 
        ///  (e.g. hConn, hStmt etc). The user is also responsible for setting the field
        ///  cursorOpen to true before using it and false to release it.
        /// </remarks>
        /// <returns></returns>
        /// <para> - integer index of the next free FtCursor object.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        public static int GetNextFreeCursor()
        {
            //Get next free cursor area.
            int curHandle = -1;
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
                    Application.Exit("\r\nDynChannel.GetNextFreeCursor(): ERROR: No available cursors.");
                }
            }
            else
            {
                //	Just use the next available cursor
                curHandle = nNextFreeCursor;
                //  And increment the free cursor count.
                nNextFreeCursor++;
            }

            cursors[curHandle].cursorOpen = true;

            return curHandle;
        }

        /// <summary>
        /// This method returns the integer index of the next free (available) FtCursor object
        /// for a prescribed table name.
        /// </summary>
        /// <remarks>
        ///  The user is responsible for populating the values of the cursor object 
        ///  (e.g. hConn, hStmt etc). The user is also responsible for setting the field
        ///  cursorOpen to true before using it and false to release it.
        /// </remarks>
        /// <returns></returns>
        /// <para> - integer index of the next free FtCursor object.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        public static int GetNextFreeCursor(string tableName)
        {
            int nextFreeCursor = GetNextFreeCursor();

            cursors[nextFreeCursor].tableName = tableName;

            return nextFreeCursor;
        }

        /// <summary>
        /// Retrieves a single row of channel data from a table in the database using 
        /// the FtCursor object created by a previous call to FtSelectChannel().
        /// </summary>
        /// <param name="curHandle"> - FtCursor object.</param>
        /// <param name="chanRec"> - a FtChan object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for chanRec.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int FtFetchChannel(int curHandle, out FtChan chanRec, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynChannel.FtFetchChannel(): Entry:  cursor = \r\n" + cursors[curHandle].ToString());
            //...Log2.v("\n\nDynChannel.FtFetchChannel(): Entry:");

            //Create an FtChan object to output.
            chanRec = new FtChan();
            //Create an array of nulls to output.
            nullInd = new SQLLEN[FtChan.NUM_COLUMNS];

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                //...Log2.v("\r\nDynChannel.FtFetchChannel(): FAIL: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // memset(chanRec, 0, sizeof(struct ftChan_));		/* clear to be safe */
            chanRec.Initialize();

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\r\nDynChannel.FtFetchChannel(): Exit: ODBC.SQLFetch() returned ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    //...Log2.v("\r\nDynChannel.FtFetchChannel(): ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return Constant.FAILURE;
                }
            }

            //...Log2.v("\r\nDynChannel.FtFetchChannel(): ODBC.SQLFetch(): SUCCEEDED");

            // Now read in the fields
            try
            {
                // FtChan member #0  string  cmd
                int colNum = 1;
                Ssutil.DbGetString(hStmt, colNum, "cmd", out chanRec.cmd, Constant.CMD_SZ, out nullInd[colNum - 1]);

                // FtChan member #1  string  recstat
                colNum = 2;
                Ssutil.DbGetString(hStmt, colNum, "recstat", out chanRec.recstat, Constant.RECSTAT_SZ, out nullInd[colNum - 1]);

                // FtChan member #2  string  call1
                colNum = 3;
                Ssutil.DbGetString(hStmt, colNum, "call1", out chanRec.call1, Constant.CALL_SZ, out nullInd[colNum - 1]);

                // FtChan member #3  string  call2
                colNum = 4;
                Ssutil.DbGetString(hStmt, colNum, "call2", out chanRec.call2, Constant.CALL_SZ, out nullInd[colNum - 1]);

                // FtChan member #4  string  bndcde
                colNum = 5;
                Ssutil.DbGetString(hStmt, colNum, "bndcde", out chanRec.bndcde, Constant.BNDCDE_SZ, out nullInd[colNum - 1]);

                // FtChan member #5  string  splan
                colNum = 6;
                Ssutil.DbGetString(hStmt, colNum, "splan", out chanRec.splan, Constant.PLAN_SZ, out nullInd[colNum - 1]);

                // FtChan member #6  short  hl
                colNum = 7;
                Ssutil.DbGetShort(hStmt, colNum, "hl", out chanRec.hl, out nullInd[colNum - 1]);

                // FtChan member #7  short  vh
                colNum = 8;
                Ssutil.DbGetShort(hStmt, colNum, "vh", out chanRec.vh, out nullInd[colNum - 1]);

                // FtChan member #8  string  chid
                colNum = 9;
                Ssutil.DbGetString(hStmt, colNum, "chid", out chanRec.chid, Constant.CHID_SZ, out nullInd[colNum - 1]);

                // FtChan member #9  double  freqtx
                colNum = 10;
                Ssutil.DbGetDouble(hStmt, colNum, "freqtx", out chanRec.freqtx, out nullInd[colNum - 1]);

                // FtChan member #10  string  poltx
                colNum = 11;
                Ssutil.DbGetString(hStmt, colNum, "poltx", out chanRec.poltx, Constant.POLTX_SZ, out nullInd[colNum - 1]);

                // FtChan member #11  short  antnumbtx1
                colNum = 12;
                Ssutil.DbGetShort(hStmt, colNum, "antnumbtx1", out chanRec.antnumbtx1, out nullInd[colNum - 1]);

                // FtChan member #12  short  antnumbtx2
                colNum = 13;
                Ssutil.DbGetShort(hStmt, colNum, "antnumbtx2", out chanRec.antnumbtx2, out nullInd[colNum - 1]);

                // FtChan member #13  string  eqpttx
                colNum = 14;
                Ssutil.DbGetString(hStmt, colNum, "eqpttx", out chanRec.eqpttx, Constant.EQPTTX_SZ, out nullInd[colNum - 1]);

                // FtChan member #14  string  eqptutx
                colNum = 15;
                Ssutil.DbGetString(hStmt, colNum, "eqptutx", out chanRec.eqptutx, Constant.EQPTUTX_SZ, out nullInd[colNum - 1]);

                // FtChan member #15  float  pwrtx
                colNum = 16;
                Ssutil.DbGetFloat(hStmt, colNum, "pwrtx", out chanRec.pwrtx, out nullInd[colNum - 1]);

                // FtChan member #16  float  atpccde
                colNum = 17;
                Ssutil.DbGetFloat(hStmt, colNum, "atpccde", out chanRec.atpccde, out nullInd[colNum - 1]);

                // FtChan member #17  float  afsltx1
                colNum = 18;
                Ssutil.DbGetFloat(hStmt, colNum, "afsltx1", out chanRec.afsltx1, out nullInd[colNum - 1]);

                // FtChan member #18  float  afsltx2
                colNum = 19;
                Ssutil.DbGetFloat(hStmt, colNum, "afsltx2", out chanRec.afsltx2, out nullInd[colNum - 1]);

                // FtChan member #19  string  traftx
                colNum = 20;
                Ssutil.DbGetString(hStmt, colNum, "traftx", out chanRec.traftx, Constant.TRAFTX_SZ, out nullInd[colNum - 1]);

                // FtChan member #20  string  srvctx
                colNum = 21;
                Ssutil.DbGetString(hStmt, colNum, "srvctx", out chanRec.srvctx, Constant.SRVCTX_SZ, out nullInd[colNum - 1]);

                // FtChan member #21  string  stattx
                colNum = 22;
                Ssutil.DbGetString(hStmt, colNum, "stattx", out chanRec.stattx, Constant.STATTX_SZ, out nullInd[colNum - 1]);

                // FtChan member #22  double  freqrx
                colNum = 23;
                Ssutil.DbGetDouble(hStmt, colNum, "freqrx", out chanRec.freqrx, out nullInd[colNum - 1]);

                // FtChan member #23  string  polrx
                colNum = 24;
                Ssutil.DbGetString(hStmt, colNum, "polrx", out chanRec.polrx, Constant.POLRX_SZ, out nullInd[colNum - 1]);

                // FtChan member #24  short  antnumbrx1
                colNum = 25;
                Ssutil.DbGetShort(hStmt, colNum, "antnumbrx1", out chanRec.antnumbrx1, out nullInd[colNum - 1]);

                // FtChan member #25  short  antnumbrx2
                colNum = 26;
                Ssutil.DbGetShort(hStmt, colNum, "antnumbrx2", out chanRec.antnumbrx2, out nullInd[colNum - 1]);

                // FtChan member #26  short  antnumbrx3
                colNum = 27;
                Ssutil.DbGetShort(hStmt, colNum, "antnumbrx3", out chanRec.antnumbrx3, out nullInd[colNum - 1]);

                // FtChan member #27  string  eqptrx
                colNum = 28;
                Ssutil.DbGetString(hStmt, colNum, "eqptrx", out chanRec.eqptrx, Constant.EQPTRX_SZ, out nullInd[colNum - 1]);

                // FtChan member #28  string  eqpturx
                colNum = 29;
                Ssutil.DbGetString(hStmt, colNum, "eqpturx", out chanRec.eqpturx, Constant.EQPTURX_SZ, out nullInd[colNum - 1]);

                // FtChan member #29  float  afslrx1
                colNum = 30;
                Ssutil.DbGetFloat(hStmt, colNum, "afslrx1", out chanRec.afslrx1, out nullInd[colNum - 1]);

                // FtChan member #30  float  afslrx2
                colNum = 31;
                Ssutil.DbGetFloat(hStmt, colNum, "afslrx2", out chanRec.afslrx2, out nullInd[colNum - 1]);

                // FtChan member #31  float  afslrx3
                colNum = 32;
                Ssutil.DbGetFloat(hStmt, colNum, "afslrx3", out chanRec.afslrx3, out nullInd[colNum - 1]);

                // FtChan member #32  float  pwrrx1
                colNum = 33;
                Ssutil.DbGetFloat(hStmt, colNum, "pwrrx1", out chanRec.pwrrx1, out nullInd[colNum - 1]);

                // FtChan member #33  float  pwrrx2
                colNum = 34;
                Ssutil.DbGetFloat(hStmt, colNum, "pwrrx2", out chanRec.pwrrx2, out nullInd[colNum - 1]);

                // FtChan member #34  float  pwrrx3
                colNum = 35;
                Ssutil.DbGetFloat(hStmt, colNum, "pwrrx3", out chanRec.pwrrx3, out nullInd[colNum - 1]);

                // FtChan member #35  string  trafrx
                colNum = 36;
                Ssutil.DbGetString(hStmt, colNum, "trafrx", out chanRec.trafrx, Constant.TRAFCODE_SZ, out nullInd[colNum - 1]);

                // FtChan member #36  float  esint
                colNum = 37;
                Ssutil.DbGetFloat(hStmt, colNum, "esint", out chanRec.esint, out nullInd[colNum - 1]);

                // FtChan member #37  float  tsint
                colNum = 38;
                Ssutil.DbGetFloat(hStmt, colNum, "tsint", out chanRec.tsint, out nullInd[colNum - 1]);

                // FtChan member #38  string  srvcrx
                colNum = 39;
                Ssutil.DbGetString(hStmt, colNum, "srvcrx", out chanRec.srvcrx, Constant.TRAFCODE_SZ, out nullInd[colNum - 1]);

                // FtChan member #39  string  statrx
                colNum = 40;
                Ssutil.DbGetString(hStmt, colNum, "statrx", out chanRec.statrx, Constant.STATRX_SZ, out nullInd[colNum - 1]);

                // FtChan member #40  string  routnumb
                colNum = 41;
                Ssutil.DbGetString(hStmt, colNum, "routnumb", out chanRec.routnumb, Constant.ROUTE_SZ, out nullInd[colNum - 1]);

                // FtChan member #41  short  stnnumb
                colNum = 42;
                Ssutil.DbGetShort(hStmt, colNum, "stnnumb", out chanRec.stnnumb, out nullInd[colNum - 1]);

                // FtChan member #42  short  hopnumb
                colNum = 43;
                Ssutil.DbGetShort(hStmt, colNum, "hopnumb", out chanRec.hopnumb, out nullInd[colNum - 1]);

                // FtChan member #43  string  sdate
                colNum = 44;
                Ssutil.DbGetString(hStmt, colNum, "sdate", out chanRec.sdate, Constant.DATE_SZ, out nullInd[colNum - 1]);

                // FtChan member #44  string  notetx
                colNum = 45;
                Ssutil.DbGetString(hStmt, colNum, "notetx", out chanRec.notetx, Constant.NOTA_SZ, out nullInd[colNum - 1]);

                // FtChan member #45  string  noterx
                colNum = 46;
                Ssutil.DbGetString(hStmt, colNum, "noterx", out chanRec.noterx, Constant.NOTA_SZ, out nullInd[colNum - 1]);

                // FtChan member #46  string  notegnl
                colNum = 47;
                Ssutil.DbGetString(hStmt, colNum, "notegnl", out chanRec.notegnl, Constant.NOTA_SZ, out nullInd[colNum - 1]);

                // FtChan member #47  string  cpoint
                colNum = 48;
                Ssutil.DbGetString(hStmt, colNum, "cpoint", out chanRec.cpoint, Constant.CPOINT_SZ, out nullInd[colNum - 1]);

                // FtChan member #48  string  feetx
                colNum = 49;
                Ssutil.DbGetString(hStmt, colNum, "feetx", out chanRec.feetx, Constant.FEETX_SZ, out nullInd[colNum - 1]);

                // FtChan member #49  string  feerx
                colNum = 50;
                Ssutil.DbGetString(hStmt, colNum, "feerx", out chanRec.feerx, Constant.FEERX_SZ, out nullInd[colNum - 1]);

                // FtChan member #50  string  mdate
                colNum = 51;
                Ssutil.DbGetString(hStmt, colNum, "mdate", out chanRec.mdate, Constant.DATE_SZ, out nullInd[colNum - 1]);

                // FtChan member #51  string  mtime
                colNum = 52;
                Ssutil.DbGetString(hStmt, colNum, "mtime", out chanRec.mtime, Constant.TIME_SZ, out nullInd[colNum - 1]);
            }
            catch (Exception e)
            {
                //...Log2.v("\r\nDynChannel.FtFetchChannel(): Exception caught: " + e.Message);
                //      seterr("dynChannel02 -- Could not get channel, because of field %s", cName);
                GenUtil.SetErr("dynChannel02 -- Could not get channel, because of field " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //	Save the key for deletes.
            //    strcpy_s(cursors[curHandle].cCurrentCall1, DB_CALL_SZ, chanRec->call1);
            cursors[curHandle].cCurrentCall1 = chanRec.call1;
            //    strcpy_s(cursors[curHandle].cCurrentCall2, DB_CALL_SZ, chanRec->call2);
            cursors[curHandle].cCurrentCall2 = chanRec.call2;
            //    strcpy_s(cursors[curHandle].cCurrentBndcde, 5, chanRec->bndcde);
            cursors[curHandle].cCurrentBndcde = chanRec.bndcde;
            //    strcpy_s(cursors[curHandle].cCurrentChid, CHID_SZ, pChan->chid);
            cursors[curHandle].cCurrentChid = chanRec.chid;

            //...Log2.v(chanRec.ToStringWN(nullInd));

            //...Log2.v("\n\nDynChannel.FtFetchChannel(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Selects channel records from an antenna table in the database for
        /// the prescribed site table name, SQL search criteria and ordering clauses; the 
        /// method returns an index to a FtCursor object that can be used by subsequent
        /// calls to FtFetchChannel().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for FtFetchChannel() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECUTE_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int FtSelectChannel(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynChannel.FtSelectChannel(): Entry");
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the select clause.
            //sprintf_s(stmt_buf, sizeof(stmt_buf), select, table);
            string stmt_buf = "select " + FtChan.AllColumnsForSqlSelect + " from " + table;

            //Construct the 'where' part of the select clause.
            //if (searchCriteria != NULL && *searchCriteria != '\0')
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                //Search criteria was specified.
                //strcat_s(stmt_buf, sizeof(stmt_buf), "where ");
                //strcat_s(stmt_buf, sizeof(stmt_buf), searchCriteria);
                stmt_buf += " where " + searchCriteria;
            }

            //If "order by" is not specified then assume it's for update.
            //if (orderBy == NULL || *orderBy == '\0')
            if (String.IsNullOrWhiteSpace(orderBy))
            {
                // Construct the 'for update' part of the select clause. */
                // strcat_s(stmt_buf, sizeof(stmt_buf), forUpdate);  <-- this is commmented out in FCSAwin?!
            }
            else if (!orderBy.Equals("NU"))
            {
                // Order by was specified.
                //strcat_s(stmt_buf, sizeof(stmt_buf), " order by ");
                //strcat_s(stmt_buf, sizeof(stmt_buf), orderBy);
                stmt_buf += " order by " + orderBy;
            }

            //...Log2.v("\r\nDynChannel.FtSelectChannel(): stmt_buf = " + stmt_buf);

            //Append the for update clause, as we want it in general.
            //safecat(stmt_buf, cAntUpdate, sizeof(stmt_buf));  <-- this is commmented out in FCSAwin?!

            //Get next free cursor area.
            curHandle = -1;
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
                    GenUtil.SetErr("dynChannel01 -- No more open cursors.");
                    return Error.NO_CURSOR_AVAILABLE;
                }
            }
            else
            {
                //	Just use the next available cursor
                curHandle = nNextFreeCursor;
                //  And increment the free cursor count.
                nNextFreeCursor++;
            }

            //Make an new connection with the DB.
            //SQLHDBC hConn = newConn();
            SQLHDBC hConn = Ssutil.NewConn();

            //Get a statement handle.
            //sqlRet = SQLAllocHandle(SQL_HANDLE_STMT, hConn, &hStmt);
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            //...Log2.v("\r\nDynChannel.FtSelectChannel(): [1] sqlRet = " + sqlRet);

            //Set the statement's attributes.
            //sqlRet = SQLSetStmtAttr(hStmt, SQL_ATTR_CURSOR_TYPE, (SQLPOINTER)SQL_CURSOR_DYNAMIC, 0);
            sqlRet = ODBC.SQLSetStmtAttr(hStmt, ODBC.SQL_ATTR_CURSOR_TYPE, (SQLPOINTER)ODBC.SQL_CURSOR_DYNAMIC, 0);
            //...Log2.v("\r\nDynChannel.FtSelectChannel(): [2] sqlRet = " + sqlRet);
            //sqlRet = SQLSetStmtAttr(hStmt, SQL_ATTR_CONCURRENCY, (SQLPOINTER)SQL_CONCUR_VALUES, 0);
            sqlRet = ODBC.SQLSetStmtAttr(hStmt, ODBC.SQL_ATTR_CONCURRENCY, (SQLPOINTER)ODBC.SQL_CONCUR_VALUES, 0);
            //...Log2.v("\r\nDynChannel.FtSelectChannel(): [3] sqlRet = " + sqlRet);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, ODBC.SQL_NTS);
            //...Log2.v("\r\nDynChannel.FtSelectChannel(): [4] sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "Could not Select Channel for criteria: " + searchCriteria + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.ODBC_EXECUTE_FAILED;
            }

            //	Populate the cursor handle structure
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].tableName = table;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cCurrentCall1 = "";
            cursors[curHandle].sqlQuery = stmt_buf;

            //...Log2.v("\n\nDynChannel.FtSelectChannel(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Closes (releases) a cursor associated with a PDF TS channel record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// FtCursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the FtCursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        public static int FtCloseChannel(int curHandle)
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
        /// Updates the fields in a PDF TS channel record to mirror those prescribed in
        /// a FtChan object.
        /// </summary>
        /// <param name="curHandle">- index of the FtCursor object.</param>
        /// <param name="pChan"> - FtChan object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pChan.</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - update attempt was successful.</para>
        /// <para>- ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>- ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>- ErrorMessages.ODBC_BINDING_ERROR - attempt to create an ODBC parameter binding failed.  </para>
        /// <para>- ErrorMessages.ODBC_QUERY_FAILED - update attempt failed - ODBC diagnostic information will be written to output.</para>
        public static int FtUpdateChannel(int curHandle, FtChan pChan, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynChannel.FtUpdateChannel(): Entry");

            //...Log2.v(pChan.ToStringWN(nullInd));

            //char update_buf[STMT_BUF_SZ];
            string update_buf;

            //SQLRETURN sqlRet = 0;
            SQLHANDLE hUpdate;

            SQLHANDLE hConn = Ssutil.NewConn();
            if (hConn == SQLHANDLE.Zero)
            {
                Application.Exit("\r\nDynChannel.FtUpdateChannel(): hConn = Ssutil.NewConn() FAILED");
            }

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (cursors[curHandle].pastLastRow)
            {
                /* Cursor is past the last row */
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_PAST_LAST_ROW);
            }

            //Prepare the update statement.

            //First construct the SQL 'where' clause.
            StringBuilder whereSB = new StringBuilder();
            whereSB.Append(" where call1='");
            whereSB.Append(cursors[curHandle].cCurrentCall1);
            whereSB.Append("' and ");
            whereSB.Append(" call2='");
            whereSB.Append(cursors[curHandle].cCurrentCall2);
            whereSB.Append("' and ");
            whereSB.Append(" bndcde='");
            whereSB.Append(cursors[curHandle].cCurrentBndcde);
            whereSB.Append("' and ");
            whereSB.Append(" chid='");
            whereSB.Append(cursors[curHandle].cCurrentChid);
            whereSB.Append("'");

            StringBuilder sb = new StringBuilder();
            sb.Append("update ");
            sb.Append(cursors[curHandle].tableName);
            sb.Append(" set ");
            sb.Append(FtChan.AllColumnsForSqlUpdateAsBindings);
            sb.Append(whereSB);

            update_buf = sb.ToString();

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pChan
            //into global memory with an SQLPOINTER pointer assigned to each one. FtChan provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = pChan.CopyToArrayOfSQLPOINTERs();

            // Bind the parameters.
            try
            {
                //This integer is used to enumerate the binding sequence; ODBC definition is that first binding is N = 1;
                int colNum;

                // FtChan member #0  string  cmd
                colNum = 1;
                Ssutil.DbBindStringInput(hUpdate, colNum, "cmd", parameterValuePtr[colNum - 1], (SQLULEN)pChan.cmd.Length, nullIndPtr[colNum - 1]);

                // FtChan member #1  string  recstat
                colNum = 2;
                Ssutil.DbBindStringInput(hUpdate, colNum, "recstat", parameterValuePtr[colNum - 1], (SQLULEN)pChan.recstat.Length, nullIndPtr[colNum - 1]);

                // FtChan member #2  string  call1
                colNum = 3;
                Ssutil.DbBindStringInput(hUpdate, colNum, "call1", parameterValuePtr[colNum - 1], (SQLULEN)pChan.call1.Length, nullIndPtr[colNum - 1]);

                // FtChan member #3  string  call2
                colNum = 4;
                Ssutil.DbBindStringInput(hUpdate, colNum, "call2", parameterValuePtr[colNum - 1], (SQLULEN)pChan.call2.Length, nullIndPtr[colNum - 1]);

                // FtChan member #4  string  bndcde
                colNum = 5;
                Ssutil.DbBindStringInput(hUpdate, colNum, "bndcde", parameterValuePtr[colNum - 1], (SQLULEN)pChan.bndcde.Length, nullIndPtr[colNum - 1]);

                // FtChan member #5  string  splan
                colNum = 6;
                Ssutil.DbBindStringInput(hUpdate, colNum, "splan", parameterValuePtr[colNum - 1], (SQLULEN)pChan.splan.Length, nullIndPtr[colNum - 1]);

                // FtChan member #6  short  hl
                colNum = 7;
                Ssutil.DbBindShortInput(hUpdate, colNum, "hl", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #7  short  vh
                colNum = 8;
                Ssutil.DbBindShortInput(hUpdate, colNum, "vh", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #8  string  chid
                colNum = 9;
                Ssutil.DbBindStringInput(hUpdate, colNum, "chid", parameterValuePtr[colNum - 1], (SQLULEN)pChan.chid.Length, nullIndPtr[colNum - 1]);

                // FtChan member #9  double  freqtx
                colNum = 10;
                Ssutil.DbBindDoubleInput(hUpdate, colNum, "freqtx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #10  string  poltx
                colNum = 11;
                Ssutil.DbBindStringInput(hUpdate, colNum, "poltx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.poltx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #11  short  antnumbtx1
                colNum = 12;
                Ssutil.DbBindShortInput(hUpdate, colNum, "antnumbtx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #12  short  antnumbtx2
                colNum = 13;
                Ssutil.DbBindShortInput(hUpdate, colNum, "antnumbtx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #13  string  eqpttx
                colNum = 14;
                Ssutil.DbBindStringInput(hUpdate, colNum, "eqpttx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.eqpttx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #14  string  eqptutx
                colNum = 15;
                Ssutil.DbBindStringInput(hUpdate, colNum, "eqptutx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.eqptutx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #15  float  pwrtx
                colNum = 16;
                Ssutil.DbBindFloatInput(hUpdate, colNum, "pwrtx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #16  float  atpccde
                colNum = 17;
                Ssutil.DbBindFloatInput(hUpdate, colNum, "atpccde", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #17  float  afsltx1
                colNum = 18;
                Ssutil.DbBindFloatInput(hUpdate, colNum, "afsltx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #18  float  afsltx2
                colNum = 19;
                Ssutil.DbBindFloatInput(hUpdate, colNum, "afsltx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #19  string  traftx
                colNum = 20;
                Ssutil.DbBindStringInput(hUpdate, colNum, "traftx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.traftx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #20  string  srvctx
                colNum = 21;
                Ssutil.DbBindStringInput(hUpdate, colNum, "srvctx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.srvctx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #21  string  stattx
                colNum = 22;
                Ssutil.DbBindStringInput(hUpdate, colNum, "stattx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.stattx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #22  double  freqrx
                colNum = 23;
                Ssutil.DbBindDoubleInput(hUpdate, colNum, "freqrx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #23  string  polrx
                colNum = 24;
                Ssutil.DbBindStringInput(hUpdate, colNum, "polrx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.polrx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #24  short  antnumbrx1
                colNum = 25;
                Ssutil.DbBindShortInput(hUpdate, colNum, "antnumbrx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #25  short  antnumbrx2
                colNum = 26;
                Ssutil.DbBindShortInput(hUpdate, colNum, "antnumbrx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #26  short  antnumbrx3
                colNum = 27;
                Ssutil.DbBindShortInput(hUpdate, colNum, "antnumbrx3", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #27  string  eqptrx
                colNum = 28;
                Ssutil.DbBindStringInput(hUpdate, colNum, "eqptrx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.eqptrx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #28  string  eqpturx
                colNum = 29;
                Ssutil.DbBindStringInput(hUpdate, colNum, "eqpturx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.eqpturx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #29  float  afslrx1
                colNum = 30;
                Ssutil.DbBindFloatInput(hUpdate, colNum, "afslrx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #30  float  afslrx2
                colNum = 31;
                Ssutil.DbBindFloatInput(hUpdate, colNum, "afslrx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #31  float  afslrx3
                colNum = 32;
                Ssutil.DbBindFloatInput(hUpdate, colNum, "afslrx3", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #32  float  pwrrx1
                colNum = 33;
                Ssutil.DbBindFloatInput(hUpdate, colNum, "pwrrx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #33  float  pwrrx2
                colNum = 34;
                Ssutil.DbBindFloatInput(hUpdate, colNum, "pwrrx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #34  float  pwrrx3
                colNum = 35;
                Ssutil.DbBindFloatInput(hUpdate, colNum, "pwrrx3", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #35  string  trafrx
                colNum = 36;
                Ssutil.DbBindStringInput(hUpdate, colNum, "trafrx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.trafrx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #36  float  esint
                colNum = 37;
                Ssutil.DbBindFloatInput(hUpdate, colNum, "esint", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #37  float  tsint
                colNum = 38;
                Ssutil.DbBindFloatInput(hUpdate, colNum, "tsint", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #38  string  srvcrx
                colNum = 39;
                Ssutil.DbBindStringInput(hUpdate, colNum, "srvcrx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.srvcrx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #39  string  statrx
                colNum = 40;
                Ssutil.DbBindStringInput(hUpdate, colNum, "statrx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.statrx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #40  string  routnumb
                colNum = 41;
                Ssutil.DbBindStringInput(hUpdate, colNum, "routnumb", parameterValuePtr[colNum - 1], (SQLULEN)pChan.routnumb.Length, nullIndPtr[colNum - 1])
                ;

                // FtChan member #41  short  stnnumb
                colNum = 42;
                Ssutil.DbBindShortInput(hUpdate, colNum, "stnnumb", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #42  short  hopnumb
                colNum = 43;
                Ssutil.DbBindShortInput(hUpdate, colNum, "hopnumb", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #43  string  sdate
                colNum = 44;
                Ssutil.DbBindStringInput(hUpdate, colNum, "sdate", parameterValuePtr[colNum - 1], (SQLULEN)pChan.sdate.Length, nullIndPtr[colNum - 1]);

                // FtChan member #44  string  notetx
                colNum = 45;
                Ssutil.DbBindStringInput(hUpdate, colNum, "notetx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.notetx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #45  string  noterx
                colNum = 46;
                Ssutil.DbBindStringInput(hUpdate, colNum, "noterx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.noterx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #46  string  notegnl
                colNum = 47;
                Ssutil.DbBindStringInput(hUpdate, colNum, "notegnl", parameterValuePtr[colNum - 1], (SQLULEN)pChan.notegnl.Length, nullIndPtr[colNum - 1]);

                // FtChan member #47  string  cpoint
                colNum = 48;
                Ssutil.DbBindStringInput(hUpdate, colNum, "cpoint", parameterValuePtr[colNum - 1], (SQLULEN)pChan.cpoint.Length, nullIndPtr[colNum - 1]);

                // FtChan member #48  string  feetx
                colNum = 49;
                Ssutil.DbBindStringInput(hUpdate, colNum, "feetx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.feetx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #49  string  feerx
                colNum = 50;
                Ssutil.DbBindStringInput(hUpdate, colNum, "feerx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.feerx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #50  string  mdate
                colNum = 51;
                Ssutil.DbBindStringInput(hUpdate, colNum, "mdate", parameterValuePtr[colNum - 1], (SQLULEN)pChan.mdate.Length, nullIndPtr[colNum - 1]);

                // FtChan member #51  string  mtime
                colNum = 52;
                Ssutil.DbBindStringInput(hUpdate, colNum, "mtime", parameterValuePtr[colNum - 1], (SQLULEN)pChan.mtime.Length, nullIndPtr[colNum - 1]);
            }
            catch (Exception e)
            {
                Ssutil.DbGetDiagStmt(hUpdate, "dynChannel03 -- Error binding parameters for site on field: " + e.Message);
                Ssutil.DisConnStmt(hConn, hUpdate);
                return -3;
            }

            //...Log2.v("\r\nDynChannel.FtUpdateChannel(): SQLExecDirect():\r\n" + update_buf);
            SQLRETURN sqlRet;
            sqlRet = ODBC.SQLExecDirect(hUpdate, update_buf, update_buf.Length);

            //...Log2.v("\r\nDynChannel.FtUpdateChannel(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = "dynChannel04 -- Error writing Channel: " + pChan.call1 + " " + pChan.call2 + " " + pChan.bndcde + " " + pChan.chid;
                Ssutil.DbGetDiagStmt(hUpdate, str);
                //...Log2.v(str);
                Ssutil.DisConnStmt(hConn, hUpdate);

                return -4;
            }

            //This method call releases hUpdate, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hUpdate);

            //Application.Exit("DynChannel.FtUpdateChannel(): forced termination.");

            //...Log2.v("\n\nDynChannel.FtUpdateChannel(): Exit");
            return 0;
        }


        /// <summary>
        /// Inserts a channel record into the database using column values prescribed 
        /// by the fields of the FtChan object.
        /// </summary>
        /// <param name="curHandle"> - index of the FtCursor object that encapsulates the details of the record insertion.</param>
        /// <param name="pChan"> - a FtChan object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pAnte</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int FtInsertChannel(int curHandle, FtChan pChan, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynChannel.FtInsertChannel(): Entry");
            //...Log2.v(pChan.ToStringWN(nullInd));
            //...Log2.v("\nDynChannel.FtInsertChannel(): Inserting FtChan with key:    {0}", pChan.KeysPlusTerseToString());

            //char update_buf[STMT_BUF_SZ];
            string update_buf;

            //SQLRETURN sqlRet = 0;
            SQLHANDLE hInsert;

            SQLHANDLE hConn = Ssutil.NewConn();
            if (hConn == SQLHANDLE.Zero)
            {
                Application.Exit("\r\nDynChannel.FtInsertChannel(): hConn = Ssutil.NewConn() FAILED");
            }

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hInsert);

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Ssutil.DisConnStmt(hConn, hInsert);
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (cursors[curHandle].pastLastRow)
            {
                /* Cursor is past the last row */
                Ssutil.DisConnStmt(hConn, hInsert);
                return (Error.DYN_PAST_LAST_ROW);
            }

            //Create the SQL insert statement.
            update_buf = FtChan.BuildSqlInsertString(cursors[curHandle].tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pChan
            //into global memory with an SQLPOINTER pointer assigned to each one. FtChan provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = pChan.CopyToArrayOfSQLPOINTERs();

            // Bind the parameters.
            try
            {
                //This integer is used to enumerate the binding sequence; ODBC definition is that first binding is N = 1;
                int colNum;

                // FtChan member #0  string  cmd
                colNum = 1;
                Ssutil.DbBindStringInput(hInsert, colNum, "cmd", parameterValuePtr[colNum - 1], (SQLULEN)pChan.cmd.Length, nullIndPtr[colNum - 1]);

                // FtChan member #1  string  recstat
                colNum = 2;
                Ssutil.DbBindStringInput(hInsert, colNum, "recstat", parameterValuePtr[colNum - 1], (SQLULEN)pChan.recstat.Length, nullIndPtr[colNum - 1]);

                // FtChan member #2  string  call1
                colNum = 3;
                Ssutil.DbBindStringInput(hInsert, colNum, "call1", parameterValuePtr[colNum - 1], (SQLULEN)pChan.call1.Length, nullIndPtr[colNum - 1]);

                // FtChan member #3  string  call2
                colNum = 4;
                Ssutil.DbBindStringInput(hInsert, colNum, "call2", parameterValuePtr[colNum - 1], (SQLULEN)pChan.call2.Length, nullIndPtr[colNum - 1]);

                // FtChan member #4  string  bndcde
                colNum = 5;
                Ssutil.DbBindStringInput(hInsert, colNum, "bndcde", parameterValuePtr[colNum - 1], (SQLULEN)pChan.bndcde.Length, nullIndPtr[colNum - 1]);

                // FtChan member #5  string  splan
                colNum = 6;
                Ssutil.DbBindStringInput(hInsert, colNum, "splan", parameterValuePtr[colNum - 1], (SQLULEN)pChan.splan.Length, nullIndPtr[colNum - 1]);

                // FtChan member #6  short  hl
                colNum = 7;
                Ssutil.DbBindShortInput(hInsert, colNum, "hl", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #7  short  vh
                colNum = 8;
                Ssutil.DbBindShortInput(hInsert, colNum, "vh", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #8  string  chid
                colNum = 9;
                Ssutil.DbBindStringInput(hInsert, colNum, "chid", parameterValuePtr[colNum - 1], (SQLULEN)pChan.chid.Length, nullIndPtr[colNum - 1]);

                // FtChan member #9  double  freqtx
                colNum = 10;
                Ssutil.DbBindDoubleInput(hInsert, colNum, "freqtx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #10  string  poltx
                colNum = 11;
                Ssutil.DbBindStringInput(hInsert, colNum, "poltx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.poltx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #11  short  antnumbtx1
                colNum = 12;
                Ssutil.DbBindShortInput(hInsert, colNum, "antnumbtx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #12  short  antnumbtx2
                colNum = 13;
                Ssutil.DbBindShortInput(hInsert, colNum, "antnumbtx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #13  string  eqpttx
                colNum = 14;
                Ssutil.DbBindStringInput(hInsert, colNum, "eqpttx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.eqpttx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #14  string  eqptutx
                colNum = 15;
                Ssutil.DbBindStringInput(hInsert, colNum, "eqptutx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.eqptutx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #15  float  pwrtx
                colNum = 16;
                Ssutil.DbBindFloatInput(hInsert, colNum, "pwrtx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #16  float  atpccde
                colNum = 17;
                Ssutil.DbBindFloatInput(hInsert, colNum, "atpccde", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #17  float  afsltx1
                colNum = 18;
                Ssutil.DbBindFloatInput(hInsert, colNum, "afsltx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #18  float  afsltx2
                colNum = 19;
                Ssutil.DbBindFloatInput(hInsert, colNum, "afsltx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #19  string  traftx
                colNum = 20;
                Ssutil.DbBindStringInput(hInsert, colNum, "traftx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.traftx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #20  string  srvctx
                colNum = 21;
                Ssutil.DbBindStringInput(hInsert, colNum, "srvctx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.srvctx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #21  string  stattx
                colNum = 22;
                Ssutil.DbBindStringInput(hInsert, colNum, "stattx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.stattx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #22  double  freqrx
                colNum = 23;
                Ssutil.DbBindDoubleInput(hInsert, colNum, "freqrx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #23  string  polrx
                colNum = 24;
                Ssutil.DbBindStringInput(hInsert, colNum, "polrx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.polrx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #24  short  antnumbrx1
                colNum = 25;
                Ssutil.DbBindShortInput(hInsert, colNum, "antnumbrx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #25  short  antnumbrx2
                colNum = 26;
                Ssutil.DbBindShortInput(hInsert, colNum, "antnumbrx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #26  short  antnumbrx3
                colNum = 27;
                Ssutil.DbBindShortInput(hInsert, colNum, "antnumbrx3", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #27  string  eqptrx
                colNum = 28;
                Ssutil.DbBindStringInput(hInsert, colNum, "eqptrx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.eqptrx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #28  string  eqpturx
                colNum = 29;
                Ssutil.DbBindStringInput(hInsert, colNum, "eqpturx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.eqpturx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #29  float  afslrx1
                colNum = 30;
                Ssutil.DbBindFloatInput(hInsert, colNum, "afslrx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #30  float  afslrx2
                colNum = 31;
                Ssutil.DbBindFloatInput(hInsert, colNum, "afslrx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #31  float  afslrx3
                colNum = 32;
                Ssutil.DbBindFloatInput(hInsert, colNum, "afslrx3", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #32  float  pwrrx1
                colNum = 33;
                Ssutil.DbBindFloatInput(hInsert, colNum, "pwrrx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #33  float  pwrrx2
                colNum = 34;
                Ssutil.DbBindFloatInput(hInsert, colNum, "pwrrx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #34  float  pwrrx3
                colNum = 35;
                Ssutil.DbBindFloatInput(hInsert, colNum, "pwrrx3", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #35  string  trafrx
                colNum = 36;
                Ssutil.DbBindStringInput(hInsert, colNum, "trafrx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.trafrx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #36  float  esint
                colNum = 37;
                Ssutil.DbBindFloatInput(hInsert, colNum, "esint", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #37  float  tsint
                colNum = 38;
                Ssutil.DbBindFloatInput(hInsert, colNum, "tsint", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #38  string  srvcrx
                colNum = 39;
                Ssutil.DbBindStringInput(hInsert, colNum, "srvcrx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.srvcrx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #39  string  statrx
                colNum = 40;
                Ssutil.DbBindStringInput(hInsert, colNum, "statrx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.statrx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #40  string  routnumb
                colNum = 41;
                Ssutil.DbBindStringInput(hInsert, colNum, "routnumb", parameterValuePtr[colNum - 1], (SQLULEN)pChan.routnumb.Length, nullIndPtr[colNum - 1]);

                // FtChan member #41  short  stnnumb
                colNum = 42;
                Ssutil.DbBindShortInput(hInsert, colNum, "stnnumb", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #42  short  hopnumb
                colNum = 43;
                Ssutil.DbBindShortInput(hInsert, colNum, "hopnumb", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                // FtChan member #43  string  sdate
                colNum = 44;
                Ssutil.DbBindStringInput(hInsert, colNum, "sdate", parameterValuePtr[colNum - 1], (SQLULEN)pChan.sdate.Length, nullIndPtr[colNum - 1]);

                // FtChan member #44  string  notetx
                colNum = 45;
                Ssutil.DbBindStringInput(hInsert, colNum, "notetx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.notetx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #45  string  noterx
                colNum = 46;
                Ssutil.DbBindStringInput(hInsert, colNum, "noterx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.noterx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #46  string  notegnl
                colNum = 47;
                Ssutil.DbBindStringInput(hInsert, colNum, "notegnl", parameterValuePtr[colNum - 1], (SQLULEN)pChan.notegnl.Length, nullIndPtr[colNum - 1]);

                // FtChan member #47  string  cpoint
                colNum = 48;
                Ssutil.DbBindStringInput(hInsert, colNum, "cpoint", parameterValuePtr[colNum - 1], (SQLULEN)pChan.cpoint.Length, nullIndPtr[colNum - 1]);

                // FtChan member #48  string  feetx
                colNum = 49;
                Ssutil.DbBindStringInput(hInsert, colNum, "feetx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.feetx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #49  string  feerx
                colNum = 50;
                Ssutil.DbBindStringInput(hInsert, colNum, "feerx", parameterValuePtr[colNum - 1], (SQLULEN)pChan.feerx.Length, nullIndPtr[colNum - 1]);

                // FtChan member #50  string  mdate
                colNum = 51;
                Ssutil.DbBindStringInput(hInsert, colNum, "mdate", parameterValuePtr[colNum - 1], (SQLULEN)pChan.mdate.Length, nullIndPtr[colNum - 1]);

                // FtChan member #51  string  mtime
                colNum = 52;
                Ssutil.DbBindStringInput(hInsert, colNum, "mtime", parameterValuePtr[colNum - 1], (SQLULEN)pChan.mtime.Length, nullIndPtr[colNum - 1]);
            }
            catch (Exception e)
            {
                Ssutil.DbGetDiagStmt(hInsert, "dynChannel03 -- Error binding parameters for site on field: " + e.Message);
                Ssutil.DisConnStmt(hConn, hInsert);
                return -3;
            }

            //...Log2.v("\r\nDynChannel.FtInsertChannel(): SQLExecDirect():\r\n" + update_buf);
            SQLRETURN sqlRet;
            sqlRet = ODBC.SQLExecDirect(hInsert, update_buf, update_buf.Length);

            //...Log2.v("\r\nDynChannel.FtInsertChannel(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = "dynChannel04 -- Error writing Channel: " + pChan.call1 + " " + pChan.call2 + " " + pChan.bndcde + " " + pChan.chid;
                Ssutil.DbGetDiagStmt(hInsert, str);
                Log2.e("\n\nDynChannel.FtInsertChannel(): ERROR: sqlRet = {0} for keys = {1}", sqlRet, pChan.KeysTerseToString());
                Log2.e("\n" + ODBC.GetDiagnostics(hInsert, update_buf));
                Ssutil.DisConnStmt(hConn, hInsert);
                return -4;
            }

            //This method call releases hInsert, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hInsert);

            //Application.Exit("DynChannel.FtInsertChannel(): forced termination.");

            //...Log2.v("\n\nDynChannel.FtInsertChannel(): Exit");
            return 0;
        }

        /// <summary>
        /// Deletes a prescribed PDF TS channel record from the database.
        /// </summary>
        /// <param name="curHandle"> - index of the FtCursor object that encapsulates the details of the record deletion.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - deletion attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        public static int FtDeleteChannel(int curHandle)
        {
            //...Log2.v("\n\nDynChannel.FtDeleteChannel(): Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hConn = Ssutil.NewConn();
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (cursors[curHandle].pastLastRow)
            {
                /* Cursor is past the last row */
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_PAST_LAST_ROW);
            }

            string delete_buf = String.Format("delete from {0} where call1='{1}' and call2='{2}' and bndcde='{3}' and chid='{4}'",
                cursors[curHandle].tableName,
                cursors[curHandle].cCurrentCall1,
                cursors[curHandle].cCurrentCall2,
                cursors[curHandle].cCurrentBndcde,
                cursors[curHandle].cCurrentChid
                );

            //...Log2.v("\r\nDynChannel.FtDeleteChannel(): SQLExecDirect():\r\n" + delete_buf);
            sqlRet = ODBC.SQLExecDirect(hUpdate, delete_buf, delete_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hUpdate, "FtDeleteChannel01 -- Error deleting from " + cursors[curHandle].tableName);
                Ssutil.DisConnStmt(hConn, hUpdate);
                return -1;
            }

            Ssutil.DisConnStmt(hConn, hUpdate);

            //...Log2.v("\n\nDynChannel.FtDeleteChannel(): Exit");
            return 0;
        }

        /// <summary>
        /// This method returns a fully-populated FtAnte object and its associated array of nullInds
        /// corresponding to a single record fetched from the User table ft_XXX_chan where XXX is 
        /// the PDF name; the (FtChan) record is uniquely identified by its key values (call1, 
        /// call2, bndcde, chid).
        /// </summary>
        /// <param name="pdfName"></param>
        /// <param name="call1"></param>
        /// <param name="call2"></param>
        /// <param name="bndcde"></param>
        /// <param name="chid"></param>
        /// <param name="ftChan"></param>
        /// <param name="nullInds"></param>
        /// <param name="found"></param>
        /// <returns></returns>
        public static int FetchFtChanByKey(string pdfName, string call1, string call2, string bndcde, string chid,
                                           out FtChan ftChan, out SQLLEN[] nullInds, out bool found)
        {
            // 'out' requirements.
            ftChan = null;
            nullInds = null;
            found = false;

            int retVal = Constant.SUCCESS;
            string tableName;
            int cursorID;
            int rc;
            string whereClause;

            // Get the complete SQL Table name from the PDF 'short' name.
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

            // Form the SQL SELECT where clause.
            whereClause = String.Format("call1='{0}' AND call2='{1}' AND bndcde='{2}' AND chid='{3}'",
                                         call1, call2, bndcde, chid);

            // SELECT the ftAnte by its key.
            cursorID = DynChannel.FtSelectChannel(tableName, whereClause, "");

            // If the SELECT worked then attempt to fetch the ftSite.
            if (cursorID < 0)
            {
                retVal = Constant.FAILURE;
            }
            else
            {
                // Constant.SUCCESS                - fetch attempt was successful.</para>
                // Constant.FAILURE                - fetch attempt failed.</para>
                // ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
                // ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
                // ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
                rc = DynChannel.FtFetchChannel(cursorID, out ftChan, out nullInds);

                switch (rc)
                {
                    case Constant.SUCCESS:
                        retVal = Constant.SUCCESS;
                        found = true;
                        break;
                    case ODBC.SQL_NO_DATA:
                        found = false;
                        retVal = Constant.SUCCESS;
                        break;
                    default:
                        retVal = Constant.FAILURE;
                        break;
                }
            }

            DynChannel.FtCloseChannel(cursorID);

            return retVal;
        }

        /// <summary>
        /// This method outputs a list of FtChan objects as SELECTed from a prescribed database table with
        /// prescribed search criteria (SQL WHERE clause) and precribed ORDER BY clause.
        /// </summary>
        /// <param name="chanTableName"></param>
        /// <param name="searchCriteria"></param>
        /// <param name="orderBy"></param>
        /// <param name="chanList"></param>
        /// <returns></returns>
        public static int GetListOfFtChans(string chanTableName, string searchCriteria, string orderBy, out List<FtChan> chanList)
        {
            // 'out' requirement.
            chanList = new List<FtChan>();

            FtChan ftChan;
            SQLLEN[] nullInds;
            int nRet = Constant.SUCCESS;

            //...Log2.v("\nDynChannel.GetListOfChannels(): chanTableName = " + chanTableName);

            int handle = DynChannel.FtSelectChannel(chanTableName, searchCriteria, orderBy);
            if (handle < 0) return Constant.FAILURE;

            //...Log2.v("\nDynChannel.GetListOfChannels(): select site handle = " + handle);

            while (DynChannel.FtFetchChannel(handle, out ftChan, out nullInds) == Constant.SUCCESS)
            {
                chanList.Add(ftChan);

                //...Log2.v("\n" + String.Format("DynChannel.GetListOfChannels(): ftChan = {0}, freqtx = {1}, freqrx = {2}", ftChan.KeysToString(), ftChan.freqtx, ftChan.freqrx));
            }

            DynChannel.FtCloseChannel(handle);

            return nRet;
        }
        /*
                /// <summary>
                /// This method returns true if a record exists in a table that has a prescribed 
                /// key: call1, call2, bndcde and chid; the table name is prescribed via the cursor.
                /// </summary>
                /// <param name="call1"> the prescribed local 'call sign' string.</param>
                /// <param name="call2"> the prescribed remote 'call sign' string.</param>
                /// <param name="bndcde"> the prescribed bandcode.</param>
                /// <param name="chid"> the prescribed channel ID' string.</param>
                /// <returns></returns>
           */

        /// <summary>
        /// This method returns true if a record exists in a table that has a prescribed 
        /// key: call1, call2, bndcde and chid as provided by the FtChan object; the table 
        /// name is prescribed via the cursor handle.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="ftChan"></param>
        /// <returns></returns>
        public static bool RecordWithKeyExists(int handle, FtChan ftChan)
        {
            string whereClause = String.Format(" call1 = '{0}' AND call2 = '{1}' AND bndcde = '{2}' AND chid = '{3}' ",
                                                ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid);

            // These columns comprise the 'key' for a ft_XXX_chan table record and so there
            // can only be 0 or 1 records.
            return (Ssutil.DbCountRows(cursors[handle].tableName, whereClause) == 1);
        }













    }
}
