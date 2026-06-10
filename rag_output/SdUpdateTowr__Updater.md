# Documented File: Updater.cs
**Repository Path:** `SdUpdateTowr\Updater.cs`
**Primary Layer:** `SdUpdateTowr`
**Namespace:** `SdUpdateTowr`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdateTowr
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides the methods that perform the fetching of records from the
    /// prescribed user su_XXX_towr table and the subsequent changes made to the main 
    /// SDB table main.sd_towr i.a.w. the command ('cmd') field in each fetched record; 
    /// changes to the main SDB table records comprise SQL INSERT (cmd = 'A'), 
    /// UPDATE ('U'), DELETE ('D') operations and also No-Op ('N').
    /// </summary>
    public class Updater
    {
        private static string fullSuTowrTableName;      // Fully-qualified SQL table name 
        private static string modDate;                  // modify date - dd-mmm-yyyy 
        private static string modTime;                  // modify time - hh:mm 
        private static int curHandle;                   // suSelectTowr handle 

        /// <summary>
        /// This method performs the top-level fetching of records from the
        /// prescribed user su_XXX_towr table and the subsequent changes made 
        /// to the main SDB table main.sd_towr i.a.w. the command ('cmd') field 
        /// in each fetched record; changes to the main SDB table records comprise
        /// SQL INSERT (cmd = 'A'), UPDATE ('U'), DELETE ('D') operations and 
        /// also No-Op ('N').
        /// </summary>
        /// <param name="hConn"></param>
        /// <param name="sdfName"></param>
        /// <param name="errCount"></param>
        /// <param name="warnCount"></param>
        /// <param name="recCounts"></param>
        /// <returns></returns>
        public static int ProcessSuTowr(SQLHDBC hConn,
                                            string sdfName,
                                            out int errCount,
                                            out int warnCount,
                                            out int[] recCounts
                                        )
        {
            // Initialize the counters.
            const int NDIM = 5;
            recCounts = Arrays.CreateAndFillArray(NDIM, 0);
            errCount = 0;
            warnCount = 0;

            int rcUpdater = Constant.SUCCESS;
            int rc = Constant.SUCCESS;

            // The following objects will be used to fetch SuTowr records from the user 
            // table [schema].sd_[sdfName]_towr.   
            SuTowr suTowr;
            SQLLEN[] suTowrNullInds;

            // The following objects will be used to insert/update/delete "detail" records
            // into/from the MDB table main.sd_towr. 
            SdTowr sdTowr;
            SQLLEN[] sdTowrNullInds;

            Console.Write("\r\n\t\tSDF UPDATE RESULTS\r\n");

            // Get the names of the su_XXX_towr table that correspond to the 
            // prescribed SDF file.
            GenUtil.UtCvtName(Constant.SU_TOWR, Info.SdfName, out fullSuTowrTableName);

            //...Log2.v("\nUpdater.SuUpdateSDBTowr(): fullSuTowrTableName = " + fullSuTowrTableName);

            // One time call to get the current date and time. These values are
            // used everwhere that needs to write the same date and/or time.
            GenUtil.UtGetDateTime(out modDate, out modTime);

            // Select all records in the user's sd_[sdfName]_towr table for subsequent fetch.
            curHandle = SuDynTowr.SuSelectTowr(fullSuTowrTableName, "", "");

            if (curHandle < 0)
            {
                Log2.e("\nUpdater.SuUpdateSDBTowr(): ERROR: call to SuDynTowr.SuSelectTowr() failed, rc = " + curHandle);
                Console.Write("Error opening cursor - ");
                Console.Write("return code from suSelectTowr is {0}\r\n", curHandle);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                return (Constant.FAILURE);
            }

            bool moreRecordsToFetch = true;
            bool successfulFetchLoop = false;

            // This is the fetch-loop that reads every record in the user's 
            // sd_[sdfName]_towr table.
            while (moreRecordsToFetch)
            {
                // Attempt to fetch an suTowr record from the sd_[sdfName]_towr table.
                rc = SuDynTowr.SuFetchTowr(curHandle, out suTowr, out suTowrNullInds);

                // Check the outcome of the fetch attempt.
                if (rc == Constant.SUCCESS)
                {
                    // The fetch succeeded. 
                    // Tumble out the bottom of this if-else construct.
                    //...Log2.v("\nUpdater.SuUpdateSDBTowr(): Fetch succeeded:" + suTowr.ToStringWN(suTowrNullInds));
                }
                else if (rc == ODBC.SQL_NO_DATA)
                {
                    // There are no more records to fetch; break out of the while loop.
                    successfulFetchLoop = true;
                    break;
                }
                else
                {
                    // A serious ODBC/SQL error must have occurred; break out of the while loop.
                    Log2.e("\nUpdater.SuUpdateSDBTowr(): ERROR: call to SuDynTowr.SuFetchTowr() failed, rc = " + rc);
                    successfulFetchLoop = false;
                    break;
                }

                // We successfully fetched a suTowr record that needs processing so first 
                // increment its counter.
                recCounts[(int)Action.Processed]++;

                // Now process the suTowr record according to its 'cmd' column value.
                switch (suTowr.cmd[0])
                {
                    case 'A':

                        // Perform an SQL INSERT into the SDB table main.sd_towr

                        // Copy values from the SuTowr record into a new SdTowr record.
                        SdTowr.MakeSdFromSu(suTowr, suTowrNullInds, out sdTowr, out sdTowrNullInds);

                        // Set the SdTowr record's modify date and time.
                        sdTowr.mdate = Info.Date;
                        sdTowr.mtime = Info.Time;
                        sdTowrNullInds[SdTowr.MDATE] = Constant.DB_NOT_NULL;
                        sdTowrNullInds[SdTowr.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL INSERT of sdTowr into main.sd_towr
                        rc = DynSdbTowr.SdInsertTowr(hConn, sdTowr, sdTowrNullInds);

                        if (rc == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Added]++;
                        }
                        else
                        {
                            rcUpdater = Constant.FAILURE;
                        }

                        break;

                    case 'D':

                        // Perform the SQL DELETE of sdTowr from main.sd_towr
                        rc = DynSdbTowr.SdDeleteTowr(hConn, suTowr.twcode);

                        if (rc == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Deleted]++;
                            Console.Write("::\t- Successful DELETE of Tower {0}\r\n", suTowr.twcode);
                        }
                        else
                        {
                            rcUpdater = Constant.FAILURE;
                            errCount++;
                        }

                        break;

                    case 'U':

                        // Perform the SQL UPDATE of sdTowr in main.sd_towr
                        // Copy values from the SuTowr record into a new SdTowr record.
                        SdTowr.MakeSdFromSu(suTowr, suTowrNullInds, out sdTowr, out sdTowrNullInds);

                        // Set the SdTowr record's modify date and time.
                        sdTowr.mdate = Info.Date;
                        sdTowr.mtime = Info.Time;
                        sdTowrNullInds[SdTowr.MDATE] = Constant.DB_NOT_NULL;
                        sdTowrNullInds[SdTowr.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL UPDATE of sdTowr in main.sd_towr
                        rc = DynSdbTowr.SdUpdateTowr(hConn, sdTowr, sdTowrNullInds);

                        if (rc == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Updated]++;
                        }
                        else
                        {
                            rcUpdater = Constant.FAILURE;
                        }

                        break;

                    case 'N':

                        Console.Write("\r\nTOWER RECORD NO ACTION: :{0}: \r\n", suTowr.twcode);

                        // No-operation.
                        recCounts[(int)Action.NoAction]++;

                        break;
                } // end of switch block.

            } //  end of suTowr fetch-loop.

            // Handle case of an unsuccessful exit from the fetch-loop.
            if (!successfulFetchLoop)
            {
                // A serious SQL/ODBC error occurred; report it */
                Log2.e("\nUpdater.SuUpdateSDBTowr(): ERROR: unsuccessful exit from fetch loop.");
                Console.Write("Error on fetch {0}\r\n", rc);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                rcUpdater = Constant.FAILURE;
            }

            if (SuDynTowr.SuCloseTowr(curHandle) != Constant.SUCCESS)
            {
                Log2.e("\nUpdater.SuUpdateSDBTowr(): ERROR: call to SuDynTowr.SuCloseTowr() failed.");
                Console.Write("Ingres error on close \r\n");
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
            }

            return rcUpdater;
        }




    }
}



```
