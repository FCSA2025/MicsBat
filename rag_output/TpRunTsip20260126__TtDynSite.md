# Documented File: TtDynSite.cs
**Repository Path:** `TpRunTsip20260126\TtDynSite.cs`
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
    /// records into the database table <b>&lt;userID&gt;.tt_&lt;tableName&gt;_&lt;runID&gt;_site</b>
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item> The methods in this class are used to perform operations on the
    /// tt SITE table. The actual SITE table name is a variable, prescribed by the caller.</item>
    /// <item>The supported operations are: TtPrepareSite, TtInsertSite and TtSiteClose.</item>
    /// <item>The caller must first call the method TtPrepareSite() that creates a cursor 
    /// for use with a subsequent call to TtInsertSite().</item>
    /// <item>The method closeSite must be called to release internal cursor resources (a cursor 
    /// encapsulates an ODBC connection and statement handle et. al.).</item>
    /// </list>
    /// </remarks>
    public class TtDynSite
    {
#if PINVOKE
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static void set_ttDynSite_ConnStmtHandles([In] SQLHANDLE hConnection, [In] SQLHANDLE hStatement);
#endif

        private static SQLHANDLE hConn;
        private static SQLHANDLE hStmtInsert;

        private static SQLHANDLE hTTConn;   //	Use different local variables for the read.
        private static SQLHANDLE hTTStmt;

        /// <summary>
        /// This method closes the internal (private) cursor object by freeing the ODBC 
        /// connection and statement handles. 
        /// </summary>
        /// <param name=""></param> 
        public static void TtSiteClose()
        {
            if (hTTStmt != SQLHANDLE.Zero)
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hTTStmt);
                hTTStmt = SQLHANDLE.Zero;
                Ssutil.DisConn(hTTConn);
                hTTConn = SQLHANDLE.Zero;
            }
            if (hStmtInsert != SQLHANDLE.Zero)
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmtInsert);
                hStmtInsert = SQLHANDLE.Zero;
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
        public static int TtPrepareSite(string table)
        {
            string insert_buf;
            SQLRETURN sqlRet;

            insert_buf = String.Format("insert into {0} (interferer, intcall1, intcall2, viccall1, viccall2, intname1, intname2, vicname1, vicname2, intoper, intoper2, vicoper, vicoper2, intlatit, intlongit, intgrnd, viclatit, viclongit, vicgrnd, report, caseno, subcases, int1int2dist, vic1vic2dist, int1vic1dist, distadv, intoffax, vicoffax, intvicaz, vicintaz, processed) values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", table);

            hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmtInsert);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTtDynSite.TtPrepareSite(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            sqlRet = ODBC.SQLPrepare(hStmtInsert, insert_buf, insert_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTtDynSite.TtPrepareSite(): ERROR: call to SQLPrepare() failed:\n" + insert_buf);
                Ssutil.DbGetDiagStmt(hStmtInsert, "ttPrepareSite03: Failure to Prepare:-\n" + insert_buf);
                return Error.ODBC_PREPARE_FAILED;
            }

            // AH: REMOVE when possible.
            // This passes hConn and hStmy handles instantiated in C# over to the 
            // associated C/C++ native code.
#if PINVOKE
            set_ttDynSite_ConnStmtHandles(hConn, hStmtInsert);
#endif

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method calls ODBC.SQLExecute() to perform insertion of a record
        /// into a DB table by using the internal (private) ODBC statement handle
        /// that was prepared on by a previous call to ODBC.SQLPrepare().
        /// </summary>
        /// <param name="ttSite"> - TtSite object (record) to be inserted into the DB table.</param>
        /// <param name="nullInd"> - array of ODBC nullInds associated with ttSite.</param>
        /// <returns></returns>
        public static int TtInsertSite(TtSite ttSite, SQLLEN[] nullInd)
        {
            SQLRETURN sqlRet = Constant.FAILURE;

            // Check that the static hStmt has been instanciated (by a previous call to TtPrepareSite()).
            if (hStmtInsert == null)
            {
                Log2.e("\nTtDynSite.TtInsertSite(): ERROR: hStmt is null.");
                return Error.ODBC_NULL_HANDLE;
            }

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            //into global memory with an SQLPOINTER pointer assigned to each one. FtAnte provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = ttSite.CopyToArrayOfSQLPOINTERs();

            try
            {
                // Initialize auto-column numbering.
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(hStmtInsert, 0, "interferer", parameterValuePtr[TtSite.INTERFERER], TtSite.INTERFERER_SZ, nullIndPtr[TtSite.INTERFERER]);
                Ssutil.DbBindStringInput(hStmtInsert, 0, "intcall1", parameterValuePtr[TtSite.INTCALL1], TtSite.INTCALL1_SZ, nullIndPtr[TtSite.INTCALL1]);
                Ssutil.DbBindStringInput(hStmtInsert, 0, "intcall2", parameterValuePtr[TtSite.INTCALL2], TtSite.INTCALL2_SZ, nullIndPtr[TtSite.INTCALL2]);
                Ssutil.DbBindStringInput(hStmtInsert, 0, "viccall1", parameterValuePtr[TtSite.VICCALL1], TtSite.VICCALL1_SZ, nullIndPtr[TtSite.VICCALL1]);
                Ssutil.DbBindStringInput(hStmtInsert, 0, "viccall2", parameterValuePtr[TtSite.VICCALL2], TtSite.VICCALL2_SZ, nullIndPtr[TtSite.VICCALL2]);
                Ssutil.DbBindStringInput(hStmtInsert, 0, "intname1", parameterValuePtr[TtSite.INTNAME1], TtSite.INTNAME1_SZ, nullIndPtr[TtSite.INTNAME1]);
                Ssutil.DbBindStringInput(hStmtInsert, 0, "intname2", parameterValuePtr[TtSite.INTNAME2], TtSite.INTNAME2_SZ, nullIndPtr[TtSite.INTNAME2]);
                Ssutil.DbBindStringInput(hStmtInsert, 0, "vicname1", parameterValuePtr[TtSite.VICNAME1], TtSite.VICNAME1_SZ, nullIndPtr[TtSite.VICNAME1]);
                Ssutil.DbBindStringInput(hStmtInsert, 0, "vicname2", parameterValuePtr[TtSite.VICNAME2], TtSite.VICNAME2_SZ, nullIndPtr[TtSite.VICNAME2]);
                Ssutil.DbBindStringInput(hStmtInsert, 0, "intoper", parameterValuePtr[TtSite.INTOPER], TtSite.INTOPER_SZ, nullIndPtr[TtSite.INTOPER]);
                Ssutil.DbBindStringInput(hStmtInsert, 0, "intoper2", parameterValuePtr[TtSite.INTOPER2], TtSite.INTOPER2_SZ, nullIndPtr[TtSite.INTOPER2]);
                Ssutil.DbBindStringInput(hStmtInsert, 0, "vicoper", parameterValuePtr[TtSite.VICOPER], TtSite.VICOPER_SZ, nullIndPtr[TtSite.VICOPER]);
                Ssutil.DbBindStringInput(hStmtInsert, 0, "vicoper2", parameterValuePtr[TtSite.VICOPER2], TtSite.VICOPER2_SZ, nullIndPtr[TtSite.VICOPER2]);
                Ssutil.DbBindIntInput(hStmtInsert, 0, "intlatit", parameterValuePtr[TtSite.INTLATIT], nullIndPtr[TtSite.INTLATIT]);
                Ssutil.DbBindIntInput(hStmtInsert, 0, "intlongit", parameterValuePtr[TtSite.INTLONGIT], nullIndPtr[TtSite.INTLONGIT]);
                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "intgrnd", parameterValuePtr[TtSite.INTGRND], nullIndPtr[TtSite.INTGRND]);
                Ssutil.DbBindIntInput(hStmtInsert, 0, "viclatit", parameterValuePtr[TtSite.VICLATIT], nullIndPtr[TtSite.VICLATIT]);
                Ssutil.DbBindIntInput(hStmtInsert, 0, "viclongit", parameterValuePtr[TtSite.VICLONGIT], nullIndPtr[TtSite.VICLONGIT]);
                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "vicgrnd", parameterValuePtr[TtSite.VICGRND], nullIndPtr[TtSite.VICGRND]);
                Ssutil.DbBindShortInput(hStmtInsert, 0, "report", parameterValuePtr[TtSite.REPORT], nullIndPtr[TtSite.REPORT]);
                Ssutil.DbBindIntInput(hStmtInsert, 0, "caseno", parameterValuePtr[TtSite.CASENO], nullIndPtr[TtSite.CASENO]);
                Ssutil.DbBindIntInput(hStmtInsert, 0, "subcases", parameterValuePtr[TtSite.SUBCASES], nullIndPtr[TtSite.SUBCASES]);
                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "int1int2dist", parameterValuePtr[TtSite.INT1INT2DIST], nullIndPtr[TtSite.INT1INT2DIST]);
                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "vic1vic2dist", parameterValuePtr[TtSite.VIC1VIC2DIST], nullIndPtr[TtSite.VIC1VIC2DIST]);
                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "int1vic1dist", parameterValuePtr[TtSite.INT1VIC1DIST], nullIndPtr[TtSite.INT1VIC1DIST]);
                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "distadv", parameterValuePtr[TtSite.DISTADV], nullIndPtr[TtSite.DISTADV]);
                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "intoffax", parameterValuePtr[TtSite.INTOFFAX], nullIndPtr[TtSite.INTOFFAX]);
                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "vicoffax", parameterValuePtr[TtSite.VICOFFAX], nullIndPtr[TtSite.VICOFFAX]);
                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "intvicaz", parameterValuePtr[TtSite.INTVICAZ], nullIndPtr[TtSite.INTVICAZ]);
                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "vicintaz", parameterValuePtr[TtSite.VICINTAZ], nullIndPtr[TtSite.VICINTAZ]);
                Ssutil.DbBindIntInput(hStmtInsert, 0, "processed", parameterValuePtr[TtSite.PROCESSED], nullIndPtr[TtSite.PROCESSED]);
            }
            catch (Exception e)
            {
                Log2.e("\nTtDynSite.TtInsertSite(): ERROR: ODBC parameter binding attempt failed: " + e.Message);
                Ssutil.DbGetDiagStmt(hStmtInsert, "ttInsertSite02: Problem binding parameter: " + e.Message);
                return Error.ODBC_BINDING_FAILED;
            }

            sqlRet = ODBC.SQLExecute(hStmtInsert);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTtDynSite.TtInsertSite(): ERROR: ODBC SQLExecute() failed.");
                Ssutil.DbGetDiagStmt(hStmtInsert, "ttInsertSite03: Failure to Execute.");
                return Error.ODBC_EXECUTE_FAILED;
            }

            return (Constant.SUCCESS);
        }






    }
}

```
