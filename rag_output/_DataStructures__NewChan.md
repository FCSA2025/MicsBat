# Documented File: NewChan.cs
**Repository Path:** `_DataStructures\NewChan.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class has fields that are isomorphic with the legacy C/C++ <b>struct newChanStruct</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class NewChan
    {
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double rxfreq;              /* Receive frequency */
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double txfreq;              /* Transmit Frequency */
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CHID_SZ)]
        public string chid;                /* channel id */
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short chidInd;              /* channel id null ind */
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short rxInd;                /* rx frequency null ind */
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short txInd;                /* tx frequency null ind */
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.STATRX_SZ)]
        public string rxstatus;            /* rx status */
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.STATTX_SZ)]
        public string txstatus;            /* tx status */
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.POLTX_SZ)]
        public string poltx;               /* tx polarisation */
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.POLRX_SZ)]
        public string polrx;			    /* rx polarisation */

        //------------------------------------------------------------------------

        public const int NUM_CLUMNS = 10;

        //------------------------------------------------------------------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public NewChan()
        {
            Initialize();
        }

        /// <summary>
        /// This method initializes the value of all member fields.
        /// </summary>
        /// <param name=""></param>
        public void Initialize()
        {
            const string STRING_INIT_VAL = "";
            const short SHORT_INIT_VAL = 0;
            const double DOUBLE_INIT_VAL = 0.0;

            rxfreq = DOUBLE_INIT_VAL;
            txfreq = DOUBLE_INIT_VAL;
            chid = STRING_INIT_VAL;
            chidInd = SHORT_INIT_VAL;
            rxInd = SHORT_INIT_VAL;
            txInd = SHORT_INIT_VAL;
            rxstatus = STRING_INIT_VAL;
            txstatus = STRING_INIT_VAL;
            poltx = STRING_INIT_VAL;
            polrx = STRING_INIT_VAL;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <returns></returns>
        override
        public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\nrxfreq = " + rxfreq);
            sb.Append("\ntxfreq = " + txfreq);
            sb.Append("\nchid = " + chid);
            sb.Append("\nchidInd = " + chidInd);
            sb.Append("\nrxInd = " + rxInd);
            sb.Append("\ntxInd = " + txInd);
            sb.Append("\nrxstatus = " + rxstatus);
            sb.Append("\ntxstatus = " + txstatus);
            sb.Append("\npoltx = " + poltx);
            sb.Append("\npolrx = " + polrx);

            return sb.ToString();
        }

        /*
        rxfreq
        txfreq
        chid
        chidInd
        rxInd
        txInd
        rxstatus
        txstatus
        poltx
        polrx
        */

    }
}

```
