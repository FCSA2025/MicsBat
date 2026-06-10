# Documented File: FeValidate.cs
**Repository Path:** `FeValidate\FeValidate.cs`
**Primary Layer:** `FeValidate`
**Namespace:** `FeValidate`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//       10        20        30        40        50        60        70        80
//345678901234567890123456789012345678901234567890123456789012345678901234567890

/// <summary>
/// This program validates the information content of a previously-imported ES 
/// data file.
/// </summary>
/// <remarks>
/// \image html "Usage - FeValidate.PNG" "Usage - FeValidate"
/// FeValidate is the main Earth Station input data validation program. Its 
/// function is to ensure that the input read in and stored in the database by 
/// FeImport is a “well-formed” set of site descriptions that can be used by 
/// the interference calculation routines and also in the administrative and 
/// billing functions. It also calculates the paths and orientation to the 
/// satellite and its arc. 
/// <para>
/// By “well-formed” we mean that the imported ES data file contain all the data
/// needed to perform interference calculations, billing functions, and other 
/// administrative functions and that all the information they contain is correct
/// and reasonable.To do this, it checks various codes, check the ordering and 
/// completeness of data and calculates fields that will later be used in the 
/// interference calculations.
/// </para>
/// <para>
/// The output of FeValidate is the input to the database update program and 
/// becomes the 'official' version stored in the database.
/// </para>
/// <para>
/// An overview of the validation process performed by FeValidate is provided 
/// below:-
/// </para>
/// <list>
/// <item>FeValidate starts merging the records in the input file with the data 
/// already in the database, record type by record type;
/// </item>
/// <item>First it merges the site records using the method FeValMergeSiteRecords,
/// then the antenna records using FeValMergeAnteRecords, then the channel 
/// records using FeValMergeChanRecords, and finally the azimuth records using 
/// FeValMergeAzimRecords;
/// </item>
/// <item>It then proceeds with the actual validation of the input information,
/// record type by record type.</item>
/// <item>FeValSiteIntra is called to check the values and formats in the input
/// site records, and then FeValSiteES is called to make the command dependent 
/// checks (‘A’dd cannot have a corresponding record in the database, ‘D’elete 
/// must have, etc.) and then check the fields themselves, such as the presence 
/// of mandatory fields, and if the field is a code that the code is in the 
/// subsidiary database.
/// </item>
/// <item>FeValidate then goes through the corresponding checking routines for 
/// antennas, channels and azimuths, in the same manner as for sites.
/// </item>
/// <item>It then validates a change of location (the main key to the table) and 
/// a change of call sign (for one of the antennas) using FeValClocES and FeValCCalES.
/// </item>
/// <item>FeValidate then gets the user’s information and operator information. 
/// If the opnote of the user is ‘CO’, then the user cannot update the database 
/// and validation fails.
/// </item>
/// <item>The title record for the file is retrieved and updated with the 
/// validation status depending on the number of errors encountered.
/// </item>
/// <item>Finally, the billing record is completed, the ODBC connection with
/// the database is closed and the “READ” queue is exited and the FeValidate 
/// program ends, with a return code equal to the total number of errors.
/// </item>
/// </list>
/// </remarks>
namespace FeValidate
{
    using _DataStructures;
    using _Utillib;
    using _NewLib;
    using SQLLEN = Int64;
    using _Configuration;
    using System.IO;
    using System.ComponentModel;
    /// <summary>
    /// This class encapsulates the Main method and supporting methods and data 
    /// that provide the functionality of the FeValidate program.
    /// </summary>
    public class FeValidate
    {
        private const int USER_SESSION = 1;
        private static string[] level = new string[] { "", "UPDATE", "TSIP ONLY" };
        private static TextWriter mTW = Console.Out;

        /// <summary>
        /// This is the main method that provides top-level control of the FeValidate program.
        /// </summary>
        /// <remarks>
        /// As a minimum, FeValidate must be called with qty.3 command line arguments:
        /// <list type="bullet">
        /// <item>database name;</item>
        /// <item>project code;</item>
        /// <item>name of a previously imported PDF.</item>
        /// </list>
        /// FeValidate reads in the previously imported PDF data and augments
        /// it with values from the main and subsiduary tables, as required.<p>
        /// By default, FeValidate writes a report of its findings to Console.Out.<p>
        /// Alternatively, using an optional command line argument, FeValidate's 
        /// report can be redirected to write a file.
        /// </remarks>
        /// <param name="args"> - string[] of command line arguments.</param>
        static void Main(string[] args)
        {
            string opnote = "";
            string userid;

            UserInfoData userInfoData;
            SuOper tOper;

            short errCount = 0;
            short errTotal = 0;
            short warnCount = 0;
            short warnTotal = 0;
            short valLevel;
            short rc;
            bool azimRecChanged = false;
            string validatedFor;
            string curDate;
            string curTime;
            int nRet;
            string pdfName;


            // Enable developmental logging.
#if false
            string mLog2FilePath = @"d:\MicsBatchLogs\FeValidate.log";
            if (Log2.SetLogFilePath(mLog2FilePath))
            {
                Log2.Erase();
                Log2.Set(Log2.FileOpenClose.PER_SESSION);
                Log2.Set(Log2.WriteMode.ENABLED);
                Log2.Set(Log2.Level.VERBOSE);
                Log2.v("\nBuild: " + Info.BuildMetaData);
            }
            else
            {
                Console.Error.Write("\r\nERROR: could not open Log2 file: " + mLog2FilePath);
            }
#endif

            // Top-level application try-catch.
            try
            {
                ParseCommandLineArgs(args);

                GetEnvVariables();

                if ((nRet = Qutils.EnterQueue(Info.DbName, "READ", 30)) != 0)
                {
                    Qutils.ExplainQueue(Info.DbName, "READ", nRet, null);
                    Application.ExitQuietly(100);
                }

                nRet = Ssutil.UtConnect(Info.DbName, USER_SESSION);
                // Check the status of the connect command.
                if (nRet != 0)
                {
                    // Can't connect to database. 
                    ErrMsg.UtPrintMessage(Error.NODATABASE, Info.DbName);
                    Qutils.ExitQueue(Info.DbName, "READ");
                    Application.ExitQuietly(101);
                }

                BiUtil.BiBillingRec("FEVALIDATE", Info.PdfName);

                GenUtil.UtGetDateTime(out curDate, out curTime);

                Console.Write("ES PDF VALIDATION REPORT    Build {0}    DATE: {1}  {2}\r\nProject Code [{3}] Process ID [{4}]\r\n",
                                 Info.CsharpBuildInfo, curDate, curTime, Info.ProjectCode, Info.ProcessID);

                // =================== Commence validation processing. =========================

                pdfName = Info.PdfName;

                Console.Write("PDF Name: {0}\r\n\r\n", pdfName);

                // Verify that the pdf exists.
                if (Ssutil.UtTableExist(Constant.FE, pdfName) == false)
                {
                    Console.Write("ERROR - PDF '{0}' does not exist\r\n", pdfName);
                    goto cleanUpAndExit;
                }

                // If file has already updated the mdb, dont validate. 
                if ((rc = (short)FilewUtil.UtFilewValidated(Constant.FE, pdfName, out validatedFor)) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(Error.FILEWVAL);
                    goto cleanUpAndExit;
                }

                if (validatedFor == Constant.UPDATE_POSTED.ToString())
                {
                    ErrMsg.UtPrintMessage(Error.ALREADYPOSTED, "ES", pdfName, "Validation");
                    goto cleanUpAndExit;
                }

                // Initialize the total error and total warning counters for this PDF.
                errTotal = 0;
                warnTotal = 0;

                // Prescribe the level of validation required.
                valLevel = Constant.UPDATE;

                // The following method calls delete all computer generated records and 
                // fill out all the missing information on the records from 
                // the MDB, as well as retrieve all the records  
                // associated with the current ones (ie. get all antennas and 
                // channels for this site, the site and all channels for this 
                // antenna or the site and antennas for this channel.         

                ValMerge.FeValPurgeRecords(pdfName);

                ValMerge.FeValMergeSiteRecords(pdfName, ref azimRecChanged);

                ValMerge.FeValMergeAnteRecords(pdfName, ref azimRecChanged);

                ValMerge.FeValMergeChanRecords(pdfName);

                ValMerge.FeValMergeAzimRecords(pdfName);

                //warnTotal += warnCount;

                // Write out the currently accumulated error, warning and informational 
                // messages and then clear their caches. 
                ValErrs.ValErrors();

                // For each record type, the err and warning counter is reset 
                // to zero.  The internal PDF validation (same checks as the  
                // vifred screens) is performed in the ..ESIntra procedure, & 
                // all other levels of validation are done in the feVal..ES   
                // procedures.  The number of errors and warnings are printed 
                // at the end, as well as the level of validation (TSIP or    
                // update).						      */

                // Validate ES PDF site records.
                Console.Write("\r\n                      ES SITE RECORDS\r\n");
                errCount = warnCount = 0;
                ValidES.FeValSiteIntra(ref errCount, ref warnCount, pdfName);
                ValSite.FeValSiteES(pdfName, ref errCount, ref warnCount);
                ValErrs.ValErrors();
                Console.Write("SUMMARY OF SITE RECORDS: {0} errors and {1} warnings\r\n",
                                 errCount, warnCount);
                errTotal += errCount;
                warnTotal += warnCount;

                // Validate ES PDF antenna records.
                Console.Write("\r\n\r\n                      ES ANTENNA RECORDS\r\n");
                errCount = warnCount = 0;
                ValidES.FeValAnteIntra(ref errCount, ref warnCount, pdfName);
                ValAnte.FeValAnteES(pdfName, ref errCount, ref warnCount, ref valLevel);
                ValErrs.ValErrors();
                Console.Write("SUMMARY OF ANTENNA RECORDS: {0} errors and {1} warnings\r\n",
                                 errCount, warnCount);
                errTotal += errCount;
                warnTotal += warnCount;

                // Validate ES PDF channel records.
                Console.Write("\r\n\r\n                      ES CHANNEL RECORDS\r\n");
                errCount = warnCount = 0;
                ValidES.FeValChanIntra(ref errCount, ref warnCount, pdfName);
                ValChan.FeValChanES(pdfName, ref errCount, ref warnCount, ref valLevel);
                ValErrs.ValErrors();
                Console.Write("SUMMARY OF CHANNEL RECORDS: {0} errors and {1} warnings.\r\n",
                                 errCount, warnCount);
                errTotal += errCount;
                warnTotal += warnCount;

                // Validate ES PDF azimuth records.
                Console.Write("\r\n\r\n                      ES AZIMUTH RECORDS\r\n");
                errCount = warnCount = 0;
                ValidES.FeValAzimIntra(ref errCount, ref warnCount, pdfName);
                ValAzim.FeValAzimES(pdfName, ref errCount, ref warnCount, ref azimRecChanged);
                ValErrs.ValErrors();
                Console.Write("SUMMARY OF AZIMUTH RECORDS: {0} errors and {1} warnings.\r\n",
                                 errCount, warnCount);
                errTotal += errCount;
                warnTotal += warnCount;

                // Validate ES PDF change of location records.
                Console.Write("\r\n\r\n                      ES CHANGE OF LOCATION RECORDS\r\n");
                errCount = warnCount = 0;
                ValCLoc.FeValCLocES(pdfName, ref errCount, ref warnCount);
                ValErrs.ValErrors();
                Console.Write("SUMMARY OF CHANGE OF LOCATON RECORDS: {0} errors and {1} warnings.\r\n",
                                 errCount, warnCount);
                errTotal += errCount;
                warnTotal += warnCount;

                // Validate ES PDF change of callsign records.
                Console.Write("\r\n\r\n                      ES CHANGE OF CALL SIGN RECORDS\r\n");
                errCount = warnCount = 0;
                ValCCal.FeValCCalES(pdfName, ref errCount, ref warnCount);
                ValErrs.ValErrors();
                Console.Write("SUMMARY OF CHANGE OF CALL SIGN RECORDS: {0} errors and {1} warnings.\r\n",
                                 errCount, warnCount);
                errTotal += errCount;
                warnTotal += warnCount;

                // Check the operator note of user running validation.

                UserInfo.UtGetUserInfo(out userInfoData);

                userid = userInfoData.oper.Trim();

                rc = (short)Suutils.SuGetOper(userid, out tOper);

                if (rc != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        Console.Write("Error - Could not determine USERID\r\n");
                        errTotal++;
                    }
                    else
                    {
                        Console.Write("Error - Could not find operator name in operator list\r\n");
                        errTotal++;
                    }
                }
                else
                {
                    opnote = tOper.opnote;
                }

                if (opnote.Equals("CO"))
                {
                    Console.Write("ERROR! - Users with operator note of 'CO' cannot Update database.\r\n");
                    errTotal++;
                }

                // Update this PDF's title record 'validated flag' to "U" if errTotal 
                // is zero or to "N" if any validation errors were encountered.
                UpdateTitleRecord(pdfName, valLevel, ref errTotal);

                // Write out the total counts of errors and warnings.
                Console.Write("\r\n\r\nThere were a total of {0} errors and {1} warnings.\r\n",
                                 errTotal, warnTotal);

                // Write out the final verdict of the validation: the PDF is 
                // either valid (no errors, but perhaps warnings) or invalid.
                if (errTotal != 0)
                {
                    Console.Write("The PDF '{0}' is invalid\r\n", pdfName);
                }
                else
                {
                    Console.Write("The PDF '{0}' is validated for :{1}:\r\n\r\n", pdfName, level[valLevel]);
                    if (valLevel == Constant.TSIP)
                    {
                        Console.Write("The PDF contains temporary ($) codes so MDB_UPDATE is not allowed\r\n\r\n");
                    }
                }

                // =================== End of validation processing. =========================

                // Although using a 'goto' construct is somewhat taboo it has been adopted
                // in this Main() method to provide a single point of exit from the application
                // that ensures release the hold on the READ queue, writes the billing record 
                // and releases ODBC resources.
                cleanUpAndExit:

                // Say goodbye.
                GenUtil.UtGetDateTime(out curDate, out curTime);
                Console.Write("PDF Validation Completed -  {0}\r\n", curTime);


                BiUtil.BiBillingRec(Constant.BI_END, "");

                Ssutil.UtDisconnect(USER_SESSION);

                Qutils.ExitQueue(Info.DbName, "READ");

                Application.RestoreConsoleOut();

                Application.ExitQuietly(errTotal);
            }
            catch (Win32Exception w)
            {
                StringBuilder sb = new StringBuilder("\r\n\r\n");
                sb.AppendLine("FeValidate.Main(): ERROR: unhandled Win32Exception:");
                sb.AppendLine(w.Source);
                sb.AppendLine(w.Message);
                sb.AppendLine(w.StackTrace);
                sb.AppendLine("Execution of FeValidate.exe has been terminated.");
                Console.Write(sb.ToString());
                Log2.e(sb.ToString());
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }
            catch (Exception e)
            {
                StringBuilder sb = new StringBuilder("\r\n\r\n");
                sb.AppendLine("FeValidate.Main(): ERROR: unhandled Managed Exception:");
                sb.AppendLine(e.Source);
                sb.AppendLine(e.Message);
                sb.AppendLine(e.StackTrace);
                sb.AppendLine("Execution of FeValidate.exe has been terminated.");
                Console.Write(sb.ToString());
                Log2.e(sb.ToString());
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when FeValidate
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program is the main ES user input data validation program: its function is to  +");
            Console.Write("\r\n ensure that the user-input (as read in and stored in the database by FeImport)      +");
            Console.Write("\r\n comprises semantically valid and coherent data that can be successfully used by     +");
            Console.Write("\r\n the TSIP calculation routines and administrative and billing functions.   ");
            Console.Write("\r\n");
            Console.Write("\r\n -------------------------------------------------------------------------------------");
            Console.Write("\r\n IMPORTANT: FeValidate.exe changes/augments the user's prescribed PDF fe_ table set  +");
            Console.Write("\r\n =========  by using MDB data to insert a site's 'unreferenced' antenna and channel  +");
            Console.Write("\r\n            records, recalculating distances, azimuths and elevations etc.           +");
            Console.Write("\r\n                                                                                     +");
            Console.Write("\r\n            When testing, *ALWAYS* run FeImport.exe before running FeValidate to     +");
            Console.Write("\r\n            ensure that the initial conditions are the same for each test run.");
            Console.Write("\r\n -------------------------------------------------------------------------------------");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: FeValidate [-o<filePath> | -a<filePath>] dbname project  pdfName");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        project          : user's project 'charge' code.");
            Console.Write("\r\n        pdfName          : the XXX in user table fe_XXX_site etc.");
            Console.Write("\r\n");
            Console.Write("\r\n        --- Options -------------------------------------------------------------");
            Console.Write("\r\n");
            Console.Write("\r\n        -o<filePath>    : redirect console output to write to a file.");
            Console.Write("\r\n                        : if the output file already exist then it is overwritten.");
            Console.Write("\r\n                        : filePath should be the fully-qualified path to the output file.");
            Console.Write("\r\n        -a<filePath>    : same as the -o option except that the output file is appended to.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n        FeValidate fcsa hulme1_0 -od:\\users\\ahulme\\results.txt tetestpdf");
            Console.Write("\r\n");
            Console.Write("\r\n NOTES:");
            Console.Write("\r\n       1. Options can appear in any order and at any position in the argument list.");
            Console.Write("\r\n       2. The -o and -a options are mutually exclusive.");
            Console.Write("\r\n\r\n BUILD: " + Info.CsharpBuildInfo);
        }

        /// <summary>
        /// This method parses the command line arguments and uses their values to set
        /// various associated internal variables and flags; a list of all the PDF names
        /// is provided as output.
        /// </summary>
        /// <param name="args"> - command line arguments.</param>
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
                    Console.Write("\r\n ERROR: Invalid flag: '{0}'", arg);
                    WriteUsageToConsole();
                    Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
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
                else
                {
                    Console.Write("\r\n ERROR: Invalid flag: '{0}'", arg);
                    WriteUsageToConsole();
                    Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
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
        }


        /// <summary>
        /// This methods gets the values of Windows environment variables that
        /// are required by FeValidate; specifically these are 'MICSUSER'
        /// and 'PASSWORD' that are needed by UtConnect to start an ODBC 'session'.
        /// </summary>
        public static void GetEnvVariables()
        {
            // Get the user's MICS ID from the environment.
            Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                //...Log2.e("\nFeValidate.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
                Application.Exit(Error.ENVVARMICSUSERNOTSET);
            }

            // Get the user's password from the environment.
            Info.Password = Environment.GetEnvironmentVariable("PASSWORD");         // REQUIRED (but can be anything).
            if (String.IsNullOrWhiteSpace(Info.Password))
            {
                // Password just has to be set to something; its value is never used.
                Info.Password = "Bananarama";
            }
        }

        /// <summary>
        /// This method updates the 'validation' field of a PDF's title record i.a.w. the
        /// outcome of the validation processing: if there are any validation errors then
        /// the field is set to "N"; if the validation succeeded (no errors) then the
        /// field is set to "U".
        /// </summary
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="valLevel"> - prescribed validation level (either Constant.UPDATE or Constant.TSIP</param>
        /// <param name="errTotal"> - on input, this must be set to the current total number of validation errors; updated on return.</param>
        public static void UpdateTitleRecord(string pdfName, int valLevel, ref short errTotal)
        {
            // Update the title record with the validated flag.
            FeTitl feTitl;
            SQLLEN[] nArrayFW;
            string fileName;
            int tID;
            short rc;

            GenUtil.UtCvtName(Constant.FE_TITL, pdfName, out fileName);

            if ((tID = DynFeTitl.FeSelectTitl(fileName, "", "")) < 0)
            {
                Console.Write("ERROR - Could not read title record.\r\n");
                errTotal++;
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }
            else
            {
                rc = (short)DynFeTitl.FeFetchTitl(tID, out feTitl, out nArrayFW);

                if (rc == Constant.SUCCESS)
                {
                    //  title record was read, can update 
                    if (errTotal != 0)
                    {
                        feTitl.validated = Constant.NOT_VALIDATED.ToString();
                    }
                    else
                    {
                        if (valLevel == Constant.TSIP)
                        {
                            feTitl.validated = Constant.TSIP_VALIDATED.ToString();
                        }
                        else
                        {
                            feTitl.validated = "U";
                        }
                    }

                    nArrayFW[FeTitl.VALIDATED] = Constant.DB_NOT_NULL;

                    DynFeTitl.FeUpdateTitl(tID, feTitl, nArrayFW);

                    DynFeTitl.FeCloseTitl(tID);

                    // Update central table record.
                    UserInfo.UtUpdateCentralTable("U", pdfName, Constant.FE, feTitl.validated, "N");

                }
                else
                {
                    Console.Write("ERROR - No title record exists for this PDF.\r\n");
                    (errTotal)++;
                }
            }
        }




    }
}

```
