# Documented File: SuFeeCode.cs
**Repository Path:** `_DataStructures\SuFeeCode.cs`
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
    /// <summary>
    /// This class has member fields that are isomorphic with the 
    /// legacy 'C' structure suFeeCode_.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SuFeeCode
    {
        // IMPORTANT!
        // =========
        // The following qty. 3 member values correspond to the legacy native 
        // structure suFeeCode_; member names are prescribed to be identical.
        // The order of appearance of these qty. 3 field members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.SU_FEE_CODE_CODE_SZ)]
        public string code;    //#0
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.SU_FEE_CODE_NUM_CHANS_SZ)]
        public string num_chans;    //#1
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float fee;    //#2

        //----------------------------------------------------------------
        //Additional public static members.

        public const int NUM_COLUMNS = 3;

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\r\ncode =      " + code);
            sb.Append("\r\nnum_chans = " + num_chans);
            sb.Append("\r\nfee =       " + fee);

            return sb.ToString();
        }
    }
}

```
