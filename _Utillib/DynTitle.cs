using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using _DataStructures;

namespace _Utillib
{
    using _Configuration;
    using _NewLib;
    using _Utillib;
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
    /// The methods in this class perform operations on a PDF TS TITLE table
    /// with canonical name <b>&lt;userID&gt;.ft_&lt;pdfName&gt;_titl</b>;	
    /// the supported operations are: selectTitle, fetchTitle,		
    /// updatechngCall, and closechngCall.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>The caller must first call selectTitle to select the required 	
    /// rows, this routine simply sets up the cursor for subsequent use.</item> 	
    /// <item>The method ftSelectTitle returns a 'cursorHandle' to the caller, 	
    /// this handle must be used in all subsequent calls to fetch, update etc</item>
    /// <item>The methods updateTitle, deleteTitle and insertTitle can   
    /// only be called after a row has been fetched. 	</item>		
    /// <item>The routine closeTitle must be called to release ODBC and cursor resources.	
    /// </list>
    /// </remarks>
    public class DynTitle
    {

        //===================================================================================================================================
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;
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
                    //There are no more open cursors
                    GenUtil.SetErr("dynSite01 -- No more open cursors.");
                    return Error.NO_CURSOR_AVAILABLE;
                    // Application.Exit("\r\nDynAntenna.GetNextFreeCursor(): ERROR: No available cursors.");
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

        //----------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Selects records from a TITLE table in the database 
        /// with canonical name <b>&lt;userID&gt;.ft_&lt;pdfName&gt;_titl</b> for
        /// the prescribed table name, SQL search criteria and ordering clauses; the 
        /// method returns an index to a cursor object that can be used by subsequent
        /// calls to FtFetchTitle().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for FtFetchChngCall() calls.</para>
        public static int FtSelectTitle(string table, string searchCriteria, string orderBy)
        {
            int curHandle;                  /* cursor handle */

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;

            StringBuilder sb = new StringBuilder();
            sb.Append(" select ");
            sb.Append(FtTitl.AllColumnsForSqlSelect);
            sb.Append(" from ");
            sb.Append(table);
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                sb.Append("where ");
                sb.Append(searchCriteria);
            }
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                sb.Append(" order by ");
                sb.Append(orderBy);
            }
            string cSQL = sb.ToString();

            //	Get next free cursor area
            curHandle = GetNextFreeCursor();

            //	allocate and open the handle
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            //	Inititialize the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].tableName = table;

            return curHandle;
        }

        /// <summary>
        /// Retrieves a single row of data from a PDF TS TITLE Table.
        /// </summary>
        /// <param name="nCursor"> - cursor object.</param>
        /// <param name="pTitle"> - a FtTitle object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pTitle.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int FtFetchTitle(int nCursor,		 //handle returned by 'ftSelectTitle'
                                        out FtTitl pTitle,  // caller's struct for row of data
                                        out SQLLEN[] nullInd)  // caller's array of null indicators
        {
            // Satisfy 'out' requirement.
            pTitle = new FtTitl();
            nullInd = NullHelper.CreateArrayOfNullInd(FtTitl.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[nCursor].hStmt;

            sqlRet = ODBC.SQLFetch(hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    return Constant.FAILURE;
                }
            }

            //	Now read in the fields
            try
            {
                int col = 1;
                sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, col, "validated", out pTitle.validated, Constant.FTTITLE_VALIDATED_SZ, out nullInd[col - 1]);
                col = 2;
                sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, col, "namef", out pTitle.namef, Constant.FTTITLE_NAMEF_SZ, out nullInd[col - 1]);
                col = 3;
                sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, col, "source", out pTitle.source, Constant.FTTITLE_SOURCE_SZ, out nullInd[col - 1]);
                col = 4;
                sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, col, "descr", out pTitle.descr, Constant.FTTITLE_DESCR_SZ, out nullInd[col - 1]);
                col = 5;
                sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, col, "mdate", out pTitle.mdate, Constant.DATE_SZ, out nullInd[col - 1]);
                col = 6;
                sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, col, "mtime", out pTitle.mtime, Constant.TIME_SZ, out nullInd[col - 1]);
            }
            catch (Exception e)
            {
                GenUtil.SetErr("dynSite02: Input error on field: " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            return Constant.SUCCESS;
        }

        /// <summary>
        /// Updates the fields in a PDF TS TITLE record to mirror those prescribed in
        /// a FtTitle object.
        /// </summary>
        /// <param name="nCursor">- index of the FtCursor object.</param>
        /// <param name="pTitle"> - FtTitle object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pTitle.</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - update attempt was successful.</para>
        /// <para>- ErrorMessages.ODBC_BINDING_ERROR - attempt to create an ODBC parameter binding failed.  </para>
        /// <para>- ErrorMessages.ODBC_QUERY_FAILED - update attempt failed - ODBC diagnostic information will be written to output.</para>
        public static int FtUpdateTitle(int nCursor,      //handle returned by 'ftSelectTitle'
                                        FtTitl pTitle,  // caller's struct for row of data
                                        SQLLEN[] nullInd)  // caller's array of null indicators
        {
            SQLRETURN sqlRet;
            SQLHANDLE hConn = Ssutil.NewConn();
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            /* Prepare the update statement */
            StringBuilder sb = new StringBuilder();
            sb.Append("update ");
            sb.Append(cursors[nCursor].tableName);
            sb.Append(" set ");
            sb.Append(FtTitl.AllColumnsForSqlUpdateAsBindings);
            string cSQL = sb.ToString();

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            //into global memory with an SQLPOINTER pointer assigned to each one. FtAnte provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = pTitle.CopyToArrayOfSQLPOINTERs();

            // Bind the parameters.
            try
            {
                int col = 1;
                Ssutil.DbBindStringInput(hUpdate, col, "validated", parameterValuePtr[col - 1], Constant.FTTITLE_VALIDATED_SZ, nullIndPtr[col - 1]);
                col = 2;
                Ssutil.DbBindStringInput(hUpdate, col, "namef", parameterValuePtr[col - 1], Constant.FTTITLE_NAMEF_SZ, nullIndPtr[col - 1]);
                col = 3;
                Ssutil.DbBindStringInput(hUpdate, col, "source", parameterValuePtr[col - 1], Constant.FTTITLE_SOURCE_SZ, nullIndPtr[col - 1]);
                col = 4;
                Ssutil.DbBindStringInput(hUpdate, col, "descr", parameterValuePtr[col - 1], Constant.FTTITLE_DESCR_SZ, nullIndPtr[col - 1]);
                col = 5;
                Ssutil.DbBindStringInput(hUpdate, col, "mdate", parameterValuePtr[col - 1], Constant.DATE_SZ, nullIndPtr[col - 1]);
                col = 6;
                Ssutil.DbBindStringInput(hUpdate, col, "mtime", parameterValuePtr[col - 1], Constant.TIME_SZ, nullIndPtr[col - 1]);
            }
            catch (Exception e)
            {

                Ssutil.DbGetDiagStmt(hUpdate, "ftUpdateTitle03 -- Error binding parameters for site on field: " + e.Message);
                return Error.ODBC_BINDING_FAILED;
            }

            sqlRet = ODBC.SQLExecDirect(hUpdate, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlRet))
            {

                Ssutil.DbGetDiagStmt(hUpdate, "ftUpdateTitle04 -- Error writing title.");
                return Error.ODBC_QUERY_FAILED;
            }

            Ssutil.DisConnStmt(hConn, hUpdate);

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Closes (releases) a cursor associated with a PDF TS TITLE record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the FtCursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        public static int FtCloseTitle(int curHandle)
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
        public static int FtInsertTitl(int curHandle, FtTitl pTitl, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynTitl.FtInsertTitl(): Entry");

            string cSQL;
            SQLRETURN sqlRet;

            SQLHANDLE hStmt;

            SQLHANDLE hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynFtTitl.FtInsertTitl(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Log2.e("\n\nDynFtTitl.FtInsertTitl(): ERROR: cursor not open.");
                Ssutil.DisConnStmt(hConn, hStmt);
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (cursors[curHandle].pastLastRow)
            {
                /* Cursor is past the last row */
                Log2.e("\n\nDynFtTitl.FtInsertTitl(): ERROR: cursor past the last row.");
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
                Log2.e("\n\nDynFtTitl.FtInsertTitl(): ERROR: a call to DbBindStringInput() failed: " + e.Message);
                Ssutil.DbGetDiagStmt(hStmt, "ftInsertTitle01 -- Error binding parameters for titl on field: " + e.Message);
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_BINDING_FAILED;
            }

            //...Log2.v("nDynTitl.FtInsertTitl(): SQLExecDirect():\r\n" + cSQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            //...Log2.v("\r\nDynTitl.FtInsertTitl(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynFtTitl.FtInsertTitl(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                Log2.e("\n" + ODBC.GetDiagnostics(hStmt, cSQL));
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_EXECUTE_FAILED;
            }

            //...Log2.v("\nDynFtTitl.FtInsertTitl(): successfuly inserted record into table: " + cursors[curHandle].tableName);

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynTitl.FtInsertTitl(): Exit");
            return Constant.SUCCESS;
        }





    }
}
