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
    /// This class has fields that are isomorphic with the SDF table <b>main.sd_towr</b>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SdTowr
    {
        // IMPORTANT!
        // =========
        // The following qty. 4 member values correspond to the legacy native 
        // structure suTowr_; member names are prescribed to be identical.
        // The order of appearance of these qty. 4 field members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and oequt of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TWCODE_SZ)]
        public string twcode;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TWDESC_SZ)]
        public string twdesc;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //----------------------------------------------------------------

        //Additional public static members.

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 4;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "twcode", "twdesc", "mdate", "mtime" };

        public const int TWCODE = 0;
        public const int TWDESC = 1;
        public const int MDATE = 2;
        public const int MTIME = 3;

        public const int TWCODE_SZ = Constant.TOWERCODE_SZ;
        public const int TWDESC_SZ = Constant.TWDESC_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //----------------------------------------------------------------

        /// <summary>
        /// Per-instance default constructor.
        /// </summary>
        public SdTowr()
        {
            twcode = "";
            twdesc = "";
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
            sb.Append("\n ===== SdTowr ===== ");

            sb.Append("\ntwcode = " + twcode);
            sb.Append("\ntwdesc = " + twdesc);
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
            sb.Append("\n ===== SdTowr ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "twcode = " + twcode);
            sb.Append("\n" + nullInds[i++] + "      " + "twdesc = " + twdesc);
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

            sb.Append(nullInds[SdTowr.TWCODE] == Constant.DB_NULL ? "NULL, " : "'" + twcode.ToString() + "', ");
            sb.Append(nullInds[SdTowr.TWDESC] == Constant.DB_NULL ? "NULL, " : "'" + twdesc.ToString() + "', ");
            sb.Append(nullInds[SdTowr.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SdTowr.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

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

            sb.Append(columnNames[SdTowr.TWCODE] + " = " + (nullInds[SdTowr.TWCODE] == Constant.DB_NULL ? "NULL, " : "'" + twcode.ToString() + "', "));
            sb.Append(columnNames[SdTowr.TWDESC] + " = " + (nullInds[SdTowr.TWDESC] == Constant.DB_NULL ? "NULL, " : "'" + twdesc.ToString() + "', "));
            sb.Append(columnNames[SdTowr.MDATE] + " = " + (nullInds[SdTowr.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SdTowr.MTIME] + " = " + (nullInds[SdTowr.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

            return sb.ToString();
        }

        /// <summary>
        /// This method returns instantiated SdTowr and SQLLEN[] nullInds objects whose
        /// members are set i.a.w. a prescribed SuTowr object and its associated nullInds; 
        /// SdTowr has identical members to SuTowr except for 'cmd' and 'recstat'.
        /// </summary>
        /// <param name="suTowr"></param>
        /// <param name="suTowrNullInds"></param>
        /// <param name="sdTowr"></param>
        /// <param name="sdTowrNullInds"></param>
        public static void MakeSdFromSu(SuTowr suTowr, SQLLEN[] suTowrNullInds, out SdTowr sdTowr, out SQLLEN[] sdTowrNullInds)
        {
            sdTowr = new SdTowr();
            sdTowrNullInds = new SQLLEN[NUM_COLUMNS];

            sdTowr.twcode = suTowr.twcode;
            sdTowrNullInds[SdTowr.TWCODE] = suTowrNullInds[SuTowr.TWCODE];

            sdTowr.twdesc = suTowr.twdesc;
            sdTowrNullInds[SdTowr.TWDESC] = suTowrNullInds[SuTowr.TWDESC];

            sdTowr.mdate = suTowr.mdate;
            sdTowrNullInds[SdTowr.MDATE] = suTowrNullInds[SuTowr.MDATE];

            sdTowr.mtime = suTowr.mtime;
            sdTowrNullInds[SdTowr.MTIME] = suTowrNullInds[SuTowr.MTIME];

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

            tgtValPtrs[TWCODE] = Marshal.AllocHGlobal(TWCODE_SZ + 1);
            nullIndPtrs[TWCODE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TWCODE + 1, tgtValPtrs[TWCODE], SdTowr.TWCODE_SZ, nullIndPtrs[TWCODE]);

            tgtValPtrs[TWDESC] = Marshal.AllocHGlobal(TWDESC_SZ + 1);
            nullIndPtrs[TWDESC] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TWDESC + 1, tgtValPtrs[TWDESC], SdTowr.TWDESC_SZ, nullIndPtrs[TWDESC]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], SdTowr.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], SdTowr.MTIME_SZ, nullIndPtrs[MTIME]);
        }



        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdEqpt object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="sdTowr"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SdTowr sdTowr, out SQLLEN[] nullInds)
        {
            sdTowr = new SdTowr();
            nullInds = NullHelper.CreateArrayOfNullInd(SdTowr.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[TWCODE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTowr.twcode = "";
                nullInds[TWCODE] = Constant.DB_NULL;
            }
            else
            {
                sdTowr.twcode = Marshal.PtrToStringAnsi(tgtValPtrs[TWCODE]).Trim();
                nullInds[TWCODE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TWDESC]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTowr.twdesc = "";
                nullInds[TWDESC] = Constant.DB_NULL;
            }
            else
            {
                sdTowr.twdesc = Marshal.PtrToStringAnsi(tgtValPtrs[TWDESC]).Trim();
                nullInds[TWDESC] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTowr.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                sdTowr.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTowr.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                sdTowr.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

        }


        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[sd_towr](" +
    "[twcode] [char](4) NOT NULL," +
    "[twdesc] [char](60) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
" CONSTRAINT [PK_sd_towr] PRIMARY KEY CLUSTERED " +
"(" +
    "[twcode] ASC" +
") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY]" +
") ON [PRIMARY]";


    }
}


