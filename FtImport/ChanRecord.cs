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
    /// This class provides methods that parse and validate Channel records 
    /// (CK, CT, CR, CQ, CO) in an TS import file, instantiate and populate an
    /// FtChan object with imported values and to insert a record into an _chan table in the database.
    /// </summary>	
    public class ChanRecord
    {
        private static string msgBuf;

        private static FtChan ftChan;
        private static SQLLEN[] ftChanNullInds;

        /// <summary>
        /// This method parses a qualified line of type 'CK', validates its CSV fields and then
        /// partially populates an FtChan object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_CK(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nChanRecord.Parse_CK(): Entry: importOK = " + importOK);

            ftChan = State.Chan;
            ftChanNullInds = State.ChanNullInds;

            int rc;

            if (State.CKflagSet)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of CHANNEL record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Error.TOOFEWCSVFIELDS;
                //...Log2.v("\n\nChanRecord.Parse_CK(): A: importOK = Constant.FAILURE");
            }
            else
            {
                State.CKflagSet = true;
            }

            //================================
            // Field #0 is the type/qualifier.
            //================================

            //==========================================
            // Field # 1 is the MDB Operation (command).            MANDATORY
            //==========================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 1, out ftChan.cmd, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel cmd field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel cmd field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.CMD] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.CMD] = Constant.DB_NOT_NULL;
            }

            //================================
            // Field # 2 is the Record Status. 
            //================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 2, out ftChan.recstat, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel recstat field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel recstat field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.RECSTAT] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.RECSTAT] = Constant.DB_NOT_NULL;
            }

            //==========================================
            // Field # 3 is the Local Call Sign (call1).                    MANDATORY,  Non-Nullable
            //==========================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 3, out ftChan.call1, 9)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel call local field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel call local field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                msgBuf = String.Format("Error - line #{0}, call1 field MUST be present in order to import.\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                ftChanNullInds[FtChan.CALL1] = Constant.DB_NOT_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.CALL1] = Constant.DB_NOT_NULL;
            }

            //===========================================
            // Field # 4 is the Remote Call Sign (call2).                   MANDATORY,  Non-Nullable
            //===========================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 4, out ftChan.call2, 9)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel call remote field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): H: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel call remote field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): I: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                msgBuf = String.Format("Error - line #{0}, call2 field MUST be present in order to import.\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                ftChanNullInds[FtChan.CALL2] = Constant.DB_NOT_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.CALL2] = Constant.DB_NOT_NULL;
            }

            //===========================
            // Field #5 is the Band Code.                           MANDATORY,  Non-Nullable
            //===========================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 5, out ftChan.bndcde, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel band code field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): J: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel band code field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): K: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                msgBuf = String.Format("Error - line #{0}, band code MUST be present in order to import.\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                ftChanNullInds[FtChan.BNDCDE] = Constant.DB_NOT_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.BNDCDE] = Constant.DB_NOT_NULL;
            }

            //============================
            // Field #6 is the Channel ID.                               MANDATORY,  Non-Nullable
            //============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 6, out ftChan.chid, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel channel id field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): L: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel channel id field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): M: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                msgBuf = String.Format("Error - line #{0}, chid field MUST be present in order to import.\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                ftChanNullInds[FtChan.CHID] = Constant.DB_NOT_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.CHID] = Constant.DB_NOT_NULL;
            }

            //=============================
            // Field #7 is the Modify Date.
            //=============================
            if ((rc = FtParsing.ParseFieldAsDate(qualLine, 7, out ftChan.mdate, 25)) < 0)
            {
                msgBuf = String.Format("Error - line #{0}, Channel modify date field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = (short)rc;
                //...Log2.v("\n\nChanRecord.Parse_CK(): N: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel modify date field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.MDATE] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.MDATE] = Constant.DB_NOT_NULL;

#if true
                // Check that the date is numerically valid.
                if (!ValUtil.UtValidateDate(chanStruct.mdate))
                {
                    msgBuf = String.Format("Error - line #{0}, Invalid date field; the day, month or year is out of range.\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Error.BADDATE;
                    //...Log2.v("\n\nChanRecord.Parse_CK(): P: importOK = Constant.FAILURE");
                }
#endif
            }

            //=============================
            // Field #8 is the Modify Time.
            //=============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 8, out ftChan.mtime, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel modify time field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel modify time field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): R: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.MTIME] = Constant.DB_NULL;
            }
#if false   // Excluded to mimic the behaviour of the legacy C/C++ code.
            else if (!FtValidation.IsValidTime(chanStruct.mtime))
            {
                msgBuf = String.Format("Error - line #{0}, Invalid time field; format should be hh:mm\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Error.BADTIME;
                //...Log2.v("\n\nChanRecord.Parse_CK(): R-1: importOK = Constant.FAILURE");
            }
#endif
            else
            {
                ftChanNullInds[FtChan.MTIME] = Constant.DB_NOT_NULL;
            }

            //=====================================================
            // Check if there are anomalous, additional CSV fields.
            //=====================================================
            string dummy;
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 9, out dummy, 1)) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, Channel; extra fields ignored\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                //...Log2.v("\n\nTitleRecord.Parse_AO(): M-1: importOK = Constant.FAILURE");
            }

            //...Log2.v("\nChanRecord.Parse_CK(): Exit: importOK = " + importOK);
            return importOK;
        }

        /// <summary>
        /// This method parses a qualified line of type 'CT', validates its CSV fields and then
        /// partially populates an FtChan object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_CT(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nChanRecord.Parse_CT(): Entry: importOK = " + importOK);

            ftChan = State.Chan;
            ftChanNullInds = State.ChanNullInds;

            int rc;

            if (State.CTflagSet)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of CHANNEL record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): A: importOK = Constant.FAILURE");
            }
            else
            {
                State.CTflagSet = true;
            }

            //================================
            // Field #0 is the type/qualifier.
            //================================

            //================================
            // Field #1 is the Frequency Plan.
            //================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 1, out ftChan.splan, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel plan field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel plan field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.SPLAN] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.SPLAN] = Constant.DB_NOT_NULL;
            }

            //====================================
            // Field #2 is HiLo.
            //====================================
            if ((rc = FtParsing.ParseFieldAsShort(qualLine, 2, out ftChan.hl)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel hl field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel hl field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.HL] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.HL] = Constant.DB_NOT_NULL;
            }

            //========================================
            // Field #3 is the Polarization (VH code).
            //========================================
            if ((rc = FtParsing.ParseFieldAsShort(qualLine, 3, out ftChan.vh)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel vh field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel vh field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.VH] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.VH] = Constant.DB_NOT_NULL;
            }

            //====================================
            // Field #4 is the Transmit Frequency.
            //====================================
            if ((rc = FtParsing.ParseFieldAsDoubleRound(qualLine, 4, out ftChan.freqtx, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx frequency field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx frequency field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.FREQTX] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Channel tx frequency rounded to 2 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftChanNullInds[FtChan.FREQTX] = Constant.DB_NOT_NULL;
            }

            //==============================
            // Field #5 is the Polarization.
            //==============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 5, out ftChan.poltx, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx polarity field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx polarity field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.POLTX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.POLTX] = Constant.DB_NOT_NULL;
            }

            //======================================
            // Field #6 is the Antenna Number Mn TX.
            //======================================
            if ((rc = FtParsing.ParseFieldAsShort(qualLine, 6, out ftChan.antnumbtx1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel primary tx antenna num. field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel primary tx antenna num. field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.ANTNUMBTX1] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.ANTNUMBTX1] = Constant.DB_NOT_NULL;
            }

            //=================================================
            // Field #7 is the Antenna Feeder System Loss TX 1.
            //=================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 7, out ftChan.afsltx1, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel primary tx antenna feed system loss field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel primary tx antenna feed system loss field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.AFSLTX1] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Channel primary tx feed system loss rounded to 1 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftChanNullInds[FtChan.AFSLTX1] = Constant.DB_NOT_NULL;
            }


            //======================================
            // Field #8 is the Antenna Number TX2.
            //======================================
            if ((rc = FtParsing.ParseFieldAsShort(qualLine, 8, out ftChan.antnumbtx2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel primary DV1 antenna num. field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel primary DV1 antenna num. field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.ANTNUMBTX2] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.ANTNUMBTX2] = Constant.DB_NOT_NULL;
            }

            //=================================================
            // Field #9 is the Antenna Feeder System Loss TX 2.
            //=================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 9, out ftChan.afsltx2, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel Secondary tx antenna feed system loss field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel Secondary tx antenna feed system loss field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.AFSLTX2] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Channel Secondary antenna F/S Loss rounded to 1 dp.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftChanNullInds[FtChan.AFSLTX2] = Constant.DB_NOT_NULL;
            }

            //=================================
            // Field #10 is the Equipment Code.
            //=================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 10, out ftChan.eqpttx, 8)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx equipment field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx equipment field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.EQPTTX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.EQPTTX] = Constant.DB_NOT_NULL;
            }

            //=================================
            // Field #11 is the Equipment Use.
            //=================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 11, out ftChan.eqptutx, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx equipment use field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx equipment use field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.EQPTUTX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.EQPTUTX] = Constant.DB_NOT_NULL;
            }

            //====================================
            // Field #12 is the Coordinated Power.
            //====================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 12, out ftChan.pwrtx, 1)) == Constant.UT_INV_CONV)
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
                ftChanNullInds[FtChan.PWRTX] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Channel tx power rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftChanNullInds[FtChan.PWRTX] = Constant.DB_NOT_NULL;
            }

            //=============================
            // Field #13 is the ATPC Range. 
            //=============================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 13, out ftChan.atpccde, 1)) == Constant.UT_INV_CONV)
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
                ftChanNullInds[FtChan.ATPCCDE] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Channel atpc code rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftChanNullInds[FtChan.ATPCCDE] = Constant.DB_NOT_NULL;
            }

            //===============================
            // Field #14 is the Traffic Code. 
            //===============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 14, out ftChan.traftx, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx traffic field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): N: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx traffic field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): O: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.TRAFTX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.TRAFTX] = Constant.DB_NOT_NULL;
            }

            // Field #15 is the Service Code.
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 15, out ftChan.srvctx, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx service code field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): R: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx service code field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): S: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.SRVCTX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.SRVCTX] = Constant.DB_NOT_NULL;
            }

            //==============================
            // Field #16 is the Status Code.
            //==============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 16, out ftChan.stattx, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx status field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): P: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx status field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.STATTX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.STATTX] = Constant.DB_NOT_NULL;
            }

            //===========================
            // Field #17 is the Fee Code.
            //===========================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 17, out ftChan.feetx, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx fee code field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): T: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx fee code field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): U: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.FEETX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.FEETX] = Constant.DB_NOT_NULL;
            }

            //=====================================================
            // Check if there are anomalous, additional CSV fields.
            //=====================================================
            string dummy;
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 18, out dummy, 1)) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, Channel; extra fields ignored\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                //...Log2.v("\n\nTitleRecord.Parse_CT(): M-1: importOK = Constant.FAILURE");
            }

            //...Log2.v("\nChanRecord.Parse_CT(): Exit: importOK = " + importOK);
            return importOK;
        }

        /// <summary>
        /// This method parses a qualified line of type 'CR', validates its CSV fields and then
        /// partially populates an FtChan object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_CR(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nChanRecord.Parse_CR(): Entry: importOK = " + importOK);

            ftChan = State.Chan;
            ftChanNullInds = State.ChanNullInds;

            int rc;

            if (State.CRflagSet)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of CHANNEL record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): A: importOK = Constant.FAILURE");
            }
            else
            {
                State.CRflagSet = true;
            }

            //================================
            // Field #0 is the type/qualifier.
            //================================

            //====================================
            // Field #1 is the Receive Frequency.
            //====================================
            if ((rc = FtParsing.ParseFieldAsDoubleRound(qualLine, 1, out ftChan.freqrx, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx frequency field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx frequency field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.FREQRX] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Channel rx frequency rounded to 2 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftChanNullInds[FtChan.FREQRX] = Constant.DB_NOT_NULL;
            }

            //==============================
            // Field #2 is the Polarization.
            //==============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 2, out ftChan.polrx, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx polarity field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx polarity field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.POLRX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.POLRX] = Constant.DB_NOT_NULL;
            }

            //======================================
            // Field #3 is the Antenna Number Mn.
            //======================================
            if ((rc = FtParsing.ParseFieldAsShort(qualLine, 3, out ftChan.antnumbrx1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel primary rx antenna number field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel primary rx antenna number field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.ANTNUMBRX1] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.ANTNUMBRX1] = Constant.DB_NOT_NULL;
            }

            //======================================
            // Field #4 is the Antenna Number Dv1.
            //======================================
            if ((rc = FtParsing.ParseFieldAsShort(qualLine, 4, out ftChan.antnumbrx2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel primary DV1 rx antenna number field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel primary DV1 rx antenna number field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.ANTNUMBRX2] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.ANTNUMBRX2] = Constant.DB_NOT_NULL;
            }

            //======================================
            // Field #5 is the Antenna Number Dv2.
            //======================================
            if ((rc = FtParsing.ParseFieldAsShort(qualLine, 5, out ftChan.antnumbrx3)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel DV2 rx antenna number field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel DV2 rx antenna number field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.ANTNUMBRX3] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.ANTNUMBRX3] = Constant.DB_NOT_NULL;
            }

            //=================================
            // Field #6 is the Equipment Code.
            //=================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 6, out ftChan.eqptrx, 8)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx equipment field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx equipment field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.EQPTRX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.EQPTRX] = Constant.DB_NOT_NULL;
            }

            //=================================
            // Field #7 is the Equipment Use.
            //=================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 7, out ftChan.eqpturx, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx equipment use field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx equipment use field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.EQPTURX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.EQPTURX] = Constant.DB_NOT_NULL;
            }

            //===============================
            // Field #8 is the Traffic Code. 
            //===============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 8, out ftChan.trafrx, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx traffic code field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): N: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx traffic code field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): O: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.TRAFRX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.TRAFRX] = Constant.DB_NOT_NULL;
            }

            // Field #9 is the Service Code.
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 9, out ftChan.srvcrx, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx service field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): R: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx service field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): S: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.SRVCRX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.SRVCRX] = Constant.DB_NOT_NULL;
            }

            //==============================
            // Field #10 is the Status Code.
            //==============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 10, out ftChan.statrx, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx status field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): P: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx status field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.STATRX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.STATRX] = Constant.DB_NOT_NULL;
            }

            //===========================
            // Field #11 is the Fee Code.
            //===========================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 11, out ftChan.feerx, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx fee code field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): T: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx fee code field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): U: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.FEERX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.FEERX] = Constant.DB_NOT_NULL;
            }

            //=====================================================
            // Check if there are anomalous, additional CSV fields.
            //=====================================================
            string dummy;
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 12, out dummy, 1)) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, Channel; extra fields ignored\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                //...Log2.v("\n\nTitleRecord.Parse_CT(): M-1: importOK = Constant.FAILURE");
            }

            //...Log2.v("\nChanRecord.Parse_CR(): Exit: importOK = " + importOK);
            return importOK;
        }

        /// <summary>
        /// This method parses a qualified line of type 'CQ', validates its CSV fields and then
        /// partially populates an FtChan object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_CQ(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nChanRecord.Parse_CR(): Entry: importOK = " + importOK);

            ftChan = State.Chan;
            ftChanNullInds = State.ChanNullInds;

            int rc;

            if (State.CQflagSet)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of CHANNEL record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CR(): A: importOK = Constant.FAILURE");
            }
            else
            {
                State.CQflagSet = true;
            }

            //================================
            // Field #0 is the type/qualifier.
            //================================

            //======================================================
            // Field #1 is the Receive Antenna Feeder System Loss Mn.
            //======================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 1, out ftChan.afslrx1, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel primary antenna feed system loss field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel primary antenna feed system loss field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.AFSLRX1] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Channel primary antenna feed system loss rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftChanNullInds[FtChan.AFSLRX1] = Constant.DB_NOT_NULL;
            }

            //======================================================
            // Field #2 is the Receive Power Mn.
            //======================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 2, out ftChan.pwrrx1, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel primary rx power field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel primary rx power field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.PWRRX1] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Channel primary rx power rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftChanNullInds[FtChan.PWRRX1] = Constant.DB_NOT_NULL;
            }


            //========================================================
            // Field #3 is the Receive Antenna Feeder System Loss Dv1.
            //========================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 3, out ftChan.afslrx2, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel DV1 rx antenna feed system loss field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel DV1 rx antenna feed system loss field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.AFSLRX2] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Channel DV1 rx antenna F/S Loss rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftChanNullInds[FtChan.AFSLRX2] = Constant.DB_NOT_NULL;
            }

            //======================================================
            // Field #4 is the Receive Power Dv1.
            //======================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 4, out ftChan.pwrrx2, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel DV1 rx power field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel DV1 rx power field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.PWRRX2] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Channel DV1 rx power rounded to 1 d.p\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftChanNullInds[FtChan.PWRRX2] = Constant.DB_NOT_NULL;
            }

            //========================================================
            // Field #5 is the Receive Antenna Feeder System Loss Dv2.
            //========================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 5, out ftChan.afslrx3, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel DV2 rx antenna feed system loss field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel DV2 rx antenna feed system loss field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.AFSLRX3] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Channel DV2 rx antenna F/S Loss rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftChanNullInds[FtChan.AFSLRX3] = Constant.DB_NOT_NULL;
            }

            //======================================================
            // Field #6 is the Receive Power Dv2.
            //======================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 6, out ftChan.pwrrx3, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel DV2 rx power field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel DV2 rx power field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.PWRRX3] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Channel DV2 rx power rounded to 1 d.p\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftChanNullInds[FtChan.PWRRX3] = Constant.DB_NOT_NULL;
            }

            //======================================================
            // Field #7 is the Cumulative Interference ES.
            //======================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 7, out ftChan.esint, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel es total interference field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel es total interference field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.ESINT] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Channel ES total interference rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftChanNullInds[FtChan.ESINT] = Constant.DB_NOT_NULL;
            }

            //======================================================
            // Field #8 is the Cumulative Interference TS.
            //======================================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 8, out ftChan.tsint, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel ts total interference field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel ts total interference field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.TSINT] = Constant.DB_NULL;
            }
            else
            {
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, Channel TS total interference rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
                ftChanNullInds[FtChan.TSINT] = Constant.DB_NOT_NULL;
            }

            //=====================================================
            // Check if there are anomalous, additional CSV fields.
            //=====================================================
            string dummy;
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 9, out dummy, 1)) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, Channel; extra fields ignored\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                //...Log2.v("\n\nTitleRecord.Parse_AO(): M-1: importOK = Constant.FAILURE");
            }

            return importOK;
        }

        /// <summary>
        /// This method parses a qualified line of type 'CO', validates its CSV fields and then
        /// partially populates an FtChan object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_CO(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nChanRecord.Parse_CK(): Entry: importOK = " + importOK);

            ftChan = State.Chan;
            ftChanNullInds = State.ChanNullInds;

            int rc;

            if (State.COflagSet)
            {
                msgBuf = String.Format("Error - line #{0}, improper order of CHANNEL record types\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Error.TOOFEWCSVFIELDS;
                //...Log2.v("\n\nChanRecord.Parse_CK(): A: importOK = Constant.FAILURE");
            }
            else
            {
                State.COflagSet = true;
            }

            //================================
            // Field #0 is the type/qualifier.
            //================================

            //==========================================
            // Field # 1 is the Route Code.           
            //==========================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 1, out ftChan.routnumb, 8)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel route number field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel route number field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CK(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.ROUTNUMB] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.ROUTNUMB] = Constant.DB_NOT_NULL;
            }

            //====================================
            // Field #2 is Station Number.
            //====================================
            if ((rc = FtParsing.ParseFieldAsShort(qualLine, 2, out ftChan.stnnumb)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel station number field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel station number field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.STNNUMB] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.STNNUMB] = Constant.DB_NOT_NULL;
            }

            //====================================
            // Field #3 is Hop Number.
            //====================================
            if ((rc = FtParsing.ParseFieldAsShort(qualLine, 3, out ftChan.hopnumb)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel hop number field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel hop number field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.HOPNUMB] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.HOPNUMB] = Constant.DB_NOT_NULL;
            }

            //================================
            // Field #4 is the Transmit Notes.
            //================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 4, out ftChan.notetx, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx note field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel tx note field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.NOTETX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.NOTETX] = Constant.DB_NOT_NULL;
            }

            //================================
            // Field #5 is the Receive Notes.
            //================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 5, out ftChan.noterx, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx note field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel rx note field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.NOTERX] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.NOTERX] = Constant.DB_NOT_NULL;
            }

            //================================
            // Field #6 is the General Notes.
            //================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 6, out ftChan.notegnl, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel general note pointer field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel general note pointer field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.NOTEGNL] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.NOTEGNL] = Constant.DB_NOT_NULL;
            }

            //================================
            // Field #7 is the Pointer.
            //================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 7, out ftChan.cpoint, 5)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel pointer field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel pointer field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.CPOINT] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.CPOINT] = Constant.DB_NOT_NULL;
            }

            //================================
            // Field #8 is the Service Date.
            //================================
            if ((rc = FtParsing.ParseFieldAsDate(qualLine, 8, out ftChan.sdate, 25)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Channel service date field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Channel service date field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChanRecord.Parse_CT(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                ftChanNullInds[FtChan.SDATE] = Constant.DB_NULL;
            }
            else
            {
                ftChanNullInds[FtChan.SDATE] = Constant.DB_NOT_NULL;
            }

            //=====================================================
            // Check if there are anomalous, additional CSV fields.
            //=====================================================
            string dummy;
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 9, out dummy, 1)) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, Channel; extra fields ignored\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                //...Log2.v("\n\nTitleRecord.Parse_AO(): M-1: importOK = Constant.FAILURE");
            }

            return importOK;
        }






    }
}
