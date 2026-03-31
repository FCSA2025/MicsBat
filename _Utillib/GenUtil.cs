using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Math;
using static _NewLib.Maths;

namespace _Utillib
{

    /// <summary>
    /// This classrovides 'utility' methods that have general application 
    /// throughout the MICS code.
    /// </summary>
    public class GenUtil
    {
#if PINVOKE
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr utTrimFld([In, Out] IntPtr iPtr);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int utCvtName([In] int tabType, [In] string buf, [In, Out] IntPtr newNameIntPtr);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern void utGetDateTime([In, Out] IntPtr dateValIntPtr, [In, Out] IntPtr timeValIntPtr);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int getSavedLicence(string cCall1, string cCall2, [In, Out] IntPtr cLicenceIntPtr, ulong cLicenceLen);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int addSavedLicence(string cCall1, string cCall2, string cLicence);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr safecopy([In, Out] IntPtr targetIntPtr, string source, int len);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern void setprojectcode(string projectCode);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr getprojectcode([In, Out] StringBuilder sb_cProj);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int seterr([In] [MarshalAs(UnmanagedType.AnsiBStr)] string s);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int seterr([In] string s1, [In] string s2);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int seterr([In] string s1, [In] string s2, [In] string s3);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int seterr([In] string s1, [In] string s2, [In] string s3, [In] string s4);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int seterr([In] string s1, [In] string s2, [In] string s3, [In] string s4, [In] string s5);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern void getUserMess([In, Out] StringBuilder sb);

        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern void reseterror();
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int seterror([In] int nErr, [In] string s1);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int seterror([In] int nErr, [In] string s1, [In] string s2);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int seterror([In] int nErr, [In] string s1, [In] string s2, [In] string s3);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int utTestBit([In, Out] ref int bitmap, int mapSz, int bitNum);

        public static int UtTestBit_NATIVE(ref int bitmap, int mapSz, int bitNum)
        {
            return utTestBit(ref bitmap, mapSz, bitNum);
        }

        public static int SetError_NATIVE(int nErr, string s1)
        {
            //...Log2.v("\r\nGenUtil.SetError(): " + nErr + "   " + s1);

            return seterror(nErr, s1);
        }

        public static int SetError_NATIVE(int nErr, string s1, string s2)
        {
            //...Log2.v("\r\nGenUtil.SetError(): " + nErr + "   " + s1 + "  " + s2);
            return seterror(nErr, s1, s2);
        }

        public static int SetError_NATIVE(int nErr, string s1, string s2, string s3)
        {
            //...Log2.v("\r\nGenUtil.SetError(): " + nErr + "   " + s1 + "  " + s2 + "  " + s3);
            return seterror(nErr, s1, s2, s3);
        }

        public static string GetUserMess_NATIVE()
        {
            StringBuilder sbUserMess = new StringBuilder(Constant.USERMESSAGE_SZ);
            //Native call.
            getUserMess(sbUserMess);
            return sbUserMess.ToString();
        }

        public static int SetErr_NATIVE(string cErrMess)
        {
            return seterr(cErrMess);
        }

        public static int SetErr_NATIVE(string s1, string s2)
        {
            return seterr(s1, s2);
        }
        public static int SetErr_NATIVE(string s1, string s2, string s3)
        {
            return seterr(s1, s2, s3);
        }
        public static int SetErr_NATIVE(string s1, string s2, string s3, string s4)
        {
            return seterr(s1, s2, s3, s4);
        }

        public static int SetErr_NATIVE(string s1, string s2, string s3, string s4, string s5)
        {
            return seterr(s1, s2, s3, s4, s5);
        }

        private static string GetProjectCode_NATIVE(out string cProj)
        {
            StringBuilder sb = new StringBuilder(Constant.PROJECTCODESTRING_SZ);
            sb.Append("anything");
            //Native call.
            IntPtr resultIntPtr = getprojectcode(sb);
            cProj = sb.ToString();
            return cProj;
        }

        public static int AddSavedLicence_NATIVE(string cCall1, string cCall2, string cLicence)
        {
            return addSavedLicence(cCall1, cCall2, cLicence);
        }

        public static int GetSavedLicence_NATIVE(string cCall1, string cCall2, out string cLicence, ulong cLicenceLen)
        {
            // Satisfy 'out' requirement.
            cLicence = null;

            int nRet = -666;
            //Make an ANSI string in unmanaged memory to pass cLicence to native code.
            IntPtr cLicenceIntPtr = Marshal.StringToHGlobalAnsi(cLicence);
            //Native call.
            nRet = getSavedLicence(cCall1, cCall2, cLicenceIntPtr, cLicenceLen);
            //Reverse marshal cLicence.
            cLicence = Marshal.PtrToStringAnsi(cLicenceIntPtr);
            return nRet;
        }

        public static string UtTrimFld_NATIVE(string inputStr)
        {
            IntPtr intPtr = utTrimFld(Marshal.StringToHGlobalAnsi(inputStr));
            return Marshal.PtrToStringAnsi(intPtr);
        }

        //Converts a displayable name to an internal table name
        public static int UtCvtName_NATIVE(int tabType, string buf, out string internalTableName)
        {
            //Allocate a block of global memory sufficient to receive the output string from the native call.
            IntPtr internalTableNameIntPtr = Marshal.AllocHGlobal(Constant.TABLE_NM_SZ);

            //Call the native function.
            int result = utCvtName(tabType, buf, internalTableNameIntPtr);

            //Marshal the string in unmanaged memory to make a new newValue string object.
            internalTableName = Marshal.PtrToStringAnsi(internalTableNameIntPtr);

            //...Log2.v("\r\nGenUtil.UtCvtName(): out internalTableName = " + internalTableName);

            return result;
        }

        private static void UtGetDateTime_NATIVE(ref string dateVal, ref string timeVal)
        {
            //...Log2.v("\n\nGenUtil.cs:  UtGetDateTime(): Entry");
            try
            {
                //Make a copies of the strings in unmanaged memory and get pointers.
                IntPtr dateValIntPtr = Marshal.StringToHGlobalAnsi(dateVal);
                //Make a copy of the string in unmanaged memory and get a pointer to it.
                IntPtr timeValIntPtr = Marshal.StringToHGlobalAnsi(timeVal);

                //Call the native function.
                //...Log2.v("\r\nGenUtil.cs:  UtGetDateTime(): before native call");
                utGetDateTime(dateValIntPtr, timeValIntPtr);
                //...Log2.v("\r\nGenUtil.cs:  UtGetDateTime(): after native call");

                //Reverse Marshal the managed strings.
                dateVal = Marshal.PtrToStringAnsi(dateValIntPtr);
                timeVal = Marshal.PtrToStringAnsi(timeValIntPtr);
            }
            catch (Exception e)
            {
                //...Log2.v("\n\n" + e.Message);
                //...Log2.v("\n\n" + e.StackTrace);
            }
            //...Log2.v("\n\nGenUtil.cs:  UtGetDateTime(): Exit");
        }

#endif

        //-----------------------------------------------------------------------------------------

        private static Mutex mFileGateMutex;
        private static string mStrPokBuffer;
        private static int mPos;
        private static bool mEndOfString = true;
        private static int count = 0;

        //-----------------------------------------------------------------------------------------



        /// <summary>
        /// Attempts to create a 'named' mutex and place a lock on it. If successful it returns 0; 
        /// if the mutex is already locked by another process the method returns a negative value.
        /// </summary>
        /// <param name="cFileName"> - prescribed 'name' of the Mutex.</param>
        /// <returns> - Constant.SUCCESS - this call succeeded </returns>
        public static int FileGate(string cFileName)
        {
            string cErrMess;

            //	We use a Mutex to serialize processes executing this section of code.
            try
            {
                mFileGateMutex = new Mutex(false, cFileName);
            }
            catch (Exception e)
            {
                //	Could not create the mutex
                Log2.e("\nGenUtil.FileGate(): ERROR: exception thrown attempting: new Mutex(false, cFileName) : " + e.Message);
                //seterr(cErrMess = make_err("filegate: Could not create the mutex for %s.", cFileName));
                cErrMess = String.Format("filegate: Could not create the mutex for {0} : reason = {1}", cFileName, e.Message);
                GenUtil.SetErr(cErrMess);
                return Error.COULD_NOT_CREATE_MUTEX;
            }

            // Check if the hold is up.
            // By prescribing a zero timeout the method does not block. It tests the state of the 
            // wait handle and returns immediately. 
            if (!mFileGateMutex.WaitOne(0))
            {
                //	The mutex is owned; we must exit.
                Log2.w("\nGenUtil.FileGate(): the Mutex is already owned by another process.");
                mFileGateMutex.Close();
                return Error.MUTEX_ALREADY_OWNED;
            }

            //...Log2.v("\nGenUtil.FileGate(): SUCCEEDED");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method releases the mutex attached to the current process. (Note that the 
        /// call argument is redundant.)
        /// </summary>
        /// <param name="cFileName"> - not used.</param>
        /// <returns></returns>
        public static int FileGateClose(string cFileName)
        {
            mFileGateMutex.ReleaseMutex();

            mFileGateMutex.Close();

            return 0;
        }


        /// <summary>
        /// This class encapsulates the triad {call1, call2, license}.
        /// </summary>
        private class TSavedLicence
        {
            private string mCall1;
            private string mCall2;
            private string mLicence;

            public string Call1
            {
                get { return mCall1; }
            }

            public string Call2
            {
                get { return mCall2; }
            }

            public string Licence
            {
                get { return mLicence; }
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="call1"></param>
            /// <param name="call2"></param>
            /// <param name="licence"></param>
            /// <returns></returns>
            public TSavedLicence(string call1, string call2, string licence)
            {
                mCall1 = call1;
                mCall2 = call2;
                mLicence = licence;
            }
        }

        private static StringBuilder mUserMess = new StringBuilder();
        private static int mUserError = 0;

        private static List<TSavedLicence> mLicenceList = new List<TSavedLicence>();

        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Appends caller-prescribed error number and textual information to a previously
        /// accumulated list of error information.
        /// </summary>
        /// <param name="nErr"> - prescribed error number.</param>
        /// <param name="cErrMess"> - one or more text strings describing an error.</param>
        public static void SetError(int nErr, params string[] cErrMess)
        {
            mUserMess = new StringBuilder();
            mUserError = nErr;

            if (cErrMess != null)
            {
                for (int i = 0; i < cErrMess.Length; i++)
                {
                    mUserMess.Append(cErrMess[i]);
                }
            }

            return;
        }

        /// <summary>
        /// Empties the accumulated list of error information from previous
        /// calls to SetError(), SetErr() and SetErrW().
        /// </summary>
        public static void ReSetError()
        {
            mUserMess.Clear();
            mUserError = 0;
        }

        /// <summary>
        /// Retrieves the current list of error information accumulated
        /// over previous calls to SetError(), SetErr() and SetErrW().
        /// </summary>
        /// <returns> - a string that concatenates the accumulated error information.</returns>
        public static string GetUserMess()
        {
            string result = "(null)";
            string message = mUserMess.ToString();

            if (!String.IsNullOrEmpty(message))
            {
                result = message;
            }

            return result;
        }

        /// <summary>
        /// Appends caller-prescribed textual error information to a previously
        /// accumulated list of error information.
        /// </summary>
        /// <param name="cErrMess"> - one or more text strings describing an error.</param>
        public static void SetErr(params string[] cErrMess)
        {
            mUserError = 0;

            if (cErrMess != null)
            {
                for (int i = 0; i < cErrMess.Length; i++)
                {
                    mUserMess.Append(cErrMess[i]);
                }
            }

            return;
        }

        /// <summary>
        /// Appends caller-prescribed textual error information and the most recent 
        /// Windows Win32 error message to a previously accumulated list of error information.
        /// </summary>
        /// <param name="cErrMess"> - one or more text strings describing an error.</param>
        public static void SetErrW(params string[] cErrMess)
        {
            mUserError = 0;

            if (cErrMess != null)
            {
                for (int i = 0; i < cErrMess.Length; i++)
                {
                    mUserMess.Append(cErrMess[i]);
                }
            }

            //	We now have the error message.  Next, add the Windows error message.
            //  First, get the Windows error ID for the last Win32 error.
            uint win32ErrorID = Kernel32.GetErrorID();
            string win32ErrorMessage = Kernel32.GetErrorMessage(win32ErrorID);
            mUserMess.Append(win32ErrorMessage);

            return;
        }

        /// <summary>
        /// Returns a new instance of a string having the same first N characters
        /// of a prescribed string.
        /// </summary>
        /// <param name="target"> - new string comprising first N chars of source string.</param>
        /// <param name="source"> - prescribed source string.</param>
        /// <param name="N"> - the number of leading characters to copy.</param>
        /// <returns> - string having the same first N characters.</returns>
        public static string SafeCopy(ref string target, string source, int N)
        {
            if (N > source.Length) N = source.Length;
            target = source.Substring(0, N);
            return target;
        }

        /// <summary>
        /// Appends licence data to a previously accumulated list of Licence data.
        /// </summary>
        /// <param name="cCall1"> - prescribed call1.</param>
        /// <param name="cCall2"> - prescribed call2.</param>
        /// <param name="cLicence"> - name/ID of licence to be appended to list.</param>
        public static void AddSavedLicence(string cCall1, string cCall2, string cLicence)
        {
            mLicenceList.Add(new TSavedLicence(cCall1, cCall2, cLicence));

            return;
        }

        /// <summary>
        /// Retrieves an instance of licence data from a previously accumulated list 
        /// of licence data that has prescribed call1 and call2.
        /// </summary>
        /// <param name="cCall1"> - prescribed call1.</param>
        /// <param name="cCall2"> - prescribed call2.</param>
        /// <param name="cLicence"> - retrieved instance of licence that has prescribed call1 and call2.</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - licence retrieval attempt was successful.</para>
        /// <para>- Constant.FAILURE - licence retrieval attempt failed.</para>
        public static int GetSavedLicence(string cCall1, string cCall2, out string cLicence)
        {
            // Satisfy 'out' requirement.
            cLicence = null;

            // Implicit in this code is the notion that from site call1 to site call2
            // there is only one licence. This seeme ill-conceived.

            int nInd;
            int nRet = Constant.FAILURE;

            for (nInd = 0; nInd < mLicenceList.Count; nInd++)
            {
                if (cCall1.Equals(mLicenceList[nInd].Call1) && cCall2.Equals(mLicenceList[nInd].Call2))
                {
                    cLicence = mLicenceList[nInd].Licence;
                    nRet = Constant.SUCCESS;
                    break;
                }
            }

            return nRet;
        }

        /// <summary>
        /// Set a bit at a prescribed position in a bitmap; the lowest bit position is zero.
        /// The maximum number of bits available is only limited by the number 
        /// of elements in the array of uint[] provided by the caller.
        /// e.g. if uint[] is of length 3, the maximum number of bits = 3 * 32 = 96.
        /// </summary>
        /// <param name="bitMap"> - an array of uint provided by the caller.</param>
        /// <param name="pseudoBitNum"> - position of the bit to be set (zero based)</param>
        /// <returns>Enums.SF.SUCCESS or Enums.SF.FAIL</returns>
        public static Enums.SF UtSetBit(ref uint[] bitMap, int pseudoBitNum)
        {
            uint mask;
            int arrayIndex;
            int actualBitNum;

            /* Check for valid bitnum */
            if (pseudoBitNum < 0 || (pseudoBitNum >= (bitMap.Length << Constant.NUM_SHIFT)))
            {
                Application.Exit("GenUtil.UtSetBit(): ERROR: invalid pseudoBitNum = " + pseudoBitNum);
                //return (Enums.SF.FAIL);
            }

            // Determine which int element the bit is in.
            arrayIndex = (pseudoBitNum >> Constant.NUM_SHIFT); /* divide by 32 to get selected int */
                                                               // Determine the actual bit position in the array element.
            actualBitNum = (pseudoBitNum % Constant.BITS_PER_INT);   /* arrayIndex within int */
                                                                     /* Create the mask */
            mask = Constant.BIT_ZERO >> actualBitNum;
            //Finally, set the bit.
            bitMap[arrayIndex] |= mask;

            return (Enums.SF.SUCCESS);
        }

        /// <summary>
        /// Test whether the bit at the prescribed position in the bitmap is 'set' (=1) or not; 
        /// the lowest bit position is zero.
        /// </summary>
        /// <param name="bitMap"> - an array of uint provided by the caller.</param>
        /// <param name="pseudoBitNum"> - position of the bit to be tested.</param>
        /// <returns>Enums.BIT.SET or Enums.BIT.CLEAR.</returns>
        public static Enums.BIT UtTestBit(uint[] bitMap, int pseudoBitNum)
        {
            uint mask;
            int arrayIndex;
            int actualBitNum;

            /* Check for valid bitnum */
            if (pseudoBitNum < 0 || (pseudoBitNum >= (bitMap.Length << Constant.NUM_SHIFT)))
            {
                Log2.e("\nGenUtil.UtTestBit(): ERROR: invalid bitNum: " + pseudoBitNum);
                return (Enums.BIT.FAIL);
            }

            // Determine which int element the bit is in.
            arrayIndex = (pseudoBitNum >> Constant.NUM_SHIFT); /* divide by 32 to get selected int */
                                                               // Determine the actual bit position in the array element.
            actualBitNum = (pseudoBitNum % Constant.BITS_PER_INT);   /* arrayIndex within int */
                                                                     // Create the mask.
            mask = Constant.BIT_ZERO >> actualBitNum;
            // Test the bit.
            if ((bitMap[arrayIndex] & mask) > 0)
            {
                return (Enums.BIT.SET);
            }
            else
            {
                return (Enums.BIT.CLEAR);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="bitMap"></param>
        /// <param name="bitNum"></param>
        /// <returns></returns>
        public static bool UtTestBit(int bitMap, int bitNum)
        {
            uint[] bitMapArray = new uint[1];
            bitMapArray[0] = (uint)bitMap;
            return (UtTestBit(bitMapArray, bitNum) == Enums.BIT.SET);
        }

        /// <summary>
        /// Clears the bit at the prescribed position in the bitmap (i.e. bit set to zero); 
        /// the lowest bit position is zero.
        /// </summary>
        /// <param name="bitMap"> - an array of uint provided by the caller.</param>
        /// <param name="pseudoBitNum"> - position of the bit to be tested.</param>
        /// <returns>Enums.SF.SUCCESS or Enums.SF.FAIL</returns>
        public static Enums.SF UtClearBit(uint[] bitMap, int pseudoBitNum)
        {
            uint mask;
            int arrayIndex;
            int actualBitNum;

            /* Check for valid bitnum */
            if (pseudoBitNum < 0 || (pseudoBitNum >= (bitMap.Length << Constant.NUM_SHIFT)))
            {
                return (Enums.SF.FAIL);
            }

            // Determine which int element the bit is in.
            arrayIndex = (pseudoBitNum >> Constant.NUM_SHIFT); /* divide by 32 to get selected int */
                                                               // Determine the actual bit position in the array element.
            actualBitNum = (pseudoBitNum % Constant.BITS_PER_INT);   /* arrayIndex within int */
                                                                     // Create the mask.
            mask = Constant.BIT_ZERO >> actualBitNum;
            // Clear the bit.
            bitMap[arrayIndex] &= (~mask);

            return (Enums.SF.SUCCESS);
        }

        /// <summary>
        /// Creates a string containing a full DB table name for a prescribed PDF
        /// name and table type. For example: "fcsa.hulme.ft_stxpwr_site".
        /// </summary>
        /// <param name="tabType"> - table type ID (e.g. Constant.FT_CHAN)</param>
        /// <param name="pdfName"> - name of PDF.</param>
        /// <param name="internalTableName"> - full DB table name.</param>
        /// <returns> - Enums.SF.SUCCESS or Enums.SF.FAIL</returns>
        public static int UtCvtName(int tabType, string pdfName, out string internalTableName)
        {
            //Console.Write("UtCvtName tabType: " + tabType.ToString() + "\n");
           // Console.Write("UtCvtName pdfName: " + pdfName + "\n");
            
            int result = Constant.FAILURE;  // default.
            internalTableName = null;  // default result.

            // Search for table conversion entry.

            Cvt cvtFound = Cvt.SearchCvtTab(tabType);
            if (cvtFound == null)
            {
                // Table type not found: set the new to the old */
                Console.Write("UtCvtName Table type not found \n");
                internalTableName = pdfName;
            }
            else  // We have found a Cvt for the prescribed tabType.
            {
                //  Check validity of pdfName.Length
                if (pdfName.Length == 0 || pdfName.Length > Constant.MAX_DISP_TAB_LEN)
                {
                    // pdfName.Length is invalid: don't construct the internalTableName.
                }
                else  // pdfName.Length is valid.
                {
                    // Check whether the DB global schema name is known at this time.
                    if (Info.GlobalSchema == null)
                    {
                        // Without the DB global schema name we can't construct the internalTableName.
                    }
                    else
                    {
                        // Construct the internal table name.
                        internalTableName = String.Format("{0}.{1}{2}{3}", Info.GlobalSchema, cvtFound.prefix, pdfName, cvtFound.suffix);
                        //Console.Write("UtCvtName internalTableName: " + internalTableName + "\n");
                        result = Constant.SUCCESS;
                    }  // Info.GlobalSchema ?
                }  // pdfName.Length ?
            }  // cvtFound == null ?

            if (result == Constant.FAILURE)
            {
                Console.Write("\nGenUtil.UtCvtName(): ERROR: construction of DB internal table name failed: tabType, pdfName = " + tabType + ", " + pdfName);
                Log2.e("\r\nGenUtil.UtCvtName(): ERROR: construction of DB internal table name failed: tabType, pdfName = " + tabType + ", " + pdfName);
            }

            Console.Write("\nGenUtil.UtCvtName(): internalTableName = " + internalTableName);
            return (result);
        }

        /// <summary>
        /// Creates three strings: one containing the current system date (dd-mmm-yyyy ), the second
        /// containing the current system time using the 24-hr clock (hh:mm:ss) and the third string being a concatenation
        /// of the date and time (dd-mmm-yyyy hh:mm:ss). It always returns the same 
        /// date and time strings for the very first call and all subsequent calls thus
        /// providing a consistent start date and time across all elements of the running program.
        /// </summary>
        /// <param name="dateVal"> - string conatining the current system date.</param>
        /// <param name="timeVal"> - string conatining the current system date.</param>
        /// <returns> - a string that concatenates the date and time separated by a single space character.</returns>
        public static void UtGetDateTime(out string dateVal, out string timeVal)
        {
            // Get current date and time from the system.
            DateTime dateTime = DateTime.Now;

            // Extract the infomation that we need.
            string yearStr = dateTime.Year.ToString();
            string monthStr = String.Format("{0:00}", dateTime.Month);
            string dayStr = String.Format("{0:00}", dateTime.Day);
            string hourStr = String.Format("{0:00}", dateTime.Hour);
            string minuteStr = String.Format("{0:00}", dateTime.Minute);

            // Construct the date string: YYYY.MM.DD
            StringBuilder sbDate = new StringBuilder();
            sbDate.Append(yearStr);
            sbDate.Append(".");
            sbDate.Append(monthStr);
            sbDate.Append(".");
            sbDate.Append(dayStr);
            dateVal = sbDate.ToString();

            // Construct the date string: HH:MM
            StringBuilder sbTime = new StringBuilder();
            sbTime.Append(hourStr);
            sbTime.Append(":");
            sbTime.Append(minuteStr);
            timeVal = sbTime.ToString();
        }

        /// <summary>
        /// Returns a string containing the current system date and time with format "dd-mmm-yyyy hh:mm:ss" 
        /// (uses the using the 24-hr clock).
        /// </summary>
        /// <returns> - the date and time separated by a single space character.</returns>
        public static string UtGetDateTime()
        {
            string dateVal;
            string timeVal;

            UtGetDateTime(out dateVal, out timeVal);
            return dateVal + "  " + timeVal;
        }

        /// <summary>
        /// Returns the user's project charge code as provided as a command line argument for 
        /// the currently executing MICS program.
        /// </summary>
        /// <param name="cProj"> - project charge code.</param>
        /// <returns> - project charge code.</returns>
        public static string GetProjectCode(out string cProj)
        {
            // 'out' requirement.
            cProj = "";

            if (String.IsNullOrWhiteSpace(Info.ProjectCode))
            {
                //...Log2.v("\r\nGenUtil.GetProjectCode(): project code not available from command line?");
                cProj = Environment.GetEnvironmentVariable("MICS_PROJECT");
                if (String.IsNullOrWhiteSpace(cProj))
                {
                    Log2.e("\r\nGenUtil.GetProjectCode(): ERROR: project code not available from command line nor the environment?");
                    cProj = "N/A";
                }
                else
                {
                    //...Log2.v("\r\nGenUtil.GetProjectCode(): project code set from environment variable MICS_PROJECT: " + cProj);
                    Info.ProjectCode = cProj;
                }
            }
            else
            {
                //...Log2.v("\r\nGenUtil.GetProjectCode(): project code already given in the command line? : " + Info.ProjectCode);
                cProj = Info.ProjectCode;
            }

            return cProj;
        }

        /// <summary>
        /// Returns the user's project charge code for the currently executing MICS program.
        /// </summary>
        /// <returns> - project charge code.</returns>
        public static string GetProjectCode()
        {
            string cProj = "";

            return GetProjectCode(out cProj);
        }

        /*****************************************************************************\
        *
        *   Return the "maximum" command of two in the first argument.
        *
        \*****************************************************************************/
        /// <summary>
        /// Returns the 'highest ranking' command character of two prescribed command characters; 
        /// the ordering is " NUBAD" with a space character being the lowest and 'D' being the highest.
        /// </summary>
        /// <param name="cOut"> - the higest ranking command.</param>
        /// <param name="cIn1"> - first prescribed command character.</param>
        /// <param name="cIn2"> - second prescribed command character.</param>
        public static void CmdMax(out string cOut, string cIn1, string cIn2)
        {
            //char cOrder[] = " NUBAD";   /*	Changed to make B > U :1275 - 2008.08.21 */
            const string cOrder = " NUBAD";

            int nOrder1 = cOrder.IndexOf(cIn1[0]);
            int nOrder2 = cOrder.IndexOf(cIn2[0]);

            /*  Error checks  */
            if (nOrder1 < 0)
            {
                cOut = cIn2;
                return;
            }
            if (nOrder2 < 0)
            {
                cOut = cIn1;
                return;
            }

            if (nOrder1 > nOrder2)
            {
                cOut = cIn1;
            }
            else
            {
                cOut = cIn2;
            }

            //...Log2.v(String.Format("\nGenUtil.CmdMax(): cIn1, cIn2, cOut: {0}, {1}, {2}", cIn1, cIn2, cOut));

            return;
        }

        /// <summary>
        /// Creates a string containing the country code for a prescribed province/state code
        /// as one of "CAN", "USA", "OTH" or "UNK"; contains the bug that "NE" (Nebraska) is part
        /// of Canada.
        /// </summary>
        /// <param name="prov"> - province or state code.</param>
        /// <param name="country"> - country code.</param>
        /// <returns>true if found "CAN", "USA", "OTH"; false if "UKN". </returns>
        public static bool UtGetCountry(string prov, out string country)
        {
            country = "UNK";

            // Canada?
            foreach (string province in Constant.CAN_PROVINCE_CODES)
            {
                if (province.Equals(prov))
                {
                    country = "CAN";
                    return true;
                }
            }

            // USA?
            foreach (string state in Constant.USA_STATE_CODES)
            {
                if (state.Equals(prov))
                {
                    country = "USA";
                    return true;
                }
            }

            // Other?
            foreach (string state in Constant.OTHER_STATE_CODES)
            {
                if (state.Equals(prov))
                {
                    country = "OTH";
                    return true;
                }
            }

            /* not in lists */
            return false;
        }

        /// <summary>
        /// This method returns a string identifying the country that contains
        /// the prescribed province or state code; the returned country code is
        /// one of "CAN, "USA", "FR" or "???".
        /// </summary>
        /// <param name="prov"></param>
        /// <returns></returns>
        public static string GetCountryCode(string prov)
        {
            string code = "???";

            if (IsInCanada(prov))
            {
                code = "CAN";
            }
            else if (IsInFrance(prov))
            {
                code = "FR";
            }
            else if (IsInUSA(prov))
            {
                code = "USA";
            }

            return code;
        }

        /// <summary>
        /// This method returns true if the prescribed province/state code is 
        /// recognized as a Canadian province.
        /// </summary>
        /// <param name="prov"></param>
        /// <returns></returns>
        public static bool IsInCanada(string prov)
        {
            bool result = false;

            foreach (string province in Constant.CAN_PROVINCE_CODES)
            {
                if (prov == province)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// This method returns true if the prescribed province/state code is 
        /// recognized as the French protectorate St. Pierre and Michelon.
        /// </summary>
        /// <param name="prov"></param>
        /// <returns></returns>
        public static bool IsInFrance(string prov)
        {
            bool result = false;

            foreach (string province in Constant.FRENCH_ISLAND_CODES)
            {
                if (prov == province)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// This method returns true if the prescribed province/state code is 
        /// recognized as a state of the USA.
        /// </summary>
        /// <param name="prov"></param>
        /// <returns></returns>
        public static bool IsInUSA(string prov)
        {
            bool result = false;

            foreach (string province in Constant.USA_STATE_CODES)
            {
                if (prov == province)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// This method returns true if the prescribed province/state code is 
        /// recognized as a state of the USA that shares a border with Canada; these are:
        /// "AK", "WA", "MT", "ND", "MN", "WI", "MI", "OH", "PA", "NY", "VT", "NH", "ME".
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static bool IsUSAborderState(string state)
        {
            bool result = false;

            foreach (string borderState in Constant.USA_BORDER_STATE_CODES)
            {
                if (state == borderState)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a string encoding a month as 3 letters into the corresponding
        /// encoding as 2 numbers, e.g. "APR" is converted to "04".
        /// </summary>
        /// <param name="month"> - On input: 3 letter month code to be converted. On output: 2 digit month code.</param>
        /// <returns>true if the prescribed 3 letter month code is valid; otherwise false.</returns>
        public static bool UtConvertMonth(ref string month)
        {
            bool retVal = true;
            string m = month.Trim().ToUpper();

            switch (m)
            {
                case "JAN":
                    month = "01";
                    break;
                case "FEB":
                    month = "02";
                    break;
                case "MAR":
                    month = "03";
                    break;
                case "APR":
                    month = "04";
                    break;
                case "MAY":
                    month = "05";
                    break;
                case "JUN":
                    month = "06";
                    break;
                case "JUL":
                    month = "07";
                    break;
                case "AUG":
                    month = "08";
                    break;
                case "SEP":
                    month = "09";
                    break;
                case "OCT":
                    month = "10";
                    break;
                case "NOV":
                    month = "11";
                    break;
                case "DEC":
                    month = "12";
                    break;
                default:
                    // Invalid month string.
                    month = "??";
                    retVal = false;
                    break;
            }

            return retVal;
        }

        /// <summary>
        /// Calculates the recieved power given all the loss components prescribed as call parameters. 
        /// Some parameters may be zero, if the caller does not want to include them yet.
        /// </summary>
        /// <param name="txpwr"> - transmitted power.</param>
        /// <param name="txfsl"> - TBD.</param>
        /// <param name="txAgain"> - transmitted antenna gain.</param>
        /// <param name="txAdisc"> - TBD.</param>
        /// <param name="dFreqKHz"> - operating frequency in KHz.</param>
        /// <param name="distKm"> - propagation distance in Km.</param>
        /// <param name="rxAgain"> - reciever antenna gain.</param>
        /// <param name="rxAdisc"> - TBD.</param>
        /// <param name="rxfsl"> - TBD.</param>
        /// <returns> - the calculated recieved power.</returns>
        public static double RxPower(double txpwr, double txfsl, double txAgain, double txAdisc, double dFreqKHz,
                                        double distKm, double rxAgain, double rxAdisc, double rxfsl)
        {
            double dRet;
            double dFreqLoss;
            double dDistLoss;
            double dAtten;

            if (dFreqKHz > 0)
            {
                dFreqLoss = Constant.POWER_LOG * Math.Log10(dFreqKHz / 1000.0);
            }
            else
            {
                dFreqLoss = 0.0;
            }

            if (distKm > 0)
            {
                dDistLoss = Constant.POWER_LOG * Math.Log10(distKm);
            }
            else
            {
                dDistLoss = 0.0;
            }

            //	Added.  zero frequencies or distances yield zero atten.
            dAtten = AtmosphericAtten(dFreqKHz, distKm);

            dRet = txpwr - txfsl + txAgain - txAdisc - dFreqLoss - dDistLoss - dAtten + rxAgain - rxAdisc - rxfsl - Constant.POWER_CONST;

            return dRet;
        }

        /// <summary>
        /// Calculates the atmospheric attenuation (dB) for a given operating frequency and 
        /// propagation distance. (See: http://www.mike-willis.com/Tutorial/PF5.htm
        /// which is based on the ITU document ITU-R P.676-8).
        /// </summary>
        /// <param name="dFreqKHz"> - operating frequency (KHz).</param>
        /// <param name="dDistKm"> - propagation distance (Km).</param>
        /// <returns> - the calculated the atmospheric attenuation (dB).</returns>
        public static double AtmosphericAtten(double dFreqKHz, double dDistKm)
        {
            double dAtten;

            //	Get the attenuation per Km.
            dAtten = AttenH20(dFreqKHz) + AttenO2(dFreqKHz);

            dAtten *= dDistKm;

            return dAtten;
        }

        /// <summary>
        /// Calculates attenuation in db/km caused by water vapour at a prescribed operating
        /// frequency in KHz. (See: http://www.mike-willis.com/Tutorial/PF5.htm
        /// which is based on the ITU document ITU-R P.676-8).
        /// </summary>
        /// <param name="dFreqKHz"> - operating frequency (KHz).</param>
        /// <returns> - the calculated attenuation (dB/Km).</returns>
        public static double AttenH20(double dFreqKHz)
        {
            const double RHO = 7.5; // Water vapour concentration in g/m^3
            double dAwater;
            double dFreqGHz = dFreqKHz / 1000000.0;  //	Equations use GHz.

            if (dFreqGHz == 0.0)
            {
                //	No Frequency, No attenuation.
                dAwater = 0.0;
            }
            else if (dFreqGHz <= 350.0)
            {
                //	It is in the valid range.
                dAwater = (0.050 +
                                     0.0021 * RHO +
                                     (3.6 / (Math.Pow(dFreqGHz - 22.2, 2) + 8.5)) +
                                     (10.6 / (Math.Pow(dFreqGHz - 183.3, 2) + 9.0)) +
                                     (8.9 / (Math.Pow(dFreqGHz - 325.4, 2) + 26.3))) *
                                  Math.Pow(dFreqGHz, 2) *
                                    RHO *
                                    0.0001;
            }
            else
            {
                //	Out of range of this simplification.
                dAwater = 0.0;
            }

            return dAwater;
        }

        /// <summary>
        /// Calculates attenuation in db/km caused by oxygen at a prescribed operating
        /// frequency in KHz. (See: http://www.mike-willis.com/Tutorial/PF5.htm
        /// which is based on the ITU document ITU-R P.676-8).
        /// </summary>
        /// <param name="dFreqKHz"> - operating frequency (KHz).</param>
        /// <returns> - the calculated attenuation (dB/Km).</returns>
        public static double AttenO2(double dFreqKHz)
        {
            double dAoxygen;
            double dFreqGHz = dFreqKHz / 1000000.0; //	Convert to GHz.

            if (dFreqGHz == 0.0)
            {
                //	No Frequency, No attenuation
                dAoxygen = 0.0;
            }
            else if (dFreqGHz <= 350.0)
            {
                if (dFreqGHz < 57.0)
                {
                    dAoxygen = (0.00719 +
                                          (6.09 / (dFreqGHz * dFreqGHz + 0.277)) +
                                            (4.81 / (Math.Pow(dFreqGHz - 57.0, 2) + 1.5))) *
                                         (dFreqGHz * dFreqGHz) *
                                         0.001;
                }
                else if (dFreqGHz > 63.0)
                {
                    dAoxygen = (0.000000379 * dFreqGHz +
                                  (0.265 / (Math.Pow(dFreqGHz - 63, 2) + 1.59)) +
                                            (0.028 / (Math.Pow(dFreqGHz - 118, 2) + 1.47))) *
                                         Math.Pow(dFreqGHz + 198.0, 2) *
                                         0.001;
                }
                else
                {
                    //	between the two ranges we use an averaged value.
                    dAoxygen = 14.9;
                }
            }
            else
            {
                // Out of range of this simplification.
                dAoxygen = 0.0;
            }

            return dAoxygen;
        }

        /// <summary>
        /// Writes string parameters to a prescribed stream via a TextWriter object; if the 
        /// TextWriter object is NULL then the method just returns.
        /// </summary>
        /// <param name="fp"> - object derived from TextWriter.</param>
        /// <param name="strParam"> - an array of string parameters to be written to fp.</param>
        public static void Qfprintf(TextWriter fp, params string[] strParam)
        {
            if (fp != null)
            {
                try
                {
                    for (int i = 0; i < strParam.Length; i++)
                    {
                        fp.Write(strParam[i]);
                    }
                    fp.Flush();
                }
                catch (Exception e)
                {
                    Log2.e("\r\nGenUtil.Qfprintf(): ERROR: try-catch exception caught: " + e.Message);
                }
            }
        }

        /// <summary>
        /// This method parses a passive antenna's acode and returns its height and width in meters
        /// if char3 of acode is 'F' then char 1-2 is height and char 4-5 is width in ft; else
        /// char 1-3 is height and char 4-6 is width in tenths of a meter.      
        /// /// </summary>
        /// <remarks>
        /// <para>
        /// If the 3rd letter of acode is 'F' then letters 1-2 give 
        /// the height and letters 4-5 the width in ft.
        /// </para>
        /// <para>
        /// If the 3rd letter of acode is <b>NOT</b> 'F' then letters 1-3 give 
        /// the height and letters 4-6 the width in tenths of a meter.
        /// </para>
        /// </remarks>
        /// <param name="acode"> - the antenna code of the passive.</param>
        /// <param name="height"> - the passive's height in meters.</param>
        /// <param name="width"> -  the passive's width in meters.</param>
        public static void TtPassiveAcode(string acode,
                                            out double height,
                                            out double width)
        {
            //...Log2.v("\n\nGenUtil.TtPassiveAcode(): Entry: acode = " + acode);

            // 'out' requirement.
            height = 0.0;
            width = 0.0;

            try
            {
                int ht, wt;

                if (acode.Length > 2 && acode[2] == 'F')
                {
                    ht = int.Parse(acode.Substring(0, 2));
                    height = (double)ht * Constant.FT_TO_METERS;

                    wt = int.Parse(acode.Substring(3, 2));
                    width = (double)wt * Constant.FT_TO_METERS;
                }
                else
                {
                    ht = int.Parse(acode.Substring(0, 3));

                    /* stored in tenths of a meter */
                    height = (double)ht / 10.0;

                    wt = int.Parse(acode.Substring(3, 3));

                    /* stored in tenths of a meter */
                    width = (double)wt / 10.0;
                }
            }
            catch (Exception e)
            {
                Log2.e("\n\nGenUtil.TtPassiveAcode() : ERROR : acode = " + acode);
                Log2.e("\n\nGenUtil.TtPassiveAcode() : ERROR : exception: " + e.Message);
                Log2.e("\n\nGenUtil.TtPassiveAcode() : ERROR : stackTrace: \n" + e.StackTrace);
            }

            //...Log2.v("\n\nGenUtil.TtPassiveAcode(): Exit");
        }

        /// <summary>
        /// Outputs the next token in a string without damaging the string.Initialize with a zero 
        /// counter.  The next token is truncated at the length of the token string.
        /// </summary>
        /// <param name="cToken"> - the next token found.</param>
        /// <param name="nTokenLen"> - number of characters in cToken.</param>
        /// <param name="cTokenString"> - the string to be searched for tokens.</param>
        /// <param name="cTokenSep"> - a string prescribing the separation pattern between tokens.</param>
        /// <param name="counter"> - provides a 'running' count of the tokens retrieved so far.</param>
        /// <returns> - 0 if the next token was found or 1 if there are no more tokens.</returns>
        public static int StepToken(out string cToken, int nTokenLen, string cTokenString, string cTokenSep, ref int counter)
        {
            //...Log2.v("\n\nGenUtil.StepToken(): Entry: cTokenString = " + Strings.AddBars(cTokenString));

            cToken = null;
            StringBuilder sb = new StringBuilder();
            int offset = 0;

            if (counter < 0 || cTokenString == null || cTokenSep == null)
            {
                return 1;
            }

            //while (*cPtr != '\0' && strchr(cTokensep, *cPtr) == NULL)
            while ((counter + offset < cTokenString.Length) && !cTokenSep.Contains(cTokenString[counter + offset]))
            {
                if (offset < nTokenLen - 1)
                {
                    sb.Append(cTokenString[counter + offset]);
                    offset++;
                }
            }

            // Finish.
            offset = Math.Min(offset, nTokenLen - 1);
            cToken = sb.ToString();

            if ((counter + offset) >= (cTokenString.Length) - 1)
            {
                //	End of the input string.
                counter = -1;
            }
            else
            {
                counter += offset + 1;
            }

            //...Log2.v("\n\nGenUtil.StepToken(): Exit: cTokenString = " + Strings.AddBars(cToken));
            return 0;
        }

        /// <summary>
        /// Checks whether a prescribed string contains only digits (0-9), or not.
        /// </summary>
        /// <param name="str"> - prescribed string to be checked.</param>
        /// <returns> - true if the string comprises only of the characters 0-9; otherwise false.</returns>
        public static bool UtIsAllDigits(string str)
        {
            bool isAllDigits = true;

            if (str != null)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    isAllDigits = isAllDigits && Char.IsDigit(str[i]);
                }
            }

            return isAllDigits;
        }

#if false  // This legacy method has a design flaw and has been superceeded.
        /// <summary>
        /// Create a MUTEX security attributes object that allows access to all; note
        /// that Info.MicsUserName <b>must</b> be set prior to calling this method.
        /// </summary>
        /// <returns> - a MutexSecurity object.</returns>
        /// <summary>
        /// Create a MUTEX security attributes object that allows access to all; note
        /// that Info.MicsUserName <b>must</b> be set prior to calling this method.
        /// </summary>
        /// <returns> - a MutexSecurity object.</returns>
        public static MutexSecurity SAEverybody()
        {
            // First of all, make sure that Info.MicsUserName has been set.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                string str = "\n\nGenUtil.SAEverybody(): ERROR: Info.MicsUserName is not set (env. var. 'MicsUser').";
                Log2.e(str);
                Console.Error.Write(str);
                Application.ExitQuietly(Error.MICSUSERNOTSET);
            }

            // Create a security object that grants no access. 
            MutexSecurity mutexSecurity = new MutexSecurity();

            // Add a rule that grants the current user the 
            // right to enter or release the mutex.
            NTAccount ntAccount = new NTAccount(Info.MicsUserName);
            MutexAccessRule rule = new MutexAccessRule(ntAccount,
                MutexRights.Synchronize | MutexRights.Modify,
                AccessControlType.Allow);
            mutexSecurity.AddAccessRule(rule);

            // Add a rule that denies the current user the 
            // right to change permissions on the mutex.
            rule = new MutexAccessRule(ntAccount,
                MutexRights.ChangePermissions,
                AccessControlType.Deny);
            mutexSecurity.AddAccessRule(rule);

            // Add a rule that allows the current user the 
            // right to read permissions on the mutex. This rule
            // is merged with the existing Allow rule.
            rule = new MutexAccessRule(ntAccount,
                MutexRights.ReadPermissions,
                AccessControlType.Allow);
            mutexSecurity.AddAccessRule(rule);

            //return pSA;
            return mutexSecurity;
        }
#endif
        /// <summary>
        /// Create a MUTEX security attributes object that allows access to all; note
        /// that Info.MicsUserName <b>must</b> be set prior to calling this method.
        /// </summary>
        /// <returns> - a MutexSecurity object.</returns>
        /// <summary>
        /// Create a MUTEX security attributes object that allows access to all; note
        /// that Info.MicsUserName <b>must</b> be set prior to calling this method.
        /// </summary>
        /// <returns> - a MutexSecurity object.</returns>
        public static MutexSecurity SAEverybody()
        {
            // First of all, make sure that Info.MicsUserName has been set.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                string str = "\n\nGenUtil.SAEverybody(): ERROR: Info.MicsUserName is not set (env. var. 'MicsUser').";
                Log2.e(str);
                Console.Error.Write(str);
                Application.ExitQuietly(Error.MICSUSERNOTSET);
            }

            // We don't have to be specific about the caller's Windows domain and username -
            // we could just use the following and everything works just fine.
            // SecurityIdentifier securityID = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

            // Create a string representing the current user.
            string user = Environment.UserDomainName + "\\" + Environment.UserName;
            //...Log2.v("\nGenUtil.SAEverybodyNew(): user = " + user);

            // Create a security object that initially grants no access. 
            MutexSecurity mutexSecurity = new MutexSecurity();

            // Now add the desired rule set to the mutexSecurity object.
            // Rules come in two flavors: 'allow' rules and 'deny' rules.
            MutexAccessRule rule;

            // Add a rule that grants the current user the right to enter or release the mutex.
            rule = new MutexAccessRule(user,
                                        MutexRights.Synchronize | MutexRights.Modify,
                                        AccessControlType.Allow);
                                        mutexSecurity.AddAccessRule(rule);

            // Add a rule that denies the current user the right to change permissions on the mutex.
            rule = new MutexAccessRule(user,
                                        MutexRights.ChangePermissions,
                                        AccessControlType.Deny);
                                        mutexSecurity.AddAccessRule(rule);

            // Add a rule that allows the current user the right to read permissions on the mutex. 
            // This rule is merged with the existing Allow rule.
            rule = new MutexAccessRule(user,
                                        MutexRights.ReadPermissions,
                                        AccessControlType.Allow);
                                        mutexSecurity.AddAccessRule(rule);
            //return pSA;
            return mutexSecurity;
        }

        /// <summary>
        /// Writes a summary of the current mutex security rules to Console.Out
        /// </summary>
        /// <param name="security"> - current MutexSecurity object.</param>
        public static void ShowSecurity(MutexSecurity security)
        {
            Console.WriteLine("\r\nCurrent access rules:\r\n");

            foreach (MutexAccessRule ar in
                security.GetAccessRules(true, true, typeof(NTAccount)))
            {
                Console.WriteLine("        User: {0}", ar.IdentityReference);
                Console.WriteLine("        Type: {0}", ar.AccessControlType);
                Console.WriteLine("      Rights: {0}", ar.MutexRights);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Returns the total process cpu time (Kernel + User) for the current process in integer milliseconds.
        /// </summary>
        /// <returns></returns>
        public static int WinClock()
        {
            Process thisProcess = Process.GetCurrentProcess();

            TimeSpan ts = thisProcess.TotalProcessorTime;

            int cpuMS = (int)thisProcess.TotalProcessorTime.TotalMilliseconds;

            return cpuMS;
        }

#if false
        /// <summary>
        /// Returns a unique name so that a table can be created while other users are running TSIP
        /// at the same time, as in batch.
        /// </summary>
        /// <param name="cUnique"></param>
        /// <param name="cMainPart"></param>
        /// <param name="cNextPart"></param>
        /// <param name="cRunName"></param>
        public static void MkUnique(out string cUnique, string cMainPart, string cNextPart, string cRunName)
        {
            Random rng = new Random();
            int randInt = rng.Next(0, Int32.MaxValue);

            cUnique = String.Format("{0}{1}{2}_{3}", cMainPart, cNextPart, cRunName, randInt);

            return;
        }
#endif
        /// <summary>
        /// Returns a unique name so that a table can be created while other users are running TSIP
        /// at the same time, as in batch.
        /// </summary>
        /// <param name="cUnique"> returns a unique table name.</param>
        /// <param name="schema"> the SQL Server database schema to be used.</param>
        /// <param name="prefix"> a prescribed string to be placed before the main part of the unique name.</param>
        /// <param name="cMainPart"> the main part of the unique name.</param>
        /// <param name="cNextPart"> the next part of the unique name.</param>
        /// <param name="cRunName"> a prescribed name for the run.</param>
        public static void MkUnique(out string cUnique, string schema, string prefix, string cMainPart, string cNextPart, string cRunName)
        {
            Random rng = new Random();
            int randInt = rng.Next(0, Int32.MaxValue);

            if (String.IsNullOrWhiteSpace(schema))
            {
                cUnique = String.Format("{1}{2}{3}{4}_{5}", prefix, cMainPart, cNextPart, cRunName, randInt);
            }
            else
            {
                cUnique = String.Format("{0}.{1}{2}{3}{4}_{5}", Info.GlobalSchema, prefix, cMainPart, cNextPart, cRunName, randInt);
            }

            cUnique = String.Format("{0}.{1}{2}{3}{4}_{5}", Info.GlobalSchema, prefix, cMainPart, cNextPart, cRunName, randInt);

            return;
        }

        /// <summary>
        /// Convert the given logitude (int) into degrees-minutes-seconds. hundreths 
        /// (string) and orientation.  
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="longStr"></param>
        /// <param name="orientationChar"></param>
        public static void UtLongConvStr(int longitude, out string longStr, out char orientationChar)
        {
            string orientationStr;
            UtLongConvStr(longitude, out longStr, out orientationStr);
            orientationChar = orientationStr[0];
        }

        /// <summary>
        /// Convert the given logitude (int) into degrees-minutes-seconds. hundreths 
        /// (string) and orientation.  
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="longStr"></param>
        /// <param name="orientation"></param>
        public static void UtLongConvStr(int longitude, out string longStr, out string orientation)
        {
            int degrees;
            int minutes;
            int seconds;
            int hundreds;

            if (longitude < 0)
            {
                orientation = "E";
                longitude *= -1;    /* make value positive */
            }
            else
            {
                orientation = "W";
            }

            degrees = longitude / 360000;
            minutes = (longitude / 6000) - (degrees * 60);
            seconds = (longitude / 100) - (((degrees * 60) + minutes) * 60);
            hundreds = (longitude % 100); /* hundreds of seconds */

            /* build string */
            longStr = String.Format("{0:D3}-{1:D2}-{2:D2}.{3:D2}",
                                        degrees, minutes, seconds, hundreds);
        }

        /// <summary>
        /// Convert the given latitude (int) into degrees-minutes-seconds. hundreths 
        /// (string) and orientation.  
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="latStr"></param>
        /// <param name="orientationChar"></param>
        public static void UtLatConvStr(int latitude, out string latStr, out char orientationChar)
        {
            string orientationStr;
            UtLatConvStr(latitude, out latStr, out orientationStr);
            orientationChar = orientationStr[0];
        }

        /// <summary>
        /// Convert the given latitude (int) into degrees-minutes-seconds. hundreths 
        /// (string) and orientation.  
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="latStr"></param>
        /// <param name="orientation"></param>
        public static void UtLatConvStr(int latitude, out string latStr, out string orientation)
        {
            int degrees;
            int minutes;
            int seconds;
            int hundreds;

            if (latitude < 0)
            {
                orientation = "S";
                latitude *= -1; /* make value positive */
            }
            else
            {
                orientation = "N";
            }

            degrees = latitude / 360000;
            minutes = (latitude / 6000) - (degrees * 60);
            seconds = (latitude / 100) - (((degrees * 60) + minutes) * 60);
            hundreds = (latitude % 100);    /* hundreds of seconds */

            /* build string */
            latStr = String.Format("{0:D2}-{1:D2}-{2:D2}.{3:D2}", degrees, minutes, seconds, hundreds);
        }

        /// <summary>
        /// Insert commas into a string representation of a double. Return a pointer to 
        /// the string, or NULL on failure. The value returned is in a static buffer 
        /// and must be consumed immediately.  
        /// </summary>
        /// <param name="dNumber"></param>
        /// <param name="nDecimals"></param>
        /// <returns></returns>
        public static string Ccommas(double dNumber, int nDecimals)
        {
            const char COMMA = ',';

            int pStart = -1;
            int pEnd;
            int nLen;
            int nLenHead;
            int nCommas;
            int pRet;
            string result;
            string cBuffStr;
            char[] cBuffArr;

            string digits = nDecimals.ToString();
            string format = "{0,50:F" + digits + "}";
            cBuffStr = String.Format(format, dNumber);
            cBuffArr = cBuffStr.ToArray();

            // C: returns a pointer to the last occurrence of character ' ' in string cBuff.
            //pStart = strrchr(cBuff, ' ') + 1;
            for (int i = 0; i < cBuffArr.Length; i++)
            {
                char c = cBuffArr[i];
                if (c == ' ')
                {
                    pStart = i + 1;
                }
            }

            if (pStart < 0)
            {
                return null;
            }

            if (nDecimals > 0)
            {
                pEnd = 49 - nDecimals;  // handle the decimal point.
            }
            else
            {
                pEnd = 50;
            }

            nLen = (int)(pEnd - pStart);
            nCommas = (nLen - 1) / 3;
            nLenHead = nLen % 3 != 0 ? nLen % 3 : 3;
            pRet = pStart - nCommas;
            while (nCommas > 0)
            {
                //memcpy(pStart - nCommas, pStart, nLenHead);
                for (int i = 0; i < nLenHead; i++)
                {
                    cBuffArr[pStart - nCommas + i] = cBuffArr[pStart + i];
                }

                cBuffArr[pStart - nCommas + nLenHead] = COMMA;
                pStart += nLenHead;
                nCommas--;
                nLenHead = 3;
            }

            result = new string(cBuffArr);
            result = result.Trim();

            return result;
        }

        /// <summary>
        /// This is the initial breakout routine that creates the parameter structure. 
        /// It returns an anonymous pointer to the structure (in the sense that the 
        /// user uses the structure as a handle and does not examine it. The other 
        /// routines accept this structure.  
        /// </summary>
        /// <param name="cBuf"> - The input string</param>
        /// <returns></returns>
        public static ParmStrct ParmBreakOut(string cBuf)   /*  The input string */
        {
            List<Parm> parmList = new List<Parm>();
            Parm parm;

            //int               nInParmCnt = 0;
            //int               nAllocSize = 0;
            int nPtr = 0;     /*  pointer into the buffer */

            /*  Go through the line, getting parameters */
            while (ParmGetNext(out parm, cBuf, ref nPtr) == 0)
            {
                parmList.Add(parm);
            }

            ParmStrct pPA = new ParmStrct();
            pPA.aParmArray = parmList.ToArray();
            pPA.nParmCnt = parmList.Count;
            pPA.nArrayCnt = parmList.Count;

            /*  Finished loading in the parameters.*/
            return pPA;
        }

        /************************************************************************\
*
*   Get the next parm=value pair in the input buffer.  The current
*   location is pointed to.
*
\************************************************************************/
        /// <summary>
        /// </summary>
        /// <param name="pParm"> - The parm structure</param>
        /// <param name="cBuf"> - The input buffer</param>
        /// <param name="nPtr"> - The current offset</param>
        /// <returns></returns>
        public static int ParmGetNext(out Parm pParm,          /*  The parm structure */
                                        string cBuf,       /*  The input buffer   */
                                    ref int nPtr)         /*  The current offset */
        {
            //...Log2.v("\nGenUtil.ParmGetNext(): Entry: cBuf = " + cBuf + "    nPtr = " + nPtr);

            // 'out' requirement.
            pParm = null;

            if (nPtr >= cBuf.Length)
            {
                return 1;      /*  End of buffer */
            }

            int nStart;
            int nLen;
            int nInd = 0;

            /*  skip commas and blanks, nPtr is left pointing to a comma or \0 */
            while (cBuf[nPtr] == ' ' || cBuf[nPtr] == ',')
            {
                nPtr++;
            }

            if (nPtr == cBuf.Length)
            {
                return 1;      /*  End of buffer */
            }

            /*  Scan for equal sign */
            nStart = nPtr;
            while (cBuf[nPtr] != '=' && nPtr != cBuf.Length)
            {
                nPtr++;
            }

            /*  We now have an equal sign or end of string.  This is the parameter
            *   name.  Allocate a string for it and convert to uppercase.        */
            nLen = nPtr - nStart + 1;
            char[] cParmName = new char[nLen];

            /*  We do a special move in to make it upper case and end on a space */
            for (nInd = 0; nStart < nPtr && cBuf[nStart] != ' '; nStart++, nInd++)
            {
                cParmName[nInd] = Char.ToUpper(cBuf[nStart]);
            }

            /*  Now the pointer is either at the = sign or the end of string.  If it
            *   is the end of string then exit  */
            //if (cBuf[*nPtr] == '\0')
            if (nPtr == cBuf.Length)
            {
                pParm.cParmValue = null;
                return -2;
            }

            /*  Get the value.  This is not general purpose in that we will not
            *   accept comma delimeted strings */
            nPtr++;    /*  skip the = sign */
                       /*  Now skip the initial blanks in the parameter */
            while (cBuf[nPtr] == ' ' && nPtr != cBuf.Length)
            {
                nPtr++;
            }

            nStart = nPtr;
            while (nPtr != cBuf.Length && cBuf[nPtr] != ',')
            {
                nPtr++;
            }

            /*  nPtr points to the terminating , or the end */
            nLen = nPtr - nStart + 1;
            char[] cParmValue = new char[nLen];

            nLen--;

            for (int i = 0; i < nLen; i++)
            {
                cParmValue[i] = cBuf[nStart + i];
            }

            pParm = new Parm();

            // Exclude the final '\0'
            pParm.cParmName = (new string(cParmName, 0, cParmName.Length - 1));
            pParm.cParmValue = (new string(cParmValue, 0, cParmValue.Length - 1));

            //...Log2.v("\nGenUtil.ParmGetNext(): cParmName:  " + pParm.cParmName + "    cParmValue : " + pParm.cParmValue);

            //...Log2.v("\nGenUtil.ParmGetNext(): Exit: nPtr = " + nPtr);
            return 0;
        }

        /*************************************************************************\
        *
        *   Return a pointer to a value by parameter name
        *
        \*************************************************************************/
        /// <summary>
        /// Return a pointer to a value by parameter name.  
        /// </summary>
        /// <param name="pPS"></param>
        /// <param name="cName"></param>
        /// <returns></returns>
        public static string ParmByName(ParmStrct pPS, string cName)
        {
            string foundValue = null;
            string cUName;
            int nLen;
            int nInd;

            if (pPS == null || cName == null)
            {
                return null;
            }

            nLen = cName.Length;
            cUName = cName.ToUpper();

            /* Now go through the parameters */
            for (nInd = 0; nInd < pPS.nParmCnt; nInd++)
            {
                if (cUName.Equals(pPS.aParmArray[nInd].cParmName))
                {
                    foundValue = pPS.aParmArray[nInd].cParmValue;
                    break;
                }
            }

            return foundValue;
        }

        // First poke must have non-null inputLine.
        // Subsequent pokes should have inoutLine as null.
        /// <summary>
        /// This method calls strpok to get the next available token; tokens are
        /// separated by commas. 
        /// </summary>
        /// <param name="inputLine"> - pointer to string to be tokenized</param>
        /// <param name="token"> - string token</param>
        /// <param name="stringLength"> - max string token length</param>
        /// <returns></returns>
        public static int UtGetInputString(string inputLine, out string token, int stringLength)
        {
            if (!String.IsNullOrWhiteSpace(inputLine))
            {
                //...Log2.v("\nGenUtil.UtGetInputString(): inputLine = " + Strings.AddBars(inputLine));
                count = 0;
            }

            string iString;
            int size;
            int result = -666;
            token = "";

            /* get next available token */
            // First poke must have non-null inputLine.
            // Subsequent pokes should have inoutLine as null.
            iString = StrPok(inputLine, Constant.DELIMITER_CHAR);

            count++;

            /* if the end of string is encountered */
            if (iString == null)
            {
                /* end of string */
                result = Constant.UT_EOLN;
            }
            else
            {
                token = iString.ToUpper();

                /* if length greater than requested length */
                size = iString.Length;
                if (size <= stringLength)
                {
                    result = size;
                }
                else
                {
                    /* too long */
                    //...Log2.v("\nGenUtil.UtGetInputString(): too long");
                    result = Constant.UT_INV_CONV;
                }
            }

            //...Log2.v(String.Format("\nGenUtil.UtGetInputString(): {0,2}  {1}  {2}", count, Strings.AddBars(token), result));
            return result;
        }

        // First poke must have non-null inputLine.
        // Subsequent pokes should have inoutLine as null.
        /// <summary>
        /// This method returns a pointer to the next available token in a string 
        /// delimited by the 'delimiter' character. Different than the 'C' function 
        /// strtok, this function can return a pointer to a NULL string.  
        /// </summary>
        /// <param name="inputLine"> - pointer to the string to be tokenized. Passed once to begin tokenizing. max 512 characters.</param>
        /// <param name="delimiter"> - delimiter character</param>
        /// <returns></returns>
        public static string StrPok(string inputLine, char delimiter)
        {
            //...Log2.v("\nGenUtil.StrPok(): Entry: inputLine = " + inputLine);

            int startPos;
            string returnString = null;

            /* initialize token string */
            // The first poke.
            if (!String.IsNullOrWhiteSpace(inputLine))
            {
                mStrPokBuffer = inputLine;
                mPos = 0;
                mEndOfString = false;

                //...Log2.v("\nGenUtil.StrPok(): first poke: inputLine = " + inputLine);
            }

            if (mEndOfString == true)
            {
                return (null);
            }

            startPos = mPos;

            /* point to next available token */
            while ((mPos < mStrPokBuffer.Length) && (mStrPokBuffer[mPos] != delimiter))
            {
                mPos++;
            }

            /* if end of string no need to null terminate the token */
            if (mPos >= mStrPokBuffer.Length)
            {
                mEndOfString = true;
                mPos = -666;
                returnString = mStrPokBuffer.Substring(startPos).Trim();
            }
            else
            {
                int length = mPos - startPos;
                returnString = mStrPokBuffer.Substring(startPos, length).Trim();
                mPos++;
            }

            //...Log2.v("\nGenUtil.StrPok(): Exit: returnString = " + returnString + "    mPos = " + mPos);
            return (returnString);
        }

        /// <summary>
        /// This method returns the current string used for 'peek and poke' operations.
        /// </summary>
        /// <returns></returns>
        public static string GetStrPokBuffer()
        {
            if (mStrPokBuffer == null)
            {
                return "";
            }
            else
            {
                return mStrPokBuffer;
            }
        }

        /// <summary>
        /// Calculate the difference in longitude at a given latitude for a specified 
        /// distance.  
        /// </summary>
        /// <param name="latStn1Sec"></param>
        /// <param name="distKm"></param>
        /// <param name="deltaLat"></param>
        /// <param name="deltaLong"></param>
        public static void UtDeltaLatLong(int latStn1Sec, double distKm, out int deltaLat, out int deltaLong)

        /* latStn1Sec, deltaLat and deltaLong are in units of hundredths of seconds */

        {
            double latRad1,                /* latitude in radians of stn 1      */
                    latRad2,                /* latitude in radians of stn 2      */
                    latDiff,                /* lat diff between stn 1 & 2        */
                    latAvr,
                    c1,                     /* temporary variable                */
                    am,                     /* temporary variable                */
                              N, M, tmp1, tmp2, tmp3, dblTmp;

            M = 0.012440435;
            N = 3252.986745;

            if (distKm == 0.0)
            {
                /* cannot allow a distance of zero (floating point errors i) */
                distKm = 0.0001;
            }

            /* convert to seconds (from hundreths) and then to radians */
            dblTmp = (double)(latStn1Sec);
            latRad1 = dblTmp / 100.0 * Constant.SEC_TO_RAD;
            latRad2 = latRad1;
            latDiff = 1.0E-8;

            latAvr = latRad2 + (latDiff / 2.0);
            c1 = 1.0 - (Constant.ECCSQ * Pow(Sin(latAvr), 2));
            am = Sqrt(c1) / Constant.S;
            tmp1 = (1.0 - Constant.ECCSQ) * latDiff;
            tmp2 = (distKm * am * M) / Cos(latAvr);
            tmp3 = Cos(latAvr) * c1 / tmp1;

            dblTmp = Pow(Sin(Pow(Acos(1.0 / (tmp2 * tmp3)), 2)), 2) * tmp2;
            dblTmp = dblTmp / Constant.SEC_TO_RAD * 100.0;
            deltaLong = (int)(dblTmp + 0.5);   /* in seconds ? */
            deltaLat = (int)((distKm * N) + 0.5);  /* in hundredths of seconds */
        }

        /// <summary>
        /// Computes the straight line interpolation Notes: Have points A and C as well 
        /// as known values at those points. Want to find the value at point B, where B 
        /// is less than C and greater than A. Key-> A: minAng value at A: minVal.  
        /// </summary>
        /// <param name="angle"> - angle at which to find unknown val</param>
        /// <param name="minAng"> - closest known angle < 'angle'</param>
        /// <param name="maxAng"> - closest known angle > 'angle'</param>
        /// <param name="minVal"> - value at minAng</param>
        /// <param name="maxVal"> - value at maxAng</param>
        /// <param name="result"> - computed value at 'angle'</param>
        public static void Interp(float angle,    /* input  - angle at which to find unknown val*/
                        float minAng,   /* input  - closest known angle < 'angle' */
                        float maxAng,   /* input  - closest known angle > 'angle' */
                        float minVal,   /* input  - value at minAng */
                        float maxVal,   /* input  - value at maxAng */
                        out float result)  /* output - computed value at 'angle' */
        {
            // 'out' requirement.
            result = 0.0f;

            if (angle >= maxAng)
            {
                /* if angle is greater than max - set result to maxVal */
                result = maxVal;
            }
            else if (angle <= minAng)
            {
                /* if angle is less than min - set result to minVal */
                result = minVal;
            }
            else
            {
                /* angle is between max and min so do interpolation */
                if (minVal == maxVal)
                {
                    /* max and min values are same, straigt line
                     * interpolated value is same value
                     */
                    result = minVal;
                }
                else
                {
                    /* do actual interpolation */
                    result = ((maxVal - minVal) * (angle - minAng)) / (maxAng - minAng) + minVal;
                }
            }
        }

        /// <summary>
        /// This calculates the Free Space propagation loss as a subroutine.  
        /// </summary>
        /// <param name="plengthKm"></param>
        /// <param name="freqMHz"></param>
        /// <param name="pLoss"></param>
        public static void FreeSpacePathLoss(double plengthKm, double freqMHz, out double pLoss)
        {
            pLoss = 32.45 + (20 * Log10(plengthKm)) + (20 * Log10(freqMHz));
            pLoss += AtmosphericAtten(freqMHz * 1000, plengthKm);
        }

        /// <summary>
        /// Add an error message in front of the already existing error message.  
        /// </summary>
        /// <param name="cErrMess"></param>
        /// <returns></returns>
        public static int AddErr(string cErrMess)
        {

            mUserMess.Insert(0, cErrMess);

            return cErrMess.Length;
        }

        private static List<string> pStrings = new List<string>();

        /// <summary>
        /// Remember a string in memory. Returns 1 if it is already there, 0 if not.  
        /// </summary>
        /// <param name="cString"></param>
        /// <returns></returns>
        public static bool Remember(string cString)
        {
            bool nFound = false;

            for (int nInd = 0; nInd < pStrings.Count; nInd++)
            {
                if (pStrings[nInd].Equals(cString))
                {
                    nFound = true;
                    break;
                }
            }

            if (!nFound)
            {
                // Add the new string to the list.
                pStrings.Add(cString);
            }

            return nFound;
        }

        /// <summary>
        /// Calculate the included angle between two unit vectors represented only as 
        /// Azimuth and Elevation.  
        /// </summary>
        /// <param name="dAz1"></param>
        /// <param name="dEl1"></param>
        /// <param name="dAz2"></param>
        /// <param name="dEl2"></param>
        /// <returns></returns>
        public static double IncAngle(double dAz1, double dEl1, double dAz2, double dEl2)
        {
            double d1x;
            double d1y;
            double d1z;
            double d2x;
            double d2y;
            double d2z;
            double dDot;

            d1x = CosD(dAz1) * CosD(dEl1);
            d1y = SinD(dAz1) * CosD(dEl1);
            d1z = SinD(dEl1);

            d2x = CosD(dAz2) * CosD(dEl2);
            d2y = SinD(dAz2) * CosD(dEl2);
            d2z = SinD(dEl2);

            dDot = (d1x * d2x) + (d1y * d2y) + (d1z * d2z);
            if (dDot >= 1.0 || dDot <= -1.0)
            {
                return (0.0);
            }
            else
            {
                return (AcosD(dDot));
            }
        }

        /// <summary>
        /// Given a short table name, set the full channel table name.  
        /// </summary>
        /// <param name="cInName"></param>
        /// <param name="cType"></param>
        /// <param name="cOutName"></param>
        /// <returns></returns>
        public static int SetFullChanName(string cInName, string cType, out string cOutName)
        {
            // 'out' requirement;
            cOutName = "";

            int nRet = 0;

            if (cType.Equals("T"))
            {
                GenUtil.UtCvtName(Constant.FT_CHAN, cInName, out cOutName);
            }
            else if (cType.Equals("E"))
            {
                GenUtil.UtCvtName(Constant.FE_CHAN, cInName, out cOutName);
            }
            else if (cType.Equals("MDB_TS"))
            {
                cOutName = "mt_chan";
            }
            else if (cType.Equals("MDB_ES"))
            {
                cOutName = "me_chan";
            }
            else if (cType.Equals("PDF_TS"))
            {
                GenUtil.UtCvtName(Constant.FT_CHAN, cInName, out cOutName);
            }
            else if (cType.Equals("PDF_ES"))
            {
                GenUtil.UtCvtName(Constant.FE_CHAN, cInName, out cOutName);
            }
            else if (cType.Equals("INTRA"))
            {
                GenUtil.UtCvtName(Constant.FT_CHAN, cInName, out cOutName);
            }
            else
            {
                nRet = -1;
            }

            return nRet;
        }

        /// <summary>
        /// This method will calculate the path loss using one of three different 
        /// equations depending on distance (0-90-160-beyond). This has been moved here 
        /// from ttCalcs (part of tsip).  
        /// </summary>
        /// <param name="distanceKm"> - distance between two sites</param>
        /// <param name="frequencyKHz"> - frequency (in kHz )</param>
        /// <param name="patloss"> - path loss result</param>
        /// <returns></returns>
        public static int CalcPatLoss(double distanceKm,  /* distance between two sites */
                          double frequencyKHz,  /* frequency (in kHz ) */
                                out double patloss)    /* path loss result */
        {
            // 'out' requirement.
            patloss = 0.0;

            double ghzfreq; /* frequency in GHz */
            int nRet = 0;

            ghzfreq = frequencyKHz / 1000000;

            if ((distanceKm > 0.0) && (ghzfreq > 0.0))
            {
                if (distanceKm > 90.0)
                {
                    if (distanceKm > 160.0)
                    {
                        patloss = 14.0 + (20.0 * (Log10(ghzfreq / 4.0))) +
                                   (80.0 * (Log10(distanceKm)));
                    }
                    else
                    {
                        patloss = -228.0 + (20.0 * (Log10(ghzfreq / 4.0))) +
                                       (190.0 * (Log10(distanceKm)));
                    }
                }
                else
                {
                    patloss = 92.45 + (20.0 * (Log10(ghzfreq))) + (20.0 * (Log10(distanceKm)));
                }

                //	Add the high frequency Attenuation due to water and oxygen.
                patloss += AtmosphericAtten(frequencyKHz, distanceKm);
            }
            else
            {
                GenUtil.SetError(9701, "Invalid data for the path loss calculations");
                nRet = -1;
            }

            return nRet;
        }

        /// <summary>
        /// This method inputs an input date string and converts it to an SI date string (YYYY.MM.DD).
		/// Valid input formats are YYYY.MM.DD or dd-mmm-yyyy.
        /// </summary>
        /// <param name="inputLine"></param>
        /// <param name="inputString"></param>
        /// <param name="stringLength"></param>
        /// <returns></returns>
        public static int UtGetInputDate(string inputLine, out string inputString, int stringLength)
        {
            // 'out' requirement.
            inputString = "";

            string istring;
            int size;

            /* get next available token */
            istring = StrPok(inputLine, Constant.DELIMITER_CHAR);

            /* if the end of string is encountered */
            if (istring == null)
            {
                /* end of string */
                return (-1);
            }

            /* remove leading and trailing blanks */
            istring = istring.Trim();

            /* if length greater than requested length */
            size = istring.Length;
            if (size == 0)
            {
                return 0;   //	No field, no problem.
            }

            if (size <= stringLength)
            {
                /*	Check which format by getting the first numeric */
                int nInd;

                for (nInd = 0; Char.IsDigit(istring[nInd]) && nInd < size; nInd++) { }
                //	nInd will be 2 for dd-mmm-yyyy and 4 for yyyy.mm.dd
                switch (nInd)
                {
                    case 2:
                        //	Old style
                        UtConvertDate(ref istring);
                        break;

                    case 4:
                        //	SI, no conversion needed.
                        break;

                    default:
                        return -2;
                        // 2025/10/1break;
                }
                inputString = istring;
                return istring.Length;
            }
            else
            {
                /* too long */
                return (-2);
            }
        }

        /// <summary>
        /// This method inputs a date in format dd-mmm-yyyy and converts it to YYYY.MM.DD .
        /// </summary>
        /// <param name="cdate"></param>
        public static bool UtConvertDate(ref string cdate)
        {
            string localDate;
            string cyear;
            string cmon;
            string cday;

            //...Log2.v("\nGenUtil.UtConvertDate(): cdate = " + cdate);

            localDate = cdate.Trim();

            // Convert the date to the format YYYY.MM.DD

            cday = localDate.Substring(0, 2);

            cmon = localDate.Substring(3, 3);

            // This will return false and cmon = "??" for an invalid input month string.
            if (!UtConvertMonth(ref cmon))
            {
                return false;
            }

            //	Handle the case of a two digit year.
            string str = localDate.Substring(7);
            if (str.Length == 2)
            {
                if (String.CompareOrdinal(str, "51") > 0)
                {
                    cyear = "19";
                }
                else
                {
                    cyear = "20";
                }
                cyear += str;
            }
            else
            {
                cyear = str;
            }

            cdate = String.Format("{0:I4}.{1:I2}.{2:I2}", cyear, cmon, cday);

            return true;
        }

        /// <summary>
        /// This method converts a latitude or longitude string to centiseconds of arc.
        /// </summary>
        /// <param name="latLongCode"></param>
        /// <param name="latLongStr"></param>
        /// <param name="orientChar"></param>
        /// <param name="latLong"></param>
        /// <returns></returns>
        public static int UtStrConvLongLat(int latLongCode,
                                            string latLongStr,
                                            char orientChar,
                                            out int latLong)
        {
            return UtStrConvLongLat(latLongCode, latLongStr, orientChar.ToString(), out latLong);
        }

        /// <summary>
        /// This method converts a latitude or longitude string to centiseconds of arc.
        /// </summary>
        /// <param name="latLongCode"></param>
        /// <param name="latLongStr"></param>
        /// <param name="orientStr"></param>
        /// <param name="latLong"></param>
        /// <returns></returns>
        public static int UtStrConvLongLat(int latLongCode,
                                            string latLongStr,
                                            string orientStr,
                                            out int latLong)
        {
            //...Log2.v(String.Format("\nGenUtil.UtStrConvLongLat(): Entry: {0}  {1}  {2}", latLongCode, latLongStr, orientStr));

            // 'out' requirement.
            latLong = Int32.MinValue;

            string[] tokens;      // [Constant.MAX_TOKENS];      /* tokens found */                            /* work string */
            int tc;

            int degrees;
            int minutes;
            int seconds;
            int hundSeconds;
            int orientation;

            try
            {
                /* check orientation characters */
                if (((latLongCode == Constant.LONGITUDE)
                        && (!orientStr.Equals(Constant.EAST))
                        && (!orientStr.Equals(Constant.WEST)))
                || ((latLongCode == Constant.LATITUDE)
                        && (!orientStr.Equals(Constant.NORTH))
                        && (!orientStr.Equals(Constant.SOUTH))))
                {
                    /* invalid orientation for given latLongCode */
                    Log2.e("\nGenUtil.UtStrConvLongLat(): ERROR: invalid orientation for given latLongCode.");
                    return (Constant.FAILURE);
                }

                /* break down workString into tokens */
                tc = TokenizeLatLong(latLongStr, out tokens);

                foreach (string token in tokens)
                {
                    //...Log2.v("\nGenUtil.UtStrConvLongLat(): " + Strings.AddBars(token));
                }

                /* init individual values */
                degrees = minutes = seconds = hundSeconds = 0;

                // Input Lat/Long format: 00-00-00.00

                /* convert each token to it's respective value.  The last token is
                   hundreths of seconds (if it exists), the second last is seconds
                   (if it exists), the third last is minutes (if it exists) and
                   the fourth last is degrees (if it exists).
                */

                // Degrees.
                degrees = Convert.ToInt32(tokens[0]);
                if (((latLongCode == Constant.LATITUDE)
                        && (degrees > Constant.MAX_LAT_DEGREES))
                    || ((latLongCode == Constant.LONGITUDE)
                        && (degrees > Constant.MAX_LONG_DEGREES)))
                {
                    Log2.e("\nGenUtil.UtStrConvLongLat(): ERROR: degrees: invalid value: " + degrees);
                    return (Constant.FAILURE);
                }

                if (degrees < 0)
                {
                    /* token string invalid - too large possibly */
                    Log2.e("\nGenUtil.UtStrConvLongLat(): ERROR: degrees: invalid value: " + degrees);
                    return (Constant.FAILURE);
                }

                // Minutes.
                minutes = Convert.ToInt32(tokens[1]);
                if (minutes > Constant.MAX_MINUTES)
                {
                    return (Constant.FAILURE);
                }
                if (minutes < 0)
                {
                    /* token string invalid - too large possibly */
                    Log2.e("\nGenUtil.UtStrConvLongLat(): ERROR: minutes: invalid value: " + minutes);
                    return (Constant.FAILURE);
                }

                // Seconds.
                seconds = Convert.ToInt32(tokens[2]);
                if (seconds > Constant.MAX_SECONDS_LL)
                {
                    Log2.e("\nGenUtil.UtStrConvLongLat(): ERROR: seconds: invalid value: " + seconds);
                    return (Constant.FAILURE);
                }
                if (seconds < 0)
                {
                    /* token string invalid - too large possibly */
                    Log2.e("\nGenUtil.UtStrConvLongLat(): ERROR: seconds: invalid value: " + seconds);
                    return (Constant.FAILURE);
                }

                // Hundreths of seconds.
                if (String.IsNullOrWhiteSpace(tokens[3]))
                {
                    hundSeconds = 0;
                }
                else
                {
                    hundSeconds = Convert.ToInt32(tokens[3]);
                    if (tokens[3].Length == 1)
                    {
                        hundSeconds *= 10;
                    }
                    else if (tokens[3].Length > 2)
                    {
                        Log2.e("\nGenUtil.UtStrConvLongLat(): ERROR: hundSeconds: too many digits: " + tokens[3]);
                        return (Constant.FAILURE);
                    }
                    if (hundSeconds < 0)
                    {
                        /* token string invalid - too large possibly */
                        Log2.e("\nGenUtil.UtStrConvLongLat(): ERROR: hundSeconds: invalid value: " + hundSeconds);
                        return (Constant.FAILURE);
                    }
                }

                // Allow a value MAX_LAT_DEGREES/MAX_LONG_DEGREES
                // only if the other values are 0.
                if ((((latLongCode == Constant.LATITUDE) && (degrees == Constant.MAX_LAT_DEGREES))
                            || ((latLongCode == Constant.LONGITUDE) && (degrees == Constant.MAX_LONG_DEGREES)))
                        &&
                            ((minutes != 0) || (seconds != 0) || (hundSeconds != 0))
                   )
                {
                    Log2.e("\nGenUtil.UtStrConvLongLat(): ERROR: invalid MAX_LAT_DEGREES/MAX_LONG_DEGREES: " + latLongStr);
                    return (Constant.FAILURE);
                }

                orientation = 1;    /* positive orientation */

                if ((orientStr.Equals(Constant.SOUTH)) || (orientStr.Equals(Constant.EAST)))
                {
                    /* set orientation to negative */
                    orientation = -1;
                }

                /* calculate converted value */
                latLong = ((((((degrees * 60) + minutes) * 60) + seconds) * 100) + hundSeconds) * orientation;

                //...Log2.v(String.Format("\nGenUtil.UtStrConvLongLat(): {0} - {1} - {2} - {3}", degrees, minutes, seconds, hundSeconds));

                //...Log2.v(String.Format("\nGenUtil.UtStrConvLongLat(): Exit: latLong = {0}", latLong));
                return (Constant.SUCCESS);

            }
            catch (Exception e)
            {
                Log2.e("\nGenUtil.UtStrConvLongLat(): ERROR: string to integer conversion exception: " + e.Message);
                return (Constant.FAILURE);
            }
        }

        /// <summary>
        /// This method parses the numerical elements of a latitude and/or longitude string
        /// (format: 00-00-00.00) and returns a four-element string array corresponding to the
        /// degrees, minutes, seconds and centiseconds.
        /// </summary>
        /// <param name="str"> - the prescribed string to be parsed.</param>
        /// <param name="tokens"> - populated string[4] object.</param>
        /// <returns> - Constant.MAX_TOKENS.</returns>
        public static int TokenizeLatLong(string str, out string[] tokens)
        {
            // 'out' requirement
            tokens = new string[Constant.MAX_TOKENS];

            int tc;     /* token counter */
            bool done;   /* signal when done flag */
            string wt;  /* work token pointer */

            /* If last char in string is '.' then it must be removed or will
             * cause problems later.  Check for this and replace it with
             * NULL if there.
            */
            if (Strings.LastCharIs(str, '.'))
            {
                str = str.Substring(0, str.Length - 1);
            }

            done = false;
            tc = 0;     /* initialize token counter */
            wt = "";     /* begin of string - first token */

            /* While not pointing to NULL terminator and done flag not set */
            int i = 0;
            while ((i < str.Length) && !done)
            {

                char c = str[i++];

                /* If character is a separator char (the '-' or '.') */
                if ((c == Constant.SEP_CHAR1) || (c == Constant.SEP_CHAR2))
                {
                    tokens[tc++] = wt;  /* register previous token */

                    if (tc == Constant.MAX_TOKENS)
                    {
                        return (Constant.MAX_TOKENS); /* too many tokens */
                    }

                    wt = ""; /* start next token */

                    if (c == Constant.SEP_CHAR2)
                    {
                        /* decimal seperator encountered */
                        tokens[tc++] = str.Substring(i);  /* the remainder of
							 * the string is the
							 * hundreds seconds
							 * token */
                        done = true;   /* set done flag */
                    }

                }
                else
                {
                    // Accumulate the character into the token buffer.
                    wt += c;

                    // AH: Bug fix on 20190728.
                    // We have to ensure that the final token is saved.
                    if (i == str.Length)
                    {
                        tokens[tc++] = wt;  /* register final token */
                        done = true;   /* set done flag */
                    }
                    // AH: end of bug fix.
                }

            }  // while()

            //...Log2.v("\nGenUtil.TokenizeLatLong(): D:             tokens[0] =        " + tokens[0]);
            //...Log2.v("\nGenUtil.TokenizeLatLong(): D:             tokens[1] =        " + tokens[1]);
            //...Log2.v("\nGenUtil.TokenizeLatLong(): D:             tokens[2] =        " + tokens[2]);
            //...Log2.v("\nGenUtil.TokenizeLatLong(): D:             tokens[3] =        " + tokens[3]);

            return (Constant.MAX_TOKENS);
        }

        /// <summary>
        /// This method converts a prescribed input string to a 32-bit floating-point value; this 
        /// decimal value is then rounded to a prescribed number of decimal places and returned.
        /// </summary>
        /// <param name="inputStr"> - the prescribed input string to be converted and rounded.</param>
        /// <param name="roundedNum"> - the converted and rounded decimal number.</param>
        /// <param name="nDecPlaces"> - the number of decimal places to be rounded to.</param>
        /// <returns></returns>
        public static int UtGetInputFloatRound(string inputStr, out float roundedNum, int nDecPlaces)
        {
            // 'out' requirement.
            roundedNum = 0.0f;

            string istring;
            float fInval;
            double dShift;

            /* get next token */
            istring = StrPok(inputStr, Constant.DELIMITER_CHAR);

            if (istring == null)
            {
                /* no more tokens */
                return (-1);
            }

            if (istring == "")
            {
                return (0); /* string length of zero, value is 0 */
            }

            /* convert string to float */
            try
            {
                fInval = Convert.ToSingle(istring);
            }
            catch
            {
                /* invalid conversion */
                Log2.e("\nGenUtil.UtGetInputFloatRound(): ERROR: invalid string for conversion to float: " + istring);
                return (-2);
            }

            /*	We round the value to the specified number of decimal places.  This is
            *	so that values will have the same value when exported and imported again. */
            dShift = Pow(10.0, (double)nDecPlaces);

            roundedNum = fInval * (float)dShift;

            if (roundedNum < 0.0)
            {
                roundedNum = (float)Ceiling(roundedNum - 0.5);
            }
            else
            {
                roundedNum = (float)Floor(roundedNum + 0.5);
            }
            roundedNum = (float)((double)roundedNum / dShift);

            /*	Check if rounding did occur and indicate it to the user */
            if (Abs(roundedNum - fInval) > 0.1 / dShift)
            {
                return (-3);        /*	Rounding did occur */
            }

            return ((int)istring.Length);
        }

        /// <summary>
        /// This method converts a prescribed input string to a 64-bit floating-point value; this 
        /// decimal value is then rounded to a prescribed number of decimal places and returned.
        /// </summary>
        /// <param name="inputStr"> - the prescribed input string to be converted and rounded.</param>
        /// <param name="roundedNum"> - the converted and rounded decimal number.</param>
        /// <param name="nDecPlaces"> - the number of decimal places to be rounded to.</param>
        /// <returns></returns>
        public static int UtGetInputDoubleRound(string inputStr, out double roundedNum, int nDecPlaces)
        {
            // 'out' requirement.
            roundedNum = 0.0;

            string istring;
            double fInval;
            double dShift;

            /* get next token */
            istring = StrPok(inputStr, Constant.DELIMITER_CHAR);

            if (istring == null)
            {
                /* no more tokens */
                return (-1);
            }

            if (istring == "")
            {
                return (0); /* string length of zero, value is 0 */
            }

            /* convert string to double */
            try
            {
                fInval = Convert.ToDouble(istring);
            }
            catch
            {
                /* invalid conversion */
                Log2.e("\nGenUtil.UtGetInputDoubleRound(): ERROR: invalid string for conversion to double: " + istring);
                return (-2);
            }

            /*	We round the value to the specified number of decimal places.  This is
            *	so that values will have the same value when exported and imported again. */
            dShift = Pow(10.0, nDecPlaces);

            roundedNum = fInval * dShift;

            if (roundedNum < 0.0)
            {
                roundedNum = Ceiling(roundedNum - 0.5);
            }
            else
            {
                roundedNum = Floor(roundedNum + 0.5);
            }
            roundedNum = roundedNum / dShift;

            /*	Check if rounding did occur and indicate it to the user */
            if (Abs(roundedNum - fInval) > 0.1 / dShift)
            {
                return (-3);        /*	Rounding did occur */
            }

            return ((int)istring.Length);
        }

        /// <summary>
        /// This method 'pokes' the next Int16 value from a 
        /// prescribed string; if the input string is null, empty or white space
        /// then the most recently cached non-trivial string is parsed.
        /// </summary>
        /// <param name="inputLine"> - the prescribed input string to be parsed.</param>
        /// <param name="number"> - the next integer number found.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>> 0: the length of the integer string detected.</item>
        /// <item>0: no integer string found.</item>
        /// </list>-1: already reached end of cached string.; 
        /// </returns>
        public static int UtGetInputSNum(string inputLine, out short number)
        {
            // 'out' requirement.
            number = 0;

            string istring;

            /* get next token */
            istring = StrPok(inputLine, Constant.DELIMITER_CHAR);

            if (istring == null)
            {
                /* no more tokens */
                return (-1);
            }

            if (istring == "")
            {
                return (0); /* string length of zero, value is 0 */
            }

            /* convert to short */
            try
            {
                number = Convert.ToInt16(istring);
            }
            catch
            {
                /* invalid conversion */
                Log2.e("\nGenUtil.UtGetInputSNum(): ERROR: invalid string for conversion to short: " + istring);
                return (-2);
            }

            return ((int)istring.Length);
        }

        /// <summary>
        /// This method 'pokes' the next Int32 value from a 
        /// prescribed string; if the input string is null, empty or white space
        /// then the most recently cached non-trivial string is parsed.
        /// </summary>
        /// <param name="inputLine"> - the prescribed input string to be parsed.</param>
        /// <param name="number"> - the next integer number found.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>> 0: the length of the integer string detected.</item>
        /// <item>0: no integer string found.</item>
        /// </list>-1: already reached end of cached string.; 
        /// </returns>
        public static int UtGetInputLNum(string inputLine, out int number)
        {
            // 'out' requirement.
            number = 0;

            string istring;

            /* get next token */
            istring = StrPok(inputLine, Constant.DELIMITER_CHAR);

            if (istring == null)
            {
                /* no more tokens */
                return (-1);
            }

            if (istring == "")
            {
                return (0); /* string length of zero, value is 0 */
            }

            /* convert string to int */
            try
            {
                number = Convert.ToInt32(istring);
            }
            catch
            {
                /* invalid conversion */
                Log2.e("\nGenUtil.UtGetInputLNum(): ERROR: invalid conversion: string to int: " + istring);
                return (-2);
            }

            return ((int)istring.Length);
        }

        /// <summary>
        /// This method returns the nearest (integer) number of <b>centiseconds</b> (0.01s) corresponding to a
        /// prescribed number of (decimal) degrees.
        /// </summary>
        /// <param name="satLongDeg"> - prescribed angle measured in (decimal) degrees.</param>
        /// <returns></returns>
        public static int FwIntSatlong(float satLongDeg)
        {
            int satLongSec;

            satLongSec = (int)(satLongDeg * 60 * 60 * 100);

            return (satLongSec);
        }

        /// <summary>
        /// This method calculates the frequency-dependent component of the Free Space
        /// propagation loss for a prescribed frequency (units: MHz).
        /// </summary>
        /// <param name="freq"> - prescribed frequency in MHz.</param>
        /// <returns></returns>
        public static double FreeSpaceFreqLoss(double freq)
        {
            return (freq > 0.0)
                   ? (double)(20 * Math.Log10(freq))
                   : 0.0;
        }

        /// <summary>
        /// This method calculates the constant and distance-dependent component of the Free Space
        /// propagation loss for a prescribed distance (units: Km).
        /// </summary>
        /// <param name="plength"> - prescribed distance in Km.</param>
        /// <returns></returns>
        public static double FreeSpaceDistLoss(double plength)
        {
            return (plength > 0.0)
                                    ? (double)(32.45 + (20 * Math.Log10(plength)))
                                    : 0.0;
        }

        /// <summary>
        /// This method returns the number of instances of a prescribed character
        /// in a prescribed string.
        /// </summary>
        /// <param name="str"> - the prescribed character.</param>
        /// <param name="c"> - the character to search for and count.</param>
        /// <returns></returns>
        public static int StrCnt(string str, char c)
        {
            int index;
            int count = 0;

            if (str == null) return -1;

            char[] chars = str.ToCharArray();

            for (index = 0; index < chars.Length; index++)
            {
                if (chars[index] == c) count++;
            }

            return count;
        }


        /// <summary>
        /// This method inputs an integer latitude value in centiseconds and outputs a
        /// a latitude string of the form dd-mm-ss.pp and a 'sense' string "N" or "S".
        /// </summary>
        /// <param name="latitude"> - input latitude in centiseconds.</param>
        /// <param name="strLatit"> - output: a latitude string of the form dd-mm-ss.pp</param>
        /// <param name="strLatitS"> - output: a 'sense' string "N" or "S".</param>
        public static void FwConvertLat(int latitude, out string strLatit, out string strLatitS)
        {
            int tempDeg, tempMin, tempSec, tempDec;

            if (latitude > 0)
            {
                strLatitS = "N";
            }
            else
            {
                strLatitS = "S";
                latitude *= -1;
            }

            tempDeg = latitude / 360000;
            tempMin = (latitude / 6000) - (tempDeg * 60);
            tempSec = (latitude / 100) - (((tempDeg * 60) + tempMin) * 60);
            tempDec = (latitude % 100);

            strLatit = String.Format("{0:D2}-{1:D2}-{2:D2}.{3:D2}", tempDeg, tempMin, tempSec, tempDec);

            return;
        }


        /// <summary>
        /// This method inputs an integer longitude value in centiseconds and outputs a
        /// a longitude string of the form ddd-mm-ss.pp and a 'sense' string "E" or "W".
        /// </summary>
        /// <param name="longitude"> - input longitude in centiseconds.</param>
        /// <param name="strLongit"> - output: a longitude string of the form ddd-mm-ss.pp</param>
        /// <param name="strLongitS"> - output: a 'sense' string "N" or "S".</param>
        public static void FwConvertLong(int longitude, out string strLongit, out string strLongitS)
        {
            int degrees;
            int minutes;
            int seconds;
            int hundreds;

            if (longitude < 0)
            {
                strLongitS = "E";
                longitude *= -1;
            }
            else
            {
                strLongitS = "W";
            }

            degrees = longitude / 360000;
            minutes = (longitude / 6000) - (degrees * 60);
            seconds = (longitude / 100) - (((degrees * 60) + minutes) * 60);
            hundreds = (longitude % 100);

            strLongit = String.Format("{0:D3}-{1:D2}-{2:D2}.{3:D2}", degrees, minutes, seconds, hundreds);

            return;
        }

        private static string mGlbFilesDir = "";

        /// <summary>
        /// This method returns the path of the MICS 'files' directory, e.g. 
        /// "d:\prod\files\"
        /// </summary>
        /// <returns></returns>
        public static string UtGetFilesDir()
        {
            if (String.IsNullOrWhiteSpace(mGlbFilesDir))
            {
                mGlbFilesDir = Ssutil.GetMicsRoot(Info.DbName) + "files\\";
            }

            return mGlbFilesDir.ToString();
        }


        /// <summary>
        /// This method returns a date string that can be used in a file name;
        /// the input date string must be of the form 'yyyy.mm.dd' and the time 
        /// string as 'hh:mm'; the output string has the form yyyymmdd_hhmm.
        /// </summary>
        /// <param name="cDate"> - input date string of the form 'yyyy.mm.dd'</param>
        /// <param name="cTime"> - input time string of the form 'hh:mm'</param>
        /// <returns></returns>
        public static string FileNameTime(string cDate, string cTime)
        {
            string cFileNameTimeBuf = "";

            cFileNameTimeBuf += cDate.Substring(0, 4);
            cFileNameTimeBuf += cDate.Substring(5, 2);
            cFileNameTimeBuf += cDate.Substring(8, 2);

            cFileNameTimeBuf += "_";

            cFileNameTimeBuf += cTime.Substring(0, 2);
            cFileNameTimeBuf += cTime.Substring(3, 2);

            return cFileNameTimeBuf;
        }

        /// <summary>
        /// This method returns the name of the MICS program to use for
        /// a prescribed table type and program type.
        /// </summary>
        /// <param name="tabType"></param>
        /// <param name="pgmType"></param>
        /// <param name="pgmName"></param>
        /// <returns></returns>
        public static int UtGetPgmName(int tabType,
                                        int pgmType,        /* EDIT, VALIDATE or PRINT */
                                        out string pgmName)
        {
            // 'out' requirement.
            pgmName = "";

            Cvt p = Cvt.SearchCvtTab(tabType);

            if (p == null)
            {
                /* table type not found */
                return (Constant.FAILURE);
            }

            if (p.pgmNames[pgmType] == null)
            {
                /* No program defined */
                return (Constant.FAILURE);
            }

            pgmName = p.pgmNames[pgmType];
            return (Constant.SUCCESS);
        }

        public static Enums.AnteUse GetDirType(string actualUseOfAntenna)
        {

            Enums.AnteUse eRet = Enums.AnteUse.eUNKNOWN;


            if (actualUseOfAntenna.Equals("TX") || actualUseOfAntenna.Equals("STX"))
            {
                eRet = Enums.AnteUse.eTX;
            }
            else if (actualUseOfAntenna.Equals("RX") || actualUseOfAntenna.Equals("DV1") || actualUseOfAntenna.Equals("DV2"))
            {
                eRet = Enums.AnteUse.eRX;
            }
            else if (actualUseOfAntenna.Equals("TR"))
            {
                eRet = Enums.AnteUse.eTR;
            }

            return eRet;
        }

        /// <summary>
        /// This method returns true if the directions of both ends of a link are logically
        /// compatible, e.g. if one end of a link is transmit then the other end must be 
        /// receive or transmit/retrieve etc.
        /// </summary>
        /// <param name="eThisEnd"></param>
        /// <param name="cOtherEnd"></param>
        /// <returns></returns>
        public static bool IsOEndType(Enums.AnteUse eThisEnd, string cOtherEnd)
        {

            bool bRet;
            Enums.AnteUse eOtherEnd = GetDirType(cOtherEnd);

            // This is verbatim from the legacy C++ code and uses a bitwise OR
            // operation that relies on the default numbering of the enumeration
            // EdirTXRX being {0, 1, 2, 3} corresponding to {?, TX, RX, TR/RX}.           
            if (((int)eThisEnd | (int)eOtherEnd) == (int)Enums.AnteUse.eTR)
            {
                /*	Valid  */
                bRet = true;
            }
            else
            {
                bRet = false;
            }

            return bRet;
        }






    }
}
