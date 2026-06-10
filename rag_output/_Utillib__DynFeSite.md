# Documented File: DynFeSite.cs
**Repository Path:** `_Utillib\DynFeSite.cs`
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

    public class DynFeSite
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
                    Application.Exit("\r\nDynFeSite.GetNextFreeCursor(): ERROR: No available cursors.");
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
        /// Selects site records from a site table in the database for
        /// the prescribed site table name, SQL search criteria and ordering clauses; the 
        /// method returns an index to a FeCursor object that can be used by subsequent
        /// calls to FeFetchSite().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for FeFetchSite() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECUTE_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int FeSelectSite(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynFeSite.FeSelectSite(): Entry");
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the select clause.
            string stmt_buf = "select " + FeSite.AllColumnsForSqlSelect + " from " + table;

            //Construct the 'where' part of the select clause.
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                stmt_buf += " where " + searchCriteria;
            }

            //If "order by" is not specified then assume it's for update.
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                // Order by was specified.
                //strcat_s(stmt_buf, sizeof(stmt_buf), " order by ");
                //strcat_s(stmt_buf, sizeof(stmt_buf), orderBy);
                stmt_buf += " order by " + orderBy;
            }

            //Get next free cursor area.
            curHandle = GetNextFreeCursor();

            //Make an new connection with the DB.
            SQLHDBC hConn = Ssutil.NewConn();

            //Get a statement handle.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            //Set the statement's attributes.
            sqlRet = ODBC.SQLSetStmtAttr(hStmt, ODBC.SQL_ATTR_CURSOR_TYPE, (SQLPOINTER)ODBC.SQL_CURSOR_DYNAMIC, 0);

            sqlRet = ODBC.SQLSetStmtAttr(hStmt, ODBC.SQL_ATTR_CONCURRENCY, (SQLPOINTER)ODBC.SQL_CONCUR_VALUES, 0);

            //...Log2.v("\nDynFeSite.FeSelectSite(): query = " + stmt_buf);
            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, stmt_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.w("DynFeSite.FeSelectSite(): Could not Select Site for criteria: " + searchCriteria);

                Ssutil.DbGetDiagStmt(hStmt, "Could not Select Site for criteria: " + searchCriteria + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);

                cursors[curHandle].hStmt = IntPtr.Zero;
                cursors[curHandle].cursorOpen = false;

                return Error.ODBC_EXECUTE_FAILED;
            }

            //	Populate the cursor handle structure
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].tableName = table;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cCurrentLoc = "";
            cursors[curHandle].sqlQuery = stmt_buf;

            //...Log2.v("\n\nDynFeSite.FeSelectSite(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Retrieves a single row of site data from a table in the database using 
        /// a previously-created Cursor object.
        /// </summary>
        /// <param name="curHandle"> - index of a Cursor object.</param>
        /// <param name="siteRec"> - a FeSite object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for siteRec.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - feCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int FeFetchSite(int curHandle, out FeSite siteRec, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynFeSite.feFetchSite(): Entry");

            //Create an FeSite object to output.
            siteRec = new FeSite();
            //Create an array of nulls to output.
            nullInd = new SQLLEN[FeSite.NUM_COLUMNS];

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                //...Log2.v("\r\nDynFeSite.feFetchSite(): FAIL: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // memset(siteRec, 0, sizeof(struct ftSite_));		/* clear to be safe */
            siteRec.Initialize();

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\r\nDynFeSite.feFetchSite(): ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\nDynFeSite.feFetchSite(): ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return Constant.FAILURE;
                }
            }

            // Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "cmd", out siteRec.cmd, Constant.CMD_SZ, out nullInd[FeSite.CMD]);
                Ssutil.DbGetString(hStmt, 2, "recstat", out siteRec.recstat, Constant.RECSTAT_SZ, out nullInd[FeSite.RECSTAT]);
                Ssutil.DbGetString(hStmt, 3, "location", out siteRec.location, Constant.LOCATION_SZ, out nullInd[FeSite.LOCATION]);
                Ssutil.DbGetString(hStmt, 4, "name", out siteRec.name, Constant.FE_SITE_NAME_SZ, out nullInd[FeSite.NAME]);
                Ssutil.DbGetString(hStmt, 5, "prov", out siteRec.prov, Constant.FE_SITE_PROV_SZ, out nullInd[FeSite.PROV]);
                Ssutil.DbGetString(hStmt, 6, "oper", out siteRec.oper, Constant.FE_SITE_OPER_SZ, out nullInd[FeSite.OPER]);
                Ssutil.DbGetInt(hStmt, 7, "latit", out siteRec.latit, out nullInd[FeSite.LATIT]);
                Ssutil.DbGetInt(hStmt, 8, "longit", out siteRec.longit, out nullInd[FeSite.LONGIT]);
                Ssutil.DbGetFloat(hStmt, 9, "grnd", out siteRec.grnd, out nullInd[FeSite.GRND]);
                Ssutil.DbGetString(hStmt, 10, "radio", out siteRec.radio, Constant.FE_SITE_RADIO_SZ, out nullInd[FeSite.RADIO]);
                Ssutil.DbGetShort(hStmt, 11, "rain", out siteRec.rain, out nullInd[FeSite.RAIN]);
                Ssutil.DbGetString(hStmt, 12, "sdate", out siteRec.sdate, Constant.FE_SITE_SDATE_SZ, out nullInd[FeSite.SDATE]);
                Ssutil.DbGetString(hStmt, 13, "stats", out siteRec.stats, Constant.FE_SITE_STATS_SZ, out nullInd[FeSite.STATS]);
                Ssutil.DbGetString(hStmt, 14, "nots", out siteRec.nots, Constant.FE_SITE_NOTS_SZ, out nullInd[FeSite.NOTS]);
                Ssutil.DbGetString(hStmt, 15, "oprtyp", out siteRec.oprtyp, Constant.FE_SITE_OPRTYP_SZ, out nullInd[FeSite.OPRTYP]);
                Ssutil.DbGetString(hStmt, 16, "reg", out siteRec.reg, Constant.FE_SITE_REG_SZ, out nullInd[FeSite.REG]);
                Ssutil.DbGetString(hStmt, 17, "mdate", out siteRec.mdate, Constant.DATE_SZ, out nullInd[FeSite.MDATE]);
                Ssutil.DbGetString(hStmt, 18, "mtime ", out siteRec.mtime, Constant.TIME_SZ, out nullInd[FeSite.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\r\nDynFeSite.FeFetchSite(): ERROR: Exception caught: " + e.Message);
                GenUtil.SetErr("feFetchSite02: Input error on field: " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\n\nDynFeSite.FeFetchSite():\n" + siteRec.ToStringWN(nullInd));

            // Save the call sign for use in deletes..
            cursors[curHandle].cCurrentLoc = siteRec.location;

            //...Log2.v("\n\nDynFeSite.feFetchSite(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Closes (releases) a cursor associated with a PDF ES site record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int FeCloseSite(int curHandle)
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
        /// Inserts a FeSite record into the database using column values prescribed 
        /// by the fields of the FeSite object.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object that encapsulates the details of the record insertion.</param>
        /// <param name="pSite"> - a FeSite object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pSite</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int FeInsertSite(int curHandle, FeSite pSite, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynFeSite.FeInsertSite(): Entry");

            string cSQL;
            SQLRETURN sqlRet;

            SQLHANDLE hStmt;

            SQLHANDLE hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynFeSite.FeInsertSite(): ERROR: call to SQLAllocHandle() failed.");
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
            cSQL = FeSite.BuildSqlInsertString(cursors[curHandle].tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pSite
            //into global memory with an SQLPOINTER pointer assigned to each one. FeSite provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = pSite.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                // Use automatic bind parameter indexing.
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(hStmt, 0, "cmd", parameterValuePtr[FeSite.CMD], (SQLULEN)pSite.cmd.Length, nullIndPtr[FeSite.CMD]);

                Ssutil.DbBindStringInput(hStmt, 0, "recstat", parameterValuePtr[FeSite.RECSTAT], (SQLULEN)pSite.recstat.Length, nullIndPtr[FeSite.RECSTAT]);

                Ssutil.DbBindStringInput(hStmt, 0, "location", parameterValuePtr[FeSite.LOCATION], (SQLULEN)pSite.location.Length, nullIndPtr[FeSite.LOCATION]);

                Ssutil.DbBindStringInput(hStmt, 0, "name", parameterValuePtr[FeSite.NAME], (SQLULEN)pSite.name.Length, nullIndPtr[FeSite.NAME]);

                Ssutil.DbBindStringInput(hStmt, 0, "prov", parameterValuePtr[FeSite.PROV], (SQLULEN)pSite.prov.Length, nullIndPtr[FeSite.PROV]);

                Ssutil.DbBindStringInput(hStmt, 0, "oper", parameterValuePtr[FeSite.OPER], (SQLULEN)pSite.oper.Length, nullIndPtr[FeSite.OPER]);

                Ssutil.DbBindIntInput(hStmt, 0, "latit", parameterValuePtr[FeSite.LATIT], nullIndPtr[FeSite.LATIT]);

                Ssutil.DbBindIntInput(hStmt, 0, "longit", parameterValuePtr[FeSite.LONGIT], nullIndPtr[FeSite.LONGIT]);

                Ssutil.DbBindFloatInput(hStmt, 0, "grnd", parameterValuePtr[FeSite.GRND], nullIndPtr[FeSite.GRND]);

                Ssutil.DbBindStringInput(hStmt, 0, "radio", parameterValuePtr[FeSite.RADIO], (SQLULEN)pSite.radio.Length, nullIndPtr[FeSite.RADIO]);

                Ssutil.DbBindShortInput(hStmt, 0, "rain", parameterValuePtr[FeSite.RAIN], nullIndPtr[FeSite.RAIN]);

                Ssutil.DbBindStringInput(hStmt, 0, "sdate", parameterValuePtr[FeSite.SDATE], (SQLULEN)pSite.sdate.Length, nullIndPtr[FeSite.SDATE]);

                Ssutil.DbBindStringInput(hStmt, 0, "stats", parameterValuePtr[FeSite.STATS], (SQLULEN)pSite.stats.Length, nullIndPtr[FeSite.STATS]);

                Ssutil.DbBindStringInput(hStmt, 0, "nots", parameterValuePtr[FeSite.NOTS], (SQLULEN)pSite.nots.Length, nullIndPtr[FeSite.NOTS]);

                Ssutil.DbBindStringInput(hStmt, 0, "oprtyp", parameterValuePtr[FeSite.OPRTYP], (SQLULEN)pSite.oprtyp.Length, nullIndPtr[FeSite.OPRTYP]);

                Ssutil.DbBindStringInput(hStmt, 0, "reg", parameterValuePtr[FeSite.REG], (SQLULEN)pSite.reg.Length, nullIndPtr[FeSite.REG]);

                Ssutil.DbBindStringInput(hStmt, 0, "mdate", parameterValuePtr[FeSite.MDATE], (SQLULEN)pSite.mdate.Length, nullIndPtr[FeSite.MDATE]);

                Ssutil.DbBindStringInput(hStmt, 0, "mtime", parameterValuePtr[FeSite.MTIME], (SQLULEN)pSite.mtime.Length, nullIndPtr[FeSite.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\n\nDynFeSite.FeInsertSite(): ERROR: a call to DbBindStringInput() failed.");
                Ssutil.DbGetDiagStmt(hStmt, "feInsertSite01 -- Error binding parameters for site on field: " + e.Message);
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_BINDING_FAILED;
            }

            //...Log2.v("nDynFeSite.FeInsertSite(): SQLExecDirect():\r\n" + cSQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            //...Log2.v("\r\nDynFeSite.FeInsertSite(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynFeSite.FeInsertSite(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                string str = "feInsertSite02 -- Error inserting site";
                Ssutil.DbGetDiagStmt(hStmt, str); ;
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_EXECUTE_FAILED;
            }

            //...Log2.v("\n\nDynFeSite.FeInsertSite(): successfuly inserted record into table: " + cursors[curHandle].tableName);

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynFeSite.FeInsertSite(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method deletes an ES site record from the database.
        /// </summary>
        /// <param name="curHandle"> - a prescribed Cursor object, as returned by a previous call to FeSelectSite().</param>
        /// <returns></returns>
        public static int FeDeleteSite(int curHandle)
        {
            //...Log2.v("\n\nDynFeSite.FeDeleteAnte(): Entry");

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

            string delete_buf = String.Format("delete from {0} where location='{1}'",
                cursors[curHandle].tableName,
                cursors[curHandle].cCurrentLoc
                );

            //...Log2.v("\nDynFeSite.FeDeleteSite(): query = " + delete_buf);
            sqlRet = ODBC.SQLExecDirect(hUpdate, delete_buf, delete_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = String.Format("feDeleteSite01 -- Error deleting [{0}] from {1}",
                      cursors[curHandle].cCurrentLoc, cursors[curHandle].tableName);
                Ssutil.DbGetDiagStmt(hUpdate, str);
                Ssutil.DisConnStmt(hConn, hUpdate);
                Log2.e("\nDynFeSite.FeDeleteSite(): ERROR: SQLExecDirect() failed.");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            Ssutil.DisConnStmt(hConn, hUpdate);

            //...Log2.v("\n\nDynFeSite.FtDeleteAnte(): Exit");
            return 0;
        }

        private const string UPDATE = "update {0} set cmd= ?, recstat= ?, location= ?, name= ?, prov= ?, oper= ?, latit= ?, longit= ?, grnd= ?, radio= ?, rain= ?, sdate= ?, stats= ?, nots= ?, oprtyp= ?, reg= ?, mdate= ?, mtime= ?  where location='{1}'";

        /// <summary>
        /// This method updates all of the column values of a record in a database ES site table.
        /// </summary>
        /// <param name="nCursor"> - a prescribed Cursor object, as returned by a previous call to FeSelectSite().</param>
        /// <param name="feSite"> - a prescribed FeSite object that provides the updated field values.</param>
        /// <param name="nullInd"> - an array of ODBC nullInds associated with feSite.</param>
        /// <returns></returns>
        public static int FeUpdateSite(int nCursor, FeSite feSite, SQLLEN[] nullInd)
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
                Log2.e("\nDynFeSite.FeUpdateSite(): ERROR: Cursor object has cursorOpen = false.");
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_CUR_NOT_OPEN);
            }
            if (cursors[nCursor].pastLastRow)
            {
                /* Cursor is past the last row */
                Log2.e("\nDynFeSite.FeUpdateSite(): ERROR: Cursor object has pastLastRow = true.");
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_PAST_LAST_ROW);
            }

            // Prepare the update statement
            update_buf = String.Format(UPDATE, cursors[nCursor].tableName, cursors[nCursor].cCurrentLoc);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pSite
            //into global memory with an SQLPOINTER pointer assigned to each one. FtSite provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = feSite.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                // Use auto-indexing of columns.
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(hUpdate, 0, "cmd", parameterValuePtr[FeSite.CMD], FeSite.CMD_SZ, nullIndPtr[FeSite.CMD]);

                Ssutil.DbBindStringInput(hUpdate, 0, "recstat", parameterValuePtr[FeSite.RECSTAT], FeSite.RECSTAT_SZ, nullIndPtr[FeSite.RECSTAT]);

                Ssutil.DbBindStringInput(hUpdate, 0, "location", parameterValuePtr[FeSite.LOCATION], FeSite.LOCATION_SZ, nullIndPtr[FeSite.LOCATION]);

                Ssutil.DbBindStringInput(hUpdate, 0, "name", parameterValuePtr[FeSite.NAME], FeSite.NAME_SZ, nullIndPtr[FeSite.NAME]);

                Ssutil.DbBindStringInput(hUpdate, 0, "prov", parameterValuePtr[FeSite.PROV], FeSite.PROV_SZ, nullIndPtr[FeSite.PROV]);

                Ssutil.DbBindStringInput(hUpdate, 0, "oper", parameterValuePtr[FeSite.OPER], FeSite.OPER_SZ, nullIndPtr[FeSite.OPER]);

                Ssutil.DbBindIntInput(hUpdate, 0, "latit", parameterValuePtr[FeSite.LATIT], nullIndPtr[FeSite.LATIT]);

                Ssutil.DbBindIntInput(hUpdate, 0, "longit", parameterValuePtr[FeSite.LONGIT], nullIndPtr[FeSite.LONGIT]);

                Ssutil.DbBindFloatInput(hUpdate, 0, "grnd", parameterValuePtr[FeSite.GRND], nullIndPtr[FeSite.GRND]);

                Ssutil.DbBindStringInput(hUpdate, 0, "radio", parameterValuePtr[FeSite.RADIO], FeSite.RADIO_SZ, nullIndPtr[FeSite.RADIO]);

                Ssutil.DbBindShortInput(hUpdate, 0, "rain", parameterValuePtr[FeSite.RAIN], nullIndPtr[FeSite.RAIN]);

                Ssutil.DbBindStringInput(hUpdate, 0, "sdate", parameterValuePtr[FeSite.SDATE], FeSite.SDATE_SZ, nullIndPtr[FeSite.SDATE]);

                Ssutil.DbBindStringInput(hUpdate, 0, "stats", parameterValuePtr[FeSite.STATS], FeSite.STATS_SZ, nullIndPtr[FeSite.STATS]);

                Ssutil.DbBindStringInput(hUpdate, 0, "nots", parameterValuePtr[FeSite.NOTS], FeSite.NOTS_SZ, nullIndPtr[FeSite.NOTS]);

                Ssutil.DbBindStringInput(hUpdate, 0, "oprtyp", parameterValuePtr[FeSite.OPRTYP], FeSite.OPRTYP_SZ, nullIndPtr[FeSite.OPRTYP]);

                Ssutil.DbBindStringInput(hUpdate, 0, "reg", parameterValuePtr[FeSite.REG], FeSite.REG_SZ, nullIndPtr[FeSite.REG]);

                Ssutil.DbBindStringInput(hUpdate, 0, "mdate", parameterValuePtr[FeSite.MDATE], FeSite.MDATE_SZ, nullIndPtr[FeSite.MDATE]);

                Ssutil.DbBindStringInput(hUpdate, 0, "mtime	", parameterValuePtr[FeSite.MTIME], FeSite.MTIME_SZ, nullIndPtr[FeSite.MTIME]);

            }
            catch (Exception e)
            {
                Log2.e("\nDynFeSite.FeUpdateSite(): ERROR: ODBC Bind attempt failed: " + e.Message);
                Ssutil.DbGetDiagStmt(hUpdate, "dynSite03 -- Error binding parameters for site on field: " + e.Message);
                return -3;
            }

            sqlRet = ODBC.SQLExecDirect(hUpdate, update_buf, update_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = String.Format("\nDynFeSite.FeUpdateSite(): ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\n{1}", sqlRet, update_buf);
                Log2.e(str);
                Ssutil.DbGetDiagStmt(hUpdate, "DynFeSite.FeUpdateSite04 -- Error writing site: " + feSite.location);
                return -4;
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);
            Ssutil.DisConn(hConn);

            return (0);
        }








    }
}

```
