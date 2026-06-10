# Documented File: Qutils 20200902.cs
**Repository Path:** `_Utillib\Qutils 20200902.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _Utillib
{
    /// <summary>
    /// Provides methods to manage queueing and waiting for database access.
    /// </summary>
    public class Qutils
    {
#if PINVOKE
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int enterqueue(string s1, string s2, int i);

        //void explainqueue(char*, char*, int, FILE*)
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern void explainqueue(string s1, string s2, int i, IntPtr p);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int exitqueue([In] string dbase, [In] string cQName);

        public static int EnterQueue_NATIVE(string s1, string s2, int i)
        {
            //...Log2.v("\r\nQutils.EnterQueue(): called");

            return enterqueue(s1, s2, i);

        }

        public static int ExitQueue_NATIVE(string dbase, string cQName)
        {
            return exitqueue(dbase, cQName);
        }

        public static void ExplainQueue_NATIVE(string s1, string s2, int i, IntPtr intPtr)
        {
            explainqueue(s1, s2, i, intPtr);
        }
#endif
        //--------------------------------------------------------------------------------------

        private static Mutex mMutex;       //	This is kept over calls if it is a write mutex.
        private static string mThisProcessMarkerFilePath = null;

        //--------------------------------------------------------------------------------------

        /// <summary>
        /// This method manages entry into the hold queue for database access; note
        /// that Info.MicsUserName <b>must</b> be set prior to calling this method.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>First it checks for the "HOLD" queue. This will return immediately with 
        /// a code of 2 meaning stop if there is a hold on the database.</item> 
        /// <item>Next, it checks for the WRITE queue.  If it can get this, and it wants it, then it holds the 
        /// mutex and exits with 0 which is 'go'. If not, it waits until it becomes available 
        /// or until the timeout is expired.</item>
        /// <item>An expired timeout returns a 1 meaning it didn't get it.</item> 
        /// <item>If it gets past the WRITE queue, and it wants a read, then while 
        /// it holds the WRITE mutex it writes its process id as the file name to the 
        /// &lt;database&gt;/files/read directory, frees the "WRITE" mutex and returns a 0.</item> 
        /// <item>If it wants a WRITE, then it does not release the WRITE, but checks the above read 
        /// directory. It reads each file in it, and checks the process id to see if that file 
        /// is still executing. If not, it deletes the file.  If there are any files left in the 
        /// directory, then it releases the WRITE and returns a 1, if not it does not release the 
        /// write and returns a 0.</item> 
        /// </list>
        /// <para>Errors return a negative number.</para>
        /// </remarks>
        /// <param name="dBase"> - name of the database.</param>
        /// <param name="cQName"> - name of the queue: "READ" or "WRITE".</param>
        /// <param name="nWaitSecs"> - maximum number of seconds to wait.</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS -attempt to enter queue was successful.</para>
        /// <para>- Any negative number - attempt failed.</para>
        public static int EnterQueue(string dBase, string cQName, int nWaitSecs)
        {
            //  This whole method needs to be redesigned i.a.w. C# best-practices for opening
            //  a Mutex: see https://docs.microsoft.com/en-us/dotnet/api/system.threading.mutex.openexisting?view=netframework-4.7.2#System_Threading_Mutex_OpenExisting_System_String_
            //  
            //  The code in this method mimics the original C/C++ code but this C# code throws
            //  several types of exceptions if problems occur.

            //...Log2.v("\nQutils.EnterQueue(): ping");

            string mutexName;
            bool createdNew = false;
            MutexSecurity pSA = GenUtil.SAEverybody();

            string cDir = Ssutil.GetMicsRoot(dBase);
            cDir += "files\\read";

            mutexName = MakeName(dBase, "HOLD");

            //	First check the hold queue.

            //  If a named Mutex already exists (e.g. Global\FCSAHOLD) an attempt to 
            //  instatiate a new Mutex with the same name throws a Win32 IOException. 
            //
            //  In previous versions of this C# code, this exception would bubble-up 
            //  to the Main() program's try-catch where it is caught and invokes an 
            //  Application.Exit(). However, the 'context' of this exception is then
            //  difficult to glean.
            //  
            //  To provide a localized Log2.e() error message, we will first catch the
            //  exception locally and then re-throw it with improved context
            //  information to be caught in the Main() program's try-catch.
            try
            {
                mMutex = new Mutex(false, mutexName, out createdNew, pSA);
            }
            catch (Exception e)
            {
                string message = "\nQutils.EnterQueue(): ERROR: attempt to instantiate a Mutex for HOLD failed.\n";
                Log2.e(message + e.Message);
                Console.Error.Write("\r\nERROR: another program has already placed a HOLD on access to the database.\r\n");

                // To maintain the same flow-of-control as the previous C# code
                // we need to re-throw the exception with some additional context.
                message = String.Format("\nQutils.EnterQueue(): ERROR: attempt to instantiate a Mutex with the name {0} threw an exception.\n{1}", mutexName, e.Message);
                throw new Exception(message, e);
            }


            if (!createdNew)
            {
                //	Could not create a new mutex, this must mean it already exists (bad).
                //	Try to open it.
                mMutex = Mutex.OpenExisting(mutexName);//   OpenMutex(SYNCHRONIZE, FALSE, cFullQName);
                if (mMutex == null)
                {
                    GenUtil.SetErrW("enterqueue: Could not create the HOLD queue mutex '%s'.", mutexName);
                    return -1;
                }
                else
                {
                    mMutex.SetAccessControl(pSA);
                }
            }

            // Check if the hold is up.
            // Block the current thread until the current WaitHandle receives a signal.
            // Setting the Timeout to zero prevents the call to WaitOne() from blocking.
            // It tests the state of the wait handle and returns immediately.
            if (!mMutex.WaitOne(0))
            {
                // The mutex is owned. This means exit.
                mMutex.Close();
                return 2;
            }
            mMutex.Close();

            //Now check for a WRITE handle.

            mutexName = MakeName(dBase, "WRITE");

            try
            {
                mMutex = new Mutex(false, mutexName, out createdNew, pSA);
            }
            catch (Exception e)
            {
                string message = "\nQutils.EnterQueue(): ERROR: attempt to instantiate a Mutex for WRITE failed.\n";
                Log2.e(message + e.Message);
                Console.Error.Write("\r\nERROR: another program holds the Mutex for WRITE.\r\n");

                // To maintain the same flow-of-control as the previous C# code
                // we need to re-throw the exception with some additional context.
                message = String.Format("\nQutils.EnterQueue(): ERROR: attempt to instantiate a Mutex with the name {0} threw an exception.\n{1}", mutexName, e.Message);
                throw new Exception(message, e);
            }

            //...Log2.v("\nQutils.EnterQueue(): new Mutex() returned createdNew = " + createdNew);

            // Check if the Mutex already existed.
            if (!createdNew)
            {
                // Could not create, probably already exists, but check.
                mMutex = Mutex.OpenExisting(mutexName, MutexRights.Synchronize);
                if (mMutex == null)
                {
                    GenUtil.SetErrW("enterqueue: Could not create mutex for '%s'.", mutexName);
                    return -2;
                }
            }

            // Establish the WRITE mutex.
            if (!mMutex.WaitOne(Constant.GLOBAL_WAIT_TIME_SECONDS * 1000))
            {
                //	Wait is held by someone else.  Exit to indicate it.
                mMutex.Close();
                return 1;
            }
            else  // mMutex.WaitOne(Constant.GLOBAL_WAIT_TIME_SECONDS * 1000)
            {
                //	We have the WRITE mutex.  Now if we are requesting WRITE, we 
                //	need to go through and see that no reads are going on.  If we
                //	are a read, then we just indicate we are reading now.
                if (cQName.Equals("WRITE"))
                {
                    //	We have the WRITE Mutex.  We also want to write to
                    //	the database, so we first check the read list to see if it is
                    //	empty, while holding the write.  The read list is a file in
                    //	\<test or prod>\files\read.  Each file has the name of its
                    //	process id, and the contents is the name of the file.


                    // Get an array containing the paths of all files in directory cDir;
                    string[] filePaths = Directory.GetFiles(cDir);

                    // Get an array of all currently running processes.
                    Process[] processes = Process.GetProcesses();

                    // Check if GetProcesses() was successful.
                    if (processes == null)
                    {
                        mMutex.ReleaseMutex();
                        mMutex.Close();
                        GenUtil.SetErrW("enterqueue: Could not get a valid handle for the process list.");
                        return -3;
                    }

                    //	Store all of the proc ids in the current snapshot.
                    int[] aProcIDs = new int[processes.Length];

                    for (int i = 0; i < processes.Length; i++)
                    {
                        aProcIDs[i] = processes[i].Id;
                    }

                    int nProcID;

                    if (filePaths != null)
                    {
                        for (int i = 0; i < filePaths.Length; i++)
                        {

                            //	For each file we get.  Check that it is still running.
                            //	The file name is the process id.
                            nProcID = Convert.ToInt32(Path.GetFileName(filePaths[i]));

                            if (nProcID != 0)
                            {
                                int nInd;
                                bool foundProcess = false;
                                //	Scan the currently executing processes.
                                for (nInd = 0; nInd < aProcIDs.Length; nInd++)
                                {
                                    if (aProcIDs[nInd] == nProcID)
                                    {
                                        foundProcess = true;
                                        break;      //	Found it
                                    }
                                }

                                if (!foundProcess)
                                {
                                    //	The file represents a proc that is no longer executing
                                    //	Delete the file, as the program did not.

                                    File.Delete(filePaths[i]);
                                }

                                else // foundProcess is true.
                                {
                                    //	This represents a program that *is* executing.  This is bad
                                    //	as we are in a write request.
                                    mMutex.ReleaseMutex();
                                    mMutex.Close();
                                    return 1;   //	Indicate that we can go no further.
                                }

                            } // if (nProcID != 0)

                        } // for (int i = 0; i < filePaths.Length; i++)

                    }  // if (filePaths != null)

                }
                else // !cQName.Equals("WRITE")
                {
                    //	We have the write mutex and we want read mode.  So while we hold this, we
                    //	write a file to the read directory with our process name as the file name.
                    Process currentProcess = Process.GetCurrentProcess();
                    //...Log2.v("\r\nQutils.EnterQueue(): cDir = " + cDir);
                    mThisProcessMarkerFilePath = String.Format("{0}\\{1}", cDir, currentProcess.Id);

                    try
                    {
                        // Create the process marker file then immediately close it.
                        FileStream fileStream = File.Create(mThisProcessMarkerFilePath);
                        fileStream.Close();
                    }
                    catch
                    {
                        GenUtil.SetErrW("enterqueue: Could not open file %s for writing.", mThisProcessMarkerFilePath);
                        mMutex.ReleaseMutex();
                        mMutex.Close();
                        return -4;
                    }

                    //	Finished with the WRITE mutex.
                    mMutex.ReleaseMutex();
                    mMutex.Close();
                } // else !cQName.Equals("WRITE")

                return Constant.SUCCESS;

            } // mMutex.WaitOne(Constant.GLOBAL_WAIT_TIME_SECONDS * 1000)

        }

        /// <summary>
        /// Exit the READ/WRITE queue. If this was a read, then kill the process
        /// file. If this was a WRITE then release the MUTEX.
        /// </summary>
        /// <param name="dbase"> - database name (e.g. "fcsa")</param>
        /// <param name="cQName"> - either "READ" or "WRITE".</param>
        /// <returns> - always return Constant.SUCCESS.</returns>
        public static int ExitQueue(string dbase, string cQName)
        {
            if (cQName.Equals("READ"))
            {
                // It is the read queue, remove the file.
                if (mThisProcessMarkerFilePath != null)
                {
                    try
                    {
                        File.Delete(mThisProcessMarkerFilePath);

                        mThisProcessMarkerFilePath = null;
                    }
                    catch (Exception e)
                    {
                        Log2.e("\nQuutils.ExitQueue(): ERROR: failed to delete ProcessMarkeFile: " + e.Message);
                    }
                }
            }
            else
            {
                mMutex.ReleaseMutex();
                mMutex.Close();
            }

            return Constant.SUCCESS;
        }

        /// <summary>
        /// Write an error message to the output stream attached to a TextWriter object.
        /// </summary>
        /// <param name="dbase"> - name of the database.</param>
        /// <param name="cQName"> - name of the queue: "READ" or "WRITE".</param>
        /// <param name="nErr"> - error code.</param>
        /// <param name="fOut"> - pointer/handle to prescribed output stream.</param>
        public static void ExplainQueue(string dbase, string cQName, int nErr, TextWriter fOut)
        {
            TextWriter tw = fOut;

            if (fOut == null)
            {
                tw = Console.Out;
            }

            switch (nErr)
            {
                case 0: // all went fine.
                    tw.Write("\r\nProgram entered the {0} queue on {1} without problem.\r\n", cQName, dbase);
                    break;

                case 1: //	Queue is held by someone else
                    if (cQName.Equals("READ"))
                    {
                        tw.Write("\r\nThe {0} on {1} is being blocked by a WRITE.  Try again later...\r\n", cQName, dbase);
                    }
                    else
                    {
                        tw.Write("\r\nThe {0} on {1} is being blocked by another WRITE, or READs.  Try again later...\r\n", cQName, dbase);
                    }
                    break;

                case 2: //	Database is on HOLD
                    tw.Write("\r\nFCSA has locked the {0} database.  Try later.\r\n", dbase);
                    break;

                default:    //	All errors come here.
                    tw.Write("\r\nError ({0}) when attempting to queue for database access:\r\n{1}\r\n", nErr, GenUtil.GetUserMess());
                    break;
            }

            return;
        }

        /// <summary>
        /// Returns a string containing the name of the mutex (or anything else that needs to be global).
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


    }
}

```
