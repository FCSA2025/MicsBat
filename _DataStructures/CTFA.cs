using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using _Configuration;
    using _NewLib;
    using System.Runtime.InteropServices;
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

    public class CTFA
    {
        private double mLowerFreqKHz;
        private double mUpperFreqKHz;

        private string mFixed;
        private string mEarthToSatellite;
        private string mSatelliteToEarth;

        private string mSrspID;

        public double LowerFreqKHz { get { return mLowerFreqKHz; } set { mLowerFreqKHz = value; } }
        public double UpperFreqKHz { get { return mUpperFreqKHz; } set { mUpperFreqKHz = value; } }
        public string Fixed { get { return mFixed; } set { mFixed = value; } }
        public string EarthToSatellite { get { return mEarthToSatellite; } set { mEarthToSatellite = value; } }
        public string SatelliteToEarth { get { return mSatelliteToEarth; } set { mSatelliteToEarth = value; } }
        public string SrspID { get { return mSrspID; } set { mSrspID = value; } }

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 6;

        //Array of strings providing the database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "lowerFreqKHz", "upperFreqKHz", "fixed", "earthToSatellite", "satelliteToEarth", "srspID" };

        public const int LOWERFREQKHZ = 0;
        public const int UPPERFREQKHZ = 1;
        public const int FIXED = 2;
        public const int EARTHTOSATELLITE = 3;
        public const int SATELLITETOEARTH = 4;
        public const int SRSPID = 5;

        public const int FIXED_SZ = 1 + 1;
        public const int EARTHTOSATELLITE_SZ = 1 + 1;
        public const int SATELLITETOEARTH_SZ = 1 + 1;
        public const int SRSPID_SZ = 256 + 1;

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static CTFA()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        public CTFA()
        {
            mLowerFreqKHz = 0.0;
            mUpperFreqKHz = 0.0;

            mFixed = "N";
            mEarthToSatellite = "N";
            mSatelliteToEarth = "N";

            mSrspID = "";
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

        public SQLPOINTER[] CopyToArrayOfSQLPOINTERinGlobalMemory()
        {
            //Can only copy arrays of float and double into native memory using Marshal method.
            double[] D = new double[1];

            SQLPOINTER[] parameterValuePtrs = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtrs[LOWERFREQKHZ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = mLowerFreqKHz;
            Marshal.Copy(D, 0, parameterValuePtrs[LOWERFREQKHZ], 1);

            parameterValuePtrs[UPPERFREQKHZ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = mUpperFreqKHz;
            Marshal.Copy(D, 0, parameterValuePtrs[UPPERFREQKHZ], 1);

            parameterValuePtrs[FIXED] = Marshal.StringToHGlobalAnsi(mFixed);

            parameterValuePtrs[EARTHTOSATELLITE] = Marshal.StringToHGlobalAnsi(mEarthToSatellite);

            parameterValuePtrs[SATELLITETOEARTH] = Marshal.StringToHGlobalAnsi(mSatelliteToEarth);

            parameterValuePtrs[SRSPID] = Marshal.StringToHGlobalAnsi(mSrspID);

            return parameterValuePtrs;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n ===== CTFA ===== ");

            sb.Append("\nmLowerFreqKHz = " + mLowerFreqKHz);
            sb.Append("\nmUpperFreqKHz = " + mUpperFreqKHz);
            sb.Append("\nmFixed = " + mFixed);
            sb.Append("\nmEarthToSatellite = " + mEarthToSatellite);
            sb.Append("\nmSatelliteToEarth = " + mSatelliteToEarth);
            sb.Append("\nmSrspID = " + mSrspID);

            return sb.ToString();
        }

        public string ToStringWN(SQLLEN[] nullInds)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("\n ===== CTFA ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "mLowerFreqKHz = " + mLowerFreqKHz);
            sb.Append("\n" + nullInds[i++] + "      " + "mUpperFreqKHz = " + mUpperFreqKHz);
            sb.Append("\n" + nullInds[i++] + "      " + "mFixed = " + mFixed);
            sb.Append("\n" + nullInds[i++] + "      " + "mEarthToSatellite = " + mEarthToSatellite);
            sb.Append("\n" + nullInds[i++] + "      " + "mSatelliteToEarth = " + mSatelliteToEarth);
            sb.Append("\n" + nullInds[i++] + "      " + "mSrspID = " + mSrspID);

            return sb.ToString();
        }

        public string ToStringAsCSV(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[CTFA.LOWERFREQKHZ] == Constant.DB_NULL ? "NULL, " : "'" + mLowerFreqKHz.ToString() + "', ");
            sb.Append(nullInds[CTFA.UPPERFREQKHZ] == Constant.DB_NULL ? "NULL, " : "'" + mUpperFreqKHz.ToString() + "', ");
            sb.Append(nullInds[CTFA.FIXED] == Constant.DB_NULL ? "NULL, " : "'" + mFixed.ToString() + "', ");
            sb.Append(nullInds[CTFA.EARTHTOSATELLITE] == Constant.DB_NULL ? "NULL, " : "'" + mEarthToSatellite.ToString() + "', ");
            sb.Append(nullInds[CTFA.SATELLITETOEARTH] == Constant.DB_NULL ? "NULL, " : "'" + mSatelliteToEarth.ToString() + "', ");
            sb.Append(nullInds[CTFA.SRSPID] == Constant.DB_NULL ? "NULL, " : "'" + mSatelliteToEarth.ToString() + "' ");

            return sb.ToString();
        }

        public string ToStringAsCSVexport(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[CTFA.LOWERFREQKHZ] == Constant.DB_NULL ? "\"\"," : "\"" + mLowerFreqKHz.ToString() + "\",");
            sb.Append(nullInds[CTFA.UPPERFREQKHZ] == Constant.DB_NULL ? "\"\"," : "\"" + mUpperFreqKHz.ToString() + "\",");
            sb.Append(nullInds[CTFA.FIXED] == Constant.DB_NULL ? "\"\"," : "\"" + mFixed.ToString() + "\",");
            sb.Append(nullInds[CTFA.EARTHTOSATELLITE] == Constant.DB_NULL ? "\"\"," : "\"" + mEarthToSatellite.ToString() + "\",");
            sb.Append(nullInds[CTFA.SATELLITETOEARTH] == Constant.DB_NULL ? "\"\"," : "\"" + mSatelliteToEarth.ToString() + "\",");
            sb.Append(nullInds[CTFA.SRSPID] == Constant.DB_NULL ? "\"\"," : "\"" + mSrspID.ToString() + "\"");

            return sb.ToString();
        }

        public string ToStringAsCSVequates(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(columnNames[CTFA.LOWERFREQKHZ] + " = " + (nullInds[CTFA.LOWERFREQKHZ] == Constant.DB_NULL ? "NULL, " : "'" + mLowerFreqKHz.ToString() + "', "));
            sb.Append(columnNames[CTFA.UPPERFREQKHZ] + " = " + (nullInds[CTFA.UPPERFREQKHZ] == Constant.DB_NULL ? "NULL, " : "'" + mUpperFreqKHz.ToString() + "', "));
            sb.Append(columnNames[CTFA.FIXED] + " = " + (nullInds[CTFA.FIXED] == Constant.DB_NULL ? "NULL, " : "'" + mFixed.ToString() + "', "));
            sb.Append(columnNames[CTFA.EARTHTOSATELLITE] + " = " + (nullInds[CTFA.EARTHTOSATELLITE] == Constant.DB_NULL ? "NULL, " : "'" + mEarthToSatellite.ToString() + "', "));
            sb.Append(columnNames[CTFA.SATELLITETOEARTH] + " = " + (nullInds[CTFA.SATELLITETOEARTH] == Constant.DB_NULL ? "NULL, " : "'" + mSatelliteToEarth.ToString() + "', "));
            sb.Append(columnNames[CTFA.SATELLITETOEARTH] + " = " + (nullInds[CTFA.SRSPID] == Constant.DB_NULL ? "NULL, " : "'" + mSrspID.ToString() + "' "));

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

        public static void BindPtrsToCols(SQLHANDLE hStmt, out SQLPOINTER[] tgtValPtrs, out SQLPOINTER[] nullIndPtrs)
        {
            tgtValPtrs = new SQLPOINTER[NUM_COLUMNS];
            nullIndPtrs = new SQLPOINTER[NUM_COLUMNS];

            tgtValPtrs[LOWERFREQKHZ] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[LOWERFREQKHZ] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, LOWERFREQKHZ + 1, tgtValPtrs[LOWERFREQKHZ], nullIndPtrs[LOWERFREQKHZ]);

            tgtValPtrs[UPPERFREQKHZ] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[UPPERFREQKHZ] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, UPPERFREQKHZ + 1, tgtValPtrs[UPPERFREQKHZ], nullIndPtrs[UPPERFREQKHZ]);

            tgtValPtrs[FIXED] = Marshal.AllocHGlobal(FIXED_SZ + 1);
            nullIndPtrs[FIXED] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, FIXED + 1, tgtValPtrs[FIXED], CTFA.FIXED_SZ, nullIndPtrs[FIXED]);

            tgtValPtrs[EARTHTOSATELLITE] = Marshal.AllocHGlobal(EARTHTOSATELLITE_SZ + 1);
            nullIndPtrs[EARTHTOSATELLITE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EARTHTOSATELLITE + 1, tgtValPtrs[EARTHTOSATELLITE], CTFA.EARTHTOSATELLITE_SZ, nullIndPtrs[EARTHTOSATELLITE]);

            tgtValPtrs[SATELLITETOEARTH] = Marshal.AllocHGlobal(SATELLITETOEARTH_SZ + 1);
            nullIndPtrs[SATELLITETOEARTH] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SATELLITETOEARTH + 1, tgtValPtrs[SATELLITETOEARTH], CTFA.SATELLITETOEARTH_SZ, nullIndPtrs[SATELLITETOEARTH]);

            tgtValPtrs[SRSPID] = Marshal.AllocHGlobal(SRSPID_SZ + 1);
            nullIndPtrs[SRSPID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SRSPID + 1, tgtValPtrs[SRSPID], CTFA.SRSPID_SZ, nullIndPtrs[SRSPID]);
        }

        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out CTFA cTFA, out SQLLEN[] nullInds)
        {
            cTFA = new CTFA();
            nullInds = NullHelper.CreateArrayOfNullInd(CTFA.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[LOWERFREQKHZ]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[LOWERFREQKHZ], D, 0, 1);
                cTFA.mLowerFreqKHz = D[0];
                nullInds[LOWERFREQKHZ] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[UPPERFREQKHZ]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[UPPERFREQKHZ], D, 0, 1);
                cTFA.mUpperFreqKHz = D[0];
                nullInds[UPPERFREQKHZ] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[FIXED]);
            if (nullInd == Constant.DB_NULL)
            {
                cTFA.mFixed = "";
                nullInds[FIXED] = Constant.DB_NULL;
            }
            else
            {
                cTFA.mFixed = Marshal.PtrToStringAnsi(tgtValPtrs[FIXED]).Trim();
                nullInds[FIXED] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EARTHTOSATELLITE]);
            if (nullInd == Constant.DB_NULL)
            {
                cTFA.mEarthToSatellite = "";
                nullInds[EARTHTOSATELLITE] = Constant.DB_NULL;
            }
            else
            {
                cTFA.mEarthToSatellite = Marshal.PtrToStringAnsi(tgtValPtrs[EARTHTOSATELLITE]).Trim();
                nullInds[EARTHTOSATELLITE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SRSPID]);
            if (nullInd == Constant.DB_NULL)
            {
                cTFA.mSrspID = "";
                nullInds[SRSPID] = Constant.DB_NULL;
            }
            else
            {
                cTFA.mSrspID = Marshal.PtrToStringAnsi(tgtValPtrs[SRSPID]).Trim();
                nullInds[SRSPID] = Constant.DB_NOT_NULL;
            }

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

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[{1}]" +
"(" +
    "[lowerFreqKHz][float] NOT NULL," +
    "[upperFreqKHz] [float] NOT NULL," +
    "[fixed] [char](1) NOT NULL," +
    "[earthToSatellite] [char](1) NOT NULL," +
    "[satelliteToEarth] [char](1) NOT NULL," +
    "[srspID] [varchar](256) NOT NULL" +
") ON[PRIMARY]";


        /* Matching CTFA bands with SRSB bands.
         
        SELECT S.srspID, S.span, S._of, S.freqLoKHz, S.freqHiKHz, *
            FROM hulme.CTFA2022 AS C
            INNER JOIN hulme.SRSPbands AS S ON (1=1)
            WHERE (hulme.isInOpenAboveRangeFloat(S.freqLoKHz, S.freqHiKHz, C.lowerFreqKHz) = 1)
                OR (hulme.isInOpenBelowRangeFloat(S.freqLoKHz, S.freqHiKHz, C.upperFreqKHz) = 1)
            ORDER BY C.lowerFreqKHz
         */





    }
}
