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
    /// This class encapsultes the dataset of an antenna code, a SuAnte object 
    /// and array of discriminations.
    /// The class is a clone of the legacy 'C' structure suAntStruct: the discriminations
    /// array is stored in global (heap) memory. The class SuAntStr
    /// provides a totally 'managed' version of this type.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SuAntStr_OLD
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ACODE_SZ)]
        public string acAntCode;
        [MarshalAsAttribute(UnmanagedType.Struct)]
        public SuAnte acAnt;
        [MarshalAsAttribute(UnmanagedType.SysUInt)]
        public IntPtr acDsc;   //struct suAntd_ *acDsc; 				

        //-----------------------------------------------------------------------

        public enum Init { ALLOCATED, UNALLOCATED }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initMode"></param>
        /// <returns></returns>
        public SuAntStr_OLD(Init initMode)
        {
            if (initMode == Init.UNALLOCATED)
            {
                //Do nothing: objects and pointers are not instantiated.
            }
            else if (initMode == Init.ALLOCATED)
            {
                acAnt = new SuAnte();

                SuAntd suAntd = new SuAntd();
                IntPtr suAntdPtr = Marshal.AllocHGlobal(Marshal.SizeOf(suAntd));
                Marshal.StructureToPtr(suAntd, suAntdPtr, false);
                acDsc = suAntdPtr;
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
            sb.Append("\r\n***** struct suAntd_ *acDsc *****");
            if (acDsc == IntPtr.Zero)
            {
                sb.Append("\r\nacDsc == IntPtr.Zero");
            }
            else
            {
                // We need to reverse marshal the pointer acDSc.
                SuAntd suAntd = new SuAntd();
                Marshal.PtrToStructure(acDsc, suAntd);
                sb.Append(suAntd.ToString());
            }
            return sb.ToString();
        }

    }
}
