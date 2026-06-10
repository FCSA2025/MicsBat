# Documented File: TtDynAnte.cs
**Repository Path:** `TpRunTsip\TtDynAnte.cs`
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
    /// records into the database table <b>&lt;userID&gt;.tt_&lt;tableName&gt;_&lt;runID&gt;_ante</b>
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item> The methods in this class are used to perform operations on the
    /// tt ANTE table. The actual ANTE table name is a variable, prescribed by the caller.</item>
    /// <item>The supported operations are: TtPrepareAnte, TtInsertAnte and TtAnteClose.</item>
    /// <item>The caller must first call the method TtPrepareAnte() that creates a cursor 
    /// for use with a subsequent call to TtInsertAnte().</item>
    /// <item>The method closeSite must be called to release internal cursor resources (a cursor 
    /// encapsulates an ODBC connection and statement handle et. al.).</item>
    /// </list>
    /// </remarks>public class TtDynAnte
    public class TtDynAnte
    {
#if PINVOKE
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static void set_ttDynAnte_ConnStmtHandles([In] SQLHANDLE hConnection, [In] SQLHANDLE hStatement);
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
        public static void TtCloseAnte()
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
        public static int TtPrepareAnte(string table)
        {
            string insert_buf;
            SQLRETURN sqlRet;

            insert_buf = String.Format("insert into {0} (interferer, intcall1, intcall2, intbndcde, intanum, viccall1, viccall2, vicbndcde, vicanum, intacode, vicacode, report, subcaseno, adiscctxh, adiscctxv, adisccrxh, adisccrxv, adiscxtxh, adiscxtxv, adiscxrxh, adiscxrxv, processed,intause, vicause, intgain, vicgain, intaxref, intamodel, vicaxref, vicamodel, intaoffax, inthopaz, intantaz, intoffantax, vicaoffax,	vichopaz,	vicantaz,	vicoffantax, intaht, vicaht, intvicel, vicintel, intelev, vicelev, caseno) values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", table);

            hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmtInsert);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTtDynAnte.TtPrepareAnte(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            sqlRet = ODBC.SQLPrepare(hStmtInsert, insert_buf, insert_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTtDynAnte.TtPrepareAnte(): ERROR: call to SQLPrepare() failed.");
                Ssutil.DbGetDiagStmt(hStmtInsert, "ttPrepareAnte01: Error preparing: " + insert_buf);
                return Error.ODBC_PREPARE_FAILED;
            }

            // AH: REMOVE when possible.
            // This passes hConn and hStmy handles instantiated in C# over to the 
            // associated C/C++ native code.
#if PINVOKE
            set_ttDynAnte_ConnStmtHandles(hConn, hStmtInsert);
#endif

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method calls ODBC.SQLExecute() to perform insertion of a record
        /// into a DB table by using the internal (private) ODBC statement handle
        /// that was prepared on by a previous call to ODBC.SQLPrepare().
        /// </summary>
        /// <param name="insertStruct"> - TtAnte object (record) to be inserted into the DB table.</param>
        /// <param name="nullInd"> - array of ODBC nullInds associated with ttAnte.</param>
        /// <returns></returns>
        public static int TtInsertAnte(TtAnte insertStruct, SQLLEN[] nullInd)
        {
            SQLRETURN sqlRet = Constant.FAILURE;

            // Check that the static hStmt has been instanciated (by a previous 
            // call to TtPrepareSite()).
            if (hStmtInsert == null)
            {
                Log2.e("\nTtDynAnte.TtInsertAnte(): ERROR: hStmt is null.");
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
            SQLPOINTER[] parameterValuePtr = insertStruct.CopyToArrayOfSQLPOINTERs();

            try
            {
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(hStmtInsert, 0, "interferer", parameterValuePtr[TtAnte.INTERFERER], TtAnte.INTERFERER_SZ, nullIndPtr[TtAnte.INTERFERER]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "intcall1", parameterValuePtr[TtAnte.INTCALL1], TtAnte.INTCALL1_SZ, nullIndPtr[TtAnte.INTCALL1]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "intcall2", parameterValuePtr[TtAnte.INTCALL2], TtAnte.INTCALL2_SZ, nullIndPtr[TtAnte.INTCALL2]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "intbndcde", parameterValuePtr[TtAnte.INTBNDCDE], TtAnte.INTBNDCDE_SZ, nullIndPtr[TtAnte.INTBNDCDE]);

                Ssutil.DbBindShortInput(hStmtInsert, 0, "intanum", parameterValuePtr[TtAnte.INTANUM], nullIndPtr[TtAnte.INTANUM]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "viccall1", parameterValuePtr[TtAnte.VICCALL1], TtAnte.VICCALL1_SZ, nullIndPtr[TtAnte.VICCALL1]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "viccall2", parameterValuePtr[TtAnte.VICCALL2], TtAnte.VICCALL2_SZ, nullIndPtr[TtAnte.VICCALL2]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "vicbndcde", parameterValuePtr[TtAnte.VICBNDCDE], TtAnte.VICBNDCDE_SZ, nullIndPtr[TtAnte.VICBNDCDE]);

                Ssutil.DbBindShortInput(hStmtInsert, 0, "vicanum", parameterValuePtr[TtAnte.VICANUM], nullIndPtr[TtAnte.VICANUM]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "intacode", parameterValuePtr[TtAnte.INTACODE], TtAnte.INTACODE_SZ, nullIndPtr[TtAnte.INTACODE]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "vicacode", parameterValuePtr[TtAnte.VICACODE], TtAnte.VICACODE_SZ, nullIndPtr[TtAnte.VICACODE]);

                Ssutil.DbBindShortInput(hStmtInsert, 0, "report", parameterValuePtr[TtAnte.REPORT], nullIndPtr[TtAnte.REPORT]);

                Ssutil.DbBindIntInput(hStmtInsert, 0, "subcaseno", parameterValuePtr[TtAnte.SUBCASENO], nullIndPtr[TtAnte.SUBCASENO]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "adiscctxh", parameterValuePtr[TtAnte.ADISCCTXH], nullIndPtr[TtAnte.ADISCCTXH]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "adiscctxv", parameterValuePtr[TtAnte.ADISCCTXV], nullIndPtr[TtAnte.ADISCCTXV]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "adisccrxh", parameterValuePtr[TtAnte.ADISCCRXH], nullIndPtr[TtAnte.ADISCCRXH]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "adisccrxv", parameterValuePtr[TtAnte.ADISCCRXV], nullIndPtr[TtAnte.ADISCCRXV]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "adiscxtxh", parameterValuePtr[TtAnte.ADISCXTXH], nullIndPtr[TtAnte.ADISCXTXH]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "adiscxtxv", parameterValuePtr[TtAnte.ADISCXTXV], nullIndPtr[TtAnte.ADISCXTXV]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "adiscxrxh", parameterValuePtr[TtAnte.ADISCXRXH], nullIndPtr[TtAnte.ADISCXRXH]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "adiscxrxv", parameterValuePtr[TtAnte.ADISCXRXV], nullIndPtr[TtAnte.ADISCXRXV]);

                Ssutil.DbBindIntInput(hStmtInsert, 0, "processed", parameterValuePtr[TtAnte.PROCESSED], nullIndPtr[TtAnte.PROCESSED]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "intause", parameterValuePtr[TtAnte.INTAUSE], TtAnte.INTAUSE_SZ, nullIndPtr[TtAnte.INTAUSE]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "vicause", parameterValuePtr[TtAnte.VICAUSE], TtAnte.VICAUSE_SZ, nullIndPtr[TtAnte.VICAUSE]);

                Ssutil.DbBindFloatInput(hStmtInsert, 0, "intgain", parameterValuePtr[TtAnte.INTGAIN], nullIndPtr[TtAnte.INTGAIN]);

                Ssutil.DbBindFloatInput(hStmtInsert, 0, "vicgain", parameterValuePtr[TtAnte.VICGAIN], nullIndPtr[TtAnte.VICGAIN]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "intaxref", parameterValuePtr[TtAnte.INTAXREF], TtAnte.INTAXREF_SZ, nullIndPtr[TtAnte.INTAXREF]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "intamodel", parameterValuePtr[TtAnte.INTAMODEL], TtAnte.INTAMODEL_SZ, nullIndPtr[TtAnte.INTAMODEL]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "vicaxref", parameterValuePtr[TtAnte.VICAXREF], TtAnte.VICAXREF_SZ, nullIndPtr[TtAnte.VICAXREF]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "vicamodel", parameterValuePtr[TtAnte.VICAMODEL], TtAnte.VICAMODEL_SZ, nullIndPtr[TtAnte.VICAMODEL]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "intaoffax", parameterValuePtr[TtAnte.INTAOFFAX], TtAnte.INTAOFFAX_SZ, nullIndPtr[TtAnte.INTAOFFAX]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "inthopaz", parameterValuePtr[TtAnte.INTHOPAZ], nullIndPtr[TtAnte.INTHOPAZ]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "intantaz", parameterValuePtr[TtAnte.INTANTAZ], nullIndPtr[TtAnte.INTANTAZ]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "intoffantax", parameterValuePtr[TtAnte.INTOFFANTAX], nullIndPtr[TtAnte.INTOFFANTAX]);

                Ssutil.DbBindStringInput(hStmtInsert, 0, "vicaoffax", parameterValuePtr[TtAnte.VICAOFFAX], TtAnte.VICAOFFAX_SZ, nullIndPtr[TtAnte.VICAOFFAX]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "vichopaz", parameterValuePtr[TtAnte.VICHOPAZ], nullIndPtr[TtAnte.VICHOPAZ]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "vicantaz", parameterValuePtr[TtAnte.VICANTAZ], nullIndPtr[TtAnte.VICANTAZ]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "vicoffantax", parameterValuePtr[TtAnte.VICOFFANTAX], nullIndPtr[TtAnte.VICOFFANTAX]);

                Ssutil.DbBindFloatInput(hStmtInsert, 0, "intaht", parameterValuePtr[TtAnte.INTAHT], nullIndPtr[TtAnte.INTAHT]);

                Ssutil.DbBindFloatInput(hStmtInsert, 0, "vicaht", parameterValuePtr[TtAnte.VICAHT], nullIndPtr[TtAnte.VICAHT]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "intvicel", parameterValuePtr[TtAnte.INTVICEL], nullIndPtr[TtAnte.INTVICEL]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "vicintel", parameterValuePtr[TtAnte.VICINTEL], nullIndPtr[TtAnte.VICINTEL]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "intelev", parameterValuePtr[TtAnte.INTELEV], nullIndPtr[TtAnte.INTELEV]);

                Ssutil.DbBindDoubleInput(hStmtInsert, 0, "vicelev", parameterValuePtr[TtAnte.VICELEV], nullIndPtr[TtAnte.VICELEV]);

                Ssutil.DbBindIntInput(hStmtInsert, 0, "caseno", parameterValuePtr[TtAnte.CASENO], nullIndPtr[TtAnte.CASENO]);

            }
            catch (Exception e)
            {
                Log2.e("\nTtDynAnte.TtInsertAnte(): ERROR: ODBC 'binding' attempt failed: " + e.Message);
                Ssutil.DbGetDiagStmt(hStmtInsert, "ttInsertAnte02: Problem binding parameter: " + e.Message);
                return Error.ODBC_BINDING_FAILED;
            }

            sqlRet = ODBC.SQLExecute(hStmtInsert);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTtDynAnte.TtInsertAnte(): ERROR: ODBC SQLExecute() failed: ");
                Ssutil.DbGetDiagStmt(hStmtInsert, "ttInsertAnte03: Error executing.");
                return (Error.ODBC_EXECUTE_FAILED);
            }

            return (Constant.SUCCESS);
        }


    }
}

```
