# Documented File: Geo_UTM_Xfer.cs
**Repository Path:** `_OHloss\Geo_UTM_Xfer.cs`
**Primary Layer:** `_OHloss`
**Namespace:** `_OHloss`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _OHloss
{/// <summary>
 /// This class has fields that are isomorphic to the legacy C/C++ struct geo_utm_xfer and
 /// is used when calling methods in the CTE library.
 /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct Geo_UTM_Xfer
    {
        // IMPORTANT!
        // =========
        // The following qty. 6 member fields correspond to the fields
        // of the legacy C/C++ struct geo_utm_xfer.
        // The order of appearance of these qty. 6 members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double latitude;                     // positive north in degrees        
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double longitude;                    // positive west in degrees         
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double easting;                      // kilometers                       
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double northing;                     // kilometers                       
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short zone;                          // utm zone number                  
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short datum;                         // NAD_83_DATUM or NAD_27_DATUM     

        //---------------------------------------------------------------------------

        public const int NUM_COLUMNS = 6;

        //---------------------------------------------------------------------------

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the member fields.
        /// </summary>
        /// <returns> - string comprising all field names and their values.</returns>
        override
        public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\nlatitude = " + latitude);
            sb.Append("\nlongitude = " + longitude);
            sb.Append("\neasting = " + easting);
            sb.Append("\nnorthing = " + northing);
            sb.Append("\nzone = " + zone);
            sb.Append("\ndatum = " + datum);

            return sb.ToString();
        }

        //-------------------------------------------------------------------------







    }
}

```
