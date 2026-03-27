using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtImport
{
    using _NewLib;
    using SQLLEN = Int64;
    using static _NewLib.Enums.FtImpQual;
    using _Utillib;
    using _Configuration;

    /// <summary>
    /// This class comprises private fields that monitor the current 'state'
    /// of the FtImport parsing and validation activities; it also provides methods
    /// that test whether complete Ante and/or Chan records have been parsed
    /// that can now be accumulated with the current Site; it also provides a method
    /// that detects when a complete set of associated Site/Ante/Chan records is ready
    /// to be written to _site, _ante and _chan tables in the DB and manages the insertions.
    /// </summary>
    class State
    {
        private static bool mTTflag = false;        /* Found title rec. */
        private static bool mGKflag = false;        /* Found chng.loc. rec. */
        private static bool mSKflag = false;        /* Found site key rec. */
        private static bool mSDflag = false;        /* Found site detail rec. */
        private static bool mAKflag = false;        /* Found ante key rec. */
        private static bool mAQflag = false;        /* Found ante req'd rec. */
        private static bool mAOflag = false;        /* Found ante opt. rec. */
        private static bool mCKflag = false;        /* Found chan key rec. */
        private static bool mCTflag = false;        /* Found chan xmit. rec. */
        private static bool mCRflag = false;        /* Found chan xcve rec. */
        private static bool mCQflag = false;        /* Found chan req rec. */
        private static bool mCOflag = false;        /* Found chan opt rec. */

        private static string mTableName_site;
        private static string mTableName_ante;
        private static string mTableName_chan;

        private static int mInsertHandle_site;
        private static int mInsertHandle_ante;
        private static int mInsertHandle_chan;

        private static int mSiteCount = 0;          /* Site count. */

        private static FtSite mFtSite = new FtSite();
        private static SQLLEN[] mFtSiteNullInds = NullHelper.CreateArrayOfNullInd(FtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

        private static FtAnte mFtAnte = new FtAnte();
        private static SQLLEN[] mFtAnteNullInds = NullHelper.CreateArrayOfNullInd(FtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

        private static FtChan mFtChan = new FtChan();
        private static SQLLEN[] mFtChanNullInds = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

        private static List<FtAnte> mFtAnteListAll = new List<FtAnte>();
        private static List<FtAnte> mFtAnteListThisSite = new List<FtAnte>();
        private static List<SQLLEN[]> mFtAnteNullIndsListThisSite = new List<SQLLEN[]>();

        private static List<FtChan> mFtChanListAll = new List<FtChan>();
        private static List<FtChan> mFtChanListThisSite = new List<FtChan>();
        private static List<SQLLEN[]> mFtChanNullIndsListThisSite = new List<SQLLEN[]>();

        private static List<QualLine> mQualLineList = new List<QualLine>();

        /// <summary>
        /// This method receives the current qualified line that is being parsed
        /// and accumulates it into a list of such qualified lines.
        /// </summary>
        /// <param name="qualLine"> - current qualified line.</param>
        public static void PushQualLine(QualLine qualLine)
        {
            mQualLineList.Add(qualLine);
        }

        /// <summary>
        /// This manages the insertion of accumulated Site, Ante and Chan
        /// records into DB _site, _ante and _chan tables.
        /// </summary>
        /// <param name="importOK"> - unchanged on exit if no error occurs.</param>
        private static void WriteSiteAnteChanToDB(ref short importOK)
        {
            int rc;
            string whereClause;

            // Insert the current FtSite object into the DB _site table.
            // Use the previously opened DynFtSite insert cursor handle.
            // This could be a duplicate FtSite insert so we first delete
            // any row that has the same call1.
            whereClause = String.Format(" call1 = '{0}'", mFtSite.call1);
            Ssutil.DbDeleteRows(mTableName_site, whereClause);

            if ((rc = DynSite.FtInsertSite(mInsertHandle_site, mFtSite, mFtSiteNullInds))
                  != Constant.SUCCESS)
            {
                Log2.e("\nState.WriteSiteAnteChanToDB(): ERROR: DB _site table insert attempt FAILED for call1 = " + mFtSite.call1);
                //ErrMsg.UtPrintMessage(rc);
                //importOK = Constant.FAILURE;
            }

            // Insert the current list of FtAnte objects into the DB _ante table.
            // Use the previously opened DynFtAnte insert cursor handle.
            // This could be a duplicate FtAnte insert so we first delete
            // any row that has the same key set: call1, call2, bndcde and anum.
            for (int index = 0; index < mFtAnteListThisSite.Count; index++)
            {
                FtAnte ftAnte = mFtAnteListThisSite[index];
                SQLLEN[] ftAnteNullInds = mFtAnteNullIndsListThisSite[index];

                whereClause = String.Format(" call1='{0}' AND call2='{1}' AND bndcde='{2}' AND anum='{3}'", ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum);
                Ssutil.DbDeleteRows(mTableName_ante, whereClause);

                if ((rc = DynAntenna.FtInsertAntenna(mInsertHandle_ante, ftAnte, ftAnteNullInds))
                      != Constant.SUCCESS)
                {
                    Log2.e("\nState.WriteSiteAnteChanToDB(): ERROR: DB _ante table insert attempt FAILED for ftAnte:  " + ftAnte.KeysToString());
                    //ErrMsg.UtPrintMessage(rc);
                    //importOK = Constant.FAILURE;
                }
            }

            // Insert the current list of FtChan objects into the DB _chan table.
            // Use the previously opened DynFtChan insert cursor handle.
            // This could be a duplicate FtChan insert so we first delete
            // any row that has the same key set: call1, call2, bndcde and chid.
            for (int index = 0; index < mFtChanListThisSite.Count; index++)
            {
                FtChan ftChan = mFtChanListThisSite[index];
                SQLLEN[] ftChanNullInds = mFtChanNullIndsListThisSite[index];

                whereClause = String.Format(" call1='{0}' AND call2='{1}' AND bndcde='{2}' AND chid='{3}'", ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid);
                Ssutil.DbDeleteRows(mTableName_chan, whereClause);

                if ((rc = DynChannel.FtInsertChannel(mInsertHandle_chan, ftChan, ftChanNullInds))
                      != Constant.SUCCESS)
                {
                    Log2.e("\nState.WriteSiteChanChanToDB(): ERROR: DB _chan table insert attempt FAILED for ftChan:  " + ftChan.KeysToString());
                    //ErrMsg.UtPrintMessage(rc);
                    //importOK = Constant.FAILURE;
                }

            }

        }

        /// <summary>
        /// This method checks the readiness of an accumulated set of Site-Ante-Chan records
        /// to be inserted into the DB by 'peeking' at the next qualified line in the PDF import text file; 
        /// if the data is ready it calls a worker method that performs the insertions into DB tables and
        /// then prepares to accumulate the next batch of Site-Ante-Chan records.
        /// </summary>
        /// <param name="nextQualLine"> - next qualified line to be 'peeked' at for type.</param>
        /// <param name="importOK"> - unchanged on exit if no errors occur.</param>
        public static void WriteRecordsToDBifReady(QualLine nextQualLine, ref short importOK)
        {
            if (mSKflag && mSDflag)  // Check that current FtSite is complete.
            {
                switch (nextQualLine.Qualifier)    // Check that the next qualified line 
                {                                  // triggers instantiation of a new FtSite.
                    case SK:
                    case SD:
                    case EOF:
                        //...Log2.v("\nState.WriteToDBifReady(): FtSite:\n" + mFtSite.ToStringWN(mFtSiteNullInds));
                        for (int index = 0; index < mFtAnteListThisSite.Count; index++)
                        {
                            FtAnte ftAnte = mFtAnteListThisSite[index];
                            SQLLEN[] nullInds = mFtAnteNullIndsListThisSite[index];
                            //...Log2.v("\nState.WriteToDBifReady(): FtAnte:\n" + ftAnte.ToStringWN(nullInds));
                        }
                        for (int index = 0; index < mFtChanListThisSite.Count; index++)
                        {
                            FtChan ftChan = mFtChanListThisSite[index];
                            SQLLEN[] nullInds = mFtChanNullIndsListThisSite[index];
                            //...Log2.v("\nState.WriteToDBifReady(): FtChan:\n" + ftChan.ToStringWN(nullInds));
                        }

                        // Only insert records into DB tables if no PDF parsing
                        // errors have been detected so far.
                        if (importOK == Constant.SUCCESS)
                        {
                            WriteSiteAnteChanToDB(ref importOK);
                        }

                        PrepareNewSite();

                        // Check whether the next qualLine is a duplicate FtSite record.
                        // 'Duplicate' in the sense that the DB _site table already has a
                        // row that has the same 'call1'.
                        if (nextQualLine.Qualifier == SK)
                        {
                            if (DynSite.FtSiteExistsInTable(mTableName_site, nextQualLine.Fields[3]))
                            {
                                // We have a duplicate
                                // To mimic the anomolous behaviour of the legacy C/C++ code
                                // the line number is artificially increased by 1.
                                string msgBuf;
                                msgBuf = String.Format("Error - line #{0}, duplicate Site record\r\n", nextQualLine.LineNum + 1);
                                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                                importOK = Constant.FAILURE;
                            }
                        }
                        break;
                    default:
                        // Nothing to do.
                        break;

                } // switch
            } // mSKflag && mSDflag
        }

        /// <summary>
        /// This method checks whether SK and SD records are the final lines in the 
        /// PDF import text file and whether they are 'complete' (i.e. both an SK and SD are present).
        /// </summary>
        /// <param name="nextQualLine"></param>
        /// <param name="importOK"></param>
        public static void CheckStateIfJustBeforeEOF(QualLine nextQualLine, ref short importOK)
        {
            if (nextQualLine.Qualifier != Enums.FtImpQual.EOF) return;

            if (!mSKflag && mSDflag)
            {
                Console.Write("Error - Site missing key record.\r\nCheck that there is a SK and SD record for each site.");
                importOK = Constant.FAILURE;
                return;
            }
            else if (mSKflag && !mSDflag)
            {
                Console.Write("Error - Site missing detail record.\r\nCheck that there is a SK and SD record for each site.");
                importOK = Constant.FAILURE;
                return;
            }

        }

        /// <summary>
        /// This method peeks at the next qualified line in the PDF import text file
        /// to determine whether a previously parsed set of Ante records is 'complete' and can be
        /// accumulated with the current Site.
        /// </summary>
        /// <param name="nextQualLine"> - the next qualified line in the parsing sequence.</param>
        /// <param name="importOK"> - unchanged on exit if no error occurs.</param>
        public static void AddAnteIfReady(QualLine nextQualLine, ref short importOK)
        {
            if (mAKflag && mAQflag && mAOflag)
            {
                // Ante is ready.
            }
            else if (mAKflag && mAQflag && (nextQualLine.Qualifier == Enums.FtImpQual.AO))
            {
                // Wait until the (optional) AO record is parsed.
                return;
            }
            else if (mAKflag && mAQflag && (nextQualLine.Qualifier != Enums.FtImpQual.AO))
            {
                // The (optional) AO record is absent.
                // This is equivalent to parsing an AO record with all fields empty.
                // Set the AO flag to trigger 'antenna ready'.
                mAOflag = true;
            }
            else
            {
                // Ante is not ready.
                return;
            }

            if (mAKflag && mAQflag && mAOflag)  // Check that current FtAnte is complete.
            {
                switch (nextQualLine.Qualifier)    // Check that the next qualified line 
                {                                  // triggers instantiation of a new FtAnte.
                    case SK:
                    case SD:
                    case AK:
                    case AQ:
                    case AO:
                    case CK:
                    case CT:
                    case CR:
                    case CQ:
                    case CO:
                    case EOF:
                        //...Log2.v("\nState.AddAnteIfReady(): FtAnte:\n" + mFtAnte.ToStringWN(mFtAnteNullInds));

                        // Check if the current mFtAnte has a duplicate key to a previously
                        // parsed Ante. If so, set importOK so that it will not be added to
                        // the Site's Ante accumulation list.
                        // 
                        if (IsDuplicateAnte(mFtAnte))
                        {
                            int lastAKlineNum = GetLastAKLineNum();
                            string msgBuf;
                            msgBuf = String.Format("Error - line #{0}, duplicate Antenna record\r\n", lastAKlineNum);
                            ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                            importOK = Constant.FAILURE;
                        }

                        // Only accumulate this Ante to the Site's Ante list if
                        // importOK is good.
                        if (importOK == Constant.SUCCESS)
                        {
                            mFtAnteListAll.Add(mFtAnte);
                            mFtAnteListThisSite.Add(mFtAnte);
                            mFtAnteNullIndsListThisSite.Add(mFtAnteNullInds);
                        }

                        PrepareNewAnte();

                        break;
                    default:
                        // Nothing to do.
                        break;
                }

            }
        }

        /// <summary>
        /// This method returns the line number of the most recently parsed
        /// AK record in the PDF import text file.
        /// </summary>
        /// <returns></returns>
        private static int GetLastAKLineNum()
        {
            int result = 0;
            int maxIndex = mQualLineList.Count - 1;

            for (int i = maxIndex; i >= 0; i--)
            {
                if (mQualLineList[i].Qualifier == Enums.FtImpQual.AK)
                {
                    result = mQualLineList[i].LineNum;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// This method returns the line number of the most recently parsed
        /// CK record in the PDF import text file.
        /// </summary>
        /// <returns></returns>
        private static int GetLastCKLineNum()
        {
            int result = 0;
            int maxIndex = mQualLineList.Count - 1;

            for (int i = maxIndex; i >= 0; i--)
            {
                if (mQualLineList[i].Qualifier == Enums.FtImpQual.CK)
                {
                    result = mQualLineList[i].LineNum;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// This method peeks at the next qualified line in the PDF import text file
        /// to determine whether a previously parsed set of Chan records is 'complete' and can be
        /// accumulated with the current Site.
        /// </summary>
        /// <param name="nextQualLine"> - the next qualified line in the parsing sequence.</param>
        /// <param name="importOK"> - unchanged on exit if no error occurs.</param>
        public static void AddChanIfReady(QualLine nextQualLine, ref short importOK)
        {
            if (mCKflag && mCTflag && mCRflag && mCQflag && mCOflag)
            {
                // Chan is ready.
            }
            else if (mCKflag && mCTflag && mCRflag && mCQflag && (nextQualLine.Qualifier == Enums.FtImpQual.CO))
            {
                // Wait until the (optional) CO record is parsed.
                return;
            }
            else if (mCKflag && mCTflag && mCRflag && mCQflag && (nextQualLine.Qualifier != Enums.FtImpQual.CO))
            {
                // The (optional) CO record is absent.
                // This is equivalent to parsing an CO record with all fields empty.
                // Set the CO flag to trigger 'antenna ready'.
                mCOflag = true;
            }
            else
            {
                // Ante is not ready.
                return;
            }

            switch (nextQualLine.Qualifier)    // Check that the next qualified line 
            {                                  // triggers instantiation of a new FtChan.
                case SK:
                case SD:
                case AK:
                case AQ:
                case AO:
                case CK:
                case CT:
                case CR:
                case CQ:
                case CO:
                case EOF:
                    if (mCKflag && mCTflag && mCRflag && mCQflag && mCOflag)  // Check that current FtChan is complete.
                    {
                        //...Log2.v("\nState.AddChanIfReady(): FtChan:\n" + mFtChan.ToStringWN(mFtChanNullInds));

                        // Check if the current mFtChan has a duplicate key to a previously
                        // parsed Chan. If so, set importOK so that it will not be added to
                        // the Site's Chan accumulation list.
                        // 
                        if (IsDuplicateChan(mFtChan))
                        {
                            int lastAKlineNum = GetLastCKLineNum();
                            string msgBuf;
                            msgBuf = String.Format("Error - line #{0}, duplicate Channel record\r\n", lastAKlineNum);
                            ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                            importOK = Constant.FAILURE;
                        }

                        // Only accumulate this Chan to the Site's Chan list if
                        // importOK is good.
                        if (importOK == Constant.SUCCESS)
                        {
                            mFtChanListAll.Add(mFtChan);
                            mFtChanListThisSite.Add(mFtChan);
                            mFtChanNullIndsListThisSite.Add(mFtChanNullInds);
                        }

                        PrepareNewChan();
                    }
                    break;
                default:
                    // Nothing to do.
                    break;
            }
        }

        /// <summary>
        /// This method determines whether a prescribed FtAnte object
        /// has the same key fields (chan1, chan2, bndcde, anum) as a 
        /// previously parsed and accumulated Ante record group (AK, AQ, and possibly AO)
        /// </summary>
        /// <param name="ftAnte"> - prescribed FtAnte object to be checked for duplication.</param>
        /// <returns></returns>
        private static bool IsDuplicateAnte(FtAnte ftAnte)
        {
            bool result = false;

            foreach (FtAnte mFtAnte in mFtAnteListAll)
            {
                if (mFtAnte.KeysToString().Equals(ftAnte.KeysToString()))
                {
                    // We have found a duplicate Ante.
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// This method determines whether a prescribed FtChan object
        /// has the same key fields (chan1, chan2, bndcde, chid) as a 
        /// previously parsed and accumulated Chan record group (CK, CT, CR, CQ, and possibly CO)
        /// </summary>
        /// <param name="ftChan"> - prescribed FtChan object to be checked for duplication.</param>
        /// <returns></returns> 
        private static bool IsDuplicateChan(FtChan ftChan)
        {
            bool result = false;

            foreach (FtChan mFtChan in mFtChanListAll)
            {
                if (mFtChan.KeysToString().Equals(ftChan.KeysToString()))
                {
                    // We have found a duplicate Chan.
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// This method prepares for the collection of the next complete set of Ante records by resetting
        /// the associated internal flags and instantiating new FtAnte and SQLLEN[] collector objects.
        /// </summary>
        private static void PrepareNewAnte()
        {
            // Clear the Ante flags.
            mAKflag = false;
            mAQflag = false;
            mAOflag = false;

            // Instantiate a new FtAnte object and nullInds.
            mFtAnte = new FtAnte();
            mFtAnteNullInds = NullHelper.CreateArrayOfNullInd(FtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
        }

        /// <summary>
        /// This method prepares for the collection of the next complete set of Chan records by resetting
        /// the associated internal flags and instantiating new FtChan and SQLLEN[] collector objects.
        /// </summary>
        private static void PrepareNewChan()
        {
            // Clear the Chan flags.
            mCKflag = false;
            mCTflag = false;
            mCRflag = false;
            mCQflag = false;
            mCOflag = false;

            // Instantiate a new FtChan object and nullInds.
            mFtChan = new FtChan();
            mFtChanNullInds = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
        }

        /// <summary>
        /// This method prepares for the collection of the next complete set of Site records by resetting
        /// the associated internal flags and instantiating new FtSite and SQLLEN[] collector objects; the method also calls
        /// the worker methods PrepareNewAnte() and PrepareNewChan().
        /// </summary>
        private static void PrepareNewSite()
        {
            // Clear the Site flags.
            mSKflag = false;
            mSDflag = false;

            // Instantiate the new FtSite object and nullInds.
            mFtSite = new FtSite();
            mFtSiteNullInds = NullHelper.CreateArrayOfNullInd(FtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            if (AnteIsComplete())
            {
                // Instantiate new FtAnte lists.
                mFtAnteListThisSite = new List<FtAnte>();
                mFtAnteNullIndsListThisSite = new List<SQLLEN[]>();

                // Prepare a new FtAnte object.
                // This method call also clear the Ante flags.
                PrepareNewAnte();
            }

            if (ChanIsComplete())
            {
                // Instantiate new FtChan lists.
                mFtChanListThisSite = new List<FtChan>();
                mFtChanNullIndsListThisSite = new List<SQLLEN[]>();

                // Prepare new FtChan object.
                // This method call also clear the Chan flags.
                PrepareNewChan();
            }

        }

        /// <summary>
        /// This method determines whether the currently accumulating set of Ante records is 'complete', i.e. comprises 
        /// an AK, AQ and possibly an AO record.
        /// </summary>
        /// <returns></returns>
        private static bool AnteIsComplete()
        {
            return (mAKflag && mAQflag) || (!mAKflag && !mAQflag);
        }

        /// <summary>
        /// This method determines whether the currently accumulating set of Chan records is 'complete', i.e. comprises 
        /// an CK, CT, CR, CQ and possibly an CO record.
        /// </summary>
        /// <returns></returns>
        private static bool ChanIsComplete()
        {
            return (mCKflag && mCTflag && mCRflag && mCTflag) || (!mCKflag && !mCTflag && !mCRflag && !mCTflag);
        }

        public static FtSite Site
        {
            get { return mFtSite; }
            set { mFtSite = value; }
        }

        public static FtAnte Ante
        {
            get { return mFtAnte; }
            set { mFtAnte = value; }
        }

        public static FtChan Chan
        {
            get { return mFtChan; }
            set { mFtChan = value; }
        }

        public static SQLLEN[] SiteNullInds
        {
            get { return mFtSiteNullInds; }
            set { mFtSiteNullInds = value; }
        }

        public static SQLLEN[] AnteNullInds
        {
            get { return mFtAnteNullInds; }
            set { mFtAnteNullInds = value; }
        }

        public static SQLLEN[] ChanNullInds
        {
            get { return mFtChanNullInds; }
            set { mFtChanNullInds = value; }
        }

        public static int SiteCount
        {
            get { return mSiteCount; }
            set { mSiteCount = value; }
        }

        public static bool TTflagSet
        {
            get { return mTTflag; }
            set { mTTflag = value; }
        }

        public static bool GKflagSet
        {
            get { return mGKflag; }
            set { mGKflag = value; }
        }

        public static bool SKflagSet
        {
            get { return mSKflag; }
            set { mSKflag = value; }
        }

        public static bool SDflagSet
        {
            get { return mSDflag; }
            set { mSDflag = value; }
        }

        public static bool AKflagSet
        {
            get { return mAKflag; }
            set { mAKflag = value; }
        }

        public static bool AQflagSet
        {
            get { return mAQflag; }
            set { mAQflag = value; }
        }

        public static bool AOflagSet
        {
            get { return mAOflag; }
            set { mAOflag = value; }
        }

        public static bool CKflagSet
        {
            get { return mCKflag; }
            set { mCKflag = value; }
        }

        public static bool CTflagSet
        {
            get { return mCTflag; }
            set { mCTflag = value; }
        }

        public static bool CRflagSet
        {
            get { return mCRflag; }
            set { mCRflag = value; }
        }

        public static bool CQflagSet
        {
            get { return mCQflag; }
            set { mCQflag = value; }
        }

        public static bool COflagSet
        {
            get { return mCOflag; }
            set { mCOflag = value; }
        }

        public static string SiteTableName
        {
            get { return mTableName_site; }
            set { mTableName_site = value; }
        }

        public static string AnteTableName
        {
            get { return mTableName_ante; }
            set { mTableName_ante = value; }
        }

        public static string ChanTableName
        {
            get { return mTableName_chan; }
            set { mTableName_chan = value; }
        }

        public static int SiteInsertHandle
        {
            get { return mInsertHandle_site; }
            set { mInsertHandle_site = value; }
        }

        public static int AnteInsertHandle
        {
            get { return mInsertHandle_ante; }
            set { mInsertHandle_ante = value; }
        }

        public static int ChanInsertHandle
        {
            get { return mInsertHandle_chan; }
            set { mInsertHandle_chan = value; }
        }

    }
}
