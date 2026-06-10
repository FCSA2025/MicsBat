# Documented File: Arrays.cs
**Repository Path:** `_NewLib\Arrays.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// Provides convenience methods for creating and initializing array objects.
    /// </summary>
    public class Arrays
    {
        /// <summary>
        /// When an array of objects is created using a 'new' operation only the array 'infrastructure'
        /// itself is instantiated - all of its elements are null. This abstract method creates an array
        /// of objects and also populates it with elements instanciated using the default constructor
        /// for element class.
        /// </summary>
        /// <typeparam name="T"> - the prescribed class type</typeparam>
        /// <param name="length"> - number of array elements required.</param>
        /// <returns>an array populated with instanciated objects.</returns>
        public static T[] CreateArrayUsingDefaultElementConstructor<T>(int length) where T : new()
        {
            T[] array = new T[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = new T();
            }
            return array;
        }

        /// <summary>
        /// When an array of objects is created using a 'new' operation only the array 'infrastructure'
        /// itself is instantiated - all of its elements are null. This abstract method creates an array
        /// of objects and also populates it with elements instanciated using the default constructor
        /// for element class.
        /// </summary>
        /// <typeparam name="T"> - the prescribed class type</typeparam>
        /// <param name="length"> - number of array elements required.</param>
        /// /// <param name="fillValue"> - every element is set to this value.</param>
        /// <returns>an array populated with instanciated objects.</returns>
        public static T[] CreateAndFillArray<T>(int length, T fillValue) where T : new()
        {
            T[] array = new T[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = fillValue;
            }
            return array;
        }

        /// <summary>
        /// Returns a string array whose elements are all equal to a prescribed 'fill' string.
        /// </summary>
        /// <param name="length"> - Required length of array.</param>
        /// <param name="fillValue"> - the 'fill' string to be used for all elements.</param>
        /// <returns> - array of string whose elements are set to a common string value.</returns>
        public static string[] CreateFillStringArray(int length, string fillValue)
        {
            string[] array = new string[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = fillValue;
            }
            return array;
        }

        /// <summary>
        /// Returns a string containing the numeric contents of a 2-D array of doubles. 
        /// This is primarily intended for logging and debugging purposes.
        /// </summary>
        /// <param name="title"> - a prescribed informative title.</param>
        /// <param name="array"> - array of doubles to be written to string.</param>
        /// <returns> - string containing the numeric contents of a double[].</returns>
        public static string doubleArray2DtoString(string title, double[,] array)
        {
            // array[M, N]
            int M = array.GetLength(0);
            int N = array.GetLength(1);

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n");
            sb.Append(title);
            sb.Append(String.Format(" double[{0}, {1}]", M, N));
            for (int m = 0; m < M; m++)
            {
                sb.Append("\r\n");
                for (int n = 0; n < N; n++)
                {
                    sb.Append(array[m, n]);
                    sb.Append("   ");
                }
            }
            sb.Append("\r\n");

            return sb.ToString();
        }

        /// <summary>
        /// Returns an array of length N+1 comprising the elements of a prescribed array of 
        /// length N and with a prescribed value for its (N+1)th element.
        /// </summary>
        /// <typeparam name="T"> - prescribed instance of a class primitive type.</typeparam>
        /// <param name="arrayT"> - array of length N on entry and N+1 on exit.</param>
        /// <param name="element"> - element to be appended.</param>
        public static void AppendElement<T>(ref T[] arrayT, T element)
        {
            List<T> listT = new List<T>(arrayT);

            listT.Add(element);

            arrayT = listT.ToArray();
        }

        /// <summary>
        /// Converts an array of abstract type to a string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ToString<T>(T[] array, string name)
        {
            string nRet = "\r\nArray " + name + " is null";

            if (array != null)
            {

                StringBuilder sb = new StringBuilder();

                if (array.Length == 0)
                {
                    sb.Append("\r\nArray " + name + " has zero length.");
                }
                else
                {
                    sb.Append("\r\nArray " + name + " has length = " + array.Length);
                    for (int i = 0; i < array.Length; i++)
                    {
                        string value;
                        if (array[i] == null)
                        {
                            value = "null";
                        }
                        else
                        {
                            value = "|" + array[i].ToString() + "|";
                        }
                        sb.Append("\r\n" + "     " + name + "[" + i + "] = " + value);
                    }
                    nRet = sb.ToString();
                }
            }

            return nRet;
        }

        // It is assumed that type <T> is blittable.
        public static IntPtr CopyArrayToGlobalMemory<T>(T[] tArray)
        {
            IntPtr intPtr = IntPtr.Zero;

            if (tArray == null)
            {
                Log2.e("\nArrays.CopyArrayToGlobalMemory(): ERROR: tArray is null");
                return intPtr;
            }

            int objectSize = Marshal.SizeOf<T>();

            if (tArray.Length == 0)
            {
                // Instantiate an IntPtr that can contain one object even
                // though this should never be accessed.
                Log2.w("\nArrays.CopyArrayToGlobalMemory(): WARNING: tArray has zero Length");
                intPtr = Marshal.AllocHGlobal(objectSize);
                return intPtr;
            }

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
