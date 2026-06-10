# Documented File: TM.cs
**Repository Path:** `_DataStructures\TM.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates the date and time to the nearest second.
    /// </summary>
    public class TM
    {
        public int tm_sec;   // seconds after the minute - [0, 60] including leap second

        public int tm_min;   // minutes after the hour - [0, 59]

        public int tm_hour;  // hours since midnight - [0, 23]

        public int tm_mday;  // day of the month - [1, 31]

        public int tm_mon;   // months since January - [0, 11]

        public int tm_year;  // years since 1900

        public int tm_wday;  // days since Sunday - [0, 6]

        public int tm_yday;  // days since January 1 - [0, 365]

        public int tm_isdst; // daylight savings time flag

        //-------------------------------------------------------------------------------

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            string str = String.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}",
                tm_year, tm_mon, tm_mday, tm_hour, tm_min, tm_sec);

            return str;
        }






    }
}

```
