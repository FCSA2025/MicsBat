# Documented File: Updater.cs
**Repository Path:** `SdUpdateCtx\Updater.cs`
**Primary Layer:** `SdUpdateCtx`
**Namespace:** `SdUpdateCtx`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdateCtx
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using System.Runtime.InteropServices;
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides the methods that perform the fetching of records from the
    /// prescribed user su_XXX_ctx_ and su_XXX_ctxd tables and the subsequent changes
    /// made to the main SDB tables main.sd_ctx_ and main.sd_ctxd i.a.w. the command
    /// ('cmd') fields in each fetched record; changes to the main SDB table records comprise
    /// SQL INSERT (cmd = 'A'), UPDATE ('U'), DELETE ('D') operations and also No-Op ('N').
    /// </summary>
    public class Updater
    {
        private static string intFileNameMaster;        /* Internal Ingres name */
        private static string intFileNameDetail;        /* Internal Ingres name */
        //private static string modDate;                  /* modify date - dd-mmm-yyyy */
        //private static string modTime;                  /* modify time - hh:mm */
        private static int rcMaster;                    /* master update return code */
        private static int rcDetail;                    /* detail update return code */
        private static int curHandleMaster;             /* suSelectCtx handle */

        /// <summary>
        /// This method performs the top-level fetching of records from the
        /// prescribed user su_XXX_ctx_ and su_XXX_ctxd tables and the subsequent changes
        /// made to the main SDB tables main.sd_ctx_ and main.sd_ctxd i.a.w. the command
        /// ('cmd') fields in each fetched record; changes to the main SDB table records comprise
        /// SQL INSERT (cmd = 'A'), UPDATE ('U'), DELETE ('D') operations and also No-Op ('N').
        /// </summary>
        /// <param name="hConn"></param>
        /// <param name="sdfName"></param>
        /// <param name="errCount"></param>
        /// <param name="warnCount"></param>
        /// <param name="recCounts"></param>
        /// <param name="detCounts"></param>
        /// <returns></returns>
        public static int ProcessSuCtxCtxD(SQLHDBC hConn,
                                            string sdfName,
                                            out int errCount,
                                            out int warnCount,
                                            out int[] recCounts,
                                            out int[] detCounts)
        {
            // Initialize the counters.
            const int NDIM = 5;
            recCounts = Arrays.CreateAndFillArray(NDIM, 0);
            detCounts = Arrays.CreateAndFillArray(NDIM, 0);
            errCount = 0;
            warnCount = 0;

            int rcUpdtCtx = Constant.SUCCESS;
            int nRet;
            int countDataPts;

            // The following objects will be used to fetch "Master" records from the user's 
            // table [schema].sd_[sdfName]_ctx_.   
            SuCtx suCtx;
            SQLLEN[] suCtxNullInds;

            // The following objects will be used to insert/update/delete "detail" records
            // into/from the MDB table main.sd_ctxd. 
            SdCtx sdCtx;
            SQLLEN[] sdCtxNullInds;

            Console.Write("\r\n\t\tSDF CTX UPDATE RESULTS\r\n");

            // Get the names of the _ctx_ and _ctxd tables that correspond to the 
            // prescribed SDF file.
            GenUtil.UtCvtName(Constant.SU_CTX, Info.SdfName, out intFileNameMaster);
            GenUtil.UtCvtName(Constant.SU_CTXD, Info.SdfName, out intFileNameDetail);

            //...Log2.v("\nUpdater.SuUpdateSDBCtx(): intFileNameMaster = " + intFileNameMaster);
            //...Log2.v("\nUpdater.SuUpdateSDBCtx(): intFileNameDetail = " + intFileNameDetail);

            // Select all records in the user's sd_[sdfName]_ctx_ table for subsequent fetch.
            curHandleMaster = SuDynCtx.SuSelectCtx(intFileNameMaster, "", "");

            if (curHandleMaster < 0)
            {
                Log2.e("\nUpdtCtx.SuUpdateSDBCtx(): ERROR: call to SuDynCtx.SuSelectCtx() failed, rc = " + curHandleMaster);
                Console.Write("Error opening cursor - ");
                Console.Write("return code from suSelectCtx is {0}\r\n", curHandleMaster);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                return (Constant.FAILURE);
            }

            bool moreRecordsToFetch = true;
            bool successfulFetchLoop = false;
            int rc = 0;
            int ctxFetchCount = 0;

            // This is the fetch-loop that reads every record in the user's 
            // sd_[sdfName]_ctx_ table.
            while (moreRecordsToFetch)
            {
                // Attempt to fetch an suCtx 'master' record from the sd_[sdfName]_ctx_ table.
                rc = SuDynCtx.SuFetchCtx(curHandleMaster, out suCtx, out suCtxNullInds);

                // Check the outcome of the fetch attempt.
                if (rc == Constant.SUCCESS)
                {
                    // The fetch succeeded. 
                    // Update the fetch count and tumble out the bottom of this if-else construct.
                    //...Log2.v("\nUpdater.SuUpdateSDBCtx(): Fetch succeeded:" + suCtx.ToStringWN(suCtxNullInds));
                    ctxFetchCount++;
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
                    Log2.e("\nUpdtCtx.SuUpdateSDBCtx(): ERROR: call to SuDynCtx.SuFetchCtx() failed, rc = " + rc);
                    successfulFetchLoop = false;
                    break;
                }

                /* Set the return codes to success prior to updates */
                rcDetail = Constant.SUCCESS;
                rcMaster = Constant.SUCCESS;

                // We successfully fetched a suCtx record that needs processing so first 
                // increment its counter.
                recCounts[(int)Action.Processed]++;

                // Write 'action' on record to the Console.
                switch (suCtx.cmd[0])
                {
                    case 'A':
                        Console.Write("\r\nCTX RECORD ADD: :{0}/{1}/{2}: \r\n", suCtx.tfcr, suCtx.tfci, suCtx.rxeqp);
                        break;
                    case 'D':
                        Console.Write("\r\nCTX RECORD DELETE: :{0}/{1}/{2}: \r\n", suCtx.tfcr, suCtx.tfci, suCtx.rxeqp);
                        break;
                    case 'B':
                    case 'U':
                        Console.Write("\r\nCTX RECORD UPDATE: :{0}/{1}/{2}: \r\n", suCtx.tfcr, suCtx.tfci, suCtx.rxeqp);
                        break;
                    case 'N':
                        Console.Write("\r\nCTX RECORD NO ACTION: :{0}/{1}/{2}: \r\n", suCtx.tfcr, suCtx.tfci, suCtx.rxeqp);
                        break;
                    default:
                        break;
                }

                // The following method call performs the appropriate Add/Delete/Update
                // operations on 'detail' records fetched from the user's sd_[sdfName]_ctxd 
                // table that have key values that match the 'master' record.
                rcDetail = ProcessSuCtxD(hConn,
                                            suCtx.tfcr,
                                            suCtx.tfci,
                                            suCtx.rxeqp,
                                            suCtx.cmd,
                                            intFileNameDetail,
                                            ref errCount,
                                            ref detCounts);

                // Now process the suCtx record according to its 'cmd' column value.
                switch (suCtx.cmd[0])
                {
                    case 'A':

                        // Check that the number of detail records in main.sd_ctxd that have the
                        // prescribed master's key is within bounds.
                        nRet = ValidDetailRecordCount(hConn, suCtx.tfcr, suCtx.tfci, suCtx.rxeqp, ref errCount, ref warnCount, out countDataPts);
                        if (nRet != Constant.SUCCESS)
                        {
                            rcUpdtCtx = Constant.FAILURE;
                        }

                        // Perform an SQL INSERT into the SDB table main.sd_ctx
                        // Copy values from the SuCtx record into a new SdCtx record.
                        SdCtx.MakeSdFromSu(suCtx, suCtxNullInds, out sdCtx, out sdCtxNullInds);

                        // Set the SdCtx record's modify date and time.
                        sdCtx.mdate = Info.Date;
                        sdCtx.mtime = Info.Time;
                        sdCtxNullInds[SdCtx.MDATE] = Constant.DB_NOT_NULL;
                        sdCtxNullInds[SdCtx.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL INSERT of sdCtx into main.sd_ctx_
                        rcMaster = DynSdbCtx.SdInsertCtx(hConn, sdCtx, sdCtxNullInds);

                        if (rcMaster == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Added]++;
                        }
                        else
                        {
                            rcUpdtCtx = Constant.FAILURE;
                            errCount++;
                        }

                        break;

                    case 'D':

                        // There is no need to check that the number of detail records in main.sd_ctxd that 
                        // have the prescribed key values is within bounds because our next action is
                        // to delete them all.

                        // Delete any detail records belonging to the master record and then
                        // delete the master record itself.

                        // Detail records: perform the SQL DELETE of a SET of sdCtxd from main.sd_ctxd.
                        //                 Some records may have been deleted by the previous call to 
                        //                 ProcessSuCtxD() and we now delete any that are remaining.
                        string where = String.Format(" tfcr = '{0}' AND tfci = '{1}' AND rxeqp = '{2}' ", suCtx.tfcr, suCtx.tfci, suCtx.rxeqp);
                        int rowCount = Ssutil.DbDeleteRowsConn(hConn, DynSdbCtxD.TableName, where);

                        //...Log2.v("\nSdUpdateAnte.ProcessSuCtxCtxD(): where = " + where);
                        //...Log2.v("\nSdUpdateAnte.ProcessSuCtxCtxD(): rowCount = " + rowCount);

                        if (rowCount < 0)
                        {
                            // An error occurred.
                            Console.Write("::\t- Error {0} on DELETE of CTX points\r\n{1}\r\n", rowCount, GenUtil.GetUserMess());
                            rcUpdtCtx = Constant.FAILURE;
                            errCount++;
                        }
                        else
                        {
                            // Update the detail counts.
                            detCounts[(int)Action.Processed] += rowCount;
                            detCounts[(int)Action.Deleted] += rowCount;
                        }

                        // Master record: perform the SQL DELETE of sdCtx from main.sd_ctx_
                        rcMaster = DynSdbCtx.SdDeleteCtx(hConn, suCtx.tfcr, suCtx.tfci, suCtx.rxeqp);

                        if (rcMaster == Constant.SUCCESS)
                        {
                            Console.Write("::\t- Successful DELETE of master and points records\r\n");
                            recCounts[(int)Action.Deleted]++;
                        }
                        else
                        {
                            rcUpdtCtx = Constant.FAILURE;
                            errCount++;
                        }

                        break;

                    case 'B':
                    case 'U':

                        // Check that the number of detail records in main.sd_ctxd that have the
                        // prescribed master's key is within bounds.
                        nRet = ValidDetailRecordCount(hConn, suCtx.tfcr, suCtx.tfci, suCtx.rxeqp, ref errCount, ref warnCount, out countDataPts);
                        if (nRet != Constant.SUCCESS)
                        {
                            rcUpdtCtx = Constant.FAILURE;
                        }

                        // Perform the SQL UPDATE of sdCtx in main.sd_ctx
                        // Copy values from the SuCtx record into a new SdCtx record.
                        SdCtx.MakeSdFromSu(suCtx, suCtxNullInds, out sdCtx, out sdCtxNullInds);

                        // Set the SdCtx record's modify date and time.
                        sdCtx.mdate = Info.Date;
                        sdCtx.mtime = Info.Time;
                        sdCtxNullInds[SdCtx.MDATE] = Constant.DB_NOT_NULL;
                        sdCtxNullInds[SdCtx.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL UPDATE of sdCtx in main.sd_ctx_
                        rcMaster = DynSdbCtx.SdUpdateCtx(hConn, sdCtx, sdCtxNullInds);

                        if (rcMaster == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Updated]++;
                        }
                        else
                        {
                            rcUpdtCtx = Constant.FAILURE;
                            errCount++;
                        }

                        break;

                    case 'N':

                        if (rcDetail == Constant.SUCCESS)
                        {
                            if (SomeDetailChanges(detCounts))
                            {
                                // Check that the number of detail records in main.sd_ctxd that have the
                                // prescribed key values is within bounds.
                                nRet = ValidDetailRecordCount(hConn, suCtx.tfcr, suCtx.tfci, suCtx.rxeqp, ref errCount, ref warnCount, out countDataPts);
                                if (nRet != Constant.SUCCESS)
                                {
                                    rcUpdtCtx = Constant.FAILURE;
                                }

                                // Perform an SQL UPDATE on the record in the SDB table main.sd_ctx_
                                // Copy values from the SuCtx record into a new SdCtx record.
                                SdCtx.MakeSdFromSu(suCtx, suCtxNullInds, out sdCtx, out sdCtxNullInds);

                                // Set the SdCtx record's ctxndp and modification date and time fields.
                                sdCtx.ctxndp = (short)countDataPts;
                                sdCtx.mdate = Info.Date;
                                sdCtx.mtime = Info.Time;

                                sdCtxNullInds[SdCtx.CTXNDP] = Constant.DB_NOT_NULL;
                                sdCtxNullInds[SdCtx.MDATE] = Constant.DB_NOT_NULL;
                                sdCtxNullInds[SdCtx.MTIME] = Constant.DB_NOT_NULL;

                                // Perform the SQL UPDATE of sdCtx into main.sd_ctx
                                rcMaster = DynSdbCtx.SdUpdateCtx(hConn, sdCtx, sdCtxNullInds);

                                if (rcMaster != Constant.SUCCESS)
                                {
                                    rcUpdtCtx = Constant.FAILURE;
                                    errCount++;
                                }
                            }
                        } //if rcDetail

                        if (rcMaster == Constant.SUCCESS && rcDetail == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.NoAction]++;
                        }

                        break;
                } // end of switch block.


                /* check the return code on the master and detail updates */
                if ((rcMaster == Constant.FAILURE) || (rcDetail == Constant.FAILURE))
                {
                    /* if either update fails, set return code to Constant.FAILURE */
                    rcUpdtCtx = Constant.FAILURE;
                }

            } //  end of suCtx fetch-loop.

            //...Log2.v("\nUpdater.SuUpdateSDBCtx(): On exit from fetch loop, ctxFetchCount = " + ctxFetchCount);

            // Handle case of an unsuccessful exit from the fetch-loop.
            if (!successfulFetchLoop)
            {
                /* some other ingres error - report it */
                Log2.e("\nUpdtCtx.SuUpdateSDBCtx(): ERROR: unsuccessful exit from fetch loop.");
                Console.Write("Error on fetch {0}\r\n", rc);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                rcUpdtCtx = Constant.FAILURE;
            }

            if (SuDynCtx.SuCloseCtx(curHandleMaster) != Constant.SUCCESS)
            {
                Log2.e("\nUpdtCtx.SuUpdateSDBCtx(): ERROR: call to SuDynCtx.SuCloseCtx() failed.");
                Console.Write("Ingres error on close \r\n");
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
            }

            return rcUpdtCtx;
        }


        /// <summary>
        /// This method performs the top-level fetching of records from the
        /// prescribed user su_XXX_ctxd table and the subsequent changes
        /// made to the main SDB table main.sd_ctxd i.a.w. the command
        /// ('cmd') field in each fetched record; changes to the main SDB table records comprise
        /// SQL INSERT (cmd = 'A'), UPDATE ('U'), DELETE ('D') operations and also No-Op ('N').
        /// </summary>
        /// <param name="hConn"></param>
        /// <param name="tfcr"></param>
        /// <param name="tfci"></param>
        /// <param name="rxeqp"></param>
        /// <param name="command"></param>
        /// <param name="subFileName"></param>
        /// <param name="errCount"></param>
        /// <param name="detCounts"></param>
        /// <returns></returns>
        private static int ProcessSuCtxD(SQLHDBC hConn,
                                            string tfcr,
                                            string tfci,
                                            string rxeqp,
                                            string command,
                                            string subFileName,
                                            ref int errCount,
                                            ref int[] detCounts
                                         )
        {
            SuCtxD suCtxD;
            SQLLEN[] nullArray;

            SdCtxD sdCtxD;
            SQLLEN[] sdNullInds;

            /* Local variables */
            int rcUpdtCtxD;      /* Function return code */
            int curHandleDetail;    /* handle returned by suSelectCtxD */
            string whereClause;  /* where clause for select */
            string orderBy;        /* orderby clause for cursor */
            //string modDate;       /* modify date - dd-mmm-yyyy format */
            //string modTime;     /* modify time - hh:mm */
            int rc = Constant.SUCCESS;      /* Function call return code */
            int rcDetail;           /* Detail update return code */

            /* Set return code for this function to Constant.SUCCESS as default */
            rcUpdtCtxD = Constant.SUCCESS;

            whereClause = String.Format(" tfcr='{0}' AND tfci='{1}' AND rxeqp='{2}' ", tfcr, tfci, rxeqp);
            orderBy = "fsep";

            /*  Select CTXD information  */
            curHandleDetail = SuDynCtxD.SuSelectCtxD(subFileName, whereClause, orderBy);

            if (curHandleDetail < 0)
            {
                Console.Write("Error opening cursor - ");
                Console.Write("return code from suSelectCtxD is {0}\r\n", curHandleDetail);
                return (Constant.FAILURE);
            }

            bool moreRecordsToFetch = true;
            bool successfulExit = false;
            int ctxdFetchCount = 0;

            while (moreRecordsToFetch)
            {
                // Attempt a fetch.
                rc = SuDynCtxD.SuFetchCtxD(curHandleDetail, out suCtxD, out nullArray);

                // Analyze the outcome of the fetch attempt.
                if (rc == Constant.SUCCESS)
                {
                    // The fetch succeeded; increment the count and carry on.
                    ctxdFetchCount++;
                    //...Log2.v("\nUpdater.SuUpdateSDBCtxD(): Fetch succeeded:" + suCtxD.ToStringWN(nullArray));
                }
                else if (rc == ODBC.SQL_NO_DATA)
                {
                    // There are no more records to fetch; break out of the while loop.
                    successfulExit = true;
                    break;
                }
                else
                {
                    // A serious ODBC/SQL error must have occurred; break out of the while loop.
                    Log2.e("\nUpdtCtx.SuUpdateSDBCtxD(): ERROR: call to SuDynCtx.SuFetchCtx() failed, rc = " + rc);
                    successfulExit = false;
                    break;
                }

                detCounts[(int)Action.Processed]++;

                switch (suCtxD.cmd[0])
                {

                    case 'A':
                        // Perform an SQL INSERT into the SDM table main.sd_ctxd

                        // print key information */
                        Console.Write("\r\n  CTX POINTS RECORD ADD: :{0}/{1}/{2}/{3:F2}: \r\n", suCtxD.tfcr, suCtxD.tfci, suCtxD.rxeqp, suCtxD.fsep);

                        // Copy values from the SuCtxD record into a new SdCtxD record.
                        SdCtxD.MakeSdFromSu(suCtxD, nullArray, out sdCtxD, out sdNullInds);

                        // Set the SdCtxD record's modify date and time.
                        sdCtxD.mdate = Info.Date;
                        sdCtxD.mtime = Info.Time;
                        sdNullInds[SdCtxD.MDATE] = Constant.DB_NOT_NULL;
                        sdNullInds[SdCtxD.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL INSERT.
                        rcDetail = DynSdbCtxD.SdInsertCtxD(hConn, sdCtxD, sdNullInds);

                        if (rcDetail == Constant.SUCCESS)
                        {
                            detCounts[(int)Action.Added]++;
                            // Print key information.
                            Console.Write("::\t- Successful INSERT of detail record {0}/{1}/{2}/{3:F3}:\r\n", suCtxD.tfcr, suCtxD.tfci, suCtxD.rxeqp, suCtxD.fsep);
                        }
                        else
                        {
                            rcUpdtCtxD = Constant.FAILURE;
                            errCount++;
                            Info.Text += String.Format("\r\nError inserting CTX data point {0}/{1}/{2}/{3:F3} record.", suCtxD.tfcr, suCtxD.tfci, suCtxD.rxeqp, suCtxD.fsep);
                        }

                        break;

                    case 'D':

                        // print key information */
                        Console.Write("\r\n  CTX POINT RECORD DELETE: :{0}/{1}/{2}/{3:F3}: \r\n", suCtxD.tfcr, suCtxD.tfci, suCtxD.rxeqp, suCtxD.fsep);

                        rcDetail = DynSdbCtxD.SdDeleteCtxD(hConn, suCtxD.tfcr, suCtxD.tfci, suCtxD.rxeqp, suCtxD.fsep);

                        if (rcDetail != Constant.SUCCESS)
                        {
                            // not a good delete of detail record - error.
                            Console.Write("::\t- Error {0} on DELETE of CTX point\n", rcDetail);
                            rcUpdtCtxD = Constant.FAILURE;
                            errCount++;
                        }
                        else
                        {
                            Console.Write("::\t- Successful DELETE of points record\r\n");
                            detCounts[(int)Action.Deleted]++;
                        }

                        break;

                    case 'B':
                    case 'U':
                        // Perform an SQL UPDATE on a record in the SDB table main.sd_ctxd

                        /* print key information */
                        Console.Write("\r\n  CTX POINT RECORD UPDATE: :{0}/{1}/{2}/{3:F3}: \r\n", suCtxD.tfcr, suCtxD.tfci, suCtxD.rxeqp, suCtxD.fsep);

                        // Copy values from the SuCtxD record into a new SdCtxD record.
                        SdCtxD.MakeSdFromSu(suCtxD, nullArray, out sdCtxD, out sdNullInds);

                        // Set the SdCtxD record's modify date and time.
                        sdCtxD.mdate = Info.Date;
                        sdCtxD.mtime = Info.Time;
                        sdNullInds[SdCtxD.MDATE] = Constant.DB_NOT_NULL;
                        sdNullInds[SdCtxD.MTIME] = Constant.DB_NOT_NULL;

                        rcDetail = DynSdbCtxD.SdUpdateCtxD(hConn, sdCtxD, sdNullInds);

                        if (rcDetail != Constant.SUCCESS)
                        {
                            rcUpdtCtxD = Constant.FAILURE;
                        }
                        else
                        {
                            detCounts[(int)Action.Updated]++;
                        }

                        break;

                    case 'N':
                        // No Operation.
                        detCounts[(int)Action.NoAction]++;
                        break;
                }

            } // end of fetch-loop.

            //...Log2.v("\nUpdater.ProcessSuCtxD(): On exit from fetch loop, ctxdFetchCount = " + ctxdFetchCount);

            // Handle case of an unsuccessful exit from the fetch-loop.
            if (!successfulExit)
            {
                /* some other ingres error - report it */
                Log2.e("\nUpdtCtx.SuUpdateSDBCtxD(): ERROR: unsuccessful exit from fetch loop.");
                Console.Write("Error on fetch {0}\r\n", rc);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                rcUpdtCtxD = Constant.FAILURE;
            }

            rc = SuDynCtxD.SuCloseCtxD(curHandleDetail);
            if (rc < 0)
            {
                Console.Write("Database error on close {0}\r\n", rc);
            }
            return (rcUpdtCtxD);
        }

        /// <summary>
        /// This method returns true if any of the Add, Update or Delete counters
        /// for a prescribed counter set are non-zero.
        /// </summary>
        /// <param name="detCounts"></param>
        /// <returns></returns>
        private static bool SomeDetailChanges(int[] detCounts)
        {
            int a = detCounts[(int)Action.Added];
            int b = detCounts[(int)Action.Deleted];
            int c = detCounts[(int)Action.Updated];

            return ((a != 0) || (b != 0) || (c != 0));
        }


        /// <summary>
        /// This method verifies that the number of records in the main SDB table main.sd_ctxd
        /// that have a prescribed SdCtx sub-key value is less than a system-prescribed maximum 
        /// (currently Constant.MAXINFLECPTS = 360).
        /// </summary>
        /// <param name="hConn"></param>
        /// <param name="tfcr"></param>
        /// <param name="tfci"></param>
        /// <param name="rxeqp"></param>
        /// <param name="errCount"></param>
        /// <param name="warnCount"></param>
        /// <param name="countDataPts"></param>
        /// <returns></returns>
        private static int ValidDetailRecordCount(SQLHDBC hConn, string tfcr, string tfci, string rxeqp, ref int errCount, ref int warnCount, out int countDataPts)
        {
            // Check that the number of detail records for this ctx_ record does not exceed
            // the prescribed system maximum MAXCTXPTS.
            int rcUpdtCtx = Constant.SUCCESS;

            // The table we need to check depends on whether 'spoofing' is enabled, or not.
            string tableName = DynSdbCtxD.TableName;

            string where = String.Format(" tfcr='{0}' AND tfci='{1}' AND rxeqp='{2}' ", tfcr, tfci, rxeqp);


            countDataPts = Ssutil.DbCountRowsConn(hConn, tableName, where);

            if (countDataPts >= 0)
            {
                if (countDataPts > Constant.MAXCTXPTS)
                {
                    errCount++;
                    rcUpdtCtx = Constant.FAILURE;
                    Console.Write("\r\nNumber of inflection points ({0}) exceeds maximum allowed: {1}\r\n",
                                    countDataPts, Constant.MAXCTXPTS);
                }

                if (countDataPts == 0)
                {
                    warnCount++;
                    Console.Write("::\t- ");
                    Console.Write("Warning no inflection points for the current record\r\n");
                }
            }
            else
            {
                // The call to Ssutil.DbCountRowsConn() failed - this is very bad.
                Console.Write(":{0}:\t- Ingres error on points check\r\n", countDataPts);
                rcUpdtCtx = Constant.FAILURE;
                errCount++;
            }

            return rcUpdtCtx;
        }


    }
}

```
