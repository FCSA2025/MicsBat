# Documented File: DynFeAnte.cs
**Repository Path:** `_Utillib\DynFeAnte.cs`
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

    /// <summary>
    /// This class provides methods that serve FeAnte data to/from a database.
    /// </summary>
    public class DynFeAnte
    {
        //===================================================================================================================================
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;
        //===================================================================================================================================

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
                    Application.Exit("\r\nDynFeAnte.GetNextFreeCursor(): ERROR: No available cursors.");
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
        /// Selects antennae records from an antenna table in the database for
        /// the prescribed site table name, SQL search criteria and ordering clauses; the 
        /// method returns an index to a FeCursor object that can be used by subsequent
        /// calls to FeFetchAntenna().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for FeFetchAntenna() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int FeSelectAnte(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynFeAnte.FeSelectAnte(): Entry");
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the select clause.
            //sprintf_s(stmt_buf, sizeof(stmt_buf), select, table);
            string stmt_buf = "select " + FeAnte.AllColumnsForSqlSelect + " from " + table;

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
            else
            {
                // Order by was specified.
                //strcat_s(stmt_buf, sizeof(stmt_buf), " order by ");
                //strcat_s(stmt_buf, sizeof(stmt_buf), orderBy);
                stmt_buf += " order by " + orderBy;
            }

            //Get a Cursor object.
            curHandle = GetNextFreeCursor();

            //Make an new connection with the DB.
            SQLHDBC hConn = Ssutil.NewConn();

            //Get a statement handle.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            //...Log2.v("\r\nDynFeAnte.FeSelectAnte(): [1] sqlRet = " + sqlRet);

            //Set the statement's attributes.
            sqlRet = ODBC.SQLSetStmtAttr(hStmt, ODBC.SQL_ATTR_CURSOR_TYPE, (SQLPOINTER)ODBC.SQL_CURSOR_DYNAMIC, 0);
            //...Log2.v("\r\nDynFeAnte.FeSelectAnte(): [2] sqlRet = " + sqlRet);
            sqlRet = ODBC.SQLSetStmtAttr(hStmt, ODBC.SQL_ATTR_CONCURRENCY, (SQLPOINTER)ODBC.SQL_CONCUR_VALUES, 0);
            //...Log2.v("\r\nDynFeAnte.FeSelectAnte(): [3] sqlRet = " + sqlRet);

            //...Log2.v("\nDynFeAnte.FeSelectAnte(): query = " + stmt_buf);
            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, ODBC.SQL_NTS);
            //...Log2.v("\r\nDynFeAnte.FeSelectAnte(): [4] sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "Could not Select Antenna for criteria: " + searchCriteria + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                Log2.e("\nDynFeAnte.FeSelectAnte(): ERROR: SQLExecute() failed");
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
            cursors[curHandle].sqlQuery = stmt_buf;

            //...Log2.v("\n\nDynFeAnte.FeSelectAnte(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Retrieves a single row of antenna data from a table in the database using 
        /// a previously created Cursor object.
        /// </summary>
        /// <param name="curHandle"> - index of a Cursor object.</param>
        /// <param name="anteRec"> - a FeAnte object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for anteRec.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FeCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int FeFetchAnte(int curHandle, out FeAnte anteRec, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynFeAnte.FeFetchAntenna(): Entry:");

            //Create an FeAnte object to output.
            anteRec = new FeAnte();
            //Create an array of nulls to output.
            nullInd = new SQLLEN[FeAnte.NUM_COLUMNS];

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Log2.e("\n\nDynFeAnte.FeFetchAntenna(): Exit: FAIL: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            anteRec.Initialize();

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\n\nDynFeAnte.FeFetchAntenna(): Exit: ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\n\nDynFeAnte.FeFetchAntenna(): Exit: ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return Constant.FAILURE;
                }
            }

            // Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "cmd", out anteRec.cmd, Constant.CMD_SZ, out nullInd[FeAnte.CMD]);
                Ssutil.DbGetString(hStmt, 2, "recstat", out anteRec.recstat, Constant.RECSTAT_SZ, out nullInd[FeAnte.RECSTAT]);
                Ssutil.DbGetString(hStmt, 3, "location", out anteRec.location, Constant.LOCATION_SZ, out nullInd[FeAnte.LOCATION]);
                Ssutil.DbGetString(hStmt, 4, "call1", out anteRec.call1, Constant.CALLSIGN_SZ, out nullInd[FeAnte.CALL1]);
                Ssutil.DbGetString(hStmt, 5, "txband", out anteRec.txband, Constant.BNDCDE_SZ, out nullInd[FeAnte.TXBAND]);
                Ssutil.DbGetString(hStmt, 6, "rxband", out anteRec.rxband, Constant.BNDCDE_SZ, out nullInd[FeAnte.RXBAND]);
                Ssutil.DbGetString(hStmt, 7, "acodetx", out anteRec.acodetx, Constant.ACODE_SZ, out nullInd[FeAnte.ACODETX]);
                Ssutil.DbGetString(hStmt, 8, "acoderx", out anteRec.acoderx, Constant.ACODE_SZ, out nullInd[FeAnte.ACODERX]);
                Ssutil.DbGetFloat(hStmt, 9, "g_t", out anteRec.g_t, out nullInd[FeAnte.G_T]);
                Ssutil.DbGetFloat(hStmt, 10, "lnat", out anteRec.lnat, out nullInd[FeAnte.LNAT]);
                Ssutil.DbGetFloat(hStmt, 11, "aht", out anteRec.aht, out nullInd[FeAnte.AHT]);
                Ssutil.DbGetFloat(hStmt, 12, "afslt", out anteRec.afslt, out nullInd[FeAnte.AFSLT]);
                Ssutil.DbGetFloat(hStmt, 13, "afslr", out anteRec.afslr, out nullInd[FeAnte.AFSLR]);
                Ssutil.DbGetFloat(hStmt, 14, "txhgmax", out anteRec.txhgmax, out nullInd[FeAnte.TXHGMAX]);
                Ssutil.DbGetFloat(hStmt, 15, "rxhgmax", out anteRec.rxhgmax, out nullInd[FeAnte.RXHGMAX]);
                Ssutil.DbGetInt(hStmt, 16, "satlongit", out anteRec.satlongit, out nullInd[FeAnte.SATLONGIT]);
                Ssutil.DbGetFloat(hStmt, 17, "satlong", out anteRec.satlong, out nullInd[FeAnte.SATLONG]);
                Ssutil.DbGetString(hStmt, 18, "satlongs", out anteRec.satlongs, Constant.FE_ANTE_SATLONGS_SZ, out nullInd[FeAnte.SATLONGS]);
                Ssutil.DbGetFloat(hStmt, 19, "az", out anteRec.az, out nullInd[FeAnte.AZ]);
                Ssutil.DbGetFloat(hStmt, 20, "el", out anteRec.el, out nullInd[FeAnte.EL]);
                Ssutil.DbGetFloat(hStmt, 21, "sarc1", out anteRec.sarc1, out nullInd[FeAnte.SARC1]);
                Ssutil.DbGetFloat(hStmt, 22, "sarc2", out anteRec.sarc2, out nullInd[FeAnte.SARC2]);
                Ssutil.DbGetFloat(hStmt, 23, "rxpre", out anteRec.rxpre, out nullInd[FeAnte.RXPRE]);
                Ssutil.DbGetFloat(hStmt, 24, "txpre", out anteRec.txpre, out nullInd[FeAnte.TXPRE]);
                Ssutil.DbGetFloat(hStmt, 25, "rxtro", out anteRec.rxtro, out nullInd[FeAnte.RXTRO]);
                Ssutil.DbGetFloat(hStmt, 26, "txtro", out anteRec.txtro, out nullInd[FeAnte.TXTRO]);
                Ssutil.DbGetString(hStmt, 27, "licence", out anteRec.licence, Constant.LICENCE_SZ, out nullInd[FeAnte.LICENCE]);
                Ssutil.DbGetString(hStmt, 28, "satname", out anteRec.satname, Constant.FE_ANTE_SATNAME_SZ, out nullInd[FeAnte.SATNAME]);
                Ssutil.DbGetString(hStmt, 29, "stata", out anteRec.stata, Constant.FE_ANTE_STATA_SZ, out nullInd[FeAnte.STATA]);
                Ssutil.DbGetString(hStmt, 30, "nota", out anteRec.nota, Constant.NOTA_SZ, out nullInd[FeAnte.NOTA]);
                Ssutil.DbGetString(hStmt, 31, "op2", out anteRec.op2, Constant.FE_ANTE_OP2_SZ, out nullInd[FeAnte.OP2]);
                Ssutil.DbGetInt(hStmt, 32, "antref", out anteRec.antref, out nullInd[FeAnte.ANTREF]);
                Ssutil.DbGetString(hStmt, 33, "orbit", out anteRec.orbit, Constant.FE_ANTE_ORBIT_SZ, out nullInd[FeAnte.ORBIT]);
                Ssutil.DbGetString(hStmt, 34, "mdate", out anteRec.mdate, Constant.DATE_SZ, out nullInd[FeAnte.MDATE]);
                Ssutil.DbGetString(hStmt, 35, "mtime", out anteRec.mtime, Constant.TIME_SZ, out nullInd[FeAnte.MTIME]);

            }
            catch (Exception e)
            {
                Log2.e("\n\nDynFeAnte.FeFetchAntenna(): ERROR: Exception caught: " + e.Message);
                GenUtil.SetErr("dynfeAnte02 -- Could not get antenna, because of field " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\nDynFeAnte.FeFetchAntenna(): anteRec = \n" + anteRec.ToStringWN(nullInd));

            //	Save the key for deletes.
            cursors[curHandle].cCurrentLoc = anteRec.location;
            cursors[curHandle].cCurrentCall1 = anteRec.call1;

            //...Log2.v("\n\nDynFeAnte.FeFetchAntenna(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Closes (releases) a cursor associated with a PDF ES antenna record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int FeCloseAnte(int curHandle)
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
        /// Inserts a FeAnte record into the database using column values prescribed 
        /// by the fields of the FeAnte object.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object that encapsulates the details of the record insertion.</param>
        /// <param name="pAnte"> - a FeAnte object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pAnte</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int FeInsertAnte(int curHandle, FeAnte pAnte, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynAnte.FeInsertAnte(): Entry");

            string cSQL;
            SQLRETURN sqlRet;

            SQLHANDLE hStmt;

            SQLHANDLE hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynAnte.FeInsertAnte(): ERROR: call to SQLAllocHandle() failed.");
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
            cSQL = FeAnte.BuildSqlInsertString(cursors[curHandle].tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            //into global memory with an SQLPOINTER pointer assigned to each one. FeAnte provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = pAnte.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                // Use automatic bind parameter indexing.
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(hStmt, 0, "cmd", parameterValuePtr[FeAnte.CMD], (SQLULEN)pAnte.cmd.Length, nullIndPtr[FeAnte.CMD]);

                Ssutil.DbBindStringInput(hStmt, 0, "recstat", parameterValuePtr[FeAnte.RECSTAT], (SQLULEN)pAnte.recstat.Length, nullIndPtr[FeAnte.RECSTAT]);

                Ssutil.DbBindStringInput(hStmt, 0, "location", parameterValuePtr[FeAnte.LOCATION], (SQLULEN)pAnte.location.Length, nullIndPtr[FeAnte.LOCATION]);

                Ssutil.DbBindStringInput(hStmt, 0, "call1", parameterValuePtr[FeAnte.CALL1], (SQLULEN)pAnte.call1.Length, nullIndPtr[FeAnte.CALL1]);

                Ssutil.DbBindStringInput(hStmt, 0, "txband", parameterValuePtr[FeAnte.TXBAND], (SQLULEN)pAnte.txband.Length, nullIndPtr[FeAnte.TXBAND]);

                Ssutil.DbBindStringInput(hStmt, 0, "rxband", parameterValuePtr[FeAnte.RXBAND], (SQLULEN)pAnte.rxband.Length, nullIndPtr[FeAnte.RXBAND]);

                Ssutil.DbBindStringInput(hStmt, 0, "acodetx", parameterValuePtr[FeAnte.ACODETX], (SQLULEN)pAnte.acodetx.Length, nullIndPtr[FeAnte.ACODETX]);

                Ssutil.DbBindStringInput(hStmt, 0, "acoderx", parameterValuePtr[FeAnte.ACODERX], (SQLULEN)pAnte.acoderx.Length, nullIndPtr[FeAnte.ACODERX]);

                Ssutil.DbBindFloatInput(hStmt, 0, "g_t", parameterValuePtr[FeAnte.G_T], nullIndPtr[FeAnte.G_T]);

                Ssutil.DbBindFloatInput(hStmt, 0, "lnat", parameterValuePtr[FeAnte.LNAT], nullIndPtr[FeAnte.LNAT]);

                Ssutil.DbBindFloatInput(hStmt, 0, "aht", parameterValuePtr[FeAnte.AHT], nullIndPtr[FeAnte.AHT]);

                Ssutil.DbBindFloatInput(hStmt, 0, "afslt", parameterValuePtr[FeAnte.AFSLT], nullIndPtr[FeAnte.AFSLT]);

                Ssutil.DbBindFloatInput(hStmt, 0, "afslr", parameterValuePtr[FeAnte.AFSLR], nullIndPtr[FeAnte.AFSLR]);

                Ssutil.DbBindFloatInput(hStmt, 0, "txhgmax", parameterValuePtr[FeAnte.TXHGMAX], nullIndPtr[FeAnte.TXHGMAX]);

                Ssutil.DbBindFloatInput(hStmt, 0, "rxhgmax", parameterValuePtr[FeAnte.RXHGMAX], nullIndPtr[FeAnte.RXHGMAX]);

                Ssutil.DbBindIntInput(hStmt, 0, "satlongit", parameterValuePtr[FeAnte.SATLONGIT], nullIndPtr[FeAnte.SATLONGIT]);

                Ssutil.DbBindFloatInput(hStmt, 0, "satlong", parameterValuePtr[FeAnte.SATLONG], nullIndPtr[FeAnte.SATLONG]);

                Ssutil.DbBindStringInput(hStmt, 0, "satlongs", parameterValuePtr[FeAnte.SATLONGS], (SQLULEN)pAnte.satlongs.Length, nullIndPtr[FeAnte.SATLONGS]);

                Ssutil.DbBindFloatInput(hStmt, 0, "az", parameterValuePtr[FeAnte.AZ], nullIndPtr[FeAnte.AZ]);

                Ssutil.DbBindFloatInput(hStmt, 0, "el", parameterValuePtr[FeAnte.EL], nullIndPtr[FeAnte.EL]);

                Ssutil.DbBindFloatInput(hStmt, 0, "sarc1", parameterValuePtr[FeAnte.SARC1], nullIndPtr[FeAnte.SARC1]);

                Ssutil.DbBindFloatInput(hStmt, 0, "sarc2", parameterValuePtr[FeAnte.SARC2], nullIndPtr[FeAnte.SARC2]);

                Ssutil.DbBindFloatInput(hStmt, 0, "rxpre", parameterValuePtr[FeAnte.RXPRE], nullIndPtr[FeAnte.RXPRE]);

                Ssutil.DbBindFloatInput(hStmt, 0, "txpre", parameterValuePtr[FeAnte.TXPRE], nullIndPtr[FeAnte.TXPRE]);

                Ssutil.DbBindFloatInput(hStmt, 0, "rxtro", parameterValuePtr[FeAnte.RXTRO], nullIndPtr[FeAnte.RXTRO]);

                Ssutil.DbBindFloatInput(hStmt, 0, "txtro", parameterValuePtr[FeAnte.TXTRO], nullIndPtr[FeAnte.TXTRO]);

                Ssutil.DbBindStringInput(hStmt, 0, "licence", parameterValuePtr[FeAnte.LICENCE], (SQLULEN)pAnte.licence.Length, nullIndPtr[FeAnte.LICENCE]);

                Ssutil.DbBindStringInput(hStmt, 0, "satname", parameterValuePtr[FeAnte.SATNAME], (SQLULEN)pAnte.satname.Length, nullIndPtr[FeAnte.SATNAME]);

                Ssutil.DbBindStringInput(hStmt, 0, "stata", parameterValuePtr[FeAnte.STATA], (SQLULEN)pAnte.stata.Length, nullIndPtr[FeAnte.STATA]);

                Ssutil.DbBindStringInput(hStmt, 0, "nota", parameterValuePtr[FeAnte.NOTA], (SQLULEN)pAnte.nota.Length, nullIndPtr[FeAnte.NOTA]);

                Ssutil.DbBindStringInput(hStmt, 0, "op2", parameterValuePtr[FeAnte.OP2], (SQLULEN)pAnte.op2.Length, nullIndPtr[FeAnte.OP2]);

                Ssutil.DbBindIntInput(hStmt, 0, "antref", parameterValuePtr[FeAnte.ANTREF], nullIndPtr[FeAnte.ANTREF]);

                Ssutil.DbBindStringInput(hStmt, 0, "orbit", parameterValuePtr[FeAnte.ORBIT], (SQLULEN)pAnte.orbit.Length, nullIndPtr[FeAnte.ORBIT]);

                Ssutil.DbBindStringInput(hStmt, 0, "mdate", parameterValuePtr[FeAnte.MDATE], (SQLULEN)pAnte.mdate.Length, nullIndPtr[FeAnte.MDATE]);

                Ssutil.DbBindStringInput(hStmt, 0, "mtime", parameterValuePtr[FeAnte.MTIME], (SQLULEN)pAnte.mtime.Length, nullIndPtr[FeAnte.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\n\nDynAnte.FeInsertAnte(): ERROR: a call to DbBindStringInput() failed.");
                Ssutil.DbGetDiagStmt(hStmt, "feInsertAnte01 -- Error binding parameters for Ante on field: " + e.Message);
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_BINDING_FAILED;
            }

            //...Log2.v("nDynAnte.FeInsertAnte(): SQLExecDirect():\r\n" + cSQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            //...Log2.v("\r\nDynAnte.FeInsertAnte(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynAnte.FeInsertAnte(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                string str = "feInsertAnte02 -- Error inserting Ante";
                Ssutil.DbGetDiagStmt(hStmt, str); ;
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //...Log2.v("\n\nDynAnte.FeInsertAnte(): successfuly inserted record into table: " + cursors[curHandle].tableName);

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynAnte.FeInsertAnte(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method deletes an ES antenna record from the database.
        /// </summary>
        /// <param name="curHandle"> - a prescribed Cursor object, as returned by a previous call to FeSelectAnte().</param>
        /// <returns></returns>
        public static int FeDeleteAnte(int curHandle)
        {
            //...Log2.v("\n\nDynFeAnte.FeDeleteAnte(): Entry");

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
            // AH: there is a bug here that needs to get fixed: call1 not call21.
            // Original C++ : static char fmt_delete[] = "delete from %s ""where location='%s' and call1='%s' ";
            string delete_buf = String.Format("delete from {0} where location='{1}' and call21='{2}'",
                cursors[curHandle].tableName,
                cursors[curHandle].cCurrentLoc,
                cursors[curHandle].cCurrentCall1
                );

            //...Log2.v("\r\nDynFeAnte.FtDeleteAntenna(): query = " + delete_buf);
            sqlRet = ODBC.SQLExecDirect(hUpdate, delete_buf, delete_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hUpdate, "ftDeleteAnte01 -- Error deleting from " + cursors[curHandle].tableName);
                Ssutil.DisConnStmt(hConn, hUpdate);
                Log2.e("\nDynFeAnte.FeDeleteAnte(): ERROR: SQLExecDirect() failed.");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            Ssutil.DisConnStmt(hConn, hUpdate);

            //...Log2.v("\n\nDynFeAnte.FeDeleteAnte(): Exit");
            return 0;
        }

        private const string UPDATE = "update {0}  set cmd= ?, recstat= ?, location= ?, call1= ?, txband= ?,  rxband= ?, acodetx= ?, acoderx= ?, g_t= ?, lnat= ?,  aht= ?, afslt= ?, afslr= ?, txhgmax= ?,rxhgmax=?,  satlongit= ?, satlong= ?, satlongs= ?, az= ?, el= ?,  sarc1= ?, sarc2= ?, rxpre= ?, txpre= ?, rxtro= ?,  txtro= ?, licence= ?, satname= ?, stata= ?, nota= ?,  op2= ?, antref= ?, orbit= ?, mdate= ?, mtime= ?   where location='{1}' and call1='{2}'";

        /// <summary>
        /// This method updates all of the column values of a record in a database ES antenna table.
        /// </summary>
        /// <param name="nCursor"> - a prescribed Cursor object, as returned by a previous call to FeSelectAnte().</param>
        /// <param name="feAnte"> - a prescribed FeAnte object that provides the updated field values.</param>
        /// <param name="nullInd"> - an array of ODBC nullInds associated with feAnte.</param>
        /// <returns></returns>
        public static int FeUpdateAnte(int nCursor, FeAnte feAnte, SQLLEN[] nullInd)
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
                Log2.e("\nDynFeAnte.FeUpdateAnte(): ERROR: Cursor object has cursorOpen = false.");
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_CUR_NOT_OPEN);
            }
            if (cursors[nCursor].pastLastRow)
            {
                /* Cursor is past the last row */
                Log2.e("\nDynFeAnte.FeUpdateAnte(): ERROR: Cursor object has pastLastRow = true.");
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_PAST_LAST_ROW);
            }

            // Prepare the update statement
            update_buf = String.Format(UPDATE, cursors[nCursor].tableName, cursors[nCursor].cCurrentLoc, cursors[nCursor].cCurrentCall1);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            //into global memory with an SQLPOINTER pointer assigned to each one. FtAnte provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = feAnte.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                // Use auto-indexing of columns.
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(hUpdate, 0, "cmd", parameterValuePtr[FeAnte.CMD], FeAnte.CMD_SZ, nullIndPtr[FeAnte.CMD]);
                Ssutil.DbBindStringInput(hUpdate, 0, "recstat", parameterValuePtr[FeAnte.RECSTAT], FeAnte.RECSTAT_SZ, nullIndPtr[FeAnte.RECSTAT]);
                Ssutil.DbBindStringInput(hUpdate, 0, "location", parameterValuePtr[FeAnte.LOCATION], FeAnte.LOCATION_SZ, nullIndPtr[FeAnte.LOCATION]);
                Ssutil.DbBindStringInput(hUpdate, 0, "call1", parameterValuePtr[FeAnte.CALL1], FeAnte.CALL1_SZ, nullIndPtr[FeAnte.CALL1]);
                Ssutil.DbBindStringInput(hUpdate, 0, "txband", parameterValuePtr[FeAnte.TXBAND], FeAnte.TXBAND_SZ, nullIndPtr[FeAnte.TXBAND]);
                Ssutil.DbBindStringInput(hUpdate, 0, "rxband", parameterValuePtr[FeAnte.RXBAND], FeAnte.RXBAND_SZ, nullIndPtr[FeAnte.RXBAND]);
                Ssutil.DbBindStringInput(hUpdate, 0, "acodetx", parameterValuePtr[FeAnte.ACODETX], FeAnte.ACODETX_SZ, nullIndPtr[FeAnte.ACODETX]);
                Ssutil.DbBindStringInput(hUpdate, 0, "acoderx", parameterValuePtr[FeAnte.ACODERX], FeAnte.ACODERX_SZ, nullIndPtr[FeAnte.ACODERX]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "g_t", parameterValuePtr[FeAnte.G_T], nullIndPtr[FeAnte.G_T]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "lnat", parameterValuePtr[FeAnte.LNAT], nullIndPtr[FeAnte.LNAT]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "aht", parameterValuePtr[FeAnte.AHT], nullIndPtr[FeAnte.AHT]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "afslt", parameterValuePtr[FeAnte.AFSLT], nullIndPtr[FeAnte.AFSLT]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "afslr", parameterValuePtr[FeAnte.AFSLR], nullIndPtr[FeAnte.AFSLR]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "txhgmax", parameterValuePtr[FeAnte.TXHGMAX], nullIndPtr[FeAnte.TXHGMAX]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "rxhgmax", parameterValuePtr[FeAnte.RXHGMAX], nullIndPtr[FeAnte.RXHGMAX]);
                Ssutil.DbBindIntInput(hUpdate, 0, "satlongit", parameterValuePtr[FeAnte.SATLONGIT], nullIndPtr[FeAnte.SATLONGIT]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "satlong", parameterValuePtr[FeAnte.SATLONG], nullIndPtr[FeAnte.SATLONG]);
                Ssutil.DbBindStringInput(hUpdate, 0, "satlongs", parameterValuePtr[FeAnte.SATLONGS], FeAnte.SATLONGS_SZ, nullIndPtr[FeAnte.SATLONGS]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "az", parameterValuePtr[FeAnte.AZ], nullIndPtr[FeAnte.AZ]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "el", parameterValuePtr[FeAnte.EL], nullIndPtr[FeAnte.EL]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "sarc1", parameterValuePtr[FeAnte.SARC1], nullIndPtr[FeAnte.SARC1]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "sarc2", parameterValuePtr[FeAnte.SARC2], nullIndPtr[FeAnte.SARC2]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "rxpre", parameterValuePtr[FeAnte.RXPRE], nullIndPtr[FeAnte.RXPRE]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "txpre", parameterValuePtr[FeAnte.TXPRE], nullIndPtr[FeAnte.TXPRE]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "rxtro", parameterValuePtr[FeAnte.RXTRO], nullIndPtr[FeAnte.RXTRO]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "txtro", parameterValuePtr[FeAnte.TXTRO], nullIndPtr[FeAnte.TXTRO]);
                Ssutil.DbBindStringInput(hUpdate, 0, "licence", parameterValuePtr[FeAnte.LICENCE], FeAnte.LICENCE_SZ, nullIndPtr[FeAnte.LICENCE]);
                Ssutil.DbBindStringInput(hUpdate, 0, "satname", parameterValuePtr[FeAnte.SATNAME], FeAnte.SATNAME_SZ, nullIndPtr[FeAnte.SATNAME]);
                Ssutil.DbBindStringInput(hUpdate, 0, "stata", parameterValuePtr[FeAnte.STATA], FeAnte.STATA_SZ, nullIndPtr[FeAnte.STATA]);
                Ssutil.DbBindStringInput(hUpdate, 0, "nota", parameterValuePtr[FeAnte.NOTA], FeAnte.NOTA_SZ, nullIndPtr[FeAnte.NOTA]);
                Ssutil.DbBindStringInput(hUpdate, 0, "op2", parameterValuePtr[FeAnte.OP2], FeAnte.OP2_SZ, nullIndPtr[FeAnte.OP2]);
                Ssutil.DbBindIntInput(hUpdate, 0, "antref", parameterValuePtr[FeAnte.ANTREF], nullIndPtr[FeAnte.ANTREF]);
                Ssutil.DbBindStringInput(hUpdate, 0, "orbit", parameterValuePtr[FeAnte.ORBIT], FeAnte.ORBIT_SZ, nullIndPtr[FeAnte.ORBIT]);
                Ssutil.DbBindStringInput(hUpdate, 0, "mdate", parameterValuePtr[FeAnte.MDATE], FeAnte.MDATE_SZ, nullIndPtr[FeAnte.MDATE]);
                Ssutil.DbBindStringInput(hUpdate, 0, "mtime", parameterValuePtr[FeAnte.MTIME], FeAnte.MTIME_SZ, nullIndPtr[FeAnte.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\nDynFeAnte.FeUpdateAnte(): ERROR: ODBC Bind attempt failed: " + e.Message);
                Ssutil.DbGetDiagStmt(hUpdate, "dynAnte03 -- Error binding parameters for site on field: " + e.Message);
                return -3;
            }

            //...Log2.v("\nDynFeAnte.FeUpdateAnte(): query = " + update_buf);
            sqlRet = ODBC.SQLExecDirect(hUpdate, update_buf, update_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = String.Format("\nDynFeAnte.FeUpdateAnte(): ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\n{1}", sqlRet, update_buf);
                Log2.e(str);
                Ssutil.DbGetDiagStmt(hUpdate, "DynFeAnte.FeUpdateAnte04 -- Error writing site: " + feAnte.location);
                return -4;
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);
            Ssutil.DisConn(hConn);

            return (0);
        }






    }
}

```
