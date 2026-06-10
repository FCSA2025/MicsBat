# Documented File: Profile_Xfer.cs
**Repository Path:** `_OHloss\Profile_Xfer.cs`
**Primary Layer:** `_OHloss`
**Namespace:** `_OHloss`

## Source Code Representation
```csharp
﻿using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _OHloss
{
    /// <summary>
    /// This class has fields that are isomorphic to the legacy C/C++ struct profile_xfer; is used when calling
    /// methods in the CTE library.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct Profile_Xfer
    {
        // IMPORTANT!
        // =========
        // The following qty. 13 member fields correspond to the fields
        // of the legacy C/C++ struct profile_xfer.
        // The order of appearance of these qty. 13 'column' members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        // input variables 
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double lat_1;          // 1st site latitude - positive north - NAD 83 
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double lng_1;          // 1st site longitude - positive west - NAD 83
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double lat_2;          // 2nd site latitude - positive north - NAD 83 
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double lng_2;          // 2nd site longitude - positive west - NAD 83
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double dist_inc;       // distance between points - meters 

        // output profile 
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int num_points;     // number of points in the distance-elevation arrays
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int error_code;

        [MarshalAsAttribute(UnmanagedType.SysUInt)]
        public IntPtr dist_array;
        [MarshalAsAttribute(UnmanagedType.SysUInt)]
        public IntPtr elev_array;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double last_dist;   // the path length-distance of the last point read in the case of an error 

        public fixed byte map_name[Constant.MAX_PATH];  // the DTED file name on which the error occurred
        public fixed byte cded50_map_name[Constant.MAX_PATH];

        [MarshalAsAttribute(UnmanagedType.U1)]
        public byte cded50_files_used;

        //---------------------------------------------------------------------------

        public const int NUM_COLUMNS = 13;

        //---------------------------------------------------------------------------


        /// <summary>
        /// Returns a string that comprises all the field names and their current
        /// values.
        /// </summary>
        /// <returns> - string comprising all field names and their values.</returns>
        override
        public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\nlat_1 = " + lat_1);
            sb.Append("\nlng_1 = " + lng_1);
            sb.Append("\nlat_2 = " + lat_2);
            sb.Append("\nlng_2 = " + lng_2);
            sb.Append("\ndist_inc = " + dist_inc);
            sb.Append("\nnum_points = " + num_points);
            sb.Append("\nerror_code = " + error_code);

            if ((dist_array != IntPtr.Zero) && (num_points > 0))
            {
                double[] distArray = GetDistArray();
                for (int i = 0; i < distArray.Length; i++)
                {
                    sb.Append("\ndist_array[" + i + "] = " + distArray[i]);
                }
            }
            else
            {
                sb.Append("\ndist_array = null or has zero elements.");
            }

            if ((elev_array != IntPtr.Zero) && (num_points > 0))
            {
                double[] elevArray = GetElevArray();
                for (int i = 0; i < elevArray.Length; i++)
                {
                    sb.Append("\nelev_array[" + i + "] = " + elevArray[i]);
                }
            }
            else
            {
                sb.Append("\nelev_array = null or has zero elements.");
            }

            sb.Append("\nlast_dist = " + last_dist);

            fixed (byte* b = map_name)
            {
                sb.Append("\nmap_name = ");
                for (int i = 0; i < Constant.MAX_PATH; i++)
                {
                    if (b[i] == 0) break;
                    sb.Append(Convert.ToChar(b[i]));
                }
            }

            fixed (byte* b = cded50_map_name)
            {
                sb.Append("\ncded50_map_name = ");
                for (int i = 0; i < Constant.MAX_PATH; i++)
                {
                    if (b[i] == 0) break;
                    sb.Append(Convert.ToChar(b[i]));
                }
            }

            sb.Append("\ncded50_files_used = " + cded50_files_used);

            return sb.ToString();
        }

        /// <summary>
        /// Returns a double[] corresponding to the distance values in unmanaged memory 
        /// 'pointed to' by the IntPtr dist_array.
        /// </summary>
        /// <returns> - array of distances.</returns>
        public double[] GetDistArray()
        {
            double[] distArray = null;

            if ((dist_array != IntPtr.Zero) && (num_points > 0))
            {
                distArray = new double[num_points];

                Marshal.Copy(dist_array, distArray, 0, num_points);
            }

            return distArray;
        }

        /// <summary>
        /// Returns a double[] corresponding to the elevation values in unmanaged memory 
        /// 'pointed to' by the IntPtr elev_array.
        /// </summary>
        /// <returns> - array of elevations.</returns>
        public double[] GetElevArray()
        {
            double[] elevArray = null;

            if ((elev_array != IntPtr.Zero) && (num_points > 0))
            {
                elevArray = new double[num_points];

                Marshal.Copy(elev_array, elevArray, 0, num_points);
            }

            return elevArray;
        }

        /*
                lat_1
                lng_1
                lat_2
                lng_2
                dist_inc
                num_points
                error_code
                dist_array
                elev_array
                last_dist
                map_name
                cded50_map_name
                cded50_files_used 
        */



    }
}

```
