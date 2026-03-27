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

    public class DynTAFLintermediate
    {

        /// <summary>
        /// Inserts a TAFL record into the database using column values prescribed 
        /// by the fields of the TAFL object.
        /// </summary>
        /// <param name="tableName"> - name of the SQL table to INSERT into.</param>
        /// <param name="tAFLintermediate"> - a TAFLintermediate object providing the values to be inserted into the SQL table.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pSite</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int Insert(string tableName, TAFLintermediate tAFLintermediate, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynTAFL.InsertTAFL(): Entry");

            string update_buf;

            SQLHANDLE hStmt;

            SQLHANDLE hConn = Ssutil.NewConn();

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            //Create the SQL insert statement.
            update_buf = TAFLintermediate.BuildSqlInsertString(tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pSite
            //into global memory with an SQLPOINTER pointer assigned to each one. FtSite provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = tAFLintermediate.CopyToArrayOfSQLPOINTERinGlobalMemory();

            // Bind the parameters.
            // This integer is used to enumerate the binding sequence; ODBC definition is that first binding is N = 1;
            int colNum = -1;
            try
            {
                colNum = 1;
                Ssutil.DbBindStringInput(hStmt, colNum, "TxRx", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.TxRx.Length, nullIndPtr[colNum - 1]);

                colNum = 2;
                Ssutil.DbBindStringInput(hStmt, colNum, "callsign", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.callsign.Length, nullIndPtr[colNum - 1]);

                colNum = 3;
                Ssutil.DbBindStringInput(hStmt, colNum, "call2", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.call2.Length, nullIndPtr[colNum - 1]);

                colNum = 4;
                Ssutil.DbBindStringInput(hStmt, colNum, "bndcde", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.bndcde.Length, nullIndPtr[colNum - 1]);

                colNum = 5;
                Ssutil.DbBindStringInput(hStmt, colNum, "sitename", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.sitename.Length, nullIndPtr[colNum - 1]);

                colNum = 6;
                Ssutil.DbBindIntInput(hStmt, colNum, "anum", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 7;
                Ssutil.DbBindStringInput(hStmt, colNum, "chid", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.chid.Length, nullIndPtr[colNum - 1]);

                colNum = 8;
                Ssutil.DbBindStringInput(hStmt, colNum, "lat", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.lat.Length, nullIndPtr[colNum - 1]);

                colNum = 9;
                Ssutil.DbBindStringInput(hStmt, colNum, "lon", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.lon.Length, nullIndPtr[colNum - 1]);

                colNum = 10;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "grnd", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 11;
                Ssutil.DbBindStringInput(hStmt, colNum, "prov", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.prov.Length, nullIndPtr[colNum - 1]);

                colNum = 12;
                Ssutil.DbBindStringInput(hStmt, colNum, "oper", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.oper.Length, nullIndPtr[colNum - 1]);

                colNum = 13;
                Ssutil.DbBindStringInput(hStmt, colNum, "opnote", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.opnote.Length, nullIndPtr[colNum - 1]);

                colNum = 14;
                Ssutil.DbBindStringInput(hStmt, colNum, "stats", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.stats.Length, nullIndPtr[colNum - 1]);

                colNum = 15;
                Ssutil.DbBindStringInput(hStmt, colNum, "icaccount", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.icaccount.Length, nullIndPtr[colNum - 1]);

                colNum = 16;
                Ssutil.DbBindStringInput(hStmt, colNum, "licence", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.licence.Length, nullIndPtr[colNum - 1]);

                colNum = 17;
                Ssutil.DbBindStringInput(hStmt, colNum, "sdate", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.sdate.Length, nullIndPtr[colNum - 1]);

                colNum = 18;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "Total_losses_dB", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 19;
                Ssutil.DbBindStringInput(hStmt, colNum, "acodetx", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.acodetx.Length, nullIndPtr[colNum - 1]);

                colNum = 20;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "ahttx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 21;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "aztx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 22;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "elevtx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 23;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "freq", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 24;
                Ssutil.DbBindStringInput(hStmt, colNum, "pol", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.pol.Length, nullIndPtr[colNum - 1]);

                colNum = 25;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "pwrtx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 26;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "losstx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 27;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "gaintx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 28;
                Ssutil.DbBindStringInput(hStmt, colNum, "eqpttx", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.eqpttx.Length, nullIndPtr[colNum - 1]);

                colNum = 29;
                Ssutil.DbBindStringInput(hStmt, colNum, "traftx", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.traftx.Length, nullIndPtr[colNum - 1]);

                colNum = 30;
                Ssutil.DbBindStringInput(hStmt, colNum, "namerx", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.namerx.Length, nullIndPtr[colNum - 1]);

                colNum = 31;
                Ssutil.DbBindIntInput(hStmt, colNum, "anumrx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 32;
                Ssutil.DbBindStringInput(hStmt, colNum, "latrx", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.latrx.Length, nullIndPtr[colNum - 1]);

                colNum = 33;
                Ssutil.DbBindStringInput(hStmt, colNum, "lonrx", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.lonrx.Length, nullIndPtr[colNum - 1]);

                colNum = 34;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "grndrx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 35;
                Ssutil.DbBindStringInput(hStmt, colNum, "provrx", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.provrx.Length, nullIndPtr[colNum - 1]);

                colNum = 36;
                Ssutil.DbBindStringInput(hStmt, colNum, "operrx", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.operrx.Length, nullIndPtr[colNum - 1]);

                colNum = 37;
                Ssutil.DbBindStringInput(hStmt, colNum, "acoderx", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.acoderx.Length, nullIndPtr[colNum - 1]);

                colNum = 38;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "ahtrx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 39;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "azrx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 40;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "elevrx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 41;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "lossrx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 42;
                Ssutil.DbBindDoubleInput(hStmt, colNum, "gainrx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 43;
                Ssutil.DbBindStringInput(hStmt, colNum, "eqptrx", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.eqptrx.Length, nullIndPtr[colNum - 1]);

                colNum = 44;
                Ssutil.DbBindStringInput(hStmt, colNum, "trafrx", parameterValuePtr[colNum - 1], (SQLULEN)tAFLintermediate.trafrx.Length, nullIndPtr[colNum - 1]);

                colNum = 45;
                Ssutil.DbBindIntInput(hStmt, colNum, "RecordAction", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                colNum = 46;
                Ssutil.DbBindIntInput(hStmt, colNum, "MICSanum", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);
            }
            catch
            {
                Log2.e("\n\nDynTAFL.InsertTAFL(): ERROR: call to Ssutil.DbBindXXXInput() failed for colNum = {0}", colNum);
                Ssutil.DisConnStmt(hConn, hStmt);
                return -1;
            }

            //...Log2.v("nDynTAFL.InsertTAFL(): SQLExecDirect():\r\n" + update_buf);
            SQLRETURN sqlRet;
            sqlRet = ODBC.SQLExecDirect(hStmt, update_buf, update_buf.Length);

            //...Log2.v("\r\nDynTAFL.InsertTAFL(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynTAFL.InsertTAFL(): ERROR: call to SQLExecDirect failed for query:\n{0}", update_buf);
                Log2.e("\n" + ODBC.GetDiagnostics(hStmt, update_buf));
                Ssutil.DisConnStmt(hConn, hStmt);
                return -2;
            }

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynTAFL.InsertTAFL(): Exit");
            return 0;
        }






    }
}
