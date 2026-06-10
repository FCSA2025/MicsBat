# Documented File: Mdb.cs
**Repository Path:** `MtUpdate\Mdb.cs`
**Primary Layer:** `MtUpdate`
**Namespace:** `MtUpdate`

## Source Code Representation
```csharp
﻿using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtUpdate
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
    /// TS-specific tables in the Main Data Base (MDB).
    /// </summary>
    public class Mdb
    {

        /// <summary>
        /// This method performs the update/add/delete for channel records in the 
        /// TS-specific MDB table <b>main.mt_chan</b>.
        /// </summary>
        /// <param name="hConn"> - handle to an existing ODBC connection.</param>
        /// <param name="pdfName"> - kernel name of the TS PDF table set.</param>
        /// <param name="userNameIn"> - MICS user ID.</param>
        /// <param name="addCount"> - number of records added, site, antenna, channel.</param>
        /// <param name="delCount"> - number of records deleted, site, antenna, channel.</param>
        /// <param name="updCount"> - number of records updated, site, antenna, channel.</param>
        /// <param name="totCount"> - number of records processed, site, antenna, channel.</param>
        /// <returns></returns>
        public static int UpdateMtChanTable(SQLHDBC hConn,
                                            string pdfName,         // pdf name - short format 
                                            string userNameIn,      // MICS ID of current user 
                                            ref int addCount,       // number of records added 
                                            ref int delCount,       // number of records deleted 
                                            ref int updCount,       // number of records updated 
                                            ref int totCount)       // number of records processed 
        {
            //...Log2.v("\n\nMdb.UpdateMtChanTable(): Entry");

            FtChan ftChan;
            SQLLEN[] ftChanNullInds;

            MtChan mtChan;
            SQLLEN[] mtChanNullInds;

            MtChan mtChanNew;
            SQLLEN[] mtChanNewNullInds;

            string sysDate;
            string sysTime;

            // Local variables 
            int chanHandle;
            int status = Constant.SUCCESS;
            int rc = Constant.SUCCESS;
            int nRet;
            string whereClause;

            string fileName;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            // Convert pdf name to long format 
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out fileName);

            // Get a handle on the pdf channel records 
            chanHandle = DynChannel.FtSelectChannel(fileName, "", "");

            if (chanHandle < 0)
            {
                Console.Write("\r\nERROR!  Could not select Channel records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                status = Constant.FAILURE;
            }

            // While there are TS Channel records to process 
            while ((rc = DynChannel.FtFetchChannel(chanHandle, out ftChan, out ftChanNullInds)) == Constant.SUCCESS)
            {
                //...Log2.v("\nMdb.UpdateMtChanTable(): fetched ftChan: " + ftChan.KeysToString());

                // Count all records processed 
                totCount++;

                // Check for "no op" record type 
                if (ftChan.cmd[0] == Constant.CMD_NO_OP)
                {
                    // Just go on to fetch the next record.
                    continue;
                }

                // Instantiate a new MtChan object and associated nullInds and populate
                // these with values from ftChan and its nullInds for all 'shared' members. 
                Make.MtChanFromFtChan(ftChan, ftChanNullInds, out mtChanNew, out mtChanNewNullInds);

                // Set the userid.
                mtChanNew.userid = userNameIn;
                mtChanNewNullInds[MtChan.USERID] = Constant.DB_NOT_NULL;

                // Get the current the date and time.
                GenUtil.UtGetDateTime(out sysDate, out sysTime);

                // Set mdate to current date.
                mtChanNew.mdate = sysDate;
                mtChanNewNullInds[MtChan.MDATE] = Constant.DB_NOT_NULL;

                // Set mtime to current time.
                mtChanNew.mtime = sysTime;
                mtChanNewNullInds[MtChan.MTIME] = Constant.DB_NOT_NULL;

                //...Log2.v("\n\n" + mtChanNew.ToStringWN(mtChanNewNullInds));

                // Check if a record with same key already exists in the MDB.
                whereClause = String.Format("call1 = '{0}'  AND  call2 = '{1}' AND  bndcde = '{2}' AND chid = '{3}' ",
                                                ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid);

                bool alreadyExistsInMdb = DynMdbChannel.MdbRecordExists(ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid);

                //...Log2.v("\n\nMdb.UpdateMtChanTable(): alreadyExistsInMdb = " + alreadyExistsInMdb);

                // Process the existing record in main.mt_chan.
                if (alreadyExistsInMdb)
                {
                    rc = ValRetrieveTS.FtValRetrieveMDBChannel(out mtChan, whereClause, out mtChanNullInds);

                    // Check if the retrieve channel worked.
                    if (rc != Constant.SUCCESS)
                    {
                        Log2.e("\nMdb.UpdateMtChanTable(): ERROR: call to FtValRetrieveMDBChannel() failed.");
                        return rc;
                    }

                    // If the record already exists in the table then the only valid
                    // commands are 'U'pdate, 'B'lank, and 'D'elete.

                    // Handle case of 'U'pdate or 'B'lank 
                    if ((ftChan.cmd[0] == Constant.CMD_UPDATE) || (ftChan.cmd[0] == Constant.CMD_BLANK))
                    {
                        // Check modify date and time  
                        if ((!ftChan.mdate.Equals(mtChan.mdate)) || (!ftChan.mtime.Equals(mtChan.mtime)))
                        {
                            // Modify dates/times don't match 
                            Console.Write("\r\nWARNING!  This Channel record has been modified since you retrieved it.\r\n");
                            Console.Write("\r\n" + mtChan.KeysToString());
                        }

                        // Update every column of the existing record in the 
                        // MDB main.mt_chan table.
                        nRet = DynMdbChannel.MtUpdateChannel(hConn, mtChanNew, mtChanNewNullInds);

                        //...Log2.v("\nMdb.UpdateMtChanTable(): A: nRet = " + nRet);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successful update of record in main.mt_chan.
                            updCount++;
                        }
                        else
                        {
                            Log2.e("\nMdb.UpdateMtChanTable(): ERROR: call to MtUpdateChannel() failed, nRet = " + nRet);
                            string str = String.Format("DynMdbChannel.MtUpdateChan04 -- Error writing Channel: {0}\r\n", ftChan.KeysToString());
                            Ssutil.DbGetDiagStmt(hUpdate, str);
                            Console.Write("\r\nERROR! {0}", GenUtil.GetUserMess());
                            GenUtil.ReSetError();
                            status = Constant.FAILURE;
                        }

                    }
                    // Handle case of 'D'elete 
                    else if (ftChan.cmd[0] == Constant.CMD_DELETE)
                    {
                        nRet = DynMdbChannel.MtDeleteChannel(hConn, ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successful update of record in main.mt_chan.
                            delCount++;
                        }
                        else
                        {
                            Console.Write("\r\nERROR!  Could not Delete Channel record\r\n");
                            Console.Write("\r\n" + ftChan.KeysToString());
                            ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                            status = Constant.FAILURE;
                        }
                    }
                    // If not delete or update then error 
                    else
                    {
                        Console.Write("\r\nERROR!  Cannot Add this Channel record.  It already exists in the MDB.\r\n");
                        Console.Write("\r\n" + mtChan.KeysToString());
                        status = Constant.FAILURE; // set transaction status to error 
                    }

                    // Process the next FtChan record.
                    continue;

                }   // End 'if record exists in MDB'  

                // Process the case where a record does not already exist in MDB.
                else
                {
                    // In this case, only the ADD operation is valid.	    

                    // Handle the case of 'A'dd.
                    if (ftChan.cmd[0] == Constant.CMD_ADD)
                    {
                        nRet = DynMdbChannel.MtInsertChannel(hConn, mtChanNew, mtChanNewNullInds);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successfully inserted a recodr into main.mt_chan
                            addCount++;
                        }
                        else
                        {
                            Log2.e("\nMdb.UpdateMtChanTable(): ERROR: call to MtInsertChannel() failed, nRet = " + nRet);
                            string str = String.Format("DynMdbChannel.MtUpdateChannelv02 -- Error inserting Channel: " + mtChanNew.KeysToString());
                            Ssutil.DbGetDiagStmt(hUpdate, str);
                            Console.Write("\r\nERROR! {0}", GenUtil.GetUserMess());
                            GenUtil.ReSetError();
                            status = Constant.FAILURE;
                        }

                    }
                    // If not an add then error 
                    else
                    {
                        string str = String.Format("\n\nMdb.UpdateMtChanTable(): ERROR: cmd = '{0}' but chan with key {1} does not exist in {2}\n",
                                                    ftChan.cmd, ftChan.KeysToString(), DynMdbChannel.TableName);
                        Log2.e(str);
                        Console.Write("\r\nERROR!  Cannot Update/Delete this Channel record.  ");
                        Console.Write("It does not exist in the MDB.\r\n");
                        Console.Write("\r\n" + ftChan.KeysToString());
                        status = Constant.FAILURE; // set transaction status to error 
                    }

                    // Process the next FtChan record.
                    continue;   // Continue on to next record 

                }   // End  'else record does not exist in MDB' 

            } // fetch loop.

            // Make sure all was OK from the fetch above 
            if (rc != Constant.NOMORERECS)
            {
                Log2.e("\nMdb.UpdateMtChanTable(): ERROR: abnormal exit from fetch-loop, rc = " + rc);
                status = Constant.FAILURE;
                Console.Write("\r\nERROR!  Problem fetching Channel records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            // Close the open cursor 
            DynChannel.FtCloseChannel(chanHandle);

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);

            //...Log2.v("\n\nMdb.UpdateMtChanTable(): Exit: status = " + status);

            // Return status of the transaction so far 
            return (status);
        }

        /// <summary>
        /// This method performs the update/add/delete for antenna records in the 
        /// TS-specific MDB table <b>main.mt_ante</b>.
        /// </summary>
        /// <param name="hConn"> - handle to an existing ODBC connection.</param>
        /// <param name="pdfName"> - kernel name of the TS PDF table set.</param>
        /// <param name="userNameIn"> - MICS user ID.</param>
        /// <param name="addCount"> - number of records added, site, antenna, channel.</param>
        /// <param name="delCount"> - number of records deleted, site, antenna, channel.</param>
        /// <param name="updCount"> - number of records updated, site, antenna, channel.</param>
        /// <param name="totCount"> - number of records processed, site, antenna, channel.</param>
        /// <returns></returns>
        public static int UpdateMtAnteTable(SQLHANDLE hConn,
                                            string pdfName,
                                            string userNameIn,
                                            ref int addCount,
                                            ref int delCount,
                                            ref int updCount,
                                            ref int totCount
        )
        {
            //...Log2.v("\n\nMdb.UpdateMtAnteTable(): Entry");

            FtAnte ftAnte;
            SQLLEN[] ftAnteNullInds;

            MtAnte mtAnte;
            SQLLEN[] mtAnteNullInds;

            MtAnte mtAnteNew;
            SQLLEN[] mtAnteNewNullInds;

            string sysDate;
            string sysTime;

            // Local variables 
            int anteHandle;
            int status = Constant.SUCCESS;
            int rc = Constant.SUCCESS;
            int nRet;
            string whereClause;

            string fileName;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            // Copy MICS ID to Ingres aware variable 
            //userName = Info.MicsUserName;

            // Convert pdf name to long format 
            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out fileName);

            // Get a handle on the pdf ante records 
            anteHandle = DynAntenna.FtSelectAntenna(fileName, "", "");

            if (anteHandle < 0)
            {
                Console.Write("\r\nERROR!  Could not select Antenna records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                status = Constant.FAILURE;
            }

            // While there are TS Ante records to process 
            while ((rc = DynAntenna.FtFetchAntenna(anteHandle, out ftAnte, out ftAnteNullInds)) == Constant.SUCCESS)
            {
                // Count all records processed 
                totCount++;

                // Check for "no op" record type 
                if (ftAnte.cmd[0] == Constant.CMD_NO_OP)
                {
                    // Just go on to fetch the next record.
                    continue;
                }

                // Instantiate a new MtAnte object and associated nullInds and populate
                // these with values from ftAnte and its nullInds for all 'shared' members. 
                Make.MtAnteFromFtAnte(ftAnte, ftAnteNullInds, out mtAnteNew, out mtAnteNewNullInds);

                // Set the userid.
                mtAnteNew.userid = userNameIn;
                mtAnteNewNullInds[MtAnte.USERID] = Constant.DB_NOT_NULL;

                // Get the current the date and time.
                GenUtil.UtGetDateTime(out sysDate, out sysTime);

                // Set mdate to current date.
                mtAnteNew.mdate = sysDate;
                mtAnteNewNullInds[MtAnte.MDATE] = Constant.DB_NOT_NULL;

                // Set mtime to current time.
                mtAnteNew.mtime = sysTime;
                mtAnteNewNullInds[MtAnte.MTIME] = Constant.DB_NOT_NULL;

                //...Log2.v("\n\n" + mtAnteNew.ToStringWN(mtAnteNewNullInds));

                // Check if a record with same key already exists in the MDB.
                whereClause = String.Format(" call1='{0}' AND call2='{1}' AND bndcde='{2}' AND anum='{3}' ",
                                                ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum);

                bool alreadyExistsInMdb = DynMdbAntenna.MdbRecordExists(ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum);

                //...Log2.v("\n\nMdb.UpdateMtAnteTable(): alreadyExistsInMdb = " + alreadyExistsInMdb);

                if (alreadyExistsInMdb)
                {
                    rc = ValRetrieveTS.FtValRetrieveMDBAntenna(out mtAnte, whereClause, out mtAnteNullInds);

                    // Check if the retrieve ante worked.
                    if (rc != Constant.SUCCESS)
                    {
                        Log2.e("\nMdb.UpdateMtAnteTable(): ERROR: call to FtValRetrieveMDBAntenna() failed.");
                        Log2.e("\nKey = " + ftAnte.KeysToString());
                        return rc;
                    }

                    // If the record already exists in the table then the only valid
                    // commands are 'U'pdate, 'B'lank, and 'D'elete.

                    // Handle case of 'U'pdate or 'B'lank 
                    if ((ftAnte.cmd[0] == Constant.CMD_UPDATE) || (ftAnte.cmd[0] == Constant.CMD_BLANK))
                    {

                        // Check modify date and time  
                        if ((!ftAnte.mdate.Equals(mtAnte.mdate)) || (!ftAnte.mtime.Equals(mtAnte.mtime)))
                        {
                            // Modify dates/times don't match 
                            Console.Write("\r\nWARNING!  This Ante record has been modified since you retrieved it.\r\n");
                            Console.Write("\r\n" + mtAnte.KeysToString());
                        }

                        // Update every column of the existing record in the 
                        // MDB main.mt_ante table.
                        nRet = DynMdbAntenna.MtUpdateAntenna(hConn, mtAnteNew, mtAnteNewNullInds);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successful update of record in main.mt_ante.
                            updCount++;
                        }
                        else
                        {
                            Log2.e("\nMdb.UpdateMtAnteTable(): ERROR: call to MtUpdateAntenna() failed, nRet = " + nRet);
                            string str = String.Format("DynMdbAntenna.MtUpdateAnte04 -- Error writing Ante: {0}\r\n", ftAnte.KeysToString());
                            Ssutil.DbGetDiagStmt(hUpdate, str);
                            Console.Write("\r\nERROR! {0}", GenUtil.GetUserMess());
                            GenUtil.ReSetError();
                            status = Constant.FAILURE;
                        }

                    }
                    // Handle case of 'D'elete 
                    else if (ftAnte.cmd[0] == Constant.CMD_DELETE)
                    {
                        // If cmd is 'D', do a deletion 

                        nRet = DynMdbAntenna.MtDeleteAntenna(hConn, ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successful update of record in main.mt_ante.
                            delCount++;
                        }
                        else
                        {
                            Console.Write("\r\nERROR!  Could not Delete Ante record\r\n");
                            Console.Write("\r\n{0}", ftAnte.KeysToString());
                            ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                            status = Constant.FAILURE;
                        }
                    }
                    // If not delete or update then error 
                    else
                    {
                        Console.Write("\r\nERROR!  Cannot Add this Ante record.  It already exists in the MDB.\r\n");
                        Console.Write("\r\n" + mtAnte.KeysToString());
                        status = Constant.FAILURE; // set transaction status to error 
                    }

                    // Process the next FtAnte record.
                    continue;

                }   // End 'if record exists in MDB'  

                else
                {

                    // ** Record does not exist in MDB ** 
                    // Can only perform an ADD function in this section of code, since   
                    // record does not exist.	    

                    // Handle the case of 'A'dd.
                    if (ftAnte.cmd[0] == Constant.CMD_ADD)
                    {
                        nRet = DynMdbAntenna.MtInsertAntenna(hConn, mtAnteNew, mtAnteNewNullInds);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successfully inserted a recodr into main.mt_ante
                            addCount++;
                        }
                        else
                        {
                            Log2.e("\nMdb.UpdateMtAnteTable(): ERROR: call to MtInsertAntenna() failed, nRet = " + nRet);
                            string str = String.Format("DynMdbAntenna.MtUpdateAntev02 -- Error inserting mtAnte: " + mtAnteNew.KeysToString());
                            Ssutil.DbGetDiagStmt(hUpdate, str);
                            Console.Write("\r\nERROR! {0}", GenUtil.GetUserMess());
                            GenUtil.ReSetError();
                            status = Constant.FAILURE;
                        }

                    }
                    // If not an add then error 
                    else
                    {
                        string str = String.Format("\n\nMdb.UpdateMtAnteTable(): ERROR: cmd = '{0}' but ante with key {1} does not exist in {2}\n",
                            ftAnte.cmd, ftAnte.KeysToString(), DynMdbAntenna.TableName);
                        Log2.e(str);
                        Console.Write("\r\nERROR!  Cannot Update/Delete this Antenna record.  ");
                        Console.Write("It does not exist in the MDB.");
                        Console.Write("\r\n" + ftAnte.KeysToString());
                        status = Constant.FAILURE; // set transaction status to error 
                    }

                    // Process the next FtAnte record.
                    continue;   // Continue on to next record 

                }   // End  'else record does not exist in MDB' 

            } // fetch loop.

            // Make sure all was OK from the fetch above 
            if (rc != Constant.NOMORERECS)
            {
                Log2.e("\nMdb.UpdateMtAnteTable(): ERROR: abnormal exit from fetch-loop, rc = " + rc);
                status = Constant.FAILURE;
                Console.Write("\r\nERROR!  Problem fetching Ante records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            // Close the open cursor 
            DynAntenna.FtCloseAntenna(anteHandle);

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);

            //...Log2.v("\n\nMdb.UpdateMtAnteTable(): Exit: status = " + status);

            // Return status of the transaction so far 
            return (status);
        }

        /// <summary>
        /// This method performs the update/add/delete for site records in the 
        /// TS-specific MDB table <b>main.mt_site</b>.
        /// </summary>
        /// <param name="hConn"> - handle to an existing ODBC connection.</param>
        /// <param name="pdfName"> - kernel name of the TS PDF table set.</param>
        /// <param name="userNameIn"> - MICS user ID.</param>
        /// <param name="addCount"> - number of records added, site, antenna, channel.</param>
        /// <param name="delCount"> - number of records deleted, site, antenna, channel.</param>
        /// <param name="updCount"> - number of records updated, site, antenna, channel.</param>
        /// <param name="totCount"> - number of records processed, site, antenna, channel.</param>
        /// <returns></returns>
        public static int UpdateMtSiteTable(SQLHDBC hConn,
                                         string pdfName,
                                         string userNameIn,
                                         ref int addCount,
                                         ref int delCount,
                                         ref int updCount,
                                         ref int totCount)
        {
            //...Log2.v("\n\nMdb.UpdateMtSiteTable(): Entry");

            FtSite ftSite;
            SQLLEN[] ftSiteNullInds;

            MtSite mtSite;
            SQLLEN[] mtSiteNullInds;

            MtSite mtSiteNew;
            SQLLEN[] mtSiteNewNullInds;

            string sysDate;
            string sysTime;

            // Local variables 
            int siteHandle;
            int status = Constant.SUCCESS;
            int rc = Constant.SUCCESS;
            int nRet;
            string whereClause;

            string fileName;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            // Convert pdf name to long format 
            GenUtil.UtCvtName(Constant.FT_SITE, pdfName, out fileName);

            // Get a handle on the pdf site records 
            siteHandle = DynSite.FtSelectSite(fileName, "", "");

            if (siteHandle < 0)
            {
                Console.Write("\r\nERROR!  Could not select Site records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                status = Constant.FAILURE;
            }

            // While there are TS Site records to process 
            while ((rc = DynSite.FtFetchSite(siteHandle, out ftSite, out ftSiteNullInds)) == Constant.SUCCESS)
            {
                // Count all records processed 
                totCount++;

                // Check for "no op" record type 
                if (ftSite.cmd[0] == Constant.CMD_NO_OP)
                {
                    // Just go on to fetch the next record.
                    continue;
                }

                // Instantiate a new MtSite object and associated nullInds and populate
                // these with values from site and nArrayFW for all 'shared' members. 
                Make.MtSiteFromFtSite(ftSite, ftSiteNullInds, out mtSiteNew, out mtSiteNewNullInds);

                // Set the userid.
                mtSiteNew.userid = userNameIn;
                mtSiteNewNullInds[MtSite.USERID] = Constant.DB_NOT_NULL;

                // Get the current the date and time.
                GenUtil.UtGetDateTime(out sysDate, out sysTime);

                // Set mdate to current date.
                mtSiteNew.mdate = sysDate;
                mtSiteNewNullInds[MtSite.MDATE] = Constant.DB_NOT_NULL;

                // Set mtime to current time.
                mtSiteNew.mtime = sysTime;
                mtSiteNewNullInds[MtSite.MTIME] = Constant.DB_NOT_NULL;

                // Set the string representations of the latitude and loingitude.
                GenUtil.FwConvertLat(mtSiteNew.latit, out mtSiteNew.strlatit, out mtSiteNew.strlatits);
                mtSiteNewNullInds[MtSite.STRLATIT] = Constant.DB_NOT_NULL;
                mtSiteNewNullInds[MtSite.STRLATITS] = Constant.DB_NOT_NULL;

                GenUtil.FwConvertLong(mtSiteNew.longit, out mtSiteNew.strlongit, out mtSiteNew.strlongits);
                mtSiteNewNullInds[MtSite.STRLONGIT] = Constant.DB_NOT_NULL;
                mtSiteNewNullInds[MtSite.STRLONGITS] = Constant.DB_NOT_NULL;

                //...Log2.v("\n\n" + mtSiteNew.ToStringWN(mtSiteNewNullInds));

                // Check if a record with same key already exists in the MDB.
                whereClause = String.Format("call1 = '{0}'", ftSite.call1);

                bool alreadyExistsInMdb = DynMdbSite.MdbRecordExists(ftSite.call1);

                //...Log2.v("\n\nMdb.UpdateMtSiteTable(): alreadyExistsInMdb = " + alreadyExistsInMdb);

                if (alreadyExistsInMdb)
                {
                    rc = ValRetrieveTS.FtValRetrieveMDBSite(out mtSite, whereClause, out mtSiteNullInds);

                    // Check if the retrieve site worked.
                    if (rc != Constant.SUCCESS)
                    {
                        Log2.e("\nMdb.UpdateMtSiteTable(): ERROR: call to FtValRetrieveMDBSite() failed for:" + ftSite.KeysToString());
                        return rc;
                    }

                    // If the record already exists in the table then the only valid
                    // commands are 'U'pdate, 'B'lank, and 'D'elete.

                    // Handle case of 'U'pdate or 'B'lank 
                    if ((ftSite.cmd[0] == Constant.CMD_UPDATE) || (ftSite.cmd[0] == Constant.CMD_BLANK))
                    {

                        // Check modify date and time  
                        if ((!ftSite.mdate.Equals(mtSite.mdate)) || (!ftSite.mtime.Equals(mtSite.mtime)))
                        {
                            // Modify dates/times don't match 
                            Console.Write("\r\nWARNING!  This Site record has been modified since you retrieved it.\r\n");
                            Console.Write("\r\n" + mtSite.KeysToString());
                        }

                        // Update every column of the existing record in the 
                        // MDB main.mt_site table.
                        nRet = DynMdbSite.MtUpdateSite(hConn, mtSiteNew, mtSiteNewNullInds);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successful update of record in main.mt_site.
                            updCount++;
                        }
                        else
                        {
                            Log2.e("\nMdb.UpdateMtSiteTable(): ERROR: call to MtUpdateSite() failed, nRet = " + nRet);
                            string str = String.Format("DynMdbSite.MtUpdateSite04 -- Error writing Site: {0}\r\n", ftSite.KeysToString());
                            Ssutil.DbGetDiagStmt(hUpdate, str);
                            Console.Write("\r\nERROR! {0}", GenUtil.GetUserMess());
                            GenUtil.ReSetError();
                            status = Constant.FAILURE;
                        }

                    }
                    // Handle case of 'D'elete 
                    else if (ftSite.cmd[0] == Constant.CMD_DELETE)
                    {
                        // If cmd is 'D', do a deletion 

                        nRet = DynMdbSite.MtDeleteSite(hConn, ftSite.call1);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successful update of record in main.mt_site.
                            delCount++;
                        }
                        else
                        {
                            Console.Write("\r\nERROR!  Could not Delete Site record\r\n");
                            Console.Write("\r\n" + ftSite.KeysToString());
                            ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                            status = Constant.FAILURE;
                        }
                    }
                    // If not delete or update then error 
                    else
                    {
                        Console.Write("\r\nERROR!  Cannot Add this Site record.  It already exists in the MDB.\r\n");
                        Console.Write("\r\n" + mtSite.KeysToString());
                        status = Constant.FAILURE;
                    }

                    // Process the next FtSite record.
                    continue;

                }   // End 'if record exists in MDB'  

                else
                {

                    // ** Record does not exist in MDB ** 
                    // Can only perform an ADD function in this section of code, since   
                    // record does not exist.	    

                    // Handle the case of 'A'dd.
                    if (ftSite.cmd[0] == Constant.CMD_ADD)
                    {
                        nRet = DynMdbSite.MtInsertSite(hConn, mtSiteNew, mtSiteNewNullInds);

                        if (nRet == Constant.SUCCESS)
                        {
                            // Successfully inserted a recodr into main.mt_site
                            addCount++;
                        }
                        else
                        {
                            Log2.e("\nMdb.UpdateMtSiteTable(): ERROR: call to MtInsertSite() failed, nRet = " + nRet);
                            string str = String.Format("DynMdbSite.MtUpdateSitev02 -- Error inserting Site: {0}", mtSiteNew.KeysToString());
                            Ssutil.DbGetDiagStmt(hUpdate, str);
                            Console.Write("\r\nERROR! {0}", GenUtil.GetUserMess());
                            GenUtil.ReSetError();
                            status = Constant.FAILURE;
                        }

                    }
                    // If not an add then error 
                    else
                    {
                        string str = String.Format("\n\nMdb.UpdateMtSiteTable(): ERROR: cmd = '{0}' but site with key {1} does not exist in {2}\n",
                            ftSite.cmd, ftSite.KeysToString(), DynMdbSite.TableName);
                        Log2.e(str);
                        Console.Write("\r\nERROR!  Cannot Update/Delete this Site record.  ");
                        Console.Write("It does not exist in the MDB.\r\n");
                        Console.Write("\r\n" + ftSite.KeysToString());
                        status = Constant.FAILURE;
                    }

                    // Process the next FtSite record.
                    continue;

                }

            } // fetch loop.

            // Make sure all was OK from the fetch above 
            if (rc != Constant.NOMORERECS)
            {
                Log2.e("\nMdb.UpdateMtSiteTable(): ERROR: abnormal exit from fetch-loop, rc = " + rc);
                status = Constant.FAILURE;
                Console.Write("\r\nERROR!  Problem fetching Site records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            // Close the open cursor 
            DynSite.FtCloseSite(siteHandle);

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);

            //...Log2.v("\n\nMdb.UpdateMtSiteTable(): Exit: status = " + status);

            // Return status of the transaction so far 
            return (status);
        }



        /// <summary>
        /// This method performs the TS MDB update/add/delete operations for the Change of Call Sign 
        /// records in the TS PDF import file.
        /// </summary>
        /// <param name="hConn"> - handle to an existing ODBC connection.</param>
        /// <param name="pdfName"> - kernel name of the TS PDF table set.</param>
        /// <param name="chngCount"> - number of records affected by Change Call Sign (GK).</param>
        /// <param name="userNameIn"> - MICS user ID.</param>
        /// <returns></returns>
        public static int UpdateMtChng(SQLHDBC hConn,
                                         string pdfName,
                                          ref int[] chngCount,
                                         string userNameIn)
        {
            FtChng ftChng;
            SQLLEN[] nArrayFW;

            string sysDate;
            string sysTime;

            // Local variables 
            int chngHandle;
            int status = Constant.SUCCESS;
            int rc = Constant.SUCCESS;
            string fileName;

            SQLRETURN sqlRet;
            SQLHANDLE hUpdate;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            // Get system date and time for stamp 
            GenUtil.UtGetDateTime(out sysDate, out sysTime);

            // Get pdf name in long format 
            GenUtil.UtCvtName(Constant.FT_CHNG_CALL, pdfName, out fileName);

            // Get a handle on change of call sign records 
            chngHandle = DynChange.FtSelectChngCall(fileName, "", "");

            if (chngHandle < 0)
            {
                Log2.e("\nMdb.UpdateMtChng(): ERROR: call to FtSelectChng() failed, cLocHandel = " + chngHandle);
                Console.Write("\r\nERROR!  Could not select Change Call Sign records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                status = Constant.FAILURE;
            }

            // While there are Chng records 
            while ((rc = DynChange.FtFetchChngCall(chngHandle, out ftChng, out nArrayFW)) == Constant.SUCCESS)
            {
                // Update the channel records.
                DynMdbChannel.MtChngUpdateChannel(hConn, ftChng, userNameIn, sysDate, sysTime, ref chngCount[Constant.CHANINDEX]);

                // Update the antenna records.
                DynMdbAntenna.MtChngUpdateAntenna(hConn, ftChng, userNameIn, sysDate, sysTime, ref chngCount[Constant.ANTEINDEX]);

                // Update the site records.
                DynMdbSite.MtChngUpdateSite(hConn, ftChng, userNameIn, sysDate, sysTime, ref chngCount[Constant.SITEINDEX]);

                // Update the tower notes records.
                DynSdbTown.SdChngUpdateTown(hConn, ftChng, sysDate, sysTime, ref chngCount[Constant.TOWNINDEX]);

                // Update the route records.
                DynSdbRout.SdChngUpdateRout(hConn, ftChng, sysDate, sysTime, ref chngCount[Constant.ROUTINDEX]);

            }   //  END while there are change records .

            // Make sure all was OK with the exit from the fetch-loop.
            if (rc != Constant.NOMORERECS)
            {
                Log2.e("\nMdb.UpdateMtChng(): ERROR: invalid exit from fetch-loop: rc = " + rc);
                status = Constant.FAILURE;
                Console.Write("\r\nERROR!  Problem fetching Change Call Sign records.\r\n");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            // Close the open cursor 
            DynChange.FtCloseChngCall(chngHandle);
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hUpdate);

            return (status);
        }










    }
}


```
