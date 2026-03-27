using _Configuration;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace _NewLib
{
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// Provides methods that implement the MICS 'logging' functionality. Read '2' as 'to'.
    /// This class supports the testing and debugging of MICS programs. It
    /// provides both 'conventional' and 'Android-style' logging capability. See remarks...
    /// </summary>
    /// <remarks>
    /// Conventional logging uses the //...Log2.v() method. The default writing mode is
    /// WriteMode.ENABLED. Calling Set(WriteMode.DISABLED) inhibits writing to the logfile.
    /// 
    /// Android-style logging is provided by the methods Log2.v(), Log2.d(), Log2.i(),
    /// Log2.w(), Log2.e() and Log2.v() methods corresponding to 6 distinct levels of reporting:
    /// 
    /// { VERBOSE, DEBUG, INFO, WARN, ERROR, NEVER }
    /// 
    /// with VERBATIM being the lowest reporting threshold and ERROR the highest.
    /// 
    /// Setting the reporting level to NEVER disables all Android-style reporting; this
    /// is useful for 'silently parking' a logging entry that might be needed later.
    /// 
    /// Setting the reporting level to VERBATIM also enables reporting for DEBUG, 
    /// INFO, WARN, and ERROR.
    /// 
    /// Setting the reporting level to INFO also enables reporting for WARN, and ERROR.
    /// 
    /// Calling Set() with parameters between Level.VERBOSE up to Level.ERROR (inclusive)
    /// automatically sets WriteMode.ENABLED. 
    /// 
    /// Calling Set(Level.NEVER) automatically sets WriteMode.DISABLED.
    /// 
    /// Calling Set(WriteMode.DISABLED) automatically sets Level.NEVER.
    /// </remarks>
    public static class Log2
    {
        private const string DEFAULT_LOGe_DIR_PATH = @"D:\perflogs";

        private const string SQL_ERROR_LOGGING_TABLE = @"hulme.MicsErrorRecords";

        private static bool mFileIsOpenable = false;
        private static WriteMode mWriteMode = WriteMode.ENABLED;
        private static FileOpenClose mFileOpenClose = FileOpenClose.PER_WRITE;
        private static Level mLevel = Level.INFO;
        private static string mLogFileName = "";
        private static int mIndent = 0;
        private const int mTab = 2;
        private static StreamWriter streamWriter = null;
        private static bool mPreviousODBCconnectionAttemptFailed = false;

        public enum WriteMode { ENABLED, DISABLED }

        public enum FileOpenClose { PER_SESSION, PER_WRITE }

        public enum Level { VERBOSE, DEBUG, INFO, WARN, ERROR, NEVER }

        /// <summary>
        /// Sets the Windows pathname of the log file to be written to. The method
        /// performs a preliminary write attempt to validate the pathname and returns true if 
        /// all is well and false if the pathname is invalid or inaccessible to the caller.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>true or false.</returns>
        public static bool SetLogFilePath(string fileName)
        {
            bool nRet = false;

            //Try to open a StreamWriter to the prescribed path.
            if (fileName.Length > 0)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(fileName, true);
                    sw.Close();
                    mLogFileName = fileName;
                    mFileIsOpenable = true;
                    nRet = true;
                }
                catch
                {
                    string str = "\nLog file could not be opened: " + fileName;
                    Console.WriteLine(str);
                }
            }

            return nRet;
        }

        /// <summary>
        /// Writes a string out to the logfile.
        /// </summary>
        /// <param name="str"> string to be written.</param>
        /// <returns>true or false.</returns>
        public static bool Write(string str)
        {
            bool nRet = false;

            if (mFileIsOpenable && (mWriteMode == WriteMode.ENABLED))
            {
                // Log should only use \n for line breaks; deal with any instances of \r\n.
                str = Regex.Replace(str, "\r", "");

                switch (mFileOpenClose)
                {
                    case FileOpenClose.PER_WRITE:
                        try
                        {
                            StreamWriter sw = File.AppendText(mLogFileName);
                            str = PrependIndent(str);
                            str = Regex.Replace(str, "\n", "\r\n");
                            sw.Write(str);
                            sw.Close(); //Implicit flush.

                            nRet = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            mFileIsOpenable = false;
                            Application.Exit("\n//...Log2.v(): ERROR: A: Exception caught: " + e.Message);
                        }
                        break;
                    case FileOpenClose.PER_SESSION:
                        try
                        {
                            str = PrependIndent(str);
                            str = Regex.Replace(str, "\n", "\r\n");
                            streamWriter.Write(str);
                            streamWriter.Flush();

                            nRet = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            mFileIsOpenable = false;
                            Application.Exit("\n//...Log2.v(): ERROR: B: Exception caught: " + e.Message);
                        }
                        break;
                }

            }
            return nRet;
        }

        /// <summary>
        /// Writes a string out to the logfile using the syntax and behavior of String.Format().
        /// </summary>
        /// <param name="firstArg"> - the first argument of a String.Format() method call.</param>
        /// <param name="args"> - the subsequent arguments of a String.Format() method call.</param>
        /// <returns>true or false.</returns>
        public static bool Write(string firstArg, params object[] args)
        {
            string payLoad = (args.Length == 0) ? firstArg : String.Format(firstArg, args);

            return Write(payLoad);
        }

        /// <summary>
        /// Sets the WriteMode attribute to be one of ENABLED or DISABLED. The logfile
        /// is only written to if the writemode is ENABLED.
        /// </summary>
        /// <param name="writeMode"></param>
        public static void Set(WriteMode writeMode)
        {
            mWriteMode = writeMode;
            if (mWriteMode == WriteMode.DISABLED)
            {
                mLevel = Level.NEVER;
            }
        }

        /// <summary>
        /// Sets the FileOpenClose attribute to be one of PER_SESSION or PER_WRITE.
        /// </summary>
        /// <remarks>
        /// PER_SESSION means that the logfile is openned (once) at the beginning of a logging session, call Write()
        /// multiple times and then the logfile closed (once) at the end of a logging session.
        /// 
        /// PER_WRITE means that the logfile is opened before performing an individual
        /// Write() operations and then is immediately closed.
        /// </remarks>
        /// <param name="fileOpenClose"></param>
        public static void Set(FileOpenClose fileOpenClose)
        {
            mFileOpenClose = fileOpenClose;

            if (mFileOpenClose == FileOpenClose.PER_SESSION)
            {
                try
                {
                    streamWriter = File.AppendText(mLogFileName);
                    mFileIsOpenable = true;
                }
                catch (Exception e)
                {
                    mFileIsOpenable = false;
                    Console.WriteLine("Log2.Set(): Error: " + e.Message);
                    Application.Exit("Log2.Set(): Exception caught: streamWriter = File.AppendText(mLogFileName); ");
                }
            }
        }

        /// <summary>
        /// This method either enables of disables writing of text
        /// to the log file. The default is enabled. This method is useful
        /// for temporarily halting the logging functionality and then
        /// resuming logging later on.
        /// </summary>
        /// <param name="level"></param>
        public static void Set(Level level)
        {
            mLevel = level;
            if (mLevel < Level.NEVER)
            {
                mWriteMode = WriteMode.ENABLED;
            }
            else
            {
                mWriteMode = WriteMode.DISABLED;
            }
        }

        /// <summary>
        /// Delete the file whose path was given in the last call to SetLogFilePath().
        /// </summary>
        public static void Erase()
        {
            if (mFileIsOpenable)
            {
                File.Delete(mLogFileName);
            }
        }

        /// <summary>
        /// Returns the full pathname of the current logfile.
        /// </summary>
        /// <returns>full pathname</returns>
        public static string GetLogFilePath()
        {
            return mLogFileName;
        }

        /// <summary>
        /// This method prepends spaces at the front of a string. The number of spaces
        /// is determined by how many times the keywords 'entry' and 'exit' have appeared
        /// in Write() strings so far. The general effect is to provide automatic indenting
        /// of message strings written to the logfile.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string PrependIndent(string str)
        {
            string result = "";

            if (str != null)
            {
                if (str.ToLower().Contains("entry"))
                {
                    mIndent += mTab;
                    if (mIndent < 0) mIndent = 0;
                }

                string indent = new String(' ', mIndent);
                if (str.StartsWith("\n\n"))
                {
                    result = "\n\n" + indent + str.Substring(2);
                }
                else if (str.StartsWith("\n"))
                {
                    result = "\n" + indent + str.Substring(1);
                }
                else
                {
                    result = indent + str;
                }

                if (str.ToLower().Contains("exit"))
                {
                    mIndent -= mTab;
                    if (mIndent < 0) mIndent = 0;
                }
            }  // if (str != null)



            return result;
        }

        /// <summary>
        /// Closes the output stream to the logfile.
        /// </summary>
        public static void Close()
        {
            if (mFileOpenClose == FileOpenClose.PER_SESSION)
            {
                try
                {
                    streamWriter.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Log2.Close(): Error: " + e.Message);
                    Application.Exit("Log2.Close(): Exception caught: streamWriter = streamWriter.Close(); ");
                }
            }
        }

        /// <summary>
        /// Writes a prescribed string to file if the reporting threshold is set to
        /// Level.VERBOSE.
        /// </summary>
        /// <param name="str"> - prescribed string.</param>
        public static void v(string str)
        {
            if (mLevel <= Level.VERBOSE)
            {
                Write(str + "    (Log2.v)");
            }
        }

        /// <summary>
        /// Writes a prescribed string to file if the reporting threshold is set to
        /// Level.VERBOSE using the syntax and behavior of String.Format().
        /// </summary>
        /// <param name="firstArg"> - the first argument of a String.Format() method call.</param>
        /// <param name="args"> - the subsequent arguments of a String.Format() method call.</param>
        public static void v(string firstArg, params object[] args)
        {
            string payLoad = (args.Length == 0) ? firstArg : String.Format(firstArg, args);

            if (mLevel <= Level.VERBOSE)
            {
                Write(payLoad + "    (Log2.v)");
            }
        }

        /// <summary>
        /// Writes a prescribed string to file if the reporting threshold is set to
        /// Level.DEBUG or below.
        /// </summary>
        /// <param name="str"> - prescribed string.</param>
        public static void d(string str)
        {
            if (mLevel <= Level.DEBUG)
            {
                Write(str + "    (Log2.d)");
            }
        }

        /// <summary>
        /// Writes a prescribed string to file if the reporting threshold is set to
        /// Level.DEBUG or below using the syntax and behavior of String.Format().
        /// </summary>
        /// <param name="firstArg"> - the first argument of a String.Format() method call.</param>
        /// <param name="args"> - the subsequent arguments of a String.Format() method call.</param>
        public static void d(string firstArg, params object[] args)
        {
            string payLoad = (args.Length == 0) ? firstArg : String.Format(firstArg, args);

            if (mLevel <= Level.DEBUG)
            {
                Write(payLoad + "    (Log2.d)");
            }
        }

        /// <summary>
        /// Writes a prescribed string to file if the reporting threshold is set to
        /// Level.INFO or below.
        /// </summary>
        /// <param name="str"> - prescribed string.</param>
        public static void i(string str)
        {
            if (mLevel <= Level.INFO)
            {
                Write(str + "    (Log2.i)");
            }
        }

        /// <summary>
        /// Writes a prescribed string to file if the reporting threshold is set to
        /// Level.INFO using the syntax and behavior of String.Format().
        /// </summary>
        /// <param name="firstArg"> - the first argument of a String.Format() method call.</param>
        /// <param name="args"> - the subsequent arguments of a String.Format() method call.</param>
        public static void i(string firstArg, params object[] args)
        {
            string payLoad = (args.Length == 0) ? firstArg : String.Format(firstArg, args);

            if (mLevel <= Level.INFO)
            {
                Write(payLoad + "    (Log2.i)");
            }
        }

        /// <summary>
        /// Writes a prescribed string to file if the reporting threshold is set to
        /// Level.WARN or below.
        /// </summary>
        /// <param name="str"> - prescribed string.</param>
        public static void w(string str)
        {
            if (mLevel <= Level.WARN)
            {
                Write(str + "    (Log2.w)");
            }
        }

        /// <summary>
        /// Writes a prescribed string to file if the reporting threshold is set to
        /// Level.WARN using the syntax and behavior of String.Format().
        /// </summary>
        /// <param name="firstArg"> - the first argument of a String.Format() method call.</param>
        /// <param name="args"> - the subsequent arguments of a String.Format() method call.</param>
        public static void w(string firstArg, params object[] args)
        {
            string payLoad = (args.Length == 0) ? firstArg : String.Format(firstArg, args);

            if (mLevel <= Level.WARN)
            {
                Write(payLoad + "    (Log2.w)");
            }
        }

        /// <summary>
        /// Writes a prescribed string to file if the reporting threshold is set to
        /// Level.ERROR or lower; if logging was not previously enabled then it is 
        /// automatically enabled by this method so that serious errors always get
        /// written to a log file.
        /// </summary>
        /// <param name="str"> - prescribed string.</param>
        public static void e(string str)
        {
#if true
            // If logging is not already enabled then turn it on and provide the
            // desired default log file path to write to.
            if (mLogFileName == "")
            {
                string thisProgramName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
                string eLogFilePath = DEFAULT_LOGe_DIR_PATH + "\\" + thisProgramName + ".log";

                if (Log2.SetLogFilePath(eLogFilePath))
                {
                    Log2.Erase();
                    Log2.Set(Log2.FileOpenClose.PER_SESSION);
                    Log2.Set(Log2.WriteMode.ENABLED);
                    Log2.Set(Log2.Level.ERROR);

                    Log2.v("\nLogging activated by Log2.e() call: {0}\n", DateTime.Now.ToString());
                }
                else
                {
                    Console.Error.Write("\nERROR: could not open Log2 file: " + eLogFilePath);
                }
            }
#endif
            // Write the prescribed string into the log file.
            if (mLevel <= Level.ERROR)
            {
                Write(str + "    (Log2.e)");
            }

        }

        /// <summary>
        /// Writes a prescribed string to file if the reporting threshold is set to
        /// Level.ERROR or lower using the syntax and behavior of String.Format(); 
        /// if logging was not previously enabled then it is automatically enabled by 
        /// this method so that serious errors always get written to a log file.
        /// </summary>
        /// <param name="firstArg"> - the first argument of a String.Format() method call.</param>
        /// <param name="args"> - the subsequent arguments of a String.Format() method call.</param>
        public static void e(string firstArg, params object[] args)
        {
            string payLoad = (args.Length == 0) ? firstArg : String.Format(firstArg, args);

            if (mLevel <= Level.ERROR)
            {
                e(payLoad);
            }
        }

        /// <summary>
        /// Writes nothing.
        /// Provided for completeness and consistency.
        /// It is also useful for 'parking' a Log write that might be needed later.
        /// </summary>
        /// <param name="str"> - prescribed string.</param>
        public static void n(string str)
        {
        }

        /// <summary>
        /// Writes nothing.
        /// Provided for completeness and consistency.
        /// It is also useful for 'parking' a Log write that might be needed later.
        /// </summary>
        /// <param name="firstArg"> - the first argument of a String.Format() method call.</param>
        /// <param name="args"> - the subsequent arguments of a String.Format() method call.</param>
        public static void n(string firstArg, params object[] args)
        {
        }

        /// <summary>
        /// This method executes an SQLExecDirect() query independent of Ssutil in _Utillib.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static int DoSQLExecDirect(string query)
        {
            // Local variables 
            SQLRETURN sqlRet = 0;

            try
            {
                // Begin transaction for updates; get an ODBC connection and statement handles.
                SQLHDBC hConn = Info.StaticConnHandle;
                SQLHANDLE hStmt;
                ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

                // Attempt to execute the SQL query (or queries).
                sqlRet = ODBC.SQLExecDirect(hStmt, query, query.Length);

                if (!ODBC.IsOKorNoData(sqlRet))
                {
                    Console.Error.Write("\nLog2.DoSQLExecDirect(): ERROR: call to ODBC.SQLExecDirect() failed.");
                    return Error.ODBC_EXECDIRECT_FAILED;
                }
            }
            catch (Exception e)
            {
                Console.Error.Write("\nLog2.DoSQLExecDirect(): ERROR: exception caught:\n{0}\n{1}", e.Message, e.StackTrace);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            return Constant.SUCCESS;
        }

        private static void InsertRecordIntoErrorLoggingTable(string errMsg)
        {
            // If the application has not already established a connection with the database
            // then we need to connect in order to write into the SQL error logging table.
            if (Info.StaticConnHandle == SQLHDBC.Zero && !mPreviousODBCconnectionAttemptFailed)
            {
                // Attempt connection to the prescribed database.
                int rc = ODBCconnect.UtConnect(Info.DbName);
                if (rc != 0)
                {
                    // Give up.
                    mPreviousODBCconnectionAttemptFailed = true;
                    Console.Error.Write("\nLog2.InsertRecordIntoErrorLoggingTable(): ERROR: call to UtConnect() failed.");
                    return;
                }
            }

            // If the calling application is currently connected to the database then insert
            // an error record into the appropriate error logging table. Only do this if
            // a logging was automatically activated by a previous call to Log2.e().
            // Also, we need to 'double up' (escape) any single quote marks that exist in the string
            // destined for the ErrorMessage column in the logging table.
            bool loggingWasAutoActivated = Path.GetDirectoryName(mLogFileName).Equals(DEFAULT_LOGe_DIR_PATH);
            bool appHasDatabaseConnection = Info.StaticConnHandle != SQLHDBC.Zero;

            if (loggingWasAutoActivated && appHasDatabaseConnection)
            {
                string query = String.Format("INSERT INTO {0} VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
                                                SQL_ERROR_LOGGING_TABLE,
                                                DateTime.Now.ToString("O"),
                                                "MICS# console program",
                                                System.Diagnostics.Process.GetCurrentProcess().ProcessName,
                                                System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                                                errMsg.Replace("'", "''"),
                                                "N/A"
                                                );

                int retVal = DoSQLExecDirect(query);

                if (retVal != Constant.SUCCESS)
                {
                    //...Console.Error.Write("\nLog2.InsertRecordIntoErrorLoggingTable(): ERROR: call to DoSQLExecDirect() failed.");
                }
            }

            return;
        }








    }
}
