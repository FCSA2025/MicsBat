# Documented File: SuAntd.cs
**Repository Path:** `_DataStructures\SuAntd.cs`
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
    /// This class has fields that are isomorphic with the DB table <b>main.sd_antd</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SuAntd
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CMD_SZ)]
        public string cmd;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ACODE_SZ)]
        public string acode;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float antang;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float dcov;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float dxpv;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float dcoh;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float dxph;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float dtilt;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int interpstat;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //----------------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 11;

        public const int CMD = 0;
        public const int ACODE = 1;
        public const int ANTANG = 2;
        public const int DCOV = 3;
        public const int DXPV = 4;
        public const int DCOH = 5;
        public const int DXPH = 6;
        public const int DTILT = 7;
        public const int INTERPSTAT = 8;
        public const int MDATE = 9;
        public const int MTIME = 10;

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int ACODE_SZ = Constant.ACODE_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "acode", "antang", "dcov", "dxpv", "dcoh", "dxph", "dtilt", "interpstat", "mdate", "mtime" };

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
        static SuAntd()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mSubsetOfFieldsAsCSV = SubListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Initialize the string members as empty strings.
        /// </summary>
        public SuAntd()
        {
            Initialize();
        }

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public void Initialize()
        {
            cmd = "";
            acode = "";
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
            sb.AppendLine("\r\n");
            sb.AppendLine("cmd =        " + cmd);
            sb.AppendLine("acode =      " + acode);
            sb.AppendLine("antang =     " + antang);
            sb.AppendLine("dcov =       " + dcov);
            sb.AppendLine("dxpv =       " + dxpv);
            sb.AppendLine("dcoh =       " + dcoh);
            sb.AppendLine("dxph =       " + dxph);
            sb.AppendLine("dtilt =      " + dtilt);
            sb.AppendLine("interpstat = " + interpstat);
            sb.AppendLine("mdate =      " + mdate);
            sb.AppendLine("mtime =      " + mtime);

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
            int n = 0;

            sb.AppendLine("\r\n");
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "cmd =        " + cmd);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "acode =      " + acode);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "antang =     " + antang);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "dcov =       " + dcov);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "dxpv =       " + dxpv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "dcoh =       " + dcoh);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "dxph =       " + dxph);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "dtilt =      " + dtilt);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "interpstat = " + interpstat);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate =      " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime =      " + mtime);

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
        /// of all column names for use in a SQL 'select' query.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private static string SubListOfColumnNamesForSQLSelect()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" ");
            for (int i = 1; i < NUM_COLUMNS; i++)
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
        public SuAntd DeepCopy()
        {
            // A deep copy object has deep copies of the nonstatic fields of the current object.
            //    - If a field is a value type then a bit-by-bit copy of the field is created. 
            //    - If a field is a reference type then a deep copy of the referred object is created.

            SuAntd suAntd = new SuAntd();

            suAntd.cmd = this.cmd;
            suAntd.acode = this.acode;
            suAntd.antang = this.antang;
            suAntd.dcov = this.dcov;
            suAntd.dxpv = this.dxpv;
            suAntd.dcoh = this.dcoh;
            suAntd.dxph = this.dxph;
            suAntd.dtilt = this.dtilt;
            suAntd.interpstat = this.interpstat;
            suAntd.mdate = this.mdate;
            suAntd.mtime = this.mtime;

            return suAntd;
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

            parameterValuePtr[ACODE] = Marshal.StringToHGlobalAnsi(acode);

            parameterValuePtr[ANTANG] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = antang;
            Marshal.Copy(F, 0, parameterValuePtr[ANTANG], 1);

            parameterValuePtr[DCOV] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = dcov;
            Marshal.Copy(F, 0, parameterValuePtr[DCOV], 1);

            parameterValuePtr[DXPV] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = dxpv;
            Marshal.Copy(F, 0, parameterValuePtr[DXPV], 1);

            parameterValuePtr[DCOH] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = dcoh;
            Marshal.Copy(F, 0, parameterValuePtr[DCOH], 1);

            parameterValuePtr[DXPH] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = dxph;
            Marshal.Copy(F, 0, parameterValuePtr[DXPH], 1);

            parameterValuePtr[DTILT] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = dtilt;
            Marshal.Copy(F, 0, parameterValuePtr[DTILT], 1);

            parameterValuePtr[INTERPSTAT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[INTERPSTAT], interpstat);

            parameterValuePtr[MDATE] = Marshal.StringToHGlobalAnsi(mdate);

            parameterValuePtr[MTIME] = Marshal.StringToHGlobalAnsi(mtime);

            return parameterValuePtr;
        }




        /*
        cmd
        acode
        antang
        dcov
        dxpv
        dcoh
        dxph
        dtilt
        interpstat
        mdate
        mtime
        */




    }
}

```
