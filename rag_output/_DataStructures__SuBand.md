# Documented File: SuBand.cs
**Repository Path:** `_DataStructures\SuBand.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using SQLHANDLE = IntPtr;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;

    /// <summary>
    /// This class has fields that are isomorphic with the 'subupt' table 
    /// <b>main.sd_band</b> except for additional fields 'cmd' and 'recstat'.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SuBand
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CMD_SZ)]
        public string cmd;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RECSTAT_SZ)]
        public string recstat;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = BNDCDE_SZ)]
        public string bndcde;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short bandbitpos;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double blo;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double bmidf;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double bhi;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = BADJ_SZ)]
        public string badj;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //--------------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 10;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "bndcde", "bandbitpos", "blo", "bmidf", "bhi", "badj", "mdate", "mtime" };

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int BNDCDE = 2;
        public const int BANDBITPOS = 3;
        public const int BLO = 4;
        public const int BMIDF = 5;
        public const int BHI = 6;
        public const int BADJ = 7;
        public const int MDATE = 8;
        public const int MTIME = 9;

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int BNDCDE_SZ = Constant.BNDCDE_SZ;
        public const int BADJ_SZ = Constant.BADJ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //--------------------------------------------------------------------------

        //Provide read-only access to these members.
        private static string mAllFieldsAsCSV;
        private static string mSubsetOfFieldsAsCSV;
        //List of all column names with bindings for SQL 'update' command.
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
        static SuBand()
        {
            mAllFieldsAsCSV = ListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        public SuBand()
        {
            cmd = "";
            recstat = "";
            bndcde = "";
            badj = "";
            mdate = "";
            mtime = "";
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
            sb.Append("INSERT INTO ");
            sb.Append(tableName);
            sb.Append(" (");
            sb.Append(AllColumnsForSqlSelect);
            sb.Append(") VALUES ");
            sb.Append(valuesSB);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n");
            sb.AppendLine("cmd =        " + cmd);
            sb.AppendLine("recstat =    " + recstat);
            sb.AppendLine("bndcde =     " + bndcde);
            sb.AppendLine("bandbitpos = " + bandbitpos);
            sb.AppendLine("blo =        " + blo);
            sb.AppendLine("bmidf =      " + bmidf);
            sb.AppendLine("bhi =        " + bhi);
            sb.AppendLine("badj =       " + badj);
            sb.AppendLine("mdate =      " + mdate);
            sb.AppendLine("mtime =      " + mtime);
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
            sb.Append("\n ===== SuBand ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "cmd = " + cmd);
            sb.Append("\n" + nullInds[i++] + "      " + "recstat = " + recstat);
            sb.Append("\n" + nullInds[i++] + "      " + "bndcde = " + bndcde);
            sb.Append("\n" + nullInds[i++] + "      " + "bandbitpos = " + bandbitpos);
            sb.Append("\n" + nullInds[i++] + "      " + "blo = " + blo);
            sb.Append("\n" + nullInds[i++] + "      " + "bmidf = " + bmidf);
            sb.Append("\n" + nullInds[i++] + "      " + "bhi = " + bhi);
            sb.Append("\n" + nullInds[i++] + "      " + "badj = " + badj);
            sb.Append("\n" + nullInds[i++] + "      " + "mdate = " + mdate);
            sb.Append("\n" + nullInds[i++] + "      " + "mtime = " + mtime);

            return sb.ToString();
        }

        /// <summary>
		/// This method returns a 'deep copy' of the current object; changes to the 
        /// field values of this object do not cause changes to the deep copy.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public SuBand DeepCopy()
        {
            SuBand suBand = new SuBand();

            suBand.cmd = this.cmd;
            suBand.recstat = this.recstat;
            suBand.bndcde = this.bndcde;
            suBand.bandbitpos = this.bandbitpos;
            suBand.blo = this.blo;
            suBand.bmidf = this.bmidf;
            suBand.bhi = this.bhi;
            suBand.badj = this.badj;
            suBand.mdate = this.mdate;
            suBand.mtime = this.mtime;

            return suBand;
        }

    }
}

```
