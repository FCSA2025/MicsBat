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
    /// This class has fields that are isomorphic with the SDF table <b>main.sd_town</b>.
    /// </summary>
    /// <remarks>
    /// Antennas are mounted on towers, the type of tower is indicated in the antenna record 
    /// itself.  A note can be attached a note to a tower reference on an antenna.  These 
    /// tower notes refer to the tower notes table.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SdTown
    {
        // IMPORTANT!
        // =========
        // The following qty. 13 member values correspond to the legacy native 
        // structure suTown_; member names are prescribed to be identical.
        // The order of appearance of these qty. 13 field members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and oequt of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

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

        //----------------------------------------------------------------

        //Additional public static members.

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 13;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "call1", "oper", "twcode", "twht", "atwrno", "twli", "twpa", "nott", "tpoint", "adate", "sdate", "mdate", "mtime" };

        public const int CALL1 = 0;
        public const int OPER = 1;
        public const int TWCODE = 2;
        public const int TWHT = 3;
        public const int ATWRNO = 4;
        public const int TWLI = 5;
        public const int TWPA = 6;
        public const int NOTT = 7;
        public const int TPOINT = 8;
        public const int ADATE = 9;
        public const int SDATE = 10;
        public const int MDATE = 11;
        public const int MTIME = 12;

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

        //----------------------------------------------------------------

        /// <summary>
        /// Per-instance default constructor.
        /// </summary>
        public SdTown()
        {
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
            sb.Append("\n ===== SdTown ===== ");

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
        /// This method returns the member values of an object formatted
        /// as a CSV string that can be used to provide the VALUES in an SQL INSERT query.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringAsCSV(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[SdTown.CALL1] == Constant.DB_NULL ? "NULL, " : "'" + call1.ToString() + "', ");
            sb.Append(nullInds[SdTown.OPER] == Constant.DB_NULL ? "NULL, " : "'" + oper.ToString() + "', ");
            sb.Append(nullInds[SdTown.TWCODE] == Constant.DB_NULL ? "NULL, " : "'" + twcode.ToString() + "', ");
            sb.Append(nullInds[SdTown.TWHT] == Constant.DB_NULL ? "NULL, " : "'" + twht.ToString() + "', ");
            sb.Append(nullInds[SdTown.ATWRNO] == Constant.DB_NULL ? "NULL, " : "'" + atwrno.ToString() + "', ");
            sb.Append(nullInds[SdTown.TWLI] == Constant.DB_NULL ? "NULL, " : "'" + twli.ToString() + "', ");
            sb.Append(nullInds[SdTown.TWPA] == Constant.DB_NULL ? "NULL, " : "'" + twpa.ToString() + "', ");
            sb.Append(nullInds[SdTown.NOTT] == Constant.DB_NULL ? "NULL, " : "'" + nott.ToString() + "', ");
            sb.Append(nullInds[SdTown.TPOINT] == Constant.DB_NULL ? "NULL, " : "'" + tpoint.ToString() + "', ");
            sb.Append(nullInds[SdTown.ADATE] == Constant.DB_NULL ? "NULL, " : "'" + adate.ToString() + "', ");
            sb.Append(nullInds[SdTown.SDATE] == Constant.DB_NULL ? "NULL, " : "'" + sdate.ToString() + "', ");
            sb.Append(nullInds[SdTown.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SdTown.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

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

            sb.Append(columnNames[SdTown.CALL1] + " = " + (nullInds[SdTown.CALL1] == Constant.DB_NULL ? "NULL, " : "'" + call1.ToString() + "', "));
            sb.Append(columnNames[SdTown.OPER] + " = " + (nullInds[SdTown.OPER] == Constant.DB_NULL ? "NULL, " : "'" + oper.ToString() + "', "));
            sb.Append(columnNames[SdTown.TWCODE] + " = " + (nullInds[SdTown.TWCODE] == Constant.DB_NULL ? "NULL, " : "'" + twcode.ToString() + "', "));
            sb.Append(columnNames[SdTown.TWHT] + " = " + (nullInds[SdTown.TWHT] == Constant.DB_NULL ? "NULL, " : "'" + twht.ToString() + "', "));
            sb.Append(columnNames[SdTown.ATWRNO] + " = " + (nullInds[SdTown.ATWRNO] == Constant.DB_NULL ? "NULL, " : "'" + atwrno.ToString() + "', "));
            sb.Append(columnNames[SdTown.TWLI] + " = " + (nullInds[SdTown.TWLI] == Constant.DB_NULL ? "NULL, " : "'" + twli.ToString() + "', "));
            sb.Append(columnNames[SdTown.TWPA] + " = " + (nullInds[SdTown.TWPA] == Constant.DB_NULL ? "NULL, " : "'" + twpa.ToString() + "', "));
            sb.Append(columnNames[SdTown.NOTT] + " = " + (nullInds[SdTown.NOTT] == Constant.DB_NULL ? "NULL, " : "'" + nott.ToString() + "', "));
            sb.Append(columnNames[SdTown.TPOINT] + " = " + (nullInds[SdTown.TPOINT] == Constant.DB_NULL ? "NULL, " : "'" + tpoint.ToString() + "', "));
            sb.Append(columnNames[SdTown.ADATE] + " = " + (nullInds[SdTown.ADATE] == Constant.DB_NULL ? "NULL, " : "'" + adate.ToString() + "', "));
            sb.Append(columnNames[SdTown.SDATE] + " = " + (nullInds[SdTown.SDATE] == Constant.DB_NULL ? "NULL, " : "'" + sdate.ToString() + "', "));
            sb.Append(columnNames[SdTown.MDATE] + " = " + (nullInds[SdTown.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SdTown.MTIME] + " = " + (nullInds[SdTown.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

            return sb.ToString();
        }

        /// <summary>
        /// This method returns instantiated SdTown and SQLLEN[] nullInds objects whose
        /// members are set i.a.w. a prescribed SuTown object and its associated nullInds; 
        /// SdTown has identical members to SuTown except for 'cmd' and 'recstat'.
        /// </summary>
        /// <param name="suTown"></param>
        /// <param name="suTownNullInds"></param>
        /// <param name="sdTown"></param>
        /// <param name="sdTownNullInds"></param>
        public static void MakeSdFromSu(SuTown suTown, SQLLEN[] suTownNullInds, out SdTown sdTown, out SQLLEN[] sdTownNullInds)
        {
            sdTown = new SdTown();
            sdTownNullInds = new SQLLEN[NUM_COLUMNS];

            sdTown.call1 = suTown.call1;
            sdTownNullInds[SdTown.CALL1] = suTownNullInds[SuTown.CALL1];

            sdTown.oper = suTown.oper;
            sdTownNullInds[SdTown.OPER] = suTownNullInds[SuTown.OPER];

            sdTown.twcode = suTown.twcode;
            sdTownNullInds[SdTown.TWCODE] = suTownNullInds[SuTown.TWCODE];

            sdTown.twht = suTown.twht;
            sdTownNullInds[SdTown.TWHT] = suTownNullInds[SuTown.TWHT];

            sdTown.atwrno = suTown.atwrno;
            sdTownNullInds[SdTown.ATWRNO] = suTownNullInds[SuTown.ATWRNO];

            sdTown.twli = suTown.twli;
            sdTownNullInds[SdTown.TWLI] = suTownNullInds[SuTown.TWLI];

            sdTown.twpa = suTown.twpa;
            sdTownNullInds[SdTown.TWPA] = suTownNullInds[SuTown.TWPA];

            sdTown.nott = suTown.nott;
            sdTownNullInds[SdTown.NOTT] = suTownNullInds[SuTown.NOTT];

            sdTown.tpoint = suTown.tpoint;
            sdTownNullInds[SdTown.TPOINT] = suTownNullInds[SuTown.TPOINT];

            sdTown.adate = suTown.adate;
            sdTownNullInds[SdTown.ADATE] = suTownNullInds[SuTown.ADATE];

            sdTown.sdate = suTown.sdate;
            sdTownNullInds[SdTown.SDATE] = suTownNullInds[SuTown.SDATE];

            sdTown.mdate = suTown.mdate;
            sdTownNullInds[SdTown.MDATE] = suTownNullInds[SuTown.MDATE];

            sdTown.mtime = suTown.mtime;
            sdTownNullInds[SdTown.MTIME] = suTownNullInds[SuTown.MTIME];

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

            tgtValPtrs[CALL1] = Marshal.AllocHGlobal(CALL1_SZ + 1);
            nullIndPtrs[CALL1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CALL1 + 1, tgtValPtrs[CALL1], SdTown.CALL1_SZ, nullIndPtrs[CALL1]);

            tgtValPtrs[OPER] = Marshal.AllocHGlobal(OPER_SZ + 1);
            nullIndPtrs[OPER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, OPER + 1, tgtValPtrs[OPER], SdTown.OPER_SZ, nullIndPtrs[OPER]);

            tgtValPtrs[TWCODE] = Marshal.AllocHGlobal(TWCODE_SZ + 1);
            nullIndPtrs[TWCODE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TWCODE + 1, tgtValPtrs[TWCODE], SdTown.TWCODE_SZ, nullIndPtrs[TWCODE]);

            tgtValPtrs[TWHT] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[TWHT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, TWHT + 1, tgtValPtrs[TWHT], nullIndPtrs[TWHT]);

            tgtValPtrs[ATWRNO] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[ATWRNO] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, ATWRNO + 1, tgtValPtrs[ATWRNO], nullIndPtrs[ATWRNO]);

            tgtValPtrs[TWLI] = Marshal.AllocHGlobal(TWLI_SZ + 1);
            nullIndPtrs[TWLI] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TWLI + 1, tgtValPtrs[TWLI], SdTown.TWLI_SZ, nullIndPtrs[TWLI]);

            tgtValPtrs[TWPA] = Marshal.AllocHGlobal(TWPA_SZ + 1);
            nullIndPtrs[TWPA] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TWPA + 1, tgtValPtrs[TWPA], SdTown.TWPA_SZ, nullIndPtrs[TWPA]);

            tgtValPtrs[NOTT] = Marshal.AllocHGlobal(NOTT_SZ + 1);
            nullIndPtrs[NOTT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, NOTT + 1, tgtValPtrs[NOTT], SdTown.NOTT_SZ, nullIndPtrs[NOTT]);

            tgtValPtrs[TPOINT] = Marshal.AllocHGlobal(TPOINT_SZ + 1);
            nullIndPtrs[TPOINT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TPOINT + 1, tgtValPtrs[TPOINT], SdTown.TPOINT_SZ, nullIndPtrs[TPOINT]);

            tgtValPtrs[ADATE] = Marshal.AllocHGlobal(ADATE_SZ + 1);
            nullIndPtrs[ADATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ADATE + 1, tgtValPtrs[ADATE], SdTown.ADATE_SZ, nullIndPtrs[ADATE]);

            tgtValPtrs[SDATE] = Marshal.AllocHGlobal(SDATE_SZ + 1);
            nullIndPtrs[SDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SDATE + 1, tgtValPtrs[SDATE], SdTown.SDATE_SZ, nullIndPtrs[SDATE]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], SdTown.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], SdTown.MTIME_SZ, nullIndPtrs[MTIME]);
        }



        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdEqpt object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="sdTown"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SdTown sdTown, out SQLLEN[] nullInds)
        {
            sdTown = new SdTown();
            nullInds = NullHelper.CreateArrayOfNullInd(SdTown.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[CALL1]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTown.call1 = "";
                nullInds[CALL1] = Constant.DB_NULL;
            }
            else
            {
                sdTown.call1 = Marshal.PtrToStringAnsi(tgtValPtrs[CALL1]).Trim();
                nullInds[CALL1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[OPER]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTown.oper = "";
                nullInds[OPER] = Constant.DB_NULL;
            }
            else
            {
                sdTown.oper = Marshal.PtrToStringAnsi(tgtValPtrs[OPER]).Trim();
                nullInds[OPER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TWCODE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTown.twcode = "";
                nullInds[TWCODE] = Constant.DB_NULL;
            }
            else
            {
                sdTown.twcode = Marshal.PtrToStringAnsi(tgtValPtrs[TWCODE]).Trim();
                nullInds[TWCODE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TWHT]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[TWHT], F, 0, 1);
                sdTown.twht = F[0];
                nullInds[TWHT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ATWRNO]);
            if (nullInd != Constant.DB_NULL)
            {
                sdTown.atwrno = (byte)Marshal.ReadInt16(tgtValPtrs[ATWRNO]);
                nullInds[ATWRNO] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TWLI]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTown.twli = "";
                nullInds[TWLI] = Constant.DB_NULL;
            }
            else
            {
                sdTown.twli = Marshal.PtrToStringAnsi(tgtValPtrs[TWLI]).Trim();
                nullInds[TWLI] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TWPA]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTown.twpa = "";
                nullInds[TWPA] = Constant.DB_NULL;
            }
            else
            {
                sdTown.twpa = Marshal.PtrToStringAnsi(tgtValPtrs[TWPA]).Trim();
                nullInds[TWPA] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[NOTT]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTown.nott = "";
                nullInds[NOTT] = Constant.DB_NULL;
            }
            else
            {
                sdTown.nott = Marshal.PtrToStringAnsi(tgtValPtrs[NOTT]).Trim();
                nullInds[NOTT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TPOINT]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTown.tpoint = "";
                nullInds[TPOINT] = Constant.DB_NULL;
            }
            else
            {
                sdTown.tpoint = Marshal.PtrToStringAnsi(tgtValPtrs[TPOINT]).Trim();
                nullInds[TPOINT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ADATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTown.adate = "";
                nullInds[ADATE] = Constant.DB_NULL;
            }
            else
            {
                sdTown.adate = Marshal.PtrToStringAnsi(tgtValPtrs[ADATE]).Trim();
                nullInds[ADATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTown.sdate = "";
                nullInds[SDATE] = Constant.DB_NULL;
            }
            else
            {
                sdTown.sdate = Marshal.PtrToStringAnsi(tgtValPtrs[SDATE]).Trim();
                nullInds[SDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTown.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                sdTown.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdTown.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                sdTown.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

        }


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
" CONSTRAINT [PK_sd_town] PRIMARY KEY CLUSTERED " +
"(" +
    "[call1] ASC," +
    "[atwrno] ASC" +
") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";


    }
}

