# Documented File: Ctx_Xref.cs
**Repository Path:** `_DataStructures\Ctx_Xref.cs`
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

namespace _DataStructures
{
    [StructLayout(LayoutKind.Sequential)]
    public class Ctx_Xref
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TFCR_SZ)]
        public string tfcr;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TFCI_SZ)]
        public string tfci;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RXEQP_SZ)]
        public string rxeqp;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = XREF_TFCR_SZ)]
        public string xref_tfcr;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = XREF_TFCI_SZ)]
        public string xref_tfci;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = XREF_RXEQP_SZ)]
        public string xref_rxeqp;

        //-------------------------------------------------------------------------

        public const int NUM_COLUMNS = 6;

        //-------------------------------------------------------------------------

        public const int TFCR_SZ = 7;
        public const int TFCI_SZ = 7;
        public const int RXEQP_SZ = 9;
        public const int XREF_TFCR_SZ = 7;
        public const int XREF_TFCI_SZ = 7;
        public const int XREF_RXEQP_SZ = 9;

        //-------------------------------------------------------------------------

        public const int TFCR = 0;
        public const int TFCI = 1;
        public const int RXEQP = 2;
        public const int XREF_TFCR = 3;
        public const int XREF_TFCI = 4;
        public const int XREF_RXEQP = 5;






    }
}

```
