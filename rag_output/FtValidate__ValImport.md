# Documented File: ValImport.cs
**Repository Path:** `FtValidate\ValImport.cs`
**Primary Layer:** `FtValidate`
**Namespace:** `FtValidate`

## Source Code Representation
```csharp
using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FtValidate
{

    using _Utillib;
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

    /// <summary>
    /// Provides methods relating to the import of TS site, antenna and channel records 
    /// and associated utility methods.
    /// </summary>
    public class ValImport
    {
#if PINVOKE
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern int ftSetAnteCmd([In] string pdfName, [In] string whereClause, [In] string cmdVal);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftFormWhereClause([In, Out] StringBuilder sb_formClauseInt, [In] string Acall1, [In] string Acall2, [In] string Abndcde, [In] short Aanum, [In] string Achid);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftImportAntenna([In] string shortName, [In] string condition, [In] string cmd);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftImportChannel([In] string shortName, [In] string condition, [In] string cmd);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftImportSite([In] string shortName, [In] string callsign, [In] string cmd);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern int ftSetChanCmd([In] string pdfName, [In] string whereClause, [In] string cmdVal);

        //----------------------------------------------------------------------

        public static int FtSetAnteCmd_NATIVE(string pdfName, string whereClause, string cmdVal)
        {
            return ftSetAnteCmd(pdfName, whereClause, cmdVal);
        }
        public static int FtSetChanCmd_NATIVE(string pdfName, string whereClause, string cmdVal)
        {
            return ftSetChanCmd(pdfName, whereClause, cmdVal);
        }

        public static void FtImportSite_NATIVE(string pdfName, string callsign, string cmd)
        {
            ftImportSite(pdfName, callsign, cmd);
        }

        public static void FtImportChannel_NATIVE(string shortName, string condition, string cmd)
        {
            ftImportChannel(shortName, condition, cmd);
        }

        public static void FtImportAntenna_NATIVE(string shortName, string condition, string cmd)
        {
            ftImportAntenna(shortName, condition, cmd);
        }

        public static void FtFormWhereClause_NATIVE(out string formClause, string Acall1, string Acall2, string Abndcde, short Aanum, string Achid)
        {
            //Use a StringBuilder to facilitate 'char * str' passed both [In] and [Out].
            StringBuilder sb_formClause = new StringBuilder(Constant.WHERE_SIZE);

            ftFormWhereClause(sb_formClause, Acall1, Acall2, Abndcde, Aanum, Achid);

            formClause = sb_formClause.ToString();

            //...Log2.v("\r\nValImport.FtFormWhereClause(): whereClause = " + formClause);
        }

#endif

        /// <summary>
        /// This method sets the cmd values for channel records in accordance with a set of criteria.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="whereClause"> - selection criteria of records to be changed.</param>
        /// <param name="cmdVal"> - the valid cmd value to be set</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int FtSetChanCmd(string pdfName, string whereClause, string cmdVal)
        {
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;

            int retCode = 0;
            string sqlStmt;  /* For dynamic MS SQL Server stmt */

            /* Local variables */
            string tableName;        /* Space for full table name */


            /* Get full table name */
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

            /* Prepare MS SQL Server statement */
            sqlStmt = String.Format("update {0} set cmd = '{1}' where {2}",
                                        tableName, cmdVal, whereClause);

            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlStmt, sqlStmt.Length);

            if (!ODBC.IsOK(sqlRet) && sqlRet != Constant.NOMORERECS)
            {
                retCode = Constant.FAILURE;
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return (retCode);
        }

        /// <summary>
        /// This method sets the cmd values for antenna records in accordance with a set of criteria.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="whereClause"> - selection criteria of records to be changed.</param>
        /// <param name="cmdVal"> - the valid cmd value to be set</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int FtSetAnteCmd(string pdfName, string whereClause, string cmdVal)
        {
            //...Log2.v(String.Format("\n\nValMerge.FtSetAnteCmd(): Entry: whereClause = {0}, cmdVal = {1}", whereClause, cmdVal));

            SQLHANDLE hStmt;
            SQLRETURN sqlRet;

            int retCode = 0;
            string sqlStmt;

            /* Local variables */
            string tableName;        /* Space for full table name */

            /* Get full table name */
            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);

            /* Prepare MS SQL Server statement */
            sqlStmt = String.Format("update {0} set cmd = '{1}' where {2}", tableName, cmdVal, whereClause);

            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            //...Log2.v("\r\nValMerge.FtSetAnteCmd(): SQLExecDirect():\r\n" + sqlStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlStmt, sqlStmt.Length);

            if (!ODBC.IsOK(sqlRet) && sqlRet != ODBC.SQL_NO_DATA)
            {
                retCode = Constant.FAILURE;
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            //...Log2.v("\n\nValMerge.FtSetAnteCmd(): Exit: retCode = " + retCode);
            return (retCode);
        }

        /// <summary>
        /// This method checks if a prescribed record is in the PDF; 
        /// if it is not then it copies it in from the MDB.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="callsign"> - call sign to pull in</param>
        /// <param name="cmd"> - command code to put in the record.</param>
        public static void FtImportSite(string pdfName, string callsign, string cmd)
        {
            //...Log2.v(String.Format("\n\nFtUtils.FtImportSite(): Entry: callsign = {0}, cmd = {1}", callsign, cmd));

            int nRet;
            MtSiteStr pSite;
            MtSiteStrNulls pSiteNull;
            FtSiteStr pftSite;
            FtSiteStrNulls pftNulls;

            nRet = MtUtils.MtGetSiteWN(callsign, out pSite, 1, out pSiteNull);

            if (nRet == Constant.SUCCESS)
            {
                /* verify that it is not in the PDF */

                nRet = FtUtils.FtGetSiteWN(pSite.stSite.call1, out pftSite, 1, pdfName, out pftNulls);

                if (nRet != Constant.SUCCESS)
                {
                    /*	The site is not in the pdf. */

                    pftSite = new FtSiteStr(FtSiteStr.Init.ALLOCATED);
                    pftNulls = new FtSiteStrNulls(FtSiteStrNulls.Init.ALLOCATED);

                    FtValCopy.FtCopySite(ref pftSite.stSite, pSite.stSite, ref pftNulls.anSiteNull, pSiteNull.anSiteNull);

                    pftSite.stSite.cmd = cmd;

                    pftSite.stSite.recstat = "C";

                    pftNulls.anSiteNull[FtSite.CMD] = Constant.DB_NOT_NULL;

                    pftNulls.anSiteNull[FtSite.RECSTAT] = Constant.DB_NOT_NULL;

                    int rc;

                    if ((rc = FtUtils.FtPutSiteWN(pftSite, pftNulls, 1, pdfName)) != Constant.SUCCESS)
                    {
                        Log2.e("\r\nFtUtils.FtImportSite(): ERROR: Could not insert site: pftSite.stSite.call1 = " + pftSite.stSite.call1 + " , FtUtils.FtPutSiteWN_NATIVE() returned " + rc);

                        ValErrs.AddMess("IMPORT - Could not insert site. Reason: %d.", pftSite.stSite.call1, "E", rc.ToString());
                    }
                }

            }

            //...Log2.v("\n\nFtUtils.FtImportSite(): Exit");
            return;
        }	/* ----- End of ftImportSite ----- */

        /// <summary>
        /// This method creates standard 'where' clauses for SQL queries.
        /// </summary>
        /// <param name="formClause"> - the created where clause.</param>
        /// <param name="Acall1"> - prescribed call sign 1.</param>
        /// <param name="Acall2"> - prescribed call sign 2.</param>
        /// <param name="Abndcde"> - prescribed band code.</param>
        /// <param name="Aanum"> - prescribed antenna number.</param>
        /// <param name="Achid"> - prescribed channel ID.</param>
        public static void FtFormWhereClause(out string formClause, /* area to put the clause */
                                             string Acall1,          /* valued requested for call sign 1 */
                                             string Acall2,          /* valued requested for call sign 2 */
                                             string Abndcde,     /* valued requested for band code  */
                                             short Aanum,           /* valued requested for antenna num */
                                             string Achid)           /* valued requested for channel ID */
        {
            //...Log2.v("\n\nValImport.FtFormWhereClause(): Entry");

            StringBuilder sb = new StringBuilder(" ");
            string str;
            bool oneOrMoreSubClauses = false;
            const string and = " and ";

            if (!String.IsNullOrWhiteSpace(Acall1))
            {
                str = String.Format("call1='{0}'", Acall1);
                oneOrMoreSubClauses = true;
                sb.Append(str);
            }

            if (!String.IsNullOrWhiteSpace(Acall2))
            {
                str = String.Format("call2='{0}'", Acall2);
                if (oneOrMoreSubClauses)
                {
                    sb.Append(and);
                }
                oneOrMoreSubClauses = true;
                sb.Append(str);
            }

            if (!String.IsNullOrWhiteSpace(Abndcde))
            {
                str = String.Format("bndcde='{0}'", Abndcde);
                if (oneOrMoreSubClauses)
                {
                    sb.Append(and);
                }
                oneOrMoreSubClauses = true;
                sb.Append(str);
            }

            if (!String.IsNullOrWhiteSpace(Achid))
            {
                str = String.Format("chid='{0}'", Achid);
                if (oneOrMoreSubClauses)
                {
                    sb.Append(and);
                }
                oneOrMoreSubClauses = true;
                sb.Append(str);
            }

            if (Aanum != Constant.NO_ANUM)
            {
                str = String.Format("anum={0}", Aanum);
                if (oneOrMoreSubClauses)
                {
                    sb.Append(and);
                }
                oneOrMoreSubClauses = true;
                sb.Append(str);
            }

            if (!oneOrMoreSubClauses)
            {
                str = String.Format("call1 like '%'");
                sb.Append(str);
            }

            formClause = sb.ToString();

            //...Log2.v("\n\nValImport.FtFormWhereClause(): Exit: " + formClause);

        }   /* ----- End of ftFormWhereClause ----- */

        /// <summary>
        /// Checks whether a prescribed record is in the PDF; if not then copy it in from the MDB.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="whereClause"> - string with condition clause (where clause).</param>
        /// <param name="cmd"> - command code to put in the record.</param>
        public static void FtImportChannel(string pdfName, string whereClause, string cmd)
        {
            //...Log2.v(String.Format("\n\nValImport.FtImportChannel(): Entry: whereClause = {0}, cmd = {1}", whereClause, cmd));

            //            struct mtChan_ mtChan;
            MtChan mtChan;
            //	DBNULLIND nArrayMDB[MT_CHAN_SIZE_];
            SQLLEN[] nArrayMDB;
            //        char dynClause[WHERE_SIZE],
            string dynClause;
            //                tableName[TABLE_NM_SZ];
            string tableName;
            //        DBNULLIND nArrayFW[FT_CHAN_SIZE_];
            SQLLEN[] nArrayFW = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            int rc;
            int cID1;
            //        struct ftChan_ tempChan;
            FtChan tempChan = new FtChan();
            int curHandle;
            int nRet;


            //    utCvtName(FT_CHAN, shortName, tableName);
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

            // Go through the channels in the mdb.  If they are in the pdf then use the
            // pdf version, if not, then copy in the mdb version.
            // exec sql open cmchan for readonly;
            curHandle = DynMdbChannel.MtSelectChannel(whereClause, "");
            //exec sql fetch cmchan into :mtChan:nArrayMDB;
            nRet = DynMdbChannel.MtFetchChannel(curHandle, out mtChan, out nArrayMDB);
            while (nRet == 0)
            {

                // Check to see if it is in the pdf.
                FtFormWhereClause(out dynClause, mtChan.call1, mtChan.call2, mtChan.bndcde, Constant.NO_ANUM, mtChan.chid);

                if ((cID1 = DynChannel.FtSelectChannel(tableName, dynClause, "")) < 0)
                {

                    ValErrs.AddMess("IMPORT - Could not read channel information. Reason: %d", tableName, "E", cID1.ToString());
                    rc = Constant.FAILURE;
                }
                else
                {
                    rc = DynChannel.FtFetchChannel(cID1, out tempChan, out nArrayFW);
                }

                if (rc != Constant.SUCCESS)
                {

                    FtValCopy.FtCopyChan(ref tempChan, mtChan, ref nArrayFW, nArrayMDB);

                    //strcpy(tempChan.recstat, "C");
                    tempChan.recstat = "C";
                    //safecopy(tempChan.cmd, cmd, sizeof(tempChan.cmd));
                    tempChan.cmd = cmd;
                    nArrayFW[FtChan.CMD] = Constant.DB_NOT_NULL;
                    nArrayFW[FtChan.RECSTAT] = Constant.DB_NOT_NULL;

                    if ((rc = (short)DynChannel.FtInsertChannel(cID1, tempChan, nArrayFW)) != Constant.SUCCESS)
                    {
                        string keyLine = ValErrs.MakeKeyLine(tempChan.call1, tempChan.call2, tempChan.bndcde, 0, tempChan.chid);
                        ValErrs.AddMess("IMPORT - Could not insert channel.  Reason: %d", keyLine, "E", rc.ToString());
                    }
                }
                else
                {
                    // The channel is in the pdf.  If powers have been changed,
                    // then we want to change all channels already in the pdf to at least
                    // the input command. GJS - 1127 - 2007.01 */
                    // char cCmd[2];
                    //...Log2.v("\nValImport.FtImportChannel(): before call to GenUtil.CmdMax(): CHARLIE");
                    string cCmd;
                    GenUtil.CmdMax(out cCmd, cmd, tempChan.cmd);

                    //            safecopy(tempChan.cmd, cCmd, sizeof(tempChan.cmd));
                    tempChan.cmd = cCmd;

                    if ((rc = DynChannel.FtUpdateChannel(cID1, tempChan, nArrayFW)) != Constant.SUCCESS)
                    {
                        string keyLine = ValErrs.MakeKeyLine(tempChan.call1, tempChan.call2, tempChan.bndcde, 0, tempChan.chid);
                        ValErrs.AddMess("IMPORT - Could not update channel.  Reason: %d", keyLine, "E", rc.ToString());
                    }
                }

                //        ftCloseChannel(cID1);
                DynChannel.FtCloseChannel(cID1);

                //exec sql fetch cmchan into :mtChan:nArrayMDB;
                nRet = DynMdbChannel.MtFetchChannel(curHandle, out mtChan, out nArrayMDB);
            }
            //exec sql close cmchan;
            DynMdbChannel.MtCloseChannel(curHandle);

            //...Log2.v("\n\nValImport.FtImportChannel(): Exit");
        }

        /// <summary>
        /// Performs the import of a channel and updates its rx fields with the tx fields.
        /// from the passed channel.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="whereClause"> - string with condition clause.</param>
        /// <param name="txChan"> - the remote channel object.</param>
        /// <param name="nullIndLoc"> - array of ODBC nullInds.</param>
        /// <param name="remoteRxUpdate"> - flags that the local TX is changing.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        public static void FtImportUpdateChannel(string pdfName, string whereClause, FtChan txChan, SQLLEN[] nullIndLoc, bool remoteRxUpdate,
                                                    ref short errCount, ref short warnCount)
        {
            //...Log2.v(String.Format("\n\nValImport.FtImportUpdateChannel(): Entry: whereClause = {0}, remoteRxUpdate = {1}", whereClause, remoteRxUpdate));

            MtChan mtChan;
            SQLLEN[] nArrayMDB;

            /* Local variables */
            string dynClause;
            string tableName;
            SQLLEN[] nArrayFW;
            short rc;
            int cID1;
            FtChan tempChan;

            int curHandle;
            int nRet;


            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

            /* Read records from the MDB  and process each record */
            //exec sql open cmchan for readonly;
            curHandle = DynMdbChannel.MtSelectChannel(whereClause, "");
            //exec sql fetch cmchan into :mtChan:nArrayMDB;
            nRet = DynMdbChannel.MtFetchChannel(curHandle, out mtChan, out nArrayMDB);
            while (nRet == 0)
            {

                /* Verify that chan rec is not in the PDF */
                ValImport.FtFormWhereClause(out dynClause, mtChan.call1, mtChan.call2, mtChan.bndcde, Constant.NO_ANUM, mtChan.chid);

                if ((cID1 = DynChannel.FtSelectChannel(tableName, dynClause, "")) < 0)
                {

                    ValErrs.AddMess("IMPORT - Could not read channel information. Reason: %d", tableName, "E", cID1.ToString());
                    rc = Constant.FAILURE;

                    DynChannel.FtCloseChannel(cID1);
                    //exec sql close cmchan;
                    DynMdbChannel.MtCloseChannel(curHandle);
                    return;
                }

                /* Is the record already in the PDF ? */
                if (DynChannel.FtFetchChannel(cID1, out tempChan, out nArrayFW) == Constant.SUCCESS)
                {
                    /* Record already in PDF */

                    /* Is the local TX side being updated ? */
                    if (remoteRxUpdate)
                    {

                        /* Verify that cmd values are reasonable
                        ** and merge remote record with MDB if
                        ** necessary */
                        rc = (short)ValBlank(txChan, nullIndLoc, ref tempChan, ref nArrayFW, mtChan, nArrayMDB, ref errCount, ref warnCount);

                        if (rc >= 0)
                        {
                            /* Come here if local cmd and remote
                             * cmd are consistent with each other.
                             */

                            /* Make remote RX data consistent with local TX data. */
                            SetRxData(ref tempChan, ref nArrayFW, txChan, nullIndLoc);


                            tempChan.cmd = txChan.cmd;
                            nArrayFW[FtChan.CMD] = Constant.DB_NOT_NULL;

                            if ((rc = (short)DynChannel.FtUpdateChannel(cID1, tempChan, nArrayFW)) != Constant.SUCCESS)
                            {
                                string keyLine = ValErrs.MakeKeyLine(tempChan.call1, tempChan.call2, tempChan.bndcde, 0, tempChan.chid);
                                ValErrs.AddMess("IMPORT - Could not update channel. Reason: %d", keyLine, "E", rc.ToString());
                            }

                        }   /* End if valBlank rc OK */

                    }   /* End if remoteRxUpdate */

                }
                else
                {
                    /* Record not in PDF. Add it to the PDF */
                    FtValCopy.FtCopyChan(ref tempChan, mtChan, ref nArrayFW, nArrayMDB);

                    tempChan.recstat = "C";
                    nArrayFW[FtChan.RECSTAT] = Constant.DB_NOT_NULL;

                    /* Is the local TX side being updated ? */
                    if (remoteRxUpdate)
                    {
                        /* Make remote RX data consistent with local TX data. */
                        SetRxData(ref tempChan, ref nArrayFW, txChan, nullIndLoc);
                    }


                    tempChan.cmd = txChan.cmd;
                    nArrayFW[FtChan.CMD] = Constant.DB_NOT_NULL;

                    if ((rc = (short)DynChannel.FtInsertChannel(cID1, tempChan, nArrayFW)) != Constant.SUCCESS)
                    {
                        string keyLine = ValErrs.MakeKeyLine(tempChan.call1, tempChan.call2, tempChan.bndcde, 0, tempChan.chid);
                        ValErrs.AddMess("IMPORT - Could not insert channel. Reason: %d", keyLine, "E", rc.ToString());
                    }

                }   /* End else record not in PDF */


                DynChannel.FtCloseChannel(cID1);
                //exec sql fetch cmchan into :mtChan:nArrayMDB;

                nRet = DynMdbChannel.MtFetchChannel(curHandle, out mtChan, out nArrayMDB);
            }   /* End while channel rec fetch is successful */

            //exec sql close cmchan;
            DynMdbChannel.MtCloseChannel(curHandle);

            //...Log2.v("\n\nValImport.FtImportUpdateChannel(): Exit");
        }   /* ----- End of ftImportUpdateChannel ----- */

        /// <summary>
        /// Performs validation steps for specific case of local command is 'B'.
        /// </summary>
        /// <remarks>
        /// If local cmd == 'B' then remote's cmd cannot be 'A' or 'D'.
        /// 
        /// If the remote cmd == 'U' then call chanMerge to merge the records.
        /// 	Now set remote cmd = 'B'.
        /// 
        /// If remote cmd == 'B' do nothing here.
        /// 
        /// If remote cmd == 'N' then replace it with a copy from the MDB and
        /// set cmd = 'B'. 
        /// </remarks>
        /// <param name="local"> - the local channel object.</param>
        /// <param name="nullIndLocal"> - array of ODBC nullInds for local channel.</param>
        /// <param name="remote"> - the remote channel object.</param>
        /// <param name="nullIndRemote"> - array of ODBC nullInds for remote channel.</param>
        /// <param name="mdb"> - remore channel record in the MDB.</param>
        /// <param name="nullIndMdb"> - array of ODBC nullInds for MDB channel.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int ValBlank(FtChan local, SQLLEN[] nullIndLocal, ref FtChan remote, ref SQLLEN[] nullIndRemote, MtChan mdb, SQLLEN[] nullIndMdb,
            ref short errCount, ref short warnCount)
        {
            //...Log2.v("\n\nValImport.ValBank(): Entry");

            /* Local variables */
            uint[] dummy = new uint[1];
            string keyLine;


            /* Is the cmd on the local record a 'blank' ? */
            if (!local.CmdEquals('B'))
            {
                /* Blanking done for 'B' type records only */
                return (0);
            }

            /* -- Come here if local cmd = 'B' -- */

            if (remote.CmdEquals('A') || remote.CmdEquals('D'))
            {
                /* remote cmd cannot be 'A' or 'D'. */
                keyLine = String.Format(" {0} {1} {2} {3}", remote.call1, remote.call2, remote.bndcde, remote.chid);
                ValErrs.AddMess("CMD field (%s) is inconsistent with remote's CMD field", keyLine, "E", local.cmd);

                errCount++;
                return (Constant.FAILURE);
            }

            if (remote.CmdEquals('N'))
            {
                /* Come here if remote is a No op. Replace the record with
                 * the MDB's record. */
                FtValCopy.FtCopyChan(ref remote, mdb, ref nullIndRemote, nullIndMdb);

            }
            else if (remote.CmdEquals('U'))
            {
                /* Remote RX data must be modified (posibly Blanked) therefore
                 * perform a Merge with the MDB. This sets things up for the
                 * change of the cmd from 'U' to 'B'.
                 */
                ValMerge.ChanMerge(mdb, nullIndMdb, ref remote, ref nullIndRemote, ref dummy);
            }

            //...Log2.v("\n\nValImport.ValBank(): Exit");
            return (Constant.SUCCESS);

        }	/* ----- End of valBlank ----- */

        /// <summary>
        /// Sets the remote channel's RX fields (those fields which are 
        /// controlled from the TX side.
        /// </summary>
        /// <param name="remote"> - remote channel object.</param>
        /// <param name="nullIndRem"> - array of ODBC nullInds for remote channel.</param>
        /// <param name="local"> - local channel object.</param>
        /// <param name="nullIndLoc"> - array of ODBC nullInds for remote channel.</param>
        public static void SetRxData(ref FtChan remote, ref SQLLEN[] nullIndRem, FtChan local, SQLLEN[] nullIndLoc)
        {
            //...Log2.v("\n\nValImport.SetRxData(): Entry");

            remote.freqrx = local.freqtx;
            nullIndRem[FtChan.FREQRX] = nullIndLoc[FtChan.FREQTX];

            remote.polrx = local.poltx;
            nullIndRem[FtChan.POLRX] = nullIndLoc[FtChan.POLTX];

            remote.srvcrx = local.srvctx;
            nullIndRem[FtChan.SRVCRX] = nullIndLoc[FtChan.SRVCTX];

            remote.statrx = local.stattx;
            nullIndRem[FtChan.STATRX] = nullIndLoc[FtChan.STATTX];

            /* If two-way channel is being made from a one-way channel
            ** then all TX information will be 'B'lanked on the local TX side.
            ** There are therefore some additional fields which we must
            ** take care of on the RX side of the remote channel, since
            ** some RX fields are not controlled automatically from the
            ** TX side. */

            /* If the local record is being 'B'lanked */
            if (local.cmd.Equals("B"))
            {
                /* If the local tx frequency is being nulled out */
                if (nullIndLoc[FtChan.FREQTX] == Constant.DB_NULL)
                {
                    /* Null out the following RX side fields */
                    nullIndRem[FtChan.ANTNUMBRX1] = Constant.DB_NULL;
                    nullIndRem[FtChan.ANTNUMBRX2] = Constant.DB_NULL;
                    nullIndRem[FtChan.ANTNUMBRX3] = Constant.DB_NULL;
                    nullIndRem[FtChan.AFSLRX1] = Constant.DB_NULL;
                    nullIndRem[FtChan.AFSLRX2] = Constant.DB_NULL;
                    nullIndRem[FtChan.AFSLRX3] = Constant.DB_NULL;
                    nullIndRem[FtChan.EQPTRX] = Constant.DB_NULL;
                    nullIndRem[FtChan.EQPTURX] = Constant.DB_NULL;
                    nullIndRem[FtChan.TRAFRX] = Constant.DB_NULL;
                    nullIndRem[FtChan.FEERX] = Constant.DB_NULL;
                }   /* End if local TX freq being nulled */

            }   /* End if local record being blanked */

            //...Log2.v("\n\nValImport.SetRxData(): Exit");
        }   /* ----- End of setRxData ----- */

        /// <summary>
        /// Check whether a prescribed record is in the PDF; if not then copy it in from the MDB.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="condition"> - string with condition clause (where clause)</param>
        /// <param name="cmd"> - command code to put in the record.</param>
        public static void FtImportAntenna(string pdfName, string condition, string cmd)
        {
            //...Log2.v("\n\nValImport.FtImportAntenna(): Entry: " + condition + "; cmd = " + cmd);

            MtAnte mtAnte;
            SQLLEN[] nArrayMDB;
            string dynClause;
            string tableName;
            SQLLEN[] nArrayFW = NullHelper.CreateArrayOfNullInd(FtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            int rc;
            int cID1;
            FtAnte tempAnte = new FtAnte();
            int curHandle;
            int nRet;

            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);

            // Go through the Antennas in the mdb.  If they are in the pdf then use the
            // pdf version, if not, then copy in the mdb version.
            // exec sql open cmchan for readonly;

            curHandle = DynMdbAntenna.MtSelectAntenna(condition, "");

            nRet = DynMdbAntenna.MtFetchAntenna(curHandle, out mtAnte, out nArrayMDB);

            while (nRet == 0)
            {
                // Verify that it is not in the PDF.
                dynClause = String.Format(" call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and anum = {3}",
                                                mtAnte.call1, mtAnte.call2, mtAnte.bndcde, mtAnte.anum);

                if ((cID1 = DynAntenna.FtSelectAntenna(tableName, dynClause, "")) < 0)
                {

                    ValErrs.AddMess("IMPORT - Could not read antenna information. Reason: %d", tableName, "E", cID1.ToString());
                    rc = Constant.FAILURE;
                }
                else
                {
                    rc = DynAntenna.FtFetchAntenna(cID1, out tempAnte, out nArrayFW);
                }

                if (rc != Constant.SUCCESS)
                {
                    //...Log2.v("\n\nValImport.FtImportAntenna(): PROTON");

                    // Not in the pdf.
                    FtValCopy.FtCopyAnte(ref tempAnte, mtAnte, ref nArrayFW, nArrayMDB);

                    tempAnte.cmd = cmd;
                    tempAnte.recstat = "C";

                    nArrayFW[FtAnte.CMD] = Constant.DB_NOT_NULL;
                    nArrayFW[FtAnte.RECSTAT] = Constant.DB_NOT_NULL;

                    if ((rc = DynAntenna.FtInsertAntenna(cID1, tempAnte, nArrayFW)) != Constant.SUCCESS)
                    {
                        string keyLine = ValErrs.MakeKeyLine(tempAnte.call1, tempAnte.call2, tempAnte.bndcde, tempAnte.anum, null);
                        ValErrs.AddMess("IMPORT - Could not insert Antenna.  Reason: %d", keyLine, "E", rc.ToString());
                    }
                }
                else
                {
                    // The Antenna is in the pdf.  If powers have been changed,
                    // then we want to change all Antennas already in the pdf to at least
                    // the input command. GJS - 1127 - 2007.01 */
                    // char cCmd[2];

                    // Bug fix: b150722A
                    // Only update the ftAnte table record if it has valid 'cmd'.
                    if (FtUtil.IsValidCmd(tempAnte.cmd))
                    {
                        //...Log2.v("\nValImport.FtImportChannel(): before call to GenUtil.CmdMax(): DELTA");
                        string cCmd;
                        GenUtil.CmdMax(out cCmd, cmd, tempAnte.cmd);

                        tempAnte.cmd = cCmd;

                        //...Log2.v("\nValImport.FtImportAntenna(): STAR: tempAnte: : cmd = " + tempAnte.cmd + ";   " + tempAnte.KeysToString());

                        if ((rc = DynAntenna.FtUpdateAntenna(cID1, tempAnte, nArrayFW)) != Constant.SUCCESS)
                        {
                            string keyLine = ValErrs.MakeKeyLine(tempAnte.call1, tempAnte.call2, tempAnte.bndcde, tempAnte.anum, null);
                            ValErrs.AddMess("ftImportAntenna - Could not update antenna for reason %d:-\r\n'%s'\r\n",
                                                        keyLine, "E", rc.ToString(), GenUtil.GetUserMess());
                            rc = -1;
                        }
                    }
                }

                DynAntenna.FtCloseAntenna(cID1);

                nRet = DynMdbAntenna.MtFetchAntenna(curHandle, out mtAnte, out nArrayMDB);
            }

            DynMdbAntenna.MtCloseAntenna(curHandle);

            //...Log2.v("\n\nValImport.FtImportAntenna(): Exit");
        }


    }
}

```
