using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;

    /// <summary>
    /// This class has fields that are isomorphic with PDF table <b>&lt;userID&gt;.fe_&lt;pdfName&gt;_site</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FeSite
    {
        // IMPORTANT!
        // =========
        // The following qty. 18 member values correspond to the columns
        // of an FeSite table stored in the database; for simplicity, the
        // member and column names are prescribed to be identical.
        // The order of appearance of these qty. 18 'column' members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CMD_SZ)]
        public string cmd;          // #00
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RECSTAT_SZ)]
        public string recstat;      // #01
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = LOCATION_SZ)]
        public string location;     // #02
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NAME_SZ)]
        public string name;         // #03
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = PROV_SZ)]
        public string prov;         // #04
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = OPER_SZ)]
        public string oper;         // #05
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int latit;           // #06
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int longit;          // #07
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float grnd;          // #08
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RADIO_SZ)]
        public string radio;        // #09
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short rain;          // #10
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SDATE_SZ)]
        public string sdate;        // #11
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STATS_SZ)]
        public string stats;        // #12
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTS_SZ)]
        public string nots;         // #13
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = OPRTYP_SZ)]
        public string oprtyp;       // #14
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = REG_SZ)]
        public string reg;          // #15
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;        // #16
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;        // #17

        //----------------------------------------------------------------------------

        public const int NUM_COLUMNS = 18;

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int LOCATION = 2;
        public const int NAME = 3;
        public const int PROV = 4;
        public const int OPER = 5;
        public const int LATIT = 6;
        public const int LONGIT = 7;
        public const int GRND = 8;
        public const int RADIO = 9;
        public const int RAIN = 10;
        public const int SDATE = 11;
        public const int STATS = 12;
        public const int NOTS = 13;
        public const int OPRTYP = 14;
        public const int REG = 15;
        public const int MDATE = 16;
        public const int MTIME = 17;
        public const int SIZE_ = 18;

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int LOCATION_SZ = Constant.LOCATION_SZ;
        public const int NAME_SZ = Constant.FE_SITE_NAME_SZ;
        public const int PROV_SZ = Constant.FE_SITE_PROV_SZ;
        public const int OPER_SZ = Constant.FE_SITE_OPER_SZ;
        public const int RADIO_SZ = Constant.FE_SITE_RADIO_SZ;
        public const int SDATE_SZ = Constant.FE_SITE_SDATE_SZ;
        public const int STATS_SZ = Constant.FE_SITE_STATS_SZ;
        public const int NOTS_SZ = Constant.FE_SITE_NOTS_SZ;
        public const int OPRTYP_SZ = Constant.FE_SITE_OPRTYP_SZ;
        public const int REG_SZ = Constant.FE_SITE_REG_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "location", "name", "prov", "oper", "latit", "longit", "grnd", "radio", "rain", "sdate", "stats", "nots", "oprtyp", "reg", "mdate", "mtime" };

        //List of all column names for SQL 'select' command, i.e. " cmd, recstat, call1, call2, bndcde, anum, ... , mtime " 
        //Provide read-only access to this 'constant'.
        private static string mAllFieldsAsCSV;
        public static string AllColumnsForSqlSelect
        {
            get { return mAllFieldsAsCSV; }
        }

        //List of all column names with bindings for SQL 'update' command, i.e. " cmd=?, recstat=?, call1=?, call2=?, bndcde=?, anum=?, ... , mtime=? "
        //Provide read-only access to this 'constant'.
        private static string mAllBindingsAsCSV;
        public static string AllColumnsForSqlUpdateAsBindings
        {
            get { return mAllBindingsAsCSV; }
        }

        //----------------------------------------------------------------------------

        /// <summary>
        /// The static constructor is automatically automatically called once , before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static FeSite()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();

            //SQLPOINTER[] ParameterValuePtr = Arrays.CreateArrayUsingDefaultElementConstructor<SQLPOINTER>(NUM_COLUMNS);
            //SQLLENPTR[] StrLen_or_IndPtr = Arrays.CreateArrayUsingDefaultElementConstructor<SQLLENPTR>(NUM_COLUMNS);
        }

        /// <summary>
        /// Default object constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public FeSite()
        {
            Initialize();
        }

        /// <summary>
        /// This method initializes the value of all member fields.
        /// </summary>
        /// <param name=""></param>
        public void Initialize()
        {
            const string STRING_INIT_VAL = "";
            const short SHORT_INIT_VAL = 0;
            const int INT_INIT_VAL = 0;
            const float FLOAT_INIT_VAL = 0.0f;

            cmd = STRING_INIT_VAL;
            recstat = STRING_INIT_VAL;
            location = STRING_INIT_VAL;
            name = STRING_INIT_VAL;
            prov = STRING_INIT_VAL;
            oper = STRING_INIT_VAL;
            latit = INT_INIT_VAL;
            longit = INT_INIT_VAL;
            grnd = FLOAT_INIT_VAL;
            radio = STRING_INIT_VAL;
            rain = SHORT_INIT_VAL;
            sdate = STRING_INIT_VAL;
            stats = STRING_INIT_VAL;
            nots = STRING_INIT_VAL;
            oprtyp = STRING_INIT_VAL;
            reg = STRING_INIT_VAL;
            mdate = STRING_INIT_VAL;
            mtime = STRING_INIT_VAL;

        }

        /// <summary>
        /// This method returns the DB table key field 'location'.
        /// </summary>
        public string KeysToString()
        {
            return String.Format("location: {0}", location);
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public new string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("========== FeSite:");
            sb.Append("\ncmd = " + cmd);
            sb.Append("\nrecstat = " + recstat);
            sb.Append("\nlocation = " + location);
            sb.Append("\nname = " + name);
            sb.Append("\nprov = " + prov);
            sb.Append("\noper = " + oper);
            sb.Append("\nlatit = " + latit);
            sb.Append("\nlongit = " + longit);
            sb.Append("\ngrnd = " + grnd);
            sb.Append("\nradio = " + radio);
            sb.Append("\nrain = " + rain);
            sb.Append("\nsdate = " + sdate);
            sb.Append("\nstats = " + stats);
            sb.Append("\nnots = " + nots);
            sb.Append("\noprtyp = " + oprtyp);
            sb.Append("\nreg = " + reg);
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
            int n = 0;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("========== FeSite:");
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "cmd = " + cmd);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "recstat = " + recstat);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "location = " + location);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "name = " + name);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "prov = " + prov);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "oper = " + oper);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "latit = " + latit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "longit = " + longit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "grnd = " + grnd);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "radio = " + radio);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rain = " + rain);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "sdate = " + sdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "stats = " + stats);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "nots = " + nots);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "oprtyp = " + oprtyp);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "reg = " + reg);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate = " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime = " + mtime);

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
        /// This method returns a string comprising a complete SQL 'insert' query using
        /// bound parameter values.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string BuildSqlInsertString(string tableName)
        {
            StringBuilder valuesSB = new StringBuilder();
            valuesSB.Append("(?");
            for (int i = 1; i < NUM_COLUMNS; i++)
            {
                valuesSB.Append(", ?");
            }
            valuesSB.Append(")");

            StringBuilder sb = new StringBuilder();
            sb.Append("insert into ");
            sb.Append(tableName);
            sb.Append(" (");
            sb.Append(AllColumnsForSqlSelect);
            sb.Append(") values ");
            sb.Append(valuesSB);

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
            //Can only copy arrays of float into native memory using Marshal method.
            float[] F = new float[1];

            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[CMD] = Marshal.StringToHGlobalAnsi(cmd);

            parameterValuePtr[RECSTAT] = Marshal.StringToHGlobalAnsi(recstat);

            parameterValuePtr[LOCATION] = Marshal.StringToHGlobalAnsi(location);

            parameterValuePtr[NAME] = Marshal.StringToHGlobalAnsi(name);

            parameterValuePtr[PROV] = Marshal.StringToHGlobalAnsi(prov);

            parameterValuePtr[OPER] = Marshal.StringToHGlobalAnsi(oper);

            parameterValuePtr[LATIT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[LATIT], latit);

            parameterValuePtr[LONGIT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[LONGIT], longit);

            parameterValuePtr[GRND] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = grnd;
            Marshal.Copy(F, 0, parameterValuePtr[GRND], 1);

            parameterValuePtr[RADIO] = Marshal.StringToHGlobalAnsi(radio);

            parameterValuePtr[RAIN] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[RAIN], rain);

            parameterValuePtr[SDATE] = Marshal.StringToHGlobalAnsi(sdate);

            parameterValuePtr[STATS] = Marshal.StringToHGlobalAnsi(stats);

            parameterValuePtr[NOTS] = Marshal.StringToHGlobalAnsi(nots);

            parameterValuePtr[OPRTYP] = Marshal.StringToHGlobalAnsi(oprtyp);

            parameterValuePtr[REG] = Marshal.StringToHGlobalAnsi(reg);

            parameterValuePtr[MDATE] = Marshal.StringToHGlobalAnsi(mdate);

            parameterValuePtr[MTIME] = Marshal.StringToHGlobalAnsi(mtime);

            return parameterValuePtr;
        }

        /*
        cmd
        recstat
        location
        name
        prov
        oper
        latit
        longit
        grnd
        radio
        rain
        sdate
        stats
        nots
        oprtyp
        reg
        mdate
        mtime
        */



    }
}
