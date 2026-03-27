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
    /// This class provides methods that parse and validate Channel records 
    /// (CK, CR, CT) in an ES import file, instantiate and populate an
    /// FeChan object with imported values and insert a record into an _chan table in the database.
    /// </summary>	
    public class ChanRecord
    {
        private static short mCKflag = Constant.CLEAR;
        private static short mCTflag = Constant.CLEAR;
        private static string msgBuf;

        private static FeChan chanStruct = new FeChan();
        private static SQLLEN[] chanNulls = NullHelper.CreateArrayOfNullInd(FeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

        /// <summary>
        /// This method parses a qualified line of type 'CK', validates its CSV fields and then
        /// partially populates an FeChan object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_CK(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nChanRecord.Parse_CK(): Entry: importOK = " + importOK);

            int rc;

            if (mCKflag == Constant.SET)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of CHANNEL record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Error.TOOFEWCSVFIELDS;
                //...Log2.v("\n\nChanRecord.Parse_CK(): A: importOK = Constant.FAILURE");
            }
            else
            {
                mCKflag = Constant.SET;
            }

            // Field #0 is the type/qualifier.

            // Field # 1 is the MDB Operation (command).
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 1, out chanStruct.cmd, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel cmd invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel cmd end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.CMD] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.CMD] = Constant.DB_NOT_NULL;
            }

            // Field # 2 is the Record Status.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 2, out chanStruct.recstat, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel recstat invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel recstat end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.RECSTAT] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.RECSTAT] = Constant.DB_NOT_NULL;
            }

            // Field # 3 is the Location.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 3, out chanStruct.location, FeChan.LOCATION_SZ)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel location invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel location end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.LOCATION] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.LOCATION] = Constant.DB_NOT_NULL;
            }

            // Field # 4 is the Call Sign.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 4, out chanStruct.call1, 9)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel call1 invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): H: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel call1 end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): I: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.CALL1] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.CALL1] = Constant.DB_NOT_NULL;
            }

            // Field #5 is the Channel Id. 
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 5, out chanStruct.chid, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel channel id invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): J: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel channel id end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): K: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.CHID] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.CHID] = Constant.DB_NOT_NULL;
            }

            // Field #6 is the Notes. 
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 6, out chanStruct.notc, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel channel note invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): L: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel channel note end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): M: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.NOTC] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.NOTC] = Constant.DB_NOT_NULL;
            }

            // Field #7 is the Modify Date.
            if ((rc = FeValidation.ParseFieldAsDate(qualLine, 7, out chanStruct.mdate, FeChan.MDATE_SZ)) < 0)
            {
                msgBuf = String.Format("Error - line #{0}, Invalid date field; format should be yyyy.mm.dd  or  dd-mmm-yyyy (mmm = JAN, FEB, MAR etc)\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = (short)rc;
                //...Log2.v("\n\nChanRecord.Parse_CK(): N: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.MDATE] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.MDATE] = Constant.DB_NOT_NULL;

                // Check that the date is numerically valid.
                if (!ValUtil.UtValidateDate(chanStruct.mdate))
                {
                    msgBuf = String.Format("Error - line #{0}, Invalid date field; the day, month or year is out of range.\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Error.BADDATE;
                    //...Log2.v("\n\nChanRecord.Parse_CK(): P: importOK = Constant.FAILURE");
                }
            }

            // Field #8 is the Modify Time.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 8, out chanStruct.mtime, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel modify time invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel modify time end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): R: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.MTIME] = Constant.DB_NULL;
            }
            else if (!FeValidation.IsValidTime(chanStruct.mtime))
            {
                msgBuf = String.Format("Error - line #{0}, Invalid time field; format should be hh:mm\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Error.BADTIME;
                //...Log2.v("\n\nChanRecord.Parse_CK(): R-1: importOK = Constant.FAILURE");
            }
            else
            {
                chanNulls[FeChan.MTIME] = Constant.DB_NOT_NULL;
            }

            //...Log2.v("\nChanRecord.Parse_CK(): Exit: importOK = " + importOK);
            return importOK;
        }

        /// <summary>
        /// This method parses a qualified line of type 'CT', validates its CSV fields and then
        /// partially populates an FeChan object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_CT(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nChanRecord.Parse_CT(): Entry: importOK = " + importOK);

            int rc;

            if (mCTflag == Constant.SET)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of CHANNEL record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): A: importOK = Constant.FAILURE");
            }
            else
            {
                mCTflag = Constant.SET;
            }

            // Field #0 is the type/qualifier.

            // Field #1 is the Frequency.
            if ((rc = FeValidation.ParseFieldAsDoubleRound(qualLine, 1, out chanStruct.freqtx, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx frequency invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx frequency end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.FREQTX] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Channel tx frequency rounded to 2 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                chanNulls[FeChan.FREQTX] = Constant.DB_NOT_NULL;
            }

            // Field #2 is the Polarization.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 2, out chanStruct.poltx, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx polarization invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx polarization end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.POLTX] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.POLTX] = Constant.DB_NOT_NULL;
            }

            // Field #3 is the Maximum Transmit Power.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 3, out chanStruct.maxtxpower, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel max. tx power invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel max. tx power end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.MAXTXPOWER] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Channel max tx power rounded to 1 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                chanNulls[FeChan.MAXTXPOWER] = Constant.DB_NOT_NULL;
            }

            // Field #4 is the Default/Normal Power. 
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 4, out chanStruct.pwrtx, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx power invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): H: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx power end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): I: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.PWRTX] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Channel tx power rounded to 2 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                chanNulls[FeChan.PWRTX] = Constant.DB_NOT_NULL;
            }

            // Field #5 is the Energy Dispersal.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 5, out chanStruct.p4khz, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel pk4hz invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): J: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel pk4khz end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): K: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.P4KHZ] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Channel pk4khz rounded to 1 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                chanNulls[FeChan.P4KHZ] = Constant.DB_NOT_NULL;
            }

            // Field #6 is the Equipment. 
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 6, out chanStruct.eqpttx, 8)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx equipment invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): L: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx equipment end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): M: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.EQPTTX] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.EQPTTX] = Constant.DB_NOT_NULL;
            }

            // Field #7 is the Traffic Code. 
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 7, out chanStruct.traftx, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx traffic code invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): N: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx traffic code end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): O: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.TRAFTX] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.TRAFTX] = Constant.DB_NOT_NULL;
            }

            // Field #8 is the Status.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 8, out chanStruct.stattx, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx status invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): P: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx status end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.STATTX] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.STATTX] = Constant.DB_NOT_NULL;
            }

            // Field #9 is the Service Code.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 9, out chanStruct.srvctx, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx service invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): R: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx service end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): S: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.SRVCTX] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.SRVCTX] = Constant.DB_NOT_NULL;
            }

            // Field #10 is the Fee Code.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 10, out chanStruct.feetx, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx fee code invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): T: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx fee code end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): U: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.FEETX] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.FEETX] = Constant.DB_NOT_NULL;
            }

            //...Log2.v("\nChanRecord.Parse_CT(): Exit: importOK = " + importOK);
            return importOK;
        }

        /// This method parses a qualified line of type 'CR', validates its CSV fields and then
        /// completes the population of an FeChan object with all the imported values; a record is then
        /// inserted into the _chan database table associated with the destination name.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="tab4Name"> - name of the table to be inserted into.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <param name="chanHandle"> - a cursor handle from a previous SQL SELECT query to be used for the SQL INSERT.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_CR(ref short importOK, string tab4Name, QualLine qualLine, int chanHandle)
        {
            //...Log2.v("\n\nChanRecord.Parse_CR(): Entry: importOK = " + importOK);

            int rc;

            // Field #0 is the type/qualifier.

            // Field #1 is the Frequency. 
            if ((rc = FeValidation.ParseFieldAsDoubleRound(qualLine, 1, out chanStruct.freqrx, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx frequency invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): A: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx frequency end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): B: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.FREQRX] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Channel rx frequency rounded to 2 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                chanNulls[FeChan.FREQRX] = Constant.DB_NOT_NULL;
            }

            // Field #2 is the Polarization.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 2, out chanStruct.polrx, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx polarity invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): C: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx polarity end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): D: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.POLRX] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.POLRX] = Constant.DB_NOT_NULL;
            }

            // Field #3 is the Default/Normal Power. 
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 3, out chanStruct.pwrrx, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx power invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): E: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx power end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): F: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.PWRRX] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Channel rx power rounded to 1 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                chanNulls[FeChan.PWRRX] = Constant.DB_NOT_NULL;
            }

            // Field #4 is the Equipment.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 4, out chanStruct.eqptrx, 8)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx equipment invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): G: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx equipment end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): H: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.EQPTRX] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.EQPTRX] = Constant.DB_NOT_NULL;
            }

            // Field #5 is the Traffic Code.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 5, out chanStruct.trafrx, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx traffic code invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): I: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx traffic end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): J: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.TRAFRX] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.TRAFRX] = Constant.DB_NOT_NULL;
            }

            // Field #6 is the Long Term Inteference Obj.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 6, out chanStruct.i20, 1)) == Constant.UT_INV_CONV)
            {
                //AH: changed (20%) to (20%%) to 'escape' the % character
                msgBuf = String.Format("Error - line #{0}, Channel interference power (20%%) invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): K: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                //AH: changed (20%) to (20%%) to 'escape' the % character
                msgBuf = String.Format("Error - line #{0}, Channel interference power (20%%) end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): L: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.I20] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Channel interference power rounded to 1 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                chanNulls[FeChan.I20] = Constant.DB_NOT_NULL;
            }

            // Field #7 is the Short Term Tropospheric Interference Obj.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 7, out chanStruct.it01, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tropospheric interference invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): M: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tropospheric end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): N: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.IT01] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Channel tropospheric interference rounded to 1 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                chanNulls[FeChan.IT01] = Constant.DB_NOT_NULL;
            }

            // Field #8 is the Short Term Precipitation Interference Obj.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 8, out chanStruct.ip01, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel precipitation interference invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): O: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel precipitation interference end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): P: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.IP01] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Channel precipitation interference rounded to 1 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                chanNulls[FeChan.IP01] = Constant.DB_NOT_NULL;
            }

            // Field #9 is the Status.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 9, out chanStruct.statrx, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx status invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx status end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): R: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.STATRX] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.STATRX] = Constant.DB_NOT_NULL;
            }

            // Field #10 is the Service Code.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 10, out chanStruct.srvcrx, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx service invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): S: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx service end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): T: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.SRVCRX] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.SRVCRX] = Constant.DB_NOT_NULL;
            }

            // Field #11 is the Fee Code.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 11, out chanStruct.feerx, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx fee code invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): U: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx fee code end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): V: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                chanNulls[FeChan.FEERX] = Constant.DB_NULL;
            }
            else
            {
                chanNulls[FeChan.FEERX] = Constant.DB_NOT_NULL;
            }

            if ((mCKflag != Constant.SET)
            || (mCTflag != Constant.SET))
            {
                msgBuf = String.Format("Error - line #{0}, improper order of CHANNEL record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): W: importOK = Constant.FAILURE");
            }

            if (FeRecExist.FeChanExist(tab4Name, chanStruct))
            {
                /* record exists */
                msgBuf = String.Format("Error - line #{0}, duplicate Channel record\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): X: importOK = Constant.FAILURE");
            }
            else if (FeRecExist.LastReturnCode != Constant.NOT_FOUND)
            {
                /* unexpected error in Exist function */
                ErrMsg.UtPrintMessage(rc);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): Y: importOK = Constant.FAILURE");
            }

            mCKflag = Constant.CLEAR;
            mCTflag = Constant.CLEAR;

            //...Log2.v("\n" + chanStruct.ToStringWN(chanNulls) + "\n");

            if (importOK == Constant.SUCCESS)
            {

                /* add to ingres file */
                if ((rc = DynFeChan.FeInsertChan(chanHandle, chanStruct, chanNulls)) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(rc);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nChanRecord.Parse_CR(): Z: importOK = Constant.FAILURE");
                }
                else
                {
                    //...Log2.v("\nChanRecord.Parse_CR(): call to FeInsertChan() succeeded.");
                }
            }

            //...Log2.v("\nChanRecord.Parse_CR(): Exit: importOK = " + importOK);
            return importOK;
        }




    }
}
