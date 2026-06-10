# Documented File: TpKeyhole.cs
**Repository Path:** `TpRunTsip 20231124\TpKeyhole.cs`
**Primary Layer:** `TpRunTsip 20231124`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace TpRunTsip
{
    /// <summary>
    /// This class has just one method, WithinRange(), that helps with
    /// the 'keyhole' calculations.
    /// </summary>
    public class TpKeyhole
    {
        /// <summary>
        /// This method returns true if the absolute value of 'value' is less than 
        /// or equal to a boundary threshold.  
        /// </summary>
        /// <param name="value"> - value to be be tested.</param>
        /// <param name="boundary"> - threshold value.</param>
        /// <returns></returns>
        public static bool WithinRange(double value, double boundary)
        {
            return (Math.Abs(value) <= boundary);
        }


    }
}

```
