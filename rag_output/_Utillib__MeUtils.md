# Documented File: MeUtils.cs
**Repository Path:** `_Utillib\MeUtils.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
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

    public class MeUtils
    {
        /// <summary>
        /// Populates an MeSiteStr object with data fetched from the the DB tables <b>main.me_site</b>,
        /// <b>main.me_ante</b>, <b>main.me_azim</b> and <b>main.me_chan</b>. 
        /// See the remarks below relating to the 'depth' of retrieval.
        /// </summary>
        /// <remarks>
        /// If nDepth = 1 only the site will be returned, 
        /// If nDepth = 2 the site and antennas will be returned,
        /// If nDepth = 3 the site, antennas and channels will be returned.
        /// If nDepth = 4 the site, antennas, channels and azimuths will be returned.
        /// </remarks>
        /// <param name="cLocation"> - call sign of the SITE to be retrieved.</param>
        /// <param name="meSiteStr"> - a MeSiteStr_NEW object populated with data.</param>
        /// <param name="nDepth"> - 1, 2 or 3 (see remarks above).</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - call succeeded in retrieving data from the DB.</para>
        /// <para>- Constant.FAILURE - invalid number of sites, antennae or channels.</para>
        /// <para>- ErrorMessages.INVALID_DATA - either cCall or nDepth is invalid.</para>
        /// <para>- ErrorMessages.ODBC_SQLALLOCHANDLE_FAILED - ODBC SQLAllocHandle() failed.</para>
        /// <para>- ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int MeGetSite(string cLocation, out MeSiteStr meSiteStr, int nDepth)
        {
            MeSiteStrNulls dummy;

            return MeGetSiteWN(cLocation, out meSiteStr, out dummy, nDepth);
        }


        /// <summary>
        /// Populates an MeSiteStr object with data fetched from the the DB tables <b>main.me_site</b>,
        /// <b>main.me_ante</b>, <b>main.me_azim</b> and <b>main.me_chan</b>together with their associated nullInds. 
        /// See the remarks below relating to the 'depth' of retrieval.
        /// </summary>
        /// <remarks>
        /// If nDepth = 1 only the site will be returned, 
        /// If nDepth = 2 the site and antennas will be returned,
        /// If nDepth = 3 the site, antennas and channels will be returned.
        /// If nDepth = 4 the site, antennas, channels and azimuths will be returned.
        /// </remarks>
        /// <param name="cLocation"> - call sign of the SITE to be retrieved.</param>
        /// <param name="meSiteStr"> - a MeSiteStr_NEW object populated with data.</param>
        /// <param name="nDepth"> - 1, 2 or 3 (see remarks above).</param>
        /// <param name="meSiteNull"> - a MeSiteNull_NEW object populated with data.</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - call succeeded in retrieving data from the DB.</para>
        /// <para>- Constant.FAILURE - invalid number of sites, antennae or channels.</para>
        /// <para>- ErrorMessages.INVALID_DATA - either cCall or nDepth is invalid.</para>
        /// <para>- ErrorMessages.ODBC_SQLALLOCHANDLE_FAILED - ODBC SQLAllocHandle() failed.</para>
        /// <para>- ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int MeGetSiteWN(string cLocation, out MeSiteStr meSiteStr, out MeSiteStrNulls meSiteNull, int nDepth)
        {
            //...Log2.v("\n\nMeUtils.MeGetSiteWN(): Entry");

            // Satisfy the 'out' requirements.
            meSiteStr = null;
            meSiteNull = null;

            string cSQLBuff;

            int nRet = Constant.SUCCESS;

            // First, sanity check the input data and return if there is a problem..
            Boolean cCallHasNoContent = String.IsNullOrWhiteSpace(cLocation);
            Boolean nDepthIsNotValid = (nDepth < 1) || (nDepth > 4);   //Therefor nDepth = 1, 2, 3 or 4.
            if (cCallHasNoContent || nDepthIsNotValid)
            {
                Log2.e("\r\nMeUtils.MeGetSiteWN(): ERROR: Invalid cCall or nDepth : " + cLocation + ", " + nDepth);
                nRet = Error.INVALID_DATA;
                return nRet;
            }

            // ============================================
            // Find and fetch the MeSite data from the MDB.
            // ============================================

            string siteWhere = " location = '" + cLocation + "'";

            int nSiteCursor = DynMeSite.MeSelectSite(siteWhere, null);

            // We are assuming that only one MeSite in the MDB has a location column that matches the cCall
            // provided by the caller. We should test for this!
            cSQLBuff = " location='" + cLocation + "' ";
            int nNumSites = Ssutil.DbCountRows("main.me_site", cSQLBuff);

            //...Log2.v("\r\nMeUtils.MeGetSiteWN(): nNumSites = " + nNumSites);

            // Check for failure of DbCountRows() to return valid number of sites.
            if (nNumSites < 0)
            {
                Log2.e("\r\nMeUtils.MeGetSiteWN(): ERROR: Marker F: DbCountRows() returned an invalid value: " + nNumSites);
                string str = String.Format("meGetSiteWN03: Ingres error {0} getting sites.", nNumSites);
                GenUtil.SetErr(str);
                DynMeSite.MeCloseSite(nSiteCursor);
                return (Constant.FAILURE);
            }
            // Handle the pathological case of no sites to read from the MDB.
            else if (nNumSites == 0)
            {
                Log2.e("\r\nMeUtils.MeGetSiteWN(): Marker G: site not found in MDB; cLocation = " + cLocation);
                DynMeSite.MeCloseSite(nSiteCursor);
                return (Constant.FAILURE);
            }
            // Handle the pathological case of too many sites to read from the MDB.
            else if (nNumSites >= 2)
            {
                Log2.e("\r\nMeUtils.MeGetSiteWN(): ERROR: Marker G-1: multiple sites in MDB with cLocation = " + cLocation);
                DynMeSite.MeCloseSite(nSiteCursor);
                return (Error.INVALID_DATA);
            }
            else // #1. We have now found the site; next get the column data. 

            {
                // First, create and populate the structured data objects to be output.
                // These constructors set appropriate initial values.
                meSiteStr = new MeSiteStr(MeSiteStr.Init.UNALLOCATED);
                meSiteNull = new MeSiteStrNulls(MeSiteStrNulls.Init.UNALLOCATED);

                // Set the depth parameter.
                meSiteStr.nDepth = nDepth;

                MeSite meSite;
                SQLLEN[] nullInds;

                if (DynMeSite.MeFetchSite(nSiteCursor, out meSite, out nullInds) == Constant.SUCCESS)
                {
                    // Save the site and associated column-nulls that we just fetched from the MDB.
                    meSiteStr.stSite = meSite;
                    meSiteNull.anSiteNull = nullInds;
                }
                else
                {
                    Log2.e("\r\nMeUtils.MeGetSiteWN(): ERROR: Marker G-2: failed to fetch an extant site from the MDB; cLocation = " + cLocation);
                    DynMeSite.MeCloseSite(nSiteCursor);
                    return (Constant.FAILURE);
                }

                // Free the cursor object (closes hStmt and hConn).
                DynMeSite.MeCloseSite(nSiteCursor);
            }

            // At this point we have the site read in from the MDB and stored in the site structure.  
            // If nDepth is 1, then this is all that was required and we can exit.  
            if (nDepth == 1)
            {
                //...Log2.v("\n\nMeUtils.MeGetSiteWN(): Exit (nDepth == 1)");
                return Constant.SUCCESS;
            }
            // End of fetching the site data from the MDB.

            // ============================================
            // Find and fetch the MeAnte data from the MDB.
            // ============================================

            // At this point, nDepth is 2, 3 or 4, so we need to fetch and store (at least) the antennae data from the MDB.
            MeAnte[] meAntes = null;
            SQLLEN[][] meAntesNulls = null;  // Usage: [antenna, column]
            int nNumAnts;

            // First find out how many antennae there are in order to allocate the array.
            //sprintf_s(cSQLBuff, sizeof(cSQLBuff), "call1 = '%s'", cCall);
            cSQLBuff = " location='" + cLocation + "' ";
            nNumAnts = Ssutil.DbCountRows("main.me_ante", cSQLBuff);

            //...Log2.v("\r\nMeUtils.MeGetSiteWN(): nNumAnts = " + nNumAnts);

            // Check for failure of DbCountRows() to return valid number of antennae.
            if (nNumAnts < 0)
            {
                Log2.e("\r\nMeUtils.MeGetSiteWN(): ERROR: Marker F: DbCountRows() returned an invalid value: " + nNumAnts);
                string str = String.Format("meGetSiteWN03: Ingres error {0} getting antennas.", nNumAnts);
                GenUtil.SetErr(str);
                return (nNumAnts);
            }

            // Handle the case of no antennae to read from the MDB.
            if (nNumAnts == 0)
            {
                Log2.w("\r\nMeUtils.MeGetSiteWN(): WARNING: Marker G: number of antennae to read from MDB is zero.");
            }
            else // #1, nNumAnts > 0
            {
                string anteWhere = " location = '" + cLocation + "'";
                string anteOrderBy = " ";

                int nAnteCursor = DynMeAnte.MeSelectAnte(anteWhere, anteOrderBy);

                // Instantiate the objects required store the antennae data (multiple fetches from the MDB).
                meAntes = Arrays.CreateArrayUsingDefaultElementConstructor<MeAnte>(nNumAnts);
                meAntesNulls = new SQLLEN[nNumAnts][];

                // Now fetch the selected antennae data from the MDB using a loop.
                MeAnte meAnte;
                SQLLEN[] nullInds;
                int nAnte = 0;
                while (DynMeAnte.MeFetchAnte(nAnteCursor, out meAnte, out nullInds) == Constant.SUCCESS)
                {
                    nAnte++;

                    // Sanity-check the number of antennae read from the MDB so far.
                    if (nAnte > nNumAnts)
                    {
                        Log2.e("\r\nMeUtils.MeGetSiteWN(): ERROR: Marker J: SQLFetch(): antennae count exceeds that of DbCountRows()");
                        DynMeAnte.MeCloseAnte(nAnteCursor);
                        GenUtil.SetError(1007, "*ERROR* feGetSiteWN: Antenna counts wrong.");
                        return (-10);
                    }

                    // Accumulate the data over successive fetch-loops.
                    meAntes[nAnte - 1] = meAnte;
                    meAntesNulls[nAnte - 1] = nullInds;

                } //end of while-loop

                // We have now accumulated all the antennae data - 'attach' it to mtSiteStr.
                meSiteStr.stAntsPtr = meAntes;
                meSiteStr.nNumAnts = nAnte;
                // We have now accumulated all the antennae nullInd data - 'attach' it to mtSiteNull.
                meSiteNull.anAntsNullPtr = meAntesNulls;

                // Free the SQL statement handle and cursor.
                DynMeAnte.MeCloseAnte(nAnteCursor);

            } // else #1, nNumAnts > 0

            // At this point we have the site and antennae read in from the MDB.  
            // If nDepth is 2, then this is all that was required and we can exit.  
            if (nDepth == 2)
            {
                //...Log2.v("\n\nMeUtils.MeGetSiteWN(): Exit (nDepth == 2)");
                return Constant.SUCCESS; ;
            }
            // End of fetching the antennae data from the MDB. -------------------------------------------


            // ============================================
            // Find and fetch the MeChan data from the MDB.
            // ============================================

            // We have now loaded the site and antennae data.  
            // If nDepth = 3 or 4, we need to fetch the channel data from the MDB.

            // Use these two objects to accumulate the channel data fetched from the MDB.
            MeChan[] meChans = null;
            SQLLEN[][] meChansNulls = null;

            // Determine how many channels there are in order to allocate the array.
            cSQLBuff = "location='" + cLocation + "'";
            int nNumChan = Ssutil.DbCountRows("main.me_chan", cSQLBuff);

            //...Log2.v("\r\nMeUtils.MeGetSiteWN(): nNumChan = " + nNumChan);

            if (nNumChan < 0)
            {
                // An error occurred - deal with it.
                Log2.e("\r\nMeUtils.MeGetSiteWN(): ERROR: Marker M: DbCountRows() returned nNumChan = " + nNumChan);
                string str = String.Format("mtGetChanWN03: Ingres error {0} getting channels.", nNumChan);
                GenUtil.SetErr(str);
                return -24;
            }

            // Handle the case of no channels to read (i.e. nNumChan == 0).
            else if (nNumChan == 0)
            {
                Log2.w("\r\nMeUtils.MeGetSiteWN(): WARNING: Marker N: number of channels to read from MDB is zero.");
            }

            // Handle the 'regular' case of nNumChan > 0.
            else  // #2, nNumChan > 0
            {
                // Accumulate the channel and nullInd data in these two objects.
                meChans = Arrays.CreateArrayUsingDefaultElementConstructor<MeChan>(nNumChan);
                meChansNulls = new SQLLEN[nNumChan][];

                string chanWhere = " location = '" + cLocation + "'";
                string chanOrderBy = " call1, chid";

                int nChanCursor = DynMeChan.MeSelectChan(chanWhere, chanOrderBy);

                // Start a loop to fetch all the selected channel data from the MDB.
                int nChan = 0;  //This counts the number of successful channel fetches so far; it is auto-incremented by FtFetchChannel().
                MeChan meChan;
                SQLLEN[] nullInds;

                while (DynMeChan.MeFetchChan(nChanCursor, out meChan, out nullInds) == Constant.SUCCESS)
                {
                    nChan++;

                    // Sanity-check the number of channel records read from the MDB so far.
                    if (nChan > nNumChan)
                    {
                        Log2.e("\r\nMeUtils.MeGetSiteWN(): ERROR: Marker R: SQLFetch(): channel count exceeds that of DbCountRows()");
                        DynMeChan.MeCloseChan(nChanCursor);
                        GenUtil.SetError(1009, "mtGetSiteWN27: Channel counts wrong.");
                        return (-27);
                    }

                    //Accumulate the channel data over successive fetch-loops.
                    meChans[nChan - 1] = meChan;
                    meChansNulls[nChan - 1] = nullInds;

                } // end of while fetch-loop.

                // We have now accumulated all the channel data - 'attach' it to mtSiteStr.
                meSiteStr.stChanPtr = meChans;
                meSiteStr.nNumChans = nChan;
                // We have now accumulated all the channel nullInd data - 'attach' it to mtSiteNull.
                meSiteNull.anChanNullPtr = meChansNulls;

                // Release resources.
                DynMeChan.MeCloseChan(nChanCursor);

            } // #2, if nNumChan > 0

            // At this point we have the site, antennae and channel data read in from the MDB.  
            // If nDepth is 3, then this is all that was required and we can exit.  
            if (nDepth == 3)
            {
                //...Log2.v("\n\nMeUtils.MeGetSiteWN(): Exit (nDepth == 3)");
                return Constant.SUCCESS; ;
            }
            // End of fetching the channel data from the MDB. -------------------------------------------

            // ============================================
            // Find and fetch the MeAzim data from the MDB.
            // ============================================

            // We have now loaded the site, antennae and channel data.  
            // We now need to fetch the azimuth data from the MDB.

            // Use these two objects to accumulate the azimuth data fetched from the MDB.
            MeAzim[] meAzims = null;
            SQLLEN[][] meAzimsNulls = null;

            // Determine how many azimuths there are in order to allocate the array.
            cSQLBuff = "location='" + cLocation + "'";
            int nNumAzim = Ssutil.DbCountRows("main.me_azim", cSQLBuff);

            //...Log2.v("\r\nMeUtils.MeGetSiteWN(): nNumAzim = " + nNumAzim);

            if (nNumAzim < 0)
            {
                // An error occurred - deal with it.
                Log2.e("\r\nMeUtils.MeGetSiteWN(): ERROR: Marker M: DbCountRows() returned nNumAzim = " + nNumAzim);
                string str = String.Format("meGetAzimWN03: Ingres error {0} getting azimuths.", nNumAzim);
                GenUtil.SetErr(str);
                return -24;
            }

            // Handle the case of no azimuths to read (i.e. nNumAzim == 0).
            else if (nNumAzim == 0)
            {
                Log2.w("\r\nMeUtils.MeGetSiteWN(): WARNING: Marker N: number of azimuths to read from MDB is zero.");
            }

            // Handle the 'regular' case of nNumAzim > 0.
            else  // #2, nNumAzim > 0
            {
                // Accumulate the azimuth and nullInd data in these two objects.
                meAzims = Arrays.CreateArrayUsingDefaultElementConstructor<MeAzim>(nNumAzim);
                meAzimsNulls = new SQLLEN[nNumAzim][];

                string azimWhere = " location = '" + cLocation + "'";
                string azimOrderBy = " call1, azim";

                int nAzimCursor = DynMeAzim.MeSelectAzim(azimWhere, azimOrderBy);

                // Start a loop to fetch all the selected azimuth data from the MDB.
                int nAzim = 0;  //This counts the number of successful azimuth fetches so far.
                MeAzim meAzim;
                SQLLEN[] nullInd;

                while (DynMeAzim.MeFetchAzim(nAzimCursor, out meAzim, out nullInd) == Constant.SUCCESS)
                {
                    nAzim++;

                    // Sanity-check the number of azimuth records read from the MDB so far.
                    if (nAzim > nNumAzim)
                    {
                        Log2.e("\r\nMeUtils.MeGetSiteWN(): ERROR: Marker R: SQLFetch(): azimuth count exceeds that of DbCountRows()");
                        DynMeAzim.MeCloseAzim(nAzimCursor);
                        GenUtil.SetError(1009, "meGetSiteWN27: azimuth counts wrong.");
                        return (-27);
                    }

                    //Accumulate the azimuth data over successive fetch-loops.
                    meAzims[nAzim - 1] = meAzim;
                    meAzimsNulls[nAzim - 1] = nullInd;

                } // end of while fetch-loop.

                // We have now accumulated all the azimuth data - 'attach' it to meSiteStr.
                meSiteStr.stAzimPtr = meAzims;
                meSiteStr.nNumAzim = nAzim;
                // We have now accumulated all the azimuth nullInd data - 'attach' it to meSiteNull.
                meSiteNull.anAzimNullPtr = meAzimsNulls;

                // Release resources.
                DynMeAzim.MeCloseAzim(nAzimCursor);

            }

            // End of fetching the azimuth data from the MDB. -------------------------------------------

            // At this point we have the site, antennae, channel and azimuth data read in from the MDB.  

            //...Log2.v("\n\nMeUtils.MeGetSiteWN(): Exit, nDepth == 4");
            return (nRet);
        }  //end of method

        /// <summary>
        /// This method returns the location, location name, province and operator
        /// of an ES site with the given call sign; if not found the arguments are unchanged.
        /// </summary>
        /// <param name="cCallSign"></param>
        /// <param name="cLocation"></param>
        /// <param name="cLocName"></param>
        /// <param name="cLocProv"></param>
        /// <param name="cLocOper"></param>
        /// <returns> the return code will be 0 if found, and 1 if not.</returns>
        public static int MeGetLocByCall(string cCallSign,                  /*	The call sign of interest */
                                            out string cLocation,           /*	Output location */
                                            out string cLocName,            /*	Output Location name */
                                            out string cLocProv,            /*	Output Location province */
                                            out string cLocOper)            /*	Output Location Operator */
        {
            // 'out' requirements.
            cLocation = "";
            cLocName = "";
            cLocProv = "";
            cLocOper = "";

            int nRet;
            int nHandle;
            string cWhere;
            MeAnte tAnte;
            SQLLEN[] nullInd;
            MeSite tSite;
            SQLLEN[] siteNull;

            cWhere = String.Format(" call1='{0}' ", cCallSign);

            nHandle = DynMeAnte.MeSelectAnte(cWhere, null);

            nRet = DynMeAnte.MeFetchAnte(nHandle, out tAnte, out nullInd);

            DynMeAnte.MeCloseAnte(nHandle);

            if (nRet != 0)
            {
                /*	Not found */
                nRet = 1;
            }
            else
            {
                /*	Found one */
                cWhere = String.Format(" location='{0}' ", tAnte.location);

                nHandle = DynMeSite.MeSelectSite(cWhere, null);

                nRet = DynMeSite.MeFetchSite(nHandle, out tSite, out siteNull);

                DynMeSite.MeCloseSite(nHandle);

                if (nRet == 0)
                {
                    cLocation = tSite.location;
                    cLocName = tSite.name;
                    cLocProv = tSite.prov;
                    cLocOper = tSite.oper;
                    nRet = 0;
                }
                else
                {
                    nRet = 1;
                }
            }

            return nRet;
        }






    }
}

```
