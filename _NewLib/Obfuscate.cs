using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// This class provides methods that can obfuscate (encrypt) a 'clear text' string
    /// and then de-obfuscate it to retrieve the original string; the obfuscation algorithm
    /// is a simple character-by-character encryption and decryption using a prescribed 'key'
    /// string; the intended use of this class is to obsfuscate passwords that would otherwise
    /// appear in clear text in .config files etc.
    /// </summary>
    public static class Obfuscate
    {
        private static Encoding e = Encoding.GetEncoding(1252);
        //private static string mKey = "In the beginning God created the heavens and the earth.";
        private static string mKey = "eRD6vN86pKuqSiFYyYGfrZ2LZe7CNolA";

        private const int LO_VIS = 32;
        private const int HI_VIS = 126;
        private const int RANGE = HI_VIS - LO_VIS + 1;

        /// <summary>
        /// This method sets the obfuscation key to the prescribed string; if this method is not called
        /// the Hide() and UnHide() methods use a default key value that is a private const string member
        /// whose length is 32 and whose characters were randomly.
        /// </summary>
        /// <param name="keyString"></param>
        public static void Setkey(string keyString)
        {
            // We need to hard fail if the keyString is null, empty or all whitespace.
            if (String.IsNullOrWhiteSpace(keyString))
            {
                Log2.e("\n\nObfuscate.Setkey(): ERROR: prescribed keyString is null, empty or all whitespace.");
                string str = Error.MsgForCode(Error.OBFUSCATIONSETKEYFAILED);
                Console.Write("\n" + str);
                Application.ExitQuietly(Error.OBFUSCATIONSETKEYFAILED);
            }

            mKey = keyString;
        }

        /// <summary>
        /// This method receives a prescribed 'clear text' string and returns an 
        /// obfuscated string of the same length that is constructed using the current
        /// key value; a return value of null indicates that the obfuscation attempt failed.
        /// </summary>
        /// <param name="clearText"></param>
        /// <returns></returns>
        public static string Hide(string clearText)
        {
            // Validate the input string.
            // All characters must be visible, i.e. ASCII codes [32, 126].
            // There are qty. 95 visible characters.
            // Note that the space character (ASCII 32) is deemed to be visible.
            if (String.IsNullOrEmpty(clearText) || !AreAllCharactersVisible(clearText)) return null;

            int KEY_LENGTH = mKey.Length;
            int NBYTES = clearText.Length;

            byte[] keyBytes = e.GetBytes(mKey);
            byte[] clearTextBytes = e.GetBytes(clearText);

            byte[] obfuscatedTextBytes = new byte[NBYTES];

            // Shift 'down' plain text visible characters from [32, 126] to [0, 94].
            Shift(ref keyBytes);
            Shift(ref clearTextBytes);

            // Obfuscate.
            for (int i = 0; i < NBYTES; i++)
            {
                obfuscatedTextBytes[i] = (byte)((clearTextBytes[i] + keyBytes[i % KEY_LENGTH]) % RANGE);

                //Console.Write("\n{0}     {1}", clearTextBytes[i], obfuscatedTextBytes[i]);
            }

            // Shift 'up' obfuscated text from [0, 94] to [32, 126] to ensure its visibility.
            ReverseShift(ref obfuscatedTextBytes);

            return e.GetString(obfuscatedTextBytes);
        }

        /// <summary>
        /// This method receives a prescribed, previously obfuscated, string and returns the 
        /// original clear text string of the same length that is re-constructed using the current
        /// key value; a return value of null indicates that the de-obfuscation attempt failed.
        /// </summary>
        /// <param name="obfuscatedText"></param>
        /// <returns></returns>
        public static string UnHide(string obfuscatedText)
        {
            // Validate the input string.
            // All characters must be visible, i.e. ASCII codes [32, 126].
            // There are qty. 95 visible characters.
            // Note that the space character (ASCII 32) is deemed to be visible.
            if (String.IsNullOrEmpty(obfuscatedText) || !AreAllCharactersVisible(obfuscatedText)) return null;

            int KEY_LENGTH = mKey.Length;
            int NBYTES = obfuscatedText.Length;

            byte[] keyBytes = e.GetBytes(mKey);
            byte[] obfuscatedTextBytes = e.GetBytes(obfuscatedText);

            byte[] clearTextBytes = new byte[NBYTES];

            // Shift 'down' plain text visible characters from [32, 126] to [0, 94].
            Shift(ref keyBytes);
            Shift(ref obfuscatedTextBytes);

            // De-obfuscate.
            for (int i = 0; i < NBYTES; i++)
            {
                // We need to be careful about the subtraction giving a negative value.
                // The % operator can return negative values, i.e. it ranges over [-94, +94].
                int n = (obfuscatedTextBytes[i] - keyBytes[i % KEY_LENGTH]) % RANGE;

                // Ensure range is [0, 94].
                n = n < 0 ? n + RANGE : n;

                clearTextBytes[i] = (byte)n;

                //Console.Write("\n{0}    {1}     {2}", obfuscatedTextBytes[i], n, clearTextBytes[i]);
            }

            // Shift 'up' de-obfuscated text from [0, 94] to [32, 126] to ensure its visibility.
            ReverseShift(ref clearTextBytes);

            return e.GetString(clearTextBytes);
        }

        /// <summary>
        /// This method return true if all of the characters of a prescribed string
        /// have code values (i.e. ASCII values) that are in the range [32, 126]; note
        /// that code value 32 is the space character and is considered to be 'visible'.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool AreAllCharactersVisible(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return false;
            }

            bool result = true;

            byte[] byteArray = e.GetBytes(str);

            foreach (byte b in byteArray)
            {
                if ((b < LO_VIS) || (b > HI_VIS))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// This method inputs a prescribed string, whose characters are in the visible
        /// range [32, 126] and substracts 32 from each code value; the method returns 
        /// a string whose character code values are in the range [0, 94].
        /// </summary>
        /// <param name="byteArray"></param>
        private static void Shift(ref byte[] byteArray)
        {
            for (int i = 0; i < byteArray.Length; i++)
            {
                byteArray[i] -= LO_VIS;
            }
        }

        /// <summary>
        /// This method inputs a prescribed string, whose characters are in the
        /// range [0, 94] and adds 32 to each code value; the method returns 
        /// a string whose character code values are in the range [32, 126].
        /// </summary>
        /// <param name="byteArray"></param>
        private static void ReverseShift(ref byte[] byteArray)
        {
            for (int i = 0; i < byteArray.Length; i++)
            {
                byteArray[i] += LO_VIS;
            }
        }

        /// <summary>
        /// This method returns a string that lists all of the encodings supported 
        /// in the current environment itemized by Unicode CodePage number, infoName and DisplayPage
        /// using the C# system call Encoding.GetEncodings().
        /// by 
        /// </summary>
        /// <returns></returns>
        public static string EncodingsInfoAsString()
        {
            StringBuilder sb = new StringBuilder();

            // Print the header.
            sb.Append("Info.CodePage      ");
            sb.Append("Info.Name                    ");
            sb.Append("Info.DisplayName");
            sb.Append("\n");

            // Display the EncodingInfo names for every encoding, and compare with the equivalent Encoding names.
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                Encoding e = ei.GetEncoding();

                sb.Append(String.Format("{0,-15}", ei.CodePage));
                if (ei.CodePage == e.CodePage)
                    sb.Append("    ");
                else
                    sb.Append("*** ");

                sb.Append(String.Format("{0,-25}", ei.Name));
                if (ei.CodePage == e.CodePage)
                    sb.Append("    ");
                else
                    sb.Append("*** ");

                sb.Append(String.Format("{0,-25}", ei.DisplayName));
                if (ei.CodePage == e.CodePage)
                    sb.Append("    ");
                else
                    sb.Append("*** ");

                sb.Append("\n");
            }

            return sb.ToString();
        }







    }
}
