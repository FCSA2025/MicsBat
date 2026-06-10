# Documented File: SiteRecord.cs
**Repository Path:** `FeImport\SiteRecord.cs`
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
    /// This class provides methods that parse and validate Site records 
    /// (SD, SK) in an ES import file, instantiate and populate an
    /// FeSite object with imported values and insert a record into an _site table in the database.
    /// </summary>	
    public class SiteRecord
    {
        private static short mSKflag = Constant.CLEAR;
        private static string msgBuf;
        private static FeSite siteStruct = new FeSite();
        private static SQLLEN[] siteNulls = NullHelper.CreateArrayOfNullInd(FeSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

        /// <summary>
        /// This method parses a qualified line of type 'SK', validates its CSV fields and then
        /// partially populates an FeSite object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_SK(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nSiteRecord.Parse_SK(): Entry: importOK = " + importOK);

            int rc;

            mSKflag = Constant.SET;

            // Field #0 is the type/qualifier.

            // Field #1 is the MDB Operation (command).
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 1, out siteStruct.cmd, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site cmd invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site cmd end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.CMD] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FeSite.CMD] = Constant.DB_NOT_NULL;
            }

            // Field #2 is the Record Status.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 2, out siteStruct.recstat, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site recstat invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site recstat end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.RECSTAT] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FeSite.RECSTAT] = Constant.DB_NOT_NULL;
            }

            // Field #3 is the Location.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 3, out siteStruct.location, FeSite.LOCATION_SZ)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site location invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site location end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.LOCATION] = Constant.DB_NULL;

            }
            else
            {

                siteNulls[FeSite.LOCATION] = Constant.DB_NOT_NULL;
            }

            // Field #4 is the Station Name.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 4, out siteStruct.name, 16)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site name invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): H: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site name end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): I: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.NAME] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FeSite.NAME] = Constant.DB_NOT_NULL;
            }

            // Field #5 is the Province.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 5, out siteStruct.prov, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site province invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): J: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site province end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): K: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.PROV] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FeSite.PROV] = Constant.DB_NOT_NULL;
            }

            // Field #6 is the Operator Code.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 6, out siteStruct.oper, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site operator code invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): L: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site operator code end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): M: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.OPER] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FeSite.OPER] = Constant.DB_NOT_NULL;
            }

            // Field #7 is the Operator Type.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 7, out siteStruct.oprtyp, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site operator type invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): N: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site operator type end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): O: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.OPRTYP] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FeSite.OPRTYP] = Constant.DB_NOT_NULL;
            }

            // Field #8 is the Modify Date.
            if ((rc = FeValidation.ParseFieldAsDate(qualLine, 8, out siteStruct.mdate, 25)) < 0)
            {
                msgBuf = String.Format("Error - line #{0}, Invalid date field; format should be yyyy.mm.dd  or  dd-mmm-yyyy (mmm = JAN, FEB, MAR etc)\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = (short)rc;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): P: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.MDATE] = Constant.DB_NULL;
            }
            else
            {
                siteNulls[FeSite.MDATE] = Constant.DB_NOT_NULL;

                // Check that the date is numerically valid.
                if (!ValUtil.UtValidateDate(siteStruct.mdate))
                {
                    msgBuf = String.Format("Error - line #{0}, Invalid date field; the day, month or year is out of range.\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Error.BADDATE;
                    //...Log2.v("\n\nSiteRecord.Parse_SK(): R: importOK = Constant.FAILURE");
                }
            }

            // Field #9 is the Modify Time.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 9, out siteStruct.mtime, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site modify time invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): S: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site modify time end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): T: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.MTIME] = Constant.DB_NULL;

            }
            else if (!FeValidation.IsValidTime(siteStruct.mtime))
            {
                msgBuf = String.Format("Error - line #{0}, Invalid time field; format should be hh:mm\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Error.BADTIME;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): T-1: importOK = Constant.FAILURE");
            }
            else
            {
                siteNulls[FeSite.MTIME] = Constant.DB_NOT_NULL;
            }

            //...Log2.v("\nSiteRecord.Parse_SK(): Exit: importOK = " + importOK);
            return importOK;
        }

        /// <summary>
        /// This method parses a qualified line of type 'SD', validates its CSV fields and then
        /// completes the population of an FeSite object with all the imported values; a record is then
        /// inserted into the _site database table associated with the destination name.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="tab2Name"> - full name of the table to be inserted into.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <param name="siteHandle"> - a cursor handle from a previous SQL SELECT query to be used for the SQL INSERT.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_SD(ref short importOK, string tab2Name, QualLine qualLine, int siteHandle)
        {
            //...Log2.v("\n\nSiteRecord.Parse_SD(): Entry: importOK = " + importOK);

            int rc;
            int nRet;
            string strLatit;
            string strLongit;
            string strOrientation;

            // Field #0 is the type/qualifier.

            // Field #1 is the Latitude + Sense.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 1, out strLatit, 15)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site latitude invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): A: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site latitude end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): B: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.LATIT] = Constant.DB_NULL;
            }
            else
            {
                /* convert strLatit from 00-00-00.00N to
                    hundreths of seconds */
                strOrientation = Strings.LastChar(strLatit).ToString();

                strLatit = strLatit.Substring(0, strLatit.Length - 1);

                //...Log2.v("\n\nSiteRecord.Parse_SD(): strLatit = " + strLatit);

                nRet = GenUtil.UtStrConvLongLat(Constant.LATITUDE, strLatit, strOrientation, out siteStruct.latit);

                if (nRet != Constant.SUCCESS)
                {
                    msgBuf = String.Format("Error - line #{0}, Site latitude invalid conversion\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nSiteRecord.Parse_SD(): C: importOK = Constant.FAILURE");
                }
                else
                {
                    siteNulls[FeSite.LATIT] = Constant.DB_NOT_NULL;
                }
            }

            // Field #2 is Longitude + Sense.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 2, out strLongit, 15)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site longitude invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site longitude end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.LONGIT] = Constant.DB_NULL;
            }
            else
            {
                /* convert strLongit from 00-00-00.00E to
                    hundreths of seconds */
                strOrientation = Strings.LastChar(strLongit).ToString();

                strLongit = strLongit.Substring(0, strLongit.Length - 1);

                nRet = GenUtil.UtStrConvLongLat(Constant.LONGITUDE, strLongit, strOrientation, out siteStruct.longit);
                if (nRet != Constant.SUCCESS)
                {
                    msgBuf = String.Format("Error - line #{0}, Site longitude invalid conversion\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nSiteRecord.Parse_SD(): F: importOK = Constant.FAILURE");
                }
                else
                {
                    siteNulls[FeSite.LONGIT] = Constant.DB_NOT_NULL;
                }
            }

            // Field #3 is the Ground Height of the site.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 3, out siteStruct.grnd, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site ground height invalid conversion\r\n",
                                qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): G: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site ground height end of data found\r\n",
                                qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): H: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.GRND] = Constant.DB_NULL;
            }
            else
            {
                siteNulls[FeSite.GRND] = Constant.DB_NOT_NULL;
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Site ground height rounded to 1 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
            }

            // Field #4 is Radio.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 4, out siteStruct.radio, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site radio invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): I: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site radio end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): J: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.RADIO] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FeSite.RADIO] = Constant.DB_NOT_NULL;
            }

            // Field #5 is Rain.
            if ((rc = FeValidation.ParseFieldAsShort(qualLine, 5, out siteStruct.rain)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site rain zone invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): K: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site rain zone end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): L: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.RAIN] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FeSite.RAIN] = Constant.DB_NOT_NULL;
            }

            // Field #6 is Site Status.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 6, out siteStruct.stats, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site status invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): M: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site status end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): N: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.STATS] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FeSite.STATS] = Constant.DB_NOT_NULL;
            }

            // Field # 7 is Notes.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 7, out siteStruct.nots, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site notes invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): O: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site notes end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): P: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.NOTS] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FeSite.NOTS] = Constant.DB_NOT_NULL;
            }

            // Field #8 is Region.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 8, out siteStruct.reg, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Site region invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Site region end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): R: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.REG] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FeSite.REG] = Constant.DB_NOT_NULL;
            }

            // Field #9 is Service Date.
            if ((rc = FeValidation.ParseFieldAsDate(qualLine, 9, out siteStruct.sdate, Constant.DATE_SZ)) < 0)
            {
                msgBuf = String.Format("Error - line #{0}, Invalid date field; format should be yyyy.mm.dd  or  dd-mmm-yyyy (mmm = JAN, FEB, MAR etc)\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = (short)rc;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): S: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FeSite.SDATE] = Constant.DB_NULL;
            }
            else
            {
                siteNulls[FeSite.SDATE] = Constant.DB_NOT_NULL;

                // Check that the date is numerically valid.
                if (!ValUtil.UtValidateDate(siteStruct.sdate))
                {
                    msgBuf = String.Format("Error - line #{0}, Invalid date field; the day, month or year is out of range.\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Error.BADDATE;
                    //...Log2.v("\n\nSiteRecord.Parse_SD(): U: importOK = Constant.FAILURE");
                }
            }

            if (mSKflag != Constant.SET)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of SITE record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): V: importOK = Constant.FAILURE");
            }

            if (FeRecExist.FeSiteExist(tab2Name, siteStruct))
            {
                /* record exists */
                msgBuf = String.Format("Error - line #{0}, duplicate Site record\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): W: importOK = Constant.FAILURE");
            }
            else if (FeRecExist.LastReturnCode != Constant.NOT_FOUND)
            {
                /* unexpected error in Exist function */
                ErrMsg.UtPrintMessage(rc);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): X: importOK = Constant.FAILURE");
            }

            mSKflag = Constant.CLEAR;

            //...Log2.v("\n" + siteStruct.ToStringWN(siteNulls) + "\n");

            if (importOK == Constant.SUCCESS)
            {
                // Insert record into DB table.
                rc = DynFeSite.FeInsertSite(siteHandle, siteStruct, siteNulls);
                if (rc != Constant.SUCCESS)
                {
                    Console.Write("\r\nError ({0}) inserting site: '{0}' from record {1}.", rc, siteStruct.location, qualLine.LineNum);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nSiteRecord.Parse_SD(): Y: importOK = Constant.FAILURE");
                }
                else
                {
                    //...Log2.v("\nSiteRecord.Parse_SD()(): call to FeInsertSite() succeeded.");
                }
            }

            //...Log2.v("\nSiteRecord.Parse_SD(): Exit: importOK = " + importOK);
            return importOK;
        }




    }
}

```
