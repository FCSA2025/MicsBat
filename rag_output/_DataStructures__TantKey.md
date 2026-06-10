# Documented File: TantKey.cs
**Repository Path:** `_DataStructures\TantKey.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates the dataset required to locate the highest
    /// antenna at the other end of a link.
    /// </summary>
    public class TantKey
    {
        public string call1;
        public string call2;
        public string bndcde;
        public int anum;
        public double aht;

        //--------------------------------------------------------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public TantKey()
        {
            call1 = "";
            call2 = "";
            bndcde = "";
            anum = 0;
            aht = 0.0;
        }

        /// <summary>
        /// This method returns an annotated, formatted, string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("TantKey: call1 = {0};  call2 = {1};  bndcde = {2};  anum = {3};  aht = {4:F6}",
                                            call1, call2, bndcde, anum, aht);
        }





    }
}

```
