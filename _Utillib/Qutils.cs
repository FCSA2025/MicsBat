using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _Utillib
{
    /// <summary>
    /// This class provides methods that manage MICS program READ and WRITE mode access 
    /// to a FCSA database with a queuing algorithm implemented using Mutex synchronization 
    /// objects.
    /// </summary>
    public class Qutils
    {
        //--------------------------------------------------------------------------------------

        // The WRITE mutex is static because it is instantiated in the method EnterQueue()
        // and closed by the method ExitQueue().
        private static Mutex mWriteMutex;

        // This READ mode marker file path is static because it is instantiated in the method 
        // EnterQueue() and used by the method ExplainQueue().
        private static string mThisProcessMarkerFilePath = null;

        // The maximum wait duration variable is static because it is instantiated in the method 
        // EnterQueue() and used by the method ExplainQueue().
        private static int mWriteQueueWaitTimeSeconds = 0;

        // This READ mode marker file directory path is static because it is instantiated by
        // EnterQueue() and used by the methods NoReadModeMarkerFiles() and ExplainQueue().
        private static string mReadAccessMarkerFilesDir = "";

        //--------------------------------------------------------------------------------------

        /// <summary>
        /// This method provides entry into a queue to obtain either READ or WRITE mode
        /// access to the FCSA database.
        /// </summary>
        /// <param name="dBase"> name of the DB to be accessed.</param>
        /// <param name="cQName"> the type of DB access mode that is requested, i.e. "READ" or "WRITE".</param>
        /// <param name="writeQueueWaitTimeSeconds"> the maximum time to be spent waiting in the queue. </param>
        /// <returns></returns>
        public static int EnterQueue(string dBase, string cQName, int writeQueueWaitTimeSeconds)
        {
            //...Log2.v(String.Format("\nQutils.EnterQueue(): Entry: dBase = {0}, cQName = {1}, writeQueueWaitTimeSeconds = {2}", dBase, cQName, writeQueueWaitTimeSeconds));

            // The move to the AWS Cloud Server caused the run-time performance of all the MICS#
            // programs to be slowed down relative to KOZA, typically by a factor of 3 to 5.
            // Consequently, the wait times specified in the MICS# programs that call EnterQueue()
            // may no longer be sufficient. For example, SdUpdateOper has a queue wait time of only 5 seconds.
            // It is too big an impact on the software configuration to change all of the MICS# main programs
            // that call EnterQueue() so the following line is an expedient fix that sets the minimum wait
            // time to 30 seconds.
            if (writeQueueWaitTimeSeconds < 30) writeQueueWaitTimeSeconds = 30;

            // Save the requested wait time for the WRITE queue as a static variable
            // so that it can be used later in ExplainQueue();
            mWriteQueueWaitTimeSeconds = writeQueueWaitTimeSeconds;

            // We will be using two named Mutexes, one for HOLD and one for WRITE.
            // The HOLD mutex is used as the top-level gatekeeper for access to the DB.
            // The FCSA admin can establish a lock on the HOLD mutex and this will block 
            // all MICS programs from performing read/write operations on DB tables.
            // In the usual case in which the admin does not have the lock on the HOLD 
            // mutex then the caller of this method can check/use the WRITE mutex to
            // establish a READ or WRITE lock on the DB access.
            // Note that the 'locks' mentioned above are *advisory* locks, where each 
            // thread cooperates by acquiring the lock before exclusively accessing 
            // the corresponding data resource that is being protected.
            string holdMutexName = MakeName(dBase, "HOLD");
            string writeMutexName = MakeName(dBase, "WRITE");

            Mutex holdMutex;

            // We first attempt to establish a lock on the HOLD mutex.
            // If this is successful we know that the FCSA admin is not currently 
            // blocking all MICS program access to the DB and that we can proceed 
            // to check/use the WRITE mutex.
            int retVal = GetLockOnNamedMutex(holdMutexName, 0, out holdMutex);

            if (retVal != Constant.SUCCESS)
            {
                // The attempt to get a lock on the HOLD mutex failed.
                if (holdMutex != null) holdMutex.Close();
                Log2.e("\nQutils.EnterQueue(): ERROR: attempt to establish lock on name Mutex " + holdMutexName + " FAILED.");
                //...Log2.v("\nQutils.EnterQueue(): Exit: returned 100");
                return 100;
            }

            //...Log2.v("\nQutils.EnterQueue(): successfully establish lock on name Mutex: " + holdMutexName);

            // We successfully got the HOLD lock, meaning that the FCSA admin is NOT
            // currently blocking all MICS access to the DB.
            // We can release the HOLD lock and close the HOLD mutex.
            holdMutex.ReleaseMutex();
            holdMutex.Close();
            //...Log2.v("\nQutils.EnterQueue(): D: holdMutex.ReleaseMutex() SUCCEEDED for " + holdMutexName);
            //...Log2.v("\nQutils.EnterQueue(): D: holdMutex.CLOSE()        SUCCEEDED for " + holdMutexName);

            // Try to establish a lock on the named Mutex Global\FCSAWRITE.
            // First try GetLockOnNamedMutex() with a time-out of zero seconds; if this
            // succeeds then proceed to service the Read or Write mode access request.
            // If GetLockOnNamedMutex(0) failed then we will try again with a longer wait time.
            retVal = GetLockOnNamedMutex(writeMutexName, 0, out mWriteMutex);

            if (retVal != Constant.SUCCESS)
            {
                Console.Write("\n       Another process has WRITE access to the DB: will queue for a maximum of {0} seconds ...\n", writeQueueWaitTimeSeconds);

                retVal = GetLockOnNamedMutex(writeMutexName, writeQueueWaitTimeSeconds, out mWriteMutex);

                if (retVal != Constant.SUCCESS)
                {
                    // The attempt to get a lock on the WRITE mutex failed.
                    if (mWriteMutex != null) mWriteMutex.Close();
                    Log2.e("\nQutils.EnterQueue(): ERROR: attempt to establish lock on name Mutex " + writeMutexName + " FAILED.");
                    //...Log2.v("\nQutils.EnterQueue(): Exit: returned 110");
                    return 110;
                }
            }

            //...Log2.v("\nQutils.EnterQueue(): successfully establish lock on name Mutex: " + writeMutexName);
            //...Log2.v("\nQutils.EnterQueue(): the caller is requesting DB access mode: " + cQName);

            // We will need to set the correct value for cDir because this is used by
            // both the Read and Write mode service code.
            // Also, check that this directory path exists.
            mReadAccessMarkerFilesDir = Ssutil.GetMicsRoot(dBase);
            mReadAccessMarkerFilesDir += "files\\read";

            if (!Directory.Exists(mReadAccessMarkerFilesDir))
            {
                Log2.e("\nQutils.EnterQueue(): ERROR: call to Directory.Exists() returned FALSE for path: " + mReadAccessMarkerFilesDir);
                //...Log2.v("\nQutils.EnterQueue(): Exit: returned 120");
                return 120;
            }

            // We have successfully got the lock on the WRITE mutex.
            // Now service the caller's request for Read or Write mode DB access.
            // If the caller is requesting Write mode DB access (i.e. cQName = "WRITE"), we need to check that 
            // no Reads are currently being performed by other processes.
            // If the caller is requesting Read mode DB access, then we can grant this right away.
            if (cQName.Equals("WRITE"))
            {
                // We already have a hold on the WRITE Mutex. 
                // The caller requests WRITE mode access to the database, so we first check 
                // whether any other currently executing processes own Read mode access to the DB.
                // If there any such processes then they will have created Read mode marker files
                // in directory prod\files\read.
                // If any Read mode access marker files currently exist we will 'poll' the situation
                // every 5 seconds until either there are no Read marker files left or we time-out.
                int errorCode;
                int blockingProcessID = -1;
                bool continuePolling = true;
                DateTime startTime = DateTime.Now;
                double elapsedSeconds = 0.0;

                Console.Write("\n");

                while (continuePolling)
                {
                    if (NoReadModeMarkerFiles(out errorCode, out blockingProcessID))
                    {
                        if (errorCode == Constant.SUCCESS)
                        {
                            // There are no current processes that own Read marker files in prod\files\read.
                            // We can return to caller with the WRITE mutex locked by this program.
                            //...Log2.v("\nQutils.EnterQueue(): there are currently no Read mode access marker files.");
                            //...Log2.v("\nQutils.EnterQueue(): Exit: returned Constant.SUCCESS");
                            return Constant.SUCCESS;
                        }
                        else
                        {
                            // Something bad happened.
                            // We need to release the lock on the WRITE Mutex and close the Mutex.
                            mWriteMutex.ReleaseMutex();
                            mWriteMutex.Close();

                            Log2.e("\nQutils.EnterQueue(): ERROR: call to NoReadModeMarkerFiles() FAILED, returned errorCode = " + errorCode);
                            //...Log2.v("\nQutils.EnterQueue(): B: mWriteMutex.ReleaseMutex() SUCCEEDED for " + writeMutexName);
                            //...Log2.v("\nQutils.EnterQueue(): B: mWriteMutex.CLOSE()        SUCCEEDED for " + writeMutexName);

                            //...Log2.v("\nQutils.EnterQueue(): Exit: returned " + errorCode);
                            return errorCode;
                        }
                    }

                    // Currently, there are Read mode access marker files in prod\files\read.
                    // First, check whether our polling has timed-out.
                    elapsedSeconds = DateTime.Now.Subtract(startTime).TotalSeconds;

                    Console.Write("\r       Another process has READ access to the DB: will queue for a maximum of {0} seconds ... {1}", writeQueueWaitTimeSeconds, Math.Truncate(elapsedSeconds));

                    if (elapsedSeconds > mWriteQueueWaitTimeSeconds) break;

                    // If we have not timed-out then Wait 5 seconds and try again.
                    Thread.Sleep(5000);
                }

                // If we reach here then we must have timed-out.
                // Release and close the WRITE mutex and return with an error code.
                mWriteMutex.ReleaseMutex();
                mWriteMutex.Close();

                Log2.e("\nQutils.EnterQueue(): ERROR: found a current process that has a READ hold on the DB. Process ID = " + blockingProcessID);
                //...Log2.v("\nQutils.EnterQueue(): B: mWriteMutex.ReleaseMutex() SUCCEEDED for " + writeMutexName);
                //...Log2.v("\nQutils.EnterQueue(): B: mWriteMutex.CLOSE()        SUCCEEDED for " + writeMutexName);
                //...Log2.v("\nQutils.EnterQueue(): Exit: returned 140");
                return 140;   //	Indicate that we can go no further.

            }
            else // Read mode DB access.
            {
                // A cQName that is not "WRITE" is treated as a request for Read mode access.

                // We already have a hold on the write mutex and the caller requests READ mode access.  
                // So while we hold this, we write a file to the read directory with our process name as the file name.
                Process currentProcess = Process.GetCurrentProcess();

                mThisProcessMarkerFilePath = String.Format("{0}\\{1}", mReadAccessMarkerFilesDir, currentProcess.Id);

                //...Log2.v("\r\nQutils.EnterQueue(): mThisProcessMarkerFilePath = " + mThisProcessMarkerFilePath);

                try
                {
                    // Create the process marker file then immediately close it.
                    FileStream fileStream = File.Create(mThisProcessMarkerFilePath);
                    fileStream.Close();

                    //...Log2.v("\nQutils.EnterQueue(): File.Create() SUCCEEDED for: " + mThisProcessMarkerFilePath);
                }
                catch
                {
                    GenUtil.SetErrW("enterqueue: Could not open file %s for writing.", mThisProcessMarkerFilePath);
                    mWriteMutex.ReleaseMutex();
                    mWriteMutex.Close();

                    Log2.e("\nQutils.EnterQueue(): ERROR: could not create file: " + mThisProcessMarkerFilePath);
                    //...Log2.v("\nQutils.EnterQueue(): C: mWriteMutex.ReleaseMutex() SUCCEEDED for " + writeMutexName);
                    //...Log2.v("\nQutils.EnterQueue(): C: mWriteMutex.CLOSE()        SUCCEEDED for " + writeMutexName);
                    //...Log2.v("\nQutils.EnterQueue(): Exit: returned 150");
                    return 150;
                }

                // Because the caller only requested Read mode access we can now release the 
                // lock on the WRITE mutex and close it.
                mWriteMutex.ReleaseMutex();
                mWriteMutex.Close();
                //...Log2.v("\nQutils.EnterQueue(): D: mWriteMutex.ReleaseMutex() SUCCEEDED for " + writeMutexName);
                //...Log2.v("\nQutils.EnterQueue(): D: mWriteMutex.CLOSE()        SUCCEEDED for " + writeMutexName);
            }

            //...Log2.v("\nQutils.EnterQueue(): Exit: retVal = " + retVal);
            return retVal;
        }

        /// <summary>
        /// This method manages the calling program's release of its READ or WRITE mode DB access. 
        /// </summary>
        /// <param name="dBase"> - database name (e.g. "fcsa")</param>
        /// <param name="cQName"> - either "READ" or "WRITE".</param>
        /// <returns> - always return Constant.SUCCESS.</returns>
        public static int ExitQueue(string dBase, string cQName)
        {
            //...Log2.v(String.Format("\nQutils.ExitQueue(): Entry: dBase = {0}, cQName = {1}", dBase, cQName));

            if (cQName.Equals("WRITE"))
            {
                if (mWriteMutex == null)
                {
                    // If we are in Write mode access then mWriteMutex should be
                    // a bona fide instance of a Mutex; if it is NULL then we have a 
                    // logical inconsistency.
                    Log2.e("\nQutils.ExitQueue(): ERROR: mWriteMutex is NULL.");
                    return Constant.FAILURE;
                }
                else
                {
                    mWriteMutex.ReleaseMutex();
                    mWriteMutex.Close();
                    //...Log2.v("\nQutils.ExitQueue(): mWriteMutex.ReleaseMutex() SUCCEEDED.");
                    //...Log2.v("\nQutils.ExitQueue(): mWriteMutex.CLOSE()        SUCCEEDED.");
                }
            }
            else
            {
                // Any cQName not equal to "WRITE" is treated as a "READ".
                // Attempt to delete the DB Read mode access marker file.
                try
                {
                    File.Delete(mThisProcessMarkerFilePath);

                    //...Log2.v("\nQutils.ExitQueue(): call to File.Delete() SUCCEEDED for file path: " + mThisProcessMarkerFilePath);

                    mThisProcessMarkerFilePath = null;
                }
                catch (Exception e)
                {
                    Log2.e("\nQuutils.ExitQueue(): ERROR: call to File.Delete() threw Exception: " + e.Message);
                    return Constant.FAILURE;
                }
            }

            //...Log2.v(String.Format("\nQutils.ExitQueue(): Exit: returned 0"));
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method writes an error message to a to a TextWriter object that describes
        /// why a previous call to EnterQueue() has failed..
        /// </summary>
        /// <param name="dbase"> - name of the database.</param>
        /// <param name="cQName"> - name of the queue: "READ" or "WRITE".</param>
        /// <param name="nErr"> - error code. as returned by EnterQueue().</param>
        /// <param name="fOut"> - a prescribed TextWriter object.</param>
        public static void ExplainQueue(string dbase, string cQName, int nErr, TextWriter fOut)
        {
            TextWriter tw = fOut;

            if (fOut == null)
            {
                tw = Console.Out;
            }

            switch (nErr)
            {
                case 0: // There were no problems.
                    tw.Write("\r\nProgram entered the {0} queue on {1} without problem.\r\n", cQName, dbase);
                    break;
                case 30: // The named mutex already exists and another process has the lock on it (timed out).
                    tw.Write("\r\nERROR: another process has the lock on the {0} Mutex for DB {1}. This program waited and timed-out.\r\n", cQName, dbase);
                    break;
                case 100: // The FCSA admin currently has a lock on the HOLD mutex.
                    tw.Write("\r\nERROR: another process has an administrator's HOLD lock on MICS program access to DB {0}.\r\n", dbase);
                    break;
                case 110: // Failed to get a lock on the WRITE mutex.
                    tw.Write("\r\nERROR: another process has WRITE access to the DB: the wait TIMED-OUT after {0} seconds.\r\n", mWriteQueueWaitTimeSeconds);
                    break;
                case 120: // The Read access mode marker file directory does not exist.
                    tw.Write("\r\nERROR: The Read access mode marker file directory {0} does not exist.\r\n", mReadAccessMarkerFilesDir);
                    break;
                case 130: // Failed to get a list of currently executing process from Windows System.
                    tw.Write("\r\nERROR: attempt to get a list of currently executing processes FAILED.");
                    break;
                case 140: // Failed to get a list of currently executing process from Windows System.
                    tw.Write("\r\nERROR: Another process has READ access to the DB: the wait TIMED-OUT after {0} seconds.\r\n", mWriteQueueWaitTimeSeconds);
                    break;
                case 150: // Attempt to create Read mode access marker file failed.
                    tw.Write("\r\nERROR: attempt to create Read mode access file FAILED for path: {0}.", mThisProcessMarkerFilePath);
                    break;
                default:
                    tw.Write("\r\nERROR: ({0}) when attempting to queue for database access:\r\n{1}\r\n", nErr, GenUtil.GetUserMess());
                    break;
            }

            return;
        }

        /// <summary>
        /// This method returns a string providing the fully-qualified name for a 
        /// globally-accessible Mutex object.
        /// </summary>
        /// <param name="dbase"> - name of database (e.g. "fcsa").</param>
        /// <param name="cNameMod"> - the name modifier.</param>
        /// <returns> - string containing the name of the mutex.</returns>
        public static string MakeName(string dbase, string cNameMod)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Global\\");
            sb.Append(dbase.ToUpper());
            sb.Append(cNameMod);

            return sb.ToString();
        }

        /// <summary>
        /// This method attempts to obtain the exclusive lock on a prescribed named-Mutex object; 
        /// if this is not immediately available the method will queue with a prescribed maximum
        /// wait duration.
        /// </summary>
        /// <param name="mutexName"></param>
        /// <param name="nWaitSecs"></param>
        /// <param name="namedMutex"></param>
        /// <returns></returns>
        private static int GetLockOnNamedMutex(string mutexName, int nWaitSecs, out Mutex namedMutex)
        {
            // 'out' requirement.
            namedMutex = null;

            //...Log2.v(String.Format("\nQutils.GetLockOnNamedMutex(): Entry: nWaitSecs = {0}", nWaitSecs));

            // We begin by assuming that the named Mutex already exists (i.e. it was created previously 
            // by another program) and attempting to join it.

            bool namedMutexDoesNotExist = false;
            bool holdMutexWasCreated = false;

            int errorCode = Constant.SUCCESS;

            // The class MutexSecurity encapsulates the Windows access control security for a named mutex.
            // Create a MutexSecurity object that we can use for both the HOLD and WRITE mutexes.
            MutexSecurity mutexSecurity = GenUtil.SAEverybody();

            //=========== Attempt to join an already existing Mutex with the prescribed name. ===============

            try
            {
                namedMutex = Mutex.OpenExisting(mutexName);
                //...Log2.v("\nQutils.GetLockOnNamedMutex(): Mutex.OpenExisting() SUCCEEDED for named mutex: " + mutexName);
            }
            catch (ArgumentNullException)
            {
                // Either mutexName is null or is longer than 260 characters.
                Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: Mutex.OpenExisting() threw ArgumentNullException for named mutex: " + mutexName);
                errorCode = 10;
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // A Mutex object with the prescribed name cannot be opened. 
                //...Log2.v("\nQutils.GetLockOnNamedMutex(): Mutex.OpenExisting() threw WaitHandleCannotBeOpenedException: the named mutex does not already exist: " + mutexName);
                namedMutexDoesNotExist = true;
            }
            catch (IOException)
            {
                // The prescribed name of the Mutex is invalid.  
                // Note that the name and common prefixes "Global" and "Local" are case-sensitive.
                // This exception also covers other errors. 
                Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: Mutex.OpenExisting() threw IOException: the prescribed name of the Mutex may be invalid: " + mutexName);
                errorCode = 11;
            }
            catch (UnauthorizedAccessException ex)
            {
                // The object mutexSecurity has been created with the required access control
                // security to allow any valid Windows user to open an existing HOLD mutex.
                // If this exception is thrown something must have gone badly wrong.
                Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: Mutex.OpenExisting() threw UnauthorizedAccessException for " + mutexName + "\n" + ex.Message);
                errorCode = 12;
            }
            catch (Exception ex)
            {
                // All other exceptions, now and in the future.
                Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: call to Mutex.OpenExisting() threw Exception for " + mutexName + "\n" + ex.Message);
                errorCode = 13;
            }

            // Return if any error conditions occured.
            if (errorCode != Constant.SUCCESS)
            {
                // e.g. mutexName is an empty string or all white space.
                //...Log2.v("\nQutils.GetLockOnNamedMutex(): Exit: returned " + errorCode);
                return errorCode;
            }

            //====== Either the named Mutex already exists and we have joined it or we need to create it. ===========

            // Handle the case where the named mutex dies not already exist - try to create it.
            // If we successfully create the named Mutex then it automatically gives us the lock
            // on it and this method has achieved its objective and can return.
            if (namedMutexDoesNotExist)
            {
                try
                {
                    // Attempt to create a named Mutex that automatically gives us the lock on it.
                    // The constructor returns a boolean value that indicates creation (or not) of a new Mutex.
                    namedMutex = new Mutex(true, mutexName, out holdMutexWasCreated, mutexSecurity);

                    //...Log2.v("\nQutils.GetLockOnNamedMutex(): new Mutex() SUCCEEDED for: " + mutexName);
                    //...Log2.v("\nQutils.GetLockOnNamedMutex(): holdMutexWasCreated = " + holdMutexWasCreated + " for: " + mutexName);

                    // If the HOLD mutex was successfully created then we own the lock on it.
                    if (holdMutexWasCreated)
                    {
                        // This program successfuly created the named Mutex object with initial lock on it.
                        // We have achieved the objective and this method can now return with a SUCCESS code.
                        //...Log2.v("\nQutils.GetLockOnNamedMutex(): this program has the lock on mutex: " + mutexName);
                        return Constant.SUCCESS;
                    }
                    else
                    {
                        // Something unexpected happened.
                        Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: new Mutex() SUCCEEDED but returned holdMutexWasCreated = FALSE.");
                        errorCode = 20;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // The named mutex exists and has access control security, but the user does not have FullControl.
                    Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: new Mutex() threw UnauthorizedAccessException for " + mutexName + " \n" + ex.Message);
                    errorCode = 21;
                }
                catch (IOException ex)
                {
                    // The prescribed name of the Mutex is invalid. 
                    // This can be for various reasons, including some restrictions that may be placed by 
                    // the operating system, such as an unknown prefix or invalid characters. 
                    // Note that the name and common prefixes "Global" and "Local" are case-sensitive.
                    // This exception also covers other errors. 
                    Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: new Mutex() threw IOException for " + mutexName + " \n" + ex.Message);
                    errorCode = 22;
                }
                catch (WaitHandleCannotBeOpenedException ex)
                {
                    // A Nutex object with the provided name cannot be created. 
                    // A synchronization object of a different type might have the same name.
                    Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: new Mutex() threw WaitHandleCannotBeOpenedException for " + mutexName + " \n" + ex.Message);
                    errorCode = 23;
                }
                catch (ArgumentException ex)
                {
                    // The prescribed name of the Mutex is longer than 260 characters.
                    Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: new Mutex() threw ArgumentException for " + mutexName + " \n" + ex.Message);
                    errorCode = 24;
                }
                catch (Exception ex)
                {
                    // All other exceptions, now and in the future.
                    Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: call to new Mutex() threw Exception for " + mutexName + " \n" + ex.Message);
                    errorCode = 25;
                }

            }

            // Return if any error conditions occured.
            if (errorCode != Constant.SUCCESS)
            {
                //...Log2.v("\nQutils.GetLockOnNamedMutex(): Exit: returned " + errorCode);
                return errorCode;
            }

            //========= The case where the named Mutex already exists and we joined it successfully. ===================

            // Attempt to get the lock on the mutex.
            try
            {
                // The WaitOne(nWaitSecs) call blocks until the mutex's WaitHandle receives a signal indicating
                // that it successfully obtained lock on it or the request times-out.
                // Note that WaitOne() expects the wait time to be in milliseconds.
                if (namedMutex.WaitOne(nWaitSecs * 1000))
                {
                    // We successfully obtained lock on the named Mutex.
                    // We have achieved our objective and can now return with a SUCCESS code.
                    //...Log2.v("\nQutils.GetLockOnNamedMutex(): holdMutex.WaitOne(nWaitSecs)     SUCCEEDED for named mutex: " + mutexName);
                    //...Log2.v("\nQutils.GetLockOnNamedMutex(): this program now has the lock on mutex: " + mutexName);
                    return Constant.SUCCESS;
                }
                else
                {
                    // If WaitOne(nWaitSecs) returns FALSE it means that the HOLD mutex is locked by another process
                    // and we timed-out.
                    Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: call to holdMutex.WaitOne({0}) returned FALSE for mutex {1}", nWaitSecs, mutexName);
                    errorCode = 30;
                }
            }
            catch (ObjectDisposedException ex)
            {
                // The mutex object has already been disposed.
                Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: call to namedMutex.WaitOne(nWaitSecs) threw ObjectDisposedException for " + mutexName + " \n" + ex.Message);
                errorCode = 31;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // nWaitSecs is a negative number other than -1, which represents an infinite time-out.
                Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: call to namedMutex.WaitOne(nWaitSecs) threw ArgumentOutOfRangeException for " + mutexName + " \n" + ex.Message);
                errorCode = 32;
            }
            catch (AbandonedMutexException ex)
            {
                // The wait completed because a thread exited without releasing a mutex.
                Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: call to namedMutex.WaitOne(nWaitSecs) threw AbandonedMutexException for " + mutexName + " \n" + ex.Message);
                errorCode = 33;
            }
            catch (InvalidOperationException ex)
            {
                // The current instance is a transparent proxy for a WaitHandle in another application domain.
                Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: call to namedMutex.WaitOne(nWaitSecs) threw InvalidOperationException for " + mutexName + " \n" + ex.Message);
                errorCode = 34;
            }
            catch (Exception ex)
            {
                // All other exceptions, now and in the future.
                Log2.e("\nQutils.GetLockOnNamedMutex(): ERROR: call to namedMutex.WaitOne(nWaitSecs) threw Exception for " + mutexName + " \n" + ex.Message);
                errorCode = 35;
            }

            //...Log2.v("\nQutils.GetLockOnNamedMutex(): Exit: returned " + errorCode);
            return errorCode;
        }

        /// <summary>
        /// This method returns TRUE if there are currently no READ mode access marker files
        /// in the "prod\files\read" directory - indicating that there are not running programs 
        /// that have READ mode access to the DB.
        /// </summary>
        /// <param name="errorCode"> - an error code identifying a specific fail condition.</param>
        /// <param name="matchingProcessID"> - the Windows process ID that 'owns' the first current READ modes marker file.</param>
        /// <returns></returns>
        private static bool NoReadModeMarkerFiles(out int errorCode, out int matchingProcessID)
        {
            //...Log2.v("\nQutils.NoReadModeMarkerFiles(): Entry.");

            // 'out' requirement.
            errorCode = Constant.SUCCESS;
            matchingProcessID = -1;

            bool result = true;

            // Get an array of all currently running processes.
            Process[] processes = null;

            try
            {
                processes = Process.GetProcesses();
            }
            catch (Exception ex)
            {
                GenUtil.SetErrW("enterqueue: Could not get a valid handle for the process list.");
                Log2.e("\nQutils.NoReadModeMarkerFiles(): ERROR: call to Process.GetProcesses() FAILED, threw exception:\n" + ex.Message);
                //...Log2.v("\nQutils.NoReadModeMarkerFiles(): Exit: FALSE with errorCode = 130");
                errorCode = 130;
                return false;
            }

            //	Store all of the process ids in the current snapshot.
            int[] allCurrentProcessIDs = new int[processes.Length];

            for (int i = 0; i < processes.Length; i++)
            {
                allCurrentProcessIDs[i] = processes[i].Id;
            }

            int markerFileProcessID;

            // Get an array containing the paths of all files in directory cDir;
            string[] filePaths;
            try
            {
                filePaths = Directory.GetFiles(mReadAccessMarkerFilesDir);
            }
            catch (Exception ex)
            {
                GenUtil.SetErrW("enterqueue: Could not get a list of files in directory prod\files\read");
                Log2.e("\nQutils.NoReadModeMarkerFiles(): ERROR: call to Directory.GetFiles() FAILED for path: " + mReadAccessMarkerFilesDir + "\n" + ex.Message);
                //...Log2.v("\nQutils.NoReadModeMarkerFiles(): Exit: FALSE with errorCode = 160");
                errorCode = 160;
                return false;
            }

            // We must handle the scenario that there are other running processes that already
            // have Read modes access on the DB and hence have marker files in the directory.
            if (filePaths.Length != 0)
            {
                // Analyze each file name found in the Read marker directory.
                for (int i = 0; i < filePaths.Length; i++)
                {
                    string filePath = Path.GetFileName(filePaths[i]);

                    // The only files that should be in the directory prod\files\read are those
                    // created there by other processes calling the EnterQueue() method requesting
                    // Read mode access; these Read mode marker files have names exactly equaly to 
                    // their process IDs, i.e. the name only contains numerical characters 0-9.
                    // If we can't convert a file name in prod\files\read to an integer then we
                    // have an intruder file that should not be there - but can be ignored.
                    try
                    {
                        markerFileProcessID = Convert.ToInt32(filePath);
                    }
                    catch (Exception)
                    {
                        //...Log2.v(String.Format("\nQutils.EnterQueue(): file {0} in {1} is not a process ID Read mode marker file.", Path.GetFileName(filePath), mReadAccessMarkerFilesDir));
                        continue;
                    }

                    // The Windows System Idle Process has ID = 0 and *will* be present as an element
                    // in the array allCurrentProcessIDs[]. Using an abundance of caution, make sure we 
                    // don't try to match a marker file whose name is 0, or 000 etc.
                    if (markerFileProcessID != 0)
                    {
                        bool foundMatchingProcess = false;

                        //	Scan the currently executing processes.
                        for (int nInd = 0; nInd < allCurrentProcessIDs.Length; nInd++)
                        {
                            if (allCurrentProcessIDs[nInd] == markerFileProcessID)
                            {
                                foundMatchingProcess = true;
                                matchingProcessID = markerFileProcessID;
                                break;      //	Found it
                            }
                        }

                        if (!foundMatchingProcess)
                        {
                            //	The file represents a proc that is no longer executing
                            //	Delete the file, as the previous program did not.

                            File.Delete(filePaths[i]);
                            //...Log2.v("\nQutils.EnterQueue(): deleted orphan file: " + filePaths[i]);
                        }
                        else // foundProcess is true.
                        {
                            // We have detected a process that is currently executing and has previously
                            // obtained DB Read mode access; this thwarts this program's request for Write mode access. 
                            Log2.e("\nQutils.EnterQueue(): ERROR: found a current process that has a READ hold on the DB. Process ID = " + matchingProcessID);
                            //...Log2.v("\nQutils.EnterQueue(): Exit: returned FALSE with errorCode = 140");
                            errorCode = 140;
                            return false;
                        }

                    } // if (nProcID != 0)

                } // for (int i = 0; i < filePaths.Length; i++)

            }  // if (filePaths != null)

            //...Log2.v("\nQutils.NoReadModeMarkerFiles(): Exit: TRUE with errorCode = " + errorCode);
            return result;
        }



    }
}
