# Documented File: Updater.cs
**Repository Path:** `SdUpdateTown\Updater.cs`
**Primary Layer:** `SdUpdateTown`
**Namespace:** `SdUpdateTown`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdateTown
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides the methods that perform the fetching of records from the
    /// prescribed user su_XXX_town table and the subsequent changes made to the main 
    /// SDB table main.sd_town i.a.w. the command ('cmd') field in each fetched record; 
    /// changes to the main SDB table records comprise SQL INSERT (cmd = 'A'), 
    /// UPDATE ('U'), DELETE ('D') operations and also No-Op ('N').
    /// </summary>
    public class Updater
    {
        private static string fullSuTownTableName;      // Fully-qualified SQL table name 
        private static string modDate;                  // modify date - dd-mmm-yyyy 
        private static string modTime;                  // modify time - hh:mm 
        private static int curHandle;                   // suSelectTown handle 

        /// <summary>
        /// This method performs the top-level fetching of records from the
        /// prescribed user su_XXX_town table and the subsequent changes made 
        /// to the main SDB table main.sd_town i.a.w. the command ('cmd') field 
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
        public static int ProcessSuTown(SQLHDBC hConn,
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

            // The following objects will be used to fetch SuTown records from the user 
            // table [schema].sd_[sdfName]_town.   
            SuTown suTown;
            SQLLEN[] suTownNullInds;

            // The following objects will be used to insert/update/delete "detail" records
            // into/from the MDB table main.sd_town. 
            SdTown sdTown;
            SQLLEN[] sdTownNullInds;

            Console.Write("\r\n\t\tSDF TOWER NOTES UPDATE RESULTS\r\n");

            // Get the names of the su_XXX_town table that correspond to the 
            // prescribed SDF file.
            GenUtil.UtCvtName(Constant.SU_TOWN, Info.SdfName, out fullSuTownTableName);

            //...Log2.v("\nUpdater.SuUpdateSDBTown(): fullSuTownTableName = " + fullSuTownTableName);

            // One time call to get the current date and time. These values are
            // used everwhere that needs to write the same date and/or time.
            GenUtil.UtGetDateTime(out modDate, out modTime);

            // Select all records in the user's sd_[sdfName]_town table for subsequent fetch.
            curHandle = SuDynTown.SuSelectTown(fullSuTownTableName, "", "");

            if (curHandle < 0)
            {
                Log2.e("\nUpdater.SuUpdateSDBTown(): ERROR: call to SuDynTown.SuSelectTown() failed, rc = " + curHandle);
                Console.Write("Error opening cursor - ");
                Console.Write("return code from suSelectTown is {0}\r\n", curHandle);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                return (Constant.FAILURE);
            }

            bool moreRecordsToFetch = true;
            bool successfulFetchLoop = false;

            // This is the fetch-loop that reads every record in the user's 
            // sd_[sdfName]_town table.
            while (moreRecordsToFetch)
            {
                // Attempt to fetch an suTown record from the sd_[sdfName]_town table.
                rc = SuDynTown.SuFetchTown(curHandle, out suTown, out suTownNullInds);

                // Check the outcome of the fetch attempt.
                if (rc == Constant.SUCCESS)
                {
                    // The fetch succeeded. 
                    // Tumble out the bottom of this if-else construct.
                    //...Log2.v("\nUpdater.SuUpdateSDBTown(): Fetch succeeded:" + suTown.ToStringWN(suTownNullInds));
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
                    Log2.e("\nUpdater.SuUpdateSDBTown(): ERROR: call to SuDynTown.SuFetchTown() failed, rc = " + rc);
                    successfulFetchLoop = false;
                    break;
                }

                // We successfully fetched a suTown record that needs processing so first 
                // increment its counter.
                recCounts[(int)Action.Processed]++;

                // Now process the suTown record according to its 'cmd' column value.
                switch (suTown.cmd[0])
                {
                    case 'A':

                        // Perform an SQL INSERT into the SDB table main.sd_town
                        Console.Write("\r\nTOWER NOTE RECORD ADD: :{0}/{1}: \r\n", suTown.call1, suTown.atwrno);

                        // Copy values from the SuTown record into a new SdTown record.
                        SdTown.MakeSdFromSu(suTown, suTownNullInds, out sdTown, out sdTownNullInds);

                        // Set the SdTown record's modify date and time.
                        sdTown.mdate = Info.Date;
                        sdTown.mtime = Info.Time;
                        sdTownNullInds[SdTown.MDATE] = Constant.DB_NOT_NULL;
                        sdTownNullInds[SdTown.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL INSERT of sdTown into main.sd_town
                        rc = DynSdbTown.SdInsertTown(hConn, sdTown, sdTownNullInds);

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

                        // Perform the SQL DELETE of sdTown from main.sd_town
                        rc = DynSdbTown.SdDeleteTown(hConn, suTown.call1, suTown.atwrno);

                        if (rc == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Deleted]++;
                            Console.Write("::\t- Successful DELETE of Tower Note: {0}/{1}\r\n", suTown.call1, suTown.atwrno);
                        }
                        else
                        {
                            rcUpdater = Constant.FAILURE;
                            errCount++;
                        }

                        break;

                    case 'U':

                        // Perform the SQL UPDATE of sdTown in main.sd_town
                        // Copy values from the SuTown record into a new SdTown record.
                        SdTown.MakeSdFromSu(suTown, suTownNullInds, out sdTown, out sdTownNullInds);

                        // Set the SdTown record's modify date and time.
                        sdTown.mdate = Info.Date;
                        sdTown.mtime = Info.Time;
                        sdTownNullInds[SdTown.MDATE] = Constant.DB_NOT_NULL;
                        sdTownNullInds[SdTown.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL UPDATE of sdTown in main.sd_town
                        rc = DynSdbTown.SdUpdateTown(hConn, sdTown, sdTownNullInds);

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

                        Console.Write("\r\nTOWER NOTES RECORD NO ACTION: :{0}/{1}: \r\n", suTown.call1, suTown.atwrno);

                        // No-operation.
                        recCounts[(int)Action.NoAction]++;

                        break;
                } // end of switch block.

            } //  end of suTown fetch-loop.

            // Handle case of an unsuccessful exit from the fetch-loop.
            if (!successfulFetchLoop)
            {
                // A serious SQL/ODBC error occurred; report it */
                Log2.e("\nUpdater.SuUpdateSDBTown(): ERROR: unsuccessful exit from fetch loop.");
                Console.Write("Error on fetch {0}\r\n", rc);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                rcUpdater = Constant.FAILURE;
            }

            if (SuDynTown.SuCloseTown(curHandle) != Constant.SUCCESS)
            {
                Log2.e("\nUpdater.SuUpdateSDBTown(): ERROR: call to SuDynTown.SuCloseTown() failed.");
                Console.Write("Ingres error on close \r\n");
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
            }

            return rcUpdater;
        }




    }
}



```
