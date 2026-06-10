# Documented File: QuickLog.cs
**Repository Path:** `_NewLib\QuickLog.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
﻿using _Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _NewLib
{
    public class QuickLog
    {
        private const string QUICK_LOG_FILE_MUTEX_NAME = "QUICK_LOG_FILE_MUTEX_NAME";
        private const string DEFAULT_QUICKLOG_FILE_PATH = @"d:\prod\files\QuickLog.txt";

        private static string mQuickLogFilePath = DEFAULT_QUICKLOG_FILE_PATH;
        private static bool mCanWriteQuickToLogFile = false;

        /// <summary>
        /// This method initializes access to the (single) QUICK log file that can
        /// be written to by multiple applications running concurrently; the file path
        /// is initially set to a default value of "d:\\prod\\files\\QuickLog.txt".
        /// </summary>
        /// <returns></returns>
        public static int Initialize()
        {
            int retVal = Constant.FAILURE;

            //...Log2.v("\nQuickLog.Initialize(): mQuickLogFilePath = " + mQuickLogFilePath);

            // Test that we can access the log using the mutex.
            using (var mutex = new Mutex(false, QUICK_LOG_FILE_MUTEX_NAME))
            {
                TextWriter twQuickLog;

                // Attempt to open the Quick log file for write/append.
                try
                {
                    mutex.WaitOne();

                    twQuickLog = new StreamWriter(mQuickLogFilePath, true);  // Append mode.
                    twQuickLog.Close();

                    mutex.ReleaseMutex();

                    mCanWriteQuickToLogFile = true;

                    retVal = Constant.SUCCESS;

                    //...Log2.v("\nQuickLog.Initialize(): log file is accessible.");
                }
                catch (Exception)
                {
                    string str = String.Format("Can't access quick log file: {0}\n", mQuickLogFilePath);
                    Console.Write("\n" + str);
                    Log2.e("\nQuickLog.Initialize(): ERROR: " + str);
                    twQuickLog = Console.Out;
                }

            }

            return retVal;
        }

        /// <summary>
        /// This method initializes access to the (single) QUICK log file that can
        /// be written to by multiple applications running concurrently; the file path
        /// is prescribed as a call parameter..
        /// </summary>
        /// <returns></returns>
        public static int Initialize(string quickLogFilePath)
        {
            mQuickLogFilePath = quickLogFilePath;

            return Initialize();
        }

        /// <summary>
        /// This method writes a message to the QuickLog file; because 
        /// multiple instances of QuickInitiator may want to write to the 
        /// same QUICK log file, a Mutex is used to queue for sole access
        /// to the log file.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static int Write(string message)
        {
            int retVal = Constant.FAILURE;

            if (!mCanWriteQuickToLogFile)
            {
                Log2.e("\nQuickLog.Write(): ERROR: QuickLog.Initialize() has not been called, or failed.");
                return retVal;
            }

            if (String.IsNullOrEmpty(message))
            {
                return retVal;
            }

            using (var mutex = new Mutex(false, QUICK_LOG_FILE_MUTEX_NAME))
            {
                mutex.WaitOne();
                File.AppendAllText(mQuickLogFilePath, message);
                mutex.ReleaseMutex();
            }

            return retVal;
        }

        /// <summary>
        /// This method writes a message to the QuickLog file; because 
        /// multiple instances of QuickInitiator may want to write to the 
        /// same QUICK log file, a Mutex is used to queue for sole access
        /// to the log file; the call arguments mimic those of String.Format()
        /// </summary>
        /// <param name="firstArg"> - the first argument of a String.Format() method call.</param>
        /// <param name="args"> - the subsequent arguments of a String.Format() method call.</param>
        /// <returns></returns>
        public static int Write(string firstArg, params object[] args)
        {
            int retVal = Constant.FAILURE;

            if (!mCanWriteQuickToLogFile)
            {
                Log2.e("\nQuickLog.Write(): ERROR: QuickLog.Initialize() has not been called, or failed.");
                return retVal;
            }

            string payLoad = (args.Length == 0) ? firstArg : String.Format(firstArg, args);

            return Write(payLoad);
        }



    }
}

```
