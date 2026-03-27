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
    /// This class has fields that are isomorphic with TS PDF table <b>&lt;userID&gt;.ft_&lt;pdfName&gt;_chan</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FtChan
    {
        // IMPORTANT!
        // =========
        // The following qty. 52 member values correspond to the columns
        // of an FtChan table stored in the database; for simplicity, the
        // member and column names are prescribed to be identical.
        // The order of appearance of these qty. 52 'column' members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CMD_SZ)]
        public string cmd;  //0
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RECSTAT_SZ)]
        public string recstat;  //1
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALL1_SZ)]
        public string call1;  //2
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALL2_SZ)]
        public string call2;  //3
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = BNDCDE_SZ)]
        public string bndcde;  //4
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SPLAN_SZ)]
        public string splan;  //5
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short hl;  //6
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short vh;  //7
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CHID_SZ)]
        public string chid;  //8
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double freqtx;  //9
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = POLTX_SZ)]
        public string poltx;  //10
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short antnumbtx1;  //11
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short antnumbtx2;  //12
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EQPTTX_SZ)]
        public string eqpttx;  //13
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EQPTUTX_SZ)]
        public string eqptutx;  //14
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float pwrtx;  //15
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float atpccde;  //16
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float afsltx1;  //17
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float afsltx2;  //18
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TRAFTX_SZ)]
        public string traftx;  //19
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SRVCTX_SZ)]
        public string srvctx;  //20
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STATTX_SZ)]
        public string stattx;  //21
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double freqrx;  //22
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = POLRX_SZ)]
        public string polrx;  //23
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short antnumbrx1;  //24
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short antnumbrx2;  //25
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short antnumbrx3;  //26
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EQPTRX_SZ)]
        public string eqptrx;  //27
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EQPTURX_SZ)]
        public string eqpturx;  //28
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float afslrx1;  //29
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float afslrx2;  //30
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float afslrx3;  //31
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float pwrrx1;  //32
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float pwrrx2;  //33
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float pwrrx3;  //34
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TRAFRX_SZ)]
        public string trafrx;  //35
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float esint;  //36
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float tsint;  //37
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SRVCRX_SZ)]
        public string srvcrx;  //38
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STATRX_SZ)]
        public string statrx;  //39
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ROUTNUMB_SZ)]
        public string routnumb;  //40
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short stnnumb;  //41
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short hopnumb;  //42
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SDATE_SZ)]
        public string sdate;  //43
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTETX_SZ)]
        public string notetx;  //44
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTERX_SZ)]
        public string noterx;  //45
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTEGNL_SZ)]
        public string notegnl;  //46
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CPOINT_SZ)]
        public string cpoint;  //47
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = FEETX_SZ)]
        public string feetx;  //48
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = FEERX_SZ)]
        public string feerx;  //49
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;  //50
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;  //51

        //----------------------------------------------------------------
        //Additional public static members.

        public const int CMD_SZ = 2;
        public const int RECSTAT_SZ = 2;
        public const int CALL1_SZ = 10;
        public const int CALL2_SZ = 10;
        public const int BNDCDE_SZ = 5;
        public const int SPLAN_SZ = 5;
        public const int CHID_SZ = 5;
        public const int POLTX_SZ = 2;
        public const int EQPTTX_SZ = 9;
        public const int EQPTUTX_SZ = 2;
        public const int TRAFTX_SZ = 7;
        public const int SRVCTX_SZ = 7;
        public const int STATTX_SZ = 2;
        public const int POLRX_SZ = 2;
        public const int EQPTRX_SZ = Constant.ECODE_SZ;
        public const int EQPTURX_SZ = 2;
        public const int TRAFRX_SZ = Constant.TRAFCODE_SZ;
        public const int SRVCRX_SZ = Constant.TRAFCODE_SZ;
        public const int STATRX_SZ = 2;
        public const int ROUTNUMB_SZ = Constant.ROUTE_SZ;
        public const int SDATE_SZ = Constant.DATE_SZ;
        public const int NOTETX_SZ = 5;
        public const int NOTERX_SZ = 5;
        public const int NOTEGNL_SZ = 5;
        public const int CPOINT_SZ = 5;
        public const int FEETX_SZ = 3;
        public const int FEERX_SZ = 3;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int CALL1 = 2;
        public const int CALL2 = 3;
        public const int BNDCDE = 4;
        public const int SPLAN = 5;
        public const int HL = 6;
        public const int VH = 7;
        public const int CHID = 8;
        public const int FREQTX = 9;
        public const int POLTX = 10;
        public const int ANTNUMBTX1 = 11;
        public const int ANTNUMBTX2 = 12;
        public const int EQPTTX = 13;
        public const int EQPTUTX = 14;
        public const int PWRTX = 15;
        public const int ATPCCDE = 16;
        public const int AFSLTX1 = 17;
        public const int AFSLTX2 = 18;
        public const int TRAFTX = 19;
        public const int SRVCTX = 20;
        public const int STATTX = 21;
        public const int FREQRX = 22;
        public const int POLRX = 23;
        public const int ANTNUMBRX1 = 24;
        public const int ANTNUMBRX2 = 25;
        public const int ANTNUMBRX3 = 26;
        public const int EQPTRX = 27;
        public const int EQPTURX = 28;
        public const int AFSLRX1 = 29;
        public const int AFSLRX2 = 30;
        public const int AFSLRX3 = 31;
        public const int PWRRX1 = 32;
        public const int PWRRX2 = 33;
        public const int PWRRX3 = 34;
        public const int TRAFRX = 35;
        public const int ESINT = 36;
        public const int TSINT = 37;
        public const int SRVCRX = 38;
        public const int STATRX = 39;
        public const int ROUTNUMB = 40;
        public const int STNNUMB = 41;
        public const int HOPNUMB = 42;
        public const int SDATE = 43;
        public const int NOTETX = 44;
        public const int NOTERX = 45;
        public const int NOTEGNL = 46;
        public const int CPOINT = 47;
        public const int FEETX = 48;
        public const int FEERX = 49;
        public const int MDATE = 50;
        public const int MTIME = 51;

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 52;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "call1", "call2", "bndcde", "splan", "hl", "vh", "chid", "freqtx", "poltx", "antnumbtx1", "antnumbtx2", "eqpttx", "eqptutx", "pwrtx", "atpccde", "afsltx1", "afsltx2", "traftx", "srvctx", "stattx", "freqrx", "polrx", "antnumbrx1", "antnumbrx2", "antnumbrx3", "eqptrx", "eqpturx", "afslrx1", "afslrx2", "afslrx3", "pwrrx1", "pwrrx2", "pwrrx3", "trafrx", "esint", "tsint", "srvcrx", "statrx", "routnumb", "stnnumb", "hopnumb", "sdate", "notetx", "noterx", "notegnl", "cpoint", "feetx", "feerx", "mdate", "mtime" };

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

        //----------------------------------------------------------------

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static FtChan()
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
        public FtChan()
        {
            //Set all column members to prescribed initial values.
            Initialize();
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
            sb.AppendLine("cmd =        " + cmd);
            sb.AppendLine("recstat =    " + recstat);
            sb.AppendLine("call1 =      " + call1);
            sb.AppendLine("call2 =      " + call2);
            sb.AppendLine("bndcde =     " + bndcde);
            sb.AppendLine("splan =      " + splan);
            sb.AppendLine("hl =         " + hl);
            sb.AppendLine("vh =         " + vh);
            sb.AppendLine("chid =       " + chid);
            sb.AppendLine("freqtx =     " + freqtx);
            sb.AppendLine("poltx =      " + poltx);
            sb.AppendLine("antnumbtx1 = " + antnumbtx1);
            sb.AppendLine("antnumbtx2 = " + antnumbtx2);
            sb.AppendLine("eqpttx =     " + eqpttx);
            sb.AppendLine("eqptutx =    " + eqptutx);
            sb.AppendLine("pwrtx =      " + pwrtx);
            sb.AppendLine("atpccde =    " + atpccde);
            sb.AppendLine("afsltx1 =    " + afsltx1);
            sb.AppendLine("afsltx2 =    " + afsltx2);
            sb.AppendLine("traftx =     " + traftx);
            sb.AppendLine("srvctx =     " + srvctx);
            sb.AppendLine("stattx =     " + stattx);
            sb.AppendLine("freqrx =     " + freqrx);
            sb.AppendLine("polrx =      " + polrx);
            sb.AppendLine("antnumbrx1 = " + antnumbrx1);
            sb.AppendLine("antnumbrx2 = " + antnumbrx2);
            sb.AppendLine("antnumbrx3 = " + antnumbrx3);
            sb.AppendLine("eqptrx =     " + eqptrx);
            sb.AppendLine("eqpturx =    " + eqpturx);
            sb.AppendLine("afslrx1 =    " + afslrx1);
            sb.AppendLine("afslrx2 =    " + afslrx2);
            sb.AppendLine("afslrx3 =    " + afslrx3);
            sb.AppendLine("pwrrx1 =     " + pwrrx1);
            sb.AppendLine("pwrrx2 =     " + pwrrx2);
            sb.AppendLine("pwrrx3 =     " + pwrrx3);
            sb.AppendLine("trafrx =     " + trafrx);
            sb.AppendLine("esint =      " + esint);
            sb.AppendLine("tsint =      " + tsint);
            sb.AppendLine("srvcrx =     " + srvcrx);
            sb.AppendLine("statrx =     " + statrx);
            sb.AppendLine("routnumb =   " + routnumb);
            sb.AppendLine("stnnumb =    " + stnnumb);
            sb.AppendLine("hopnumb =    " + hopnumb);
            sb.AppendLine("sdate =      " + sdate);
            sb.AppendLine("notetx =     " + notetx);
            sb.AppendLine("noterx =     " + noterx);
            sb.AppendLine("notegnl =    " + notegnl);
            sb.AppendLine("cpoint =     " + cpoint);
            sb.AppendLine("feetx =      " + feetx);
            sb.AppendLine("feerx =      " + feerx);
            sb.AppendLine("mdate =      " + mdate);
            sb.AppendLine("mtime =      " + mtime);

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
        /// This method initializes the value of all member fields.
        /// </summary>
        /// <param name=""></param>
        public void Initialize()
        {
            const string STRING_INIT_VAL = "";
            const short SHORT_INIT_VAL = 0;
            const float FLOAT_INIT_VAL = 0.0f;
            const double DOUBLE_INIT_VAL = 0.0;

            cmd = STRING_INIT_VAL;    //0
            recstat = STRING_INIT_VAL;    //1
            call1 = STRING_INIT_VAL;    //2
            call2 = STRING_INIT_VAL;    //3
            bndcde = STRING_INIT_VAL;    //4
            splan = STRING_INIT_VAL;    //5
            hl = SHORT_INIT_VAL;    //6
            vh = SHORT_INIT_VAL;    //7
            chid = STRING_INIT_VAL;    //8
            freqtx = DOUBLE_INIT_VAL;    //9
            poltx = STRING_INIT_VAL;    //10
            antnumbtx1 = SHORT_INIT_VAL;    //11
            antnumbtx2 = SHORT_INIT_VAL;    //12
            eqpttx = STRING_INIT_VAL;    //13
            eqptutx = STRING_INIT_VAL;    //14
            pwrtx = FLOAT_INIT_VAL;    //15
            atpccde = FLOAT_INIT_VAL;    //16
            afsltx1 = FLOAT_INIT_VAL;    //17
            afsltx2 = FLOAT_INIT_VAL;    //18
            traftx = STRING_INIT_VAL;    //19
            srvctx = STRING_INIT_VAL;    //20
            stattx = STRING_INIT_VAL;    //21
            freqrx = DOUBLE_INIT_VAL;    //22
            polrx = STRING_INIT_VAL;    //23
            antnumbrx1 = SHORT_INIT_VAL;    //24
            antnumbrx2 = SHORT_INIT_VAL;    //25
            antnumbrx3 = SHORT_INIT_VAL;    //26
            eqptrx = STRING_INIT_VAL;    //27
            eqpturx = STRING_INIT_VAL;    //28
            afslrx1 = FLOAT_INIT_VAL;    //29
            afslrx2 = FLOAT_INIT_VAL;    //30
            afslrx3 = FLOAT_INIT_VAL;    //31
            pwrrx1 = FLOAT_INIT_VAL;    //32
            pwrrx2 = FLOAT_INIT_VAL;    //33
            pwrrx3 = FLOAT_INIT_VAL;    //34
            trafrx = STRING_INIT_VAL;    //35
            esint = FLOAT_INIT_VAL;    //36
            tsint = FLOAT_INIT_VAL;    //37
            srvcrx = STRING_INIT_VAL;    //38
            statrx = STRING_INIT_VAL;    //39
            routnumb = STRING_INIT_VAL;    //40
            stnnumb = SHORT_INIT_VAL;    //41
            hopnumb = SHORT_INIT_VAL;    //42
            sdate = STRING_INIT_VAL;    //43
            notetx = STRING_INIT_VAL;    //44
            noterx = STRING_INIT_VAL;    //45
            notegnl = STRING_INIT_VAL;    //46
            cpoint = STRING_INIT_VAL;    //47
            feetx = STRING_INIT_VAL;    //48
            feerx = STRING_INIT_VAL;    //49
            mdate = STRING_INIT_VAL;    //50
            mtime = STRING_INIT_VAL;    //51
        }

        /// <summary>
        /// This method supports testing by setting the fields of this object
        /// to predetermined reference values.
        /// </summary>
        /// <param name=""></param>
        public void SetAllColumnsToPrescribedTestValues()
        {
            cmd = "0";    //0
            recstat = "1";    //1
            call1 = "02";    //2
            call2 = "03";    //3
            bndcde = "04";    //4
            splan = "05";    //5
            hl = 6;    //6
            vh = 7;    //7
            chid = "08";    //8
            freqtx = 9.0;    //9
            poltx = "A";    //10
            antnumbtx1 = 11;    //11
            antnumbtx2 = 12;    //12
            eqpttx = "13";    //13
            eqptutx = "E";    //14
            pwrtx = 15.0F;    //15
            atpccde = 16.0F;    //16
            afsltx1 = 17.0F;    //17
            afsltx2 = 18.0F;    //18
            traftx = "19";    //19
            srvctx = "20";    //20
            stattx = "X";    //21
            freqrx = 22.0;    //22
            polrx = "Y";    //23
            antnumbrx1 = 24;    //24
            antnumbrx2 = 25;    //25
            antnumbrx3 = 26;    //26
            eqptrx = "27";    //27
            eqpturx = "X";    //28
            afslrx1 = 29.0F;    //29
            afslrx2 = 30.0F;    //30
            afslrx3 = 31.0F;    //31
            pwrrx1 = 32.0F;    //32
            pwrrx2 = 33.0F;    //33
            pwrrx3 = 34.0F;    //34
            trafrx = "35";    //35
            esint = 36.0F;    //36
            tsint = 37.0F;    //37
            srvcrx = "38";    //38
            statrx = "Z";    //39
            routnumb = "40";    //40
            stnnumb = 41;    //41
            hopnumb = 42;    //42
            sdate = "43";    //43
            notetx = "44";    //44
            noterx = "45";    //45
            notegnl = "46";    //46
            cpoint = "47";    //47
            feetx = "48";    //48
            feerx = "49";    //49
            mdate = "50";    //50
            mtime = "51";    //51
        }

        /// <summary>
        /// For each field of this object, the method allocates a type-specific amount
        /// of global (heap) memory, copies the value of the field into it, and returns
        /// an IntPtr[] containing all of the start addresses.
        /// </summary>
        /// <returns></returns>
        public SQLPOINTER[] CopyToArrayOfSQLPOINTERs()
        {
            //Can only copy arrays of float and/or double into native memory using P/Invoke marshalling.
            float[] F = new float[1];
            double[] D = new double[1];

            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[0] = Marshal.StringToHGlobalAnsi(cmd);  // #0  string  cmd

            parameterValuePtr[1] = Marshal.StringToHGlobalAnsi(recstat);  // #1  string  recstat

            parameterValuePtr[2] = Marshal.StringToHGlobalAnsi(call1);  // #2  string  call1

            parameterValuePtr[3] = Marshal.StringToHGlobalAnsi(call2);  // #3  string  call2

            parameterValuePtr[4] = Marshal.StringToHGlobalAnsi(bndcde);  // #4  string  bndcde

            parameterValuePtr[5] = Marshal.StringToHGlobalAnsi(splan);  // #5  string  splan

            parameterValuePtr[6] = Marshal.AllocHGlobal(sizeof(short));  // #6  short  hl
            Marshal.WriteInt16(parameterValuePtr[6], hl);

            parameterValuePtr[7] = Marshal.AllocHGlobal(sizeof(short));  // #7  short  vh
            Marshal.WriteInt16(parameterValuePtr[7], vh);

            parameterValuePtr[8] = Marshal.StringToHGlobalAnsi(chid);  // #8  string  chid

            parameterValuePtr[9] = Marshal.AllocHGlobal(sizeof(double));  // #9  double  freqtx
            D[0] = freqtx;
            Marshal.Copy(D, 0, parameterValuePtr[9], 1);

            parameterValuePtr[10] = Marshal.StringToHGlobalAnsi(poltx);  // #10  string  poltx

            parameterValuePtr[11] = Marshal.AllocHGlobal(sizeof(short));  // #11  short  antnumbtx1
            Marshal.WriteInt16(parameterValuePtr[11], antnumbtx1);

            parameterValuePtr[12] = Marshal.AllocHGlobal(sizeof(short));  // #12  short  antnumbtx2
            Marshal.WriteInt16(parameterValuePtr[12], antnumbtx2);

            parameterValuePtr[13] = Marshal.StringToHGlobalAnsi(eqpttx);  // #13  string  eqpttx

            parameterValuePtr[14] = Marshal.StringToHGlobalAnsi(eqptutx);  // #14  string  eqptutx

            parameterValuePtr[15] = Marshal.AllocHGlobal(sizeof(float));  // #15  float  pwrtx
            F[0] = pwrtx;
            Marshal.Copy(F, 0, parameterValuePtr[15], 1);

            parameterValuePtr[16] = Marshal.AllocHGlobal(sizeof(float));  // #16  float  atpccde
            F[0] = atpccde;
            Marshal.Copy(F, 0, parameterValuePtr[16], 1);

            parameterValuePtr[17] = Marshal.AllocHGlobal(sizeof(float));  // #17  float  afsltx1
            F[0] = afsltx1;
            Marshal.Copy(F, 0, parameterValuePtr[17], 1);

            parameterValuePtr[18] = Marshal.AllocHGlobal(sizeof(float));  // #18  float  afsltx2
            F[0] = afsltx2;
            Marshal.Copy(F, 0, parameterValuePtr[18], 1);

            parameterValuePtr[19] = Marshal.StringToHGlobalAnsi(traftx);  // #19  string  traftx

            parameterValuePtr[20] = Marshal.StringToHGlobalAnsi(srvctx);  // #20  string  srvctx

            parameterValuePtr[21] = Marshal.StringToHGlobalAnsi(stattx);  // #21  string  stattx

            parameterValuePtr[22] = Marshal.AllocHGlobal(sizeof(double));  // #22  double  freqrx
            D[0] = freqrx;
            Marshal.Copy(D, 0, parameterValuePtr[22], 1);

            parameterValuePtr[23] = Marshal.StringToHGlobalAnsi(polrx);  // #23  string  polrx

            parameterValuePtr[24] = Marshal.AllocHGlobal(sizeof(short));  // #24  short  antnumbrx1
            Marshal.WriteInt16(parameterValuePtr[24], antnumbrx1);

            parameterValuePtr[25] = Marshal.AllocHGlobal(sizeof(short));  // #25  short  antnumbrx2
            Marshal.WriteInt16(parameterValuePtr[25], antnumbrx2);

            parameterValuePtr[26] = Marshal.AllocHGlobal(sizeof(short));  // #26  short  antnumbrx3
            Marshal.WriteInt16(parameterValuePtr[26], antnumbrx3);

            parameterValuePtr[27] = Marshal.StringToHGlobalAnsi(eqptrx);  // #27  string  eqptrx

            parameterValuePtr[28] = Marshal.StringToHGlobalAnsi(eqpturx);  // #28  string  eqpturx

            parameterValuePtr[29] = Marshal.AllocHGlobal(sizeof(float));  // #29  float  afslrx1
            F[0] = afslrx1;
            Marshal.Copy(F, 0, parameterValuePtr[29], 1);

            parameterValuePtr[30] = Marshal.AllocHGlobal(sizeof(float));  // #30  float  afslrx2
            F[0] = afslrx2;
            Marshal.Copy(F, 0, parameterValuePtr[30], 1);

            parameterValuePtr[31] = Marshal.AllocHGlobal(sizeof(float));  // #31  float  afslrx3
            F[0] = afslrx3;
            Marshal.Copy(F, 0, parameterValuePtr[31], 1);

            parameterValuePtr[32] = Marshal.AllocHGlobal(sizeof(float));  // #32  float  pwrrx1
            F[0] = pwrrx1;
            Marshal.Copy(F, 0, parameterValuePtr[32], 1);

            parameterValuePtr[33] = Marshal.AllocHGlobal(sizeof(float));  // #33  float  pwrrx2
            F[0] = pwrrx2;
            Marshal.Copy(F, 0, parameterValuePtr[33], 1);

            parameterValuePtr[34] = Marshal.AllocHGlobal(sizeof(float));  // #34  float  pwrrx3
            F[0] = pwrrx3;
            Marshal.Copy(F, 0, parameterValuePtr[34], 1);

            parameterValuePtr[35] = Marshal.StringToHGlobalAnsi(trafrx);  // #35  string  trafrx

            parameterValuePtr[36] = Marshal.AllocHGlobal(sizeof(float));  // #36  float  esint
            F[0] = esint;
            Marshal.Copy(F, 0, parameterValuePtr[36], 1);

            parameterValuePtr[37] = Marshal.AllocHGlobal(sizeof(float));  // #37  float  tsint
            F[0] = tsint;
            Marshal.Copy(F, 0, parameterValuePtr[37], 1);

            parameterValuePtr[38] = Marshal.StringToHGlobalAnsi(srvcrx);  // #38  string  srvcrx

            parameterValuePtr[39] = Marshal.StringToHGlobalAnsi(statrx);  // #39  string  statrx

            parameterValuePtr[40] = Marshal.StringToHGlobalAnsi(routnumb);  // #40  string  routnumb

            parameterValuePtr[41] = Marshal.AllocHGlobal(sizeof(short));  // #41  short  stnnumb
            Marshal.WriteInt16(parameterValuePtr[41], stnnumb);

            parameterValuePtr[42] = Marshal.AllocHGlobal(sizeof(short));  // #42  short  hopnumb
            Marshal.WriteInt16(parameterValuePtr[42], hopnumb);

            parameterValuePtr[43] = Marshal.StringToHGlobalAnsi(sdate);  // #43  string  sdate

            parameterValuePtr[44] = Marshal.StringToHGlobalAnsi(notetx);  // #44  string  notetx

            parameterValuePtr[45] = Marshal.StringToHGlobalAnsi(noterx);  // #45  string  noterx

            parameterValuePtr[46] = Marshal.StringToHGlobalAnsi(notegnl);  // #46  string  notegnl

            parameterValuePtr[47] = Marshal.StringToHGlobalAnsi(cpoint);  // #47  string  cpoint

            parameterValuePtr[48] = Marshal.StringToHGlobalAnsi(feetx);  // #48  string  feetx

            parameterValuePtr[49] = Marshal.StringToHGlobalAnsi(feerx);  // #49  string  feerx

            parameterValuePtr[50] = Marshal.StringToHGlobalAnsi(mdate);  // #50  string  mdate

            parameterValuePtr[51] = Marshal.StringToHGlobalAnsi(mtime);  // #51  string  mtime

            return parameterValuePtr;
        }

        /// <summary>
		/// For each field of this object, this method copies the value of the field into
        /// a prescribed location in global (heap) memory given by the elements in 
        /// SQLPOINTER[] array.
        /// </summary>
        /// <param name="parameterValuePtr"></param>
        public void CopyToExistingArrayOfSQLPOINTERs(SQLPOINTER[] parameterValuePtr)
        {
            //Can only copy arrays of float and/or double into native memory using P/Invoke marshalling.
            float[] F = new float[1];
            double[] D = new double[1];

            WSTP(cmd, parameterValuePtr[0]);  // #0  string  cmd

            WSTP(recstat, parameterValuePtr[1]);  // #1  string  recstat

            WSTP(call1, parameterValuePtr[2]);  // #2  string  call1

            WSTP(call2, parameterValuePtr[3]);  // #3  string  call2

            WSTP(bndcde, parameterValuePtr[4]);  // #4  string  bndcde

            WSTP(splan, parameterValuePtr[5]);  // #5  string  splan

            Marshal.WriteInt16(parameterValuePtr[6], hl);  // #6  short  hl

            Marshal.WriteInt16(parameterValuePtr[7], vh);  // #7  short  vh

            WSTP(chid, parameterValuePtr[8]);  // #8  string  chid

            D[0] = freqtx;
            Marshal.Copy(D, 0, parameterValuePtr[9], 1);  // #9  double  freqtx

            WSTP(poltx, parameterValuePtr[10]);  // #10  string  poltx

            Marshal.WriteInt16(parameterValuePtr[11], antnumbtx1);  // #11  short  antnumbtx1

            Marshal.WriteInt16(parameterValuePtr[12], antnumbtx2);  // #12  short  antnumbtx2

            WSTP(eqpttx, parameterValuePtr[13]);  // #13  string  eqpttx

            WSTP(eqptutx, parameterValuePtr[14]);  // #14  string  eqptutx

            F[0] = pwrtx;
            Marshal.Copy(F, 0, parameterValuePtr[15], 1);  // #15  float  pwrtx

            F[0] = atpccde;
            Marshal.Copy(F, 0, parameterValuePtr[16], 1);  // #16  float  atpccde

            F[0] = afsltx1;
            Marshal.Copy(F, 0, parameterValuePtr[17], 1);  // #17  float  afsltx1

            F[0] = afsltx2;
            Marshal.Copy(F, 0, parameterValuePtr[18], 1);  // #18  float  afsltx2

            WSTP(traftx, parameterValuePtr[19]);  // #19  string  traftx

            WSTP(srvctx, parameterValuePtr[20]);  // #20  string  srvctx

            WSTP(stattx, parameterValuePtr[21]);  // #21  string  stattx

            D[0] = freqrx;
            Marshal.Copy(D, 0, parameterValuePtr[22], 1);  // #22  double  freqrx

            WSTP(polrx, parameterValuePtr[23]);  // #23  string  polrx

            Marshal.WriteInt16(parameterValuePtr[24], antnumbrx1);  // #24  short  antnumbrx1

            Marshal.WriteInt16(parameterValuePtr[25], antnumbrx2);  // #25  short  antnumbrx2

            Marshal.WriteInt16(parameterValuePtr[26], antnumbrx3);  // #26  short  antnumbrx3

            WSTP(eqptrx, parameterValuePtr[27]);  // #27  string  eqptrx

            WSTP(eqpturx, parameterValuePtr[28]);  // #28  string  eqpturx

            F[0] = afslrx1;
            Marshal.Copy(F, 0, parameterValuePtr[29], 1);  // #29  float  afslrx1

            F[0] = afslrx2;
            Marshal.Copy(F, 0, parameterValuePtr[30], 1);  // #30  float  afslrx2

            F[0] = afslrx3;
            Marshal.Copy(F, 0, parameterValuePtr[31], 1);  // #31  float  afslrx3

            F[0] = pwrrx1;
            Marshal.Copy(F, 0, parameterValuePtr[32], 1);  // #32  float  pwrrx1

            F[0] = pwrrx2;
            Marshal.Copy(F, 0, parameterValuePtr[33], 1);  // #33  float  pwrrx2

            F[0] = pwrrx3;
            Marshal.Copy(F, 0, parameterValuePtr[34], 1);  // #34  float  pwrrx3

            WSTP(trafrx, parameterValuePtr[35]);  // #35  string  trafrx

            F[0] = esint;
            Marshal.Copy(F, 0, parameterValuePtr[36], 1);  // #36  float  esint

            F[0] = tsint;
            Marshal.Copy(F, 0, parameterValuePtr[37], 1);  // #37  float  tsint

            WSTP(srvcrx, parameterValuePtr[38]);  // #38  string  srvcrx

            WSTP(statrx, parameterValuePtr[39]);  // #39  string  statrx

            WSTP(routnumb, parameterValuePtr[40]);  // #40  string  routnumb

            Marshal.WriteInt16(parameterValuePtr[41], stnnumb);  // #41  short  stnnumb

            Marshal.WriteInt16(parameterValuePtr[42], hopnumb);  // #42  short  hopnumb

            WSTP(sdate, parameterValuePtr[43]);  // #43  string  sdate

            WSTP(notetx, parameterValuePtr[44]);  // #44  string  notetx

            WSTP(noterx, parameterValuePtr[45]);  // #45  string  noterx

            WSTP(notegnl, parameterValuePtr[46]);  // #46  string  notegnl

            WSTP(cpoint, parameterValuePtr[47]);  // #47  string  cpoint

            WSTP(feetx, parameterValuePtr[48]);  // #48  string  feetx

            WSTP(feerx, parameterValuePtr[49]);  // #49  string  feerx

            WSTP(mdate, parameterValuePtr[50]);  // #50  string  mdate

            WSTP(mtime, parameterValuePtr[51]);  // #51  string  mtime

            return;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the member fields together with
        /// their associated ODBC nullInds.
        /// </summary>
        /// <param name="nulls"></param>
        /// <returns></returns>
        public string ToStringWN(SQLLEN[] nulls)
        {
            StringBuilder sb = new StringBuilder();
            int n = 0;

            sb.AppendLine();
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "cmd =        " + cmd);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "recstat =    " + recstat);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "call1 =      " + call1);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "call2 =      " + call2);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "bndcde =     " + bndcde);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "splan =      " + splan);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "hl =         " + hl);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "vh =         " + vh);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "chid =       " + chid);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "freqtx =     " + freqtx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "poltx =      " + poltx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "antnumbtx1 = " + antnumbtx1);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "antnumbtx2 = " + antnumbtx2);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "eqpttx =     " + eqpttx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "eqptutx =    " + eqptutx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "pwrtx =      " + pwrtx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "atpccde =    " + atpccde);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "afsltx1 =    " + afsltx1);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "afsltx2 =    " + afsltx2);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "traftx =     " + traftx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "srvctx =     " + srvctx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "stattx =     " + stattx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "freqrx =     " + freqrx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "polrx =      " + polrx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "antnumbrx1 = " + antnumbrx1);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "antnumbrx2 = " + antnumbrx2);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "antnumbrx3 = " + antnumbrx3);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "eqptrx =     " + eqptrx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "eqpturx =    " + eqpturx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "afslrx1 =    " + afslrx1);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "afslrx2 =    " + afslrx2);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "afslrx3 =    " + afslrx3);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "pwrrx1 =     " + pwrrx1);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "pwrrx2 =     " + pwrrx2);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "pwrrx3 =     " + pwrrx3);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "trafrx =     " + trafrx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "esint =      " + esint);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "tsint =      " + tsint);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "srvcrx =     " + srvcrx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "statrx =     " + statrx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "routnumb =   " + routnumb);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "stnnumb =    " + stnnumb);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "hopnumb =    " + hopnumb);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "sdate =      " + sdate);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "notetx =     " + notetx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "noterx =     " + noterx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "notegnl =    " + notegnl);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "cpoint =     " + cpoint);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "feetx =      " + feetx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "feerx =      " + feerx);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "mdate =      " + mdate);
            sb.AppendLine("FtChan:  " + nulls[n++] + ":   " + "mtime =      " + mtime);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns an SQLPOINTER[] whose elements point to a block of global
        /// memory sufficient to hold any valid value of the corresponding field 
        /// (i.e. max length strings).
        /// </summary>
        /// <remarks>
        /// NOTES: 
        ///       1. The number of bytes allocated for string fields is the maximum number of
        ///          characters plus 1 - to accommodate a final \0 terminator.
        ///       2. This method does not set any FtAnte field values; this is the responsibility
        ///          of the user.
        /// </remarks>
        /// <returns></returns>
        public static SQLPOINTER[] AllocateArrayOfSQLPOINTERsToBuffers()
        {
            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[0] = Marshal.AllocHGlobal(Constant.CMD_SZ + 1);  // #0  string  cmd

            parameterValuePtr[1] = Marshal.AllocHGlobal(Constant.RECSTAT_SZ + 1);  // #1  string  recstat

            parameterValuePtr[2] = Marshal.AllocHGlobal(Constant.CALL_SZ + 1);  // #2  string  call1

            parameterValuePtr[3] = Marshal.AllocHGlobal(Constant.CALL_SZ + 1);  // #3  string  call2

            parameterValuePtr[4] = Marshal.AllocHGlobal(Constant.BNDCDE_SZ + 1);  // #4  string  bndcde

            parameterValuePtr[5] = Marshal.AllocHGlobal(Constant.PLAN_SZ + 1);  // #5  string  splan

            parameterValuePtr[6] = Marshal.AllocHGlobal(sizeof(short));  // #6  short  hl

            parameterValuePtr[7] = Marshal.AllocHGlobal(sizeof(short));  // #7  short  vh

            parameterValuePtr[8] = Marshal.AllocHGlobal(Constant.CHID_SZ + 1);  // #8  string  chid

            parameterValuePtr[9] = Marshal.AllocHGlobal(sizeof(double));  // #9  double  freqtx

            parameterValuePtr[10] = Marshal.AllocHGlobal(Constant.POLTX_SZ + 1);  // #10  string  poltx

            parameterValuePtr[11] = Marshal.AllocHGlobal(sizeof(short));  // #11  short  antnumbtx1

            parameterValuePtr[12] = Marshal.AllocHGlobal(sizeof(short));  // #12  short  antnumbtx2

            parameterValuePtr[13] = Marshal.AllocHGlobal(Constant.EQPTTX_SZ + 1);  // #13  string  eqpttx

            parameterValuePtr[14] = Marshal.AllocHGlobal(Constant.EQPTUTX_SZ + 1);  // #14  string  eqptutx

            parameterValuePtr[15] = Marshal.AllocHGlobal(sizeof(float));  // #15  float  pwrtx

            parameterValuePtr[16] = Marshal.AllocHGlobal(sizeof(float));  // #16  float  atpccde

            parameterValuePtr[17] = Marshal.AllocHGlobal(sizeof(float));  // #17  float  afsltx1

            parameterValuePtr[18] = Marshal.AllocHGlobal(sizeof(float));  // #18  float  afsltx2

            parameterValuePtr[19] = Marshal.AllocHGlobal(Constant.TRAFTX_SZ + 1);  // #19  string  traftx

            parameterValuePtr[20] = Marshal.AllocHGlobal(Constant.SRVCTX_SZ + 1);  // #20  string  srvctx

            parameterValuePtr[21] = Marshal.AllocHGlobal(Constant.STATTX_SZ + 1);  // #21  string  stattx

            parameterValuePtr[22] = Marshal.AllocHGlobal(sizeof(double));  // #22  double  freqrx

            parameterValuePtr[23] = Marshal.AllocHGlobal(Constant.POLRX_SZ + 1);  // #23  string  polrx

            parameterValuePtr[24] = Marshal.AllocHGlobal(sizeof(short));  // #24  short  antnumbrx1

            parameterValuePtr[25] = Marshal.AllocHGlobal(sizeof(short));  // #25  short  antnumbrx2

            parameterValuePtr[26] = Marshal.AllocHGlobal(sizeof(short));  // #26  short  antnumbrx3

            parameterValuePtr[27] = Marshal.AllocHGlobal(Constant.EQPTRX_SZ + 1);  // #27  string  eqptrx

            parameterValuePtr[28] = Marshal.AllocHGlobal(Constant.EQPTURX_SZ + 1);  // #28  string  eqpturx

            parameterValuePtr[29] = Marshal.AllocHGlobal(sizeof(float));  // #29  float  afslrx1

            parameterValuePtr[30] = Marshal.AllocHGlobal(sizeof(float));  // #30  float  afslrx2

            parameterValuePtr[31] = Marshal.AllocHGlobal(sizeof(float));  // #31  float  afslrx3

            parameterValuePtr[32] = Marshal.AllocHGlobal(sizeof(float));  // #32  float  pwrrx1

            parameterValuePtr[33] = Marshal.AllocHGlobal(sizeof(float));  // #33  float  pwrrx2

            parameterValuePtr[34] = Marshal.AllocHGlobal(sizeof(float));  // #34  float  pwrrx3

            parameterValuePtr[35] = Marshal.AllocHGlobal(Constant.TRAFCODE_SZ + 1);  // #35  string  trafrx

            parameterValuePtr[36] = Marshal.AllocHGlobal(sizeof(float));  // #36  float  esint

            parameterValuePtr[37] = Marshal.AllocHGlobal(sizeof(float));  // #37  float  tsint

            parameterValuePtr[38] = Marshal.AllocHGlobal(Constant.TRAFCODE_SZ + 1);  // #38  string  srvcrx

            parameterValuePtr[39] = Marshal.AllocHGlobal(Constant.STATRX_SZ + 1);  // #39  string  statrx

            parameterValuePtr[40] = Marshal.AllocHGlobal(Constant.ROUTE_SZ + 1);  // #40  string  routnumb

            parameterValuePtr[41] = Marshal.AllocHGlobal(sizeof(short));  // #41  short  stnnumb

            parameterValuePtr[42] = Marshal.AllocHGlobal(sizeof(short));  // #42  short  hopnumb

            parameterValuePtr[43] = Marshal.AllocHGlobal(Constant.DATE_SZ + 1);  // #43  string  sdate

            parameterValuePtr[44] = Marshal.AllocHGlobal(Constant.NOTA_SZ + 1);  // #44  string  notetx

            parameterValuePtr[45] = Marshal.AllocHGlobal(Constant.NOTA_SZ + 1);  // #45  string  noterx

            parameterValuePtr[46] = Marshal.AllocHGlobal(Constant.NOTA_SZ + 1);  // #46  string  notegnl

            parameterValuePtr[47] = Marshal.AllocHGlobal(Constant.CPOINT_SZ + 1);  // #47  string  cpoint

            parameterValuePtr[48] = Marshal.AllocHGlobal(Constant.FEETX_SZ + 1);  // #48  string  feetx

            parameterValuePtr[49] = Marshal.AllocHGlobal(Constant.FEERX_SZ + 1);  // #49  string  feerx

            parameterValuePtr[50] = Marshal.AllocHGlobal(Constant.DATE_SZ + 1);  // #50  string  mdate

            parameterValuePtr[51] = Marshal.AllocHGlobal(Constant.TIME_SZ + 1);  // #51  string  mtime

            return parameterValuePtr;
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
        /// This method writes an ASCII copy of a string into a buffer
        /// in global (heap) memory whose start address is given by the 
        /// prescribed IntPtr. (WSTP = Write String To Pointer).
        /// </summary>
        /// <param name="str"></param>
        /// <param name="intPtr"></param>
        public void WSTP(string str, IntPtr intPtr)
        {
            // Create an ASCII encoding.
            Encoding ascii = Encoding.ASCII;

            Byte[] bytes = ascii.GetBytes(str + "\0");
            Marshal.Copy(bytes, 0, intPtr, bytes.Length);
        }

        /// <summary>
        /// Ths method returns true if the first character of this 
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
        /// This method returns a string that concatenates the 'key'
        /// fields {call1, call2, bndcde, chid}.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string KeysToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("call1 = " + call1);
            sb.Append("   call2 = " + call2);
            sb.Append("   bndcde = " + bndcde);
            sb.Append("   chid = " + chid);
            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated multi-line string that provides the values of
        /// this object's call1, call2, bndcde, chid, freqtx, and freqrx members.
        /// </summary>
        /// <returns></returns>
        public string KeysPlusToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("call1: " + call1 + ",  ");
            sb.Append("call2: " + call2 + ",  ");
            sb.Append("bndcde: " + bndcde + ",  ");
            sb.Append("chid: " + chid + ",  ");
            sb.Append("freqtx: " + freqtx + ",  ");
            sb.Append("freqrx: " + freqrx);
            return sb.ToString();
        }

        /// <summary>
        /// This method returns a single-line string that provides the values of
        /// this object's call1, call2, bndcde, and chid members.
        /// </summary>
        /// <returns></returns>
        public string KeysTerseToString()
        {
            return String.Format("{0,9}, {1,9}, {2,4}, {3,4}", call1, call2, bndcde, chid);
        }

        /// <summary>
        /// This method returns a single-line string that provides the values of
        /// this object's call1, call2, bndcde, chid, freqtx, and freqrx members.
        /// </summary>
        /// <returns></returns>
        public string KeysPlusTerseToString()
        {
            return String.Format("{0,9}, {1,9}, {2,4}, {3,4}, {4,9:F0} / {5,9:F0}", call1, call2, bndcde, chid, freqtx, freqrx);
        }

        /// <summary>
        /// This method returns a string that can be used as a unique key for this FtChan
        /// object; the search key is simply the concatanation of the string values of this object's
        /// call1, call2, bndcde and chid values.
        /// </summary>
        /// <returns></returns>
        public string UniqueKey()
        {
            return (call1.Trim() + call2.Trim() + bndcde.Trim() + chid.Trim()).ToUpper();
        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[{1}]" +
"(" +
    "[cmd] [char](1) NULL," +
    "[recstat] [char](1) NULL," +
    "[call1] [char](9) NOT NULL," +
    "[call2] [char](9) NOT NULL," +
    "[bndcde] [char](4) NOT NULL," +
    "[splan] [char](4) NULL," +
    "[hl] [tinyint] NULL," +
    "[vh] [tinyint] NULL," +
    "[chid] [char](4) NOT NULL," +
    "[freqtx] [float] NULL," +
    "[poltx] [char](1) NULL," +
    "[antnumbtx1] [tinyint] NULL," +
    "[antnumbtx2] [tinyint] NULL," +
    "[eqpttx] [char](8) NULL," +
    "[eqptutx] [char](1) NULL," +
    "[pwrtx] [real] NULL," +
    "[atpccde] [real] NULL," +
    "[afsltx1] [real] NULL," +
    "[afsltx2] [real] NULL," +
    "[traftx] [char](6) NULL," +
    "[srvctx] [char](6) NULL," +
    "[stattx] [char](1) NULL," +
    "[freqrx] [float] NULL," +
    "[polrx] [char](1) NULL," +
    "[antnumbrx1] [tinyint] NULL," +
    "[antnumbrx2] [tinyint] NULL," +
    "[antnumbrx3] [tinyint] NULL," +
    "[eqptrx] [char](8) NULL," +
    "[eqpturx] [char](1) NULL," +
    "[afslrx1] [real] NULL," +
    "[afslrx2] [real] NULL," +
    "[afslrx3] [real] NULL," +
    "[pwrrx1] [real] NULL," +
    "[pwrrx2] [real] NULL," +
    "[pwrrx3] [real] NULL," +
    "[trafrx] [char](6) NULL," +
    "[esint] [real] NULL," +
    "[tsint] [real] NULL," +
    "[srvcrx] [char](6) NULL," +
    "[statrx] [char](1) NULL," +
    "[routnumb] [char](8) NULL," +
    "[stnnumb] [tinyint] NULL," +
    "[hopnumb] [tinyint] NULL," +
    "[sdate] [char](10) NULL," +
    "[notetx] [char](4) NULL," +
    "[noterx] [char](4) NULL," +
    "[notegnl] [char](4) NULL," +
    "[cpoint] [char](4) NULL," +
    "[feetx] [char](2) NULL," +
    "[feerx] [char](2) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
    "CONSTRAINT[PK_FtChan] PRIMARY KEY CLUSTERED " +
    "(" +
    "[call1] ASC," +
    "[call2] ASC," +
    "[bndcde] ASC," +
    "[chid] ASC" +
    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON[PRIMARY]" +
") ON[PRIMARY]";

    }
}
