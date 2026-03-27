using System;

namespace SdUpdateRout
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides the methods that perform the fetching of records from the
    /// prescribed user su_XXX_rout table and the subsequent changes made to the main 
    /// SDB table main.sd_rout i.a.w. the command ('cmd') field in each fetched record; 
    /// changes to the main SDB table records comprise SQL INSERT (cmd = 'A'), 
    /// UPDATE ('U'), DELETE ('D') operations and also No-Op ('N').
    /// </summary>
    public class Updater
    {
        private static string fullSuRoutTableName;      // Fully-qualified SQL table name 
        private static string modDate;                  // modify date - dd-mmm-yyyy 
        private static string modTime;                  // modify time - hh:mm 
        private static int curHandle;                   // suSelectRout handle 

        /// <summary>
        /// This method performs the top-level fetching of records from the
        /// prescribed user su_XXX_rout table and the subsequent changes made 
        /// to the main SDB table main.sd_rout i.a.w. the command ('cmd') field 
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
        public static int ProcessSuRout(SQLHDBC hConn,
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

            // The following objects will be used to fetch SuRout records from the user 
            // table [schema].sd_[sdfName]_rout.   
            SuRout suRout;
            SQLLEN[] suRoutNullInds;

            // The following objects will be used to insert/update/delete "detail" records
            // into/from the MDB table main.sd_rout. 
            SdRout sdRout;
            SQLLEN[] sdRoutNullInds;

            Console.Write("\r\n\t\tSDF ROUTE UPDATE RESULTS\r\n");

            // Get the names of the su_XXX_rout table that correspond to the 
            // prescribed SDF file.
            GenUtil.UtCvtName(Constant.SU_ROUT, Info.SdfName, out fullSuRoutTableName);

            //...Log2.v("\nUpdater.SuUpdateSDBRout(): fullSuRoutTableName = " + fullSuRoutTableName);

            // One time call to get the current date and time. These values are
            // used everwhere that needs to write the same date and/or time.
            GenUtil.UtGetDateTime(out modDate, out modTime);

            // Select all records in the user's sd_[sdfName]_rout table for subsequent fetch.
            curHandle = SuDynRout.SuSelectRout(fullSuRoutTableName, "", "");

            if (curHandle < 0)
            {
                Log2.e("\nUpdater.SuUpdateSDBRout(): ERROR: call to SuDynRout.SuSelectRout() failed, rc = " + curHandle);
                Console.Write("Error opening cursor - ");
                Console.Write("return code from suSelectRout is {0}\r\n", curHandle);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                return (Constant.FAILURE);
            }

            bool moreRecordsToFetch = true;
            bool successfulFetchLoop = false;

            // This is the fetch-loop that reads every record in the user's 
            // sd_[sdfName]_rout table.
            while (moreRecordsToFetch)
            {
                // Attempt to fetch an suRout record from the sd_[sdfName]_rout table.
                rc = SuDynRout.SuFetchRout(curHandle, out suRout, out suRoutNullInds);

                // Check the outcome of the fetch attempt.
                if (rc == Constant.SUCCESS)
                {
                    // The fetch succeeded. 
                    // Tumble out the bottom of this if-else construct.
                    //...Log2.v("\nUpdater.SuUpdateSDBRout(): Fetch succeeded:" + suRout.ToStringWN(suRoutNullInds));
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
                    Log2.e("\nUpdater.SuUpdateSDBRout(): ERROR: call to SuDynRout.SuFetchRout() failed, rc = " + rc);
                    successfulFetchLoop = false;
                    break;
                }

                // We successfully fetched a suRout record that needs processing so first 
                // increment its counter.
                recCounts[(int)Action.Processed]++;

                // Now process the suRout record according to its 'cmd' column value.
                switch (suRout.cmd[0])
                {
                    case 'A':

                        // Perform an SQL INSERT into the SDB table main.sd_rout
                        Console.Write("\r\nROUTE RECORD ADD: :{0}/{1}: \r\n", suRout.rcomp, suRout.routnumb);

                        // Copy values from the SuRout record into a new SdRout record.
                        SdRout.MakeSdFromSu(suRout, suRoutNullInds, out sdRout, out sdRoutNullInds);

                        // Set the SdRout record's modify date and time.
                        sdRout.mdate = Info.Date;
                        sdRout.mtime = Info.Time;
                        sdRoutNullInds[SdRout.MDATE] = Constant.DB_NOT_NULL;
                        sdRoutNullInds[SdRout.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL INSERT of sdRout into main.sd_rout
                        rc = DynSdbRout.SdInsertRout(hConn, sdRout, sdRoutNullInds);

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

                        // Perform the SQL DELETE of sdRout from main.sd_rout
                        rc = DynSdbRout.SdDeleteRout(hConn, suRout.rcomp, suRout.routnumb);

                        if (rc == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Deleted]++;
                            Console.Write("::\t- Successful DELETE of route {0}/{1}\r\n", suRout.rcomp, suRout.routnumb);
                        }
                        else
                        {
                            rcUpdater = Constant.FAILURE;
                            errCount++;
                        }

                        break;

                    case 'U':

                        // Perform the SQL UPDATE of sdRout in main.sd_rout
                        // Copy values from the SuRout record into a new SdRout record.
                        SdRout.MakeSdFromSu(suRout, suRoutNullInds, out sdRout, out sdRoutNullInds);

                        // Set the SdRout record's modify date and time.
                        sdRout.mdate = Info.Date;
                        sdRout.mtime = Info.Time;
                        sdRoutNullInds[SdRout.MDATE] = Constant.DB_NOT_NULL;
                        sdRoutNullInds[SdRout.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL UPDATE of sdRout in main.sd_rout
                        rc = DynSdbRout.SdUpdateRout(hConn, sdRout, sdRoutNullInds);

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

                        Console.Write("\r\nROUTE RECORD NO ACTION: :{0}/{1}: \r\n", suRout.rcomp, suRout.routnumb);

                        // No-operation.
                        recCounts[(int)Action.NoAction]++;

                        break;
                } // end of switch block.

            } //  end of suRout fetch-loop.

            // Handle case of an unsuccessful exit from the fetch-loop.
            if (!successfulFetchLoop)
            {
                // A serious SQL/ODBC error occurred; report it */
                Log2.e("\nUpdater.SuUpdateSDBRout(): ERROR: unsuccessful exit from fetch loop.");
                Console.Write("Error on fetch {0}\r\n", rc);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                rcUpdater = Constant.FAILURE;
            }

            if (SuDynRout.SuCloseRout(curHandle) != Constant.SUCCESS)
            {
                Log2.e("\nUpdater.SuUpdateSDBRout(): ERROR: call to SuDynRout.SuCloseRout() failed.");
                Console.Write("Ingres error on close \r\n");
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
            }

            return rcUpdater;
        }




    }
}

