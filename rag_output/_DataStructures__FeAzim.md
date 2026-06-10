# Documented File: FeAzim.cs
**Repository Path:** `_DataStructures\FeAzim.cs`
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
    /// This class has fields that are isomorphic with PDF table <b>&lt;userID&gt;.fe_&lt;pdfName&gt;_azim</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FeAzim
    {
        // IMPORTANT!
        // =========
        // The following qty. 11 member values correspond to the columns
        // of an FeAzim table stored in the database; for simplicity, the
        // member and column names are prescribed to be identical.
        // The order of appearance of these qty. 11 'column' members MUST be
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
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = DELETEALL_SZ)]
        public string deleteall;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = LOCATION_SZ)]
        public string location;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALL1_SZ)]
        public string call1;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float azim;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float elev;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float dist;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float loss;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //------------------------------------------------------------------
        public const int NUM_COLUMNS = 11;

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int DELETEALL = 2;
        public const int LOCATION = 3;
        public const int CALL1 = 4;
        public const int AZIM = 5;
        public const int ELEV = 6;
        public const int DIST = 7;
        public const int LOSS = 8;
        public const int MDATE = 9;
        public const int MTIME = 10;

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int DELETEALL_SZ = Constant.FE_AZIM_DELETEALL_SZ;
        public const int LOCATION_SZ = Constant.LOCATION_SZ;
        public const int CALL1_SZ = Constant.CALLSIGN_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;
        //------------------------------------------------------------------

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "deleteall", "location", "call1", "azim", "elev", "dist", "loss", "mdate", "mtime" };

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
        static FeAzim()
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
        public FeAzim()
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

            cmd = STRING_INIT_VAL;
            recstat = STRING_INIT_VAL;
            deleteall = STRING_INIT_VAL;
            location = STRING_INIT_VAL;
            call1 = STRING_INIT_VAL;
            azim = FLOAT_INIT_VAL;
            elev = FLOAT_INIT_VAL;
            dist = FLOAT_INIT_VAL;
            loss = FLOAT_INIT_VAL;
            mdate = STRING_INIT_VAL;
            mtime = STRING_INIT_VAL;
        }

        /// <summary>
        /// This method returns a string that concatenates the 'key'
        /// fields call1 and azim.
        /// </summary>
        public string KeysToString()
        {
            return String.Format("location: {0} ;  call1: {1} ; azim: {2}", location, call1, azim);
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

            sb.AppendLine("========== FeAzim:");
            sb.Append("\ncmd = " + cmd);
            sb.Append("\nrecstat = " + recstat);
            sb.Append("\ndeleteall = " + deleteall);
            sb.Append("\nlocation = " + location);
            sb.Append("\ncall1 = " + call1);
            sb.Append("\nazim = " + azim);
            sb.Append("\nelev = " + elev);
            sb.Append("\ndist = " + dist);
            sb.Append("\nloss = " + loss);
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

            sb.AppendLine("========== FeAzim:");
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "cmd = " + cmd);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "recstat = " + recstat);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "deleteall = " + deleteall);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "location = " + location);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "call1 = " + call1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "azim = " + azim);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "elev = " + elev);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "dist = " + dist);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "loss = " + loss);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate = " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime = " + mtime);

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

            parameterValuePtr[DELETEALL] = Marshal.StringToHGlobalAnsi(deleteall);

            parameterValuePtr[LOCATION] = Marshal.StringToHGlobalAnsi(location);

            parameterValuePtr[CALL1] = Marshal.StringToHGlobalAnsi(call1);

            parameterValuePtr[AZIM] = Marshal.AllocHGlobal(sizeof(double));
            F[0] = azim;
            Marshal.Copy(F, 0, parameterValuePtr[AZIM], 1);

            parameterValuePtr[ELEV] = Marshal.AllocHGlobal(sizeof(double));
            F[0] = elev;
            Marshal.Copy(F, 0, parameterValuePtr[ELEV], 1);

            parameterValuePtr[DIST] = Marshal.AllocHGlobal(sizeof(double));
            F[0] = dist;
            Marshal.Copy(F, 0, parameterValuePtr[DIST], 1);

            parameterValuePtr[LOSS] = Marshal.AllocHGlobal(sizeof(double));
            F[0] = loss;
            Marshal.Copy(F, 0, parameterValuePtr[LOSS], 1);

            parameterValuePtr[MDATE] = Marshal.StringToHGlobalAnsi(mdate);

            parameterValuePtr[MTIME] = Marshal.StringToHGlobalAnsi(mtime);

            return parameterValuePtr;
        }



        /*
        cmd
        recstat
        deleteall
        location
        call1
        azim
        elev
        dist
        loss
        mdate
        mtime
        */

    }
}

```
