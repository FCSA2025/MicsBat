# Documented File: SuCtx.cs
**Repository Path:** `_DataStructures\SuCtx.cs`
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
    /// <b>main.sd_ctx</b> except for the additional fields 'cmd' and 'recstat'.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SuCtx
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
        public const int NUM_COLUMNS = 12;

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

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int TFCR = 2;
        public const int TFCI = 3;
        public const int RXEQP = 4;
        public const int RQCO = 5;
        public const int RQCULL = 6;
        public const int RQWRST = 7;
        public const int CTXNDP = 8;
        public const int CTXDESC = 9;
        public const int MDATE = 10;
        public const int MTIME = 11;

        //------------------------------------------------------------------

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "tfcr", "tfci", "rxeqp", "rqco", "rqcull", "rqwrst", "ctxndp", "ctxdesc", "mdate", "mtime" };

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
        static SuCtx()
        {
            mAllFieldsAsCSV = ListOfColumnNamesForSQLSelect();
            mSubsetOfFieldsAsCSV = SubListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        public SuCtx()
        {
            cmd = "";
            recstat = "";
            tfcr = "";
            tfci = "";
            rxeqp = "";
            ctxdesc = "";
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
            sb.Append("\n ===== SuCtx ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "cmd = " + cmd);
            sb.Append("\n" + nullInds[i++] + "      " + "recstat = " + recstat);
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

            sb.Append(nullInds[SuCtx.CMD] == Constant.DB_NULL ? "NULL, " : "'" + cmd.ToString() + "', ");
            sb.Append(nullInds[SuCtx.RECSTAT] == Constant.DB_NULL ? "NULL, " : "'" + recstat.ToString() + "', ");
            sb.Append(nullInds[SuCtx.TFCR] == Constant.DB_NULL ? "NULL, " : "'" + tfcr.ToString() + "', ");
            sb.Append(nullInds[SuCtx.TFCI] == Constant.DB_NULL ? "NULL, " : "'" + tfci.ToString() + "', ");
            sb.Append(nullInds[SuCtx.RXEQP] == Constant.DB_NULL ? "NULL, " : "'" + rxeqp.ToString() + "', ");
            sb.Append(nullInds[SuCtx.RQCO] == Constant.DB_NULL ? "NULL, " : "'" + rqco.ToString() + "', ");
            sb.Append(nullInds[SuCtx.RQCULL] == Constant.DB_NULL ? "NULL, " : "'" + rqcull.ToString() + "', ");
            sb.Append(nullInds[SuCtx.RQWRST] == Constant.DB_NULL ? "NULL, " : "'" + rqwrst.ToString() + "', ");
            sb.Append(nullInds[SuCtx.CTXNDP] == Constant.DB_NULL ? "NULL, " : "'" + ctxndp.ToString() + "', ");
            sb.Append(nullInds[SuCtx.CTXDESC] == Constant.DB_NULL ? "NULL, " : "'" + ctxdesc.ToString() + "', ");
            sb.Append(nullInds[SuCtx.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SuCtx.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

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

            sb.Append(columnNames[SuCtx.CMD] + " = " + (nullInds[SuCtx.CMD] == Constant.DB_NULL ? "NULL, " : "'" + cmd.ToString() + "', "));
            sb.Append(columnNames[SuCtx.RECSTAT] + " = " + (nullInds[SuCtx.RECSTAT] == Constant.DB_NULL ? "NULL, " : "'" + recstat.ToString() + "', "));
            sb.Append(columnNames[SuCtx.TFCR] + " = " + (nullInds[SuCtx.TFCR] == Constant.DB_NULL ? "NULL, " : "'" + tfcr.ToString() + "', "));
            sb.Append(columnNames[SuCtx.TFCI] + " = " + (nullInds[SuCtx.TFCI] == Constant.DB_NULL ? "NULL, " : "'" + tfci.ToString() + "', "));
            sb.Append(columnNames[SuCtx.RXEQP] + " = " + (nullInds[SuCtx.RXEQP] == Constant.DB_NULL ? "NULL, " : "'" + rxeqp.ToString() + "', "));
            sb.Append(columnNames[SuCtx.RQCO] + " = " + (nullInds[SuCtx.RQCO] == Constant.DB_NULL ? "NULL, " : "'" + rqco.ToString() + "', "));
            sb.Append(columnNames[SuCtx.RQCULL] + " = " + (nullInds[SuCtx.RQCULL] == Constant.DB_NULL ? "NULL, " : "'" + rqcull.ToString() + "', "));
            sb.Append(columnNames[SuCtx.RQWRST] + " = " + (nullInds[SuCtx.RQWRST] == Constant.DB_NULL ? "NULL, " : "'" + rqwrst.ToString() + "', "));
            sb.Append(columnNames[SuCtx.CTXNDP] + " = " + (nullInds[SuCtx.CTXNDP] == Constant.DB_NULL ? "NULL, " : "'" + ctxndp.ToString() + "', "));
            sb.Append(columnNames[SuCtx.CTXDESC] + " = " + (nullInds[SuCtx.CTXDESC] == Constant.DB_NULL ? "NULL, " : "'" + ctxdesc.ToString() + "', "));
            sb.Append(columnNames[SuCtx.MDATE] + " = " + (nullInds[SuCtx.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SuCtx.MTIME] + " = " + (nullInds[SuCtx.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

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



    }
}

```
