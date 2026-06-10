# Documented File: FtValidate.cs
**Repository Path:** `FtValidate\FtValidate.cs`
**Primary Layer:** `FtValidate`
**Namespace:** `FtValidate`

## Source Code Representation
```csharp
using System;
using System.IO;
using _Utillib;
using _NewLib;
using _Configuration;
using _DataStructures;
using System.Collections.Generic;

/// <summary>
/// The principle terrestrial input validation program.
/// </summary>
/// <remarks>
/// \image html "Usage - FtValidate.PNG" "Usage - FtValidate"
/// FtValidate is the main terrestrial input validation program: its function is to ensure that the user-input, read 
/// in and stored in the database by ftImport, comprises a “well-formed” set of site descriptions that can be 
/// successfully used by the interference calculation routines and also in the administrative and billing functions. 
/// 
/// By “well-formed” we mean that they contain all the data needed to perform interference calculations, 
/// billing functions, and other administrative functions; and that all the information they contain is 
/// correct and reasonable.  To do this, it must check various codes, check the ordering and completeness 
/// of data and calculate fields that would be used in the interference calculations.  
/// 
/// User PDF files that 'pass' FtValidate's checks can be used by the Database Administrator to update the MDB TS tables.
/// </remarks>
namespace FtValidate
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using SQLLEN = Int64;

    /// <summary>
    /// FtValidate is the main terrestrial input validation program: its function is to ensure that the user-input, read 
    /// in and stored in the database by ftImport, comprises a “well-formed” set of site descriptions that can be 
    /// successfully used by the interference calculation routines and also in the administrative and billing functions. 
    /// </summary>
    public static class FtValidate
    {

        private const int userSession = 1;
        //private static StreamWriter swRedirected;
        public static bool IsVerbose = false;
        private static string[] Main_level = { "", "UPDATE", "TSIP" };

        // This global is set to FALSE if a user submits invalid passive data.
        // If this is the case we set this flag so the passive power calculations 
        // are not done
        public static bool runpassives;

        //------------------------------------------------------------------------------------
        private static int mHiLoMinDist = Constant.HILO_MINIMUM_DISTANCE; // Units: seconds of latitude.
        private static bool mDoHiLo = true;
        //------------------------------------------------------------------------------------

        /// <summary>
        /// This is the main method that provides top-level control for the FtValidate program; 
        /// it calls other validation modules in a prescribed sequence.
        /// </summary>
        /// <remarks>
        /// Nothing is explicitely returned, but the PDF is changed in the process (records are added, others are filled in) 
        /// and a validation report is produced, directed to Console.Out, but that can be redirected to a file using the -O option. 
        /// </remarks>
        /// <param name="args">an array of strings that captures the command-line parameters.</param>
        public static void Main(string[] args)
        {
            try
            {
#if true  // reset 2025-07-25
                string mLog2FilePath = @"d:\MicsBatchLogs\FtValidate.log";
                if (Log2.SetLogFilePath(mLog2FilePath))
                {
                    Log2.Erase();
                    Log2.Set(Log2.FileOpenClose.PER_SESSION);
                    Log2.Set(Log2.WriteMode.ENABLED);
                    Log2.Set(Log2.Level.VERBOSE);
                    Info.BuildMetaData = Info.CollateExeMetaData();
                    Log2.v("\nBuild: " + Info.ManagedBuildInfo());
                }
#endif
                string userid;
                int nRet;
                SuOper tOper = new SuOper();
                SQLLEN[] nArrayFWt;
                short errCount;
                short errTotal = 0;
                short warnCount;
                short warnTotal = 0;
                short valLevel;
                int tID;
                int missingSites;
                string fileName = new string(new char[Constant.TABLE_NM_SZ]);
                string curDate = new string(new char[15]);
                string curTime = new string(new char[10]);
                string cLastCall = new string(new char[Constant.CALLSIGN_SZ]);
                double dDistKm;
                int exitCode = 0;

                //string logFilePath = "";
                string str;

                FtTitl ftTitl;
                UserInfoData userInfo = new UserInfoData();

                // Use static object Info to collate environment variables, process IDs etc.
                Info.DbName = null;       // Set by command line argument.
                Info.ProjectCode = null;  // Set by command line argument.

                Info.MicsUserName = Environment.GetEnvironmentVariable("MicsUser");     // REQUIRED.
                Info.Password = Environment.GetEnvironmentVariable("Password");         // REQUIRED (but can be anything).
                Info.MicsRootDir = Environment.GetEnvironmentVariable("MICS_ROOT_DIR"); // OPTIONAL.
                Info.FcsaDisk = Environment.GetEnvironmentVariable("FCSADisk");         // OPTIONAL (defaults to D:).

                Info.ProcessID = Process.GetCurrentProcess().Id;
                Info.BuildMetaData = Info.CollateExeMetaData();

                // Disable the 'spoofing' mode.
                // The spoofing mode replaces the MDB schema 'main' with the schema of the
                // user that executed this program (e.g. 'hulme'). Consequently, hulme.mt_chan
                // acts as a surrogate for main.mt_chan etc.
                // Spoofing mode can be turned on for test purposes using a command line flag.
                // The default value of this parameter is 'true' so the following line is just
                // for emphasis of its initial state.
                Info.SpoofModeIsOff = true;

                // Process the command line arguments.
                ParseCommandLineArgs(args);

                // Now queue for access to the database.
                if ((nRet = Qutils.EnterQueue(Info.DbName, "READ", 5)) != 0)
                {
                    Qutils.ExplainQueue(Info.DbName, "READ", nRet, null); //    Explain any error message.
                    Log2.e("\n\nFtValidate.Main(): ERROR: Qutils.EnterQueue() returned nRet = " + nRet);
                    Application.Exit(Error.UNABLE_TO_ENTER_QUEUE);
                }

                /* Attempt connection to the prescribed database */
                int rc = Ssutil.UtConnect(Info.DbName, userSession);
                if (rc != 0)
                {
                    str = String.Format("\r\nFtValidate.Main(): ERROR: Cannot connect to database {0}, reason {1:D}\r\n", Info.DbName, rc);
                    Log2.e(str);
                    Console.Write(str);
                    Qutils.ExitQueue(Info.DbName, "READ");
                    Application.Exit(Error.ODBC_SQLCONNECT_FAILED);
                }

                // First call to the billing method to initialize the gathering of billing information.
                BiUtil.BiBillingRec("FTVALIDATE", Info.PdfName);

                // Set the operating state of the classes that provide high-level read/write
                // methods that access TS MDB tables.
                // We must do this after the call to Ssutil.UtConnect() because only then is
                // the user's schema (Info.GlobalSchema) set to its correct value.
                DynMdbAntenna.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMdbChannel.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMdbSite.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynSdbTown.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynSdbRout.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynSdbOper.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);

                // Write the output report's header. 
                Console.Write("FtValidate.exe: Build Configuration: {0}\r\n", Info.CsharpBuildConf);
                Console.Write("\r\nPDF VALIDATION REPORT - Build {0}", Info.CsharpBuildInfo);
                Console.Write("\r\nRUN DATE: {0}", GenUtil.UtGetDateTime());
                Console.Write("\r\nProject Code [{0}] Process Id: {1}", Info.ProjectCode.ToLower(), Info.ProcessID);

                if (IsVerbose)
                {
                    Console.Write("V! Output will be Verbose.\r\n");
                }

                Console.Write("\r\n");

                //============ Process the PDF prescribed in the command-line ============

                // FtImport truncates the pdf name to the first 16 characters when creating
                // associated tables in the database.
                string pdfName = Info.PdfName;

                if (pdfName.Length > Constant.DISP_NM_SZ - 1)
                {
                    pdfName = pdfName.Substring(0, Constant.DISP_NM_SZ - 1);
                }

                //...Log2.v("\r\nforeach pdfName: " + pdfName);

                /* do for each file name received */
                runpassives = true;

                /* get the pdf name */
                str = String.Format("\r\nPDF Name: {0}\r\n\r\n", pdfName);
                Console.Write(str);

                // Verify that the PDF exists in the database.
                if (!Ssutil.UtTableExist(Constant.FT, pdfName))
                {
                    Log2.e("\nFtValidate.Main(): ERROR: One or more user ft_ tables was not found in the database for root: {0}.", pdfName);
                    Console.Write("ERROR - pdf '{0}' does not exist\r\n", pdfName);
                    exitCode = Error.USERTABLENOTFOUND;
                    goto cleanUpAndExit;
                }

                // If file has already updated the MDB, dont validate.
                string validatedFor = "initial";
                rc = FilewUtil.UtFilewValidated(Constant.FT, pdfName, out validatedFor);

                if (rc != Constant.SUCCESS)
                {
                    str = String.Format("\nFtValidate.Main(): ERROR: unable to fetch validation code from table {0}.ft_{1}_titl.", Info.GlobalSchema, pdfName);
                    Log2.e(str);
                    Console.Write(str);
                    ErrMsg.UtPrintMessage(Error.FILEWVAL);
                    exitCode = Error.FTTITLEFETCHFAILED;
                    goto cleanUpAndExit;
                }

                if (validatedFor.ToCharArray()[0] == Constant.UPDATE_POSTED)
                {
                    str = String.Format("\nFtValidate.Main(): ERROR: These user tables have already updated the MDB.");
                    Log2.e(str);
                    Console.Write(str);
                    ErrMsg.UtPrintMessage(Error.ALREADYPOSTED, "TS", pdfName, "Validation");
                    exitCode = Error.ALREADYPOSTED;
                    goto cleanUpAndExit;
                }

                // Reset error and warning totals to 0.
                errTotal = warnTotal = 0;
                // Reset error and warning count variables.
                errCount = warnCount = 0;

                // Default the validation level to "valid for UPDATE".
                valLevel = Constant.UPDATE;

                // The following method calls fill out all the missing information on the 
                // records from the MDB, as well as retrieving all the records associated 
                // with the current ones (ie. get all antennas and channels for this site, 
                // the site and all channels for this antenna or the site and antennas for 
                // this channel.

                if (IsVerbose)
                {
                    Console.Write("\r\nV! Starting to purge computer generated records in the input file.\r\n");
                    Console.Out.Flush();
                }

                // This method deletes all 'C'omputer generated records from the PDF; this 
                // prevents outdated records from previous validations from causing problems.
                FtValPurgeRecords(pdfName, ref errTotal);

                // Prints all the errors, then warnings, in key order; reset arrays to null.
                ValErrs.ValErrors();

                if (IsVerbose)
                {
                    Console.Write("\r\nV! Finished purging the input file.\r\n");
                    Console.Out.Flush();
                }

                if (errTotal > 0)
                {
                    Console.Write("\r\nThe above errors in the first pass preclude continuing.\r\n");
                    goto cleanUpAndExit; ;
                }

                if (IsVerbose)
                {
                    Console.Write("\r\nV! Starting to merge in the MDB records to the file.  Sites first.\r\n");
                }

                // Merge the site information in the MDB and in the PDF.
                ValMerge.FtValMergeSiteRecords(pdfName);
                if (IsVerbose)
                {
                    Console.Write("\r\nV! Finished merging in the site records. Now antennas\r\n");
                    Console.Out.Flush();
                }

                // Merge the antenna information in the MDB and in the PDF.
                ValMerge.FtValMergeAnteRecords(pdfName);

                if (IsVerbose)
                {
                    Console.Write("\r\nV! Finished merging in the antenna records.  Now channels\r\n");
                    Console.Out.Flush();
                }

                // Merge the channel information in the MDB and in the PDF.
                ValMerge.FtValMergeChanRecords(pdfName, ref errCount, ref warnCount);

                if (IsVerbose)
                {
                    Console.Write("\r\nV! Finished merging in the channel reocords.  Now passives.\r\n");
                    Console.Out.Flush();
                }

                // Builds the entire passive link based on limited passive information provided 
                // in the PDF; if a passive is being added without a complete link, this erroneous 
                // situation is reported.
                rc = ValMerge.FtValMergePassiveLinks(pdfName, ref errCount, ref warnCount);

                if (IsVerbose)
                {
                    Console.Write("\r\nV! Finished merging in the passives.\r\n");
                    Console.Out.Flush();
                }

                // Prints the errors, then warnings, in key order; reset arrays to null.
                ValErrs.ValErrors();

                if (rc != Constant.SUCCESS)
                {
                    //...Log2.v("\nFtValidate.Main(): Exit: FtValMergePassiveLinks did not succeed ");

                    if (rc == Error.BADPASSIVEDATA)
                    {
                        ErrMsg.UtErrMessage(Error.BADPASSIVEDATA);
                    }
                    else
                    {
                        ErrMsg.UtErrMessage(Error.GENERROR, "Unable to merge passive data. Validate cancelled");
                        ValErrs.ValErrors();
                    }

                    errCount = Constant.FAILURE;
                    goto cleanUpAndExit;
                }

                // Update the warning and error counts.
                errTotal += errCount;
                warnTotal += warnCount;

                // Reset error and warning count variables.
                errCount = warnCount = 0;

                // --- PDF Validation starts here. --- 
                // For each record type, the err and warning counters are reset 
                // to zero. The internal PDF validation (same checks as the  
                // vifred screens) is performed in the ..TSIntra method, and 
                // all other levels of validation are done in the ftVal..TS   
                // methods. The number of errors and warnings is printed 
                // at the end, as well as the level of validation (TSIP or    
                // update).	

                // --- Perform Site Record Validation. ---
                Console.Write("\r\n                        TS SITE RECORDS\r\n");

                // Perform Intra (internal) validation.
                if (IsVerbose)
                {
                    Console.Write("\r\nV! Performing Internal Site Validation.\r\n");
                    Console.Out.Flush();
                }

                // Validate the site tables provided in the PDF (no inter-table validation).
                ValidTS.FtValSiteTSIntra(pdfName, ref errCount, ref warnCount, ref valLevel);

                // Perform MDB and related validation.
                if (IsVerbose)
                {
                    Console.Write("\r\nV! Performing External (MDB) Site Validation.\r\n");
                    Console.Out.Flush();
                }

                // Read all the site records in the PDF, determine if each record is being added,
                // modified or deleted, and validate accordingly.
                ValSite.FtValSiteTS(pdfName, ref errCount, ref warnCount);

                // --- Check for missing opposing sites ---

                if (IsVerbose)
                {
                    Console.Write("\r\nV! Checking for missing call2 sites.\r\n");
                    Console.Out.Flush();
                }

                missingSites = 0;

                // Check whether there are opposing sites that are not included in the PDF.
                rc = ValChan.FtSiteSearch(pdfName, ref errCount, ref warnCount);

                if (rc == Constant.FAILURE)
                {
                    missingSites = 1;
                }

                // Report summary of site validation findings.
                ValErrs.ValErrors();
                str = String.Format("SUMMARY OF SITE RECORDS:  {0:D} errors and {1:D} warnings\r\n", errCount, warnCount);
                Console.Write(str);

                // Update the error and warning totals.
                errTotal += errCount;
                warnTotal += warnCount;

                // --- Perform Antenna Record Validation. --- 
                Console.Write("\r\n\r\n                         TS ANTENNA RECORDS\r\n");


                /* Reset error and warning count variables */
                errCount = warnCount = 0;

                /* Perform 'intra' table validation */
                if (IsVerbose)
                {
                    Console.Write("\r\nV! Performing Internal Antenna Validation.\r\n");
                    Console.Out.Flush();
                }

                // Validate the antenna tables provided in the PDF (no inter-table validation).
                ValidTS.FtValAnteTSIntra(pdfName, ref errCount, ref warnCount, ref valLevel);

                // Perform MDB and related validation.
                if (IsVerbose)
                {
                    Console.Write("\r\nV! Performing external (MDB) Antenna validation.\r\n");
                    Console.Out.Flush();
                }

                // Read all the antenna records in the PDF, determine if each record is being added,
                // modified or deleted, and validate accordingly.
                ValAnte.FtValAnteTS(pdfName, ref errCount, ref warnCount, ref valLevel);

                /*	In the case of passive antennas.  Store the normals in the off-axis
                *		azimuths.  This requires a separate pass, because it requires pulling
                *		in three sites for every angle.  1209 - GJS - 2006.07	*/
                if (IsVerbose)
                {
                    Console.Write("\r\nV! Setting the passive normals.\r\n");
                    Console.Out.Flush();
                }

                ValPassive.FtSetPassiveNormals(pdfName, ref errCount, ref warnCount);

                /* Report summary of antenna validation findings */
                ValErrs.ValErrors();
                Console.Write("SUMMARY OF ANTENNA RECORDS:  ");
                str = String.Format("{0:D} errors and {1:D} warnings\r\n", errCount, warnCount);
                Console.Write(str);
                errTotal += errCount;
                warnTotal += warnCount;


                /* --- Perform Channel Record Validation --- */
                Console.Write("\r\n\r\n                         TS CHANNEL RECORDS\r\n");

                /* Reset error and warning count variables */
                errCount = warnCount = 0;

                /* Perform Intra (internal) validation */
                if (IsVerbose)
                {
                    Console.Write("\r\nV! performing internal channel validation.\r\n");
                    Console.Out.Flush();
                }

                ValidTS.FtValChanTSIntra(pdfName, ref errCount, ref warnCount, ref valLevel);

                /* Perform MDB and related validation */
                if (IsVerbose)
                {
                    Console.Write("\r\nV! Performing external (MDB) channel validation.\r\n");
                    Console.Out.Flush();
                }

                ValChan.FtValChanTS(pdfName, ref errCount, ref warnCount, ref valLevel);

                /* Must go back and check for passive reflectors.  If one
                ** is being used, then must set reflected TX powers from
                ** calculated RX values. */
                if (IsVerbose)
                {
                    Console.Write("\r\nV! Setting passive TX powers.\r\n");
                    Console.Out.Flush();
                }

                ValPassive.FtValSetPassiveTx(pdfName, ref errCount, ref warnCount);

                /* Report summary of channel validation findings */
                ValErrs.ValErrors();
                Console.Write("\r\nSUMMARY OF CHANNEL RECORDS:  ");
                str = String.Format("{0:D} errors and {1:D} warnings.\r\n", errCount, warnCount);
                Console.Write(str);
                errTotal += errCount;
                warnTotal += warnCount;

                /* --- Perform Change of Callsign Record Validation --- */
                /* the validation for change of call sign simply verifies that*/
                /* the old call sign does exist, the new one does not,the name*/
                /* old call sign combination is correct,and the old call sign */
                /* is not referenced in the pdf                               */

                Console.Write("\r\n\r\n                         TS CHANGE OF CALL SIGN RECORDS\r\n");

                /* Reset error and warning count variables */
                errCount = warnCount = 0;

                /* Perform MDB and related validation */
                ValChng.FtValChngTS(pdfName, ref errCount, ref warnCount);

                /* Report summary of channel validation findings */
                ValErrs.ValErrors();
                Console.Write("SUMMARY OF CHANGE OF CALL SIGN RECORDS:  ");
                str = String.Format("{0:D} errors and {1:D} warnings.\r\n\r\n", errCount, warnCount);
                Console.Write(str);
                errTotal += errCount;
                warnTotal += warnCount;


                /* --- Check operator note of person running validation -- */

                UserInfo.UtGetUserInfo(out userInfo);

                userid = userInfo.oper.Trim();

                nRet = Suutils.SuGetOper(userid, out tOper);

                if (IsVerbose)
                {
                    str = String.Format("\r\nV! Getting Oper: {0}, ret={1:D}\r\n", userid, nRet);
                    Console.Write(str);
                    Console.Out.Flush();
                }

                if (nRet != 0)
                {
                    if (nRet != 1)
                    {
                        str = String.Format("Error - Could not determine USERID, reason:[{0:D}]\r\n", nRet);
                        Console.Write(str);
                        Console.Out.Flush();
                        errTotal++;
                    }
                    else
                    {
                        str = String.Format("Error - Could not find your operator name ({0}) in operator list\r\n", userid);
                        Console.Write(str);
                        Console.Out.Flush();
                        errTotal++;
                    }
                }

                if (tOper.opnote.Equals("CO"))
                {
                    Console.Write("ERROR! - Users with operator note of 'CO' cannot Update database.\r\n");
                    Console.Out.Flush();
                    errTotal++;
                }

                /*	Check that there are no orphaned antennas -- 1274 - GJS - 2008.08.06 */
                /*	Go through the finished ft file and pull in each site.  For each site
                *		we will go through the antennas and check that some channel refers to
                *		them.  If not, an error is signalled. */
                if (IsVerbose)
                {
                    Console.Write("\r\nV! Checking for Orphaned antennas...\r\n");
                    Console.Out.Flush();
                }

                //pdfName is an iteration enumeration variable and so it can't be passed using 'ref'.
                //Solution: make a copy of pfdName and pass that by 'ref' instead.
                string shortName = pdfName;

                cLastCall = "";
                string cCall = "";

                //============ Loop over all call signs =============

                while (FtUtils.FtNextCall(out cCall, cLastCall, shortName) == 0)
                {
                    //AH: The following line is VITAL!
                    cLastCall = cCall;

                    FtSiteStr pSite;
                    int nInd;
                    int nIndChan;
                    bool IsReferenced;

                    if (FtUtils.FtGetSite(cLastCall, out pSite, 3, shortName) != 0)
                    {
                        // Something unexpected happened.
                        str = String.Format("\r\n*ERROR* - unreferenced antennas check can't retrieve {0}\r\n{1}\r\n", cLastCall, GenUtil.GetUserMess());
                        Console.Write(str);
                        Console.Out.Flush();
                        errTotal++;
                        break;
                    }
                    else
                    {
                        /*	Go through the antennas trying to find channels that refer to it. */
                        for (nInd = 0; nInd < pSite.nNumAnts; nInd++)
                        {
                            FtAnte pAnt = pSite.stAntsPtr[nInd];
                            IsReferenced = false;

                            if (!pAnt.cmd.ToUpper().Equals("D"))
                            {
                                /*	For everything except deleted antennas  */
                                for (nIndChan = 0; nIndChan < pSite.nNumChans; nIndChan++)
                                {
                                    FtChan pChan = pSite.stChanPtr[nIndChan];
                                    bool identical_call2 = pChan.call2.Equals(pAnt.call2);
                                    bool identical_bndcde = pChan.bndcde.Equals(pAnt.bndcde);
                                    if (identical_call2 && identical_bndcde)
                                    {
                                        /*	The channel is for the same link */
                                        bool rx1 = (pAnt.anum == pChan.antnumbrx1);
                                        bool rx2 = (pAnt.anum == pChan.antnumbrx2);
                                        bool rx3 = (pAnt.anum == pChan.antnumbrx3);
                                        bool tx1 = (pAnt.anum == pChan.antnumbtx1);
                                        bool tx2 = (pAnt.anum == pChan.antnumbtx2);
                                        if (rx1 || rx2 || rx3 || tx1 || tx2)
                                        {
                                            IsReferenced = true;
                                            break;
                                        }
                                    }
                                }

                                if (!IsReferenced)
                                {
                                    str = String.Format("\r\n*ERROR* Antenna from {0} to {1} band {2}, anum {3:D} is not referenced by any channel.", pAnt.call1, pAnt.call2, pAnt.bndcde, pAnt.anum);
                                    Console.WriteLine(str);
                                    errTotal++;
                                }
                            }
                        } //end of: for (nInd = 0; nInd < pSite.nNumAnts; nInd++)
                    }
                } //========== end of loop over all call signs ==========

                if (mDoHiLo)
                {
                    if (IsVerbose)
                    {
                        Console.Write("V! Producing the HiLoCheck report...\r\n");
                        Console.Out.Flush();
                    }

                    dDistKm = mHiLoMinDist * 0.03087;
                    str = String.Format("\r\nHiLoCheck for distance: {0:f2}Km ({1:D} Seconds)\r\n", dDistKm, mHiLoMinDist);
                    Console.Write(str);
                    Console.Out.Flush();

                    nRet = HiLoAnalysis2021.HiloCheckFunc(shortName, IsVerbose, dDistKm, Console.Out);

                    if (nRet < 0)
                    {
                        Console.Write("WARNING - There were hilo processing errors in the file.\r\n");
                        Console.Out.Flush();
                    }
                    else
                    {
                        str = String.Format("There were {0:D} hilo violations.\r\n", nRet);
                        Console.Write(str);
                        Console.Out.Flush();
                    }
                }
                else
                {
                    Console.Write("\r\nNo HiLoCheck will be done.\r\n");
                    Console.Out.Flush();
                } //end of if (nDist >= 0)


                /* --- Update the title record with the validated flag --- */
                if (IsVerbose)
                {
                    Console.Write("V! Updating the Title Record. Getting Data...\r\n");
                    Console.Out.Flush(); ;
                }

                GenUtil.UtCvtName(Constant.FT_TITL, pdfName, out fileName);

                tID = DynTitle.FtSelectTitle(fileName, "", "");
                if (tID < 0)
                {
                    Console.Write("ERROR - Could not read title record \r\n");
                    errTotal++;
                }
                else
                {
                    rc = DynTitle.FtFetchTitle(tID, out ftTitl, out nArrayFWt);

                    if (rc == Constant.SUCCESS)
                    {
                        /*  Title record was read, can update */
                        if (errTotal != 0)
                        {
                            /* If errors, then NOT VALID */
                            ftTitl.validated = string.Format("{0}", Constant.NOT_VALIDATED);
                        }
                        else
                        {
                            /* If valid only for TSIP */
                            if (valLevel == Constant.TSIP)
                            {
                                if (missingSites == 1)
                                {
                                    ftTitl.validated = string.Format("{0}", Constant.M_TSIP_VALIDATED);
                                }
                                else
                                {
                                    ftTitl.validated = string.Format("{0}", Constant.TSIP_VALIDATED);
                                }
                                // Shared links are not currently implemented because they were never used.
                            }
                            else
                            {
                                /* Check for shared links */
                                /* No shared links still outstanding */
                                if (missingSites == 1)
                                {
                                    ftTitl.validated = string.Format("{0}", Constant.M_UPDATE_VALIDATED);
                                }
                                else
                                {
                                    ftTitl.validated = string.Format("{0}", Constant.UPDATE_VALIDATED);

                                }
                            }
                        }
                        if (IsVerbose)
                        {
                            Console.Write("\r\nV! Updating the title record, I/O...\r\n");
                            Console.Out.Flush();
                        }
                        /* Perform the update to the title record */

                        nArrayFWt[Constant.FT_TITL_VALIDATED] = Constant.DB_NOT_NULL;
                        rc = DynTitle.FtUpdateTitle(tID, ftTitl, nArrayFWt);
                        if (rc != Constant.SUCCESS)
                        {
                            Console.Write("ERROR - Could not update title record \r\n");
                            Console.Out.Flush();
                            ErrMsg.UtErrMessage(rc);
                        }
                        DynTitle.FtCloseTitle(tID);

                        UserInfo.UtUpdateCentralTable("U", shortName, Constant.FT, ftTitl.validated, "N");

                    }
                    else
                    {
                        Console.Write("ERROR - There is no title record in the PDF \r\n");
                        Console.Out.Flush();
                        errTotal++;
                    }
                }
                ValErrs.ValErrors();

                str = String.Format("\r\n\r\nThere were a total of {0:D} errors and {1:D} warnings.\r\n", errTotal, warnTotal);
                Console.Write(str);
                Console.Out.Flush();

                if (errTotal != 0)
                {
                    str = String.Format("The PDF '{0}' is invalid\r\n", shortName);
                    Console.Write(str);
                    Console.Out.Flush();
                }
                else
                {
                    str = String.Format("The PDF '{0}' is validated for :{1}:\r\n\r\n", shortName, Main_level[valLevel]);
                    Console.Write(str);
                    Console.Out.Flush();
                    if (valLevel == Constant.TSIP)
                    {
                        Console.Write("The PDF contains temporary ($) codes and MDB_UPDATE " + "is not allowed\r\n\r\n");
                        Console.Out.Flush();
                    }
                }

                /* Get system date and time */
                GenUtil.UtGetDateTime(out curDate, out curTime);
                str = String.Format("PDF Validation Completed - {0}\r\n", curTime);
                Console.Write(str);
                Console.Out.Flush();

                //======= end of PDF processing ===========================================

                // Although using a 'goto' construct is somewhat taboo it has been adopted
                // in this Main() method to provide a single point of exit from the application
                // that ensures release the hold on the WRITE queue, writes the billing record 
                // and releases ODBC resources.
                cleanUpAndExit:

                BiUtil.BiBillingRec(Constant.BI_END, "");

                //AH:
                //The following call frees up the ODBC handle to 'Environment' 
                //resources that were used during the session.
                Ssutil.UtDisconnect(userSession);

                // Free the Mutex on the DB.
                Qutils.ExitQueue(Info.DbName, "READ");

                // Exit the application with a meaningful exit code.
                // The application is only 'successful' when both exitCode and errTotal are zero.
                // If exitCode is non-zero then the application failed before it attempted to validate the PDF.
                // If exitCode is zero but errTotal is non-zero then the application 'failed' because
                // of bona fide validation errors.
                Application.ExitQuietly(exitCode != 0 ? exitCode : errTotal);

            } //try
            catch (Win32Exception w)
            {
                StringBuilder sb = new StringBuilder("\r\n\r\n");
                sb.AppendLine("FtValidate.Main(): ERROR: unhandled Win32Exception:");
                sb.AppendLine(w.Source);
                sb.AppendLine(w.Message);
                sb.AppendLine(w.StackTrace);
                sb.AppendLine("Execution of FtValidate.exe has been terminated.");
                Console.Write(sb.ToString());
                Log2.e(sb.ToString());
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }
            catch (Exception e)
            {
                StringBuilder sb = new StringBuilder("\r\n\r\n");
                sb.AppendLine("FtValidate.Main(): ERROR: unhandled Managed Exception:");
                sb.AppendLine(e.Source);
                sb.AppendLine(e.Message);
                sb.AppendLine(e.StackTrace);
                sb.AppendLine("Execution of FtValidate.exe has been terminated.");
                Console.Write(sb.ToString());
                Log2.e(sb.ToString());
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

        } // ----- End of main -----

        /// <summary>
        /// This method deletes all 'C'omputer generated records from the PDF; this prevents 
        /// outdated records from previous validations from causing problems.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="nErrors"> - returns the cummulative number of errors encountered.</param>
        private static void FtValPurgeRecords(string pdfName, ref short nErrors)
        {
            //SQLLEN[] nArrayFWt = Arrays.InitializeWithDefaultInstances<SQLLEN>(Constant.FT_TITL_SIZE_);
            SQLLEN[] nArrayFWs = Arrays.CreateArrayUsingDefaultElementConstructor<SQLLEN>(Constant.FT_SITE_SIZE_);
            SQLLEN[] nArrayFWa;
            SQLLEN[] nArrayFWc;

            int id1;
            int id2;
            string tableName = new string(new char[Constant.TABLE_NM_SZ]);
            FtSite ftSite = new FtSite();
            FtAnte ftAnte;
            FtChan ftChan = new FtChan();
            int nRet;

            /* read site information which are computer generated */
            GenUtil.UtCvtName(Constant.FT_SITE, pdfName, out tableName);
            id1 = DynSite.FtSelectSite(tableName, "recstat = 'C'", "");
            if (id1 < 0)
            {
                ValErrs.AddMess("PURGE 1 - could not read site information. Reason %d", pdfName, "E", id1.ToString());
                nErrors++;
                return;
            }

            /* delete computer generated sites */
            while (DynSite.FtFetchSite(id1, out ftSite, out nArrayFWs) == Constant.SUCCESS)
            {
                // Bug fix: b150722A
                // If the ftSite has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftSite to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftSite.cmd))
                {
                    continue;
                }

                /*	If the command has been changed to anything other than N or U, then
                *		its probable that the user has made a change without altering the
                *		recstat. This is part of 1070 -- GJS -- 2006.06 */
                if (Strings.Not_N_or_U(ftSite.cmd))
                {
                    /*	The user has changed the record.  Say so. */
                    ftSite.recstat = "U";
                    DynSite.FtUpdateSite(id1, ftSite, nArrayFWs);
                    ValErrs.AddMess("PURGE 2 - Site has recstat 'C' and command %s. Changed to recstat 'U'.", ftSite.call1, "W", ftSite.cmd);
                }
                else
                {
                    nRet = DynSite.FtDeleteSite(id1);
                    if (nRet != Constant.SUCCESS)
                    {
                        ValErrs.AddMess("PURGE 3 - Could not delete.  Reason %d", ftSite.call1, "E", nRet.ToString());
                        nErrors++;
                    }
                }
            } //end of while()

            DynSite.FtCloseSite(id1);

            /* read ante information which are computer generated */
            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);

            id1 = DynAntenna.FtSelectAntenna(tableName, "recstat = 'C'", "");
            if (id1 < 0)
            {
                ValErrs.AddMess("PURGE 4 - could not read antenna information. Reason %d", tableName, "E", id1.ToString());
                nErrors++;
                return;
            }


            /* delete computer generated antenna */
            while (DynAntenna.FtFetchAntenna(id1, out ftAnte, out nArrayFWa) == Constant.SUCCESS)
            {
                // Bug fix: b150722A
                // If the ftAnte has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftAnte to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftAnte.cmd))
                {
                    continue;
                }

                if (Strings.Not_N_or_U(ftAnte.cmd))
                {
                    /*	The user has changed the record.  Say so. */
                    ftAnte.recstat = "U";
                    DynAntenna.FtUpdateAntenna(id1, ftAnte, nArrayFWa);
                    string keyLine = ValErrs.MakeKeyLine(ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum, null);
                    ValErrs.AddMess("PURGE 5 - Antenna has recstat 'C' and command %s. Changed to recstat 'U'.", keyLine, "W", ftAnte.cmd);
                }
                else
                {
                    nRet = DynAntenna.FtDeleteAntenna(id1);
                    if (nRet != Constant.SUCCESS)
                    {
                        string keyLine = ValErrs.MakeKeyLine(ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum, null);
                        ValErrs.AddMess("PURGE 6 - Cannot delete antenna.  Reason: %d", keyLine, "E", nRet.ToString());
                        nErrors++;
                    }
                }
            }
            DynAntenna.FtCloseAntenna(id1);


            /* read chan information which are computer generated */
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);
            if ((id1 = DynChannel.FtSelectChannel(tableName, "recstat = 'C'", "")) < 0)
            {
                ValErrs.AddMess("PURGE 7 - could not read channel information. Reason %d", tableName, "E", id1.ToString());
                nErrors++;
                return;
            }

            /* delete computer generated chans */
            while (DynChannel.FtFetchChannel(id1, out ftChan, out nArrayFWc) == Constant.SUCCESS)
            {
                // Bug fix: b150722A
                // If the ftChan has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftChan to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftChan.cmd))
                {
                    continue;
                }

                if (Strings.Not_N_or_U(ftChan.cmd))
                {
                    /*	The user has changed the record.  Say so. */
                    ftChan.recstat = "U";
                    DynChannel.FtUpdateChannel(id1, ftChan, nArrayFWc);
                    string keyLine = ValErrs.MakeKeyLine(ftChan.call1, ftChan.call2, ftChan.bndcde, 0, ftChan.chid);
                    ValErrs.AddMess("PURGE 8 - Channel has recstat 'C' and command %s. Changed to recstat 'U'.", keyLine, "W", ftChan.cmd);
                }
                else
                {
                    string keyLine = ValErrs.MakeKeyLine(ftChan.call1, ftChan.call2, ftChan.bndcde, 0, ftChan.chid);
                    if ((nRet = DynChannel.FtDeleteChannel(id1)) != Constant.SUCCESS)
                    {
                        ValErrs.AddMess("PURGE 9 - Cannot delete channel.  Reason: %d", keyLine, "E", nRet.ToString());
                        nErrors++;
                    }
                }
            }

            DynChannel.FtCloseChannel(id1);


            /*****************************************************************************
        // *
          *   Now we go through the sites, antennas and channels and make sure that all
          *   the keys needed are present.  These are call1s for sites, call1,call2,and
          *   band for antennas, and call1,call2,band, and chid for channels.
            *
            \*****************************************************************************/
            GenUtil.UtCvtName(Constant.FT_SITE, pdfName, out tableName);
            if ((id2 = DynSite.FtSelectSite(tableName, "", "")) < 0)
            {
                ValErrs.AddMess("PURGE 10 - could not read site information. Reason %d", tableName, "E", id2.ToString());
                nErrors++;
                return;
            }

            /* delete sites with no call1 */
            while (DynSite.FtFetchSite(id2, out ftSite, out nArrayFWs) == Constant.SUCCESS)
            {
                // Bug fix: b150722A
                // If the ftSite has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftSite to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftSite.cmd))
                {
                    continue;
                }

                /*	If the command has been changed to anything other than N or U, then
                *		its probable that the user has made a change without altering the
                *		recstat. This is part of 1070 -- GJS -- 2006.06 */
                if (ftSite.call1.Length == 0)
                {
                    /*  There is no call1.  Say so. */
                    ValErrs.AddMess("PURGE 11 - Site named '%s' has no call sign.", tableName, "E", ftSite.name);
                    nErrors++;
                }
            }
            DynSite.FtCloseSite(id2);


            /* read ante information  */
            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);

            if ((id1 = DynAntenna.FtSelectAntenna(tableName, "", "")) < 0)
            {
                ValErrs.AddMess("PURGE 12 - could not read antenne information. Reason %d", tableName, "E", id1.ToString());
                nErrors++;
                return;
            }

            /* delete antenna with no call1, call2, band or anum */
            while (DynAntenna.FtFetchAntenna(id1, out ftAnte, out nArrayFWa) == Constant.SUCCESS)
            {
                // Bug fix: b150722A
                // If the ftAnte has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftAnte to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftAnte.cmd))
                {
                    continue;
                }

                if (ftAnte.call1.Length == 0 || ftAnte.call2.Length == 0 || ftAnte.bndcde.Length == 0 || ftAnte.anum == 0)
                {
                    string keyLine = ValErrs.MakeKeyLine(ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum, null);
                    ValErrs.AddMess("PURGE 13 - Antenna has at least one of call1, " + "call2, band code, or anum blank.", keyLine, "E");
                    nErrors++;
                }
            }
            DynAntenna.FtCloseAntenna(id1);


            /* read chan information */
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);
            if ((id1 = DynChannel.FtSelectChannel(tableName, "", "")) < 0)
            {
                ValErrs.AddMess("PURGE 14 - could not read channel information.  Reason %d", tableName, "E", id1.ToString());
                nErrors++;
                return;
            }

            /* delete computer generated chans */
            while (DynChannel.FtFetchChannel(id1, out ftChan, out nArrayFWc) == Constant.SUCCESS)
            {
                // Bug fix: b150722A
                // If the ftChan has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftChan to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftChan.cmd))
                {
                    continue;
                }

                if (ftChan.call1.Length == 0 || ftChan.call2.Length == 0 || ftChan.bndcde.Length == 0 || ftChan.chid.Length == 0)
                {
                    /* No complete channel key. */
                    string keyLine = ValErrs.MakeKeyLine(ftChan.call1, ftChan.call2, ftChan.bndcde, 0, ftChan.chid);
                    ValErrs.AddMess("PURGE 15 - Channel has at least one of call1, " + "call2, band code, or channel id blank.", keyLine, "E");
                    nErrors++;
                }
            }

            DynChannel.FtCloseChannel(id1);

        } // ----- End of FtValPurgeRecords -----

        /// <summary>
        /// This method parses the command line arguments prescribed by the user.
        /// </summary>
        /// <param name="args"> - User prescribed command line arguments.</param>
        public static void ParseCommandLineArgs(string[] args)
        {
            if (args.Length == 0)
            {
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            int redirectCount = 0;
            string outFilePath = "";
            FileMode fileMode = FileMode.Create;

            // Create separate lists of 'flag' args prefixed with '-' and those that
            // are not.
            List<string> flagArgs = new List<string>();
            List<string> regularArgs = new List<string>();

            foreach (string arg in args)
            {
                if (Strings.IsNumeric(arg))
                {
                    regularArgs.Add(arg);
                }
                else if (arg.StartsWith("-"))
                {
                    flagArgs.Add(arg);
                }
                else
                {
                    regularArgs.Add(arg);
                }
            }

            // Parse the flags.
            foreach (string arg in flagArgs)
            {
                // Check that we don't just have a '-' character.
                if (arg.Length == 1)
                {
                    //Console.Write("\r\n ERROR: Invalid flag: '{0}'", arg);
                    //WriteUsageToConsole();
                    //Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                }

                if (arg.StartsWith("-O", StringComparison.OrdinalIgnoreCase))
                {
                    // Redirect Console.Out to stream to a text file.
                    redirectCount++;
                    outFilePath = arg.Substring(2);
                    fileMode = FileMode.Create;
                }
                else if (arg.StartsWith("-A", StringComparison.OrdinalIgnoreCase))
                {
                    // Similar to the '-o' option but file mode is APPEND.
                    redirectCount++;
                    outFilePath = arg.Substring(2);
                    fileMode = FileMode.Append;
                }
                else if (arg.StartsWith("-V", StringComparison.OrdinalIgnoreCase))
                {
                    //	Verbose output has been prescribed.
                    IsVerbose = true;
                }
                else if (arg.StartsWith("-S", StringComparison.OrdinalIgnoreCase))
                {
                    // Spoofing mode has been prescribed.
                    Info.SpoofModeIsOff = false;
                }
                else if (arg.StartsWith("-!", StringComparison.OrdinalIgnoreCase))
                {
                    if (arg.Length > 2)
                    {
                        string schemaName = arg.Substring(2);

                        // If we don't check the user's permissions now the program may fail at
                        // run time with an SQL error.
                        if (User.IsInAdministratorGroup())
                        {
                            // Save the schema to the Info static structure member.
                            // This has a global effect throughout the code so that all synthesized SQL
                            // queries will explicitely include the schema name 'forced' on the command-line.
                            Info.GlobalSchema = schemaName;
                            Ssutil.Glb_Schema = schemaName;
                        }
                        else
                        {
                            Console.Write("\n The -! option is restricted to users who are members of the Administrators Group.");
                            WriteUsageToConsole();
                            Environment.Exit(98);
                        }
                    }
                    else
                    {
                        Console.Write("\n Invalid syntax for option -!");
                        WriteUsageToConsole();
                        Environment.Exit(98);
                    }
                }
                else if (arg.StartsWith("-M:", StringComparison.OrdinalIgnoreCase))
                {
                    /*	Set the minimum distance for Hilocheck in seconds of latitude. */
                    // Need to handle "-m:", "-m:-3", "-m:12" etc
                    string numStr = arg.Substring(3);
                    if (!String.IsNullOrWhiteSpace(numStr))
                    {
                        // Attempt to convert the numStr to an integer.
                        try
                        {
                            mHiLoMinDist = Int32.Parse(numStr);
                            if (mHiLoMinDist < 0)
                            {
                                // Negative number prescribes that HiLo is not to be run.
                                mDoHiLo = false;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("\r\n ERROR: -m: option: invalid integer after colon: " + e.Message);
                            Application.Exit(98);
                        }
                    }

                }

            }

            // Parse the regular arguments; there must be qty. 3 of them.
            if (regularArgs.Count != 3)
            {
                Console.Write("\r\n ERROR: Too few or too many mandatory arguments; should be qty. 3.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Parse the qty. 3 mandatory arguments.
            Info.DbName = regularArgs[0];
            Info.ProjectCode = regularArgs[1];
            Info.PdfName = regularArgs[2];

            // Finally, if requested, handle the redirection of Console.Out to a file.
            if (redirectCount > 1)
            {
                Console.Write("\r\n ERROR: The -o and/or -a option can only appear once.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }
            else if (redirectCount == 1)
            {
                Application.RedirectConsoleOutToFile(outFilePath, fileMode);
            }

            //...Log2.v("\nInfo.DbName      = " + Info.DbName);
            //...Log2.v("\nInfo.ProjectCode = " + Info.ProjectCode);
            //...Log2.v("\nInfo.PdfName     = " + Info.PdfName);
            for (int i = 0; i < flagArgs.Count; i++)
            {
                //...Log2.v(String.Format("\nflagArgs[{0}]      = {1}", i, flagArgs[i]));
            }

            Log2.Write("\n\nPDFname = " + Info.PdfName);
        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program is the main TS user input data validation program: its function is to  +");
            Console.Write("\r\n ensure that the user-input (as read in and stored in the database by FtImport)      +");
            Console.Write("\r\n comprises semantically valid and coherent data that can be successfully used by     +");
            Console.Write("\r\n the TSIP calculation routines and administrative and billing functions.   ");
            Console.Write("\r\n");
            Console.Write("\r\n -------------------------------------------------------------------------------------");
            Console.Write("\r\n IMPORTANT: FtValidate.exe changes/augments the user's prescribed PDF ft_ table set  +");
            Console.Write("\r\n =========  by using MDB data to insert a site's 'unreferenced' antenna and channel  +");
            Console.Write("\r\n            records, recalculating distances, azimuths and elevations etc.           +");
            Console.Write("\r\n                                                                                     +");
            Console.Write("\r\n            When testing, *ALWAYS* run FtImport.exe before running FtValidate to     +");
            Console.Write("\r\n            ensure that the initial conditions are the same for each test run.");
            Console.Write("\r\n -------------------------------------------------------------------------------------");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: FtValidate dbname project pdfName [-!<schema>] [-v] [-o<filePath> | -a<filePath>] [-m:<hiloDist>] ");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        project          : user's project 'charge' code.");
            Console.Write("\r\n        pdfName          : the XXX in user table ft_XXX_site etc.");
            Console.Write("\r\n");
            Console.Write("\r\n        --- Options --------------------------------------------------------------------------");
            Console.Write("\r\n");
            Console.Write("\r\n        -v              : verbose; provides additional output messages that indicate progress.");
            Console.Write("\r\n        -o<filePath>    : redirect console output to write to a file.");
            Console.Write("\r\n                        : if the output file already exists then it is overwritten.");
            Console.Write("\r\n                        : filePath should be the fully-qualified path to the output file.");
            Console.Write("\r\n        -a<filePath>    : same as the -o option except that the output file is appended to.");
            Console.Write("\r\n        -m:<hiloDist>   : prescribes the minimum (integer) seconds of latitude for HiloCheck.");
            Console.Write("\r\n                        : if this option is absent then hiloDist defaults to 7s (216m).");
            Console.Write("\r\n                        : if hiloDist is negative then the HiloCheck is not run.");
            Console.Write("\r\n        -s              : enables MDB and SDB table 'spoofing' mode.");
            Console.Write("\r\n        -!<schema>      : creates and populates user tables with the precribed schema name.");
            Console.Write("\r\n                          Usage restricted to users who are members of the Administrator Group.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n        FtValidate fcsa hulme1_0 -v -m:-1 -ad:\\users\\ahulme\\results.txt tafloutv2");
            Console.Write("\r\n");
            Console.Write("\r\n        FtValidate fcsa hulme1_0 -!fmda2 -m:-1 -ad:\\users\\ahulme\\results.txt tafloutv2");
            Console.Write("\r\n");
            Console.Write("\r\n NOTES:");
            Console.Write("\r\n       1. Options can appear in any order and at any position in the argument list.");
            Console.Write("\r\n       2. The -o and -a options are mutually exclusive.");
            Console.Write("\r\n       3. Options can be placed anywhere, in any order, on the command-line.");
            Console.Write("\r\n\r\n BUILD: " + Info.ManagedBuildInfo());
        }

    } //class
} //namespace

```
