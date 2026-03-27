using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates an MtSite object and its associated arrays of antennae and channels.
    /// The class is a clone of the legacy 'C' structure mtSiteStr: the ante and chan arrays
    /// are stored in global (heap) memory. The class MtSiteStr provides a totally 'managed' version
    /// of this type. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MtSiteStr_OLD
    {
        // IMPORTANT!
        // =========
        // The following qty. 7 member values correspond to the columns
        // of an FtSite table stored in the database; for simplicity, the
        // member and column names are prescribed to be identical.
        // The order of appearance of these qty. 7 'column' members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.Struct)]
        public MtSite stSite;                   /*	Storage for the site information  */
        public IntPtr stAntsPtr;                /*	Pointer to an array of antennas  */
        public IntPtr stChanPtr; 			    /*	Pointer to an array of channels   */
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nNumChans;                   /*  Number of Channels                */
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nNumAnts;                    /*  Number of Antennas				  */
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nDepth;                      /*	Depth user requested			  */
        public IntPtr pAntennas;                /*	Pointer normally to subupt ants	  */

        //-----------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 7;

        //------------------------------------------------------------------
        public enum Init { ALLOCATED, UNALLOCATED }

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private MtSiteStr_OLD() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initMode"></param>
        /// <returns></returns>
        public MtSiteStr_OLD(Init initMode)
        {
            if (initMode == Init.UNALLOCATED)
            {
                stSite = null;
            }
            else if (initMode == Init.ALLOCATED)
            {
                stSite = new MtSite();
            }

            // We do not know how many elements the arrays will have so just assign 'null' pointer values.
            stAntsPtr = IntPtr.Zero;
            stChanPtr = IntPtr.Zero;
            pAntennas = IntPtr.Zero;

            nNumChans = 0;
            nNumAnts = 0;
            nDepth = -666;  //This is deliberately set to an invalid value;
        }

        [DllImport("_APItest.dll", CharSet = CharSet.Ansi)]
        private static extern int API_SizeOf_mtAnte_();
        [DllImport("_APItest.dll", CharSet = CharSet.Ansi)]
        private static extern int API_SizeOf_mtChan_();

        /// <summary>
        /// This method returns a deep-copy of the MtAnte object located
        /// at a prescribed index position in the antenna array whose start position
        /// in global memory is given by stAntsPtr.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public MtAnte Get_stAnts(int index)
        {
            MtAnte mtAnte = null;

            if (stAntsPtr != null)
            {
                bool indexWithinBounds = (index >= 0) && (index < nNumAnts);
                if (!indexWithinBounds)
                {
                    Console.WriteLine("MtAnte.Get_stAnts(int index): index = " + index + " is out of bounds.");
                    Console.WriteLine("                              nNumAnts = " + nNumAnts);
                    Environment.Exit(666);
                }
                IntPtr intPtr = stAntsPtr + (index * API_SizeOf_mtAnte_());
                mtAnte = new MtAnte();
                Marshal.PtrToStructure(stAntsPtr, mtAnte);
            }
            else
            {
                Console.WriteLine("MtAnte.Get_stAnts(int index): stAntsPtr is null.");
                Environment.Exit(666);
            }
            return mtAnte;
        }

        /// <summary>
        /// This method returns a deep-copy of the MtChan object located
        /// at a prescribed index position in the channel array whose start position
        /// in global memory is given by stChanPtr.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public MtChan Get_stChan(int index)
        {
            MtChan mtChan = null;

            if (stChanPtr != null)
            {
                bool indexWithinBounds = (index >= 0) && (index < nNumChans);
                if (!indexWithinBounds)
                {
                    Console.WriteLine("MtChan.Get_stChan(int index): index = " + index + " is out of bounds.");
                    Console.WriteLine("                              nNumChans = " + nNumChans);
                    Environment.Exit(666);
                }
                IntPtr intPtr = stChanPtr + (index * API_SizeOf_mtChan_());
                mtChan = new MtChan();
                Marshal.PtrToStructure(stChanPtr, mtChan);
            }
            else
            {
                Console.WriteLine("MtChan.Get_stChan(int index): stChanPtr is null.");
                Environment.Exit(666);
            }
            return mtChan;
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
            sb.Append("\r\n");
            sb.Append("\r\n===== MtSiteStr (Start) =====");
            sb.Append("\r\nnNumChans = " + nNumChans);
            sb.Append("\r\nnNumAnts = " + nNumAnts);
            sb.Append("\r\nnDepth = " + nDepth);
            sb.Append("\r\npAntennas = " + "0x" + pAntennas.ToString("x16"));

            sb.Append("\r\n");
            sb.Append("\r\n===== MtSite =====");
            sb.Append(stSite);

            for (int index = 0; index < nNumAnts; index++)
            {
                sb.Append("\r\n===== MtAnte [" + index + "] =====");
                sb.Append(Get_stAnts(index).ToString());
            }

            for (int index = 0; index < nNumChans; index++)
            {
                sb.Append("\r\n===== MtChan [" + index + "] =====");
                sb.Append(Get_stChan(index).ToString());
            }

            sb.Append("\r\n===== MtSiteStr (End) =====");
            sb.Append("\r\n");
            return sb.ToString();
        }
    }
}
