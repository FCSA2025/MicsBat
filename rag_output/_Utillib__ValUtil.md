# Documented File: ValUtil.cs
**Repository Path:** `_Utillib\ValUtil.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace _Utillib
{
    /// <summary>
    /// Provides methods to validate discrete parameter values e.g. latitude,
    /// longitude, province/state code, date etc.
    /// </summary>
    public class ValUtil
    {
        private static int[] maxDay = new int[13] { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        /// <summary>
        /// Checks whether a prescribed latitude value (unit: 1/100th of a second or arc) 
        /// is in the range Constant.LAT_MINIMUM to Constant.LAT_MAXIMUM corresponding to 
        /// [-90, 90] degrees.
        /// </summary>
        /// <param name="latit"> - prescribed longitude value (unit: 1/100th of a second or arc).</param>
        /// <returns> - true or false.</returns>
        public static bool UtValidateLat(int latit)
        {
            if ((latit < Constant.LAT_MINIMUM) || (latit > Constant.LAT_MAXIMUM))
            {
                return (false);
            }
            else
            {
                return (true);
            }
        }

        /// <summary>
        /// Checks whether a prescribed longitude value (unit: 1/100th of a second or arc) 
        /// is in the range Constant.LONG_MINIMUM to Constant.LONG_MAXIMUM corresponding to 
        /// [-180, 180] degrees.
        /// </summary>
        /// <param name="longit"> - prescribed longitude value (unit: 1/100th of a second or arc).</param>
        /// <returns> - true or false.</returns>
        public static bool UtValidateLong(int longit)
        {
            if ((longit < Constant.LONG_MINIMUM) || (longit > Constant.LONG_MAXIMUM))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks whether a province or state code is found in any of the arrays
        /// Constant.CAN_PROVINCE_CODES, Constant.USA_STATE_CODES or Constant.OTHER_STATE_CODES.
        /// </summary>
        /// <param name="prov"> - prescribed province or state code (should be 2 letters).</param>
        /// <returns> - true or false.</returns>
        public static bool UtValidateProv(string prov)
        {
            /*	Note that this method returns SUCCESS (0) for valid and FAILURE (1) for invalid*/

            bool retVal = false;
            string cCountryCode;

            if (GenUtil.UtGetCountry(prov, out cCountryCode))
            {
                // The "get country from province" succeeded.
                retVal = true;
            }

            return retVal;
        }

        /// <summary>
        /// Checks whether a date string has one of the canonical formats:
        ///"dd-mmm-yy", "dd-mmm-yyyy", "yymmdd", "yyyymmdd" or "yyyy.mm.dd".
        /// If the string does have a valid format then the year checked if it
        /// is in the range 1960 to 2050. A month number is validated to be
        /// in the range [1, 12]. Month three letter abbreviations are checked
        /// for validity. Finally the day number is checked to be in the range
        /// [1, D] where D is the last day in the month (e.g. 28, 29, 30 or 31)
        /// taking leap years into account.
        /// </summary>
        /// <param name="dateVal"> - date string to be validated.</param>
        /// <returns> - true or false.</returns>
        public static bool UtValidateDate(string dateVal)
        {
            //...Log2.v("\n\nValidTS.UtValidateDate(): Entry");

            string localDate;
            string cyear = "";
            string cmon = "";
            string cday = "";
            int[] maxDay = new int[13] { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

            int iyear;
            int imon;
            int iday;

            localDate = dateVal.Trim();

            /* this validates the date.  The accepted formats are  */
            /*  dd-mmm-yy  dd-mmm-yyyy    yymmdd    yyyymmdd       */
            /*	yyyy.mm.dd added -- GJS 1283 2009.06.12 */

            const string A = "[a-zA-Z]";
            const string N = "[0-9]";

            bool validPattern = false;

            // Check for dd_mmm_yy
            string pattern_dd_mmm_yy = N + N + "-" + A + A + A + "-" + N + N;
            bool dd_mmm_yy = (localDate.Length == 9) && Regex.IsMatch(localDate, pattern_dd_mmm_yy);
            validPattern |= dd_mmm_yy;

            // Check for dd-mmm-yyyy
            string pattern_dd_mmm_yyyy = N + N + "-" + A + A + A + "-" + N + N + N + N;
            bool dd_mmm_yyyy = (localDate.Length == 11) && Regex.IsMatch(localDate, pattern_dd_mmm_yyyy);
            validPattern |= dd_mmm_yyyy;

            // Check for yymmdd
            string pattern_yymmdd = N + "{6}";
            bool yymmdd = (localDate.Length == 6) && Regex.IsMatch(localDate, pattern_yymmdd);
            validPattern |= yymmdd;

            // Check for yyyymmdd
            string pattern_yyyymmdd = N + "{8}";
            bool yyyymmdd = (localDate.Length == 8) && Regex.IsMatch(localDate, pattern_yyyymmdd);
            validPattern |= yyyymmdd;

            // Check for yyyy.mm.dd
            string pattern_yyyyOmmOdd = N + N + N + N + @"\." + N + N + @"\." + N + N;
            bool yyyyOmmOdd = (localDate.Length == 10) && Regex.IsMatch(localDate, pattern_yyyyOmmOdd);
            validPattern |= yyyyOmmOdd;

            // If not one of the valid patterns we can return false at this point.
            if (!validPattern) return false;

            if (dd_mmm_yy || dd_mmm_yyyy)
            {
                cday = localDate.Substring(0, 2);
                cmon = localDate.Substring(3, 3);
                cyear = localDate.Substring(7);

                // Convert cmon from AAA to NN.
                if (!GenUtil.UtConvertMonth(ref cmon))
                {
                    // The month conversion attempt failed - incorrect AAA.
                    return false;
                }
            }

            if (yymmdd)
            {
                cday = localDate.Substring(4, 2);
                cmon = localDate.Substring(2, 2);
                cyear = localDate.Substring(0, 2);
            }

            if (yyyymmdd)
            {
                cday = localDate.Substring(6, 2);
                cmon = localDate.Substring(4, 2);
                cyear = localDate.Substring(0, 4);
            }

            if (yyyyOmmOdd)
            {
                cday = localDate.Substring(8, 2);
                cmon = localDate.Substring(5, 2);
                cyear = localDate.Substring(0, 4);
            }

            //convert the date's numerical elements to integer values.
            iday = Convert.ToInt32(cday);
            imon = Convert.ToInt32(cmon);
            iyear = Convert.ToInt32(cyear);

            // adjust for dates between 1976 and 2050.
            if (iyear < 50)
            {
                iyear += 2000;
            }
            else if ((iyear >= 50) && (iyear < 100))
            {
                iyear += 1900;
            }

            // Range check: year.
            if ((iyear < 1960) || (iyear > 2050))
            {
                return false;
            }

            // Range check: month.
            if ((imon < 1) || (imon > 12))
            {
                return false;
            }

            // Range check: day.
            bool isLeapYear = (iyear % 4 == 0 && iyear % 100 != 0) || iyear % 400 == 0;
            if (isLeapYear && (imon == 2))
            {
                // Case: February during a leap year.
                if ((iday > 29) || (iday < 1))
                {
                    return false;
                }
            }
            else
            { /* any other month */
                if ((iday > maxDay[imon]) || (iday < 1))
                {
                    return false;
                }
            }

            //...Log2.v("\n\nValidTS.UtValidateDate(): Exit");

            // If the thread of control reaches here the date is valid.
            return true;
        }







    }
}

```
