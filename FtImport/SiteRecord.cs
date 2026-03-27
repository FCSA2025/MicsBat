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
    /// This class provides methods that parse and validate Site records 
    /// (SK, SD) in an TS import file, instantiate and populate an
    /// FtSite object with imported values and to insert a record into an _site table in the database.
    /// </summary>	
    public class SiteRecord
    {
        private static string msgBuf;

        /// <summary>
        /// This method parses a qualified line of type 'SK', validates its CSV fields and then
        /// partially populates an FtSite object with the imported values.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_SK(ref short importOK, QualLine qualLine)
        {
            //...Log2.v("\n\nSiteRecord.Parse_SK(): Entry: importOK = " + importOK);

            int rc;
            int nRet;
            string strLatit;
            string strLongit;
            string strOrientation;

            FtSite ftSite = State.Site;
            SQLLEN[] siteNulls = State.SiteNullInds;

            if (State.SKflagSet)
            {
                msgBuf = String.Format("Error - improper order of SITE record types\r\nCheck that there is a SK and SD record for each site.\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else
            {
                State.SKflagSet = true;
            }

            //================================
            // Field #0 is the type/qualifier.
            //================================

            //=========================================
            // Field #1 is the MDB Operation (command).         MANDATORY
            //=========================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 1, out ftSite.cmd, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site cmd field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): B: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site cmd field, end of data found\r\n\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): C: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.CMD] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FtSite.CMD] = Constant.DB_NOT_NULL;
            }

            //=========================================
            // Field #2 is the Record Status.
            //=========================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 2, out ftSite.recstat, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site recstat field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site recstat field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.RECSTAT] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FtSite.RECSTAT] = Constant.DB_NOT_NULL;
            }

            //=========================================
            // Field #3 is the Local Call Sign (call1).         MANDATORY,  Non-Nullable
            //=========================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 3, out ftSite.call1, 9)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site call1 field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site call1 field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                msgBuf = String.Format("Error - line #{0}, call1 field MUST be present.\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                siteNulls[FtSite.CALL1] = Constant.DB_NOT_NULL;
            }
            else
            {

                siteNulls[FtSite.CALL1] = Constant.DB_NOT_NULL;
            }

            //=========================================
            // Field #4 is the Site Name.
            //=========================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 4, out ftSite.name, 32)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site name field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): H: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site name field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): I: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.NAME] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FtSite.NAME] = Constant.DB_NOT_NULL;
            }

            //=========================================
            // Field #5 is the Latitude + Sense.
            //=========================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 5, out strLatit, 15)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site latitude field, too long\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): A: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site latitude field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): B: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.LATIT] = Constant.DB_NULL;
            }
            else
            {
                /* convert strLatit from 00-00-00.00N to
                    hundreths of seconds */
                strOrientation = Strings.LastChar(strLatit).ToString();

                strLatit = strLatit.Substring(0, strLatit.Length - 1);

                //...Log2.v("\n\nSiteRecord.Parse_SD(): strLatit = " + strLatit);

                nRet = GenUtil.UtStrConvLongLat(Constant.LATITUDE, strLatit, strOrientation, out ftSite.latit);

                if (nRet != Constant.SUCCESS)
                {
                    msgBuf = String.Format("Error - line #{0}, import Site latitude field, invalid conversion\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nSiteRecord.Parse_SD(): C: importOK = Constant.FAILURE");
                }
                else
                {
                    siteNulls[FtSite.LATIT] = Constant.DB_NOT_NULL;
                }
            }

            //=========================================
            // Field #6 is Longitude + Sense.
            //=========================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 6, out strLongit, 15)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site longitude field, too long\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): D: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site longitude field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): E: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.LONGIT] = Constant.DB_NULL;
            }
            else
            {
                /* convert strLongit from 00-00-00.00E to
                    hundreths of seconds */
                strOrientation = Strings.LastChar(strLongit).ToString();

                strLongit = strLongit.Substring(0, strLongit.Length - 1);

                nRet = GenUtil.UtStrConvLongLat(Constant.LONGITUDE, strLongit, strOrientation, out ftSite.longit);
                if (nRet != Constant.SUCCESS)
                {
                    msgBuf = String.Format("Error - line #{0}, import Site longitude field, invalid conversion\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nSiteRecord.Parse_SD(): F: importOK = Constant.FAILURE");
                }
                else
                {
                    siteNulls[FtSite.LONGIT] = Constant.DB_NOT_NULL;
                }
            }

            //===========================================
            // Field #7 is the Ground Height of the site.
            //===========================================
            if ((rc = FtParsing.ParseFieldAsFloatRound(qualLine, 7, out ftSite.grnd, 1)) == Constant.UT_INV_CONV)
            {
                // The following lines are commented out so as to mimic
                // the bug in the legacy C/C++ code.

                msgBuf = String.Format("Error - line #{0}, import Site ground height field, invalid conversion\r\n",
                                qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;

                //...Log2.v("\n\nSiteRecord.Parse_SD(): G: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site ground height field, end of data found\r\n",
                                qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): H: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.GRND] = Constant.DB_NULL;
            }
            else
            {
                siteNulls[FtSite.GRND] = Constant.DB_NOT_NULL;
                if (rc == -3)
                {
                    msgBuf = String.Format("*Note - line #{0}, ground height was rounded to 1 d.p.\r\n",
                                    qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
            }

            //===========================================
            // Field #8 is the Modify Date.
            //===========================================
            if ((rc = FtParsing.ParseFieldAsDate(qualLine, 8, out ftSite.mdate, 25)) < 0)
            {
                msgBuf = String.Format("Error - line #{0}, import Site modify date field, invalid date\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = (short)rc;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): P: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #%d, import Site modify date field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.MDATE] = Constant.DB_NULL;
            }
            else
            {
                siteNulls[FtSite.MDATE] = Constant.DB_NOT_NULL;

                // Check that the date is numerically valid.
                if (!ValUtil.UtValidateDate(ftSite.mdate))
                {
                    msgBuf = String.Format("Error - line #{0}, import Site modify date field, invalid date\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nSiteRecord.Parse_SK(): R: importOK = Constant.FAILURE");
                }
            }

            //===========================================
            // Field #9 is the Modify Time.
            //===========================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 9, out ftSite.mtime, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site modify time field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): S: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site modify time field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): T: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.MTIME] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FtSite.MTIME] = Constant.DB_NOT_NULL;
            }

            //=====================================================
            // Check if there are anomalous, additional CSV fields.
            //=====================================================
            string dummy;
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 10, out dummy, 1)) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, import Site; extra fields ignored\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                //...Log2.v("\n\nTitleRecord.Parse_TT(): M-1: importOK = Constant.FAILURE");
            }

            //...Log2.v("\nSiteRecord.Parse_SK(): Exit: importOK = " + importOK);
            return importOK;
        }


        /// <summary>
        /// This method parses a qualified line of type 'SD', validates its CSV fields and then
        /// completes the population of an FtSite object with all the imported values; a record is then
        /// inserted into the _site database table associated with the destination name.
        /// </summary>
        /// <param name="importOK"> - outputs zero if successful, otherwise a negative error code.</param>
        /// <param name="tabName"> - full name of the table to be inserted into.</param>
        /// <param name="qualLine"> - a prescribed qualified line, to be parsed and its values imported.</param>
        /// <param name="siteHandle"> - a cursor handle from a previous SQL SELECT query to be used for the SQL INSERT.</param>
        /// <returns>Zero if successful, otherwise a negative error code.</returns>
        public static int Parse_SD(ref short importOK, string tabName, QualLine qualLine, int siteHandle)
        {
            //...Log2.v("\n\nSiteRecord.Parse_SD(): Entry: importOK = " + importOK);

            int rc;

            FtSite ftSite = State.Site;
            SQLLEN[] siteNulls = State.SiteNullInds;

            if (State.SDflagSet)
            {
                msgBuf = String.Format("Error - improper order of SITE record types\r\nCheck that there is a SK and SD record for each site.\r\n");
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else
            {
                State.SDflagSet = true;
            }

            //================================
            // Field #0 is the type/qualifier.
            //================================

            // Field #1 is the Province.
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 1, out ftSite.prov, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site province field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): J: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site province field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): K: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.PROV] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FtSite.PROV] = Constant.DB_NOT_NULL;
            }

            //================================
            // Field #2 is the Operator Code.
            //================================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 2, out ftSite.oper, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site operator code field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): L: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site operator code field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): M: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.OPER] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FtSite.OPER] = Constant.DB_NOT_NULL;
            }

            //=========================
            // Field #3 is Site Status.
            //=========================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 3, out ftSite.stats, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site status field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): M: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site status field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): N: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.STATS] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FtSite.STATS] = Constant.DB_NOT_NULL;
            }

            //==========================
            // Field #4 is the Location.
            //==========================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 4, out ftSite.loc, 25)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site location field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): F: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site location field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SK(): G: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.LOC] = Constant.DB_NULL;
            }
            else
            {
                siteNulls[FtSite.LOC] = Constant.DB_NOT_NULL;
            }

            //=========================
            // Field # 5 is IC Account.
            //=========================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 5, out ftSite.icaccount, 12)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site IC account field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): O: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site IC account field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): P: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.ICACCOUNT] = Constant.DB_NULL;

            }
            else
            {
                siteNulls[FtSite.ICACCOUNT] = Constant.DB_NOT_NULL;
            }

            //=========================
            // Field # 6 is Region.
            //=========================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 6, out ftSite.reg, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site region field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): O: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site region field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): P: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.REG] = Constant.DB_NULL;
            }
            else
            {
                siteNulls[FtSite.REG] = Constant.DB_NOT_NULL;
            }

            //==========================
            // Field # 7 is Site Number.
            //==========================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 7, out ftSite.snumb, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site site number field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): O: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site site number field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): P: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.SNUMB] = Constant.DB_NULL;
            }
            else
            {
                siteNulls[FtSite.SNUMB] = Constant.DB_NOT_NULL;
            }

            //===============================
            // Field # 8 is Number of Towers.
            //===============================
            if ((rc = FtParsing.ParseFieldAsShort(qualLine, 8, out ftSite.notwr)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site number towers field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): O: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site number towers field, end of data\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): P: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.NOTWR] = Constant.DB_NULL;
            }
            else
            {
                siteNulls[FtSite.NOTWR] = Constant.DB_NOT_NULL;
            }

            //===================
            // Field #9 is Notes.
            //===================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 9, out ftSite.nots, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site notes field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site notes field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): R: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.NOTS] = Constant.DB_NULL;
            }
            else
            {
                siteNulls[FtSite.NOTS] = Constant.DB_NOT_NULL;
            }

            //======================
            // Field #10 is Pointer.
            //======================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 10, out ftSite.spoint, 4)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site pointer field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site pointer field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): R: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.SPOINT] = Constant.DB_NULL;
            }
            else
            {
                siteNulls[FtSite.SPOINT] = Constant.DB_NOT_NULL;
            }

            // Field #11 is Service Date.
            if ((rc = FtParsing.ParseFieldAsDate(qualLine, 11, out ftSite.sdate, 11)) < 0)
            {
                msgBuf = String.Format("Error - line #{0}, import Site service date field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = (short)rc;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): S: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site service date field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): R: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.SDATE] = Constant.DB_NULL;
            }
            else
            {
                siteNulls[FtSite.SDATE] = Constant.DB_NOT_NULL;

                // Check that the date is numerically valid.
                if (!ValUtil.UtValidateDate(ftSite.sdate))
                {
                    msgBuf = String.Format("Error - line #{0}, import Site service date field, invalid date\r\n", qualLine.LineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Constant.FAILURE;
                    //...Log2.v("\n\nSiteRecord.Parse_SD(): U: importOK = Constant.FAILURE");
                }
            }

            //============================
            // Field #12 is Operator Type.
            //============================
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 12, out ftSite.oprtyp, 2)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Site oper type field, invalid conversion\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): Q: importOK = Constant.FAILURE");
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Site operator type field, end of data found\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                //...Log2.v("\n\nSiteRecord.Parse_SD(): R: importOK = Constant.FAILURE");
            }
            else if (rc == 0)
            {
                siteNulls[FtSite.OPRTYP] = Constant.DB_NULL;
            }
            else
            {
                siteNulls[FtSite.OPRTYP] = Constant.DB_NOT_NULL;
            }

            //=====================================================
            // Check if there are anomalous, additional CSV fields.
            //=====================================================
            string dummy;
            if ((rc = FtParsing.ParseFieldAsString(qualLine, 13, out dummy, 1)) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, import Site; extra fields ignored\r\n", qualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                //...Log2.v("\n\nTitleRecord.Parse_TT(): M-1: importOK = Constant.FAILURE");
            }

            //...Log2.v("\nSiteRecord.Parse_SD(): Exit: importOK = " + importOK);
            return importOK;
        }



    }
}
