using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using _Configuration;
using _NewLib;
using _DataStructures;
using _Utillib;
using _Auxlib;

namespace FtValidate
{

    using SQLCHAR = Byte;
    using SQLCHARPTR = String;            //Invented to mimic (char *) for [In]  only.
    using SQLCHARPTRINOUT = IntPtr;       //Invented to mimic (char *) for [In, Out].
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLLEN = Int64;
    using SQLLENPTR = IntPtr;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;
    using SQLSETPOSIROW = UInt64;
    using SQLSMALLINT = Int16;
    using SQLSMALLINTPTR = IntPtr;
    using SQLULEN = UInt64;
    using SQLUSMALLINT = UInt16;

    /// <summary>
    /// Provides methods relating to the validation of antennae.
    /// </summary>
    public class ValAnte
    {
#if PINVOKE
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValAnteTS([In] string pdfName, [In, Out] ref short errCount, [In, Out] ref short warnCount, [In, Out] ref short valLevel);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValAnteAddTS([In] FtAnte ftAnte, [In] SQLLEN[] nArrayFW, [In] string pdfName, [In, Out] ref short errCount, [In, Out] ref short warnCount, [In, Out] ref short valLevel, [In] string keyLine);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern int ftCheckLicence([In] FtAnte ftAnte, [In] SQLLEN[] nArrayFW, [In, Out] ref short warnCount, [In] string keyLine);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValAnteDelTS([In] FtAnte ftAnte, [In] string pdfName, [In, Out] ref short errCount, [In, Out] ref short warnCount, [In] string keyLine);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValAnteUpdtTS([In] FtAnte ftAnte, [In] SQLLEN[] nArrayFW, [In] string pdfName, [In, Out] ref short errCount, [In, Out] ref short warnCount, [In, Out] ref short valLevel, [In] string keyLine);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern int ftCheckChanExist([In] FtAnte ftAnte, [In, Out] ref short warnCount, [In] string keyLine, [In] string pdfName);
        //--------------------------------------------------------------------------------------------
        public static void FtValAnteAddTS_NATIVE(FtAnte ftAnte, SQLLEN[] nArrayFW, string pdfName, ref short errCount, ref short warnCount, ref short valLevel, string keyLine)
        {
            ftValAnteAddTS(ftAnte, nArrayFW, pdfName, ref errCount, ref warnCount, ref valLevel, keyLine);
        }

        public static int FtCheckLicence_NATIVE(FtAnte ftAnte, SQLLEN[] nArrayFW, ref short warnCount, string keyLine)
        {
            return ftCheckLicence(ftAnte, nArrayFW, ref warnCount, keyLine);
        }

        public static void FtValAnteDelTS_NATIVE(FtAnte ftAnte, string pdfName, ref short errCount, ref short warnCount, string keyLine)
        {
            ftValAnteDelTS(ftAnte, pdfName, ref errCount, ref warnCount, keyLine);
        }

        public static void FtValAnteUpdtTS_NATIVE(FtAnte ftAnte, SQLLEN[] nArrayFW, string pdfName, ref short errCount, ref short warnCount, ref short valLevel, string keyLine)
        {
            ftValAnteUpdtTS(ftAnte, nArrayFW, pdfName, ref errCount, ref warnCount, ref valLevel, keyLine);
        }

        public static int FtCheckChanExist_NATIVE(FtAnte ftAnte, ref short warnCount, string keyLine, string pdfName)
        {
            return ftCheckChanExist(ftAnte, ref warnCount, keyLine, pdfName);
        }



        public static void FtValAnteTS_NATIVE(string pdfName, ref short errCount, ref short warnCount, ref short valLevel)
        {
            ftValAnteTS(pdfName, ref errCount, ref warnCount, ref valLevel);
        }
#endif

        /// <summary>
        ///  This method reads all the antenna records in the PDF, determines if each record is being added, 
        ///  modified or deleted, and performs validation accordingly.
        /// </summary>
        /// <param name="pdfName"> - name of the prescribed PDF file.</param>
        /// <param name="errCount"> - returns the number of errors encountered.</param>
        /// <param name="warnCount"> - returns the number of warnings issued.</param>
        /// <param name="valLevel"> - returns the level of validation attained.</param>
        public static void FtValAnteTS(string pdfName, ref short errCount, ref short warnCount, ref short valLevel)
        {
            //...Log2.v("\n\nValAnte.FtValAnteTS(): Entry");

            /* Local MS SQL Server aware variables */
            string whereClause = null;

            /* Local variables */
            SQLLEN[] nArrayFW;
            int aID1;
            string tableName = ""; // = new string(new char[Constant.TABLE_NM_SZ]);
            string keyLine; // = new string(new char[Constant.KEYLINESZ]);
            FtAnte ftAnte;
            int rc = Constant.SUCCESS;
            int nCount;

            // Get long name of antenna table.
            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);

            //Select the antenna information in the DB.
            //The returned value (aID1) is the numeric ID of the cursor 
            //that can be used to read the selected antenna data.
            aID1 = DynAntenna.FtSelectAntenna(tableName, whereClause, "");

            if (aID1 < 0)
            {
                ValErrs.AddMess("Could not read antenna information, cause(%d)", tableName, "W", aID1.ToString());
                warnCount++;
                return;
            }


            while (DynAntenna.FtFetchAntenna(aID1, out ftAnte, out nArrayFW) == Constant.SUCCESS)
            {
                keyLine = string.Format("{0} {1} {2} {3:D}", ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum);

                //...Log2.v("\r\nValAnte.FtValAnteTS(): keyLine = " + keyLine);

                // Bug fix: b150722A
                // If the ftAnte has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftAnte to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftAnte.cmd))
                {
                    continue;
                }

                char cmdChar = 'z';  // Initialize to any char that is not A, B, D, N or U.
                if (!String.IsNullOrWhiteSpace(ftAnte.cmd))
                {
                    cmdChar = ftAnte.cmd[0];
                }

                //...Log2.v("\r\nValAnte.FtValAnteTS(): ftAnte.cmd = " + ftAnte.cmd);
                //...Log2.v("\r\nValAnte.FtValAnteTS(): cmdChar    = " + cmdChar);
                switch (cmdChar)
                {
                    case 'A':
                        //...Log2.v("\r\nValAnte.FtValAnteTS(): case 'A'");
                        FtValAnteAddTS(ftAnte, nArrayFW, pdfName, ref errCount, ref warnCount, ref valLevel, keyLine);

                        // AH: b191119A
                        // Checking of licences is no longer required.
                        // Instance #1 of 2.
                        // FtCheckLicence(ftAnte, nArrayFW, ref warnCount, keyLine);
                        break;

                    case 'D':
                        //...Log2.v("\r\nValAnte.FtValAnteTS(): case 'D'");
                        FtValAnteDelTS(ftAnte, pdfName, ref errCount, ref warnCount, keyLine);
                        break;

                    case 'B':
                    case 'U':
                        //...Log2.v("\r\nValAnte.FtValAnteTS(): case 'B & 'U''");
                        FtValAnteUpdtTS(ftAnte, nArrayFW, pdfName, ref errCount, ref warnCount, ref valLevel, keyLine);

                        // AH: b191119A
                        // Checking of licences is no longer required.
                        // Instance #2 of 2.
                        // FtCheckLicence(ftAnte, nArrayFW, ref warnCount, keyLine);
                        break;

                    case 'N':
                        //...Log2.v("\r\nValAnte.FtValAnteTS(): case 'N'");
                        /* Determine if antenna is in MDB. */
                        whereClause = string.Format("call1='{0}' and call2='{1}' and bndcde='{2}' and anum={3:D}", ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum);
                        nCount = DynMdbAntenna.DbCountRows(whereClause);
                        if (nCount == 0)
                        {
                            /* Record not in MDB */
                            keyLine = string.Format("{0} {1} {2} {3:D}", ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum);
                            ValErrs.AddMess("Antenna does not exist in MDB.", keyLine, "E");
                            errCount++;
                        }

                        if (FtCheckChanExist(ftAnte, ref warnCount, keyLine, pdfName) == 0)
                        {
                            ValErrs.AddMess("You are deleting all channels for this antenna.", keyLine, "E");
                            errCount++;
                        }

                        break;

                    default:
                        // Bug fix: b150722A.
                        // The ftAnte's command was validated in a prior call to FtValAnteTSIntra() in 
                        // FtValidate.Main(). Consequently, we don't need to validate it again.
                        //...Log2.v("\r\nValAnte.FtValAnteTS(): case DEFAULT");
                        break;
                }

                rc = DynAntenna.FtUpdateAntenna(aID1, ftAnte, nArrayFW);

                if (rc != Constant.SUCCESS)
                {
                    ValErrs.AddMess("Could not update PDF Antenna data, cause(%d).", keyLine, "E", rc.ToString());
                    errCount++;
                }

            } // End while

            DynAntenna.FtCloseAntenna(aID1);

            //...Log2.v("\n\nValAnte.FtValAnteTS(): Exit");

        } // ----- End of ftValAnteTS -----



        /// <summary>
        /// Validates TS antenna records that are "ADD".
        /// </summary>
        /// <param name="ftAnte"> - antenna information.</param>
        /// <param name="nArrayFW"> - array of ODBC nullInds.</param>
        /// <param name="pdfName"> - pdf display name.</param>
        /// <param name="errCount"> - the cummulative number of errors encountered.</param>
        /// <param name="warnCount"> - the cummulative number of warnings encountered.</param>
        /// <param name="valLevel"> - level of validation.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        public static void FtValAnteAddTS(FtAnte ftAnte, SQLLEN[] nArrayFW, string pdfName, ref short errCount, ref short warnCount, ref short valLevel, string keyLine)
        {
            //...Log2.v("\n\nValAnte.FtValAnteAddTS(): Entry");
            /* Local variables */
            MtAnte mtAnte = new MtAnte();
            SQLLEN[] nArrayMDB;
            int rc;
            string whereClause = new string(new char[Constant.WHERE_SIZE]);

            /* make sure antenna does not already exist */
            ValImport.FtFormWhereClause(out whereClause, ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum, "");

            //...Log2.v("\r\nValAnte.FtValAnteAddTS(): A");
            rc = ValRetrieveTS.FtValRetrieveMDBAntenna(out mtAnte, whereClause, out nArrayMDB);
            //...Log2.v("\r\nValAnte.FtValAnteAddTS(): B");

            switch (rc)
            {
                case Constant.SUCCESS:
                    //...Log2.v("\r\nValAnte.FtValAnteAddTS(): case Constant.SUCCESS");
                    ValErrs.AddMess("Antenna already exists in MDB", keyLine, "E");
                    errCount++;
                    break;

                case Constant.NOMORERECS:
                    //...Log2.v("\r\nValAnte.FtValAnteAddTS(): case Constant.NOMORERECS");
                    break;

                case Constant.RECLOCK:
                    //...Log2.v("\r\nValAnte.FtValAnteAddTS(): case Constant.RECLOCK");
                    ValErrs.AddMess("Antenna record locked in MDB", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    //...Log2.v("\r\nValAnte.FtValAnteAddTS(): case default");
                    ValErrs.AddMess("Antenna undefined error: %d", keyLine, "W", rc.ToString());
                    warnCount++;
                    break;
            }

            //...Log2.v("\r\nValAnte.FtValAnteAddTS(): C");
            FtValAnteFieldsTS(ftAnte, nArrayFW, pdfName, ref errCount, ref warnCount, ref valLevel, ref keyLine);
            //...Log2.v("\r\nValAnte.FtValAnteAddTS(): D");

            //...Log2.v("\n\nValAnte.FtValAnteAddTS(): Exit");

        } // ----- End of ftValAnteAddTS -----

        /// <summary>
        /// Validates TS antenna records that are "Delete".
        /// </summary>
        /// <param name="ftAnte"> - antenna information.</param>
        /// <param name="pdfName"> - name of imported PDF file.</param>
        /// <param name="errCount"> - the cummulative number of errors encountered.</param>
        /// <param name="warnCount"> - the cummulative number of warnings encountered.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        public static void FtValAnteDelTS(FtAnte ftAnte, string pdfName, ref short errCount, ref short warnCount, string keyLine)
        {
            //...Log2.v("\nValAnte.FtValAnteDelTS(): Entry");

            /* Local variables */
            SQLLEN[] nArrayMDB; ;
            int rc;
            string whereClause = new string(new char[Constant.WHERE_SIZE]);
            MtAnte mtAnte = new MtAnte();


            /* make sure antenna does exist */
            ValImport.FtFormWhereClause(out whereClause, ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum, "");
            rc = ValRetrieveTS.FtValRetrieveMDBAntenna(out mtAnte, whereClause, out nArrayMDB);

            switch (rc)
            {
                case Constant.SUCCESS:
                    break;

                case Constant.NOMORERECS:
                    ValErrs.AddMess("Antenna to delete does not exist in MDB", keyLine, "E");
                    errCount++;
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Antenna to delete record locked  in MDB", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Antenna to delete undefined error: %d", keyLine, "W", rc.ToString());
                    warnCount++;
                    break;
            }

            /* find out if antenna is being used.  If so, FATAL ERROR */
            errCount += (short)AnteDel.FtValidateAnteDelete(ref pdfName, ref ftAnte.call1, ref ftAnte.call2, ref ftAnte.bndcde, ftAnte.anum);

            //...Log2.v("\nValAnte.FtValAnteDelTS(): Exit");
        } // ----- End of ftValAnteDelTS -----


        /// <summary>
        /// Validates TS antenna records that are "Update".
        /// </summary>
        /// <param name="ftAnte"> - antenna information.</param>
        /// <param name="nArrayFW"> - array of ODBC nullInds.</param>
        /// <param name="pdfName"> - name of PDF.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="valLevel"> - validation level.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        public static void FtValAnteUpdtTS(FtAnte ftAnte, SQLLEN[] nArrayFW, string pdfName, ref short errCount, ref short warnCount, ref short valLevel, string keyLine)
        {
            //...Log2.v("\nValAnte.FtValAnteUpdtTS(): Entry");

            /* Local variables */
            SQLLEN[] nArrayTemp;
            SQLLEN[] nArrayMDB;
            int ID1;
            int rc;
            string whereClause = new string(new char[Constant.WHERE_SIZE]);
            string tableName = new string(new char[Constant.TABLE_NM_SZ]);
            FtChan ftChan;
            MtAnte mtAnte = new MtAnte();
            int recExist = Constant.SUCCESS;


            /* make sure antenna does exist */
            ValImport.FtFormWhereClause(out whereClause, ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum, "");

            rc = ValRetrieveTS.FtValRetrieveMDBAntenna(out mtAnte, whereClause, out nArrayMDB);

            switch (rc)
            {
                case Constant.SUCCESS:
                    break;

                case Constant.NOMORERECS:
                    ValErrs.AddMess("Antenna to update does not exist in MDB", keyLine, "E");
                    recExist = Constant.FAILURE;
                    errCount++;
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Antenna to update record locked  in MDB", keyLine, "W");
                    recExist = Constant.FAILURE;
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Antenna to update record undefined error: %d", keyLine, "W", rc.ToString());
                    recExist = Constant.FAILURE;
                    warnCount++;
                    break;
            }

            if (rc != Constant.SUCCESS)
            {
                /* 	if we couldn't properly retrieve this record we don't want to continue
                *		validating */
                //...Log2.v("\nValAnte.FtValAnteUpdtTS(): Return: A");
                return;
            }

            if ((string.Compare(ftAnte.acode, mtAnte.acode) != 0) || (string.Compare(ftAnte.ause, mtAnte.ause) != 0) || (ftAnte.aht != mtAnte.aht))
            {
                /* 	acode, ause or aht are changing, change the channels
                *		aht added 2006.02.13 - 1184 - GJS	*/
                whereClause = string.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and " + "(antnumbrx1 = {3:D} or antnumbrx2 = {4:D} or antnumbrx3 = {5:D} " + " or antnumbtx1 ={6:D} or antnumbtx2 = {7:D})", ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum, ftAnte.anum, ftAnte.anum, ftAnte.anum, ftAnte.anum);

                GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

                /* read channel information */
                if ((ID1 = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                {
                    Log2.e("\r\nValAnte.FtValAnteUpdtTS(): ERROR: FtSelectChannel() returned " + ID1);
                    ValErrs.AddMess("Could not read channel information", keyLine, "W");
                    warnCount++;
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }
                else
                {
                    rc = DynChannel.FtFetchChannel(ID1, out ftChan, out nArrayTemp);
                    while (rc == Constant.SUCCESS)
                    {
                        /* Fix for bug631 - only change cmd->'U' if it is currently a 'N'. */
                        if ((string.Compare(ftChan.cmd, "N") == 0))
                        {
                            ftChan.cmd = "U".Substring(0, 1);
                        }
                        DynChannel.FtUpdateChannel(ID1, ftChan, nArrayTemp);
                        rc = DynChannel.FtFetchChannel(ID1, out ftChan, out nArrayTemp);
                    }
                    DynChannel.FtCloseChannel(ID1);
                }
            }

            /* Check modify date and time */
            if ((((string.Compare(ftAnte.mdate, mtAnte.mdate)) != 0) || ((string.Compare(ftAnte.mtime, mtAnte.mtime)) != 0)) && (recExist == Constant.SUCCESS))
            {
                /* Modify dates/times don't match */
                ValErrs.AddMess("This Antenna record has been modified since you retrieved it.", keyLine, "W");
                warnCount++;
            }

            FtValAnteFieldsTS(ftAnte, nArrayFW, pdfName, ref errCount, ref warnCount, ref valLevel, ref keyLine);

            if (FtCheckChanExist(ftAnte, ref warnCount, keyLine, pdfName) == 0)
            {
                ValErrs.AddMess("You are deleting all channels for this antenna.", keyLine, "E");
                errCount++;
            }

            //...Log2.v("\nValAnte.FtValAnteUpdtTS(): Exit");
        } // ----- End of FtValAnteUpdtTS -----

        /// <summary>
        /// Validates all of the TS antenna fields.
        /// </summary>
        /// <param name="ftAnte"> - antenna information.</param>
        /// <param name="nArrayFW"> - array of ODBC nullInds.</param>
        /// <param name="pdfName"> - name of PDF.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warnings count.</param>
        /// <param name="valLevel"> - validation level.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        public static void FtValAnteFieldsTS(FtAnte ftAnte, SQLLEN[] nArrayFW, string pdfName, ref short errCount, ref short warnCount, ref short valLevel, ref string keyLine)
        {
            //...Log2.v("\n\nValAnte.FtValAnteFieldsTS(): Entry");
            /* Local variables */
            SQLLEN[] nArrayTemp = Arrays.CreateArrayUsingDefaultElementConstructor<SQLLEN>(FtAnte.NUM_COLUMNS);
            SQLLEN[] nArrayMDB; ;
            int rc = new int();
            int sID1;
            int siteFound;
            int site2Found;
            double site1Ground = 0.0;
            double site2Ground = 0.0;
            double site1Lat = 0.0;
            double site2Lat = 0.0;
            double site1Long = 0.0;
            double site2Long = 0.0;
            double distanceKM = 0.0;
            double bearing12 = 0.0;
            double bearing21 = 0.0;
            double elevAng12 = 0.0;
            double elevAng21 = 0.0;
            double antHt1Amsl = 0.0;
            double antHt2Amsl = 0.0;
            double antHt2 = 0.0;
            string whereClause = new string(new char[Constant.WHERE_SIZE]);
            string tableName = new string(new char[Constant.TABLE_NM_SZ]);
            FtSite ftSite = new FtSite();
            MtSite mtSite;

            /* Local variables */
            string FWoper = new string(new char[7]);

            if (nArrayFW[FtAnte.AUSE] == Constant.DB_NULL)
            {
                /* must enter data in field */
                ValErrs.AddMess("Must enter Antenna Use field.", keyLine, "E");
                errCount++;
            }


            if (nArrayFW[FtAnte.ACODE] == Constant.DB_NULL)
            {
                /* must enter data in field */
                ValErrs.AddMess("Must enter Antenna Code field.", keyLine, "E");
                errCount++;
            }


            if (nArrayFW[FtAnte.AHT] == Constant.DB_NULL)
            {
                /* must enter data in field */
                ValErrs.AddMess("Must enter Antenna Height field.", keyLine, "E");
                errCount++;
            }

            /* get the operator code , lat and long and ground from the Site table */
            GenUtil.UtCvtName(Constant.FT_SITE, pdfName, out tableName);

            ValImport.FtFormWhereClause(out whereClause, ftAnte.call1, "", "", Constant.NO_ANUM, "");
            siteFound = Constant.FAILURE;
            rc = Constant.FAILURE;
            if ((sID1 = DynSite.FtSelectSite(tableName, whereClause, "")) >= 0)
            {
                /* site record selected , fetch it */
                if (DynSite.FtFetchSite(sID1, out ftSite, out nArrayTemp) == Constant.SUCCESS)
                {
                    rc = Constant.SUCCESS;
                    /* found the record in the pdf */
                    FWoper = ftSite.oper.Trim();
                    site1Long = (double)(ftSite.longit) / 100.0;
                    site1Lat = (double)(ftSite.latit) / 100.0;
                    site1Ground = ftSite.grnd;
                    siteFound = Constant.SUCCESS;
                }
                DynSite.FtCloseSite(sID1);
            }

            if (rc != Constant.SUCCESS)
            {
                /* could not find remote site in the pdf */
                /* Look for it in the MDB */

                if (ValRetrieveTS.FtValRetrieveMDBSite(out mtSite, whereClause, out nArrayMDB) == Constant.SUCCESS)
                {
                    /* found the record in the mdb */
                    FWoper = mtSite.oper.Trim();
                    site1Long = (double)(mtSite.longit) / 100.0;
                    site1Lat = (double)(mtSite.latit) / 100.0;
                    site1Ground = mtSite.grnd;
                    siteFound = Constant.SUCCESS;
                }
                else
                {
                    /* Site is neither in the PDF nor the MDB */
                    ValErrs.AddMess("No local site exists for this antenna", keyLine, "E");
                    errCount++;
                    site1Long = site1Lat = site1Ground = 0.0;
                    siteFound = Constant.FAILURE;
                }
            }

            /* get the latitude and longitude and grnd from the remote Site table */
            GenUtil.UtCvtName(Constant.FT_SITE, pdfName, out tableName);

            ValImport.FtFormWhereClause(out whereClause, ftAnte.call2, "", "", Constant.NO_ANUM, "");
            site2Found = Constant.FAILURE;
            rc = Constant.FAILURE;
            if ((sID1 = DynSite.FtSelectSite(tableName, whereClause, "")) >= 0)
            {
                /* site record selected , fetch it */
                if (DynSite.FtFetchSite(sID1, out ftSite, out nArrayTemp) == Constant.SUCCESS)
                {
                    rc = Constant.SUCCESS;
                    /* found the record in the pdf */
                    site2Long = (double)(ftSite.longit) / 100.0;
                    site2Lat = (double)(ftSite.latit) / 100.0;
                    site2Ground = ftSite.grnd;
                    site2Found = Constant.SUCCESS;
                }
                DynSite.FtCloseSite(sID1);
            }

            if (rc != Constant.SUCCESS)
            {
                /* could not find remote site in the pdf */
                /* Look for it in the MDB */
                if (ValRetrieveTS.FtValRetrieveMDBSite(out mtSite, whereClause, out nArrayMDB) == Constant.SUCCESS)
                {
                    /* found the record in the mdb */
                    site2Long = (double)(mtSite.longit) / 100.0;
                    site2Lat = (double)(mtSite.latit) / 100.0;
                    site2Ground = mtSite.grnd;
                    site2Found = Constant.SUCCESS;
                }
                else
                { // Site is neither in the PDF nor the MDB
                    ValErrs.AddMess("No remote site exists for this antenna", keyLine, "E");
                    errCount++;
                    site2Long = site2Lat = site2Ground = 0.0;
                    site2Found = Constant.FAILURE;
                }
            }

            /* validate nota against snote table (nonum) */
            if ((nArrayFW[FtAnte.NOTA] != Constant.DB_NULL) && (siteFound == Constant.SUCCESS))
            {
                SuNote tNote;

                rc = Suutils.SuGetNote(FWoper, ftAnte.nota, out tNote);
                if (rc != 0)
                {
                    ValErrs.AddMess("Nota (%s) not present in SDB for Operator '%s'", keyLine, "E", ftAnte.nota, FWoper);
                    errCount++;
                }
            }

            //...Log2.v("\r\nValAnte.FtValAnteFieldsTS(): A");

            /* validate call1 and atwrno against sd_town table
                =============================================== */
            if (nArrayFW[FtAnte.ATWRNO] != Constant.DB_NULL && siteFound == Constant.SUCCESS)
            {
                SuTown sctTown;

                rc = Suutils.SuGetTown(ftAnte.call1, ftAnte.atwrno, out sctTown);

                /*  The twcode could be null */
                if (rc != 0)
                {
                    ValErrs.AddMess("Tower %d not present for this Call1 in Tower Notes SDB.", keyLine, "E", ftAnte.atwrno.ToString());
                    errCount++;
                }
            }

            //...Log2.v("\r\nValAnte.FtValAnteFieldsTS(): BZ");

            SuBand pBand = new SuBand();
            rc = Suutils.SuGetBand(ftAnte.bndcde, out pBand);

            //...Log2.v("\r\nValAnte.FtValAnteFieldsTS(): B-1");

            if (rc != 0)
            {
                //...Log2.v("\r\nValAnte.FtValAnteFieldsTS(): B-2");
                ValErrs.AddMess("Band Code (%s) not present in SDB", keyLine, "E", ftAnte.bndcde);
                errCount++;
            }

            //...Log2.v("\r\nValAnte.FtValAnteFieldsTS(): C");

            /* validate acode for special characters or */
            /* validate acode against sante table (acode) */


            //if ((ftAnte.acode[Convert.ToString(ftAnte.acode.Trim()).Length - 1] == '%') && (ftAnte.call1[0] != '%'))
            if (Strings.LastCharIs(ftAnte.acode.Trim(), '%') && !Strings.FirstCharIs(ftAnte.call1, '%'))

            {
                /* first char of CALL1 is not %  and last of ACODE is % */
                /* Is the site being changed to % */
                if (ValidTS.FtChkChng(pdfName, ftAnte.call1, "%") != Constant.SUCCESS)
                {
                    ValErrs.AddMess("Call sign (%s) must start with %% if Antenna Code starts with %%", keyLine, "E", ftAnte.call1);
                    errCount++;
                }
            }

            //...Log2.v("\r\nValAnte.FtValAnteFieldsTS(): D");

            //if ((ftAnte.acode[Convert.ToString(ftAnte.acode.Trim()).Length - 1] == '%') && (ftAnte.call1[0] == '%'))
            if (Strings.LastCharIs(ftAnte.acode.Trim(), '%') && Strings.FirstCharIs(ftAnte.call1, '%'))
            {
                SuAntStr pAnt;

                if (Suutils.SuGetAnt(ftAnte.acode, out pAnt) != 0)
                {
                    ValErrs.AddMess("Antenna Code (%s) not present in the Antenna Table", keyLine, "E", ftAnte.acode);
                    errCount++;
                }
            }


            //...Log2.v("\r\nValAnte.FtValAnteFieldsTS(): E");

            //            if (((string.Compare(ftAnte.cmd, "A") == Constant.SUCCESS) || (string.Compare(ftAnte.cmd, "U") == Constant.SUCCESS) || (string.Compare(ftAnte.cmd, "B") == Constant.SUCCESS)) && ((string.Compare(ftAnte.acode, 0, "CCIR", 0, 4) == 0) || (ftAnte.acode[0] == '$')))
            if (((string.Compare(ftAnte.cmd, "A") == Constant.SUCCESS) || (string.Compare(ftAnte.cmd, "U") == Constant.SUCCESS) || (string.Compare(ftAnte.cmd, "B") == Constant.SUCCESS)) && ((string.Compare(ftAnte.acode, 0, "CCIR", 0, 4) == 0) || Strings.FirstCharIs(ftAnte.acode, '$')))
            {
                //...Log2.v("\r\nValAnte.FtValAnteFieldsTS(): E-1");
                /* let this situation pass SDB validation */
            }
            else
            {
                //...Log2.v("\r\nValAnte.FtValAnteFieldsTS(): E-2");
                SuAntStr pAnt; ;

                if (Suutils.SuGetAnt(ftAnte.acode, out pAnt) != 0)
                {
                    //...Log2.v("\r\nValAnte.FtValAnteFieldsTS(): E-3");
                    ValErrs.AddMess("Antenna Code (%s) not present in the Antenna Table.", keyLine, "E", ftAnte.acode);
                    errCount++;
                }
            }

            //...Log2.v("\r\nValAnte.FtValAnteFieldsTS(): F");

            /* If temporary codes being used, then only valid for TSIP */
            if (Strings.FirstCharIs(ftAnte.acode, '$'))
            {
                //...Log2.v("\r\nValAnte.FtValAnteFieldsTS(): F-1");
                /* first character of acode is $ */
                valLevel = Constant.TSIP;
            }



            /****************************************************************************
        // *
            * If this is an area site (indicated by call1 of AH for an area hub, AR
            *	for an Area Remote, and AP for the single representation of a Point-to-Point
            *	area, then we expect the distance to already be present, and this is what
            *	we leave.  GJS - 1095 - 2005.03.09
            *
            \****************************************************************************/
            if (ftAnte.call1[0] != 'A')
            {
                /* 	This is not an area site so calculate distance and azimuth between
                *		two sites - if both exist */
                if ((siteFound != Constant.FAILURE) && (site2Found != Constant.FAILURE))
                {
                    /* 	calculate dist and azmth using DISTAN.  We first check that the lats
                    *		and longs are not equal.  If they are, we move the lat of the lowest
                    *		ascii site North .1 sec, and give the user an error message
                    *		GJS  7/14/2000  Task 1102 */
                    if (site1Lat == site2Lat && site1Long == site2Long)
                    {
                        /*	They are co-located.  */
                        ValErrs.AddMess("Sites %s and %s at each end of link are co-located.", keyLine, "E", ftAnte.call1, ftAnte.call2);
                        errCount++;
                    }

                    AxSub2.AxDistan(site1Lat, site2Lat, site1Long, site2Long, out distanceKM, out bearing12, out bearing21);

                    /* Set local distance and azimuth */
                    /* Check to be sure that we don't get values very close to 0.0,
                    ** MS SQL Server forms chokes on things like that ! */
                    ftAnte.dist = (float)distanceKM;
                    ftAnte.azmth = (float)bearing12;
                    nArrayFW[FtAnte.DIST] = Constant.DB_NOT_NULL;
                    nArrayFW[FtAnte.AZMTH] = Constant.DB_NOT_NULL;

                    if (ftAnte.dist == 0.0)
                    {
                        /* Lat & Long for these two sites is the same ! */
                        ftAnte.dist = 0.01f;
                    }

                    /* calculate elvtn using ELEV  */

                    /* get the remote antenna's height */
                    antHt2 = RemoteAnte.GetHighestRemMainAntenna(pdfName, ref ftAnte);

                    //&&Console.Error.Write("\nE:antHt2 = {0:F6}", antHt2);

                    if (antHt2 < 0.0)
                    {
                        /* Can't find remote antennae */
                        if (antHt2 == Constant.NO_LOC_CHANS)
                        {
                            /* Problem is because no local chan's - ie)
                            ** an unused antenna.  There is no way to find
                            ** the remote of an unused antenna */
                            ValErrs.AddMess("Cannot calculate elev. ang. Antenna has no channels.", keyLine, "W");
                            warnCount++;
                        }
                        else
                        {
                            /* A more serious problem exists */
                            ValErrs.AddMess("Could not retrieve remote Antenna", keyLine, "E");
                            errCount++;
                        }
                    }
                    else
                    {
                        /* got the remote antenna's height */
                        antHt2Amsl = (antHt2 + site2Ground) / 1000.0;

                        //&&Console.Error.Write("\nD: {0:F6}  {1:F6}", antHt2, site2Ground);

                        /* convert height to km */
                        antHt1Amsl = (ftAnte.aht + site1Ground) / 1000.0;

                        AxSub3.AxElev(antHt1Amsl, antHt2Amsl, distanceKM, out elevAng12, out elevAng21);

                        //&&Console.Error.Write("\nC: {0:F6}  {1:F6}  {2:F6}  {3:F6}  {4:F6}", antHt1Amsl, antHt2Amsl, distanceKM, elevAng12, elevAng21);

                        /*
                        *		Corrected by using the actual triangle task 1179 GJS 2004.08
                        */
                        ftAnte.dist = (float)AxSub3.PathDist(antHt1Amsl, antHt2Amsl, distanceKM);
                        if (ftAnte.dist > 90.0)
                        {
                            ValErrs.AddMess("Distance (%s) is greater than 90km.", keyLine, "W", ftAnte.dist.ToString("#.0"));
                            warnCount++;
                        }

                        ftAnte.elvtn = (float)elevAng12;
                        nArrayFW[FtAnte.ELVTN] = Constant.DB_NOT_NULL;

                    }
                }
            }
            else
            {
                /*	This is an area site, just print a message and don't recalculate geometry */
                nArrayFW[FtAnte.DIST] = Constant.DB_NOT_NULL;
                nArrayFW[FtAnte.AZMTH] = Constant.DB_NOT_NULL;
                nArrayFW[FtAnte.ELVTN] = Constant.DB_NOT_NULL;
                ValErrs.AddMess("This is an Area Site.  Geometry will not be recalculated.", keyLine, "W");
            }

            /* on add set offazm to N if blank */
            if ((nArrayFW[FtAnte.OFFAZM] == Constant.DB_NULL) && (string.Compare(ftAnte.cmd, "A") == 0))
            {
                ftAnte.offazm = "N";
                nArrayFW[FtAnte.OFFAZM] = Constant.DB_NOT_NULL;
            }

            //...Log2.v("\n\nValAnte.FtValAnteFieldsTS(): Exit");

        } // ----- End of ftValAnteFieldsTS -----


        /// <summary>
        /// Warns users if their PDF would leave an antenna without channels.
        /// </summary>
        /// <param name="ftAnte"> - antenna information.</param>
        /// <param name="warnCount"> - cummulative warnings count.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        /// <param name="pdfName"> - name of PDF.</param>
        /// <returns></returns>
        public static int FtCheckChanExist(FtAnte ftAnte, ref short warnCount, string keyLine, string pdfName)
        {
            string whereClause = new string(new char[Constant.WHERE_SIZE]);
            string mdbClause = new string(new char[Constant.WHERE_SIZE]);
            string tableName = new string(new char[Constant.TABLE_NM_SZ]);
            int chanHandle;
            int mdbHandle;
            int rc;
            FtChan ftChan = new FtChan();
            SQLLEN[] pdfArrayTemp;
            MtChan mtChan = new MtChan();
            SQLLEN[] mdbArrayTemp = NullHelper.CreateArrayOfNullInd(Constant.MT_CHAN_SIZE_, NullHelper.ColumnStatus.NULL); /* return code TRUE -> there are channels

        														FALSE -> there are no channels */
            int chanExist = Constant.FALSE;

            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

            whereClause = string.Format("cmd != '{0}' and call1 = '{1}' and call2 = '{2}' and bndcde = '{3}' and " + "(antnumbrx1 = {4:D} or antnumbrx2 = {5:D} or antnumbrx3 = {6:D} or " + "antnumbtx1 = {7:D} or antnumbtx2 = {8:D})", "D", ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum, ftAnte.anum, ftAnte.anum, ftAnte.anum, ftAnte.anum);

            if ((chanHandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
            {
                Log2.e("\r\nValAnte.CheckChanExist(): ERROR: FtSelectChannel() returned " + chanHandle);
                ValErrs.AddMess("Could not read channel information", keyLine, "W");
                warnCount++;
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }
            else
            {
                rc = DynChannel.FtFetchChannel(chanHandle, out ftChan, out pdfArrayTemp);
                if (rc == Constant.SUCCESS)
                {
                    chanExist = 1;
                }
            }

            DynChannel.FtCloseChannel(chanHandle);

            if (chanExist == 0)
            /* if we know there will be channels don't bother */
            {
                mdbClause = string.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and (antnumbrx1 = {3:D} or antnumbrx2 = {4:D} or antnumbrx3 = {5:D} or antnumbtx1 = {6:D} or antnumbtx2 = {7:D})", ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum, ftAnte.anum, ftAnte.anum, ftAnte.anum, ftAnte.anum);

                mdbHandle = DynMdbChannel.MtSelectChannel(mdbClause, "");

                //...Log2.v("\r\nValAnte.CheckChanExist(): mdbHandle = ");

                if (mdbHandle < 0)
                {
                    ValErrs.AddMess("Could not select channels in mdb.  Reason: %d", keyLine, "W", mdbHandle.ToString());
                    warnCount++;
                }
                else
                {
                    while ((DynMdbChannel.MtFetchChannel(mdbHandle, out mtChan, out mdbArrayTemp) == Constant.SUCCESS) && (chanExist != 1))
                    {
                        /* loop through each mdb channel record until we determine that there
                           will be channels after PDF is run
                        */
                        ValImport.FtFormWhereClause(out whereClause, mtChan.call1, mtChan.call2, mtChan.bndcde, Constant.NO_ANUM, mtChan.chid);
                        if ((chanHandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                        {
                            ValErrs.AddMess("Could not select channels in pdf.  Reason %d", keyLine, "W", chanHandle.ToString());
                            warnCount++;
                        }
                        else
                        {
                            if (DynChannel.FtFetchChannel(chanHandle, out ftChan, out pdfArrayTemp) == Constant.SUCCESS)
                            {
                                /* Once we get the mdb channel record we look in the PDF to
                                   find the corresponding rec.  If it doesn't exist then
                                   there will be a chan rec. after the PDF is run.  If it
                                   does exist then it will exist as long as the PDF rec is not
                                   marked as Delete
                                */

                                if (!ftChan.CmdEquals('D'))
                                {
                                    /* chan will exist */
                                    chanExist = 1;
                                }
                            }
                            else
                            {
                                /* chan will exist */
                                chanExist = 1;
                            }
                        }
                        DynChannel.FtCloseChannel(chanHandle);
                    }
                    DynMdbChannel.MtCloseChannel(mdbHandle);
                }
            }
            return ((int)chanExist);
        }

        /// <summary>
        /// Check the licence within the local file.
        /// </summary>
        /// <remarks>
        /// We check if the call1-call2 have appeared before (without ordering), and if so what the licence was.
        /// If we have no record of it, we make one. The warning level is incremented if the licences exist but don't match,
        /// 	and 1 is returned.
        /// </remarks>
        /// <param name="ftAnte"> - antenna information.</param>
        /// <param name="nArrayFW"> - array of ODBC nullInds.</param>
        /// <param name="warnCount"> - cummulative warnings count.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        /// <returns></returns>
        public static int FtCheckLicence(FtAnte ftAnte, SQLLEN[] nArrayFW, ref short warnCount, string keyLine)
        {
            //...Log2.v("\n\nValAnte.FtCheckLicence(): Entry: ftAnte = " + ftAnte.KeysToStringTerse());

            string cLicence = new string(new char[Constant.LICENCE_SZ]);
            int nRet;

            //  This is only interesting if the antenna has a licence
            if (nArrayFW[FtAnte.LICENCE] != Constant.DB_NULL && ftAnte.licence.Length > 0)
            {
                //...Log2.v("\nValAnte.FtCheckLicence(): A: nArrayFW[FtAnte.LICENCE] != Constant.DB_NULL && ftAnte.licence.Length > 0");

                //	See if we can get a saved licence for this pair
                if (GenUtil.GetSavedLicence(ftAnte.call1, ftAnte.call2, out cLicence) == 0)
                {
                    //...Log2.v("\nValAnte.FtCheckLicence(): B: cLicence = " + cLicence);

                    //	Got the licence, check that the two are the same.
                    if (string.Compare(cLicence, ftAnte.licence) == 0)
                    {
                        //...Log2.v("\nValAnte.FtCheckLicence(): C: cLicence and ftAnte.licence are the SAME");

                        //	They are the same.  All is good.
                        nRet = 0;
                    }
                    else
                    {
                        //...Log2.v("\nValAnte.FtCheckLicence(): D: cLicence and ftAnte.licence are DIFFERENT");

                        //	Found one and it is not the same.  Increment the warnings and return 1.
                        ValErrs.AddMess("Licence fields differ between %s and %s.", keyLine, "W", ftAnte.call1, ftAnte.call2);
                        warnCount++;
                        nRet = 1;
                    }
                }
                else
                {
                    //...Log2.v("\nValAnte.FtCheckLicence(): E: no existing saved licence.");

                    //	The licence is not present.  Add it.
                    GenUtil.AddSavedLicence(ftAnte.call1, ftAnte.call2, ftAnte.licence);
                    nRet = 0; // Okay so far.

                    //...Log2.v("\nValAnte.FtCheckLicence(): E: saving license for pair: " + ftAnte.call1 + ", " + ftAnte.call2 + ", " +ftAnte.licence);
                }
            }
            else
            {
                //...Log2.v("\nValAnte.FtCheckLicence(): F: ftAnte does not have a lincece.");

                //	no licence.  Still okay
                nRet = 0;
            }

            //...Log2.v("\nValAnte.FtCheckLicence(): Exit: nRet = " + nRet);
            return nRet;
        }


    }
}
