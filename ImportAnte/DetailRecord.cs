using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportAnte
{
    using SQLLEN = Int64;
    public class DetailRecord
    {
        public static void ParseLine(int lineNum, string[] fields, out SuAntd antdStruct, out SQLLEN[] antdNulls, ref short importOK)
        {
            // 'out' requirement.
            antdStruct = new SuAntd();
            antdNulls = NullHelper.CreateArrayOfNullInd(SuAntd.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            string msgBuf;
            int rc;

            if ((rc = CSV.ParseFieldAsString(fields, 1, out antdStruct.cmd, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd cmd field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd cmd field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                antdNulls[SuAntd.CMD] = Constant.DB_NULL;
            }
            else
            {
                antdNulls[SuAntd.CMD] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsString(fields, 2, out antdStruct.acode, 12)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd acode field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd acode field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                msgBuf = String.Format("Error - line #{0}, discrimination acode field, MUST be present for import\n",
                                lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                antdNulls[SuAntd.ACODE] = Constant.DB_NOT_NULL;
            }
            else
            {
                antdNulls[SuAntd.ACODE] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsFloatRound(fields, 3, out antdStruct.antang, -1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd antang field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd antang field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                msgBuf = String.Format("Error - line #{0}, Antd offaxis angle, MUST be present for import\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                antdNulls[SuAntd.ANTANG] = Constant.DB_NULL;
            }
            else
            {
                antdNulls[SuAntd.ANTANG] = Constant.DB_NOT_NULL;
            }

            // ===========================================================================
            // In the legacy C/C+ code the parse order is:  dcoh -> dxph -> dcov -> dxpv.
            // This is incorrect; the correct order i.a.w. FCSA specification is the same
            // as the order of columns in a su_XXX_antd table (and members in SuAntd);
            // Consequently, the code below uses the correct parse order:
            // dcov -> dxpv -> dcoh -> dxph
            //============================================================================

            if ((rc = CSV.ParseFieldAsFloatRound(fields, 4, out antdStruct.dcov, -1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd dcov field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd dcov field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                antdNulls[SuAntd.DCOV] = Constant.DB_NULL;
            }
            else
            {
                antdNulls[SuAntd.DCOV] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsFloatRound(fields, 5, out antdStruct.dxpv, -1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd dxpv field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd dxpv field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                antdNulls[SuAntd.DXPV] = Constant.DB_NULL;
            }
            else
            {
                antdNulls[SuAntd.DXPV] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsFloatRound(fields, 6, out antdStruct.dcoh, -1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd dcoh field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd dcoh field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                antdNulls[SuAntd.DCOH] = Constant.DB_NULL;
            }
            else
            {
                antdNulls[SuAntd.DCOH] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsFloatRound(fields, 7, out antdStruct.dxph, -1))
                == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd dxph field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd dxph field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                antdNulls[SuAntd.DXPH] = Constant.DB_NULL;
            }
            else
            {
                antdNulls[SuAntd.DXPH] = Constant.DB_NOT_NULL;
            }

            /*
             * TASK 499: Processing for dtilt field added for
             * this task, for PCS Antenna tilt calculation.
             * Look for the dtilt field ONLY if the number
             * of fields, as indicated by the number of comma
             * delimiters, is more than SuAntd.MTIME.
             */
            int numFields = fields.Length;
            int index = 8;
            if (numFields > (SuAntd.MTIME + 1))
            {
                if ((rc = CSV.ParseFieldAsFloatRound(fields, index++, out antdStruct.dtilt, -1)) == Constant.UT_INV_CONV)
                {
                    msgBuf = String.Format("Error - line #{0}, import Antd dtilt field, invalid conversion\n", lineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Constant.FAILURE;
                }
                else if (rc == Constant.UT_EOLN)
                {
                    msgBuf = String.Format("Error - line #{0}, import Antd dtilt field, end of data found\n", lineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Constant.FAILURE;
                }
                else if (rc == 0)
                {
                    antdNulls[SuAntd.DTILT] = Constant.DB_NULL;
                }
                else
                {
                    antdNulls[SuAntd.DTILT] = Constant.DB_NOT_NULL;
                }
            }
            else
            {
                antdNulls[SuAntd.DTILT] = Constant.DB_NULL;
            }

//...Log2.v("\nDetailRecord.ParseLine(): ZULU: index = " + index);
//...Log2.v("\nDetailRecord.ParseLine(): ZULU: field = " + fields[index]);
            if ((rc = CSV.ParseFieldAsInt(fields, index++, out antdStruct.interpstat)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd interpstat field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd interpstat field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                // The CSV field was empty.
                antdNulls[SuAntd.INTERPSTAT] = Constant.DB_NULL;
            }
            else
            {
                antdNulls[SuAntd.INTERPSTAT] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsDate(fields, index++, out antdStruct.mdate, 25)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd mdate field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd mdate field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                antdNulls[SuAntd.MDATE] = Constant.DB_NULL;
            }
            else
            {
                antdNulls[SuAntd.MDATE] = Constant.DB_NOT_NULL;
                bool ok = ValUtil.UtValidateDate(antdStruct.mdate);
                //if (rc == Constant.FAILED_FORMAT || rc == Constant.FAILED_RANGE)
                if (!ok)
                {
                    msgBuf = String.Format("Error - line #{0}, importAntd mdate field, invalid date\n", lineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Constant.FAILURE;
                }
            }

            if ((rc = CSV.ParseFieldAsString(fields, index++, out antdStruct.mtime, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd mtime field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Antd mtime field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                antdNulls[SuAntd.MTIME] = Constant.DB_NULL;
            }
            else
            {
                antdNulls[SuAntd.MTIME] = Constant.DB_NOT_NULL;
            }

            string dummy;
            if (CSV.ParseFieldAsString(fields, index++, out dummy, 1) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, import Ante detail; extra fields ignored\r\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
            }

        }







    }
}
