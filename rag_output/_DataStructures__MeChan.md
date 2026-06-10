# Documented File: MeChan.cs
**Repository Path:** `_DataStructures\MeChan.cs`
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
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;

    /// <summary>
    /// This class has fields that are isomorphic with the DB table <b>main.me_chan</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class MeChan
    {
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

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = USERID_SZ)]
        public string userid;

        //-------------------------------------------------------------------------

        public const int NUM_COLUMNS = 28;

        public const int LOCATION_SZ = Constant.LOCATION_SZ;
        public const int CALL1_SZ = Constant.CALLSIGN_SZ;
        public const int CHID_SZ = 5;
        public const int POLTX_SZ = 2;
        public const int EQPTTX_SZ = 9;
        public const int TRAFTX_SZ = 7;
        public const int STATTX_SZ = 2;
        public const int FEETX_SZ = 3;
        public const int POLRX_SZ = 2;
        public const int EQPTRX_SZ = 9;
        public const int TRAFRX_SZ = 7;
        public const int STATRX_SZ = 2;
        public const int FEERX_SZ = 3;
        public const int NOTC_SZ = 5;
        public const int SRVCTX_SZ = 7;
        public const int SRVCRX_SZ = 7;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;
        public const int USERID_SZ = 13;

        public const int LOCATION = 0;
        public const int CALL1 = 1;
        public const int CHID = 2;
        public const int FREQTX = 3;
        public const int POLTX = 4;
        public const int MAXTXPOWER = 5;
        public const int PWRTX = 6;
        public const int P4KHZ = 7;
        public const int EQPTTX = 8;
        public const int TRAFTX = 9;
        public const int STATTX = 10;
        public const int FEETX = 11;
        public const int FREQRX = 12;
        public const int POLRX = 13;
        public const int PWRRX = 14;
        public const int EQPTRX = 15;
        public const int TRAFRX = 16;
        public const int STATRX = 17;
        public const int I20 = 18;
        public const int IT01 = 19;
        public const int IP01 = 20;
        public const int FEERX = 21;
        public const int NOTC = 22;
        public const int SRVCTX = 23;
        public const int SRVCRX = 24;
        public const int MDATE = 25;
        public const int MTIME = 26;
        public const int USERID = 27;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "location", "call1", "chid", "freqtx", "poltx", "maxtxpower", "pwrtx", "p4khz", "eqpttx", "traftx", "stattx", "feetx", "freqrx", "polrx", "pwrrx", "eqptrx", "trafrx", "statrx", "i20", "it01", "ip01", "feerx", "notc", "srvctx", "srvcrx", "mdate", "mtime", "userid" };

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

        // Using C# reflection.
        /* USAGE
        
            Foo f = new Foo();
            // Set
            f["Bar"] = "asdf";
            // Get
            string s = (string)f["Bar"];
        */
        public object this[string propertyName]
        {
            get { return this.GetType().GetProperty(propertyName).GetValue(this, null); }
            set { this.GetType().GetProperty(propertyName).SetValue(this, value, null); }
        }

        /// <summary>
        /// The static constructor is automatically called once , before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static MeChan()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// This method returns a string that concatenates the 'key'
        /// fields {location, call1, chid}.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string KeysToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("location = " + location);
            sb.Append("   call1 = " + call1);
            sb.Append("    chid = " + chid);
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

            sb.AppendLine("========== MeChan:");

            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "location =   " + location);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "call1 =      " + call1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "chid =       " + chid);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "freqtx =     " + freqtx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "poltx =      " + poltx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "maxtxpower = " + maxtxpower);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "pwrtx =      " + pwrtx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "p4khz =      " + p4khz);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "eqpttx =     " + eqpttx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "traftx =     " + traftx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "stattx =     " + stattx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "feetx =      " + feetx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "freqrx =     " + freqrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "polrx =      " + polrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "pwrrx =      " + pwrrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "eqptrx =     " + eqptrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "trafrx =     " + trafrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "statrx =     " + statrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "i20 =        " + i20);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "it01 =       " + it01);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ip01 =       " + ip01);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "feerx =      " + feerx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "notc =       " + notc);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "srvctx =     " + srvctx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "srvcrx =     " + srvcrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate =      " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime =      " + mtime);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "userid =     " + userid);

            return sb.ToString();
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

            sb.Append(nullInds[MeChan.LOCATION] == Constant.DB_NULL ? "NULL," : "'" + location.ToString() + "',");
            sb.Append(nullInds[MeChan.CALL1] == Constant.DB_NULL ? "NULL," : "'" + call1.ToString() + "',");
            sb.Append(nullInds[MeChan.CHID] == Constant.DB_NULL ? "NULL," : "'" + chid.ToString() + "',");
            sb.Append(nullInds[MeChan.FREQTX] == Constant.DB_NULL ? "NULL," : "'" + freqtx.ToString() + "',");
            sb.Append(nullInds[MeChan.POLTX] == Constant.DB_NULL ? "NULL," : "'" + poltx.ToString() + "',");
            sb.Append(nullInds[MeChan.MAXTXPOWER] == Constant.DB_NULL ? "NULL," : "'" + maxtxpower.ToString() + "',");
            sb.Append(nullInds[MeChan.PWRTX] == Constant.DB_NULL ? "NULL," : "'" + pwrtx.ToString() + "',");
            sb.Append(nullInds[MeChan.P4KHZ] == Constant.DB_NULL ? "NULL," : "'" + p4khz.ToString() + "',");
            sb.Append(nullInds[MeChan.EQPTTX] == Constant.DB_NULL ? "NULL," : "'" + eqpttx.ToString() + "',");
            sb.Append(nullInds[MeChan.TRAFTX] == Constant.DB_NULL ? "NULL," : "'" + traftx.ToString() + "',");
            sb.Append(nullInds[MeChan.STATTX] == Constant.DB_NULL ? "NULL," : "'" + stattx.ToString() + "',");
            sb.Append(nullInds[MeChan.FEETX] == Constant.DB_NULL ? "NULL," : "'" + feetx.ToString() + "',");
            sb.Append(nullInds[MeChan.FREQRX] == Constant.DB_NULL ? "NULL," : "'" + freqrx.ToString() + "',");
            sb.Append(nullInds[MeChan.POLRX] == Constant.DB_NULL ? "NULL," : "'" + polrx.ToString() + "',");
            sb.Append(nullInds[MeChan.PWRRX] == Constant.DB_NULL ? "NULL," : "'" + pwrrx.ToString() + "',");
            sb.Append(nullInds[MeChan.EQPTRX] == Constant.DB_NULL ? "NULL," : "'" + eqptrx.ToString() + "',");
            sb.Append(nullInds[MeChan.TRAFRX] == Constant.DB_NULL ? "NULL," : "'" + trafrx.ToString() + "',");
            sb.Append(nullInds[MeChan.STATRX] == Constant.DB_NULL ? "NULL," : "'" + statrx.ToString() + "',");
            sb.Append(nullInds[MeChan.I20] == Constant.DB_NULL ? "NULL," : "'" + i20.ToString() + "',");
            sb.Append(nullInds[MeChan.IT01] == Constant.DB_NULL ? "NULL," : "'" + it01.ToString() + "',");
            sb.Append(nullInds[MeChan.IP01] == Constant.DB_NULL ? "NULL," : "'" + ip01.ToString() + "',");
            sb.Append(nullInds[MeChan.FEERX] == Constant.DB_NULL ? "NULL," : "'" + feerx.ToString() + "',");
            sb.Append(nullInds[MeChan.NOTC] == Constant.DB_NULL ? "NULL," : "'" + notc.ToString() + "',");
            sb.Append(nullInds[MeChan.SRVCTX] == Constant.DB_NULL ? "NULL," : "'" + srvctx.ToString() + "',");
            sb.Append(nullInds[MeChan.SRVCRX] == Constant.DB_NULL ? "NULL," : "'" + srvcrx.ToString() + "',");
            sb.Append(nullInds[MeChan.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',");
            sb.Append(nullInds[MeChan.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "',");
            sb.Append(nullInds[MeChan.USERID] == Constant.DB_NULL ? "NULL" : "'" + userid.ToString() + "'");
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

            sb.Append(columnNames[MeChan.LOCATION] + "=" + (nullInds[MeChan.LOCATION] == Constant.DB_NULL ? "NULL," : "'" + location.ToString() + "',"));
            sb.Append(columnNames[MeChan.CALL1] + "=" + (nullInds[MeChan.CALL1] == Constant.DB_NULL ? "NULL," : "'" + call1.ToString() + "',"));
            sb.Append(columnNames[MeChan.CHID] + "=" + (nullInds[MeChan.CHID] == Constant.DB_NULL ? "NULL," : "'" + chid.ToString() + "',"));
            sb.Append(columnNames[MeChan.FREQTX] + "=" + (nullInds[MeChan.FREQTX] == Constant.DB_NULL ? "NULL," : "'" + freqtx.ToString() + "',"));
            sb.Append(columnNames[MeChan.POLTX] + "=" + (nullInds[MeChan.POLTX] == Constant.DB_NULL ? "NULL," : "'" + poltx.ToString() + "',"));
            sb.Append(columnNames[MeChan.MAXTXPOWER] + "=" + (nullInds[MeChan.MAXTXPOWER] == Constant.DB_NULL ? "NULL," : "'" + maxtxpower.ToString() + "',"));
            sb.Append(columnNames[MeChan.PWRTX] + "=" + (nullInds[MeChan.PWRTX] == Constant.DB_NULL ? "NULL," : "'" + pwrtx.ToString() + "',"));
            sb.Append(columnNames[MeChan.P4KHZ] + "=" + (nullInds[MeChan.P4KHZ] == Constant.DB_NULL ? "NULL," : "'" + p4khz.ToString() + "',"));
            sb.Append(columnNames[MeChan.EQPTTX] + "=" + (nullInds[MeChan.EQPTTX] == Constant.DB_NULL ? "NULL," : "'" + eqpttx.ToString() + "',"));
            sb.Append(columnNames[MeChan.TRAFTX] + "=" + (nullInds[MeChan.TRAFTX] == Constant.DB_NULL ? "NULL," : "'" + traftx.ToString() + "',"));
            sb.Append(columnNames[MeChan.STATTX] + "=" + (nullInds[MeChan.STATTX] == Constant.DB_NULL ? "NULL," : "'" + stattx.ToString() + "',"));
            sb.Append(columnNames[MeChan.FEETX] + "=" + (nullInds[MeChan.FEETX] == Constant.DB_NULL ? "NULL," : "'" + feetx.ToString() + "',"));
            sb.Append(columnNames[MeChan.FREQRX] + "=" + (nullInds[MeChan.FREQRX] == Constant.DB_NULL ? "NULL," : "'" + freqrx.ToString() + "',"));
            sb.Append(columnNames[MeChan.POLRX] + "=" + (nullInds[MeChan.POLRX] == Constant.DB_NULL ? "NULL," : "'" + polrx.ToString() + "',"));
            sb.Append(columnNames[MeChan.PWRRX] + "=" + (nullInds[MeChan.PWRRX] == Constant.DB_NULL ? "NULL," : "'" + pwrrx.ToString() + "',"));
            sb.Append(columnNames[MeChan.EQPTRX] + "=" + (nullInds[MeChan.EQPTRX] == Constant.DB_NULL ? "NULL," : "'" + eqptrx.ToString() + "',"));
            sb.Append(columnNames[MeChan.TRAFRX] + "=" + (nullInds[MeChan.TRAFRX] == Constant.DB_NULL ? "NULL," : "'" + trafrx.ToString() + "',"));
            sb.Append(columnNames[MeChan.STATRX] + "=" + (nullInds[MeChan.STATRX] == Constant.DB_NULL ? "NULL," : "'" + statrx.ToString() + "',"));
            sb.Append(columnNames[MeChan.I20] + "=" + (nullInds[MeChan.I20] == Constant.DB_NULL ? "NULL," : "'" + i20.ToString() + "',"));
            sb.Append(columnNames[MeChan.IT01] + "=" + (nullInds[MeChan.IT01] == Constant.DB_NULL ? "NULL," : "'" + it01.ToString() + "',"));
            sb.Append(columnNames[MeChan.IP01] + "=" + (nullInds[MeChan.IP01] == Constant.DB_NULL ? "NULL," : "'" + ip01.ToString() + "',"));
            sb.Append(columnNames[MeChan.FEERX] + "=" + (nullInds[MeChan.FEERX] == Constant.DB_NULL ? "NULL," : "'" + feerx.ToString() + "',"));
            sb.Append(columnNames[MeChan.NOTC] + "=" + (nullInds[MeChan.NOTC] == Constant.DB_NULL ? "NULL," : "'" + notc.ToString() + "',"));
            sb.Append(columnNames[MeChan.SRVCTX] + "=" + (nullInds[MeChan.SRVCTX] == Constant.DB_NULL ? "NULL," : "'" + srvctx.ToString() + "',"));
            sb.Append(columnNames[MeChan.SRVCRX] + "=" + (nullInds[MeChan.SRVCRX] == Constant.DB_NULL ? "NULL," : "'" + srvcrx.ToString() + "',"));
            sb.Append(columnNames[MeChan.MDATE] + "=" + (nullInds[MeChan.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',"));
            sb.Append(columnNames[MeChan.MTIME] + "=" + (nullInds[MeChan.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "',"));
            sb.Append(columnNames[MeChan.USERID] + "=" + (nullInds[MeChan.USERID] == Constant.DB_NULL ? "NULL" : "'" + userid.ToString() + "'"));

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

            parameterValuePtr[USERID] = Marshal.StringToHGlobalAnsi(userid);

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
            sb.Append("INSERT INTO ");
            sb.Append(tableName);
            //sb.Append(" (");
            //sb.Append(AllColumnsForSqlSelect);
            //sb.Append(") ");
            sb.Append(" VALUES ");
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

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[me_chan] " +
"(" +
    "[location] [char](10) NOT NULL," +
    "[call1] [char](9) NOT NULL," +
    "[chid] [char](4) NOT NULL," +
    "[freqtx] [float] NULL," +
    "[poltx] [char](1) NULL," +
    "[maxtxpower] [real] NULL," +
    "[pwrtx] [real] NULL," +
    "[p4khz] [real] NULL," +
    "[eqpttx] [char](8) NULL," +
    "[traftx] [char](6) NULL," +
    "[stattx] [char](1) NULL," +
    "[feetx] [char](2) NULL," +
    "[freqrx] [float] NULL," +
    "[polrx] [char](1) NULL," +
    "[pwrrx] [real] NULL," +
    "[eqptrx] [char](8) NULL," +
    "[trafrx] [char](6) NULL," +
    "[statrx] [char](1) NULL," +
    "[i20] [real] NULL," +
    "[it01] [real] NULL," +
    "[ip01] [real] NULL," +
    "[feerx] [char](2) NULL," +
    "[notc] [char](4) NULL," +
    "[srvctx] [char](6) NULL," +
    "[srvcrx] [char](6) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
    "[userid] [char](12) NULL," +
    "CONSTRAINT [PK_me_chan] PRIMARY KEY CLUSTERED " +
    "(" +
        "[location] ASC," +
        "[call1] ASC," +
        "[chid] ASC" +
    ") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";

        /*
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
        userid
        */




    }
}

```
