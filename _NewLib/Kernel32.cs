using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using _Configuration;
using System.Security.AccessControl;
using System.Security.Principal;

namespace _NewLib
{
    /// <summary>
    /// Provides methods for accessing information from the native system runtime 
    /// library <b>kernel32.dll</b> using calls from C# managed code.
    /// </summary>
    public class Kernel32
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern bool GetProcessTimes(IntPtr hProcess,
    out System.Runtime.InteropServices.ComTypes.FILETIME lpCreationTime,
    out System.Runtime.InteropServices.ComTypes.FILETIME lpExitTime,
    out System.Runtime.InteropServices.ComTypes.FILETIME lpKernelTime,
    out System.Runtime.InteropServices.ComTypes.FILETIME lpUserTime);

        [DllImport("msvcrt.dll", CharSet = CharSet.Ansi)]
        private static extern Int64 time([In, Out] IntPtr timerIntPtr);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, [Out] StringBuilder lpBuffer, uint nSize, string[] Arguments);
        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static uint GetErrorID()
        {
            return GetLastError();
        }

        /// <summary>
        /// Returns the Windows error message for a prescribed error ID, as retreived
        /// by a call to GetErrorID(). 
        /// </summary>
        /// <param name="messageID"> - Windows error ID number.</param>
        /// <returns>Windows error message string.</returns>
        public static string GetErrorMessage(uint messageID)
        {
            string errorMessage = "";
            StringBuilder sb = new StringBuilder(Constant.WINDOWS_MESSAGE_SZ);

            uint nRet = FormatMessage(Constant.FORMAT_MESSAGE_FROM_SYSTEM, IntPtr.Zero, messageID, 0, sb,
                                        Constant.WINDOWS_MESSAGE_SZ, null);

            if (nRet > 0)
            {
                errorMessage = sb.ToString();
            }
            return errorMessage;
        }

        /// <summary>
        /// Calls native kernel32.dll function: time()
        /// </summary>
        /// <param name="timer"></param>
        /// <returns>time()</returns>
        public static Int64 Time(ref Int64 timer)
        {
            //Marshall timer for [In, Out]
            IntPtr timerIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(timer));
            Marshal.WriteInt64(timerIntPtr, timer);
            //Native call.
            Int64 _time = time(timerIntPtr);
            //Reverse marshal timer.
            timer = Marshal.ReadInt64(timerIntPtr);
            return _time;
        }

        /// <summary>
        /// Returns a .NET DateTime object populated with values from a
        /// System.Runtime.InteropServices.ComTypes.FILETIME object.
        /// </summary>
        /// <param name="fileTime"></param>
        /// <returns>DateTime object</returns>
        public static DateTime FiletimeToDateTime(System.Runtime.InteropServices.ComTypes.FILETIME fileTime)
        {
            //NB! uint conversion must be done on both fields before ulong conversion
            ulong hFT2 = unchecked((((ulong)(uint)fileTime.dwHighDateTime) << 32) | (uint)fileTime.dwLowDateTime);
            return DateTime.FromFileTimeUtc((long)hFT2);
        }

        /// <summary>
        /// Returns a .NET TimeSpan object populated with values from a
        /// System.Runtime.InteropServices.ComTypes.FILETIME object.
        /// </summary>
        /// <param name="fileTime"></param>
        /// <returns>DateTime object</returns>
        public static TimeSpan FiletimeToTimeSpan(System.Runtime.InteropServices.ComTypes.FILETIME fileTime)
        {
            //NB! uint conversion must be done on both fields before ulong conversion
            ulong hFT2 = unchecked((((ulong)(uint)fileTime.dwHighDateTime) << 32) | (uint)fileTime.dwLowDateTime);
            return TimeSpan.FromTicks((long)hFT2);
        }

        /// <summary>
        /// This method returns an annotated multi-line string that provides the current access
        /// rules for the prescribed MutexSecurity object.
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        public static string SecurityToString(MutexSecurity security)
        {
            if (security == null) return "Kernel32.SecurityToString(): called with null argument.\n";

            StringBuilder sb = new StringBuilder();

            sb.Append("\nCurrent access rules for MutexSecurity object:\n");

            foreach (MutexAccessRule ar in
                security.GetAccessRules(true, true, typeof(NTAccount)))
            {
                sb.Append("\n        User: " + ar.IdentityReference);
                sb.Append("\n        Type: " + ar.AccessControlType);
                sb.Append("\n      Rights: " + ar.MutexRights);
                sb.Append("\n");
            }

            return sb.ToString();
        }

    } //class

} //namespace
