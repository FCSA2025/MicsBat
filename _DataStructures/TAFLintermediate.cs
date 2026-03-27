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

    public class TAFLintermediate
    {
        public string TxRx = "";
        public string callsign = "";
        public string call2 = "";
        public string bndcde = "";
        public string sitename = "";
        public int anum = int.MinValue;
        public string chid = "";
        public string lat = "";
        public string lon = "";
        public double grnd = double.MinValue;
        public string prov = "";
        public string oper = "";
        public string opnote = "";
        public string stats = "";
        public string icaccount = "";
        public string licence = "";
        public string sdate = "";
        public double Total_losses_dB = double.MinValue;
        public string acodetx = "";
        public double ahttx = double.MinValue;
        public double aztx = double.MinValue;
        public double elevtx = double.MinValue;
        public double freq = double.MinValue;
        public string pol = "";
        public double pwrtx = double.MinValue;
        public double losstx = double.MinValue;
        public double gaintx = double.MinValue;
        public string eqpttx = "";
        public string traftx = "";

        public string namerx = "";
        public int anumrx = int.MinValue;
        public string latrx = "";
        public string lonrx = "";
        public double grndrx = double.MinValue;
        public string provrx = "";
        public string operrx = "";
        public string acoderx = "";
        public double ahtrx = double.MinValue;
        public double azrx = double.MinValue;
        public double elevrx = double.MinValue;
        public double lossrx = double.MinValue;
        public double gainrx = double.MinValue;
        public string eqptrx = "";
        public string trafrx = "";

        public int RecordAction = int.MinValue;
        public int MICSanum = int.MinValue;

        // The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 46;

        // Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "TxRx", "callsign", "call2", "bndcde", "sitename", "anum", "chid", "lat", "lon", "grnd", "prov", "oper", "opnote", "stats", "icaccount", "licence", "sdate", "Total_losses_dB", "acodetx", "ahttx", "aztx", "elevtx", "freq", "pol", "pwrtx", "losstx", "gaintx", "eqpttx", "traftx", "namerx", "anumrx", "latrx", "lonrx", "grndrx", "provrx", "operrx", "acoderx", "ahtrx", "azrx", "elevrx", "lossrx", "gainrx", "eqptrx", "trafrx", "RecordAction", "MICSanum" };

        // Field position definitions.
        public const int TXRX = 0;
        public const int CALLSIGN = 1;
        public const int CALL2 = 2;
        public const int BNDCDE = 3;
        public const int SITENAME = 4;
        public const int ANUM = 5;
        public const int CHID = 6;
        public const int LAT = 7;
        public const int LON = 8;
        public const int GRND = 9;
        public const int PROV = 10;
        public const int OPER = 11;
        public const int OPNOTE = 12;
        public const int STATS = 13;
        public const int ICACCOUNT = 14;
        public const int LICENCE = 15;
        public const int SDATE = 16;
        public const int TOTAL_LOSSES_DB = 17;
        public const int ACODETX = 18;
        public const int AHTTX = 19;
        public const int AZTX = 20;
        public const int ELEVTX = 21;
        public const int FREQ = 22;
        public const int POL = 23;
        public const int PWRTX = 24;
        public const int LOSSTX = 25;
        public const int GAINTX = 26;
        public const int EQPTTX = 27;
        public const int TRAFTX = 28;
        public const int NAMERX = 29;
        public const int ANUMRX = 30;
        public const int LATRX = 31;
        public const int LONRX = 32;
        public const int GRNDRX = 33;
        public const int PROVRX = 34;
        public const int OPERRX = 35;
        public const int ACODERX = 36;
        public const int AHTRX = 37;
        public const int AZRX = 38;
        public const int ELEVRX = 39;
        public const int LOSSRX = 40;
        public const int GAINRX = 41;
        public const int EQPTRX = 42;
        public const int TRAFRX = 43;
        public const int RECORDACTION = 44;
        public const int MICSANUM = 45;

        // String fields : maximum lengths.
        public const int TXRX_SZ = 2;
        public const int CALLSIGN_SZ = 10;
        public const int CALL2_SZ = 10;
        public const int BNDCDE_SZ = 6;
        public const int SITENAME_SZ = 32;
        public const int CHID_SZ = 5;
        public const int LAT_SZ = 12;
        public const int LON_SZ = 13;
        public const int PROV_SZ = 2;
        public const int OPER_SZ = 10;
        public const int OPNOTE_SZ = 2;
        public const int STATS_SZ = 1;
        public const int ICACCOUNT_SZ = 13;
        public const int LICENCE_SZ = 20;
        public const int SDATE_SZ = 12;
        public const int ACODETX_SZ = 40;
        public const int POL_SZ = 1;
        public const int EQPTTX_SZ = 10;
        public const int TRAFTX_SZ = 10;
        public const int NAMERX_SZ = 255;
        public const int LATRX_SZ = 12;
        public const int LONRX_SZ = 13;
        public const int PROVRX_SZ = 2;
        public const int OPERRX_SZ = 255;
        public const int ACODERX_SZ = 40;
        public const int EQPTRX_SZ = 10;
        public const int TRAFRX_SZ = 10;

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static TAFLintermediate()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
       }

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
        /// This method returns an annotated multi-line string that shows the current
        /// values of the member variables of this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n ===== TAFLintermediate ===== ");

            sb.Append("\nTxRx = " + TxRx);
            sb.Append("\ncallsign = " + callsign);
            sb.Append("\ncall2 = " + call2);
            sb.Append("\nbndcde = " + bndcde);
            sb.Append("\nsitename = " + sitename);
            sb.Append("\nanum = " + anum);
            sb.Append("\nchid = " + chid);
            sb.Append("\nlat = " + lat);
            sb.Append("\nlon = " + lon);
            sb.Append("\ngrnd = " + grnd);
            sb.Append("\nprov = " + prov);
            sb.Append("\noper = " + oper);
            sb.Append("\nopnote = " + opnote);
            sb.Append("\nstats = " + stats);
            sb.Append("\nicaccount = " + icaccount);
            sb.Append("\nlicence = " + licence);
            sb.Append("\nsdate = " + sdate);
            sb.Append("\nTotal_losses_dB = " + Total_losses_dB);
            sb.Append("\nacodetx = " + acodetx);
            sb.Append("\nahttx = " + ahttx);
            sb.Append("\naztx = " + aztx);
            sb.Append("\nelevtx = " + elevtx);
            sb.Append("\nfreq = " + freq);
            sb.Append("\npol = " + pol);
            sb.Append("\npwrtx = " + pwrtx);
            sb.Append("\nlosstx = " + losstx);
            sb.Append("\ngaintx = " + gaintx);
            sb.Append("\neqpttx = " + eqpttx);
            sb.Append("\ntraftx = " + traftx);
            sb.Append("\nnamerx = " + namerx);
            sb.Append("\nanumrx = " + anumrx);
            sb.Append("\nlatrx = " + latrx);
            sb.Append("\nlonrx = " + lonrx);
            sb.Append("\ngrndrx = " + grndrx);
            sb.Append("\nprovrx = " + provrx);
            sb.Append("\noperrx = " + operrx);
            sb.Append("\nacoderx = " + acoderx);
            sb.Append("\nahtrx = " + ahtrx);
            sb.Append("\nazrx = " + azrx);
            sb.Append("\nelevrx = " + elevrx);
            sb.Append("\nlossrx = " + lossrx);
            sb.Append("\ngainrx = " + gainrx);
            sb.Append("\neqptrx = " + eqptrx);
            sb.Append("\ntrafrx = " + trafrx);
            sb.Append("\nRecordAction = " + RecordAction);
            sb.Append("\nMICSanum = " + MICSanum);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated multi-line string that shows the current
        /// values of the member variables of this object as well as the associated
        /// nullInd values.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringWN(SQLLEN[] nullInds)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("\n ===== TAFLintermediate ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "TxRx = " + TxRx);
            sb.Append("\n" + nullInds[i++] + "      " + "callsign = " + callsign);
            sb.Append("\n" + nullInds[i++] + "      " + "call2 = " + call2);
            sb.Append("\n" + nullInds[i++] + "      " + "bndcde = " + bndcde);
            sb.Append("\n" + nullInds[i++] + "      " + "sitename = " + sitename);
            sb.Append("\n" + nullInds[i++] + "      " + "anum = " + anum);
            sb.Append("\n" + nullInds[i++] + "      " + "chid = " + chid);
            sb.Append("\n" + nullInds[i++] + "      " + "lat = " + lat);
            sb.Append("\n" + nullInds[i++] + "      " + "lon = " + lon);
            sb.Append("\n" + nullInds[i++] + "      " + "grnd = " + grnd);
            sb.Append("\n" + nullInds[i++] + "      " + "prov = " + prov);
            sb.Append("\n" + nullInds[i++] + "      " + "oper = " + oper);
            sb.Append("\n" + nullInds[i++] + "      " + "opnote = " + opnote);
            sb.Append("\n" + nullInds[i++] + "      " + "stats = " + stats);
            sb.Append("\n" + nullInds[i++] + "      " + "icaccount = " + icaccount);
            sb.Append("\n" + nullInds[i++] + "      " + "licence = " + licence);
            sb.Append("\n" + nullInds[i++] + "      " + "sdate = " + sdate);
            sb.Append("\n" + nullInds[i++] + "      " + "Total_losses_dB = " + Total_losses_dB);
            sb.Append("\n" + nullInds[i++] + "      " + "acodetx = " + acodetx);
            sb.Append("\n" + nullInds[i++] + "      " + "ahttx = " + ahttx);
            sb.Append("\n" + nullInds[i++] + "      " + "aztx = " + aztx);
            sb.Append("\n" + nullInds[i++] + "      " + "elevtx = " + elevtx);
            sb.Append("\n" + nullInds[i++] + "      " + "freq = " + freq);
            sb.Append("\n" + nullInds[i++] + "      " + "pol = " + pol);
            sb.Append("\n" + nullInds[i++] + "      " + "pwrtx = " + pwrtx);
            sb.Append("\n" + nullInds[i++] + "      " + "losstx = " + losstx);
            sb.Append("\n" + nullInds[i++] + "      " + "gaintx = " + gaintx);
            sb.Append("\n" + nullInds[i++] + "      " + "eqpttx = " + eqpttx);
            sb.Append("\n" + nullInds[i++] + "      " + "traftx = " + traftx);
            sb.Append("\n" + nullInds[i++] + "      " + "namerx = " + namerx);
            sb.Append("\n" + nullInds[i++] + "      " + "anumrx = " + anumrx);
            sb.Append("\n" + nullInds[i++] + "      " + "latrx = " + latrx);
            sb.Append("\n" + nullInds[i++] + "      " + "lonrx = " + lonrx);
            sb.Append("\n" + nullInds[i++] + "      " + "grndrx = " + grndrx);
            sb.Append("\n" + nullInds[i++] + "      " + "provrx = " + provrx);
            sb.Append("\n" + nullInds[i++] + "      " + "operrx = " + operrx);
            sb.Append("\n" + nullInds[i++] + "      " + "acoderx = " + acoderx);
            sb.Append("\n" + nullInds[i++] + "      " + "ahtrx = " + ahtrx);
            sb.Append("\n" + nullInds[i++] + "      " + "azrx = " + azrx);
            sb.Append("\n" + nullInds[i++] + "      " + "elevrx = " + elevrx);
            sb.Append("\n" + nullInds[i++] + "      " + "lossrx = " + lossrx);
            sb.Append("\n" + nullInds[i++] + "      " + "gainrx = " + gainrx);
            sb.Append("\n" + nullInds[i++] + "      " + "eqptrx = " + eqptrx);
            sb.Append("\n" + nullInds[i++] + "      " + "trafrx = " + trafrx);
            sb.Append("\n" + nullInds[i++] + "      " + "RecordAction = " + RecordAction);
            sb.Append("\n" + nullInds[i++] + "      " + "MICSanum = " + MICSanum);

            return sb.ToString();
        }

        public SQLPOINTER[] CopyToArrayOfSQLPOINTERinGlobalMemory()
        {
            //Can only copy arrays of float and double into native memory using Marshal method.
            float[] F = new float[1];
            double[] D = new double[1];

            SQLPOINTER[] parameterValuePtrs = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtrs[TXRX] = Marshal.StringToHGlobalAnsi(TxRx);

            parameterValuePtrs[CALLSIGN] = Marshal.StringToHGlobalAnsi(callsign);

            parameterValuePtrs[CALL2] = Marshal.StringToHGlobalAnsi(call2);

            parameterValuePtrs[BNDCDE] = Marshal.StringToHGlobalAnsi(bndcde);

            parameterValuePtrs[SITENAME] = Marshal.StringToHGlobalAnsi(sitename);

            parameterValuePtrs[ANUM] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtrs[ANUM], anum);

            parameterValuePtrs[CHID] = Marshal.StringToHGlobalAnsi(chid);

            parameterValuePtrs[LAT] = Marshal.StringToHGlobalAnsi(lat);

            parameterValuePtrs[LON] = Marshal.StringToHGlobalAnsi(lon);

            parameterValuePtrs[GRND] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = grnd;
            Marshal.Copy(D, 0, parameterValuePtrs[GRND], 1);

            parameterValuePtrs[PROV] = Marshal.StringToHGlobalAnsi(prov);

            parameterValuePtrs[OPER] = Marshal.StringToHGlobalAnsi(oper);

            parameterValuePtrs[OPNOTE] = Marshal.StringToHGlobalAnsi(opnote);

            parameterValuePtrs[STATS] = Marshal.StringToHGlobalAnsi(stats);

            parameterValuePtrs[ICACCOUNT] = Marshal.StringToHGlobalAnsi(icaccount);

            parameterValuePtrs[LICENCE] = Marshal.StringToHGlobalAnsi(licence);

            parameterValuePtrs[SDATE] = Marshal.StringToHGlobalAnsi(sdate);

            parameterValuePtrs[TOTAL_LOSSES_DB] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = Total_losses_dB;
            Marshal.Copy(D, 0, parameterValuePtrs[TOTAL_LOSSES_DB], 1);

            parameterValuePtrs[ACODETX] = Marshal.StringToHGlobalAnsi(acodetx);

            parameterValuePtrs[AHTTX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = ahttx;
            Marshal.Copy(D, 0, parameterValuePtrs[AHTTX], 1);

            parameterValuePtrs[AZTX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = aztx;
            Marshal.Copy(D, 0, parameterValuePtrs[AZTX], 1);

            parameterValuePtrs[ELEVTX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = elevtx;
            Marshal.Copy(D, 0, parameterValuePtrs[ELEVTX], 1);

            parameterValuePtrs[FREQ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = freq;
            Marshal.Copy(D, 0, parameterValuePtrs[FREQ], 1);

            parameterValuePtrs[POL] = Marshal.StringToHGlobalAnsi(pol);

            parameterValuePtrs[PWRTX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = pwrtx;
            Marshal.Copy(D, 0, parameterValuePtrs[PWRTX], 1);

            parameterValuePtrs[LOSSTX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = losstx;
            Marshal.Copy(D, 0, parameterValuePtrs[LOSSTX], 1);

            parameterValuePtrs[GAINTX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = gaintx;
            Marshal.Copy(D, 0, parameterValuePtrs[GAINTX], 1);

            parameterValuePtrs[EQPTTX] = Marshal.StringToHGlobalAnsi(eqpttx);

            parameterValuePtrs[TRAFTX] = Marshal.StringToHGlobalAnsi(traftx);

            parameterValuePtrs[NAMERX] = Marshal.StringToHGlobalAnsi(namerx);

            parameterValuePtrs[ANUMRX] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtrs[ANUMRX], anumrx);

            parameterValuePtrs[LATRX] = Marshal.StringToHGlobalAnsi(latrx);

            parameterValuePtrs[LONRX] = Marshal.StringToHGlobalAnsi(lonrx);

            parameterValuePtrs[GRNDRX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = grndrx;
            Marshal.Copy(D, 0, parameterValuePtrs[GRNDRX], 1);

            parameterValuePtrs[PROVRX] = Marshal.StringToHGlobalAnsi(provrx);

            parameterValuePtrs[OPERRX] = Marshal.StringToHGlobalAnsi(operrx);

            parameterValuePtrs[ACODERX] = Marshal.StringToHGlobalAnsi(acoderx);

            parameterValuePtrs[AHTRX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = ahtrx;
            Marshal.Copy(D, 0, parameterValuePtrs[AHTRX], 1);

            parameterValuePtrs[AZRX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = azrx;
            Marshal.Copy(D, 0, parameterValuePtrs[AZRX], 1);

            parameterValuePtrs[ELEVRX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = elevrx;
            Marshal.Copy(D, 0, parameterValuePtrs[ELEVRX], 1);

            parameterValuePtrs[LOSSRX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = lossrx;
            Marshal.Copy(D, 0, parameterValuePtrs[LOSSRX], 1);

            parameterValuePtrs[GAINRX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = gainrx;
            Marshal.Copy(D, 0, parameterValuePtrs[GAINRX], 1);

            parameterValuePtrs[EQPTRX] = Marshal.StringToHGlobalAnsi(eqptrx);

            parameterValuePtrs[TRAFRX] = Marshal.StringToHGlobalAnsi(trafrx);

            parameterValuePtrs[RECORDACTION] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtrs[RECORDACTION], RecordAction);

            parameterValuePtrs[MICSANUM] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtrs[MICSANUM], MICSanum);

            return parameterValuePtrs;
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

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[{1}]" +
"(" +
    "[TxRx] [varchar](2) NOT NULL, " +
    "[callsign] [varchar](10) NOT NULL, " +
    "[call2] [varchar](10) NULL, " +
    "[bndcde] [varchar](6) NULL, " +
    "[sitename] [varchar](32) NULL, " +
    "[anum] [int] NULL, " +
    "[chid] [varchar](5) NULL, " +
    "[lat] [varchar](12) NULL, " +
    "[lon] [varchar](13) NULL, " +
    "[grnd] [float] NULL, " +
    "[prov] [varchar](2) NULL, " +
    "[oper] [varchar](10) NULL, " +
    "[opnote] [varchar](2) NULL, " +
    "[stats] [varchar](1) NULL, " +
    "[icaccount] [varchar](13) NULL, " +
    "[licence] [varchar](20) NULL, " +
    "[sdate] [varchar](12) NULL, " +
    "[Total_losses_dB] [float] NULL, " +
    "[acodetx] [varchar](40) NULL, " +
    "[ahttx] [float] NULL, " +
    "[aztx] [float] NULL, " +
    "[elevtx] [float] NULL, " +
    "[freq] [float] NULL, " +
    "[pol] [varchar](1) NULL, " +
    "[pwrtx] [float] NULL, " +
    "[losstx] [float] NULL, " +
    "[gaintx] [float] NULL, " +
    "[eqpttx] [varchar](10) NULL, " +
    "[traftx] [varchar](10) NULL, " +
    "[namerx] [varchar](255) NULL, " +
    "[anumrx] [int] NULL, " +
    "[latrx] [varchar](12) NULL, " +
    "[lonrx] [varchar](13) NULL, " +
    "[grndrx] [float] NULL, " +
    "[provrx] [varchar](2) NULL, " +
    "[operrx] [varchar](255) NULL, " +
    "[acoderx] [varchar](40) NULL, " +
    "[ahtrx] [float] NULL, " +
    "[azrx] [float] NULL, " +
    "[elevrx] [float] NULL, " +
    "[lossrx] [float] NULL, " +
    "[gainrx] [float] NULL, " +
    "[eqptrx] [varchar](10) NULL, " +
    "[trafrx] [varchar](10) NULL, " +
    "[RecordAction] [int] NULL, " +
    "[MICSanum][int] NULL " +
") ON[PRIMARY]";






    }
}
