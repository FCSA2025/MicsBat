# Documented File: MtChan.cs
**Repository Path:** `_DataStructures\MtChan.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
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
    /// This class has fields that are isomorphic with the master table <b>main.mt_chan</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    [Serializable]
    public class MtChan
    {
        // IMPORTANT!
        // =========
        // The following qty. 51 member values correspond to the columns
        // of an MtAnte table stored in the database; for simplicity, the
        // member and column names are prescribed to be identical.
        // The order of appearance of these qty. 51 'column' members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALL1_SZ)]
        public string call1;  // #0
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALL2_SZ)]
        public string call2;  // #1
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = BNDCDE_SZ)]
        public string bndcde;  // #2
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SPLAN_SZ)]
        public string splan;  // #3
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short hl;  // #4
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short vh;  // #5
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CHID_SZ)]
        public string chid;  // #6
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double freqtx;  // #7
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = POLTX_SZ)]
        public string poltx;  // #8
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short antnumbtx1;  // #9
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short antnumbtx2;  // #10
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EQPTTX_SZ)]
        public string eqpttx;  // #11
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EQPTUTX_SZ)]
        public string eqptutx;  // #12
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float pwrtx;  // #13
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float atpccde;  // #14
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float afsltx1;  // #15
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float afsltx2;  // #16
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TRAFTX_SZ)]
        public string traftx;  // #17
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SRVCTX_SZ)]
        public string srvctx;  // #18
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STATTX_SZ)]
        public string stattx;  // #19
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double freqrx;  // #20
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = POLRX_SZ)]
        public string polrx;  // #21
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short antnumbrx1;  // #22
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short antnumbrx2;  // #23
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short antnumbrx3;  // #24
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EQPTRX_SZ)]
        public string eqptrx;  // #25
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EQPTURX_SZ)]
        public string eqpturx;  // #26
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float afslrx1;  // #27
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float afslrx2;  // #28
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float afslrx3;  // #29
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float pwrrx1;  // #30
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float pwrrx2;  // #31
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float pwrrx3;  // #32
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TRAFRX_SZ)]
        public string trafrx;  // #33
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float esint;  // #34
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float tsint;  // #35
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SRVCRX_SZ)]
        public string srvcrx;  // #36
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STATRX_SZ)]
        public string statrx;  // #37
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ROUTNUMB_SZ)]
        public string routnumb;  // #38
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short stnnumb;  // #39
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short hopnumb;  // #40
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SDATE_SZ)]
        public string sdate;  // #41
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTETX_SZ)]
        public string notetx;  // #42
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTERX_SZ)]
        public string noterx;  // #43
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTEGNL_SZ)]
        public string notegnl;  // #44
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CPOINT_SZ)]
        public string cpoint;  // #45
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = FEETX_SZ)]
        public string feetx;  // #46
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = FEERX_SZ)]
        public string feerx;  // #47
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;  // #48
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;  // #49
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = USERID_SZ)]
        public string userid;  // #50

        //----------------------------------------------------------------
        //Additional public static members.

        //----------------------------------------------------------------

        public const int CALL1_SZ = Constant.CALL_SZ;
        public const int CALL2_SZ = Constant.CALL_SZ;
        public const int BNDCDE_SZ = Constant.BNDCDE_SZ;
        public const int SPLAN_SZ = Constant.PLAN_SZ;
        public const int CHID_SZ = Constant.CHID_SZ;
        public const int POLTX_SZ = Constant.POLTX_SZ;
        public const int EQPTTX_SZ = Constant.EQPTTX_SZ;
        public const int EQPTUTX_SZ = Constant.EQPTUTX_SZ;
        public const int TRAFTX_SZ = Constant.TRAFTX_SZ;
        public const int SRVCTX_SZ = Constant.SRVCTX_SZ;
        public const int STATTX_SZ = Constant.STATTX_SZ;
        public const int POLRX_SZ = Constant.POLRX_SZ;
        public const int EQPTRX_SZ = Constant.ECODE_SZ;
        public const int EQPTURX_SZ = Constant.EQPTURX_SZ;
        public const int TRAFRX_SZ = Constant.TRAFCODE_SZ;
        public const int SRVCRX_SZ = Constant.TRAFCODE_SZ;
        public const int STATRX_SZ = Constant.STATRX_SZ;
        public const int ROUTNUMB_SZ = Constant.ROUTE_SZ;
        public const int SDATE_SZ = Constant.DATE_SZ;
        public const int NOTETX_SZ = Constant.NOTA_SZ;
        public const int NOTERX_SZ = Constant.NOTA_SZ;
        public const int NOTEGNL_SZ = Constant.NOTA_SZ;
        public const int CPOINT_SZ = Constant.CPOINT_SZ;
        public const int FEETX_SZ = Constant.FEETX_SZ;
        public const int FEERX_SZ = Constant.FEERX_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;
        public const int USERID_SZ = Constant.USERID_SZ;


        public const int CALL1 = 0;
        public const int CALL2 = 1;
        public const int BNDCDE = 2;
        public const int SPLAN = 3;
        public const int HL = 4;
        public const int VH = 5;
        public const int CHID = 6;
        public const int FREQTX = 7;
        public const int POLTX = 8;
        public const int ANTNUMBTX1 = 9;
        public const int ANTNUMBTX2 = 10;
        public const int EQPTTX = 11;
        public const int EQPTUTX = 12;
        public const int PWRTX = 13;
        public const int ATPCCDE = 14;
        public const int AFSLTX1 = 15;
        public const int AFSLTX2 = 16;
        public const int TRAFTX = 17;
        public const int SRVCTX = 18;
        public const int STATTX = 19;
        public const int FREQRX = 20;
        public const int POLRX = 21;
        public const int ANTNUMBRX1 = 22;
        public const int ANTNUMBRX2 = 23;
        public const int ANTNUMBRX3 = 24;
        public const int EQPTRX = 25;
        public const int EQPTURX = 26;
        public const int AFSLRX1 = 27;
        public const int AFSLRX2 = 28;
        public const int AFSLRX3 = 29;
        public const int PWRRX1 = 30;
        public const int PWRRX2 = 31;
        public const int PWRRX3 = 32;
        public const int TRAFRX = 33;
        public const int ESINT = 34;
        public const int TSINT = 35;
        public const int SRVCRX = 36;
        public const int STATRX = 37;
        public const int ROUTNUMB = 38;
        public const int STNNUMB = 39;
        public const int HOPNUMB = 40;
        public const int SDATE = 41;
        public const int NOTETX = 42;
        public const int NOTERX = 43;
        public const int NOTEGNL = 44;
        public const int CPOINT = 45;
        public const int FEETX = 46;
        public const int FEERX = 47;
        public const int MDATE = 48;
        public const int MTIME = 49;
        public const int USERID = 50;

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 51;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "call1", "call2", "bndcde", "splan", "hl", "vh", "chid", "freqtx", "poltx", "antnumbtx1", "antnumbtx2", "eqpttx", "eqptutx", "pwrtx", "atpccde", "afsltx1", "afsltx2", "traftx", "srvctx", "stattx", "freqrx", "polrx", "antnumbrx1", "antnumbrx2", "antnumbrx3", "eqptrx", "eqpturx", "afslrx1", "afslrx2", "afslrx3", "pwrrx1", "pwrrx2", "pwrrx3", "trafrx", "esint", "tsint", "srvcrx", "statrx", "routnumb", "stnnumb", "hopnumb", "sdate", "notetx", "noterx", "notegnl", "cpoint", "feetx", "feerx", "mdate", "mtime", "userid" };

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
        static MtChan()
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
        public MtChan()
        {
        }

        //------------------------------------------------------------------

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n===== MtChan =====");

            sb.Append("\ncall1 = " + call1);
            sb.Append("\ncall2 = " + call2);
            sb.Append("\nbndcde = " + bndcde);
            sb.Append("\nsplan = " + splan);
            sb.Append("\nhl = " + hl);
            sb.Append("\nvh = " + vh);
            sb.Append("\nchid = " + chid);
            sb.Append("\nfreqtx = " + freqtx);
            sb.Append("\npoltx = " + poltx);
            sb.Append("\nantnumbtx1 = " + antnumbtx1);
            sb.Append("\nantnumbtx2 = " + antnumbtx2);
            sb.Append("\neqpttx = " + eqpttx);
            sb.Append("\neqptutx = " + eqptutx);
            sb.Append("\npwrtx = " + pwrtx);
            sb.Append("\natpccde = " + atpccde);
            sb.Append("\nafsltx1 = " + afsltx1);
            sb.Append("\nafsltx2 = " + afsltx2);
            sb.Append("\ntraftx = " + traftx);
            sb.Append("\nsrvctx = " + srvctx);
            sb.Append("\nstattx = " + stattx);
            sb.Append("\nfreqrx = " + freqrx);
            sb.Append("\npolrx = " + polrx);
            sb.Append("\nantnumbrx1 = " + antnumbrx1);
            sb.Append("\nantnumbrx2 = " + antnumbrx2);
            sb.Append("\nantnumbrx3 = " + antnumbrx3);
            sb.Append("\neqptrx = " + eqptrx);
            sb.Append("\neqpturx = " + eqpturx);
            sb.Append("\nafslrx1 = " + afslrx1);
            sb.Append("\nafslrx2 = " + afslrx2);
            sb.Append("\nafslrx3 = " + afslrx3);
            sb.Append("\npwrrx1 = " + pwrrx1);
            sb.Append("\npwrrx2 = " + pwrrx2);
            sb.Append("\npwrrx3 = " + pwrrx3);
            sb.Append("\ntrafrx = " + trafrx);
            sb.Append("\nesint = " + esint);
            sb.Append("\ntsint = " + tsint);
            sb.Append("\nsrvcrx = " + srvcrx);
            sb.Append("\nstatrx = " + statrx);
            sb.Append("\nroutnumb = " + routnumb);
            sb.Append("\nstnnumb = " + stnnumb);
            sb.Append("\nhopnumb = " + hopnumb);
            sb.Append("\nsdate = " + sdate);
            sb.Append("\nnotetx = " + notetx);
            sb.Append("\nnoterx = " + noterx);
            sb.Append("\nnotegnl = " + notegnl);
            sb.Append("\ncpoint = " + cpoint);
            sb.Append("\nfeetx = " + feetx);
            sb.Append("\nfeerx = " + feerx);
            sb.Append("\nmdate = " + mdate);
            sb.Append("\nmtime = " + mtime);
            sb.Append("\nuserid = " + userid);

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
            sb.Append("\r\n===== MtChan =====");

            sb.Append("\n" + nullInds[i++] + "      " + "call1 = " + call1);
            sb.Append("\n" + nullInds[i++] + "      " + "call2 = " + call2);
            sb.Append("\n" + nullInds[i++] + "      " + "bndcde = " + bndcde);
            sb.Append("\n" + nullInds[i++] + "      " + "splan = " + splan);
            sb.Append("\n" + nullInds[i++] + "      " + "hl = " + hl);
            sb.Append("\n" + nullInds[i++] + "      " + "vh = " + vh);
            sb.Append("\n" + nullInds[i++] + "      " + "chid = " + chid);
            sb.Append("\n" + nullInds[i++] + "      " + "freqtx = " + freqtx);
            sb.Append("\n" + nullInds[i++] + "      " + "poltx = " + poltx);
            sb.Append("\n" + nullInds[i++] + "      " + "antnumbtx1 = " + antnumbtx1);
            sb.Append("\n" + nullInds[i++] + "      " + "antnumbtx2 = " + antnumbtx2);
            sb.Append("\n" + nullInds[i++] + "      " + "eqpttx = " + eqpttx);
            sb.Append("\n" + nullInds[i++] + "      " + "eqptutx = " + eqptutx);
            sb.Append("\n" + nullInds[i++] + "      " + "pwrtx = " + pwrtx);
            sb.Append("\n" + nullInds[i++] + "      " + "atpccde = " + atpccde);
            sb.Append("\n" + nullInds[i++] + "      " + "afsltx1 = " + afsltx1);
            sb.Append("\n" + nullInds[i++] + "      " + "afsltx2 = " + afsltx2);
            sb.Append("\n" + nullInds[i++] + "      " + "traftx = " + traftx);
            sb.Append("\n" + nullInds[i++] + "      " + "srvctx = " + srvctx);
            sb.Append("\n" + nullInds[i++] + "      " + "stattx = " + stattx);
            sb.Append("\n" + nullInds[i++] + "      " + "freqrx = " + freqrx);
            sb.Append("\n" + nullInds[i++] + "      " + "polrx = " + polrx);
            sb.Append("\n" + nullInds[i++] + "      " + "antnumbrx1 = " + antnumbrx1);
            sb.Append("\n" + nullInds[i++] + "      " + "antnumbrx2 = " + antnumbrx2);
            sb.Append("\n" + nullInds[i++] + "      " + "antnumbrx3 = " + antnumbrx3);
            sb.Append("\n" + nullInds[i++] + "      " + "eqptrx = " + eqptrx);
            sb.Append("\n" + nullInds[i++] + "      " + "eqpturx = " + eqpturx);
            sb.Append("\n" + nullInds[i++] + "      " + "afslrx1 = " + afslrx1);
            sb.Append("\n" + nullInds[i++] + "      " + "afslrx2 = " + afslrx2);
            sb.Append("\n" + nullInds[i++] + "      " + "afslrx3 = " + afslrx3);
            sb.Append("\n" + nullInds[i++] + "      " + "pwrrx1 = " + pwrrx1);
            sb.Append("\n" + nullInds[i++] + "      " + "pwrrx2 = " + pwrrx2);
            sb.Append("\n" + nullInds[i++] + "      " + "pwrrx3 = " + pwrrx3);
            sb.Append("\n" + nullInds[i++] + "      " + "trafrx = " + trafrx);
            sb.Append("\n" + nullInds[i++] + "      " + "esint = " + esint);
            sb.Append("\n" + nullInds[i++] + "      " + "tsint = " + tsint);
            sb.Append("\n" + nullInds[i++] + "      " + "srvcrx = " + srvcrx);
            sb.Append("\n" + nullInds[i++] + "      " + "statrx = " + statrx);
            sb.Append("\n" + nullInds[i++] + "      " + "routnumb = " + routnumb);
            sb.Append("\n" + nullInds[i++] + "      " + "stnnumb = " + stnnumb);
            sb.Append("\n" + nullInds[i++] + "      " + "hopnumb = " + hopnumb);
            sb.Append("\n" + nullInds[i++] + "      " + "sdate = " + sdate);
            sb.Append("\n" + nullInds[i++] + "      " + "notetx = " + notetx);
            sb.Append("\n" + nullInds[i++] + "      " + "noterx = " + noterx);
            sb.Append("\n" + nullInds[i++] + "      " + "notegnl = " + notegnl);
            sb.Append("\n" + nullInds[i++] + "      " + "cpoint = " + cpoint);
            sb.Append("\n" + nullInds[i++] + "      " + "feetx = " + feetx);
            sb.Append("\n" + nullInds[i++] + "      " + "feerx = " + feerx);
            sb.Append("\n" + nullInds[i++] + "      " + "mdate = " + mdate);
            sb.Append("\n" + nullInds[i++] + "      " + "mtime = " + mtime);
            sb.Append("\n" + nullInds[i++] + "      " + "userid = " + userid);

            return sb.ToString();
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
		/// This method supports testing by setting the fields of this object
        /// to predetermined reference values.
        /// </summary>
        /// <param name=""></param>
        public void SetAllColumnsToPrescribedTestValues()
        {
            call1 = "00";    //0
            call2 = "01";    //1
            bndcde = "02";    //2
            splan = "03";    //3
            hl = 4;    //4
            vh = 5;    //5
            chid = "06";    //6
            freqtx = 7.0;    //7
            poltx = "8";    //8
            antnumbtx1 = 9;    //9
            antnumbtx2 = 10;    //10
            eqpttx = "11";    //11
            eqptutx = "C";    //12
            pwrtx = 13.0F;    //13
            atpccde = 14.0F;    //14
            afsltx1 = 15.0F;    //15
            afsltx2 = 16.0F;    //16
            traftx = "17";    //17
            srvctx = "18";    //18
            stattx = "9";    //19
            freqrx = 20.0;    //20
            polrx = "1";    //21
            antnumbrx1 = 22;    //22
            antnumbrx2 = 23;    //23
            antnumbrx3 = 24;    //24
            eqptrx = "25";    //25
            eqpturx = "6";    //26
            afslrx1 = 27.0F;    //27
            afslrx2 = 28.0F;    //28
            afslrx3 = 29.0F;    //29
            pwrrx1 = 30.0F;    //30
            pwrrx2 = 31.0F;    //31
            pwrrx3 = 32.0F;    //32
            trafrx = "33";    //33
            esint = 34.0F;    //34
            tsint = 35.0F;    //35
            srvcrx = "36";    //36
            statrx = "7";    //37
            routnumb = "38";    //38
            stnnumb = 39;    //39
            hopnumb = 40;    //40
            sdate = "41";    //41
            notetx = "42";    //42
            noterx = "43";    //43
            notegnl = "44";    //44
            cpoint = "45";    //45
            feetx = "46";    //46
            feerx = "47";    //47
            mdate = "48";    //48
            mtime = "49";    //49
            userid = "50";    //50
        }

        /// <summary>
		/// This method returns
        /// an array of IntPtr that point to the start addresses of non-contiguous blocks of global
        /// (heap) memory, each of a sufficient size to hold the value of a specific member field 
        /// of this object.
        /// Thus, this method creates the parameter bindings prior to an SQL / ODBC 'update' or 
        /// 'insert' query.
        /// </summary>
        /// <returns></returns>
        public static SQLPOINTER[] AllocateArrayOfSQLPOINTERsToBuffers()
        {
            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[0] = Marshal.AllocHGlobal(CALL1_SZ + 1);  // #0
            parameterValuePtr[1] = Marshal.AllocHGlobal(CALL2_SZ + 1);  // #1
            parameterValuePtr[2] = Marshal.AllocHGlobal(BNDCDE_SZ + 1);  // #2
            parameterValuePtr[3] = Marshal.AllocHGlobal(SPLAN_SZ + 1);  // #3
            parameterValuePtr[4] = Marshal.AllocHGlobal(sizeof(short));  // #4
            parameterValuePtr[5] = Marshal.AllocHGlobal(sizeof(short));  // #5
            parameterValuePtr[6] = Marshal.AllocHGlobal(CHID_SZ + 1);  // #6
            parameterValuePtr[7] = Marshal.AllocHGlobal(sizeof(double));  // #7
            parameterValuePtr[8] = Marshal.AllocHGlobal(POLTX_SZ + 1);  // #8
            parameterValuePtr[9] = Marshal.AllocHGlobal(sizeof(short));  // #9
            parameterValuePtr[10] = Marshal.AllocHGlobal(sizeof(short));  // #10
            parameterValuePtr[11] = Marshal.AllocHGlobal(EQPTTX_SZ + 1);  // #11
            parameterValuePtr[12] = Marshal.AllocHGlobal(EQPTUTX_SZ + 1);  // #12
            parameterValuePtr[13] = Marshal.AllocHGlobal(sizeof(float));  // #13
            parameterValuePtr[14] = Marshal.AllocHGlobal(sizeof(float));  // #14
            parameterValuePtr[15] = Marshal.AllocHGlobal(sizeof(float));  // #15
            parameterValuePtr[16] = Marshal.AllocHGlobal(sizeof(float));  // #16
            parameterValuePtr[17] = Marshal.AllocHGlobal(TRAFTX_SZ + 1);  // #17
            parameterValuePtr[18] = Marshal.AllocHGlobal(SRVCTX_SZ + 1);  // #18
            parameterValuePtr[19] = Marshal.AllocHGlobal(STATTX_SZ + 1);  // #19
            parameterValuePtr[20] = Marshal.AllocHGlobal(sizeof(double));  // #20
            parameterValuePtr[21] = Marshal.AllocHGlobal(POLRX_SZ + 1);  // #21
            parameterValuePtr[22] = Marshal.AllocHGlobal(sizeof(short));  // #22
            parameterValuePtr[23] = Marshal.AllocHGlobal(sizeof(short));  // #23
            parameterValuePtr[24] = Marshal.AllocHGlobal(sizeof(short));  // #24
            parameterValuePtr[25] = Marshal.AllocHGlobal(EQPTRX_SZ + 1);  // #25
            parameterValuePtr[26] = Marshal.AllocHGlobal(EQPTURX_SZ + 1);  // #26
            parameterValuePtr[27] = Marshal.AllocHGlobal(sizeof(float));  // #27
            parameterValuePtr[28] = Marshal.AllocHGlobal(sizeof(float));  // #28
            parameterValuePtr[29] = Marshal.AllocHGlobal(sizeof(float));  // #29
            parameterValuePtr[30] = Marshal.AllocHGlobal(sizeof(float));  // #30
            parameterValuePtr[31] = Marshal.AllocHGlobal(sizeof(float));  // #31
            parameterValuePtr[32] = Marshal.AllocHGlobal(sizeof(float));  // #32
            parameterValuePtr[33] = Marshal.AllocHGlobal(TRAFRX_SZ + 1);  // #33
            parameterValuePtr[34] = Marshal.AllocHGlobal(sizeof(float));  // #34
            parameterValuePtr[35] = Marshal.AllocHGlobal(sizeof(float));  // #35
            parameterValuePtr[36] = Marshal.AllocHGlobal(SRVCRX_SZ + 1);  // #36
            parameterValuePtr[37] = Marshal.AllocHGlobal(STATRX_SZ + 1);  // #37
            parameterValuePtr[38] = Marshal.AllocHGlobal(ROUTNUMB_SZ + 1);  // #38
            parameterValuePtr[39] = Marshal.AllocHGlobal(sizeof(short));  // #39
            parameterValuePtr[40] = Marshal.AllocHGlobal(sizeof(short));  // #40
            parameterValuePtr[41] = Marshal.AllocHGlobal(SDATE_SZ + 1);  // #41
            parameterValuePtr[42] = Marshal.AllocHGlobal(NOTETX_SZ + 1);  // #42
            parameterValuePtr[43] = Marshal.AllocHGlobal(NOTERX_SZ + 1);  // #43
            parameterValuePtr[44] = Marshal.AllocHGlobal(NOTEGNL_SZ + 1);  // #44
            parameterValuePtr[45] = Marshal.AllocHGlobal(CPOINT_SZ + 1);  // #45
            parameterValuePtr[46] = Marshal.AllocHGlobal(FEETX_SZ + 1);  // #46
            parameterValuePtr[47] = Marshal.AllocHGlobal(FEERX_SZ + 1);  // #47
            parameterValuePtr[48] = Marshal.AllocHGlobal(MDATE_SZ + 1);  // #48
            parameterValuePtr[49] = Marshal.AllocHGlobal(MTIME_SZ + 1);  // #49
            parameterValuePtr[50] = Marshal.AllocHGlobal(USERID_SZ + 1);  // #50

            return parameterValuePtr;
        }


        /// <summary>
		/// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned MtChan object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="mtChan"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out MtChan mtChan, out SQLLEN[] nullInds)
        {
            mtChan = new MtChan();
            nullInds = NullHelper.CreateArrayOfNullInd(MtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[CALL1]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.call1 = "";
                nullInds[CALL1] = Constant.DB_NULL;
            }
            else
            {
                mtChan.call1 = Marshal.PtrToStringAnsi(tgtValPtrs[CALL1]).Trim();
                nullInds[CALL1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CALL2]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.call2 = "";
                nullInds[CALL2] = Constant.DB_NULL;
            }
            else
            {
                mtChan.call2 = Marshal.PtrToStringAnsi(tgtValPtrs[CALL2]).Trim();
                nullInds[CALL2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BNDCDE]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.bndcde = "";
                nullInds[BNDCDE] = Constant.DB_NULL;
            }
            else
            {
                mtChan.bndcde = Marshal.PtrToStringAnsi(tgtValPtrs[BNDCDE]).Trim();
                nullInds[BNDCDE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SPLAN]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.splan = "";
                nullInds[SPLAN] = Constant.DB_NULL;
            }
            else
            {
                mtChan.splan = Marshal.PtrToStringAnsi(tgtValPtrs[SPLAN]).Trim();
                nullInds[SPLAN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[HL]);
            if (nullInd != Constant.DB_NULL)
            {
                mtChan.hl = Marshal.ReadInt16(tgtValPtrs[HL]);
                nullInds[HL] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[VH]);
            if (nullInd != Constant.DB_NULL)
            {
                mtChan.vh = Marshal.ReadInt16(tgtValPtrs[VH]);
                nullInds[VH] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CHID]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.chid = "";
                nullInds[CHID] = Constant.DB_NULL;
            }
            else
            {
                mtChan.chid = Marshal.PtrToStringAnsi(tgtValPtrs[CHID]).Trim();
                nullInds[CHID] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[FREQTX]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[FREQTX], D, 0, 1);
                mtChan.freqtx = D[0];
                nullInds[FREQTX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[POLTX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.poltx = "";
                nullInds[POLTX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.poltx = Marshal.PtrToStringAnsi(tgtValPtrs[POLTX]).Trim();
                nullInds[POLTX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANTNUMBTX1]);
            if (nullInd != Constant.DB_NULL)
            {
                mtChan.antnumbtx1 = Marshal.ReadInt16(tgtValPtrs[ANTNUMBTX1]);
                nullInds[ANTNUMBTX1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANTNUMBTX2]);
            if (nullInd != Constant.DB_NULL)
            {
                mtChan.antnumbtx2 = Marshal.ReadInt16(tgtValPtrs[ANTNUMBTX2]);
                nullInds[ANTNUMBTX2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EQPTTX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.eqpttx = "";
                nullInds[EQPTTX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.eqpttx = Marshal.PtrToStringAnsi(tgtValPtrs[EQPTTX]).Trim();
                nullInds[EQPTTX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EQPTUTX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.eqptutx = "";
                nullInds[EQPTUTX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.eqptutx = Marshal.PtrToStringAnsi(tgtValPtrs[EQPTUTX]).Trim();
                nullInds[EQPTUTX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[PWRTX]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[PWRTX], F, 0, 1);
                mtChan.pwrtx = F[0];
                nullInds[PWRTX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ATPCCDE]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ATPCCDE], F, 0, 1);
                mtChan.atpccde = F[0];
                nullInds[ATPCCDE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AFSLTX1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[AFSLTX1], F, 0, 1);
                mtChan.afsltx1 = F[0];
                nullInds[AFSLTX1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AFSLTX2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[AFSLTX2], F, 0, 1);
                mtChan.afsltx2 = F[0];
                nullInds[AFSLTX2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TRAFTX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.traftx = "";
                nullInds[TRAFTX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.traftx = Marshal.PtrToStringAnsi(tgtValPtrs[TRAFTX]).Trim();
                nullInds[TRAFTX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SRVCTX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.srvctx = "";
                nullInds[SRVCTX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.srvctx = Marshal.PtrToStringAnsi(tgtValPtrs[SRVCTX]).Trim();
                nullInds[SRVCTX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[STATTX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.stattx = "";
                nullInds[STATTX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.stattx = Marshal.PtrToStringAnsi(tgtValPtrs[STATTX]).Trim();
                nullInds[STATTX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[FREQRX]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[FREQRX], D, 0, 1);
                mtChan.freqrx = D[0];
                nullInds[FREQRX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[POLRX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.polrx = "";
                nullInds[POLRX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.polrx = Marshal.PtrToStringAnsi(tgtValPtrs[POLRX]).Trim();
                nullInds[POLRX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANTNUMBRX1]);
            if (nullInd != Constant.DB_NULL)
            {
                mtChan.antnumbrx1 = Marshal.ReadInt16(tgtValPtrs[ANTNUMBRX1]);
                nullInds[ANTNUMBRX1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANTNUMBRX2]);
            if (nullInd != Constant.DB_NULL)
            {
                mtChan.antnumbrx2 = Marshal.ReadInt16(tgtValPtrs[ANTNUMBRX2]);
                nullInds[ANTNUMBRX2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANTNUMBRX3]);
            if (nullInd != Constant.DB_NULL)
            {
                mtChan.antnumbrx3 = Marshal.ReadInt16(tgtValPtrs[ANTNUMBRX3]);
                nullInds[ANTNUMBRX3] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EQPTRX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.eqptrx = "";
                nullInds[EQPTRX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.eqptrx = Marshal.PtrToStringAnsi(tgtValPtrs[EQPTRX]).Trim();
                nullInds[EQPTRX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EQPTURX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.eqpturx = "";
                nullInds[EQPTURX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.eqpturx = Marshal.PtrToStringAnsi(tgtValPtrs[EQPTURX]).Trim();
                nullInds[EQPTURX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AFSLRX1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[AFSLRX1], F, 0, 1);
                mtChan.afslrx1 = F[0];
                nullInds[AFSLRX1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AFSLRX2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[AFSLRX2], F, 0, 1);
                mtChan.afslrx2 = F[0];
                nullInds[AFSLRX2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AFSLRX3]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[AFSLRX3], F, 0, 1);
                mtChan.afslrx3 = F[0];
                nullInds[AFSLRX3] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[PWRRX1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[PWRRX1], F, 0, 1);
                mtChan.pwrrx1 = F[0];
                nullInds[PWRRX1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[PWRRX2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[PWRRX2], F, 0, 1);
                mtChan.pwrrx2 = F[0];
                nullInds[PWRRX2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[PWRRX3]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[PWRRX3], F, 0, 1);
                mtChan.pwrrx3 = F[0];
                nullInds[PWRRX3] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TRAFRX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.trafrx = "";
                nullInds[TRAFRX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.trafrx = Marshal.PtrToStringAnsi(tgtValPtrs[TRAFRX]).Trim();
                nullInds[TRAFRX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ESINT]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ESINT], F, 0, 1);
                mtChan.esint = F[0];
                nullInds[ESINT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TSINT]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[TSINT], F, 0, 1);
                mtChan.tsint = F[0];
                nullInds[TSINT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SRVCRX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.srvcrx = "";
                nullInds[SRVCRX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.srvcrx = Marshal.PtrToStringAnsi(tgtValPtrs[SRVCRX]).Trim();
                nullInds[SRVCRX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[STATRX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.statrx = "";
                nullInds[STATRX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.statrx = Marshal.PtrToStringAnsi(tgtValPtrs[STATRX]).Trim();
                nullInds[STATRX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ROUTNUMB]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.routnumb = "";
                nullInds[ROUTNUMB] = Constant.DB_NULL;
            }
            else
            {
                mtChan.routnumb = Marshal.PtrToStringAnsi(tgtValPtrs[ROUTNUMB]).Trim();
                nullInds[ROUTNUMB] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[STNNUMB]);
            if (nullInd != Constant.DB_NULL)
            {
                mtChan.stnnumb = Marshal.ReadInt16(tgtValPtrs[STNNUMB]);
                nullInds[STNNUMB] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[HOPNUMB]);
            if (nullInd != Constant.DB_NULL)
            {
                mtChan.hopnumb = Marshal.ReadInt16(tgtValPtrs[HOPNUMB]);
                nullInds[HOPNUMB] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.sdate = "";
                nullInds[SDATE] = Constant.DB_NULL;
            }
            else
            {
                mtChan.sdate = Marshal.PtrToStringAnsi(tgtValPtrs[SDATE]).Trim();
                nullInds[SDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[NOTETX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.notetx = "";
                nullInds[NOTETX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.notetx = Marshal.PtrToStringAnsi(tgtValPtrs[NOTETX]).Trim();
                nullInds[NOTETX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[NOTERX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.noterx = "";
                nullInds[NOTERX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.noterx = Marshal.PtrToStringAnsi(tgtValPtrs[NOTERX]).Trim();
                nullInds[NOTERX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[NOTEGNL]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.notegnl = "";
                nullInds[NOTEGNL] = Constant.DB_NULL;
            }
            else
            {
                mtChan.notegnl = Marshal.PtrToStringAnsi(tgtValPtrs[NOTEGNL]).Trim();
                nullInds[NOTEGNL] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CPOINT]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.cpoint = "";
                nullInds[CPOINT] = Constant.DB_NULL;
            }
            else
            {
                mtChan.cpoint = Marshal.PtrToStringAnsi(tgtValPtrs[CPOINT]).Trim();
                nullInds[CPOINT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[FEETX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.feetx = "";
                nullInds[FEETX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.feetx = Marshal.PtrToStringAnsi(tgtValPtrs[FEETX]).Trim();
                nullInds[FEETX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[FEERX]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.feerx = "";
                nullInds[FEERX] = Constant.DB_NULL;
            }
            else
            {
                mtChan.feerx = Marshal.PtrToStringAnsi(tgtValPtrs[FEERX]).Trim();
                nullInds[FEERX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                mtChan.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                mtChan.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[USERID]);
            if (nullInd == Constant.DB_NULL)
            {
                mtChan.userid = "";
                nullInds[USERID] = Constant.DB_NULL;
            }
            else
            {
                mtChan.userid = Marshal.PtrToStringAnsi(tgtValPtrs[USERID]).Trim();
                nullInds[USERID] = Constant.DB_NOT_NULL;
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
            Bind.BindBufferToString(hStmt, CALL1 + 1, tgtValPtrs[CALL1], MtChan.CALL1_SZ, nullIndPtrs[CALL1]);

            tgtValPtrs[CALL2] = Marshal.AllocHGlobal(CALL2_SZ + 1);
            nullIndPtrs[CALL2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CALL2 + 1, tgtValPtrs[CALL2], MtChan.CALL2_SZ, nullIndPtrs[CALL2]);

            tgtValPtrs[BNDCDE] = Marshal.AllocHGlobal(BNDCDE_SZ + 1);
            nullIndPtrs[BNDCDE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, BNDCDE + 1, tgtValPtrs[BNDCDE], MtChan.BNDCDE_SZ, nullIndPtrs[BNDCDE]);

            tgtValPtrs[SPLAN] = Marshal.AllocHGlobal(SPLAN_SZ + 1);
            nullIndPtrs[SPLAN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SPLAN + 1, tgtValPtrs[SPLAN], MtChan.SPLAN_SZ, nullIndPtrs[SPLAN]);

            tgtValPtrs[HL] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[HL] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, HL + 1, tgtValPtrs[HL], nullIndPtrs[HL]);

            tgtValPtrs[VH] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[VH] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, VH + 1, tgtValPtrs[VH], nullIndPtrs[VH]);

            tgtValPtrs[CHID] = Marshal.AllocHGlobal(CHID_SZ + 1);
            nullIndPtrs[CHID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CHID + 1, tgtValPtrs[CHID], MtChan.CHID_SZ, nullIndPtrs[CHID]);

            tgtValPtrs[FREQTX] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[FREQTX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, FREQTX + 1, tgtValPtrs[FREQTX], nullIndPtrs[FREQTX]);

            tgtValPtrs[POLTX] = Marshal.AllocHGlobal(POLTX_SZ + 1);
            nullIndPtrs[POLTX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, POLTX + 1, tgtValPtrs[POLTX], MtChan.POLTX_SZ, nullIndPtrs[POLTX]);

            tgtValPtrs[ANTNUMBTX1] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[ANTNUMBTX1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, ANTNUMBTX1 + 1, tgtValPtrs[ANTNUMBTX1], nullIndPtrs[ANTNUMBTX1]);

            tgtValPtrs[ANTNUMBTX2] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[ANTNUMBTX2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, ANTNUMBTX2 + 1, tgtValPtrs[ANTNUMBTX2], nullIndPtrs[ANTNUMBTX2]);

            tgtValPtrs[EQPTTX] = Marshal.AllocHGlobal(EQPTTX_SZ + 1);
            nullIndPtrs[EQPTTX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EQPTTX + 1, tgtValPtrs[EQPTTX], MtChan.EQPTTX_SZ, nullIndPtrs[EQPTTX]);

            tgtValPtrs[EQPTUTX] = Marshal.AllocHGlobal(EQPTUTX_SZ + 1);
            nullIndPtrs[EQPTUTX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EQPTUTX + 1, tgtValPtrs[EQPTUTX], MtChan.EQPTUTX_SZ, nullIndPtrs[EQPTUTX]);

            tgtValPtrs[PWRTX] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[PWRTX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, PWRTX + 1, tgtValPtrs[PWRTX], nullIndPtrs[PWRTX]);

            tgtValPtrs[ATPCCDE] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[ATPCCDE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, ATPCCDE + 1, tgtValPtrs[ATPCCDE], nullIndPtrs[ATPCCDE]);

            tgtValPtrs[AFSLTX1] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[AFSLTX1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, AFSLTX1 + 1, tgtValPtrs[AFSLTX1], nullIndPtrs[AFSLTX1]);

            tgtValPtrs[AFSLTX2] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[AFSLTX2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, AFSLTX2 + 1, tgtValPtrs[AFSLTX2], nullIndPtrs[AFSLTX2]);

            tgtValPtrs[TRAFTX] = Marshal.AllocHGlobal(TRAFTX_SZ + 1);
            nullIndPtrs[TRAFTX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TRAFTX + 1, tgtValPtrs[TRAFTX], MtChan.TRAFTX_SZ, nullIndPtrs[TRAFTX]);

            tgtValPtrs[SRVCTX] = Marshal.AllocHGlobal(SRVCTX_SZ + 1);
            nullIndPtrs[SRVCTX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SRVCTX + 1, tgtValPtrs[SRVCTX], MtChan.SRVCTX_SZ, nullIndPtrs[SRVCTX]);

            tgtValPtrs[STATTX] = Marshal.AllocHGlobal(STATTX_SZ + 1);
            nullIndPtrs[STATTX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, STATTX + 1, tgtValPtrs[STATTX], MtChan.STATTX_SZ, nullIndPtrs[STATTX]);

            tgtValPtrs[FREQRX] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[FREQRX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, FREQRX + 1, tgtValPtrs[FREQRX], nullIndPtrs[FREQRX]);

            tgtValPtrs[POLRX] = Marshal.AllocHGlobal(POLRX_SZ + 1);
            nullIndPtrs[POLRX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, POLRX + 1, tgtValPtrs[POLRX], MtChan.POLRX_SZ, nullIndPtrs[POLRX]);

            tgtValPtrs[ANTNUMBRX1] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[ANTNUMBRX1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, ANTNUMBRX1 + 1, tgtValPtrs[ANTNUMBRX1], nullIndPtrs[ANTNUMBRX1]);

            tgtValPtrs[ANTNUMBRX2] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[ANTNUMBRX2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, ANTNUMBRX2 + 1, tgtValPtrs[ANTNUMBRX2], nullIndPtrs[ANTNUMBRX2]);

            tgtValPtrs[ANTNUMBRX3] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[ANTNUMBRX3] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, ANTNUMBRX3 + 1, tgtValPtrs[ANTNUMBRX3], nullIndPtrs[ANTNUMBRX3]);

            tgtValPtrs[EQPTRX] = Marshal.AllocHGlobal(EQPTRX_SZ + 1);
            nullIndPtrs[EQPTRX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EQPTRX + 1, tgtValPtrs[EQPTRX], MtChan.EQPTRX_SZ, nullIndPtrs[EQPTRX]);

            tgtValPtrs[EQPTURX] = Marshal.AllocHGlobal(EQPTURX_SZ + 1);
            nullIndPtrs[EQPTURX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EQPTURX + 1, tgtValPtrs[EQPTURX], MtChan.EQPTURX_SZ, nullIndPtrs[EQPTURX]);

            tgtValPtrs[AFSLRX1] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[AFSLRX1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, AFSLRX1 + 1, tgtValPtrs[AFSLRX1], nullIndPtrs[AFSLRX1]);

            tgtValPtrs[AFSLRX2] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[AFSLRX2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, AFSLRX2 + 1, tgtValPtrs[AFSLRX2], nullIndPtrs[AFSLRX2]);

            tgtValPtrs[AFSLRX3] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[AFSLRX3] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, AFSLRX3 + 1, tgtValPtrs[AFSLRX3], nullIndPtrs[AFSLRX3]);

            tgtValPtrs[PWRRX1] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[PWRRX1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, PWRRX1 + 1, tgtValPtrs[PWRRX1], nullIndPtrs[PWRRX1]);

            tgtValPtrs[PWRRX2] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[PWRRX2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, PWRRX2 + 1, tgtValPtrs[PWRRX2], nullIndPtrs[PWRRX2]);

            tgtValPtrs[PWRRX3] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[PWRRX3] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, PWRRX3 + 1, tgtValPtrs[PWRRX3], nullIndPtrs[PWRRX3]);

            tgtValPtrs[TRAFRX] = Marshal.AllocHGlobal(TRAFRX_SZ + 1);
            nullIndPtrs[TRAFRX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TRAFRX + 1, tgtValPtrs[TRAFRX], MtChan.TRAFRX_SZ, nullIndPtrs[TRAFRX]);

            tgtValPtrs[ESINT] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[ESINT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, ESINT + 1, tgtValPtrs[ESINT], nullIndPtrs[ESINT]);

            tgtValPtrs[TSINT] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[TSINT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, TSINT + 1, tgtValPtrs[TSINT], nullIndPtrs[TSINT]);

            tgtValPtrs[SRVCRX] = Marshal.AllocHGlobal(SRVCRX_SZ + 1);
            nullIndPtrs[SRVCRX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SRVCRX + 1, tgtValPtrs[SRVCRX], MtChan.SRVCRX_SZ, nullIndPtrs[SRVCRX]);

            tgtValPtrs[STATRX] = Marshal.AllocHGlobal(STATRX_SZ + 1);
            nullIndPtrs[STATRX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, STATRX + 1, tgtValPtrs[STATRX], MtChan.STATRX_SZ, nullIndPtrs[STATRX]);

            tgtValPtrs[ROUTNUMB] = Marshal.AllocHGlobal(ROUTNUMB_SZ + 1);
            nullIndPtrs[ROUTNUMB] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ROUTNUMB + 1, tgtValPtrs[ROUTNUMB], MtChan.ROUTNUMB_SZ, nullIndPtrs[ROUTNUMB]);

            tgtValPtrs[STNNUMB] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[STNNUMB] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, STNNUMB + 1, tgtValPtrs[STNNUMB], nullIndPtrs[STNNUMB]);

            tgtValPtrs[HOPNUMB] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[HOPNUMB] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, HOPNUMB + 1, tgtValPtrs[HOPNUMB], nullIndPtrs[HOPNUMB]);

            tgtValPtrs[SDATE] = Marshal.AllocHGlobal(SDATE_SZ + 1);
            nullIndPtrs[SDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SDATE + 1, tgtValPtrs[SDATE], MtChan.SDATE_SZ, nullIndPtrs[SDATE]);

            tgtValPtrs[NOTETX] = Marshal.AllocHGlobal(NOTETX_SZ + 1);
            nullIndPtrs[NOTETX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, NOTETX + 1, tgtValPtrs[NOTETX], MtChan.NOTETX_SZ, nullIndPtrs[NOTETX]);

            tgtValPtrs[NOTERX] = Marshal.AllocHGlobal(NOTERX_SZ + 1);
            nullIndPtrs[NOTERX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, NOTERX + 1, tgtValPtrs[NOTERX], MtChan.NOTERX_SZ, nullIndPtrs[NOTERX]);

            tgtValPtrs[NOTEGNL] = Marshal.AllocHGlobal(NOTEGNL_SZ + 1);
            nullIndPtrs[NOTEGNL] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, NOTEGNL + 1, tgtValPtrs[NOTEGNL], MtChan.NOTEGNL_SZ, nullIndPtrs[NOTEGNL]);

            tgtValPtrs[CPOINT] = Marshal.AllocHGlobal(CPOINT_SZ + 1);
            nullIndPtrs[CPOINT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CPOINT + 1, tgtValPtrs[CPOINT], MtChan.CPOINT_SZ, nullIndPtrs[CPOINT]);

            tgtValPtrs[FEETX] = Marshal.AllocHGlobal(FEETX_SZ + 1);
            nullIndPtrs[FEETX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, FEETX + 1, tgtValPtrs[FEETX], MtChan.FEETX_SZ, nullIndPtrs[FEETX]);

            tgtValPtrs[FEERX] = Marshal.AllocHGlobal(FEERX_SZ + 1);
            nullIndPtrs[FEERX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, FEERX + 1, tgtValPtrs[FEERX], MtChan.FEERX_SZ, nullIndPtrs[FEERX]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], MtChan.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], MtChan.MTIME_SZ, nullIndPtrs[MTIME]);

            tgtValPtrs[USERID] = Marshal.AllocHGlobal(USERID_SZ + 1);
            nullIndPtrs[USERID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, USERID + 1, tgtValPtrs[USERID], MtChan.USERID_SZ, nullIndPtrs[USERID]);
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

            sb.Append(nullInds[MtChan.CALL1] == Constant.DB_NULL ? "NULL," : "'" + call1.ToString() + "',");
            sb.Append(nullInds[MtChan.CALL2] == Constant.DB_NULL ? "NULL," : "'" + call2.ToString() + "',");
            sb.Append(nullInds[MtChan.BNDCDE] == Constant.DB_NULL ? "NULL," : "'" + bndcde.ToString() + "',");
            sb.Append(nullInds[MtChan.SPLAN] == Constant.DB_NULL ? "NULL," : "'" + splan.ToString() + "',");
            sb.Append(nullInds[MtChan.HL] == Constant.DB_NULL ? "NULL," : "'" + hl.ToString() + "',");
            sb.Append(nullInds[MtChan.VH] == Constant.DB_NULL ? "NULL," : "'" + vh.ToString() + "',");
            sb.Append(nullInds[MtChan.CHID] == Constant.DB_NULL ? "NULL," : "'" + chid.ToString() + "',");
            sb.Append(nullInds[MtChan.FREQTX] == Constant.DB_NULL ? "NULL," : "'" + freqtx.ToString() + "',");
            sb.Append(nullInds[MtChan.POLTX] == Constant.DB_NULL ? "NULL," : "'" + poltx.ToString() + "',");
            sb.Append(nullInds[MtChan.ANTNUMBTX1] == Constant.DB_NULL ? "NULL," : "'" + antnumbtx1.ToString() + "',");
            sb.Append(nullInds[MtChan.ANTNUMBTX2] == Constant.DB_NULL ? "NULL," : "'" + antnumbtx2.ToString() + "',");
            sb.Append(nullInds[MtChan.EQPTTX] == Constant.DB_NULL ? "NULL," : "'" + eqpttx.ToString() + "',");
            sb.Append(nullInds[MtChan.EQPTUTX] == Constant.DB_NULL ? "NULL," : "'" + eqptutx.ToString() + "',");
            sb.Append(nullInds[MtChan.PWRTX] == Constant.DB_NULL ? "NULL," : "'" + pwrtx.ToString() + "',");
            sb.Append(nullInds[MtChan.ATPCCDE] == Constant.DB_NULL ? "NULL," : "'" + atpccde.ToString() + "',");
            sb.Append(nullInds[MtChan.AFSLTX1] == Constant.DB_NULL ? "NULL," : "'" + afsltx1.ToString() + "',");
            sb.Append(nullInds[MtChan.AFSLTX2] == Constant.DB_NULL ? "NULL," : "'" + afsltx2.ToString() + "',");
            sb.Append(nullInds[MtChan.TRAFTX] == Constant.DB_NULL ? "NULL," : "'" + traftx.ToString() + "',");
            sb.Append(nullInds[MtChan.SRVCTX] == Constant.DB_NULL ? "NULL," : "'" + srvctx.ToString() + "',");
            sb.Append(nullInds[MtChan.STATTX] == Constant.DB_NULL ? "NULL," : "'" + stattx.ToString() + "',");
            sb.Append(nullInds[MtChan.FREQRX] == Constant.DB_NULL ? "NULL," : "'" + freqrx.ToString() + "',");
            sb.Append(nullInds[MtChan.POLRX] == Constant.DB_NULL ? "NULL," : "'" + polrx.ToString() + "',");
            sb.Append(nullInds[MtChan.ANTNUMBRX1] == Constant.DB_NULL ? "NULL," : "'" + antnumbrx1.ToString() + "',");
            sb.Append(nullInds[MtChan.ANTNUMBRX2] == Constant.DB_NULL ? "NULL," : "'" + antnumbrx2.ToString() + "',");
            sb.Append(nullInds[MtChan.ANTNUMBRX3] == Constant.DB_NULL ? "NULL," : "'" + antnumbrx3.ToString() + "',");
            sb.Append(nullInds[MtChan.EQPTRX] == Constant.DB_NULL ? "NULL," : "'" + eqptrx.ToString() + "',");
            sb.Append(nullInds[MtChan.EQPTURX] == Constant.DB_NULL ? "NULL," : "'" + eqpturx.ToString() + "',");
            sb.Append(nullInds[MtChan.AFSLRX1] == Constant.DB_NULL ? "NULL," : "'" + afslrx1.ToString() + "',");
            sb.Append(nullInds[MtChan.AFSLRX2] == Constant.DB_NULL ? "NULL," : "'" + afslrx2.ToString() + "',");
            sb.Append(nullInds[MtChan.AFSLRX3] == Constant.DB_NULL ? "NULL," : "'" + afslrx3.ToString() + "',");
            sb.Append(nullInds[MtChan.PWRRX1] == Constant.DB_NULL ? "NULL," : "'" + pwrrx1.ToString() + "',");
            sb.Append(nullInds[MtChan.PWRRX2] == Constant.DB_NULL ? "NULL," : "'" + pwrrx2.ToString() + "',");
            sb.Append(nullInds[MtChan.PWRRX3] == Constant.DB_NULL ? "NULL," : "'" + pwrrx3.ToString() + "',");
            sb.Append(nullInds[MtChan.TRAFRX] == Constant.DB_NULL ? "NULL," : "'" + trafrx.ToString() + "',");
            sb.Append(nullInds[MtChan.ESINT] == Constant.DB_NULL ? "NULL," : "'" + esint.ToString() + "',");
            sb.Append(nullInds[MtChan.TSINT] == Constant.DB_NULL ? "NULL," : "'" + tsint.ToString() + "',");
            sb.Append(nullInds[MtChan.SRVCRX] == Constant.DB_NULL ? "NULL," : "'" + srvcrx.ToString() + "',");
            sb.Append(nullInds[MtChan.STATRX] == Constant.DB_NULL ? "NULL," : "'" + statrx.ToString() + "',");
            sb.Append(nullInds[MtChan.ROUTNUMB] == Constant.DB_NULL ? "NULL," : "'" + routnumb.ToString() + "',");
            sb.Append(nullInds[MtChan.STNNUMB] == Constant.DB_NULL ? "NULL," : "'" + stnnumb.ToString() + "',");
            sb.Append(nullInds[MtChan.HOPNUMB] == Constant.DB_NULL ? "NULL," : "'" + hopnumb.ToString() + "',");
            sb.Append(nullInds[MtChan.SDATE] == Constant.DB_NULL ? "NULL," : "'" + sdate.ToString() + "',");
            sb.Append(nullInds[MtChan.NOTETX] == Constant.DB_NULL ? "NULL," : "'" + notetx.ToString() + "',");
            sb.Append(nullInds[MtChan.NOTERX] == Constant.DB_NULL ? "NULL," : "'" + noterx.ToString() + "',");
            sb.Append(nullInds[MtChan.NOTEGNL] == Constant.DB_NULL ? "NULL," : "'" + notegnl.ToString() + "',");
            sb.Append(nullInds[MtChan.CPOINT] == Constant.DB_NULL ? "NULL," : "'" + cpoint.ToString() + "',");
            sb.Append(nullInds[MtChan.FEETX] == Constant.DB_NULL ? "NULL," : "'" + feetx.ToString() + "',");
            sb.Append(nullInds[MtChan.FEERX] == Constant.DB_NULL ? "NULL," : "'" + feerx.ToString() + "',");
            sb.Append(nullInds[MtChan.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',");
            sb.Append(nullInds[MtChan.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "',");
            sb.Append(nullInds[MtChan.USERID] == Constant.DB_NULL ? "NULL" : "'" + userid.ToString() + "'");


            return sb.ToString();
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

            sb.Append(columnNames[MtChan.CALL1] + "=" + (nullInds[MtChan.CALL1] == Constant.DB_NULL ? "NULL," : "'" + call1.ToString() + "',"));
            sb.Append(columnNames[MtChan.CALL2] + "=" + (nullInds[MtChan.CALL2] == Constant.DB_NULL ? "NULL," : "'" + call2.ToString() + "',"));
            sb.Append(columnNames[MtChan.BNDCDE] + "=" + (nullInds[MtChan.BNDCDE] == Constant.DB_NULL ? "NULL," : "'" + bndcde.ToString() + "',"));
            sb.Append(columnNames[MtChan.SPLAN] + "=" + (nullInds[MtChan.SPLAN] == Constant.DB_NULL ? "NULL," : "'" + splan.ToString() + "',"));
            sb.Append(columnNames[MtChan.HL] + "=" + (nullInds[MtChan.HL] == Constant.DB_NULL ? "NULL," : "'" + hl.ToString() + "',"));
            sb.Append(columnNames[MtChan.VH] + "=" + (nullInds[MtChan.VH] == Constant.DB_NULL ? "NULL," : "'" + vh.ToString() + "',"));
            sb.Append(columnNames[MtChan.CHID] + "=" + (nullInds[MtChan.CHID] == Constant.DB_NULL ? "NULL," : "'" + chid.ToString() + "',"));
            sb.Append(columnNames[MtChan.FREQTX] + "=" + (nullInds[MtChan.FREQTX] == Constant.DB_NULL ? "NULL," : "'" + freqtx.ToString() + "',"));
            sb.Append(columnNames[MtChan.POLTX] + "=" + (nullInds[MtChan.POLTX] == Constant.DB_NULL ? "NULL," : "'" + poltx.ToString() + "',"));
            sb.Append(columnNames[MtChan.ANTNUMBTX1] + "=" + (nullInds[MtChan.ANTNUMBTX1] == Constant.DB_NULL ? "NULL," : "'" + antnumbtx1.ToString() + "',"));
            sb.Append(columnNames[MtChan.ANTNUMBTX2] + "=" + (nullInds[MtChan.ANTNUMBTX2] == Constant.DB_NULL ? "NULL," : "'" + antnumbtx2.ToString() + "',"));
            sb.Append(columnNames[MtChan.EQPTTX] + "=" + (nullInds[MtChan.EQPTTX] == Constant.DB_NULL ? "NULL," : "'" + eqpttx.ToString() + "',"));
            sb.Append(columnNames[MtChan.EQPTUTX] + "=" + (nullInds[MtChan.EQPTUTX] == Constant.DB_NULL ? "NULL," : "'" + eqptutx.ToString() + "',"));
            sb.Append(columnNames[MtChan.PWRTX] + "=" + (nullInds[MtChan.PWRTX] == Constant.DB_NULL ? "NULL," : "'" + pwrtx.ToString() + "',"));
            sb.Append(columnNames[MtChan.ATPCCDE] + "=" + (nullInds[MtChan.ATPCCDE] == Constant.DB_NULL ? "NULL," : "'" + atpccde.ToString() + "',"));
            sb.Append(columnNames[MtChan.AFSLTX1] + "=" + (nullInds[MtChan.AFSLTX1] == Constant.DB_NULL ? "NULL," : "'" + afsltx1.ToString() + "',"));
            sb.Append(columnNames[MtChan.AFSLTX2] + "=" + (nullInds[MtChan.AFSLTX2] == Constant.DB_NULL ? "NULL," : "'" + afsltx2.ToString() + "',"));
            sb.Append(columnNames[MtChan.TRAFTX] + "=" + (nullInds[MtChan.TRAFTX] == Constant.DB_NULL ? "NULL," : "'" + traftx.ToString() + "',"));
            sb.Append(columnNames[MtChan.SRVCTX] + "=" + (nullInds[MtChan.SRVCTX] == Constant.DB_NULL ? "NULL," : "'" + srvctx.ToString() + "',"));
            sb.Append(columnNames[MtChan.STATTX] + "=" + (nullInds[MtChan.STATTX] == Constant.DB_NULL ? "NULL," : "'" + stattx.ToString() + "',"));
            sb.Append(columnNames[MtChan.FREQRX] + "=" + (nullInds[MtChan.FREQRX] == Constant.DB_NULL ? "NULL," : "'" + freqrx.ToString() + "',"));
            sb.Append(columnNames[MtChan.POLRX] + "=" + (nullInds[MtChan.POLRX] == Constant.DB_NULL ? "NULL," : "'" + polrx.ToString() + "',"));
            sb.Append(columnNames[MtChan.ANTNUMBRX1] + "=" + (nullInds[MtChan.ANTNUMBRX1] == Constant.DB_NULL ? "NULL," : "'" + antnumbrx1.ToString() + "',"));
            sb.Append(columnNames[MtChan.ANTNUMBRX2] + "=" + (nullInds[MtChan.ANTNUMBRX2] == Constant.DB_NULL ? "NULL," : "'" + antnumbrx2.ToString() + "',"));
            sb.Append(columnNames[MtChan.ANTNUMBRX3] + "=" + (nullInds[MtChan.ANTNUMBRX3] == Constant.DB_NULL ? "NULL," : "'" + antnumbrx3.ToString() + "',"));
            sb.Append(columnNames[MtChan.EQPTRX] + "=" + (nullInds[MtChan.EQPTRX] == Constant.DB_NULL ? "NULL," : "'" + eqptrx.ToString() + "',"));
            sb.Append(columnNames[MtChan.EQPTURX] + "=" + (nullInds[MtChan.EQPTURX] == Constant.DB_NULL ? "NULL," : "'" + eqpturx.ToString() + "',"));
            sb.Append(columnNames[MtChan.AFSLRX1] + "=" + (nullInds[MtChan.AFSLRX1] == Constant.DB_NULL ? "NULL," : "'" + afslrx1.ToString() + "',"));
            sb.Append(columnNames[MtChan.AFSLRX2] + "=" + (nullInds[MtChan.AFSLRX2] == Constant.DB_NULL ? "NULL," : "'" + afslrx2.ToString() + "',"));
            sb.Append(columnNames[MtChan.AFSLRX3] + "=" + (nullInds[MtChan.AFSLRX3] == Constant.DB_NULL ? "NULL," : "'" + afslrx3.ToString() + "',"));
            sb.Append(columnNames[MtChan.PWRRX1] + "=" + (nullInds[MtChan.PWRRX1] == Constant.DB_NULL ? "NULL," : "'" + pwrrx1.ToString() + "',"));
            sb.Append(columnNames[MtChan.PWRRX2] + "=" + (nullInds[MtChan.PWRRX2] == Constant.DB_NULL ? "NULL," : "'" + pwrrx2.ToString() + "',"));
            sb.Append(columnNames[MtChan.PWRRX3] + "=" + (nullInds[MtChan.PWRRX3] == Constant.DB_NULL ? "NULL," : "'" + pwrrx3.ToString() + "',"));
            sb.Append(columnNames[MtChan.TRAFRX] + "=" + (nullInds[MtChan.TRAFRX] == Constant.DB_NULL ? "NULL," : "'" + trafrx.ToString() + "',"));
            sb.Append(columnNames[MtChan.ESINT] + "=" + (nullInds[MtChan.ESINT] == Constant.DB_NULL ? "NULL," : "'" + esint.ToString() + "',"));
            sb.Append(columnNames[MtChan.TSINT] + "=" + (nullInds[MtChan.TSINT] == Constant.DB_NULL ? "NULL," : "'" + tsint.ToString() + "',"));
            sb.Append(columnNames[MtChan.SRVCRX] + "=" + (nullInds[MtChan.SRVCRX] == Constant.DB_NULL ? "NULL," : "'" + srvcrx.ToString() + "',"));
            sb.Append(columnNames[MtChan.STATRX] + "=" + (nullInds[MtChan.STATRX] == Constant.DB_NULL ? "NULL," : "'" + statrx.ToString() + "',"));
            sb.Append(columnNames[MtChan.ROUTNUMB] + "=" + (nullInds[MtChan.ROUTNUMB] == Constant.DB_NULL ? "NULL," : "'" + routnumb.ToString() + "',"));
            sb.Append(columnNames[MtChan.STNNUMB] + "=" + (nullInds[MtChan.STNNUMB] == Constant.DB_NULL ? "NULL," : "'" + stnnumb.ToString() + "',"));
            sb.Append(columnNames[MtChan.HOPNUMB] + "=" + (nullInds[MtChan.HOPNUMB] == Constant.DB_NULL ? "NULL," : "'" + hopnumb.ToString() + "',"));
            sb.Append(columnNames[MtChan.SDATE] + "=" + (nullInds[MtChan.SDATE] == Constant.DB_NULL ? "NULL," : "'" + sdate.ToString() + "',"));
            sb.Append(columnNames[MtChan.NOTETX] + "=" + (nullInds[MtChan.NOTETX] == Constant.DB_NULL ? "NULL," : "'" + notetx.ToString() + "',"));
            sb.Append(columnNames[MtChan.NOTERX] + "=" + (nullInds[MtChan.NOTERX] == Constant.DB_NULL ? "NULL," : "'" + noterx.ToString() + "',"));
            sb.Append(columnNames[MtChan.NOTEGNL] + "=" + (nullInds[MtChan.NOTEGNL] == Constant.DB_NULL ? "NULL," : "'" + notegnl.ToString() + "',"));
            sb.Append(columnNames[MtChan.CPOINT] + "=" + (nullInds[MtChan.CPOINT] == Constant.DB_NULL ? "NULL," : "'" + cpoint.ToString() + "',"));
            sb.Append(columnNames[MtChan.FEETX] + "=" + (nullInds[MtChan.FEETX] == Constant.DB_NULL ? "NULL," : "'" + feetx.ToString() + "',"));
            sb.Append(columnNames[MtChan.FEERX] + "=" + (nullInds[MtChan.FEERX] == Constant.DB_NULL ? "NULL," : "'" + feerx.ToString() + "',"));
            sb.Append(columnNames[MtChan.MDATE] + "=" + (nullInds[MtChan.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',"));
            sb.Append(columnNames[MtChan.MTIME] + "=" + (nullInds[MtChan.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "',"));
            sb.Append(columnNames[MtChan.USERID] + "=" + (nullInds[MtChan.USERID] == Constant.DB_NULL ? "NULL" : "'" + userid.ToString() + "'"));


            return sb.ToString();
        }


        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[mt_chan](" +
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
    "[userid] [char](12) NULL," +
" CONSTRAINT [PK_mt_chan] PRIMARY KEY CLUSTERED " +
"(" +
    "[call1] ASC," +
    "[call2] ASC," +
    "[bndcde] ASC," +
    "[chid] ASC" +
")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";










    }
}

```
