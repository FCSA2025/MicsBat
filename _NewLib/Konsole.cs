using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// Provides methods for writing to native codes' <b>stdout</b> stream.
    /// </summary>
    public class Konsole
    {
        [DllImport("_APItest.dll", CharSet = CharSet.Ansi)]
        private static extern void API_printf(string s);
        [DllImport("_APItest.dll", CharSet = CharSet.Ansi)]
        private static extern void API_fflushStdOut();

        /// <summary>
        /// Call native code's fflush(stdout).
        /// </summary>
        public void Flush()
        {
            API_fflushStdOut();
        }

        /// <summary>
        /// Call native codes' printf(s).
        /// </summary>
        /// <param name="s">string to print.</param>
        public static void Write(string s)
        {
            // String s could contain the character '%' that will cause the native
            // code printf() function to throw a runtime exception with little 
            // useful information.
            // So ... we need to escape every '%' as "%%".
            s = s.Replace("%", "%%");

            API_printf(s);
        }

        /// <summary>
        /// Calls native codes' printf("\r\n")
        /// </summary>
        public static void WriteLine()
        {
            API_printf("\r\n");
        }

        /// <summary>
        /// Calls native codes' printf("%s\r\n", s)
        /// </summary>
        /// <param name="s"></param>
        public static void WriteLine(string s)
        {
            string p = s + "\r\n";
            API_printf(p);
        }
    }
}
