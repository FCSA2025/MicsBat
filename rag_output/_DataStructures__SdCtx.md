# Documented File: SdCtx.cs
**Repository Path:** `_DataStructures\SdCtx.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using _Configuration;

namespace _DataStructures
{
    using _NewLib;
    using SQLHANDLE = IntPtr;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;

    /// <summary>
    /// This class has fields that are isomorphic with the Subsidiary DB table 
    /// <b>main.sd_ctx</b> .
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SdCtx
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TFCR_SZ)]
        public string tfcr;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TFCI_SZ)]
        public string tfci;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RXEQP_SZ)]
        public string rxeqp;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rqco;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rqcull;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rqwrst;

        [MarshalAsAttribute(UnmanagedType.I2)]
        public short ctxndp;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CTXDESC_SZ)]
        public string ctxdesc;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //-----------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 10;

        //-----------------------------------------------------------------

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int TFCR_SZ = Constant.TRAFCODE_SZ;
        public const int TFCI_SZ = Constant.TRAFCODE_SZ;
        public const int RXEQP_SZ = Constant.ECODE_SZ;
        public const int CTXDESC_SZ = 41;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //-----------------------------------------------------------------

        public const int TFCR = 0;
        public const int TFCI = 1;
        public const int RXEQP = 2;
        public const int RQCO = 3;
        public const int RQCULL = 4;
        public const int RQWRST = 5;
        public const int CTXNDP = 6;
        public const int CTXDESC = 7;
        public const int MDATE = 8;
        public const int MTIME = 9;

        //------------------------------------------------------------------

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "tfcr", "tfci", "rxeqp", "rqco", "rqcull", "rqwrst", "ctxndp", "ctxdesc", "mdate", "mtime" };

        //Provide read-only access to these members.
        private static string mAllFieldsAsCSV;
        private static string mSubsetOfFieldsAsCSV;
        //List of all column names with bindings for SQL 'update' command, i.e. " cmd=?, recstat=?, call1=?, call2=?, bndcde=?, anum=?, ... , mtime=? "
        private static string mAllBindingsAsCSV;

        public static string AllColumnsForSqlSelect
        {
            get { return mAllFieldsAsCSV; }
        }

        public static string SubsetOfColumnsForSqlSelect
        {
            get { return mSubsetOfFieldsAsCSV; }
        }

        public static string AllColumnsForSqlUpdateAsBindings
        {
            get { return mAllBindingsAsCSV; }
        }

        //----------------------------------------------------------------

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static SdCtx()
        {
            mAllFieldsAsCSV = ListOfColumnNamesForSQLSelect();
            mSubsetOfFieldsAsCSV = SubListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        public SdCtx()
        {
            tfcr = "";
            tfci = "";
            rxeqp = "";
            ctxdesc = "";
            mdate = "";
            mtime = "";
        }

        /// <summary>
        /// This method returns a string that concatenates the 'key'
        /// fields {tfcr, tfci, rxeqp}.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string KeysToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("tfcr = " + tfcr);
            sb.Append("   tfci = " + tfci);
            sb.Append("   rxeqp =" + rxeqp);
            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the member fields together with
        /// their associated ODBC nullInds.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringWN(SQLLEN[] nullInds)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("\n ===== SdCtx ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "tfcr = " + tfcr);
            sb.Append("\n" + nullInds[i++] + "      " + "tfci = " + tfci);
            sb.Append("\n" + nullInds[i++] + "      " + "rxeqp = " + rxeqp);
            sb.Append("\n" + nullInds[i++] + "      " + "rqco = " + rqco);
            sb.Append("\n" + nullInds[i++] + "      " + "rqcull = " + rqcull);
            sb.Append("\n" + nullInds[i++] + "      " + "rqwrst = " + rqwrst);
            sb.Append("\n" + nullInds[i++] + "      " + "ctxndp = " + ctxndp);
            sb.Append("\n" + nullInds[i++] + "      " + "ctxdesc = " + ctxdesc);
            sb.Append("\n" + nullInds[i++] + "      " + "mdate = " + mdate);
            sb.Append("\n" + nullInds[i++] + "      " + "mtime = " + mtime);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns the member values of an object formatted
        /// as a CSV string that can be used to provide the VALUES in an SQL INSERT query.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringAsCSV(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[SdCtx.TFCR] == Constant.DB_NULL ? "NULL, " : "'" + tfcr.ToString() + "', ");
            sb.Append(nullInds[SdCtx.TFCI] == Constant.DB_NULL ? "NULL, " : "'" + tfci.ToString() + "', ");
            sb.Append(nullInds[SdCtx.RXEQP] == Constant.DB_NULL ? "NULL, " : "'" + rxeqp.ToString() + "', ");
            sb.Append(nullInds[SdCtx.RQCO] == Constant.DB_NULL ? "NULL, " : "'" + rqco.ToString() + "', ");
            sb.Append(nullInds[SdCtx.RQCULL] == Constant.DB_NULL ? "NULL, " : "'" + rqcull.ToString() + "', ");
            sb.Append(nullInds[SdCtx.RQWRST] == Constant.DB_NULL ? "NULL, " : "'" + rqwrst.ToString() + "', ");
            sb.Append(nullInds[SdCtx.CTXNDP] == Constant.DB_NULL ? "NULL, " : "'" + ctxndp.ToString() + "', ");
            sb.Append(nullInds[SdCtx.CTXDESC] == Constant.DB_NULL ? "NULL, " : "'" + ctxdesc.ToString() + "', ");
            sb.Append(nullInds[SdCtx.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SdCtx.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

            return sb.ToString();
        }

        /// <summary>
        /// This method returns the member values of an object formatted
        /// as a string of columnName = 'value' expressions that can be used
        /// in the SET clause of an SQL UPDATE query.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringAsCSVequates(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(columnNames[SdCtx.TFCR] + " = " + (nullInds[SdCtx.TFCR] == Constant.DB_NULL ? "NULL, " : "'" + tfcr.ToString() + "', "));
            sb.Append(columnNames[SdCtx.TFCI] + " = " + (nullInds[SdCtx.TFCI] == Constant.DB_NULL ? "NULL, " : "'" + tfci.ToString() + "', "));
            sb.Append(columnNames[SdCtx.RXEQP] + " = " + (nullInds[SdCtx.RXEQP] == Constant.DB_NULL ? "NULL, " : "'" + rxeqp.ToString() + "', "));
            sb.Append(columnNames[SdCtx.RQCO] + " = " + (nullInds[SdCtx.RQCO] == Constant.DB_NULL ? "NULL, " : "'" + rqco.ToString() + "', "));
            sb.Append(columnNames[SdCtx.RQCULL] + " = " + (nullInds[SdCtx.RQCULL] == Constant.DB_NULL ? "NULL, " : "'" + rqcull.ToString() + "', "));
            sb.Append(columnNames[SdCtx.RQWRST] + " = " + (nullInds[SdCtx.RQWRST] == Constant.DB_NULL ? "NULL, " : "'" + rqwrst.ToString() + "', "));
            sb.Append(columnNames[SdCtx.CTXNDP] + " = " + (nullInds[SdCtx.CTXNDP] == Constant.DB_NULL ? "NULL, " : "'" + ctxndp.ToString() + "', "));
            sb.Append(columnNames[SdCtx.CTXDESC] + " = " + (nullInds[SdCtx.CTXDESC] == Constant.DB_NULL ? "NULL, " : "'" + ctxdesc.ToString() + "', "));
            sb.Append(columnNames[SdCtx.MDATE] + " = " + (nullInds[SdCtx.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SdCtx.MTIME] + " = " + (nullInds[SdCtx.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

            return sb.ToString();
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
        /// of all member field values except 'cmd' and 'recstat' for use in 
        /// a SQL 'select' query.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private static string SubListOfColumnNamesForSQLSelect()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" ");
            for (int i = 2; i < NUM_COLUMNS; i++)
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
        /// of all member field values except 'cmd' and 'recstat' for use in 
        /// a SQL 'select' query.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private static string ListOfColumnNamesForSQLSelect()
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

        /// <summary>
        /// For each field of this object, the method allocates a type-specific amount
        /// of global (heap) memory, copies the value of the field into it, and returns
        /// an IntPtr[] containing all of the start addresses.
        /// </summary>
        /// <returns></returns>
        public SQLPOINTER[] CopyToArrayOfSQLPOINTERinGlobalMemory()
        {
            //Can only copy arrays of float and double into native memory using Marshal method.
            float[] F = new float[1];
            double[] D = new double[1];

            SQLPOINTER[] parameterValuePtrs = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtrs[TFCR] = Marshal.StringToHGlobalAnsi(tfcr);

            parameterValuePtrs[TFCI] = Marshal.StringToHGlobalAnsi(tfci);

            parameterValuePtrs[RXEQP] = Marshal.StringToHGlobalAnsi(rxeqp);

            parameterValuePtrs[RQCO] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = rqco;
            Marshal.Copy(F, 0, parameterValuePtrs[RQCO], 1);

            parameterValuePtrs[RQCULL] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = rqcull;
            Marshal.Copy(F, 0, parameterValuePtrs[RQCULL], 1);

            parameterValuePtrs[RQWRST] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = rqwrst;
            Marshal.Copy(F, 0, parameterValuePtrs[RQWRST], 1);

            parameterValuePtrs[CTXNDP] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtrs[CTXNDP], ctxndp);

            parameterValuePtrs[CTXDESC] = Marshal.StringToHGlobalAnsi(ctxdesc);

            parameterValuePtrs[MDATE] = Marshal.StringToHGlobalAnsi(mdate);

            parameterValuePtrs[MTIME] = Marshal.StringToHGlobalAnsi(mtime);

            return parameterValuePtrs;
        }

        /// <summary>
        /// This method returns instantiated SdCtx and SQLLEN[] nullInds objects whose
        /// members are set i.a.w. a prescribed SuCtx object and its associated nullInds; 
        /// SdCtx has identical members to SuCtx except for 'cmd'  and 'recstat'.
        /// </summary>
        /// <param name="suCtx"></param>
        /// <param name="suCtxNullInds"></param>
        /// <param name="sdCtx"></param>
        /// <param name="sdCtxNullInds"></param>
        public static void MakeSdFromSu(SuCtx suCtx, SQLLEN[] suCtxNullInds, out SdCtx sdCtx, out SQLLEN[] sdCtxNullInds)
        {
            // SdCtx has identical members to SuCtx except for 'cmd' and 'recstat'.

            sdCtx = new SdCtx();
            sdCtxNullInds = new SQLLEN[SdCtx.NUM_COLUMNS];

            sdCtx.tfcr = suCtx.tfcr;
            sdCtxNullInds[SdCtx.TFCR] = suCtxNullInds[SuCtx.TFCR];

            sdCtx.tfci = suCtx.tfci;
            sdCtxNullInds[SdCtx.TFCI] = suCtxNullInds[SuCtx.TFCI];

            sdCtx.rxeqp = suCtx.rxeqp;
            sdCtxNullInds[SdCtx.RXEQP] = suCtxNullInds[SuCtx.RXEQP];

            sdCtx.rqco = suCtx.rqco;
            sdCtxNullInds[SdCtx.RQCO] = suCtxNullInds[SuCtx.RQCO];

            sdCtx.rqcull = suCtx.rqcull;
            sdCtxNullInds[SdCtx.RQCULL] = suCtxNullInds[SuCtx.RQCULL];

            sdCtx.rqwrst = suCtx.rqwrst;
            sdCtxNullInds[SdCtx.RQWRST] = suCtxNullInds[SuCtx.RQWRST];

            sdCtx.ctxndp = suCtx.ctxndp;
            sdCtxNullInds[SdCtx.CTXNDP] = suCtxNullInds[SuCtx.CTXNDP];

            sdCtx.ctxdesc = suCtx.ctxdesc;
            sdCtxNullInds[SdCtx.CTXDESC] = suCtxNullInds[SuCtx.CTXDESC];

            sdCtx.mdate = suCtx.mdate;
            sdCtxNullInds[SdCtx.MDATE] = suCtxNullInds[SuCtx.MDATE];

            sdCtx.mtime = suCtx.mtime;
            sdCtxNullInds[SdCtx.MTIME] = suCtxNullInds[SuCtx.MTIME];
        }

        /// <summary>
        /// This method returns an IntPtr[] whose elements point to the start addresses of possibly 
        /// non-contiguous blocks of global (heap) memory, each of a sufficient size to hold the 
        /// value of a specific member of this object; each of the pointers is then registered
        /// with ODBC as a 'binding'. Similarly for the nullInd array associated with this object.
        /// The binding order is that of the columns in the table main.sd_ctx; the ODBC binding
        /// 'count' starts at zero.
        /// Thus, this method creates the parameter bindings prior to an SQL / ODBC 'fetch', 'update' or 
        /// 'insert' query.
        /// </summary>
        /// <param name="hStmt"></param>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        public static void BindPtrsToCols(SQLHANDLE hStmt, out SQLPOINTER[] tgtValPtrs, out SQLPOINTER[] nullIndPtrs)
        {
            tgtValPtrs = new SQLPOINTER[NUM_COLUMNS];
            nullIndPtrs = new SQLPOINTER[NUM_COLUMNS];

            tgtValPtrs[TFCR] = Marshal.AllocHGlobal(TFCR_SZ + 1);
            nullIndPtrs[TFCR] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TFCR + 1, tgtValPtrs[TFCR], SdCtx.TFCR_SZ, nullIndPtrs[TFCR]);

            tgtValPtrs[TFCI] = Marshal.AllocHGlobal(TFCI_SZ + 1);
            nullIndPtrs[TFCI] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TFCI + 1, tgtValPtrs[TFCI], SdCtx.TFCI_SZ, nullIndPtrs[TFCI]);

            tgtValPtrs[RXEQP] = Marshal.AllocHGlobal(RXEQP_SZ + 1);
            nullIndPtrs[RXEQP] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, RXEQP + 1, tgtValPtrs[RXEQP], SdCtx.RXEQP_SZ, nullIndPtrs[RXEQP]);

            tgtValPtrs[RQCO] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[RQCO] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, RQCO + 1, tgtValPtrs[RQCO], nullIndPtrs[RQCO]);

            tgtValPtrs[RQCULL] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[RQCULL] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, RQCULL + 1, tgtValPtrs[RQCULL], nullIndPtrs[RQCULL]);

            tgtValPtrs[RQWRST] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[RQWRST] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, RQWRST + 1, tgtValPtrs[RQWRST], nullIndPtrs[RQWRST]);

            tgtValPtrs[CTXNDP] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[CTXNDP] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, CTXNDP + 1, tgtValPtrs[CTXNDP], nullIndPtrs[CTXNDP]);

            tgtValPtrs[CTXDESC] = Marshal.AllocHGlobal(CTXDESC_SZ + 1);
            nullIndPtrs[CTXDESC] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CTXDESC + 1, tgtValPtrs[CTXDESC], SdCtx.CTXDESC_SZ, nullIndPtrs[CTXDESC]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], SdCtx.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], SdCtx.MTIME_SZ, nullIndPtrs[MTIME]);
        }

        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdCtx object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="sdCtx"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SdCtx sdCtx, out SQLLEN[] nullInds)
        {
            sdCtx = new SdCtx();
            nullInds = NullHelper.CreateArrayOfNullInd(SdCtx.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[TFCR]);
            if (nullInd == Constant.DB_NULL)
            {
                sdCtx.tfcr = "";
                nullInds[TFCR] = Constant.DB_NULL;
            }
            else
            {
                sdCtx.tfcr = Marshal.PtrToStringAnsi(tgtValPtrs[TFCR]).Trim();
                nullInds[TFCR] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TFCI]);
            if (nullInd == Constant.DB_NULL)
            {
                sdCtx.tfci = "";
                nullInds[TFCI] = Constant.DB_NULL;
            }
            else
            {
                sdCtx.tfci = Marshal.PtrToStringAnsi(tgtValPtrs[TFCI]).Trim();
                nullInds[TFCI] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RXEQP]);
            if (nullInd == Constant.DB_NULL)
            {
                sdCtx.rxeqp = "";
                nullInds[RXEQP] = Constant.DB_NULL;
            }
            else
            {
                sdCtx.rxeqp = Marshal.PtrToStringAnsi(tgtValPtrs[RXEQP]).Trim();
                nullInds[RXEQP] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RQCO]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RQCO], F, 0, 1);
                sdCtx.rqco = F[0];
                nullInds[RQCO] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RQCULL]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RQCULL], F, 0, 1);
                sdCtx.rqcull = F[0];
                nullInds[RQCULL] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RQWRST]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RQWRST], F, 0, 1);
                sdCtx.rqwrst = F[0];
                nullInds[RQWRST] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CTXNDP]);
            if (nullInd != Constant.DB_NULL)
            {
                sdCtx.ctxndp = Marshal.ReadInt16(tgtValPtrs[CTXNDP]);
                nullInds[CTXNDP] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CTXDESC]);
            if (nullInd == Constant.DB_NULL)
            {
                sdCtx.ctxdesc = "";
                nullInds[CTXDESC] = Constant.DB_NULL;
            }
            else
            {
                sdCtx.ctxdesc = Marshal.PtrToStringAnsi(tgtValPtrs[CTXDESC]).Trim();
                nullInds[CTXDESC] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdCtx.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                sdCtx.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdCtx.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                sdCtx.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

        }



        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[sd_ctx](" +
    "[tfcr] [char](6) NOT NULL," +
    "[tfci] [char](6) NOT NULL," +
    "[rxeqp] [char](8) NOT NULL," +
    "[rqco] [real] NULL," +
    "[rqcull] [real] NULL," +
    "[rqwrst] [real] NULL," +
    "[ctxndp] [smallint] NULL," +
    "[ctxdesc] [char](40) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
" CONSTRAINT [PK_sd_ctx] PRIMARY KEY CLUSTERED " +
"(" +
    "[tfcr] ASC," +
    "[tfci] ASC," +
    "[rxeqp] ASC" +
") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";






    }
}


```
