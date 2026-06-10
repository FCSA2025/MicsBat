# Documented File: TeDynChan.cs
**Repository Path:** `TpRunTsip20260126\TeDynChan.cs`
**Primary Layer:** `TpRunTsip20260126`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TpRunTsip
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
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
    /// Provides methods to prepare and insert 
    /// records into the database table <b>&lt;userID&gt;.te_&lt;tableName&gt;_&lt;runID&gt;_chan</b>
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item> The methods in this class are used to perform operations on the
    /// te CHAN table. The actual CHAN table name is a variable, prescribed by the caller.</item>
    /// <item>The supported operations are: TePrepareChan, TeInsertChan and TeChanClose.</item>
    /// <item>The caller must first call the method TePrepareChan() that creates a cursor 
    /// for use with a subsequent call to TeInsertChan().</item>
    /// <item>The method TeCloseChan() must be called to release internal cursor resources (a cursor 
    /// encapsulates an ODBC connection and statement handle et. al.).</item>
    /// </list>
    /// </remarks>
    public class TeDynChan
    {
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int tePrepareChan([In] string table);

        public static SQLHANDLE hTECStmt;
        public static SQLHDBC hTECConn = SQLHDBC.Zero;
        public static string cChanTable;

        /// <summary>
        /// This method calls ODBC.SQLPrepare() to prepare a table insert on an internal (private) statement 
        /// handle that can then be used in a subsequent call to ODBC.SQLExecute to
        /// actually perform the SQL record insertion.
        /// </summary>
        /// <param name="table"> - full name of the DB table into which the record is to be inserted.</param>
        /// <returns></returns>
        public static int TePrepareChan(string table)
        {
            SQLRETURN sqlRet;

            string insert_buf;

            cChanTable = table;

            // For a hybrid build, we need to set the static variable cChanTable
            // in native file teDynChan.cpp.
#if PINVOKE
            tePrepareChan(table);
#endif

            insert_buf = String.Format("insert into {0} (interferer, terrcall1, terrcall2, terrbndcde, terranum, terrchid, earthlocation, earthcall1, earthchid, inttraftx, victrafrx, inteqpttx, viceqptrx, intfreqtx, inttxpwr, inttxpwr2, inttxafls, inttxafls2, vicrxafls, vicfreqrx, vicpwrrx, stattx, statrx, energy, etreport, tereport, ctxinttraftx, ctxvictrafrx, ctxeqpt, calctype, earthmdsc, terrmdsc, eartheirp, terreirp, freqsep, scang, loss20mode1, loss01mode1, loss01mode2, calci20mode1, calci01mode1, calci01mode2, reqd20mode1, reqd01mode1, reqd01mode2, marg20mode1, marg01mode1, marg01mode2, remterracode, remterragain, processed, terrant) values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", table);

            hTECConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hTECConn, out hTECStmt);

            sqlRet = ODBC.SQLPrepare(hTECStmt, insert_buf, insert_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTeDynChan.TePrepareChan(): ERROR: SQLPrepare() failed.");

                Ssutil.DbGetDiagStmt(hTECStmt, "tePrepareChan01: Could not prepare es channel.");

                return Error.ODBC_PREPARE_FAILED;
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method closes the internal (private) cursor object by freeing the ODBC 
        /// connection and statement handles. 
        /// </summary>
        /// <param name=""></param>
        public static int TeCloseChan()
        {
            if (hTECConn != null)
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hTECStmt);

                hTECStmt = SQLHANDLE.Zero;

                Ssutil.DisConn(hTECConn);

                hTECConn = SQLHDBC.Zero;
            }
            return 0;
        }

        /// <summary>
        /// This method calls ODBC.SQLExecute() to perform insertion of a record
        /// into a DB table by using the internal (private) ODBC statement handle
        /// that was prepared on by a previous call to ODBC.SQLPrepare().
        /// </summary>
        /// <param name="teChan"> - TeChan object (record) to be inserted into the DB table.</param>
        /// <param name="nullInds"> - array of ODBC nullInds associated with ttChan.</param>
        /// <returns></returns>
        public static int TeInsertChan(TeChan teChan, SQLLEN[] nullInds)
        {
            SQLRETURN sqlRet;

            /*  The following 're-prepares the channel insert since we may have done
            *   a commit in the meantime */
            if (hTECConn == null)
            {
                TePrepareChan(cChanTable);
            }

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            //into global memory with an SQLPOINTER pointer assigned to each one. FtAnte provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = teChan.CopyToArrayOfSQLPOINTERs();

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInds);

            //	exec sql execute insert_chan using...
            try
            {
                // Initialize auto-increment binding 'number'.
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(hTECStmt, 0, "interferer", parameterValuePtr[TeChan.INTERFERER], TeChan.INTERFERER_SZ, nullIndPtr[TeChan.INTERFERER]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "terrcall1", parameterValuePtr[TeChan.TERRCALL1], TeChan.TERRCALL1_SZ, nullIndPtr[TeChan.TERRCALL1]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "terrcall2", parameterValuePtr[TeChan.TERRCALL2], TeChan.TERRCALL2_SZ, nullIndPtr[TeChan.TERRCALL2]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "terrbndcde", parameterValuePtr[TeChan.TERRBNDCDE], TeChan.TERRBNDCDE_SZ, nullIndPtr[TeChan.TERRBNDCDE]);
                Ssutil.DbBindShortInput(hTECStmt, 0, "terranum", parameterValuePtr[TeChan.TERRANUM], nullIndPtr[TeChan.TERRANUM]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "terrchid", parameterValuePtr[TeChan.TERRCHID], TeChan.TERRCHID_SZ, nullIndPtr[TeChan.TERRCHID]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "earthlocation", parameterValuePtr[TeChan.EARTHLOCATION], TeChan.EARTHLOCATION_SZ, nullIndPtr[TeChan.EARTHLOCATION]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "earthcall1", parameterValuePtr[TeChan.EARTHCALL1], TeChan.EARTHCALL1_SZ, nullIndPtr[TeChan.EARTHCALL1]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "earthchid", parameterValuePtr[TeChan.EARTHCHID], TeChan.EARTHCHID_SZ, nullIndPtr[TeChan.EARTHCHID]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "inttraftx", parameterValuePtr[TeChan.INTTRAFTX], TeChan.INTTRAFTX_SZ, nullIndPtr[TeChan.INTTRAFTX]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "victrafrx", parameterValuePtr[TeChan.VICTRAFRX], TeChan.VICTRAFRX_SZ, nullIndPtr[TeChan.VICTRAFRX]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "inteqpttx", parameterValuePtr[TeChan.INTEQPTTX], TeChan.INTEQPTTX_SZ, nullIndPtr[TeChan.INTEQPTTX]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "viceqptrx", parameterValuePtr[TeChan.VICEQPTRX], TeChan.VICEQPTRX_SZ, nullIndPtr[TeChan.VICEQPTRX]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "intfreqtx", parameterValuePtr[TeChan.INTFREQTX], nullIndPtr[TeChan.INTFREQTX]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "inttxpwr", parameterValuePtr[TeChan.INTTXPWR], nullIndPtr[TeChan.INTTXPWR]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "inttxpwr2", parameterValuePtr[TeChan.INTTXPWR2], nullIndPtr[TeChan.INTTXPWR2]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "inttxafls", parameterValuePtr[TeChan.INTTXAFLS], nullIndPtr[TeChan.INTTXAFLS]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "inttxafls2", parameterValuePtr[TeChan.INTTXAFLS2], nullIndPtr[TeChan.INTTXAFLS2]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "vicrxafls", parameterValuePtr[TeChan.VICRXAFLS], nullIndPtr[TeChan.VICRXAFLS]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "vicfreqrx", parameterValuePtr[TeChan.VICFREQRX], nullIndPtr[TeChan.VICFREQRX]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "vicpwrrx", parameterValuePtr[TeChan.VICPWRRX], nullIndPtr[TeChan.VICPWRRX]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "stattx", parameterValuePtr[TeChan.STATTX], TeChan.STATTX_SZ, nullIndPtr[TeChan.STATTX]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "statrx", parameterValuePtr[TeChan.STATRX], TeChan.STATRX_SZ, nullIndPtr[TeChan.STATRX]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "energy", parameterValuePtr[TeChan.ENERGY], nullIndPtr[TeChan.ENERGY]);

                /* calculated fields */
                Ssutil.DbBindShortInput(hTECStmt, 0, "etreport", parameterValuePtr[TeChan.ETREPORT], nullIndPtr[TeChan.ETREPORT]);
                Ssutil.DbBindShortInput(hTECStmt, 0, "tereport", parameterValuePtr[TeChan.TEREPORT], nullIndPtr[TeChan.TEREPORT]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "ctxinttraftx", parameterValuePtr[TeChan.CTXINTTRAFTX], TeChan.CTXINTTRAFTX_SZ, nullIndPtr[TeChan.CTXINTTRAFTX]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "ctxvictrafrx", parameterValuePtr[TeChan.CTXVICTRAFRX], TeChan.CTXVICTRAFRX_SZ, nullIndPtr[TeChan.CTXVICTRAFRX]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "ctxeqpt", parameterValuePtr[TeChan.CTXEQPT], TeChan.CTXEQPT_SZ, nullIndPtr[TeChan.CTXEQPT]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "calctype", parameterValuePtr[TeChan.CALCTYPE], TeChan.CALCTYPE_SZ, nullIndPtr[TeChan.CALCTYPE]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "earthmdsc", parameterValuePtr[TeChan.EARTHMDSC], nullIndPtr[TeChan.EARTHMDSC]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "terrmdsc", parameterValuePtr[TeChan.TERRMDSC], nullIndPtr[TeChan.TERRMDSC]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "eartheirp", parameterValuePtr[TeChan.EARTHEIRP], nullIndPtr[TeChan.EARTHEIRP]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "terreirp", parameterValuePtr[TeChan.TERREIRP], nullIndPtr[TeChan.TERREIRP]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "freqsep", parameterValuePtr[TeChan.FREQSEP], nullIndPtr[TeChan.FREQSEP]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "scang", parameterValuePtr[TeChan.SCANG], nullIndPtr[TeChan.SCANG]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "loss20mode1", parameterValuePtr[TeChan.LOSS20MODE1], nullIndPtr[TeChan.LOSS20MODE1]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "loss01mode1", parameterValuePtr[TeChan.LOSS01MODE1], nullIndPtr[TeChan.LOSS01MODE1]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "loss01mode2", parameterValuePtr[TeChan.LOSS01MODE2], nullIndPtr[TeChan.LOSS01MODE2]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "calci20mode1", parameterValuePtr[TeChan.CALCI20MODE1], nullIndPtr[TeChan.CALCI20MODE1]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "calci01mode1", parameterValuePtr[TeChan.CALCI01MODE1], nullIndPtr[TeChan.CALCI01MODE1]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "calci01mode2", parameterValuePtr[TeChan.CALCI01MODE2], nullIndPtr[TeChan.CALCI01MODE2]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "reqd20mode1", parameterValuePtr[TeChan.REQD20MODE1], nullIndPtr[TeChan.REQD20MODE1]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "reqd01mode1", parameterValuePtr[TeChan.REQD01MODE1], nullIndPtr[TeChan.REQD01MODE1]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "reqd01mode2", parameterValuePtr[TeChan.REQD01MODE2], nullIndPtr[TeChan.REQD01MODE2]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "marg20mode1", parameterValuePtr[TeChan.MARG20MODE1], nullIndPtr[TeChan.MARG20MODE1]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "marg01mode1", parameterValuePtr[TeChan.MARG01MODE1], nullIndPtr[TeChan.MARG01MODE1]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "marg01mode2", parameterValuePtr[TeChan.MARG01MODE2], nullIndPtr[TeChan.MARG01MODE2]);
                Ssutil.DbBindStringInput(hTECStmt, 0, "remterracode", parameterValuePtr[TeChan.REMTERRACODE], TeChan.REMTERRACODE_SZ, nullIndPtr[TeChan.REMTERRACODE]);
                Ssutil.DbBindDoubleInput(hTECStmt, 0, "remterragain", parameterValuePtr[TeChan.REMTERRAGAIN], nullIndPtr[TeChan.REMTERRAGAIN]);
                Ssutil.DbBindIntInput(hTECStmt, 0, "processed", parameterValuePtr[TeChan.PROCESSED], nullIndPtr[TeChan.PROCESSED]);
                Ssutil.DbBindShortInput(hTECStmt, 0, "terrant", parameterValuePtr[TeChan.TERRANT], nullIndPtr[TeChan.TERRANT]);

            }
            catch (Exception e)
            {
                Log2.e("\nTeDynChan.TeInsertChan(): ERROR: DbBind failed: " + e.Message);
                Ssutil.DbGetDiagStmt(hTECStmt, "teInsertChan02: Could not bind field: " + e.Message);
                return Error.ODBC_BINDING_FAILED;
            }

            sqlRet = ODBC.SQLExecute(hTECStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTeDynChan.TeInsertChan(): ERROR: SQLExecute() failed, returned: " + sqlRet);
                Ssutil.DbGetDiagStmt(hTECStmt, "teInsertChan03: Error executing.");
                return Error.ODBC_EXECUTE_FAILED;
            }

            return (Constant.SUCCESS);
        }








    }
}

```
