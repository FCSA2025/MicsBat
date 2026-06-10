# Documented File: SuAnte.cs
**Repository Path:** `_DataStructures\SuAnte.cs`
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
    /// This class has fields that are isomorphic with the 'subupt' DB table 
    /// <b>main.sd_ante</b> except for the additional fields 'cmd' and 'recstat'.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SuAnte
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CMD_SZ)]
        public string cmd;          //00
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RECSTAT_SZ)]
        public string recstat;      //01
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ACODE_SZ)]
        public string acode;        //02
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int axtype;          //03
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = AXREF_SZ)]
        public string axref;        //04
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float again;         //05
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float abw;           //06
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short arms;          //07
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ABAND_SZ)]
        public string aband;        //08
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = AMANU_SZ)]
        public string amanu;        //09
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = APATTERN_SZ)]
        public string apattern;     //10
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = AMODEL_SZ)]
        public string amodel;       //11
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short anip;          //12
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float ax0;           //13
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ADESC_SZ)]
        public string adesc;        //14
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ANTYPE_SZ)]
        public string antype;       //15
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float aftbr;         //16
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double lofreq;       //17
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double hifreq;       //18
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = BANDCODES_SZ)]
        public string bandcodes;    //19
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;        //20
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;        //21

        //--------------------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 22;

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int ACODE = 2;
        public const int AXTYPE = 3;
        public const int AXREF = 4;
        public const int AGAIN = 5;
        public const int ABW = 6;
        public const int ARMS = 7;
        public const int ABAND = 8;
        public const int AMANU = 9;
        public const int APATTERN = 10;
        public const int AMODEL = 11;
        public const int ANIP = 12;
        public const int AX0 = 13;
        public const int ADESC = 14;
        public const int ANTYPE = 15;
        public const int AFTBR = 16;
        public const int LOFREQ = 17;
        public const int HIFREQ = 18;
        public const int BANDCODES = 19;
        public const int MDATE = 20;
        public const int MTIME = 21;

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int ACODE_SZ = Constant.ACODE_SZ;
        public const int AXREF_SZ = Constant.ACODE_SZ;
        public const int ABAND_SZ = Constant.ABAND_SZ;
        public const int AMANU_SZ = Constant.AMANU_SZ;
        public const int APATTERN_SZ = Constant.ACODE_SZ;
        public const int AMODEL_SZ = Constant.ANTE_MODEL_SZ;
        public const int ADESC_SZ = Constant.ADESC_SZ;
        public const int ANTYPE_SZ = Constant.ANTYPE_SZ;
        public const int BANDCODES_SZ = Constant.BANDCODES_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "acode", "axtype", "axref", "again", "abw", "arms", "aband", "amanu", "apattern", "amodel", "anip", "ax0", "adesc", "antype", "aftbr", "lofreq", "hifreq", "bandcodes", "mdate", "mtime" };

        //Provide read-only access to these members.
        private static string mAllFieldsAsCSV;
        private static string mSubsetOfFieldsAsCSV;
        //List of all column names with bindings for SQL 'update' command, i.e. " cmd=?, recstat=?, call1=?, call2=?, bndcde=?, anum=?, ... , mtime=? "
        private static string mAllBindingsAsCSV;

        public static string AllColumnsForSqlSelect
        {
            get { return mAllFieldsAsCSV; }
        }

        public static string SubsetOfColumnsForSqlSelect
        {
            get { return mSubsetOfFieldsAsCSV; }
        }

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
        static SuAnte()
        {
            mAllFieldsAsCSV = ListOfColumnNamesForSQLSelect();
            mSubsetOfFieldsAsCSV = SubListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        public SuAnte()
        {
            cmd = "";
            recstat = "";
            acode = "";
            axref = "";
            aband = "";
            amanu = "";
            apattern = "";
            amodel = "";
            adesc = "";
            antype = "";
            bandcodes = "";
            mdate = "";
            mtime = "";
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
            sb.Append("\r\n");
            sb.AppendLine("cmd =      " + cmd);
            sb.AppendLine("recstat =  " + recstat);
            sb.AppendLine("acode =    " + acode);
            sb.AppendLine("axtype =    " + axtype);
            sb.AppendLine("axref =   " + axref);
            sb.AppendLine("again =     " + again);
            sb.AppendLine("abw =     " + abw);
            sb.AppendLine("arms =    " + arms);
            sb.AppendLine("aband =      " + aband);
            sb.AppendLine("amanu =    " + amanu);
            sb.AppendLine("apattern =    " + apattern);
            sb.AppendLine("amodel =     " + amodel);
            sb.AppendLine("anip =   " + anip);
            sb.AppendLine("ax0 =   " + ax0);
            sb.AppendLine("adesc =   " + adesc);
            sb.AppendLine("antype =    " + antype);
            sb.AppendLine("aftbr = " + aftbr);
            sb.AppendLine("lofreq = " + lofreq);
            sb.AppendLine("hifreq = " + hifreq);
            sb.AppendLine("bandcodes = " + bandcodes);
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
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\r\n");

            int n = 0;
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "cmd =       " + cmd);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "recstat =   " + recstat);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "acode =     " + acode);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "axtype =    " + axtype);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "axref =     " + axref);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "again =     " + again);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "abw =       " + abw);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "arms =      " + arms);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "aband =     " + aband);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "amanu =     " + amanu);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "apattern =  " + apattern);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "amodel =    " + amodel);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "anip =      " + anip);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ax0 =       " + ax0);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "adesc =     " + adesc);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "antype =    " + antype);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "aftbr =     " + aftbr);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "lofreq =    " + lofreq);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "hifreq =    " + hifreq);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "bandcodes = " + bandcodes);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate =     " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime =     " + mtime);

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
        /// of all member field values except 'cmd' and 'recstat' for use in 
        /// a SQL 'select' query.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private static string SubListOfColumnNamesForSQLSelect()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" ");
            for (int i = 2; i < NUM_COLUMNS; i++)
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
        /// of all member field values except 'cmd' and 'recstat' for use in 
        /// a SQL 'select' query.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private static string ListOfColumnNamesForSQLSelect()
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
		/// This method returns a 'deep copy' of the current object; changes to the 
        /// field values of this object do not cause changes to the deep copy.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public SuAnte DeepCopy()
        {
            // A deep copy object has deep copies of the nonstatic fields of the current object.
            //    - If a field is a value type then a bit-by-bit copy of the field is created. 
            //    - If a field is a reference type then a deep copy of the referred object is created.

            SuAnte suAnte = new SuAnte();

            suAnte.cmd = this.cmd;
            suAnte.recstat = this.recstat;
            suAnte.acode = this.acode;
            suAnte.axtype = this.axtype;
            suAnte.axref = this.axref;
            suAnte.again = this.again;
            suAnte.abw = this.abw;
            suAnte.arms = this.arms;
            suAnte.aband = this.aband;
            suAnte.amanu = this.amanu;
            suAnte.apattern = this.apattern;
            suAnte.amodel = this.amodel;
            suAnte.anip = this.anip;
            suAnte.ax0 = this.ax0;
            suAnte.adesc = this.adesc;
            suAnte.antype = this.antype;
            suAnte.aftbr = this.aftbr;
            suAnte.lofreq = this.lofreq;
            suAnte.hifreq = this.hifreq;
            suAnte.bandcodes = this.bandcodes;
            suAnte.mdate = this.mdate;
            suAnte.mtime = this.mtime;

            return suAnte;
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
            //Can only copy arrays of float and double into native memory using Marshal method.
            float[] F = new float[1];
            double[] D = new double[1];

            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[CMD] = Marshal.StringToHGlobalAnsi(cmd);

            parameterValuePtr[RECSTAT] = Marshal.StringToHGlobalAnsi(recstat);

            parameterValuePtr[ACODE] = Marshal.StringToHGlobalAnsi(acode);

            parameterValuePtr[AXTYPE] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[AXTYPE], axtype);

            parameterValuePtr[AXREF] = Marshal.StringToHGlobalAnsi(axref);

            parameterValuePtr[AGAIN] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = again;
            Marshal.Copy(F, 0, parameterValuePtr[AGAIN], 1);

            parameterValuePtr[ABW] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = abw;
            Marshal.Copy(F, 0, parameterValuePtr[ABW], 1);

            parameterValuePtr[ARMS] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[ARMS], arms);

            parameterValuePtr[ABAND] = Marshal.StringToHGlobalAnsi(aband);

            parameterValuePtr[AMANU] = Marshal.StringToHGlobalAnsi(amanu);

            parameterValuePtr[APATTERN] = Marshal.StringToHGlobalAnsi(apattern);

            parameterValuePtr[AMODEL] = Marshal.StringToHGlobalAnsi(amodel);

            parameterValuePtr[ANIP] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[ANIP], anip);

            parameterValuePtr[AX0] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = ax0;
            Marshal.Copy(F, 0, parameterValuePtr[AX0], 1);

            parameterValuePtr[ADESC] = Marshal.StringToHGlobalAnsi(adesc);

            parameterValuePtr[ANTYPE] = Marshal.StringToHGlobalAnsi(antype);

            parameterValuePtr[AFTBR] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = aftbr;
            Marshal.Copy(F, 0, parameterValuePtr[AFTBR], 1);

            parameterValuePtr[LOFREQ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = lofreq;
            Marshal.Copy(D, 0, parameterValuePtr[LOFREQ], 1);

            parameterValuePtr[HIFREQ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = hifreq;
            Marshal.Copy(D, 0, parameterValuePtr[HIFREQ], 1);

            parameterValuePtr[BANDCODES] = Marshal.StringToHGlobalAnsi(bandcodes);

            parameterValuePtr[MDATE] = Marshal.StringToHGlobalAnsi(mdate);

            parameterValuePtr[MTIME] = Marshal.StringToHGlobalAnsi(mtime);

            return parameterValuePtr;
        }







    }
}

```
