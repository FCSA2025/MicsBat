# Documented File: FtChng.cs
**Repository Path:** `_DataStructures\FtChng.cs`
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
    /// This class has fields that are isomorphic with TS PDF table <b>&lt;userID&gt;.ft_&lt;pdfName&gt;_chng</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FtChng
    {
        // IMPORTANT!
        // =========
        // The following qty. 3 member values correspond to the columns
        // of an FtChng table stored in the database; for simplicity, the
        // member and column names are prescribed to be identical.
        // The order of appearance of these qty. 3 'column' members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NEWCALL1_SZ)]
        public string newcall1;  //#0
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = OLDCALL1_SZ)]
        public string oldcall1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NAME_SZ)]
        public string name;

        //--------------------------------------------------------------------

        public const int NUM_COLUMNS = 3;

        public const int NEWCALL1 = 0;
        public const int OLDCALL1 = 1;
        public const int NAME = 2;

        public const int NEWCALL1_SZ = Constant.DB_CALL_SZ;
        public const int OLDCALL1_SZ = Constant.DB_CALL_SZ;
        public const int NAME_SZ = Constant.FT_SITE_NAME_SZ;

        public const string AllColumnsForSqlSelect = " newCall1, oldCall1, name ";

        //--------------------------------------------------------------------------

        /// <summary>
        /// Default object constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public FtChng()
        {
            newcall1 = "";
            oldcall1 = "";
            name = "";
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

            sb.Append("\r\nnewcall1 = " + newcall1);
            sb.Append("\r\noldcall1 = " + oldcall1);
            sb.Append("\r\nname =     " + name);

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

            sb.AppendLine("\n========== FtChngCall:");
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "newcallsign = " + newcall1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "oldcallsign = " + oldcall1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "name        = " + name);

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
            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[NEWCALL1] = Marshal.StringToHGlobalAnsi(newcall1);

            parameterValuePtr[OLDCALL1] = Marshal.StringToHGlobalAnsi(oldcall1);

            parameterValuePtr[NAME] = Marshal.StringToHGlobalAnsi(name);

            return parameterValuePtr;
        }


    }
}

```
