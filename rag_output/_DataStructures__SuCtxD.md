# Documented File: SuCtxD.cs
**Repository Path:** `_DataStructures\SuCtxD.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using _Configuration;

namespace _DataStructures
{
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;

    /// <summary>
    /// This class has fields that are isomorphic with the Subsidiary DB table 
    /// <b>main.sd_ctxd</b> except for the additional fields 'cmd' and 'recstat'.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SuCtxD
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CMD_SZ)]
        public string cmd;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RECSTAT_SZ)]
        public string recstat;

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
        public const int NUM_COLUMNS = 9;

        //-----------------------------------------------------------------

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int TFCR_SZ = Constant.TRAFCODE_SZ;
        public const int TFCI_SZ = Constant.TRAFCODE_SZ;
        public const int RXEQP_SZ = Constant.ECODE_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //-----------------------------------------------------------------

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int TFCR = 2;
        public const int TFCI = 3;
        public const int RXEQP = 4;
        public const int FSEP = 5;
        public const int RQ = 6;
        public const int MDATE = 7;
        public const int MTIME = 8;

        //-----------------------------------------------------------------

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "tfcr", "tfci", "rxeqp", "fsep", "rq", "mdate", "mtime" };

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
        static SuCtxD()
        {
            mAllFieldsAsCSV = ListOfColumnNamesForSQLSelect();
            mSubsetOfFieldsAsCSV = SubListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        public SuCtxD()
        {
            cmd = "";
            recstat = "";
            tfcr = "";
            tfci = "";
            rxeqp = "";
            mdate = "";
            mtime = "";
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
            sb.Append("\n ===== SuCtxD ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "cmd = " + cmd);
            sb.Append("\n" + nullInds[i++] + "      " + "recstat = " + recstat);
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

            sb.Append(nullInds[SuCtxD.CMD] == Constant.DB_NULL ? "NULL, " : "'" + cmd.ToString() + "', ");
            sb.Append(nullInds[SuCtxD.RECSTAT] == Constant.DB_NULL ? "NULL, " : "'" + recstat.ToString() + "', ");
            sb.Append(nullInds[SuCtxD.TFCR] == Constant.DB_NULL ? "NULL, " : "'" + tfcr.ToString() + "', ");
            sb.Append(nullInds[SuCtxD.TFCI] == Constant.DB_NULL ? "NULL, " : "'" + tfci.ToString() + "', ");
            sb.Append(nullInds[SuCtxD.RXEQP] == Constant.DB_NULL ? "NULL, " : "'" + rxeqp.ToString() + "', ");
            sb.Append(nullInds[SuCtxD.FSEP] == Constant.DB_NULL ? "NULL, " : "'" + fsep.ToString() + "', ");
            sb.Append(nullInds[SuCtxD.RQ] == Constant.DB_NULL ? "NULL, " : "'" + rq.ToString() + "', ");
            sb.Append(nullInds[SuCtxD.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SuCtxD.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

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

            sb.Append(columnNames[SuCtxD.CMD] + " = " + (nullInds[SuCtxD.CMD] == Constant.DB_NULL ? "NULL, " : "'" + cmd.ToString() + "', "));
            sb.Append(columnNames[SuCtxD.RECSTAT] + " = " + (nullInds[SuCtxD.RECSTAT] == Constant.DB_NULL ? "NULL, " : "'" + recstat.ToString() + "', "));
            sb.Append(columnNames[SuCtxD.TFCR] + " = " + (nullInds[SuCtxD.TFCR] == Constant.DB_NULL ? "NULL, " : "'" + tfcr.ToString() + "', "));
            sb.Append(columnNames[SuCtxD.TFCI] + " = " + (nullInds[SuCtxD.TFCI] == Constant.DB_NULL ? "NULL, " : "'" + tfci.ToString() + "', "));
            sb.Append(columnNames[SuCtxD.RXEQP] + " = " + (nullInds[SuCtxD.RXEQP] == Constant.DB_NULL ? "NULL, " : "'" + rxeqp.ToString() + "', "));
            sb.Append(columnNames[SuCtxD.FSEP] + " = " + (nullInds[SuCtxD.FSEP] == Constant.DB_NULL ? "NULL, " : "'" + fsep.ToString() + "', "));
            sb.Append(columnNames[SuCtxD.RQ] + " = " + (nullInds[SuCtxD.RQ] == Constant.DB_NULL ? "NULL, " : "'" + rq.ToString() + "', "));
            sb.Append(columnNames[SuCtxD.MDATE] + " = " + (nullInds[SuCtxD.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SuCtxD.MTIME] + " = " + (nullInds[SuCtxD.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

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

            parameterValuePtrs[CMD] = Marshal.StringToHGlobalAnsi(cmd);

            parameterValuePtrs[RECSTAT] = Marshal.StringToHGlobalAnsi(recstat);

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

    }
}

```
