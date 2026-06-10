# Documented File: SdPlnd.cs
**Repository Path:** `_DataStructures\SdPlnd.cs`
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
    using _NewLib;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using SQLHANDLE = IntPtr;

    /// <summary>
    /// This class has member field that are isomorphic to the 
    /// columns of DB table <b>main.sd_plnd</b> .
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SdPlnd
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SBAND_SZ)]
        public string sband;    //#2
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SPLAN_SZ)]
        public string splan;    //#3
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short spno;    //#4
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double set1;    //#5
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = S1CHID_SZ)]
        public string s1chid;    //#6
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double set2;    //#7
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = S2CHID_SZ)]
        public string s2chid;    //#8
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double set3;    //#9
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = S3CHID_SZ)]
        public string s3chid;    //#10
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double set4;    //#11
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = S4CHID_SZ)]
        public string s4chid;    //#12
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;    //#13
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;    //#14

        //-------------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 13;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "sband", "splan", "spno", "set1", "s1chid", "set2", "s2chid", "set3", "s3chid", "set4", "s4chid", "mdate", "mtime" };

        public const int SBAND = 0;
        public const int SPLAN = 1;
        public const int SPNO = 2;
        public const int SET1 = 3;
        public const int S1CHID = 4;
        public const int SET2 = 5;
        public const int S2CHID = 6;
        public const int SET3 = 7;
        public const int S3CHID = 8;
        public const int SET4 = 9;
        public const int S4CHID = 10;
        public const int MDATE = 11;
        public const int MTIME = 12;

        public const int SBAND_SZ = Constant.BNDCDE_SZ;
        public const int SPLAN_SZ = Constant.PLAN_SZ;
        public const int S1CHID_SZ = Constant.CHID_SZ;
        public const int S2CHID_SZ = Constant.CHID_SZ;
        public const int S3CHID_SZ = Constant.CHID_SZ;
        public const int S4CHID_SZ = Constant.CHID_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //-------------------------------------------------------------------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public SdPlnd()
        {
            sband = "";
            splan = "";
            s1chid = "";
            s2chid = "";
            s3chid = "";
            s4chid = "";
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
            sb.Append("\nspno = " + spno);
            sb.Append("\nset1 = " + set1);
            sb.Append("\ns1chid = " + s1chid);
            sb.Append("\nset2 = " + set2);
            sb.Append("\ns2chid = " + s2chid);
            sb.Append("\nset3 = " + set3);
            sb.Append("\ns3chid = " + s3chid);
            sb.Append("\nset4 = " + set4);
            sb.Append("\ns4chid = " + s4chid);
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
            sb.Append("\n ===== SdPlnd ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "sband = " + sband);
            sb.Append("\n" + nullInds[i++] + "      " + "splan = " + splan);
            sb.Append("\n" + nullInds[i++] + "      " + "spno = " + spno);
            sb.Append("\n" + nullInds[i++] + "      " + "set1 = " + set1);
            sb.Append("\n" + nullInds[i++] + "      " + "s1chid = " + s1chid);
            sb.Append("\n" + nullInds[i++] + "      " + "set2 = " + set2);
            sb.Append("\n" + nullInds[i++] + "      " + "s2chid = " + s2chid);
            sb.Append("\n" + nullInds[i++] + "      " + "set3 = " + set3);
            sb.Append("\n" + nullInds[i++] + "      " + "s3chid = " + s3chid);
            sb.Append("\n" + nullInds[i++] + "      " + "set4 = " + set4);
            sb.Append("\n" + nullInds[i++] + "      " + "s4chid = " + s4chid);
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

            sb.Append(nullInds[SdPlnd.SBAND] == Constant.DB_NULL ? "NULL, " : "'" + sband.ToString() + "', ");
            sb.Append(nullInds[SdPlnd.SPLAN] == Constant.DB_NULL ? "NULL, " : "'" + splan.ToString() + "', ");
            sb.Append(nullInds[SdPlnd.SPNO] == Constant.DB_NULL ? "NULL, " : "'" + spno.ToString() + "', ");
            sb.Append(nullInds[SdPlnd.SET1] == Constant.DB_NULL ? "NULL, " : "'" + set1.ToString() + "', ");
            sb.Append(nullInds[SdPlnd.S1CHID] == Constant.DB_NULL ? "NULL, " : "'" + s1chid.ToString() + "', ");
            sb.Append(nullInds[SdPlnd.SET2] == Constant.DB_NULL ? "NULL, " : "'" + set2.ToString() + "', ");
            sb.Append(nullInds[SdPlnd.S2CHID] == Constant.DB_NULL ? "NULL, " : "'" + s2chid.ToString() + "', ");
            sb.Append(nullInds[SdPlnd.SET3] == Constant.DB_NULL ? "NULL, " : "'" + set3.ToString() + "', ");
            sb.Append(nullInds[SdPlnd.S3CHID] == Constant.DB_NULL ? "NULL, " : "'" + s3chid.ToString() + "', ");
            sb.Append(nullInds[SdPlnd.SET4] == Constant.DB_NULL ? "NULL, " : "'" + set4.ToString() + "', ");
            sb.Append(nullInds[SdPlnd.S4CHID] == Constant.DB_NULL ? "NULL, " : "'" + s4chid.ToString() + "', ");
            sb.Append(nullInds[SdPlnd.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SdPlnd.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

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

            sb.Append(columnNames[SdPlnd.SBAND] + " = " + (nullInds[SdPlnd.SBAND] == Constant.DB_NULL ? "NULL, " : "'" + sband.ToString() + "', "));
            sb.Append(columnNames[SdPlnd.SPLAN] + " = " + (nullInds[SdPlnd.SPLAN] == Constant.DB_NULL ? "NULL, " : "'" + splan.ToString() + "', "));
            sb.Append(columnNames[SdPlnd.SPNO] + " = " + (nullInds[SdPlnd.SPNO] == Constant.DB_NULL ? "NULL, " : "'" + spno.ToString() + "', "));
            sb.Append(columnNames[SdPlnd.SET1] + " = " + (nullInds[SdPlnd.SET1] == Constant.DB_NULL ? "NULL, " : "'" + set1.ToString() + "', "));
            sb.Append(columnNames[SdPlnd.S1CHID] + " = " + (nullInds[SdPlnd.S1CHID] == Constant.DB_NULL ? "NULL, " : "'" + s1chid.ToString() + "', "));
            sb.Append(columnNames[SdPlnd.SET2] + " = " + (nullInds[SdPlnd.SET2] == Constant.DB_NULL ? "NULL, " : "'" + set2.ToString() + "', "));
            sb.Append(columnNames[SdPlnd.S2CHID] + " = " + (nullInds[SdPlnd.S2CHID] == Constant.DB_NULL ? "NULL, " : "'" + s2chid.ToString() + "', "));
            sb.Append(columnNames[SdPlnd.SET3] + " = " + (nullInds[SdPlnd.SET3] == Constant.DB_NULL ? "NULL, " : "'" + set3.ToString() + "', "));
            sb.Append(columnNames[SdPlnd.S3CHID] + " = " + (nullInds[SdPlnd.S3CHID] == Constant.DB_NULL ? "NULL, " : "'" + s3chid.ToString() + "', "));
            sb.Append(columnNames[SdPlnd.SET4] + " = " + (nullInds[SdPlnd.SET4] == Constant.DB_NULL ? "NULL, " : "'" + set4.ToString() + "', "));
            sb.Append(columnNames[SdPlnd.S4CHID] + " = " + (nullInds[SdPlnd.S4CHID] == Constant.DB_NULL ? "NULL, " : "'" + s4chid.ToString() + "', "));
            sb.Append(columnNames[SdPlnd.MDATE] + " = " + (nullInds[SdPlnd.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SdPlnd.MTIME] + " = " + (nullInds[SdPlnd.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

            return sb.ToString();
        }

        /// <summary>
        /// This method returns instantiated SdOper and SQLLEN[] nullInds objects whose
        /// members are set i.a.w. a prescribed SuOper object and its associated nullInds; 
        /// SdOper has identical members to SuOper except for 'cmd' and 'recstat'.
        /// </summary>
        /// <param name="suPlnd"></param>
        /// <param name="suPlndNullInds"></param>
        /// <param name="sdPlnd"></param>
        /// <param name="sdPlndNullInds"></param>
        public static void MakeSdFromSu(SuPlnd suPlnd, SQLLEN[] suPlndNullInds, out SdPlnd sdPlnd, out SQLLEN[] sdPlndNullInds)
        {
            sdPlnd = new SdPlnd();
            sdPlndNullInds = new SQLLEN[NUM_COLUMNS];


            sdPlnd.sband = suPlnd.sband;
            sdPlndNullInds[SdPlnd.SBAND] = suPlndNullInds[SuPlnd.SBAND];

            sdPlnd.splan = suPlnd.splan;
            sdPlndNullInds[SdPlnd.SPLAN] = suPlndNullInds[SuPlnd.SPLAN];

            sdPlnd.spno = suPlnd.spno;
            sdPlndNullInds[SdPlnd.SPNO] = suPlndNullInds[SuPlnd.SPNO];

            sdPlnd.set1 = suPlnd.set1;
            sdPlndNullInds[SdPlnd.SET1] = suPlndNullInds[SuPlnd.SET1];

            sdPlnd.s1chid = suPlnd.s1chid;
            sdPlndNullInds[SdPlnd.S1CHID] = suPlndNullInds[SuPlnd.S1CHID];

            sdPlnd.set2 = suPlnd.set2;
            sdPlndNullInds[SdPlnd.SET2] = suPlndNullInds[SuPlnd.SET2];

            sdPlnd.s2chid = suPlnd.s2chid;
            sdPlndNullInds[SdPlnd.S2CHID] = suPlndNullInds[SuPlnd.S2CHID];

            sdPlnd.set3 = suPlnd.set3;
            sdPlndNullInds[SdPlnd.SET3] = suPlndNullInds[SuPlnd.SET3];

            sdPlnd.s3chid = suPlnd.s3chid;
            sdPlndNullInds[SdPlnd.S3CHID] = suPlndNullInds[SuPlnd.S3CHID];

            sdPlnd.set4 = suPlnd.set4;
            sdPlndNullInds[SdPlnd.SET4] = suPlndNullInds[SuPlnd.SET4];

            sdPlnd.s4chid = suPlnd.s4chid;
            sdPlndNullInds[SdPlnd.S4CHID] = suPlndNullInds[SuPlnd.S4CHID];

            sdPlnd.mdate = suPlnd.mdate;
            sdPlndNullInds[SdPlnd.MDATE] = suPlndNullInds[SuPlnd.MDATE];

            sdPlnd.mtime = suPlnd.mtime;
            sdPlndNullInds[SdPlnd.MTIME] = suPlndNullInds[SuPlnd.MTIME];

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
            Bind.BindBufferToString(hStmt, SBAND + 1, tgtValPtrs[SBAND], SdPlnd.SBAND_SZ, nullIndPtrs[SBAND]);

            tgtValPtrs[SPLAN] = Marshal.AllocHGlobal(SPLAN_SZ + 1);
            nullIndPtrs[SPLAN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SPLAN + 1, tgtValPtrs[SPLAN], SdPlnd.SPLAN_SZ, nullIndPtrs[SPLAN]);

            tgtValPtrs[SPNO] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[SPNO] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, SPNO + 1, tgtValPtrs[SPNO], nullIndPtrs[SPNO]);

            tgtValPtrs[SET1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[SET1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, SET1 + 1, tgtValPtrs[SET1], nullIndPtrs[SET1]);

            tgtValPtrs[S1CHID] = Marshal.AllocHGlobal(S1CHID_SZ + 1);
            nullIndPtrs[S1CHID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, S1CHID + 1, tgtValPtrs[S1CHID], SdPlnd.S1CHID_SZ, nullIndPtrs[S1CHID]);

            tgtValPtrs[SET2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[SET2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, SET2 + 1, tgtValPtrs[SET2], nullIndPtrs[SET2]);

            tgtValPtrs[S2CHID] = Marshal.AllocHGlobal(S2CHID_SZ + 1);
            nullIndPtrs[S2CHID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, S2CHID + 1, tgtValPtrs[S2CHID], SdPlnd.S2CHID_SZ, nullIndPtrs[S2CHID]);

            tgtValPtrs[SET3] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[SET3] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, SET3 + 1, tgtValPtrs[SET3], nullIndPtrs[SET3]);

            tgtValPtrs[S3CHID] = Marshal.AllocHGlobal(S3CHID_SZ + 1);
            nullIndPtrs[S3CHID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, S3CHID + 1, tgtValPtrs[S3CHID], SdPlnd.S3CHID_SZ, nullIndPtrs[S3CHID]);

            tgtValPtrs[SET4] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[SET4] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, SET4 + 1, tgtValPtrs[SET4], nullIndPtrs[SET4]);

            tgtValPtrs[S4CHID] = Marshal.AllocHGlobal(S4CHID_SZ + 1);
            nullIndPtrs[S4CHID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, S4CHID + 1, tgtValPtrs[S4CHID], SdPlnd.S4CHID_SZ, nullIndPtrs[S4CHID]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], SdPlnd.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], SdPlnd.MTIME_SZ, nullIndPtrs[MTIME]);
        }


        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdOper object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="sdPlnd"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SdPlnd sdPlnd, out SQLLEN[] nullInds)
        {
            sdPlnd = new SdPlnd();
            nullInds = NullHelper.CreateArrayOfNullInd(SdPlnd.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[SBAND]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlnd.sband = "";
                nullInds[SBAND] = Constant.DB_NULL;
            }
            else
            {
                sdPlnd.sband = Marshal.PtrToStringAnsi(tgtValPtrs[SBAND]).Trim();
                nullInds[SBAND] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SPLAN]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlnd.splan = "";
                nullInds[SPLAN] = Constant.DB_NULL;
            }
            else
            {
                sdPlnd.splan = Marshal.PtrToStringAnsi(tgtValPtrs[SPLAN]).Trim();
                nullInds[SPLAN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SPNO]);
            if (nullInd != Constant.DB_NULL)
            {
                sdPlnd.spno = Marshal.ReadInt16(tgtValPtrs[SPNO]);
                nullInds[SPNO] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SET1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[SET1], D, 0, 1);
                sdPlnd.set1 = D[0];
                nullInds[SET1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[S1CHID]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlnd.s1chid = "";
                nullInds[S1CHID] = Constant.DB_NULL;
            }
            else
            {
                sdPlnd.s1chid = Marshal.PtrToStringAnsi(tgtValPtrs[S1CHID]).Trim();
                nullInds[S1CHID] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SET2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[SET2], D, 0, 1);
                sdPlnd.set2 = D[0];
                nullInds[SET2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[S2CHID]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlnd.s2chid = "";
                nullInds[S2CHID] = Constant.DB_NULL;
            }
            else
            {
                sdPlnd.s2chid = Marshal.PtrToStringAnsi(tgtValPtrs[S2CHID]).Trim();
                nullInds[S2CHID] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SET3]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[SET3], D, 0, 1);
                sdPlnd.set3 = D[0];
                nullInds[SET3] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[S3CHID]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlnd.s3chid = "";
                nullInds[S3CHID] = Constant.DB_NULL;
            }
            else
            {
                sdPlnd.s3chid = Marshal.PtrToStringAnsi(tgtValPtrs[S3CHID]).Trim();
                nullInds[S3CHID] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SET4]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[SET4], D, 0, 1);
                sdPlnd.set4 = D[0];
                nullInds[SET4] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[S4CHID]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlnd.s4chid = "";
                nullInds[S4CHID] = Constant.DB_NULL;
            }
            else
            {
                sdPlnd.s4chid = Marshal.PtrToStringAnsi(tgtValPtrs[S4CHID]).Trim();
                nullInds[S4CHID] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlnd.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                sdPlnd.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdPlnd.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                sdPlnd.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[sd_plnd](" +
    "[sband] [char](4) NOT NULL," +
    "[splan] [char](4) NOT NULL," +
    "[spno] [tinyint] NOT NULL," +
    "[set1] [float] NULL," +
    "[s1chid] [char](4) NULL," +
    "[set2] [float] NULL," +
    "[s2chid] [char](4) NULL," +
    "[set3] [float] NULL," +
    "[s3chid] [char](4) NULL," +
    "[set4] [float] NULL," +
    "[s4chid] [char](4) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
" CONSTRAINT [PK_sd_plnd] PRIMARY KEY CLUSTERED " +
"(" +
    "[sband] ASC," +
    "[splan] ASC," +
    "[spno] ASC" +
")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";




        /*
        sband
        splan
        spno
        set1
        s1chid
        set2
        s2chid
        set3
        s3chid
        set4
        s4chid
        mdate
        mtime
        */









    }
}

```
