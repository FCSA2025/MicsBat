# Documented File: CtxStruct.cs
**Repository Path:** `_DataStructures\CtxStruct.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using _Configuration;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates an array of Ctx data points and its associated metadata.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CtxStruct
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CTXTRAFTX_SZ)]
        public string ctxtraftx = "";

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CTXTRAFRX_SZ)]
        public string ctxtrafrx = "";

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CTXEQPT_SZ)]
        public string ctxeqpt = "";

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALCTYPE_SZ)]
        public string calcType = "";

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double rqco;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int numPts;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int useCounter;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 2 * Constant.MAX_CTX_PTS)]
        public float[,] ctxPts = new float[Constant.MAX_CTX_PTS, 2];

        //---------------------------------------------------------------------------

        public const int NUM_COLUMNS = 8;

        //---------------------------------------------------------------------------

        public const int CTXTRAFTX_SZ = 7;
        public const int CTXTRAFRX_SZ = 7;
        public const int CTXEQPT_SZ = 9;
        public const int CALCTYPE_SZ = 4;

        //---------------------------------------------------------------------------

        public const int CTXTRAFTX = 0;
        public const int CTXTRAFRX = 1;
        public const int CTXEQPT = 2;
        public const int CALCTYPE = 3;
        public const int RQCO = 4;
        public const int NUMPTS = 5;
        public const int USECOUNTER = 6;
        public const int CTXPTS = 7;

        //--------------------------------------------------------------------------

        private static int count = 0;
        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            count++;
            sb.Append(String.Format("\n\n======= CtxStruct # {0} =======", count));

            sb.Append("\nctxtraftx = " + ctxtraftx);
            sb.Append("\nctxtrafrx = " + ctxtrafrx);
            sb.Append("\nctxeqpt = " + ctxeqpt);
            sb.Append("\ncalcType = " + calcType);
            sb.Append("\nrqco = " + rqco);
            sb.Append("\nnumPts = " + numPts);
            sb.Append("\nuseCounter = " + useCounter);

            for (int i = 0; i <= numPts; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    sb.Append(String.Format("\nctxPts[{0}, {1}] = {2}", i, j, ctxPts[i, j]));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// This method creates a deep-clone of this CtxStruct object.
        /// A new object is created and populated with the same member
        /// field values as this object; this ensures that no cross-object data
        /// corruption can occur.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public CtxStruct MakeDeepClone()
        {
            CtxStruct clone = new CtxStruct();

            clone.ctxtraftx = ctxtraftx;
            clone.ctxtrafrx = ctxtrafrx;
            clone.ctxeqpt = ctxeqpt;
            clone.calcType = calcType;
            clone.rqco = rqco;
            clone.numPts = numPts;
            clone.useCounter = useCounter;

            clone.ctxPts = new float[Constant.MAX_CTX_PTS, 2];
            for (int i = 0; i < Constant.MAX_CTX_PTS; i++)
            {
                clone.ctxPts[i, 0] = ctxPts[i, 0];
                clone.ctxPts[i, 1] = ctxPts[i, 1];
            }

            return clone;
        }


    }
}

```
