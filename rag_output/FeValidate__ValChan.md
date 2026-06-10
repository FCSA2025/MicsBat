# Documented File: ValChan.cs
**Repository Path:** `FeValidate\ValChan.cs`
**Primary Layer:** `FeValidate`
**Namespace:** `FeValidate`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeValidate
{
    using _Auxlib;
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;

    using SQLLEN = Int64;

    /// <summary>
    /// This class provides methods that validate the channel information
    /// provided by a previously imported PDF.
    /// </summary>
    public class ValChan
    {

        private const string titleLine = @"VALIDATION AGAINST SDB";

        /// <summary>
        /// This method validates ES channel records i.a.w. their prescribed
        /// MDB operation (ADD, BLANK-OUT, DELETE, NO-CHANGE, UPDATE).  
        /// </summary>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - number of errors encountered.</param>
        /// <param name="warnCount"> - number of warnings encountered.</param>
        /// <param name="valLevel"> - level of validation.</param>
        public static void FeValChanES(string pdfName,    // pdf display name 
                                 ref short errCount,   // number of errors encountered 
                                 ref short warnCount,  // number of warnings encountered 
                                 ref short valLevel)   // level of validation 
        {

            SQLLEN[] nArrayFW;
            int cID1;
            string whereClause;
            string tableName;
            string keyLine;
            FeChan feChan;

            whereClause = "";
            keyLine = "";

            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out tableName);

            // read channel information 
            if ((cID1 = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read channel information. Reason: %d", tableName, "W", cID1.ToString());
                warnCount++;
                return;
            }

            while (DynFeChan.FeFetchChan(cID1, out feChan, out nArrayFW) == Constant.SUCCESS)
            {
                keyLine = String.Format("KEY: {0} {1} {2}", feChan.location, feChan.call1, feChan.chid);

                switch (feChan.cmd[0])
                {
                    case 'A':
                        FeValChanAdd(feChan, nArrayFW, pdfName, ref errCount, ref warnCount, ref valLevel, keyLine);
                        break;

                    case 'D':
                        feValChanDel(feChan, ref errCount, ref warnCount, keyLine);
                        break;

                    case 'B':
                    case 'U':
                        FeValChanUpdt(feChan, nArrayFW, pdfName, ref errCount, ref warnCount, ref valLevel, keyLine);
                        break;

                    case 'N':
                        feValChanNoop(feChan, nArrayFW, pdfName, ref errCount, ref warnCount, keyLine);
                        break;

                    default:
                        ValErrs.AddMess("Command (%c) is invalid.", keyLine, "E", feChan.cmd[0].ToString());
                        errCount++;
                        break;
                }

                DynFeChan.FeUpdateChan(cID1, feChan, nArrayFW);
            }
            DynFeChan.FeCloseChan(cID1);
        }

        /// <summary>
        /// This method validates ES channel records whose MDB operation is "ADD".  
        /// </summary>
        /// <remarks>
        /// This procedure performs the channel validation for an ADD record. It verifies 
        /// that the record does not already exist and calls a field validation method to 
        /// verify that all its fields are correct.  
        /// </remarks>
        /// <param name="feChan"> - channel information, an instance of FeChan.</param>
        /// <param name="nArrayFW"> - array with ODBC null flags for feChan.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="valLevel"> - level of validation</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValChanAdd(FeChan feChan, // chan info 
                                                         SQLLEN[] nArrayFW, // array with null flags 
                                                         string pdfName,    // pdf display name 
                                                         ref short errCount,    // number of errors 
                                                         ref short warnCount,   // number of warnings 
                                                         ref short valLevel,    // level of validation 
                                                         string keyLine)        // key info to print 
        {
            SQLLEN[] nArrayMDB;
            int rc;
            string whereClause;
            MeChan meChan;

            // make sure channel does not already exist 
            ValImport.FeFormWhereClause(out whereClause, feChan.location, feChan.call1, feChan.chid);

            rc = ValRetrieveES.FeValRetrieveMDBChannel(out meChan, whereClause, out nArrayMDB);

            switch (rc)
            {
                case Constant.SUCCESS:
                    ValErrs.AddMess("Channel record already exists in MDB, cannot ADD", keyLine, "E");
                    errCount++;
                    break;

                case Constant.NOMORERECS:
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Channel record locked in MDB.", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Channel record undefined error: %d", keyLine, "W", rc.ToString());
                    warnCount++;
                    break;
            }

            FeValChanFields(feChan, nArrayFW, ref errCount, ref warnCount, pdfName, ref valLevel, keyLine);
        }

        /// <summary>
        /// This method validates ES channel records whose MDB operation is "DELETE".  
        /// </summary>
        /// <remarks>
        /// This procedure performs the channel validation for a DELETE record. It 
        /// verifies that the record does indeed exist and deletes all information pointing to the 
        /// channel record.  
        /// </remarks>
        /// <param name="feChan"> - channel information, instance of FeChan.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void feValChanDel(FeChan feChan,// chan info 
                                                         ref short errCount,    // number of errors 
                                                         ref short warnCount,   // number of warnings 
                                                         string keyLine)    // key info to print 
        {
            SQLLEN[] nArrayMDB;
            int rc;
            string whereClause;
            MeChan meChan;

            if (!feChan.recstat.Equals("C"))
            {
                // if channel is user generated make sure chan does exist 
                ValImport.FeFormWhereClause(out whereClause, feChan.location, feChan.call1,
                                                    feChan.chid);

                rc = ValRetrieveES.FeValRetrieveMDBChannel(out meChan, whereClause, out nArrayMDB);

                switch (rc)
                {
                    case Constant.SUCCESS:
                        break;

                    case Constant.NOMORERECS:
                        ValErrs.AddMess("Channel does not exist in MDB", keyLine, "E");
                        errCount++;
                        break;

                    case Constant.RECLOCK:
                        ValErrs.AddMess("Channel record locked in MDB", keyLine, "W");
                        warnCount++;
                        break;

                    default:
                        ValErrs.AddMess("Channel record undefined error: %d", keyLine, "W", rc.ToString());
                        warnCount++;
                        break;
                }
            }
        }

        /// <summary>
        /// This method validates ES channel records whose MDB operation is "UPDATE".  
        /// </summary>
        /// <remarks>
        /// This method performs the channel validation for an UPDATE record. It verifies 
        /// that the record does indeed already exist and calls the field validation method to 
        /// verify that all fields are correct.  
        /// </remarks>
        /// <param name="feChan"> - channel information, instance of FeChan.</param>
        /// <param name="nArrayFW"> - array with ODBC null flags for feChan.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="valLevel"> - level of validation.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValChanUpdt(FeChan feChan,    // chan info 
                                                            SQLLEN[] nArrayFW,  // array with null flags 
                                                            string pdfName, // pdf display name 
                                                            ref short errCount, // number of errors 
                                                            ref short warnCount,    // number of warnings 
                                                            ref short valLevel, // level of validation 
                                                            string keyLine) // key info to print 
        {

            SQLLEN[] nArrayMDB;
            int rc;
            string whereClause;
            MeChan meChan;

            // make sure chan does exist 
            ValImport.FeFormWhereClause(out whereClause, feChan.location, feChan.call1, feChan.chid);

            rc = ValRetrieveES.FeValRetrieveMDBChannel(out meChan, whereClause, out nArrayMDB);
            switch (rc)
            {
                case Constant.SUCCESS:
                    break;

                case Constant.NOMORERECS:
                    ValErrs.AddMess("Channel does not exist in MDB", keyLine, "E");
                    errCount++;
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Channel record locked  in MDB", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Channel undefined error: %d", keyLine, "W", rc.ToString());
                    warnCount++;
                    break;
            }

            if (rc != Constant.SUCCESS)
            {
                /* 	if we couldn't properly retrieve this record we don't want to continue
                *		validating. */
                return;
            }

            // Check modify date and time 
            if (((!feChan.mdate.Equals(meChan.mdate))) ||
                 ((!feChan.mtime.Equals(meChan.mtime))))
            {
                // Modify dates/times don't match 
                ValErrs.AddMess("This Channel record has been modified since you retrieved it.",
                                keyLine, "W");
                warnCount++;
            }

            FeValChanFields(feChan, nArrayFW, ref errCount, ref warnCount, pdfName, ref valLevel, keyLine);
        }

        /// <summary>
        /// This method validates that a channel record exists in the MDB; if it does not 
        /// exist an error message is added to the report.  
        /// </summary>
        /// <param name="feChan"> - channel information, instance of FeChan.</param>
        /// <param name="nArrayFW"> - array of ODBC nullInds for feChan.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void feValChanNoop(FeChan feChan,    // chan info 
                                                            SQLLEN[] nArrayFW,
                                                            string pdfName,
                                                            ref short errCount, // number of errors 
                                                            ref short warnCount,    // number of warnings 
                                                            string keyLine) // key info to print 
        {

            SQLLEN[] nArrayMDB;
            SQLLEN[] nArrayAnte;
            short rc;
            int ID1;
            string whereClause;
            string tableName;
            MeChan meChan;
            FeAnte feAnte;

            // make sure chan does exist 
            ValImport.FeFormWhereClause(out whereClause, feChan.location, feChan.call1, feChan.chid);

            rc = (short)ValRetrieveES.FeValRetrieveMDBChannel(out meChan, whereClause, out nArrayMDB);
            switch (rc)
            {
                case Constant.SUCCESS:
                    break;

                case Constant.NOMORERECS:
                    ValErrs.AddMess("Channel does not exist in MDB.", keyLine, "E");
                    errCount++;
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Channel record locked  in MDB.", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Channel undefined error: %d", keyLine, "W", rc.ToString());
                    warnCount++;
                    break;
            }

            // check that all channel frequencies fall into antenna band range 

            // retrieve antenna record 
            GenUtil.UtCvtName(Constant.FE_ANTE, pdfName, out tableName);
            ValImport.FeFormWhereClause(out whereClause, feChan.location, feChan.call1, "");

            if ((ID1 = DynFeAnte.FeSelectAnte(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read Antenna information. Reason: %d", keyLine, "W", ID1.ToString());
                warnCount++;
                return;
            }
            else
            {
                if (DynFeAnte.FeFetchAnte(ID1, out feAnte, out nArrayAnte) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("No Antenna record for this channel.", keyLine, "E");
                    errCount++;
                    DynFeAnte.FeCloseAnte(ID1);
                    return;
                }
                DynFeAnte.FeCloseAnte(ID1);
            }
            if (nArrayFW[FeChan.FREQTX] != Constant.DB_NULL)
            {
                ValFreq(feAnte.txband, feChan.freqtx, "TX", keyLine, ref errCount);
            }
            if (nArrayFW[FeChan.FREQRX] != Constant.DB_NULL)
            {
                ValFreq(feAnte.rxband, feChan.freqrx, "RX", keyLine, ref errCount);
            }
        }

        /// <summary>
        /// This method validates individual fields in a 
        /// channel record for the MDB operations of "ADD" and "UPDATE"; it ensures that 
        /// all required (mandated) information is indeed present in the channel record.  
        /// </summary>
        /// <param name="feChan"> - channel information, instance of FeChan.</param>
        /// <param name="nArrayFW"> - array with ODBC null flags for feChan.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="valLevel"> - level of validation</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValChanFields(FeChan feChan,// chan info 
                                                        SQLLEN[] nArrayFW,  // array with null flags 
                                                        ref short errCount, // number of errors 
                                                        ref short warnCount,    // number of warnings 
                                                        string pdfName, // pdf display name 
                                                        ref short valLevel, // level of validation 
                                                        string keyLine) // key info to print 
        {

            string whereClause;
            SQLLEN[] nArraySite;
            SQLLEN[] nArrayAnte;
            bool anteExists = true;
            int ID1;
            string tableName;
            FeAnte feAnte;
            FeSite feSite;


            if (nArrayFW[FeChan.CMD] == Constant.DB_NULL)
            {
                // must enter data in field 
                ValErrs.AddMess("Must Enter Command field.", keyLine, "E");
                errCount++;
            }

            if (nArrayFW[FeChan.FREQTX] != Constant.DB_NULL)
            {
                if (nArrayFW[FeChan.POLTX] == Constant.DB_NULL)
                {
                    // must enter data in field 
                    ValErrs.AddMess("Must Enter TX Polarization field", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.MAXTXPOWER] == Constant.DB_NULL)
                {
                    // must enter data in field 
                    ValErrs.AddMess("Must Enter Max TX Power field", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.PWRTX] == Constant.DB_NULL)
                {
                    // must enter data in field 
                    ValErrs.AddMess("Must Enter TX Power field.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.EQPTTX] == Constant.DB_NULL)
                {
                    // must enter data in field 
                    ValErrs.AddMess("Must Enter TX Equipment field.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.STATTX] == Constant.DB_NULL)
                {
                    // must enter data in field 
                    ValErrs.AddMess("Must Enter TX Status field.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.TRAFTX] == Constant.DB_NULL)
                {
                    // must enter data in field 
                    ValErrs.AddMess("Must Enter TX Traffic field.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.FEETX] == Constant.DB_NULL)
                {
                    // must enter data in field 
                    ValErrs.AddMess("Must Enter TX Fee Code field.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.P4KHZ] == Constant.DB_NULL)
                {
                    // must enter data in field 
                    ValErrs.AddMess("Must Enter Energy Dispersal field.", keyLine, "E");
                    errCount++;
                }

            }

            if (nArrayFW[FeChan.FREQRX] != Constant.DB_NULL)
            {
                if (nArrayFW[FeChan.POLRX] == Constant.DB_NULL)
                {
                    // must enter data in field 
                    ValErrs.AddMess("Must Enter RX Polarization field.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.PWRRX] == Constant.DB_NULL)
                {
                    // must enter data in field 
                    ValErrs.AddMess("Must Enter RX Power field.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.EQPTRX] == Constant.DB_NULL)
                {
                    // must enter data in field 
                    ValErrs.AddMess("Must Enter RX Equipment field.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.STATRX] == Constant.DB_NULL)
                {
                    // must enter data in field 
                    ValErrs.AddMess("Must Enter RX Status field.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.TRAFRX] == Constant.DB_NULL)
                {
                    // must enter data in field 
                    ValErrs.AddMess("Must Enter RX Traffic field.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.FEERX] == Constant.DB_NULL)
                {
                    // must enter data in field 
                    ValErrs.AddMess("Must Enter RX Fee Code field.", keyLine, "E");
                    errCount++;
                }

            }

            if (nArrayFW[FeChan.I20] == Constant.DB_NULL)
            {
                // must enter data in field 
                ValErrs.AddMess("Must Enter Interference Power field.", keyLine, "E");
                errCount++;
            }

            if (nArrayFW[FeChan.IP01] == Constant.DB_NULL)
            {
                // must enter data in field 
                ValErrs.AddMess("Must Enter Precipitation Interference field.", keyLine, "E");
                errCount++;
            }

            if (nArrayFW[FeChan.IT01] == Constant.DB_NULL)
            {
                // must enter data in field 
                ValErrs.AddMess("Must Enter Tropospheric Interference field.", keyLine, "E");
                errCount++;
            }


            // retrieve site record 
            GenUtil.UtCvtName(Constant.FE_SITE, pdfName, out tableName);

            ValImport.FeFormWhereClause(out whereClause, feChan.location, "", "");

            if ((ID1 = DynFeSite.FeSelectSite(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read site information. Reason: %d",
                                keyLine, "W", ID1.ToString());
                warnCount++;
                /* if site non existent, antenna will also not exist
                 * so no more validation can be done
                 */
                return;
            }
            else
            {
                if (DynFeSite.FeFetchSite(ID1, out feSite, out nArraySite) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("No Site exists for this channel", keyLine, "E");
                    errCount++;
                    DynFeSite.FeCloseSite(ID1);
                    return;
                }
                DynFeSite.FeCloseSite(ID1);
            }

            // retrieve antenna record 
            GenUtil.UtCvtName(Constant.FE_ANTE, pdfName, out tableName);

            ValImport.FeFormWhereClause(out whereClause, feChan.location, feChan.call1, "");

            if ((ID1 = DynFeAnte.FeSelectAnte(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read Antenna information. Reason: %d", keyLine, "W", ID1.ToString());
                warnCount++;
                anteExists = false;
                return;
            }
            else
            {
                if (DynFeAnte.FeFetchAnte(ID1, out feAnte, out nArrayAnte) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("No Antenna record for this channel.", keyLine, "E");
                    anteExists = false;
                    errCount++;
                    DynFeAnte.FeCloseAnte(ID1);
                    return;
                }
                DynFeAnte.FeCloseAnte(ID1);
            }

            /* required fields are only required on Blank or ADD.  Update
             * pulls in fields from MDB if not user entered.  Noop not
             * checked.  Delete does not require any but key fields.
             */

            // validate notc against snote table (nonum) 
            if (nArrayFW[FeChan.NOTC] != Constant.DB_NULL && feChan.notc.Length > 0)
            {
                SuNote suNote;
                int nRet;

                nRet = Suutils.SuGetNote(feSite.oper, feChan.notc, out suNote);
                if (nRet != 0)
                {
                    ValErrs.AddMess("Note (%s) not present in SDB for Operator :%s:",
                                    keyLine, "E", feChan.notc, feSite.oper);
                    errCount++;
                }
            }

            if (anteExists == false)
            {
                return;
            }

            if (nArrayFW[FeChan.FREQTX] != Constant.DB_NULL)
            {
                // all tx side fields must be present or blank 
                FeValTXSide(feChan, feAnte, nArrayFW, nArrayAnte, ref errCount, ref warnCount, ref valLevel, keyLine);
            }

            if (nArrayFW[FeChan.FREQRX] != Constant.DB_NULL)
            {
                // all rx side fields must be present or blank 
                FeValRXSide(feChan, feAnte, nArrayFW, nArrayAnte, ref errCount, ref warnCount, ref valLevel, keyLine);
            }

            if ((nArrayFW[FeChan.FREQTX] == Constant.DB_NULL) &&
                    (nArrayFW[FeChan.FREQRX] == Constant.DB_NULL))
            {
                ValErrs.AddMess("Must enter at least one TX or RX Frequency.", keyLine, "E");
                errCount++;
            }
        }

        /// <summary>
        /// This method validates the fields that are required (mandated) if the channel is a TX channel.  
        /// </summary>
        /// <param name="feChan"> - channel information, an instance of FeChan.</param>
        /// <param name="feAnte"> - antenna information, an instance of FeAnte</param>
        /// <param name="nArrayFW"> - array of ODBC nullInds for feChan.</param>
        /// <param name="nArrayAnte"> - array of ODBC nullInds for feAnte</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="valLevel"> - level of validation.</param>
        /// <param name="keyLine"> - key information to print.</param>
        public static void FeValTXSide(FeChan feChan, // structure with chan info 
                                                FeAnte feAnte, // structure with ante infe 
                                                SQLLEN[] nArrayFW,  // chan array with null flags 
                                                SQLLEN[] nArrayAnte,    // ante array with null flags 
                                                ref short errCount, // number of errors 
                                                ref short warnCount,    // number of warnings 
                                                ref short valLevel, // level of validation 
                                                string keyLine)     // key information to print 
        {
            bool valEqpt = true;
            SuEqpt tEqpt;
            SuTraf tTraf;
            int nRet;

            if (nArrayAnte[FeAnte.TXBAND] != Constant.DB_NULL && feAnte.txband.Length > 0)
            {
                // all tx chan fields must be present  and  valid 
                if (nArrayFW[FeChan.EQPTTX] != Constant.DB_NULL && feChan.eqpttx.Length > 0)
                {
                    if (feChan.eqpttx.StartsWith("$"))
                    {
                        valLevel = Constant.TSIP;
                    }
                    else
                    {
                        nRet = Suutils.SuGetEqpt(feChan.eqpttx, out tEqpt);
                        if (nRet != 0)
                        {
                            ValErrs.AddMess("TX Equipment (%s) not present in SDB", keyLine, "E", feChan.eqpttx);
                            errCount++;
                            valEqpt = false;
                        }
                    }
                }
                else
                {
                    ValErrs.AddMess("TX Equipment must be present", keyLine, "E");
                    errCount++;
                    valEqpt = false;
                }

                if (nArrayFW[FeChan.TRAFTX] != Constant.DB_NULL && feChan.traftx.Length > 0)
                {
                    if (feChan.traftx.StartsWith("$"))
                    {
                        valLevel = Constant.TSIP;
                    }
                    else if ((valEqpt == true) && (feChan.traftx.StartsWith("D")))
                    {
                        nRet = Suutils.SuGetTraf(feChan.traftx, feChan.eqpttx, out tTraf);
                        if (nRet != 0)
                        {
                            ValErrs.AddMess("TX Traffic/Eqpt pair (%s/%s) not present in the SDB.",
                                            keyLine, "W", feChan.traftx, feChan.eqpttx);
                            warnCount++;
                        }
                    }
                    else
                    {
                        nRet = Suutils.SuGetTraf(feChan.traftx, null, out tTraf);
                        if (nRet != 0)
                        {
                            ValErrs.AddMess("TX Traffic not present in the SDB", keyLine, "E", feChan.traftx);
                            errCount++;
                        }
                        else if (nRet < 0)
                        {
                            ValErrs.AddMess("Ingres error (%d) when getting TX Traffic.", keyLine, "E", nRet.ToString());
                            errCount++;
                        }
                    }
                }
                else
                {
                    ValErrs.AddMess("TX Traffic must be present", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.FREQTX] != Constant.DB_NULL)
                {
                    ValFreq(feAnte.txband, feChan.freqtx, "TX", keyLine, ref errCount);
                }
                else
                {
                    ValErrs.AddMess("TX Frequency must be present", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.POLTX] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("TX Polarization must be present.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.MAXTXPOWER] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Max TX Power must be present.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.PWRTX] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("TX Power must be present.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.STATTX] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("TX Status must be present.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.FEETX] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("TX Fee must be present.", keyLine, "E");
                    errCount++;
                }
            }
            else  //if (nArrayAnte[FeAnte.TXBAND] == Constant.DB_NULL || feAnte.txband.Length == 0)
            {
                // all tx chan fields must not be present 
                if (nArrayFW[FeChan.FREQTX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("TX Frequency invalid for this antenna (TX data must not be present).",
                                    keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.POLTX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("TX Polarization invalid for this antenna (TX data must not be present).",
                                    keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.MAXTXPOWER] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("Max TX Power invalid for this antenna (TX data must not be present).",
                                    keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.PWRTX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("TX Power invalid for this antenna (TX data must not be present).",
                                    keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.EQPTTX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("TX Equipment invalid for this antenna (TX data must not be present).",
                                    keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.TRAFTX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("TX Traffic invalid for this antenna (TX data must not be present).",
                                    keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.STATTX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("TX Status invalid for this antenna (TX data must not be present).",
                                    keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.FEETX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("TX Fee code invalid for this antenna (TX data must not be present).",
                                    keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.SRVCTX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("TX Service invalid for this antenna (TX data must not be present).",
                                    keyLine, "E");
                    errCount++;
                }
            }
        }

        /// <summary>
        /// This method validates that a prescribed frequency lies within the numerical
        /// bounds associated with a prescribed band code i.a.w. the information in the
        /// SDB sd_band table.
        /// </summary>
        /// <param name="bndcde"></param>
        /// <param name="freq"></param>
        /// <param name="type"></param>
        /// <param name="keyLine"></param>
        /// <param name="errCount"></param>
        public static void ValFreq(string bndcde,
                                        double freq,
                                        string type,
                                        string keyLine,
                                        ref short errCount)
        {
            SuBand pBand;
            int nRet;

            // retrieve the high and low range for band 
            nRet = Suutils.SuGetBand(bndcde, out pBand);
            if (nRet == 0)
            {
                string str;
                if (!IsInBand(pBand, freq, out str))
                {
                    ValErrs.AddMess(str, keyLine, "E");
                    errCount++;
                }
            }
        }

        /// <summary>
        /// This method validates the fields that are required (mandated) if the channel is a RX channel.  
        /// </summary>
        /// <param name="feChan"> - channel information, an instance of FeChan.</param>
        /// <param name="feAnte"> - antenna information, an instance of FeAnte</param>
        /// <param name="nArrayFW"> - array of ODBC nullInds for feChan.</param>
        /// <param name="nArrayAnte"> - array of ODBC nullInds for feAnte</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="valLevel"> - level of validation.</param>
        /// <param name="keyLine"> - key information to print.</param>
        public static void FeValRXSide(FeChan feChan, // structure with chan info 
                                                FeAnte feAnte, // structure with ante info 
                                                SQLLEN[] nArrayFW,  // array with null flags 
                                                SQLLEN[] nArrayAnte,    // array with null flags 
                                                ref short errCount, // number of errors 
                                                ref short warnCount,    // number of warnings 
                                                ref short valLevel, // level of validation 
                                                string keyLine)     // key information to print 
        {
            bool valEqpt = true;
            SuEqpt tEqpt;
            SuTraf tTraf;
            int nRet;

            if (nArrayAnte[FeAnte.RXBAND] != Constant.DB_NULL)
            {
                // all rx chan fields must be present  and  valid 
                if (nArrayFW[FeChan.EQPTRX] != Constant.DB_NULL)
                {
                    if (feChan.eqptrx.StartsWith("$"))
                    {
                        valLevel = Constant.TSIP;
                    }
                    else
                    {
                        nRet = Suutils.SuGetEqpt(feChan.eqptrx, out tEqpt);
                        if (nRet != 0)
                        {
                            ValErrs.AddMess("RX Equipment (%s) not present in SDB.",
                                            keyLine, "E", feChan.eqptrx);
                            errCount++;
                            valEqpt = false;
                        }
                    }
                }
                else
                {
                    ValErrs.AddMess("RX Equipment must be present.", keyLine, "E");
                    errCount++;
                    valEqpt = false;
                }

                if (nArrayFW[FeChan.TRAFRX] != Constant.DB_NULL)
                {
                    if (feChan.trafrx.StartsWith("$"))
                    {
                        valLevel = Constant.TSIP;
                    }
                    else if ((valEqpt == true) &&
                                       (feChan.trafrx.StartsWith("D")))
                    {
                        // traf is digital 
                        nRet = Suutils.SuGetTraf(feChan.trafrx, feChan.eqptrx, out tTraf);
                        if (nRet != 0)
                        {
                            ValErrs.AddMess("RX Traffic/Eqpt pair (%s/%s) not present in the SDB.",
                                            keyLine, "W", feChan.trafrx, feChan.eqptrx);
                            warnCount++;
                        }
                    }
                    else
                    {
                        // eqpt is not digital (eg. analog, $code) 
                        nRet = Suutils.SuGetTraf(feChan.trafrx, null, out tTraf);
                        if (nRet != 0)
                        {
                            ValErrs.AddMess("RX Traffic (%s) not present in the SDB.", keyLine, "E", feChan.trafrx);
                            errCount++;
                        }
                        else if (nRet < 0)
                        {
                            ValErrs.AddMess("Ingres error reading RX Traffic: %d", keyLine, "E", nRet.ToString());
                            errCount++;
                        }
                    }
                }
                else
                {
                    ValErrs.AddMess("RX Traffic must be present.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.FREQRX] != Constant.DB_NULL)
                {
                    ValFreq(feAnte.rxband, feChan.freqrx, "RX", keyLine, ref errCount);
                }
                else
                {
                    ValErrs.AddMess("RX Frequency must be present.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.POLRX] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("RX Polarization must be present.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.PWRRX] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("RX Power must be present.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.STATRX] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("RX Status must be present.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.I20] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Interfer. Power must be present.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.IT01] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Tropo. Interfer. must be present.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.IP01] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Precipitation Interference must be present.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeChan.FEERX] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("RX Fee must be present.", keyLine, "E");
                    errCount++;
                }
            }
            else
            {
                // all rx chan fields must not be present 
                if (nArrayFW[FeChan.FREQRX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("RX Frequency invalid for this antenna (RX data must not be present).", keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.POLRX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("RX Polarization invalid for this antenna (RX data must not be present).", keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.PWRRX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("RX Power invalid for this antenna (RX data must not be present).", keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.EQPTRX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("RX Equipment invalid for this antenna (RX data must not be present).", keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.TRAFRX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("RX Traffic invalid for this antenna (RX data must not be present).", keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.STATRX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("RX Status invalid for this antenna (RX data must not be present).", keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.I20] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("Interfer. Power invalid for this antenna (RX data must not be present).", keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.IT01] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("Tropo. Interfer. invalid for this antenna (RX data must not be present).", keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.IP01] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("Precip. Interfer. invalid for this antenna (RX data must not be present).", keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.FEERX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("RX Fee code invalid for this antenna (RX data must not be present).", keyLine, "E");
                    errCount++;
                }
                if (nArrayFW[FeChan.SRVCRX] != Constant.DB_NULL)
                {
                    ValErrs.AddMess("RX Service invalid for this antenna (RX data must not be present).", keyLine, "E");
                    errCount++;
                }
            }
        }

        /// <summary>
        /// This method returns true if the prescribed frequency (in KHz) lies inside the [blo, bhi] frequency interval of the 
        /// prescribed SuBand object; otherwise false.
        /// </summary>
        /// <param name="suBand"> - a prescribed SuBand object.</param>
        /// <param name="frequency"> - a prescribed frequency to test.</param>
        /// <param name="message"> - an output message that describes any error conditions encountered.</param>
        /// <returns>true or false.</returns>
        public static bool IsInBand(SuBand suBand, double frequency, out string message)
        {
            // 'out' requirement.
            message = "";

            bool isInBand = IsInRange(frequency, suBand.blo, suBand.bhi);

            if (!isInBand)
            {
                message = String.Format("Frequency TX ({0:F2}) Out of Range :{1:F2}: to :{2:F2}:", frequency, suBand.blo, suBand.bhi);
            }
            // If bndcde is 7B check for the internal prohibited zone.
            else if (suBand.bndcde.Equals("7B"))
            {
                // Determine if frequency is in the internal prohibited zone.
                bool isInProhibitedZone = IsInRange(frequency, Constant.SUB_PLAN_I_LOWER_INTERVAL_FREQ_HI, Constant.SUB_PLAN_I_UPPER_INTERVAL_FREQ_LO);

                if (isInProhibitedZone)
                {
                    isInBand = false;
                    message = String.Format("Frequency TX ({0:F2}) is in prohibited range :{1:F2}: to :{2:F2}: - see ISED/SRSP-307.1", frequency, Constant.SUB_PLAN_I_LOWER_INTERVAL_FREQ_HI, Constant.SUB_PLAN_I_UPPER_INTERVAL_FREQ_LO);
                }
            }

            return isInBand;
        }

        /// <summary>
        /// This method returns true if the prescribed test value lies within the closed interval
        /// [lower, upper] on the real line.
        /// </summary>
        /// <param name="testValue"> a prescribed value to test for inclusion.</param>
        /// <param name="lower"> - the prescribed lower bound of the real interval.</param>
        /// <param name="upper"> - the prescribed upper bound of the real interval.</param>
        /// <returns></returns>
        private static bool IsInRange(double testValue, double lower, double upper)
        {
            bool result = false;

            if ((testValue >= lower) && (testValue <= upper))
            {
                result = true;
            }

            return result;
        }

    }
}

```
