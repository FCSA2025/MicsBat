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

    public class DynAuditTrail
    {
        private const string mBaseTableName = "audit_trail";

        private const string mAdmTableName = "adm." + mBaseTableName;

        private static string mTableName = mAdmTableName;

        private static bool mMdbWriteEnabled = true;

        private static bool mSpoofModeIsOff = true;

        private const string mINSERT = "INSERT INTO {0} VALUES ({1}) ";

        //====================================================================================
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;
        //====================================================================================

        public static string TableName
        {
            get { return mTableName; }
        }

        public static bool WriteEnabled
        {
            get { return mMdbWriteEnabled; }
        }

        /// <summary>
        /// This method can be used to set whether the <b>adm.audit_trail</b> table should be written to,
        /// (SQL DELETE, INSERT and/or UPDATE queries) or not; it can also set whether the SQL queries should
        /// target the actual adm.audit_trail table or a user's 'spoof' table (e.g. hulme.audit_trail).
        /// </summary>
        /// <param name="mdbWriteEnabled"> - boolean that prescribes whether audit_trail record
        /// DELETE, INSERT and/or UPDATE operations will be performed (the default) or inhibited.</param>
        /// <param name="spoofModeIsOff"> - boolean that prescribes whether 'spoof' mode is enabled or disabled (the default).</param>
        /// <param name="userSchema"> - the user's prescribed SQL Server schema name.</param>
        public static void SetState(bool mdbWriteEnabled, bool spoofModeIsOff, string userSchema)
        {
            mMdbWriteEnabled = mdbWriteEnabled;
            mSpoofModeIsOff = spoofModeIsOff;


            if (spoofModeIsOff)
            {
                mTableName = mAdmTableName;
            }
            else
            {
                if (String.IsNullOrWhiteSpace(userSchema))
                {
                    Log2.e("\nDynAuditTrail.SetState(): ERROR: spoofMode is ON but userSchema is null or whitespace.");
                    // Set the mTableName to something nonsensical so that all subsequent
                    // SQL queries will fail because the table does not exist.
                    mTableName = "idonotexist" + "." + mBaseTableName;
                }
                else
                {
                    mTableName = userSchema + "." + mBaseTableName;
                }

            }
        }

        /// <summary>
        /// Inserts an AuditTable record into the adm.audit_table using column values prescribed 
        /// by the fields of the AuditTable object.
        /// </summary>
        /// <param name="auditTrail"> - a MeChan object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for meChan</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int AuditTrailInsert(AuditTrail auditTrail, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynAuditTrail.AuditTrailInsert: Entry");

            string cSQL;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLHANDLE hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nDynAuditTrail.AuditTrailInsert: ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            //Create the SQL insert statement.
            cSQL = String.Format(mINSERT, mTableName, auditTrail.ToStringAsCSV(nullInd));

            //...Log2.v("\nDynAuditTrail.AuditTrailInsert: SQLExecDirect():\r\n" + cSQL);

            if (mMdbWriteEnabled)
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\n\nDynAuditTrail.AuditTrailInsert: ERROR: a call to SQLExecDirect() failed for: " + cSQL);
                    string str = "AuditTrail -- Error inserting record";
                    Ssutil.DbGetDiagStmt(hStmt, str); ;
                    Ssutil.DisConnStmt(hConn, hStmt);
                    return Error.ODBC_EXECDIRECT_FAILED;
                }

                //...Log2.v("\n\nDynAuditTrail.AuditTrailInsert: successfully inserted record into table " + mTableName);
            }
            else
            {
                Info.Text += "\r\n\r\n" + cSQL;
                //...Log2.v("\nDynAuditTrail.AuditTrailInsert: Insertion of AuditTrail records is DISABLED.");
            }

            //This method call releases hStmt, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\n\nDynAuditTrail.AuditTrailInsert: Exit");
            return Constant.SUCCESS;
        }





    }
}
