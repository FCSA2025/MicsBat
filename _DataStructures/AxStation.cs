using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates station information: name, latitude, longitude, 
    /// elevation above datum and antenna height above ground level.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class AxStation
    {
        // IMPORTANT!
        // =========
        // The following qty. 4 member values mirror the 'native' 'C' structure
        // axStation, defined in axStr.h
        //
        // The order of appearance of these qty. 4 fields MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.LPStr, SizeConst = 33)]
        public string name;
        [MarshalAsAttribute(UnmanagedType.Struct)]
        public AxLocation LL;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double elevM;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double antHtM;

        //----------------------------------------------------------------------

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public AxStation()
        {
            LL = new AxLocation();
            Initialize();
        }

        /// <summary>
        /// This method initializes all numeric field values to zero
        /// and sets all strings to the empty string.
        /// </summary>
        /// <param name=""></param>
        public void Initialize()
        {
            name = "";
            elevM = 0.0;
            antHtM = 0.0;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\n=== AxStation ===");
            sb.Append("\n     name   = " + name);
            sb.Append("\n     elevM  = " + elevM);
            sb.Append("\n     antHtM = " + antHtM);
            sb.Append("\n     LL     = " + LL.ToString());

            return sb.ToString();
        }


    }
}
