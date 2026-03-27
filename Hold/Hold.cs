using _Configuration;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// This program attempts to place a 'HOLD' on the database that will block other
/// programs from accessing it.
/// </summary>
/// <remarks>
/// It first checks that there is not already a hold on the database.If there is 
/// it exits; if not, it establishes a hold on the database and waits until the 
/// user presses any keyboard key. When a key is pressed the program releases 
/// the hold and exits.
/// 
/// The command-line usage is:
/// \image html "Usage - Hold.PNG" ""
/// </remarks>
namespace Hold
{
    /// <summary>
    /// This class provides the Main() method for the MICS program Hold.exe
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class Hold
    {

        /// <summary>
        /// This is the Main() method for the MICS Hold program.
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            string context = "";

            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLogs\Hold.log";
                if (Log2.SetLogFilePath(mLog2FilePath))
                {
                    Log2.Erase();
                    Log2.Set(Log2.FileOpenClose.PER_SESSION);
                    Log2.Set(Log2.WriteMode.ENABLED);
                    Log2.Set(Log2.Level.VERBOSE);
                    Info.BuildMetaData = Info.CollateExeMetaData();
                    Log2.v("\nBuild: " + Info.BuildMetaData);
                }
                else
                {
                    Console.Error.Write("\r\nERROR: could not open Log2 file: " + mLog2FilePath);
                }
#endif
                string mutexName;
                MutexSecurity pSA;

                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                mutexName = Qutils.MakeName(Info.DbName, "HOLD");

                //...Log2.v("\nHold.Main(): mutexName = " + mutexName);

                // The value of Info.MicsUserName must be set prior to the call to 
                // GenUtil.SAEverybody(); get the user's MICS ID from the environment.
                Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
                if (String.IsNullOrWhiteSpace(Info.MicsUserName))
                {
                    Log2.e("\nHold.Main():: ERROR: Windows environment variable MicsUser is not set.");
                    context = "\r\nERROR: Windows environment variable MICSUSER is not set.\r\n";
                    Console.Error.Write(context);
                    Application.ExitQuietly(Error.ENVVARMICSUSERNOTSET);
                }

                // Create a MUTEX security attributes object that allows access to all.
                pSA = GenUtil.SAEverybody();

                // Check the HOLD queue.

                // Attempt to instantiate a Mutex object with the name "Global\FCSAHOLD".
                Mutex mMutex;
                bool createdNew = false;

                // Just in case we get an exception thrown we need a good context message.
                context = "\r\nAnother program has already placed a HOLD on access to the database.\r\n\r\n";

                mMutex = new Mutex(false, mutexName, out createdNew, pSA);

                // Check for failure to instantiate the named Mutex.
                if (mMutex == null)
                {
                    // Could not create a new mutex, this must mean it already exists (bad).
                    Log2.e("\nHold.Main(): ERROR: failed to instantiate a Mutex object.");
                    context = "\r\nHold.Main(): ERROR: failed to instantiate a named Mutex object: ";
                    Console.Error.Write(context + mutexName + "\r\n");
                    Application.ExitQuietly(127);
                }

                //...Log2.v("\nHold.Main(): Successfully instantiated a Mutex object.");

                // Check if the HOLD is up.
                // Block the current thread until the current WaitHandle receives a signal.
                // Setting the Timeout to zero prevents the call to WaitOne() from blocking.
                // It tests the state of the wait handle and returns immediately.
                if (!mMutex.WaitOne(0))
                {
                    // The mutex is owned; we need to exit.
                    Log2.e("\nHold.Main(): ERROR: mMutex.WaitOne(0) returned false.");
                    mMutex.Close();
                    Console.Write("\r\nA hold is already taken on this database.\r\n");
                    Application.ExitQuietly(2);
                }

                //...Log2.v("\nHold.Main(): mMutex.WaitOne(0) returned true.");

                //	We have the HOLD; let the user know.
                Console.Write("\r\nDatabase {0} is held.  Press any key to release...", Info.DbName);

                // Wait for the user to press any key.
                Console.ReadKey();

                // Whenever a thread acquires a mutex (for example, by calling its WaitOne method), 
                // it must subsequently call ReleaseMutex to relinquish ownership of the mutex and 
                // unblock other threads that may be trying to gain ownership of the mutex. 
                mMutex.ReleaseMutex();

                // The Mutex class inherits from its superclass WaitHandle that has the method
                // Dispose(): this method releases all resources used by the current instance 
                // of Mutex.
                mMutex.Dispose();

                Console.Write("\r\nHold on {0} is released.\r\n", Info.DbName);

                Application.ExitQuietly(0);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nHold.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nHold.Main(): stack trace: \r\n\r\n" + e.StackTrace);
                Console.Error.Write(context);
                Console.Error.Write(e.Message);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n This program attempts to place a 'HOLD' on the database that will block other");
            Console.Write("\r\n programs from accessing it. It first checks that there is not already a hold on");
            Console.Write("\r\n the database. If there is it exits; if not, it establishes a hold on the database");
            Console.Write("\r\n and waits until the user presses any keyboard key. When a key is pressed the");
            Console.Write("\r\n program releases the hold and exits.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: Hold <dbName>");
            Console.Write("\r\n =====");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n      Hold fcsa");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. The environment variable MICSUSER *MUST* be set.");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the user-prescribed command-line arguments.
        /// </summary>
        /// <param name="args"></param>
        public static void ParseCommandLineArgs(string[] args)
        {
            if (args.Length != 1)
            {
                WriteUsageToConsole();
                Application.ExitQuietly(10);
            }


            Info.DbName = args[0];
        }




    }
}

