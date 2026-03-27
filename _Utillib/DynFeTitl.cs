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
    public class DynFeTitl
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
                    // There are no more open cursors
                    //GenUtil.SetErr("dynTitl01 -- No more open cursors.");
                    //return -1;
                    Application.Exit("\r\nDynFeTitl.GetNextFreeCursor(): ERROR: No available cursors.");
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
        /// Selects titl records from a '_titl' table in the database for
        /// the prescribed titl table name, SQL search criteria and ordering clauses; the 
        /// method returns an index to a Cursor object that can be used by subsequent
        /// calls to FeFetchTitle().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for FeFetchTitl() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECUTE_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int FeSelectTitl(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynFeTitl.FeSelectTitle(): Entry");
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the select clause.
            string cSQL = "select " + FeTitl.AllColumnsForSqlSelect + " from " + table;

            //Construct the 'where' part of the select clause.
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                cSQL += " where " + searchCriteria;
            }

            //If "order by" is not specified then assume it's for update.
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                // Order by was specified.
                //strcat_s(stmt_buf, sizeof(stmt_buf), " order by ");
                //strcat_s(stmt_buf, sizeof(stmt_buf), orderBy);
                cSQL += " order by " + orderBy;
            }

            //Get next free cursor area.
            curHandle = GetNextFreeCursor();

            //Make an new connection with the DB.
            SQLHDBC hConn = Ssutil.NewConn();

            //Get a statement handle.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            // Execute the SQL query.
            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("DynFeTitl.FeSelectTitle(): ERROR: call to SQLExecDirect() failed for: " + cSQL);

                Ssutil.DbGetDiagStmt(hStmt, "Could not Select Titl for criteria: " + searchCriteria + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);

                cursors[curHandle].hStmt = IntPtr.Zero;
                cursors[curHandle].cursorOpen = false;

                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //	Populate the cursor handle structure
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].tableName = table;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cCurrentLoc = "";
            cursors[curHandle].sqlQuery = cSQL;

            //...Log2.v("\n\nDynFeTitl.FeSelectTitle(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Closes (releases) a cursor associated with a PDF ES 'titl' record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int FeCloseTitl(int curHandle)
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
        /// Retrieves a single row of 'titl' data from a table in the database using 
        /// a previously-created Cursor object.
        /// </summary>
        /// <param name="curHandle"> - index of a Cursor object.</param>
        /// <param name="titlRec"> - a FeTitl object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for titlRec.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - feCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int FeFetchTitl(int curHandle, out FeTitl titlRec, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynFeTitl.feFetchTitl(): Entry");

            //Create an FeTitl object to output.
            titlRec = new FeTitl();
            //Create an array of nulls to output.
            nullInd = new SQLLEN[FeTitl.NUM_COLUMNS];

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                Log2.e("\r\nDynFeTitl.feFetchTitl(): ERROR: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\r\nDynFeTitl.feFetchTitl(): ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    //...Log2.v("\r\nDynFeTitl.feFetchTitl(): ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return Constant.FAILURE;
                }
            }

            // Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "validated", out titlRec.validated, FeTitl.VALIDATED_SZ, out nullInd[FeTitl.VALIDATED]);

                Ssutil.DbGetString(hStmt, 2, "namef", out titlRec.namef, FeTitl.NAMEF_SZ, out nullInd[FeTitl.NAMEF]);

                Ssutil.DbGetString(hStmt, 3, "source", out titlRec.source, FeTitl.SOURCE_SZ, out nullInd[FeTitl.SOURCE]);

                Ssutil.DbGetString(hStmt, 4, "descr", out titlRec.descr, FeTitl.DESCR_SZ, out nullInd[FeTitl.DESCR]);

                Ssutil.DbGetString(hStmt, 5, "mdate", out titlRec.mdate, FeTitl.MDATE_SZ, out nullInd[FeTitl.MDATE]);

                Ssutil.DbGetString(hStmt, 6, "mtime", out titlRec.mtime, FeTitl.MTIME_SZ, out nullInd[FeTitl.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\r\nDynFeTitl.FeFetchTitl(): ERROR: Exception caught: " + e.Message);
                GenUtil.SetErr("feFetchTitl02: Input error on field: " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\n\nDynFeTitl.feFetchTitl(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Inserts a 'titl' record into the database using column values prescribed 
        /// by the fields of the FeTitl object.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object that encapsulates the details of the record insertion.</param>
        /// <param name="pTitl"> - a FeTitl object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pTitl</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int FeInsertTitl(int curHandle, FeTitl pTitl, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynTitl.FeInsertTitl(): Entry");

            string cSQL;
            SQLRETURN sqlRet;

            SQLHANDLE hStmt;

            SQLHANDLE hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynTitl.FeInsertTitl(): ERROR: call to SQLAllocHandle() failed.");
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
            cSQL = FeTitl.BuildSqlInsertString(cursors[curHandle].tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pTitl
            //into global memory with an SQLPOINTER pointer assigned to each one. FeTitl provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = pTitl.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                // The SQL/ODBC defines that the first binding has index = 1;

                Ssutil.DbBindStringInput(hStmt, FeTitl.VALIDATED + 1, "validated", parameterValuePtr[FeTitl.VALIDATED], (SQLULEN)pTitl.validated.Length, nullIndPtr[FeTitl.VALIDATED]);

                Ssutil.DbBindStringInput(hStmt, FeTitl.NAMEF + 1, "namef", parameterValuePtr[FeTitl.NAMEF], (SQLULEN)pTitl.namef.Length, nullIndPtr[FeTitl.NAMEF]);

                Ssutil.DbBindStringInput(hStmt, FeTitl.SOURCE + 1, "source", parameterValuePtr[FeTitl.SOURCE], (SQLULEN)pTitl.source.Length, nullIndPtr[FeTitl.SOURCE]);

                Ssutil.DbBindStringInput(hStmt, FeTitl.DESCR + 1, "descr", parameterValuePtr[FeTitl.DESCR], (SQLULEN)pTitl.descr.Length, nullIndPtr[FeTitl.DESCR]);

                Ssutil.DbBindStringInput(hStmt, FeTitl.MDATE + 1, "mdate", parameterValuePtr[FeTitl.MDATE], (SQLULEN)pTitl.mdate.Length, nullIndPtr[FeTitl.MDATE]);

                Ssutil.DbBindStringInput(hStmt, FeTitl.MTIME + 1, "mtime", parameterValuePtr[FeTitl.MTIME], (SQLULEN)pTitl.mtime.Length, nullIndPtr[FeTitl.MTIME]);

            }
            catch (Exception e)
            {
                Log2.e("\n\nDynTitl.FeInsertTitl(): ERROR: a call to DbBindStringInput() failed: " + e.Message);
                Ssutil.DbGetDiagStmt(hStmt, "ftInsertTitl01 -- Error binding parameters for titl on field: " + e.Message);
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_BINDING_FAILED;
            }

            //...Log2.v("nDynTitl.FeInsertTitl(): SQLExecDirect():\r\n" + cSQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            //...Log2.v("\r\nDynTitl.FeInsertTitl(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynTitl.FeInsertTitl(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                string str = "feInsertTitl02 -- Error inserting title";
                Ssutil.DbGetDiagStmt(hStmt, str); ;
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_EXECUTE_FAILED;
            }

            //...Log2.v("\nDynTitl.FeInsertTitl(): successfuly inserted record into table: " + cursors[curHandle].tableName);

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynTitl.FeInsertTitl(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method updates all of the column values of a record in a database ES title table.
        /// </summary>
        /// <param name="curHandle"> - a prescribed Cursor object, as returned by a previous call to FeSelectTitl().</param>
        /// <param name="feTitl"> - a prescribed FeTitl object that provides the updated field values.</param>
        /// <param name="nullInd"> - an array of ODBC nullInds associated with feTitl.</param>
        /// <returns></returns>
        public static int FeUpdateTitl(int curHandle,		/* handle returned by 'feSelectTitl */
                                        FeTitl feTitl,        /* caller's struct for row of data */
                                        SQLLEN[] nullInd)   /* caller's array of null indicators */
        {
            FeDeleteTitl(curHandle);

            return FeInsertTitl(curHandle, feTitl, nullInd);
        }

        /// <summary>
        /// This method deletes an ES title record from the database.
        /// </summary>
        /// <param name="curHandle"> - a prescribed Cursor object, as returned by a previous call to FeSelectTitl().</param>
        /// <returns></returns>
        public static int FeDeleteTitl(int curHandle)
        {
            string delete_buf;
            SQLRETURN sqlRet;
            SQLHANDLE hConn = Ssutil.NewConn();
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            /* Prepare statement */
            delete_buf = String.Format("delete from {0} ", cursors[curHandle].tableName);

            sqlRet = ODBC.SQLExecDirect(hUpdate, delete_buf, delete_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hUpdate, "feDeleteTitl01 -- Error deleting from " + cursors[curHandle].tableName);
                return -1;
            }

            Ssutil.DisConnStmt(hConn, hUpdate);

            return 0;
        }


    }
}
