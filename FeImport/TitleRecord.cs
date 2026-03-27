using _Configuration;
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
    /// This class provides methods that parse and validate Title records 
    /// (TD, TE) in an ES import file, instantiate and populate an
    /// FeTitl object with imported values and insert a record into an _titl table in the database.
    /// </summary>	
    public class TitleRecord
    {
        private static short mTEflag = Constant.CLEAR;
        private static string msgBuf;
        private static FeTitl titlStruct = new FeTitl();
        private static SQLLEN[] titlNulls = NullHelper.CreateArrayOfNullInd(FeTitl.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

        /// <summary>
        /// This method parses a qualified line of type 'TE', validates its CSV fields and then
        /// begins the population of an FeTitl object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_TE(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nTitleRecord.Parse_TE(): Entry: importOK = " + importOK);

            int rc;

            if (mTEflag == Constant.SET)
            {
                msgBuf = String.Format("Error - line #{0}, only one line with the 'TE' qualifier is permitted.\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Error.TOOFEWCSVFIELDS;
                //...Log2.v("\n\nTitleRecord.Parse_TE(): A: importOK = Constant.FAILURE");
            }
            else
            {
                mTEflag = Constant.SET;
            }

            // It is assumed that number of CSV fields found versus required
            // has already been validated.

            // Field #0 is the type/qualifier.

            // Field #1 is the record validation code.
            // Override whatever the user provided in this field by setting
            // the validated flag to 'N'.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 1, out titlStruct.validated, 1))
                == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Title validated flag invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TE(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Title validated flag end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TE(): C: importOK = Constant.FAILURE");
            }
            else
            {
                // Override whatever the user prescribed and set the validated flag to 'N'.
                titlStruct.validated = String.Format("{0}", Constant.NOT_VALIDATED);
                titlNulls[FeTitl.VALIDATED] = Constant.DB_NOT_NULL;
            }

            // Field #2 is the File Name.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 2, out titlStruct.namef, 16)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Title filename invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TE(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Title filename end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TE(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                titlNulls[FeTitl.NAMEF] = Constant.DB_NULL;
            }
            else
            {
                titlNulls[FeTitl.NAMEF] = Constant.DB_NOT_NULL;
            }

            // Field #3 is the Source (operator code).
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 3, out titlStruct.source, 6))
                == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Title source name field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TE(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Title source name end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TE(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                titlNulls[FeTitl.SOURCE] = Constant.DB_NULL;

            }
            else
            {
                titlNulls[FeTitl.SOURCE] = Constant.DB_NOT_NULL;
            }

            // Field #4 is the Modify Date.
            if ((rc = FeValidation.ParseFieldAsDate(qualLine, 4, out titlStruct.mdate, 25)) < 0)
            {
                msgBuf = String.Format("Error - line #{0}, Invalid date field; format should be yyyy.mm.dd  or  dd-mmm-yyyy (mmm = JAN, FEB, MAR etc)\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = (short)rc;
                //...Log2.v("\n\nTitleRecord.Parse_TE(): H: importOK = Constant.FAILURE");
            }
            else if (rc == 0)  // parsed string has zero length.
            {
                titlNulls[FeTitl.MDATE] = Constant.DB_NULL;
            }
            else
            {
                titlNulls[FeTitl.MDATE] = Constant.DB_NOT_NULL;

                // Check that the date is numerically valid.
                if (!ValUtil.UtValidateDate(titlStruct.mdate))
                {
                    msgBuf = String.Format("Error - line #{0}, Invalid date field; the day, month or year is out of range.\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Error.BADDATE;
                    //...Log2.v("\n\nTitleRecord.Parse_TE(): K: importOK = Constant.FAILURE");
                }
            }

            // Field #5 is the Modify Time.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 5, out titlStruct.mtime, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Title modify time invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TE(): L: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Title modify time end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TE(): M: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                titlNulls[FeTitl.MTIME] = Constant.DB_NULL;

            }
            else if (!FeValidation.IsValidTime(titlStruct.mtime))
            {
                msgBuf = String.Format("Error - line #{0}, Invalid time field; format should be hh:mm\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Error.BADTIME;
                //...Log2.v("\n\nTitleRecord.Parse_TE(): M-1: importOK = Constant.FAILURE");
            }
            else
            {
                titlNulls[FeTitl.MTIME] = Constant.DB_NOT_NULL;
            }

            //...Log2.v("\nTitleRecord.Parse_TE(): Exit: importOK = " + importOK);
            return importOK;
        }

        /// <summary>
        /// This method parses a qualified line of type 'TD', validates its CSV fields and then
        /// completes the population of an FeTitl object with all the imported values; a record is then
        /// inserted into the _titl database table associated with the destination name.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <param name="titlHandle"> - a cursor handle from a previous SQL SELECT query to be used for the SQL INSERT.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_TD(ref short importOK, QualLine qualLine, int titlHandle)
        {
            //...Log2.v("\n\nTitleRecord.Parse_TD(): Entry: importOK = " + importOK);

            int rc;

            // Field #0 is the type/qualifier.

            // Field #1 is the Title Description.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 1, out titlStruct.descr, 40))
                                    == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Title description invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TD(): A: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Title description end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TD(): B: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                titlNulls[FeTitl.DESCR] = Constant.DB_NULL;
            }
            else
            {
                titlNulls[FeTitl.DESCR] = Constant.DB_NOT_NULL;
            }


            if (mTEflag != Constant.SET)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of TITLE record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TD(): C: importOK = Constant.FAILURE");
            }

            //TEflag = Constant.CLEAR;

            //...Log2.v("\n" + titlStruct.ToStringWN(titlNulls) + "\n");

            if (importOK == Constant.SUCCESS)
            {
                /* add to ingres file */
                if ((rc = DynFeTitl.FeInsertTitl(titlHandle, titlStruct, titlNulls))
                      != Constant.SUCCESS)
                {
                    //...Log2.e("\nTitleRecord.Parse_TD(): ERROR: call to FeInsertTitl() failed.");
                    ErrMsg.UtPrintMessage(rc);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nTitleRecord.Parse_TD(): D: importOK = Constant.FAILURE");
                }
                else
                {
                    //...Log2.v("\nTitleRecord.Parse_TD(): call to FeInsertTitl() succeeded.");
                }
            }

            //...Log2.v("\nTitleRecord.Parse_TD(): Exit: importOK = " + importOK);
            return importOK;
        }






    }
}
