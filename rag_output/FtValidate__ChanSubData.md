# Documented File: ChanSubData.cs
**Repository Path:** `FtValidate\ChanSubData.cs`
**Primary Layer:** `FtValidate`
**Namespace:** `FtValidate`

## Source Code Representation
```csharp
using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FtValidate
{
    /// <summary>
    /// Encapsulates the information needed to keep track of all channels associated with 
    /// an antenna.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class ChanSubData
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 2)]
        public string cmd;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CALL_SZ)]
        public string call1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CALL_SZ)]
        public string call2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.BNDCDE_SZ)]
        public string bndcde;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.CHID_SZ)]
        public string chid;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short tx1;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short tx2;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short rx1;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short rx2;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short rx3;

        //-----------------------------------------------------------------

        /// <summary>
        /// This method returns the values of this object's fields as a single
        /// formatted string comprising annotated, multi-line text.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n");
            sb.AppendLine("cmd =    " + cmd);
            sb.AppendLine("call1 =  " + call1);
            sb.AppendLine("call2 =  " + call2);
            sb.AppendLine("bndcde = " + bndcde);
            sb.AppendLine("chid =   " + chid);
            sb.AppendLine("tx1 =    " + tx1);
            sb.AppendLine("tx2 =    " + tx2);
            sb.AppendLine("rx1 =    " + rx1);
            sb.AppendLine("rx2 =    " + rx2);
            sb.AppendLine("rx3 =    " + rx3);
            return sb.ToString();
        }

        /// <summary>
        /// This method returns the values of this object's fields as a single
        /// formatted string comprising annotated, single-line text.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string ToStringSingleLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("cmd = " + cmd);
            sb.Append(", call1 = " + call1);
            sb.Append(", call2 = " + call2);
            sb.Append(", bndcde = " + bndcde);
            sb.Append(", chid = " + chid);
            sb.Append(", tx1 = " + tx1);
            sb.Append(", tx2 = " + tx2);
            sb.Append(", rx1 = " + rx1);
            sb.Append(", rx2 = " + rx2);
            sb.Append(", rx3 = " + rx3);
            return sb.ToString();
        }


    }
}

```
