using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Data.Odbc;


/// <summary>
/// This application is run by WebMICS whenever a user requests a TSIP run. 
/// It provides the supervisory framework in which multiple instances
/// of TpRunTsip can execute concurrently while other TSIP jobs wait in a queue. 
/// TsipInitiator sends TSIP output reports to the MICS user as Email attachments.
/// </summary>
/// <remarks>
/// Conceptually:
/// <list type="bullet">
/// <item>Each instance of a running TpRunTsip process occupies one TSIP 'slot';</item>
/// <item>TsipInitiator imposes a limit on the number of available 'slots';</item>
/// <item>As of 30-Jul-2018, the total number of 'slots' is 5;</item>
/// <item>Whenever TsipInitiator is run by WebMics, if a TSIP 'slot' is
/// available, it spawns an independent Windows process that executes
/// an instance of TpRunTsip.exe;</item>
/// <item>If all the TSIP 'slots' are currently occupied the TSIP job waits in a queue.</item>
/// </list>
/// </para>
/// The command-line usage is:
/// \image html "Usage - TsipInitiator.PNG" ""
/// </remarks>
namespace TsipInitiator
{
    using System.Configuration;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides the Main() method for the TsipInitiator application.
    /// </summary>
    class TsipInitiator
    {
        /// <summary>
        /// This Main() method starts and supervises a queued TSIP run.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
#if true
            // Turn on developmental logging.
            string mLog2FilePath = @"d:\MicsBatchLogs\TsipInitiator.log";
            if (Log2.SetLogFilePath(mLog2FilePath))
            {
                Log2.Erase();
                Log2.Set(Log2.FileOpenClose.PER_SESSION);
                Log2.Set(Log2.WriteMode.ENABLED);
                Log2.Set(Log2.Level.VERBOSE);

                Log2.i("\n Build {0}\n" + Info.BuildMetaData);
            }
            else
            {
                Console.Error.Write("\nERROR: could not open Log2 file: " + mLog2FilePath);
            }
#endif

            string dbName = "";         // FCSA DB server name
            string projectCode = "";    // FCSA project account code
            string paramFileName = "";  // PDF file name	
            string userid = "";         // Unix userid for owner of tsip						
            string destPath;            // pathname of destination path for tsip							

            UserInfoData userInfo;

            TextWriter twOldStdOut;
            TextWriter twOldStdErr;
            TextWriter twConsoleFile = null; ;

            string str;
            string reportPrefix = "";   // tpRunTsip destination argument			
            string curDate;             // Current date argument for update programs 
            string curTime;             // Current time argument for update programs 
            string submail;             // email subject line							
            string cEmail;              //	Email address and its length 

            int nMaxLen = 129;
            int nRet = 0;
            string cDelFlag;

            string cTsipJob = "TSIPJOB";
            int nJob;
            string cEventName;
            int nTime;

            string consoleFileName;
            string cFileRoot;
            string tsip_email;
            bool IsDeleted = false;
            const int userSessionID = 1;
            int tpRunTsipExitCode = -666;
            int sendEmailRetVal = Constant.SUCCESS;

            try
            {
                // Parse the command-line arguments.
                // Later, the same command-line arguments are used to call TpRunTsip.exe
                // Note that any -P flag/arg is discarded on return from this method.
                ParseCommandLineArgs(ref args, out dbName, out projectCode, out paramFileName, out reportPrefix);

                // Get the current date and time.
                GenUtil.UtGetDateTime(out curDate, out curTime);

                // Check that we have write access to the Tsip Log File.
                nRet = TsipQ.InitTsipLogFileAccess(dbName);
                if (nRet != Constant.SUCCESS)
                {
                    Console.Error.Write("\n\nTsipInitiator: Main(): ERROR: call to TsipQ.InitTsipLogFileAccess() failed.");
                    Log2.e("\nTsipInitiator.Main(): call to TsipQ.InitTsipLogFileAccess() failed.");
                }

                // Write a message to the Tsip Log.
                str = String.Format("\n\n{0} {1}: Queueing tsip {2} {3} {4} {5}",
                                    curDate, curTime, dbName, projectCode, reportPrefix, paramFileName);
                TsipQ.WriteToTsipLog(str);

                //...Log2.v(str);

                // The static Info class is used to pass values to Ssutil.UtConnect();
                // Set the required Info fields.
                Info.DbName = dbName;
                Info.ProjectCode = projectCode;

                Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
                if (String.IsNullOrWhiteSpace(Info.MicsUserName))
                {
                    string msg = @"\nTsipInitiator.Main(): ERROR: Windows environment variable MicsUser is not set.";
                    Console.Error.Write("\n" + msg);
                    Log2.e(msg);
                    TsipQ.WriteToTsipLog(msg);
                    Application.Exit(99);
                }

                Info.Password = Environment.GetEnvironmentVariable("PASSWORD");         // REQUIRED (but can be anything).
                if (String.IsNullOrWhiteSpace(Info.Password))
                {
                    // Password just has to be set to something; its value is never used.
                    Info.Password = "Bananarama";
                }

                // Start the user's session.
                // Get and translate queue and job number.
                nRet = Ssutil.UtConnect(dbName, userSessionID);
                if (nRet != Constant.SUCCESS)
                {
                    Console.Error.Write("\n\nTsipInitiator: Main(): ERROR: call to Ssutil.UtConnect() failed, nRet = " + nRet);
                    str = String.Format("Can't connect to database {0} ({1})\n", dbName, nRet);
                    TsipQ.WriteToTsipLog("\n" + str);
                    Log2.e("\nTsipInitiator.Main(): ERROR: " + str);
                    Application.Exit(125);
                }

                // Name the gate used for serializing this table.
                TsipQ.SetGateName(dbName);

                cTsipJob += dbName.ToUpper();

                nJob = Ssutil.GetNextNum(cTsipJob);

                //	Make the name of the console file.
                cFileRoot = String.Format("{0}_{1}", reportPrefix, paramFileName);
                consoleFileName = String.Format("{0}.CONSOLE", cFileRoot);

                if (UserInfo.UtGetUserInfo(out userInfo) != Constant.SUCCESS)
                {
                    str = @"ERROR: call to UserInfo.UtGetUserInfo() failed.";
                    Log2.e("\nTsipInitiator.Main(): " + str);
                    Console.Error.Write("\n\nTsipInitiator.Main(): " + str);
                    TsipQ.WriteToTsipLog("\n" + str);
                    Application.Exit(117);
                }

                // Get the destination path (destPath) from the Windows environment variable WORK_DIR.
                //
                // If WORK_DIR is not set then the method GetUserRoot() is called and sets
                // destPath to something like "D:\inetpub\wwwroot\mics\userdirs\hulme\hulme1".
                // The "wwwroot" is applicable to the case of an actual FCSA customer logged into
                // WebMICS via the external internet.
                //
                // The subsequent call to TsipQ.StartTsip(), below, sets an environment variable 
                // TARGETDIRFORTSIPREPORTS to the value of destPath which is how the destination 
                // directory is passed to TpRunTsip. 

                Info.WorkDir = Environment.GetEnvironmentVariable("WORK_DIR");

                if (String.IsNullOrWhiteSpace(Info.WorkDir))
                {
                    // WORK_DIR not set in the environment; use the following instead.
                    destPath = Ssutil.GetUserRoot(dbName);

                    TsipQ.WriteToTsipLog(String.Format("\n*WARNING* Can't get the working directory from the environment.\nUsing: {0}.\n", destPath));
                    //...Log2.v("\n\nTsipInitiator.Main(): environment variable 'WORD_DIR' is not set.");
                }
                else
                {
                    //...Log2.v("\nTsipInitiator.Main(): environment variable WORK_DIR = " + cDestVar);

                    destPath = Info.WorkDir;
                }

                // The eventname is the name for the event we will wait on if we have to queue.
                cEventName = String.Format("Global\\TSIPJOB{0}{1}", dbName, nJob);

                // Insert into the queue as a wait.
                nRet = TsipQ.InsertTsipQ(nJob, dbName, projectCode, reportPrefix, paramFileName, cEventName, userInfo.micsUser.micsid);
                if (nRet != Constant.SUCCESS)
                {
                    if (nRet == Error.ALREADY_IN_QUEUE)
                    {
                        //Duplicate in queue.  return 2
                        str = "ERROR: call to TsipQ.InsertTsipQ() failed: this job is already in the queue.";
                        Log2.e("\nTsipInitiator.Main(): " + str);
                        Console.Error.Write("\n\nTsipInitiator.Main(): " + str);
                        TsipQ.WriteToTsipLog("\n" + str);
                        Application.Exit(2);
                    }
                    else
                    {
                        string userMess = (nRet == Constant.FAILURE) ? GenUtil.GetUserMess() : "-";
                        str = String.Format("ODBC error ({0}) inserting {1} -o{2} {3}\n\t{4}\n",
                                                nRet, projectCode, reportPrefix, paramFileName, userMess);
                        Log2.e("\nTsipInitiator.Main(): ERROR: call to TsipQ.InsertTsipQ() failed: " + str);
                        TsipQ.WriteToTsipLog("\n" + str);
                        Application.Exit(115); // Error inserting into tsip queue.
                    }
                }

                //...Log2.v("\nTsipInitiator.Main(): successful call to TsipQ.InsertTsipQ().");

                // Check that the destination path is accessible.
                if (!Directory.Exists(destPath))
                {
                    submail = String.Format("Can't run TSIP job {0}. Cannot reach path {1}", paramFileName, destPath);
                    Log2.e("\nTsipInitiator.Main(): ERROR: " + submail);
                    TsipQ.WriteToTsipLog(String.Format("\n{0}\n", submail));
                    Application.Exit(121);
                }

                // Check that we can create, and write to, a file in the destination directory.
                string testFilePath = Path.Combine(destPath, "JuNkTeSt.txt");

                //...Log2.v("\nTsipInitiator.Main(): testFilePath = " + testFilePath);

                try
                {
                    TextWriter twTest = new StreamWriter(testFilePath);
                    twTest.Write("Hello");
                    twTest.Close();
                    File.Delete(testFilePath);
                }
                catch
                {
                    // We cannot write to the destination directory,
                    // so we cannot run TSIP.
                    submail = String.Format("Can't run TSIP job {0}. Cannot write to directory {1}",
                                                    paramFileName, destPath);
                    Log2.e("\nTsipInitiator.Main(): ERROR: " + submail);
                    TsipQ.WriteToTsipLog(String.Format("\n{0}\n", submail));
                    Console.Error.Write("\nTsipInitiator.Main(): ERROR: " + submail);
                    Application.Exit(120);
                }

                //...Log2.v("\nTsipInitiator.Main(): successful file create/write test for: " + testFilePath);

                nRet = 0;
                nTime = 0;
                while (nRet == 0)
                {

                    //	Start trying to run the tsip.
                    int nQRet;
                    int tsipProcessID = 0;

                    nTime++;

                    //	First check to see if the hold/write queue is set.
                    if ((nQRet = Qutils.EnterQueue(dbName, "READ", 30)) != Constant.SUCCESS)
                    {
                        if (nTime == 1)
                        {
                            // Explain any error message.
                            Qutils.ExplainQueue(dbName, "READ", nQRet, null);
                        }

                        //...Log2.v("\nTsipInitiator.Main(): could not enter queue: try again in 30 seconds.");
                        Console.Error.Write("\n\nTsipInitiator.Main(): Qutils.EnterQueue(): could not enter queue: will try again in 30 seconds.");

                        Thread.Sleep(30 * 1000); // Wait and try again.
                        continue;
                    }

                    //...Log2.v("\nTsipInitiator.Main(): successfully entered the queue.");

                    // The following call to TestAndFlag() determines whether there is
                    // an available 'slot' for a new TpRunTsip process to run in or
                    // we have to wait until one becomes available.
                    if ((nRet = TsipQ.TestAndFlag(dbName, nJob, "X")) == 0)
                    {
                        //...Log2.v("\nTsipInitiator.Main(): call to TsipQ.TestAndFlag() succeeded.");

                        // The number of jobs running is less than the maximum so
                        // we can now run TpRunTsip.

                        // Open the console file for writing.
                        string consoleFilePath = Path.Combine(destPath, consoleFileName);

                        Log2.v("\nTsipInitiator.Main(): consoleFilePath = " + consoleFilePath);

                        try
                        {
                            twConsoleFile = new StreamWriter(consoleFilePath);
                        }
                        catch
                        {
                            Log2.e("\nTsipInitiator.Main(): ERROR: could not open console file: " + consoleFilePath);
                            TsipQ.WriteToTsipLog(String.Format("\nCould not open {0} as a console.  Exiting.\n", consoleFilePath));
                            Console.Error.Write("\n\nTsipInitiator.Main(): ERROR: could not open console file: " + consoleFilePath);
                            Qutils.ExitQueue(dbName, "READ");
                            Application.Exit(119);
                        }

                        // Redirect stdout and stderr to the console file.
                        twOldStdOut = Console.Out;
                        twOldStdErr = Console.Error;
                        Console.SetOut(twConsoleFile);
                        Console.SetError(twConsoleFile);

                        // Declare a new process to execute TpRunTsip.
                        Process tsipProcess;

                        // Instantiate tsipProcess: start running TpRunTsip as an independent process.
                        tsipProcessID = TsipQ.StartTsip(dbName, args, destPath, nJob, out tsipProcess);

                        if (tsipProcessID > 0)
                        {
                            //	If the TpRunTsip process started correctly then wait here for it to finish.
                            Log2.v("\n\nTsipInitiator.Main(): call to TsipQ.StartTsip() was successful.");
                            tsipProcess.WaitForExit();

                            tpRunTsipExitCode = nRet = tsipProcess.ExitCode;

                            Console.Write("\n\nTpRunTsip.exe:      exit code = {0}", tsipProcess.ExitCode);
                        }
                        else
                        {
                            Log2.e("\nTsipInitiator.Main(): ERROR: call to TsipQ.StartTsip() returned " + tsipProcessID);
                            Console.Error.Write("\n\nTsipInitiator.Main(): ERROR: call to TsipQ.StartTsip() returned " + tsipProcessID);
                            nRet = tsipProcessID;
                        }

                        // Restore stdout and stderr.
                        Console.SetOut(twOldStdOut);
                        Console.SetError(twOldStdErr);

                        Qutils.ExitQueue(dbName, "READ");

                        Console.Write("\n\nTpRunTsip.exe:      exit code = {0}", tsipProcess.ExitCode);

                        // Either TpRunTsip executed and terminated or never ran at all.
                        // We can now exit the while (nRet == 0) {...} loop.
                        break;
                    }

                    else if (nRet == Error.JOB_HAS_NO_ROOM_TO_RUN)
                    {
                        //...Log2.v("\nTsipInitiator.Main(): call to TsipQ.TestAndFlag() returned Error.JOB_HAS_NO_ROOM_TO_RUN.");

                        //	There is no room to run.  We must wait for the event.
                        if (TsipQ.CheckRunningProgs() == Constant.SUCCESS)
                        {
                            // Wait for a tsip processing 'slot' to become available
                            //...Log2.v("\nTsipInitiator.Main(): commencing wait for NamedEvent = " + cEventName);

                            nRet = TsipQ.WaitForEvent(nJob);

                            //...Log2.v("\nTsipInitiator.Main(): slot now available ...");
                        }
                        else
                        {
                            //	Some weren't running, and were reset.  Try again.
                            //...Log2.v("\nTsipInitiator.Main(): some slots weren't running, and were reset. Trying again...");
                            continue;
                        }

                    }
                    else if (nRet == Error.JOB_IN_QUEUE_HAS_BEEN_DELETED)
                    {
                        // The job in the queue has been deleted.
                        str = "ERROR: Job in queue has been deleted.";
                        Log2.e("\nTsipInitiator.Main(): " + str);
                        Console.Error.Write("\n\nTsipInitiator.Main(): " + str);
                        Qutils.ExitQueue(dbName, "READ");
                        TsipQ.WriteToTsipLog("\n" + str);
                        IsDeleted = true;
                        break;
                    }
                    else
                    {
                        str = String.Format("Queueing error: Job {0} - {1}", nJob, nRet);
                        Log2.e("\nTsipInitiator.Main(): ERROR: " + str);
                        Console.Error.Write("\n\nTsipInitiator.Main(): ERROR: " + str);
                        TsipQ.WriteToTsipLog("\n" + str);
                        break;
                    }

                } // while (nRet == 0)


                //	We have finished. Flag it in the database, and start the next one in the queue.
                if (IsDeleted)
                {
                    TsipQ.EndJob(nJob, "D", nRet);
                }
                else
                {
                    TsipQ.EndJob(nJob, "F", nRet);
                }

                // If TpRunTsip succesfully completed then send reports to the
                // WebMICS user as attachments to an email.

                Log2.v("\nGetting user email");
                nRet = Ssutil.EmailAddr(userid, userInfo.micsUser.micsid, out cEmail, nMaxLen, out tsip_email, out cDelFlag);

                Log2.v("\nTsipInitiator.Main(): cEmail     = " + cEmail);
                Log2.v("\nTsipInitiator.Main(): tsip_email = " + tsip_email);
                Log2.v("\nTsipInitiator.Main(): cDelFlag   = " + cDelFlag);

                if (!IsDeleted && (nRet == 0) && (tpRunTsipExitCode == 0))
                {
                    //	We have an email address for this micsid

                    // Does the user want email?
                    bool userWantsEmail = tsip_email.ToUpper().Equals("Y");

                    if (userWantsEmail)
                    {
                        // To get here !IsDeleted would have to be true, i.e. IsDeleted is false.
                        // Thus, the final argument in this call is redundant.
                        Log2.v("\nTsipInitiator.Main(): the user wants email.");

                        sendEmailRetVal = EmailReportsToUser(paramFileName, cDelFlag, dbName, destPath, cFileRoot, cEmail, IsDeleted);
                    }
                    else
                    {
                        // User does not want email.
                        TsipQ.WriteToTsipLog(String.Format("\nNo tsip email requested for {0}\n", userInfo.micsUser.micsid));
                        Log2.v("\nTsipInitiator.Main(): No tsip email desired for " + userInfo.micsUser.micsid);
                    }

                }

                else if (nRet != 0)
                {
                    TsipQ.WriteToTsipLog(String.Format("\nNo email address for {0}\n", userInfo.micsUser.micsid));
                    Log2.e("\nTsipInitiator.Main(): ERROR: No email address for " + userInfo.micsUser.micsid);
                }
                else if (tpRunTsipExitCode != 0)
                {
                    TsipQ.WriteToTsipLog(String.Format("\nERROR: TpRunTsip.exe failed, returned exitCode = {0}", tpRunTsipExitCode));
                    Log2.e("\nTsipInitiator.Main(): ERROR: TpRunTsip.exe failed, returned exitCode = " + tpRunTsipExitCode);
                }

                TsipQ.WriteToTsipLog("\nTsipInitiator.exe:  exit code = " + sendEmailRetVal);
                twConsoleFile.Write("\n\nTsipInitiator.exe:  exit code = {0}", sendEmailRetVal);

                // Close the console file.
                twConsoleFile.Close();

                // Terminate the user's session.
                Ssutil.UtDisconnect(userSessionID);

                // Normal successful exit.
                Application.Exit(sendEmailRetVal);
            }
            catch (Exception e)
            {
                Qutils.ExitQueue(Info.DbName, "READ");

                Log2.e("\n\nTsipInitiator.Main(): exception caught: " + e.Message);
                Log2.e("\n\nTsipInitiator.Main(): stack trace: \n\n" + e.StackTrace);
                TsipQ.WriteToTsipLog("\nTsipInitiator.Main(): exception caught: " + e.Message);
                Application.Exit(Error.FATAL_EXCEPTION);
            }

        }  // Main()

        /// <summary>
        /// This method manages the sending of TSIP output reports to a
        /// MICS user as attachments to an Email.
        /// </summary>
        /// <remarks>
        /// As of MICS# enhancement 180301A the capability to send emails has been encapsulated
        /// within TsipInitiator.exe using the class TsipEmail.cs
        /// 
        /// The following legacy text is no longer relevant but is included here just in case
        /// we have to resume using the stand-alone MICS# program tsipemail.exe some time in the
        /// future.
        /// 
        /// --------------------- SUPERCEDED -----------------------------------------
        /// 
        /// Suppose we run TSIP using a parameter file named 'myparamFile' that 
        /// comprises three distinct 'runs', labelled R01, R02 and R03.
        /// We construct and send the email by callings the MICS console program 
        /// tsipemail.exe using a command line like:
        /// <code>
        ///      tsipemail.exe  D:\Users\ahulme tsip_ myparamFile andrew.hulme@rogers.com N R01 R02 R03
        /// </code>
        /// This is what tsipemail.exe does:
        /// <list>
        /// <item>It searches the directory D:\\Users\\ahulme</item>
        /// <item>Specifically, it searches for any file names that match the following wild-card templates:</item>
        /// <list>
        /// <item>tsip_ myparamFile_R01.*</item>
        /// <item>tsip_ myparamFile_R02.*</item>
        /// <item>tsip_ myparamFile_R03.*</item>
        /// </list>
        /// <item>It constructs and sends an email as follows:</item>
        /// <list>
        /// <item>Recipient is:  andrew.hulme@rogers.com</item>
        /// <item>The body text is the contents of the report file tsip_ myparamFile.ERR</item>
        /// <item>The attachments are all the files that match the wild-card file names 
        /// cited above; if the file names do not end with the extension .txt then 
        /// the tsipemail.exe email program adds that extension.</item>
        /// </list>
        /// </list>
        /// </remarks>
        /// <param name="pdfName"></param>
        /// <param name="delFlag"></param>
        /// <param name="database"></param>
        /// <param name="destPath"></param>
        /// <param name="fileRoot"></param>
        /// <param name="emailAddress"></param>
        /// <param name="isDeleted"></param>
        private static int EmailReportsToUser(string pdfName,
            string delFlag,
            string database,
            string destPath,
            string fileRoot,
            string emailAddress,
            bool isDeleted)
        {
            int retVal = Constant.SUCCESS;

            Log2.v("\n in EmailReportsToUser");

            //	Set up the array of arguments to pass to the tsipemail program.  We fill in the
            //	first with the normal parameters and the runnames follow after.
            List<string> runs = new List<string>();
            int parmHandle;
            TpParm tParm;
            SQLLEN[] aParmNulls;
            string[] acRunNames = new string[Constant.RUNNAME_SZ];
            string cTableName;
            int rc;
            string str;

            delFlag = delFlag.ToUpper();
            Log2.v("\n in EmailReportsToUser-delFlag: " + delFlag);

            // Enhancement 180301A - email send capability absorbed int0 TsipInitiator.exe
            //string cEmailProg = Ssutil.GetBinPath("tsipemail", database);

            //	Now fill in the rest of the array with the runnames from the parameter table.
            GenUtil.UtCvtName(Constant.TP_PARM, pdfName, out cTableName);

            if ((parmHandle = TpDynParm.TpSelectParm(cTableName, "", "runname")) >= 0)
            {
                while ((rc = TpDynParm.TpFetchParm(parmHandle, out tParm, out aParmNulls)) == Constant.SUCCESS)
                {
                    runs.Add(tParm.runname);
                }
            }
            else
            {
                str = String.Format("No records in parm table. (% {0})\n", parmHandle);
                Log2.e("\nTsipInitiator.EmailReportsToUser(): WARNING: " + str);
                TsipQ.WriteToTsipLog("\n" + str);
            }

            string cFromAddr = ConfigurationManager.AppSettings["FromEmailAddress"];

            // The setting of usage-specific TSIP email parameters and the actual sending 
            // via SMTP are encapsulated in the static class TsipEmail. The email parameters
            // are all checked for validity prior to attempting the SQL send.

            TsipEmail.TsipFileFolder = destPath;
            TsipEmail.TsipFileRoot = fileRoot;
            TsipEmail.EmailAddress = emailAddress;
            TsipEmail.DelFlag = delFlag;
            TsipEmail.Runs = runs;

            //...Log2.v("\n\nTsipInitiator.EmailReportsToUser(): {0}\n", TsipEmail.ToString());

            // Make up to 5 attempts to send the email.
            string errMsg;
            for (int attempt = 1; attempt <= 5; attempt++)
            {
                retVal = TsipEmail.Send(out errMsg);

                if (retVal == Constant.SUCCESS)
                {
                    str = String.Format("On attempt {0} output from {1} emailed to {2}", attempt, fileRoot, emailAddress);
                    Console.Write("\n\n{0}", str);
                    TsipQ.WriteToTsipLog("\n" + str);
                    //...Log2.v("\nTsipInitiator.EmailReportsToUser(): " + str);
                    break;
                }
                else
                {
                    str = String.Format("ERROR: On attempt {0}, failed to send email from {1} to {2}.\n{3}", attempt, fileRoot, emailAddress, errMsg);
                    Console.Write("\n\n{0}", str);
                    TsipQ.WriteToTsipLog("\n" + str);
                    Log2.e("\nTsipInitiator.EmailReportsToUser(): " + str);
                }

                // 5 second delay before next send attempt.
                Thread.Sleep(5000);
            }

            return retVal;
        }


        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This application is launched by WebMICS whenever a user requests a TSIP run.   +");
            Console.Write("\r\n It provides the supervisory framework in which multiple instances of TpRunTsip +");
            Console.Write("\r\n can execute concurrently while other TSIP jobs wait in a queue. TsipInitiator  +");
            Console.Write("\r\n sends TSIP output reports to the MICS user as attachments to an email."); 
            Console.Write("\r\n\r\n");
            Console.Write("\r\n USAGE: TsipInitiator <dbName> <project> <paramTableName> [-o<prefix>] [-p<binDirPath>]");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        project          : user's project 'charge' code.");
            Console.Write("\r\n        paramTableName   : the XXX in TSIP parameter table tp_XXX_parm listed by WebMICS using:");
            Console.Write("\r\n                                --> Interference Analysis (TSIP)");
            Console.Write("\r\n                                    --> Open TSIP Parameter Files");
            Console.Write("\r\n");
            Console.Write("\r\n        --- Options ----------------------------------------------------------------");
            Console.Write("\r\n");
            Console.Write("\r\n        -o<prefix>       : a string added as a prefix to the names of TSIP output");
            Console.Write("\r\n                           report files, e.g. -oXXX produces TSIP report names like ");
            Console.Write("\r\n                                                XXX_tstest0183_3.CASEDET.");
            Console.Write("\r\n");
            Console.Write("\r\n        -p<binDirPath>   : TsipInitiator.exe spawns a Windows command-shell process ");
            Console.Write("\r\n                           that runs TpRunTsip.exe. By default, TsipInitiator looks for");
            Console.Write("\r\n                           TpRunTsip.exe in d:\\prod\\bin. If the -p option is used,");
            Console.Write("\r\n                           TsipInitiator looks for TpRunTsip.exe in the directory");
            Console.Write("\r\n                           prescribed by 'binDirPath'.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     TsipInitiator fcsa hulme1_0 -otsip myParamFileName");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with qty. 3 command-line arguments.");
            Console.Write("\r\n       2. Options can appear in any order at any position.");
            Console.Write("\r\n       3. This version has the 20260505 email password enhancement,");
            Console.Write("\r\n          with using SQL email");
            Console.Write("\r\n");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.ManagedBuildInfo());
        }

        /// <summary>
        /// This method parses the user-prescribed command-line arguments.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="dbName"></param>
        /// <param name="projectCode"></param>
        /// <param name="paramFileName"></param>
        /// <param name="reportPrefix"></param>
        public static void ParseCommandLineArgs(ref string[] args, out string dbName, out string projectCode, out string paramFileName, out string reportPrefix)
        {
            // 'out' requirement.
            dbName = "";
            projectCode = "";
            paramFileName = "";
            reportPrefix = "";

            List<string> newArgs = new List<string>();

            if (args.Length == 0)
            {
                WriteUsageToConsole();
                Application.Exit(Error.COMMAND_LINE_ERROR);
            }

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

            // Make a copy of the regular (mandatory) args.
            newArgs.AddRange(regularArgs);

            // Parse the option flags.
            foreach (string arg in flagArgs)
            {
                // Check that we don't just have a minus character.
                if (arg.Length == 1)
                {
                    Console.Write("\r\n Invalid flag: '{0}'", arg);
                    WriteUsageToConsole();
                    Application.Exit(Error.COMMAND_LINE_ERROR);
                }

                // Process the option flags.
                string flag = arg.Substring(0, 2).ToUpper();
                switch (flag)
                {
                    case "-O":
                        reportPrefix = arg.Substring(2);
                        // Copy the -O flag arg but NOT any -P flag/arg.
                        newArgs.Add(arg);
                        break;
                    case "-P":
                        string dirPath = arg.Substring(2);
                        if (!Directory.Exists(dirPath))
                        {
                            Console.Write("\r\n Directory does not exist: {0}\r\n", dirPath);
                            Log2.e("\n\nTsipInitiator.ParseCommandLineArgs(): ERROR: '-p' option: directory does not exist: " + dirPath);
                            WriteUsageToConsole();
                            Application.Exit(Error.COMMAND_LINE_ERROR);
                        }
                        Ssutil.SetMicsBinDirPath(dirPath);
                        break;
                    default:
                        Console.Write("\r\n Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.Exit(Error.COMMAND_LINE_ERROR);
                        break;
                }

            }

            // Parse the regular arguments; there must be qty. 3 of them.
            if (regularArgs.Count != 3)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.Exit(Error.COMMAND_LINE_ERROR);
            }

            // Parse the args.
            Info.DbName = dbName = regularArgs[0];
            Info.ProjectCode = projectCode = regularArgs[1];
            paramFileName = regularArgs[2];

            // Finally, return the newArgs as an array that replaces args[].
            args = newArgs.ToArray();

            //...Log2.v("\nTsipInitiator.ParseCommandLineArgs(): Info.DbName      = " + Info.DbName);
            //...Log2.v("\nTsipInitiator.ParseCommandLineArgs(): Info.ProjectCode = " + Info.ProjectCode);
            //...Log2.v("\nTsipInitiator.ParseCommandLineArgs(): paramFileName    = " + paramFileName);
            //...Log2.v("\nTsipInitiator.ParseCommandLineArgs(): reportPrefix     = " + reportPrefix);
        }



    }
}
