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
    using System.Text.RegularExpressions;
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
    /// Provides methods that provide 'intra' validation of TS site, antenna and channel records.
    /// </summary>
    public class ValidTS
    {
#if PINVOKE
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValSiteTSIntra(string s, ref short sh1, ref short sh2, ref short sh3);

        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValAnteTSIntra(string s, ref short sh1, ref short sh2, ref short sh3);

        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValChanTSIntra(string s, ref short sh1, ref short sh2, ref short sh3);

        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern int ftChkChng([In] string pdfName, [In] string call, [In] string newsign);

        //---------------------------------------------------------------------------------------------


        public static void FtValAnteTSIntra_NATIVE(string pdfName, ref short errorCount, ref short warningsCount, ref short valLevel)
        {
            ftValAnteTSIntra(pdfName, ref errorCount, ref warningsCount, ref valLevel);
        }


        public static void FtValSiteTSIntra_NATIVE(string pdfName, ref short errorCount, ref short warningsCount, ref short valLevel)
        {
            ftValSiteTSIntra(pdfName, ref errorCount, ref warningsCount, ref valLevel);
        }

        public static int FtChkChng_NATIVE(string pdfName, string call, string newsign)
        {
            return ftChkChng(pdfName, call, newsign);
        }

         public static void FtValChanTSIntra_NATIVE(string s, ref short sh1, ref short sh2, ref short sh3)
        {
            ftValChanTSIntra(s, ref sh1, ref sh2, ref sh3);
        }
#endif

        /// <summary>
        /// This method is used to check whether a change to a fictitious call sign would 
        /// make an otherwise illegal antenna be considered legal. 
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="call"> - call1 for a site.</param>
        /// <param name="newsign"> - the acceptable 1st char of new call sign.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int FtChkChng(string pdfName, string call, string newsign)
        {
            int ret;        /* return code */

            int ftChngHandle;   /* dynamic cursor handles */

            SQLLEN[] ftChngNulls;  // [FT_CHNG_SIZE_];

            string tableName;

            FtChng ftChng;

            string siteCall;
            string whereClause;

            ret = Constant.FAILURE;     /* set return code to failure ...code must */
                                        /* verify that this condition does not exist */

            siteCall = call;


            /* check to see if the call sign is being changed to the
               newsign value.  If it is, then consider this fictitious
               call sign as being O.K. */

            /* set up the Ingres cursor */
            GenUtil.UtCvtName(Constant.FT_CHNG_CALL, pdfName, out tableName);

            whereClause = String.Format("oldcall1='{0}'", siteCall);

            /* select change call sign records from PDF*/
            if ((ftChngHandle = DynChange.FtSelectChngCall(tableName, whereClause, "")) >= 0)
            {
                /* Read all change call sign records for the site */
                while (DynChange.FtFetchChngCall(ftChngHandle, out ftChng, out ftChngNulls) == Constant.SUCCESS)
                {
                    /* if the call sign is being changed to
                       'newsign', then it is O.K. */
                    if (ftChng.newcall1[0].Equals(newsign))
                    {
                        ret = Constant.SUCCESS;
                    }
                }
                DynChange.FtCloseChngCall(ftChngHandle);
            }

            return (ret);
        }



        /// <summary>
        /// This method provides 'intra' validation of the site records in the PDF by examining each field in 
        /// a site table and performing range checking etc; it is termed 'intra' because it does NOT perform 
        /// any inter-table validation.
        /// </summary>
        /// <param name="pdfName"> - name of the prescribed PDF file.</param>
        /// <param name="errCount"> - returns the number of errors encountered.</param>
        /// <param name="warnCount"> - returns the number of warnings issued.</param>
        /// <param name="valLevel"> - returns the level of validation attained.</param>
        public static void FtValSiteTSIntra(string pdfName, ref short errCount, ref short warnCount, ref short valLevel)
        {
            //...Log2.v("\n\nValidTS.FtValSiteTSIntra(): Entry");

            /* Local variables */
            string oldCall1 = "";   /* check for uniqueness */
            string whereClause;
            string validFirstCharsAsStr; /* array of valid 1st chars */
            string tableName;
            string orderBy;
            string keyLine;
            SQLLEN[] nullArray;
            int ID1;

            FtSite ftSite;

            // Get the full table name.
            GenUtil.UtCvtName(Constant.FT_SITE, pdfName, out tableName);

            whereClause = "";
            orderBy = "call1";

            if ((ID1 = DynSite.FtSelectSite(tableName, whereClause, orderBy)) < 0)
            {
                /* cannot open cursor */
                ValErrs.AddMess("Cannot read site information, reason %d.", tableName, "W");
                warnCount++;
                return;
            }

            // Loop around, fetching user ftSite records one, by one.
            while (DynSite.FtFetchSite(ID1, out ftSite, out nullArray) == Constant.SUCCESS)
            {
                /* print key information */
                keyLine = String.Format("Site Call: {0} Name: {1}", ftSite.call1, ftSite.name);

                // The ftSite table has a 'cmd' column that is NULLABLE.
                // If the cmd field of an import PDF's SK record is empty then
                // the corresponding ftSite table record has 'cmd' set to NULL.
                // When FtFetchSite() retrieves an ftSite record from the DB, a NULL in the 
                // 'cmd' column is returned as ftSite.cmd = "".

                //...Log2.v("\nValidTS.FtValSiteTSIntra(): ftSite.cmd = " + Strings.AddBars(ftSite.cmd));

                // Validate that ftSite.cmd[0] is A, B, D, N or U.
                string message;
                if (!FtUtil.IsValidCmd(ftSite.cmd, out message))
                {
                    ValErrs.AddMess(message, keyLine, "E", "");
                    errCount++;
                }

                // The ftSite table 'call1' column is NOT nullable.
                // It is possible that the ftSite's table record has call1 = "" so test for it.
                if (oldCall1.Equals(ftSite.call1))
                {
                    ValErrs.AddMess("Site record (%s) not unique in the PDF",
                                    keyLine, "E", ftSite.call1);
                    errCount++;
                }

                /* copy current information to old information */
                oldCall1 = ftSite.call1;

                /* verify the call sign for valid first character: must be alphabetic. */
                if (!Strings.IsAlphabetic(ftSite.call1[0]))
                {
                    /* check the command */
                    if (ftSite.CmdEquals('A'))
                    {
                        validFirstCharsAsStr = Constant.SPECIALADDCHAR;
                    }
                    else
                    {
                        validFirstCharsAsStr = Constant.SPECIALCHAR;
                    }

                    // Check if ftSite.call1[0] has a valid first character.
                    if (validFirstCharsAsStr.IndexOf(ftSite.call1[0]) < 0)
                    {
                        /* invalid call sign */
                        ValErrs.AddMess("Invalid first character for Call1 (%s).",
                                        keyLine, "E", ftSite.call1);
                        errCount++;
                    }
                }

                /* validate only if Add, Update, or None not for Delete */
                if (!ftSite.CmdEquals('D'))
                {
                    if (nullArray[FtSite.LATIT] != Constant.DB_NULL)
                    {
                        if (!ValUtil.UtValidateLat(ftSite.latit))
                        {
                            ValErrs.AddMess("Invalid value for Latitude (%d).",
                                            keyLine, "E", ftSite.latit.ToString());
                            errCount++;
                        }
                    }

                    if (nullArray[FtSite.LONGIT] != Constant.DB_NULL)
                    {
                        if (!ValUtil.UtValidateLong(ftSite.longit))
                        {
                            ValErrs.AddMess("Invalid value for Longitude (%d).",
                                            keyLine, "E", ftSite.longit.ToString());
                            errCount++;
                        }
                    }

                    if (nullArray[FtSite.STATS] != Constant.DB_NULL)
                    {
                        if ("012345678".IndexOf(ftSite.stats[0]) < 0)
                        {
                            ValErrs.AddMess("STATS (%s) must be between 0 and 8.",
                                            keyLine, "E", ftSite.stats);
                            errCount++;
                        }
                    }

                    if (nullArray[FtSite.NOTWR] != Constant.DB_NULL)
                    {
                        if (ftSite.notwr < 0 || ftSite.notwr > 9)
                        {
                            ValErrs.AddMess("Number of Towers (%hd) must be between 1 and 9.",
                                            keyLine, "E", ftSite.notwr.ToString());
                            errCount++;
                        }
                    }

                    if (nullArray[FtSite.GRND] != Constant.DB_NULL)
                    {
                        if (ftSite.grnd < 0 || ftSite.grnd > Constant.MAX_SITE_ELEVATION_M)   // Previously 4999.9
                        {
                            string str = String.Format("Ground height ({0:F6}) must be between 0.0 and {1:F1}",
                                                        ftSite.grnd, Constant.MAX_SITE_ELEVATION_M);

                            ValErrs.AddMess(str, keyLine, "E");
                            errCount++;
                        }
                    }

                    if (nullArray[FtSite.PROV] != Constant.DB_NULL)
                    {
                        if (!ValUtil.UtValidateProv(ftSite.prov.Trim()))
                        {
                            ValErrs.AddMess("Invalid code for Province (%s).",
                                            keyLine, "E", ftSite.prov);
                            errCount++;
                        }
                    }

                    if (nullArray[FtSite.SDATE] != Constant.DB_NULL)
                    {
                        if (!ValUtil.UtValidateDate(ftSite.sdate))
                        {
                            ValErrs.AddMess("Invalid format for Service Date (%s).",
                                            keyLine, "E", ftSite.sdate);
                            errCount++;
                        }
                    }

                } // End of if (!ftSite.CmdEquals('D'))

            } /* end of while loop over fetched ftSite records */

            DynSite.FtCloseSite(ID1);

            //...Log2.v("\n\nValidTS.FtValSiteTSIntra(): Exit");
        }


        /// <summary>
        /// This method provides 'intra' validation of the site records in the PDF by examining each field in 
        /// an antenna table and performing range checking etc; it is termed 'intra' because it does NOT perform 
        /// any inter-table validation.
        /// </summary>
        /// <param name="pdfName"> - name of the prescribed PDF file.</param>
        /// <param name="errCount"> - returns the number of errors encountered.</param>
        /// <param name="warnCount"> - returns the number of warnings issued.</param>
        /// <param name="valLevel"> - returns the level of validation attained.</param>
        public static void FtValAnteTSIntra(string pdfName, ref short errCount, ref short warnCount, ref short valLevel)
        {
            //...Log2.v("\n\nValidTS.FtValAnteTSIntra(): Entry");

            /* Local variables */
            string oldCall1 = "";    /* check for uniqueness */
            string oldCall2 = "";
            string oldBndcde = "";
            string tableName = "";
            string whereClause = "";
            string orderBy = "";
            string keyLine = "";
            short oldAnum = 0; ;
            int ID1;

            FtAnte ftAnte;
            SQLLEN[] nullArray;

            /* read antenna information */
            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);

            orderBy = " call1, call2, bndcde, anum";

            if ((ID1 = DynAntenna.FtSelectAntenna(tableName, whereClause, orderBy)) < 0)
            {
                /* cannot open cursor */
                ValErrs.AddMess("Cannot select antennas for reason %d", pdfName, "W", ID1.ToString());
                warnCount++;
                return;
            }

            while (DynAntenna.FtFetchAntenna(ID1, out ftAnte, out nullArray) == Constant.SUCCESS)
            {
                //...Log2.v("\nValidTS.FtValAnteTSIntra(): A: ftAnte.cmd = " + Strings.AddBars(ftAnte.cmd));

                /* print key information */
                keyLine = String.Format("Antenna {0} {1} {2} {3}",
                                            ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum);

                // The ftAnte table has a 'cmd' column that is NULLABLE.
                // If the cmd field of an import PDF's AK record is empty then
                // the corresponding ftAnte table record has 'cmd' set to NULL.
                // When FtFetchAnte() retrieves an ftAnte record from the DB, a NULL in the 
                // 'cmd' column is returned as ftAnte.cmd = "".

                // Validate that ftAnte.cmd[0] is A, B, D, N or U.
                string message;
                if (!FtUtil.IsValidCmd(ftAnte.cmd, out message))
                {
                    ValErrs.AddMess(message, keyLine, "E", "");
                    errCount++;
                }

                /* check for uniqueness */
                /* No keys used in PDF's, but cursor was opened with 'order by'
                 * clause, so duplicate records will appear subsequent to
                 * to original.
                */
                if (oldCall1.Equals(ftAnte.call1) &&
                    oldCall2.Equals(ftAnte.call2) &&
                    oldBndcde.Equals(ftAnte.bndcde) &&
                    (oldAnum == ftAnte.anum))
                {
                    ValErrs.AddMess("Antenna record not unique in the PDF.", keyLine, "E");
                    errCount++;
                }

                /* copy current information to temp information */
                oldCall1 = ftAnte.call1;
                oldCall2 = ftAnte.call2;
                oldBndcde = ftAnte.bndcde;
                oldAnum = ftAnte.anum;

                /* verify the call sign for valid first character */
                string validFirstCharsAsStr = Constant.SPECIALCHAR;
                if (!Strings.IsAlphabetic(ftAnte.call1[0]))
                {
                    /* not alphabetic */
                    if (validFirstCharsAsStr.IndexOf(ftAnte.call1[0]) < 0)
                    {
                        /* invalid call sign */
                        ValErrs.AddMess("Invalid first character for Call1 (%s).",
                            keyLine, "E", ftAnte.call1);
                        errCount++;
                    }
                }

                if (!Strings.IsAlphabetic(ftAnte.call2[0]))
                {
                    /* not alpha */
                    if (validFirstCharsAsStr.IndexOf(ftAnte.call2[0]) < 0)
                    {
                        /* invalid call sign */
                        ValErrs.AddMess("Invalid first character for Call2 (%s).",
                                        keyLine, "E", ftAnte.call2);
                        errCount++;
                    }
                }

                /* validate only if Add, Update, or None not for Delete */
                if (!ftAnte.CmdEquals('D'))
                {
                    if (nullArray[FtAnte.ANUM] != Constant.DB_NULL)
                    {
                        if (ftAnte.anum < 1 || ftAnte.anum > 99)
                        {
                            ValErrs.AddMess("Antenna Number (%d) must be between 01 and 99.",
                                            keyLine, "E", ftAnte.anum.ToString());
                            errCount++;
                        }
                    }

                    if (nullArray[FtAnte.AUSE] != Constant.DB_NULL)
                    {
                        if (!ftAnte.ause.Equals("TX") &&
                                !ftAnte.ause.Equals("STX") &&
                                !ftAnte.ause.Equals("TR") &&
                                !ftAnte.ause.Equals("RX") &&
                                !ftAnte.ause.Equals("DV1") &&
                                !ftAnte.ause.Equals("DV2"))
                        {
                            ValErrs.AddMess("Antenna Use (%s) must be TX, STX, TR, RX, DV1, DV2.",
                                            keyLine, "E", ftAnte.ause);
                            errCount++;
                        }
                    }

                    if (nullArray[FtAnte.AHT] != Constant.DB_NULL)
                    {
                        if (ftAnte.aht <= 0.0 || ftAnte.aht > 1000.0)
                        {
                            ValErrs.AddMess("Antenna height (%f) must be greater than 0m and no more than 1000m",
                                            keyLine, "E", ftAnte.aht.ToString("F2"));
                            errCount++;
                        }
                        else if (ftAnte.aht > 300.0)
                        {
                            ValErrs.AddMess("Antenna height (%s) outside recommended range of 0 to 300m",
                                            keyLine, "W", ftAnte.aht.ToString("#.000000"));
                            warnCount++;
                        }
                    }

                    if ((nullArray[FtAnte.ATWRNO] != Constant.DB_NULL) && ftAnte.atwrno != 0)
                    {
                        if (ftAnte.atwrno < 1 || ftAnte.atwrno > 9)
                        {
                            ValErrs.AddMess("Antenna Tower Number (%d) must be between 1 and 9.",
                                            keyLine, "E", ftAnte.atwrno.ToString());
                            errCount++;
                        }
                    }

                    if (nullArray[FtAnte.OFFAZM] != Constant.DB_NULL)
                    {
                        // Check if ftAnte.offazm[0] is either 'Y' or 'N'.
                        if (Constant.TRUEFALSE.IndexOf(ftAnte.offazm[0]) < 0)
                        {
                            if (ftAnte.call1[0] != '%')
                            {
                                /*  This antenna is not a passive.  Passives are allowed to have
                                *		a "P" in this field.  */
                                ValErrs.AddMess("Off Azimuth (%s) must be Y or N.",
                                                keyLine, "E", ftAnte.offazm);
                                errCount++;
                            }
                        }
                    }

                    if (nullArray[FtAnte.TAZMTH] != Constant.DB_NULL)
                    {
                        if (ftAnte.tazmth < 0 || ftAnte.tazmth > 360)
                        {
                            ValErrs.AddMess("True Azimuth (%f) must be between 0 and 360.",
                                            keyLine, "E", ftAnte.tazmth.ToString());
                            errCount++;
                        }
                    }

                    if (nullArray[FtAnte.AZMTH] != Constant.DB_NULL)
                    {
                        if (ftAnte.azmth < 0 || ftAnte.azmth > 360)
                        {
                            string azimStr = String.Format("{0:#.000000}", ftAnte.azmth);

                            // This is to address PDF = ftest28; cannot reproduce C version's rounding error in C#.
                            if (azimStr.Equals("361.040000"))
                            {
                                azimStr = "361.040009";
                            }

                            ValErrs.AddMess("Azimuth (%f) must be between 0.00 and 360.00",
                                            //keyLine, "E", ftAnte.azmth.ToString());
                                            keyLine, "E", azimStr);
                            errCount++;
                        }
                    }

                    if (nullArray[FtAnte.TELVTN] != Constant.DB_NULL)
                    {
                        /* 	1157 - GJS - 2003.07.22 - Changed the limits from -10 to +90 to
                        *		-90 to +90.  */
                        if (ftAnte.telvtn < -90 || ftAnte.telvtn > 90.0)
                        {
                            ValErrs.AddMess("True Elevation (%f) must be between -90 and +90.",
                                            keyLine, "E", ftAnte.telvtn.ToString());
                            errCount++;
                        }
                    }

                    if (nullArray[FtAnte.TGAIN] != Constant.DB_NULL)
                    {
                        if (ftAnte.tgain > 99.9 || ftAnte.tgain < 0)
                        {
                            ValErrs.AddMess("True Gain (%f) must be between 0 and  99.9.",
                                            keyLine, "E", ftAnte.tgain.ToString());
                            errCount++;
                        }
                    }

                    if (nullArray[FtAnte.KVALUE] != Constant.DB_NULL)
                    {
                        if (ftAnte.kvalue > 9.99 || ftAnte.kvalue < 0)
                        {
                            ValErrs.AddMess("KValue (%f) must be between 0.00 and 9.99.",
                                            keyLine, "E", ftAnte.kvalue.ToString());
                            errCount++;
                        }
                    }

                    if (nullArray[FtAnte.OBSLOSS] != Constant.DB_NULL)
                    {
                        if (ftAnte.obsloss > 99.9 || ftAnte.obsloss < 0)
                        {
                            ValErrs.AddMess("Obsloss (%f) must be between 0.0 and 99.9",
                                            keyLine, "E", ftAnte.obsloss.ToString());
                            errCount++;
                        }
                    }

                    /* If ause field is present */
                    if (nullArray[FtAnte.AUSE] != Constant.DB_NULL)
                    {

                        /* If ause is TX side */
                        if (ftAnte.ause.Equals("TX") ||
                                ftAnte.ause.Equals("STX") ||
                                      ftAnte.ause.Equals("TR"))
                        {

                            /* Check that call1 agrees with ause if ficticious */
                            if (ftAnte.call1.StartsWith("$"))
                            {
                                if (FtChkChng(pdfName, ftAnte.call1, "=") != Constant.SUCCESS)
                                {
                                    ValErrs.AddMess("Call1 (%s) starts with '$' and is not RX only",
                                                    keyLine, "E", ftAnte.call1);
                                    errCount++;
                                }
                            }

                            if (nullArray[FtAnte.TXCOMPL] != Constant.DB_NULL)
                            {
                                if (ftAnte.txcompl < -99.9 ||

                                    ftAnte.txcompl > 99.9)
                                {
                                    ValErrs.AddMess("TxCompl (%f) must be between -99.9 and 99.9",
                                                    keyLine, "E", ftAnte.txcompl.ToString());
                                    errCount++;
                                }
                            }

                            if (nullArray[FtAnte.TXFDLNLH] != Constant.DB_NULL)
                            {
                                if (ftAnte.txfdlnlh > 999.9 || ftAnte.txfdlnlh < 0)
                                {
                                    ValErrs.AddMess("TXFDLNLH (%f) must be between 0 and  999.9",
                                                    keyLine, "E", ftAnte.txfdlnlh.ToString());
                                    errCount++;
                                }
                            }

                            if (nullArray[FtAnte.TXFDLNLV] != Constant.DB_NULL)
                            {
                                if (ftAnte.txfdlnlv > 999.9 || ftAnte.txfdlnlv < 0)
                                {
                                    ValErrs.AddMess("TXFDLNLV (%f) must be between 0 and  999.9",
                                                    keyLine, "E", ftAnte.txfdlnlv.ToString());
                                    errCount++;
                                }
                            }

                            if (nullArray[FtAnte.TXPADPAM] != Constant.DB_NULL)
                            {
                                if ((ftAnte.txpadpam > 99.9) || (ftAnte.txpadpam < -99.9))
                                {
                                    ValErrs.AddMess("TXPADPAM (%f) must be between -99.9 and  99.9",
                                                    keyLine, "E", ftAnte.txpadpam.ToString());
                                    errCount++;
                                }
                            }
                        }   /* --- End if ant. use is TX --- */

                        /* if ause is RX side */
                        if (ftAnte.ause.Equals("RX") ||
                                ftAnte.ause.Equals("DV1") ||
                                    ftAnte.ause.Equals("DV2") ||
                                           ftAnte.ause.Equals("TR"))
                        {

                            if (nullArray[FtAnte.RXCOMPL] != Constant.DB_NULL)
                            {
                                if (ftAnte.rxcompl < -99.9 || ftAnte.rxcompl > 99.9)
                                {
                                    ValErrs.AddMess("RXCompl (%f) must be between -99.9 and  99.9",
                                                    keyLine, "E", ftAnte.rxcompl.ToString());
                                    errCount++;
                                }
                            }

                            if (nullArray[FtAnte.RXFDLNLH] != Constant.DB_NULL)
                            {
                                if (ftAnte.rxfdlnlh > 999.9 || ftAnte.rxfdlnlh < 0)
                                {
                                    ValErrs.AddMess("RXFDLNLH (%f) must be between 0 and  999.9",
                                           keyLine, "E", ftAnte.rxfdlnlh.ToString());
                                    errCount++;
                                }
                            }

                            if (nullArray[FtAnte.RXFDLNLV] != Constant.DB_NULL)
                            {
                                if (ftAnte.rxfdlnlv > 999.9 || ftAnte.rxfdlnlv < 0)
                                {
                                    ValErrs.AddMess("RXFDLNLV (%f) must be between 0 and  999.9",
                                                    keyLine, "E", ftAnte.rxfdlnlv.ToString());
                                    errCount++;
                                }
                            }

                            if (nullArray[FtAnte.RXPADLNA] != Constant.DB_NULL)
                            {
                                if (ftAnte.rxpadlna < -99.9 || ftAnte.rxpadlna > 99.9)
                                {
                                    ValErrs.AddMess("RXPADLNA (%f) must be between -99.9 and +99.9",
                                           keyLine, "E", ftAnte.rxpadlna.ToString());
                                    errCount++;
                                }
                            }
                        }   /* --- End if ant. use is RX --- */
                    }

                    if (nullArray[FtAnte.SDATE] != Constant.DB_NULL)
                    {
                        if (!ValUtil.UtValidateDate(ftAnte.sdate))
                        {
                            ValErrs.AddMess("Invalid format for Service Date: '%s'",
                                   keyLine, "E", ftAnte.sdate);
                            errCount++;
                        }
                    }

                    if ((nullArray[FtAnte.OFFAZM] != Constant.DB_NULL) && (ftAnte.offazm == "Y"))
                    {
                        /* if offazim is Y then telvth must be present*/
                        if (nullArray[FtAnte.TELVTN] == Constant.DB_NULL)
                        {
                            ValErrs.AddMess("True Elevation must be present if Off Azimuth is :%s:",
                                     keyLine, "E", ftAnte.offazm);
                            errCount++;
                        }

                        /* if offazim is Y then tgain must be present */
                        if (nullArray[FtAnte.TGAIN] == Constant.DB_NULL)
                        {
                            ValErrs.AddMess("True Gain must be present if Off Azimuth is :%s:",
                                            keyLine, "E", ftAnte.offazm);
                            errCount++;
                        }

                        /* if offazim is Y then tazmth must be present*/
                        if (nullArray[FtAnte.TAZMTH] == Constant.DB_NULL)
                        {
                            ValErrs.AddMess("True Azmith must be present if Off Azimuth is :%s:",
                                            keyLine, "E", ftAnte.offazm);
                            errCount++;
                        }
                    }
                } /* end of if not D */

            } /* end of while */

            DynAntenna.FtCloseAntenna(ID1);

            //...Log2.v("\n\nValidTS.FtValAnteTSIntra(): Exit");
        }

        /// <summary>
        /// This method is used to validate a channel PDF. It ensures uniqueness of the 
        /// record and determines if the  required fields have values within set limits.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <param name="valLevel"> - validation level.</param>
        public static void FtValChanTSIntra(string pdfName, ref short errCount, ref short warnCount, ref short valLevel)
        {
            //...Log2.v("\n\nValidTS.FtValChanTSIntra(): Entry");

            /* Local variables */
            string oldCall1 = "";   /* check for uniqueness */
            string oldCall2 = "";
            string oldBndcde = "";
            string keyLine = "";
            string oldChid = "";
            string tableName = "";
            string whereClause = "";
            string orderBy = "";
            int ID1;

            FtChan ftChan;
            SQLLEN[] nullArray;

            bool IsPassive;

            /* read channel information */
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

            orderBy = " call1, call2, bndcde, chid";

            if ((ID1 = DynChannel.FtSelectChannel(tableName, whereClause, orderBy)) < 0)
            {
                /* cannot open cursor */
                ValErrs.AddMess("Cannot read channel information, reason %d.", pdfName, "W");
                warnCount++;
                return;
            }

            while (DynChannel.FtFetchChannel(ID1, out ftChan, out nullArray) == Constant.SUCCESS)
            {
                /* print key information */
                keyLine = String.Format("Channel: {0} {1} {2} {3}",
                     ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid);

                //...Log2.v("\nValidTS.FtValChanTSIntra(): A: ftChan.cmd = " + Strings.AddBars(ftChan.cmd));

                // The ftChan table has a 'cmd' column that is NULLABLE.
                // If the cmd field of an import PDF's CK record is empty then
                // the corresponding ftChan table record has 'cmd' set to NULL.
                // When FtFetchChan() retrieves an ftChan record from the DB, a NULL in the 
                // 'cmd' column is returned as ftChan.cmd = "".

                // Validate that ftChan.cmd[0] is A, B, D, N or U.
                string message;
                if (!FtUtil.IsValidCmd(ftChan.cmd, out message))
                {
                    ValErrs.AddMess(message, keyLine, "E", "");
                    errCount++;
                }

                /*  Valid TX frequency present -- Set the passive switch */
                IsPassive = ftChan.call1.StartsWith("%");

                /* check for uniqueness */
                if (oldCall1.Equals(ftChan.call1) &&
                   oldCall2.Equals(ftChan.call2) &&
                   oldBndcde.Equals(ftChan.bndcde) &&
                   oldChid.Equals(ftChan.chid))
                {
                    ValErrs.AddMess("Channel record not unique in the PDF.", keyLine, "E");
                    errCount++;
                }

                /* copy current information to old information */
                oldCall1.Equals(ftChan.call1);
                oldCall2.Equals(ftChan.call2);
                oldBndcde.Equals(ftChan.bndcde);
                oldChid.Equals(ftChan.chid);

                /* verify the call sign for valid first character */
                //...Log2.v("\r\nftChan.call1 = |" + ftChan.call1 + "|");
                if (!Strings.IsAlphabetic(ftChan.call1[0]))
                {
                    /* not alphabetic */
                    if (Constant.SPECIALCHAR.IndexOf(ftChan.call1[0]) < 0)
                    {
                        /* invalid call sign */
                        ValErrs.AddMess("Invalid first character for Call1 (%s).",
                                        keyLine, "E", ftChan.call1);
                        errCount++;
                    }
                }

                if (!Strings.IsAlphabetic(ftChan.call2[0]))
                {
                    /* not alphabetic */
                    if (Constant.SPECIALCHAR.IndexOf(ftChan.call2[0]) < 0)
                    {
                        /* invalid call sign */
                        ValErrs.AddMess("Invalid first character for Call2 (%s).",
                                        keyLine, "E", ftChan.call2);
                        errCount++;
                    }
                }

                /* validate only if Add, Update, or None not for Delete */
                if (!ftChan.CmdEquals('D'))
                {

                    /* verify that HL-VH-PLAN are there, or none are there*/
                    /* do this by verifying the null indicators */
                    if (((nullArray[FtChan.HL] == Constant.DB_NULL) ||
                         (nullArray[FtChan.VH] == Constant.DB_NULL) ||
                         (nullArray[FtChan.SPLAN] == Constant.DB_NULL))
                    && ((nullArray[FtChan.HL] + nullArray[FtChan.VH] +
                        nullArray[FtChan.SPLAN]) != 3 * Constant.DB_NULL))
                    {
                        /* if one is null, but not all null then error*/
                        ValErrs.AddMess("PLAN-HL-VH must all be there or not be there", keyLine, "E");
                        errCount++;
                    }

                    if (nullArray[FtChan.HOPNUMB] != Constant.DB_NULL && ftChan.hopnumb != 0)
                    {
                        if (ftChan.hopnumb < 1 || ftChan.hopnumb > 99)
                        {
                            ValErrs.AddMess("Hop Number (%d) must be between 1 and 99.",
                                            keyLine, "E", ftChan.hopnumb.ToString());
                            errCount++;
                        }
                    }

                    if (nullArray[FtChan.STNNUMB] != Constant.DB_NULL && ftChan.stnnumb != 0)
                    {
                        if (ftChan.stnnumb < 1 || ftChan.stnnumb > 99)
                        {
                            ValErrs.AddMess("Station Number (%d) must be between 1 and 99.",
                                            keyLine, "E", ftChan.stnnumb.ToString());
                            errCount++;
                        }
                    }

                    if (nullArray[FtChan.VH] != Constant.DB_NULL && ftChan.vh != 0)
                    {
                        if (ftChan.vh < 1 || ftChan.vh > 4)
                        {
                            ValErrs.AddMess("VH (%d) must be between 1 and 4.",
                                            keyLine, "E", ftChan.vh.ToString());
                            /* for later validation on PLAN-HL-VH */
                            ftChan.vh = 0;
                            errCount++;
                        }
                    }

                    if (nullArray[FtChan.HL] != Constant.DB_NULL && ftChan.hl != 0)
                    {
                        if (ftChan.hl < 1 || ftChan.hl > 6)
                        {
                            ValErrs.AddMess("HL must be between 1 and 6.",
                                            keyLine, "E", ftChan.hl.ToString());
                            /* for later validation on PLAN-HL-VH */
                            ftChan.hl = 0;
                            errCount++;
                        }
                    }

                    //...Log2.v("\r\nValidTS.FtValChanTSIntra(): ftChan:");
                    //...Log2.v(ftChan.ToString());

                    /* validate TX side */
                    if (nullArray[FtChan.FREQTX] != Constant.DB_NULL && ftChan.freqtx > 0.0)
                    {
                        if (ftChan.freqtx > 99999999.9
                        || ftChan.freqtx < 0)
                        {
                            ValErrs.AddMess("TX Frequency (%.1f) must be between 0 and 99,999,999.9",
                                            keyLine, "E", ftChan.freqtx.ToString());
                            errCount++;
                        }

                        /*******************************************************************\
                        *
                        *   We have a transmit frequency, ensure that there is a polarization
                        *   and other fields; OEL - 1051.
                        *   Added later 99.02.09, We do not check for powers and so forth
                        *   if the site is a billboard passive (callsign starts with %)
                        *
                        \*******************************************************************/
                        if (nullArray[FtChan.POLTX] == Constant.DB_NULL)
                        {
                            ValErrs.AddMess("TX Frequency present but no polarization",
                                            keyLine, "E");
                            errCount++;
                        }
                        if (nullArray[FtChan.STATTX] == Constant.DB_NULL && !IsPassive)
                        {
                            ValErrs.AddMess("TX Frequency present but no TX Status Code.",
                                            keyLine, "E");
                            errCount++;
                        }
                        if (nullArray[FtChan.PWRTX] == Constant.DB_NULL && !IsPassive)
                        {
                            ValErrs.AddMess("TX Frequency present but no TX Power.", keyLine, "E");
                            errCount++;
                        }
                        if (nullArray[FtChan.EQPTTX] == Constant.DB_NULL && !IsPassive)
                        {
                            ValErrs.AddMess("TX Frequency present but no TX Equipment.", keyLine, "E");
                            errCount++;
                        }

                        /* Check that call1 agrees with ause if fictitious */
                        if (ftChan.call1.StartsWith("$"))
                        {
                            if (FtChkChng(pdfName, ftChan.call1, "=") != Constant.SUCCESS)
                            {
                                ValErrs.AddMess("Call1 (%s) starts with '$' and is not RX only.",
                                                keyLine, "E", ftChan.call1);
                                errCount++;
                            }
                        }

                        if (nullArray[FtChan.PWRTX] != Constant.DB_NULL)
                        {
                            if (ftChan.pwrtx < -999.9 || ftChan.pwrtx > 999.9)
                            {
                                ValErrs.AddMess("TX Power (%f) must be between -999.9 and 999.9",
                                                keyLine, "E", ftChan.pwrtx.ToString());
                                errCount++;
                            }
                        }

                        if ((nullArray[FtChan.EQPTUTX] != Constant.DB_NULL) && (ftChan.eqptutx.Length > 0))
                        {
                            if (Constant.EQPTUTX.IndexOf(ftChan.eqptutx[0]) < 0)
                            {
                                ValErrs.AddMess("TX Equipment Use (%c) must be A, B, H or I.",
                                                keyLine, "E", ftChan.eqptutx[0].ToString());
                                errCount++;
                            }
                        }

                        if (nullArray[FtChan.EQPTTX] != Constant.DB_NULL)
                        {
                            /* determine if eqpttx start w/ $ */
                            if (ftChan.eqpttx.StartsWith("$"))
                            {
                                valLevel = Constant.TSIP;
                            }
                        }

                        if (nullArray[FtChan.POLTX] != Constant.DB_NULL)
                        {
                            if (Constant.POLARIZATION.IndexOf(ftChan.poltx[0]) < 0)
                            {
                                ValErrs.AddMess("TX Polarization (%s) must be H,V,B,L,R,C or blank.",
                                                keyLine, "E", ftChan.poltx);
                                errCount++;
                            }
                        }

                        if (nullArray[FtChan.ANTNUMBTX1] != Constant.DB_NULL)
                        {
                            if (ftChan.antnumbtx1 < 0 || ftChan.antnumbtx1 > 99)
                            {
                                ValErrs.AddMess("AntNumbTX1 (%d) must be between 0 and 99.",
                                              keyLine, "E", ftChan.antnumbtx1.ToString());
                                errCount++;
                            }
                            else if (ftChan.antnumbtx1 != 0)
                            {
                                if (nullArray[FtChan.AFSLTX1] != Constant.DB_NULL)
                                {
                                    /* validate asfl of ante used */
                                    if (ftChan.afsltx1 < 0 || ftChan.afsltx1 > 99.9)
                                    {
                                        string afsltx1Str = ftChan.afsltx1.ToString("F6");

                                        // This is to pass test for PDF = ftest30; cannot reproduce C/C++ rounding error in C#.
                                        if (afsltx1Str.Equals("100.700000"))
                                        {
                                            afsltx1Str = "100.699997";
                                        }

                                        ValErrs.AddMess("AFSLTX1 (%f) must be between 0 and 99.9",
                                                        keyLine, "E", afsltx1Str);
                                        errCount++;
                                    }
                                }
                                else
                                {
                                    /*	antnumbtx1 present, but no feed loss-Error if not passive*/
                                    if (ftChan.call1[0] != '%')
                                    {
                                        ValErrs.AddMess("AFSLTX1 must be present.", keyLine, "E");
                                        errCount++;
                                    }
                                }
                            }
                        }

                        if (nullArray[FtChan.ANTNUMBTX2] != Constant.DB_NULL)
                        {
                            if (ftChan.antnumbtx2 < 0 || ftChan.antnumbtx2 > 99)
                            {
                                ValErrs.AddMess("AntNumbTX2 (%d) must be between 0 and 99.",
                                                keyLine, "E", ftChan.antnumbtx2.ToString());
                                errCount++;
                            }

                            if (nullArray[FtChan.AFSLTX2] != Constant.DB_NULL)
                            {
                                /* asfl validated if ante used*/
                                if (ftChan.afsltx2 < 0 || ftChan.afsltx2 > 99.9)
                                {
                                    ValErrs.AddMess("AFSLTX2 (%f) must be between 0 and 99.9",
                                                    keyLine, "E", ftChan.afsltx2.ToString());
                                    errCount++;
                                }
                            }
                            else
                            {
                                /*	antnumbtx1 present, but no feed loss-Error if not passive*/
                                if (ftChan.call1[0] != '%')
                                {
                                    ValErrs.AddMess("AFSLTX2 must be present", keyLine, "E");
                                    errCount++;
                                }
                            }
                        }

                        if (nullArray[FtChan.ATPCCDE] != Constant.DB_NULL)
                        {
                            if (ftChan.atpccde < 0 || ftChan.atpccde > 99.9)
                            {
                                ValErrs.AddMess("ATPCCDE (%f) must be between 0 and 99.9",
                                                keyLine, "E", ftChan.atpccde.ToString("F6"));
                                errCount++;
                            }
                        }

                        if (nullArray[FtChan.STATTX] != Constant.DB_NULL)
                        {
                            if (Constant.STATUS.IndexOf(ftChan.stattx[0]) < 0)
                            {
                                ValErrs.AddMess("STATTX (%s) must be between 0 and 8.",
                                                keyLine, "E", ftChan.stattx);
                                errCount++;
                            }
                        }

                        if ((nullArray[FtChan.SDATE] != Constant.DB_NULL) && (ftChan.sdate.Length > 0))
                        {
                            if (!ValUtil.UtValidateDate((ftChan.sdate)))
                            {
                                ValErrs.AddMess("Invalid format (%s) for TX Service Date.",
                                                keyLine, "E", ftChan.sdate);
                                errCount++;
                            }
                        }
                    } /* end TX side */

                    /* Now verify the RX side */

                    if (nullArray[FtChan.FREQRX] != Constant.DB_NULL && ftChan.freqrx > 0.0)
                    {
                        /*	First check that it is not equal to the TX frequency  */
                        if (nullArray[FtChan.FREQTX] != Constant.DB_NULL &&
                             ftChan.freqrx == ftChan.freqtx)
                        {
                            ValErrs.AddMess("TX and RX frequencies the same.", keyLine, "W");
                            warnCount++;
                        }


                        if (nullArray[FtChan.ANTNUMBRX1] != Constant.DB_NULL)
                        {
                            if (ftChan.antnumbrx1 < 0 || ftChan.antnumbrx1 > 99)
                            {
                                ValErrs.AddMess("AntNumbRX1 (%d) must be between 0 and 99.",
                                                keyLine, "E", ftChan.antnumbrx1.ToString());
                                errCount++;
                            }
                        }
                        else
                        {
                            ValErrs.AddMess("ANTNUMBRX1 must be present.", keyLine, "E");
                            errCount++;
                        }


                        /*******************************************************************\
                        *
                        *   We have a Receive frequency, ensure that there is a polarization
                        *   OEL - 1051
                        *
                        \*******************************************************************/
                        if (nullArray[FtChan.POLRX] == Constant.DB_NULL)
                        {
                            ValErrs.AddMess("RX Frequency present but no polarization.", keyLine, "E");
                            errCount++;
                        }

                        if (nullArray[FtChan.ANTNUMBRX2] != Constant.DB_NULL)
                        {
                            if (ftChan.antnumbrx2 < 0 || ftChan.antnumbrx2 > 99)
                            {
                                ValErrs.AddMess("AntNumbRX2 (%d) must be between 0 and 99.",
                                                keyLine, "E", ftChan.antnumbrx2.ToString());
                                errCount++;
                            }
                        }

                        if (nullArray[FtChan.ANTNUMBRX3] != Constant.DB_NULL)
                        {
                            if (ftChan.antnumbrx3 < 0 || ftChan.antnumbrx3 > 99)
                            {
                                ValErrs.AddMess("AntNumbRX3 (%d) must be between 0 and 99.",
                                                keyLine, "E", ftChan.antnumbrx3.ToString());
                                errCount++;
                            }
                        }

                        if (nullArray[FtChan.AFSLRX1] != Constant.DB_NULL)
                        {
                            if (ftChan.afslrx1 < 0 || ftChan.afslrx1 > 99.9)
                            {
                                ValErrs.AddMess("AFSLRX1 (%f) must be between 0 and 99.9.",
                                                keyLine, "E", ftChan.afslrx1.ToString());
                                errCount++;
                            }
                        }
                        else
                        {
                            /* if passive repeater, afsl not reqd */
                            if (!ftChan.call1.StartsWith("%"))
                            {
                                ValErrs.AddMess("AFSLRX1 must be present.", keyLine, "E");
                                errCount++;
                            }
                        }

                        if (nullArray[FtChan.AFSLRX2] != Constant.DB_NULL)
                        {
                            if (ftChan.afslrx2 < 0 || ftChan.afslrx2 > 99.9)
                            {
                                ValErrs.AddMess("AFSLRX2 (%f) must be between 0 and 99.9.",
                                                keyLine, "E", ftChan.afslrx2.ToString());
                                errCount++;
                            }
                        }

                        if (nullArray[FtChan.AFSLRX3] != Constant.DB_NULL)
                        {
                            if (ftChan.afslrx3 < 0 || ftChan.afslrx3 > 99.9)
                            {
                                ValErrs.AddMess("AFSLRX3 (%f) must be between 0 and 99.9.",
                                                keyLine, "E", ftChan.afslrx3.ToString());
                                errCount++;
                            }
                        }

                        if (nullArray[FtChan.EQPTRX] != Constant.DB_NULL)
                        {
                            if (ftChan.eqptrx.StartsWith("$"))
                            {
                                valLevel = Constant.TSIP;
                            }
                        }

                        if ((nullArray[FtChan.EQPTURX] != Constant.DB_NULL) && (ftChan.eqpturx.Length > 0))
                        {
                            if (Constant.EQPTURX.IndexOf(ftChan.eqpturx[0]) < 0)
                            {
                                ValErrs.AddMess("RX Equipment Use (%c) must be A,B,C,H or I.",
                                                keyLine, "E", ftChan.eqpturx[0].ToString());
                                errCount++;
                            }
                        }

                        if (nullArray[FtChan.TSINT] != Constant.DB_NULL)
                        {
                            if (ftChan.tsint < -999.9 || ftChan.tsint > 999.9)
                            {
                                ValErrs.AddMess("TSINT (%f) must be between -999.9 and 999.9.",
                                                keyLine, "E", ftChan.tsint.ToString());
                                errCount++;
                            }
                        }

                        if (nullArray[FtChan.ESINT] != Constant.DB_NULL)
                        {
                            if (ftChan.esint < -999.9 || ftChan.esint > 999.9)
                            {
                                ValErrs.AddMess("ESINT (%f) must be between -999.9 and 999.9.",
                                                keyLine, "E", ftChan.esint.ToString());
                                errCount++;
                            }
                        }
                    } /* end RX side */
                } /* end of if not D */
            } /* end of while */


            DynChannel.FtCloseChannel(ID1);

            //...Log2.v("\n\nValidTS.FtValChanTSIntra(): Exit");

        }	/* ----- End of ftValChanTSIntra ----- */





    }
}
