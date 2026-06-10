# Documented File: SqlMicsErrRec.cs
**Repository Path:** `_DataStructures\SqlMicsErrRec.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
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
    using SQLUINTEGER = UInt32;

    public class SqlMicsErrRec
    {
        private string mDateTimeOfError;
        private string mProvenance;
        private string mApplicationReportingError;
        private string mMicsUserID;
        private string mErrorMessage;
        private string mRemedialAction;

        public string DateTimeOfError { get => mDateTimeOfError; set => mDateTimeOfError = value; }
        public string Provenance { get => mProvenance; set => mProvenance = value; }
        public string ApplicationReportingError { get => mApplicationReportingError; set => mApplicationReportingError = value; }
        public string MicsUserID { get => mMicsUserID; set => mMicsUserID = value; }
        public string ErrorMessage { get => mErrorMessage; set => mErrorMessage = value; }
        public string RemedialAction { get => mRemedialAction; set => mRemedialAction = value; }

        //===================================================================================

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 6;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "DateTimeOfError", "Provenance", "ApplicationReportingError", "MicsUserID", "ErrorMessage", "RemedialAction" };

        public const int DATETIMEOFERROR = 0;
        public const int PROVENANCE = 1;
        public const int APPLICATIONREPORTINGERROR = 2;
        public const int MICSUSERID = 3;
        public const int ERRORMESSAGE = 4;
        public const int REMEDIALACTION = 5;

        public const int DATETIMEOFERROR_SZ = 64 + 1;
        public const int PROVENANCE_SZ = 64 + 1;
        public const int APPLICATIONREPORTINGERROR_SZ = 64 + 1;
        public const int MICSUSERID_SZ = 64 + 1;
        public const int ERRORMESSAGE_SZ = 1000000 + 1;
        public const int REMEDIALACTION_SZ = 1000000 + 1;

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static SqlMicsErrRec()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();

            SQLPOINTER[] ParameterValuePtr = Arrays.CreateArrayUsingDefaultElementConstructor<SQLPOINTER>(NUM_COLUMNS);
            SQLLENPTR[] StrLen_or_IndPtr = Arrays.CreateArrayUsingDefaultElementConstructor<SQLLENPTR>(NUM_COLUMNS);
        }

        private SqlMicsErrRec() { }

        public SqlMicsErrRec(string errorMessage, string remedialAction)
        {
            mDateTimeOfError = DateTime.Now.ToString("s");
            mProvenance = "MICS# console program";
            mApplicationReportingError = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            mMicsUserID = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            mErrorMessage = errorMessage;
            mRemedialAction = remedialAction;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n ===== SQLtableErrorRecord ===== ");

            sb.Append("\nmDateTimeOfError = " + mDateTimeOfError);
            sb.Append("\nmProvenance = " + mProvenance);
            sb.Append("\nmApplicationReportingError = " + mApplicationReportingError);
            sb.Append("\nmMicsUserID = " + mMicsUserID);
            sb.Append("\nmErrorMessage = " + mErrorMessage);
            sb.Append("\nmRemedialAction = " + mRemedialAction);

            return sb.ToString();
        }

        public string ToStringWN(SQLLEN[] nullInds)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("\n ===== SQLtableErrorRecord ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "mDateTimeOfError = " + mDateTimeOfError);
            sb.Append("\n" + nullInds[i++] + "      " + "mProvenance = " + mProvenance);
            sb.Append("\n" + nullInds[i++] + "      " + "mApplicationReportingError = " + mApplicationReportingError);
            sb.Append("\n" + nullInds[i++] + "      " + "mMicsUserID = " + mMicsUserID);
            sb.Append("\n" + nullInds[i++] + "      " + "mErrorMessage = " + mErrorMessage);
            sb.Append("\n" + nullInds[i++] + "      " + "mRemedialAction = " + mRemedialAction);

            return sb.ToString();
        }

        public string ToStringAsCSV(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[SqlMicsErrRec.DATETIMEOFERROR] == Constant.DB_NULL ? "NULL, " : "'" + mDateTimeOfError.ToString() + "', ");
            sb.Append(nullInds[SqlMicsErrRec.PROVENANCE] == Constant.DB_NULL ? "NULL, " : "'" + mProvenance.ToString() + "', ");
            sb.Append(nullInds[SqlMicsErrRec.APPLICATIONREPORTINGERROR] == Constant.DB_NULL ? "NULL, " : "'" + mApplicationReportingError.ToString() + "', ");
            sb.Append(nullInds[SqlMicsErrRec.MICSUSERID] == Constant.DB_NULL ? "NULL, " : "'" + mMicsUserID.ToString() + "', ");
            sb.Append(nullInds[SqlMicsErrRec.ERRORMESSAGE] == Constant.DB_NULL ? "NULL, " : "'" + mErrorMessage.ToString() + "', ");
            sb.Append(nullInds[SqlMicsErrRec.REMEDIALACTION] == Constant.DB_NULL ? "NULL, " : "'" + mRemedialAction.ToString() + "' ");

            return sb.ToString();
        }

        public string ToStringAsCSVexport(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[SqlMicsErrRec.DATETIMEOFERROR] == Constant.DB_NULL ? "\"\"," : "\"" + mDateTimeOfError.ToString() + "\",");
            sb.Append(nullInds[SqlMicsErrRec.PROVENANCE] == Constant.DB_NULL ? "\"\"," : "\"" + mProvenance.ToString() + "\",");
            sb.Append(nullInds[SqlMicsErrRec.APPLICATIONREPORTINGERROR] == Constant.DB_NULL ? "\"\"," : "\"" + mApplicationReportingError.ToString() + "\",");
            sb.Append(nullInds[SqlMicsErrRec.MICSUSERID] == Constant.DB_NULL ? "\"\"," : "\"" + mMicsUserID.ToString() + "\",");
            sb.Append(nullInds[SqlMicsErrRec.ERRORMESSAGE] == Constant.DB_NULL ? "\"\"," : "\"" + mErrorMessage.ToString() + "\",");
            sb.Append(nullInds[SqlMicsErrRec.REMEDIALACTION] == Constant.DB_NULL ? "\"\"," : "\"" + mRemedialAction.ToString() + "\"");

            return sb.ToString();
        }

        public string ToStringAsCSVequates(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(columnNames[SqlMicsErrRec.DATETIMEOFERROR] + " = " + (nullInds[SqlMicsErrRec.DATETIMEOFERROR] == Constant.DB_NULL ? "NULL, " : "'" + mDateTimeOfError.ToString() + "', "));
            sb.Append(columnNames[SqlMicsErrRec.PROVENANCE] + " = " + (nullInds[SqlMicsErrRec.PROVENANCE] == Constant.DB_NULL ? "NULL, " : "'" + mProvenance.ToString() + "', "));
            sb.Append(columnNames[SqlMicsErrRec.APPLICATIONREPORTINGERROR] + " = " + (nullInds[SqlMicsErrRec.APPLICATIONREPORTINGERROR] == Constant.DB_NULL ? "NULL, " : "'" + mApplicationReportingError.ToString() + "', "));
            sb.Append(columnNames[SqlMicsErrRec.MICSUSERID] + " = " + (nullInds[SqlMicsErrRec.MICSUSERID] == Constant.DB_NULL ? "NULL, " : "'" + mMicsUserID.ToString() + "', "));
            sb.Append(columnNames[SqlMicsErrRec.ERRORMESSAGE] + " = " + (nullInds[SqlMicsErrRec.ERRORMESSAGE] == Constant.DB_NULL ? "NULL, " : "'" + mErrorMessage.ToString() + "', "));
            sb.Append(columnNames[SqlMicsErrRec.REMEDIALACTION] + " = " + (nullInds[SqlMicsErrRec.REMEDIALACTION] == Constant.DB_NULL ? "NULL, " : "'" + mRemedialAction.ToString() + "' "));

            return sb.ToString();
        }

        public static void BindPtrsToCols(SQLHANDLE hStmt, out SQLPOINTER[] tgtValPtrs, out SQLPOINTER[] nullIndPtrs)
        {
            tgtValPtrs = new SQLPOINTER[NUM_COLUMNS];
            nullIndPtrs = new SQLPOINTER[NUM_COLUMNS];

            tgtValPtrs[DATETIMEOFERROR] = Marshal.AllocHGlobal(DATETIMEOFERROR_SZ + 1);
            nullIndPtrs[DATETIMEOFERROR] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, DATETIMEOFERROR + 1, tgtValPtrs[DATETIMEOFERROR], SqlMicsErrRec.DATETIMEOFERROR_SZ, nullIndPtrs[DATETIMEOFERROR]);

            tgtValPtrs[PROVENANCE] = Marshal.AllocHGlobal(PROVENANCE_SZ + 1);
            nullIndPtrs[PROVENANCE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, PROVENANCE + 1, tgtValPtrs[PROVENANCE], SqlMicsErrRec.PROVENANCE_SZ, nullIndPtrs[PROVENANCE]);

            tgtValPtrs[APPLICATIONREPORTINGERROR] = Marshal.AllocHGlobal(APPLICATIONREPORTINGERROR_SZ + 1);
            nullIndPtrs[APPLICATIONREPORTINGERROR] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, APPLICATIONREPORTINGERROR + 1, tgtValPtrs[APPLICATIONREPORTINGERROR], SqlMicsErrRec.APPLICATIONREPORTINGERROR_SZ, nullIndPtrs[APPLICATIONREPORTINGERROR]);

            tgtValPtrs[MICSUSERID] = Marshal.AllocHGlobal(MICSUSERID_SZ + 1);
            nullIndPtrs[MICSUSERID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MICSUSERID + 1, tgtValPtrs[MICSUSERID], SqlMicsErrRec.MICSUSERID_SZ, nullIndPtrs[MICSUSERID]);

            tgtValPtrs[ERRORMESSAGE] = Marshal.AllocHGlobal(ERRORMESSAGE_SZ + 1);
            nullIndPtrs[ERRORMESSAGE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ERRORMESSAGE + 1, tgtValPtrs[ERRORMESSAGE], SqlMicsErrRec.ERRORMESSAGE_SZ, nullIndPtrs[ERRORMESSAGE]);

            tgtValPtrs[REMEDIALACTION] = Marshal.AllocHGlobal(REMEDIALACTION_SZ + 1);
            nullIndPtrs[REMEDIALACTION] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, REMEDIALACTION + 1, tgtValPtrs[REMEDIALACTION], SqlMicsErrRec.REMEDIALACTION_SZ, nullIndPtrs[REMEDIALACTION]);
        }

        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SqlMicsErrRec sQLtableErrorRecord, out SQLLEN[] nullInds)
        {
            sQLtableErrorRecord = new SqlMicsErrRec();
            nullInds = NullHelper.CreateArrayOfNullInd(SqlMicsErrRec.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[DATETIMEOFERROR]);
            if (nullInd == Constant.DB_NULL)
            {
                sQLtableErrorRecord.mDateTimeOfError = "";
                nullInds[DATETIMEOFERROR] = Constant.DB_NULL;
            }
            else
            {
                sQLtableErrorRecord.mDateTimeOfError = Marshal.PtrToStringAnsi(tgtValPtrs[DATETIMEOFERROR]).Trim();
                nullInds[DATETIMEOFERROR] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[PROVENANCE]);
            if (nullInd == Constant.DB_NULL)
            {
                sQLtableErrorRecord.mProvenance = "";
                nullInds[PROVENANCE] = Constant.DB_NULL;
            }
            else
            {
                sQLtableErrorRecord.mProvenance = Marshal.PtrToStringAnsi(tgtValPtrs[PROVENANCE]).Trim();
                nullInds[PROVENANCE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[APPLICATIONREPORTINGERROR]);
            if (nullInd == Constant.DB_NULL)
            {
                sQLtableErrorRecord.mApplicationReportingError = "";
                nullInds[APPLICATIONREPORTINGERROR] = Constant.DB_NULL;
            }
            else
            {
                sQLtableErrorRecord.mApplicationReportingError = Marshal.PtrToStringAnsi(tgtValPtrs[APPLICATIONREPORTINGERROR]).Trim();
                nullInds[APPLICATIONREPORTINGERROR] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MICSUSERID]);
            if (nullInd == Constant.DB_NULL)
            {
                sQLtableErrorRecord.mMicsUserID = "";
                nullInds[MICSUSERID] = Constant.DB_NULL;
            }
            else
            {
                sQLtableErrorRecord.mMicsUserID = Marshal.PtrToStringAnsi(tgtValPtrs[MICSUSERID]).Trim();
                nullInds[MICSUSERID] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ERRORMESSAGE]);
            if (nullInd == Constant.DB_NULL)
            {
                sQLtableErrorRecord.mErrorMessage = "";
                nullInds[ERRORMESSAGE] = Constant.DB_NULL;
            }
            else
            {
                sQLtableErrorRecord.mErrorMessage = Marshal.PtrToStringAnsi(tgtValPtrs[ERRORMESSAGE]).Trim();
                nullInds[ERRORMESSAGE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[REMEDIALACTION]);
            if (nullInd == Constant.DB_NULL)
            {
                sQLtableErrorRecord.mRemedialAction = "";
                nullInds[REMEDIALACTION] = Constant.DB_NULL;
            }
            else
            {
                sQLtableErrorRecord.mRemedialAction = Marshal.PtrToStringAnsi(tgtValPtrs[REMEDIALACTION]).Trim();
                nullInds[REMEDIALACTION] = Constant.DB_NOT_NULL;
            }

        }

        public SQLPOINTER[] CopyToArrayOfSQLPOINTERinGlobalMemory()
        {
            SQLPOINTER[] parameterValuePtrs = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtrs[DATETIMEOFERROR] = Marshal.StringToHGlobalAnsi(mDateTimeOfError);

            parameterValuePtrs[PROVENANCE] = Marshal.StringToHGlobalAnsi(mProvenance);

            parameterValuePtrs[APPLICATIONREPORTINGERROR] = Marshal.StringToHGlobalAnsi(mApplicationReportingError);

            parameterValuePtrs[MICSUSERID] = Marshal.StringToHGlobalAnsi(mMicsUserID);

            parameterValuePtrs[ERRORMESSAGE] = Marshal.StringToHGlobalAnsi(mErrorMessage);

            parameterValuePtrs[REMEDIALACTION] = Marshal.StringToHGlobalAnsi(mRemedialAction);

            return parameterValuePtrs;
        }

        /// <summary>
        /// This method returns a string comprising a complete SQL 'insert' query using
        /// bound parameter values.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string BuildSqlInsertString(string tableName)
        {
            StringBuilder valuesSB = new StringBuilder();
            valuesSB.Append("(?");
            for (int i = 1; i < NUM_COLUMNS; i++)
            {
                valuesSB.Append(", ?");
            }
            valuesSB.Append(")");

            StringBuilder sb = new StringBuilder();
            sb.Append("insert into ");
            sb.Append(tableName);
            sb.Append(" (");
            sb.Append(AllColumnsForSqlSelect);
            sb.Append(") values ");
            sb.Append(valuesSB);

            return sb.ToString();
        }

        //List of all column names for SQL 'select' command, i.e. " cmd, recstat, call1, call2, bndcde, anum, ... , mtime " 
        //Provide read-only access to this 'constant'.
        private static string mAllFieldsAsCSV;
        public static string AllColumnsForSqlSelect
        {
            get { return mAllFieldsAsCSV; }
        }

        //List of all column names with bindings for SQL 'update' command, i.e. " cmd=?, recstat=?, call1=?, call2=?, bndcde=?, anum=?, ... , mtime=? "
        //Provide read-only access to this 'constant'.
        private static string mAllBindingsAsCSV;
        public static string AllColumnsForSqlUpdateAsBindings
        {
            get { return mAllBindingsAsCSV; }
        }

        /// <summary>
        /// This method returns a string comprising a comma-separated sequence
        /// of all column names for use in a SQL 'select' query.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private static string ListOfAllColumnNamesForSQLSelect()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" ");
            for (int i = 0; i < NUM_COLUMNS; i++)
            {
                sb.Append(columnNames[i]);
                if (i != NUM_COLUMNS - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(" ");
            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string comprising a comma-separated sequence
        /// of all column names for use in a SQL 'update' query using
        /// bound parameter values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private static string ListOfAllColumnNamesForSQLUpdate()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" ");
            for (int i = 0; i < NUM_COLUMNS; i++)
            {
                sb.Append(columnNames[i] + "=?");
                if (i != NUM_COLUMNS - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(" ");
            return sb.ToString();
        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[{1}]" +
"(" +
"	[DateTimeOfError] [varchar](64) NOT NULL," +
"	[Provenance] [varchar](64) NOT NULL, " +
"	[ApplicationReportingError] [varchar](64) NOT NULL, " +
"	[MicsUserID] [varchar](64) NOT NULL, " +
"	[ErrorMessage] [varchar](MAX) NOT NULL, " +
"	[RemedialAction] [varchar](MAX) NULL, " +
"	CONSTRAINT[PK_SQL_TABLE_MICS_ERROR_RECORD] PRIMARY KEY CLUSTERED " +
"(" +
"	[DateTimeOfError] ASC" +
")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON[PRIMARY]) " +
"ON[PRIMARY]" ;



    }
}

```
