# Documented File: ValChan.cs
**Repository Path:** `FtValidate\ValChan.cs`
**Primary Layer:** `FtValidate`
**Namespace:** `FtValidate`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FtValidate
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
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
    /// Provides methods relating to the validation of channels.
    /// </summary>
    public class ValChan
    {
#if PINVOKE
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValChanTS(string s, ref short sh1, ref short sh2, ref short sh3);

        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern int ftSiteSearch(string s, ref short sh1, ref short sh2);

        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValChanFieldsTS([In] FtChan ftChan, [In] SQLLEN[] nArrayFW, [In, Out] ref short errCount, [In, Out] ref short warnCount, [In] string pdfName,
                                            [In, Out] ref short valLevel, [In] string keyLine);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern double getAnteGain([In] string tableName, [In] string call1, [In] string call2, [In] string bndcde,
                                        [In] short anum, [Out] out double dist, [Out] out int retCode);
        //--------------------------------------------------------------------------------------

        public static double GetAnteGain_NATIVE(string tableName, string call1, string call2, string bndcde, short anum,
                                                     out double dist, out int retCode)
        {
            retCode = -666;
            return getAnteGain(tableName, call1, call2, bndcde, anum, out dist, out retCode);
        }

        public static void FtValChanFieldsTS_NATIVE(FtChan ftChan, SQLLEN[] nArrayFW, ref short errCount, ref short warnCount, string pdfName,
                                    ref short valLevel, string keyLine)
        {
            //...Log2.v("\n\nValChan.FtValChanFieldsTS_NATIVE(): Entry");
            ftValChanFieldsTS(ftChan, nArrayFW, ref errCount, ref warnCount, pdfName, ref valLevel, keyLine);
            //...Log2.v("\n\nValChan.FtValChanFieldsTS_NATIVE(): Exit");
        }

        public static int FtSiteSearch_NATIVE(string pdfName, ref short errorCount, ref short warningsCount)
        {
            return ftSiteSearch(pdfName, ref errorCount, ref warningsCount);
        }

        public static void FtValChanTS_NATIVE(string s, ref short sh1, ref short sh2, ref short sh3)
        {
            ftValChanTS(s, ref sh1, ref sh2, ref sh3);
        }
#endif

        /// <summary>
        /// Returns the gain of the antenna.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="call1"></param>
        /// <param name="call2"></param>
        /// <param name="bndcde"></param>
        /// <param name="anum"></param>
        /// <param name="dist"></param>
        /// <param name="retCode"></param>
        /// <returns>Antenna gain.</returns>
        public static double GetAnteGain(string tableName, string call1, string call2, string bndcde, short anum,
                                             out double dist, out int retCode)
        {
            // Satisfy 'out' requirements.
            dist = 0.0;
            retCode = 0;

            double again = 0.0;
            FtAnte antenna;

            int sID;
            SQLLEN[] nullIndAnte;
            string whereClause;
            int rc = Constant.SUCCESS;          /* Function call return code */

            /* Assemble selection clause for desired antennae */
            whereClause = String.Format("call1='{0}' and call2='{1}' and bndcde='{2}' and anum={3}",
                      call1.Trim(), call2.Trim(), bndcde.Trim(), anum);

            if ((sID = DynAntenna.FtSelectAntenna(tableName, whereClause, "")) < 0)
            {
                /* Can't select on table name */
                retCode = -1;
            }
            else
            {
                if ((rc = DynAntenna.FtFetchAntenna(sID, out antenna, out nullIndAnte)) == Constant.SUCCESS)
                {
                    if (nullIndAnte[FtAnte.DIST] != Constant.DB_NULL)
                    {
                        dist = antenna.dist;
                    }
                    else
                    {
                        dist = 0.001;  /*	Distance is used in logs.  Set it small */
                        retCode = -1;
                    }

                    /* get local antenna gain  */
                    rc = Suutils.GetLocAnteGain(antenna.acode, out again);
                    if (rc != 0)
                    {
                        /* Could not find antenna gain */
                        retCode = -1;

                    }
                    else
                    {
                        /* Got antenna gain ok */
                        retCode = 0;
                    }

                }
                else
                {
                    /* Can't get antenna */
                    retCode = -1;
                }
            }

            DynAntenna.FtCloseAntenna(sID);

            return (again);

        }

        /// <summary>
        /// Checks whether there are opposing sites whcih are not included in the PDF.
        /// </summary>
        /// <param name="pdfName"> - name of PDF.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <returns>Constant.SUCCESS = No missing sites were found; Constant.FAILURE = some missing sites were found.
        /// </returns>
        public static int FtSiteSearch(string pdfName, ref short errCount, ref short warnCount)
        {
            //...Log2.v("\n\nValChan.FtSiteSearch(): Entry");

            /* Local variables */
            string whereClause = "";
            string sortClause = "";
            string Call2 = "\0";
            string siteTableName = "";
            int siteCount;
            string chanTableName;
            int chanCursor;
            int retCode;

            FtChan ftChan;
            SQLLEN[] nArrayFW;

            sortClause = "call2";

            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out chanTableName);

            GenUtil.UtCvtName(Constant.FT_SITE, pdfName, out siteTableName);

            retCode = Constant.SUCCESS;

            /* read channel information */
            if ((chanCursor = DynChannel.FtSelectChannel(chanTableName, whereClause, sortClause)) < 0)
            {

                ValErrs.AddMess("Could not read channel information", pdfName, "W");
                warnCount++;
                return retCode;
            }

            while (DynChannel.FtFetchChannel(chanCursor, out ftChan, out nArrayFW) == Constant.SUCCESS)
            {
                // Bug fix:   b150722A
                // Delivered: 13-Jan-2020
                // If the ftChan has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftChan to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftChan.cmd))
                {
                    continue;
                }

                if (!ftChan.call2.Equals(Call2))
                {

                    Call2 = ftChan.call2;

                    whereClause = String.Format(" call1 = '{0}'", ftChan.call2);
                    siteCount = Ssutil.DbCountRows(siteTableName, whereClause);
                    if (siteCount == 0)
                    {

                        ValErrs.AddMess("Site %s has no opposing site: %s in the PDF.\r\nThis link will not be processed by TSIP.",
                                        pdfName, "W", ftChan.call1, Call2);
                        warnCount++;
                        retCode = Constant.FAILURE;
                    }
                    else if (siteCount < 0)
                    {

                        ValErrs.AddMess("Could not read site information for %s.  Reason: %d.",
                                        pdfName, "W", Call2, siteCount.ToString());
                        warnCount++;
                        return retCode;
                    }
                }
            }

            DynChannel.FtCloseChannel(chanCursor);

            //...Log2.v("\n\nValChan.FtSiteSearch(): Exit: retCode = " + retCode);
            return retCode;
        }    /* ********** END OF ftSiteSearch ********** */

        /// <summary>
        /// Validates a TS Channel.
        /// </summary>
        /// <param name="pdfName"> - name of PDF.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <param name="valLevel"> - validation level.</param>
        public static void FtValChanTS(string pdfName, ref short errCount, ref short warnCount, ref short valLevel)
        {
            //...Log2.v("\n\nValChan.FtValChanTS(): Entry");

            string whereClause = "";
            string tableName;
            string keyLine;
            int cID1;

            FtChan ftChan;
            SQLLEN[] nArrayFW;

            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

            /* read channel information */
            if ((cID1 = DynChannel.FtSelectChannel(tableName, whereClause, "call1,call2,bndcde,chid")) < 0)
            {
                ValErrs.AddMess("Could not read channel information reason: %d", tableName, "W", cID1.ToString());
                warnCount++;
                return;
            }

            while (DynChannel.FtFetchChannel(cID1, out ftChan, out nArrayFW) == Constant.SUCCESS)
            {
                // Bug fix: b150722A
                // If the ftChan has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftChan to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftChan.cmd))
                {
                    //continue;
                }

                keyLine = String.Format("Channel: {0} {1} {2} {3}", ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid);
                string str = String.Format("\r\nValidating {0} ({1})", keyLine, ftChan.cmd);
                //...Log2.v(str);

                char cmdChar = 'z';
                if (ftChan.cmd != null && ftChan.cmd.Length > 0)
                {
                    cmdChar = ftChan.cmd[0];
                }

                switch (cmdChar)
                {
                    case 'A':

                        FtValChanAddTS(ref ftChan, ref nArrayFW, pdfName, ref errCount, ref warnCount,
                                       ref valLevel, keyLine);
                        break;

                    case 'D':

                        FtValChanDelTS(ftChan, pdfName, ref errCount, ref warnCount, keyLine);
                        break;

                    case 'U':
                    case 'B':

                        FtValChanUpdtTS(ftChan, nArrayFW, pdfName, ref errCount, ref warnCount,
                                        ref valLevel, keyLine);
                        break;

                    case 'N':
                        /* Determine if record is in MDB */
                        whereClause = String.Format(" call1='{0}' and call2='{1}' and bndcde='{2}' and chid='{3}'",
                                ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid);

                        if (DynMdbChannel.DbCountRows(whereClause) <= 0)
                        {
                            /* Record not in MDB */
                            ValErrs.AddMess("Channel does not exist in MDB", keyLine, "E");
                            errCount++;
                        }
                        break;

                    default:
                        // Bug fix: b150722A.
                        // The ftChan's command was validated in a prior call to FtValChanTSIntra() in 
                        // FtValidate.Main(). Consequently, we don't need to validate it again.
                        break;
                }

                if (DynChannel.FtUpdateChannel(cID1, ftChan, nArrayFW) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("Could not Update Channel.", keyLine, "E");
                    errCount++;
                }
            }

            DynChannel.FtCloseChannel(cID1);

            //...Log2.v("\n\nValChan.FtValChanTS(): Exit");
        }   /* ----- End of ftValChanTS ----- */

        /// <summary>
        /// Validates TS channel records that are "ADD"
        /// </summary>
        /// <remarks>
        /// This method performs the channel validation for an ADD record.
        /// It verifies that the record does not exist and calls the field 
        /// validation routine to verify that all fields are correct.
        /// </remarks>
        /// <param name="ftChan"> - channel information.</param>
        /// <param name="nArrayFW"> - array of ODBC nullInds.</param>
        /// <param name="pdfName"> - name of PDF.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <param name="valLevel"> - validation level.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        public static void FtValChanAddTS(ref FtChan ftChan, ref SQLLEN[] nArrayFW,
            string pdfName, ref short errCount, ref short warnCount, ref short valLevel, string keyLine)
        {
            //...Log2.v("\n\nValChan.FtValChanAddTS(): Entry");

            /* Local variables */
            SQLLEN[] nArrayTemp = null,
            nArrayMDB;
            int rc;
            int ID1;
            string whereClause;
            string tableName;
            FtChan ftChan2 = null;
            MtChan mtChan;

            /* make sure channel does not already exist */
            ValImport.FtFormWhereClause(out whereClause, ftChan.call1, ftChan.call2, ftChan.bndcde, Constant.NO_ANUM, ftChan.chid);

            rc = ValRetrieveTS.FtValRetrieveMDBChannel(out mtChan, whereClause, out nArrayMDB);

            switch (rc)
            {
                case Constant.SUCCESS:
                    ValErrs.AddMess("Channel already exists in MDB.", keyLine, "E");
                    errCount++;
                    break;

                case Constant.NOMORERECS:
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Channel record locked in MDB", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Channel undefined error: %d", keyLine, "W", rc.ToString());
                    warnCount++;
                    break;
            }

            /* when adding, must add both sides at the same time */
            ValImport.FtFormWhereClause(out whereClause, ftChan.call2, ftChan.call1, ftChan.bndcde,
                                            Constant.NO_ANUM, ftChan.chid);

            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

            if ((ID1 = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read channel information: %d", keyLine, "W", ID1.ToString());
                warnCount++;
            }
            else
            {
                if (DynChannel.FtFetchChannel(ID1, out ftChan2, out nArrayTemp) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("Must add Both sides of Channel at the same time.", keyLine, "E");
                    errCount++;

                    DynChannel.FtCloseChannel(ID1);
                    return;
                }
                DynChannel.FtCloseChannel(ID1);
            }

            /* if add or update to RX side, get new values */
            ChannelCalculations(pdfName, ftChan, nArrayFW, ftChan2, nArrayTemp,
                                    ref warnCount, keyLine);

            FtValChanFieldsTS(ftChan, nArrayFW, ref errCount, ref warnCount, pdfName,
                                    ref valLevel, keyLine);

            //...Log2.v("\n\nValChan.FtValChanAddTS(): Exit");
        }/* ----- End of ftValChanAddTS ----- */

        /// <summary>
        /// Validates TS channel records that are "DELETE".
        /// </summary>
        /// <remarks>
        /// This method does all the validation for a delete. It verifies that the record 
        /// does in fact exist and retrieves all information below the record to delete
        /// as well as all information pointing to the record.
        /// </remarks>
        /// <param name="ftChan"> - channel information.</param>
        /// <param name="pdfName"> - name of PDF.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <param name="keyLine"> - validation level.</param>
        public static void FtValChanDelTS(FtChan ftChan, string pdfName, ref short errCount, ref short warnCount, string keyLine)
        {
            //...Log2.v("\n\nValChan.FtValChanDelTS(): Entry");

            MtChan mtChan;      /* MDB Channel record */
            SQLLEN[] nArrayMDB; /* Null ind for mtChan */
            string whereClause;  /* selection criteria */
            int chanHandle;         /* PDF Channel handle */
            FtChan remoteChan;     /* remote channel record */
            SQLLEN[] nArrayPDF; /* remote chan null ind */
            string tableName;
            int nCount;
            int nMDBHandle;

            /* make sure chan does exist */
            whereClause = String.Format(" call1='{0}' and call2='{1}' and bndcde='{2}' and chid='{3}'",
                                            ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid);

            nCount = DynMdbChannel.DbCountRows(whereClause);
            if (nCount <= 0)
            {
                /* Error - treat as record not found in  MDB */
                ValErrs.AddMess("Channel does not exist in MDB.", keyLine, "E");
                errCount++;
                return;
            }

            /* Come here if channel exists in MDB. Get other side of channel and
             * delete it also. Try reading other side from the PDF, if not in PDF
             * then get other side from MDB.
             */
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);
            whereClause = String.Format(" call1='{0}' and call2='{1}' and bndcde='{2}' and chid ='{3}'",
                    ftChan.call2, ftChan.call1, ftChan.bndcde, ftChan.chid);

            if ((chanHandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read channel information: %d", keyLine, "W", chanHandle.ToString());
                warnCount++;
                return;
            }

            if (DynChannel.FtFetchChannel(chanHandle, out remoteChan, out nArrayPDF) == Constant.SUCCESS)
            {
                /* Come here if remote side is in PDF */
                if (!remoteChan.CmdEquals('D'))
                {
                    if (remoteChan.recstat[0] == 'U')
                    {
                        /* Error. Other side must also be Delete. */
                        ValErrs.AddMess("Must delete both sides of channel.", keyLine, "E");
                        errCount++;
                    }
                    else
                    {
                        /* Change cmd to Delete */
                        remoteChan.cmd = "D";

                        DynChannel.FtUpdateChannel(chanHandle, remoteChan, nArrayPDF);
                    }
                }


                DynChannel.FtCloseChannel(chanHandle);

                return;
            }

            /* Come here if other side not in PDF. Get the other side from the
             * MDB and set the cmd = 'D'.
             */
            if ((nMDBHandle = DynMdbChannel.MtSelectChannel(whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read MDB channel information: %d", keyLine, "W", nMDBHandle.ToString());
                warnCount++;
                return;
            }

            if (DynMdbChannel.MtFetchChannel(nMDBHandle, out mtChan, out nArrayMDB) != Constant.SUCCESS)
            {
                /* Error. Other side does not exist in MDB. */
                ValErrs.AddMess("Remote side of channel not found in MDB.", keyLine, "E");
                errCount++;

                DynMdbChannel.MtCloseChannel(nMDBHandle);
                return;
            }

            DynMdbChannel.MtCloseChannel(nMDBHandle);

            /* Come here if record read from MDB. Insert the record into the
             * PDF.
             */
            FtValCopy.FtCopyChan(ref remoteChan, mtChan, ref nArrayPDF, nArrayMDB);

            remoteChan.cmd = "D";

            nArrayPDF[FtChan.CMD] = Constant.DB_NULL;

            remoteChan.recstat = "C";

            nArrayPDF[FtChan.RECSTAT] = Constant.DB_NULL;


            DynChannel.FtInsertChannel(chanHandle, remoteChan, nArrayPDF);

            DynChannel.FtCloseChannel(chanHandle);

            //...Log2.v("\n\nValChan.FtValChanDelTS(): Exit");
        }   /* ----- End of ftValChanDelTS ----- */

        /// <summary>
        /// Validates TS channel records that are "UPDATE".
        /// </summary>
        /// <remarks>
        /// This method performs the channel validation for an UPDATE record.
        /// It verifies that the record exists and calls the field validation routine to
        /// verify that all fields are correct.
        /// </remarks>
        /// <param name="ftChan"> - channel information</param>
        /// <param name="nArrayFW"> - array of ODBC nullInds.</param>
        /// <param name="pdfName"> - name of PDF.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <param name="valLevel"> - validation level.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        public static void FtValChanUpdtTS(FtChan ftChan, SQLLEN[] nArrayFW, string pdfName, ref short errCount, ref short warnCount, ref short valLevel, string keyLine)
        {
            //...Log2.v("\n\nValChan.ftValChanUpdtTS(): Entry");

            SQLLEN[] nArrayMDB;
            SQLLEN[] nullIndChanRem = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            int rc;
            int ID1;
            string whereClause,
                    tableName;
            MtChan mtChan;
            FtChan ftChan2 = null;
            bool IsOK = true;


            /* make sure chan does exist */
            ValImport.FtFormWhereClause(out whereClause, ftChan.call1, ftChan.call2, ftChan.bndcde,
                              Constant.NO_ANUM, ftChan.chid);

            rc = ValRetrieveTS.FtValRetrieveMDBChannel(out mtChan, whereClause, out nArrayMDB);

            switch (rc)
            {
                case Constant.SUCCESS:
                    break;

                case Constant.NOMORERECS:
                    ValErrs.AddMess("Channel does not exist in MDB.", keyLine, "E");
                    errCount++;
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Channel record locked  in MDB.", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Channel undefined error: %d", keyLine, "W", rc.ToString());
                    errCount++;
                    break;
            }

            if (rc != Constant.SUCCESS)
            {
                /* if we couldn't properly retrieve this record we don't want to
                   continue validating  */
                return;
            }

            /* Get the remote side of the channel record */
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

            whereClause = String.Format(
                    "call1='{0}' and call2='{1}' and bndcde='{2}' and chid='{3}'",
                            ftChan.call2, ftChan.call1, ftChan.bndcde, ftChan.chid);

            if ((ID1 = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read remote channel data, reason: %d", keyLine, "W", ID1.ToString());
                warnCount++;
                IsOK = false;      /*	We cannot go further here.  */
            }
            else
            {
                if (DynChannel.FtFetchChannel(ID1, out ftChan2, out nullIndChanRem) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("Cannot update RX data for remote channel.", keyLine, "W");
                    warnCount++;
                    IsOK = false;
                }
            }

            if (IsOK)
            {
                /*  We can do the channel calculations.  */
                if (nArrayFW[FtChan.FREQTX] != Constant.DB_NULL)
                {
                    /* Come here if TX or TX and RX channel */
                    if (!ftChan2.CmdEquals('B'))
                    {
                        ftChan2.cmd = "U";
                    }
                    nullIndChanRem[FtChan.CMD] = Constant.DB_NOT_NULL;
                    /*	Perform the channel calculations both ways, and store the
                    *		other way.  We will update locally later. */
                    ChannelCalculations(pdfName, ftChan, nArrayFW, ftChan2,
                                                            nullIndChanRem, ref warnCount, keyLine);

                    ChannelCalculations(pdfName, ftChan2, nullIndChanRem, ftChan,
                                                            nArrayFW, ref warnCount, keyLine);

                    DynChannel.FtUpdateChannel(ID1, ftChan2, nullIndChanRem);

                }
                else
                {
                    /* Come here if RX only channel */
                    ChannelCalculations(pdfName, ftChan, nArrayFW, ftChan2,
                                                            nullIndChanRem, ref warnCount, keyLine);
                }
            }
            else
            {
                ValErrs.AddMess("No channel calculations done. ", keyLine, "I");
            }

            DynChannel.FtCloseChannel(ID1);

            /* Check modify date and time */
            if (!ftChan.mdate.Equals(mtChan.mdate) || !ftChan.mtime.Equals(mtChan.mtime))
            {
                /* Modify dates/times don't match */
                ValErrs.AddMess("This Channel record has been modified since you retrieved it.",
                                keyLine, "W");

                //...Log2.v("\nValChan.ftValChanUpdtTS(): This Channel record has been modified since you retrieved it.");
                //...Log2.v(String.Format("\nValChan.ftValChanUpdtTS(): mtChan: {0}, mdate = {1}, mtime = {2}", mtChan.KeysToString(), mtChan.mdate, mtChan.mtime));
                //...Log2.v(String.Format("\nValChan.ftValChanUpdtTS(): ftChan: {0}, mdate = {1}, mtime = {2}", ftChan.KeysToString(), ftChan.mdate, ftChan.mtime));

                warnCount++;
            }

            FtValChanFieldsTS(ftChan, nArrayFW, ref errCount, ref warnCount, pdfName,
                                ref valLevel, keyLine);

            //...Log2.v("\n\nValChan.ftValChanUpdtTS(): Exit");
        }   /* ----- End of ftValChanUpdtTS ----- */

        /// <summary>
        /// Performs all calculations needed for the channel record.  It receives an FtChan object, 
        /// and populates it with the new values.
        /// </summary>
        /// <param name="pdfName"> - name of PDF.</param>
        /// <param name="chanLoc"> - local channel object.</param>
        /// <param name="nullIndLoc"> - array of ODBC nullInds for local channel object.</param>
        /// <param name="chanRem"> - remote channel object.</param>
        /// <param name="nullIndRem"> - array of ODBC nullInds for remote channel object.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <param name="keyLine"> - validation level.</param>
        public static void ChannelCalculations(string pdfName, FtChan chanLoc, SQLLEN[] nullIndLoc, FtChan chanRem, SQLLEN[] nullIndRem, ref short warnCount, string keyLine)
        {
            /* Local variables */
            double aGainRem = 0.0;
            double aGainLoc1 = 0.0;
            double aGainLoc2 = 0.0;
            double aGainLoc3 = 0.0;
            double remAnteDist = 0.0;
            double dist = 0.0;
            string anteName;
            string chanName;
            int locErrorCount = 0;          /* local error count */
            int retCode = Constant.SUCCESS;         /* Function call return code */
            int rc = Constant.SUCCESS;          /* Function call return code */

            string sqlCommand;

            if (chanLoc.call1[0] != 'A')
            {
                /* set freqrx to freqtx on remote side if not an Area Coordination site */
                chanLoc.freqrx = chanRem.freqtx;
                nullIndLoc[FtChan.FREQRX] = nullIndRem[FtChan.FREQTX];

                /* set polrx to poltx on remote side */
                chanLoc.polrx = chanRem.poltx;
                nullIndLoc[FtChan.POLRX] = nullIndRem[FtChan.POLTX];

                if (!chanLoc.traftx.Trim().Equals(chanRem.trafrx.Trim()))
                {
                    ValErrs.AddMess("Local transmit (%s) and Remote receive (%s) traffic codes do not agree.",
                                    keyLine, "W", chanLoc.traftx, chanRem.trafrx);
                    warnCount++;
                }
            }

            /* set srvcrx to srvctx on remote side */
            chanLoc.srvcrx = chanRem.srvctx;
            nullIndLoc[FtChan.SRVCRX] = nullIndRem[FtChan.SRVCTX];

            /* set statrx to stattx on remote side */
            chanLoc.statrx = chanRem.stattx;
            nullIndLoc[FtChan.STATRX] = nullIndRem[FtChan.STATTX];

            if (!chanLoc.feetx.Trim().Equals(chanRem.feerx.Trim()))
            {
                ValErrs.AddMess("Local transmit (%s) and Remote receive (%s) fee codes do not agree.",
                                keyLine, "W", chanLoc.feetx, chanRem.feerx);
                warnCount++;
            }

            /* retrieve main TX antenna for remote channel */
            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out anteName);
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out chanName);


            /* -- Calculate power values -- */

            /* If there is a passive antenna involved, then must
             * perform special calculations.  Passive's have call1's which
             * start with '%'.
            */
            if (chanLoc.call1.StartsWith("%") || chanLoc.call2.StartsWith("%"))
            {
                /* Perform calculations for passive antennae powers */
                if (FtValidate.runpassives)
                {
                    rc = ValPassive.FtCalcPassivePwrs(pdfName, ref chanLoc, ref nullIndLoc);

                    //...Log2.v("\nValChan.ChannelCalculations(): HERRING: rc = " + rc);

                    if (rc != Constant.SUCCESS)
                    {
                        ValErrs.AddMess("Passive power calculations are incorrect for reason %s\r\n(%s)",
                                        keyLine, "W", rc.ToString(), GenUtil.GetUserMess());
                        warnCount++;
                        locErrorCount++;                        /*	This has to be an error because of all
																		*		that can happen. */
                    }

                    /* As a final step we should verify that for the
                     * passive antennas the left side RX is equal to the
                     * right side TX
                     */
                    if ((rc == Constant.SUCCESS) && chanLoc.call1.StartsWith("%"))
                    {
                        FtChan tChan;
                        SQLLEN[] aNulls;

                        sqlCommand = String.Format(" call1 = '{0}' and call2 != '{1}' and bndcde = '{2}' and chid = '{3}'",
                                 chanLoc.call1, chanLoc.call2, chanLoc.bndcde, chanLoc.chid);
                        int nHandle = DynChannel.FtSelectChannel(chanName, sqlCommand, "");
                        int nRet = DynChannel.FtFetchChannel(nHandle, out tChan, out aNulls);
                        if (nRet != 0)
                        {
                            ValErrs.AddMess("Could not set reflected channel power", keyLine, "W");
                            warnCount++;
                            locErrorCount++;
                        }
                        else
                        {
                            tChan.pwrtx = chanLoc.pwrrx1;
                            aNulls[FtChan.PWRTX] = Constant.DB_NOT_NULL;
                            nRet = DynChannel.FtUpdateChannel(nHandle, tChan, aNulls);
                            if (nRet != 0)
                            {
                                ValErrs.AddMess("Could not update reflected channel power", keyLine, "W");
                                warnCount++;
                                locErrorCount++;
                            }
                        }
                        DynChannel.FtCloseChannel(nHandle);
                    }
                }
            }
            else
            {   /* - Calculations for regular antennae - */

                if (nullIndRem[FtChan.ANTNUMBTX1] != Constant.DB_NULL)
                {
                    aGainRem = GetAnteGain(anteName, chanRem.call1, chanRem.call2,
                                           chanRem.bndcde, chanRem.antnumbtx1, out remAnteDist, out retCode);
                    if (retCode < 0)
                    {
                        /* Can't get antenna  */
                        ValErrs.AddMess("Could not read remote channel's Antenna TX1 data.",
                                        keyLine, "W");
                        warnCount++;
                        locErrorCount++;
                    }
                }

                /* retrieve main RX antenna for local channel */
                if (nullIndLoc[FtChan.ANTNUMBRX1] != Constant.DB_NULL)
                {
                    aGainLoc1 = GetAnteGain(anteName, chanLoc.call1, chanLoc.call2,
                                            chanLoc.bndcde, chanLoc.antnumbrx1, out dist, out retCode);
                    if (retCode < 0)
                    {
                        /* Can't get antenna  */
                        ValErrs.AddMess("Could not read Antenna RX1 data.", keyLine, "W");
                        warnCount++;
                        locErrorCount++;
                    }
                }

                /* retrieve diversity RX 2 antenna for local channel */
                if (nullIndLoc[FtChan.ANTNUMBRX2] != Constant.DB_NULL)
                {
                    aGainLoc2 = GetAnteGain(anteName, chanLoc.call1, chanLoc.call2,
                                             chanLoc.bndcde, chanLoc.antnumbrx2, out dist, out retCode);
                    if (retCode < 0)
                    {
                        /* Can't get antenna  */
                        ValErrs.AddMess("Could not read Antenna RX2 data", keyLine, "W");
                        warnCount++;
                        locErrorCount++;
                    }
                }

                /* retrieve diversity RX 3antenna for local channel */
                if (nullIndLoc[FtChan.ANTNUMBRX3] != Constant.DB_NULL)
                {
                    aGainLoc3 = GetAnteGain(anteName, chanLoc.call1, chanLoc.call2,
                                             chanLoc.bndcde, chanLoc.antnumbrx3, out dist, out retCode);
                    if (retCode < 0)
                    {
                        /* Can't get antenna  */
                        ValErrs.AddMess("Could not read Antenna RX3 data.", keyLine, "W");
                        warnCount++;
                        locErrorCount++;
                    }
                }

                if (locErrorCount != 0)
                {
                    /* Come here if error occured on getting antenna data */
                    return;
                }

                if (chanLoc.call1[0] != 'A')
                {
                    /*	The other end calculations are not done for area coordination.  This
                    *		means any site whose call sign begins with A.
                    *		GJS - 1195 -	2005.03.21 */

                    /* note: for these calculations, dist must be in KM.
                     * If dist is zero, then we have a problem, so change dist to an
                     * acceptable value.
                     */
                    if (remAnteDist <= 0)
                    {
                        remAnteDist = 0.001;
                    }

                    /* calculate pwrrx1 */
                    if (nullIndRem[FtChan.FREQTX] != Constant.DB_NULL)
                    {
                        chanLoc.pwrrx1 = (float)GenUtil.RxPower(chanRem.pwrtx, chanRem.afsltx1, aGainRem,
                                                                            0.0, chanRem.freqtx, remAnteDist, aGainLoc1,
                                                                            0.0, chanLoc.afslrx1);
                        nullIndLoc[FtChan.PWRRX1] = Constant.DB_NOT_NULL;
                    }
                    else
                    {
                        nullIndLoc[FtChan.PWRRX1] = Constant.DB_NULL;
                    }

                    /* calculate pwrrx2 */
                    if (nullIndRem[FtChan.FREQTX] != Constant.DB_NULL &&
                        nullIndLoc[FtChan.ANTNUMBRX2] != Constant.DB_NULL)
                    {
                        chanLoc.pwrrx2 = (float)GenUtil.RxPower(chanRem.pwrtx, chanRem.afsltx1, aGainRem,
                                                                            0.0, chanRem.freqtx, remAnteDist, aGainLoc2,
                                                                            0.0, chanLoc.afslrx2);
                        nullIndLoc[FtChan.PWRRX2] = Constant.DB_NOT_NULL;
                    }
                    else
                    {
                        nullIndLoc[FtChan.PWRRX2] = Constant.DB_NULL;
                    }

                    /* calculate pwrrx3 */
                    if (nullIndRem[FtChan.FREQTX] != Constant.DB_NULL &&
                        nullIndLoc[FtChan.ANTNUMBRX3] != Constant.DB_NULL)
                    {
                        chanLoc.pwrrx3 = (float)GenUtil.RxPower(chanRem.pwrtx, chanRem.afsltx1, aGainRem,
                                                                            0.0, chanRem.freqtx, remAnteDist, aGainLoc3,
                                                                            0.0, chanLoc.afslrx3);
                        nullIndLoc[FtChan.PWRRX3] = Constant.DB_NOT_NULL;
                    }
                    else
                    {
                        nullIndLoc[FtChan.PWRRX3] = Constant.DB_NULL;
                    }
                }
            }   /* End else calculations for regular antennae */
        }   /* ----- End of channelCalculations ----- */

        /// <summary>
        /// Validates all of the TS channel fields.
        /// </summary>
        /// <remarks>
        /// This method validates all individual fields on the channel record.
        /// This validation is done for both an add and  an update.
        /// </remarks>
        /// <param name="ftChan"> - channel information.</param>
        /// <param name="nArrayFW"> - array of ODBC nullInds.</param>
        /// <param name="errorCount"> - cummulative error count.</param>
        /// <param name="warningCount"> - cummulative warning count.</param>
        /// <param name="pdfName"> - name of PDF.</param>
        /// <param name="valLevel"> - validation level.</param>
        /// <param name="keyLine"> - key information if necessary to print.</param>
        public static void FtValChanFieldsTS(FtChan ftChan, SQLLEN[] nArrayFW, ref short errorCount, ref short warningCount, string pdfName,
                                            ref short valLevel, string keyLine)
        {
            //...Log2.v("\n\nValChan.FtValChanFieldsTS(): Entry");

            string whereClause;

            FtSite ftSite;
            short recExist;

            /* Local variables */
            SQLLEN[] nArrayTempSite;
            SQLLEN[] nArrayTemp;
            int ID1;
            int siteFound;
            string auseTX;
            string auseRX;
            string tableName;
            string calPol;          /* calculated polarization */
            FtAnte ftAnte;
            int nRet;
            SuBand pBand = null;       // Needed in several places

            if (nArrayFW[FtChan.PWRTX] != Constant.DB_NULL)
            {
                /* Check that the transmit power is within reasonable values and not passive
                */
                if ((ftChan.pwrtx < 0 || ftChan.pwrtx > 50.0) && ftChan.call1[0] != '%')
                {
                    ValErrs.AddMess("WARNING -- Transmit power recommended between 0 and 50dbm.",
                                    keyLine, "W");
                    warningCount++;
                }
            }

            /* retrieve site record */
            siteFound = ValGetRecs.GetSite(pdfName, ftChan.call1, out ftSite, out nArrayTempSite);
            if (siteFound == Constant.FAILURE)
            {
                ValErrs.AddMess("Site %s not found.", keyLine, "E", ftChan.call1);
                errorCount++;
            }


            /* verify antenna numbers */
            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);

            if (nArrayFW[FtChan.FREQTX] != Constant.DB_NULL)
            {
                /* verify all TX side fields */

                ValImport.FtFormWhereClause(out whereClause, ftChan.call1, ftChan.call2, ftChan.bndcde,
                                                    ftChan.antnumbtx1, "");

                if ((ID1 = DynAntenna.FtSelectAntenna(tableName, whereClause, "")) < 0)
                {
                    ValErrs.AddMess("Could not read antenna information from PDF, cause: %d.",
                                    keyLine, "W");
                    warningCount++;
                }
                else
                {
                    if (DynAntenna.FtFetchAntenna(ID1, out ftAnte, out nArrayTemp) != Constant.SUCCESS)
                    {
                        ValErrs.AddMess("No main TX Antenna for this channel.", keyLine, "E");
                        errorCount++;
                        auseTX = "";
                    }
                    else
                    {
                        auseTX = ftAnte.ause;
                        if (!auseTX.StartsWith("TX") &&
                            !auseTX.StartsWith("TR"))
                        {
                            ValErrs.AddMess("Invalid Antenna Use (%s) for main TX Antenna %d",
                                     keyLine, "E", auseTX, ftChan.antnumbtx1.ToString());
                            errorCount++;
                        }
                        /*  check the feed system loss if this is not passive  */
                        if (nArrayFW[FtChan.AFSLTX1] != Constant.DB_NULL &&
                                ftChan.afsltx1 <= 0.0 &&
                                ftChan.call1[0] != '%')
                        {
                            ValErrs.AddMess("Afsltx1 is 0", keyLine, "W");
                            warningCount++;
                        }
                    }
                    DynAntenna.FtCloseAntenna(ID1);
                }


                /* verify diversity TX antenna */

                if (nArrayFW[FtChan.ANTNUMBTX2] != Constant.DB_NULL)
                {
                    /* verify TX diversity antenna  */
                    ValImport.FtFormWhereClause(out whereClause, ftChan.call1, ftChan.call2,
                                      ftChan.bndcde, ftChan.antnumbtx2, "");

                    if ((ID1 = DynAntenna.FtSelectAntenna(tableName, whereClause, "")) < 0)
                    {
                        ValErrs.AddMess("Could not read antenna information. Reason: %d", keyLine, "W", ID1.ToString());
                        warningCount++;
                    }
                    else
                    {
                        if (DynAntenna.FtFetchAntenna(ID1, out ftAnte, out nArrayTemp) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("No TX or STX Antenna %d for this channel",
                                            keyLine, "E", ftChan.antnumbtx2.ToString());
                            errorCount++;
                        }
                        else
                        { /* antenna record was found */
                            if (!ftAnte.ause.StartsWith("STX") &&
                                !ftAnte.ause.StartsWith("TR") &&
                                    !ftAnte.ause.StartsWith("TX"))
                            {
                                if (!ftAnte.ause.StartsWith("DV1") && !ftAnte.ause.StartsWith("DV2"))
                                {
                                    ValErrs.AddMess("Invalid Ause for TX or STX Antenna %d.",
                                             keyLine, "E", ftChan.antnumbtx2.ToString());
                                    errorCount++;
                                }
                                else
                                {
                                    ValErrs.AddMess("Transmit channel refer to an antenna (%d) marked as diversity (DV1 or DV2).",
                                                                keyLine, "W", ftChan.antnumbtx2.ToString());
                                    warningCount++;
                                }
                            }

                            /*	Check that, if there is an antenna, there is a feed system loss */
                            if (nArrayFW[FtChan.AFSLTX2] != Constant.DB_NULL &&
                                    ftChan.afsltx2 <= 0 &&
                                    ftChan.call1[0] != '%')
                            {
                                ValErrs.AddMess("Antenna secondary FSL is 0", keyLine, "W");
                                warningCount++;
                            }
                        }
                        DynAntenna.FtCloseAntenna(ID1);
                    }
                }

                /* verify note tx field against the SDB */

                if ((nArrayFW[FtChan.NOTETX] != Constant.DB_NULL) && (siteFound == Constant.SUCCESS))
                {
                    SuNote tNote;

                    nRet = Suutils.SuGetNote(ftSite.oper, ftChan.notetx, out tNote);
                    if (nRet != 0)
                    {
                        ValErrs.AddMess("NoteTX (%s) not present in SDB for Operator :%s:",
                                 keyLine, "E", ftChan.notetx, ftSite.oper);
                        errorCount++;
                    }
                }


                /* verify eqpt against the SDB */

                if (nArrayFW[FtChan.EQPTTX] != Constant.DB_NULL)
                {
                    if (!ftChan.eqpttx.StartsWith("$"))
                    {
                        SuEqpt tEqpt;

                        nRet = Suutils.SuGetEqpt(ftChan.eqpttx, out tEqpt);
                        if (nRet != 0)
                        {
                            ValErrs.AddMess("TX Equipment '%s' not present in SDB",
                                     keyLine, "E", ftChan.eqpttx);
                            errorCount++;
                        }
                    }
                    else
                    {
                        /* Temporary codes in use - valid only for TSIP */
                        valLevel = Constant.TSIP;
                    }
                }


                /* verify the FEE code */

                if (nArrayFW[FtChan.FEETX] != Constant.DB_NULL)
                {
                    SuFeeCode tFee;
                    recExist = (short)Suutils.SuGetFeeCode(ftChan.feetx, out tFee);
                    if (recExist != 0)
                    {
                        ValErrs.AddMess("FeeTX (%s) must be 1-9 or A-T,V,X", keyLine, "E", ftChan.feetx);
                        errorCount++;
                    }
                }
                else 
                {
                    ValErrs.AddMess("FeeTX must be 1-9 or A-T,V,X", keyLine, "E");
                    // The following enters a blank line after the previous AddMess();
                    ValErrs.AddMess("", keyLine, "E");
                    errorCount++;
                }

                /* verify the traffic code */
                if (nArrayFW[FtChan.TRAFTX] != Constant.DB_NULL)
                {
                    if ((ftChan.traftx[0] != 'A') &&
                        (ftChan.traftx[0] != '$'))
                    {
                        SuTraf tTraf;
                        int nRet_ = Suutils.SuGetTraf(ftChan.traftx, ftChan.eqpttx, out tTraf);
                        if (nRet_ != 0)
                        {
                            ValErrs.AddMess("TX Traffic code (%s) & Equipment (%s) pair not present in the SDB.",
                                                        keyLine, "W", ftChan.traftx, ftChan.eqpttx);
                            warningCount++;
                        }

                    }
                    else if (ftChan.traftx[0] == '$')
                    {
                        /* Temporary codes being used, only valid for TSIP */
                        valLevel = Constant.TSIP;
                    }
                    else
                    {
                        SuTraf tTraf;
                        int nRet_ = Suutils.SuGetTraf(ftChan.traftx, null, out tTraf);
                        if (nRet_ != 0)
                        {
                            ValErrs.AddMess("TX Traffic Code (%s) not present in the SDB.",
                                                        keyLine, "E", ftChan.traftx);
                            errorCount++;
                        }
                    }
                }

            }  /* end of tx side */


            /* If an RX Frequency is present, must validate RX material */
            if (nArrayFW[FtChan.FREQRX] != Constant.DB_NULL)
            {
                /* verify the RX side fields */

                /* verify the antenna numbers */
                ValImport.FtFormWhereClause(out whereClause, ftChan.call1, ftChan.call2, ftChan.bndcde,
                                  ftChan.antnumbrx1, "");

                /*
                Check that an antenna number has been selected.  If it has
                then attempt to find that antenna and verify
                */

                if (nArrayFW[FtChan.ANTNUMBRX1] != Constant.DB_NULL)
                {
                    if ((ID1 = DynAntenna.FtSelectAntenna(tableName, whereClause, "")) < 0)
                    {
                        ValErrs.AddMess("Could not read antenna information from PDF.  Reason: %d ",
                                 keyLine, "W", ID1.ToString());
                        warningCount++;

                        ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    }
                    else
                    {
                        if (DynAntenna.FtFetchAntenna(ID1, out ftAnte, out nArrayTemp) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("No Main RX Antenna for this channel\r\n", keyLine, "E");
                            errorCount++;

                            //memset(auseRX, 0, sizeof(auseRX));
                            auseRX = "";
                        }
                        else
                        {
                            auseRX = ftAnte.ause;
                            if (!auseRX.StartsWith("RX") && !auseRX.StartsWith("TR"))
                            {
                                ValErrs.AddMess("Invalid Ause (%s) for main RX Antenna %d.",
                                keyLine, "E", auseRX, ftChan.antnumbrx1.ToString());
                                errorCount++;
                            }

                            if (nArrayFW[FtChan.AFSLRX1] != Constant.DB_NULL &&
                                    ftChan.afslrx1 <= 0 &&
                                    ftChan.call1[0] != '%')
                            {
                                ValErrs.AddMess("Afslrx1 is 0", keyLine, "W");
                                warningCount++;
                            }
                        }
                        DynAntenna.FtCloseAntenna(ID1);
                    }
                }
                else
                {
                    ValErrs.AddMess("No Main RX Antenna for this channel", keyLine, "E");
                    errorCount++;

                    //memset(auseRX, 0, sizeof(auseRX));
                    auseRX = "";
                }


                /* verify RX diversity antenna  */

                if (nArrayFW[FtChan.ANTNUMBRX2] != Constant.DB_NULL)
                {

                    ValImport.FtFormWhereClause(out whereClause, ftChan.call1, ftChan.call2,
                                      ftChan.bndcde, ftChan.antnumbrx2, "");
                    if ((ID1 = DynAntenna.FtSelectAntenna(tableName, whereClause, "")) < 0)
                    {
                        ValErrs.AddMess("Could not read antenna information. Reason %d", keyLine, "W", ID1.ToString());
                        warningCount++;
                    }
                    else
                    {
                        if (DynAntenna.FtFetchAntenna(ID1, out ftAnte, out nArrayTemp) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("No RX DV1 Antenna for this channel.", keyLine, "E");
                            errorCount++;
                        }
                        else
                        { /* antenna record was found */
                            if (!ftAnte.ause.StartsWith("DV1") &&
                                !ftAnte.ause.StartsWith("TR"))
                            {
                                ValErrs.AddMess("Invalid ause (%s) for RX or DV1 Antenna %d",
                                                            keyLine, "E", ftAnte.ause, ftChan.antnumbrx2.ToString());
                                errorCount++;
                            }

                            if (nArrayFW[FtChan.AFSLRX2] != Constant.DB_NULL &&
                                    ftChan.afslrx2 <= 0 &&
                                    ftChan.call1[0] != '%')
                            {
                                ValErrs.AddMess("Ant FSL for RX or DV1 antenna is 0.", keyLine, "W");
                                warningCount++;
                            }
                        }
                        DynAntenna.FtCloseAntenna(ID1);
                    }
                }


                if (nArrayFW[FtChan.ANTNUMBRX3] != Constant.DB_NULL)
                {
                    /* verify RX diversity 2 antenna  */

                    ValImport.FtFormWhereClause(out whereClause, ftChan.call1, ftChan.call2,
                                      ftChan.bndcde, ftChan.antnumbrx3, "");

                    if ((ID1 = DynAntenna.FtSelectAntenna(tableName, whereClause, "")) < 0)
                    {
                        ValErrs.AddMess("Could not read antenna information, reason: %d.",
                                        keyLine, "W", ID1.ToString());
                        warningCount++;
                    }
                    else
                    {
                        if (DynAntenna.FtFetchAntenna(ID1, out ftAnte, out nArrayTemp) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("No RX or DV2 Antenna for this channel", keyLine, "E");
                            errorCount++;
                        }
                        else
                        { /* antenna record was found */
                            if (!ftAnte.ause.StartsWith("DV2") &&
                                !ftAnte.ause.StartsWith("TR"))
                            {
                                ValErrs.AddMess("Invalid ause (%s) for RX or DV2 Antenna %d.",
                                                keyLine, "E", ftAnte.ause, ftChan.antnumbrx3.ToString());
                                errorCount++;
                            }

                            if (nArrayFW[FtChan.AFSLRX3] != Constant.DB_NULL &&
                                    ftChan.afslrx3 <= 0 &&
                                    ftChan.call1[0] != '%')
                            {
                                ValErrs.AddMess("Ant FSL RX DV2 is 0\r\n", keyLine, "W");
                                warningCount++;
                            }
                        }
                        DynAntenna.FtCloseAntenna(ID1);
                    }
                }


                if (nArrayFW[FtChan.NOTERX] != Constant.DB_NULL)
                {
                    SuNote tNote;

                    nRet = Suutils.SuGetNote(ftSite.oper, ftChan.noterx, out tNote);
                    if (nRet != 0)
                    {
                        ValErrs.AddMess("NoteRx (%s) not present in SDB for Operator :%s:",
                                 keyLine, "E", ftChan.noterx, ftSite.oper);
                        errorCount++;
                    }
                }

                if (nArrayFW[FtChan.EQPTRX] != Constant.DB_NULL)
                {
                    if (!ftChan.eqptrx.StartsWith("$"))
                    {
                        SuEqpt tEqpt;
                        int nRet_ = Suutils.SuGetEqpt(ftChan.eqptrx, out tEqpt);
                        if (nRet_ != 0)
                        {
                            ValErrs.AddMess("RX Equipment (%s) not present in SDB.",
                                     keyLine, "E", ftChan.eqptrx);
                            errorCount++;
                        }
                    }
                    else
                    {
                        valLevel = Constant.TSIP;
                    }
                }
                else
                {
                    ValErrs.AddMess("RX Equipment not present.\r\n", keyLine, "E");
                    // The following enters a blank line after the previous AddMess();
                    ValErrs.AddMess("", keyLine, "E");
                    errorCount++;
                }

                if (nArrayFW[FtChan.FEERX] != Constant.DB_NULL)
                {
                    SuFeeCode tFee;
                    recExist = (short)Suutils.SuGetFeeCode(ftChan.feerx, out tFee);
                    if (recExist != 0)
                    {
                        ValErrs.AddMess("FeeRX (%s) must be 1-9 or A-T,V,X.",
                                        keyLine, "E", ftChan.feerx);
                        errorCount++;
                    }
                }


                /* Validate RX traffic / equipment */

                if (nArrayFW[FtChan.TRAFRX] != Constant.DB_NULL)
                {
                    if ((ftChan.trafrx[0] != 'A') &&
                        (ftChan.trafrx[0] != '$'))
                    {
                        SuTraf tTraf;
                        nRet = Suutils.SuGetTraf(ftChan.trafrx, ftChan.eqptrx, out tTraf);
                        if (nRet != 0)
                        {
                            ValErrs.AddMess("RX Traffic code (%s) & Equipment (%s) pair not present in the SDB.",
                                            keyLine, "W", ftChan.trafrx, ftChan.eqptrx);
                            warningCount++;
                        }

                    }
                    else if (ftChan.trafrx[0] == '$')
                    {
                        /* Temporary codes being used, only valid for TSIP */
                        valLevel = Constant.TSIP;
                    }
                    else
                    {
                        SuTraf tTraf;
                        nRet = Suutils.SuGetTraf(ftChan.trafrx, null, out tTraf);
                        if (nRet != 0)
                        {
                            ValErrs.AddMess("RX Traffic Code (%s) not present in the SDB.",
                                     keyLine, "E", ftChan.trafrx);
                            errorCount++;
                        }
                    }
                }
                else
                {
                    ValErrs.AddMess("RX Traffic not present.", keyLine, "E");
                    errorCount++;
                }
            } /* end of rx side */


            if ((nArrayFW[FtChan.NOTEGNL] != Constant.DB_NULL) && (siteFound == Constant.SUCCESS))
            {
                SuNote tNote;
                nRet = Suutils.SuGetNote(ftSite.oper, ftChan.notegnl, out tNote);
                if (nRet != 0)
                {
                    ValErrs.AddMess("NoteGnl (%s) not present in SDB for Operator :%s:",
                             keyLine, "E", ftChan.notegnl, ftSite.oper);
                    errorCount++;
                }
            }


            /* validate bndcde against the SDB */
            nRet = Suutils.SuGetBand(ftChan.bndcde, out pBand);
            if (nRet != 0)
            {
                ValErrs.AddMess("Band Code (%s) not present in SDB.",
                                keyLine, "E", ftChan.bndcde);
                pBand = null;
                errorCount++;
            }

            if (nArrayFW[FtChan.ROUTNUMB] != Constant.DB_NULL)
            {
                SuRout tRoute;
                nRet = Suutils.SuGetRout(ftSite.oper, ftChan.routnumb, out tRoute);
                if (nRet != 0)
                {
                    /* route not defined for operator */
                    ValErrs.AddMess("Route %s not defined for operator %s.",
                             keyLine, "E", ftChan.routnumb, ftSite.oper);
                    errorCount++;
                }
            }

            //...Log2.v("\r\nValChan.FtValChanFieldsTS(): A");

            SuPlan tPlan;
            SuPlnd[] pPlnd = null;
            int nNum = 0;
            int nInd = -666;

            /* validate plan code */
            if (nArrayFW[FtChan.SPLAN] != Constant.DB_NULL)
            {
                if (ftChan.splan[0] == '$')
                {
                    /* Temporary codes in use - valid only for TSIP */
                    valLevel = Constant.TSIP;
                }
                else
                {
                    nRet = Suutils.SuGetPlan(ftChan.splan, ftChan.bndcde, out tPlan, true, out pPlnd, out nNum);
                    //...Log2.v("\r\nValChan.FtValChanFieldsTS(): B: nNum = " + nNum);
                    if (nRet != 0)
                    {
                        ValErrs.AddMess("Plan (%s) not present in the SDB.", keyLine, "E", ftChan.splan);
                        errorCount++;
                    }
                }
            }

            /* verify for a valid frequency according to the HL - VH - PLAN */
            /* combination, using the bandcode and the splan table          */

            if ((nArrayFW[FtChan.FREQTX] != Constant.DB_NULL) &&
                (nArrayFW[FtChan.SPLAN] != Constant.DB_NULL) &&
                    (ftChan.splan[0] != '$') &&     /* Temp code in use */
                    (nArrayFW[FtChan.VH] != Constant.DB_NULL) &&
                    (nArrayFW[FtChan.HL] != Constant.DB_NULL))
            {
                /* freqtx - plan - hl - vh all there */

                //...Log2.v("\r\nValChan.FtValChanFieldsTS(): switch(): ftChan.hl = " + ftChan.hl);
                switch (ftChan.hl)
                {
                    case 1:
                        nInd = Suutils.SuPlndFindFreq(pPlnd, nNum, 1, ftChan.freqtx);
                        break;

                    case 2:
                        nInd = Suutils.SuPlndFindFreq(pPlnd, nNum, 2, ftChan.freqtx);
                        break;

                    case 3:
                        nInd = Suutils.SuPlndFindFreq(pPlnd, nNum, 3, ftChan.freqtx);
                        break;

                    case 4:
                        nInd = Suutils.SuPlndFindFreq(pPlnd, nNum, 4, ftChan.freqtx);
                        break;

                    case 5:
                        nInd = Suutils.SuPlndFindFreq(pPlnd, nNum, 1, ftChan.freqtx);
                        if (nInd < 0)
                        {
                            nInd = Suutils.SuPlndFindFreq(pPlnd, nNum, 3, ftChan.freqtx);
                        }
                        break;

                    case 6:
                        nInd = Suutils.SuPlndFindFreq(pPlnd, nNum, 2, ftChan.freqtx);
                        if (nInd < 0)
                        {
                            nInd = Suutils.SuPlndFindFreq(pPlnd, nNum, 4, ftChan.freqtx);
                        }
                        break;
                } /* end of switch */

                if (nInd < 0)
                {
                    //...Log2.v("\r\nValChan.FtValChanFieldsTS(): Frequency (%s) does not agree with the Plan, hiLo, VH combination.");
                    ValErrs.AddMess("Frequency (%s) does not agree with the Plan, hiLo, VH combination.",
                                    keyLine, "E", ftChan.freqtx.ToString("#.00"));
                    errorCount++;
                }
                else    /* Can't verify polarization if no Plan data */
                {
                    /* Verify the polarization */
                    ValPol(ftChan.freqtx, ftChan.hl, ftChan.vh, pPlnd[nInd], out calPol);
                    if (ftChan.poltx[0] != calPol[0])
                    {
                        /* Invalid TX polarization */
                        ValErrs.AddMess("TX Polarization :%s: does not agree with the HiLo, VH Code combination.\r\n", keyLine, "E", ftChan.poltx);
                        errorCount++;
                    }
                }

            }
            else if (nArrayFW[FtChan.FREQTX] != Constant.DB_NULL)
            {
                /* valid against band range in sd_band */

                if (pBand != null)
                {
                    string str;
                    if (!IsInBand(pBand, ftChan.freqtx, out str))
                    {
                        ValErrs.AddMess(str, keyLine, "E");
                        errorCount++;
                    }
                }
            }

            //...Log2.v("\n\nValChan.FtValChanFieldsTS(): Exit");

        }   /* ----- End of ftValChanFieldsTS ----- */

        /// <summary>
        /// Get valid polarization given a channel's frequency, hilo, vh code and the
        /// PLAN detailed (sd_plnd) record corresponding to the frequency.
        /// </summary>
        /// <remarks>
        /// If the hilo is in the range of 1-4 inclusive then there is no
        /// ambiguity about which SET the frequency belongs to, to determine
        /// the polarity simply call the routine 'ftGetPol'.
        /// 
        /// If the hilo code is 5 then the frequency can belong to either
        /// SET 1 or SET 3. If the hilo code is 6 then the frequency can
        /// belong to either SET 2 or SET 4. To resolve this ambiguity we
        /// must compare the channel's frequency to the individual's SET
        /// in the 	sdPlnd row (there are 4 sets in the row), when we get
        /// a match we can then determine to which SET the channel's freq.
        /// belongs.
        /// 
        /// To really understand what is happening you MUST refer to:
        /// 
        /// 	    page B-72 of the TSIP REFERENCE MANUAL Series 1 Part C
        /// 
        /// while looking at this page you must also look at the FCSA BAND
        /// PLAN  report.
        /// </remarks>
        /// <param name="freqtx"> - channels' frequency.</param>
        /// <param name="hl"> - HiLo code.</param>
        /// <param name="vh"> - VH code.</param>
        /// <param name="sdPlnd"> - frequency's row in sd_plnd table.</param>
        /// <param name="pol"> - calculated polarization for freq.</param>
        /// <returns>The correct polarization for the channel.</returns>
        public static int ValPol(double freqtx,                 /* Channel's frequency */
                                    short hl,                   /* Hi lo code */
                                    short vh,                   /* VH code */
                                    SuPlnd sdPlnd,              /* frequency's row in sd_plnd table */
                                    out string pol)             /* calculated polarization for freq */
        {

            if (hl <= 4)
            {
                /* only one SET of freq. used from plan */
                FtUtil.FtGetPol(vh, hl, out pol);
                return 0;
            }

            /* Can't identify the SET from the HiLo code. Must use
             * the frequency to identify the SET.
             */
            if (hl == 5)
            {
                /* must be set 1 or set 3 */
                if (freqtx == sdPlnd.set1)
                {
                    /* IT IS SET 1 */
                    FtUtil.FtGetPol(vh, 1, out pol);
                }
                else
                {
                    /* Must be SET 3 */
                    FtUtil.FtGetPol(vh, 3, out pol);
                }
            }
            else
            {
                /* Hl must = 6 */
                /* Frequency must be in SET 2 or SET 4 */
                if (freqtx == sdPlnd.set2)
                {
                    /* It is SET 2 */
                    FtUtil.FtGetPol(vh, 2, out pol);
                }
                else
                {
                    FtUtil.FtGetPol(vh, 4, out pol);
                }
            }

            return 0;
        }	/* ----- End of valPol ----- */

        /// <summary>
        /// This method returns true if the prescribed frequency (in KHz) lies inside the [blo, bhi] frequency interval of the 
        /// prescribed SuBand object; otherwise false.
        /// </summary>
        /// <param name="suBand"> - a prescribed SuBand object.</param>
        /// <param name="frequency"> - a prescribed frequency to test.</param>
        /// <param name="message"> - an output message that describes any error conditions encountered.</param>
        /// <returns>true or false.</returns>
        public static bool IsInBand(SuBand suBand, double frequency, out string message)
        {
            // 'out' requirement.
            message = "";

            bool isInBand = IsInRange(frequency, suBand.blo, suBand.bhi);

            if (!isInBand)
            {
                message = String.Format("Frequency TX ({0:F2}) Out of Range :{1:F2}: to :{2:F2}:", frequency, suBand.blo, suBand.bhi);
            }
            // If bndcde is 7B check for the internal prohibited zone.
            else if (suBand.bndcde.Equals("7B"))
            {
                // Determine if frequency is in the internal prohibited zone.
                bool isInProhibitedZone = IsInRange(frequency, Constant.SUB_PLAN_I_LOWER_INTERVAL_FREQ_HI, Constant.SUB_PLAN_I_UPPER_INTERVAL_FREQ_LO);

                if (isInProhibitedZone)
                {
                    isInBand = false;
                    message = String.Format("Frequency TX ({0:F2}) is in prohibited range :{1:F2}: to :{2:F2}: - see ISED/SRSP-307.1", frequency, Constant.SUB_PLAN_I_LOWER_INTERVAL_FREQ_HI, Constant.SUB_PLAN_I_UPPER_INTERVAL_FREQ_LO);
                }
            }

            return isInBand;
        }

        /// <summary>
        /// This method returns true if the prescribed test value lies within the closed interval
        /// [lower, upper] on the real line.
        /// </summary>
        /// <param name="testValue"> a prescribed value to test for inclusion.</param>
        /// <param name="lower"> - the prescribed lower bound of the real interval.</param>
        /// <param name="upper"> - the prescribed upper bound of the real interval.</param>
        /// <returns></returns>
        private static bool IsInRange(double testValue, double lower, double upper)
        {
            bool result = false;

            if ((testValue >= lower) && (testValue <= upper))
            {
                result = true;
            }

            return result;
        }

    }
}

```
