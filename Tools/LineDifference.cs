using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{

    /// <summary>
    /// 
    /// </summary>
    public class LineDifference
    {
        public enum Outcome { IDENTICAL, DIFFERENT, A_NOTFOUND, B_NOTFOUND, AB_NOTFOUND }

        // Line and column numbers both start at 1.
        public int lineNumber = 0;
        public int colNumber = 0;
        public string lineA = "";
        public string lineB = "";
        public string indicator = "";
        public Outcome outcome = Outcome.DIFFERENT;
        public string message = "";

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public LineDifference()
        {

        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\noutcome = " + outcome);
            sb.Append("\nmessage = " + message);
            sb.Append("\nlineNumber = " + lineNumber);
            sb.Append("\ncolNumber  = " + colNumber);
            sb.Append("\nlineA      = " + lineA);
            sb.Append("\nlineB      = " + lineB);
            sb.Append("\nindicator  = " + indicator);

            return sb.ToString();
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="colNum"></param>
        /// <returns></returns>
        public static string MakeIndicator(int colNum)
        {
            string result = "";

            if (colNum > 0)
            {
                char[] indicator = new char[colNum];
                for (int i = 0; i < colNum - 1; i++)
                {
                    indicator[i] = ' ';
                }
                indicator[colNum - 1] = '^';

                result = new string(indicator);
            }

            return result;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="lineA"></param>
        /// <param name="lineB"></param>
        /// <returns></returns>
        public static int FindColumnNumber(string lineA, string lineB)
        {
            int result = 0;

            if (lineA == null || lineB == null)
            {
                return result;
            }

            int minLen = Math.Min(lineA.Length, lineB.Length);
            int maxLen = Math.Max(lineA.Length, lineB.Length);

            for (int i = 0; i < maxLen; i++)
            {
                if (i >= lineA.Length || i >= lineB.Length)
                {
                    result = i + 1;
                    break;
                }

                if (lineA[i] != lineB[i])
                {
                    result = i + 1;
                    break;
                }

            }

            return result;
        }



    }
}
