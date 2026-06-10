# Documented File: MtSite.cs
**Repository Path:** `_DataStructures\MtSite.cs`
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
    /// This class has fields that are isomorphic with the master table <b>main.mt_site</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    [Serializable]
    public class MtSite
    {
        // IMPORTANT!
        // =========
        // The following qty. 32 member values correspond to the columns
        // of an MtSite table stored in the database; for simplicity, the
        // member and column names are prescribed to be identical.
        // The order of appearance of these qty. 32 'column' members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALL1_SZ)]
        public string call1;    // #0
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NAME_SZ)]
        public string name;    // #1
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = PROV_SZ)]
        public string prov;    // #2
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = OPER_SZ)]
        public string oper;    // #3
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int latit;    // #4
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STRLATIT_SZ)]
        public string strlatit;    // #5
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STRLATITS_SZ)]
        public string strlatits;    // #6
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int longit;    // #7
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STRLONGIT_SZ)]
        public string strlongit;    // #8
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STRLONGITS_SZ)]
        public string strlongits;    // #9
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float grnd;    // #10
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STATS_SZ)]
        public string stats;    // #11
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SDATE_SZ)]
        public string sdate;    // #12
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = LOC_SZ)]
        public string loc;    // #13
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ICACCOUNT_SZ)]
        public string icaccount;    // #14
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = REG_SZ)]
        public string reg;    // #15
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SPOINT_SZ)]
        public string spoint;    // #16
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTS_SZ)]
        public string nots;    // #17
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = OPRTYP_SZ)]
        public string oprtyp;    // #18
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SNUMB_SZ)]
        public string snumb;    // #19
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short notwr;    // #20
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd1;    // #21
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd2;    // #22
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd3;    // #23
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd4;    // #24
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd5;    // #25
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd6;    // #26
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd7;    // #27
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int bandwd8;    // #28
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;    // #29
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;    // #30
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = USERID_SZ)]
        public string userid;    // #31

        // The following is a new field, introduced in 2020, to speed-up import processing
        // of ISED/TAFL data.
        // In the SQL database, the SiteCoords field is a 'computed' field.
        // Never attempt to SQL UPDATE the value of this field in a table record.
        // Never attempt to insert a new record into the mt_site table that prescribes
        // the value of this field.
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SITECOORDS_SZ)]
        public string SiteCoords;// #32


        //----------------------------------------------------------------
        //Additional public static members.

        // Constants that provide the field/column index from the field name.
        public const int CALL1 = 0;
        public const int NAME = 1;
        public const int PROV = 2;
        public const int OPER = 3;
        public const int LATIT = 4;
        public const int STRLATIT = 5;
        public const int STRLATITS = 6;
        public const int LONGIT = 7;
        public const int STRLONGIT = 8;
        public const int STRLONGITS = 9;
        public const int GRND = 10;
        public const int STATS = 11;
        public const int SDATE = 12;
        public const int LOC = 13;
        public const int ICACCOUNT = 14;
        public const int REG = 15;
        public const int SPOINT = 16;
        public const int NOTS = 17;
        public const int OPRTYP = 18;
        public const int SNUMB = 19;
        public const int NOTWR = 20;
        public const int BANDWD1 = 21;
        public const int BANDWD2 = 22;
        public const int BANDWD3 = 23;
        public const int BANDWD4 = 24;
        public const int BANDWD5 = 25;
        public const int BANDWD6 = 26;
        public const int BANDWD7 = 27;
        public const int BANDWD8 = 28;
        public const int MDATE = 29;
        public const int MTIME = 30;
        public const int USERID = 31;
        public const int SITECOORDS = 32;

        public const int CALL1_SZ = Constant.CALLSIGN_SZ;
        public const int NAME_SZ = Constant.MT_SITE_NAME_LEN;
        public const int PROV_SZ = Constant.PROV_SZ;
        public const int OPER_SZ = Constant.OPERCODE_SZ;
        public const int STRLATIT_SZ = Constant.LATITUDE_SZ;
        public const int STRLATITS_SZ = Constant.N_E_S_W_SZ;
        public const int STRLONGIT_SZ = Constant.LONGITUDE_SZ;
        public const int STRLONGITS_SZ = Constant.N_E_S_W_SZ;
        public const int STATS_SZ = Constant.STATS_SZ;
        public const int SDATE_SZ = Constant.DATE_SZ;
        public const int LOC_SZ = Constant.MT_SITE_LOC_LEN;
        public const int ICACCOUNT_SZ = Constant.ICACCOUNT_SZ;
        public const int REG_SZ = Constant.REG_SZ;
        public const int SPOINT_SZ = Constant.SPOINT_SZ;
        public const int NOTS_SZ = Constant.NOTS_SZ;
        public const int OPRTYP_SZ = Constant.OPRTYP_SZ;
        public const int SNUMB_SZ = Constant.SNUMB_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;
        public const int USERID_SZ = Constant.MICS_USERID_SZ;
        public const int SITECOORDS_SZ = Constant.SITECOORDS_SZ;

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 33;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "call1", "name", "prov", "oper", "latit", "strlatit", "strlatits", "longit", "strlongit", "strlongits", "grnd", "stats", "sdate", "loc", "icaccount", "reg", "spoint", "nots", "oprtyp", "snumb", "notwr", "bandwd1", "bandwd2", "bandwd3", "bandwd4", "bandwd5", "bandwd6", "bandwd7", "bandwd8", "mdate", "mtime", "userid", "SiteCoords" };

        //List of all column names for SQL 'select' command, i.e. " cmd, recstat, call1, call2, bndcde, anum, ... , mtime " 
        //Provide read-only access to this 'constant'.
        private static string mAllFieldsAsCSV;
        public static string AllColumnsForSqlSelect
        {
            get { return mAllFieldsAsCSV; }
        }

        //List of all column names for SQL 'select' command, i.e. " cmd, recstat, call1, call2, bndcde, anum, ... , mtime " 
        //Provide read-only access to this 'constant'.
        private static string mAllFieldsAsCSVexceptSiteCoords;
        public static string AllColumnsForSqlSelectexceptSiteCoords
        {
            get { return mAllFieldsAsCSVexceptSiteCoords; }
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
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static MtSite()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllFieldsAsCSVexceptSiteCoords = ListOfAllColumnNamesForSQLSelectExceptSiteCoords();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();

            SQLPOINTER[] ParameterValuePtr = Arrays.CreateArrayUsingDefaultElementConstructor<SQLPOINTER>(NUM_COLUMNS);
            SQLLENPTR[] StrLen_or_IndPtr = Arrays.CreateArrayUsingDefaultElementConstructor<SQLLENPTR>(NUM_COLUMNS);
        }

        /// <summary>
        /// Default object constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public MtSite()
        {
            //Set all column members to prescribed initial values.
            //Initialize();
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

            parameterValuePtr[0] = Marshal.StringToHGlobalAnsi(call1);  // #0  string  call1

            parameterValuePtr[1] = Marshal.StringToHGlobalAnsi(name);  // #1  string  name

            parameterValuePtr[2] = Marshal.StringToHGlobalAnsi(prov);  // #2  string  prov

            parameterValuePtr[3] = Marshal.StringToHGlobalAnsi(oper);  // #3  string  oper

            parameterValuePtr[4] = Marshal.AllocHGlobal(sizeof(int));  // #4  int  latit
            Marshal.WriteInt32(parameterValuePtr[4], latit);

            parameterValuePtr[5] = Marshal.StringToHGlobalAnsi(strlatit);  // #5  string  strlatit

            parameterValuePtr[6] = Marshal.StringToHGlobalAnsi(strlatits);  // #6  string  strlatits

            parameterValuePtr[7] = Marshal.AllocHGlobal(sizeof(int));  // #7  int  longit
            Marshal.WriteInt32(parameterValuePtr[7], longit);

            parameterValuePtr[8] = Marshal.StringToHGlobalAnsi(strlongit);  // #8  string  strlongit

            parameterValuePtr[9] = Marshal.StringToHGlobalAnsi(strlongits);  // #9  string  strlongits

            parameterValuePtr[10] = Marshal.AllocHGlobal(sizeof(float));  // #10  float  grnd
            F[0] = grnd;
            Marshal.Copy(F, 0, parameterValuePtr[10], 1);

            parameterValuePtr[11] = Marshal.StringToHGlobalAnsi(stats);  // #11  string  stats

            parameterValuePtr[12] = Marshal.StringToHGlobalAnsi(sdate);  // #12  string  sdate

            parameterValuePtr[13] = Marshal.StringToHGlobalAnsi(loc);  // #13  string  loc

            parameterValuePtr[14] = Marshal.StringToHGlobalAnsi(icaccount);  // #14  string  icaccount

            parameterValuePtr[15] = Marshal.StringToHGlobalAnsi(reg);  // #15  string  reg

            parameterValuePtr[16] = Marshal.StringToHGlobalAnsi(spoint);  // #16  string  spoint

            parameterValuePtr[17] = Marshal.StringToHGlobalAnsi(nots);  // #17  string  nots

            parameterValuePtr[18] = Marshal.StringToHGlobalAnsi(oprtyp);  // #18  string  oprtyp

            parameterValuePtr[19] = Marshal.StringToHGlobalAnsi(snumb);  // #19  string  snumb

            parameterValuePtr[20] = Marshal.AllocHGlobal(sizeof(short));  // #20  short  notwr
            Marshal.WriteInt16(parameterValuePtr[20], notwr);

            parameterValuePtr[21] = Marshal.AllocHGlobal(sizeof(int));  // #21  int  bandwd1
            Marshal.WriteInt32(parameterValuePtr[21], bandwd1);

            parameterValuePtr[22] = Marshal.AllocHGlobal(sizeof(int));  // #22  int  bandwd2
            Marshal.WriteInt32(parameterValuePtr[22], bandwd2);

            parameterValuePtr[23] = Marshal.AllocHGlobal(sizeof(int));  // #23  int  bandwd3
            Marshal.WriteInt32(parameterValuePtr[23], bandwd3);

            parameterValuePtr[24] = Marshal.AllocHGlobal(sizeof(int));  // #24  int  bandwd4
            Marshal.WriteInt32(parameterValuePtr[24], bandwd4);

            parameterValuePtr[25] = Marshal.AllocHGlobal(sizeof(int));  // #25  int  bandwd5
            Marshal.WriteInt32(parameterValuePtr[25], bandwd5);

            parameterValuePtr[26] = Marshal.AllocHGlobal(sizeof(int));  // #26  int  bandwd6
            Marshal.WriteInt32(parameterValuePtr[26], bandwd6);

            parameterValuePtr[27] = Marshal.AllocHGlobal(sizeof(int));  // #27  int  bandwd7
            Marshal.WriteInt32(parameterValuePtr[27], bandwd7);

            parameterValuePtr[28] = Marshal.AllocHGlobal(sizeof(int));  // #28  int  bandwd8
            Marshal.WriteInt32(parameterValuePtr[28], bandwd8);

            parameterValuePtr[29] = Marshal.StringToHGlobalAnsi(mdate);  // #29  string  mdate

            parameterValuePtr[30] = Marshal.StringToHGlobalAnsi(mtime);  // #30  string  mtime

            parameterValuePtr[31] = Marshal.StringToHGlobalAnsi(userid);  // #31  string  userid

            parameterValuePtr[32] = Marshal.StringToHGlobalAnsi(SiteCoords);  // #32  string  userid

            return parameterValuePtr;
        }

        /// <summary>
		/// This method supports testing by setting the fields of this object
        /// to predetermined reference values.
        /// </summary>
        /// <param name=""></param>
        public void SetAllColumnsToPrescribedTestValues()
        {
            call1 = "00";    //0
            name = "01";    //1
            prov = "02";    //2
            oper = "03";    //3
            latit = 4;    //4
            strlatit = "05";    //5
            strlatits = "06";    //6
            longit = 7;    //7
            strlongit = "08";    //8
            strlongits = "09";    //9
            grnd = 10.0F;    //10
            stats = "11";    //11
            sdate = "12";    //12
            loc = "13";    //13
            icaccount = "14";    //14
            reg = "15";    //15
            spoint = "16";    //16
            nots = "17";    //17
            oprtyp = "18";    //18
            snumb = "19";    //19
            notwr = 20;    //20
            bandwd1 = 21;    //21
            bandwd2 = 22;    //22
            bandwd3 = 23;    //23
            bandwd4 = 24;    //24
            bandwd5 = 25;    //25
            bandwd6 = 26;    //26
            bandwd7 = 27;    //27
            bandwd8 = 28;    //28
            mdate = "29";    //29
            mtime = "30";    //30
            userid = "31";    //31
            SiteCoords = "45-30-11.00-075-51-01.00";  //32
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
        /// of all column names for use in a SQL 'select' query.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
       private static string ListOfAllColumnNamesForSQLSelectExceptSiteCoords()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" ");
            for (int i = 0; i < NUM_COLUMNS - 1; i++)
            {
                sb.Append(columnNames[i]);
                if (i != NUM_COLUMNS - 2)
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

        //--------------------------------------------------------------------------------

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n===== MtSite =====");

            sb.AppendLine("call1 =      " + call1);
            sb.AppendLine("name =       " + name);
            sb.AppendLine("prov =       " + prov);
            sb.AppendLine("oper =       " + oper);
            sb.AppendLine("latit =      " + latit);
            sb.AppendLine("strlatit =   " + strlatit);
            sb.AppendLine("strlatits =  " + strlatits);
            sb.AppendLine("longit =     " + longit);
            sb.AppendLine("strlongit =  " + strlongit);
            sb.AppendLine("strlongits = " + strlongits);
            sb.AppendLine("grnd =       " + grnd);
            sb.AppendLine("stats =      " + stats);
            sb.AppendLine("sdate =      " + sdate);
            sb.AppendLine("loc =        " + loc);
            sb.AppendLine("icaccount =  " + icaccount);
            sb.AppendLine("reg =        " + reg);
            sb.AppendLine("spoint =     " + spoint);
            sb.AppendLine("nots =       " + nots);
            sb.AppendLine("oprtyp =     " + oprtyp);
            sb.AppendLine("snumb =      " + snumb);
            sb.AppendLine("notwr =      " + notwr);
            sb.AppendLine("bandwd1 =    " + bandwd1);
            sb.AppendLine("bandwd2 =    " + bandwd2);
            sb.AppendLine("bandwd3 =    " + bandwd3);
            sb.AppendLine("bandwd4 =    " + bandwd4);
            sb.AppendLine("bandwd5 =    " + bandwd5);
            sb.AppendLine("bandwd6 =    " + bandwd6);
            sb.AppendLine("bandwd7 =    " + bandwd7);
            sb.AppendLine("bandwd8 =    " + bandwd8);
            sb.AppendLine("mdate =      " + mdate);
            sb.AppendLine("mtime =      " + mtime);
            sb.AppendLine("userid =     " + userid);
            sb.AppendLine("SiteCoords = " + SiteCoords);
            return sb.ToString();
        }


        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned MtSite object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="mtSite"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out MtSite mtSite, out SQLLEN[] nullInds)
        {
            mtSite = new MtSite();
            nullInds = NullHelper.CreateArrayOfNullInd(MtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[CALL1]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.call1 = "";
                nullInds[CALL1] = Constant.DB_NULL;
            }
            else
            {
                mtSite.call1 = Marshal.PtrToStringAnsi(tgtValPtrs[CALL1]).Trim();
                nullInds[CALL1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[NAME]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.name = "";
                nullInds[NAME] = Constant.DB_NULL;
            }
            else
            {
                mtSite.name = Marshal.PtrToStringAnsi(tgtValPtrs[NAME]).Trim();
                nullInds[NAME] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[PROV]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.prov = "";
                nullInds[PROV] = Constant.DB_NULL;
            }
            else
            {
                mtSite.prov = Marshal.PtrToStringAnsi(tgtValPtrs[PROV]).Trim();
                nullInds[PROV] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[OPER]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.oper = "";
                nullInds[OPER] = Constant.DB_NULL;
            }
            else
            {
                mtSite.oper = Marshal.PtrToStringAnsi(tgtValPtrs[OPER]).Trim();
                nullInds[OPER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LATIT]);
            if (nullInd != Constant.DB_NULL)
            {
                mtSite.latit = Marshal.ReadInt32(tgtValPtrs[LATIT]);
                nullInds[LATIT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[STRLATIT]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.strlatit = "";
                nullInds[STRLATIT] = Constant.DB_NULL;
            }
            else
            {
                mtSite.strlatit = Marshal.PtrToStringAnsi(tgtValPtrs[STRLATIT]).Trim();
                nullInds[STRLATIT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[STRLATITS]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.strlatits = "";
                nullInds[STRLATITS] = Constant.DB_NULL;
            }
            else
            {
                mtSite.strlatits = Marshal.PtrToStringAnsi(tgtValPtrs[STRLATITS]).Trim();
                nullInds[STRLATITS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LONGIT]);
            if (nullInd != Constant.DB_NULL)
            {
                mtSite.longit = Marshal.ReadInt32(tgtValPtrs[LONGIT]);
                nullInds[LONGIT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[STRLONGIT]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.strlongit = "";
                nullInds[STRLONGIT] = Constant.DB_NULL;
            }
            else
            {
                mtSite.strlongit = Marshal.PtrToStringAnsi(tgtValPtrs[STRLONGIT]).Trim();
                nullInds[STRLONGIT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[STRLONGITS]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.strlongits = "";
                nullInds[STRLONGITS] = Constant.DB_NULL;
            }
            else
            {
                mtSite.strlongits = Marshal.PtrToStringAnsi(tgtValPtrs[STRLONGITS]).Trim();
                nullInds[STRLONGITS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[GRND]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[GRND], F, 0, 1);
                mtSite.grnd = F[0];
                nullInds[GRND] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[STATS]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.stats = "";
                nullInds[STATS] = Constant.DB_NULL;
            }
            else
            {
                mtSite.stats = Marshal.PtrToStringAnsi(tgtValPtrs[STATS]).Trim();
                nullInds[STATS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.sdate = "";
                nullInds[SDATE] = Constant.DB_NULL;
            }
            else
            {
                mtSite.sdate = Marshal.PtrToStringAnsi(tgtValPtrs[SDATE]).Trim();
                nullInds[SDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LOC]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.loc = "";
                nullInds[LOC] = Constant.DB_NULL;
            }
            else
            {
                mtSite.loc = Marshal.PtrToStringAnsi(tgtValPtrs[LOC]).Trim();
                nullInds[LOC] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ICACCOUNT]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.icaccount = "";
                nullInds[ICACCOUNT] = Constant.DB_NULL;
            }
            else
            {
                mtSite.icaccount = Marshal.PtrToStringAnsi(tgtValPtrs[ICACCOUNT]).Trim();
                nullInds[ICACCOUNT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[REG]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.reg = "";
                nullInds[REG] = Constant.DB_NULL;
            }
            else
            {
                mtSite.reg = Marshal.PtrToStringAnsi(tgtValPtrs[REG]).Trim();
                nullInds[REG] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SPOINT]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.spoint = "";
                nullInds[SPOINT] = Constant.DB_NULL;
            }
            else
            {
                mtSite.spoint = Marshal.PtrToStringAnsi(tgtValPtrs[SPOINT]).Trim();
                nullInds[SPOINT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[NOTS]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.nots = "";
                nullInds[NOTS] = Constant.DB_NULL;
            }
            else
            {
                mtSite.nots = Marshal.PtrToStringAnsi(tgtValPtrs[NOTS]).Trim();
                nullInds[NOTS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[OPRTYP]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.oprtyp = "";
                nullInds[OPRTYP] = Constant.DB_NULL;
            }
            else
            {
                mtSite.oprtyp = Marshal.PtrToStringAnsi(tgtValPtrs[OPRTYP]).Trim();
                nullInds[OPRTYP] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SNUMB]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.snumb = "";
                nullInds[SNUMB] = Constant.DB_NULL;
            }
            else
            {
                mtSite.snumb = Marshal.PtrToStringAnsi(tgtValPtrs[SNUMB]).Trim();
                nullInds[SNUMB] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[NOTWR]);
            if (nullInd != Constant.DB_NULL)
            {
                mtSite.notwr = Marshal.ReadInt16(tgtValPtrs[NOTWR]);
                nullInds[NOTWR] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BANDWD1]);
            if (nullInd != Constant.DB_NULL)
            {
                mtSite.bandwd1 = Marshal.ReadInt32(tgtValPtrs[BANDWD1]);
                nullInds[BANDWD1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BANDWD2]);
            if (nullInd != Constant.DB_NULL)
            {
                mtSite.bandwd2 = Marshal.ReadInt32(tgtValPtrs[BANDWD2]);
                nullInds[BANDWD2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BANDWD3]);
            if (nullInd != Constant.DB_NULL)
            {
                mtSite.bandwd3 = Marshal.ReadInt32(tgtValPtrs[BANDWD3]);
                nullInds[BANDWD3] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BANDWD4]);
            if (nullInd != Constant.DB_NULL)
            {
                mtSite.bandwd4 = Marshal.ReadInt32(tgtValPtrs[BANDWD4]);
                nullInds[BANDWD4] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BANDWD5]);
            if (nullInd != Constant.DB_NULL)
            {
                mtSite.bandwd5 = Marshal.ReadInt32(tgtValPtrs[BANDWD5]);
                nullInds[BANDWD5] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BANDWD6]);
            if (nullInd != Constant.DB_NULL)
            {
                mtSite.bandwd6 = Marshal.ReadInt32(tgtValPtrs[BANDWD6]);
                nullInds[BANDWD6] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BANDWD7]);
            if (nullInd != Constant.DB_NULL)
            {
                mtSite.bandwd7 = Marshal.ReadInt32(tgtValPtrs[BANDWD7]);
                nullInds[BANDWD7] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BANDWD8]);
            if (nullInd != Constant.DB_NULL)
            {
                mtSite.bandwd8 = Marshal.ReadInt32(tgtValPtrs[BANDWD8]);
                nullInds[BANDWD8] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                mtSite.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                mtSite.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[USERID]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.userid = "";
                nullInds[USERID] = Constant.DB_NULL;
            }
            else
            {
                mtSite.userid = Marshal.PtrToStringAnsi(tgtValPtrs[USERID]).Trim();
                nullInds[USERID] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SITECOORDS]);
            if (nullInd == Constant.DB_NULL)
            {
                mtSite.SiteCoords = "";
                nullInds[SITECOORDS] = Constant.DB_NULL;
            }
            else
            {
                mtSite.SiteCoords = Marshal.PtrToStringAnsi(tgtValPtrs[SITECOORDS]).Trim();
                nullInds[SITECOORDS] = Constant.DB_NOT_NULL;
            }

        }

        /// <summary>
        /// This method returns
        /// an array of IntPtr that point to the start addresses of non-contiguous blocks of global
        /// (heap) memory, each of a sufficient size to hold the value of a specific member field 
        /// of this object. Similarly for the nullInd array associated with this object.
        /// Thus, this method creates the parameter bindings prior to an SQL / ODBC 'update' or 
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
            Bind.BindBufferToString(hStmt, CALL1 + 1, tgtValPtrs[CALL1], MtSite.CALL1_SZ, nullIndPtrs[CALL1]);

            tgtValPtrs[NAME] = Marshal.AllocHGlobal(NAME_SZ + 1);
            nullIndPtrs[NAME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, NAME + 1, tgtValPtrs[NAME], MtSite.NAME_SZ, nullIndPtrs[NAME]);

            tgtValPtrs[PROV] = Marshal.AllocHGlobal(PROV_SZ + 1);
            nullIndPtrs[PROV] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, PROV + 1, tgtValPtrs[PROV], MtSite.PROV_SZ, nullIndPtrs[PROV]);

            tgtValPtrs[OPER] = Marshal.AllocHGlobal(OPER_SZ + 1);
            nullIndPtrs[OPER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, OPER + 1, tgtValPtrs[OPER], MtSite.OPER_SZ, nullIndPtrs[OPER]);

            tgtValPtrs[LATIT] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[LATIT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, LATIT + 1, tgtValPtrs[LATIT], nullIndPtrs[LATIT]);

            tgtValPtrs[STRLATIT] = Marshal.AllocHGlobal(STRLATIT_SZ + 1);
            nullIndPtrs[STRLATIT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, STRLATIT + 1, tgtValPtrs[STRLATIT], MtSite.STRLATIT_SZ, nullIndPtrs[STRLATIT]);

            tgtValPtrs[STRLATITS] = Marshal.AllocHGlobal(STRLATITS_SZ + 1);
            nullIndPtrs[STRLATITS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, STRLATITS + 1, tgtValPtrs[STRLATITS], MtSite.STRLATITS_SZ, nullIndPtrs[STRLATITS]);

            tgtValPtrs[LONGIT] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[LONGIT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, LONGIT + 1, tgtValPtrs[LONGIT], nullIndPtrs[LONGIT]);

            tgtValPtrs[STRLONGIT] = Marshal.AllocHGlobal(STRLONGIT_SZ + 1);
            nullIndPtrs[STRLONGIT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, STRLONGIT + 1, tgtValPtrs[STRLONGIT], MtSite.STRLONGIT_SZ, nullIndPtrs[STRLONGIT]);

            tgtValPtrs[STRLONGITS] = Marshal.AllocHGlobal(STRLONGITS_SZ + 1);
            nullIndPtrs[STRLONGITS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, STRLONGITS + 1, tgtValPtrs[STRLONGITS], MtSite.STRLONGITS_SZ, nullIndPtrs[STRLONGITS]);

            tgtValPtrs[GRND] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[GRND] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, GRND + 1, tgtValPtrs[GRND], nullIndPtrs[GRND]);

            tgtValPtrs[STATS] = Marshal.AllocHGlobal(STATS_SZ + 1);
            nullIndPtrs[STATS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, STATS + 1, tgtValPtrs[STATS], MtSite.STATS_SZ, nullIndPtrs[STATS]);

            tgtValPtrs[SDATE] = Marshal.AllocHGlobal(SDATE_SZ + 1);
            nullIndPtrs[SDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SDATE + 1, tgtValPtrs[SDATE], MtSite.SDATE_SZ, nullIndPtrs[SDATE]);

            tgtValPtrs[LOC] = Marshal.AllocHGlobal(LOC_SZ + 1);
            nullIndPtrs[LOC] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, LOC + 1, tgtValPtrs[LOC], MtSite.LOC_SZ, nullIndPtrs[LOC]);

            tgtValPtrs[ICACCOUNT] = Marshal.AllocHGlobal(ICACCOUNT_SZ + 1);
            nullIndPtrs[ICACCOUNT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ICACCOUNT + 1, tgtValPtrs[ICACCOUNT], MtSite.ICACCOUNT_SZ, nullIndPtrs[ICACCOUNT]);

            tgtValPtrs[REG] = Marshal.AllocHGlobal(REG_SZ + 1);
            nullIndPtrs[REG] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, REG + 1, tgtValPtrs[REG], MtSite.REG_SZ, nullIndPtrs[REG]);

            tgtValPtrs[SPOINT] = Marshal.AllocHGlobal(SPOINT_SZ + 1);
            nullIndPtrs[SPOINT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SPOINT + 1, tgtValPtrs[SPOINT], MtSite.SPOINT_SZ, nullIndPtrs[SPOINT]);

            tgtValPtrs[NOTS] = Marshal.AllocHGlobal(NOTS_SZ + 1);
            nullIndPtrs[NOTS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, NOTS + 1, tgtValPtrs[NOTS], MtSite.NOTS_SZ, nullIndPtrs[NOTS]);

            tgtValPtrs[OPRTYP] = Marshal.AllocHGlobal(OPRTYP_SZ + 1);
            nullIndPtrs[OPRTYP] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, OPRTYP + 1, tgtValPtrs[OPRTYP], MtSite.OPRTYP_SZ, nullIndPtrs[OPRTYP]);

            tgtValPtrs[SNUMB] = Marshal.AllocHGlobal(SNUMB_SZ + 1);
            nullIndPtrs[SNUMB] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SNUMB + 1, tgtValPtrs[SNUMB], MtSite.SNUMB_SZ, nullIndPtrs[SNUMB]);

            tgtValPtrs[NOTWR] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[NOTWR] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, NOTWR + 1, tgtValPtrs[NOTWR], nullIndPtrs[NOTWR]);

            tgtValPtrs[BANDWD1] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[BANDWD1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, BANDWD1 + 1, tgtValPtrs[BANDWD1], nullIndPtrs[BANDWD1]);

            tgtValPtrs[BANDWD2] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[BANDWD2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, BANDWD2 + 1, tgtValPtrs[BANDWD2], nullIndPtrs[BANDWD2]);

            tgtValPtrs[BANDWD3] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[BANDWD3] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, BANDWD3 + 1, tgtValPtrs[BANDWD3], nullIndPtrs[BANDWD3]);

            tgtValPtrs[BANDWD4] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[BANDWD4] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, BANDWD4 + 1, tgtValPtrs[BANDWD4], nullIndPtrs[BANDWD4]);

            tgtValPtrs[BANDWD5] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[BANDWD5] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, BANDWD5 + 1, tgtValPtrs[BANDWD5], nullIndPtrs[BANDWD5]);

            tgtValPtrs[BANDWD6] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[BANDWD6] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, BANDWD6 + 1, tgtValPtrs[BANDWD6], nullIndPtrs[BANDWD6]);

            tgtValPtrs[BANDWD7] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[BANDWD7] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, BANDWD7 + 1, tgtValPtrs[BANDWD7], nullIndPtrs[BANDWD7]);

            tgtValPtrs[BANDWD8] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[BANDWD8] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, BANDWD8 + 1, tgtValPtrs[BANDWD8], nullIndPtrs[BANDWD8]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], MtSite.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], MtSite.MTIME_SZ, nullIndPtrs[MTIME]);

            tgtValPtrs[USERID] = Marshal.AllocHGlobal(USERID_SZ + 1);
            nullIndPtrs[USERID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, USERID + 1, tgtValPtrs[USERID], MtSite.USERID_SZ, nullIndPtrs[USERID]);

            tgtValPtrs[SITECOORDS] = Marshal.AllocHGlobal(SITECOORDS_SZ + 1);
            nullIndPtrs[SITECOORDS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SITECOORDS + 1, tgtValPtrs[SITECOORDS], MtSite.SITECOORDS_SZ, nullIndPtrs[SITECOORDS]);
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
            sb.Append("\r\n===== MtSite =====");

            sb.Append("\n" + nullInds[i++] + "      " + "call1 = " + call1);
            sb.Append("\n" + nullInds[i++] + "      " + "name = " + name);
            sb.Append("\n" + nullInds[i++] + "      " + "prov = " + prov);
            sb.Append("\n" + nullInds[i++] + "      " + "oper = " + oper);
            sb.Append("\n" + nullInds[i++] + "      " + "latit = " + latit);
            sb.Append("\n" + nullInds[i++] + "      " + "strlatit = " + strlatit);
            sb.Append("\n" + nullInds[i++] + "      " + "strlatits = " + strlatits);
            sb.Append("\n" + nullInds[i++] + "      " + "longit = " + longit);
            sb.Append("\n" + nullInds[i++] + "      " + "strlongit = " + strlongit);
            sb.Append("\n" + nullInds[i++] + "      " + "strlongits = " + strlongits);
            sb.Append("\n" + nullInds[i++] + "      " + "grnd = " + grnd);
            sb.Append("\n" + nullInds[i++] + "      " + "stats = " + stats);
            sb.Append("\n" + nullInds[i++] + "      " + "sdate = " + sdate);
            sb.Append("\n" + nullInds[i++] + "      " + "loc = " + loc);
            sb.Append("\n" + nullInds[i++] + "      " + "icaccount = " + icaccount);
            sb.Append("\n" + nullInds[i++] + "      " + "reg = " + reg);
            sb.Append("\n" + nullInds[i++] + "      " + "spoint = " + spoint);
            sb.Append("\n" + nullInds[i++] + "      " + "nots = " + nots);
            sb.Append("\n" + nullInds[i++] + "      " + "oprtyp = " + oprtyp);
            sb.Append("\n" + nullInds[i++] + "      " + "snumb = " + snumb);
            sb.Append("\n" + nullInds[i++] + "      " + "notwr = " + notwr);
            sb.Append("\n" + nullInds[i++] + "      " + "bandwd1 = " + bandwd1);
            sb.Append("\n" + nullInds[i++] + "      " + "bandwd2 = " + bandwd2);
            sb.Append("\n" + nullInds[i++] + "      " + "bandwd3 = " + bandwd3);
            sb.Append("\n" + nullInds[i++] + "      " + "bandwd4 = " + bandwd4);
            sb.Append("\n" + nullInds[i++] + "      " + "bandwd5 = " + bandwd5);
            sb.Append("\n" + nullInds[i++] + "      " + "bandwd6 = " + bandwd6);
            sb.Append("\n" + nullInds[i++] + "      " + "bandwd7 = " + bandwd7);
            sb.Append("\n" + nullInds[i++] + "      " + "bandwd8 = " + bandwd8);
            sb.Append("\n" + nullInds[i++] + "      " + "mdate = " + mdate);
            sb.Append("\n" + nullInds[i++] + "      " + "mtime = " + mtime);
            sb.Append("\n" + nullInds[i++] + "      " + "userid = " + userid);
            sb.Append("\n" + nullInds[i++] + "      " + "SiteCoords = " + SiteCoords);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns the member values of an object formatted
        /// as a CSV string that can be used to provide the VALUES in an SQL INSERT query; 
        /// note that the 'SiteCoords' field is excluded from the CSV list because it is
        /// a 'computed' field in the database.
        /// </summary>
        /// <param name="nullInds"> - array of ODBC null indicators.</param>
        /// <returns></returns>
        public string ToStringAsCSV(SQLLEN[] nullInds)
        {
            // The field 'name' could contain single quotes that will cause an SQL error.
            // The solution is to 'escape' the single quote by 'doubling up',
            // e.g. O'Brian becomes O''Brian.
            string nameEscaped = (String.IsNullOrWhiteSpace(name)) ? "" : name.Replace("'", "''");

            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[MtSite.CALL1] == Constant.DB_NULL ? "NULL," : "'" + call1.ToString() + "',");
            sb.Append(nullInds[MtSite.NAME] == Constant.DB_NULL ? "NULL," : "'" + nameEscaped + "',");
            sb.Append(nullInds[MtSite.PROV] == Constant.DB_NULL ? "NULL," : "'" + prov.ToString() + "',");
            sb.Append(nullInds[MtSite.OPER] == Constant.DB_NULL ? "NULL," : "'" + oper.ToString() + "',");
            sb.Append(nullInds[MtSite.LATIT] == Constant.DB_NULL ? "NULL," : "'" + latit.ToString() + "',");
            sb.Append(nullInds[MtSite.STRLATIT] == Constant.DB_NULL ? "NULL," : "'" + strlatit.ToString() + "',");
            sb.Append(nullInds[MtSite.STRLATITS] == Constant.DB_NULL ? "NULL," : "'" + strlatits.ToString() + "',");
            sb.Append(nullInds[MtSite.LONGIT] == Constant.DB_NULL ? "NULL," : "'" + longit.ToString() + "',");
            sb.Append(nullInds[MtSite.STRLONGIT] == Constant.DB_NULL ? "NULL," : "'" + strlongit.ToString() + "',");
            sb.Append(nullInds[MtSite.STRLONGITS] == Constant.DB_NULL ? "NULL," : "'" + strlongits.ToString() + "',");
            sb.Append(nullInds[MtSite.GRND] == Constant.DB_NULL ? "NULL," : "'" + grnd.ToString() + "',");
            sb.Append(nullInds[MtSite.STATS] == Constant.DB_NULL ? "NULL," : "'" + stats.ToString() + "',");
            sb.Append(nullInds[MtSite.SDATE] == Constant.DB_NULL ? "NULL," : "'" + sdate.ToString() + "',");
            sb.Append(nullInds[MtSite.LOC] == Constant.DB_NULL ? "NULL," : "'" + loc.ToString() + "',");
            sb.Append(nullInds[MtSite.ICACCOUNT] == Constant.DB_NULL ? "NULL," : "'" + icaccount.ToString() + "',");
            sb.Append(nullInds[MtSite.REG] == Constant.DB_NULL ? "NULL," : "'" + reg.ToString() + "',");
            sb.Append(nullInds[MtSite.SPOINT] == Constant.DB_NULL ? "NULL," : "'" + spoint.ToString() + "',");
            sb.Append(nullInds[MtSite.NOTS] == Constant.DB_NULL ? "NULL," : "'" + nots.ToString() + "',");
            sb.Append(nullInds[MtSite.OPRTYP] == Constant.DB_NULL ? "NULL," : "'" + oprtyp.ToString() + "',");
            sb.Append(nullInds[MtSite.SNUMB] == Constant.DB_NULL ? "NULL," : "'" + snumb.ToString() + "',");
            sb.Append(nullInds[MtSite.NOTWR] == Constant.DB_NULL ? "NULL," : "'" + notwr.ToString() + "',");
            sb.Append(nullInds[MtSite.BANDWD1] == Constant.DB_NULL ? "NULL," : "'" + bandwd1.ToString() + "',");
            sb.Append(nullInds[MtSite.BANDWD2] == Constant.DB_NULL ? "NULL," : "'" + bandwd2.ToString() + "',");
            sb.Append(nullInds[MtSite.BANDWD3] == Constant.DB_NULL ? "NULL," : "'" + bandwd3.ToString() + "',");
            sb.Append(nullInds[MtSite.BANDWD4] == Constant.DB_NULL ? "NULL," : "'" + bandwd4.ToString() + "',");
            sb.Append(nullInds[MtSite.BANDWD5] == Constant.DB_NULL ? "NULL," : "'" + bandwd5.ToString() + "',");
            sb.Append(nullInds[MtSite.BANDWD6] == Constant.DB_NULL ? "NULL," : "'" + bandwd6.ToString() + "',");
            sb.Append(nullInds[MtSite.BANDWD7] == Constant.DB_NULL ? "NULL," : "'" + bandwd7.ToString() + "',");
            sb.Append(nullInds[MtSite.BANDWD8] == Constant.DB_NULL ? "NULL," : "'" + bandwd8.ToString() + "',");
            sb.Append(nullInds[MtSite.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',");
            sb.Append(nullInds[MtSite.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "',");
            sb.Append(nullInds[MtSite.USERID] == Constant.DB_NULL ? "NULL" : "'" + userid.ToString() + "'");

            //sb.Append(nullInds[MtSite.SITECOORDS] == Constant.DB_NULL ? "NULL" : "'" + SiteCoords.ToString() + "'");

            return sb.ToString();
        }


        /// <summary>
        /// This method returns the member values of an object formatted
        /// as a string of columnName = 'value' expressions that can be used
        /// in the SET clause of an SQL UPDATE query ; 
        /// note that the 'SiteCoords' field is excluded from the CSV list because it is
        /// a 'computed' field in the database.
        /// </summary>
        /// <param name="nullInds"> - array of ODBC null indicators.</param>
        /// <returns></returns>
        public string ToStringAsCSVequates(SQLLEN[] nullInds)
        {
            // The field 'name' could contain single quotes that will cause an SQL error.
            // The solution is to 'escape' the single quote by 'doubling up',
            // e.g. O'Brian becomes O''Brian.
            string nameEscaped = name.Replace("'", "''");

            StringBuilder sb = new StringBuilder();

            sb.Append(columnNames[MtSite.CALL1] + "=" + (nullInds[MtSite.CALL1] == Constant.DB_NULL ? "NULL," : "'" + call1.ToString() + "',"));
            sb.Append(columnNames[MtSite.NAME] + "=" + (nullInds[MtSite.NAME] == Constant.DB_NULL ? "NULL," : "'" + nameEscaped + "',"));
            sb.Append(columnNames[MtSite.PROV] + "=" + (nullInds[MtSite.PROV] == Constant.DB_NULL ? "NULL," : "'" + prov.ToString() + "',"));
            sb.Append(columnNames[MtSite.OPER] + "=" + (nullInds[MtSite.OPER] == Constant.DB_NULL ? "NULL," : "'" + oper.ToString() + "',"));
            sb.Append(columnNames[MtSite.LATIT] + "=" + (nullInds[MtSite.LATIT] == Constant.DB_NULL ? "NULL," : "'" + latit.ToString() + "',"));
            sb.Append(columnNames[MtSite.STRLATIT] + "=" + (nullInds[MtSite.STRLATIT] == Constant.DB_NULL ? "NULL," : "'" + strlatit.ToString() + "',"));
            sb.Append(columnNames[MtSite.STRLATITS] + "=" + (nullInds[MtSite.STRLATITS] == Constant.DB_NULL ? "NULL," : "'" + strlatits.ToString() + "',"));
            sb.Append(columnNames[MtSite.LONGIT] + "=" + (nullInds[MtSite.LONGIT] == Constant.DB_NULL ? "NULL," : "'" + longit.ToString() + "',"));
            sb.Append(columnNames[MtSite.STRLONGIT] + "=" + (nullInds[MtSite.STRLONGIT] == Constant.DB_NULL ? "NULL," : "'" + strlongit.ToString() + "',"));
            sb.Append(columnNames[MtSite.STRLONGITS] + "=" + (nullInds[MtSite.STRLONGITS] == Constant.DB_NULL ? "NULL," : "'" + strlongits.ToString() + "',"));
            sb.Append(columnNames[MtSite.GRND] + "=" + (nullInds[MtSite.GRND] == Constant.DB_NULL ? "NULL," : "'" + grnd.ToString() + "',"));
            sb.Append(columnNames[MtSite.STATS] + "=" + (nullInds[MtSite.STATS] == Constant.DB_NULL ? "NULL," : "'" + stats.ToString() + "',"));
            sb.Append(columnNames[MtSite.SDATE] + "=" + (nullInds[MtSite.SDATE] == Constant.DB_NULL ? "NULL," : "'" + sdate.ToString() + "',"));
            sb.Append(columnNames[MtSite.LOC] + "=" + (nullInds[MtSite.LOC] == Constant.DB_NULL ? "NULL," : "'" + loc.ToString() + "',"));
            sb.Append(columnNames[MtSite.ICACCOUNT] + "=" + (nullInds[MtSite.ICACCOUNT] == Constant.DB_NULL ? "NULL," : "'" + icaccount.ToString() + "',"));
            sb.Append(columnNames[MtSite.REG] + "=" + (nullInds[MtSite.REG] == Constant.DB_NULL ? "NULL," : "'" + reg.ToString() + "',"));
            sb.Append(columnNames[MtSite.SPOINT] + "=" + (nullInds[MtSite.SPOINT] == Constant.DB_NULL ? "NULL," : "'" + spoint.ToString() + "',"));
            sb.Append(columnNames[MtSite.NOTS] + "=" + (nullInds[MtSite.NOTS] == Constant.DB_NULL ? "NULL," : "'" + nots.ToString() + "',"));
            sb.Append(columnNames[MtSite.OPRTYP] + "=" + (nullInds[MtSite.OPRTYP] == Constant.DB_NULL ? "NULL," : "'" + oprtyp.ToString() + "',"));
            sb.Append(columnNames[MtSite.SNUMB] + "=" + (nullInds[MtSite.SNUMB] == Constant.DB_NULL ? "NULL," : "'" + snumb.ToString() + "',"));
            sb.Append(columnNames[MtSite.NOTWR] + "=" + (nullInds[MtSite.NOTWR] == Constant.DB_NULL ? "NULL," : "'" + notwr.ToString() + "',"));
            sb.Append(columnNames[MtSite.BANDWD1] + "=" + (nullInds[MtSite.BANDWD1] == Constant.DB_NULL ? "NULL," : "'" + bandwd1.ToString() + "',"));
            sb.Append(columnNames[MtSite.BANDWD2] + "=" + (nullInds[MtSite.BANDWD2] == Constant.DB_NULL ? "NULL," : "'" + bandwd2.ToString() + "',"));
            sb.Append(columnNames[MtSite.BANDWD3] + "=" + (nullInds[MtSite.BANDWD3] == Constant.DB_NULL ? "NULL," : "'" + bandwd3.ToString() + "',"));
            sb.Append(columnNames[MtSite.BANDWD4] + "=" + (nullInds[MtSite.BANDWD4] == Constant.DB_NULL ? "NULL," : "'" + bandwd4.ToString() + "',"));
            sb.Append(columnNames[MtSite.BANDWD5] + "=" + (nullInds[MtSite.BANDWD5] == Constant.DB_NULL ? "NULL," : "'" + bandwd5.ToString() + "',"));
            sb.Append(columnNames[MtSite.BANDWD6] + "=" + (nullInds[MtSite.BANDWD6] == Constant.DB_NULL ? "NULL," : "'" + bandwd6.ToString() + "',"));
            sb.Append(columnNames[MtSite.BANDWD7] + "=" + (nullInds[MtSite.BANDWD7] == Constant.DB_NULL ? "NULL," : "'" + bandwd7.ToString() + "',"));
            sb.Append(columnNames[MtSite.BANDWD8] + "=" + (nullInds[MtSite.BANDWD8] == Constant.DB_NULL ? "NULL," : "'" + bandwd8.ToString() + "',"));
            sb.Append(columnNames[MtSite.MDATE] + "=" + (nullInds[MtSite.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',"));
            sb.Append(columnNames[MtSite.MTIME] + "=" + (nullInds[MtSite.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "',"));
            sb.Append(columnNames[MtSite.USERID] + "=" + (nullInds[MtSite.USERID] == Constant.DB_NULL ? "NULL" : "'" + userid.ToString() + "'"));

            //sb.Append(columnNames[MtSite.SITECOORDS] + "=" + (nullInds[MtSite.SITECOORDS] == Constant.DB_NULL ? "NULL" : "'" + SiteCoords.ToString() + "'"));

            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated string for the key field 'call1'.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string KeysToString()
        {
            string str = String.Format("call1 = {0}", call1);

            return str;
        }

        /// <summary>
        /// This method returns a GeoPoint object corresponding to the (lat, long) location
        /// of this MtSite object.
        /// </summary>
        /// <returns></returns>
        public GeoPoint PositionAsGeoPoint()
        {
            double lat;
            double lng;

            // Latitude.
            lat = latit * Constant.CENTISECONDS_TO_DEGREES;
            if (strlatits == "S") lat = -lat;

            // Longitude.
            lng = longit * Constant.CENTISECONDS_TO_DEGREES;
            if (strlongits == "W") lng = -lng;

            return new GeoPoint(lat, lng);
        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[mt_site](" +
    "[call1] [char](9) NOT NULL," +
    "[name] [char](32) NULL," +
    "[prov] [char](2) NULL," +
    "[oper] [char](6) NULL," +
    "[latit] [int] NULL," +
    "[strlatit] [char](11) NULL," +
    "[strlatits] [char](1) NULL," +
    "[longit] [int] NULL," +
    "[strlongit] [char](12) NULL," +
    "[strlongits] [char](1) NULL," +
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
    "[mtime] [char](8) NULL," +
    "[userid] [char](12) NULL," +
    "[SiteCoords]  AS (([strlatit]+'-')+[strlongit])," +
"CONSTRAINT [PK_mt_site] PRIMARY KEY CLUSTERED " +
"(" +
    "[call1] ASC" +
")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";


    }
}

```
