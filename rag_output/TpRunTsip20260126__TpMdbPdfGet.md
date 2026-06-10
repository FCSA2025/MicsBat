# Documented File: TpMdbPdfGet.cs
**Repository Path:** `TpRunTsip20260126\TpMdbPdfGet.cs`
**Primary Layer:** `TpRunTsip20260126`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TpRunTsip
{
    using _Configuration;
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
    /// Provides methods that select, fetch, copy and insert records to/from
    /// the main database tables.
    /// </summary>
    public class TpMdbPdfGet
    {
#if PINVOKE
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static void utGetInterferenceGroups([In] string runname, [In] string siteName, [In] string anteName, [In, Out] ref int TsEsStnGroups, [In, Out] ref int EsTsStnGroups);

        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int ttChanGet([In] string cCall1,
                            [In] string cCall2,
                            [In] string bndcde,
                            [In] string chid,
                            [In] string tabName,
                            [In] short isMDB,
                            [In, Out] FtChan chanStruct,
                            [In, Out] SQLLEN[] chanNulls);
        public static void UtGetInterferenceGroups_NATIVE(string runname,
                                                             string siteName,
                                                             string anteName,
                                                             out int TsEsStnGroups,
                                                             out int EsTsStnGroups)
        {
            //...Log2.v("\nTpMdbPdfGet.UtGetInterferenceGroups_NATIVE(): Entry");

            // out.
            TsEsStnGroups = 0;
            EsTsStnGroups = 0;

            // Native call.
            utGetInterferenceGroups(runname, siteName, anteName, ref TsEsStnGroups, ref EsTsStnGroups);

            //...Log2.v("\nTpMdbPdfGet.UtGetInterferenceGroups_NATIVE(): Exit");
        }

        public static int TtChanGet_NATIVE(string cCall1,
                            string cCall2,
                            string bndcde,
                            string chid,
                            string tabName,
                            bool isMDB,
                            out FtChan chanStruct,
                            out SQLLEN[] chanNulls)
        {
            // 'out' requirements;
            chanStruct = new FtChan();
            chanNulls = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            short isMDB_short = Constant.FALSE;
            if (isMDB)
            {
                isMDB_short = Constant.TRUE;
            }

            int retVal = ttChanGet(cCall1, cCall2, bndcde, chid, tabName, isMDB_short, chanStruct, chanNulls);

            return retVal;
        }
#endif

        /// <summary>
        /// This method selects TS Ante data, that matches the seleciton criteria, 
        /// from the appropriate TS table, based on the TSIP proposed and environment 
        /// files specified. It returns a handle (index) to a cursor object that can
        /// be used in subsequent calls to FetchTerrAnte().
        /// </summary>
        /// <param name="tsAnteTableName"> - eponym.</param>
        /// <param name="selection"> - SQL 'WHERE' clause.</param>
        /// <param name="envType"> - if envType == "MDB_TS" then the MDB is used as the environment.</param>
        /// <returns></returns>
        public static int SelectTerrAnte(string tsAnteTableName,
                                             string selection,
                                             string envType)
        {
            int anteHandle;
            string selectClause;

            if (envType.Equals("MDB_TS"))
            {
                /* MDB is used as environment */
                anteHandle = DynMdbAntenna.MtSelectAntenna(selection, "");
            }
            else
            {
                /* a proposed or an environment pdf is used */
                /* exclude deleted records */
                selection.Trim();
                if (String.IsNullOrWhiteSpace(selection))
                {
                    selectClause = "cmd != 'D'";
                }
                else
                {
                    selectClause = String.Format("{0} and cmd != 'D'", selection);
                }

                anteHandle = DynAntenna.FtSelectAntenna(tsAnteTableName, selectClause, "");
            }
            return (anteHandle);
        }

        /// <summary>
        /// This method populates an FtAnte object by fetching TS Ante data from the DB, 
        /// using a cursor previously prepared by SelectTerrAnte(). 
        /// </summary>
        /// <param name="anteHandle"> - handle (index) of cursor returned by SelectTerrAnte().</param>
        /// <param name="ftAnte"> - FtAnte object to be populated.</param>
        /// <param name="ftAnteNullInds"> - ODBC nullInds associated with ftAnte.</param>
        /// <param name="envType"> - if envType == "MDB_TS" then the MDB is used as the environment.</param>
        /// <returns></returns>
        public static int FetchTerrAnte(int anteHandle,
                                    out FtAnte ftAnte,
                                    out SQLLEN[] ftAnteNullInds,
                                    string envType)
        {
            // 'out' requirement.
            ftAnte = null;
            ftAnteNullInds = null;

            int rc;

            if (envType.Equals("MDB_TS"))
            {
                SQLLEN[] mdbAnteNulls;
                MtAnte mdbAnteStruct;

                /* the mdb is used as environment */
                if ((rc = DynMdbAntenna.MtFetchAntenna(anteHandle, out mdbAnteStruct, out mdbAnteNulls)) == Constant.SUCCESS)
                {
                    ftAnte = new FtAnte();
                    ftAnteNullInds = NullHelper.CreateArrayOfNullInd(FtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                    /* copy mdb Structure into the pdf structure */
                    FtValCopy.FtCopyAnte(ref ftAnte, mdbAnteStruct, ref ftAnteNullInds, mdbAnteNulls);
                }

            }
            else
            {
                /* a pdf (environment or proposed) is used */
                rc = DynAntenna.FtFetchAntenna(anteHandle, out ftAnte, out ftAnteNullInds);
            }
            return (rc);
        }

        /// <summary>
        /// This method closes the TS Antenna cursor opened by SelectTerrAnte() and releases
        /// the associated ODBC connection and statement handles.  
        /// </summary>
        /// <param name="anteHandle"> - cursor handle (index) to be closed.</param>
        /// <param name="envType"> - "MDB_TS" or other.</param>
        public static void CloseTerrAnte(int anteHandle, string envType)
        {
            if (envType.Equals("MDB_TS"))
            {
                /* the mdb is used as environment */
                DynMdbAntenna.MtCloseAntenna(anteHandle);
            }
            else
            {
                /* a pdf (environment or proposed) is used */
                DynAntenna.FtCloseAntenna(anteHandle);
            }
        }

        /// <summary>
        /// This method populates a FtSite object with data from the appropriate TS 
        /// table and having the prescribed call sign.  
        /// </summary>
        /// <param name="cCallSign"> - call sign.</param>
        /// <param name="tabName"> - name of table to read from if not an MDB table.</param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        /// <param name="ftSite"> - FtSite object to be populated.</param>
        /// <param name="ftSiteNullInds"> - ODBC nullInds associated with ftSite.</param>
        /// <returns></returns>
        public static int TtSiteGetCall(string cCallSign,
                                    string tabName,         //	Expanded table name for the site.
                                    bool isMDB,
                                    out FtSite ftSite,
                                    out SQLLEN[] ftSiteNullInds)
        {
            // 'out' requirement.
            ftSite = null;
            ftSiteNullInds = null;

            SQLRETURN nRet;
            int nHandle;

            string cSelection;

            if (isMDB == true)
            {
                cSelection = String.Format("call1='{0}'", cCallSign);

                nHandle = DynMdbSite.MtSelectSite(cSelection, null);

                MtSite mtSite;
                SQLLEN[] mtSiteNullInds;

                nRet = (SQLRETURN)DynMdbSite.MtFetchSite(nHandle, out mtSite, out mtSiteNullInds);

                DynMdbSite.MtCloseSite(nHandle);

                if (ODBC.IsOK(nRet))
                {
                    ftSite = new FtSite();
                    ftSiteNullInds = NullHelper.CreateArrayOfNullInd(FtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                    /* copy mdb info into pdf structure */
                    FtValCopy.FtCopySite(ref ftSite, mtSite, ref ftSiteNullInds, mtSiteNullInds);
                }
                DynMdbSite.MtCloseSite(nHandle);
            }
            else
            {
                cSelection = String.Format("call1='{0}' and cmd != 'D' ", cCallSign);

                nHandle = DynSite.FtSelectSite(tabName, cSelection, null);

                nRet = (SQLRETURN)DynSite.FtFetchSite(nHandle, out ftSite, out ftSiteNullInds);

                DynSite.FtCloseSite(nHandle);
            }

            return (nRet);
        }

        /// <summary>
        /// This method populates an FeSite object with data fetched from the appropriate 
        /// DB ES table that matches the prescribed selection criteria.  
        /// </summary>
        /// <param name="location"> - prescribed location field/column value for selection.</param>
        /// <param name="tabName"> - name of table if not a main table.</param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        /// <param name="feSite"> - FeSite object to be populated.</param>
        /// <param name="feSiteNullInds"> - ODBC nullInds associated with feSite.</param>
        /// <returns></returns>
        public static int TeSiteGetLoc(string location,
                                    string tabName,
                                    bool isMDB,
                                    out FeSite feSite,
                                    out SQLLEN[] feSiteNullInds)
        {
            // 'out' requirement
            feSite = null;
            feSiteNullInds = null;

            SQLLEN[] meNulls;  //[ME_SITE_SIZE_];
            MeSite meSite;
            string sqlCommand;

            int nCursor;
            int nRet;

            if (isMDB)
            {
                sqlCommand = String.Format("location='{0}'", location);

                nCursor = DynMeSite.MeSelectSite(sqlCommand, "");

                nRet = DynMeSite.MeFetchSite(nCursor, out meSite, out meNulls);

                if (nRet != 0)
                {
                    if (nRet != Constant.NOMORERECS)
                    {
                        return (Error.DYN_MS_SQL_SERVER_ERR);
                    }
                    else
                    {
                        return Constant.FAILURE;
                    }
                }
                else
                {
                    DynMeSite.MeCloseSite(nCursor);

                    /* copy mdb info into pdf structure */
                    // AH: HERE
                    feSite = new FeSite();
                    feSiteNullInds = NullHelper.CreateArrayOfNullInd(FeSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                    FeValCopy.FeCopySite(ref feSite, meSite, ref feSiteNullInds, meNulls);
                }
            }
            else
            {
                sqlCommand = String.Format("location = '{0}' and cmd != 'D'", location);

                nCursor = DynFeSite.FeSelectSite(tabName, sqlCommand, "");

                nRet = DynFeSite.FeFetchSite(nCursor, out feSite, out feSiteNullInds);

                if (nRet != 0)
                {
                    if (nRet == Constant.NOMORERECS)
                    {
                        return (Error.DYN_MS_SQL_SERVER_ERR);
                    }
                    else
                    {
                        Log2.e("\nTpMdbPdfGet.TeSiteGetLoc(): ERROR: FeFetchSite() failed.");
                        return Constant.FAILURE;
                    }
                }

                DynFeSite.FeCloseSite(nCursor);
            }
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method selects ES Ante data, that matches the seleciton criteria, 
        /// from the appropriate ES table, based on the TSIP proposed and environment 
        /// files specified. It returns a handle (index) to a cursor object that can
        /// be used in subsequent calls to FetchEarthAnte().
        /// </summary>
        /// <param name="tsAnteTableName"> - name of table to be used if not a main table.</param>
        /// <param name="selection"> - part of an SQL 'WHERE' clause used for record selection.</param>
        /// <param name="envType"> - "MDB_ES" or other.</param>
        /// <returns></returns>
        public static int SelectEarthAnte(string tsAnteTableName,
                                        string selection,
                                        string envType)
        {
            int anteHandle;
            string selectClause;

            if (envType.Equals("MDB_ES"))
            {
                /* MDB is used as environment */
                anteHandle = DynMeAnte.MeSelectAnte(selection, "");
            }
            else
            {
                /* a proposed or an environment pdf is used */
                /* exclude deleted records */
                selection.Trim();
                if (String.IsNullOrWhiteSpace(selection))
                {
                    selectClause = "cmd != 'D'";
                }
                else
                {
                    selectClause = String.Format("{0} and cmd != 'D'", selection);
                }

                anteHandle = DynFeAnte.FeSelectAnte(tsAnteTableName, selectClause, "");
            }
            return (anteHandle);
        }

        /// <summary>
        /// This method closes the ES Antenna cursor opened by SelectTerrAnte() and releases
        /// the associated ODBC connection and statement handles.
        /// </summary>
        /// <param name="anteHandle"> - handle (index) of handle to be closed.</param>
        /// <param name="envType"> - "MDB_ES" or other.</param>
        public static void CloseEarthAnte(int anteHandle, string envType)
        {
            if (envType.Equals("MDB_ES"))
            {
                /* the mdb is used as environment */
                DynMeAnte.MeCloseAnte(anteHandle);
            }
            else
            {
                /* a pdf (environment or proposed) is used */
                DynFeAnte.FeCloseAnte(anteHandle);
            }
        }

        /// <summary>
        /// This method populates an FeAnte object by fetching ES Ante data from the DB, 
        /// using a cursor previously prepared by SelectEarthAnte().
        /// </summary>
        /// <param name="anteHandle"> - handle (index) of cursor returned by SelectTerrAnte().</param>
        /// <param name="feAnte"> - FeAnte object to be populated.</param>
        /// <param name="feAnteNullInds"> - ODBC nullInds associated with ftAnte.</param>
        /// <param name="envType"> - if envType == "MDB_TS" then the MDB is used as the environment.</param>
        /// <returns></returns>
        public static int FetchEarthAnte(int anteHandle,
                                     out FeAnte feAnte,
                                     out SQLLEN[] feAnteNullInds,
                                     string envType)
        {
            // 'out' requirement.
            feAnte = null;
            feAnteNullInds = null;

            MeAnte mdbAnteStruct;
            SQLLEN[] mdbAnteNulls;  //[ME_ANTE_SIZE_];
            int rc;

            if (envType.Equals("MDB_ES"))
            {
                /* the mdb is used as environment */
                if ((rc = DynMeAnte.MeFetchAnte(anteHandle, out mdbAnteStruct, out mdbAnteNulls)) == Constant.SUCCESS)
                {
                    /* copy mdb Structure into the pdf structure */
                    feAnte = new FeAnte();
                    feAnteNullInds = NullHelper.CreateArrayOfNullInd(FeAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                    FeValCopy.FeCopyAnte(ref feAnte, mdbAnteStruct, ref feAnteNullInds, mdbAnteNulls);
                }
            }
            else
            {
                /* a pdf (environment or proposed) is used */
                rc = DynFeAnte.FeFetchAnte(anteHandle, out feAnte, out feAnteNullInds);
            }
            return (rc);
        }

        /// <summary>
        /// This method populates an FtAnte object with data from 
        /// the appropriate TS table that matches the prescribed selection criteria: 
        /// {call1, call2, bndcde & anum}.  
        /// </summary>
        /// <param name="cCall1"> - local call sign.</param>
        /// <param name="cCall2"> - remote call sign.</param>
        /// <param name="cBand"> - band code.</param>
        /// <param name="anum"> - antenna number.</param>
        /// <param name="tabName"> - name of DB table to use if not MDB.</param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        /// <param name="ftAnte"> - FtAnte object to be populated.</param>
        /// <param name="ftAnteNulls"> - ODBC nullInds associated with ftAnte.</param>
        /// <returns></returns>
        public static int TtAnteGet(string cCall1,
                            string cCall2,
                            string cBand,
                            int anum,
                            string tabName,
                            bool isMDB,
                            out FtAnte ftAnte,
                            out SQLLEN[] ftAnteNulls)
        {
            // 'out' requirement.
            ftAnte = null;
            ftAnteNulls = null;

            SQLLEN[] mtNulls;  //[MtAnte.SIZE_];
            MtAnte mtAnte;
            int nRet;

            if (isMDB)
            {
                // BUG FIX PENDING.
                // If the MDB tables are incoherent (i.e. channels can't find their antennas etc)
                // MtReadAnte() will return mtAnte as null with a non-zero nRet.
                // We need some 'guard' code to detect and handle this pathological situation.
                nRet = MtUtils.MtReadAnte(cCall1, cCall2, cBand, anum, out mtAnte, out mtNulls);

                if (nRet != Constant.SUCCESS) Log2.e("\n\nTpMdbPdfGet.TtAnteGet(): ERROR: nRet = " + nRet);

                /* copy mdb info into pdf structure */
                ftAnte = new FtAnte();
                ftAnteNulls = NullHelper.CreateArrayOfNullInd(FtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                FtValCopy.FtCopyAnte(ref ftAnte, mtAnte, ref ftAnteNulls, mtNulls);
            }
            else
            {
                //	memset(&ftAnte, 0, sizeof(ftAnte));
                nRet = FtUtils.FtReadAnte(cCall1, cCall2, cBand, anum, tabName, out ftAnte, out ftAnteNulls);
                if (ftAnte.cmd.Equals("D"))
                {
                    nRet = Constant.NOMORERECS;
                }
            }

            return (nRet);
        }

        /// <summary> 
        /// This method populates an FeAnte object with data from 
        /// the appropriate ES table that matches the prescribed selection criteria. 
        /// </summary>
        /// <param name="select"> - part of an SQL SELECT query WHERE clause.</param>
        /// <param name="tabName"> - name of DB table to use if not MDB.</param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        /// <param name="feAnte">- FeAnte object to be populated.</param>
        /// <param name="feAnteNulls"> - ODBC nullInds associated with feAnte.</param>
        /// <returns></returns>
        public static int TeAnteGet(string select,
                            string tabName,
                            bool isMDB,
                            out FeAnte feAnte,
                            out SQLLEN[] feAnteNulls)
        {
            // 'out' requirement.
            feAnte = null;
            feAnteNulls = null;

            SQLLEN[] meNulls;  // [ME_ANTE_SIZE_];
            MeAnte meAnte;

            int curHandle;
            int nRet;

            if (isMDB)
            {
                curHandle = DynMeAnte.MeSelectAnte(select, "");
                if (curHandle < 0)
                {
                    return curHandle;
                }
                nRet = DynMeAnte.MeFetchAnte(curHandle, out meAnte, out meNulls);
                if (nRet != 0)
                {
                    return (Constant.FAILURE);
                }

                /* copy mdb info into pdf structure */
                feAnte = new FeAnte();
                feAnteNulls = NullHelper.CreateArrayOfNullInd(FeAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                FeValCopy.FeCopyAnte(ref feAnte, meAnte, ref feAnteNulls, meNulls);

                DynMeAnte.MeCloseAnte(curHandle);
            }
            else
            {
                curHandle = DynFeAnte.FeSelectAnte(tabName, select, "");
                if (curHandle < 0)
                {
                    return curHandle;
                }
                nRet = DynFeAnte.FeFetchAnte(curHandle, out feAnte, out feAnteNulls);
                if (nRet != 0)
                {
                    return (Constant.FAILURE);
                }

                DynFeAnte.FeCloseAnte(curHandle);
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method populates an FtSiteStr object with data from 
        /// the appropriate TS tables that matches the prescribed selection criteria; FtSiteStr is a 
        /// class type that encapsulates an FtSite and its associated FtAntes.
        /// </summary>
        /// <param name="cCall1"> - local call sign.</param>
        /// <param name="tabName"> - name of DB table to use if not MDB.</param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        /// <param name="pSite">- FtSiteStr object to be populated.</param>
        /// <param name="siteNulls"> - FtSiteStrNulls object providing ODBC nullInd data for the contents of pSite.</param>
        /// <returns></returns>
        public static int TtSiteGetAnte(string cCall1,
                                    string tabName,
                                    bool isMDB,
                                    out FtSiteStr pSite,
                                    out FtSiteStrNulls siteNulls)
        {
            // 'out' requirements.
            pSite = null;
            siteNulls = null;

            int nRet;
            MtSiteStr pMtSite;
            MtSiteStrNulls pMtNulls;


            if (isMDB == true)
            {
                nRet = MtUtils.MtGetSiteWN(cCall1, out pMtSite, 2, out pMtNulls);  // Just to the antenna level 
                if (nRet == 0)
                {
                    /* copy mdb info into pdf structure */
                    MtUtils.MtToFtWN(out pSite, out siteNulls, pMtSite, pMtNulls);    /*  Ptr to input mt site          */

                }
            }
            else
            {
                nRet = FtUtils.FtGetSiteWN(cCall1, out pSite, 2, tabName, out siteNulls);
            }

            return (nRet);
        }

        /// <summary>
        /// This method selects ES Chan data, that matches the seleciton criteria, 
        /// from the appropriate ES table, based on the TSIP proposed and environment 
        /// files specified. It returns a handle (index) to a cursor object that can
        /// be used in subsequent calls to FetchEarthChan(). 
        /// </summary>
        /// <param name="tsChanTableName"> - name of table to be used if not a main table.</param>
        /// <param name="selection"> - part of an SQL 'WHERE' clause used for record selection.</param>
        /// <param name="envType"> - "MDB_ES" or other.</param>
        /// <returns></returns>
        public static int SelectEarthChan(string tsChanTableName,
                                        string selection,
                                        string envType)
        {
            // Beware: there are two distinct species of chanHandle: one from DynMeChan
            // and the other from DynFeChan.
            int chanHandle;

            string selectClause;

            if (envType.Equals("MDB_ES"))
            {
                //&&Console.Error.Write("\ntpMdbPdfGet.selectEarthChan(): molusc");
                /* MDB is used as environment */
                chanHandle = DynMeChan.MeSelectChan(selection, "");
            }
            else
            {
                /* a proposed or an environment pdf is used */
                /* exclude deleted records */
                selection.Trim();
                if (String.IsNullOrWhiteSpace(selection))
                {
                    //&&Console.Error.Write("\ntpMdbPdfGet.selectEarthChan(): snail");
                    selectClause = "cmd != 'D'";
                }
                else
                {
                    //&&Console.Error.Write("\ntpMdbPdfGet.selectEarthChan(): eel");
                    selectClause = String.Format("{0} and cmd != 'D'", selection);
                }

                chanHandle = DynFeChan.FeSelectChan(tsChanTableName, selectClause, "");
            }
            return (chanHandle);
        }

        /// <summary>
        /// This method populates an FeChan object by fetching ES Chan data from the DB, 
        /// using a cursor previously prepared by SelectEarthChan().
        /// </summary>
        /// <param name="chanHandle"> - handle (index) of cursor returned by SelectEarthChan().</param>
        /// <param name="chanStruct"> - FeChan object to be populated.</param>
        /// <param name="chanNulls"> - ODBC nullInds associated with chanStruct.</param>
        /// <param name="envType"> - if envType == "MDB_ES" then the MDB is used as the environment.</param>
        /// <returns></returns>
        public static int FetchEarthChan(int chanHandle,
                                     out FeChan chanStruct,
                                     out SQLLEN[] chanNulls,
                                     string envType)
        {
            // 'out' requirements.
            chanStruct = null;
            chanNulls = null;

            int rc;

            if (envType.Equals("MDB_ES"))
            {
                MeChan mdbChanStruct;
                SQLLEN[] mdbChanNulls;

                /* the mdb is used as environment */
                //&&Console.Error.Write("\ntpMdbPdfGet.teChanGet(): tiger");
                if ((rc = DynMeChan.MeFetchChan(chanHandle, out mdbChanStruct, out mdbChanNulls)) == Constant.SUCCESS)
                {
                    chanStruct = new FeChan();
                    chanNulls = NullHelper.CreateArrayOfNullInd(FeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                    /* copy mdb Structure into the pdf structure */
                    FeValCopy.FeCopyChan(ref chanStruct, mdbChanStruct, ref chanNulls, mdbChanNulls);
                }
            }
            else
            {
                /* a pdf (environment or proposed) is used */
                rc = DynFeChan.FeFetchChan(chanHandle, out chanStruct, out chanNulls);
            }
            return (rc);
        }

        /// <summary>
        /// This method selects TS Chan data, that matches the selection criteria, 
        /// from the appropriate TS table, based on the TSIP proposed and environment 
        /// files specified. It returns a handle (index) to a cursor object that can
        /// be used in subsequent calls to FetchTerrChan().  
        /// </summary>
        /// <param name="tsChanTableName"> - name of table to be used if not a main table.</param>
        /// <param name="selection"> - part of an SQL 'WHERE' clause used for record selection.</param>
        /// <param name="envType"> - "MDB_TS" or other.</param>
        /// <returns></returns>
        public static int SelectTerrChan(string tsChanTableName,
                                     string selection,
                                     string envType)
        {
            int chanHandle;
            string selectClause;

            if (envType.Equals("MDB_TS"))
            {
                /* MDB is used as environment */
                chanHandle = DynMdbChannel.MtSelectChannel(selection, "");
            }
            else
            {
                /* a proposed or an environment pdf is used */

                /* exclude deleted records */
                if (String.IsNullOrWhiteSpace(selection))
                {
                    selectClause = "cmd != 'D'";
                }
                else
                {
                    selectClause = String.Format("{0} and cmd != 'D'", selection);
                }

                chanHandle = DynChannel.FtSelectChannel(tsChanTableName, selectClause, "call1");
            }
            return (chanHandle);
        }

        /// <summary>
        /// This method populates an FtChan object by fetching TS Chan data from the DB, 
        /// using a cursor previously prepared by SelectTerrChan(). 
        /// </summary>
        /// <param name="chanHandle"> - handle (index) of cursor returned by SelectTerrChan().</param>
        /// <param name="chanStruct"> - FtChan object to be populated.</param>
        /// <param name="chanNulls"> - ODBC nullInds associated with chanStruct.</param>
        /// <param name="envType"> - if envType == "MDB_TS" then the MDB is used as the environment.</param>
        /// <returns></returns>
        public static int FetchTerrChan(int chanHandle,
                                    out FtChan chanStruct,
                                    out SQLLEN[] chanNulls,
                                    string envType)
        {
            // 'out' requirements.
            chanStruct = null;
            chanNulls = null;

            int rc;

            if (envType.Equals("MDB_TS"))
            {
                /* the mdb is used as environment */
                MtChan mdbChanStruct;
                SQLLEN[] mdbChanNulls;

                if ((rc = DynMdbChannel.MtFetchChannel(chanHandle, out mdbChanStruct, out mdbChanNulls)) == Constant.SUCCESS)
                {
                    /* copy mdb Structure into the pdf structure */
                    chanStruct = new FtChan();
                    chanNulls = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                    FtValCopy.FtCopyChan(ref chanStruct, mdbChanStruct, ref chanNulls, mdbChanNulls);
                }

            }
            else
            {
                /* a pdf (environment or proposed) is used */
                rc = DynChannel.FtFetchChannel(chanHandle, out chanStruct, out chanNulls);
            }
            return (rc);
        }

        /// <summary>
        /// This method closes the TS Channel cursor opened by SelectTerrChan() and releases
        /// the associated ODBC connection and statement handles. 
        /// </summary>
        /// <param name="chanHandle"> - cursor handle (index) to be closed.</param>
        /// <param name="envType"> - "MDB_TS" or other.</param>
        public static void CloseTerrChan(int chanHandle,
                                     string envType)
        {
            if (envType.Equals("MDB_TS"))
            {
                /* the mdb is used as environment */
                DynMdbChannel.MtCloseChannel(chanHandle);
            }
            else
            {
                /* a pdf (environment or proposed) is used */
                DynChannel.FtCloseChannel(chanHandle);
            }
        }

        /// <summary>
        /// This method closes the ES Channel cursor opened by SelectEarthChan() and releases
        /// the associated ODBC connection and statement handles. 
        /// </summary>
        /// <param name="chanHandle"> - cursor handle (index) to be closed.</param>
        /// <param name="envType"> - "MDB_ES" or other.</param>
        public static void CloseEarthChan(int chanHandle,
                                        string envType)
        {
            if (envType.Equals("MDB_ES"))
            {
                /* the mdb is used as environment */
                DynMeChan.MeCloseChan(chanHandle);
            }
            else
            {
                /* a pdf (environment or proposed) is used */
                DynFeChan.FeCloseChan(chanHandle);
            }
        }

        /// <summary>
        /// This method populates an FtSiteStr object with data from 
        /// the appropriate TS tables that matches the prescribed selection criteria (call sign);
        /// FtSiteStr is a class type that encapsulates an FtSite and its associated 
        /// FtAntes and FtChans. 
        /// </summary>
        /// <param name="cCallSign"> - local call sign.</param>
        /// <param name="tabName"> - name of DB table to use if not MDB.</param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        /// <param name="pSite">- FtSiteStr object to be populated.</param>
        /// <param name="siteNulls"> - FtSiteStrNulls object providing ODBC nullInd data for the contents of pSite.</param>
        /// <returns></returns>
        public static int TtFullSiteGet(string cCallSign,
                                    string tabName,
                                    bool isMDB,
                                    out FtSiteStr pSite,
                                    out FtSiteStrNulls siteNulls)
        {
            // 'out' requirements.
            pSite = null;
            siteNulls = null;

            int nRet;
            MtSiteStr pMtSite;
            MtSiteStrNulls pMtNulls;

            if (isMDB)
            {
                nRet = MtUtils.MtGetSiteWN(cCallSign, out pMtSite, 3, out pMtNulls);
                if (nRet == 0)
                {
                    // Copy mdb info into pdf structure */
                    MtUtils.MtToFtWN(out pSite, out siteNulls, pMtSite, pMtNulls);
                }
            }
            else
            {
                nRet = FtUtils.FtGetSiteWN(cCallSign, out pSite, 3, tabName, out siteNulls);
            }

            return (nRet);
        }

        /// <summary>
        /// This method populates an FeChan object with data from 
        /// the appropriate ES table that matches the prescribed selection criteria. 
        /// </summary>
        /// <param name="select"> - part of an SQL SELECT query WHERE clause.</param>
        /// <param name="tabName"> - name of DB table to use if not MDB.</param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        /// <param name="chanStruct">- FeChan object to be populated.</param>
        /// <param name="chanNulls"> - ODBC nullInds associated with chanStruct.</param>
        /// <returns></returns>
        public static int TeChanGet(string select,
                            string tabName,
                            bool isMDB,
                            out FeChan chanStruct,
                            out SQLLEN[] chanNulls)
        {
            // 'out' requirements.
            chanStruct = null;
            chanNulls = null;

            SQLLEN[] meNulls;  //[MeChan.NUM_COLUMNS];
            MeChan meChan;
            int nHandle;
            int nRet;

            if (isMDB == true)
            {
                nHandle = DynMeChan.MeSelectChan(select, "location,call1,chid");
                //!!Console.Error.Write("\nTpMdbPdfGet.teChanGet(): horse, nHandle = " + nHandle);
                if (nHandle < 0)
                {
                    return nHandle;
                }
                //&&Console.Error.Write("\ntpMdbPdfGet.teChanGet(): Gnat");
                nRet = DynMeChan.MeFetchChan(nHandle, out meChan, out meNulls);
                //!!Console.Error.Write("\nTpMdbPdfGet.teChanGet(): horse, nHandle = " + nHandle);
                DynMeChan.MeCloseChan(nHandle);

                if (nRet == 0)
                {
                    chanStruct = new FeChan();
                    chanNulls = NullHelper.CreateArrayOfNullInd(FeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                    /* copy mdb info into pdf structure */
                    FeValCopy.FeCopyChan(ref chanStruct, meChan, ref chanNulls, meNulls);
                }
            }
            else
            {
                nHandle = DynFeChan.FeSelectChan(tabName, select, "location,call1,chid");
                if (nHandle < 0)
                {
                    return nHandle;
                }

                nRet = DynFeChan.FeFetchChan(nHandle, out chanStruct, out chanNulls);

                DynFeChan.FeCloseChan(nHandle);
            }

            return nRet;
        }


        /// <summary>
        /// This method populates an FtAnte object with data from 
        /// the appropriate TS table that matches the prescribed selection criteria.  
        /// </summary>
        /// <param name="selection"> - part of an SQL SELECT query WHERE clause.</param>
        /// <param name="tabName"> - name of DB table to use if not MDB.</param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        /// <param name="anteStruct"> - FtAnte object to be populated.</param>
        /// <param name="anteNulls"> - ODBC nullInds associated with anteStruct.</param>
        /// <returns></returns>
        public static int TtAnteGetCond(string selection,
                                    string tabName,
                                    bool isMDB,
                                    out FtAnte anteStruct,
                                    out SQLLEN[] anteNulls)
        {
            // 'out' requirements.
            anteStruct = null;
            anteNulls = null;

            SQLLEN[] mtNulls;  //[MtAnte.SIZE_];
            MtAnte mtAnte;
            int nRet;
            int nHandle;

            if (isMDB)
            {
                //memset(&mtAnte, 0, sizeof(mtAnte));  /*	Zero to handle nulls. */
                //sqlCommand = String.Format("select * from {0} where {1}", tabName, select);
                //exec sql execute immediate :sqlCommand into :mtAnte:mtNulls;
                //if (sqlca.sqlcode == Constant.NOMORERECS) {
                //	return(Constant.FAILURE);
                //} else if ((sqlca.sqlcode != Constant.SUCCESS)	&& (sqlca.sqlcode != SEVERALRECS)) {
                //	return(Constant.DYN_MS_SQL_SERVER_ERR);
                //}
                nHandle = DynMdbAntenna.MtSelectAntenna(selection, "");
                if (nHandle < 0)
                {
                    return nHandle;
                }

                nRet = DynMdbAntenna.MtFetchAntenna(nHandle, out mtAnte, out mtNulls);

                DynMdbAntenna.MtCloseAntenna(nHandle);

                if (nRet == 0)
                {
                    anteStruct = new FtAnte();
                    anteNulls = NullHelper.CreateArrayOfNullInd(FtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                    /* copy mdb info into pdf structure */
                    FtValCopy.FtCopyAnte(ref anteStruct, mtAnte, ref anteNulls, mtNulls);
                }
            }
            else
            {
                nHandle = DynAntenna.FtSelectAntenna(tabName, selection, "");

                nRet = DynAntenna.FtFetchAntenna(nHandle, out anteStruct, out anteNulls);

                DynAntenna.FtCloseAntenna(nHandle);
            }

            return (nRet);
        }

        /// <summary>
        /// This method populates an FtChan object with data from 
        /// the appropriate TS table that matches the prescribed selection criteria: 
        /// {call1, call2, bndcde & chid}. 
        /// </summary>
        /// <param name="cCall1"> - local call sign.</param>
        /// <param name="cCall2"> - remote call sign.</param>
        /// <param name="bndcde"> - band code.</param>
        /// <param name="chid"> - channel ID.</param>
        /// <param name="tabName"> - name of DB table to use if not MDB.</param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not</param>
        /// <param name="chanStruct"> - FtChan object to be populated.</param>
        /// <param name="chanNulls"> - ODBC nullInds associated with chanStruct.</param>
        /// <returns></returns>
        public static int TtChanGet(string cCall1,
                            string cCall2,
                            string bndcde,
                            string chid,
                            string tabName,
                            bool isMDB,
                            out FtChan chanStruct,
                            out SQLLEN[] chanNulls)
        {
            // 'out' requirements.
            chanStruct = null;
            chanNulls = null;

            SQLLEN[] mtNulls;   // [MtChan.SIZE_];
            MtChan mtChan;

            int nRet = 0;

            if (isMDB == true)
            {
                nRet = MtUtils.MtReadChan(cCall1, cCall2, bndcde, chid, out mtChan, out mtNulls);

                if (nRet == Constant.NOMORERECS)
                {
                    return (Constant.FAILURE);
                }
                else if (nRet != 0)
                {
                    return (Error.DYN_MS_SQL_SERVER_ERR);
                }

                chanStruct = new FtChan();
                chanNulls = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                /* copy mdb info into pdf structure */
                FtValCopy.FtCopyChan(ref chanStruct, mtChan, ref chanNulls, mtNulls);
            }
            else
            {
                nRet = FtUtils.FtReadChan(cCall1, cCall2, bndcde, chid, tabName, out chanStruct, out chanNulls);

                if (nRet == Constant.NOMORERECS)
                {
                    return (Constant.FAILURE);
                }
                else if (chanStruct.cmd.Equals("D"))
                {
                    return (Constant.FAILURE);
                }
                else if (nRet != 0)
                {
                    return (Error.DYN_MS_SQL_SERVER_ERR);
                }
            }

            return (nRet);
        }

        /// <summary>
        /// This method returns the counts of the Es-Ts and Ts-Es interference cases for ES.  
        /// </summary>
        /// <param name="runname"> - User-defined unique run ID.</param>
        /// <param name="siteName"> - name of site table.</param>
        /// <param name="anteName"> - name of ante table.</param>
        /// <param name="TsEsStnGroups"> - count of the number of Ts-Es interference cases for ES.</param>
        /// <param name="EsTsStnGroups"> - count of the number of Es-Ts interference cases for ES.</param>
        /// <returns></returns>
        public static int UtGetInterferenceGroups(string runname,
                                                            string siteName,
                                                            string anteName,
                                                            out int TsEsStnGroups,
                                                            out int EsTsStnGroups)
        {
            // 'out' requirement.
            TsEsStnGroups = 0;
            EsTsStnGroups = 0;

            int rv = Constant.FAILURE;
            string selcount;
            string cUnique;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTpMdbPdfGet.UtGetInterferenceGroups(): ERROR: call to SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            GenUtil.MkUnique(out cUnique, Info.GlobalSchema, "", "stat_cnt_", "", runname);
            /*	In ingres 2.5 it looks like dropping a table that does not exist
            *		will cause an error message. So remove this. GJS - 2002.07.02
            selcount = String.Format("drop table {0}", cUnique);
            exec sql execute immediate :selcount;
            */

            selcount = String.Format("create view {0} as select distinct a.terrcall1, a.earthlocation, b.earthcall1, b.earthband from {1} a, {2} b",
                                             cUnique, siteName, anteName);
            selcount += " where a.terrcall1 = b.terrcall1 and a.earthlocation = b.earthlocation and interferer = 'T'";

            //	exec sql execute immediate :selcount;
            sqlRet = ODBC.SQLExecDirect(hStmt, selcount, selcount.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTpMdbPdfGet.UtGetInterferenceGroups(): ERROR: A: call to SQLExecDirect() failed: \n" + selcount);
                TsEsStnGroups = -1;
                rv = Error.ODBC_EXECDIRECT_FAILED;
            }
            else
            {
                TsEsStnGroups = Ssutil.DbCountRows(cUnique, null);
                if (TsEsStnGroups < 0)
                {
                    TsEsStnGroups = 0;
                }

                rv = Constant.SUCCESS;

                selcount = String.Format("drop view {0}", cUnique);
                sqlRet = ODBC.SQLExecDirect(hStmt, selcount, selcount.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\nTpMdbPdfGet.UtGetInterferenceGroups(): ERROR: B: call to SQLExecDirect() failed: \n" + selcount);
                    rv = Error.ODBC_EXECDIRECT_FAILED;
                }
            }

            selcount = String.Format("create view {0} as select distinct a.earthlocation, b.earthcall1, b.earthband, a.terrcall1 from {1} a, {2} b",
                                             cUnique, siteName, anteName);
            selcount += " where a.earthlocation = b.earthlocation and a.terrcall1 = b.terrcall1 and interferer = 'E'";

            //exec sql execute immediate :selcount;
            sqlRet = ODBC.SQLExecDirect(hStmt, selcount, selcount.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTpMdbPdfGet.UtGetInterferenceGroups(): ERROR: C: call to SQLExecDirect() failed: \n" + selcount);
                EsTsStnGroups = -1;
                rv = Error.ODBC_EXECDIRECT_FAILED;
            }
            else
            {
                EsTsStnGroups = Ssutil.DbCountRows(cUnique, null);
                if (EsTsStnGroups < 0)
                {
                    EsTsStnGroups = 0;
                }

                rv = Constant.SUCCESS;

                selcount = String.Format("drop view {0}", cUnique);

                sqlRet = ODBC.SQLExecDirect(hStmt, selcount, selcount.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\nTpMdbPdfGet.UtGetInterferenceGroups(): ERROR: D: call to SQLExecDirect() failed: " + selcount);
                    rv = Error.ODBC_EXECDIRECT_FAILED;
                }

            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return rv;
        }

        /// <summary>
        /// This method enumerates sites in the <b>main.mt_site</b> table or a 
        /// caller-defined table that match a prescribed selection criteria.  
        /// </summary>
        /// <param name="cSearch"> - part of an SQL SELECT query WHERE clause.</param>
        /// <param name="cTable"> - name of table to be searched; if null or blank then the main table is used.</param>
        /// <param name="cCallFound"> - on input this prescribes that any found call sign must 
        /// be alphabetically greater than cCallFound; if a record that matches all the search 
        /// criteria is found, cCallFound returns its call sign.</param>
        /// <returns></returns>
        public static int TtEnumSite(string cSearch, string cTable, ref string cCallFound)
        {
            if (String.IsNullOrWhiteSpace(cTable))
            {
                //	mdb.
                return MtUtils.MtEnumSite(cSearch, ref cCallFound);
            }
            else
            {
                //	pdf
                return FtUtils.FtEnumSite(cSearch, cTable, ref cCallFound);
            }
        }

        /// <summary>
        /// This method selects TS Chan data, that matches the selection criteria, 
        /// from the appropriate TS table, based on the TSIP proposed and environment 
        /// files specified. It returns a handle (index) to a cursor object that can
        /// be used in subsequent calls to TtTtFetchChannel(). 
        /// </summary>
        /// <param name="chanTableName"> - name of table to be used.</param>
        /// <param name="select"> - part of an SQL 'WHERE' clause used for record selection.</param>
        /// <param name="envType"> - "MDB_TS" or other.</param>
        /// <returns></returns>
        public static int TtTtSelectChannel(string chanTableName,
                                            string select,
                                            string envType)
        {
            int chanHandle;
            string selectClause;

            if (envType.Equals("MDB_TS"))
            {   /* MDB is used */
                chanHandle = DynMdbChannel.MtSelectChannel(select, "");

            }
            else
            {   /* an environment pdf is used */
                select = select.Trim();
                selectClause = select;
                if (String.IsNullOrWhiteSpace(select))
                {
                    selectClause = "cmd != 'D'";
                }
                else
                {
                    selectClause += " and cmd != 'D'";
                }

                chanHandle = DynChannel.FtSelectChannel(chanTableName, selectClause, "call1");
            }
            return (chanHandle);
        }

        /// <summary>
        /// This method populates an FtChan object by fetching TS Chan data from the DB, 
        /// using a cursor previously prepared by TtTtSelectChannel().
        /// </summary>
        /// <param name="chanHandle"> - handle (index) of cursor returned by TtTtSelectChannel()</param>
        /// <param name="chanStruct"> - FtChan object to be populated.</param>
        /// <param name="chanNulls"> - ODBC nullInds associated with chanStruct.</param>
        /// <param name="envType"> - if envType == "MDB_TS" then the MDB is used as the environment.</param>
        /// <returns></returns>
        public static int TtTtFetchChannel(int chanHandle,
                                            out FtChan chanStruct,
                                            out SQLLEN[] chanNulls,
                                            string envType)
        {
            // 'out' requirements.
            chanStruct = null;
            chanNulls = null;

            int rc;
            SQLLEN[] mdbChanNulls;  //[MtChan.NUM_COLUMNS];
            MtChan mdbChanStruct;

            if (envType.Equals("MDB_TS"))
            {   /* the mdb is used */
                rc = DynMdbChannel.MtFetchChannel(chanHandle, out mdbChanStruct, out mdbChanNulls);

                if (rc == Constant.SUCCESS)
                {
                    /*copy mdb Structure into the pdf structure */
                    chanStruct = new FtChan();
                    chanNulls = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                    FtValCopy.FtCopyChan(ref chanStruct, mdbChanStruct, ref chanNulls, mdbChanNulls);

                    chanStruct.cmd = "N";
                }
            }
            else
            {   /* a pdf is used */
                rc = DynChannel.FtFetchChannel(chanHandle, out chanStruct, out chanNulls);
            }

            return rc;
        }

        /// <summary>
        /// This method closes the TS Channel cursor opened by TtTtSelectChannel() and releases
        /// the associated ODBC connection and statement handles.  
        /// </summary>
        /// <param name="chanHandle"> - cursor handle (index) to be closed.</param>
        /// <param name="envType"> - "MDB_TS" or other.</param>
        public static void TtTtCloseChannel(int chanHandle, string envType)
        {
            if (envType.Equals("MDB_TS"))
            {
                /* the mdb is used */
                DynMdbChannel.MtCloseChannel(chanHandle);
            }
            else
            {
                /* a pdf is used */
                DynChannel.FtCloseChannel(chanHandle);
            }
        }





    }
}

```
