# Documented File: SuSatOper.cs
**Repository Path:** `_DataStructures\SuSatOper.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    public class SuSatOper
    {
        public string opercode;
        public string operator_name;
        public string date_added;

        //------------------------------------------------------------------------

        public const int NUM_COLUMNS = 3;

        //------------------------------------------------------------------------

        public const int OPERCODE = 0;
        public const int OPERATOR_NAME = 1;
        public const int DATE_ADDED = 2;

        public const int OPERCODE_SZ = 7;
        public const int OPERATOR_NAME_SZ = 41;
        public const int DATE_ADDED_SZ = Constant.DATE_SZ;

        //------------------------------------------------------------------------

        /// <summary>
        /// This is the default constructor.
        /// </summary>
        public SuSatOper()
        {
            opercode = "";
            operator_name = "";
            date_added = "";
        }

        //------------------------------------------------------------------------
        //------------------------------------------------------------------------
        //------------------------------------------------------------------------





    }
}

```
