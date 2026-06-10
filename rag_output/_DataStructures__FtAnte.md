# Documented File: FtAnte.cs
**Repository Path:** `_DataStructures\FtAnte.cs`
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
using _NewLib;

namespace _DataStructures
{
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
    /// This class has fields that are isomorphic with TS PDF table <b>&lt;userID&gt;.ft_&lt;pdfName&gt;_ante</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FtAnte
    {
        // IMPORTANT!
        // =========
        // The following qty. 37 member values correspond to the columns
        // of an FtAnte table stored in the database; for simplicity, the
        // member and column names are prescribed to be identical.
        // The order of appearance of these qty. 37 'column' members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CMD_SZ)]
        public string cmd;  //0
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.RECSTAT_SZ)]
        public string recstat;  //1
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CALL_SZ)]
        public string call1;  //2
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CALL_SZ)]
        public string call2;  //3
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.BNDCDE_SZ)]
        public string bndcde; //4
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short anum; //5
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.AUSE_SZ)]
        public string ause; //6
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FT_ANTE_ACODE_SZ)]
        public string acode; //7
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float aht; //8
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float azmth; //9
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float elvtn; //10
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float dist; //11
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.OFFAZM_SZ)]
        public string offazm; //12
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float tazmth; //13
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float telvtn; //14
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float tgain; //15
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.XFDLN_SZ)]
        public string txfdlnth; //16
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txfdlnlh; //17
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.XFDLN_SZ)]
        public string txfdlntv; //18
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txfdlnlv; //19
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.XFDLN_SZ)]
        public string rxfdlnth; //20
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxfdlnlh; //21
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.XFDLN_SZ)]
        public string rxfdlntv; //22
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxfdlnlv; //23
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txpadpam; //24
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxpadlna; //25
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txcompl; //26
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxcompl; //27
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float obsloss; //28
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float kvalue; //29
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short atwrno; //30
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.NOTA_SZ)]
        public string nota; //31
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.APOINT_SZ)]
        public string apoint; //32
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.DATE_SZ)]
        public string sdate; //33
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.LICENCE_SZ)]
        public string licence; //34
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.DATE_SZ)]
        public string mdate; //35
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.TIME_SZ)]
        public string mtime; //36

        //----------------------------------------------------------------
        //Additional public static members.
        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int CALL1 = 2;
        public const int CALL2 = 3;
        public const int BNDCDE = 4;
        public const int ANUM = 5;
        public const int AUSE = 6;
        public const int ACODE = 7;
        public const int AHT = 8;
        public const int AZMTH = 9;
        public const int ELVTN = 10;
        public const int DIST = 11;
        public const int OFFAZM = 12;
        public const int TAZMTH = 13;
        public const int TELVTN = 14;
        public const int TGAIN = 15;
        public const int TXFDLNTH = 16;
        public const int TXFDLNLH = 17;
        public const int TXFDLNTV = 18;
        public const int TXFDLNLV = 19;
        public const int RXFDLNTH = 20;
        public const int RXFDLNLH = 21;
        public const int RXFDLNTV = 22;
        public const int RXFDLNLV = 23;
        public const int TXPADPAM = 24;
        public const int RXPADLNA = 25;
        public const int TXCOMPL = 26;
        public const int RXCOMPL = 27;
        public const int OBSLOSS = 28;
        public const int KVALUE = 29;
        public const int ATWRNO = 30;
        public const int NOTA = 31;
        public const int APOINT = 32;
        public const int SDATE = 33;
        public const int LICENCE = 34;
        public const int MDATE = 35;
        public const int MTIME = 36;

        //---------------------------------------------------------------------

        public const int CMD_SZ = 2;
        public const int RECSTAT_SZ = 2;
        public const int CALL1_SZ = 10;
        public const int CALL2_SZ = 10;
        public const int BNDCDE_SZ = 5;
        public const int AUSE_SZ = 4;
        public const int ACODE_SZ = 13;
        public const int OFFAZM_SZ = 2;
        public const int TXFDLNTH_SZ = 3;
        public const int TXFDLNTV_SZ = 3;
        public const int RXFDLNTH_SZ = 3;
        public const int RXFDLNTV_SZ = 3;
        public const int NOTA_SZ = 5;
        public const int APOINT_SZ = 5;
        public const int SDATE_SZ = Constant.DATE_SZ;
        public const int LICENCE_SZ = Constant.LICENCE_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 37;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "call1", "call2", "bndcde", "anum", "ause", "acode", "aht", "azmth", "elvtn", "dist", "offazm", "tazmth", "telvtn", "tgain", "txfdlnth", "txfdlnlh", "txfdlntv", "txfdlnlv", "rxfdlnth", "rxfdlnlh", "rxfdlntv", "rxfdlnlv", "txpadpam", "rxpadlna", "txcompl", "rxcompl", "obsloss", "kvalue", "atwrno", "nota", "apoint", "sdate", "licence", "mdate", "mtime" };

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
        static FtAnte()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Default object constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public FtAnte()
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
            const float FLOAT_INIT_VAL = 0.0f;

            cmd = STRING_INIT_VAL;
            recstat = STRING_INIT_VAL;
            call1 = STRING_INIT_VAL;
            call2 = STRING_INIT_VAL;
            bndcde = STRING_INIT_VAL;
            anum = SHORT_INIT_VAL;
            ause = STRING_INIT_VAL;
            acode = STRING_INIT_VAL;
            aht = FLOAT_INIT_VAL;
            azmth = FLOAT_INIT_VAL;
            elvtn = FLOAT_INIT_VAL;
            dist = FLOAT_INIT_VAL;
            offazm = STRING_INIT_VAL;
            tazmth = FLOAT_INIT_VAL;
            telvtn = FLOAT_INIT_VAL;
            tgain = FLOAT_INIT_VAL;
            txfdlnth = STRING_INIT_VAL;
            txfdlnlh = FLOAT_INIT_VAL;
            txfdlntv = STRING_INIT_VAL;
            txfdlnlv = FLOAT_INIT_VAL;
            rxfdlnth = STRING_INIT_VAL;
            rxfdlnlh = FLOAT_INIT_VAL;
            rxfdlntv = STRING_INIT_VAL;
            rxfdlnlv = FLOAT_INIT_VAL;
            txpadpam = FLOAT_INIT_VAL;
            rxpadlna = FLOAT_INIT_VAL;
            txcompl = FLOAT_INIT_VAL;
            rxcompl = FLOAT_INIT_VAL;
            obsloss = FLOAT_INIT_VAL;
            kvalue = FLOAT_INIT_VAL;
            atwrno = SHORT_INIT_VAL;
            nota = STRING_INIT_VAL;
            apoint = STRING_INIT_VAL;
            sdate = STRING_INIT_VAL;
            licence = STRING_INIT_VAL;
            mdate = STRING_INIT_VAL;
            mtime = STRING_INIT_VAL;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("cmd =      " + cmd);
            sb.AppendLine("recstat =  " + recstat);
            sb.AppendLine("call1 =    " + call1);
            sb.AppendLine("call2 =    " + call2);
            sb.AppendLine("bndcde =   " + bndcde);
            sb.AppendLine("anum =     " + anum);
            sb.AppendLine("ause =     " + ause);
            sb.AppendLine("acode =    " + acode);
            sb.AppendLine("aht =      " + aht);
            sb.AppendLine("azmth =    " + azmth);
            sb.AppendLine("elvtn =    " + elvtn);
            sb.AppendLine("dist =     " + dist);
            sb.AppendLine("offazm =   " + offazm);
            sb.AppendLine("tazmth =   " + tazmth);
            sb.AppendLine("telvtn =   " + telvtn);
            sb.AppendLine("tgain =    " + tgain);
            sb.AppendLine("txfdlnth = " + txfdlnth);
            sb.AppendLine("txfdlnlh = " + txfdlnlh);
            sb.AppendLine("txfdlntv = " + txfdlntv);
            sb.AppendLine("txfdlnlv = " + txfdlnlv);
            sb.AppendLine("rxfdlnth = " + rxfdlnth);
            sb.AppendLine("rxfdlnlh = " + rxfdlnlh);
            sb.AppendLine("rxfdlntv = " + rxfdlntv);
            sb.AppendLine("rxfdlnlv = " + rxfdlnlv);
            sb.AppendLine("txpadpam = " + txpadpam);
            sb.AppendLine("rxpadlna = " + rxpadlna);
            sb.AppendLine("txcompl =  " + txcompl);
            sb.AppendLine("rxcompl =  " + rxcompl);
            sb.AppendLine("obsloss =  " + obsloss);
            sb.AppendLine("kvalue =   " + kvalue);
            sb.AppendLine("atwrno =   " + atwrno);
            sb.AppendLine("nota =     " + nota);
            sb.AppendLine("apoint =   " + apoint);
            sb.AppendLine("sdate =    " + sdate);
            sb.AppendLine("licence =  " + licence);
            sb.AppendLine("mdate =    " + mdate);
            sb.AppendLine("mtime =    " + mtime);

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
            sb.AppendLine();
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "cmd =      " + cmd);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "recstat =  " + recstat);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "call1 =    " + call1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "call2 =    " + call2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "bndcde =   " + bndcde);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "anum =     " + anum);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ause =     " + ause);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "acode =    " + acode);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "aht =      " + aht);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "azmth =    " + azmth);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "elvtn =    " + elvtn);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "dist =     " + dist);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "offazm =   " + offazm);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tazmth =   " + tazmth);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "telvtn =   " + telvtn);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tgain =    " + tgain);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txfdlnth = " + txfdlnth);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txfdlnlh = " + txfdlnlh);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txfdlntv = " + txfdlntv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txfdlnlv = " + txfdlnlv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxfdlnth = " + rxfdlnth);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxfdlnlh = " + rxfdlnlh);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxfdlntv = " + rxfdlntv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxfdlnlv = " + rxfdlnlv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txpadpam = " + txpadpam);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxpadlna = " + rxpadlna);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txcompl =  " + txcompl);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxcompl =  " + rxcompl);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "obsloss =  " + obsloss);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "kvalue =   " + kvalue);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "atwrno =   " + atwrno);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "nota =     " + nota);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "apoint =   " + apoint);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "sdate =    " + sdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "licence =  " + licence);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate =    " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime =    " + mtime);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string that concatenates the 'key'
        /// fields {call1, call2, bndcde, anum}.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string KeysToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("call1 = " + call1);
            sb.Append(" call2 = " + call2);
            sb.Append(" bndcde = " + bndcde);
            sb.Append(" anum = " + anum);
            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string that concatenates the 'key'
        /// fields {call1, call2, bndcde, anum}.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string KeysToStringTerse()
        {
            return String.Format("{0}, {1}, {2}, {3}", call1, call2, bndcde, anum);
        }

        /// <summary>
        /// For each field of this object, the method allocates a type-specific amount
        /// of global (heap) memory, copies the value of the field into it, and returns
        /// an IntPtr[] containing all of the start addresses. 
        /// </summary>
        /// <returns></returns>
        public SQLPOINTER[] CopyToArrayOfSQLPOINTERs()
        {
            //Can only copy arrays of float into native memory using Marshal method.
            float[] F = new float[1];

            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[0] = Marshal.StringToHGlobalAnsi(cmd);  // #0  string  cmd

            parameterValuePtr[1] = Marshal.StringToHGlobalAnsi(recstat);  // #1  string  recstat

            parameterValuePtr[2] = Marshal.StringToHGlobalAnsi(call1);  // #2  string  call1

            parameterValuePtr[3] = Marshal.StringToHGlobalAnsi(call2);  // #3  string  call2

            parameterValuePtr[4] = Marshal.StringToHGlobalAnsi(bndcde);  // #4  string  bndcde

            parameterValuePtr[5] = Marshal.AllocHGlobal(sizeof(short));  // #5  short  anum
            Marshal.WriteInt16(parameterValuePtr[5], anum);

            parameterValuePtr[6] = Marshal.StringToHGlobalAnsi(ause);  // #6  string  ause

            parameterValuePtr[7] = Marshal.StringToHGlobalAnsi(acode);  // #7  string  acode

            parameterValuePtr[8] = Marshal.AllocHGlobal(sizeof(float));  // #8  float  aht
            F[0] = aht;
            Marshal.Copy(F, 0, parameterValuePtr[8], 1);

            parameterValuePtr[9] = Marshal.AllocHGlobal(sizeof(float));  // #9  float  azmth
            F[0] = azmth;
            Marshal.Copy(F, 0, parameterValuePtr[9], 1);

            parameterValuePtr[10] = Marshal.AllocHGlobal(sizeof(float));  // #10  float  elvtn
            F[0] = elvtn;
            Marshal.Copy(F, 0, parameterValuePtr[10], 1);

            parameterValuePtr[11] = Marshal.AllocHGlobal(sizeof(float));  // #11  float  dist
            F[0] = dist;
            Marshal.Copy(F, 0, parameterValuePtr[11], 1);

            parameterValuePtr[12] = Marshal.StringToHGlobalAnsi(offazm);  // #12  string  offazm

            parameterValuePtr[13] = Marshal.AllocHGlobal(sizeof(float));  // #13  float  tazmth
            F[0] = tazmth;
            Marshal.Copy(F, 0, parameterValuePtr[13], 1);

            parameterValuePtr[14] = Marshal.AllocHGlobal(sizeof(float));  // #14  float  telvtn
            F[0] = telvtn;
            Marshal.Copy(F, 0, parameterValuePtr[14], 1);

            parameterValuePtr[15] = Marshal.AllocHGlobal(sizeof(float));  // #15  float  tgain
            F[0] = tgain;
            Marshal.Copy(F, 0, parameterValuePtr[15], 1);

            parameterValuePtr[16] = Marshal.StringToHGlobalAnsi(txfdlnth);  // #16  string  txfdlnth

            parameterValuePtr[17] = Marshal.AllocHGlobal(sizeof(float));  // #17  float  txfdlnlh
            F[0] = txfdlnlh;
            Marshal.Copy(F, 0, parameterValuePtr[17], 1);

            parameterValuePtr[18] = Marshal.StringToHGlobalAnsi(txfdlntv);  // #18  string  txfdlntv

            parameterValuePtr[19] = Marshal.AllocHGlobal(sizeof(float));  // #19  float  txfdlnlv
            F[0] = txfdlnlv;
            Marshal.Copy(F, 0, parameterValuePtr[19], 1);

            parameterValuePtr[20] = Marshal.StringToHGlobalAnsi(rxfdlnth);  // #20  string  rxfdlnth

            parameterValuePtr[21] = Marshal.AllocHGlobal(sizeof(float));  // #21  float  rxfdlnlh
            F[0] = rxfdlnlh;
            Marshal.Copy(F, 0, parameterValuePtr[21], 1);

            parameterValuePtr[22] = Marshal.StringToHGlobalAnsi(rxfdlntv);  // #22  string  rxfdlntv

            parameterValuePtr[23] = Marshal.AllocHGlobal(sizeof(float));  // #23  float  rxfdlnlv
            F[0] = rxfdlnlv;
            Marshal.Copy(F, 0, parameterValuePtr[23], 1);

            parameterValuePtr[24] = Marshal.AllocHGlobal(sizeof(float));  // #24  float  txpadpam
            F[0] = txpadpam;
            Marshal.Copy(F, 0, parameterValuePtr[24], 1);

            parameterValuePtr[25] = Marshal.AllocHGlobal(sizeof(float));  // #25  float  rxpadlna
            F[0] = rxpadlna;
            Marshal.Copy(F, 0, parameterValuePtr[25], 1);

            parameterValuePtr[26] = Marshal.AllocHGlobal(sizeof(float));  // #26  float  txcompl
            F[0] = txcompl;
            Marshal.Copy(F, 0, parameterValuePtr[26], 1);

            parameterValuePtr[27] = Marshal.AllocHGlobal(sizeof(float));  // #27  float  rxcompl
            F[0] = rxcompl;
            Marshal.Copy(F, 0, parameterValuePtr[27], 1);

            parameterValuePtr[28] = Marshal.AllocHGlobal(sizeof(float));  // #28  float  obsloss
            F[0] = obsloss;
            Marshal.Copy(F, 0, parameterValuePtr[28], 1);

            parameterValuePtr[29] = Marshal.AllocHGlobal(sizeof(float));  // #29  float  kvalue
            F[0] = kvalue;
            Marshal.Copy(F, 0, parameterValuePtr[29], 1);

            parameterValuePtr[30] = Marshal.AllocHGlobal(sizeof(short));  // #30  short  atwrno
            Marshal.WriteInt16(parameterValuePtr[30], atwrno);

            parameterValuePtr[31] = Marshal.StringToHGlobalAnsi(nota);  // #31  string  nota

            parameterValuePtr[32] = Marshal.StringToHGlobalAnsi(apoint);  // #32  string  apoint

            parameterValuePtr[33] = Marshal.StringToHGlobalAnsi(sdate);  // #33  string  sdate

            parameterValuePtr[34] = Marshal.StringToHGlobalAnsi(licence);  // #34  string  licence

            parameterValuePtr[35] = Marshal.StringToHGlobalAnsi(mdate);  // #35  string  mdate

            parameterValuePtr[36] = Marshal.StringToHGlobalAnsi(mtime);  // #36  string  mtime

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
            //Can only copy arrays of float into native memory using Marshal method.
            float[] F = new float[1];

            WSTP(cmd, parameterValuePtr[0]);  // #0  string  cmd

            WSTP(recstat, parameterValuePtr[1]);  // #1  string  recstat

            WSTP(call1, parameterValuePtr[2]);  // #2  string  call1

            WSTP(call2, parameterValuePtr[3]);  // #3  string  call2

            WSTP(bndcde, parameterValuePtr[4]);  // #4  string  bndcde

            Marshal.WriteInt16(parameterValuePtr[5], anum);  // #5  short  anum

            WSTP(ause, parameterValuePtr[6]);  // #6  string  ause

            WSTP(acode, parameterValuePtr[7]);  // #7  string  acode

            F[0] = aht;
            Marshal.Copy(F, 0, parameterValuePtr[8], 1);  // #8  float  aht

            F[0] = azmth;
            Marshal.Copy(F, 0, parameterValuePtr[9], 1);  // #9  float  azmth

            F[0] = elvtn;
            Marshal.Copy(F, 0, parameterValuePtr[10], 1);  // #10  float  elvtn

            F[0] = dist;
            Marshal.Copy(F, 0, parameterValuePtr[11], 1);  // #11  float  dist

            WSTP(offazm, parameterValuePtr[12]);  // #12  string  offazm

            F[0] = tazmth;
            Marshal.Copy(F, 0, parameterValuePtr[13], 1);  // #13  float  tazmth

            F[0] = telvtn;
            Marshal.Copy(F, 0, parameterValuePtr[14], 1);  // #14  float  telvtn

            F[0] = tgain;
            Marshal.Copy(F, 0, parameterValuePtr[15], 1);  // #15  float  tgain

            WSTP(txfdlnth, parameterValuePtr[16]);  // #16  string  txfdlnth

            F[0] = txfdlnlh;
            Marshal.Copy(F, 0, parameterValuePtr[17], 1);  // #17  float  txfdlnlh

            WSTP(txfdlntv, parameterValuePtr[18]);  // #18  string  txfdlntv

            F[0] = txfdlnlv;
            Marshal.Copy(F, 0, parameterValuePtr[19], 1);  // #19  float  txfdlnlv

            WSTP(rxfdlnth, parameterValuePtr[20]);  // #20  string  rxfdlnth

            F[0] = rxfdlnlh;
            Marshal.Copy(F, 0, parameterValuePtr[21], 1);  // #21  float  rxfdlnlh

            WSTP(rxfdlntv, parameterValuePtr[22]);  // #22  string  rxfdlntv

            F[0] = rxfdlnlv;
            Marshal.Copy(F, 0, parameterValuePtr[23], 1);  // #23  float  rxfdlnlv

            F[0] = txpadpam;
            Marshal.Copy(F, 0, parameterValuePtr[24], 1);  // #24  float  txpadpam

            F[0] = rxpadlna;
            Marshal.Copy(F, 0, parameterValuePtr[25], 1);  // #25  float  rxpadlna

            F[0] = txcompl;
            Marshal.Copy(F, 0, parameterValuePtr[26], 1);  // #26  float  txcompl

            F[0] = rxcompl;
            Marshal.Copy(F, 0, parameterValuePtr[27], 1);  // #27  float  rxcompl

            F[0] = obsloss;
            Marshal.Copy(F, 0, parameterValuePtr[28], 1);  // #28  float  obsloss

            F[0] = kvalue;
            Marshal.Copy(F, 0, parameterValuePtr[29], 1);  // #29  float  kvalue

            Marshal.WriteInt16(parameterValuePtr[30], atwrno);  // #30  short  atwrno

            WSTP(nota, parameterValuePtr[31]);  // #31  string  nota

            WSTP(apoint, parameterValuePtr[32]);  // #32  string  apoint

            WSTP(sdate, parameterValuePtr[33]);  // #33  string  sdate

            WSTP(licence, parameterValuePtr[34]);  // #34  string  licence

            WSTP(mdate, parameterValuePtr[35]);  // #35  string  mdate

            WSTP(mtime, parameterValuePtr[36]);  // #36  string  mtime

        }
        /// <summary>
        /// This method supports testing by setting the fields of this object
        /// to predetermined reference values.
        /// </summary>
        /// <param name=""></param>
        public void SetAllColumnsToPrescribedTestValues()
        {
            cmd = "A";  //  #0  string  cmd

            recstat = "B";  //  #1  string  recstat

            call1 = "C23456789";  //  #2  string  call1

            call2 = "D23456789";  //  #3  string  call2

            bndcde = "E234";  //  #4  string  bndcde

            anum = 5;  //  #5  short  anum

            ause = "F23";  //  #6  string  ause

            acode = "G23456789012";  //  #7  string  acode

            aht = 8.0f;  //  #8  float  aht

            azmth = 9.0f;  //  #9  float  azmth

            elvtn = 10.0f;  //  #10  float  elvtn

            dist = 11.0f;  //  #11  float  dist

            offazm = "H";  //  #12  string  offazm

            tazmth = 13.0f;  //  #13  float  tazmth

            telvtn = 14.0f;  //  #14  float  telvtn

            tgain = 15.0f;  //  #15  float  tgain

            txfdlnth = "i2";  //  #16  string  txfdlnth

            txfdlnlh = 17.0f;  //  #17  float  txfdlnlh

            txfdlntv = "J2";  //  #18  string  txfdlntv

            txfdlnlv = 19.0f;  //  #19  float  txfdlnlv

            rxfdlnth = "K2";  //  #20  string  rxfdlnth

            rxfdlnlh = 21.0f;  //  #21  float  rxfdlnlh

            rxfdlntv = "L2";  //  #22  string  rxfdlntv

            rxfdlnlv = 23.0f;  //  #23  float  rxfdlnlv

            txpadpam = 24.0f;  //  #24  float  txpadpam

            rxpadlna = 25.0f;  //  #25  float  rxpadlna

            txcompl = 26.0f;  //  #26  float  txcompl

            rxcompl = 27.0f;  //  #27  float  rxcompl

            obsloss = 28.0f;  //  #28  float  obsloss

            kvalue = 29.0f;  //  #29  float  kvalue

            atwrno = 30;  //  #30  short  atwrno

            nota = "M234";  //  #31  string  nota

            apoint = "N234";  //  #32  string  apoint

            sdate = "o234567890";  //  #33  string  sdate

            licence = "P234567890123";  //  #34  string  licence

            mdate = "Q234567890";  //  #35  string  mdate

            mtime = "R2345678";  //  #36  string  mtime
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
        public static SQLPOINTER[] AllocateArrayOfSQLPOINTERsWithMaxStringBuffers()
        {
            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[0] = Marshal.AllocHGlobal(Constant.CMD_SZ + 1);  // #0  string  cmd

            parameterValuePtr[1] = Marshal.AllocHGlobal(Constant.RECSTAT_SZ + 1);  // #1  string  recstat

            parameterValuePtr[2] = Marshal.AllocHGlobal(Constant.CALL_SZ + 1);  // #2  string  call1

            parameterValuePtr[3] = Marshal.AllocHGlobal(Constant.CALL_SZ + 1);  // #3  string  call2

            parameterValuePtr[4] = Marshal.AllocHGlobal(Constant.BNDCDE_SZ + 1);  // #4  string  bndcde

            parameterValuePtr[5] = Marshal.AllocHGlobal(sizeof(short));  // #5  short  anum

            parameterValuePtr[6] = Marshal.AllocHGlobal(Constant.AUSE_SZ + 1);  // #6  string  ause

            parameterValuePtr[7] = Marshal.AllocHGlobal(Constant.FT_ANTE_ACODE_SZ + 1);  // #7  string  acode

            parameterValuePtr[8] = Marshal.AllocHGlobal(sizeof(float));  // #8  float  aht

            parameterValuePtr[9] = Marshal.AllocHGlobal(sizeof(float));  // #9  float  azmth

            parameterValuePtr[10] = Marshal.AllocHGlobal(sizeof(float));  // #10  float  elvtn

            parameterValuePtr[11] = Marshal.AllocHGlobal(sizeof(float));  // #11  float  dist

            parameterValuePtr[12] = Marshal.AllocHGlobal(Constant.OFFAZM_SZ + 1);  // #12  string  offazm

            parameterValuePtr[13] = Marshal.AllocHGlobal(sizeof(float));  // #13  float  tazmth

            parameterValuePtr[14] = Marshal.AllocHGlobal(sizeof(float));  // #14  float  telvtn

            parameterValuePtr[15] = Marshal.AllocHGlobal(sizeof(float));  // #15  float  tgain

            parameterValuePtr[16] = Marshal.AllocHGlobal(Constant.XFDLN_SZ + 1);  // #16  string  txfdlnth

            parameterValuePtr[17] = Marshal.AllocHGlobal(sizeof(float));  // #17  float  txfdlnlh

            parameterValuePtr[18] = Marshal.AllocHGlobal(Constant.XFDLN_SZ + 1);  // #18  string  txfdlntv

            parameterValuePtr[19] = Marshal.AllocHGlobal(sizeof(float));  // #19  float  txfdlnlv

            parameterValuePtr[20] = Marshal.AllocHGlobal(Constant.XFDLN_SZ + 1);  // #20  string  rxfdlnth

            parameterValuePtr[21] = Marshal.AllocHGlobal(sizeof(float));  // #21  float  rxfdlnlh

            parameterValuePtr[22] = Marshal.AllocHGlobal(Constant.XFDLN_SZ + 1);  // #22  string  rxfdlntv

            parameterValuePtr[23] = Marshal.AllocHGlobal(sizeof(float));  // #23  float  rxfdlnlv

            parameterValuePtr[24] = Marshal.AllocHGlobal(sizeof(float));  // #24  float  txpadpam

            parameterValuePtr[25] = Marshal.AllocHGlobal(sizeof(float));  // #25  float  rxpadlna

            parameterValuePtr[26] = Marshal.AllocHGlobal(sizeof(float));  // #26  float  txcompl

            parameterValuePtr[27] = Marshal.AllocHGlobal(sizeof(float));  // #27  float  rxcompl

            parameterValuePtr[28] = Marshal.AllocHGlobal(sizeof(float));  // #28  float  obsloss

            parameterValuePtr[29] = Marshal.AllocHGlobal(sizeof(float));  // #29  float  kvalue

            parameterValuePtr[30] = Marshal.AllocHGlobal(sizeof(short));  // #30  short  atwrno

            parameterValuePtr[31] = Marshal.AllocHGlobal(Constant.NOTA_SZ + 1);  // #31  string  nota

            parameterValuePtr[32] = Marshal.AllocHGlobal(Constant.APOINT_SZ + 1);  // #32  string  apoint

            parameterValuePtr[33] = Marshal.AllocHGlobal(Constant.DATE_SZ + 1);  // #33  string  sdate

            parameterValuePtr[34] = Marshal.AllocHGlobal(Constant.LICENCE_SZ + 1);  // #34  string  licence

            parameterValuePtr[35] = Marshal.AllocHGlobal(Constant.DATE_SZ + 1);  // #35  string  mdate

            parameterValuePtr[36] = Marshal.AllocHGlobal(Constant.TIME_SZ + 1);  // #36  string  mtime

            // If this array of pointers is used to instantiate reusable ODBC parameter bindings,
            // we need to fill it with valid string values to prevent SQLExecute errors.
            FtAnte ftAnte = new FtAnte();
            ftAnte.CopyToExistingArrayOfSQLPOINTERs(parameterValuePtr);

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
        /// This method returns true if this FtAnte object matches a
        /// prescribed {call1, call2, bndcde, anum} key set.
        /// </summary>
        /// <param name="call1"></param>
        /// <param name="call2"></param>
        /// <param name="bndcde"></param>
        /// <param name="anum"></param>
        /// <returns></returns>
        public bool KeyIs(string call1, string call2, string bndcde, short anum)
        {
            bool result = false;

            if (String.Equals(this.call1, call1) &&
                String.Equals(this.call2, call2) &&
                String.Equals(this.bndcde, bndcde) &&
                this.anum == anum)
            {
                result = true;
            }

            return result;
        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[{1}]" +
"(" +
    "[cmd] [char](1) NULL," +
    "[recstat] [char](1) NULL," +
    "[call1] [char](9) NOT NULL," +
    "[call2] [char](9) NOT NULL," +
    "[bndcde] [char](4) NOT NULL," +
    "[anum] [smallint] NOT NULL," +
    "[ause] [char](3) NULL," +
    "[acode] [char](12) NULL," +
    "[aht] [real] NULL," +
    "[azmth] [real] NULL," +
    "[elvtn] [real] NULL," +
    "[dist] [real] NULL," +
    "[offazm] [char](1) NULL," +
    "[tazmth] [real] NULL," +
    "[telvtn] [real] NULL," +
    "[tgain] [real] NULL," +
    "[txfdlnth] [char](2) NULL," +
    "[txfdlnlh] [real] NULL," +
    "[txfdlntv] [char](2) NULL," +
    "[txfdlnlv] [real] NULL," +
    "[rxfdlnth] [char](2) NULL," +
    "[rxfdlnlh] [real] NULL," +
    "[rxfdlntv] [char](2) NULL," +
    "[rxfdlnlv] [real] NULL," +
    "[txpadpam] [real] NULL," +
    "[rxpadlna] [real] NULL," +
    "[txcompl] [real] NULL," +
    "[rxcompl] [real] NULL," +
    "[obsloss] [real] NULL," +
    "[kvalue] [real] NULL," +
    "[atwrno] [tinyint] NULL," +
    "[nota] [char](4) NULL," +
    "[apoint] [char](4) NULL," +
    "[sdate] [char](10) NULL," +
    "[licence] [char](13) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
    "CONSTRAINT[PK_FtSite] PRIMARY KEY CLUSTERED " +
    "(" +
    "[call1] ASC," +
    "[call2] ASC," +
    "[bndcde] ASC," +
    "[anum] ASC" +
    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON[PRIMARY]" +
") ON[PRIMARY]";


    }
}

```
