# Documented File: FtSiteStr_OLD.cs
**Repository Path:** `_DataStructures\FtSiteStr_OLD.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates an FtSite object and its associated arrays of antennae and channels.
    /// The class is a clone of the legacy 'C' structure ftSiteStr_: the ante and chan arrays
    /// are stored in global (heap) memory. The class FtSiteStr provides a totally 'managed' version
    /// of this type.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FtSiteStr_OLD
    {
        [MarshalAsAttribute(UnmanagedType.Struct)]
        public FtSite stSite;           /*	Storage for the site information  */

        public IntPtr stAntsPtr;     /*	Pointer to an array of antennas  */

        public IntPtr stChanPtr; 			/*	Pointer to an array of channels   */

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nNumChans;           /*  Number of Channels                */

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nNumAnts;            /*  Number of Antennas				  */

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nDepth;              /*	Depth user requested			  */

        public IntPtr pAntennas;        /*	Pointer normally to subupt ants	  */

        //----------------------------------------------------------------------------
        public enum Init { ALLOCATED, UNALLOCATED }

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private FtSiteStr_OLD() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initMode"></param>
        /// <returns></returns>
        public FtSiteStr_OLD(Init initMode)
        {
            if (initMode == Init.UNALLOCATED)
            {
                //Do nothing: objects and pointers are not instantiated.
            }
            else if (initMode == Init.ALLOCATED)
            {
                stSite = new FtSite();

                //As we are blitting to a C structure, the pointer stAntsPtr MUST
                //point to an address in .NET's 'UnManaged' memory.
                FtAnte ftAnte = new FtAnte();
                IntPtr ftAntePtr = Marshal.AllocHGlobal(Marshal.SizeOf(ftAnte));
                Marshal.StructureToPtr(ftAnte, ftAntePtr, false);
                stAntsPtr = ftAntePtr;

                //As we are blitting to a C structure, the pointer stChanPtr MUST
                //point to an address in .NET's 'UnManaged' memory.
                FtChan ftChan = new FtChan();
                IntPtr ftChanPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ftChan));
                Marshal.StructureToPtr(ftChan, ftChanPtr, false);
                stChanPtr = ftChanPtr;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ftSiteStr"></param>
        /// <returns></returns>
        public FtSiteStr_OLD(FtSiteStr ftSiteStr)
        {
            if (ftSiteStr == null)
            {
                Log2.e("\nFtSiteStr_OLD.FtSiteStr_OLD(): ERROR: ftSiteStr is null.");
                return;
            }

            // FtSite.
            stSite = ftSiteStr.stSite;

            // Copy managed arrays to an allocated block of global memory.
            stAntsPtr = Arrays.CopyArrayToGlobalMemory<FtAnte>(ftSiteStr.stAntsPtr);
            stChanPtr = Arrays.CopyArrayToGlobalMemory<FtChan>(ftSiteStr.stChanPtr);

            // Scalars.
            nNumChans = ftSiteStr.nNumChans;
            nNumAnts = ftSiteStr.nNumAnts;
            nDepth = ftSiteStr.nDepth;
            pAntennas = ftSiteStr.pAntennas;
        }


        [DllImport("_APItest.dll", CharSet = CharSet.Ansi)]
        private static extern int API_SizeOf_ftAnte_();
        [DllImport("_APItest.dll", CharSet = CharSet.Ansi)]
        private static extern int API_SizeOf_ftChan_();

        /// <summary>
        /// This method returns a deep-copy of the FtAnte object located
        /// at a prescribed index position in the antenna array whose start position
        /// in global memory is given by stAntsPtr.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public FtAnte Get_stAnts(int index)
        {
            FtAnte ftAnte = null;

            if (stAntsPtr != null)
            {
                bool indexWithinBounds = (index >= 0) && (index < nNumAnts);
                if (!indexWithinBounds)
                {
                    Console.WriteLine("FtAnte.Get_stAnts(int index): index = " + index + " is out of bounds.");
                    Console.WriteLine("                              nNumAnts = " + nNumAnts);
                    Environment.Exit(666);
                }
                IntPtr intPtr = stAntsPtr + (index * API_SizeOf_ftAnte_());
                ftAnte = new FtAnte();
                Marshal.PtrToStructure(stAntsPtr, ftAnte);
            }
            else
            {
                Console.WriteLine("FtAnte.Get_stAnts(int index): stAntsPtr is null.");
                Environment.Exit(666);
            }
            return ftAnte;
        }

        /// <summary>
        /// This method returns a deep-copy of the FtChan object located
        /// at a prescribed index position in the channel array whose start position
        /// in global memory is given by stChanPtr.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public FtChan Get_stChan(int index)
        {
            FtChan ftChan = null;

            if (stChanPtr != null)
            {
                bool indexWithinBounds = (index >= 0) && (index < nNumChans);
                if (!indexWithinBounds)
                {
                    Console.WriteLine("FtChan.Get_stChan(int index): index = " + index + " is out of bounds.");
                    Console.WriteLine("                              nNumChans = " + nNumChans);
                    Environment.Exit(666);
                }
                IntPtr intPtr = stChanPtr + (index * API_SizeOf_ftChan_());
                ftChan = new FtChan();
                Marshal.PtrToStructure(stChanPtr, ftChan);
            }
            else
            {
                Console.WriteLine("FtChan.Get_stChan(int index): stChanPtr is null.");
                Environment.Exit(666);
            }
            return ftChan;
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
            sb.Append("\r\n===== FtSiteStr (Start) =====");
            sb.Append("\r\nnNumAnts = " + nNumAnts);
            sb.Append("\r\nnNumChans = " + nNumChans);
            sb.Append("\r\nnDepth = " + nDepth);
            sb.Append("\r\npAntennas = " + "0x" + pAntennas.ToString("x16"));

            sb.Append("\r\n");
            sb.Append("\r\n===== FtSite =====");
            sb.Append(stSite);

            for (int index = 0; index < nNumAnts; index++)
            {
                sb.Append("\r\n===== FtAnte [" + index + "] =====");
                sb.Append(Get_stAnts(index).ToString());
            }

            for (int index = 0; index < nNumChans; index++)
            {
                sb.Append("\r\n===== FtChan [" + index + "] =====");
                sb.Append(Get_stChan(index).ToString());
            }

            sb.Append("\r\n===== FtSiteStr (End) =====");
            sb.Append("\r\n");
            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the FtSite object and primitive type field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string ToStringFtSiteOnly()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n");
            sb.Append("\r\n===== FtSiteStr (Start) =====");
            sb.Append("\r\nnNumAnts = " + nNumAnts);
            sb.Append("\r\nnNumChans = " + nNumChans);
            sb.Append("\r\nnDepth = " + nDepth);
            sb.Append("\r\npAntennas = " + "0x" + pAntennas.ToString("x16"));

            sb.Append("\r\n");
            sb.Append("\r\n===== FtSite =====");
            sb.Append(stSite);

            return sb.ToString();
        }



    }
}

```
