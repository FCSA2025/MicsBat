# Documented File: TitleRecord.cs
**Repository Path:** `FtImport\TitleRecord.cs`
**Primary Layer:** `FtImport`
**Namespace:** `FtImport`

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

namespace FtImport
{
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides a method that parses and validates a Title record 
    /// (TT) in a TS PDF import text file, instantiate and populate an
    /// FtTitl object with imported values and insert a record into a _titl table in the database.
    /// </summary>	
    public class TitleRecord
    {
        //private static short mTEflag = Constant.CLEAR;
        private static string msgBuf;
        private static FtTitl titlStruct = new FtTitl();
        private static SQLLEN[] titlNulls = NullHelper.CreateArrayOfNullInd(FtTitl.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

        /// <summary>
        /// This method parses and validates a Title record 
        /// (TT) in a TS PDF import text file, instantiate and populate an
        /// FtTitl object with imported values and insert a record into a _titl table in the database.
        /// </summary>
        /// <param name="importOK"> - unchanged on exit if no error occurs.</param>
        /// <param name="tableName_titl"></param>
        /// <param name="qualLine"></param>
        /// <param name="titlHandle"></param>
        /// <returns></returns>
        public static int Parse_TT(ref short importOK, string tableName_titl, QualLine qualLine, int titlHandle)
        {
            //...Log2.v("\n\nTitleRecord.Parse_TT(): Entry: importOK = " + importOK);

            int rc;

            State.TTflagSet = true;

            // It is assumed that number of CSV fields found versus required
            // has already been validated.

            //================================
            // Field #0 is the type/qualifier.
            //================================

            // Field #1 is the record validation code.
            // Override whatever the user provided in this field by setting
            // the validated flag to 'N'.
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 1, out titlStruct.validated, 1))
                == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Title validated flag, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TT(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Title validated flag, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TT(): C: importOK = Constant.FAILURE");
            }
            else
            {
                // Override whatever the user prescribed and set the validated flag to 'N'.
                titlStruct.validated = String.Format("{0}", Constant.NOT_VALIDATED);
                titlNulls[FtTitl.VALIDATED] = Constant.DB_NOT_NULL;
            }

            // Field #2 is the File Name.
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 2, out titlStruct.namef, 16)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Title filename, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TT(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Title filename, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TT(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                titlNulls[FtTitl.NAMEF] = Constant.DB_NULL;
            }
            else
            {
                titlNulls[FtTitl.NAMEF] = Constant.DB_NOT_NULL;
            }

            // Field #3 is the Source (operator code).
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 3, out titlStruct.source, 6))
                == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Title source name, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TT(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Title source name, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TT(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                titlNulls[FtTitl.SOURCE] = Constant.DB_NULL;

            }
            else
            {
                titlNulls[FtTitl.SOURCE] = Constant.DB_NOT_NULL;
            }

            // Field #4 is the Description.
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 4, out titlStruct.descr, 40))
                == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Title description, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TT(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Title description, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TT(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                titlNulls[FtTitl.DESCR] = Constant.DB_NULL;

            }
            else
            {
                titlNulls[FtTitl.DESCR] = Constant.DB_NOT_NULL;
            }

            // Field #5 is the Modify Date.
            if ((rc = FtParsing.ParseFieldAsDate(qualLine, 5, out titlStruct.mdate, 25)) < 0)
            {
                msgBuf = String.Format("Error - line #{0}, import Title  modify date, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = (short)rc;
                //...Log2.v("\n\nTitleRecord.Parse_TT(): H: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Title  modify date, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TT(): H-1: importOK = Constant.FAILURE");
            }
            else if (rc == 0)  // parsed string has zero length.
            {
                titlNulls[FtTitl.MDATE] = Constant.DB_NULL;
            }
            else
            {
                titlNulls[FtTitl.MDATE] = Constant.DB_NOT_NULL;

                // Check that the date is numerically valid.
                if (!ValUtil.UtValidateDate(titlStruct.mdate))
                {
                    msgBuf = String.Format("Error - line #{0}, import Title modify date, invalid date\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nTitleRecord.Parse_TT(): K: importOK = Constant.FAILURE");
                }
            }

            // Field #6 is the Modify Time.
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 6, out titlStruct.mtime, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Title modify time, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TT(): L: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Title modify time, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nTitleRecord.Parse_TT(): M: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                titlNulls[FtTitl.MTIME] = Constant.DB_NULL;

            }
            else
            {
                titlNulls[FtTitl.MTIME] = Constant.DB_NOT_NULL;
            }

            // Check if there are anomalous, additional CSV fields.
            string dummy;
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 7, out dummy, 1)) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, import Title extra fields ignored\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                //...Log2.v("\n\nTitleRecord.Parse_TT(): M-1: importOK = Constant.FAILURE");
            }

            // Write the title information as a record in the DC _titl table.
            // There must only ever be ONE title record so previous records are deleted.
            if (importOK == Constant.SUCCESS)
            {
                Ssutil.DbDeleteRows(tableName_titl, null);

                /* add to ingres file */
                if ((rc = DynTitle.FtInsertTitl(titlHandle, titlStruct, titlNulls))
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

            State.TTflagSet = false;

            //...Log2.v("\nTitleRecord.Parse_TT(): Exit: importOK = " + importOK);
            return importOK;
        }








    }
}

```
