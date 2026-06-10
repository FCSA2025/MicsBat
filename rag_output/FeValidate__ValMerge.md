# Documented File: ValMerge.cs
**Repository Path:** `FeValidate\ValMerge.cs`
**Primary Layer:** `FeValidate`
**Namespace:** `FeValidate`

## Source Code Representation
```csharp
﻿using System;
using static System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _DataStructures;

namespace FeValidate
{
    using _Configuration;
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
    /// This class provides methods that 'merge' information from the ES MDB tables and
    /// the PDF data set and then writes the merged records back to the ES PDF tables. The
    /// details of the merging algorithm depend on the ES record type (site, antenna, azimuth
    /// and channel) and also on the PDF record's MDP operation command (ADD, BLANK, DELETE, 
    /// NO-CHANGE and UPDATE).
    /// </summary>
    public class ValMerge
    {
        /// <summary>
        /// This method deletes all computer-generated site, antennae, azimuth and channel 
        /// records from a PDF data set as a prelude to validation; computer-generated records
        /// have a 'recstat' field equal to 'C' (as opposed to user-defined records whose 'recstat' is 'U').
        /// </summary>
        /// <param name="pdfName"> - name of the PDF.</param>
        public static void FeValPurgeRecords(string pdfName)
        {
            //...Log2.v("\n\nValMerge.FeValPurgeRecords(): Entry");

            int maxDim = Max(FeSite.NUM_COLUMNS, Max(FeAnte.NUM_COLUMNS, Max(FeChan.NUM_COLUMNS, FeAzim.NUM_COLUMNS)));
            SQLLEN[] nArrayFW = NullHelper.CreateArrayOfNullInd(maxDim, NullHelper.ColumnStatus.NULL);
            int id1;
            string tableName;

            FeSite feSite;
            FeAnte feAnte;
            FeChan feChan;
            FeAzim feAzim;

            int nRet;

            // read site information that is computer generated 
            GenUtil.UtCvtName(Constant.FE_SITE, pdfName, out tableName);
            if ((id1 = DynFeSite.FeSelectSite(tableName, "recstat = 'C'", "")) < 0)
            {
                ValErrs.AddMess("PURGE 1 - could not read site information. Reason: %d",
                                pdfName, "W", id1.ToString());
                return;
            }

            // delete computer generated sites 
            while ((nRet = DynFeSite.FeFetchSite(id1, out feSite, out nArrayFW)) == Constant.SUCCESS)
            {
                if ((nRet = DynFeSite.FeDeleteSite(id1)) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("PURGE 2 - Error deleting site %s. Reason %d",
                                    pdfName, "W", feSite.location, nRet.ToString());
                }
            }
            DynFeSite.FeCloseSite(id1);


            // read ante information that is computer generated 
            GenUtil.UtCvtName(Constant.FE_ANTE, pdfName, out tableName);
            if ((id1 = DynFeAnte.FeSelectAnte(tableName, "recstat = 'C'", "")) < 0)
            {
                ValErrs.AddMess("PURGE 3 - could not select antenna information. Reason: %d",
                                pdfName, "W", id1.ToString());
                return;
            }

            // delete computer generated antenna 
            while (DynFeAnte.FeFetchAnte(id1, out feAnte, out nArrayFW) == Constant.SUCCESS)
            {
                if (DynFeAnte.FeDeleteAnte(id1) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("PURGE 4 - could not delete antenna information. Reason: %d",
                                    pdfName, "W", id1.ToString());
                }
            }
            DynFeAnte.FeCloseAnte(id1);


            // read chan information that is computer generated 
            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out tableName);
            if ((id1 = DynFeChan.FeSelectChan(tableName, "recstat = 'C'", "")) < 0)
            {
                ValErrs.AddMess("PURGE 5 - could not read channel information. Reason: %d",
                                pdfName, "W", id1.ToString());
                return;
            }

            // delete computer generated chans 
            while (DynFeChan.FeFetchChan(id1, out feChan, out nArrayFW) == Constant.SUCCESS)
            {
                if (DynFeChan.FeDeleteChan(id1) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("PURGE 6 - could not fetch channel information. Reason: %d",
                                    pdfName, "W", id1.ToString());
                }
            }
            DynFeChan.FeCloseChan(id1);


            // read azim information that is computer generated 
            GenUtil.UtCvtName(Constant.FE_AZIM, pdfName, out tableName);
            if ((id1 = DynFeAzim.FeSelectAzim(tableName, "recstat = 'C'", "")) < 0)
            {
                ValErrs.AddMess("PURGE 7 - could not read azimuth information. Reason: %d",
                                pdfName, "W", id1.ToString());
                return;
            }

            // delete computer generated azims 
            while (DynFeAzim.FeFetchAzim(id1, out feAzim, out nArrayFW) == Constant.SUCCESS)
            {
                if (DynFeAzim.FeDeleteAzim(id1) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("PURGE 8 - could not fetch azimuth information. Reason: %d",
                                    pdfName, "W", id1.ToString());
                }
            }
            DynFeAzim.FeCloseAzim(id1);

            //...Log2.v("\nValMerge.FeValPurgeRecords(): Exit");
        }

        /// <summary>
        /// This method merges information in the PDF's _site table with
        /// that of a site with the same location in the MDB table me_site i.a.w. 
        /// the MDB operation command in the PDF record; the merged site information is written
        /// back to the PDF _site table.
        /// </summary>
        /// <remarks>
        /// \section sect Site merging by cmd type: 
        /// This method manages the merging of site information from the MDB and from the PDF 
        /// i.a.w. the MDB operation command in the PDF record, as described below:
        /// \subsection subsect00 if (feSite.cmd.Equals("B"))
        /// If the site record's MDB operation is BLANK-OUT then it is ignored; nothing in the PDF data set changes.
        /// \subsection subsect01 if (feSite.cmd.Equals("N"))
        /// When a site record's MDB operation cmd is NO-CHANGE (NOOP) all of its fields are
        /// overwritten by the field values for that site in the MDB site table.
        /// \subsection subsect2 if (feSite.cmd.Equals("A"))
        /// If the PDF wants to ADD a site we must ensure that all the antenna, azimuth and channel records that
        /// reference the site are also 'pulled into' the PDF data set. The following methods are called in succession:
        /// <list type="bullet">
        /// <item>ValImport.FeImportAntenna : This methods checks whether prescribed antenna records 
        /// (selected by the 'whereClause') exist in the PDF's _ante table;  : if an individual 
        /// azimuth record is not in the PDF's _ante table then it is imported (copied) in from the MDB's 
        /// me_ante table. The imported record's MDB operation cmd is set to "N".</item>
        /// <item>ValImport.FeImportChannel : This methods checks whether prescribed channel records 
        /// (selected by the 'whereClause') exist in the PDF's _chan table;  : if an individual 
        /// channel record is not in the PDF's _chan table then it is imported (copied) in from the MDB's 
        /// me_chan table. The imported record's MDB operation cmd is set to "N".</item>
        /// <item>ValImport.FeImportAzimuth : This methods checks whether prescribed azimuth records 
        /// (selected by the 'whereClause') exist in the PDF's _azim table;  : if an individual 
        /// azimuth record is not in the PDF's _azim table then it is imported (copied) in from the MDB's 
        /// me_azim table. The imported record's MDB operation cmd is set to "N".</item>
        /// </list>
        /// \subsection subsect3 if (feSite.cmd.Equals("D"))
        /// If the PDF wants to DELETE a site we must ensure that all the antenna, azimuth and channel records that
        /// reference the site are also 'pulled into' the PDF data set for subsequent deletion themselves. 
        /// The same three worker methods are called as for the ADD case described above <b>except</b> that the
        /// MDB operation cmd of the imported antenna, channel and azimuth records are <b>set to "D"</b>.
        /// \subsection subsect4 if (feSite.cmd.Equals("U"))
        /// If the PDF wants to UPDATE a site then the site record's fields are parsed to identify
        /// any that are blank (i.e. have ODBC NULL); these blank fields are then populated with
        /// the field values of the site with the same location from the MDB me_site table.
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="azimRecChanged"> - boolean flag that indicates whether an 
        /// azimuth record changed prior to this call.</param>
        public static void FeValMergeSiteRecords(string pdfName, ref bool azimRecChanged)
        {
            //...Log2.v("\n\nValMerge.FeValMergeSiteRecords: Entry");

            FeSite feSite;
            SQLLEN[] feSiteNullInds;

            MeSite meSite;
            SQLLEN[] meSiteNullInds;

            int sID1;
            string tableName;
            string whereClause;
            int nRet;
            int nHandle;

            MeSiteStr meSiteStr;
            MeSiteStrNulls meSiteStrNull;

            GenUtil.UtCvtName(Constant.FE_SITE, pdfName, out tableName);

            // read site information 
            if ((sID1 = DynFeSite.FeSelectSite(tableName, "", "")) < 0)
            {
                ValErrs.AddMess("MERGE 1 - could not read site information. Reason: %d",
                                pdfName, "W", sID1.ToString());
                return;
            }

            while (DynFeSite.FeFetchSite(sID1, out feSite, out feSiteNullInds) == Constant.SUCCESS)
            {
                if (feSite.cmd.Equals("N"))
                {
                    nRet = MeUtils.MeGetSiteWN(feSite.location, out meSiteStr, out meSiteStrNull, 1);
                    if (nRet == 0)
                    {
                        // MDB record found 
                        FeValCopy.FeCopySite(ref feSite, meSiteStr.stSite, ref feSiteNullInds, meSiteStrNull.anSiteNull);
                        if ((nRet = DynFeSite.FeUpdateSite(sID1, feSite, feSiteNullInds)) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("MERGE 2 - could not update site information. Reason: %d",
                                            pdfName, "W", nRet.ToString());
                        }
                    }
                    continue;
                }

                // retrieve all local info 
                ValImport.FeFormWhereClause(out whereClause, feSite.location, "", "");

                /* if adding site, pull in antes and chans !! there should
                   not be any of these records found */
                if (feSite.cmd.Equals("A"))
                {
                    ValImport.FeImportAntenna(pdfName, whereClause, "N");

                    ValImport.FeImportChannel(pdfName, whereClause, "N");

                    ValImport.FeImportAzimuth(pdfName, whereClause, "N");

                }

                // if deleting site, pull in subsidiary records too 
                if (feSite.cmd.Equals("D"))
                {
                    ValImport.FeImportAntenna(pdfName, whereClause, "D");
                    ValImport.FeImportChannel(pdfName, whereClause, "D");
                    ValImport.FeImportAzimuth(pdfName, whereClause, "D");

                }

                if ((!feSite.cmd.Equals("U")) && (!feSite.cmd.Equals("B")))
                {
                    // if not Update or Blank (ie. Add or Del) dont merge 
                    continue;
                }

                // fetch the corresponding MDB record 
                ValImport.FeFormWhereClause(out whereClause, feSite.location, "", "");
                nHandle = DynMeSite.MeSelectSite(whereClause, null);
                nRet = DynMeSite.MeFetchSite(nHandle, out meSite, out meSiteNullInds);
                DynMeSite.MeCloseSite(nHandle);
                if (nRet != 0)
                {
                    // if MDB record not found, get next from FW 
                    continue;
                }

                if (feSite.cmd.Equals("U"))
                {
                    /* Updating record so pull in info from mdb -
                     * not done when blanking is the command */
                    if (feSiteNullInds[FeSite.LOCATION] == Constant.DB_NULL)
                    {
                        feSite.location = meSite.location;
                        feSiteNullInds[FeSite.LOCATION] = meSiteNullInds[MeSite.LOCATION];
                    }

                    if (feSiteNullInds[FeSite.NAME] == Constant.DB_NULL)
                    {
                        feSite.name = meSite.name;
                        feSiteNullInds[FeSite.NAME] = meSiteNullInds[MeSite.NAME];
                    }
                    if (feSiteNullInds[FeSite.PROV] == Constant.DB_NULL)
                    {
                        feSite.prov = meSite.prov;
                        feSiteNullInds[FeSite.PROV] = meSiteNullInds[MeSite.PROV];
                    }
                    if (feSiteNullInds[FeSite.OPER] == Constant.DB_NULL)
                    {
                        feSite.oper = meSite.oper;
                        feSiteNullInds[FeSite.OPER] = meSiteNullInds[MeSite.OPER];
                    }
                    if (feSiteNullInds[FeSite.LATIT] == Constant.DB_NULL)
                    {
                        feSite.latit = meSite.latit;
                        feSiteNullInds[FeSite.LATIT] = meSiteNullInds[MeSite.LATIT];
                    }
                    if (feSiteNullInds[FeSite.LONGIT] == Constant.DB_NULL)
                    {
                        feSite.longit = meSite.longit;
                        feSiteNullInds[FeSite.LONGIT] = meSiteNullInds[MeSite.LONGIT];
                    }
                    if (feSiteNullInds[FeSite.GRND] == Constant.DB_NULL)
                    {
                        feSite.grnd = meSite.grnd;
                        feSiteNullInds[FeSite.GRND] = meSiteNullInds[MeSite.GRND];
                    }

                    if (feSiteNullInds[FeSite.RADIO] == Constant.DB_NULL)
                    {
                        feSite.radio = meSite.radio;
                        feSiteNullInds[FeSite.RADIO] = meSiteNullInds[MeSite.RADIO];
                    }

                    if (feSiteNullInds[FeSite.RAIN] == Constant.DB_NULL)
                    {
                        feSite.rain = meSite.rain;
                        feSiteNullInds[FeSite.RAIN] = meSiteNullInds[MeSite.RAIN];
                    }

                    if (feSiteNullInds[FeSite.STATS] == Constant.DB_NULL)
                    {
                        feSite.stats = meSite.stats;
                        feSiteNullInds[FeSite.STATS] = meSiteNullInds[MeSite.STATS];
                    }
                    if (feSiteNullInds[FeSite.SDATE] == Constant.DB_NULL)
                    {
                        feSite.sdate = meSite.sdate;
                        feSiteNullInds[FeSite.SDATE] = meSiteNullInds[MeSite.SDATE];
                    }
                    if (feSiteNullInds[FeSite.REG] == Constant.DB_NULL)
                    {
                        feSite.reg = meSite.reg;
                        feSiteNullInds[FeSite.REG] = meSiteNullInds[MeSite.REG];
                    }
                    if (feSiteNullInds[FeSite.NOTS] == Constant.DB_NULL)
                    {
                        feSite.nots = meSite.nots;
                        feSiteNullInds[FeSite.NOTS] = meSiteNullInds[MeSite.NOTS];
                    }
                    if (feSiteNullInds[FeSite.OPRTYP] == Constant.DB_NULL)
                    {
                        feSite.oprtyp = meSite.oprtyp;
                        feSiteNullInds[FeSite.OPRTYP] = meSiteNullInds[MeSite.OPRTYP];
                    }
                }
                if (feSiteNullInds[FeSite.MDATE] == Constant.DB_NULL)
                {
                    feSite.mdate = meSite.mdate;
                    feSiteNullInds[FeSite.MDATE] = meSiteNullInds[MeSite.MDATE];
                }
                if (feSiteNullInds[FeSite.MTIME] == Constant.DB_NULL)
                {
                    feSite.mtime = meSite.mtime;
                    feSiteNullInds[FeSite.MTIME] = meSiteNullInds[MeSite.MTIME];
                }

                if ((nRet = DynFeSite.FeUpdateSite(sID1, feSite, feSiteNullInds)) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("MERGE 3 - could not update site information. Reason: %d",
                                    pdfName, "W", nRet.ToString());
                }

                // pull in ante + chan recs if latit, longit or grnd have changed 
                if ((meSite.latit != feSite.latit) ||
                    (meSite.longit != feSite.longit) ||
                    (meSite.grnd != feSite.grnd))
                {
                    azimRecChanged = true;
                    /* in the val Azim section this flag indicates
                       that we should warn users that the azim record
                       may have to be changed */
                    ValImport.FeImportAntenna(pdfName, whereClause, "U");
                    /* must be set to update so that we can change the values of azim, elev, txhgmax, rxhgmax
                       due to a change in latit, longit, or grnd */

                    ValImport.FeImportChannel(pdfName, whereClause, "N");
                    ValImport.FeImportAzimuth(pdfName, whereClause, "N");

                }
                else
                {
                    if (!meSite.oper.Equals(feSite.oper))
                    {
                        ValImport.FeImportAntenna(pdfName, whereClause, "U");
                    }
                }
            }
            DynFeSite.FeCloseSite(sID1);

            //...Log2.v("\nValMerge.FeValMergeSiteRecords: Exit");
        }

        /// <summary>
        /// This method merges information in the PDF's _ante table with
        /// that of an antenna with the same location and callsign in the MDB table me_site i.a.w. 
        /// the MDB operation command in the PDF record; the merged antenna information is written
        /// back to the PDF _ante table.
        /// </summary>
        /// <remarks>
        /// \section secta Antennae merging by cmd type: 
        /// This method manages the merging of antenna information from the MDB and from the PDF 
        /// i.a.w. the MDB operation command in the PDF record, as described below:
        /// \subsection subsect00a if (feAnte.cmd.Equals("B"))
        /// If the antenna record's MDB operation is BLANK-OUT then it is ignored; nothing in the PDF data set changes.
        /// \subsection subsect01a if (feAnte.cmd.Equals("N"))
        /// When a site record's MDB operation cmd is NO-CHANGE (NOOP) all of its fields are
        /// overwritten by the field values for that site in the MDB site table.
        /// \subsection subsect2a if (feSite.cmd.Equals("A"))
        /// If the PDF wants to ADD a site we must ensure that all the antenna, azimuth and channel records that
        /// reference the site are also 'pulled into' the PDF data set. The following methods are called in succession:
        /// <list type="bullet">
        /// <item>ValImport.FeImportChannel : This methods checks whether prescribed channel records 
        /// (selected by the 'whereClause') exist in the PDF's _chan table;  : if an individual 
        /// channel record is not in the PDF's _chan table then it is imported (copied) in from the MDB's 
        /// me_chan table. The imported record's MDB operation cmd is set to "N".</item>
        /// <item>ValImport.FeImportAzimuth : This methods checks whether prescribed azimuth records 
        /// (selected by the 'whereClause') exist in the PDF's _azim table;  : if an individual 
        /// azimuth record is not in the PDF's _azim table then it is imported (copied) in from the MDB's 
        /// me_azim table. The imported record's MDB operation cmd is set to "N".</item>
        /// </list>
        /// \subsection subsect3a if (feSite.cmd.Equals("D"))
        /// If the PDF wants to DELETE an antenna we must ensure that all the azimuth and channel records that
        /// reference the antenna are also 'pulled into' the PDF data set for subsequent deletion themselves. 
        /// The same two worker methods are called as for the ADD case described above <b>except</b> that the
        /// MDB operation cmd of the imported channel and azimuth records are <b>set to "D"</b>.
        /// \subsection subsect4a if (feSite.cmd.Equals("U"))
        /// If the PDF wants to UPDATE an antenna then the site record's fields are parsed to identify
        /// any that are blank (i.e. have ODBC NULL); these blank fields are then populated with
        /// the field values of the antenna with the same location and callsign from the MDB me_ante table.
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="azimRecChanged"> - boolean flag that indicates whether an 
        /// azimuth record changed prior to this call.</param>
        public static void FeValMergeAnteRecords(string pdfName,  // pdf display name 
                                                 ref bool azimRecChanged)
        {
            //...Log2.v("\n\nValMerge.FeValMergeAnteRecords: Entry");

            SQLLEN[] nArrayFW; //[FeAnte.SIZE_];
            int aID1;
            bool pullrecs = false;
            string tableName;
            string whereClauseLoc;
            string whereClauseCall;
            FeAnte feAnte;
            MeAnte meAnte;
            SQLLEN[] nArrayMDB; //[ME_ANTE_SIZE_];
            int nHandle;
            int nRet;

            GenUtil.UtCvtName(Constant.FE_ANTE, pdfName, out tableName);

            // read antenna information 
            if ((aID1 = DynFeAnte.FeSelectAnte(tableName, "", "")) < 0)
            {
                ValErrs.AddMess("MERGE 4 - could not read antenna information. Reason: %d",
                                pdfName, "W", aID1.ToString());
                return;
            }

            while (DynFeAnte.FeFetchAnte(aID1, out feAnte, out nArrayFW) == Constant.SUCCESS)
            {

                if (feAnte.cmd.Equals("N"))
                {
                    ValImport.FeFormWhereClause(out whereClauseLoc, feAnte.location, feAnte.call1, "");

                    nHandle = DynMeAnte.MeSelectAnte(whereClauseLoc, "call1");

                    nRet = DynMeAnte.MeFetchAnte(nHandle, out meAnte, out nArrayMDB);

                    DynMeAnte.MeCloseAnte(nHandle);

                    if (nRet == 0)
                    {
                        // MDB record found 
                        FeValCopy.FeCopyAnte(ref feAnte, meAnte, ref nArrayFW, nArrayMDB);

                        if ((nRet = DynFeAnte.FeUpdateAnte(aID1, feAnte, nArrayFW)) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("MERGE 5 - could not update antenna information. Reason: %d",
                                            pdfName, "W", nRet.ToString());
                        }
                    }
                    continue;
                }

                ValImport.FeFormWhereClause(out whereClauseLoc, feAnte.location, "", "");

                whereClauseCall = String.Format("location= '{0}' and call1 = '{1}' ",
                                feAnte.location, feAnte.call1);

                ValImport.FeImportSite(pdfName, whereClauseLoc, "N");

                // if delete get all chans + azims as 'D', site as 'N' 
                if (feAnte.cmd.Equals("D"))
                {
                    ValImport.FeImportChannel(pdfName, whereClauseCall, "D");
                    ValImport.FeImportAzimuth(pdfName, whereClauseCall, "D");
                }

                if ((!feAnte.cmd.Equals("U")) && (!feAnte.cmd.Equals("B")))
                {
                    // if not Update or Blank (ie. Add or Del) dont merge 
                    continue;
                }

                // fetch the corresponding MDB record 
                nHandle = DynMeAnte.MeSelectAnte(whereClauseCall, null);

                nRet = DynMeAnte.MeFetchAnte(nHandle, out meAnte, out nArrayMDB);

                DynMeAnte.MeCloseAnte(nHandle);

                if (nRet != 0)
                {
                    // if MDB record not found, get next from FW 
                    continue;
                }

                if (feAnte.cmd.Equals("U"))
                {
                    /* Updating record so pull in info from mdb -
                     * not done when blanking is the command
                     */
                    if (nArrayFW[FeAnte.LOCATION] == Constant.DB_NULL)
                    {
                        feAnte.location = meAnte.location;
                        nArrayFW[FeAnte.LOCATION] = nArrayMDB[MeAnte.LOCATION];
                    }
                    if (nArrayFW[FeAnte.CALL1] == Constant.DB_NULL)
                    {
                        feAnte.call1 = meAnte.call1;
                        nArrayFW[FeAnte.CALL1] = nArrayMDB[MeAnte.CALL1];
                    }
                    if (nArrayFW[FeAnte.RXBAND] == Constant.DB_NULL)
                    {
                        feAnte.rxband = meAnte.rxband;
                        nArrayFW[FeAnte.RXBAND] = nArrayMDB[MeAnte.RXBAND];
                    }
                    if (nArrayFW[FeAnte.TXBAND] == Constant.DB_NULL)
                    {
                        feAnte.txband = meAnte.txband;
                        nArrayFW[FeAnte.TXBAND] = nArrayMDB[MeAnte.TXBAND];
                    }
                    if (nArrayFW[FeAnte.ACODETX] == Constant.DB_NULL)
                    {
                        feAnte.acodetx = meAnte.acodetx;
                        nArrayFW[FeAnte.ACODETX] = nArrayMDB[MeAnte.ACODETX];
                    }
                    if (nArrayFW[FeAnte.ACODERX] == Constant.DB_NULL)
                    {
                        feAnte.acoderx = meAnte.acoderx;
                        nArrayFW[FeAnte.ACODERX] = nArrayMDB[MeAnte.ACODERX];
                    }
                    if (nArrayFW[FeAnte.G_T] == Constant.DB_NULL)
                    {
                        feAnte.g_t = meAnte.g_t;
                        nArrayFW[FeAnte.G_T] = nArrayMDB[MeAnte.G_T];
                    }
                    if (nArrayFW[FeAnte.LNAT] == Constant.DB_NULL)
                    {
                        feAnte.lnat = meAnte.lnat;
                        nArrayFW[FeAnte.LNAT] = nArrayMDB[MeAnte.LNAT];
                    }
                    if (nArrayFW[FeAnte.AHT] == Constant.DB_NULL)
                    {
                        feAnte.aht = meAnte.aht;
                        nArrayFW[FeAnte.AHT] = nArrayMDB[MeAnte.AHT];
                    }
                    if (nArrayFW[FeAnte.AFSLT] == Constant.DB_NULL)
                    {
                        feAnte.afslt = meAnte.afslt;
                        nArrayFW[FeAnte.AFSLT] = nArrayMDB[MeAnte.AFSLT];
                    }
                    if (nArrayFW[FeAnte.AFSLR] == Constant.DB_NULL)
                    {
                        feAnte.afslr = meAnte.afslr;
                        nArrayFW[FeAnte.AFSLR] = nArrayMDB[MeAnte.AFSLR];
                    }
                    if (nArrayFW[FeAnte.TXHGMAX] == Constant.DB_NULL)
                    {
                        feAnte.txhgmax = meAnte.txhgmax;
                        nArrayFW[FeAnte.TXHGMAX] = nArrayMDB[MeAnte.TXHGMAX];
                    }
                    if (nArrayFW[FeAnte.RXHGMAX] == Constant.DB_NULL)
                    {
                        feAnte.rxhgmax = meAnte.rxhgmax;
                        nArrayFW[FeAnte.RXHGMAX] = nArrayMDB[MeAnte.RXHGMAX];
                    }
                    if (nArrayFW[FeAnte.SATLONGIT] == Constant.DB_NULL)
                    {
                        feAnte.satlongit = meAnte.satlongit;
                        nArrayFW[FeAnte.SATLONGIT] = nArrayMDB[MeAnte.SATLONGIT];
                    }
                    if (nArrayFW[FeAnte.SATLONG] == Constant.DB_NULL)
                    {
                        feAnte.satlong = meAnte.satlong;
                        nArrayFW[FeAnte.SATLONG] = nArrayMDB[MeAnte.SATLONG];
                    }
                    if (nArrayFW[FeAnte.SATLONGS] == Constant.DB_NULL)
                    {
                        feAnte.satlongs = meAnte.satlongs;
                        nArrayFW[FeAnte.SATLONGS] = nArrayMDB[MeAnte.SATLONGS];
                    }
                    if (nArrayFW[FeAnte.AZ] == Constant.DB_NULL)
                    {
                        feAnte.az = meAnte.az;
                        nArrayFW[FeAnte.AZ] = nArrayMDB[MeAnte.AZ];
                    }
                    if (nArrayFW[FeAnte.EL] == Constant.DB_NULL)
                    {
                        feAnte.el = meAnte.el;
                        nArrayFW[FeAnte.EL] = nArrayMDB[MeAnte.EL];
                    }
                    if (nArrayFW[FeAnte.SARC1] == Constant.DB_NULL)
                    {
                        feAnte.sarc1 = meAnte.sarc1;
                        nArrayFW[FeAnte.SARC1] = nArrayMDB[MeAnte.SARC1];
                    }
                    if (nArrayFW[FeAnte.SARC2] == Constant.DB_NULL)
                    {
                        feAnte.sarc2 = meAnte.sarc2;
                        nArrayFW[FeAnte.SARC2] = nArrayMDB[MeAnte.SARC2];
                    }
                    if (nArrayFW[FeAnte.RXPRE] == Constant.DB_NULL)
                    {
                        feAnte.rxpre = meAnte.rxpre;
                        nArrayFW[FeAnte.RXPRE] = nArrayMDB[MeAnte.RXPRE];
                    }
                    if (nArrayFW[FeAnte.TXPRE] == Constant.DB_NULL)
                    {
                        feAnte.txpre = meAnte.txpre;
                        nArrayFW[FeAnte.TXPRE] = nArrayMDB[MeAnte.TXPRE];
                    }
                    if (nArrayFW[FeAnte.RXTRO] == Constant.DB_NULL)
                    {
                        feAnte.rxtro = meAnte.rxtro;
                        nArrayFW[FeAnte.RXTRO] = nArrayMDB[MeAnte.RXTRO];
                    }
                    if (nArrayFW[FeAnte.TXTRO] == Constant.DB_NULL)
                    {
                        feAnte.txtro = meAnte.txtro;
                        nArrayFW[FeAnte.TXTRO] = nArrayMDB[MeAnte.TXTRO];
                    }
                    if (nArrayFW[FeAnte.LICENCE] == Constant.DB_NULL)
                    {
                        feAnte.licence = meAnte.licence;
                        nArrayFW[FeAnte.LICENCE] = nArrayMDB[MeAnte.LICENCE];
                    }
                    if (nArrayFW[FeAnte.SATNAME] == Constant.DB_NULL)
                    {
                        feAnte.satname = meAnte.satname;
                        nArrayFW[FeAnte.SATNAME] = nArrayMDB[MeAnte.SATNAME];
                    }
                    if (nArrayFW[FeAnte.STATA] == Constant.DB_NULL)
                    {
                        feAnte.stata = meAnte.stata;
                        nArrayFW[FeAnte.STATA] = nArrayMDB[MeAnte.STATA];
                    }
                    if (nArrayFW[FeAnte.NOTA] == Constant.DB_NULL)
                    {
                        feAnte.nota = meAnte.nota;
                        nArrayFW[FeAnte.NOTA] = nArrayMDB[MeAnte.NOTA];
                    }
                    if (nArrayFW[FeAnte.OP2] == Constant.DB_NULL)
                    {
                        feAnte.op2 = meAnte.op2;
                        nArrayFW[FeAnte.OP2] = nArrayMDB[MeAnte.OP2];
                    }
                    if (nArrayFW[FeAnte.ANTREF] == Constant.DB_NULL)
                    {
                        feAnte.antref = meAnte.antref;
                        nArrayFW[FeAnte.ANTREF] = nArrayMDB[MeAnte.ANTREF];
                    }
                }
                if (nArrayFW[FeAnte.MDATE] == Constant.DB_NULL)
                {
                    feAnte.mdate = meAnte.mdate;
                    nArrayFW[FeAnte.MDATE] = nArrayMDB[MeAnte.MDATE];
                }
                if (nArrayFW[FeAnte.MTIME] == Constant.DB_NULL)
                {
                    feAnte.mtime = meAnte.mtime;
                    nArrayFW[FeAnte.MTIME] = nArrayMDB[MeAnte.MTIME];
                }

                if (DynFeAnte.FeUpdateAnte(aID1, feAnte, nArrayFW) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }

                pullrecs = CheckAnteExtractRules(feAnte, ref azimRecChanged);
                if (pullrecs == true)
                {
                    ValImport.FeImportChannel(pdfName, whereClauseCall, "N");
                    ValImport.FeImportAzimuth(pdfName, whereClauseCall, "N");
                }
            }

            DynFeAnte.FeCloseAnte(aID1);

            //...Log2.v("\nValMerge.FeValMergeAnteRecords: Exit");
        }

        private static MeAnte meAnte;   // MDB antenna structure 

        /// <summary>
        /// This method determines if any of the required (mandated) fields of a PDF
        /// antenna record differ from those in the MDB me_ante table.  
        /// </summary>
        /// <param name="feAnte"> - antenna information, an instance of FeAnte.</param>
        /// <param name="azimRecChanged"> - returns true if a difference is found.</param>
        /// <returns></returns>
        static bool CheckAnteExtractRules(FeAnte feAnte, ref bool azimRecChanged)
        {
            //...Log2.v("\n\nValMerge.CheckAnteExtractRules(): Entry");

            SQLLEN[] nullInd; //[ME_ANTE_SIZE_];
            bool pulled = false; // by default do not pull in MDB records 
            string cWhere;

            int nHandle;
            int nRet;

            // select the same mdb records that was passed down as a pdf antenna record 
            ValImport.FeFormWhereClause(out cWhere, feAnte.location, feAnte.call1, "");

            nHandle = DynMeAnte.MeSelectAnte(cWhere, null);

            nRet = DynMeAnte.MeFetchAnte(nHandle, out meAnte, out nullInd);

            DynMeAnte.MeCloseAnte(nHandle);

            // If we found that record the following query looks really scary, however,
            // the principle is very simple.  Essentially we are trying to determine if 
            // a pdf record we pass down to this function is different from the copy in 
            // the MDB. Thus the IF statement examines each required field and check the 
            // pdf copy and mdb copy are not the same.
            if (nRet == 0)
            {
                if (((nullInd[MeAnte.ACODETX] != Constant.DB_NULL) &&
                         (!feAnte.acodetx.Equals(meAnte.acodetx))) ||
                        ((nullInd[MeAnte.ACODERX] != Constant.DB_NULL) &&
                         (!feAnte.acoderx.Equals(meAnte.acoderx))) ||
                        ((nullInd[MeAnte.AHT] != Constant.DB_NULL) &&
                         (feAnte.aht != meAnte.aht)) ||
                        ((nullInd[MeAnte.AFSLT] != Constant.DB_NULL) &&
                         (feAnte.afslt != meAnte.afslt)) ||
                        ((nullInd[MeAnte.AFSLR] != Constant.DB_NULL) &&
                         (feAnte.afslr != meAnte.afslr)) ||
                        ((nullInd[MeAnte.SATLONG] != Constant.DB_NULL) &&
                         (feAnte.satlong != meAnte.satlong)) ||
                        ((nullInd[MeAnte.SATLONGS] != Constant.DB_NULL) &&
                         (!feAnte.satlongs.Equals(meAnte.satlongs))) ||
                        ((nullInd[MeAnte.SARC1] != Constant.DB_NULL) &&
                         (feAnte.sarc1 != meAnte.sarc1)) ||
                        ((nullInd[MeAnte.SARC2] != Constant.DB_NULL) &&
                         (feAnte.sarc2 != meAnte.sarc2)) ||
                        ((nullInd[MeAnte.RXPRE] != Constant.DB_NULL) &&
                         (feAnte.rxpre != meAnte.rxpre)) ||
                        ((nullInd[MeAnte.TXPRE] != Constant.DB_NULL) &&
                         (feAnte.txpre != meAnte.txpre)) ||
                        ((nullInd[MeAnte.RXTRO] != Constant.DB_NULL) &&
                         (feAnte.rxtro != meAnte.rxtro)) ||
                        ((nullInd[MeAnte.TXTRO] != Constant.DB_NULL) &&
                         (feAnte.txtro != meAnte.txtro)) ||
                        ((nullInd[MeAnte.STATA] != Constant.DB_NULL) &&
                         (!feAnte.stata.Equals(meAnte.stata))) ||
                        ((nullInd[MeAnte.OP2] != Constant.DB_NULL) &&
                         (!feAnte.op2.Equals(meAnte.op2))))
                {
                    azimRecChanged = true;
                    pulled = true; // if the records are different we must pull in MDB records.
                }

            }

            //...Log2.v("\nValMerge.CheckAnteExtractRules(): Exit");
            return (pulled);
        }

        /// <summary>
        /// This method merges information in the PDF's _chan table with
        /// that of a channel with the same location, callsign and channel ID in the MDB table me_chan i.a.w. 
        /// the MDB operation command in the PDF record; the merged channel information is written
        /// back to the PDF's _chan table.
        /// </summary>
        /// <remarks>
        /// \section sectc Channel merging by cmd type: 
        /// This method manages the merging of channel information from the MDB and from the PDF 
        /// i.a.w. the MDB operation command in the PDF record, as described below:
        /// \subsection subsect00c if (feChan.cmd.Equals("B"))
        /// If the channel record's MDB operation is BLANK-OUT then it is ignored; nothing in the PDF data set changes.
        /// \subsection subsect01c if (feChan.cmd.Equals("N"))
        /// When a channel record's MDB operation cmd is NO-CHANGE (NOOP) all of its fields are
        /// overwritten by the field values for that channel in the MDB site table.
        /// \subsection subsect2c if (feChan.cmd.Equals("A"))
        /// If the PDF wants to ADD a channel record then its information is left unchanged.
        /// \subsection subsect3c if (feSite.cmd.Equals("D"))
        /// If the PDF wants to DELETE a channel record then its information is left unchanged.
        /// \subsection subsect4c if (feSite.cmd.Equals("U"))
        /// If the PDF wants to UPDATE a channel record then the channel record's fields are parsed to identify
        /// any that are blank (i.e. have ODBC NULL); these blank fields are then populated with
        /// the field values of the channel with the same location, callsign and channel ID from the MDB me_chan table.
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF.</param>
        public static void FeValMergeChanRecords(string pdfName)
        {
            //...Log2.v("\n\nValMerge.FeValMergeChanRecords(): Entry");

            SQLLEN[] nArrayFW; //[FeChan.SIZE_];
            int cID1;
            bool pullrecs = false;
            string tableName;
            string whereClauseLoc; //[WHERE_SIZE],
            string whereClauseCall; //[WHERE_SIZE];
            FeChan feChan;
            MeChan meChan;
            SQLLEN[] nArrayMDB; //[ME_CHAN_SIZE_];
            int nRet;
            int nHandle;

            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out tableName);

            // read Channel information 
            if ((cID1 = DynFeChan.FeSelectChan(tableName, "", "")) < 0)
            {
                ValErrs.AddMess("MERGE 6 - could not select channel information. Reason: %d",
                                pdfName, "W", cID1.ToString());
                return;
            }

            while (DynFeChan.FeFetchChan(cID1, out feChan, out nArrayFW) == Constant.SUCCESS)
            {

                if (feChan.cmd.Equals("N"))
                {
                    ValImport.FeFormWhereClause(out whereClauseLoc, feChan.location, feChan.call1, feChan.chid);

                    nHandle = DynMeChan.MeSelectChan(whereClauseLoc, null);

                    nRet = DynMeChan.MeFetchChan(nHandle, out meChan, out nArrayMDB);

                    DynMeChan.MeCloseChan(nHandle);

                    if (nRet == 0)
                    {
                        // MDB record found 
                        FeValCopy.FeCopyChan(ref feChan, meChan, ref nArrayFW, nArrayMDB);

                        if ((nRet = DynFeChan.FeUpdateChan(cID1, feChan, nArrayFW)) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("MERGE 7 - could not update channel information. Reason: %d",
                                            pdfName, "W", nRet.ToString());
                        }
                    }
                    continue;
                }

                // pull in above site 
                ValImport.FeFormWhereClause(out whereClauseLoc, feChan.location, "", "");
                whereClauseCall = String.Format("location = '{0}' and call1 = '{1}' ",
                                feChan.location,
                                feChan.call1);

                ValImport.FeImportSite(pdfName, whereClauseLoc, "N");

                ValImport.FeImportAntenna(pdfName, whereClauseCall, "N");

                if ((!feChan.cmd.Equals("U")) && (!feChan.cmd.Equals("B")))
                {
                    // if not Update or Blank (ie. Add or Del) dont merge 
                    continue;
                }

                // fetch the corresponding MDB record 
                ValImport.FeFormWhereClause(out whereClauseLoc, feChan.location, feChan.call1, feChan.chid);

                nHandle = DynMeChan.MeSelectChan(whereClauseLoc, null);

                nRet = DynMeChan.MeFetchChan(nHandle, out meChan, out nArrayMDB);

                DynMeChan.MeCloseChan(nHandle);

                if (nRet != 0)
                {
                    // if MDB record not found, get next from FW 
                    continue;
                }

                if (feChan.cmd.Equals("U"))
                {
                    /* Updating record so pull in info from mdb -
                     * not done when blanking is the command
                     */
                    if (nArrayFW[FeChan.LOCATION] == Constant.DB_NULL)
                    {
                        feChan.location = meChan.location;
                        nArrayFW[FeChan.LOCATION] = nArrayMDB[MeChan.LOCATION];
                    }
                    if (nArrayFW[FeChan.CALL1] == Constant.DB_NULL)
                    {
                        feChan.call1 = meChan.call1;
                        nArrayFW[FeChan.CALL1] = nArrayMDB[MeChan.CALL1];
                    }
                    if (nArrayFW[FeChan.CHID] == Constant.DB_NULL)
                    {
                        feChan.chid = meChan.chid;
                        nArrayFW[FeChan.CHID] = nArrayMDB[MeChan.CHID];
                    }
                    if (nArrayFW[FeChan.FREQTX] == Constant.DB_NULL)
                    {
                        feChan.freqtx = meChan.freqtx;
                        nArrayFW[FeChan.FREQTX] = nArrayMDB[MeChan.FREQTX];
                    }
                    if (nArrayFW[FeChan.POLTX] == Constant.DB_NULL)
                    {
                        feChan.poltx = meChan.poltx;
                        nArrayFW[FeChan.POLTX] = nArrayMDB[MeChan.POLTX];
                    }
                    if (nArrayFW[FeChan.MAXTXPOWER] == Constant.DB_NULL)
                    {
                        feChan.maxtxpower = meChan.maxtxpower;
                        nArrayFW[FeChan.MAXTXPOWER] = nArrayMDB[MeChan.MAXTXPOWER];
                    }
                    if (nArrayFW[FeChan.PWRTX] == Constant.DB_NULL)
                    {
                        feChan.pwrtx = meChan.pwrtx;
                        nArrayFW[FeChan.PWRTX] = nArrayMDB[MeChan.PWRTX];
                    }
                    if (nArrayFW[FeChan.P4KHZ] == Constant.DB_NULL)
                    {
                        feChan.p4khz = meChan.p4khz;
                        nArrayFW[FeChan.P4KHZ] = nArrayMDB[MeChan.P4KHZ];
                    }
                    if (nArrayFW[FeChan.EQPTTX] == Constant.DB_NULL)
                    {
                        feChan.eqpttx = meChan.eqpttx;
                        nArrayFW[FeChan.EQPTTX] = nArrayMDB[MeChan.EQPTTX];
                    }
                    if (nArrayFW[FeChan.TRAFTX] == Constant.DB_NULL)
                    {
                        feChan.traftx = meChan.traftx;
                        nArrayFW[FeChan.TRAFTX] = nArrayMDB[MeChan.TRAFTX];
                    }
                    if (nArrayFW[FeChan.STATTX] == Constant.DB_NULL)
                    {
                        feChan.stattx = meChan.stattx;
                        nArrayFW[FeChan.STATTX] = nArrayMDB[MeChan.STATTX];
                    }
                    if (nArrayFW[FeChan.FEETX] == Constant.DB_NULL)
                    {
                        feChan.feetx = meChan.feetx;
                        nArrayFW[FeChan.FEETX] = nArrayMDB[MeChan.FEETX];
                    }
                    if (nArrayFW[FeChan.FREQRX] == Constant.DB_NULL)
                    {
                        feChan.freqrx = meChan.freqrx;
                        nArrayFW[FeChan.FREQRX] = nArrayMDB[MeChan.FREQRX];
                    }
                    if (nArrayFW[FeChan.POLRX] == Constant.DB_NULL)
                    {
                        feChan.polrx = meChan.polrx;
                        nArrayFW[FeChan.POLRX] = nArrayMDB[MeChan.POLRX];
                    }
                    if (nArrayFW[FeChan.PWRRX] == Constant.DB_NULL)
                    {
                        feChan.pwrrx = meChan.pwrrx;
                        nArrayFW[FeChan.PWRRX] = nArrayMDB[MeChan.PWRRX];
                    }
                    if (nArrayFW[FeChan.EQPTRX] == Constant.DB_NULL)
                    {
                        feChan.eqptrx = meChan.eqptrx;
                        nArrayFW[FeChan.EQPTRX] = nArrayMDB[MeChan.EQPTRX];
                    }
                    if (nArrayFW[FeChan.TRAFRX] == Constant.DB_NULL)
                    {
                        feChan.trafrx = meChan.trafrx;
                        nArrayFW[FeChan.TRAFRX] = nArrayMDB[MeChan.TRAFRX];
                    }
                    if (nArrayFW[FeChan.STATRX] == Constant.DB_NULL)
                    {
                        feChan.statrx = meChan.statrx;
                        nArrayFW[FeChan.STATRX] = nArrayMDB[MeChan.STATRX];
                    }
                    if (nArrayFW[FeChan.I20] == Constant.DB_NULL)
                    {
                        feChan.i20 = meChan.i20;
                        nArrayFW[FeChan.I20] = nArrayMDB[MeChan.I20];
                    }
                    if (nArrayFW[FeChan.IT01] == Constant.DB_NULL)
                    {
                        feChan.it01 = meChan.it01;
                        nArrayFW[FeChan.IT01] = nArrayMDB[MeChan.IT01];
                    }
                    if (nArrayFW[FeChan.IP01] == Constant.DB_NULL)
                    {
                        feChan.ip01 = meChan.ip01;
                        nArrayFW[FeChan.IP01] = nArrayMDB[MeChan.IP01];
                    }
                    if (nArrayFW[FeChan.FEERX] == Constant.DB_NULL)
                    {
                        feChan.feerx = meChan.feerx;
                        nArrayFW[FeChan.FEERX] = nArrayMDB[MeChan.FEERX];
                    }
                    if (nArrayFW[FeChan.NOTC] == Constant.DB_NULL)
                    {
                        feChan.notc = meChan.notc;
                        nArrayFW[FeChan.NOTC] = nArrayMDB[MeChan.NOTC];
                    }
                    if (nArrayFW[FeChan.SRVCTX] == Constant.DB_NULL)
                    {
                        feChan.srvctx = meChan.srvctx;
                        nArrayFW[FeChan.SRVCTX] = nArrayMDB[MeChan.SRVCTX];
                    }
                    if (nArrayFW[FeChan.SRVCRX] == Constant.DB_NULL)
                    {
                        feChan.srvcrx = meChan.srvcrx;
                        nArrayFW[FeChan.SRVCRX] = nArrayMDB[MeChan.SRVCRX];
                    }
                }
                if (nArrayFW[FeChan.MDATE] == Constant.DB_NULL)
                {
                    feChan.mdate = meChan.mdate;
                    nArrayFW[FeChan.MDATE] = nArrayMDB[MeChan.MDATE];
                }
                if (nArrayFW[FeChan.MTIME] == Constant.DB_NULL)
                {
                    feChan.mtime = meChan.mtime;
                    nArrayFW[FeChan.MTIME] = nArrayMDB[MeChan.MTIME];
                }

                if ((nRet = DynFeChan.FeUpdateChan(cID1, feChan, nArrayFW)) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("MERGE 7 - could not update channel information. Reason: %d",
                                    pdfName, "W", nRet.ToString());
                }

                pullrecs = CheckChanExtractRules(feChan);
                if (pullrecs == true)
                {
                    ValImport.FeImportAzimuth(pdfName, whereClauseCall, "N");
                }
            }
            DynFeChan.FeCloseChan(cID1);

            //...Log2.v("\nValMerge.FeValMergeChanRecords(): Exit");
        }

        /// <summary>
        /// This method determines if any of the required (mandated) fields of a PDF
        /// channel record differ from those in the MDB me_chan table.  
        /// </summary>
        /// <param name="feChan"> - channel information, an instance of FeChan.</param>
        /// <returns> true if a difference is found.</returns>
        public static bool CheckChanExtractRules(FeChan feChan)
        {
            //...Log2.v("\n\nValMerge.CheckChanExtractRules(): Entry");

            MeChan meChan;
            SQLLEN[] nullInd; //[MeChan.SIZE_];
            bool pulled = false;
            string cWhere;
            int nHandle;
            int nRet;

            /* select the same mdb records that was passed down as a
               pdf antenna record */
            ValImport.FeFormWhereClause(out cWhere, feChan.location, feChan.call1, feChan.chid);

            nHandle = DynMeChan.MeSelectChan(cWhere, null);

            nRet = DynMeChan.MeFetchChan(nHandle, out meChan, out nullInd);

            DynMeChan.MeCloseChan(nHandle);

            if (nRet == 0)
            {
                /* the following query looks really scary, however,
                   the principle is very simple.  Essentially we are
                   trying to determine if a pdf record we pass down to
                   this function is different from the copy in the MDB.
                   Thus the IF statement examines each required field
                   and check the pdf copy and mdb copy are not the same.
                */
                if (((feChan.freqtx != meChan.freqtx) &&
                         (nullInd[MeChan.FREQTX] != Constant.DB_NULL)) ||
                        ((!feChan.poltx.Equals(meChan.poltx)) &&
                         (nullInd[MeChan.POLTX] != Constant.DB_NULL)) ||
                        ((feChan.maxtxpower != meChan.maxtxpower) &&
                         (nullInd[MeChan.MAXTXPOWER] != Constant.DB_NULL)) ||
                        ((feChan.pwrtx != meChan.pwrtx) &&
                         (nullInd[MeChan.PWRTX] != Constant.DB_NULL)) ||
                        ((feChan.p4khz != meChan.p4khz) &&
                         (nullInd[MeChan.P4KHZ] != Constant.DB_NULL)) ||
                        ((!feChan.eqpttx.Equals(meChan.eqpttx)) &&
                         (nullInd[MeChan.EQPTTX] != Constant.DB_NULL)) ||
                        ((!feChan.traftx.Equals(meChan.traftx)) &&
                         (nullInd[MeChan.TRAFTX] != Constant.DB_NULL)) ||
                        ((!feChan.stattx.Equals(meChan.stattx)) &&
                         (nullInd[MeChan.STATTX] != Constant.DB_NULL)) ||
                        ((feChan.freqrx != meChan.freqrx) &&
                         (nullInd[MeChan.FREQTX] != Constant.DB_NULL)) ||
                        ((!feChan.polrx.Equals(meChan.polrx)) &&
                         (nullInd[MeChan.POLTX] != Constant.DB_NULL)) ||
                        ((feChan.pwrrx != meChan.pwrrx) &&
                         (nullInd[MeChan.PWRRX] != Constant.DB_NULL)) ||
                        ((!feChan.eqptrx.Equals(meChan.eqptrx)) &&
                         (nullInd[MeChan.EQPTRX] != Constant.DB_NULL)) ||
                        ((!feChan.trafrx.Equals(meChan.trafrx)) &&
                         (nullInd[MeChan.TRAFRX] != Constant.DB_NULL)) ||
                        ((!feChan.statrx.Equals(meChan.statrx)) &&
                         (nullInd[MeChan.STATRX] != Constant.DB_NULL)) ||
                        ((feChan.i20 != meChan.i20) &&
                         (nullInd[MeChan.I20] != Constant.DB_NULL)) ||
                        ((feChan.it01 != meChan.it01) &&
                         (nullInd[MeChan.IT01] != Constant.DB_NULL)) ||
                        ((feChan.ip01 != meChan.ip01) &&
                         (nullInd[MeChan.IP01] != Constant.DB_NULL)))
                {
                    pulled = true; // must pull in MDB recs 
                }
            }

            //...Log2.v("\nValMerge.CheckChanExtractRules(): Exit");
            return (pulled);
        }

        /// <summary>
        /// This method merges azimuth records in the PDF's _azim table with
        /// that of azimuth records with the same location, callsign and azimuth in the MDB table me_azim i.a.w. 
        /// the MDB operation command in the PDF record; the merged azimuth information is written
        /// back to the PDF's _azim table.
        /// </summary>
        /// <remarks>
        /// \section sectaz Azimuth merging by cmd type: 
        /// This method manages the merging of azimuth information from the MDB and from the PDF 
        /// i.a.w. the MDB operation command in the PDF record, as described below:
        /// \subsection subsect00az if (feAzim.cmd.Equals("B"))
        /// If the azimuth record's MDB operation is BLANK-OUT then it is ignored; nothing in the PDF data set changes.
        /// \subsection subsect01az if (feAzim.cmd.Equals("N"))
        /// When an azimuth record's MDB operation cmd is NO-CHANGE (NOOP) all of its fields are
        /// overwritten by the field values for that azimuth in the MDB site table. We must also ensure that 
        /// all the site and antenna records that reference the location and callsign  are also 'pulled into' 
        /// the PDF data set.  The following methods are called in succession:
        /// <list type="bullet">
        /// <item>ValImport.FeImportSite : This methods checks whether prescribed site records 
        /// (selected by the 'whereClause') exist in the PDF's _site table;  : if an individual 
        /// site record is not in the PDF's _site table then it is imported (copied) in from the MDB's 
        /// me_site table. The imported record's MDB operation cmd is set to "N".</item>
        /// <item>ValImport.FeImportAntenna : This methods checks whether prescribed antenna records 
        /// (selected by the 'whereClause') exist in the PDF's _ante table;  : if an individual 
        /// azimuth record is not in the PDF's _ante table then it is imported (copied) in from the MDB's 
        /// me_ante table. The imported record's MDB operation cmd is set to "N".</item>
        /// </list>
        /// \subsection subsect2az if (feAzim.cmd.Equals("A"))
        /// If the PDF wants to ADD an azimuth record then its information is left unchanged.
        /// \subsection subsect3az if (feSite.cmd.Equals("D"))
        /// If the PDF wants to DELETE an azimuth record we must ensure that all the site and antenna records that
        /// reference the location and callsign  are also 'pulled into' the PDF data set using the same
        /// worker-methods as the "N" case to import the site and antenna records from the MDB.
        /// \subsection subsectaz if (feSite.cmd.Equals("U"))
        /// If the PDF wants to UPDATE an azimuth record then the azimuth record's fields are parsed to identify
        /// any that are blank (i.e. have ODBC NULL); these blank fields are then populated with
        /// the field values of the azimuth with the same location, callsign and azimuth from the MDB me_azim table.
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF.</param>
        public static void FeValMergeAzimRecords(string pdfName)  // pdf display name 
        {
            //...Log2.v("\n\nValMerge.FeValMergeAzimRecords: Entry");

            SQLLEN[] feAzimNullInds; //[FeAzim.SIZE_];
            int aID1;
            string tableName;
            string whereClause;
            FeAzim feAzim;
            MeAzim meAzim;
            SQLLEN[] meAzimNullInds; //[MeAzim.SIZE_];
            int nRet;
            int nHandle;

            GenUtil.UtCvtName(Constant.FE_AZIM, pdfName, out tableName);

            /* can't merge azimuth pdf records with mdb records at same time as
             * records are imported from the mdb since azimuth import needs the
             * records sorted and merge needs to update the pdf records.  These
             * two functions, sorting and updating cannot be
             * done with the same cursor. Therefore, merge first then import
             */
            // read Azimuth information 
            if ((aID1 = DynFeAzim.FeSelectAzim(tableName, "", "")) < 0)
            {
                ValErrs.AddMess("MERGE 8 - could not read azimuth information. Reason: %d",
                                pdfName, "W", aID1.ToString());
                return;
            }

            while (DynFeAzim.FeFetchAzim(aID1, out feAzim, out feAzimNullInds) == Constant.SUCCESS)
            {

                // If the cmd is NOOP overwrite the records in the PDF's _azim table with
                // records from the MDB me_azim table that have the same location, callsign and 
                // azimuth.
                if (feAzim.cmd.Equals("N"))
                {
                    whereClause = String.Format("location='{0}' and call1='{1}' and azim='{2}'",
                                        feAzim.location, feAzim.call1, feAzim.azim);

                    nHandle = DynMeAzim.MeSelectAzim(whereClause, null);

                    nRet = DynMeAzim.MeFetchAzim(nHandle, out meAzim, out meAzimNullInds);

                    DynMeAzim.MeCloseAzim(nHandle);

                    if (nRet == 0)
                    {
                        // MDB record found 
                        FeValCopy.FeCopyAzim(ref feAzim, meAzim, ref feAzimNullInds, meAzimNullInds);

                        if ((nRet = DynFeAzim.FeUpdateAzim(aID1, feAzim, feAzimNullInds)) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("MERGE 9 - could not update azimuth information. Reason: %d",
                                            pdfName, "W", nRet.ToString());
                        }
                    }
                }

                if (feAzim.cmd.Equals("A"))
                {
                    continue;
                }

                // We are now left with the cases: B, D, N, U

                // If the PDF wants to DELETE an azim we must ensure that all the site and antenna records that
                // reference the location and callsign  are also 'pulled into' the PDF data set.
                if (feAzim.cmd.Equals("D"))
                {
                    // pull in above site 
                    ValImport.FeFormWhereClause(out whereClause, feAzim.location, "", "");
                    ValImport.FeImportSite(pdfName, whereClause, "N");

                    // pull in antennas 
                    whereClause = String.Format("location = '{0}' and call1 = '{1}' ",
                                    feAzim.location,
                                    feAzim.call1);
                    ValImport.FeImportAntenna(pdfName, whereClause, "N");
                }

                // fetch the corresponding MDB record 
                whereClause = String.Format("location='{0}' and call1='{1}' and azim='{2}'",
                                    feAzim.location, feAzim.call1, feAzim.azim);

                nHandle = DynMeAzim.MeSelectAzim(whereClause, null);

                nRet = DynMeAzim.MeFetchAzim(nHandle, out meAzim, out meAzimNullInds);

                DynMeAzim.MeCloseAzim(nHandle);

                if (nRet != 0)
                {
                    // if MDB record not found, get next from FW 
                    continue;
                }

                if (feAzim.cmd.Equals("U"))
                {
                    /* Updating record so pull in info from mdb -
                     * not done when blanking is the command
                     */
                    if (feAzimNullInds[FeAzim.LOCATION] == Constant.DB_NULL)
                    {
                        feAzim.location = meAzim.location;
                        feAzimNullInds[FeAzim.LOCATION] = meAzimNullInds[MeAzim.LOCATION];
                    }
                    if (feAzimNullInds[FeAzim.CALL1] == Constant.DB_NULL)
                    {
                        feAzim.call1 = meAzim.call1;
                        feAzimNullInds[FeAzim.CALL1] = meAzimNullInds[MeAzim.CALL1];
                    }
                    if (feAzimNullInds[FeAzim.AZIM] == Constant.DB_NULL)
                    {
                        feAzim.azim = meAzim.azim;
                        feAzimNullInds[FeAzim.AZIM] = meAzimNullInds[MeAzim.AZIM];
                    }
                    if (feAzimNullInds[FeAzim.ELEV] == Constant.DB_NULL)
                    {
                        feAzim.elev = meAzim.elev;
                        feAzimNullInds[FeAzim.ELEV] = meAzimNullInds[MeAzim.ELEV];
                    }
                    if (feAzimNullInds[FeAzim.DIST] == Constant.DB_NULL)
                    {
                        feAzim.dist = meAzim.dist;
                        feAzimNullInds[FeAzim.DIST] = meAzimNullInds[MeAzim.DIST];
                    }
                    if (feAzimNullInds[FeAzim.LOSS] == Constant.DB_NULL)
                    {
                        feAzim.loss = meAzim.loss;
                        feAzimNullInds[FeAzim.LOSS] = meAzimNullInds[MeAzim.LOSS];
                    }
                }

                if (feAzimNullInds[FeAzim.MDATE] == Constant.DB_NULL)
                {
                    feAzim.mdate = meAzim.mdate;
                    feAzimNullInds[FeAzim.MDATE] = meAzimNullInds[MeAzim.MDATE];
                }
                if (feAzimNullInds[FeAzim.MTIME] == Constant.DB_NULL)
                {
                    feAzim.mtime = meAzim.mtime;
                    feAzimNullInds[FeAzim.MTIME] = meAzimNullInds[MeAzim.MTIME];
                }

                if ((nRet = DynFeAzim.FeUpdateAzim(aID1, feAzim, feAzimNullInds)) != Constant.SUCCESS)
                {
                    ValErrs.AddMess("MERGE 10 - could not update azimuth information. Reason: %d",
                                    pdfName, "W", nRet.ToString());
                }
            }
            DynFeAzim.FeCloseAzim(aID1);

            // read Azimuth information 
            if ((aID1 = DynFeAzim.FeSelectAzim(tableName, "", "location,call1")) < 0)
            {
                ValErrs.AddMess("MERGE 11 - Could not read azimuth information. Reason: %d",
                                pdfName, "W", aID1.ToString());
                return;
            }

            while ((DynFeAzim.FeFetchAzim(aID1, out feAzim, out feAzimNullInds) == Constant.SUCCESS) &&
                         (feAzim.cmd.Equals("N")))
            {
                // pull in above site 
                ValImport.FeFormWhereClause(out whereClause, feAzim.location, "", "");
                ValImport.FeImportSite(pdfName, whereClause, "N");

                // pull in antennas 
                whereClause = String.Format("location = '{0}' and call1 = '{1}' ",
                                feAzim.location, feAzim.call1);
                ValImport.FeImportAntenna(pdfName, whereClause, "N");
            }
            DynFeAzim.FeCloseAzim(aID1);

            //...Log2.v("\nValMerge.FeValMergeAzimRecords: Exit");
        }


    }
}

```
