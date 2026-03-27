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
    /// This class has fields that are isomorphic with the 'subupt' table <b>main.sd_rout</b>
    /// except for the additional fields 'cmd' and 'recstat'.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SuRout
    {
        // IMPORTANT!
        // =========
        // The following qty. 9 member values correspond to the legacy native 
        // structure suRout_; member names are prescribed to be identical.
        // The order of appearance of these qty. 9 field members MUST be
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
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RCOMP_SZ)]
        public string rcomp;    //#2
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ROUTNUMB_SZ)]
        public string routnumb;    //#3
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RTPROV_SZ)]
        public string rtprov;    //#4
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RTCALL_SZ)]
        public string rtcall;    //#5
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RTNAME_SZ)]
        public string rtname;    //#6
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;    //#7
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;    //#8

        //----------------------------------------------------------------
        //Additional public static members.

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 9;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "rcomp", "routnumb", "rtprov", "rtcall", "rtname", "mdate", "mtime" };

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int RCOMP = 2;
        public const int ROUTNUMB = 3;
        public const int RTPROV = 4;
        public const int RTCALL = 5;
        public const int RTNAME = 6;
        public const int MDATE = 7;
        public const int MTIME = 8;

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int RCOMP_SZ = Constant.OPERCODE_SZ;
        public const int ROUTNUMB_SZ = Constant.ROUTE_SZ;
        public const int RTPROV_SZ = Constant.PROV_SZ;
        public const int RTCALL_SZ = Constant.CALLSIGN_SZ;
        public const int RTNAME_SZ = Constant.RTNAME_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //----------------------------------------------------------------

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

        //-------------------------------------------------------------------------------

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static SuRout()
        {
            mAllFieldsAsCSV = ListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        public SuRout()
        {
            cmd = "";
            recstat = "";
            rcomp = "";
            routnumb = "";
            rtprov = "";
            rtcall = "";
            rtname = "";
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

            sb.Append("\r\ncmd =      " + cmd);
            sb.Append("\r\nrecstat =  " + recstat);
            sb.Append("\r\nrcomp =    " + rcomp);
            sb.Append("\r\nroutnumb = " + routnumb);
            sb.Append("\r\nrtprov =   " + rtprov);
            sb.Append("\r\nrtcall =   " + rtcall);
            sb.Append("\r\nrtname =   " + rtname);
            sb.Append("\r\nmdate =    " + mdate);
            sb.Append("\r\nmtime =    " + mtime);

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
            sb.Append("\n ===== SuRout ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "cmd = " + cmd);
            sb.Append("\n" + nullInds[i++] + "      " + "recstat = " + recstat);
            sb.Append("\n" + nullInds[i++] + "      " + "rcomp = " + rcomp);
            sb.Append("\n" + nullInds[i++] + "      " + "routnumb = " + routnumb);
            sb.Append("\n" + nullInds[i++] + "      " + "rtprov = " + rtprov);
            sb.Append("\n" + nullInds[i++] + "      " + "rtcall = " + rtcall);
            sb.Append("\n" + nullInds[i++] + "      " + "rtname = " + rtname);
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

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[sd_rout](" +
    "[rcomp] [char](6) NOT NULL," +
    "[routnumb] [char](8) NOT NULL," +
    "[rtprov] [char](2) NULL," +
    "[rtcall] [char](9) NULL," +
    "[rtname] [char](48) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
 "CONSTRAINT [PK_sd_rout] PRIMARY KEY CLUSTERED " +
"(" +
    "[rcomp] ASC," +
    "[routnumb] ASC" +
")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";


    }
}
