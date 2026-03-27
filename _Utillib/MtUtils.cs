using _DataStructures;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using _Configuration;
    using _NewLib;
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
    /// Provides general purpose utility methods for accessing the master database.
    /// </summary>
    public class MtUtils
    {
#if PINVOKE_1
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int mtGetSiteWN([In] string cCall, [In, Out] ref IntPtr pSite, int nDepth, [In, Out] ref IntPtr pSiteNull);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int ftGetSite(StringBuilder sb1, [In, Out] ref IntPtr intPtr, [In] int i, StringBuilder sb2);

        public static int MtGetSiteWN_NATIVE(string cCall, out MtSiteStr pSite, int nDepth, out MtSiteNull pSiteNull)
        {
            //...Log2.v("\n\nMtUtils.MtGetSiteWN_NATIVE(): Entry");

            pSite = new MtSiteStr(MtSiteStr.Init.UNALLOCATED);
            pSiteNull = new MtSiteNull(MtSiteNull.Init.UNALLOCATED);
            int nRet = -666;

            //Marshal pSite for native call.
            IntPtr pSitePtr = Marshal.AllocHGlobal(Marshal.SizeOf(pSite));
            Marshal.StructureToPtr(pSite, pSitePtr, false);

            //Marshal pSiteNull for native call.
            IntPtr pSiteNullPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pSiteNull));
            Marshal.StructureToPtr(pSiteNull, pSiteNullPtr, false);

            //Native call.
            nRet = mtGetSiteWN(cCall, ref pSitePtr, 1, ref pSiteNullPtr);

            //Reverse marshal pSite after native call.
            Marshal.PtrToStructure(pSitePtr, pSite);

            //Reverse marshal pSiteNull after native call.
            Marshal.PtrToStructure(pSiteNullPtr, pSiteNull);

            //...Log2.v("\n\nMtUtils.MtGetSiteWN_NATIVE(): Exit");
            return nRet;
        }

#endif
        //----------------------------------------------------------------------------------

        public const string SELECT = "SELECT call1,call2,bndcde,anum,ause,acode,aht,azmth,elvtn,dist,offazm,tazmth,telvtn,tgain,txfdlnth,txfdlnlh,txfdlntv,txfdlnlv,rxfdlnth,rxfdlnlh,rxfdlntv,rxfdlnlv,txpadpam,rxpadlna,txcompl,rxcompl,obsloss,kvalue,atwrno,nota,apoint,sdate,licence,mdate,mtime,userid FROM  main.mt_ante WHERE call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and anum = {3} ";

        // The statics used by MtEnumSite
        static SQLHANDLE hEnumStmt;
        static SQLHDBC hEnumConn;

        //----------------------------------------------------------------------------------

        /// <summary>
        /// Populates a MtSiteStr object with data fetched from the the DB tables <b>main.mt_site</b>,
        /// <b>main.mt_ante</b> and <b>main.mt_chan</b>together with their associated nullInds. 
        /// The caller prescribes the 'depth' of retrieval, i.e. 'site only', site and antennae' 
        /// or 'site, antennae and channels'.
        /// </summary>
        /// <remarks>
        /// If nDepth = 1 only the site will be returned, 
        /// If nDepth = 2 the site and antennas will be returned,
        /// If nDepth = 3 the site, antennas and channels will be returned.
        /// </remarks>
        /// <param name="cCall"> - call sign of the SITE to be retrieved.</param>
        /// <param name="mtSiteStr"> - a MtSiteStr_NEW object populated with data.</param>
        /// <param name="nDepth"> - 1, 2 or 3 (see remarks above).</param>
        /// <param name="mtSiteNull"> - a MtSiteNull_NEW object populated with data.</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - call succeeded in retrieving data from the DB.</para>
        /// <para>- Constant.FAILURE - invalid number of sites, antennae or channels.</para>
        /// <para>- ErrorMessages.INVALID_DATA - either cCall or nDepth is invalid.</para>
        /// <para>- ErrorMessages.ODBC_SQLALLOCHANDLE_FAILED - ODBC SQLAllocHandle() failed.</para>
        /// <para>- ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int MtGetSiteWN(string cCall, out MtSiteStr mtSiteStr, int nDepth, out MtSiteStrNulls mtSiteNull)
        {
            //...Log2.v("\n\nMtUtils.MtGetSiteWN(): Entry");

            string cSQLBuff;

            int nRet = Constant.SUCCESS;

            // Satisfy the 'out' requirements.
            mtSiteStr = null;
            mtSiteNull = null;

            // First, sanity check the input data and return if there is a problem..
            Boolean cCallHasNoContent = String.IsNullOrWhiteSpace(cCall);
            Boolean nDepthIsNotValid = (nDepth < 1) || (nDepth > 3);   //Therefor nDepth = 1, 2, or 3.
            if (cCallHasNoContent || nDepthIsNotValid)
            {
                Log2.e("\r\nMtUtils.MtGetSiteWN(): ERROR: Invalid cCall or nDepth : " + cCall + ", " + nDepth);
                nRet = Error.INVALID_DATA;
                return nRet;
            }

            // ============================================
            // Find and fetch the MtSite data from the MDB.
            // ============================================

            string siteWhere = " call1 = '" + cCall + "'";

            int nSiteCursor = DynMdbSite.MtSelectSite(siteWhere, null);

            // We are assuming that only one MtSite in the MDB has a call1 column that matches the cCall
            // provided by the caller. We should test for this!
            cSQLBuff = " call1='" + cCall + "' ";
            int nNumSites = DynMdbSite.DbCountRows(cSQLBuff);

            //...Log2.v("\r\nMtUtils.MtGetSiteWN(): nNumSites = " + nNumSites);

            // Check for failure of DbCountRows() to return valid number of sites.
            if (nNumSites < 0)
            {
                Log2.e("\r\nMtUtils.MtGetSiteWN(): ERROR: Marker F: DbCountRows() returned an invalid value: " + nNumSites);
                string str = String.Format("mtGetSiteWN03: Ingres error {0} getting sites.", nNumSites);
                GenUtil.SetErr(str);
                DynMdbSite.MtCloseSite(nSiteCursor);
                return (Constant.FAILURE);
            }
            // Handle the pathological case of no sites to read from the MDB.
            else if (nNumSites == 0)
            {
                //...Log2.v("\r\nMtUtils.MtGetSiteWN(): Marker G: site not found in MDB; cCall = " + cCall);
                DynMdbSite.MtCloseSite(nSiteCursor);
                return (Constant.FAILURE);
            }
            // Handle the pathological case of too many sites to read from the MDB.
            else if (nNumSites >= 2)
            {
                Log2.e("\r\nMtUtils.MtGetSiteWN(): ERROR: Marker G-1: multiple sites in MDB with cCall = " + cCall);
                DynMdbSite.MtCloseSite(nSiteCursor);
                return (Error.INVALID_DATA);
            }
            else // #1. We have now found the site; next get the column data. 

            {
                // First, create and populate the structured data objects to be output.
                // These constructors set appropriate initial values.
                mtSiteStr = new MtSiteStr(MtSiteStr.Init.UNALLOCATED);
                mtSiteNull = new MtSiteStrNulls(MtSiteStrNulls.Init.UNALLOCATED);

                // Set the depth parameter.
                mtSiteStr.nDepth = nDepth;

                MtSite mtSite;
                SQLLEN[] nullInd = NullHelper.CreateArrayOfNullInd(MtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                if (DynMdbSite.MtFetchSite(nSiteCursor, out mtSite, out nullInd) == Constant.SUCCESS)
                {
                    // Save the site and associated column-nulls that we just fetched from the MDB.
                    mtSiteStr.stSite = mtSite;
                    mtSiteNull.anSiteNull = nullInd;
                }
                else
                {
                    Log2.e("\r\nMtUtils.MtGetSiteWN(): ERROR: Marker G-2: failed to fetch an extant site from the MDB; cCall = " + cCall);
                    DynMdbSite.MtCloseSite(nSiteCursor);
                    return (Constant.FAILURE);
                }

                // Free the cursor object (closes hStmt and hConn).
                DynMdbSite.MtCloseSite(nSiteCursor);
            }

            // At this point we have the site read in from the MDB and stored in the site structure.  
            // If nDepth is 1, then this is all that was required and we can exit.  
            if (nDepth == 1)
            {
                //...Log2.v("\n\nMtUtils.MtGetSiteWN(): Exit (nDepth == 1)");
                return Constant.SUCCESS;
            }
            // End of fetching the site data from the MDB.

            // ============================================
            // Find and fetch the MtAnte data from the MDB.
            // ============================================

            // At this point, nDepth is 2 or 3, so we need to fetch and store (at least) the antennae data from the MDB.
            MtAnte[] pAnts = null;
            SQLLEN[][] pAntNull = null;  // Usage: [antenna, column]
            int nNumAnts;

            // First find out how many antennae there are in order to allocate the array.
            //sprintf_s(cSQLBuff, sizeof(cSQLBuff), "call1 = '%s'", cCall);
            cSQLBuff = " call1='" + cCall + "' ";
            nNumAnts = DynMdbAntenna.DbCountRows(cSQLBuff);

            //...Log2.v("\r\nMtUtils.MtGetSiteWN(): nNumAnts = " + nNumAnts);

            // Check for failure of DbCountRows() to return valid number of antennae.
            if (nNumAnts < 0)
            {
                Log2.e("\r\nMtUtils.MtGetSiteWN(): ERROR: Marker F: DbCountRows() returned an invalid value: " + nNumAnts);
                string str = String.Format("mtGetSiteWN03: Ingres error {0} getting antennas.", nNumAnts);
                GenUtil.SetErr(str);
                return (nNumAnts);
            }

            // Handle the case of no antennae to read from the MDB.
            if (nNumAnts == 0)
            {
                Log2.w("\r\nMtUtils.MtGetSiteWN(): WARNING: Marker G: number of antennae to read from MDB is zero.");
            }
            else // #1, nNumAnts > 0
            {
                string anteWhere = " call1 = '" + cCall + "'";
                string anteOrderBy = " call1, call2, bndcde, anum";

                int nAnteCursor = DynMdbAntenna.MtSelectAntenna(anteWhere, anteOrderBy);

                // Instantiate the objects required store the antennae data (multiple fetches from the MDB).
                pAnts = Arrays.CreateArrayUsingDefaultElementConstructor<MtAnte>(nNumAnts);
                pAntNull = new SQLLEN[nNumAnts][];

                // Now fetch the selected antennae data from the MDB using a loop.
                MtAnte mtAnte;
                SQLLEN[] nullInd;
                int nAnte = 0;
                while (DynMdbAntenna.MtFetchAntenna(nAnteCursor, out mtAnte, out nullInd) == Constant.SUCCESS)
                {
                    nAnte++;

                    // Sanity-check the number of antennae read from the MDB so far.
                    if (nAnte > nNumAnts)
                    {
                        Log2.e("\r\nMtUtils.MtGetSiteWN(): ERROR: Marker J: SQLFetch(): antennae count exceeds that of DbCountRows()");
                        DynMdbAntenna.MtCloseAntenna(nAnteCursor);
                        GenUtil.SetError(1007, "*ERROR* ftGetSiteWN: Antenna counts wrong.");
                        return (-10);
                    }

                    // Accumulate the data over successive fetch-loops.
                    pAnts[nAnte - 1] = mtAnte;
                    pAntNull[nAnte - 1] = nullInd;

                } //end of while-loop

                // We have now accumulated all the antennae data - 'attach' it to mtSiteStr.
                mtSiteStr.stAntsPtr = pAnts;
                mtSiteStr.nNumAnts = nAnte;
                // We have now accumulated all the antennae nullInd data - 'attach' it to mtSiteNull.
                mtSiteNull.anAntsNullPtr = pAntNull;


                // Free the SQL statement handle and cursor.
                DynMdbAntenna.MtCloseAntenna(nAnteCursor);

            } // else #1, nNumAnts > 0

            // At this point we have the site and antennae read in from the MDB.  
            // If nDepth is 2, then this is all that was required and we can exit.  
            if (nDepth == 2)
            {
                //...Log2.v("\n\nMtUtils.MtGetSiteWN(): Exit (nDepth == 2)");
                return Constant.SUCCESS; ;
            }
            // End of fetching the antennae data from the MDB. -------------------------------------------



            // ============================================
            // Find and fetch the MtChan data from the MDB.
            // ============================================

            // We have now loaded the site and antennae data (if any).  
            // If nDepth = 3, we need to fetch the channel data from the MDB.

            if (nDepth == 3) // #3
            {
                // Use these two objects to accumulate the channel data fetched from the MDB.
                MtChan[] pChan = null;
                SQLLEN[][] pChanNull = null;

                // Determine how many channels there are in order to allocate the array.
                cSQLBuff = "call1='" + cCall + "'";
                int nNumChan = DynMdbChannel.DbCountRows(cSQLBuff);

                //...Log2.v("\r\nMtUtils.MtGetSiteWN(): nNumChan = " + nNumChan);

                if (nNumChan < 0)
                {
                    // An error occurred - deal with it.
                    Log2.e("\r\nMtUtils.MtGetSiteWN(): ERROR: Marker M: DbCountRows() returned nNumChan = " + nNumChan);
                    string str = String.Format("mtGetSiteWN03: Ingres error {0} getting channels.", nNumAnts);
                    GenUtil.SetErr(str);
                    return -24;
                }

                // Handle the case of no channels to read (i.e. nNumChan == 0).
                else if (nNumChan == 0)
                {
                    Log2.w("\r\nMtUtils.MtGetSiteWN(): WARNING: Marker N: number of channels to read from MDB is zero.");
                }

                // Handle the 'regular' case of nNumChan > 0.
                else  // #2, nNumChan > 0
                {
                    // Accumulate the channel and nullInd data in these two objects.
                    pChan = Arrays.CreateArrayUsingDefaultElementConstructor<MtChan>(nNumChan);
                    pChanNull = new SQLLEN[nNumChan][];

                    string chanWhere = " call1 = '" + cCall + "'";
                    string chanOrderBy = " call1, call2, bndcde, chid";

                    int nChanCursor = DynMdbChannel.MtSelectChannel(chanWhere, chanOrderBy);

                    // Start a loop to fetch all the selected channel data from the MDB.
                    int nChan = 0;  //This counts the number of successful channel fetches so far; it is auto-incremented by FtFetchChannel().
                    MtChan mtChan;
                    SQLLEN[] nullInd;

                    while (DynMdbChannel.MtFetchChannel(nChanCursor, out mtChan, out nullInd) == Constant.SUCCESS)
                    {
                        nChan++;

                        // Sanity-check the number of channel records read from the MDB so far.
                        if (nChan > nNumChan)
                        {
                            Log2.e("\r\nMtUtils.MtGetSiteWN(): ERROR: Marker R: SQLFetch(): channel count exceeds that of DbCountRows()");
                            DynMdbChannel.MtCloseChannel(nChanCursor);
                            GenUtil.SetError(1009, "mtGetSiteWN27: Channel counts wrong.");
                            return (-27);
                        }

                        //Accumulate the channel data over successive fetch-loops.
                        pChan[nChan - 1] = mtChan;
                        pChanNull[nChan - 1] = nullInd;

                    } // end of while fetch-loop.

                    // We have now accumulated all the channel data - 'attach' it to mtSiteStr.
                    mtSiteStr.stChanPtr = pChan;
                    mtSiteStr.nNumChans = nChan;
                    // We have now accumulated all the channel nullInd data - 'attach' it to mtSiteNull.
                    mtSiteNull.anChanNullPtr = pChanNull;

                    // Release resources.
                    DynMdbChannel.MtCloseChannel(nChanCursor);

                } // #2, if nNumChan > 0

            } // #3, if (nDepth == 3)

            //...Log2.v("\n\nMtUtils.MtGetSiteWN(): Exit, nDepth == 3");
            return (nRet);
        }  //end of method

        //--------------------------------------------------------------------------------------------       

        /// <summary>
        /// Retrieves a SITE record from the the db table <b>main.mt_site</b>. 
        /// The caller prescribes the 'depth' of retrieval, i.e. 'site only', site and antennae' 
        /// or 'site, antennae and channels'.
        /// </summary>
        /// <remarks>
        /// If nDepth = 1 only the site will be returned, 
        /// If nDepth = 2 the site and antennas will be returned,
        /// If nDepth = 3 the site, antennas and channels will be returned.
        /// </remarks>
        /// <param name="cCall"> - call sign of the SITE to be retrieved.</param>
        /// <param name="pSite"> - a MtSiteStr_NEW object populated with data.</param>
        /// <param name="nDepth"> - 1, 2 or 3 (see remarks above).</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - call succeeded in retrieving data from the DB.</para>
        /// <para>- Constant.FAILURE - invalid number of sites, antennae or channels.</para>
        /// <para>- ErrorMessages.INVALID_DATA - either cCall or nDepth is invalid.</para>
        /// <para>- ErrorMessages.ODBC_SQLALLOCHANDLE_FAILED - ODBC SQLAllocHandle() failed.</para>
        /// <para>- ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int MtGetSite(string cCall, out MtSiteStr pSite, int nDepth)
        {
            MtSiteStrNulls pNulls;
            int nRet;

            /*  Just get them with nulls, and then discard the null array.  */
            nRet = MtGetSiteWN(cCall, out pSite, nDepth, out pNulls);

            //MtFreeNulls(pNulls);

            return (nRet);
        }

        /// <summary>
        /// Returns the index of the next element in an array of channels that has
        /// prescribed call2, band, and anum. The first call should be made with 
        /// nLastOne set to -1.
        /// </summary>
        /// <param name="pChan"> - an array of MtChan objects.</param>
        /// <param name="nChanCount"> - number of channels.</param>
        /// <param name="cCall2"> - prescribed call2.</param>
        /// <param name="cBand"> - precribed band code.</param>
        /// <param name="nAnum"> - prescribed antenna number.</param>
        /// <param name="nLastOne"> - index returned by previous call.</param>
        /// <returns></returns>
        /// <para>- non-negative - index of next channel objec that satisfies prescribed criteria.</para>
        /// <para>- Constant.FAILURE - no match found.</para>
        public static int MtNextChan(MtChan[] pChan, int nChanCount, string cCall2, string cBand, short nAnum, int nLastOne)
        {
            //...Log2.v("\n\nMtUtils.MtNextChan(): Entry: nChanCount = " + nChanCount + "   nLastOne = " + nLastOne);

            int nRet = -2;

            for (nLastOne++; nLastOne < nChanCount; nLastOne++)
            {
                if (pChan[nLastOne].call2.Equals(cCall2) &&
                        pChan[nLastOne].bndcde.Equals(cBand) &&
                            (pChan[nLastOne].antnumbtx1 == nAnum ||
                            pChan[nLastOne].antnumbtx2 == nAnum ||
                            pChan[nLastOne].antnumbrx1 == nAnum ||
                            pChan[nLastOne].antnumbrx2 == nAnum ||
                            pChan[nLastOne].antnumbrx3 == nAnum))
                {
                    /*  We got one. */
                    nRet = nLastOne;
                    break;
                }
            }
            //...Log2.v("\n\nMtUtils.MtNextChan(): Exit: returned " + nRet);
            return nRet;
        }

        /// <summary>
        /// Returns a count of the number of channels for an antenna, given prescribed 
        /// values of call2, bandcode and anum.
        /// </summary>
        /// <param name="call2"> - call2</param>
        /// <param name="bndcde"> - band code.</param>
        /// <param name="anum"> - antenna number.</param>
        /// <param name="pChan"> - an array of MtChan objects.</param>
        /// <param name="nChanCount"> - the number of MtChan objects in pChan.</param>
        /// <returns> - count of the number of channels.</returns>
        public static int MtChansInAnte1(string call2, string bndcde, int anum, MtChan[] pChan, int nChanCount)
        {
            //...Log2.v("\n\nMtUtils.MtChansInAnte1(): Entry");

            int nCount = 0;
            int nLast;

            nLast = MtNextChan(pChan, nChanCount, call2, bndcde, (short)anum, -1);
            while (nLast >= 0)
            {
                nCount++;
                nLast = MtNextChan(pChan, nChanCount, call2, bndcde, (short)anum, nLast);
            }
            //...Log2.v("\n\nMtUtils.MtChansInAnte1(): Exit: returned " + nCount);
            return nCount;
        }

        /// <summary>
        /// Searches a MtSiteStr_NEW object for a channel with prescribed  call2, band and chid.
        /// </summary>
        /// <param name="pSite"> - MtSiteStr_NEW object to search.</param>
        /// <param name="cCall"> - prescribed call.</param>
        /// <param name="cBand"> - prescribed band code.</param>
        /// <param name="cChid"> - prescribed channel ID.</param>
        /// <returns></returns>
        /// <para>- non-negative - index of the first element of the channel array that satisfies the precribed criteria.</para>
        /// <para>- Constant.FAILURE - no match found.</para>
        public static int FindmtChid(MtSiteStr pSite, string cCall, string cBand, string cChid)
        {
            //...Log2.v("\n\nMtUtils.FindmtChid(): Entry");

            short nInd = 0;
            int nRet = -1;
            MtChan pChan;

            if (pSite == null)
            {
                /*  If there is no site, then don't find the channel. */
                nRet = -1;
            }
            else
            {
                for (nInd = 0; nInd < pSite.nNumChans; nInd++)
                {
                    pChan = pSite.stChanPtr[nInd];
                    if (pChan.call2.Equals(cCall) && pChan.bndcde.Equals(cBand) && pChan.chid.Equals(cChid))
                    {
                        /*  Got it return its index */
                        nRet = nInd;
                        break;
                    }
                }
            }
            //...Log2.v("\n\nMtUtils.FindmtChid(): Exit, returned " + nRet);
            return nRet;
        }

        /// <summary>
        /// Check whether a MtSiteStr_NEW object contains an antenna with
        /// prescribed call2, band code, and anum.
        /// </summary>
        /// <param name="pSite"> - MtSiteStr_NEW object to be searched.</param>
        /// <param name="cCall2"> - prescribed call2 sign.</param>
        /// <param name="bndcde"> - prescribed band code.</param>
        /// <param name="nAnum"> - prescribed antenna number.</param>
        /// <returns></returns>
        /// <para> non-negative - the index of the FtAnte array element that is the first match found.</para>
        /// <para> negative - no match found.</para>
        public static int MtFindAnte(MtSiteStr pSite, string cCall2, string bndcde, int nAnum)
        {
            int nRet = -1;
            int nInd = 0;

            if (pSite == null || pSite.nDepth < 2)
            {
                nRet = -2;      /*	No site, or antennas not loaded */
            }
            else
            {
                for (nInd = 0;
                    nInd < pSite.nNumAnts;
                    nInd++)
                {
                    if (pSite.stAntsPtr[nInd].call2.Trim().Equals(cCall2.Trim()))
                    {
                        /*  Possible - same call2, now check band and antenna */
                        if (pSite.stAntsPtr[nInd].bndcde.Trim().Equals(bndcde.Trim()))
                        {
                            /* Okay band is good, now the antenna, we check anum */
                            if (pSite.stAntsPtr[nInd].anum == nAnum)
                            {
                                /*  It's a hit.  mark it as found */
                                nRet = nInd;
                                break;
                            }
                        }
                    }
                }
            }

            return (nRet);
        }

        /// <summary>
        /// Determines whether there is any site record in the database table <b>main.mt_site</b> that satisfies a
        /// prescribed search criteria (i.e. an SQL 'where' clause).
        /// </summary>
        /// <param name="cSearch"> - prescribes the 'where' clause parameters.</param>
        /// <param name="cCallFound"> - on input this prescribes that any found call sign must be aphabetically greater 
        /// than cCallFound; if a record that matches all the search criteria is found, cCallFound returns its call sign.</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - at least one SITE record exists that satisfies the search criteria.</para>
        /// <para>- ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed.</para>
        /// <para>- ErrorMessages.ODBC_GET_FAILED - call to ODBC.SQLGetData() threw an exception. </para>
        /// <para>- Constant.NOMORERECS - no SITE record found that matches search criteria.</para>
        public static int MtEnumSite(string cSearch,
                                     ref string cCallFound)
        {
            //...Log2.v("\nMtUtils.MtEnumSite(): Entry: cSearch = " + Strings.EnQuote(cSearch));

            int nRet = 0;
            SQLLEN vNullInd;
            string s_SQL;
            string cWhere;

            SQLRETURN sqlRet;


            /*	Create the statement. */
            hEnumConn = Ssutil.NewConn();
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hEnumConn, out hEnumStmt);

            if (cSearch == null || cSearch.Length == 0)
            {
                cWhere = "";
            }
            else
            {
                cWhere = "(";
                cWhere += cSearch;
                cWhere += ") and ";
            }
            s_SQL = String.Format("select top 1 call1 FROM {0} WHERE {1} call1 > '{2}' ORDER BY call1 ",
                                    DynMdbSite.TableName, cWhere, cCallFound);

            //...Log2.v("\nMtUtils.MtEnumSite(): s_SQL =\n" + s_SQL);

            sqlRet = ODBC.SQLExecDirect(hEnumStmt, s_SQL, s_SQL.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nMtUtils.MtEnumSite(): ERROR: call to SQLExecDirect() failed for:\n" + s_SQL);
                Log2.e("\n" + Environment.StackTrace);

                /*  Error encountered */
                nRet = -1;
            }

            /*  Now get the next call sign */
            if (nRet >= 0)
            {
                sqlRet = ODBC.SQLFetch(hEnumStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    nRet = 1;
                }
                else
                {
                    if (Ssutil.DbGetString(hEnumStmt, 1, "Call1", out cCallFound, Constant.CALLSIGN_SZ, out vNullInd) == 0)
                    {
                        nRet = 0;
                    }
                    else
                    {
                        Ssutil.DbGetDiagStmt(hEnumStmt, "ftEnumSite - Could not get callsign");
                        nRet = -1;
                    }
                }
            }

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hEnumStmt);
            Ssutil.DisConn(hEnumConn);

            //...Log2.v("\nMtUtils.MtEnumSite(): Exit: nRet = " + nRet);
            return nRet;
        }

        /// <summary>
        /// Read a single antenna. This is out of band as well.  
        /// </summary>
        /// <param name="cCall1"></param>
        /// <param name="cCall2"></param>
        /// <param name="cBand"></param>
        /// <param name="anum"></param>
        /// <param name="mtAnte"></param>
        /// <param name="mtAnteNulls"></param>
        /// <returns></returns>
        public static int MtReadAnte(string cCall1, string cCall2, string cBand, int anum,
    out MtAnte mtAnte, out SQLLEN[] mtAnteNulls)
        {
            // 'out' requirement.
            mtAnte = null;
            mtAnteNulls = null;

            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();
            SQLRETURN sqlRet;

            string cSQLBuff;
            int nRet = 0;

            /*	Allocate space and read in the antennas.  We use the ingres
                    begin/end sequence for this.  This is proprietary, but the
              preparation and execution of cursors that would be needed
              (because the tablename has to be changed each time) was not
              felt to be worth it, and the actual work is very localized,
              meaning that it could be changed easily at any time.
            */
            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQLBuff = String.Format(SELECT, cCall1, cCall2, cBand, anum);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQLBuff, cSQLBuff.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                //	Something went wrong on the exec.
                Log2.e("\nMtUtils.MtReadAnte(): ERROR: SQLExecDirect() failed.");
                Ssutil.DbGetDiagStmt(hStmt, "mtReadAnte04 - Error retrieving antenna.");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            // Now fetch the antenna
            sqlRet = ODBC.SQLFetch(hStmt);
            if (sqlRet == ODBC.SQL_NO_DATA)
            {
                Log2.e("\n\nMtUtils.MtReadAnte(): ERROR: MDB tables are incoherent: sqlRet == ODBC.SQL_NO_DATA for query = \n" + cSQLBuff);
                nRet = Constant.NOMORERECS;
            }
            else if (!ODBC.IsOK(sqlRet))
            {
                /*  Something has gone terribly wrong! */
                Log2.e("\nMtUtils.MtReadAnte(): ERROR: SQLFetch() failed.");
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, hStmt);
                nRet = -1;
            }
            else
            {
                /*  Got the antenna.  move it into the array.  Zero the array first */
                mtAnte = new MtAnte();
                mtAnteNulls = NullHelper.CreateArrayOfNullInd(MtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                try
                {
                    Ssutil.DbGetString(hStmt, 1, "call1", out mtAnte.call1, MtAnte.CALL1_SZ, out mtAnteNulls[MtAnte.CALL1]);
                    Ssutil.DbGetString(hStmt, 2, "call2", out mtAnte.call2, MtAnte.CALL2_SZ, out mtAnteNulls[MtAnte.CALL2]);
                    Ssutil.DbGetString(hStmt, 3, "bndcde", out mtAnte.bndcde, MtAnte.BNDCDE_SZ, out mtAnteNulls[MtAnte.BNDCDE]);
                    Ssutil.DbGetShort(hStmt, 4, "anum", out mtAnte.anum, out mtAnteNulls[MtAnte.ANUM]);
                    Ssutil.DbGetString(hStmt, 5, "ause", out mtAnte.ause, MtAnte.AUSE_SZ, out mtAnteNulls[MtAnte.AUSE]);
                    Ssutil.DbGetString(hStmt, 6, "acode", out mtAnte.acode, MtAnte.ACODE_SZ, out mtAnteNulls[MtAnte.ACODE]);
                    Ssutil.DbGetFloat(hStmt, 7, "aht", out mtAnte.aht, out mtAnteNulls[MtAnte.AHT]);
                    Ssutil.DbGetFloat(hStmt, 8, "azmth", out mtAnte.azmth, out mtAnteNulls[MtAnte.AZMTH]);
                    Ssutil.DbGetFloat(hStmt, 9, "elvtn", out mtAnte.elvtn, out mtAnteNulls[MtAnte.ELVTN]);
                    Ssutil.DbGetFloat(hStmt, 10, "dist", out mtAnte.dist, out mtAnteNulls[MtAnte.DIST]);
                    Ssutil.DbGetString(hStmt, 11, "offazm", out mtAnte.offazm, MtAnte.OFFAZM_SZ, out mtAnteNulls[MtAnte.OFFAZM]);
                    Ssutil.DbGetFloat(hStmt, 12, "tazmth", out mtAnte.tazmth, out mtAnteNulls[MtAnte.TAZMTH]);
                    Ssutil.DbGetFloat(hStmt, 13, "telvtn", out mtAnte.telvtn, out mtAnteNulls[MtAnte.TELVTN]);
                    Ssutil.DbGetFloat(hStmt, 14, "tgain", out mtAnte.tgain, out mtAnteNulls[MtAnte.TGAIN]);
                    Ssutil.DbGetString(hStmt, 15, "txfdlnth", out mtAnte.txfdlnth, MtAnte.TXFDLNTH_SZ, out mtAnteNulls[MtAnte.TXFDLNTH]);
                    Ssutil.DbGetFloat(hStmt, 16, "txfdlnlh", out mtAnte.txfdlnlh, out mtAnteNulls[MtAnte.TXFDLNLH]);
                    Ssutil.DbGetString(hStmt, 17, "txfdlntv", out mtAnte.txfdlntv, MtAnte.TXFDLNTV_SZ, out mtAnteNulls[MtAnte.TXFDLNTV]);
                    Ssutil.DbGetFloat(hStmt, 18, "txfdlnlv", out mtAnte.txfdlnlv, out mtAnteNulls[MtAnte.TXFDLNLV]);
                    Ssutil.DbGetString(hStmt, 19, "rxfdlnth", out mtAnte.rxfdlnth, MtAnte.RXFDLNTH_SZ, out mtAnteNulls[MtAnte.RXFDLNTH]);
                    Ssutil.DbGetFloat(hStmt, 20, "rxfdlnlh", out mtAnte.rxfdlnlh, out mtAnteNulls[MtAnte.RXFDLNLH]);
                    Ssutil.DbGetString(hStmt, 21, "rxfdlntv", out mtAnte.rxfdlntv, MtAnte.RXFDLNTV_SZ, out mtAnteNulls[MtAnte.RXFDLNTV]);
                    Ssutil.DbGetFloat(hStmt, 22, "rxfdlnlv", out mtAnte.rxfdlnlv, out mtAnteNulls[MtAnte.RXFDLNLV]);
                    Ssutil.DbGetFloat(hStmt, 23, "txpadpam", out mtAnte.txpadpam, out mtAnteNulls[MtAnte.TXPADPAM]);
                    Ssutil.DbGetFloat(hStmt, 24, "rxpadlna", out mtAnte.rxpadlna, out mtAnteNulls[MtAnte.RXPADLNA]);
                    Ssutil.DbGetFloat(hStmt, 25, "txcompl", out mtAnte.txcompl, out mtAnteNulls[MtAnte.TXCOMPL]);
                    Ssutil.DbGetFloat(hStmt, 26, "rxcompl", out mtAnte.rxcompl, out mtAnteNulls[MtAnte.RXCOMPL]);
                    Ssutil.DbGetFloat(hStmt, 27, "obsloss", out mtAnte.obsloss, out mtAnteNulls[MtAnte.OBSLOSS]);
                    Ssutil.DbGetFloat(hStmt, 28, "kvalue", out mtAnte.kvalue, out mtAnteNulls[MtAnte.KVALUE]);
                    Ssutil.DbGetShort(hStmt, 29, "atwrno", out mtAnte.atwrno, out mtAnteNulls[MtAnte.ATWRNO]);
                    Ssutil.DbGetString(hStmt, 30, "nota", out mtAnte.nota, MtAnte.NOTA_SZ, out mtAnteNulls[MtAnte.NOTA]);
                    Ssutil.DbGetString(hStmt, 31, "apoint", out mtAnte.apoint, MtAnte.APOINT_SZ, out mtAnteNulls[MtAnte.APOINT]);
                    Ssutil.DbGetString(hStmt, 32, "sdate", out mtAnte.sdate, MtAnte.SDATE_SZ, out mtAnteNulls[MtAnte.SDATE]);
                    Ssutil.DbGetString(hStmt, 33, "licence", out mtAnte.licence, MtAnte.LICENCE_SZ, out mtAnteNulls[MtAnte.LICENCE]);
                    Ssutil.DbGetString(hStmt, 34, "mdate", out mtAnte.mdate, MtAnte.MDATE_SZ, out mtAnteNulls[MtAnte.MDATE]);
                    Ssutil.DbGetString(hStmt, 35, "mtime", out mtAnte.mtime, MtAnte.MTIME_SZ, out mtAnteNulls[MtAnte.MTIME]);
                    Ssutil.DbGetString(hStmt, 36, "userid", out mtAnte.userid, MtAnte.USERID_SZ, out mtAnteNulls[MtAnte.USERID]);
                }
                catch (Exception e)
                {
                    Log2.e("\nMtUtils.MtReadAnte(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                    GenUtil.SetError(1011, "Could not get antenna, because of field " + e.Message);
                    nRet = Error.ODBC_GET_FAILED;
                }
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return nRet;
        }

        /// <summary>
        /// </summary>
        /// <param name="ftSiteStr"></param>
        /// <param name="ftSiteStrNulls"></param>
        /// <param name="mtSiteStr"></param>
        /// <param name="mtSiteStrNulls"></param>
        /// <returns></returns>
        public static int MtToFtWN(out FtSiteStr ftSiteStr,    /*  Ptr to output Ptr to ft site  */
                                    out FtSiteStrNulls ftSiteStrNulls,
                                    MtSiteStr mtSiteStr,    /*  Ptr to input mt site          */
                                    MtSiteStrNulls mtSiteStrNulls)
        {
            // 'out' requirements.
            ftSiteStr = null;
            ftSiteStrNulls = null;

            short nInd = 0;
            int nRet = 0;

            if (mtSiteStr == null)
            {
                // Pathological case: there is nothing to do.
            }
            else
            {
                // FtSiteStr is a 'high-level' structure composed of ab FtSite object, an
                // array of FtAnte objects and an array of FtChan objects.

                // FtSiteStrNulls is also a highlevel composite structure that holds all
                // the nullInd information for a single FtSiteStr object.

                // MtSiteStr and MtSiteStrNulls can be considered as holding 'subsets' of the information carried by FtSiteStr and FtSiteStrNulls, respectively.

                //------------------ FtSite member & nullInds -------------------------

                // Create a FtSite object.
                ftSiteStr = new FtSiteStr(FtSiteStr.Init.UNALLOCATED);

                // Populate the FtSiteStr object's stSite member.
                MtToFt_Site(out ftSiteStr.stSite, mtSiteStr.stSite);

                // Create the null structure for the FtSiteStr object.
                ftSiteStrNulls = new FtSiteStrNulls(FtSiteStrNulls.Init.UNALLOCATED);

                // Populate the null structure for the FtSiteStr object's stSite member..
                MtToFt_SiteNulls(out ftSiteStrNulls.anSiteNull, mtSiteStrNulls.anSiteNull);

                //------------------ FtAnte[] member & nullInds -------------------------

                ftSiteStr.stAntsPtr = new FtAnte[mtSiteStr.nNumAnts];
                ftSiteStrNulls.anAntsNullPtr = new SQLLEN[mtSiteStr.nNumAnts][];

                for (nInd = 0; nInd < mtSiteStr.nNumAnts; nInd++)
                {
                    MtToFt_Ante(out ftSiteStr.stAntsPtr[nInd], mtSiteStr.stAntsPtr[nInd]);

                    MtToFt_AnteNulls(out ftSiteStrNulls.anAntsNullPtr[nInd], mtSiteStrNulls.anAntsNullPtr[nInd]);
                }

                //------------------ FtChan[] member & nullInds -------------------------

                /*  Channels */
                ftSiteStr.stChanPtr = new FtChan[mtSiteStr.nNumChans];
                ftSiteStrNulls.anChanNullPtr = new SQLLEN[mtSiteStr.nNumChans][];

                for (nInd = 0; nInd < mtSiteStr.nNumChans; nInd++)
                {
                    MtToFt_Chan(out ftSiteStr.stChanPtr[nInd], mtSiteStr.stChanPtr[nInd]);
                    MtToFt_ChanNulls(out ftSiteStrNulls.anChanNullPtr[nInd], mtSiteStrNulls.anChanNullPtr[nInd]);
                }

                //------------------ Copy the other information -------------------------
                ftSiteStr.nNumAnts = mtSiteStr.nNumAnts;
                ftSiteStr.nNumChans = mtSiteStr.nNumChans;
                ftSiteStr.nDepth = mtSiteStr.nDepth;
            }

            return nRet;
        }

        /// <summary>
        /// </summary>
        /// <param name="ftSite"></param>
        /// <param name="mtSite"></param>
        public static void MtToFt_Site(out FtSite ftSite, MtSite mtSite)
        {
            ftSite = new FtSite();

            ftSite.call1 = mtSite.call1;
            ftSite.name = mtSite.name;
            ftSite.prov = mtSite.prov;
            ftSite.oper = mtSite.oper;
            ftSite.latit = mtSite.latit;
            ftSite.longit = mtSite.longit;
            ftSite.grnd = mtSite.grnd;
            ftSite.stats = mtSite.stats;
            ftSite.sdate = mtSite.loc;
            ftSite.icaccount = mtSite.icaccount;
            ftSite.reg = mtSite.reg;
            ftSite.spoint = mtSite.spoint;
            ftSite.nots = mtSite.nots;
            ftSite.oprtyp = mtSite.oprtyp;
            ftSite.notwr = mtSite.notwr;
            ftSite.bandwd1 = mtSite.bandwd1;
            ftSite.bandwd2 = mtSite.bandwd2;
            ftSite.bandwd3 = mtSite.bandwd3;
            ftSite.bandwd4 = mtSite.bandwd4;
            ftSite.bandwd5 = mtSite.bandwd5;
            ftSite.bandwd6 = mtSite.bandwd6;
            ftSite.bandwd7 = mtSite.bandwd7;
            ftSite.bandwd8 = mtSite.bandwd8;
            ftSite.mdate = mtSite.mdate;
            ftSite.mtime = mtSite.mtime;

            return;
        }

        /// <summary>
        /// Copy the nulls across from an mt site to an ft Site.  
        /// </summary>
        /// <param name="ftSiteNulls"></param>
        /// <param name="mtSiteNulls"></param>
        /// <returns></returns>
        public static int MtToFt_SiteNulls(out SQLLEN[] ftSiteNulls, SQLLEN[] mtSiteNulls)
        {
            ftSiteNulls = NullHelper.CreateArrayOfNullInd(FtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            ftSiteNulls[FtSite.CMD] = Constant.DB_NULL;
            ftSiteNulls[FtSite.RECSTAT] = Constant.DB_NULL;
            ftSiteNulls[FtSite.CALL1] = mtSiteNulls[MtSite.CALL1];
            ftSiteNulls[FtSite.NAME] = mtSiteNulls[MtSite.NAME];
            ftSiteNulls[FtSite.PROV] = mtSiteNulls[MtSite.PROV];
            ftSiteNulls[FtSite.OPER] = mtSiteNulls[MtSite.OPER];
            ftSiteNulls[FtSite.LATIT] = mtSiteNulls[MtSite.LATIT];
            ftSiteNulls[FtSite.LONGIT] = mtSiteNulls[MtSite.LONGIT];
            ftSiteNulls[FtSite.GRND] = mtSiteNulls[MtSite.GRND];
            ftSiteNulls[FtSite.STATS] = mtSiteNulls[MtSite.STATS];
            ftSiteNulls[FtSite.SDATE] = mtSiteNulls[MtSite.SDATE];
            ftSiteNulls[FtSite.LOC] = mtSiteNulls[MtSite.LOC];
            ftSiteNulls[FtSite.ICACCOUNT] = mtSiteNulls[MtSite.ICACCOUNT];
            ftSiteNulls[FtSite.REG] = mtSiteNulls[MtSite.REG];
            ftSiteNulls[FtSite.SPOINT] = mtSiteNulls[MtSite.SPOINT];
            ftSiteNulls[FtSite.NOTS] = mtSiteNulls[MtSite.NOTS];
            ftSiteNulls[FtSite.OPRTYP] = mtSiteNulls[MtSite.OPRTYP];
            ftSiteNulls[FtSite.SNUMB] = mtSiteNulls[MtSite.SNUMB];
            ftSiteNulls[FtSite.NOTWR] = mtSiteNulls[MtSite.NOTWR];
            ftSiteNulls[FtSite.BANDWD1] = mtSiteNulls[MtSite.BANDWD1];
            ftSiteNulls[FtSite.BANDWD2] = mtSiteNulls[MtSite.BANDWD2];
            ftSiteNulls[FtSite.BANDWD3] = mtSiteNulls[MtSite.BANDWD3];
            ftSiteNulls[FtSite.BANDWD4] = mtSiteNulls[MtSite.BANDWD4];
            ftSiteNulls[FtSite.BANDWD5] = mtSiteNulls[MtSite.BANDWD5];
            ftSiteNulls[FtSite.BANDWD6] = mtSiteNulls[MtSite.BANDWD6];
            ftSiteNulls[FtSite.BANDWD7] = mtSiteNulls[MtSite.BANDWD7];
            ftSiteNulls[FtSite.BANDWD8] = mtSiteNulls[MtSite.BANDWD8];
            ftSiteNulls[FtSite.MDATE] = mtSiteNulls[MtSite.MDATE];
            ftSiteNulls[FtSite.MTIME] = mtSiteNulls[MtSite.MTIME];

            return 0;
        }

        /// <summary>
        /// </summary>
        /// <param name="ftAnte"></param>
        /// <param name="mtAnte"></param>
        public static void MtToFt_Ante(out FtAnte ftAnte, MtAnte mtAnte)
        {
            ftAnte = new FtAnte();

            ftAnte.call1 = mtAnte.call1;
            ftAnte.call2 = mtAnte.call2;
            ftAnte.bndcde = mtAnte.bndcde;
            ftAnte.anum = mtAnte.anum;
            ftAnte.ause = mtAnte.ause;
            ftAnte.acode = mtAnte.acode;
            ftAnte.aht = mtAnte.aht;
            ftAnte.azmth = mtAnte.azmth;
            ftAnte.elvtn = mtAnte.elvtn;
            ftAnte.dist = mtAnte.dist;
            ftAnte.offazm = mtAnte.offazm;
            ftAnte.tazmth = mtAnte.tazmth;
            ftAnte.telvtn = mtAnte.telvtn;
            ftAnte.tgain = mtAnte.tgain;
            ftAnte.txfdlnth = mtAnte.txfdlnth;
            ftAnte.txfdlnlh = mtAnte.txfdlnlh;
            ftAnte.txfdlntv = mtAnte.txfdlntv;
            ftAnte.txfdlnlv = mtAnte.txfdlnlv;
            ftAnte.rxfdlnth = mtAnte.rxfdlnth;
            ftAnte.rxfdlnlh = mtAnte.rxfdlnlh;
            ftAnte.rxfdlntv = mtAnte.rxfdlntv;
            ftAnte.rxfdlnlv = mtAnte.rxfdlnlv;
            ftAnte.txpadpam = mtAnte.txpadpam;
            ftAnte.rxpadlna = mtAnte.rxpadlna;
            ftAnte.txcompl = mtAnte.txcompl;
            ftAnte.rxcompl = mtAnte.rxcompl;
            ftAnte.obsloss = mtAnte.obsloss;
            ftAnte.kvalue = mtAnte.kvalue;
            ftAnte.atwrno = mtAnte.atwrno;
            ftAnte.nota = mtAnte.nota;
            ftAnte.apoint = mtAnte.apoint;
            ftAnte.sdate = mtAnte.sdate;
            ftAnte.licence = mtAnte.licence;
            ftAnte.mdate = mtAnte.mdate;
            ftAnte.mtime = mtAnte.mtime;

            return;
        }

        /// <summary>
        /// </summary>
        /// <param name="ftChan"></param>
        /// <param name="mtChan"></param>
        public static void MtToFt_Chan(out FtChan ftChan, MtChan mtChan)
        {
            ftChan = new FtChan();

            ftChan.call1 = mtChan.call1;
            ftChan.call2 = mtChan.call2;
            ftChan.bndcde = mtChan.bndcde;
            ftChan.splan = mtChan.splan;
            ftChan.hl = mtChan.hl;
            ftChan.vh = mtChan.vh;
            ftChan.chid = mtChan.chid;
            ftChan.freqtx = mtChan.freqtx;
            ftChan.poltx = mtChan.poltx;
            ftChan.antnumbtx1 = mtChan.antnumbtx1;
            ftChan.antnumbtx2 = mtChan.antnumbtx2;
            ftChan.eqpttx = mtChan.eqpttx;
            ftChan.eqptutx = mtChan.eqptutx;
            ftChan.pwrtx = mtChan.pwrtx;
            ftChan.atpccde = mtChan.atpccde;
            ftChan.afsltx1 = mtChan.afsltx1;
            ftChan.afsltx2 = mtChan.afsltx2;
            ftChan.traftx = mtChan.traftx;
            ftChan.srvctx = mtChan.srvctx;
            ftChan.stattx = mtChan.stattx;
            ftChan.freqrx = mtChan.freqrx;
            ftChan.polrx = mtChan.polrx;
            ftChan.antnumbrx1 = mtChan.antnumbrx1;
            ftChan.antnumbrx2 = mtChan.antnumbrx2;
            ftChan.antnumbrx3 = mtChan.antnumbrx3;
            ftChan.eqptrx = mtChan.eqptrx;
            ftChan.eqpturx = mtChan.eqpturx;
            ftChan.afslrx1 = mtChan.afslrx1;
            ftChan.afslrx2 = mtChan.afslrx2;
            ftChan.afslrx3 = mtChan.afslrx3;
            ftChan.pwrrx1 = mtChan.pwrrx1;
            ftChan.pwrrx2 = mtChan.pwrrx2;
            ftChan.pwrrx3 = mtChan.pwrrx3;
            ftChan.trafrx = mtChan.trafrx;
            ftChan.esint = mtChan.esint;
            ftChan.tsint = mtChan.tsint;
            ftChan.srvcrx = mtChan.srvcrx;
            ftChan.statrx = mtChan.statrx;
            ftChan.routnumb = mtChan.routnumb;
            ftChan.stnnumb = mtChan.stnnumb;
            ftChan.hopnumb = mtChan.hopnumb;
            ftChan.sdate = mtChan.sdate;
            ftChan.notetx = mtChan.notetx;
            ftChan.noterx = mtChan.noterx;
            ftChan.notegnl = mtChan.notegnl;
            ftChan.cpoint = mtChan.cpoint;
            ftChan.feetx = mtChan.feetx;
            ftChan.feerx = mtChan.feerx;
            ftChan.mdate = mtChan.mdate;
            ftChan.mtime = mtChan.mtime;

            return;
        }

        /// <summary>
        /// Copy over the antenna nulls from mt to ft.  
        /// </summary>
        /// <param name="ftAnteNulls"></param>
        /// <param name="mtAnteNulls"></param>
        /// <returns></returns>
        public static int MtToFt_AnteNulls(out SQLLEN[] ftAnteNulls, SQLLEN[] mtAnteNulls)
        {
            ftAnteNulls = NullHelper.CreateArrayOfNullInd(FtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            ftAnteNulls[FtAnte.CMD] = Constant.DB_NULL;
            ftAnteNulls[FtAnte.RECSTAT] = Constant.DB_NULL;
            ftAnteNulls[FtAnte.CALL1] = mtAnteNulls[MtAnte.CALL1];
            ftAnteNulls[FtAnte.CALL2] = mtAnteNulls[MtAnte.CALL2];
            ftAnteNulls[FtAnte.BNDCDE] = mtAnteNulls[MtAnte.BNDCDE];
            ftAnteNulls[FtAnte.ANUM] = mtAnteNulls[MtAnte.ANUM];
            ftAnteNulls[FtAnte.AUSE] = mtAnteNulls[MtAnte.AUSE];
            ftAnteNulls[FtAnte.ACODE] = mtAnteNulls[MtAnte.ACODE];
            ftAnteNulls[FtAnte.AHT] = mtAnteNulls[MtAnte.AHT];
            ftAnteNulls[FtAnte.AZMTH] = mtAnteNulls[MtAnte.AZMTH];
            ftAnteNulls[FtAnte.ELVTN] = mtAnteNulls[MtAnte.ELVTN];
            ftAnteNulls[FtAnte.DIST] = mtAnteNulls[MtAnte.DIST];
            ftAnteNulls[FtAnte.OFFAZM] = mtAnteNulls[MtAnte.OFFAZM];
            ftAnteNulls[FtAnte.TAZMTH] = mtAnteNulls[MtAnte.TAZMTH];
            ftAnteNulls[FtAnte.TELVTN] = mtAnteNulls[MtAnte.TELVTN];
            ftAnteNulls[FtAnte.TGAIN] = mtAnteNulls[MtAnte.TGAIN];
            ftAnteNulls[FtAnte.TXFDLNTH] = mtAnteNulls[MtAnte.TXFDLNTH];
            ftAnteNulls[FtAnte.TXFDLNLH] = mtAnteNulls[MtAnte.TXFDLNLH];
            ftAnteNulls[FtAnte.TXFDLNTV] = mtAnteNulls[MtAnte.TXFDLNTV];
            ftAnteNulls[FtAnte.TXFDLNLV] = mtAnteNulls[MtAnte.TXFDLNLV];
            ftAnteNulls[FtAnte.RXFDLNTH] = mtAnteNulls[MtAnte.RXFDLNTH];
            ftAnteNulls[FtAnte.RXFDLNLH] = mtAnteNulls[MtAnte.RXFDLNLH];
            ftAnteNulls[FtAnte.RXFDLNTV] = mtAnteNulls[MtAnte.RXFDLNTV];
            ftAnteNulls[FtAnte.RXFDLNLV] = mtAnteNulls[MtAnte.RXFDLNLV];
            ftAnteNulls[FtAnte.TXPADPAM] = mtAnteNulls[MtAnte.TXPADPAM];
            ftAnteNulls[FtAnte.RXPADLNA] = mtAnteNulls[MtAnte.RXPADLNA];
            ftAnteNulls[FtAnte.TXCOMPL] = mtAnteNulls[MtAnte.TXCOMPL];
            ftAnteNulls[FtAnte.RXCOMPL] = mtAnteNulls[MtAnte.RXCOMPL];
            ftAnteNulls[FtAnte.OBSLOSS] = mtAnteNulls[MtAnte.OBSLOSS];
            ftAnteNulls[FtAnte.KVALUE] = mtAnteNulls[MtAnte.KVALUE];
            ftAnteNulls[FtAnte.ATWRNO] = mtAnteNulls[MtAnte.ATWRNO];
            ftAnteNulls[FtAnte.NOTA] = mtAnteNulls[MtAnte.NOTA];
            ftAnteNulls[FtAnte.APOINT] = mtAnteNulls[MtAnte.APOINT];
            ftAnteNulls[FtAnte.SDATE] = mtAnteNulls[MtAnte.SDATE];
            ftAnteNulls[FtAnte.LICENCE] = mtAnteNulls[MtAnte.LICENCE];
            ftAnteNulls[FtAnte.MDATE] = mtAnteNulls[MtAnte.MDATE];
            ftAnteNulls[FtAnte.MTIME] = mtAnteNulls[MtAnte.MTIME];

            return 0;
        }

        /// <summary>
        /// Copy across the channel nulls from mt to ft.  
        /// </summary>
        /// <param name="ftChanNulls"></param>
        /// <param name="mtChanNulls"></param>
        /// <returns></returns>
        public static int MtToFt_ChanNulls(out SQLLEN[] ftChanNulls, SQLLEN[] mtChanNulls)
        {
            ftChanNulls = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            ftChanNulls[FtChan.CMD] = Constant.DB_NULL;
            ftChanNulls[FtChan.RECSTAT] = Constant.DB_NULL;
            ftChanNulls[FtChan.CALL1] = mtChanNulls[MtChan.CALL1];
            ftChanNulls[FtChan.CALL2] = mtChanNulls[MtChan.CALL2];
            ftChanNulls[FtChan.BNDCDE] = mtChanNulls[MtChan.BNDCDE];
            ftChanNulls[FtChan.SPLAN] = mtChanNulls[MtChan.SPLAN];
            ftChanNulls[FtChan.HL] = mtChanNulls[MtChan.HL];
            ftChanNulls[FtChan.VH] = mtChanNulls[MtChan.VH];
            ftChanNulls[FtChan.CHID] = mtChanNulls[MtChan.CHID];
            ftChanNulls[FtChan.FREQTX] = mtChanNulls[MtChan.FREQTX];
            ftChanNulls[FtChan.POLTX] = mtChanNulls[MtChan.POLTX];
            ftChanNulls[FtChan.ANTNUMBTX1] = mtChanNulls[MtChan.ANTNUMBTX1];
            ftChanNulls[FtChan.ANTNUMBTX2] = mtChanNulls[MtChan.ANTNUMBTX2];
            ftChanNulls[FtChan.EQPTTX] = mtChanNulls[MtChan.EQPTTX];
            ftChanNulls[FtChan.EQPTUTX] = mtChanNulls[MtChan.EQPTUTX];
            ftChanNulls[FtChan.PWRTX] = mtChanNulls[MtChan.PWRTX];
            ftChanNulls[FtChan.ATPCCDE] = mtChanNulls[MtChan.ATPCCDE];
            ftChanNulls[FtChan.AFSLTX1] = mtChanNulls[MtChan.AFSLTX1];
            ftChanNulls[FtChan.AFSLTX2] = mtChanNulls[MtChan.AFSLTX2];
            ftChanNulls[FtChan.TRAFTX] = mtChanNulls[MtChan.TRAFTX];
            ftChanNulls[FtChan.SRVCTX] = mtChanNulls[MtChan.SRVCTX];
            ftChanNulls[FtChan.STATTX] = mtChanNulls[MtChan.STATTX];
            ftChanNulls[FtChan.FREQRX] = mtChanNulls[MtChan.FREQRX];
            ftChanNulls[FtChan.POLRX] = mtChanNulls[MtChan.POLRX];
            ftChanNulls[FtChan.ANTNUMBRX1] = mtChanNulls[MtChan.ANTNUMBRX1];
            ftChanNulls[FtChan.ANTNUMBRX2] = mtChanNulls[MtChan.ANTNUMBRX2];
            ftChanNulls[FtChan.ANTNUMBRX3] = mtChanNulls[MtChan.ANTNUMBRX3];
            ftChanNulls[FtChan.EQPTRX] = mtChanNulls[MtChan.EQPTRX];
            ftChanNulls[FtChan.EQPTURX] = mtChanNulls[MtChan.EQPTURX];
            ftChanNulls[FtChan.AFSLRX1] = mtChanNulls[MtChan.AFSLRX1];
            ftChanNulls[FtChan.AFSLRX2] = mtChanNulls[MtChan.AFSLRX2];
            ftChanNulls[FtChan.AFSLRX3] = mtChanNulls[MtChan.AFSLRX3];
            ftChanNulls[FtChan.PWRRX1] = mtChanNulls[MtChan.PWRRX1];
            ftChanNulls[FtChan.PWRRX2] = mtChanNulls[MtChan.PWRRX2];
            ftChanNulls[FtChan.PWRRX3] = mtChanNulls[MtChan.PWRRX3];
            ftChanNulls[FtChan.TRAFRX] = mtChanNulls[MtChan.TRAFRX];
            ftChanNulls[FtChan.ESINT] = mtChanNulls[MtChan.ESINT];
            ftChanNulls[FtChan.TSINT] = mtChanNulls[MtChan.TSINT];
            ftChanNulls[FtChan.SRVCRX] = mtChanNulls[MtChan.SRVCRX];
            ftChanNulls[FtChan.STATRX] = mtChanNulls[MtChan.STATRX];
            ftChanNulls[FtChan.ROUTNUMB] = mtChanNulls[MtChan.ROUTNUMB];
            ftChanNulls[FtChan.STNNUMB] = mtChanNulls[MtChan.STNNUMB];
            ftChanNulls[FtChan.HOPNUMB] = mtChanNulls[MtChan.HOPNUMB];
            ftChanNulls[FtChan.SDATE] = mtChanNulls[MtChan.SDATE];
            ftChanNulls[FtChan.NOTETX] = mtChanNulls[MtChan.NOTETX];
            ftChanNulls[FtChan.NOTERX] = mtChanNulls[MtChan.NOTERX];
            ftChanNulls[FtChan.NOTEGNL] = mtChanNulls[MtChan.NOTEGNL];
            ftChanNulls[FtChan.CPOINT] = mtChanNulls[MtChan.CPOINT];
            ftChanNulls[FtChan.FEETX] = mtChanNulls[MtChan.FEETX];
            ftChanNulls[FtChan.FEERX] = mtChanNulls[MtChan.FEERX];
            ftChanNulls[FtChan.MDATE] = mtChanNulls[MtChan.MDATE];
            ftChanNulls[FtChan.MTIME] = mtChanNulls[MtChan.MTIME];

            return 0;
        }

        /// <summary>
        /// Read in a channel from the database. This is out of band. It is implemented 
        /// for the conversion of tsip because I haven't the time to fix the logic in 
        /// the program. GJS - 1283 - 2010. 01. 13.  
        /// </summary>
        /// <param name="cCall1"></param>
        /// <param name="cCall2"></param>
        /// <param name="bndcde"></param>
        /// <param name="chid"></param>
        /// <param name="pChan"></param>
        /// <param name="pChanNull"></param>
        /// <returns></returns>
        public static int MtReadChan(string cCall1,
                                        string cCall2,
                                        string bndcde,
                                        string chid,
                                        out MtChan pChan,
                                        out SQLLEN[] pChanNull)
        {
            // 'out' requirements.
            pChan = null;
            pChanNull = null;

            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            SQLHDBC hConnection = Ssutil.NewConn();

            string cSQLBuff;
            int nRet = 0;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConnection, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nMtUtils.MtReadChan(): ERROR: call to SQLAllocHandle() failed.");
                Ssutil.DbGetDiagStmt(hStmt, "mtReadChan - Could not allocate handle for channel");
                Ssutil.DisConn(hConnection);
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            /*  Allocated array of channels, now read them in using the
                channel structure. */
            cSQLBuff = String.Format("SELECT call1,call2,bndcde,splan,hl,vh,chid,freqtx,poltx,antnumbtx1,antnumbtx2,eqpttx,eqptutx,pwrtx,atpccde,afsltx1,afsltx2,traftx,srvctx,stattx,freqrx,polrx,antnumbrx1,antnumbrx2,antnumbrx3,eqptrx,eqpturx,afslrx1,afslrx2,afslrx3,pwrrx1,pwrrx2,pwrrx3,trafrx,esint,tsint,srvcrx,statrx,routnumb,stnnumb,hopnumb,sdate,notetx,noterx,notegnl,cpoint,feetx,feerx,mdate,mtime,userid FROM  main.mt_chan WHERE call1 = '{0}' and call2='{1}' and bndcde='{2}' and chid='{3}' ",
                cCall1, cCall2, bndcde, chid);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQLBuff, cSQLBuff.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nMtUtils.MtReadChan(): ERROR: call to SQLExecDirect() failed.");
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, hStmt);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConnection);
                nRet = Error.ODBC_EXECDIRECT_FAILED;
            }

            sqlRet = ODBC.SQLFetch(hStmt);

            if (ODBC.IsNoData(sqlRet))
            {
                //	End of data.
                nRet = Constant.NOMORERECS;
            }
            else if (!ODBC.IsOK(sqlRet))
            {
                // Something unexpected just happened.
                Log2.e("\nMtUtils.MtReadChan(): ERROR: call to SQLFetch() failed.");
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, hStmt);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConnection);
                nRet = Error.ODBC_FETCH_FAILED;
            }
            else
            {
                /*  Got the channel.  move it into the array */
                /* Retrieve the data for the channel */
                pChan = new MtChan();
                pChanNull = NullHelper.CreateArrayOfNullInd(MtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                try
                {

                    Ssutil.DbGetString(hStmt, 1, "call1", out pChan.call1, MtChan.CALL1_SZ, out pChanNull[MtChan.CALL1]);
                    Ssutil.DbGetString(hStmt, 2, "call2", out pChan.call2, MtChan.CALL2_SZ, out pChanNull[MtChan.CALL2]);
                    Ssutil.DbGetString(hStmt, 3, "bndcde", out pChan.bndcde, MtChan.BNDCDE_SZ, out pChanNull[MtChan.BNDCDE]);
                    Ssutil.DbGetString(hStmt, 4, "splan", out pChan.splan, MtChan.SPLAN_SZ, out pChanNull[MtChan.SPLAN]);
                    Ssutil.DbGetShort(hStmt, 5, "hl", out pChan.hl, out pChanNull[MtChan.HL]);
                    Ssutil.DbGetShort(hStmt, 6, "vh", out pChan.vh, out pChanNull[MtChan.VH]);
                    Ssutil.DbGetString(hStmt, 7, "chid", out pChan.chid, MtChan.CHID_SZ, out pChanNull[MtChan.CHID]);
                    Ssutil.DbGetDouble(hStmt, 8, "freqtx", out pChan.freqtx, out pChanNull[MtChan.FREQTX]);
                    Ssutil.DbGetString(hStmt, 9, "poltx", out pChan.poltx, MtChan.POLTX_SZ, out pChanNull[MtChan.POLTX]);
                    Ssutil.DbGetShort(hStmt, 10, "antnumbtx1", out pChan.antnumbtx1, out pChanNull[MtChan.ANTNUMBTX1]);
                    Ssutil.DbGetShort(hStmt, 11, "antnumbtx2", out pChan.antnumbtx2, out pChanNull[MtChan.ANTNUMBTX2]);
                    Ssutil.DbGetString(hStmt, 12, "eqpttx", out pChan.eqpttx, MtChan.EQPTTX_SZ, out pChanNull[MtChan.EQPTTX]);
                    Ssutil.DbGetString(hStmt, 13, "eqptutx", out pChan.eqptutx, MtChan.EQPTUTX_SZ, out pChanNull[MtChan.EQPTUTX]);
                    Ssutil.DbGetFloat(hStmt, 14, "pwrtx", out pChan.pwrtx, out pChanNull[MtChan.PWRTX]);
                    Ssutil.DbGetFloat(hStmt, 15, "atpccde", out pChan.atpccde, out pChanNull[MtChan.ATPCCDE]);
                    Ssutil.DbGetFloat(hStmt, 16, "afsltx1", out pChan.afsltx1, out pChanNull[MtChan.AFSLTX1]);
                    Ssutil.DbGetFloat(hStmt, 17, "afsltx2", out pChan.afsltx2, out pChanNull[MtChan.AFSLTX2]);
                    Ssutil.DbGetString(hStmt, 18, "traftx", out pChan.traftx, MtChan.TRAFTX_SZ, out pChanNull[MtChan.TRAFTX]);
                    Ssutil.DbGetString(hStmt, 19, "srvctx", out pChan.srvctx, MtChan.SRVCTX_SZ, out pChanNull[MtChan.SRVCTX]);
                    Ssutil.DbGetString(hStmt, 20, "stattx", out pChan.stattx, MtChan.STATTX_SZ, out pChanNull[MtChan.STATTX]);
                    Ssutil.DbGetDouble(hStmt, 21, "freqrx", out pChan.freqrx, out pChanNull[MtChan.FREQRX]);
                    Ssutil.DbGetString(hStmt, 22, "polrx", out pChan.polrx, MtChan.POLRX_SZ, out pChanNull[MtChan.POLRX]);
                    Ssutil.DbGetShort(hStmt, 23, "antnumbrx1", out pChan.antnumbrx1, out pChanNull[MtChan.ANTNUMBRX1]);
                    Ssutil.DbGetShort(hStmt, 24, "antnumbrx2", out pChan.antnumbrx2, out pChanNull[MtChan.ANTNUMBRX2]);
                    Ssutil.DbGetShort(hStmt, 25, "antnumbrx3", out pChan.antnumbrx3, out pChanNull[MtChan.ANTNUMBRX3]);
                    Ssutil.DbGetString(hStmt, 26, "eqptrx", out pChan.eqptrx, MtChan.EQPTRX_SZ, out pChanNull[MtChan.EQPTRX]);
                    Ssutil.DbGetString(hStmt, 27, "eqpturx", out pChan.eqpturx, MtChan.EQPTURX_SZ, out pChanNull[MtChan.EQPTURX]);
                    Ssutil.DbGetFloat(hStmt, 28, "afslrx1", out pChan.afslrx1, out pChanNull[MtChan.AFSLRX1]);
                    Ssutil.DbGetFloat(hStmt, 29, "afslrx2", out pChan.afslrx2, out pChanNull[MtChan.AFSLRX2]);
                    Ssutil.DbGetFloat(hStmt, 30, "afslrx3", out pChan.afslrx3, out pChanNull[MtChan.AFSLRX3]);
                    Ssutil.DbGetFloat(hStmt, 31, "pwrrx1", out pChan.pwrrx1, out pChanNull[MtChan.PWRRX1]);
                    Ssutil.DbGetFloat(hStmt, 32, "pwrrx2", out pChan.pwrrx2, out pChanNull[MtChan.PWRRX2]);
                    Ssutil.DbGetFloat(hStmt, 33, "pwrrx3", out pChan.pwrrx3, out pChanNull[MtChan.PWRRX3]);
                    Ssutil.DbGetString(hStmt, 34, "trafrx", out pChan.trafrx, MtChan.TRAFRX_SZ, out pChanNull[MtChan.TRAFRX]);
                    Ssutil.DbGetFloat(hStmt, 35, "esint", out pChan.esint, out pChanNull[MtChan.ESINT]);
                    Ssutil.DbGetFloat(hStmt, 36, "tsint", out pChan.tsint, out pChanNull[MtChan.TSINT]);
                    Ssutil.DbGetString(hStmt, 37, "srvcrx", out pChan.srvcrx, MtChan.SRVCRX_SZ, out pChanNull[MtChan.SRVCRX]);
                    Ssutil.DbGetString(hStmt, 38, "statrx", out pChan.statrx, MtChan.STATRX_SZ, out pChanNull[MtChan.STATRX]);
                    Ssutil.DbGetString(hStmt, 39, "routnumb", out pChan.routnumb, MtChan.ROUTNUMB_SZ, out pChanNull[MtChan.ROUTNUMB]);
                    Ssutil.DbGetShort(hStmt, 40, "stnnumb", out pChan.stnnumb, out pChanNull[MtChan.STNNUMB]);
                    Ssutil.DbGetShort(hStmt, 41, "hopnumb", out pChan.hopnumb, out pChanNull[MtChan.HOPNUMB]);
                    Ssutil.DbGetString(hStmt, 42, "sdate", out pChan.sdate, MtChan.SDATE_SZ, out pChanNull[MtChan.SDATE]);
                    Ssutil.DbGetString(hStmt, 43, "notetx", out pChan.notetx, MtChan.NOTETX_SZ, out pChanNull[MtChan.NOTETX]);
                    Ssutil.DbGetString(hStmt, 44, "noterx", out pChan.noterx, MtChan.NOTERX_SZ, out pChanNull[MtChan.NOTERX]);
                    Ssutil.DbGetString(hStmt, 45, "notegnl", out pChan.notegnl, MtChan.NOTEGNL_SZ, out pChanNull[MtChan.NOTEGNL]);
                    Ssutil.DbGetString(hStmt, 46, "cpoint", out pChan.cpoint, MtChan.CPOINT_SZ, out pChanNull[MtChan.CPOINT]);
                    Ssutil.DbGetString(hStmt, 47, "feetx", out pChan.feetx, MtChan.FEETX_SZ, out pChanNull[MtChan.FEETX]);
                    Ssutil.DbGetString(hStmt, 48, "feerx", out pChan.feerx, MtChan.FEERX_SZ, out pChanNull[MtChan.FEERX]);
                    Ssutil.DbGetString(hStmt, 49, "mdate", out pChan.mdate, MtChan.MDATE_SZ, out pChanNull[MtChan.MDATE]);
                    Ssutil.DbGetString(hStmt, 50, "mtime", out pChan.mtime, MtChan.MTIME_SZ, out pChanNull[MtChan.MTIME]);
                    Ssutil.DbGetString(hStmt, 51, "userid", out pChan.userid, MtChan.USERID_SZ, out pChanNull[MtChan.USERID]);

                }
                catch (Exception e)
                {
                    Log2.e("\nMtUtils.MtReadChan(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                    GenUtil.SetError(1012, "Could not retrieve channel field: " + e.Message);
                    nRet = Error.ODBC_GET_FAILED;
                }
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConnection);

            return nRet;
        }

        /// <summary>
        /// This method returns a list of strings comprising all the TS call signs found
        /// in the MDB table <b>main.mt_site</b> that conform with the prescribed 'WHERE'
        /// and 'ORDER BY' clauses for a SQL SELECT query.
        /// </summary>
        /// <param name="whereClause"></param>
        /// <param name="orderByClause"></param>
        /// <param name="allCallSigns"></param>
        /// <returns></returns>
        public static int GetTsCallSigns(string whereClause, string orderByClause, out List<string> allCallSigns)
        {
            //...Log2.v("\n\nMtUtils.GetListAllMtCallSigns(): Entry");

            // 'out' requirement.
            allCallSigns = new List<string>();

            int nRet = Constant.SUCCESS;

            if (!String.IsNullOrWhiteSpace(whereClause)) whereClause = " WHERE " + whereClause;
            if (!String.IsNullOrWhiteSpace(orderByClause)) orderByClause = " ORDER BY " + orderByClause;

            string sqlQuery = String.Format("SELECT call1 FROM {0} {1} {2}", DynMdbSite.TableName, whereClause, orderByClause);

            SQLHDBC hConn = Ssutil.NewConn();
            SQLHSTMT mhStmt;

            SQLRETURN sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out mhStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nMtUtils.GetAllTsCallSigns(): ERROR : Exit: call to SQLAllocHandle() failed, sqlRet = " + sqlRet);
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            sqlRet = ODBC.SQLExecDirect(mhStmt, sqlQuery, sqlQuery.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nMtUtils.GetAllTsCallSigns(): ERROR : Exit: call to SQLExecDirect() failed, sqlQuery = \n" + sqlQuery);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            // Do the fetches.
            while((sqlRet = ODBC.SQLFetch(mhStmt)) == Constant.SUCCESS) {

                string call1;
                SQLLEN nullInd;

                try
                {
                    Ssutil.DbGetString(mhStmt, 1, "call1", out call1, MtAnte.CALL1_SZ, out nullInd);

                    allCallSigns.Add(call1);
                }
                catch (Exception e)
                {
                    Log2.e("\nMtUtils.GetAllTsCallSigns(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                    nRet = Error.ODBC_GET_FAILED;
                }
            }

            if (sqlRet != ODBC.SQL_NO_DATA)
            {
                Log2.e("\r\nMtUtils.GetAllTsCallSigns(): ERROR: fetch an extant site from the MDB");

                nRet = Error.ODBC_FETCH_FAILED; 
            }
        
            Ssutil.DisConnStmt(hConn, mhStmt);

            //...Log2.v("\n\nMtUtils.GetAllTsCallSigns(): Exit");
            return (nRet);
        }

    }
}
