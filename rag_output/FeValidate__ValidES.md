# Documented File: ValidES.cs
**Repository Path:** `FeValidate\ValidES.cs`
**Primary Layer:** `FeValidate`
**Namespace:** `FeValidate`

## Source Code Representation
```csharp
﻿using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeValidate
{
    using _Configuration;
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
    /// This class provides methods that perform 'intra' validation of
    /// the site, antenna, azimuth and channel information provided by 
    /// a previously imported PDF; 'intra' validation encompasses all
    /// checking of the ES PDF table records that can be performed without
    /// reading information from the ES MDB tables.
    /// </summary>
    public class ValidES
    {
        private const string titleLine = "INTERNAL VALIDATION";

        /// <summary>
        /// This method provides the initial 'intra' validation of the site 
        /// information in a previously imported PDF that can be performed
        /// without accessing the main and subsiduary database tables. 
        /// </summary>
        /// <remarks>
        /// The method checks that there are no duplicate {location} site 'keys' 
        /// and then parses through the site-specific entries checking for format/syntax and
        /// numerical bounds.
        /// </remarks>
        /// <param name="errCount"> - cummulative count of errors.</param>
        /// <param name="warnCount"> - cummulative count of warnings.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        public static void FeValSiteIntra(ref short errCount, ref short warnCount, string pdfName)
        {
            FeSite feSite;
            SQLLEN[] nArray;
            string oldLocation = "";
            string tableName;
            string keyLine = "";
            int siteHandle;

            GenUtil.UtCvtName(Constant.FE_SITE, pdfName, out tableName);

            // Read site information.
            if ((siteHandle = DynFeSite.FeSelectSite(tableName, "", "location")) < 0)
            {
                ValErrs.AddMess("Could not read site information. Reason: %d",
                                pdfName, "W", siteHandle.ToString());
                warnCount++;
                return;
            }

            while (DynFeSite.FeFetchSite(siteHandle, out feSite, out nArray) == Constant.SUCCESS)
            {
                keyLine = String.Format("KEY:  {0}", feSite.location);

                // AH: this should never occur because FeImport flags an error
                //     if duplicate site locations are found in the PDF.
                // Check for uniqueness.
                if (Strings.StrnCmp(oldLocation, feSite.location, 10) == 0)
                {
                    ValErrs.AddMess("Site record not unique in PDF.", keyLine, "E");
                    errCount++;
                }

                // Copy current information to old information.
                oldLocation = feSite.location;

                // This validation is also redundant because FeImport
                // performs the same check.
                // Verify the command for a valid code.
                if (!Strings.CharIsOneOfABDNU(feSite.cmd[0]))
                {
                    ValErrs.AddMess("Command (%s) must be A, B, D, N or U.",
                                    keyLine, "E", feSite.cmd);
                    errCount++;
                }

                if (!feSite.cmd.Equals("D"))
                {
                    if (nArray[FeSite.PROV] != Constant.DB_NULL)
                    {
                        if (!ValUtil.UtValidateProv(feSite.prov.Trim()))
                        {
                            ValErrs.AddMess("Invalid province code (%s).", keyLine, "E", feSite.prov);
                            errCount++;
                        }
                    }

                    if (nArray[FeSite.LATIT] != Constant.DB_NULL)
                    {
                        if (!ValUtil.UtValidateLat(feSite.latit))
                        {
                            ValErrs.AddMess("Invalid value for Latitude (%d).",
                                   keyLine, "E", feSite.latit.ToString());
                            errCount++;
                        }
                    }

                    if (nArray[FeSite.LONGIT] != Constant.DB_NULL)
                    {
                        if (!ValUtil.UtValidateLong(feSite.longit))
                        {
                            ValErrs.AddMess("Invalid value for Longitude (%d).",
                                            keyLine, "E", feSite.longit.ToString());
                            errCount++;
                        }
                    }

                    if (nArray[FeSite.GRND] != Constant.DB_NULL)
                    {
                        if (feSite.grnd < 0.0 || feSite.grnd > Constant.MAX_SITE_ELEVATION_M)   // Previously 4999.9
                        {
                            string str = String.Format("Ground height ({0:F1}) must be between 0.0 and {1:F1}",
                            feSite.grnd, Constant.MAX_SITE_ELEVATION_M);

                            ValErrs.AddMess(str, keyLine, "E");
                            errCount++;
                        }
                    }

                    if (nArray[FeSite.RADIO] != Constant.DB_NULL)
                    {
                        if (!Strings.CharIsOneOfABC(feSite.radio[0]))
                        {
                            ValErrs.AddMess("Radio (%s) must be A, B or C.", keyLine, "E", feSite.radio);
                            errCount++;
                        }
                    }

                    if (nArray[FeSite.RAIN] != Constant.DB_NULL)
                    {
                        if (feSite.rain < 0 || feSite.rain > 99)
                        {
                            ValErrs.AddMess("Rain (%d) must be between 0 and 99",
                                            keyLine, "E", feSite.rain.ToString());
                            errCount++;
                        }
                    }

                    if (nArray[FeSite.SDATE] != Constant.DB_NULL)
                    {
                        if (!ValUtil.UtValidateDate(feSite.sdate.Trim()))
                        {
                            ValErrs.AddMess("Invalid format for Service Date (%s).",
                                            keyLine, "E", feSite.sdate);
                            errCount++;
                        }
                    }

                    if (nArray[FeSite.STATS] != Constant.DB_NULL)
                    {
                        if (!Strings.CharIsOneOf012345678(feSite.stats[0]))
                        {
                            ValErrs.AddMess("Status (%s) must be between 0 and 8.",
                                   keyLine, "E", feSite.stats);
                            errCount++;
                        }
                    }
                }
            } // end of while 
            DynFeSite.FeCloseSite(siteHandle);
        }

        /// <summary>
        /// This method provides the initial 'intra' validation of the antennae 
        /// information in a previously imported PDF that can be performed
        /// without accessing the main and subsiduary database tables. 
        /// </summary>
        /// <remarks>
        /// The method checks that there are no duplicate {location, call1} antenna 'keys' 
        /// and then parses through antenna-specific entries checking for format/syntax and
        /// numerical bounds.
        /// </remarks>
        /// <param name="errCount"> - cummulative count of errors.</param>
        /// <param name="warnCount"> - cummulative count of warnings.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        public static void FeValAnteIntra(ref short errCount, ref short warnCount, string pdfName)
        {
            FeAnte feAnte;
            SQLLEN[] nArray;
            int anteHandle;
            string oldLocation;   // Check for uniqueness 
            string tableName;
            string oldCall1;
            string keyLine = "";

            short recFound;
            SuSatOper suSatOp;

            oldLocation = "";
            oldCall1 = "";

            GenUtil.UtCvtName(Constant.FE_ANTE, pdfName, out tableName);

            // Read antenna information. 
            if ((anteHandle = DynFeAnte.FeSelectAnte(tableName, "", "location, call1")) < 0)
            {
                ValErrs.AddMess("Could not read antenna information. Reason: %d",
                                pdfName, "W", anteHandle.ToString());
                warnCount++;
                return;
            }

            while (DynFeAnte.FeFetchAnte(anteHandle, out feAnte, out nArray) == Constant.SUCCESS)
            {
                keyLine = String.Format("KEY: {0} {1}.", feAnte.location, feAnte.call1);

                // Check for uniqueness.
                if ((Strings.StrnCmp(oldLocation, feAnte.location, 10) == 0) &&
                        (Strings.StrnCmp(oldCall1, feAnte.call1, 9) == 0))
                {
                    ValErrs.AddMess("Antenna record not unique in PDF.", keyLine, "E");
                    errCount++;
                }

                // Copy current information to temp information.
                oldLocation = feAnte.location;
                oldCall1 = feAnte.call1;

                if (!Strings.CharIsOneOfABDNU(feAnte.cmd[0]))
                {
                    ValErrs.AddMess("Command (%s) must be A, B, D, N or U.",
                                    keyLine, "E", feAnte.cmd);
                    errCount++;
                }

                if (!feAnte.cmd.Equals("D"))
                {
                    // Verify that the call sign has a valid first character.
                    // Using regex syntax, [a-zA-Z0-9] are all valid first characters.
                    // Fictitious callsigns can begin with "=" or "$".
                    //	    if "=",  if TX only, RX only, or both TX and RX
                    //	    if "$",  if RX only

                    // Verify all fields that are not RX or TX related.
                    if (!Strings.IsAlphabetic(feAnte.call1[0]))
                    {
                        // First char is not alpha-numeric. 
                        if (feAnte.call1[0] == '$')
                        {
                            if ((nArray[FeAnte.ACODETX] != Constant.DB_NULL) &&
                                (FeChkChng(pdfName, feAnte.call1, '=')) != Constant.SUCCESS)
                            {
                                // Invalid call sign. 
                                ValErrs.AddMess("A call sign (%s) that begins with '$' must have RX frequencies only.",
                                                keyLine, "E", feAnte.call1);
                                errCount++;
                            }
                        }
                        else if (feAnte.call1[0] != '=')
                        {
                            // Invalid call sign.
                            ValErrs.AddMess("Invalid 1st character for Call Sign (%s).",
                                            keyLine, "E", feAnte.call1);
                            errCount++;
                        }
                    }

                    if (nArray[FeAnte.G_T] != Constant.DB_NULL)
                    {
                        if (feAnte.g_t < -99.9 || feAnte.g_t > 99.9)
                        {
                            ValErrs.AddMess("Gain/Temp. (%f) must be between -99.9 and 99.9.",
                                            keyLine, "E", feAnte.g_t.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeAnte.LNAT] != Constant.DB_NULL)
                    {
                        if (feAnte.lnat < 0.0 || feAnte.lnat > 999.9)
                        {
                            ValErrs.AddMess("Noise Temp. (%f) must be between 0.0 and 999.9.",
                                            keyLine, "E", feAnte.lnat.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeAnte.AHT] != Constant.DB_NULL)
                    {
                        if (feAnte.aht < 0.0 || feAnte.aht > 1000.0)
                        {
                            ValErrs.AddMess("Antenna Height (%f) must be between 0.0 and 1000.0.",
                                            keyLine, "E", feAnte.aht.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeAnte.OP2] != Constant.DB_NULL)
                    {
                        recFound = (short)Suutils.SuGetSatOper(feAnte.op2, out suSatOp);
                        if (recFound != 0)
                        {
                            ValErrs.AddMess("Satellite Operator (%s) must be a valid code.",
                                            keyLine, "E", feAnte.op2);
                            errCount++;
                        }
                    }

                    if (nArray[FeAnte.SATLONG] != Constant.DB_NULL)
                    {
                        if (feAnte.satlong < Constant.MINSATLONG || feAnte.satlong > Constant.MAXSATLONG)
                        {
                            ValErrs.AddMess("Satellite Long. (%.2f) must be between %.2f and %.2f.",
                                            keyLine, "E", feAnte.satlong.ToString("F2"),
                                            Constant.MINSATLONG.ToString("F2"), Constant.MAXSATLONG.ToString("F2"));
                            errCount++;
                        }
                    }
                    if (nArray[FeAnte.SATLONGS] != Constant.DB_NULL)
                    {
                        if ((!feAnte.satlongs.Equals("E")) &&
                                (!feAnte.satlongs.Equals("W")))
                        {
                            ValErrs.AddMess("Satellite Long. Sense (%s) must be E or W.",
                                            keyLine, "E", feAnte.satlongs);
                            errCount++;

                        }
                    }

                    if (nArray[FeAnte.SARC1] != Constant.DB_NULL)
                    {
                        if (feAnte.sarc1 < -180.0 || feAnte.sarc1 > 180.0)
                        {
                            ValErrs.AddMess("Arc Orbit Center (%f) must be between -180.0 and 180.0.",
                                            keyLine, "E", feAnte.sarc1.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeAnte.SARC2] != Constant.DB_NULL)
                    {
                        if (feAnte.sarc2 < 0.0 || feAnte.sarc2 > 90.0)
                        {
                            ValErrs.AddMess("Arc Half Width (%f) must be between 0.0 and 90.0.",
                                            keyLine, "E", feAnte.sarc2.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeAnte.STATA] != Constant.DB_NULL)
                    {
                        if (!Strings.CharIsOneOf012345678(feAnte.stata[0]))
                        {
                            ValErrs.AddMess("Status (%s) must be between 0 and 8.",
                                            keyLine, "E", feAnte.stata);
                            errCount++;
                        }
                    }

                    if (nArray[FeAnte.ANTREF] != Constant.DB_NULL)
                    {
                        if (feAnte.antref < 0 || feAnte.antref > 999999)
                        {
                            ValErrs.AddMess("Antenna Reference (%d) must be between 0 and 999999.",
                                            keyLine, "E", feAnte.antref.ToString());
                            errCount++;
                        }
                    }

                    // Verify all fields that are required for TX only.
                    if (nArray[FeAnte.AFSLT] != Constant.DB_NULL)
                    {
                        if (feAnte.afslt < 0.0 || feAnte.afslt > 99.9)
                        {
                            ValErrs.AddMess("TX Afsl (%f) must be between 0.0 and 99.9.",
                                            keyLine, "E", feAnte.afslt.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeAnte.TXPRE] != Constant.DB_NULL)
                    {
                        if (feAnte.txpre < 0.0 || feAnte.txpre > 9999.99)
                        {
                            ValErrs.AddMess("TX Precip. Scatter Dist. (%f) must be between 0.0 and 9999.99.",
                                            keyLine, "E", feAnte.txpre.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeAnte.TXTRO] != Constant.DB_NULL)
                    {
                        if (feAnte.txtro < 0.0 || feAnte.txtro > 9999.99)
                        {
                            ValErrs.AddMess("TX Tropo. Scatter Dist. (%f) must be between 0.0 and 9999.99.",
                                            keyLine, "E", feAnte.txtro.ToString("F6"));
                            errCount++;
                        }
                    }

                    // Verify all fields that are required on the RX side only.
                    if (nArray[FeAnte.AFSLR] != Constant.DB_NULL)
                    {
                        if (feAnte.afslr < 0.0 || feAnte.afslr > 99.9)
                        {
                            ValErrs.AddMess("RX Afsl (%f) must be between 0.0 and 99.9.",
                                            keyLine, "E", feAnte.afslr.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeAnte.RXPRE] != Constant.DB_NULL)
                    {
                        if (feAnte.rxpre < 0.0 || feAnte.rxpre > 9999.99)
                        {
                            ValErrs.AddMess("RX Precip. Scatter Dist. (%f) must be between 0.0 and 9999.99.",
                                            keyLine, "E", feAnte.rxpre.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeAnte.RXTRO] != Constant.DB_NULL)
                    {
                        if (feAnte.rxtro < 0.0 || feAnte.rxtro > 9999.99)
                        {
                            ValErrs.AddMess("RX Tropo. Scatter Dist. (%f) must be between 0.0 and 9999.99.",
                                            keyLine, "E", feAnte.rxtro.ToString("F6"));
                            errCount++;
                        }
                    }
                }
            } // end of while 
            DynFeAnte.FeCloseAnte(anteHandle);
        }

        /// <summary>
        /// This method checks whether a change to a fictitious call sign would 
        /// make an otherwise illegal antenna be considered legal.
        /// </summary>
        /// <remarks>
        /// The method checks that there are no duplicate {location, call1, azim} azimuth 'keys' 
        /// and then parses through the azimuth-specific entries checking for format/syntax and
        /// numerical bounds.
        /// </remarks>
        /// <param name="pdfName"></param>
        /// <param name="call"></param>
        /// <param name="newsign"> - site call1 info</param>
        /// <returns> - if the call sign is being changed to 'newsign', then Constant.SUCCESS 
        /// is returned; otherwise Constant.FAILURE. 
        /// </returns>
        public static int FeChkChng(string pdfName, string call, char newsign)
        {
            int ret;
            int feChngHandle;       // dynamic cursor handles 
            SQLLEN[] feChngNulls;
            string tableName;
            FeCCal feCCal;
            string siteCall;
            string whereClause;

            ret = Constant.FAILURE;     // Set return code to failure ...code must 
                                        // verify that this condition does not exist. 

            siteCall = call;


            // Check to see if the call sign is being changed to the
            // newsign value.  If it is, then consider this fictitious
            // call sign as being OK.

            // Set up the Ingres cursor. 
            GenUtil.UtCvtName(Constant.FE_CCAL, pdfName, out tableName);

            whereClause = String.Format("oldcallsign='{0}'", siteCall);

            // Select change call sign records from PDF.
            if ((feChngHandle = DynFeCCal.FeSelectCCal(tableName, whereClause, "")) >= 0)
            {
                // Read all change call sign records for the site. 
                while (DynFeCCal.FeFetchCCal(feChngHandle, out feCCal, out feChngNulls) == Constant.SUCCESS)
                {
                    // If the call sign is being changed to 'newsign', then it is OK.
                    if (feCCal.newcallsign[0] == newsign)
                    {
                        ret = Constant.SUCCESS;
                    }
                }

                DynFeCCal.FeCloseCCal(feChngHandle);
            }

            return (ret);

        }   // ----- End of feChkChng ----- 

        /// <summary>
        /// This method provides the initial 'intra' validation of the channel 
        /// information in a previously imported PDF that can be performed
        /// without accessing the main and subsiduary database tables. 
        /// </summary>
        /// <remarks>
        /// The method checks that there are no duplicate {location, call1, chid} channel 'keys' 
        /// and then parses through the channel-specific entries checking for format/syntax and
        /// numerical bounds.
        /// </remarks>
        /// <param name="errCount"> - cummulative count of errors.</param>
        /// <param name="warnCount"> - cummulative count of warnings.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        public static void FeValChanIntra(ref short errCount, ref short warnCount, string pdfName)
        {
            FeChan feChan;
            SQLLEN[] nArray;
            int chanHandle;
            string oldLocation;
            string oldCall1;
            string oldChid;
            string tableName;
            string keyLine = "";

            oldLocation = "";
            oldCall1 = "";
            oldChid = "";

            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out tableName);

            // Read channel information.
            if ((chanHandle = DynFeChan.FeSelectChan(tableName, "", "location,call1,chid")) < 0)
            {
                ValErrs.AddMess("Could not read channel information. Reason: %d",
                                pdfName, "W", chanHandle.ToString());
                warnCount++;
                return;
            }

            while (DynFeChan.FeFetchChan(chanHandle, out feChan, out nArray) == Constant.SUCCESS)
            {
                keyLine = String.Format("KEY: {0} {1} {2}.", feChan.location, feChan.call1, feChan.chid);

                // Check for uniqueness.
                if ((Strings.StrnCmp(oldLocation, feChan.location, 10) == 0) &&
                        (Strings.StrnCmp(oldCall1, feChan.call1, 9) == 0) &&
                        (Strings.StrnCmp(oldChid, feChan.chid, 4) == 0))
                {
                    ValErrs.AddMess("Channel record not unique in PDF.", keyLine, "E");
                    errCount++;
                }

                // Copy current information to old information.
                oldLocation = feChan.location;
                oldCall1 = feChan.call1;
                oldChid = feChan.chid;

                if (!Strings.CharIsOneOfABDNU(feChan.cmd[0]))
                {
                    ValErrs.AddMess("Command (%s) must be A, B, D, N or U.",
                                    keyLine, "E", feChan.cmd);
                    errCount++;
                }

                if (!feChan.cmd.Equals("D"))
                {
                    // Verify all fields that are not RX or TX related. 

                    // Verify the call sign for valid first character.
                    if (!Strings.IsAlphabetic(feChan.call1[0]))
                    {
                        // First char is not alpha-numeric. 
                        if (feChan.call1[0] == '$')
                        {
                            if ((nArray[FeChan.FREQTX] != Constant.DB_NULL) &&
                                    (FeChkChng(pdfName, feChan.call1, '=')) != Constant.SUCCESS)
                            {
                                // Invalid call sign. 
                                ValErrs.AddMess("A call sign (%s) that begins with '$' must have RX frequencies only.",
                                                keyLine, "E", feChan.call1);
                                errCount++;
                            }
                        }
                        else if (feChan.call1[0] != '=')
                        {
                            // Invalid call sign.
                            ValErrs.AddMess("Invalid 1st character for Call Sign (%s).",
                                         keyLine, "E", feChan.call1);
                            errCount++;
                        }
                    }

                    if (nArray[FeChan.P4KHZ] != Constant.DB_NULL)
                    {
                        if (feChan.p4khz < 0.0 || feChan.p4khz > 50.0)
                        {
                            ValErrs.AddMess("Energy Dispersal (%f) must be between 0.0 and 50.0.",
                                            keyLine, "E", feChan.p4khz.ToString("F6"));
                            errCount++;
                        }
                    }

                    // Verify all TX side fields.
                    if (nArray[FeChan.FREQTX] != Constant.DB_NULL)
                    {
                        if (feChan.freqtx < 0.0 || feChan.freqtx > 99999999.9)
                        {
                            ValErrs.AddMess("TX Frequency (%f) must be between 0.0 and 99999999.9.",
                                            keyLine, "E", feChan.freqtx.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeChan.POLTX] != Constant.DB_NULL)
                    {
                        if (!Strings.CharIsOneOfHVBLRCsp(feChan.poltx[0]))
                        {
                            ValErrs.AddMess("TX Polarization (%s) must be H, V, B, L, R, C.",
                                            keyLine, "E", feChan.poltx);
                            errCount++;
                        }
                    }

                    if (nArray[FeChan.MAXTXPOWER] != Constant.DB_NULL)
                    {
                        if (feChan.maxtxpower < -99.9 || feChan.maxtxpower > 99.9)
                        {
                            ValErrs.AddMess("MaxTXPower (%f) must be between -99.9 and 99.9.",
                                            keyLine, "E", feChan.maxtxpower.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeChan.PWRTX] != Constant.DB_NULL)
                    {
                        if (feChan.pwrtx < -99.99 || feChan.pwrtx > 99.99)
                        {
                            ValErrs.AddMess("TX Power (%f) must be between -99.99 and 99.99.",
                                            keyLine, "E", feChan.pwrtx.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeChan.STATTX] != Constant.DB_NULL)
                    {
                        if (!Strings.CharIsOneOf012345678(feChan.stattx[0]))
                        {
                            ValErrs.AddMess("TX Status (%s) must be between 0 and 8.",
                                            keyLine, "E", feChan.stattx);
                            errCount++;
                        }
                    }

                    if (nArray[FeChan.FEETX] != Constant.DB_NULL)
                    {
                        SuFeeCode tFee;

                        if (Suutils.SuGetFeeCode(feChan.feetx, out tFee) != 0)
                        {
                            ValErrs.AddMess("TX Fee (%s) not recognized.", keyLine, "E", feChan.feetx);
                            errCount++;
                        }
                    }

                    // Verify all the RX side fields. 
                    if (nArray[FeChan.FREQRX] != Constant.DB_NULL)
                    {
                        if (feChan.freqrx < 0.0 || feChan.freqrx > 99999999.9)
                        {
                            ValErrs.AddMess("RX Frequency (%f) must be between 0.0 and 99999999.9.",
                                            keyLine, "E", feChan.freqrx.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeChan.POLRX] != Constant.DB_NULL)
                    {
                        if (!Strings.CharIsOneOfHVBLRCsp(feChan.polrx[0]))
                        {
                            ValErrs.AddMess("RX Polarization (%s) must be H, V, B, L, R, C.",
                                            keyLine, "E", feChan.polrx);
                            errCount++;
                        }
                    }

                    if (nArray[FeChan.PWRRX] != Constant.DB_NULL)
                    {
                        if (feChan.pwrrx < -999.9 || feChan.pwrrx > 0.0)
                        {
                            ValErrs.AddMess("RX Power (%f) must be between 0.0 and -999.9.",
                                            keyLine, "E", feChan.pwrrx.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeChan.STATRX] != Constant.DB_NULL)
                    {
                        if (!Strings.CharIsOneOf012345678(feChan.statrx[0]))
                        {
                            ValErrs.AddMess("RX Status (%s) must be between 0 and 8.",
                                            keyLine, "E", feChan.statrx);
                            errCount++;
                        }
                    }

                    if (nArray[FeChan.I20] != Constant.DB_NULL)
                    {
                        if (feChan.i20 < -999.9 || feChan.i20 > 0.0)
                        {
                            ValErrs.AddMess("Interference Power (%f) must be between -999.9 and 0.0.",
                                            keyLine, "E", feChan.i20.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeChan.IP01] != Constant.DB_NULL)
                    {
                        if (feChan.ip01 < -999.9 || feChan.ip01 > 0.0)
                        {
                            ValErrs.AddMess("Percipitation Interference (%f) must be between -999.9 and 0.0.",
                                            keyLine, "E", feChan.ip01.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeChan.IT01] != Constant.DB_NULL)
                    {
                        if (feChan.it01 < -999.9 || feChan.it01 > 0.0)
                        {
                            ValErrs.AddMess("Tropospheric Interference (%f) must be between -999.9 and 0.0.",
                                            keyLine, "E", feChan.it01.ToString("F6"));
                            errCount++;
                        }
                    }

                    if (nArray[FeChan.FEERX] != Constant.DB_NULL)
                    {
                        SuFeeCode tFee;

                        if (Suutils.SuGetFeeCode(feChan.feerx, out tFee) != 0)
                        /*(strchr(FEECAT, feChan.feerx[0]) == 0) ||
                    ((feChan.feerx[1] != '\0') && (feChan.feerx[1] != ' ')))*/
                        {
                            ValErrs.AddMess("RX Fee (%s) not recognized.", keyLine, "E", feChan.feerx);
                            errCount++;
                        }
                    }

                } // if (!feChan.cmd.Equals("D"))

            } // end of while 
            DynFeChan.FeCloseChan(chanHandle);
        }

        /// <summary>
        /// This method provides the initial 'intra' validation of the azimuth 
        /// information in a previously imported PDF that can be performed
        /// without accessing the main and subsiduary database tables. 
        /// </summary>
        /// <remarks>
        /// The method checks that there are no duplicate {location, call1, azim} azimuth 'keys' 
        /// and then parses through the azimuth-specific entries checking for format/syntax and
        /// numerical bounds.
        /// </remarks>
        /// <param name="errCount"> - cummulative count of errors.</param>
        /// <param name="warnCount"> - cummulative count of warnings.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        public static void FeValAzimIntra(ref short errCount, ref short warnCount, string pdfName)
        {
            FeAzim feAzim;
            SQLLEN[] nArray;
            float oldAzim = -1.0f; ;
            int azimHandle;
            string oldLocation = "";
            string oldCall1 = "";
            string tableName;
            string keyLine = "";

            GenUtil.UtCvtName(Constant.FE_AZIM, pdfName, out tableName);

            // Read azimuth information.
            if ((azimHandle = DynFeAzim.FeSelectAzim(tableName, "", "location,call1,azim")) < 0)
            {
                ValErrs.AddMess("WARNING - Could not read azimuth information. Reason: %d",
                                pdfName, "W", azimHandle.ToString());
                warnCount++;
                return;
            }

            while (DynFeAzim.FeFetchAzim(azimHandle, out feAzim, out nArray) == Constant.SUCCESS)
            {
                keyLine = String.Format("KEY: {0} {1} {2:F1}.",
                                feAzim.location, feAzim.call1, feAzim.azim);

                // Check for uniqueness.
                if ((Strings.StrnCmp(oldLocation, feAzim.location, 10) == 0) &&
                        (Strings.StrnCmp(oldCall1, feAzim.call1, 9) == 0) &&
                        (oldAzim == feAzim.azim))
                {
                    ValErrs.AddMess("Azimuth record not unique in PDF.", keyLine, "E");
                    errCount++;
                }

                // Copy current information to temp information.
                oldLocation = feAzim.location;
                oldCall1 = feAzim.call1;
                oldAzim = feAzim.azim;

                if (!Strings.CharIsOneOfABDNU(feAzim.cmd[0]))
                {
                    ValErrs.AddMess("Command (%s) must be A, U, D, B, or N.", keyLine, "E", feAzim.cmd);
                    errCount++;
                }

                if ((!feAzim.cmd.Equals("D")) && (!feAzim.deleteall.Equals("Y")))
                {
                    // Verify the call sign for valid first character.
                    if (!Strings.IsAlphabetic(feAzim.call1[0]))
                    {
                        // Not alpha-numeric.
                        if (Strings.StrChr("$=", feAzim.call1[0]) == 0)
                        {
                            // Invalid call sign.
                            ValErrs.AddMess("Invalid first character for Call Sign (%s).",
                                            keyLine, "E", feAzim.call1);
                            errCount++;
                        }
                    }

                    if (feAzim.azim < 0.0 || feAzim.azim > 360.0)
                    {
                        ValErrs.AddMess("Azimuth (%f) must be between 0.00 and 360.00.",
                                keyLine, "E", feAzim.azim.ToString("F6"));
                        errCount++;
                    }

                    if (feAzim.elev < -90.0 || feAzim.elev > 90.0)
                    {
                        ValErrs.AddMess("Elevation (%f) must be between -90.00 and 90.00.",
                                        keyLine, "E", feAzim.elev.ToString("F6"));
                        errCount++;
                    }

                    if (feAzim.dist < 0.0 || feAzim.dist > 999.99)
                    {
                        ValErrs.AddMess("Distance (%f) must be between 0.00 and 999.99.",
                                        keyLine, "E", feAzim.dist.ToString("F6"));
                        errCount++;
                    }

                    if (feAzim.loss < 0.0 || feAzim.loss > 999.99)
                    {
                        ValErrs.AddMess("Loss (%f) must be between 0.00 and 999.99.",
                                        keyLine, "E", feAzim.loss.ToString("F6"));
                        errCount++;
                    }
                }
            } // end while 
            DynFeAzim.FeCloseAzim(azimHandle);

        }	// ----- End of feValAzimIntra ----- 







    }
}

```
