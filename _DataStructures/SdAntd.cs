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
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;            //Invented to mimic (char *) for [In]  only.
    using SQLCHARPTRINOUT = IntPtr;       //Invented to mimic (char *) for [In, Out].
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLINTEGERPTR = IntPtr;
    using SQLLEN = Int64;
    using SQLLENPTR = IntPtr;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;
    using SQLSETPOSIROW = UInt64;
    using SQLSMALLINT = Int16;
    using SQLSMALLINTPTR = IntPtr;
    using SQLULEN = UInt64;
    using SQLUSMALLINT = UInt16;

    /// <summary>
    /// This class has fields that are isomorphic with the DB table <b>main.sd_antd</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SdAntd
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ACODE_SZ)]
        public string acode;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float antang;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float dcov;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float dxpv;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float dcoh;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float dxph;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float dtilt;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int interpstat;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //----------------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 10;

        public const int ACODE = 0;
        public const int ANTANG = 1;
        public const int DCOV = 2;
        public const int DXPV = 3;
        public const int DCOH = 4;
        public const int DXPH = 5;
        public const int DTILT = 6;
        public const int INTERPSTAT = 7;
        public const int MDATE = 8;
        public const int MTIME = 9;

        public const int ACODE_SZ = Constant.ACODE_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "acode", "antang", "dcov", "dxpv", "dcoh", "dxph", "dtilt", "interpstat", "mdate", "mtime" };

        //List of all column names with bindings for SQL 'update' command, i.e. " cmd=?, recstat=?, call1=?, call2=?, bndcde=?, anum=?, ... , mtime=? "
        private static string mAllBindingsAsCSV;

        private static string mAllFieldsAsCSV;

        //Provide read-only access to specific members.
        public static string AllColumnsForSqlSelect { get { return mAllFieldsAsCSV; } }
        public static string AllColumnsForSqlUpdateAsBindings { get { return mAllBindingsAsCSV; } }


        //----------------------------------------------------------------

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static SdAntd()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Initialize the string members as empty strings.
        /// </summary>
        public SdAntd()
        {
            Initialize();
        }

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public void Initialize()
        {
            acode = "";
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
            sb.AppendLine("\r\n");
            sb.AppendLine("acode =      " + acode);
            sb.AppendLine("antang =     " + antang);
            sb.AppendLine("dcov =       " + dcov);
            sb.AppendLine("dxpv =       " + dxpv);
            sb.AppendLine("dcoh =       " + dcoh);
            sb.AppendLine("dxph =       " + dxph);
            sb.AppendLine("dtilt =      " + dtilt);
            sb.AppendLine("interpstat = " + interpstat);
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
            StringBuilder sb = new StringBuilder();
            int n = 0;

            sb.AppendLine("\r\n");
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "acode =      " + acode);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "antang =     " + antang);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "dcov =       " + dcov);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "dxpv =       " + dxpv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "dcoh =       " + dcoh);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "dxph =       " + dxph);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "dtilt =      " + dtilt);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "interpstat = " + interpstat);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate =      " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime =      " + mtime);

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
        /// For each field of this object, the method allocates a type-specific amount
        /// of global (heap) memory, copies the value of the field into it, and returns
        /// an IntPtr[] containing all of the start addresses.
        /// </summary>
        /// <returns></returns>
        public SQLPOINTER[] CopyToArrayOfSQLPOINTERinGlobalMemory()
        {
            //Can only copy arrays of float and double into native memory using Marshal method.
            float[] F = new float[1];
            double[] D = new double[1];

            SQLPOINTER[] parameterValuePtrs = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtrs[ACODE] = Marshal.StringToHGlobalAnsi(acode);

            parameterValuePtrs[ANTANG] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = antang;
            Marshal.Copy(F, 0, parameterValuePtrs[ANTANG], 1);

            parameterValuePtrs[DCOV] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = dcov;
            Marshal.Copy(F, 0, parameterValuePtrs[DCOV], 1);

            parameterValuePtrs[DXPV] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = dxpv;
            Marshal.Copy(F, 0, parameterValuePtrs[DXPV], 1);

            parameterValuePtrs[DCOH] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = dcoh;
            Marshal.Copy(F, 0, parameterValuePtrs[DCOH], 1);

            parameterValuePtrs[DXPH] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = dxph;
            Marshal.Copy(F, 0, parameterValuePtrs[DXPH], 1);

            parameterValuePtrs[DTILT] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = dtilt;
            Marshal.Copy(F, 0, parameterValuePtrs[DTILT], 1);

            parameterValuePtrs[INTERPSTAT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtrs[INTERPSTAT], interpstat);

            parameterValuePtrs[MDATE] = Marshal.StringToHGlobalAnsi(mdate);

            parameterValuePtrs[MTIME] = Marshal.StringToHGlobalAnsi(mtime);

            return parameterValuePtrs;
        }

        /// <summary>
        /// This method returns an IntPtr[] whose elements point to the start addresses of possibly 
        /// non-contiguous blocks of global (heap) memory, each of a sufficient size to hold the 
        /// value of a specific member of this object; each of the pointers is then registered
        /// with ODBC as a 'binding'. Similarly for the nullInd array associated with this object.
        /// The binding order is that of the columns in the table main.sd_ante; the ODBC binding
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

            tgtValPtrs[ACODE] = Marshal.AllocHGlobal(ACODE_SZ + 1);
            nullIndPtrs[ACODE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ACODE + 1, tgtValPtrs[ACODE], SdAntd.ACODE_SZ, nullIndPtrs[ACODE]);

            tgtValPtrs[ANTANG] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[ANTANG] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, ANTANG + 1, tgtValPtrs[ANTANG], nullIndPtrs[ANTANG]);

            tgtValPtrs[DCOV] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[DCOV] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, DCOV + 1, tgtValPtrs[DCOV], nullIndPtrs[DCOV]);

            tgtValPtrs[DXPV] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[DXPV] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, DXPV + 1, tgtValPtrs[DXPV], nullIndPtrs[DXPV]);

            tgtValPtrs[DCOH] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[DCOH] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, DCOH + 1, tgtValPtrs[DCOH], nullIndPtrs[DCOH]);

            tgtValPtrs[DXPH] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[DXPH] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, DXPH + 1, tgtValPtrs[DXPH], nullIndPtrs[DXPH]);

            tgtValPtrs[DTILT] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[DTILT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, DTILT + 1, tgtValPtrs[DTILT], nullIndPtrs[DTILT]);

            tgtValPtrs[INTERPSTAT] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[INTERPSTAT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, INTERPSTAT + 1, tgtValPtrs[INTERPSTAT], nullIndPtrs[INTERPSTAT]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], SdAntd.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], SdAntd.MTIME_SZ, nullIndPtrs[MTIME]);
        }

        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdAnte object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="sdAntd"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SdAntd sdAntd, out SQLLEN[] nullInds)
        {
            sdAntd = new SdAntd();
            nullInds = NullHelper.CreateArrayOfNullInd(SdAntd.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACODE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdAntd.acode = "";
                nullInds[ACODE] = Constant.DB_NULL;
            }
            else
            {
                sdAntd.acode = Marshal.PtrToStringAnsi(tgtValPtrs[ACODE]).Trim();
                nullInds[ACODE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANTANG]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ANTANG], F, 0, 1);
                sdAntd.antang = F[0];
                nullInds[ANTANG] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[DCOV]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[DCOV], F, 0, 1);
                sdAntd.dcov = F[0];
                nullInds[DCOV] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[DXPV]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[DXPV], F, 0, 1);
                sdAntd.dxpv = F[0];
                nullInds[DXPV] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[DCOH]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[DCOH], F, 0, 1);
                sdAntd.dcoh = F[0];
                nullInds[DCOH] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[DXPH]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[DXPH], F, 0, 1);
                sdAntd.dxph = F[0];
                nullInds[DXPH] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[DTILT]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[DTILT], F, 0, 1);
                sdAntd.dtilt = F[0];
                nullInds[DTILT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[INTERPSTAT]);
            if (nullInd != Constant.DB_NULL)
            {
                sdAntd.interpstat = Marshal.ReadInt32(tgtValPtrs[INTERPSTAT]);
                nullInds[INTERPSTAT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdAntd.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                sdAntd.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdAntd.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                sdAntd.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

        }

        /// <summary>
        /// This method returns the member values of an object formatted
        /// as a string of columnName = 'value' expressions that can be used
        /// in the SET clause of an SQL UPDATE query.
        /// </summary>
        /// <param name="nullInds"> - array of ODBC null indicators.</param>
        /// <returns></returns>
        public string ToStringAsCSVequates(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(columnNames[SdAntd.ACODE] + "=" + (nullInds[SdAntd.ACODE] == Constant.DB_NULL ? "NULL," : "'" + acode.ToString() + "',"));
            sb.Append(columnNames[SdAntd.ANTANG] + "=" + (nullInds[SdAntd.ANTANG] == Constant.DB_NULL ? "NULL," : "'" + antang.ToString() + "',"));
            sb.Append(columnNames[SdAntd.DCOV] + "=" + (nullInds[SdAntd.DCOV] == Constant.DB_NULL ? "NULL," : "'" + dcov.ToString() + "',"));
            sb.Append(columnNames[SdAntd.DXPV] + "=" + (nullInds[SdAntd.DXPV] == Constant.DB_NULL ? "NULL," : "'" + dxpv.ToString() + "',"));
            sb.Append(columnNames[SdAntd.DCOH] + "=" + (nullInds[SdAntd.DCOH] == Constant.DB_NULL ? "NULL," : "'" + dcoh.ToString() + "',"));
            sb.Append(columnNames[SdAntd.DXPH] + "=" + (nullInds[SdAntd.DXPH] == Constant.DB_NULL ? "NULL," : "'" + dxph.ToString() + "',"));
            sb.Append(columnNames[SdAntd.DTILT] + "=" + (nullInds[SdAntd.DTILT] == Constant.DB_NULL ? "NULL," : "'" + dtilt.ToString() + "',"));
            sb.Append(columnNames[SdAntd.INTERPSTAT] + "=" + (nullInds[SdAntd.INTERPSTAT] == Constant.DB_NULL ? "NULL," : "'" + interpstat.ToString() + "',"));
            sb.Append(columnNames[SdAntd.MDATE] + "=" + (nullInds[SdAntd.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',"));
            sb.Append(columnNames[SdAntd.MTIME] + "=" + (nullInds[SdAntd.MTIME] == Constant.DB_NULL ? "NULL" : "'" + mtime.ToString() + "'"));

            return sb.ToString();
        }

        /// <summary>
        /// This method returns the member values of an object formatted
        /// as a CSV string that can be used to provide the VALUES in an SQL INSERT query.
        /// </summary>
        /// <param name="nullInds"> - array of ODBC null indicators.</param>
        /// <returns></returns>
        public string ToStringAsCSV(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[SdAntd.ACODE] == Constant.DB_NULL ? "NULL," : "'" + acode.ToString() + "',");
            sb.Append(nullInds[SdAntd.ANTANG] == Constant.DB_NULL ? "NULL," : "'" + antang.ToString() + "',");
            sb.Append(nullInds[SdAntd.DCOV] == Constant.DB_NULL ? "NULL," : "'" + dcov.ToString() + "',");
            sb.Append(nullInds[SdAntd.DXPV] == Constant.DB_NULL ? "NULL," : "'" + dxpv.ToString() + "',");
            sb.Append(nullInds[SdAntd.DCOH] == Constant.DB_NULL ? "NULL," : "'" + dcoh.ToString() + "',");
            sb.Append(nullInds[SdAntd.DXPH] == Constant.DB_NULL ? "NULL," : "'" + dxph.ToString() + "',");
            sb.Append(nullInds[SdAntd.DTILT] == Constant.DB_NULL ? "NULL," : "'" + dtilt.ToString() + "',");
            sb.Append(nullInds[SdAntd.INTERPSTAT] == Constant.DB_NULL ? "NULL," : "'" + interpstat.ToString() + "',");
            sb.Append(nullInds[SdAntd.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',");
            sb.Append(nullInds[SdAntd.MTIME] == Constant.DB_NULL ? "NULL" : "'" + mtime.ToString() + "'");

            return sb.ToString();
        }

        /// <summary>
        /// This method returns instantiated SdAntd and SQLLEN[] nullInds objects whose
        /// members are set i.a.w. a prescribed SuAntd object and its associated nullInds; SdAntd 
        /// has identical members to SuAntd except for 'cmd'.
        /// </summary>
        /// <param name="suAntd"></param>
        /// <param name="suAntdNullInds"></param>
        /// <param name="sdAntd"></param>
        /// <param name="sdAntdNullInds"></param>
        public static void MakeSdFromSu(SuAntd suAntd, SQLLEN[] suAntdNullInds, out SdAntd sdAntd, out SQLLEN[] sdAntdNullInds)
        {
            // SdAntd has identical members to SuAntd except for 'cmd'.

            sdAntd = new SdAntd();
            sdAntdNullInds = new SQLLEN[SdAntd.NUM_COLUMNS];

            sdAntd.acode = suAntd.acode;
            sdAntdNullInds[SdAntd.ACODE] = suAntdNullInds[SuAntd.ACODE];

            sdAntd.antang = suAntd.antang;
            sdAntdNullInds[SdAntd.ANTANG] = suAntdNullInds[SuAntd.ANTANG];

            sdAntd.dcov = suAntd.dcov;
            sdAntdNullInds[SdAntd.DCOV] = suAntdNullInds[SuAntd.DCOV];

            sdAntd.dxpv = suAntd.dxpv;
            sdAntdNullInds[SdAntd.DXPV] = suAntdNullInds[SuAntd.DXPV];

            sdAntd.dcoh = suAntd.dcoh;
            sdAntdNullInds[SdAntd.DCOH] = suAntdNullInds[SuAntd.DCOH];

            sdAntd.dxph = suAntd.dxph;
            sdAntdNullInds[SdAntd.DXPH] = suAntdNullInds[SuAntd.DXPH];

            sdAntd.dtilt = suAntd.dtilt;
            sdAntdNullInds[SdAntd.DTILT] = suAntdNullInds[SuAntd.DTILT];

            sdAntd.interpstat = suAntd.interpstat;
            sdAntdNullInds[SdAntd.INTERPSTAT] = suAntdNullInds[SuAntd.INTERPSTAT];

            sdAntd.mdate = suAntd.mdate;
            sdAntdNullInds[SdAntd.MDATE] = suAntdNullInds[SuAntd.MDATE];

            sdAntd.mtime = suAntd.mtime;
            sdAntdNullInds[SdAntd.MTIME] = suAntdNullInds[SuAntd.MTIME];
        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[sd_antd](" +
    "[acode] [char](12) NOT NULL," +
    "[antang] [real] NOT NULL," +
    "[dcov] [real] NULL," +
    "[dxpv] [real] NULL," +
    "[dcoh] [real] NULL," +
    "[dxph] [real] NULL," +
    "[dtilt] [real] NULL," +
    "[interpstat] [int] NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
 "CONSTRAINT [PK_sd_antd] PRIMARY KEY CLUSTERED " +
"(" +
    "[acode] ASC," +
    "[antang] ASC" +
") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";



        /*
        acode
        antang
        dcov
        dxpv
        dcoh
        dxph
        dtilt
        interpstat
        mdate
        mtime
        */




    }
}

