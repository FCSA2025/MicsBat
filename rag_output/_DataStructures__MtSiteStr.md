# Documented File: MtSiteStr.cs
**Repository Path:** `_DataStructures\MtSiteStr.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates an MtSite object and its associated MtAnte[],
    /// and MtChan[] arrays.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    [Serializable]
    public class MtSiteStr
    {
        // IMPORTANT!
        // =========
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
        public MtAnte[] stAntsPtr;                /* An array of antennas  */
        public MtChan[] stChanPtr; 			      /* An array of channels   */
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
        private MtSiteStr() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initMode"></param>
        /// <returns></returns>
        public MtSiteStr(Init initMode)
        {
            if (initMode == Init.UNALLOCATED)
            {
                stSite = null;

            }
            else if (initMode == Init.ALLOCATED)
            {
                stSite = new MtSite();
            }

            stAntsPtr = null;  // We don't know anything about the array dimensions.
            stChanPtr = null;  // We don't know anything about the array dimensions.
            pAntennas = IntPtr.Zero;
            nNumChans = 0;
            nNumAnts = 0;
            nDepth = -666;  //This is deliberately set to an invalid value;
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
                sb.Append("\r\n===== MtAnte [" + index + "] Key Only =====");
                sb.Append(stAntsPtr[index].KeysToString());
            }

            for (int index = 0; index < nNumChans; index++)
            {
                sb.Append("\r\n===== MtChan [" + index + "] Key Only =====");
                sb.Append(stChanPtr[index].KeysToString());
            }

            sb.Append("\r\n===== MtSiteStr (End) =====");
            sb.Append("\r\n");
            return sb.ToString();
        }



    }
}


```
