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
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;

    public class DynTsipReports
    {
        private const string mINSERT = "INSERT INTO {0} ({1}) VALUES ({2}) ";
        private const string mDROP = "IF OBJECT_ID('{0}') IS NOT NULL DROP TABLE {0}";

        /// <summary>
        /// Inserts a record into a database table whose columns are isomorphic with the
        /// public members of a TsipReports class object.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <param name="tsipReports"> - a TsipReports object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for mtSite</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int Insert(string tableName, TsipReports tsipReports, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nTsipReports.Insert(): Entry");

            string cSQL;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nTsipReports.Insert(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            //Create the SQL insert statement.
            cSQL = String.Format(mINSERT, tableName, TsipReports.AllColumnsForSqlSelect, tsipReports.ToStringAsCSV(nullInd));

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string diagnostics = ODBC.GetDiagnostics(hStmt, cSQL);
                Log2.e("\n\nTsipReports.Insert(): ERROR: a call to SQLExecDirect() failed for: {0}\n{1}", cSQL, diagnostics);
                
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //This method call releases hStmt.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            Ssutil.DisConn(hConn);

            //...Log2.v("\n\nTsipReports.Insert(): Exit");
            return Constant.SUCCESS;
        }

        public static int DropTableIfExists(string tableName)
        {
            //...Log2.v("\n\nTsipReports.DropTableIfExists(): Entry");

            string cSQL;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nTsipReports.DropTableIfExists(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            //Create the SQL insert statement.
            cSQL = String.Format(mDROP, tableName);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string diagnostics = ODBC.GetDiagnostics(hStmt, cSQL);
                Log2.e("\n\nTsipReports.DropTableIfExists(): ERROR: a call to SQLExecDirect() failed for: {0}\n{1}", cSQL, diagnostics);

                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //This method call releases hStmt.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            Ssutil.DisConn(hConn);

            //...Log2.v("\n\nTsipReports.Insert(): Exit");
            return Constant.SUCCESS;
        }

        public static int CreateTable(string tableName)
        {
            //...Log2.v("\n\nTsipReports.CreateTable(): Entry");

            string cSQL;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nTsipReports.CreateTable(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            //Create the SQL insert statement.
            cSQL = String.Format(TsipReports.CREATE_TABLE, tableName);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string diagnostics = ODBC.GetDiagnostics(hStmt, cSQL);
                Log2.e("\n\nTsipReports.CreateTable(): ERROR: a call to SQLExecDirect() failed for: {0}\n{1}", cSQL, diagnostics);

                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //This method call releases hStmt.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            Ssutil.DisConn(hConn);

            //...Log2.v("\n\nTsipReports.Insert(): Exit");
            return Constant.SUCCESS;
        }

    }
}
