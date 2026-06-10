# Documented File: SdRout.cs
**Repository Path:** `_DataStructures\SdRout.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using _Configuration;
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
    /// This class has fields that are isomorphic with the SDF table <b>main.sd_rout</b>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SdRout
    {
        // IMPORTANT!
        // =========
        // The following qty. 7 member values correspond to the legacy native 
        // structure suRout_; member names are prescribed to be identical.
        // The order of appearance of these qty. 7 field members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and oequt of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RCOMP_SZ)]
        public string rcomp;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ROUTNUMB_SZ)]
        public string routnumb;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RTPROV_SZ)]
        public string rtprov;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RTCALL_SZ)]
        public string rtcall;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RTNAME_SZ)]
        public string rtname;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //----------------------------------------------------------------

        //Additional public static members.

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 7;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "rcomp", "routnumb", "rtprov", "rtcall", "rtname", "mdate", "mtime" };

        public const int RCOMP = 0;
        public const int ROUTNUMB = 1;
        public const int RTPROV = 2;
        public const int RTCALL = 3;
        public const int RTNAME = 4;
        public const int MDATE = 5;
        public const int MTIME = 6;

        public const int RCOMP_SZ = Constant.OPERCODE_SZ;
        public const int ROUTNUMB_SZ = Constant.ROUTE_SZ;
        public const int RTPROV_SZ = Constant.PROV_SZ;
        public const int RTCALL_SZ = Constant.CALLSIGN_SZ;
        public const int RTNAME_SZ = Constant.RTNAME_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //----------------------------------------------------------------

        /// <summary>
        /// Per-instance default constructor.
        /// </summary>
        public SdRout()
        {
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
            sb.Append("\n ===== SdRout ===== ");

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
        /// This method returns the member values of an object formatted
        /// as a CSV string that can be used to provide the VALUES in an SQL INSERT query.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringAsCSV(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[SdRout.RCOMP] == Constant.DB_NULL ? "NULL, " : "'" + rcomp.ToString() + "', ");
            sb.Append(nullInds[SdRout.ROUTNUMB] == Constant.DB_NULL ? "NULL, " : "'" + routnumb.ToString() + "', ");
            sb.Append(nullInds[SdRout.RTPROV] == Constant.DB_NULL ? "NULL, " : "'" + rtprov.ToString() + "', ");
            sb.Append(nullInds[SdRout.RTCALL] == Constant.DB_NULL ? "NULL, " : "'" + rtcall.ToString() + "', ");
            sb.Append(nullInds[SdRout.RTNAME] == Constant.DB_NULL ? "NULL, " : "'" + rtname.ToString() + "', ");
            sb.Append(nullInds[SdRout.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SdRout.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

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

            sb.Append(columnNames[SdRout.RCOMP] + " = " + (nullInds[SdRout.RCOMP] == Constant.DB_NULL ? "NULL, " : "'" + rcomp.ToString() + "', "));
            sb.Append(columnNames[SdRout.ROUTNUMB] + " = " + (nullInds[SdRout.ROUTNUMB] == Constant.DB_NULL ? "NULL, " : "'" + routnumb.ToString() + "', "));
            sb.Append(columnNames[SdRout.RTPROV] + " = " + (nullInds[SdRout.RTPROV] == Constant.DB_NULL ? "NULL, " : "'" + rtprov.ToString() + "', "));
            sb.Append(columnNames[SdRout.RTCALL] + " = " + (nullInds[SdRout.RTCALL] == Constant.DB_NULL ? "NULL, " : "'" + rtcall.ToString() + "', "));
            sb.Append(columnNames[SdRout.RTNAME] + " = " + (nullInds[SdRout.RTNAME] == Constant.DB_NULL ? "NULL, " : "'" + rtname.ToString() + "', "));
            sb.Append(columnNames[SdRout.MDATE] + " = " + (nullInds[SdRout.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SdRout.MTIME] + " = " + (nullInds[SdRout.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

            return sb.ToString();
        }

        /// <summary>
        /// This method returns instantiated SdRout and SQLLEN[] nullInds objects whose
        /// members are set i.a.w. a prescribed SuRout object and its associated nullInds; 
        /// SdRout has identical members to SuRout except for 'cmd' and 'recstat'.
        /// </summary>
        /// <param name="suRout"></param>
        /// <param name="suRoutNullInds"></param>
        /// <param name="sdRout"></param>
        /// <param name="sdRoutNullInds"></param>
        public static void MakeSdFromSu(SuRout suRout, SQLLEN[] suRoutNullInds, out SdRout sdRout, out SQLLEN[] sdRoutNullInds)
        {
            sdRout = new SdRout();
            sdRoutNullInds = new SQLLEN[NUM_COLUMNS];

            sdRout.rcomp = suRout.rcomp;
            sdRoutNullInds[SdRout.RCOMP] = suRoutNullInds[SuRout.RCOMP];

            sdRout.routnumb = suRout.routnumb;
            sdRoutNullInds[SdRout.ROUTNUMB] = suRoutNullInds[SuRout.ROUTNUMB];

            sdRout.rtprov = suRout.rtprov;
            sdRoutNullInds[SdRout.RTPROV] = suRoutNullInds[SuRout.RTPROV];

            sdRout.rtcall = suRout.rtcall;
            sdRoutNullInds[SdRout.RTCALL] = suRoutNullInds[SuRout.RTCALL];

            sdRout.rtname = suRout.rtname;
            sdRoutNullInds[SdRout.RTNAME] = suRoutNullInds[SuRout.RTNAME];

            sdRout.mdate = suRout.mdate;
            sdRoutNullInds[SdRout.MDATE] = suRoutNullInds[SuRout.MDATE];

            sdRout.mtime = suRout.mtime;
            sdRoutNullInds[SdRout.MTIME] = suRoutNullInds[SuRout.MTIME];

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

            tgtValPtrs[RCOMP] = Marshal.AllocHGlobal(RCOMP_SZ + 1);
            nullIndPtrs[RCOMP] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, RCOMP + 1, tgtValPtrs[RCOMP], SdRout.RCOMP_SZ, nullIndPtrs[RCOMP]);

            tgtValPtrs[ROUTNUMB] = Marshal.AllocHGlobal(ROUTNUMB_SZ + 1);
            nullIndPtrs[ROUTNUMB] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ROUTNUMB + 1, tgtValPtrs[ROUTNUMB], SdRout.ROUTNUMB_SZ, nullIndPtrs[ROUTNUMB]);

            tgtValPtrs[RTPROV] = Marshal.AllocHGlobal(RTPROV_SZ + 1);
            nullIndPtrs[RTPROV] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, RTPROV + 1, tgtValPtrs[RTPROV], SdRout.RTPROV_SZ, nullIndPtrs[RTPROV]);

            tgtValPtrs[RTCALL] = Marshal.AllocHGlobal(RTCALL_SZ + 1);
            nullIndPtrs[RTCALL] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, RTCALL + 1, tgtValPtrs[RTCALL], SdRout.RTCALL_SZ, nullIndPtrs[RTCALL]);

            tgtValPtrs[RTNAME] = Marshal.AllocHGlobal(RTNAME_SZ + 1);
            nullIndPtrs[RTNAME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, RTNAME + 1, tgtValPtrs[RTNAME], SdRout.RTNAME_SZ, nullIndPtrs[RTNAME]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], SdRout.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], SdRout.MTIME_SZ, nullIndPtrs[MTIME]);
        }


        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdEqpt object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="sdRout"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SdRout sdRout, out SQLLEN[] nullInds)
        {
            sdRout = new SdRout();
            nullInds = NullHelper.CreateArrayOfNullInd(SdRout.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[RCOMP]);
            if (nullInd == Constant.DB_NULL)
            {
                sdRout.rcomp = "";
                nullInds[RCOMP] = Constant.DB_NULL;
            }
            else
            {
                sdRout.rcomp = Marshal.PtrToStringAnsi(tgtValPtrs[RCOMP]).Trim();
                nullInds[RCOMP] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ROUTNUMB]);
            if (nullInd == Constant.DB_NULL)
            {
                sdRout.routnumb = "";
                nullInds[ROUTNUMB] = Constant.DB_NULL;
            }
            else
            {
                sdRout.routnumb = Marshal.PtrToStringAnsi(tgtValPtrs[ROUTNUMB]).Trim();
                nullInds[ROUTNUMB] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RTPROV]);
            if (nullInd == Constant.DB_NULL)
            {
                sdRout.rtprov = "";
                nullInds[RTPROV] = Constant.DB_NULL;
            }
            else
            {
                sdRout.rtprov = Marshal.PtrToStringAnsi(tgtValPtrs[RTPROV]).Trim();
                nullInds[RTPROV] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RTCALL]);
            if (nullInd == Constant.DB_NULL)
            {
                sdRout.rtcall = "";
                nullInds[RTCALL] = Constant.DB_NULL;
            }
            else
            {
                sdRout.rtcall = Marshal.PtrToStringAnsi(tgtValPtrs[RTCALL]).Trim();
                nullInds[RTCALL] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RTNAME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdRout.rtname = "";
                nullInds[RTNAME] = Constant.DB_NULL;
            }
            else
            {
                sdRout.rtname = Marshal.PtrToStringAnsi(tgtValPtrs[RTNAME]).Trim();
                nullInds[RTNAME] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdRout.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                sdRout.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdRout.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                sdRout.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

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
" CONSTRAINT [PK_sd_rout] PRIMARY KEY CLUSTERED " +
"(" +
    "[rcomp] ASC," +
    "[routnumb] ASC" +
") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";


    }
}

```
