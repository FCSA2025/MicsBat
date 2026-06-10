# Documented File: FeCCal.cs
**Repository Path:** `_DataStructures\FeCCal.cs`
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

    public class FeCCal
    {
        public string newcallsign;      // Constant.CALLSIGN_SZ
        public string oldcallsign;      // Constant.CALLSIGN_SZ

        //------------------------------------------------------------------

        public const int NUM_COLUMNS = 2;

        public const int NEWCALLSIGN = 0;
        public const int OLDCALLSIGN = 1;

        public const int NEWCALLSIGN_SZ = Constant.CALLSIGN_SZ;
        public const int OLDCALLSIGN_SZ = Constant.CALLSIGN_SZ;

        public const string AllColumnsForSqlSelect = " newCallsign, oldCallsign ";

        //------------------------------------------------------------------

        /// <summary>
        /// This is the default constructor.
        /// </summary>
        public FeCCal()
        {
            newcallsign = "";
            oldcallsign = "";
        }

        /// <summary>
        /// This method returns the DB table key field 'newcallsign'.
        /// </summary>
        public string KeysToString()
        {
            return String.Format("newcallsign: {0}", newcallsign);
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

            sb.AppendLine("newcallsign = " + newcallsign);
            sb.AppendLine("oldcallsign = " + oldcallsign);

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

            sb.AppendLine("\n========== FeCCal:");
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "newcallsign = " + newcallsign);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "oldcallsign = " + oldcallsign);

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
            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[NEWCALLSIGN] = Marshal.StringToHGlobalAnsi(newcallsign);

            parameterValuePtr[OLDCALLSIGN] = Marshal.StringToHGlobalAnsi(oldcallsign);

            return parameterValuePtr;
        }





    }
}

```
