# Documented File: FeAnte.cs
**Repository Path:** `_DataStructures\FeAnte.cs`
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
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;

    /// <summary>
    /// This class has fields that are isomorphic with PDF table
    /// <b>&lt;userID&gt;.fe_&lt;pdfName&gt;_ante</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FeAnte
    {
        // IMPORTANT!
        // =========
        // The following qty. 35 member values correspond to the columns
        // of an FeAnte table stored in the database; for simplicity, the
        // member and column names are prescribed to be identical.
        // The order of appearance of these qty. 35 'column' members MUST be
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
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TXBAND_SZ)]
        public string txband;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RXBAND_SZ)]
        public string rxband;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ACODETX_SZ)]
        public string acodetx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ACODERX_SZ)]
        public string acoderx;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float g_t;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float lnat;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float aht;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float afslt;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float afslr;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txhgmax;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxhgmax;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int satlongit;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float satlong;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SATLONGS_SZ)]
        public string satlongs;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float az;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float el;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float sarc1;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float sarc2;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxpre;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txpre;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxtro;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txtro;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = LICENCE_SZ)]
        public string licence;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SATNAME_SZ)]
        public string satname;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STATA_SZ)]
        public string stata;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTA_SZ)]
        public string nota;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = OP2_SZ)]
        public string op2;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int antref;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ORBIT_SZ)]
        public string orbit;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //-----------------------------------------------------------------
        public const int NUM_COLUMNS = 35;

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int LOCATION = 2;
        public const int CALL1 = 3;
        public const int TXBAND = 4;
        public const int RXBAND = 5;
        public const int ACODETX = 6;
        public const int ACODERX = 7;
        public const int G_T = 8;
        public const int LNAT = 9;
        public const int AHT = 10;
        public const int AFSLT = 11;
        public const int AFSLR = 12;
        public const int TXHGMAX = 13;
        public const int RXHGMAX = 14;
        public const int SATLONGIT = 15;
        public const int SATLONG = 16;
        public const int SATLONGS = 17;
        public const int AZ = 18;
        public const int EL = 19;
        public const int SARC1 = 20;
        public const int SARC2 = 21;
        public const int RXPRE = 22;
        public const int TXPRE = 23;
        public const int RXTRO = 24;
        public const int TXTRO = 25;
        public const int LICENCE = 26;
        public const int SATNAME = 27;
        public const int STATA = 28;
        public const int NOTA = 29;
        public const int OP2 = 30;
        public const int ANTREF = 31;
        public const int ORBIT = 32;
        public const int MDATE = 33;
        public const int MTIME = 34;

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int LOCATION_SZ = Constant.LOCATION_SZ;
        public const int CALL1_SZ = Constant.CALLSIGN_SZ;
        public const int TXBAND_SZ = Constant.BNDCDE_SZ;
        public const int RXBAND_SZ = Constant.BNDCDE_SZ;
        public const int ACODETX_SZ = Constant.ACODE_SZ;
        public const int ACODERX_SZ = Constant.ACODE_SZ;
        public const int SATLONGS_SZ = Constant.FE_ANTE_SATLONGS_SZ;
        public const int LICENCE_SZ = Constant.LICENCE_SZ;
        public const int SATNAME_SZ = Constant.FE_ANTE_SATNAME_SZ;
        public const int STATA_SZ = Constant.FE_ANTE_STATA_SZ;
        public const int NOTA_SZ = Constant.NOTA_SZ;
        public const int OP2_SZ = Constant.FE_ANTE_OP2_SZ;
        public const int ORBIT_SZ = Constant.FE_ANTE_ORBIT_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //-----------------------------------------------------------------

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "location", "call1", "txband", "rxband", "acodetx", "acoderx", "g_t", "lnat", "aht", "afslt", "afslr", "txhgmax", "rxhgmax", "satlongit", "satlong", "satlongs", "az", "el", "sarc1", "sarc2", "rxpre", "txpre", "rxtro", "txtro", "licence", "satname", "stata", "nota", "op2", "antref", "orbit", "mdate", "mtime" };

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

        //----------------------------------------------------------------------------

        /// <summary>
        /// The static constructor is automatically called once , before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static FeAnte()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();

            //SQLPOINTER[] ParameterValuePtr = Arrays.CreateArrayUsingDefaultElementConstructor<SQLPOINTER>(NUM_COLUMNS);
            //SQLLENPTR[] StrLen_or_IndPtr = Arrays.CreateArrayUsingDefaultElementConstructor<SQLLENPTR>(NUM_COLUMNS);
        }

        /// <summary>
        /// Default object constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public FeAnte()
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
            const int INT_INIT_VAL = 0;
            const float FLOAT_INIT_VAL = 0.0f;

            cmd = STRING_INIT_VAL;
            recstat = STRING_INIT_VAL;
            location = STRING_INIT_VAL;
            call1 = STRING_INIT_VAL;
            txband = STRING_INIT_VAL;
            rxband = STRING_INIT_VAL;
            acodetx = STRING_INIT_VAL;
            acoderx = STRING_INIT_VAL;
            g_t = FLOAT_INIT_VAL;
            lnat = FLOAT_INIT_VAL;
            aht = FLOAT_INIT_VAL;
            afslt = FLOAT_INIT_VAL;
            afslr = FLOAT_INIT_VAL;
            txhgmax = FLOAT_INIT_VAL;
            rxhgmax = FLOAT_INIT_VAL;
            satlongit = INT_INIT_VAL; ;
            satlong = FLOAT_INIT_VAL;
            satlongs = STRING_INIT_VAL;
            az = FLOAT_INIT_VAL;
            el = FLOAT_INIT_VAL;
            sarc1 = FLOAT_INIT_VAL;
            sarc2 = FLOAT_INIT_VAL;
            rxpre = FLOAT_INIT_VAL;
            txpre = FLOAT_INIT_VAL;
            rxtro = FLOAT_INIT_VAL;
            txtro = FLOAT_INIT_VAL;
            licence = STRING_INIT_VAL;
            satname = STRING_INIT_VAL;
            stata = STRING_INIT_VAL;
            nota = STRING_INIT_VAL;
            op2 = STRING_INIT_VAL;
            antref = INT_INIT_VAL; ;
            orbit = STRING_INIT_VAL;
            mdate = STRING_INIT_VAL;
            mtime = STRING_INIT_VAL;
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

            sb.AppendLine("========== FeAnte:");
            sb.Append("\ncmd = " + cmd);
            sb.Append("\nrecstat = " + recstat);
            sb.Append("\nlocation = " + location);
            sb.Append("\ncall1 = " + call1);
            sb.Append("\ntxband = " + txband);
            sb.Append("\nrxband = " + rxband);
            sb.Append("\nacodetx = " + acodetx);
            sb.Append("\nacoderx = " + acoderx);
            sb.Append("\ng_t = " + g_t);
            sb.Append("\nlnat = " + lnat);
            sb.Append("\naht = " + aht);
            sb.Append("\nafslt = " + afslt);
            sb.Append("\nafslr = " + afslr);
            sb.Append("\ntxhgmax = " + txhgmax);
            sb.Append("\nrxhgmax = " + rxhgmax);
            sb.Append("\nsatlongit = " + satlongit); ;
            sb.Append("\nsatlong = " + satlong);
            sb.Append("\nsatlongs = " + satlongs);
            sb.Append("\naz = " + az);
            sb.Append("\nel = " + el);
            sb.Append("\nsarc1 = " + sarc1);
            sb.Append("\nsarc2 = " + sarc2);
            sb.Append("\nrxpre = " + rxpre);
            sb.Append("\ntxpre = " + txpre);
            sb.Append("\nrxtro = " + rxtro);
            sb.Append("\ntxtro = " + txtro);
            sb.Append("\nlicence = " + licence);
            sb.Append("\nsatname = " + satname);
            sb.Append("\nstata = " + stata);
            sb.Append("\nnota = " + nota);
            sb.Append("\nop2 = " + op2);
            sb.Append("\nantref = " + antref); ;
            sb.Append("\norbit = " + orbit);
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

            sb.AppendLine("========== FeAnte:");
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "cmd = " + cmd);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "recstat = " + recstat);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "location = " + location);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "call1 = " + call1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txband = " + txband);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxband = " + rxband);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "acodetx = " + acodetx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "acoderx = " + acoderx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "g_t = " + g_t);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "lnat = " + lnat);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "aht = " + aht);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "afslt = " + afslt);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "afslr = " + afslr);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txhgmax = " + txhgmax);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxhgmax = " + rxhgmax);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "satlongit = " + satlongit); ;
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "satlong = " + satlong);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "satlongs = " + satlongs);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "az = " + az);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "el = " + el);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "sarc1 = " + sarc1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "sarc2 = " + sarc2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxpre = " + rxpre);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txpre = " + txpre);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxtro = " + rxtro);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txtro = " + txtro);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "licence = " + licence);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "satname = " + satname);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "stata = " + stata);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "nota = " + nota);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "op2 = " + op2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "antref = " + antref); ;
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "orbit = " + orbit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate = " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime = " + mtime);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string that concatenates the 'key'
        /// fields location and call1.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string KeysToString()
        {
            return String.Format("location: {0} ;  call1: {1}", location, call1);
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


            parameterValuePtr[CMD] = Marshal.StringToHGlobalAnsi(cmd);

            parameterValuePtr[RECSTAT] = Marshal.StringToHGlobalAnsi(recstat);

            parameterValuePtr[LOCATION] = Marshal.StringToHGlobalAnsi(location);

            parameterValuePtr[CALL1] = Marshal.StringToHGlobalAnsi(call1);

            parameterValuePtr[TXBAND] = Marshal.StringToHGlobalAnsi(txband);

            parameterValuePtr[RXBAND] = Marshal.StringToHGlobalAnsi(rxband);

            parameterValuePtr[ACODETX] = Marshal.StringToHGlobalAnsi(acodetx);

            parameterValuePtr[ACODERX] = Marshal.StringToHGlobalAnsi(acoderx);

            parameterValuePtr[G_T] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = g_t;
            Marshal.Copy(F, 0, parameterValuePtr[G_T], 1);

            parameterValuePtr[LNAT] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = lnat;
            Marshal.Copy(F, 0, parameterValuePtr[LNAT], 1);

            parameterValuePtr[AHT] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = aht;
            Marshal.Copy(F, 0, parameterValuePtr[AHT], 1);

            parameterValuePtr[AFSLT] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = afslt;
            Marshal.Copy(F, 0, parameterValuePtr[AFSLT], 1);

            parameterValuePtr[AFSLR] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = afslr;
            Marshal.Copy(F, 0, parameterValuePtr[AFSLR], 1);

            parameterValuePtr[TXHGMAX] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = txhgmax;
            Marshal.Copy(F, 0, parameterValuePtr[TXHGMAX], 1);

            parameterValuePtr[RXHGMAX] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = rxhgmax;
            Marshal.Copy(F, 0, parameterValuePtr[RXHGMAX], 1);

            parameterValuePtr[SATLONGIT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[SATLONGIT], satlongit);

            parameterValuePtr[SATLONG] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = satlong;
            Marshal.Copy(F, 0, parameterValuePtr[SATLONG], 1);

            parameterValuePtr[SATLONGS] = Marshal.StringToHGlobalAnsi(satlongs);

            parameterValuePtr[AZ] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = az;
            Marshal.Copy(F, 0, parameterValuePtr[AZ], 1);

            parameterValuePtr[EL] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = el;
            Marshal.Copy(F, 0, parameterValuePtr[EL], 1);

            parameterValuePtr[SARC1] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = sarc1;
            Marshal.Copy(F, 0, parameterValuePtr[SARC1], 1);

            parameterValuePtr[SARC2] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = sarc2;
            Marshal.Copy(F, 0, parameterValuePtr[SARC2], 1);

            parameterValuePtr[RXPRE] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = rxpre;
            Marshal.Copy(F, 0, parameterValuePtr[RXPRE], 1);

            parameterValuePtr[TXPRE] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = txpre;
            Marshal.Copy(F, 0, parameterValuePtr[TXPRE], 1);

            parameterValuePtr[RXTRO] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = rxtro;
            Marshal.Copy(F, 0, parameterValuePtr[RXTRO], 1);

            parameterValuePtr[TXTRO] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = txtro;
            Marshal.Copy(F, 0, parameterValuePtr[TXTRO], 1);

            parameterValuePtr[LICENCE] = Marshal.StringToHGlobalAnsi(licence);

            parameterValuePtr[SATNAME] = Marshal.StringToHGlobalAnsi(satname);

            parameterValuePtr[STATA] = Marshal.StringToHGlobalAnsi(stata);

            parameterValuePtr[NOTA] = Marshal.StringToHGlobalAnsi(nota);

            parameterValuePtr[OP2] = Marshal.StringToHGlobalAnsi(op2);

            parameterValuePtr[ANTREF] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[ANTREF], antref);

            parameterValuePtr[ORBIT] = Marshal.StringToHGlobalAnsi(orbit);

            parameterValuePtr[MDATE] = Marshal.StringToHGlobalAnsi(mdate);

            parameterValuePtr[MTIME] = Marshal.StringToHGlobalAnsi(mtime);

            return parameterValuePtr;
        }

        /*
        cmd
        recstat
        location
        call1
        txband
        rxband
        acodetx
        acoderx
        g_t
        lnat
        aht
        afslt
        afslr
        txhgmax
        rxhgmax
        satlongit
        satlong
        satlongs
        az
        el
        sarc1
        sarc2
        rxpre
        txpre
        rxtro
        txtro
        licence
        satname
        stata
        nota
        op2
        antref
        orbit
        mdate
        mtime
        */


    }
}

```
