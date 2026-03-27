using DBAccess;
using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace JobSubmission
{
    //	This is the general job submission routine using .NET functions with asynchronous read of batch program output.
    //  It allows the user to optionally enter the name of the output file.

    public class JobSubmitNet
    {
        private static string OutputFile;
        private static StringBuilder batStdErr = null;  // string of stderr output
        private static StringBuilder batStdOut = null;  // string of stdout output
        //private static StreamWriter swErr = null;       // sdterr output file
        private static StreamWriter swOut = null;       // stdout output file
        private static StreamWriter sw = null;          // log file
        private static int exit_code;
        private static int numerrlines;
        private static int numoutlines;

        public static void WriteLogger(dblogger oLog, StreamWriter sw)
        {
            sw.WriteLine("logserial: " + oLog.logserial);
            sw.WriteLine("logstarttime: " + oLog.logstarttime);
            sw.WriteLine("loguserid: " + oLog.loguserid);
            sw.WriteLine("logpcode: " + oLog.logpcode);
            sw.WriteLine("logprogram: " + oLog.logprogram);
            sw.WriteLine("logargs: " + oLog.logargs);
            sw.WriteLine("logfinishtime: " + oLog.logfinishtime);
            sw.WriteLine("logreturncode: " + oLog.logreturncode);
            sw.WriteLine("logerrorcode: " + oLog.logerrorcode);
            sw.WriteLine("logerrordesc: " + oLog.logerrordesc);
            sw.WriteLine("");

        }
        public static dblogger SubmitJob(dblogger oLog, string outFile, int wait_secs)
        {
            // this routine submits job using info in dblogger object and
            // it inserts a web.dblogger record before starting the process, 
            // and updates it on completion (or timeout)

            // wait_secs = <0:  submit job and return without waiting
            // wait_secs =  0:  submit job and wait for completion
            // wait_secs = >0: submit job and wait wait_secs seconds or for return

            // settings for logerrorcode are:
            // Process ran with no errors and no fcn errors:     0
            // Process ran with errors and no fcn errors:       -1
            // Failed to create duplicate token:               -90
            // Failed to create duplicate token:               -91*no longer used
            // Failed to get exit code from completed process: -92
            // Failed to update logger record:                 -95
            // Failed to insert logger record:                 -96
            // Failed to connect to database to insert logger: -97
            // Failed to create the process to run job:        -98
            // Job timed out:                                  -99

            // settings for logreturncode are:
            // Process ran with no errors and no fcn errors:     0
            // Process did not get started                      -1
            // Process started, but we do not wait for it       -2
            // Process started, but error getting exit code    -99
            // Other - process return code

            HttpContext ctx = HttpContext.Current;
            numerrlines = 0;
            numoutlines = 0;


            int wait_msecs = wait_secs * 1000;

            OutputFile = outFile;

            // set up logfile name
            string logfile = "";

            // write log info - skip if file is locked
            bool writeinfo = true;

            try  // try to reset default file
            {
                logfile = ctx.Application["web_drive"].ToString() + "\\extractlogs\\" +
                         ctx.Session["s_user"].ToString() + "submitNET.txt";
                sw = new StreamWriter(logfile, false);
                writeinfo = true;
            }
            catch  // default file failed - try one with timestamp
            {
                try
                {
                    DateTime curTime = DateTime.Now;
                    string disTime = curTime.ToString("yyyyMMddHHmmss");
                    logfile = ctx.Application["web_drive"].ToString() + "\\extractlogs\\" +
                        ctx.Session["s_user"].ToString() + "submitNET" + disTime + ".txt";

                    sw = new StreamWriter(logfile, false);
                    writeinfo = true;
                }
                catch  // timestamp version failed - leave writeinfo as false
                {
                }
            }

            // open output file
            try
            {
                StreamWriter swOut = new StreamWriter(outFile, false);
                if (writeinfo) sw.WriteLine("outfile:" + outFile + " opened");
            }
            catch (Exception)
            {
                if (writeinfo) sw.WriteLine("outfile:" + outFile + " open failed");
            }
            if (writeinfo) sw.WriteLine("CALLING ID:" + ctx.User.Identity.Name);
            if (writeinfo) sw.WriteLine(DateTime.Now + " : " + ctx.Session["FCSASESS"].ToString());
            if (writeinfo) sw.WriteLine("OUTFILE:" + OutputFile.ToString());
            if (writeinfo) sw.WriteLine("Program:" + oLog.logprogram);
            if (writeinfo) sw.WriteLine("Args:" + oLog.logargs);
            if (writeinfo) sw.WriteLine("B4 Impersonation:" + WindowsIdentity.GetCurrent().Name);
            if (writeinfo) sw.Flush();

            // get windows identity for impersonation
            //WindowsPrincipal wp = (WindowsPrincipal)ctx.Session["principalw"];
            //WindowsIdentity wi = (WindowsIdentity)wp.Identity;

            // create an impersonation context
            //WindowsImpersonationContext wic = wi.Impersonate();
            //if (writeinfo) sw.WriteLine("After Impersonation:" + WindowsIdentity.GetCurrent().Name);
            //if (writeinfo) sw.WriteLine("IMPUSER:" + wi.Name.ToString());

            bool redirected = false;  // tracks if stdout is redirected

            // all 'Print' batch jobs write directly to specified file
            // others write to standard output
            // so if file name specified for program other than *Print, we must capture redirected stdout
            if (outFile != " " && oLog.logprogram.IndexOf("Print") < 0)  // specific filename supplied - redirect stdout to it
            {
                redirected = true;
            }

            if (writeinfo) sw.WriteLine("Redirection:" + redirected.ToString());
            if (writeinfo) sw.WriteLine("Loading environment vars");

            if (writeinfo) sw.WriteLine("Loading environment vars");
            Environment.SetEnvironmentVariable("SqlInstance", ctx.Application["Sql_Instance"].ToString());
            Environment.SetEnvironmentVariable("MicsUser", ctx.Session["s_user"].ToString());
            Environment.SetEnvironmentVariable("Password", ctx.Session["s_password"].ToString());
            Environment.SetEnvironmentVariable("Domain", ctx.Application["AD_Domain"].ToString());
            Environment.SetEnvironmentVariable("webdrive", ctx.Application["web_drive"].ToString());
            Environment.SetEnvironmentVariable("work_dir", ctx.Session["user_dir"].ToString());
            Environment.SetEnvironmentVariable("DBName", ctx.Session["db_name"].ToString());
            Environment.SetEnvironmentVariable("MICS_PROJECT", ctx.Session["defProject"].ToString());
            Environment.SetEnvironmentVariable("MICS_NAD_FILE", ctx.Application["web_drive"].ToString() + "\\prod\\files\\ntv2_0");

            if (writeinfo) sw.WriteLine("Environment vars loaded");
            if (writeinfo) sw.Flush();
            oLog.logstarttime = DateTime.Now;

            // insert web.logger record for this process
            int logret;
            if ((logret = oLog.Start()) != 0)  // system error inserting dblogger record
            {
                if (writeinfo) sw.WriteLine("Error inserting dblogger");
                if (writeinfo) sw.WriteLine("oLogError:" + logret.ToString());
                if (writeinfo) sw.WriteLine("oLogDesc: " + oLog.logerrordesc);
                if (writeinfo) sw.Close();
                oLog.logreturncode = -1;
                return (oLog);
            }

            if (writeinfo) sw.WriteLine("logger inserted");

            // create Process object
            Process batProg = new Process();
            batProg.EnableRaisingEvents = true; // needed to get exit event and exit code

            //  load process startup info required to run program as prescribed Windows user.
            batProg.StartInfo.Domain = "FCSA";
            batProg.StartInfo.UserName = "FCSA\\venn1";
            if (writeinfo) sw.WriteLine(ctx.Session["s_password"].ToString());
            SecureString ss = new SecureString();
            foreach (char c in ctx.Session["s_password"].ToString())
            {
                ss.AppendChar(c);
            }

            batProg.StartInfo.Password = ss;

            if (writeinfo) sw.WriteLine("After StartInfo pwd");
            if (writeinfo) sw.Flush();

            //  load process startup info relating to job execution.
            batProg.StartInfo.FileName = oLog.logprogram;
            batProg.StartInfo.Arguments = oLog.logargs;
            batProg.StartInfo.UseShellExecute = false;  // no console display
            batProg.StartInfo.CreateNoWindow = true;
            batProg.StartInfo.UseShellExecute = false;

            // redirect stderr and create event handler
            batProg.StartInfo.RedirectStandardInput = true;
            batProg.StartInfo.RedirectStandardError = true;
            batStdErr = new StringBuilder("");
            batProg.ErrorDataReceived += new DataReceivedEventHandler(batProg_ErrorDataReceived);

            // if we are capturing stdout, then redirect stdout and create event handler
            if (redirected)
            {
                batProg.StartInfo.RedirectStandardOutput = true;
                batStdOut = new StringBuilder("");
                batProg.OutputDataReceived += new DataReceivedEventHandler(batProg_OutputDataReceived);
            }

            // set the batprog environment variables appropriate for the prescribed Windows user.
            batProg.StartInfo.EnvironmentVariables["SqlInstance"] = ctx.Application["Sql_Instance"].ToString();
            batProg.StartInfo.EnvironmentVariables["MicsUser"] = ctx.Session["s_user"].ToString();
            //batProg.StartInfo.EnvironmentVariables["Password"] = ctx.Session["s_password"].ToString();
            batProg.StartInfo.EnvironmentVariables["Domain"] = ctx.Application["AD_Domain"].ToString();
            batProg.StartInfo.EnvironmentVariables["webdrive"] = ctx.Application["web_drive"].ToString();
            batProg.StartInfo.EnvironmentVariables["work_dir"] = ctx.Session["user_dir"].ToString();
            //batProg.StartInfo.EnvironmentVariables["MICS_NAD_FILE"] = ctx.Application["web_drive"].ToString() + "\\prod\\files\\ntv2_0";
            batProg.StartInfo.EnvironmentVariables["MICS_PROJECT"] = ctx.Session["defProject"].ToString();
            if (writeinfo) sw.WriteLine("After batprog environment variables set");
            if (writeinfo) sw.Flush();

            // start batch job

            if (writeinfo) sw.WriteLine("Filename: " + batProg.StartInfo.FileName);
            if (writeinfo) sw.WriteLine("Args: " + batProg.StartInfo.Arguments);
            if (writeinfo) sw.WriteLine("Domain: " + batProg.StartInfo.Domain);
            if (writeinfo) sw.WriteLine("UserName: " + batProg.StartInfo.UserName);
            sw.Flush();

            try
            {
                //bool ret = batProg.Start(oLog.logprogram, oLog.logargs, "FCSA\\venn1",ss, "FCSA");
                bool ret = batProg.Start();
            }
            catch (Exception e2)
            {
                if (writeinfo) sw.WriteLine("batProg.Start failed:" + e2.Message);
                if (writeinfo) sw.Close();
                oLog.logreturncode = -1;
                oLog.logerrorcode = -98;  // create failed
                oLog.logerrordesc = "ERROR:Process.Start: " + e2.Message;
                return oLog;
            }

            DateTime batStart = batProg.StartTime;

            if (writeinfo) sw.WriteLine("batProg.Start succeeded at: " + batStart.ToString("yyyyMMddHHmmss"));

            // start read of redirected stderr
            batProg.BeginErrorReadLine();

            // start read of redirected stdout
            if (redirected)
            {
                batProg.BeginOutputReadLine();
                if (writeinfo) sw.WriteLine("BeginOutputReadLine started");
                if (writeinfo) sw.Flush();
            }

            // wait until process finishes or for specified time
            // if specified wait time is negative, return without waiting
            // if specified wait time is 0, wait is infinite
            // otherwise use actual wait time

            if (wait_secs > 0)  // wait for specified time
            {
                if (writeinfo) sw.WriteLine("Waiting for " + wait_msecs.ToString() + " millisecs");

                batProg.WaitForExit(wait_msecs);
                DateTime batExit = batProg.ExitTime;
                TimeSpan ElapsedTime = batExit.Subtract(batStart);
                double batRun = ElapsedTime.TotalMilliseconds;

                if (writeinfo) sw.WriteLine("batStart " + batStart.ToString("yyyyMMddHHmmss"));
                if (writeinfo) sw.WriteLine("batExit " + batExit.ToString("yyyyMMddHHmmss"));
                if (writeinfo) sw.WriteLine("batRun " + batRun.ToString());

                batProg.WaitForExit();  // this forces clearing of srderr and stdout bufers

                if ((int)batRun == wait_msecs)
                {
                    // if  TsipInitiator does not exit in specified time then exit here and let it run
                    if (oLog.logprogram.IndexOf("TsipInitiator") >= 0)
                    {
                        if (writeinfo) sw.WriteLine("TsipInitiator queued");
                        if (writeinfo) sw.Close();
                        oLog.logfinishtime = oLog.logstarttime;
                        oLog.logreturncode = 0;
                        oLog.logerrorcode = 0;
                        oLog.logerrordesc = "Left to run in queue";
                        return oLog;
                    }
                    else
                    {
                        if (writeinfo) sw.WriteLine("Program timed out");
                        if (writeinfo) sw.Close();
                        oLog.logreturncode = -1;
                        oLog.logerrorcode = -99;  // timeout
                        oLog.logerrordesc = "Program timed out";
                        return oLog;
                    }
                }
            }

            if (wait_secs == 0)   // wait for completion
            {
                if (writeinfo) sw.WriteLine("Waiting for completion");
                if (writeinfo) sw.Flush();
                batProg.WaitForExit();
            }

            if (wait_secs < 0)  // don't wait - just return
            {
                if (writeinfo) sw.WriteLine("Program submitted - returning");
                if (writeinfo) sw.Close();

                oLog.logreturncode = -2;
                oLog.logerrorcode = 0;  // submitted 
                oLog.logerrordesc = "Program submitted";
                return oLog;
            }

            // get process exit code
            try
            {
                exit_code = batProg.ExitCode;
                if (writeinfo) sw.WriteLine("Exit code: " + exit_code.ToString());
                if (writeinfo) sw.Flush();
            }
            catch (Exception e2)
            {
                if (writeinfo) sw.WriteLine(e2.Message);
                if (writeinfo) sw.Close();
                oLog.logreturncode = -99;
                oLog.logerrorcode = -92;
                oLog.logerrordesc = "ERROR:Cannot get exit code for process" + e2.Message;

                return oLog;
            }

            if (redirected)
            {
                // close output file

                swOut.Close();
            }

            if (writeinfo) sw.WriteLine("OK:" + exit_code.ToString());
            if (writeinfo) sw.WriteLine("");
            if (writeinfo) WriteLogger(oLog, sw);
            if (writeinfo) sw.Close();

            switch (exit_code)
            {
                case 0: // success
                    oLog.logfinishtime = DateTime.Now;
                    oLog.logreturncode = 0;
                    oLog.logerrorcode = 0;
                    oLog.logerrordesc = "";
                    break;
                default:
                    if ((oLog.logprogram.IndexOf("ftValidate") >= 0 || oLog.logprogram.IndexOf("feValidate") >= 0) &&
                        (exit_code < 98 || exit_code > 100))
                    {
                        // special case for ftValidate and feValidate
                        // these validation routines return exit code of number of errors found (except 98,99,100 are error conditions)
                        // so any other non-zero exit_code is overridden to return 0 
                        oLog.logfinishtime = DateTime.Now;
                        oLog.logreturncode = 0;
                        oLog.logerrorcode = 0;
                        oLog.logerrordesc = "";
                    }
                    else if (oLog.logprogram.IndexOf("sdfValidate") >= 0 && exit_code < 2)
                    {
                        // special case for sdfValidate
                        // this validation routine returns exit code of 0 if no errors found
                        // 1 af any errors found
                        // 2 if system error
                        oLog.logfinishtime = DateTime.Now;
                        oLog.logreturncode = 0;
                        oLog.logerrorcode = 0;
                        oLog.logerrordesc = "";
                    }
                    else if (oLog.logprogram.IndexOf("ftImport") >= 0 && (exit_code == -1))
                    {
                        // special case for ftImport
                        // this import routine returns exit code of 0 if no errors found
                        // -1 if any errors found
                        oLog.logfinishtime = DateTime.Now;
                        oLog.logreturncode = -1;
                        oLog.logerrorcode = 0;
                        oLog.logerrordesc = "Errors found";
                    }
                    else if (oLog.logprogram.IndexOf("esImport") >= 0 && exit_code <= 2)
                    {
                        // special case for esImport
                        // this import routine returns exit code of 1 if only warnings found
                        // 2 if any errors found
                        oLog.logfinishtime = DateTime.Now;
                        oLog.logreturncode = (int)exit_code;
                        oLog.logerrorcode = 0;
                        switch (exit_code)
                        {
                            case 1:
                                oLog.logerrordesc = "Completed with warnings only";
                                break;
                            case 2:
                                oLog.logerrordesc = "Completed with errors";
                                break;
                        }
                    }
                    else if (oLog.logprogram.IndexOf("TsipInitiator") >= 0 && exit_code == 2)
                    {
                        // special case for TsipInitiator 
                        // this routine returns exit code of 2 if job is duplicate for queue
                        oLog.logfinishtime = DateTime.Now;
                        oLog.logreturncode = 2;
                        oLog.logerrorcode = 2;
                        oLog.logerrordesc = "Duplicate job - cancelled";
                    }
                    else if (oLog.logprogram.IndexOf("tsipQdelete") >= 0 && exit_code > 0)
                    {
                        // special case for tsipQdelete 
                        // this routine returns exit code of:
                        // 1 if job number not in queue
                        // 2 if job found in queue, but belongs to other user
                        // 3 if not in wait state
                        // 10+ if other error
                        oLog.logfinishtime = DateTime.Now;
                        oLog.logreturncode = (int)exit_code;
                        oLog.logerrorcode = 0;
                        oLog.logerrordesc = "Job exited with code " + exit_code.ToString();
                    }
                    else if (oLog.logprogram.IndexOf("sdfImport") >= 0 && exit_code <= 2)
                    {
                        // special case for sdfImport
                        // this validation routine returns exit code of 0 if no errors found
                        // 1 af any warnings but no errors foud
                        // 2 if errors found
                        oLog.logfinishtime = DateTime.Now;
                        oLog.logreturncode = (int)exit_code;
                        oLog.logerrorcode = 0;
                        switch (exit_code)
                        {
                            case 1:
                                oLog.logerrordesc = "Completed with warnings only";
                                break;
                            case 2:
                                oLog.logerrordesc = "Completed with errors";
                                break;
                        }
                    }
                    else
                    {
                        oLog.logfinishtime = DateTime.Now;
                        oLog.logreturncode = (int)exit_code;
                        oLog.logerrorcode = -1;
                        oLog.logerrordesc = "Error " + oLog.logreturncode.ToString() + " from " + oLog.logprogram;
                    }
                    break;
            }
            return oLog;

        }

        static void batProg_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            // collect the stderr output
            if (!String.IsNullOrEmpty(e.Data))
            {
                numerrlines++;
                batStdErr.Append(Environment.NewLine + e.Data);
            }
        }

        static void batProg_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // collect the stdout output
            sw.Write("in batProg_OutputDataReceived");
            sw.Write(e.Data);

            sw.Flush();

            if (!String.IsNullOrEmpty(e.Data))
            {
                numoutlines++;
                //swOut.Write(e.Data);
                //swOut.Flush(); 
                batStdOut.Append(Environment.NewLine + e.Data);
            }
        }
    }
}
