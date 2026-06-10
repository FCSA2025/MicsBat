# Documented File: TpDynParm.cs
**Repository Path:** `_Utillib\TpDynParm.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _Configuration;
using _DataStructures;
using _NewLib;

namespace _Utillib
{
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
    public class TpDynParm
    {
        static string mStmtBuf;
        const string mSelect = "select protype, envtype, proname, envname, tsorbout, spherecalc, fsep, coordist, analopt, margin, numchan, chancodes, country, selsites, numcodes, codes, runname, reports, numcases, numtecases, parmparm, mdate, mtime from ";
        const string mInsert = "insert into {0} (protype, envtype, proname, envname, tsorbout, spherecalc, fsep, coordist, analopt, margin, numchan, chancodes, country, selsites, numcodes, codes, runname, reports, numcases, numtecases, parmparm, mdate, mtime) values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

        //===================================================================================================================================
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;
        //===================================================================================================================================


        /// <summary>
        /// Select a Parm record from a Parm table.  
        /// </summary>
        /// <remarks>
        /// This method performs the setup for an open cursor operation. The caller 
        /// identifies the Parm table and the search criteria for the selection and how 
        /// the rows are to be ordered. If searchCriteria is NULL then all the rows are 
        /// retrieved. If orderBy is NULL then the cursor is set for UPDATE. The MS SQL Server 
        /// documentation states that 'order by' and 'update' are mutually exclusive. 
        /// Testing indicates that the caller can in fact select with the 'order by' 
        /// and then perform an update contrary to the documentation.  
        /// </remarks>
        /// <param name="table"> - full MS SQL Server Parm table name</param>
        /// <param name="searchCriteria"> - the search criteria excluding the 'where'</param>
        /// <param name="orderBy"> - the order by info of the select statement excluding the actual 'order by'</param>
        /// <returns></returns>
        public static int TpSelectParm(string table,            /* full MS SQL Server Parm table name */
                                         string searchCriteria, /* selection criteria */
                                         string orderBy         /* how is selection to be ordered */
                                        )
        {
            //...Log2.v("\nTpDynParm.TpSelectParm(): Entry-" + table + " " + searchCriteria + " " + orderBy);

            int curHandle;                  /* cursor handle */

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;

            /* Construct the select clause */
            mStmtBuf = mSelect + table;

            /* Construct the 'where' part of the select clause */
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                // Search criteria was specified.
                mStmtBuf += " where ";
                mStmtBuf += searchCriteria;
            }

            /* If "order by" is not specified then assume its for update */
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                /* Order by was specified */
                mStmtBuf += "  order by ";
                mStmtBuf += orderBy;
            }

            //	Get next free cursor area
            curHandle = GetNextFreeCursor();

            //	allocate and open the handle
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLPrepare(hStmt, mStmtBuf, mStmtBuf.Length);

            sqlRet = ODBC.SQLExecute(hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "tpSelectParm Error:\n" + mStmtBuf + "\n");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.ODBC_GET_FAILED;
            }

            //	Inititialize the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].tableName = table;
            cursors[curHandle].nRowNumber = 0;

            //...Log2.v("\nTpDynParm.TpSelectParm(): Exit: returns " + curHandle);
            return curHandle;
        }

        /// <summary>
        /// Gets a row from a TS Parm table.  
        /// </summary>
        /// <remarks>
        /// This method fetches a row from a PDF TS Parm table. The routine 
        /// 'ftSelectParm' must have been called prior to calling this routine.  
        /// <para>
        /// Caller must supply an array of null indicators large enough (one null 
        /// indicator per nullable field in a row).  
        /// </para>
        /// </remarks>
        /// <param name="nCursor"> - handle returned by 'ftSelectParm'</param>
        /// <param name="parmRec"> - the caller suplied struct 'ParmRec' is filled in with the contents of a row</param>
        /// <param name="nullInd"> - the appropriate fields in the caller supplied array are filled in</param>
        /// <returns></returns>
        public static int TpFetchParm(int nCursor,	/* handle returned by 'ftSelectParm' */
                                out TpParm parmRec, /* caller's struct for row of data */
                                out SQLLEN[] nullInd)   /* caller's array of null indicators */
        {
            //...Log2.v("\nTpDynParm.TpFetchParm(): Entry");

            // Out.
            parmRec = null;
            nullInd = null;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[nCursor].hStmt;

            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    return sqlRet;
                }
                else
                {
                    //	Error
                    return Constant.FAILURE;
                }
            }

            cursors[nCursor].nRowNumber++;

            // Create the 'out' data objects.
            parmRec = new TpParm();
            nullInd = NullHelper.CreateArrayOfNullInd(TpParm.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            //	Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "protype", out parmRec.protype, TpParm.PROTYPE_SZ, out nullInd[TpParm.PROTYPE]);

                Ssutil.DbGetString(hStmt, 2, "envtype", out parmRec.envtype, TpParm.ENVTYPE_SZ, out nullInd[TpParm.ENVTYPE]);

                Ssutil.DbGetString(hStmt, 3, "proname", out parmRec.proname, TpParm.PRONAME_SZ, out nullInd[TpParm.PRONAME]);

                Ssutil.DbGetString(hStmt, 4, "envname", out parmRec.envname, TpParm.ENVNAME_SZ, out nullInd[TpParm.ENVNAME]);

                Ssutil.DbGetString(hStmt, 5, "tsorbout", out parmRec.tsorbout, TpParm.TSORBOUT_SZ, out nullInd[TpParm.TSORBOUT]);

                Ssutil.DbGetString(hStmt, 6, "spherecalc", out parmRec.spherecalc, TpParm.SPHERECALC_SZ, out nullInd[TpParm.SPHERECALC]);

                Ssutil.DbGetDouble(hStmt, 7, "fsep", out parmRec.fsep, out nullInd[TpParm.FSEP]);

                Ssutil.DbGetDouble(hStmt, 8, "coordist", out parmRec.coordist, out nullInd[TpParm.COORDIST]);

                Ssutil.DbGetString(hStmt, 9, "analopt", out parmRec.analopt, TpParm.ANALOPT_SZ, out nullInd[TpParm.ANALOPT]);

                Ssutil.DbGetDouble(hStmt, 10, "margin", out parmRec.margin, out nullInd[TpParm.MARGIN]);

                Ssutil.DbGetShort(hStmt, 11, "numchan", out parmRec.numchan, out nullInd[TpParm.NUMCHAN]);

                Ssutil.DbGetString(hStmt, 12, "chancodes", out parmRec.chancodes, TpParm.CHANCODES_SZ, out nullInd[TpParm.CHANCODES]);

                Ssutil.DbGetString(hStmt, 13, "country", out parmRec.country, TpParm.COUNTRY_SZ, out nullInd[TpParm.COUNTRY]);

                Ssutil.DbGetString(hStmt, 14, "selsites", out parmRec.selsites, TpParm.SELSITES_SZ, out nullInd[TpParm.SELSITES]);

                Ssutil.DbGetShort(hStmt, 15, "numcodes", out parmRec.numcodes, out nullInd[TpParm.NUMCODES]);

                Ssutil.DbGetString(hStmt, 16, "codes", out parmRec.codes, TpParm.CODES_SZ, out nullInd[TpParm.CODES]);

                Ssutil.DbGetString(hStmt, 17, "runname", out parmRec.runname, TpParm.RUNNAME_SZ, out nullInd[TpParm.RUNNAME]);

                Ssutil.DbGetInt(hStmt, 18, "reports", out parmRec.reports, out nullInd[TpParm.REPORTS]);

                Ssutil.DbGetInt(hStmt, 19, "numcases", out parmRec.numcases, out nullInd[TpParm.NUMCASES]);

                Ssutil.DbGetInt(hStmt, 20, "numtecases", out parmRec.numtecases, out nullInd[TpParm.NUMTECASES]);

                Ssutil.DbGetString(hStmt, 21, "parmparm", out parmRec.parmparm, TpParm.PARMPARM_SZ, out nullInd[TpParm.PARMPARM]);

                Ssutil.DbGetString(hStmt, 22, "mdate", out parmRec.mdate, TpParm.MDATE_SZ, out nullInd[TpParm.MDATE]);

                Ssutil.DbGetString(hStmt, 23, "mtime", out parmRec.mtime, TpParm.MTIME_SZ, out nullInd[TpParm.MTIME]);
            }
            catch (Exception e)
            {
                GenUtil.SetErr("dynParm02: Input error on field: " + e.Message);
                Log2.e("\nTpDynParm.TpFetchParm(): ERROR: DbGet failed for: " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v("\nTpDynParm.TpFetchParm(): Exit: returns " + 0);
            return (Constant.SUCCESS);
        }

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
            //...Log2.v("\nTpDynParm.GetNextFreeCursor(): returns " + curHandle);
            return curHandle;
        }

        /// <summary>
        /// Close a cursor associated with a PDF TS Parm table.  
        /// </summary>
        /// <param name="curHandle"></param>
        /// <returns></returns>
        public static int TpCloseParm(int curHandle)
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

            //...Log2.v("\nTpDynParm.TpCloseParm(): returns " + 0);
            return 0;
        }

        /// <summary>
        /// Updates a row from a TS Parm table.  
        /// </summary>
        /// <remarks>
        /// This method updates a row from a PDF TS Parm table. The routine 
        /// 'ftSelectParm' must have been called prior to calling this routine.  
        /// </remarks>
        /// <param name="nCursor"> - handle returned by 'ftSelectParm</param>
        /// <param name="tpParm"></param>
        /// <param name="nullInd"> - the appropriate fields in the caller supplied array are filled in</param>
        /// <returns></returns>
        public static int TpUpdateParm(int nCursor,         /* handle returned by 'ftSelectParm */
                                        TpParm tpParm,     /* caller's struct for row of data */
                                        SQLLEN[] nullInd)   /* caller's array of null indicators */
        {
            SQLRETURN sqlRet;
            SQLHDBC hConn = Ssutil.NewConn();

            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            /* Prepare the update statement */
            //update_buf = String.Format(update, cursors[nCursor].tableName, tpParm.runname);

            StringBuilder sb = new StringBuilder();
            sb.Append("update ");
            sb.Append(cursors[nCursor].tableName);
            sb.Append(" set ");
            // ATTENTION! ... only 22 of the 27 members of tpParm need ODBC bindings for DB table update.
            //                so we need to explicitely specify them.
            //       binding #01    :    protype
            //       binding #02    :    envtype
            //       binding #03    :    proname
            //       binding #04    :    envname
            //       binding #05    :    tsorbout
            //       binding #06    :    spherecalc
            //       binding #07    :    fsep
            //       binding #08    :    coordist
            //       binding #09    :    analopt
            //       binding #10    :    margin
            //       binding #11    :    numchan
            //       binding #12    :    chancodes
            //       binding #13    :    country
            //       binding #14    :    selsites
            //       binding #15    :    numcodes
            //       binding #16    :    codes
            //       binding #17    :    reports
            //       binding #18    :    numcases
            //       binding #19    :    numtecases
            //       binding #20    :    parmparm
            //       binding #21    :    mdate
            //       binding #22    :    mtime
            sb.Append("protype =?, envtype =?, proname =?, envname =?, tsorbout =?, spherecalc =?, fsep =?, coordist =?, analopt =?, margin =?, numchan =?, chancodes =?, country =?, selsites =?, numcodes =?, codes =?, reports =?, numcases =?, numtecases =?, parmparm =?, mdate =?, mtime =?");
            sb.Append(" where runname='");
            sb.Append(tpParm.runname);
            sb.Append("'");

            string update_buf = sb.ToString();
            //...Log2.v("\nTpDynParm.TpUpdateParm(): update_buf = \n" + update_buf);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            //into global memory with an SQLPOINTER pointer assigned to each one. FtAnte provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = tpParm.CopyToArrayOfSQLPOINTERs();

            // Bind the parameters.
            // ATTENTION! ... We only need to bind a subset of tpParm's members for the table update.
            //                Beware of the difference between binding#, table column# and tpParm member# !
            try
            {
                int b = 1;  // used for the binding#, starting at 1.
                Ssutil.DbBindStringInput(hUpdate, b, "protype", parameterValuePtr[TpParm.PROTYPE], (SQLULEN)tpParm.protype.Length, nullIndPtr[TpParm.PROTYPE]);
                b = 2;
                Ssutil.DbBindStringInput(hUpdate, b, "envtype", parameterValuePtr[TpParm.ENVTYPE], (SQLULEN)tpParm.envtype.Length, nullIndPtr[TpParm.ENVTYPE]);
                b = 3;
                Ssutil.DbBindStringInput(hUpdate, b, "proname", parameterValuePtr[TpParm.PRONAME], (SQLULEN)tpParm.proname.Length, nullIndPtr[TpParm.PRONAME]);
                b = 4;
                Ssutil.DbBindStringInput(hUpdate, b, "envname", parameterValuePtr[TpParm.ENVNAME], (SQLULEN)tpParm.envname.Length, nullIndPtr[TpParm.ENVNAME]);
                b = 5;
                Ssutil.DbBindStringInput(hUpdate, b, "tsorbout", parameterValuePtr[TpParm.TSORBOUT], (SQLULEN)tpParm.tsorbout.Length, nullIndPtr[TpParm.TSORBOUT]);
                b = 6;
                Ssutil.DbBindStringInput(hUpdate, b, "spherecalc", parameterValuePtr[TpParm.SPHERECALC], (SQLULEN)tpParm.spherecalc.Length, nullIndPtr[TpParm.SPHERECALC]);
                b = 7;
                Ssutil.DbBindDoubleInput(hUpdate, b, "fsep", parameterValuePtr[TpParm.FSEP], nullIndPtr[TpParm.FSEP]);
                b = 8;
                Ssutil.DbBindDoubleInput(hUpdate, b, "coordist", parameterValuePtr[TpParm.COORDIST], nullIndPtr[TpParm.COORDIST]);
                b = 9;
                Ssutil.DbBindStringInput(hUpdate, b, "analopt", parameterValuePtr[TpParm.ANALOPT], (SQLULEN)tpParm.analopt.Length, nullIndPtr[TpParm.ANALOPT]);
                b = 10;
                Ssutil.DbBindDoubleInput(hUpdate, b, "margin", parameterValuePtr[TpParm.MARGIN], nullIndPtr[TpParm.MARGIN]);
                b = 11;
                Ssutil.DbBindShortInput(hUpdate, b, "numchan", parameterValuePtr[TpParm.NUMCHAN], nullIndPtr[TpParm.NUMCHAN]);
                b = 12;
                Ssutil.DbBindStringInput(hUpdate, b, "chancodes", parameterValuePtr[TpParm.CHANCODES], (SQLULEN)tpParm.chancodes.Length, nullIndPtr[TpParm.CHANCODES]);
                b = 13;  // DB table does not have columns: tempant, tempctx, tempplan, tempequip
                Ssutil.DbBindStringInput(hUpdate, b, "country", parameterValuePtr[TpParm.COUNTRY], (SQLULEN)tpParm.country.Length, nullIndPtr[TpParm.COUNTRY]);
                b = 14;
                Ssutil.DbBindStringInput(hUpdate, b, "selsites", parameterValuePtr[TpParm.SELSITES], (SQLULEN)tpParm.selsites.Length, nullIndPtr[TpParm.SELSITES]);
                b = 15;
                Ssutil.DbBindShortInput(hUpdate, b, "numcodes", parameterValuePtr[TpParm.NUMCODES], nullIndPtr[TpParm.NUMCODES]);
                b = 16;
                Ssutil.DbBindStringInput(hUpdate, b, "codes", parameterValuePtr[TpParm.CODES], (SQLULEN)tpParm.codes.Length, nullIndPtr[TpParm.CODES]);
                b = 17;
                Ssutil.DbBindIntInput(hUpdate, b, "reports", parameterValuePtr[TpParm.REPORTS], nullIndPtr[TpParm.REPORTS]);
                b = 18;  // DB table column 'runname' is not being updated.
                Ssutil.DbBindIntInput(hUpdate, b, "numcases", parameterValuePtr[TpParm.NUMCASES], nullIndPtr[TpParm.NUMCASES]);
                b = 19;
                Ssutil.DbBindIntInput(hUpdate, b, "numtecases", parameterValuePtr[TpParm.NUMTECASES], nullIndPtr[TpParm.NUMTECASES]);
                b = 20;
                Ssutil.DbBindStringInput(hUpdate, b, "parmparm", parameterValuePtr[TpParm.PARMPARM], (SQLULEN)tpParm.parmparm.Length, nullIndPtr[TpParm.PARMPARM]);
                b = 21;
                Ssutil.DbBindStringInput(hUpdate, b, "mdate", parameterValuePtr[TpParm.MDATE], (SQLULEN)tpParm.mdate.Length, nullIndPtr[TpParm.MDATE]);
                b = 22;
                Ssutil.DbBindStringInput(hUpdate, b, "mtime", parameterValuePtr[TpParm.MTIME], (SQLULEN)tpParm.mtime.Length, nullIndPtr[TpParm.MTIME]);

            }
            catch (Exception e)
            {
                Ssutil.DbGetDiagStmt(hUpdate, "dynParm03 -- Error binding parameters for Parm on field: " + e.Message);
                return Error.ODBC_BINDING_FAILED;
            }

            sqlRet = ODBC.SQLExecDirect(hUpdate, update_buf, update_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTpDynParm.TpUpdateParm(): ERROR: SQLExecDirect() failed");
                Ssutil.DbGetDiagStmt(hUpdate, "dynParm04 -- Error updating Parm:");
                return Error.ODBC_QUERY_FAILED;
            }

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);
            Ssutil.DisConn(hConn);

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Insert a row from a PDF TS Parm table.  
        /// </summary>
        /// <param name="curHandle"></param>
        /// <param name="tpParm"></param>
        /// <param name="nullInd"> - caller's array of null indicators</param>
        /// <returns></returns>
        public static int TpInsertParm(int curHandle, TpParm tpParm, SQLLEN[] nullInd)
        {

            SQLRETURN sqlRet;
            SQLHANDLE hConn = Ssutil.NewConn();
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                return (Error.DYN_CUR_NOT_OPEN);
            }

            string update_buf = String.Format(mInsert, cursors[curHandle].tableName);
            //...Log2.v("\nTpDynParm.TpInsertParm(): update_buf = \n" + update_buf);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            //into global memory with an SQLPOINTER pointer assigned to each one. FtAnte provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = tpParm.CopyToArrayOfSQLPOINTERs();

            // Bind the parameters.
            // ATTENTION! ... We only need to bind a subset of tpParm's members for the table update.
            //                Beware of the difference between binding#, table column# and tpParm member# !
            try
            {
                int b = 1;  // used for the binding#, starting at 1.
                Ssutil.DbBindStringInput(hUpdate, b, "protype", parameterValuePtr[TpParm.PROTYPE], (SQLULEN)tpParm.protype.Length, nullIndPtr[TpParm.PROTYPE]);
                b = 2;
                Ssutil.DbBindStringInput(hUpdate, b, "envtype", parameterValuePtr[TpParm.ENVTYPE], (SQLULEN)tpParm.envtype.Length, nullIndPtr[TpParm.ENVTYPE]);
                b = 3;
                Ssutil.DbBindStringInput(hUpdate, b, "proname", parameterValuePtr[TpParm.PRONAME], (SQLULEN)tpParm.proname.Length, nullIndPtr[TpParm.PRONAME]);
                b = 4;
                Ssutil.DbBindStringInput(hUpdate, b, "envname", parameterValuePtr[TpParm.ENVNAME], (SQLULEN)tpParm.envname.Length, nullIndPtr[TpParm.ENVNAME]);
                b = 5;
                Ssutil.DbBindStringInput(hUpdate, b, "tsorbout", parameterValuePtr[TpParm.TSORBOUT], (SQLULEN)tpParm.tsorbout.Length, nullIndPtr[TpParm.TSORBOUT]);
                b = 6;
                Ssutil.DbBindStringInput(hUpdate, b, "spherecalc", parameterValuePtr[TpParm.SPHERECALC], (SQLULEN)tpParm.spherecalc.Length, nullIndPtr[TpParm.SPHERECALC]);
                b = 7;
                Ssutil.DbBindDoubleInput(hUpdate, b, "fsep", parameterValuePtr[TpParm.FSEP], nullIndPtr[TpParm.FSEP]);
                b = 8;
                Ssutil.DbBindDoubleInput(hUpdate, b, "coordist", parameterValuePtr[TpParm.COORDIST], nullIndPtr[TpParm.COORDIST]);
                b = 9;
                Ssutil.DbBindStringInput(hUpdate, b, "analopt", parameterValuePtr[TpParm.ANALOPT], (SQLULEN)tpParm.analopt.Length, nullIndPtr[TpParm.ANALOPT]);
                b = 10;
                Ssutil.DbBindDoubleInput(hUpdate, b, "margin", parameterValuePtr[TpParm.MARGIN], nullIndPtr[TpParm.MARGIN]);
                b = 11;
                Ssutil.DbBindShortInput(hUpdate, b, "numchan", parameterValuePtr[TpParm.NUMCHAN], nullIndPtr[TpParm.NUMCHAN]);
                b = 12;
                Ssutil.DbBindStringInput(hUpdate, b, "chancodes", parameterValuePtr[TpParm.CHANCODES], (SQLULEN)tpParm.chancodes.Length, nullIndPtr[TpParm.CHANCODES]);
                b = 13;  // DB table does not have columns: tempant, tempctx, tempplan, tempequip
                Ssutil.DbBindStringInput(hUpdate, b, "country", parameterValuePtr[TpParm.COUNTRY], (SQLULEN)tpParm.country.Length, nullIndPtr[TpParm.COUNTRY]);
                b = 14;
                Ssutil.DbBindStringInput(hUpdate, b, "selsites", parameterValuePtr[TpParm.SELSITES], (SQLULEN)tpParm.selsites.Length, nullIndPtr[TpParm.SELSITES]);
                b = 15;
                Ssutil.DbBindShortInput(hUpdate, b, "numcodes", parameterValuePtr[TpParm.NUMCODES], nullIndPtr[TpParm.NUMCODES]);
                b = 16;
                Ssutil.DbBindStringInput(hUpdate, b, "codes", parameterValuePtr[TpParm.CODES], (SQLULEN)tpParm.codes.Length, nullIndPtr[TpParm.CODES]);
                b = 17;
                Ssutil.DbBindStringInput(hUpdate, b, "runname", parameterValuePtr[TpParm.RUNNAME], (SQLULEN)tpParm.runname.Length, nullIndPtr[TpParm.RUNNAME]);
                b = 18;
                Ssutil.DbBindIntInput(hUpdate, b, "reports", parameterValuePtr[TpParm.REPORTS], nullIndPtr[TpParm.REPORTS]);
                b = 19;  // DB table column 'runname' is not being updated.
                Ssutil.DbBindIntInput(hUpdate, b, "numcases", parameterValuePtr[TpParm.NUMCASES], nullIndPtr[TpParm.NUMCASES]);
                b = 20;
                Ssutil.DbBindIntInput(hUpdate, b, "numtecases", parameterValuePtr[TpParm.NUMTECASES], nullIndPtr[TpParm.NUMTECASES]);
                b = 21;
                Ssutil.DbBindStringInput(hUpdate, b, "parmparm", parameterValuePtr[TpParm.PARMPARM], (SQLULEN)tpParm.parmparm.Length, nullIndPtr[TpParm.PARMPARM]);
                b = 22;
                Ssutil.DbBindStringInput(hUpdate, b, "mdate", parameterValuePtr[TpParm.MDATE], (SQLULEN)tpParm.mdate.Length, nullIndPtr[TpParm.MDATE]);
                b = 23;
                Ssutil.DbBindStringInput(hUpdate, b, "mtime", parameterValuePtr[TpParm.MTIME], (SQLULEN)tpParm.mtime.Length, nullIndPtr[TpParm.MTIME]);

            }
            catch (Exception e)
            {
                Log2.e("\nTpDynParm.TpInsertParm(): ERROR: ODBC binding failed: " + e.Message);
                Ssutil.DbGetDiagStmt(hUpdate, "tpInsertParm01 -- Error binding parameters for Parm on field: " + e.Message);
                return Error.ODBC_BINDING_FAILED;
            }

            sqlRet = ODBC.SQLExecDirect(hUpdate, update_buf, update_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hUpdate, "tpInsertParm02 -- Error inserting Parm.");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            Ssutil.DisConnStmt(hConn, hUpdate);

            return Constant.SUCCESS;
        }






    }
}

```
