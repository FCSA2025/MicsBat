# Documented File: MtSiteNull_OLD.cs
**Repository Path:** `_DataStructures\MtSiteNull_OLD.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using _Configuration;
    using _NewLib;
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;  //Invented to mimic (char *)
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLLEN = Int64;
    using SQLLENPTR = IntPtr;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;
    using SQLSETPOSIROW = UInt64;
    using SQLSMALLINT = Int16;
    using SQLSMALLINTPTR = IntPtr;
    using SQLULEN = UInt64;
    using SQLUSMALLINT = UInt16;

    /// <summary>
    /// This class encapsulates ODBC nullInds for an MtSite object and its associated 
    /// nullInd arrays for the associated antennae and channels.
    /// The class is a clone of the legacy 'C' structure mtSiteNulls: the ante and 
    /// chan nullInd arrays are stored in global (heap) memory. The class MtSiteStrNulls
    /// provides a totally 'managed' version of this type.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MtSiteNull_OLD
    {
        // IMPORTANT!
        // =========
        // The following qty. 3 member values correspond to the columns
        // of an FtSite table stored in the database; for simplicity, the
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

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = Constant.MT_SITE_SIZE_, ArraySubType = UnmanagedType.I8)]
        public SQLLEN[] anSiteNull;
        [MarshalAsAttribute(UnmanagedType.SysUInt)]
        public SQLLENPTR anAntsNullPtr;
        [MarshalAsAttribute(UnmanagedType.SysUInt)]
        public SQLLENPTR anChanNullPtr;

        //------------------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 3;

        //------------------------------------------------------------------------------
        public enum Init { ALLOCATED, UNALLOCATED }

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private MtSiteNull_OLD() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initMode"></param>
        /// <returns></returns>
        public MtSiteNull_OLD(Init initMode)
        {
            anAntsNullPtr = SQLLENPTR.Zero;
            anChanNullPtr = SQLLENPTR.Zero;

            if (initMode == Init.UNALLOCATED)
            {
                anSiteNull = null;
            }
            else if (initMode == Init.ALLOCATED)
            {
                anSiteNull = NullHelper.CreateArrayOfNullInd(Constant.MT_SITE_SIZE_, NullHelper.ColumnStatus.NULL);
            }
        }

    }
}

```
