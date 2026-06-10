# Documented File: FeTitl.cs
**Repository Path:** `_DataStructures\FeTitl.cs`
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
    using SQLPOINTER = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class has fields that are isomorphic with ES PDF table <b>&lt;userID&gt;.fe_&lt;pdfName&gt;_titl</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FeTitl
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FTTITLE_VALIDATED_SZ)]
        public string validated;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FTTITLE_NAMEF_SZ)]
        public string namef;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FTTITLE_SOURCE_SZ)]
        public string source;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FTTITLE_DESCR_SZ)]
        public string descr;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.DATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.TIME_SZ)]
        public string mtime;

        //----------------------------------------------------------------------------------

        public const int NUM_COLUMNS = 6;

        public const string AllColumnsForSqlSelect = " validated, namef, source, descr, mdate, mtime";

        public const int VALIDATED = 0;
        public const int NAMEF = 1;
        public const int SOURCE = 2;
        public const int DESCR = 3;
        public const int MDATE = 4;
        public const int MTIME = 5;

        public const int VALIDATED_SZ = Constant.FTTITLE_VALIDATED_SZ;
        public const int NAMEF_SZ = Constant.FTTITLE_NAMEF_SZ;
        public const int SOURCE_SZ = Constant.FTTITLE_SOURCE_SZ;
        public const int DESCR_SZ = Constant.FTTITLE_DESCR_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //----------------------------------------------------------------------------------

        /// <summary>
        /// This is the default constructor.
        /// </summary>
        public FeTitl()
        {
            validated = "";
            namef = "";
            source = "";
            descr = "";
            mdate = "";
            mtime = "";
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
            sb.Append("\r\n");
            sb.AppendLine("validated = " + validated);
            sb.AppendLine("namef     = " + namef);
            sb.AppendLine("source    = " + source);
            sb.AppendLine("descr     = " + descr);
            sb.AppendLine("mdate     = " + mdate);
            sb.AppendLine("mtime     = " + mtime);
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

            sb.AppendLine("\n========== FeTitl:");
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "validated = " + validated);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "namef     = " + namef);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "source    = " + source);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "descr     = " + descr);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate     = " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime     = " + mtime);

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

            parameterValuePtr[VALIDATED] = Marshal.StringToHGlobalAnsi(validated);
            parameterValuePtr[NAMEF] = Marshal.StringToHGlobalAnsi(namef);
            parameterValuePtr[SOURCE] = Marshal.StringToHGlobalAnsi(source);
            parameterValuePtr[DESCR] = Marshal.StringToHGlobalAnsi(descr);
            parameterValuePtr[MDATE] = Marshal.StringToHGlobalAnsi(mdate);
            parameterValuePtr[MTIME] = Marshal.StringToHGlobalAnsi(mtime);

            return parameterValuePtr;
        }

        /*
        validated
        namef
        source
        descr
        mdate
        mtime
        */

    }
}

```
