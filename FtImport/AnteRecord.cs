using _Configuration;
using _DataStructures;
using _Utillib;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtImport
{
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides methods that parse and validate Antenna records 
    /// (AK, AQ, AO) in an TS import file, instantiate and populate an
    /// FtAnte object with imported values and to insert a record into an _ante table in the database.
    /// </summary>
    public class AnteRecord
    {
        private static string msgBuf;

        private static FtAnte ftAnte;
        private static SQLLEN[] ftAnteNullInds;

        /// <summary>
        /// This method parses qualified lines of type 'AK', validates its CSV fields and then
        /// partially populates an FtAnte object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_AK(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nAnteRecord.Parse_AK(): Entry: importOK = " + importOK);

            ftAnte = State.Ante;
            ftAnteNullInds = State.AnteNullInds;

            int rc;

            if (State.AKflagSet)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of ANTENNA record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): A: importOK = Constant.FAILURE");
            }
            else
            {
                State.AKflagSet = true;
            }

            //================================
            // Field #0 is the type/qualifier.
            //================================

            //===========================
            // Field #1 is MDB Operation.                   MANDATORY
            //===========================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 1, out ftAnte.cmd, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna cmd field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna cmd field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.CMD] = Constant.DB_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.CMD] = Constant.DB_NOT_NULL;
            }

            //===============================
            // Field #2 is the Record Status.
            //===============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 2, out ftAnte.recstat, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna recstat field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna recstat field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.RECSTAT] = Constant.DB_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.RECSTAT] = Constant.DB_NOT_NULL;
            }

            //=========================================
            // Field #3 is the Local Call Sign (call1).     MANDATORY,  Non-Nullable
            //=========================================
            rc = FtParsing.ParseFieldAsString(qualLine, 3, out ftAnte.call1, 9);
            if (rc == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna call local field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna call local field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                msgBuf = String.Format("Error - line #{0}, call1 field MUST be present in order to import.\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                ftAnteNullInds[FtAnte.CALL1] = Constant.DB_NOT_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.CALL1] = Constant.DB_NOT_NULL;
            }

            //==========================================
            // Field #4 is the Remote Call Sign (call2).    MANDATORY,  Non-Nullable
            //==========================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 4, out ftAnte.call2, 9)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna call remote field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): H: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna call remote field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): I: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                msgBuf = String.Format("Error - line #{0}, call2 field MUST be present in order to import.\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                ftAnteNullInds[FtAnte.CALL2] = Constant.DB_NOT_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.CALL2] = Constant.DB_NOT_NULL;
            }

            //============================
            // Field # 5 is the Band Code.                  MANDATORY,  Non-Nullable
            //============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 5, out ftAnte.bndcde, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna band code field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): J: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna band code field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): K: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                msgBuf = String.Format("Error - line #{0}, band code MUST be present in order to import.\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                ftAnteNullInds[FtAnte.BNDCDE] = Constant.DB_NOT_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.BNDCDE] = Constant.DB_NOT_NULL;
            }

            //================================
            // Field #6 is the Antenna Number.              MANDATORY,  Non-Nullable
            //================================
            if ((rc = FtParsing.ParseFieldAsShort(qualLine, 6, out ftAnte.anum)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna antenna number field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): L: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna antenna number field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): M: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                msgBuf = String.Format("Error - line #{0}, antenna number field MUST be present in order to import.\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                ftAnteNullInds[FtAnte.ANUM] = Constant.DB_NOT_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.ANUM] = Constant.DB_NOT_NULL;
            }

            //=============================
            // Field #7 is the Modify Date.
            //=============================
            if ((rc = FtParsing.ParseFieldAsDate(qualLine, 7, out ftAnte.mdate, 25)) < 0)
            {
                msgBuf = String.Format("Error - line #{0}, Invalid date field; format should be yyyy.mm.dd  or  dd-mmm-yyyy (mmm = JAN, FEB, MAR etc)\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = (short)rc;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): R: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna modify date field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): M: importOK = Constant.FAILURE");
            }
            else if (rc == 0)  // string has zero length.
            {
                ftAnteNullInds[FtAnte.MDATE] = Constant.DB_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.MDATE] = Constant.DB_NOT_NULL;

                // Check that the date is numerically valid.
                if (!ValUtil.UtValidateDate(ftAnte.mdate))
                {
                    msgBuf = String.Format("Error - line #{0}, import Antenna modify date field, invalid date\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nAnteRecord.Parse_AK(): T: importOK = Constant.FAILURE");
                }
            }

            // Field #8 is the Modify Time.
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 8, out ftAnte.mtime, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna modify time field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): U: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna modify time field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): V: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.MTIME] = Constant.DB_NULL;
            }
#if false   // The following code is excluded so as to mimic the behaviour
            // of the legacy C/C++ code.
            else if (!FtValidation.IsValidTime(anteStruct.mtime))
            {
                msgBuf = String.Format("Error - line #{0}, Invalid time field; format should be hh:mm\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Error.BADTIME;
                //...Log2.v("\n\nAnteRecord.Parse_AK(): V: importOK = Constant.FAILURE");
            }
#endif
            else
            {
                ftAnteNullInds[FtAnte.MTIME] = Constant.DB_NOT_NULL;
            }

            //=====================================================
            // Check if there are anomalous, additional CSV fields.
            //=====================================================
            string dummy;
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 9, out dummy, 1)) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, import Site; extra fields ignored\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                //...Log2.v("\n\nTitleRecord.Parse_TT(): M-1: importOK = Constant.FAILURE");
            }

            //...Log2.v("\nAnteRecord.Parse_AK(): Exit: importOK = " + importOK);

            return importOK;
        }

        /// <summary>
        /// This method parses a qualified line of type 'AQ', validates its CSV fields and then
        /// partially populates an FtAnte object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_AQ(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nAnteRecord.Parse_AQ(): Entry: importOK = " + importOK);

            ftAnte = State.Ante;
            ftAnteNullInds = State.AnteNullInds;

            int rc;

            if (State.AQflagSet)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of ANTENNA record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): A: importOK = Constant.FAILURE");
            }
            else
            {
                State.AQflagSet = true;
            }

            // Field #0 is the type/qualifier.

            //=============================
            // Field #1 is the Antenna Use.
            //=============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 1, out ftAnte.ause, 3)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna antenna use field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna antenna use field, end of data found\\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.AUSE] = Constant.DB_NULL;

            }
            else
            {
                ftAnteNullInds[FtAnte.AUSE] = Constant.DB_NOT_NULL;
            }

            //==============================
            // Field #2 is the Antenna Code.
            //==============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 2, out ftAnte.acode, 12)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna antenna code field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna antenna code field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.ACODE] = Constant.DB_NULL;

            }
            else
            {
                ftAnteNullInds[FtAnte.ACODE] = Constant.DB_NOT_NULL;
            }

            //================================
            // Field #3 is the Antenna Height.
            //================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 3, out ftAnte.aht, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna height field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna height field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.AHT] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna height rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.AHT] = Constant.DB_NOT_NULL;
            }

            //=========================
            // Field #4 is the Azimuth.
            //=========================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 4, out ftAnte.azmth, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna azimuth field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): H: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna azimuth field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): I: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.AZMTH] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #%d, Antenna azimuth rounded to 2 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.AZMTH] = Constant.DB_NOT_NULL;
            }

            //=================================
            // Field #5 is the Elevation Angle.
            //=================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 5, out ftAnte.elvtn, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna elevation angle field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): J: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna elevation angle field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): K: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.ELVTN] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, *Note - line #%d, Antenna elevation rounded to 2 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.ELVTN] = Constant.DB_NOT_NULL;
            }

            // Field #6 is the Distance.
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 6, out ftAnte.dist, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna distance, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): L: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antenna distance, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): M: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.DIST] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna distance rounded to 2 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.DIST] = Constant.DB_NOT_NULL;
            }

            // Field #7 is the Antenna Height.
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 7, out ftAnte.offazm, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna off azimuth field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): N: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna off azimuth field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): O: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.OFFAZM] = Constant.DB_NULL;

            }
            else
            {
                ftAnteNullInds[FtAnte.OFFAZM] = Constant.DB_NOT_NULL;
            }

            //==============================
            // Field #8 is the True Azimuth.
            //==============================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 8, out ftAnte.tazmth, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna true azimuth field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): P: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna true azimuth field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.TAZMTH] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna true azimuth rounded to 2 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.TAZMTH] = Constant.DB_NOT_NULL;
            }

            //======================================
            // Field #9 is the True Elevation Angle.
            //======================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 9, out ftAnte.telvtn, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna height true elevation field, invalid conversion\\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): R: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna height true elevation field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): S: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.TELVTN] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna true elevation rounded to 2 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.TELVTN] = Constant.DB_NOT_NULL;
            }

            //======================================
            // Field #10 is the True Gain.
            //======================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 10, out ftAnte.tgain, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna true gain field, invalid conversion\\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): R: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna true gain field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): S: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.TGAIN] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna true gain rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.TGAIN] = Constant.DB_NOT_NULL;
            }

            //===================================
            // Field #11 is the Obstruction Loss.
            //===================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 11, out ftAnte.obsloss, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna obstruction loss field, invalid conversion\\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): R: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna obstruction loss field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): S: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.OBSLOSS] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna obstruction loss rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.OBSLOSS] = Constant.DB_NOT_NULL;
            }

            //===================================
            // Field #12 is the Kvalue.
            //===================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 12, out ftAnte.kvalue, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna k value field, invalid conversion\\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): R: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna k value field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): S: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.KVALUE] = Constant.DB_NULL;

            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna k value rounded to 2 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.KVALUE] = Constant.DB_NOT_NULL;
            }

            //===============================
            // Field #13 is the Tower Number.
            //===============================
            if ((rc = FtParsing.ParseFieldAsShort(qualLine, 13, out ftAnte.atwrno)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tower number field, invalid conversion\\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): R: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tower number field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): S: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.ATWRNO] = Constant.DB_NULL;

            }
            else
            {
                ftAnteNullInds[FtAnte.ATWRNO] = Constant.DB_NOT_NULL;
            }

            //===============================
            // Field #14 is the Service Date.
            //===============================
            if ((rc = FtParsing.ParseFieldAsDate(qualLine, 14, out ftAnte.sdate, 25)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna service date field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TT(): H: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna service date field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TT(): H-1: importOK = Constant.FAILURE");
            }
            else if (rc == 0)  // parsed string has zero length.
            {
                ftAnteNullInds[FtAnte.SDATE] = Constant.DB_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.SDATE] = Constant.DB_NOT_NULL;

                // Check that the date is numerically valid.
                if (!ValUtil.UtValidateDate(ftAnte.sdate))
                {
                    msgBuf = String.Format("Error - line #{0}, Antenna service date field, invalid date\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nTitleRecord.Parse_TT(): K: importOK = Constant.FAILURE");
                }
            }

            //=====================================================
            // Check if there are anomalous, additional CSV fields.
            //=====================================================
            string dummy;
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 15, out dummy, 1)) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, Antenna; extra fields ignored\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                //...Log2.v("\n\nTitleRecord.Parse_TT(): M-1: importOK = Constant.FAILURE");
            }

            //...Log2.v("\nAnteRecord.Parse_AQ(): Exit: importOK = " + importOK);
            return importOK;
        }

        /// <summary>
        /// This method parses a qualified line of type 'AO', validates its CSV fields and then
        /// partially populates an FtAnte object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <param name="anteHandle"> - the DynAntenna handle to be used for inserting the FtAnte object into the _ante DB table.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_AO(ref short importOK, QualLine qualLine, int anteHandle)
        {
            ftAnte = State.Ante;
            ftAnteNullInds = State.AnteNullInds;

            int rc;

            if (State.AOflagSet /*   || !State.AKflagSet  || !State.AQflagSet */)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of ANTENNA record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): A: importOK = Constant.FAILURE");
            }
            else
            {
                State.AOflagSet = true;
            }

            //================================
            // Field #0 is the type/qualifier.
            //================================

            //====================================================
            // Field #1 is the Transmit Feed Line Horizontal Type.
            //====================================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 1, out ftAnte.txfdlnth, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx feed line H type field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx feed line H type field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.TXFDLNTH] = Constant.DB_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.TXFDLNTH] = Constant.DB_NOT_NULL;
            }

            //======================================================
            // Field #2 is the Transmit Feed Line Horizontal Length.
            //======================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 2, out ftAnte.txfdlnlh, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx feed line H length field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx feed line H length field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.TXFDLNLH] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna tx feed H length rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.TXFDLNLH] = Constant.DB_NOT_NULL;
            }

            //====================================================
            // Field #3 is the Transmit Feed Line Vertical Type.
            //====================================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 3, out ftAnte.txfdlntv, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx feed line V type field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx feed line V type field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.TXFDLNTV] = Constant.DB_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.TXFDLNTV] = Constant.DB_NOT_NULL;
            }

            //======================================================
            // Field #4 is the Transmit Feed Line Vertical Length.
            //======================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 4, out ftAnte.txfdlnlv, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx feed line V length field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx feed line V length field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.TXFDLNLV] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna tx feed V length rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.TXFDLNLV] = Constant.DB_NOT_NULL;
            }

            //======================================================
            // Field #5 is the Transmit Pad/Amplifier.
            //======================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 5, out ftAnte.txpadpam, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx PAD or power Amp. field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx PAD or power Amp. field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.TXPADPAM] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna tx PAD or power Amp rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.TXPADPAM] = Constant.DB_NOT_NULL;
            }

            //======================================================
            // Field #6 is the Transmit Component Loss.
            //======================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 6, out ftAnte.txcompl, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx component loss field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna tx component loss field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.TXCOMPL] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna tx component loss rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.TXCOMPL] = Constant.DB_NOT_NULL;
            }


            //====================================================
            // Field #7 is the Receive Feed Line Horizontal Type.
            //====================================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 7, out ftAnte.rxfdlnth, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx feed line H type field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx feed line H type field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.RXFDLNTH] = Constant.DB_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.RXFDLNTH] = Constant.DB_NOT_NULL;
            }

            //======================================================
            // Field #8 is the Receive Feed Line Horizontal Length.
            //======================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 8, out ftAnte.rxfdlnlh, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx feed line H length field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx feed line H length field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AQ(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.RXFDLNLH] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna rx feed H length rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.RXFDLNLH] = Constant.DB_NOT_NULL;
            }

            //====================================================
            // Field #9 is the Receive Feed Line Vertical Type.
            //====================================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 9, out ftAnte.rxfdlntv, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx feed line V type field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx feed line V type field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.RXFDLNTV] = Constant.DB_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.RXFDLNTV] = Constant.DB_NOT_NULL;
            }

            //======================================================
            // Field #10 is the Receive Feed Line Vertical Length.
            //======================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 10, out ftAnte.rxfdlnlv, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx feed line V length field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx feed line V length field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.RXFDLNLV] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna rx feed line V length rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.RXFDLNLV] = Constant.DB_NOT_NULL;
            }

            //======================================================
            // Field #11 is the Receive Pad/Amplifier.
            //======================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 11, out ftAnte.rxpadlna, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx PAD or LNA field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx PAD or LNA field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.RXPADLNA] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna rx PAD or LNA rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.RXPADLNA] = Constant.DB_NOT_NULL;
            }

            //======================================================
            // Field #12 is the Receive Component Loss.
            //======================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 12, out ftAnte.rxcompl, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx component loss field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna rx component loss field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.RXCOMPL] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Antenna rx component loss rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftAnteNullInds[FtAnte.RXCOMPL] = Constant.DB_NOT_NULL;
            }

            //===========================
            // Field #13 is Notes.
            //===========================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 13, out ftAnte.nota, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna notes field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna notes field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.NOTA] = Constant.DB_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.NOTA] = Constant.DB_NOT_NULL;
            }

            //===========================
            // Field #14 is Pointer.
            //===========================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 14, out ftAnte.apoint, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna pointer field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Antenna pointer field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.APOINT] = Constant.DB_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.APOINT] = Constant.DB_NOT_NULL;
            }

            //===========================
            // Field #15 is the License.
            //===========================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 15, out ftAnte.licence, 13)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, licence field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                // The following code is commented out to mimic the behaviour of a bug
                // or intentional deception (?!) in the C/C++ code
#if false
                msgBuf = String.Format("Error - line #{0}, licence field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nAnteRecord.Parse_AO(): C: importOK = Constant.FAILURE");
#endif
            }
            else if (rc == 0)
            {
                ftAnteNullInds[FtAnte.LICENCE] = Constant.DB_NULL;
            }
            else
            {
                ftAnteNullInds[FtAnte.LICENCE] = Constant.DB_NOT_NULL;
            }

            //=====================================================
            // Check if there are anomalous, additional CSV fields.
            //=====================================================
            string dummy;
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 16, out dummy, 1)) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, Antenna; extra fields ignored\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                //...Log2.v("\n\nTitleRecord.Parse_AO(): M-1: importOK = Constant.FAILURE");
            }


            return importOK;
        }


    }
}