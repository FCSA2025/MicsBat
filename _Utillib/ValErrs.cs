using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace _Utillib
{
    /// <summary>
    /// Provides methods to cache (accumulate) and print out validation errors and warning
    /// and informational messages; these are stored separately in expandable lists.
    /// These are writen out when the ValErrors() method is called, and the cache is automatically reset (cleared).
    /// </summary>
    public class ValErrs
    {
#if PINVOKE
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int valErrs();
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int addmess([In] string cFormat, [In] string cKey, [In] string cType);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int addmess([In] string cFormat, [In] string cKey, [In] string cType, [In] __arglist);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr makeKeyLine([In] string call1, [In] string call2, [In] string bndcde, [In] int anum, [In] string chid);

        public static int AddMess_NATIVE(string cFormat, string cKey, string cType, params string[] pList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nAddMessage_NATIVE(): ");
            sb.Append(cFormat);
            sb.Append(" | ");
            sb.Append(cKey);
            sb.Append(" | ");
            sb.Append(cType);
            foreach (string str in pList)
            {
                sb.Append(" | ");
                sb.Append(str);
            }
            sb.Append(" | ");
            //...Log2.v(sb.ToString());

            int nRet = -666;
            switch (pList.Length)
            {
                case 0:
                    addmess(cFormat, cKey, cType);
                    break;
                case 1:
                    addmess(cFormat, cKey, cType, __arglist(pList[0]));
                    break;
                case 2:
                    addmess(cFormat, cKey, cType, __arglist(pList[0], pList[1]));
                    break;
            }
            return nRet;
        }

        public static string MakeKeyLine_NATIVE(string call1, string call2, string bndcde, int anum, string chid)
        {
            return Marshal.PtrToStringAnsi(makeKeyLine(call1, call2, bndcde, anum, chid));
        }

        /// <summary>
        /// This method prints all the errors, then warnings, in key order; the arrays are then reset to null.
        /// </summary>
        public static void ValErrors_NATIVE()
        {
            valErrs();
        }
#endif

        //-------------------------------------------------------------------------------------------------------

        /// <summary>
        /// This class encapsulates a 'qualified message' comprising a message string, its type 
        /// and the database table 'key' that it is associated with.
        /// </summary>
        private class TMessage
        {
            public string cMessage;
            public string cKey;                 /*	This size is taken from KEYLINESZ */
            public char cType;

            /// <summary>
            /// This is a constructor that sets the values cKey, cMess and cType.
            /// </summary>
            /// <param name="cKey"></param>
            /// <param name="cMess"></param>
            /// <param name="cType"></param>
            /// <returns></returns>
            public TMessage(string cKey, string cMess, char cType)
            {
                this.cKey = cKey;
                this.cMessage = cMess;
                this.cType = cType;
            }
        }

        private static List<TMessage> atInfos = new List<TMessage>();

        private static List<TMessage> atWarnings = new List<TMessage>();

        private static List<TMessage> atErrors = new List<TMessage>();


        //-------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Writes out the currently accumulated error, warning and informational messages, 
        /// in that sequence. Messages are ordered according to their 'key'. The three
        /// individual message caches are then cleared.
        /// </summary>
        public static void ValErrors()
        {
            /*	Print errors, then warnings  */
            if (atErrors.Count > 0)
            {
                Console.Write("\r\n                *********** ERRORS ***********\r\n\r\n");
                ValPrint(atErrors);
                atErrors.Clear();
            }

            if (atWarnings.Count > 0)
            {
                Console.Write("\r\n                ----------- WARNINGS -----------\r\n\r\n");
                ValPrint(atWarnings);
                atWarnings.Clear();
            }

            if (atInfos.Count > 0)
            {
                Console.Write("\r\n                ---- INFORMATIONAL MESSAGES ----\r\n\r\n");
                ValPrint(atInfos);
                atInfos.Clear();
            }

            Console.Write("\r\n");

            Console.Out.Flush();

            return;
        }

        /// <summary>
        /// Writes out currently accumulated messages from a prescribed message cache
        /// (there are 3 distinct message caches: 'errors', 'warnings' and 'informational').
        /// </summary>
        /// <param name="aMessList"> - one of atErrors, atWarnings or atInfos.</param>
        private static void ValPrint(List<TMessage> aMessList)
        {
            //...Log2.v("\n\nValErrs.ValPrint(): Entry");

            string cLastKey = "";

            foreach (TMessage tMessage in aMessList)
            {

                if (!tMessage.cKey.Equals(cLastKey))
                {
                    /*	Different key. */
                    cLastKey = tMessage.cKey;
                    Console.Write("\r\n{0}\r\n", cLastKey);
                }
                Console.Write("{0}\r\n", tMessage.cMessage);
                //...Log2.v("\r\nValErrs.ValPrint(): " + tMessage.cMessage);
            }

            //...Log2.v("\n\nValErrs.ValPrint(): Exit");
        }

        /// <summary>
        /// Accumulates messages in one of the caches maintained by this class as prescribed by
        /// the cType parameter that should be one of "E", "W" or "I" (errors, warning or information).
        /// </summary>
        /// <param name="cFormat"> - this is the leading message and can contain the 'C' style format descriptors %c, %s, %d or %f.</param>
        /// <param name="cKey"> - this string is used to provide a specific context for the message (e.g. the 'key' values for DB record).</param>
        /// <param name="cType"> - one of "E", "W" or "I".</param>
        /// <param name="pList"> - (optional) one or more additional messages.</param>
        public static void AddMess(string cFormat, string cKey, string cType, params string[] pList)
        {
            string cBuff = "";

            if (pList != null)
            {
                string cSharpFormat = ConvertCFormatDescriptors(cFormat);

                //...Log2.v("\n\nValErrs.AddMess(): cSharpFormat = " + cSharpFormat);
                //...Log2.v("\nValErrs.AddMess(): pList = " + Strings.ArrayOfStringsToCSV(pList));

                cBuff = String.Format(cSharpFormat, pList);
            }

            //...Log2.v("\nVarErrs.AddMess(): cBuff = " + cBuff);

            //...Log2.v(String.Format("\nValErrs.AddMess(): List counts {0},  {1}, {2}", atErrors.Count, atWarnings.Count, atInfos.Count));

            /*	Insert the message into the appropriate list. */
            switch (cType[0])
            {
                case 'E':
                    atErrors.Add(new TMessage(cKey, cBuff, cType[0]));
                    break;
                case 'W':
                    atWarnings.Add(new TMessage(cKey, cBuff, cType[0]));
                    break;
                case 'I':
                    atInfos.Add(new TMessage(cKey, cBuff, cType[0]));
                    break;
                default:
                    Log2.e("\r\nValErrs.AddMess(): Error: unknown type: " + cType);
                    break;
            }

            return;
        }

        /// <summary>
        /// Returns a string that converts 'C' style format descriptors (%c, %s, %d or %f)
        /// into C# format descriptors than can be successfully processsed in a call
        /// to String.Format().
        /// </summary>
        /// <param name="formatStr"> - string conatining 'C' style format descriptors.</param>
        /// <returns> - converted string.</returns>
        public static string ConvertCFormatDescriptors(string formatStr)
        {
            string cSharpFormatStr = formatStr.Trim();
            const char bell = (char)0x0007;

            // First, parse the string and convert any %c, %s, %d, %f etc to the audible bell character.
            string pattern = @"%(c|s|d|f)";
            cSharpFormatStr = Regex.Replace(cSharpFormatStr, pattern, bell.ToString());

            // We now have to deal with C descriptors like %.2f , %11.2f etc.
            // First, find every match with the following pattern, interpret its %f format
            // and generate a C# descriptor equivalent.
            pattern = @"%(\d*)\.(\d+)f";
            MatchCollection matchColl = Regex.Matches(cSharpFormatStr, pattern);
            List<string> cSharpDesc = new List<string>();

            foreach (Match m in matchColl)
            {
                string preDecimal = m.Groups[1].ToString();
                string postDecimal = m.Groups[2].ToString();

                int numDecPlaces = Convert.ToInt32(postDecimal);

                string cSD = ":#." + Strings.RepeatedChar('0', numDecPlaces);

                if (!String.IsNullOrEmpty(preDecimal))
                {
                    cSD = "," + preDecimal + cSD;
                }

                cSharpDesc.Add(cSD);
            }

            // Now replace every floating point descriptor match with the ETX character.
            const char eTX = (char)4;
            cSharpFormatStr = Regex.Replace(cSharpFormatStr, pattern, eTX.ToString());

            // Finally, parse the string character by character, substituting the bell char
            // with the substitute C# descriptor.
            int pos = 0;
            int fPatternIndex = 0;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cSharpFormatStr.Length; i++)
            {
                char c = cSharpFormatStr[i];
                if (c == bell)
                {
                    sb.Append("{" + pos + "}");
                    pos++;
                }
                else if (c == eTX)
                {
                    sb.Append("{" + pos + cSharpDesc[fPatternIndex] + "}");
                    fPatternIndex++;
                    pos++;
                }
                else
                {
                    sb.Append(c);
                }
            }
            cSharpFormatStr = sb.ToString();

            return sb.ToString();
        }

        //public static string ConvertCFormatDescriptors(string formatStr)
        //{
        //    string cSharpFormatStr = formatStr.Trim();
        //    const char bell = (char)0x0007;

        //    // First, parse the string and convert any %s, %d, %f etc to the audible bell character.
        //    cSharpFormatStr = Regex.Replace(cSharpFormatStr, @"%\w", bell.ToString());

        //    // Next, replace the tokens by C# format descriptors, e.g. {0}, {1} etc
        //    int pos = 0;
        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < cSharpFormatStr.Length; i++)
        //    {
        //        char c = cSharpFormatStr[i];
        //        if (c == bell)
        //        {
        //            sb.Append("{" + pos + "}");
        //            pos++;
        //        }
        //        else
        //        {
        //            sb.Append(c);
        //        }
        //    }
        //    return sb.ToString();
        //}

        /// <summary>
        /// Returns a string that provides a context-specific  'key line' for use with AddMess().
        /// This 'key line' is a composite of the values call1, call2, bndcde, anum or chid.
        /// </summary>
        /// <param name="call1"> - call1.</param>
        /// <param name="call2"> - call2.</param>
        /// <param name="bndcde"> - band code.</param>
        /// <param name="anum"> - antenna number.</param>
        /// <param name="chid"> - channel ID number.</param>
        /// <returns> - 'key line' string.</returns>
        public static string MakeKeyLine(string call1, string call2, string bndcde, int anum, string chid)
        {
            //...Log2.v("\n\nValErrs.MakeKeyLine(): Entry");

            string glbKeyLine;
            glbKeyLine = call1;

            if (!String.IsNullOrWhiteSpace(call2))
            {
                glbKeyLine += "->" + call2;
                if (!String.IsNullOrWhiteSpace(bndcde))
                {
                    glbKeyLine += " Band:" + bndcde;
                }
                if (anum != 0)
                {
                    glbKeyLine += " Anum:" + anum.ToString();
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(chid))
                    {
                        glbKeyLine += " Chid:" + chid;
                    }
                }

            }

            //...Log2.v("\n\nValErrs.MakeKeyLine(): Exit: glbKeyLine = |" + glbKeyLine + "|");
            return glbKeyLine;
        }






    }
}
