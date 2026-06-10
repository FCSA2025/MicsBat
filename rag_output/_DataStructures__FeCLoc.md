# Documented File: FeCLoc.cs
**Repository Path:** `_DataStructures\FeCLoc.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using System.Runtime.InteropServices;
    using SQLPOINTER = IntPtr;
    using SQLLEN = Int64;

    public class FeCLoc
    {
        public string newlocation;      // Constant.LOCATION_SZ

        public string oldlocation;      // Constant.LOCATION_SZ

        public string name;             // 33

        //------------------------------------------------------------------

        public const int NUM_COLUMNS = 3;

        public const int NEWLOCATION = 0;
        public const int OLDLOCATION = 1;
        public const int NAME = 2;

        public const int NEWLOCATION_SZ = Constant.LOCATION_SZ;
        public const int OLDLOCATION_SZ = Constant.LOCATION_SZ;
        public const int NAME_SZ = Constant.FT_SITE_NAME_SZ;

        public const string AllColumnsForSqlSelect = " newlocation, oldlocation, name ";

        //------------------------------------------------------------------

        /// <summary>
        /// This is the default constructor.
        /// </summary>
        public FeCLoc()
        {
            newlocation = "";
            oldlocation = "";
            name = "";
        }

        /// <summary>
        /// This method returns the DB table key field 'newlocation'.
        /// </summary>
        public string KeysToString()
        {
            return String.Format("newlocation: {0}", newlocation);
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

            sb.AppendLine("newlocation = " + newlocation);
            sb.AppendLine("oldlocation = " + oldlocation);
            sb.AppendLine("name        = " + name);

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

            sb.AppendLine("\n========== FeCLoc:");

            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "newlocation = " + newlocation);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "oldlocation = " + oldlocation);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "name        = " + name);

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

            parameterValuePtr[NEWLOCATION] = Marshal.StringToHGlobalAnsi(newlocation);

            parameterValuePtr[OLDLOCATION] = Marshal.StringToHGlobalAnsi(oldlocation);

            parameterValuePtr[NAME] = Marshal.StringToHGlobalAnsi(name);

            return parameterValuePtr;
        }


    }
}

```
