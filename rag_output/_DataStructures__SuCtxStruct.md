# Documented File: SuCtxStruct.cs
**Repository Path:** `_DataStructures\SuCtxStruct.cs`
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
    public class SuCtxStruct
    {
        [MarshalAsAttribute(UnmanagedType.LPStruct)]
        public SuCtx CtxV;		/*	The basic CTX data */

        [MarshalAsAttribute(UnmanagedType.LPStruct)]
        public SuCtxD[] pCtxD;  /*	The array of data points */

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nDepth; /*	1 for just the CTX data, 2 for points as well */



    }
}

```
