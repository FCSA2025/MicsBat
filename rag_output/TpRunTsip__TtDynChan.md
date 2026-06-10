# Documented File: TtDynChan.cs
**Repository Path:** `TpRunTsip\TtDynChan.cs`
**Primary Layer:** `TpRunTsip`
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
    using SQLHANDLE = IntPtr;
    using SQLRETURN = Int16;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;

    using System.Runtime.InteropServices;

    /// <summary>
    /// Provides methods to prepare and insert 
    /// records into the database table <b>&lt;userID&gt;.tt_&lt;tableName&gt;_&lt;runID&gt;_chan</b>
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item> The methods in this class are used to perform operations on the
    /// tt CHAN table. The actual CHAN table name is a variable, prescribed by the caller.</item>
    /// <item>The supported operations are: TtPrepareChan, TtInsertChan and TtChanClose.</item>
    /// <item>The caller must first call the method TtPrepareChan() that creates a cursor 
    /// for use with a subsequent call to TtInsertChan().</item>
    /// <item>The method closeChan must be called to release internal cursor resources (a cursor 
    /// encapsulates an ODBC connection and statement handle et. al.).</item>
    /// </list>
    /// </remarks>
    public class TtDynChan
    {
#if PINVOKE
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static void set_ttDynChan_ConnStmtHandles([In] SQLHANDLE hConnection, [In] SQLHANDLE hStatement);
#endif
        private static SQLHANDLE hConnInsert;
        private static SQLHANDLE hStmtInsert;

        private static SQLHANDLE hConnSelect;   //	Use different local variables for the read.
        private static SQLHANDLE hStmtSelect;


        /// <summary>
        /// This method closes the internal (private) cursor object by freeing the ODBC 
        /// connection and statement handles. 
        /// </summary>
        /// <param name=""></param> 
        public static void TtChanClose()
        {
            if (hStmtSelect != SQLHANDLE.Zero)
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmtSelect);
                hStmtSelect = SQLHANDLE.Zero;
                Ssutil.DisConn(hConnSelect);
                hConnSelect = SQLHANDLE.Zero;
            }

            if (hStmtInsert != SQLHANDLE.Zero)
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmtInsert);
                hStmtInsert = SQLHANDLE.Zero;
                Ssutil.DisConn(hConnInsert);
                hConnInsert = SQLHANDLE.Zero;
            }

            return;
        }

        /// <summary>
        /// This method closes the internal (private) cursor object by freeing the ODBC 
        /// connection and statement handles. 
        /// </summary>
        /// <param name=""></param> 
        public static void TtChanClose(SQLHANDLE hConn, SQLHANDLE hStmt)
        {
            if (hStmt != SQLHANDLE.Zero)
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                hStmt = SQLHANDLE.Zero;
                Ssutil.DisConn(hConn);
                hConn = SQLHANDLE.Zero;
            }

            return;
        }

        /// <summary>
        /// This method calls ODBC.SQLPrepare() to prepare a table insert on an internal (private) statement 
        /// handle that can then be used in a subsequent call to ODBC.SQLExecute to
        /// actually perform the SQL record insertion.
        /// </summary>
        /// <param name="table"> - full name of the DB table into which the record is to be inserted.</param>
        /// <returns></returns>
        public static int TtPrepareChan(string table)
        {
            string insert_buf;
            SQLRETURN sqlRet;

            insert_buf = String.Format("insert into {0} (interferer, intcall1, intcall2, intbndcde, intanum, intchid, viccall1,  viccall2, vicbndcde, vicanum, vicchid, intpolar, vicpolar, intstattx, vicstatrx, inttraftx, victrafrx,  inteqpttx, viceqptrx, intfreqtx, vicfreqrx, vicpwrrx, intpwrtx, intafsltx,  vicafslrx, rxant, txant, ctxinttraftx, ctxvictrafrx, ctxeqpt,  calctype, report, totantdisc, freqsep, reqdcalc, patloss, calcico, calcixp,  resti, eirpadv, tiltdisc,  pathloss80, calcico80, calcixp80, reqd80, resti80,  pathloss99, calcico99, calcixp99, reqd99, resti99,  ohresult, rqco,  processed, caseno, ctxinteqpt, inteqtype, viceqtype,  intbwchans, vicbwchans) values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", table);

            hConnInsert = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConnInsert, out hStmtInsert);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTtDynAnte.TtPrepareChan(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            sqlRet = ODBC.SQLPrepare(hStmtInsert, insert_buf, insert_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTtDynAnte.TtPrepareChan(): ERROR: call to SQLPrepare() failed.");
                Ssutil.DbGetDiagStmt(hStmtInsert, "ttPrepareChan01: Error preparing: " + insert_buf);
                return Error.ODBC_PREPARE_FAILED;
            }

            // AH: REMOVE when possible.
            // This passes hConn and hStmy handles instantiated in C# over to the 
            // associated C/C++ native code.
#if PINVOKE
            set_ttDynChan_ConnStmtHandles(hConnInsert, hStmtInsert);
#endif
            return (Constant.SUCCESS);
        }


        /// <summary>
        /// This method calls ODBC.SQLExecute() to perform an SQL 'SELECT' query 
        /// on an internal (private) ODBC statement handle that can then be used in a 
        /// subsequent call to ODBC.SQLFetch() to retrieve the selected records.        
        /// </summary>
        /// <param name="table"> - full name of the DB table to be read.</param>
        /// <param name="orderby"> - 'ORDER BY' clause in the SQL SELECT query.</param>
        /// <param name="cInWhere"> - 'WHERE' clause in the SQL SELECT query.</param>
        /// <param name="hConn"> - ODBC connection handle.</param>
        /// <param name="hStmt"> - ODBC statement handle.</param>
        /// <returns></returns>
        public static int TtChanPrepareRead(string table, string orderby, string cInWhere, out SQLHANDLE hConn, out SQLHANDLE hStmt)
        {
            // 'out' requirement.
            hConn = SQLHANDLE.Zero;
            hStmt = SQLHANDLE.Zero;

            string read_buf;

            string cOrder;
            string cWhere;

            SQLRETURN sqlRet;

            if (orderby == null || orderby.Equals(""))
            {
                cOrder = "";
            }
            else
            {
                cOrder = " ORDER BY ";
                cOrder += orderby;
            }

            if (cInWhere == null || cInWhere.Equals(""))
            {
                cWhere = "";
            }
            else
            {
                cWhere = " WHERE ";
                cWhere += cInWhere;
            }

            read_buf = String.Format("select interferer, intcall1, intcall2, intbndcde, intanum,intchid, viccall1, viccall2, vicbndcde, vicanum, vicchid,intpolar, vicpolar, intstattx,vicstatrx, inttraftx, victrafrx, inteqpttx, viceqptrx, intfreqtx, vicfreqrx,vicpwrrx, intpwrtx, intafsltx, vicafslrx, rxant, txant, ctxinttraftx,ctxvictrafrx, ctxeqpt, calctype, report, totantdisc, freqsep,reqdcalc, patloss, calcico, calcixp, resti, eirpadv,tiltdisc, pathloss80, calcico80, calcixp80, reqd80, resti80, pathloss99, calcico99, calcixp99, reqd99, resti99, ohresult, rqco, processed, caseno, ctxinteqpt, inteqtype, viceqtype, intbwchans, vicbwchans from {0}.{1} {2} {3} ",
                                               Info.GlobalSchema, table, cWhere, cOrder);

            hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTtDynChan.TtChanPrepareRead(): ERROR: SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            sqlRet = ODBC.SQLExecDirect(hStmt, read_buf, read_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    return Constant.NOMORERECS;
                }
                else
                {
                    Log2.e("\nTtDynChan.TtChanPrepareRead(): ERROR: SQLExecDirect() failed.");
                    Ssutil.DbGetDiagStmt(hStmt, "ttChanPrepareRead02: Read error:");
                    return Error.ODBC_EXECUTE_FAILED - 2;
                }
            }

            return (0);
        }

        /// <summary>
        /// This method performs an ODBC.SQLFetch() call on a statement handle 
        /// prepared in a previous call to TtChanPrepareRead().
        /// </summary>
        /// <param name="hStmt"> - ODBC statement handle.</param>
        /// <param name="pChan"> - TtChan object (record) to be read from the DB table.</param>
        /// <param name="nullInd"> - array of ODBC nullInds associated with pChan.</param>
        /// <returns></returns>
        public static int TtChanRead(SQLHANDLE hStmt, out TtChan pChan, out SQLLEN[] nullInd)
        {
            // 'out' requirements.
            pChan = null;
            nullInd = null;

            SQLRETURN sqlRet;

            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (ODBC.IsNoData(sqlRet))
                {
                    return ODBC.SQL_NO_DATA;
                }
                else
                {
                    Log2.e("\nTtDynChan.TtChanRead(): ERROR: SQLFetch() failed.");
                    Ssutil.DbGetDiagStmt(hStmt, "ttChanRead01: Could not fetch.");
                    return Error.ODBC_FETCH_FAILED;
                }
            }

            pChan = new TtChan();
            nullInd = NullHelper.CreateArrayOfNullInd(TtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            try
            {
                Ssutil.DbStartGets();
                Ssutil.DbGetString(hStmt, 0, "interferer", out pChan.interferer, TtChan.INTERFERER_SZ, out nullInd[TtChan.INTERFERER]);
                Ssutil.DbGetString(hStmt, 0, "intcall1", out pChan.intcall1, TtChan.INTCALL1_SZ, out nullInd[TtChan.INTCALL1]);
                Ssutil.DbGetString(hStmt, 0, "intcall2", out pChan.intcall2, TtChan.INTCALL2_SZ, out nullInd[TtChan.INTCALL2]);
                Ssutil.DbGetString(hStmt, 0, "intbndcde", out pChan.intbndcde, TtChan.INTBNDCDE_SZ, out nullInd[TtChan.INTBNDCDE]);
                Ssutil.DbGetShort(hStmt, 0, "intanum", out pChan.intanum, out nullInd[TtChan.INTANUM]);
                Ssutil.DbGetString(hStmt, 0, "intchid", out pChan.intchid, TtChan.INTCHID_SZ, out nullInd[TtChan.INTCHID]);
                Ssutil.DbGetString(hStmt, 0, "viccall1", out pChan.viccall1, TtChan.VICCALL1_SZ, out nullInd[TtChan.VICCALL1]);
                Ssutil.DbGetString(hStmt, 0, "viccall2", out pChan.viccall2, TtChan.VICCALL2_SZ, out nullInd[TtChan.VICCALL2]);
                Ssutil.DbGetString(hStmt, 0, "vicbndcde", out pChan.vicbndcde, TtChan.VICBNDCDE_SZ, out nullInd[TtChan.VICBNDCDE]);
                Ssutil.DbGetShort(hStmt, 0, "vicanum", out pChan.vicanum, out nullInd[TtChan.VICANUM]);
                Ssutil.DbGetString(hStmt, 0, "vicchid", out pChan.vicchid, TtChan.VICCHID_SZ, out nullInd[TtChan.VICCHID]);
                Ssutil.DbGetString(hStmt, 0, "intpolar", out pChan.intpolar, TtChan.INTPOLAR_SZ, out nullInd[TtChan.INTPOLAR]);
                Ssutil.DbGetString(hStmt, 0, "vicpolar", out pChan.vicpolar, TtChan.VICPOLAR_SZ, out nullInd[TtChan.VICPOLAR]);
                Ssutil.DbGetString(hStmt, 0, "intstattx", out pChan.intstattx, TtChan.INTSTATTX_SZ, out nullInd[TtChan.INTSTATTX]);
                Ssutil.DbGetString(hStmt, 0, "vicstatrx", out pChan.vicstatrx, TtChan.VICSTATRX_SZ, out nullInd[TtChan.VICSTATRX]);
                Ssutil.DbGetString(hStmt, 0, "inttraftx", out pChan.inttraftx, TtChan.INTTRAFTX_SZ, out nullInd[TtChan.INTTRAFTX]);
                Ssutil.DbGetString(hStmt, 0, "victrafrx", out pChan.victrafrx, TtChan.VICTRAFRX_SZ, out nullInd[TtChan.VICTRAFRX]);
                Ssutil.DbGetString(hStmt, 0, "inteqpttx", out pChan.inteqpttx, TtChan.INTEQPTTX_SZ, out nullInd[TtChan.INTEQPTTX]);
                Ssutil.DbGetString(hStmt, 0, "viceqptrx", out pChan.viceqptrx, TtChan.VICEQPTRX_SZ, out nullInd[TtChan.VICEQPTRX]);
                Ssutil.DbGetDouble(hStmt, 0, "intfreqtx", out pChan.intfreqtx, out nullInd[TtChan.INTFREQTX]);
                Ssutil.DbGetDouble(hStmt, 0, "vicfreqrx", out pChan.vicfreqrx, out nullInd[TtChan.VICFREQRX]);
                Ssutil.DbGetDouble(hStmt, 0, "vicpwrrx", out pChan.vicpwrrx, out nullInd[TtChan.VICPWRRX]);
                Ssutil.DbGetDouble(hStmt, 0, "intpwrtx", out pChan.intpwrtx, out nullInd[TtChan.INTPWRTX]);
                Ssutil.DbGetDouble(hStmt, 0, "intafsltx", out pChan.intafsltx, out nullInd[TtChan.INTAFSLTX]);
                Ssutil.DbGetDouble(hStmt, 0, "vicafslrx", out pChan.vicafslrx, out nullInd[TtChan.VICAFSLRX]);
                Ssutil.DbGetShort(hStmt, 0, "rxant", out pChan.rxant, out nullInd[TtChan.RXANT]);
                Ssutil.DbGetShort(hStmt, 0, "txant", out pChan.txant, out nullInd[TtChan.TXANT]);
                Ssutil.DbGetString(hStmt, 0, "ctxinttraftx", out pChan.ctxinttraftx, TtChan.CTXINTTRAFTX_SZ, out nullInd[TtChan.CTXINTTRAFTX]);
                Ssutil.DbGetString(hStmt, 0, "ctxvictrafrx", out pChan.ctxvictrafrx, TtChan.CTXVICTRAFRX_SZ, out nullInd[TtChan.CTXVICTRAFRX]);
                Ssutil.DbGetString(hStmt, 0, "ctxeqpt", out pChan.ctxeqpt, TtChan.CTXEQPT_SZ, out nullInd[TtChan.CTXEQPT]);
                Ssutil.DbGetString(hStmt, 0, "calctype", out pChan.calctype, TtChan.CALCTYPE_SZ, out nullInd[TtChan.CALCTYPE]);
                Ssutil.DbGetShort(hStmt, 0, "report", out pChan.report, out nullInd[TtChan.REPORT]);
                Ssutil.DbGetDouble(hStmt, 0, "totantdisc", out pChan.totantdisc, out nullInd[TtChan.TOTANTDISC]);
                Ssutil.DbGetDouble(hStmt, 0, "freqsep", out pChan.freqsep, out nullInd[TtChan.FREQSEP]);
                Ssutil.DbGetDouble(hStmt, 0, "reqdcalc", out pChan.reqdcalc, out nullInd[TtChan.REQDCALC]);
                Ssutil.DbGetDouble(hStmt, 0, "patloss", out pChan.patloss, out nullInd[TtChan.PATLOSS]);
                Ssutil.DbGetDouble(hStmt, 0, "calcico", out pChan.calcico, out nullInd[TtChan.CALCICO]);
                Ssutil.DbGetDouble(hStmt, 0, "calcixp", out pChan.calcixp, out nullInd[TtChan.CALCIXP]);
                Ssutil.DbGetDouble(hStmt, 0, "resti", out pChan.resti, out nullInd[TtChan.RESTI]);
                Ssutil.DbGetDouble(hStmt, 0, "eirpadv", out pChan.eirpadv, out nullInd[TtChan.EIRPADV]);
                Ssutil.DbGetDouble(hStmt, 0, "tiltdisc", out pChan.tiltdisc, out nullInd[TtChan.TILTDISC]);
                Ssutil.DbGetDouble(hStmt, 0, "pathloss80", out pChan.pathloss80, out nullInd[TtChan.PATHLOSS80]);
                Ssutil.DbGetDouble(hStmt, 0, "calcico80", out pChan.calcico80, out nullInd[TtChan.CALCICO80]);
                Ssutil.DbGetDouble(hStmt, 0, "calcixp80", out pChan.calcixp80, out nullInd[TtChan.CALCIXP80]);
                Ssutil.DbGetDouble(hStmt, 0, "reqd80", out pChan.reqd80, out nullInd[TtChan.REQD80]);
                Ssutil.DbGetDouble(hStmt, 0, "resti80", out pChan.resti80, out nullInd[TtChan.RESTI80]);
                Ssutil.DbGetDouble(hStmt, 0, "pathloss99", out pChan.pathloss99, out nullInd[TtChan.PATHLOSS99]);
                Ssutil.DbGetDouble(hStmt, 0, "calcico99", out pChan.calcico99, out nullInd[TtChan.CALCICO99]);
                Ssutil.DbGetDouble(hStmt, 0, "calcixp99", out pChan.calcixp99, out nullInd[TtChan.CALCIXP99]);
                Ssutil.DbGetDouble(hStmt, 0, "reqd99", out pChan.reqd99, out nullInd[TtChan.REQD99]);
                Ssutil.DbGetDouble(hStmt, 0, "resti99", out pChan.resti99, out nullInd[TtChan.RESTI99]);
                Ssutil.DbGetInt(hStmt, 0, "ohresult", out pChan.ohresult, out nullInd[TtChan.OHRESULT]);
                Ssutil.DbGetDouble(hStmt, 0, "rqco", out pChan.rqco, out nullInd[TtChan.RQCO]);
                Ssutil.DbGetInt(hStmt, 0, "processed", out pChan.processed, out nullInd[TtChan.PROCESSED]);
                Ssutil.DbGetInt(hStmt, 0, "caseno", out pChan.caseno, out nullInd[TtChan.CASENO]);
                Ssutil.DbGetString(hStmt, 0, "ctxinteqpt", out pChan.ctxinteqpt, TtChan.CTXINTEQPT_SZ, out nullInd[TtChan.CTXINTEQPT]);
                Ssutil.DbGetString(hStmt, 0, "inteqtype", out pChan.inteqtype, TtChan.INTEQTYPE_SZ, out nullInd[TtChan.INTEQTYPE]);
                Ssutil.DbGetString(hStmt, 0, "viceqtype", out pChan.viceqtype, TtChan.VICEQTYPE_SZ, out nullInd[TtChan.VICEQTYPE]);
                Ssutil.DbGetDouble(hStmt, 0, "intbwchans", out pChan.intbwchans, out nullInd[TtChan.INTBWCHANS]);
                Ssutil.DbGetDouble(hStmt, 0, "vicbwchans", out pChan.vicbwchans, out nullInd[TtChan.VICBWCHANS]);
            }
            catch (Exception e)
            {
                Log2.e("\nTtDynChan.TtChanRead(): ERROR: ODBC 'Get' request failed: " + e.Message);
                Ssutil.DbGetDiagStmt(hStmt, "ttChanRead02: Get column error for " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            pChan.calctype.Trim();  //	length is used as a marker.

            return 0;
        }

        /// <summary>
        /// This method calls ODBC.SQLExecute() to perform insertion of a record
        /// into a DB table by using the internal (private) ODBC statement handle
        /// that was prepared on by a previous call to ODBC.SQLPrepare().
        /// </summary>
        /// <param name="insertStruct"> - TtChan object (record) to be inserted into the DB table.</param>
        /// <param name="nullInd"> - array of ODBC nullInds associated with ttChan.</param>
        /// <returns></returns>
        public static int TtInsertChan(TtChan insertStruct, SQLLEN[] nullInd)
        {
            //...Log2.v("\nttInsertChan(): Entry");

            SQLRETURN sqlRet;

            //...Log2.v("\nTtDynChan.TtInsertChan():\n" + insertStruct.ToStringWN(nullInd));

            // Check that the static hStmt has been instanciated (by a previous 
            // call to TtPrepareChan()).
            if (hStmtInsert == null)
            {
                Log2.e("\nTtDynChan.TtInsertChan(): ERROR: hStmt is null.");
                return Error.ODBC_NULL_HANDLE;
            }

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLPOINTER[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            //into global memory with an SQLPOINTER pointer assigned to each one. TtChan provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = insertStruct.CopyToArrayOfSQLPOINTERs();

            try
            {
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(hStmtInsert, 0, "interferer", parameterValuePtr[TtChan.INTERFERER], TtChan.INTERFERER_SZ, nullIndPtr[TtChan.INTERFERER]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "intcall1", parameterValuePtr[TtChan.INTCALL1], TtChan.INTCALL1_SZ, nullIndPtr[TtChan.INTCALL1]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "intcall2", parameterValuePtr[TtChan.INTCALL2], TtChan.INTCALL2_SZ, nullIndPtr[TtChan.INTCALL2]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "intbndcde", parameterValuePtr[TtChan.INTBNDCDE], TtChan.INTBNDCDE_SZ, nullIndPtr[TtChan.INTBNDCDE]);

                Ssutil.DbBindShortInput(hStmtInsert, 0, "intanum", parameterValuePtr[TtChan.INTANUM], nullIndPtr[TtChan.INTANUM]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "intchid", parameterValuePtr[TtChan.INTCHID], TtChan.INTCHID_SZ, nullIndPtr[TtChan.INTCHID]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "viccall1", parameterValuePtr[TtChan.VICCALL1], TtChan.VICCALL1_SZ, nullIndPtr[TtChan.VICCALL1]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "viccall2", parameterValuePtr[TtChan.VICCALL2], TtChan.VICCALL2_SZ, nullIndPtr[TtChan.VICCALL2]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "vicbndcde", parameterValuePtr[TtChan.VICBNDCDE], TtChan.VICBNDCDE_SZ, nullIndPtr[TtChan.VICBNDCDE]);

                Ssutil.DbBindShortInput(hStmtInsert, 0, "vicanum", parameterValuePtr[TtChan.VICANUM], nullIndPtr[TtChan.VICANUM]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "vicchid", parameterValuePtr[TtChan.VICCHID], TtChan.VICCHID_SZ, nullIndPtr[TtChan.VICCHID]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "intpolar", parameterValuePtr[TtChan.INTPOLAR], TtChan.INTPOLAR_SZ, nullIndPtr[TtChan.INTPOLAR]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "vicpolar", parameterValuePtr[TtChan.VICPOLAR], TtChan.VICPOLAR_SZ, nullIndPtr[TtChan.VICPOLAR]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "intstattx", parameterValuePtr[TtChan.INTSTATTX], TtChan.INTSTATTX_SZ, nullIndPtr[TtChan.INTSTATTX]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "vicstatrx", parameterValuePtr[TtChan.VICSTATRX], TtChan.VICSTATRX_SZ, nullIndPtr[TtChan.VICSTATRX]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "inttraftx", parameterValuePtr[TtChan.INTTRAFTX], TtChan.INTTRAFTX_SZ, nullIndPtr[TtChan.INTTRAFTX]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "victrafrx", parameterValuePtr[TtChan.VICTRAFRX], TtChan.VICTRAFRX_SZ, nullIndPtr[TtChan.VICTRAFRX]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "inteqpttx", parameterValuePtr[TtChan.INTEQPTTX], TtChan.INTEQPTTX_SZ, nullIndPtr[TtChan.INTEQPTTX]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "viceqptrx", parameterValuePtr[TtChan.VICEQPTRX], TtChan.VICEQPTRX_SZ, nullIndPtr[TtChan.VICEQPTRX]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "intfreqtx", parameterValuePtr[TtChan.INTFREQTX], nullIndPtr[TtChan.INTFREQTX]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "vicfreqrx", parameterValuePtr[TtChan.VICFREQRX], nullIndPtr[TtChan.VICFREQRX]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "vicpwrrx", parameterValuePtr[TtChan.VICPWRRX], nullIndPtr[TtChan.VICPWRRX]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "intpwrtx", parameterValuePtr[TtChan.INTPWRTX], nullIndPtr[TtChan.INTPWRTX]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "intafsltx", parameterValuePtr[TtChan.INTAFSLTX], nullIndPtr[TtChan.INTAFSLTX]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "vicafslrx", parameterValuePtr[TtChan.VICAFSLRX], nullIndPtr[TtChan.VICAFSLRX]);

                Ssutil.DbBindShortInput(hStmtInsert, 0, "rxant", parameterValuePtr[TtChan.RXANT], nullIndPtr[TtChan.RXANT]);

                Ssutil.DbBindShortInput(hStmtInsert, 0, "txant", parameterValuePtr[TtChan.TXANT], nullIndPtr[TtChan.TXANT]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "ctxinttraftx", parameterValuePtr[TtChan.CTXINTTRAFTX], TtChan.CTXINTTRAFTX_SZ, nullIndPtr[TtChan.CTXINTTRAFTX]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "ctxvictrafrx", parameterValuePtr[TtChan.CTXVICTRAFRX], TtChan.CTXVICTRAFRX_SZ, nullIndPtr[TtChan.CTXVICTRAFRX]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "ctxeqpt", parameterValuePtr[TtChan.CTXEQPT], TtChan.CTXEQPT_SZ, nullIndPtr[TtChan.CTXEQPT]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "calctype", parameterValuePtr[TtChan.CALCTYPE], TtChan.CALCTYPE_SZ, nullIndPtr[TtChan.CALCTYPE]);

                Ssutil.DbBindShortInput(hStmtInsert, 0, "report", parameterValuePtr[TtChan.REPORT], nullIndPtr[TtChan.REPORT]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "totantdisc", parameterValuePtr[TtChan.TOTANTDISC], nullIndPtr[TtChan.TOTANTDISC]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "freqsep", parameterValuePtr[TtChan.FREQSEP], nullIndPtr[TtChan.FREQSEP]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "reqdcalc", parameterValuePtr[TtChan.REQDCALC], nullIndPtr[TtChan.REQDCALC]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "patloss", parameterValuePtr[TtChan.PATLOSS], nullIndPtr[TtChan.PATLOSS]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "calcico", parameterValuePtr[TtChan.CALCICO], nullIndPtr[TtChan.CALCICO]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "calcixp", parameterValuePtr[TtChan.CALCIXP], nullIndPtr[TtChan.CALCIXP]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "resti", parameterValuePtr[TtChan.RESTI], nullIndPtr[TtChan.RESTI]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "eirpadv", parameterValuePtr[TtChan.EIRPADV], nullIndPtr[TtChan.EIRPADV]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "tiltdisc", parameterValuePtr[TtChan.TILTDISC], nullIndPtr[TtChan.TILTDISC]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "pathloss80", parameterValuePtr[TtChan.PATHLOSS80], nullIndPtr[TtChan.PATHLOSS80]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "calcico80", parameterValuePtr[TtChan.CALCICO80], nullIndPtr[TtChan.CALCICO80]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "calcixp80", parameterValuePtr[TtChan.CALCIXP80], nullIndPtr[TtChan.CALCIXP80]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "reqd80", parameterValuePtr[TtChan.REQD80], nullIndPtr[TtChan.REQD80]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "resti80", parameterValuePtr[TtChan.RESTI80], nullIndPtr[TtChan.RESTI80]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "pathloss99", parameterValuePtr[TtChan.PATHLOSS99], nullIndPtr[TtChan.PATHLOSS99]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "calcico99", parameterValuePtr[TtChan.CALCICO99], nullIndPtr[TtChan.CALCICO99]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "calcixp99", parameterValuePtr[TtChan.CALCIXP99], nullIndPtr[TtChan.CALCIXP99]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "reqd99", parameterValuePtr[TtChan.REQD99], nullIndPtr[TtChan.REQD99]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "resti99", parameterValuePtr[TtChan.RESTI99], nullIndPtr[TtChan.RESTI99]);

                Ssutil.DbBindIntInput(hStmtInsert, 0, "ohresult", parameterValuePtr[TtChan.OHRESULT], nullIndPtr[TtChan.OHRESULT]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "rqco", parameterValuePtr[TtChan.RQCO], nullIndPtr[TtChan.RQCO]);

                Ssutil.DbBindIntInput(hStmtInsert, 0, "processed", parameterValuePtr[TtChan.PROCESSED], nullIndPtr[TtChan.PROCESSED]);

                Ssutil.DbBindIntInput(hStmtInsert, 0, "caseno", parameterValuePtr[TtChan.CASENO], nullIndPtr[TtChan.CASENO]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "ctxinteqpt", parameterValuePtr[TtChan.CTXINTEQPT], TtChan.CTXINTEQPT_SZ, nullIndPtr[TtChan.CTXINTEQPT]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "inteqtype", parameterValuePtr[TtChan.INTEQTYPE], TtChan.INTEQTYPE_SZ, nullIndPtr[TtChan.INTEQTYPE]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "viceqtype", parameterValuePtr[TtChan.VICEQTYPE], TtChan.VICEQTYPE_SZ, nullIndPtr[TtChan.VICEQTYPE]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "intbwchans", parameterValuePtr[TtChan.INTBWCHANS], nullIndPtr[TtChan.INTBWCHANS]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "vicbwchans", parameterValuePtr[TtChan.VICBWCHANS], nullIndPtr[TtChan.VICBWCHANS]);

            }
            catch (Exception e)
            {
                Log2.e("\nTtDynChan.TtInsertChan(): ERROR: ODBC binding attempt failed:  " + e.Message);
                Ssutil.DbGetDiagStmt(hStmtInsert, "ttInsertChan02: Problem binding column " + e.Message);
                return Error.ODBC_BINDING_FAILED;
            }

            sqlRet = ODBC.SQLExecute(hStmtInsert);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTtDynChan.TtInsertChan(): ERROR: call to SQLExecute() failed.");
                return Error.ODBC_EXECUTE_FAILED;
            }

            //...Log2.v("\nttInsertChan(): Exit");
            return (Constant.SUCCESS);
        }





    }
}

```
