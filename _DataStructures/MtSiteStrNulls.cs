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
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]

    /// <summary>
    /// This class encapsulates all the ODBC nullInd values for a populated MtSiteStr object.
    /// </summary>
    public class MtSiteStrNulls
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
        public SQLLEN[][] anAntsNullPtr;
        [MarshalAsAttribute(UnmanagedType.SysUInt)]
        public SQLLEN[][] anChanNullPtr;

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
        private MtSiteStrNulls() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initMode"></param>
        /// <returns></returns>
        public MtSiteStrNulls(Init initMode)
        {
            if (initMode == Init.UNALLOCATED)
            {
                anSiteNull = null;
            }
            else if (initMode == Init.ALLOCATED)
            {
                anSiteNull = NullHelper.CreateArrayOfNullInd(MtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            }

            anAntsNullPtr = null;  // We don't know anything about the array dimensions.
            anChanNullPtr = null;  // We don't know anything about the array dimensions.
        }




    }
}

