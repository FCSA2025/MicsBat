# Documented File: AnteRecord.cs
**Repository Path:** `FeImport\AnteRecord.cs`
**Primary Layer:** `FeImport`
**Namespace:** `FeImport`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _Utillib;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeImport
{
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides methods that parse and validate Antenna records 
    /// (AK, AR, AS, AT) in an ES import file, instantiate and populate an
    /// FeAnte object with imported values and insert a record into an _ante table in the database.
    /// </summary>
    public class AnteRecord
    {
        private static short mAKflag = Constant.CLEAR;
        private static short mATflag = Constant.CLEAR;
        private static short mARflag = Constant.CLEAR;
        private static string msgBuf;
        private static char c;
        private static double dTemp;                 // Used in rounding.

        private static FeAnte anteStruct = new FeAnte();
        private static SQLLEN[] anteNulls = NullHelper.CreateArrayOfNullInd(FeAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

        /// <summary>
        /// This method parses qualified lines of type 'AK', validates its CSV fields and then
        /// partially populates an FeAnte object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_AK(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nAnteRecord.Parse_AK(): Entry: importOK = " + importOK);

            int rc;

            if (mAKflag == Constant.SET)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of ANTENNA record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): A: importOK = Constant.FAILURE");

            }
            else
            {
                mAKflag = Constant.SET;
            }

            //	Initialize the null array to all null
            anteNulls = NullHelper.CreateArrayOfNullInd(FeAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            // Field #0 is the type/qualifier.

            // Field #1 is MDB Operation.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 1, out anteStruct.cmd, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna cmd invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna cmd end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.CMD] = Constant.DB_NULL;

            }
            else
            {
                anteNulls[FeAnte.CMD] = Constant.DB_NOT_NULL;
            }

            // Field #2 is the Record Status.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 2, out anteStruct.recstat, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Ante recstat invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Ante recstat end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.RECSTAT] = Constant.DB_NULL;

            }
            else
            {
                anteNulls[FeAnte.RECSTAT] = Constant.DB_NOT_NULL;
            }

            // Field #3 is the Location.
            rc = FeValidation.ParseFieldAsString(qualLine, 3, out anteStruct.location, FeAnte.LOCATION_SZ);
            if (rc == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna location invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna location end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.LOCATION] = Constant.DB_NULL;

            }
            else
            {
                anteNulls[FeAnte.LOCATION] = Constant.DB_NOT_NULL;
            }

            // Field #4 is the Call Sign.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 4, out anteStruct.call1, 9)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna call local invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): H: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna call local end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): I: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.CALL1] = Constant.DB_NULL;

            }
            else
            {
                anteNulls[FeAnte.CALL1] = Constant.DB_NOT_NULL;
            }

            // Field # 5 is the License.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 5, out anteStruct.licence, FeAnte.LICENCE_SZ - 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna licence invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): J: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna licence end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): K: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.LICENCE] = Constant.DB_NULL;

            }
            else
            {
                anteNulls[FeAnte.LICENCE] = Constant.DB_NOT_NULL;
            }

            // Field #6 is the Status.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 6, out anteStruct.stata, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna status invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): L: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna status end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): M: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.STATA] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[FeAnte.STATA] = Constant.DB_NOT_NULL;
            }

            // Field #7 is Notes.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 7, out anteStruct.nota, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna notes invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): N: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna notes end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): O: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.NOTA] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[FeAnte.NOTA] = Constant.DB_NOT_NULL;
            }

            // Field # 8 is the Antenna Reference.
            if ((rc = FeValidation.ParseFieldAsInt(qualLine, 8, out anteStruct.antref)) == Error.INVALIDNAMEORVALUE)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna Reference: invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): P: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna Reference: end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.ANTREF] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[FeAnte.ANTREF] = Constant.DB_NOT_NULL;
            }

            // Field #9 is the Modify Date.
            if ((rc = FeValidation.ParseFieldAsDate(qualLine, 9, out anteStruct.mdate, FeAnte.MDATE_SZ)) < 0)
            {
                msgBuf = String.Format("Error - line #{0}, Invalid date field; format should be yyyy.mm.dd  or  dd-mmm-yyyy (mmm = JAN, FEB, MAR etc)\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = (short)rc;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): R: importOK = Constant.FAILURE");
            }
            else if (rc == 0)  // string has zero length.
            {
                anteNulls[FeAnte.MDATE] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[FeAnte.MDATE] = Constant.DB_NOT_NULL;

                // Check that the date is numerically valid.
                if (!ValUtil.UtValidateDate(anteStruct.mdate))
                {
                    msgBuf = String.Format("Error - line #{0}, Invalid date field; the day, month or year is out of range.\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Error.BADDATE;
                    //...Log2.v("\n\nAnteRecord.Parse_AK(): T: importOK = Constant.FAILURE");
                }
            }

            // Field #10 is the Modify Time.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 10, out anteStruct.mtime, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna modify time invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): U: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna modify time end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): V: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.MTIME] = Constant.DB_NULL;
            }
            else if (!FeValidation.IsValidTime(anteStruct.mtime))
            {
                msgBuf = String.Format("Error - line #{0}, Invalid time field; format should be hh:mm\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Error.BADTIME;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): V: importOK = Constant.FAILURE");
            }
            else
            {
                anteNulls[FeAnte.MTIME] = Constant.DB_NOT_NULL;
            }

            //...Log2.v("\nAnteRecord.Parse_AK(): Exit: importOK = " + importOK);

            return importOK;
        }

        /// <summary>
        /// This method parses a qualified line of type 'AT', validates its CSV fields and then
        /// partially populates an FeAnte object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_AT(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nAnteRecord.Parse_AT(): Entry: importOK = " + importOK);

            int rc;

            if (mATflag == Constant.SET)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of ANTENNA record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): A: importOK = Constant.FAILURE");
            }
            else
            {
                mATflag = Constant.SET;
            }

            // Field #0 is the type/qualifier.

            // Field #1 is the Band Code
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 1, out anteStruct.txband, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx band invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx band end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.TXBAND] = Constant.DB_NULL;

            }
            else
            {
                anteNulls[FeAnte.TXBAND] = Constant.DB_NOT_NULL;
            }

            // Field #2 is the Antenna Code.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 2, out anteStruct.acodetx, 12)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna antenna code TX invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna antenna code TX end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.ACODETX] = Constant.DB_NULL;

            }
            else
            {
                anteNulls[FeAnte.ACODETX] = Constant.DB_NOT_NULL;
            }

            // Field #3 is the Antenna Feed System Loss.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 3, out anteStruct.afslt, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx antenna feed loss invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx antenna feed loss end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.AFSLT] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna tx antenna feed loss rounded to 1 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteNulls[FeAnte.AFSLT] = Constant.DB_NOT_NULL;
            }

            // Field #4 is the Maximum Antenna Gain.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 4, out anteStruct.txhgmax, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna max. tx Horizon gain invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): H: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna max. tx Horizon gain end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): I: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.TXHGMAX] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna max tx Horizon gain rounded to 1 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteNulls[FeAnte.TXHGMAX] = Constant.DB_NOT_NULL;
            }

            // Field #5 is the Tropospheric Scatter Distance.
            if ((rc = FeValidation.ParseFieldAsDoubleRound(qualLine, 5, out dTemp, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx trop. scatter invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): J: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx trop.  scatter end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): K: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.TXTRO] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna tx trop. scatter rounded to 2 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteStruct.txtro = (float)dTemp;
                anteNulls[FeAnte.TXTRO] = Constant.DB_NOT_NULL;
            }

            // Field #6 is the Precipitation Scatter Distance.
            if ((rc = FeValidation.ParseFieldAsDoubleRound(qualLine, 6, out dTemp, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx precip. scatter invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): L: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx precip.  scatter end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): M: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.TXPRE] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna tx precip. scatter rounded to 2 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteStruct.txpre = (float)dTemp;
                anteNulls[FeAnte.TXPRE] = Constant.DB_NOT_NULL;
            }

            // Field #7 is the Antenna Height.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 7, out anteStruct.aht, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna height invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): N: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna height end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): O: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.AHT] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna height rounded to 1 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteNulls[FeAnte.AHT] = Constant.DB_NOT_NULL;
            }

            // Field #8 is the Azimuth
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 8, out anteStruct.az, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna azimuth invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): P: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna azimuth end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.AZ] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna Azimuth rounded to 2 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteNulls[FeAnte.AZ] = Constant.DB_NOT_NULL;
            }

            // Field #9 is the Elevation Angle.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 9, out anteStruct.el, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna elevation angle invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): R: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna elevation angle end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AT(): S: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.EL] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna elevation angle rounded to 2 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteNulls[FeAnte.EL] = Constant.DB_NOT_NULL;
            }

            //...Log2.v("\nAnteRecord.Parse_AT(): Exit: importOK = " + importOK);

            return importOK;
        }

        /// <summary>
        /// This method parses a qualified line of type 'AR', validates its CSV fields and then
        /// partially populates an FeAnte object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_AR(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nAnteRecord.Parse_AR(): Entry: importOK = " + importOK);

            int rc;

            if (mARflag == Constant.SET)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of ANTENNA record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): A: importOK = Constant.FAILURE");
            }
            else
            {
                mARflag = Constant.SET;
            }

            // Field #0 is the type/qualifier.

            // Field #1 is the Band Code.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 1, out anteStruct.rxband, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx band code invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx band code end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.RXBAND] = Constant.DB_NULL;

            }
            else
            {
                anteNulls[FeAnte.RXBAND] = Constant.DB_NOT_NULL;
            }

            // Field #2 is the Antenna Code.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 2, out anteStruct.acoderx, 12))
                        == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna code RX invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna code RX end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.ACODERX] = Constant.DB_NULL;

            }
            else
            {
                anteNulls[FeAnte.ACODERX] = Constant.DB_NOT_NULL;
            }

            // Field #3 is the Antenna Feed System Loss.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 3, out anteStruct.afslr, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx antenna feed loss invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx antenna feed loss end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.AFSLR] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna rx feed loss rounded to 1 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteNulls[FeAnte.AFSLR] = Constant.DB_NOT_NULL;
            }

            // Field #4 is the Maximum Antenna Gain. 
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 4, out anteStruct.rxhgmax, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna max. rx Horizon gain invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): H: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna max. rx Horizon gain end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): I: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.RXHGMAX] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna max. rx Horizon gain rounded to 1 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteNulls[FeAnte.RXHGMAX] = Constant.DB_NOT_NULL;
            }

            // Field #5 is the Tropospheric Scatter Distance.
            if ((rc = FeValidation.ParseFieldAsDoubleRound(qualLine, 5, out dTemp, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx tropos. scatter invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): J: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx tropos. scatter end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): K: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.RXTRO] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna rx tropos. scatter rounded to 2 dp.\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteStruct.rxtro = (float)dTemp;
                anteNulls[FeAnte.RXTRO] = Constant.DB_NOT_NULL;
            }

            // Field #6 is the Precipitation Scatter Distance.
            if ((rc = FeValidation.ParseFieldAsDoubleRound(qualLine, 6, out dTemp, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx precip. scatter invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): L: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx precip.  scatter end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): M: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.RXPRE] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna rx precip. scatter rounded to 2 dp.\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteStruct.rxpre = (float)dTemp;
                anteNulls[FeAnte.RXPRE] = Constant.DB_NOT_NULL;
            }

            // Field #7 is the Gain/Temperature.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 7, out anteStruct.g_t, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna gain/temperature invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): N: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna gain/temperature end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): O: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.G_T] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna gain/temperature rounded to 1 dp.\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteNulls[FeAnte.G_T] = Constant.DB_NOT_NULL;
            }

            // Field #8 is the Noise Temperature.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 8, out anteStruct.lnat, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna Noise Temp. invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): P: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna Noise Temp. end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AR(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.LNAT] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna Noise Temp. rounded to 1 dp.\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteNulls[FeAnte.LNAT] = Constant.DB_NOT_NULL;
            }

            //...Log2.v("\nAnteRecord.Parse_AR(): Exit: importOK = " + importOK);

            return importOK;
        }

        /// <summary>
        /// This method parses a qualified line of type 'AS', validates its CSV fields and then
        /// completes the population of an FeAnte object with all the imported values; a record is then
        /// inserted into the _ante database table associated with the destination name.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="tab3Name"> - name of the table to be inserted into.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <param name="anteHandle"> - a cursor handle from a previous SQL SELECT query to be used for the SQL INSERT.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_AS(ref short importOK, string tab3Name, QualLine qualLine, int anteHandle)
        {
            //...Log2.v("\n\nAnteRecord.Parse_AS(): Entry: importOK = " + importOK);

            int rc;
            string strLongit;

            // Field #0 is the type/qualifier.

            // Field #1 is the Satellite Name.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 1, out anteStruct.satname, 16)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna satellite name invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): A: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna satellite name end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): B: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.SATNAME] = Constant.DB_NULL;

            }
            else
            {
                anteNulls[FeAnte.SATNAME] = Constant.DB_NOT_NULL;
            }

            // Field #2 is the Satellite Operator.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 2, out anteStruct.op2, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna satellite oper. code invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): C: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna satellite oper. code end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): D: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.OP2] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[FeAnte.OP2] = Constant.DB_NOT_NULL;
            }

            // Field #3 is the Orbit Type.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 3, out anteStruct.orbit, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna orbit code invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): E: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna orbit code end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): F: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.ORBIT] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[FeAnte.ORBIT] = Constant.DB_NOT_NULL;
            }

            // Field #4 is the Satellite Longitude.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 4, out strLongit, 7)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna satlong invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): G: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna satlong end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): H: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.SATLONGIT] = Constant.DB_NULL;
                anteNulls[FeAnte.SATLONG] = Constant.DB_NULL;
                anteNulls[FeAnte.SATLONGS] = Constant.DB_NULL;

            }
            else
            {
                c = Strings.LastChar(strLongit);
                if (!Char.IsDigit(c))
                {
                    anteStruct.satlongs = c.ToString();
                    anteNulls[FeAnte.SATLONGS] = Constant.DB_NOT_NULL;
                    strLongit = Strings.DropLastChar(strLongit);
                }
                anteStruct.satlong = Convert.ToSingle(strLongit);
                anteNulls[FeAnte.SATLONG] = Constant.DB_NOT_NULL;

                anteStruct.satlongit = (int)(anteStruct.satlong * 360000);
                if ((anteStruct.satlongs.Equals("E")) && (anteNulls[FeAnte.SATLONGS] != Constant.DB_NULL))
                {
                    anteStruct.satlongit *= -1;
                }
                anteNulls[FeAnte.SATLONGIT] = Constant.DB_NOT_NULL;
            }

            // Field #5 is the Arc Orbit Center.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 5, out anteStruct.sarc1, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna service arc invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): I: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna service arc end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): J: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.SARC1] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna service arc rounded to 2 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteNulls[FeAnte.SARC1] = Constant.DB_NOT_NULL;
            }

            // Field #6 is the Arc Half Width.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 6, out anteStruct.sarc2, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna 1/2 of service arc invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): K: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna 1/2 of service arc end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): L: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                anteNulls[FeAnte.SARC2] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Antenna 1/2 arc width rounded to 2 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                anteNulls[FeAnte.SARC2] = Constant.DB_NOT_NULL;
            }

            if ((mAKflag != Constant.SET)
            || (mATflag != Constant.SET)
            || (mARflag != Constant.SET))
            {
                msgBuf = String.Format("Error - line #{0}, improper order of ANTENNA record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): M: importOK = Constant.FAILURE");
            }

            if (FeRecExist.FeAnteExist(tab3Name, anteStruct))
            {
                /* record exists */
                msgBuf = String.Format("Error - line #{0}, duplicate Antenna record\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): N: importOK = Constant.FAILURE");
            }
            else if (FeRecExist.LastReturnCode != Constant.NOT_FOUND)
            {
                /* unexpected error in Exist function */
                ErrMsg.UtPrintMessage(rc);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AS(): O: importOK = Constant.FAILURE");
            }

            mAKflag = Constant.CLEAR;
            mATflag = Constant.CLEAR;
            mARflag = Constant.CLEAR;

            //...Log2.v("\n" + anteStruct.ToStringWN(anteNulls) + "\n");

            if (importOK == Constant.SUCCESS)
            {

                /* add to ingres file */
                if ((rc = DynFeAnte.FeInsertAnte(anteHandle, anteStruct, anteNulls)) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(rc);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nAnteRecord.Parse_AS(): P: importOK = Constant.FAILURE");
                }
                else
                {
                    //...Log2.v("\nAnteRecord.Parse_AS(): call to FeInsertAnte() succeeded.");
                }
            }

            //...Log2.v("\nAnteRecord.Parse_AS(): Exit: importOK = " + importOK);

            return importOK;
        }





    }
}

```
