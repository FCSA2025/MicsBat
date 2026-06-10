# Documented File: FtParsing.cs
**Repository Path:** `FtImport\FtParsing.cs`
**Primary Layer:** `FtImport`
**Namespace:** `FtImport`

## Source Code Representation
```csharp
﻿using System;
using static System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _NewLib;
using static _NewLib.Enums;
using static _NewLib.Enums.FtImpQual;
using static _NewLib.Enums.FtImpQualGroup;
using static _NewLib.Enums.FtImpMand;
using _Configuration;
using _Utillib;
using System.Text.RegularExpressions;

namespace FtImport
{
    /// <summary>
    /// This class provides methods that parse and validate individual 
    /// fields in a qualifield line in a PDF import text file as a
    /// string, an integer, a floating-point number, a date field or a
    /// time field.
    /// </summary>
    public class FtParsing
    {

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to be a string with a prescribed maximum length; an 'empty' field
        /// in a PDF record will produce a string 'token' of zero length; the returned 
        /// value is the token length.
        /// </summary>
        /// <param name="qualLine"> - prescribed qualified line.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="token"> - output string.</param>
        /// <param name="maxStringLength"> - prescribed maximum string length.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsString(QualLine qualLine, int fieldNum, out string token, int maxStringLength)
        {
            // 'out' requirement.
            token = "";

            // Guard against invalid array index exceptions.
            if (fieldNum >= qualLine.Fields.Length)
            {
                return Constant.UT_EOLN;
            }

            int size;
            int result = -666;

            // The Fields array was created elsewhere using a String.Split(',') operation.
            // As a consequence all elements of Fields should contain a string (possibly of zero length)
            // and should never be a null string.
            string field = qualLine.Fields[fieldNum];


            // Out of an abundance of caution, check the field for null.
            if (field == null)
            {
                result = Constant.UT_EOLN;
            }
            else
            {
                token = field.ToUpper().Trim();

                /* if length greater than requested length */
                size = token.Length;
                if (size <= maxStringLength)
                {
                    result = size;
                }
                else
                {
                    /* too long */
                    //...Log2.v("\nValidation.ParseFieldAsString(): too long");
                    result = Constant.UT_INV_CONV;
                }
            }

            string str = String.Format("\r\nValidation.ParseFieldAsString(): {0,2}  {1}  {2}", fieldNum, Strings.AddBars(token), result);
            //...Log2.v(str);
            return result;
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a date that has a prescribed maximum length; valid date 
        /// formats are:  yyyy.mm.dd  and/or  dd-mmm-yyyy where (mmm = jan, feb, mar ...).
        /// </summary>
        /// <param name="qualLine"> - prescribed qualified line.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="dateString"> - string encoding the data, converted to yyyy.mm.dd .</param>
        /// <param name="maxStringLength"> - prescribed maximum string length.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsDate(QualLine qualLine, int fieldNum, out string dateString, int maxStringLength)
        {
            // 'out' requirement.
            dateString = "";

            // Valid formats are (yyyy.mm.dd) or (dd-mmm-yyyy).

            string field;
            int size;

            // Guard against invalid array index exceptions.
            if (fieldNum >= qualLine.Fields.Length)
            {
                return Constant.UT_EOLN;
            }

            /* get next available token */
            field = qualLine.Fields[fieldNum];

            // Guard against a null string.
            if (field == null)
            {
                /* end of string */
                return (Constant.UT_EOLN);
            }

            // Remove leading and trailing blanks.
            field = field.Trim().ToUpper();

            size = field.Length;
            if (size == 0)
            {
                return Constant.SUCCESS;   //	No field, no problem.
            }
            else if (size > maxStringLength)
            {
                /* too long */
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

                //...Log2.v("\nFtValidation.ParseFieldAsDate(): field, nLeadDigits: " + field + "  " + nleadDigits);

                // Act on the match success flags.
                if (nleadDigits == 2)
                {
                    // Convert the date field to the preferred 'internal' format.
                    // To mimic the legacy C/C++ code, no 'guarding' is performed.
                    GenUtil.UtConvertDate(ref field);
                    //if (!GenUtil.UtConvertDate(ref field))
                    //{
                    //   //...Log2.v("\nFtValidation.ParseFieldAsDate(): == 2; return Constant.UT_INV_CONV");
                    //    return Constant.UT_INV_CONV;
                    //}

                }
                else if (nleadDigits == 4)
                {
                    // We're good with this; it is the preferred 'internal' format.
                }
                else // invalid number of leading digits.
                {
                    //...Log2.v("\nFtValidation.ParseFieldAsDate(): neither 2 nor 4; return Constant.UT_INV_CONV");
                    return Constant.UT_INV_CONV;
                }

                dateString = field;

                return field.Length;
            }

        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a float that has a prescribed number of decimal places.
        /// </summary>
        /// <param name="qualLine"> - prescribed qualified line.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="fNumber"> - string encoding a floating-point number</param>
        /// <param name="nDecPlaces"> - prescribed maximum number of decimal places.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsFloatRound(QualLine qualLine, int fieldNum, out float fNumber, int nDecPlaces)
        {
            // 'out' requirement.
            fNumber = 0.0f;

            string istring;
            float fInput;
            double dShift;

            // Guard against invalid array index exceptions.
            if (fieldNum >= qualLine.Fields.Length)
            {
                return -1;
            }

            /* get next token */
            istring = qualLine.Fields[fieldNum];

            if (istring == null)
            {
                /* no more tokens */
                return (-1);
            }

            istring = istring.Trim();

            if (istring == "")
            {
                return (0); /* string length of zero, value is 0 */
            }

            /* convert string to float */
            try
            {
                fInput = Convert.ToSingle(istring);
            }
            catch
            {
                /* invalid conversion */

                Log2.e("\nFtValidation.ParseFieldAsFloatRound(): ERROR: invalid string for conversion to float: " + Strings.AddBars(istring));

                //AH: ISSUE.  return of zero interpreted as "we have no valid number so set
                //            nullInd = DB_NULL.
                //            Would be more intuitive to return DB_NULL (-1).
                return (-2);
            }

            /*	We round the value to the specified number of decimal places.  This is
            *	so that values will have the same value when exported and imported again. */
            dShift = Pow(10.0, (double)nDecPlaces);

            fNumber = fInput * (float)dShift;

            if (fNumber < 0.0)
            {
                fNumber = (float)Ceiling(fNumber - 0.5);
            }
            else
            {
                fNumber = (float)Floor(fNumber + 0.5);
            }
            fNumber = (float)((double)fNumber / dShift);

            /*	Check if rounding did occur and indicate it to the user */
            if (Abs(fNumber - fInput) > 0.1 / dShift)
            {
                return (-3);        /*	Rounding did occur */
            }

            return ((int)istring.Length);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a short integer.
        /// </summary>
        /// <param name="qualLine"> - prescribed qualified line.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="number"> - string encoding a floating-point number</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsShort(QualLine qualLine, int fieldNum, out short number)
        {
            // 'out' requirement.
            number = 0;

            // Guard against invalid array index exceptions.
            if (fieldNum >= qualLine.Fields.Length)
            {
                return -1;
            }

            string istring;

            /* get next token */
            istring = qualLine.Fields[fieldNum];

            if (istring == null)
            {
                /* no more tokens */
                return (-1);
            }

            istring = istring.Trim();

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
                Log2.e("\nFtValidation.ParseFieldAsShort(): ERROR: invalid string for conversion to short: " + Strings.AddBars(istring));
                return (Error.INVALIDNAMEORVALUE);
            }

            return ((int)istring.Length);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode an integer value.
        /// </summary>
        /// <param name="qualLine"> - prescribed qualified line.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="number"> - string encoding a floating-point number</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsInt(QualLine qualLine, int fieldNum, out int number)
        {
            // 'out' requirement.
            number = 0;

            // Guard against invalid array index exceptions.
            if (fieldNum >= qualLine.Fields.Length)
            {
                return -1;
            }

            string istring;

            /* get next token */
            istring = qualLine.Fields[fieldNum];

            if (istring == null)
            {
                /* no more tokens */
                return (-1);
            }

            istring = istring.Trim();

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
                Log2.e("\nFtValidation.ParseFieldAsInt(): ERROR: invalid string for conversion to int: " + Strings.AddBars(istring));
                return (Error.INVALIDNAMEORVALUE);
            }

            return ((int)istring.Length);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a double that has a prescribed number of decimal places.
        /// </summary>
        /// <param name="qualLine"> - prescribed qualified line.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="dNumber"> - string encoding a double-precision floating-point number</param>
        /// <param name="nDecPlaces"> - prescribed maximum number of decimal places.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsDoubleRound(QualLine qualLine, int fieldNum, out double dNumber, int nDecPlaces)
        {
            // 'out' requirement.
            dNumber = 0.0;

            // Guard against invalid array index exceptions.
            if (fieldNum >= qualLine.Fields.Length)
            {
                return -1;
            }

            string istring;
            double fInval;
            double dShift;

            /* get next token */
            istring = qualLine.Fields[fieldNum];

            if (istring == null)
            {
                /* no more tokens */
                return (-1);
            }

            istring = istring.Trim();

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
                Log2.e("\nFtValidation.ParseFieldAsDoubleRound(): ERROR: invalid string for conversion to double: " + Strings.AddBars(istring));
                return (Error.INVALIDNAMEORVALUE);
            }

            /*	We round the value to the specified number of decimal places.  This is
            *	so that values will have the same value when exported and imported again. */
            dShift = Pow(10.0, nDecPlaces);

            dNumber = fInval * dShift;

            if (dNumber < 0.0)
            {
                dNumber = Ceiling(dNumber - 0.5);
            }
            else
            {
                dNumber = Floor(dNumber + 0.5);
            }
            dNumber = dNumber / dShift;

            /*	Check if rounding did occur and indicate it to the user */
            if (Abs(dNumber - fInval) > 0.1 / dShift)
            {
                return (-3);        /*	Rounding did occur */
            }

            return ((int)istring.Length);
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
        /// This methods inputs an array of strings corresponding to lines in the TS
        /// import text file and parses them to identify, and accumulate, only 'qualified' lines
        /// , i.e. have the prefix AK, AR, AT, AS, CK, CR, CT, GK, 
        /// LK, SK, SR, ST, TD, TE and/or ZK, and that have the correct number of
        /// comma-separated-values (CSV) for that qualifier type. 
        /// See <a href="ES Data File - Text File Format.pdf">ES data file specification</a>.
        /// </summary>
        /// <param name="rawLines"> - array of lines from the TS import text file.</param>
        /// <param name="qualLines"> - output array of qualified lines.</param>
        /// <returns>Zero if no errors are found; otherwise, negative.</returns>
        public static short ParseForQualLines(string[] rawLines, out QualLine[] qualLines)
        {
            // 'out' requirement.
            qualLines = new QualLine[0];

            QualLine qualLine;

            short importOK = Constant.FAILURE;
            //string msgBuf;

            List<QualLine> qualLineList = new List<QualLine>();

            int lineNum = 0;

            foreach (string rawLine in rawLines)
            {
                lineNum++;

                //...Log2.v("\n\nQualLine.ParseForQualLines(): rawline #" + lineNum);
                //...Log2.v("\nQualLine.ParseForQualLines(): rawLine: " + Strings.AddBars(rawLine));

                string lineText = rawLine;

                // Skip over any comment lines.
                if (Strings.FirstCharIs(lineText, Constant.COMMENT_CHAR))
                {
                    continue;
                }
                else if (String.IsNullOrWhiteSpace(lineText))
                {
                    // To mimic the behaviour of the legacy C/C++ code we have
                    // to parse blank lines as a QualLine with qualifier = FtImpQual.UNKNOWN
                    // qualLine = new QualLine(lineNum, "", FtImpQual.UNKNOWN, new string[1] { "XX" });

                    // AH: just ignore any blank lines.
                    continue;
                }
                else
                {
                    // If we reach here the rawLine must contain some text.
                    // Split it into CSV fields.
                    // Note that ',,' and ',\n' parse as strings of zero length.
                    string[] fields = rawLine.Split(',');
                    //...Log2.v("\nQualLine.ParseForQualLines(): fields.Length = " + fields.Length);
                    //...Log2.v("\nQualLine.ParseForQualLines(): fields[0] = " + Strings.AddBars(fields[0]));

                    // The first CSV field provides the line 'qualifier' token.
                    string firstToken = fields[0];
                    FtImpQual qualifier = StringToFtImpQual(firstToken);
                    //...Log2.v("\nQualLine.ParseForQualLines(): qualifier = " + qualifier);

                    qualLine = new QualLine(lineNum, lineText, qualifier, fields);
                }

                // Add to the list of qualified lines.
                qualLineList.Add(qualLine);
            }

            // Convert the list to an array that is provided as output.
            qualLines = qualLineList.ToArray();

            //...Log2.v("\nQualLine.ParseForQualLines(): qualified lines:");
            foreach (QualLine qLine in qualLines)
            {
                //...Log2.v(qLine.ToString());
            }

            importOK = Constant.SUCCESS;
            return importOK;
        }

        /// <summary>
        /// This method converts a string representation of a line qualifier
        /// to its FtImpQual enumeration value, e.g. input of "TE" gives and 
        /// output of FtImpQual.TE
        /// </summary>
        /// <param name="qualifierString"> - string representation of a qualifier, e.g. "TE".</param>
        /// <returns>FtImpQual enumeration value.</returns>
        public static FtImpQual StringToFtImpQual(string qualifierString)
        {
            if (String.IsNullOrWhiteSpace(qualifierString))
            {
                return FtImpQual.UNKNOWN;
            }

            FtImpQual result;

            switch (qualifierString.Trim())
            {
                case "TT":
                    result = TT;
                    break;
                case "GK":
                    result = GK;
                    break;
                case "SK":
                    result = SK;
                    break;
                case "SD":
                    result = SD;
                    break;
                case "AK":
                    result = AK;
                    break;
                case "AQ":
                    result = AQ;
                    break;
                case "AO":
                    result = AO;
                    break;
                case "CK":
                    result = CK;
                    break;
                case "CT":
                    result = CT;
                    break;
                case "CR":
                    result = CR;
                    break;
                case "CQ":
                    result = CQ;
                    break;
                case "CO":
                    result = CO;
                    break;
                default:
                    result = FtImpQual.UNKNOWN;
                    break;
            }

            return result;
        }



    }
}

```
