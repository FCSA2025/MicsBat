using System;

namespace SdUpdateBand
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides the methods that perform the fetching of records from the
    /// prescribed user su_XXX_band table and the subsequent changes made to the main 
    /// SDB table main.sd_band i.a.w. the command ('cmd') field in each fetched record; 
    /// changes to the main SDB table records comprise SQL INSERT (cmd = 'A'), 
    /// UPDATE ('U'), DELETE ('D') operations and also No-Op ('N').
    /// </summary>
    public class Updater
    {
        private static string fullSuBandTableName;      // Fully-qualified SQL table name 
        private static string modDate;                  // modify date - dd-mmm-yyyy 
        private static string modTime;                  // modify time - hh:mm 
        private static int curHandle;                   // suSelectBand handle 

        /// <summary>
        /// This method performs the top-level fetching of records from the
        /// prescribed user su_XXX_band table and the subsequent changes made 
        /// to the main SDB table main.sd_band i.a.w. the command ('cmd') field 
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
        public static int ProcessSuBand(SQLHDBC hConn,
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

            int rcUpdtBand = Constant.SUCCESS;
            int rc;

            // The following objects will be used to fetch SuBand records from the user 
            // table [schema].sd_[sdfName]_band.   
            SuBand suBand;
            SQLLEN[] suBandNullInds;

            // The following objects will be used to insert/update/delete "detail" records
            // into/from the MDB table main.sd_band. 
            SdBand sdBand;
            SQLLEN[] sdBandNullInds;

            Console.Write("\r\n\t\tSDF BAND UPDATE RESULTS\r\n");

            // Get the names of the su_XXX_band table that correspond to the 
            // prescribed SDF file.
            GenUtil.UtCvtName(Constant.SU_BAND, Info.SdfName, out fullSuBandTableName);

            //...Log2.v("\nUpdtBand.SuUpdateSDBBand(): fullSuBandTableName = " + fullSuBandTableName);

            // One time call to get the current date and time. These values are
            // used everwhere that needs to write the same date and/or time.
            GenUtil.UtGetDateTime(out modDate, out modTime);

            // One time call to get next available band bit position in the
            // sd_band table & use this value to assign sequential numbers
            // to the add transactions in the sdf
            if ((rc = BandBitPosManager.Initialize()) != Constant.SUCCESS)
            {
                // Failed to get the number of the next bit position.
                return (rc);
            }

            // Select all records in the user's sd_[sdfName]_band table for subsequent fetch.
            curHandle = SuDynBand.SuSelectBand(fullSuBandTableName, "", "");

            if (curHandle < 0)
            {
                Log2.e("\nUpdtBand.SuUpdateSDBBand(): ERROR: call to SuDynBand.SuSelectBand() failed, rc = " + curHandle);
                Console.Write("Error opening cursor - ");
                Console.Write("return code from suSelectBand is {0}\r\n", curHandle);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                return (Constant.FAILURE);
            }

            bool moreRecordsToFetch = true;
            bool successfulFetchLoop = false;

            // This is the fetch-loop that reads every record in the user's 
            // sd_[sdfName]_band table.
            while (moreRecordsToFetch)
            {
                // Attempt to fetch an suBand record from the sd_[sdfName]_band table.
                rc = SuDynBand.SuFetchBand(curHandle, out suBand, out suBandNullInds);

                // Check the outcome of the fetch attempt.
                if (rc == Constant.SUCCESS)
                {
                    // The fetch succeeded. 
                    // Tumble out the bottom of this if-else construct.
                    //...Log2.v("\nUpdtBand.SuUpdateSDBBand(): Fetch succeeded:" + suBand.ToStringWN(suBandNullInds));
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
                    Log2.e("\nUpdtBand.SuUpdateSDBBand(): ERROR: call to SuDynBand.SuFetchBand() failed, rc = " + rc);
                    successfulFetchLoop = false;
                    break;
                }

                // We successfully fetched a suBand record that needs processing so first 
                // increment its counter.
                recCounts[(int)Action.Processed]++;

                // Now process the suBand record according to its 'cmd' column value.
                switch (suBand.cmd[0])
                {
                    case 'A':

                        // Perform an SQL INSERT into the SDB table main.sd_band
                        // Copy values from the SuBand record into a new SdBand record.
                        SdBand.MakeSdFromSu(suBand, suBandNullInds, out sdBand, out sdBandNullInds);

                        // Set the SdBand record's modify date and time.
                        sdBand.mdate = Info.Date;
                        sdBand.mtime = Info.Time;
                        sdBandNullInds[SdBand.MDATE] = Constant.DB_NOT_NULL;
                        sdBandNullInds[SdBand.MTIME] = Constant.DB_NOT_NULL;

                        // We also need to assign a unique bit position to the new bndcde.
                        sdBand.bandbitpos = (short)BandBitPosManager.GetNextAvailable();
                        sdBandNullInds[SdBand.BANDBITPOS] = Constant.DB_NOT_NULL;

                        // Perform the SQL INSERT of sdBand into main.sd_band
                        rc = DynSdbBand.SdInsertBand(hConn, sdBand, sdBandNullInds);

                        if (rc == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Added]++;
                            BandBitPosManager.Commit();
                        }
                        else
                        {
                            rcUpdtBand = Constant.FAILURE;
                        }

                        break;

                    case 'D':

                        // Perform the SQL DELETE of sdBand from main.sd_band
                        rc = DynSdbBand.SdDeleteBand(hConn, suBand.bndcde);

                        if (rc == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Deleted]++;
                        }
                        else
                        {
                            rcUpdtBand = Constant.FAILURE;
                        }

                        break;

                    case 'U':

                        // Perform the SQL UPDATE of sdBand in main.sd_band
                        // Copy values from the SuBand record into a new SdBand record.
                        SdBand.MakeSdFromSu(suBand, suBandNullInds, out sdBand, out sdBandNullInds);

                        // Set the SdBand record's modify date and time.
                        sdBand.mdate = Info.Date;
                        sdBand.mtime = Info.Time;
                        sdBandNullInds[SdBand.MDATE] = Constant.DB_NOT_NULL;
                        sdBandNullInds[SdBand.MTIME] = Constant.DB_NOT_NULL;

                        // Telecon with Claudia Cameron 14-May-2019.
                        //
                        // We must prevent the user from changing the 'bandbitpos' assignment 
                        // for an existing record in main.sd_band. The following code fetches 
                        // the existing 'bandbitpos' for the prescribed 'bndcde' and ensures 
                        // that its value is preserved through the update.
                        //
                        // We know that this application previously ran sdfValidate.exe on the
                        // su_XXX_band table and validation was successful. Consequently, we
                        // make the simplifying assumption that a record with the prescribed
                        // bndcde currently exists in main.sd_band and we go and fetch it 
                        // any 'failure checking'.

                        SdBand main_sdBand;
                        SQLLEN[] tempNulls;

                        if (DynSdbBand.FetchRecordWithBndCde(suBand.bndcde, out main_sdBand, out tempNulls))
                        {
                            // Perform the override to ensure preservation of the existing main.sd_band 
                            // bandbitpos for the prescribed bndcde.
                            sdBand.bandbitpos = main_sdBand.bandbitpos;
                            sdBandNullInds[SdBand.BANDBITPOS] = Constant.DB_NOT_NULL;

                            // Warn the user if the SDF record's bandbitpos does not match the
                            // existing value in main.sd_band for the prescribed bndcde.
                            bool bandbitposIsOK = (suBandNullInds[SuBand.BANDBITPOS] != Constant.DB_NULL) &&
                                                        (suBand.bandbitpos == main_sdBand.bandbitpos);

                            if (!bandbitposIsOK)
                            {
                                string suBandBitPosStr = (suBandNullInds[SuBand.BANDBITPOS] == Constant.DB_NULL ? "NULL" : suBand.bandbitpos.ToString());

                                Console.Write("\r\nWARNING: a SDF Band update (U) record is attempting to change the 'bandbitpos' for bndcde = {0} from {1} to {2}.",
                                                suBand.bndcde, main_sdBand.bandbitpos, suBandBitPosStr);
                                Console.Write("\r\n         THIS IS NOT ALLOWED. The existing 'bandbitpos' value will be preserved in the update.\r\n");

                                warnCount++;

                                //...Log2.v("\n\nUpdater.ProcessSuBand(): MISMATCH of bandbitpos values for update (U) of record for bndcde = " + suBand.bndcde);
                            }
                        }
                        else
                        {
                            // Call to DynSdbBand.FetchRecordWithBndCde() failed.
                            rcUpdtBand = Constant.FAILURE;
                            Log2.e("\n\nUpdater.ProcessSuBand(): ERROR: call to DynSdbBand.FetchRecordWithBndCde() failed for bndcde = " + suBand.bndcde);
                            Console.Write("\r\nERROR: SDF Band update (U) record refers to bdcde = '{0}' but no record");
                            Console.Write("\r\n       with this key exists in SDB table main.sd_main");
                            errCount++;
                            return Constant.FAILURE;
                        }

                        // Perform the SQL UPDATE of sdBand in main.sd_band
                        rc = DynSdbBand.SdUpdateBand(hConn, sdBand, sdBandNullInds);

                        if (rc == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Updated]++;
                        }
                        else
                        {
                            rcUpdtBand = Constant.FAILURE;
                        }

                        break;

                    case 'N':

                        Console.Write("\r\nBAND RECORD NO ACTION: :{0}: \r\n", suBand.bndcde);

                        // No-operation.
                        recCounts[(int)Action.NoAction]++;

                        break;
                } // end of switch block.

            } //  end of suBand fetch-loop.

            // Handle case of an unsuccessful exit from the fetch-loop.
            if (!successfulFetchLoop)
            {
                // A serious SQL/ODBC error occurred; report it */
                Log2.e("\nUpdtBand.SuUpdateSDBBand(): ERROR: unsuccessful exit from fetch loop.");
                Console.Write("Error on fetch {0}\r\n", rc);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                rcUpdtBand = Constant.FAILURE;
            }

            if (SuDynBand.SuCloseBand(curHandle) != Constant.SUCCESS)
            {
                Log2.e("\nUpdtBand.SuUpdateSDBBand(): ERROR: call to SuDynBand.SuCloseBand() failed.");
                Console.Write("Ingres error on close \r\n");
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
            }

            return rcUpdtBand;
        }




    }
}

