# Documented File: SuTown.cs
**Repository Path:** `_DataStructures\SuTown.cs`
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
    /// This class has fields that are isomorphic with the 'subupt' table <b>main.sd_town</b> 
    /// except for the additional fields 'cmd' and 'recstat'.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SuTown
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CMD_SZ)]
        public string cmd;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RECSTAT_SZ)]
        public string recstat;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALL1_SZ)]
        public string call1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = OPER_SZ)]
        public string oper;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TWCODE_SZ)]
        public string twcode;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float twht;
        [MarshalAsAttribute(UnmanagedType.U1)]
        public byte atwrno;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TWLI_SZ)]
        public string twli;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TWPA_SZ)]
        public string twpa;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTT_SZ)]
        public string nott;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TPOINT_SZ)]
        public string tpoint;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ADATE_SZ)]
        public string adate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SDATE_SZ)]
        public string sdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //--------------------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 15;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "call1", "oper", "twcode", "twht", "atwrno", "twli", "twpa", "nott", "tpoint", "adate", "sdate", "mdate", "mtime" };

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int CALL1 = 2;
        public const int OPER = 3;
        public const int TWCODE = 4;
        public const int TWHT = 5;
        public const int ATWRNO = 6;
        public const int TWLI = 7;
        public const int TWPA = 8;
        public const int NOTT = 9;
        public const int TPOINT = 10;
        public const int ADATE = 11;
        public const int SDATE = 12;
        public const int MDATE = 13;
        public const int MTIME = 14;

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int CALL1_SZ = Constant.CALLSIGN_SZ;
        public const int OPER_SZ = Constant.OPERCODE_SZ;
        public const int TWCODE_SZ = Constant.SUTOWN_TWCODE_SZ;
        public const int TWLI_SZ = Constant.SUTOWN_TWLI_SZ;
        public const int TWPA_SZ = Constant.SUTOWN_TWPA_SZ;
        public const int NOTT_SZ = Constant.SUTOWN_NOTT_SZ;
        public const int TPOINT_SZ = Constant.SUTOWN_TPOINT_SZ;
        public const int ADATE_SZ = Constant.DATE_SZ;
        public const int SDATE_SZ = Constant.DATE_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //---------------------------------------------------------------------------------

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

        //---------------------------------------------------------------------------------

        // <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static SuTown()
        {
            mAllFieldsAsCSV = ListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        public SuTown()
        {
            cmd = "";
            recstat = "";
            call1 = "";
            oper = "";
            twcode = "";
            twli = "";
            twpa = "";
            nott = "";
            tpoint = "";
            adate = "";
            sdate = "";
            mdate = "";
            mtime = "";
        }

        //---------------------------------------------------------------------------------



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
            sb.AppendLine("cmd =     " + cmd);
            sb.AppendLine("recstat = " + recstat);
            sb.AppendLine("call1 =   " + call1);
            sb.AppendLine("oper =    " + oper);
            sb.AppendLine("twcode =  " + twcode);
            sb.AppendLine("twht =    " + twht);
            sb.AppendLine("atwrno =  " + atwrno);
            sb.AppendLine("twli =    " + twli);
            sb.AppendLine("twpa =    " + twpa);
            sb.AppendLine("nott =    " + nott);
            sb.AppendLine("tpoint =  " + tpoint);
            sb.AppendLine("adate =   " + adate);
            sb.AppendLine("sdate =   " + sdate);
            sb.AppendLine("mdate =   " + mdate);
            sb.AppendLine("mtime =   " + mtime);
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
            sb.Append("\n ===== SuTown ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "cmd = " + cmd);
            sb.Append("\n" + nullInds[i++] + "      " + "recstat = " + recstat);
            sb.Append("\n" + nullInds[i++] + "      " + "call1 = " + call1);
            sb.Append("\n" + nullInds[i++] + "      " + "oper = " + oper);
            sb.Append("\n" + nullInds[i++] + "      " + "twcode = " + twcode);
            sb.Append("\n" + nullInds[i++] + "      " + "twht = " + twht);
            sb.Append("\n" + nullInds[i++] + "      " + "atwrno = " + atwrno);
            sb.Append("\n" + nullInds[i++] + "      " + "twli = " + twli);
            sb.Append("\n" + nullInds[i++] + "      " + "twpa = " + twpa);
            sb.Append("\n" + nullInds[i++] + "      " + "nott = " + nott);
            sb.Append("\n" + nullInds[i++] + "      " + "tpoint = " + tpoint);
            sb.Append("\n" + nullInds[i++] + "      " + "adate = " + adate);
            sb.Append("\n" + nullInds[i++] + "      " + "sdate = " + sdate);
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


        //
        // BEWARE: this is the SQL query to create a SDF main_sd_town table,
        //         NOT an su_town table.
        //
        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[sd_town](" +
    "[call1] [char](9) NOT NULL," +
    "[oper] [char](6) NULL," +
    "[twcode] [char](4) NULL," +
    "[twht] [real] NULL," +
    "[atwrno] [tinyint] NOT NULL," +
    "[twli] [char](1) NULL," +
    "[twpa] [char](1) NULL," +
    "[nott] [char](4) NULL," +
    "[tpoint] [char](4) NULL," +
    "[adate] [char](10) NULL," +
    "[sdate] [char](10) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
"CONSTRAINT [PK_sd_town] PRIMARY KEY CLUSTERED " +
"(" +
    "[call1] ASC," +
    "[atwrno] ASC" +
")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";






    }
}

```
