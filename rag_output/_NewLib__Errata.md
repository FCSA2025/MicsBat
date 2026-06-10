# Documented File: Errata.cs
**Repository Path:** `_NewLib\Errata.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// This class provides a home for any static methods that perform small-scale
    /// corrections of errata in fetched MDB data or intermediate data.
    /// </summary>
    public class Errata
    {
        /// <summary>
        /// This method returns a string corresponding to the errata-corrected province/state code
        /// for a prescribed TS site call sign; the extant province/state code is provided as input
        /// and is the returned value when the call sign is not 'in the errata list'.
        /// </summary>
        /// <param name="call1"></param>
        /// <param name="extantProvince"></param>
        /// <returns></returns>
        public static string CorrectProvinceTS(string call1, string extantProvince)
        {
            string correctProvince = "";

            switch (call1)
            {
                case "CFQ810":
                    correctProvince = "NY";
                    break;
                case "CHG519":
                    correctProvince = "SP";
                    break;
                case "CKO427":
                case "CZJ410":
                case "CZJ411":
                case "CZJ412":
                case "VEF439":
                    correctProvince = "AO";
                    break;
                case "VXG855":
                    correctProvince = "AK";
                    break;
                default:
                    correctProvince = extantProvince;
                    break;
            }

            return correctProvince;
        }






    }
}

```
