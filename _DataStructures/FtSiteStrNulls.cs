using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using System.Runtime.InteropServices;
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
    /// This class encapsulates all the ODBC nullInd values for a populated FtSiteStr object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FtSiteStrNulls
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

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = FtSite.NUM_COLUMNS, ArraySubType = UnmanagedType.I8)]
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
        private FtSiteStrNulls() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initMode"></param>
        /// <returns></returns>
        public FtSiteStrNulls(Init initMode)
        {
            if (initMode == Init.UNALLOCATED)
            {
                anSiteNull = null;
            }
            else if (initMode == Init.ALLOCATED)
            {
                anSiteNull = NullHelper.CreateArrayOfNullInd(FtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            }

            anAntsNullPtr = null;  // We don't know anything about the array dimensions.
            anChanNullPtr = null;  // We don't know anything about the array dimensions.
        }

        /// <summary>
        /// This constructor creates a new FtSiteStrNulls object and populates it with
        /// deep-copies of the member values of a prescribed FtSiteStrNulls_OLD object.
        /// </summary>
        /// <param name="ftSiteStrNull_OLD"></param>
        /// <param name="numAntennae"></param>
        /// <param name="numChannels"></param>
        /// <returns></returns>
        public FtSiteStrNulls(FtSiteStrNulls_OLD ftSiteStrNull_OLD, int numAntennae, int numChannels)
        {
            // SQLLEN[].
            anSiteNull = ftSiteStrNull_OLD.anSiteNull;

            // SQLLEN[][].
            anAntsNullPtr = new SQLLEN[numAntennae][];
            int stride = Marshal.SizeOf<SQLLEN>() * FtAnte.NUM_COLUMNS;
            for (int i = 0; i < numAntennae; i++)
            {
                IntPtr offset = ftSiteStrNull_OLD.anAntsNull + i * stride;
                anAntsNullPtr[i] = new SQLLEN[FtAnte.NUM_COLUMNS];
                Marshal.Copy(offset, anAntsNullPtr[i], 0, FtAnte.NUM_COLUMNS);
            }

            anChanNullPtr = new SQLLEN[numChannels][];
            stride = Marshal.SizeOf<SQLLEN>() * FtChan.NUM_COLUMNS;
            for (int i = 0; i < numChannels; i++)
            {
                IntPtr offset = ftSiteStrNull_OLD.anChanNull + i * stride;
                anChanNullPtr[i] = new SQLLEN[FtChan.NUM_COLUMNS];
                Marshal.Copy(offset, anChanNullPtr[i], 0, FtChan.NUM_COLUMNS);
            }

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

            sb.Append("\n\n===== anSiteNull[] =====");
            for (int i = 0; i < FtSite.NUM_COLUMNS; i++)
            {
                sb.Append("\n   anSiteNull[" + i + "] = " + anSiteNull[i]);
            }

            return sb.ToString();
        }


    }
}
