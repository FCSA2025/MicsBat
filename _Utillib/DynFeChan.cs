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

    public class DynFeChan
    {

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
                    Application.Exit("\r\nDynFeChan.GetNextFreeCursor(): ERROR: No available cursors.");
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
        /// Retrieves a single row of channel data from a table in the database using 
        /// the FtCursor object created by a previous call to FtSelectChannel().
        /// </summary>
        /// <param name="curHandle"> - FtCursor object.</param>
        /// <param name="chanRec"> - a FtChan object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC out nullInds for chanRec.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int FeFetchChan(int curHandle, out FeChan chanRec, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynFeChan.FeFetchChan(): Entry:");

            //Create an FtChan object to output.
            chanRec = new FeChan();
            //Create an array of nulls to output.
            nullInd = new SQLLEN[FeChan.NUM_COLUMNS];

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Log2.e("\r\nDynFeChan.FeFetchChan(): FAIL: if (!cursors[curHandle].cursorOpen)");
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
                    //...Log2.v("\r\nDynFeChan.FeFetchChan(): ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\r\nDynFeChan.FeFetchChan(): ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return Constant.FAILURE;
                }
            }

            //...Log2.v("\r\nDynFeChan.FeFetchChan(): ODBC.SQLFetch(): SUCCEEDED");

            // Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "cmd", out chanRec.cmd, Constant.CMD_SZ, out nullInd[FeChan.CMD]);
                Ssutil.DbGetString(hStmt, 2, "recstat", out chanRec.recstat, Constant.RECSTAT_SZ, out nullInd[FeChan.RECSTAT]);
                Ssutil.DbGetString(hStmt, 3, "location", out chanRec.location, Constant.LOCATION_SZ, out nullInd[FeChan.LOCATION]);
                Ssutil.DbGetString(hStmt, 4, "call1", out chanRec.call1, Constant.CALLSIGN_SZ, out nullInd[FeChan.CALL1]);
                Ssutil.DbGetString(hStmt, 5, "chid", out chanRec.chid, Constant.CHID_SZ, out nullInd[FeChan.CHID]);
                Ssutil.DbGetDouble(hStmt, 6, "freqtx", out chanRec.freqtx, out nullInd[FeChan.FREQTX]);
                Ssutil.DbGetString(hStmt, 7, "poltx", out chanRec.poltx, Constant.POLTX_SZ, out nullInd[FeChan.POLTX]);
                Ssutil.DbGetFloat(hStmt, 8, "maxtxpower", out chanRec.maxtxpower, out nullInd[FeChan.MAXTXPOWER]);
                Ssutil.DbGetFloat(hStmt, 9, "pwrtx", out chanRec.pwrtx, out nullInd[FeChan.PWRTX]);
                Ssutil.DbGetFloat(hStmt, 10, "p4khz", out chanRec.p4khz, out nullInd[FeChan.P4KHZ]);
                Ssutil.DbGetString(hStmt, 11, "eqpttx", out chanRec.eqpttx, Constant.EQPTTX_SZ, out nullInd[FeChan.EQPTTX]);
                Ssutil.DbGetString(hStmt, 12, "traftx", out chanRec.traftx, Constant.TRAFTX_SZ, out nullInd[FeChan.TRAFTX]);
                Ssutil.DbGetString(hStmt, 13, "stattx", out chanRec.stattx, Constant.STATTX_SZ, out nullInd[FeChan.STATTX]);
                Ssutil.DbGetString(hStmt, 14, "feetx", out chanRec.feetx, Constant.FEETX_SZ, out nullInd[FeChan.FEETX]);
                Ssutil.DbGetDouble(hStmt, 15, "freqrx", out chanRec.freqrx, out nullInd[FeChan.FREQRX]);
                Ssutil.DbGetString(hStmt, 16, "polrx", out chanRec.polrx, Constant.POLRX_SZ, out nullInd[FeChan.POLRX]);
                Ssutil.DbGetFloat(hStmt, 17, "pwrrx", out chanRec.pwrrx, out nullInd[FeChan.PWRRX]);
                Ssutil.DbGetString(hStmt, 18, "eqptrx", out chanRec.eqptrx, Constant.EQPTRX_SZ, out nullInd[FeChan.EQPTRX]);
                Ssutil.DbGetString(hStmt, 19, "trafrx", out chanRec.trafrx, Constant.FE_CHAN_TRAFRX_SZ, out nullInd[FeChan.TRAFRX]);
                Ssutil.DbGetString(hStmt, 20, "statrx", out chanRec.statrx, Constant.STATRX_SZ, out nullInd[FeChan.STATRX]);
                Ssutil.DbGetFloat(hStmt, 21, "i20", out chanRec.i20, out nullInd[FeChan.I20]);
                Ssutil.DbGetFloat(hStmt, 22, "it01", out chanRec.it01, out nullInd[FeChan.IT01]);
                Ssutil.DbGetFloat(hStmt, 23, "ip01", out chanRec.ip01, out nullInd[FeChan.IP01]);
                Ssutil.DbGetString(hStmt, 24, "feerx", out chanRec.feerx, Constant.FEERX_SZ, out nullInd[FeChan.FEERX]);
                Ssutil.DbGetString(hStmt, 25, "notc", out chanRec.notc, Constant.NOTA_SZ, out nullInd[FeChan.NOTC]);
                Ssutil.DbGetString(hStmt, 26, "srvctx", out chanRec.srvctx, Constant.SRVCTX_SZ, out nullInd[FeChan.SRVCTX]);
                Ssutil.DbGetString(hStmt, 27, "srvcrx", out chanRec.srvcrx, Constant.SRVCTX_SZ, out nullInd[FeChan.SRVCRX]);
                Ssutil.DbGetString(hStmt, 28, "mdate", out chanRec.mdate, Constant.DATE_SZ, out nullInd[FeChan.MDATE]);
                Ssutil.DbGetString(hStmt, 29, "mtime", out chanRec.mtime, Constant.TIME_SZ, out nullInd[FeChan.MTIME]);

            }
            catch (Exception e)
            {
                Log2.e("\r\nDynFeChan.FeFetchChan(): ERROR: Exception caught: " + e.Message);
                GenUtil.SetErr("dynfeChan02 -- Could not get Channel, because of field " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\n\nDynFeChan.FeFetchChan(): chanRec = \n" + chanRec.ToStringWN(nullInd));

            //	Save the key for deletes.
            cursors[curHandle].cCurrentLoc = chanRec.location;
            cursors[curHandle].cCurrentCall1 = chanRec.call1;
            cursors[curHandle].cCurrentChid = chanRec.chid;

            //...Log2.v("\n\nDynFeChan.FeFetchChan(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Selects channel records from a FeChan table in the database for
        /// the prescribed site table name, SQL search criteria and ordering clauses; the 
        /// method returns an index to a FCursor object that can be used by subsequent
        /// calls to FeFetchChan().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for FeFetchChan() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed to return data. </para>
        public static int FeSelectChan(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynFeChan.FeSelectChan(): Entry");
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the select clause.
            string stmt_buf = "select " + FeChan.AllColumnsForSqlSelect + " from " + table;

            //Construct the 'where' part of the select clause.
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                //Search criteria was specified.
                //strcat_s(stmt_buf, sizeof(stmt_buf), "where ");
                //strcat_s(stmt_buf, sizeof(stmt_buf), searchCriteria);
                stmt_buf += " where " + searchCriteria;
            }

            //If "order by" is not specified then assume it's for update.
            if (String.IsNullOrWhiteSpace(orderBy))
            {
                // Construct the 'for update' part of the select clause. */
                // strcat_s(stmt_buf, sizeof(stmt_buf), forUpdate);  <-- this is commmented out in FCSAwin?!
            }
            else if (!orderBy.Equals("NU"))
            {
                // Order by was specified.
                stmt_buf += " order by " + orderBy;
            }

            //...Log2.v("\nDynFeChan.FeSelectChan(): query = " + stmt_buf);

            //Get next free cursor.
            curHandle = GetNextFreeCursor();

            //Make an new connection with the DB.
            SQLHDBC hConn = Ssutil.NewConn();

            //Get a statement handle.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, ODBC.SQL_NTS);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "Could not Select Channel for criteria: " + searchCriteria + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                Log2.e("\nDynFeChan.FeSelectChan(): ERROR: SQLExecDirect() failed.");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //	Populate the cursor handle structure
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].tableName = table;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cCurrentLoc = "";
            cursors[curHandle].cCurrentCall1 = "";
            cursors[curHandle].cCurrentChid = "";
            cursors[curHandle].sqlQuery = stmt_buf;

            //...Log2.v("\n\nDynFeChan.FeSelectChan(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Closes (releases) a cursor associated with a PDF ES channel record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int FeCloseChan(int curHandle)
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

            // Close the cursor.

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
        /// Inserts a FeChan record into the database using column values prescribed 
        /// by the fields of the FeChan object.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object that encapsulates the details of the record insertion.</param>
        /// <param name="pChan"> - a FeChan object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pChan</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int FeInsertChan(int curHandle, FeChan pChan, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynChan.FeInsertChan(): Entry");

            string cSQL;
            SQLRETURN sqlRet;

            SQLHANDLE hStmt;

            SQLHANDLE hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynChan.FeInsertChan(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Ssutil.DisConnStmt(hConn, hStmt);
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (cursors[curHandle].pastLastRow)
            {
                /* Cursor is past the last row */
                Ssutil.DisConnStmt(hConn, hStmt);
                return (Error.DYN_PAST_LAST_ROW);
            }

            //Create the SQL insert statement.
            cSQL = FeChan.BuildSqlInsertString(cursors[curHandle].tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pChan
            //into global memory with an SQLPOINTER pointer assigned to each one. FeChan provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = pChan.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                // Use automatic bind parameter indexing.
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(hStmt, 0, "cmd", parameterValuePtr[FeChan.CMD], (SQLULEN)pChan.cmd.Length, nullIndPtr[FeChan.CMD]);

                Ssutil.DbBindStringInput(hStmt, 0, "recstat", parameterValuePtr[FeChan.RECSTAT], (SQLULEN)pChan.recstat.Length, nullIndPtr[FeChan.RECSTAT]);

                Ssutil.DbBindStringInput(hStmt, 0, "location", parameterValuePtr[FeChan.LOCATION], (SQLULEN)pChan.location.Length, nullIndPtr[FeChan.LOCATION]);

                Ssutil.DbBindStringInput(hStmt, 0, "call1", parameterValuePtr[FeChan.CALL1], (SQLULEN)pChan.call1.Length, nullIndPtr[FeChan.CALL1]);

                Ssutil.DbBindStringInput(hStmt, 0, "chid", parameterValuePtr[FeChan.CHID], (SQLULEN)pChan.chid.Length, nullIndPtr[FeChan.CHID]);

                Ssutil.DbBindDoubleInput(hStmt, 0, "freqtx", parameterValuePtr[FeChan.FREQTX], nullIndPtr[FeChan.FREQTX]);

                Ssutil.DbBindStringInput(hStmt, 0, "poltx", parameterValuePtr[FeChan.POLTX], (SQLULEN)pChan.poltx.Length, nullIndPtr[FeChan.POLTX]);

                Ssutil.DbBindFloatInput(hStmt, 0, "maxtxpower", parameterValuePtr[FeChan.MAXTXPOWER], nullIndPtr[FeChan.MAXTXPOWER]);

                Ssutil.DbBindFloatInput(hStmt, 0, "pwrtx", parameterValuePtr[FeChan.PWRTX], nullIndPtr[FeChan.PWRTX]);

                Ssutil.DbBindFloatInput(hStmt, 0, "p4khz", parameterValuePtr[FeChan.P4KHZ], nullIndPtr[FeChan.P4KHZ]);

                Ssutil.DbBindStringInput(hStmt, 0, "eqpttx", parameterValuePtr[FeChan.EQPTTX], (SQLULEN)pChan.eqpttx.Length, nullIndPtr[FeChan.EQPTTX]);

                Ssutil.DbBindStringInput(hStmt, 0, "traftx", parameterValuePtr[FeChan.TRAFTX], (SQLULEN)pChan.traftx.Length, nullIndPtr[FeChan.TRAFTX]);

                Ssutil.DbBindStringInput(hStmt, 0, "stattx", parameterValuePtr[FeChan.STATTX], (SQLULEN)pChan.stattx.Length, nullIndPtr[FeChan.STATTX]);

                Ssutil.DbBindStringInput(hStmt, 0, "feetx", parameterValuePtr[FeChan.FEETX], (SQLULEN)pChan.feetx.Length, nullIndPtr[FeChan.FEETX]);

                Ssutil.DbBindDoubleInput(hStmt, 0, "freqrx", parameterValuePtr[FeChan.FREQRX], nullIndPtr[FeChan.FREQRX]);

                Ssutil.DbBindStringInput(hStmt, 0, "polrx", parameterValuePtr[FeChan.POLRX], (SQLULEN)pChan.polrx.Length, nullIndPtr[FeChan.POLRX]);

                Ssutil.DbBindFloatInput(hStmt, 0, "pwrrx", parameterValuePtr[FeChan.PWRRX], nullIndPtr[FeChan.PWRRX]);

                Ssutil.DbBindStringInput(hStmt, 0, "eqptrx", parameterValuePtr[FeChan.EQPTRX], (SQLULEN)pChan.eqptrx.Length, nullIndPtr[FeChan.EQPTRX]);

                Ssutil.DbBindStringInput(hStmt, 0, "trafrx", parameterValuePtr[FeChan.TRAFRX], (SQLULEN)pChan.trafrx.Length, nullIndPtr[FeChan.TRAFRX]);

                Ssutil.DbBindStringInput(hStmt, 0, "statrx", parameterValuePtr[FeChan.STATRX], (SQLULEN)pChan.statrx.Length, nullIndPtr[FeChan.STATRX]);

                Ssutil.DbBindFloatInput(hStmt, 0, "i20", parameterValuePtr[FeChan.I20], nullIndPtr[FeChan.I20]);

                Ssutil.DbBindFloatInput(hStmt, 0, "it01", parameterValuePtr[FeChan.IT01], nullIndPtr[FeChan.IT01]);

                Ssutil.DbBindFloatInput(hStmt, 0, "ip01", parameterValuePtr[FeChan.IP01], nullIndPtr[FeChan.IP01]);

                Ssutil.DbBindStringInput(hStmt, 0, "feerx", parameterValuePtr[FeChan.FEERX], (SQLULEN)pChan.feerx.Length, nullIndPtr[FeChan.FEERX]);

                Ssutil.DbBindStringInput(hStmt, 0, "notc", parameterValuePtr[FeChan.NOTC], (SQLULEN)pChan.notc.Length, nullIndPtr[FeChan.NOTC]);

                Ssutil.DbBindStringInput(hStmt, 0, "srvctx", parameterValuePtr[FeChan.SRVCTX], (SQLULEN)pChan.srvctx.Length, nullIndPtr[FeChan.SRVCTX]);

                Ssutil.DbBindStringInput(hStmt, 0, "srvcrx", parameterValuePtr[FeChan.SRVCRX], (SQLULEN)pChan.srvcrx.Length, nullIndPtr[FeChan.SRVCRX]);

                Ssutil.DbBindStringInput(hStmt, 0, "mdate", parameterValuePtr[FeChan.MDATE], (SQLULEN)pChan.mdate.Length, nullIndPtr[FeChan.MDATE]);

                Ssutil.DbBindStringInput(hStmt, 0, "mtime", parameterValuePtr[FeChan.MTIME], (SQLULEN)pChan.mtime.Length, nullIndPtr[FeChan.MTIME]);

            }
            catch (Exception e)
            {
                Log2.e("\n\nDynChan.FeInsertChan(): ERROR: a call to DbBindStringInput() failed.");
                Ssutil.DbGetDiagStmt(hStmt, "feInsertChan01 -- Error binding parameters for site on field: " + e.Message);
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_BINDING_FAILED;
            }

            //...Log2.v("nDynChan.FeInsertChan(): SQLExecDirect():\r\n" + cSQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            //...Log2.v("\r\nDynChan.FeInsertChan(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynChan.FeInsertChan(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                string str = "feInsertChan02 -- Error inserting chan";
                Ssutil.DbGetDiagStmt(hStmt, str); ;
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //...Log2.v("\n\nDynChan.FeInsertChan(): successfuly inserted record into table: " + cursors[curHandle].tableName);

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynChan.FeInsertChan(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method deletes an ES channel record from the database.
        /// </summary>
        /// <param name="curHandle"> - a prescribed Cursor object, as returned by a previous call to FeSelectChan().</param>
        /// <returns></returns>
        public static int FeDeleteChan(int curHandle)
        {
            //...Log2.v("\n\nDynFeChan.FeDeleteChan(): Entry");

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

            string delete_buf = String.Format("delete from {0} where location='{1}' and call1='{2}' and chid='{3}'",
                cursors[curHandle].tableName,
                cursors[curHandle].cCurrentLoc,
                cursors[curHandle].cCurrentCall1,
                cursors[curHandle].cCurrentChid
                );

            //...Log2.v("\nDynFeChan.FeDeleteChan(): query = " + delete_buf);
            sqlRet = ODBC.SQLExecDirect(hUpdate, delete_buf, delete_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = String.Format("feDeleteChan01 -- Error deleting from {0}",
                                            cursors[curHandle].tableName);
                Ssutil.DbGetDiagStmt(hUpdate, str);
                Ssutil.DisConnStmt(hConn, hUpdate);
                Log2.e("\nDynFeChan.FeDeleteChan(): ERROR: SQLExecDirect() failed.");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            Ssutil.DisConnStmt(hConn, hUpdate);

            //...Log2.v("\n\nDynFeChan.FeDeleteChan(): Exit");
            return 0;
        }

        private const string UPDATE = "update {0} set cmd= ?, recstat= ?, location= ?, call1= ?, chid= ?, freqtx= ?, poltx= ?, maxtxpower= ?, pwrtx= ?, p4khz= ?, eqpttx= ?, traftx= ?, stattx= ?, feetx= ?, freqrx= ?, polrx= ?, pwrrx= ?, eqptrx= ?, trafrx= ?, statrx= ?, i20= ?, it01= ?, ip01= ?, feerx= ?, notc= ?, srvctx= ?, srvcrx= ?, mdate= ?, mtime= ?  where location='{1}' and call1='{2}' and chid='{3}' ";

        /// <summary>
        /// This method updates all of the column values of a record in a database ES channel table.
        /// </summary>
        /// <param name="nCursor"> - a prescribed Cursor object, as returned by a previous call to FeSelectChan().</param>
        /// <param name="feChan"> - a prescribed FeChan object that provides the updated field values.</param>
        /// <param name="nullInd"> - an array of ODBC nullInds associated with feChan.</param>
        /// <returns></returns>
        public static int FeUpdateChan(int nCursor, FeChan feChan, SQLLEN[] nullInd)
        {
            string update_buf;

            SQLRETURN sqlRet;
            SQLHANDLE hConn = Ssutil.NewConn();
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            // Check the cursor for any nonsense.
            if (!cursors[nCursor].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Log2.e("\nDynFeChan.FeUpdateChan(): ERROR: Cursor object has cursorOpen = false.");
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_CUR_NOT_OPEN);
            }
            if (cursors[nCursor].pastLastRow)
            {
                /* Cursor is past the last row */
                Log2.e("\nDynFeChan.FeUpdateChan(): ERROR: Cursor object has pastLastRow = true.");
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_PAST_LAST_ROW);
            }

            // Prepare the update statement
            update_buf = String.Format(UPDATE, cursors[nCursor].tableName, cursors[nCursor].cCurrentLoc, cursors[nCursor].cCurrentCall1, cursors[nCursor].cCurrentChid);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pChan
            //into global memory with an SQLPOINTER pointer assigned to each one. FeChan provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = feChan.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                // Use auto-indexing of columns.
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(hUpdate, 0, "cmd", parameterValuePtr[FeChan.CMD], FeChan.CMD_SZ, nullIndPtr[FeChan.CMD]);
                Ssutil.DbBindStringInput(hUpdate, 0, "recstat", parameterValuePtr[FeChan.RECSTAT], FeChan.RECSTAT_SZ, nullIndPtr[FeChan.RECSTAT]);
                Ssutil.DbBindStringInput(hUpdate, 0, "location", parameterValuePtr[FeChan.LOCATION], FeChan.LOCATION_SZ, nullIndPtr[FeChan.LOCATION]);
                Ssutil.DbBindStringInput(hUpdate, 0, "call1", parameterValuePtr[FeChan.CALL1], FeChan.CALL1_SZ, nullIndPtr[FeChan.CALL1]);
                Ssutil.DbBindStringInput(hUpdate, 0, "chid", parameterValuePtr[FeChan.CHID], FeChan.CHID_SZ, nullIndPtr[FeChan.CHID]);
                Ssutil.DbBindDoubleInput(hUpdate, 0, "freqtx", parameterValuePtr[FeChan.FREQTX], nullIndPtr[FeChan.FREQTX]);
                Ssutil.DbBindStringInput(hUpdate, 0, "poltx", parameterValuePtr[FeChan.POLTX], FeChan.POLTX_SZ, nullIndPtr[FeChan.POLTX]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "maxtxpower", parameterValuePtr[FeChan.MAXTXPOWER], nullIndPtr[FeChan.MAXTXPOWER]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "pwrtx", parameterValuePtr[FeChan.PWRTX], nullIndPtr[FeChan.PWRTX]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "p4khz", parameterValuePtr[FeChan.P4KHZ], nullIndPtr[FeChan.P4KHZ]);
                Ssutil.DbBindStringInput(hUpdate, 0, "eqpttx", parameterValuePtr[FeChan.EQPTTX], FeChan.EQPTTX_SZ, nullIndPtr[FeChan.EQPTTX]);
                Ssutil.DbBindStringInput(hUpdate, 0, "traftx", parameterValuePtr[FeChan.TRAFTX], FeChan.TRAFTX_SZ, nullIndPtr[FeChan.TRAFTX]);
                Ssutil.DbBindStringInput(hUpdate, 0, "stattx", parameterValuePtr[FeChan.STATTX], FeChan.STATTX_SZ, nullIndPtr[FeChan.STATTX]);
                Ssutil.DbBindStringInput(hUpdate, 0, "feetx", parameterValuePtr[FeChan.FEETX], FeChan.FEETX_SZ, nullIndPtr[FeChan.FEETX]);
                Ssutil.DbBindDoubleInput(hUpdate, 0, "freqrx", parameterValuePtr[FeChan.FREQRX], nullIndPtr[FeChan.FREQRX]);
                Ssutil.DbBindStringInput(hUpdate, 0, "polrx", parameterValuePtr[FeChan.POLRX], FeChan.POLRX_SZ, nullIndPtr[FeChan.POLRX]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "pwrrx", parameterValuePtr[FeChan.PWRRX], nullIndPtr[FeChan.PWRRX]);
                Ssutil.DbBindStringInput(hUpdate, 0, "eqptrx", parameterValuePtr[FeChan.EQPTRX], FeChan.EQPTRX_SZ, nullIndPtr[FeChan.EQPTRX]);
                Ssutil.DbBindStringInput(hUpdate, 0, "trafrx", parameterValuePtr[FeChan.TRAFRX], FeChan.TRAFRX_SZ, nullIndPtr[FeChan.TRAFRX]);
                Ssutil.DbBindStringInput(hUpdate, 0, "statrx", parameterValuePtr[FeChan.STATRX], FeChan.STATRX_SZ, nullIndPtr[FeChan.STATRX]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "i20", parameterValuePtr[FeChan.I20], nullIndPtr[FeChan.I20]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "it01", parameterValuePtr[FeChan.IT01], nullIndPtr[FeChan.IT01]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "ip01", parameterValuePtr[FeChan.IP01], nullIndPtr[FeChan.IP01]);
                Ssutil.DbBindStringInput(hUpdate, 0, "feerx", parameterValuePtr[FeChan.FEERX], FeChan.FEERX_SZ, nullIndPtr[FeChan.FEERX]);
                Ssutil.DbBindStringInput(hUpdate, 0, "notc", parameterValuePtr[FeChan.NOTC], FeChan.NOTC_SZ, nullIndPtr[FeChan.NOTC]);
                Ssutil.DbBindStringInput(hUpdate, 0, "srvctx", parameterValuePtr[FeChan.SRVCTX], FeChan.SRVCTX_SZ, nullIndPtr[FeChan.SRVCTX]);
                Ssutil.DbBindStringInput(hUpdate, 0, "srvcrx", parameterValuePtr[FeChan.SRVCRX], FeChan.SRVCRX_SZ, nullIndPtr[FeChan.SRVCRX]);
                Ssutil.DbBindStringInput(hUpdate, 0, "mdate", parameterValuePtr[FeChan.MDATE], FeChan.MDATE_SZ, nullIndPtr[FeChan.MDATE]);
                Ssutil.DbBindStringInput(hUpdate, 0, "mtime", parameterValuePtr[FeChan.MTIME], FeChan.MTIME_SZ, nullIndPtr[FeChan.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\nDynFeChan.FeUpdateChan(): ERROR: ODBC Bind attempt failed: " + e.Message);
                Ssutil.DbGetDiagStmt(hUpdate, "feUpdateChan03 -- Error binding parameters for channel on field: " + e.Message);
                return -3;
            }

            //...Log2.v("\nDynFeChan.FeUpdateChan(): query = " + update_buf);
            sqlRet = ODBC.SQLExecDirect(hUpdate, update_buf, update_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = String.Format("\nDynFeChan.FeUpdateChan(): ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\n{1}", sqlRet, update_buf);
                Log2.e(str);
                str = String.Format("feUpdateChan04 -- Error writing Channel: {0} {1} {2} ",
                              feChan.location, feChan.call1, feChan.chid);
                Ssutil.DbGetDiagStmt(hUpdate, str);
                return -4;
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);
            Ssutil.DisConn(hConn);

            return (0);
        }






    }
}
