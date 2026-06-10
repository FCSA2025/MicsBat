# Documented File: Updater.cs
**Repository Path:** `SdUpdateNote\Updater.cs`
**Primary Layer:** `SdUpdateNote`
**Namespace:** `SdUpdateNote`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdateNote
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides the methods that perform the fetching of records from the
    /// prescribed user su_XXX_note table and the subsequent changes made to the main 
    /// SDB table main.sd_note i.a.w. the command ('cmd') field in each fetched record; 
    /// changes to the main SDB table records comprise SQL INSERT (cmd = 'A'), 
    /// UPDATE ('U'), DELETE ('D') operations and also No-Op ('N').
    /// </summary>
    public class Updater
    {
        private static string fullSuNoteTableName;      // Fully-qualified SQL table name 
        private static string modDate;                  // modify date - dd-mmm-yyyy 
        private static string modTime;                  // modify time - hh:mm 
        private static int curHandle;                   // suSelectNote handle 

        /// <summary>
        /// This method performs the top-level fetching of records from the
        /// prescribed user su_XXX_note table and the subsequent changes made 
        /// to the main SDB table main.sd_note i.a.w. the command ('cmd') field 
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
        public static int ProcessSuNote(SQLHDBC hConn,
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

            // The following objects will be used to fetch SuNote records from the user 
            // table [schema].sd_[sdfName]_note.   
            SuNote suNote;
            SQLLEN[] suNoteNullInds;

            // The following objects will be used to insert/update/delete "detail" records
            // into/from the MDB table main.sd_note. 
            SdNote sdNote;
            SQLLEN[] sdNoteNullInds;

            Console.Write("\r\n\t\tSDF NOTE UPDATE RESULTS\r\n");

            // Get the names of the su_XXX_note table that correspond to the 
            // prescribed SDF file.
            GenUtil.UtCvtName(Constant.SU_NOTE, Info.SdfName, out fullSuNoteTableName);

            //...Log2.v("\nUpdater.SuUpdateSDBNote(): fullSuNoteTableName = " + fullSuNoteTableName);

            // One time call to get the current date and time. These values are
            // used everwhere that needs to write the same date and/or time.
            GenUtil.UtGetDateTime(out modDate, out modTime);

            // Select all records in the user's sd_[sdfName]_note table for subsequent fetch.
            curHandle = SuDynNote.SuSelectNote(fullSuNoteTableName, "", "");

            if (curHandle < 0)
            {
                Log2.e("\nUpdater.SuUpdateSDBNote(): ERROR: call to SuDynNote.SuSelectNote() failed, rc = " + curHandle);
                Console.Write("Error opening cursor - ");
                Console.Write("return code from suSelectNote is {0}\r\n", curHandle);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                return (Constant.FAILURE);
            }

            bool moreRecordsToFetch = true;
            bool successfulFetchLoop = false;

            // This is the fetch-loop that reads every record in the user's 
            // sd_[sdfName]_note table.
            while (moreRecordsToFetch)
            {
                // Attempt to fetch an suNote record from the sd_[sdfName]_note table.
                rc = SuDynNote.SuFetchNote(curHandle, out suNote, out suNoteNullInds);

                // Check the outcome of the fetch attempt.
                if (rc == Constant.SUCCESS)
                {
                    // The fetch succeeded. 
                    // Tumble out the bottom of this if-else construct.
                    //...Log2.v("\nUpdater.SuUpdateSDBNote(): Fetch succeeded:" + suNote.ToStringWN(suNoteNullInds));
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
                    Log2.e("\nUpdater.SuUpdateSDBNote(): ERROR: call to SuDynNote.SuFetchNote() failed, rc = " + rc);
                    successfulFetchLoop = false;
                    break;
                }

                // We successfully fetched a suNote record that needs processing so first 
                // increment its counter.
                recCounts[(int)Action.Processed]++;

                // Now process the suNote record according to its 'cmd' column value.
                switch (suNote.cmd[0])
                {
                    case 'A':

                        // Perform an SQL INSERT into the SDB table main.sd_note
                        // Copy values from the SuNote record into a new SdNote record.
                        SdNote.MakeSdFromSu(suNote, suNoteNullInds, out sdNote, out sdNoteNullInds);

                        // Set the SdNote record's modify date and time.
                        sdNote.mdate = Info.Date;
                        sdNote.mtime = Info.Time;
                        sdNoteNullInds[SdNote.MDATE] = Constant.DB_NOT_NULL;
                        sdNoteNullInds[SdNote.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL INSERT of sdNote into main.sd_note
                        rc = DynSdbNote.SdInsertNote(hConn, sdNote, sdNoteNullInds);

                        if (rc == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Added]++;
                            Console.Write("\r\nNOTE RECORD ADD: :{0}-{1}: \r\n", suNote.oper, suNote.nonum);
                        }
                        else
                        {
                            rcUpdater = Constant.FAILURE;
                        }

                        break;

                    case 'D':

                        // Perform the SQL DELETE of sdNote from main.sd_note
                        rc = DynSdbNote.SdDeleteNote(hConn, suNote.oper, suNote.nonum);

                        if (rc == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Deleted]++;
                            Console.Write("::\t- Successful DELETE of note {0}-{1}\r\n", suNote.oper, suNote.nonum);
                        }
                        else
                        {
                            rcUpdater = Constant.FAILURE;
                            errCount++;
                        }

                        break;

                    case 'U':

                        // Perform the SQL UPDATE of sdNote in main.sd_note
                        // Copy values from the SuNote record into a new SdNote record.
                        SdNote.MakeSdFromSu(suNote, suNoteNullInds, out sdNote, out sdNoteNullInds);

                        // Set the SdNote record's modify date and time.
                        sdNote.mdate = Info.Date;
                        sdNote.mtime = Info.Time;
                        sdNoteNullInds[SdNote.MDATE] = Constant.DB_NOT_NULL;
                        sdNoteNullInds[SdNote.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL UPDATE of sdNote in main.sd_note
                        rc = DynSdbNote.SdUpdateNote(hConn, sdNote, sdNoteNullInds);

                        if (rc == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Updated]++;
                            Console.Write("\r\nNOTE RECORD UPDATE: :{0}-{1}: \r\n", suNote.oper, suNote.nonum);
                        }
                        else
                        {
                            rcUpdater = Constant.FAILURE;
                        }

                        break;

                    case 'N':

                        Console.Write("\r\nNOTE RECORD NO ACTION: :{0}-{1}: \r\n", suNote.oper, suNote.nonum);

                        // No-operation.
                        recCounts[(int)Action.NoAction]++;

                        break;
                } // end of switch block.

            } //  end of suNote fetch-loop.

            // Handle case of an unsuccessful exit from the fetch-loop.
            if (!successfulFetchLoop)
            {
                // A serious SQL/ODBC error occurred; report it */
                Log2.e("\nUpdater.SuUpdateSDBNote(): ERROR: unsuccessful exit from fetch loop.");
                Console.Write("Error on fetch {0}\r\n", rc);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                rcUpdater = Constant.FAILURE;
            }

            if (SuDynNote.SuCloseNote(curHandle) != Constant.SUCCESS)
            {
                Log2.e("\nUpdater.SuUpdateSDBNote(): ERROR: call to SuDynNote.SuCloseNote() failed.");
                Console.Write("Ingres error on close \r\n");
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
            }

            return rcUpdater;
        }




    }
}



```
