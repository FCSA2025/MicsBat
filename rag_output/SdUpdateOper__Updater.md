# Documented File: Updater.cs
**Repository Path:** `SdUpdateOper\Updater.cs`
**Primary Layer:** `SdUpdateOper`
**Namespace:** `SdUpdateOper`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdateOper
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides the methods that perform the fetching of records from the
    /// prescribed user su_XXX_oper table and the subsequent changes made to the main 
    /// SDB table main.sd_oper i.a.w. the command ('cmd') field in each fetched record; 
    /// changes to the main SDB table records comprise SQL INSERT (cmd = 'A'), 
    /// UPDATE ('U'), DELETE ('D') operations and also No-Op ('N').
    /// </summary>
    public class Updater
    {
        private static string fullSuOperTableName;      // Fully-qualified SQL table name 
        private static string modDate;                  // modify date - dd-mmm-yyyy 
        private static string modTime;                  // modify time - hh:mm 
        private static int curHandle;                   // suSelectOper handle 

        /// <summary>
        /// This method performs the top-level fetching of records from the
        /// prescribed user su_XXX_oper table and the subsequent changes made 
        /// to the main SDB table main.sd_oper i.a.w. the command ('cmd') field 
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
        public static int ProcessSuOper(SQLHDBC hConn,
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

            // The following objects will be used to fetch SuOper records from the user 
            // table [schema].sd_[sdfName]_oper.   
            SuOper suOper;
            SQLLEN[] suOperNullInds;

            // The following objects will be used to insert/update/delete "detail" records
            // into/from the MDB table main.sd_oper. 
            SdOper sdOper;
            SQLLEN[] sdOperNullInds;

            Console.Write("\r\n\t\tSDF OPERATOR UPDATE RESULTS\r\n");

            // Get the names of the su_XXX_oper table that correspond to the 
            // prescribed SDF file.
            GenUtil.UtCvtName(Constant.SU_OPER, Info.SdfName, out fullSuOperTableName);

            //...Log2.v("\nUpdater.SuUpdateSDBOper(): fullSuOperTableName = " + fullSuOperTableName);

            // One time call to get the current date and time. These values are
            // used everwhere that needs to write the same date and/or time.
            GenUtil.UtGetDateTime(out modDate, out modTime);

            // Select all records in the user's sd_[sdfName]_oper table for subsequent fetch.
            curHandle = SuDynOper.SuSelectOper(fullSuOperTableName, "", "");

            if (curHandle < 0)
            {
                Log2.e("\nUpdater.SuUpdateSDBOper(): ERROR: call to SuDynOper.SuSelectOper() failed, rc = " + curHandle);
                Console.Write("Error opening cursor - ");
                Console.Write("return code from suSelectOper is {0}\r\n", curHandle);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                return (Constant.FAILURE);
            }

            bool moreRecordsToFetch = true;
            bool successfulFetchLoop = false;

            // This is the fetch-loop that reads every record in the user's 
            // sd_[sdfName]_oper table.
            while (moreRecordsToFetch)
            {
                // Attempt to fetch an suOper record from the sd_[sdfName]_oper table.
                rc = SuDynOper.SuFetchOper(curHandle, out suOper, out suOperNullInds);

                // Check the outcome of the fetch attempt.
                if (rc == Constant.SUCCESS)
                {
                    // The fetch succeeded. 
                    // Tumble out the bottom of this if-else construct.
                    //...Log2.v("\nUpdater.SuUpdateSDBOper(): Fetch succeeded:" + suOper.ToStringWN(suOperNullInds));
                }
                else if (rc == ODBC.SQL_NO_DATA)
                {
                    // There are no more records to fetch; break out of the while loop.
                    successfulFetchLoop = true;
                    //...Log2.v("\nUpdater.SuUpdateSDBOper(): rc == ODBC.SQL_NO_DATA");
                    break;
                }
                else
                {
                    // A serious ODBC/SQL error must have occurred; break out of the while loop.
                    Log2.e("\nUpdater.SuUpdateSDBOper(): ERROR: call to SuDynOper.SuFetchOper() failed, rc = " + rc);
                    successfulFetchLoop = false;
                    break;
                }

                // We successfully fetched a suOper record that needs processing so first 
                // increment its counter.
                recCounts[(int)Action.Processed]++;

                // Now process the suOper record according to its 'cmd' column value.
                switch (suOper.cmd[0])
                {
                    case 'A':

                        // Perform an SQL INSERT into the SDB table main.sd_oper
                        // Copy values from the SuOper record into a new SdOper record.
                        SdOper.MakeSdFromSu(suOper, suOperNullInds, out sdOper, out sdOperNullInds);

                        // Set the SdOper record's modify date and time.
                        sdOper.mdate = Info.Date;
                        sdOper.mtime = Info.Time;
                        sdOperNullInds[SdOper.MDATE] = Constant.DB_NOT_NULL;
                        sdOperNullInds[SdOper.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL INSERT of sdOper into main.sd_oper
                        rc = DynSdbOper.SdInsertOper(hConn, sdOper, sdOperNullInds);

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

                        // Perform the SQL DELETE of sdOper from main.sd_oper
                        rc = DynSdbOper.SdDeleteOper(hConn, suOper.oper);

                        if (rc == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Deleted]++;
                            Console.Write("::\t- Successful DELETE of operator {0}\r\n", suOper.oper);
                        }
                        else
                        {
                            rcUpdater = Constant.FAILURE;
                            errCount++;
                        }

                        break;

                    case 'U':

                        // Perform the SQL UPDATE of sdOper in main.sd_oper
                        // Copy values from the SuOper record into a new SdOper record.
                        SdOper.MakeSdFromSu(suOper, suOperNullInds, out sdOper, out sdOperNullInds);

                        // Set the SdOper record's modify date and time.
                        sdOper.mdate = Info.Date;
                        sdOper.mtime = Info.Time;
                        sdOperNullInds[SdOper.MDATE] = Constant.DB_NOT_NULL;
                        sdOperNullInds[SdOper.MTIME] = Constant.DB_NOT_NULL;

                        // Telecon with Claudia Cameron 14-May-2019.
                        //
                        // We must prevent the user from changing the 'bandbitpos' assignment 
                        // for an existing record in main.sd_oper. The following code fetches 
                        // the existing 'bandbitpos' for the prescribed 'ecode' and ensures 
                        // that its value is preserved through the update.
                        //
                        // We know that this application previously ran sdfValidate.exe on the
                        // su_XXX_oper table and validation was successful. Consequently, we
                        // make the simplifying assumption that a record with the prescribed
                        // ecode currently exists in main.sd_oper and we go and fetch it 
                        // any 'failure checking'.

                        //SdOper main_sdOper;
                        //SQLLEN[] tempNulls;

                        //if (DynSdbOper.FetchRecordWithEcode(suOper.ecode, out main_sdOper, out tempNulls))
                        //{

                        //}
                        //else
                        //{
                        //    // Call to DynSdbOper.FetchRecordWithEcode() failed.
                        //    rcUpdater = Constant.FAILURE;
                        //    Log2.e("\n\nUpdater.ProcessSuOper(): ERROR: call to DynSdbOper.FetchRecordWithEcode() failed for ecode = " + suOper.ecode);
                        //    Console.Write("\r\nERROR: SDF Oper update (U) record refers to ecode = '{0}' but no record");
                        //    Console.Write("\r\n       with this key exists in SDB table main.sd_main");
                        //    errCount++;
                        //    return Constant.FAILURE;
                        //}

                        // Perform the SQL UPDATE of sdOper in main.sd_oper
                        rc = DynSdbOper.SdUpdateOper(hConn, sdOper, sdOperNullInds);

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

                        Console.Write("\r\nOPERATOR RECORD NO ACTION: :{0}: \r\n", suOper.oper);

                        // No-operation.
                        recCounts[(int)Action.NoAction]++;

                        break;
                } // end of switch block.

            } //  end of suOper fetch-loop.

            // Handle case of an unsuccessful exit from the fetch-loop.
            if (!successfulFetchLoop)
            {
                // A serious SQL/ODBC error occurred; report it */
                Log2.e("\nUpdater.SuUpdateSDBOper(): ERROR: unsuccessful exit from fetch loop.");
                Console.Write("Error on fetch {0}\r\n", rc);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                rcUpdater = Constant.FAILURE;
            }

            if (SuDynOper.SuCloseOper(curHandle) != Constant.SUCCESS)
            {
                Log2.e("\nUpdater.SuUpdateSDBOper(): ERROR: call to SuDynOper.SuCloseOper() failed.");
                Console.Write("Ingres error on close \r\n");
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
            }

            return rcUpdater;
        }




    }
}


```
