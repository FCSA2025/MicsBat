# Documented File: Updater.cs
**Repository Path:** `SdUpdateEqpt\Updater.cs`
**Primary Layer:** `SdUpdateEqpt`
**Namespace:** `SdUpdateEqpt`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdateEqpt
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides the methods that perform the fetching of records from the
    /// prescribed user su_XXX_eqpt table and the subsequent changes made to the main 
    /// SDB table main.sd_eqpt i.a.w. the command ('cmd') field in each fetched record; 
    /// changes to the main SDB table records comprise SQL INSERT (cmd = 'A'), 
    /// UPDATE ('U'), DELETE ('D') operations and also No-Op ('N').
    /// </summary>
    public class Updater
    {
        private static string fullSuEqptTableName;      // Fully-qualified SQL table name 
        private static string modDate;                  // modify date - dd-mmm-yyyy 
        private static string modTime;                  // modify time - hh:mm 
        private static int curHandle;                   // suSelectEqpt handle 

        /// <summary>
        /// This method performs the top-level fetching of records from the
        /// prescribed user su_XXX_eqpt table and the subsequent changes made 
        /// to the main SDB table main.sd_eqpt i.a.w. the command ('cmd') field 
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
        public static int ProcessSuEqpt(SQLHDBC hConn,
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

            // The following objects will be used to fetch SuEqpt records from the user 
            // table [schema].sd_[sdfName]_eqpt.   
            SuEqpt suEqpt;
            SQLLEN[] suEqptNullInds;

            // The following objects will be used to insert/update/delete "detail" records
            // into/from the MDB table main.sd_eqpt. 
            SdEqpt sdEqpt;
            SQLLEN[] sdEqptNullInds;

            Console.Write("\r\n\t\tSDF EQUIPMENT UPDATE RESULTS\r\n");

            // Get the names of the su_XXX_eqpt table that correspond to the 
            // prescribed SDF file.
            GenUtil.UtCvtName(Constant.SU_EQPT, Info.SdfName, out fullSuEqptTableName);

            //...Log2.v("\nUpdater.SuUpdateSDBEqpt(): fullSuEqptTableName = " + fullSuEqptTableName);

            // One time call to get the current date and time. These values are
            // used everwhere that needs to write the same date and/or time.
            GenUtil.UtGetDateTime(out modDate, out modTime);

            // Select all records in the user's sd_[sdfName]_eqpt table for subsequent fetch.
            curHandle = SuDynEqpt.SuSelectEqpt(fullSuEqptTableName, "", "");

            if (curHandle < 0)
            {
                Log2.e("\nUpdater.SuUpdateSDBEqpt(): ERROR: call to SuDynEqpt.SuSelectEqpt() failed, rc = " + curHandle);
                Console.Write("Error opening cursor - ");
                Console.Write("return code from suSelectEqpt is {0}\r\n", curHandle);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                return (Constant.FAILURE);
            }

            bool moreRecordsToFetch = true;
            bool successfulFetchLoop = false;

            // This is the fetch-loop that reads every record in the user's 
            // sd_[sdfName]_eqpt table.
            while (moreRecordsToFetch)
            {
                // Attempt to fetch an suEqpt record from the sd_[sdfName]_eqpt table.
                rc = SuDynEqpt.SuFetchEqpt(curHandle, out suEqpt, out suEqptNullInds);

                // Check the outcome of the fetch attempt.
                if (rc == Constant.SUCCESS)
                {
                    // The fetch succeeded. 
                    // Tumble out the bottom of this if-else construct.
                    //...Log2.v("\nUpdater.SuUpdateSDBEqpt(): Fetch succeeded:" + suEqpt.ToStringWN(suEqptNullInds));
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
                    Log2.e("\nUpdater.SuUpdateSDBEqpt(): ERROR: call to SuDynEqpt.SuFetchEqpt() failed, rc = " + rc);
                    successfulFetchLoop = false;
                    break;
                }

                // We successfully fetched a suEqpt record that needs processing so first 
                // increment its counter.
                recCounts[(int)Action.Processed]++;

                // Now process the suEqpt record according to its 'cmd' column value.
                switch (suEqpt.cmd[0])
                {
                    case 'A':

                        // Perform an SQL INSERT into the SDB table main.sd_eqpt
                        // Copy values from the SuEqpt record into a new SdEqpt record.
                        SdEqpt.MakeSdFromSu(suEqpt, suEqptNullInds, out sdEqpt, out sdEqptNullInds);

                        // Set the SdEqpt record's modify date and time.
                        sdEqpt.mdate = Info.Date;
                        sdEqpt.mtime = Info.Time;
                        sdEqptNullInds[SdEqpt.MDATE] = Constant.DB_NOT_NULL;
                        sdEqptNullInds[SdEqpt.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL INSERT of sdEqpt into main.sd_eqpt
                        rc = DynSdbEqpt.SdInsertEqpt(hConn, sdEqpt, sdEqptNullInds);

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

                        // Perform the SQL DELETE of sdEqpt from main.sd_eqpt
                        rc = DynSdbEqpt.SdDeleteEqpt(hConn, suEqpt.ecode);

                        if (rc == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Deleted]++;
                            Console.Write("::\t- Successful DELETE of equipment {0}\r\n", suEqpt.ecode);
                        }
                        else
                        {
                            rcUpdater = Constant.FAILURE;
                            errCount++;
                        }

                        break;

                    case 'U':

                        // Perform the SQL UPDATE of sdEqpt in main.sd_eqpt
                        // Copy values from the SuEqpt record into a new SdEqpt record.
                        SdEqpt.MakeSdFromSu(suEqpt, suEqptNullInds, out sdEqpt, out sdEqptNullInds);

                        // Set the SdEqpt record's modify date and time.
                        sdEqpt.mdate = Info.Date;
                        sdEqpt.mtime = Info.Time;
                        sdEqptNullInds[SdEqpt.MDATE] = Constant.DB_NOT_NULL;
                        sdEqptNullInds[SdEqpt.MTIME] = Constant.DB_NOT_NULL;

                        // Telecon with Claudia Cameron 14-May-2019.
                        //
                        // We must prevent the user from changing the 'bandbitpos' assignment 
                        // for an existing record in main.sd_eqpt. The following code fetches 
                        // the existing 'bandbitpos' for the prescribed 'ecode' and ensures 
                        // that its value is preserved through the update.
                        //
                        // We know that this application previously ran sdfValidate.exe on the
                        // su_XXX_eqpt table and validation was successful. Consequently, we
                        // make the simplifying assumption that a record with the prescribed
                        // ecode currently exists in main.sd_eqpt and we go and fetch it 
                        // any 'failure checking'.

                        //SdEqpt main_sdEqpt;
                        //SQLLEN[] tempNulls;

                        //if (DynSdbEqpt.FetchRecordWithEcode(suEqpt.ecode, out main_sdEqpt, out tempNulls))
                        //{

                        //}
                        //else
                        //{
                        //    // Call to DynSdbEqpt.FetchRecordWithEcode() failed.
                        //    rcUpdater = Constant.FAILURE;
                        //    Log2.e("\n\nUpdater.ProcessSuEqpt(): ERROR: call to DynSdbEqpt.FetchRecordWithEcode() failed for ecode = " + suEqpt.ecode);
                        //    Console.Write("\r\nERROR: SDF Eqpt update (U) record refers to ecode = '{0}' but no record");
                        //    Console.Write("\r\n       with this key exists in SDB table main.sd_main");
                        //    errCount++;
                        //    return Constant.FAILURE;
                        //}

                        // Perform the SQL UPDATE of sdEqpt in main.sd_eqpt
                        rc = DynSdbEqpt.SdUpdateEqpt(hConn, sdEqpt, sdEqptNullInds);

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

                        Console.Write("\r\nEQUIPMENT RECORD NO ACTION: :{0}: \r\n", suEqpt.ecode);

                        // No-operation.
                        recCounts[(int)Action.NoAction]++;

                        break;
                } // end of switch block.

            } //  end of suEqpt fetch-loop.

            // Handle case of an unsuccessful exit from the fetch-loop.
            if (!successfulFetchLoop)
            {
                // A serious SQL/ODBC error occurred; report it */
                Log2.e("\nUpdater.SuUpdateSDBEqpt(): ERROR: unsuccessful exit from fetch loop.");
                Console.Write("Error on fetch {0}\r\n", rc);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                rcUpdater = Constant.FAILURE;
            }

            if (SuDynEqpt.SuCloseEqpt(curHandle) != Constant.SUCCESS)
            {
                Log2.e("\nUpdater.SuUpdateSDBEqpt(): ERROR: call to SuDynEqpt.SuCloseEqpt() failed.");
                Console.Write("Ingres error on close \r\n");
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
            }

            return rcUpdater;
        }




    }
}


```
