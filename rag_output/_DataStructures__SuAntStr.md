# Documented File: SuAntStr.cs
**Repository Path:** `_DataStructures\SuAntStr.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using _Configuration;
using _DataStructures;
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
    /// This class encapsulates the dataset of an antenna code, a SuAnte object and
    /// an array of SuAntd objects.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SuAntStr
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ACODE_SZ)]
        public string acAntCode;
        [MarshalAsAttribute(UnmanagedType.Struct)]
        public SuAnte acAnt;
        [MarshalAsAttribute(UnmanagedType.SysUInt)]
        public SuAntd[] acDscPtr;   //struct suAntd_ *acDsc; 				

        //-----------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 3;

        public enum Init { ALLOCATED, UNALLOCATED }

        //----------------------------------------------------------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public SuAntStr()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public SuAntStr(Init initMode)
        {
            if (initMode == Init.UNALLOCATED)
            {
                //Do nothing: objects and pointers are not instantiated.
            }
            else if (initMode == Init.ALLOCATED)
            {
                acAntCode = "";
                acAnt = new SuAnte();
                acDscPtr = null;
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
            sb.Append("\r\nacAntCode     = " + acAntCode);
            sb.Append("\r\n***** SuAnte acAnt *****");
            sb.Append(acAnt);
            sb.Append("\r\n***** SuAntd[] acDscPtr *****");
            if (acDscPtr == null)
            {
                sb.Append("\r\nacDscPtr == null");
            }
            else
            {
                foreach (SuAntd suAntd in acDscPtr)
                {
                    sb.Append(suAntd.ToString());
                }
            }
            return sb.ToString();
        }

        /// <summary>
		/// This method returns a 'deep copy' of the current object; changes to the 
        /// field values of this object do not cause changes to the deep copy.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public SuAntStr DeepCopy()
        {
            // A deep copy object has deep copies of the nonstatic fields of the current object.
            //    - If a field is a value type then a bit-by-bit copy of the field is created. 
            //    - If a field is a reference type then a deep copy of the referred object is created.

            SuAntStr suAntStr = new SuAntStr(Init.UNALLOCATED);

            // Strings are immutable.
            suAntStr.acAntCode = this.acAntCode;

            suAntStr.acAnt = this.acAnt.DeepCopy();

            if (this.acDscPtr == null)
            {
                suAntStr.acDscPtr = null;
            }
            else
            {
                // We have a bona fide array of SuAntds; we need a deep copy of each array element.

                // First, create a new SuAntd[].
                suAntStr.acDscPtr = new SuAntd[this.acDscPtr.Length];

                // Now create deep copies, element-by-element.
                for (int i = 0; i < this.acDscPtr.Length; i++)
                {
                    suAntStr.acDscPtr[i] = this.acDscPtr[i].DeepCopy();
                }
            }

            return suAntStr;
        }


    }
}


```
