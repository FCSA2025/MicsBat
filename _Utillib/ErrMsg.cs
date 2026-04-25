using _Configuration;
using _DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace _Utillib
{
    /// <summary>
    /// Provides methods to write fully-assembled error messages
    /// to Console.Out. The caller provides an error ID number and one or more
    /// inline message strings. The error ID number is used to lookup a 'standard'
    /// error message contained in the array ErrorMessages.Table[].
    /// </summary>
    public class ErrMsg
    {
#if PINVOKE
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int utPrintMessage(int errorNumber);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int utPrintMessage(int errorNumber, string s1, string s2, string s3);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int utErrMessage(int errorNumber);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int utErrMessage(int errorNumber, string s);

        public static void UtErrMessage_NATIVE(int errorNumber)
        {
            utErrMessage(errorNumber);
        }

        public static void UtErrMessage_NATIVE(int errorNumber, string errorMessage)
        {
            utErrMessage(errorNumber, errorMessage);
        }

        public static int UtPrintMessage_NATIVE(int errorNumber)
        {
            return utPrintMessage(errorNumber);
        }

        public static int UtPrintMessage_NATIVE(int errorNumber, string s1, string s2, string s3)
        {
            return utPrintMessage(errorNumber, s1, s2, s3);
        }
#endif

        //------------------------------------------------------------------------
        private static TextWriter mTW = Console.Out;

        /// <summary>
        /// This method sets the output stream that the error messages
        /// are sent to if not explicitely prescribed in a call to UtErrMessage().
        /// The default is Console.Out.
        /// </summary>
        /// <param name="tw"> - a TextWriter object that writes to a file.</param>
        public static void SetDefaultOutputStream(TextWriter tw)
        {
            if (tw != null)
            {
                mTW = tw;
            }
        }

        /// <summary>
        /// This method resets the output stream that error messages are sent
        /// to Console.Out.
        /// </summary>
        public static void ResetDefaultOutputStream()
        {
            mTW = Console.Out;
        }


        /// <summary>
        /// Writes a fully-assembled error message, with GenUtil.GetUserMess() 
        /// appended, to Console.Out
        /// </summary>
        /// <param name="errorNumber"> - the error number to be processed.</param>
        /// <param name="paramArray"> - user-defined inline error messages.</param>
        public static int UtErrMessage(int errorNumber, params string[] paramArray)
        {
            // Write fully-assembled error message to stdout.
            mTW.Write(AssembleErrorMessage(errorNumber, paramArray));

            string str = GenUtil.GetUserMess();
            if (!String.IsNullOrWhiteSpace(str) && (str != "(null)"))
            {
                mTW.Write("\r\n>>{0}\r\n", str);
            }

            return 0;
        }

        /// <summary>
        /// Writes a fully-assembled error message to Console.Out
        /// </summary>
        /// <param name="errorNumber"> - the error number to be processed.</param>
        /// <param name="paramArray"> - user-defined inline error messages.</param>
        public static int UtPrintMessage(int errorNumber, params string[] paramArray)
        {
            try
            { // Write fully-assembled error message to stdout.
                string str = AssembleErrorMessage(errorNumber, paramArray);
                mTW.WriteLine(str);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UtPrintMessage1:" + ex.Message);
            }
            return 0;
        }

        /// <summary>
        /// Writes a fully-assembled error message to a prescribed output stream.
        /// </summary>
        /// <param name="errorNumber"> - the error number to be processed.</param>
        /// <param name="param"> - user-defined inline error message.</param>
        /// <param name="tW"> - caller prescribed TextWriter object to write to.</param>
        public static int UtPrintMessage(int errorNumber, string param, TextWriter tW)
        {
            try { // Write fully-assembled error message to prescribed TextWriter object.
                string[] paramArray = new string[1];
                paramArray[0] = param;
                string str = AssembleErrorMessage(errorNumber, paramArray);
                tW.WriteLine(str);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UtPrintMessage2:" + ex.Message);
            }
            return 0;
        }

        /// <summary>
        /// Writes a fully-assembled error message to a prescribed TextWriter stream.
        /// </summary>
        /// <param name="tw"> - the prescribed TextWriter object.</param>
        /// <param name="errorNumber"> - the error number to be processed.</param>
        /// <param name="paramArray"> - user-defined inline error messages.</param>
        public static int UtPrintMessage(TextWriter tw, int errorNumber, params string[] paramArray)
        {
            try { // Write fully-assembled error message to stdout.
                string str = AssembleErrorMessage(errorNumber, paramArray);
                tw.WriteLine(str);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UtPrintMessage3:" + ex.Message);
            }
            return 0;
        }

        /// <summary>
        /// Returns a string comprising a fully-assembled error message for prescribed
        /// error number and user-defined inline error message strings.
        /// </summary>
        /// <param name="errorNumber"></param>
        /// <param name="paramArray"></param>
        /// <returns></returns>
        private static string AssembleErrorMessage(int errorNumber, params string[] paramArray)
        {
            string messbuff = "";
            int argc;                /* argument count */

            string message = null;

            for (int i = 0; i < Error.Table.Length; i++)
            {
                if (Error.Table[i].Number == errorNumber)
                {
                    message = Error.Table[i].Message;
                    break;
                }
            }  // for (int i = 0; i < ErrorMessages.Table.Length; i++)

            /* Check to see if message number was found */
            if (message == null)
            {
                /* Error number not found */
                messbuff = String.Format("Undefined error message {0}", errorNumber);
            }
            else  //if (message == null)
            {
                // Message number was found. Assemble error line, with parameters.
                // There can be 0, 1 or 2 arguments. Process accordingly.
                string pattern = @"\{[0-9]\}";
                argc = Regex.Matches(message, pattern).Count;
                if (argc == 0 || argc > Constant.EM_ARG_MAX)
                {
                    /* No parameters or too many parameters. Simply print
                     * error message
                     */
                    messbuff = message;
                }
                else  // if (argc == 0 || argc > Constant.EM_ARG_MAX)
                {
                    // We have 1, 2 or 3 arguments.
                    if (argc == 1)
                    {
                        messbuff = String.Format(message, paramArray[0]);
                    }
                    else if (argc == 2)
                    {
                        messbuff = String.Format(message, paramArray[0], paramArray[1]);
                    }
                    else if (argc == 3)
                    {
                        messbuff = String.Format(message, paramArray[0], paramArray[1], paramArray[2]);
                    }

                }  // if (argc == 0 || argc > Constant.EM_ARG_MAX)
            } // if (message == null)

            // Return the fully-assembled error message.
            return messbuff;
        }

    }
}
