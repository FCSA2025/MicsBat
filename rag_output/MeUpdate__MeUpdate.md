# Documented File: MeUpdate.cs
**Repository Path:** `MeUpdate\MeUpdate.cs`
**Primary Layer:** `MeUpdate`
**Namespace:** `MeUpdate`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _NewLib;
using _OHloss;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This program inserts new records and updates or deletes existing records
/// in the ES-specific Main Data Base (MDB) tables i.a.w. the directives 
/// contained in successfully validated ES PDF import file table sets.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - MeUpdate.PNG" ""
/// </remarks>
namespace MeUpdate
{
    using SQLLEN = Int64;
    using SQLHDBC = IntPtr;
    using SQLRETURN = Int16;
    using SQLUINTEGER = UInt32;
    using SQLPOINTER = IntPtr;
    using System.Runtime.InteropServices;

    /// <summary>
    /// This class provides the Main() method for the MeUpdate application that
    /// inserts new records and updates or deletes existing records
    /// in the ES-specific Main Data Base (MDB) tables i.a.w. the directives 
    /// contained in successfully validated ES PDF import file table sets.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class MeUpdate
    {
        private static bool mIsCreateSpoofTables = false;
        private static bool mIsAbortIfAlreadyPosted = true;
        private static bool mIsWriteSpoofTestData = false;

        /// <summary>
        /// This is the Main() method for the MICS program MeUpdate and
        /// provides high-level functionality and control.
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\MeUpdate.log";
                if (Log2.SetLogFilePath(mLog2FilePath))
                {
                    Log2.Erase();
                    Log2.Set(Log2.FileOpenClose.PER_SESSION);
                    Log2.Set(Log2.WriteMode.ENABLED);
                    Log2.Set(Log2.Level.VERBOSE);
                    Info.BuildMetaData = Info.CollateExeMetaData();
                    //...Log2.v("\nBuild: " + Info.BuildMetaData);
                }
                else
                {
                    Console.Error.Write("\r\nERROR: could not open Log2 file: " + mLog2FilePath);
                }
#endif
                int rc = 0;
                string validatedFor;

                int[] addCount;     /* total recs added , site, ante, azim, chan */
                int[] delCount;     /* total recs deleted, site, ante, azim,chan */
                int[] updCount;     /* total recs updated, site, ante, azim,chan */
                int[] totCount;     /* total records procd, site, ante, azim,chan */
                int[] cCalCount;    /* total record change call sign,site,an,az,ch*/
                int[] cLocCount;	/* total record change location, site,an,az,ch*/

                // Enable attempts to insert/update/delete MDB table records.
                // Disable 'spoof' mode.
                // These can be negated for test purposes using command line flags.
                Info.MdbWriteEnabled = true;

                // Disable the 'spoofing' mode.
                // The spoofing mode replaces the MDB schema 'main' with the schema of the
                // user that executed this program (e.g. 'hulme'). Consequently, hulme.me_chan
                // acts as a surrogate for main.me_chan etc.
                // Spoofing mode can be turned on for test purposes using a command line flag.
                Info.SpoofModeIsOff = true;

                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                GetEnvVariablesForUtConnect();

                //	Grab write access to the database, if possible.  If not, exit.
                if ((rc = Qutils.EnterQueue(Info.DbName, "WRITE", 30)) != Constant.SUCCESS)
                {
                    Log2.e("\nMeUpdate.Main(): ERROR: call to Qutils.EnterQueue() failed, rc = " + rc);
                    Qutils.ExplainQueue(Info.DbName, "WRITE", rc, null);
                    Application.ExitQuietly(100);
                }

                // Establish an FCSA user session with the database.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    Console.Write("\r\nMeUpdate: Could not connect to database {0}\r\n{1}\r\n",
                                        Info.DbName, GenUtil.GetUserMess());

                    Application.ExitQuietly(101);
                }

                // Create new spoof tables and populate with test data, as required.
                if (mIsCreateSpoofTables)
                {
                    Spoofing.CreateSpoofMdbTables(Info.GlobalSchema, mIsWriteSpoofTestData);
                }

                // Initialize the billing.
                BiUtil.BiBillingRec("MEUPDATE", Info.PdfName);

                //...Log2.v("\nMeUpdate.Main(): Info:\n" + Info.ToString());

                // Verify that the ES pdf exists; if not, clean up then exit.
                if (!Ssutil.UtTableExist(Constant.FE, Info.PdfName))
                {
                    Console.Write("\r\nERROR! \r\nPDF '{0}' does not exist\r\n\r\n", Info.PdfName);
                    goto cleanUpAndExit;
                }

                // If file has already updated the mdb, dont update.
                if ((rc = FilewUtil.UtFilewValidated(Constant.FE, Info.PdfName, out validatedFor)) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(Error.FILEWVAL);
                    Console.Write("\r\n");
                    goto cleanUpAndExit;
                }

                // Check if the PDF's ES _title record has its 'validated' column set to 
                // 'P' for posted. Abort processing or ignore depending on command line option.
                if (validatedFor[0].Equals(Constant.UPDATE_POSTED))
                {
                    if (mIsAbortIfAlreadyPosted)
                    {
                        ErrMsg.UtPrintMessage(Error.ALREADYPOSTED, "ES", Info.PdfName, "Update");
                        Console.Write("\r\n");
                        goto cleanUpAndExit;
                    }
                    else
                    {
                        // Cheat.
                        validatedFor = "U";
                    }
                }

                // We only update the MDB tables if the PDF's ES _title record has its 
                // 'validated' column set to either 'M' or 'U'.
                if (!validatedFor[0].Equals(Constant.UPDATE_VALIDATED) &&
                    !validatedFor[0].Equals(Constant.M_UPDATE_VALIDATED))
                {
                    //	Not validated for update.
                    Console.Write("\r\n*ERROR* - Filew has not been validated for update.\r\n");
                    goto cleanUpAndExit;
                }

                // Initialize the counters.
                const int NDIM = 4;
                addCount = Arrays.CreateAndFillArray(NDIM, 0);    /* total recs added , site, ante, azim, chan */
                delCount = Arrays.CreateAndFillArray(NDIM, 0);    /* total recs deleted, site, ante, azim,chan */
                updCount = Arrays.CreateAndFillArray(NDIM, 0);    /* total recs updated, site, ante, azim,chan */
                totCount = Arrays.CreateAndFillArray(NDIM, 0);    /* total records procd, site, ante, azim,chan */
                cCalCount = Arrays.CreateAndFillArray(NDIM, 0);   /* total record change call sign,site,an,az,ch*/
                cLocCount = Arrays.CreateAndFillArray(NDIM, 0);   /* total record change location, site,an,az,ch*/

                // Set the operating state of the classes that provide high-level read/write
                // methods that access ES MDB tables.
                DynMeAnte.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMeAzim.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMeChan.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMeSite.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);

                // Set the operating state of the classes that provide high-level read/write
                // methods that access the DB adm.audit_table table.
                DynAuditTrail.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);

                // ============================================================
                // Call the update controller method.
                // This method provides the core functionality for MeUpdate.
                // ============================================================
                rc = MeDoEsUpdate(Info.PdfName, ref addCount, ref delCount, ref updCount,
                                    ref totCount, ref cCalCount, ref cLocCount);

                // Count the total number of ES MDB table records that were changed.
                int totalProcessed = 0;

                totalProcessed += SumArrayElements(totCount);
                totalProcessed += SumArrayElements(cCalCount);
                totalProcessed += SumArrayElements(cLocCount);

                // Handle errors and/or no records changed scenario.
                if (rc != Constant.SUCCESS || totalProcessed == 0)
                {
                    Console.Write("\r\n");
                    Console.Write("\r\nWARNING!");
                    Console.Write("\r\nNo records were Updated, Deleted, or Added");
                    Console.Write("\r\n\r\n");
                    goto cleanUpAndExit;
                }

                // Some records were changed: write change summary to the Console.
                if (Info.MdbWriteEnabled)
                {
                    /* Print summary of changes */
                    PrintSummaryOfChanges(addCount, delCount, updCount, totCount, cCalCount, cLocCount);

                    // Update the validate flag to 'P'osted
                    SetValidationToPosted();
                }
                // If writing to the MDB has been prohibited by the user then write to Console all
                // of the SQL queries that were inhibited.
                else
                {
                    Console.Write("\r\n");
                    Console.Write("\r\n[-d] command-line option: INHIBITED SQL QUERIES:");
                    Console.Write("\r\n================================================");
                    Console.Write("\r\n");
                    Console.Write(Info.Text);
                    Console.Write("\r\n");
                }

                // Although using a 'goto to label' construct is somewhat taboo
                // it has been adopted in this Main() program to avoid having multiple nested
                // if-else constructs that would make it significantly more difficult 
                // to comprehend the flow of control.
                cleanUpAndExit:

                // Release the hold on the database.
                Qutils.ExitQueue(Info.DbName, "WRITE");

                // Write the billing information to the DB.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(rc);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nMeUpdate.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nMeUpdate.Main(): stack trace: \r\n\r\n" + e.StackTrace);
                BiUtil.BiBillingRec(Constant.BI_END, "");
                Ssutil.UtDisconnect(1);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

        }

        /// <summary>
        /// This methods gets the values of Windows environment variables that
        /// are required to execute GetCoords; specifically these are 'MICSUSER'
        /// and 'PASSWORD' that are required for a successful call to UtConnect().
        /// </summary>
        public static void GetEnvVariablesForUtConnect()
        {
            // Get the user's MICS ID from the environment.
            Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                Log2.e("\r\nFeImport.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
                Application.ExitQuietly(Error.ENVVARMICSUSERNOTSET);
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
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n MeUpdate updates the ES 'main' tables with records from a ");
            Console.Write("\r\n previously imported and validated ES PDF.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: MeUpdate  <dbName> <projectCode> <pdfName> [-c] [-d] [-p] [-s] [-t]");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        projectCode      : user's project billing code.");
            Console.Write("\r\n        pdfName          : ES PDF table set root name (the XXX in fe_XXX_site, etc).");
            Console.Write("\r\n");
            Console.Write("\r\n        --- For Testing -------------------------------------------------------------");
            Console.Write("\r\n        -c               : create empty ES spoof tables in the user's schema.");
            Console.Write("\r\n        -d               : disable attempts to insert/update/delete MDB table records.");
            Console.Write("\r\n        -p               : process update even when ES _titl validated is 'P'osted.");
            Console.Write("\r\n        -s               : enable spoof mode - schema 'main' is replaced by user's schema.");
            Console.Write("\r\n        -t               : create empty ES spoof tables and populate with simple test data.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     MeUpdate  fcsa  HULME1_0  esupdates  ");
            Console.Write("\r\n     ");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the user-prescribed command-line arguments.
        /// </summary>
        /// <param name="args"></param>
        public static void ParseCommandLineArgs(string[] args)
        {
            if (args.Length == 0)
            {
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Create separate lists of 'flag' args prefixed with '-' and those that
            // are not.
            List<string> flagArgs = new List<string>();
            List<string> regularArgs = new List<string>();

            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
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
                // Check that we don't just have a minus character.
                if (arg.Length == 1)
                {
                    Console.Write("\r\n Invalid flag: '{0}'", arg);
                    WriteUsageToConsole();
                    Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                }

                // Process the flags.
                string flag = Strings.DropFirstChar(arg).ToUpper();
                switch (flag)
                {
                    case "D":
                        Info.MdbWriteEnabled = false;
                        break;
                    case "S":
                        Info.SpoofModeIsOff = false;
                        break;
                    case "C":
                        mIsCreateSpoofTables = true;
                        break;
                    case "P":
                        mIsAbortIfAlreadyPosted = false;
                        break;
                    case "T":
                        mIsCreateSpoofTables = true;
                        mIsWriteSpoofTestData = true;
                        break;
                    default:
                        Console.Write("\r\n Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                        break;
                }

            }

            // Parse the regular arguments; there must be qty. 4 of them.
            if (regularArgs.Count != 3)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Constant.FAILURE);
            }

            // Parse the args.
            Info.DbName = regularArgs[0];
            Info.ProjectCode = regularArgs[1];
            Info.PdfName = regularArgs[2];
        }

        /// <summary>
        /// This method provides the core functionality of the MeUpdate application
        /// </summary>
        /// <param name="pdfName"></param>
        /// <param name="addCount"> - number of records added, site, antenna, channel.</param>
        /// <param name="delCount"> - number of records deleted, site, antenna, channel.</param>
        /// <param name="updCount"> - number of records updated, site, antenna, channel.</param>
        /// <param name="totCount"> - number of records processed, site, antenna, channel.</param>
        /// <param name="cCalCount"> - number of records affected by Change Call (GK).</param>
        /// <param name="cLocCount"> - number of records affected by Change Location (LK).</param>
        /// <returns></returns>
        private static int MeDoEsUpdate(string pdfName,
                                        ref int[] addCount,     /* number of records added, site, antenna, channel */
                                        ref int[] delCount,     /* number of records deleted, site, antenna, channel */
                                        ref int[] updCount,     /* number of records updated, site, antenna, channel */
                                        ref int[] totCount,     /* number of records processed, site, antenna, channel*/
                                        ref int[] cCalCount,    /* number of records affected by Change Call */
                                        ref int[] cLocCount 	/* number of records affected by Change Loc */
)
        {
            FeTitl feTitl;    /* storage for titl record */
            SQLLEN[] nullInds; //[FE_TITL_SIZE_]; /* Ingres Null Array */

            /* Local variables */
            UserInfoData userInfo;  /* storage for userInfo */
            int titlHandle;         /* storage for title handle */
            int status = Constant.SUCCESS;        /* transaction status */
            string curDate;     /* current date */
            string curTime;     /* current time */
            string fullFeTitlTableName;    /* pdf name - long format */
            int rc;             /* return code */

            SQLHDBC hConn;

            /* Get the user's username for report output */
            if ((rc = UserInfo.UtGetUserInfo(out userInfo)) != Constant.SUCCESS)
            {
                ErrMsg.UtPrintMessage(rc);
                return (Constant.FAILURE);
            }

            /* Get current time/date for report header */
            GenUtil.UtGetDateTime(out curDate, out curTime);

            /* Print header for ES Update report */
            Console.Write("Date: {0}            Build: {1}            Time: {2}\r\n",
                                curDate, Info.BuildMetaData, curTime);
            Console.Write("          ****************** Master DataBase Update ******************\r\n");
            Console.Write("          ***  PDF Name: {0,-16}      User Name: {1,-7} ****\r\n",
                                    pdfName, userInfo.micsUser.micsid);

            /* Convert ES PDF name from short to long format */
            GenUtil.UtCvtName(Constant.FE_TITL, pdfName, out fullFeTitlTableName);

            /* Get a handle on the ES PDF data - to access Title record */
            if ((titlHandle = DynFeTitl.FeSelectTitl(fullFeTitlTableName, "", "")) < 0)
            {
                Console.Write("\r\nERROR! \r\nCould not access pdf title record\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                Console.Write("\r\n");
                return (Constant.FAILURE);
            }

            /* Check PDF for validation mark */
            if ((DynFeTitl.FeFetchTitl(titlHandle, out feTitl, out nullInds)) != Constant.SUCCESS)
            {
                Console.Write("\r\nERROR! \r\nCould not read pdf title record.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                Console.Write("\r\n");
                return (Constant.FAILURE);
            }
            else
            {
                DynFeTitl.FeCloseTitl(titlHandle);
            }

            // Begin transaction for updates */
            hConn = Ssutil.NewConn();

            // Change to Manual Commit mode.  This will commit everything up to here
            // and not commit anything until we explicitly commit it or roll it back.
            rc = Ssutil.SetCommitMode(hConn, Enums.COMMIT.MANUAL);

            //sqlRet = ODBC.SQLSetConnectAttr(hConn, ODBC.SQL_ATTR_AUTOCOMMIT, (SQLPOINTER)ODBC.SQL_AUTOCOMMIT_OFF, ODBC.SQL_IS_UINTEGER);

            if (rc != Constant.SUCCESS)
            {
                //	Could not set SQL commit mode to manual.
                Log2.e("\nMeUpdate.MeDoEsUpdate(): ERROR: could not set the database connection to manual commit.");
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_DBC, hConn);
                Console.Write("\r\nMeDoTsUpdate: Could not set the database connection to manual commit.\r\n{0}\r\n",
                                GenUtil.GetUserMess());
                return Constant.FAILURE;
            }

            /* Handle channel records from PDF */
            rc = Mdb.UpdateMeChanTable(hConn, pdfName, userInfo.micsUser.micsid, ref addCount[Constant.CHANINDEX],
                                              ref delCount[Constant.CHANINDEX], ref updCount[Constant.CHANINDEX], ref totCount[Constant.CHANINDEX]);
            if (rc != Constant.SUCCESS)
            {
                Log2.e("\nMeUpdate.MeDoEsUpdate(): ERROR: call to UpdateMeChanTable() failed, rc = " + rc);
                status = Constant.FAILURE;
            }

            /* Handle antenna records from PDF */
            rc = Mdb.UpdateMeAnteTable(hConn, pdfName, userInfo.micsUser.micsid, ref addCount[Constant.ANTEINDEX],
                                              ref delCount[Constant.ANTEINDEX], ref updCount[Constant.ANTEINDEX], ref totCount[Constant.ANTEINDEX]);
            if (rc != Constant.SUCCESS)
            {
                Log2.e("\nMeUpdate.MeDoEsUpdate(): ERROR: call to UpdateMeAnteTable() failed, rc = " + rc);
                status = Constant.FAILURE;
            }

            /* Handle azimuth records from PDF */
            rc = Mdb.UpdateMeAzimTable(hConn, pdfName, userInfo.micsUser.micsid, ref addCount[Constant.AZIMINDEX], ref delCount[Constant.AZIMINDEX],
                                ref updCount[Constant.AZIMINDEX], ref totCount[Constant.AZIMINDEX]);
            if (rc != Constant.SUCCESS)
            {
                Log2.e("\nMeUpdate.MeDoEsUpdate(): ERROR: call to UpdateMeAzimTable() failed, rc = " + rc);
                status = Constant.FAILURE;
            }

            /* Handle site records from PDF */
            rc = Mdb.UpdateMeSiteTable(hConn, pdfName, userInfo.micsUser.micsid, ref addCount[Constant.SITEINDEX], ref delCount[Constant.SITEINDEX],
                                              ref updCount[Constant.SITEINDEX], ref totCount[Constant.SITEINDEX]);
            if (rc != Constant.SUCCESS)
            {
                Log2.e("\nMeUpdate.MeDoEsUpdate(): ERROR: call to UpdateMeSiteTable() failed, rc = " + rc);
                status = Constant.FAILURE;
            }

            /* Handle change of location records */
            rc = Mdb.UpdateMeCLoc(hConn, pdfName, ref cLocCount, userInfo.micsUser.micsid);
            if (rc != Constant.SUCCESS)
            {
                Log2.e("\nMeUpdate.MeDoEsUpdate(): ERROR: call to UpdateMeCLocTable() failed, rc = " + rc);
                status = Constant.FAILURE;
            }

            /* Handle change of callsign records */
            rc = Mdb.UpdateMeCCal(hConn, pdfName, ref cCalCount, userInfo.micsUser.micsid);
            if (rc != Constant.SUCCESS)
            {
                Log2.e("\nMeUpdate.MeDoEsUpdate(): ERROR: call to UpdateMeCCalTable() failed, rc = " + rc);
                status = Constant.FAILURE;
            }


            /* Decide to commit or rollback, then return */
            if (status == Constant.SUCCESS)
            {
                Ssutil.DbCommit(hConn);

                Ssutil.DisConn(hConn);

                status = Constant.SUCCESS;
            }
            else
            {
                Ssutil.DbRollBack(hConn);

                Ssutil.DisConn(hConn);

                status = Constant.FAILURE;
            }

            // Restore Commit mode to AUTO.
            Ssutil.SetCommitMode(hConn, Enums.COMMIT.AUTO);

            return status;
        }

        /// <summary>
        /// This method writes to the Console a summary of the number of ES  MDB table
        /// records that were added, deleted, modified and/or updated.
        /// </summary>
        /// <param name="addCount"> - number of records added, site, antenna, channel.</param>
        /// <param name="delCount"> - number of records deleted, site, antenna, channel.</param>
        /// <param name="updCount"> - number of records updated, site, antenna, channel.</param>
        /// <param name="totCount"> - number of records processed, site, antenna, channel.</param>
        /// <param name="cCalCount"> - number of records affected by Change Call (GK).</param>
        /// <param name="cLocCount"> - number of records affected by Change Location (LK).</param>
        private static void PrintSummaryOfChanges(int[] addCount,
                                                    int[] delCount,
                                                    int[] updCount,
                                                    int[] totCount,
                                                    int[] cCalCount,
                                                    int[] cLocCount)
        {
            /* Print summary of changes */
            Console.Write("\r\n");
            Console.Write("\tSite records added       : {0}\r\n", addCount[Constant.SITEINDEX]);
            Console.Write("\tSite records updated     : {0}\r\n", updCount[Constant.SITEINDEX]);
            Console.Write("\tSite records deleted     : {0}\r\n", delCount[Constant.SITEINDEX]);
            Console.Write("\tSite records modified by \r\n");
            Console.Write("\t  Change of Location     : {0}\r\n", cLocCount[Constant.SITEINDEX]);
            Console.Write("\tSite records processed   : {0}\r\n", totCount[Constant.SITEINDEX]);

            Console.Write("\r\n");
            Console.Write("\tAntenna records added    : {0}\r\n", addCount[Constant.ANTEINDEX]);
            Console.Write("\tAntenna records updated  : {0}\r\n", updCount[Constant.ANTEINDEX]);
            Console.Write("\tAntenna records deleted  : {0}\r\n", delCount[Constant.ANTEINDEX]);
            Console.Write("\tAntenna records modified by \r\n");
            Console.Write("\t  Change of Location     : {0}\r\n", cLocCount[Constant.ANTEINDEX]);
            Console.Write("\tAntenna records modified by \r\n");
            Console.Write("\t  Change of Call sign    : {0}\r\n", cCalCount[Constant.ANTEINDEX]);
            Console.Write("\tAntenna records processed: {0}\r\n", totCount[Constant.ANTEINDEX]);

            Console.Write("\r\n");
            Console.Write("\tAzimuth records added    : {0}\r\n", addCount[Constant.AZIMINDEX]);
            Console.Write("\tAzimuth records updated  : {0}\r\n", updCount[Constant.AZIMINDEX]);
            Console.Write("\tAzimuth records deleted  : {0}\r\n", delCount[Constant.AZIMINDEX]);
            Console.Write("\tAzimuth records modified by \r\n");
            Console.Write("\t  Change of Location     : {0}\r\n", cLocCount[Constant.AZIMINDEX]);
            Console.Write("\tAzimuth records modified by \r\n");
            Console.Write("\t  Change of Call sign    : {0}\r\n", cCalCount[Constant.AZIMINDEX]);
            Console.Write("\tAzimuth records processed: {0}\r\n", totCount[Constant.AZIMINDEX]);

            Console.Write("\r\n");
            Console.Write("\tChannel records added    : {0}\r\n", addCount[Constant.CHANINDEX]);
            Console.Write("\tChannel records updated  : {0}\r\n", updCount[Constant.CHANINDEX]);
            Console.Write("\tChannel records deleted  : {0}\r\n", delCount[Constant.CHANINDEX]);
            Console.Write("\tChannel records modified by \r\n");
            Console.Write("\t  Change of Location     : {0}\r\n", cLocCount[Constant.CHANINDEX]);
            Console.Write("\tChannel records modified by \r\n");
            Console.Write("\t  Change of Call sign    : {0}\r\n", cCalCount[Constant.CHANINDEX]);
            Console.Write("\tChannel records processed: {0}\r\n", totCount[Constant.CHANINDEX]);

            Console.Write("\r\n" + (Info.SpoofModeIsOff ? "Master DataBase Update " : "Spoof DataBase Update "));
            Console.Write("successfully completed.\r\n");
        }

        /// <summary>
        /// This methods sets the validation field in the ES PDF's _titl table record and
        /// the web.user_tables record to 'P' for 'Posted'.
        /// </summary>
        private static void SetValidationToPosted()
        {
            // Update the validate flag - P)osted
            if (Info.MdbWriteEnabled)
            {
                int cursorID;
                FeTitl feTitl;
                SQLLEN[] nullInds;
                string fullTitlTableName;
                int rc;

                AtRecordChng.AtRecordChange(Info.DbName, Info.PdfName, Constant.FE);

                GenUtil.UtCvtName(Constant.FE_TITL, Info.PdfName, out fullTitlTableName);

                if ((cursorID = DynFeTitl.FeSelectTitl(fullTitlTableName, "", "")) < 0)
                {
                    Log2.e("\nMeUpdate.Main(): ERROR: call to FeSelectTitl() failed, fullTitlTableName = " + fullTitlTableName);
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }
                else
                {

                    if ((rc = DynFeTitl.FeFetchTitl(cursorID, out feTitl, out nullInds)) == Constant.SUCCESS)
                    {
                        feTitl.validated = Constant.UPDATE_POSTED.ToString();
                        nullInds[FeTitl.VALIDATED] = Constant.DB_NOT_NULL;

                        DynFeTitl.FeUpdateTitl(cursorID, feTitl, nullInds);
                    }
                    else
                    {
                        Log2.e("\nMeUpdate.Main(): ERROR: call to FeFetchTitl() failed, fullTitlTableName = " + fullTitlTableName);
                        ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    }

                    DynFeTitl.FeCloseTitl(cursorID);

                    UserInfo.UtUpdateCentralTable("U", Info.PdfName, Constant.FE, "P", "N");
                }
            }
        }

        /// <summary>
        /// This method returns the summation of all of the elements of
        /// a prescribed integer array.
        /// </summary>
        /// <param name="elements"> - array of integers.</param>
        /// <returns></returns>
        private static int SumArrayElements(int[] elements)
        {
            int count = 0;

            foreach (int element in elements)
            {
                count += element;
            }

            return count;
        }


    }
}



```
