# Documented File: Updater.cs
**Repository Path:** `SdUpdateTraf\Updater.cs`
**Primary Layer:** `SdUpdateTraf`
**Namespace:** `SdUpdateTraf`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdateTraf
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides the methods that perform the fetching of records from the
    /// prescribed user su_XXX_traf table and the subsequent changes made to the main 
    /// SDB table main.sd_traf i.a.w. the command ('cmd') field in each fetched record; 
    /// changes to the main SDB table records comprise SQL INSERT (cmd = 'A'), 
    /// UPDATE ('U'), DELETE ('D') operations and also No-Op ('N').
    /// </summary>
    public class Updater
    {
        private static string fullSuTrafTableName;      // Fully-qualified SQL table name 
        private static string modDate;                  // modify date - dd-mmm-yyyy 
        private static string modTime;                  // modify time - hh:mm 
        private static int curHandle;                   // suSelectTraf handle 

        /// <summary>
        /// This method performs the top-level fetching of records from the
        /// prescribed user su_XXX_traf table and the subsequent changes made 
        /// to the main SDB table main.sd_traf i.a.w. the command ('cmd') field 
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
        public static int ProcessSuTraf(SQLHDBC hConn,
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

            // The following objects will be used to fetch SuTraf records from the user 
            // table [schema].sd_[sdfName]_traf.   
            SuTraf suTraf;
            SQLLEN[] suTrafNullInds;

            // The following objects will be used to insert/update/delete "detail" records
            // into/from the MDB table main.sd_traf. 
            SdTraf sdTraf;
            SQLLEN[] sdTrafNullInds;

            Console.Write("\r\n\t\tSDF TRAFFIC UPDATE RESULTS\r\n");

            // Get the names of the su_XXX_traf table that correspond to the 
            // prescribed SDF file.
            GenUtil.UtCvtName(Constant.SU_TRAF, Info.SdfName, out fullSuTrafTableName);

            //...Log2.v("\nUpdater.SuUpdateSDBTraf(): fullSuTrafTableName = " + fullSuTrafTableName);

            // One time call to get the current date and time. These values are
            // used everwhere that needs to write the same date and/or time.
            GenUtil.UtGetDateTime(out modDate, out modTime);

            // Select all records in the user's sd_[sdfName]_traf table for subsequent fetch.
            curHandle = SuDynTraf.SuSelectTraf(fullSuTrafTableName, "", "");

            if (curHandle < 0)
            {
                Log2.e("\nUpdater.SuUpdateSDBTraf(): ERROR: call to SuDynTraf.SuSelectTraf() failed, rc = " + curHandle);
                Console.Write("Error opening cursor - ");
                Console.Write("return code from suSelectTraf is {0}\r\n", curHandle);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                return (Constant.FAILURE);
            }

            bool moreRecordsToFetch = true;
            bool successfulFetchLoop = false;

            // This is the fetch-loop that reads every record in the user's 
            // sd_[sdfName]_traf table.
            while (moreRecordsToFetch)
            {
                // Attempt to fetch an suTraf record from the sd_[sdfName]_traf table.
                rc = SuDynTraf.SuFetchTraf(curHandle, out suTraf, out suTrafNullInds);

                // Check the outcome of the fetch attempt.
                if (rc == Constant.SUCCESS)
                {
                    // The fetch succeeded. 
                    // Tumble out the bottom of this if-else construct.
                    //...Log2.v("\nUpdater.SuUpdateSDBTraf(): Fetch succeeded:" + suTraf.ToStringWN(suTrafNullInds));
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
                    Log2.e("\nUpdater.SuUpdateSDBTraf(): ERROR: call to SuDynTraf.SuFetchTraf() failed, rc = " + rc);
                    successfulFetchLoop = false;
                    break;
                }

                // We successfully fetched a suTraf record that needs processing so first 
                // increment its counter.
                recCounts[(int)Action.Processed]++;

                // Now process the suTraf record according to its 'cmd' column value.
                switch (suTraf.cmd[0])
                {
                    case 'A':

                        // Perform an SQL INSERT into the SDB table main.sd_traf
                        Console.Write("\r\nTRAFFIC RECORD ADD: :{0}/{1}: \r\n", suTraf.trafcode, suTraf.ecode);

                        // Copy values from the SuTraf record into a new SdTraf record.
                        SdTraf.MakeSdFromSu(suTraf, suTrafNullInds, out sdTraf, out sdTrafNullInds);

                        // Set the SdTraf record's modify date and time.
                        sdTraf.mdate = Info.Date;
                        sdTraf.mtime = Info.Time;
                        sdTrafNullInds[SdTraf.MDATE] = Constant.DB_NOT_NULL;
                        sdTrafNullInds[SdTraf.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL INSERT of sdTraf into main.sd_traf
                        rc = DynSdbTraf.SdInsertTraf(hConn, sdTraf, sdTrafNullInds);

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

                        // Perform the SQL DELETE of sdTraf from main.sd_traf
                        rc = DynSdbTraf.SdDeleteTraf(hConn, suTraf.trafcode, suTraf.ecode);

                        if (rc == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Deleted]++;
                            Console.Write("::\t- Successful DELETE of traffic {0}/{1}\r\n", suTraf.trafcode, suTraf.ecode);
                        }
                        else
                        {
                            rcUpdater = Constant.FAILURE;
                            errCount++;
                        }

                        break;

                    case 'U':

                        Console.Write("\r\nTRAFFIC RECORD UPDATE: :{0}/{1}: \r\n", suTraf.trafcode, suTraf.ecode);

                        // Perform the SQL UPDATE of sdTraf in main.sd_traf
                        // Copy values from the SuTraf record into a new SdTraf record.
                        SdTraf.MakeSdFromSu(suTraf, suTrafNullInds, out sdTraf, out sdTrafNullInds);

                        // Set the SdTraf record's modify date and time.
                        sdTraf.mdate = Info.Date;
                        sdTraf.mtime = Info.Time;
                        sdTrafNullInds[SdTraf.MDATE] = Constant.DB_NOT_NULL;
                        sdTrafNullInds[SdTraf.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL UPDATE of sdTraf in main.sd_traf
                        rc = DynSdbTraf.SdUpdateTraf(hConn, sdTraf, sdTrafNullInds);

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

                        Console.Write("\r\nTRAFFIC RECORD NO ACTION: :{0}/{1}: \r\n", suTraf.trafcode, suTraf.ecode);

                        // No-operation.
                        recCounts[(int)Action.NoAction]++;

                        break;
                } // end of switch block.

            } //  end of suTraf fetch-loop.

            // Handle case of an unsuccessful exit from the fetch-loop.
            if (!successfulFetchLoop)
            {
                // A serious SQL/ODBC error occurred; report it */
                Log2.e("\nUpdater.SuUpdateSDBTraf(): ERROR: unsuccessful exit from fetch loop.");
                Console.Write("Error on fetch {0}\r\n", rc);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                rcUpdater = Constant.FAILURE;
            }

            if (SuDynTraf.SuCloseTraf(curHandle) != Constant.SUCCESS)
            {
                Log2.e("\nUpdater.SuUpdateSDBTraf(): ERROR: call to SuDynTraf.SuCloseTraf() failed.");
                Console.Write("Ingres error on close \r\n");
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
            }

            return rcUpdater;
        }




    }
}



```
