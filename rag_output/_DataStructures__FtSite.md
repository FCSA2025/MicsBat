# Documented File: FtSite.cs
**Repository Path:** `_DataStructures\FtSite.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using _Configuration;

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
    /// This class has fields that are isomorphic with TS PDF table <b>&lt;userID&gt;.ft_&lt;pdfName&gt;_site</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FtSite
    {
        // IMPORTANT!
        // =========
        // The following qty. 29 member values correspond to the columns
        // of an FtSite table stored in the database; for simplicity, the
        // member and column names are prescribed to be identical.
        // The order of appearance of these qty. 29 'column' members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CMD_SZ)]
        public string cmd;    //#00
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.RECSTAT_SZ)]
        public string recstat;    //#01
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.DB_CALL_SZ)]
        public string call1;    //#02
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FT_SITE_NAME_SZ)]
        public string name;    //#03
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.PROV_SZ)]
        public string prov;    //#04
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.OPERCODE_SZ)]
        public string oper;    //#05
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int latit;    //#06
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int longit;    //#07
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float grnd;    //#08
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.STATS_SZ)]
        public string stats;    //#09
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.DATE_SZ)]
        public string sdate;    //#10
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FT_SITE_LOC_SZ)]
        public string loc;    //#11
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ICACCOUNT_SZ)]
        public string icaccount;    //#12
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.REG_SZ)]
        public string reg;    //#13
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.SPOINT_SZ)]
        public string spoint;    //#14
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.NOTS_SZ)]
        public string nots;    //#15
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.OPRTYP_SZ)]
        public string oprtyp;    //#16
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.SNUMB_SZ)]
        public string snumb;    //#17
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short notwr;    //#18
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd1;        //#19                 /*	The following must always be adjacent */
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd2;    //#20
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd3;    //#21
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd4;    //#22
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd5;    //#23
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd6;    //#24
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd7;    //#25
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd8;    //#26
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.DATE_SZ)]
        public string mdate;    //#27
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.TIME_SZ)]
        public string mtime;    //#28

        //----------------------------------------------------------------
        //Additional public static members.

        // Constants that provide the field/column index from the field name.
        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int CALL1 = 2;
        public const int NAME = 3;
        public const int PROV = 4;
        public const int OPER = 5;
        public const int LATIT = 6;
        public const int LONGIT = 7;
        public const int GRND = 8;
        public const int STATS = 9;
        public const int SDATE = 10;
        public const int LOC = 11;
        public const int ICACCOUNT = 12;
        public const int REG = 13;
        public const int SPOINT = 14;
        public const int NOTS = 15;
        public const int OPRTYP = 16;
        public const int SNUMB = 17;
        public const int NOTWR = 18;
        public const int BANDWD1 = 19;
        public const int BANDWD2 = 20;
        public const int BANDWD3 = 21;
        public const int BANDWD4 = 22;
        public const int BANDWD5 = 23;
        public const int BANDWD6 = 24;
        public const int BANDWD7 = 25;
        public const int BANDWD8 = 26;
        public const int MDATE = 27;
        public const int MTIME = 28;

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 29;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "call1", "name", "prov", "oper", "latit", "longit", "grnd", "stats", "sdate", "loc", "icaccount", "reg", "spoint", "nots", "oprtyp", "snumb", "notwr", "bandwd1", "bandwd2", "bandwd3", "bandwd4", "bandwd5", "bandwd6", "bandwd7", "bandwd8", "mdate", "mtime" };

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

        //-------------------------------------------------------------------------

        /// <summary>
        // The static constructor is called at most one time, before any
        // instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static FtSite()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();

            SQLPOINTER[] ParameterValuePtr = Arrays.CreateArrayUsingDefaultElementConstructor<SQLPOINTER>(NUM_COLUMNS);
            SQLLENPTR[] StrLen_or_IndPtr = Arrays.CreateArrayUsingDefaultElementConstructor<SQLLENPTR>(NUM_COLUMNS);
        }

        /// <summary>
        /// Default object constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public FtSite()
        {
            //Set all column members to prescribed initial values.
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

            cmd = STRING_INIT_VAL;    //00
            recstat = STRING_INIT_VAL;    //01
            call1 = STRING_INIT_VAL;    //02
            name = STRING_INIT_VAL;    //03
            prov = STRING_INIT_VAL;    //04
            oper = STRING_INIT_VAL;    //05
            latit = INT_INIT_VAL;    //06
            longit = INT_INIT_VAL;    //07
            grnd = FLOAT_INIT_VAL;    //08
            stats = STRING_INIT_VAL;    //09
            sdate = STRING_INIT_VAL;    //10
            loc = STRING_INIT_VAL;    //11
            icaccount = STRING_INIT_VAL;    //12
            reg = STRING_INIT_VAL;    //13
            spoint = STRING_INIT_VAL;    //14
            nots = STRING_INIT_VAL;    //15
            oprtyp = STRING_INIT_VAL;    //16
            snumb = STRING_INIT_VAL;    //17
            notwr = SHORT_INIT_VAL;    //18
            bandwd1 = INT_INIT_VAL;    //19
            bandwd2 = INT_INIT_VAL;    //20
            bandwd3 = INT_INIT_VAL;    //21
            bandwd4 = INT_INIT_VAL;    //22
            bandwd5 = INT_INIT_VAL;    //23
            bandwd6 = INT_INIT_VAL;    //24
            bandwd7 = INT_INIT_VAL;    //25
            bandwd8 = INT_INIT_VAL;    //26
            mdate = STRING_INIT_VAL;    //27
            mtime = STRING_INIT_VAL;    //28
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
            sb.AppendLine("cmd =       " + cmd);
            sb.AppendLine("recstat =   " + recstat);
            sb.AppendLine("call1 =     " + call1);
            sb.AppendLine("name =      " + name);
            sb.AppendLine("prov =      " + prov);
            sb.AppendLine("oper =      " + oper);
            sb.AppendLine("latit =     " + latit);
            sb.AppendLine("longit =    " + longit);
            sb.AppendLine("grnd =      " + grnd);
            sb.AppendLine("stats =     " + stats);
            sb.AppendLine("sdate =     " + sdate);
            sb.AppendLine("loc =       " + loc);
            sb.AppendLine("icaccount = " + icaccount);
            sb.AppendLine("reg =       " + reg);
            sb.AppendLine("spoint =    " + spoint);
            sb.AppendLine("nots =      " + nots);
            sb.AppendLine("oprtyp =    " + oprtyp);
            sb.AppendLine("snumb =     " + snumb);
            sb.AppendLine("notwr =     " + notwr);
            sb.AppendLine("bandwd1 =   " + bandwd1);
            sb.AppendLine("bandwd2 =   " + bandwd2);
            sb.AppendLine("bandwd3 =   " + bandwd3);
            sb.AppendLine("bandwd4 =   " + bandwd4);
            sb.AppendLine("bandwd5 =   " + bandwd5);
            sb.AppendLine("bandwd6 =   " + bandwd6);
            sb.AppendLine("bandwd7 =   " + bandwd7);
            sb.AppendLine("bandwd8 =   " + bandwd8);
            sb.AppendLine("mdate =     " + mdate);
            sb.AppendLine("mtime =     " + mtime);
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

            parameterValuePtr[0] = Marshal.StringToHGlobalAnsi(cmd);  // #00  string  cmd

            parameterValuePtr[1] = Marshal.StringToHGlobalAnsi(recstat);  // #01  string  recstat

            parameterValuePtr[2] = Marshal.StringToHGlobalAnsi(call1);  // #02  string  call1

            parameterValuePtr[3] = Marshal.StringToHGlobalAnsi(name);  // #03  string  name

            parameterValuePtr[4] = Marshal.StringToHGlobalAnsi(prov);  // #04  string  prov

            parameterValuePtr[5] = Marshal.StringToHGlobalAnsi(oper);  // #05  string  oper

            parameterValuePtr[6] = Marshal.AllocHGlobal(sizeof(int));  // #06  int  latit
            Marshal.WriteInt32(parameterValuePtr[6], latit);

            parameterValuePtr[7] = Marshal.AllocHGlobal(sizeof(int));  // #07  int  longit
            Marshal.WriteInt32(parameterValuePtr[7], longit);

            parameterValuePtr[8] = Marshal.AllocHGlobal(sizeof(float));  // #08  float  grnd
            F[0] = grnd;
            Marshal.Copy(F, 0, parameterValuePtr[8], 1);

            parameterValuePtr[9] = Marshal.StringToHGlobalAnsi(stats);  // #09  string  stats

            parameterValuePtr[10] = Marshal.StringToHGlobalAnsi(sdate);  // #10  string  sdate

            parameterValuePtr[11] = Marshal.StringToHGlobalAnsi(loc);  // #11  string  loc

            parameterValuePtr[12] = Marshal.StringToHGlobalAnsi(icaccount);  // #12  string  icaccount

            parameterValuePtr[13] = Marshal.StringToHGlobalAnsi(reg);  // #13  string  reg

            parameterValuePtr[14] = Marshal.StringToHGlobalAnsi(spoint);  // #14  string  spoint

            parameterValuePtr[15] = Marshal.StringToHGlobalAnsi(nots);  // #15  string  nots

            parameterValuePtr[16] = Marshal.StringToHGlobalAnsi(oprtyp);  // #16  string  oprtyp

            parameterValuePtr[17] = Marshal.StringToHGlobalAnsi(snumb);  // #17  string  snumb

            parameterValuePtr[18] = Marshal.AllocHGlobal(sizeof(short));  // #18  short  notwr
            Marshal.WriteInt16(parameterValuePtr[18], notwr);

            parameterValuePtr[19] = Marshal.AllocHGlobal(sizeof(int));  // #19  int  bandwd1
            Marshal.WriteInt32(parameterValuePtr[19], bandwd1);

            parameterValuePtr[20] = Marshal.AllocHGlobal(sizeof(int));  // #20  int  bandwd2
            Marshal.WriteInt32(parameterValuePtr[20], bandwd2);

            parameterValuePtr[21] = Marshal.AllocHGlobal(sizeof(int));  // #21  int  bandwd3
            Marshal.WriteInt32(parameterValuePtr[21], bandwd3);

            parameterValuePtr[22] = Marshal.AllocHGlobal(sizeof(int));  // #22  int  bandwd4
            Marshal.WriteInt32(parameterValuePtr[22], bandwd4);

            parameterValuePtr[23] = Marshal.AllocHGlobal(sizeof(int));  // #23  int  bandwd5
            Marshal.WriteInt32(parameterValuePtr[23], bandwd5);

            parameterValuePtr[24] = Marshal.AllocHGlobal(sizeof(int));  // #24  int  bandwd6
            Marshal.WriteInt32(parameterValuePtr[24], bandwd6);

            parameterValuePtr[25] = Marshal.AllocHGlobal(sizeof(int));  // #25  int  bandwd7
            Marshal.WriteInt32(parameterValuePtr[25], bandwd7);

            parameterValuePtr[26] = Marshal.AllocHGlobal(sizeof(int));  // #26  int  bandwd8
            Marshal.WriteInt32(parameterValuePtr[26], bandwd8);

            parameterValuePtr[27] = Marshal.StringToHGlobalAnsi(mdate);  // #27  string  mdate

            parameterValuePtr[28] = Marshal.StringToHGlobalAnsi(mtime);  // #28  string  mtime

            return parameterValuePtr;
        }

        /// <summary>
		/// This method supports testing by setting the fields of this object
        /// to predetermined reference values.
        /// </summary>
        /// <param name=""></param>
        public void SetAllColumnsToPrescribedTestValues()
        {
            cmd = "0";    //00
            recstat = "1";    //01
            call1 = "02";    //02
            name = "03";    //03
            prov = "04";    //04
            oper = "05";    //05
            latit = 6;    //06
            longit = 7;    //07
            grnd = 8.0F;    //08
            stats = "9";    //09
            sdate = "10";    //10
            loc = "11";    //11
            icaccount = "12";    //12
            reg = "13";    //13
            spoint = "14";    //14
            nots = "15";    //15
            oprtyp = "16";    //16
            snumb = "17";    //17
            notwr = 18;    //18
            bandwd1 = 19;    //19
            bandwd2 = 20;    //20
            bandwd3 = 21;    //21
            bandwd4 = 22;    //22
            bandwd5 = 23;    //23
            bandwd6 = 24;    //24
            bandwd7 = 25;    //25
            bandwd8 = 26;    //26
            mdate = "27";    //27
            mtime = "28";    //28
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
        /// This method returns an array containing the current values
        /// all of the bandwd* fields in this object.
        /// </summary>
        /// <returns></returns>
        public uint[] GetBandWdArray()
        {
            uint[] array = new uint[Constant.BANDWD_SZ];

            array[0] = (uint)bandwd1;
            array[1] = (uint)bandwd2;
            array[2] = (uint)bandwd3;
            array[3] = (uint)bandwd4;
            array[4] = (uint)bandwd5;
            array[5] = (uint)bandwd6;
            array[6] = (uint)bandwd7;
            array[7] = (uint)bandwd8;

            return array;
        }

        /// <summary>
        /// This method returns a BandBits object representing the current values
        /// all of the bandwd* fields of this FtSite object.
        /// </summary>
        /// <returns></returns>
        public BandBits GetBandBits()
        {
            uint[] array = GetBandWdArray();

            return new BandBits(array);
        }

        /// <summary>
        /// This method returns true if the first character of this 
        /// object's cmd field equals the prescribed character.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool CmdEquals(char c)
        {
            bool nRet = false;

            if (cmd != null && cmd.Length > 0)
            {
                if (cmd[0] == c)
                {
                    nRet = true;
                }
            }

            return nRet;
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

            sb.Append("\r\n");
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "cmd =       " + cmd);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "recstat =   " + recstat);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "call1 =     " + call1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "name =      " + name);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "prov =      " + prov);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "oper =      " + oper);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "latit =     " + latit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "longit =    " + longit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "grnd =      " + grnd);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "stats =     " + stats);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "sdate =     " + sdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "loc =       " + loc);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "icaccount = " + icaccount);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "reg =       " + reg);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "spoint =    " + spoint);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "nots =      " + nots);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "oprtyp =    " + oprtyp);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "snumb =     " + snumb);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "notwr =     " + notwr);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "bandwd1 =   " + bandwd1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "bandwd2 =   " + bandwd2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "bandwd3 =   " + bandwd3);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "bandwd4 =   " + bandwd4);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "bandwd5 =   " + bandwd5);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "bandwd6 =   " + bandwd6);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "bandwd7 =   " + bandwd7);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "bandwd8 =   " + bandwd8);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate =     " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime =     " + mtime);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated string for the key field 'chan1'.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string KeysToString()
        {
            string str = String.Format("call1 = {0}", call1);

            return str;
        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[{1}]" +
"(" +
    "[cmd] [char](1) NULL," +
    "[recstat] [char](1) NULL," +
    "[call1] [char](9) NOT NULL," +
    "[name] [char](32) NULL," +
    "[prov] [char](2) NULL," +
    "[oper] [char](6) NULL," +
    "[latit] [int] NULL," +
    "[longit] [int] NULL," +
    "[grnd] [real] NULL," +
    "[stats] [char](1) NULL," +
    "[sdate] [char](10) NULL," +
    "[loc] [char](25) NULL," +
    "[icaccount] [char](12) NULL," +
    "[reg] [char](2) NULL," +
    "[spoint] [char](4) NULL," +
    "[nots] [char](4) NULL," +
    "[oprtyp] [char](2) NULL," +
    "[snumb] [char](4) NULL," +
    "[notwr] [tinyint] NULL," +
    "[bandwd1] [int] NULL," +
    "[bandwd2] [int] NULL," +
    "[bandwd3] [int] NULL," +
    "[bandwd4] [int] NULL," +
    "[bandwd5] [int] NULL," +
    "[bandwd6] [int] NULL," +
    "[bandwd7] [int] NULL," +
    "[bandwd8] [int] NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime][char](8) NULL," +
    "CONSTRAINT[PK_FtTitl] PRIMARY KEY CLUSTERED " +
    "(" +
        "	[call1] ASC" +
    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON[PRIMARY]" +
") ON[PRIMARY]";


    }

}

```
