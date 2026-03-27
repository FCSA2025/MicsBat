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
    /// This class provides methods that serve SuAnte data to/from a database.
    /// </summary>
    public class SuDynAnte
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
                    Application.Exit("\r\nSuDynAnte.GetNextFreeCursor(): ERROR: No available cursors.");
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
        /// method returns an index to a SuCursor object that can be used by subsequent
        /// calls to SuFetchAnte().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for SuFetchAnte() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int SuSelectAnte(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nSuDynAnte.SuSelectAnte(): Entry");
            SQLHANDLE hStmt;
            SQLHANDLE hUpdate;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the select clause.
            string stmt_buf = "select " + SuAnte.AllColumnsForSqlSelect + " from " + table;

            //Construct the 'where' part of the select clause.
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                stmt_buf += " where " + searchCriteria;
            }

            //If "order by" is not specified then assume it's for update.
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                // Order by was specified.
                stmt_buf += " order by " + orderBy;
            }

            //Get a Cursor object.
            curHandle = GetNextFreeCursor();

            //Make an new connection with the DB.
            SQLHDBC hConn = Ssutil.NewConn();

            //Get a statement handle.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            //Separate statement handle for update.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            //...Log2.v("\r\nSuDynAnte.SuSelectAnte(): [1] sqlRet = " + sqlRet);

            //...Log2.v("\nSuDynAnte.SuSelectAnte(): query = " + stmt_buf);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, ODBC.SQL_NTS);

            //...Log2.v("\r\nSuDynAnte.SuSelectAnte(): [4] sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "Could not Select Antenna for criteria: " + searchCriteria + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                Log2.e("\nSuDynAnte.SuSelectAnte(): ERROR: SQLExecute() failed");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //	Populate the cursor handle structure
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].cCurrAcode = "";
            cursors[curHandle].tableName = table;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hUpdate = hUpdate;
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].sqlQuery = stmt_buf;

            //...Log2.v("\n\nSuDynAnte.SuSelectAnte(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Retrieves a single row of antenna data from a table in the database using 
        /// a previously created Cursor object.
        /// </summary>
        /// <param name="curHandle"> - index of a Cursor object.</param>
        /// <param name="suAnte"> - a SuAnte object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for anteRec.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - SuCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static short SuFetchAnte(int curHandle, out SuAnte suAnte, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nSuDynAnte.SuFetchAnte(): Entry:");

            //Create an SuAnte object to output.
            suAnte = new SuAnte();
            //Create an array of nulls to output.
            nullInd = new SQLLEN[SuAnte.NUM_COLUMNS];

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Log2.e("\n\nSuDynAnte.SuFetchAnte(): Exit: FAIL: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\n\nSuDynAnte.SuFetchAnte(): Exit: ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return sqlRet;
                }
                else
                {
                    //	Error
                    Log2.e("\n\nSuDynAnte.SuFetchAnte(): Exit: ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return Error.DYN_MS_SQL_SERVER_ERR;
                }
            }

            // Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "cmd", out suAnte.cmd, SuAnte.CMD_SZ, out nullInd[SuAnte.CMD]);
                Ssutil.DbGetString(hStmt, 2, "recstat", out suAnte.recstat, SuAnte.RECSTAT_SZ, out nullInd[SuAnte.RECSTAT]);
                Ssutil.DbGetString(hStmt, 3, "acode", out suAnte.acode, SuAnte.ACODE_SZ, out nullInd[SuAnte.ACODE]);
                Ssutil.DbGetInt(hStmt, 4, "axtype", out suAnte.axtype, out nullInd[SuAnte.AXTYPE]);
                Ssutil.DbGetString(hStmt, 5, "axref", out suAnte.axref, SuAnte.AXREF_SZ, out nullInd[SuAnte.AXREF]);
                Ssutil.DbGetFloat(hStmt, 6, "again", out suAnte.again, out nullInd[SuAnte.AGAIN]);
                Ssutil.DbGetFloat(hStmt, 7, "abw", out suAnte.abw, out nullInd[SuAnte.ABW]);
                Ssutil.DbGetShort(hStmt, 8, "arms", out suAnte.arms, out nullInd[SuAnte.ARMS]);
                Ssutil.DbGetString(hStmt, 9, "aband", out suAnte.aband, SuAnte.ABAND_SZ, out nullInd[SuAnte.ABAND]);
                Ssutil.DbGetString(hStmt, 10, "amanu", out suAnte.amanu, SuAnte.AMANU_SZ, out nullInd[SuAnte.AMANU]);
                Ssutil.DbGetString(hStmt, 11, "apattern", out suAnte.apattern, SuAnte.APATTERN_SZ, out nullInd[SuAnte.APATTERN]);
                Ssutil.DbGetString(hStmt, 12, "amodel", out suAnte.amodel, SuAnte.AMODEL_SZ, out nullInd[SuAnte.AMODEL]);
                Ssutil.DbGetShort(hStmt, 13, "anip", out suAnte.anip, out nullInd[SuAnte.ANIP]);
                Ssutil.DbGetFloat(hStmt, 14, "ax0", out suAnte.ax0, out nullInd[SuAnte.AX0]);
                Ssutil.DbGetString(hStmt, 15, "adesc", out suAnte.adesc, SuAnte.ADESC_SZ, out nullInd[SuAnte.ADESC]);
                Ssutil.DbGetString(hStmt, 16, "antype", out suAnte.antype, SuAnte.ANTYPE_SZ, out nullInd[SuAnte.ANTYPE]);
                Ssutil.DbGetFloat(hStmt, 17, "aftbr", out suAnte.aftbr, out nullInd[SuAnte.AFTBR]);
                Ssutil.DbGetDouble(hStmt, 18, "lofreq", out suAnte.lofreq, out nullInd[SuAnte.LOFREQ]);
                Ssutil.DbGetDouble(hStmt, 19, "hifreq", out suAnte.hifreq, out nullInd[SuAnte.HIFREQ]);
                Ssutil.DbGetString(hStmt, 20, "bandcodes", out suAnte.bandcodes, SuAnte.BANDCODES_SZ, out nullInd[SuAnte.BANDCODES]);
                Ssutil.DbGetString(hStmt, 21, "mdate", out suAnte.mdate, SuAnte.MDATE_SZ, out nullInd[SuAnte.MDATE]);
                Ssutil.DbGetString(hStmt, 22, "mtime", out suAnte.mtime, SuAnte.MTIME_SZ, out nullInd[SuAnte.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\n\nSuDynAnte.SuFetchAnte(): ERROR: Exception caught: " + e.Message);
                GenUtil.SetErr("dynfeAnte02 -- Could not get antenna, because of field " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\nSuDynAnte.SuFetchAnte(): anteRec = \n" + suAnte.ToStringWN(nullInd));

            //	Save the key for deletes.
            cursors[curHandle].cCurrentCall1 = suAnte.acode;

            //...Log2.v("\n\nSuDynAnte.SuFetchAnte(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method checks whether a record, whose 'acode' column is the same
        /// as a prescribed SuAnte object, exists in a prescribed DB table; if a matching record
        /// is found the method returns Constant.SUCCESS.
        /// </summary>
        /// <param name="tableName"> - prescribed DB table name.</param>
        /// <param name="anteStruct"> - prescribed SuAnte object.</param>
        /// <returns></returns>
        public static int SuAnteExist(string tableName, SuAnte anteStruct)
        {
            //...Log2.v("\nSuDynAnte(): Entry: acode = " + anteStruct.acode);

            SuAnte tempStruct;
            SQLLEN[] tempNulls;
            SQLRETURN retVal;
            int cursorHandle;
            string searchCriteria;

            searchCriteria = String.Format("acode = '{0}'", anteStruct.acode);

            if ((cursorHandle = SuSelectAnte(tableName, searchCriteria, "")) < 0)
            {
                return (cursorHandle);   /* in-conclusive result due to ERRORS */
            }

            retVal = SuFetchAnte(cursorHandle, out tempStruct, out tempNulls);

            if (ODBC.IsOK(retVal))
            {
                // We found a matching record.
                SuCloseAnte(cursorHandle);
                return (Constant.SUCCESS);
            }
            else // Check for NOMORERECS.
            {
                if (retVal == Constant.NOMORERECS)
                {
                    SuCloseAnte(cursorHandle);
                    return (Constant.NOT_FOUND); /* this ante record does not exist */
                }
                else
                {
                    // Something unexpected happened.
                    SuCloseAnte(cursorHandle);
                    return (Error.DYN_MS_SQL_SERVER_ERR); /* this ante record does not exist */
                }

            }
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
        public static int SuCloseAnte(int curHandle)
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
        /// Inserts a SuAnte record into the database using column values prescribed 
        /// by the fields of the SuAnte object.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object that encapsulates the details of the record insertion.</param>
        /// <param name="suAnte"> - a SuAnte object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pAnte</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int SuInsertAnte(int curHandle, SuAnte suAnte, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynAnte.SuInsertAnte(): Entry");

            string cSQL;
            SQLRETURN sqlRet;

            SQLHANDLE hStmt;

            SQLHANDLE hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynAnte.SuInsertAnte(): ERROR: call to SQLAllocHandle() failed.");
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
            cSQL = SuAnte.BuildSqlInsertString(cursors[curHandle].tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            //into global memory with an SQLPOINTER pointer assigned to each one. SuAnte provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = suAnte.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            try
            {
                Ssutil.DbBindStringInput(hStmt, 1, "cmd", parameterValuePtr[SuAnte.CMD], SuAnte.CMD_SZ, nullIndPtr[SuAnte.CMD]);
                Ssutil.DbBindStringInput(hStmt, 2, "recstat", parameterValuePtr[SuAnte.RECSTAT], SuAnte.RECSTAT_SZ, nullIndPtr[SuAnte.RECSTAT]);
                Ssutil.DbBindStringInput(hStmt, 3, "acode", parameterValuePtr[SuAnte.ACODE], SuAnte.ACODE_SZ, nullIndPtr[SuAnte.ACODE]);
                Ssutil.DbBindIntInput(hStmt, 4, "axtype", parameterValuePtr[SuAnte.AXTYPE], nullIndPtr[SuAnte.AXTYPE]);
                Ssutil.DbBindStringInput(hStmt, 5, "axref", parameterValuePtr[SuAnte.AXREF], SuAnte.AXREF_SZ, nullIndPtr[SuAnte.AXREF]);
                Ssutil.DbBindFloatInput(hStmt, 6, "again", parameterValuePtr[SuAnte.AGAIN], nullIndPtr[SuAnte.AGAIN]);
                Ssutil.DbBindFloatInput(hStmt, 7, "abw", parameterValuePtr[SuAnte.ABW], nullIndPtr[SuAnte.ABW]);
                Ssutil.DbBindShortInput(hStmt, 8, "arms", parameterValuePtr[SuAnte.ARMS], nullIndPtr[SuAnte.ARMS]);
                Ssutil.DbBindStringInput(hStmt, 9, "aband", parameterValuePtr[SuAnte.ABAND], SuAnte.ABAND_SZ, nullIndPtr[SuAnte.ABAND]);
                Ssutil.DbBindStringInput(hStmt, 10, "amanu", parameterValuePtr[SuAnte.AMANU], SuAnte.AMANU_SZ, nullIndPtr[SuAnte.AMANU]);
                Ssutil.DbBindStringInput(hStmt, 11, "apattern", parameterValuePtr[SuAnte.APATTERN], SuAnte.APATTERN_SZ, nullIndPtr[SuAnte.APATTERN]);
                Ssutil.DbBindStringInput(hStmt, 12, "amodel", parameterValuePtr[SuAnte.AMODEL], SuAnte.AMODEL_SZ, nullIndPtr[SuAnte.AMODEL]);
                Ssutil.DbBindShortInput(hStmt, 13, "anip", parameterValuePtr[SuAnte.ANIP], nullIndPtr[SuAnte.ANIP]);
                Ssutil.DbBindFloatInput(hStmt, 14, "ax0", parameterValuePtr[SuAnte.AX0], nullIndPtr[SuAnte.AX0]);
                Ssutil.DbBindStringInput(hStmt, 15, "adesc", parameterValuePtr[SuAnte.ADESC], SuAnte.ADESC_SZ, nullIndPtr[SuAnte.ADESC]);
                Ssutil.DbBindStringInput(hStmt, 16, "antype", parameterValuePtr[SuAnte.ANTYPE], SuAnte.ANTYPE_SZ, nullIndPtr[SuAnte.ANTYPE]);
                Ssutil.DbBindFloatInput(hStmt, 17, "aftbr", parameterValuePtr[SuAnte.AFTBR], nullIndPtr[SuAnte.AFTBR]);
                Ssutil.DbBindDoubleInput(hStmt, 18, "lofreq", parameterValuePtr[SuAnte.LOFREQ], nullIndPtr[SuAnte.LOFREQ]);
                Ssutil.DbBindDoubleInput(hStmt, 19, "hifreq", parameterValuePtr[SuAnte.HIFREQ], nullIndPtr[SuAnte.HIFREQ]);
                Ssutil.DbBindStringInput(hStmt, 20, "bandcodes", parameterValuePtr[SuAnte.BANDCODES], SuAnte.BANDCODES_SZ, nullIndPtr[SuAnte.BANDCODES]);
                Ssutil.DbBindStringInput(hStmt, 21, "mdate", parameterValuePtr[SuAnte.MDATE], SuAnte.MDATE_SZ, nullIndPtr[SuAnte.MDATE]);
                Ssutil.DbBindStringInput(hStmt, 22, "mtime", parameterValuePtr[SuAnte.MTIME], SuAnte.MTIME_SZ, nullIndPtr[SuAnte.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\n\nDynAnte.SuInsertAnte(): ERROR: a call to DbBindStringInput() failed.");
                string str = String.Format("suInsertAnte03 -- Error binding parameters for antenna {0} on field: {1}", suAnte.acode, e.Message);
                Ssutil.DbGetDiagStmt(hStmt, str);
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_BINDING_FAILED;
            }

            //...Log2.v("nDynAnte.SuInsertAnte(): SQLExecDirect():\r\n" + cSQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            //...Log2.v("\r\nDynAnte.SuInsertAnte(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynAnte.SuInsertAnte(): ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                string str = String.Format("suInsertAnte02 -- Error inserting antenna: {0}", suAnte.acode);
                Ssutil.DbGetDiagStmt(hStmt, str); ;
                Ssutil.DisConnStmt(hConn, hStmt);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //...Log2.v("\n\nDynAnte.SuInsertAnte(): successfuly inserted record into table: " + cursors[curHandle].tableName);

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynAnte.SuInsertAnte(): Exit");
            return Constant.SUCCESS;
        }
#if false
        /// <summary>
        /// This method deletes an ES antenna record from the database.
        /// </summary>
        /// <param name="curHandle"> - a prescribed Cursor object, as returned by a previous call to SuSelectAnte().</param>
        /// <returns></returns>
        public static int SuDeleteAnte(int curHandle)
        {
            //...Log2.v("\n\nSuDynAnte.SuDeleteAnte(): Entry");

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

            string delete_buf = String.Format("delete from {0} where location='{1}' and call21='{2}'",
                cursors[curHandle].tableName,
                cursors[curHandle].cCurrentLoc,
                cursors[curHandle].cCurrentCall1
                );

            //...Log2.v("\r\nSuDynAnte.FtDeleteAntenna(): query = " + delete_buf);
            sqlRet = ODBC.SQLExecDirect(hUpdate, delete_buf, delete_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hUpdate, "ftDeleteAnte01 -- Error deleting from " + cursors[curHandle].tableName);
                Ssutil.DisConnStmt(hConn, hUpdate);
                Log2.e("\nSuDynAnte.SuDeleteAnte(): ERROR: SQLExecDirect() failed.");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            Ssutil.DisConnStmt(hConn, hUpdate);

            //...Log2.v("\n\nSuDynAnte.SuDeleteAnte(): Exit");
            return 0;
        }

        private const string UPDATE = "update {0}  set cmd= ?, recstat= ?, location= ?, call1= ?, txband= ?,  rxband= ?, acodetx= ?, acoderx= ?, g_t= ?, lnat= ?,  aht= ?, afslt= ?, afslr= ?, txhgmax= ?,rxhgmax=?,  satlongit= ?, satlong= ?, satlongs= ?, az= ?, el= ?,  sarc1= ?, sarc2= ?, rxpre= ?, txpre= ?, rxtro= ?,  txtro= ?, licence= ?, satname= ?, stata= ?, nota= ?,  op2= ?, antref= ?, orbit= ?, mdate= ?, mtime= ?   where location='{1}' and call1='{2}'";

        /// <summary>
        /// This method updates all of the column values of a record in a database ES antenna table.
        /// </summary>
        /// <param name="nCursor"> - a prescribed Cursor object, as returned by a previous call to SuSelectAnte().</param>
        /// <param name="feAnte"> - a prescribed SuAnte object that provides the updated field values.</param>
        /// <param name="nullInd"> - an array of ODBC nullInds associated with feAnte.</param>
        /// <returns></returns>
        public static int SuUpdateAnte(int nCursor, SuAnte feAnte, SQLLEN[] nullInd)
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
                Log2.e("\nSuDynAnte.SuUpdateAnte(): ERROR: Cursor object has cursorOpen = false.");
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_CUR_NOT_OPEN);
            }
            if (cursors[nCursor].pastLastRow)
            {
                /* Cursor is past the last row */
                Log2.e("\nSuDynAnte.SuUpdateAnte(): ERROR: Cursor object has pastLastRow = true.");
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

                Ssutil.DbBindStringInput(hUpdate, 0, "cmd", parameterValuePtr[SuAnte.CMD], SuAnte.CMD_SZ, nullIndPtr[SuAnte.CMD]);
                Ssutil.DbBindStringInput(hUpdate, 0, "recstat", parameterValuePtr[SuAnte.RECSTAT], SuAnte.RECSTAT_SZ, nullIndPtr[SuAnte.RECSTAT]);
                Ssutil.DbBindStringInput(hUpdate, 0, "location", parameterValuePtr[SuAnte.LOCATION], SuAnte.LOCATION_SZ, nullIndPtr[SuAnte.LOCATION]);
                Ssutil.DbBindStringInput(hUpdate, 0, "call1", parameterValuePtr[SuAnte.CALL1], SuAnte.CALL1_SZ, nullIndPtr[SuAnte.CALL1]);
                Ssutil.DbBindStringInput(hUpdate, 0, "txband", parameterValuePtr[SuAnte.TXBAND], SuAnte.TXBAND_SZ, nullIndPtr[SuAnte.TXBAND]);
                Ssutil.DbBindStringInput(hUpdate, 0, "rxband", parameterValuePtr[SuAnte.RXBAND], SuAnte.RXBAND_SZ, nullIndPtr[SuAnte.RXBAND]);
                Ssutil.DbBindStringInput(hUpdate, 0, "acodetx", parameterValuePtr[SuAnte.ACODETX], SuAnte.ACODETX_SZ, nullIndPtr[SuAnte.ACODETX]);
                Ssutil.DbBindStringInput(hUpdate, 0, "acoderx", parameterValuePtr[SuAnte.ACODERX], SuAnte.ACODERX_SZ, nullIndPtr[SuAnte.ACODERX]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "g_t", parameterValuePtr[SuAnte.G_T], nullIndPtr[SuAnte.G_T]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "lnat", parameterValuePtr[SuAnte.LNAT], nullIndPtr[SuAnte.LNAT]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "aht", parameterValuePtr[SuAnte.AHT], nullIndPtr[SuAnte.AHT]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "afslt", parameterValuePtr[SuAnte.AFSLT], nullIndPtr[SuAnte.AFSLT]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "afslr", parameterValuePtr[SuAnte.AFSLR], nullIndPtr[SuAnte.AFSLR]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "txhgmax", parameterValuePtr[SuAnte.TXHGMAX], nullIndPtr[SuAnte.TXHGMAX]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "rxhgmax", parameterValuePtr[SuAnte.RXHGMAX], nullIndPtr[SuAnte.RXHGMAX]);
                Ssutil.DbBindIntInput(hUpdate, 0, "satlongit", parameterValuePtr[SuAnte.SATLONGIT], nullIndPtr[SuAnte.SATLONGIT]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "satlong", parameterValuePtr[SuAnte.SATLONG], nullIndPtr[SuAnte.SATLONG]);
                Ssutil.DbBindStringInput(hUpdate, 0, "satlongs", parameterValuePtr[SuAnte.SATLONGS], SuAnte.SATLONGS_SZ, nullIndPtr[SuAnte.SATLONGS]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "az", parameterValuePtr[SuAnte.AZ], nullIndPtr[SuAnte.AZ]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "el", parameterValuePtr[SuAnte.EL], nullIndPtr[SuAnte.EL]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "sarc1", parameterValuePtr[SuAnte.SARC1], nullIndPtr[SuAnte.SARC1]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "sarc2", parameterValuePtr[SuAnte.SARC2], nullIndPtr[SuAnte.SARC2]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "rxpre", parameterValuePtr[SuAnte.RXPRE], nullIndPtr[SuAnte.RXPRE]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "txpre", parameterValuePtr[SuAnte.TXPRE], nullIndPtr[SuAnte.TXPRE]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "rxtro", parameterValuePtr[SuAnte.RXTRO], nullIndPtr[SuAnte.RXTRO]);
                Ssutil.DbBindFloatInput(hUpdate, 0, "txtro", parameterValuePtr[SuAnte.TXTRO], nullIndPtr[SuAnte.TXTRO]);
                Ssutil.DbBindStringInput(hUpdate, 0, "licence", parameterValuePtr[SuAnte.LICENCE], SuAnte.LICENCE_SZ, nullIndPtr[SuAnte.LICENCE]);
                Ssutil.DbBindStringInput(hUpdate, 0, "satname", parameterValuePtr[SuAnte.SATNAME], SuAnte.SATNAME_SZ, nullIndPtr[SuAnte.SATNAME]);
                Ssutil.DbBindStringInput(hUpdate, 0, "stata", parameterValuePtr[SuAnte.STATA], SuAnte.STATA_SZ, nullIndPtr[SuAnte.STATA]);
                Ssutil.DbBindStringInput(hUpdate, 0, "nota", parameterValuePtr[SuAnte.NOTA], SuAnte.NOTA_SZ, nullIndPtr[SuAnte.NOTA]);
                Ssutil.DbBindStringInput(hUpdate, 0, "op2", parameterValuePtr[SuAnte.OP2], SuAnte.OP2_SZ, nullIndPtr[SuAnte.OP2]);
                Ssutil.DbBindIntInput(hUpdate, 0, "antref", parameterValuePtr[SuAnte.ANTREF], nullIndPtr[SuAnte.ANTREF]);
                Ssutil.DbBindStringInput(hUpdate, 0, "orbit", parameterValuePtr[SuAnte.ORBIT], SuAnte.ORBIT_SZ, nullIndPtr[SuAnte.ORBIT]);
                Ssutil.DbBindStringInput(hUpdate, 0, "mdate", parameterValuePtr[SuAnte.MDATE], SuAnte.MDATE_SZ, nullIndPtr[SuAnte.MDATE]);
                Ssutil.DbBindStringInput(hUpdate, 0, "mtime", parameterValuePtr[SuAnte.MTIME], SuAnte.MTIME_SZ, nullIndPtr[SuAnte.MTIME]);
            }
            catch (Exception e)
            {
                Log2.e("\nSuDynAnte.SuUpdateAnte(): ERROR: ODBC Bind attempt failed: " + e.Message);
                Ssutil.DbGetDiagStmt(hUpdate, "dynAnte03 -- Error binding parameters for site on field: " + e.Message);
                return -3;
            }

            Log2.e("\nSuDynAnte.SuUpdateAnte(): query = " + update_buf);
            sqlRet = ODBC.SQLExecDirect(hUpdate, update_buf, update_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = String.Format("\nSuDynAnte.SuUpdateAnte(): ERROR: SQLExecDirect failed, sqlRet = {0}, SQL query:\n{1}", sqlRet, update_buf);
                Log2.e(str);
                Ssutil.DbGetDiagStmt(hUpdate, "SuDynAnte.SuUpdateAnte04 -- Error writing site: " + feAnte.location);
                return -4;
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);
            Ssutil.DisConn(hConn);

            return (0);
        }

#endif




    }
}
