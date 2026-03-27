using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeUpdate
{
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;            //Invented to mimic (char *) for [In]  only.
    using SQLCHARPTRINOUT = IntPtr;       //Invented to mimic (char *) for [In, Out].
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLINTEGERPTR = IntPtr;
    using SQLLEN = Int64;
    using SQLLENPTR = IntPtr;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;
    using SQLSETPOSIROW = UInt64;
    using SQLSMALLINT = Int16;
    using SQLSMALLINTPTR = IntPtr;
    using SQLULEN = UInt64;
    using SQLUSMALLINT = UInt16;
    using SQLUINTEGER = UInt32;
    using _DataStructures;
    using _Utillib;
    using _NewLib;
    using System.Runtime.InteropServices;

    /// <summary>
    /// This class provides methods that add, delete and/or update records in the
    /// ES-specific tables in the Main Data Base (MDB).
    /// </summary>
    public class Mdb
    {

        /// <summary>
        /// This method performs the update/add/delete for channel records in the 
        /// ES-specific MDB table <b>main.me_chan</b>.
        /// </summary>
        /// <param name="hConn"> - handle to an existing ODBC connection.</param>
        /// <param name="pdfName"> - kernel name of the ES PDF table set.</param>
        /// <param name="userNameIn"> - MICS user ID.</param>
        /// <param name="addCount"> - number of records added, site, antenna, channel.</param>
        /// <param name="delCount"> - number of records deleted, site, antenna, channel.</param>
        /// <param name="updCount"> - number of records updated, site, antenna, channel.</param>
        /// <param name="totCount"> - number of records processed, site, antenna, channel.</param>
        /// <returns></returns>
        public static int UpdateMeChanTable(SQLHDBC hConn,
                                            string pdfName,         /* pdf name - short format */
                                            string userNameIn,      /* MICS ID of current user */
                                            ref int addCount,       /* number of records added */
                                            ref int delCount,       /* number of records deleted */
                                            ref int updCount,       /* number of records updated */
                                            ref int totCount)       /* number of records processed */
        {
            //...Log2.v("\n\nMdb.UpdateMeChanTable(): Entry");

            FeChan feChan;              /* storage for pdf chan record */
            SQLLEN[] feChanNullInds;    /* storage for nulls */

            MeChan meChan;              /* storage for MDB chan record */
            SQLLEN[] meChanNullInds;    /* storage for nulls */

            MeChan meChanNew;           /* storage for MDB chan record */
            SQLLEN[] meChanNewNullInds; /* storage for nulls */

            string sysDate; /* storage for system date */
            string sysTime; /* storage for system time */

            /* Local variables */
            int chanHandle;         /* storage for chan handle */
            int status = Constant.SUCCESS;        /* transaction status */
            int rc = Constant.SUCCESS;      /* Fctn call ret code */
            int nRet;
            string whereClause;     /* selection clause for SQL */

            string fileName;    /* pdf name - long format */

            SQLRETURN sqlRet = 0;
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            /* Copy MICS ID to Ingres aware variable */
            //userName = Info.MicsUserName;

            /* Convert pdf name to long format */
            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out fileName);

            /* Get a handle on the pdf channel records */
            chanHandle = DynFeChan.FeSelectChan(fileName, "", "");

            if (chanHandle < 0)
            {
                Console.Write("\r\nERROR!  Could not select Channel records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                status = Constant.FAILURE;
            }

            /* While there are ES Channel records to process */
            while ((rc = DynFeChan.FeFetchChan(chanHandle, out feChan, out feChanNullInds)) == Constant.SUCCESS)
            {
                /* Count all records processed */
                totCount++;

                /* Check for "no op" record type */
                if (feChan.cmd[0] == Constant.CMD_NO_OP)
                {
                    // Just go on to fetch the next record.
                    continue;
                }

                // Instantiate a new MeChan object and associated nullInds and populate
                // these with values from chan and nArrayFW for all 'shared' members. 
                Make.MeChanFromFeChan(feChan, feChanNullInds, out meChanNew, out meChanNewNullInds);

                // Set the userid.
                meChanNew.userid = userNameIn;
                meChanNewNullInds[MeChan.USERID] = Constant.DB_NOT_NULL;

                // Get the current the date and time.
                GenUtil.UtGetDateTime(out sysDate, out sysTime);

                // Set mdate to current date.
                meChanNew.mdate = sysDate;
                meChanNewNullInds[MeChan.MDATE] = Constant.DB_NOT_NULL;

                // Set mtime to current time.
                meChanNew.mtime = sysTime;
                meChanNewNullInds[MeChan.MTIME] = Constant.DB_NOT_NULL;

                //...Log2.v("\n\n" + meChanNew.ToStringWN(meChanNewNullInds));

                // Check if a record with same key already exists in the MDB.
                whereClause = String.Format("location = '{0}'  AND  call1 = '{1}' AND  chid = '{2}'",
                                                feChan.location, feChan.call1, feChan.chid);

                bool alreadyExistsInMdb = DynMeChan.MdbRecordExists(feChan.location, feChan.call1, feChan.chid);

                //...Log2.v("\n\nMdb.UpdateMeChanTable(): alreadyExistsInMdb = " + alreadyExistsInMdb);

                if (alreadyExistsInMdb)
                {
                    rc = ValRetrieveES.FeValRetrieveMDBChannel(out meChan, whereClause, out meChanNullInds);

                    // Check if the retrieve channel worked.
                    if (rc != Constant.SUCCESS)
                    {
                        Log2.e("\nMdb.UpdateMeChanTable(): ERROR: call to FeValRetrieveMDBChannel() failed.");
                        return rc;
                    }

                    // If the record already exists in the table then the only valid
                    // commands are 'U'pdate, 'B'lank, and 'D'elete.

                    /* Handle case of 'U'pdate or 'B'lank */
                    if ((feChan.cmd[0] == Constant.CMD_UPDATE) || (feChan.cmd[0] == Constant.CMD_BLANK))
                    {

                        /* Check modify date and time  */
                        if ((!feChan.mdate.Equals(meChan.mdate)) || (!feChan.mtime.Equals(meChan.mtime)))
                        {
                            /* Modify dates/times don't match */
                            Console.Write("\r\nWARNING!  This Channel record has been modified since you retrieved it.\r\n");
                            Console.Write("Location = {0}, Call1 = {1}, ", meChan.location, meChan.call1);
                            Console.Write("Chid = {0}\r\n", meChan.chid);
                        }

                        // Update every column of the existing record in the 
                        // MDB main.me_chan table.
                        nRet = DynMeChan.MeUpdateChan(hConn, meChanNew, meChanNewNullInds);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successful update of record in main.me_chan.
                            updCount++;
                        }
                        else
                        {
                            Log2.e("\nMdb.UpdateMeChanTable(): ERROR: call to MeUpdateChan() failed, nRet = " + nRet);
                            string str = String.Format("DynMeChan.MeUpdateChan04 -- Error writing Channel: {0} {1} {2}\r\n",
                                                                                  feChan.location, feChan.call1, feChan.chid);
                            Ssutil.DbGetDiagStmt(hUpdate, str);
                            Console.Write("\r\nERROR! {0}", GenUtil.GetUserMess());
                            GenUtil.ReSetError();
                            status = Constant.FAILURE;
                        }

                    }
                    /* Handle case of 'D'elete */
                    else if (feChan.cmd[0] == Constant.CMD_DELETE)
                    {
                        /* If cmd is 'D', do a deletion */

                        nRet = DynMeChan.MeDeleteChan(hConn, feChan.location, feChan.call1, feChan.chid);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successful update of record in main.me_chan.
                            delCount++;
                        }
                        else
                        {
                            Console.Write("\r\nERROR!  Could not Delete Channel record\r\n");
                            Console.Write("Location = {0}, call1 = {1}, chid = {2}\r\n",
                                            feChan.location, feChan.call1, feChan.chid);
                            ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                            status = Constant.FAILURE;
                        }
                    }
                    /* If not delete or update then error */
                    else
                    {
                        Console.Write("\r\nERROR!  Cannot Add this Channel record.  It already exists in the MDB.\r\n");
                        Console.Write("Location = {0}, Call1 = {1}, Chid = {2}\r\n",
                                            meChan.location, meChan.call1, meChan.chid);
                        status = Constant.FAILURE; /* set transaction status to error */
                    }

                    // Process the next FeChan record.
                    continue;

                }   /* End 'if record exists in MDB'  */

                else
                {

                    // ** Record does not exist in MDB ** 
                    // Can only perform an ADD function in this section of code, since   
                    // record does not exist.	    

                    // Handle the case of 'A'dd.
                    if (feChan.cmd[0] == Constant.CMD_ADD)
                    {
                        nRet = DynMeChan.MeInsertChan(hConn, meChanNew, meChanNewNullInds);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successfully inserted a recodr into main.me_chan
                            addCount++;
                        }
                        else
                        {
                            Log2.e("\nMdb.UpdateMeChanTable(): ERROR: call to MeInsertChan() failed, nRet = " + nRet);
                            string str = String.Format("DynMeChan.MeUpdateChannelv02 -- Error inserting Channel: {0} {1} {2}",
                                                        meChanNew.location, meChanNew.call1, meChanNew.chid);
                            Ssutil.DbGetDiagStmt(hUpdate, str);
                            Console.Write("\r\nERROR! {0}", GenUtil.GetUserMess());
                            GenUtil.ReSetError();
                            status = Constant.FAILURE;
                        }

                    }
                    /* If not an add then error */
                    else
                    {
                        Console.Write("\r\nERROR!  Cannot Update/Delete this Channel record.  ");
                        Console.Write("It does not exist in the MDB.\r\n");
                        Console.Write("Location = {0}, Call1 = {1}, Chid = {2}\r\n",
                                            feChan.location, feChan.call1, feChan.chid);
                        status = Constant.FAILURE; /* set transaction status to error */
                    }

                    // Process the next FeChan record.
                    continue;   /* Continue on to next record */

                }   /* End  'else record does not exist in MDB' */

            } // fetch loop.

            /* Make sure all was OK from the fetch above */
            if (rc != Constant.NOMORERECS)
            {
                Log2.e("\nMdb.UpdateMeChanTable(): ERROR: abnormal exit from fetch-loop, rc = " + rc);
                status = Constant.FAILURE;
                Console.Write("\r\nERROR!  Problem fetching Channel records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            /* Close the open cursor */
            DynFeChan.FeCloseChan(chanHandle);

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);

            //...Log2.v("\n\nMdb.UpdateMeChanTable(): Exit: status = " + status);

            /* Return status of the transaction so far */
            return (status);
        }

        /// <summary>
        /// This method performs the update/add/delete for antenna records in the 
        /// ES-specific MDB table <b>main.me_ante</b>.
        /// </summary>
        /// <param name="hConn"> - handle to an existing ODBC connection.</param>
        /// <param name="pdfName"> - kernel name of the ES PDF table set.</param>
        /// <param name="userNameIn"> - MICS user ID.</param>
        /// <param name="addCount"> - number of records added, site, antenna, channel.</param>
        /// <param name="delCount"> - number of records deleted, site, antenna, channel.</param>
        /// <param name="updCount"> - number of records updated, site, antenna, channel.</param>
        /// <param name="totCount"> - number of records processed, site, antenna, channel.</param>
        /// <returns></returns>
        public static int UpdateMeAnteTable(SQLHANDLE hConn,    //	Connection to use.
                                            string pdfName,     /* Name of pdf in short format */
                                            string userNameIn,  /* MICS User ID of person at keyboard */
                                            ref int addCount,       /* number of records added */
                                            ref int delCount,       /* number of records deleted */
                                            ref int updCount,       /* number of records updated */
                                            ref int totCount        /* number of records procesed */
        )
        {
            //...Log2.v("\n\nMdb.UpdateMeAnteTable(): Entry");

            FeAnte feAnte;              /* storage for pdf ante record */
            SQLLEN[] feAnteNullInds;    /* storage for nulls */

            MeAnte meAnte;              /* storage for MDB ante record */
            SQLLEN[] meAnteNullInds;    /* storage for nulls */

            MeAnte meAnteNew;           /* storage for MDB ante record */
            SQLLEN[] meAnteNewNullInds; /* storage for nulls */

            string sysDate; /* storage for system date */
            string sysTime; /* storage for system time */

            /* Local variables */
            int anteHandle;         /* storage for ante handle */
            int status = Constant.SUCCESS;        /* transaction status */
            int rc = Constant.SUCCESS;      /* Fctn call ret code */
            int nRet;
            string whereClause;     /* selection clause for SQL */

            string fileName;    /* pdf name - long format */

            SQLRETURN sqlRet = 0;
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            /* Copy MICS ID to Ingres aware variable */
            //userName = Info.MicsUserName;

            /* Convert pdf name to long format */
            GenUtil.UtCvtName(Constant.FE_ANTE, pdfName, out fileName);

            /* Get a handle on the pdf ante records */
            anteHandle = DynFeAnte.FeSelectAnte(fileName, "", "");

            if (anteHandle < 0)
            {
                Console.Write("\r\nERROR!  Could not select Antenna records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                status = Constant.FAILURE;
            }

            /* While there are ES Ante records to process */
            while ((rc = DynFeAnte.FeFetchAnte(anteHandle, out feAnte, out feAnteNullInds)) == Constant.SUCCESS)
            {
                /* Count all records processed */
                totCount++;

                /* Check for "no op" record type */
                if (feAnte.cmd[0] == Constant.CMD_NO_OP)
                {
                    // Just go on to fetch the next record.
                    continue;
                }

                // Instantiate a new MeAnte object and associated nullInds and populate
                // these with values from ante and nArrayFW for all 'shared' members. 
                Make.MeAnteFromFeAnte(feAnte, feAnteNullInds, out meAnteNew, out meAnteNewNullInds);

                // Set the userid.
                meAnteNew.userid = userNameIn;
                meAnteNewNullInds[MeAnte.USERID] = Constant.DB_NOT_NULL;

                // Get the current the date and time.
                GenUtil.UtGetDateTime(out sysDate, out sysTime);

                // Set mdate to current date.
                meAnteNew.mdate = sysDate;
                meAnteNewNullInds[MeAnte.MDATE] = Constant.DB_NOT_NULL;

                // Set mtime to current time.
                meAnteNew.mtime = sysTime;
                meAnteNewNullInds[MeAnte.MTIME] = Constant.DB_NOT_NULL;

                //...Log2.v("\n\n" + meAnteNew.ToStringWN(meAnteNewNullInds));

                // Check if a record with same key already exists in the MDB.
                whereClause = String.Format("location = '{0}'  AND  call1 = '{1}'",
                                                feAnte.location, feAnte.call1);

                bool alreadyExistsInMdb = DynMeAnte.MdbRecordExists(feAnte.location, feAnte.call1);

                //...Log2.v("\n\nMdb.UpdateMeAnteTable(): alreadyExistsInMdb = " + alreadyExistsInMdb);

                if (alreadyExistsInMdb)
                {
                    rc = ValRetrieveES.FeValRetrieveMDBAntenna(out meAnte, whereClause, out meAnteNullInds);

                    // Check if the retrieve ante worked.
                    if (rc != Constant.SUCCESS)
                    {
                        Log2.e("\nMdb.UpdateMeAnteTable(): ERROR: call to FeValRetrieveMDBAnte() failed.");
                        return rc;
                    }

                    // If the record already exists in the table then the only valid
                    // commands are 'U'pdate, 'B'lank, and 'D'elete.

                    /* Handle case of 'U'pdate or 'B'lank */
                    if ((feAnte.cmd[0] == Constant.CMD_UPDATE) || (feAnte.cmd[0] == Constant.CMD_BLANK))
                    {

                        /* Check modify date and time  */
                        if ((!feAnte.mdate.Equals(meAnte.mdate)) || (!feAnte.mtime.Equals(meAnte.mtime)))
                        {
                            /* Modify dates/times don't match */
                            Console.Write("\r\nWARNING!  This Ante record has been modified since you retrieved it.\r\n");
                            Console.Write("Location = {0}, Call1 = {1}, ", meAnte.location, meAnte.call1);
                        }

                        // Update every column of the existing record in the 
                        // MDB main.me_ante table.
                        nRet = DynMeAnte.MeUpdateAnte(hConn, meAnteNew, meAnteNewNullInds);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successful update of record in main.me_ante.
                            updCount++;
                        }
                        else
                        {
                            Log2.e("\nMdb.UpdateMeAnteTable(): ERROR: call to MeUpdateAnte() failed, nRet = " + nRet);
                            string str = String.Format("DynMeAnte.MeUpdateAnte04 -- Error writing Ante: {0} {1}\r\n",
                                                                                  feAnte.location, feAnte.call1);
                            Ssutil.DbGetDiagStmt(hUpdate, str);
                            Console.Write("\r\nERROR! {0}", GenUtil.GetUserMess());
                            GenUtil.ReSetError();
                            status = Constant.FAILURE;
                        }

                    }
                    /* Handle case of 'D'elete */
                    else if (feAnte.cmd[0] == Constant.CMD_DELETE)
                    {
                        /* If cmd is 'D', do a deletion */

                        nRet = DynMeAnte.MeDeleteAnte(hConn, feAnte.location, feAnte.call1);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successful update of record in main.me_ante.
                            delCount++;
                        }
                        else
                        {
                            Console.Write("\r\nERROR!  Could not Delete Ante record\r\n");
                            Console.Write("Location = {0}, call1 = {1}\r\n",
                                         feAnte.location, feAnte.call1);
                            ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                            status = Constant.FAILURE;
                        }
                    }
                    /* If not delete or update then error */
                    else
                    {
                        Console.Write("\r\nERROR!  Cannot Add this Ante record.  It already exists in the MDB.\r\n");
                        Console.Write("Location = {0}, Call1 = {1}\r\n",
                                            meAnte.location, meAnte.call1);
                        status = Constant.FAILURE; /* set transaction status to error */
                    }

                    // Process the next FeAnte record.
                    continue;

                }   /* End 'if record exists in MDB'  */

                else
                {

                    // ** Record does not exist in MDB ** 
                    // Can only perform an ADD function in this section of code, since   
                    // record does not exist.	    

                    // Handle the case of 'A'dd.
                    if (feAnte.cmd[0] == Constant.CMD_ADD)
                    {
                        nRet = DynMeAnte.MeInsertAnte(hConn, meAnteNew, meAnteNewNullInds);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successfully inserted a recodr into main.me_ante
                            addCount++;
                        }
                        else
                        {
                            Log2.e("\nMdb.UpdateMeAnteTable(): ERROR: call to MeInsertAnte() failed, nRet = " + nRet);
                            string str = String.Format("DynMeAnte.MeUpdateAntev02 -- Error inserting Channel: {0} {1}",
                                                        meAnteNew.location, meAnteNew.call1);
                            Ssutil.DbGetDiagStmt(hUpdate, str);
                            Console.Write("\r\nERROR! {0}", GenUtil.GetUserMess());
                            GenUtil.ReSetError();
                            status = Constant.FAILURE;
                        }

                    }
                    /* If not an add then error */
                    else
                    {
                        Console.Write("\r\nERROR!  Cannot Update/Delete this Antenna record.  ");
                        Console.Write("It does not exist in the MDB.\r\n");
                        Console.Write("Location = {0}, Call1 = {1}\r\n",
                                            feAnte.location, feAnte.call1);
                        status = Constant.FAILURE; /* set transaction status to error */
                    }

                    // Process the next FeAnte record.
                    continue;   /* Continue on to next record */

                }   /* End  'else record does not exist in MDB' */

            } // fetch loop.

            /* Make sure all was OK from the fetch above */
            if (rc != Constant.NOMORERECS)
            {
                Log2.e("\nMdb.UpdateMeAnteTable(): ERROR: abnormal exit from fetch-loop, rc = " + rc);
                status = Constant.FAILURE;
                Console.Write("\r\nERROR!  Problem fetching Ante records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            /* Close the open cursor */
            DynFeAnte.FeCloseAnte(anteHandle);

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);

            //...Log2.v("\n\nMdb.UpdateMeAnteTable(): Exit: status = " + status);

            /* Return status of the transaction so far */
            return (status);
        }

        /// <summary>
        /// This method performs the update/add/delete for azimuth records in the 
        /// ES-specific MDB table <b>main.me_azim</b>.
        /// </summary>
        /// <param name="hConn"> - handle to an existing ODBC connection.</param>
        /// <param name="pdfName"> - kernel name of the ES PDF table set.</param>
        /// <param name="userNameIn"> - MICS user ID.</param>
        /// <param name="addCount"> - number of records added, site, antenna, channel.</param>
        /// <param name="delCount"> - number of records deleted, site, antenna, channel.</param>
        /// <param name="updCount"> - number of records updated, site, antenna, channel.</param>
        /// <param name="totCount"> - number of records processed, site, antenna, channel.</param>
        /// <returns></returns>
        public static int UpdateMeAzimTable(SQLHDBC hConn,
                                            string pdfName,
                                            string userNameIn,
                                            ref int addCount,       /* number of records added */
                                            ref int delCount,       /* number of records deleted */
                                            ref int updCount,       /* number of records updated */
                                            ref int totCount)       /* number of records procesed */
        {
            //...Log2.v("\n\nMdb.UpdateMeAzimTable(): Entry");

            FeAzim feAzim;              /* storage for pdf azim record */
            SQLLEN[] feAzimNullInds;    /* storage for nulls */

            MeAzim meAzim;              /* storage for MDB azim record */
            SQLLEN[] meAzimNullInds;    /* storage for nulls */

            MeAzim meAzimNew;           /* storage for MDB azim record */
            SQLLEN[] meAzimNewNullInds; /* storage for nulls */

            string sysDate; /* storage for system date */
            string sysTime; /* storage for system time */

            /* Local variables */
            int azimHandle;         /* storage for azim handle */
            int status = Constant.SUCCESS;        /* transaction status */
            int rc = Constant.SUCCESS;      /* Fctn call ret code */
            int nRet;
            string whereClause;     /* selection clause for SQL */

            string fileName;    /* pdf name - long format */

            SQLRETURN sqlRet = 0;
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            /* Copy MICS ID to Ingres aware variable */
            //userName = Info.MicsUserName;

            /* Convert pdf name to long format */
            GenUtil.UtCvtName(Constant.FE_AZIM, pdfName, out fileName);

            /* Get a handle on the pdf azim records */
            azimHandle = DynFeAzim.FeSelectAzim(fileName, "", "");

            if (azimHandle < 0)
            {
                Console.Write("\r\nERROR!  Could not select Azim records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                status = Constant.FAILURE;
            }

            /* While there are ES Azim records to process */
            while ((rc = DynFeAzim.FeFetchAzim(azimHandle, out feAzim, out feAzimNullInds)) == Constant.SUCCESS)
            {
                /* Count all records processed */
                totCount++;

                /* Check for "no op" record type */
                if (feAzim.cmd[0] == Constant.CMD_NO_OP)
                {
                    // Just go on to fetch the next record.
                    continue;
                }

                // Instantiate a new MeAzim object and associated nullInds and populate
                // these with values from azim and nArrayFW for all 'shared' members. 
                Make.MeAzimFromFeAzim(feAzim, feAzimNullInds, out meAzimNew, out meAzimNewNullInds);

                // Set the userid.
                meAzimNew.userid = userNameIn;
                meAzimNewNullInds[MeAzim.USERID] = Constant.DB_NOT_NULL;

                // Get the current the date and time.
                GenUtil.UtGetDateTime(out sysDate, out sysTime);

                // Set mdate to current date.
                meAzimNew.mdate = sysDate;
                meAzimNewNullInds[MeAzim.MDATE] = Constant.DB_NOT_NULL;

                // Set mtime to current time.
                meAzimNew.mtime = sysTime;
                meAzimNewNullInds[MeAzim.MTIME] = Constant.DB_NOT_NULL;

                //...Log2.v("\n\n" + meAzimNew.ToStringWN(meAzimNewNullInds));

                // Check if a record with same key already exists in the MDB.
                whereClause = String.Format("location = '{0}'  AND  call1 = '{1}' AND  azim = '{2}'",
                                                feAzim.location, feAzim.call1, feAzim.azim);

                bool alreadyExistsInMdb = DynMeAzim.MdbRecordExists(feAzim.location, feAzim.call1, feAzim.azim);

                //...Log2.v("\n\nMdb.UpdateMeAzimTable(): alreadyExistsInMdb = " + alreadyExistsInMdb);

                if (alreadyExistsInMdb)
                {
                    rc = ValRetrieveES.FeValRetrieveMDBAzimuth(out meAzim, whereClause, out meAzimNullInds);

                    // Check if the retrieve azim worked.
                    if (rc != Constant.SUCCESS)
                    {
                        Log2.e("\nMdb.UpdateMeAzimTable(): ERROR: call to FeValRetrieveMDBAzim() failed.");
                        return rc;
                    }

                    // If the record already exists in the table then the only valid
                    // commands are 'U'pdate, 'B'lank, and 'D'elete.

                    /* Handle case of 'U'pdate or 'B'lank */
                    if ((feAzim.cmd[0] == Constant.CMD_UPDATE) || (feAzim.cmd[0] == Constant.CMD_BLANK))
                    {

                        /* Check modify date and time  */
                        if ((!feAzim.mdate.Equals(meAzim.mdate)) || (!feAzim.mtime.Equals(meAzim.mtime)))
                        {
                            /* Modify dates/times don't match */
                            Console.Write("\r\nWARNING!  This Azim record has been modified since you retrieved it.\r\n");
                            Console.Write("Location = {0}, Call1 = {1}, ", meAzim.location, meAzim.call1);
                            Console.Write("Azim = {0}\r\n", meAzim.azim);
                        }

                        // Update every column of the existing record in the 
                        // MDB main.me_azim table.
                        nRet = DynMeAzim.MeUpdateAzim(hConn, meAzimNew, meAzimNewNullInds);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successful update of record in main.me_azim.
                            updCount++;
                        }
                        else
                        {
                            Log2.e("\nMdb.UpdateMeAzimTable(): ERROR: call to MeUpdateAzim() failed, nRet = " + nRet);
                            string str = String.Format("DynMeAzim.MeUpdateAzim04 -- Error writing Azim: {0} {1} {2}\r\n",
                                                                                  feAzim.location, feAzim.call1, feAzim.azim);
                            Ssutil.DbGetDiagStmt(hUpdate, str);
                            Console.Write("\r\nERROR! {0}", GenUtil.GetUserMess());
                            GenUtil.ReSetError();
                            status = Constant.FAILURE;
                        }

                    }
                    /* Handle case of 'D'elete */
                    else if (feAzim.cmd[0] == Constant.CMD_DELETE)
                    {
                        /* If cmd is 'D', do a deletion */

                        nRet = DynMeAzim.MeDeleteAzim(hConn, feAzim.location, feAzim.call1, feAzim.azim);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successful update of record in main.me_azim.
                            delCount++;
                        }
                        else
                        {
                            Console.Write("\r\nERROR!  Could not Delete Azim record\r\n");
                            Console.Write("Location = {0}, call1 = {1}, azim = {2}\r\n",
                                            feAzim.location, feAzim.call1, feAzim.azim);
                            ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                            status = Constant.FAILURE;
                        }
                    }
                    /* If not delete or update then error */
                    else
                    {
                        Console.Write("\r\nERROR!  Cannot Add this Azim record.  It already exists in the MDB.\r\n");
                        Console.Write("Location = {0}, Call1 = {1}, Azim = {2}\r\n",
                                            meAzim.location, meAzim.call1, meAzim.azim);
                        status = Constant.FAILURE; /* set transaction status to error */
                    }

                    // Process the next FeAzim record.
                    continue;

                }   /* End 'if record exists in MDB'  */

                else
                {

                    // ** Record does not exist in MDB ** 
                    // Can only perform an ADD function in this section of code, since   
                    // record does not exist.	    

                    // Handle the case of 'A'dd.
                    if (feAzim.cmd[0] == Constant.CMD_ADD)
                    {
                        nRet = DynMeAzim.MeInsertAzim(hConn, meAzimNew, meAzimNewNullInds);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successfully inserted a recodr into main.me_azim
                            addCount++;
                        }
                        else
                        {
                            Log2.e("\nMdb.UpdateMeAzimTable(): ERROR: call to MeInsertAzim() failed, nRet = " + nRet);
                            string str = String.Format("DynMeAzim.MeUpdateAzimv02 -- Error inserting Azim: {0} {1} {2}",
                                                        meAzimNew.location, meAzimNew.call1, meAzimNew.azim);
                            Ssutil.DbGetDiagStmt(hUpdate, str);
                            Console.Write("\r\nERROR! {0}", GenUtil.GetUserMess());
                            GenUtil.ReSetError();
                            status = Constant.FAILURE;
                        }

                    }
                    /* If not an add then error */
                    else
                    {
                        Console.Write("\r\nERROR!  Cannot Update/Delete this Azim record.  ");
                        Console.Write("It does not exist in the MDB.\r\n");
                        Console.Write("Location = {0}, Call1 = {1}, Azim = {2}\r\n",
                                            feAzim.location, feAzim.call1, feAzim.azim);
                        status = Constant.FAILURE; /* set transaction status to error */
                    }

                    // Process the next FeAzim record.
                    continue;   /* Continue on to next record */

                }   /* End  'else record does not exist in MDB' */

            } // fetch loop.

            /* Make sure all was OK from the fetch above */
            if (rc != Constant.NOMORERECS)
            {
                Log2.e("\nMdb.UpdateMeAzimTable(): ERROR: abnormal exit from fetch-loop, rc = " + rc);
                status = Constant.FAILURE;
                Console.Write("\r\nERROR!  Problem fetching Azim records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            /* Close the open cursor */
            DynFeAzim.FeCloseAzim(azimHandle);

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);

            //...Log2.v("\n\nMdb.UpdateMeAzimTable(): Exit: status = " + status);

            /* Return status of the transaction so far */
            return (status);


        }

        /// <summary>
        /// This method performs the update/add/delete for site records in the 
        /// ES-specific MDB table <b>main.me_site</b>.
        /// </summary>
        /// <param name="hConn"> - handle to an existing ODBC connection.</param>
        /// <param name="pdfName"> - kernel name of the ES PDF table set.</param>
        /// <param name="userNameIn"> - MICS user ID.</param>
        /// <param name="addCount"> - number of records added, site, antenna, channel.</param>
        /// <param name="delCount"> - number of records deleted, site, antenna, channel.</param>
        /// <param name="updCount"> - number of records updated, site, antenna, channel.</param>
        /// <param name="totCount"> - number of records processed, site, antenna, channel.</param>
        /// <returns></returns>
        public static int UpdateMeSiteTable(SQLHDBC hConn,
                                         string pdfName,    /* pdf display name */
                                         string userNameIn,     /* name of user submitting the pdf */
                                         ref int addCount,      /* number of records added */
                                         ref int delCount,      /* number of records deleted */
                                         ref int updCount,      /* number of records updated */
                                         ref int totCount)      /* number of records processed */
        {
            //...Log2.v("\n\nMdb.UpdateMeSiteTable(): Entry");

            FeSite feSite;              /* storage for pdf site record */
            SQLLEN[] feSiteNullInds;    /* storage for nulls */

            MeSite meSite;              /* storage for MDB site record */
            SQLLEN[] meSiteNullInds;    /* storage for nulls */

            MeSite meSiteNew;           /* storage for MDB site record */
            SQLLEN[] meSiteNewNullInds; /* storage for nulls */

            string sysDate; /* storage for system date */
            string sysTime; /* storage for system time */

            /* Local variables */
            int siteHandle;         /* storage for site handle */
            int status = Constant.SUCCESS;        /* transaction status */
            int rc = Constant.SUCCESS;      /* Fctn call ret code */
            int nRet;
            string whereClause;     /* selection clause for SQL */

            string fileName;    /* pdf name - long format */

            SQLRETURN sqlRet = 0;
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            /* Copy MICS ID to Ingres aware variable */
            //userName = Info.MicsUserName;

            /* Convert pdf name to long format */
            GenUtil.UtCvtName(Constant.FE_SITE, pdfName, out fileName);

            /* Get a handle on the pdf site records */
            siteHandle = DynFeSite.FeSelectSite(fileName, "", "");

            if (siteHandle < 0)
            {
                Console.Write("\r\nERROR!  Could not select Site records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                status = Constant.FAILURE;
            }

            /* While there are ES Site records to process */
            while ((rc = DynFeSite.FeFetchSite(siteHandle, out feSite, out feSiteNullInds)) == Constant.SUCCESS)
            {
                /* Count all records processed */
                totCount++;

                /* Check for "no op" record type */
                if (feSite.cmd[0] == Constant.CMD_NO_OP)
                {
                    // Just go on to fetch the next record.
                    continue;
                }

                // Instantiate a new MeSite object and associated nullInds and populate
                // these with values from site and nArrayFW for all 'shared' members. 
                Make.MeSiteFromFeSite(feSite, feSiteNullInds, out meSiteNew, out meSiteNewNullInds);

                // Set the userid.
                meSiteNew.userid = userNameIn;
                meSiteNewNullInds[MeSite.USERID] = Constant.DB_NOT_NULL;

                // Get the current the date and time.
                GenUtil.UtGetDateTime(out sysDate, out sysTime);

                // Set mdate to current date.
                meSiteNew.mdate = sysDate;
                meSiteNewNullInds[MeSite.MDATE] = Constant.DB_NOT_NULL;

                // Set mtime to current time.
                meSiteNew.mtime = sysTime;
                meSiteNewNullInds[MeSite.MTIME] = Constant.DB_NOT_NULL;

                // Set the string representations of the latitude and loingitude.
                GenUtil.FwConvertLat(meSiteNew.latit, out meSiteNew.strlatit, out meSiteNew.strlatits);
                meSiteNewNullInds[MeSite.STRLATIT] = Constant.DB_NOT_NULL;
                meSiteNewNullInds[MeSite.STRLATITS] = Constant.DB_NOT_NULL;

                GenUtil.FwConvertLong(meSiteNew.longit, out meSiteNew.strlongit, out meSiteNew.strlongits);
                meSiteNewNullInds[MeSite.STRLONGIT] = Constant.DB_NOT_NULL;
                meSiteNewNullInds[MeSite.STRLONGITS] = Constant.DB_NOT_NULL;

                //...Log2.v("\n\n" + meSiteNew.ToStringWN(meSiteNewNullInds));

                // Check if a record with same key already exists in the MDB.
                whereClause = String.Format("location = '{0}'", feSite.location);

                bool alreadyExistsInMdb = DynMeSite.MdbRecordExists(feSite.location);

                //...Log2.v("\n\nMdb.UpdateMeSiteTable(): alreadyExistsInMdb = " + alreadyExistsInMdb);

                if (alreadyExistsInMdb)
                {
                    rc = ValRetrieveES.FeValRetrieveMDBSite(out meSite, whereClause, out meSiteNullInds);

                    // Check if the retrieve site worked.
                    if (rc != Constant.SUCCESS)
                    {
                        Log2.e("\nMdb.UpdateMeSiteTable(): ERROR: call to FeValRetrieveMDBSite() failed.");
                        return rc;
                    }

                    // If the record already exists in the table then the only valid
                    // commands are 'U'pdate, 'B'lank, and 'D'elete.

                    /* Handle case of 'U'pdate or 'B'lank */
                    if ((feSite.cmd[0] == Constant.CMD_UPDATE) || (feSite.cmd[0] == Constant.CMD_BLANK))
                    {

                        /* Check modify date and time  */
                        if ((!feSite.mdate.Equals(meSite.mdate)) || (!feSite.mtime.Equals(meSite.mtime)))
                        {
                            /* Modify dates/times don't match */
                            Console.Write("\r\nWARNING!  This Site record has been modified since you retrieved it.\r\n");
                            Console.Write("Location = {0}", meSite.location);
                        }

                        // Update every column of the existing record in the 
                        // MDB main.me_site table.
                        nRet = DynMeSite.MeUpdateSite(hConn, meSiteNew, meSiteNewNullInds);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successful update of record in main.me_site.
                            updCount++;
                        }
                        else
                        {
                            Log2.e("\nMdb.UpdateMeSiteTable(): ERROR: call to MeUpdateSite() failed, nRet = " + nRet);
                            string str = String.Format("DynMeSite.MeUpdateSite04 -- Error writing Site: {0}\r\n",
                                                                                  feSite.location);
                            Ssutil.DbGetDiagStmt(hUpdate, str);
                            Console.Write("\r\nERROR! {0}", GenUtil.GetUserMess());
                            GenUtil.ReSetError();
                            status = Constant.FAILURE;
                        }

                    }
                    /* Handle case of 'D'elete */
                    else if (feSite.cmd[0] == Constant.CMD_DELETE)
                    {
                        /* If cmd is 'D', do a deletion */

                        nRet = DynMeSite.MeDeleteSite(hConn, feSite.location);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successful update of record in main.me_site.
                            delCount++;
                        }
                        else
                        {
                            Console.Write("\r\nERROR!  Could not Delete Site record\r\n");
                            Console.Write("Location = {0}\r\n", feSite.location);
                            ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                            status = Constant.FAILURE;
                        }
                    }
                    /* If not delete or update then error */
                    else
                    {
                        Console.Write("\r\nERROR!  Cannot Add this Site record.  It already exists in the MDB.\r\n");
                        Console.Write("Location = {0}\r\n", meSite.location);
                        status = Constant.FAILURE; /* set transaction status to error */
                    }

                    // Process the next FeSite record.
                    continue;

                }   /* End 'if record exists in MDB'  */

                else
                {

                    // ** Record does not exist in MDB ** 
                    // Can only perform an ADD function in this section of code, since   
                    // record does not exist.	    

                    // Handle the case of 'A'dd.
                    if (feSite.cmd[0] == Constant.CMD_ADD)
                    {
                        nRet = DynMeSite.MeInsertSite(hConn, meSiteNew, meSiteNewNullInds);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successfully inserted a recodr into main.me_site
                            addCount++;
                        }
                        else
                        {
                            Log2.e("\nMdb.UpdateMeSiteTable(): ERROR: call to MeInsertSite() failed, nRet = " + nRet);
                            string str = String.Format("DynMeSite.MeUpdateSitev02 -- Error inserting Site: {0} {1} {2}",
                                                        meSiteNew.location);
                            Ssutil.DbGetDiagStmt(hUpdate, str);
                            Console.Write("\r\nERROR! {0}", GenUtil.GetUserMess());
                            GenUtil.ReSetError();
                            status = Constant.FAILURE;
                        }

                    }
                    /* If not an add then error */
                    else
                    {
                        Console.Write("\r\nERROR!  Cannot Update/Delete this Site record.  ");
                        Console.Write("It does not exist in the MDB.\r\n");
                        Console.Write("Location = {0}\r\n", feSite.location);
                        status = Constant.FAILURE; /* set transaction status to error */
                    }

                    // Process the next FeSite record.
                    continue;   /* Continue on to next record */

                }   /* End  'else record does not exist in MDB' */

            } // fetch loop.

            /* Make sure all was OK from the fetch above */
            if (rc != Constant.NOMORERECS)
            {
                Log2.e("\nMdb.UpdateMeSiteTable(): ERROR: abnormal exit from fetch-loop, rc = " + rc);
                status = Constant.FAILURE;
                Console.Write("\r\nERROR!  Problem fetching Site records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            /* Close the open cursor */
            DynFeSite.FeCloseSite(siteHandle);

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);

            //...Log2.v("\n\nMdb.UpdateMeSiteTable(): Exit: status = " + status);

            /* Return status of the transaction so far */
            return (status);
        }

        /// <summary>
        /// This method performs the update/add/delete for the Change of Location records in the 
        /// ES-specific MDB table <b>main.me_cloc</b>.
        /// </summary>
        /// <param name="hConn"> - handle to an existing ODBC connection.</param>
        /// <param name="pdfName"> - kernel name of the ES PDF table set.</param>
        /// <param name="cLocCount"> - number of records affected by Change Location (LK).</param>
        /// <param name="userNameIn"> - MICS user ID.</param>
        /// <returns></returns>
        public static int UpdateMeCLoc(SQLHDBC hConn,
                                            string pdfName,
                                            ref int[] cLocCount,
                                            string userNameIn)
        {
            FeCLoc cLoc;    /* storage for pdf cLoc record */
            SQLLEN[] nArrayFW;  /* Ingres Null array */

            string userName;        /* INGRES aware userName var */
            string sysDate; /* System date for stamp */
            string sysTime; /* System time for stamp */

            /* Local variables */
            int cLocHandle;         /* storage for cLoc handle */
            int status = Constant.SUCCESS;        /* transaction status */
            int rc = Constant.SUCCESS;      /* Fctn call ret code */
            string fileName;    /* pdf name - long format */

            SQLRETURN sqlRet;
            SQLHANDLE hUpdate;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            /* Get userName in INGRES aware variable */
            userName = userNameIn;

            /* Get system date and time for stamp */
            GenUtil.UtGetDateTime(out sysDate, out sysTime);

            /* Get pdf name in long format */
            GenUtil.UtCvtName(Constant.FE_CLOC, pdfName, out fileName);

            /* Get a handle on change of location records */
            cLocHandle = DynFeCLoc.FeSelectCLoc(fileName, "", "");
            if (cLocHandle < 0)
            {
                Log2.e("\nMdb.UpdateMeCLoc(): ERROR: call to FeSelectCLoc() failed, cLocHandel = " + cLocHandle);
                Console.Write("\r\nERROR!  Could not select Change Location records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                status = Constant.FAILURE;
            }

            /* While there are cLoc records */
            while ((rc = DynFeCLoc.FeFetchCLoc(cLocHandle, out cLoc, out nArrayFW)) == Constant.SUCCESS)
            {
                // Update the MDB site records.
                DynMeSite.MeCLocUpdateSite(hConn, cLoc, userName, sysDate, sysTime, ref cLocCount[0]);

                // Update the antenna records.
                DynMeAnte.MeCLocUpdateAnte(hConn, cLoc, userName, sysDate, sysTime, ref cLocCount[1]);

                // Update the channel records.
                DynMeChan.MeCLocUpdateChan(hConn, cLoc, userName, sysDate, sysTime, ref cLocCount[2]);

                // Update the azimuth records.
                DynMeAzim.MeCLocUpdateAzim(hConn, cLoc, userName, sysDate, sysTime, ref cLocCount[3]);

            }   //  END while there are change records .

            // Make sure all was OK with the exit from the fetch-loop.
            if (rc != Constant.NOMORERECS)
            {
                Log2.e("\nMdb.UpdateMeCLoc(): ERROR: invalid exit from fetch-loop: rc = " + rc);
                status = Constant.FAILURE;
                Console.Write("\r\nERROR!  Problem fetching Change Location records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            /* Close the open cursor */
            DynFeCLoc.FeCloseCLoc(cLocHandle);
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);

            return (status);		/* return status to caller */
        }

        /// <summary>
        /// This method performs the update/add/delete for the Change of Call Sign records in the 
        /// ES-specific MDB table <b>main.me_ccal</b>.
        /// </summary>
        /// <param name="hConn"> - handle to an existing ODBC connection.</param>
        /// <param name="pdfName"> - kernel name of the ES PDF table set.</param>
        /// <param name="cCalCount"> - number of records affected by Change Call Sign (GK).</param>
        /// <param name="userNameIn"> - MICS user ID.</param>
        /// <returns></returns>
        public static int UpdateMeCCal(SQLHDBC hConn,
                                         string pdfName,
                                          ref int[] cCalCount,
                                         string userNameIn)
        {
            FeCCal feCCal;    /* storage for pdf cLoc record */
            SQLLEN[] nArrayFW;  /* Ingres Null array */

            string userName;        /* INGRES aware userName var */
            string sysDate; /* System date for stamp */
            string sysTime; /* System time for stamp */

            /* Local variables */
            int cCalHandle;         /* storage for cLoc handle */
            int status = Constant.SUCCESS;        /* transaction status */
            int rc = Constant.SUCCESS;      /* Fctn call ret code */
            string fileName;    /* pdf name - long format */

            SQLRETURN sqlRet;
            SQLHANDLE hUpdate;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            /* Get userName in INGRES aware variable */
            userName = userNameIn;

            /* Get system date and time for stamp */
            GenUtil.UtGetDateTime(out sysDate, out sysTime);

            /* Get pdf name in long format */
            GenUtil.UtCvtName(Constant.FE_CCAL, pdfName, out fileName);

            /* Get a handle on change of location records */
            cCalHandle = DynFeCCal.FeSelectCCal(fileName, "", "");
            if (cCalHandle < 0)
            {
                Log2.e("\nMdb.UpdateMeCCal(): ERROR: call to FeSelectCCal() failed, cLocHandel = " + cCalHandle);
                Console.Write("\r\nERROR!  Could not select Change Call Sign records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                status = Constant.FAILURE;
            }

            /* While there are CCal records */
            while ((rc = DynFeCCal.FeFetchCCal(cCalHandle, out feCCal, out nArrayFW)) == Constant.SUCCESS)
            {
                // Update the antenna records.
                DynMeAnte.MeCCalUpdateAnte(hConn, feCCal, userName, sysDate, sysTime, ref cCalCount[1]);

                // Update the channel records.
                DynMeChan.MeCCalUpdateChan(hConn, feCCal, userName, sysDate, sysTime, ref cCalCount[2]);

                // Update the azimuth records.
                DynMeAzim.MeCCalUpdateAzim(hConn, feCCal, userName, sysDate, sysTime, ref cCalCount[3]);

            }   //  END while there are change records .

            // Make sure all was OK with the exit from the fetch-loop.
            if (rc != Constant.NOMORERECS)
            {
                Log2.e("\nMdb.UpdateMeCCal(): ERROR: invalid exit from fetch-loop: rc = " + rc);
                status = Constant.FAILURE;
                Console.Write("\r\nERROR!  Problem fetching Change Call Sign records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            /* Close the open cursor */
            DynFeCCal.FeCloseCCal(cCalHandle);
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);

            return (status);		/* return status to caller */
        }










    }
}
