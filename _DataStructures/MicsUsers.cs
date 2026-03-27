using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _Configuration;
using System.Runtime.InteropServices;

namespace _DataStructures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MicsUsers
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ID_SZ)]
        public string micsid;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.ID_SZ)]
        public string ultrixid;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int permissions0;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int permissions1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string password;
    }
}
