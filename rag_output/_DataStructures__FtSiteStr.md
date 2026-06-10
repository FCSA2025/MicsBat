# Documented File: FtSiteStr.cs
**Repository Path:** `_DataStructures\FtSiteStr.cs`
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
    /// This class encapsulates an FtSite object and its associated FtAnte[],
    /// and FtChan[] arrays.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FtSiteStr
    {
        [MarshalAsAttribute(UnmanagedType.Struct)]
        public FtSite stSite;           /*	Storage for the site information  */

        public FtAnte[] stAntsPtr;     /*	Pointer to an array of antennas  */

        public FtChan[] stChanPtr; 			/*	Pointer to an array of channels   */

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nNumChans;           /*  Number of Channels                */

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nNumAnts;            /*  Number of Antennas				  */

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nDepth;              /*	Depth user requested			  */

        public IntPtr pAntennas;        /*	Pointer normally to subupt ants	  */

        //-----------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 7;

        //------------------------------------------------------------------
        public enum Init { ALLOCATED, UNALLOCATED }

        /// <summary>
        /// Private default constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private FtSiteStr() { }

        /// <summary>
        /// Object constructor.
        /// </summary>
        /// <param name="initMode"></param>
        /// <returns></returns>
        public FtSiteStr(Init initMode)
        {
            if (initMode == Init.UNALLOCATED)
            {
                // Do nothing.
            }
            else if (initMode == Init.ALLOCATED)
            {
                stSite = new FtSite();
            }

            //stAntsPtr = null;  // We don't know anything about the array dimensions.
            //stChanPtr = null;  // We don't know anything about the array dimensions.
            stAntsPtr = new FtAnte[0];
            stChanPtr = new FtChan[0];

            pAntennas = IntPtr.Zero;
            nNumChans = 0;
            nNumAnts = 0;
            nDepth = 1;  //Default setting is site only.
        }

        /// <summary>
        /// Constructor that creates a new object and initializes its member
        /// values to those of a prescribed (legacy) FtSiteStr_OLD object. 
        /// (FtSiteStr_OLD objects have ante and chan arrays stored in global memory
        /// whereas FtSiteStr is a 'managed' object.)
        /// </summary>
        /// <param name="ftSiteStr_OLD"></param>
        /// <returns></returns>
        public FtSiteStr(FtSiteStr_OLD ftSiteStr_OLD)
        {
            // Scalars.
            nNumChans = ftSiteStr_OLD.nNumChans;
            nNumAnts = ftSiteStr_OLD.nNumAnts;
            nDepth = ftSiteStr_OLD.nDepth;

            // IntPtr.
            pAntennas = ftSiteStr_OLD.pAntennas;

            // FtSite object.
            stSite = ftSiteStr_OLD.stSite;

            // FtAnte[].
            stAntsPtr = new FtAnte[nNumAnts];
            int stride = Marshal.SizeOf<FtAnte>();
            for (int i = 0; i < nNumAnts; i++)
            {
                IntPtr offset = ftSiteStr_OLD.stAntsPtr + (i * stride);
                stAntsPtr[i] = Marshal.PtrToStructure<FtAnte>(offset);
            }

            // FtChan[].
            stChanPtr = new FtChan[nNumChans];
            stride = Marshal.SizeOf<FtChan>();
            for (int i = 0; i < nNumChans; i++)
            {
                IntPtr offset = ftSiteStr_OLD.stChanPtr + (i * stride);
                stChanPtr[i] = Marshal.PtrToStructure<FtChan>(offset);
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
            sb.Append("\r\n");
            sb.Append("\r\n===== FtSiteStr (Start) =====");
            sb.Append("\r\nnNumChans = " + nNumChans);
            sb.Append("\r\nnNumAnts = " + nNumAnts);
            sb.Append("\r\nnDepth = " + nDepth);
            sb.Append("\r\npAntennas = " + "0x" + pAntennas.ToString("x16"));

            sb.Append("\r\n");
            sb.Append("\r\n===== FtSite =====");
            sb.Append(stSite.ToString());

            if (stAntsPtr == null)
            {
                sb.Append("\r\nstAntsPtr is NULL");
            }
            else
            {
                for (int index = 0; index < nNumAnts; index++)
                {
                    sb.Append("\r\n===== FtAnte [" + index + "] =====");

                    if (stAntsPtr[index] == null)
                    {
                        sb.Append("\r\nNULL");
                    }
                    else
                    {
                        sb.Append(stAntsPtr[index].ToString());
                    }
                }
            }

            if (stChanPtr == null)
            {
                sb.Append("\r\nstChanPtr is NULL");
            }
            else
            {
                for (int index = 0; index < nNumChans; index++)
                {
                    sb.Append("\r\n===== FtChan [" + index + "] =====");

                    if (stChanPtr[index] == null)
                    {
                        sb.Append("\r\nNULL");
                    }
                    else
                    {
                        sb.Append(stChanPtr[index].ToString());
                    }
                }
            }

            sb.Append("\r\n===== FtSiteStr (End) =====");
            sb.Append("\r\n");
            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the just the FtSite object and the 
        /// primitive type members.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string ToStringFtSiteOnly()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n");
            sb.Append("\r\n===== FtSiteOnly (Start) =====");
            sb.Append("\r\nnNumAnts = " + nNumAnts);
            sb.Append("\r\nnNumChans = " + nNumChans);
            sb.Append("\r\nnDepth = " + nDepth);
            sb.Append("\r\npAntennas = " + "0x" + pAntennas.ToString("x16"));

            sb.Append("\r\n");
            sb.Append("\r\n===== FtSiteOnly (End) =====");
            sb.Append(stSite);

            return sb.ToString();
        }

        /// <summary>
        /// This static method creates a new FtSiteStr object and initializes its member
        /// values to those of a prescribed (legacy) FtSiteStr_OLD object. 
        /// (FtSiteStr_OLD objects have ante and chan arrays stored in global memory
        /// whereas FtSiteStr is a 'managed' object.)
        /// </summary>
        /// <param name="ftSiteStr"></param>
        /// <returns></returns>
        public static FtSiteStr Convert(FtSiteStr_OLD ftSiteStr)
        {
            FtSiteStr ftSiteStr_NEW = null;

            if (ftSiteStr != null)
            {
                ftSiteStr_NEW = new FtSiteStr();

                ftSiteStr_NEW.stSite = ftSiteStr.stSite;         //	Storage for the site information  

                ftSiteStr_NEW.stAntsPtr = new FtAnte[ftSiteStr.nNumAnts];     // Pointer to an array of antennas
                IntPtr intPtr = ftSiteStr.stAntsPtr;
                int offset = Marshal.SizeOf<FtAnte>();
                for (int nInd = 0; nInd < ftSiteStr.nNumAnts; nInd++)
                {
                    ftSiteStr_NEW.stAntsPtr[nInd] = new FtAnte();
                    Marshal.PtrToStructure(intPtr, ftSiteStr_NEW.stAntsPtr[nInd]);
                    intPtr += offset;
                }

                ftSiteStr_NEW.stChanPtr = new FtChan[ftSiteStr.nNumChans];    // Pointer to an array of channels   
                intPtr = ftSiteStr.stChanPtr;
                offset = Marshal.SizeOf<FtChan>();
                for (int nInd = 0; nInd < ftSiteStr.nNumChans; nInd++)
                {
                    ftSiteStr_NEW.stChanPtr[nInd] = new FtChan();
                    Marshal.PtrToStructure(intPtr, ftSiteStr_NEW.stChanPtr[nInd]);
                    intPtr += offset;
                }

                ftSiteStr_NEW.nNumChans = ftSiteStr.nNumChans;   // Number of Channels                

                ftSiteStr_NEW.nNumAnts = ftSiteStr.nNumAnts;     // Number of Antennas				  

                ftSiteStr_NEW.nDepth = ftSiteStr.nDepth;         //	Depth user requested			  

                ftSiteStr_NEW.pAntennas = ftSiteStr.pAntennas;   //	Pointer normally to subupt ants	  
            }

            return ftSiteStr_NEW;
        }





    }
}

```
