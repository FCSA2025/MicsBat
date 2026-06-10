# Documented File: FtSiteStrNulls_OLD.cs
**Repository Path:** `_DataStructures\FtSiteStrNulls_OLD.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using _NewLib;
    using System.Runtime.InteropServices;
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;            //Invented to mimic (char *) for [In]  only.
    using SQLCHARPTRINOUT = IntPtr;       //Invented to mimic (char *) for [In, Out].
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLINTEGERPTR = IntPtr;
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
    /// This class encapsulates ODBC nullInds for an FtSite object and its associated 
    /// nullInd arrays for the associated antennae and channels.
    /// The class is a clone of the legacy 'C' structure ftSiteStrNull_: the ante and 
    /// chan nullInd arrays are stored in global (heap) memory. The class FtSiteStrNulls
    /// provides a totally 'managed' version of this type.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FtSiteStrNulls_OLD
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

        //public IntPtr anSiteNull;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = FtSite.NUM_COLUMNS)]
        public SQLLEN[] anSiteNull;

        public IntPtr anAntsNull;

        public IntPtr anChanNull;

        //---------------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 3;

        //---------------------------------------------------------------------------

        public enum Init { ALLOCATED, UNALLOCATED }

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private FtSiteStrNulls_OLD() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initMode"></param>
        /// <returns></returns>
        public FtSiteStrNulls_OLD(Init initMode)
        {
            if (initMode == Init.UNALLOCATED)
            {
                //Do nothing: objects and pointers are not instantiated.
            }
            else if (initMode == Init.ALLOCATED)
            {
                //anSiteNull = Marshal.AllocHGlobal(FtSite.NUM_COLUMNS * sizeof(SQLLEN));
                anSiteNull = new SQLLEN[FtSite.NUM_COLUMNS];
                anAntsNull = new IntPtr();
                anChanNull = new IntPtr();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ftSiteStrNulls"></param>
        /// <returns></returns>
        public FtSiteStrNulls_OLD(FtSiteStrNulls ftSiteStrNulls)
        {
            if (ftSiteStrNulls == null)
            {
                Log2.e("\nFtSiteStrNulls_OLD.FtSiteStrNulls_OLD(): ERROR: ftSiteStrNulls is null.");
                return;
            }

            // SQLLEN[].
            anSiteNull = ftSiteStrNulls.anSiteNull;

            // SQLLEN[][];
            anAntsNull = NullHelper.CopyNullInd2dArrayIntoGlobalMemory(ftSiteStrNulls.anAntsNullPtr);

            anChanNull = NullHelper.CopyNullInd2dArrayIntoGlobalMemory(ftSiteStrNulls.anChanNullPtr);

        }




    }
}

```
