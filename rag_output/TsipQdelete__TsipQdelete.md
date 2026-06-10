# Documented File: TsipQdelete.cs
**Repository Path:** `TsipQdelete\TsipQdelete.cs`
**Primary Layer:** `TsipQdelete`
**Namespace:** `TsipQdelete`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This program deletes a prescribed job that is waiting in the TSIP queue. 
/// </summary>
/// <remarks>
/// USAGE:
/// <code>
///        tsipQdelete <database> [<Job Number>]
/// </code>
/// The following functionality appears to have existed in the past but is currently not implemented:
/// if no job number is given, the program provides a list of all current jobs in the TSIP queue.
/// </remarks>
namespace TsipQdelete
{
    /// <summary>
    /// This class provides a Main() method that deletes a prescribed job from the TSIP queue. 
    /// </summary>
    public class TsipQdelete
    {
        /// <summary>
        /// This Main() method manages the deletion a prescribed job from the TSIP queue. 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            int nRet;
            UserInfoData tUser;
            string cSQL;
            int nJob;
            TSIPQ tQel = null;

            try
            {
                //	If there are no arguments, error
                Info.BuildMetaData = Info.CollateExeMetaData();

                if (args.Length != 1 && args.Length != 2)
                {
                    Console.Write("\nUsage: tsipQdelete build {0}:-\n       tsipQdelete <database> [<Job Number>]\n", Info.BuildMetaData);
                    Application.Exit(127);
                }
#if false
                // Turn on developmental logging.
                string mLog2FilePath = @"d:\MicsBatchLogs\TsipQdelete.log";
                if (Log2.SetLogFilePath(mLog2FilePath))
                {
                    Log2.Erase();
                    Log2.Set(Log2.FileOpenClose.PER_SESSION);
                    Log2.Set(Log2.WriteMode.ENABLED);
                    Log2.Set(Log2.Level.VERBOSE);

                    Log2.i("\n Build: " + Info.BuildMetaData);
                }
                else
                {
                    Console.Error.Write("\nERROR: could not open Log2 file: " + mLog2FilePath);
                }
#endif
                // Get and set required environment variables.
                Info.DbName = args[0];
                Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
                if (String.IsNullOrWhiteSpace(Info.MicsUserName))
                {
                    Log2.e("\nTsipQdelete.Main(): ERROR: Windows environment variable MicsUser is not set.");
                    Application.Exit(99);
                }

                Info.Password = Environment.GetEnvironmentVariable("PASSWORD");         // REQUIRED (but can be anything).
                if (String.IsNullOrWhiteSpace(Info.Password))
                {
                    // Password just has to be set to something; its value is never used.
                    Info.Password = "Bananarama";
                }

                // Attempt to connect for a MICS user 'session'.
                nRet = Ssutil.UtConnect(Info.DbName, 1);
                if (nRet != Constant.SUCCESS)
                {
                    //	Could not connect. present error message.
                    Log2.e("\nTsipQdelete.Main(): ERROR: call to Ssutil.UtConnect() failed for database name = " + Info.DbName);
                    Console.Write("\nCan't connect to database {0} ({1})\n", Info.DbName, nRet);
                    Application.Exit(125);
                }

                // We are now successfully connected to the database.

                TsipQ.SetGateName(Info.DbName);       //	Name the gate used for serializing this table.

                string gateName = TsipQ.GetGateName();

                if (TsipQ.GuardIn(gateName, 30) == 0)
                {
                    //	Connected.  Check for the second arg.  If not there just list the queue.
                    if (args.Length == 1)
                    {
                        //	No job number.  List the queue
                        TsipQList();

                        nRet = 1;
                    }
                    else
                    {
                        //	There is a job number, just delete it.
                        //	First get the user info.
                        nJob = Convert.ToInt32(args[1]);

                        nRet = UserInfo.UtGetUserInfo(out tUser);

                        nRet = TsipQ.GetTsipQJob(nJob, out tQel);

                        //...Log2.v("\ntQel:\n" + tQel.ToString());

                        if (nRet == 0)
                        {
                            //	We got the row for the job.  Check it is for the right user.
                            if (tQel.TQ_MicsID.Equals(tUser.micsUser.micsid))
                            {
                                //...Log2.v("\nTsipQdelete.Main(): the prescribed Tsip job has been found in the queue\ntQel:\n" + tQel.ToString());

                                //	User is the same.
                                if (tQel.TQ_Status.Equals("W"))
                                {
                                    //	We have found the waiting record, now update.
                                    cSQL = String.Format("Update web.tsip_queue set TQ_Status = 'D', TQ_TimeStart = CURRENT_TIMESTAMP, TQ_ProcID = -1 where TQ_Job = {0}",
                                                        nJob);
                                    nRet = Ssutil.DbExecute(cSQL) ? 0 : 10;

                                    //...Log2.v("\nTsipQdelete.Main(): the prescribed Tsip job has been found and is currently marked as waiting.");
                                    //...Log2.v("\nTsipQdelete.Main(): the prescribed Tsip job has been updated: TQ_Status = 'D'");
                                }
                                else
                                {
                                    //...Log2.v("\nTsipQdelete.Main(): the prescribed Tsip job is NOT waiting; current status = " + tQel.TQ_Status);
                                    nRet = 3;   // Finished, or executing.
                                }
                            }
                            else
                            {
                                //...Log2.v("\nTsipQdelete.Main(): the prescribed Tsip job belongs to another user.");
                                nRet = 2;       //	Found but belongs to another user.
                            }
                        }
                        else if (nRet == 1)
                        {
                            //...Log2.v("\nTsipQdelete.Main(): the prescribed Tsip job is not found in the queue.");
                            nRet = 1;           //	Job not found.
                        }
                        else
                        {
                            Log2.e("\nTsipQdelete.Main(): ERROR: the call to TsipQ.GetTsipQJob() failed.");
                            nRet = 11;      //	Error
                        }
                    }
                    TsipQ.GuardOut(gateName);    // Let the other process at the queue.

                    if (nRet == 0)
                    {
                        //	Signal tsipInitiator to do the actual deletion.
                        TsipQ.SignalEvent(tQel.TQ_Job);
                    }

                }
                else
                {
                    //...Log2.v("\nTsipQdelete.Main(): TSIP Queue is busy.\n");
                    Console.Write("\nTsip Queue is busy.\n");
                    Ssutil.UtDisconnect(1);
                    Application.Exit(123);
                }

                Ssutil.UtDisconnect(1);

                Application.Exit(nRet);

            }
            catch (Exception e)
            {
                Log2.e("\n\nTsipQdelete.Main(): exception caught: " + e.Message);
                Log2.e("\n\nTsipQdelete.Main(): stack trace: \n\n" + e.StackTrace);

                Application.Exit(Error.FATAL_EXCEPTION);
            }




        } // Main()

        /// <summary>
        /// This methods writes a header sentence to stdout.
        /// </summary>
        /// <returns></returns>
        public static int TsipQList()
        {
            Console.Write("\nCurrent unfinished tsip Queue...\n");

            return Constant.SUCCESS;
        }







    }
}

```
