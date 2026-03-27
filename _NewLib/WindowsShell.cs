//using _Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace _NewLib
{
    /// <summary>
    /// This class encapsulates the functionality of running a single command
    /// in a Windows Shell (Command Prompt) and returning the resulting
    /// stdout, stderr and exitCode output data.
    /// </summary>
    public class WindowsShell
    {
        public const int SUCCESS = 0;
        public const int FAILURE = 0;

        private static bool mIsProcessing;
        private static StringBuilder mSbStdOut;
        private static StringBuilder mSbStdErr;

        /// <summary>
        /// This method processes a single command in a Windows Shell and
        /// provides the resulting stdout, stderr and exitCode data; command-line
        /// arguments are prescribed as a string[].
        /// </summary>
        /// <param name="command"> - Windows command to be executed, e.g. "D:\Users\ahulme\MICS#\_bin\Release\FeImport.exe"</param>
        /// <param name="args"> - command line arguments as string[].</param>
        /// <param name="stdOut"> - resulting stdout data.</param>
        /// <param name="stdErr"> - resulting stderr data.</param>
        /// <param name="exitCode"> - resulting exit code.</param>
        /// <returns>Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int RunCommand(string command, string[] args, out string stdOut, out string stdErr, out int exitCode)
        {
            StringBuilder sb = new StringBuilder(" ");

            foreach (string arg in args)
            {
                sb.Append(arg);
                sb.Append(" ");
            }

            int nRet = RunCommand(command, sb.ToString(), out stdOut, out stdErr, out exitCode);

            return nRet;
        }

        /// <summary>
        /// This method processes a single command in a Windows Shell and
        /// provides the resulting stdout, stderr and exitCode data; command-line
        /// arguments are prescribed in a single string.
        /// </summary>
        /// <param name="command"> - Windows command to be executed, e.g. "D:\Users\ahulme\MICS#\_bin\Release\FeImport.exe"</param>
        /// <param name="args"> - command line arguments as a single string.</param>
        /// <param name="stdOut"> - resulting stdout data.</param>
        /// <param name="stdErr"> - resulting stderr data.</param>
        /// <param name="exitCode"> - resulting exit code.</param>
        /// <returns>Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int RunCommand(string command, string args, out string stdOut, out string stdErr, out int exitCode)
        {
            return RunCommand(null, command, args, out stdOut, out stdErr, out exitCode);
        }

        /// <summary>
        /// This method processes a single command in a Windows Shell and
        /// provides the resulting stdout, stderr and exitCode data; command-line
        /// arguments are prescribed in a single string; this version of the 
        /// method receives MICSUSER as an argument and should be used if the 
        /// command being run calls another MICS# program that accesses the database.
        /// </summary>
        /// <param name="micsUser"> - Prescribes the MICS username (equivalent to the MICSUSER environment variable.</param>
        /// <param name="command"> - Windows command to be executed, e.g. "D:\Users\ahulme\MICS#\_bin\Release\FeImport.exe"</param>
        /// <param name="args"> - command line arguments as a single string.</param>
        /// <param name="stdOut"> - resulting stdout data.</param>
        /// <param name="stdErr"> - resulting stderr data.</param>
        /// <param name="exitCode"> - resulting exit code.</param>
        /// <returns>Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int RunCommand(string micsUser, string command, string args, out string stdOut, out string stdErr, out int exitCode)
        {
            // 'out' requirements.
            stdOut = "";
            stdErr = "";
            exitCode = 0;
            int retVal = SUCCESS;

            //...Log2.v("\nWindowsShell.RunCommand(): " + command + "  " + args);

            mIsProcessing = true;

            try
            {
                // Instantiate the StringBuilder objects used to accumulate data for the
                // stdout and stderr output streams.
                mSbStdOut = new StringBuilder();
                mSbStdErr = new StringBuilder();

                // Instantiate a ProcessStartInfo object.
                ProcessStartInfo startInfo = new ProcessStartInfo(command, args);

                // This version of RunCommand() sets micsUser.
                if (!String.IsNullOrWhiteSpace(micsUser))
                {
                    startInfo.EnvironmentVariables["MICSUSER"] = micsUser;
                    startInfo.EnvironmentVariables["PASSWORD"] = "bananarama";
                }

                // Redirect the standard output of the process. 
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                // Set UseShellExecute to false for redirection
                startInfo.UseShellExecute = false;

                // Instantiate a new process.
                Process proc = new Process();

                // Attach the ProcessStartInfo object to the process.
                proc.StartInfo = startInfo;

                // Prescribe that the 'Exited' event should be raised when the process terminates.
                proc.EnableRaisingEvents = true;

                // Attach an event handler to asynchronously read stdout.
                proc.OutputDataReceived += new DataReceivedEventHandler(ProcessStdOutDataReceived);

                // Attach an event handler to asynchronously read stderr.
                proc.ErrorDataReceived += new DataReceivedEventHandler(ProcessStdErrDataReceived);

                // Attach an event handler to catch the processes' Exit event.
                proc.Exited += new EventHandler(ProcessExited);

                // Start the process.
                proc.Start();

                // Set the process's runtime priority.
                proc.PriorityClass = ProcessPriorityClass.Normal;

                // Start the asynchronous read of the sort output stream. Note this line!
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                while (mIsProcessing == true)
                {
                    Thread.Sleep(100);  // Check every 0.1s.
                }

                // Set the out parameters.
                stdOut = mSbStdOut.ToString();
                stdErr = mSbStdErr.ToString();
                exitCode = proc.ExitCode;

                //...Log2.v("\nWindowsShell.RunCommand(): stdOut:\n" + stdOut);
                //...Log2.v("\nWindowsShell.RunCommand(): stdErr:\n" + stdErr);
                //...Log2.v("\nWindowsShell.RunCommand(): exitCode: " + exitCode);

            }
            catch (Exception e)
            {
                Log2.e("\nWindowsShell.RunCommand(): ERROR: " + command + "  " + args);
                Log2.e("\nWindowsShell.RunCommand(): ERROR: exception: " + e.Message);
                retVal = FAILURE;
            }

            return retVal;
        }

        /// <summary>
        /// This method processes a single command in a Windows Shell and
        /// provides the resulting stdout, stderr and exitCode data; command-line
        /// arguments are prescribed in a single string; this version of the 
        /// method receives MICSUSER as an argument and should be used if the 
        /// command being run calls another MICS# program that accesses the database.
        /// </summary>
        /// <param name="windowsLoginDomain"> - Prescribes the Windows network domain name.</param>
        /// <param name="windowsLoginUsername"> - Prescribes the MICS username (equivalent to the MICSUSER environment variable.</param>
        /// <param name="windowsLoginPassword"> - Prescribes the user's Windows login password.</param>
        /// <param name="micsUserEnvVar"> - Prescribes the value of the environment variable MICSUSER.</param>
        /// <param name="command"> - Windows command to be executed, e.g. "D:\Users\ahulme\MICS#\_bin\Release\FeImport.exe"</param>
        /// <param name="args"> - command line arguments as a single string.</param>
        /// <param name="stdOut"> - resulting stdout data.</param>
        /// <param name="stdErr"> - resulting stderr data.</param>
        /// <param name="exitCode"> - resulting exit code.</param>
        /// <returns>Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int RunCommand(string windowsLoginDomain, string windowsLoginUsername, string windowsLoginPassword, string micsUserEnvVar, string command, string args, out string stdOut, out string stdErr, out int exitCode)
        {
            // 'out' requirements.
            stdOut = "";
            stdErr = "";
            exitCode = 0;
            int retVal = SUCCESS;

            //...Log2.v("\nWindowsShell.RunCommand(): " + command + "  " + args);

            mIsProcessing = true;

            try
            {
                // Instantiate the StringBuilder objects used to accumulate data for the
                // stdout and stderr output streams.
                mSbStdOut = new StringBuilder();
                mSbStdErr = new StringBuilder();

                // Instantiate a ProcessStartInfo object.
                ProcessStartInfo startInfo = new ProcessStartInfo(command, args);

                startInfo.Domain = windowsLoginDomain;
                startInfo.UserName = windowsLoginUsername;

                SecureString ss = new SecureString();
                foreach (char c in windowsLoginPassword) ss.AppendChar(c);
                startInfo.Password = ss;

                // This version of RunCommand() sets micsUser.
                if (!String.IsNullOrWhiteSpace(windowsLoginUsername))
                {
                    startInfo.EnvironmentVariables["MICSUSER"] = micsUserEnvVar;
                    startInfo.EnvironmentVariables["PASSWORD"] = "bananarama";
                }

                // Load the entire Windows user profile from the registry.
                startInfo.LoadUserProfile = true;

                // Redirect the standard output of the process. 
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                // Set UseShellExecute to false for redirection
                startInfo.UseShellExecute = false;

                // Disable the Windows command window that would otherwise pop-up.
                startInfo.CreateNoWindow = true;

                // Instantiate a new process.
                Process proc = new Process();

                // Attach the ProcessStartInfo object to the process.
                proc.StartInfo = startInfo;

                // Prescribe that the 'Exited' event should be raised when the process terminates.
                proc.EnableRaisingEvents = true;

                // Attach an event handler to asynchronously read stdout.
                proc.OutputDataReceived += new DataReceivedEventHandler(ProcessStdOutDataReceived);

                // Attach an event handler to asynchronously read stderr.
                proc.ErrorDataReceived += new DataReceivedEventHandler(ProcessStdErrDataReceived);

                // Attach an event handler to catch the processes' Exit event.
                proc.Exited += new EventHandler(ProcessExited);

                // Start the process.
                proc.Start();

                // Set the process's runtime priority.
                proc.PriorityClass = ProcessPriorityClass.Normal;

                // Start the asynchronous read of the sort output stream. Note this line!
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                while (mIsProcessing == true)
                {
                    Thread.Sleep(100);  // Check every 0.1s.
                }

                // Set the out parameters.
                stdOut = mSbStdOut.ToString();
                stdErr = mSbStdErr.ToString();
                exitCode = proc.ExitCode;

                //...Log2.v("\nWindowsShell.RunCommand(): stdOut:\n" + stdOut);
                //...Log2.v("\nWindowsShell.RunCommand(): stdErr:\n" + stdErr);
                //...Log2.v("\nWindowsShell.RunCommand(): exitCode: " + exitCode);

            }
            catch (Exception e)
            {
                Log2.e("\nWindowsShell.RunCommand(): ERROR: " + command + "  " + args);
                Log2.e("\nWindowsShell.RunCommand(): ERROR: exception: " + e.Message);
                Log2.e("\n{0}", e.StackTrace);
                retVal = FAILURE;
            }

            return retVal;
        }


        /// <summary>
        /// This method provides an event handler for the process 'Exited'
        /// event that is triggered when the command (being processed) terminates. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ProcessExited(object sender, EventArgs e)
        {
            ((Process)sender).WaitForExit();

            //Console.WriteLine("Exited (Event)");
            mIsProcessing = false;
        }

        /// <summary>
        /// This method provides an event handler for the 'stderr data received'
        /// event that is triggered whenever the stderr buffer gets "full-up" (4KB)
        /// or the command terminates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ProcessStdErrDataReceived(object sender, DataReceivedEventArgs e)
        {
            //Console.WriteLine("Error data: {0}", e.Data);
            mSbStdErr.AppendLine(e.Data);
        }

        /// <summary>
        /// This method provides an event handler for the 'stdout data received'
        /// event that is triggered whenever the stdout buffer gets "full-up" (4KB)
        /// or the command terminates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ProcessStdOutDataReceived(object sender, DataReceivedEventArgs e)
        {
            //Console.WriteLine("Output data: {0}", e.Data);
            mSbStdOut.AppendLine(e.Data);
        }










    }
}
