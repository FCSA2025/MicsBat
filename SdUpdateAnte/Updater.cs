using System;

namespace SdUpdateAnte
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides the methods that perform the fetching of records from the
    /// prescribed user su_XXX_ante and su_XXX_antd tables and the subsequent changes
    /// made to the main SDB tables main.sd_ante and main.sd_antd i.a.w. the command
    /// ('cmd') fields in each fetched record; changes to the main SDB table records comprise
    /// SQL INSERT (cmd = 'A'), UPDATE ('U'), DELETE ('D') operations and also No-Op ('N').
    /// </summary>
    public class Updater
    {
        private static string intFileNameMaster;        /* Internal Ingres name */
        private static string intFileNameDetail;        /* Internal Ingres name */
        private static string modDate;                  /* modify date - dd-mmm-yyyy */
        private static string modTime;                  /* modify time - hh:mm */
        private static int rcMaster;                    /* master update return code */
        private static int rcDetail;                    /* detail update return code */
        private static int curHandleMaster;             /* suSelectAnte handle */

        /// <summary>
        /// This method performs the top-level fetching of records from the
        /// prescribed user su_XXX_ante and su_XXX_antd tables and the subsequent changes
        /// made to the main SDB tables main.sd_ante and main.sd_antd i.a.w. the command
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
        public static int ProcessSuAnteAntd(SQLHDBC hConn,
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

            int rcUpdtAnte = Constant.SUCCESS;
            int nRet;
            int countDataPts;

            // The following objects will be used to fetch "Master" records from the user's 
            // table [schema].sd_[sdfName]_ante.   
            SuAnte suAnte;
            SQLLEN[] suAnteNullInds;

            // The following objects will be used to insert/update/delete "detail" records
            // into/from the MDB table main.sd_antd. 
            SdAnte sdAnte;
            SQLLEN[] sdAnteNullInds;

            Console.Write("\r\n\t\tSDF UPDATE RESULTS\r\n");

            // Get the names of the _ante and _antd tables that correspond to the 
            // prescribed SDF file.
            GenUtil.UtCvtName(Constant.SU_ANTE, Info.SdfName, out intFileNameMaster);
            GenUtil.UtCvtName(Constant.SU_ANTD, Info.SdfName, out intFileNameDetail);

            //...Log2.v("\nUpdtAnte.SuUpdateSDBAnte(): intFileNameMaster = " + intFileNameMaster);
            //...Log2.v("\nUpdtAnte.SuUpdateSDBAnte(): intFileNameDetail = " + intFileNameDetail);

            // One time call to get the current date and time. These values are
            // used everwhere that needs to write the same date and/or time.
            GenUtil.UtGetDateTime(out modDate, out modTime);

            Console.Write("\r\nSubsidiary Antenna Update\r\n");

            // Select all records in the user's sd_[sdfName]_ante table for subsequent fetch.
            curHandleMaster = SuDynAnte.SuSelectAnte(intFileNameMaster, "", "");

            if (curHandleMaster < 0)
            {
                Log2.e("\nUpdtAnte.SuUpdateSDBAnte(): ERROR: call to SuDynAnte.SuSelectAnte() failed, rc = " + curHandleMaster);
                Console.Write("Error opening cursor - ");
                Console.Write("return code from suSelectAnte is {0}\r\n", curHandleMaster);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                return (Constant.FAILURE);
            }

            bool moreRecordsToFetch = true;
            bool successfulFetchLoop = false;
            int rc = 0;
            //int fetchCount = 0;

            // This is the fetch-loop that reads every record in the user's 
            // sd_[sdfName]_ante table.
            while (moreRecordsToFetch)
            {
                // Attempt to fetch an suAnte 'master' record from the sd_[sdfName]_ante table.
                rc = SuDynAnte.SuFetchAnte(curHandleMaster, out suAnte, out suAnteNullInds);

                // Check the outcome of the fetch attempt.
                if (rc == Constant.SUCCESS)
                {
                    // The fetch succeeded. 
                    // Tumble out the bottom of this if-else construct.
                    //...Log2.v("\nUpdtAnte.SuUpdateSDBAnte(): Fetch succeeded:" + suAnte.ToStringWN(suAnteNullInds));
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
                    Log2.e("\nUpdtAnte.SuUpdateSDBAnte(): ERROR: call to SuDynAnte.SuFetchAnte() failed, rc = " + rc);
                    successfulFetchLoop = false;
                    break;
                }

                /* Set the return codes to success prior to updates */
                rcDetail = Constant.SUCCESS;
                rcMaster = Constant.SUCCESS;

                // We successfully fetched a suAnte record that needs processing so first 
                // increment its counter.
                recCounts[(int)Action.Processed]++;

                switch (suAnte.cmd[0])
                {
                    case 'A':
                        Console.Write("\r\nANTENNA RECORD ADD: :{0}: \r\n", suAnte.acode);
                        break;
                    case 'D':
                        Console.Write("\r\nANTENNA RECORD DELETE: :{0}: \r\n", suAnte.acode);
                        break;
                    case 'B':
                    case 'U':
                        Console.Write("\r\nANTENNA RECORD UPDATE: :{0}: \r\n", suAnte.acode);
                        break;
                    case 'N':
                        Console.Write("\r\nANTENNA RECORD NO ACTION: :{0}: \r\n", suAnte.acode);
                        break;
                    default:
                        break;
                }

                // The following method call performs the appropriate Add/Delete/Update
                // operations on 'detail' records fetched from the user's sd_[sdfName]_antd 
                // table that have an 'acode' value that matches the 'master' record.
                rcDetail = ProcessSuAntd(hConn,
                                            suAnte.acode,
                                            suAnte.cmd,
                                            intFileNameDetail,
                                            ref errCount,
                                            ref detCounts);

                // Now process the suAnte record according to its 'cmd' column value.
                switch (suAnte.cmd[0])
                {
                    case 'A':

                        // Check that the number of detail records in main.sd_antd that have the
                        // prescribed 'acode' is within bounds.
                        nRet = ValidDetailRecordCount(hConn, suAnte.acode, ref errCount, ref warnCount, out countDataPts);
                        if (nRet != Constant.SUCCESS)
                        {
                            rcUpdtAnte = Constant.FAILURE;
                        }

                        // Perform an SQL INSERT into the SDB table main.sd_ante
                        // Copy values from the SuAnte record into a new SdAnte record.
                        SdAnte.MakeSdFromSu(suAnte, suAnteNullInds, out sdAnte, out sdAnteNullInds);

                        // Set the SdAnte record's modify date and time.
                        sdAnte.mdate = Info.Date;
                        sdAnte.mtime = Info.Time;
                        sdAnteNullInds[SdAnte.MDATE] = Constant.DB_NOT_NULL;
                        sdAnteNullInds[SdAnte.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL INSERT of sdAnte into main.sd_ante
                        rcDetail = DynSdbAnte.SdInsertAnte(hConn, sdAnte, sdAnteNullInds);

                        if (rcDetail == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Added]++;
                        }
                        else
                        {
                            rcUpdtAnte = Constant.FAILURE;
                        }

                        break;

                    case 'D':

                        // There is no need to check that the number of detail records in main.sd_antd that 
                        // have the prescribed 'acode' is within bounds because our next action is
                        // to delete them all.

                        // Delete any detail records belonging to the master record and then
                        // delete the master record itself.

                        // Delete the detail records.
                        string where = String.Format(" acode='{0}' ", suAnte.acode);
                        int rowCount = Ssutil.DbDeleteRowsConn(hConn, DynSdbAntd.TableName, where);

                        //...Log2.v("\nSdUpdateAnte.ProcessSuAnteAntd(): where = " + where);
                        //...Log2.v("\nSdUpdateAnte.ProcessSuAnteAntd(): rowCount = " + rowCount);

                        if (rowCount < 0)
                        {
                            // An error occurred.
                            Console.Write("::\t- Error {0} on DELETE of Antenna points\r\n{1}\r\n", rowCount, GenUtil.GetUserMess());
                            rcUpdtAnte = Constant.FAILURE;
                            errCount++;
                        }
                        else
                        {
                            // Update the detail counts.
                            detCounts[(int)Action.Processed] += rowCount;
                            detCounts[(int)Action.Deleted] += rowCount;
                        }

                        // Perform the SQL DELETE of sdAnte from main.sd_ante
                        rcMaster = DynSdbAnte.SdDeleteAnte(hConn, suAnte.acode);

                        if (rcMaster == Constant.SUCCESS)
                        {
                            Console.Write("::\t- Successful DELETE of master and points records\r\n");
                            recCounts[(int)Action.Deleted]++;
                        }
                        else
                        {
                            rcUpdtAnte = Constant.FAILURE;
                        }

                        break;

                    case 'B':
                    case 'U':

                        // Check that the number of detail records in main.sd_antd that have the
                        // prescribed 'acode' is within bounds.
                        nRet = ValidDetailRecordCount(hConn, suAnte.acode, ref errCount, ref warnCount, out countDataPts);
                        if (nRet != Constant.SUCCESS)
                        {
                            rcUpdtAnte = Constant.FAILURE;
                        }

                        // Perform the SQL UPDATE of sdAnte in main.sd_ante
                        // Copy values from the SuAnte record into a new SdAnte record.
                        SdAnte.MakeSdFromSu(suAnte, suAnteNullInds, out sdAnte, out sdAnteNullInds);

                        // Set the SdAnte record's modify date and time.
                        sdAnte.mdate = Info.Date;
                        sdAnte.mtime = Info.Time;
                        sdAnteNullInds[SdAnte.MDATE] = Constant.DB_NOT_NULL;
                        sdAnteNullInds[SdAnte.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL UPDATE of sdAnte in main.sd_ante
                        rcDetail = DynSdbAnte.SdUpdateAnte(hConn, sdAnte, sdAnteNullInds);

                        if (rcMaster == Constant.SUCCESS)
                        {
                            recCounts[(int)Action.Updated]++;
                        }
                        else
                        {
                            rcUpdtAnte = Constant.FAILURE;
                        }

                        break;

                    /* process any details, even if no action on the master */
                    case 'N':

                        if (rcDetail == Constant.SUCCESS)
                        {
                            if (SomeDetailChanges(detCounts))
                            {
                                // Check that the number of detail records in main.sd_antd that have the
                                // prescribed 'acode' is within bounds.
                                nRet = ValidDetailRecordCount(hConn, suAnte.acode, ref errCount, ref warnCount, out countDataPts);
                                if (nRet != Constant.SUCCESS)
                                {
                                    rcUpdtAnte = Constant.FAILURE;
                                }

                                // Perform an SQL UPDATE on the record in the SDB table main.sd_ante
                                // Copy values from the SuAnte record into a new SdAnte record.
                                SdAnte.MakeSdFromSu(suAnte, suAnteNullInds, out sdAnte, out sdAnteNullInds);

                                // Set the SdAnte record's anip and modification date and time fields.
                                sdAnte.anip = (short)countDataPts;
                                sdAnte.mdate = Info.Date;
                                sdAnte.mtime = Info.Time;

                                sdAnteNullInds[SdAnte.ANIP] = Constant.DB_NOT_NULL;
                                sdAnteNullInds[SdAnte.MDATE] = Constant.DB_NOT_NULL;
                                sdAnteNullInds[SdAnte.MTIME] = Constant.DB_NOT_NULL;

                                // Perform the SQL UPDATE of sdAnte into main.sd_ante
                                rcDetail = DynSdbAnte.SdUpdateAnte(hConn, sdAnte, sdAnteNullInds);

                                if (rcUpdtAnte == Constant.SUCCESS)
                                {
                                    recCounts[(int)Action.NoAction]++;
                                }
                                else
                                {
                                    rcUpdtAnte = Constant.FAILURE;
                                    errCount++;
                                }

                            }
                        }

                        break;
                } // end of switch block.

                /* check the return code on the master and detail updates */
                if ((rcMaster == Constant.FAILURE) || (rcDetail == Constant.FAILURE))
                {
                    /* if either update fails, set return code to Constant.FAILURE */
                    rcUpdtAnte = Constant.FAILURE;
                }

            } //  end of suAnte fetch-loop.

            // Handle case of an unsuccessful exit from the fetch-loop.
            if (!successfulFetchLoop)
            {
                /* some other ingres error - report it */
                Log2.e("\nUpdtAnte.SuUpdateSDBAnte(): ERROR: unsuccessful exit from fetch loop.");
                Console.Write("Error on fetch {0}\r\n", rc);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                rcUpdtAnte = Constant.FAILURE;
            }

            if (SuDynAnte.SuCloseAnte(curHandleMaster) != Constant.SUCCESS)
            {
                Log2.e("\nUpdtAnte.SuUpdateSDBAnte(): ERROR: call to SuDynAnte.SuCloseAnte() failed.");
                Console.Write("Ingres error on close \r\n");
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
            }

            return rcUpdtAnte;
        }

        /// <summary>
        /// This method performs the top-level fetching of records from the
        /// prescribed user su_XXX_antd table and the subsequent changes
        /// made to the main SDB table main.sd_antd i.a.w. the command
        /// ('cmd') field in each fetched record; changes to the main SDB table records comprise
        /// SQL INSERT (cmd = 'A'), UPDATE ('U'), DELETE ('D') operations and also No-Op ('N').
        /// </summary>
        /// <param name="hConn"></param>
        /// <param name="aCode"></param>
        /// <param name="command"></param>
        /// <param name="subFileName"></param>
        /// <param name="errCount"></param>
        /// <param name="detCounts"></param>
        /// <returns></returns>
        private static int ProcessSuAntd(SQLHDBC hConn,                 /* o - Connection to database */
                                            string aCode,                       /* i - master antenna code */
                                            string command,                 /* i - master record command */
                                            string subFileName,         /* i - displayable table name */
                                            ref int errCount,             /* o - errors count */
                                            ref int[] detCounts
                                         )
        {
            SuAntd antdTransRec;
            SQLLEN[] nullArray;

            SdAntd sdAntd;
            SQLLEN[] sdNullInds;

            /* Local variables */
            int rcUpdtAntd;      /* Function return code */
            int curHandleDetail;    /* handle returned by suSelectAntd */
            string whereClause;  /* where clause for select */
            string orderBy;        /* orderby clause for cursor */
            string modDate;       /* modify date - dd-mmm-yyyy format */
            string modTime;     /* modify time - hh:mm */
            int rc = Constant.SUCCESS;      /* Function call return code */
            int rcDetail;           /* Detail update return code */

            /* Set return code for this function to Constant.SUCCESS as default */
            rcUpdtAntd = Constant.SUCCESS;

            whereClause = String.Format("acode = '{0}'", aCode);
            orderBy = "acode, antang";

            /* One time call to get the current date and time in an Ingres
             * acceptable format */
            GenUtil.UtGetDateTime(out modDate, out modTime);

            /*  Read Antenna information  */
            curHandleDetail = SuDynAntd.SuSelectAntd(subFileName, whereClause, orderBy);

            if (curHandleDetail < 0)
            {
                Console.Write("Error opening cursor - ");
                Console.Write("return code from suSelectAntd is {0}\r\n", curHandleDetail);
                return (Constant.FAILURE);
            }

            bool moreRecordsToFetch = true;
            bool successfulExit = false;
            int fetchCount = 0;

            while (moreRecordsToFetch)
            {
                // Attempt a fetch.
                rc = SuDynAntd.SuFetchAntd(curHandleDetail, out antdTransRec, out nullArray);

                // Analyze the outcome of the fetch attempt.
                if (rc == Constant.SUCCESS)
                {
                    // The fetch succeeded; increment the count and carry on.
                    fetchCount++;
                    //...Log2.v("\nUpdtAnte.SuUpdateSDBAntd(): Fetch succeeded:" + antdTransRec.ToStringWN(nullArray));
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
                    Log2.e("\nUpdtAnte.SuUpdateSDBAntd(): ERROR: call to SuDynAnte.SuFetchAnte() failed, rc = " + rc);
                    successfulExit = false;
                    break;
                }

                if (antdTransRec.cmd[0] != 'N')
                {
                    detCounts[(int)Action.Processed]++;
                }

                switch (antdTransRec.cmd[0])
                {

                    case 'A':
                        // Perform an SQL INSERT into the SDM table main.sd_antd

                        // Copy values from the SuAntd record into a new SdAntd record.
                        SdAntd.MakeSdFromSu(antdTransRec, nullArray, out sdAntd, out sdNullInds);

                        // Set the SdAntd record's modify date and time.
                        sdAntd.mdate = Info.Date;
                        sdAntd.mtime = Info.Time;
                        sdNullInds[SdAntd.MDATE] = Constant.DB_NOT_NULL;
                        sdNullInds[SdAntd.MTIME] = Constant.DB_NOT_NULL;

                        // Perform the SQL INSERT.
                        rcDetail = DynSdbAntd.SdInsertAntd(hConn, sdAntd, sdNullInds);

                        if (rcDetail == Constant.SUCCESS)
                        {
                            detCounts[(int)Action.Added]++;
                            // Print key information.
                            Console.Write("\r\n  ANTENNA POINT RECORD ADD: :{0}/{1:F2}:\r\n", antdTransRec.acode, antdTransRec.antang);

                        }
                        else
                        {
                            rcUpdtAntd = Constant.FAILURE;
                        }

                        break;

                    case 'D':

                        // print key information */
                        Console.Write("\r\n  ANTENNA POINT RECORD DELETE: :{0}/{1:F2}: \r\n",
                                        antdTransRec.acode, antdTransRec.antang);

                        rcDetail = DynSdbAntd.SdDeleteAntd(hConn, antdTransRec.acode, antdTransRec.antang);

                        if (rcDetail != Constant.SUCCESS)
                        {
                            // not a good delete of detail record - error.
                            Console.Write("::\t- Error {0} on DELETE of ANTENNA point\n", rcDetail);
                            rcUpdtAntd = Constant.FAILURE;
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
                        // Perform an SQL UPDATE on a record in the SDB table main.sd_antd

                        /* print key information */
                        Console.Write("\r\n  ANTENNA POINT RECORD UPDATE: :{0}/{1:F2}: \r\n",
                                                antdTransRec.acode, antdTransRec.antang);

                        // Copy values from the SuAntd record into a new SdAntd record.
                        SdAntd.MakeSdFromSu(antdTransRec, nullArray, out sdAntd, out sdNullInds);

                        // Set the SdAntd record's modify date and time.
                        sdAntd.mdate = Info.Date;
                        sdAntd.mtime = Info.Time;
                        sdNullInds[SdAntd.MDATE] = Constant.DB_NOT_NULL;
                        sdNullInds[SdAntd.MTIME] = Constant.DB_NOT_NULL;

                        rcDetail = DynSdbAntd.SdUpdateAntd(hConn, sdAntd, sdNullInds);

                        if (rcDetail != Constant.SUCCESS)
                        {
                            rcUpdtAntd = Constant.FAILURE;
                        }
                        else
                        {
                            detCounts[(int)Action.Updated]++;
                        }

                        break;

                    case 'N':

                        if ((command[0]) == 'D')
                        {
                            detCounts[(int)Action.NoAction]++;

                            Console.Write("\r\n  ANTENNA POINT RECORD NO ACTION: ");
                            Console.Write(":{0}/{1:F2}:\r\n", antdTransRec.acode, antdTransRec.antang);
                        }

                        break;
                }

            } // end of fetch-loop.

            // Handle case of an unsuccessful exit from the fetch-loop.
            if (!successfulExit)
            {
                /* some other ingres error - report it */
                Log2.e("\nUpdtAnte.SuUpdateSDBAntd(): ERROR: unsuccessful exit from fetch loop.");
                Console.Write("Error on fetch {0}\r\n", rc);
                Console.Write("\t{0}\r\n", GenUtil.GetUserMess());
                rcUpdtAntd = Constant.FAILURE;
            }

            rc = SuDynAntd.SuCloseAntd(curHandleDetail);
            if (rc < 0)
            {
                Console.Write("Database error on close {0}\r\n", rc);
            }
            return (rcUpdtAntd);
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
        /// This method verifies that the number of records in the main SDB table main.sd_antd
        /// that have a precribed 'acode' key value is less than a system-prescribed maximum 
        /// (currently Constant.MAXINFLECPTS = 360).
        /// </summary>
        /// <param name="hConn"></param>
        /// <param name="acode"></param>
        /// <param name="errCount"></param>
        /// <param name="warnCount"></param>
        /// <param name="countDataPts"></param>
        /// <returns></returns>
        private static int ValidDetailRecordCount(SQLHDBC hConn, string acode, ref int errCount, ref int warnCount, out int countDataPts)
        {
            // Check that the number of detail records for this ante record does not exceed
            // the prescribed system maximum MAXINFLECPTS.
            int rcUpdtAnte = Constant.SUCCESS;

            // The table we need to check depends on whether 'spoofing' is enabled, or not.
            string tableName = DynSdbAntd.TableName;

            string where = String.Format("acode='{0}'", acode);

            countDataPts = Ssutil.DbCountRowsConn(hConn, tableName, where);

            if (countDataPts >= 0)
            {
                if (countDataPts > Constant.MAXINFLECPTS)
                {
                    errCount++;
                    rcUpdtAnte = Constant.FAILURE;
                    Console.Write("\r\nNumber of inflection points ({0}) exceeds maximum allowed: {1}\r\n",
                                    countDataPts, Constant.MAXINFLECPTS);
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
                rcUpdtAnte = Constant.FAILURE;
                errCount++;
            }

            return rcUpdtAnte;
        }


    }
}
