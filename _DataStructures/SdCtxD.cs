using System;
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
    /// <b>main.sd_ctxd</b> .
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SdCtxD
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TFCR_SZ)]
        public string tfcr;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TFCI_SZ)]
        public string tfci;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RXEQP_SZ)]
        public string rxeqp;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public double fsep;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rq;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //-----------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 7;

        //-----------------------------------------------------------------

        public const int TFCR_SZ = Constant.TRAFCODE_SZ;
        public const int TFCI_SZ = Constant.TRAFCODE_SZ;
        public const int RXEQP_SZ = Constant.ECODE_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //-----------------------------------------------------------------

        public const int TFCR = 0;
        public const int TFCI = 1;
        public const int RXEQP = 2;
        public const int FSEP = 3;
        public const int RQ = 4;
        public const int MDATE = 5;
        public const int MTIME = 6;

        //-----------------------------------------------------------------

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "tfcr", "tfci", "rxeqp", "fsep", "rq", "mdate", "mtime" };

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
        static SdCtxD()
        {
            mAllFieldsAsCSV = ListOfColumnNamesForSQLSelect();
            mSubsetOfFieldsAsCSV = SubListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        public SdCtxD()
        {
            tfcr = "";
            tfci = "";
            rxeqp = "";
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
            sb.Append("   fsep =" + fsep);
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
            sb.Append("\n ===== SdCtxd ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "tfcr = " + tfcr);
            sb.Append("\n" + nullInds[i++] + "      " + "tfci = " + tfci);
            sb.Append("\n" + nullInds[i++] + "      " + "rxeqp = " + rxeqp);
            sb.Append("\n" + nullInds[i++] + "      " + "fsep = " + fsep);
            sb.Append("\n" + nullInds[i++] + "      " + "rq = " + rq);
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

            sb.Append(nullInds[SdCtxD.TFCR] == Constant.DB_NULL ? "NULL, " : "'" + tfcr.ToString() + "', ");
            sb.Append(nullInds[SdCtxD.TFCI] == Constant.DB_NULL ? "NULL, " : "'" + tfci.ToString() + "', ");
            sb.Append(nullInds[SdCtxD.RXEQP] == Constant.DB_NULL ? "NULL, " : "'" + rxeqp.ToString() + "', ");
            sb.Append(nullInds[SdCtxD.FSEP] == Constant.DB_NULL ? "NULL, " : "'" + fsep.ToString() + "', ");
            sb.Append(nullInds[SdCtxD.RQ] == Constant.DB_NULL ? "NULL, " : "'" + rq.ToString() + "', ");
            sb.Append(nullInds[SdCtxD.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SdCtxD.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

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

            sb.Append(columnNames[SdCtxD.TFCR] + " = " + (nullInds[SdCtxD.TFCR] == Constant.DB_NULL ? "NULL, " : "'" + tfcr.ToString() + "', "));
            sb.Append(columnNames[SdCtxD.TFCI] + " = " + (nullInds[SdCtxD.TFCI] == Constant.DB_NULL ? "NULL, " : "'" + tfci.ToString() + "', "));
            sb.Append(columnNames[SdCtxD.RXEQP] + " = " + (nullInds[SdCtxD.RXEQP] == Constant.DB_NULL ? "NULL, " : "'" + rxeqp.ToString() + "', "));
            sb.Append(columnNames[SdCtxD.FSEP] + " = " + (nullInds[SdCtxD.FSEP] == Constant.DB_NULL ? "NULL, " : "'" + fsep.ToString() + "', "));
            sb.Append(columnNames[SdCtxD.RQ] + " = " + (nullInds[SdCtxD.RQ] == Constant.DB_NULL ? "NULL, " : "'" + rq.ToString() + "', "));
            sb.Append(columnNames[SdCtxD.MDATE] + " = " + (nullInds[SdCtxD.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SdCtxD.MTIME] + " = " + (nullInds[SdCtxD.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

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

            parameterValuePtrs[FSEP] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = fsep;
            Marshal.Copy(D, 0, parameterValuePtrs[FSEP], 1);

            parameterValuePtrs[RQ] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = rq;
            Marshal.Copy(F, 0, parameterValuePtrs[RQ], 1);

            parameterValuePtrs[MDATE] = Marshal.StringToHGlobalAnsi(mdate);

            parameterValuePtrs[MTIME] = Marshal.StringToHGlobalAnsi(mtime);

            return parameterValuePtrs;
        }

        /// <summary>
        /// This method returns instantiated SdCtxD and SQLLEN[] nullInds objects whose
        /// members are set i.a.w. a prescribed SdCtxD object and its associated nullInds; 
        /// SdCtxD has identical members to SdCtxD except for 'cmd'  and 'recstat'.
        /// </summary>
        /// <param name="suCtxD"></param>
        /// <param name="suCtxDNullInds"></param>
        /// <param name="sdCtxD"></param>
        /// <param name="sdCtxDNullInds"></param>
        public static void MakeSdFromSu(SuCtxD suCtxD, SQLLEN[] suCtxDNullInds, out SdCtxD sdCtxD, out SQLLEN[] sdCtxDNullInds)
        {
            // SdCtxD has identical members to SdCtxD except for 'cmd' and 'recstat'.

            sdCtxD = new SdCtxD();
            sdCtxDNullInds = new SQLLEN[SdCtxD.NUM_COLUMNS];

            sdCtxD.tfcr = suCtxD.tfcr;
            sdCtxDNullInds[SdCtxD.TFCR] = suCtxDNullInds[SdCtxD.TFCR];

            sdCtxD.tfci = suCtxD.tfci;
            sdCtxDNullInds[SdCtxD.TFCI] = suCtxDNullInds[SdCtxD.TFCI];

            sdCtxD.rxeqp = suCtxD.rxeqp;
            sdCtxDNullInds[SdCtxD.RXEQP] = suCtxDNullInds[SdCtxD.RXEQP];

            sdCtxD.fsep = suCtxD.fsep;
            sdCtxDNullInds[SdCtxD.FSEP] = suCtxDNullInds[SdCtxD.FSEP];

            sdCtxD.rq = suCtxD.rq;
            sdCtxDNullInds[SdCtxD.RQ] = suCtxDNullInds[SdCtxD.RQ];

            sdCtxD.mdate = suCtxD.mdate;
            sdCtxDNullInds[SdCtxD.MDATE] = suCtxDNullInds[SdCtxD.MDATE];

            sdCtxD.mtime = suCtxD.mtime;
            sdCtxDNullInds[SdCtxD.MTIME] = suCtxDNullInds[SdCtxD.MTIME];
        }

        /// <summary>
        /// This method returns an IntPtr[] whose elements point to the start addresses of possibly 
        /// non-contiguous blocks of global (heap) memory, each of a sufficient size to hold the 
        /// value of a specific member of this object; each of the pointers is then registered
        /// with ODBC as a 'binding'. Similarly for the nullInd array associated with this object.
        /// The binding order is that of the columns in the table main.sd_ctxd; the ODBC binding
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
            Bind.BindBufferToString(hStmt, TFCR + 1, tgtValPtrs[TFCR], SdCtxD.TFCR_SZ, nullIndPtrs[TFCR]);

            tgtValPtrs[TFCI] = Marshal.AllocHGlobal(TFCI_SZ + 1);
            nullIndPtrs[TFCI] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TFCI + 1, tgtValPtrs[TFCI], SdCtxD.TFCI_SZ, nullIndPtrs[TFCI]);

            tgtValPtrs[RXEQP] = Marshal.AllocHGlobal(RXEQP_SZ + 1);
            nullIndPtrs[RXEQP] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, RXEQP + 1, tgtValPtrs[RXEQP], SdCtxD.RXEQP_SZ, nullIndPtrs[RXEQP]);

            tgtValPtrs[FSEP] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[FSEP] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, FSEP + 1, tgtValPtrs[FSEP], nullIndPtrs[FSEP]);

            tgtValPtrs[RQ] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[RQ] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, RQ + 1, tgtValPtrs[RQ], nullIndPtrs[RQ]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], SdCtxD.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], SdCtxD.MTIME_SZ, nullIndPtrs[MTIME]);
        }

        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdCtxD object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="sdCtxD"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SdCtxD sdCtxD, out SQLLEN[] nullInds)
        {
            sdCtxD = new SdCtxD();
            nullInds = NullHelper.CreateArrayOfNullInd(SdCtxD.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[TFCR]);
            if (nullInd == Constant.DB_NULL)
            {
                sdCtxD.tfcr = "";
                nullInds[TFCR] = Constant.DB_NULL;
            }
            else
            {
                sdCtxD.tfcr = Marshal.PtrToStringAnsi(tgtValPtrs[TFCR]).Trim();
                nullInds[TFCR] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TFCI]);
            if (nullInd == Constant.DB_NULL)
            {
                sdCtxD.tfci = "";
                nullInds[TFCI] = Constant.DB_NULL;
            }
            else
            {
                sdCtxD.tfci = Marshal.PtrToStringAnsi(tgtValPtrs[TFCI]).Trim();
                nullInds[TFCI] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RXEQP]);
            if (nullInd == Constant.DB_NULL)
            {
                sdCtxD.rxeqp = "";
                nullInds[RXEQP] = Constant.DB_NULL;
            }
            else
            {
                sdCtxD.rxeqp = Marshal.PtrToStringAnsi(tgtValPtrs[RXEQP]).Trim();
                nullInds[RXEQP] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[FSEP]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[FSEP], D, 0, 1);
                sdCtxD.fsep = D[0];
                nullInds[FSEP] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RQ]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RQ], F, 0, 1);
                sdCtxD.rq = F[0];
                nullInds[RQ] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdCtxD.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                sdCtxD.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdCtxD.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                sdCtxD.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[sd_ctxd](" +
    "[tfcr] [char](6) NOT NULL," +
    "[tfci] [char](6) NOT NULL," +
    "[rxeqp] [char](8) NOT NULL," +
    "[fsep] [real] NOT NULL," +
    "[rq] [real] NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
" CONSTRAINT [PK_sd_ctxd] PRIMARY KEY CLUSTERED " +
"(" +
    "[tfcr] ASC," +
    "[tfci] ASC," +
    "[rxeqp] ASC," +
    "[fsep] ASC" +
") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";





    }
}

