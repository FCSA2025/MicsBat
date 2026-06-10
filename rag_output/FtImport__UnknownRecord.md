# Documented File: UnknownRecord.cs
**Repository Path:** `FtImport\UnknownRecord.cs`
**Primary Layer:** `FtImport`
**Namespace:** `FtImport`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _Utillib;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtImport
{
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides a method that reports an error for a line in the
    /// PDF import text file that has not been 'qualified' as a valid record
    /// type and has been assigned the type 'UNKNOWN'.
    /// </summary>	
    public class UnknownRecord
    {
        //private static short mTEflag = Constant.CLEAR;
        private static string msgBuf;
        private static FtTitl titlStruct = new FtTitl();
        private static SQLLEN[] titlNulls = NullHelper.CreateArrayOfNullInd(FtTitl.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

        /// <summary>
        /// This method reports an error for a line in the
        /// PDF import text file that has not been 'qualified' as a valid record
        /// type and has been assigned the type 'UNKNOWN'.
        /// </summary>
        /// <param name="qualLine"> - the currently parsed line in the PDF import text file.</param>
        public static void Parse_UNKNOWN(QualLine qualLine)
        {
            //...Log2.v("\n\nTitleRecord.Parse_TT(): Entry: importOK = " + importOK);

            msgBuf = String.Format("Error - line #{0}, import TS RECORD TYPE field, invalid conversion\r\n", qualLine.LineNum);
            ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);

            return;
        }








    }
}


```
