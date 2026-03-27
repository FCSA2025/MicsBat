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
    /// This class provides methods that parse and validate 'change of location' (LK) and 
    /// 'change of call sign' (GK) records 
    /// in an ES import file, instantiate and populate an
    /// (FeCLoc or FeCCal) object with imported values and insert a record into a (_cloc or _ccal) table in the database.
    /// </summary>	
    public class ChangeRecord
    {
        private static string msgBuf;

        private static FeCLoc cLocStruct = new FeCLoc();
        private static SQLLEN[] cLocNulls = NullHelper.CreateArrayOfNullInd(FeCLoc.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

        private static FeCCal cCalStruct = new FeCCal();
        private static SQLLEN[] cCalNulls = NullHelper.CreateArrayOfNullInd(FeCCal.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

        /// <summary>
        /// This method parses a qualified line of type 'LK', validates its CSV fields,
        /// partially populates an FeCLoc object with the imported values, and then inserts
        /// a record into the _cloc database table associated with the destination name.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="tab5Name"> - full name of the DB table to be inserted into.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <param name="cLocHandle"> - a cursor handle from a previous SQL SELECT query to be used for the SQL INSERT.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_LK(ref short importOK, string tab5Name, QualLine qualLine, int cLocHandle)
        {
            //...Log2.v("\n\nChangeRecord.Parse_LK(): Entry: importOK = " + importOK);

            int rc;

            // Field #0 is the type/qualifier.

            // Field #1 is the Old Call Sign.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 1, out cLocStruct.oldlocation, FeCLoc.OLDLOCATION_SZ)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Change Location old location invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_LK(): A: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Change Location old location end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_LK(): B: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                cLocNulls[FeCLoc.OLDLOCATION] = Constant.DB_NULL;
            }
            else
            {
                cLocNulls[FeCLoc.OLDLOCATION] = Constant.DB_NOT_NULL;
            }

            // Field #2 is the New Location.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 2, out cLocStruct.newlocation, FeCLoc.NEWLOCATION_SZ)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Change Location new location invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_LK(): C: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Change Location new location end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_LK(): D: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                cLocNulls[FeCLoc.NEWLOCATION] = Constant.DB_NULL;
            }
            else
            {
                cLocNulls[FeCLoc.NEWLOCATION] = Constant.DB_NOT_NULL;
            }

            // Field #3 is the Station Name.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 3, out cLocStruct.name, 16)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Change Location name invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_LK(): E: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Change Location name location end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_LK(): F: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                cLocNulls[FeCLoc.NAME] = Constant.DB_NULL;
            }
            else
            {
                cLocNulls[FeCLoc.NAME] = Constant.DB_NOT_NULL;
            }

            if (FeRecExist.FeChngLocExist(tab5Name, cLocStruct))
            {
                /* record exists */
                msgBuf = String.Format("Error - line #{0}, duplicate Change of Location record\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_LK(): G: importOK = Constant.FAILURE");
            }
            else if (FeRecExist.LastReturnCode != Constant.NOT_FOUND)
            {
                /* unexpected error in Exist function */
                ErrMsg.UtPrintMessage(rc);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_LK(): H: importOK = Constant.FAILURE");
            }

            //...Log2.v("\n" + cLocStruct.ToStringWN(cLocNulls) + "\n");

            if (importOK == Constant.SUCCESS)
            {
                /* add to ingres file */
                if ((rc = DynFeCLoc.FeInsertCLoc(cLocHandle, cLocStruct, cLocNulls)) != Constant.SUCCESS)
                {
                    //...Log2.e("\nChangeRecord.Parse_LK(): ERROR: call to FeInsertCLoc() failed.");
                    ErrMsg.UtPrintMessage(rc);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nChangeRecord.Parse_LK(): I: importOK = Constant.FAILURE");
                }
                else
                {
                    //...Log2.v("\nChangeRecord.Parse_LK(): call to FeInsertCLoc() succeeded.");
                }
            }

            //...Log2.v("\n\nChangeRecord.Parse_LK(): Exit");
            return importOK;
        }

        /// <summary>
        /// This method parses a qualified line of type 'GK', validates its CSV fields,
        /// partially populates an FeCCal object with the imported values, and then inserts
        /// a record into the _ccal database table associated with the destination name.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="tab7Name"> - full name of the DB table to be inserted into.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <param name="cCalHandle"> - a cursor handle from a previous SQL SELECT query to be used for the SQL INSERT.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_GK(ref short importOK, string tab7Name, QualLine qualLine, int cCalHandle)
        {
            //...Log2.v("\n\nChangeRecord.Parse_GK(): Entry: importOK = " + importOK);

            int rc;

            // Field #0 is the type/qualifier.

            // Field #1 is Old Call Sign.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 1, out cCalStruct.oldcallsign, 9)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Change CallSign old callsign invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): A: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Change CallSign old callsign end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): B: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                cCalNulls[FeCCal.OLDCALLSIGN] = Constant.DB_NULL;
            }
            else
            {
                cCalNulls[FeCCal.OLDCALLSIGN] = Constant.DB_NOT_NULL;
            }

            // Field #2 is the New Call Sign.
            if ((rc = FeValidation.ParseFieldAsString(qualLine, 2, out cCalStruct.newcallsign, 9)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, Change CallSign new callsign invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): C: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, Change CallSign new callsign end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): D: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                cCalNulls[FeCCal.NEWCALLSIGN] = Constant.DB_NULL;
            }
            else
            {
                cCalNulls[FeCCal.NEWCALLSIGN] = Constant.DB_NOT_NULL;
            }

            if (FeRecExist.FeChngCallExist(tab7Name, cCalStruct))
            {
                /* record exists */
                msgBuf = String.Format("Error - line #{0}, duplicate Change of Call Sign record\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): E: importOK = Constant.FAILURE");
            }
            else if (FeRecExist.LastReturnCode != Constant.NOT_FOUND)
            {
                /* unexpected error in Exist function */
                ErrMsg.UtPrintMessage(rc);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nChangeRecord.Parse_GK(): F: importOK = Constant.FAILURE");
            }

            //...Log2.v("\n" + cCalStruct.ToStringWN(cCalNulls) + "\n");

            if (importOK == Constant.SUCCESS)
            {
                /* add to ingres file */
                if ((rc = DynFeCCal.FeInsertCCal(cCalHandle, cCalStruct, cCalNulls)) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(rc);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nChangeRecord.Parse_GK(): G: importOK = Constant.FAILURE");
                }
                else
                {
                    //...Log2.v("\nChangeRecord.Parse_GK(): call to FeInsertCCal() succeeded.");
                }
            }

            //...Log2.v("\n\nChangeRecord.Parse_GK(): Exit: importOK = " + importOK);
            return importOK;
        }









    }
}
