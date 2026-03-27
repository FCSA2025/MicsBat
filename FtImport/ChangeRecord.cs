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
    /// This class provides methods that parse and validate 'change of call sign' (GK) records 
    /// in an TS import file, instantiate and populate a
    /// FtChng object with imported values and to insert a record into a _chng table in the database.
    /// </summary>	
    public class ChangeRecord
    {
        private static string msgBuf;

        private static FtChng ftChngCall = new FtChng();
        private static SQLLEN[] cCalNulls = NullHelper.CreateArrayOfNullInd(FtChng.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

        /// <summary>
        /// This method parses a qualified line of type 'GK', validates its CSV fields,
        /// partially populates an FtChng object with the imported values, and then inserts
        /// a record into the _ccal database table associated with the destination name.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="tabName"> - full name of the DB table to be inserted into.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <param name="cCalHandle"> - a cursor handle from a previous SQL SELECT query to be used for the SQL INSERT.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_GK(ref short importOK, string tabName, QualLine qualLine, int cCalHandle)
        {
            //...Log2.v("\n\nChangeRecord.Parse_GK(): Entry: importOK = " + importOK);

            State.GKflagSet = true;

            int rc;

            //================================
            // Field #0 is the type/qualifier.
            //================================

            //===========================
            // Field #1 is Old Call Sign.               MANDATORY,  Non-Nullable
            //===========================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 1, out ftChngCall.oldcall1, 9)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Change Call Sign old call1 field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): A: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Change Call Sign old call1 field, end of data\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): B: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                // Bug fix b191204A.
                // We have a zero-length string but the field is MANDATORY.
                msgBuf = String.Format("Error - line #{0}, Change Call Sign old call1 field MUST be present in order to import.\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): C: importOK = Constant.FAILURE");
            }
            else
            {
                cCalNulls[FtChng.OLDCALL1] = Constant.DB_NOT_NULL;
            }

            //===============================
            // Field #2 is the New Call Sign.           MANDATORY,  Non-Nullable
            //===============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 2, out ftChngCall.newcall1, 9)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Change Call Sign new call1 field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): C: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Change Call Sign new call1 field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): D: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                // Bug fix b191204A.
                // We have a zero-length string but the field is MANDATORY.
                msgBuf = String.Format("Error - line #{0}, Change Call Sign new call1 field MUST be present in order to import.\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): C: importOK = Constant.FAILURE");
            }
            else
            {
                cCalNulls[FtChng.NEWCALL1] = Constant.DB_NOT_NULL;
            }

            //==============================
            // Field #3 is the Station name.            MANDATORY
            //==============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 3, out ftChngCall.name, 32)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Change Call Sign name field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): A: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Change Call Sign name field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): B: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                // Bug fix b191204A.
                // We have a zero-length string but the field is MANDATORY.
                msgBuf = String.Format("Error - line #{0}, Change Call Sign station name field MUST be present in order to import.\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): C: importOK = Constant.FAILURE");
            }
            else
            {
                cCalNulls[FtChng.NAME] = Constant.DB_NOT_NULL;
            }

            // Check if there are anomalous, additional CSV fields.
            string dummy;
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 4, out dummy, 1)) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, Change Call Sign; extra fields ignored\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                //...Log2.v("\n\nTitleRecord.Parse_TT(): M-1: importOK = Constant.FAILURE");
            }

            // Handle the case of duplicate records in the table.
            if (FtRecExist.FtChngExist(tabName, ftChngCall))
            {
                /* record exists */
                int lineNumber = qualLine.LineNum;
                // The following line is needed to replicate C/C++ bug.
                lineNumber = lineNumber + 1;
                msgBuf = String.Format("Error - line #{0}, duplicate Change of Call Sign record", lineNumber);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): E: importOK = Constant.FAILURE");
            }
            else if (FtRecExist.LastReturnCode != Constant.NOT_FOUND)
            {
                /* unexpected error in Exist function */
                //ErrMsg.UtPrintMessage(rc);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): F: importOK = Constant.FAILURE");
            }

            //...Log2.v("\n" + ftChngCall.ToStringWN(cCalNulls) + "\n");

            // The following nullInd reassignments are required to exactly mimic
            // the behaviour of the legacy C/C+ code.
            cCalNulls[FtChng.OLDCALL1] = Constant.DB_NOT_NULL;
            cCalNulls[FtChng.NEWCALL1] = Constant.DB_NOT_NULL;
            cCalNulls[FtChng.NAME] = Constant.DB_NOT_NULL;

            if (importOK == Constant.SUCCESS)
            {
                if ((rc = DynChange.FtInsertChngCall(cCalHandle, ftChngCall, cCalNulls)) != Constant.SUCCESS)
                {
                    //ErrMsg.UtPrintMessage(rc);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nChangeRecord.Parse_GK(): G: importOK = Constant.FAILURE");
                }
                else
                {
                    //...Log2.v("\nChangeRecord.Parse_GK(): call to FtInsertChngCall() succeeded.");
                }
            }

            State.GKflagSet = false;

            //...Log2.v("\n\nChangeRecord.Parse_GK(): Exit: importOK = " + importOK);
            return importOK;
        }









    }
}
