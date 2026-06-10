# Documented File: SdTraf.cs
**Repository Path:** `_DataStructures\SdTraf.cs`
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
    /// This class has fields that are isomorphic with the SDF table <b>main.sd_traf</b>.
    /// </summary>
    /// <remarks>
    /// Antennas are mounted on towers, the type of tower is indicated in the antenna record 
    /// itself.  A note can be attached a note to a tower reference on an antenna.  These 
    /// tower notes refer to the tower notes table.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SdTraf
    {
        // IMPORTANT!
        // =========
        // The following qty. 7 member values correspond to the legacy native 
        // structure suTraf_; member names are prescribed to be identical.
        // The order of appearance of these qty. 7 field members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and oequt of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TRAFCODE_SZ)]
        public string trafcode;    //#0
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ECODE_SZ)]
        public string ecode;    //#1
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = XREFTRCDE_SZ)]
        public string xreftrcde;    //#2
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = XREFEQCDE_SZ)]
        public string xrefeqcde;    //#3
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TRDESC_SZ)]
        public string trdesc;    //#4
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;    //#5
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;    //#6

        //----------------------------------------------------------------

        //Additional public static members.

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 7;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "trafcode", "ecode", "xreftrcde", "xrefeqcde", "trdesc", "mdate", "mtime" };

        public const int TRAFCODE = 0;
        public const int ECODE = 1;
        public const int XREFTRCDE = 2;
        public const int XREFEQCDE = 3;
        public const int TRDESC = 4;
        public const int MDATE = 5;
        public const int MTIME = 6;

        public const int TRAFCODE_SZ = Constant.TRAFCODE_SZ;
        public const int ECODE_SZ = Constant.ECODE_SZ;
        public const int XREFTRCDE_SZ = Constant.TRAFCODE_SZ;
        public const int XREFEQCDE_SZ = Constant.ECODE_SZ;
        public const int TRDESC_SZ = Constant.TRDESC_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //----------------------------------------------------------------

        /// <summary>
        /// Per-instance default constructor.
        /// </summary>
        public SdTraf()
        {
            trafcode = "";
            ecode = "";
            xreftrcde = "";
            xrefeqcde = "";
            trdesc = "";
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
            sb.Append("\n ===== SuTraf ===== ");

            sb.Append("\r\ntrafcode =  " + trafcode);
            sb.Append("\r\necode =     " + ecode);
            sb.Append("\r\nxreftrcde = " + xreftrcde);
            sb.Append("\r\nxrefeqcde = " + xrefeqcde);
            sb.Append("\r\ntrdesc =    " + trdesc);
            sb.Append("\r\nmdate =     " + mdate);
            sb.Append("\r\nmtime =     " + mtime);

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
            sb.Append("\n ===== SdTraf ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "trafcode = " + trafcode);
            sb.Append("\n" + nullInds[i++] + "      " + "ecode = " + ecode);
            sb.Append("\n" + nullInds[i++] + "      " + "xreftrcde = " + xreftrcde);
            sb.Append("\n" + nullInds[i++] + "      " + "xrefeqcde = " + xrefeqcde);
            sb.Append("\n" + nullInds[i++] + "      " + "trdesc = " + trdesc);
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

            sb.Append(nullInds[SdTraf.TRAFCODE] == Constant.DB_NULL ? "NULL, " : "'" + trafcode.ToString() + "', ");
            sb.Append(nullInds[SdTraf.ECODE] == Constant.DB_NULL ? "NULL, " : "'" + ecode.ToString() + "', ");
            sb.Append(nullInds[SdTraf.XREFTRCDE] == Constant.DB_NULL ? "NULL, " : "'" + xreftrcde.ToString() + "', ");
            sb.Append(nullInds[SdTraf.XREFEQCDE] == Constant.DB_NULL ? "NULL, " : "'" + xrefeqcde.ToString() + "', ");
            sb.Append(nullInds[SdTraf.TRDESC] == Constant.DB_NULL ? "NULL, " : "'" + trdesc.ToString() + "', ");
            sb.Append(nullInds[SdTraf.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SdTraf.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

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

            sb.Append(columnNames[SdTraf.TRAFCODE] + " = " + (nullInds[SdTraf.TRAFCODE] == Constant.DB_NULL ? "NULL, " : "'" + trafcode.ToString() + "', "));
            sb.Append(columnNames[SdTraf.ECODE] + " = " + (nullInds[SdTraf.ECODE] == Constant.DB_NULL ? "NULL, " : "'" + ecode.ToString() + "', "));
            sb.Append(columnNames[SdTraf.XREFTRCDE] + " = " + (nullInds[SdTraf.XREFTRCDE] == Constant.DB_NULL ? "NULL, " : "'" + xreftrcde.ToString() + "', "));
            sb.Append(columnNames[SdTraf.XREFEQCDE] + " = " + (nullInds[SdTraf.XREFEQCDE] == Constant.DB_NULL ? "NULL, " : "'" + xrefeqcde.ToString() + "', "));
            sb.Append(columnNames[SdTraf.TRDESC] + " = " + (nullInds[SdTraf.TRDESC] == Constant.DB_NULL ? "NULL, " : "'" + trdesc.ToString() + "', "));
            sb.Append(columnNames[SdTraf.MDATE] + " = " + (nullInds[SdTraf.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SdTraf.MTIME] + " = " + (nullInds[SdTraf.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

            return sb.ToString();
        }

        /// <summary>
        /// This method returns instantiated SdTraf and SQLLEN[] nullInds objects whose
        /// members are set i.a.w. a prescribed SuTraf object and its associated nullInds; 
        /// SdTraf has identical members to SuTraf except for 'cmd' and 'recstat'.
        /// </summary>
        /// <param name="suTraf"></param>
        /// <param name="suTrafNullInds"></param>
        /// <param name="sdTraf"></param>
        /// <param name="sdTrafNullInds"></param>
        public static void MakeSdFromSu(SuTraf suTraf, SQLLEN[] suTrafNullInds, out SdTraf sdTraf, out SQLLEN[] sdTrafNullInds)
        {
            sdTraf = new SdTraf();
            sdTrafNullInds = new SQLLEN[NUM_COLUMNS];

            sdTraf.trafcode = suTraf.trafcode;
            sdTrafNullInds[SdTraf.TRAFCODE] = suTrafNullInds[SuTraf.TRAFCODE];

            sdTraf.ecode = suTraf.ecode;
            sdTrafNullInds[SdTraf.ECODE] = suTrafNullInds[SuTraf.ECODE];

            sdTraf.xreftrcde = suTraf.xreftrcde;
            sdTrafNullInds[SdTraf.XREFTRCDE] = suTrafNullInds[SuTraf.XREFTRCDE];

            sdTraf.xrefeqcde = suTraf.xrefeqcde;
            sdTrafNullInds[SdTraf.XREFEQCDE] = suTrafNullInds[SuTraf.XREFEQCDE];

            sdTraf.trdesc = suTraf.trdesc;
            sdTrafNullInds[SdTraf.TRDESC] = suTrafNullInds[SuTraf.TRDESC];

            sdTraf.mdate = suTraf.mdate;
            sdTrafNullInds[SdTraf.MDATE] = suTrafNullInds[SuTraf.MDATE];

            sdTraf.mtime = suTraf.mtime;
            sdTrafNullInds[SdTraf.MTIME] = suTrafNullInds[SuTraf.MTIME];

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

            tgtValPtrs[TRAFCODE] = Marshal.AllocHGlobal(TRAFCODE_SZ + 1);
            nullIndPtrs[TRAFCODE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TRAFCODE + 1, tgtValPtrs[TRAFCODE], SdTraf.TRAFCODE_SZ, nullIndPtrs[TRAFCODE]);

            tgtValPtrs[ECODE] = Marshal.AllocHGlobal(ECODE_SZ + 1);
            nullIndPtrs[ECODE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ECODE + 1, tgtValPtrs[ECODE], SdTraf.ECODE_SZ, nullIndPtrs[ECODE]);

            tgtValPtrs[XREFTRCDE] = Marshal.AllocHGlobal(XREFTRCDE_SZ + 1);
            nullIndPtrs[XREFTRCDE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, XREFTRCDE + 1, tgtValPtrs[XREFTRCDE], SdTraf.XREFTRCDE_SZ, nullIndPtrs[XREFTRCDE]);

            tgtValPtrs[XREFEQCDE] = Marshal.AllocHGlobal(XREFEQCDE_SZ + 1);
            nullIndPtrs[XREFEQCDE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, XREFEQCDE + 1, tgtValPtrs[XREFEQCDE], SdTraf.XREFEQCDE_SZ, nullIndPtrs[XREFEQCDE]);

            tgtValPtrs[TRDESC] = Marshal.AllocHGlobal(TRDESC_SZ + 1);
            nullIndPtrs[TRDESC] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TRDESC + 1, tgtValPtrs[TRDESC], SdTraf.TRDESC_SZ, nullIndPtrs[TRDESC]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], SdTraf.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], SdTraf.MTIME_SZ, nullIndPtrs[MTIME]);
        }



        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdEqpt object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="sdTraf"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SdTraf sdTraf, out SQLLEN[] nullInds)
        {
            sdTraf = new SdTraf();
            nullInds = NullHelper.CreateArrayOfNullInd(SdTraf.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[TRAFCODE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTraf.trafcode = "";
                nullInds[TRAFCODE] = Constant.DB_NULL;
            }
            else
            {
                sdTraf.trafcode = Marshal.PtrToStringAnsi(tgtValPtrs[TRAFCODE]).Trim();
                nullInds[TRAFCODE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ECODE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTraf.ecode = "";
                nullInds[ECODE] = Constant.DB_NULL;
            }
            else
            {
                sdTraf.ecode = Marshal.PtrToStringAnsi(tgtValPtrs[ECODE]).Trim();
                nullInds[ECODE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[XREFTRCDE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTraf.xreftrcde = "";
                nullInds[XREFTRCDE] = Constant.DB_NULL;
            }
            else
            {
                sdTraf.xreftrcde = Marshal.PtrToStringAnsi(tgtValPtrs[XREFTRCDE]).Trim();
                nullInds[XREFTRCDE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[XREFEQCDE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTraf.xrefeqcde = "";
                nullInds[XREFEQCDE] = Constant.DB_NULL;
            }
            else
            {
                sdTraf.xrefeqcde = Marshal.PtrToStringAnsi(tgtValPtrs[XREFEQCDE]).Trim();
                nullInds[XREFEQCDE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TRDESC]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTraf.trdesc = "";
                nullInds[TRDESC] = Constant.DB_NULL;
            }
            else
            {
                sdTraf.trdesc = Marshal.PtrToStringAnsi(tgtValPtrs[TRDESC]).Trim();
                nullInds[TRDESC] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTraf.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                sdTraf.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTraf.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                sdTraf.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

        }


        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[sd_traf](" +
    "[trafcode] [char](6) NOT NULL," +
    "[ecode] [char](8) NOT NULL," +
    "[xreftrcde] [char](6) NULL," +
    "[xrefeqcde] [char](8) NULL," +
    "[trdesc] [char](30) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
" CONSTRAINT [PK_sd_traf] PRIMARY KEY CLUSTERED " +
"(" +
    "[trafcode] ASC," +
    "[ecode] ASC" +
") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";


    }
}



```
