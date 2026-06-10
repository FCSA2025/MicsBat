# Documented File: Strings.cs
**Repository Path:** `_NewLib\Strings.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// Provides MICS-specific string and character tests and operations that
    /// are not already provided by the standard .NET String class.
    /// </summary>
    public class Strings
    {
        /// <summary>
        /// This method loosely mimics the 'C' library function strchr().
        /// </summary>
        /// <param name="str"> - string to be searched.</param>
        /// <param name="character"> - character to search for.</param>
        /// <returns>
        /// Returns 666 + index of the first occurrence of character in the string str;
        /// if the character does not occur in the string then the returned value is zero.
        /// </returns>
        public static int StrChr(string str, char character)
        {
            int nRet = 0;

            if (String.IsNullOrEmpty(str))
            {
                return nRet;
            }
            else
            {
                int index = str.IndexOf(character);
                if (index >= 0)
                {
                    nRet = 666 + index;
                }
            }

            return nRet;
        }

        /// <summary>
        /// This method replicates the 'C' library function strcmp().
        /// </summary>
        /// <param name="str1"> - 1st. string to be compared.</param>
        /// <param name="str2"> - 2nd. string to be compared.</param>
        /// <returns>
        /// &lt; 0 :  the first character that does not match has a lower value in str1 than in str2.<br />
        /// =    0 :  the strings are identical.<br/>
        /// &gt; 0 :  the first character that does not match has a greater value in str1 than in str2
        /// </returns>
        public static int StrCmp(string str1, string str2)
        {
            int nRet = Int32.MinValue;

            if (String.IsNullOrEmpty(str1) || String.IsNullOrEmpty(str2))
            {
                return nRet;
            }

            return String.Compare(str1, str2, StringComparison.Ordinal);
        }

        /// <summary>
        /// This method replicates the 'C' library function strncmp().
        /// </summary>
        /// <param name="str1"> - 1st. string to be compared.</param>
        /// <param name="str2"> - 2nd. string to be compared.</param>
        /// <param name="num"> - maximum number of characters to compare.</param>
        /// <returns>
        /// &lt; 0 :  the first character that does not match has a lower value in str1 than in str2.<br />
        /// =    0 :  the strings are identical.<br/>
        /// &gt; 0 :  the first character that does not match has a greater value in str1 than in str2
        /// </returns>
        public static int StrnCmp(string str1, string str2, int num)
        {
            //...Log2.v("\nstr1 = " + Strings.AddBars(str1));
            //...Log2.v("\nstr2 = " + Strings.AddBars(str2));
            //...Log2.v("\nnum = " + num);

            int nRet = Int32.MinValue;

            // Handle the pathological case of either str1 or str2 being null.
            if (str1 == null || str2 == null)
            {
                nRet = Int32.MinValue;
            }
            // Handle the pathological case of num = < 0.
            else if (num < 0)
            {
                nRet = Int32.MinValue;
            }
            // Handle the pathological case of num = 0.
            else if (num == 0)
            {
                nRet = 0;
            }
            // Handle the regular case.
            else if (num > 0)
            {
                string str1_Truncated = str1.Substring(0, Math.Min(num, str1.Length));
                string str2_Truncated = str2.Substring(0, Math.Min(num, str2.Length));

                nRet = String.Compare(str1_Truncated, str2_Truncated, StringComparison.Ordinal);
            }

            return nRet;
        }

        /// <summary>
        /// This method closely mimics the 'C' library function strncpy().
        /// </summary>
        /// <param name="strTgt"> - the target string.</param>
        /// <param name="strSrc"> - the source string.</param>
        /// <param name="num"> - maximum number of characters to copy.</param>
        /// <returns>
        /// zero.
        /// </returns>
        public static int StrnCpy(out string strTgt, string strSrc, int num)
        {
            // 'out' requirement.
            strTgt = "";

            if (String.IsNullOrEmpty(strSrc))
            {
                strTgt = strSrc;
            }
            else if (num == 0)
            {
                strTgt = "";
            }
            else if (num > 0)
            {
                strTgt = strSrc.Substring(0, Math.Min(num, strSrc.Length));
            }

            return 0;
        }

        /// <summary>
        /// Delimit a string using the 'vertical bar' character, e.g. "hello  "
        /// becomes "|hello  |"
        /// </summary>
        /// <param name="str"> - string to be delimitted.</param>
        /// <returns>Delimitted string.</returns>
        public static string AddBars(string str)
        {
            string result = null;
            if (str == null)
            {
                result = "null";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("|");
                sb.Append(str);
                sb.Append("|");
                result = sb.ToString();
            }
            return result;
        }

        /// <summary>
        /// Return a string comprising a prescribed character repeated a prescribed number of times.
        /// </summary>
        /// <param name="c"> - character.</param>
        /// <param name="nRepeat"> - length of string.</param>
        /// <returns>Repeated characters.</returns>
        public static string RepeatedChar(char c, int nRepeat)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < nRepeat; i++)
            {
                sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Tests whether a string is not equal to "N" or "U".
        /// </summary>
        /// <param name="str"> - string to test.</param>
        /// <returns>true or false.</returns>
        public static bool Not_N_or_U(string str)
        {
            bool not_N = !str.Equals("N");
            bool not_U = !str.Equals("U");
            return (not_N && not_U);
        }

        /// <summary>
        /// Tests whether a character is alphabetic, ie A-Z or a-z; it mimics the
        /// C++ function 'isalpha()'.
        /// </summary>
        /// <param name="c"> - character to test.</param>
        /// <returns>true or false.</returns>
        public static bool IsAlphabetic(char c)
        {
            bool isLowerCase = (c >= 97) && (c <= 122);
            bool isUpperCase = (c >= 65) && (c <= 90);

            return isLowerCase || isUpperCase;
        }

        /// <summary>
        /// Tests whether a character is a decimal digit, i.e. 0, 1, 2, ... 9;
        /// it mimics the C++ function 'isdigit()'.
        /// </summary>
        /// <param name="c"> - character to test.</param>
        /// <returns>true or false.</returns>
        public static bool IsDigit(char c)
        {
            return (c >= 48) && (c <= 57);
        }

        /// <summary>
        /// Tests whether a character is a punctuation character; it mimics the
        /// C++ function 'ispunct()' .
        /// </summary>
        /// <param name="c"> - character to test.</param>
        /// <returns>true or false.</returns>
        public static bool IsPunct(char c)
        {
            bool result = false;

            int asciiCode = (int)c;

            if (Maths.InRange(0x21, 0x2F, asciiCode) ||     // !"#$%&'()*+,-./
                Maths.InRange(0x3A, 0x40, asciiCode) ||     // :;<=>?@
                Maths.InRange(0x5B, 0x60, asciiCode) ||     // [\]^_`
                Maths.InRange(0x7B, 0x7E, asciiCode))       // {|}~
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Tests whether a character is one of 'A', 'B', 'D', 'N' or 'U'.
        /// </summary>
        /// <param name="c"> - character to test.</param>
        /// <returns>true or false.</returns>
        public static bool CharIsOneOfABDNU(char c)
        {

            bool result = false;
            switch (c)
            {
                case 'A':
                case 'B':
                case 'D':
                case 'N':
                case 'U':
                    result = true;
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }

        /// <summary>
        /// Tests whether a character is one of 'A', 'B', or 'C'.
        /// </summary>
        /// <param name="c"> - character to test.</param>
        /// <returns>true or false.</returns>
        public static bool CharIsOneOfABC(char c)
        {

            bool result = false;
            switch (c)
            {
                case 'A':
                case 'B':
                case 'C':
                    result = true;
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }

        /// <summary>
        /// Tests whether a character is one of '0', '1', ... '8'.
        /// </summary>
        /// <param name="c"> - character to test.</param>
        /// <returns>true or false.</returns>
        public static bool CharIsOneOf012345678(char c)
        {

            bool result = false;
            switch (c)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                    result = true;
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }

        /// <summary>
        /// Tests whether a character occurs in the string "HVBLRC ".
        /// </summary>
        /// <param name="c"> - character to test.</param>
        /// <returns>true or false.</returns>
        public static bool CharIsOneOfHVBLRCsp(char c)
        {

            bool result = false;
            switch (c)
            {
                case 'H':
                case 'V':
                case 'B':
                case 'L':
                case 'R':
                case 'C':
                case ' ':
                    result = true;
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }

        /// <summary>
        /// This method returns true if the first character of a string
        /// is equal to '=', '$', '%', or ';'.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool CallSignIsFicticious(string str)
        {
            bool result = false;
            if (!String.IsNullOrEmpty(str))
            {
                switch (str[0])
                {
                    case '=':
                    case '$':
                    case '%':
                    case ';':
                        result = true;
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// This method returns true if the first character of a string
        /// is equal to a prescribed character.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool FirstCharIs(string str, char c)
        {
            bool result = false;
            if (!String.IsNullOrEmpty(str))
            {
                char firstChar = str[0];
                if (firstChar == c)
                {
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// This method returns the first character of a string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static char FirstChar(string str)
        {
            char result = '\0';
            if (!String.IsNullOrEmpty(str))
            {
                result = str[0];
            }
            return result;
        }

        /// <summary>
        /// This method returns true if the last character of a string
        /// is equal to a prescribed character.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool LastCharIs(string str, char c)
        {
            bool result = false;
            if (!String.IsNullOrEmpty(str))
            {
                int indexOfLastChar = str.Length - 1;
                char lastChar = str[indexOfLastChar];
                if (lastChar == c)
                {
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// This method returns the last character of a string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static char LastChar(string str)
        {
            char result = '\0';
            if (!String.IsNullOrEmpty(str))
            {
                int indexOfLastChar = str.Length - 1;
                result = str[indexOfLastChar];
            }
            return result;
        }

        /// <summary>
        /// This method returns true if a prescribed string contains only
        /// valid Windows filename characters.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasValidFileNameChars(string str)
        {
            bool allValid = true;

            if (String.IsNullOrWhiteSpace(str)) return false;

            char[] invalidChars = Path.GetInvalidFileNameChars();

            foreach (char c in str)
            {
                allValid &= !invalidChars.Contains<char>(c);
                if (!allValid) break;
            }

            return allValid;
        }

        /// <summary>
        /// This method places a quotation mark character at the
        /// start and end of a prescribed string.  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EnQuote(string str)
        {
            return "\"" + str + "\"";
        }

        /// <summary>
        /// This method concatanates an array of strings into a single string
        /// with a space inserted between elements.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string CommandLineFromArgs(string[] args)
        {
            string cl = "";
            foreach (string arg in args)
            {
                cl += arg + " ";
            }
            return cl;
        }

        /// <summary>
        /// This method returns the prescribed string with its first character removed; if the 
        /// prescribed string is null then the returned string is null; if empty then 
        /// empty is returned; if length is 1 an empty string is returned.
        /// </summary>
        /// <param name="str"> - the prescribed input string.</param>
        /// <returns> - the input string with its first character removed.</returns>
        public static string DropFirstChar(string str)
        {
            string result = "";

            if (String.IsNullOrEmpty(str))
            {
                result = str;
            }
            else if (str.Length > 1)
            {
                result = str.Substring(1, str.Length - 1);
            }

            return result;
        }

        /// <summary>
        /// This method returns the prescribed string with its last character removed; if the 
        /// prescribed string is null then the returned string is null; if empty then 
        /// empty is returned; if length is 1 an empty string is returned.
        /// </summary>
        /// <param name="str"> - the prescribed input string.</param>
        /// <returns> - the input string with its last character removed.</returns>
        public static string DropLastChar(string str)
        {
            string result = "";

            if (String.IsNullOrEmpty(str))
            {
                result = str;
            }
            else if (str.Length > 1)
            {
                result = str.Substring(0, str.Length - 1);
            }

            return result;
        }

        /// <summary>
        /// This method inputs an array of strings and outputs a single string
        /// that concatenates the array elements with a single space character
        /// inserted between them; this reverse engineers the command line
        /// arguments given the C# args[].
        /// </summary>
        /// <param name="args"> - prescribed array of strings.</param>
        /// <returns></returns>
        public static string OneLine(string[] args)
        {
            string line = "";

            if (args != null)
            {
                foreach (string arg in args)
                {
                    line += arg + " ";
                }
            }

            return line;
        }

        /// <summary>
        /// This method converts an unsigned 32-bit integer to its
        /// representation as a binary number string.
        /// </summary>
        /// <param name="Decimal"> - prescribed Uint32 integer.</param>
        /// <returns></returns>
        public static string ToBinary(uint Decimal)
        {
            // Declare a few variables we're going to need
            uint BinaryHolder;
            char[] BinaryArray;
            string BinaryResult = "";

            while (Decimal > 0)
            {
                BinaryHolder = Decimal % 2;
                BinaryResult += BinaryHolder;
                Decimal = Decimal / 2;
            }

            // Pad with zeros until the string length is 32.
            for (int i = BinaryResult.Length + 1; i <= 32; i++)
            {
                BinaryResult += "0";
            }

            // The algoritm gives us the binary number in reverse order (mirrored)
            // We store it in an array so that we can reverse it back to normal
            BinaryArray = BinaryResult.ToCharArray();
            Array.Reverse(BinaryArray);
            BinaryResult = new string(BinaryArray);

            return BinaryResult;
        }

        /// <summary>
        /// This method converts an unsigned 32-bit integer to its
        /// representation as a binary number string annotated above with
        /// a 'ruler' (0123456789012...).
        /// </summary>
        /// <param name="number"> - prescribed Uint32 integer.</param>
        /// <returns></returns>
        public static string ToBinaryWR(uint number)
        {
            string str = "";

            str += MakeRuler(32) + "\n" + ToBinary(number);

            return str;
        }

        /// <summary>
        /// This method creates a string of digits of the repeating form
        /// 01234567890123...
        /// </summary>
        /// <param name="numChars"> - the prescribed number of digits to be in the string.</param>
        /// <returns></returns>
        public static string MakeRuler(int numChars)
        {
            string ruler = "";

            if (numChars < 1)
            {
                return "";
            }

            for (int i = 0; i < numChars; i++)
            {
                int digit = i % 10;

                ruler += Convert.ToString(digit);
            }

            return ruler;
        }

        /// <summary>
        /// This method returns true if the prescribed string can be converted
        /// to a number (decimal or integer); otherwise false.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumeric(string str)
        {
            bool isNumeric = false;

            try
            {
                double d = Convert.ToDouble(str);
                isNumeric = true;
            }
            catch
            {
                isNumeric = false;
            }

            return isNumeric;
        }

        /// <summary>
        /// This method returns true if the prescribed string can be converted
        /// to an integer; otherwise false.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsInteger(string str)
        {
            bool isInteger = false;

            try
            {
                double d = Convert.ToInt64(str);
                isInteger = true;
            }
            catch
            {
                isInteger = false;
            }

            return isInteger;
        }

        /// <summary>
        /// This method returns a string of qty. 96 hex digits giving the SHA384 hash code for a 
        /// prescribed message string; this can be used to compare two very long strings for exact equality
        /// without having to compare individual characters.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string HashSHA384(string message)
        {
            string result = "";

            // The input message string is encoded as a sequence of 2-byte UniCode characters.
            // We need to convert the message into a byte[] of ASCII values.
            byte[] messageBytes = ASCIIEncoding.ASCII.GetBytes(message);

            // Now perform the SHA-384 hash.
            // The hash value is a 384/8 = 48 element byte array.
            SHA384 shaM = new SHA384Managed();
            byte[] hashBytes = shaM.ComputeHash(messageBytes);

            // Finally, convert hashBytes[] into a C# string, of length 2 * 48 = 96, of hex characters.
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(String.Format("{0:x2}", hashBytes[i]));
            }
            result = sb.ToString();

            //...Log2.v("\nStrings.SHA384Hash(): hash = " + result);

            return result;
        }

        /// <summary>
        /// This method returns a string equal to a prescribed string that
        /// has any trailing alphabetic [a-zA-Z] characters removed.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string RemoveAlphaCharsFromEnd(string name)
        {
            string result = "";

            if (!String.IsNullOrWhiteSpace(name))
            {
                int numAlphaCharsAtEnd = 0;
                int length = name.Length;

                for (int i = 0; i < length; i++)
                {
                    // Backwards enumeration used.
                    char c = name[length - 1 - i];

                    if (IsAlphabetic(c))
                    {
                        numAlphaCharsAtEnd++;
                    }
                    else break;
                }

                result = name.Substring(0, length - numAlphaCharsAtEnd);
            }

            return result;
        }

        /// <summary>
        /// This method returns a string that is the concatenation of all
        /// the elements of a prescribed list of strings.
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        public static string ListOfStringsToString(List<string> strings)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < strings.Count; i++)
            {
                if (i == 0)
                {
                    sb.Append(strings[i]);
                }
                else
                {
                    sb.Append("\n" + strings[i]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string that is the CSV concatenation of all
        /// the elements of a prescribed list of strings.
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        public static string ListOfStringsToCSV(List<string> strings)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < strings.Count; i++)
            {
                if (i == 0)
                {
                    sb.Append(strings[i]);
                }
                else
                {
                    sb.Append(", " + strings[i]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string that is the CSV concatenation of all
        /// the elements of a prescribed list of strings.
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        public static string ArrayOfStringsToCSV(string[] strings)
        {
            if (strings == null || strings.Length == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < strings.Length; i++)
            {
                if (i == 0)
                {
                    sb.Append(strings[i]);
                }
                else
                {
                    sb.Append(", " + strings[i]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string that is an intelligently truncated version of 
        /// a prescribed string; if the prescribed string is shorter than the requested
        /// truncated length then the prescribed input value is returned as the result; the method provides
        /// guards against the prescribed input string being null or empty.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="truncatedLength"></param>
        /// <returns></returns>
        public static string Truncate(string original, int truncatedLength)
        {
            string result = "";

            // Sanity checks.
            if (String.IsNullOrEmpty(original)) return original;
            if (truncatedLength == 0) return "";
            if (truncatedLength < 0) return original;

            // Perform the truncation.
            result = original.Length > truncatedLength ? original.Substring(0, truncatedLength) : original;

            return result;
        }

        /// <summary>
        /// This method returns true if a prescribed string is found as an
        /// element of a prescribed list of strings; the matching is case-sensitive.
        /// </summary>
        /// <param name="list"> - prescribed list of strings.</param>
        /// <param name="searchStr"> - prescribed string to search for.</param>
        /// <returns></returns>
        public static bool IsInList(List<String> list, String searchStr)
        {
            if (list != null)
            {
                foreach (String listItem in list)
                {
                    if (listItem.Equals(searchStr))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// This method returns true if a prescribed string is found as an
        /// element of a prescribed list of strings; the matching is not sensitive to case.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsInListCaseInsensitive(List<String> list, String name)
        {
            foreach (String listItem in list)
            {
                if (listItem.ToLower().Equals(name.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This method inputs a List of strings and returns a List of strings that
        /// contains no duplicated string elements; string elements only appear once.
        /// </summary>
        /// <param name="InList"></param>
        /// <returns></returns>
        public static List<string> RemoveDuplicates(List<string> InList)
        {
            List<string> ResultList = new List<string>();
            foreach (string str in InList)
            {
                if (!ResultList.Contains(str))
                {
                    ResultList.Add(str);
                }
            }
            return ResultList;
        } //Method

        /// <summary>
        /// This method returns a string with a prescribed length that comprises all space " " characters.
        /// </summary>
        /// <param name="numSpaces"></param>
        /// <returns></returns>
        public static string Spaces(int numSpaces)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < numSpaces; i++) sb.Append(" ");

            return sb.ToString();
        }

        /// <summary>
        /// This method is passed a string that may contain diacritics and returns a 
        /// string with all the diacritics removed; for example 'Crme brle' is 
        /// returned as 'Creme brulee'.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveDiacritics(string text)
        {
            /// This routine converts string to a form where any accent is moved as a character after the letter
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            /// It then reads through this string and copies its contents, less the accents, to a new string
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            /// Finally it converts this last back to a normal form
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        ///     Calculate the difference between 2 strings using the Levenshtein distance algorithm
        /// </summary>
        /// <param name="source1">First string</param>
        /// <param name="source2">Second string</param>
        /// <returns></returns>
        public static int LevenshteinDistance(string source1, string source2) //O(n*m)
        {
            source1 = source1.Trim().ToUpper();
            source2 = source2.Trim().ToUpper();

            var source1Length = source1.Length;
            var source2Length = source2.Length;

            var matrix = new int[source1Length + 1, source2Length + 1];

            // First calculation, if one entry is empty return full length
            if (source1Length == 0)
                return source2Length;

            if (source2Length == 0)
                return source1Length;

            // Initialization of matrix with row size source1Length and columns size source2Length
            for (var i = 0; i <= source1Length; matrix[i, 0] = i++) { }
            for (var j = 0; j <= source2Length; matrix[0, j] = j++) { }

            // Calculate rows and collumns distances
            for (var i = 1; i <= source1Length; i++)
            {
                for (var j = 1; j <= source2Length; j++)
                {
                    var cost = (source2[j - 1] == source1[i - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            // return result
            return matrix[source1Length, source2Length];
        }

        /// <summary>
        /// This method return the substring corresponding to the first word
        /// of a sentence; leading space is trimmed and the first word is delimitted
        /// by the first blank space between the first and second words (if any). 
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public static string GetFirstWord(string sentence)
        {
            string firstWord = "";

            // Isolate the first word.
            string[] words = sentence.Trim().Split(' ');
            firstWord = words[0];

            return firstWord;
        }

        /// <summary>
        /// This method is passed a string and then parses it character-by-character and
        /// removes any characters that are not in the ranges [a-z], [A-Z] or [0-9].
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveNonAlphaNumeric(string str)
        {
            string result = "";

            List<char> alphaNumerics = new List<char>();

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                bool isNumber = (c >= 48) && (c <= 57);
                bool isAlpha = ((c >= 65) && (c <= 90)) || ((c >= 97) && (c <= 122));

                if (isNumber || isAlpha)
                {
                    alphaNumerics.Add(c);
                }
            }

            result = new String(alphaNumerics.ToArray());

            return result;
        }

        /// <summary>
        /// This method inputs a single line from a C# source file and returns a string 
        /// that can safely be parsed to detect {} delimited blocks; the issue is that 
        /// string literals "..." can themselves contain characters like '"' that are 
        /// preceeded by the escape character '\' and/or contain '{' or '}' characters 
        /// that would confuse any block parsing algorithm.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string MakeSafeForBlockParsing(string line)
        {
            // Assumptions:
            //              - the string is a valid line of C# code.
            //              - there are no embedded comments.

            string result = "";

            // Address the pathological case of a char being set to '"'.
            result = line.Replace("'\"'", "'$'");

            CharParser cp = new CharParser(result);

            bool inQuote = false;
            bool isAmpersanded = false;
            List<char> charList = new List<char>();

            do
            {
                if (!inQuote && cp.ThisChar == '"')
                {
                    inQuote = true;
                    if (cp.PreviousChar == '@') isAmpersanded = true;
                    charList.Add(cp.ThisChar);
                }
                else if (inQuote && isAmpersanded && cp.ThisChar == '"')
                {
                    charList.Add(cp.ThisChar);
                    inQuote = false;
                    isAmpersanded = false;
                }
                else if (inQuote && !isAmpersanded && cp.PreviousChar != '\\' && cp.ThisChar == '"')
                {
                    charList.Add(cp.ThisChar);
                    inQuote = false;
                    isAmpersanded = false;
                }
                else if (inQuote)
                {
                    charList.Add('$');
                }
                else
                {
                    charList.Add(cp.ThisChar);
                }

            } while (cp.Advance());

            return new string(charList.ToArray());
        }

        public static bool EndsWith(string strToTest, string strToFind, bool caseSensitive)
        {
            bool result = false;

            if (String.IsNullOrEmpty(strToTest)) return result;
            if (String.IsNullOrEmpty(strToFind)) return result;

            if (strToFind.Length > strToTest.Length) return result;

            if (!caseSensitive)
            {
                strToTest = strToTest.ToUpper();
                strToFind = strToFind.ToUpper();
            }

            int startPos = strToTest.Length - strToFind.Length;

            result = strToTest.Substring(startPos, strToFind.Length).Equals(strToFind);

            return result;
        }



    }
}

```
