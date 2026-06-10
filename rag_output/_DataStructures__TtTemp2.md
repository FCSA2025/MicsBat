# Documented File: TtTemp2.cs
**Repository Path:** `_DataStructures\TtTemp2.cs`
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

    /// <summary>
    /// This class encapsulates the dataset for a pair of callsigns.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TtTemp2
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALL1_SZ)]
        public string call1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALL2_SZ)]
        public string call2;

        //---------------------------------------------------------------------

        public const int NUM_COLUMNS = 2;

        //---------------------------------------------------------------------

        public const int CALL1 = 0;
        public const int CALL2 = 1;

        //---------------------------------------------------------------------

        public const int CALL1_SZ = Constant.CALLSIGN_SZ;
        public const int CALL2_SZ = Constant.CALLSIGN_SZ;

        //---------------------------------------------------------------------

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the member fields.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "";
            str += String.Format("\n   call1 = {0}", call1);
            str += String.Format("\n   call2 = {0}", call2);

            return str;
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
            string str = "";
            str += String.Format("\n{0}   call1 = {1}", nullInds[0], call1);
            str += String.Format("\n{0}   call2 = {1}", nullInds[1], call2);

            return str;
        }

    }
}

```
