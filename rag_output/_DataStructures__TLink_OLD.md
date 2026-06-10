# Documented File: TLink_OLD.cs
**Repository Path:** `_DataStructures\TLink_OLD.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using _Configuration;
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
    /// This class encapsulates the dataset required to characterize a single link. 
    /// The class is a clone of the legacy 'C' structure TLink_: the antennae and
    /// channel array are stored in global (heap) memory. The class TLink
    /// provides a totally 'managed' version of this type.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TLink_OLD
    {
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int eWhich;  //0

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CALL_SZ)]
        public string call2;  //1

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.BNDCDE_SZ)]
        public string bndcde;  //2

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nNumAnts;  //3

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nNumChan;  //4

        public IntPtr aAntsPtr;  //5

        public IntPtr aChanPtr;  //6

        //-----------------------------------------------------------------

        public const int NUM_COLUMNS = 7;

        //-----------------------------------------------------------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tLink"></param>
        /// <returns></returns>
        public TLink_OLD(TLink tLink)
        {
            if (tLink == null)
            {
                return;
            }

            // Scalars.
            eWhich = (int)tLink.eWhich;
            call2 = tLink.call2;
            bndcde = tLink.bndcde;
            nNumAnts = tLink.nNumAnts;
            nNumChan = tLink.nNumChan;

            // int[].
            aAntsPtr = Arrays.CopyArrayToGlobalMemory<int>(tLink.aAnts);

            aChanPtr = Arrays.CopyArrayToGlobalMemory<int>(tLink.aChan);

        }

        /// <summary>
        /// This static method converts TLink[] objects to TLink_OLD[] objects.
        /// </summary>
        /// <param name="tArray"></param>
        /// <returns></returns>
        public static TLink_OLD[] ConvertArray(TLink[] tArray)
        {
            TLink_OLD[] tArray_OLD = null;

            if (tArray == null)
            {
                return null;
            }
            else if (tArray.Length == 0)
            {
                return null;
            }

            tArray_OLD = new TLink_OLD[tArray.Length];

            for (int i = 0; i < tArray.Length; i++)
            {
                tArray_OLD[i] = new TLink_OLD(tArray[i]);
            }

            return tArray_OLD;
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

            sb.Append("\n\n===== TLink object =====");
            sb.Append("\neWhich = " + eWhich);
            sb.Append("\ncall2 = " + call2);
            sb.Append("\nbndcde = " + bndcde);
            sb.Append("\nnNumAnts = " + nNumAnts);
            sb.Append("\naAntsPtr = " + aAntsPtr.ToString("x16"));
            sb.Append("\nnNumChan = " + nNumChan);
            sb.Append("\naChanPtr = " + aChanPtr.ToString("x16"));

            return sb.ToString();
        }

        /// <summary>
        /// This method allocates a contiguous block of global (heap) memory
        /// sufficient to hold a prescribed TLink_OLD[] array object, copies in
        /// the elements of the TLink_OLD[] array and returns an IntPtr giving the
        /// block's start address in global memory.
        /// </summary>
        /// <param name="tArray"></param>
        /// <returns></returns>
        public static IntPtr CopyArrayToGlobalMemory(TLink_OLD[] tArray)
        {
            IntPtr intPtr = IntPtr.Zero;

            if (tArray == null)
            {
                Log2.e("\nArrays.CopyArrayToGlobalMemory(): ERROR: tArray is null");
                return intPtr;
            }
            else if (tArray.Length == 0)
            {
                Log2.e("\nArrays.CopyArrayToGlobalMemory(): ERROR: tArray has zero Length");
                return intPtr;
            }

            int objectSize = Marshal.SizeOf<TLink_OLD>();
            int objectArraySize = objectSize * tArray.Length;

            intPtr = Marshal.AllocHGlobal(objectArraySize);

            IntPtr offset;
            for (int i = 0; i < tArray.Length; i++)
            {
                offset = intPtr + i * objectSize;
                Marshal.StructureToPtr(tArray[i], offset, true);
            }

            return intPtr;
        }





    }
}

```
