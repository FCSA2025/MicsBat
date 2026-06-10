# Documented File: ValAnte.cs
**Repository Path:** `FeValidate\ValAnte.cs`
**Primary Layer:** `FeValidate`
**Namespace:** `FeValidate`

## Source Code Representation
```csharp
﻿using System;
using static System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeValidate
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using _Auxlib;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides methods that validate the antennae information
    /// provided by a previously imported PDF.
    /// </summary>
    public class ValAnte
    {
        private static ScanFormatted sf = new ScanFormatted();

        /// <summary>
        /// This method validates ES antenna records i.a.w. their prescribed
        /// MDB operation (ADD, BLANK-OUT, DELETE, NO-CHANGE, UPDATE).  
        /// </summary>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - number of errors encountered.</param>
        /// <param name="warnCount"> - number of warnings encountered.</param>
        /// <param name="valLevel"> - level of validation.</param>
        public static void FeValAnteES(string pdfName,    // pdf display name 
                                 ref short errCount,   // number of errors encountered 
                                 ref short warnCount,  // number of warnings encountered 
                                 ref short valLevel)   // level of validation 
        {
            SQLLEN[] nArrayFW;
            int aID1;
            string whereClause = "";
            string tableName;
            string keyLine = "";
            FeAnte feAnte;

            GenUtil.UtCvtName(Constant.FE_ANTE, pdfName, out tableName);

            // read antenna information 
            if ((aID1 = DynFeAnte.FeSelectAnte(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read antenna information. Reason %d", tableName, "W", aID1.ToString());
                warnCount++;
                return;
            }

            while (DynFeAnte.FeFetchAnte(aID1, out feAnte, out nArrayFW) == Constant.SUCCESS)
            {
                keyLine = String.Format("Antenna: {0} {1}", feAnte.location, feAnte.call1);

                switch (feAnte.cmd[0])
                {
                    case 'A':
                        FeValAnteAdd(feAnte, nArrayFW, pdfName,
                                                 ref errCount, ref warnCount, ref valLevel, keyLine);
                        break;

                    case 'D':
                        FeValAnteDel(feAnte, pdfName, ref errCount, ref warnCount, keyLine);
                        break;

                    case 'U':
                    case 'B':
                        FeValAnteUpdt(feAnte, nArrayFW, pdfName, ref errCount, ref warnCount,
                                      ref valLevel, keyLine);
                        break;

                    case 'N':
                        FeValAnteNoop(feAnte, ref errCount, ref warnCount, keyLine, pdfName);
                        break;

                    default:
                        ValErrs.AddMess("Command (%s) is invalid.", keyLine, "E", feAnte.cmd);
                        errCount++;
                        break;
                }
                DynFeAnte.FeUpdateAnte(aID1, feAnte, nArrayFW);
            }
            DynFeAnte.FeCloseAnte(aID1);
        }

        /// <summary>
        /// This method validates ES PDF antenna records whose MDB operation is "ADD".  
        /// </summary>
        /// <remarks>
        /// This procedure performs the antenna validation for an ADD record. It 
        /// verifies that the record does not already exist and then calls lower-level 
        /// methods that verify that all fields have the correct format/syntax and
        /// numerical bounds.  
        /// </remarks>
        /// <param name="feAnte"> - antenna information.</param>
        /// <param name="nArrayFW"> - null flags for feAnte.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="valLevel"> - level of Validation.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValAnteAdd(FeAnte feAnte,    // ante info 
                                                 SQLLEN[] nArrayFW, // null flags 
                                                 string pdfName,    // pdf display name 
                                                 ref short errCount,    // number of errors 
                                                 ref short warnCount,   // number of warnings 
                                                 ref short valLevel,    // level of Validation 
                                                 string keyLine)    // key info to print 
        {
            SQLLEN[] nArrayMDB;
            bool valAcodeTx;
            bool valAcodeRx;
            int rc;
            string whereClause;
            MeAnte meAnte;
            string cLocation = "";
            string cName = "";
            string cProv = "";
            string cOper = "";

            /*	We cannot add an antenna when the call sign already exists, even if it
            *		does not exist in the current site. GJS (1101) */
            if (MeUtils.MeGetLocByCall(feAnte.call1, out cLocation, out cName, out cProv, out cOper) == 0)
            {
                /*	This call sign already exists in the database -- can't add */
                ValErrs.AddMess("Call Sign is already used in Location %s (%s, %s by %s)",
                             keyLine, "W", cLocation.Trim(), cName.Trim(),
                             cProv, cOper.Trim());
                warnCount++;
            }

            // make sure antenna does not already exist in database 
            ValImport.FeFormWhereClause(out whereClause, feAnte.location, feAnte.call1, "");

            rc = (short)ValRetrieveES.FeValRetrieveMDBAntenna(out meAnte, whereClause, out nArrayMDB);
            switch (rc)
            {
                case Constant.SUCCESS:
                    ValErrs.AddMess("Antenna already exists in MDB, cannot ADD", keyLine, "E");
                    errCount++;
                    break;

                case Constant.NOMORERECS:
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Antenna record locked in MDB", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Antenna undefined error: %d", keyLine, "W", rc.ToString());
                    warnCount++;
                    break;
            }

            FeValAnteFields(feAnte, nArrayFW, pdfName, ref errCount, ref warnCount,
                              ref valLevel, out valAcodeTx, out valAcodeRx, keyLine);

            if (errCount == 0)
            {
                FeCalcAnteFields(feAnte, nArrayFW, valAcodeTx, valAcodeRx,
                                     pdfName, ref warnCount, keyLine);
            }

            if (FeCheckChanExist(feAnte, ref warnCount, keyLine, pdfName) == Constant.FALSE)
            {
                ValErrs.AddMess("This antenna will no longer have associated channels",
                                keyLine, "W");
                warnCount++;
            }
        }

        /// <summary>
        /// This method validates ES PDF antenna records whose MDB operation is "DELETE".  
        /// </summary>
        /// <remarks>
        /// This procedure performs the antenna validation for a DELETE record. It 
        /// verifies that the record does indeed exist, and retrieves all information 
        /// below the antenna record (i.e. azimuths, channels) to delete as well as all information pointing to the 
        /// antenna record.   
        /// </remarks>
        /// <param name="feAnte"> - antenna information.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValAnteDel(FeAnte feAnte,    // ante info 
                                                         string pdfName,    // pdf display name 
                                                         ref short errCount,    // number of errors 
                                                         ref short warnCount,   // number of warnings 
                                                         string keyLine)    // key info to print 
        {
            SQLLEN[] nArrayTemp;
            SQLLEN[] nArrayAz;
            SQLLEN[] nArrayMDB;
            short rc;
            string whereClause;
            string tableName;
            int ID1;
            FeChan feChan;
            FeAzim feAzim;
            MeAnte meAnte;

            ValImport.FeFormWhereClause(out whereClause, feAnte.location, feAnte.call1, "");

            if ((!String.IsNullOrWhiteSpace(feAnte.recstat)) && (feAnte.recstat[0] != 'C'))
            {
                // if antenna is user generated, make sure it exists in MDB 
                rc = (short)ValRetrieveES.FeValRetrieveMDBAntenna(out meAnte, whereClause, out nArrayMDB);
                switch (rc)
                {
                    case Constant.SUCCESS:
                        break;

                    case Constant.NOMORERECS:
                        ValErrs.AddMess("Antenna does not exist in MDB", keyLine, "E");
                        errCount++;
                        break;

                    case Constant.RECLOCK:
                        ValErrs.AddMess("Antenna record locked in MDB", keyLine, "W");
                        warnCount++;
                        break;

                    default:
                        ValErrs.AddMess("Antenna undefined error: %d", keyLine, "W", rc.ToString());
                        warnCount++;
                        break;
                }
            }

            // delete channel information 
            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out tableName);

            ValImport.FeFormWhereClause(out whereClause, feAnte.location, feAnte.call1, "");

            if ((ID1 = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read channel information. Reason %d", keyLine, "W", ID1.ToString());
                warnCount++;
            }
            else
            {
                //feChan.cmd[0] = 0;

                while (DynFeChan.FeFetchChan(ID1, out feChan, out nArrayTemp) == Constant.SUCCESS)
                {
                    if ((feChan.cmd[0] != 'N') && (feChan.cmd[0] != 'D'))
                    {
                        ValErrs.AddMess("Cannot delete channel if being Added or Updated",
                               keyLine, "E");
                        errCount++;
                    }
                    else
                    {
                        feChan.cmd = "D";
                        DynFeChan.FeUpdateChan(ID1, feChan, nArrayTemp);
                    }
                }
                DynFeChan.FeCloseChan(ID1);
            }

            // delete azimuth information 
            GenUtil.UtCvtName(Constant.FE_AZIM, pdfName, out tableName);
            if ((ID1 = DynFeAzim.FeSelectAzim(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read azimuth information. Reason %d",
                                keyLine, "W", ID1.ToString());
                warnCount++;
            }
            else
            {
                while (DynFeAzim.FeFetchAzim(ID1, out feAzim, out nArrayAz) == Constant.SUCCESS)
                {
                    /*
                     * TASK 503, 511: Check the command field for the azimuths rather
                     * than the channels here. The channels are already checked
                     * above.
                     */
                    if ((feAzim.cmd[0] != 'N') && (feAzim.cmd[0] != 'D'))
                    {
                        ValErrs.AddMess("Cannot delete azimuth if being added or updated",
                                        keyLine, "E");
                        errCount++;
                    }
                    else
                    {
                        feAzim.cmd = "D";
                        DynFeAzim.FeUpdateAzim(ID1, feAzim, nArrayAz);
                    }
                }
                DynFeAzim.FeCloseAzim(ID1);
            }
        }

        /// <summary>
        /// This method validates ES PDF antenna records whose MDB operation is "UPDATE".  
        /// </summary>
        /// <remarks>
        /// This procedure performs the antenna validation for an ADD record. It 
        /// verifies that the record does indeed already exist and then calls lower-level 
        /// methods that verify that all fields have the correct format/syntax and
        /// numerical bounds.  
        /// </remarks>
        /// <param name="feAnte"> - antenna information.</param>
        /// <param name="nArrayFW"> - null flags for feAnte.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="valLevel"> - level of Validation.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValAnteUpdt(FeAnte feAnte,   // ante info 
                                                            SQLLEN[] nArrayFW,  // array with null flags 
                                                            string pdfName, // pdf display name 
                                                            ref short errCount, // number of errors 
                                                            ref short warnCount,    // number of warnings 
                                                            ref short valLevel, // level of Validation 
                                                            string keyLine) // key info to print 
        {
            short rc;
            bool valAcodeTx;
            bool valAcodeRx;
            SQLLEN[] nArrayMDB;
            string whereClause;
            MeAnte meAnte;

            // make sure antenna does exist 
            ValImport.FeFormWhereClause(out whereClause, feAnte.location, feAnte.call1, "");

            rc = (short)ValRetrieveES.FeValRetrieveMDBAntenna(out meAnte, whereClause, out nArrayMDB);
            switch (rc)
            {
                case Constant.SUCCESS:
                    break;

                case Constant.NOMORERECS:
                    ValErrs.AddMess("Antenna does not exist in MDB", keyLine, "E");
                    errCount++;
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Antenna record locked in MDB", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Antenna undefined error: %d", keyLine, "W", rc.ToString());
                    warnCount++;
                    break;
            }

            if (rc != Constant.SUCCESS)
            {
                /* if we couldn't properly retrieve this record
             * we don't want to continue validating	  */
                return;
            }

            // Check modify date and time 
            if (((!feAnte.mdate.Equals(meAnte.mdate))) ||
                 ((!feAnte.mtime.Equals(meAnte.mtime))))
            {
                // Modify dates/times don't match 
                ValErrs.AddMess("This Antenna record has been modified since you retrieved it.",
                                keyLine, "W");
                warnCount++;
            }

            FeValAnteFields(feAnte, nArrayFW, pdfName, ref errCount, ref warnCount,
                              ref valLevel, out valAcodeTx, out valAcodeRx, keyLine);

            if (errCount == 0)
            {
                FeCalcAnteFields(feAnte, nArrayFW, valAcodeTx, valAcodeRx,
                                     pdfName, ref warnCount, keyLine);
            }

            if (FeCheckChanExist(feAnte, ref warnCount, keyLine, pdfName) == Constant.FALSE)
            {
                ValErrs.AddMess("This antenna will no longer have associated channels",
                                keyLine, "W");
                warnCount++;
            }
        }

        /// <summary>
        /// This method attempts to fetch an antenna record from the MDB and reports an 
        /// error message if no record exists; it checks that any ficticious call sign
        /// (prefixed by '$' or '=') is legal and reports a warning if this antenna will no 
        /// longer have any associated channels.
        /// </summary>
        /// <param name="feAnte"> - antenna information.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        public static void FeValAnteNoop(FeAnte feAnte,   // ante info 
                                                            ref short errCount, // number of errors 
                                                            ref short warnCount,    // number of warnings 
                                                            string keyLine,     // key info to print 
                                                            string pdfName) // pdfName 
        {
            short rc;
            SQLLEN[] nArrayMDB;
            string whereClause;
            MeAnte meAnte;

            // make sure antenna does exist 
            ValImport.FeFormWhereClause(out whereClause, feAnte.location, feAnte.call1, "");

            rc = (short)ValRetrieveES.FeValRetrieveMDBAntenna(out meAnte, whereClause, out nArrayMDB);
            switch (rc)
            {
                case Constant.SUCCESS:
                    break;

                case Constant.NOMORERECS:
                    ValErrs.AddMess("Antenna does not exist in MDB", keyLine, "E");
                    errCount++;
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Antenna record locked in MDB", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Antenna undefined error: %d", keyLine, "W", rc.ToString());
                    warnCount++;
                    break;
            }

            // check to be sure ficticious call sign is legal 
            if (feAnte.call1[0] == '$')
            {
                if ((CheckRXFictCall(pdfName, feAnte.call1) != Constant.SUCCESS) &&
                    (ValidES.FeChkChng(pdfName, feAnte.call1, '=') != Constant.SUCCESS))
                {
                    ValErrs.AddMess("A site that begins with '$' must be receive only",
                                    keyLine, "E");
                    errCount++;
                }
            }

            // check to be sure ficticious call sign is legal 
            if (feAnte.call1[0] == '=')
            {
                if ((CheckTXFictCall(pdfName, feAnte.call1) != Constant.SUCCESS) &&
                    (ValidES.FeChkChng(pdfName, feAnte.call1, '$') != Constant.SUCCESS))
                {
                    ValErrs.AddMess("A call sign that begins with '=' (%s) must have a TX frequency", "", "E", feAnte.call1);
                    errCount++;
                }
            }

            if (FeCheckChanExist(feAnte, ref warnCount, keyLine, pdfName) != Constant.TRUE)
            {
                ValErrs.AddMess("This antenna will no longer have associated channels",
                        keyLine, "W");
                warnCount++;
            }

        }

        /// <summary>
        /// This method provides detailed validation of individual fields in an antenna 
        /// record for the MDB operations of "ADD" and "UPDATE". 
        /// </summary>
        /// <param name="feAnte"> - antenna information.</param>
        /// <param name="nArrayFW"> - null flags for feAnte.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="valLevel"> - level of Validation.</param>
        /// <param name="valAcodeTx"> - boolean indicating that txband and acodetx are valid w.r.t. SDB tables.</param>
        /// <param name="valAcodeRx"> - boolean indicating that rxband and acoderx are valid w.r.t. SDB tables.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValAnteFields(FeAnte feAnte, // ante info 
                                          SQLLEN[] nArrayFW,    // array with null flags 
                                          string pdfName,   // pdf display name 
                                          ref short errCount,   // number of errors 
                                          ref short warnCount,  // number of warnings 
                                          ref short valLevel,   // level of validation 
                                          out bool valAcodeTx,
                                          out bool valAcodeRx,
                                          string keyLine) // key info to print 
        {
            SQLLEN[] nArrayTemp;
            int sID1;
            string whereClause;
            string tableName;
            char fictsign;
            string signroot;
            string operroot;
            short opersize;

            FeSite feSite = new FeSite();

            SuNote tNote;
            int nRet;

            // get operator code from the site table 
            GenUtil.UtCvtName(Constant.FE_SITE, pdfName, out tableName);

            ValImport.FeFormWhereClause(out whereClause, feAnte.location, "", "");

            if ((sID1 = DynFeSite.FeSelectSite(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Cannot read site information :%d:", keyLine, "W", sID1.ToString());
                warnCount++;
            }
            else
            {
                if (DynFeSite.FeFetchSite(sID1, out feSite, out nArrayTemp) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("No site exists for this antenna", keyLine, "E");
                    errCount++;
                }
                DynFeSite.FeCloseSite(sID1);
            }

            feSite.oper = feSite.oper.Trim();
            feAnte.call1 = feAnte.call1.Trim();

            if (Strings.StrnCmp(feSite.oper, feAnte.call1, feSite.oper.Length) == Constant.SUCCESS)
            {
                ValErrs.AddMess("Call Sign (%s) begins with Site Record operator code.\r\nFollow Ficticious Call Sign Rules.",
                           keyLine, "E", feAnte.call1);
                errCount++;
            }

            /* must check that the call sign in the antenna record corresponds
               to the operator code in the site record if the call sign is
               ficticious */
            fictsign = feAnte.call1[0];
            if ((fictsign == '=') ||
                (fictsign == '$') ||
                (fictsign == ';') ||
                    (fictsign == '%'))
            {
                feSite.oper = feSite.oper.Trim();
                opersize = (short)feSite.oper.Length;
                if (opersize > 4)
                {
                    opersize = 4;
                }
                Strings.StrnCpy(out operroot, feSite.oper, opersize);
                Strings.StrnCpy(out signroot, Strings.DropFirstChar(feAnte.call1), opersize);

                if (Strings.StrnCmp(signroot, operroot, opersize) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("Characters following %c in Call Sign must agree with oper code field",
                                    keyLine, "E", feAnte.call1[0].ToString());
                    errCount++;
                }
            }

            // validate nota against snote table (nonum) 
            if (nArrayFW[FeAnte.NOTA] != Constant.DB_NULL && feAnte.nota.Length > 0)
            {
                nRet = Suutils.SuGetNote(feSite.oper, feAnte.nota, out tNote);
                if (nRet != 0)
                {
                    ValErrs.AddMess("Note (%s) not present in Note table for Operator :%s:",
                           keyLine, "E", feAnte.nota, feSite.oper);
                    errCount++;
                }
            }

            // validate rxband and acoderx against sband table (bndcde) 
            valAcodeRx = ValAcode(feAnte.acoderx, nArrayFW[FeAnte.ACODERX],
                                                         feAnte.rxband, nArrayFW[FeAnte.RXBAND], false,
                                                         ref errCount, ref valLevel, keyLine);

            // validate txband and acodetx against sband table (bndcde) 
            valAcodeTx = ValAcode(feAnte.acodetx, nArrayFW[FeAnte.ACODETX],
                                                         feAnte.txband, nArrayFW[FeAnte.TXBAND], true,
                                                         ref errCount, ref valLevel, keyLine);

            if ((feAnte.cmd.Equals("A")) || (feAnte.cmd.Equals("B")))
            {
                if (nArrayFW[FeAnte.AHT] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Antenna Height must be present", keyLine, "E");
                    errCount++;
                }

                if ((nArrayFW[FeAnte.SATLONG] == Constant.DB_NULL) ||
                    (nArrayFW[FeAnte.SATLONGS] == Constant.DB_NULL))
                {
                    ValErrs.AddMess("Satellite Longitude and Sense must be present", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeAnte.SARC1] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Arc Orbit Center must be present", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeAnte.SARC2] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Arc Half Width must be present", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeAnte.OP2] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Satellite Operator must be present", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeAnte.STATA] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Status must be present", keyLine, "E");
                    errCount++;
                }

                if ((nArrayFW[FeAnte.TXBAND] == Constant.DB_NULL) &&
                    (nArrayFW[FeAnte.RXBAND] == Constant.DB_NULL))
                {
                    ValErrs.AddMess("TX and/or RX data must be present", keyLine, "E");
                    errCount++;
                    return;
                }

                if (nArrayFW[FeAnte.RXBAND] != Constant.DB_NULL)
                {
                    // all RX must be present 
                    if (nArrayFW[FeAnte.ACODERX] == Constant.DB_NULL)
                    {
                        ValErrs.AddMess("RX Antenna Code must be present", keyLine, "E");
                        errCount++;
                    }
                    if (nArrayFW[FeAnte.AFSLR] == Constant.DB_NULL)
                    {
                        ValErrs.AddMess("RX AFSL must be present", keyLine, "E");
                        errCount++;
                    }
                    if (nArrayFW[FeAnte.RXPRE] == Constant.DB_NULL)
                    {
                        ValErrs.AddMess("RX Precipitation Scatter Dist. must be present",
                               keyLine, "E");
                        errCount++;
                    }
                    if (nArrayFW[FeAnte.RXTRO] == Constant.DB_NULL)
                    {
                        ValErrs.AddMess("RX Tropo. Scatter Dist. must be present", keyLine, "E");
                        errCount++;
                    }
                }
                else
                {
                    // all RX must not be present 
                    if (nArrayFW[FeAnte.ACODERX] != Constant.DB_NULL)
                    {
                        ValErrs.AddMess("RX Antenna Code invalid without all RX data", keyLine, "E");
                        errCount++;
                    }
                    if (nArrayFW[FeAnte.AFSLR] != Constant.DB_NULL)
                    {
                        ValErrs.AddMess("RX AFSL invalid without all RX data", keyLine, "E");
                        errCount++;
                    }
                    if (nArrayFW[FeAnte.RXPRE] != Constant.DB_NULL)
                    {
                        ValErrs.AddMess("RX Precip. Scatter Dist. invalid without all RX data",
                                        keyLine, "E");
                        errCount++;
                    }
                    if (nArrayFW[FeAnte.RXTRO] != Constant.DB_NULL)
                    {
                        ValErrs.AddMess("RX Tropo. Scatter Dist. invalid without all RX data",
                                        keyLine, "E");
                        errCount++;
                    }
                }

                if (nArrayFW[FeAnte.TXBAND] != Constant.DB_NULL)
                {
                    // all TX must be present 
                    if (nArrayFW[FeAnte.ACODETX] == Constant.DB_NULL)
                    {
                        ValErrs.AddMess("TX Antenna Code must be present", keyLine, "E");
                        errCount++;
                    }
                    if (nArrayFW[FeAnte.AFSLT] == Constant.DB_NULL)
                    {
                        ValErrs.AddMess("TX AFSL must be present", keyLine, "E");
                        errCount++;
                    }
                    if (nArrayFW[FeAnte.TXPRE] == Constant.DB_NULL)
                    {
                        ValErrs.AddMess("TX Precip. Scatter Dist. must be present", keyLine, "E");
                        errCount++;
                    }
                    if (nArrayFW[FeAnte.TXTRO] == Constant.DB_NULL)
                    {
                        ValErrs.AddMess("TX Tropo. Scatter Dist. must be present", keyLine, "E");
                        errCount++;
                    }
                }
                else
                {
                    // all TX must not be present 
                    if (nArrayFW[FeAnte.ACODETX] != Constant.DB_NULL)
                    {
                        ValErrs.AddMess("TX Antenna Code invalid without all TX data", keyLine, "E");
                        errCount++;
                    }
                    if (nArrayFW[FeAnte.AFSLT] != Constant.DB_NULL)
                    {
                        ValErrs.AddMess("TX AFSL invalid without all TX data", keyLine, "E");
                        errCount++;
                    }
                    if (nArrayFW[FeAnte.TXPRE] != Constant.DB_NULL)
                    {
                        ValErrs.AddMess("TX Precip. Scatter Dist. invalid without all TX data",
                                        keyLine, "E");
                        errCount++;
                    }
                    if (nArrayFW[FeAnte.TXTRO] != Constant.DB_NULL)
                    {
                        ValErrs.AddMess("TX Tropo. Scatter Dist. invalid without all TX data",
                                        keyLine, "E");
                        errCount++;
                    }
                }
            }

            // check to be sure ficticious call sign is legal 
            if (feAnte.call1[0] == '$')
            {
                if ((CheckRXFictCall(pdfName, feAnte.call1) != Constant.SUCCESS) &&
                    (ValidES.FeChkChng(pdfName, feAnte.call1, '=') != Constant.SUCCESS))
                {
                    ValErrs.AddMess("A site that begins with '$' must be receive only",
                                    feAnte.call1, "E");
                    errCount++;
                }
            }

            // check to be sure ficticious call sign is legal 
            if (feAnte.call1[0] == '=')
            {
                if ((CheckTXFictCall(pdfName, feAnte.call1) != Constant.SUCCESS) &&
                    (ValidES.FeChkChng(pdfName, feAnte.call1, '$') != Constant.SUCCESS))
                {
                    ValErrs.AddMess("A call sign (%s) that begins with '=' must have a TX frequency.",
                    keyLine, "E", feAnte.call1);
                    errCount++;
                }
            }
        }

        /// <summary>
        /// This method validates an antenna code and a band code w.r.t. the SDB tables,
        /// ensures that the Rx and Tx codes are in the  correct position and that the 
        /// Rx and Tx codes have the correct format/syntax and that are rxband, acoderx, 
        /// txband, acodetx sre valid w.r.t. the SDB tables.
        /// </summary>
        /// <param name="acode"> - an antenna code.</param>
        /// <param name="acodeNull"> - ODBC nullInd for acode.</param>
        /// <param name="band"> - a band code.</param>
        /// <param name="bandNull"> - ODBC nullInd for band.</param>
        /// <param name="isTX"> - boolean to indicate if acode and band relate to a transmitter.</param>
        /// <param name="errCount"> - cummulative count of errors.</param>
        /// <param name="valLevel"> - depth of validation.</param>
        /// <param name="keyLine"> - key info to be printed.</param>
        /// <returns></returns>
        public static bool ValAcode(string acode,
                                            SQLLEN acodeNull,
                                            string band,
                                            SQLLEN bandNull,
                                            bool isTX,
                                            ref short errCount,
                                            ref short valLevel,
                                            string keyLine)
        {
            SuBand suBand;
            int nRet;

            string FWband;

            if (acodeNull != Constant.DB_NULL && acode.Length > 0)
            {
                FWband = band;

                if (bandNull != Constant.DB_NULL && band.Length > 0)
                {
                    nRet = Suutils.SuGetBand(band, out suBand);

                    if (nRet != 0)
                    {
                        if (isTX == true)
                        {
                            ValErrs.AddMess("TXBand (%s) not present in Band table.", keyLine, "E", band);
                        }
                        else
                        {
                            ValErrs.AddMess("RXBand (%s) not present in Band table.", keyLine, "E", band);
                        }
                        errCount++;
                    }
                }
                else
                {
                    if (isTX == true)
                    {
                        ValErrs.AddMess("Must enter a value for txband.", keyLine, "E");
                    }
                    else
                    {
                        ValErrs.AddMess("Must enter a value for rxband.", keyLine, "E");
                    }
                    errCount++;
                }

                if (acode.StartsWith("$"))
                {
                    // first character of acode is $ 
                    valLevel = Constant.TSIP;
                }
                else if (Strings.StrnCmp(acode, "CCIR", 4) != 0)
                {
                    /*not CCIR and first char is not $)*/
                    SuAntStr suAntStr;
                    nRet = Suutils.SuGetAnt(acode, out suAntStr);

                    if (nRet != 0)
                    {
                        if (isTX == true)
                        {
                            ValErrs.AddMess("TX Antenna Code (%s) not present in the Antenna table.",
                                            keyLine, "E", acode);
                        }
                        else
                        {
                            ValErrs.AddMess("RX Antenna Code (%s) not present in the Antenna table.",
                                            keyLine, "E", acode);
                        }
                        errCount++;
                        return (false);
                    }

                }
                else
                {
                    // CCIR  - pos 5-6 must be rxgain, 7-8 must be rxgain
                    if (!Strings.IsDigit(acode[4]))
                    {
                        if (isTX == true)
                        {
                            ValErrs.AddMess("Antenna Code TX position 5 must be a numeric if CCIR: %s.",
                                            keyLine, "E", acode);
                        }
                        else
                        {
                            ValErrs.AddMess("Antenna Code RX position 5 must be a numeric if CCIR: %s.",
                                            keyLine, "E", acode);
                        }
                        errCount++;
                    }
                    if (!Strings.IsDigit(acode[5]))
                    {
                        if (isTX == true)
                        {
                            ValErrs.AddMess("Antenna Code TX position 6 must be a numeric if CCIR: %s.",
                                            keyLine, "E", acode);
                        }
                        else
                        {
                            ValErrs.AddMess("Antenna Code RX position 6 must be a numeric if CCIR: %s.",
                                            keyLine, "E", acode);
                        }
                        errCount++;
                    }
                    if (!Strings.IsDigit(acode[6]))
                    {
                        if (isTX == true)
                        {
                            ValErrs.AddMess("Antenna Code TX position 7 must be a numeric if CCIR: %s.",
                                            keyLine, "E", acode);
                        }
                        else
                        {
                            ValErrs.AddMess("Antenna Code RX position 7 must be a numeric if CCIR: %s.",
                                            keyLine, "E", acode);
                        }
                        errCount++;
                    }
                    if (!Strings.IsDigit(acode[7]))
                    {
                        if (isTX == true)
                        {
                            ValErrs.AddMess("Antenna Code TX position 8 must be a numeric if CCIR: %s.",
                                            keyLine, "E", acode);
                        }
                        else
                        {
                            ValErrs.AddMess("Antenna Code RX position 8 must be a numeric if CCIR: %s.",
                                            keyLine, "E", acode);
                        }
                        errCount++;
                    }
                }
                return (true);
            }
            return (false);
        }

        /// <summary>
        /// This method determines whether an antenna record with a '$' prefix on 
        /// its call1 field has a transmitting antenna; this would be invalid
        /// because '$' indicates a receive-only antenna. 
        /// </summary>
        /// <param name="pdfName"></param>
        /// <param name="call1"> - ante call1 info</param>
        /// <returns></returns>
        public static int CheckRXFictCall(string pdfName, string call1) // ante call1 info 
        {
            string anteCall1;
            string tableName;

            FeChan tempChan;
            SQLLEN[] nullIndChan;
            int chanHandle;
            string whereClause;
            int ret = Constant.SUCCESS; // assume all's well unless otherwise proven 

            anteCall1 = call1;
            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out tableName);

            /* check to see if a TX frequency is being added
               this would mean that a '$' call sign is not allowed */

            whereClause = String.Format("call1 = '{0}'", anteCall1);

            if ((chanHandle = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
            {
                return (Error.DYN_MS_SQL_SERVER_ERR);
            }

            while (DynFeChan.FeFetchChan(chanHandle, out tempChan, out nullIndChan) == Constant.SUCCESS)
            {
                if (nullIndChan[FeChan.FREQTX] == Constant.DB_NOT_NULL)
                {
                    ret = Constant.FAILURE; // an error will be reported 
                }
            }
            DynFeChan.FeCloseChan(chanHandle);

            return (ret); /* return the status of the check
			if it is a failure, there is a problem
			with the ficticious call signs */
        }

        /// <summary>
        /// This method determines whether an antenna record with a '=' prefix on 
        /// its call1 field has a transmitting antenna; if it does not, this would be invalid
        /// because '=' indicates that an antenna is transmit-only or transmit-receive. 
        /// </summary>
        /// <param name="pdfName"> - name of a PDF.</param>
        /// <param name="call1"> - antenna calsign.</param>
        /// <returns></returns>
        public static int CheckTXFictCall(string pdfName, string call1)
        {
            int ret;        // return code 
            int rc;
            int pdfcheck;   // check within PDFs to see if channel has changed 
            int addcheck;   // check to see if channel record exists in PDF 
            int count = 0;  // counter to see if channel record exists in MDB 
            int chanHandle; // dynamic cursor handles 
            int meChanHandle;
            FeChan tempChan;        // temporary structures 
            SQLLEN[] nullIndChan;
            string tableName;
            MeChan meChan;
            SQLLEN[] meChanNulls;

            string antecall1;
            string whereClause;
            string selectionCriteria;

            ret = Constant.FAILURE;     // set return code to failure ...code must 
            pdfcheck = Constant.FAILURE;    // verify that this condition does not exist 
            addcheck = Constant.FAILURE;

            antecall1 = call1;
            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out tableName);

            /* check to see if a TX frequency is being added
               this would mean that a '=' call sign is allowed */

            whereClause = String.Format("call1 = '{0}' and (cmd = '{1}' or cmd = '{2}')",
                                  antecall1, "A", "B");

            if ((chanHandle = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
            {
                return (Error.DYN_MS_SQL_SERVER_ERR);
            }

            while (DynFeChan.FeFetchChan(chanHandle, out tempChan, out nullIndChan) == Constant.SUCCESS)
            {
                addcheck = Constant.SUCCESS;
                if (nullIndChan[FeChan.FREQTX] != Constant.DB_NULL)
                {
                    ret = Constant.SUCCESS; // no error will be reported 
                }
            }
            DynFeChan.FeCloseChan(chanHandle);


            /* if no channel is being added or blanked, check MDB for channels
               that exist already that have a TX frequency */

            selectionCriteria = String.Format("call1 = '{0}'", antecall1);

            if (ret != Constant.SUCCESS)
            {
                if ((meChanHandle = DynMeChan.MeSelectChan(selectionCriteria, "")) < 0)
                {
                    // error selecting mdb ante table 
                    return (meChanHandle);
                }

                while ((rc = DynMeChan.MeFetchChan(meChanHandle, out meChan, out meChanNulls)) == Constant.SUCCESS)
                {
                    count = 1;
                    /* this counter identifies that there is at
                       least one channel record */

                    pdfcheck = Constant.FAILURE;

                    if (meChanNulls[MeChan.FREQTX] != Constant.DB_NULL)
                    {
                        /* found a TX frequency - now make sure that
                           it will stay a TX frequency after the mdb
                           is updated (ie. is there a delete or blank
                           record in the PDF) */

                        pdfcheck = Constant.SUCCESS;
                        // PDF does not change MDB record 

                        whereClause = String.Format("call1 = '{0}' and call1 = '{1}' and chid = '{2}' and cmd = '{3}'",
                                meChan.call1, meChan.call1, meChan.chid, "D");
                        // check for record deletions in the PDF 

                        if ((chanHandle = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
                        {
                            return (Error.DYN_MS_SQL_SERVER_ERR);
                        }

                        while (DynFeChan.FeFetchChan(chanHandle, out tempChan, out nullIndChan) == Constant.SUCCESS)
                        {
                            pdfcheck = Constant.FAILURE;
                            // PDF changes MDB record 
                        }

                        DynFeChan.FeCloseChan(chanHandle);


                        whereClause = String.Format("call1 = '{0}' and call1 = '{1}' and chid = '{2}' and cmd = '{3}'",
                                             meChan.call1, meChan.call1, meChan.chid, "B");
                        // check for record blanks in the MDB 

                        if ((chanHandle = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
                        {
                            return (Error.DYN_MS_SQL_SERVER_ERR);
                        }

                        while (DynFeChan.FeFetchChan(chanHandle, out tempChan, out nullIndChan) == Constant.SUCCESS)
                        {
                            if (nullIndChan[FeChan.FREQTX] == Constant.DB_NULL)
                            {
                                pdfcheck = Constant.FAILURE;
                                // PDF changes MDB 
                            }
                        }

                        DynFeChan.FeCloseChan(chanHandle);
                    }
                    else
                    {
                        /* 	did not find TX frequency - now make sure that it stays a non-TX
                        *		record after the mdb is updated (ie. is there an update or blank
                        * 	record in the PDF) */
                        whereClause = String.Format("call1 = '{0}' and call1 = '{1}' and chid = '{2}' and cmd = '{3}'",
                                             meChan.call1, meChan.call1, meChan.chid, "U");

                        if ((chanHandle = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
                        {
                            return (Error.DYN_MS_SQL_SERVER_ERR);
                        }

                        while (DynFeChan.FeFetchChan(chanHandle, out tempChan, out nullIndChan) == Constant.SUCCESS)
                        {
                            if (nullIndChan[FeChan.FREQTX] != Constant.DB_NULL)
                            {
                                pdfcheck = Constant.SUCCESS;
                            }
                        }

                        DynFeChan.FeCloseChan(chanHandle);

                        whereClause = String.Format("call1 = '{0}' and call1 = '{1}' and chid = '{2}' and cmd = '{3}'",
                                             meChan.call1, meChan.call1, meChan.chid, "B");

                        if ((chanHandle = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
                        {
                            return (Error.DYN_MS_SQL_SERVER_ERR);
                        }

                        while (DynFeChan.FeFetchChan(chanHandle, out tempChan, out nullIndChan) == Constant.SUCCESS)
                        {
                            if (nullIndChan[FeChan.FREQTX] != Constant.DB_NULL)
                            {
                                pdfcheck = Constant.SUCCESS;
                            }
                        }

                        DynFeChan.FeCloseChan(chanHandle);
                    }
                    if (pdfcheck == Constant.SUCCESS)
                    {
                        /* the MDB stays a TX record or becomes a TX record after the update is
                           performed */
                        ret = Constant.SUCCESS;
                    }
                }
                DynMeChan.MeCloseChan(meChanHandle);

                if ((count == 0) && (addcheck != Constant.SUCCESS))
                {
                    ret = Constant.SUCCESS; // no channels being added its just a ante and/or antenna 
                }
            }

            return (ret); /* return the status of the check if it is a failure, there is a problem
			with the ficticious call signs */
        }   // ----- End of FtRecExist.CheckFictCall ----- 

        /// <summary>
        /// This method ensures that channels will exist for the prescribed antenna
        /// after this PDF is run; see the additional remarks.
        /// </summary>
        /// <remarks>
        /// This method performs a 'look ahead' analysis to ensure that there will be channels 
        /// associated with this antenna after the PDF has been successfully validated:
        /// <list type="bullet">
        /// <item>First check to see if any channels in the PDF are A,U,B or D. This would mean that there will 
        /// be channels after the PDF has been run. </item>
        /// <item>If there are 'D' channels in the PDF, go through the MDB and for each channel 
        /// record see if it is not marked as 'D'. Stop as soon as you find one case where 
        /// the MDB record is not being deleted in the PDF. </item>
        /// <item>If all referenced MDB channel records are being deleted then issue a 
        /// warning that this antenna will be no channels after the PDF has run.</item>
        /// </list>
        /// </remarks>
        /// <param name="feAnte"> - antenna information.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <returns></returns>
        public static int FeCheckChanExist(FeAnte feAnte, // ante info 
                                                    ref short warnCount,        // warning count 
                                                    string keyLine,
                                                    string pdfName)     // pdfName 
        {
            string whereClause;
            string MDBClause;
            string tableName;
            int chanHandle,
                        mdbHandle;
            FeChan feChan;
            SQLLEN[] pdfArrayTemp;
            MeChan meChan;
            SQLLEN[] mdbArrayTemp;
            int ret;    /* return code true -> there are channels
				       false -> there are no channels */

            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out tableName);

            ret = Constant.FALSE; // by default there are no channels 

            ValImport.FeFormWhereClause(out whereClause, feAnte.location, feAnte.call1, "");

            if ((chanHandle = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read channel information. Reason: %d", keyLine, "W", chanHandle.ToString());
                warnCount++;
            }
            else
            {
                while (DynFeChan.FeFetchChan(chanHandle, out feChan, out pdfArrayTemp) == Constant.SUCCESS)
                {
                    if (feChan.cmd[0] != 'D')
                    {
                        /* go through the PDF and if you find at least
                           one case where there is a non-delete channel
                           set the return code to true (there will be
                           channels after the PDF is run )
                        */
                        ret = Constant.TRUE;
                    }
                }
                DynFeChan.FeCloseChan(chanHandle);
            }

            if (ret != Constant.TRUE) // if we know there will be channels don't bother 
            {
                MDBClause = String.Format("location = '{0}' and call1 = '{1}'",
                                    feAnte.location, feAnte.call1);
                if ((mdbHandle = DynMeChan.MeSelectChan(MDBClause, "")) < 0)
                {
                    ValErrs.AddMess("Could not read channel from the mdb. Reason %s",
                                    keyLine, "W", mdbHandle.ToString());
                    warnCount++;
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }
                else
                {
                    while ((DynMeChan.MeFetchChan(mdbHandle, out meChan, out mdbArrayTemp) == Constant.SUCCESS) &&
                             (ret != Constant.TRUE))
                    {
                        /* loop through each mdb channel record
                           until we determine that there will be
                           channels after PDF is run (ret = true)
                        */

                        ValImport.FeFormWhereClause(out whereClause, meChan.location, meChan.call1, meChan.chid);
                        if ((chanHandle = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
                        {
                            ValErrs.AddMess("Could not select channel from mdb. Reason: %d",
                                            tableName, "W", chanHandle.ToString());
                            warnCount++;
                        }
                        else
                        {
                            if (DynFeChan.FeFetchChan(chanHandle, out feChan, out pdfArrayTemp) == Constant.SUCCESS)
                            {
                                /* Once we get the mdb channel record we look in the PDF to
                                   find the corresponding rec.  If it doesn't exist then
                                   there will be a chan rec.  After the PDF is run.  If it
                                   does exist then it will exist as long as the PDF rec is not
                                   marked as Delete
                                */
                                if (feChan.cmd[0] != 'D')
                                {
                                    // chan will exist 
                                    ret = Constant.TRUE;
                                }
                            }
                            else
                            {
                                // chan will exist 
                                ret = Constant.TRUE;
                            }
                        }
                        DynFeChan.FeCloseChan(chanHandle);
                    }
                    DynMeChan.MeCloseChan(mdbHandle);
                }
            }

            return (ret);
        }

        /// <summary>
        /// This method populates these antenna record fields: 
        /// az, el, satlongit, rxhgmax and txhgmax.    
        /// </summary>
        /// <param name="feAnte"> - antenna information.</param>
        /// <param name="nArrayFW"> - null flags for feAnte.</param>
        /// <param name="valAcodeTx"> - boolean indicating that txband and acodetx are valid w.r.t. SDB tables.</param>
        /// <param name="valAcodeRx"> - boolean indicating that rxband and acoderx are valid w.r.t. SDB tables.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeCalcAnteFields(FeAnte feAnte,    // pdf ante struct
                                                     SQLLEN[] nArrayFW,     // null indicators 
                                                     bool valAcodeTx,
                                                     bool valAcodeRx,
                                                     string pdfName,        // pdf dipslay name
                                                     ref short warnCount,       // number or warnings 
                                                     string keyLine)        // key info to print 
        {

            SQLLEN[] nArrayTemp;
            double elevAng;
            double azim;
            double rtElevAng;
            int sID1;
            string tableName;
            string whereClause;
            string units;
            FeSite feSite = new FeSite();

            AxStation site = new AxStation();
            AxStation satellite = new AxStation();

            units = "M";

            GenUtil.UtCvtName(Constant.FE_SITE, pdfName, out tableName);

            ValImport.FeFormWhereClause(out whereClause, feAnte.location, "", "");

            // get site information 
            if ((sID1 = DynFeSite.FeSelectSite(tableName, whereClause, "")) < 0)
            {
                warnCount++;
                ValErrs.AddMess("Cannot read site for this antenna :%d:",
                       keyLine, "W", sID1.ToString());
            }
            else
            {
                if (DynFeSite.FeFetchSite(sID1, out feSite, out nArrayTemp) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }
                DynFeSite.FeCloseSite(sID1);
            }

            // fill in site structure 
            site.elevM = feSite.grnd;
            site.antHtM = feAnte.aht;

            LoadSite(feSite.latit, feSite.longit, ref site);

            // fill in the satellite structure 
            satellite.LL.longSeconds = feAnte.satlongit / 100;

            // calculate azim, elevAng and rtElevAng
            SatAze.AxSataze(units, ref site, Constant.REFIND, ref satellite, out azim, out elevAng, out rtElevAng);

            feAnte.az = (float)azim;
            feAnte.el = (float)elevAng;
            nArrayFW[FeAnte.AZ] = Constant.DB_NOT_NULL;
            nArrayFW[FeAnte.EL] = Constant.DB_NOT_NULL;

            // calculate sat long in hundreds of seconds 
            feAnte.satlongit = GenUtil.FwIntSatlong(feAnte.satlong);

            if (feAnte.satlongs.Equals("E"))
            {
                feAnte.satlongit *= -1;
            }
            nArrayFW[FeAnte.SATLONGIT] = Constant.DB_NOT_NULL;

            // calculate RXHGMAX 
            if (valAcodeRx)
            {
                if (CalcHGmax(elevAng, feAnte.acoderx, nArrayFW[FeAnte.ACODERX], false,
                                            out feAnte.rxhgmax, out nArrayFW[FeAnte.RXHGMAX]) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("Cannot read Antenna Pattern for RX acode: %s:",
                                    keyLine, "W", feAnte.acoderx);
                    warnCount++;
                }
            }

            // calculate TXHGMAX 
            if (valAcodeTx)
            {
                if (CalcHGmax(elevAng, feAnte.acodetx, nArrayFW[FeAnte.ACODETX], true,
                                            out feAnte.txhgmax, out nArrayFW[FeAnte.TXHGMAX]) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("Cannot read Antenna Pattern for TX acode :%s:", keyLine, "W", feAnte.acodetx);
                    warnCount++;
                }
            }
        }

        /// <summary>
        /// This method converts latitude and logitude from decimal degrees to 
        /// hours, minutes, and seconds and populates the 
        /// associated fields of the prescribed AxStation object.  
        /// </summary>
        /// <param name="latitude"> - prescribed decimal latitude.</param>
        /// <param name="longitude"> - prescribed decimal longitude.</param>
        /// <param name="site"> - prescribed AxStation object.</param>
        public static void LoadSite(int latitude, int longitude, ref AxStation site)
        {
            int degrees;
            int minutes;
            int seconds;

            if (longitude < 0)
            {
                site.LL.longSens = "E";
                longitude *= -1;    // make value positive 
            }
            else
            {
                site.LL.longSens = "W";
            }

            degrees = longitude / 360000;
            minutes = (longitude / 6000) - (degrees * 60);
            seconds = (longitude / 100) - (((degrees * 60) + minutes) * 60);

            site.LL.longDeg = degrees;
            site.LL.longMin = minutes;
            site.LL.longSec = seconds;

            if (latitude < 0)
            {
                site.LL.latSens = "S";
                latitude *= -1; // make value positive 
            }
            else
            {
                site.LL.latSens = "N";
            }

            degrees = latitude / 360000;
            minutes = (latitude / 6000) - (degrees * 60);
            seconds = (latitude / 100) - (((degrees * 60) + minutes) * 60);

            site.LL.latDeg = degrees;
            site.LL.latMin = minutes;
            site.LL.latSec = seconds;
        }

        /// <summary>
        /// This method calculates, if possible, the maximum antenna gain (db) that is
        /// used to populate the antenna record's rxhgmax and txhgmax fields.
        /// </summary>
        /// <param name="elevAng"> - elevation angle.</param>
        /// <param name="acode"> - antenna code.</param>
        /// <param name="acodeNull"> - ODBC nullInd for acode.</param>
        /// <param name="isTX"> - boolean to indicate if this call is for the transmit case.</param>
        /// <param name="hgmax"> - the calculated maximum gain (dB).</param>
        /// <param name="hgmaxNull"> - ODBC nullInd for hgmax.</param>
        /// <returns></returns>
        public static int CalcHGmax(double elevAng,
                                         string acode,
                                         SQLLEN acodeNull,
                                         bool isTX,
                                         out float hgmax,
                                         out SQLLEN hgmaxNull)
        {
            // 'out' requirements.
            hgmax = 0.0f;
            hgmaxNull = Constant.DB_NULL;

            string Sacode = "";
            float sdAgain;
            string junk;
            int tmpG, rc;

            PatternStruct patDat;
            float tmpHGmax,
                        sdDcoh,
                        sdDcov,
                        dxpv,
                        dxph;

            SuAntStr suAntStr;
            int nRet;

            if (acodeNull != Constant.DB_NULL)
            {

                if (acode.StartsWith("$"))
                {
                    // not calculated for temporary codes 
                    hgmax = 0.0f;
                    hgmaxNull = Constant.DB_NULL;
                    return (Constant.SUCCESS);
                }

                if (Strings.StrnCmp(acode, "CCIR", 4) == 0)
                {
                    if (Abs(elevAng) <= 1.0)
                    {
                        // calc interfer antenna gain 
                        if (isTX)
                        {
                            // position 7&8 of acode is TX gain 
                            sf.Parse(acode, "%4s%2d%2f");
                            junk = (string)sf.Results[0];
                            tmpG = (int)sf.Results[1];
                            tmpHGmax = (float)sf.Results[2];
                        }
                        else
                        {
                            // position 5&6 from acode is RX gain 
                            sf.Parse(acode, "%4s%2f%2d");
                            junk = (string)sf.Results[0];
                            tmpHGmax = (float)sf.Results[1];
                            tmpG = (int)sf.Results[2];
                        }
                    }
                    else if (Abs(elevAng) <= 48.0)
                    {
                        tmpHGmax = (float)(32.0 - 25.0 * Log10(Abs(elevAng)));
                    }
                    else
                    {
                        tmpHGmax = -10.0f;
                    }

                    hgmax = tmpHGmax;

                    hgmaxNull = Constant.DB_NOT_NULL;
                }
                else
                {
                    // using the antenna code get its antenna pattern 
                    if ((rc = TpGetDat.TpGetPattern(acode, out patDat)) != Constant.SUCCESS)
                    {

                        hgmax = 0.0f;

                        hgmaxNull = Constant.DB_NULL;
                        if ((rc == Error.NOANTEFOUND) || (rc == Error.NOANTDFOUND))
                        {
                            // could not find ante Pattern 
                            return (Constant.FAILURE);
                        }
                        else
                        {
                            // INGRES error occurred 
                            ErrMsg.UtPrintMessage(rc);
                            return (Constant.FAILURE);
                        }
                    }
                    if ((patDat.nulls[Constant.DCOV] == Constant.DB_NULL) && (patDat.nulls[Constant.DCOH] == Constant.DB_NULL))
                    {

                        hgmax = 0.0f;

                        hgmaxNull = Constant.DB_NULL;
                        return (Constant.FAILURE);
                    }

                    // using pattern and elev angle find antenna discrim 
                    TpSub.TpFindDisc(patDat.pattern, patDat.numPts, (float)(Abs(elevAng)), out sdDcov,
                       out dxpv, out sdDcoh, out dxph);

                    // get ante again from SDB for interferer 
                    nRet = Suutils.SuGetAnt(acode, out suAntStr);
                    if (nRet != 0)
                    {

                        hgmax = 0.0f;

                        hgmaxNull = Constant.DB_NULL;
                        if (nRet != Constant.NOMORERECS)
                        {
                            ValErrs.AddMess("calcHGmax could not get antenna gain: %d.", Sacode, "E", nRet.ToString());
                            return (Constant.FAILURE);
                        }
                        else
                        {
                            return (Constant.FAILURE);
                        }
                    }
                    else
                    {
                        sdAgain = suAntStr.acAnt.again;
                    }

                    if (patDat.nulls[Constant.DCOV] == Constant.DB_NULL)
                    {

                        hgmax = sdAgain - sdDcoh;
                    }
                    else if (patDat.nulls[Constant.DCOH] == Constant.DB_NULL)
                    {

                        hgmax = sdAgain - sdDcov;
                    }
                    else
                    {

                        hgmax = sdAgain - Min(sdDcoh, sdDcov);
                    }

                    hgmaxNull = Constant.DB_NOT_NULL;
                }
            }
            else
            {

                hgmax = 0.0f;

                hgmaxNull = Constant.DB_NULL;
            }

            return (Constant.SUCCESS);
        }








    }
}

```
