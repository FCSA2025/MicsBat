# Documented File: Application.cs
**Repository Path:** `_NewLib\Application.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A library comprising 'new' classes (in addition to the legacy Auxlib 
/// and Utillib libraries) created during the refactoring of the MICS 
/// source code from the legacy C/C++ code to the current C# code.
/// </summary>
namespace _NewLib
{
    /// <summary>
    /// Provides methods to redirect the stream Console.Out to a file, restore Console.Out and
    /// various ways to exit a MICS program by writing out informative meesages then terminating.
    /// </summary>
    public class Application
    {
#if PINVOKE
        [DllImport("_APItest.dll", CharSet = CharSet.Ansi)]
        private static extern void API_FlushStdOut();
        [DllImport("_APItest.dll", CharSet = CharSet.Ansi)]
        private static extern void API_RestoreStdOut();
#endif
        //-------------------------------------------------------------------------------------
        // For redirection of Console.Out to a file.
        static FileStream fs = null;
        static StreamWriter swRedirectionToFile = null;
        //----------------------------------------------------------------------------------

        /// <summary>
        /// This method sets the run-time process priority level for the
        /// current application.
        /// </summary>
        /// <remarks>
        /// The input parameter priorityLevel can be set to any one 
        /// of the following enumeration values:
        /// <list type="bullet">
        /// <item>ProcessPriorityClass.Idle;</item>
        /// <item>ProcessPriorityClass.BelowNormal; </item>
        /// <item>ProcessPriorityClass.Normal;</item>
        /// <item>ProcessPriorityClass.AboveNormal</item>
        /// <item>ProcessPriorityClass.High; OR</item>
        /// <item>ProcessPriorityClass.RealTime.</item>
        /// </list>
        /// </remarks>
        /// <param name="priorityLevel"> - see remarks.</param>
        public static void SetPriority(ProcessPriorityClass priorityLevel)
        {
            Process currentProcess = Process.GetCurrentProcess();
            try
            {
                currentProcess.PriorityClass = ProcessPriorityClass.Normal;
                //...Log2.v("\n\nApplication.SetPriority(): succeeded.");
            }
            catch (Exception e)
            {
                Log2.e("\n\nApplication.SetPriority(): ERROR: exception: " + e.Message);
                Console.Write("\nCould not set our priority class...");
            }
        }

        /// <summary>
        /// Redirects the stream Console.Out to write out to a prescribed file.
        /// </summary>
        /// <param name="filePath"> - full pathname of file to write to.</param>
        /// <param name="fileMode"> - FileMode.CreateNew, Create, Open, OpenOrCreate, Truncate or Append.</param>
        public static void RedirectConsoleOutToFile(string filePath, FileMode fileMode)
        {
            try
            {
                fs = new FileStream(filePath, fileMode);
                swRedirectionToFile = new StreamWriter(fs);
                Console.SetOut(swRedirectionToFile);
            }
            catch
            {
                Console.WriteLine("\r\nProcessing '-O' or '>>' option: exception caught trying to redirect StdOut");
                Application.Exit(98);
            }
        }

        /// <summary>
        /// Restores Console.Out
        /// </summary>
        public static void RestoreConsoleOut()
        {
            if (swRedirectionToFile != null)
            {
                // Close previous output stream and redirect output to standard output.
                Console.Out.Close();
                StreamWriter sw = new StreamWriter(Console.OpenStandardOutput());
                sw.AutoFlush = true;
                Console.SetOut(sw);

                swRedirectionToFile.Close();
                swRedirectionToFile = null;
            }
        }


        /// <summary>
        /// Terminate the application and return the exit code 666 to Windows.
        /// </summary>
        public static void Exit()
        {
            Exit(666);
        }

        /// <summary>
        /// Write the prescribed message to the Log file then terminate the application and return 
        /// the exit code 0 to Windows.
        /// </summary>
        /// <param name="message"> - information to be written to logFile.</param>
        public static void Exit(string message)
        {
            Exit(message, 0);
        }

        /// <summary>
        /// Write the prescribed message to the Log file then terminate the application and return 
        /// the prescribed exit code to Windows.
        /// </summary>
        /// <param name="message"> - information to be written to logFile.</param>
        /// <param name="exitCode"> - exit code to write out.</param>
        public static void Exit(string message, int exitCode)
        {
            if (!String.IsNullOrWhiteSpace(message))
            {
                //...Log2.v("\n\nApplication.Exit(): " + message);
            }

            Exit(exitCode);
        }

        /// <summary>
        /// Terminate the application with closing message to Console.Out; returns 
        /// the prescribed exit code to Windows.
        /// </summary>
        /// <param name="exitCode"></param>
        public static void Exit(int exitCode)
        {
            Console.Write("\n\n{0}.exe:  exit code = {1}\n", Info.ApplicationName, exitCode);
            //...Log2.v("\nApplication.Exit(): exit code = " + exitCode);

            if (exitCode != 0)
            {
                Log2.e("\n\nApplication.Exit(): StackTrace: " + Environment.StackTrace);
            }

            Console.Out.Flush();

            RestoreConsoleOut();

            Environment.Exit(exitCode);
        }

        /// <summary>
        /// Quietly terminate the application; returns the prescribed exit code to Windows.
        /// </summary>
        /// <param name="exitCode"></param>
        public static void ExitQuietly(int exitCode)
        {
            //...Log2.v("\n\nApplication.ExitQuietly(): exit code = " + exitCode);

            if (exitCode != 0)
            {
                Log2.e("\n\nApplication.Exit(): StackTrace: " + Environment.StackTrace);
            }

            Console.Out.Flush();

            RestoreConsoleOut();

            Environment.Exit(exitCode);
        }

    }
}

```
