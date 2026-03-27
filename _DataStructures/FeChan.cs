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
    /// This class has fields that are isomorphic with PDF table <b>&lt;userID&gt;.fe_&lt;pdfName&gt;_chan</b>    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FeChan
    {
        // IMPORTANT!
        // =========
        // The following qty. 29 member values correspond to the columns
        // of an FeChan table stored in the database; for simplicity, the
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

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CMD_SZ)]
        public string cmd;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RECSTAT_SZ)]
        public string recstat;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = LOCATION_SZ)]
        public string location;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALL1_SZ)]
        public string call1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CHID_SZ)]
        public string chid;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double freqtx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = POLTX_SZ)]
        public string poltx;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float maxtxpower;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float pwrtx;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float p4khz;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EQPTTX_SZ)]
        public string eqpttx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TRAFTX_SZ)]
        public string traftx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STATTX_SZ)]
        public string stattx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = FEETX_SZ)]
        public string feetx;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double freqrx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = POLRX_SZ)]
        public string polrx;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float pwrrx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EQPTRX_SZ)]
        public string eqptrx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TRAFRX_SZ)]
        public string trafrx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STATRX_SZ)]
        public string statrx;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float i20;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float it01;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float ip01;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = FEERX_SZ)]
        public string feerx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTC_SZ)]
        public string notc;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SRVCTX_SZ)]
        public string srvctx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SRVCRX_SZ)]
        public string srvcrx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //---------------------------------------------------------------------------------
        public const int NUM_COLUMNS = 29;

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int LOCATION = 2;
        public const int CALL1 = 3;
        public const int CHID = 4;
        public const int FREQTX = 5;
        public const int POLTX = 6;
        public const int MAXTXPOWER = 7;
        public const int PWRTX = 8;
        public const int P4KHZ = 9;
        public const int EQPTTX = 10;
        public const int TRAFTX = 11;
        public const int STATTX = 12;
        public const int FEETX = 13;
        public const int FREQRX = 14;
        public const int POLRX = 15;
        public const int PWRRX = 16;
        public const int EQPTRX = 17;
        public const int TRAFRX = 18;
        public const int STATRX = 19;
        public const int I20 = 20;
        public const int IT01 = 21;
        public const int IP01 = 22;
        public const int FEERX = 23;
        public const int NOTC = 24;
        public const int SRVCTX = 25;
        public const int SRVCRX = 26;
        public const int MDATE = 27;
        public const int MTIME = 28;

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int LOCATION_SZ = Constant.LOCATION_SZ;
        public const int CALL1_SZ = Constant.CALLSIGN_SZ;
        public const int CHID_SZ = Constant.CHID_SZ;
        public const int POLTX_SZ = Constant.POLTX_SZ;
        public const int EQPTTX_SZ = Constant.EQPTTX_SZ;
        public const int TRAFTX_SZ = Constant.TRAFTX_SZ;
        public const int STATTX_SZ = Constant.STATTX_SZ;
        public const int FEETX_SZ = Constant.FEETX_SZ;
        public const int POLRX_SZ = Constant.POLRX_SZ;
        public const int EQPTRX_SZ = Constant.EQPTRX_SZ;
        public const int TRAFRX_SZ = Constant.FE_CHAN_TRAFRX_SZ;
        public const int STATRX_SZ = Constant.STATRX_SZ;
        public const int FEERX_SZ = Constant.FEERX_SZ;
        public const int NOTC_SZ = Constant.NOTA_SZ;
        public const int SRVCTX_SZ = Constant.SRVCTX_SZ;
        public const int SRVCRX_SZ = Constant.SRVCTX_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //---------------------------------------------------------------------------------

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "location", "call1", "chid", "freqtx", "poltx", "maxtxpower", "pwrtx", "p4khz", "eqpttx", "traftx", "stattx", "feetx", "freqrx", "polrx", "pwrrx", "eqptrx", "trafrx", "statrx", "i20", "it01", "ip01", "feerx", "notc", "srvctx", "srvcrx", "mdate", "mtime" };

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

        /// <summary>
        /// The static constructor is automatically called once , before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static FeChan()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Default object constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public FeChan()
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
            const float FLOAT_INIT_VAL = 0.0f;
            const double DOUBLE_INIT_VAL = 0.0;

            cmd = STRING_INIT_VAL;
            recstat = STRING_INIT_VAL;
            location = STRING_INIT_VAL;
            call1 = STRING_INIT_VAL;
            chid = STRING_INIT_VAL;
            freqtx = DOUBLE_INIT_VAL;
            poltx = STRING_INIT_VAL;
            maxtxpower = FLOAT_INIT_VAL;
            pwrtx = FLOAT_INIT_VAL;
            p4khz = FLOAT_INIT_VAL;
            eqpttx = STRING_INIT_VAL;
            traftx = STRING_INIT_VAL;
            stattx = STRING_INIT_VAL;
            feetx = STRING_INIT_VAL;
            freqrx = DOUBLE_INIT_VAL;
            polrx = STRING_INIT_VAL;
            pwrrx = FLOAT_INIT_VAL;
            eqptrx = STRING_INIT_VAL;
            trafrx = STRING_INIT_VAL;
            statrx = STRING_INIT_VAL;
            i20 = FLOAT_INIT_VAL;
            it01 = FLOAT_INIT_VAL;
            ip01 = FLOAT_INIT_VAL;
            feerx = STRING_INIT_VAL;
            notc = STRING_INIT_VAL;
            srvctx = STRING_INIT_VAL;
            srvcrx = STRING_INIT_VAL;
            mdate = STRING_INIT_VAL;
            mtime = STRING_INIT_VAL;
        }

        /// <summary>
        /// This method returns a string that concatenates the 'key'
        /// fields call1 and chid.
        /// </summary>
        public string KeysToString()
        {
            return String.Format("location: {0} ;  call1: {1} ; chid: {2}", location, call1, chid);
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
            sb.Append("\ncall1 = " + call1);
            sb.Append("\nchid = " + chid);
            sb.Append("\nfreqtx = " + freqtx);
            sb.Append("\npoltx = " + poltx);
            sb.Append("\nmaxtxpower = " + maxtxpower);
            sb.Append("\npwrtx = " + pwrtx);
            sb.Append("\np4khz = " + p4khz);
            sb.Append("\neqpttx = " + eqpttx);
            sb.Append("\ntraftx = " + traftx);
            sb.Append("\nstattx = " + stattx);
            sb.Append("\nfeetx = " + feetx);
            sb.Append("\nfreqrx = " + freqrx);
            sb.Append("\npolrx = " + polrx);
            sb.Append("\npwrrx = " + pwrrx);
            sb.Append("\neqptrx = " + eqptrx);
            sb.Append("\ntrafrx = " + trafrx);
            sb.Append("\nstatrx = " + statrx);
            sb.Append("\ni20 = " + i20);
            sb.Append("\nit01 = " + it01);
            sb.Append("\nip01 = " + ip01);
            sb.Append("\nfeerx = " + feerx);
            sb.Append("\nnotc = " + notc);
            sb.Append("\nsrvctx = " + srvctx);
            sb.Append("\nsrvcrx = " + srvcrx);
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
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "call1 = " + call1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "chid = " + chid);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "freqtx = " + freqtx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "poltx = " + poltx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "maxtxpower = " + maxtxpower);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "pwrtx = " + pwrtx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "p4khz = " + p4khz);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "eqpttx = " + eqpttx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "traftx = " + traftx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "stattx = " + stattx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "feetx = " + feetx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "freqrx = " + freqrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "polrx = " + polrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "pwrrx = " + pwrrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "eqptrx = " + eqptrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "trafrx = " + trafrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "statrx = " + statrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "i20 = " + i20);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "it01 = " + it01);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ip01 = " + ip01);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "feerx = " + feerx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "notc = " + notc);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "srvctx = " + srvctx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "srvcrx = " + srvcrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate = " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime = " + mtime);

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

            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[CMD] = Marshal.StringToHGlobalAnsi(cmd);

            parameterValuePtr[RECSTAT] = Marshal.StringToHGlobalAnsi(recstat);

            parameterValuePtr[LOCATION] = Marshal.StringToHGlobalAnsi(location);

            parameterValuePtr[CALL1] = Marshal.StringToHGlobalAnsi(call1);

            parameterValuePtr[CHID] = Marshal.StringToHGlobalAnsi(chid);

            parameterValuePtr[FREQTX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = freqtx;
            Marshal.Copy(D, 0, parameterValuePtr[FREQTX], 1);

            parameterValuePtr[POLTX] = Marshal.StringToHGlobalAnsi(poltx);

            parameterValuePtr[MAXTXPOWER] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = maxtxpower;
            Marshal.Copy(F, 0, parameterValuePtr[MAXTXPOWER], 1);

            parameterValuePtr[PWRTX] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = pwrtx;
            Marshal.Copy(F, 0, parameterValuePtr[PWRTX], 1);

            parameterValuePtr[P4KHZ] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = p4khz;
            Marshal.Copy(F, 0, parameterValuePtr[P4KHZ], 1);

            parameterValuePtr[EQPTTX] = Marshal.StringToHGlobalAnsi(eqpttx);

            parameterValuePtr[TRAFTX] = Marshal.StringToHGlobalAnsi(traftx);

            parameterValuePtr[STATTX] = Marshal.StringToHGlobalAnsi(stattx);

            parameterValuePtr[FEETX] = Marshal.StringToHGlobalAnsi(feetx);

            parameterValuePtr[FREQRX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = freqrx;
            Marshal.Copy(D, 0, parameterValuePtr[FREQRX], 1);

            parameterValuePtr[POLRX] = Marshal.StringToHGlobalAnsi(polrx);

            parameterValuePtr[PWRRX] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = pwrrx;
            Marshal.Copy(F, 0, parameterValuePtr[PWRRX], 1);

            parameterValuePtr[EQPTRX] = Marshal.StringToHGlobalAnsi(eqptrx);

            parameterValuePtr[TRAFRX] = Marshal.StringToHGlobalAnsi(trafrx);

            parameterValuePtr[STATRX] = Marshal.StringToHGlobalAnsi(statrx);

            parameterValuePtr[I20] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = i20;
            Marshal.Copy(F, 0, parameterValuePtr[I20], 1);

            parameterValuePtr[IT01] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = it01;
            Marshal.Copy(F, 0, parameterValuePtr[IT01], 1);

            parameterValuePtr[IP01] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = ip01;
            Marshal.Copy(F, 0, parameterValuePtr[IP01], 1);

            parameterValuePtr[FEERX] = Marshal.StringToHGlobalAnsi(feerx);

            parameterValuePtr[NOTC] = Marshal.StringToHGlobalAnsi(notc);

            parameterValuePtr[SRVCTX] = Marshal.StringToHGlobalAnsi(srvctx);

            parameterValuePtr[SRVCRX] = Marshal.StringToHGlobalAnsi(srvcrx);

            parameterValuePtr[MDATE] = Marshal.StringToHGlobalAnsi(mdate);

            parameterValuePtr[MTIME] = Marshal.StringToHGlobalAnsi(mtime);

            return parameterValuePtr;
        }

        /*
        cmd
        recstat
        location
        call1
        chid
        freqtx
        poltx
        maxtxpower
        pwrtx
        p4khz
        eqpttx
        traftx
        stattx
        feetx
        freqrx
        polrx
        pwrrx
        eqptrx
        trafrx
        statrx
        i20
        it01
        ip01
        feerx
        notc
        srvctx
        srvcrx
        mdate
        mtime
        */


    }
}
