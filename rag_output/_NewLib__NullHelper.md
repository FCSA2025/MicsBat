# Documented File: NullHelper.cs
**Repository Path:** `_NewLib\NullHelper.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _Configuration;

namespace _NewLib
{
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
    /// Provides convenience methods for creating, initializing and using ODBC null indicators 
    /// ('nullInds').
    /// </summary>
    public class NullHelper
    {
        public enum ColumnStatus
        {
            NULL, NOT_NULL
        }

        private const int NUM_BYTES = sizeof(SQLLEN);

        /// <summary>
        /// Creates an SQLLENPTR[] in global (unmanaged) memory with each IntPtr element 
        /// instantiated and pointing to an individual SQLLEN (int64) value encoding 
        /// either an ODBC NULL or NOT_NULL.
        /// </summary>
        /// <param name="nDim"> - prescribed length of array.</param>
        /// <param name="columnStatus"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns>SQLLENPTR[]</returns>
        public static SQLLENPTR[] CreateArrayOfSQLLENPTRinGlobalMemory(int nDim, ColumnStatus columnStatus)
        {
            SQLLENPTR[] nullIndPtr = new SQLLENPTR[nDim];

            SQLLEN fillValue;
            if (columnStatus == ColumnStatus.NULL)
            {
                fillValue = Constant.DB_NULL;
            }
            else
            {
                fillValue = Constant.DB_NOT_NULL;
            }

            for (int i = 0; i < nDim; i++)
            {
                nullIndPtr[i] = Marshal.AllocHGlobal(NUM_BYTES);
                Marshal.WriteInt64(nullIndPtr[i], fillValue);
            }
            return nullIndPtr;
        }

        /// <summary>
        /// Creates an SQLLENPTR[] in global (unmanaged) memory with each IntPtr element 
        /// instantiated and pointing to an individual SQLLEN (int64) value encoding from
        /// a prescribed SQLLEN array.
        /// </summary>
        /// <param name="nullInds"> - array of nullInds.</param>
        /// <returns>SQLLENPTR[]</returns>
        public static SQLLENPTR[] CreateArrayOfSQLLENPTRinGlobalMemory(SQLLEN[] nullInds)
        {
            SQLLENPTR[] nullIndPtr = new SQLLENPTR[nullInds.Length];
            const int NUM_BYTES = sizeof(SQLLEN);
            for (int i = 0; i < nullInds.Length; i++)
            {
                nullIndPtr[i] = Marshal.AllocHGlobal(NUM_BYTES);
                Marshal.WriteInt64(nullIndPtr[i], nullInds[i]);
            }
            return nullIndPtr;
        }

        /// <summary>
        /// Creates a SQLLEN[] array of nullInds with all elements initialized to either
        /// DB_NULL or DB_NOT_NULL.
        /// </summary>
        /// <param name="numCols"> - prescribed length of array.</param>
        /// <param name="status"> - one of NULL or NOT_NULL.</param>
        /// <returns>SQLLEN[]</returns>
        public static SQLLEN[] CreateArrayOfNullInd(int numCols, ColumnStatus status)
        {
            SQLLEN[] nullInds = new SQLLEN[numCols];

            SQLLEN value = SQLLEN.MinValue;
            switch (status)
            {
                case ColumnStatus.NULL:
                    value = Constant.DB_NULL;
                    break;
                case ColumnStatus.NOT_NULL:
                    value = Constant.DB_NOT_NULL;
                    break;
                default:
                    break;
            }
            for (int i = 0; i < numCols; i++)
            {
                nullInds[i] = value;
            }

            return nullInds;
        }

        /// <summary>
        /// Copy elements from a source SQLLEN[] array into a target SQLLEN[] array
        /// with prescribed element offset and number of elements to copy.
        /// </summary>
        /// <param name="source"> - SQLLEN[] to read from.</param>
        /// <param name="sourceOffset"> - index of first source element to copy.</param>
        /// <param name="target"> - SQLLEN[] to write to.</param>
        /// <param name="targetOffset"> - index of first target element to write to.</param>
        /// <param name="numElementsToCopy"> - number of elements to copy.</param>
        public static void Copy(SQLLEN[] source, int sourceOffset, SQLLEN[] target, int targetOffset, int numElementsToCopy)
        {
            try
            {
                for (int i = 0; i < numElementsToCopy; i++)
                {
                    target[i + targetOffset] = source[i + sourceOffset];
                }
            }
            catch (Exception e)
            {
                string str = "\r\nNullHelper.Copy(): ERROR: " + e.Message;
                //...Log2.v(str);
                Application.Exit(str);
            }
        }

        /// <summary>
        /// Fills an existing SQLLEN[] array with a common value.
        /// </summary>
        /// <param name="nullIndArray"> - SQLLEN[].</param>
        /// <param name="fillValue"> - prescribed common fill value.</param>
        public static void FillArray(ref SQLLEN[] nullIndArray, SQLLEN fillValue)
        {
            for (int i = 0; i < nullIndArray.Length; i++)
            {
                nullIndArray[i] = fillValue;
            }
        }

        /// <summary>
        /// If nullInd = DB_NULL return zero else return value of x.
        /// </summary>
        /// <param name="x"> - prescribed float value.</param>
        /// <param name="nullInd"> - nullInd value.</param>
        /// <returns>zero or x.</returns>
        public static float ReturnZeroIfNullElseValue(float x, SQLLEN nullInd)
        {
            float retVal = 0.0f;
            if (nullInd != Constant.DB_NULL)
            {
                retVal = x;
            }
            return retVal;
        }

        // NOTE: It is assumed that each element of the SQLPOINTER[] points to a previously 
        //       allocated block of global memoery sufficient to hold an SQLLEN value.

        /// <summary>
        /// Copies an SQLLEN[] array nullInds into the global (unmanaged) memory pointed
        /// to by an SQLPOINTER[] (IntPtr[]) array.
        /// </summary>
        /// <remarks>
        /// It is assumed that each element of the SQLPOINTER[] points to a previously 
        /// allocated block of global memory sufficient to hold an SQLLEN value.
        /// </remarks>
        /// <param name="nullInds"> - array of nullInd values.</param>
        /// <param name="sqlPointers"> - array of IntPtr[]</param>
        public static void CopyNullIndsToSQLPOINTERArray(SQLLEN[] nullInds, SQLPOINTER[] sqlPointers)
        {
            for (int i = 0; i < nullInds.Length; i++)
            {
                Marshal.WriteInt64(sqlPointers[i], 0, nullInds[i]);
            }
        }

        /// <summary>
        /// This method returns an IntPtr to a contiguous block of global
        /// (heap) memory into which a prescribed SQLLEN[] has been copied,
        /// for use in subsequent ODBC operations.
        /// </summary>
        /// <param name="tArray"></param>
        /// <returns></returns>
        public static IntPtr CopyNullIndArrayIntoGlobalMemory(SQLLEN[] tArray)
        {
            IntPtr intPtr = IntPtr.Zero;

            if (tArray == null)
            {
                return intPtr;
            }
            else if (tArray.Length == 0)
            {
                return intPtr;
            }

            int objectSize = Marshal.SizeOf<SQLLEN>();
            int objectArraySize = objectSize * tArray.Length;

            intPtr = Marshal.AllocHGlobal(objectArraySize);

            Marshal.Copy(tArray, 0, intPtr, tArray.Length);

            return intPtr;
        }

        /// <summary>
        /// This method returns an IntPtr to a contiguous block of global
        /// (heap) memory into which a prescribed non-jagged SQLLEN[][] has been copied,
        /// for use in subsequent ODBC operations.
        /// </summary>
        /// <param name="tArray"></param>
        /// <returns></returns>
        public static IntPtr CopyNullInd2dArrayIntoGlobalMemory(SQLLEN[][] tArray)
        {
            IntPtr intPtr = IntPtr.Zero;

            if (tArray == null)
            {
                Log2.e("\nNullHelper.CopyNullInd2dArrayIntoGlobalMemory(): ERROR: tArray[][] is null.");
                return intPtr;
            }
            else if (tArray.Length == 0)
            {
                Log2.e("\nNullHelper.CopyNullInd2dArrayIntoGlobalMemory(): ERROR: tArray[] has Length of zero.");
                return intPtr;
            }

            int objectSize = Marshal.SizeOf<SQLLEN>();

            // tArray[M][N].
            int N = tArray[0].Length;
            int M = tArray.Length;

            int object2dArraySize = objectSize * N * M;

            intPtr = Marshal.AllocHGlobal(object2dArraySize);

            IntPtr stride;
            for (int m = 0; m < M; m++)
            {
                stride = intPtr + m * N * objectSize;
                for (int n = 0; n < N; n++)
                {
                    Marshal.Copy(tArray[m], 0, stride, N);
                }
            }

            return intPtr;
        }

        /// <summary>
        /// This method returns DB_NULL if a prescribed string is null, empty
        /// or contains only whitespace; otherwise it returns DB_NOT_NULL.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="nullInd"></param>
        public static void BlankNull(string f, out SQLLEN nullInd)
        {
            // 'out' requirement;
            nullInd = Constant.DB_NULL;

            if (!String.IsNullOrWhiteSpace(f))
            {
                nullInd = Constant.DB_NOT_NULL;
            }
        }

        /// <summary>
        /// This method returns DB_NULL if a prescribed integer is zero;
        /// otherwise it returns DB_NOT_NULL.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="nullInd"></param>
        public static void ZeroNull(int f, out SQLLEN nullInd)
        {
            nullInd = Constant.DB_NULL;

            if (f != 0)
            {
                nullInd = Constant.DB_NOT_NULL;
            }
        }

        /// <summary>
        /// This method returns DB_NULL if a prescribed float is zero;
        /// otherwise it returns DB_NOT_NULL.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="nullInd"></param>
        public static void ZeroNull(float f, out SQLLEN nullInd)
        {
            nullInd = Constant.DB_NULL;

            if (f != 0.0f)
            {
                nullInd = Constant.DB_NOT_NULL;
            }
        }

        /// <summary>
        /// This method reads nullInd values from prescribed locations
        /// in global (heap) memory and returns them as a SQLLEN[].
        /// </summary>
        /// <param name="nullIndPtrs"></param>
        /// <returns></returns>
        public static SQLLEN[] CopyFromBuffers(SQLPOINTER[] nullIndPtrs)
        {
            SQLLEN[] nullInds = new SQLLEN[nullIndPtrs.Length];

            for (int i = 0; i < nullIndPtrs.Length; i++)
            {
                nullInds[i] = Marshal.ReadInt64(nullIndPtrs[i]);
            }

            return nullInds;
        }


    }
}

```
