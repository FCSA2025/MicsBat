# Documented File: TeDynSite.cs
**Repository Path:** `TpRunTsip 20231124\TeDynSite.cs`
**Primary Layer:** `TpRunTsip 20231124`
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
    /// records into the database table <b>&lt;userID&gt;.te_&lt;tableName&gt;_&lt;runID&gt;_site</b>
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item> The methods in this class are used to perform operations on the
    /// te SITE table. The actual SITE table name is a variable, prescribed by the caller.</item>
    /// <item>The supported operations are: TePrepareSite, TeInsertSite and TeSiteClose.</item>
    /// <item>The caller must first call the method TePrepareSite() that creates a cursor 
    /// for use with a subsequent call to TeInsertSite().</item>
    /// <item>The method TeCloseSite() must be called to release internal cursor resources (a cursor 
    /// encapsulates an ODBC connection and statement handle et. al.).</item>
    /// </list>
    /// </remarks>
    public class TeDynSite
    {
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int tePrepareSite([In] string table);

        //------------------------------------------------------------------

        private static SQLHANDLE mhTESStmt;
        private static SQLHDBC mhTESConn = SQLHDBC.Zero;
        private static string mcSiteTable;

        /// <summary>
        /// This method calls ODBC.SQLPrepare() to prepare a table insert on an internal (private) statement 
        /// handle that can then be used in a subsequent call to ODBC.SQLExecute to
        /// actually perform the SQL record insertion.
        /// </summary>
        /// <param name="table"> - full name of the DB table into which the record is to be inserted.</param>
        /// <returns></returns>
        public static int TePrepareSite(string table)
        {
            string insert_buf;
            SQLRETURN sqlRet;

            mcSiteTable = table;

            // For a hybrid build, we need to set the static variable cSiteTable
            // in native file teDynSite.cpp.
#if PINVOKE
            tePrepareSite(table);
#endif

            insert_buf = String.Format("insert into {0} (terrcall1, terrcall2, earthlocation, terrname1, terrname2, earthname, terroper, terroper2, earthoper, terrlatit,terrlongit, terrgrnd, earthlatit, earthlongit, earthgrnd, radiozone, rainzone, etreport, tereport, etcaseno, tecaseno, etsubcases, tesubcases, intreq, etdist, etazim, teazim, tudist, tuazim, utazim, eudist, euazim, ueazim, processed) values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", table);

            mhTESConn = Ssutil.NewConn();

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, mhTESConn, out mhTESStmt);

            sqlRet = ODBC.SQLPrepare(mhTESStmt, insert_buf, insert_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTeDynSite.TePrepareSite(): ERROR: SQLPrepare() failed.");
                Ssutil.DbGetDiagStmt(mhTESStmt, "tePrepareSite01: Could not prepare the te site.");

                return Error.ODBC_PREPARE_FAILED;
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method closes the internal (private) cursor object by freeing the ODBC 
        /// connection and statement handles. 
        /// </summary>
        /// <param name=""></param> 
        public static int CloseTESite()
        {
            if (mhTESConn != null)
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, mhTESStmt);

                mhTESStmt = SQLHANDLE.Zero;

                Ssutil.DisConn(mhTESConn);

                mhTESConn = SQLHDBC.Zero;
            }
            return 0;
        }

        /// <summary>
        /// <summary>
        /// This method calls ODBC.SQLExecute() to perform insertion of a record
        /// into a DB table by using the internal (private) ODBC statement handle
        /// that was prepared on by a previous call to ODBC.SQLPrepare().
        /// </summary>
        /// <param name="teSite"> - TeSite object (record) to be inserted into the DB table.</param>
        /// <param name="nullInds"> - array of ODBC nullInds associated with teSite.</param>
        /// <returns></returns>
        public static int TeInsertSite(TeSite teSite, SQLLEN[] nullInds)
        {
            SQLRETURN sqlRet;

            if (mhTESConn == SQLHDBC.Zero)
            {
                TePrepareSite(mcSiteTable);
            }

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInds);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of insertStruct
            //into global memory with an SQLPOINTER pointer assigned to each one. TeSite provides
            //a convenience method that does exactly this.

            SQLPOINTER[] parameterValuePtr = teSite.CopyToArrayOfSQLPOINTERs();

            //	exec sql execute insert_site using...
            try
            {
                // Resets the auto column index to zero.
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(mhTESStmt, 0, "terrcall1", parameterValuePtr[TeSite.TERRCALL1], TeSite.TERRCALL1_SZ, nullIndPtr[TeSite.TERRCALL1]);
                Ssutil.DbBindStringInput(mhTESStmt, 0, "terrcall2", parameterValuePtr[TeSite.TERRCALL2], TeSite.TERRCALL2_SZ, nullIndPtr[TeSite.TERRCALL2]);
                Ssutil.DbBindStringInput(mhTESStmt, 0, "earthlocation", parameterValuePtr[TeSite.EARTHLOCATION], TeSite.EARTHLOCATION_SZ, nullIndPtr[TeSite.EARTHLOCATION]);
                Ssutil.DbBindStringInput(mhTESStmt, 0, "terrname1", parameterValuePtr[TeSite.TERRNAME1], TeSite.TERRNAME1_SZ, nullIndPtr[TeSite.TERRNAME1]);
                Ssutil.DbBindStringInput(mhTESStmt, 0, "terrname2", parameterValuePtr[TeSite.TERRNAME2], TeSite.TERRNAME2_SZ, nullIndPtr[TeSite.TERRNAME2]);
                Ssutil.DbBindStringInput(mhTESStmt, 0, "earthname", parameterValuePtr[TeSite.EARTHNAME], TeSite.EARTHNAME_SZ, nullIndPtr[TeSite.EARTHNAME]);
                Ssutil.DbBindStringInput(mhTESStmt, 0, "terroper", parameterValuePtr[TeSite.TERROPER], TeSite.TERROPER_SZ, nullIndPtr[TeSite.TERROPER]);
                Ssutil.DbBindStringInput(mhTESStmt, 0, "terroper2", parameterValuePtr[TeSite.TERROPER2], TeSite.TERROPER2_SZ, nullIndPtr[TeSite.TERROPER2]);
                Ssutil.DbBindStringInput(mhTESStmt, 0, "earthoper", parameterValuePtr[TeSite.EARTHOPER], TeSite.EARTHOPER_SZ, nullIndPtr[TeSite.EARTHOPER]);
                Ssutil.DbBindIntInput(mhTESStmt, 0, "terrlatit", parameterValuePtr[TeSite.TERRLATIT], nullIndPtr[TeSite.TERRLATIT]);
                Ssutil.DbBindIntInput(mhTESStmt, 0, "terrlongit", parameterValuePtr[TeSite.TERRLONGIT], nullIndPtr[TeSite.TERRLONGIT]);
                Ssutil.DbBindDoubleInput(mhTESStmt, 0, "terrgrnd", parameterValuePtr[TeSite.TERRGRND], nullIndPtr[TeSite.TERRGRND]);
                Ssutil.DbBindIntInput(mhTESStmt, 0, "earthlatit", parameterValuePtr[TeSite.EARTHLATIT], nullIndPtr[TeSite.EARTHLATIT]);
                Ssutil.DbBindIntInput(mhTESStmt, 0, "earthlongit", parameterValuePtr[TeSite.EARTHLONGIT], nullIndPtr[TeSite.EARTHLONGIT]);
                Ssutil.DbBindDoubleInput(mhTESStmt, 0, "earthgrnd", parameterValuePtr[TeSite.EARTHGRND], nullIndPtr[TeSite.EARTHGRND]);
                Ssutil.DbBindStringInput(mhTESStmt, 0, "radiozone", parameterValuePtr[TeSite.RADIOZONE], TeSite.RADIOZONE_SZ, nullIndPtr[TeSite.RADIOZONE]);
                Ssutil.DbBindShortInput(mhTESStmt, 0, "rainzone", parameterValuePtr[TeSite.RAINZONE], nullIndPtr[TeSite.RAINZONE]);
                Ssutil.DbBindShortInput(mhTESStmt, 0, "etreport", parameterValuePtr[TeSite.ETREPORT], nullIndPtr[TeSite.ETREPORT]);
                Ssutil.DbBindShortInput(mhTESStmt, 0, "tereport", parameterValuePtr[TeSite.TEREPORT], nullIndPtr[TeSite.TEREPORT]);
                Ssutil.DbBindIntInput(mhTESStmt, 0, "etcaseno", parameterValuePtr[TeSite.ETCASENO], nullIndPtr[TeSite.ETCASENO]);
                Ssutil.DbBindIntInput(mhTESStmt, 0, "tecaseno", parameterValuePtr[TeSite.TECASENO], nullIndPtr[TeSite.TECASENO]);
                Ssutil.DbBindIntInput(mhTESStmt, 0, "etsubcases", parameterValuePtr[TeSite.ETSUBCASES], nullIndPtr[TeSite.ETSUBCASES]);
                Ssutil.DbBindIntInput(mhTESStmt, 0, "tesubcases", parameterValuePtr[TeSite.TESUBCASES], nullIndPtr[TeSite.TESUBCASES]);
                Ssutil.DbBindStringInput(mhTESStmt, 0, "intreq", parameterValuePtr[TeSite.INTREQ], TeSite.INTREQ_SZ, nullIndPtr[TeSite.INTREQ]);
                Ssutil.DbBindDoubleInput(mhTESStmt, 0, "etdist", parameterValuePtr[TeSite.ETDIST], nullIndPtr[TeSite.ETDIST]);
                Ssutil.DbBindDoubleInput(mhTESStmt, 0, "etazim", parameterValuePtr[TeSite.ETAZIM], nullIndPtr[TeSite.ETAZIM]);
                Ssutil.DbBindDoubleInput(mhTESStmt, 0, "teazim", parameterValuePtr[TeSite.TEAZIM], nullIndPtr[TeSite.TEAZIM]);
                Ssutil.DbBindDoubleInput(mhTESStmt, 0, "tudist", parameterValuePtr[TeSite.TUDIST], nullIndPtr[TeSite.TUDIST]);
                Ssutil.DbBindDoubleInput(mhTESStmt, 0, "tuazim", parameterValuePtr[TeSite.TUAZIM], nullIndPtr[TeSite.TUAZIM]);
                Ssutil.DbBindDoubleInput(mhTESStmt, 0, "utazim", parameterValuePtr[TeSite.UTAZIM], nullIndPtr[TeSite.UTAZIM]);
                Ssutil.DbBindDoubleInput(mhTESStmt, 0, "eudist", parameterValuePtr[TeSite.EUDIST], nullIndPtr[TeSite.EUDIST]);
                Ssutil.DbBindDoubleInput(mhTESStmt, 0, "euazim", parameterValuePtr[TeSite.EUAZIM], nullIndPtr[TeSite.EUAZIM]);
                Ssutil.DbBindDoubleInput(mhTESStmt, 0, "ueazim", parameterValuePtr[TeSite.UEAZIM], nullIndPtr[TeSite.UEAZIM]);
                Ssutil.DbBindIntInput(mhTESStmt, 0, "processed", parameterValuePtr[TeSite.PROCESSED], nullIndPtr[TeSite.PROCESSED]);
            }
            catch (Exception e)
            {
                Log2.e("\nTeDynSite.TeInsertSite(): ERROR: ODBC 'Bind' attempt failed: " + e.Message);
                Ssutil.DbGetDiagStmt(mhTESStmt, "teInsertSite02: Could not bind " + e.Message + " to insert site.");
                return Error.ODBC_BINDING_FAILED;
            }

            sqlRet = ODBC.SQLExecute(mhTESStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTeDynSite.TeInsertSite(): ERROR: SQLExecute() failed trying an insert.");
                Ssutil.DbGetDiagStmt(mhTESStmt, "teInsertSite03: Could not execute the insert te site.");
                return Error.ODBC_EXECUTE_FAILED - 3;
            }

            return (Constant.SUCCESS);
        }



    }
}

```
