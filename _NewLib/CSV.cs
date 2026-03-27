using System;
using static System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _NewLib.Enums;
using static _NewLib.Enums.FtImpQual;
using static _NewLib.Enums.FtImpQualGroup;
using static _NewLib.Enums.FtImpMand;
using _Configuration;
using System.Text.RegularExpressions;

namespace _NewLib
{
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides methods that parse and validate individual 
    /// fields in a qualifield line in a PDF import text file as a
    /// string, an integer, a floating-point number, a date field or a
    /// time field.
    /// </summary>
    public class CSV
    {

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to be a string with a prescribed maximum length.
        /// </summary>
        /// <param name="csv"> - prescribed array of CSV fields.</param>
        /// <param name="fieldNames"> - prescribed array of CSV field names.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="minStringLength"> - prescribed minimum string length.</param>
        /// <param name="maxStringLength"> - prescribed maximum string length.</param>
        /// <param name="value"> - output string.</param>
        /// <param name="nullInd"> - ODBC null indicator.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static string ParseFieldAsString(string[] csv, string[] fieldNames, int fieldNum, int minStringLength, int maxStringLength, out string value, out SQLLEN nullInd)
        {
            // 'out' requirement.
            value = "";
            nullInd = Constant.DB_NULL;

            string msg = String.Format("CSV.ParseFieldAsString(): ");

            // Guard against invalid array index exceptions.
            if ((fieldNum < 0) || (fieldNum >= csv.Length))
            {
                msg += String.Format("array index out of bounds: csv[].length = {0}, fieldNum = {1}", csv.Length, fieldNum);
                throw new Exception(msg);
            }

            // Guard against invalid array index exceptions.
            if ((fieldNum < 0) || (fieldNum >= fieldNames.Length))
            {
                msg += String.Format("array index out of bounds: fieldNames.length = {0}, fieldNum = {1}", fieldNames.Length, fieldNum);
                throw new Exception(msg);
            }

            string field = csv[fieldNum].Trim();
            string fieldName = fieldNames[fieldNum];

            if (field.Length < minStringLength)
            {
                msg += String.Format("{0} = {1}: string too short: field.length = {2}, minStringLength = {3}", fieldName, field, field.Length, minStringLength);
                throw new Exception(msg);
            }
            else if (field.Length > maxStringLength)
            {
                msg += String.Format("{0} = {1}: string too long : field.length = {2}, maxStringLength = {3}", fieldName, field, field.Length, maxStringLength);
                throw new Exception(msg);
            }

            // We have a string with a valid length.
            value = field;
            nullInd = Constant.DB_NOT_NULL;

            // Need to escape single ' marks by doubling-up, i.e. ' becomes ''.
            value = value.Replace("'", "''");

            return value;
        }

        /// <summary>
        /// This method parses a prescribed CSV field expecting a string value.
        /// </summary>
        /// <param name="csv"> - prescribed array of CSV fields.</param>
        /// <param name="fieldNames"> - prescribed array of CSV field names.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="value"> - the parsed string value.</param>
        /// <param name="nullInd"> - ODBC null indicator.</param>
        /// <returns></returns>
        public static string ParseFieldAsString(string[] csv, string[] fieldNames, int fieldNum, out string value, out SQLLEN nullInd)
        {
            return ParseFieldAsString(csv, fieldNames, fieldNum, 0, int.MaxValue, out value, out nullInd);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a date that has a prescribed maximum length; valid date 
        /// formats are:  yyyy.mm.dd  and/or  dd-mmm-yyyy where (mmm = jan, feb, mar ...).
        /// </summary>
        /// <param name="csv"> - prescribed array of CSV fields.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="maxStringLength"> - prescribed maximum string length.</param>
        /// <param name="dateString"> - string encoding the data, converted to yyyy.mm.dd .</param>
        /// <param name="nullInd"> - ODBC null indicator.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsDate(string[] csv, int fieldNum, int maxStringLength, out string dateString, out SQLLEN nullInd)
        {
            // 'out' requirement.
            dateString = "";
            nullInd = Constant.DB_NULL;

            // Valid formats are (yyyy.mm.dd) or (dd-mmm-yyyy).

            string str = String.Format("\r\nParseCSV.ParseFieldAsDate()        : {0,4}  ", fieldNum);

            string field;
            int size;

            // Guard against invalid array index exceptions.
            if (fieldNum >= csv.Length)
            {
                //...Log2.v(str + "=== Field is missing ===");
                return Constant.UT_EOLN;
            }

            /* get next available value */
            field = csv[fieldNum];

            // Guard against a null string.
            if (field == null)
            {
                /* end of string */
                //...Log2.v(str + "=== EOL ===");
                return (Constant.UT_EOLN);
            }

            // Remove leading and trailing blanks.
            field = field.Trim().ToUpper();

            size = field.Length;
            if (size == 0)
            {
                //...Log2.v(str + "=== Field is empty ===");
                return Constant.SUCCESS;   //	No field, no problem.
            }
            else if (size > maxStringLength)
            {
                /* too long */
                //...Log2.v(str + "=== Field is too long ===");
                return (Constant.UT_INV_CONV);
            }
            else
            {
                // We will follow the algorithm used in the legacy C/C++
                // code even though it is inadequate.
                // Attempt to guess which of these formats the input
                // string conforms to by determining whether the string
                // has 2 or 4 leading numerical digits.
                int nleadDigits = 0;
                for (int i = 0; i < size; i++)
                {
                    if (Char.IsDigit(field.ToCharArray()[i]))
                    {
                        nleadDigits++;
                    }
                    else
                    {
                        break;
                    }
                }

                //	nInd will be 2 for dd-mmm-yyyy and 4 for yyyy.mm.dd

                //...Log2.v("\nCSV.ParseFieldAsDate(): field, nLeadDigits: " + field + "  " + nleadDigits);

                // Act on the match success flags.
                if (nleadDigits == 2)
                {
                    // Convert the date field to the preferred 'internal' format.
                    // To mimic the legacy C/C++ code, no 'guarding' is performed.
                    ConvertDate(ref field);
                }
                else if (nleadDigits == 4)
                {
                    // We're good with this; it is the preferred 'internal' format.
                }
                else // invalid number of year digits.
                {
                    //...Log2.v("\nFtParseCSV.ParseFieldAsDate(): neither 2 nor 4; return Constant.UT_INV_CONV");
                    //...Log2.v(str + "=== Invalid number of leading digits ===");
                    return Constant.UT_INV_CONV;
                }

                dateString = field;
                nullInd = Constant.DB_NOT_NULL;

                str += String.Format("{0}", Strings.AddBars(dateString));
                //...Log2.v(str);

                return field.Length;
            }

        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a float that has a prescribed number of decimal places.
        /// </summary>
        /// <param name="csv"> - prescribed array of CSV fields.</param>
        /// <param name="fieldNames"> - prescribed array of CSV field names.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="minValue"> - prescribed minimum value of field.</param>
        /// <param name="maxValue"> - prescribed maximum value of field.</param>
        /// <param name="nDecPlaces"> - prescribed maximum number of decimal places; if negative then no rounding is required.</param>
        /// <param name="fValue"> - string encoding a floating-point number</param>
        /// <param name="nullInd"> - ODBC null indicator.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsFloatRound(string[] csv, string[] fieldNames, int fieldNum, float minValue, float maxValue, int nDecPlaces, out float fValue, out SQLLEN nullInd)
        {
            // 'out' requirement.
            fValue = 0.0f;
            nullInd = Constant.DB_NULL;

            string msg = String.Format("CSV.ParseFieldAsFloatRound(): ");

            // Guard against invalid array index exceptions.
            if ((fieldNum < 0) || (fieldNum >= csv.Length))
            {
                msg += String.Format("array index out of bounds: csv[].length = {0}, fieldNum = {1}", csv.Length, fieldNum);
                throw new Exception(msg);
            }

            // Guard against invalid array index exceptions.
            if ((fieldNum < 0) || (fieldNum >= fieldNames.Length))
            {
                msg += String.Format("array index out of bounds: fieldNames.length = {0}, fieldNum = {1}", fieldNames.Length, fieldNum);
                throw new Exception(msg);
            }

            string field = csv[fieldNum];
            string fieldName = fieldNames[fieldNum];

            double dValue;
            double dInval;
            double dShift;

            field = field.Trim();

            // If the field is empty default the value to zero and return.
            if (String.IsNullOrWhiteSpace(field))
            {
                fValue = 0.0f;
                return Constant.SUCCESS;
            }

            // Attempt the conversion.
            try
            {
                dInval = Convert.ToDouble(field);
                nullInd = Constant.DB_NOT_NULL;
            }
            catch
            {
                msg += String.Format("{0} = {1}: invalid conversion to float.", fieldName, field);
                throw new Exception(msg);
            }

            // If nDecPlaces is negative then no rounding is required.
            if (nDecPlaces < 0)
            {
                dValue = dInval;
            }
            else
            {
                //	We round the value to the specified number of decimal places.  This is
                //	so that values will have the same value when exported and imported again.
                dShift = Pow(10.0, nDecPlaces);

                dValue = dInval * dShift;

                if (dValue < 0.0)
                {
                    dValue = Ceiling(dValue - 0.5);
                }
                else
                {
                    dValue = Floor(dValue + 0.5);
                }
                dValue = dValue / dShift;
            }

            // Convert to float.
            fValue = (float)dValue;

            // Now check the upper and lower bounds.
            if (fValue < minValue)
            {
                msg += String.Format("{0} = {1}: converted float is below minimum limit of {2}.", fieldName, fValue, minValue);
                throw new Exception(msg);
            }
            else if (fValue > maxValue)
            {
                msg += String.Format("{0} = {1}: converted float is above maximum limit of {2}.", fieldName, fValue, maxValue);
                throw new Exception(msg);
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a float that has a prescribed number of decimal places.
        /// </summary>
        /// <param name="csv"> - prescribed array of CSV fields.</param>
        /// <param name="fieldNames"> - prescribed array of CSV field names.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="nDecPlaces"> - prescribed maximum number of decimal places; if negative then no rounding is required.</param>
        /// <param name="fValue"> - string encoding a floating-point number</param>
        /// <param name="nullInd"> - ODBC null indicator.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsFloatRound(string[] csv, string[] fieldNames, int fieldNum, int nDecPlaces, out float fValue, out SQLLEN nullInd)
        {
            return ParseFieldAsFloatRound(csv, fieldNames, fieldNum, float.MinValue, float.MaxValue, nDecPlaces, out fValue, out nullInd);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a short integer.
        /// </summary>
        /// <param name="csv"> - prescribed array of CSV fields.</param>
        /// <param name="fieldNames"> - prescribed array of CSV field names.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="minValue"> - prescribed minimum value of field.</param>
        /// <param name="maxValue"> - prescribed maximum value of field.</param>
        /// <param name="shortValue"> - string encoding a floating-point number</param>
        /// <param name="nullInd"> - ODBC null indicator.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsShort(string[] csv, string[] fieldNames, int fieldNum, short minValue, short maxValue, out short shortValue, out SQLLEN nullInd)
        {
            // 'out' requirement.
            shortValue = 0;
            nullInd = Constant.DB_NULL;

            string msg = String.Format("CSV.ParseFieldAsShort(): ");

            // Guard against invalid array index exceptions.
            if ((fieldNum < 0) || (fieldNum >= csv.Length))
            {
                msg += String.Format("array index out of bounds: csv[].length = {0}, fieldNum = {1}", csv.Length, fieldNum);
                throw new Exception(msg);
            }

            // Guard against invalid array index exceptions.
            if ((fieldNum < 0) || (fieldNum >= fieldNames.Length))
            {
                msg += String.Format("array index out of bounds: fieldNames.length = {0}, fieldNum = {1}", fieldNames.Length, fieldNum);
                throw new Exception(msg);
            }

            string field = csv[fieldNum];
            string fieldName = fieldNames[fieldNum];

            // If the field is empty default the value to zero and return.
            if (String.IsNullOrWhiteSpace(field))
            {
                shortValue = 0;
                return Constant.SUCCESS;
            }

            // Attempt the conversion.
            try
            {
                shortValue = Convert.ToInt16(field);
                nullInd = Constant.DB_NOT_NULL;
            }
            catch
            {
                msg += String.Format("{0} = {1}: invalid conversion to short.", fieldName, field);
                throw new Exception(msg);
            }

            // Now check the upper and lower bounds.
            if (shortValue < minValue)
            {
                msg += String.Format("{0} = {1}: converted short is below minimum limit of {2}.", fieldName, shortValue, minValue);
                throw new Exception(msg);
            }
            else if (shortValue > maxValue)
            {
                msg += String.Format("{0} = {1}: converted short is above maximum limit of {2}.", fieldName, shortValue, maxValue);
                throw new Exception(msg);
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a short integer.
        /// </summary>
        /// <param name="csv"> - prescribed array of CSV fields.</param>
        /// <param name="fieldNames"> - prescribed array of CSV field names.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="shortValue"> - string encoding a floating-point number</param>
        /// <param name="nullInd"> - ODBC null indicator.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsShort(string[] csv, string[] fieldNames, int fieldNum, out short shortValue, out SQLLEN nullInd)
        {
            return ParseFieldAsShort(csv, fieldNames, fieldNum, short.MinValue, short.MaxValue, out shortValue, out nullInd);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a long integer.
        /// </summary>
        /// <param name="csv"> - prescribed array of CSV fields.</param>
        /// <param name="fieldNames"> - prescribed array of CSV field names.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="minValue"> - prescribed minimum value of field.</param>
        /// <param name="maxValue"> - prescribed maximum value of field.</param>
        /// <param name="longValue"> - string encoding a floating-point number</param>
        /// <param name="nullInd"> - ODBC null indicator.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsLong(string[] csv, string[] fieldNames, int fieldNum, long minValue, long maxValue, out long longValue, out SQLLEN nullInd)
        {
            // 'out' requirement.
            longValue = 0;
            nullInd = Constant.DB_NULL;

            string msg = String.Format("CSV.ParseFieldAsLong(): ");

            // Guard against invalid array index exceptions.
            if ((fieldNum < 0) || (fieldNum >= csv.Length))
            {
                msg += String.Format("array index out of bounds: csv[].length = {0}, fieldNum = {1}", csv.Length, fieldNum);
                throw new Exception(msg);
            }

            // Guard against invalid array index exceptions.
            if ((fieldNum < 0) || (fieldNum >= fieldNames.Length))
            {
                msg += String.Format("array index out of bounds: fieldNames.length = {0}, fieldNum = {1}", fieldNames.Length, fieldNum);
                throw new Exception(msg);
            }

            string field = csv[fieldNum];
            string fieldName = fieldNames[fieldNum];

            // If the field is empty default the value to zero and return.
            if (String.IsNullOrWhiteSpace(field))
            {
                longValue = 0;
                return Constant.SUCCESS;
            }

            // Attempt the conversion.
            try
            {
                longValue = Convert.ToInt64(field);
                nullInd = Constant.DB_NOT_NULL;
            }
            catch
            {
                msg += String.Format("{0} = {1}: invalid conversion to long.", fieldName, field);
                throw new Exception(msg);
            }

            // Now check the upper and lower bounds.
            if (longValue < minValue)
            {
                msg += String.Format("{0} = {1}: converted long is below minimum limit of {2}.", fieldName, longValue, minValue);
                throw new Exception(msg);
            }
            else if (longValue > maxValue)
            {
                msg += String.Format("{0} = {1}: converted long is above maximum limit of {2}.", fieldName, longValue, maxValue);
                throw new Exception(msg);
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a long integer.
        /// </summary>
        /// <param name="csv"> - prescribed array of CSV fields.</param>
        /// <param name="fieldNames"> - prescribed array of CSV field names.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="longValue"> - string encoding as a long integer.</param>
        /// <param name="nullInd"> - ODBC null indicator.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsLong(string[] csv, string[] fieldNames, int fieldNum, out long longValue, out SQLLEN nullInd)
        {
            return ParseFieldAsLong(csv, fieldNames, fieldNum, long.MinValue, long.MaxValue, out longValue, out nullInd);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode an integer value.
        /// </summary>
        /// <param name="csv"> - prescribed array of CSV fields.</param>
        /// <param name="fieldNames"> - prescribed array of CSV field names.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="minValue"> - prescribed minimum value of field.</param>
        /// <param name="maxValue"> - prescribed maximum value of field.</param>
        /// <param name="intValue"> - string encoding an int value.</param>
        /// <param name="nullInd"> - ODBC null indicator.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsInt(string[] csv, string[] fieldNames, int fieldNum, int minValue, int maxValue, out int intValue, out SQLLEN nullInd)
        {
            // 'out' requirement.
            intValue = 0;
            nullInd = Constant.DB_NULL;

            string msg = String.Format("CSV.ParseFieldAsInt(): ");

            // Guard against invalid array index exceptions.
            if ((fieldNum < 0) || (fieldNum >= csv.Length))
            {
                msg += String.Format("array index out of bounds: csv[].length = {0}, fieldNum = {1}", csv.Length, fieldNum);
                throw new Exception(msg);
            }

            // Guard against invalid array index exceptions.
            if ((fieldNum < 0) || (fieldNum >= fieldNames.Length))
            {
                msg += String.Format("array index out of bounds: fieldNames.length = {0}, fieldNum = {1}", fieldNames.Length, fieldNum);
                throw new Exception(msg);
            }

            string field = csv[fieldNum];
            string fieldName = fieldNames[fieldNum];

            // If the field is empty default the value to zero and return.
            if (String.IsNullOrWhiteSpace(field))
            {
                intValue = 0;
                return Constant.SUCCESS;
            }

            // Attempt the conversion.
            try
            {
                intValue = Convert.ToInt32(field);
                nullInd = Constant.DB_NOT_NULL;
            }
            catch
            {
                msg += String.Format("{0} = {1}: invalid conversion to int.", fieldName, field);
                throw new Exception(msg);
            }

            // Now check the upper and lower bounds.
            if (intValue < minValue)
            {
                msg += String.Format("{0} = {1}: converted int is below minimum limit of {2}.", fieldName, intValue, minValue);
                throw new Exception(msg);
            }
            else if (intValue > maxValue)
            {
                msg += String.Format("{0} = {1}: converted int is above maximum limit of {2}.", fieldName, intValue, maxValue);
                throw new Exception(msg);
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode an integer value.
        /// </summary>
        /// <param name="csv"> - prescribed array of CSV fields.</param>
        /// <param name="fieldNames"> - prescribed array of CSV field names.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="intValue"> - string encoding an int value.</param>
        /// <param name="nullInd"> - ODBC null indicator.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsInt(string[] csv, string[] fieldNames, int fieldNum, out int intValue, out SQLLEN nullInd)
        {
            return ParseFieldAsInt(csv, fieldNames, fieldNum, int.MinValue, int.MaxValue, out intValue, out nullInd);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a double that has a prescribed number of decimal places.
        /// </summary>
        /// <param name="csv"> - prescribed array of CSV fields.</param>
        /// <param name="fieldNames"> - prescribed array of CSV field names.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="minValue"> - prescribed minimum value of field.</param>
        /// <param name="maxValue"> - prescribed maximum value of field.</param>
        /// <param name="nDecPlaces"> - prescribed maximum number of decimal places; if negative then no rounding is required.</param>
        /// <param name="dValue"> - string encoding a double-precision floating-point number</param>
        /// <param name="nullInd"> - ODBC null indicator.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsDoubleRound(string[] csv, string[] fieldNames, int fieldNum, double minValue, double maxValue, int nDecPlaces, out double dValue, out SQLLEN nullInd)
        {
            // 'out' requirement.
            dValue = 0.0;
            nullInd = Constant.DB_NULL;

            string msg = String.Format("CSV.ParseFieldAsDoubleRound(): ");

            // Guard against invalid array index exceptions.
            if ((fieldNum < 0) || (fieldNum >= csv.Length))
            {
                msg += String.Format("array index out of bounds: csv[].length = {0}, fieldNum = {1}", csv.Length, fieldNum);
                throw new Exception(msg);
            }

            // Guard against invalid array index exceptions.
            if ((fieldNum < 0) || (fieldNum >= fieldNames.Length))
            {
                msg += String.Format("array index out of bounds: fieldNames.length = {0}, fieldNum = {1}", fieldNames.Length, fieldNum);
                throw new Exception(msg);
            }

            string field = csv[fieldNum];
            string fieldName = fieldNames[fieldNum];

            double fInval;
            double dShift;

            field = field.Trim();

            // If the field is empty return with nullInd set to Constant.DB_NULL.
            if (String.IsNullOrWhiteSpace(field))
            {
                dValue = 0.0;
                return Constant.SUCCESS;
            }

            // Attempt the conversion.
            try
            {
                fInval = Convert.ToDouble(field);
                nullInd = Constant.DB_NOT_NULL;
            }
            catch
            {
                msg += String.Format("{0} = {1}: invalid conversion to double.", fieldName, field);
                throw new Exception(msg);
            }

            // If nDecPlaces is negative then no rounding is required.
            if (nDecPlaces < 0)
            {
                dValue = fInval;
            }
            else
            {
                //	We round the value to the specified number of decimal places.  This is
                //	so that values will have the same value when exported and imported again.
                dShift = Pow(10.0, nDecPlaces);

                dValue = fInval * dShift;

                if (dValue < 0.0)
                {
                    dValue = Ceiling(dValue - 0.5);
                }
                else
                {
                    dValue = Floor(dValue + 0.5);
                }
                dValue = dValue / dShift;
            }

            // Now check the upper and lower bounds.
            if (dValue < minValue)
            {
                msg += String.Format("{0} = {1}: converted double is below minimum limit of {2}.", fieldName, dValue, minValue);
                throw new Exception(msg);
            }
            else if (dValue > maxValue)
            {
                msg += String.Format("{0} = {1}: converted double is above maximum limit of {2}.", fieldName, dValue, maxValue);
                throw new Exception(msg);
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a double that has a prescribed number of decimal places.
        /// </summary>
        /// <param name="csv"> - prescribed array of CSV fields.</param>
        /// <param name="fieldNames"> - prescribed array of CSV field names.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="nDecPlaces"> - prescribed maximum number of decimal places; if negative then no rounding is required.</param>
        /// <param name="dValue"> - string encoding a double-precision floating-point number</param>
        /// <param name="nullInd"> - ODBC null indicator.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsDoubleRound(string[] csv, string[] fieldNames, int fieldNum, int nDecPlaces, out double dValue, out SQLLEN nullInd)
        {
            return ParseFieldAsDoubleRound(csv, fieldNames, fieldNum, double.MinValue, double.MaxValue, nDecPlaces, out dValue, out nullInd);
        }

        /// <summary>
        /// This method determines whether a prescribed string has a 
        /// valid time format hh:mm  ; no numerical bounds checking
        /// is performed.
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>
        public static bool IsValidTime(string candidate)
        {
            bool result = false;

            if (!String.IsNullOrWhiteSpace(candidate))
            {
                string pattern = @"\d\d:\d\d";
                Match match = Regex.Match(candidate, pattern);
                result = match.Success;
            }

            return result;
        }

        /// <summary>
        /// This methods inputs a single line of text in CSV format and returns an
        /// string array giving the individual CSV text fields; the method handles
        /// fields that are quoted and contain commas.
        /// </summary>
        /// <param name="line"> - a prescribed single line of text read from the import file.</param>
        /// <param name="fields"> - output array of CSV fields.</param>
        /// <returns></returns>
        public static int ParseFields(string line, out string[] fields)
        {
            // Any field may be contained in quotes. However fields that contain a line-break, 
            // comma, or quotation marks must be contained in quotes.
            //
            // To re-emphasize the above, line breaks within a field are allowed within a CSV as
            // long as they are wrapped in quotation marks, this is what trips most people up who
            // are simply reading line by line like it’s a regular text file.
            // 
            // Note that individual double-quote marks (") within a field can be 'escaped' by doing
            // double double-quote marks ("") - but this is not handled by this method.

            // 'out' requirement.
            fields = new string[0];

            string field = "";
            List<string> fieldsList = new List<string>();

            // A comma could occur inside a quoted string; this would confuse the 'split'.
            // Replace every comma inside a quoted string with the TILDE char '~'.

            List<char> characters = new List<char>();

            bool insideQuotedvalue = false;
            const char QUOTE = '"';
            const char TILDE = '~';
            const char COMMA = ',';

            int importOK = Constant.FAILURE;

            if (!String.IsNullOrWhiteSpace(line))
            {
                // If we reach here the rawLine must contain some text.
                char[] chars = line.ToCharArray();

                for (int i = 0; i < line.Length; i++)
                {
                    char c = chars[i];

                    if (i == line.Length - 1)
                    {
                        // Be careful: the last character on a line could be a comma which appends a blank field.
                        if (c == COMMA)
                        {
                            field = new String(characters.ToArray());
                            fieldsList.Add(field);
                            fieldsList.Add("");
                        }
                        else
                        {
                            characters.Add(c);
                            field = new String(characters.ToArray());
                            fieldsList.Add(field);
                        }
                    }
                    else if (!insideQuotedvalue && (c == QUOTE))
                    {
                        insideQuotedvalue = true;
                        characters.Add(c);
                    }
                    else if (insideQuotedvalue && (c == QUOTE))
                    {
                        insideQuotedvalue = false;
                        characters.Add(c);
                    }
                    else if (insideQuotedvalue && (c == COMMA))
                    {
                        characters.Add(TILDE);
                    }
                    // This general case successfully handles the first character being a comma.
                    else if (!insideQuotedvalue && (c == COMMA))
                    {
                        field = new string(characters.ToArray());
                        fieldsList.Add(field);
                        characters = new List<char>();
                    }
                    else
                    {
                        characters.Add(c);
                    }
                }

                List<string> deQuotedFields = new List<string>();

                for (int i = 0; i < fieldsList.Count; i++)
                {
                    // Replace any TILDE by a COMMA.
                    string str = fieldsList[i].Replace(TILDE, COMMA);

                    str = str.Trim();

                    if (str != "")
                    {
                        if (str.First() == QUOTE) str = str.Substring(1, str.Length - 1);
                        if (str.Last() == QUOTE) str = str.Substring(0, str.Length - 1);
                    }

                    string reducedStr = str.Trim();

                    deQuotedFields.Add(reducedStr);

                    //Console.Error.Write("\n{0,3}: |{1}|", i + 1, reducedStr);
                }

                fields = deQuotedFields.ToArray();

                importOK = fields.Length;
            }

            return importOK;
        }

        /// <summary>
        /// This method converts a date string like 14-MAY-19 or 14-MAY-2019
        /// to the string 14.05.19 (where the '-' can be any separator character).
        /// </summary>
        /// <param name="cdate"> - input string of the form 14-MAY-19 or 14-MAY-2019.</param>
        /// <returns></returns>
        public static bool ConvertDate(ref string cdate)
        {
            string localDate;
            string cyear;
            string cmon;
            string cday;


            localDate = cdate.Trim();

            // Convert the date to the format YYYY.MM.DD

            cday = localDate.Substring(0, 2);

            cmon = localDate.Substring(3, 3);

            // This will return false and cmon = "??" for an invalid input month string.
            if (!ConvertMonth(ref cmon))
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
        /// Converts a string encoding a month as 3 letters into the corresponding
        /// encoding as 2 numbers, e.g. "APR" is converted to "04".
        /// </summary>
        /// <param name="month"> - On input: 3 letter month code to be converted. On output: 2 digit month code.</param>
        /// <returns>true if the prescribed 3 letter month code is valid; otherwise false.</returns>
        public static bool ConvertMonth(ref string month)
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

        public static string EncodeFieldAsCSV(string field)
        {
            if (String.IsNullOrWhiteSpace(field)) return "";

            string escapedField = "";

            // Fields containing a line-break, double-quote or commas need to be double-quoted.
            bool doubleQuotingRequired = field.Contains('"') || field.Contains(',') || field.Contains("\n");

            if (doubleQuotingRequired)
            {
                // If a double-quote occurs inside the field then it must be double-up,
                // i.e. " must be replaced by "".
                field = field.Trim().Replace("\"", "\"\"");
                escapedField = "\"" + field + "\"";
            }
            else
            {
                escapedField = field.Trim();
            }

            return escapedField;
        }


    }
}


