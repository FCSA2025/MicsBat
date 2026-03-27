using _Configuration;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace _DataStructures
{
    using _NewLib;
    using SQLHANDLE = IntPtr;
    using SQLLEN = Int64;
    using SQLLENPTR = IntPtr;
    using SQLPOINTER = IntPtr;

    /// <summary>
    /// This class has fields that are isomorphic with the master table <b>main.mt_ante</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    [Serializable]
    public class MtAnte
    {
        // IMPORTANT!
        // =========
        // The following qty. 36 member values correspond to the columns
        // of an MtAnte table stored in the database; for simplicity, the
        // member and column names are prescribed to be identical.
        // The order of appearance of these qty. 36 'column' members MUST be
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
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short anum;  // #3
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = AUSE_SZ)]
        public string ause;  // #4
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ACODE_SZ)]
        public string acode;  // #5
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float aht;  // #6
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float azmth;  // #7
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float elvtn;  // #8
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float dist;  // #9
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = OFFAZM_SZ)]
        public string offazm;  // #10
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float tazmth;  // #11
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float telvtn;  // #12
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float tgain;  // #13
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TXFDLNTH_SZ)]
        public string txfdlnth;  // #14
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txfdlnlh;  // #15
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TXFDLNTV_SZ)]
        public string txfdlntv;  // #16
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txfdlnlv;  // #17
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RXFDLNTH_SZ)]
        public string rxfdlnth;  // #18
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxfdlnlh;  // #19
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RXFDLNTV_SZ)]
        public string rxfdlntv;  // #20
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxfdlnlv;  // #21
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txpadpam;  // #22
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxpadlna;  // #23
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txcompl;  // #24
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxcompl;  // #25
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float obsloss;  // #26
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float kvalue;  // #27
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short atwrno;  // #28
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTA_SZ)]
        public string nota;  // #29
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = APOINT_SZ)]
        public string apoint;  // #30
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SDATE_SZ)]
        public string sdate;  // #31
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = LICENCE_SZ)]

        // The order of the 'license' member here is different from that of the 
        // column named 'license' in the DB table main.mt_ante.
        public string licence;  // #32
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]

        public string mdate;  // #33
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;  // #34
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = USERID_SZ)]
        public string userid;  // #35

        //----------------------------------------------------------------
        //Additional public static members.

        public const int CALL1 = 0;
        public const int CALL2 = 1;
        public const int BNDCDE = 2;
        public const int ANUM = 3;
        public const int AUSE = 4;
        public const int ACODE = 5;
        public const int AHT = 6;
        public const int AZMTH = 7;
        public const int ELVTN = 8;
        public const int DIST = 9;
        public const int OFFAZM = 10;
        public const int TAZMTH = 11;
        public const int TELVTN = 12;
        public const int TGAIN = 13;
        public const int TXFDLNTH = 14;
        public const int TXFDLNLH = 15;
        public const int TXFDLNTV = 16;
        public const int TXFDLNLV = 17;
        public const int RXFDLNTH = 18;
        public const int RXFDLNLH = 19;
        public const int RXFDLNTV = 20;
        public const int RXFDLNLV = 21;
        public const int TXPADPAM = 22;
        public const int RXPADLNA = 23;
        public const int TXCOMPL = 24;
        public const int RXCOMPL = 25;
        public const int OBSLOSS = 26;
        public const int KVALUE = 27;
        public const int ATWRNO = 28;
        public const int NOTA = 29;
        public const int APOINT = 30;
        public const int SDATE = 31;
        public const int LICENCE = 35;  // <--- Anomaly: differs from main.mt_ante.
        public const int MDATE = 32;
        public const int MTIME = 33;
        public const int USERID = 34;

        //-------------------------------------------------------------------------

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
        public const int USERID_SZ = 13;

        //-------------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 36;

        //Array of strings providing the class-member / database-column names.
        //Note: the order is that of columns in the table main.mt_ante.
        private static string[] columnNames = new string[NUM_COLUMNS] { "call1", "call2", "bndcde", "anum", "ause", "acode", "aht", "azmth", "elvtn", "dist", "offazm", "tazmth", "telvtn", "tgain", "txfdlnth", "txfdlnlh", "txfdlntv", "txfdlnlv", "rxfdlnth", "rxfdlnlh", "rxfdlntv", "rxfdlnlv", "txpadpam", "rxpadlna", "txcompl", "rxcompl", "obsloss", "kvalue", "atwrno", "nota", "apoint", "sdate", "mdate", "mtime", "userid", "licence " };

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
        static MtAnte()
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
        public MtAnte()
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

            call1 = STRING_INIT_VAL;    //0
            call2 = STRING_INIT_VAL;    //1
            bndcde = STRING_INIT_VAL;    //2
            anum = SHORT_INIT_VAL;    //3
            ause = STRING_INIT_VAL;    //4
            acode = STRING_INIT_VAL;    //5
            aht = FLOAT_INIT_VAL;    //6
            azmth = FLOAT_INIT_VAL;    //7
            elvtn = FLOAT_INIT_VAL;    //8
            dist = FLOAT_INIT_VAL;    //9
            offazm = STRING_INIT_VAL;    //10
            tazmth = FLOAT_INIT_VAL;    //11
            telvtn = FLOAT_INIT_VAL;    //12
            tgain = FLOAT_INIT_VAL;    //13
            txfdlnth = STRING_INIT_VAL;    //14
            txfdlnlh = FLOAT_INIT_VAL;    //15
            txfdlntv = STRING_INIT_VAL;    //16
            txfdlnlv = FLOAT_INIT_VAL;    //17
            rxfdlnth = STRING_INIT_VAL;    //18
            rxfdlnlh = FLOAT_INIT_VAL;    //19
            rxfdlntv = STRING_INIT_VAL;    //20
            rxfdlnlv = FLOAT_INIT_VAL;    //21
            txpadpam = FLOAT_INIT_VAL;    //22
            rxpadlna = FLOAT_INIT_VAL;    //23
            txcompl = FLOAT_INIT_VAL;    //24
            rxcompl = FLOAT_INIT_VAL;    //25
            obsloss = FLOAT_INIT_VAL;    //26
            kvalue = FLOAT_INIT_VAL;    //27
            atwrno = SHORT_INIT_VAL;    //28
            nota = STRING_INIT_VAL;    //29
            apoint = STRING_INIT_VAL;    //30
            sdate = STRING_INIT_VAL;    //31
            licence = STRING_INIT_VAL;    //32
            mdate = STRING_INIT_VAL;    //33
            mtime = STRING_INIT_VAL;    //34
            userid = STRING_INIT_VAL;    //35
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
            sb.Append("\r\n===== MtAnte =====");

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
            sb.AppendLine("mdate =    " + mdate);
            sb.AppendLine("mtime =    " + mtime);
            sb.AppendLine("userid =   " + userid);

            // We will use the column order of the table.main.mt_ante.
            sb.AppendLine("licence =  " + licence);

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
            sb.Append("   call2 = " + call2);
            sb.Append("   bndcde = " + bndcde);
            sb.Append("   anum = " + anum);
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
            anum = 3;    //3
            ause = "04";    //4
            acode = "05";    //5
            aht = 6.0F;    //6
            azmth = 7.0F;    //7
            elvtn = 8.0F;    //8
            dist = 9.0F;    //9
            offazm = "A";    //10
            tazmth = 11.0F;    //11
            telvtn = 12.0F;    //12
            tgain = 13.0F;    //13
            txfdlnth = "14";    //14
            txfdlnlh = 15.0F;    //15
            txfdlntv = "16";    //16
            txfdlnlv = 17.0F;    //17
            rxfdlnth = "18";    //18
            rxfdlnlh = 19.0F;    //19
            rxfdlntv = "20";    //20
            rxfdlnlv = 21.0F;    //21
            txpadpam = 22.0F;    //22
            rxpadlna = 23.0F;    //23
            txcompl = 24.0F;    //24
            rxcompl = 25.0F;    //25
            obsloss = 26.0F;    //26
            kvalue = 27.0F;    //27
            atwrno = 28;    //28
            nota = "29";    //29
            apoint = "30";    //30
            sdate = "31";    //31
            licence = "32";    //32
            mdate = "33";    //33
            mtime = "34";    //34
            userid = "35";    //35
        }


        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned MtAnte object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="mtAnte"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out MtAnte mtAnte, out SQLLEN[] nullInds)
        {
            mtAnte = new MtAnte();
            nullInds = NullHelper.CreateArrayOfNullInd(MtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[CALL1]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.call1 = "";
                nullInds[CALL1] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.call1 = Marshal.PtrToStringAnsi(tgtValPtrs[CALL1]).Trim();
                nullInds[CALL1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CALL2]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.call2 = "";
                nullInds[CALL2] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.call2 = Marshal.PtrToStringAnsi(tgtValPtrs[CALL2]).Trim();
                nullInds[CALL2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BNDCDE]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.bndcde = "";
                nullInds[BNDCDE] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.bndcde = Marshal.PtrToStringAnsi(tgtValPtrs[BNDCDE]).Trim();
                nullInds[BNDCDE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANUM]);
            if (nullInd != Constant.DB_NULL)
            {
                mtAnte.anum = Marshal.ReadInt16(tgtValPtrs[ANUM]);
                nullInds[ANUM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AUSE]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.ause = "";
                nullInds[AUSE] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.ause = Marshal.PtrToStringAnsi(tgtValPtrs[AUSE]).Trim();
                nullInds[AUSE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACODE]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.acode = "";
                nullInds[ACODE] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.acode = Marshal.PtrToStringAnsi(tgtValPtrs[ACODE]).Trim();
                nullInds[ACODE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AHT]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[AHT], F, 0, 1);
                mtAnte.aht = F[0];
                nullInds[AHT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AZMTH]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[AZMTH], F, 0, 1);
                mtAnte.azmth = F[0];
                nullInds[AZMTH] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ELVTN]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ELVTN], F, 0, 1);
                mtAnte.elvtn = F[0];
                nullInds[ELVTN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[DIST]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[DIST], F, 0, 1);
                mtAnte.dist = F[0];
                nullInds[DIST] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[OFFAZM]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.offazm = "";
                nullInds[OFFAZM] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.offazm = Marshal.PtrToStringAnsi(tgtValPtrs[OFFAZM]).Trim();
                nullInds[OFFAZM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TAZMTH]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[TAZMTH], F, 0, 1);
                mtAnte.tazmth = F[0];
                nullInds[TAZMTH] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TELVTN]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[TELVTN], F, 0, 1);
                mtAnte.telvtn = F[0];
                nullInds[TELVTN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TGAIN]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[TGAIN], F, 0, 1);
                mtAnte.tgain = F[0];
                nullInds[TGAIN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TXFDLNTH]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.txfdlnth = "";
                nullInds[TXFDLNTH] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.txfdlnth = Marshal.PtrToStringAnsi(tgtValPtrs[TXFDLNTH]).Trim();
                nullInds[TXFDLNTH] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TXFDLNLH]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[TXFDLNLH], F, 0, 1);
                mtAnte.txfdlnlh = F[0];
                nullInds[TXFDLNLH] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TXFDLNTV]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.txfdlntv = "";
                nullInds[TXFDLNTV] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.txfdlntv = Marshal.PtrToStringAnsi(tgtValPtrs[TXFDLNTV]).Trim();
                nullInds[TXFDLNTV] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TXFDLNLV]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[TXFDLNLV], F, 0, 1);
                mtAnte.txfdlnlv = F[0];
                nullInds[TXFDLNLV] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RXFDLNTH]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.rxfdlnth = "";
                nullInds[RXFDLNTH] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.rxfdlnth = Marshal.PtrToStringAnsi(tgtValPtrs[RXFDLNTH]).Trim();
                nullInds[RXFDLNTH] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RXFDLNLH]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RXFDLNLH], F, 0, 1);
                mtAnte.rxfdlnlh = F[0];
                nullInds[RXFDLNLH] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RXFDLNTV]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.rxfdlntv = "";
                nullInds[RXFDLNTV] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.rxfdlntv = Marshal.PtrToStringAnsi(tgtValPtrs[RXFDLNTV]).Trim();
                nullInds[RXFDLNTV] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RXFDLNLV]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RXFDLNLV], F, 0, 1);
                mtAnte.rxfdlnlv = F[0];
                nullInds[RXFDLNLV] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TXPADPAM]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[TXPADPAM], F, 0, 1);
                mtAnte.txpadpam = F[0];
                nullInds[TXPADPAM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RXPADLNA]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RXPADLNA], F, 0, 1);
                mtAnte.rxpadlna = F[0];
                nullInds[RXPADLNA] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TXCOMPL]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[TXCOMPL], F, 0, 1);
                mtAnte.txcompl = F[0];
                nullInds[TXCOMPL] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RXCOMPL]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RXCOMPL], F, 0, 1);
                mtAnte.rxcompl = F[0];
                nullInds[RXCOMPL] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[OBSLOSS]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[OBSLOSS], F, 0, 1);
                mtAnte.obsloss = F[0];
                nullInds[OBSLOSS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[KVALUE]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[KVALUE], F, 0, 1);
                mtAnte.kvalue = F[0];
                nullInds[KVALUE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ATWRNO]);
            if (nullInd != Constant.DB_NULL)
            {
                mtAnte.atwrno = Marshal.ReadInt16(tgtValPtrs[ATWRNO]);
                nullInds[ATWRNO] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[NOTA]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.nota = "";
                nullInds[NOTA] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.nota = Marshal.PtrToStringAnsi(tgtValPtrs[NOTA]).Trim();
                nullInds[NOTA] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[APOINT]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.apoint = "";
                nullInds[APOINT] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.apoint = Marshal.PtrToStringAnsi(tgtValPtrs[APOINT]).Trim();
                nullInds[APOINT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.sdate = "";
                nullInds[SDATE] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.sdate = Marshal.PtrToStringAnsi(tgtValPtrs[SDATE]).Trim();
                nullInds[SDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[USERID]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.userid = "";
                nullInds[USERID] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.userid = Marshal.PtrToStringAnsi(tgtValPtrs[USERID]).Trim();
                nullInds[USERID] = Constant.DB_NOT_NULL;
            }

            // The following lines for 'license' are *not* order-critical because
            // we are reading from an existing ODBC 'bound-pounter'.
            nullInd = Marshal.ReadInt64(nullIndPtrs[LICENCE]);
            if (nullInd == Constant.DB_NULL)
            {
                mtAnte.licence = "";
                nullInds[LICENCE] = Constant.DB_NULL;
            }
            else
            {
                mtAnte.licence = Marshal.PtrToStringAnsi(tgtValPtrs[LICENCE]).Trim();
                nullInds[LICENCE] = Constant.DB_NOT_NULL;
            }

        }

        /// <summary>
        /// This method returns an array of IntPtr that point to the start addresses of possibly 
        /// non-contiguous blocks of global (heap) memory, each of a sufficient size to hold the 
        /// value of a specific member of this object; each of the pointers is then registered
        /// with ODBC as a 'binding'. Similarly for the nullInd array associated with this object.
        /// The binding order is that of the columns in the table main.mt_ante.
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
            Bind.BindBufferToString(hStmt, CALL1 + 1, tgtValPtrs[CALL1], MtAnte.CALL1_SZ, nullIndPtrs[CALL1]);

            tgtValPtrs[CALL2] = Marshal.AllocHGlobal(CALL2_SZ + 1);
            nullIndPtrs[CALL2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CALL2 + 1, tgtValPtrs[CALL2], MtAnte.CALL2_SZ, nullIndPtrs[CALL2]);

            tgtValPtrs[BNDCDE] = Marshal.AllocHGlobal(BNDCDE_SZ + 1);
            nullIndPtrs[BNDCDE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, BNDCDE + 1, tgtValPtrs[BNDCDE], MtAnte.BNDCDE_SZ, nullIndPtrs[BNDCDE]);

            tgtValPtrs[ANUM] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[ANUM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, ANUM + 1, tgtValPtrs[ANUM], nullIndPtrs[ANUM]);

            tgtValPtrs[AUSE] = Marshal.AllocHGlobal(AUSE_SZ + 1);
            nullIndPtrs[AUSE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, AUSE + 1, tgtValPtrs[AUSE], MtAnte.AUSE_SZ, nullIndPtrs[AUSE]);

            tgtValPtrs[ACODE] = Marshal.AllocHGlobal(ACODE_SZ + 1);
            nullIndPtrs[ACODE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ACODE + 1, tgtValPtrs[ACODE], MtAnte.ACODE_SZ, nullIndPtrs[ACODE]);

            tgtValPtrs[AHT] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[AHT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, AHT + 1, tgtValPtrs[AHT], nullIndPtrs[AHT]);

            tgtValPtrs[AZMTH] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[AZMTH] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, AZMTH + 1, tgtValPtrs[AZMTH], nullIndPtrs[AZMTH]);

            tgtValPtrs[ELVTN] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[ELVTN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, ELVTN + 1, tgtValPtrs[ELVTN], nullIndPtrs[ELVTN]);

            tgtValPtrs[DIST] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[DIST] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, DIST + 1, tgtValPtrs[DIST], nullIndPtrs[DIST]);

            tgtValPtrs[OFFAZM] = Marshal.AllocHGlobal(OFFAZM_SZ + 1);
            nullIndPtrs[OFFAZM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, OFFAZM + 1, tgtValPtrs[OFFAZM], MtAnte.OFFAZM_SZ, nullIndPtrs[OFFAZM]);

            tgtValPtrs[TAZMTH] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[TAZMTH] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, TAZMTH + 1, tgtValPtrs[TAZMTH], nullIndPtrs[TAZMTH]);

            tgtValPtrs[TELVTN] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[TELVTN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, TELVTN + 1, tgtValPtrs[TELVTN], nullIndPtrs[TELVTN]);

            tgtValPtrs[TGAIN] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[TGAIN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, TGAIN + 1, tgtValPtrs[TGAIN], nullIndPtrs[TGAIN]);

            tgtValPtrs[TXFDLNTH] = Marshal.AllocHGlobal(TXFDLNTH_SZ + 1);
            nullIndPtrs[TXFDLNTH] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TXFDLNTH + 1, tgtValPtrs[TXFDLNTH], MtAnte.TXFDLNTH_SZ, nullIndPtrs[TXFDLNTH]);

            tgtValPtrs[TXFDLNLH] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[TXFDLNLH] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, TXFDLNLH + 1, tgtValPtrs[TXFDLNLH], nullIndPtrs[TXFDLNLH]);

            tgtValPtrs[TXFDLNTV] = Marshal.AllocHGlobal(TXFDLNTV_SZ + 1);
            nullIndPtrs[TXFDLNTV] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TXFDLNTV + 1, tgtValPtrs[TXFDLNTV], MtAnte.TXFDLNTV_SZ, nullIndPtrs[TXFDLNTV]);

            tgtValPtrs[TXFDLNLV] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[TXFDLNLV] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, TXFDLNLV + 1, tgtValPtrs[TXFDLNLV], nullIndPtrs[TXFDLNLV]);

            tgtValPtrs[RXFDLNTH] = Marshal.AllocHGlobal(RXFDLNTH_SZ + 1);
            nullIndPtrs[RXFDLNTH] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, RXFDLNTH + 1, tgtValPtrs[RXFDLNTH], MtAnte.RXFDLNTH_SZ, nullIndPtrs[RXFDLNTH]);

            tgtValPtrs[RXFDLNLH] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[RXFDLNLH] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, RXFDLNLH + 1, tgtValPtrs[RXFDLNLH], nullIndPtrs[RXFDLNLH]);

            tgtValPtrs[RXFDLNTV] = Marshal.AllocHGlobal(RXFDLNTV_SZ + 1);
            nullIndPtrs[RXFDLNTV] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, RXFDLNTV + 1, tgtValPtrs[RXFDLNTV], MtAnte.RXFDLNTV_SZ, nullIndPtrs[RXFDLNTV]);

            tgtValPtrs[RXFDLNLV] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[RXFDLNLV] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, RXFDLNLV + 1, tgtValPtrs[RXFDLNLV], nullIndPtrs[RXFDLNLV]);

            tgtValPtrs[TXPADPAM] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[TXPADPAM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, TXPADPAM + 1, tgtValPtrs[TXPADPAM], nullIndPtrs[TXPADPAM]);

            tgtValPtrs[RXPADLNA] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[RXPADLNA] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, RXPADLNA + 1, tgtValPtrs[RXPADLNA], nullIndPtrs[RXPADLNA]);

            tgtValPtrs[TXCOMPL] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[TXCOMPL] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, TXCOMPL + 1, tgtValPtrs[TXCOMPL], nullIndPtrs[TXCOMPL]);

            tgtValPtrs[RXCOMPL] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[RXCOMPL] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, RXCOMPL + 1, tgtValPtrs[RXCOMPL], nullIndPtrs[RXCOMPL]);

            tgtValPtrs[OBSLOSS] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[OBSLOSS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, OBSLOSS + 1, tgtValPtrs[OBSLOSS], nullIndPtrs[OBSLOSS]);

            tgtValPtrs[KVALUE] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[KVALUE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, KVALUE + 1, tgtValPtrs[KVALUE], nullIndPtrs[KVALUE]);

            tgtValPtrs[ATWRNO] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[ATWRNO] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, ATWRNO + 1, tgtValPtrs[ATWRNO], nullIndPtrs[ATWRNO]);

            tgtValPtrs[NOTA] = Marshal.AllocHGlobal(NOTA_SZ + 1);
            nullIndPtrs[NOTA] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, NOTA + 1, tgtValPtrs[NOTA], MtAnte.NOTA_SZ, nullIndPtrs[NOTA]);

            tgtValPtrs[APOINT] = Marshal.AllocHGlobal(APOINT_SZ + 1);
            nullIndPtrs[APOINT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, APOINT + 1, tgtValPtrs[APOINT], MtAnte.APOINT_SZ, nullIndPtrs[APOINT]);

            tgtValPtrs[SDATE] = Marshal.AllocHGlobal(SDATE_SZ + 1);
            nullIndPtrs[SDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SDATE + 1, tgtValPtrs[SDATE], MtAnte.SDATE_SZ, nullIndPtrs[SDATE]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], MtAnte.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], MtAnte.MTIME_SZ, nullIndPtrs[MTIME]);

            tgtValPtrs[USERID] = Marshal.AllocHGlobal(USERID_SZ + 1);
            nullIndPtrs[USERID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, USERID + 1, tgtValPtrs[USERID], MtAnte.USERID_SZ, nullIndPtrs[USERID]);

            // We use the column order of the table.main.mt_ante; this is 
            // taken care of by numbering LICENSE = 35.
            tgtValPtrs[LICENCE] = Marshal.AllocHGlobal(LICENCE_SZ + 1);
            nullIndPtrs[LICENCE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, LICENCE + 1, tgtValPtrs[LICENCE], MtAnte.LICENCE_SZ, nullIndPtrs[LICENCE]);
        }


        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the member fields together with
        /// their associated ODBC nullInds; the column order is that of the
        /// table main.mt_ante (not that of MtAnte).
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringWN(SQLLEN[] nullInds)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n===== MtAnte =====");

            sb.Append("\n" + nullInds[i++] + "      " + "call1 = " + call1);
            sb.Append("\n" + nullInds[i++] + "      " + "call2 = " + call2);
            sb.Append("\n" + nullInds[i++] + "      " + "bndcde = " + bndcde);
            sb.Append("\n" + nullInds[i++] + "      " + "anum = " + anum);
            sb.Append("\n" + nullInds[i++] + "      " + "ause = " + ause);
            sb.Append("\n" + nullInds[i++] + "      " + "acode = " + acode);
            sb.Append("\n" + nullInds[i++] + "      " + "aht = " + aht);
            sb.Append("\n" + nullInds[i++] + "      " + "azmth = " + azmth);
            sb.Append("\n" + nullInds[i++] + "      " + "elvtn = " + elvtn);
            sb.Append("\n" + nullInds[i++] + "      " + "dist = " + dist);
            sb.Append("\n" + nullInds[i++] + "      " + "offazm = " + offazm);
            sb.Append("\n" + nullInds[i++] + "      " + "tazmth = " + tazmth);
            sb.Append("\n" + nullInds[i++] + "      " + "telvtn = " + telvtn);
            sb.Append("\n" + nullInds[i++] + "      " + "tgain = " + tgain);
            sb.Append("\n" + nullInds[i++] + "      " + "txfdlnth = " + txfdlnth);
            sb.Append("\n" + nullInds[i++] + "      " + "txfdlnlh = " + txfdlnlh);
            sb.Append("\n" + nullInds[i++] + "      " + "txfdlntv = " + txfdlntv);
            sb.Append("\n" + nullInds[i++] + "      " + "txfdlnlv = " + txfdlnlv);
            sb.Append("\n" + nullInds[i++] + "      " + "rxfdlnth = " + rxfdlnth);
            sb.Append("\n" + nullInds[i++] + "      " + "rxfdlnlh = " + rxfdlnlh);
            sb.Append("\n" + nullInds[i++] + "      " + "rxfdlntv = " + rxfdlntv);
            sb.Append("\n" + nullInds[i++] + "      " + "rxfdlnlv = " + rxfdlnlv);
            sb.Append("\n" + nullInds[i++] + "      " + "txpadpam = " + txpadpam);
            sb.Append("\n" + nullInds[i++] + "      " + "rxpadlna = " + rxpadlna);
            sb.Append("\n" + nullInds[i++] + "      " + "txcompl = " + txcompl);
            sb.Append("\n" + nullInds[i++] + "      " + "rxcompl = " + rxcompl);
            sb.Append("\n" + nullInds[i++] + "      " + "obsloss = " + obsloss);
            sb.Append("\n" + nullInds[i++] + "      " + "kvalue = " + kvalue);
            sb.Append("\n" + nullInds[i++] + "      " + "atwrno = " + atwrno);
            sb.Append("\n" + nullInds[i++] + "      " + "nota = " + nota);
            sb.Append("\n" + nullInds[i++] + "      " + "apoint = " + apoint);
            sb.Append("\n" + nullInds[i++] + "      " + "sdate = " + sdate);
            sb.Append("\n" + nullInds[i++] + "      " + "mdate = " + mdate);
            sb.Append("\n" + nullInds[i++] + "      " + "mtime = " + mtime);
            sb.Append("\n" + nullInds[i++] + "      " + "userid = " + userid);

            // We will use the column order of the table.main.mt_ante.
            sb.Append("\n" + nullInds[i++] + "      " + "licence = " + licence);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns the member values of an object formatted
        /// as a CSV string that can be used to provide the VALUES in an SQL INSERT query;
        /// note that the position of the 'license' field differs between the sequence of
        /// members of the class MtAnte and the sequence of the columns of DB table main.mt_ante.
        /// </summary>
        /// <param name="nullInds"> - array of ODBC null indicators.</param>
        /// <returns></returns>
        public string ToStringAsCSV(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[MtAnte.CALL1] == Constant.DB_NULL ? "NULL," : "'" + call1.ToString() + "',");
            sb.Append(nullInds[MtAnte.CALL2] == Constant.DB_NULL ? "NULL," : "'" + call2.ToString() + "',");
            sb.Append(nullInds[MtAnte.BNDCDE] == Constant.DB_NULL ? "NULL," : "'" + bndcde.ToString() + "',");
            sb.Append(nullInds[MtAnte.ANUM] == Constant.DB_NULL ? "NULL," : "'" + anum.ToString() + "',");
            sb.Append(nullInds[MtAnte.AUSE] == Constant.DB_NULL ? "NULL," : "'" + ause.ToString() + "',");
            sb.Append(nullInds[MtAnte.ACODE] == Constant.DB_NULL ? "NULL," : "'" + acode.ToString() + "',");
            sb.Append(nullInds[MtAnte.AHT] == Constant.DB_NULL ? "NULL," : "'" + aht.ToString() + "',");
            sb.Append(nullInds[MtAnte.AZMTH] == Constant.DB_NULL ? "NULL," : "'" + azmth.ToString() + "',");
            sb.Append(nullInds[MtAnte.ELVTN] == Constant.DB_NULL ? "NULL," : "'" + elvtn.ToString() + "',");
            sb.Append(nullInds[MtAnte.DIST] == Constant.DB_NULL ? "NULL," : "'" + dist.ToString() + "',");
            sb.Append(nullInds[MtAnte.OFFAZM] == Constant.DB_NULL ? "NULL," : "'" + offazm.ToString() + "',");
            sb.Append(nullInds[MtAnte.TAZMTH] == Constant.DB_NULL ? "NULL," : "'" + tazmth.ToString() + "',");
            sb.Append(nullInds[MtAnte.TELVTN] == Constant.DB_NULL ? "NULL," : "'" + telvtn.ToString() + "',");
            sb.Append(nullInds[MtAnte.TGAIN] == Constant.DB_NULL ? "NULL," : "'" + tgain.ToString() + "',");
            sb.Append(nullInds[MtAnte.TXFDLNTH] == Constant.DB_NULL ? "NULL," : "'" + txfdlnth.ToString() + "',");
            sb.Append(nullInds[MtAnte.TXFDLNLH] == Constant.DB_NULL ? "NULL," : "'" + txfdlnlh.ToString() + "',");
            sb.Append(nullInds[MtAnte.TXFDLNTV] == Constant.DB_NULL ? "NULL," : "'" + txfdlntv.ToString() + "',");
            sb.Append(nullInds[MtAnte.TXFDLNLV] == Constant.DB_NULL ? "NULL," : "'" + txfdlnlv.ToString() + "',");
            sb.Append(nullInds[MtAnte.RXFDLNTH] == Constant.DB_NULL ? "NULL," : "'" + rxfdlnth.ToString() + "',");
            sb.Append(nullInds[MtAnte.RXFDLNLH] == Constant.DB_NULL ? "NULL," : "'" + rxfdlnlh.ToString() + "',");
            sb.Append(nullInds[MtAnte.RXFDLNTV] == Constant.DB_NULL ? "NULL," : "'" + rxfdlntv.ToString() + "',");
            sb.Append(nullInds[MtAnte.RXFDLNLV] == Constant.DB_NULL ? "NULL," : "'" + rxfdlnlv.ToString() + "',");
            sb.Append(nullInds[MtAnte.TXPADPAM] == Constant.DB_NULL ? "NULL," : "'" + txpadpam.ToString() + "',");
            sb.Append(nullInds[MtAnte.RXPADLNA] == Constant.DB_NULL ? "NULL," : "'" + rxpadlna.ToString() + "',");
            sb.Append(nullInds[MtAnte.TXCOMPL] == Constant.DB_NULL ? "NULL," : "'" + txcompl.ToString() + "',");
            sb.Append(nullInds[MtAnte.RXCOMPL] == Constant.DB_NULL ? "NULL," : "'" + rxcompl.ToString() + "',");
            sb.Append(nullInds[MtAnte.OBSLOSS] == Constant.DB_NULL ? "NULL," : "'" + obsloss.ToString() + "',");
            sb.Append(nullInds[MtAnte.KVALUE] == Constant.DB_NULL ? "NULL," : "'" + kvalue.ToString() + "',");
            sb.Append(nullInds[MtAnte.ATWRNO] == Constant.DB_NULL ? "NULL," : "'" + atwrno.ToString() + "',");
            sb.Append(nullInds[MtAnte.NOTA] == Constant.DB_NULL ? "NULL," : "'" + nota.ToString() + "',");
            sb.Append(nullInds[MtAnte.APOINT] == Constant.DB_NULL ? "NULL," : "'" + apoint.ToString() + "',");
            sb.Append(nullInds[MtAnte.SDATE] == Constant.DB_NULL ? "NULL," : "'" + sdate.ToString() + "',");
            sb.Append(nullInds[MtAnte.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',");
            sb.Append(nullInds[MtAnte.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "',");
            sb.Append(nullInds[MtAnte.USERID] == Constant.DB_NULL ? "NULL," : "'" + userid.ToString() + "',");

            // We will use the column order of the table.main.mt_ante.
            sb.Append(nullInds[MtAnte.LICENCE] == Constant.DB_NULL ? "NULL" : "'" + licence.ToString() + "'");

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

            sb.Append(columnNames[MtAnte.CALL1] + "=" + (nullInds[MtAnte.CALL1] == Constant.DB_NULL ? "NULL," : "'" + call1.ToString() + "',"));
            sb.Append(columnNames[MtAnte.CALL2] + "=" + (nullInds[MtAnte.CALL2] == Constant.DB_NULL ? "NULL," : "'" + call2.ToString() + "',"));
            sb.Append(columnNames[MtAnte.BNDCDE] + "=" + (nullInds[MtAnte.BNDCDE] == Constant.DB_NULL ? "NULL," : "'" + bndcde.ToString() + "',"));
            sb.Append(columnNames[MtAnte.ANUM] + "=" + (nullInds[MtAnte.ANUM] == Constant.DB_NULL ? "NULL," : "'" + anum.ToString() + "',"));
            sb.Append(columnNames[MtAnte.AUSE] + "=" + (nullInds[MtAnte.AUSE] == Constant.DB_NULL ? "NULL," : "'" + ause.ToString() + "',"));
            sb.Append(columnNames[MtAnte.ACODE] + "=" + (nullInds[MtAnte.ACODE] == Constant.DB_NULL ? "NULL," : "'" + acode.ToString() + "',"));
            sb.Append(columnNames[MtAnte.AHT] + "=" + (nullInds[MtAnte.AHT] == Constant.DB_NULL ? "NULL," : "'" + aht.ToString() + "',"));
            sb.Append(columnNames[MtAnte.AZMTH] + "=" + (nullInds[MtAnte.AZMTH] == Constant.DB_NULL ? "NULL," : "'" + azmth.ToString() + "',"));
            sb.Append(columnNames[MtAnte.ELVTN] + "=" + (nullInds[MtAnte.ELVTN] == Constant.DB_NULL ? "NULL," : "'" + elvtn.ToString() + "',"));
            sb.Append(columnNames[MtAnte.DIST] + "=" + (nullInds[MtAnte.DIST] == Constant.DB_NULL ? "NULL," : "'" + dist.ToString() + "',"));
            sb.Append(columnNames[MtAnte.OFFAZM] + "=" + (nullInds[MtAnte.OFFAZM] == Constant.DB_NULL ? "NULL," : "'" + offazm.ToString() + "',"));
            sb.Append(columnNames[MtAnte.TAZMTH] + "=" + (nullInds[MtAnte.TAZMTH] == Constant.DB_NULL ? "NULL," : "'" + tazmth.ToString() + "',"));
            sb.Append(columnNames[MtAnte.TELVTN] + "=" + (nullInds[MtAnte.TELVTN] == Constant.DB_NULL ? "NULL," : "'" + telvtn.ToString() + "',"));
            sb.Append(columnNames[MtAnte.TGAIN] + "=" + (nullInds[MtAnte.TGAIN] == Constant.DB_NULL ? "NULL," : "'" + tgain.ToString() + "',"));
            sb.Append(columnNames[MtAnte.TXFDLNTH] + "=" + (nullInds[MtAnte.TXFDLNTH] == Constant.DB_NULL ? "NULL," : "'" + txfdlnth.ToString() + "',"));
            sb.Append(columnNames[MtAnte.TXFDLNLH] + "=" + (nullInds[MtAnte.TXFDLNLH] == Constant.DB_NULL ? "NULL," : "'" + txfdlnlh.ToString() + "',"));
            sb.Append(columnNames[MtAnte.TXFDLNTV] + "=" + (nullInds[MtAnte.TXFDLNTV] == Constant.DB_NULL ? "NULL," : "'" + txfdlntv.ToString() + "',"));
            sb.Append(columnNames[MtAnte.TXFDLNLV] + "=" + (nullInds[MtAnte.TXFDLNLV] == Constant.DB_NULL ? "NULL," : "'" + txfdlnlv.ToString() + "',"));
            sb.Append(columnNames[MtAnte.RXFDLNTH] + "=" + (nullInds[MtAnte.RXFDLNTH] == Constant.DB_NULL ? "NULL," : "'" + rxfdlnth.ToString() + "',"));
            sb.Append(columnNames[MtAnte.RXFDLNLH] + "=" + (nullInds[MtAnte.RXFDLNLH] == Constant.DB_NULL ? "NULL," : "'" + rxfdlnlh.ToString() + "',"));
            sb.Append(columnNames[MtAnte.RXFDLNTV] + "=" + (nullInds[MtAnte.RXFDLNTV] == Constant.DB_NULL ? "NULL," : "'" + rxfdlntv.ToString() + "',"));
            sb.Append(columnNames[MtAnte.RXFDLNLV] + "=" + (nullInds[MtAnte.RXFDLNLV] == Constant.DB_NULL ? "NULL," : "'" + rxfdlnlv.ToString() + "',"));
            sb.Append(columnNames[MtAnte.TXPADPAM] + "=" + (nullInds[MtAnte.TXPADPAM] == Constant.DB_NULL ? "NULL," : "'" + txpadpam.ToString() + "',"));
            sb.Append(columnNames[MtAnte.RXPADLNA] + "=" + (nullInds[MtAnte.RXPADLNA] == Constant.DB_NULL ? "NULL," : "'" + rxpadlna.ToString() + "',"));
            sb.Append(columnNames[MtAnte.TXCOMPL] + "=" + (nullInds[MtAnte.TXCOMPL] == Constant.DB_NULL ? "NULL," : "'" + txcompl.ToString() + "',"));
            sb.Append(columnNames[MtAnte.RXCOMPL] + "=" + (nullInds[MtAnte.RXCOMPL] == Constant.DB_NULL ? "NULL," : "'" + rxcompl.ToString() + "',"));
            sb.Append(columnNames[MtAnte.OBSLOSS] + "=" + (nullInds[MtAnte.OBSLOSS] == Constant.DB_NULL ? "NULL," : "'" + obsloss.ToString() + "',"));
            sb.Append(columnNames[MtAnte.KVALUE] + "=" + (nullInds[MtAnte.KVALUE] == Constant.DB_NULL ? "NULL," : "'" + kvalue.ToString() + "',"));
            sb.Append(columnNames[MtAnte.ATWRNO] + "=" + (nullInds[MtAnte.ATWRNO] == Constant.DB_NULL ? "NULL," : "'" + atwrno.ToString() + "',"));
            sb.Append(columnNames[MtAnte.NOTA] + "=" + (nullInds[MtAnte.NOTA] == Constant.DB_NULL ? "NULL," : "'" + nota.ToString() + "',"));
            sb.Append(columnNames[MtAnte.APOINT] + "=" + (nullInds[MtAnte.APOINT] == Constant.DB_NULL ? "NULL," : "'" + apoint.ToString() + "',"));
            sb.Append(columnNames[MtAnte.SDATE] + "=" + (nullInds[MtAnte.SDATE] == Constant.DB_NULL ? "NULL," : "'" + sdate.ToString() + "',"));
            sb.Append(columnNames[MtAnte.MDATE] + "=" + (nullInds[MtAnte.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',"));
            sb.Append(columnNames[MtAnte.MTIME] + "=" + (nullInds[MtAnte.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "',"));
            sb.Append(columnNames[MtAnte.USERID] + "=" + (nullInds[MtAnte.USERID] == Constant.DB_NULL ? "NULL," : "'" + userid.ToString() + "',"));

            // We will use the column order of the table.main.mt_ante.
            sb.Append(columnNames[MtAnte.LICENCE] + "=" + (nullInds[MtAnte.LICENCE] == Constant.DB_NULL ? "NULL" : "'" + licence.ToString() + "'"));

            return sb.ToString();
        }

        /// <summary>
        /// This method returns true if this MtAnte object matches a
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
"CREATE TABLE [{0}].[mt_ante](" +
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
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
    "[userid] [char](12) NULL," +
    "[licence] [char](13) NULL," +
" CONSTRAINT [PK_mt_ante] PRIMARY KEY CLUSTERED " +
"(" +
    "[call1] ASC," +
    "[call2] ASC," +
    "[bndcde] ASC," +
    "[anum] ASC" +
") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";

    }
}
