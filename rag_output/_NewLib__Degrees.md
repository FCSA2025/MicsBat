# Documented File: Degrees.cs
**Repository Path:** `_NewLib\Degrees.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// Provides sin cos, tan, asin, acos, atan and atan2d functions for 
    /// values in degrees. (The standard class library Math uses radians.)
    /// </summary>
    public class Degrees
    {
        /// <summary>
        /// sin(x); x in degrees.
        /// </summary>
        /// <param name="X"></param>
        /// <returns>Math.Sin(Constant.DEG_TO_RAD * (X))</returns>
        public static double SinD(double X) { return Math.Sin(Constant.DEG_TO_RAD * (X)); }

        /// <summary>
        /// cos(x); x in degrees.
        /// </summary>
        /// <param name="X"></param>
        /// <returns>Math.Cos(Constant.DEG_TO_RAD * (X))</returns>
        public static double CosD(double X) { return Math.Cos(Constant.DEG_TO_RAD * (X)); }

        /// <summary>
        /// tan(x); x in degrees.
        /// </summary>
        /// <param name="X"></param>
        /// <returns>Math.Tan(Constant.DEG_TO_RAD * (X))</returns>
        public static double TanD(double X) { return Math.Tan(Constant.DEG_TO_RAD * (X)); }

        /// <summary>
        /// arcsine(x) converted to degrees.
        /// </summary>
        /// <param name="X"></param>
        /// <returns>Math.Asin(X) * Constant.RAD_TO_DEG</returns>
        public static double ASinD(double X) { return Math.Asin(X) * Constant.RAD_TO_DEG; }

        /// <summary>
        /// arccosine(x) converted to degrees.
        /// </summary>
        /// <param name="X"></param>
        /// <returns>Math.Acos(X) * Constant.RAD_TO_DEG</returns>
        public static double ACosD(double X) { return Math.Acos(X) * Constant.RAD_TO_DEG; }

        /// <summary>
        /// arctan(x) converted to degrees.
        /// </summary>
        /// <param name="X"></param>
        /// <returns>Math.Atan(X) * Constant.RAD_TO_DEG</returns>
        public static double ATanD(double X) { return Math.Atan(X) * Constant.RAD_TO_DEG; }

        /// <summary>
        /// Atan2(y, x) converted to degrees (+ve x-axis is zero degrees).
        /// </summary>
        /// <param name="Y">y coordinate.</param>
        /// <param name="X">x coordinate.</param>
        /// <returns>Math.Atan2(Y, X) * Constant.RAD_TO_DEG</returns>
        public static double ATan2D(double Y, double X) { return Math.Atan2(Y, X) * Constant.RAD_TO_DEG; }
    }
}

```
