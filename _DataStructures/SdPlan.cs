using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using _NewLib;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using SQLHANDLE = IntPtr;

    /// <summary>
    /// This class encapsulates the dataset of a Plan and is isomorphic
    /// to the DB table <b>main.sd_plan</b> .
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SdPlan
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SBAND_SZ)]
        public string sband;    //#0
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SPLAN_SZ)]
        public string splan;    //#1
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SRSP_SZ)]
        public string srsp;    //#2
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SRSPISS_SZ)]
        public string srspiss;    //#3
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CONFORM_SZ)]
        public string conform;    //#4
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = USCAN_SZ)]
        public string uscan;    //#5
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;    //#6
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;    //#7

        //------------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 8;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "sband", "splan", "srsp", "srspiss", "conform", "uscan", "mdate", "mtime" };

        public const int SBAND = 0;
        public const int SPLAN = 1;
        public const int SRSP = 2;
        public const int SRSPISS = 3;
        public const int CONFORM = 4;
        public const int USCAN = 5;
        public const int MDATE = 6;
        public const int MTIME = 7;

        public const int SBAND_SZ = Constant.BNDCDE_SZ;
        public const int SPLAN_SZ = Constant.PLAN_SZ;
        public const int SRSP_SZ = Constant.SRSP_SZ;
        public const int SRSPISS_SZ = Constant.SRSPISS_SZ;
        public const int CONFORM_SZ = Constant.CONFORM_SZ;
        public const int USCAN_SZ = Constant.USCAN_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //-------------------------------------------------------------------------

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public SdPlan()
        {
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

            sb.Append("\nsband = " + sband);
            sb.Append("\nsplan = " + splan);
            sb.Append("\nsrsp = " + srsp);
            sb.Append("\nsrspiss = " + srspiss);
            sb.Append("\nconform = " + conform);
            sb.Append("\nuscan = " + uscan);
            sb.Append("\nmdate = " + mdate);
            sb.Append("\nmtime = " + mtime);

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
            sb.Append("\n ===== SdPlan ===== ");

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
        /// This method returns the member values of an object formatted
        /// as a CSV string that can be used to provide the VALUES in an SQL INSERT query.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringAsCSV(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[SdPlan.SBAND] == Constant.DB_NULL ? "NULL, " : "'" + sband.ToString() + "', ");
            sb.Append(nullInds[SdPlan.SPLAN] == Constant.DB_NULL ? "NULL, " : "'" + splan.ToString() + "', ");
            sb.Append(nullInds[SdPlan.SRSP] == Constant.DB_NULL ? "NULL, " : "'" + srsp.ToString() + "', ");
            sb.Append(nullInds[SdPlan.SRSPISS] == Constant.DB_NULL ? "NULL, " : "'" + srspiss.ToString() + "', ");
            sb.Append(nullInds[SdPlan.CONFORM] == Constant.DB_NULL ? "NULL, " : "'" + conform.ToString() + "', ");
            sb.Append(nullInds[SdPlan.USCAN] == Constant.DB_NULL ? "NULL, " : "'" + uscan.ToString() + "', ");
            sb.Append(nullInds[SdPlan.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SdPlan.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

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

            sb.Append(columnNames[SdPlan.SBAND] + " = " + (nullInds[SdPlan.SBAND] == Constant.DB_NULL ? "NULL, " : "'" + sband.ToString() + "', "));
            sb.Append(columnNames[SdPlan.SPLAN] + " = " + (nullInds[SdPlan.SPLAN] == Constant.DB_NULL ? "NULL, " : "'" + splan.ToString() + "', "));
            sb.Append(columnNames[SdPlan.SRSP] + " = " + (nullInds[SdPlan.SRSP] == Constant.DB_NULL ? "NULL, " : "'" + srsp.ToString() + "', "));
            sb.Append(columnNames[SdPlan.SRSPISS] + " = " + (nullInds[SdPlan.SRSPISS] == Constant.DB_NULL ? "NULL, " : "'" + srspiss.ToString() + "', "));
            sb.Append(columnNames[SdPlan.CONFORM] + " = " + (nullInds[SdPlan.CONFORM] == Constant.DB_NULL ? "NULL, " : "'" + conform.ToString() + "', "));
            sb.Append(columnNames[SdPlan.USCAN] + " = " + (nullInds[SdPlan.USCAN] == Constant.DB_NULL ? "NULL, " : "'" + uscan.ToString() + "', "));
            sb.Append(columnNames[SdPlan.MDATE] + " = " + (nullInds[SdPlan.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SdPlan.MTIME] + " = " + (nullInds[SdPlan.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

            return sb.ToString();
        }

        /// <summary>
        /// This method returns instantiated SdPlan and SQLLEN[] nullInds objects whose
        /// members are set i.a.w. a prescribed SuPlan object and its associated nullInds; 
        /// SdPlan has identical members to SuPlan except for 'cmd' and 'recstat'.
        /// </summary>
        /// <param name="suPlan"></param>
        /// <param name="suPlanNullInds"></param>
        /// <param name="sdPlan"></param>
        /// <param name="sdPlanNullInds"></param>
        public static void MakeSdFromSu(SuPlan suPlan, SQLLEN[] suPlanNullInds, out SdPlan sdPlan, out SQLLEN[] sdPlanNullInds)
        {
            sdPlan = new SdPlan();
            sdPlanNullInds = new SQLLEN[NUM_COLUMNS];

            sdPlan.sband = suPlan.sband;
            sdPlanNullInds[SdPlan.SBAND] = suPlanNullInds[SuPlan.SBAND];

            sdPlan.splan = suPlan.splan;
            sdPlanNullInds[SdPlan.SPLAN] = suPlanNullInds[SuPlan.SPLAN];

            sdPlan.srsp = suPlan.srsp;
            sdPlanNullInds[SdPlan.SRSP] = suPlanNullInds[SuPlan.SRSP];

            sdPlan.srspiss = suPlan.srspiss;
            sdPlanNullInds[SdPlan.SRSPISS] = suPlanNullInds[SuPlan.SRSPISS];

            sdPlan.conform = suPlan.conform;
            sdPlanNullInds[SdPlan.CONFORM] = suPlanNullInds[SuPlan.CONFORM];

            sdPlan.uscan = suPlan.uscan;
            sdPlanNullInds[SdPlan.USCAN] = suPlanNullInds[SuPlan.USCAN];

            sdPlan.mdate = suPlan.mdate;
            sdPlanNullInds[SdPlan.MDATE] = suPlanNullInds[SuPlan.MDATE];

            sdPlan.mtime = suPlan.mtime;
            sdPlanNullInds[SdPlan.MTIME] = suPlanNullInds[SuPlan.MTIME];
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

            tgtValPtrs[SBAND] = Marshal.AllocHGlobal(SBAND_SZ + 1);
            nullIndPtrs[SBAND] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SBAND + 1, tgtValPtrs[SBAND], SdPlan.SBAND_SZ, nullIndPtrs[SBAND]);

            tgtValPtrs[SPLAN] = Marshal.AllocHGlobal(SPLAN_SZ + 1);
            nullIndPtrs[SPLAN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SPLAN + 1, tgtValPtrs[SPLAN], SdPlan.SPLAN_SZ, nullIndPtrs[SPLAN]);

            tgtValPtrs[SRSP] = Marshal.AllocHGlobal(SRSP_SZ + 1);
            nullIndPtrs[SRSP] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SRSP + 1, tgtValPtrs[SRSP], SdPlan.SRSP_SZ, nullIndPtrs[SRSP]);

            tgtValPtrs[SRSPISS] = Marshal.AllocHGlobal(SRSPISS_SZ + 1);
            nullIndPtrs[SRSPISS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SRSPISS + 1, tgtValPtrs[SRSPISS], SdPlan.SRSPISS_SZ, nullIndPtrs[SRSPISS]);

            tgtValPtrs[CONFORM] = Marshal.AllocHGlobal(CONFORM_SZ + 1);
            nullIndPtrs[CONFORM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CONFORM + 1, tgtValPtrs[CONFORM], SdPlan.CONFORM_SZ, nullIndPtrs[CONFORM]);

            tgtValPtrs[USCAN] = Marshal.AllocHGlobal(USCAN_SZ + 1);
            nullIndPtrs[USCAN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, USCAN + 1, tgtValPtrs[USCAN], SdPlan.USCAN_SZ, nullIndPtrs[USCAN]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], SdPlan.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], SdPlan.MTIME_SZ, nullIndPtrs[MTIME]);
        }


        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdOper object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="sdPlan"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SdPlan sdPlan, out SQLLEN[] nullInds)
        {
            sdPlan = new SdPlan();
            nullInds = NullHelper.CreateArrayOfNullInd(SdPlan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[SBAND]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlan.sband = "";
                nullInds[SBAND] = Constant.DB_NULL;
            }
            else
            {
                sdPlan.sband = Marshal.PtrToStringAnsi(tgtValPtrs[SBAND]).Trim();
                nullInds[SBAND] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SPLAN]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlan.splan = "";
                nullInds[SPLAN] = Constant.DB_NULL;
            }
            else
            {
                sdPlan.splan = Marshal.PtrToStringAnsi(tgtValPtrs[SPLAN]).Trim();
                nullInds[SPLAN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SRSP]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlan.srsp = "";
                nullInds[SRSP] = Constant.DB_NULL;
            }
            else
            {
                sdPlan.srsp = Marshal.PtrToStringAnsi(tgtValPtrs[SRSP]).Trim();
                nullInds[SRSP] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SRSPISS]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlan.srspiss = "";
                nullInds[SRSPISS] = Constant.DB_NULL;
            }
            else
            {
                sdPlan.srspiss = Marshal.PtrToStringAnsi(tgtValPtrs[SRSPISS]).Trim();
                nullInds[SRSPISS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CONFORM]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlan.conform = "";
                nullInds[CONFORM] = Constant.DB_NULL;
            }
            else
            {
                sdPlan.conform = Marshal.PtrToStringAnsi(tgtValPtrs[CONFORM]).Trim();
                nullInds[CONFORM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[USCAN]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlan.uscan = "";
                nullInds[USCAN] = Constant.DB_NULL;
            }
            else
            {
                sdPlan.uscan = Marshal.PtrToStringAnsi(tgtValPtrs[USCAN]).Trim();
                nullInds[USCAN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlan.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                sdPlan.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlan.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                sdPlan.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[sd_plan](" +
    "[sband] [char](4) NOT NULL," +
    "[splan] [char](4) NOT NULL," +
    "[srsp] [char](10) NULL," +
    "[srspiss] [char](2) NULL," +
    "[conform] [char](1) NULL," +
    "[uscan] [char](1) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
" CONSTRAINT [PK_sd_plan] PRIMARY KEY CLUSTERED " +
"(" +
    "[sband] ASC," +
    "[splan] ASC" +
") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";




        /*
        sband
        splan
        srsp
        srspiss
        conform
        uscan
        mdate
        mtime
        */



    }
}
