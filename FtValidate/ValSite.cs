using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FtValidate
{
    using _Configuration;
    using _DataStructures;

    using _NewLib;
    using _Utillib;
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;            //Invented to mimic (char *) for [In]  only.
    using SQLCHARPTRINOUT = IntPtr;       //Invented to mimic (char *) for [In, Out].
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLINTEGERPTR = IntPtr;
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
    /// Provides methods related to the validation of site information.
    /// </summary>
    public class ValSite
    {
#if PINVOKE
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValSiteTS(string s, ref short sh1, ref short sh2);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern int checkAnteExist([In] FtSite ftSite, [In, Out] ref short warnCount, [In] string keyLine, [In] string pdfName);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValSiteAddTS([In] FtSite ftSite, [In] SQLLEN[] nArrayFW, [In] string pdfName, [In, Out] ref short errCount, [In, Out] ref short warnCount, [In] string keyLine);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern int setBandWord([In] string pdfName, [In, Out] FtSite ftSite, [In, Out] SQLLEN[] nArrayFW);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern int mdbLatLong([In] string call1, [In] int latit, [In] int longit, [In] string keyLine, [In] string titleLine, [In, Out] ref short warnCount);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValSiteDelTS([In] FtSite ftSite, [In] string pdfName, ref short errCount, [In, Out] ref short warnCount, [In] string keyLine);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValSiteUpdtTS([In] FtSite ftSite, [In] SQLLEN[] nArrayFW, [In] string pdfName, [In, Out] ref short errCount, [In, Out] ref short warnCount, [In] string keyLine);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValSiteFieldsTS([In] FtSite ftSite, [In] SQLLEN[] nArrayFW, [In, Out] ref short errCount, [In, Out] ref short warnCount, [In] string keyLine, [In]string pdfName);
        
        static void FtValSiteFieldsTS_NATIVE(FtSite ftSite, SQLLEN[] nArrayFW, ref short errCount, ref short warnCount, string keyLine, string pdfName)
        {
            ftValSiteFieldsTS(ftSite, nArrayFW, ref errCount, ref warnCount, keyLine, pdfName);
        }

        public static void FtValSiteUpdtTS_NATIVE(FtSite ftSite, SQLLEN[] nArrayFW, string pdfName, ref short errCount, ref short warnCount, string keyLine)
        {
            ftValSiteUpdtTS(ftSite, nArrayFW, pdfName, ref errCount, ref warnCount, keyLine);
        }
        public static void FtValSiteDelTS_NATIVE(FtSite ftSite, string pdfName, ref short errCount, ref short warnCount, string keyLine)
        {
            ftValSiteDelTS(ftSite, pdfName, ref errCount, ref warnCount, keyLine);
        }

        public static int MdbLatLong_NATIVE(string call1, int latit, int longit, string keyLine, string titleLine, ref short warnCount)
        {
            return mdbLatLong(call1, latit, longit, keyLine, titleLine, ref warnCount);
        }

        
        public static int SetBandWord_NATIVE(string pdfName, ref FtSite ftSite, ref SQLLEN[] nArrayFW)
        {
            return setBandWord(pdfName, ftSite, nArrayFW);
        }

        public static void FtValSiteTS_NATIVE(string pdfName, ref short errorCount, ref short warningsCount)
        {
            ftValSiteTS(pdfName, ref errorCount, ref warningsCount);
        }

        public static bool CheckAnteExist_NATIVE(FtSite ftSite, ref short warnCount, string keyLine, string pdfName)
        {
            bool exists = false;

            int nRet = checkAnteExist(ftSite, ref warnCount, keyLine, pdfName);

            if (nRet == Constant.TRUE)
            {
                exists = true;
            }

            return exists;
        }
        
        public static void FtValSiteAddTS_NATIVE(FtSite ftSite, SQLLEN[] nArrayFW, string pdfName, ref short errCount, ref short warnCount, string keyLine)
        {
            ftValSiteAddTS(ftSite, nArrayFW, pdfName, ref errCount, ref warnCount, keyLine);
        }
#endif

        //------------------------------------------------------------------------------

        /// <summary>
        /// Encapsulates the data structure of an antenna number / band stack element.
        /// </summary>
        private class AnumBand
        {
            public string cmd;         /* PDF cmd */
            public string call2;
            public string bndcde;      /* bandcode */
            public short anum;          /* antenna number */

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name=""></param>
            /// <returns></returns>
            public AnumBand()
            {
                cmd = "";
                call2 = "";
                bndcde = "";
                anum = 0;
            }
        }

        /* Allocate storage for Band code list */
        private static AnumBand[] bandStack = new AnumBand[Constant.MAX_BAND_CODES];
        private static int topOfStack = 0;          /* cur size of list */

        //--------------------------------------------------------------------------------

        /// <summary>
        /// Performs the site validation for a DELETE record; it verifies that the record does not
        /// exist and calls the field validation routine to verify that all fields are correct.
        /// </summary>
        /// <param name="ftSite"> - the site object.</param>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        public static void FtValSiteDelTS(FtSite ftSite, string pdfName, ref short errCount, ref short warnCount, string keyLine)
        {
            /* Local variables */
            SQLLEN[] nArrayTemp;  // [max(FtAnte.SIZE_, FtChan.SIZE_)],
            SQLLEN[] nArrayMDB;
            int ID1;
            int rc;
            string whereClause;
            string tableName;
            MtSite mtSite;
            FtAnte ftAnte;
            FtChan ftChan;


            /* make sure site does exist */
            whereClause = String.Format("call1='{0}'", ftSite.call1);

            rc = ValRetrieveTS.FtValRetrieveMDBSite(out mtSite, whereClause, out nArrayMDB);

            keyLine = String.Format("Call: {0}", ftSite.call1);

            switch (rc)
            {
                case Constant.SUCCESS:
                    break;

                case Constant.NOMORERECS:
                    ValErrs.AddMess("Site does not exist in MDB", keyLine, "E");
                    errCount++;
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Site record locked  in MDB", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Site undefined error", keyLine, "W");
                    warnCount++;
                    break;
            }

            /* any local or remote antennae in the PDF must be for Deletion. */

            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);
            whereClause = String.Format("(call1='{0}'  or call2='{1}') and cmd != 'D'",
                      ftSite.call1, ftSite.call1);

            if ((ID1 = DynAntenna.FtSelectAntenna(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read antenna information", keyLine, "W");
                warnCount++;

            }
            else
            {
                while (DynAntenna.FtFetchAntenna(ID1, out ftAnte, out nArrayTemp) == Constant.SUCCESS)
                {
                    ValErrs.AddMess("Antenna refers to non existent site.", keyLine, "E");
                    errCount++;
                }
                DynAntenna.FtCloseAntenna(ID1);
            }

            /* modify channel information */
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);
            if ((ID1 = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read channel information", keyLine, "W");
                warnCount++;
            }
            else
            {
                while (DynChannel.FtFetchChannel(ID1, out ftChan, out nArrayTemp) == Constant.SUCCESS)
                {
                    ValErrs.AddMess("Channel refers to non existent site.", keyLine, "E");
                    errCount++;
                }
                DynChannel.FtCloseChannel(ID1);
            }

        }   /* ----- End of ftValSiteDelTS ----- */

        /// <summary>
        /// Checks that there are no sites in the database that have
        /// the same Lat and Long but not the same callsign as the current one in
        /// the PDF.
        /// </summary>
        /// <remarks>
        /// For addition the call sign check shouldn't matter (the callsign
        /// want be in the MDB anyway), but it is required when the site is being
        /// updated, and the new lat and long conflict with an entirely different
        /// site.
        /// </remarks>
        /// <param name="call1"> - callsign of the site.</param>
        /// <param name="latit"> - latitude of the site.</param>
        /// <param name="longit"> - longitude of the site.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        /// <param name="titleLine"> - error title line.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <returns> - Constant.SUCCESS if check succeeds.</returns>
        public static int MdbLatLong(string call1, int latit, int longit, string keyLine, string titleLine, ref short warnCount)
        {
            string SQLselect;
            string cCallFound = "";
            int nRet = 0;
            MtSiteStr pSite;

            keyLine = String.Format("Site Call: {0}", call1);

            SQLselect = String.Format("latit = {0} and longit = {1} and call1 != '{2}'", latit, longit, call1);

            nRet = MtUtils.MtEnumSite(SQLselect, ref cCallFound);

            while (nRet == 0)
            {
                nRet = MtUtils.MtGetSite(cCallFound, out pSite, 1);    // Just get the site information

                if (nRet == 0)
                {
                    ValErrs.AddMess("Lat & Long for site is the same as for existing site:- \r\n\t%s %s %s\r\nCheck to ensure coordinates are correct.",
                                    keyLine, "W", pSite.stSite.call1, pSite.stSite.name, pSite.stSite.oper);
                    warnCount++;
                }

                nRet = MtUtils.MtEnumSite(SQLselect, ref cCallFound);
            }

            return 0;
        }

        /// <summary>
        /// Calculates the band word for each site in the PDF;
        /// it considers antennae in the PDF and the MDB with the data in the
        /// PDF taking precedence over the MDB data.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="ftSite"> - the site object.</param>
        /// <param name="nArraySite"> - array of ODBC nullInds for ftSite.</param>
        /// <returns>  - Constant.SUCCESS if calculation succeeds.</returns>
        public static int SetBandWord(string pdfName, ref FtSite ftSite, ref SQLLEN[] nArraySite)
        {
            /* Local MS SQL Server aware variables */
            int bandBitPos;           /* bit pos in bitmap */
            string whereClause;/* selection criteria */
            string bndcde;  /* ASCII bandcode */
            int nRet;

            /* Local variables */
            int anteHandle;         /* Ante. table handle */
            uint[] bandWord = new uint[Constant.FT_BANDWD_CT];   /* bandword bitmap */
            string tableName;           /* internal table name */
            FtAnte tempAnte;                    /* antenna record */
            SQLLEN[] nullIndAnte;   /* ante. rec null indicators */


            MtSiteStr pmtSite;
            SuBand pBand;

            InitBandStack();            /* initialize Band list */

            /* Start off by tabulating a list of band codes */
            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);
            nRet = MtUtils.MtGetSite(ftSite.call1, out pmtSite, 2); //	Just get the antennas.
            if (nRet == 0)
            {
                for (int nInd = 0; nInd < pmtSite.nNumAnts; nInd++)
                {
                    //if (strlen(pmtSite.stAnts[nInd].bndcde) > 0)
                    if (!String.IsNullOrWhiteSpace(pmtSite.stAntsPtr[nInd].bndcde))
                    {
                        PushBand("N", pmtSite.stAntsPtr[nInd].call2,
                                 pmtSite.stAntsPtr[nInd].bndcde, pmtSite.stAntsPtr[nInd].anum);
                    }
                }
            }

            //freeSite(pmtSite);

            /* find all antennas in the PDF associated with this site */
            whereClause = String.Format(" call1 = '{0}'", ftSite.call1);

            if ((anteHandle = DynAntenna.FtSelectAntenna(tableName, whereClause, "")) < 0)
            {
                return (Error.DYN_MS_SQL_SERVER_ERR);
            }

            /* Fetch all antennae from PDF. Add their bands to the list */
            while (DynAntenna.FtFetchAntenna(anteHandle, out tempAnte, out nullIndAnte) == Constant.SUCCESS)
            {
                if (nullIndAnte[FtAnte.BNDCDE] != Constant.DB_NULL)
                {
                    /* add this band to the list */
                    PushBand(tempAnte.cmd, tempAnte.call2, tempAnte.bndcde, tempAnte.anum);
                }
            }

            DynAntenna.FtCloseAntenna(anteHandle);

            /* for each band in the list, set the bitpos to 1 */

            /* Process each band in the list. For each band get its bit position and
             * set the appropriate bit.
             */
            bandWord[0] = 0;
            while (PopBand(out bndcde) >= 0)
            {
                //		exec sql select bandbitpos into :bandBitPos from sd_band
                //			where bndcde = :bndcde;
                nRet = Suutils.SuGetBand(bndcde, out pBand);
                if (nRet == 0)
                {
                    /* set bit position to 1 */
                    bandBitPos = pBand.bandbitpos;
                    GenUtil.UtSetBit(ref bandWord, (bandBitPos - 1));
                }
                else
                {
                    return (Error.DYN_MS_SQL_SERVER_ERR);
                }

                //freeBand(pBand);
            }

            ftSite.bandwd1 = (int)bandWord[0];
            nArraySite[FtSite.BANDWD1] = Constant.DB_NOT_NULL;
            ftSite.bandwd2 = (int)bandWord[1];
            nArraySite[FtSite.BANDWD2] = Constant.DB_NOT_NULL;
            ftSite.bandwd3 = (int)bandWord[2];
            nArraySite[FtSite.BANDWD3] = Constant.DB_NOT_NULL;
            ftSite.bandwd4 = (int)bandWord[3];
            nArraySite[FtSite.BANDWD4] = Constant.DB_NOT_NULL;
            ftSite.bandwd5 = (int)bandWord[4];
            nArraySite[FtSite.BANDWD5] = Constant.DB_NOT_NULL;
            ftSite.bandwd6 = (int)bandWord[5];
            nArraySite[FtSite.BANDWD6] = Constant.DB_NOT_NULL;
            ftSite.bandwd7 = (int)bandWord[6];
            nArraySite[FtSite.BANDWD7] = Constant.DB_NOT_NULL;
            ftSite.bandwd8 = (int)bandWord[7];
            nArraySite[FtSite.BANDWD8] = Constant.DB_NOT_NULL;

            return (Constant.SUCCESS);

        }   /* ----- End of setBandWord ----- */

        /// <summary>
        /// Determine whether a PDF will leave a site without any antennae.
        /// </summary>
        /// <param name="ftSite"> - the site object.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <returns>true or false.</returns>
        public static bool CheckAnteExist(FtSite ftSite, ref short warnCount, string keyLine, string pdfName)
        {
            string whereClause;
            string mdbClause;
            string tableName;
            int anteHandle,
                mdbHandle,
                rc;
            FtAnte ftAnte;
            SQLLEN[] pdfArrayTemp;  //[FtAnte.SIZE_];
            MtAnte mtAnte;
            SQLLEN[] mdbArrayTemp;  //[MtAnte.SIZE_],
            bool anteExist = false;

            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);

            whereClause = String.Format("cmd != '{0}' and call1 = '{1}'", "D", ftSite.call1);

            if ((anteHandle = DynAntenna.FtSelectAntenna(tableName, whereClause, ""))
                < 0)
            {
                ValErrs.AddMess("Could not read antenna information", keyLine, "W");
                warnCount++;
            }
            else
            {
                rc = DynAntenna.FtFetchAntenna(anteHandle, out ftAnte, out pdfArrayTemp);
                if (rc == Constant.SUCCESS)
                {
                    anteExist = true;
                }
            }

            DynAntenna.FtCloseAntenna(anteHandle);

            if (anteExist == false)
            {
                /* if we know there will be antennas don't bother.  If we come here there are
                     no antennas so we must go to the MDB. */
                mdbClause = String.Format("call1 = '{0}'", ftSite.call1);

                if ((mdbHandle = DynMdbAntenna.MtSelectAntenna(mdbClause, "")) < 0)
                {
                    warnCount++;
                }
                else
                {
                    while ((DynMdbAntenna.MtFetchAntenna(mdbHandle, out mtAnte, out mdbArrayTemp) == Constant.SUCCESS) &&
                           (anteExist != true))
                    {

                        /* loop through each mdb antenna record
                           until we determine that there will be
                           antennas after PDF is run
                        */
                        ValImport.FtFormWhereClause(out whereClause, mtAnte.call1, mtAnte.call2,
                                            mtAnte.bndcde, mtAnte.anum, "");
                        if ((anteHandle = DynAntenna.FtSelectAntenna(tableName, whereClause, "")) < 0)
                        {
                            warnCount++;
                        }
                        else
                        {
                            if (DynAntenna.FtFetchAntenna(anteHandle, out ftAnte, out pdfArrayTemp) == Constant.SUCCESS)
                            {
                                /* Once we get the mdb antenna record we look in the PDF to
                                   find the corresponding rec. If it doesn't exist then
                                   there will be a ante rec	after the PDF is run.  If it
                                   does exist then it will exist	as long as the PDF rec is not
                                   marked as Delete
                                */

                                if (!ftAnte.CmdEquals('D'))
                                {
                                    /* ante will exist */
                                    anteExist = true;
                                }
                            }
                            else
                            {
                                /* ante will exist */
                                anteExist = true;
                            }
                        }
                        DynAntenna.FtCloseAntenna(anteHandle);
                    }
                    DynMdbAntenna.MtCloseAntenna(mdbHandle);
                }
            }
            return anteExist;
        }

        /// <summary>
        /// Performs the site validation for an
        /// ADD record; it verifies that the record does not
        /// exist and calls the field validation routine to
        /// verify that all fields are correct.
        /// </summary>
        /// <param name="ftSite"> - the site object.</param>
        /// <param name="nArrayFW"> - array of ODBC nullInds for ftSite.</param>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        public static void FtValSiteAddTS(FtSite ftSite, SQLLEN[] nArrayFW, string pdfName, ref short errCount, ref short warnCount, string keyLine)
        {
            /* Local variables */
            MtSite mtSite;
            SQLLEN[] nArrayMDB;
            int rc;
            string whereClause;

            /* make sure call1 does not already exist */
            whereClause = String.Format(" call1='{0}'", ftSite.call1);
            rc = ValRetrieveTS.FtValRetrieveMDBSite(out mtSite, whereClause, out nArrayMDB);

            switch (rc)
            {
                case Constant.SUCCESS:
                    ValErrs.AddMess("Site %s already exists in MDB",
                             keyLine, "E", ftSite.call1);
                    errCount++;
                    break;

                case Constant.NOMORERECS:
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Site %s locked in MDB", keyLine, "W", ftSite.call1);
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Site undefined error for %s", keyLine, "W", ftSite.call1);
                    warnCount++;
                    break;
            }

            FtValSiteFieldsTS(ftSite, nArrayFW, ref errCount, ref warnCount, keyLine, pdfName);
            if (SiteCalculations(ftSite, nArrayFW) != Constant.SUCCESS)
            {
                ValErrs.AddMess("Operator Code (%s) not present in SDB",
                                keyLine, "E", ftSite.oper);
                errCount++;
            }

            //if (strncmp(ftSite.call1, "=", 1) == 0)
            if (ftSite.call1.StartsWith("="))
            {
                if (FtRecExist.CheckFictCall(pdfName, ftSite.call1) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("Must have a transmitting frequency if CALL1 begins with '=' (%s)",
                                    keyLine, "E", ftSite.call1);
                    errCount++;
                }
            }
        }   /* ----- End of ftValSiteAddTS ----- */

        /// <summary>
        /// This method reads all the site records in the PDF, determines if each record is being added,
        /// modified or deleted, and validates accordingly.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        public static void FtValSiteTS(string pdfName, ref short errCount, ref short warnCount)
        {
            //...Log2.v("\n\nValSite.FtValSiteTS(): Entry");

            string Scall;   /* Site call1 value */
            string whereClause = "";
            string SQLselect;

            /* Local variables */
            SQLLEN[] nArrayFW;
            short passcheck;    /* check if passive antenna criteria are met */
            int sID1;
            int acodesize;  /* size of antenna code field */
            int anteHandle; /* antenna handle for PDF */
            string tableName;
            string keyLine;
            string titleLine = "VALIDATION AGAINST SDB";
            FtChng ftChng;
            FtAnte tempAnte;
            FtSite ftSite;
            SQLLEN[] nullIndAnte;

            int nRet;
            int nInd;
            MtSiteStr pmtSite;
            MtSiteStrNulls pmtNull;
            string cCallFound = "";

            GenUtil.UtCvtName(Constant.FT_SITE, pdfName, out tableName);

            /* read site information */
            if ((sID1 = DynSite.FtSelectSite(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not select any sites (reason: %d)", tableName, "W", sID1.ToString());
                warnCount++;

                return;
            }

            // Loop around, fetching user ftSite records one, by one.
            while (DynSite.FtFetchSite(sID1, out ftSite, out nArrayFW) == Constant.SUCCESS)
            {
                // Bug fix: b150722A
                // If the ftSite has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftSite to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftSite.cmd))
                {
                    continue;
                }

                keyLine = "Site Call: " + ftSite.call1;

                char cmdChar = 'z';  // Initialize cmdChar to a char that is not A, B, D, N or U.

                // If the site record's command letter has been set the use it.
                if (ftSite.cmd != null && ftSite.cmd.Length > 0)
                {
                    cmdChar = ftSite.cmd[0];
                }

                switch (cmdChar)
                {
                    case 'A':
                        FtValSiteAddTS(ftSite, nArrayFW, pdfName, ref errCount, ref warnCount, keyLine);
                        if (SetBandWord(pdfName, ref ftSite, ref nArrayFW) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("Could not set bandword", keyLine, "W");
                            warnCount++;
                        }

                        /*
                         * TASK 492: We check here that antennas exist for the current
                         * site. If they don't, tell the user. If Antennas are missing,
                         * the channels must be missing as well, or their presence
                         * without antennas will be caught by another message.
                         */
                        if (!CheckAnteExist(ftSite, ref warnCount, keyLine, pdfName))
                        {
                            //...Log2.v("\nValSite.FtValSiteTS(): case 'A': A site must have at least one antenna and one channel.");
                            ValErrs.AddMess("A site must have at least one antenna and one channel.", keyLine, "E");
                            errCount++;
                        }

                        /*
                         * TASK 491: This checks that there are no sites in the database
                         * that have the same Lat and Long as the one we are adding.
                         */
                        MdbLatLong(ftSite.call1, ftSite.latit, ftSite.longit, keyLine, titleLine, ref warnCount);
                        break;

                    case 'D':
                        FtValSiteDelTS(ftSite, pdfName, ref errCount, ref warnCount, keyLine);
                        break;

                    case 'B':
                    case 'U':
                        FtValSiteUpdtTS(ftSite, nArrayFW, pdfName, ref errCount, ref warnCount, out keyLine);
                        if (SetBandWord(pdfName, ref ftSite, ref nArrayFW) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("Could not set bandword", keyLine, "W");
                            warnCount++;
                        }
                        /*
                         * TASK 492: We check here that antennas exist for the current
                         * site. If they don't, tell the user. If Antennas are missing,
                         * the channels must be missing as well, or their presence
                         * without antennas will be caught by another message.
                         */
                        if (!CheckAnteExist(ftSite, ref warnCount, keyLine, pdfName))
                        {
                            //...Log2.v("\nValSite.FtValSiteTS(): case 'B' or 'U': A site must have at least one antenna and one channel.");
                            ValErrs.AddMess("A site must have at least one antenna and one channel.", keyLine, "E");
                            errCount++;
                        }
                        /*
                         * TASK 491: This checks that there are no sites in the database
                         * that have the same Lat and Long as the one we are adding.
                         */
                        MdbLatLong(ftSite.call1, ftSite.latit, ftSite.longit, keyLine, titleLine, ref warnCount);
                        break;

                    case 'N':
                        /* Ensure that the site exists in the MDB */
                        //	Get the antennas as well, we will need them later.
                        nRet = MtUtils.MtGetSiteWN(ftSite.call1, out pmtSite, 2, out pmtNull);
                        if (nRet != 0)
                        {
                            /* Site does not exist */
                            keyLine = String.Format("KEY: {0}\r\n", ftSite.call1);
                            ValErrs.AddMess("Site does not exist in MDB", keyLine, "E");
                            errCount++;
                            break;
                        }

                        /* Recalculate the band word */
                        if (SetBandWord(pdfName, ref ftSite, ref nArrayFW) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("Could not set bandword", keyLine, "W");
                            warnCount++;
                        }

                        /* If band word changed, then must change this
                         * record cmd to 'U'pdate so bandword is changed
                         * in MDB
                        */
                        if ((ftSite.bandwd1 != pmtSite.stSite.bandwd1)
                        || (ftSite.bandwd2 != pmtSite.stSite.bandwd2)
                        || (ftSite.bandwd3 != pmtSite.stSite.bandwd3)
                        || (ftSite.bandwd4 != pmtSite.stSite.bandwd4)
                        || (ftSite.bandwd5 != pmtSite.stSite.bandwd5)
                        || (ftSite.bandwd6 != pmtSite.stSite.bandwd6)
                        || (ftSite.bandwd7 != pmtSite.stSite.bandwd7)
                        || (ftSite.bandwd8 != pmtSite.stSite.bandwd8))
                        {

                            /* Set cmd on site rec to 'U' */
                            ftSite.cmd = "U";
                            nArrayFW[FtSite.CMD] = Constant.DB_NOT_NULL;
                            /* Perform 'U'pdate validation
                             * - just to be sure and to get the
                             * cmd value written out to the pdf */
                            FtValSiteUpdtTS(ftSite, nArrayFW, pdfName, ref errCount, ref warnCount, out keyLine);
                        }

                        /* check to be sure ficticious call sign is legal */
                        if (ftSite.call1[0] == '=')
                        {
                            if (FtRecExist.CheckFictCall(pdfName, ftSite.call1) != Constant.SUCCESS)
                            {
                                ValErrs.AddMess("A site that begins with '=' must have a TX frequency (%s)",
                                                keyLine, "E", ftSite.call1);
                                errCount++;
                            }
                        }

                        /* another check to be sure that if a site is a
                           passive site (begins with %) all, antennas
                           associated with that site have antenna codes
                           that end with a '%' */

                        if (ftSite.call1[0] == '%')
                        {
                            /* check call sign */
                            passcheck = Constant.SUCCESS; /* OK by default */
                            Scall = ftSite.call1;

                            //	Go through the antennas in the mdb file...
                            for (nInd = 0; nInd < pmtSite.nNumAnts; nInd++)
                            {
                                string acode;
                                int acodesize_;
                                acode = pmtSite.stAntsPtr[nInd].acode;
                                acode = acode.Trim();
                                acodesize_ = acode.Length;
                                if (acode[acodesize_ - 1] != '%')
                                {
                                    passcheck = Constant.FAILURE;
                                    break;
                                }
                            }

                            /* do a similar check within the PDF */

                            whereClause = String.Format(" call1 = '{0}'", Scall);
                            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);

                            if ((anteHandle = DynAntenna.FtSelectAntenna(tableName, whereClause, "")) < 0)
                            {
                                ValErrs.AddMess("Could not read antenna information.", keyLine, "W");
                                warnCount++;
                                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                                return;
                            }
                            else
                            {
                                while (DynAntenna.FtFetchAntenna(anteHandle, out tempAnte, out nullIndAnte) == Constant.SUCCESS)
                                {
                                    if ((nullIndAnte[FtAnte.ACODE] != Constant.DB_NULL) && !tempAnte.CmdEquals('D'))
                                    {
                                        /* for all cases where there is an acode and the antenna is
                                        not being deleted, follow the same procedure as was done
                                        above for the MDB */

                                        tempAnte.acode = tempAnte.acode.Trim();
                                        acodesize = tempAnte.acode.Length;
                                        if (tempAnte.acode[acodesize - 1] != '%')
                                        {
                                            passcheck = Constant.FAILURE;
                                        }
                                    }
                                }
                            }

                            DynAntenna.FtCloseAntenna(anteHandle);

                            if (passcheck != Constant.SUCCESS)
                            {
                                /* check to see if the call sign is being changed to a non-passive
                                 * value.  If it is, then consider this antenna number as being OK */

                                GenUtil.UtCvtName(Constant.FT_CHNG_CALL, pdfName, out tableName);
                                whereClause = String.Format(" oldcall1='{0}'", ftSite.call1);

                                /* select change call sign records from PDF */
                                if ((sID1 = DynChange.FtSelectChngCall(tableName, whereClause, "")) >= 0)
                                {
                                    /* Read all change call sign records for the site */
                                    while (DynChange.FtFetchChngCall(sID1, out ftChng, out nArrayFW) == Constant.SUCCESS)
                                    {
                                        /* if the call sign is being changed to anything other
                                         * than a '%' then it is OK */
                                        if (ftChng.newcall1[0] != '%')
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            ValErrs.AddMess("If site is a passive repeater all antenna codes\r\nfor that site must end with percent sign (%s)", keyLine, "E", ftSite.call1);
                                            errCount++;
                                        }
                                    }
                                }
                                DynChange.FtCloseChngCall(sID1);
                            }
                        }
                        /*
                         * TASK 492: We check here that antennas exist for the current
                         * site. If they don't, tell the user. If Antennas are missing,
                         * the channels must be missing as well, or their presence
                         * without antennas will be caught by another message.
                         */
                        if (!CheckAnteExist(ftSite, ref warnCount, keyLine, pdfName))
                        {
                            //...Log2.v("\nValSite.FtValSiteTS(): case 'N': A site must have at least one antenna and one channel.");
                            ValErrs.AddMess("A site must have at least one antenna and one channel.", keyLine, "E");
                            errCount++;
                        }

                        break;

                    default:
                        // Bug fix: b150722A.
                        // The ftSite's command was validated in a prior call to FtValSiteTSIntra() in 
                        // FtValidate.Main(). Consequently, we don't need to validate it again.
                        break;
                }

                DynSite.FtUpdateSite(sID1, ftSite, nArrayFW);

                /* Look for duplicate site coordinates in the pdf */

                GenUtil.UtCvtName(Constant.FT_SITE, pdfName, out tableName);

                /*
                 * TASK 491: This checks that there are no sites in the PDF
                 * that have the same Lat and Long as the current one.
                 */
                SQLselect = String.Format(" latit = {0} and longit = {1} and call1 != '{2}'",
                                                ftSite.latit, ftSite.longit, ftSite.call1);
                nRet = FtUtils.FtEnumSite(SQLselect, pdfName, ref cCallFound);
                if (nRet == 0)
                {
                    /* Lat & Long for at least two sites is the same ! */
                    keyLine = String.Format("Site call: {0}", ftSite.call1);
                    ValErrs.AddMess("Lat & Long for site is the same as for PDF site: %s",
                                                keyLine, "W", cCallFound);
                    warnCount++;
                }

            } // End of while loop over fetched ftSite records.

            DynSite.FtCloseSite(sID1);

            //...Log2.v("\n\nValSite.FtValSiteTS(): Exit");
        }   /* ----- End of ftValSiteTS ----- */

        /// <summary>
        /// Performs all calculations needed for the
        /// site record; it receives a structure, and fills it out with
        /// the new values.
        /// </summary>
        /// <param name="siteRec"> - the site object.</param>
        /// <param name="nArrayFW"> - array of ODBC nullInds for siteRec.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int SiteCalculations(FtSite siteRec, SQLLEN[] nArrayFW)
        {
            SuOper sctOper;
            int nRet;

            /* Local variables */
            string oprTyp;

            nRet = Suutils.SuGetOper(siteRec.oper, out sctOper);

            /*	The oprtyp field is computed from the oper type code and the
            *		type of file ("T" in this case). */
            if (nRet == 0)
            {
                oprTyp = sctOper.opnote[0].ToString();
                oprTyp += "T";
            }
            else
            {
                return (Constant.FAILURE);
            }


            siteRec.oprtyp = oprTyp;
            nArrayFW[FtSite.OPRTYP] = Constant.DB_NOT_NULL;

            return (Constant.SUCCESS);

        }   /* ----- End of siteCalculations ----- */

        /// <summary>
        /// For efficiency, AnumBand objects are stored in a stack; this method initializes the
        /// 'top of stack' counter.
        /// </summary>

        /// <summary>
        /// Initialize the stack.  
        /// </summary>
        /// <param name=""></param>
        private static void InitBandStack()
        {
            topOfStack = 0;         /* stack is empty */
        }

        /// <summary>
        /// For efficiency, AnumBand objects are stored in a stack; this method 'pushes'
        /// (stores) an AnumBand object onto the top of the stack.
        /// </summary>
        /// <remarks>
        /// The stack does not allow duplicates
        /// of call2, band and anum.  This is used to handle deletes.  It is assumed
        /// that deletes will come in from the pdf and that the pdf is registered last
        /// so that any antenna being deleted will have a delete sign.  The band
        /// from these antennae are ignored in popping.
        /// </remarks>
        /// <param name="cmd"> - command letter in PDF.</param>
        /// <param name="call2"> - call sign.</param>
        /// <param name="bndcde"> - band code.</param>
        /// <param name="anum"> - antenna number.</param>
        private static void PushBand(string cmd, string call2, string bndcde, short anum)
        {
            string tmpBndcde;  //[Constant.BANDCDE_SZ];     /* workspace for bandcode */
            int i;                  /* loop control */

            if (topOfStack >= Constant.MAX_BAND_CODES)
            {
                /* List is full */
                return;
            }

            /* Trim all leading and railing spaces. */
            tmpBndcde = bndcde.Trim();

            /* Check for duplicates. Is bandcode and anum already in the stack? */
            for (i = 0; i < topOfStack; i++)
            {
                //if (strcmp(call2, bandStack[i].call2) == 0
                //     && strcmp(bandStack[i].bndcde, tmpBndcde) == 0
                //     && bandStack[i].anum == anum)
                if (call2.Equals(bandStack[i].call2)
                        && bandStack[i].bndcde.Equals(tmpBndcde)
                        && bandStack[i].anum.Equals(anum))
                {
                    /*  This code is set up for changes to be made if necessary */
                    if (cmd == "D")
                    {
                        /*  Indicate this is a delete */
                        bandStack[i].cmd = cmd;
                    }
                    else
                    {
                        if (cmd != "N")
                        {
                            /*  Indicate that it is anything but an N */
                            bandStack[i].cmd = cmd;
                        }
                    }
                    return;
                }
            }

            /* Come here if entry not in the stack. Add to the stack. */
            bandStack[topOfStack] = new AnumBand();

            bandStack[topOfStack].cmd = cmd;
            bandStack[topOfStack].call2 = call2;
            bandStack[topOfStack].bndcde = tmpBndcde;
            bandStack[topOfStack].anum = anum;

            topOfStack++;

            return;

        }   /* ----- End of pushBand ----- */

        /// <summary>
        /// For efficiency, AnumBand objects are stored in a stack; this method 'pops'
        /// (retrieves) an AnumBand object from the top of the stack. If cmd == 'D' 
        /// then get the next item on the stack.
        /// </summary>
        /// <param name="bndcde"> - band code.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        private static int PopBand(out string bndcde)
        {
            // Satisfy 'out' requirement.
            bndcde = "";

            if (topOfStack <= 0)
            {
                /* Stack is empty */
                return (-1);
            }

            --topOfStack;

            if (bandStack[topOfStack].cmd != null && bandStack[topOfStack].cmd.Length > 0)
            {
                if (bandStack[topOfStack].cmd[0] == 'D')
                {
                    /* ignore 'D'eleted records */
                    return (PopBand(out bndcde));
                }
            }

            /* Pop the element off the stack */
            bndcde = bandStack[topOfStack].bndcde;

            return (0);
        }   /* ----- End of popBand ----- */

        /// <summary>
        /// Validates all the individual fields on
        /// a TS site record; this validation is done
        /// for both an add and an update.
        /// </summary>
        /// <param name="ftSite"> - the site object.</param>
        /// <param name="nArrayFW">  - array of ODBC nullInds for ftSite.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        /// <param name="shortName"> - name of the PDF file.</param>
        public static void FtValSiteFieldsTS(FtSite ftSite, /* site info */
                                                            SQLLEN[] nArrayFW,      /* array of nulls */
                                                            ref short errCount,     /* number of errors */
                                                            ref short warnCount,        /* number of warns */
                                                            string keyLine,         /* keys to print */
                                                            string shortName)
        {
            //...Log2.v("\n\nValSite.FtValSiteFieldsTS(): Entry: call1 = " + ftSite.call1);

            /* Local MS SQL Server aware variables */
            string Scall = "";
            string siteOper;

            FtAnte tempAnte;
            FtChng ftChng;
            SQLLEN[] nullIndAnte;
            MtAnte mtAnte;
            SQLLEN[] nullMDBAnte;
            string whereClause;
            string tableName;
            char fictcall;    /* first stringacter of the call sign */
            string callroot;    /* first six+ stringacters of the callsign */
            string operroot;    /* first six stringacters of the oper code */
            short passcheck = Constant.SUCCESS; /* passive antenna check */
            short opersize; /* length of operator field */
            int anteHandle; /* antenna handle */
            int acodesize;  /* size of antenna code values */
            int sID1;
            bool IsLLEntered = false;

            ftSite.oper = ftSite.oper.Trim();
            ftSite.call1 = ftSite.call1.Trim();

            opersize = (short)ftSite.oper.Length;

            if (opersize > 0 && ftSite.oper.Equals(ftSite.call1))
            {
                ValErrs.AddMess("Call Sign (%s) begins with Site Record operator code.\r\nFollow Ficticious Call Sign Rules.",
                                            keyLine, "E", ftSite.call1);
                errCount++;
            }

            if (nArrayFW[FtSite.NAME] == Constant.DB_NULL)
            {
                /* Must enter data in fld. */
                ValErrs.AddMess("Must Enter Name field.", keyLine, "E");
                errCount++;
            }

            if (nArrayFW[FtSite.PROV] == Constant.DB_NULL)
            {
                /* Must enter data in fld. */
                ValErrs.AddMess("Must Enter Province field.", keyLine, "E");
                errCount++;
            }

            IsLLEntered = true;
            if (nArrayFW[FtSite.LATIT] == Constant.DB_NULL)
            {
                /* Must enter data in fld. */
                ValErrs.AddMess("Must Enter Latitude field.", keyLine, "E");
                errCount++;
                IsLLEntered = false;
            }

            if (nArrayFW[FtSite.LONGIT] == Constant.DB_NULL)
            {
                /* Must enter data in field */
                ValErrs.AddMess("Must Enter Longitude field.", keyLine, "E");
                errCount++;
                IsLLEntered = false;
            }

            /*	Check that, if the L/L has been entered, that the coords are valid
            *		GJS - 1209 - 2006.07.27 */
            if (IsLLEntered)
            {
                if (!ValidLatLong(ftSite.latit, ftSite.longit))
                {
                    /*	Not in our area.  */
                    ValErrs.AddMess("Lat/Long outside our area.", keyLine, "W");
                    warnCount++;
                }
            }

            if (nArrayFW[FtSite.GRND] == Constant.DB_NULL)
            {
                /* Must enter data in field */
                ValErrs.AddMess("Must Enter Ground Height field.", keyLine, "E");
                errCount++;
            }

            if (nArrayFW[FtSite.STATS] == Constant.DB_NULL)
            {
                /* Must enter data in field */
                ValErrs.AddMess("Must Enter Status field.", keyLine, "E");
                errCount++;
            }

            siteOper = ftSite.oper;
            if (nArrayFW[FtSite.NOTS] != Constant.DB_NULL)
            {
                SuNote tNote;
                /* validate nots against snote table (nonum) */
                if (Suutils.SuGetNote(siteOper, ftSite.nots, out tNote) != 0)
                {
                    ValErrs.AddMess("Nots not present in SDB for Operator :%s:",
                              keyLine, "E", ftSite.oper);
                    errCount++;
                }
            }

            if ((nArrayFW[FtSite.OPER] != Constant.DB_NULL) && (siteOper.Length > 0))
            {
                /* validate oper against soper table (oper) */
                SuOper tOper;
                int nRet;

                nRet = Suutils.SuGetOper(siteOper, out tOper);

                if (nRet != 0)
                {
                    ValErrs.AddMess("Operator Code (%s) not present in SDB",
                                    keyLine, "E", ftSite.oper);
                    errCount++;
                }
                else if ((tOper.opnote[1] != 'C') && (tOper.opnote[1] != 'F'))
                {
                    ValErrs.AddMess("Operator (%s) is not a coordinating member.",
                                    keyLine, "E", ftSite.oper);
                    errCount++;
                }

                fictcall = ftSite.call1[0];
                if ((fictcall == '=') ||
                    (fictcall == '$') ||
                    (fictcall == ';') ||
                     (fictcall == '%'))
                {
                    ftSite.oper = ftSite.oper.Trim();
                    opersize = (short)ftSite.oper.Length;
                    if (opersize > 6)
                    {
                        opersize = 6;
                    }
                    operroot = ftSite.oper.Substring(0, opersize);
                    callroot = ftSite.call1.Substring(1, opersize);

                    //...Log2.v(String.Format("\n|{0}, {1}, |{2}|, |{3}|", ftSite.oper, opersize, operroot, callroot));

                    if (!callroot.Equals(operroot))
                    {
                        ValErrs.AddMess("Characters following %c in call1 must agree with oper code field",
                                keyLine, "E", ftSite.call1[0].ToString());
                        errCount++;
                    }
                }

                if (ftSite.call1[0] == '%')
                {
                    /* if site begins with a '%' it is a passive site and all
                         antennas at that site must have acodes that end with '%' */
                    whereClause = String.Format("call1 = '{0}'", Scall);
                    if ((anteHandle = DynMdbAntenna.MtSelectAntenna(whereClause, "")) < 0)
                    {
                        ValErrs.AddMess("WARNING - Could not read MDB antenna information", keyLine, "W");
                        warnCount++;
                        return;
                    }
                    else
                    {
                        while (DynMdbAntenna.MtFetchAntenna(anteHandle, out mtAnte, out nullMDBAnte) == Constant.SUCCESS)
                        {
                            if ((nullMDBAnte[MtAnte.ACODE] != Constant.DB_NULL))
                            {
                                /* 	if acode exists and if the	antenna is not being deleted
                                        perform similar check as was done above for the MDB */
                                mtAnte.acode = mtAnte.acode.Trim();
                                acodesize = mtAnte.acode.Length;
                                if (mtAnte.acode[acodesize - 1] != '%')
                                {
                                    passcheck = Constant.FAILURE;
                                }
                            }
                        }
                    }
                    DynMdbAntenna.MtCloseAntenna(anteHandle);

                    /* perform similar check on all PDF ante records */
                    whereClause = String.Format("call1 = '{0}'", Scall);
                    GenUtil.UtCvtName(Constant.FT_ANTE, shortName, out tableName);

                    if ((anteHandle = DynAntenna.FtSelectAntenna(tableName, whereClause, "")) < 0)
                    {
                        ValErrs.AddMess("WARNING - Could not read antenna information", keyLine, "W");
                        warnCount++;
                        return;
                    }
                    else
                    {
                        while (DynAntenna.FtFetchAntenna(anteHandle, out tempAnte, out nullIndAnte) == Constant.SUCCESS)
                        {
                            if ((nullIndAnte[FtAnte.ACODE] != Constant.DB_NULL) && !tempAnte.CmdEquals('D'))
                            {
                                /* 	if acode exists and if the	antenna is not being deleted
                                        perform similar check as was done above for the MDB */

                                tempAnte.acode = tempAnte.acode.Trim();
                                acodesize = tempAnte.acode.Length;
                                if (tempAnte.acode[acodesize - 1] != '%')
                                {
                                    passcheck = Constant.FAILURE;
                                }
                            }
                        }
                    }
                    DynAntenna.FtCloseAntenna(anteHandle);

                    if (passcheck != Constant.SUCCESS)
                    {
                        /* check to see if the call sign is
                         * being changed to a non-passive
                         * value.  If it is, then consider
                         * this antenna number as being OK */

                        /* set up the Ingres cursor */
                        GenUtil.UtCvtName(Constant.FT_CHNG_CALL, shortName, out tableName);

                        whereClause = String.Format("oldcall1='{0}'", ftSite.call1);

                        /* select change call sign records from PDF*/
                        if ((sID1 = DynChange.FtSelectChngCall(tableName, whereClause, "")) >= 0)
                        {
                            /* Read all change call sign records for the site */
                            while (DynChange.FtFetchChngCall(sID1, out ftChng, out nArrayFW) == Constant.SUCCESS)
                            {
                                /* if the call sign is being changed to anything other
                                 * than a '%' then it is OK */
                                if (ftChng.newcall1[0] != '%')
                                {
                                    continue;
                                }
                                else
                                {
                                    ValErrs.AddMess("If site is a passive repeater all antenna codes\r\nfor that site must end with percent sign",
                                                    keyLine, "E");
                                    errCount++;
                                }
                            }
                        }
                        DynChange.FtCloseChngCall(sID1);
                    }
                }

            }
            else
            {
                /*	No oper code */
                ValErrs.AddMess("Operator Code must be present.", keyLine, "E");
                errCount++;
            }

            //...Log2.v("\n\nValSite.FtValSiteFieldsTS(): Exit");
        }   /* ----- End of ftValSiteFieldsTS ----- */

        /// <summary>
        /// Checks that the latitude and Longitude are possible for Canada and 
        /// the Northern United States.
        /// </summary>
        /// <param name="inLatcS"> - latitude in 1/100th of a second.</param>
        /// <param name="inLongcS"> - longitude in 1/100th of a second.</param>
        /// <returns>true or false.</returns>
        public static bool ValidLatLong(int inLatcS, int inLongcS)
        {
            bool bRet = false;
            if (inLatcS == 0 ||
                    inLongcS == 0 ||
                    inLongcS < 44 * 360000 ||
                    inLongcS > 142 * 360000 ||
                    inLatcS < 40 * 360000 ||
                    inLatcS > 84 * 360000)
            {
                bRet = false;
            }
            else
            {
                if (inLatcS < 60 * 360000)
                {
                    if (inLongcS < 88 * 360000)
                    {
                        bRet = true;
                    }
                    else if (inLatcS > 47 * 360000)
                    {
                        bRet = true;
                    }
                    else
                    {
                        bRet = false;
                    }
                }
                else
                {
                    bRet = true;
                }
            }
            return bRet;
        }

        /// <summary>
        /// Performs the site validation for an UPDATE record; it verifies that 
        /// the record does not already exist and calls the field validation routine 
        /// to verify that all fields are correct.
        /// </summary>
        /// <param name="ftSite"> - the site object.</param>
        /// <param name="nArrayFW">  - array of ODBC nullInds for ftSite.</param>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        public static void FtValSiteUpdtTS(FtSite ftSite,  /* site info */
                                                        SQLLEN[] nArrayFW,               /* array with null flags */
                                                        string pdfName,          /* pdf display name */
                                                        ref short errCount,        /* number of errs encountered */
                                                        ref short warnCount,   /* number of warnings */
                                                        out string keyLine)      /* keys if necesary to print */
        {
            //...Log2.v("\n\nValSite.FtValSiteUpdtTS(): Entry");

            /* Local variables */
            int aID1;
            int rc;
            string whereClause;
            string tableName;
            string chantable = "";
            SQLLEN[] nArrayMDB;
            SQLLEN[] nArrayTemp;
            FtAnte ftAnte;
            MtSite mtSite;
            FtChan ftChan;

            /* make sure site does exist */
            ValImport.FtFormWhereClause(out whereClause, ftSite.call1, "", "", Constant.NO_ANUM, "");
            rc = ValRetrieveTS.FtValRetrieveMDBSite(out mtSite, whereClause, out nArrayMDB);
            keyLine = String.Format("KEY: {0}\r\n", ftSite.call1);

            switch (rc)
            {
                case Constant.SUCCESS:
                    break;

                case Constant.NOMORERECS:
                    ValErrs.AddMess("Site does not exist in MDB", keyLine, "E");
                    errCount++;
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Site record locked in MDB", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Site undefined error", keyLine, "W");
                    warnCount++;
                    break;
            }

            if (rc != Constant.SUCCESS)
            {
                /* if we couldn't properly retrieve this record
             * we don't want to continue validating
               */
                return;
            }

            if (ftSite.call1.StartsWith("="))
            {
                if (FtRecExist.CheckFictCall(pdfName, ftSite.call1) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("Must have a transmitting frequency if Call1 begins with '=' (%s)",
                                    keyLine, "E", ftSite.call1);
                    errCount++;
                }
            }


            /* if lat, long or ground are changing, modify Antenna records */
            /* on remote and local side                                    */
            if ((ftSite.latit != mtSite.latit) ||
                    (ftSite.longit != mtSite.longit) ||
                    (ftSite.grnd != mtSite.grnd))
            {
                ValImport.FtFormWhereClause(out whereClause, ftSite.call1, "", "", Constant.NO_ANUM, "");

                /* read antenna information */
                GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);
                if ((aID1 = DynAntenna.FtSelectAntenna(tableName, whereClause, "")) < 0)
                {
                    ValErrs.AddMess("Could not read antenna information", keyLine, "W");
                    warnCount++;
                }
                else
                {

                    while (DynAntenna.FtFetchAntenna(aID1, out ftAnte, out nArrayTemp) == Constant.SUCCESS)
                    {
                        /* Fix for bug 631.  Only change cmd.'U'
                         * if it is currently 'N' */
                        if (ftAnte.cmd.Equals("N"))
                        {
                            ftAnte.cmd = "U";
                            DynAntenna.FtUpdateAntenna(aID1, ftAnte, nArrayTemp);
                        }
                    }
                    DynAntenna.FtCloseAntenna(aID1);

                    /*  1066 - Fix the channels as well, so that the rx power will be
                    *   recalculated */
                    GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out chantable);
                    if ((aID1 = DynChannel.FtSelectChannel(chantable, whereClause, "")) < 0)
                    {
                        ValErrs.AddMess("Could not read channel information", keyLine, "W");
                        warnCount++;
                    }
                    else
                    {

                        while (DynChannel.FtFetchChannel(aID1, out ftChan, out nArrayTemp) == Constant.SUCCESS)
                        {
                            if (ftChan.cmd.Equals("N"))
                            {
                                ftChan.cmd = "U";
                                DynChannel.FtUpdateChannel(aID1, ftChan, nArrayTemp);
                            }
                        }
                        DynChannel.FtCloseChannel(aID1);
                    }
                }

                /* read remote antenna information */

                ValImport.FtFormWhereClause(out whereClause, "", ftSite.call1, "", Constant.NO_ANUM, "");
                if ((aID1 = DynAntenna.FtSelectAntenna(tableName, whereClause, "")) < 0)
                {
                    ValErrs.AddMess("Could not read antenna information", keyLine, "W");
                    warnCount++;
                }
                else
                {

                    while (DynAntenna.FtFetchAntenna(aID1, out ftAnte, out nArrayTemp) == Constant.SUCCESS)
                    {
                        /* Fix for bug 631.  Only change cmd.'U'
                         * if it is currently 'N' */
                        if (ftAnte.cmd.Equals("N"))
                        {
                            ftAnte.cmd = "U";
                            DynAntenna.FtUpdateAntenna(aID1, ftAnte, nArrayTemp);
                        }
                    }
                    DynAntenna.FtCloseAntenna(aID1);
                    /*  1066 - Fix the channels as well, so that the rx power will be
                    *   recalculated */
                    if ((aID1 = DynChannel.FtSelectChannel(chantable, whereClause, "")) < 0)
                    {
                        ValErrs.AddMess("Could not read channel information", keyLine, "W");
                        warnCount++;
                    }
                    else
                    {
                        while (DynChannel.FtFetchChannel(aID1, out ftChan, out nArrayTemp) == Constant.SUCCESS)
                        {
                            if (ftChan.cmd.Equals("N"))
                            {
                                ftChan.cmd = "U";
                                DynChannel.FtUpdateChannel(aID1, ftChan, nArrayTemp);
                            }
                        }
                        DynChannel.FtCloseChannel(aID1);
                    }
                }
            }

            /* Check modify date and time */
            if (ftSite.mdate.Length > 0 && ftSite.mtime.Length > 0)
            {
                if ((!ftSite.mdate.Equals(mtSite.mdate)) || (!ftSite.mtime.Equals(mtSite.mtime)))
                {
                    /* Modify dates/times don't match */
                    ValErrs.AddMess("This Site record has been modified since you retrieved it.",
                                    keyLine, "W");
                    warnCount++;
                }
            }

            FtValSiteFieldsTS(ftSite, nArrayFW, ref errCount, ref warnCount, keyLine, pdfName);
            if (SiteCalculations(ftSite, nArrayFW) != Constant.SUCCESS)
            {
                ValErrs.AddMess("Operator Code (%s) not present in SDB",
                                keyLine, "E", ftSite.oper);
                errCount++;
            }

            //...Log2.v("\n\nValSite.FtValSiteUpdtTS(): Exit");
        }   /* ----- End of ftValSiteUpdtTS ----- */





    }
}
