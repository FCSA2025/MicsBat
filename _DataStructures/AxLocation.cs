using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A library that provides classes whose fields are either isomorphic with specific
/// database tables or are used to encapsulate various subsets of intermediate data to simplify the code. 
/// </summary>
namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates the dataset for a pair of latitude
    /// and longitude coordinates expressed in degrees, minutes and
    /// seconds of arc and the directional 'sense' for latitude 
    /// ("N" or "S") and longitude ("E" or "W").
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class AxLocation
    {
        // IMPORTANT!
        // =========
        // The following qty. 10 member values mirror the 'native' 'C' structure
        // axLocation, defined in axStr.h
        //
        // The order of appearance of these qty. 10 fields MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int latDeg;  //0
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int latMin;  //1
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int latSec;  //2
        [MarshalAsAttribute(UnmanagedType.LPStr, SizeConst = 2)]
        public string latSens; //3
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double latSeconds;  //4
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int longDeg;  //5
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int longMin;  //6
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int longSec;  //7
        [MarshalAsAttribute(UnmanagedType.LPStr, SizeConst = 2)]
        public string longSens;  //8
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double longSeconds; //9

        //-----------------------------------------------------------------------

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public AxLocation()
        {
            Initialize();
        }

        /// <summary>
        /// This method initializes all numeric field values to zero
        /// and sets all strings to the empty string.
        /// </summary>
        /// <param name=""></param>
        public void Initialize()
        {
            latDeg = 0;
            latMin = 0;
            latSec = 0;
            latSens = "";
            latSeconds = 0.0;
            longDeg = 0;
            longMin = 0;
            longSec = 0;
            longSens = "";
            longSeconds = 0.0;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\n----- AxLocation -----");
            sb.Append("\n     latDeg = " + latDeg);  //0
            sb.Append("\n     latMin = " + latMin);  //1
            sb.Append("\n     latSec = " + latSec);  //2
            sb.Append("\n     latSens = " + latSens); //3
            sb.Append("\n     latSeconds = " + latSeconds);  //4
            sb.Append("\n     longDeg = " + longDeg);  //5
            sb.Append("\n     longMin = " + longMin);  //6
            sb.Append("\n     longSec = " + longSec);  //7
            sb.Append("\n     longSens = " + longSens);  //8
            sb.Append("\n     longSeconds = " + longSeconds); //9

            return sb.ToString();
        }


    }
}
