# Documented File: UserInfoData.cs
**Repository Path:** `_DataStructures\UserInfoData.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates a pentad of user data comprising process 
    /// ID, MICS user ID, operator code, project code and the date/time 
    /// it was added.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class UserInfoData
    {
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int pid;
        public MicsUsers micsUser;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 7)]
        public string oper;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string pCode;
        [MarshalAsAttribute(UnmanagedType.I8)]
        public Int64 whenadded;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public UserInfoData()
        {
            pid = 0;
            micsUser = null;
            oper = null;
            pCode = null;
            whenadded = 0;
        }

        /// <summary>
        /// This method returns the date/time that the user was added encoded 
        /// as seconds past the epoch 1st Jan. 1970 00:00:00 UTC.
        /// </summary>
        /// <remarks>
        /// <b>Note:</b> this method is a recoding of a legacy 'C' subroutine
        /// that used the Unix/Linux/Java epoch, 1st Jan. 1970 00:00:00 UTC. 
        /// <para></para>
        /// .NET date/time methods uses a different epoch, namely 0001-01-01T00:00:00.
        /// </remarks>
        /// <returns></returns>
        public DateTime WhenAddedDateTime()
        {
            // Unix timestamp is seconds past epoch 1st Jan. 1970 00:00:00 UTC
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds((double)whenadded);
            return dtDateTime;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n");
            sb.AppendLine("pid =       " + pid);
            sb.AppendLine("micsUser.micsid =       " + micsUser.micsid);
            sb.AppendLine("micsUser.ultrixid =     " + micsUser.ultrixid);
            sb.AppendLine("micsUser.permissions0 = " + micsUser.permissions0);
            sb.AppendLine("micsUser.permissions1 = " + micsUser.permissions1);
            sb.AppendLine("micsUser.password =     " + micsUser.password);
            sb.AppendLine("oper =      " + oper);
            sb.AppendLine("pcode =     " + pCode);
            sb.AppendLine("whenadded = " + whenadded);
            return sb.ToString();
        }
    }
}

```
