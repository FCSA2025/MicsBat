# Documented File: SuPlan.cs
**Repository Path:** `_DataStructures\SuPlan.cs`
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
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class has fields that are isomorphic with the 'subupt' table <b>main.sd_plan</b>
    /// except for the additional fields 'cmd' and 'recstat'.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SuPlan
    {
        // IMPORTANT!
        // =========
        // The following qty. 10 member values correspond to the legacy native 
        // structure suPlan_; member names are prescribed to be identical.
        // The order of appearance of these qty. 10 field members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CMD_SZ)]
        public string cmd;    //#0
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RECSTAT_SZ)]
        public string recstat;    //#1
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SBAND_SZ)]
        public string sband;    //#2
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SPLAN_SZ)]
        public string splan;    //#3
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SRSP_SZ)]
        public string srsp;    //#4
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SRSPISS_SZ)]
        public string srspiss;    //#5
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CONFORM_SZ)]
        public string conform;    //#6
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = USCAN_SZ)]
        public string uscan;    //#7
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;    //#8
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;    //#9

        //----------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 10;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "sband", "splan", "srsp", "srspiss", "conform", "uscan", "mdate", "mtime" };

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int SBAND = 2;
        public const int SPLAN = 3;
        public const int SRSP = 4;
        public const int SRSPISS = 5;
        public const int CONFORM = 6;
        public const int USCAN = 7;
        public const int MDATE = 8;
        public const int MTIME = 9;

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int SBAND_SZ = Constant.BNDCDE_SZ;
        public const int SPLAN_SZ = Constant.PLAN_SZ;
        public const int SRSP_SZ = Constant.SRSP_SZ;
        public const int SRSPISS_SZ = Constant.SRSPISS_SZ;
        public const int CONFORM_SZ = Constant.CONFORM_SZ;
        public const int USCAN_SZ = Constant.USCAN_SZ;
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

        public static string AllColumnsForSqlUpdateAsBindings
        {
            get { return mAllBindingsAsCSV; }
        }

        //------------------------------------------------------------------------

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static SuPlan()
        {
            mAllFieldsAsCSV = ListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        public SuPlan()
        {
            cmd = "";
            recstat = "";
            sband = "";
            splan = "";
            srsp = "";
            srspiss = "";
            conform = "";
            uscan = "";
            mdate = "";
            mtime = "";
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

            sb.Append("\r\ncmd =     " + cmd);
            sb.Append("\r\nrecstat = " + recstat);
            sb.Append("\r\nsband =   " + sband);
            sb.Append("\r\nsplan =   " + splan);
            sb.Append("\r\nsrsp =    " + srsp);
            sb.Append("\r\nsrspiss = " + srspiss);
            sb.Append("\r\nconform = " + conform);
            sb.Append("\r\nuscan =   " + uscan);
            sb.Append("\r\nmdate =   " + mdate);
            sb.Append("\r\nmtime =   " + mtime);

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
            sb.Append("\n ===== SuPlan ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "cmd = " + cmd);
            sb.Append("\n" + nullInds[i++] + "      " + "recstat = " + recstat);
            sb.Append("\n" + nullInds[i++] + "      " + "sband = " + sband);
            sb.Append("\n" + nullInds[i++] + "      " + "splan = " + splan);
            sb.Append("\n" + nullInds[i++] + "      " + "srsp = " + srsp);
            sb.Append("\n" + nullInds[i++] + "      " + "srspiss = " + srspiss);
            sb.Append("\n" + nullInds[i++] + "      " + "conform = " + conform);
            sb.Append("\n" + nullInds[i++] + "      " + "uscan = " + uscan);
            sb.Append("\n" + nullInds[i++] + "      " + "mdate = " + mdate);
            sb.Append("\n" + nullInds[i++] + "      " + "mtime = " + mtime);

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



    }
}

```
