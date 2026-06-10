# Documented File: TpParm.cs
**Repository Path:** `_DataStructures\TpParm.cs`
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
    /// This class has fields that are isomorphic with TSIP user-defined parameter tables
    /// of the type <b>&lt;userID&gt;.tt_&lt;pdfName&gt;_&lt;runName&gt;_parm</b> 
    /// except for the additional fields tempant, tempctx, tempplan, and tempequip.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class TpParm
    {
        // IMPORTANT!
        // =========
        // The following qty. 27 member fields correspond to the fields
        // of the legacy C/C++ struct tpParm_.
        // The order of appearance of these qty. 27 non-static fields MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = PROTYPE_SZ)]
        public string protype;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ENVTYPE_SZ)]
        public string envtype;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = PRONAME_SZ)]
        public string proname;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ENVNAME_SZ)]
        public string envname;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TSORBOUT_SZ)]
        public string tsorbout;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SPHERECALC_SZ)]
        public string spherecalc;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double fsep;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double coordist;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ANALOPT_SZ)]
        public string analopt;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double margin;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short numchan;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CHANCODES_SZ)]
        public string chancodes;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TEMPANT_SZ)]
        public string tempant;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TEMPCTX_SZ)]
        public string tempctx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TEMPPLAN_SZ)]
        public string tempplan;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TEMPEQUIP_SZ)]
        public string tempequip;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = COUNTRY_SZ)]
        public string country;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SELSITES_SZ)]
        public string selsites;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short numcodes;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CODES_SZ)]
        public string codes;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RUNNAME_SZ)]
        public string runname;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int reports;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int numcases;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int numtecases;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = PARMPARM_SZ)]
        public string parmparm;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //---------------------------------------------------------------------------

        public const int NUM_COLUMNS = 27;

        //---------------------------------------------------------------------------

        public const int PROTYPE = 0;
        public const int ENVTYPE = 1;
        public const int PRONAME = 2;
        public const int ENVNAME = 3;
        public const int TSORBOUT = 4;
        public const int SPHERECALC = 5;
        public const int FSEP = 6;
        public const int COORDIST = 7;
        public const int ANALOPT = 8;
        public const int MARGIN = 9;
        public const int NUMCHAN = 10;
        public const int CHANCODES = 11;
        public const int TEMPANT = 12;
        public const int TEMPCTX = 13;
        public const int TEMPPLAN = 14;
        public const int TEMPEQUIP = 15;
        public const int COUNTRY = 16;
        public const int SELSITES = 17;
        public const int NUMCODES = 18;
        public const int CODES = 19;
        public const int RUNNAME = 20;
        public const int REPORTS = 21;
        public const int NUMCASES = 22;
        public const int NUMTECASES = 23;
        public const int PARMPARM = 24;
        public const int MDATE = 25;
        public const int MTIME = 26;

        // TpParm string fields.
        public const int PROTYPE_SZ = 2;
        public const int ENVTYPE_SZ = 9;
        public const int PRONAME_SZ = Constant.TABLE_NM_SZ;
        public const int ENVNAME_SZ = Constant.TABLE_NM_SZ;
        public const int TSORBOUT_SZ = 2;
        public const int SPHERECALC_SZ = 2;
        public const int ANALOPT_SZ = 5;
        public const int CHANCODES_SZ = 20;
        public const int TEMPANT_SZ = 17;
        public const int TEMPCTX_SZ = 17;
        public const int TEMPPLAN_SZ = 17;
        public const int TEMPEQUIP_SZ = 17;
        public const int COUNTRY_SZ = 4;
        public const int SELSITES_SZ = 16;
        public const int CODES_SZ = 165;
        public const int RUNNAME_SZ = 6;
        public const int PARMPARM_SZ = 51;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIMESIZE;

        //---------------------------------------------------------------------------------

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "protype", "envtype", "proname", "envname", "tsorbout", "spherecalc", "fsep", "coordist", "analopt", "margin", "numchan", "chancodes", "tempant", "tempctx", "tempplan", "tempequip", "country", "selsites", "numcodes", "codes", "runname", "reports", "numcases", "numtecases", "parmparm", "mdate", "mtime" };

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

        //---------------------------------------------------------------------------------

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static TpParm()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TpParm()
        {
            Initialize();
        }

        /// <summary>
        /// This method initializes the value of all member fields.
        /// </summary>
        public void Initialize()
        {
            const double INIT_DOUBLE = 0.0;
            const short INIT_SHORT = 0;
            const int INIT_INT = 0;
            const string INIT_STRING = "";

            protype = INIT_STRING;
            envtype = INIT_STRING;
            proname = INIT_STRING;
            envname = INIT_STRING;
            tsorbout = INIT_STRING;
            spherecalc = INIT_STRING;
            fsep = INIT_DOUBLE;
            coordist = INIT_DOUBLE;
            analopt = INIT_STRING;
            margin = INIT_DOUBLE;
            numchan = INIT_SHORT;
            chancodes = INIT_STRING;
            tempant = INIT_STRING;
            tempctx = INIT_STRING;
            tempplan = INIT_STRING;
            tempequip = INIT_STRING;
            country = INIT_STRING;
            selsites = INIT_STRING;
            numcodes = INIT_SHORT;
            codes = INIT_STRING;
            runname = INIT_STRING;
            reports = INIT_INT;
            numcases = INIT_INT;
            numtecases = INIT_INT;
            parmparm = INIT_STRING;
            mdate = INIT_STRING;
            mtime = INIT_STRING;
        }


        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <returns> - string comprising all field names and their values.</returns>
        override
        public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\n\n----- TpParm -----");
            sb.Append("\nprotype = " + protype);
            sb.Append("\nenvtype = " + envtype);
            sb.Append("\nproname = " + proname);
            sb.Append("\nenvname = " + envname);
            sb.Append("\ntsorbout = " + tsorbout);
            sb.Append("\nspherecalc = " + spherecalc);
            sb.Append("\nfsep = " + fsep);
            sb.Append("\ncoordist = " + coordist);
            sb.Append("\nanalopt = " + analopt);
            sb.Append("\nmargin = " + margin);
            sb.Append("\nnumchan = " + numchan);
            sb.Append("\nchancodes = " + chancodes);
            sb.Append("\ntempant = " + tempant);
            sb.Append("\ntempctx = " + tempctx);
            sb.Append("\ntempplan = " + tempplan);
            sb.Append("\ntempequip = " + tempequip);
            sb.Append("\ncountry = " + country);
            sb.Append("\nselsites = " + selsites);
            sb.Append("\nnumcodes = " + numcodes);
            sb.Append("\ncodes = " + codes);
            sb.Append("\nrunname = " + runname);
            sb.Append("\nreports = " + reports);
            sb.Append("\nnumcases = " + numcases);
            sb.Append("\nnumtecases = " + numtecases);
            sb.Append("\nparmparm = " + parmparm);
            //sb.Append("\nmdate = " + mdate);
            //sb.Append("\nmtime = " + mtime);

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
            //nullInds[n++].ToString().PadRight(6) +

            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "protype = " + protype);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "envtype = " + envtype);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "proname = " + proname);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "envname = " + envname);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "tsorbout = " + tsorbout);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "spherecalc = " + spherecalc);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "fsep = " + fsep);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "coordist = " + coordist);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "analopt = " + analopt);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "margin = " + margin);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "numchan = " + numchan);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "chancodes = " + chancodes);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "tempant = " + tempant);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "tempctx = " + tempctx);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "tempplan = " + tempplan);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "tempequip = " + tempequip);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "country = " + country);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "selsites = " + selsites);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "numcodes = " + numcodes);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "codes = " + codes);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "runname = " + runname);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "reports = " + reports);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "numcases = " + numcases);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "numtecases = " + numtecases);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "parmparm = " + parmparm);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "mdate = " + mdate);
            sb.Append("\n" + nullInds[n++].ToString().PadRight(6) + "mtime = " + mtime);

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
        /// For each field of this object, the method allocates a type-specific amount
        /// of global (heap) memory, copies the value of the field into it, and returns
        /// an IntPtr[] containing all of the start addresses. 
        /// </summary>
        /// <returns></returns>
        public SQLPOINTER[] CopyToArrayOfSQLPOINTERs()
        {
            //Can only copy arrays of double into native memory using Marshal method.
            double[] D = new double[1];

            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[0] = Marshal.StringToHGlobalAnsi(protype);        //00

            parameterValuePtr[1] = Marshal.StringToHGlobalAnsi(envtype);        //01

            parameterValuePtr[2] = Marshal.StringToHGlobalAnsi(proname);        //02

            parameterValuePtr[3] = Marshal.StringToHGlobalAnsi(envname);        //03

            parameterValuePtr[4] = Marshal.StringToHGlobalAnsi(tsorbout);       //04

            parameterValuePtr[5] = Marshal.StringToHGlobalAnsi(spherecalc);     //05

            parameterValuePtr[6] = Marshal.AllocHGlobal(sizeof(double));        //06
            D[0] = fsep;
            Marshal.Copy(D, 0, parameterValuePtr[6], 1);

            parameterValuePtr[7] = Marshal.AllocHGlobal(sizeof(double));        //07
            D[0] = coordist;
            Marshal.Copy(D, 0, parameterValuePtr[7], 1);

            parameterValuePtr[8] = Marshal.StringToHGlobalAnsi(analopt);        //08

            parameterValuePtr[9] = Marshal.AllocHGlobal(sizeof(double));        //09
            D[0] = margin;
            Marshal.Copy(D, 0, parameterValuePtr[9], 1);

            parameterValuePtr[10] = Marshal.AllocHGlobal(sizeof(short));        //10
            Marshal.WriteInt16(parameterValuePtr[10], numchan);

            parameterValuePtr[11] = Marshal.StringToHGlobalAnsi(chancodes);     //11

            parameterValuePtr[12] = Marshal.StringToHGlobalAnsi(tempant);       //12

            parameterValuePtr[13] = Marshal.StringToHGlobalAnsi(tempctx);       //13

            parameterValuePtr[14] = Marshal.StringToHGlobalAnsi(tempplan);      //14

            parameterValuePtr[15] = Marshal.StringToHGlobalAnsi(tempequip);     //15

            parameterValuePtr[16] = Marshal.StringToHGlobalAnsi(country);       //16

            parameterValuePtr[17] = Marshal.StringToHGlobalAnsi(selsites);      //17

            parameterValuePtr[18] = Marshal.AllocHGlobal(sizeof(short));        //18
            Marshal.WriteInt16(parameterValuePtr[18], numcodes);

            parameterValuePtr[19] = Marshal.StringToHGlobalAnsi(codes);         //19

            parameterValuePtr[20] = Marshal.StringToHGlobalAnsi(runname);       //20

            parameterValuePtr[21] = Marshal.AllocHGlobal(sizeof(int));          //21
            Marshal.WriteInt32(parameterValuePtr[21], reports);

            parameterValuePtr[22] = Marshal.AllocHGlobal(sizeof(int));          //22
            Marshal.WriteInt32(parameterValuePtr[22], numcases);

            parameterValuePtr[23] = Marshal.AllocHGlobal(sizeof(int));          //23
            Marshal.WriteInt32(parameterValuePtr[23], numtecases);

            parameterValuePtr[24] = Marshal.StringToHGlobalAnsi(parmparm);      //24

            parameterValuePtr[25] = Marshal.StringToHGlobalAnsi(mdate);         //25

            parameterValuePtr[26] = Marshal.StringToHGlobalAnsi(mtime);         //26

            return parameterValuePtr;
        }

        /*
        protype
        envtype
        proname
        envname
        tsorbout
        spherecalc
        fsep
        coordist
        analopt
        margin
        numchan
        chancodes
        tempant
        tempctx
        tempplan
        tempequip
        country
        selsites
        numcodes
        codes
        runname
        reports
        numcases
        numtecases
        parmparm
        mdate
        mtime
        */





    }
}

```
