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
    /// This class provides methods that parse and validate Azimuth records 
    /// (ZK) in an ES import file, instantiate and populate a
    /// FeAzim object with imported values and insert a record into an _azim table in the database.
    /// </summary>	
    public class AzimRecord
    {
        private static string msgBuf;
        private static FeAzim azimStruct = new FeAzim();
        private static SQLLEN[] azimNulls = NullHelper.CreateArrayOfNullInd(FeAzim.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

        /// This method parses a qualified line of type 'ZK', validates its CSV fields and then
        /// completes the population of an FeAzim object with all the imported values; a record is then
        /// inserted into the _azim database table associated with the destination name.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <param name="azimHandle"> - a cursor handle from a previous SQL SELECT query to be used for the SQL INSERT.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_ZK(ref short importOK, QualLine qualLine, int azimHandle)
        {
            //...Log2.v("\n\nAzimRecord.Parse_ZK(): Entry: importOK = " + importOK);

            int rc;

            // Field #0 is the qualifier/type.

            // Field #1 is 'Delete all associated Azimuth Angles', Y = Yes, N = No.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 1, out azimStruct.deleteall, 1))
                                                == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Azim deleteall invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR,
                    msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): A: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Azim deleteall end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): B: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                azimNulls[FeAzim.DELETEALL] = Constant.DB_NULL;
            }
            else
            {
                azimNulls[FeAzim.DELETEALL] = Constant.DB_NOT_NULL;
            }

            // Field #2 is MDB Operation (command).
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 2, out azimStruct.cmd, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth cmd invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth cmd end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                azimNulls[FeAzim.CMD] = Constant.DB_NULL;
            }
            else
            {
                azimNulls[FeAzim.CMD] = Constant.DB_NOT_NULL;
            }

            // Field #3 is the Record Status.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 3, out azimStruct.recstat, 1))
                                == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Azim recstat invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Azim recstat end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                azimNulls[FeAzim.RECSTAT] = Constant.DB_NULL;
            }
            else
            {
                azimNulls[FeAzim.RECSTAT] = Constant.DB_NOT_NULL;
            }

            // Field #4 is the Location.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 4, out azimStruct.location, FeAzim.LOCATION_SZ))
                             == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth location invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): H: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth location end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): I: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                azimNulls[FeAzim.LOCATION] = Constant.DB_NULL;
            }
            else
            {
                azimNulls[FeAzim.LOCATION] = Constant.DB_NOT_NULL;
            }

            // Field #5 is the Call Sign.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 5, out azimStruct.call1, 9)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth call1 invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): J: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth call1 end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): K: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                azimNulls[FeAzim.CALL1] = Constant.DB_NULL;
            }
            else
            {
                azimNulls[FeAzim.CALL1] = Constant.DB_NOT_NULL;
            }

            // Field #6 is the Azimuth. 
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 6, out azimStruct.azim, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth azimuth angle invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): L: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth azim end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): M: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                azimNulls[FeAzim.AZIM] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Azimuth azim rounded to 2 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                azimNulls[FeAzim.AZIM] = Constant.DB_NOT_NULL;
            }

            // Field #7 is the Elevation.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 7, out azimStruct.elev, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth elevation angle invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): N: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth elev end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): O: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                azimNulls[FeAzim.ELEV] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Azimuth elev rounded to 2 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                azimNulls[FeAzim.ELEV] = Constant.DB_NOT_NULL;
            }

            // Field #8 is the Distance.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 8, out azimStruct.dist, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth distance invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): P: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth dist end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                azimNulls[FeAzim.DIST] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Azimuth distance rounded to 2 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                azimNulls[FeAzim.DIST] = Constant.DB_NOT_NULL;
            }

            // Field #9 is the Loss.
            if ((rc = FeValidation.ParseFieldAsFloatRound(qualLine, 9, out azimStruct.loss, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth loss invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): R: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth loss end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): S: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                azimNulls[FeAzim.LOSS] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*NOTE - line #{0}, Azimuth loss rounded to 2 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                azimNulls[FeAzim.LOSS] = Constant.DB_NOT_NULL;
            }

            // Field #10 is the Modify Date.
            if ((rc = FeValidation.ParseFieldAsDate(qualLine, 10, out azimStruct.mdate, FeAzim.MDATE_SZ)) < 0)
            {
                msgBuf = String.Format("Error - line #{0}, Invalid date field; format should be yyyy.mm.dd  or  dd-mmm-yyyy (mmm = JAN, FEB, MAR etc)\r\n", qualLine.LineNum);
                importOK = (short)rc;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): T: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                azimNulls[FeAzim.MDATE] = Constant.DB_NULL;
            }
            else
            {
                azimNulls[FeAzim.MDATE] = Constant.DB_NOT_NULL;

                // Check that the date is numerically valid.
                if (!ValUtil.UtValidateDate(azimStruct.mdate))
                {
                    msgBuf = String.Format("Error - line #{0}, Invalid date field; the day, month or year is out of range.\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Error.BADDATE;
                    //...Log2.v("\n\nAzimRecord.Parse_ZK(): V: importOK = Constant.FAILURE");
                }
            }

            // Field #11 is the Modify Time.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 11, out azimStruct.mtime, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth modify time invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): W: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Azimuth mtime end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): X: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                azimNulls[FeAzim.MTIME] = Constant.DB_NULL;
            }
            else if (!FeValidation.IsValidTime(azimStruct.mtime))
            {
                msgBuf = String.Format("Error - line #{0}, Invalid time field; format should be hh:mm\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Error.BADTIME;
                //...Log2.v("\n\nAzimRecord.Parse_ZK(): X-1: importOK = Constant.FAILURE");
            }
            else
            {
                azimNulls[FeAzim.MTIME] = Constant.DB_NOT_NULL;
            }

            //...Log2.v("\n" + azimStruct.ToStringWN(azimNulls) + "\n");

            if (importOK == Constant.SUCCESS)
            {
                /* add to ingres file */
                if ((rc = DynFeAzim.FeInsertAzim(azimHandle, azimStruct, azimNulls))
                      != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(rc);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nAzimRecord.Parse_ZK(): Y: importOK = Constant.FAILURE");
                }
            }

            //...Log2.v("\nAzimRecord.Parse_ZK(): Exit: importOK = " + importOK);

            return importOK;
        }




    }
}
