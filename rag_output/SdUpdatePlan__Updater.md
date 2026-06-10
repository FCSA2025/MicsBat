# Documented File: Updater.cs
**Repository Path:** `SdUpdatePlan\Updater.cs`
**Primary Layer:** `SdUpdatePlan`
**Namespace:** `SdUpdatePlan`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdatePlan
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
    /// prescribed user su_XXX_plan and su_XXX_plnd tables and the subsequent changes
    /// made to the main SDB tables main.sd_plan and main.sd_plnd i.a.w. the command
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
        private static int curHandleMaster;             /* suSelectPlan handle */

        /// <summary>
        /// This method performs the top-level fetching of records from the
        /// prescribed user su_XXX_plan and su_XXX_plnd tables and the subsequent changes
        /// made to the main SDB tables main.sd_plan and main.sd_plnd i.a.w. the command
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
        public static int ProcessSuPlanPlnd(SQLHDBC hConn,
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

            int rcUpdtPlan = Constant.SUCCESS;
            int nRet;
            int countDataPts;

            // The following objects will be used to fetch "Master" records from the user's 
            // table [schema].sd_[sdfName]_plan.   
            SuPlan suPlan;
            SQLLEN[] suPlanNullInds;

            // The following objects will be used to insert/update/delete "detail" records
            // into/from the MDB table main.sd_plnd. 
            SdPlan sdPlan;
            SQLLEN[] sdPlanNullInds;

            Console.Write("\r\n\t\tSDF UPDATE RESULTS\r\n");
            Console.Write("\r\nSubsidiary Plan Update\r\n");

            // Get the names of the _plan and _plnd tables that correspond to the 
            // prescribed SDF file.
            GenUtil.UtCvtName(Constant.SU_PLAN, Info.SdfName, out intFileNameMaster);
            GenUtil.UtCvtName(Constant.SU_PLND, Info.SdfName, out intFileNameDetail);

            //...Log2.v("\nUpdater.SuUpdateSDBPlan(): intFileNameMaster = " + intFileNameMaster);
            //...Log2.v("\nUpdater.SuUpdateSDBPlan(): intFileNameDetail = " + intFileNameDetail);

            // Select all records in the user's sd_[sdfName]_plan table for subsequent fetch.
            curHandleMaster = SuDynPlan.SuSelectPlan(intFileNameMaster, "", "");

            if (curHandleMaster < 0)
            {
                Log2.e("\nUpdtPlan.SuUpdateSDBPlan(): ERROR: call to SuDynPlan.SuSelectPlan() failed, rc = " + curHandleMaster);
                Console.Write("Error opening cursor - ");
                Console.Write("return code from suSelectPlan is {0}\r\n", curHandleMaster);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                return (Constant.FAILURE);
            }

            bool moreRecordsToFetch = true;
            bool successfulFetchLoop = false;
            int rc = 0;
            int planFetchCount = 0;

            // This is the fetch-loop that reads every record in the user's 
            // sd_[sdfName]_plan table.
            while (moreRecordsToFetch)
            {
                // Attempt to fetch an suPlan 'master' record from the sd_[sdfName]_plan table.
                rc = SuDynPlan.SuFetchPlan(curHandleMaster, out suPlan, out suPlanNullInds);

                // Check the outcome of the fetch attempt.
                if (rc == Constant.SUCCESS)
                {
                    // The fetch succeeded. 
                    // Update the fetch count and tumble out the bottom of this if-else construct.
                    //...Log2.v("\nUpdater.SuUpdateSDBPlan(): Fetch succeeded:" + suPlan.ToStringWN(suPlanNullInds));
                    planFetchCount++;
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
                    Log2.e("\nUpdtPlan.SuUpdateSDBPlan(): ERROR: call to SuDynPlan.SuFetchPlan() failed, rc = " + rc);
                    successfulFetchLoop = false;
                    break;
                }

                /* Set the return codes to success prior to updates */
                rcDetail = Constant.SUCCESS;
                rcMaster = Constant.SUCCESS;

                // We successfully fetched a suPlan record that needs processing so first 
                // increment its counter.
                recCounts[(int)Action.Processed]++;

                // Write 'action' on record to the Console.
                switch (suPlan.cmd[0])
                {
                    case 'A':
                        Console.Write("\r\nPLAN RECORD ADD: :{0}:{1}: \r\n", suPlan.sband, suPlan.splan);
                        break;
                    case 'D':
                        Console.Write("\r\nPLAN RECORD DELETE: :{0}:{1}: \r\n", suPlan.sband, suPlan.splan);
                        break;
                    case 'B':
                    case 'U':
                        Console.Write("\r\nPLAN RECORD UPDATE: :{0}:{1}: \r\n", suPlan.sband, suPlan.splan);
                        break;
                    case 'N':
                        Console.Write("\r\nPLAN RECORD NO ACTION: :{0}:{1}: \r\n", suPlan.sband, suPlan.splan);
                        break;
                    default:
                        break;
                }

                // The following method call performs the appropriate Add/Delete/Update
                // operations on 'detail' records fetched from the user's sd_[sdfName]_plnd 
                // table that have key values that match the 'master' record.
                rcDetail = ProcessSuPlnD(hConn,
                                            suPlan.sband,
                                            suPlan.splan,
                                            suPlan.cmd,
                                            intFileNameDetail,
                                            ref errCount,
                                            ref detCounts);

                // Now process the suPlan record according to its 'cmd' column value.
                switch (suPlan.cmd[0])
                {
                    case 'A':

                        // Check that the number of detail records in main.sd_plnd that have the
                        // prescribed master's key is within bounds.
                        nRet = ValidDetailRecordCount(hConn, suPlan.sband, suPlan.splan, ref errCount, ref warnCount, out countDataPts);
                        if (nRet != Constant.SUCCESS)
                        {
                            rcUpdtPlan = Constant.FAILURE;
                        }

                        // Perform an SQL INSERT into the SDB table main.sd_plan
                        // Copy values from the SuPlan record into a new SdPlan record.
                        SdPlan.MakeSdFromSu(suPlan, suPlanNullInds, out sdPlan, out sdPlanNullInds);

                        // Set the SdPlan record's modify date and time.
                        sdPlan.mdate = Info.Date;
                        sdPlan.mtime = Info.Time;
                        sdPlanNullInds[SdPlan.MDATE] = Constant.DB_NOT_NULL;
                        sdPlanNullInds[SdPlan.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL INSERT of sdPlan into main.sd_plan
                        rcMaster = DynSdbPlan.SdInsertPlan(hConn, sdPlan, sdPlanNullInds);

                        if (rcMaster == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Added]++;
                        }
                        else
                        {
                            rcUpdtPlan = Constant.FAILURE;
                            errCount++;
                        }

                        break;

                    case 'D':

                        // There is no need to check that the number of detail records in main.sd_plnd that 
                        // have the prescribed key values is within bounds because our next action is
                        // to delete them all.

                        // Delete any detail records belonging to the master record and then
                        // delete the master record itself.

                        // Detail records: perform the SQL DELETE of a SET of sdPlnd from main.sd_plnd.
                        //                 Some records may have been deleted by the previous call to 
                        //                 ProcessSuPlnd() and we now delete any that are remaining.
                        string where = String.Format(" sband = '{0}' AND splan = '{1}' ", suPlan.sband, suPlan.splan);
                        int rowCount = Ssutil.DbDeleteRowsConn(hConn, DynSdbPlnd.TableName, where);

                        //...Log2.v("\nSdUpdateAnte.ProcessSuPlanPlnd(): where = " + where);
                        //...Log2.v("\nSdUpdateAnte.ProcessSuPlanPlnd(): rowCount = " + rowCount);

                        if (rowCount < 0)
                        {
                            // An error occurred.
                            Console.Write("::\t- Error {0} on DELETE of PLAN points\r\n{1}\r\n", rowCount, GenUtil.GetUserMess());
                            rcUpdtPlan = Constant.FAILURE;
                            errCount++;
                        }
                        else
                        {
                            // Update the detail counts.
                            detCounts[(int)Action.Processed] += rowCount;
                            detCounts[(int)Action.Deleted] += rowCount;
                        }

                        // Master record: perform the SQL DELETE of sdPlan from main.sd_plan
                        rcMaster = DynSdbPlan.SdDeletePlan(hConn, suPlan.sband, suPlan.splan);

                        if (rcMaster == Constant.SUCCESS)
                        {
                            Console.Write("::\t- Successful DELETE of master and points records\r\n");
                            recCounts[(int)Action.Deleted]++;
                        }
                        else
                        {
                            rcUpdtPlan = Constant.FAILURE;
                            errCount++;
                        }

                        break;

                    case 'B':
                    case 'U':

                        // Check that the number of detail records in main.sd_plnd that have the
                        // prescribed master's key is within bounds.
                        nRet = ValidDetailRecordCount(hConn, suPlan.sband, suPlan.splan, ref errCount, ref warnCount, out countDataPts);
                        if (nRet != Constant.SUCCESS)
                        {
                            rcUpdtPlan = Constant.FAILURE;
                        }

                        // Perform the SQL UPDATE of sdPlan in main.sd_plan
                        // Copy values from the SuPlan record into a new SdPlan record.
                        SdPlan.MakeSdFromSu(suPlan, suPlanNullInds, out sdPlan, out sdPlanNullInds);

                        // Set the SdPlan record's modify date and time.
                        sdPlan.mdate = Info.Date;
                        sdPlan.mtime = Info.Time;
                        sdPlanNullInds[SdPlan.MDATE] = Constant.DB_NOT_NULL;
                        sdPlanNullInds[SdPlan.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL UPDATE of sdPlan in main.sd_plan
                        rcMaster = DynSdbPlan.SdUpdatePlan(hConn, sdPlan, sdPlanNullInds);

                        if (rcMaster == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Updated]++;
                        }
                        else
                        {
                            rcUpdtPlan = Constant.FAILURE;
                            errCount++;
                        }

                        break;

                    case 'N':

                        if (rcDetail == Constant.SUCCESS)
                        {
                            if (SomeDetailChanges(detCounts))
                            {
                                // Check that the number of detail records in main.sd_plnd that have the
                                // prescribed key values is within bounds.
                                nRet = ValidDetailRecordCount(hConn, suPlan.sband, suPlan.splan, ref errCount, ref warnCount, out countDataPts);
                                if (nRet != Constant.SUCCESS)
                                {
                                    rcUpdtPlan = Constant.FAILURE;
                                }

                                // Perform an SQL UPDATE on the record in the SDB table main.sd_plan
                                // Copy values from the SuPlan record into a new SdPlan record.
                                SdPlan.MakeSdFromSu(suPlan, suPlanNullInds, out sdPlan, out sdPlanNullInds);

                                // Set the SdPlan record's modification date and time fields.
                                sdPlan.mdate = Info.Date;
                                sdPlan.mtime = Info.Time;

                                sdPlanNullInds[SdPlan.MDATE] = Constant.DB_NOT_NULL;
                                sdPlanNullInds[SdPlan.MTIME] = Constant.DB_NOT_NULL;

                                // Perform the SQL UPDATE of sdPlan into main.sd_plan
                                rcMaster = DynSdbPlan.SdUpdatePlan(hConn, sdPlan, sdPlanNullInds);

                                if (rcMaster != Constant.SUCCESS)
                                {
                                    rcUpdtPlan = Constant.FAILURE;
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
                    rcUpdtPlan = Constant.FAILURE;
                }

            } //  end of suPlan fetch-loop.

            //...Log2.v("\nUpdater.SuUpdateSDBPlan(): On exit from fetch loop, planFetchCount = " + planFetchCount);

            // Handle case of an unsuccessful exit from the fetch-loop.
            if (!successfulFetchLoop)
            {
                /* some other ingres error - report it */
                Log2.e("\nUpdtPlan.SuUpdateSDBPlan(): ERROR: unsuccessful exit from fetch loop.");
                Console.Write("Error on fetch {0}\r\n", rc);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                rcUpdtPlan = Constant.FAILURE;
            }

            if (SuDynPlan.SuClosePlan(curHandleMaster) != Constant.SUCCESS)
            {
                Log2.e("\nUpdtPlan.SuUpdateSDBPlan(): ERROR: call to SuDynPlan.SuClosePlan() failed.");
                Console.Write("Ingres error on close \r\n");
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
            }

            return rcUpdtPlan;
        }


        /// <summary>
        /// This method performs the top-level fetching of records from the
        /// prescribed user su_XXX_plnd table and the subsequent changes
        /// made to the main SDB table main.sd_plnd i.a.w. the command
        /// ('cmd') field in each fetched record; changes to the main SDB table records comprise
        /// SQL INSERT (cmd = 'A'), UPDATE ('U'), DELETE ('D') operations and also No-Op ('N').
        /// </summary>
        /// <param name="hConn"></param>
        /// <param name="sband"></param>
        /// <param name="splan"></param>
        /// <param name="command"></param>
        /// <param name="subFileName"></param>
        /// <param name="errCount"></param>
        /// <param name="detCounts"></param>
        /// <returns></returns>
        private static int ProcessSuPlnD(SQLHDBC hConn,
                                            string sband,
                                            string splan,
                                            string command,
                                            string subFileName,
                                            ref int errCount,
                                            ref int[] detCounts
                                         )
        {
            SuPlnd suPlnd;
            SQLLEN[] nullArray;

            SdPlnd sdPlnd;
            SQLLEN[] sdNullInds;

            /* Local variables */
            int rcUpdtPlnd;      /* Function return code */
            int curHandleDetail;    /* handle returned by suSelectPlnd */
            string whereClause;  /* where clause for select */
            string orderBy;        /* orderby clause for cursor */
            //string modDate;       /* modify date - dd-mmm-yyyy format */
            //string modTime;     /* modify time - hh:mm */
            int rc = Constant.SUCCESS;      /* Function call return code */
            int rcDetail;           /* Detail update return code */

            /* Set return code for this function to Constant.SUCCESS as default */
            rcUpdtPlnd = Constant.SUCCESS;

            whereClause = String.Format(" sband='{0}' AND splan='{1}' ", sband, splan);
            orderBy = "sband, splan, spno ";

            /*  Select PLND information  */
            curHandleDetail = SuDynPlnd.SuSelectPlnd(subFileName, whereClause, orderBy);

            if (curHandleDetail < 0)
            {
                Console.Write("Error opening cursor - ");
                Console.Write("return code from suSelectPlnd is {0}\r\n", curHandleDetail);
                return (Constant.FAILURE);
            }

            bool moreRecordsToFetch = true;
            bool successfulExit = false;
            int plndFetchCount = 0;

            while (moreRecordsToFetch)
            {
                // Attempt a fetch.
                rc = SuDynPlnd.SuFetchPlnd(curHandleDetail, out suPlnd, out nullArray);

                // Analyze the outcome of the fetch attempt.
                if (rc == Constant.SUCCESS)
                {
                    // The fetch succeeded; increment the count and carry on.
                    plndFetchCount++;
                    //...Log2.v("\nUpdater.SuUpdateSDBPlnd(): Fetch succeeded:" + suPlnd.ToStringWN(nullArray));
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
                    Log2.e("\nUpdtPlan.SuUpdateSDBPlnd(): ERROR: call to SuDynPlan.SuFetchPlan() failed, rc = " + rc);
                    successfulExit = false;
                    break;
                }

                detCounts[(int)Action.Processed]++;

                switch (suPlnd.cmd[0])
                {

                    case 'A':
                        // Perform an SQL INSERT into the SDM table main.sd_plnd

                        // print key information */
                        Console.Write("\r\n  PLAN ARRAY RECORD ADD: :{0}/{1} ({2}):\r\n", suPlnd.sband, suPlnd.splan, suPlnd.spno);

                        // Copy values from the SuPlnd record into a new SdPlnd record.
                        SdPlnd.MakeSdFromSu(suPlnd, nullArray, out sdPlnd, out sdNullInds);

                        // Set the SdPlnd record's modify date and time.
                        sdPlnd.mdate = Info.Date;
                        sdPlnd.mtime = Info.Time;
                        sdNullInds[SdPlnd.MDATE] = Constant.DB_NOT_NULL;
                        sdNullInds[SdPlnd.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL INSERT.
                        rcDetail = DynSdbPlnd.SdInsertPlnd(hConn, sdPlnd, sdNullInds);

                        if (rcDetail == Constant.SUCCESS)
                        {
                            detCounts[(int)Action.Added]++;
                        }
                        else
                        {
                            rcUpdtPlnd = Constant.FAILURE;
                            errCount++;
                            Info.Text += String.Format("\r\nError inserting PLAN data point {0}:{1}:{2} record.", suPlnd.sband, suPlnd.splan, suPlnd.spno);
                        }

                        break;

                    case 'D':

                        // print key information */
                        Console.Write("\r\n  PLAN DETAIL RECORD DELETE: :{0}/{1} ({2}): ", suPlnd.sband, suPlnd.splan, suPlnd.spno);

                        rcDetail = DynSdbPlnd.SdDeletePlnd(hConn, suPlnd.sband, suPlnd.splan, suPlnd.spno);

                        if (rcDetail != Constant.SUCCESS)
                        {
                            // not a good delete of detail record - error.
                            Console.Write("::\t- Error {0} on DELETE of PLAN point\n", rcDetail);
                            rcUpdtPlnd = Constant.FAILURE;
                            errCount++;
                        }
                        else
                        {
                            Console.Write("\r\n::\t- Successful DELETE of PLAN Detail record\r\n");
                            detCounts[(int)Action.Deleted]++;
                        }

                        break;

                    case 'B':
                    case 'U':
                        // Perform an SQL UPDATE on a record in the SDB table main.sd_plnd

                        /* print key information */
                        Console.Write("\r\n  PLAN POINT RECORD UPDATE: :{0}/{1}/{2}: \r\n", suPlnd.sband, suPlnd.splan, suPlnd.spno);

                        // Copy values from the SuPlnd record into a new SdPlnd record.
                        SdPlnd.MakeSdFromSu(suPlnd, nullArray, out sdPlnd, out sdNullInds);

                        // Set the SdPlnd record's modify date and time.
                        sdPlnd.mdate = Info.Date;
                        sdPlnd.mtime = Info.Time;
                        sdNullInds[SdPlnd.MDATE] = Constant.DB_NOT_NULL;
                        sdNullInds[SdPlnd.MTIME] = Constant.DB_NOT_NULL;

                        rcDetail = DynSdbPlnd.SdUpdatePlnd(hConn, sdPlnd, sdNullInds);

                        if (rcDetail != Constant.SUCCESS)
                        {
                            rcUpdtPlnd = Constant.FAILURE;
                        }
                        else
                        {
                            detCounts[(int)Action.Updated]++;
                        }

                        break;

                    case 'N':
                        // No Operation.
                        if (command == "D")
                        {
                            detCounts[(int)Action.NoAction]++;

                            Console.Write("\r\n  PLAN POINT RECORD NO ACTION: ");
                            Console.Write(":{0}/{1} ({2}):\r\n", suPlnd.sband, suPlnd.splan, suPlnd.spno);
                        }
                        break;
                }

            } // end of fetch-loop.

            //...Log2.v("\nUpdater.ProcessSuPlnd(): On exit from fetch loop, plndFetchCount = " + plndFetchCount);

            // Handle case of an unsuccessful exit from the fetch-loop.
            if (!successfulExit)
            {
                /* some other ingres error - report it */
                Log2.e("\nUpdtPlan.SuUpdateSDBPlnd(): ERROR: unsuccessful exit from fetch loop.");
                Console.Write("Error on fetch {0}\r\n", rc);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                rcUpdtPlnd = Constant.FAILURE;
            }

            rc = SuDynPlnd.SuClosePlnd(curHandleDetail);
            if (rc < 0)
            {
                Console.Write("Database error on close {0}\r\n", rc);
            }
            return (rcUpdtPlnd);
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
        /// This method verifies that the number of records in the main SDB table main.sd_plnd
        /// that have a prescribed SdPlan sub-key value is less than a system-prescribed maximum 
        /// (currently Constant.MAXPLANDETAILINDEX = 255).
        /// </summary>
        /// <param name="hConn"></param>
        /// <param name="sband"></param>
        /// <param name="splan"></param>
        /// <param name="errCount"></param>
        /// <param name="warnCount"></param>
        /// <param name="countDataPts"></param>
        /// <returns></returns>
        private static int ValidDetailRecordCount(SQLHDBC hConn, string sband, string splan, ref int errCount, ref int warnCount, out int countDataPts)
        {
            // Check that the number of detail records for this plan record does not exceed
            // the prescribed system maximum MAXPLANDETAILINDEX.

            // The table main.sd_plnd has the unusual property that the 'spno' column is
            // only allocated a single byte in which to store its value. Consequently, the
            // largest possible value of 'spno' is 255.

            // The current value of MAXPLANDETAILINDEX is 255; consequently the numerical
            // check below is nugatory. The code is left in just in case the value of the
            // upper bound MAXPLANDETAILINDEX is ever reduced from its current value in which
            // case the numerical test becomes significant.

            int rcUpdtPlan = Constant.SUCCESS;

            // The table we need to check depends on whether 'spoofing' is enabled, or not.
            string tableName = DynSdbPlnd.TableName;

            string where = String.Format(" sband='{0}' AND splan='{1}' ", sband, splan);


            countDataPts = Ssutil.DbCountRowsConn(hConn, tableName, where);

            if (countDataPts >= 0)
            {
                if (countDataPts > Constant.MAXPLANDETAILINDEX)
                {
                    errCount++;
                    rcUpdtPlan = Constant.FAILURE;
                    Console.Write("\r\nNumber of inflection points ({0}) exceeds maximum allowed: {1}\r\n",
                                    countDataPts, Constant.MAXPLANDETAILINDEX);
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
                rcUpdtPlan = Constant.FAILURE;
                errCount++;
            }

            return rcUpdtPlan;
        }


    }
}


```
