using DBAccess;
using ErrorUtilities;
using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace JobSubmission
{
    public class JobSubmit
    {
        public static string OutputFile;
        public static string sout;
        // set up streamwriter for redirection
        //private static StreamWriter redirect;

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            public String lpReserved;
            public String lpDesktop;
            public String lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public SafeFileHandle hStdInput;
            public SafeFileHandle hStdOutput;
            public SafeFileHandle hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int Length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadFile(SafeFileHandle hFile, [Out] byte[] lpBuffer,
           uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        static extern bool CreatePipe(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe,
           ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr SecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public extern static SafeFileHandle GetStdHandle(int whichHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public extern static UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", EntryPoint = "SetHandleInformation", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static bool SetHandleInformation(SafeFileHandle hObject, int dwMask, uint dwFlags);

        [DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUser", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public extern static bool CreateProcessAsUser(IntPtr hToken, String lpApplicationName, String lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandle, int dwCreationFlags, IntPtr lpEnvironment,
            String lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        //[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        //public extern static bool CreateProcess(String lpApplicationName, String lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
        //    ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandle, int dwCreationFlags, IntPtr lpEnvironment,
        //    String lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        private const int HANDLE_FLAG_INHERIT = 0x00000001;
        private const UInt32 INFINITE = 0xFFFFFFFF;
        private const UInt32 WAIT_TIMEOUT = 0x00000102;
        public const Int32 Startf_UseStdHandles = 0x00000100;
        public const Int32 StdInputHandle = -10;
        public const Int32 StdOutputHandle = -11;
        public const Int32 StdErrorHandle = -12;

        //	This is the general job submission routine.  It allows the user to enter the name of the
        //	output file.
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

            SafeFileHandle stdinHandle = null;
            SafeFileHandle stdoutReadHandle = null;
            SafeFileHandle stdoutWriteHandle = null;
            SafeFileHandle stderrHandle = null;

            uint exit_code;
            uint wait_msecs = (uint)wait_secs * 1000;
            string prog_args = oLog.logprogram + " " + oLog.logargs;

            OutputFile = outFile;

            // set up logfile name
            string logfile = "";

            // write log info - skip if file is locked
            StreamWriter sw = null;
            bool writeinfo = true;

            try  // try to reset default file
            {
                logfile = ctx.Application["web_drive"].ToString() + "\\extractlogs\\" +
                         ctx.Session["site_type"] + "_" + ctx.Session["s_user"].ToString() + "submit5.txt";
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
                        ctx.Session["s_user"].ToString() + "submit5" + disTime + ".txt";

                    sw = new StreamWriter(logfile, false);
                    writeinfo = true;
                }
                catch  // timestamp version failed - leave writeinfo as false
                {
                }
            }

            if (writeinfo) sw.WriteLine("CALLING ID:" + ctx.User.Identity.Name);
            if (writeinfo) sw.WriteLine(DateTime.Now + " : " + ctx.Session["FCSASESS"].ToString());
            if (writeinfo) sw.WriteLine("OUTFILE:" + OutputFile.ToString());
            if (writeinfo) sw.WriteLine("Progargs:" + prog_args);
            if (writeinfo) sw.Flush();

            // set token pointers
            IntPtr Token = new IntPtr(0);
            bool ret;

            WindowsPrincipal wp = (WindowsPrincipal)ctx.Session["principalw"];
            WindowsIdentity wi = (WindowsIdentity)wp.Identity;
            Token = wi.Token;

            if (writeinfo) sw.WriteLine("wpidentity:" + wp.Identity.Name.ToString() + ":");
            if (writeinfo) sw.WriteLine("winame:" + wi.Name.ToString() + ":");
            if (writeinfo) sw.WriteLine("wpuser:" + wi.User.ToString() + ":");
            if (writeinfo) sw.Flush();

            // by setting descriptor to (IntPtr)0, this passes current default attributes for Token user
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.bInheritHandle = true;
            sa.Length = Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = (IntPtr)0;

            if (writeinfo) sw.WriteLine("IMPUSER:" + wi.Name.ToString()); sw.Flush();

            bool redirected = false;  // tracks if stdout is redirected
            if (outFile != " " && oLog.logprogram.IndexOf("Print") < 0)  // specific filename supplied - redirect stdout to it
            {
                redirected = true;
            }

            if (writeinfo) sw.WriteLine("Loading STARTUPINFO"); sw.Flush();

            // declare startup info object for new process
            STARTUPINFO si = new STARTUPINFO();

            // info for stdout redirection is loaded by default to avoid null handle errors

            SECURITY_ATTRIBUTES sa1 = new SECURITY_ATTRIBUTES();
            sa1.Length = Marshal.SizeOf(sa1);
            sa1.lpSecurityDescriptor = IntPtr.Zero;
            sa1.bInheritHandle = true;


            // load settings for process startup
            stdinHandle = GetStdHandle(StdInputHandle);
            CreatePipe(out stdoutReadHandle, out stdoutWriteHandle, ref sa1, 131072);
            // block inheritance of stdoutReadHandle
            SetHandleInformation(stdoutReadHandle, HANDLE_FLAG_INHERIT, 0);
            stderrHandle = GetStdHandle(StdErrorHandle);
            si.dwFlags = Startf_UseStdHandles;
            si.hStdInput = stdinHandle;
            si.hStdOutput = stdoutWriteHandle;
            si.hStdError = stderrHandle;
            si.cb = Marshal.SizeOf(si);
            si.lpDesktop = "";
            if (writeinfo) sw.WriteLine("Loaded STARTUPINFO redirected"); sw.Flush();

            if (writeinfo) sw.WriteLine("Loading environment vars");
            Environment.SetEnvironmentVariable("SqlInstance", ctx.Application["Sql_Instance"].ToString());
            // all batch program IsedTaflToTable apps must run under user hulme1, so override current use for these
            string UPProg = oLog.logprogram.ToUpper();
            if (UPProg.IndexOf("ISEDTAFLTOTABLE") == 0)
            {
                Environment.SetEnvironmentVariable("MicsUser", "hulme1");
            }
            else
            {
                Environment.SetEnvironmentVariable("MicsUser", ctx.Session["s_user"].ToString());
            }
            Environment.SetEnvironmentVariable("Password", ctx.Session["s_password"].ToString());
            Environment.SetEnvironmentVariable("Domain", ctx.Application["AD_Domain"].ToString());
            Environment.SetEnvironmentVariable("webdrive", ctx.Application["web_drive"].ToString());
            Environment.SetEnvironmentVariable("work_dir", ctx.Session["user_dir"].ToString());
            Environment.SetEnvironmentVariable("odbc", ctx.Application["ODBC_DSN"].ToString());
            Environment.SetEnvironmentVariable("DBName", ctx.Session["db_name"].ToString());
            Environment.SetEnvironmentVariable("MICS_PROJECT", ctx.Session["defProject"].ToString());
            Environment.SetEnvironmentVariable("MICS_NAD_FILE", ctx.Application["web_drive"].ToString() + "\\prod\\files\\ntv2_0");
            //Environment.SetEnvironmentVariable("SiteType", ctx.Application["Site_type"].ToString());
            // use db_name as proxy for SiteType

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
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            ret = CreateProcessAsUser(Token, null, prog_args, ref sa, ref sa, true, 0, (IntPtr)0, null, ref si, out pi);

            if (ret == false)
            {
                if (writeinfo) sw.WriteLine("CreateProcessAsUser failed with " + Marshal.GetLastWin32Error());
                if (writeinfo) sw.Close();

                // close pipe handles
                stdoutWriteHandle.Close();
                stdoutWriteHandle.Dispose();
                stdoutReadHandle.Close();
                stdoutReadHandle.Dispose();

                oLog.logreturncode = -1;
                oLog.logerrorcode = -98;  // create failed
                oLog.logerrordesc = "ERROR:CreateProcessAsUser: " + Marshal.GetLastWin32Error();
                return oLog;
            }
            else
            {
                if (writeinfo) sw.WriteLine("CreateProcessAsUser succeeded");

                // close process in and error handles
                if (!stdinHandle.IsClosed)
                {
                    stdinHandle.Close();
                    stdinHandle.Dispose();
                }
                if (!stderrHandle.IsClosed)
                {
                    stderrHandle.Close();
                    stderrHandle.Dispose();
                }
                // wait until process finishes or for specified time
                // if specified wait time is negative, return without waiting
                // if specified wait time is 0, wait is infinite
                // otherwise use actual wait time

                if (wait_secs > 0)  // wait for specified time
                {
                    if (writeinfo) sw.WriteLine("Waiting for " + wait_msecs.ToString() + " millisecs");

                    uint retwait = WaitForSingleObject(pi.hProcess, wait_msecs);

                    if (retwait == WAIT_TIMEOUT)
                    {
                        if (!redirected)
                        {
                            // close stdout read/write handles as job did not complete( or was tsipinitator)
                            stdoutWriteHandle.Close();
                            stdoutWriteHandle.Dispose();
                            stdoutReadHandle.Close();
                            stdoutReadHandle.Dispose();
                        }

                        CloseHandle(pi.hProcess);
                        CloseHandle(pi.hThread);

                        // if  TsipInitiator does not exit in specified time then exit here and let it run
                        if (oLog.logprogram.IndexOf("TsipInitiator") >= 0)
                        {
                            if (writeinfo) sw.WriteLine("TsipInitiator queued");
                            if (writeinfo) sw.Close();
                            oLog.logfinishtime = oLog.logstarttime;
                            oLog.logreturncode = 0;
                            oLog.logerrorcode = 0;
                            oLog.logerrordesc = "Left to run to completion";
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
                    else // completed early (before allowable wait time)
                    {
                        if (oLog.logprogram.IndexOf("TsipInitiator") >= 0)
                        {
                            if (writeinfo) sw.WriteLine("TsipInitiator complete");
                            if (writeinfo) sw.Close();
                            oLog.logfinishtime = oLog.logstarttime;
                            oLog.logreturncode = 0;
                            oLog.logerrorcode = 0;
                            oLog.logerrordesc = "Completed successfully";
                            return oLog;
                        }
                    }
                }

                if (wait_secs == 0)   // wait for completion
                {
                    if (writeinfo) sw.WriteLine("Waiting for completion");

                    WaitForSingleObject(pi.hProcess, INFINITE);
                }

                if (wait_secs < 0)  // don't wait - just return
                {
                    if (writeinfo) sw.WriteLine("Program submitted - returning");
                    if (writeinfo) sw.Close();

                    if (pi.hProcess != IntPtr.Zero) CloseHandle(pi.hProcess);
                    if (pi.hThread != IntPtr.Zero) CloseHandle(pi.hThread);

                    // close stdout read/write handles - never used
                    stdoutWriteHandle.Close();
                    stdoutWriteHandle.Dispose();
                    stdoutReadHandle.Close();
                    stdoutReadHandle.Dispose();

                    oLog.logreturncode = -2;
                    oLog.logerrorcode = 0;  // submitted 
                    oLog.logerrordesc = "Program submitted";
                    return oLog;
                }

                // get process exit code
                try
                {
                    GetExitCodeProcess(pi.hProcess, out exit_code);
                    if (writeinfo) sw.WriteLine("Exit code: " + exit_code.ToString());
                }
                catch (Exception e2)
                {
                    if (writeinfo) sw.WriteLine(e2.Message);
                    if (writeinfo) sw.Close();
                    oLog.logreturncode = -99;
                    oLog.logerrorcode = -92;
                    oLog.logerrordesc = "ERROR:Cannot get exit code for process" + e2.Message;

                    // close all open handles
                    stdoutReadHandle.Close();
                    stdoutReadHandle.Dispose();
                    stdoutWriteHandle.Close();
                    stdoutWriteHandle.Dispose();
                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                    return oLog;
                }

                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);

            }

            if (redirected)  // read any info from stdout pipe
            {
                // close stdout write handle to flush data
                stdoutWriteHandle.Close();
                stdoutWriteHandle.Dispose();

                // write output file
                StreamWriter sw2 = new StreamWriter(outFile, false);
                byte[] buffer = new byte[4096];
                IntPtr arg = IntPtr.Zero;
                uint dwRead = 0;
                bool success = false;

                for (; ; )
                {
                    Array.Clear(buffer, 0, 4096);
                    success = ReadFile(stdoutReadHandle, buffer, 4096, out dwRead, arg);
                    if (writeinfo) sw.WriteLine("Success:" + success.ToString() + " Bytes:" + dwRead.ToString());
                    if (success)  //read was successful
                    {
                        if (dwRead == 0)  // no more bytes read 
                        {
                            break;
                        }
                        else if (dwRead == 4096) // buffer is full
                        {
                            //if (writeinfo) sw.WriteLine("Writing buffer:");
                            //byte[] pbyte100 = new byte[100];
                            //System.Buffer.BlockCopy(buffer, 0, pbyte100, 0, 100);
                            //if (writeinfo) sw.WriteLine(Encoding.UTF8.GetString(pbyte100));

                            sw2.Write(Encoding.UTF8.GetString(buffer));
                        }
                        else  // buffer is partially loaded
                        {
                            if (writeinfo) sw.WriteLine("Writing partial buffer:");
                            //byte[] pbyte100 = new byte[100];
                            //System.Buffer.BlockCopy(buffer, 0, pbyte100, 0, 100);
                            //if (writeinfo) sw.WriteLine(Encoding.UTF8.GetString(pbyte100));

                            int count = (int)dwRead;
                            byte[] pbyte = new byte[count];
                            System.Buffer.BlockCopy(buffer, 0, pbyte, 0, count);
                            sw2.Write(Encoding.UTF8.GetString(pbyte));
                        }
                    }
                    else
                    {
                        break;
                    }
                    //if (!success || dwRead == 0)
                    //    break;
                    //if (writeinfo) sw.WriteLine("Writing buffer:");

                    //sw2.Write(Encoding.UTF8.GetString(buffer));
                }
                sw2.Close();
                // close stdout read handle
                stdoutReadHandle.Close();
                stdoutReadHandle.Dispose();
            }
            else
            {
                stdoutReadHandle.Close();
                stdoutReadHandle.Dispose();
                stdoutWriteHandle.Close();
                stdoutWriteHandle.Dispose();
            }

            if (writeinfo) sw.WriteLine("OK:" + exit_code.ToString());
            if (writeinfo) sw.WriteLine("");


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
                    else if (oLog.logprogram.IndexOf("ftImport") >= 0 && exit_code == 4294967295)   // 429.. is uint32 for -1
                    //else if (oLog.logprogram.IndexOf("ftImport") >= 0 && exit_code == 1)   // 429.. is uint32 for -1
                    {
                        // special case for ftImport
                        // this import routine returns exit code of 0 if no errors found
                        // -1 if any errors found
                        oLog.logfinishtime = DateTime.Now;
                        oLog.logreturncode = -1;
                        oLog.logerrorcode = 0;
                        oLog.logerrordesc = "Errors found";
                    }
                    else if (oLog.logprogram.IndexOf("feImport") >= 0 && exit_code == 4294967295)   // 429.. is uint32 for -1
                    {
                        // special case for esImport
                        // this import routine returns exit code of 0 if no errors found
                        // -1 if any errors found
                        oLog.logfinishtime = DateTime.Now;
                        oLog.logreturncode = -1;
                        oLog.logerrorcode = 0;
                        oLog.logerrordesc = "Errors found";
                    }           
                    else if (oLog.logprogram.IndexOf("pl5Import") >= 0 && exit_code <= 2)
                    {
                        // special case for pl5Import (Pathloss)
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
            if (writeinfo) WriteLogger(oLog, sw);
            if (writeinfo) sw.Close();
            return oLog;

        }
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
        /*********************************************************************************************/
        public static dblogger SubmitJob2(dblogger oLog, string outFile, int wait_secs)
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

            SafeFileHandle stdinHandle = null;
            //SafeFileHandle stderrHandle = null;

            uint exit_code;
            uint wait_msecs = (uint)wait_secs * 1000;
            string prog_args = oLog.logprogram + " " + oLog.logargs;

            OutputFile = outFile;

            // set up logfile name
            string logfile = "";

            // write log info - skip if file is locked
            StreamWriter sw = null;
            bool writeinfo = false;

            try  // try to reset default file
            {
                logfile = ctx.Application["web_drive"].ToString() + "\\extractlogs\\" +
                         ctx.Session["s_user"].ToString() + "submit6.txt";
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
                        ctx.Session["s_user"].ToString() + "submit6" + disTime + ".txt";

                    sw = new StreamWriter(logfile, false);
                    writeinfo = true;
                }
                catch  // timestamp version failed - leave writeinfo as false
                {
                }
            }

            if (writeinfo) sw.WriteLine("CALLING ID:" + ctx.User.Identity.Name);
            if (writeinfo) sw.WriteLine(DateTime.Now + " : " + ctx.Session["FCSASESS"].ToString());
            if (writeinfo) sw.WriteLine("OUTFILE:" + OutputFile.ToString());
            if (writeinfo) sw.WriteLine("Progargs:" + prog_args);
            if (writeinfo) sw.Flush();

            // set token pointers
            IntPtr Token = new IntPtr(0);
            bool ret;

            WindowsPrincipal wp = (WindowsPrincipal)ctx.Session["principalw"];
            WindowsIdentity wi = (WindowsIdentity)wp.Identity;
            Token = wi.Token;

            if (writeinfo) sw.WriteLine("wpidentity:" + wp.Identity.Name.ToString() + ":");
            if (writeinfo) sw.WriteLine("winame:" + wi.Name.ToString() + ":");
            if (writeinfo) sw.WriteLine("wpuser:" + wi.User.ToString() + ":");
            if (writeinfo) sw.Flush();

            // by setting descriptor to (IntPtr)0, this passes current default attributes for Token user
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.bInheritHandle = true;
            sa.Length = Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = (IntPtr)0;

            if (writeinfo) sw.WriteLine("IMPUSER:" + wi.Name.ToString());

            bool redirected = false;  // tracks if stdout is to be redirected
            if (outFile != " " && oLog.logprogram.IndexOf("Print") < 0)  // specific filename supplied - redirect stdout to it
            {
                redirected = true;
            }

            if (writeinfo) sw.WriteLine("Loading STARTUPINFO");

            // declare startup info object for new process
            STARTUPINFO si = new STARTUPINFO();

            //SECURITY_ATTRIBUTES sa1 = new SECURITY_ATTRIBUTES();
            //sa1.Length = Marshal.SizeOf(sa1);
            //sa1.lpSecurityDescriptor = IntPtr.Zero;
            //sa1.bInheritHandle = true;

            // load settings for process startup
            const uint GENERIC_WRITE = 0x40000000;
            const uint FILE_SHARE_WRITE = 0x00000002;
            const uint CREATE_ALWAYS = 0x00000004;
            const uint KEEP_FILE = 0x00000000;

            SafeFileHandle outHandle = (SafeFileHandle)null;
            bool outHandleCreated = false;
            SafeFileHandle errHandle = (SafeFileHandle)null;
            bool errHandleCreated = false;

            try
            {
                //public static extern SafeFileHandle CreateFile(
                //string lpFileName,
                //uint dwDesiredAccess,
                //uint dwShareMode,
                //IntPtr SecurityAttributes,
                //uint dwCreationDisposition,
                //uint dwFlagsAndAttributes,
                //IntPtr hTemplateFile               

                outHandle = CreateFile("\\\\.\\" + outFile,
                    GENERIC_WRITE,
                    FILE_SHARE_WRITE,
                    (IntPtr)0,
                    CREATE_ALWAYS,
                    KEEP_FILE,
                    (IntPtr)null);

                bool success = SetHandleInformation(outHandle, HANDLE_FLAG_INHERIT, 1);

                outHandleCreated = true;

                if (writeinfo) sw.WriteLine("HandleCreated:" + outHandleCreated.ToString());

                errHandle = CreateFile("\\\\.\\" + outFile + "err",
                    GENERIC_WRITE,
                    FILE_SHARE_WRITE,
                    (IntPtr)0,
                    CREATE_ALWAYS,
                    KEEP_FILE,
                    (IntPtr)null);

                success = SetHandleInformation(errHandle, HANDLE_FLAG_INHERIT, 1);
                errHandleCreated = true;
                if (writeinfo) sw.WriteLine("HandleCreated:" + errHandleCreated.ToString());
            }
            catch (Exception ex)
            {
                outHandleCreated = false;
                if (writeinfo) sw.WriteLine("HandleCreated:" + outHandleCreated.ToString());
                errHandleCreated = false;
                if (writeinfo) sw.WriteLine("HandleCreated:" + errHandleCreated.ToString());
                if (writeinfo) sw.Close();
                ErrorUtils.NotifySystemOps(ex, "CreateFile failed");
            }

            stdinHandle = GetStdHandle(StdInputHandle);
            //CreatePipe(out stdoutReadHandle, out stdoutWriteHandle, ref sa1, 32768);
            // block inheritance of stdoutReadHandle
            //SetHandleInformation(stdoutReadHandle, HANDLE_FLAG_INHERIT, 0);
            si.dwFlags = Startf_UseStdHandles;
            si.hStdInput = stdinHandle;
            si.hStdOutput = outHandle;
            si.hStdError = errHandle;
            si.lpDesktop = "";
            si.cb = Marshal.SizeOf(si);
            if (writeinfo) sw.WriteLine("Loaded STARTUPINFO redirected to file");


            if (writeinfo) sw.WriteLine("Loading environment vars");
            Environment.SetEnvironmentVariable("SqlInstance", ctx.Application["Sql_Instance"].ToString());
            Environment.SetEnvironmentVariable("MicsUser", ctx.Session["s_user"].ToString());
            Environment.SetEnvironmentVariable("Password", ctx.Session["s_password"].ToString());
            Environment.SetEnvironmentVariable("Domain", ctx.Application["AD_Domain"].ToString());
            Environment.SetEnvironmentVariable("webdrive", ctx.Application["web_drive"].ToString());
            Environment.SetEnvironmentVariable("work_dir", ctx.Session["user_dir"].ToString());
            Environment.SetEnvironmentVariable("MICS_NAD_FILE", ctx.Application["web_drive"].ToString() + "\\prod\\files\\ntv2_0");
            Environment.SetEnvironmentVariable("MICS_PROJECT", ctx.Session["defProject"].ToString());
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
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            ret = CreateProcessAsUser(Token, null, prog_args, ref sa, ref sa, true, 0, (IntPtr)0, null, ref si, out pi);

            if (ret == false)
            {
                if (writeinfo) sw.WriteLine("CreateProcessAsUser failed with " + Marshal.GetLastWin32Error());
                if (writeinfo) sw.Close();

                oLog.logreturncode = -1;
                oLog.logerrorcode = -98;  // create failed
                oLog.logerrordesc = "ERROR:CreateProcessAsUser: " + Marshal.GetLastWin32Error();
                return oLog;
            }
            else
            {
                if (writeinfo) sw.WriteLine("CreateProcessAsUser succeeded");

                // close process stdin and error handles
                //if (!stdinHandle.IsClosed)
                //{
                //    stdinHandle.Close();
                //stdinHandle.Dispose();
                //}
                //if (!stderrHandle.IsClosed)
                //{
                //    stderrHandle.Close();
                //stderrHandle.Dispose();
                //}
                //if (!outHandle.IsClosed)
                //{
                //    outHandle.Close();
                //   outHandle.Dispose();
                // }

                // wait until process finishes or for specified time
                // if specified wait time is negative, return without waiting
                // if specified wait time is 0, wait is infinite
                // otherwise use actual wait time

                if (wait_secs > 0)  // wait for specified time
                {
                    if (writeinfo) sw.WriteLine("Waiting for " + wait_msecs.ToString() + " millisecs");

                    uint retwait = WaitForSingleObject(pi.hProcess, wait_msecs);

                    if (!outHandle.IsClosed)
                    {
                        outHandle.Close();
                        outHandle.Dispose();
                    }

                    if (!errHandle.IsClosed)
                    {
                        errHandle.Close();
                        errHandle.Dispose();
                    }

                    if (retwait == WAIT_TIMEOUT)
                    {

                        CloseHandle(pi.hProcess);
                        CloseHandle(pi.hThread);

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

                    WaitForSingleObject(pi.hProcess, INFINITE);
                    if (!outHandle.IsClosed)
                    {
                        outHandle.Close();
                        outHandle.Dispose();
                    }
                    if (!errHandle.IsClosed)
                    {
                        errHandle.Close();
                        errHandle.Dispose();
                    }

                }

                if (wait_secs < 0)  // don't wait - just return
                {
                    if (writeinfo) sw.WriteLine("Program submitted - returning");
                    if (writeinfo) sw.Close();

                    if (pi.hProcess != IntPtr.Zero) CloseHandle(pi.hProcess);
                    if (pi.hThread != IntPtr.Zero) CloseHandle(pi.hThread);

                    oLog.logreturncode = -2;
                    oLog.logerrorcode = 0;  // submitted 
                    oLog.logerrordesc = "Program submitted";
                    return oLog;
                }

                // get process exit code
                try
                {
                    GetExitCodeProcess(pi.hProcess, out exit_code);
                    if (writeinfo) sw.WriteLine("Exit code: " + exit_code.ToString());
                }
                catch (Exception e2)
                {
                    if (writeinfo) sw.WriteLine(e2.Message);
                    if (writeinfo) sw.Close();
                    oLog.logreturncode = -99;
                    oLog.logerrorcode = -92;
                    oLog.logerrordesc = "ERROR:Cannot get exit code for process" + e2.Message;

                    // close all open handles
                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                    return oLog;
                }

                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);

            }

            if (redirected)  // read any info from stdout pipe
            {

            }
            else
            {
            }
            if (writeinfo) sw.WriteLine("OK:" + exit_code.ToString());
            if (writeinfo) sw.WriteLine("");

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
                    else if (oLog.logprogram.IndexOf("ftImport") >= 0 && exit_code != 0)   // failed
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
            if (writeinfo) WriteLogger(oLog, sw);
            if (writeinfo) sw.Close();

            return oLog;

        }
        public static dblogger SubmitJobPwd(dblogger oLog, int wait_secs)
        {
            // this routine submits job using info in dblogger object

            // wait_secs =  0:  submit job and wait for completion

            // settings for logerrorcode are:
            // Process ran with no errors and no fcn errors:     0
            // Process ran with errors and no fcn errors:       -1
            // Failed to get exit code from completed process: -92
            // Failed to create the process to run job:        -98
            // Job timed out:                                  -99

            // settings for logreturncode are:
            // Process ran with no errors and no fcn errors:     0
            // Process did not get started                      -1
            // Process started, but we do not wait for it       -2
            // Process started, but error getting exit code    -99
            // Other - process return code

            HttpContext ctx = HttpContext.Current;

            // set up logfile name
            string logfile = "";

            // write log info - skip if file is locked
            StreamWriter sw = null;
            bool writeinfo = true;

            try  // try to reset default file
            {
                logfile = ctx.Application["web_drive"].ToString() + "\\extractlogs\\" +
                          "venn1submitpwd.txt";
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
                         "venn1submitpwd" + disTime + ".txt";

                    sw = new StreamWriter(logfile, false);
                    writeinfo = true;
                }
                catch  // timestamp version failed - leave writeinfo as false
                {
                }
            }

            oLog.logstarttime = DateTime.Now;

            Process myProcess = null;

            try
            {
                myProcess = Process.Start(oLog.logprogram, oLog.logargs);
            }
            catch (Exception e1)
            {
                ErrorUtils.NotifySystemOps(e1, "SubmitJobPwd");
                oLog.logreturncode = -1;
                oLog.logerrorcode = -98;  // create failed
                oLog.logerrordesc = "ERROR:Process.Start: " + Marshal.GetLastWin32Error();
                return oLog;
            }

            myProcess.WaitForExit();
            int exit_code = myProcess.ExitCode;

            sw.WriteLine("exit: " + exit_code.ToString());

            if (writeinfo) sw.WriteLine("");
            switch (exit_code)
            {
                case 0: // success
                    oLog.logfinishtime = DateTime.Now;
                    oLog.logreturncode = 0;
                    oLog.logerrorcode = 0;
                    oLog.logerrordesc = "";
                    break;
                default:
                    // only case is for user AD password change
                    oLog.logfinishtime = DateTime.Now;
                    oLog.logreturncode = (int)exit_code;
                    oLog.logerrorcode = 0;
                    oLog.logerrordesc = "";
                    break;
            }

            if (writeinfo) WriteLogger(oLog, sw);
            if (writeinfo) sw.Close();

            return oLog;

        }
    }
}
